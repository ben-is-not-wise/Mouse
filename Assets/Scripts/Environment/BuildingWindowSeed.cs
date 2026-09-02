using UnityEngine;

namespace HackedDesign
{
    // Gives this building's BuildingWindows-shader material a stable, per-instance random
    // seed so different buildings sharing the same material don't flicker in sync. Set once
    // at spawn via a MaterialPropertyBlock (not driven by transform position, which would
    // change every frame on parallax layers and make the flicker recompute constantly).
    //
    // Also carries every other per-building tuning value the shader exposes (grid layout,
    // flicker timing, colors) via the same MaterialPropertyBlock, so buildings that need
    // different values - e.g. dimmer/bluer parallax layers for atmospheric depth - can still
    // share one material instead of needing their own material asset. MaterialPropertyBlocks
    // don't break SRP Batcher the way separate materials do.
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class BuildingWindowSeed : MonoBehaviour
    {
        static readonly int SeedId = Shader.PropertyToID("_Seed");
        static readonly int GridOffsetId = Shader.PropertyToID("_GridOffset");
        static readonly int WindowSizeId = Shader.PropertyToID("_WindowSize");
        static readonly int WindowGapId = Shader.PropertyToID("_WindowGap");
        static readonly int MinIntervalId = Shader.PropertyToID("_MinInterval");
        static readonly int MaxIntervalId = Shader.PropertyToID("_MaxInterval");
        static readonly int FlickerChanceId = Shader.PropertyToID("_FlickerChance");
        static readonly int FlickerProbabilityId = Shader.PropertyToID("_FlickerProbability");
        static readonly int QuickFlickChanceId = Shader.PropertyToID("_QuickFlickChance");
        static readonly int KeyToleranceId = Shader.PropertyToID("_KeyTolerance");
        static readonly int BuildingColorId = Shader.PropertyToID("_BuildingColor");
        static readonly int WindowColorAId = Shader.PropertyToID("_WindowColorA");
        static readonly int WindowColorBId = Shader.PropertyToID("_WindowColorB");

        [SerializeField] SpriteRenderer spriteRenderer;

        [Header("Grid")]
        [SerializeField] Vector2 gridOffset = new Vector2(24, 24);
        [SerializeField] Vector2 windowSize = new Vector2(64, 32);
        [SerializeField] Vector2 windowGap = new Vector2(12, 12);

        [Header("Flicker")]
        [SerializeField] float minInterval = 2f;
        [SerializeField] float maxInterval = 8f;
        [SerializeField, Range(0, 1)] float flickerChance = 0.6f;
        [SerializeField, Range(0, 1)] float flickerProbability = 0.3f;
        [SerializeField, Range(0, 1)] float quickFlickChance = 0.1f;
        [SerializeField, Range(0, 1)] float keyTolerance = 0.05f;

        [Header("Color")]
        [SerializeField] Color buildingColor = Color.black;
        [SerializeField] Color windowColorA = new Color(0.04f, 0.0298f, 0.0102f);
        [SerializeField] Color windowColorB = new Color(1f, 0.9402f, 0.8f);

        private void Awake()
        {
            this.AutoBind(ref spriteRenderer);

            // Randomise the seed once per instance, play mode only - OnValidate/OnEnable
            // (which also run in edit mode via [ExecuteAlways], to preview inspector edits)
            // must NOT touch the seed, or every recompile/selection would re-roll it.
            if (Application.isPlaying)
            {
                ApplyPropertyBlock(new Vector4(Random.value * 1000f, Random.value * 1000f, 0f, 0f));
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                ApplyPropertyBlock(Vector4.zero);
            }
        }

        private void OnValidate()
        {
            // Only preview in edit mode - in play mode this would stomp the per-instance
            // random seed set in Awake() every time a field is touched.
            if (!Application.isPlaying)
            {
                ApplyPropertyBlock(Vector4.zero);
            }
        }

        private void ApplyPropertyBlock(Vector4 seed)
        {
            this.AutoBind(ref spriteRenderer);
            if (spriteRenderer == null)
            {
                return;
            }

            var mpb = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(mpb);
            mpb.SetVector(SeedId, seed);
            mpb.SetVector(GridOffsetId, gridOffset);
            mpb.SetVector(WindowSizeId, windowSize);
            mpb.SetVector(WindowGapId, windowGap);
            mpb.SetFloat(MinIntervalId, minInterval);
            mpb.SetFloat(MaxIntervalId, maxInterval);
            mpb.SetFloat(FlickerChanceId, flickerChance);
            mpb.SetFloat(FlickerProbabilityId, flickerProbability);
            mpb.SetFloat(QuickFlickChanceId, quickFlickChance);
            mpb.SetFloat(KeyToleranceId, keyTolerance);
            mpb.SetColor(BuildingColorId, buildingColor);
            mpb.SetColor(WindowColorAId, windowColorA);
            mpb.SetColor(WindowColorBId, windowColorB);
            spriteRenderer.SetPropertyBlock(mpb);
        }
    }
}
