using HackedDesign.UI;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public class Act0RoofState : IState
    {
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IDialogManager dialog;

        public bool PlayerActionAllowed => false;
        public bool Battle => false;

        public Act0RoofState(IPlayerController player, ILevelManager level, IDialogManager dialog)
        {
            this.player = player;
            this.level = level;
            this.dialog = dialog;
        }

        public void Begin()
        {
            var cutscene = level.ShowCutscene(Cutscenes.Rooftop1, true, 0, 25, 0, true, true, player);
            level.Reset();
            player.Character.ExecuteCommand(new OutfitSwapCommand("PD"));
            player.Teleport(level.GetLevelPlayerSpawnLocation());

            cutscene.Play();
        }

        public void End() { }

        public void Update() { }

        public void FixedUpdate() { }

        public void LateUpdate() { }

        public void Menu()
        {
            //GameManager.Instance.SetStartMenu();
        }

        public void Select()
        {

        }
    }
}