
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public class Spawn : MonoBehaviour
    {
        [SerializeField] private List<EnemySpawnSet> difficultyLevels = new();
        [SerializeField] private List<Trap> traps = new();
        [SerializeField] private List<GameObject> props = new();
        [SerializeField] private bool mustSpawn = false;
        [SerializeField] private bool chaseSpawn = false;

        public bool MustSpawn => mustSpawn;
        public bool ChaseSpawn => chaseSpawn;

        public bool CanSpawnEnemy(EnemyType enemyType, int difficulty)
        {
            var set = GetSpawnSet(difficulty);
            return set != null && set.CanSpawnEnemy(enemyType);
        }

        public EnemyController GetRandomEnemyPrefab(int difficulty)
        {
            var set = GetSpawnSet(difficulty);
            return set == null ? null : set.GetRandomEnemyPrefab();
        }

        public EnemyController GetChaseSpawn(int difficulty)
        {
            var set = GetSpawnSet(difficulty);
            return set.GetChaseEnemyPrefab();
        }

        private EnemySpawnSet GetSpawnSet(int difficulty)
        {
            return difficultyLevels.Count == 0 ? null : difficultyLevels[Mathf.Clamp(difficulty, 0, difficultyLevels.Count - 1)];
        }

        public bool CanSpawnProp(string name) => props.Any(prop => prop.name == name);

        public GameObject GetRandomProp() => props.Count == 0 ? null : props[Random.Range(0, props.Count)];

        public Trap GetRandomTrap() => traps.Count == 0 ? null : traps[Random.Range(0, traps.Count)];
    }
}
