using UnityEngine;
using System.Collections;

public class IGC_Command_pwd : IGC_Command {

	public IGC_Command_pwd(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "pwd";
		this.help_text = "print the name of the folder you are currently in";
		this.description = "print the current working directory";
	}
	
	public override string command_function()
	{
		return issuer.cwd;
	}
}
