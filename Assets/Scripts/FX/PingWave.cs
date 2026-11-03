using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// An expanding pixel-art ring for the Ping program. Each target is pinged when the ring reaches
    /// it rather than all at once. Instances are pooled and created on demand, so no prefab is needed:
    /// call PingWave.Spawn with the PingRing material.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PingWave : MonoBehaviour
    {
        private const float PixelsPerUnit = 16f;
        private const float QuadPadding = 1.15f;

        private static readonly List<PingWave> pool = new();

        private static readonly int RadiusId = Shader.PropertyToID("_Radius");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int PpuId = Shader.PropertyToID("_PPU");

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propertyBlock;
        private Mesh quadMesh;
        private float builtQuadRadius = -1f;

        private readonly List<Interactable> pending = new();
        private readonly List<float> pendingDistances = new();

        private float maxRadius;
        private float duration;
        private float startTime;

        public static void Spawn(Material material, string sortingLayer, int sortingOrder, Vector3 position, float radius, float duration, IReadOnlyList<Interactable> targets)
        {
            pool.RemoveAll(w => w == null);

            var wave = pool.Find(w => !w.gameObject.activeSelf);
            if (wave == null)
            {
                var go = new GameObject("PingWave");
                wave = go.AddComponent<PingWave>();
                pool.Add(wave);
            }

            wave.Play(material, sortingLayer, sortingOrder, position, radius, duration, targets);
        }

        void Awake()
        {
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.propertyBlock = new MaterialPropertyBlock();
        }

        void OnDestroy()
        {
            if (this.quadMesh != null)
            {
                Destroy(this.quadMesh);
            }
        }

        private void Play(Material material, string sortingLayer, int sortingOrder, Vector3 position, float radius, float duration, IReadOnlyList<Interactable> targets)
        {
            this.transform.position = position;
            this.meshRenderer.sharedMaterial = material;
            this.meshRenderer.sortingLayerName = sortingLayer;
            this.meshRenderer.sortingOrder = sortingOrder;

            this.maxRadius = Mathf.Max(radius, 0.01f);
            this.duration = Mathf.Max(duration, 0.01f);
            this.startTime = Time.time;

            this.pending.Clear();
            this.pendingDistances.Clear();
            foreach (var target in targets)
            {
                if (target == null)
                {
                    continue;
                }
                this.pending.Add(target);
                this.pendingDistances.Add(Vector2.Distance(position, target.transform.position));
            }

            if (this.builtQuadRadius < this.maxRadius)
            {
                this.RebuildQuad();
            }

            this.gameObject.SetActive(true);
            this.Repaint(0f, 1f);
        }

        void Update()
        {
            float t = Mathf.Clamp01((Time.time - this.startTime) / this.duration);

            // Ease out: the ring leaves fast and slows as it reaches the edge of the ping radius.
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float radius = eased * this.maxRadius;
            float alpha = 1f - (t * t);

            for (int i = this.pending.Count - 1; i >= 0; i--)
            {
                if (radius >= this.pendingDistances[i] || t >= 1f)
                {
                    if (this.pending[i] != null)
                    {
                        this.pending[i].Ping();
                    }
                    this.pending.RemoveAt(i);
                    this.pendingDistances.RemoveAt(i);
                }
            }

            this.Repaint(radius, alpha);

            if (t >= 1f)
            {
                this.gameObject.SetActive(false);
            }
        }

        private void Repaint(float radius, float alpha)
        {
            Vector3 center = this.transform.position;
            this.propertyBlock.SetFloat(RadiusId, radius);
            this.propertyBlock.SetFloat(AlphaId, alpha);
            this.propertyBlock.SetVector(CenterId, new Vector4(center.x, center.y, 0f, 0f));
            this.propertyBlock.SetFloat(PpuId, PixelsPerUnit);
            this.meshRenderer.SetPropertyBlock(this.propertyBlock);
        }

        private void RebuildQuad()
        {
            if (this.quadMesh == null)
            {
                this.quadMesh = new Mesh { name = "PingWaveQuad" };
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
