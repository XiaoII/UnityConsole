using UnityEngine;
using System.Collections;

public class IGC_Command_file : IGC_Command {

	private IGC_UserRegistry registry;
	private IGC_FileSystem fs;
	private string keyword;


	public IGC_Command_file(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "file";
		this.usage = "usage:\n";
		this.usage += "get file info\nfile <filename>\n\n";
		this.usage += "edit file read groups\nfile -r <add|rm> <groupname> <filename>\n\n";
		this.usage += "edit file write groups\nfile -w <add|rm> <groupname> <filename>\n\n";
		this.usage += "protect/unprotect file\nfile -p <filename> <y|n>\n";
		this.help_text = "use the file command to get info on a specific file, protect/unprotect a file or add or remove an access group.";
		this.description = "file info, edit groups, (un)protect file";
	}
	
	public override string command_function()
	{
		registry = virtualSystem.userRegistry;
		fs = virtualSystem.fileSystem;
		
		if(argv.Length > 1){
			keyword = argv[1];
			switch(keyword){
				case "-w": return GroupActions();
				case "-r": return GroupActions();
				case "-p": return ProtectedYN();;
				default: return FileInfo();
			}
		}

		return malformed_error+"\nusage: file -<g|p> ...";
	}

	private string GroupActions(){
		if(argv.Length != 5){
			return malformed_error+"\nusage: file -r|w <add|rm> <groupname> <filename>";
		}

		string action = argv [2]; 

		if(action != "add" && action != "rm"){return "action "+argv[2]+" not understood";}

		IGC_UserGroup group = registry.GetGroup (argv [3]);
		if(group == null){return argv[3]+" does not exist";}

		IGC_URL url = fs.ParseURL (argv [4], issuer.cwd);
		IGC_File file = fs.GetFile (url.fullpath);
		if(file == null){return url.fullpath+" does not exist";}

		bool writeGroup = argv [1] == "-w";

		if(fs.CanAccessFile(file, issuer)){
			if(action == "add"){
				if(writeGroup){
					if(file.ApplyEditGroup(group) != null){
						return group.name+" added to "+file.path;
					}else{
						return file.path+" already belongs to "+group.name;
					}
				}else{
					if(file.ApplyAccesGroup(group) != null){
						return group.name+" added to "+file.path;
					}else{
						return file.path+" already belongs to "+group.name;
					}
				}
			}else if(action == "rm"){ //redundant, but more legible
				if(writeGroup){
					if(file.RemoveEditGroup(group)){
						return group.name+" removed from "+file.path;
					}else{
						return file.path+" does not belong to "+group.name;
					}
				}else{
					if(file.RemoveAccessGroup(group)){
						return group.name+" removed from "+file.path;
					}else{
						return file.path+" does not belong to "+group.name;
					}
				}
			}
		}

		return "you do not have permission to alter this file";
	}

	private string FileInfo(){
		if(argv.Length != 2){return malformed_error+"\nusage: file <filename>";}

		IGC_URL url = fs.ParseURL (argv [1], issuer.cwd);
		IGC_File file = fs.GetFile (url.fullpath);

		if(file != null){
			string output = "TYPE: "+file.type;
			output += "\nOWNER: "+file.owner.name;
			output += "\nPROTECTED: "+file.protectedFile;
			output += "\nREAD GROUPS: "+string.Join(", ", file.ListAccessGroups());
			output += "\nWRITE GROUPS: "+string.Join(", ", file.ListEditGroups());
			return output;
		}
		return "file "+url.fullpath+" does not exist";
	}

	private string ProtectedYN(){
		if(argv.Length != 4){return malformed_error+"\nusage: file -a <filename> <y|n>";}

		bool pro = argv [3] == "y" ? true : false;
		IGC_URL url = fs.ParseURL (argv [2], issuer.cwd);
		IGC_File file = fs.GetFile (url.fullpath);

		if(file == null){return url.fullpath+" does not exist";}

		if(fs.CanAccessFile(file, issuer)){
			file.Protect(pro);
			return url.fullpath+" is now "+ (pro ? "protected" : "unprotected");
		}

		return "you do not have permission to alter this file";
	}
}
