using UnityEngine;
using System.Collections;

public class IGC_Debug_sysinfo : IGC_Command {
	
	public IGC_Debug_sysinfo(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.usage = "usage: sysinfo [-g|t|s|vs|fs|ur]";
	}
	
	public override string command_function ()
	{
		//terminal
		string termOutput = "___TERMINAL___\n";

		InGameComputer igc = issuer.terminal;

		termOutput += "instance id: " + igc.instanceID + "\n";
		termOutput += "current power state: " + igc.powerState + "\n";
		termOutput += "login name/pass: " + igc.loginName + "/" + issuer.terminal.loginPass + "\n";
		termOutput += "start in on state: " + igc.startInOnState + "\n";
		termOutput += "current user: " + (igc.currentUser != null ? igc.currentUser.name : "null") + "\n";
		termOutput += "bounce back user: " + (igc.previousUser != null ? igc.previousUser.name : "null") + "\n";
		termOutput += "remote login: " + igc.loggedInRemotely + "\n";
		termOutput += "in use: " + igc.inUse + "\n";
		termOutput += "terminal occupied: " + igc.terminalOccupied + "\n";
		termOutput += "booting up: " + igc.bootingUp + "\n";
		termOutput += "network ready: " + igc.networkReady  + "\n";

		//shell
		string shellOutput = "___SHELL___\n";

		IGC_Shell sh = issuer.terminal.shell;
		
		shellOutput += "display width/height: " + sh.displayWidth +"/"+ sh.displayHeight + "\n";
		shellOutput += "prompt width: " + sh.promptWidth + "\n";
		shellOutput += "language: " + sh.lang.LangName + "\n";
		shellOutput += "terminal id: " + sh.computer.instanceID + "\n";
		shellOutput += "input mode: " + sh.inputMode + "\n";
		shellOutput += "capslock: " + sh.capslock + "\n";
		shellOutput += "ctrl: " + sh.ctrl + "\n";
		shellOutput += "history count: " + sh.history.Count + "\n";
		shellOutput += "cursor offset x/y: " + sh.cursorOffset +"/"+ sh.cursorOffsetVertical + "\n";
		shellOutput += "display line offset: " + sh.lineOffset + "\n";
		shellOutput += "last edited: " + (sh.lastEditedFile != null ? sh.lastEditedFile.path : "null") + "\n";
		shellOutput += "raw edit string len: " + sh.rawEditString.Length + "\n";
		shellOutput += "raw display text len: " + sh.rawDisplayText.Length + "\n";
		shellOutput += "formatted display len: " + sh.output.text.Length + "\n";
		shellOutput += "raw prompt text len: " + sh.rawPromptText.Length + "\n";
		shellOutput += "formatted prompt len: " + sh.prompt.text.Length + "\n";
		shellOutput += "infoline len: " + sh.infoLine.text.Length + "\n";


		//virtual system
		string vsOutput = "___VIRTUAL SYSTEM___\n";
		
		IGC_VirtualSystem vs = virtualSystem;

		vsOutput += "instance id: " + vs.instanceID + "\n";
		vsOutput += "model name: " + vs.modelName + "\n";
		vsOutput += "ip: " + vs.IP + "\n";
		vsOutput += "permanent ip: " + vs.hasPermanentIP + "\n";
		vsOutput += "language: " + vs.language.LangName + "\n";
		vsOutput += "startup stasks complete: " + vs.startUpTasksComplete + "\n";
		vsOutput += "network ready: " + vs.networkReady + "\n";
		vsOutput += "erase save data on start: " + vs.eraseSaveDataOnStart + "\n";
		vsOutput += "resore data len: " + vs.restoreData.Length + "\n";

		//userregistry
		string urOutput = "___USER REGISTRY___\n";
		
		IGC_UserRegistry ur = virtualSystem.userRegistry;

		urOutput += "virtual system id: " + ur.virtualSystem.instanceID + "\n";
		urOutput += "ready for use: " + ur.ready + "\n";
		urOutput += "users logged in: " + ur.loggedInUsers.Count + "\n";
		urOutput += "users: " + ur.users.Count + "\n";
		urOutput += "groups: " + ur.groups.Count + "\n";
		urOutput += "users editor string: \n" + string.Join("\n", ur.usersList) + "\n";
		urOutput += "groups editor string: \n" + string.Join("\n", ur.groupsList) + "\n";

		//file system
		string fsOutput = "___FILE SYSTEM___\n";
		
		IGC_FileSystem fs = virtualSystem.fileSystem;

		fsOutput += "virtual system id: " + fs.virtualSystem.instanceID + "\n";
		fsOutput += "ready for use: " + fs.ready + "\n";
		fsOutput += "files: " + fs.files.Count + "\n";
		fsOutput += "root node: " + fs.rootNode.name + "\n";

		//unique game instance id
		string gameID = "unique game instance id: " + IGC_VirtualSystem.uniqueID + "\n";

		string output = "";

		if(flags.Count > 0){
			foreach(var k in flags.Keys){
				if(k == "g"){output = gameID;}
				if(k == "t"){output = termOutput;}
				if(k == "s"){output = shellOutput;}
				if(k == "vs"){output = vsOutput;}
				if(k == "fs"){output = fsOutput;}
				if(k == "ur"){output = urOutput;}
			}
			if(output == ""){return "flag(s) not recognised";}
		}else{
			output = gameID + termOutput + shellOutput + vsOutput + urOutput + fsOutput;
		}

		issuer.terminal.shell.EnterViewMode (output);

		return "";
	}
}
