using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// Draws a regular convex polygon outline (3..12 sides, with optional fill and dash pattern) on
    /// the world pixel grid, so it keeps the pixel-art look at any screen resolution. Each edge can
    /// be shown or hidden independently. Radius, rotation, side count and visible edges are safe to
    /// change every frame - all updates are MaterialPropertyBlock writes, no mesh rebuild, no allocs.
    ///
    /// Edge index i runs from vertex i to vertex i+1 (mod sides). Vertex 0 sits at +X, vertices go
    /// CCW 360/sides deg apart. At sides = 6 this is the flat-top hexagon (edge 1 is the top edge);
    /// other counts appear rotated - use rotationDegrees to orient them.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexIndicator: MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;
        private const float QuadPadding = 1.15f;
        private const int MinSides = 3;
        private const int MaxSides = 12;

        [Header("Shape")]
        [SerializeField] private float radius = 3f;
        [SerializeField] private float maxRadius = 8f;
        [SerializeField, Range(MinSides, MaxSides)] private int sides = 6;
        [SerializeField] private float rotationDegrees = 0f;
        [SerializeField] private bool useTransformRotation = true;

        [Header("Edges (vertex i -> vertex i+1, one entry per side)")]
        [SerializeField] private bool[] edges = new[] { true, true, true, true, true, true };

        [Header("Outline")]
        [SerializeField] private Color outlineColor = Color.cyan;
        [SerializeField] private float lineWidthPixels = 1f;
        [SerializeField] private bool inwardOutline = true;

        [Header("Per-edge outline colour (overrides Outline Color when enabled)")]
        [SerializeField] private bool useEdgeColors = false;
        [SerializeField] private Color[] edgeColors = new[] { Color.cyan, Color.cyan, Color.cyan, Color.cyan, Color.cyan, Color.cyan };

        [Header("Fill")]
        [SerializeField] private bool fillEnabled = false;
        [SerializeField] private Color fillColor = new Color(0f, 1f, 1f, 0.15f);

        [Header("Dash")]
        [SerializeField] private bool dashEnabled = false;
        [SerializeField] private float dashLengthPixels = 3f;
        [SerializeField] private float gapLengthPixels = 2f;
        [SerializeField] private float dashScrollPixelsPerSecond = 0f;

        [Header("Render")]
        [SerializeField] private string sortingLayer = "Default";
        [SerializeField] private int sortingOrder = 0;
        [SerializeField] private Material material = null;

        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int UseEdgeColorsId = Shader.PropertyToID("_UseEdgeColors");
        private static readonly int EdgeColorsId = Shader.PropertyToID("_EdgeColors");
        private static readonly int FillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int RotationRadId = Shader.PropertyToID("_RotationRad");
        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int LineWidthPixelsId = Shader.PropertyToID("_LineWidthPixels");
        private static readonly int FillEnabledId = Shader.PropertyToID("_FillEnabled");
        private static readonly int InwardOutlineId = Shader.PropertyToID("_InwardOutline");
        private static readonly int SidesId = Shader.PropertyToID("_Sides");
        private static readonly int EdgeMaskId = Shader.PropertyToID("_EdgeMask");
        private static readonly int DashEnabledId = Shader.PropertyToID("_DashEnabled");
        private static readonly int DashLengthPixelsId = Shader.PropertyToID("_DashLengthPixels");
        private static readonly int GapLengthPixelsId = Shader.PropertyToID("_GapLengthPixels");
        private static readonly int DashOffsetPixelsId = Shader.PropertyToID("_DashOffsetPixels");
        private static readonly int PpuId = Shader.PropertyToID("_PPU");
        private static readonly int CamPixelOffsetId = Shader.PropertyToID("_CamPixelOffset");

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Material ownedMaterial;
        private Mesh quadMesh;
        private float builtQuadRadius = -1f;
        private int edgeMask = 0x3F;
        private readonly Vector4[] edgeColorCache = new Vector4[MaxSides];
        private readonly float[] edgeMaskCache = new float[MaxSides];

        private int MaskWidth => (1 << this.sides) - 1;

        public float Radius
        {
            get => this.radius;
            set => this.radius = Mathf.Clamp(value, 0f, this.maxRadius);
        }

        public int Sides
        {
            get => this.sides;
            set
            {
                this.sides = Mathf.Clamp(value, MinSides, MaxSides);
                this.EnsureArrayLengths();
                this.SyncEdgeMaskFromFields();
            }
        }

        public float RotationDegrees
        {
            get => this.rotationDegrees;
            set => this.rotationDegrees = value;
        }

        public Color OutlineColor
        {
            get => this.outlineColor;
            set => this.outlineColor = value;
        }

        public float LineWidthPixels
        {
            get => this.lineWidthPixels;
            set => this.lineWidthPixels = value;
        }

        public bool FillEnabled
        {
            get => this.fillEnabled;
            set => this.fillEnabled = value;
        }

        public Color FillColor
        {
            get => this.fillColor;
            set => this.fillColor = value;
        }

        public bool DashEnabled
        {
            get => this.dashEnabled;
            set => this.dashEnabled = value;
        }

        void Awake()
        {
            this.EnsureInitialized();
        }

        // Rebinds component refs and recreates the owned material / quad. Safe to call every frame:
        // it early-outs once set up. Needed because [ExecuteAlways] re-runs OnEnable/LateUpdate after
        // a domain reload without re-running Awake, and the private fields below are reset to null.
        private void EnsureInitialized()
        {
            if (this.meshRenderer.OrNull() != null && this.propertyBlock != null && this.quadMesh.OrNull() != null)
            {
                return;
            }

            if (this.meshFilter.OrNull() == null)
            {
                this.meshFilter = this.GetComponent<MeshFilter>();
            }

            if (this.meshRenderer.OrNull() == null)
            {
                this.meshRenderer = this.GetComponent<MeshRenderer>();
            }

            this.propertyBlock ??= new MaterialPropertyBlock();

            if (this.material.OrNull() == null)
            {
                this.ownedMaterial = new Material(Shader.Find("HackedDesign/HexIndicator"));
                this.material = this.ownedMaterial;
            }

            this.meshRenderer.sharedMaterial = this.material;
            this.meshRenderer.sortingLayerName = this.sortingLayer;
            this.meshRenderer.sortingOrder = this.sortingOrder;

            this.EnsureArrayLengths();
            this.SyncEdgeMaskFromFields();
            this.RebuildQuad();
        }

        void OnEnable()
        {
            this.EnsureInitialized();

            if (this.meshRenderer.OrNull() != null)
            {
                this.meshRenderer.enabled = true;
            }
        }

        void OnDisable()
        {
            if (this.meshRenderer.OrNull() != null)
            {
                this.meshRenderer.enabled = false;
            }
        }

        void OnDestroy()
        {
            if (this.ownedMaterial.OrNull() != null)
            {
                this.DestroyOwned(this.ownedMaterial);
            }

            if (this.quadMesh.OrNull() != null)
            {
                this.DestroyOwned(this.quadMesh);
            }
        }

        private void DestroyOwned(Object obj)
        {
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        void OnValidate()
        {
            this.maxRadius = Mathf.Max(this.maxRadius, 0.01f);
            this.radius = Mathf.Clamp(this.radius, 0f, this.maxRadius);
            this.sides = Mathf.Clamp(this.sides, MinSides, MaxSides);
            this.EnsureArrayLengths();
            this.SyncEdgeMaskFromFields();

            if (!Application.isPlaying && this.isActiveAndEnabled && this.meshRenderer.OrNull() != null)
            {
                this.PushPropertyBlock();
            }
        }

        void LateUpdate()
        {
            this.EnsureInitialized();

            if (this.builtQuadRadius < this.maxRadius)
            {
                this.RebuildQuad();
            }

            this.PushPropertyBlock();
        }

        private void PushPropertyBlock()
        {
            if (this.meshRenderer.OrNull() == null)
            {
                return;
            }

            float rotationRad = ((this.useTransformRotation ? this.transform.eulerAngles.z : 0f) + this.rotationDegrees) * Mathf.Deg2Rad;
            float dashOffset = Mathf.Round(this.dashScrollPixelsPerSecond * Time.time);
            Vector3 center = this.transform.position;

            this.propertyBlock.SetColor(OutlineColorId, this.outlineColor);
            this.propertyBlock.SetColor(FillColorId, this.fillColor);
            this.propertyBlock.SetFloat(UseEdgeColorsId, this.useEdgeColors ? 1f : 0f);
            bool linearColor = QualitySettings.activeColorSpace == ColorSpace.Linear;
            for (int i = 0; i < MaxSides; i++)
            {
                Color c = this.useEdgeColors && this.edgeColors != null && i < this.edgeColors.Length
                    ? this.edgeColors[i]
                    : this.outlineColor;
                // SetVectorArray skips the gamma->linear conversion that SetColor does; match it here.
                this.edgeColorCache[i] = linearColor ? (Vector4)c.linear : (Vector4)c;
                this.edgeMaskCache[i] = (this.edgeMask & (1 << i)) != 0 ? 1f : 0f;
            }
            this.propertyBlock.SetVectorArray(EdgeColorsId, this.edgeColorCache);
            this.propertyBlock.SetFloatArray(EdgeMaskId, this.edgeMaskCache);
            this.propertyBlock.SetFloat(SidesId, this.sides);
            this.propertyBlock.SetFloat(RadiusId, Mathf.Clamp(this.radius, 0f, this.maxRadius));
            this.propertyBlock.SetFloat(RotationRadId, rotationRad);
            this.propertyBlock.SetVector(CenterId, new Vector4(center.x, center.y, 0f, 0f));
            this.propertyBlock.SetFloat(LineWidthPixelsId, Mathf.Max(this.lineWidthPixels, 0f));
            this.propertyBlock.SetFloat(FillEnabledId, this.fillEnabled ? 1f : 0f);
            this.propertyBlock.SetFloat(InwardOutlineId, this.inwardOutline ? 1f : 0f);
            this.propertyBlock.SetFloat(DashEnabledId, this.dashEnabled ? 1f : 0f);
            this.propertyBlock.SetFloat(DashLengthPixelsId, Mathf.Max(this.dashLengthPixels, 0f));
            this.propertyBlock.SetFloat(GapLengthPixelsId, Mathf.Max(this.gapLengthPixels, 0f));
            this.propertyBlock.SetFloat(DashOffsetPixelsId, dashOffset);
            this.propertyBlock.SetFloat(PpuId, PixelsPerUnit);
            this.propertyBlock.SetVector(CamPixelOffsetId, Vector4.zero);

            this.meshRenderer.SetPropertyBlock(this.propertyBlock);
        }

        /// <summary>Sets every edge at once from a bitmask (bit i = edge i visible).</summary>
        public void SetEdgeMask(int mask)
        {
            this.EnsureArrayLengths();
            this.edgeMask = mask & this.MaskWidth;
            for (int i = 0; i < this.sides; i++)
            {
                this.edges[i] = (this.edgeMask & (1 << i)) != 0;
            }
        }

        public int GetEdgeMask() => this.edgeMask;

        public void SetEdge(int index, bool shown)
        {
            if (index < 0 || index >= this.sides)
            {
                return;
            }

            int bit = 1 << index;
            this.SetEdgeMask(shown ? (this.edgeMask | bit) : (this.edgeMask & ~bit));
        }

        public bool GetEdge(int index) => index >= 0 && index < this.sides && (this.edgeMask & (1 << index)) != 0;

        public void SetMaxRadius(float value)
        {
            this.maxRadius = Mathf.Max(value, 0.01f);
            this.radius = Mathf.Clamp(this.radius, 0f, this.maxRadius);
            this.RebuildQuad();
        }

        public bool UseEdgeColors
        {
            get => this.useEdgeColors;
            set => this.useEdgeColors = value;
        }

        /// <summary>Sets the outline colour of a single edge (0..sides-1) and enables per-edge colouring.</summary>
        public void SetEdgeColor(int index, Color color)
        {
            if (index < 0 || index >= this.sides)
            {
                return;
            }

            this.EnsureArrayLengths();
            this.edgeColors[index] = color;
            this.useEdgeColors = true;
        }

        public Color GetEdgeColor(int index)
        {
            if (index < 0 || index >= this.sides || this.edgeColors == null || index >= this.edgeColors.Length)
            {
                return this.outlineColor;
            }

            return this.edgeColors[index];
        }

        /// <summary>Sets every edge colour at once and enables per-edge colouring.</summary>
        public void SetAllEdgeColors(Color color)
        {
            this.EnsureArrayLengths();
            for (int i = 0; i < this.sides; i++)
            {
                this.edgeColors[i] = color;
            }

            this.useEdgeColors = true;
        }

        // Resizes the edges / edgeColors arrays to exactly `sides`, preserving existing entries and
        // filling any new slots with sensible defaults (edge shown, outline colour).
        private void EnsureArrayLengths()
        {
            this.sides = Mathf.Clamp(this.sides, MinSides, MaxSides);

            if (this.edges == null || this.edges.Length != this.sides)
            {
                bool[] resizedEdges = new bool[this.sides];
                for (int i = 0; i < this.sides; i++)
                {
                    resizedEdges[i] = this.edges == null || i >= this.edges.Length || this.edges[i];
                }

                this.edges = resizedEdges;
            }

            if (this.edgeColors == null || this.edgeColors.Length != this.sides)
            {
                Color[] resizedColors = new Color[this.sides];
                for (int i = 0; i < this.sides; i++)
                {
                    resizedColors[i] = this.edgeColors != null && i < this.edgeColors.Length ? this.edgeColors[i] : this.outlineColor;
                }

                this.edgeColors = resizedColors;
            }
        }

        private void SyncEdgeMaskFromFields()
        {
            this.EnsureArrayLengths();

            int mask = 0;
            for (int i = 0; i < this.sides; i++)
            {
                if (this.edges[i])
                {
                    mask |= 1 << i;
                }
            }

            this.edgeMask = mask;
        }

        private void RebuildQuad()
        {
            if (this.meshFilter.OrNull() == null)
            {
                return;
            }

            if (this.quadMesh.OrNull() == null)
            {
                this.quadMesh = new Mesh { name = "HexIndicatorQuad" };
            }

            float half = this.maxRadius * QuadPadding;

            this.quadMesh.Clear();
            this.quadMesh.vertices = new[]
            {
                new Vector3(-half, -half, 0f),
                new Vector3(half, -half, 0f),
                new Vector3(half, half, 0f),
                new Vector3(-half, half, 0f),
            };
            this.quadMesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            this.quadMesh.bounds = new Bounds(Vector3.zero, new Vector3(half * 2f, half * 2f, 0.1f));

            this.meshFilter.sharedMesh = this.quadMesh;
            this.builtQuadRadius = this.maxRadius;
        }
    }
}
