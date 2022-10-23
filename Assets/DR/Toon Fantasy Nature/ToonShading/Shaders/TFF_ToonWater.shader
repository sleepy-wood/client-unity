// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TFF_ToonWater"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (0,0.6117647,1,1)
		_DeepColor("Deep Color", Color) = (0,0.3333333,0.8509804,1)
		_ShallowColorDepth("Shallow Color Depth", Range( 0 , 15)) = 2.75
		_Opacity("Opacity", Range( 0 , 1)) = 0.5
		_OpacityDepth("Opacity Depth", Range( 0 , 20)) = 6.5
		_FoamColor("Foam Color", Color) = (0.8705882,0.8705882,0.8705882,1)
		_FoamHardness("Foam Hardness", Range( 0 , 1)) = 0.33
		_FoamDistance("Foam Distance", Range( 0 , 1)) = 0.05
		_FoamOpacity("Foam Opacity", Range( 0 , 1)) = 0.65
		_FoamScale("Foam Scale", Range( 0 , 1)) = 0.2
		_FoamSpeed("Foam Speed", Range( 0 , 1)) = 0.125
		_FresnelColor("Fresnel Color", Color) = (0.8313726,0.8313726,0.8313726,1)
		_FresnelIntensity("Fresnel Intensity", Range( 0 , 1)) = 0.4
		[Toggle(_WAVES_ON)] _Waves("Waves", Float) = 1
		_WaveAmplitude("Wave Amplitude", Range( 0 , 1)) = 0.5
		_WaveIntensity("Wave Intensity", Range( 0 , 1)) = 0.15
		_WaveSpeed("Wave Speed", Range( 0 , 1)) = 1
		_ReflectionsOpacity("Reflections Opacity", Range( 0 , 1)) = 0.65
		_ReflectionsScale("Reflections Scale", Range( 1 , 40)) = 4.8
		_ReflectionsScrollSpeed("Reflections Scroll Speed", Range( -1 , 1)) = 0.05
		_ReflectionsCutoff("Reflections Cutoff", Range( 0 , 1)) = 0.35
		_ReflectionsCutoffScale("Reflections Cutoff Scale", Range( 1 , 40)) = 3
		_ReflectionsCutoffScrollSpeed("Reflections Cutoff Scroll Speed", Range( -1 , 1)) = -0.025
		[Normal]_NormalMap("Normal Map", 2D) = "bump" {}
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.5
		#pragma shader_feature _WAVES_ON
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha addshadow fullforwardshadows nolightmap  nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
			float3 worldPos;
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

		uniform float _WaveSpeed;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _WaveAmplitude;
		uniform float _WaveIntensity;
		uniform sampler2D _NoiseTexture;
		uniform float _ReflectionsScrollSpeed;
		uniform float _ReflectionsScale;
		uniform float _ReflectionsOpacity;
		uniform float4 _ShallowColor;
		uniform float4 _DeepColor;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _ShallowColorDepth;
		uniform float4 _FresnelColor;
		uniform float _FresnelIntensity;
		uniform float4 _FoamColor;
		uniform float _FoamSpeed;
		uniform float _FoamScale;
		uniform float _FoamDistance;
		uniform float _FoamHardness;
		uniform float _FoamOpacity;
		uniform float _ReflectionsCutoffScrollSpeed;
		uniform float _ReflectionsCutoffScale;
		uniform float _ReflectionsCutoff;
		uniform float _Opacity;
		uniform float _OpacityDepth;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 uv_NormalMap = v.texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			#ifdef _WAVES_ON
				float3 staticSwitch321 = ( ase_vertexNormal * ( sin( ( ( _Time.y * _WaveSpeed ) - ( UnpackNormal( tex2Dlod( _NormalMap, float4( uv_NormalMap, 0, 0.0) ) ).b * ( _WaveAmplitude * 30.0 ) ) ) ) * (0.0 + (_WaveIntensity - 0.0) * (0.15 - 0.0) / (1.0 - 0.0)) ) );
			#else
				float3 staticSwitch321 = float3( 0,0,0 );
			#endif
			v.vertex.xyz += staticSwitch321;
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
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth163 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth163 = abs( ( screenDepth163 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( tex2D( _NoiseTexture, ( ( (0.0 + (_FoamSpeed - 0.0) * (2.5 - 0.0) / (1.0 - 0.0)) * _Time.y ) + ( i.uv_texcoord * (30.0 + (_FoamScale - 0.0) * (1.0 - 30.0) / (1.0 - 0.0)) ) ) ).r * (0.0 + (_FoamDistance - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) ) ) );
			float clampResult208 = clamp( distanceDepth163 , 0.0 , 1.0 );
			float clampResult160 = clamp( pow( clampResult208 , (1.0 + (_FoamHardness - 0.0) * (10.0 - 1.0) / (1.0 - 0.0)) ) , 0.0 , 1.0 );
			float temp_output_156_0 = ( ( 1.0 - clampResult160 ) * _FoamOpacity );
			float screenDepth191 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth191 = abs( ( screenDepth191 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( (0.0 + (_FoamDistance - 0.0) * (15.0 - 0.0) / (1.0 - 0.0)) ) );
			float clampResult207 = clamp( distanceDepth191 , 0.0 , 1.0 );
			float FoamScale330 = _FoamScale;
			float temp_output_185_0 = ( ( 1.0 - clampResult207 ) * ( tex2D( _NoiseTexture, ( ( _Time.y * _FoamSpeed ) + ( i.uv_texcoord * (15.0 + (FoamScale330 - 0.0) * (1.0 - 15.0) / (1.0 - 0.0)) ) ) ).r * (0.0 + (_FoamOpacity - 0.0) * (0.85 - 0.0) / (1.0 - 0.0)) ) );
			float screenDepth294 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth294 = abs( ( screenDepth294 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _OpacityDepth ) );
			float clampResult295 = clamp( distanceDepth294 , 0.0 , 1.0 );
			float clampResult299 = clamp( ( ( temp_output_156_0 + temp_output_185_0 ) + _Opacity + clampResult295 ) , 0.0 , 1.0 );
			float3 ase_worldPos = i.worldPos;
			float3 normalizeResult232 = normalize( ( _WorldSpaceCameraPos - ase_worldPos ) );
			float2 temp_cast_8 = (_ReflectionsCutoffScrollSpeed).xx;
			float2 panner342 = ( 1.0 * _Time.y * temp_cast_8 + ( i.uv_texcoord * (2.0 + (_ReflectionsCutoffScale - 0.0) * (10.0 - 2.0) / (10.0 - 0.0)) ));
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult108 = dot( reflect( -normalizeResult232 , (WorldNormalVector( i , UnpackNormal( tex2D( _NormalMap, panner342 ) ) )) ) , ase_worldlightDir );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float2 temp_cast_9 = (_ReflectionsScrollSpeed).xx;
			float2 panner40 = ( 1.0 * _Time.y * temp_cast_9 + ( i.uv_texcoord * _ReflectionsScale ));
			float temp_output_37_0 = ( UnpackNormal( tex2D( _NormalMap, panner40 ) ).g * (0.0 + (_ReflectionsOpacity - 0.0) * (8.0 - 0.0) / (1.0 - 0.0)) );
			float3 clampResult120 = clamp( ( ( pow( dotResult108 , exp( (0.0 + (_ReflectionsCutoff - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) ) ) * ase_lightColor.rgb ) * temp_output_37_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			float3 lerpResult90 = lerp( ( clampResult120 * float3( i.uv_texcoord ,  0.0 ) ) , ( ase_lightColor.rgb * ase_lightAtten ) , ( 1.0 - ase_lightAtten ));
			c.rgb = lerpResult90;
			c.a = clampResult299;
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
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 appendResult31 = (float2(( ase_screenPosNorm.x + 0.01 ) , ( ase_screenPosNorm.y + 0.01 )));
			float2 temp_cast_1 = (_ReflectionsScrollSpeed).xx;
			float2 panner40 = ( 1.0 * _Time.y * temp_cast_1 + ( i.uv_texcoord * _ReflectionsScale ));
			float temp_output_37_0 = ( UnpackNormal( tex2D( _NormalMap, panner40 ) ).g * (0.0 + (_ReflectionsOpacity - 0.0) * (8.0 - 0.0) / (1.0 - 0.0)) );
			float Turbulence291 = temp_output_37_0;
			float4 lerpResult24 = lerp( ase_screenPosNorm , float4( appendResult31, 0.0 , 0.0 ) , Turbulence291);
			float screenDepth146 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth146 = abs( ( screenDepth146 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _ShallowColorDepth ) );
			float clampResult211 = clamp( distanceDepth146 , 0.0 , 1.0 );
			float4 lerpResult142 = lerp( _ShallowColor , _DeepColor , clampResult211);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV136 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode136 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV136, (0.0 + (_FresnelIntensity - 1.0) * (10.0 - 0.0) / (0.0 - 1.0)) ) );
			float clampResult209 = clamp( fresnelNode136 , 0.0 , 1.0 );
			float4 lerpResult133 = lerp( lerpResult142 , _FresnelColor , clampResult209);
			float4 blendOpSrc300 = ( 0.0 * tex2D( _NoiseTexture, lerpResult24.xy ) );
			float4 blendOpDest300 = lerpResult133;
			float screenDepth163 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth163 = abs( ( screenDepth163 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( ( tex2D( _NoiseTexture, ( ( (0.0 + (_FoamSpeed - 0.0) * (2.5 - 0.0) / (1.0 - 0.0)) * _Time.y ) + ( i.uv_texcoord * (30.0 + (_FoamScale - 0.0) * (1.0 - 30.0) / (1.0 - 0.0)) ) ) ).r * (0.0 + (_FoamDistance - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) ) ) );
			float clampResult208 = clamp( distanceDepth163 , 0.0 , 1.0 );
			float clampResult160 = clamp( pow( clampResult208 , (1.0 + (_FoamHardness - 0.0) * (10.0 - 1.0) / (1.0 - 0.0)) ) , 0.0 , 1.0 );
			float temp_output_156_0 = ( ( 1.0 - clampResult160 ) * _FoamOpacity );
			float screenDepth191 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth191 = abs( ( screenDepth191 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( (0.0 + (_FoamDistance - 0.0) * (15.0 - 0.0) / (1.0 - 0.0)) ) );
			float clampResult207 = clamp( distanceDepth191 , 0.0 , 1.0 );
			float FoamScale330 = _FoamScale;
			float temp_output_185_0 = ( ( 1.0 - clampResult207 ) * ( tex2D( _NoiseTexture, ( ( _Time.y * _FoamSpeed ) + ( i.uv_texcoord * (15.0 + (FoamScale330 - 0.0) * (1.0 - 15.0) / (1.0 - 0.0)) ) ) ).r * (0.0 + (_FoamOpacity - 0.0) * (0.85 - 0.0) / (1.0 - 0.0)) ) );
			float3 temp_cast_3 = (1.0).xxx;
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 lerpResult7 = lerp( temp_cast_3 , ase_lightColor.rgb , 0.75);
			float3 normalizeResult232 = normalize( ( _WorldSpaceCameraPos - ase_worldPos ) );
			float2 temp_cast_5 = (_ReflectionsCutoffScrollSpeed).xx;
			float2 panner342 = ( 1.0 * _Time.y * temp_cast_5 + ( i.uv_texcoord * (2.0 + (_ReflectionsCutoffScale - 0.0) * (10.0 - 2.0) / (10.0 - 0.0)) ));
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult108 = dot( reflect( -normalizeResult232 , (WorldNormalVector( i , UnpackNormal( tex2D( _NormalMap, panner342 ) ) )) ) , ase_worldlightDir );
			float3 clampResult120 = clamp( ( ( pow( dotResult108 , exp( (0.0 + (_ReflectionsCutoff - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) ) ) * ase_lightColor.rgb ) * temp_output_37_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
			o.Emission = ( ( ( ( blendOpSrc300 + blendOpDest300 ) + ( _FoamColor * temp_output_156_0 ) + ( _FoamColor * temp_output_185_0 ) ) * float4( lerpResult7 , 0.0 ) ) + float4( clampResult120 , 0.0 ) ).rgb;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18935
2353;77;1454;748;6245.087;1615.84;2.076408;True;False
Node;AmplifyShaderEditor.RangedFloatNode;177;-4944.057,-773.5247;Float;False;Property;_FoamSpeed;Foam Speed;10;0;Create;True;0;0;0;False;0;False;0.125;0.125;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-4811.354,-430.5248;Float;False;Property;_FoamScale;Foam Scale;9;0;Create;True;0;0;0;False;0;False;0.2;0.1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;180;-4458.059,-693.924;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;182;-4490.054,-610.6247;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;328;-4630.549,-826.6267;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;327;-4463.134,-463.9725;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;30;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-4264.715,1701.584;Float;False;Property;_ReflectionsScale;Reflections Scale;18;0;Create;True;0;0;0;False;0;False;4.8;6;1;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-4205.662,-731.6252;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;339;-6050.331,878.7787;Float;False;Property;_ReflectionsCutoffScale;Reflections Cutoff Scale;21;0;Create;True;0;0;0;False;0;False;3;5;1;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;41;-4248.541,1580.583;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-4204.356,-630.2246;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;173;-5232.027,-871.7961;Float;True;Property;_NoiseTexture;Noise Texture;24;0;Create;True;0;0;0;False;0;False;None;41d01fe6e8db970449385e7f43d4aa06;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;172;-3995.271,-284.7159;Float;False;Property;_FoamDistance;Foam Distance;7;0;Create;True;0;0;0;False;0;False;0.05;0.04;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;45;-5207.198,1505.125;Float;True;Property;_NormalMap;Normal Map;23;1;[Normal];Create;True;0;0;0;False;0;False;None;db76e0ea49f14f147bee066871c70d88;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;253;-4303.472,-876.855;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;247;-4250.948,1793.32;Float;False;Property;_ReflectionsScrollSpeed;Reflections Scroll Speed;19;0;Create;True;0;0;0;False;0;False;0.05;0.035;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;174;-3972.264,-708.325;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;338;-5567.157,752.7777;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;344;-5761.197,856.1603;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;10;False;3;FLOAT;2;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-3936.153,1637.801;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;276;-4240.283,1404.784;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;40;-3749.059,1646.234;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-3682.716,1829.842;Float;False;Property;_ReflectionsOpacity;Reflections Opacity;17;0;Create;True;0;0;0;False;0;False;0.65;0.31;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;325;-3652.749,-477.7855;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;170;-3746.261,-764.1719;Inherit;True;Property;_TextureSample3;Texture Sample 3;32;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceCameraPos;230;-4913.765,378.1346;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;340;-5569.564,965.5146;Float;False;Property;_ReflectionsCutoffScrollSpeed;Reflections Cutoff Scroll Speed;22;0;Create;True;0;0;0;False;0;False;-0.025;-0.025;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;330;-4469.236,-206.2479;Inherit;False;FoamScale;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;228;-4904.476,521.1351;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;341;-5254.77,809.9957;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;342;-5067.676,818.4287;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0.1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;167;-3366.104,-652.344;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;231;-4581.978,439.0242;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;336;-3361.807,1801.903;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;39;-3517.411,1571.542;Inherit;True;Property;_TextureSample0;Texture Sample 0;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;331;-3859.902,277.3207;Inherit;False;330;FoamScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-3110.718,1526.32;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;218;-4691.716,685.0096;Inherit;True;Property;_TextureSample5;Texture Sample 5;40;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;232;-4417.712,513.7786;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;26;-4002.373,-1267.839;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;162;-3269.352,-510.5533;Float;False;Property;_FoamHardness;Foam Hardness;6;0;Create;True;0;0;0;False;0;False;0.33;0.33;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;332;-3627.084,231.9495;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;15;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;163;-3081.554,-677.7533;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-3973.617,-1062.813;Float;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;200;-3682.04,86.26563;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;333;-4112.28,-14.44718;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;198;-3650.654,-47.96329;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-3707.875,-2148.751;Float;False;Property;_ShallowColorDepth;Shallow Color Depth;2;0;Create;True;0;0;0;False;0;False;2.75;4;0;15;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;137;-3600.203,-1822.516;Float;False;Property;_FresnelIntensity;Fresnel Intensity;12;0;Create;True;0;0;0;False;0;False;0.4;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;251;-4776.923,-107.4519;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.NegateNode;242;-4248.241,599.1326;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;215;-4345.526,692.3788;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;291;-2844.731,1476.337;Inherit;False;Turbulence;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;197;-3336.181,88.40479;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;324;-2935.057,-559.5585;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-3701.617,-1171.813;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-4306.912,1086.504;Float;False;Property;_ReflectionsCutoff;Reflections Cutoff;20;0;Create;True;0;0;0;False;0;False;0.35;0.45;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-3336.181,-23.5952;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-3703.617,-1039.813;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;208;-2823.62,-716.0146;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;335;-3275.425,-339.7545;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;15;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;292;-3528.146,-940.1373;Inherit;False;291;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;146;-3349.992,-2141.228;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;191;-2697.633,-376.1268;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ReflectOpNode;234;-4061.457,620.4695;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;308;-2133.019,1695.526;Float;False;Property;_WaveAmplitude;Wave Amplitude;14;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;195;-3120.445,19.70538;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;109;-4066.71,922.8796;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;323;-3277.878,-1866.145;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;252;-3613.041,-168.5398;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TFHCRemapNode;104;-3888.712,1137.879;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;322;-3389.377,2215.97;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PowerNode;161;-2655.053,-662.853;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;307;-2032.02,1785.526;Float;False;Constant;_Float5;Float 5;16;0;Create;True;0;0;0;False;0;False;30;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;158;-2602.202,-541.8613;Float;False;Property;_FoamOpacity;Foam Opacity;8;0;Create;True;0;0;0;False;0;False;0.65;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;31;-3495.617,-1122.813;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;309;-1950.244,1497.917;Inherit;True;Property;_TextureSample2;Texture Sample 2;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;311;-1876.244,1385.917;Float;False;Property;_WaveSpeed;Wave Speed;16;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ExpOpNode;106;-3647.712,1138.879;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;312;-1878.244,1284.917;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;211;-3077.25,-2206.991;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;160;-2501.454,-669.5529;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;207;-2396.567,-361.3667;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;108;-3696.712,864.8806;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;136;-2948.464,-1897.474;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;24;-3242.824,-1224.616;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;334;-2164.44,-197.7451;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.85;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;145;-3355.598,-2327.641;Float;False;Property;_DeepColor;Deep Color;1;0;Create;True;0;0;0;False;0;False;0,0.3333333,0.8509804,1;0.3396226,0.254717,0.2963979,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;283;-4195.99,-1381.379;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ColorNode;144;-3357.609,-2499.496;Float;False;Property;_ShallowColor;Shallow Color;0;0;Create;True;0;0;0;False;0;False;0,0.6117647,1,1;0.3960784,0.6039216,0.5524715,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;-1784.02,1713.526;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;193;-2878.555,-197.9528;Inherit;True;Property;_TextureSample4;Texture Sample 4;38;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;209;-2662.726,-1922.949;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;304;-2989.008,-1456.633;Float;False;Constant;_Float4;Float 4;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;116;-3440.712,1217.878;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.OneMinusNode;157;-1929.416,-693.1175;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;313;-1579.244,1499.917;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;250;-3044.558,-1351.932;Inherit;True;Property;_TextureSample1;Texture Sample 1;28;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-1935.248,-323.0315;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;142;-2870.249,-2318.203;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;135;-2916.434,-2149.101;Float;False;Property;_FresnelColor;Fresnel Color;11;0;Create;True;0;0;0;False;0;False;0.8313726,0.8313726,0.8313726,1;0.8313726,0.8313726,0.8313726,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;189;-1933.043,-430.3739;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;314;-1627.244,1347.917;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;107;-3448.712,958.8796;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;293;-1986.044,216.6788;Float;False;Property;_OpacityDepth;Opacity Depth;4;0;Create;True;0;0;0;False;0;False;6.5;6.5;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;185;-1741.658,-429.6407;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;155;-1896.206,-941.9939;Float;False;Property;_FoamColor;Foam Color;5;0;Create;True;0;0;0;False;0;False;0.8705882,0.8705882,0.8705882,1;0.745283,0.745283,0.745283,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;329;-2615.246,-1377.247;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;115;-3169.713,1096.88;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;317;-1546.244,1649.917;Float;False;Property;_WaveIntensity;Wave Intensity;15;0;Create;True;0;0;0;False;0;False;0.15;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-1623.072,-756.7065;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;315;-1340.244,1457.917;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;133;-2300.623,-2060.479;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;153;-1245.794,-979.9147;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;184;-1219.319,-585.1722;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;316;-1137.244,1446.917;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-2655.682,1010.661;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendOpsNode;300;-1325.759,-1226.356;Inherit;False;LinearDodge;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;326;-1219.781,1593.017;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;10;-1053.874,-313.6953;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;8;-1038.874,-393.6953;Float;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;294;-1660.639,213.1233;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1100.037,-186.0254;Float;False;Constant;_LightColorInfluence;Light Color Influence;17;0;Create;True;0;0;0;False;0;False;0.75;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;93;-674.4756,814.4929;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightAttenuation;94;-704.5146,967.3743;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-744,-593;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;319;-1023.477,1264.383;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;88;-627.7927,499.2284;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;295;-1360.963,241.3745;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;296;-1538.928,130.925;Float;False;Property;_Opacity;Opacity;3;0;Create;True;0;0;0;False;0;False;0.5;0.65;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;120;-2266.853,884.6415;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;186;-1509.561,-550.1731;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;318;-914.4769,1459.383;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-763.7581,-295.9514;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-502,-347;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;87;-256.8796,298.513;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-362.7733,593.2342;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;297;-1184.928,160.9251;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;95;-343.2908,745.5903;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;320;-642.4764,1401.383;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;321;-371.4765,1194.383;Float;True;Property;_Waves;Waves;13;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;90;-32.40416,300.2202;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;3;-161,9;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;299;-1026.928,156.9251;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;203;278.7999,46.39999;Float;False;True;-1;3;;0;0;CustomLighting;Toon/TFF_ToonWater;False;False;False;False;False;False;True;False;True;False;False;False;False;False;True;True;False;False;False;False;True;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;328;0;177;0
WireConnection;327;0;183;0
WireConnection;175;0;328;0
WireConnection;175;1;180;0
WireConnection;176;0;182;0
WireConnection;176;1;327;0
WireConnection;253;0;173;0
WireConnection;174;0;175;0
WireConnection;174;1;176;0
WireConnection;344;0;339;0
WireConnection;42;0;41;0
WireConnection;42;1;43;0
WireConnection;276;0;45;0
WireConnection;40;0;42;0
WireConnection;40;2;247;0
WireConnection;325;0;172;0
WireConnection;170;0;253;0
WireConnection;170;1;174;0
WireConnection;330;0;183;0
WireConnection;341;0;338;0
WireConnection;341;1;344;0
WireConnection;342;0;341;0
WireConnection;342;2;340;0
WireConnection;167;0;170;1
WireConnection;167;1;325;0
WireConnection;231;0;230;0
WireConnection;231;1;228;0
WireConnection;336;0;38;0
WireConnection;39;0;276;0
WireConnection;39;1;40;0
WireConnection;37;0;39;2
WireConnection;37;1;336;0
WireConnection;218;0;45;0
WireConnection;218;1;342;0
WireConnection;232;0;231;0
WireConnection;332;0;331;0
WireConnection;163;0;167;0
WireConnection;333;0;177;0
WireConnection;251;0;173;0
WireConnection;242;0;232;0
WireConnection;215;0;218;0
WireConnection;291;0;37;0
WireConnection;197;0;200;0
WireConnection;197;1;332;0
WireConnection;324;0;162;0
WireConnection;27;0;26;1
WireConnection;27;1;29;0
WireConnection;196;0;198;0
WireConnection;196;1;333;0
WireConnection;28;0;26;2
WireConnection;28;1;29;0
WireConnection;208;0;163;0
WireConnection;335;0;172;0
WireConnection;146;0;150;0
WireConnection;191;0;335;0
WireConnection;234;0;242;0
WireConnection;234;1;215;0
WireConnection;195;0;196;0
WireConnection;195;1;197;0
WireConnection;323;0;137;0
WireConnection;252;0;251;0
WireConnection;104;0;103;0
WireConnection;322;0;45;0
WireConnection;161;0;208;0
WireConnection;161;1;324;0
WireConnection;31;0;27;0
WireConnection;31;1;28;0
WireConnection;309;0;322;0
WireConnection;106;0;104;0
WireConnection;211;0;146;0
WireConnection;160;0;161;0
WireConnection;207;0;191;0
WireConnection;108;0;234;0
WireConnection;108;1;109;0
WireConnection;136;3;323;0
WireConnection;24;0;26;0
WireConnection;24;1;31;0
WireConnection;24;2;292;0
WireConnection;334;0;158;0
WireConnection;283;0;173;0
WireConnection;310;0;308;0
WireConnection;310;1;307;0
WireConnection;193;0;252;0
WireConnection;193;1;195;0
WireConnection;209;0;136;0
WireConnection;157;0;160;0
WireConnection;313;0;309;3
WireConnection;313;1;310;0
WireConnection;250;0;283;0
WireConnection;250;1;24;0
WireConnection;188;0;193;1
WireConnection;188;1;334;0
WireConnection;142;0;144;0
WireConnection;142;1;145;0
WireConnection;142;2;211;0
WireConnection;189;0;207;0
WireConnection;314;0;312;0
WireConnection;314;1;311;0
WireConnection;107;0;108;0
WireConnection;107;1;106;0
WireConnection;185;0;189;0
WireConnection;185;1;188;0
WireConnection;329;0;304;0
WireConnection;329;1;250;0
WireConnection;115;0;107;0
WireConnection;115;1;116;1
WireConnection;156;0;157;0
WireConnection;156;1;158;0
WireConnection;315;0;314;0
WireConnection;315;1;313;0
WireConnection;133;0;142;0
WireConnection;133;1;135;0
WireConnection;133;2;209;0
WireConnection;153;0;155;0
WireConnection;153;1;156;0
WireConnection;184;0;155;0
WireConnection;184;1;185;0
WireConnection;316;0;315;0
WireConnection;117;0;115;0
WireConnection;117;1;37;0
WireConnection;300;0;329;0
WireConnection;300;1;133;0
WireConnection;326;0;317;0
WireConnection;294;0;293;0
WireConnection;12;0;300;0
WireConnection;12;1;153;0
WireConnection;12;2;184;0
WireConnection;295;0;294;0
WireConnection;120;0;117;0
WireConnection;186;0;156;0
WireConnection;186;1;185;0
WireConnection;318;0;316;0
WireConnection;318;1;326;0
WireConnection;7;0;8;0
WireConnection;7;1;10;1
WireConnection;7;2;11;0
WireConnection;5;0;12;0
WireConnection;5;1;7;0
WireConnection;87;0;120;0
WireConnection;87;1;88;0
WireConnection;91;0;93;1
WireConnection;91;1;94;0
WireConnection;297;0;186;0
WireConnection;297;1;296;0
WireConnection;297;2;295;0
WireConnection;95;0;94;0
WireConnection;320;0;319;0
WireConnection;320;1;318;0
WireConnection;321;0;320;0
WireConnection;90;0;87;0
WireConnection;90;1;91;0
WireConnection;90;2;95;0
WireConnection;3;0;5;0
WireConnection;3;1;120;0
WireConnection;299;0;297;0
WireConnection;203;2;3;0
WireConnection;203;9;299;0
WireConnection;203;13;90;0
WireConnection;203;11;321;0
ASEEND*/
//CHKSM=975E732A5DA4A8EC105D9C843DCAD0959BAB0DE6