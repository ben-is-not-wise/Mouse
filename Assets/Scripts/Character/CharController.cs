#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;

namespace HackedDesign
{

    public class CharController : MonoBehaviour, ICharacterExecute
    {
        [Header("Actions")]
        [SerializeField] private UnityEvent dieActions;
        [SerializeField] private UnityEvent hitActions;
        [Header("References")]
        [SerializeField] private AttackController attackController;
        [SerializeField] private OperatingSystem operatingSystem;
        [SerializeField] private PhysicsController? body = null;
        [SerializeField] private Animator? animator = null;
        [SerializeField] private new Collider2D? collider = null;
        [SerializeField] private Transform head;
        [SerializeField] private Ghost ghost;
        [SerializeField] private ShadowCaster2D shadow;
        [Header("Settings")]
        [SerializeField] private CharacterSettings? settings = null;
        
        #region State
        private ICharacterState currentState;

        public ICharacterState CurrentState
        {
            get => currentState;
            set
            {
                currentState?.End();
                currentState = value;
                currentState?.Begin();
            }
        }

        public void SetIdleState() => CurrentState = new CharacterIdleState(this, Animator);
        public void SetBattleState() => CurrentState = new CharacterBattleState(this, attackController, Animator);
        public void SetSitState() => CurrentState = new CharacterSittingState(Animator);
        public void SetDeadState() => CurrentState = new CharacterDeadState(Animator);
        #endregion State

        #region Properties

        public OperatingSystem OperatingSystem { get => operatingSystem; private set => operatingSystem = value; }
        public Animator? Animator { get => animator; set => animator = value; }
        public PhysicsController? Body { get => body; set => body = value; }
        public CharacterSettings? Settings { get => settings; set => settings = value; }

        public ShadowCaster2D? Shadow => shadow;

        public Transform Head => head;

        public bool IsAnimatingAttack => this.attackController.IsAnimatingAttack;
        public bool IsCrouched { get; private set; }

        public bool JumpHoldFlag { get; set; }
        public bool IsWalking { get; private set; } = false;
        public bool GhostToggle { get; private set; } = false;

        public bool IsPlayer => this.gameObject.CompareTag(Tags.Player);

        public FXType HitFXType => settings != null ? settings.HitFX : FXType.EnvHit;

        #endregion Properties

        private bool jumpFlag = false;
        private bool knockback = false;

        private bool Aiming { get; set; } = false;
        private float DesiredMovement => InputDirection * Mathf.Max(
            CurrentState.CurrentSpeed(
                new CharacterSpeedContext() 
                { 
                    crouched = IsCrouched, 
                    crouchSpeed = Settings != null ? Settings.CrouchSpeed : 0, 
                    walk = IsWalking,
                    walkSpeed = Settings != null ? Settings.WalkSpeed : 0,
                    runSpeed = Settings != null ? Settings.RunSpeed : 0,
                    momentum = OperatingSystem.Momentum,
                    knockback = knockback
                }
            ),
            0f);
        private bool Jump
        {
            get => jumpFlag;
            set
            {
                if (Animator.EnsureNotNull(this, nameof(Animator)) && !IsDead && !jumpFlag && value)
                {
                    Animator.SetTrigger(AnimatorParams.Jump);
                }
                jumpFlag = value;
            }
        }
        private float InputDirection { get; set; }
        private float InputClimb { get; set; }

        void Awake()
        {
            this.AutoBind(ref operatingSystem);
            this.AutoBind(ref body);
            this.AutoBind(ref collider);
            this.AutoBind(ref shadow);
            if (head == null)
            {
                head = transform;
            }
            operatingSystem.hitActions += Hit;
            operatingSystem.dieActions += Die;
            SetIdleState();
        }

        #region Commands
        public void SetCrouch(bool flag) => IsCrouched = flag;

        public void SetMovement(float inputDirection, float inputClimb)
        {
            InputDirection = inputDirection;
            InputClimb = inputClimb;
        }

        public void SetJump() => Jump = true;
        public void ClearJump() => Jump = false;

        public void SetWalk(bool flag) => IsWalking = flag;
        public void WalkToggle() => IsWalking = !IsWalking;
        public void SetAim(bool flag) => Aiming = flag;

        public void ApplyKnockback(Vector3 direction)
        {
            if (body == null)
            {
                return;
            }

            if(OperatingSystem.Health == 0)
            {
                return;
            }

            Debug.Log("Knockback start", this);

            this.knockback = true;
            body.Knockback(direction, Settings.EnsureNotNull(this, nameof(Settings)) ? Settings.KnockbackAmount : 0);
            body.CurrentlyKnockback = true;
            if (Animator.EnsureNotNull(this, nameof(Animator)))
            {
                Animator.SetTrigger(AnimatorParams.Knockback);
            }

            if(IsPlayer)
            {
                CameraShake.Instance.Shake(0.5f, 0.3f);
            }

            StartCoroutine(KnockbackPause());
        }

        private IEnumerator KnockbackPause()
        {
            yield return new WaitForSeconds(Settings.EnsureNotNull(this, nameof(Settings)) ? Settings.KnockbackTime : 0);
            Debug.Log("Knockback pause over", this);
            StartCoroutine(KnockbackOver());
        }

        private IEnumerator KnockbackOver()
        {
            yield return new WaitForSeconds(Settings.EnsureNotNull(this, nameof(Settings)) ? Settings.KnockbackFreezeTime : 0);
            Debug.Log("Knockback over", this);
            Stop();
            knockback = false;
            if (body)
            {
                body.CurrentlyKnockback = false;
            }
        }

        public void TriggerInteract()
        {
            if (!Animator.EnsureNotNull(this, nameof(Animator)))
            {
                return;
            }
            Animator.SetTrigger(AnimatorParams.Interact);
        }

        public void Roll()
        {
            if (!Animator.EnsureNotNull(this, nameof(Animator)) || Mathf.Abs(InputDirection) < Mathf.Epsilon)
            {
                return;
            }
            Animator.SetTrigger(AnimatorParams.Roll);
        }

        public void Stop()
        {
            if (!Body.EnsureNotNull(this, nameof(Animator)))
            {
                return;
            }
            Body.Stop();
        }

        public void Freeze()
        {
            if (!Body.EnsureNotNull(this, nameof(Animator)))
            {
                return;
            }
            Body.Freeze();
        }

        public void ToggleGhost()
        {
            GhostToggle = !GhostToggle;

            if (ghost.OrNull() != null)
            {
                if (GhostToggle)
                {
                    ghost.StartTrail();
                }
                else
                {
                    ghost.StopTrail();
                }
            }
        }

        #endregion Commands

        #region Update

        public void Reset()
        {
            Stop();
            OperatingSystem.Reset(Settings);
            if (Animator.EnsureNotNull(this, nameof(Animator)))
            {
                Animator.SetBool(AnimatorParams.Grounded, true);
                //Animator.Play(AnimatorParams.Idle);
            }
            SetCrouch(false);
            UpdateSpriteDirection(1f, 1f);
            InputDirection = 0f;
            InputClimb = 0f;
            if (collider != null)
            {
                collider.enabled = true;
            }
        }

        public void Physics()
        {
            if (IsDead || Body == null || Body.Static)
            {
                return;
            }

            if (!knockback)
            {
                if (Mathf.Abs(InputDirection) < Mathf.Epsilon || Body.CurrentlyClimbingLedge || Body.OnWall)
                {
                    OperatingSystem.Momentum -= Time.fixedDeltaTime * Settings.MomentumLoss;
                }
                else
                {
                    if (Body.OnGround)
                    {
                        OperatingSystem.Momentum += Time.fixedDeltaTime * Settings.BaseMomentumFactor;
                    }
                    //OperatingSystem.Momentum -= Time.fixedDeltaTime * Settings.MomentumAirLoss;

                }

                Body.FixedMovement(DesiredMovement, InputClimb, Jump, JumpHoldFlag, OperatingSystem.Momentum);
            }

            Jump = false;
            JumpHoldFlag = false;
        }

        public void Animate()
        {

            if (Body != null && Body.CurrentlyClimbingLedge && Animator != null)
            {
                Animator.ResetTrigger(AnimatorParams.Jump);
            }
            CurrentState.ResetAnimationTriggers();
            CurrentState.Animate(new CharacterAnimationContext()
            {
                crouched = IsCrouched,
                onGround = Body == null || Body.OnGround,
                onWall = Body != null && Body.OnWall,
                velocityY = Body == null ? 0 : Body.VelocityY,
                movementMagnitude = Mathf.Abs(DesiredMovement),
                rollOnLand = Body != null && Body.LastFallTime > 1f,
                aiming = Aiming,
                isClimbingLedge = Body != null && Body.CurrentlyClimbingLedge,
                knockback = knockback
            });
        }

        public bool CanHear(Vector3 position)
        {
            var vectorToPlayer = Head.transform.position - position;
            var hearDistance = IsCrouched ? 0f : IsWalking ? 1f : 4f;

            return vectorToPlayer.magnitude < hearDistance;
        }

        public RaycastHit2D? CanSee(Vector3 position, float maxVisualRange, LayerMask lineOfSightMask)
        {
            var vectorToPlayer = Head.transform.position - position;

            return vectorToPlayer.sqrMagnitude > (maxVisualRange * maxVisualRange)
                ? null
                : Physics2D.Raycast(position, vectorToPlayer.normalized, Settings != null ? Settings.ShootDistance : 0, lineOfSightMask);
        }

        #endregion Update

        #region Death
        public bool IsDead => !CurrentState.IsAlive;
        public bool CanAttack => CurrentState.CanAttack;
        public UnityEvent DieActions { get => this.dieActions; set => this.dieActions = value; }
        public UnityEvent HitActions { get => this.hitActions; set => this.hitActions = value; }
        public bool Knockback { get => this.knockback; set => this.knockback = value; }

        private void Die()
        {
            Debug.Log("die " + this.transform.name);
            if (IsDead)
            {
                return;
            }

            if(Animator.EnsureNotNull(this, nameof(Animator)))
            {
                Animator.SetBool(AnimatorParams.Dead, true);
                Animator.SetTrigger(AnimatorParams.Dying);
            }

            if (Body != null)
            {
                Body.Flying = false;
            }

            SetDeadState();

            DieActions?.Invoke();
        }

        public void Splat()
        {
            if (Animator.EnsureNotNull(this, nameof(Animator)))
            {
                Animator.SetBool(AnimatorParams.Dead, true);
                Animator.SetTrigger(AnimatorParams.Splat);
            }
            SetDeadState();

            StartCoroutine(EndFall());
        }

        private IEnumerator EndFall()
        {
            yield return new WaitUntil(() => IsDeadAnimation());
            DieActions?.Invoke();
        }

        private bool IsDeadAnimation() => Animator && Animator.GetCurrentAnimatorStateInfo(0).IsTag(AnimatorParams.IsDeadTag);
        #endregion Death

        #region Attack
        public void Attack(Vector3 target, bool aiming) => CurrentState.Attack(new CharacterAttackContext()
        {
            target = target,
            aiming = aiming,
            knockback = knockback
        });

        #endregion Attack

        #region Health

        public void TakeDamage(int amount, Vector3 contact, Vector3 direction, bool applyKnockback)
        {
            Debug.Log($"take damage {amount}" , this);
            FXPool.Instance.Spawn(settings? settings.HitFX : FXType.EnvHit, contact, direction);
            OperatingSystem.Health -= amount;
            if(OperatingSystem.Health > 0 && applyKnockback)
            {
                ApplyKnockback(direction);
            }
        }

        public void TakeMomentumHit(int amount, Vector3 contact, Vector3 direction)
        {
            OperatingSystem.Momentum -= amount;
        }

        private void Hit() => HitActions?.Invoke();

        #endregion Health

        #region Animation
        public void UpdateSpriteDirection(float movementDirection, float facingDirection)
        {
            if (Body != null && Body.OnWall)
            {
                transform.right = Body.ContactNormal.x < 0 ? Vector3.right : Vector3.left;
                return;
            }

            if (Mathf.Abs(movementDirection) >= Mathf.Epsilon)
            {
                transform.right = new Vector3(movementDirection, 0, 0);
            }
            else if (Mathf.Abs(facingDirection) >= Mathf.Epsilon)
            {
                transform.right = new Vector3(facingDirection, 0, 0);
            }
        }

        #endregion Animation

        #region Commands
        public void ExecuteCommand(ICharacterCommand cmd) => cmd.Execute(this);
        #endregion Commands
    }

    public struct CharacterAttackContext
    {
        public Vector3 target;
        public bool aiming;
        public bool knockback;
    }

    public struct CharacterSpeedContext
    {
        public bool crouched;
        public float crouchSpeed;
        public bool walk;
        public float walkSpeed;
        public float runSpeed;
        public float momentum;
        public bool knockback;
    }

    public struct CharacterAnimationContext
    {
        public bool crouched;
        public bool onGround;
        public bool onWall;
        public float velocityY;
        public float movementMagnitude;
        public bool rollOnLand;
        public bool aiming;
        public bool isClimbingLedge;
        public bool knockback;
    }
}