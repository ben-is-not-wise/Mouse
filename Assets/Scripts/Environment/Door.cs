using UnityEngine;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace HackedDesign
{
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Animator))]
    public class Door : MonoBehaviour
    {
        [field: SerializeField, NotNull] private Animator animator;
        [field: SerializeField, NotNull] private Interactable interactable;

        private float timerStart = 0;

        void Awake()
        {
            this.AutoBind(ref animator);
            this.AutoBind(ref interactable);
            interactable.interactAction.AddListener(Trigger);
        }

        public bool IsOpen { get; private set; } = false;

        public void Trigger()
        {
            if (!IsOpen)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        private void Open()
        {
            IsOpen = true;
            if(animator != null)
            {
                animator.SetTrigger("open");
            }
            this.timerStart = Time.time;
            StartCoroutine(AutoClose());
        }

        private void Close()
        {
            IsOpen = false;
            if (animator != null)
            {
                animator.SetTrigger("close");
            }
        }

        private IEnumerator AutoClose()
        {
            //yield return new WaitForSeconds(6);
            while (Time.time < (this.timerStart + 2))
            {
                yield return null;
            }

            Close();
        }

        void OnCollisionStay(Collision collision) => this.timerStart = Time.time;
    }
}