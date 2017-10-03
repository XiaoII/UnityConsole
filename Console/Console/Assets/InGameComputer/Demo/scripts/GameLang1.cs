using UnityEngine;
using System.Collections;

public class GameLang1 : IGC_Language 
{
	public override void DefineCustomLanguage()
	{
		//custom commands get defined here
		commands.Add("opendoor", new OpenDoor(ref this.virtualSystem));
	}
}