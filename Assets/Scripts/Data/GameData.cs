


using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HackedDesign
{
    [System.Serializable]
    public class GameData
    {
        public HashSet<string> GameFlags = new();

        public List<MissionData> completedMissions = new();
        //public List<MissionData> availableMissions = new();
        private MissionData currentMission;

        public MissionData CurrentMission { get => currentMission; private set => currentMission = value; }

        public void Reset()
        {
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

    

    public class MissionData
    {
        public int seed;
        public string corp;
        public MissionType missionType;
    }
}
