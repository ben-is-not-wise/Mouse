using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(PlayingState))]
    public class LoadLevelState : AbstractState
    {
        private readonly IGame game;

        public override bool PlayerActionAllowed => true;
        public override bool Battle => true;

        public LoadLevelState(IGame game)
        {
            this.game = game;
        }

        public override void Begin()
        {
            LoadLevel();
            game.Player.Reset();
            game.Player.Teleport(game.Level.GetLevelPlayerSpawnLocation() + Vector3.up);
            game.Level.SpawnEnemies(33);
        }

        private void LoadLevel()
        {
            game.Level.Clear();
            game.Level.Generate(Random.Range(1, 1000), 1);
            game.Level.RainOn();
        }

        public override void End() => game.LevelTimer.Timer.Start();

        public override void Update()
        {
            game.LevelTimer.Reset();
            game.SetStatePlaying();
        }
    }
}
