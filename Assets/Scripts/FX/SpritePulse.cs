using UnityEngine;

namespace HackedDesign
{
    public class SpritePulse : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer? spriteRenderer;
        [SerializeField] private float minAlpha = 0.35f;
        [SerializeField] private float maxAlpha = 0.85f;
        [SerializeField] private float speed = 1.0f;

        private void Awake()
        {
            if (this.spriteRenderer == null)
            {
                this.spriteRenderer = this.GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (this.spriteRenderer == null)
            {
                return;
            }

            float t = (Mathf.Sin(Time.time * this.speed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(this.minAlpha, this.maxAlpha, t);
            Color color = this.spriteRenderer.color;
            color.a = alpha;
            this.spriteRenderer.color = color;
        }
    }
}
