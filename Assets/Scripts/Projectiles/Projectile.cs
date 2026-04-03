using HackedDesign.UI.DamageNumbers;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace HackedDesign
{
    public class Projectile: MonoBehaviour
    {
        [SerializeField] private float lifetimeSeconds = 3f;

        private Rigidbody2D rb;
        private Vector3 start;
        private int damage;

        private Coroutine lifetimeCoroutine;

        public enum ProjectileType
        {
            Bullet
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Type of this projectile. Assign on the prefab in the inspector.
        public ProjectileType Type = ProjectileType.Bullet;

        public void Launch(Vector3 start, Vector3 dir, float force, int damage, bool gravity = false)
        {
            this.start = start;
            this.damage = damage;

            rb.gravityScale = gravity ? 1 : 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
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

            // Nothing hit within lifetime → return to pool
            ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ApplyHit(other.transform, this.transform.position, true);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(lifetimeCoroutine);
                lifetimeCoroutine = null;
            }

            gameObject.SetActive(false);
        }

        private void ApplyHit(Transform hitTransform, Vector2 hitPoint, bool hitEnv)
        {
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
        }
    }
}
