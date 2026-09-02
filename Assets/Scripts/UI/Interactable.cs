using EPOOutline;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HackedDesign
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(EventTrigger))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private Outlinable outlinable;
        [SerializeField] public UnityEvent interactAction;
        [SerializeField] private OperatingSystem os;
        [SerializeField] private string label;
        [SerializeField] private bool repeatable = true;
        [SerializeField] private string gameFlag = "";
        [SerializeField] private InteractionType interactionType = InteractionType.None;

        private bool interact = false;
        private bool target = false;
        private bool ping = false;
        private StatusIcon statusIcon;

        private bool touched = false;

        private float pingTimer = 0;

        private Transform playerTransform;

        public string Label { get => label; set => label = value; }

        public OperatingSystem OS => os;

        public InteractionType InteractionType => interactionType;

        void Awake()
        {
            this.AutoBind(ref outlinable);
            this.AutoBind(ref os);
            statusIcon = GetComponentInChildren<StatusIcon>();
            Target(false);
            FixTag();
            SetStatusIcon();

            var eventTrigger = gameObject.GetComponent<EventTrigger>();

            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<EventTrigger>();
            }

            var click = new EventTrigger.Entry();
            click.eventID = EventTriggerType.PointerClick;

            click.callback.AddListener((data) => TriggerInteract());

            eventTrigger.triggers.Add(click);

            var enter = new EventTrigger.Entry();
            enter.eventID = EventTriggerType.PointerEnter;

            enter.callback.AddListener((data) => Target(true));

            eventTrigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry();
            exit.eventID = EventTriggerType.PointerExit;

            exit.callback.AddListener((data) => Target(false));

            eventTrigger.triggers.Add(exit);

        }

        private void FixTag()
        {
            if (this.gameObject.CompareTag("Untagged"))
            {
                this.gameObject.tag = "Interactable";
            }
        }

        private void SetStatusIcon()
        {

            if (statusIcon != null)
            {
                switch (interactionType)
                {
                    case InteractionType.None:
                        statusIcon.Hide();
                        break;
                    case InteractionType.Talk:
                        statusIcon.Talk();
                        break;
                    case InteractionType.Use:
                        statusIcon.Interact();
                        break;
                    case InteractionType.Exit:
                        statusIcon.Exit();
                        break;
                }
            }
        }

        public void Ping()
        {
            pingTimer = Time.time;
            ping = true;
        }

        public void TriggerInteract()
        {
            Debug.Log("Trigger Interact", this);

            if (!CanTrigger())
            {
                return;
            }

            touched = true;

            if (!string.IsNullOrEmpty(gameFlag))
            {
                Game.Instance.GameData.GameFlags.Add(gameFlag);
            }

            interactAction?.Invoke();

            if(!repeatable && statusIcon != null)
            {
                statusIcon.Hide();
            }
        }

        private void Update()
        {
            if (pingTimer + 4 < Time.time)
            {
                ping = false;
            }

            if (interact)
            {
                outlinable.enabled = true;

                if (PlayerInRange())
                {
                    //FDAC3D
                    ColorUtility.TryParseHtmlString("FDAC3D", out var color);
                    outlinable.OutlineParameters.Color = color;
                }
                else
                {
                    outlinable.OutlineParameters.Color = Color.grey;
                }
            }
            else if (ping)
            {
                outlinable.enabled = true;
                outlinable.OutlineParameters.Color = Color.magenta;
            }
            else if (target)
            {
                outlinable.enabled = true;
                if (PlayerInRange())
                {
                    ColorUtility.TryParseHtmlString("#FDAC3D", out var color);
                    outlinable.OutlineParameters.Color = color;
                }
                else
                {

                    outlinable.OutlineParameters.Color = Color.grey;
                }
            }
            else
            {
                outlinable.enabled = false;
            }
        }

        public void Target(bool flag)
        {
            if(!CanTrigger())
            {
                return;
            }

            target = flag;
        }

        public void Interact(bool flag)
        {

            if (!CanTrigger())
            {
                return;
            }

            interact = flag;

            if (flag)
            {
                //enterInteractAction?.Invoke();
            }
            else
            {
                //exitInteractAction?.Invoke();
            }
            //outlinable.enabled = flag;
            //outlinable.OutlineParameters.Color = Color.yellow;
        }

        // FIXME: This polls the player's distance every frame to tint the outline, and lazily
        // caches the player transform from the Game singleton. The cleaner approach is to
        // self-generate a trigger CircleCollider2D (radius 2.5) on startup, on a child object
        // whose layer the UI Physics2DRaycaster ignores, and flip a 'playerInRange' bool from
        // OnTriggerEnter2D/OnTriggerExit2D forwarded off that child. That removes both the
        // per-frame distance check and the Game.Instance reach, with no prefab edits.
        private bool PlayerInRange()
        {
            if (playerTransform == null)
            {
                playerTransform = Game.Instance.Player.transform;
            }

            return (playerTransform.position - transform.position).magnitude < 2.5f;
        }

        private bool CanTrigger() => repeatable || !touched;
        
    }

    public enum InteractionType
    {
        None,
        Talk,
        Use,
        Attack,
        Exit
    }
}