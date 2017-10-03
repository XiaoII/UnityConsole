using UnityEngine;
using System.Collections;

public class IGC_Command_shutdown : IGC_Command {

	public IGC_Command_shutdown(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "shutdown";
		this.help_text = "shutdown the computer. if other users are logged in and you are an administrator you may force a shutdown, otherwise you cannot turn of the computer.";
		this.description = "shutdown the computer";
	}
	
	public override string command_function ()
	{
		if(virtualSystem.userRegistry.loggedInUsers.Count > 1){
			if(flags.ContainsKey("f")){
				if(issuer.isAdmin){
					virtualSystem.ForceShutdown(issuer);
					return ""; 
				}else{
					return "Only admins may force a shutdown";
				}
			}
			return "others are logged into this system. use the -f flag to force a shutdown.";
		}

		virtualSystem.StartShutdownTimer();
		return "";
	}


}
