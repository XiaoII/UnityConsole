using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class IGC_Shell : MonoBehaviour{
	
	public int 
		displayWidth = 50,
		displayHeight = 10;
	[Range(0,10)]
	public float lineHeightModifier = 2f;
	[Range(0,100)]
	public float charWidthModifier = 16.6f;

	[HideInInspector] public IGC_Language lang;
	[HideInInspector] public InGameComputer computer, terminal;
	[HideInInspector] public TextMesh output, prompt, infoLine;
	[HideInInspector] public int[] displayXY;
	[HideInInspector] public Transform cursor;
	[HideInInspector] public IGC_File lastEditedFile;
	[HideInInspector] public List<string> 
		history = new List<string>(),
		currentEditStringList = new List<string>();
	[HideInInspector] public float 
		gameUnitLineHeight = 0,
		gameUnitCharWidth = 0,//0.111f,
		gameUnitCharHeight = 0;//0.264f; 
	[HideInInspector] public int 
		cursorOffset = 0, 
		cursorOffsetVertical = 0, 
		lineOffset = 0,
		promptWidth,
		inputMode = 0;
	[HideInInspector] public string 
		rawDisplayText,
		rawPromptText = string.Empty,
		rawEditString,
		formattedEditString;
	[HideInInspector] public enum inputModes {
		CLI = 0,
		TextView = 1,
		TextEdit = 2
	}
	[HideInInspector] public bool 
		capslock = false,
		ctrl = false;
	private string currentKey = string.Empty;
	private IGC_InputChars keys = new IGC_InputChars();
	private string[] keyStrings;
	private float lastInputTime;
	private int historyIndex = 0;

	void Start()
	{
		computer = gameObject.GetComponent<InGameComputer>();
		terminal = computer;
		cursor = computer.transform.Find("monitor/display/cursor_holder/cursor");
		lang = computer.virtualSystem.language;
		output = computer.output;
		prompt = computer.prompt;
		infoLine = computer.infoLine;

		displayXY = new int[2];
		displayXY [0] = displayWidth;
		displayXY [1] = displayHeight;
		promptWidth = displayXY [0] - 4;

		FieldInfo[] keyProps = keys.GetType().GetFields();
		keyStrings = new string[keyProps.Length];
		int i = 0;

		foreach(FieldInfo prop in keyProps){
			keyStrings[i++] = (string)prop.GetValue(keys);
		}

	//set cursor and char dimensions
		float
			resolution = (Screen.dpi + 1) * 150f,
			fontWidth = (output.fontSize == 0 ? 1 : output.fontSize) * 16.6f;
		gameUnitCharWidth = fontWidth / resolution;
		gameUnitCharHeight = gameUnitCharWidth * 1.5f;
		gameUnitLineHeight = ((output.lineSpacing * fontWidth) / resolution) * 2;

		cursor.localScale = new Vector3 (gameUnitCharWidth, gameUnitCharHeight, 1);

		StartCoroutine (BlinkCursor ());
		output.text = FormatForCLI("");
	}

	private void UpdateCursor(){
		if(inputMode == (int)inputModes.CLI){
			cursor.localPosition = new Vector3(gameUnitCharWidth * (Mathf.Clamp (prompt.text.Length - cursorOffset, 0, promptWidth)), 0,0);
		}else if(inputMode == (int)inputModes.TextEdit){
			cursor.localPosition = new Vector3(
				output.transform.localPosition.x + (gameUnitCharWidth * (cursorOffset)) - 0.3732371f,
				output.transform.localPosition.x - (gameUnitLineHeight * cursorOffsetVertical) - 0.326726f,
				0);
		}else if(inputMode == (int)inputModes.TextView){
			
		}
	}

	public void stdout(string message){
		string saveRaw = rawPromptText;
		rawPromptText = "\t";
		output.text = FormatForCLI (message);
		rawPromptText = saveRaw;
	}

	IEnumerator BlinkCursor(){
		while(true){
			if(inputMode != (int)inputModes.TextView){
				if(Time.time - lastInputTime < 0.3f){
					cursor.GetComponent<Renderer>().enabled = true;
				}else{
					if(computer.powerState && inputMode != (int)inputModes.TextView){
						cursor.GetComponent<Renderer>().enabled = !cursor.GetComponent<Renderer>().enabled;
					}else{
						cursor.GetComponent<Renderer>().enabled = false;
					}
				}
			}
			yield return new WaitForSeconds(.7f);
		}
	}


	void Update(){
		if(computer.inUse){
			if(Input.anyKeyDown){lastInputTime = Time.time;}

		//ctrl key down?
			ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		//capslock
			if(Input.GetKeyDown(KeyCode.CapsLock)){
				capslock = !capslock; 
				return;
			}
		//backspacing
			if(Input.GetKeyDown(KeyCode.Backspace)){
				StopCoroutine("Delete");
				StartCoroutine(Delete("BACKSPACE"));
				return;
			}
		//deleting
			if(Input.GetKeyDown(KeyCode.Delete)){
				StopCoroutine("Delete");
				StartCoroutine(Delete("DELETE"));
				return;
			}
		//spaces
			if(Input.GetKeyDown(KeyCode.Space)){TypeChar(" ");}

			if(inputMode == (int)inputModes.CLI){
				if(CLIReturnKey()){return;};
				if(CLIArrowKeys()){return;};
				if(CLITabKey()){return;};
				if(CLICTRLKeys()){return;};
			}else if(inputMode == (int)inputModes.TextEdit){
				if(EditArrowKeys()){return;}
				if(EditReturnKey()){return;}
				if(Input.GetKeyDown(KeyCode.Escape)){ ExitEditMode();}
			}else if(inputMode == (int)inputModes.TextView){
				if(ViewArrowKeys()){return;}
				if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q)){ ExitViewMode();return;}
			}

		//all keys that produce typable strings
			foreach(string key in keyStrings){
				if(Input.GetKeyDown(key)){
					TypeChar(key);
				}else{
					currentKey = string.Empty; 
				}
			}
		}
	}

	IEnumerator Delete(string message){
		while(true){
			if(rawPromptText.Length == 0 && inputMode == (int)inputModes.CLI){
				yield return false;
			}else if(Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)){ 
				TypeChar(message);
				yield return new WaitForSeconds(1f);
			}else if (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete)){
				TypeChar(message);
				yield return new WaitForSeconds(1f/30f);
			}else if (!Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete)){
				yield return false;
			}
		}
	}

	public void TypeChar(string c){

		int safeIndex;

		if(inputMode == (int)inputModes.CLI){
			if(c == "BACKSPACE"){
				safeIndex = rawPromptText.Length -1 -cursorOffset;
				if(safeIndex < 0){return;}
				rawPromptText = rawPromptText.Remove(safeIndex, 1);
				c = "";
			}else if(c == "DELETE"){
				safeIndex = rawPromptText.Length - cursorOffset;
				if(safeIndex >= rawPromptText.Length){return;}
				rawPromptText = rawPromptText.Remove(safeIndex, 1);
				cursorOffset--;
				c = "";
			}
		}

		bool shift = Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift);
		
		if(shift){
			if(c == keys.ALPHA9){c = "(";}
			else if(c == keys.ALPHA0){c = ")";}
			else if(c == keys.LEFTBRACKET){c = "{";}
			else if(c == keys.RIGHTBRACKET){c = "}";}
			else if(c == keys.SEMICOLON){c = ":";}
			else if(c == keys.QUOTESINGLE){c = "\"";}
			else if(c == keys.COMMA){c = "<";}
			else if(c == keys.PERIOD){c = ">";}
			else if(c == keys.MINUS){c = "_";}
			else if(c == keys.EQUALS){c = "+";}
			else if(c == keys.BACKSLASH){c = "?";}
			else if(c == keys.FORWARDSLASH){c = "|";}
			else if(c == keys.ALPHA1){c = "!";}
			else if(c == keys.ALPHA2){c = "@";}
			else if(c == keys.ALPHA3){c = "#";}
			else if(c == keys.ALPHA4){c = "$";}
			else if(c == keys.ALPHA5){c = "%";}
			else if(c == keys.ALPHA6){c = "^";}
			else if(c == keys.ALPHA7){c = "&";}
			else if(c == keys.ALPHA8){c = "*";}
			else if(c == keys.TICK){c = "~";}
		}
		
		currentKey = (shift && !capslock) || (capslock && !shift)
			? c.ToUpper() 
			: c;

		switch(inputMode){
			case (int)inputModes.CLI: TypeCharCLI(currentKey); break;
			case (int)inputModes.TextEdit: TypeCharEdit(currentKey); break;
			case (int)inputModes.TextView: TypeCharView(currentKey); break;
		}
	}

	private void TypeCharView(string c)
	{
		output.text = FormatForEdit (rawEditString);
	}

	private void TypeCharEdit(string c){

		//get location in edit string
		int pointInRawText = 0; 

		for(int i=0; i<lineOffset+cursorOffsetVertical; i++){
			if(currentEditStringList.Count == 0){break;}

			pointInRawText += currentEditStringList[i].Length;

			if(currentEditStringList[i].Length < displayXY[0]){
				pointInRawText++; 
			}
		}
		pointInRawText += cursorOffset; // should match currentEditStringList [lineOffset + cursorOffsetVertical] [cursorOffset]

		if (cursorOffset >= currentEditStringList [lineOffset + cursorOffsetVertical].Length) {
			cursorOffset  = currentEditStringList [lineOffset + cursorOffsetVertical].Length;
		}

		//deletion
		if(c == "BACKSPACE"){
			if(pointInRawText < 1){return;}
			rawEditString = rawEditString.Remove(pointInRawText - 1, 1);
			cursorOffset --;
			if(cursorOffset < 0){ 
				if(cursorOffsetVertical > 0){
					cursorOffsetVertical --;
				}else if(lineOffset > 0){
					lineOffset --;
				}
				cursorOffset = currentEditStringList [lineOffset + cursorOffsetVertical].Length;
			}

			c = "";
		}else if(c == "DELETE"){
			if(pointInRawText >= rawEditString.Length-1){return;}
			rawEditString = rawEditString.Remove(pointInRawText, 1);
			c = "";
		}

		//typing
		if(cursorOffset == displayXY[0]){
			cursorOffset = 0;
			if(cursorOffsetVertical == displayXY[1]-1){
				lineOffset ++;
			}else{
				cursorOffsetVertical ++;
			}
		} 

		if(c != string.Empty){
			rawEditString = rawEditString.Insert(pointInRawText, c);
			if(c == "\n"){
				cursorOffset = 0;
				if(cursorOffsetVertical == displayXY[1]-1){
					lineOffset ++;
				}else{
					cursorOffsetVertical ++;
				}
			}else{
				cursorOffset ++;
			}
		}

		//updating text and cursor
		output.text = FormatForEdit (rawEditString);
		UpdateCursor ();
		UpdateScreenNetwork ();
	}

	private string ReplaceAtIndex(int i, char value, string word)
	{
		char[] letters = word.ToCharArray();
		string[] s = new string[letters.Length];

		letters[i] = value;

		for(int a=0; a<letters.Length; a++){
			s[a] = letters[a].ToString();
		}

		return string.Join("", s);
	}

	private void TypeCharCLI(string c){
		int safeIndex;
		
		if(cursorOffset == 0){
			rawPromptText += currentKey;
		}else{
			safeIndex = Mathf.Clamp(rawPromptText.Length - cursorOffset, 0, rawPromptText.Length-1);
			rawPromptText = rawPromptText.Insert (safeIndex , c);
		}
		
		int promptOffset = cursorOffset - promptWidth > 0 ? cursorOffset - promptWidth : 0,
		startIndex = rawPromptText.Length - 1 >  promptWidth
			? rawPromptText.Length - promptWidth - promptOffset
				: 0,
				substrLength = rawPromptText.Length - 1 > promptWidth
				? promptWidth
				: rawPromptText.Length;
		
		prompt.text = rawPromptText.Substring(startIndex, substrLength);
		UpdateCursor ();
		UpdateScreenNetwork ();
	}

	private string FormatForCLI(string unformatted)
	{
		bool noFlare = rawPromptText == "\t"; 
		if(noFlare){rawPromptText = "";}

		//append unformatted to raw text, reduce length to dixplay x*y
		rawDisplayText = 
			(noFlare ? "" : "> ")
			+ rawPromptText 
			+ (noFlare ? "" : "\n")
			+ unformatted 
			+ (unformatted.Length > 0 ? "\n" : "")
			+ rawDisplayText;

		rawPromptText = "";
		prompt.text = "";
		int displayLimit = (displayXY [0] * displayXY [1]);
		
		rawDisplayText = rawDisplayText.Length > displayLimit 
			? (rawDisplayText.Substring(0,displayLimit))
			: (rawDisplayText);
		
		string formatted = "";
		
		int pointer = 0;
		for(var i=0; i<displayXY[1]; i++){
			if(pointer > rawDisplayText.Length){break;}
			
			int lookAheadLength = pointer + displayXY[0] > rawDisplayText.Length ? rawDisplayText.Length-pointer : displayXY[0],
			breakInLine = rawDisplayText.Substring(pointer, lookAheadLength).IndexOf("\n"); 
			
			if(breakInLine > -1){
				//Debug.Log("break at line "+i+" at char "+breakInLine);
				formatted += rawDisplayText.Substring(pointer, breakInLine+1);
				formatted += formatted[formatted.Length-1] == '\n' ? "" : "\n";
				pointer += breakInLine + 1;
				continue;
			}
			
			formatted += rawDisplayText.Substring(pointer, lookAheadLength)+"\n";
			pointer += displayXY[0];
		}

		UpdateScreenNetwork ();
		return formatted;
	}

	private string FormatForEdit(string unformatted)
	{
		int i = 0;
		List<string> formatted = new List<string>();
		string[] 
			lines = unformatted.Split('\n'),
			visibleText = new string[displayXY[1]];

		foreach(string line in lines){
			i = 0;
			while(true){
				int length = i + displayXY[0] > line.Length 
					? line.Length - i
					: displayXY[0];

				string l = line.Substring(i, length);
				formatted.Add(l);
				
				i += displayXY[0];
				if(i>line.Length-1){break;}
			}
		}

		for(i=0; i<visibleText.Length; i++){
			if(formatted.Count <= i + lineOffset){break;}
			if(formatted[i + lineOffset] != null){
				visibleText[i] = formatted[i + lineOffset];
			}
		}

		currentEditStringList = formatted;
		formattedEditString = string.Join ("\n", visibleText);

		UpdateScreenNetwork ();

		return formattedEditString;
	}

	public void Reset(){
		rawDisplayText = "";
		rawPromptText = "";
		if(rawPromptText != null && output != null && infoLine != null){
			prompt.text = "";
			infoLine.text = "";
			output.text = "";
		}
		cursorOffset = 0;
		cursorOffsetVertical = 0;
		history.Clear ();

		UpdateScreenNetwork ();
	}

	private bool CLICTRLKeys(){
		if(ctrl){
			if(Input.GetKeyDown(KeyCode.C)){
				rawPromptText = "";
				prompt.text = "";
				TypeChar("");
				return true;
			}
			else if(Input.GetKeyDown(KeyCode.L)){
				lang.commands["clear"].trigger("", ref computer.currentUser);
				TypeChar("");
				return true;
			}
			if(Input.GetKeyDown(KeyCode.A)){
				cursorOffset = rawPromptText.Length;
				TypeChar("");
				return true;
			}
			if(Input.GetKeyDown(KeyCode.E)){
				cursorOffset = 0;
				TypeChar("");
				return true;
			}
		}
		return false;
	}

	private bool CLIArrowKeys(){
	//history up and down
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			if(historyIndex < history.Count){
				++historyIndex;
				if(historyIndex == history.Count){
					rawPromptText = "";
				}else{
					rawPromptText = history[historyIndex];
				}
			}
			TypeChar("");
			return true;
		}
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			if(historyIndex > 0){
				rawPromptText = history[--historyIndex];
			}
			TypeChar("");
			return true;
		}
	//left and right offset
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			bool lim = cursorOffset < rawPromptText.Length;
			//int safeIndex;
			if(lim){
				if(ctrl){//word jumping
					//safeIndex = Mathf.Clamp(rawPromptText.Length -2 - cursorOffset, 0, rawPromptText.Length-1);
					if(rawPromptText[rawPromptText.Length -2 - cursorOffset] != ' ' && lim){//if you're just past a space jump back one char
						cursorOffset++;
					}
					//safeIndex = Mathf.Clamp(rawPromptText.Length -1 - cursorOffset, 0, rawPromptText.Length-1);
					while(rawPromptText[rawPromptText.Length -1 - cursorOffset] != ' ' && lim){//look for the next space back
						cursorOffset++;
						TypeChar ("");
					}
				}else{
					cursorOffset++;
				}
			}
			TypeChar ("");
			return true;
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			bool lim = cursorOffset > 0;
			//int safeIndex;
			if(lim){
				if(ctrl){//word jumping
					//safeIndex = Mathf.Clamp(rawPromptText.Length +1 -cursorOffset, 0, rawPromptText.Length-1);
					if(rawPromptText[rawPromptText.Length +1 -cursorOffset] != ' ' && lim){
						cursorOffset--;
					}
					//safeIndex = Mathf.Clamp(rawPromptText.Length - cursorOffset, 0, rawPromptText.Length-1);
					while(rawPromptText[rawPromptText.Length - cursorOffset] != ' ' && lim){//look for the next space back
						cursorOffset--;
						TypeChar ("");
					}
				}else{
					cursorOffset--;
				}
			}
			TypeChar ("");
			return true;
		}
		return false;
	}

	private string TrimFront(string trim)
	{
		while(trim[0] == ' '){trim = trim.Remove(0,1);}
		return trim;
	}

	private bool CLIReturnKey(){
		bool valid = false;
		if(Input.GetKeyDown(KeyCode.Return)){

			if(prompt.text == ""){
				output.text = FormatForCLI("");
				valid = true;
			}else{
				//semicolon command seporation
				string[] cmds = rawPromptText.Split(new string[1]{";"}, System.StringSplitOptions.RemoveEmptyEntries);
				 
				foreach(string cmd in cmds){
					valid = false;

					string trimmed, pipeTo = "";

					//piping
					if(cmd.IndexOf("|") > -1 && cmd.IndexOf("|") < cmd.Length-1){
						string[] pipedCmd = IGC_Utils.SplitString("|", cmd);
						trimmed = TrimFront(pipedCmd[0]);
						pipeTo = TrimFront(pipedCmd[1]);
					}else{
						trimmed = TrimFront(cmd);
					}


					string outputString = string.Empty; 
					if(trimmed != ""){history.Add(trimmed + (pipeTo != "" ? "| "+pipeTo : "") );}

					foreach(string name in lang.commands.Keys){
						if(computer.noUser && !lang.cmdSafeWhileLoggedOut(name)){continue;}

						if(trimmed.IndexOf(name) == 0){
							//trigger command
							valid = true;
							outputString = lang.commands[name].trigger(trimmed, ref computer.currentUser);

							//if this command should be piped
							if(pipeTo != ""){
								foreach(string pipeCmd in lang.commands.Keys){
									if(pipeTo.IndexOf(pipeCmd) == 0){
										//and the pipe command accepts stdin
										if(!lang.commands[pipeCmd].CanPipe){
											valid = false;
											outputString = pipeCmd + " does not accept standard input";
											break;
										}
										//run the output of the left command through the cmd right of the pipe
										outputString = lang.commands[pipeCmd].trigger(pipeCmd +" "+ outputString, ref computer.currentUser);
										Debug.Log (outputString);
									}
								}
							}
						}
					}

					historyIndex = history.Count; 
					rawPromptText = trimmed == "" ? " " : trimmed; //this ensures a carrot and line break if no text on command line
					output.text = FormatForCLI(outputString);
					cursorOffset = 0;
					UpdateCursor();
				}
			}
			computer.UpdateInfoLine ();
			UpdateScreenNetwork ();
		}

		return valid;
	}


	private bool CLITabKey(){
		if(Input.GetKeyDown(KeyCode.Tab)){
			List<string> matches = new List<string>();
			foreach(string name in lang.commands.Keys){
				if(computer.noUser && !lang.cmdSafeWhileLoggedOut(name)){continue;}
				if(name.IndexOf(rawPromptText) == 0){
					matches.Add(name);
				}
			}
			if(matches.Count == 1){
				rawPromptText = matches[0]+" ";
			}else{
				string memory = rawPromptText;
				matches.Sort();
				rawPromptText = "\t";
				output.text = FormatForCLI(string.Join(" ", matches.ToArray()));
				rawPromptText = memory;
			}
			TypeChar("");

			UpdateScreenNetwork ();

			return true;
		}
		return false;
	}

	private bool ViewArrowKeys(){
		//history up and down
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			if(lineOffset + displayXY[1] < currentEditStringList.Count){
				lineOffset ++;
			}
			TypeChar("");
			return true;
		}
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			if(lineOffset > 0){
				lineOffset --;
			}
			TypeChar("");
			return true;
		}

		return false;
	}

	private bool EditReturnKey(){
		if(Input.GetKeyDown(KeyCode.Return)){
			TypeChar ("\n");
			return true;
		}
		return false;
	}

	private bool EditArrowKeys(){
		//history up and down
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			if(cursorOffsetVertical < displayXY[1] - 1){
				cursorOffsetVertical ++;
			}

			if(
				(lineOffset + displayXY[1] < currentEditStringList.Count)
			&&	(cursorOffsetVertical == displayXY[1] - 1)
			){
				lineOffset ++;
			}
			TypeChar("");
			return true;
		}
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			if(cursorOffsetVertical > 0){
				cursorOffsetVertical --;
			}
			if(
				(lineOffset > 0)
			&&	(cursorOffsetVertical == 0)
			){
				lineOffset --;
			}
			TypeChar("");
			return true;
		}
		//left and right offset
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			if(cursorOffset >= 0){
				cursorOffset --;
				if(cursorOffset < 0){ //wrap line up
					if(cursorOffsetVertical > 0){
						cursorOffsetVertical --;
					}else if(lineOffset > 0){
						lineOffset --;
					}
					cursorOffset = currentEditStringList [lineOffset + cursorOffsetVertical].Length;
				}
			}
			TypeChar ("");
			return true;
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			if(cursorOffset <= displayXY[0]){
				cursorOffset ++;
				int lineEnd = currentEditStringList [lineOffset + cursorOffsetVertical].Length;
				if(cursorOffset > lineEnd){ //wrap line up
					if(cursorOffsetVertical < displayXY[1] - 1){
						cursorOffsetVertical ++;
					}else if(lineOffset < displayXY[1] + currentEditStringList.Count){
						lineOffset ++;
					}
					cursorOffset = 0;
				}
			}
			TypeChar ("");
			return true;
		}
		return false;
	}

	public void EnterEditMode (IGC_File targetFile){
		StartCoroutine (SetupEditMode (targetFile));
		inputMode = (int)inputModes.TextEdit;
	}

	IEnumerator SetupEditMode(IGC_File targetFile){
		while(true){
			yield return new WaitForSeconds(0.1f);
			break;
		} 
		lastEditedFile = targetFile;
		string fname = computer.virtualSystem.fileSystem.ParseURL(targetFile.path, "/").filename;
		prompt.text = "EDITING " + fname + " | ESCAPE TO EXIT";
		rawEditString = targetFile.data;
		output.text = FormatForEdit (rawEditString);
		UpdateCursor ();

		UpdateScreenNetwork ();
	}

	public void ExitEditMode (){
		cursorOffset = 0;
		cursorOffsetVertical = 0;
		lineOffset = 0;
		inputMode = (int)inputModes.CLI;
		prompt.text = "";
		output.text = output.text = FormatForCLI ("save file? type save and hit return.\nyou can still save the file until you edit another.");
		UpdateCursor ();

		SyncLastEdited();
		UpdateScreenNetwork ();
	}

	public void SyncLastEdited ()
	{
		if(computer.virtualSystem.networkReady){
			computer.virtualSystem.GetComponent<NetworkView>().RPC("SyncEditedFileRPC", RPCMode.Others, lastEditedFile.path, rawEditString);
		}
	}

	public string SaveLastEdited ()
	{
		if (lastEditedFile == null){return "nothing to save";}

		if(computer.virtualSystem.networkReady){
			computer.virtualSystem.GetComponent<NetworkView>().RPC("SaveEditedFileRPC", RPCMode.Others);
		}

		return SaveActions ();
	}

	private string SaveActions()
	{
		lastEditedFile.data = rawEditString;
		string output = lastEditedFile.name + " saved";
		
		lastEditedFile = null;
		rawEditString = "";

		return output;
	}

	public void EnterViewMode (string text)
	{
		StartCoroutine (SetupViewMode (text));
		cursorOffset = 0;
		inputMode = (int)inputModes.TextView;
	}
	
	IEnumerator SetupViewMode(string text)
	{
		while(true){
			yield return new WaitForSeconds(0.1f);
			break;
		}
		prompt.text = "arrow up/down to view. press escape or q to exit";
		rawEditString = text;
		output.text = FormatForEdit (rawEditString);
		UpdateCursor ();

		UpdateScreenNetwork ();
	}
	
	public void ExitViewMode ()
	{
		lineOffset = 0;
		inputMode = (int)inputModes.CLI;
		prompt.text = "";
		output.text = FormatForCLI ("");
		UpdateCursor ();

		UpdateScreenNetwork ();
	}

	public bool UpdateScreenNetwork()
	{
		//Debug.Log (Time.frameCount);

		if(computer == null){return false;}

		if(computer.networkReady){
			GetComponent<NetworkView>().RPC (
				"UpdateScreenRPC", 
				RPCMode.Others, 
				prompt.text, 
				output.text, 
				infoLine.text,
				cursorOffset, 
				cursorOffsetVertical, 
				lineOffset, 
				inputMode,
				rawPromptText,
				rawDisplayText
			);
			return true;
		}

		return false;
	}

	[RPC] void UpdateScreenRPC(string promptText, string outputText, string infolineText, int cursoroffset, int cursoroffestvertical, int lineoffset, int inputmode, string rawPrompt, string rawDisplay)
	{
		prompt.text = promptText;
		output.text = outputText;
		infoLine.text = infolineText;
		this.lineOffset = lineoffset;
		this.cursorOffset = cursoroffset;
		this.cursorOffsetVertical = cursoroffestvertical;
		this.inputMode = inputmode;
		this.rawPromptText = rawPrompt;
		this.rawDisplayText = rawDisplay;
		UpdateCursor ();
	}

	[RPC] void SyncEditedFileRPC(string filepath, string data)
	{
		lastEditedFile = computer.virtualSystem.fileSystem.GetFile (filepath);
		rawEditString = data;
	}

	[RPC] void SaveEditedFileRPC()
	{
		SaveActions();
	}
}



/* test text 

string txt = @"Other options
You can pass the following options to the who command (taken from the who command man page):

Just open a  -a, --all         same as -b -d --login -p -r -t -T -u
  -b, --boot        time of last system boot
  -d, --dead        print dead processes
  -H, --heading     print line of column headings
  -l, --login       print system login processes
      --lookup      attempt to canonicalize hostnames via DNS
  -m                only hostname and user associated with stdin
  -p, --process     print active processes spawned by init
  -q, --count       all login names and number of users logged on
  -r, --runlevel    print current runlevel
  -s, --short       print only name, line, and time (default)
  -t, --time        print last system clock change
  -T, -w, --mesg    add user's message status as +, - or ?
  -u, --users       list users logged in
      --message     same as -T
      --writable    same as -T
      --help     display this help and exit
	  --version  output version information and exit

__   __                 _
\ \ / /                | |             
 \ V /___  _   _ _ __  | |     ___   __ 
  \ // _ \| | | | '__| | |    / _ \ / _` |
  | | (_) | |_| | |    | |___| (_) | (_| |
  \_/\___/ \__,_|_|    \_____/\___/ \__, |
                                     __/ |
                                    |___/


TextEditor is a sweet class and gives you a ton of functionality - copying, pasting, moving words, controlling the cursor and selection, and a whole lot more. Unfortunately, there is NO documentation and you have to rely on autocomplete or reflection to figure out what your options are. 
So please, spread the word!";


 */