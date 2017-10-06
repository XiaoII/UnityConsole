using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(IGC_UserRegistry))]
[RequireComponent(typeof(IGC_FileSystem))]

public class IGC_VirtualSystem : MonoBehaviour {

	public static string uniqueID = string.Empty;

	public bool eraseSaveDataOnStart = false;
	public string 
		modelName = "CRL 5000",
		IP;

public string department = "Department";

	[HideInInspector] public InGameComputer terminal;
	[HideInInspector] public IGC_Language language;
	[HideInInspector] public IGC_FileSystem fileSystem;
	[HideInInspector] public IGC_UserRegistry userRegistry;
	[HideInInspector] public int instanceID;
	[HideInInspector] public bool networkReady { get { return this.GetComponent<NetworkView>() != null && GetComponent<NetworkView>().enabled && this.startUpTasksComplete && Network.peerType != NetworkPeerType.Disconnected; } }
	[HideInInspector] public string restoreData;
	[HideInInspector] public bool 
		startUpTasksComplete = false, 
		hasPermanentIP = false;

	private static List<IGC_VirtualSystem> instances = new List<IGC_VirtualSystem>(); 
	private static List<string> IPs = new List<string>();
	private static int IDCounter = 0;


	void Awake()
	{
		GenerateUniqueID();

		if(IP == ""){
			GenerateIP();
		}else{
			hasPermanentIP = true;
		}

		if(eraseSaveDataOnStart || !hasPermanentIP)
		{
			EraseSaveData();
			gameObject.GetComponent<InGameComputer>().EraseSaveData();
		}

		terminal = gameObject.GetComponent<InGameComputer> ();
		userRegistry = gameObject.GetComponent<IGC_UserRegistry> ();
		fileSystem = gameObject.GetComponent<IGC_FileSystem> ();
	}

	void Start()
	{
		//if you don't manually add a language extension to the computer object it will load the base langauge
		language = gameObject.GetComponent<IGC_Language> ();
		if(language == null){language = gameObject.AddComponent<IGC_Language>();}

		//if there's a networkview and no multiplayer connection, 
		if(Network.peerType == NetworkPeerType.Disconnected && GetComponent<NetworkView>() != null && !InGameComputer.waitForNetwork){ 
			GetComponent<NetworkView>().enabled = false;
			//Destroy(networkView);
		}


        //make random for better immersion
        if (department == "Pl")//Player
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "0" + idNo  + " " + IP;
        }
        if (department == "IT")//IT
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "1" + idNo + " " + IP;
        }
        if (department == "HR")//Human Res
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "2" + idNo +  " " + IP;
        }
        if (department == "FN")//Finance
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "3" + idNo + " " + IP;
        }
        if (department == "SL")//Sales
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "4" + idNo + " " + IP;
        }
        if (department == "DV")//Development
        {
            System.Random rnd = new System.Random();
            int idNo = rnd.Next(100, 1000);
            gameObject.name = department + "5" + idNo + " " + IP;
        }


        //start the chain of init events
        if (Network.peerType == NetworkPeerType.Disconnected && InGameComputer.waitForNetwork){ //if you instructed the comps to wait for the network
			StartCoroutine("InitializeComputerSystem");
			return;
		}

		fileSystem.Initialize();// the if above does not return start the chain
	}

	private IEnumerator InitializeComputerSystem()
	{
		//wait until 
		while(true)
		{
			if(Network.peerType != NetworkPeerType.Disconnected){break;}
			yield return null;
		}

		InGameComputer.waitForNetwork = false;
		fileSystem.Initialize();
	}

	public void OnFilesReady()
	{
		userRegistry.BuildUsersList();
	}

	public void OnUsersReady()
	{
		
	}

	public void OnGroupsReady()
	{
		fileSystem.ApplyFileGroups();
	}
	
	public void OnSystemReady()
	{
		startUpTasksComplete = true;

		//users file
		IGC_File etcUsers = fileSystem.GetFile("/etc/users");
		if(etcUsers == null){etcUsers = fileSystem.CreateFile(fileSystem.ParseURL("/etc/users", "/"), userRegistry.rootUser, false);}
		etcUsers.data = GetUsersString(SystemStateString());

		//groups file
		IGC_File etcGroups = fileSystem.GetFile("/etc/groups");
		if(etcGroups == null){etcGroups = fileSystem.CreateFile(fileSystem.ParseURL("/etc/groups", "/"), userRegistry.rootUser, false);}
		etcGroups.data = GetGroupsString(SystemStateString());

		gameObject.GetComponent<InGameComputer>().OnVirtualSystemReady();
	}

	void OnDestroy()
	{
		if(hasPermanentIP && Network.peerType != NetworkPeerType.Client){
			SaveState();
		} 
		//Debug.Log ("virtual system save string:\n"+SystemStateKey()+"\n"+SystemStateString());

		instances = new List<IGC_VirtualSystem>();
		IDCounter = 0;
	}

	private void GenerateIP ()
	{
		if(GetComponent<NetworkView>() == null || Network.peerType != NetworkPeerType.Client){
			string[] iplist = new string[4];
			string ip;
            string[] networks = new string[3];
            networks[0] = "192";
            networks[1] = "10";
            networks[2] = "172";
            



            while (true && this.IP == string.Empty){
				for(int i=0; i<4; i++){
					iplist[i] = Random.Range(0,255).ToString();
				}

				ip = string.Join (".", iplist);

				if(!IPs.Contains(ip)){
					this.IP = ip;
					IPs.Add(ip);
					break;
				}
			}

			//gameObject.name = department + " " + IP;
		}
	}

	public static IGC_VirtualSystem GetInstanceByID(int ID)
	{
		foreach(IGC_VirtualSystem instance in instances){
			if(instance.instanceID == ID){
				return instance;
			}
		}
		return null;
	}

	//assign id and add to instance list
	private void GenerateUniqueID()
	{
		if(IGC_VirtualSystem.uniqueID == string.Empty){
			string 
				seed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
				idString = string.Empty;

			for(int i=0; i<32; i++){
				idString += seed[Random.Range(0, seed.Length)];
			}

			IGC_VirtualSystem.uniqueID = idString;
		}

		this.instanceID = IDCounter++;
		instances.Add(this);
	}

	public void StartShutdownTimer ()
	{
		StartCoroutine("ShutdownTimer");
	}

	public void ForceShutdown(IGC_User admin)
	{
		MessageUsers("system shut down ordered by admin: "+admin.name);
		StartShutdownTimer();
	}

	public void MessageUser(IGC_User user, string message)
	{
		if(userRegistry.loggedInUsers.Contains(user) ) {
			user.terminal.shell.stdout(message);
		}
	}

	public void MessageUsers(string message)
	{
		foreach (IGC_User user in userRegistry.loggedInUsers) {
			user.terminal.shell.stdout(message);
		}
	}

	IEnumerator ShutdownTimer()
	{
		int i = 6;
		while(--i > 0){
			yield return new WaitForSeconds(1f);
			foreach (IGC_User user in userRegistry.loggedInUsers) {
				user.terminal.shell.stdout("shutting down in "+i);
			}
		}
		userRegistry.BootUsers ();
		terminal.PowerOff ();
	}


/* ================================================================
 * 			saving state, restoring state
 * 
 * ================================================================
 */


	public bool HasSaveData()
	{
		if( PlayerPrefs.GetString (SystemStateKey ()) != ""){
			return true;
		}
		return false;
	}

	public void SaveState()
	{
		PlayerPrefs.SetString (SystemStateKey (), SystemStateString ());
	}

	public void EraseSaveData()
	{
		PlayerPrefs.DeleteKey (SystemStateKey());
	}

//unique key for system save in playerprefs
	public string SystemStateKey()
	{
		string saveKey = Application.loadedLevel
			+ (Network.peerType == NetworkPeerType.Disconnected ? "offline" : "online")
			+ "IGC_VirtualSystem"
			+ this.IP;

		return saveKey;
	}

//get save string from playerprefs
	public string GetSaveString()
	{	
		return PlayerPrefs.GetString (SystemStateKey());
	}

	/*public void RestoreState(string target)
	{
		Debug.Log(SystemStateKey());
		Debug.Log(GetSaveString());
		Debug.Log(GetUsersString(target));
		Debug.Log(GetGroupsString(target));
		Debug.Log(GetFilesString(target));
	}*/

//get one chunk of save string OR current state string
	public string GetUsersString(string target)
	{
		string[] output = IGC_Utils.SplitString("\n\n", target);
		if(output.Length > 0){
			return output[0]; 
		}
		return "";
	}

	public string GetGroupsString(string target)
	{
		string[] output = IGC_Utils.SplitString("\n\n", target);
		if(output.Length > 1){
			return output[1]; 
		}
		return "";
	}

	public string GetFilesString(string target)
	{
		string[] output = IGC_Utils.SplitString("\n\n", target);
		if(output.Length > 2){
			return output[2]; 
		}
		return "";
	}


// current state of virtualSystem
	public string SystemStateString()
	{
		string saveString = "";
		
		// save users
		if(userRegistry.users.Values.Count == 0){saveString += "NONE\n";}
		
		foreach(IGC_User user in userRegistry.users.Values){

			int terminalID = user.terminal == null 
				? -1 //if it's not remotely logged in--if the user's term is the local one, this val is -1
				: user.terminal.instanceID;

			saveString += user.name
				+":"+ user.password
				+":"+ user.isAdmin.ToString() 
				+":"+ user.loggedInRemotely.ToString()
				+":"+ terminalID
				+":"+ user.cwd
				+":"+ userRegistry.loggedInUsers.Contains(user).ToString(); //user logged in?
			
			saveString += "\n";
		}
		
		saveString += "\n";
		
		// save groups
		if(userRegistry.groups.Values.Count == 0){saveString += "NONE\n";}
		
		foreach(IGC_UserGroup group in userRegistry.groups.Values){
			
			saveString += group.name
				+":"+ group.creator
					+":";
			
			foreach(IGC_User user in group.users){
				saveString += user.name+"~";
			}
			
			saveString += ":";
			
			foreach(IGC_User admin in group.admins){
				saveString += admin.name+"~";
			}
			
			saveString += "\n";
		}
		
		saveString += "\n";
		
		// save filesystem
		if(fileSystem.files.Values.Count == 0){saveString += "NONE";}
		
		foreach(IGC_File file in fileSystem.files.Values){

			saveString += file.path
				+":"+ file.fileOwner
					+":"+ file.protectedFile.ToString()
					+":"+ file.isDir.ToString()
					+":"+ file.ext
					+":"+ file.type
					+":"+ (file.data == "" ? "NONE" : IGC_Utils.EscapeForSave(file.data) )
					+":";
			
			foreach(IGC_UserGroup group in file.accessGroups){
				saveString += group.name+"~";
			}
			if(file.accessGroups.Count == 0){
				saveString += "NONE";
			}
			saveString += ":";


			foreach(IGC_UserGroup editgroup in file.editGroups){
				saveString += editgroup.name+"~";
			}
			if(file.editGroups.Count == 0){
				saveString += "NONE";
			}

			
			saveString += "\n";
		}
		
		return saveString;
	}

/* ================================================================
 * 			RPCs
 * 
 * ================================================================
 */


	//get signal from client gameinstance, fire this rpc (see userregistry rpc call)
	[RPC] void RequestStateFromServer(string gameInstanceID)
	{
		GetComponent<NetworkView>().RPC("ReturnStateRequest", RPCMode.Others, gameInstanceID, this.IP, SystemStateString());
	}
	
	//get signal on all instances but only act if id is the same as the original sender
	[RPC] void ReturnStateRequest(string gameInstanceID, string ip, string stateString)
	{
		if(gameInstanceID == IGC_VirtualSystem.uniqueID){
			restoreData = stateString;
			this.IP = ip;
			this.restoreData = stateString;
			gameObject.name = "Computer_" + IP;

			fileSystem.BuildFilesFromFileString();
		}
	}
}