using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HackedDesign
{
    public abstract class PhasedCutsceneBehaviour : MonoBehaviour, ICutscene
    {
        [SerializeField] protected List<CutscenePhase> phases;

        protected IGame game;
        protected int currentPhaseIndex = -1;

        public int CurrentPhaseIndex => currentPhaseIndex;
        public int PhaseCount => phases?.Count ?? 0;

        public virtual void Play(IGame game)
        {
            this.game = game;
            GoToPhase(0);
        }

        public virtual void Stop(IGame game)
        {
            game.UI.FullScreenFX.Hide();
            DeactivateAllPhaseObjects();
            currentPhaseIndex = -1;
        }

        // Wire into a phase's onEnter UnityEvent to fade the screen.
        public void FadeToBlack() => game.UI.FullScreenFX.FadeToBlack();

        public void FadeOut() => game.UI.FullScreenFX.FadeOut();

        // Fades to black, then advances once the fade completes rather than waiting on a phase's dialogKey.
        protected void FadeToBlackThenGoToPhase(int index)
        {
            game.UI.FullScreenFX.FadeToBlack(new UnityAction(() => GoToPhase(index)));
        }

        protected virtual void GoToPhase(int index)
        {
            if (phases == null || index < 0 || index >= phases.Count)
            {
                return;
            }

            currentPhaseIndex = index;

            DeactivateAllPhaseObjects();

            var phase = phases[index];

            if (phase.activeObjects != null)
            {
                foreach (var obj in phase.activeObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }

            phase.onEnter?.Invoke();

            if (!string.IsNullOrEmpty(phase.dialogKey))
            {
                int enteredIndex = index;
                game.DialogManager.ShowDialog(phase.dialogKey, new UnityAction(() => OnPhaseDialogOver(enteredIndex)));
            }
        }

        /// <summary>
        /// Called after the current phase's dialog is dismissed, when it has a dialogKey set.
        /// Default behaviour advances to the next phase, or calls OnCutsceneComplete() if this was the last one.
        /// Override to insert custom transitions (e.g. a fade) between specific phases.
        /// </summary>
        protected virtual void OnPhaseDialogOver(int index)
        {
            int next = index + 1;

            if (next >= PhaseCount)
            {
                OnCutsceneComplete();
                return;
            }

            GoToPhase(next);
        }

        /// <summary>
        /// Called when the last phase's dialog has been dismissed. No-op by default.
        /// </summary>
        protected virtual void OnCutsceneComplete()
        {
        }

        protected void DeactivateAllPhaseObjects()
        {
            if (phases == null)
            {
                return;
            }

            foreach (var phase in phases)
            {
                if (phase.activeObjects == null)
                {
                    continue;
                }

                foreach (var obj in phase.activeObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                    }
                }
            }
        }
    }
}
