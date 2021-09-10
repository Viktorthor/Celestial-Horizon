using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletPro;

// This script is part of the BulletPro package for Unity.
// But it's only used in the example scene and I recommend writing a better one that fits your needs.
// Author : Simon Albou <albou.simon@gmail.com>

public class BPDemo_SkyHelper : MonoBehaviour {

	public GameObject redWarning;
	public Transform groundLevel;
	public float turnTime = 1f;
	public float turnSpeed = 0.002f;

	public void TurnBulletToPlayer(Bullet bullet, Vector3 hit)
	{
		bullet.moduleHoming.homingTags.tagList = 0;
		bullet.moduleHoming.homingTags["Player"] = true;

		Transform nt = bullet.moduleHoming.RefreshTarget();
		if (nt == null) return;
		
		// vertical/straight trajectory
		bullet.self.position = new Vector3(nt.position.x, bullet.self.position.y, bullet.self.position.z);
		bullet.moduleHoming.LookAtTarget(1);
		Instantiate(redWarning, new Vector3(nt.position.x, groundLevel.position.y, bullet.self.position.z), Quaternion.identity);

		// curved trajectory
		//StartCoroutine(SlowlyTurnBullet(bullet, turnTime));
	}

	IEnumerator SlowlyTurnBullet(Bullet bullet, float duration)
	{
		//float orientation = 0;
		float elapsedTime = 0;
		Vector3 targetPos = bullet.moduleHoming.currentTarget.position;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			bullet.moduleMovement.LookAt(targetPos, turnSpeed);
			yield return new WaitForEndOfFrame();
		}
	}
}
