// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon/TFF_ToonFire"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_BurnSpeed("Burn Speed", Range( 0 , 1)) = -0.75
		_ColorBlend("Color Blend", Range( 0 , 1)) = 0.65
		_TopColor("Top Color", Color) = (1,0,0.2424197,0)
		_BaseColor("Base Color", Color) = (1,0.808957,0,0)
		_InnerColor("Inner Color", Color) = (0,0.5678592,1,0)
		_OpacityCutoff("Opacity Cutoff", Range( 0 , 1)) = 0.1
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		_MaskTexture("Mask Texture", 2D) = "white" {}
		_Shading("Shading", Range( 0 , 1)) = 0
		_Brightness("Brightness", Range( 0 , 5)) = 1.49
		_DepthFade("Depth Fade", Range( 0 , 10)) = 0
		_Offset("Offset", Range( 0 , 1)) = 0.125
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.5
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nolightmap  nodirlightmap vertex:vertexDataFunc 
		struct Input
		{
			float4 screenPos;
			float2 uv_texcoord;
		};

		uniform float _Offset;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFade;
		uniform float4 _BaseColor;
		uniform float _ColorBlend;
		uniform float4 _TopColor;
		uniform sampler2D _NoiseTexture;
		uniform float _BurnSpeed;
		uniform sampler2D _MaskTexture;
		uniform float4 _MaskTexture_ST;
		uniform float4 _InnerColor;
		uniform float _Shading;
		uniform float _Brightness;
		uniform float _OpacityCutoff;
		uniform float _Cutoff = 0.5;


		inline float4 ASESafeNormalize(float4 inVec)
		{
			float dp3 = max( 0.001f , dot( inVec , inVec ) );
			return inVec* rsqrt( dp3);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			//Calculate new billboard vertex position and normal;
			float3 upCamVec = normalize ( UNITY_MATRIX_V._m10_m11_m12 );
			float3 forwardCamVec = -normalize ( UNITY_MATRIX_V._m20_m21_m22 );
			float3 rightCamVec = normalize( UNITY_MATRIX_V._m00_m01_m02 );
			float4x4 rotationCamMatrix = float4x4( rightCamVec, 0, upCamVec, 0, forwardCamVec, 0, 0, 0, 0, 1 );
			v.normal = normalize( mul( float4( v.normal , 0 ), rotationCamMatrix )).xyz;
			v.tangent.xyz = normalize( mul( float4( v.tangent.xyz , 0 ), rotationCamMatrix )).xyz;
			//This unfortunately must be made to take non-uniform scaling into account;
			//Transform to world coords, apply rotation and transform back to local;
			v.vertex = mul( v.vertex , unity_ObjectToWorld );
			v.vertex = mul( v.vertex , rotationCamMatrix );
			v.vertex = mul( v.vertex , unity_WorldToObject );
			float4 transform420 = mul(unity_ObjectToWorld,float4( 0,0,0,1 ));
			float4 normalizeResult422 = ASESafeNormalize( ( float4( _WorldSpaceCameraPos , 0.0 ) - transform420 ) );
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz += ( ( _Offset * normalizeResult422 ) + float4( ( 0.1 * ( 0 + ase_vertex3Pos ) ) , 0.0 ) ).xyz;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth408 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth408 = saturate( abs( ( screenDepth408 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) ) );
			float smoothstepResult381 = smoothstep( 0.0 , _ColorBlend , i.uv_texcoord.y);
			float temp_output_425_0 = (0.0 + (_BurnSpeed - 0.0) * (-2.0 - 0.0) / (1.0 - 0.0));
			float2 appendResult357 = (float2(0.0 , temp_output_425_0));
			float2 uv_TexCoord350 = i.uv_texcoord + float2( 2.6,0 );
			float2 panner359 = ( 1.0 * _Time.y * appendResult357 + ( 1.0 * uv_TexCoord350 ));
			float2 appendResult358 = (float2(0.0 , ( temp_output_425_0 * 2 )));
			float2 panner360 = ( 1.0 * _Time.y * appendResult358 + ( uv_TexCoord350 * 0.5 ));
			float2 uv_MaskTexture = i.uv_texcoord * _MaskTexture_ST.xy + _MaskTexture_ST.zw;
			float4 tex2DNode373 = tex2D( _MaskTexture, uv_MaskTexture );
			o.Emission = ( saturate( distanceDepth408 ) * ( ( ( ( ( _BaseColor * ( 1.0 - smoothstepResult381 ) ) + ( smoothstepResult381 * _TopColor ) ) * ( 1.0 - ( ( ( tex2D( _NoiseTexture, panner359 ).r * tex2D( _NoiseTexture, panner360 ).r ) + tex2DNode373.r ) * tex2DNode373.r ) ) ) + ( _InnerColor * ( ( ( tex2D( _NoiseTexture, panner359 ).r * tex2D( _NoiseTexture, panner360 ).r ) + tex2DNode373.r ) * tex2DNode373.r ) ) ) * saturate( ( ( 1.0 - _Shading ) + ( ( ( tex2D( _NoiseTexture, panner359 ).r * tex2D( _NoiseTexture, panner360 ).r ) + tex2DNode373.r ) * tex2DNode373.r ) ) ) * _Brightness ) ).rgb;
			o.Alpha = 1;
			clip( step( (0.02 + (_OpacityCutoff - 0.0) * (1.0 - 0.02) / (1.0 - 0.0)) , ( ( ( tex2D( _NoiseTexture, panner359 ).r * tex2D( _NoiseTexture, panner360 ).r ) + tex2DNode373.r ) * tex2DNode373.r ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18935
2353;77;1454;748;1363.546;-1291.67;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;354;-3626.603,676.8858;Inherit;False;Property;_BurnSpeed;Burn Speed;1;0;Create;True;0;0;0;False;0;False;-0.75;0.28;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;425;-3259.867,681.3926;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;353;-2908.696,765.7859;Inherit;False;Constant;_Noise1Scale;Noise 1 Scale;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;424;-2899.304,1115.171;Inherit;False;2;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;356;-2909.196,1009.785;Inherit;False;Constant;_Noise2Scale;Noise 2 Scale;1;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;350;-2948.494,866.4176;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;2.6,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;357;-2656.695,659.7859;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;358;-2653.695,1097.786;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;351;-2654.493,777.4178;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;352;-2653.593,987.9178;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;359;-2455.695,706.7859;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;346;-2493.63,827.9129;Inherit;True;Property;_NoiseTexture;Noise Texture;7;0;Create;True;0;0;0;False;0;False;None;1107fb1c60b1cbe40b0dc1a2684c79a8;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.PannerNode;360;-2455.695,1027.785;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;345;-2214.83,941.0128;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;347;-2216.231,734.3362;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;348;-1842.828,856.7799;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;380;-1570.315,306.6875;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;382;-1615.476,453.4054;Inherit;False;Property;_ColorBlend;Color Blend;2;0;Create;True;0;0;0;False;0;False;0.65;0.575;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;373;-2215.538,1148.101;Inherit;True;Property;_MaskTexture;Mask Texture;8;0;Create;True;0;0;0;False;0;False;-1;None;6f1976b262bf3224a82eca3e872fb819;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;381;-1267.073,340.3058;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;374;-1644.863,855.941;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;375;-1431.422,855.9408;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;386;-1080.574,457.3056;Inherit;False;Property;_TopColor;Top Color;3;0;Create;True;0;0;0;False;0;False;1,0,0.2424197,0;0.4433962,0.1726722,0.06901923,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;383;-1032.475,283.1052;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;387;-1092.276,75.10513;Inherit;False;Property;_BaseColor;Base Color;4;0;Create;True;0;0;0;False;0;False;1,0.808957,0,0;1,0.1843137,0.4045319,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;384;-827.0764,262.3051;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;405;-789.4717,921.2824;Inherit;False;Property;_Shading;Shading;9;0;Create;True;0;0;0;False;0;False;0;0.348;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;363;-1142.045,852.4604;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;385;-825.7753,361.1058;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;367;-669.4366,708.0319;Inherit;False;Property;_InnerColor;Inner Color;5;0;Create;True;0;0;0;False;0;False;0,0.5678592,1,0;0.5849056,0.2708778,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;388;-582.6733,307.8051;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;390;-621.49,501.6916;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceCameraPos;419;-336.5614,1256.69;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;406;-668.5184,981.1091;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;420;-312.0414,1428.288;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;421;22.3819,1358.644;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;389;-439.6733,406.6051;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BillboardNode;413;-284.1115,1835.491;Inherit;False;Spherical;False;True;0;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;409;5.010071,669.2429;Inherit;False;Property;_DepthFade;Depth Fade;11;0;Create;True;0;0;0;False;0;False;0;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;404;-524.3671,1051.451;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;369;-390.6364,858.4323;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;412;-282.1115,1916.491;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;372;-120.1453,833.3228;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;362;-1130.331,1158.878;Inherit;False;Property;_OpacityCutoff;Opacity Cutoff;6;0;Create;True;0;0;0;False;0;False;0.1;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;407;-126.9899,1098.243;Inherit;False;Property;_Brightness;Brightness;10;0;Create;True;0;0;0;False;0;False;1.49;3.81;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;418;147.4718,1255.512;Inherit;False;Property;_Offset;Offset;12;0;Create;True;0;0;0;False;0;False;0.125;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;408;257.0101,689.2429;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;422;196.965,1380.991;Inherit;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;415;-23.23558,1784.261;Inherit;False;Constant;_Float0;Float 0;11;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;403;-126.6487,1003.912;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;414;-5.754746,1870.56;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;416;165.7943,1846.385;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;573.8351,1259.248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCRemapNode;391;-790.5807,1161.997;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.02;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;410;591.0101,741.2429;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;402;206.6938,879.1827;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;417;823.908,1271.295;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StepOpNode;365;-519.1299,1163.179;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;411;780.2628,921.1683;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;203;1038.798,856.5071;Float;False;True;-1;3;;0;0;Standard;Toon/TFF_ToonFire;False;False;False;False;False;False;True;False;True;False;False;False;False;False;True;True;False;False;False;False;True;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;18;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Spherical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;425;0;354;0
WireConnection;424;0;425;0
WireConnection;357;1;425;0
WireConnection;358;1;424;0
WireConnection;351;0;353;0
WireConnection;351;1;350;0
WireConnection;352;0;350;0
WireConnection;352;1;356;0
WireConnection;359;0;351;0
WireConnection;359;2;357;0
WireConnection;360;0;352;0
WireConnection;360;2;358;0
WireConnection;345;0;346;0
WireConnection;345;1;360;0
WireConnection;347;0;346;0
WireConnection;347;1;359;0
WireConnection;348;0;347;1
WireConnection;348;1;345;1
WireConnection;381;0;380;2
WireConnection;381;2;382;0
WireConnection;374;0;348;0
WireConnection;374;1;373;1
WireConnection;375;0;374;0
WireConnection;375;1;373;1
WireConnection;383;0;381;0
WireConnection;384;0;387;0
WireConnection;384;1;383;0
WireConnection;363;0;375;0
WireConnection;385;0;381;0
WireConnection;385;1;386;0
WireConnection;388;0;384;0
WireConnection;388;1;385;0
WireConnection;390;0;363;0
WireConnection;406;0;405;0
WireConnection;421;0;419;0
WireConnection;421;1;420;0
WireConnection;389;0;388;0
WireConnection;389;1;390;0
WireConnection;404;0;406;0
WireConnection;404;1;363;0
WireConnection;369;0;367;0
WireConnection;369;1;363;0
WireConnection;372;0;389;0
WireConnection;372;1;369;0
WireConnection;408;0;409;0
WireConnection;422;0;421;0
WireConnection;403;0;404;0
WireConnection;414;0;413;0
WireConnection;414;1;412;0
WireConnection;416;0;415;0
WireConnection;416;1;414;0
WireConnection;423;0;418;0
WireConnection;423;1;422;0
WireConnection;391;0;362;0
WireConnection;410;0;408;0
WireConnection;402;0;372;0
WireConnection;402;1;403;0
WireConnection;402;2;407;0
WireConnection;417;0;423;0
WireConnection;417;1;416;0
WireConnection;365;0;391;0
WireConnection;365;1;363;0
WireConnection;411;0;410;0
WireConnection;411;1;402;0
WireConnection;203;2;411;0
WireConnection;203;10;365;0
WireConnection;203;11;417;0
ASEEND*/
//CHKSM=5D8D20DF808E46C40BE7266B4C7083F22307A89A