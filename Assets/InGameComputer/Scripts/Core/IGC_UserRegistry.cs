using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IGC_UserRegistry : MonoBehaviour {

	public string
		defaultUsername = "defaultUser",
		defaultUserPassword = "password",
		rootUserPassword = "rootpass";
	public string[] 
		usersList,
		groupsList;

	public Dictionary<string, IGC_User> users = new Dictionary<string, IGC_User>();
	public Dictionary<string, IGC_UserGroup> groups = new Dictionary<string, IGC_UserGroup>();
	public IGC_User rootUser, defaultUser, systemUser;
	public List<IGC_User> loggedInUsers = new List<IGC_User>();

	[HideInInspector] public IGC_VirtualSystem virtualSystem;
	[HideInInspector] public bool ready = false;



	void Awake()
	{
		virtualSystem = gameObject.GetComponent<IGC_VirtualSystem>();
		systemUser = new IGC_User ("system", "generate a better one later", true, virtualSystem, false); //system user is needed even if everything else is loaded from a statestring
		systemUser.terminal = gameObject.GetComponent<InGameComputer> ();

		if(string.IsNullOrEmpty(rootUserPassword) || string.IsNullOrEmpty(defaultUserPassword)){//can't let the editor vals slide
			Debug.LogError("UserRegistry (virtualSystem:"+virtualSystem.IP+"): default rootuser and defaultuser password cannot be left empty in the editor");
		}
	}
	
	public void BuildUsersList()
	{
		if(Network.peerType == NetworkPeerType.Client || (Network.peerType != NetworkPeerType.Client && virtualSystem.HasSaveData())){
			BuildUsersFromUserString(virtualSystem.GetUsersString(virtualSystem.restoreData));
			virtualSystem.OnUsersReady();
			BuildGroupsFromGroupsString(virtualSystem.GetGroupsString(virtualSystem.restoreData));
			virtualSystem.OnGroupsReady(); 	
		}else{
			//create system default users
			defaultUser = AddUser(new IGC_User(defaultUsername, defaultUserPassword, false, virtualSystem, true), systemUser);
			rootUser = AddUser(new IGC_User("root", rootUserPassword, true, virtualSystem, true), systemUser);

			//encrypt editor specified user password
			for(int i=0; i<usersList.Length; i++){
				string [] userinfo = IGC_Utils.SplitString(":", usersList[i]);
				userinfo[1] = IGC_Utils.Md5Sum(userinfo[1]);

				usersList[i] = string.Join(":", userinfo);
			}

			BuildUsersFromUserString(string.Join("\n", usersList));
			BuildGroupsFromGroupsString(string.Join("\n", groupsList));
		}

		ready = true;
		virtualSystem.OnSystemReady ();
	}
	
	public void BuildUsersFromUserString(string usersString)
	{
		if(usersString == "NONE" || usersString == "" || usersString == null){return;}

		string[] userStringList = IGC_Utils.SplitString("\n",  usersString);

	//foreach user in usersting
		for(int i=0; i<userStringList.Length; i++){
			string[] user = IGC_Utils.SplitString(":",  userStringList[i]);

			int terminalID = user[4] == "-1" 
				? System.Int32.Parse(user[4])
				: gameObject.GetComponent<InGameComputer>().instanceID;

			string
				username = user[0],
				userCwd = user[5];
			bool 
				isAdmin = 			user[2] == "True" ? true : false,
				loggedIn = 			user[6] == "True" ? true : false,
				loggedRemotely = 	user[3] == "True" ? true : false;

			IGC_User newUser = AddUser(new IGC_User(username, "", isAdmin, virtualSystem, true), systemUser); 

			newUser.password = user[1]; //need to assign it directly since the user class encrypts whatever you hand to it
			newUser.loggedInRemotely = loggedRemotely;
			newUser.terminal = InGameComputer.GetInstanceByID(terminalID);
			newUser.cwd = userCwd;

			if(loggedIn){
				loggedInUsers.Add(newUser);
			}
		}
	}

	public void BuildGroupsFromGroupsString (string groupsString)
	{
		if(groupsString == "NONE" || groupsString == "" || groupsString == null){return;}

		string[] groupStringList = IGC_Utils.SplitString("\n",  groupsString);

	//foreach group in groupstring
		for(int i=0; i<groupStringList.Length; i++){
			string[] 
				group = IGC_Utils.SplitString(":",  groupStringList[i]),
				groupUsers = IGC_Utils.SplitString("~",  group[2]),
				groupAdmins = IGC_Utils.SplitString("~",  group[3]);

		//create group
			IGC_UserGroup newGroup = AddGroup(group[0], users[group[1]]);
		//add users 
			foreach(string user in groupUsers){
				newGroup.AddUser(users[user], false);
			}
		//add admins
			foreach(string admin in groupAdmins){
				newGroup.AddUser(users[admin], true);
			}
		}
	}

	public IGC_UserGroup GetGroup(string groupName)
	{
		if(groups.ContainsKey(groupName)){
			return groups[groupName];
		}
		return null;
	}

	public IGC_UserGroup AddGroup(string groupname, IGC_User owner)
	{
		groupname = IGC_Utils.EscapeUserOrGroup (groupname);

		if(!groups.ContainsKey(groupname)){
			groups.Add(groupname, new IGC_UserGroup(groupname, owner));

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("AddGroupRPC", RPCMode.Others, groupname, owner.name);
			}

			return groups[groupname];
		}
		return null;
	}
	
	public bool RemoveGroup(IGC_UserGroup group, IGC_User user)
	{
		//add perms stuff later
		if(groups.ContainsKey(group.name)){
			groups.Remove(group.name);

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("RemoveGroupRPC", RPCMode.Others, group.name);
			}

			return true;
		}
		return false;
	}

	public IGC_User GetUser(string username)
	{
		if(users.ContainsKey(username)){
			return users[username];
		}
		return null;
	}

	public IGC_User AddUser(IGC_User user, IGC_User requester)
	{
		user.name = IGC_Utils.EscapeUserOrGroup(user.name);

		if(!users.ContainsKey(user.name)){
			users.Add(user.name, user);

			CreateUserHomeDir(user); 

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("AddUserRPC", RPCMode.Others, user.name, user.password, user.isAdmin, virtualSystem.instanceID, user.canLogin);
			}

			return user;
		}

		return null;
	}

	private void CreateUserHomeDir(IGC_User user)
	{
		IGC_File homeDir = virtualSystem.fileSystem.CreateFile(virtualSystem.fileSystem.ParseURL(user.homedir, user.cwd), systemUser, true);
		
		if(homeDir == null){//if it exists, createfile will return null
			homeDir = virtualSystem.fileSystem.files["/home/"+user.name];
		}
		
		homeDir.fileOwner = user.name;
	}

	public bool RemoveUser(IGC_User user)
	{
		if(users.ContainsKey(user.name)){
			users.Remove(user.name);

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("RemoveUserRPC", RPCMode.Others, user.name);
			}

			return true;
		}
		return false;
	}

	public IGC_User Login(string username, string password, InGameComputer terminal)
	{ 
		if(users[username].CheckPassword(password)){
			LoginActions(username, terminal);

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("LoginRPC", RPCMode.Others, username, terminal.instanceID);
			}

			return users[username];
		}else{
			return null;
		}
	}

	public void LoginActions(string username, InGameComputer terminal)
	{
		loggedInUsers.Add(users[username]);
		users [username].loggedInRemotely = virtualSystem.instanceID != terminal.gameObject.GetComponent<IGC_VirtualSystem> ().instanceID;
		users[username].terminal = terminal;
		terminal.currentUser = users[username];
	}

	public bool Logout(IGC_User user)
	{
		if(loggedInUsers.Contains(user)){
			loggedInUsers.Remove(user);
			user.terminal.currentUser = null;
			user.terminal.shell.Reset();

			if(user.loggedInRemotely){	
				BounceToPreviousUser(user);
			}

			user.terminal.UpdateInfoLine();

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC("LogoutRPC", RPCMode.Others, user.name);
			}

			return true;
		}
		return false;
	}

	private void BounceToPreviousUser (IGC_User user)
	{
		IGC_VirtualSystem localVS = user.terminal.transform.GetComponent<IGC_VirtualSystem>();
		IGC_User prevUser = user.terminal.previousUser;
		user.loggedInRemotely = false;
		user.terminal.SwapVirtualSystem(ref localVS);
		//log in previous user on local terminal
		user.terminal.currentUser = prevUser;
		user.terminal.previousUser = null;
	}

	public string[] ListUsers()
	{
		string[] output = new string[users.Count];

		int i = 0;
		foreach(IGC_User user in users.Values){
			output[i++] = user.name; 
		}
		return output;
	}

	public string[] ListGroups()
	{
		string[] output = new string[groups.Count];
		
		int i = 0;
		foreach(IGC_UserGroup group in groups.Values){
			output[i++] = group.name; 
		}
		return output;
	}

	public void BootUsers()
	{
		while(loggedInUsers.Count > 0){
			Logout( loggedInUsers[0]);
		}
	}

	public void BootUser(IGC_User admin, IGC_User user)
	{
		user.terminal.shell.stdout("You have been booted off the system by: "+admin.name);
		Logout (user);
	}


	[RPC] private void RemoveGroupRPC (string groupname)
	{
		groups.Remove(groupname);
	}

	[RPC] private void AddGroupRPC (string groupname, string ownername)
	{
		groups.Add (groupname, new IGC_UserGroup(groupname, GetUser(ownername))); 
	}

	[RPC] private void LoginRPC (string username, int terminalID)
	{
		LoginActions(username, InGameComputer.GetInstanceByID(terminalID));
	}

	[RPC] private void LogoutRPC (string username)
	{
		IGC_User user = users[username];
		user.terminal.currentUser = null;
		user.terminal.shell.Reset();
		
		if(user.loggedInRemotely){	
			BounceToPreviousUser(user);
		}

		user.terminal.UpdateInfoLine();
		loggedInUsers.Remove(user);
	}


	[RPC] private void RemoveUserRPC (string username)
	{
		users.Remove(username);
	}

	[RPC] private void AddUserRPC (string username, string password, bool isAdmin, int vsID, bool canLogin)
	{
		IGC_User user = new IGC_User (username, password, isAdmin, IGC_VirtualSystem.GetInstanceByID (vsID), canLogin);
		users.Add(user.name, user);
		CreateUserHomeDir(user); 
	}

	[RPC] private void UserAddGroupRPC (string groupname, string username, bool asAdmin)
	{
		groups [groupname].users.Add (users [username]);
		if(asAdmin){
			groups[groupname].admins.Add(users[username]);
		}
		users [username].groups.Add (groups [groupname]);
	}

	[RPC] private void UserRMGroupRPC (string groupname, string username)
	{
		users[username].groups.Remove (groups [groupname]);
		groups [groupname].users.Remove(users [username]);
		if(groups[groupname].admins.Contains(users[username])){
			groups[groupname].admins.Remove(users[username]);
		}
	}

	[RPC] private void UserAdminStatusRPC (string username, bool status)
	{
		users [username].admin = status;
	}

	[RPC] void UpdateCWDRPC(string username, string cwd)
	{
		users [username].cwd = cwd;
	}

	[RPC] void SetPasswordRPC(string username, string passwd)
	{
		users [username].SetPassword(passwd);
	}
}
