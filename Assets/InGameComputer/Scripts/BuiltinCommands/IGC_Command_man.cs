using UnityEngine;
using System.Collections;

public class IGC_Command_man : IGC_Command {
	
	public IGC_Command_man(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "man";
		this.help_text = "type this command followed by any other command for more information on its use";
		this.description = "get manual entry for provided command";
	}
	
	public override string command_function()
	{
		string cmd = argv [argv.Length > 1 ? 1 : 0];
		IGC_Language lang = virtualSystem.language;
		
		if(!lang.HasCommand(cmd)){
			return "Command ["+cmd+"] not found";
		}

		if(lang.commands[cmd].help_text == string.Empty){
			return "Command ["+cmd+"] has no manual entry";
		}
		if(!lang.cmdSafeWhileLoggedOut(name)){
			return "";	
		}

		string output = "";
		if(lang.commands[cmd].usage != string.Empty){
			output += lang.commands[cmd].usage+"\n";
		}
		output += lang.commands [cmd].help_text;

		issuer.terminal.shell.EnterViewMode (output);
		return "";
	}
}
