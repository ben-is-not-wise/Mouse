using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(MainMenuState))]
    public class PausedState : AbstractState
    {
        private readonly IGame game;
        private readonly PausePresenter pauseMenu;
        private readonly PausePresenter.PauseState startingState;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;


        private float prevTimeScale = 0;

        public PausedState(IGame game, PausePresenter pauseMenu)
        {
            this.game = game;
            this.pauseMenu = pauseMenu;
            this.startingState = PausePresenter.PauseState.None;
        }

        public PausedState(IGame game, PausePresenter pauseMenu, PausePresenter.PauseState startingState)
        {
            this.game = game;
            this.pauseMenu = pauseMenu;
            this.startingState = startingState;
        }

        public override void Begin()
        {
            pauseMenu.Continue += OnContinue;
            pauseMenu.Exit += OnExit;
            pauseMenu.Show();
            pauseMenu.Repaint(startingState);
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        public override void End()
        {
            pauseMenu.Continue -= OnContinue;
            pauseMenu.Exit -= OnExit;
            pauseMenu.Hide();
            Time.timeScale = prevTimeScale;
        }

        private void OnContinue() => game.ResumeFromPause();

        private void OnExit() => game.SetStateMainMenu();
    }
}