using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// Draws whole game pixels with pooled 1x1 sprites, so pixel-art FX stay on the 16 PPU grid
    /// without a prefab or texture asset. Call Begin, draw, then End each frame; renderers not
    /// drawn this frame are hidden.
    /// </summary>
    public class PixelBatch
    {
        public const float PixelsPerUnit = 16f;

        private const string UnlitShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

        private static Sprite pixelSprite;
        private static Material fallbackMaterial;

        private readonly List<SpriteRenderer> renderers = new();
        private readonly Transform parent;
        private Material material;
        private string sortingLayer;
        private int sortingOrder;
        private int used;

        public PixelBatch(Transform parent, Material material, string sortingLayer, int sortingOrder)
        {
            this.parent = parent;
            this.Configure(material, sortingLayer, sortingOrder);
        }

        public static Sprite PixelSprite
        {
            get
            {
                if (pixelSprite == null)
                {
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        name = "PixelBatchTexture"
                    };
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    pixelSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), PixelsPerUnit);
                    pixelSprite.name = "PixelBatchSprite";
                }

                return pixelSprite;
            }
        }

        public static Vector2Int ToPixel(Vector2 world) => new(Mathf.FloorToInt(world.x * PixelsPerUnit), Mathf.FloorToInt(world.y * PixelsPerUnit));

        public static Vector2 ToWorld(Vector2Int pixel) => new((pixel.x + 0.5f) / PixelsPerUnit, (pixel.y + 0.5f) / PixelsPerUnit);

        public static void Line(Vector2Int from, Vector2Int to, List<Vector2Int> path)
        {
            path.Clear();
            int dx = Mathf.Abs(to.x - from.x);
            int dy = -Mathf.Abs(to.y - from.y);
            int stepX = from.x < to.x ? 1 : -1;
            int stepY = from.y < to.y ? 1 : -1;
            int error = dx + dy;
            int x = from.x;
            int y = from.y;

            while (true)
            {
                path.Add(new Vector2Int(x, y));
                if (x == to.x && y == to.y)
                {
                    break;
                }

                int doubled = 2 * error;
                if (doubled >= dy)
                {
                    error += dy;
                    x += stepX;
                }
                if (doubled <= dx)
                {
                    error += dx;
                    y += stepY;
                }
            }
        }

        public void Configure(Material material, string sortingLayer, int sortingOrder)
        {
            this.material = material != null ? material : FallbackMaterial();
            this.sortingLayer = sortingLayer;
            this.sortingOrder = sortingOrder;

            foreach (var renderer in this.renderers)
            {
                this.Apply(renderer);
            }
        }

        public void Begin()
        {
            this.used = 0;
        }

        public void Draw(Vector2Int pixel, Color colour) => this.DrawRect(pixel.x, pixel.y, 1, 1, colour);

        public void DrawRect(int x, int y, int width, int height, Color colour)
        {
            if (width <= 0 || height <= 0 || colour.a <= 0f)
            {
                return;
            }

            var renderer = this.Next();
            renderer.transform.position = new Vector3((x + width * 0.5f) / PixelsPerUnit, (y + height * 0.5f) / PixelsPerUnit, 0f);
            renderer.transform.localScale = new Vector3(width, height, 1f);
            renderer.color = colour;
        }

        public void End()
        {
            for (int i = this.used; i < this.renderers.Count; i++)
            {
                if (this.renderers[i].enabled)
                {
                    this.renderers[i].enabled = false;
                }
            }
        }

        public void Clear()
        {
            this.Begin();
            this.End();
        }

        private SpriteRenderer Next()
        {
            if (this.used == this.renderers.Count)
            {
                var go = new GameObject("Pixel");
                go.transform.SetParent(this.parent, false);
                var created = go.AddComponent<SpriteRenderer>();
                created.sprite = PixelSprite;
                this.Apply(created);
                this.renderers.Add(created);
            }

            var renderer = this.renderers[this.used++];
            renderer.enabled = true;
            return renderer;
        }

        private void Apply(SpriteRenderer renderer)
        {
            if (this.material != null)
            {
                renderer.sharedMaterial = this.material;
            }
            renderer.sortingLayerName = this.sortingLayer;
            renderer.sortingOrder = this.sortingOrder;
        }

        private static Material FallbackMaterial()
        {
            if (fallbackMaterial == null)
            {
                var shader = Shader.Find(UnlitShaderName);
                if (shader != null)
                {
                    fallbackMaterial = new Material(shader) { name = "PixelBatchUnlit" };
                }
            }

            return fallbackMaterial;
        }
    }
}
