using System;

namespace HackedDesign
{
    [Serializable]
    public class HackInstance
    {
        public Hack definition;
        public Subroutine[] installed;

        public HackInstance(Hack definition)
        {
            this.definition = definition;
            this.installed = new Subroutine[definition != null ? definition.subroutineSlots : 0];
        }
    }

    [Serializable]
    public class UpgradeInstance
    {
        public Upgrade definition;

        public UpgradeInstance(Upgrade definition)
        {
            this.definition = definition;
        }
    }

    [Serializable]
    public class WeaponInstance
    {
        public const int ModSlots = 1;

        public WeaponSettings definition;
        public WeaponMod[] installed;

        public WeaponInstance(WeaponSettings definition)
        {
            this.definition = definition;
            this.installed = new WeaponMod[ModSlots];
        }
    }

    public enum WeaponSlotId
    {
        Primary,
        Grenade,
        Melee
    }
}
