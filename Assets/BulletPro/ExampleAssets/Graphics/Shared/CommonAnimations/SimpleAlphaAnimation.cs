using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAlphaAnimation : MonoBehaviour
{
	public AnimationCurve animCurve = AnimationCurve.Constant(0, 1, 1);
	public float period = 2f;
	public SpriteRenderer spriteRenderer;
	float age;

	void Update()
	{
		age += Time.deltaTime;
		float ratio = (age % period)/period;

		spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, animCurve.Evaluate(ratio));
	}	
}
