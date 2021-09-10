using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_KillableCharacter : MonoBehaviour {

	[Header("Stats")]
	public float maxHealth = 100;
	[System.NonSerialized] public float curHealth;

	[Header("References")]
	public BulletEmitter[] bulletEmitters;
	public SpriteRenderer sprite;
	public BulletReceiver receiver;
	public Transform lifebar;
	[System.NonSerialized] public SpriteRenderer lifebarSprite;

	[System.NonSerialized] public bool isAlive;
	Coroutine fadeAlpha;

	[Header("Events")]
	public UnityEvent onHurt;
	public UnityEvent onDeath;
	public UnityEvent onRespawn;

	void Awake()
	{
		lifebarSprite = lifebar.GetComponent<SpriteRenderer>();

		isAlive = true;

		curHealth = maxHealth;
		UpdateLifebar();
	}

	public void Hurt(Bullet bullet, Vector3 hitPoint)
	{
		if (!isAlive) return;
		curHealth -= bullet.moduleParameters.GetFloat("_PowerLevel");
		UpdateLifebar();
		if (curHealth > 0)
		{
			if (onHurt != null)
				onHurt.Invoke();
		}
		else Die();
	}

	void Die()
	{
		isAlive = false;
		lifebarSprite.enabled = false;
		for (int i = 0; i < bulletEmitters.Length; i++) bulletEmitters[i].Kill();
		receiver.enabled = false;
		if (onDeath != null) onDeath.Invoke();
	}

	public void AlphaFadeOut() { fadeAlpha = StartCoroutine(FadeAlpha(1.5f)); }

	IEnumerator FadeAlpha(float duration)
	{
		float innerTimer = 0;
		while (innerTimer < duration)
		{
			innerTimer += Time.deltaTime;
			Color cur = Color.white;
			cur.a = 1-innerTimer/duration;
			sprite.color = cur;
			yield return null;
		}
		sprite.enabled = false;
	}

	public void Respawn()
	{
		curHealth = maxHealth;
		UpdateLifebar();
		isAlive = true;
		lifebarSprite.enabled = true;
		if (fadeAlpha != null) StopCoroutine(fadeAlpha);
		sprite.enabled = true;
		sprite.color = Color.white;
		receiver.enabled = true;
		if (onRespawn != null) onRespawn.Invoke();
		
		for (int i = 0; i < bulletEmitters.Length; i++)
		{
			bulletEmitters[i].Kill();
			if (bulletEmitters[i].playAtStart)
				bulletEmitters[i].Boot();
		}
	}

	public void UpdateLifebar()
	{
		lifebar.localScale = new Vector3(curHealth/maxHealth, lifebar.localScale.y, lifebar.localScale.z);		
	}
}
