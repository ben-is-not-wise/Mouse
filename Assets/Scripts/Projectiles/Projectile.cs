using HackedDesign.UI.DamageNumbers;
using System.Collections;
using UnityEngine;

namespace HackedDesign
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile: MonoBehaviour
    {
        [SerializeField] private float lifetimeSeconds = 3f;
        [SerializeField] private int defaultDamage = 10000;

        [Header("Explosion (grenade)")]
        [SerializeField] private bool explosive = false;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private LayerMask explosionMask = ~0;
        [SerializeField] private FXType explosionFX = FXType.EnvHit;
        [SerializeField] private float armingSeconds = 0.1f;
        [SerializeField] private float spinSpeed = 0f;

        private Rigidbody2D rb;
        private Vector3 start;
        private int damage;
        private float armedTime;
        private CharController owner;

        private Coroutine lifetimeCoroutine;

        public enum ProjectileType
        {
            Bullet,
            Grenade
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            damage = defaultDamage;
        }

        // Type of this projectile. Assign on the prefab in the inspector.
        public ProjectileType Type = ProjectileType.Bullet;

        public void Launch(Vector3 start, Vector3 dir, float force, int damage, bool gravity = false, CharController owner = null)
        {
            this.start = start;
            this.damage = damage;
            this.owner = owner;
            this.armedTime = Time.time + (explosive ? armingSeconds : 0f);

            rb.gravityScale = gravity ? 1 : 0;
            rb.linearVelocity = Vector2.zero;
            // Spin backwards relative to the throw direction so it reads as a forward roll.
            rb.angularVelocity = spinSpeed != 0f ? spinSpeed * (dir.x >= 0f ? -1f : 1f) : 0f;
            if (force != 0f)
            {
                rb.AddForce((Vector2)dir.normalized * force, ForceMode2D.Impulse);
            }
        }

        private void OnEnable()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
            }

            lifetimeCoroutine = StartCoroutine(LifetimeTimeout());
        }

        void OnDisable()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }
        }

        private IEnumerator LifetimeTimeout()
        {
            yield return new WaitForSeconds(lifetimeSeconds);

            if(explosive)
            {
                Explode();
            }

            // Nothing hit within lifetime → return to pool
            ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Time.time < armedTime)
            {
                return;
            }

            if (owner != null && other.GetComponentInParent<CharController>() == owner)
            {
                return;
            }

            if (explosive)
            {
                Explode();
            }
            else
            {
                ApplyHit(other.transform, this.transform.position, true);
            }

            ReturnToPool();
        }

        private void Explode()
        {
            var center = this.transform.position;
            FXPool.Instance.Spawn(explosionFX, center, Vector3.up);

            var hits = Physics2D.OverlapCircleAll(center, explosionRadius, explosionMask);
            foreach (var h in hits)
            {
                if (h.TryGetComponent<BreakGlass>(out var glass))
                {
                    glass.Break(center);
                }
                else if (h.TryGetComponent<CharController>(out var targetChar) && targetChar != owner && !targetChar.IsDead)
                {
                    var point = h.ClosestPoint(center);
                    targetChar.TakeDamage(damage, point, (Vector3)point - center, true);
                    DamageNumberPool.Instance.Spawn(damage, point);
                    FXPool.Instance.Spawn(targetChar.HitFXType, point, (Vector3)point - center);
                }
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }

            owner = null;
            gameObject.SetActive(false);
        }

        private void ApplyHit(Transform hitTransform, Vector2 hitPoint, bool hitEnv)
        {
            Debug.Log("Bullet hit " + hitTransform.name);

            if (hitTransform.TryGetComponent<BreakGlass>(out var glass))
            {
                glass.Break(start);
            }
            else if (hitTransform.TryGetComponent<CharController>(out var targetChar))
            {
                if (!targetChar.IsDead)
                {
                    targetChar.TakeDamage(damage, hitPoint, (Vector3)hitPoint - start, true);
                    DamageNumberPool.Instance.Spawn(damage, hitPoint);
                }

                FXPool.Instance.Spawn(targetChar.HitFXType, hitPoint, (Vector3)hitPoint - start);
            }
            else if (hitEnv)
            {
                FXPool.Instance.Spawn(FXType.EnvHit, hitPoint, start - (Vector3)hitPoint);
            }

            ReturnToPool();
        }
    }
}
