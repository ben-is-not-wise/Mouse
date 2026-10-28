using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    [ExecuteAlways]
    public class PanelFrame : MonoBehaviour
    {
        [SerializeField] private RectTransform frameTopLeft;
        [SerializeField] private Text frameTitle;
        [SerializeField] private RectTransform frameTopRight;

        [SerializeField] private RectTransform frameBottomLeft;
        [SerializeField] private Text frameBottomTitle;
        [SerializeField] private RectTransform frameBottomRight;

        [SerializeField] private float notchPaddingLeft = 3f;
        [SerializeField] private float notchPaddingRight = 3f;
        [SerializeField] private float leftMargin = 5f;
        [SerializeField] private float rightMargin = 0f;

        private void OnEnable()
        {
            this.RefreshFrame();
        }

        private void OnValidate()
        {
            this.RefreshFrame();
        }

        private void OnRectTransformDimensionsChange()
        {
            this.RefreshFrame();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                this.RefreshFrame();
            }
        }

        public void SetTitle(string text)
        {
            this.frameTitle.text = text;
            this.RefreshFrame();
        }

        public void RefreshFrame()
        {
            this.RefreshNotch(this.frameTopLeft, this.frameTitle, this.frameTopRight);
            this.RefreshNotch(this.frameBottomLeft, this.frameBottomTitle, this.frameBottomRight);
        }

        private void RefreshNotch(RectTransform flexibleLeft, Text title, RectTransform flexibleRight)
        {
            if (title == null || flexibleRight == null)
            {
                return;
            }

            RectTransform titleRect = title.rectTransform;
            titleRect.anchoredPosition = new Vector2(this.leftMargin + this.notchPaddingLeft, titleRect.anchoredPosition.y);
            titleRect.sizeDelta = new Vector2(title.preferredWidth, titleRect.sizeDelta.y);

            if (flexibleLeft != null)
            {
                flexibleLeft.sizeDelta = new Vector2(this.leftMargin, flexibleLeft.sizeDelta.y);
            }

            float notchRightEdge = titleRect.anchoredPosition.x + titleRect.sizeDelta.x + this.notchPaddingRight;
            flexibleRight.offsetMin = new Vector2(notchRightEdge, flexibleRight.offsetMin.y);
            flexibleRight.offsetMax = new Vector2(-this.rightMargin, flexibleRight.offsetMax.y);
        }
    }
}
