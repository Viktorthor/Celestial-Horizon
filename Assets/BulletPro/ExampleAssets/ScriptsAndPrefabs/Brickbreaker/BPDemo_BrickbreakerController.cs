using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_BrickbreakerController : MonoBehaviour {

	Transform self;

	[Header("References")]
	public Transform leftEnd;
	public Transform rightEnd, leftWall, rightWall;
	public BulletEmitter bulletEmitter, fireEmitter;
	public SpriteRenderer spriteRenderer;
	public Sprite normalPaddle, shootingPaddle;

	[Header("Stats")]
	public float speed = 5f;
	public float speedMultiplier = 2f;
	public float maxBounceAngle = 70f;

	[Header("Key Binding")]
	public KeyCode leftMove = KeyCode.LeftArrow;
	public KeyCode rightMove = KeyCode.RightArrow;
	public KeyCode shootKey = KeyCode.Space;
	public KeyCode fastKey = KeyCode.LeftShift;

	bool hasPowerUp;

	void Awake()
	{
		self = transform;
		hasPowerUp = false;
	}

	public void BounceBall(Bullet bullet, Vector3 hitPoint)
	{
		float angle = Mathf.Lerp(-maxBounceAngle, maxBounceAngle, Mathf.InverseLerp(leftEnd.position.x, rightEnd.position.x, hitPoint.x));
		float x = bullet.self.position.x;
		float z = bullet.self.position.z;
		bullet.moduleMovement.LookAt(new Vector3(x, 10000, z));
		bullet.moduleMovement.Rotate(-angle);
	}

	void Update()
	{
		MovementUpdate();
		ShotUpdate();
		PowerUpUpdate();
	}

	void MovementUpdate()
	{
		float input = 0;
		if (Input.GetKey(leftMove)) input--;
		if (Input.GetKey(rightMove)) input++;

		if (input == 0) return;

		if (input > 0)
			if (rightEnd.position.x > rightWall.position.x)
				return;

		if (input < 0)
			if (leftEnd.position.x < leftWall.position.x)
				return;

		float fast = Input.GetKey(fastKey) ? speedMultiplier : 1f;

		self.Translate(speed * fast * input * Vector3.right * Time.deltaTime);
	}

	void ShotUpdate()
	{
		if (Input.GetKeyDown(shootKey))
			bulletEmitter.Play();
	}

	void PowerUpUpdate()
	{
		if (!hasPowerUp) return;

		if (!fireEmitter.isPlaying)
		{
			spriteRenderer.sprite = normalPaddle;
			hasPowerUp = false;
		}
	}

	public void EnablePowerUp()
	{
		hasPowerUp = true;
		spriteRenderer.sprite = shootingPaddle;
		fireEmitter.Stop();
		fireEmitter.Play();
	}
}