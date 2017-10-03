using UnityEngine;
using System.Collections;

public class IGC_Command_help : IGC_Command {

	public IGC_Command_help(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "help";
		this.help_text = "type help to see basic system usage info.";
		this.description = "basic system usage information";
	}

	public override string command_function()
	{
		if(issuer == null){
			return "use the login command to begin.\nonce logged in, use the help command again for detailed system usage info.";
		}

		IGC_Shell shell = issuer.terminal.shell;
		IGC_Language lang = virtualSystem.language;

		string output = "";

		output += "Welcome to the "+virtualSystem.modelName+" Workstation!\n";

		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}// horizontal bar
		output += "general use:\n";
		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}

		output += "type in commands and hit return to execute them. use the up and down arrows to cycle through previously typed commands. hit the tab key to complete a partially typed command or list all available commands if the prompt line is blank.\n";

		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}// horizontal bar
		output += "usage notation:\n";
		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}

		output += "[]          optional item\n";
		output += "<>          variable required item\n";
		output += "|           or\n";

		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}// horizontal bar
		output += "key actions:\n";
		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}

		output += "___ Command Line Mode ___\n";
		output += "return      execute command\n";
		output += "up          cycle back in command history\n";
		output += "down        cycle forward in command history\n";
		output += "right       move cursor right one character\n";
		output += "left        move cursor left one character\n";
		output += "tab         auto complete command or list all\n";
		output += "ctrl+C      clear prompt\n";
		output += "ctrl+L      clear screen\n";
		output += "ctrl+A      jump to beginning of promt\n";
		output += "ctrl+E      jump to end of promt\n";
		output += "___ Text Edit Mode ___\n";
		output += "up          move cursor up one line\n";
		output += "down        move cursor down one line\n";
		output += "right       move cursor right one character\n";
		output += "left        move cursor left one character\n";
		output += "escape      exit edit mode\n";
		output += "___ Text View Mode ___\n";
		output += "up          move text up one line\n";
		output += "down        move text down one line\n";
		output += "q|escape    exit view mode\n";

		for(int i=0; i<shell.displayXY[0]; i++){output += "=";} 
		output += "available commands:\n";
		for(int i=0; i<shell.displayXY[0]; i++){output += "=";}
		output += "\n";

		foreach(IGC_Command cmd in lang.commands.Values){
			string 
				padding = "            ",
				cmd_info = cmd.name+(padding.Remove(0, cmd.name.Length))+cmd.description;

			int pointer = shell.displayXY[0];
			while(pointer < cmd_info.Length){
				cmd_info = cmd_info.Insert(pointer, "\n"+padding);
				pointer += shell.displayXY[0];
			}

			output += cmd_info+"\n";
		}

		issuer.terminal.shell.EnterViewMode (output);
		return "";
	}
}
