using EPOOutline;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HackedDesign
{
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private Outlinable outlinable;
        [SerializeField] public UnityEvent interactAction;
        [SerializeField] private OperatingSystem os;
        [SerializeField] private string label;

        private bool interact = false;
        private bool target = false;
        private bool ping = false;

        private float pingTimer = 0;

        public string Label { get => label; set => label = value; }

        public OperatingSystem OS => os;

        void Awake()
        {
            this.AutoBind(ref outlinable);
            this.AutoBind(ref os);
            Target(false);

            FixTag();
        }

        private void FixTag()
        {
            if (this.gameObject.CompareTag("Untagged"))
            {
                this.gameObject.tag = "Interactable";
            }
        }

        public void Ping()
        {
            pingTimer = Time.time;
            ping = true;
        }

        public void TriggerInteract()
        {

            Debug.Log("Invoke interact");
            interactAction?.Invoke();

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

                if ((Game.Instance.Player.transform.position - this.transform.position).magnitude < 2.5f)
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
                if ((Game.Instance.Player.transform.position - this.transform.position).magnitude < 2.5f)
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
            target = flag;

            //outlinable.enabled = flag;
            //outlinable.OutlineParameters.Color = Color.white;
        }

        public void Interact(bool flag)
        {
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
    }
}