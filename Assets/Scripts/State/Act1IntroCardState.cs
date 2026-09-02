using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(IntermissionState))]
    public class Act1IntroCardState : AbstractState
    {
        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        private readonly ActPresenter presenter;

        private readonly IGame game;

        public Act1IntroCardState(IGame game, ActPresenter presenter)
        {
            this.game = game;
            this.presenter = presenter;
            this.presenter.finishedEvent.AddListener(Continue);
        }

        public override void Begin() => presenter.Show();

        private void Continue() 
        {
            Debug.Log("Continue");
            game.SetStateIntermission();
        }
 

        public override void End() => presenter.Hide();
    }
}
