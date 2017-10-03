using UnityEngine;
using System.Collections;

public class DebugLang : IGC_Language 
{
	public override void DefineCustomLanguage()
	{
		//name your language
		this.LangName = "crlix + debug";

		//define custom commands here
		commands.Add("sysinfo", new IGC_Debug_sysinfo(ref this.virtualSystem));
	}
}
