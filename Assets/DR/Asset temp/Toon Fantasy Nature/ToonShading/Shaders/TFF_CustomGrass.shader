// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TFF_CustomGrass"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_TextureSample("Texture Sample", 2D) = "white" {}
		_Color1("Color 1", Color) = (0,0,0,1)
		_Color2("Color 2", Color) = (1,1,1,0)
		_Color1Level("Color 1 Level", Range( 0 , 1)) = 0.35
		_WindNoiseTexture("Wind Noise Texture", 2D) = "white" {}
		_WindScroll("Wind Scroll", Range( 0 , 1)) = 0.1
		_WindJitter("Wind Jitter", Range( 0 , 1)) = 0.1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _WindNoiseTexture;
		uniform float _WindScroll;
		uniform float _WindJitter;
		uniform sampler2D _TextureSample;
		uniform float4 _TextureSample_ST;
		uniform float4 _Color2;
		uniform float4 _Color1;
		uniform float _Color1Level;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult60 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 temp_output_61_0 = ( appendResult60 * 0.1 );
			float2 panner63 = ( ( (0.0 + (_WindScroll - 0.0) * (0.3 - 0.0) / (1.0 - 0.0)) * _Time.y ) * float2( 1,1 ) + temp_output_61_0);
			float2 panner74 = ( ( _Time.y * (0.0 + (_WindJitter - 0.0) * (0.5 - 0.0) / (1.0 - 0.0)) ) * float2( 1,1 ) + ( temp_output_61_0 * float2( 2,2 ) ));
			float4 WindScroll68 = ( ( pow( tex2Dlod( _WindNoiseTexture, float4( panner63, 0, 0.0) ) , 2.5 ) * tex2Dlod( _WindNoiseTexture, float4( panner74, 0, 0.0) ) ) * v.color );
			v.vertex.xyz += WindScroll68.rgb;
			v.vertex.w = 1;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_TextureSample = i.uv_texcoord * _TextureSample_ST.xy + _TextureSample_ST.zw;
			float4 tex2DNode127 = tex2D( _TextureSample, uv_TextureSample );
			float4 lerpResult119 = lerp( ( _Color2 * tex2DNode127 ) , ( tex2DNode127 * _Color1 ) , saturate( ( i.uv_texcoord.y + (-1.0 + (_Color1Level - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) ) ));
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			UnityGI gi162 = gi;
			float3 diffNorm162 = ase_worldNormal;
			gi162 = UnityGI_Base( data, 1, diffNorm162 );
			float3 indirectDiffuse162 = gi162.indirect.diffuse + diffNorm162 * 0.0001;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 temp_output_151_0 = ( ase_lightAtten * ase_lightColor.rgb );
			float3 temp_cast_1 = (1.0).xxx;
			#ifdef UNITY_PASS_FORWARDBASE
				float3 staticSwitch161 = temp_cast_1;
			#else
				float3 staticSwitch161 = temp_output_151_0;
			#endif
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult155 = dot( ase_worldNormal , ase_worldlightDir );
			float3 break154 = temp_output_151_0;
			c.rgb = ( lerpResult119 * ( ( ( float4( indirectDiffuse162 , 0.0 ) * ase_lightColor ) * tex2DNode127 ) + ( ( tex2DNode127 * float4( staticSwitch161 , 0.0 ) ) * ( (dotResult155*0.495 + 0.5) * max( max( break154.x , break154.y ) , break154.z ) ) ) ) ).rgb;
			c.a = 1;
			clip( tex2DNode127.a - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows dithercrossfade vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18935
2353;77;1454;748;-1038.349;-52.78766;1;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;59;-986.3873,-1095.243;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;64;-1161.37,-825.0721;Inherit;False;Property;_WindScroll;Wind Scroll;6;0;Create;True;0;0;0;False;0;False;0.1;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-731.3701,-654.072;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-719.3701,-939.0721;Inherit;False;Constant;_NoiseSize;Noise Size;3;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;60;-707.387,-1111.242;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1145.484,-480.3462;Inherit;False;Property;_WindJitter;Wind Jitter;7;0;Create;True;0;0;0;False;0;False;0.1;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;80;-820.8512,-837.0983;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;149;-794.0867,1426.035;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;150;-794.0867,1522.035;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-524.3701,-1101.072;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;-518.3701,-783.072;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;79;-820.6073,-487.6216;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;-554.0868,1426.035;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;63;-280.3696,-1093.072;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-519.2531,-575.8951;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-278.613,-814.8079;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;154;-394.0868,1618.035;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WorldNormalVector;153;-570.0868,1122.036;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;152;-618.0867,1266.035;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;67;23.23467,-1118.616;Inherit;True;Property;_WindNoiseTexture;Wind Noise Texture;5;0;Create;True;0;0;0;False;0;False;-1;None;41d01fe6e8db970449385e7f43d4aa06;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;74;-105.5129,-814.1079;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;1,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-334.3833,257.0578;Inherit;False;Property;_Color1Level;Color 1 Level;4;0;Create;True;0;0;0;False;0;False;0.35;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-538.0868,1010.036;Inherit;False;Constant;_Value;Value;1;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;155;-330.0868,1202.035;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;156;-122.0867,1618.035;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;159;5.91327,1650.035;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;72;420.66,-1060.765;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;2.5;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;75;102.4871,-815.1079;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;67;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;157;-58.08672,1266.035;Inherit;True;3;0;FLOAT;1;False;1;FLOAT;0.495;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;162;-55.04341,619.8387;Inherit;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;132;-38.45207,261.2453;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;164;16.02286,711.3237;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.StaticSwitch;161;-138.0867,962.0361;Inherit;False;Property;_UNITY_PASS_FORWARDBASE2;UNITY_PASS_FORWARDBASE;1;0;Create;False;0;0;0;True;0;False;0;0;0;False;UNITY_PASS_FORWARDBASE;Toggle;2;UNITY_PASS_FORWARDBASE;Key1;Fetch;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;128;-73.40897,123.4554;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;127;-144.6953,-297.3263;Inherit;True;Property;_TextureSample;Texture Sample;1;0;Create;True;0;0;0;False;0;False;-1;None;cc7e7bd6e8649344dbadd965a58f1123;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;112;-55.41583,-90.67146;Inherit;False;Property;_Color1;Color 1;2;0;Create;True;0;0;0;False;0;False;0,0,0,1;0.1946422,0.509434,0.3875169,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;632.5909,-915.3308;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;83;531.6942,-739.6173;Inherit;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;160;213.9132,1266.035;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;110;-57.90097,-474.9566;Inherit;False;Property;_Color2;Color 2;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.2082592,0.3709351,0.3773585,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;129;212.1222,204.1867;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;597.9133,962.0361;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;168;348.9566,659.8387;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;709.9133,770.0361;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;319.8973,-392.9248;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;131;341.5467,205.0753;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;331.7307,-231.4161;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;886.6118,-859.1572;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;169;857.2586,1080.143;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;1118.331,-864.3554;Inherit;False;WindScroll;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;119;669.6838,-306.5066;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;171;1088.914,852.6544;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;1608.459,815.4155;Inherit;False;68;WindScroll;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;1438.949,467.6183;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1923.736,187.0483;Float;False;True;-1;2;;0;0;CustomLighting;Toon/TFF_CustomGrass;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;15.82;0,0,0,0;VertexOffset;False;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;60;0;59;1
WireConnection;60;1;59;3
WireConnection;80;0;64;0
WireConnection;61;0;60;0
WireConnection;61;1;62;0
WireConnection;65;0;80;0
WireConnection;65;1;66;0
WireConnection;79;0;77;0
WireConnection;151;0;149;0
WireConnection;151;1;150;1
WireConnection;63;0;61;0
WireConnection;63;1;65;0
WireConnection;76;0;66;0
WireConnection;76;1;79;0
WireConnection;73;0;61;0
WireConnection;154;0;151;0
WireConnection;67;1;63;0
WireConnection;74;0;73;0
WireConnection;74;1;76;0
WireConnection;155;0;153;0
WireConnection;155;1;152;0
WireConnection;156;0;154;0
WireConnection;156;1;154;1
WireConnection;159;0;156;0
WireConnection;159;1;154;2
WireConnection;72;0;67;0
WireConnection;75;1;74;0
WireConnection;157;0;155;0
WireConnection;132;0;130;0
WireConnection;161;1;151;0
WireConnection;161;0;158;0
WireConnection;78;0;72;0
WireConnection;78;1;75;0
WireConnection;160;0;157;0
WireConnection;160;1;159;0
WireConnection;129;0;128;2
WireConnection;129;1;132;0
WireConnection;166;0;127;0
WireConnection;166;1;161;0
WireConnection;168;0;162;0
WireConnection;168;1;164;0
WireConnection;170;0;168;0
WireConnection;170;1;127;0
WireConnection;116;0;110;0
WireConnection;116;1;127;0
WireConnection;131;0;129;0
WireConnection;117;0;127;0
WireConnection;117;1;112;0
WireConnection;84;0;78;0
WireConnection;84;1;83;0
WireConnection;169;0;166;0
WireConnection;169;1;160;0
WireConnection;68;0;84;0
WireConnection;119;0;116;0
WireConnection;119;1;117;0
WireConnection;119;2;131;0
WireConnection;171;0;170;0
WireConnection;171;1;169;0
WireConnection;172;0;119;0
WireConnection;172;1;171;0
WireConnection;0;10;127;4
WireConnection;0;13;172;0
WireConnection;0;11;134;0
ASEEND*/
//CHKSM=F8878E814F25D6243B946B3987DE3E6E3F30D21D