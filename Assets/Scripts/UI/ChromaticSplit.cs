#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    // Pink/blue chromatic split for any uGUI Graphic (Image, Text, RawImage). Works like Unity's
    // Shadow effect: the mesh is drawn three times, a blue copy shifted left, a pink copy shifted
    // right, then the original on top, so only the edges pick up colour. Offsets are in canvas
    // units, which are game pixels with the 640x360 reference resolution, and are rounded so the
    // copies stay on the pixel grid. Call Glitch() to kick the split wider for a moment
    // (window open, tab switch, cursor move).
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Effects/Chromatic Split")]
    public class ChromaticSplit : BaseMeshEffect
    {
        [SerializeField] private Color leftColor = new Color(0.243f, 0.773f, 1f, 1f);   // blue
        [SerializeField] private Color rightColor = new Color(1f, 0.31f, 0.639f, 1f);   // pink
        [SerializeField] private Vector2 offset = new Vector2(1f, 0f);
        [SerializeField] private bool useGraphicAlpha = true;

        [Header("Glitch")]
        [SerializeField] private float glitchOffset = 4f;
        [SerializeField] private float glitchDuration = 0.08f;
        [SerializeField] private bool glitchOnEnable = true;

        private static readonly List<UIVertex> Source = new List<UIVertex>();
        private static readonly List<UIVertex> Output = new List<UIVertex>();

        private float glitchUntil;
        private float glitchAmount;

        public Vector2 Offset
        {
            get => offset;
            set { 
                offset = value; 
                Refresh(); 
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying && glitchOnEnable)
            {
                Glitch();
            }
        }

        // Widen the split for a few frames. Uses unscaled time so it works while the game is paused.
        public void Glitch() => Glitch(glitchOffset, glitchDuration);

        public void Glitch(float amount, float duration)
        {
            glitchAmount = amount;
            glitchUntil = Time.unscaledTime + duration;
            Refresh();
        }

        void Update()
        {
            if (glitchUntil > 0f && Time.unscaledTime >= glitchUntil)
            {
                glitchUntil = 0f;
                Refresh();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
            {
                return;
            }

            var current = glitchUntil > 0f ? offset.normalized * glitchAmount : offset;
            var shift = new Vector3(Mathf.Round(current.x), Mathf.Round(current.y), 0f);

            Source.Clear();
            Output.Clear();
            vh.GetUIVertexStream(Source);

            AppendCopy(Source, -shift, leftColor);
            AppendCopy(Source, shift, rightColor);
            Output.AddRange(Source);

            vh.Clear();
            vh.AddUIVertexTriangleStream(Output);
        }

        private void AppendCopy(List<UIVertex> verts, Vector3 shift, Color tint)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                var v = verts[i];
                v.position += shift;
                Color32 c = tint;
                if (useGraphicAlpha)
                {
                    c.a = (byte)(c.a * v.color.a / 255);
                }
                v.color = c;
                Output.Add(v);
            }
        }

        private void Refresh()
        {
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Refresh();
        }
#endif
    }
}
