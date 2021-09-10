using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_ExplodeOnHit : MonoBehaviour {

	public ParticleSystem explosionFX;
	float timestamp;
	public float explosionDuration = 0.2f;

	public void Awake()
	{
		ParticleSystem.EmissionModule em = explosionFX.emission;
		em.enabled = false;
		timestamp = -10f;
		explosionFX.Play();
	}

	void Update()
	{
		if (Time.time - timestamp > explosionDuration)
		{
			ParticleSystem.EmissionModule em = explosionFX.emission;
			em.enabled = false;
			enabled = false;
		}
	}

	public void LaunchExplosion(Bullet bullet, Vector3 position)
	{
		explosionFX.transform.position = position;
		ParticleSystem.EmissionModule em = explosionFX.emission;
		em.enabled = true;
		timestamp = Time.time;
		enabled = true;
	}
}
