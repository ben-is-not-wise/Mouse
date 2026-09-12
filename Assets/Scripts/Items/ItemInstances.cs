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
    public class BarrierInstance : ItemInstance
    {
        public Barrier definition;
        public Subroutine[] installed;

        public BarrierInstance(Barrier definition) : base(definition)
        {
            this.definition = definition;
            this.installed = new Subroutine[definition != null ? definition.subroutineSlots : 0];
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

    public enum WeaponSlotId
    {
        Primary,
        Grenade,
        Melee
    }
}
