using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    [TransitionsTo(typeof(MissionSelectState))]
    public class IntermissionState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => false;

        public IntermissionState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.Level.Clear();
            game.Level.ShowCutscene("Hotdog Stand", false, true, game.Player);
            game.Player.Character.ExecuteCommand(new FacingCommand(0, -1f));
            game.Player.Character.SetStateIdle();
            //game.Player.Character.SetStateSitting();

            //game.DialogManager.ShowDialog("intro_intermission1", new UnityAction(Intro1Over));
        }

        public void Intro1Over()
        {
            Debug.Log("Intro1 over");

            game.DialogManager.ShowDialog("intro_intermission2", new UnityAction(Intro2Over));
        }

        public void Intro2Over() => game.SetStateMissionSelect();

        public override void End() => game.UI.ActionBar.Hide();

        public override void Update() => game.Player.UpdateIdleBehaviour();

        public override void FixedUpdate() => game.Player.FixedUpdateBehaviour();

        public override void LateUpdate() => game.Player.LateUpdateBehaviour();
    }
}
