using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class IGC_User {

	public string name;
	public bool isAdmin { get {return admin;}}
	public bool loggedInRemotely = false, admin = false;
	public List<IGC_UserGroup> groups = new List<IGC_UserGroup>();
	public InGameComputer terminal;
	public string cwd, homedir, password;
	public bool canLogin = true;
	public IGC_VirtualSystem virtualSystem;
	

	public IGC_User(string name, string password, bool admin, IGC_VirtualSystem virtualSystem, bool canLogin){
		this.name = name;
		SetPassword(password);
		this.admin = admin;
		this.virtualSystem = virtualSystem;
		this.canLogin = canLogin;
		cwd = "/home/" + name;
		homedir = cwd;
	}

	public void SetPassword(string pass){
		this.password = IGC_Utils.Md5Sum (pass);
	}

	public bool CheckPassword(string pass){
		return IGC_Utils.Md5Sum(pass) == password;
	}

	public void Admin(bool tf){
		admin = tf;

		if(virtualSystem.networkReady){
			virtualSystem.GetComponent<NetworkView>().RPC("UserAdminStatusRPC", RPCMode.Others, this.name, tf);
		}
	}
}
