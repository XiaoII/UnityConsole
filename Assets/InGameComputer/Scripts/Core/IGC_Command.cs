using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class IGC_Command {

	public Dictionary<string, string> flags = new Dictionary<string, string> ();
	public string[] argv;
	public IGC_User issuer;
	public IGC_VirtualSystem virtualSystem;
	public bool CanPipe = false;
	public string 
		name = "name",
		help_text = "help text",
		description = "help text",
		output = "",
		usage = "";
	protected string 
		malformed_error = "error: command malformed";

	public string trigger(string cmd_str, ref IGC_User asUser)
	{
		var tempArgv = new List<string>();
		string lastQuote = String.Empty;
		int lastCut = 0;

	//parse args
		for(int i=0; i<cmd_str.Length; i++){
			if( (cmd_str[i] == '"' || cmd_str[i] == '\'') && lastQuote == String.Empty){
				lastQuote = cmd_str[i].ToString();
			}else if(lastQuote == cmd_str[i].ToString()){
				lastQuote = String.Empty;
			}

			if( (lastQuote == String.Empty && cmd_str[i] == ' ') || i == cmd_str.Length-1){
				string arg = cmd_str.Substring(lastCut, i+1-lastCut);
				if(arg.Length > 0 && arg.Replace(" ","") != ""){//make sure it's not a space, or escaped space
					tempArgv.Add(arg.Trim());
				}
				lastCut = i;
			}
		}
	//parse flags
		/*
		 * format:
		 * 	-key[[:|=]value]
		 */
		flags.Clear ();

		foreach(string arg in tempArgv){
			if(arg[0] == '-'){
				string flag = arg;
				while(flag.Length != 0 && flag[0] == '-'){flag = flag.Remove(0,1);}
				string[] f = flag.Split(new string[2]{"=",":"}, StringSplitOptions.RemoveEmptyEntries);
				if(f.Length > 1){
					flags.Add(f[0], f[1]);
				}else{
					flags.Add(f[0], "");
				}
			}
		}

		this.argv = tempArgv.ToArray ();
		this.issuer = asUser;

		return command_function();
	}

	public virtual string command_function(){return "";}
}
