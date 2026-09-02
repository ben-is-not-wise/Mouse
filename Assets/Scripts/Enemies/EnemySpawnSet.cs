using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "EnemySpawnSet", menuName = "Mouse/Spawns/Enemy Spawn Set")]
    public class EnemySpawnSet : ScriptableObject
    {
        [SerializeField] private List<EnemyController> enemyPrefabs = new();

        [SerializeField] private EnemyController chasePrefab;

        public bool CanSpawnEnemy(EnemyType enemyType) => enemyPrefabs.Any(e => e.EnemyType == enemyType);

        public EnemyController GetRandomEnemyPrefab() => enemyPrefabs.Count == 0 ? null : enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        public EnemyController GetChaseEnemyPrefab() => chasePrefab;
    }
}
