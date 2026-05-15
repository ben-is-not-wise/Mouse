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
            Game.Instance.SetStateLoadLevel();
        }

        public void ExitClick()
        {
            Game.Instance.SetStateMainMenu();
        }
    }
}
