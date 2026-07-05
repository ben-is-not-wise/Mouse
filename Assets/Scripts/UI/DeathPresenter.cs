using HackedDesign.UI;
using System;
using UnityEngine;

namespace HackedDesign.UI
{
    public class DeathPresenter : AbstractPresenter
    {
        public override void Repaint()
        {

        }

        public void RestartClick()
        {
            if(Game.Instance.GameData.FinishedTutorial)
            {
                Game.Instance.SetStateLoadLevel();
            }
            else
            {
                Game.Instance.SetStateAct0LoadTutorialLevel();
            }
            
        }

        public void ExitClick()
        {
            Game.Instance.SetStateMainMenu();
        }
    }
}
