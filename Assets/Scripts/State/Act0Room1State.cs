using HackedDesign.UI;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public class Act0Room1State : IState
    {
        private readonly IPlayerController player;
        private readonly ILevelManager level;

        public bool PlayerActionAllowed => true;
        public bool Battle => false;

        ICutscene cutscene;

        public Act0Room1State(IPlayerController player, ILevelManager level)
        {
            this.player = player;
            this.level = level;
        }

        public void Begin()
        {
            level.Reset();
            this.cutscene = level.ShowCutscene(Cutscenes.MouseStartingRoom1, false, true, player);
            player.Character.Shadow.enabled = false;
            player.Character.SetStateIdle();
            
            this.cutscene.Play();
        }

        public void End()
        {
            this.cutscene.Stop();
            player.Character.Shadow.enabled = true;
        }

        public void Update() => player.UpdateIdleBehaviour();

        public void FixedUpdate() => player.FixedUpdateBehaviour();

        public void LateUpdate() => player.LateUpdateBehaviour();

        public void Menu()
        {

        }

        public void Select()
        {

        }
    }
}