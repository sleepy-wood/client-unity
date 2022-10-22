Shader "Hidden/Broccoli/Colored Branch Preview Mode"
{
	Properties {
		_Color ("Branch Color", Color) = (0.9, 0.9, 0.9, 0.5)
		_LevelColor ("Branch Level Color", Color) = (0.63, 0.75, 0.88, 0.5)
		_SelectionColor ("Selected Color", Color) = (0.9, 0.6, 0.6, 0.7)
		_TunedColor ("Tuned Color", Color) = (0.75, 0.75, 0.75, 0.5)
		_SelectedLevel ("Selected Level", Float) = -1
	}
	SubShader {

		CGINCLUDE
		struct Input
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float4 uv6 : TEXCOORD5;
			};
	
			struct Varying
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 uv6 : TEXCOORD5;
			};
		ENDCG

		Tags { "RenderType"="Opaque" }
		Pass {
			//Blend One Zero
            //ZTest LEqual
            //Cull Off
            //ZWrite Off
			Blend One Zero
            ZTest LEqual
            Cull Off
            ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			float4 _Color;
			float4 _LevelColor;
			float4 _SelectionColor;
			float4 _TunedColor;
			float _SelectedLevel;

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _DoClip;
			fixed _Cutoff;

			
			Varying vert (Input v) {
				Varying o;
				o.position = UnityObjectToClipPos(v.position );
				o.uv = v.uv;
				o.uv6 = v.uv6;

				return o;
			}

			half4 frag( Varying i ) : SV_Target {
				/// UV6 information of the mesh.
				/// x: id of the branch.
				/// y: if of the branch skin.
				/// z: id of the struct.
				/// w: tuned.
				if (i.uv6.w == 2) {
					return _SelectionColor;
				} else if (i.uv6.w == 3) {
					return _SelectionColor;
				} else if (i.uv6.z == _SelectedLevel) {
					return _LevelColor;
				} else if (i.uv6.w == 1) {
					return _TunedColor;
				}

				return _Color;
			}
			ENDCG
		}
		
	}
	FallBack "Diffuse"
}