#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    // A tab in a NEURO-SHELL window: plain label, pink underline when it is the current tab.
    // Listens to the OperatingSystem itself, so OSPresenter doesn't need to know about tabs.
    // Set the Button's Transition to None; this script handles all the visual states.
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private OperatingSystem? os;
        [SerializeField] private OSTab tab;

        [Header("Parts")]
        [SerializeField] private Text? label;
        [SerializeField] private Graphic? underline;

        [Header("Colours")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.55f, 0.6f, 1f);

        private Button? button;
        private ChromaticSplit? split;
        private bool hovered;
        private bool wasActive;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Click);
            if (label != null)
            {
                split = label.GetComponent<ChromaticSplit>();
            }
        }

        void OnEnable()
        {
            if (os != null)
            {
                os.changeActions += Refresh;
            }
            wasActive = IsActive;
            Refresh();
        }

        void OnDisable()
        {
            if (os != null)
            {
                os.changeActions -= Refresh;
            }
        }

        private bool IsActive => os != null && os.CurrentTab == tab;

        private void Click()
        {
            if (os != null)
            {
                os.CurrentTab = tab;
            }
        }

        private void Refresh()
        {
            var active = IsActive;

            if (underline != null)
            {
                underline.enabled = active;
            }

            if (label != null)
            {
                label.color = active ? activeColor : hovered ? hoverColor : inactiveColor;
            }

            // Optional: if the label has a ChromaticSplit, only the current tab gets the split,
            // and it glitches wider for a moment when the tab becomes current.
            if (split != null)
            {
                split.enabled = active;
                if (active && !wasActive)
                {
                    split.Glitch();
                }
            }

            wasActive = active;
        }

        public void OnPointerEnter(PointerEventData eventData) { hovered = true; Refresh(); }
        public void OnPointerExit(PointerEventData eventData) { hovered = false; Refresh(); }
        public void OnSelect(BaseEventData eventData) { hovered = true; Refresh(); }
        public void OnDeselect(BaseEventData eventData) { hovered = false; Refresh(); }
    }
}
