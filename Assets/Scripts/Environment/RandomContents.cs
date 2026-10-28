
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    [ExecuteAlways]
    public class RandomContents : MonoBehaviour
    {
        [SerializeField] private float changeToPickSomething = 1.0f;
        //[SerializeField] private List<GameObject> randomPrefabs = new();
        [SerializeField] private List<WeightedPrefab> weightedPrefabs = new();

        private GameObject preview;

        void Awake()
        {
            if (Application.isPlaying)
            {
                PickPrefab();
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                RerollPreview();
            }
        }

        void OnDisable() => ClearPreview();

        [ContextMenu("Reroll Preview")]
        private void RerollPreview()
        {
            ClearPreview();
            if (this.gameObject.scene.IsValid())
            {
                this.preview = PickPrefab();
                if (this.preview != null)
                {
                    this.preview.hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        private void ClearPreview()
        {
            if (this.preview != null)
            {
                DestroyImmediate(this.preview);
            }
            this.preview = null;
        }

        private GameObject PickPrefab()
        {
            //if (randomPrefabs.Count == 0)
            //{
            //    return;
            //}

            if (Random.value < changeToPickSomething)
            {
                var prefab = GetRandomPrefab();
                if (prefab == null)
                {
                    return null;
                }
                var gameObj = Instantiate(prefab, transform.position, Quaternion.identity, this.transform);
                gameObj.ClearCloneSuffix();
                return gameObj;
            }
            return null;
        }

        public GameObject GetRandomPrefab()
        {
            if (weightedPrefabs == null || weightedPrefabs.Count == 0)
            {
                Debug.LogError("Invalid weighted prefabs list");
                return null;
            }

            float totalWeight = 0f;
            foreach (var item in weightedPrefabs)
            {
                totalWeight += item.weight;
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);

            float runningTotal = 0f;
            foreach (var item in weightedPrefabs)
            {
                runningTotal += item.weight;
                if (randomValue <= runningTotal)
                {
                    return item.prefab;
                }
            }

            return weightedPrefabs[weightedPrefabs.Count - 1].prefab;

        }
    }

    [System.Serializable]
    public class WeightedPrefab
    {
        public float weight = 1;
        public GameObject prefab;
    }
}
