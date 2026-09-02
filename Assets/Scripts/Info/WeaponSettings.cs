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
        public int minShootDamage = 10;
        public int maxShootDamage = 100;
        public int minMeleeDamage = 10;
        public int maxMeleeDamage = 100;
        public float projectileForce = 10f;

        public int RandomShootDamage => Random.Range(minShootDamage, maxShootDamage + 1);
        public int RandomMeleeDamage => Random.Range(minMeleeDamage, maxMeleeDamage + 1);

        
    }

    public enum WeaponType
    {
        Unarmed,
        Gun,
        Grenade
    }
}
