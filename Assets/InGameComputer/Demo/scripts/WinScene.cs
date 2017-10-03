using UnityEngine;
using System.Collections;

public class WinScene : MonoBehaviour {

	void Start () {
		PlayerPrefs.DeleteAll();
		StartCoroutine ("StartOver");
	}

	private IEnumerator StartOver()
	{
		while(true){
			yield return new WaitForSeconds(2f);
			Application.LoadLevel("start");
			break;
		}
	}
}
