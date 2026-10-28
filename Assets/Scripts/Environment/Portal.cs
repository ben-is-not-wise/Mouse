using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Portal: MonoBehaviour
    {
        [SerializeField] SpriteRenderer portalArrowDown;
        [SerializeField] SpriteRenderer portalArrowUp;
        [SerializeField] BoxCollider2D trigger;
        [SerializeField] public bool down = true;
        [SerializeField] private Portal connectedPortal;

        [SerializeField] private UnityEvent triggerEvent;


        public Vector3 Destination => connectedPortal.transform.position;

        void Awake()
        {
            if(trigger != null)
            {
                trigger.isTrigger = true;
            }

            ShowArrow(false);
        }

        public void Trigger()
        {
            triggerEvent.Invoke();
        }
        
        public void ShowArrow(bool flag)
        {
            portalArrowDown?.gameObject.SetActive(down && flag);
            portalArrowUp?.gameObject.SetActive(!down && flag);
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.CompareTag(Tags.Player))
            {
                ShowArrow(true);
                collision.gameObject.TryGetComponent<IPlayerController>(out var player);
                if(player != null)
                {
                    player.SetPortal(this);
                }
                    
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Tags.Player))
            {
                ShowArrow(false);
                collision.gameObject.TryGetComponent<IPlayerController>(out var player);
                if (player != null)
                {
                    player.SetPortal(null);
                }
            }
        }

    }
}
