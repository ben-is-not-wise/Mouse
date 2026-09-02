#nullable enable
using HackedDesign.UI.DamageNumbers;
using System.Collections;
using UnityEngine;

namespace HackedDesign
{
    public class AttackController : MonoBehaviour, IAttackController
    {
        [Header("References")]
        [SerializeField] private CharController character;
        [SerializeField] private Transform? pivot;
        [SerializeField] private Transform? barrel;
        [SerializeField] private OperatingSystem? operatingSystem;
        [SerializeField] private Animator? animator;
        [Header("Settings")]
        [SerializeField] private CharacterSettings? settings = null;

        private const string AttackAnimTag = "Attack";

        private const float ShootShakeIntensity = 0.7f;
        private const float ShootShakeTime = 0.1f;
        private const float MeleeShakeIntensity = 0.5f;
        private const float MeleeShakeTime = 0.3f;

        private float nextAttackTimer = float.NegativeInfinity;

        private bool isPlayer = false;

        private static readonly int[] meleeExecuteAnimations = {
            AnimatorParams.Punch,
            AnimatorParams.Kick,
            AnimatorParams.Melee
        };

        private static readonly int[] meleeAnticipationAnimations =
        {
            AnimatorParams.PunchAnticipate, AnimatorParams.KickAnticipate, AnimatorParams.MeleeAnticipate
        };

        public Vector3 Pivot => pivot != null ? pivot.position : this.transform.position;
        public bool CanAttack => Time.time >= nextAttackTimer;
        public bool HasGun => OperatingSystem.CurrentWeapon.weaponType == WeaponType.Gun;
        public bool HasGrenade => OperatingSystem.CurrentWeapon.weaponType == WeaponType.Grenade;
        public bool CanShoot => OperatingSystem.HasAmmo;
        public bool IsAnimatingAttack => animator.EnsureNotNull(this, nameof(animator)) && animator.GetCurrentAnimatorStateInfo(0).IsTag(AttackAnimTag);

        void Awake()
        {
            this.AutoBind(ref character);
            this.AutoBind(ref operatingSystem);
            this.AutoBind(ref animator);

            operatingSystem.Require(this, nameof(operatingSystem));
            animator.Require(this, nameof(animator));
            settings.Require(this, nameof(settings));

            isPlayer = CompareTag(Tags.Player);
        }

        private OperatingSystem OperatingSystem => operatingSystem!;
        private Animator Animator => animator!;
        private CharacterSettings Settings => settings!;

        public void Melee()
        {
            if (!CanAttack || IsAnimatingAttack)
            {
                return;
            }

            if (character.Knockback)
            {
                UpdateNextAttackTimer();
                return;
            }

            StartCoroutine(MeleeAnticipate());
        }

        private IEnumerator MeleeAnticipate()
        {
            int meleeType = Random.Range(0, CanShoot && HasGun ? 3 : 2);
            //int meleeType = GetRandomMeleeAnimationType();
            Animator.SetTrigger(meleeAnticipationAnimations[meleeType]);

            float elapsedTime = 0f;
            while (elapsedTime < CalcAnticipateDelay())
            {
                // If we get knocked back, cancel the attack
                if (character.Knockback)
                {
                    Animator.ResetTrigger(AnimatorParams.ShootAnticipate);
                    UpdateNextAttackTimer();
                    yield break; // Exit the coroutine
                }

                elapsedTime += Time.deltaTime;
                yield return null; // Wait one frame
            }

            MeleeExecute(meleeType);
        }

        private void MeleeExecute(int meleeType)
        {
            UpdateNextAttackTimer();

            if (!character.CanAttack || character.Knockback)
            {
                return;
            }

            Animator.SetTrigger(meleeExecuteAnimations[meleeType]);

            var results = Physics2D.OverlapCircleAll(Pivot, Settings.MeleeDistance, Settings.AttackMask);

            if (results.Length > 0)
            {
                if (isPlayer)
                {
                    CameraShake.Instance.Shake(MeleeShakeIntensity, MeleeShakeTime);
                }

                foreach (var result in results)
                {
                    ApplyHit(result.transform, result.ClosestPoint(Pivot), false);
                }
            }
        }

        public void Shoot(Vector3 target)
        {
            if (!CanAttack || IsAnimatingAttack)
            {
                return;
            }

            if (character.Knockback)
            {
                UpdateNextAttackTimer();
                return;
            }

            StartCoroutine(ShootAnticipate(target));
        }

        private IEnumerator ShootAnticipate(Vector3 target)
        {
            Animator.SetTrigger(AnimatorParams.ShootAnticipate);

            float elapsedTime = 0f;

            while (elapsedTime < CalcAnticipateDelay())
            {
                // If we get knocked back, cancel the attack
                if (character.Knockback)
                {
                    Animator.ResetTrigger(AnimatorParams.ShootAnticipate);
                    UpdateNextAttackTimer();
                    yield break; // Exit the coroutine
                }

                elapsedTime += Time.deltaTime;
                yield return null; // Wait one frame
            }

            ShootExecute(target);
        }

        private float CalcAnticipateDelay() => settings != null ? settings.AnticipateDelay : 0;

        private void ShootExecute(Vector3 target)
        {
            UpdateNextAttackTimer();

            if (!character.CanAttack || character.Knockback)
            {
                return;
            }

            Animator.SetTrigger(AnimatorParams.Shoot);

            if (isPlayer)
            {
                CameraShake.Instance.Shake(ShootShakeIntensity, ShootShakeTime);
                AlertNearbyEnemies();
            }

            var start = Pivot;

            var dir = (target - start).normalized;

            ProjectilePool.Instance.Spawn(Projectile.ProjectileType.Bullet, start, dir, OperatingSystem.CurrentWeapon.RandomShootDamage, OperatingSystem.CurrentWeapon.projectileForce, owner: character);

            OperatingSystem.DecreaseAmmo();

            Debug.DrawRay(Pivot, target - Pivot, Color.red, 0.3f);
        }

        public void Throw(Vector3 target)
        {
            if (!CanAttack || IsAnimatingAttack)
            {
                return;
            }

            if (character.Knockback)
            {
                UpdateNextAttackTimer();
                return;
            }

            StartCoroutine(ThrowAnticipate(target));
        }

        private IEnumerator ThrowAnticipate(Vector3 target)
        {
            Animator.SetTrigger(AnimatorParams.ShootAnticipate);

            float elapsedTime = 0f;

            while (elapsedTime < CalcAnticipateDelay())
            {
                if (character.Knockback)
                {
                    Animator.ResetTrigger(AnimatorParams.ShootAnticipate);
                    UpdateNextAttackTimer();
                    yield break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            ThrowExecute(target);
        }

        private void ThrowExecute(Vector3 target)
        {
            UpdateNextAttackTimer();

            if (!character.CanAttack || character.Knockback)
            {
                return;
            }

            Animator.SetTrigger(AnimatorParams.Shoot);

            if (isPlayer)
            {
                CameraShake.Instance.Shake(ShootShakeIntensity, ShootShakeTime);
                AlertNearbyEnemies();
            }

            var start = Pivot;
            float speed = OperatingSystem.CurrentWeapon.projectileForce;

            var velocity = SolveArc(start, target, speed);

            ProjectilePool.Instance.Spawn(Projectile.ProjectileType.Grenade, start, (Vector3)velocity.normalized, OperatingSystem.CurrentWeapon.RandomShootDamage, velocity.magnitude, gravity: true, owner: character);

            OperatingSystem.DecreaseAmmo();
        }

        // Launch angle for grenades, measured above horizontal.
        private const float LaunchAngleDegrees = 20f;

        // Ballistic launch velocity at a fixed angle, solving for the speed that lands on the
        // target. Clamps to maxSpeed (undershoots if out of range); lobs at maxSpeed when the
        // target sits above what this angle can reach.
        private static Vector2 SolveArc(Vector2 from, Vector2 to, float maxSpeed)
        {
            float g = Mathf.Abs(Physics2D.gravity.y);
            float dx = to.x - from.x;
            float dy = to.y - from.y;
            float dir = dx < 0 ? -1f : 1f;
            float range = Mathf.Abs(dx);

            float theta = LaunchAngleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);
            float denom = range * Mathf.Tan(theta) - dy;

            if (g > 0f && cos > 0f && denom > 0f)
            {
                float speed = Mathf.Sqrt(g * range * range / (2f * cos * cos * denom));
                speed = Mathf.Min(speed, maxSpeed);
                return new Vector2(dir * speed * cos, speed * sin);
            }

            return new Vector2(dir * maxSpeed * cos, maxSpeed * sin);
        }

        private void AlertNearbyEnemies()
        {
            var hits = Physics2D.OverlapCircleAll(Pivot, settings ? settings.AlertRadius : 20);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IAi>(out var ai))
                {
                    var inline = Physics2D.Linecast(Pivot, ai.Character.transform.position, settings ? settings.AttackMask : 0);
                    if (inline.transform != null && inline.transform.gameObject == ai.Character.gameObject)
                    {
                        Debug.Log($"alerting ai {ai.Character.name}");
                        ai.Alert(Pivot);
                    }
                }
            }
        }

        private void UpdateNextAttackTimer() => nextAttackTimer = Time.time + Settings.AttackRate;

        private void ApplyHit(Transform hitTransform, Vector2 hitPoint, bool hitEnv)
        {
            if (hitTransform.TryGetComponent<BreakGlass>(out var glass))
            {
                glass.Break(Pivot);
                return;
            }

            if (hitTransform.TryGetComponent<CharController>(out var targetChar))
            {
                if (!targetChar.IsDead)
                {
                    var damage = OperatingSystem.CurrentWeapon.RandomMeleeDamage;
                    targetChar.TakeDamage(damage, hitPoint, (Vector3)hitPoint - Pivot, true);
                    DamageNumberPool.Instance.Spawn(damage, hitPoint);

                    FXPool.Instance.Spawn(FXType.Blood, hitPoint, (Vector3)hitPoint - Pivot);
                }

                return;
            }
            
            if (hitEnv)
            {
                FXPool.Instance.Spawn(FXType.EnvHit, hitPoint, Pivot - (Vector3)hitPoint);
            }
        }
    }
}
