using UnityEngine;
using System.Collections;

public class IGC_Command_color : IGC_Command {
	
	public IGC_Command_color(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "color";
		this.usage = "usage: color -s=<r>,<g>,<b> -t=<r>,<g>,<b>";
		this.help_text = "use this command to set the screen and text colors. use the -s flag for the screen, and the -t flag for the text. \nexample (red screen, blue text):\ncolor -s=1,0,0 -t=0,0,1";
		this.description = "set the screen and text colors";
	}
	
	public override string command_function()
	{
		string output = "";

		foreach(string key in flags.Keys){
			if(key != "s" && key != "t"){continue;}

			string[] channels = IGC_Utils.SplitString(",", flags[key]);

			if(channels.Length != 3){return malformed_error+"\n"+usage;}

			Color color = IGC_Utils.ColorFromString(flags[key]);

			if(key == "s"){
				output+= "screen color set to "+flags["s"];
				issuer.terminal.SetScreenColor(color);
				issuer.terminal.screenColor = color;
			}
			if(key == "t"){
				output+= "text color set to "+flags["t"];
				issuer.terminal.SetTextColor(color);
				issuer.terminal.textColor = color;
			}
		}

		output = output == "" ? "please specify a screen or text color\n" + usage : output;

		InGameComputer term = issuer.terminal;

		if(virtualSystem.networkReady){
			term.GetComponent<NetworkView>().RPC("SetColorsRPC", RPCMode.Others, 
				IGC_Utils.ColorString(term.screenColor), 
				IGC_Utils.ColorString(term.textColor)
			);
		}

		return output;
	}
}
