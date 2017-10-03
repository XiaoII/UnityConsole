using UnityEngine;
using System.Collections;

public class IGC_Command_less : IGC_Command {
	
	public IGC_Command_less(ref IGC_VirtualSystem virtualSystem)
	{
		this.CanPipe = true;
		this.virtualSystem = virtualSystem;
		this.name = "less";
		this.usage = "usage: <command> | less";
		this.help_text = "use this command display long texts in view mode that would otherwise be cutoff by the screen. Type any command followed by a | character, then the less command to pipe the former command's output into view mode. \nexample:\nusers | less";
		this.description = "display text in view mode";
	}
	
	public override string command_function ()
	{
		issuer.terminal.shell.EnterViewMode (string.Join (" ", IGC_Utils.ArrayShift (argv)));
		return "";
	}
}
