using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(EnemyIdleState), typeof(EnemySearchingState))]
    public class EnemyAlertState: IEnemyState
    {
        //private readonly EnemyController enemyController;
        private readonly IAi ai;
        private float startTriggerTime = 0;

        private bool sawPlayer = false;
        private float lastSawPlayer = 0;

        public bool IsAlive => true;

        public EnemyAlertState(IAi ai)
        {
            //this.enemyController = enemyController;
            this.ai = ai;
            this.ai.Character.ExecuteCommand(new WalkCommand(false));
        }

        public void UpdateBehaviour(AiContext ctx)
        {
            if(ctx.playerIsDead)
            {
                Debug.Log("player is dead");
                this.ai.CurrentState = new EnemyIdleState(this.ai);
                return;
            }

            if (!ctx.settings.Stationary && ctx.canSeePlayer)
            {
                this.ai.Character.ExecuteCommand(new FacingCommand(0, ctx.playerPosition.x <= this.ai.Character.transform.position.x ? -1 : 1));
            }

            this.ai.Character.ExecuteCommand(new AimCommand(ctx.bullets > 0));

            var canSeePlayer = ctx.canSeePlayer;

            if(!canSeePlayer && lastSawPlayer + ctx.settings.GiveUpTime <= Time.time)
            {
                this.ai.CurrentState = new EnemySearchingState(this.ai);
                return;
            }

            if (!canSeePlayer && sawPlayer)
            {
                sawPlayer = false;
                lastSawPlayer = Time.time;
            }
            else if(canSeePlayer)
            {
                sawPlayer = true;

                if (ctx.bullets > 0)
                {

                    this.ai.Character.ExecuteCommand(new MoveCommand(0, 0)); // We need to stop moving
                    if (startTriggerTime + ctx.settings.RecognitionTime <= Time.time)
                    {
                        this.ai.Character.Attack(ctx.lastKnownPlayerPosition, true);
                        startTriggerTime = Time.time + this.ai.Character.Settings.AttackRate;
                    }
                }
                else
                {
                    // If we're out of bullets, let's try and move toward the player
                    var distanceToPlayer = this.ai.Character.transform.position - ctx.playerPosition;

                    if (distanceToPlayer.magnitude > 1.5f)
                    {
                        Vector2 move = ctx.flying ? distanceToPlayer.normalized : new Vector2(distanceToPlayer.x > 0 ? -1 : 1, 0);

                        if (ctx.wallInFront || ctx.dropInFront)
                        {
                            Debug.Log("drop");
                            move = Vector2.zero;
                        }
                        ai.Character.ExecuteCommand(new MoveCommand(move.x, move.y));
                    }
                    else
                    {

                        ai.Character.ExecuteCommand(new MoveCommand(0, 0));
                        if (startTriggerTime + ctx.settings.RecognitionTime <= Time.time)
                        {
                            this.ai.Character.Attack(ctx.lastKnownPlayerPosition, false);
                            startTriggerTime = Time.time + this.ai.Character.Settings.AttackRate;
                        }
                    }
                }
            }
        }


        public void Begin()
        {
            startTriggerTime = Time.time;
            lastSawPlayer = Time.time;
            sawPlayer = true;
            this.ai.Icon.Alert();
        }

        public void End() => this.ai.Icon.Hide();
    }
}
