#nullable enable
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace HackedDesign.UI
{
    public interface IUIManager
    {
        MainMenuPresenter? MainMenu { get; }
        DeathPresenter? Death { get; }
        PausePresenter? Pause { get; }
        ActionBarPresenter? ActionBar { get; }
        PausePresenter? OS { get; }
        TracePresenter? Trace { get; }
        DialogPresenter? Dialog { get; }
        MissionPresenter? Mission { get; }
        TargetPresenter? Target { get; }
        ActPresenter? Act0 { get; }
        ActPresenter? Act1 { get; }
        ActPresenter? Act2 { get; }
        ActPresenter? Act3 { get; }
        FullScreenFXPresenter FullScreenFX { get; }

        void HideUI();
    }

    public class UIManager : AutoSingleton<UIManager>, IUIManager
    {
        [SerializeField] private MainMenuPresenter? mainMenuPresenter = null;
        [SerializeField] private DeathPresenter? deathPresenter = null;
        [SerializeField] private PausePresenter? pausePresenter = null;
        [SerializeField] private ActionBarPresenter? actionBarPresenter = null;
        [SerializeField] private PausePresenter? osPresenter = null;
        [SerializeField] private TracePresenter? tracePresenter = null;
        [SerializeField] private DialogPresenter? dialogPresenter = null;
        [SerializeField] private MissionPresenter? missionPresenter = null;
        [SerializeField] private TargetPresenter? targetPresenter = null;
        [SerializeField] private ActPresenter? act0Presenter = null;
        [SerializeField] private ActPresenter? act1Presenter = null;
        [SerializeField] private ActPresenter? act2Presenter = null;
        [SerializeField] private ActPresenter? act3Presenter = null;
        [field: SerializeField, NotNull] public FullScreenFXPresenter FullScreenFX { get; private set; } = null!;

        public MainMenuPresenter? MainMenu => mainMenuPresenter;
        public DeathPresenter? Death => deathPresenter;
        public PausePresenter? Pause => pausePresenter;
        public ActionBarPresenter? ActionBar => actionBarPresenter;
        public PausePresenter? OS => osPresenter;
        public TracePresenter? Trace => tracePresenter;
        public DialogPresenter? Dialog => dialogPresenter;
        public MissionPresenter? Mission => missionPresenter;
        public TargetPresenter? Target => targetPresenter;
        public ActPresenter? Act0 => act0Presenter;
        public ActPresenter? Act1 => act1Presenter;
        public ActPresenter? Act2 => act2Presenter;
        public ActPresenter? Act3 => act3Presenter;

        public void HideUI()
        {
            mainMenuPresenter.HideIfValid(this, nameof(mainMenuPresenter));
            deathPresenter.HideIfValid(this, nameof(deathPresenter));
            pausePresenter.HideIfValid(this, nameof(pausePresenter));
            actionBarPresenter.HideIfValid(this, nameof(actionBarPresenter));
            osPresenter.HideIfValid(this, nameof(osPresenter));
            tracePresenter.HideIfValid(this, nameof(tracePresenter));
            dialogPresenter.HideIfValid(this, nameof(dialogPresenter));
            missionPresenter.HideIfValid(this, nameof(missionPresenter));
            targetPresenter.HideIfValid(this, nameof(targetPresenter));
            act0Presenter.HideIfValid(this, nameof(act0Presenter));
            act1Presenter.HideIfValid(this, nameof(act1Presenter));
            act2Presenter.HideIfValid(this, nameof(act2Presenter));
            act3Presenter.HideIfValid(this, nameof(act3Presenter));
            FullScreenFX.HideIfValid(this, nameof(FullScreenFX));
        }
    }
}
