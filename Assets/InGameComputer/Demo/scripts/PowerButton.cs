using UnityEngine;
using System.Collections;

public class PowerButton : MonoBehaviour {

	public InGameComputer comp;
	public Material on, off;

	private Transform circle, bar;

	void Start()
	{
		circle = transform.Find ("circle");
		bar = transform.Find ("bar");

		if(!comp.startInOnState)
		{
			ChangeMat(off);
		}
	}

	public void Power()
	{
		if(comp.powerState)
		{
			ChangeMat(off);
			comp.PowerOff();
		}
		else
		{
			ChangeMat(on);
			comp.PowerOn();
		}
	}

	private void ChangeMat(Material mat)
	{
		bar.GetComponent<Renderer>().material = mat;
		circle.GetComponent<Renderer>().material = mat;
	}
}
