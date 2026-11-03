using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Hack", menuName = "Mouse/PU/Generic Hack")]
    public abstract class Hack : Item
    {
        [SerializeField] public string shortName;
        [SerializeField] public Sprite buttonIcon;
        [SerializeField] public Sprite puIcon;
        [SerializeField] public float puUsage;
        [SerializeField] public float puTime;
        [SerializeField, Range(1, 3)] public int subroutineSlots = 1;
        [SerializeField] public float range = 10f;
        [Tooltip("Speed of the packet sent down the aim line, in pixels per second. 0 applies the hack instantly.")]
        [SerializeField, Min(0)] private float travelSpeed = 0f;

        public float TravelSpeed => this.travelSpeed;

        public virtual bool RequiresTarget => true;

        /// <summary>Hacks with their own button (e.g. Ping) stay off the action bar.</summary>
        public virtual bool HasOwnButton => false;

        public virtual bool IsBlocked(OperatingSystem os, Vector2 from) => false;
        public abstract void Trigger(GameObject target, PlayerController player);

        /// <summary>Called when the hack's packet lands at hitPoint. Defaults to the plain Trigger.</summary>
        public virtual void Trigger(GameObject target, PlayerController player, Vector3 hitPoint) => this.Trigger(target, player);


    }

    public enum HackResult
    {
        Success,
        NoHack,
        NoTarget,
        OutOfRange,
        NoEnergy,
        CoolingDown
    }
}
