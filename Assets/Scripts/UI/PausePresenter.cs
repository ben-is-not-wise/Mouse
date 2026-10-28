using HackedDesign.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HackedDesign.UI
{
    public class PausePresenter : AbstractPresenter
    {
        [SerializeField] private OperatingSystem os;
        [SerializeField] private RectTransform contentsTasks;
        [SerializeField] private RectTransform contentsInterface;
        [SerializeField] private RectTransform contentsStash;
        [SerializeField] private RectTransform contentsMemory;
        [SerializeField] private RectTransform contentsMusic;
        [SerializeField] private RectTransform contentsShutdown;
        [SerializeField] private Image profilePic;
        [SerializeField] private Text nameText;
        [SerializeField] private Text messageText;

        [SerializeField] private ComputerMessage messageItemPrefab;

        [SerializeField] private Transform messagesParent;


        private PauseState state = PauseState.None;
        

        public event Action Continue;
        public event Action Exit;

        void OnEnable()
        {
            os.changeActions += Repaint;
        }

        void OnDisable()
        {
            os.changeActions -= Repaint;
        }

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

            contentsTasks.gameObject.SetActive(os.CurrentTab == OSTab.Tasks);
            contentsInterface.gameObject.SetActive(os.CurrentTab == OSTab.Body);
            contentsStash.gameObject.SetActive(os.CurrentTab == OSTab.Stash);
            contentsMemory.gameObject.SetActive(os.CurrentTab == OSTab.Data);
            contentsMusic.gameObject.SetActive(os.CurrentTab == OSTab.Music);
            contentsShutdown.gameObject.SetActive(os.CurrentTab == OSTab.Shutdown);
            
            //ShowMessages();

        }

        //private void ClearMessages()
        //{
        //    for (int i = 0; i < messagesParent.childCount; i++)
        //    {
        //        messagesParent.GetChild(i).gameObject.SetActive(false);
        //        Destroy(messagesParent.GetChild(i).gameObject);
        //    }
        //}

        //private void ShowMessages()
        //{
        //    var messages = DialogManager.Instance.CurrentMessages;

        //    ClearMessages();

        //    if (messages != null)
        //    {
        //        foreach (var message in messages)
        //        {
        //            var m = Instantiate(messageItemPrefab, messagesParent);
        //            m.Line = message;
        //            m.clickEvent += ClickMessage;
        //        }
        //    }
        //}

        //private void ShowMessage(DialogLine line)
        //{
        //    profilePic.sprite = DialogManager.Instance.GetSpeakerSprite(line);
        //    nameText.text = line.Speakertitle;
        //    messageText.text = line.Text;
        //}

        public void ClickMessage(DialogLine line)
        {
            Debug.Log($"Click message passthrough {line.Subject}");
            //ShowMessage(line);
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

        public void OnExit() => Exit?.Invoke();

        public enum PauseState
        {
            None,
            Boom,
            Messages
        }
    }

    
}
