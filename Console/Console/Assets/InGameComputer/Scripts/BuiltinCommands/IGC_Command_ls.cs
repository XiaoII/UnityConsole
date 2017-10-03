using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

public class IGC_Command_ls : IGC_Command {

	private IGC_FileSystem fs;

	public IGC_Command_ls(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "ls";
		this.usage = "usage: ls [-a|l] <directory>";
		this.help_text = "use this command to display the contents a folder. you must add the -a flag to display hidden files and folders (files and folders whose names begin with a period). the -l flag will list aditional information on the files in the specified directory.";
		this.description = "display the contents of a directory";
	}

	public override string command_function()
	{
		fs = virtualSystem.fileSystem;
		string targetPath = issuer.cwd;
		bool 
			showHidden = false,
			asList = false;

		if(argv[argv.Length-1][0] != '-' && argv.Length > 1){
			targetPath = argv[argv.Length-1];
		}

		foreach(var flag in this.flags){
			if(flag.Key.IndexOf("a") >= 0){showHidden = true;}
			if(flag.Key.IndexOf("l") >= 0){asList = true;}
		}

		return ls (fs.ParseURL(targetPath, issuer.cwd), showHidden, asList);
	}
	
	private string ls(IGC_URL url, bool showHidden, bool asList){
		IGC_File file = fs.GetFile (url.fullpath);

		if(file != null){
			if(!fs.CanAccessFile(file, issuer)){return "you do not have permission to view this directory";}

			if(file.isDir){
				return FormatLSString(fs.ListFiles(url, issuer.cwd, showHidden), asList);
			}else{
				return file.path + " is not a directory";
			}
		}
		return url.fullpath + " does not exist";
	}

	private string FormatLSString(IGC_URL[] urls, bool asList){
		if(urls.Length == 0){return "";}

		string[] list = new string[urls.Length];

		for(int i=0; i<urls.Length; i++){
			if(asList){
				IGC_File file = fs.GetFile(urls[i].fullpath);
				string uniformLengthOwnerName = file.owner.name.PadRight(9, ' ').Substring(0,9);

				list[i] = (file.protectedFile ? "y" : "n") + " - " + uniformLengthOwnerName + " - " + file.accessGroups.Count+"/"+file.editGroups.Count + " - "+(file.isDir ? "d" : "f")+" - " + file.name;
			}else{
				list[i] = urls[i].filename;
			}
		}

		string columns = urls.Length+" items\nprotected | owner | groups r/w | type | name\n";

		return (asList ? columns : "") + string.Join((asList ? "\n" : " "), list);
	}
}
