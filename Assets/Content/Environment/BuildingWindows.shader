Shader "HackedDesign/BuildingWindows"
{
    // Paint windows on the building sprite as a flat "key" color. This shader finds pixels
    // matching that color and replaces them with an animated, per-window random lit/unlit
    // flicker, leaving the rest of the sprite untouched.
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _BuildingColor ("Building Tint (replaces black)", Color) = (0,0,0,1)
        _BuildingTolerance ("Building Black Tolerance", Range(0,1)) = 0.05

        _KeyColor ("Window Key Color", Color) = (1,0,1,1)
        _KeyTolerance ("Key Tolerance", Range(0,1)) = 0.05

        _WindowColorA ("Window Color (unlit)", Color) = (0.2,0.15,0.05,1)
        _WindowColorB ("Window Color (lit)", Color) = (1,0.85,0.5,1)

        // Per-instance random offset (set once at spawn, e.g. by BuildingWindowSeed.cs) so
        // different building instances sharing this material don't flicker in sync. Must NOT
        // be derived from a live transform - parallax layers move every frame, which would
        // make the flicker recompute on every tiny movement instead of holding steady.
        _Seed ("Per-Instance Random Seed", Vector) = (0,0,0,0)

        _GridOffset ("Grid Offset - where the first window starts (pixels)", Vector) = (0,0,0,0)
        _WindowSize ("Window Size (pixels)", Vector) = (16,16,0,0)
        _WindowGap ("Gap Between Windows (pixels)", Vector) = (8,8,0,0)
        _MinInterval ("Min Seconds Between Switches", Range(0.1,60)) = 2
        _MaxInterval ("Max Seconds Between Switches", Range(0.1,60)) = 8
        _FlickerChance ("Chance A Window Is Lit", Range(0,1)) = 0.6
        _FlickerProbability ("Chance A Window Flickers (else stays fixed)", Range(0,1)) = 0.3
        _QuickFlickChance ("Chance A Window Quick-Flicks (brief 1s blip, else stays fixed)", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
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
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float4 _Color;
            float4 _BuildingColor;
            float _BuildingTolerance;
            float4 _KeyColor;
            float _KeyTolerance;
            float4 _WindowColorA;
            float4 _WindowColorB;
            float2 _Seed;
            float2 _GridOffset;
            float2 _WindowSize;
            float2 _WindowGap;
            float _MinInterval;
            float _MaxInterval;
            float _FlickerChance;
            float _FlickerProbability;
            float _QuickFlickChance;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = vp.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                float dist = distance(tex.rgb, _KeyColor.rgb);
                if (dist < _KeyTolerance && tex.a > 0.01)
                {
                    // Grid the texture into window-sized cells (in texture pixels, not raw UV
                    // fractions - much easier to reason about: "windows are ~16px with an 8px
                    // gap" rather than a 0-1 UV fraction). _GridOffset shifts the grid so it
                    // starts at your first window instead of the texture's top-left corner.
                    float2 cellSize = _WindowSize + _WindowGap;
                    float2 texelCoord = IN.uv * _MainTex_TexelSize.zw;
                    // Sprite V runs bottom-to-top, but pixel offsets are measured top-down (as
                    // in an image editor) - flip Y so _GridOffset.y means "from the top".
                    texelCoord.y = _MainTex_TexelSize.w - texelCoord.y;
                    texelCoord -= _GridOffset;
                    float2 cell = floor(texelCoord / cellSize) + _Seed;

                    // Each window is randomly sorted into one of three behaviors, decided once
                    // per cell: flicker (cycles on/off with random hold times), quick-flick
                    // (normally off, briefly blips on), or static (fixed forever).
                    float category = hash21(cell + 202.5);

                    float lit;
                    if (category < _FlickerProbability)
                    {
                        // Each window gets its own random hold time between _MinInterval and
                        // _MaxInterval, and a per-cell time offset so windows sharing a similar
                        // interval don't all switch on the same beat. Every time that window's
                        // "clock" ticks over, it independently rolls a new random on/off state -
                        // a hard switch, not a smooth cycle.
                        float interval = lerp(_MinInterval, _MaxInterval, hash21(cell + 3.1));
                        float phase = hash21(cell + 91.7) * 1000.0;
                        float segment = floor(_Time.y / interval + phase);

                        lit = step(1.0 - _FlickerChance, hash21(cell + segment * 13.37));
                    }
                    else if (category < _FlickerProbability + _QuickFlickChance)
                    {
                        // Mostly off, but once per random cycle (same interval range as above)
                        // it flicks on for exactly 1 second, then back off - like a faulty bulb.
                        float interval = lerp(_MinInterval, _MaxInterval, hash21(cell + 3.1));
                        float phase = hash21(cell + 91.7) * 1000.0;
                        float secondsIntoCycle = frac(_Time.y / interval + phase) * interval;

                        lit = secondsIntoCycle < 1.0 ? 1.0 : 0.0;
                    }
                    else
                    {
                        lit = step(1.0 - _FlickerChance, hash21(cell + 200.0));
                    }

                    half4 windowColor = lerp(_WindowColorA, _WindowColorB, lit);
                    return half4(windowColor.rgb, tex.a);
                }

                // The building's base color in the art is pure black - key it out the same
                // way as windows so it can be tinted per-material/per-instance.
                if (distance(tex.rgb, float3(0, 0, 0)) < _BuildingTolerance && tex.a > 0.01)
                {
                    return half4(_BuildingColor.rgb, tex.a);
                }

                return tex;
            }
            ENDHLSL
        }
    }
}
