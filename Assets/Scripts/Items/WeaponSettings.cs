using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Mouse/Settings/Weapon")]
    public class WeaponSettings : Item
    {
        public static List<WeaponSettings> AllWeapons { get; set; } = new List<WeaponSettings>();

        public static WeaponSettings FindByName(string name) => AllWeapons.FirstOrDefault(a => a.name == name);

        private void OnEnable() => AllWeapons.Add(this);

        private void OnDisable() => AllWeapons.Remove(this);

        public WeaponType weaponType;
        public int minKineticLevel = 0;
        public string longDescription;
        public float projectileForce = 10f;
    }

    public enum WeaponType
    {
        Unarmed,
        Gun,
        Grenade
    }
}
