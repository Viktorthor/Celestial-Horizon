using System.Collections;
using System.Collections.Generic;
using BulletPro;
using UnityEngine;
using UnityEngine.UI;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_SampleSceneManager : MonoBehaviour {

	public static BPDemo_SampleSceneManager instance;

	[Header("References")]
	public BPDemo_KillableCharacter playerScript;
	public BPDemo_KillableCharacter enemyScript;
	public Text enemyShotName, playerShotName;

	[Header("Initial Config")]
	public int defaultEnemyShot = 0;
	public int defaultPlayerShot = 0;

	[Header("Sample Patterns")]
	public EmitterProfile[] enemyShots;
	public EmitterProfile[] playerShots;
	int currentEnemyShot, currentPlayerShot;

	void Start ()
	{
		if (!instance) instance = this;
		instance = this;

		currentEnemyShot = defaultEnemyShot;
		currentPlayerShot = defaultPlayerShot;

		if (currentEnemyShot >= enemyShots.Length) currentEnemyShot = 0;
		if (currentEnemyShot < 0) currentEnemyShot = enemyShots.Length-1;
		if (currentPlayerShot >= playerShots.Length) currentPlayerShot = 0;
		if (currentPlayerShot < 0) currentPlayerShot = playerShots.Length-1;

		RefreshEnemyShot();
		RefreshPlayerShot();
	}
	
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			// I'd rather have the scene reloaded, but chances are this scene won't be in your build settings :

			if (playerScript) playerScript.Respawn();
			if (enemyScript) enemyScript.Respawn();
		}

		if (Input.GetKeyDown(KeyCode.X)) NextEnemyShot();
		if (Input.GetKeyDown(KeyCode.C)) NextPlayerShot();
	}

	public void NextEnemyShot()
	{
		currentEnemyShot++;
		if (currentEnemyShot >= enemyShots.Length) currentEnemyShot = 0;
		RefreshEnemyShot();
	}

	public void PrevEnemyShot()
	{
		currentEnemyShot--;
		if (currentEnemyShot < 0) currentEnemyShot = enemyShots.Length-1;
		RefreshEnemyShot();
	}

	public void NextPlayerShot()
	{
		currentPlayerShot++;
		if (currentPlayerShot >= playerShots.Length) currentPlayerShot = 0;
		RefreshPlayerShot();
	}

	public void PrevPlayerShot()
	{
		currentPlayerShot--;
		if (currentPlayerShot < 0) currentPlayerShot = playerShots.Length-1;
		RefreshPlayerShot();
	}

	void RefreshEnemyShot()
	{
		for (int i = 0; i < enemyScript.bulletEmitters.Length; i++)
		{
			enemyScript.bulletEmitters[i].Kill();
			enemyScript.bulletEmitters[i].emitterProfile = enemyShots[currentEnemyShot];
			enemyScript.bulletEmitters[i].Play();
		}
		
		enemyShotName.text = enemyShots[currentEnemyShot].name;
		if (enemyScript) enemyScript.Respawn();
	}

	void RefreshPlayerShot()
	{
		for (int i = 0; i < playerScript.bulletEmitters.Length; i++)
		{
			playerScript.bulletEmitters[i].Kill();
		}
		
		playerScript.bulletEmitters[0].emitterProfile = playerShots[currentPlayerShot];

		playerShotName.text = playerShots[currentPlayerShot].name;
		//playerScript.shootScript.Play();
	}
}
