using UnityEngine;
using System.Collections;

public class IGC_Command_history : IGC_Command {

	public IGC_Command_history(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "history";
		this.help_text = "this commands displays previous phrases typed entered into the command line.";
		this.description = "display past commands";
	}

	public override string command_function ()
	{
		return " - "+string.Join("\n - ", issuer.terminal.shell.history.ToArray());
	}
}
