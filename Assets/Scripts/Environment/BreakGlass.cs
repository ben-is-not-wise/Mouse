using UnityEngine;

namespace HackedDesign
{
    [RequireComponent(typeof(Collider2D))]
    public class BreakGlass: MonoBehaviour
    {
        [SerializeField] new private Collider2D collider;
        [SerializeField] private ParticleSystem glassBreakEffect;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool breakable = true;
        [SerializeField] private bool playOnce = true;
        
        private bool hasPlayed = false;

        private void Awake()
        {
            collider = GetComponent<Collider2D>();
        }

        public void Break(Vector3 other)
        {
            if (!breakable || hasPlayed)
            {
                return;
            }

            var x = transform.position.x - other.x;

            glassBreakEffect.transform.right = x > 0 ? Vector3.right : Vector3.left;

            glassBreakEffect.Play();
            spriteRenderer.enabled = false;
            hasPlayed = true;
            collider.enabled = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(!breakable)
            {
                return;
            }

            if(collision.gameObject.CompareTag(Tags.Player) && !(playOnce && hasPlayed))
            {
                if (collision.relativeVelocity.magnitude > Game.Instance.GameSettings.ShatterMagnitude)
                {
                    Break(collision.gameObject.transform.position);
                }
            }
        }
    }
}
