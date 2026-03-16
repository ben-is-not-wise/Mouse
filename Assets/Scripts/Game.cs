#nullable enable
using HackedDesign.UI;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace HackedDesign
{
    public interface IGame
    {
        IState CurrentState { get; }
        GameData GameData { get; }
        GameSettings GameSettings { get; }
        MissionTimer? LevelTimer { get; }
        PlayerController Player { get; }
        int RandomSeed { get; set; }
        Level Level { get; }
        EnemyManager EnemyManager { get; }

        bool HackMode { get; set; }

        void NewGame();
        void SetStateAct0();
        void SetStateAct1();
        void SetStateAct2();
        void SetStateAct3();
        void SetStateDeath();
        void SetStateElevator();
        void SetStateIntermission();
        void SetStateLevelEnd();
        void SetStateLoading();
        void SetStateMainMenu();
        void SetStateMissionSelect();
        void SetStateOS();
        void SetStatePaused();
        void SetStatePlaying();
        void SetStateQuit();
        void SetStateRoof1();
        void SetStateRoom1();
    }

    public class Game : AutoSingleton<Game>, IGame
    {
        public const string GameVersion = "1.0";
        [Header("Game")]
        [field: SerializeField, NotNull] public PlayerController Player { get; private set; } = null!;
        [field: SerializeField, NotNull] public Level Level { get; private set; } = null!;
        [field: SerializeField, NotNull] public EnemyManager EnemyManager { get; private set; } = null!;
        [SerializeField] private MissionTimer? levelTimer = null;
        [SerializeField] private DialogManager? dialogManager = null;
        [Header("UI")]
        [SerializeField] private MainMenuPresenter? mainMenuPresenter = null;
        [SerializeField] private DeathPresenter? deathPresenter = null;
        [SerializeField] private PausePresenter? pausePresenter = null;
        [SerializeField] private ActionBarPresenter? actionBarPresenter = null;
        [SerializeField] private OSPresenter? osPresenter = null;
        [SerializeField] private TracePresenter? tracePresenter = null;
        [SerializeField] private DialogPresenter? dialogPresenter = null;
        [SerializeField] private MissionPresenter? missionPresenter = null;
        [SerializeField] private TargetPresenter? targetPresenter = null;
        [SerializeField] private ElevatorPresenter? elevatorPresenter = null;
        [SerializeField] private ActPresenter? act0Presenter = null;
        [SerializeField] private ActPresenter? act1Presenter = null;
        [SerializeField] private ActPresenter? act2Presenter = null;
        [SerializeField] private ActPresenter? act3Presenter = null;

        [Header("Data")]
        [field: SerializeField, NotNull] public GameData GameData { get; private set; } = new();

        [Header("Settings")]
        [field: SerializeField, NotNull] public GameSettings GameSettings { get; private set; } = null!;

        #region Properties
        public MissionTimer? LevelTimer { get => levelTimer; private set => levelTimer = value; }
        public int RandomSeed { get; set; } = 2;

        public bool HackMode { get; set; } = false;
        #endregion

        #region Unity Messages
        void Start() => Initialization();
        private void Update() => CurrentState.Update();
        private void LateUpdate() => CurrentState.LateUpdate();
        private void FixedUpdate() => CurrentState.FixedUpdate();
        #endregion

        #region State

        private IState currentState = new EmptyState();

        public IState CurrentState
        {
            get => this.currentState;
            private set
            {
                this.currentState?.End();
                Debug.Log($"Entering state: {value.GetType().Name}");
                this.currentState = value;
                this.currentState?.Begin();
            }
        }

        public void SetStateRoof1() => CurrentState = new Intro1RoofState(Player, Level, dialogManager);
        public void SetStateRoom1() => CurrentState = new Room1State(Player, Level, dialogManager);
        public void SetStateMissionSelect() => CurrentState = new MissionSelectState(missionPresenter);
        public void SetStateIntermission() => CurrentState = new IntermissionState(this, Player, Level, dialogManager, actionBarPresenter);
        public void SetStatePlaying() => CurrentState = new PlayingState(this, Player, EnemyManager, LevelTimer, actionBarPresenter, tracePresenter, GameSettings.Debug);
        public void SetStateLoading() => CurrentState = new LoadingState(this, Player, Level, EnemyManager);
        public void SetStateMainMenu() => CurrentState = new MainMenuState(mainMenuPresenter);
        public void SetStateDeath() => CurrentState = new DeathState(deathPresenter);
        public void SetStateLevelEnd() => CurrentState = new LevelEndState(Player);
        public void SetStatePaused() => CurrentState = new PausedState(pausePresenter);
        public void SetStateOS() => CurrentState = new OSState(this, osPresenter);
        public void SetStateElevator() => CurrentState = new ElevatorState(this, elevatorPresenter);
        public void SetStateAct0() => CurrentState = new Act0State(this, act0Presenter, GameSettings.SkipIntro);
        public void SetStateAct1() => CurrentState = new Act1State(act1Presenter);
        public void SetStateAct2() => CurrentState = new Act2State(act2Presenter);
        public void SetStateAct3() => CurrentState = new Act3State(act3Presenter);
        public void SetStateQuit() => Application.Quit();

        #endregion

        public void NewGame()
        {
            GameData.Reset();
            Player.Reset();

            Player.Character.OperatingSystem.CurrentMission = 1;

            SetStateAct0();
        }

        private void Initialization()
        {
            if (!Player.EnsureNotNull(this, nameof(Player)))
            {
                Debug.LogError("Player is null");
                Application.Quit();
                return;
            }

            if(!Player.Character.EnsureNotNull(this, nameof(Player.Character)))
            {
                Debug.LogError("Player Character is null");
                Application.Quit();
                return;
            }

            Player.Character.ExecuteCommand(new FreezeCommand());
            HideUI();
            SetStateMainMenu();
        }

        private void HideUI()
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
            elevatorPresenter.HideIfValid(this, nameof(elevatorPresenter));
            act0Presenter.HideIfValid(this, nameof(act0Presenter));
            act1Presenter.HideIfValid(this, nameof(act1Presenter));
            act2Presenter.HideIfValid(this, nameof(act2Presenter));
            act3Presenter.HideIfValid(this, nameof(act3Presenter));
        }
    }
}