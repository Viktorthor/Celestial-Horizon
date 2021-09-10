using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_PlayerController : MonoBehaviour {

	[Header("Stats")]
	public float moveSpeed;

	//[Header("References to self")]
	[System.NonSerialized] public Transform self;
	[System.NonSerialized] public BulletEmitter shootScript;
	[System.NonSerialized] public BPDemo_KillableCharacter healthScript;

	[Header("Key Binding")]
	public KeyCode shotButton = KeyCode.LeftShift;
	public KeyCode left = KeyCode.LeftArrow;
	public KeyCode right = KeyCode.RightArrow;
	public KeyCode up = KeyCode.UpArrow;
	public KeyCode down = KeyCode.DownArrow;

	[Header("References")]
	public Transform leftWall;
	public Transform rightWall, upperWall, lowerWall;

	[System.NonSerialized]
	public Vector3 inputVector;

	// Get component references
	public virtual void Awake()
	{
		self = transform;
		shootScript = GetComponent<BulletEmitter>();
		healthScript = GetComponent<BPDemo_KillableCharacter>();
	}

	// Controls
	public virtual void Update()
	{
		if (!healthScript.isAlive) return;

		inputVector = Vector3.zero;

		if (Input.GetKey(left))		inputVector.x--;
		if (Input.GetKey(right))	inputVector.x++;
		if (Input.GetKey(down))		inputVector.y--;
		if (Input.GetKey(up))		inputVector.y++;

		if (inputVector.x < 0 && self.position.x-0.5f < leftWall.position.x) inputVector.x = 0;
		if (inputVector.x > 0 && self.position.x+0.5f > rightWall.position.x) inputVector.x = 0;
		if (inputVector.y < 0 && self.position.y-0.5f < lowerWall.position.y) inputVector.y = 0;
		if (inputVector.y > 0 && self.position.y+0.5f > upperWall.position.y) inputVector.y = 0;

		self.Translate(inputVector.normalized * moveSpeed * Time.deltaTime, Space.Self);

		if (Input.GetKeyDown(shotButton))	shootScript.Play();
		if (Input.GetKeyUp(shotButton))		shootScript.Stop(PlayOptions.RootAndSubEmitters);
	}
}
