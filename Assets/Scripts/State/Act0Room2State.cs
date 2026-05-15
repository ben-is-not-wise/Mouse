using HackedDesign.UI;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public class Act0Room2State : IState
    {
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IDialogManager dialog;

        public bool PlayerActionAllowed => true;
        public bool Battle => false;


        public Act0Room2State(IPlayerController player, ILevelManager level, IDialogManager dialog)
        {
            this.player = player;
            this.level = level;
            this.dialog = dialog;
        }

        public void Begin()
        {
            level.Reset();
            level.ShowCutscene(Cutscenes.MouseStartingRoom2, true, true, player);
            player.Character.Shadow.enabled = false;
            player.Character.SetStateIdle();
            dialog.ShowDialog("intro_room1", Dialog1End);
        }

        public void End() => player.Character.Shadow.enabled = true;

        public void Dialog1End()
        {

        }

        public void Update() => player.UpdateIdleBehaviour();

        public void FixedUpdate() => player.FixedUpdateBehaviour();

        public void LateUpdate() => player.LateUpdateBehaviour();

        public void Menu()
        {
            //GameManager.Instance.SetStartMenu();
        }

        public void Select()
        {

        }
    }
}