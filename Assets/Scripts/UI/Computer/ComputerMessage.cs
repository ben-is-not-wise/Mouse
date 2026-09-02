using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HackedDesign
{
    public class ComputerMessage : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Image icon;

        private DialogLine line;

        public DialogLine Line { get =>line; set
            {
                line = value;
                titleText.text = line.Subject;
                icon.color = line.Read.HasValue && line.Read.Value ? Color.grey : Color.white;
                titleText.color = line.Read.HasValue && line.Read.Value ? Color.grey : Color.white;
            }
        }

        public UnityAction<DialogLine> clickEvent;

        public void Click()
        {
            Debug.Log($"Clicked message {Line.Subject}");
            clickEvent.Invoke(Line);
        }

    }
}
