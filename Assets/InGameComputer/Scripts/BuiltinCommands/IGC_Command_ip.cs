using UnityEngine;
using System.Collections;

public class IGC_Command_ip : IGC_Command {
	
	public IGC_Command_ip(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "ip";
		this.help_text = "the ip command displays the IP address of the current machine.";
		this.description = "print IP address";
	}
	
	public override string command_function()
	{
		return virtualSystem.IP;
	}
}
