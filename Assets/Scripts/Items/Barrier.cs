using UnityEngine;
using System.Collections.Generic;

namespace HackedDesign
{
    public abstract class Barrier : Item
    {
        [SerializeField, Range(0, 5)] private float radius = 1.5f;
        [SerializeField, Range(1, 3)] public int subroutineSlots = 1;

        public float Radius => this.radius;

        public abstract void Apply(OperatingSystem os);
        public abstract void Remove(OperatingSystem os);
    }
}
