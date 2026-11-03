using HackedDesign.UI.DamageNumbers;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>Cracks the target's barrier edge that faces Mouse, if it is one of the colours this breaker can crack.</summary>
    [CreateAssetMenu(fileName = "Breaker", menuName = "Mouse/OS/Breaker")]
    public class Breaker : Hack
    {
        [SerializeField] private EdgeColour[] breaks = { EdgeColour.Red };

        [Header("Impact")]
        [SerializeField] private HackImpact.Style impact = new();
        [SerializeField] private string tagText = "BREACH";
        [SerializeField] private string blockedTagText = "BLOCKED";
        [SerializeField] private Color blockedTagColour = new(0.55f, 0.55f, 0.6f, 1f);
        [Tooltip("Pixels above the target's bounds where the tag appears.")]
        [SerializeField] private float tagOffsetPixels = 4f;

        public override bool IsBlocked(OperatingSystem os, Vector2 from)
        {
            int edge = os.FacingEdge(from);
            if (edge < 0)
            {
                return false;
            }

            var facing = os.BarrierSlot.Edges[edge];
            return !facing.broken && System.Array.IndexOf(this.breaks, facing.colour) < 0;
        }

        public override void Trigger(GameObject target, PlayerController player) =>
            this.Trigger(target, player, target != null ? target.transform.position : player.transform.position);

        public override void Trigger(GameObject target, PlayerController player, Vector3 hitPoint)
        {
            if (target == null || !target.TryGetComponent<Interactable>(out var interactable) || interactable.OS == null)
            {
                return;
            }

            var os = interactable.OS;
            var from = player.Character.OperatingSystem.BarrierCentre;
            int edge = os.FacingEdge(from);
            if (this.IsBlocked(os, from))
            {
                Debug.Log($"{this.name} blocked by edge {edge} ({os.BarrierSlot.Edges[edge].colour}) on {target.name}", this);
                this.SpawnTag(target, hitPoint, this.blockedTagText, this.blockedTagColour);
                return;
            }

            if (!os.BreakFacingEdge(from))
            {
                return;
            }

            var edgeColour = os.BarrierSlot.Edges[edge].colour;
            Debug.Log($"{this.name} cracked edge {edge} ({edgeColour}) on {target.name}", this);

            var colour = os.barrier.OrNull() != null ? os.barrier.ColourFor(edgeColour) : Color.white;
            bool hasEdge = os.TryGetEdge(edge, out var edgeStart, out var edgeEnd);
            HackImpact.Spawn(this.impact, hitPoint, colour, hasEdge, edgeStart, edgeEnd);
            this.SpawnTag(target, hitPoint, this.tagText, colour);
        }

        private void SpawnTag(GameObject target, Vector3 hitPoint, string text, Color colour)
        {
            if (DamageNumberPool.Instance == null)
            {
                return;
            }

            float top = target.TryGetComponent<Collider2D>(out var body) ? body.bounds.max.y : hitPoint.y;
            DamageNumberPool.Instance.SpawnTag(text, colour, new Vector3(hitPoint.x, top + this.tagOffsetPixels / PixelBatch.PixelsPerUnit, hitPoint.z));
        }
    }
}
