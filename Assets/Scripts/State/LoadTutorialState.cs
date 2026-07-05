using HackedDesign.UI;
using UnityEngine;

namespace HackedDesign
{
    public class LoadTutorialState : IState
    {
        private readonly IGame game;
        private readonly IPlayerController player;
        private readonly ILevelManager level;
        private readonly IEnemyManager enemyManager;

        public bool PlayerActionAllowed => true;
        public bool Battle => true;

        public LoadTutorialState(IGame game, IPlayerController player, ILevelManager level, IEnemyManager enemyManager)
        {
            this.game = game;
            this.player = player;
            this.level = level;
            this.enemyManager = enemyManager;
        }

        public void Begin()
        {
            Debug.Log("load tut state");
            level.RainOn();
            player.Reset();
            level.Reset();
            player.Teleport(level.GetLevelPlayerSpawnLocation() + Vector3.up);
            level.SpawnEnemies(20);
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