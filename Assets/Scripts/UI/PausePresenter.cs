using HackedDesign.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class PausePresenter : AbstractPresenter
    {
        [SerializeField] private RectTransform contentsBoom;
        [SerializeField] private RectTransform contentsMessages;
        [SerializeField] private Image profilePic;
        [SerializeField] private Text nameText;
        [SerializeField] private Text messageText;

        [SerializeField] private ComputerMessage messageItemPrefab;

        [SerializeField] private Transform messagesParent;


        private PauseState state = PauseState.None;
        

        public event Action Continue;
        public event Action Exit;

        public void Repaint(PauseState state)
        {
            this.state = state;
            Repaint();
        }

        public override void Repaint()
        {
            RepaintContents();
        }

        public void RepaintContents()
        {
            contentsBoom.gameObject.SetActive(state == PauseState.Boom);
            contentsMessages.gameObject.SetActive(state == PauseState.Messages);
            
            ShowMessages();

        }

        private void ClearMessages()
        {
            for (int i = 0; i < messagesParent.childCount; i++)
            {
                messagesParent.GetChild(i).gameObject.SetActive(false);
                Destroy(messagesParent.GetChild(i).gameObject);
            }
        }

        private void ShowMessages()
        {
            var messages = DialogManager.Instance.CurrentMessages;

            ClearMessages();

            if (messages != null)
            {
                foreach (var message in messages)
                {
                    var m = Instantiate(messageItemPrefab, messagesParent);
                    m.Line = message;
                    m.clickEvent += ClickMessage;
                }
            }
        }

        private void ShowMessage(DialogLine line)
        {
            profilePic.sprite = DialogManager.Instance.GetSpeakerSprite(line);
            nameText.text = line.Speakertitle;
            messageText.text = line.Text;
        }

        public void ClickMessage(DialogLine line)
        {
            Debug.Log($"Click message passthrough {line.Subject}");
            ShowMessage(line);
        }

        public void MessagesClick()
        {
            state = PauseState.Messages;
            Repaint();
        }

        public void BoomClick()
        {
            state = PauseState.Boom;
            Repaint();
        }

        public void CloseClick()
        {
            Debug.Log("close click");
            state = PauseState.None;
            Repaint();
        }

        public void AcceptClick()
        {
            Game.Instance.GameData.AddFlag(GameFlags.AcceptBossMission);
            CloseClick();
        }

        public void ContinueClick() => Continue?.Invoke();

        public void ExitClick() => Exit?.Invoke();

        public enum PauseState
        {
            None,
            Boom,
            Messages
        }
    }

    
}
