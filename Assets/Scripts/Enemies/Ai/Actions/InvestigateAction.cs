#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public class InvestigateAction : IUtilityAction
    {
        private const float ArrivalRange = 1.5f;
        private const float SearchTime = 4f;
        private const float LookInterval = 1f;

        private readonly AiSteering steering = new AiSteering();

        private Vector3 lastTarget;
        private bool searching;
        private bool gaveUp;
        private float searchStartTime;
        private float nextLook;

        public float Score(IAi ai, AiContext ctx)
        {
            if (ctx.canSeePlayer)
            {
                return 0f;
            }

            bool aware = ctx.canHearPlayer || ctx.hasSeenPlayer || ctx.hasSeenDeadEnemies;
            if (!aware)
            {
                gaveUp = false;
                searching = false;
                return 0f;
            }

            if (ctx.canHearPlayer)
            {
                gaveUp = false;
            }

            return gaveUp ? 0f : 0.5f;
        }

        public void Begin(IAi ai)
        {
            ai.Character.ExecuteCommand(new WalkCommand(false));
            ai.Character.ExecuteCommand(new AimCommand(false));
            ai.Icon.Searching();
            searching = false;
            gaveUp = false;
        }

        public void End(IAi ai) => ai.Icon.Hide();

        public void Perform(IAi ai, AiContext ctx)
        {
            if (Game.Instance.Player.Character.IsDead)
            {
                return;
            }

            if ((ctx.lastKnownPlayerPosition - lastTarget).sqrMagnitude > 0.01f)
            {
                lastTarget = ctx.lastKnownPlayerPosition;
                searching = false;
                gaveUp = false;
            }

            if (ctx.settings.Stationary)
            {
                int look = (ctx.position - ctx.lastKnownPlayerPosition).x < 0 ? 1 : -1;
                ai.Character.ExecuteCommand(new FacingCommand(0, look));
                return;
            }

            if (!searching && Mathf.Abs(ctx.lastKnownPlayerPosition.x - ctx.position.x) <= ArrivalRange)
            {
                searching = true;
                searchStartTime = Time.time;
                nextLook = Time.time;
            }

            if (searching)
            {
                ai.Character.ExecuteCommand(new MoveCommand(0, 0));

                if (Time.time >= nextLook)
                {
                    ai.Character.ExecuteCommand(new FacingCommand(0, ctx.facing * -1));
                    nextLook = Time.time + LookInterval;
                }

                if (Time.time >= searchStartTime + SearchTime)
                {
                    gaveUp = true;
                }
            }
            else
            {
                steering.MoveToward(ai, ctx, ctx.lastKnownPlayerPosition);
            }
        }
    }
}
