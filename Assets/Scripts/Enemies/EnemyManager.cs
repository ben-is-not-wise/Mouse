using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace HackedDesign
{
    public interface IEnemyManager
    {
        void Reset();
        EnemyController SpawnRandom(EnemySpawn spawn, int difficulty);
        void UpdateAllBehaviour();
        void UpdateAllFixedBehaviour(IPlayerController player);
        void UpdateAllLateBehaviour();
        void StopAll();
    }

    public class EnemyManager : AutoSingleton<EnemyManager>, IEnemyManager
    {
        [SerializeField] private Transform outsideParent;
        private readonly List<EnemyController> pool = new(100);

        public Transform OutsideParent => this.outsideParent ?? this.transform;

        public void Reset()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] == null)
                {
                    continue;
                }

                pool[i].gameObject.SetActive(false);
                Destroy(pool[i].gameObject);
            }

            pool.Clear();

            for (int i = 0; i < OutsideParent.childCount; i++)
            {
                OutsideParent.GetChild(i).gameObject.SetActive(false);
                Destroy(OutsideParent.GetChild(i).gameObject);
            }

            SetOutside(true);
        }

        public EnemyController SpawnChase(EnemySpawn spawn, int difficulty)
        {
            Debug.Log("Spawning chase enemy at " + spawn.transform.position);

            var enemyPrefab = spawn.GetChaseSpawn(difficulty);

            Debug.Log("spawning " + enemyPrefab.EnemyType);

            var enemy = pool.Where(e => e.EnemyType == enemyPrefab.EnemyType).OrderBy(_ => Random.value).FirstOrDefault();

            if (enemy == null)
            {
                enemy = InstantiateChaseEnemy(enemyPrefab, spawn);
                //enemy = InstantiateNewEnemy(spawn, difficulty);
                pool.Add(enemy);
            }

            enemy.Spawn(spawn.transform.position);

            return enemy;

        }

        public EnemyController SpawnRandom(EnemySpawn spawn, int difficulty)
        {
            Debug.Log("Spawning enemy at " + spawn.transform.position);

            var prefab = spawn.GetRandomEnemyPrefab(difficulty);

            if (prefab == null)
            {
                Debug.LogWarning("No enemy prefab for difficulty " + difficulty, spawn);
                return null;
            }

            var enemy = FindInactiveEnemyForSpawn(prefab);

            if (enemy == null)
            {
                enemy = InstantiateNewEnemy(prefab, spawn, difficulty);
                pool.Add(enemy);
            }

            enemy.Spawn(spawn.transform.position);

            return enemy;
        }

        public EnemyController InstantiateChaseEnemy(EnemyController prefab, EnemySpawn spawn)
        {
            var enemy = Instantiate(prefab, spawn.transform.position + (Vector3)prefab.EnemySettings.SpawnOffset, Quaternion.identity, OutsideParent);
            enemy.gameObject.ClearCloneSuffix();
            return enemy;
        }

        public EnemyController InstantiateNewEnemy(EnemyController prefab, EnemySpawn spawn, int difficulty)
        {
            if (prefab == null)
            {
                Debug.LogError("Could not find a prefab to spawn", this);
                return null;
            }

            var enemy = Instantiate(prefab, spawn.transform.position + (Vector3)prefab.EnemySettings.SpawnOffset, Quaternion.identity, spawn.Parent);
            enemy.gameObject.ClearCloneSuffix();
            return enemy;
        }

        public EnemyController FindInactiveEnemyForSpawn(EnemyController prefab) =>
            pool.Where(e => !e.gameObject.activeSelf && e.EnemyType == prefab.EnemyType).OrderBy(_ => Random.value).FirstOrDefault();

        public void Remove(EnemyController enemy) => pool.Remove(enemy);

        public void SetOutside(bool flag)
        {
            OutsideParent.gameObject.SetActive(flag);
        }

        public void UpdateAllBehaviour()
        {
            foreach (var enemy in pool.Where(e => e.gameObject.activeInHierarchy))
            {
                enemy.UpdateBehaviour();
            }
        }

        public void UpdateAllFixedBehaviour(IPlayerController player)
        {
            foreach (var enemy in pool.Where(e => e.gameObject.activeInHierarchy))
            {
                enemy.FixedUpdateBehaviour(player);
            }
        }

        public void UpdateAllLateBehaviour()
        {
            foreach (var enemy in pool.Where(e => e.gameObject.activeInHierarchy))
            {
                enemy.LateUpdateBehaviour();
            }
        }

        public void StopAll()
        {
            foreach (var enemy in pool.Where(e => e.gameObject.activeInHierarchy))
            {
                enemy.Character.ExecuteCommand(new StopCommand());
            }
        }
    }
}
