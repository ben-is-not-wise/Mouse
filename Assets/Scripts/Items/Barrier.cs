using UnityEngine;
using System.Collections.Generic;

namespace HackedDesign
{
    public abstract class Barrier : Item
    {
        [SerializeField, Range(0, 5)] private float radius = 1.5f;
        [SerializeField, Range(1, 3)] public int subroutineSlots = 1;
        [SerializeField] private EdgeColour[] edgeLayout = { EdgeColour.Red, EdgeColour.Red, EdgeColour.Red };
        [Tooltip("Seconds a broken edge stays fully open before it starts to refill.")]
        [SerializeField, Min(0)] private float regrowDelay = 5f;
        [Tooltip("Seconds the refill takes once it starts.")]
        [SerializeField, Min(0)] private float regrowTime = 10f;

        public float Radius => this.radius;
        public IReadOnlyList<EdgeColour> EdgeLayout => this.edgeLayout;
        public float RegrowDelay => this.regrowDelay;
        public float RegrowTime => this.regrowTime;

        public abstract void Apply(OperatingSystem os);
        public abstract void Remove(OperatingSystem os);
    }
}
