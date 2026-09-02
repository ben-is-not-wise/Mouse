using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "BuildingSet", menuName = "Mouse/Environment/Building Set")]
    public class BuildingSet : ScriptableObject
    {
        [SerializeField] private List<GameObject> startBuildings = new();
        [SerializeField] private List<GameObject> midBuildings = new();
        [SerializeField] private List<GameObject> endBuildings = new();
        [SerializeField] private int minLength = 20;
        [SerializeField] private int maxLength = 20;

        public int GetRandomLength() => Random.Range(minLength, maxLength + 1);

        public GameObject GetRandomStartBuilding() => startBuildings.Count == 0 ? null : startBuildings[Random.Range(0, startBuildings.Count)];

        public GameObject GetRandomMidBuilding() => midBuildings.Count == 0 ? null : midBuildings[Random.Range(0, midBuildings.Count)];

        public GameObject GetRandomEndBuilding() => endBuildings.Count == 0 ? null : endBuildings[Random.Range(0, endBuildings.Count)];
    }
}
