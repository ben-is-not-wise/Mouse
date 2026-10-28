using UnityEngine;

namespace HackedDesign
{
    [ExecuteAlways]
    public class WallLayout : MonoBehaviour
    {
        void Start()
        {
            this.Layout();
        }

        void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                this.Layout();
            }
        }

        private void OnTransformChildrenChanged()
        {
            this.Layout();
        }

        public void Layout()
        {
            var cursor = this.transform.position.x;

            foreach (Transform child in this.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                var renderers = child.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    continue;
                }

                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                var offset = cursor - bounds.min.x;
                if (!Mathf.Approximately(offset, 0f))
                {
                    child.position += new Vector3(offset, 0f, 0f);
                }

                cursor += bounds.size.x;
            }
        }
    }
}
