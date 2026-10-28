using UnityEngine;

namespace HackedDesign
{
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class AdBlockGlitch : MonoBehaviour
    {
        private const string MaterialName = "AdBlockGlitch";
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        [SerializeField] private SpriteRenderer spriteRenderer;

        void Awake()
        {
            this.AutoBind(ref spriteRenderer);
            if (Application.isPlaying)
            {
                this.ApplySeed(new Vector4(Random.value * 1000f, Random.value * 1000f, 0f, 0f));
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                this.ApplySeed(Vector4.zero);
            }
        }

        private void ApplySeed(Vector4 seed)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var block = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(block);
            block.SetVector(SeedId, seed);
            spriteRenderer.SetPropertyBlock(block);
        }

#if UNITY_EDITOR
        void Reset()
        {
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            foreach (var guid in UnityEditor.AssetDatabase.FindAssets($"t:Material {MaterialName}"))
            {
                var material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (material != null && material.name == MaterialName)
                {
                    UnityEditor.Undo.RecordObject(spriteRenderer, "Apply Ad Block Glitch");
                    spriteRenderer.sharedMaterial = material;
                    return;
                }
            }

            Debug.LogWarning($"Couldn't find the {MaterialName} material to apply.", this);
        }
#endif
    }
}
