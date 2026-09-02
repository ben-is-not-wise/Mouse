using System;

namespace HackedDesign.UI
{
    public class MainMenuPresenter : AbstractPresenter
    {
        public event Action StartGame;
        public event Action Options;
        public event Action Credits;
        public event Action Exit;

        public void StartClick() => StartGame?.Invoke();

        public void OptionsClick() => Options?.Invoke();

        public void CreditsClick() => Credits?.Invoke();

        public void ExitClick() => Exit?.Invoke();
    }
}
