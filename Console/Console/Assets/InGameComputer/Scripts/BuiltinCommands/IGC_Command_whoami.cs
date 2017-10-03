using UnityEngine;
using System.Collections;

public class IGC_Command_whoami : IGC_Command {

	public IGC_Command_whoami(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "whoami";
		this.help_text = "prints the name of the current user";
		this.description = this.help_text;
	}

	public override string command_function ()
	{
		return issuer.terminal.currentUser.name;
	}

}
