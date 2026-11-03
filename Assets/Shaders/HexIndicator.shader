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
        [PerRendererData] _Opacity          ("Opacity", Float)                  = 1
        [PerRendererData] _GhostAlpha       ("Ghost Alpha (hidden edges)", Float) = 0
        [PerRendererData] _RegrowAlpha      ("Regrow Alpha", Float)             = 0.6
        [PerRendererData] _ChromaOffset     ("Chroma Offset (px xy)", Vector)   = (0,0,0,0)
        [PerRendererData] _ChromaColorA     ("Chroma Color A", Color)           = (1,0.24,0.6,0.6)
        [PerRendererData] _ChromaColorB     ("Chroma Color B", Color)           = (0.24,0.72,1,0.6)
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
            float  _EdgeRegrow[12]; // per-edge regrow progress 0..1 for hidden edges; < 0 = none
            float  _Opacity;
            float  _GhostAlpha;
            float  _RegrowAlpha;
            float4 _ChromaOffset;   // xy = screen-space pixel offset of the colour split; 0 = off
            float4 _ChromaColorA;
            float4 _ChromaColorB;

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

            int SectorOf(float2 p, float sides)
            {
                float norm = frac(atan2(p.y, p.x) / TWO_PI);
                return clamp((int)floor(norm * sides), 0, (int)sides - 1);
            }

            float Band(float d)
            {
                return (_InwardOutline > 0.5)
                    ? step(d, 0.0) * step(-_LineWidthPixels, d)
                    : step(abs(d), _LineWidthPixels * 0.5);
            }

            // Outline band of the visible edges only, at a local pixel position (used for the chroma split).
            float VisibleBand(float2 p, float r, float sides)
            {
                return Band(RegularPolygonSdf(p, r, sides)) * SampleEdgeMask(SectorOf(p, sides));
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
                int   sector = SectorOf(rp, sides);
                float edgeOn = SampleEdgeMask(sector);

                // --- Position along the owning edge, in pixels (dash + regrow both use it). ---
                float sideStep  = TWO_PI / sides;
                float sideLenPx = 2.0 * circumRadiusPx * sin(PI / sides);
                float a0 = sector * sideStep;
                float2 v0 = float2(cos(a0), sin(a0)) * circumRadiusPx;
                float2 v1 = float2(cos(a0 + sideStep), sin(a0 + sideStep)) * circumRadiusPx;
                float2 dir = normalize(v1 - v0);
                float t = clamp(dot(rp - v0, dir), 0.0, sideLenPx);

                // --- Dash gate: arc length along the perimeter, modulo (dash + gap), in pixels. ---
                float dashGate = 1.0;
                if (_DashEnabled > 0.5)
                {
                    float s = sector * sideLenPx + t + _DashOffsetPixels;
                    float period = max(_DashLengthPixels + _GapLengthPixels, 0.0001);
                    dashGate = step(fmod(s, period), _DashLengthPixels);
                }

                // --- Outline band + fill, hard step() only (no fwidth / smoothstep -> no AA). ---
                float band = Band(d);
                float outline = band * edgeOn * dashGate;
                float fill = (_FillEnabled > 0.5) ? step(d, 0.0) : 0.0;

                // Hidden edges: a faint dotted placeholder, plus a solid run that grows from vertex i
                // as the edge regrows (progress 0..1 in _EdgeRegrow).
                float hidden = band * (1.0 - edgeOn);
                float regrow = _EdgeRegrow[clamp(sector, 0, 11)];
                float grown = hidden * step(0.0, regrow) * step(t, floor(regrow * sideLenPx));
                float ghost = hidden * (1.0 - grown) * step(fmod(floor(t), 2.0), 0.5) * step(0.001, _GhostAlpha);

                // Chromatic split: the visible outline sampled at +/- an offset given in screen pixels.
                float chromaA = 0.0;
                float chromaB = 0.0;
                if (dot(_ChromaOffset.xy, _ChromaOffset.xy) > 0.0)
                {
                    float2 off = Rotate(_ChromaOffset.xy, -_RotationRad);
                    chromaA = VisibleBand(rp - off, circumRadiusPx, sides) * (1.0 - outline);
                    chromaB = VisibleBand(rp + off, circumRadiusPx, sides) * (1.0 - outline) * (1.0 - chromaA);
                    float keep = (1.0 - grown) * (1.0 - ghost);
                    chromaA *= keep;
                    chromaB *= keep;
                }

                float4 outlineCol = (_UseEdgeColors > 0.5) ? _EdgeColors[sector] : _OutlineColor;

                float4 col = float4(0, 0, 0, 0);
                col += float4(_FillColor.rgb, _FillColor.a) * fill * (1.0 - outline) * (1.0 - grown) * (1.0 - ghost) * (1.0 - chromaA) * (1.0 - chromaB);
                col += float4(outlineCol.rgb, outlineCol.a) * outline;
                col += float4(outlineCol.rgb, outlineCol.a * _RegrowAlpha) * grown;
                col += float4(outlineCol.rgb, outlineCol.a * _GhostAlpha) * ghost;
                col += _ChromaColorA * chromaA;
                col += _ChromaColorB * chromaB;
                col.a *= _Opacity;

                clip(col.a - 0.001);
                return col;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
