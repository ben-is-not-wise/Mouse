using UnityEngine;
using TMPro;
using System.Collections;

namespace HackedDesign.UI.DamageNumbers
{
    public class DamageNumber: MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        private Rigidbody2D rb;

        private int number;
        private Coroutine lifetimeCoroutine;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Show(int number, Vector2 start)
        {
            this.number = number;
            text.text = number.ToString();
            this.transform.position = start;
            rb.AddForce(Random.onUnitCircle * (number/20), ForceMode2D.Impulse);
            this.gameObject.SetActive(true);
            lifetimeCoroutine = StartCoroutine(End());
        }

        private IEnumerator End()
        {
            yield return new WaitForSeconds(number / 25);
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
    }
}
