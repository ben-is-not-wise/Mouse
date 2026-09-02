using HackedDesign.UI;

namespace HackedDesign
{
    [TransitionsTo(typeof(LoadLevelState), typeof(LoadTutorialState))]
    public class DeathState : AbstractState
    {
        private readonly IGame game;
        private readonly DeathPresenter deathMenu;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        public DeathState(IGame game, DeathPresenter deathMenu)
        {
            this.game = game;
            this.deathMenu = deathMenu;
        }

        public override void Begin()
        {
            deathMenu.Restart += OnRestart;
            deathMenu.Exit += OnExit;
            deathMenu.Show();
        }

        public override void End()
        {
            deathMenu.Restart -= OnRestart;
            deathMenu.Exit -= OnExit;
            deathMenu.Hide();
        }

        private void OnRestart()
        {
            if (game.GameData.FinishedTutorial)
            {
                game.SetStateLoadLevel();
            }
            else
            {
                game.SetStateAct0LoadTutorialLevel();
            }
        }

        private void OnExit() => game.SetStateMainMenu();
    }
}
