using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InGameComputer : MonoBehaviour {

	public static bool waitForNetwork = false;

	public string 
		loginName = "username",
		loginPass = "password";
	public bool startInOnState = true; //public but only affects actual power at Awake()
	public bool powerState { get { return power; } } //public but read only
	public bool inUse = false;
	public Color 
		screenColor = Color.blue, 
		textColor = Color.green;
	[HideInInspector] public IGC_Shell shell; 
	[HideInInspector] public IGC_VirtualSystem virtualSystem;
	[HideInInspector] public TextMesh output, prompt, caret, infoLine;
	[HideInInspector] public Transform screen, cursor;
	[HideInInspector] public InGameComputer localhost, remotehost;
	[HideInInspector] public IGC_User currentUser, previousUser;
	[HideInInspector] public bool noUser { get { return currentUser == null; } }
	[HideInInspector] public int instanceID;
	[HideInInspector] public bool loggedInRemotely = false, terminalOccupied = false, bootingUp = false;
	[HideInInspector] public bool networkReady { 
		get { 
			return (
				this.GetComponent<NetworkView>() != null &&
				GetComponent<NetworkView>().enabled &&
				virtualSystem.startUpTasksComplete &&
				Network.peerType != NetworkPeerType.Disconnected
			); 
		} 
	}

	//unique identifiers for RPC communication
	private static int IDCounter=0;
	private static List<InGameComputer> instances = new List<InGameComputer>();
	private bool power = true;//can't let peeps set this externally :p

	void Awake()
	{
		GenerateUniqueID(this);
	}

	void Start()
	{
		//if there's a networkview and no multiplayer connection, 
		if(Network.peerType == NetworkPeerType.Disconnected && GetComponent<NetworkView>() != null && !InGameComputer.waitForNetwork){
			GetComponent<NetworkView>().enabled = false;
			//Destroy(networkView);
		}

		if(virtualSystem == null){
			virtualSystem = gameObject.GetComponent<IGC_VirtualSystem>();
		}

		cursor = transform.Find ("monitor/display/cursor_holder/cursor");
		output = transform.Find ("monitor/display/output").GetComponent<TextMesh>();
		prompt = transform.Find ("monitor/display/prompt").GetComponent<TextMesh>();
		caret = transform.Find ("monitor/display/caret").GetComponent<TextMesh>();
		infoLine = transform.Find ("monitor/display/info_line").GetComponent<TextMesh>();
		screen = transform.Find ("monitor/screenPivot/screen");

		shell = gameObject.GetComponent<IGC_Shell>();

		localhost = this;
	}

	public void OnVirtualSystemReady()
	{
		if(Network.peerType != NetworkPeerType.Client && HasSaveData()){
			LoadSavedState(GetSaveString());
		}

		if(networkReady && Network.peerType == NetworkPeerType.Client){
			GetComponent<NetworkView>().RPC("RequestTerminalStateFromServer", RPCMode.Server, IGC_VirtualSystem.uniqueID);
		}

		if(Network.peerType != NetworkPeerType.Client){ //if client, all state data sent from server
			if(HasSaveData()){

			}else{
				if(!startInOnState){
					PowerOff();
				}else{
					if(loginName != string.Empty){ 
						if(virtualSystem.userRegistry.GetUser(loginName) == null){
							Debug.LogWarning(loginName + " is not a valid username. change the loginName field.");
						}else{
							virtualSystem.userRegistry.Login(loginName, loginPass, this);
							UpdateInfoLine();
						}
					}

					SetScreenColor (screenColor);
					SetTextColor (textColor);
				}
			}
		}


	}


	void OnDestroy()
	{
		if(Network.peerType != NetworkPeerType.Client){
			SaveState();
		}
		//Debug.Log ("terminal save string:\n-\n"+TerminalStateKey()+"\n-\n"+TerminalStateString());

		instances = new List<InGameComputer>();
		IDCounter = 0;
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) 
	{
		Application.LoadLevel ("start");
	}
	
	public static InGameComputer GetInstanceByID(int ID)
	{
		foreach(InGameComputer instance in instances){
			if(instance.instanceID == ID){
				return instance;
			}
		}
		return null;
	}

	//assign id and add to instance list
	private void GenerateUniqueID(InGameComputer instance)
	{
		instance.instanceID = IDCounter++;
		instances.Add(instance); 
	}

	public void UseComputer()
	{
		if (!terminalOccupied && power && !bootingUp) {
			inUse = true;

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC ("TerminalOccupiedRPC", RPCMode.Others, true);
			}
		}
	}

	public void LeaveComputer()
	{
		inUse = false;

		if(virtualSystem.networkReady){
			GetComponent<NetworkView>().RPC ("TerminalOccupiedRPC", RPCMode.Others, false);
		}
	}

	public void PowerOn()
	{
		PowerActions (true);

		if(virtualSystem.networkReady){
			GetComponent<NetworkView>().RPC ("PowerRPC", RPCMode.Others, true);
		}

		StartCoroutine ("StartUpAnimation");
	}

	public void PowerOff() 
	{

		PowerActions (false);

		if(virtualSystem.networkReady){
			GetComponent<NetworkView>().RPC ("PowerRPC", RPCMode.Others, false);
		}

		StopCoroutine("StartUpAnimation");
		StopCoroutine("StartUpAnimationProgressBar");
	}

	private void PowerActions(bool on){
		if(on){
			power = true;
			MonitorOn ();
		}else{
			virtualSystem.userRegistry.Logout(currentUser);
			power = false;
			MonitorOff ();
			shell.Reset ();
			if(this.currentUser != null){
				virtualSystem.userRegistry.Logout (this.currentUser); 
			}
			inUse = false;
		}
	}

	private void MonitorOff(){MonitorState (false);}

	private void MonitorOn(){MonitorState (true);}

	private void MonitorState(bool onoff)
	{
		infoLine.GetComponent<Renderer>().enabled = onoff;
		prompt.GetComponent<Renderer>().enabled = onoff;
		caret.GetComponent<Renderer>().enabled = onoff;
		output.GetComponent<Renderer>().enabled = onoff;
		SetScreenColor (onoff ? screenColor : Color.black);
	}

	public void SetScreenColor(Color c)
	{
		screen.GetComponent<Renderer>().material.color = c;
	}

	public void SetTextColor(Color c)
	{
		cursor.GetComponent<Renderer>().material.color = c;
		infoLine.GetComponent<Renderer>().material.color = c;
		caret.GetComponent<Renderer>().material.color = c;
		prompt.GetComponent<Renderer>().material.color = c;
		output.GetComponent<Renderer>().material.color = c;
	}

	public void SwapVirtualSystem(ref IGC_VirtualSystem target)
	{
		this.virtualSystem = target;
		this.shell.lang = virtualSystem.language;

		if (networkReady) {
			GetComponent<NetworkView>().RPC ("SwapVSRPC", RPCMode.Others, target.instanceID);
		}
	}

	public void UpdateInfoLine()
	{

		if(currentUser == null || virtualSystem == null){
			Debug.LogWarning("you're trying to update the infoline with invalid data");
			return;
		}

		string text = currentUser.name + " @ " + virtualSystem.IP;

		infoLine.text = currentUser == null 
			? "" 
			: text.Substring(0, (text.Length < shell.displayWidth ? text.Length : shell.displayWidth));
	}

	public void SetPrevUser(IGC_User user)
	{
		this.previousUser = user;
		if(networkReady){
			GetComponent<NetworkView>().RPC("SyncPrevUserRPC", RPCMode.Others, user.name);
		}
	}

	private IEnumerator StartUpAnimation()
	{
		bootingUp = true;
		while(true){
			infoLine.text = "";
			StartCoroutine("StartUpAnimationProgressBar");
			shell.output.text = ("loading operating system.");
			yield return new WaitForSeconds(.5f);
			shell.output.text = ("loading operating system..");
			yield return new WaitForSeconds(.5f);
			shell.output.text = ("loading operating system...");
			yield return new WaitForSeconds(.5f);
			shell.output.text = ("loading operating system....");
			yield return new WaitForSeconds(1.5f);
			shell.stdout ("OS: loaded");
			yield return new WaitForSeconds(.9f);
			shell.stdout ("UserRegistry: loaded");
			yield return new WaitForSeconds(2.1f);
			shell.stdout ("FileSystem: loaded");
			yield return new WaitForSeconds(1.5f);
			shell.stdout ("GroupAssociations: loaded");
			yield return new WaitForSeconds(3f);
			StopCoroutine("StartUpAnimationProgressBar");
			infoLine.text = "";
			shell.output.text = ("Model: "+this.virtualSystem.modelName+"\nLogin to begin.\ntype \"help\" for help.");
			shell.UpdateScreenNetwork();
			break;
		}
		bootingUp = false;

		if (networkReady) {
			GetComponent<NetworkView>().RPC("LoadAnimationCompleteRPC", RPCMode.Others);
		}
	}

	private IEnumerator StartUpAnimationProgressBar()
	{
		while(true){
			infoLine.text += "*";
			shell.UpdateScreenNetwork();
			yield return new WaitForSeconds(Random.Range(0.3f, 0.6f));
		}
	}

/* ================================================================
 * 			saving state, restoring state
 * 
 * ================================================================
 */

	public bool HasSaveData()
	{
		if( GetSaveString() != ""){
			return true;
		}
		return false;
	}

	public void EraseSaveData()
	{
		PlayerPrefs.DeleteKey (TerminalStateKey());
	}

	public void SaveState()
	{
		PlayerPrefs.SetString (TerminalStateKey (), TerminalStateString ());
	}
	
	//unique key for system save in playerprefs
	public string TerminalStateKey()
	{
		string saveKey = Application.loadedLevel
			+ (Network.peerType == NetworkPeerType.Disconnected ? "offline" : "online")
			+ "InGameComputer"
			+ this.instanceID;
		
		return saveKey;
	}

	public string TerminalStateString()
	{
		string saveString = (currentUser != null ? currentUser.name : "null")
			+":"+ (previousUser != null ? previousUser.name : "null")
			+":"+ powerState.ToString()
			+":"+ virtualSystem.instanceID.ToString()
			+":"+ (!string.IsNullOrEmpty(shell.rawPromptText) ? IGC_Utils.EscapeForSave(shell.rawPromptText) : "null")
			+":"+ (!string.IsNullOrEmpty(shell.rawDisplayText) ? IGC_Utils.EscapeForSave(shell.rawDisplayText) : "null")
			+":"+ IGC_Utils.ColorString(screenColor)
			+":"+ IGC_Utils.ColorString(textColor);

		//Debug.Log (instanceID+":"+virtualSystem.IP);

		return saveString;
	}
	
	//get save string from playerprefs
	public string GetSaveString()
	{	
		return PlayerPrefs.GetString (TerminalStateKey());
	}

	private void LoadSavedState(string stateString)
	{
		string[] termInfo = IGC_Utils.SplitString(":", stateString);
		int vsID = System.Int32.Parse(termInfo[3]);
		bool powerstate = (termInfo[2] == "True" ? true : false);
		string 
			currentUsername = termInfo[0],
			prevUsername = termInfo[1],
			rawPrompt = (termInfo[4] != "null" ? IGC_Utils.UnescapeSaved(termInfo[4]) : ""),
			rawDisplay = (termInfo[5] != "null" ? IGC_Utils.UnescapeSaved(termInfo[5]) : "");

		screenColor = IGC_Utils.ColorFromString (termInfo [6]);
		textColor = IGC_Utils.ColorFromString (termInfo [7]);
		
		//set vs
		if(vsID != this.virtualSystem.instanceID){
			IGC_VirtualSystem tempRef = IGC_VirtualSystem.GetInstanceByID(vsID);
			SwapVirtualSystem(ref tempRef);
		}

		//since comp instances load one at a time, it's possible to swap a terminal's VS for one that's not setup yet.
		//therefore you have to wait for ready before setting current and prev users
		StartCoroutine(SetCurrenUserWhenURReady(virtualSystem.userRegistry, currentUsername, prevUsername));

		//set power state
		PowerActions(powerstate);

		SetTextColor (textColor);//only text here, screen is already set in MonitorActions

		shell.rawPromptText = rawPrompt;
		shell.rawDisplayText = rawDisplay;
	}

	private IEnumerator SetCurrenUserWhenURReady(IGC_UserRegistry ur, string currentuser, string prevuser)
	{
		while(!ur.ready){yield return null;}

		//set current user
		if(!string.IsNullOrEmpty(currentuser) && currentuser != "null"){
			currentUser = virtualSystem.userRegistry.users[currentuser];
			currentUser.terminal = this;
			UpdateInfoLine();
		}

		//set prev user
		if(!string.IsNullOrEmpty(prevuser) && prevuser != "null"){
			this.previousUser = currentUser.terminal.gameObject.GetComponent<IGC_VirtualSystem>().userRegistry.users[prevuser];
		}
	}


	[RPC] void SyncPrevUserRPC(string username)
	{
		//Debug.Log (username);
		previousUser = virtualSystem.userRegistry.users[username];
	}

	[RPC] void SwapVSRPC(int vsID)
	{
		IGC_VirtualSystem vs = IGC_VirtualSystem.GetInstanceByID (vsID);
		this.virtualSystem = vs;
		this.shell.lang = vs.language;
	}

	[RPC] void LoadAnimationCompleteRPC()
	{
		bootingUp = false;
	}

	[RPC] void TerminalOccupiedRPC(bool tf)
	{
		terminalOccupied = tf;
	}

	[RPC] void PowerRPC(bool on)
	{
		PowerActions (on);
	}

	//get signal from client gameinstance, fire this rpc (see userregistry rpc call)
	[RPC] void RequestTerminalStateFromServer(string gameInstanceID)
	{
		GetComponent<NetworkView>().RPC("ReturnTerminalStateRequest", RPCMode.Others, 
			gameInstanceID,
	        terminalOccupied,
	        bootingUp,
			TerminalStateString()
		); 

		shell.UpdateScreenNetwork();
	}

	//get signal on all instances but only act if id is the same as the original sender
	[RPC] void ReturnTerminalStateRequest(string gameInstanceID, bool terminaloccupied, bool bootingup, string termString)
	{
		//Debug.Log("i got the terminal return \n"+gameInstanceID+"\n"+IGC_VirtualSystem.uniqueID);
		
		if(gameInstanceID == IGC_VirtualSystem.uniqueID){
			//Debug.Log("it's for me, terminal");

			terminalOccupied = terminaloccupied;
			bootingUp = bootingup;

			LoadSavedState(termString);
		}
	}

	[RPC] public void SetColorsRPC(string s, string t)
	{
		SetScreenColor (IGC_Utils.ColorFromString(s));
		SetTextColor (IGC_Utils.ColorFromString(t));
	}
}
