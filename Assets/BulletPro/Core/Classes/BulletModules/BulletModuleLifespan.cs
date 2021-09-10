using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	// Module for handling bullet lifetime
	public class BulletModuleLifespan : BulletModule
	{
		// if true, the bullet will die after a certain amount of time.
		public bool hasLimitedLifetime;
		// if true, the bullet will die after a certain travelled distance.
		public bool hasLimitedRange;
		// ^ These two booleans can exist at the same time.

		private float _lifespan;
		public float lifespan
		{
			get { return _lifespan; }
			set
			{
				_lifespan = value;
				moduleHoming.homingOverLifetime.UpdateInternalValues(bullet);
				moduleRenderer.alphaOverLifetime.UpdateInternalValues(bullet);
				moduleRenderer.colorOverLifetime.UpdateInternalValues(bullet);
				moduleMovement.speedOverLifetime.UpdateInternalValues(bullet);
				moduleMovement.angularSpeedOverLifetime.UpdateInternalValues(bullet);
				moduleMovement.scaleOverLifetime.UpdateInternalValues(bullet);
				moduleMovement.moveXFromAnim.UpdateInternalValues(bullet);
				moduleMovement.moveYFromAnim.UpdateInternalValues(bullet);
				moduleMovement.rotateFromAnim.UpdateInternalValues(bullet);
				moduleMovement.scaleFromAnim.UpdateInternalValues(bullet);
			}
		}

		public float maxTravellableDistance;

		public override void Enable() { base.Enable(); }
		public override void Disable() { base.Disable(); }

		// Called at Bullet.Update()
		public void Update()
		{
			// enabled spawn module means we're still waiting for the actual spawn
			if (moduleSpawn.isEnabled) return;

			if (hasLimitedLifetime)
				if (bullet.timeSinceAlive > lifespan)
					bullet.Die(true);

			if (hasLimitedRange)
				if (moduleMovement.totalTravelledDistance > maxTravellableDistance)
					bullet.Die(true);
		}

		// Called at Bullet.ApplyBulletParams()
		public void ApplyBulletParams(BulletParams bp)
		{
			isEnabled = bp.hasLifespan || bp.hasLimitedRange;

			hasLimitedLifetime = bp.hasLifespan;
			hasLimitedRange = bp.hasLimitedRange;

			if (hasLimitedLifetime) lifespan = solver.SolveDynamicFloat(bp.lifespan, 10405888, ParameterOwner.Bullet);
			if (hasLimitedRange) maxTravellableDistance = solver.SolveDynamicFloat(bp.maxTravellableDistance, 11396727, ParameterOwner.Bullet);
		}

		// Shows how many seconds this bullet has to live.
		public float GetRemainingLifespan() => (lifespan - bullet.timeSinceAlive);
		public float GetRemainingLifespanRatio() => (1 - (bullet.timeSinceAlive / lifespan)); // 1 to 0, dies at 0

		// Shows how many meters the bullet can travel before dying.
		public float GetRemainingTravellableDistance() => (maxTravellableDistance - moduleMovement.totalTravelledDistance);
		public float GetRemainingTravellableDistanceRatio() => (1 - (moduleMovement.totalTravelledDistance / maxTravellableDistance)); // 1 to 0, dies at 0
	}
}