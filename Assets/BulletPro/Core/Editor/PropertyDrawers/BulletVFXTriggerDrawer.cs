using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
	[CustomPropertyDrawer(typeof(BulletVFXTrigger))]
	public class BulletVFXTriggerDrawer : PropertyDrawer
	{
		int oldIndent;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float result = EditorGUIUtility.singleLineHeight + 1;

			float widthThreshold = 540;
			bool singleLine = EditorGUIUtility.currentViewWidth > widthThreshold;

			SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
			if (!enabledProp.boolValue) singleLine = true;
			
			float numberOfLines = singleLine ? 1 : 2;
			float verticalSpace = singleLine ? 0 : 2;

			return result * numberOfLines + verticalSpace;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			SerializedProperty enabledProp = property.FindPropertyRelative("enabled");
			bool twoLines = EditorGUIUtility.currentViewWidth <= 540;
			if (!enabledProp.boolValue) twoLines = false;
			DrawGUI(position, property, enabledProp, label, twoLines);

			EditorGUI.indentLevel = oldIndent;

			EditorGUI.EndProperty();
		}

		void DrawGUI(Rect position, SerializedProperty property, SerializedProperty enabledProp, GUIContent label, bool twoLines)
		{
			// fixes the usual double-draw bug
			if (position.width == 1) return;

			float h = EditorGUIUtility.singleLineHeight;
			float w = position.width;
			float x = position.x + oldIndent * 15;
			float y = position.y;
			//if (twoLines) y += 4; // turns out not doing this is prettier

			float w1 = 20; // ticker rect
			float w2 = 100; // name rect
			float w3 = 50; // play/stop enum rect
			float w4 = 50; // "unless" rect
			float w5 = 200; // canceller big enum rect
			float space = 5;
			float secondLineIndent = 30;

			// reduce w5 if it bites past maximum width
			float maxw5 = w - (oldIndent*15 + secondLineIndent + w1 + w4 + space*2);
			if (!twoLines) maxw5 -= w1 + w2 + w3;
			if (w5 > maxw5) w5 = maxw5;

			float curX = x;

			Rect[] rects = new Rect[6];
			int i = 0;
			rects[i] = new Rect(curX, y, w1, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w2, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w3, h); curX += space + rects[i++].width + 5;
			if (twoLines)
			{
				y += h + 2;
				curX = x + secondLineIndent;
			}
			rects[i] = new Rect(curX, y, w1, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w4, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w5, h); curX += space + rects[i++].width;

			SerializedProperty behaviourProp = property.FindPropertyRelative("behaviour");
			SerializedProperty unlessProp = property.FindPropertyRelative("unless");
			SerializedProperty cancellerProp = property.FindPropertyRelative("canceller");

			i = 0;
			
			// swap positions of two controls so PrefixLabel points to the toggle (hence the extra +1/-2/+1)
			i++;
			EditorGUI.PrefixLabel(rects[i++], label);
			i -= 2;
			EditorGUI.PropertyField(rects[i++], enabledProp, GUIContent.none);
			i++;
			EditorGUI.BeginDisabledGroup(!enabledProp.boolValue);
				EditorGUI.PropertyField(rects[i++], behaviourProp, GUIContent.none);
				if (enabledProp.boolValue)
				{
					// swap again for the "unless" part
					i++;
					EditorGUI.PrefixLabel(rects[i++], new GUIContent("unless...", "Tick this box to specify special conditions that cancel the play/stop of this effect."));
					i -= 2;
					EditorGUI.PropertyField(rects[i++], unlessProp, GUIContent.none);
					i++;
					EditorGUI.BeginDisabledGroup(!unlessProp.boolValue);
						EditorGUI.PropertyField(rects[i++], cancellerProp, GUIContent.none);
					EditorGUI.EndDisabledGroup();
				}
			EditorGUI.EndDisabledGroup();
		}
	}
}
