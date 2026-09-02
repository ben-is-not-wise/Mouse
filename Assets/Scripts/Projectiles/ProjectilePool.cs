using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    public class ProjectilePool : MonoBehaviour
    {
        // List of projectile prefabs to be configured in the inspector. Each must have a Projectile component.
        [SerializeField]
        private List<Projectile> prefabs = new List<Projectile>();

        // Map projectile types to their prefab (configured in inspector)
        private readonly Dictionary<Projectile.ProjectileType, Projectile> prefabByType = new Dictionary<Projectile.ProjectileType, Projectile>();

        // Internal pool: for each projectile type we keep a list of instantiated projectiles (both active and inactive).
        private readonly Dictionary<Projectile.ProjectileType, List<Projectile>> pool = new Dictionary<Projectile.ProjectileType, List<Projectile>>();

        public static ProjectilePool Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Build lookup from configured prefabs by their ProjectileType.
            prefabByType.Clear();
            for (int i = 0; i < prefabs.Count; i++)
            {
                Projectile p = prefabs[i];
                if (p == null)
                {
                    continue;
                }

                Projectile.ProjectileType type = p.Type;
                if (!prefabByType.ContainsKey(type))
                {
                    prefabByType[type] = p;
                }
            }
        }

        // Spawn by projectile type
        public Projectile Spawn(Projectile.ProjectileType type, Vector3 position, Vector3 direction, int damage, float force = 0f, bool gravity = false, CharController owner = null)
        {
            if (!prefabByType.TryGetValue(type, out Projectile prefab))
            {
                return null;
            }

            return Spawn(prefab, position, direction, damage, force, gravity, owner);
        }

        // Spawn by prefab reference
        public Projectile Spawn(Projectile prefab, Vector3 position, Vector3 direction, int damage, float force = 0f, bool gravity = false, CharController owner = null)
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = GetPooledInstance(prefab);
            if (instance == null)
            {
                return null;
            }

            instance.transform.position = position;

            if (direction != Vector3.zero)
            {
                // 2D rotation: set z-angle so sprite faces the direction on the XY plane (side-scroller)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                instance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            instance.gameObject.SetActive(true);

            instance.Launch(position, (Vector2)direction.normalized, force, damage, gravity, owner);

            return instance;
        }

        private Projectile GetPooledInstance(Projectile prefab)
        {
            Projectile.ProjectileType type = prefab.Type;

            if (!pool.TryGetValue(type, out var list))
            {
                list = new List<Projectile>();
                pool[type] = list;
            }

            // Try find inactive instance to reuse
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p == null)
                {
                    continue;
                }

                if (!p.gameObject.activeInHierarchy)
                {
                    return p;
                }
            }

            // No inactive instance found: create a new one
            var go = Instantiate(prefab.gameObject, transform);
            var proj = go.GetComponent<Projectile>();
            if (proj == null)
            {
                // Should not happen - prefab must contain Projectile
                Destroy(go);
                return null;
            }

            // Ensure it's initially inactive until Spawn completes
            proj.gameObject.SetActive(false);
            list.Add(proj);
            return proj;
        }
    }
}
