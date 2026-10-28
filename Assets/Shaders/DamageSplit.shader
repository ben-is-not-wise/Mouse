Shader "HackedDesign/DamageSplit"
{
    // Full screen damage effect for a Full Screen Pass Renderer Feature. The pink/blue split
    // creeps in from the screen edges as Mouse gets hurt, rows of pixels tear sideways, and
    // near death the edges bleed toward the vignette colour with a heartbeat. Driven by two
    // globals set from DamageEffect.cs: _Damage (0 healthy, 0.5 hurt, 1 critical) and
    // _DamagePulse (a short kick on each hit). Offsets are whole game pixels on the 640x360 grid.
    Properties
    {
        _LeftColor ("Left Fringe (Blue)", Color) = (0.20624998, 0.5, 0.5, 1)
        _RightColor ("Right Fringe (Pink)", Color) = (0.70980394, 0.15686275, 0.3254902, 1)
        _FringeStrength ("Fringe Strength", Range(0, 4)) = 2

        _MaxSplit ("Max Split (game pixels)", Float) = 4
        _PulseSplit ("Extra Split On Hit (game pixels)", Float) = 3
        _InnerEdge ("How Far In At Full Damage (0 centre, 1 edge)", Range(0, 1)) = 0.2

        _TearAmount ("Tear Chance At Full Damage", Range(0, 1)) = 0.12
        _TearHeight ("Tear Band Height (game pixels)", Float) = 3
        _TearShift ("Max Tear Shift (game pixels)", Float) = 6
        _TearRate ("Tear Changes Per Second", Float) = 12

        _VignetteColor ("Critical Vignette Colour", Color) = (0.9960785, 0.08627451, 0.28627452, 1)
        _VignetteStrength ("Critical Vignette Strength", Range(0, 1)) = 0.35
        _HeartRate ("Critical Heartbeats Per Second", Float) = 1.6

        _GameResolution ("Game Resolution", Vector) = (640, 360, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "DamageSplit"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Set globally by DamageEffect.cs.
            float _Damage;
            float _DamagePulse;

            CBUFFER_START(UnityPerMaterial)
                float4 _LeftColor;
                float4 _RightColor;
                float _FringeStrength;
                float _MaxSplit;
                float _PulseSplit;
                float _InnerEdge;
                float _TearAmount;
                float _TearHeight;
                float _TearShift;
                float _TearRate;
                float4 _VignetteColor;
                float _VignetteStrength;
                float _HeartRate;
                float4 _GameResolution;
            CBUFFER_END

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float3 Sample(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv).rgb;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                float damage = saturate(_Damage);
                float pulse = saturate(_DamagePulse);
                float amount = saturate(damage + pulse * 0.5);
                if (amount <= 0.001)
                {
                    return float4(Sample(uv), 1);
                }

                float2 res = max(_GameResolution.xy, 1);
                float2 px = 1.0 / res;
                float2 cell = floor(uv * res);

                // Edge mask from the centre of each game pixel so it stays blocky.
                float2 fromCentre = abs((cell + 0.5) * px - 0.5) * 2.0;
                float edge = max(fromCentre.x, fromCentre.y);
                float inner = lerp(1.0, _InnerEdge, amount);
                float mask = saturate((edge - inner) / max(1.0 - inner, 0.001));

                // Scan tears: whole bands of rows jump sideways, more often as damage rises.
                float tick = floor(_Time.y * _TearRate);
                float band = floor(cell.y / max(_TearHeight, 1));
                float roll = Hash(float2(band, tick));
                float tearChance = _TearAmount * saturate(damage + pulse);
                if (roll < tearChance)
                {
                    float dir = Hash(float2(tick, band)) < 0.5 ? -1.0 : 1.0;
                    float shift = round(1.0 + (roll / max(tearChance, 0.0001)) * (_TearShift - 1.0));
                    uv.x += dir * shift * px.x;
                }

                // Pink/blue split: any edge a few pixels to either side ghosts in as a coloured fringe.
                float split = round(mask * _MaxSplit * amount + pulse * _PulseSplit);
                float2 offset = float2(split * px.x, 0);
                float3 col = Sample(uv);
                if (split >= 1)
                {
                    float centre = Luminance(col);
                    float left = abs(Luminance(Sample(uv - offset)) - centre) * _FringeStrength;
                    float right = abs(Luminance(Sample(uv + offset)) - centre) * _FringeStrength;
                    col = lerp(col, _LeftColor.rgb, saturate(left));
                    col = lerp(col, _RightColor.rgb, saturate(right));
                }

                // Critical: edges darken and bleed toward the vignette colour on a heartbeat.
                float critical = saturate((damage - 0.5) * 2.0);
                float beat = pow(saturate(sin(_Time.y * _HeartRate * 6.2831853)), 8);
                float vignette = mask * mask * critical;
                col *= 1.0 - vignette * 0.35;
                col = lerp(col, _VignetteColor.rgb, vignette * _VignetteStrength * (0.6 + 0.4 * beat));

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}
