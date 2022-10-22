Shader "Hidden/LineShader" 
{
SubShader 
{
	Blend SrcAlpha OneMinusSrcAlpha 
	ZWrite Off 
	Cull Off 
	Fog { Mode Off }
	
	Pass 
	{  
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		uniform float4 _LineColor;
		uniform sampler2D _LineTexture;

		struct v2f
		{
			float2 texcoord : TEXCOORD0;
			float4 vertex : POSITION;
		};

		v2f vert (float2 texcoord : TEXCOORD0, float4 vertex : POSITION)
		{
			v2f o;
			#if UNITY_VERSION >= 560 
			o.vertex = mul(UNITY_MATRIX_MVP, vertex); 
			#else 
			#if UNITY_SHADER_NO_UPGRADE 
			o.vertex = mul(UNITY_MATRIX_MVP, vertex);
			#else
			o.vertex = mul(UNITY_MATRIX_MVP, vertex);
			#endif 
			#endif
			o.texcoord = texcoord;
			return o;
		}

		float4 frag (v2f i) : COLOR
		{
			return _LineColor * tex2D (_LineTexture, i.texcoord);
		}
		ENDCG
	}
}

}