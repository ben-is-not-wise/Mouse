using UnityEngine;

namespace HackedDesign
{
    public class SortingLayerColorOverride : MonoBehaviour
    {
        private static readonly int colorPropertyId = Shader.PropertyToID("_Color");

        [SerializeField] private Material flatMaterial;
        [SerializeField] private Color color = Color.white;

        void Start()
        {
            var spriteRenderer = this.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                return;
            }

            var block = new MaterialPropertyBlock();
            spriteRenderer.sharedMaterial = flatMaterial;
            spriteRenderer.GetPropertyBlock(block);
            block.SetColor(colorPropertyId, color);
            spriteRenderer.SetPropertyBlock(block);
        }
    }
}
