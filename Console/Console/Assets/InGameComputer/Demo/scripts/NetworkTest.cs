using UnityEngine;
using System.Collections;

public class NetworkTest : MonoBehaviour {

	public string serverIP = "192.168.1.103";
	public int port = 6666;
	private bool isWebPlayer = Application.platform == RuntimePlatform.OSXWebPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer;

	void Start()
	{
		Network.Disconnect();
		Cursor.visible = true;
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10,10,300,200));

		if(Network.peerType == NetworkPeerType.Disconnected){
			if(!isWebPlayer){
				if(GUILayout.Button("start server")){
					InGameComputer.waitForNetwork = true;
					var useNat = !Network.HavePublicAddress();
					Network.InitializeServer (10, port, useNat);
					Application.LoadLevel("demoScene");
				}
			}

			if(!isWebPlayer){
				GUILayout.BeginHorizontal();

				if(GUILayout.Button("connect to server")){
					InGameComputer.waitForNetwork = true;
					Network.Connect(serverIP, port);
					Application.LoadLevel("demoScene");
				}
				serverIP = GUILayout.TextField(serverIP);
				GUILayout.EndHorizontal();
			}

			if(GUILayout.Button("single player")){
				Application.LoadLevel("demoScene");
			}
			if(GUILayout.Button("delete save data")){
				PlayerPrefs.DeleteAll();
			}
			if(GUILayout.Button("visit website")){
				Application.OpenURL("http://ingamecomputer.crltools.org");
			}
		}else{
			GUILayout.Label(Network.peerType.ToString());
		}

		GUILayout.EndArea();
	}
	/*
	void OnConnectedToServer()
	{
		Application.LoadLevel("demoScene");
	}

	void OnServerInitialized()
	{
		Application.LoadLevel("demoScene");
	}*/
}
