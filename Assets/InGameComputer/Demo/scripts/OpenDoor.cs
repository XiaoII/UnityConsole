using UnityEngine;
using System.Collections;

public class OpenDoor : IGC_Command {
	
	public OpenDoor(ref IGC_VirtualSystem virtualSystem)
	{
		this.virtualSystem = virtualSystem;
		this.name = "opendoor";
		this.help_text = "this command will open a door."; 
	}
	
	public override string command_function ()
	{
		GameObject[] doors = GameObject.FindGameObjectsWithTag ("door");

		foreach(GameObject go in doors){
			Door door = go.GetComponent<Door>();

			if(door.masterSystem == virtualSystem){
				door.Open();
			}
		}

		return "";
	}
}
