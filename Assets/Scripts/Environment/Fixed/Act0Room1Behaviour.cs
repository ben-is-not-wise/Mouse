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
        private IGame game;

        public void ExitRoomInteract()
        {
            if (game.GameData.GameFlags.Contains(GameFlags.AcceptBossMission))
            {
                Debug.Log("exit");
                game.SetStateAct0Roof();
            }
            else
            {
                DialogManager.Instance.ShowDialog("intro_room1door");
            }
        }

        public void Play(IGame game)
        {
            this.game = game;
            game.DialogManager.ShowDialog("intro_room1", () => { });
        }

        public void AirconEvent()
        {
            game.DialogManager.ShowDialog("intro_room1aircon", () => { });
        }

        public void ComputerEvent()
        {
            game.SetStatePaused();
        }

        public void BedEvent()
        {
            game.DialogManager.ShowDialog("intro_room1bed", () => { });
        }

        public void CatEvent()
        {
            game.DialogManager.ShowDialog("intro_room1cat", () => { });
        }

        public void CoffeeEvent()
        {
            game.DialogManager.ShowDialog("intro_room1coffee", () => { });
        }

        public void Stop(IGame game) => game.DialogManager.HideDialog();

    }
}
