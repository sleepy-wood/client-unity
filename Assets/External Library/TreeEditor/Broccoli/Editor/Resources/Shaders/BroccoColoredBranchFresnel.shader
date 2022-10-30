Shader "Hidden/Broccoli/Colored Branch Fresnel"
 {
	Properties 
	{
	_InnerColor ("Inner Color", Color) = (0.25, 0.25, 0.25, 0.5)
	_RimColor ("Rim Color", Color) = (0.4, 0.4, 0.4, 0.0)
	_RimPower ("Rim Power", Range(0.5,8.0)) = 5.0
	}
	SubShader 
	{
	Tags { "Queue" = "Transparent" }

	Cull Back
	Blend One One

	CGPROGRAM
	#pragma surface surf Lambert

	struct Input 
	{
		float3 viewDir;
	};

	float4 _InnerColor;
	float4 _RimColor;
	float _RimPower;

	void surf (Input IN, inout SurfaceOutput o) 
	{
		o.Albedo = _InnerColor.rgb * 0.5;
		half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
		o.Emission = _RimColor.rgb * pow (rim, _RimPower);
	}
	ENDCG
	} 
	Fallback "Diffuse"
}