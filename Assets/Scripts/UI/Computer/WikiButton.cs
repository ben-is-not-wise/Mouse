using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class WikiButton: Button
    {
        [SerializeField] private Text leftLabel;
        [SerializeField] private Text rightLabel;
        [SerializeField] private Color activeColor = Color.white;

        private bool active;

        public bool Active
        {
            get => this.active;
            set
            {
                this.active = value;
                this.DoStateTransition(this.currentSelectionState, false);
            }
        }

        public void Set(string text, int discovered, int max)
        {
            leftLabel.text = text;
            rightLabel.text = $"{discovered}/{max}";
        }

        public void Set(string text)
        {
            leftLabel.text = text;
            rightLabel.text = string.Empty;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (this.active && (state == SelectionState.Normal || state == SelectionState.Selected) && this.transition == Transition.ColorTint && this.targetGraphic != null)
            {
                this.targetGraphic.CrossFadeColor(this.activeColor * this.colors.colorMultiplier, instant ? 0 : this.colors.fadeDuration, true, true);
                return;
            }

            base.DoStateTransition(state, instant);
        }
    }
}
