#nullable enable
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    [RequireComponent(typeof(CharController))]
    public class EnemyController : MonoBehaviour, IAi
    {
        [Header("Actions")]
        [SerializeField, NotNull] private UnityAction hitBehaviour;
        [SerializeField, NotNull] private UnityAction deathBehaviour;
        [Header("Game Objects")]
        [SerializeField, NotNull] private CharController character;
        [SerializeField, NotNull] private StatusIcon characterStatusIcon;
        [SerializeField, NotNull] private Transform? aimPivot = null;
        [Header("Settings")]
        [SerializeField] private EnemyType enemyType;
        [SerializeField] private bool useUtilityBrain = false;
        [SerializeField] private UtilityBrainSettings? brainSettings = null;
        [SerializeField] private LayerMask lineOfSightMask;
        [SerializeField] private LayerMask movementMask;
        [SerializeField, NotNull] private EnemySettings enemySettings;

        private PlayerController? player = null;

        private const float HearingPositionError = 2f;

        private const float AlertBroadcastInterval = 1f;
        private const int SenseInterval = 4;

        private static int senseCounter = 0;
        private readonly int senseOffset = senseCounter++ % SenseInterval;
        private int senseTick = 0;

        private IEnemyState currentState;
        private bool wasHearingPlayer = false;
        private float lastPerceivedTime = 0f;
        private float nextAlertBroadcast = 0f;

        public IEnemyState CurrentState
        {
            get => this.currentState;
            set
            {
                currentState?.End();
                currentState = value;
                currentState?.Begin();
            }
        }

        public StatusIcon Icon => characterStatusIcon;

        public bool HasSeenPlayer
        {
            get; private set;
        }

        public Vector3 LastKnownPlayerPosition { get; private set; }

        public bool CanSeePlayer { get; private set; }

        public bool CanHearPlayer { get; private set; }

        public bool HasBeenAlerted { get; private set; }

        public bool HasSeenDeadEnemies { get; private set; } = false;

        public bool PlayerInFrontOfUs
        {
            get
            {
                var facing = this.player != null ? this.player.transform.position.x <= this.transform.position.x ? -1 : 1 : -1;

                return Mathf.Sign(facing) == Mathf.Sign(transform.right.x);
            }
        }

        public EnemySettings EnemySettings { get => enemySettings; private set => enemySettings = value; }
        public CharController Character { get => character; private set => character = value; }

        public bool WallInFront
        {
            get
            {
                var boxA = new Vector2(transform.position.x + (transform.right.x * 0.25f), transform.position.y + 0.25f);
                var boxB = new Vector2(boxA.x + (transform.right.x * 0.5f), boxA.y + (2f - 0.5f));
#if UNITY_EDITOR
                if (Application.isPlaying && Application.isEditor)
                {
                    Debug.DrawLine(boxA, boxB, Color.green);
                }
#endif
                return Physics2D.OverlapArea(boxA, boxB, movementMask);
            }
        }

        public bool DropInFront
        {
            get
            {
                if (character.Body == null || (character.Body != null && character.Body.Flying))
                {
                    return false;
                }

                var boxA = new Vector2(transform.position.x + (transform.right.x * 0.25f), transform.position.y);
                var boxB = new Vector2(boxA.x + (transform.right.x * 0.5f), boxA.y - 0.25f);
#if UNITY_EDITOR
                if (Application.isPlaying && Application.isEditor)
                {
                    Debug.DrawLine(boxA, boxB, Color.green);
                }
#endif
                return !Physics2D.OverlapArea(boxA, boxB, movementMask);
            }
        }

        public float GroundDistance
        {
            get
            {
                var origin = (Vector2)transform.position;
                var distance = EnemySettings.GroundProbeDistance;
                var hit = Physics2D.Raycast(origin, Vector2.down, distance, movementMask);
#if UNITY_EDITOR
                if (Application.isPlaying && Application.isEditor)
                {
                    Debug.DrawLine(origin, origin + Vector2.down * distance, Color.cyan);
                }
#endif
                return hit.collider != null ? hit.distance : float.PositiveInfinity;
            }
        }

        public UnityAction HitBehaviour { get => this.hitBehaviour; set => this.hitBehaviour = value; }
        public UnityAction DeathBehaviour { get => this.deathBehaviour; set => this.deathBehaviour = value; }
        public EnemyType EnemyType { get => this.enemyType; set => this.enemyType = value; }

        void Awake()
        {
            this.AutoBind(ref character);
            this.characterStatusIcon = GetComponentInChildren<StatusIcon>();
            Character.DieActions.AddListener(Die);
            Character.HitActions.AddListener(Hit);
            CurrentState = useUtilityBrain
                ? new EnemyUtilityState(this, brainSettings != null ? brainSettings.Build() : DefaultBrain.Create())
                : new EnemyIdleState(this);
        }

        private void Start()
        {
            this.player = Game.Instance.Player;
            Reset();
        }

        public void Spawn(Vector3 position)
        {
            if (Character.Body != null && Character.Body.Flying)
            {
                position += Vector3.up * Random.Range(0, 5);
            }

            transform.position = position;
            gameObject.SetActive(true);
        }

        private void UpdateDetect()
        {
            if (!this.player.EnsureNotNull(this, nameof(this.player)) || !this.player.Character.EnsureNotNull(this, nameof(this.player.Character)))
            {
                return;
            }

            if (!aimPivot.EnsureNotNull(this, nameof(aimPivot)))
            {
                Debug.LogError("aimPivot is null");
                return;
            }

            CanHearPlayer = this.player.Character.CanHear(aimPivot.position);

            var hit = InVisionCone(this.player.transform.position)
                ? this.player.Character.CanSee(aimPivot.position, EnemySettings.MaxVisualRange, lineOfSightMask)
                : null;

            if (hit.HasValue && hit.Value.transform != null && hit.Value.transform.CompareTag(Tags.Player))
            {
                CanSeePlayer = true;
                HasSeenPlayer = true;
                LastKnownPlayerPosition = hit.Value.point;
            }
            else
            {
                CanSeePlayer = false;
            }

            if (CanHearPlayer && !CanSeePlayer && !wasHearingPlayer)
            {
                LastKnownPlayerPosition = this.player.transform.position + (Vector3)(Random.insideUnitCircle * HearingPositionError);
            }
            wasHearingPlayer = CanHearPlayer;

            if (!HasSeenDeadEnemies)
            {
                var hits = Physics2D.OverlapCircleAll(aimPivot.position, 5f, lineOfSightMask);
                foreach (var h in hits)
                {
                    if (h.CompareTag(Tags.Enemy) && h.TryGetComponent<IAi>(out var ai) && !ai.CurrentState.IsAlive)
                    {
                        Debug.Log("has seen dead enemies", this);
                        HasSeenDeadEnemies = true;
                        break;
                    }
                }
            }

            if ((this.player.transform.position - aimPivot.position).sqrMagnitude <= EnemySettings.ProximityRange * EnemySettings.ProximityRange)
            {
                HasBeenAlerted = true;
                LastKnownPlayerPosition = this.player.transform.position;
                lastPerceivedTime = Time.time;
            }

            if (CanSeePlayer && Time.time >= nextAlertBroadcast)
            {
                AlertNearbyAllies(LastKnownPlayerPosition);
                nextAlertBroadcast = Time.time + AlertBroadcastInterval;
            }

            if (CanSeePlayer || CanHearPlayer)
            {
                lastPerceivedTime = Time.time;
            }
            else if (Time.time > lastPerceivedTime + EnemySettings.MemoryTime)
            {
                HasSeenPlayer = false;
                HasBeenAlerted = false;
                HasSeenDeadEnemies = false;
                wasHearingPlayer = false;
            }
        }

        private void AlertNearbyAllies(Vector3 position)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, EnemySettings.AlertRadius, lineOfSightMask);
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject)
                {
                    continue;
                }

                if (h.CompareTag(Tags.Enemy) && h.TryGetComponent<IAi>(out var ai) && ai.CurrentState.IsAlive)
                {
                    ai.Alert(position);
                }
            }
        }

        private bool InVisionCone(Vector3 target)
        {
            var toTarget = target - aimPivot!.position;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            return Vector2.Angle(transform.right, toTarget) <= EnemySettings.FieldOfView * 0.5f;
        }

        public void Reset()
        {
            Character.Reset();
            Character.SetStateBattle();
            lastPerceivedTime = Time.time;
        }

        public void UpdateBehaviour()
        {
            Character.OperatingSystem.UpdateBehaviour();
        }

        public void FixedUpdateBehaviour()
        {
            if (senseTick++ % SenseInterval == senseOffset)
            {
                UpdateDetect();
            }

            if (Game.Instance.Player.Character.IsDead)
            {
                Character.SetMovement(0, 0);
                return;
            }

            CurrentState.UpdateBehaviour(new AiContext()
            {
                name = this.name,
                position = transform.position,
                canHearPlayer = CanHearPlayer || HasBeenAlerted,
                canSeePlayer = CanSeePlayer,
                hasSeenPlayer = HasSeenPlayer,
                hasSeenDeadEnemies = HasSeenDeadEnemies,
                facing = Mathf.RoundToInt(Character.transform.right.x),
                settings = EnemySettings,
                playerInFrontOfUs = PlayerInFrontOfUs,
                lastKnownPlayerPosition = LastKnownPlayerPosition,
                wallInFront = WallInFront,
                dropInFront = DropInFront,
                groundDistance = (character.Body && character.Body.Flying) ? GroundDistance : float.PositiveInfinity,
                movementMask = movementMask,
                bullets = character.OperatingSystem.Ammo,
                flying = character.Body ? character.Body.Flying : false,
            });

            Character.Physics();
        }

        public void LateUpdateBehaviour() => Character.Animate();

        public float DistanceToPlayer() => this.player != null ? (this.player.transform.position - transform.position).magnitude : int.MaxValue;

        public void Alert(Vector3 position)
        {
            LastKnownPlayerPosition = position;
            HasBeenAlerted = true;
            lastPerceivedTime = Time.time;
        }

        private void Hit()
        {
            //Debug.Log("took a hit", this);
        }

        private void Die() => CurrentState = new EnemyDeadState();
    }
}
