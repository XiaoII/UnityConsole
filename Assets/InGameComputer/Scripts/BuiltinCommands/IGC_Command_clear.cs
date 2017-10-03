using UnityEngine;
using System.Collections;

public class IGC_Command_clear : IGC_Command {

	public IGC_Command_clear(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "clear";
		this.help_text = "use this command to erase all text on the screen.";
		this.description = "clear the screen";
	}
	
	public override string command_function ()
	{
		if(issuer == null){return "";}

		IGC_Shell shell = issuer.terminal.shell;
		shell.rawDisplayText = "";
		shell.rawPromptText = "";
		shell.prompt.text = "";
		shell.output.text = "";
		shell.cursorOffset = 0;
		shell.cursorOffsetVertical = 0;
		
		if(issuer.terminal.networkReady){
			shell.GetComponent<NetworkView>().RPC (
				"UpdateScreenRPC", 
				RPCMode.Others, 
				shell.prompt.text, 
				shell.output.text, 
				shell.cursorOffset, 
				shell.cursorOffsetVertical, 
				shell.lineOffset, 
				shell.inputMode
			);
		}
		return "";
	}
}
