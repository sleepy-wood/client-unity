Shader "Hidden/Broccoli/Billboard Normals" 
{
    Properties
    {
        _MainTex("Base texture", 2D) = "white" {}
        _BumpMap("Normal Map", 2D) = "bump" {}
        _Cutoff  ("Cutoff", Float) = 0.1
    }
 
    Subshader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    LOD 100

        //ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha 
 
        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
 
                struct v2f
                {
                    float4 pos    : SV_POSITION;
                    half3 worldNormal : TEXCOORD0;
                    half3 tspace0 : TEXCOORD1;
                    half3 tspace1 : TEXCOORD2;
                    half3 tspace2 : TEXCOORD3;
                    float2 uv : TEXCOORD4;
                    float3 viewNormal : TEXCOORD5;
                    float4 worldPos: TEXCOORD6;
                };
 
                v2f vert(float4 vertex : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT, float2 uv : TEXCOORD0)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(vertex);
 
                    // transform normal vectors from model space to world space
                    float3 worldNorm =
                        normalize(
                            unity_WorldToObject[0].xyz * normal.x +
                            unity_WorldToObject[1].xyz * normal.y +
                            unity_WorldToObject[2].xyz * normal.z
                            );
 
                    o.worldPos = mul(unity_ObjectToWorld, vertex);
                    o.viewNormal = mul((float3x3)UNITY_MATRIX_V, worldNorm);

                    // UnityCG.cginc file contains function to transform
                    // normal from object to world space, use that
                    o.worldNormal = UnityObjectToWorldNormal(normal);

                    half3 wNormal = UnityObjectToWorldNormal(normal);
                    half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
                    half tangentSign = tangent.w * unity_WorldTransformParams.w;
                    half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                    o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                    o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                    o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
                    o.uv = uv;
 
                    return o;
                }

                // textures from shader properties
                sampler2D _MainTex;
                sampler2D _BumpMap;
                float _Cutoff;
 
                float4 frag(v2f i, fixed facing : VFACE) : SV_Target
                {
                    fixed4 c = 0;
                    // same as from previous shader...
                    half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uv));
                    half4 talbedo = tex2D(_MainTex, i.uv);

                    float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
 
                    float3 NormalBlend_MatcapUV_Detail = i.viewNormal.rgb * float3(-1, -1, 1);
                    float3 NormalBlend_MatcapUV_Base = (mul(UNITY_MATRIX_V, float4(viewDirection, 0)).rgb*float3(-1, -1, 1)) + float3(0, 0, 1);
 
                    float3 noSknewViewNormal = NormalBlend_MatcapUV_Base *
                        dot(NormalBlend_MatcapUV_Base, NormalBlend_MatcapUV_Detail) / NormalBlend_MatcapUV_Base.b - NormalBlend_MatcapUV_Detail;
 
                    noSknewViewNormal = float4(noSknewViewNormal,1);
                    
                    if (facing < 0) {
                        c.rgb = -noSknewViewNormal*0.5+0.5;
                    } else {
                        c.rgb = noSknewViewNormal*0.5+0.5;
                    }
                    c.a = talbedo.a;
                    c.a = clamp(c.a * 2, 0, 1);;
                    clip(c.a - 0.1);
                    return c;
                }
            ENDCG
        }
    }
    Fallback "VertexLit"
}