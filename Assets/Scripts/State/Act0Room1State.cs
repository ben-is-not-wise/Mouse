namespace HackedDesign
{
    public class Act0Room1State : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => false;

        ICutscene cutscene;

        public Act0Room1State(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.Level.Clear();
            this.cutscene = game.Level.ShowCutscene(Cutscenes.MouseStartingRoom1, false, true, game.Player);
            //game.Player.Character.Shadow.enabled = false;
            game.Player.Character.SetStateIdle();
            DialogManager.Instance.SetMessages("intro_messages");

            this.cutscene.Play(game);
        }

        public override void End()
        {
            this.cutscene.Stop(game);
            //game.Player.Character.Shadow.enabled = true;
        }

        public override void Update() => game.Player.UpdateIdleBehaviour();

        public override void FixedUpdate() => game.Player.FixedUpdateBehaviour();

        public override void LateUpdate() => game.Player.LateUpdateBehaviour();

        public override void Pause() => game.SetStatePaused();
    }
}
