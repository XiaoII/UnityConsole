using UnityEngine;
using System.Collections;

public class IGC_Command_rm : IGC_Command {

	public IGC_Command_rm(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "rm";
		this.usage = "usage: rm <file | directory -r>";
		this.help_text = "use this command to PERMANENTLY delete files and folders. you must add the -r flag if you intend to delete a folder. deleting folders destroys all their content.";
		this.description = "delete files and directories";
	}
	
	public override string command_function ()
	{
		if(argv.Length < 2){return malformed_error+"\n"+usage;}

		IGC_FileSystem fs = virtualSystem.fileSystem;
		IGC_URL url = fs.ParseURL(argv [1], issuer.cwd);
		IGC_File file = fs.GetFile(url.fullpath);


		if(file != null){
			if(!fs.CanAccessFile(file, issuer)){return "you do not have permission to delete "+file.path;}

			if(file.isDir && argv.Length < 3){
				return "you must type '-r' after the folder name to delete a folder and all files/folders within";
			}

			if(!file.isDir || (file.isDir && argv[2] == "-r")){
				if(fs.RMFile(url, issuer)){
					return url.fullpath+" deleted";
				}else{
					return "system error. could not delete file...?";
				}
			}else{
				return malformed_error+usage;
			}
		}
		return url.fullpath+" does not exist";
	}
}
