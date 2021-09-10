using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_PlayerAnimation : MonoBehaviour {

	[Header("References")]
	public BPDemo_PlayerController playerController;
	public Animator animator;

	public void Update()
	{
		animator.SetFloat("HorizMovement", playerController.inputVector.x);
	}
}