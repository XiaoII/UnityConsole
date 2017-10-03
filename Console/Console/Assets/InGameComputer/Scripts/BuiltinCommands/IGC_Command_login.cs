using UnityEngine;
using System.Collections;

public class IGC_Command_login : IGC_Command {

	public IGC_Command_login(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "login";
		this.usage = "usage: login <username> <password>";
		this.help_text = "use this command to log in as a specific user or switch to another user.";
		this.description = "log in as <user> with <password>";
	}
	
	public override string command_function()
	{
		IGC_UserRegistry ur = virtualSystem.userRegistry;

		if(argv.Length > 2){
			if(!ur.users.ContainsKey(argv[1])){return "user '"+argv[1]+"' does not exist";}

			InGameComputer terminal = issuer == null ? virtualSystem.terminal : issuer.terminal;

			IGC_User checkLoggedIn = ur.GetUser(argv[1]);

			if(ur.loggedInUsers.Contains(checkLoggedIn)){return argv[1]+" is already logged in";}

			if(issuer != null){ 
				if(issuer.terminal != null){
					if(issuer.terminal.currentUser != null){
						if(issuer.loggedInRemotely){
							ur.loggedInUsers.Remove(issuer); //the logout function will bounce the user back to their own terminal so here, if remotelogged, we will keep things simple
						}else{
							virtualSystem.userRegistry.Logout(issuer.terminal.currentUser);
						}
					}
				}
			}

			IGC_User user = ur.Login(argv [1], argv [2], terminal);

			if(user != null){
				return "welcome, "+user.name;
			}else{
				return "password incorrect, try again";
			}
		}else{
			return "error: command malformed\n"+usage; 
		}
	}
}






	

		