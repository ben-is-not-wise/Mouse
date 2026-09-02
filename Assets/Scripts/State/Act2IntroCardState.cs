using HackedDesign.UI;

namespace HackedDesign
{
    public class Act2IntroCardState : AbstractState
    {
        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;

        private readonly ActPresenter presenter;

        public Act2IntroCardState(ActPresenter presenter)
        {
            this.presenter = presenter;
            this.presenter.finishedEvent.AddListener(Continue);
        }

        public override void Begin() => presenter.Show();

        private void Continue()
        {
        }

        public override void End() => presenter.Hide();
    }
}
