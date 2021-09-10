using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
    // A class that caches references to serialized properties.
    // Works like an extension of BulletParamInspector.cs.
    public class BulletVFXParamInspector
    {
        // Main property, points to the object being inspected
        SerializedProperty prop;

        // Main properties and triggers
        SerializedProperty tagProp, attachToBulletTransform, useDefaultParticles, particleSystemPrefab;
        SerializedProperty onBulletBirth, onVisible, onInvisible, onCollision;
        SerializedProperty onPatternShoot, onBulletDeath, vfxOverrides;

        // Shortcuts for common overrides
        SerializedProperty replaceColorWithBulletColor;
        SerializedProperty replaceSizeWithNumber, sizeNewValue;
        SerializedProperty multiplySizeWithNumber, sizeMultiplier;
        SerializedProperty multiplySizeWithBulletScale, multiplySpeedWithBulletScale;

        // Reorderable list of overrides
        ReorderableList rlist;

        // The equivalent of OnEnable.
        public void Initialize(SerializedProperty propToLoad)
        {
            prop = propToLoad;

            tagProp = prop.FindPropertyRelative("tag");
            attachToBulletTransform = prop.FindPropertyRelative("attachToBulletTransform");
			useDefaultParticles = prop.FindPropertyRelative("useDefaultParticles");
			particleSystemPrefab = prop.FindPropertyRelative("particleSystemPrefab");

			onBulletBirth = prop.FindPropertyRelative("onBulletBirth");
			onVisible = prop.FindPropertyRelative("onVisible");
			onInvisible = prop.FindPropertyRelative("onInvisible");
			onCollision = prop.FindPropertyRelative("onCollision");
			onPatternShoot = prop.FindPropertyRelative("onPatternShoot");
			onBulletDeath = prop.FindPropertyRelative("onBulletDeath");
			vfxOverrides = prop.FindPropertyRelative("vfxOverrides");

            replaceColorWithBulletColor = prop.FindPropertyRelative("replaceColorWithBulletColor");
            replaceSizeWithNumber = prop.FindPropertyRelative("replaceSizeWithNumber");
            sizeNewValue = prop.FindPropertyRelative("sizeNewValue");
            multiplySizeWithNumber = prop.FindPropertyRelative("multiplySizeWithNumber");
            sizeMultiplier = prop.FindPropertyRelative("sizeMultiplier");
            multiplySizeWithBulletScale = prop.FindPropertyRelative("multiplySizeWithBulletScale");
            multiplySpeedWithBulletScale = prop.FindPropertyRelative("multiplySpeedWithBulletScale");

            rlist = new ReorderableList(prop.serializedObject, vfxOverrides, true, true, true, true);
            rlist.drawHeaderCallback = (Rect rect) =>
			{
				EditorGUI.LabelField(rect, new GUIContent("Advanced Shuriken Overrides, from top to bottom:", "If you fill this list, you can choose any parameter from this effect's Particle System, and override it with another value. This new value can be based on various bullet parameters."), EditorStyles.boldLabel);
			};
			rlist.drawElementCallback = OverrideDrawer;
			rlist.onRemoveCallback += (ReorderableList list) =>
			{
				vfxOverrides.DeleteArrayElementAtIndex(list.index);
			};
			rlist.onAddCallback += (ReorderableList list) =>
			{
				vfxOverrides.arraySize++;
				if (vfxOverrides.arraySize==1)
				{
					SerializedProperty newProp = vfxOverrides.GetArrayElementAtIndex(0);
					SerializedProperty col = newProp.FindPropertyRelative("colorValue");
					SerializedProperty curve = newProp.FindPropertyRelative("curveValue");
					DynamicParameterUtility.SetFixedAnimationCurve(curve, AnimationCurve.EaseInOut(0, 0, 1, 1), true);
					DynamicParameterUtility.SetFixedColor(col, Color.white, true);
					prop.serializedObject.ApplyModifiedProperties();
				}
			};
			rlist.elementHeightCallback += (int index) =>
			{
				float lines = 2;
				float verticalAdd = 4;
				return rlist.elementHeight * lines + verticalAdd;
			};
        }

        public void DrawVFXInspector()
		{
			EditorGUILayout.PropertyField(tagProp, new GUIContent("Tag", "Give a tag to this effect so you remember it. This tag can also be used to call the VFX from your code."));
			EditorGUILayout.PropertyField(attachToBulletTransform, new GUIContent("Attach to Bullet Transform", "If checked, the VFX will follow the bullet throughout its lifetime."));
			EditorGUILayout.PropertyField(useDefaultParticles, new GUIContent(useDefaultParticles.displayName, "If true, this bullet's VFX will be a copy of the default Particle System from your Scene Setup's BulletVFXManager. If false, you will provide a specific Particle System prefab instead."));
			if (!useDefaultParticles.boolValue)
			{
				EditorGUI.indentLevel += 2;
				EditorGUILayout.PropertyField(particleSystemPrefab, new GUIContent(particleSystemPrefab.displayName, "This bullet's VFX will be a copy of the provided Particle System."));
				EditorGUI.indentLevel -= 2;
			}

            EditorGUILayout.Space(12);
			EditorGUILayout.LabelField("When to play or stop this effect:", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(onBulletBirth, new GUIContent(onBulletBirth.displayName, "Trigger a VFX when the bullet is created."));
			EditorGUILayout.PropertyField(onBulletDeath, new GUIContent(onBulletDeath.displayName, "Trigger a VFX when the bullet dies."));
			EditorGUILayout.PropertyField(onCollision, new GUIContent(onCollision.displayName, "Trigger a VFX when the bullet hits a Receiver."));
			EditorGUILayout.PropertyField(onPatternShoot, new GUIContent(onPatternShoot.displayName, "Trigger a VFX when the bullet shoots other bullets."));
			EditorGUILayout.PropertyField(onVisible, new GUIContent(onVisible.displayName, "Trigger a VFX when the bullet's sprite/mesh becomes visible."));
			EditorGUILayout.PropertyField(onInvisible, new GUIContent(onInvisible.displayName, "Trigger a VFX when the bullet's sprite/mesh becomes invisible."));
			EditorGUI.indentLevel -= 1;

			EditorGUILayout.Space(12);
			EditorGUILayout.LabelField("Override particle parameters based on the bullet:", EditorStyles.boldLabel);

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            float targetLW = 210;
            if (EditorGUIUtility.labelWidth < targetLW)
                EditorGUIUtility.labelWidth = targetLW;
			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(replaceColorWithBulletColor, new GUIContent(replaceColorWithBulletColor.displayName, "Tint particle system's Start Color with color from the bullet's Renderer."));
            EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(replaceSizeWithNumber, new GUIContent(replaceSizeWithNumber.displayName, "Override particle system's Start Size with another value."));
            if (replaceSizeWithNumber.boolValue)
                EditorGUILayout.PropertyField(sizeNewValue, GUIContent.none);
            EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(multiplySizeWithNumber, new GUIContent(multiplySizeWithNumber.displayName, "Multiply particle system's Start Size with another value."));
            if (multiplySizeWithNumber.boolValue)
                EditorGUILayout.PropertyField(sizeMultiplier, GUIContent.none);
            EditorGUILayout.EndHorizontal();
			EditorGUILayout.PropertyField(multiplySizeWithBulletScale, new GUIContent(multiplySizeWithBulletScale.displayName, "Multiply particle system's Start Size with this bullet's scale. Recommended for visual consistency."));
			EditorGUILayout.PropertyField(multiplySpeedWithBulletScale, new GUIContent(multiplySpeedWithBulletScale.displayName, "Multiply particle system's Start Speed with another value. Recommended so the particles keep the same aspect at every scale."));
			EditorGUI.indentLevel -= 1;
            EditorGUIUtility.labelWidth = oldLabelWidth;

            EditorGUILayout.Space(12);
            rlist.DoLayoutList();
		}

        public void OverrideDrawer(Rect rect, int index, bool isActive, bool isFocused)
        {
			rect.y += 2;
            EditorGUI.PropertyField(rect, vfxOverrides.GetArrayElementAtIndex(index));
        }
    }
}