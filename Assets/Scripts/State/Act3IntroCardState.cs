using HackedDesign.UI;

namespace HackedDesign
{
    public class Act3IntroCardState : IState
    {

        public bool PlayerActionAllowed => false;
        public bool Battle => false;


        private readonly ActPresenter presenter;

        public Act3IntroCardState(ActPresenter presenter)
        {
            this.presenter = presenter;
            this.presenter.finishedEvent.AddListener(Continue);
        }

        public void Begin() => presenter.Show();

        private void Continue()
        {
        }

        public void End() => presenter.Hide();

        public void Update() { }

        public void FixedUpdate() { }

        public void LateUpdate() { }

        public void Menu() { }

        public void Select() { }
    }
}