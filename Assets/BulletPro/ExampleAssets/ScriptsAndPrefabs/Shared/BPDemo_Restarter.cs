using System.Collections;
using System.Collections.Generic;
using BulletPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_Restarter : MonoBehaviour {

	public UnityEvent OnRestarted;

	[Header("References")]
	public BPDemo_KillableCharacter playerScript;
	public BPDemo_KillableCharacter enemyScript;
	
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			// I'd rather have the scene reloaded, but chances are this scene won't be in your build settings :

			if (playerScript) playerScript.Respawn();
			if (enemyScript) enemyScript.Respawn();

			if (OnRestarted != null) OnRestarted.Invoke();
		}
	}
}
