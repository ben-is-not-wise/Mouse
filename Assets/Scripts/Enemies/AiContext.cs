#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public struct AiContext
    {
        public string name;
        public Vector3 position;
        public bool canSeePlayer;
        public bool canHearPlayer;
        public bool hasSeenPlayer;
        public bool playerInFrontOfUs;
        public bool hasSeenDeadEnemies;
        public int facing;
        public Vector3 lastKnownPlayerPosition;
        public bool wallInFront;
        public bool dropInFront;
        public float groundDistance;
        public LayerMask movementMask;
        public int bullets;
        public bool flying;
        public EnemySettings settings;
    }
}
