using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    [System.Serializable]
    public class CutscenePhase
    {
        public string name;
        public GameObject[] activeObjects;
        public UnityEvent onEnter;
        public string dialogKey;
    }
}
