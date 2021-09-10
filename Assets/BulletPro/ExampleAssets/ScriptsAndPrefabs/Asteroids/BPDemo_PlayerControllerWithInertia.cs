using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_PlayerControllerWithInertia : BPDemo_PlayerController
{
	struct TimestampedDirection
	{
		public float startSpeed, maxSpeed, curSpeed;
		public float accelerationTime, decelerationTime;
		public AnimationCurve accelerationCurve, decelerationCurve;
		public KeyCode key;
		float timeSinceHeld, timeSinceReleased;
		bool isHeld;

		void UpdatePressed()
		{
			if (!isHeld)
			{
				timeSinceHeld = 0;
				startSpeed = curSpeed;
			}
			
			isHeld = true;
			timeSinceHeld += Time.deltaTime;
			if (accelerationTime > 0)
				curSpeed = Mathf.Lerp(startSpeed, maxSpeed, accelerationCurve.Evaluate(timeSinceHeld/accelerationTime));
			else curSpeed = maxSpeed;
		}

		void UpdateUnpressed()
		{
			if (isHeld)
			{
				timeSinceReleased = 0;
				startSpeed = curSpeed;
			}
			
			isHeld = false;
			timeSinceReleased += Time.deltaTime;
			if (decelerationTime > 0)
				curSpeed = Mathf.Lerp(0, startSpeed, decelerationCurve.Evaluate(timeSinceReleased/decelerationTime));
			else curSpeed = 0;
		}

		public void Update()
		{
			if (Input.GetKey(key)) UpdatePressed();
			else UpdateUnpressed();
		}
	}

	[Header("Inertia Stats")]
	public float maxDirectionalValue = 1;
	public float accelerationTime = 1;
	public float decelerationTime = 1;
	public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
	public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);
	TimestampedDirection leftSpeed, rightSpeed, upSpeed, downSpeed;
	private Vector2 curSpeed;

	public override void Awake()
	{
		base.Awake();

		leftSpeed = new TimestampedDirection();
		rightSpeed = new TimestampedDirection();
		upSpeed = new TimestampedDirection();
		downSpeed = new TimestampedDirection();

		leftSpeed.maxSpeed = rightSpeed.maxSpeed = upSpeed.maxSpeed = downSpeed.maxSpeed = maxDirectionalValue;
		leftSpeed.accelerationTime = rightSpeed.accelerationTime = upSpeed.accelerationTime = downSpeed.accelerationTime = accelerationTime;
		leftSpeed.decelerationTime = rightSpeed.decelerationTime = upSpeed.decelerationTime = downSpeed.decelerationTime = decelerationTime;
		leftSpeed.accelerationCurve = rightSpeed.accelerationCurve = upSpeed.accelerationCurve = downSpeed.accelerationCurve = accelerationCurve;
		leftSpeed.decelerationCurve = rightSpeed.decelerationCurve = upSpeed.decelerationCurve = downSpeed.decelerationCurve = decelerationCurve;

		leftSpeed.key = left;
		rightSpeed.key = right;
		upSpeed.key = up;
		downSpeed.key = down;
	}

	// Controls
	public override void Update()
	{
		if (!healthScript.isAlive) return;

		leftSpeed.Update();
		rightSpeed.Update();
		upSpeed.Update();
		downSpeed.Update();

		Vector2 finalDirection = new Vector2(rightSpeed.curSpeed-leftSpeed.curSpeed, upSpeed.curSpeed-downSpeed.curSpeed);

		if (finalDirection.x < 0 && self.position.x-0.5f < leftWall.position.x) finalDirection.x = 0;
		if (finalDirection.x > 0 && self.position.x+0.5f > rightWall.position.x) finalDirection.x = 0;
		if (finalDirection.y < 0 && self.position.y-0.5f < lowerWall.position.y) finalDirection.y = 0;
		if (finalDirection.y > 0 && self.position.y+0.5f > upperWall.position.y) finalDirection.y = 0;

		self.Translate(finalDirection * moveSpeed * Time.deltaTime, Space.Self);

		if (Input.GetKeyDown(shotButton))	shootScript.Play();
		if (Input.GetKeyUp(shotButton))		shootScript.Stop(PlayOptions.RootAndSubEmitters);
	}
}
