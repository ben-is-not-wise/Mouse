using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// Pixel-art impact for a hack packet landing on a barrier: a stepped expanding ring of points,
    /// pink and blue copies of the broken edge drifting apart, and a burst of falling shards that
    /// blink out. Pooled and built on demand like PingWave.
    /// </summary>
    public class HackImpact : MonoBehaviour
    {
        [Serializable]
        public class Style
        {
            public Material material;
            [SortingLayer] public string sortingLayer = "Default";
            public int sortingOrder = 102;

            [Header("Ring")]
            [Min(3)] public int ringPoints = 16;
            [Min(0)] public float ringStartRadius = 2f;
            [Min(0)] public float ringEndRadius = 12f;
            [Min(1)] public int ringSteps = 4;
            [Min(0)] public float ringDuration = 0.2f;
            public Color ringColour = Color.white;

            [Header("Edge split")]
            [Tooltip("Wait for the barrier's white break flash before splitting.")]
            [Min(0)] public float splitDelay = 0.1f;
            [Min(0)] public float splitDuration = 0.2f;
            [Min(0)] public int splitMinPixels = 1;
            [Min(0)] public int splitMaxPixels = 3;
            public Color splitColourA = new(0.996f, 0.086f, 0.286f, 1f);
            public Color splitColourB = new(0.208f, 0.502f, 0.502f, 1f);

            [Header("Shards")]
            [Min(0)] public int shardCount = 10;
            [Min(0)] public float shardLife = 0.6f;
            [Min(0)] public float shardSpeed = 60f;
            [Min(0)] public float shardGravity = 240f;
            [Min(0)] public float shardBlinkTime = 0.2f;
            [Min(0.01f)] public float shardBlinkRate = 15f;
        }

        private struct Shard
        {
            public Vector2 position;
            public Vector2 velocity;
            public Color colour;
            public float life;
        }

        private static readonly List<HackImpact> pool = new();

        private readonly List<Vector2Int> edgePath = new();
        private readonly List<Shard> shards = new();
        private PixelBatch pixels;
        private Style style;
        private Vector2Int centre;
        private Vector2Int splitNormal;
        private bool hasEdge;
        private float startTime;
        private float endTime;

        public static void Spawn(Style style, Vector2 point, Color edgeColour, bool hasEdge, Vector2 edgeStart, Vector2 edgeEnd)
        {
            pool.RemoveAll(i => i == null);

            var impact = pool.Find(i => !i.gameObject.activeSelf);
            if (impact == null)
            {
                impact = new GameObject("HackImpact").AddComponent<HackImpact>();
                pool.Add(impact);
            }

            impact.Play(style, point, edgeColour, hasEdge, edgeStart, edgeEnd);
        }

        private void Play(Style style, Vector2 point, Color edgeColour, bool hasEdge, Vector2 edgeStart, Vector2 edgeEnd)
        {
            this.style = style;
            this.centre = PixelBatch.ToPixel(point);
            this.hasEdge = hasEdge;
            this.startTime = Time.time;

            if (this.pixels == null)
            {
                this.pixels = new PixelBatch(this.transform, style.material, style.sortingLayer, style.sortingOrder);
            }
            else
            {
                this.pixels.Configure(style.material, style.sortingLayer, style.sortingOrder);
            }

            this.edgePath.Clear();
            if (hasEdge)
            {
                PixelBatch.Line(PixelBatch.ToPixel(edgeStart), PixelBatch.ToPixel(edgeEnd), this.edgePath);
                var along = edgeEnd - edgeStart;
                var normal = new Vector2(-along.y, along.x);
                this.splitNormal = Mathf.Abs(normal.x) > Mathf.Abs(normal.y)
                    ? new Vector2Int((int)Mathf.Sign(normal.x), 0)
                    : new Vector2Int(0, (int)Mathf.Sign(normal.y));
            }

            this.shards.Clear();
            Color[] shardColours = { edgeColour, style.splitColourA, style.splitColourB };
            for (int i = 0; i < style.shardCount; i++)
            {
                var source = this.edgePath.Count > 0 ? (Vector2)this.edgePath[UnityEngine.Random.Range(0, this.edgePath.Count)] : (Vector2)this.centre;
                var direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.2f, 1f)).normalized;
                this.shards.Add(new Shard
                {
                    position = source,
                    velocity = direction * style.shardSpeed * UnityEngine.Random.Range(0.5f, 1f),
                    colour = shardColours[i % shardColours.Length],
                    life = style.shardLife * UnityEngine.Random.Range(0.7f, 1f)
                });
            }

            this.endTime = this.startTime + Mathf.Max(style.ringDuration, hasEdge ? style.splitDelay + style.splitDuration : 0f, style.shardLife);
            this.gameObject.SetActive(true);
            this.Repaint(0f, 0f);
        }

        void Update()
        {
            float now = Time.time;
            if (now >= this.endTime)
            {
                this.pixels.Clear();
                this.gameObject.SetActive(false);
                return;
            }

            this.Repaint(now - this.startTime, Time.deltaTime);
        }

        private void Repaint(float elapsed, float deltaTime)
        {
            this.pixels.Begin();
            this.DrawRing(elapsed);
            this.DrawSplit(elapsed);
            this.DrawShards(elapsed, deltaTime);
            this.pixels.End();
        }

        private void DrawRing(float elapsed)
        {
            if (elapsed >= this.style.ringDuration)
            {
                return;
            }

            float step = Mathf.Ceil(elapsed / Mathf.Max(this.style.ringDuration, 0.0001f) * this.style.ringSteps) / this.style.ringSteps;
            float radius = Mathf.Lerp(this.style.ringStartRadius, this.style.ringEndRadius, step);
            for (int i = 0; i < this.style.ringPoints; i++)
            {
                float angle = i * Mathf.PI * 2f / this.style.ringPoints;
                var offset = new Vector2Int(Mathf.RoundToInt(Mathf.Cos(angle) * radius), Mathf.RoundToInt(Mathf.Sin(angle) * radius));
                this.pixels.Draw(this.centre + offset, this.style.ringColour);
            }
        }

        private void DrawSplit(float elapsed)
        {
            float t = (elapsed - this.style.splitDelay) / Mathf.Max(this.style.splitDuration, 0.0001f);
            if (!this.hasEdge || t < 0f || t >= 1f)
            {
                return;
            }

            int drift = Mathf.RoundToInt(Mathf.Lerp(this.style.splitMinPixels, this.style.splitMaxPixels, t));
            var offset = this.splitNormal * drift;
            foreach (var pixel in this.edgePath)
            {
                this.pixels.Draw(pixel + offset, this.style.splitColourA);
                this.pixels.Draw(pixel - offset, this.style.splitColourB);
            }
        }

        private void DrawShards(float elapsed, float deltaTime)
        {
            for (int i = 0; i < this.shards.Count; i++)
            {
                var shard = this.shards[i];
                shard.velocity.y -= this.style.shardGravity * deltaTime;
                shard.position += shard.velocity * deltaTime;
                this.shards[i] = shard;

                float remaining = shard.life - elapsed;
                if (remaining <= 0f)
                {
                    continue;
                }
                if (remaining < this.style.shardBlinkTime && Mathf.Repeat(remaining * this.style.shardBlinkRate, 1f) < 0.5f)
                {
                    continue;
                }

                this.pixels.Draw(new Vector2Int(Mathf.RoundToInt(shard.position.x), Mathf.RoundToInt(shard.position.y)), shard.colour);
            }
        }
    }
}
