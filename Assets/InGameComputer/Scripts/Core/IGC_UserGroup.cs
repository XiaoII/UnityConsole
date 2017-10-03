using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IGC_UserGroup {

	public string name, creator;
	public List<IGC_User> users = new List<IGC_User>();
	public List<IGC_User> admins = new List<IGC_User>();

	public IGC_UserGroup(string name, IGC_User groupCreator)
	{
		this.name = name;
		this.creator = groupCreator.name;
		AddUser (groupCreator, true);
	}

	public IGC_User AddUser(IGC_User user, bool asAdmin)
	{
		if(users.Contains(user)){
			return user;
		}
		this.users.Add (user);
		user.groups.Add (this);

		if(asAdmin){
			admins.Add(user);
		}

		if (user.virtualSystem.networkReady) {
			user.virtualSystem.gameObject.GetComponent<NetworkView>().RPC("UserAddGroupRPC", RPCMode.Others, this.name, user.name, asAdmin);
		}

		return user;
	}

	public bool RemoveUser(IGC_User user, IGC_User issuer)
	{
		if(!admins.Contains(issuer) && !issuer.isAdmin){return false;}

		if(!users.Contains(user)){return false;}
		
		user.groups.Remove (this);
		this.users.Remove (user);

		if(admins.Contains(user)){admins.Remove(user);}

		if (user.virtualSystem.networkReady) {
			user.virtualSystem.gameObject.GetComponent<NetworkView>().RPC("UserRMGroupRPC", RPCMode.Others, this.name, user.name);
		}

		return true;
	}
}