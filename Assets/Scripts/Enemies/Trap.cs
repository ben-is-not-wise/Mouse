using HackedDesign.UI.DamageNumbers;
using System.Collections;

using UnityEngine;

namespace HackedDesign
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Collider2D))]
    public class Trap : MonoBehaviour
    {
        private new Collider2D collider2D;
        private Animator animator;
        [SerializeField] private int minAmount = 25;
        [SerializeField] private int maxAmount = 99;
        //[SerializeField] private int amount = 200;
        [SerializeField] private DamageType damageType = DamageType.Damage;

        private void Awake()
        {
            collider2D = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Tags.Player) && other.TryGetComponent<PlayerController>(out var player))
            {
                var hit = collider2D.ClosestPoint(other.transform.position);
                AttackPlayer(hit, player.Character);
            }
        }

        private void AttackPlayer(Vector2 hit, CharController character)
        {
            AnimateAttack();

            switch (damageType)
            {
                case DamageType.Damage:
                    character.ApplyKnockback(Vector2.up);
                    var damage = Random.Range(minAmount, maxAmount);
                    var hitPoint = hit + Vector2.up * 0.5f;
                    character.TakeDamage(damage, hitPoint, Vector2.up, false);
                    DamageNumberPool.Instance.Spawn(damage, hitPoint);
                    break;
                case DamageType.Momentum:
                    character.ApplyKnockback(Vector2.up);
                    character.TakeMomentumHit(Random.Range(minAmount, maxAmount), hit, Vector2.up);
                    break;
            }
        }

        private void AnimateAttack()
        {
            if (animator)
            {
                animator.SetTrigger(AnimatorParams.Attack);
                StartCoroutine(Reset());
            }
        }

        private IEnumerator Reset()
        {
            yield return new WaitForEndOfFrame();

            animator.ResetTrigger(AnimatorParams.Attack);
        }
    }

    public enum DamageType
    {
        None,
        Damage,
        Momentum,
    }
}
