
using DunGen;
using log4net.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public interface ILevelManager
    {
        void Generate(int level, int length, int difficulty);
        Vector3 GetLevelPlayerSpawnLocation();
        List<Spawn> GetSpawnLocationsOnLevel();
        List<Spawn> GetSpawnLocationsOnLevel(EnemyType type);

        void RainOn();
        void RainOff();
        void Clear();

        void Reset();

        ICutscene ShowCutscene(string name, bool cityBg, bool rain, IPlayerController player);
        void SpawnEnemies(int count);
        ICutscene ShowCutscene(string name, bool randomLevel, int level, int length, int difficulty, bool cityBg, bool rain, IPlayerController player);
    }
    public class Level : AutoSingleton<Level>, ILevelManager
    {
        [SerializeField] EnemyManager enemyManager;
        [Header("Environment")]
        [SerializeField] Transform sky;
        [SerializeField] Transform smog;
        [SerializeField] Transform rain;
        [SerializeField] List<RuntimeDungeon> backgroundLevels;
        [Header("Dungeon")]
        [SerializeField] RuntimeDungeon runtimeDungeon;
        [Header("Prefabs")]
        [SerializeField] List<Transform> namedRooms;
        [SerializeField] List<GameObject> namedRoomPrefabs;
        [SerializeField] GameObject levelStartPrefab;

        [Header("Background")]
        [SerializeField] List<GameObject> bg1Prefabs = new List<GameObject>();
        [SerializeField] Transform bg1Parent;
        [SerializeField] List<GameObject> bg2Prefabs = new List<GameObject>();
        [SerializeField] Transform bg2Parent;
        [SerializeField] List<GameObject> bg3Prefabs = new List<GameObject>();
        [SerializeField] Transform bg3Parent;

        [Header("Buildings")]
        [SerializeField] Transform parent;
        [SerializeField] Transform propsParent;
        [SerializeField] List<GameObject> startBuildings = new List<GameObject>();
        [SerializeField] List<GameObject> midBuildings = new List<GameObject>();
        [SerializeField] List<GameObject> endBuildings = new List<GameObject>();
        [Header("Settings")]
        [SerializeField] int propCount = 50;

        private GameObject namedRoom;
        private GameObject levelStart;

        public bool LevelComplete { get; set; }

        void Start()
        {
            Clear();
        }

        public void Clear()
        {
            LevelComplete = false;
            if (this.namedRoom)
            {
                this.namedRoom.SetActive(false);
                Destroy(this.namedRoom);
            }

            if (this.levelStart != null)
            {
                this.levelStart.SetActive(false);
                Destroy(this.levelStart);
            }

            //backgroundLevels.ForEach(x => x.Generator.Clear(true));

            //runtimeDungeon.Generator.ClearAllDungeons(true);

            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        public void Reset()
        {
            enemyManager.Reset();
        }

        public void RainOff() => this.rain.gameObject.SetActive(false);
        public void RainOn() => this.rain.gameObject.SetActive(true);

        public ICutscene ShowCutscene(string name, bool randomLevel, int level, int length, int difficulty, bool cityBg, bool rain, IPlayerController player)
        {
            Clear();
            Random.InitState(level);

            if (cityBg)
            {
                SpawnBG1();
                SpawnBG2();
                SpawnBG3();
            }

            var room = namedRoomPrefabs.First(x => x.name == name);
            this.namedRoom = Instantiate(room, this.transform);
            this.namedRoom.SetActive(true);

            var buildingX = this.namedRoom.GetWorldBounds().max.x;

            for (int i = 1; i <= length; i++)
            {
                buildingX = SpawnBuilding(midBuildings[Random.Range(0, midBuildings.Count)], buildingX, difficulty);
            }

            SpawnBuilding(endBuildings[Random.Range(0, endBuildings.Count)], buildingX, difficulty);

            ClearProps();
            SpawnProps(propCount);
            SpawnTraps(5 * difficulty);


            var spawn = this.namedRoom.transform.Find("Spawn");
            if (spawn)
            {
                player.Teleport(this.namedRoom.transform.Find("Spawn").transform.position);
            }

            this.rain.gameObject.SetActive(rain);

            return this.namedRoom.GetComponent<ICutscene>();


        }

        public ICutscene ShowCutscene(string name, bool cityBg, bool rain, IPlayerController player)
        {
            Clear();
            Random.InitState(1);
            

            if (cityBg)
            {
                SpawnBG1();
                SpawnBG2();
                SpawnBG3();
                //backgroundLevels.ForEach(bg =>
                //{
                //    try
                //    {
                //        bg.Generator.Seed = 50;
                //        bg.Generate();
                //    }
                //    catch
                //    {
                //        Debug.LogWarning("Failed to generate bg level");
                //    }
                //}
                //);
            }

            var room = namedRoomPrefabs.First(x => x.name == name);

            this.namedRoom = Instantiate(room, this.transform);
            this.namedRoom.SetActive(true);

            var buildingX = this.namedRoom.GetWorldBounds().max.x;




            var spawn = this.namedRoom.transform.Find("Spawn");
            if (spawn)
            {
                player.Teleport(this.namedRoom.transform.Find("Spawn").transform.position);
            }

            this.rain.gameObject.SetActive(rain);

            return this.namedRoom.GetComponent<ICutscene>();
        }

        public void Generate(int level, int length, int difficulty)
        {
            Random.InitState(level);
            //Random.seed = level;

            SpawnBG1();
            SpawnBG2();
            SpawnBG3();

            //backgroundLevels.ForEach(bg =>
            //{
            //    try
            //    {
            //        bg.Generator.Seed = level;
            //        bg.Generate();
            //    }
            //    catch
            //    {
            //        Debug.LogWarning("Failed to generate bg level");
            //    }
            //});

            var buildingX = SpawnBuilding(startBuildings[Random.Range(0, startBuildings.Count)], 0, difficulty);

            for (int i = 1; i <= length; i++)
            {
                buildingX = SpawnBuilding(midBuildings[Random.Range(0, midBuildings.Count)], buildingX, difficulty);
            }

            SpawnBuilding(endBuildings[Random.Range(0, endBuildings.Count)], buildingX, difficulty);

            ClearProps();
            SpawnProps(propCount);
            SpawnTraps(5 * difficulty);

            //ElevatorManager.Instance.Refresh();
        }

        private void SpawnBG1() => SpawnBG(bg1Prefabs, bg1Parent);

        private void SpawnBG2() => SpawnBG(bg2Prefabs, bg2Parent);

        private void SpawnBG3() => SpawnBG(bg3Prefabs, bg3Parent);

        private void SpawnBG(List<GameObject> prefabs, Transform parent)
        {
            float xPosition = 0;
            for (int i = 0; i < 20; i++)
            {
                var prefab = prefabs[Random.Range(0, prefabs.Count)];
                var bg = Instantiate(prefab, new Vector3(xPosition, Random.Range(-3f, 3f) + parent.transform.position.y, 0), Quaternion.identity, parent);
                xPosition = bg.GetWorldBounds().max.x + Random.Range(0f, 5f);
            }
        }

        private float SpawnBuilding(GameObject prefab, float xPosition, int difficulty)
        {
            var building = Instantiate(prefab, new Vector3(xPosition + BuildingDistance(difficulty, xPosition), BuildingHeight(difficulty), 0), Quaternion.identity, parent);
            return building.GetWorldBounds().max.x;
        }

        private void ClearProps()
        {
            for (int i = 0; i < propsParent.childCount; i++)
            {
                var child = propsParent.GetChild(i);
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        private void SpawnProps(int count)
        {
            var locations = GetSpawnLocationsOnLevel();

            for (int i = 0; i < count; i++)
            {
                if(locations.Count <= i)
                {
                    return;
                }

                var propPrefab = locations[i].GetRandomProp();

                if (propPrefab != null) 
                {
                    Instantiate(propPrefab, locations[i].transform.position, Quaternion.identity, propsParent);
                }
            }
        }

        private void SpawnTraps(int count)
        {
            var locations = GetSpawnLocationsOnLevel();

            for (int i = 0; i < count; i++)
            {
                if (locations.Count <= i)
                {
                    return;
                }

                var trapPrefab = locations[i].GetRandomTrap();

                if (trapPrefab != null) 
                {
                    Instantiate(trapPrefab, locations[i].transform.position, Quaternion.identity, propsParent);
                }
            }
        }

        public void SpawnEnemies(int count)
        {
            enemyManager.Reset();
            var spawns = GetSpawnLocationsOnLevel();

            var mustSpawnLocations = spawns.Where(s => s.MustSpawn).ToList();

            Debug.Log("Spawning required " + Mathf.Min(count, mustSpawnLocations.Count) + " enemies");
            for (int i = 0; i < Mathf.Min(count, mustSpawnLocations.Count); i++)
            {
                enemyManager.Spawn(mustSpawnLocations[i]);
            }

            spawns = spawns.Where(s => !s.MustSpawn).ToList();

            Debug.Log("Spawning " + Mathf.Min(count, spawns.Count) + " enemies");

            for (int i = 0; i < Mathf.Min(count, spawns.Count); i++)
            {
                enemyManager.Spawn(spawns[i]);
            }
        }

        private float BuildingHeight(int difficulty) => Random.Range(-2, 2) * difficulty;
        private float BuildingDistance(int difficulty, float xPosition) => 1 + Random.Range(2f + (xPosition / 200), 6f + (xPosition / 200)) * difficulty;

        public Vector3 GetLevelPlayerSpawnLocation()
        {
            var spawn = GameObject.FindGameObjectWithTag("Respawn");

            return spawn != null ? spawn.transform.position : Vector3.zero;
        }

        public List<Spawn> GetSpawnLocationsOnLevel() => FindObjectsByType<Spawn>(FindObjectsInactive.Exclude).OrderBy(_ => Random.value).ToList();
        public List<Spawn> GetSpawnLocationsOnLevel(EnemyType type) => FindObjectsByType<Spawn>(FindObjectsInactive.Exclude).Where(x => x.CanSpawnEnemy(type)).OrderBy(_ => Random.value).ToList();

        //public List<Spawn> GetPropSpawnLocationsOnLevel() => FindObjectsByType<Spawn>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).OrderBy(_ => Random.value).ToList();
    }

    public static class Cutscenes
    {
        public static string Rooftop1 = "Act 0 Rooftop 1";
        public static string Rooftop2 = "Act 0 Rooftop 2";
        public static string Rooftop3 = "Act 0 Rooftop 3";
        public static string MouseStartingRoom1 = "Act 0 Mouse Starting Room 1";
        public static string MouseStartingRoom2 = "Act 0 Mouse Starting Room 2";
    }
}