namespace HackedDesign
{
    public class Act0RoofState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        public Act0RoofState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            var cutscene = game.Level.ShowCutscene(Cutscenes.Rooftop1, true, 0, 0, true, true, game.Player);
            game.Level.Reset();
            game.Player.Character.ExecuteCommand(new OutfitSwapCommand("PD"));
            game.Player.Teleport(game.Level.GetLevelPlayerSpawnLocation());

            cutscene.Play(game);
        }
    }
}
