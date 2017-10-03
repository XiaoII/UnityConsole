using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {

	private Player player;
	private bool menu = false;

	void Start()
	{
		Cursor.visible = false;
		player = GameObject.Find("player").GetComponent<Player>();
	}

	void OnGUI () 
	{
		if(Input.GetKeyDown(KeyCode.Tab))
		{
			menu = true;
			player.pauseLooking = true;
			Cursor.visible = true;
		}

		if(Input.GetKeyUp(KeyCode.Tab))
		{
			menu = false;
			player.pauseLooking = false;
			Cursor.visible = false;
		}

		GUILayout.BeginArea (new Rect (10, 10, 200, 200));
			if(menu){
				Cursor.visible = true;
				if(GUILayout.Button("exit to start menu")){
					Application.LoadLevel("start");
				}
			}else{
				GUILayout.Label("hold tab for menu");
			}
		GUILayout.EndArea ();
	}
}
