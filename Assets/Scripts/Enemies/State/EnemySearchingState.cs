using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(EnemyAlertState), typeof(EnemyIdleState))]
    public class EnemySearchingState : IEnemyState
    {
        private readonly IAi ai;

        private float reactionStartTime = 0f;


        private bool sawPlayer = false;

        public bool IsAlive => true;

        public EnemySearchingState(IAi ai)
        {
            this.ai = ai;
            ai.Character.ExecuteCommand(new WalkCommand(false));
        }

        public void UpdateBehaviour(AiContext ctx)
        {
            if (ctx.playerIsDead)
            {
                return;
            }

            if (!sawPlayer && (ctx.canSeePlayer || ctx.canHearPlayer))
            {
                sawPlayer = true;
                reactionStartTime = Time.time;
            }
            else if (sawPlayer && !(ctx.canSeePlayer || ctx.canHearPlayer))
            {
                sawPlayer = false;
                reactionStartTime = Time.time;
            }

            if(Time.time >= reactionStartTime + ctx.settings.ReactionTime)
            {
                if(sawPlayer)
                {
                    ai.CurrentState = new EnemyAlertState(ai);
                }
                else
                {
                    if(!ctx.hasSeenPlayer)
                    {
                        ai.CurrentState = new EnemyIdleState(ai);
                    }
                }
            }

            float move = 0;
            move = Mathf.Sign((ctx.position - ctx.lastKnownPlayerPosition).x) < 0 ? 1 : -1;

            ai.Character.ExecuteCommand(new FacingCommand(move, 0));

            if (!ctx.settings.Stationary)
            {
                
                if (ctx.wallInFront || ctx.dropInFront)
                {
                    move = 0;
                }

                ai.Character.ExecuteCommand(new MoveCommand(move, 0));
            }
        }

        public void Begin() => ai.Icon.Searching();

        public void End() => ai.Icon.Hide();
    }
}
