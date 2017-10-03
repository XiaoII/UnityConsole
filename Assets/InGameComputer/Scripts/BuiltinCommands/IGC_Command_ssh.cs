using UnityEngine;
using System.Collections;

public class IGC_Command_ssh : IGC_Command {

	public IGC_Command_ssh(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "ssh";
		this.usage = "usage: ssh <remote_user_name>:<remote_user_password>@<remote_ip_address>";
		this.help_text = "the ssh command allows you to log in as users on other computers from your current computer.";
		this.description = "log in on other computers";
	}
	
	public override string command_function ()
	{
		if(argv.Length == 1){return this.usage+"\n"+this.help_text;}
		if(argv.Length != 2){return this.malformed_error+"\nusage: ssh remote_user:remote_user_password@Remote_address";}

		if(issuer.loggedInRemotely){return "you are already logged in on a remote system. terminate the current connection before starting another ssh connection.";}

		string[] 
			remoteAddress = argv [1].Split (new string[1]{"@"}, System.StringSplitOptions.RemoveEmptyEntries),
			userInfo = remoteAddress[0].Split (new string[1]{":"}, System.StringSplitOptions.RemoveEmptyEntries);
	
		if(userInfo.Length != 2 || remoteAddress.Length != 2){return "address "+argv[1]+" not understood";}

		string remoteIP = remoteAddress [1];

		GameObject r = GameObject.Find ("Computer_" + remoteIP);
		if(r == null){return "no host with that address";}
		if(!r.GetComponent<InGameComputer>().powerState){return "no response. are you sure the machine you're trying to access is not off?";}

		if(r == issuer.terminal.gameObject){return "you cannot create an ssh connection with the local system.";}

		IGC_VirtualSystem remotehost = r.GetComponent<IGC_VirtualSystem>();

		IGC_User remoteUser = remotehost.userRegistry.GetUser(userInfo[0]);
		if(remoteUser == null){return userInfo[0]+" does not exist on "+remoteIP;}


		if(remotehost.userRegistry.loggedInUsers.Contains(remoteUser)){return remoteUser.name+" is already logged in";}

		if(!remoteUser.CheckPassword(userInfo[1])){return "password incorrect";}

		issuer.terminal.SetPrevUser(issuer);
		issuer.terminal.SwapVirtualSystem(ref remotehost);

		issuer.terminal.virtualSystem.userRegistry.Login (userInfo [0], userInfo [1], issuer.terminal);
		//issuer.terminal.currentUser.loggedInRemotely = true;

		return "connected to " + remotehost.IP + " as " + remoteUser.name;
	}
}
