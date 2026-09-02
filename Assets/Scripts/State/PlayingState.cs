using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(PausedState), typeof(OSState))]
    public class PlayingState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => true;
        public override bool LevelComplete => true;

        public PlayingState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            game.LevelTimer.Timer.OnTimerStop += TimeOut;
            Debug.Log("Set start battle");
            game.Player.Character.SetOutfit("Street");
            game.Player.Character.ExecuteCommand(new RollToggleCommand(false));
            game.Player.Character.SetStateBattle();
            game.UI.ActionBar.Show();
            game.UI.Trace.Show();
        }

        public override void End()
        {
            game.LevelTimer.Timer.OnTimerStop -= TimeOut;
            game.Player.Stop();
            game.EnemyManager.StopAll();
            game.UI.ActionBar.Hide();
            game.UI.Trace.Hide();
            
        }

        private void TimeOut() => Debug.Log("Timeout");

        public override void Update()
        {
            game.LevelTimer.Timer.Tick(Time.deltaTime);
            game.Player.UpdateBattleBehaviour();
            game.EnemyManager.UpdateAllBehaviour();
        }

        public override void FixedUpdate()
        {
            game.Player.FixedUpdateBehaviour();
            game.EnemyManager.UpdateAllFixedBehaviour(game.Player);
        }

        public override void LateUpdate()
        {
            game.Player.LateUpdateBehaviour();
            game.EnemyManager.UpdateAllLateBehaviour();
            game.UI.Trace.Repaint(game.LevelTimer.Timer);
        }

        public override void Pause() => game.SetStatePaused();

        public override void Select() => game.SetStateOS();
    }
}