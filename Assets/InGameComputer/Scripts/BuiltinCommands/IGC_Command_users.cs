using UnityEngine;
using System.Linq;
using System.Collections;

public class IGC_Command_users : IGC_Command {

	private IGC_UserRegistry registry;
	private string keyword;

	public IGC_Command_users(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "users";

		this.usage = "usage:\n";
		this.usage += "list all users on this machine\nusers\n\n";
		this.usage += "get user info\nusers info <user_name>\n\n";
		this.usage += "add new user\nusers add <user_name> <password> <admin? y|n>\n\n";
		this.usage += "delete user\nusers rm <user_name>\n\n";
		this.usage += "change user's admin status\nusers admin <user_name> <y|n>\n";
		
		this.help_text = "use the users command to add, remove, and edit users. only administrators can delete users or change a user's admin status.";
		this.description = "add, remove, edit users";
	}
	
	public override string command_function()
	{
		registry = virtualSystem.userRegistry;

		if(argv.Length == 1){
			return "users: "+string.Join(", ", registry.ListUsers());
		}

		if(argv.Length > 1){
			keyword = argv[1];
			return UserActions();
		}
		return "";
	}

	private string UserActions(){
		if(argv.Length < 2){return "usage: users <info|add|rm|admin> ...";}
		
		switch (keyword) {
			case "info":
				return UserInfo();
			case "add":
				return AddUser();
			case "rm":
				return RemoveUser();
			case "admin":
				return AdminStatus();
			default:
				return malformed_error+"\n"+"usage: users <add|rm|info|admin> ...";
		}
	}

	private string UserInfo(){
		if(argv.Length != 3){
			return malformed_error+"\n"+"usage: users info <user_name>";
		}
		
		IGC_User user = registry.GetUser(argv [2]);
		
		if(user != null){
			string[] groups = new string[user.groups.Count];
			int i = 0;
			
			foreach(IGC_UserGroup group in user.groups){
				groups[i++] = group.name;
			}

			return "ADMIN: "+user.isAdmin.ToString()+"\nGROUPS: "+string.Join(", ", groups)+"\nCWD: "+user.cwd;
		}
		return "user " + argv[2] + " does not exist";
	}

	private string AddUser(){
		if(argv.Length != 5){
			return malformed_error+"\n"+"usage: users add <user_name> <password> <admin? y|n>";
		}

		bool wantAdmin = argv [4] == "y" ? true : false;

		string username = IGC_Utils.EscapeUserOrGroup (argv [2]);

		if(wantAdmin && !issuer.isAdmin){return "only admins can create admins";}

		if(registry.AddUser(new IGC_User(username, argv[3], wantAdmin, virtualSystem, true), issuer) != null){
			return "user " + username + " created successfully";
		}else{
			return "user " + username + " already exists";
		}
	}

	private string RemoveUser(){
		if(argv.Length != 3){
			return malformed_error+"\n"+"usage: users rm <user_name>";
		}

		IGC_User user = registry.GetUser (argv [2]);
		if(user == null){return "user " + argv[2] + " does not exist";}

		if(!issuer.isAdmin){return "only administrators can delete users";}

		registry.RemoveUser(user);
		return "user " + user.name + " deleted. make sure to remove /home/"+user.name+"/ if you no longer need it";
	}

	private string AdminStatus(){
		if(argv.Length != 4){
			return malformed_error+"\n"+"usage: users admin <user_name> <y|n>";
		}

		if(!issuer.isAdmin){
			return "you do not have permission to grant administrative privaleges";
		}

		bool admin = argv [3] == "y" ? true : false;
		IGC_User user = registry.GetUser (argv [2]);

		if(user == null){
			return "user "+argv[2]+" does not exist";
		}

		user.Admin(admin); 
		return "user "+argv[2]+" is "+(admin ? "now" : "no longer")+" an administrator";
	}
}
