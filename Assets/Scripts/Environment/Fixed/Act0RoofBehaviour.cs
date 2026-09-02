namespace HackedDesign
{
    public class Act0RoofBehaviour : PhasedCutsceneBehaviour
    {
        public override void Stop(IGame game)
        {
            game.DialogManager.HideDialog();
            base.Stop(game);
        }

        protected override void OnPhaseDialogOver(int index)
        {
            // Phase 3 (index 2) is preceded by a fade-to-black.
            if (index == 1)
            {
                FadeToBlackThenGoToPhase(2);
                return;
            }

            base.OnPhaseDialogOver(index);
        }

        protected override void OnCutsceneComplete()
        {
            Stop(game);
            game.Player.Reset();
            game.SetStateAct0LoadTutorialLevel();
        }

        // Wire these into each phase's onEnter UnityEvent in the Inspector.

        public void FaceUp() => game.Player.Character.ExecuteCommand(new FacingCommand(0, 1f));

        public void FaceDown() => game.Player.Character.ExecuteCommand(new FacingCommand(0, -1f));

        public void SetSitting()
        {
            game.Player.Character.SetStateSitting();
            game.Player.Character.Animate();
        }

        public void SetIdle()
        {
            game.Player.Character.SetStateIdle();
            game.Player.Character.Animate();
        }

        public void Animate() => game.Player.Character.Animate();
    }
}
