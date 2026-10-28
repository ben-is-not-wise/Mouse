using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class MenuLightPulse : MonoBehaviour
    {
        [SerializeField] private Image? image;
        [SerializeField] private float minAlpha = 0.35f;
        [SerializeField] private float maxAlpha = 0.85f;
        [SerializeField] private float speed = 1.0f;

        private void Awake()
        {
            if (this.image == null)
            {
                this.image = this.GetComponent<Image>();
            }
        }

        private void Update()
        {
            if (this.image == null)
            {
                return;
            }

            float t = (Mathf.Sin(Time.time * this.speed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(this.minAlpha, this.maxAlpha, t);
            Color color = this.image.color;
            color.a = alpha;
            this.image.color = color;
        }
    }
}
