using HackedDesign.UI;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class IntermissionState : IState
    {
        private readonly IGame game;
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IPresenter actionPresenter;
        private readonly IDialogManager dialog;

        public bool PlayerActionAllowed => true;
        public bool Battle => false;


        public IntermissionState(IGame game, IPlayerController player, ILevelManager level, IDialogManager dialog, IPresenter actionPresenter)
        {
            this.game = game;
            this.player = player;
            this.level = level;
            this.dialog = dialog;
            this.actionPresenter = actionPresenter;
        }

        public void Begin()
        {
            
            level.Clear();
            level.ShowCutscene("Hotdog Stand", false, true, player);
            player.Character.ExecuteCommand(new FacingCommand(0, 1f));
            player.Character.SetStateSitting();

            dialog.ShowDialog("intro_intermission1", new UnityAction(Intro1Over));
        }

        public void Intro1Over()
        {
            Debug.Log("Intro1 over");

            dialog.ShowDialog("intro_intermission2", new UnityAction(Intro2Over));
        }

        public void Intro2Over() => game.SetStateMissionSelect();


        public void End() => actionPresenter.Hide();

        public void Update() => player.UpdateSitBehaviour();

        public void FixedUpdate()
        {
            player.FixedUpdateBehaviour();
        }

        public void LateUpdate()
        {
            player.LateUpdateBehaviour();
            
        }

        public void Menu()
        {
            //GameManager.Instance.SetStartMenu();
        }

        public void Select()
        {

        }
    }
}