using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	[AddComponentMenu("BulletPro/Managers/Bullet VFX Manager")]
	public class BulletVFXManager : MonoBehaviour
	{
		public static BulletVFXManager instance;
		[HideInInspector]
		public BulletVFX[] effectPool;
		public ParticleSystem defaultParticles;
		public ParticleSystemRenderer defaultParticleRenderer;
		public GameObject vfxPrefab; // prefab to be instantiated in Editor mode, will then get pooled
		Transform self;

		void Awake()
		{
			if (instance == null) instance = this;
			else Debug.LogWarning("Beware : there is more than one instance of BulletVFXManager in the scene!");

			self = transform;
		}

		public static BulletVFX GetFreeBullet()
		{
			if (instance == null)
			{
				Debug.LogWarning("No bullet pool found in scene !");
				return null;
			}

			return instance.GetFreeBulletLocal();
		}

		public BulletVFX GetFreeBulletLocal()
		{
			if (effectPool == null)
			{
				Debug.LogWarning(name + " has no pool!");
				return null;
			}
			if (effectPool.Length == 0)
			{
				Debug.LogWarning(name + " : pool is empty!");
				return null;
			}

			for (int i = 0; i < effectPool.Length; i++)
				if (!effectPool[i].thisParticleSystem.isPlaying)
				{
					// Reset parent if applicable
					if (effectPool[i].thisTransform.parent != self)
					{
						effectPool[i].thisTransform.SetParent(self);
						effectPool[i].thisTransform.localScale = Vector3.one;
					}
					return effectPool[i];
				}

			Debug.LogWarning(name + " has not enough bullets in pool!");
			return null;
		}

		// All the Play functions below return the VFX object used from the pool.

		// Overload 1 : play the default VFX with wanted orientation, color and size
		public BulletVFX PlayVFXAt(Vector3 position, float rotation, Color color, float size)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, rotation, color, size);
			return bvfx;
		}

		// Overload 2 : set VFX to wanted ParticleSystem settings and then play it
		public BulletVFX PlayVFXAt(Vector3 position, float rotation, ParticleSystem psSettings, float size)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, rotation, psSettings, size);
			return bvfx;
		}

		// Overload 3 : similar to overload 1 but with full rotation transmitted via (global) eulerAngles
		public BulletVFX PlayVFXAt(Vector3 position, Vector3 eulerAngles, Color color, float size)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, eulerAngles, color, size);
			return bvfx;
		}

		// Overload 4 : similar to overload 2 but with full rotation transmitted via (global) eulerAngles		
		public BulletVFX PlayVFXAt(Vector3 position, Vector3 eulerAngles, ParticleSystem psSettings, float size)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, eulerAngles, psSettings, size);
			return bvfx;
		}

		// Overload 5 : play the default VFX with a simple rotation and any overrides
		public BulletVFX PlayVFXAt(Vector3 position, float rotation, List<BulletVFXOverride> overrides)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, rotation, overrides);
			return bvfx;
		}

		// Overload 6 : play the default VFX with full rotation and any overrides
		public BulletVFX PlayVFXAt(Vector3 position, Vector3 eulerAngles, List<BulletVFXOverride> overrides)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, eulerAngles, overrides);
			return bvfx;
		}

		// Overload 7 : play wanted ParticleSystem with a simple rotation and any overrides
		public BulletVFX PlayVFXAt(Vector3 position, float rotation, ParticleSystem psSettings, List<BulletVFXOverride> overrides)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, rotation, psSettings, overrides);
			return bvfx;
		}

		// Overload 8 : play wanted ParticleSystem with full rotation and any overrides
		public BulletVFX PlayVFXAt(Vector3 position, Vector3 eulerAngles, ParticleSystem psSettings, List<BulletVFXOverride> overrides)
		{
			BulletVFX bvfx = GetFreeBulletLocal();
			if (!bvfx) return null;
			bvfx.Play(position, eulerAngles, psSettings, overrides);
			return bvfx;
		}
	}
}
