using HackedDesign.UI;

using System;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class TargetPresenter : AbstractPresenter
    {
        [Header("UI")]
        [SerializeField] private Text nameLabel;
        [SerializeField] private Slider targetHealthbar;

        public void Repaint(Interactable interactable)
        {
            if (interactable == null)
            {
                Repaint();
                return;
            }

            Show();
            nameLabel.text = string.IsNullOrEmpty(interactable.Label) ? interactable.name : interactable.Label;
            if(interactable.OS != null)
            {
                targetHealthbar.maxValue = interactable.OS.MaxHealth;
                targetHealthbar.value = interactable.OS.Health;
            }
            else
            {
                targetHealthbar.maxValue = 1;
                targetHealthbar.value = 0;
            }
        }

        public void Repaint(string name)
        {
            nameLabel.text = name;
            targetHealthbar.value = 0;
        }

        public override void Repaint()
        {
            Hide();
            nameLabel.text = "";
        }
    }
}
