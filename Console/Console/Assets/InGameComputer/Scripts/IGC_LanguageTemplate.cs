using UnityEngine;
using System.Collections;

public class IGC_LanguageTemplate : IGC_Language 
{
	public override void DefineCustomLanguage()
	{
		//name your language
		this.LangName = "language name";
		
		/* define custom commands here
		 * 
		 * for each command you want to add to this language, copy the following 
		 * line and replance the name and class with that of your command
		 * 
		 * commands.Add("name_of_command", new ClassOfCommand(ref this.virtualSystem));
		 */
	}
}

