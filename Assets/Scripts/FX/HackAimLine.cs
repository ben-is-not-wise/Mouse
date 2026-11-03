using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>
    /// Packet trace aim line for targeted hacks. While one is selected, a dotted pixel line marches
    /// out from Mouse's visor along the aim: grey while searching, pink/blue with corner brackets
    /// when locked, dim when short on momentum, fading past the hack's range, and ending in a pink
    /// X when a wall blocks a target. Also sends the packet that carries the hack to its target.
    /// Repaint must be called every frame it should show; otherwise it hides itself.
    /// </summary>
    public class HackAimLine : MonoBehaviour
    {
        private enum AimState
        {
            Searching,
            Locked,
            Dim,
            Blocked,
            OutOfRange
        }

        [SerializeField] private Transform origin;
        [SerializeField] private Material material;
        [SerializeField, SortingLayer] private string sortingLayer = "Default";
        [SerializeField] private int sortingOrder = 100;

        [Header("Dots")]
        [SerializeField, Min(1)] private int dotSpacing = 4;
        [SerializeField, Min(0)] private int startGap = 2;
        [SerializeField] private float lockedSpeed = 30f;
        [SerializeField] private float searchingSpeed = 14f;
        [SerializeField] private Color searchingColour = new(0.55f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color[] lockedColours =
        {
            new(0.996f, 0.086f, 0.286f, 1f),
            new(0.208f, 0.502f, 0.502f, 1f),
            new(0.71f, 0.157f, 0.325f, 1f),
            new(0.208f, 0.502f, 0.502f, 1f)
        };
        [SerializeField] private Color dimColour = new(0.36f, 0.08f, 0.16f, 1f);
        [SerializeField, Min(1)] private int outOfRangeFadePixels = 20;

        [Header("Brackets")]
        [SerializeField, Min(1)] private int bracketArm = 3;
        [SerializeField, Min(0)] private int bracketStartPadding = 6;
        [SerializeField, Min(0)] private int bracketEndPadding = 0;
        [SerializeField, Min(0)] private float bracketSnapDuration = 0.18f;
        [SerializeField] private Color bracketColour = new(0.996f, 0.086f, 0.286f, 1f);

        [Header("Blocked")]
        [SerializeField] private Color blockedColour = new(0.996f, 0.086f, 0.286f, 1f);
        [SerializeField, Min(1)] private int crossRadius = 2;

        [Header("Packet")]
        [SerializeField] private HackPacket.Style packet = new();

        private readonly List<Vector2Int> path = new();
        private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        private Transform container;
        private PixelBatch pixels;
        private int shownFrame = -1;
        private bool visible;
        private Interactable lockedTarget;
        private float lockedAt;
        private Vector2 end;

        public Vector2 Origin => this.origin != null ? this.origin.position : this.transform.position;

        void Awake()
        {
            this.container = new GameObject("HackAimLine").transform;
            this.pixels = new PixelBatch(this.container, this.material, this.sortingLayer, this.sortingOrder);
        }

        void OnDestroy()
        {
            if (this.container != null)
            {
                Destroy(this.container.gameObject);
            }
        }

        void LateUpdate()
        {
            if (this.visible && this.shownFrame != Time.frameCount)
            {
                this.Hide();
            }
        }

        public void Repaint(OperatingSystem os, Targeter targeter, Vector3 playerPosition)
        {
            var hack = os.ActiveActionHack?.definition;
            Vector2 direction = targeter.AimDirection;
            if (hack == null || !hack.RequiresTarget || !os.Enabled || direction.sqrMagnitude < 0.01f)
            {
                this.Hide();
                return;
            }

            this.shownFrame = Time.frameCount;
            this.visible = true;

            Vector2 start = this.Origin;
            var target = targeter.CurrentTarget;
            AimState state;
            Vector2 lineEnd;

            if (target != null && (target.transform.position - playerPosition).sqrMagnitude <= hack.range * hack.range)
            {
                lineEnd = targeter.TargetPoint;
                state = os.Momentum < hack.puUsage ? AimState.Dim : AimState.Locked;
            }
            else if (target != null)
            {
                lineEnd = start + direction * hack.range;
                state = AimState.OutOfRange;
            }
            else
            {
                lineEnd = this.Probe(start, direction, hack.range, targeter, out bool blocked);
                state = blocked ? AimState.Blocked : AimState.Searching;
            }

            bool locked = state == AimState.Locked || state == AimState.Dim;
            if (!locked)
            {
                this.lockedTarget = null;
            }
            else if (this.lockedTarget != target)
            {
                this.lockedTarget = target;
                this.lockedAt = Time.time;
            }
            this.end = lineEnd;

            if (locked && target.OS != null && target.OS.barrier.OrNull() != null)
            {
                target.OS.barrier.HighlightEdge(target.OS.FacingEdge(os.BarrierCentre));
            }

            this.pixels.Begin();
            this.DrawDots(start, lineEnd, state);
            if (locked)
            {
                this.DrawBrackets(target, state == AimState.Dim ? this.dimColour : this.bracketColour);
            }
            else if (state == AimState.Blocked)
            {
                this.DrawCross(PixelBatch.ToPixel(lineEnd));
            }
            this.pixels.End();
        }

        public void Hide()
        {
            if (!this.visible)
            {
                return;
            }

            this.visible = false;
            this.lockedTarget = null;
            this.pixels.Clear();
        }

        /// <summary>Sends the hack down the line as a packet; its effect applies when the packet lands.</summary>
        public void Fire(Hack hack, GameObject target, PlayerController player)
        {
            Vector3 hitPoint = this.lockedTarget != null && target == this.lockedTarget.gameObject
                ? this.end
                : target != null ? target.transform.position : this.end;

            if (hack.TravelSpeed <= 0f)
            {
                hack.Trigger(target, player, hitPoint);
                return;
            }

            HackPacket.Spawn(this.packet, this.Origin, hitPoint, hack.TravelSpeed, () => hack.Trigger(target, player, hitPoint));
        }

        // Where a searching line stops: the first wall, or the end of the hack's range. Blocked when
        // something hackable sits behind that wall within range.
        private Vector2 Probe(Vector2 start, Vector2 direction, float range, Targeter targeter, out bool blocked)
        {
            blocked = false;
            int count = Physics2D.RaycastNonAlloc(start, direction, this.hitBuffer, range, targeter.TargetMask | targeter.LaserMask);
            Vector2? wall = null;

            for (int i = 0; i < count; i++)
            {
                var collider = this.hitBuffer[i].collider;
                if (collider.transform.root == this.transform.root)
                {
                    continue;
                }

                bool hackable = collider.TryGetComponent<Interactable>(out var interactable) && interactable.OS != null;
                if (wall == null)
                {
                    if (!collider.isTrigger && !hackable && targeter.LaserMask.Contains(collider.gameObject.layer))
                    {
                        wall = this.hitBuffer[i].point;
                    }
                }
                else if (hackable)
                {
                    blocked = true;
                    break;
                }
            }

            return wall ?? start + direction * range;
        }

        private void DrawDots(Vector2 start, Vector2 lineEnd, AimState state)
        {
            PixelBatch.Line(PixelBatch.ToPixel(start), PixelBatch.ToPixel(lineEnd), this.path);

            bool locked = state == AimState.Locked || state == AimState.Dim;
            int march = Mathf.FloorToInt(Time.time * (locked ? this.lockedSpeed : this.searchingSpeed));
            int offset = march % this.dotSpacing;
            int cycle = march / this.dotSpacing;
            int fadeFrom = state == AimState.OutOfRange ? this.path.Count - this.outOfRangeFadePixels : int.MaxValue;

            for (int i = offset; i < this.path.Count; i += this.dotSpacing)
            {
                if (i < this.startGap)
                {
                    continue;
                }

                int ordinal = (i - offset) / this.dotSpacing;
                Color colour = state switch
                {
                    AimState.Locked => this.LockedColour(ordinal - cycle),
                    AimState.Dim => this.dimColour,
                    _ => this.searchingColour
                };

                if (i >= fadeFrom)
                {
                    if ((ordinal & 1) == 1)
                    {
                        continue;
                    }
                    colour.a *= 1f - (float)(i - fadeFrom) / this.outOfRangeFadePixels;
                }

                this.pixels.Draw(this.path[i], colour);
            }
        }

        private Color LockedColour(int index)
        {
            if (this.lockedColours == null || this.lockedColours.Length == 0)
            {
                return this.bracketColour;
            }

            int length = this.lockedColours.Length;
            return this.lockedColours[((index % length) + length) % length];
        }

        private void DrawBrackets(Interactable target, Color colour)
        {
            Bounds bounds = target.TryGetComponent<Collider2D>(out var body)
                ? body.bounds
                : new Bounds(target.transform.position, Vector3.one * 0.5f);

            float t = this.bracketSnapDuration > 0f ? Mathf.Clamp01((Time.time - this.lockedAt) / this.bracketSnapDuration) : 1f;
            int padding = Mathf.RoundToInt(Mathf.Lerp(this.bracketStartPadding, this.bracketEndPadding, t));
            var min = PixelBatch.ToPixel(bounds.min) - Vector2Int.one * padding;
            var max = PixelBatch.ToPixel(bounds.max) + Vector2Int.one * padding;
            int arm = this.bracketArm;

            this.pixels.DrawRect(min.x, min.y, arm, 1, colour);
            this.pixels.DrawRect(min.x, min.y, 1, arm, colour);
            this.pixels.DrawRect(max.x - arm + 1, min.y, arm, 1, colour);
            this.pixels.DrawRect(max.x, min.y, 1, arm, colour);
            this.pixels.DrawRect(min.x, max.y, arm, 1, colour);
            this.pixels.DrawRect(min.x, max.y - arm + 1, 1, arm, colour);
            this.pixels.DrawRect(max.x - arm + 1, max.y, arm, 1, colour);
            this.pixels.DrawRect(max.x, max.y - arm + 1, 1, arm, colour);
        }

        private void DrawCross(Vector2Int centre)
        {
            for (int d = -this.crossRadius; d <= this.crossRadius; d++)
            {
                this.pixels.Draw(centre + new Vector2Int(d, d), this.blockedColour);
                this.pixels.Draw(centre + new Vector2Int(d, -d), this.blockedColour);
            }
        }
    }
}
