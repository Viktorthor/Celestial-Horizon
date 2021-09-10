using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_RedWarning : MonoBehaviour {

	public AnimationCurve alphaCurve;
	public SpriteRenderer spriteRenderer;
	float startAlpha, elapsedTime;

	void Awake()
	{
		startAlpha = spriteRenderer.color.a;
		enabled = false;

		Color c = spriteRenderer.color;
		spriteRenderer.color = new Color(c.r, c.g, c.b, 0f);
		StartCoroutine(AlphaFadeIn());
	}

	IEnumerator AlphaFadeIn()
	{
		float ratio = 0;
		while (ratio < 1 && !enabled)
		{
			ratio += Time.deltaTime * 2.0f;
			Color c = spriteRenderer.color;
			spriteRenderer.color = new Color(c.r, c.g, c.b, startAlpha * ratio);
			yield return new WaitForEndOfFrame();
		}
	}

	void Update()
	{
		elapsedTime += Time.deltaTime;
		float curAlpha = alphaCurve.Evaluate(elapsedTime);
		Color c = spriteRenderer.color;
		spriteRenderer.color = new Color(c.r, c.g, c.b, curAlpha * startAlpha);

		if (elapsedTime > alphaCurve.keys[alphaCurve.keys.Length-1].time)
			Destroy(gameObject);
	}

	public void KillWarning()
	{
		enabled = true;
	}
}
