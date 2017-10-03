using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IGC_FileSystem : MonoBehaviour{

	public Dictionary<string, IGC_File> files = new Dictionary<string, IGC_File>();

	[HideInInspector] public IGC_VirtualSystem virtualSystem;
	[HideInInspector] public bool ready = false;
	[HideInInspector] public Transform rootNode;
	
	private Transform trashBin;

	
	void Start()
	{
		virtualSystem = gameObject.GetComponent<IGC_VirtualSystem>();

		trashBin = transform.Find ("TrashBin");
		rootNode = transform.Find ("fileSystem");
	} 

	public void Initialize(){
		if(Network.peerType == NetworkPeerType.Client){
			//sync ip and state with server instance
			StartCoroutine("WaitForVSUniqueID");
		}else if(virtualSystem.HasSaveData()){
			virtualSystem.restoreData = virtualSystem.GetSaveString();
			BuildFilesFromFileString();
		}else{
			files.Add ("/", rootNode.GetComponent<IGC_File>()); 
			GetFilesRecursive (rootNode);
			ready = true;
			virtualSystem.OnFilesReady();
		}
	}

	IEnumerator WaitForVSUniqueID()
	{
		while(IGC_VirtualSystem.uniqueID == string.Empty){
			//Debug.Log("waiting...");
			yield return null;
		}

		if(GetComponent<NetworkView>() != null && Network.peerType != NetworkPeerType.Server){
			//Debug.Log("i sent the request to the server");
			GetComponent<NetworkView>().RPC("RequestStateFromServer", RPCMode.Server, IGC_VirtualSystem.uniqueID);
		}
	}


	public void BuildFilesFromFileString()
	{
		string filesString = virtualSystem.GetFilesString(virtualSystem.restoreData);

		if(filesString == "NONE"){return;}

		Transform[] children = new Transform[transform.childCount]; //if you loop through a transform and change children's parents it alters the transform list b4 next loop

		int c=0;
		foreach(Transform child in rootNode){//build child list
			children[c++] = child;
		}

		foreach(Transform child in children){//edit it
			if(child != null){
				child.parent = trashBin;
			}
		}

		string[] filesStringList = IGC_Utils.SplitString("\n", filesString);

	//foreach file in filestring
		for(int i=0; i<filesStringList.Length; i++){

			string[] file = IGC_Utils.SplitString(":",  filesStringList[i]);
			IGC_URL url = ParseURL(file[0], "/");
			string dataString = file[6] != "NONE" 
				? IGC_Utils.UnescapeSaved(file[6])
				: "";
			bool 
				prot = file[2] == "True" ? true : false,
				isdir = file[3] == "True" ? true : false;

			if(url.filename == ""){//root
				var rootFile = rootNode.GetComponent<IGC_File>();
				files.Add("/", rootFile);
				rootFile.fileOwner = file[1]; 
			}else{
				GameObject go = new GameObject(url.filename);
				go.transform.parent = rootNode.Find(url.dirpath.Remove(0,1)).transform;
				IGC_File fileComp = go.AddComponent<IGC_File>();
				fileComp.isDir = isdir;
				fileComp.protectedFile = prot;
				fileComp.fileOwner = file[1]; 
				fileComp.data = dataString;

				files.Add(url.fullpath, fileComp);
			}
		}

		ready = true;
		virtualSystem.OnFilesReady();
	}

	public void ApplyFileGroups()
	{
		string filesString = virtualSystem.GetFilesString(virtualSystem.restoreData);
		string[] filesStringList = IGC_Utils.SplitString("\n", filesString);

		for(int i=0; i<filesStringList.Length; i++){

			string[] 
				fileString = IGC_Utils.SplitString(":",  filesStringList[i]),
				groups = IGC_Utils.SplitString("~", fileString[7]),
				editgroups = IGC_Utils.SplitString("~", fileString[8]);

			IGC_File file = GetFile(fileString[0]);

			if(file == null){continue;} //if there are no groups of the file ! exist, go to next

			if(fileString[7] != "NONE"){
				foreach(string groupname in groups){
					file.accessGroups.Add(virtualSystem.userRegistry.groups[groupname]);
				}
			}

			if(fileString[8] != "NONE"){
				foreach(string editgroupname in editgroups){
					file.editGroups.Add(virtualSystem.userRegistry.groups[editgroupname]);
				}
			}
		}
	}

	public void GetFilesRecursive(Transform target)
	{
		foreach(Transform child in target){
			IGC_File f = child.GetComponent<IGC_File> ();
			if(f != null){
				files.Add (f.path, f);
			}
			GetFilesRecursive(child);
		}
	}

	public IGC_File GetFile(string path)
	{
		if(files.ContainsKey(path)){
			return files[path];
		}
		return null;
	}

	public bool FileExists(string path)
	{
		if(files.ContainsKey(path)){
			return true;
		}
		return false;
	}

	public string[] PathDirList(string path)
	{
		return path.Split (new string[1]{"/"}, System.StringSplitOptions.RemoveEmptyEntries);
	}

	public IGC_URL ParseURL(string path, string cwd)
	{
		var cwdList = PathDirList (cwd);

		path = IGC_Utils.EscapeURL (path);

		while(path[path.Length-1] == '/' && path.Length > 1){path = path.Remove(path.Length-1, 1);}//strip end slash

		if(path.IndexOf("./") == 0){
			path = cwd + path.Remove(0,1); //replace ./ with cwd
		}else if(path.IndexOf("..") == 0){ //going up dirs
			if(cwdList.Length == 0){return null;}

			string[] dirs = PathDirList(path);
			int ups = 0;
			for(int i=0; i<dirs.Length; i++){if(dirs[i] == ".."){ups ++;dirs[i] = "|)r(|";}else{break;} } //count the ups
			if(ups > cwdList.Length){return null;}
			string[] tempList = cwdList;
			for(int a=0; a<ups; a++){
				tempList[tempList.Length-a-1] = "|)r(|";
			}

			path = "/"+string.Join("/",tempList)+"/"+string.Join("/",dirs);
			path = path.Replace("/|)r(|", "");
			if(path.IndexOf("../") != -1){return null;}
			if(path == ""){path = "/";}
		}

		else if(path.IndexOf("/") != 0){path = (cwd == "/" ? "" : cwd )+ "/" + path;} //if just a name, prepend cwd/

		string[] dirList = PathDirList (path);
		string 
			dirpath = "/", 
			filename = dirList.Length > 0 ? dirList[dirList.Length - 1] : "",
			dirname = dirList.Length > 1 ? dirList[dirList.Length - 2] : "/";

		for(int d=0; d<dirList.Length-1; d++){
			dirpath += dirList[d]+(dirList.Length-2 == d ? "" : "/");
		}
		//Debug.Log (path);
		return new IGC_URL(filename, dirname, dirpath, path, cwd);
	}

	public bool IsDir(string path)
	{
		return GetFile (path).isDir;
	}

	public IGC_File CreateFile(IGC_URL url, IGC_User user, bool isDir)
	{
		if(FileExists(url.fullpath)){return null;}

		IGC_File targetDir = GetFile (url.dirpath);

		if(targetDir != null && targetDir.isDir){
			if(!CanAccessFile(targetDir, user)){return null;}

			IGC_File file = CreateFileGameObject(url, user.name, isDir);

			if(virtualSystem.networkReady){
				GetComponent<NetworkView>().RPC ("CreateFileRPC", RPCMode.Others, url.fullpath, user.name, isDir);
			}

			return file;
		}else{
			return null;
		}
	}

	private IGC_File CreateFileGameObject (IGC_URL url, string username, bool isDir)
	{
		GameObject go = new GameObject(url.filename);
		go.transform.parent = url.dirpath == "/" 
			? rootNode.transform 
			: GetFile(url.dirpath).transform;
		
		IGC_File file = go.AddComponent<IGC_File>();
		file.fileOwner = username;
		file.isDir = isDir;
		files.Add(url.fullpath, file);

		return file;
	}

	public bool RMFile(IGC_URL url, IGC_User user)
	{
		if(FileExists(url.fullpath)){
			IGC_File file = GetFile(url.fullpath);
			if(!CanAccessFile(file, user)){
				return false;
			}else{

				RemoveFileActions(file);

				if(virtualSystem.networkReady){
					GetComponent<NetworkView>().RPC ("RemoveFileRPC", RPCMode.Others, url.fullpath);
				}

				return true;
			}
		}else{
			return false;
		}
	}

	private void RemoveFileActions(IGC_File file)
	{
		string fileIndex = file.path;
		RemoveFolderChildren(file.transform);
		Destroy(file.gameObject);
		files.Remove(fileIndex);
	}

	private void RemoveFolderChildren(Transform dir)
	{
		foreach(Transform child in dir){
			if(child.childCount > 0){ RemoveFolderChildren(child); }
			this.files.Remove(child.GetComponent<IGC_File>().path);
		}
	}
	public void MoveFile(IGC_URL oldURL, IGC_URL newURL)
	{
		IGC_File file = GetFile (oldURL.fullpath);

		files.Remove (oldURL.fullpath);
		
		file.path = newURL.fullpath;
		file.transform.parent = GetFile (newURL.dirpath).transform;
		file.gameObject.name = newURL.filename;
		
		files.Add (newURL.fullpath, file);
	}

	public void CopyFile(IGC_URL target, IGC_URL copy)
	{
		IGC_File file = GetFile (target.fullpath);
		IGC_File dir = GetFile (copy.dirpath);
		IGC_File newFile = (GameObject.Instantiate (file.gameObject) as GameObject).GetComponent<IGC_File>();
		
		newFile.gameObject.name = copy.filename;
		newFile.path = copy.fullpath;
		newFile.transform.parent = dir.transform;
		newFile.accessGroups = file.accessGroups;
		newFile.editGroups = file.editGroups; 
		
		files.Add (copy.fullpath, newFile);
	}

	public IGC_URL[] ListFiles(IGC_URL url, string cwd, bool showHidden)
	{
		Transform dir = GetFile(url.fullpath).transform;

		List<IGC_URL> list = new List<IGC_URL>();

		foreach(Transform file in dir){
			if(showHidden || file.name[0] != '.'){
				list.Add(ParseURL(file.GetComponent<IGC_File>().path, cwd));
			}
		}

		return list.ToArray();
	}

	public bool CanEditFile(IGC_File file, IGC_User user)
	{
		if(user.isAdmin){
			return true;
		}else{
			if(file.protectedFile){
				return false;
			}
		}
		if(file.editGroups.Count == 0){
			if(file.accessGroups.Count == 0){
				return true;
			}
			foreach(IGC_UserGroup accessGroup in file.accessGroups){
				if(user.groups.Contains(accessGroup)){
					return true;
				}
			}
		}

		foreach(IGC_UserGroup editGroup in file.editGroups){
			if(user.groups.Contains(editGroup)){
				return true;
			}
		}
		return false;
	}

	public bool CanAccessFile(IGC_File file, IGC_User user)
	{
		if(user.isAdmin){ 
			//Debug.Log("admins can access any file. ");
			return true;
		}else{
			if(file.protectedFile){ 
				//Debug.Log("not admin? if protected, no. ");
				return false;
			}else{ 
				if(file.accessGroups.Count == 0){ 
					//Debug.Log("if not protected. if you own it, yes. ");
					return true;
				}else{ 
					if(file.accessGroups.Count > 0){ 
						foreach(IGC_UserGroup ag in file.accessGroups){
							if(user.groups.Contains(ag)){
								//Debug.Log("if not... if you belong to one of its access groups, yes");
								return true;
							}
						}
					}
				}
			}
		}
		//Debug.Log("FINAL NO");
		return false; 
	}

	[RPC] void CreateFileRPC(string filepath, string username, bool isDir)
	{
		IGC_URL url = ParseURL(filepath, "/");
		CreateFileGameObject (url, username, isDir);
	}

	[RPC] void RemoveFileRPC(string filepath)
	{
		IGC_File file = GetFile (filepath);
		RemoveFileActions (file);
	}
	//below needs checking
	[RPC] void AddAccessGroupRPC(string filepath, string groupname)
	{
		IGC_File file = GetFile (filepath);
		file.accessGroups.Add(virtualSystem.userRegistry.groups [groupname]); 
	}
	
	[RPC] void RemoveEditGroupRPC(string filepath, string groupname)
	{
		IGC_File file = GetFile (filepath);
		file.editGroups.Remove (virtualSystem.userRegistry.groups [groupname]);
	}

	[RPC] void AddEditGroupRPC(string filepath, string groupname)
	{
		IGC_File file = GetFile (filepath);
		file.editGroups.Add(virtualSystem.userRegistry.groups [groupname]); 
	}
	
	[RPC] void RemoveAccessGroupRPC(string filepath, string groupname)
	{
		IGC_File file = GetFile (filepath);
		file.accessGroups.Remove (virtualSystem.userRegistry.groups [groupname]);
	}

	[RPC] void ProtectFileRPC(string filepath, bool status)
	{
		IGC_File file = GetFile (filepath);
		file.protectedFile = status;
	}

	[RPC] void MoveFileRPC(string oldPath, string newPath) 
	{
		MoveFile (ParseURL (oldPath, "/"), ParseURL (newPath, "/")); 
	}

	[RPC] void CopyFileRPC(string targetPath, string copyPath) 
	{
		CopyFile (ParseURL (targetPath, "/"), ParseURL (copyPath, "/")); 
	}
}