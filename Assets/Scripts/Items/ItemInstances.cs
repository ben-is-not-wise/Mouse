using System;
using UnityEngine;

namespace HackedDesign
{
    [Serializable]
    public abstract class ItemInstance
    {
        public int seed;
        public RolledStats stats = new();

        protected ItemInstance(Item definition, int? seed = null)
        {
            this.seed = seed ?? Guid.NewGuid().GetHashCode();
            stats.Roll(definition != null ? definition.RandomStats : null, new System.Random(this.seed));
        }
    }

    [Serializable]
    public class HackInstance : ItemInstance
    {
        public Hack definition;
        public Subroutine[] installed;
        [NonSerialized] public float readyTime = 0;

        public HackInstance(Hack definition) : base(definition)
        {
            this.definition = definition;
            this.installed = new Subroutine[definition != null ? definition.subroutineSlots : 0];
        }
    }

    [Serializable]
    public class UpgradeInstance : ItemInstance
    {
        public Upgrade definition;

        public UpgradeInstance(Upgrade definition) : base(definition)
        {
            this.definition = definition;
        }
    }

    [Serializable]
    public class BarrierEdge
    {
        public EdgeColour colour;
        public bool broken;
        public float brokenAt;
        public float regrowStartAt;
        public float regrowAt;

        /// <summary>0 while broken and waiting, rising to 1 as the edge refills. -1 while intact.</summary>
        public float RegrowProgress(float time)
        {
            if (!broken)
            {
                return -1f;
            }
            float span = regrowAt - regrowStartAt;
            return span <= 0f ? (time >= regrowAt ? 1f : 0f) : Mathf.Clamp01((time - regrowStartAt) / span);
        }
    }

    [Serializable]
    public class BarrierInstance : ItemInstance
    {
        private const int MinFaces = 3;
        private const int MaxFaces = 12;

        public Barrier definition;
        public Subroutine[] installed;
        [NonSerialized] private BarrierEdge[] edges;

        public BarrierInstance(Barrier definition) : base(definition)
        {
            this.definition = definition;
            this.installed = new Subroutine[definition != null ? definition.subroutineSlots : 0];
        }

        // Built on first use, so it also works for an instance restored from a save.
        public BarrierEdge[] Edges => edges ??= BuildEdges();

        public int Faces => Edges.Length;

        /// <summary>Breaks an edge: it stays open for `delay` seconds, then refills over `duration` seconds.</summary>
        public bool BreakEdge(int index, float delay, float duration)
        {
            var all = Edges;
            if (index < 0 || index >= all.Length || all[index].broken)
            {
                return false;
            }
            all[index].broken = true;
            all[index].brokenAt = Time.time;
            all[index].regrowStartAt = Time.time + delay;
            all[index].regrowAt = Time.time + delay + duration;
            return true;
        }

        public void Tick(float time)
        {
            foreach (var edge in Edges)
            {
                if (edge.broken && time >= edge.regrowAt)
                {
                    edge.broken = false;
                }
            }
        }

        // Face count comes from the BarrierFaces stat when rolled, otherwise from the layout length.
        // The layout repeats around the barrier if it is shorter than the face count.
        private BarrierEdge[] BuildEdges()
        {
            var layout = definition != null ? definition.EdgeLayout : null;
            int layoutCount = layout != null && layout.Count > 0 ? layout.Count : MinFaces;
            int faces = Mathf.Clamp(Mathf.RoundToInt(stats.Get(ItemStatKeys.BarrierFaces, layoutCount)), MinFaces, MaxFaces);

            var result = new BarrierEdge[faces];
            for (int i = 0; i < faces; i++)
            {
                result[i] = new BarrierEdge
                {
                    colour = layout != null && layout.Count > 0 ? layout[i % layout.Count] : EdgeColour.Red
                };
            }
            return result;
        }
    }

    [Serializable]
    public class WeaponInstance : ItemInstance
    {
        public const int ModSlots = 1;

        public WeaponSettings definition;
        public WeaponMod[] installed;

        public WeaponInstance(WeaponSettings definition) : base(definition)
        {
            this.definition = definition;
            this.installed = new WeaponMod[ModSlots];
        }

        public int ShootDamage => RollBetween(ItemStatKeys.PhysicalShootDamageMin, ItemStatKeys.PhysicalShootDamageMax);

        public int MeleeDamage => RollBetween(ItemStatKeys.PhysicalMeleeDamageMin, ItemStatKeys.PhysicalMeleeDamageMax);

        private int RollBetween(string minKey, string maxKey)
        {
            int lo = Mathf.RoundToInt(stats.Get(minKey));
            int hi = Mathf.RoundToInt(stats.Get(maxKey));
            return UnityEngine.Random.Range(Mathf.Min(lo, hi), Mathf.Max(lo, hi) + 1);
        }
    }
}
