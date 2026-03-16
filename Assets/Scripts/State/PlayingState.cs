using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    public class PlayingState : IState
    {
        private readonly IGame game;
        private readonly IPlayerController player;
        private readonly IPresenter actionBar;
        private readonly IPresenter traceBar;
        private readonly IEnemyManager enemyManager;
        private readonly IMissionTimer timer;
        private readonly bool debug;

        public bool PlayerActionAllowed => true;
        public bool Battle => true;

        public PlayingState(IGame game, IPlayerController player, IEnemyManager enemyManager, IMissionTimer timer, IPresenter actionBar, IPresenter traceBar, bool debug = false)
        {
            this.game = game;
            this.player = player;
            this.enemyManager = enemyManager;
            this.timer = timer;
            this.actionBar = actionBar;
            this.traceBar = traceBar;
            this.debug = debug;
        }

        public void Begin()
        {
            timer.Timer.OnTimerStop += TimeOut;
            player.Character.SetBattleState();
            actionBar.Show();
            traceBar.Show();
            //UnityEngine.Cursor.visible = false;
        }

        public void End()
        {
            timer.Timer.OnTimerStop -= TimeOut;
            player.Stop();
            traceBar.Hide();
            actionBar.Hide();
            //UnityEngine.Cursor.visible = true;
        }

        private void TimeOut() => Debug.Log("Timeout");

        public void Update()
        {
            timer.Timer.Tick(Time.deltaTime);
            player.UpdateBattleBehaviour();
            enemyManager.UpdateAllBehaviour();
        }

        public void FixedUpdate()
        {
            player.FixedUpdateBehaviour();
            enemyManager.UpdateAllFixedBehaviour();
        }

        public void LateUpdate()
        {
            player.LateUpdateBehaviour();
            enemyManager.UpdateAllLateBehaviour();
            traceBar.Repaint();
        }

        public void Menu() => game.SetStatePaused();

        public void Select() => game.SetStateOS();
    }
}