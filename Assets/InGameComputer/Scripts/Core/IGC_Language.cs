using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IGC_Language : MonoBehaviour{

	public string LangName = "crlix";
	public Dictionary<string, IGC_Command> commands = new Dictionary<string, IGC_Command>();
	[HideInInspector] public IGC_VirtualSystem virtualSystem;

	public void Start()
	{
		this.virtualSystem = gameObject.GetComponent<IGC_VirtualSystem>();
		StartCoroutine ("DefineLangauge");
	}


	private IEnumerator DefineLangauge()
	{
		while(this.virtualSystem == null){yield return null;}

		//built-in commands get defined here
		commands.Add ("ip", new IGC_Command_ip(ref this.virtualSystem));
		commands.Add ("mv", new IGC_Command_mv(ref this.virtualSystem));
		commands.Add ("cp", new IGC_Command_cp(ref this.virtualSystem));
		commands.Add ("rm", new IGC_Command_rm(ref this.virtualSystem));
		commands.Add ("cd", new IGC_Command_cd(ref this.virtualSystem));
		commands.Add ("ls", new IGC_Command_ls(ref this.virtualSystem));
		commands.Add ("ssh", new IGC_Command_ssh(ref this.virtualSystem));
		commands.Add ("pwd", new IGC_Command_pwd(ref this.virtualSystem));
		commands.Add ("cat", new IGC_Command_cat(ref this.virtualSystem));
		commands.Add ("man", new IGC_Command_man(ref this.virtualSystem));
		commands.Add ("who", new IGC_Command_who(ref this.virtualSystem));
		commands.Add ("less", new IGC_Command_less(ref this.virtualSystem));
		commands.Add ("help", new IGC_Command_help(ref this.virtualSystem));
		commands.Add ("edit", new IGC_Command_edit(ref this.virtualSystem));
		commands.Add ("save", new IGC_Command_save(ref this.virtualSystem));
		commands.Add ("file", new IGC_Command_file(ref this.virtualSystem));
		commands.Add ("clear", new IGC_Command_clear(ref this.virtualSystem));
		commands.Add ("mkdir", new IGC_Command_mkdir(ref this.virtualSystem));
		commands.Add ("touch", new IGC_Command_touch(ref this.virtualSystem));
		commands.Add ("users", new IGC_Command_users(ref this.virtualSystem));
		commands.Add ("login", new IGC_Command_login(ref this.virtualSystem));
		commands.Add ("color", new IGC_Command_color(ref this.virtualSystem));
		commands.Add ("groups", new IGC_Command_groups(ref this.virtualSystem));
		commands.Add ("logout", new IGC_Command_logout(ref this.virtualSystem));
		commands.Add ("whoami", new IGC_Command_whoami(ref this.virtualSystem));
		commands.Add ("passwd", new IGC_Command_passwd(ref this.virtualSystem));
		commands.Add ("history", new IGC_Command_history(ref this.virtualSystem));
		commands.Add ("shutdown", new IGC_Command_shutdown(ref this.virtualSystem));
        commands.Add ("netscan", new IGC_Command_netscan(ref this.virtualSystem));

        //aliases 
        commands.Add ("su", new IGC_Command_login(ref this.virtualSystem));
		commands.Add ("rename", new IGC_Command_mv(ref this.virtualSystem));

		DefineCustomLanguage();
	}

	public virtual void DefineCustomLanguage(){}

	public bool HasCommand(string cmd)
	{
		return commands.ContainsKey (cmd);
	}

	public bool cmdSafeWhileLoggedOut(string cmd)
	{
		return cmd == "help" || cmd == "login" || cmd == "man" || cmd == "testargv";
	}
}
