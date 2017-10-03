using UnityEngine;
using System.Collections;

public class IGC_Command_edit : IGC_Command {

	public IGC_Command_edit(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "edit";
		this.usage = "usage: edit <file_name>";
		this.help_text = "the edit command opens a text file for editing.\nwhen you finish editing press escape to exit edit mode. you will then be prompted to run the save command. if you do not run save the file will remain unchanged. you may wait to save the file until you use the edit command again; when you do that, all changes are erased. see \"key actions\" section in the help command for more info.";
		this.description = "edit text files";
	}
	
	public override string command_function()
	{
		if(argv.Length != 2){return malformed_error+"\n"+usage;}

		IGC_FileSystem fs = virtualSystem.fileSystem;

		IGC_URL url = fs.ParseURL (argv [1], issuer.cwd);
		IGC_File file = fs.GetFile (url.fullpath);

		if(file == null){ return url.fullpath+" does not exits";}

		if(!fs.CanEditFile(file, issuer)){ return "you do not have permission to edit this file";}

		issuer.terminal.shell.EnterEditMode (file);
		return "";
	}
}