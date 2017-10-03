using UnityEngine;
using System.Collections;

public class GameLang2 : IGC_Language 
{
	public override void DefineCustomLanguage()
	{
		//custom commands get defined here
		commands.Add("bridge", new ControlBridge(ref this.virtualSystem));
	}
}