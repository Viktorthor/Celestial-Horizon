using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_SidescrollerPlayerController : MonoBehaviour {

	[Header("Stats")]
	public float moveSpeed;
	public float gravity;
	public AnimationCurve jumpCurve = AnimationCurve.Constant(0, 1, 1);
	public float curveAccelerationOnRelease = 3f;
	public int maxAllowedJumps = 2;

	[Header("Fake physics")]
	public Transform groundLevel;

	[Header("References")]
	public Transform playerGraphics;
	public SpriteRenderer playerGraphicsRenderer;
	public Animator animator;
	public Transform leftWall, rightWall;
	public AudioSource audioShoot;
	public BulletEmitter afterImageEmitter;

	//[Header("References to self")]
	[System.NonSerialized] public Transform self;
	[System.NonSerialized] public BulletEmitter shootScript;
	[System.NonSerialized] public BPDemo_KillableCharacter healthScript;

	[Header("Key Binding")]
	public KeyCode shotButton = KeyCode.LeftShift;
	public KeyCode left = KeyCode.LeftArrow;
	public KeyCode right = KeyCode.RightArrow;
	public KeyCode jump = KeyCode.UpArrow;

	private Vector3 inputVector;
	private int jumpsAllowedLeft;
	private float timeSinceLastJump, initialScale;
	private bool isGrounded, isJumping;
	private float shootStartVolume;

	// Get component references
	void Awake()
	{
		self = transform;
		shootScript = GetComponent<BulletEmitter>();
		healthScript = GetComponent<BPDemo_KillableCharacter>();

		inputVector = Vector3.zero;
		jumpsAllowedLeft = maxAllowedJumps;
		isGrounded = isJumping = false;

		initialScale = Mathf.Abs(playerGraphics.localScale.x);

		shootStartVolume = audioShoot.volume;
		audioShoot.volume = 0;
	}

	// Controls
	public void Update()
	{
		if (!healthScript.isAlive) return;

		InputUpdate();
		MovementUpdate();
		AnimationUpdate();
	}

	void InputUpdate()
	{
		inputVector = Vector3.zero;

		if (Input.GetKey(left))		inputVector.x -= moveSpeed;
		if (Input.GetKey(right))	inputVector.x += moveSpeed;
		if (Input.GetKeyDown(jump))	StartJump();

		if (Input.GetKeyDown(shotButton))	shootScript.Play();
		if (Input.GetKeyUp(shotButton))		shootScript.Pause(PlayOptions.RootAndSubEmitters);

		audioShoot.volume = shootScript.isPlaying ? shootStartVolume : 0;
	}

	void MovementUpdate()
	{
		// process jumping and/or gravity
		float currentGravity = gravity;
		if (isJumping)
		{
			timeSinceLastJump += Time.deltaTime * (Input.GetKey(jump) ? 1 : curveAccelerationOnRelease);
			if (timeSinceLastJump >= jumpCurve.keys[jumpCurve.keys.Length-1].time)
				isJumping = false;

			currentGravity *= -jumpCurve.Evaluate(timeSinceLastJump);
		}
		inputVector.y -= currentGravity;

		// process walls - using another vector so that animation can show running against a wall
		Vector3 finalVector = new Vector3(inputVector.x, inputVector.y, inputVector.z);
		if (finalVector.x < 0 && self.position.x < leftWall.position.x)
			finalVector.x = 0;
		if (finalVector.x > 0 && self.position.x > rightWall.position.x)
			finalVector.x = 0;

		// trail effect
		if (finalVector.x == 0 && isGrounded) afterImageEmitter.Stop();
		else afterImageEmitter.Play();

		// final translate
		self.Translate(finalVector * Time.deltaTime, Space.Self);

		// fake physics : we just assume there's a flat ground at a given position
		bool previouslyGrounded = isGrounded;
		isGrounded = self.position.y <= groundLevel.position.y;
		if (isGrounded && inputVector.y < 0)
		{
			if (!previouslyGrounded) animator.SetTrigger("Land");
			self.position = new Vector3(self.position.x, groundLevel.position.y, self.position.z);
			isJumping = false;
			jumpsAllowedLeft = maxAllowedJumps;

		}
	}

	void AnimationUpdate()
	{
		if (inputVector.x != 0)
			playerGraphics.localScale = new Vector3(initialScale * Mathf.Sign(inputVector.x), playerGraphics.localScale.y, playerGraphics.localScale.z);

		animator.SetBool("IsMoving", inputVector.x != 0);
		//animator.SetBool("IsGrounded", isGrounded);
		//animator.SetBool("IsShooting", shootScript.isPlaying);
	}

	void StartJump()
	{
		if (jumpsAllowedLeft < 1) return;
		animator.SetTrigger("Jump");
		timeSinceLastJump = 0f;
		jumpsAllowedLeft--;
		isJumping = true;
	}

	public void OnDeath()
	{
		afterImageEmitter.Stop();
	}
}