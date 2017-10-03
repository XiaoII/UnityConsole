using UnityEngine;
using System.Collections;

public class IGC_TrashBin : MonoBehaviour {

	void Update () {
		foreach(Transform garbageItem in transform){
			Destroy(garbageItem.gameObject);
		}
	}
}
