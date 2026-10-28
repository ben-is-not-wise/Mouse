using System;
using UnityEngine;

namespace HackedDesign.UI
{
    public class MainMenuPresenter : AbstractPresenter
    {
        [SerializeField] private GameObject bootWindow;
        [SerializeField] private GameObject creditsWindow;
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private GameObject gameContents;
        [SerializeField] private GameObject videoContents;
        [SerializeField] private GameObject audioContents;
        [SerializeField] private GameObject inputContents;

        private MenuState currentMenuState = MenuState.Boot;
        private SettingsState currentSettingsState = SettingsState.Game;

        public event Action StartGame;

        public event Action SkipGame;
        public event Action Options;
        public event Action Credits;
        public event Action Exit;

        public void OnStart() => StartGame?.Invoke();

        public void OnSkip() => SkipGame?.Invoke();

        public void OnOptions()
        {
            currentMenuState = MenuState.Settings;
            Repaint();
        }

        public void OnCredits()
        {
            currentMenuState = MenuState.Credits;
            Repaint();
        }

        public void OnCreditsClose()
        {
            currentMenuState = MenuState.Boot;
            Repaint();
        }

        public void OnSettingsClose()
        {
            currentMenuState = MenuState.Boot;
            Repaint();
        }

        public void OnGame()
        {
            currentSettingsState = SettingsState.Game;
            Repaint();
        }

        public void OnVideo()
        {
            currentSettingsState = SettingsState.Video;
            Repaint();
        }

        public void OnAudio()
        {
            currentSettingsState = SettingsState.Audio;
            Repaint();
        }

        public void OnInput()
        {
            currentSettingsState = SettingsState.Input;
            Repaint();
        }

        public void OnExit() => Exit?.Invoke();

        void Awake()
        {
            HideAll();
            UnityExtensions.EnsureNotNull(bootWindow, nameof(bootWindow));
            UnityExtensions.EnsureNotNull(settingsWindow, nameof(settingsWindow));
            UnityExtensions.EnsureNotNull(creditsWindow, nameof(creditsWindow));
            UnityExtensions.EnsureNotNull(gameContents, nameof(gameContents));
        }

        private void HideAll()
        {
            bootWindow.SetActive(false);
            creditsWindow.SetActive(false);
            settingsWindow.SetActive(false);
            gameContents.SetActive(false);
            videoContents.SetActive(false);
            audioContents.SetActive(false);
            inputContents.SetActive(false);
        }

        public void OnReady()
        {
            Repaint();
        }

        public override void Repaint()
        {
            bootWindow.SetActive(currentMenuState == MenuState.Boot);
            creditsWindow.SetActive(currentMenuState == MenuState.Credits);
            settingsWindow.SetActive(currentMenuState == MenuState.Settings);
            gameContents.SetActive(currentSettingsState == SettingsState.Game);
            videoContents.SetActive(currentSettingsState == SettingsState.Video);
            audioContents.SetActive(currentSettingsState == SettingsState.Audio);
            inputContents.SetActive(currentSettingsState == SettingsState.Input);
        }

        private enum MenuState
        {
            Boot,
            Credits,
            Settings,
            Exit

        }

        private enum SettingsState
        {
            Game,
            Video,
            Audio,
            Input
        }
    }
}
