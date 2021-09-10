using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(BulletReceiver))]
	public class BulletReceiverInspector : Editor
	{
		BulletReceiver[] brs;
		SerializedProperty colliderType, hitboxSize, hitboxOffset, startingChildren;
		SerializedProperty parentSyncFoldout, syncEnable, syncDisable, syncCollisionTags;
		SerializedProperty killBulletOnCollision, maxSimultaneousCollisionsPerFrame;
		SerializedProperty collisionTags, collisionTagsFoldout, OnHitByBullet, gizmoColor, gizmoZOffset;
		SerializedProperty advancedEventsFoldout, OnHitByBulletEnter, OnHitByBulletStay, OnHitByBulletExit;

		bool lockBehaviour, lockEvents;

		void OnEnable()
		{
			brs = new BulletReceiver[targets.Length];
			for (int i = 0; i < brs.Length; i++)
				brs[i] = targets[i] as BulletReceiver;


			//self = serializedObject.FindProperty("self");
			colliderType = serializedObject.FindProperty("colliderType");
			hitboxSize = serializedObject.FindProperty("hitboxSize");
			hitboxOffset = serializedObject.FindProperty("hitboxOffset");
			startingChildren = serializedObject.FindProperty("startingChildren");
			killBulletOnCollision = serializedObject.FindProperty("killBulletOnCollision");
			maxSimultaneousCollisionsPerFrame = serializedObject.FindProperty("maxSimultaneousCollisionsPerFrame");
			collisionTags = serializedObject.FindProperty("collisionTags");
			collisionTagsFoldout = serializedObject.FindProperty("collisionTagsFoldout");
			OnHitByBullet = serializedObject.FindProperty("OnHitByBullet");
			gizmoColor = serializedObject.FindProperty("gizmoColor");
			gizmoZOffset = serializedObject.FindProperty("gizmoZOffset");

			parentSyncFoldout = serializedObject.FindProperty("parentSyncFoldout");
			syncEnable = serializedObject.FindProperty("syncEnable");
			syncDisable = serializedObject.FindProperty("syncDisable");
			syncCollisionTags = serializedObject.FindProperty("syncCollisionTags");
			
			advancedEventsFoldout = serializedObject.FindProperty("advancedEventsFoldout");
			OnHitByBulletEnter = serializedObject.FindProperty("OnHitByBulletEnter");
			OnHitByBulletStay = serializedObject.FindProperty("OnHitByBulletStay");
			OnHitByBulletExit = serializedObject.FindProperty("OnHitByBulletExit");

			// Find out if this Receiver is used as a child of another Receiver in the scene
			lockBehaviour = false;
			lockEvents = false;
			Object[] allReceivers = Resources.FindObjectsOfTypeAll(typeof(BulletReceiver));
			if (allReceivers == null) return;
			if (allReceivers.Length == 0) return;
			for (int i = 0; i < allReceivers.Length; i++)
			{
				BulletReceiver br = allReceivers[i] as BulletReceiver;
				if (br.colliderType != BulletReceiverType.Composite) continue;
				if (br.startingChildren == null) continue;
				if (br.startingChildren.Length == 0) continue;
				for (int j = 0; j < br.startingChildren.Length; j++)
				{
					for (int k = 0; k < brs.Length; k++)
					{
						if (br.startingChildren[j] == brs[k])
						{
							lockBehaviour = true;
							lockEvents = true;
							break;
						}
					}
					if (lockBehaviour) break;
				}
				if (lockBehaviour) break;
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			//EditorGUILayout.Space();
			//EditorGUILayout.LabelField("Basic Collision Info", EditorStyles.boldLabel);
			//EditorGUILayout.PropertyField(self);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(colliderType);
			if (colliderType.enumValueIndex == (int)BulletReceiverType.Composite)
			{
				EditorGUILayout.PropertyField(startingChildren, new GUIContent(startingChildren.displayName, "Select the BulletReceivers that should be a part of this one."));
				parentSyncFoldout.boolValue = EditorGUILayout.Foldout(parentSyncFoldout.boolValue, new GUIContent("Composite Sync Options", "Display options to manage behaviour of the children Receivers."), true);
				if (parentSyncFoldout.boolValue)
				{
					EditorGUI.indentLevel += 2;
					EditorGUILayout.PropertyField(syncEnable, new GUIContent(syncEnable.displayName, "Enable all children when this Receiver gets enabled."));
					EditorGUILayout.PropertyField(syncDisable, new GUIContent(syncDisable.displayName, "Disable all children when this Receiver gets disabled."));
					EditorGUILayout.PropertyField(syncCollisionTags, new GUIContent(syncCollisionTags.displayName, "Copy this Receiver's Collision Tags into its children upon parenting them, and at Awake.\nWarning: if you manually modify CollisionTags in your code, you will still have to manually call SyncCollisionTags() !"));
					EditorGUI.indentLevel -= 2;
				}
			}
			else
			{
				EditorGUILayout.PropertyField(hitboxSize);
				EditorGUILayout.PropertyField(hitboxOffset);
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
			if (lockBehaviour)
			{
				string msg = "Some of the selected BulletReceivers are used as part of a Composite BulletReceiver. Their behaviour is irrelevant : you should edit it in the Composite Receiver instead.";
				if (targets.Length == 1) msg = "This BulletReceiver is used as part of a Composite BulletReceiver. Its behaviour is irrelevant : you should edit it in the Composite Receiver instead.";
				EditorGUILayout.HelpBox(msg, MessageType.Info);
				if (GUILayout.Button("Access this behaviour anyway")) lockBehaviour = false;
			}
			else
			{
				EditorGUILayout.PropertyField(killBulletOnCollision);
				EditorGUILayout.PropertyField(maxSimultaneousCollisionsPerFrame, new GUIContent("Max Collisions Per Frame", "If more bullets strike this Receiver at once in a single frame, excess collisions will be negated. 0 means Infinity."));

				collisionTagsFoldout.boolValue = EditorGUILayout.Foldout(collisionTagsFoldout.boolValue, "Collision Tags", true);
				if (collisionTagsFoldout.boolValue)
				{
					EditorGUILayout.PropertyField(collisionTags);
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUIUtility.currentViewWidth*0.4f);
					Color defC = GUI.color;
					GUI.color = new Color(0.6f, 1f, 1f, 1f);
					if (GUILayout.Button("Manage Tags", EditorStyles.miniButton))
					{
						BulletProSettings bcs = Resources.Load("BulletProSettings") as BulletProSettings;
						if (bcs == null)
							bcs = BulletProAssetCreator.CreateCollisionSettingsAsset();
						else
						{
							#if UNITY_2018_3_OR_NEWER
							SettingsService.OpenProjectSettings("Project/Bullet Pro");
							#else
							EditorGUIUtility.PingObject(bcs);
							EditorUtility.FocusProjectWindow();
							Selection.activeObject = bcs;
							#endif
						}
					}
					GUI.color = defC;
					EditorGUILayout.LabelField("");
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.Space();

				int hasEmptyTags = 0;
				int hasCorrectTags = 0;
				for (int i = 0; i < brs.Length; i++)
				{
					if (brs[i].collisionTags.tagList == 0)
						hasEmptyTags++;
					else hasCorrectTags++;
				}
				if (hasEmptyTags > 0)
				{
					string str = "Selected object has no Collision Tags. It won't collide with anything.";
					if (brs.Length > 1)
					{
						if (hasEmptyTags == 1)
							str = "One of the selected objects has no Collision Tags. It won't collide with anything.";
						else if (hasCorrectTags > 0)
							str = "Some of the selected objects have no Collision Tags. They won't collide with anything.";
						else
							str = "Selected objects have no Collision Tags. They won't collide with anything.";
					}
					if (!collisionTagsFoldout.boolValue)
						str += "\nYou may need to unfold and activate the Collision Tags above.";
					else
						str += "\nYou may need to click on some Collision Tags above.";
					EditorGUILayout.HelpBox(str, MessageType.Warning);
				}
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
			if (lockEvents)
			{
				string msg = "Some of the selected BulletReceivers are used as part of a Composite BulletReceiver. Their events will not trigger : you should edit events from the Composite Receiver instead.";
				if (targets.Length == 1) msg = "This BulletReceiver is used as part of a Composite BulletReceiver. Its events will not trigger : you should edit events from the Composite Receiver instead.";
				EditorGUILayout.HelpBox(msg, MessageType.Info);
				if (GUILayout.Button("Access these events anyway")) lockEvents = false;
			}
			else
			{
				EditorGUILayout.PropertyField(OnHitByBullet);

				advancedEventsFoldout.boolValue = EditorGUILayout.Foldout(advancedEventsFoldout.boolValue, "Advanced Events (Enter, Stay, Exit)", true);
				if (advancedEventsFoldout.boolValue)
				{
					EditorGUILayout.Space();
					EditorGUILayout.PropertyField(OnHitByBulletEnter);
					EditorGUILayout.PropertyField(OnHitByBulletStay);
					EditorGUILayout.PropertyField(OnHitByBulletExit);
				}
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Gizmo", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(gizmoColor);
			EditorGUILayout.PropertyField(gizmoZOffset);

			serializedObject.ApplyModifiedProperties();

			for (int i = 0; i < brs.Length; i++)
				if (brs[i].self == null)
				{
					brs[i].self = brs[i].transform;
					EditorUtility.SetDirty(brs[i]);
				}
		}
	}
}
