using HackedDesign.UI;

namespace HackedDesign
{
    [TransitionsTo(typeof(Act0Room1State))]
    public class Act0IntroCardState : AbstractState
    {
        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        private readonly IGame game;
        private readonly ActPresenter presenter;

        public Act0IntroCardState(IGame game, ActPresenter presenter)
        {
            this.game = game;
            this.presenter = presenter;
            this.presenter.finishedEvent.AddListener(Continue);
        }

        public override void Begin() => presenter.Show();

        private void Continue() => game.SetStateAct0Room1();

        public override void End() => presenter.Hide();
    }
}
