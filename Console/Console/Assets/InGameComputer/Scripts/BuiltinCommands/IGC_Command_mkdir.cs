using UnityEngine;
using System.Collections;

public class IGC_Command_mkdir : IGC_Command {

	public IGC_Command_mkdir(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "mkdir";
		this.usage = "usage: mkdir <directory_path>";
		this.help_text = "use this command to create new folders";
		this.description = "create new directory";
	}
	
	public override string command_function()
	{
		if(argv.Length < 2){return malformed_error+"\n"+usage;}

		IGC_FileSystem fs = virtualSystem.fileSystem;
		IGC_URL url = fs.ParseURL (argv [1], issuer.cwd);

		if(!fs.CanAccessFile(fs.GetFile(url.dirpath), issuer)){return "you do not have permission to create files in that location";}

		if(!fs.FileExists(url.fullpath)){
			IGC_File dir = fs.CreateFile(url, issuer, true);
			if(dir != null){
				return "directory "+url.fullpath+" created successfully";
			}else{
				return "error: broken path";
			}
		}
		return url.fullpath+" already exists";
	}
}
