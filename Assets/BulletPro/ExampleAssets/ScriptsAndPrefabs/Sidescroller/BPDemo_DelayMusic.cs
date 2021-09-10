using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_DelayMusic : MonoBehaviour {

	public AudioSource source;
	public float delay;

	void Start ()
	{
		StartCoroutine(PlayDelayed());	
	}

	IEnumerator PlayDelayed()
	{
		yield return new WaitForSeconds(delay);
		source.Play();
	}
}
