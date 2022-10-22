Shader "Hidden/Broccoli/SproutLabExtra"
{
    // Metallic red
    // Smoothness green
    // AO blue
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0
        _Glossiness ("Glossiness", Range(0,1)) = 0
        _IsLinearColorSpace ("Is Linear Color Space", Float) = 0
    }
    SubShader {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 100

        Lighting Off

        Pass {
            Name "Albedo"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_fog
                
                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 uv3: TEXCOORD3;
                    fixed4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 uv3: TEXCOORD3;
                    fixed4 color : COLOR;
                    UNITY_FOG_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed _Cutoff;
                fixed _Metallic;
                fixed _Glossiness;
                float _IsLinearColorSpace;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    //UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.color = v.color;
                    o.uv3 = v.uv3;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }
                float3 hsv_to_rgb(float3 HSV)
                {
                    float3 RGB = HSV.z;
        
                    float var_h = HSV.x * 6;
                    float var_i = floor(var_h);   // Or ... var_i = floor( var_h )
                    float var_1 = HSV.z * (1.0 - HSV.y);
                    float var_2 = HSV.z * (1.0 - HSV.y * (var_h-var_i));
                    float var_3 = HSV.z * (1.0 - HSV.y * (1-(var_h-var_i)));
                    if      (var_i == 0) { RGB = float3(HSV.z, var_3, var_1); }
                    else if (var_i == 1) { RGB = float3(var_2, HSV.z, var_1); }
                    else if (var_i == 2) { RGB = float3(var_1, HSV.z, var_3); }
                    else if (var_i == 3) { RGB = float3(var_1, var_2, HSV.z); }
                    else if (var_i == 4) { RGB = float3(var_3, var_1, HSV.z); }
                    else                 { RGB = float3(HSV.z, var_1, var_2); }            
                    return (RGB);
                }
                
                fixed4 frag (v2f i) : SV_Target
                {
                    if (i.uv3.z == 0) {
                        fixed4 colBlue = fixed4(0, 0, 1, 1);
                        return colBlue;
                    }
                    fixed4 col = tex2D(_MainTex, i.texcoord);
                    fixed4 vcol = i.color;
                    clip(col.a * vcol.a - _Cutoff);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    col.g = _Metallic * 1;
                    col.r = _Glossiness * 1;
                    col.b = vcol.r * 1.5;
                    col = saturate (col);
                    
                    if (_IsLinearColorSpace)
                        col.rgb = pow (col.rgb, 2.2);
                        
                    return  col;
                }
                
            ENDCG
        }
    }
}