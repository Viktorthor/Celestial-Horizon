using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BPDemo_TargetFramerateSetter : MonoBehaviour {

	public int targetFramerate = 60;

	void Awake ()
	{
		Application.targetFrameRate = targetFramerate;	
	}
}
