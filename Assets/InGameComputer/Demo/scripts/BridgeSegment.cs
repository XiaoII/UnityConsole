using UnityEngine;
using System.Collections;

public class BridgeSegment : MonoBehaviour {

	[HideInInspector] private Vector3 targetPosition;

	void Start () 
	{
		targetPosition = transform.localPosition;
	}

	public void SetTargetPosition(Vector3 tp)
	{
		if(Network.peerType != NetworkPeerType.Disconnected)
		{
			GetComponent<NetworkView>().RPC("SetTargetPositionRPC", RPCMode.Others, tp);
		}
		targetPosition = tp;
	}

	void Update () 
	{
		if(Vector3.Distance(transform.localPosition, targetPosition) > .001)
		{
			transform.localPosition = Vector3.Lerp (transform.localPosition, targetPosition, 0.5f);
		}
	}

	[RPC] private void SetTargetPositionRPC(Vector3 tp)
	{
		targetPosition = tp;
	}
}
