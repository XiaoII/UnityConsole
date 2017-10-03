using UnityEngine;
using System.Collections;

public class IGC_Command_touch : IGC_Command {

	public IGC_Command_touch(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "touch";
		this.usage = "usage: touch <file>";
		this.help_text = "use this command to create new files";
		this.description = "create new file";
	}
	
	public override string command_function()
	{
		if(argv.Length < 2){return malformed_error+"\n"+usage;}

		IGC_FileSystem fs = virtualSystem.fileSystem;
		IGC_URL url = fs.ParseURL(argv [1], issuer.cwd);

		if(!fs.FileExists(url.fullpath)){
			IGC_File file = fs.CreateFile(url, issuer, false);

			if(file != null){
				return "file "+file.path+" created successfully";
			}else{
				return "error: insufficient privilages or broken path";
			}
		}
		return url.fullpath+" already exists";
	}
}
