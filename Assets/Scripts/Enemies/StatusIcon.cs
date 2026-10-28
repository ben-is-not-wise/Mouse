#nullable enable
using System;
using UnityEngine;

namespace HackedDesign
{
    public class StatusIcon : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer? sprite;
        [SerializeField] private Sprite? alertSprite;
        [SerializeField] private Sprite? searchingSprite;
        [SerializeField] private Sprite? pickupSprite;
        [SerializeField] private Sprite? talkSprite;
        [SerializeField] private Sprite? interactSprite;

        [SerializeField] private Sprite? exitSprite;
        [SerializeField] private bool showDefault = false;
        [SerializeField] private Color alertColor = new(1f, 0.2f, 0.2f, 1f);

        private static readonly int FillId = Shader.PropertyToID("_Fill");
        private MaterialPropertyBlock? propertyBlock;
        private float fill = 0f;
        private bool showingFill = false;

        public bool IsHidden => sprite == null || sprite.sprite == null;

        public float Fill
        {
            get => this.fill;
            set
            {
                this.fill = Mathf.Clamp01(value);
                if (this.showingFill)
                {
                    ApplyFill(this.fill);
                }
            }
        }

        void Awake()
        {
            this.AutoBind(ref sprite);
            sprite.Require(nameof(sprite));
            Hide();
            if(showDefault)
            {
                Talk();
            }
          
        }

        public void Hide() => Show(null, false, Color.white);

        public void Talk() => Show(talkSprite, false, Color.white);

        public void Interact() => Show(interactSprite, false, Color.white);

        public void Exit() => Show(exitSprite, false, Color.white);

        public void Alert() => Show(alertSprite, false, alertColor);

        public void Searching() => Show(searchingSprite, true, Color.white);

        public void Pickup() => Show(pickupSprite, false, Color.white);

        private void Show(Sprite? icon, bool useFill, Color color)
        {
            sprite!.sprite = icon;
            sprite.color = color;
            this.showingFill = useFill;
            ApplyFill(useFill ? this.fill : 1f);
        }

        private void ApplyFill(float value)
        {
            this.propertyBlock ??= new MaterialPropertyBlock();
            sprite!.GetPropertyBlock(this.propertyBlock);
            this.propertyBlock.SetFloat(FillId, value);
            sprite.SetPropertyBlock(this.propertyBlock);
        }
    }
}
