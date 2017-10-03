using UnityEngine;
using System.Collections;

public class IGC_Command_cat : IGC_Command {

	public IGC_Command_cat(ref IGC_VirtualSystem virtualSystem){
		this.virtualSystem = virtualSystem;
		this.name = "cat";
		this.usage = "usage: cat <file_name>";
		this.help_text = "print the specified file";
		this.description = this.help_text;
	}

	public override string command_function ()
	{
		IGC_FileSystem fs = virtualSystem.fileSystem;

		if(argv.Length > 1){
			IGC_URL url = fs.ParseURL(argv[1], issuer.cwd);

			IGC_File file = fs.GetFile(url.fullpath);

			if(file != null){
				if(!fs.CanAccessFile(file, issuer)){return "You do not have permission to view this file";}

				return file.data;
			}else{
				return "file "+url.fullpath+" does not exist";
			}
		}else{
			return malformed_error + "\n" + this.usage;
		}
	}
}
