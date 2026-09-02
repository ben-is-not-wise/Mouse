using HackedDesign.UI;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign.UI
{
    public class ActPresenter : AbstractPresenter
    {
        [HideInInspector] public UnityEvent finishedEvent;


        public void NextClick()
        {
            finishedEvent.Invoke();
        }
    }
}
