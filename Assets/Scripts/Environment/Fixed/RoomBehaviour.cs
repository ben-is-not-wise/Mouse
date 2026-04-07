using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class RoomBehaviour: MonoBehaviour
    {
        public void ExitRoom1Interact() => Game.Instance.SetStateAct0Roof();
        public void ExitRoom2Interact() => Game.Instance.SetStateAct0Loading();
    }
}
