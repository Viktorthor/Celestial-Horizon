using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	[System.Flags]
	public enum ShurikenModuleMask
	{
		Main = (1 << 0),
		Emission = (1 << 1),
		Shape = (1 << 2),
		VelocityOverLifetime = (1 << 3),
		LimitVelocityOverLifetime = (1 << 4),
		InheritVelocity = (1 << 5),
		ForceOverLifetime = (1 << 6),
		ColorOverLifetime = (1 << 7),
		ColorBySpeed = (1 << 8),
		SizeOverLifetime = (1 << 9),
		SizeBySpeed = (1 << 10),
		RotationOverLifetime = (1 << 11),
		RotationBySpeed = (1 << 12),
		ExternalForces = (1 << 13),
		Noise = (1 << 14),
		Collision = (1 << 15),
		Trigger = (1 << 16),
		TextureSheetAnimation = (1 << 17),
		Lights = (1 << 18),
		Trail = (1 << 19),
		Renderer = (1 << 20)
	}

	[RequireComponent(typeof(ParticleSystem))]
	public class BulletVFX : MonoBehaviour
	{
		public Transform thisTransform;
		public ParticleSystem thisParticleSystem;
		private ParticleSystem defaultPS;
		public ParticleSystemRenderer thisParticleRenderer;
		private ParticleSystemRenderer defaultPSR;
		private Transform bulletCanvas;

		// so we don't have to copy from default particles every time
		private ParticleSystem previousCopiedPS;
		private ParticleSystemRenderer previousCopiedPSR;
		private ShurikenModuleMask previouslyOverridenModules;

		// Which bullet is currently using this object as VFX?
		[System.NonSerialized] public Bullet currentOwner;
		// For which VFXParams (from VFXModule) has this object been recruited?
		[System.NonSerialized] public int indexInCurrentOwner;

		void Start()
		{
			defaultPS = BulletVFXManager.instance.defaultParticles;
			defaultPSR = BulletVFXManager.instance.defaultParticleRenderer;
			bulletCanvas = BulletPoolManager.instance.mainTransform;
			
			previouslyOverridenModules = 0;
			previousCopiedPS = null;
			previousCopiedPSR = null;
			CopyParticlesFrom(defaultPS);

			ResetOwnership();
		}

		public void ResetOwnership()
		{
			if (currentOwner != null)
			{
				currentOwner.moduleVFX.Stop -= StopIndex;
				currentOwner.moduleVFX.ResetBullet -= ResetOwnership;
			}

			currentOwner = null;
			indexInCurrentOwner = -1;
		}

		public void GiveOwner(Bullet newOwner, int vfxIndex=-1)
		{
			ResetOwnership();

			if (newOwner == null) return;

			currentOwner = newOwner;
			indexInCurrentOwner = vfxIndex;

			currentOwner.moduleVFX.Stop += StopIndex;
			currentOwner.moduleVFX.ResetBullet += ResetOwnership;
		}

		public void Stop()
		{
			thisParticleSystem.Stop();
			ResetOwnership();
		}

		// Called by bullet VFX module events, if event args match
		void StopIndex(int index)
		{
			if (index == indexInCurrentOwner) Stop();
		}

		#region multiple overloads of the Play function

		// Overload 1 : play the default VFX with wanted orientation, color and size
		public void Play(Vector3 position, float rotation, Color color, float size)
		{
			CopyParticlesFrom(defaultPS);
			previouslyOverridenModules = ShurikenModuleMask.Main;

			thisTransform.position = position;
			thisTransform.rotation = bulletCanvas.rotation;
			thisTransform.Rotate(Vector3.forward, rotation, Space.Self);

			ParticleSystem.MainModule mainModule = thisParticleSystem.main;
			mainModule.startColor = color;
			mainModule.startSpeed = defaultPS.main.startSpeed;
			mainModule.startSpeedMultiplier *= size;
			
			if (defaultPS.main.startSize3D)
			{
				mainModule.startSizeX = defaultPS.main.startSizeX;
				mainModule.startSizeXMultiplier *= size;
				mainModule.startSizeY = defaultPS.main.startSizeY;
				mainModule.startSizeYMultiplier *= size;
				mainModule.startSizeZ = defaultPS.main.startSizeZ;
				mainModule.startSizeZMultiplier *= size;
			}
			else
			{
				mainModule.startSize = defaultPS.main.startSize;
				mainModule.startSizeMultiplier *= size;
			}
			
			thisParticleSystem.Play();
		}

		// Overload 2 : set VFX to wanted ParticleSystem settings and then play it
		public void Play(Vector3 position, float rotation, ParticleSystem psSettings, float size)
		{
			thisTransform.position = position;
			thisTransform.rotation = bulletCanvas.rotation;
			thisTransform.Rotate(Vector3.forward, rotation, Space.Self);

			CopyParticlesFrom(psSettings);
			previouslyOverridenModules = ShurikenModuleMask.Main;

			ParticleSystem.MainModule mainModule = thisParticleSystem.main;
			mainModule.startSizeMultiplier *= size;
			mainModule.startSizeXMultiplier *= size;
			mainModule.startSizeYMultiplier *= size;
			mainModule.startSizeZMultiplier *= size;
			mainModule.startSpeedMultiplier *= size; // multiplying the speed keeps the system shape proportions
			thisParticleSystem.Play();
		}

		// Overload 3 : similar to overload 1, but with full rotation transmitted via (global) eulerAngles
		public void Play(Vector3 position, Vector3 eulerAngles, Color color, float size)
		{
			CopyParticlesFrom(defaultPS);
			previouslyOverridenModules = ShurikenModuleMask.Main;

			thisTransform.position = position;
			thisTransform.eulerAngles = eulerAngles;
			ParticleSystem.MainModule mainModule = thisParticleSystem.main;
			mainModule.startColor = color;
			mainModule.startSpeed = defaultPS.main.startSpeed;
			mainModule.startSpeedMultiplier *= size;
			
			if (defaultPS.main.startSize3D)
			{
				mainModule.startSizeX = defaultPS.main.startSizeX;
				mainModule.startSizeXMultiplier *= size;
				mainModule.startSizeY = defaultPS.main.startSizeY;
				mainModule.startSizeYMultiplier *= size;
				mainModule.startSizeZ = defaultPS.main.startSizeZ;
				mainModule.startSizeZMultiplier *= size;
			}
			else
			{
				mainModule.startSize = defaultPS.main.startSize;
				mainModule.startSizeMultiplier *= size;
			}
			
			thisParticleSystem.Play();
		}

		// Overload 4 : similar to overload 2, but with full rotation transmitted via (global) eulerAngles
		public void Play(Vector3 position, Vector3 eulerAngles, ParticleSystem psSettings, float size)
		{
			thisTransform.position = position;
			thisTransform.eulerAngles = eulerAngles;
			CopyParticlesFrom(psSettings);
			previouslyOverridenModules = 0;
			ParticleSystem.MainModule mainModule = thisParticleSystem.main;
			mainModule.startSizeMultiplier *= size;
			mainModule.startSizeXMultiplier *= size;
			mainModule.startSizeYMultiplier *= size;
			mainModule.startSizeZMultiplier *= size;
			mainModule.startSpeedMultiplier *= size; // multiplying the speed keeps the system shape proportions
			thisParticleSystem.Play();
		}

		// Overload 5 : play the default VFX with a simple rotation and any overrides
		public void Play(Vector3 position, float rotation, List<BulletVFXOverride> overrides)
		{
			CopyParticlesFrom(defaultPS);
			thisTransform.position = position;
			thisTransform.rotation = bulletCanvas.rotation;
			thisTransform.Rotate(Vector3.forward, rotation, Space.Self);
			ApplyOverrides(overrides);
			thisParticleSystem.Play();
		}

		// Overload 6 : play the default VFX with full rotation and any overrides
		public void Play(Vector3 position, Vector3 eulerAngles, List<BulletVFXOverride> overrides)
		{
			CopyParticlesFrom(defaultPS);
			thisTransform.position = position;
			thisTransform.eulerAngles = eulerAngles;
			ApplyOverrides(overrides);
			thisParticleSystem.Play();
		}

		// Overload 7 : play wanted ParticleSystem with a simple rotation and any overrides
		public void Play(Vector3 position, float rotation, ParticleSystem psSettings, List<BulletVFXOverride> overrides)
		{
			thisTransform.position = position;
			thisTransform.rotation = bulletCanvas.rotation;
			thisTransform.Rotate(Vector3.forward, rotation, Space.Self);
			CopyParticlesFrom(psSettings);
			ApplyOverrides(overrides);
			thisParticleSystem.Play();
		}

		// Overload 8 : play wanted ParticleSystem with full rotation and any overrides
		public void Play(Vector3 position, Vector3 eulerAngles, ParticleSystem psSettings, List<BulletVFXOverride> overrides)
		{
			thisTransform.position = position;
			thisTransform.eulerAngles = eulerAngles;
			CopyParticlesFrom(psSettings);
			ApplyOverrides(overrides);
			thisParticleSystem.Play();
		}

		#endregion

		// Returns whether an override was used on a module last time this VFX was used. Helps avoid unnecessary deep copies.
		bool IsOverridden(ShurikenModuleMask particleModule)
		{
			return (previouslyOverridenModules & particleModule) > 0;
		}

		// Copies settings from another ParticleSystem. There's a ton of properties so this is long. But it gives better perfs than reflection.
		public void CopyParticlesFrom(ParticleSystem psSettings)
		{
			bool isNewPS = (previousCopiedPS != psSettings);
			previousCopiedPS = psSettings;

			bool shouldCopy = isNewPS;
			if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Main);
			if (shouldCopy) CopyMainModuleFrom(psSettings);

			shouldCopy = isNewPS;
			if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Renderer);
			if (shouldCopy)
			{
				ParticleSystemRenderer psrSettings = previousCopiedPSR;
				if (isNewPS)
				{
					// we must take literally anything to avoid calling GetComponent
					if (psSettings == defaultPS) psrSettings = defaultPSR;
					else psrSettings = psSettings.GetComponent<ParticleSystemRenderer>();
				}
				CopyParticleSystemRendererFrom(psrSettings);
				previousCopiedPSR = psrSettings;
			}

			if (psSettings.collision.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Collision);
				if (shouldCopy)
					CopyCollisionModuleFrom(psSettings);
			}
			else { ParticleSystem.CollisionModule m = thisParticleSystem.collision; m.enabled = false; }

			if (psSettings.colorBySpeed.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.ColorBySpeed);
				if (shouldCopy)
					CopyColorBySpeedModuleFrom(psSettings);
			}
			else { ParticleSystem.ColorBySpeedModule m = thisParticleSystem.colorBySpeed; m.enabled = false; }

			if (psSettings.colorOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.ColorOverLifetime);
				if (shouldCopy)
					CopyColorOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.ColorOverLifetimeModule m = thisParticleSystem.colorOverLifetime; m.enabled = false; }

			if (psSettings.emission.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Emission);
				if (shouldCopy)
					CopyEmissionModuleFrom(psSettings);
			}
			else { ParticleSystem.EmissionModule m = thisParticleSystem.emission; m.enabled = false; }

			if (psSettings.externalForces.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.ExternalForces);
				if (shouldCopy)
					CopyExternalForcesModuleFrom(psSettings);
			}
			else { ParticleSystem.ExternalForcesModule m = thisParticleSystem.externalForces; m.enabled = false; }

			if (psSettings.forceOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.ForceOverLifetime);
				if (shouldCopy)
					CopyForceOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.ForceOverLifetimeModule m = thisParticleSystem.forceOverLifetime; m.enabled = false; }

			if (psSettings.inheritVelocity.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.InheritVelocity);
				if (shouldCopy)
					CopyInheritVelocityModuleFrom(psSettings);
			}
			else { ParticleSystem.InheritVelocityModule m = thisParticleSystem.inheritVelocity; m.enabled = false; }

			if (psSettings.lights.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Lights);
				if (shouldCopy)
					CopyLightsModuleFrom(psSettings);
			}
			else { ParticleSystem.LightsModule m = thisParticleSystem.lights; m.enabled = false; }

			if (psSettings.limitVelocityOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.LimitVelocityOverLifetime);
				if (shouldCopy)
					CopyLimitVelocityOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.LimitVelocityOverLifetimeModule m = thisParticleSystem.limitVelocityOverLifetime; m.enabled = false; }

			if (psSettings.noise.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Noise);
				if (shouldCopy)
					CopyNoiseModuleFrom(psSettings);
			}
			else { ParticleSystem.NoiseModule m = thisParticleSystem.noise; m.enabled = false; }

			if (psSettings.rotationBySpeed.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.RotationBySpeed);
				if (shouldCopy)
					CopyRotationBySpeedModuleFrom(psSettings);
			}
			else { ParticleSystem.RotationBySpeedModule m = thisParticleSystem.rotationBySpeed; m.enabled = false; }

			if (psSettings.rotationOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.RotationOverLifetime);
				if (shouldCopy)
					CopyRotationOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.RotationOverLifetimeModule m = thisParticleSystem.rotationOverLifetime; m.enabled = false; }

			if (psSettings.shape.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Shape);
				if (shouldCopy)
					CopyShapeModuleFrom(psSettings);
			}
			else { ParticleSystem.ShapeModule m = thisParticleSystem.shape; m.enabled = false; }

			if (psSettings.sizeBySpeed.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.SizeBySpeed);
				if (shouldCopy)
					CopySizeBySpeedModuleFrom(psSettings);
			}
			else { ParticleSystem.SizeBySpeedModule m = thisParticleSystem.sizeBySpeed; m.enabled = false; }

			if (psSettings.sizeOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.SizeOverLifetime);
				if (shouldCopy)
					CopySizeOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.SizeOverLifetimeModule m = thisParticleSystem.sizeOverLifetime; m.enabled = false; }

			if (psSettings.subEmitters.enabled)
			{
				shouldCopy = isNewPS;
				// (no override affects this module)
				if (shouldCopy)
					CopySubEmittersModuleFrom(psSettings);
			}
			else { ParticleSystem.SubEmittersModule m = thisParticleSystem.subEmitters; m.enabled = false; }

			if (psSettings.textureSheetAnimation.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.TextureSheetAnimation);
				if (shouldCopy)
					CopyTextureSheetAnimationModuleFrom(psSettings);
			}
			else { ParticleSystem.TextureSheetAnimationModule m = thisParticleSystem.textureSheetAnimation; m.enabled = false; }

			if (psSettings.trails.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Trail);
				if (shouldCopy)
					CopyTrailModuleFrom(psSettings);
			}
			else { ParticleSystem.TrailModule m = thisParticleSystem.trails; m.enabled = false; }

			if (psSettings.trigger.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.Trigger);
				if (shouldCopy)
					CopyTriggerModuleFrom(psSettings);
			}
			else { ParticleSystem.TriggerModule m = thisParticleSystem.trigger; m.enabled = false; }

			if (psSettings.velocityOverLifetime.enabled)
			{
				shouldCopy = isNewPS;
				if (!shouldCopy) shouldCopy = IsOverridden(ShurikenModuleMask.VelocityOverLifetime);
				if (shouldCopy)
					CopyVelocityOverLifetimeModuleFrom(psSettings);
			}
			else { ParticleSystem.VelocityOverLifetimeModule m = thisParticleSystem.velocityOverLifetime; m.enabled = false; }
		}

		#region Copy each module property by property (Unity doesn't support something like GetModule<T>...)

		void CopyMainModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.MainModule m = thisParticleSystem.main;
			ParticleSystem.MainModule m2 = ps.main;

			m.customSimulationSpace = m2.customSimulationSpace;
			m.duration = m2.duration;
			m.gravityModifier = m2.gravityModifier;
			m.loop = m2.loop;
			m.maxParticles = m2.maxParticles;
			m.playOnAwake = m2.playOnAwake;
			m.prewarm = m2.prewarm;
			#if UNITY_2018_1_OR_NEWER
			m.flipRotation = m2.flipRotation;
			#else
			m.randomizeRotationDirection = m2.randomizeRotationDirection;
			#endif
			m.scalingMode = m2.scalingMode;
			m.simulationSpace = m2.simulationSpace;
			m.simulationSpeed = m2.simulationSpeed;
			m.startColor = m2.startColor;
			m.startDelay = m2.startDelay;
			m.startLifetime = m2.startLifetime;
			
			m.startRotation = m2.startRotation;
			m.startRotation3D = m2.startRotation3D;
			m.startRotationX = m2.startRotationX;
			m.startRotationY = m2.startRotationY;
			m.startRotationZ = m2.startRotationZ;
			
			m.startSize = m2.startSize;
			m.startSize3D = m2.startSize3D;
			m.startSizeX = m2.startSizeX;
			m.startSizeY = m2.startSizeY;
			m.startSizeZ = m2.startSizeZ;
			
			m.startSpeed = m2.startSpeed;
		}

		void CopyCollisionModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.CollisionModule m = thisParticleSystem.collision;
			ParticleSystem.CollisionModule m2 = ps.collision;

			m.enabled = true;

			m.bounce = m2.bounce;
			m.collidesWith = m2.collidesWith;
			m.dampen = m2.dampen;
			m.enableDynamicColliders = m2.enableDynamicColliders;
			//m.enableInteriorCollisions = m2.enableInteriorCollisions; // deprecated as of 2017.1
			m.lifetimeLoss = m2.lifetimeLoss;
			m.maxCollisionShapes = m2.maxCollisionShapes;
			m.maxKillSpeed = m2.maxKillSpeed;
			//m.maxPlaneCount = m2.maxPlaneCount; // is read only
			m.minKillSpeed = m2.minKillSpeed;
			m.mode = m2.mode;
			m.quality = m2.quality;
			m.radiusScale = m2.radiusScale;
			m.sendCollisionMessages = m2.sendCollisionMessages;
			m.type = m2.type;
			m.voxelSize = m2.voxelSize;

			// GetPlanes and SetPlanes could (or should) be called here, they're quite shady functions
		}

		void CopyColorBySpeedModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.ColorBySpeedModule m = thisParticleSystem.colorBySpeed;
			ParticleSystem.ColorBySpeedModule m2 = ps.colorBySpeed;

			m.enabled = true;

			m.color = m2.color;
			m.range = m2.range;
		}

		void CopyColorOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.ColorOverLifetimeModule m = thisParticleSystem.colorOverLifetime;
			ParticleSystem.ColorOverLifetimeModule m2 = ps.colorOverLifetime;

			m.enabled = true;
			m.color = m2.color;
		}

		void CopyEmissionModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.EmissionModule m = thisParticleSystem.emission;
			ParticleSystem.EmissionModule m2 = ps.emission;

			m.enabled = true;

			//m.burstCount = m2.burstCount; // is read only
			//m.rate = m2.rate; // is deprecated
			//m.rateMultiplier = m2.rateMultiplier; // is deprecated
			m.rateOverDistance = m2.rateOverDistance;
			m.rateOverTime = m2.rateOverTime;
			//m.type = m2.type; // is deprecated

			if (m2.burstCount == 0)
			{
				if (m.burstCount > 0)
					m.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0, 0) });
			}
			else // if (m2.burstCount > 0)
			{
				ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[m2.burstCount];
				m2.GetBursts(bursts);
				m.SetBursts(bursts);
			}
		}

		void CopyExternalForcesModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.ExternalForcesModule m = thisParticleSystem.externalForces;
			ParticleSystem.ExternalForcesModule m2 = ps.externalForces;

			m.enabled = true;
			m.multiplier = m2.multiplier;
		}

		void CopyForceOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.ForceOverLifetimeModule m = thisParticleSystem.forceOverLifetime;
			ParticleSystem.ForceOverLifetimeModule m2 = ps.forceOverLifetime;

			m.enabled = true;

			m.randomized = m2.randomized;
			m.space = m2.space;
			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;
		}

		void CopyInheritVelocityModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.InheritVelocityModule m = thisParticleSystem.inheritVelocity;
			ParticleSystem.InheritVelocityModule m2 = ps.inheritVelocity;

			m.enabled = true;

			m.curve = m2.curve;
			m.mode = m2.mode;
		}

		void CopyLightsModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.LightsModule m = thisParticleSystem.lights;
			ParticleSystem.LightsModule m2 = ps.lights;

			m.enabled = true;

			m.alphaAffectsIntensity = m2.alphaAffectsIntensity;
			m.intensity = m2.intensity;
			m.light = m2.light;
			m.maxLights = m2.maxLights;
			m.range = m2.range;
			m.ratio = m2.ratio;
			m.sizeAffectsRange = m2.sizeAffectsRange;
			m.useParticleColor = m2.useParticleColor;
			m.useRandomDistribution = m2.useRandomDistribution;
		}

		void CopyLimitVelocityOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.LimitVelocityOverLifetimeModule m = thisParticleSystem.limitVelocityOverLifetime;
			ParticleSystem.LimitVelocityOverLifetimeModule m2 = ps.limitVelocityOverLifetime;

			m.enabled = true;

			m.dampen = m2.dampen;
			m.limit = m2.limit;
			m.limitX = m2.limitX;
			m.limitY = m2.limitY;
			m.limitZ = m2.limitZ;
			m.separateAxes = m2.separateAxes;
			m.space = m2.space;
		}

		void CopyNoiseModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.NoiseModule m = thisParticleSystem.noise;
			ParticleSystem.NoiseModule m2 = ps.noise;

			m.enabled = true;

			m.damping = m2.damping;
			m.frequency = m2.frequency;
			m.octaveCount = m2.octaveCount;
			m.octaveScale = m2.octaveScale;
			m.quality = m2.quality;
			m.remap = m2.remap;
			m.remapEnabled = m2.remapEnabled;
			m.remapX = m2.remapX;
			m.remapY = m2.remapY;
			m.remapZ = m2.remapZ;
			m.scrollSpeed = m2.scrollSpeed;
			m.separateAxes = m2.separateAxes;
			m.strength = m2.strength;
			m.strengthX = m2.strengthX;
			m.strengthY = m2.strengthY;
			m.strengthZ = m2.strengthZ;
		}

		void CopyRotationBySpeedModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.RotationBySpeedModule m = thisParticleSystem.rotationBySpeed;
			ParticleSystem.RotationBySpeedModule m2 = ps.rotationBySpeed;

			m.enabled = true;
			m.range = m2.range;
			m.separateAxes = m2.separateAxes;
			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;
		}

		void CopyRotationOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.RotationOverLifetimeModule m = thisParticleSystem.rotationOverLifetime;
			ParticleSystem.RotationOverLifetimeModule m2 = ps.rotationOverLifetime;

			m.enabled = true;

			m.separateAxes = m2.separateAxes;
			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;
		}

		void CopyShapeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.ShapeModule m = thisParticleSystem.shape;
			ParticleSystem.ShapeModule m2 = ps.shape;

			m.enabled = true;

#if UNITY_5
		m.box = m2.box;
#else // new field name for Unity 2017
			m.scale = m2.scale;
#endif

			m.alignToDirection = m2.alignToDirection;
			m.angle = m2.angle;
			m.arc = m2.arc;
			m.length = m2.length;
			m.mesh = m2.mesh;
			m.meshMaterialIndex = m2.meshMaterialIndex;
			m.meshRenderer = m2.meshRenderer;
			//m.meshScale = m2.meshScale;
			m.scale = m2.scale; // .meshScale is now called .scale from 2017.1
			m.meshShapeType = m2.meshShapeType;
			m.normalOffset = m2.normalOffset;
			m.position = m2.position;
			m.rotation = m2.rotation;
			m.radius = m2.radius;
			//m.randomDirection = m2.randomDirection; // is deprecated
			m.randomDirectionAmount = m2.randomDirectionAmount;
			m.shapeType = m2.shapeType;
			m.skinnedMeshRenderer = m2.skinnedMeshRenderer;
			m.sphericalDirectionAmount = m2.sphericalDirectionAmount;
			m.useMeshColors = m2.useMeshColors;
			m.useMeshMaterialIndex = m2.useMeshMaterialIndex;
		}

		void CopySizeBySpeedModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.SizeBySpeedModule m = thisParticleSystem.sizeBySpeed;
			ParticleSystem.SizeBySpeedModule m2 = ps.sizeBySpeed;

			m.enabled = true;

			m.range = m2.range;
			m.separateAxes = m2.separateAxes;
			m.size = m2.size;
			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;
		}

		void CopySizeOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.SizeOverLifetimeModule m = thisParticleSystem.sizeOverLifetime;
			ParticleSystem.SizeOverLifetimeModule m2 = ps.sizeOverLifetime;

			m.enabled = true;

			m.separateAxes = m2.separateAxes;
			m.size = m2.size;
			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;
		}

		void CopySubEmittersModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.SubEmittersModule m = thisParticleSystem.subEmitters;
			ParticleSystem.SubEmittersModule m2 = ps.subEmitters;

			m.enabled = true;

			if (m.subEmittersCount > 0)
			{
				int max = m.subEmittersCount;
				for (int i = 0; i < max; i++)
					m.RemoveSubEmitter(0);
			}

			if (m2.subEmittersCount > 0)
			{
				int max = m2.subEmittersCount;
				for (int i = 0; i < max; i++)
					m.AddSubEmitter(m2.GetSubEmitterSystem(i), m2.GetSubEmitterType(i), m2.GetSubEmitterProperties(i));
			}
		}

		void CopyTextureSheetAnimationModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.TextureSheetAnimationModule m = thisParticleSystem.textureSheetAnimation;
			ParticleSystem.TextureSheetAnimationModule m2 = ps.textureSheetAnimation;

			m.enabled = true;

			m.animation = m2.animation;
			m.cycleCount = m2.cycleCount;
			#if UNITY_2018_3_OR_NEWER
			#else
			m.flipU = m2.flipU;
			m.flipV = m2.flipV;
			#endif
			m.frameOverTime = m2.frameOverTime;
			m.numTilesX = m2.numTilesX;
			m.numTilesY = m2.numTilesY;
			m.rowIndex = m2.rowIndex;
			m.startFrame = m2.startFrame;
			#if UNITY_2019_1_OR_NEWER
			m.rowMode = m2.rowMode;
			#else
			m.useRandomRow = m2.useRandomRow;
			#endif
			m.uvChannelMask = m2.uvChannelMask;
		}

		void CopyTrailModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.TrailModule m = thisParticleSystem.trails;
			ParticleSystem.TrailModule m2 = ps.trails;

			m.enabled = true;

			m.colorOverLifetime = m2.colorOverLifetime;
			m.colorOverTrail = m2.colorOverTrail;
			m.dieWithParticles = m2.dieWithParticles;
			m.inheritParticleColor = m2.inheritParticleColor;
			m.lifetime = m2.lifetime;
			m.minVertexDistance = m2.minVertexDistance;
			m.ratio = m2.ratio;
			m.sizeAffectsLifetime = m2.sizeAffectsLifetime;
			m.sizeAffectsWidth = m2.sizeAffectsWidth;
			m.textureMode = m2.textureMode;
			m.widthOverTrail = m2.widthOverTrail;
			m.worldSpace = m2.worldSpace;
		}

		void CopyTriggerModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.TriggerModule m = thisParticleSystem.trigger;
			ParticleSystem.TriggerModule m2 = ps.trigger;

			m.enabled = true;

			m.enter = m2.enter;
			m.exit = m2.exit;
			m.inside = m2.inside;
			m.outside = m2.outside;
			//m.maxColliderCount = m2.maxColliderCount; // is read only
			m.radiusScale = m2.radiusScale;

			#if UNITY_2020_2_OR_NEWER
			if (m.colliderCount > 0)
			#else
			if (m.maxColliderCount > 0)
			#endif
			{
				#if UNITY_2020_2_OR_NEWER
				int max = m.colliderCount;
				#else
				int max = m.maxColliderCount;
				#endif
				for (int i = 0; i < max; i++) m.SetCollider(i, null);
				// This line may cause a nullref exception. If so, commenting it can fix the issue,
				// but this VFX's trigger module wouldn't be usable again,
				// unless it's used with more colliders than the current value of maxColliderCount.
				// (We're talking about a rare, deep edge case here)
			}

			#if UNITY_2020_2_OR_NEWER
			if (m2.colliderCount > 0)
			#else
			if (m2.maxColliderCount > 0)
			#endif
			{
				#if UNITY_2020_2_OR_NEWER
				int max = m2.colliderCount;
				#else
				int max = m2.maxColliderCount;
				#endif
				for (int i = 0; i < max; i++)
					m.SetCollider(i, m2.GetCollider(i));
			}
		}

		void CopyVelocityOverLifetimeModuleFrom(ParticleSystem ps)
		{
			ParticleSystem.VelocityOverLifetimeModule m = thisParticleSystem.velocityOverLifetime;
			ParticleSystem.VelocityOverLifetimeModule m2 = ps.velocityOverLifetime;

			m.enabled = true;

			m.x = m2.x;
			m.y = m2.y;
			m.z = m2.z;

			#if UNITY_2018_1_OR_NEWER
			m.orbitalOffsetX = m2.orbitalOffsetX;
			m.orbitalOffsetY = m2.orbitalOffsetY;
			m.orbitalOffsetZ = m2.orbitalOffsetZ;
			m.orbitalX = m.orbitalX;
			m.orbitalY = m.orbitalY;
			m.orbitalZ = m.orbitalZ;
			m.radial = m.radial;
			m.space = m.space;
			m.speedModifier = m.speedModifier;
			#endif
		}

		void CopyParticleSystemRendererFrom(ParticleSystemRenderer psr)
		{
			if (!psr.enabled) { thisParticleRenderer.enabled = false; return; }

			thisParticleRenderer.enabled = true;

			// inherited from Renderer
			thisParticleRenderer.sortingOrder = psr.sortingOrder;
			thisParticleRenderer.sortingLayerID = psr.sortingLayerID;
			thisParticleRenderer.sortingLayerName = psr.sortingLayerName;
			thisParticleRenderer.materials = psr.sharedMaterials;
			thisParticleRenderer.receiveShadows = psr.receiveShadows;
			thisParticleRenderer.motionVectorGenerationMode = psr.motionVectorGenerationMode;
			thisParticleRenderer.lightmapIndex = psr.lightmapIndex;
			thisParticleRenderer.lightmapScaleOffset = psr.lightmapScaleOffset;

			// PSR properties
			thisParticleRenderer.alignment = psr.alignment;
			thisParticleRenderer.cameraVelocityScale = psr.cameraVelocityScale;
			thisParticleRenderer.lengthScale = psr.lengthScale;
			thisParticleRenderer.maxParticleSize = psr.maxParticleSize;
			thisParticleRenderer.mesh = psr.mesh;
			//thisParticleRenderer.meshCount = psr.meshCount; // is read only
			thisParticleRenderer.minParticleSize = psr.minParticleSize;
			thisParticleRenderer.normalDirection = psr.normalDirection;
			thisParticleRenderer.pivot = psr.pivot;
			thisParticleRenderer.renderMode = psr.renderMode;
			thisParticleRenderer.sortingFudge = psr.sortingFudge;
			thisParticleRenderer.sortMode = psr.sortMode;
			thisParticleRenderer.trailMaterial = psr.trailMaterial;
			thisParticleRenderer.velocityScale = psr.velocityScale;
			#if UNITY_2018_3_OR_NEWER
			thisParticleRenderer.flip = psr.flip;
			#endif

			if (psr.meshCount > 0)
			{
				Mesh[] meshes = new Mesh[psr.meshCount];
				psr.GetMeshes(meshes);
				thisParticleRenderer.SetMeshes(meshes);
			}
		}

#endregion
	
		#region Applying overrides to any module

		public void ApplyOverrides(List<BulletVFXOverride> overrides)
		{
			previouslyOverridenModules = 0;

			if (overrides == null) return;
			if (overrides.Count == 0) return;
			for (int i = 0; i < overrides.Count; i++)
				ApplyOverride(overrides[i]);
		}

		public void ApplyOverride(BulletVFXOverride vfxOverride)
		{
			BulletVFXParameterType paramType = vfxOverride.parameterToOverride;

			// save some lines by finding out generic stuff
			BulletVFXAtomicParameterType atomicParamType = BulletVFXOverride.GetAtomicParameterType(paramType);

			// exposing modules
			ParticleSystem.MainModule mm = thisParticleSystem.main;
			ParticleSystem.EmissionModule em = thisParticleSystem.emission;
			ParticleSystem.ShapeModule sm = thisParticleSystem.shape;
			ParticleSystem.VelocityOverLifetimeModule volm = thisParticleSystem.velocityOverLifetime;
			ParticleSystem.LimitVelocityOverLifetimeModule lvolm = thisParticleSystem.limitVelocityOverLifetime;
			ParticleSystem.InheritVelocityModule ivm = thisParticleSystem.inheritVelocity;
			ParticleSystem.ForceOverLifetimeModule folm = thisParticleSystem.forceOverLifetime;
			ParticleSystem.ColorOverLifetimeModule colm = thisParticleSystem.colorOverLifetime;
			ParticleSystem.ColorBySpeedModule cbsm = thisParticleSystem.colorBySpeed;
			ParticleSystem.SizeOverLifetimeModule solm = thisParticleSystem.sizeOverLifetime;
			ParticleSystem.SizeBySpeedModule sbsm = thisParticleSystem.sizeBySpeed;
			ParticleSystem.RotationOverLifetimeModule rolm = thisParticleSystem.rotationOverLifetime;
			ParticleSystem.RotationBySpeedModule rbsm = thisParticleSystem.rotationBySpeed;
			ParticleSystem.ExternalForcesModule efm = thisParticleSystem.externalForces;
			ParticleSystem.NoiseModule nm = thisParticleSystem.noise;
			ParticleSystem.CollisionModule cm = thisParticleSystem.collision;
			ParticleSystem.TriggerModule tm = thisParticleSystem.trigger;
			ParticleSystem.TextureSheetAnimationModule tsam = thisParticleSystem.textureSheetAnimation;
			ParticleSystem.LightsModule lm = thisParticleSystem.lights;
			ParticleSystem.TrailModule trm = thisParticleSystem.trails;

			switch (paramType)
			{
				#region Main module - common stuff
				
				case BulletVFXParameterType.MainModuleStartColor:
					mm.startColor = ComputeMinMaxGradient(vfxOverride, mm.startColor);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleSystemDuration:
					mm.duration = ComputeFloatValue(vfxOverride, mm.duration);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartLifetime:
					mm.startLifetime = ComputeMinMaxCurve(vfxOverride, mm.startLifetime);	
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartSpeed:
					mm.startSpeed = ComputeMinMaxCurve(vfxOverride, mm.startSpeed);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartSize:
					mm.startSize = ComputeMinMaxCurve(vfxOverride, mm.startSize);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartSizeX:
					mm.startSizeX = ComputeMinMaxCurve(vfxOverride, mm.startSizeX);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartSizeY:
					mm.startSizeY = ComputeMinMaxCurve(vfxOverride, mm.startSizeY);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartSizeZ:
					mm.startSizeZ = ComputeMinMaxCurve(vfxOverride, mm.startSizeZ);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleSimulationSpace:
					mm.simulationSpace = (ParticleSystemSimulationSpace)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleSimulationSpeed:
					mm.simulationSpeed = ComputeFloatValue(vfxOverride, mm.simulationSpeed);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleMaxParticles:
					mm.maxParticles = ComputeIntValue(vfxOverride, mm.maxParticles);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				#endregion

				#region Main module - rare stuff

				case BulletVFXParameterType.MainModulePrewarm:
					mm.prewarm = vfxOverride.boolValue;
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleLooping:
					mm.loop = vfxOverride.boolValue;
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartDelay:
					mm.startDelay = ComputeMinMaxCurve(vfxOverride, mm.startDelay);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleGravityModifier:
					mm.gravityModifier = ComputeMinMaxCurve(vfxOverride, mm.gravityModifier);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartRotation:
					mm.startRotation = ComputeMinMaxCurve(vfxOverride, mm.startRotation);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartRotationX:
					mm.startRotationX = ComputeMinMaxCurve(vfxOverride, mm.startRotationX);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartRotationY:
					mm.startRotationY = ComputeMinMaxCurve(vfxOverride, mm.startRotationY);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleStartRotationZ:
					mm.startRotationZ = ComputeMinMaxCurve(vfxOverride, mm.startRotationZ);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;
					
				case BulletVFXParameterType.MainModuleFlipRotation:
					mm.flipRotation = ComputeFloatValue(vfxOverride, mm.flipRotation);
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleScalingMode:
					mm.scalingMode = (ParticleSystemScalingMode)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				case BulletVFXParameterType.MainModuleCustomSimulationSpace:
					mm.customSimulationSpace = vfxOverride.objectReferenceValue as Transform;
					previouslyOverridenModules |= ShurikenModuleMask.Main;
					return;

				#endregion

				#region Emission module

				case BulletVFXParameterType.EmissionModuleRateOverTime:
					em.rateOverTime = ComputeMinMaxCurve(vfxOverride, em.rateOverTime);
					previouslyOverridenModules |= ShurikenModuleMask.Emission;
					return;

				case BulletVFXParameterType.EmissionModuleRateOverDistance:
					em.rateOverDistance = ComputeMinMaxCurve(vfxOverride, em.rateOverDistance);
					previouslyOverridenModules |= ShurikenModuleMask.Emission;
					return;

				#endregion

				#region Shape module - dimension

				case BulletVFXParameterType.ShapeModuleScale:
					sm.scale = ComputeVector3Value(vfxOverride, sm.scale);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleAlignToDirection:
					sm.alignToDirection = vfxOverride.boolValue;
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleArc:
					sm.arc = ComputeFloatValue(vfxOverride, sm.arc);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleLength:
					sm.length = ComputeFloatValue(vfxOverride, sm.length);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleNormalOffset:
					sm.normalOffset = ComputeFloatValue(vfxOverride, sm.normalOffset);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModulePosition:
					sm.position = ComputeVector3Value(vfxOverride, sm.position);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleRotation:
					sm.rotation = ComputeVector3Value(vfxOverride, sm.rotation);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleRadius:
					sm.radius = ComputeFloatValue(vfxOverride, sm.radius);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleRandomPositionAmount:
					sm.randomPositionAmount = ComputeFloatValue(vfxOverride, sm.randomPositionAmount);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleRandomDirectionAmount:
					sm.randomDirectionAmount = ComputeFloatValue(vfxOverride, sm.randomDirectionAmount);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleSphericalDirectionAmount:
					sm.sphericalDirectionAmount = ComputeFloatValue(vfxOverride, sm.sphericalDirectionAmount);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				#endregion

				#region Shape module - geometry

				case BulletVFXParameterType.ShapeModuleMesh:
					sm.mesh = vfxOverride.objectReferenceValue as Mesh;
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleMeshMaterialIndex:
					sm.meshMaterialIndex = ComputeIntValue(vfxOverride, sm.meshMaterialIndex);
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleMeshShapeType:
					sm.meshShapeType = (ParticleSystemMeshShapeType)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleShapeType:
					sm.shapeType = (ParticleSystemShapeType)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				case BulletVFXParameterType.ShapeModuleSkinnedMeshRenderer:
					sm.skinnedMeshRenderer = vfxOverride.objectReferenceValue as SkinnedMeshRenderer;
					previouslyOverridenModules |= ShurikenModuleMask.Shape;
					return;

				#endregion

				#region Velocity over lifetime

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetX:
					volm.orbitalOffsetX = ComputeMinMaxCurve(vfxOverride, volm.orbitalOffsetX);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetY:
					volm.orbitalOffsetY = ComputeMinMaxCurve(vfxOverride, volm.orbitalOffsetY);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalOffsetZ:
					volm.orbitalOffsetZ = ComputeMinMaxCurve(vfxOverride, volm.orbitalOffsetZ);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalX:
					volm.orbitalX = ComputeMinMaxCurve(vfxOverride, volm.orbitalX);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalY:
					volm.orbitalY = ComputeMinMaxCurve(vfxOverride, volm.orbitalY);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleOrbitalZ:
					volm.orbitalZ = ComputeMinMaxCurve(vfxOverride, volm.orbitalZ);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleRadial:
					volm.radial = ComputeMinMaxCurve(vfxOverride, volm.radial);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleSpace:
					volm.space = (ParticleSystemSimulationSpace)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleSpeedModifier:
					volm.speedModifier = ComputeMinMaxCurve(vfxOverride, volm.speedModifier);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleX:
					volm.x = ComputeMinMaxCurve(vfxOverride, volm.x);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleY:
					volm.y = ComputeMinMaxCurve(vfxOverride, volm.y);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				case BulletVFXParameterType.VelocityOverLifetimeModuleZ:
					volm.z = ComputeMinMaxCurve(vfxOverride, volm.z);
					previouslyOverridenModules |= ShurikenModuleMask.VelocityOverLifetime;
					return;

				#endregion

				#region Limit velocity over lifetime

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleDampen:
					lvolm.dampen = ComputeFloatValue(vfxOverride, lvolm.dampen);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleDrag:
					lvolm.drag = ComputeMinMaxCurve(vfxOverride, lvolm.drag);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimit:
					lvolm.limit = ComputeMinMaxCurve(vfxOverride, lvolm.limit);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitX:
					lvolm.limitX = ComputeMinMaxCurve(vfxOverride, lvolm.limitX);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitY:
					lvolm.limitY = ComputeMinMaxCurve(vfxOverride, lvolm.limitY);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleLimitZ:
					lvolm.limitZ = ComputeMinMaxCurve(vfxOverride, lvolm.limitZ);
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				case BulletVFXParameterType.LimitVelocityOverLifetimeModuleSpace:
					lvolm.space = (ParticleSystemSimulationSpace)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.LimitVelocityOverLifetime;
					return;

				#endregion

				#region Inherit velocity

				case BulletVFXParameterType.InheritVelocityModuleCurve:
					ivm.curve = ComputeMinMaxCurve(vfxOverride, ivm.curve);
					previouslyOverridenModules |= ShurikenModuleMask.InheritVelocity;
					return;

				case BulletVFXParameterType.InheritVelocityModuleMode:
					ivm.mode = (ParticleSystemInheritVelocityMode)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.InheritVelocity;
					return;

				#endregion

				#region Force over lifetime

				case BulletVFXParameterType.ForceOverLifetimeModuleX:
					folm.x = ComputeMinMaxCurve(vfxOverride, folm.x);
					previouslyOverridenModules |= ShurikenModuleMask.ForceOverLifetime;
					return;

				case BulletVFXParameterType.ForceOverLifetimeModuleY:
					folm.y = ComputeMinMaxCurve(vfxOverride, folm.y);
					previouslyOverridenModules |= ShurikenModuleMask.ForceOverLifetime;
					return;

				case BulletVFXParameterType.ForceOverLifetimeModuleZ:
					folm.z = ComputeMinMaxCurve(vfxOverride, folm.z);
					previouslyOverridenModules |= ShurikenModuleMask.ForceOverLifetime;
					return;

				case BulletVFXParameterType.ForceOverLifetimeModuleSpace:
					folm.space = (ParticleSystemSimulationSpace)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.ForceOverLifetime;
					return;

				#endregion

				#region Color over lifetime

				case BulletVFXParameterType.ColorOverLifetimeModule:
					colm.color = ComputeMinMaxGradient(vfxOverride, colm.color);
					previouslyOverridenModules |= ShurikenModuleMask.ColorOverLifetime;
					return;

				#endregion

				#region Color by speed

				case BulletVFXParameterType.ColorBySpeedModuleColor:
					cbsm.color = ComputeMinMaxGradient(vfxOverride, cbsm.color);
					previouslyOverridenModules |= ShurikenModuleMask.ColorBySpeed;
					return;

				case BulletVFXParameterType.ColorBySpeedModuleRange:
					cbsm.range = ComputeVector2Value(vfxOverride, cbsm.range);
					previouslyOverridenModules |= ShurikenModuleMask.ColorBySpeed;
					return;

				#endregion

				#region Size over lifetime

				case BulletVFXParameterType.SizeOverLifetimeModuleSize:
					solm.size = ComputeMinMaxCurve(vfxOverride, solm.size);
					previouslyOverridenModules |= ShurikenModuleMask.SizeOverLifetime;
					return;

				case BulletVFXParameterType.SizeOverLifetimeModuleX:
					solm.x = ComputeMinMaxCurve(vfxOverride, solm.x);
					previouslyOverridenModules |= ShurikenModuleMask.SizeOverLifetime;
					return;

				case BulletVFXParameterType.SizeOverLifetimeModuleY:
					solm.y = ComputeMinMaxCurve(vfxOverride, solm.y);
					previouslyOverridenModules |= ShurikenModuleMask.SizeOverLifetime;
					return;

				case BulletVFXParameterType.SizeOverLifetimeModuleZ:
					solm.z = ComputeMinMaxCurve(vfxOverride, solm.z);
					previouslyOverridenModules |= ShurikenModuleMask.SizeOverLifetime;
					return;

				#endregion

				#region Size by speed

				case BulletVFXParameterType.SizeBySpeedModuleRange:
					sbsm.range = ComputeVector2Value(vfxOverride, sbsm.range);
					previouslyOverridenModules |= ShurikenModuleMask.SizeBySpeed;
					return;

				case BulletVFXParameterType.SizeBySpeedModuleSize:
					sbsm.size = ComputeMinMaxCurve(vfxOverride, sbsm.size);
					previouslyOverridenModules |= ShurikenModuleMask.SizeBySpeed;
					return;

				case BulletVFXParameterType.SizeBySpeedModuleX:
					sbsm.x = ComputeMinMaxCurve(vfxOverride, sbsm.x);
					previouslyOverridenModules |= ShurikenModuleMask.SizeBySpeed;
					return;

				case BulletVFXParameterType.SizeBySpeedModuleY:
					sbsm.y = ComputeMinMaxCurve(vfxOverride, sbsm.y);
					previouslyOverridenModules |= ShurikenModuleMask.SizeBySpeed;
					return;

				case BulletVFXParameterType.SizeBySpeedModuleZ:
					sbsm.z = ComputeMinMaxCurve(vfxOverride, sbsm.z);
					previouslyOverridenModules |= ShurikenModuleMask.SizeBySpeed;
					return;

				#endregion

				#region Rotation over lifetime

				case BulletVFXParameterType.RotationOverLifetimeModuleX:
					rolm.x = ComputeMinMaxCurve(vfxOverride, rolm.x);
					previouslyOverridenModules |= ShurikenModuleMask.RotationOverLifetime;
					return;

				case BulletVFXParameterType.RotationOverLifetimeModuleY:
					rolm.y = ComputeMinMaxCurve(vfxOverride, rolm.y);
					previouslyOverridenModules |= ShurikenModuleMask.RotationOverLifetime;
					return;

				case BulletVFXParameterType.RotationOverLifetimeModuleZ:
					rolm.z = ComputeMinMaxCurve(vfxOverride, rolm.z);
					previouslyOverridenModules |= ShurikenModuleMask.RotationOverLifetime;
					return;

				#endregion

				#region Rotation by speed

				case BulletVFXParameterType.RotationBySpeedModuleRange:
					rbsm.range = ComputeVector2Value(vfxOverride, rbsm.range);
					previouslyOverridenModules |= ShurikenModuleMask.RotationBySpeed;
					return;

				case BulletVFXParameterType.RotationBySpeedModuleX:
					rbsm.x = ComputeMinMaxCurve(vfxOverride, rbsm.x);
					previouslyOverridenModules |= ShurikenModuleMask.RotationBySpeed;
					return;

				case BulletVFXParameterType.RotationBySpeedModuleY:
					rbsm.y = ComputeMinMaxCurve(vfxOverride, rbsm.y);
					previouslyOverridenModules |= ShurikenModuleMask.RotationBySpeed;
					return;

				case BulletVFXParameterType.RotationBySpeedModuleZ:
					rbsm.z = ComputeMinMaxCurve(vfxOverride, rbsm.z);
					previouslyOverridenModules |= ShurikenModuleMask.RotationBySpeed;
					return;

				#endregion

				#region External forces
				
				case BulletVFXParameterType.ExternalForcesModuleMultiplier:
					efm.multiplierCurve = ComputeMinMaxCurve(vfxOverride, efm.multiplierCurve);
					previouslyOverridenModules |= ShurikenModuleMask.ExternalForces;
					return;
				
				#endregion

				#region Noise

				case BulletVFXParameterType.NoiseModuleDamping:
					nm.damping = vfxOverride.boolValue;
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleFrequency:
					nm.frequency = ComputeFloatValue(vfxOverride, nm.frequency);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleOctaveCount:
					nm.octaveCount = ComputeIntValue(vfxOverride, nm.octaveCount);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleOctaveScale:
					nm.octaveScale = ComputeFloatValue(vfxOverride, nm.octaveScale);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleOctaveMultiplier:
					nm.octaveMultiplier = ComputeFloatValue(vfxOverride, nm.octaveMultiplier);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleQuality:
					nm.quality = (ParticleSystemNoiseQuality)vfxOverride.intValue;
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleRemap:
					nm.remap = ComputeMinMaxCurve(vfxOverride, nm.remap);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleRemapX:
					nm.remapX = ComputeMinMaxCurve(vfxOverride, nm.remapX);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleRemapY:
					nm.remapY = ComputeMinMaxCurve(vfxOverride, nm.remapY);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleRemapZ:
					nm.remapZ = ComputeMinMaxCurve(vfxOverride, nm.remapZ);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleScrollSpeed:
					nm.scrollSpeed = ComputeMinMaxCurve(vfxOverride, nm.scrollSpeed);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleSizeAmount:
					nm.sizeAmount = ComputeMinMaxCurve(vfxOverride, nm.sizeAmount);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleStrength:
					nm.strength = ComputeMinMaxCurve(vfxOverride, nm.strength);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleStrengthX:
					nm.strengthX = ComputeMinMaxCurve(vfxOverride, nm.strengthX);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleStrengthY:
					nm.strengthY = ComputeMinMaxCurve(vfxOverride, nm.strengthY);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				case BulletVFXParameterType.NoiseModuleStrengthZ:
					nm.strengthZ = ComputeMinMaxCurve(vfxOverride, nm.strengthZ);
					previouslyOverridenModules |= ShurikenModuleMask.Noise;
					return;

				#endregion

				#region Collision

				case BulletVFXParameterType.CollisionModuleBounce:
					cm.bounce = ComputeMinMaxCurve(vfxOverride, cm.bounce);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				case BulletVFXParameterType.CollisionModuleDampen:
					cm.dampen = ComputeMinMaxCurve(vfxOverride, cm.dampen);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				case BulletVFXParameterType.CollisionModuleLifetimeLoss:
					cm.lifetimeLoss = ComputeMinMaxCurve(vfxOverride, cm.lifetimeLoss);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				case BulletVFXParameterType.CollisionModuleMinKillSpeed:
					cm.minKillSpeed = ComputeFloatValue(vfxOverride, cm.minKillSpeed);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				case BulletVFXParameterType.CollisionModuleMaxKillSpeed:
					cm.maxKillSpeed = ComputeFloatValue(vfxOverride, cm.maxKillSpeed);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				case BulletVFXParameterType.CollisionModuleRadiusScale:
					cm.radiusScale = ComputeFloatValue(vfxOverride, cm.radiusScale);
					previouslyOverridenModules |= ShurikenModuleMask.Collision;
					return;

				#endregion

				#region Triggers

				case BulletVFXParameterType.TriggersModuleRadiusScale:
					tm.radiusScale = ComputeFloatValue(vfxOverride, tm.radiusScale);
					previouslyOverridenModules |= ShurikenModuleMask.Trigger;
					return;

				#endregion

				// (Skipping Sub-emitter module)
				#region Texture Sheet Animation

				case BulletVFXParameterType.TextureSheetAnimationModuleCycleCount:
					tsam.cycleCount = ComputeIntValue(vfxOverride, tsam.cycleCount);
					previouslyOverridenModules |= ShurikenModuleMask.TextureSheetAnimation;
					return;

				case BulletVFXParameterType.TextureSheetAnimationModuleFrameOverTime:
					tsam.frameOverTime = ComputeMinMaxCurve(vfxOverride, tsam.frameOverTime);
					previouslyOverridenModules |= ShurikenModuleMask.TextureSheetAnimation;
					return;

				case BulletVFXParameterType.TextureSheetAnimationModuleRowIndex:
					tsam.rowIndex = ComputeIntValue(vfxOverride, tsam.rowIndex);
					previouslyOverridenModules |= ShurikenModuleMask.TextureSheetAnimation;
					return;

				case BulletVFXParameterType.TextureSheetAnimationModuleStartFrame:
					tsam.startFrame = ComputeMinMaxCurve(vfxOverride, tsam.startFrame);
					previouslyOverridenModules |= ShurikenModuleMask.TextureSheetAnimation;
					return;

				#endregion

				#region Lights module

				case BulletVFXParameterType.LightsModuleIntensity:
					lm.intensity = ComputeMinMaxCurve(vfxOverride, lm.intensity);
					previouslyOverridenModules |= ShurikenModuleMask.Lights;
					return;

				case BulletVFXParameterType.LightsModuleLight:
					lm.light = vfxOverride.objectReferenceValue as Light;
					previouslyOverridenModules |= ShurikenModuleMask.Lights;
					return;

				case BulletVFXParameterType.LightsModuleRange:
					lm.range = ComputeMinMaxCurve(vfxOverride, lm.range);
					previouslyOverridenModules |= ShurikenModuleMask.Lights;
					return;

				case BulletVFXParameterType.LightsModuleRatio:
					lm.ratio = ComputeFloatValue(vfxOverride, lm.ratio);
					previouslyOverridenModules |= ShurikenModuleMask.Lights;
					return;

				#endregion

				#region Trail module

				case BulletVFXParameterType.TrailModuleRatio:
					trm.ratio = ComputeFloatValue(vfxOverride, trm.ratio);
					previouslyOverridenModules |= ShurikenModuleMask.Trail;
					return;

				case BulletVFXParameterType.TrailModuleLifetime:
					trm.lifetime = ComputeMinMaxCurve(vfxOverride, trm.lifetime);
					previouslyOverridenModules |= ShurikenModuleMask.Trail;
					return;

				case BulletVFXParameterType.TrailModuleWidthOverTrail:
					trm.widthOverTrail = ComputeMinMaxCurve(vfxOverride, trm.widthOverTrail);
					previouslyOverridenModules |= ShurikenModuleMask.Trail;
					return;

				case BulletVFXParameterType.TrailModuleColorOverLifetime:
					trm.colorOverLifetime = ComputeMinMaxGradient(vfxOverride, trm.colorOverLifetime);
					previouslyOverridenModules |= ShurikenModuleMask.Trail;
					return;

				case BulletVFXParameterType.TrailModuleColorOverTrail:
					trm.colorOverTrail = ComputeMinMaxGradient(vfxOverride, trm.colorOverTrail);
					previouslyOverridenModules |= ShurikenModuleMask.Trail;
					return;

				#endregion

				#region Renderer

				case BulletVFXParameterType.RendererSortingOrder:
					thisParticleRenderer.sortingOrder = ComputeIntValue(vfxOverride, thisParticleRenderer.sortingOrder);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererSortingLayerName:
					thisParticleRenderer.sortingLayerName = ComputeStringValue(vfxOverride, thisParticleRenderer.sortingLayerName);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererMaterial:
					thisParticleRenderer.material = vfxOverride.objectReferenceValue as Material;
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererTrailMaterial:
					thisParticleRenderer.trailMaterial = vfxOverride.objectReferenceValue as Material;
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererMinParticleSize:
					thisParticleRenderer.minParticleSize = ComputeFloatValue(vfxOverride, thisParticleRenderer.minParticleSize);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererMaxParticleSize:
					thisParticleRenderer.maxParticleSize = ComputeFloatValue(vfxOverride, thisParticleRenderer.maxParticleSize);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererFlip:
					thisParticleRenderer.flip = ComputeVector3Value(vfxOverride, thisParticleRenderer.flip);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				case BulletVFXParameterType.RendererPivot:
					thisParticleRenderer.pivot = ComputeVector3Value(vfxOverride, thisParticleRenderer.pivot);
					previouslyOverridenModules |= ShurikenModuleMask.Renderer;
					return;

				#endregion
			}
		}

		ParticleSystem.MinMaxCurve ComputeMinMaxCurve(BulletVFXOverride vfxOverride, ParticleSystem.MinMaxCurve baseValue)
		{
			baseValue.mode = vfxOverride.curveMode;
			if (vfxOverride.curveMode == ParticleSystemCurveMode.Constant)
				baseValue.constant = ComputeFloatValue(vfxOverride, baseValue.constant);
			else if (vfxOverride.curveMode == ParticleSystemCurveMode.Curve)
				baseValue.curve = vfxOverride.curveValue;
			else if (vfxOverride.curveMode == ParticleSystemCurveMode.TwoConstants)
			{
				// This wording automatically handles .OverrideBoth
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMax)
					baseValue.constantMin = ComputeFloatValue(vfxOverride, baseValue.constantMin);
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMin)
					baseValue.constantMax = ComputeFloatValue(vfxOverride, baseValue.constantMax);					
			}
			else if (vfxOverride.curveMode == ParticleSystemCurveMode.TwoCurves)
			{
				// This wording automatically handles .OverrideBoth
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMax)
					baseValue.curveMin = vfxOverride.curveValue;
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMin)
					baseValue.curveMax = vfxOverride.curveValue;							
			}

			return baseValue;
		}

		ParticleSystem.MinMaxGradient ComputeMinMaxGradient(BulletVFXOverride vfxOverride, ParticleSystem.MinMaxGradient baseValue)
		{
			baseValue.mode = vfxOverride.gradientMode;
			if (vfxOverride.gradientMode == ParticleSystemGradientMode.Gradient)
				baseValue.gradient = vfxOverride.gradientValue;
			else if (vfxOverride.gradientMode == ParticleSystemGradientMode.TwoGradients)
			{
				// This wording automatically handles .OverrideBoth
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMax)
					baseValue.gradientMin = vfxOverride.gradientValue;
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMin)
					baseValue.gradientMax = vfxOverride.gradientValue;
			}
			else if (vfxOverride.gradientMode == ParticleSystemGradientMode.TwoColors)
			{
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMax)
					baseValue.colorMin = ComputeColorValue(vfxOverride, baseValue.colorMin);
				if (vfxOverride.minMaxOverrideMode != MinMaxOverrideMode.OverrideMin)
					baseValue.colorMax = ComputeColorValue(vfxOverride, baseValue.colorMax);
			}
			// the following implies either .Color or .RandomColor
			else baseValue.color = ComputeColorValue(vfxOverride, baseValue.color);

			return baseValue;
		}

		float ComputeFloatValue(BulletVFXOverride vfxOverride, float baseValue)
		{
			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.ReplaceWith)
				return vfxOverride.floatValue;

			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.MultiplyBy)
				return vfxOverride.floatValue * baseValue;

			// assumes .Add
			return vfxOverride.floatValue + baseValue;
		}

		int ComputeIntValue(BulletVFXOverride vfxOverride, int baseValue)
		{
			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.ReplaceWith)
				return vfxOverride.intValue;

			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.MultiplyBy)
				return vfxOverride.intValue * baseValue;

			// assumes .Add
			return vfxOverride.intValue + baseValue;
		}

		Vector2 ComputeVector2Value(BulletVFXOverride vfxOverride, Vector2 baseValue)
		{
			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.ReplaceWith)
				return vfxOverride.vector2Value;

			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.MultiplyBy)
				return vfxOverride.vector2Value * baseValue;

			// assumes .Add
			return vfxOverride.vector2Value + baseValue;
		}

		Vector3 ComputeVector3Value(BulletVFXOverride vfxOverride, Vector3 baseValue)
		{
			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.ReplaceWith)
				return vfxOverride.vector3Value;

			if (vfxOverride.numberOverrideMode == VFXNumberOverrideMode.MultiplyBy)
				return new Vector3(
					vfxOverride.vector3Value.x * baseValue.x,
					vfxOverride.vector3Value.y * baseValue.y,
					vfxOverride.vector3Value.z * baseValue.z);

			// assumes .Add
			return vfxOverride.vector3Value + baseValue;
		}

		Color ComputeColorValue(BulletVFXOverride vfxOverride, Color baseValue)
		{
			if (vfxOverride.colorOverrideMode == VFXColorOverrideMode.ReplaceWith)
				return vfxOverride.colorValue;

			if (vfxOverride.colorOverrideMode == VFXColorOverrideMode.Add)
				return vfxOverride.colorValue + baseValue;

			if (vfxOverride.colorOverrideMode == VFXColorOverrideMode.AlphaBlend)
			{
				float invA = 1f-vfxOverride.colorValue.a;
				return new Color(
					baseValue.r*invA+vfxOverride.colorValue.r*vfxOverride.colorValue.a,
					baseValue.g*invA+vfxOverride.colorValue.g*vfxOverride.colorValue.a,
					baseValue.b*invA+vfxOverride.colorValue.b*vfxOverride.colorValue.a,
					baseValue.a+vfxOverride.colorValue.a*(1f-baseValue.a));
			}

			if (vfxOverride.colorOverrideMode == VFXColorOverrideMode.Subtract)
				return baseValue - vfxOverride.colorValue;

			if (vfxOverride.colorOverrideMode == VFXColorOverrideMode.MultiplyBy)
				return vfxOverride.colorValue * baseValue;

			// assumes .Average
			return new Color(
				(baseValue.r + vfxOverride.colorValue.r)*0.5f,
				(baseValue.g + vfxOverride.colorValue.g)*0.5f,
				(baseValue.b + vfxOverride.colorValue.b)*0.5f,
				(baseValue.a + vfxOverride.colorValue.a)*0.5f);			
		}

		string ComputeStringValue(BulletVFXOverride vfxOverride, string baseValue)
		{
			if (vfxOverride.stringOverrideMode == VFXStringOverrideMode.ReplaceWith)
				return vfxOverride.stringValue;

			// assumes Append
			return baseValue + vfxOverride.stringValue;
		}

		#endregion
	}
}