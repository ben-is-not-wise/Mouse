Shader "HackedDesign/PingRing"
{
    Properties
    {
        // Colours, thickness and split are set on the material. Centre, radius and alpha are
        // driven per-renderer by the PingWave component via a MaterialPropertyBlock.
        _CoreColor          ("Core Color", Color)                    = (1,1,1,1)
        _SplitColorA        ("Split Color A (left)", Color)          = (1,0.25,0.6,1)
        _SplitColorB        ("Split Color B (right)", Color)         = (0.2,0.8,1,1)
        _ThicknessPixels    ("Ring Thickness (px)", Float)           = 1
        _SplitPixels        ("Chromatic Split (px)", Float)          = 1
        _TrailPixels        ("Trail Length (px)", Float)             = 6
        _TrailAlpha         ("Trail Alpha", Range(0,1))              = 0.35
        _TrailSteps         ("Trail Steps", Float)                   = 3
        [PerRendererData] _Radius ("Radius (world units)", Float)    = 3
        [PerRendererData] _Alpha  ("Alpha", Float)                   = 1
        [PerRendererData] _Center ("Centre (world xy)", Vector)      = (0,0,0,0)
        [PerRendererData] _PPU    ("Pixels Per Unit", Float)         = 16
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"    = "Plane"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "PingRing2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 positionWS : TEXCOORD0;
            };

            float4 _CoreColor;
            float4 _SplitColorA;
            float4 _SplitColorB;
            float  _ThicknessPixels;
            float  _SplitPixels;
            float  _TrailPixels;
            float  _TrailAlpha;
            float  _TrailSteps;
            float  _Radius;
            float  _Alpha;
            float4 _Center;
            float  _PPU;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.positionWS = worldPos.xy;
                return OUT;
            }

            // Signed distance in pixels from the ring's centre line. Negative inside.
            float RingDistance(float2 p, float2 centre, float radiusPx)
            {
                return length(p - centre) - radiusPx;
            }

            float Band(float d)
            {
                return step(abs(d), _ThicknessPixels * 0.5);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Quantise to the world pixel grid, same as HexIndicator: one value per game pixel.
                float invPpu = 1.0 / _PPU;
                float2 pixelCoord = floor(IN.positionWS * _PPU);
                float2 p = pixelCoord + 0.5;
                float2 centre = _Center.xy * _PPU;
                float radiusPx = max(_Radius * _PPU, 0.0);

                float dCore = RingDistance(p, centre, radiusPx);
                float core = Band(dCore);
                float splitA = Band(RingDistance(p, centre - float2(_SplitPixels, 0), radiusPx));
                float splitB = Band(RingDistance(p, centre + float2(_SplitPixels, 0), radiusPx));

                // Stepped trail inside the ring so the fade stays pixel-art, not a smooth gradient.
                float inside = -dCore - _ThicknessPixels * 0.5;
                float trail = 0.0;
                if (_TrailPixels > 0.0 && inside > 0.0 && inside < _TrailPixels)
                {
                    float steps = max(_TrailSteps, 1.0);
                    trail = ceil((1.0 - inside / _TrailPixels) * steps) / steps * _TrailAlpha;
                }

                float4 col = float4(0, 0, 0, 0);
                if (core > 0.5)
                {
                    col = _CoreColor;
                }
                else if (splitA > 0.5)
                {
                    col = _SplitColorA;
                }
                else if (splitB > 0.5)
                {
                    col = _SplitColorB;
                }
                else if (trail > 0.0)
                {
                    col = float4(_CoreColor.rgb, trail);
                }

                col.a *= _Alpha;
                clip(col.a - 0.001);
                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
