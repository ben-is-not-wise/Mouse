#nullable enable
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HackedDesign.UI
{
    public abstract class AbstractPresenter : MonoBehaviour, IPresenter
    {
        public virtual void Show()
        {
            if (!gameObject.activeInHierarchy)
            {
                if(EventSystem.current.TryGetComponent<InputSystemUIInputModule>(out var inputSystem))
                {
                    inputSystem.actionsAsset.Disable();
                    inputSystem.actionsAsset.Enable();
                }

                gameObject.SetActive(true);
            }
        }

        public virtual void Hide()
        {
            if (gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }

        public void Toggle() => gameObject.SetActive(!gameObject.activeInHierarchy);

        public virtual void Repaint() { }
    }
}