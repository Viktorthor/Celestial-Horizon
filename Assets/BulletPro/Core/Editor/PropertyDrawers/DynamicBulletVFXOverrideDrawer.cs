using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro.EditorScripts
{
	[CustomPropertyDrawer(typeof(DynamicBulletVFXOverride))]
	public class DynamicBulletVFXOverrideDrawer : PropertyDrawer
	{
		int oldIndent;
		SerializedProperty paramType, enumPropertyCurrentlyClicked;
		SerializedProperty curveMode, gradientMode;

		// Continuously used for displaying the enum button, even if not clicked
		string[] _optionList;
		string[] optionList
		{
			get
			{
				if (_optionList == null)
					_optionList = BuildShortenedOverrideOptionList();
				return _optionList;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float result = EditorGUIUtility.singleLineHeight + 1;
			float numberOfLines = 2;

			return result * numberOfLines;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			DrawGUI(position, property, label, false);

			EditorGUI.indentLevel = oldIndent;

			EditorGUI.EndProperty();
		}

		void DrawGUI(Rect position, SerializedProperty property, GUIContent label, bool twoLines)
		{
			// fixes the usual double-draw bug
			if (position.width == 1) return;

			paramType = property.FindPropertyRelative("parameterToOverride");
			BulletVFXAtomicParameterType atomicParamType = BulletVFXOverride.GetAtomicParameterType((BulletVFXParameterType)paramType.enumValueIndex);

			// Caching/shortening values
			float h = EditorGUIUtility.singleLineHeight;
			float w = position.width;
			float x = position.x + oldIndent * 15;
			float y = position.y;

			// Tweakable values
			float w1 = 210; // big shuriken enum
			float w2 = 50; // curve mode button
			float w3 = 100; // override min/max/both enum
			float w4 = 95; // blend mode enum
			float w5 = 100; // reference value enum
			float space = 5;
			float secondLineIndent = 30;

			// reduce w1 if there's not enough room, start property calculations by doing so
			float totalFirstLine = oldIndent*15 + w1;
			if (atomicParamType == BulletVFXAtomicParameterType.MinMaxCurve)
			{
				totalFirstLine += w2 + space;
				curveMode = property.FindPropertyRelative("curveMode");
				ParticleSystemCurveMode cmValue = (ParticleSystemCurveMode)curveMode.enumValueIndex;
				if (cmValue == ParticleSystemCurveMode.TwoConstants || cmValue == ParticleSystemCurveMode.TwoCurves)
					totalFirstLine += w3 + space;
			}
			else if (atomicParamType == BulletVFXAtomicParameterType.MinMaxGradient)
			{
				totalFirstLine += w2 + space;
				gradientMode = property.FindPropertyRelative("gradientMode");
				ParticleSystemGradientMode gmValue = (ParticleSystemGradientMode)gradientMode.enumValueIndex;
				if (gmValue == ParticleSystemGradientMode.TwoColors || gmValue == ParticleSystemGradientMode.TwoGradients)
					totalFirstLine += w3 + space;
			}
			float overshoot = totalFirstLine - w;
			if (overshoot > 0) w1 -= overshoot;

			// remaining width for dynamic value
			float w6 = w - (oldIndent*15 + secondLineIndent + w4 + w5 + space*2);

			// calculate rects
			float curX = x;
			Rect[] rects = new Rect[6];
			int i = 0;
			rects[i] = new Rect(curX, y, w1, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w2, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w3, h); curX += space + rects[i++].width;
			y += h + 2;
			curX = x + secondLineIndent;
			rects[i] = new Rect(curX, y, w4, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w5, h); curX += space + rects[i++].width;
			rects[i] = new Rect(curX, y, w6, h); curX += space + rects[i++].width;
			i = 0;

			// carries over info from line 1 to line 2, if minmaxcurve/gradient
			bool needsBlendModeDisplay = false;

			// First line : selecting the parameter to override
			//EditorGUI.PropertyField(rects[i++], paramType, GUIContent.none);
			string paramDisplayName = optionList[paramType.enumValueIndex];
			if (GUI.Button(rects[i++], paramDisplayName, EditorStyles.popup))
			{
				BuildGenericMenu(paramType, BuildOverrideOptionList());
				return;
			}
			
			// First line : handling min/max parameters
			if (atomicParamType == BulletVFXAtomicParameterType.MinMaxCurve)
			{
				GUIStyle btnStyle = new GUIStyle(EditorStyles.miniButton);
				btnStyle.fontSize = 10;
				if (GUI.Button(rects[i++], "Mode...", btnStyle))
				{
					BuildGenericMenu(curveMode, System.Enum.GetNames(typeof(ParticleSystemCurveMode)));
					return;
				}
				ParticleSystemCurveMode cmValue = (ParticleSystemCurveMode)curveMode.enumValueIndex;
				if (cmValue == ParticleSystemCurveMode.TwoConstants || cmValue == ParticleSystemCurveMode.TwoCurves)
				{
					SerializedProperty minMaxOverrideMode = property.FindPropertyRelative("minMaxOverrideMode");
					EditorGUI.PropertyField(rects[i++], minMaxOverrideMode, GUIContent.none);
				}
				else i++;
				if (cmValue == ParticleSystemCurveMode.Constant || cmValue == ParticleSystemCurveMode.TwoConstants)
					needsBlendModeDisplay = true;
			}
			else if (atomicParamType == BulletVFXAtomicParameterType.MinMaxGradient)
			{
				GUIStyle btnStyle = EditorStyles.miniButton;
				btnStyle.fontSize = 10;
				if (GUI.Button(rects[i++], "Mode...", btnStyle))
				{
					BuildGenericMenu(gradientMode, System.Enum.GetNames(typeof(ParticleSystemGradientMode)));
					return;
				}
				ParticleSystemGradientMode gmValue = (ParticleSystemGradientMode)gradientMode.enumValueIndex;
				if (gmValue == ParticleSystemGradientMode.TwoColors || gmValue == ParticleSystemGradientMode.TwoGradients)
				{
					SerializedProperty minMaxOverrideMode = property.FindPropertyRelative("minMaxOverrideMode");
					EditorGUI.PropertyField(rects[i++], minMaxOverrideMode, GUIContent.none);
				}
				else i++;
				if (!(gmValue == ParticleSystemGradientMode.Gradient || gmValue == ParticleSystemGradientMode.TwoGradients))
					needsBlendModeDisplay = true;
			}
			else i += 2;

			// Second line : blend mode
			SerializedProperty blendMode = null;
			if (atomicParamType == BulletVFXAtomicParameterType.String)
				blendMode = property.FindPropertyRelative("stringOverrideMode");
			else if (atomicParamType == BulletVFXAtomicParameterType.MinMaxGradient && needsBlendModeDisplay)
				blendMode = property.FindPropertyRelative("colorOverrideMode");
			else if (atomicParamType == BulletVFXAtomicParameterType.ConstantFloat
				|| atomicParamType == BulletVFXAtomicParameterType.ConstantInt
				|| atomicParamType == BulletVFXAtomicParameterType.Vector2
				|| atomicParamType == BulletVFXAtomicParameterType.Vector3
				|| (atomicParamType == BulletVFXAtomicParameterType.MinMaxCurve && needsBlendModeDisplay))
				blendMode = property.FindPropertyRelative("numberOverrideMode");
			if (blendMode != null)
				EditorGUI.PropertyField(rects[i++], blendMode, GUIContent.none);
			else EditorGUI.LabelField(rects[i++], "Replace with :");

			// Second line : bools that will help see if we need to prompt for value
			bool usesRefColor = false;
			bool usesRefFloat = false;
			bool usesRefGradient = false;

			// Second line : reference value
			SerializedProperty referenceValue = null;
			if (atomicParamType == BulletVFXAtomicParameterType.MinMaxGradient)
			{
				if (needsBlendModeDisplay)
				{
					referenceValue = property.FindPropertyRelative("referenceColor");
					usesRefColor = true;
				}
				else
				{
					referenceValue = property.FindPropertyRelative("referenceGradient");
					usesRefGradient = true;
				}
			}
			else if (atomicParamType == BulletVFXAtomicParameterType.ConstantFloat
				|| (atomicParamType == BulletVFXAtomicParameterType.MinMaxCurve && needsBlendModeDisplay))
			{
				referenceValue = property.FindPropertyRelative("referenceFloat");
				usesRefFloat = true;
			}
			if (referenceValue != null)
				EditorGUI.PropertyField(rects[i++], referenceValue, GUIContent.none);
			else i++;

			// Second line : determine if we need to prompt for value
			bool usesBulletValue = false;
			if (referenceValue != null)
			{
				int idx = referenceValue.enumValueIndex;
				if (usesRefColor)
					usesBulletValue = (idx != (int)VFXReferenceColor.CustomValue);
				else if (usesRefGradient)
					usesBulletValue = (idx != (int)VFXReferenceGradient.CustomValue);
				else if (usesRefFloat)
					usesBulletValue = (idx != (int)VFXReferenceFloat.CustomValue);
				if (usesBulletValue) return;
			}

			// Second line : actual value
			string propName = "";
			switch (atomicParamType)
			{
				case BulletVFXAtomicParameterType.Bool:
					propName = "boolValue";
					break;

				case BulletVFXAtomicParameterType.ConstantFloat:
					propName = "floatValue";
					break;

				case BulletVFXAtomicParameterType.ConstantInt:
					propName = "intValue";
					break;

				case BulletVFXAtomicParameterType.Enum:
					propName = "enumValue";
					break;

				case BulletVFXAtomicParameterType.MinMaxCurve:
					propName = needsBlendModeDisplay ? "floatValue" : "curveValue";
					break;

				case BulletVFXAtomicParameterType.MinMaxGradient:
					propName = needsBlendModeDisplay ? "colorValue" : "gradientValue";
					break;

				case BulletVFXAtomicParameterType.Object:
					propName = "objectReferenceValue";
					break;

				case BulletVFXAtomicParameterType.String:
					propName = "stringValue";
					break;

				case BulletVFXAtomicParameterType.Vector2:
					propName = "vector2Value";
					break;

				case BulletVFXAtomicParameterType.Vector3:
					propName = "vector3Value";
					break;
			}
			
			// merge the two last rects if we didn't prompt for a reference value
			if (referenceValue == null)
				rects[i] = new Rect(rects[i-1].x, rects[i].y, rects[i-1].width+space+rects[i].width, rects[i].height);
			
			// and finally draw the property
			SerializedProperty usedValue = property.FindPropertyRelative(propName);
			EditorGUI.PropertyField(rects[i++], usedValue, GUIContent.none);
		}

		// Builds the GenericMenu so the user can pick parameters sorted by Shuriken module. Works with any enum.
		void BuildGenericMenu(SerializedProperty curProp, string[] options)
		{
			enumPropertyCurrentlyClicked = curProp;
			GenericMenu gm = new GenericMenu();
			for (int i = 0; i < options.Length; i++)
				gm.AddItem(new GUIContent(options[i]), curProp.enumValueIndex==i, ValidateEnumOption, i);

			gm.ShowAsContext();
		}

		// Big string array to be used by the GenericMenu as the options list
		string[] BuildOverrideOptionList()
		{
			return new string[]
			{
				// Main module - common stuff
				"Main/Start Color",
				"Main/System Duration",
				"Main/Start Lifetime",
				"Main/Start Speed",
				"Main/Start Size",
				"Main/Start Size X",
				"Main/Start Size Y",
				"Main/Start Size Z",
				"Main/Simulation Space",
				"Main/Simulation Speed",        
				"Main/Max Particles",

				// Main module - rare stuff
				"Main/Prewarm",
				"Main/Looping",
				"Main/Start Delay",
				"Main/Gravity Modifier",
				"Main/Start Rotation",
				"Main/Start Rotation X",
				"Main/Start Rotation Y",
				"Main/Start Rotation Z",
				"Main/Flip Rotation",
				"Main/Scaling Mode",
				"Main/Custom Simulation Space",

				// Emission module
				"Emission/Rate Over Time",
				"Emission/Rate Over Distance",

				// Shape module - dimension
				"Shape/Scale",
				"Shape/Align To Direction",
				"Shape/Arc",
				"Shape/Length",
				"Shape/Normal Offset",
				"Shape/Position",
				"Shape/Rotation",
				"Shape/Radius",
				"Shape/Random Position Amount",
				"Shape/Random Direction Amount",
				"Shape/Spherical Direction Amount",

				// Shape module - geometry
				"Shape/Mesh",
				"Shape/Mesh Material Index",
				"Shape/Mesh Shape Type",
				"Shape/Shape Type",
				"Shape/Skinned Mesh Renderer",

				// Velocity over lifetime
				"Velocity Over Lifetime/Orbital Offset X",
				"Velocity Over Lifetime/Orbital Offset Y",
				"Velocity Over Lifetime/Orbital Offset Z",
				"Velocity Over Lifetime/Orbital X",
				"Velocity Over Lifetime/Orbital Y",
				"Velocity Over Lifetime/Orbital Z",
				"Velocity Over Lifetime/Radial",
				"Velocity Over Lifetime/Space",
				"Velocity Over Lifetime/Speed Modifier",
				"Velocity Over Lifetime/X",
				"Velocity Over Lifetime/Y",
				"Velocity Over Lifetime/Z",

				// Limit velocity over lifetime
				"Limit Velocity Over Lifetime/Dampen",
				"Limit Velocity Over Lifetime/Drag",
				"Limit Velocity Over Lifetime/Limit",
				"Limit Velocity Over Lifetime/Limit X",
				"Limit Velocity Over Lifetime/Limit Y",
				"Limit Velocity Over Lifetime/Limit Z",
				"Limit Velocity Over Lifetime/Space",

				// Inherit velocity
				"Inherit Velocity/Curve",
				"Inherit Velocity/Mode",

				// Force over lifetime
				"Force Over Lifetime/X",
				"Force Over Lifetime/Y",
				"Force Over Lifetime/Z",
				"Force Over Lifetime/Space",

				// Color over lifetime
				"Color Over Lifetime/Color",

				// Color by speed
				"Color By Speed/Color",
				"Color By Speed/Range",

				// Size over lifetime
				"Size Over Lifetime/Size",
				"Size Over Lifetime/X",
				"Size Over Lifetime/Y",
				"Size Over Lifetime/Z",

				// Size by speed
				"Size By Speed/Range",
				"Size By Speed/Size",
				"Size By Speed/X",
				"Size By Speed/Y",
				"Size By Speed/Z",

				// Rotation over lifetime
				"Rotation Over Lifetime/X",
				"Rotation Over Lifetime/Y",
				"Rotation Over Lifetime/Z",

				// Rotation by speed
				"Rotation By Speed/Range",
				"Rotation By Speed/X",
				"Rotation By Speed/Y",
				"Rotation By Speed/Z",

				// External forces
				"External Forces/Multiplier",

				// Noise
				"Noise/Damping",
				"Noise/Frequency",
				"Noise/Octave Count",
				"Noise/Octave Scale",
				"Noise/Octave Multiplier",
				"Noise/Quality",
				"Noise/Remap",
				"Noise/Remap X",
				"Noise/Remap Y",
				"Noise/Remap Z",
				"Noise/Scroll Speed",
				"Noise/Size Amount",
				"Noise/Strength",
				"Noise/Strength X",
				"Noise/Strength Y",
				"Noise/Strength Z",

				// Collision
				"Collision/Bounce",
				"Collision/Dampen",
				"Collision/Lifetime Loss",
				"Collision/Min Kill Speed",
				"Collision/Max Kill Speed",
				"Collision/Radius Scale",

				// Triggers
				"Triggers/Radius Scale",

				// (Skipping Sub-emitter module)
				// Texture Sheet Animation
				"Texture Sheet Animation/Cycle Count",
				"Texture Sheet Animation/Frame Over Time",
				"Texture Sheet Animation/Row Index",
				"Texture Sheet Animation/Start Frame",

				// Lights module
				"Lights/Intensity",
				"Lights/Light",
				"Lights/Range",
				"Lights/Ratio",

				// Trail module
				"Trail/Ratio",
				"Trail/Lifetime",
				"Trail/Width Over Trail",
				"Trail/Color Over Lifetime",
				"Trail/Color Over Trail",

				// Renderer
				"Renderer/Sorting Order",
				"Renderer/Sorting Layer Name",
				"Renderer/Material",
				"Renderer/Trail Material",
				"Renderer/Min Particle Size",
				"Renderer/Max Particle Size",
				"Renderer/Flip",
				"Renderer/Pivot"
			};
		}

		// Same string array, but with abbreviations for display purposes in enum button
		string[] BuildShortenedOverrideOptionList()
		{
			return new string[]
			{
				// Main module - common stuff
				"Start Color",
				"System Duration",
				"Start Lifetime",
				"Start Speed",
				"Start Size",
				"Start Size X",
				"Start Size Y",
				"Start Size Z",
				"Simulation Space",
				"Simulation Speed",        
				"Max Particles",

				// Main module - rare stuff
				"Prewarm",
				"Looping",
				"Start Delay",
				"Gravity Modifier",
				"Start Rotation",
				"Start Rotation X",
				"Start Rotation Y",
				"Start Rotation Z",
				"Flip Rotation",
				"Scaling Mode",
				"Custom Simulation Space",

				// Emission module
				"Emission Rate Over Time",
				"Emission Rate Over Distance",

				// Shape module - dimension
				"Shape Scale",
				"Align Shape To Direction",
				"Shape Arc",
				"Shape Length",
				"Shape Normal Offset",
				"Shape Position",
				"Shape Rotation",
				"Shape Radius",
				"Shape Random Pos. Amount",
				"Shape Random Dir. Amount",
				"Shape Spherical Dir. Amount",

				// Shape module - geometry
				"Shape Mesh",
				"Shape Mesh Material Index",
				"Shape Mesh Shape Type",
				"Shape Shape Type",
				"Shape Skinned Mesh Renderer",

				// Velocity over lifetime
				"Vel. Over Lifetime / Orbital Offset X",
				"Vel. Over Lifetime / Orbital Offset Y",
				"Vel. Over Lifetime / Orbital Offset Z",
				"Vel. Over Lifetime / Orbital X",
				"Vel. Over Lifetime / Orbital Y",
				"Vel. Over Lifetime / Orbital Z",
				"Vel. Over Lifetime / Radial",
				"Vel. Over Lifetime / Space",
				"Vel. Over Lifetime / Speed Modifier",
				"Vel. Over Lifetime / X",
				"Vel. Over Lifetime / Y ",
				"Vel. Over Lifetime / Z",

				// Limit velocity over lifetime
				"Limit Velocity / Dampen",
				"Limit Velocity / Drag",
				"Limit Velocity / Limit",
				"Limit Velocity / Limit X",
				"Limit Velocity / Limit Y",
				"Limit Velocity / Limit Z",
				"Limit Velocity / Space",

				// Inherit velocity
				"Inherit Velocity / Curve",
				"Inherit Velocity / Mode",

				// Force over lifetime
				"Force Over Lifetime X",
				"Force Over Lifetime Y",
				"Force Over Lifetime Z",
				"Force Over Lifetime Sim Space",

				// Color over lifetime
				"Color Over Lifetime",

				// Color by speed
				"Color By Speed",
				"Color By Speed (Range)",

				// Size over lifetime
				"Size Over Lifetime",
				"Size Over Lifetime X",
				"Size Over Lifetime Y",
				"Size Over Lifetime Z",

				// Size by speed
				"Size By Speed (Range)",
				"Size By Speed",
				"Size X By Speed",
				"Size Y By Speed",
				"Size Z By Speed",

				// Rotation over lifetime
				"Rotation Over Lifetime X",
				"Rotation Over Lifetime Y",
				"Rotation Over Lifetime Z",

				// Rotation by speed
				"Rotation By Speed (Range)",
				"Rotation X By Speed",
				"Rotation Y By Speed",
				"Rotation Z By Speed",

				// External forces
				"External Forces Multiplier",

				// Noise
				"Noise Damping",
				"Noise Frequency",
				"Noise Octave Count",
				"Noise Octave Scale",
				"Noise Octave Multiplier",
				"Noise Quality",
				"Noise Remap",
				"Noise Remap X",
				"Noise Remap Y",
				"Noise Remap Z",
				"Noise Scroll Speed",
				"Noise Size Amount",
				"Noise Strength",
				"Noise Strength X",
				"Noise Strength Y",
				"Noise Strength Z",

				// Collision
				"Collision Bounce",
				"Collision Dampen",
				"Collision Lifetime Loss",
				"Collision Min Kill Speed",
				"Collision Max Kill Speed",
				"Collision Radius Scale",

				// Triggers
				"Trigger Radius Scale",

				// (Skipping Sub-emitter module)
				// Texture Sheet Animation
				"Texture Sheet Anim. Cycle Count",
				"Texture Sheet Anim. Frame Over Time",
				"Texture Sheet Anim. Row Index",
				"Texture Sheet Anim. Start Frame",

				// Lights module
				"Light Intensity",
				"Light Prefab",
				"Light Range",
				"Light Ratio",

				// Trail module
				"Trail Ratio",
				"Trail Lifetime",
				"Trail Width Over Trail",
				"Trail Color Over Lifetime",
				"Trail Color Over Trail",

				// Renderer
				"Sorting Order",
				"Sorting Layer Name",
				"Material",
				"Trail Material",
				"(Renderer) Min Particle Size",
				"(Renderer) Max Particle Size",
				"(Renderer) Flip",
				"(Renderer) Pivot"
			};
		}

		// Called by any GenericMenu at validation
		void ValidateEnumOption(object i)
		{
			enumPropertyCurrentlyClicked.enumValueIndex = (int)i;
			enumPropertyCurrentlyClicked.serializedObject.ApplyModifiedProperties();

			if (enumPropertyCurrentlyClicked == paramType) UpdateDynamicEnumDisplay();
		}

		// Called upon changing parameter type, so the dynamic enum field looks different
		void UpdateDynamicEnumDisplay()
		{
			BulletVFXParameterType bvpt = (BulletVFXParameterType) paramType.enumValueIndex;
			
			if (BulletVFXOverride.GetAtomicParameterType(bvpt) != BulletVFXAtomicParameterType.Enum)
				return;

			if (bvpt == BulletVFXParameterType.MainModuleScalingMode)
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemScalingMode));
			else if (bvpt == BulletVFXParameterType.ShapeModuleShapeType)
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemShapeType));				
			else if (bvpt == BulletVFXParameterType.ShapeModuleMeshShapeType)
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemMeshShapeType));				
			else if (bvpt == BulletVFXParameterType.InheritVelocityModuleMode)
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemInheritVelocityMode));
			else if (bvpt == BulletVFXParameterType.NoiseModuleQuality)
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemNoiseQuality));
			else // simulation spaces
				DynamicParameterUtility.SetEnumType(paramType, typeof(ParticleSystemSimulationSpace));
		}
	}
}
