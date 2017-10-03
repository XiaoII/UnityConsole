using UnityEngine;
using System.Collections;

public class Win : IGC_Command {
	
	public Win(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "win";
		this.help_text = "Use this command to win the game.";
	}
	
	public override string command_function ()
	{
		if(!issuer.isAdmin){return "only system administrators can win";}

		GameObject.Find ("player").SendMessage ("WinGame");
		
		return "";
	}
}
