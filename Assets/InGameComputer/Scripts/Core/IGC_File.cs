using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IGC_File : MonoBehaviour {

	public string fileOwner = "default", data = "";
	public bool protectedFile = false;
	private IGC_VirtualSystem virtualSystem; 
	public bool isDir = false;
	public string[] 
		fileAccessGroups,
		fileEditGroups; 
	public IGC_User owner { get { return virtualSystem.userRegistry.users[fileOwner]; } }
	public List<IGC_UserGroup> 
		accessGroups = new List<IGC_UserGroup>(),
		editGroups = new List<IGC_UserGroup>();

	[HideInInspector] public bool hidden;
	[HideInInspector] public string 
		path,
		ext,
		type;


	void Start () 
	{
		GetPath ();
		type = isDir ? "directory" : "file";
		ext = GetExt ();
		hidden = name [0] == '.' ? true : false;

		Transform up = transform;
		while(true){
			up = up.parent;
			virtualSystem = up.GetComponent<IGC_VirtualSystem>();
			if(virtualSystem != null){break;}
		}

		StartCoroutine ("GetGroupsAndFileOwner");
	}

	private string GetExt()
	{
		string fext = "";
		int start = name.IndexOf (".") + 1;
		if(start > 0){
			fext = name.Substring(start, name.Length - start);
		}else{
			fext = "";
		}

		if(fext == ""){return "NONE";}

		return fext;
	}

	void OnDestroy(){
		//Debug.Log ("FILE: "+this.name+" ("+this.virtualSystem.IP+") @"+Time.time+":"+Time.frameCount);
	}

	private void GetPath () 
	{
		Transform up = transform;

		if(!string.IsNullOrEmpty(path)){return;} //this is so paths can be changed in the cp command

		if(up.name == "fileSystem"){path = "/";}

		while(up.name != "fileSystem"){
			path = "/"+up.name+path;
			up = up.parent;
		}
	}

	private IEnumerator GetGroupsAndFileOwner()
	{
		while(true){
			if(virtualSystem.userRegistry != null){ 
				if(virtualSystem.userRegistry.ready){
					break;
				}
			}
			yield return null;
		}

		IGC_UserRegistry ur = virtualSystem.userRegistry;

		if(fileAccessGroups != null){
			int i=0;
			foreach(string groupName in fileAccessGroups){
				IGC_UserGroup group = ur.GetGroup(groupName);
				if(group != null){
					accessGroups.Add(group);
				}else{
					fileAccessGroups[i] = "GROUP IS NULL. CHECK /groups";
					Debug.LogWarning("group "+groupName+" assigned to IGC_File @ "+virtualSystem.transform.name+" "+path+" does not exist. it was not assigned.");
				}i++;
			}
		}

		if(fileEditGroups != null){
			int i=0;
			foreach(string groupName in fileEditGroups){
				IGC_UserGroup group = ur.GetGroup(groupName);
				if(group != null){
					editGroups.Add(group);
				}else{
					fileEditGroups[i] = "GROUP IS NULL. CHECK /groups";
					Debug.LogWarning("group "+groupName+" assigned to IGC_File @ "+virtualSystem.transform.name+" "+path+" does not exist. it was not assigned.");
				}i++;
			}
		}
	}

	public IGC_UserGroup ApplyAccesGroup(IGC_UserGroup group)
	{
		if(!accessGroups.Contains(group)){
			accessGroups.Add(group);

			if(virtualSystem.networkReady){
				virtualSystem.GetComponent<NetworkView>().RPC ("AddAccessGroupRPC", RPCMode.Others, this.path, group.name);
			}

			return group;
		}
		return null;
	}

	public IGC_UserGroup ApplyEditGroup(IGC_UserGroup group)
	{
		if(!editGroups.Contains(group)){
			editGroups.Add(group);
			
			if(virtualSystem.networkReady){
				virtualSystem.GetComponent<NetworkView>().RPC ("AddEditGroupRPC", RPCMode.Others, this.path, group.name);
			}
			
			return group;
		}
		return null;
	}

	public bool RemoveAccessGroup(IGC_UserGroup group){
		if(accessGroups.Contains(group)){
			accessGroups.Remove(group);

			if(virtualSystem.networkReady){
				virtualSystem.GetComponent<NetworkView>().RPC ("RemoveAccessGroupRPC", RPCMode.Others, this.path, group.name);
			}

			return true;
		}
		return false;
	}

	public bool RemoveEditGroup(IGC_UserGroup group)
	{
		if(editGroups.Contains(group)){
			editGroups.Remove(group);
			
			if(virtualSystem.networkReady){
				virtualSystem.GetComponent<NetworkView>().RPC ("RemoveEditGroupRPC", RPCMode.Others, this.path, group.name);
			}
			
			return true;
		}
		return false;
	}

	public void Protect(bool tf)
	{
		if(virtualSystem.networkReady){
			virtualSystem.GetComponent<NetworkView>().RPC ("ProtectFileRPC", RPCMode.Others, this.path, tf);
		}

		this.protectedFile = tf;
	}

	public string[] ListAccessGroups()
	{
		string[] output = new string[accessGroups.Count];
		
		int i = 0;
		foreach(IGC_UserGroup group in accessGroups){
			output[i++] = group.name; 
		}
		return output;
	}

	public string[] ListEditGroups()
	{
		string[] output = new string[editGroups.Count];
		
		int i = 0;
		foreach(IGC_UserGroup group in editGroups){
			output[i++] = group.name; 
		}
		return output;
	}
}
