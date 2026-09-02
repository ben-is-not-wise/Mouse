namespace HackedDesign
{
    public class LevelEndState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        public LevelEndState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.Player.Stop();
            game.Player.Character.SetStateIdle();
        }

        public override void Update()
        {
            game.Player.UpdateIdleBehaviour();
        }
    }
}
