using UnityEngine;

namespace HackedDesign
{
    // Drives the full screen DamageSplit shader from Mouse's health. Three stages: healthy
    // (no effect), hurt (split creeps in from the edges) and critical (split reaches further,
    // rows tear, edges pulse). Every hit also kicks a short pulse, even when healthy.
    // Needs a Full Screen Pass Renderer Feature on the renderer using the DamageSplit material.
    public class DamageEffect : MonoBehaviour
    {
        private static readonly int DamageId = Shader.PropertyToID("_Damage");
        private static readonly int PulseId = Shader.PropertyToID("_DamagePulse");

        [SerializeField] private OperatingSystem os;

        [Header("Stages (fraction of max health)")]
        [SerializeField, Range(0f, 1f)] private float hurtBelow = 0.5f;
        [SerializeField, Range(0f, 1f)] private float criticalBelow = 0.2f;
        [SerializeField, Range(0f, 1f)] private float hurtAmount = 0.5f;
        [SerializeField, Range(0f, 1f)] private float criticalAmount = 1f;

        [Header("Timing")]
        [Tooltip("How fast the effect moves between stages, per second.")]
        [SerializeField] private float blendSpeed = 2f;
        [SerializeField] private float hitPulse = 1f;
        [Tooltip("How fast the hit pulse fades, per second.")]
        [SerializeField] private float pulseDecay = 5f;

        private float current;
        private float target;
        private float pulse;

        void OnEnable()
        {
            if (this.os != null)
            {
                this.os.changeActions += Refresh;
                this.os.hitActions += Hit;
            }
            Refresh();
            this.current = this.target;
            Apply();
        }

        void OnDisable()
        {
            if (this.os != null)
            {
                this.os.changeActions -= Refresh;
                this.os.hitActions -= Hit;
            }
            Shader.SetGlobalFloat(DamageId, 0f);
            Shader.SetGlobalFloat(PulseId, 0f);
        }

        void Update()
        {
            var dt = Time.unscaledDeltaTime;
            this.current = Mathf.MoveTowards(this.current, this.target, this.blendSpeed * dt);
            this.pulse = Mathf.MoveTowards(this.pulse, 0f, this.pulseDecay * dt);
            Apply();
        }

        private void Refresh()
        {
            if (this.os == null || this.os.MaxHealth <= 0)
            {
                this.target = 0f;
                return;
            }

            var ratio = this.os.Health / this.os.MaxHealth;
            this.target = ratio <= this.criticalBelow ? this.criticalAmount
                        : ratio <= this.hurtBelow ? this.hurtAmount
                        : 0f;
        }

        private void Hit()
        {
            this.pulse = this.hitPulse;
        }

        private void Apply()
        {
            Shader.SetGlobalFloat(DamageId, this.current);
            Shader.SetGlobalFloat(PulseId, this.pulse);
        }
    }
}
