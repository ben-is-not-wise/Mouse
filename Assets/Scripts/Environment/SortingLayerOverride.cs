using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HackedDesign
{
    public class SortingLayerOverride : MonoBehaviour
    {
        [SerializeField, SortingLayer] private string sortingLayer = "Default";
        [SerializeField] private int orderInLayer = 0;
        [SerializeField] private SortingLayerList lightTargetSortingLayers = new();

        void Start()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingLayerName = sortingLayer;
                renderers[i].sortingOrder = orderInLayer;
            }

            var canvases = GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].renderMode != RenderMode.WorldSpace)
                {
                    continue;
                }
                canvases[i].sortingLayerName = sortingLayer;
                canvases[i].sortingOrder = orderInLayer;
            }

            if (lightTargetSortingLayers != null && lightTargetSortingLayers.names.Length > 0)
            {
                var ids = new int[lightTargetSortingLayers.names.Length];
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = SortingLayer.NameToID(lightTargetSortingLayers.names[i]);
                }

                var lights = GetComponentsInChildren<Light2D>(true);
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].targetSortingLayers = ids;
                }
            }
        }
    }
}
