using UnityEngine;
using System.Collections;

public class IGC_Command_cd : IGC_Command {

	private IGC_FileSystem fs;

	public IGC_Command_cd(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "cd";
		this.usage = "usage: cd [directory]";
		this.help_text = "use this command to move into other folders. use an absolute path (example: /home/username ) or a relative path (./folder_name) (folder_name). use \"..\" to move upward to the parent directory of the one you are currently in. if you do not supply a path you will cd to your home directory.";
		this.description = "move to another directory";
	}
	
	public override string command_function()
	{
		fs = virtualSystem.fileSystem;

		if(argv.Length > 1){
			return cd (fs.ParseURL(argv[1], issuer.cwd));
		}else{
			return cd (fs.ParseURL(issuer.homedir, issuer.cwd));
		}
	}

	private string cd(IGC_URL url){
		IGC_File file = fs.GetFile (url.fullpath);
		
		if(file != null){
			if(file.isDir){
				if(!fs.CanAccessFile(file, issuer)){return "you do not have permission to enter this directory";}

				issuer.cwd = file.path;

				if(virtualSystem.networkReady){
					virtualSystem.GetComponent<NetworkView>().RPC("UpdateCWDRPC", RPCMode.Others, issuer.name, file.path);
				}

				return "";
			}else{
				return url.fullpath + " is not a directory";
			}
		}
		return url.fullpath + " does not exist";
	}
}
