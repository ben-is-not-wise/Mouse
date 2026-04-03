#nullable enable
using System.Collections;
using UnityEngine;

namespace HackedDesign
{
    public class Ghost : MonoBehaviour
    {
        [Header("Ghost Settings")]
        public float spawnInterval = 0.1f;
        public float ghostLifetime = 0.5f;
        public Color ghostColor = new Color(1, 1, 1, 0.4f);
        public Material? ghostMaterial;

        [Header("References")]
        [SerializeField] private Transform? ghostParent;
        [SerializeField] private SpriteRenderer? targetRenderer;

        private bool spawning = false;

        public void StartTrail()
        {
            if (!spawning)
            {
                StartCoroutine(SpawnTrail());
            }
        }

        public void StopTrail()
        {
            spawning = false;
        }

        private IEnumerator SpawnTrail()
        {
            spawning = true;

            while (spawning)
            {
                if (targetRenderer != null)
                {
                    SpawnGhost();
                }

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnGhost()
        {
            GameObject ghost = new GameObject("GhostTrail");
            ghost.transform.parent = ghostParent;
            SpriteRenderer sr = ghost.AddComponent<SpriteRenderer>();

            sr.sprite = targetRenderer!.sprite;
            sr.flipX = targetRenderer.flipX;
            sr.flipY = targetRenderer.flipY;
            sr.transform.position = targetRenderer.transform.position;
            sr.transform.rotation = targetRenderer.transform.rotation;
            sr.transform.localScale = targetRenderer.transform.localScale;

            sr.sortingLayerID = targetRenderer.sortingLayerID;
            sr.sortingOrder = targetRenderer.sortingOrder - 1;

            sr.material = ghostMaterial.OrNull() != null ? targetRenderer.material : null;
            sr.color = ghostColor;

            ghost.AddComponent<GhostFade>().Init(ghostLifetime, ghostColor);
        }
    }

    public class GhostFade : MonoBehaviour
    {
        private float lifetime;
        private Color initialColor;

        public void Init(float duration, Color color)
        {
            lifetime = duration;
            initialColor = color;
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float time = 0f;
            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            while (time < lifetime)
            {
                float t = time / lifetime;
                sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0, t));
                time += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}