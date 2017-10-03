For tutorials and more detailed information on the use of the In Game Computer, visit http://ingamecomputer.crltools.org

Contact us at crltools@gmail.com



Contents:

	(1) > Setting up a ComputerSystem Prefab
	(2) > Creating a Custom Command
	(3) > Creating a Custom Language
	(4) > Editor Vars
	(5) > File Setup
	(6) > Setting Up the Demo Scene
	(7) > Integrating With Your Game


(1) Setting up a ComputerSystem Prefab

	1- Navigate to the IGC/Prefab folder and drag the "ComputerSystem" prefab onto the scene or the heirarchy window. 
	2- Configure it to your liking (see the editor vars and File Setup sections below)



(2) Creating a Custom Command

	1- create a new c# script. 
	2- open the IGC_CommandTemplate file in IGC/scripts, copy the contents, then paste the contents into your new c# file. Make sure the class name and the constructor name are that of your script.
	3- define what your script does in the command_function function.
	4- then...



(3) Creating a Custom Language

	1- create a new c# script. 
	2- open the IGC_LanguageTemplate file in IGC/scripts, copy the contents, then paste the contents into your new c# file. Make sure the class name is that of your script.
	3- add your custom command(s) to this language's commands List<string, IGC_Command>.
	4- add your new language as a component to the game object of the computer system you want to use this language.



(4) Editor Vars

	Note that all the editor variables (except for the IGC_Shell variables) are ignored 
	if the IGC instance is loading from saved data. 

	InGameComputer

		Login Name
			If you supply a user name here, the computer system will log this user in immediately after initialization. Note that it MUST be a valid username and "Start In On State" must be checked. If the computer starts in the off state, or the username is not valid, no one will be logged in when the game starts.
		
		Login Pass
			The is the password for the user you wish to have logged in at game start.
		
		Start In On State
			If this is checked, the computer will start in the on state, if not, it will shutdown immediately after initialization (usually in the first or second frame. it's not noticable at load).
		
		Start In Use
			If this is checked, the computer will accept input as soon as it finishes initialization. Make sure you want this before you use this feature. If you have more than one instance of the IGC this is not recomended.
		
		Screen Color
			The color of the screen will be set to this the first time the computer system loads.
		
		Text Color
			The color of the text will be set to this the first time the computer system loads.
		

	IGC_VirtualSystem

		Erase Save Data On Start
			If this is checked, all save-data for this computer system will be erased on startup and it will load based on its configuration in the editor
		

		Model Name
			The fictional name of this computer system's "model."
		
		IP
			If this is left blank, a random ip will be generated. IMPORTANT: if you do not specify an IP address, the computer system will not save its state. Only use random IP's if you want to computer system to totally reset every time the scene is loaded.
		

	IGC_UserRegistry

		Default Username
			This will be the name of the default user for this computer system.
		
		Default User Password
			the password for the default user
		
		Root User Password
			The password for the root user
		
		Users List
			The users list is how you create additional users at startup. Enter the number of additional users you'd like to add in the Size field, then for each user, enter the following information in EXACTLY the following format.
			username:password:isAdmin?:loggedInRemotely?:terminalID:currentWorkingDirectory:loggedIn?
			Be sure to capitolize the first letter of True/False values. loggedInRemotely and loggedIn should always be false. terminalID should always be -1. This is the format of the save string, it has some stuff that is not used here but must be defined for initialization and sending state strings over the network.
			Example:
			coolperson:ilovecoolstuff:True:False:-1:/home/coolperson:False
		
		Groups List
			The groups list is how you create groups at startup. Enter the number of additional users you'd like to add in the Size field, then for each group, enter the following information in EXACTLY the following format.
			groupname:groupCreator:username~username~...:adminName~adminName~...

			for user and admin names, be sure to add a "~" after each--even the last one. admins are listed BOTH in the users list AND the admins list.
			Example:
			coolgroup:coolperson:root~coolperson~:coolperson~
		

	IGC_Shell

		Display Width
			The number of characters the terminal fits horizontally.
		
		Display Height
			The maximum number of lines of text the terminal will show.
		
		Line Height Modifier
			This is to set the spacing of the cursor, it does not affect the actual line height of the font. Use the font's settings for that.
		
		Char Width Modifier
			This only affects the distance the cursor moves when you type or backspace.
	


(5) File Setup

	To setup a file system that the computer will create during its initialization process, we'll need to drill into the computer system heirarchy.

	The file system is mirrored by a tree of actual game objects. This enables you to set up an environment exactly the way you want it and also monitor changes to the file system during gameplay in the editor.

	You'll notice that several files already exist. The home folder, the etc folder and its contents are the only required files--don't delete them. It's important to note that the "fileSystem" game object atually represents the root folder (/) of the file system.

	To create a file, make a new, empty game object and place it where you want it in the file system. Then Add an IGC_File component to the new game object and configure it to your liking.

	Each file MUST have an IGC_File component. Let's take a look at its editor variables.

		File Owner
			This is required. Enter a valid username.
		
		Data
			The textual data contained in this file
		
		Protected File
			Protect this file?
		
		Is Dir
			Is this file a directory?
		
		File Access Groups
			For each item in this array, if it matches the name of an existing group, that group will be assigned as an access group for this file. Users who do not belong to one of this file's access groups can not read or write to this file--or if it's a directory, they can't enter it.
		
		File Edit Groups
			For each item in this array, if it matches the name of an existing group, that group will be assigned as an edit group for this file. Users who do not belong to one of this file's edit groups cannot write to this file.



(6) Setting Up the Demo Scene

	Ensure that you have added all three scenes (start, demoScene and winScene) to the build settings of your project.

	If you plan to test the multiplayer features, ensure that you check "Run In Background" in the "Resolution and Presentation" section of the player prefs (Edit -> Project Settings -> Player)



(7) Integrating With Your Game

	There are two ways to have an IGC instance accept input. Either check the "In Use" boolean in the editor variables of the InGameComputer component, or use the UseComputer() and LeaveComputer() methods in the InGameComputer class.

	The former is recomended only for games or scenes that will never change perspective and in which only input into the IGC instance is expected.

	The latter is the recommended method. Incorporate the UseComputer() and LeaveComputer() methods into your player script and be sure to pause all other input listening while using the IGC instance (or you might walk around or trigger player actions while simply typing into the IGC terminal).

	See the Player script in IGC/Demo/scripts for reference.