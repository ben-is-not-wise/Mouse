using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public class FXPool:AutoSingleton<FXPool>
    {
        [SerializeField] private List<FX> fxPrefabs;
        
        private readonly Dictionary<FXType,List<FX>> fxPool = new();

        protected override void Awake() => base.Awake();

        public void DespawnAll()
        {
            foreach (var type in fxPool.Values)
            {
                foreach(var fx in type)
                {
                    fx.Despawn();
                }
            }
            fxPool.Clear();
        }

        public void Spawn(FXType type, Vector3 position, Vector3 direction)
        {

            if(!fxPool.TryGetValue(type, out var fxList))
            {
                fxList = new List<FX>();
                fxPool[type] = fxList;
            }

            var fx = fxList.FirstOrDefault(f => !f.Playing && !f.isActiveAndEnabled);

            if (fx == null)
            {
                var prefab = fxPrefabs.FirstOrDefault(x => x.FxType == type);

                if (prefab == null)
                {
                    Debug.LogWarning($"Prefab not found for FXType {type}");
                    return;
                }

                fx = Instantiate(prefab, this.transform);

                fxPool[type].Add(fx);
            }

            fx.Spawn(position, direction);
        }
    }
}
