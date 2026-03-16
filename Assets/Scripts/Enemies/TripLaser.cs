using HackedDesign.UI.DamageNumbers;
using System.Collections;

using UnityEngine;

namespace HackedDesign
{
    public class TripLaser : MonoBehaviour
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
            //animator.SetBool("on", true);
            //animator.SetFloat("type", 1);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Tags.Player))
            {
                var hit = collider2D.ClosestPoint(other.transform.position);
                AttackPlayer(hit);
            }
        }

        private void AttackPlayer(Vector2 hit)
        {
            AnimateAttack();

            switch (damageType)
            {
                case DamageType.Damage:
                    Game.Instance.Player.Character.Knockback(Vector2.up);
                    var damage = Random.Range(minAmount, maxAmount);
                    var hitPoint = hit + Vector2.up * 0.5f;
                    Game.Instance.Player.Character.TakeDamage(damage, hitPoint, Vector2.up);
                    DamageNumberPool.Instance.Spawn(damage, hitPoint);
                    break;
                case DamageType.Momentum:
                    Game.Instance.Player.Character.Knockback(Vector2.up);
                    Game.Instance.Player.Character.TakeMomentumHit(Random.Range(minAmount, maxAmount), hit, Vector2.up);
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
            if (animator)
            {
                animator.ResetTrigger("attack");
            }
        }
    }

    public enum DamageType
    {
        None,
        Damage,
        Momentum,
    }
}
