using UnityEngine;
using System.Collections;

public class IGC_Command_cp : IGC_Command {
	
	public IGC_Command_cp(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "cp";
		this.usage = "usage: cp <file> <copy_location>";
		this.help_text = "the cp command copies a file to a new location.";
		this.description = "copy a file";
	}
	
	public override string command_function()
	{
		if(argv.Length != 3){return this.malformed_error+"\n"+this.usage;}
		
		IGC_FileSystem fs = virtualSystem.fileSystem;
		
		IGC_URL 
			target = fs.ParseURL(argv[1], issuer.cwd),
			copy = fs.ParseURL(argv[2], issuer.cwd);
		
		if(!fs.FileExists(target.fullpath)){return "cant copy "+target.fullpath+" because it doesn't exist";}
		if(!fs.FileExists(copy.dirpath)){return "cant copy "+target.filename+" to "+copy.dirname+" because that directory does not exist.";}
		if(fs.FileExists(copy.fullpath)){return copy.fullpath+" already exists";}
		
		IGC_File file = fs.GetFile (target.fullpath);
		IGC_File dir = fs.GetFile (copy.dirpath);
		
		if(!fs.CanAccessFile(file, issuer)){return "you do not have permission to copy "+target.fullpath;}
		if(!fs.CanAccessFile(dir, issuer)){return "you do not have permission to access "+copy.dirname;}
		

		fs.CopyFile (target, copy);

		if(virtualSystem.networkReady){
			fs.GetComponent<NetworkView>().RPC("CopyFileRPC", RPCMode.Others, target.fullpath, copy.fullpath);
		}

		return target.filename+" copied to "+copy.fullpath;
	}
}