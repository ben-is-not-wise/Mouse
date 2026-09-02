namespace HackedDesign
{
    public class Act0Room2State : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => false;

        public Act0Room2State(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.Level.Clear();
            game.Level.ShowCutscene(Cutscenes.MouseStartingRoom2, true, true, game.Player);
            //game.Player.Character.Shadow.enabled = false;
            game.Player.Character.SetStateIdle();
            game.DialogManager.ShowDialog("intro_room1", Dialog1End);
        }

        //public override void End() => game.Player.Character.Shadow.enabled = true;

        public void Dialog1End()
        {

        }

        public override void Update() => game.Player.UpdateIdleBehaviour();

        public override void FixedUpdate() => game.Player.FixedUpdateBehaviour();

        public override void LateUpdate() => game.Player.LateUpdateBehaviour();
    }
}
