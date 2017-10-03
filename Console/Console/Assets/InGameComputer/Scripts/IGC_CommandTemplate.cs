using UnityEngine;
using System.Collections;

public class IGC_CommandTemplate : IGC_Command {

	/* useful variables from the base class
	 * 
	 * - virtualSystem 
	 * 		the virtual system this command will run on
	 * - issuer 
	 * 		the user who triggered this command
	 * - argv 
	 * 		a string array of each word used in the string passed as the command. Phrases wrapped in quotes are treated as one word.
	 * - flags  
	 * 		a Dictionary<string, string> containing each element in argv that begins with the - char. If the element contains a = or : the value string will be set to everything after the = or : char, and the key will be everything between the - and the = or :. if neither of those chars is present, then the key will be all text in the flag, and the value will be empty.
	 */

	public IGC_CommandTemplate(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "command_name";
		this.usage = "usage: ...";
		this.help_text = "Describe what this command does and how to use it.";
		this.description = "describe your command in 8 words or less";
	}
	
	public override string command_function ()
	{
		//your command code here.

		//the string returned by this command will be displayed on the terminal screen of the user who triggered this command
		return "";
	}
}
