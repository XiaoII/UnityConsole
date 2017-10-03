using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public Material crosshairActive, crosshairInactive;
	public float 
		moveSpeed = 8,
		lookSpeed = 2,
		reach = 10;

	[HideInInspector] public bool 
		pauseMovement = false,
		pauseLooking = false;

	private Transform cam, crosshair;
	private InGameComputer currentComp;
	private Vector3 spawnpoint;
	private Quaternion spawnRotation;
	private bool actionKeyPressed = false;


	void Start()
	{
		spawnpoint = transform.position;
		spawnRotation = transform.rotation;
		cam = transform.Find ("camera");
		crosshair = transform.Find ("camera/crosshair");

		crosshair.GetComponent<Renderer>().material = crosshairInactive;
	}

	void Update () 
	{
		//looking
		if(!pauseLooking)
		{
			transform.Rotate (0, Input.GetAxis ("Mouse X") * lookSpeed, 0);
			cam.Rotate (-Input.GetAxis ("Mouse Y") * lookSpeed, 0, 0);
		}

		//walking
		float 
			damping = currentComp != null 
				? Vector3.Distance (currentComp.transform.position, transform.position) / (reach * 1.1f)
				: 1,
			speed = damping * damping * damping * moveSpeed;

		if(Input.GetKey(KeyCode.W)){transform.Translate(0,0,Time.deltaTime * speed);}
		if(Input.GetKey(KeyCode.S)){transform.Translate(0,0,-Time.deltaTime * speed);}
		if(Input.GetKey(KeyCode.A)){transform.Translate(-Time.deltaTime * speed,0,0);}
		if(Input.GetKey(KeyCode.D)){transform.Translate(Time.deltaTime * speed,0,0);}

		//interaction
		actionKeyPressed = Input.GetKeyDown (KeyCode.E);

		if(crosshair.GetComponent<Renderer>().material != crosshairInactive)
		{
			crosshair.GetComponent<Renderer>().material = crosshairInactive;
		}

		RaycastHit hit;
		if(Physics.Raycast(cam.position, cam.forward, out hit, reach, ~(1<<8) ))
		{
			if(hit.transform.gameObject.layer == 9)
			{
				if(crosshair.GetComponent<Renderer>().material != crosshairActive)
				{
					crosshair.GetComponent<Renderer>().material = crosshairActive;
				}

				ButtonActions(hit);
			}
		}

		ComputerActions(hit);
	}
	
	private void ComputerActions(RaycastHit hit)
	{
		if(hit.transform == null) 
		{
			if(currentComp != null)
			{
				currentComp.LeaveComputer();
				currentComp = null;
			}
			pauseMovement = false;
			return;
		}

		if(hit.transform.tag == "computer" && currentComp == null)
		{
			InGameComputer comp = hit.transform.GetComponent<InGameComputer>();

			if(comp.powerState && !comp.bootingUp){
				currentComp = comp;
				pauseMovement = true;
				currentComp.UseComputer();
			}

		}
		else if(hit.transform.tag != "computer" && currentComp != null)
		{
			pauseMovement = false;
			currentComp.LeaveComputer();
			currentComp = null;
		}
	}
	
	private void ButtonActions(RaycastHit hit)
	{
		if(hit.transform.tag == "powerButton")
		{
			if(actionKeyPressed)
			{
				hit.transform.GetComponent<PowerButton>().Power();
			}
		}
	}

	public void ReturnToStart()
	{
		transform.position = spawnpoint;
		transform.rotation = spawnRotation;
	}

	public void WinGame()
	{
		GetComponent<NetworkView>().RPC("WinGameRPC", RPCMode.Others);
		Application.LoadLevel ("winScene");
	}

	[RPC] public void WinGameRPC()
	{
		Application.LoadLevel ("winScene");
	}
}
