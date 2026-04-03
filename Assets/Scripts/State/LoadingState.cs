using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    public class LoadingState : IState
    {
        private readonly IGame game;
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IEnemyManager enemyManager;

        public bool PlayerActionAllowed => true;
        public bool Battle => true;

        public LoadingState(IGame game, IPlayerController player, ILevelManager level, IEnemyManager enemyManager)
        {
            this.game = game;
            this.player = player;
            this.level = level;
            this.enemyManager = enemyManager;
        }

        public void Begin()
        {
            LoadLevel();
            player.Reset();
            player.Teleport(level.GetLevelPlayerSpawnLocation() + Vector3.up);
            level.SpawnEnemies(33);
        }

        private void LoadLevel()
        {
            level.Reset();
            level.Generate(Random.Range(1, 1000), 50, 1);
            level.RainOn();
        }

        public void End() => game.LevelTimer.Timer.Start();

        public void Update()
        {
            game.LevelTimer.Reset();
            game.SetStatePlaying();
        }

        public void FixedUpdate()
        {
        }

        public void LateUpdate()
        {
           
        }

        public void Menu()
        {
        }

        public void Select()
        {

        }
    }
}