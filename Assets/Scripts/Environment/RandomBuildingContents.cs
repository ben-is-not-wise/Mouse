using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    public class RandomBuildingContents : MonoBehaviour
    {
        [SerializeField] private List<GameObject> contents;

        void Awake()
        {
            foreach(var x in contents)
            {
                x.SetActive(false);
            }

            if (contents.Count > 0)
            {
                contents[Random.Range(0, contents.Count)].SetActive(true);
            }
        }
    }
}