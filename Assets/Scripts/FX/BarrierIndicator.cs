using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// Drives a HexIndicator as a character's barrier. Idle, the barrier is only a faint, colourless
    /// hint of its intact edges. A Ping (or an edge breaking) reveals it for revealDuration: full opacity, the
    /// pink/blue colour split with a stepped jitter, and broken edges shown as a dotted placeholder
    /// that refills from one end as the edge regrows. After the timeout it fades back in steps.
    /// </summary>
    public class BarrierIndicator: MonoBehaviour
    {
        public HexIndicator indicator;

        // Indexed by EdgeColour. Pink and blue stay reserved for Mouse's own UI.
        [SerializeField] private Color[] edgePalette =
        {
            new(1f, 0.2f, 0.2f, 1f),    // Red: Firewall
            new(1f, 0.7f, 0.1f, 1f),    // Amber: Authentication
            new(0.6f, 0.3f, 1f, 1f),    // Violet: Encryption
            new(0.2f, 1f, 0.4f, 1f),    // Green: Intrusion detection
            new(1f, 1f, 1f, 1f),        // White: Watchdog
            new(0.25f, 0.25f, 0.3f, 1f) // Black: Black ICE
        };

        [Header("Reveal")]
        [SerializeField] private float revealDuration = 10f;
        [SerializeField] private float fadeDuration = 0.75f;
        [SerializeField, Min(1)] private int fadeSteps = 4;
        [SerializeField, Range(0f, 1f)] private float revealedOpacity = 0.85f;
        [SerializeField, Range(0f, 1f)] private float idleOpacity = 0.05f;
        [Tooltip("Idle edges drop their defence colour and draw in this colour instead, so colour only shows on Ping.")]
        [SerializeField] private bool colourlessWhenIdle = true;
        [SerializeField] private Color idleColour = Color.white;

        [Header("Broken edges")]
        [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.3f;
        [SerializeField, Range(0f, 1f)] private float regrowAlpha = 0.55f;
        [SerializeField] private float breakFlashDuration = 0.1f;
        [SerializeField] private Color breakFlashColour = Color.white;

        [Header("Hack lock")]
        [Tooltip("While Mouse is locked on with a targeted hack, light the edge it would crack in its defence colour, even if not Pinged.")]
        [SerializeField] private bool showFacingEdgeOnLock = true;
        [SerializeField] private float lockBlinkDuration = 0.25f;
        [SerializeField] private float lockBlinkRate = 12f;

        [Header("Glitch")]
        [SerializeField] private int chromaPixels = 1;
        [SerializeField] private float jitterInterval = 0.08f;
        [SerializeField, Range(0f, 1f)] private float jitterChance = 0.2f;
        [SerializeField] private float glitchInDuration = 0.25f;

        private float revealedAt = float.NegativeInfinity;
        private float heldAt = float.NegativeInfinity;
        private float nextJitterAt = 0f;
        private Vector2 jitter = Vector2.zero;
        private int lockedEdge = -1;
        private int lockedFrame = int.MinValue;
        private float lockedAt = float.NegativeInfinity;

        public bool ShowFacingEdgeOnLock
        {
            get => this.showFacingEdgeOnLock;
            set => this.showFacingEdgeOnLock = value;
        }

        /// <summary>Lights an edge as the one a locked hack would crack. Call every frame while locked.</summary>
        public void HighlightEdge(int edge)
        {
            if (Time.frameCount - this.lockedFrame > 1)
            {
                this.lockedAt = Time.time;
            }

            this.lockedEdge = edge;
            this.lockedFrame = Time.frameCount;
        }

        /// <summary>Shows the barrier in full for revealDuration (restarts the timer if already shown).</summary>
        public void Reveal()
        {
            this.revealedAt = Time.time;
        }

        public bool IsRevealed => Time.time - Mathf.Max(this.revealedAt, this.heldAt) < this.revealDuration + this.fadeDuration;

        public void Repaint(BarrierInstance barrier, float currentRotation, bool show)
        {
            if (!show)
            {
                if (indicator.OrNull() != null && indicator.isActiveAndEnabled)
                {
                    indicator.gameObject.SetActive(false);
                }
                return;
            }

            float now = Time.time;
            var edges = barrier.Edges;

            // A broken edge holds the barrier revealed until it has fully regrown, so the player can
            // watch the refill. The usual timeout then counts from the moment it is whole again.
            for (int i = 0; i < edges.Length; i++)
            {
                if (edges[i].broken)
                {
                    this.revealedAt = Mathf.Max(this.revealedAt, edges[i].brokenAt);
                    this.heldAt = now;
                }
            }

            float reveal = RevealAmount(now);
            bool glitching = now - this.revealedAt < this.glitchInDuration || (reveal > 0f && reveal < 1f);

            indicator.RotationDegrees = currentRotation;
            indicator.Radius = barrier.definition.Radius;
            if (indicator.Sides != edges.Length)
            {
                indicator.Sides = edges.Length;
            }

            // A lit edge needs full opacity while the rest stay at the base opacity, so the others
            // carry the difference in their colour alpha.
            bool locked = this.showFacingEdgeOnLock && this.lockedEdge >= 0 && Time.frameCount - this.lockedFrame <= 1;
            float baseOpacity = Mathf.Lerp(this.idleOpacity, this.revealedOpacity, reveal);
            indicator.Opacity = locked ? Mathf.Max(baseOpacity, this.revealedOpacity) : baseOpacity;
            float otherAlpha = indicator.Opacity > 0f ? baseOpacity / indicator.Opacity : 0f;
            bool lockBlinkOn = now - this.lockedAt >= this.lockBlinkDuration || Mathf.Repeat((now - this.lockedAt) * this.lockBlinkRate, 1f) < 0.5f;
            indicator.GhostAlpha = this.ghostAlpha * reveal;
            indicator.RegrowAlpha = this.regrowAlpha;
            indicator.ChromaOffsetPixels = reveal > 0f ? new Vector2(this.chromaPixels, 0f) + Jitter(now, glitching) : Vector2.zero;

            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];
                bool flashing = edge.broken && now - edge.brokenAt < this.breakFlashDuration;

                indicator.SetEdge(i, !edge.broken || flashing);
                Color colour = this.colourlessWhenIdle ? Color.Lerp(this.idleColour, ColourFor(edge.colour), reveal) : ColourFor(edge.colour);
                colour = flashing ? this.breakFlashColour : colour;
                if (locked && i == this.lockedEdge && !edge.broken)
                {
                    colour = ColourFor(edge.colour);
                    colour.a = lockBlinkOn ? 1f : otherAlpha;
                }
                else
                {
                    colour.a *= otherAlpha;
                }
                indicator.SetEdgeColor(i, colour);
                indicator.SetEdgeRegrow(i, edge.broken && !flashing ? edge.RegrowProgress(now) : -1f);
            }

            indicator.gameObject.SetActive(true);
        }

        // 1 while revealed, then steps down to 0 over the fade, so the fade reads as pixel-art frames.
        private float RevealAmount(float now)
        {
            float since = now - Mathf.Max(this.revealedAt, this.heldAt);
            if (since < this.revealDuration)
            {
                return 1f;
            }

            if (this.fadeDuration <= 0f || since >= this.revealDuration + this.fadeDuration)
            {
                return 0f;
            }

            float k = (since - this.revealDuration) / this.fadeDuration;
            return 1f - Mathf.Ceil(k * this.fadeSteps) / this.fadeSteps;
        }

        // Stepped whole-pixel jitter of the colour split: occasional while shown, constant while glitching in or out.
        private Vector2 Jitter(float now, bool glitching)
        {
            if (now >= this.nextJitterAt)
            {
                this.nextJitterAt = now + this.jitterInterval;
                this.jitter = glitching || Random.value < this.jitterChance
                    ? new Vector2(Random.Range(-1, 3), Random.Range(-1, 2))
                    : Vector2.zero;
            }

            return this.jitter;
        }

        public Color ColourFor(EdgeColour colour)
        {
            int index = (int)colour;
            return edgePalette != null && index < edgePalette.Length ? edgePalette[index] : Color.white;
        }
    }
}
