namespace HackedDesign
{
    [TransitionsTo(typeof(IntermissionState), typeof(LoadLevelState))]
    public class MissionSelectState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => true;

        public MissionSelectState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.UI.Mission.Select += OnSelect;
            game.UI.Mission.Continue += OnContinue;
            game.UI.Mission.Show();
            game.UI.Mission.Repaint(game.GameData);
        }

        public override void End()
        {
            game.UI.Mission.Select -= OnSelect;
            game.UI.Mission.Continue -= OnContinue;
            game.UI.Mission.Hide();
        }

        private void OnSelect() => game.SetStateIntermission();

        private void OnContinue() => game.SetStateLoadLevel();
    }
}
