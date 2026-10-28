Shader "HackedDesign/AdBlockGlitch"
{
    // Mouse's optical ad blocker. Covers every opaque pixel of the sprite with a grid of
    // palette-coloured squares that slowly drift sideways row by row, change colour on their
    // own clocks, and occasionally tear. The sprite is only used as a mask (and, optionally,
    // peeks through some blocks), so any billboard sprite works without bespoke ad art.
    // Everything is computed in whole texture pixels so it stays on the pixel-art grid.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _ColorA ("Color A", Color) = (0.9019608,0.34901962,0.5019608,1)
        _ColorB ("Color B", Color) = (0.5490196,0.18039216,0.29803923,1)
        _ColorC ("Color C", Color) = (0.08627451,0.03137255,0.05882353,1)
        _ColorD ("Color D", Color) = (1,0.8235294,0.8784314,1)
        // How often each colour is picked, relative to the others (A,B,C,D).
        _ColorWeights ("Color Weights (A,B,C,D)", Vector) = (4,3,2,1)
        _GapColor ("Gap Color", Color) = (0,0,0,1)

        _BlockSize ("Block Size (pixels)", Float) = 8
        _BlockGap ("Block Gap (pixels)", Float) = 1
        _SubdivideChance ("Chance A Block Splits Into 4", Range(0,1)) = 0.25

        _FrameRate ("Animation Frames Per Second", Float) = 8
        _MinHold ("Min Seconds A Block Holds Its Colour", Float) = 1.5
        _MaxHold ("Max Seconds A Block Holds Its Colour", Float) = 6

        _Speed ("Row Drift Speed", Float) = 0.15
        _Distortion ("Row Drift Distance (pixels)", Float) = 3
        _TearChance ("Chance A Row Tears Each Frame", Range(0,0.2)) = 0.01
        _TearAmount ("Tear Distance (pixels)", Float) = 12

        _RevealChance ("Chance A Block Shows The Sprite Underneath", Range(0,1)) = 0

        // Per-instance random offset, set once at spawn by AdBlockGlitch.cs so billboards
        // sharing this material don't glitch in sync.
        _Seed ("Per-Instance Random Seed", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float4 _Color;
                float4 _ColorA;
                float4 _ColorB;
                float4 _ColorC;
                float4 _ColorD;
                float4 _ColorWeights;
                float4 _GapColor;
                float _BlockSize;
                float _BlockGap;
                float _SubdivideChance;
                float _FrameRate;
                float _MinHold;
                float _MaxHold;
                float _Speed;
                float _Distortion;
                float _TearChance;
                float _TearAmount;
                float _RevealChance;
                float4 _Seed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vp.positionCS;
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Smooth 1D value noise in [-1,1] - one independent curve per row band.
            float driftNoise(float band, float t)
            {
                float i = floor(t);
                float f = frac(t);
                float a = hash21(float2(band, i));
                float b = hash21(float2(band, i + 1.0));
                return lerp(a, b, f * f * (3.0 - 2.0 * f)) * 2.0 - 1.0;
            }

            float4 pickColor(float roll)
            {
                float4 w = max(_ColorWeights, 0.0);
                roll *= max(w.x + w.y + w.z + w.w, 0.0001);
                if (roll < w.x)
                {
                    return _ColorA;
                }
                roll -= w.x;
                if (roll < w.y)
                {
                    return _ColorB;
                }
                roll -= w.y;
                if (roll < w.z)
                {
                    return _ColorC;
                }
                return _ColorD;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                clip(sprite.a - 0.01);

                // Work in whole texture pixels, and step time at a low frame rate, so every
                // movement is a hard 1px pixel-art step rather than a smooth slide.
                float2 pixel = floor(IN.uv * _MainTex_TexelSize.zw);
                float frame = floor(_Time.y * _FrameRate);
                float time = frame / max(_FrameRate, 0.001);
                float blockSize = max(floor(_BlockSize), 1.0);

                // Each row of blocks drifts left/right on its own slow curve, and very
                // occasionally jumps much further for a single frame (a "tear").
                float band = floor(pixel.y / blockSize);
                float shift = round(driftNoise(band + _Seed.x, time * _Speed + _Seed.y) * _Distortion);
                float2 tearKey = float2(band, frame) + _Seed.xy;
                if (hash21(tearKey + 7.7) < _TearChance)
                {
                    shift += round((hash21(tearKey + 31.1) * 2.0 - 1.0) * _TearAmount);
                }
                pixel.x += shift;

                float2 cell = floor(pixel / blockSize);
                float2 local = pixel - cell * blockSize;
                float2 cellId = cell + _Seed.xy;

                // Some blocks split into a 2x2 of smaller squares, so the pattern isn't a
                // perfectly regular grid.
                float halfSize = floor(blockSize * 0.5);
                if (halfSize >= 1.0 && hash21(cellId + 17.3) < _SubdivideChance)
                {
                    float2 quadrant = min(floor(local / halfSize), 1.0);
                    cellId = cellId * 2.0 + quadrant + 500.0;
                    local -= quadrant * halfSize;
                }

                float alpha = sprite.a * IN.color.a;
                if (any(local < floor(_BlockGap)))
                {
                    return half4(_GapColor.rgb * IN.color.rgb, alpha * _GapColor.a);
                }

                // Every block keeps its colour for its own random hold time, then re-rolls.
                float hold = max(lerp(_MinHold, _MaxHold, hash21(cellId + 3.1)), 0.01);
                float segment = floor(time / hold + hash21(cellId + 91.7) * 100.0);
                float4 color = pickColor(hash21(cellId + segment * 13.37));

                if (hash21(cellId + segment * 7.13 + 55.5) < _RevealChance)
                {
                    half4 peek = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (pixel + 0.5) * _MainTex_TexelSize.xy);
                    color.rgb = peek.a > 0.01 ? peek.rgb : color.rgb;
                }

                return half4(color.rgb * IN.color.rgb, alpha * color.a);
            }
            ENDHLSL
        }
    }
}
