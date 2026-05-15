using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Act0Room1Behaviour: MonoBehaviour, ICutscene
    {
        public void ExitRoomInteract() => Game.Instance.SetStateAct0Roof();

        public void Play() 
        {
            DialogManager.Instance.ShowDialog("intro_room1", Dialog1End);
        }

        public void Stop() 
        {
            DialogManager.Instance.HideDialog();
        }

        public void Dialog1End()
        {
        }
    }
}
