Shader "HackedDesign/HexIndicator"
{
    Properties
    {
        // Every property is driven per-renderer by the HexIndicator component via a
        // MaterialPropertyBlock, so they are hidden from the material inspector. The values below
        // are only fallback defaults for using this shader without that component.
        [PerRendererData] _OutlineColor     ("Outline Color", Color)            = (0,1,1,1)
        [PerRendererData] _UseEdgeColors    ("Use Per-Edge Colors", Float)      = 0
        [PerRendererData] _FillColor        ("Fill Color", Color)               = (0,1,1,0.15)
        [PerRendererData] _Radius           ("Radius (world units, centre->vertex)", Float) = 3
        [PerRendererData] _Sides            ("Sides (3..12)", Float)            = 6
        [PerRendererData] _RotationRad      ("Rotation (radians)", Float)       = 0
        [PerRendererData] _Center           ("Centre (world xy)", Vector)       = (0,0,0,0)
        [PerRendererData] _LineWidthPixels  ("Line Width (px)", Float)          = 1
        [PerRendererData] _FillEnabled      ("Fill Enabled", Float)             = 0
        [PerRendererData] _InwardOutline    ("Inward Outline", Float)           = 1
        [PerRendererData] _DashEnabled      ("Dash Enabled", Float)             = 0
        [PerRendererData] _DashLengthPixels ("Dash Length (px)", Float)         = 3
        [PerRendererData] _GapLengthPixels  ("Gap Length (px)", Float)          = 2
        [PerRendererData] _DashOffsetPixels ("Dash Offset (px)", Float)         = 0
        [PerRendererData] _PPU              ("Pixels Per Unit", Float)          = 16
        [PerRendererData] _CamPixelOffset   ("Camera Subpixel Offset (world xy)", Vector) = (0,0,0,0)
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
            Name "HexIndicator2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define PI     3.14159265359
            #define TWO_PI 6.28318530718

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 positionWS : TEXCOORD0;
            };

            float4 _OutlineColor;
            float  _UseEdgeColors;
            float4 _EdgeColors[12]; // per-edge outline colour, set by HexIndicator; index = sector
            float  _EdgeMask[12];   // per-edge visibility (1 / 0), set by HexIndicator; index = sector
            float4 _FillColor;
            float  _Radius;
            float  _Sides;
            float  _RotationRad;
            float4 _Center;
            float  _LineWidthPixels;
            float  _FillEnabled;
            float  _InwardOutline;
            float  _DashEnabled;
            float  _DashLengthPixels;
            float  _GapLengthPixels;
            float  _DashOffsetPixels;
            float  _PPU;
            float4 _CamPixelOffset;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.positionCS = TransformWorldToHClip(worldPos);
                // Affine interpolation over a quad is exact, so every fragment gets its true world xy.
                OUT.positionWS = worldPos.xy;
                return OUT;
            }

            float2 Rotate(float2 v, float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2(c * v.x - s * v.y, s * v.x + c * v.y);
            }

            // Exact SDF of a regular convex polygon, in pixels. Negative inside.
            //   r = circumradius (centre -> vertex);  n = side count.
            // Vertices sit at angle k*(2*PI/n) with vertex 0 on +X; edge k spans vertex k -> k+1.
            // n = 6 reproduces the previous flat-top hexagon exactly.
            float RegularPolygonSdf(float2 p, float r, float n)
            {
                float an = TWO_PI / max(n, 3.0);
                float apothem = r * cos(an * 0.5);   // centre -> edge
                float halfEdge = r * sin(an * 0.5);  // half the side length

                // Fold p into the wedge centred on one edge's outward normal, so that edge is the
                // vertical segment x = apothem, y in [-halfEdge, halfEdge].
                float theta = atan2(p.y, p.x);
                float k = round((theta - an * 0.5) / an);
                float rot = k * an + an * 0.5;
                float c = cos(rot);
                float s = sin(rot);
                float2 q = float2(c * p.x + s * p.y, -s * p.x + c * p.y);

                return length(q - float2(apothem, clamp(q.y, -halfEdge, halfEdge))) * sign(q.x - apothem);
            }

            float SampleEdgeMask(int idx)
            {
                return _EdgeMask[clamp(idx, 0, 11)];
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // --- Quantise the fragment to the world pixel grid (1 / _PPU units). ---
                // Every screen pixel inside one grid cell resolves to the same sample point, so the
                // result is one value per game pixel: a hard stairstep, independent of screen resolution.
                float invPpu = 1.0 / _PPU;
                float2 pixelCoord   = floor((IN.positionWS - _CamPixelOffset.xy) * _PPU);
                float2 snappedWorld = (pixelCoord + 0.5) * invPpu + _CamPixelOffset.xy;

                // --- Into the polygon's local pixel space: centre, rotate, scale to pixels. ---
                float2 rp = Rotate(snappedWorld - _Center.xy, -_RotationRad) * _PPU;
                float  sides          = clamp(_Sides, 3.0, 12.0);
                float  circumRadiusPx = max(_Radius * _PPU, 0.0);

                float d = RegularPolygonSdf(rp, circumRadiusPx, sides);

                // --- Which edge owns this fragment (clean angular sector cut). ---
                // Vertex i sits at angle i*(360/n) (vertex 0 at +X, CCW); sector i spans that wedge.
                float ang = atan2(rp.y, rp.x);
                float norm = frac(ang / TWO_PI);
                int   sector = (int)floor(norm * sides);
                sector = clamp(sector, 0, (int)sides - 1);
                float edgeOn = SampleEdgeMask(sector);

                // --- Dash gate: arc length along the perimeter, modulo (dash + gap), in pixels. ---
                float dashGate = 1.0;
                if (_DashEnabled > 0.5)
                {
                    float sideStep  = TWO_PI / sides;
                    float sideLenPx = 2.0 * circumRadiusPx * sin(PI / sides);
                    float a0 = sector * sideStep;
                    float2 v0 = float2(cos(a0), sin(a0)) * circumRadiusPx;
                    float2 v1 = float2(cos(a0 + sideStep), sin(a0 + sideStep)) * circumRadiusPx;
                    float2 dir = normalize(v1 - v0);
                    float t = clamp(dot(rp - v0, dir), 0.0, sideLenPx);
                    float s = sector * sideLenPx + t + _DashOffsetPixels;
                    float period = max(_DashLengthPixels + _GapLengthPixels, 0.0001);
                    dashGate = step(fmod(s, period), _DashLengthPixels);
                }

                // --- Outline band + fill, hard step() only (no fwidth / smoothstep -> no AA). ---
                float band = (_InwardOutline > 0.5)
                    ? step(d, 0.0) * step(-_LineWidthPixels, d)
                    : step(abs(d), _LineWidthPixels * 0.5);
                float outline = band * edgeOn * dashGate;
                float fill = (_FillEnabled > 0.5) ? step(d, 0.0) : 0.0;

                float4 outlineCol = (_UseEdgeColors > 0.5) ? _EdgeColors[sector] : _OutlineColor;

                float4 col;
                col.rgb = _FillColor.rgb * fill * (1.0 - outline) + outlineCol.rgb * outline;
                col.a   = _FillColor.a   * fill * (1.0 - outline) + outlineCol.a   * outline;

                clip(col.a - 0.001);
                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
