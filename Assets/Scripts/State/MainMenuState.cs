using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(Act1IntroCardState), typeof(Act0Room2State), typeof(LoadTutorialState), typeof(Act0IntroCardState))]
    public class MainMenuState : AbstractState
    {
        private readonly IGame game;
        private readonly MainMenuPresenter mainMenu;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        public MainMenuState(IGame game, MainMenuPresenter mainMenu)
        {
            this.game = game;
            this.mainMenu = mainMenu;
        }

        public override void Begin()
        {
            mainMenu.StartGame += OnStart;
            mainMenu.Options += OnOptions;
            mainMenu.Credits += OnCredits;
            mainMenu.Exit += OnExit;
            mainMenu.Show();
        }

        public override void End()
        {
            mainMenu.StartGame -= OnStart;
            mainMenu.Options -= OnOptions;
            mainMenu.Credits -= OnCredits;
            mainMenu.Exit -= OnExit;
            mainMenu.Hide();
        }

        private void OnStart() => game.NewGame();

        private void OnOptions() => Debug.Log("Main Menu Options");

        private void OnCredits() => Debug.Log("Credits Click");

        private void OnExit() => game.SetStateQuit();
    }
}
