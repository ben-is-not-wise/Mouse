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
        [SerializeField] private bool showDefault = false;

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

        public void Hide() => sprite!.sprite = null;

        public void Talk() => sprite!.sprite = talkSprite;

        public void Interact() => sprite!.sprite = interactSprite;

        public void Alert() => sprite!.sprite = alertSprite;

        public void Searching() => sprite!.sprite = searchingSprite;

        public void Pickup() => sprite!.sprite = pickupSprite;
    }
}
