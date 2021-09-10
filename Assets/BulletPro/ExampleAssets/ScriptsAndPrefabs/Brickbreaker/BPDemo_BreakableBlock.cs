using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_BreakableBlock : MonoBehaviour {

	[Header("Stats")]
	public int hitsNeeded = 3;
	[Range(0f, 1f)]
	public float powerUpDropRate = 0.2f;

	int healthLeft, curSprite;

	[Header("Appearance")]
	public Color[] possibleColors;
	public Sprite[] sprites;

	[Header("References")]
	public ParticleSystem explosionFX;
	public SpriteRenderer graphics;
	public BulletEmitter powerUpEmitter;
	public AudioSource hitAudio, deathAudio, powerupAudio;
	public BulletReceiver[] receivers;
	
	float hurtTimestamp;

	void Start()
	{
		Respawn();
	}

	public void HurtBlock()
	{
		if (Time.time - hurtTimestamp < 0.2f) return;
		hurtTimestamp = Time.time;

		healthLeft--;
		if (healthLeft == 0) Die();
		else
		{
			hitAudio.Play();
			curSprite++;
			graphics.sprite = sprites[curSprite % sprites.Length];
		}
	}

	void Die()
	{
		if (Random.value < powerUpDropRate)
		{
			powerupAudio.Play();
			powerUpEmitter.Play();
		}
		else deathAudio.Play();

		explosionFX.Play();
		graphics.enabled = false;
		for (int i = 0; i < receivers.Length; i++)
			receivers[i].enabled = false;
	}

	public void Respawn()
	{
		powerUpEmitter.Kill();

		hurtTimestamp = -10f;
		healthLeft = hitsNeeded;
		curSprite = 0;
		graphics.enabled = true;
		graphics.color = possibleColors[Random.Range(0, possibleColors.Length)];
		graphics.sprite = sprites[0];
		for (int i = 0; i < receivers.Length; i++)
			receivers[i].enabled = true;

		// copy color in particles
		ParticleSystem.MainModule mm = explosionFX.main;
		Color c = graphics.color;
		Color burstColor = new Color((c.r+1f)*0.5f, (c.g+1f)*0.5f, (c.b+1f)*0.5f, 0.8f); 
		mm.startColor = burstColor;
	}
}