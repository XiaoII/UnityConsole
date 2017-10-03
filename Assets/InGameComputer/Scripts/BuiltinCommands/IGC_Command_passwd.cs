using UnityEngine;
using System.Collections;

public class IGC_Command_passwd : IGC_Command {

	public IGC_Command_passwd(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "passwd";
		this.usage = "usage: passwd [username] <new_password>";
		this.help_text = "type YOUR new password as the second word of the command. if a valid username is the second word of the command and you are an administrator, the password of the listed user will be changed.";
		this.description = "change a user's password";
	}
	
	public override string command_function()
	{
		if(argv.Length == 2){
			issuer.SetPassword(argv[1]);
			return "your new password has been set";
		}
		if(argv.Length == 3){
			IGC_User user = virtualSystem.userRegistry.GetUser(argv[1]);

			if(user == null){return "user does not exist";}

			if(!issuer.admin && user.name != issuer.name){return "only admins can change other users' passwords";}

			user.SetPassword(argv[2]);

			if(virtualSystem.networkReady){
				virtualSystem.GetComponent<NetworkView>().RPC("SetPasswordRPC", RPCMode.Others, user.name, argv[2]);
			}

			return user.name+"'s new password has been set";
		}

		return malformed_error + "\n" + usage;
	}
}
