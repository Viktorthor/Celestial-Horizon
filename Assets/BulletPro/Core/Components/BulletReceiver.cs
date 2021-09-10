using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	[System.Serializable]
	public class HitByBulletEvent : UnityEvent<Bullet, Vector3> { }

	public enum BulletReceiverType { Circle, Line, Composite }

	// A hitbox that sends events when hit by bullets. Has OnEnter, OnStay and OnExit.
	// Be careful though, as a same instance can only send one OnEnter, one OnStay and one OnExit per frame, regardless of the bullet.
	[AddComponentMenu("BulletPro/Bullet Receiver")]
	public class BulletReceiver : MonoBehaviour
	{
		public Transform self;
		public BulletReceiverType colliderType = BulletReceiverType.Circle;

		// If a Receiver A has a parent B, it means B is composite and A is part of it.
		// Being a child means the receiver calls no event on its own, the parent does it instead.
		public BulletReceiver[] startingChildren;
		public List<BulletReceiver> children { get; private set; }
		public BulletReceiver parent { get; private set; }
		public bool syncEnable = true;
		public bool syncDisable = true;
		public bool syncCollisionTags = true;

		public float hitboxSize = 0.1f;
		public Vector2 hitboxOffset = Vector2.zero;
		public bool killBulletOnCollision = true;
		[Tooltip("If more bullets strike this Receiver at once, excess collisions will be negated. 0 means Infinity.")]
		public uint maxSimultaneousCollisionsPerFrame = 1;
		public List<Bullet> bulletsHitThisFrame { get; private set; }
		public List<Bullet> bulletsHitLastFrame { get; private set; }
		public CollisionTags collisionTags;

		public HitByBulletEvent OnHitByBullet;

		// Yes, they do exist, and are usable, but they're most likely confusing and rarely useful - yet might be needed in some cases.
		public HitByBulletEvent OnHitByBulletEnter, OnHitByBulletStay, OnHitByBulletExit;

		#if UNITY_EDITOR
		public bool parentSyncFoldout;
		public bool collisionTagsFoldout;
		public bool advancedEventsFoldout;
		public Color gizmoColor = Color.black;
		public float gizmoZOffset = 0f;
		#endif

		bool collisionEnabled; // helps avoiding bullets to hit this if we just disabled collisions

		// We need a reference to call it frequently, so we know if Compute Shaders are enabled
		private BulletCollisionManager collisionManager;

#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if (!enabled) return;
			if (colliderType == BulletReceiverType.Composite) return;

			if (!self) self = transform;
			//float avgScale = thisTransform.lossyScale.x * 0.5f + thisTransform.lossyScale.y * 0.5f;
			// there's no point in taking scale.x into account
			float avgScale = self.lossyScale.y;
			Gizmos.color = gizmoColor;
			Vector3 trPos = self.position;
			if (gizmoZOffset != 0) trPos += self.lossyScale.z * gizmoZOffset * self.forward;
			if (hitboxOffset.x != 0) trPos += self.lossyScale.x * hitboxOffset.x * self.right;
			if (hitboxOffset.y != 0) trPos += self.lossyScale.y * hitboxOffset.y * self.up;

			if (colliderType == BulletReceiverType.Circle)
				Gizmos.DrawWireSphere(trPos, hitboxSize * avgScale);
			else
				Gizmos.DrawLine(trPos, trPos + self.up * hitboxSize * avgScale);
		}
#endif

		public void Awake()
		{
			if (self == null) self = transform;
			bulletsHitThisFrame = new List<Bullet>();
			bulletsHitLastFrame = new List<Bullet>();

			children = new List<BulletReceiver>();
			if (startingChildren != null)
				if (startingChildren.Length > 0)
					for (int i = 0; i < startingChildren.Length; i++)
						startingChildren[i]?.SetParent(this);

			// wait one frame so that the managers exist. Start is unreliable since it can be disabled
			StartCoroutine(PostAwake());
		}

		IEnumerator PostAwake()
		{
			yield return new WaitForEndOfFrame();

			// No loop needed here, because every child does it on its own
			if (enabled) EnableCollisions();
			else collisionEnabled = false;

			collisionManager = BulletCollisionManager.instance;
		}

		// Bullet memory and Exit event are handled when the collisions are not processed - for the GPU mode, it's in Update.
		public void Update()
		{
			if (collisionManager == null)
			{
				collisionManager = BulletCollisionManager.instance;
				if (collisionManager == null)
				{
					Debug.LogError("BulletPro Error: no Collision Manager found in scene. Try redoing the Scene Setup, or check if your Manager was disabled during this object's Awake.");
					return;
				}
			}
			
			if (!collisionManager.disableComputeShaders)
				BulletMemoryUpdate();
		}

		// Bullet memory and Exit event are handled when the collisions are not processed - for the CPU mode, it's in LateUpdate.
		public void LateUpdate()
		{
			if (collisionManager == null)
			{
				collisionManager = BulletCollisionManager.instance;
				if (collisionManager == null)
				{
					Debug.LogError("BulletPro Error: no Collision Manager found in scene. Try redoing the Scene Setup, or check if your Manager was disabled during this object's Awake.");
					return;
				}
			}

			if (collisionManager.disableComputeShaders)
				BulletMemoryUpdate();
		}

		// Called at either Update or LateUpdate
		void BulletMemoryUpdate()
		{
			// Process Exit event
			if (OnHitByBulletExit != null)
				if (bulletsHitLastFrame.Count > 0)
					for (int i = 0; i < bulletsHitLastFrame.Count; i++)
						if (!bulletsHitThisFrame.Contains(bulletsHitLastFrame[i]))
							OnHitByBulletExit.Invoke(bulletsHitLastFrame[i], bulletsHitLastFrame[i].self.position);

			// Flush lists
			bulletsHitLastFrame.Clear();
			bulletsHitLastFrame.TrimExcess();
			if (bulletsHitThisFrame.Count > 0)
				for (int i = 0; i < bulletsHitThisFrame.Count; i++)
					bulletsHitLastFrame.Add(bulletsHitThisFrame[i]);
			bulletsHitThisFrame.Clear();
			bulletsHitThisFrame.TrimExcess();
		}

		// Handles parenting in both ways.
		public void SetParent(BulletReceiver newParent)
		{
			// Can't parent to itself
			if (newParent == this) return;

			// Can't add the same twice
			if (parent == newParent) return;

			// Can't make a loop
			if (newParent != null)
			{
				bool loops = false;
				BulletReceiver temp = newParent;
				while (temp.parent != null)
				{
					if (temp.parent == this)
					{
						loops = true;
						break;
					}
					temp = temp.parent;
				}
				if (loops) return;
			}

			// Unregister as child of previous parent, if any
			parent?.children?.Remove(this);

			// Actual assignation
			parent = newParent;

			if (newParent == null) return;

			// Register as child of new parent
			newParent.children?.Add(this);

			// Sync collisionTags if needed
			if (newParent.syncCollisionTags)
				collisionTags = newParent.collisionTags;
		}

		// Copies collisionTags into children. Should be manually called if collisionTags are manually changed.
		public void SyncCollisionTags()
		{
			if (children == null) return;
			if (children.Count > 0) return;
			for (int i = 0; i < children.Count; i++)
				if (children[i] != null)
					children[i].collisionTags = collisionTags;
		}

		// Called on collision : OnEnter and OnStay are handled if needed, but it will rarely be the case.
		// Signature says "Vector3, Bullet" instead of "Bullet, Vector3" so events cannot call this and enter an infinite loop.
		public void GetHit(Vector3 collisionPoint, Bullet bullet)
		{
			bulletsHitThisFrame.Add(bullet);
			OnHitByBullet?.Invoke(bullet, collisionPoint);

			// Compute Enter/Stay events here
			if (!bulletsHitLastFrame.Contains(bullet))
			{
				OnHitByBulletEnter?.Invoke(bullet, collisionPoint);

				// Also handle collision VFX here, if any
				for (int i = 0; i < bullet.moduleVFX.availableVFX.Count; i++)
				{
					BulletVFXTrigger trig = bullet.moduleVFX.availableVFX[i].onCollision;
					if (!bullet.moduleVFX.AskPermission(trig)) continue;
					if (trig.behaviour == BulletVFXBehaviour.Play) bullet.moduleVFX.PlayVFX(i);
					else if (trig.behaviour == BulletVFXBehaviour.Stop) bullet.moduleVFX.StopVFX(i);
				}
			}
			else OnHitByBulletStay?.Invoke(bullet, collisionPoint);

			// Kill bullet if needed
			if (killBulletOnCollision && bullet.moduleCollision.dieOnCollision)
			{
				if (bullet.moduleCollision.deathTiming == BulletDeathTiming.Immediately)
					bullet.Die(collisionPoint);
				else // if AtEndOfFrame
					bullet.moduleCollision.ScheduleDeath(collisionPoint);
			}
		}

		#region information getters

		// Has this receiver reach its collision amount limit for this frame?
		public bool CanAcceptCollisionsThisFrame()
		{
			return (maxSimultaneousCollisionsPerFrame < 1) || (bulletsHitThisFrame.Count < maxSimultaneousCollisionsPerFrame);
		}

		public bool HasAlreadyCollidedThisFrame(Bullet bullet)
		{
			return bulletsHitThisFrame.Contains(bullet);
		}

		#endregion

		// Since toggling .enabled is way more intuitive, this whole toolbox is private
		#region collision toggle toolbox

		protected void EnableCollisions()
		{
			if (collisionEnabled) return;
			collisionEnabled = true;
			BulletCollisionManager.AddReceiver(this);
		}

		protected void DisableCollisions()
		{
			if (!collisionEnabled) return;
			collisionEnabled = false;
			BulletCollisionManager.RemoveReceiver(this);
		}

		protected void ToggleCollisions()
		{
			if (collisionEnabled) DisableCollisions();
			else EnableCollisions();
		}

		protected void SetCollisions(bool active)
		{
			if (active) EnableCollisions();
			else DisableCollisions();

			if (active && !syncEnable) return;
			if (!active && !syncDisable) return;

			if (children != null)
				if (children.Count > 0)
					for (int i = 0; i < children.Count; i++)
						children[i]?.SetCollisions(active);
		}

		#endregion

		void OnEnable()
		{
			if (BulletCollisionManager.instance == null) return;
			SetCollisions(true);
		}

		void OnDisable()
		{
			if (BulletCollisionManager.instance == null) return;
			SetCollisions(false);

			// Uncomment this to trigger OnCollisionExit if needed
			//BulletMemoryUpdate();

			// Flush lists
			bulletsHitLastFrame.Clear();
			bulletsHitLastFrame.TrimExcess();
			bulletsHitThisFrame.Clear();
			bulletsHitThisFrame.TrimExcess();
		}

		// Not recommended, but we can't leave it into the manager (or in its parent) if it gets destroyed
		void OnDestroy()
		{
			collisionEnabled = false;
			BulletCollisionManager.RemoveReceiver(this);

			// If it has a parent :
			parent?.children?.Remove(this);
			// If it has children :
			if (children != null)
				while (children.Count > 0)
					children[0].SetParent(null);
		}
	}
}