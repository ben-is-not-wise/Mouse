#nullable enable
using System.Collections;
using UnityEngine;

namespace HackedDesign
{
    public class FX: MonoBehaviour
    {
        private const float MinimumDuration = 0.1f;

        [SerializeField] private ParticleSystem? particleSystem;
        [SerializeField] private Animator? animator;
        [SerializeField] private FXType fxType;

        private Coroutine? despawnRoutine;
        private float particleDuration;
        private int[] stateHashes = System.Array.Empty<int>();
        private float[] clipLengths = System.Array.Empty<float>();

        private void Awake()
        {
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                particleDuration = main.duration + main.startLifetime.constantMax;
            }

            if (animator != null && animator.runtimeAnimatorController != null)
            {
                var clips = animator.runtimeAnimatorController.animationClips;
                stateHashes = new int[clips.Length];
                clipLengths = new float[clips.Length];
                for (int i = 0; i < clips.Length; i++)
                {
                    stateHashes[i] = Animator.StringToHash(clips[i].name);
                    clipLengths[i] = clips[i].length;
                }

                if (clips.Length == 0)
                {
                    Debug.LogWarning($"{name}: animator controller has no states, FX will not animate", this);
                }
            }
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            if (despawnRoutine != null)
            {
                StopCoroutine(despawnRoutine);
                despawnRoutine = null;
            }

            this.gameObject.SetActive(true);
            this.transform.position = position;
            this.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            float duration = particleDuration;

            if (animator != null && stateHashes.Length > 0)
            {
                int index = Random.Range(0, stateHashes.Length);
                animator.Play(stateHashes[index], 0, 0f);
                duration = Mathf.Max(duration, clipLengths[index]);
            }

            despawnRoutine = StartCoroutine(DespawnAfter(Mathf.Max(duration, MinimumDuration)));
        }

        public void Despawn()
        {
            if (despawnRoutine != null)
            {
                StopCoroutine(despawnRoutine);
                despawnRoutine = null;
            }

            if (particleSystem != null)
            {
                particleSystem.Stop();
            }

            gameObject.SetActive(false);
        }

        private IEnumerator DespawnAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            despawnRoutine = null;
            Despawn();
        }

        public bool Playing => despawnRoutine != null;

        public FXType FxType { get => this.fxType; set => this.fxType = value; }
    }

    public enum FXType
    {
        Blood,
        EnvHit,
        Machine,
        FragGrenade,
        ElectricGrenade
    }
}
