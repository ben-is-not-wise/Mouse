using HackedDesign.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign.UI
{
    public class DialogPresenter : AbstractPresenter
    {
        [SerializeField] private DialogManager dialogManager;
        [SerializeField] private Typewriter typewriter;
        [SerializeField] private UnityEngine.UI.Text nameText;
        [SerializeField] private UnityEngine.UI.Text dialogText;
        [SerializeField] private HackedDesign.UI.AvatarPresenter avatar;
        [SerializeField] private Sprite defaultAvatar;
        [HideInInspector] public UnityEvent finishedEvent;

        private int currentPage = 0;

        private void Awake()
        {
            this.AutoBind(ref typewriter);
            typewriter.Text = dialogText;
        }

        public override void Repaint()
        {
            if (dialogManager.CurrentDialog != null && dialogManager.CurrentDialog[currentPage] != null)
            {
                var page = dialogManager.CurrentDialog[currentPage];

                if(!string.IsNullOrWhiteSpace(page.SetFlag))
                {
                    Game.Instance.GameData.AddFlag(page.SetFlag);
                }

                dialogText.text = page.Text;
                avatar.SetByName(page.Speaker);
                nameText.text = page.Speakertitle;
                typewriter.Play(page.Text);
            }
            else
            {
                Debug.LogWarning("Unknown dialog", this);
                dialogText.text = "";
                avatar.Set(AvatarPresenter.Gender.Unknown, 0, 0);
            }
        }

        public void NextClick()
        {
            currentPage++;

            Debug.Log("Page " + currentPage + "/" + dialogManager.CurrentDialog.Count, this);

            if (currentPage >= dialogManager.CurrentDialog.Count)
            {
                Debug.Log("Hide", this);
                currentPage = 0;
                Hide();
                finishedEvent.Invoke();
            }
            else
            {
                Repaint();
            }
        }

        public void SkipClick()
        {
            Debug.Log("Skip", this);
            currentPage = 0;
            Hide();
            finishedEvent.Invoke();
        }
    }
}
