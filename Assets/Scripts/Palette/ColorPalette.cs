using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Mouse/Colour Palette")]
    public class ColorPalette : ScriptableObject
    {
        [SerializeField] private List<ColorPaletteEntry> entries = new List<ColorPaletteEntry>();

        public event Action Changed;

        public IReadOnlyList<ColorPaletteEntry> Entries => this.entries;

        public ColorPaletteEntry Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (var entry in this.entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            return null;
        }

        public bool TryGetColor(string id, out Color color)
        {
            var entry = this.Find(id);
            color = entry != null ? entry.Color : Color.magenta;
            return entry != null;
        }

        public ColorPaletteEntry Add(string entryName, Color color)
        {
            var entry = new ColorPaletteEntry(entryName, color);
            this.entries.Add(entry);
            this.NotifyChanged();
            return entry;
        }

        public void NotifyChanged()
        {
            this.Changed?.Invoke();
        }

        private void OnValidate()
        {
            var seen = new HashSet<string>();
            foreach (var entry in this.entries)
            {
                if (string.IsNullOrEmpty(entry.Id) || !seen.Add(entry.Id))
                {
                    entry.RegenerateId();
                    seen.Add(entry.Id);
                }
            }

            this.NotifyChanged();
        }
    }

    [Serializable]
    public class ColorPaletteEntry
    {
        [SerializeField, HideInInspector] private string id;
        [SerializeField] private string name;
        [SerializeField] private Color color = Color.white;
        [Tooltip("Brightness boost in stops for bloom. 0 leaves the colour exactly as picked; each +1 doubles it.")]
        [SerializeField, Min(0f)] private float intensity;

        public ColorPaletteEntry(string name, Color color)
        {
            this.id = Guid.NewGuid().ToString("N");
            this.name = name;
            var max = Mathf.Max(color.r, color.g, color.b);
            if (max > 1f)
            {
                this.color = new Color(color.r / max, color.g / max, color.b / max, color.a);
                this.intensity = Mathf.Log(max, 2f);
            }
            else
            {
                this.color = color;
            }
        }

        public string Id => this.id;

        public string Name => this.name;

        public Color BaseColor => this.color;

        public float Intensity => this.intensity;

        public Color Color
        {
            get
            {
                if (this.intensity <= 0f)
                {
                    return this.color;
                }

                var scale = Mathf.Pow(2f, this.intensity);
                return new Color(this.color.r * scale, this.color.g * scale, this.color.b * scale, this.color.a);
            }
        }

        internal void RegenerateId()
        {
            this.id = Guid.NewGuid().ToString("N");
        }
    }
}
