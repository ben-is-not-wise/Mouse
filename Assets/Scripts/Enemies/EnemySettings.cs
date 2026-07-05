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
        [SerializeField] private float memoryTime = 15f;
        [SerializeField] private float alertRadius = 12f;
        [SerializeField] private float proximityRange = 2.5f;
        [SerializeField] private float aimError = 0.5f;
        [SerializeField] private float targetLeading = 0f;
        [SerializeField] private float maxVisualRange = 30f;
        [SerializeField] private float fieldOfView = 120f;
        [SerializeField] private float minRoamTime = 1f;
        [SerializeField] private float maxRoamTime = 10.0f;
        [SerializeField] private float roamTime = 7f;
        [Header("Flying")]
        [SerializeField] private float hoverHeight = 4f;
        [SerializeField] private float hoverDeadband = 0.5f;
        [SerializeField] private float groundProbeDistance = 8f;
        [Header("Movement")]
        [SerializeField] private bool canJumpGaps = false;
        [field: SerializeField] public Vector2 SpawnOffset {  get; set; }
        
        public float ReactionTime { get => reactionTime;  }
        public bool Aggressive { get => aggressive;  }
        public float GiveUpTime { get => giveUpTime;  }
        public float MemoryTime { get => memoryTime; }
        public float AlertRadius { get => alertRadius; }
        public float ProximityRange { get => proximityRange; }
        public float AimError { get => aimError; }
        public float TargetLeading { get => targetLeading; }
        public float MaxVisualRange { get => maxVisualRange;  }
        public float FieldOfView { get => fieldOfView; }
        public float RecognitionTime { get => recognitionTime;  }
        public float MinRoamTime { get => minRoamTime; }
        public float MaxRoamTime { get => maxRoamTime;  }
        public float RoamTime { get => roamTime; }
        public bool Stationary { get => this.stationary;  }
        public float HoverHeight { get => hoverHeight; }
        public float HoverDeadband { get => hoverDeadband; }
        public float GroundProbeDistance { get => groundProbeDistance; }
        public bool CanJumpGaps { get => canJumpGaps; }

    }
}
