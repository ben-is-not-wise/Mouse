
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public class Spawn : MonoBehaviour
    {
        [SerializeField] private List<EnemyController> enemyPrefabs = new();
        [SerializeField] private List<Trap> traps = new();
        [SerializeField] private List<GameObject> props = new();
        [SerializeField] private bool mustSpawn = false;

        public bool MustSpawn => mustSpawn;

        public bool CanSpawnEnemy(EnemyType enemyType) => enemyPrefabs.Any(e => e.EnemyType == enemyType);

        public EnemyController GetRandomEnemyPrefab() => enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        public bool CanSpawnProp(string name) => props.Any(prop => prop.name == name);

        public GameObject GetRandomProp() => props.Count == 0 ? null : props[Random.Range(0, props.Count)];

        public Trap GetRandomTrap() => traps.Count == 0 ? null : traps[Random.Range(0, traps.Count)];
    }
}
