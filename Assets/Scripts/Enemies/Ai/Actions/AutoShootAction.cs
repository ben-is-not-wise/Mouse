#nullable enable
using UnityEngine;

namespace HackedDesign
{
    // Suppression fire: while chasing without a clean shot (e.g. mid-Advance), takes an
    // occasional pot-shot at the player's actual position. Scores below Attack, so it
    // only ever preempts lower-priority actions and never interrupts an active engagement.
    public class AutoShootAction : IUtilityAction
    {
        private bool hasFired;
        private float nextEligibleTime;

        public float Score(IAi ai, AiContext ctx)
        {
            if (ctx.playerIsDead || ctx.canSeePlayer || ctx.bullets <= 0)
            {
                return 0f;
            }

            if (hasFired || Time.time < nextEligibleTime)
            {
                return 0f;
            }

            return ctx.settings.AutoShootScore;
        }

        public void Begin(IAi ai)
        {
            hasFired = false;
            ai.Character.ExecuteCommand(new WalkCommand(false));
            ai.Icon.Alert();
        }

        public void End(IAi ai) => ai.Icon.Hide();

        public void Perform(IAi ai, AiContext ctx)
        {
            if (ctx.playerIsDead || hasFired)
            {
                return;
            }

            ai.Character.ExecuteCommand(new FacingCommand(0, ctx.playerPosition.x <= ctx.position.x ? -1 : 1));
            ai.Character.ExecuteCommand(new MoveCommand(0, 0));
            ai.Character.ExecuteCommand(new AimCommand(true));

            var aim = ctx.playerPosition + (Vector3)(Random.insideUnitCircle * ctx.settings.AimError);
            ai.Character.Attack(aim, aiming: true);

            hasFired = true;
            nextEligibleTime = Time.time + Random.Range(ctx.settings.AutoShootMinInterval, ctx.settings.AutoShootMaxInterval);
        }
    }
}
