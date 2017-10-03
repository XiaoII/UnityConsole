using UnityEngine;
using System.Collections;

public class IGC_Command_save : IGC_Command {
	
	public IGC_Command_save(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "save";
		this.help_text = "the save command saves the edits made to the previously edited file. you may wait to save changes to a file until you use the edit program again. once you open any file, all previous edits are forgotten.";
		this.description = "save last edited file";
	}
	
	public override string command_function()
	{
		return issuer.terminal.shell.SaveLastEdited();
	}
}