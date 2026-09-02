using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(PausedState), typeof(OSState))]
    public class LoadTutorialState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => true;
        public override bool LevelComplete => true;

        public LoadTutorialState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            Debug.Log("load tut state");
            game.Level.RainOn();
            game.Player.Reset();
            game.Level.Reset();
            game.UI.ActionBar.Show();
            game.Player.Character.SetOutfit("PD");
            game.Player.Character.ExecuteCommand(new RollToggleCommand(false));
            game.Player.Character.SetStateBattle();
            game.Player.Teleport(game.Level.GetLevelPlayerSpawnLocation() + Vector3.up);
            game.Level.SpawnEnemies(20);
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

        public override void Pause() => game.SetStatePaused(); // FIXME: Use a custom pause presenter here

        public override void Select() => game.SetStateOS();
    }
}
