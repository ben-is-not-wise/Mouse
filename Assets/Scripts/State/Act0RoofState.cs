using HackedDesign.UI;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Act0RoofState : IState
    {
        private const string DialogId = "intro_roof1";
        //private const string LevelName = "Rooftop";
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IDialogManager dialog;

        public bool PlayerActionAllowed => true;
        public bool Battle => false;

        public Act0RoofState(IPlayerController player, ILevelManager level, IDialogManager dialog)
        {
            this.player = player;
            this.level = level;
            this.dialog = dialog;
        }

        public void Begin()
        {
            level.ShowNamedRoom(NamedLevels.Rooftop, true, false, player);
            player.Character.ExecuteCommand(new FacingCommand(0, 1f));
            //player.Character.SetIdleState();
            player.Character.SetSitState();
            
            dialog.ShowDialog(DialogId, new UnityAction(DialogOver));
        }

        private void DialogOver()
        {
            Debug.Log("Dialog over roof state");
            Game.Instance.SetStateAct0Room2();
        }

        public void End() => dialog.HideDialog();

        public void Update() => player.UpdateSitBehaviour();

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