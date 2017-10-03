using UnityEngine;
using System.Collections;

public class IGC_Command_who : IGC_Command {

	public IGC_Command_who(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "who";
		this.help_text = "the who command lists all users currently logged in on this machine";
		this.description = "list logged in users";
	}
	
	public override string command_function ()
	{
		var users = virtualSystem.userRegistry.loggedInUsers;
		string [] output = new string[users.Count];
		int i = 0;

		foreach (IGC_User user in users) {
			output[i++] = user.name;
		}

		return string.Join (", ", output);
	}
}
