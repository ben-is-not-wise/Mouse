using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(PausedState), typeof(OSState))]
    public class LoadTestingState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => true;
        public override bool LevelComplete => true;

        public LoadTestingState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            Debug.Log("Load Testing State");
            game.Level.RainOn();
            game.Player.Reset();
            game.Player.Character.OperatingSystem.Enabled = true;
            game.Level.Reset();
            game.UI.ActionBar.Show();
            game.Player.Character.SetOutfit("Street");
            game.Player.Character.ExecuteCommand(new RollToggleCommand(false));
            game.Player.Character.SetStateBattle();
            game.Player.Teleport(game.Level.GetLevelPlayerSpawnLocation() + Vector3.up);
            game.Level.SpawnEnemies(3);
        }

        public override void End()
        {
            game.UI.ActionBar.Hide();
            game.Player.Stop();
        }

        public override void Update()
        {
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
        }

        public override void Pause() => game.SetStatePaused(); 

        public override void Select() => game.SetStateOS();
    }
}
