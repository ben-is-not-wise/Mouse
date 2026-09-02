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

        EnemyManager EnemyManager { get; }

        DialogManager DialogManager { get; }

        Level Level { get; }
        IUIManager UI { get; }
        int RandomSeed { get; set; }


        bool HackMode { get; set; }

        void NewGame();
        void SetStateAct0IntroCard();
        void SetStateAct1IntroCard();
        void SetStateAct2IntroCard();
        void SetStateAct3IntroCard();
        void SetStateDeath();
        void SetStateIntermission();
        void SetStateLevelEnd();
        void SetStateLoadLevel();
        void SetStateMainMenu();
        void SetStateMissionSelect();
        void SetStateOS();
        void SetStatePaused();
        void SetStatePaused(PausePresenter.PauseState startingState);
        void ResumeFromPause();
        void SetStatePlaying();
        void SetStateQuit();
        void SetStateAct0Roof();
        void SetStateAct0Room1();
        void SetStateAct0Room2();
        void SetStateAct0LoadTutorialLevel();
    }

    public class Game : AutoSingleton<Game>, IGame
    {
        public const string GameVersion = "1.0";
        [Header("Game")]
        [field: SerializeField, NotNull] public PlayerController Player { get; private set; } = null!;
        [field: SerializeField, NotNull] public Level Level { get; private set; } = null!;
        [field: SerializeField, NotNull] public EnemyManager EnemyManager { get; private set; } = null!;
        [field: SerializeField, NotNull] public DialogManager DialogManager { get; private set; } = null!;
        [SerializeField] private MissionTimer? levelTimer = null;
        [Header("UI")]
        [SerializeField, NotNull] private UIManager uiManager = null!;

        [Header("Data")]
        [field: SerializeField, NotNull] public GameData GameData { get; private set; } = new();

        [Header("Settings")]
        [field: SerializeField, NotNull] public GameSettings GameSettings { get; private set; } = null!;

        #region Properties
        public IUIManager UI => uiManager;
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

        public void SetStateAct0Roof() => CurrentState = new Act0RoofState(this);
        public void SetStateAct0Room1() => CurrentState = new Act0Room1State(this);
        public void SetStateAct0Room2() => CurrentState = new Act0Room2State(this);
        public void SetStateMissionSelect() => CurrentState = new MissionSelectState(this);
        public void SetStateIntermission() => CurrentState = new IntermissionState(this);
        public void SetStatePlaying() => CurrentState = new PlayingState(this);
        public void SetStateLoadLevel() => CurrentState = new LoadLevelState(this);
        public void SetStateAct0LoadTutorialLevel() => CurrentState = new LoadTutorialState(this);
        public void SetStateMainMenu() => CurrentState = new MainMenuState(this, uiManager.MainMenu);
        public void SetStateDeath() => CurrentState = new DeathState(this, uiManager.Death);
        public void SetStateLevelEnd() => CurrentState = new LevelEndState(this);

        private IState? suspendedState;

        public void SetStatePaused(PausePresenter.PauseState startingState)
        {
            suspendedState = currentState;
            currentState.Suspend();
            Debug.Log($"Entering state: {nameof(PausedState)}");
            currentState = new PausedState(this, uiManager.Pause, startingState);
            currentState.Begin();
        }

        public void SetStatePaused()
        {
            suspendedState = currentState;
            currentState.Suspend();
            Debug.Log($"Entering state: {nameof(PausedState)}");
            currentState = new PausedState(this, uiManager.Pause);
            currentState.Begin();
        }

        public void ResumeFromPause()
        {
            currentState.End();
            Debug.Log($"Resuming state: {suspendedState!.GetType().Name}");
            currentState = suspendedState;
            suspendedState = null;
            currentState.Resume();
        }

        public void SetStateOS() => CurrentState = new OSState(this);
        public void SetStateAct0IntroCard() => CurrentState = new Act0IntroCardState(this, uiManager.Act0);
        public void SetStateAct1IntroCard() => CurrentState = new Act1IntroCardState(this, uiManager.Act1);
        public void SetStateAct2IntroCard() => CurrentState = new Act2IntroCardState(uiManager.Act2);
        public void SetStateAct3IntroCard() => CurrentState = new Act3IntroCardState(uiManager.Act3);
        public void SetStateQuit() => Application.Quit();

        #endregion

        public void NewGame()
        {
            GameData.Reset();
            Player.Reset();

            Player.Character.OperatingSystem.CurrentMission = 1;

            switch(GameSettings.SkipStage)
            {
                case SkipStage.SkipAct0:
                    Debug.Log("SkipAct0");
                    SetStateAct1IntroCard();
                    break;
                case SkipStage.SkipTutLevel:
                    Debug.Log("SkipTut");
                    SetStateAct0Room2();
                    break;
                case SkipStage.SkipIntro:
                    Debug.Log("SkipIntro");
                    var cutscene = Level.ShowCutscene(Cutscenes.Rooftop1, true, 0, 0, true, true, Player);
                    cutscene.Stop(this);
                    Level.Reset();
                    Player.Teleport(Level.GetLevelPlayerSpawnLocation() + Vector3.up);
                    Player.Character.ExecuteCommand(new OutfitSwapCommand("PD"));
                    SetStateAct0LoadTutorialLevel();
                    break;
                default:
                    Debug.Log("Intro");
                    SetStateAct0IntroCard();
                    break;
            }
            
        }

        private void Initialization()
        {
            EnemyManager.Reset();

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
            uiManager.HideUI();
            SetStateMainMenu();
        }
    }
}