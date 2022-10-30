Shader "Broccoli/SproutLabComposite"
{
    Properties
    {
        _MainTex ("Base (RGB) Transparency (A)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)

        [Toggle(EFFECT_HUE_VARIATION)] _HueVariationKwToggle("Hue Variation", Float) = 0
        _HueVariationColor ("Hue Variation Color", Color) = (1.0,0.5,0.0,0.1)

        [Toggle(EFFECT_BUMP)] _NormalMapKwToggle("Normal Mapping", Float) = 0
        _BumpMap ("Normalmap", 2D) = "bump" {}

        _ExtraTex ("Smoothness (R), Metallic (G), AO (B)", 2D) = "(0.5, 0.0, 1.0)" {}
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0

        [Toggle(EFFECT_SUBSURFACE)] _SubsurfaceKwToggle("Subsurface", Float) = 0
        _SubsurfaceTex ("Subsurface (RGB)", 2D) = "white" {}
        _SubsurfaceColor ("Subsurface Color", Color) = (1,1,1,1)
        _SubsurfaceIndirect ("Subsurface Indirect", Range(0.0, 1.0)) = 0.25

        [Toggle(EFFECT_BILLBOARD)] _BillboardKwToggle("Billboard", Float) = 0
        _BillboardShadowFade ("Billboard Shadow Fade", Range(0.0, 1.0)) = 0.5

        [Enum(No,2,Yes,0)] _TwoSided ("Two Sided", Int) = 2 // enum matches cull mode
        [KeywordEnum(None,Fastest,Fast,Better,Best,Palm)] _WindQuality ("Wind Quality", Range(0,5)) = 0

        _TintColor ("Tint Color", Color) = (0.5,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "DisableBatching"="LODFading"
        }
        LOD 400
        Cull [_TwoSided]

        CGPROGRAM
            #pragma surface SpeedTreeSurf2 SpeedTreeSubsurface vertex:SpeedTreeVert dithercrossfade addshadow
            #pragma target 3.0
            #pragma multi_compile_vertex LOD_FADE_PERCENTAGE
            #pragma instancing_options assumeuniformscaling maxcount:50

            #pragma shader_feature _WINDQUALITY_NONE _WINDQUALITY_FASTEST _WINDQUALITY_FAST _WINDQUALITY_BETTER _WINDQUALITY_BEST _WINDQUALITY_PALM
            #pragma shader_feature EFFECT_BILLBOARD
            #pragma shader_feature EFFECT_HUE_VARIATION
            #pragma shader_feature EFFECT_SUBSURFACE
            #pragma shader_feature EFFECT_BUMP
            #pragma shader_feature EFFECT_EXTRA_TEX

            #define ENABLE_WIND
            #define EFFECT_BACKSIDE_NORMALS
            #include "SpeedTree8Common.cginc"

            float4 _TintColor;
            sampler2D _OverlayTex;

            struct Input2
            {
                half2   uv_MainTex  : TEXCOORD0;
                fixed4  color       : COLOR;

                #ifdef EFFECT_BACKSIDE_NORMALS
                    fixed   facing      : VFACE;
                #endif
            };

            void SpeedTreeSurf2(Input2 IN, inout SurfaceOutputStandard OUT)
            {
                fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                // transparency
                OUT.Alpha = color.a * IN.color.a;
                clip(OUT.Alpha - 0.3333);

                // color
                OUT.Albedo = color.rgb;

                // hue variation
                //#ifdef EFFECT_HUE_VARIATION
                // Custom tint per leaf using B color channel to lerp.
                if (IN.color.g == 0) {
                    half3 shiftedColor = lerp(OUT.Albedo, _TintColor.rgb, IN.color.b);

                    // preserve vibrance
                    half maxBase = max(OUT.Albedo.r, max(OUT.Albedo.g, OUT.Albedo.b));
                    half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
                    maxBase /= newMaxBase;
                    maxBase = maxBase * 0.5f + 0.5f;
                    shiftedColor.rgb *= maxBase;

                    OUT.Albedo = saturate(shiftedColor);
                }
                //#endif

                // normal
                #ifdef EFFECT_BUMP
                    OUT.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
                #elif defined(EFFECT_BACKSIDE_NORMALS) || defined(EFFECT_BILLBOARD)
                    OUT.Normal = float3(0, 0, 1);
                #endif

                // flip normal on backsides
                #ifdef EFFECT_BACKSIDE_NORMALS
                    if (IN.facing < 0.5)
                    {
                        OUT.Normal.z = -OUT.Normal.z;
                    }
                #endif

                // adjust billboard normals to improve GI and matching
                #ifdef EFFECT_BILLBOARD
                    OUT.Normal.z *= 0.5;
                    OUT.Normal = normalize(OUT.Normal);
                #endif

                // extra
                #ifdef EFFECT_EXTRA_TEX
                    fixed4 extra = tex2D(_ExtraTex, IN.uv_MainTex);
                    OUT.Smoothness = extra.r;
                    OUT.Metallic = extra.g;
                    OUT.Occlusion = extra.b * IN.color.r;
                #else
                    OUT.Smoothness = _Glossiness;
                    OUT.Metallic = _Metallic;
                    OUT.Occlusion = IN.color.r;
                #endif

                // subsurface (hijack emissive)
                #ifdef EFFECT_SUBSURFACE
                    OUT.Emission = tex2D(_SubsurfaceTex, IN.uv_MainTex) * _SubsurfaceColor;
                #endif
            }

            void SpeedTreeSurf3(Input IN, inout SurfaceOutputStandard OUT)
            {
                fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;

                // transparency
                OUT.Alpha = color.a * IN.color.a;
                clip(OUT.Alpha - 0.3333);

                // color
                OUT.Albedo = color.rgb;

                // hue variation
                #ifdef EFFECT_HUE_VARIATION
                    half3 shiftedColor = lerp(OUT.Albedo, _HueVariationColor.rgb, IN.color.g);

                    // preserve vibrance
                    half maxBase = max(OUT.Albedo.r, max(OUT.Albedo.g, OUT.Albedo.b));
                    half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
                    maxBase /= newMaxBase;
                    maxBase = maxBase * 0.5f + 0.5f;
                    shiftedColor.rgb *= maxBase;

                    OUT.Albedo = saturate(shiftedColor);
                #endif

                // normal
                #ifdef EFFECT_BUMP
                    OUT.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
                #elif defined(EFFECT_BACKSIDE_NORMALS) || defined(EFFECT_BILLBOARD)
                    OUT.Normal = float3(0, 0, 1);
                #endif

                // flip normal on backsides
                #ifdef EFFECT_BACKSIDE_NORMALS
                    if (IN.facing < 0.5)
                    {
                        OUT.Normal.z = -OUT.Normal.z;
                    }
                #endif

                // adjust billboard normals to improve GI and matching
                #ifdef EFFECT_BILLBOARD
                    OUT.Normal.z *= 0.5;
                    OUT.Normal = normalize(OUT.Normal);
                #endif

                // extra
                #ifdef EFFECT_EXTRA_TEX
                    fixed4 extra = tex2D(_ExtraTex, IN.uv_MainTex);
                    OUT.Smoothness = extra.r;
                    OUT.Metallic = extra.g;
                    OUT.Occlusion = extra.b * IN.color.r;
                #else
                    OUT.Smoothness = _Glossiness;
                    OUT.Metallic = _Metallic;
                    OUT.Occlusion = IN.color.r;
                #endif

                // subsurface (hijack emissive)
                #ifdef EFFECT_SUBSURFACE
                    OUT.Emission = tex2D(_SubsurfaceTex, IN.uv_MainTex) * _SubsurfaceColor;
                #endif
            }

        ENDCG
    }

    // targeting SM2.0: Many effects are disabled for fewer instructions
    SubShader
    {
        Tags
        {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "DisableBatching"="LODFading"
        }
        LOD 400
        Cull [_TwoSided]

        CGPROGRAM
            #pragma surface SpeedTreeSurf Standard vertex:SpeedTreeVert addshadow noinstancing
            #pragma multi_compile_vertex LOD_FADE_PERCENTAGE
            #pragma shader_feature EFFECT_BILLBOARD
            #pragma shader_feature EFFECT_EXTRA_TEX

            #include "SpeedTree8Common.cginc"

        ENDCG
    }

    FallBack "Transparent/Cutout/VertexLit"
    //CustomEditor "SpeedTree8ShaderGUI"
}