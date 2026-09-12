using HackedDesign.UI;

namespace HackedDesign
{
    public class LevelEndState : AbstractState
    {
        private readonly IGame game; 
        private readonly EndLevelPresenter endLevelMenu;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        public LevelEndState(IGame game)
        {
            this.game = game;
            this.endLevelMenu = UIManager.Instance.EndLevel;
        }

        public override void Begin()
        {
            game.EnemyManager.StopAll();
            game.Player.Stop();
            game.Player.Character.SetStateIdle();

            endLevelMenu.Continue += OnContinue;
            endLevelMenu.Exit += OnExit;
            endLevelMenu.Show();

        }

        public override void End()
        {
            endLevelMenu.Continue -= OnContinue;
            endLevelMenu.Exit -= OnExit;
            endLevelMenu.Hide();
        }

        public override void Update()
        {
            //game.Player.UpdateIdleBehaviour();
        }

        private void OnContinue()
        {
            if (game.GameData.FinishedTutorial)
            {
                game.SetStateIntermission();
            }
            else
            {
                game.SetStateAct0LoadTutorialLevel();
            }
        }

        private void OnExit() => game.SetStateMainMenu();
    }
}
