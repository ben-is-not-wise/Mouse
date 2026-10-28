using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "BuildingSet", menuName = "Mouse/Environment/Building Set")]
    public class LevelSet : ScriptableObject
    {
        [SerializeField] private List<GameObject> startBuildings = new();
        [SerializeField] private List<GameObject> midBuildings = new();
        [SerializeField] private List<GameObject> endBuildings = new();
        [SerializeField] private List<GameObject> baseBuildings = new();
        [SerializeField] private int minLength = 20;
        [SerializeField] private int maxLength = 20;

        public int GetRandomLength() => Random.Range(minLength, maxLength + 1);

        public GameObject GetRandomStartBuilding() => GetRandomBuilding(startBuildings);

        public GameObject GetRandomMidBuilding() => GetRandomBuilding(midBuildings);

        public GameObject GetRandomEndBuilding() => GetRandomBuilding(endBuildings);

        public GameObject GetRandomBaseBuilding() => GetRandomBuilding(baseBuildings);

        public GameObject GetRandomBuilding(List<GameObject> set) => set.Count == 0 ? null : set[Random.Range(0, set.Count)];

    }
}
