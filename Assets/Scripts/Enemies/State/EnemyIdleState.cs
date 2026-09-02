using UnityEngine;

namespace HackedDesign
{
    [TransitionsTo(typeof(EnemySearchingState))]
    public class EnemyIdleState: IEnemyState
    {
        private readonly IAi ai;
        private bool isRoaming = false;
        private float startPhaseChange = 0;

        private const int MaxPhaseOffset = 10;

        public bool IsAlive => true;

        public EnemyIdleState(IAi ai)
        {
            this.ai = ai;
            var facing = Random.value < 0.5f ? 1 : -1;

            this.ai.Character.ExecuteCommand(new FacingCommand(0, facing));
            this.ai.Character.ExecuteCommand(new WalkCommand(true));
            this.isRoaming = Random.value < 0.5f;
            this.startPhaseChange = Time.time + Random.Range(0, MaxPhaseOffset); 
        }

        public void UpdateBehaviour(AiContext ctx)
        {
            this.ai.Character.ExecuteCommand(new AimCommand(false));

            if (ctx.hasSeenDeadEnemies || (ctx.canSeePlayer && (ctx.canHearPlayer || ctx.playerInFrontOfUs)))
            {
                this.ai.CurrentState = new EnemySearchingState(this.ai);
                return;
            }

            if(ctx.settings.Stationary)
            {
                return;
            }

            if (startPhaseChange + ctx.settings.RoamTime < Time.time)
            {

                isRoaming = !isRoaming;
                Debug.Log("Switch roaming " + isRoaming + " " + ctx.name);
                startPhaseChange = Time.time;

                // Randomly switch direction during a phase change
                if(Random.value < 0.33f)
                {
                    this.ai.Character.ExecuteCommand(new FacingCommand(0, ctx.facing * -1));
                }
            }

            // FIXME: Use constants for facing directions
            if (ctx.wallInFront || ctx.dropInFront)
            {
                this.ai.Character.ExecuteCommand(new FacingCommand(0, ctx.facing * -1));
            }

            float move = 0;
            if(isRoaming && !ctx.wallInFront && !ctx.dropInFront)
            {
                move = ctx.facing;
            }
            this.ai.Character.ExecuteCommand(new MoveCommand(move, 0));
        }

        //private bool HasSeenDeadEnemies(Vector3 position)
        //{
        //    var hits = Physics2D.OverlapCircleAll(position, 5f);
        //}

        public void Begin()
        {

        }

        public void End()
        {

        }
    }
}
