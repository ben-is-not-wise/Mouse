using UnityEngine;
using TMPro;
using System.Collections;

namespace HackedDesign.UI.DamageNumbers
{
    public class DamageNumber: MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [Header("Tag")]
        [SerializeField] private float tagRisePixels = 8f;
        [SerializeField] private float tagDuration = 1f;
        [SerializeField] private float tagGlitchDuration = 0.12f;
        [SerializeField] private float tagBlinkDuration = 0.25f;
        [SerializeField] private float tagBlinkRate = 15f;
        [SerializeField] private int tagGlitchPixels = 2;
        [SerializeField] private Color tagSplitColourA = new(0.996f, 0.086f, 0.286f, 1f);
        [SerializeField] private Color tagSplitColourB = new(0.208f, 0.502f, 0.502f, 1f);

        private Rigidbody2D rb;

        private int number;
        private Coroutine lifetimeCoroutine;
        private TextMeshPro splitA;
        private TextMeshPro splitB;
        private Color numberColour;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            numberColour = text.color;
        }

        public void Show(int number, Vector2 start)
        {
            this.number = number;
            text.text = number.ToString();
            text.color = numberColour;
            text.enabled = true;
            SetSplitVisible(false);
            rb.simulated = true;
            this.transform.position = start;
            rb.AddForce(Random.onUnitCircle * (number/20), ForceMode2D.Impulse);
            this.gameObject.SetActive(true);
            lifetimeCoroutine = StartCoroutine(End());
        }

        /// <summary>A short terminal word (e.g. BREACH) that rises, glitches pink/blue in and out, then blinks out.</summary>
        public void ShowTag(string word, Color colour, Vector3 start)
        {
            text.text = word;
            text.color = colour;
            text.enabled = true;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            EnsureSplit();
            splitA.text = word;
            splitB.text = word;
            this.transform.position = Snap(start);
            this.gameObject.SetActive(true);
            lifetimeCoroutine = StartCoroutine(TagLifetime(Snap(start)));
        }

        private IEnumerator End()
        {
            yield return new WaitForSeconds(number / 25);
            ReturnToPool();
        }

        private IEnumerator TagLifetime(Vector3 start)
        {
            float startTime = Time.time;
            float elapsed = 0f;
            while (elapsed < tagDuration)
            {
                float rise = Mathf.Round(tagRisePixels * elapsed / tagDuration) / PixelBatch.PixelsPerUnit;
                this.transform.position = start + new Vector3(0f, rise, 0f);

                float remaining = tagDuration - elapsed;
                bool glitching = elapsed < tagGlitchDuration || remaining < tagBlinkDuration;
                SetSplitVisible(glitching);
                if (glitching)
                {
                    splitA.transform.localPosition = new Vector3(Random.Range(1, tagGlitchPixels + 1), Random.Range(-1, 2), 0f);
                    splitB.transform.localPosition = new Vector3(-Random.Range(1, tagGlitchPixels + 1), Random.Range(-1, 2), 0f);
                }

                text.enabled = remaining >= tagBlinkDuration || Mathf.Repeat(remaining * tagBlinkRate, 1f) >= 0.5f;
                yield return null;
                elapsed = Time.time - startTime;
            }

            ReturnToPool();
        }

        private void EnsureSplit()
        {
            if (splitA == null)
            {
                splitA = CreateSplit("SplitA", tagSplitColourA);
                splitB = CreateSplit("SplitB", tagSplitColourB);
            }
        }

        private TextMeshPro CreateSplit(string name, Color colour)
        {
            var go = new GameObject(name);
            go.transform.SetParent(this.transform, false);
            var copy = go.AddComponent<TextMeshPro>();
            copy.font = text.font;
            copy.fontSharedMaterial = text.fontSharedMaterial;
            copy.fontSize = text.fontSize;
            copy.alignment = text.alignment;
            copy.textWrappingMode = TextWrappingModes.NoWrap;
            copy.rectTransform.sizeDelta = text.rectTransform.sizeDelta;
            copy.color = colour;
            if (text.TryGetComponent<MeshRenderer>(out var source))
            {
                copy.sortingLayerID = source.sortingLayerID;
                copy.sortingOrder = source.sortingOrder - 1;
            }
            go.SetActive(false);
            return copy;
        }

        private void SetSplitVisible(bool visible)
        {
            if (splitA == null)
            {
                return;
            }

            if (splitA.gameObject.activeSelf != visible)
            {
                splitA.gameObject.SetActive(visible);
                splitB.gameObject.SetActive(visible);
            }
        }

        private static Vector3 Snap(Vector3 position) => new(
            Mathf.Round(position.x * PixelBatch.PixelsPerUnit) / PixelBatch.PixelsPerUnit,
            Mathf.Round(position.y * PixelBatch.PixelsPerUnit) / PixelBatch.PixelsPerUnit,
            position.z);

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
