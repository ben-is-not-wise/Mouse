


using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HackedDesign
{
    [System.Serializable]
    public class GameData
    {
        public List<string> GameFlags = new();

        public void AddFlag(string flag)
        {
            if (!Game.Instance.GameData.GameFlags.Contains(flag))
            {
                Game.Instance.GameData.GameFlags.Add(flag);
            }
        }

        [SerializeField] private List<DiscoveredInfo> discoveredInfo = new();

        public void Discover(InfoCategory category, string name)
        {
            if (!IsDiscovered(category, name))
            {
                this.discoveredInfo.Add(new DiscoveredInfo(category, name));
            }
        }

        public void Discover(Item item) => Discover(item.InfoCategory, item.name);

        public bool IsDiscovered(Item item) => IsDiscovered(item.InfoCategory, item.name);

        public bool IsDiscovered(InfoCategory category, string name) => this.discoveredInfo.Any(info => info.Category == category && info.Name == name);

        public IEnumerable<string> GetDiscovered(InfoCategory category) => this.discoveredInfo.Where(info => info.Category == category).Select(info => info.Name);

        public List<MissionData> completedMissions = new();
        //public List<MissionData> availableMissions = new();
        private MissionData currentMission;


        public MissionData CurrentMission { get => currentMission; private set => currentMission = value; }
        public bool FinishedTutorial { get; set; }

        public void Reset()
        {
            this.discoveredInfo.Clear();
            CurrentMission = new MissionData()
            {
                seed = 1,
                corp = "Ranbow",
                missionType = MissionType.Infiltrate
            }; 
        }

        private void AddMission(int seed, string corp, MissionType type)
        {


            //availableMissions.Add(firstMission);
        }

        /*
        public void SelectMission(int seed)
        {
            Debug.Log("Select mission " + seed);
            var found = availableMissions.Find(x => x.seed == seed);
            if(found != null)
            {
                CurrentMission = found;
            }
        }*/
    }


    [System.Serializable]
    public struct DiscoveredInfo
    {
        [SerializeField] private InfoCategory category;
        [SerializeField] private string name;

        public DiscoveredInfo(InfoCategory category, string name)
        {
            this.category = category;
            this.name = name;
        }

        public InfoCategory Category => category;
        public string Name => name;
    }

    [System.Serializable]
    public class MissionData
    {
        public int seed;
        public string corp;
        public MissionType missionType;
    }
}
