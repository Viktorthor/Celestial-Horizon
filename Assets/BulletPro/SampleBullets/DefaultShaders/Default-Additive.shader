// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "BulletPro Samples/Default Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _RotationOffset ("Rotation Offset (Degrees)", Float) = 0
        _RotationSpeed ("Rotation Speed (Degrees/sec)", Float) = 0
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One

        Pass
        {
        CGPROGRAM
            #pragma vertex BulletVert
            #pragma fragment BulletFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

			v2f BulletVert(appdata_t IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				#ifdef UNITY_INSTANCING_ENABLED
				IN.vertex.xy *= _Flip.xy;
				#endif

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				//OUT.color = _Color * _RendererColor;
				OUT.color = IN.color * _Color * _RendererColor;

				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

            float _RotationSpeed, _RotationOffset;

			float4 BulletFrag(v2f IN) : SV_Target
			{
                // apply rotation
                IN.texcoord.xy -= 0.5;
                float t = 0.017453292 * (_RotationOffset + _RotationSpeed * _Time.y); // degrees to radians
                float x = IN.texcoord.x * cos(t) - IN.texcoord.y * sin(t);
                float y = IN.texcoord.y * cos(t) + IN.texcoord.x * sin(t);
                IN.texcoord.xy = float2(x,y);
                IN.texcoord.xy += 0.5;

				float4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
        ENDCG
        }
    }
}
