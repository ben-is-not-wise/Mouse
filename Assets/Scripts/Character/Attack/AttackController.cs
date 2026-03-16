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

            StartCoroutine(MeleeAnticipate());
        }

        private IEnumerator MeleeAnticipate()
        {
            int meleeType = Random.Range(0, CanShoot && HasGun ? 3 : 2);
            //int meleeType = GetRandomMeleeAnimationType();
            Animator.SetTrigger(meleeAnticipationAnimations[meleeType]);

            yield return new WaitForSeconds(settings.AnticipateDelay);
            MeleeExecute(meleeType);
        }

        private void MeleeExecute(int meleeType)
        {
            if(!character.CanAttack)
            {
                return;
            }

            UpdateNextAttackTimer();
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

            StartCoroutine(ShootAnticipate(target));
        }

        private IEnumerator ShootAnticipate(Vector3 target)
        {
            Animator.SetTrigger(AnimatorParams.ShootAnticipate);
            yield return new WaitForSeconds(settings.AnticipateDelay);
            ShootExecute(target);
        }

        private void ShootExecute(Vector3 target)
        {
            if (!character.CanAttack)
            {
                return;
            }
            UpdateNextAttackTimer();
            Animator.SetTrigger(AnimatorParams.Shoot);

            if (isPlayer)
            {
                CameraShake.Instance.Shake(ShootShakeIntensity, ShootShakeTime);
                AlertNearbyEnemies();
            }

            var start = barrel != null ? barrel.position : this.transform.position;

            var dir = (target - start).normalized;

            ProjectilePool.Instance.Spawn(Projectile.ProjectileType.Bullet, start, dir, OperatingSystem.CurrentWeapon.RandomShootDamage, 100f);

            OperatingSystem.DecreaseAmmo();

            Debug.DrawRay(Pivot, target - Pivot, Color.red, 0.3f);

            //var result = Physics2D.Raycast(Pivot, target - Pivot, Settings.ShootDistance, Settings.AttackMask);

            //if (result)
            //{
            //    ApplyHit(result.transform, result.point, true);
            //}
        }

        private void AlertNearbyEnemies()
        {
            var hits = Physics2D.OverlapCircleAll(Pivot, settings? settings.AlertRadius : 20);

            foreach(var hit in hits)
            {
                if(hit.TryGetComponent<IAi>(out var ai))
                {
                    var inline = Physics2D.Linecast(Pivot, ai.Character.transform.position, settings ? settings.AttackMask : 0);
                    if(inline.transform != null && inline.transform.gameObject == ai.Character.gameObject)
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
            }
            else if (hitTransform.TryGetComponent<CharController>(out var targetChar))
            {
                var damage = OperatingSystem.CurrentWeapon.RandomMeleeDamage;
                targetChar.TakeDamage(damage, hitPoint, (Vector3)hitPoint - Pivot);
                DamageNumberPool.Instance.Spawn(damage, hitPoint);
            }
            else if(hitEnv)
            {
                FXPool.Instance.Spawn(FXType.EnvHit, hitPoint, Pivot - (Vector3)hitPoint);
            }
        }
    }
}
