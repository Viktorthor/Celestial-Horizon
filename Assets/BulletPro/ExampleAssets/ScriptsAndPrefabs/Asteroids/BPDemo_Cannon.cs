using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_Cannon : MonoBehaviour {

	public Transform self;
	public Transform crosshair;

	void Start ()
	{
		if (!self) self = transform;
	}

	void Update () 
	{
		Vector2 diff = crosshair.position - self.position;
		float angle = Vector2.Angle(self.up, diff);
		Vector3 cross = Vector3.Cross(self.up, diff);
		if (cross.z < 0) angle *= -1;
		self.Rotate(Vector3.forward, angle, Space.Self);
	}
}
