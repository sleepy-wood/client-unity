Shader "Hidden/Broccoli/SproutLabAlbedo"
{
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _TintColor ("Tint Color", Color) = (0.5,1,1,1)
        _BranchShade ("Branch Shade", Float) = 1
        _BranchSat ("Branch Saturation", Float) = 1
        _SproutSat ("SproutSaturation", Float) = 1
        _ApplyExtraSat ("Apply Saturation", Float) = 1
        _IsLinearColorSpace ("Is Linear Color Space", Float) = 0
    }
    SubShader {
        Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 100

        Cull Off
        Lighting Off
        //Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            Name "Albedo"
            HLSLPROGRAM
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
                float4 _TintColor;
                float _BranchShade;
                float _BranchSat;
                float _SproutSat;
                float _ApplyExtraSat;
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




                /*
                ** Photoshop & misc math
                ** Blending modes, RGB/HSL/Contrast/Desaturate
                **
                ** Romain Dura | Romz
                ** Blog: http://blog.mouaif.org
                ** Post: http://blog.mouaif.org/?p=94
                */


                /*
                ** Desaturation
                */

                float4 Desaturate(float3 color, float Desaturation)
                {
                    float3 grayXfer = float3(0.3, 0.59, 0.11);
                    float grayf = dot(grayXfer, color);
                    float3 gray = float3(grayf, grayf, grayf);
                    return float4(lerp(color, gray, Desaturation), 1.0);
                }


                /*
                ** Hue, saturation, luminance
                */

                float3 RGBToHSL(float3 color)
                {
                    float3 hsl; // init to 0 to avoid warnings ? (and reverse if + remove first part)
                    
                    float fmin = min(min(color.r, color.g), color.b);    //Min. value of RGB
                    float fmax = max(max(color.r, color.g), color.b);    //Max. value of RGB
                    float delta = fmax - fmin;             //Delta RGB value

                    hsl.z = (fmax + fmin) / 2.0; // Luminance

                    if (delta == 0.0)		//This is a gray, no chroma...
                    {
                        hsl.x = 0.0;	// Hue
                        hsl.y = 0.0;	// Saturation
                    }
                    else                                    //Chromatic data...
                    {
                        if (hsl.z < 0.5)
                            hsl.y = delta / (fmax + fmin); // Saturation
                        else
                            hsl.y = delta / (2.0 - fmax - fmin); // Saturation
                        
                        float deltaR = (((fmax - color.r) / 6.0) + (delta / 2.0)) / delta;
                        float deltaG = (((fmax - color.g) / 6.0) + (delta / 2.0)) / delta;
                        float deltaB = (((fmax - color.b) / 6.0) + (delta / 2.0)) / delta;

                        if (color.r == fmax )
                            hsl.x = deltaB - deltaG; // Hue
                        else if (color.g == fmax)
                            hsl.x = (1.0 / 3.0) + deltaR - deltaB; // Hue
                        else if (color.b == fmax)
                            hsl.x = (2.0 / 3.0) + deltaG - deltaR; // Hue

                        if (hsl.x < 0.0)
                            hsl.x += 1.0; // Hue
                        else if (hsl.x > 1.0)
                            hsl.x -= 1.0; // Hue
                    }

                    return hsl;
                }

                float HueToRGB(float f1, float f2, float hue)
                {
                    if (hue < 0.0)
                        hue += 1.0;
                    else if (hue > 1.0)
                        hue -= 1.0;
                    float res;
                    if ((6.0 * hue) < 1.0)
                        res = f1 + (f2 - f1) * 6.0 * hue;
                    else if ((2.0 * hue) < 1.0)
                        res = f2;
                    else if ((3.0 * hue) < 2.0)
                        res = f1 + (f2 - f1) * ((2.0 / 3.0) - hue) * 6.0;
                    else
                        res = f1;
                    return res;
                }

                float3 HSLToRGB(float3 hsl)
                {
                    float3 rgb;
                    
                    if (hsl.y == 0.0)
                        rgb = float3(hsl.z, hsl.z, hsl.z); // Luminance
                    else
                    {
                        float f2;
                        
                        if (hsl.z < 0.5)
                            f2 = hsl.z * (1.0 + hsl.y);
                        else
                            f2 = (hsl.z + hsl.y) - (hsl.y * hsl.z);
                            
                        float f1 = 2.0 * hsl.z - f2;
                        
                        rgb.r = HueToRGB(f1, f2, hsl.x + (1.0/3.0));
                        rgb.g = HueToRGB(f1, f2, hsl.x);
                        rgb.b= HueToRGB(f1, f2, hsl.x - (1.0/3.0));
                    }
                    
                    return rgb;
                }

                /*
                ** Contrast, saturation, brightness
                ** Code of this function is from TGM's shader pack
                ** http://irrlicht.sourceforge.net/phpBB2/viewtopic.php?t=21057
                */

                // For all settings: 1.0 = 100% 0.5=50% 1.5 = 150%
                float3 ContrastSaturationBrightness(float3 color, float brt, float sat, float con)
                {
                    // Increase or decrease theese values to adjust r, g and b color channels seperately
                    const float AvgLumR = 0.5;
                    const float AvgLumG = 0.5;
                    const float AvgLumB = 0.5;
                    
                    const float3 LumCoeff = float3(0.2125, 0.7154, 0.0721);
                    
                    float3 AvgLumin = float3(AvgLumR, AvgLumG, AvgLumB);
                    float3 brtColor = color * brt;
                    float intensityf = dot(brtColor, LumCoeff);
                    float3 intensity = float3(intensityf, intensityf, intensityf);
                    float3 satColor = lerp(intensity, brtColor, sat);
                    float3 conColor = lerp(AvgLumin, satColor, con);
                    return conColor;
                }

                /*
                ** Float blending modes
                ** Adapted from here: http://www.nathanm.com/photoshop-blending-math/
                ** But I modified the HardMix (wrong condition), Overlay, SoftLight, ColorDodge, ColorBurn, VividLight, PinLight (inverted layers) ones to have correct results
                */

                #define BlendLinearDodgef 			BlendAddf
                #define BlendLinearBurnf 			BlendSubstractf
                #define BlendAddf(base, blend) 		min(base + blend, 1.0)
                #define BlendSubstractf(base, blend) 	max(base + blend - 1.0, 0.0)
                #define BlendLightenf(base, blend) 		max(blend, base)
                #define BlendDarkenf(base, blend) 		min(blend, base)
                #define BlendLinearLightf(base, blend) 	(blend < 0.5 ? BlendLinearBurnf(base, (2.0 * blend)) : BlendLinearDodgef(base, (2.0 * (blend - 0.5))))
                #define BlendScreenf(base, blend) 		(1.0 - ((1.0 - base) * (1.0 - blend)))
                #define BlendOverlayf(base, blend) 	(base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend)))
                #define BlendSoftLightf(base, blend) 	((blend < 0.5) ? (2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend)))
                #define BlendColorDodgef(base, blend) 	((blend == 1.0) ? blend : min(base / (1.0 - blend), 1.0))
                #define BlendColorBurnf(base, blend) 	((blend == 0.0) ? blend : max((1.0 - ((1.0 - base) / blend)), 0.0))
                #define BlendVividLightf(base, blend) 	((blend < 0.5) ? BlendColorBurnf(base, (2.0 * blend)) : BlendColorDodgef(base, (2.0 * (blend - 0.5))))
                #define BlendPinLightf(base, blend) 	((blend < 0.5) ? BlendDarkenf(base, (2.0 * blend)) : BlendLightenf(base, (2.0 *(blend - 0.5))))
                #define BlendHardMixf(base, blend) 	((BlendVividLightf(base, blend) < 0.5) ? 0.0 : 1.0)
                #define BlendReflectf(base, blend) 		((blend == 1.0) ? blend : min(base * base / (1.0 - blend), 1.0))



                /*
                ** Vector3 blending modes
                */

                // Component wise blending
                #define Blend(base, blend, funcf) 		float3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))

                #define BlendNormal(base, blend) 		(base)
                #define BlendLighten				BlendLightenf
                #define BlendDarken				BlendDarkenf
                #define BlendMultiply(base, blend) 		(base * blend)
                #define BlendAverage(base, blend) 		((base + blend) / 2.0)
                #define BlendAdd(base, blend) 		min(base + blend, float3(1.0, 1.0, 1.0))
                #define BlendSubstract(base, blend) 	max(base + blend - float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0))
                #define BlendDifference(base, blend) 	abs(base - blend)
                #define BlendNegation(base, blend) 	(float3(1.0, 1.0, 1.0) - abs(float3(1.0, 1.0, 1.0) - base - blend))
                #define BlendExclusion(base, blend) 	(base + blend - 2.0 * base * blend)
                #define BlendScreen(base, blend) 		Blend(base, blend, BlendScreenf)
                #define BlendOverlay(base, blend) 		Blend(base, blend, BlendOverlayf)
                #define BlendSoftLight(base, blend) 	Blend(base, blend, BlendSoftLightf)
                #define BlendHardLight(base, blend) 	BlendOverlay(blend, base)
                #define BlendColorDodge(base, blend) 	Blend(base, blend, BlendColorDodgef)
                #define BlendColorBurn(base, blend) 	Blend(base, blend, BlendColorBurnf)
                #define BlendLinearDodge			BlendAdd
                #define BlendLinearBurn			BlendSubstract

                // Linear Light is another contrast-increasing mode
                // If the blend color is darker than midgray, Linear Light darkens the image by decreasing the brightness. If the blend color is lighter than midgray, the result is a brighter image due to increased brightness.
                #define BlendLinearLight(base, blend) 	Blend(base, blend, BlendLinearLightf)

                #define BlendVividLight(base, blend) 	Blend(base, blend, BlendVividLightf)
                #define BlendPinLight(base, blend) 		Blend(base, blend, BlendPinLightf)
                #define BlendHardMix(base, blend) 		Blend(base, blend, BlendHardMixf)
                #define BlendReflect(base, blend) 		Blend(base, blend, BlendReflectf)
                #define BlendGlow(base, blend) 		BlendReflect(blend, base)
                #define BlendPhoenix(base, blend) 		(min(base, blend) - max(base, blend) + float3(1.0, 1.0, 1.0))
                #define BlendOpacity(base, blend, F, O) 	(F(base, blend) * O + blend * (1.0 - O))

                // Hue Blend mode creates the result color by combining the luminance and saturation of the base color with the hue of the blend color.
                float3 BlendHue(float3 base, float3 blend)
                {
                    float3 baseHSL = RGBToHSL(base);
                    return HSLToRGB(float3(RGBToHSL(blend).r, baseHSL.g, baseHSL.b));
                }

                // Saturation Blend mode creates the result color by combining the luminance and hue of the base color with the saturation of the blend color.
                float3 BlendSaturation(float3 base, float3 blend)
                {
                    float3 baseHSL = RGBToHSL(base);
                    return HSLToRGB(float3(baseHSL.r, RGBToHSL(blend).g, baseHSL.b));
                }

                // Color Mode keeps the brightness of the base color and applies both the hue and saturation of the blend color.
                float3 BlendColor(float3 base, float3 blend)
                {
                    float3 blendHSL = RGBToHSL(blend);
                    return HSLToRGB(float3(blendHSL.r, blendHSL.g, RGBToHSL(base).b));
                }

                // Luminosity Blend mode creates the result color by combining the hue and saturation of the base color with the luminance of the blend color.
                float3 BlendLuminosity(float3 base, float3 blend)
                {
                    float3 baseHSL = RGBToHSL(base);
                    return HSLToRGB(float3(baseHSL.r, baseHSL.g, RGBToHSL(blend).b));
                }
                
                fixed4 frag (v2f i) : Color
                {
                    fixed4 col = tex2D(_MainTex, i.texcoord);
                    if (_IsLinearColorSpace) {
                        col.rgb = pow (col.rgb, 0.4545);
                        _TintColor.rgb = pow (_TintColor.rgb, 0.4545);
                    }
                    fixed4 vcol = i.color;
                    clip(col.a * vcol.a - _Cutoff);
                    //UNITY_APPLY_FOG(i.fogCoord, col);
                    col.rgb *= 1 - ((1 - i.color.r) / 2);

                    if (i.color.g == 0) { // Green channel is 0 for sprouts.
                        float3 shiftedColor = BlendColor (col, lerp(col, _TintColor.rgb, i.color.b));

                        // preserve vibrance
                        half maxBase = max(col.r, max(col.g, col.b));
                        half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
                        maxBase /= newMaxBase;
                        maxBase = maxBase * 0.5f + 0.5f;
                        shiftedColor.rgb *= maxBase;

                        if (_ApplyExtraSat == 0) {
                            col.rgb = ContrastSaturationBrightness (saturate (shiftedColor), 1.0, _SproutSat, 1);
                        } else {
                            col.rgb = ContrastSaturationBrightness (saturate (shiftedColor), 1.0, _SproutSat * 1.2, 1.1);
                        }
                    } else { // Green channel is >0 for branches.
                        col.rgb = ContrastSaturationBrightness (col.rgb, 1, _BranchSat, 1);
                    }
                    if (_IsLinearColorSpace)
                        col.rgb = pow (col.rgb, 2.2);
                    return  col;
                }
                
            ENDHLSL
        }
    }
}