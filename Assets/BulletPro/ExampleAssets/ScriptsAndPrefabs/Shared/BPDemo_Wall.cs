using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_Wall : MonoBehaviour {

	Transform self;
	public float bounceCooldown = 0.2f;
	public BounceChannel channel = BounceChannel.Horizontal;

	void Awake()
	{
		self = transform;
	}

	public void BounceBullet(Bullet bullet, Vector3 hitPoint)
	{
		bullet.moduleMovement.Bounce(self.up, bounceCooldown, channel);
	}
}
