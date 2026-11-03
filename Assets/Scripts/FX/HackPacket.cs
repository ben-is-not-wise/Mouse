using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// The bright packet a targeted hack sends down the aim line. It walks a pixel line from the
    /// origin to the hit point at a fixed speed and calls onArrive when it lands, so the hack's
    /// effect applies on arrival. Pooled and built on demand like PingWave.
    /// </summary>
    public class HackPacket : MonoBehaviour
    {
        public enum Shape
        {
            Square,
            Arrow
        }

        [Serializable]
        public class Style
        {
            public Material material;
            [SortingLayer] public string sortingLayer = "Default";
            public int sortingOrder = 101;
            public Shape shape = Shape.Arrow;
            [Min(1)] public int coreSize = 3;
            [Tooltip("Arrow length in pixels, tip to base.")]
            [Min(1)] public int arrowLength = 5;
            [Tooltip("Half-width gained per pixel back from the tip. 1 = right-angled tip, lower = sharper.")]
            [Range(0.25f, 1.5f)] public float arrowSpread = 0.6f;
            public Color coreColour = Color.white;
            [Min(0)] public int trailPixels = 6;
            public Color trailColour = new(0.996f, 0.086f, 0.286f, 1f);
        }

        private static readonly List<HackPacket> pool = new();

        private readonly List<Vector2Int> path = new();
        private PixelBatch pixels;
        private Style style;
        private Action onArrive;
        private float speed;
        private float startTime;
        private Vector2 direction;

        public static void Spawn(Style style, Vector2 from, Vector2 to, float pixelsPerSecond, Action onArrive)
        {
            pool.RemoveAll(p => p == null);

            var packet = pool.Find(p => !p.gameObject.activeSelf);
            if (packet == null)
            {
                packet = new GameObject("HackPacket").AddComponent<HackPacket>();
                pool.Add(packet);
            }

            packet.Play(style, from, to, pixelsPerSecond, onArrive);
        }

        private void Play(Style style, Vector2 from, Vector2 to, float pixelsPerSecond, Action onArrive)
        {
            this.style = style;
            this.onArrive = onArrive;
            this.speed = Mathf.Max(pixelsPerSecond, 1f);
            this.startTime = Time.time;
            this.direction = (to - from).sqrMagnitude > 0f ? (to - from).normalized : Vector2.right;
            PixelBatch.Line(PixelBatch.ToPixel(from), PixelBatch.ToPixel(to), this.path);

            if (this.pixels == null)
            {
                this.pixels = new PixelBatch(this.transform, style.material, style.sortingLayer, style.sortingOrder);
            }
            else
            {
                this.pixels.Configure(style.material, style.sortingLayer, style.sortingOrder);
            }

            this.gameObject.SetActive(true);
            this.Repaint(0);
        }

        void Update()
        {
            int head = Mathf.FloorToInt((Time.time - this.startTime) * this.speed);
            if (head >= this.path.Count - 1)
            {
                this.pixels.Clear();
                this.gameObject.SetActive(false);
                var arrive = this.onArrive;
                this.onArrive = null;
                arrive?.Invoke();
                return;
            }

            this.Repaint(head);
        }

        private void Repaint(int head)
        {
            this.pixels.Begin();

            int behind = this.style.shape == Shape.Arrow ? this.style.arrowLength - 1 : 0;
            for (int i = 1; i <= this.style.trailPixels; i++)
            {
                int index = head - behind - i;
                if (index < 0)
                {
                    break;
                }
                var colour = this.style.trailColour;
                colour.a *= 1f - (float)(i - 1) / (this.style.trailPixels + 1);
                this.pixels.Draw(this.path[index], colour);
            }

            var centre = this.path[Mathf.Clamp(head, 0, this.path.Count - 1)];
            if (this.style.shape == Shape.Arrow)
            {
                this.DrawArrow(centre);
            }
            else
            {
                int half = this.style.coreSize / 2;
                this.pixels.DrawRect(centre.x - half, centre.y - half, this.style.coreSize, this.style.coreSize, this.style.coreColour);
            }

            this.pixels.End();
        }

        // Rows of pixels behind the tip, each wider than the last, so the head points along the flight path.
        private void DrawArrow(Vector2Int tip)
        {
            var across = new Vector2(-this.direction.y, this.direction.x);
            for (int back = 0; back < this.style.arrowLength; back++)
            {
                Vector2 row = (Vector2)tip - this.direction * back;
                float half = back * this.style.arrowSpread;
                for (float side = -half; side <= half; side += 0.5f)
                {
                    var point = row + across * side;
                    this.pixels.Draw(new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y)), this.style.coreColour);
                }
            }
        }
    }
}
