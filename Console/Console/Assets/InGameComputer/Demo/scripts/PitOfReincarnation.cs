using UnityEngine;
using System.Collections;

public class PitOfReincarnation : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if(other.name == "player")
		{
			other.GetComponent<Player>().ReturnToStart();
		}
	}
}
