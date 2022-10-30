Shader "Hidden/Broccoli/Tree Creator Branch Selection" {
// https://www.alanzucconi.com/tag/setglobalfloatarray/
Properties {
    _BranchColor ("Branch Color", Color) = (0.5, 0.5, 0.5, 0.5)
    _SelectedColor ("Selected Color", Color) = (0.4, 0.8, 0.9, 1.0)
    _TunedColor ("Tuned Color", Color) = (0.9, 0.5, 0.5, 0.7)
  }
  SubShader {
    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        float4 _BranchColor;
        float4 _SelectedColor;
        float4 _TunedColor;

        // vertex input: position, UV
        struct appdata {
            float4 vertex : POSITION;
            float4 texcoord : TEXCOORD0;
            float4 texcoord2 : TEXCOORD2;
        };

        struct v2f {
            float4 pos : SV_POSITION;
            float4 uv : TEXCOORD0;
            float4 uv3 : TEXCOORD2;
        };
        
        v2f vert (appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex );
            o.uv = float4( v.texcoord.xy, 0, 0 );
            o.uv3 = v.texcoord2;
            return o;
        }
        
        half4 frag( v2f i ) : SV_Target {
            if (i.uv3.z == 1) {
                return _TunedColor;
            } else if (i.uv3.z == 2) {
                return _SelectedColor;
            } else if (i.uv3.z == 3) {
                return _SelectedColor;
            }
            /*
            if (_BranchId == i.uv3.x) {
              half4 c = frac( i.uv );
              if (any(saturate(i.uv) - i.uv))
                  c.b = 0.5;
              return c;
            }
            */
            return _BranchColor;
        }
        ENDCG
    }
  }
  FallBack "Diffuse"
}