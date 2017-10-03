using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	public float animationDuration = 1;
	public IGC_VirtualSystem masterSystem;
	private float animationTime, animationRate = 0.03f;

	public void Open()
	{
		if(Network.peerType != NetworkPeerType.Disconnected)
		{
			GetComponent<NetworkView>().RPC("OpenRPC", RPCMode.Others);
		}
		StartCoroutine ("Animate"); 
	}

	private IEnumerator Animate () 
	{
		animationTime = 0;

		while(true)
		{
			animationTime += animationRate;

			//float percent = animationTime / animationDuration;

			transform.position = new Vector3(
				transform.position.x + (transform.localScale.z * animationRate),
				transform.position.y,
				transform.position.z
			);
			
			if(animationTime >= animationDuration){break;}

			yield return new WaitForSeconds(animationRate);
		}
	}

	[RPC] private void OpenRPC()
	{
		StartCoroutine ("Animate");
	}
}
