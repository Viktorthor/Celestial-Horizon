using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_LightningBarrierBehaviour : MonoBehaviour {

	public float delay = 0.15f;
	public SpriteRenderer spriteRenderer;
	public Sprite[] sprites;

	void Update ()
	{
		if (Time.time % delay < Time.deltaTime)
			spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
	}
}
