using UnityEngine;
using System.Linq;
using System.Collections;

public class IGC_Command_groups : IGC_Command {

	private IGC_UserRegistry registry;
	private string keyword;
	
	public IGC_Command_groups(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "groups";

		this.usage = "usage:\n";
		this.usage += "list all groups on this machine\ngroups\n\n";
		this.usage += "get group info\ngroups info <group_name>\n\n";
		this.usage += "add new group\ngroups add <group_name>\n\n";
		this.usage += "remove group\ngroups rm <group_name>\n\n";
		this.usage += "add user to group\ngroups adduser <group_name> <username> <group admin? y|n>\n\n";
		this.usage += "remove user from group\ngroups rmuser <group_name> <username>\n\n";

		this.help_text = "use the groups command to add, remove, and edit groups. only group administrators can add users to a group or remove users from a group. group creators are automatically assigned as an administrator.";
		this.description = "add, remove, edit groups";
	}
	
	public override string command_function()
	{
		registry = virtualSystem.userRegistry;
		
		if(argv.Length == 1){
			return "groups: "+string.Join(", ", registry.ListGroups());
		}
		
		if(argv.Length > 1){
			keyword = argv[1];
			return GroupActions();
		}
		return "";
	}

	private string GroupActions(){
		if(argv.Length < 2){return "groups: "+string.Join(", ", registry.ListGroups());}
		
		keyword = argv [1];
		
		switch (keyword) {
		case "info":
			return GroupInfo();
		case "add":
			return AddGroup();
		case "rm":
			return RemoveGroup();
		case "adduser":
			return AddUserToGroup();
		case "rmuser":
			return RemoveUserFromGroup();
		default:
			return malformed_error+"\n"+"usage: groups <add|remove|adduser|rmuser> ...";
		}
	}
	
	private string RemoveUserFromGroup(){
		if(argv.Length != 4){
			return malformed_error+"\n"+"usage: groups rmuser <group_name> <username>";
		}

		IGC_UserGroup group = registry.GetGroup (argv [2]);
		if(group == null){return "group "+argv[2]+" does not exist";}

		IGC_User user = registry.GetUser(argv[3]);
		if(user == null){return "user "+argv[3]+" does not exist";}

		if(!user.groups.Contains(group)){
			return user.name+" is not in "+group.name;
		}
		
		if(!group.admins.Contains(issuer) && !issuer.isAdmin){
			return "only system or group administrators can add or remove users";
		}

		if(group.RemoveUser(user, issuer)){
			return user.name + " removed from group " + group.name;
		}else{
			return "insufficient privilages or user not in group.";
		}
	}
	
	private string AddUserToGroup(){
		if(argv.Length != 5){
			return malformed_error+"\n"+"usage: groups adduser <group_name> <username> <group admin? y|n>";
		}
		
		IGC_User user = registry.GetUser(argv[3]);
		if(user == null){return "user "+argv[3]+" does not exist";}
		
		IGC_UserGroup group = registry.GetGroup (argv [2]);
		if(group == null){return "group "+argv[2]+" does not exist";}

		if(user.groups.Contains(group)){
			return user.name+" is already in "+group.name;
		}

		if(!group.admins.Contains(issuer) && !issuer.isAdmin){
			return "only system or group administrators can add or remove users";
		}

		bool asAdmin = argv [4] == "y" ? true : false;

		group.AddUser (user, asAdmin);
		
		return user.name + " added to group " + group.name + (asAdmin ? " as admin" : "");
	}
	
	private string AddGroup(){
		if(argv.Length != 3){
			return malformed_error+"\n"+"usage: groups add <group_name>";
		}
		
		string groupname = IGC_Utils.EscapeUserOrGroup (argv [2]);
		
		if(registry.AddGroup(groupname, issuer) != null){
			return "group " + groupname + " created successfully";
		}else{
			return "group " + groupname + " already exists";
		}
	}
	
	private string RemoveGroup(){
		if(argv.Length != 3){
			return malformed_error+"\n"+"usage: groups rm <group_name>";
		}
		
		IGC_UserGroup group = registry.GetGroup (argv [2]);
		
		if(group != null){
			if(!group.admins.Contains(issuer) && !issuer.isAdmin){
				return "only system or group administrators can remove groups";
			}

			registry.RemoveGroup(group, issuer);
			
			return "group " + argv[2] + " removed successfully";
		}
		return "group " + argv[2] + " does not exist";
	}
	
	private string GroupInfo(){
		if(argv.Length != 3){
			return malformed_error+"\n"+"usage: groups info <group_name>";
		}
		
		IGC_UserGroup group = registry.GetGroup (argv [2]);
		
		if(group != null){
			
			string[] users = new string[group.users.Count];
			int i = 0;
			
			foreach(IGC_User user in group.users){
				if(!group.admins.Contains(user)){
					users[i++] = user.name;
				}
			}
			
			string[] admins = new string[group.admins.Count];
			i = 0;
			
			foreach(IGC_User user in group.admins){
				admins[i++] = user.name;
			}
			
			return "ADMINS: "+string.Join(", ", admins)+"\nUSERS: "+string.Join(", ", users.Where(s => !string.IsNullOrEmpty(s)).ToArray());
		}
		return "group " + argv[2] + " does not exist";
	}

}
