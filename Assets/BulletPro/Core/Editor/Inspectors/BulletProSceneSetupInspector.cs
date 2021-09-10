using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
	[CustomEditor(typeof(BulletProSceneSetup))]
	public class BulletProSceneSetupInspector : Editor
	{
		SerializedProperty buildNumber;
		SerializedProperty makePersistentBetweenScenes;
		SerializedProperty enableGizmo, gizmoColor;

		void OnEnable()
		{
			buildNumber = serializedObject.FindProperty("buildNumber");
			makePersistentBetweenScenes = serializedObject.FindProperty("makePersistentBetweenScenes");
			enableGizmo = serializedObject.FindProperty("enableGizmo");
			gizmoColor = serializedObject.FindProperty("gizmoColor");

			// Build number in the scene setup exists since version 11 :
			if (buildNumber.intValue < 11)
			{
				buildNumber.intValue = 11;
				serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}
		}

		public override void OnInspectorGUI()
		{
			if (buildNumber.intValue > BulletProSettings.buildNumber)
			{
				EditorGUILayout.HelpBox("This object is marked as created in a BulletPro build number which is superior to your current version.\nThis is most likely an error.\nPlease try replacing this Scene Setup object with a new one.", MessageType.Error);
				return;
			}

			serializedObject.Update();

			EditorGUILayout.PropertyField(makePersistentBetweenScenes);
			EditorGUILayout.PropertyField(enableGizmo);
			if (enableGizmo.boolValue)
				EditorGUILayout.PropertyField(gizmoColor);

			//EditorGUILayout.LabelField("This Scene Setup has been created under Bullet Pro build version "+buildNumber.intValue.ToString()+".");

			EditorGUILayout.HelpBox("This object just needs to exist once in your scene.\n"+
            "Your whole gameplay is contained in this object's XY plane.\n"+
            "You can rotate this object to rotate your gameplay.", MessageType.Info);

			serializedObject.ApplyModifiedProperties();
		}
	}
}

