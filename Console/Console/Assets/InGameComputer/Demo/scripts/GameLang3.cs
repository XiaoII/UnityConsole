using UnityEngine;
using System.Collections;

public class GameLang3 : IGC_Language 
{
	public override void DefineCustomLanguage()
	{
		//custom commands get defined here
		commands.Add("win", new Win(ref this.virtualSystem));
		commands.Add("opendoor", new OpenDoor(ref this.virtualSystem));
	}
}