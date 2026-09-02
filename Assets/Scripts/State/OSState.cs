using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(MainMenuState), typeof(PlayingState))]
    public class OSState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        private float prevTimeScale = 0;

        public OSState(IGame game)
        {
            this.game = game;
        }

        public override void Begin() { 
            game.UI.OS.Show();
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        public override void End()
        {
            
            game.UI.OS.Hide();
            Time.timeScale = prevTimeScale;
        }

        public override void Update() => game.UI.OS.Repaint();

        public override void Pause() => game.SetStateMainMenu();

        public override void Select() => game.SetStatePlaying();
    }
}
