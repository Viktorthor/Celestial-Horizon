using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	// Module that handles particle systems at various moments such as birth, death or collision.
	public class BulletModuleVFX : BulletModule
	{
		#region properties

		// VFX-related vars (1.2). This stores all the VFX that can be played by this bullet.
		public List<BulletVFXParams> availableVFX;

		// Sends events to all VFX objects in the pool in case they need to be stopped
		public event Action<int> Stop;
		public event Action ResetBullet;

		// If trying to play a VFX while dying, this bool forbids the VFX object to subscribe to events
		bool isDying;

		// User can set this in case VFX should originate from a different object
		public Transform vfxOrigin;

		// Travels through the multiple overloads of PlayVFX, so the last one can know which index a VFX object is used for
		private int currentVFXIndex;

		#endregion

		// Called at Bullet.Awake()
		public override void Awake()
		{
			base.Awake();
			availableVFX = new List<BulletVFXParams>();

			// Reset values
			currentVFXIndex = -1;
			vfxOrigin = self;
			isDying = false;
		}

		public override void Enable() { base.Enable(); }
		public override void Disable() { base.Disable(); }

		// Called at Bullet.Die(bool). VFX will only play if allowed by the argument.
		public void Die(bool spawnFX)
		{
			isDying = true;
			Death_ToggleFX(spawnFX);
			Death_End();
		}

		// Called at Bullet.Die(Vector3). VFX will only play at wanted position.
		public void Die(Vector3 vfxPosition)
		{
			isDying = true;
			Death_PositionFX(vfxPosition);
			Death_End();
		}

		// Component of the Die() function
		void Death_ToggleFX(bool spawnFX)
		{
			if (availableVFX == null) return;
			if (availableVFX.Count == 0) return;

			for (int i = 0; i < availableVFX.Count; i++)
			{
				BulletVFXTrigger trig = availableVFX[i].onBulletDeath;
				if (!AskPermission(trig)) continue;
				if (trig.behaviour == BulletVFXBehaviour.Play && spawnFX) PlayVFX(i);
				else if (trig.behaviour == BulletVFXBehaviour.Stop) Stop?.Invoke(i);
			}
		}

		// Component of the Die() function
		void Death_PositionFX(Vector3 vfxPosition)
		{
			if (availableVFX == null) return;
			if (availableVFX.Count == 0) return;

			// Play/stop relevant VFX
			for (int i = 0; i < availableVFX.Count; i++)
			{
				BulletVFXTrigger trig = availableVFX[i].onBulletDeath;
				if (!AskPermission(trig)) continue;
				if (trig.behaviour == BulletVFXBehaviour.Play) PlayVFX(vfxPosition, i);
				else if (trig.behaviour == BulletVFXBehaviour.Stop) Stop?.Invoke(i);
			}
		}

		// Component of the Die() function
		void Death_End()
		{
			ResetBullet?.Invoke();

			// Reset values
			currentVFXIndex = -1;
			vfxOrigin = self;
		}

		// Called at Bullet.ApplyBulletParams()
		public void ApplyBulletParams(BulletParams bp)
		{
			isDying = false;
			availableVFX.Clear();
			if (bp.vfxParams == null) return;
			else if (bp.vfxParams.Length == 0) return;
			for (int i = 0; i < bp.vfxParams.Length; i++)
				availableVFX.Add(solver.SolveDynamicBulletVFXParams(bp.vfxParams[i], 17789231, ParameterOwner.Bullet));
		}

		#region VFX toolbox, with new functions from v1.2

		// Looks at a VFXTrigger and tells if a specific Play/Stop action is allowed
		public bool AskPermission(BulletVFXTrigger trig)
		{
			if (!trig.enabled) return false;

			if (trig.unless)
			{
				if ((trig.canceller & BulletVFXCanceller.BulletGraphicsAreEnabled) != 0)
					if (moduleRenderer.isEnabled)
						return false;

				if ((trig.canceller & BulletVFXCanceller.BulletGraphicsAreDisabled) != 0)
					if (!moduleRenderer.isEnabled)
						return false;
			}

			return true;
		}
		
		// Stops a VFX if it's currently playing and registered. Also unregisters it in the process.
		public void StopVFX(int index)
		{
			Stop?.Invoke(index);
		}

		// Stops VFX just like the overload above, but fetches them by name.
		public void StopVFX(string vfxTag)
		{
            if (availableVFX == null) return;
            if (availableVFX.Count == 0) return;

            // Not great because of O(n²) complexity, but most bullets will only have one or two VFX.
            for (int i = 0; i < availableVFX.Count; i++)
                if (availableVFX[i].tag == vfxTag)
                    Stop?.Invoke(i);
		}

		#region play VFX at bullet position

		// Plays the desired VFX at bullet position, possibly with wanted overrides.
		public BulletVFX PlayVFX(ParticleSystem particleSystemSettings, bool attach=false, List<BulletVFXOverride> overrides=null)
		{
			overrides = UpdateOverrides(overrides);		
			BulletVFX bvfx = vfxManager.PlayVFXAt(vfxOrigin.position, vfxOrigin.eulerAngles, particleSystemSettings, overrides);
			if (bvfx == null) return null;
			
			if (isDying) bvfx.ResetOwnership();
			else
			{
				bvfx.GiveOwner(bullet, currentVFXIndex);
				if (attach)
				{
					Vector3 oldScale = bvfx.thisTransform.localScale;
					bvfx.thisTransform.SetParent(self);
					bvfx.thisTransform.localScale = oldScale;
				}
			}
			currentVFXIndex = -1;
			return bvfx;
		}

		// Plays the n-th VFX in this bullet's list of available VFX.
		public BulletVFX PlayVFX(int index=0)
		{
			// Extra safety checks, but shouldn't ever happen
			if (availableVFX == null) return null;
			if (index < 0) return null;
			if (index >= availableVFX.Count) return null;
			
			currentVFXIndex = index;
			ParticleSystem ps = availableVFX[index].useDefaultParticles ? BulletVFXManager.instance.defaultParticles : availableVFX[index].particleSystemPrefab;
			return PlayVFX(ps, availableVFX[index].attachToBulletTransform, availableVFX[index].vfxOverrides);
		}

		// Plays the VFX named <vfxTag> in this bullet's list of available VFX. Returns all objects that play a VFX.
		public List<BulletVFX> PlayVFX(string vfxTag)
		{
			List<BulletVFX> result = new List<BulletVFX>();

			// Extra safety checks, but shouldn't ever happen
			if (availableVFX == null) return result;
			if (availableVFX.Count == 0) return result;
			
			for (int i = 0; i < availableVFX.Count; i++)
			{
				if (availableVFX[i].tag != vfxTag) continue;
				BulletVFX bvfx = PlayVFX(i);
				if (bvfx != null) result.Add(bvfx);
			}

			// If no relevant VFX is found, nothing happens and an empty list will be returned.
			return result;
		}

		#endregion

		#region play VFX at custom position

		// Plays the desired VFX at bullet position, possibly with wanted overrides.
		public BulletVFX PlayVFX(ParticleSystem particleSystemSettings, Vector3 position, bool attach=false, List<BulletVFXOverride> overrides=null)
		{
			overrides = UpdateOverrides(overrides);		
			BulletVFX bvfx = vfxManager.PlayVFXAt(position, 0, particleSystemSettings, overrides);
			if (bvfx == null) return null;
			
			if (isDying) bvfx.ResetOwnership();
			else
			{
				bvfx.GiveOwner(bullet, currentVFXIndex);
				if (attach)
				{
					Vector3 oldScale = bvfx.thisTransform.localScale;
					bvfx.thisTransform.SetParent(self);
					bvfx.thisTransform.localScale = oldScale;
				}
			}
			currentVFXIndex = -1;
			return bvfx;
		}

		// Plays the n-th VFX in this bullet's list of available VFX.
		public BulletVFX PlayVFX(Vector3 position, int index=0)
		{
			// Extra safety checks, but shouldn't ever happen
			if (availableVFX == null) return null;
			if (index < 0) return null;
			if (index >= availableVFX.Count) return null;
			
			currentVFXIndex = index;
			ParticleSystem ps = availableVFX[index].useDefaultParticles ? BulletVFXManager.instance.defaultParticles : availableVFX[index].particleSystemPrefab;
			return PlayVFX(ps, position, availableVFX[index].attachToBulletTransform, availableVFX[index].vfxOverrides);
		}

		// Plays the VFX named <vfxTag> in this bullet's list of available VFX.
		public List<BulletVFX> PlayVFX(string vfxTag, Vector3 position)
		{
			List<BulletVFX> result = new List<BulletVFX>();

			// Extra safety checks, but shouldn't ever happen
			if (availableVFX == null) return result;
			if (availableVFX.Count == 0) return result;
			
			for (int i = 0; i < availableVFX.Count; i++)
			{
				if (availableVFX[i].tag != vfxTag) continue;
				BulletVFX bvfx = PlayVFX(position, i);
				if (bvfx != null) result.Add(bvfx);
			}

			// If no relevant VFX is found, nothing happens and an empty list will be returned.
			return result;
		}

		#endregion

		// Update data in overrides with current color, current scale, etc etc
		List<BulletVFXOverride> UpdateOverrides(List<BulletVFXOverride> rawOverrides)
		{
			if (rawOverrides == null) return null;
			if (rawOverrides.Count == 0) return rawOverrides;
			for (int i = 0; i < rawOverrides.Count; i++)
				rawOverrides[i] = UpdateOverride(rawOverrides[i]);

			return rawOverrides;
		}
		
		BulletVFXOverride UpdateOverride(BulletVFXOverride rawOverride)
		{
			if (rawOverride.referenceColor == VFXReferenceColor.BulletColor)
			{
				Color col = Color.white;
				if (bullet.renderMode == BulletRenderMode.Sprite)
					col = spriteRenderer.color;
				rawOverride.colorValue = col;
			}

			if (rawOverride.referenceGradient == VFXReferenceGradient.BulletLifetimeGradient)
				rawOverride.gradientValue = moduleRenderer.colorEvolution;

			if (rawOverride.referenceFloat == VFXReferenceFloat.BulletLifetime)
				rawOverride.floatValue = moduleLifespan.GetRemainingLifespan();
			else if (rawOverride.referenceFloat == VFXReferenceFloat.BulletScale)
				rawOverride.floatValue = moduleMovement.currentScale;
			else if (rawOverride.referenceFloat == VFXReferenceFloat.BulletSpeed)
				rawOverride.floatValue = moduleMovement.currentSpeed;

			return rawOverride;
		}

		#endregion
	}
}