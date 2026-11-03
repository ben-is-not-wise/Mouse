using HackedDesign.UI.DamageNumbers;
using UnityEngine;

namespace HackedDesign
{
    /// <summary>Cracks the target's barrier edge that faces Mouse. Any colour; reactions come later.</summary>
    [CreateAssetMenu(fileName = "Breaker", menuName = "Mouse/OS/Breaker")]
    public class Breaker : Hack
    {
        [Header("Impact")]
        [SerializeField] private HackImpact.Style impact = new();
        [SerializeField] private string tagText = "BREACH";
        [Tooltip("Pixels above the target's bounds where the tag appears.")]
        [SerializeField] private float tagOffsetPixels = 4f;

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
            if (!os.BreakFacingEdge(from))
            {
                return;
            }

            var edgeColour = os.BarrierSlot.Edges[edge].colour;
            Debug.Log($"Breaker cracked edge {edge} ({edgeColour}) on {target.name}", this);

            var colour = os.barrier.OrNull() != null ? os.barrier.ColourFor(edgeColour) : Color.white;
            bool hasEdge = os.TryGetEdge(edge, out var edgeStart, out var edgeEnd);
            HackImpact.Spawn(this.impact, hitPoint, colour, hasEdge, edgeStart, edgeEnd);

            if (DamageNumberPool.Instance != null)
            {
                float top = target.TryGetComponent<Collider2D>(out var body) ? body.bounds.max.y : hitPoint.y;
                DamageNumberPool.Instance.SpawnTag(this.tagText, colour, new Vector3(hitPoint.x, top + this.tagOffsetPixels / PixelBatch.PixelsPerUnit, hitPoint.z));
            }
        }
    }
}
