using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign.UI.DamageNumbers
{
    public class DamageNumberPool : AutoSingleton<DamageNumberPool>
    {
        [SerializeField] private DamageNumber damageNumberPrefab;
        [SerializeField] private GameSettings settings;

        private readonly List<DamageNumber> pool = new List<DamageNumber>();

        public void Spawn(int number, Vector3 start)
        {
            if (settings.ShowDamageNumbers)
            {
                var dn = GetPooledInstance();
                dn.Show(number, start);
            }
        }

        private DamageNumber GetPooledInstance()
        {
            foreach(var x in pool)
            {
                if(!x.gameObject.activeInHierarchy)
                {
                    return x;
                }
            }

            var go = Instantiate(damageNumberPrefab, transform);

            if (!go.TryGetComponent<DamageNumber>(out var dn))
            {
                // Should not happen
                Destroy(go);
                return null;
            }

            pool.Add(dn);

            return dn;
        }
    }
}
