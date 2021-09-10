using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	public enum PlayOptions { AllBullets, RootAndSubEmitters, RootOnly, DoNothing }
	public enum KillOptions { AllBullets, AllSubEmitters, AllBulletsButRoot, AllSubEmittersButRoot, RootOnly, EndlessPatternsOnly, DoNotKillAnything }

	[AddComponentMenu("BulletPro/Bullet Emitter")]
	public class BulletEmitter : MonoBehaviour
	{
		[Header("Gameplay")]
		public EmitterProfile emitterProfile;
		private BulletParams firstBulletParams;
		public Transform patternOrigin;
		public bool playAtStart;

		// Bullet Emitter is considered "playing" if at least one sub-emitter is still playing.
		public bool isPlaying
		{
			get
			{
				if (subEmitters == null) return false;
				else if (subEmitters.Count == 0) return false;
				else for (int i = 0; i < subEmitters.Count; i++)
				{
					if (subEmitters[i].modulePatterns.isPlaying)
						return true;
				}
				return false;
			}
			// Note : This script contains commented occurrences of "isPlaying=true" and "isPlaying=false".
			// This is a leftover from the old isPlaying behaviour that may be reused in the future.
		}

		public bool allBulletsPaused { get; private set; }

		private bool isInitialized;
		private bool justSwitchedProfile; // retains this info to abort calling Kill() upon detecting a new profile

		// emitters shot in the tree, bullets shot in the tree
		[System.NonSerialized]
		public List<Bullet> subEmitters, bullets;

		// a fast access to this emitter's Root Bullet
		public Bullet rootBullet
		{
			get
			{
				if (subEmitters == null) return null;
				if (subEmitters.Count == 0) return null;
				return subEmitters[0];
			}
		}

		#if UNITY_EDITOR
		[Header("Gizmo")]
		public Color gizmoColor = Color.green;
		public float gizmoSize = 0.5f;

		private void OnDrawGizmos()
		{
			if (gizmoSize == 0) return;
			if (patternOrigin == null) return;

			Color oldCol = Gizmos.color;
			Matrix4x4 oldMat = Gizmos.matrix;
			Gizmos.color = gizmoColor;
			Gizmos.matrix = patternOrigin.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, gizmoSize);

			// three parallel lines
			/* */
			Vector3 leftLinePos = Vector3.left * gizmoSize * 0.5f;
			Vector3 rightLinePos = Vector3.right * gizmoSize * 0.5f;
			Vector3 upLinePos = Vector3.up * gizmoSize * 0.5f;
			Gizmos.DrawLine(leftLinePos, leftLinePos + Vector3.up * gizmoSize * 4f);
			Gizmos.DrawLine(rightLinePos, rightLinePos + Vector3.up * gizmoSize * 4f);
			Gizmos.DrawLine(upLinePos, upLinePos + Vector3.up * gizmoSize * 4f);
			/* */

			// arrow tip + simple angles
			/* *
			Vector3 arrowTip = Vector3.up * 4 * gizmoSize;

			Gizmos.DrawLine(Vector3.zero, new Vector3(-1.5f, 1.5f, 0) * gizmoSize);
			Gizmos.DrawLine(Vector3.zero, new Vector3(-1.5f, 3, 0) * gizmoSize);
			Gizmos.DrawLine(Vector3.zero, arrowTip);
			Gizmos.DrawLine(Vector3.zero, new Vector3(1.5f, 3, 0) * gizmoSize);
			Gizmos.DrawLine(Vector3.zero, new Vector3(1.5f, 1.5f, 0) * gizmoSize);

			Gizmos.DrawLine(arrowTip, arrowTip + new Vector3(-1, -2, 0) * gizmoSize * 0.3f);
			Gizmos.DrawLine(arrowTip, arrowTip + new Vector3(1, -2, 0) * gizmoSize * 0.3f);
			/* */

			// multiple arrows (90° and 45° angle)
			/* *
			DrawArrow(-1f, 1f);
			DrawArrow(-1f, 2f);
			DrawArrow(0f, 4f);
			DrawArrow(1f, 2f);
			DrawArrow(1f, 1f);
			/* */

			Gizmos.color = oldCol;
			Gizmos.matrix = oldMat;
		}

		// Currently unused, but could be
		void DrawArrow(float tx, float ty)
		{
			Vector3 tip = new Vector3(tx, ty, 0) * gizmoSize;

			// tweakable :
			float angle = 0.05f; // radians
			float sideCut = 0.9f;

			// non-tweakable :
			float c = Mathf.Cos(angle);
			float s = Mathf.Sin(angle);
			Vector3 leftBit = new Vector3(tip.x*c-tip.y*s, tip.x*s+tip.y*c);
			c = Mathf.Cos(-angle);
			s = Mathf.Sin(-angle);
			Vector3 rightBit = new Vector3(tip.x*c-tip.y*s, tip.x*s+tip.y*c);

			Gizmos.DrawLine(Vector3.zero, tip);
			Gizmos.DrawLine(tip, leftBit * sideCut);
			Gizmos.DrawLine(tip, rightBit * sideCut);
		}
		#endif

		void Awake()
		{
			subEmitters = new List<Bullet>();
			bullets = new List<Bullet>();
			justSwitchedProfile = false;
		}

		void Start()
		{
			if (!patternOrigin) patternOrigin = transform;
			StartCoroutine(PostStart());
		}

		// Wait one frame so that any bullet has the managers. Delayed- functions wait for this before they work.
		IEnumerator PostStart()
		{
			yield return new WaitForEndOfFrame();
			isInitialized = true;
			if (playAtStart) Play();
			yield return null;
		}

		// Very first initiator function of a danmaku tree. Called by controls if the tree does not exist. (so, the first time)
		// By "tree", we mean : Initiator -> bullet -> pattern -> shot -> bullet -> pattern -> ...
		// Basically a ripoff of Bullet.EmitSingleBullet().
		// Important note: b.Prepare() calls Boot() so it does "Play" the pattern, not just "Init" it.
		// Returns whether it succeeded in creating this very first bullet.
		bool Launch()
		{
			if (!emitterProfile)
			{
				// This warning is commented out, because it spammed the console in rather common cases.
				// Feel free to restore this.
				//Debug.LogWarning("BulletPro Warning: " + name + " can\'t launch pattern, EmitterProfile is missing!");
				return false;
			}

			#if UNITY_EDITOR
			if (emitterProfile.buildNumber != BulletProSettings.buildNumber)
			{
				Debug.LogWarning("BulletPro Warning: " + emitterProfile.name + " has the wrong version number. Please update it from Tools > BulletPro > Update Assets.");
				return false;
			}
			#endif

			firstBulletParams = emitterProfile.rootBullet;

			if (!firstBulletParams)
			{
				Debug.LogWarning("BulletPro Warning: " + name + " can\'t launch pattern, EmitterProfile lacks Root Bullet!");
				return false;
			}

			// Fetching a fresh new bullet from the pools
			Bullet b = null;
			if (firstBulletParams.renderMode == BulletRenderMode.Sprite)
			{
				b = BulletPoolManager.GetFreeBullet();
				
				// in case of fail, fallback to the other pool if bullet is invisible, because 2D or 3D won't matter
				if ((!b) && (!firstBulletParams.isVisible))
					b = BulletPoolManager.GetFree3DBullet();
			}
			else
			{
				b = BulletPoolManager.GetFree3DBullet();

				// in case of fail, fallback to the other pool if bullet is invisible, because 2D or 3D won't matter
				if ((!b) && (!firstBulletParams.isVisible))
					b = BulletPoolManager.GetFreeBullet();
			}
			if (!b)
			{
				Debug.LogWarning(name + " can\'t launch pattern, available Bullets are missing!");
				return false;
			}

			b.self.position = patternOrigin.position;
			b.self.eulerAngles = patternOrigin.eulerAngles;

			// setting up inherited data for dynamic parameters
			InheritedSpawnData isd = new InheritedSpawnData();
			isd.randomSeed = Random.value;
			isd.bulletID = firstBulletParams.uniqueIndex;
			isd.positionInShot = Vector2.zero;
			isd.rotationAtSpawn = b.self.localEulerAngles.z;
			isd.randomValues = b.dynamicSolver.inheritedData.randomValues;
			b.dynamicSolver.inheritedData = isd;

			if (b.dynamicSolver.SolveDynamicBool(firstBulletParams.isChildOfEmitter, 26448033, ParameterOwner.Bullet))
				b.self.SetParent(patternOrigin);
			else
			{
				if (b.renderMode == BulletRenderMode.Sprite)
					b.self.SetParent(BulletPoolManager.regularPoolContainer);
				else
					b.self.SetParent(BulletPoolManager.meshPoolContainer);
			}

			b.ApplyBulletParams(firstBulletParams);

			// Set orientation if homing and not delayed
			if (firstBulletParams.homing)
			{
				bool delayed = b.moduleSpawn.isEnabled && (b.moduleSpawn.timeBeforeSpawn > 0);
				if (!delayed)
				{
					b.moduleHoming.MoveToTarget(b.moduleHoming.spawnOnTarget);
					b.moduleHoming.LookAtTarget(b.moduleHoming.homingSpawnRate);
				}
			}

			// Instantiate additional behaviour if needed.
			// Done here instead of ApplyBulletParams() to avoid behaviour stacking if one calls ChangeBulletParams().
			if (firstBulletParams.behaviourPrefabs != null)
				if (firstBulletParams.behaviourPrefabs.Length > 0)
					for (int i = 0; i < firstBulletParams.behaviourPrefabs.Length; i++)
					{
						GameObject bpPrefab = b.dynamicSolver.SolveDynamicObjectReference(firstBulletParams.behaviourPrefabs[i], 16163779, ParameterOwner.Bullet) as GameObject;

						if (bpPrefab == null) continue;

						BaseBulletBehaviour bbb = BulletBehaviourManager.GetFreeBehaviour(bpPrefab);
						bbb.bullet = b;
						bbb.OnBulletBirth();
					}

			b.emitter = this;
			// newly-created root bullet is registered as subEmitter in every case, even if it has no pattern
			subEmitters.Add(b);
			b.isRootBullet = true; // this bool prevents it from re-registering as subEmitter later on
			bullets.Add(b);

			allBulletsPaused = false;

			//isPlaying = true; // makes up for not calling it earlier, otherwise the first Play() call wouldn't flip it

			b.FirstPrepare(false);

			return true;
		}

		// Allows switching EmitterProfile without killing already-emitted bullets.
		// Simply setting .emitterProfile works as well, but it automatically calls Kill() in the process.
		// Also flushes any link between the emitter and its previous bullets, so their scripts can no longer use data from this emitter.
		public void SwitchProfile(
			EmitterProfile newProfile,
			bool forgetAboutPreviousBullets = false,
			PlayOptions stopPreviousEmission = PlayOptions.RootAndSubEmitters,
			PlayOptions playNextEmission = PlayOptions.RootAndSubEmitters,
			KillOptions killPreviousBullets = KillOptions.DoNotKillAnything)
		{
			Kill(killPreviousBullets);
			Stop(stopPreviousEmission);

			// Messing with the subEmitters list would lead to complex, unwanted behaviours from the previous bullet tree.
			// So we just reset it and cut links to previous subEmitters, so retroactive control is made impossible.
			subEmitters.Clear();
			subEmitters.TrimExcess();
			
			// Forgetting about bullets means all links between bullets and this emitter are cut.
			// If enabled, even killing them with this.Kill() will become impossible.
			if (forgetAboutPreviousBullets)
			{
				if (bullets.Count > 0)
				for (int i = 0; i < bullets.Count; i++)
					bullets[i].emitter = null;
				
				bullets.Clear();
				bullets.TrimExcess();
			}

			// This boolean tells the emitter not to call Kill() at next Play call.
			justSwitchedProfile = true;
			emitterProfile = newProfile;
			if (emitterProfile != null)
				Play(playNextEmission);
		}

		#region Global controls on whole danmaku tree

		#region public tag-based functions

		// Get, among bullets (indirectly) fired by this emitter, all the ones whose active Patterns have wanted tag.
		// Warning : if SwitchProfile() has been used and the bullets List hasn't been flushed, this can retrieve bullets from the previous profile.
		public List<Bullet> FindWithPatternTag(string patternTag)
		{
			List<Bullet> result = new List<Bullet>();
			if (bullets == null) return result;
			if (bullets.Count == 0) return result;
			for (int i = 0; i < bullets.Count; i++)
			{
				PatternRuntimeInfo[] pri = bullets[i].modulePatterns.patternRuntimeInfo;
				if (pri == null) continue;
				if (pri.Length == 0) continue;
				bool found = false;
				for (int j = 0; j < pri.Length; j++)
				{
					if (!pri[j].HasTag(patternTag)) continue;
					found = true;
					break;
				}
				if (found) result.Add(bullets[i]);
			}

			return result;
		}

		// Gets all bullets to call Play on a certain pattern tag
		public void PlayPatternTag(string patternTag)
		{
			if (subEmitters[0] != null)
				subEmitters[0].modulePatterns.Play(patternTag);
		}

		// Gets all bullets to call Pause on a certain pattern tag
		public void PausePatternTag(string patternTag)
		{
			if (subEmitters[0] != null)
				subEmitters[0].modulePatterns.Pause(patternTag);
		}

		// Gets all bullets to call Reset on a certain pattern tag
		public void ResetPatternTag(string patternTag)
		{
			if (subEmitters[0] != null)
				subEmitters[0].modulePatterns.ResetPattern(patternTag);
		}

		// Gets all bullets to call Stop on a certain pattern tag
		public void StopPatternTag(string patternTag)
		{
			if (subEmitters[0] != null)
				subEmitters[0].modulePatterns.Stop(patternTag);
		}

		// Gets all bullets to call Boot on a certain pattern tag
		public void BootPatternTag(string patternTag)
		{
			if (subEmitters[0] != null)
				subEmitters[0].modulePatterns.Boot(patternTag);
		}

		#endregion

		// Pipeline : this.Foo() calls bullet.modulePatterns.Foo(), which calls RuntimePatternInfo.Foo().
		// this.FooAll() does the same but kills (or pauses) the whole tree beforehand.

		#region public control functions

		public void Play(PlayOptions range = PlayOptions.RootAndSubEmitters)
		{
			if (range == PlayOptions.DoNothing) return;

			// Emitter being not initialized means we are calling Play() from Start or Awake.
			// In this case, the script must wait one frame so it gets all the managers.
			if (!isInitialized)
			{
				if (gameObject.activeInHierarchy)
					StartCoroutine(DelayedPlay(range));
				return;
			}

			if (emitterProfile == null) return;
			if (emitterProfile.rootBullet != firstBulletParams)
			{
				if (justSwitchedProfile) justSwitchedProfile = false;
				else Kill();
				Launch();
				return;
			}
			
			if (range == PlayOptions.RootOnly) PlaySingle();
			else
			{
				if (range == PlayOptions.AllBullets)
					allBulletsPaused = false;
				PlayAll();
			}
		}

		public void Pause(PlayOptions range = PlayOptions.RootAndSubEmitters)
		{
			if (range == PlayOptions.DoNothing) return;

			if (!isInitialized)
			{
				if (gameObject.activeInHierarchy)
					StartCoroutine(DelayedPause(range));
				return;
			}
			if (range == PlayOptions.RootOnly) PauseSingle();
			else
			{
				if (range == PlayOptions.AllBullets)
					allBulletsPaused = true;
				PauseAll();
			}
		}

		public void Reinitialize(PlayOptions range = PlayOptions.RootAndSubEmitters)
		{
			if (range == PlayOptions.DoNothing) return;

			if (!isInitialized)
			{
				if (gameObject.activeInHierarchy)
					StartCoroutine(DelayedReinitialize(range));
				return;
			}

			if (range == PlayOptions.RootOnly) ResetSingle();
			else
			{
				if (range == PlayOptions.AllBullets)
					allBulletsPaused = false;
				ResetAll();
			}
		}

		public void Boot(PlayOptions range = PlayOptions.RootAndSubEmitters)
		{
			if (range == PlayOptions.DoNothing) return;

			if (!isInitialized)
			{
				if (gameObject.activeInHierarchy)
					StartCoroutine(DelayedBoot(range));
				return;
			}

			if (emitterProfile == null) return;
			if (emitterProfile.rootBullet != firstBulletParams)
			{
				if (justSwitchedProfile) justSwitchedProfile = false;
				else Kill();
				Launch();
				return;
			}

			if (range == PlayOptions.RootOnly) BootSingle();
			else
			{
				if (range == PlayOptions.AllBullets)
					allBulletsPaused = false;
				BootAll();
			}
		}

		public void Stop(PlayOptions range = PlayOptions.RootAndSubEmitters)
		{
			if (range == PlayOptions.DoNothing) return;

			if (!isInitialized)
			{
				if (gameObject.activeInHierarchy)
					StartCoroutine(DelayedStop(range));
				return;
			}
			if (range == PlayOptions.RootOnly) StopSingle();
			else
			{
				if (range == PlayOptions.AllBullets)
					allBulletsPaused = true;
				StopAll();
			}
		}

		#endregion

		#region control functions delayed for initialization

		// Waits until the managers exist before calling Play
		IEnumerator DelayedPlay(PlayOptions range)
		{
			while (!isInitialized) yield return new WaitForEndOfFrame();
			Play(range);
		}

		// Waits until the managers exist before calling Pause
		IEnumerator DelayedPause(PlayOptions range)
		{
			while (!isInitialized) yield return new WaitForEndOfFrame();
			Pause(range);
		}

		// Waits until the managers exist before calling Reinitialize
		IEnumerator DelayedReinitialize(PlayOptions range)
		{
			while (!isInitialized) yield return new WaitForEndOfFrame();
			Reinitialize(range);
		}

		// Waits until the managers exist before calling Boot
		IEnumerator DelayedBoot(PlayOptions range)
		{
			while (!isInitialized) yield return new WaitForEndOfFrame();
			Boot(range);
		}

		// Waits until the managers exist before calling Stop
		IEnumerator DelayedStop(PlayOptions range)
		{
			while (!isInitialized) yield return new WaitForEndOfFrame();
			Stop(range);
		}

		#endregion
		
		#region private functions

		// Plays, or resume, the root emitter.
		void PlaySingle()
		{
			//isPlaying = true;

			if (subEmitters.Count == 0) { Launch(); return; }

			subEmitters[0].modulePatterns.Play();
		}

		// Pauses the root emitter.
		void PauseSingle()
		{
			//isPlaying = false;

			if (subEmitters.Count == 0) { Launch(); StopAll(); return; }

			subEmitters[0].modulePatterns.Pause();
		}

		// Simply resets root emitter to start time, without calling Play or Pause
		void ResetSingle()
		{
			if (subEmitters.Count == 0) { Launch(); StopAll(); return; }

			subEmitters[0].modulePatterns.ResetPattern();
		}

		// Interrupts the root and resets it to its start time. Stop = Pause + ResetRoot.
		void StopSingle()
		{
			//isPlaying = false;

			if (subEmitters.Count == 0) { Launch(); StopAll(); return; }

			subEmitters[0].modulePatterns.Stop();
		}

		// Resets the root to its start time and plays it. Boot = ResetRoot + Play.
		void BootSingle()
		{
			//isPlaying = true;

			if (subEmitters.Count == 0) { Launch(); return; }

			subEmitters[0].modulePatterns.Boot();
		}

		// Plays, or resume, the whole pattern.
		void PlayAll()
		{
			//isPlaying = true;

			if (subEmitters.Count == 0) { Launch(); return; }

			for (int i = 0; i < subEmitters.Count; i++)
				subEmitters[i].modulePatterns.Play();
		}

		// Pauses the whole pattern.
		void PauseAll()
		{
			//isPlaying = false;

			if (subEmitters.Count == 0) { Launch(); StopAll(); return; }

			for (int i = 0; i < subEmitters.Count; i++)
				subEmitters[i].modulePatterns.Pause();
		}

		// Simply resets whole pattern to start time, without calling Play or Pause
		void ResetAll()
		{
			if (subEmitters.Count == 0) { Launch(); StopAll(); return; }

			KillAllEmittersButRoot();
			subEmitters[0].modulePatterns.ResetPattern();
		}

		// Interrupts the whole pattern and resets it to its start time. StopAll = PauseAll + ResetAll.
		void StopAll()
		{
			//isPlaying = false;

			// Checks the return value of Launch(), so it can't go into an infinite loop
			if (subEmitters.Count == 0) { if (Launch()) StopAll(); return; }

			KillAllEmittersButRoot();
			subEmitters[0].modulePatterns.Stop();
		}

		// Resets the whole pattern to its start time and plays it. Boot = ResetPattern + Play.
		void BootAll()
		{
			//isPlaying = true;

			if (subEmitters.Count == 0) { Launch(); return; }

			KillAllEmittersButRoot();
			subEmitters[0].modulePatterns.Boot();
		}

		#endregion

		#endregion

		#region kill functions

		public void Kill(KillOptions killOptions = KillOptions.AllBullets)
		{
			if (killOptions == KillOptions.DoNotKillAnything) return;

			if (!isInitialized) return;

			if (killOptions == KillOptions.AllSubEmitters) KillEmitters();
			if (killOptions == KillOptions.AllBulletsButRoot) KillAllBulletsButRoot();
			if (killOptions == KillOptions.AllSubEmittersButRoot) KillAllEmittersButRoot();
			if (killOptions == KillOptions.RootOnly) KillRootOnly();
			if (killOptions == KillOptions.EndlessPatternsOnly) KillEndlessPatterns();
			if (killOptions == KillOptions.AllBullets) KillAllBullets();
		}

		// Kill functions do not flip the isPlaying flag, unless they take down every sub-emitter involved.

		// Ends the whole current pattern tree, including root.
		void KillEmitters()
		{
			//isPlaying = false;

			if (subEmitters.Count == 0) return;

			List<Bullet> all = new List<Bullet>();
			
			for (int i = 0; i < subEmitters.Count; i++)
				all.Add(subEmitters[i]);
			for (int i = 0; i < all.Count; i++)
				all[i].Die(true);
		}

		// Ends all subEmitters emitted by the root pattern.
		void KillAllEmittersButRoot()
		{
			if (subEmitters.Count < 2) return;

			List<Bullet> all = new List<Bullet>();
			for (int i = 1; i < subEmitters.Count; i++)
				all.Add(subEmitters[i]);
			for (int i = 0; i < all.Count; i++)
				all[i].Die(true);
		}

		// Ends all bullets emitted by the root pattern.
		void KillAllBulletsButRoot()
		{
			if (bullets.Count < 2) return;
			Bullet curRootBullet = null;
			if (subEmitters.Count > 0) curRootBullet = subEmitters[0];

			List<Bullet> all = new List<Bullet>();
			for (int i = 0; i < bullets.Count; i++)
				if (bullets[i] != curRootBullet)
					all.Add(bullets[i]);
			for (int i = 0; i < all.Count; i++)
				all[i].Die(true);
		}

		// Ends the first pattern only ; if it has emitted other emitters, they will remain.
		void KillRootOnly()
		{
			if (subEmitters.Count == 0) return;
			subEmitters[0].Die(true);
		}

		// Ends all patterns that wouldn't finish by themselves. Not called here, but could be useful elsewhere.
		void KillEndlessPatterns()
		{
			if (subEmitters.Count == 0) return;

			List<Bullet> mustDie = new List<Bullet>();
			for (int i = 0; i < subEmitters.Count; i++)
			{
				if (!subEmitters[i].moduleLifespan.isEnabled)
				{
					bool isEndless = false;
					if (subEmitters[i].modulePatterns.patternsShot != null)
						if (subEmitters[i].modulePatterns.patternsShot.Length > 0)
							for (int j = 0; j < subEmitters[i].modulePatterns.patternsShot.Length; j++)
								if (subEmitters[i].modulePatterns.patternsShot[j].IsEndless())
									isEndless = true;
					if (isEndless) mustDie.Add(subEmitters[i]);
				}
			}

			for (int i = 0; i < mustDie.Count; i++)
				mustDie[i].Die(true);
		}

		// Downright kills all bullets involved in tree.
		void KillAllBullets()
		{
			//isPlaying = false;

			if (bullets.Count == 0) return;

			List<Bullet> all = new List<Bullet>();
			for (int i = 0; i < bullets.Count; i++)
				all.Add(bullets[i]);
			for (int i = 0; i < all.Count; i++)
				all[i].Die(true);
		}

		#endregion
	}
}
