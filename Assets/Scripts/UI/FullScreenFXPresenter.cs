#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class FullScreenFXPresenter : AbstractPresenter
    {
        [SerializeField] private Image overlay = null!;
        [SerializeField] private float fadeInDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        private Material? material;
        private Coroutine? running;

        private Material Material => material ??= overlay.material;

        public override void Show()
        {
            base.Show();
            SetValues(0f, 0f);
        }

        public override void Hide()
        {
            if (running != null)
            {
                StopCoroutine(running);
                running = null;
            }
            base.Hide();
        }

        public override void Repaint() { }

        public void FadeToBlack(UnityAction? onComplete = null)
        {
            Show();
            Run(Animate(0f, 1f, fadeInDuration, onComplete));
        }

        public void FadeOut(UnityAction? onComplete = null)
        {
            if (!gameObject.activeInHierarchy)
            {
                onComplete?.Invoke();
                return;
            }

            Run(Animate(1f, 0f, fadeOutDuration, () =>
            {
                Hide();
                onComplete?.Invoke();
            }));
        }

        private void Run(IEnumerator routine)
        {
            if (running != null)
            {
                StopCoroutine(running);
            }
            running = StartCoroutine(routine);
        }

        private IEnumerator Animate(float from, float to, float duration, UnityAction? onComplete)
        {
            float time = 0f;
            SetValues(from, from);

            while (time < duration)
            {
                float v = Mathf.Lerp(from, to, time / duration);
                SetValues(v, v);
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            SetValues(to, to);
            running = null;
            onComplete?.Invoke();
        }

        private void SetValues(float intensity, float fade)
        {
            Material.SetFloat(IntensityId, intensity);
            Material.SetFloat(FadeId, fade);
        }
    }
}
