using HackedDesign.UI.DamageNumbers;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Crash", menuName = "Mouse/OS/Crash")]
    public class Crash : Hack
    {
        [SerializeField, Min(0)] private float stunDuration = 3f;
        [SerializeField] private EdgeColour[] blockedBy = { EdgeColour.Red, EdgeColour.Black };

        [Header("Impact")]
        [SerializeField] private HackImpact.Style impact = new();
        [SerializeField] private string tagText = "CRASH";
        [SerializeField] private Color tagColour = new(1f, 0.6f, 0.1f, 1f);
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
            return !facing.broken && System.Array.IndexOf(this.blockedBy, facing.colour) >= 0;
        }

        public override void Trigger(GameObject target, PlayerController player) =>
            this.Trigger(target, player, target != null ? target.transform.position : player.transform.position);

        public override void Trigger(GameObject target, PlayerController player, Vector3 hitPoint)
        {
            if (target == null || !target.TryGetComponent<Interactable>(out var interactable) || interactable.OS == null)
            {
                return;
            }

            if (this.IsBlocked(interactable.OS, player.Character.OperatingSystem.BarrierCentre))
            {
                Debug.Log($"Crash blocked on {target.name}", this);
                this.SpawnTag(target, hitPoint, this.blockedTagText, this.blockedTagColour);
                return;
            }

            var enemy = target.GetComponentInParent<EnemyController>();
            if (enemy == null || !enemy.Stun(this.stunDuration))
            {
                Debug.Log($"Crash landed on {target.name} but it can't be stunned", this);
                return;
            }

            Debug.Log($"Crash stunned {target.name} for {this.stunDuration}s", this);
            HackImpact.Spawn(this.impact, hitPoint, this.tagColour, false, hitPoint, hitPoint);
            this.SpawnTag(target, hitPoint, this.tagText, this.tagColour);
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
