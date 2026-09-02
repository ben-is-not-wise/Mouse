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
        EnemyController SpawnRandom(Spawn spawn, int difficulty);
        void UpdateAllBehaviour();
        void UpdateAllFixedBehaviour(IPlayerController player);
        void UpdateAllLateBehaviour();
        void StopAll();
    }

    public class EnemyManager : AutoSingleton<EnemyManager>, IEnemyManager
    {
        private readonly List<EnemyController> pool = new(100);

        public void Reset()
        {
            for(int i = 0; i < this.transform.childCount; i++)
            {
                this.transform.GetChild(i).gameObject.SetActive(false);
                Destroy(this.transform.GetChild(i).gameObject);
            }

            pool.Clear();
        }

        public EnemyController SpawnChase(Spawn spawn, int difficulty)
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

        public EnemyController SpawnRandom(Spawn spawn, int difficulty)
        {
            Debug.Log("Spawning enemy at " + spawn.transform.position);

            var prefab = spawn.GetRandomEnemyPrefab(difficulty);

            var enemy = FindInactiveEnemyForSpawn(prefab);

            if(enemy == null)
            {
                enemy = InstantiateNewEnemy(prefab, spawn, difficulty);
                pool.Add(enemy);
            }

            enemy.Spawn(spawn.transform.position);

            return enemy;
        }

        public EnemyController InstantiateChaseEnemy(EnemyController prefab, Spawn spawn)
        {
            var enemy = Instantiate(prefab, spawn.transform.position + (Vector3)prefab.EnemySettings.SpawnOffset, Quaternion.identity, this.transform);
            enemy.gameObject.ClearCloneSuffix();
            return enemy;
        }

        public EnemyController InstantiateNewEnemy(EnemyController prefab, Spawn spawn, int difficulty)
        {
            if (prefab == null)
            {
                Debug.LogError("Could not find a prefab to spawn", this);
                return null;
            }           

            var enemy = Instantiate(prefab, spawn.transform.position + (Vector3)prefab.EnemySettings.SpawnOffset, Quaternion.identity, this.transform);
            enemy.gameObject.ClearCloneSuffix();
            return enemy;
        }

        public EnemyController FindInactiveEnemyForSpawn(EnemyController prefab) =>
            pool.Where(e => !e.gameObject.activeInHierarchy && e.EnemyType == prefab.EnemyType).OrderBy(_ => Random.value).FirstOrDefault();
        
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
