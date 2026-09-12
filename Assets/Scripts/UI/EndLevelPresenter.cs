using System;

namespace HackedDesign.UI
{
    public class EndLevelPresenter : AbstractPresenter
    {
        public event Action Continue;
        public event Action Exit;

        public void ContinueClick() => Continue?.Invoke();

        public void ExitClick() => Exit?.Invoke();
    }
}
