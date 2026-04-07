using System;

using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "EnemySettings", menuName = "Mouse/Settings/Enemy")]
    public class EnemySettings : ScriptableObject
    {
        [SerializeField] private float reactionTime = 1.5f;
        [SerializeField] private float recognitionTime = 0.66f;
        [SerializeField] private bool aggressive = true;
        [SerializeField] private bool stationary = false;
        [SerializeField] private float giveUpTime = 10f;
        [SerializeField] private float maxVisualRange = 30f;
        [SerializeField] private float minRoamTime = 1f;
        [SerializeField] private float maxRoamTime = 10.0f;
        [SerializeField] private float roamTime = 7f;
        [field: SerializeField] public Vector2 SpawnOffset {  get; set; }
        
        public float ReactionTime { get => reactionTime;  }
        public bool Aggressive { get => aggressive;  }
        public float GiveUpTime { get => giveUpTime;  }
        public float MaxVisualRange { get => maxVisualRange;  }
        public float RecognitionTime { get => recognitionTime;  }
        public float MinRoamTime { get => minRoamTime; }
        public float MaxRoamTime { get => maxRoamTime;  }
        public float RoamTime { get => roamTime; }
        public bool Stationary { get => this.stationary;  }

    }
}
