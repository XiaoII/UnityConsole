using UnityEngine;
using System.Collections;

public class IGC_Command_logout : IGC_Command {

	public IGC_Command_logout(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "logout";
		this.help_text = "use this command to log out the current user. if you are connected to another machine remotely, you will log out as the remote user and resume your session as the last logged user on the local system.";
		this.description = "logout the current user";
	}
	
	public override string command_function()
	{
		virtualSystem.userRegistry.Logout(issuer);

		return "";
	}
}
