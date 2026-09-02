using System;

namespace HackedDesign.UI
{
    public class DeathPresenter : AbstractPresenter
    {
        public event Action Restart;
        public event Action Exit;

        public void RestartClick() => Restart?.Invoke();

        public void ExitClick() => Exit?.Invoke();
    }
}
