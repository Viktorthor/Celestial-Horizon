using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script is part of the BulletPro package for Unity.
// Author : Simon Albou <albou.simon@gmail.com>

namespace BulletPro
{
	#region int

	public class MicroActionCustomIntMultiply : MicroActionCustomParam<int>
	{
		public MicroActionCustomIntMultiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, int inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetInt(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			int intVal = startValue + (int)(((float)(endValue-startValue))*GetRatio());
			bullet.moduleParameters.SetInt(customParamName, intVal);			
		}
	}

	public class MicroActionCustomIntAdd : MicroActionCustomParam<int>
	{
		public MicroActionCustomIntAdd(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, int inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetInt(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			int intVal = startValue + (int)(((float)(endValue-startValue))*GetRatio());
			bullet.moduleParameters.SetInt(customParamName, intVal);			
		}
	}

	public class MicroActionCustomIntSet : MicroActionCustomParam<int>
	{
		public MicroActionCustomIntSet(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, int inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetInt(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			int intVal = startValue + (int)(((float)(endValue-startValue))*GetRatio());
			bullet.moduleParameters.SetInt(customParamName, intVal);			
		}
	}

	#endregion

	#region float

	public class MicroActionCustomFloatMultiply : MicroActionCustomParam<float>
	{
		public MicroActionCustomFloatMultiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetFloat(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetFloat(customParamName, Mathf.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomFloatAdd : MicroActionCustomParam<float>
	{
		public MicroActionCustomFloatAdd(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetFloat(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetFloat(customParamName, Mathf.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	public class MicroActionCustomFloatSet : MicroActionCustomParam<float>
	{
		public MicroActionCustomFloatSet(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetFloat(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetFloat(customParamName, Mathf.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	#endregion

	#region Slider01

	public class MicroActionCustomSlider01Multiply : MicroActionCustomParam<float>
	{
		public MicroActionCustomSlider01Multiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetSlider01(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetSlider01(customParamName, Mathf.Clamp01(Mathf.Lerp(startValue, endValue, GetRatio())));			
		}
	}

	public class MicroActionCustomSlider01Add : MicroActionCustomParam<float>
	{
		public MicroActionCustomSlider01Add(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetSlider01(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetSlider01(customParamName, Mathf.Clamp01(Mathf.Lerp(startValue, endValue, GetRatio())));			
		}
	}

	public class MicroActionCustomSlider01Set : MicroActionCustomParam<float>
	{
		public MicroActionCustomSlider01Set(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetSlider01(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetSlider01(customParamName, Mathf.Clamp01(Mathf.Lerp(startValue, endValue, GetRatio())));			
		}
	}

	#endregion

	#region long

	public class MicroActionCustomLongMultiply : MicroActionCustomParam<long>
	{
		public MicroActionCustomLongMultiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, long inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetLong(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetLong(customParamName, startValue + (endValue-startValue)*(long)GetRatio());			
		}
	}

	public class MicroActionCustomLongAdd : MicroActionCustomParam<long>
	{
		public MicroActionCustomLongAdd(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, long inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetLong(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetLong(customParamName, startValue + (endValue-startValue)*(long)GetRatio());			
		}
	}

	public class MicroActionCustomLongSet : MicroActionCustomParam<long>
	{
		public MicroActionCustomLongSet(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, long inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetLong(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetLong(customParamName, startValue + (endValue-startValue)*(long)GetRatio());			
		}
	}

	#endregion

	#region double

	public class MicroActionCustomDoubleMultiply : MicroActionCustomParam<double>
	{
		public MicroActionCustomDoubleMultiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, double inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetDouble(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetDouble(customParamName, startValue + (endValue-startValue)*(double)GetRatio());			
		}
	}

	public class MicroActionCustomDoubleAdd : MicroActionCustomParam<double>
	{
		public MicroActionCustomDoubleAdd(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, double inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetDouble(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetDouble(customParamName, startValue + (endValue-startValue)*(double)GetRatio());			
		}
	}

	public class MicroActionCustomDoubleSet : MicroActionCustomParam<double>
	{
		public MicroActionCustomDoubleSet(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, double inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetDouble(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetDouble(customParamName, startValue + (endValue-startValue)*(double)GetRatio());			
		}
	}

	#endregion

	#region Vector2

	public class MicroActionCustomVector2Multiply : MicroActionCustomParam<Vector2>
	{
		public MicroActionCustomVector2Multiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, Vector2.zero, curveType)
		{
			startValue = bullet.moduleParameters.GetVector2(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector2(customParamName, Vector2.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomVector2Add : MicroActionCustomParam<Vector2>
	{
		public MicroActionCustomVector2Add(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector2 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector2(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector2(customParamName, Vector2.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	public class MicroActionCustomVector2Set : MicroActionCustomParam<Vector2>
	{
		public MicroActionCustomVector2Set(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector2 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector2(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector2(customParamName, Vector2.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	#endregion

	#region Vector3

	public class MicroActionCustomVector3Multiply : MicroActionCustomParam<Vector3>
	{
		public MicroActionCustomVector3Multiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, Vector3.zero, curveType)
		{
			startValue = bullet.moduleParameters.GetVector3(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector3(customParamName, Vector3.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomVector3Add : MicroActionCustomParam<Vector3>
	{
		public MicroActionCustomVector3Add(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector3 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector3(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector3(customParamName, Vector3.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	public class MicroActionCustomVector3Set : MicroActionCustomParam<Vector3>
	{
		public MicroActionCustomVector3Set(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector3 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector3(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector3(customParamName, Vector3.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	#endregion

	#region Vector4

	public class MicroActionCustomVector4Multiply : MicroActionCustomParam<Vector4>
	{
		public MicroActionCustomVector4Multiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, float inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, Vector4.zero, curveType)
		{
			startValue = bullet.moduleParameters.GetVector4(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector4(customParamName, Vector4.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomVector4Add : MicroActionCustomParam<Vector4>
	{
		public MicroActionCustomVector4Add(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector4 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector4(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector4(customParamName, Vector4.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	public class MicroActionCustomVector4Set : MicroActionCustomParam<Vector4>
	{
		public MicroActionCustomVector4Set(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Vector4 inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetVector4(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetVector4(customParamName, Vector4.Lerp(startValue, endValue, GetRatio()));					
		}
	}

	#endregion

	#region Color

	public class MicroActionCustomColorMultiply : MicroActionCustomParam<Color>
	{
		public MicroActionCustomColorMultiply(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Color inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetColor(paramName);
			endValue = startValue * inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetColor(customParamName, Color.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomColorAdd : MicroActionCustomParam<Color>
	{
		public MicroActionCustomColorAdd(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Color inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetColor(paramName);
			endValue = startValue + inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetColor(customParamName, Color.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomColorSet : MicroActionCustomParam<Color>
	{
		public MicroActionCustomColorSet(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Color inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetColor(paramName);
			endValue = inputValue;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetColor(customParamName, Color.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	public class MicroActionCustomColorOverlay : MicroActionCustomParam<Color>
	{
		public MicroActionCustomColorOverlay(Bullet thisBullet, float lerpTime, AnimationCurve lerpCurve, string paramName, Color inputValue, PatternCurveType curveType=PatternCurveType.None)
			: base(thisBullet, lerpTime, lerpCurve, paramName, inputValue, curveType)
		{
			startValue = bullet.moduleParameters.GetColor(paramName);
			float finalAlpha = startValue.a + inputValue.a*(1-startValue.a);
			endValue = startValue * (1-inputValue.a) + inputValue * inputValue.a;
			endValue.a = finalAlpha;
		}

		public override void UpdateParameter(float deltaTime)
		{
			bullet.moduleParameters.SetColor(customParamName, Color.Lerp(startValue, endValue, GetRatio()));			
		}
	}

	#endregion
}