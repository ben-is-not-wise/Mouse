#nullable enable
using UnityEngine;

namespace HackedDesign
{
    // Relentless chaser: always aware of the player, advances toward them at normal
    // move speed (e.g. fliers entering from the left of the map). Stops when it reaches
    // the player and re-engages / turns around once the player moves out of a fuzzy band.
    public class AdvanceAction : IUtilityAction
    {
        private const float ArrivalRange = 1.2f;
        private const float ReengageRange = 2.0f;

        private readonly AiSteering steering = new AiSteering();

        private bool stopped;

        public float Score(IAi ai, AiContext ctx) => 0.4f;

        public void Begin(IAi ai)
        {
            stopped = false;
            ai.Character.ExecuteCommand(new WalkCommand(false));
            ai.Character.ExecuteCommand(new AimCommand(false));
            ai.Icon.Alert();
        }

        public void End(IAi ai) => ai.Icon.Hide();

        public void Perform(IAi ai, AiContext ctx)
        {
            if (Game.Instance.Player.Character.IsDead)
            {
                return;
            }

            var playerPosition = Game.Instance.Player.transform.position;
            float distance = (playerPosition - ctx.position).magnitude;

            if (!stopped && distance <= ArrivalRange)
            {
                stopped = true;
            }
            else if (stopped && distance >= ReengageRange)
            {
                stopped = false;
            }

            if (stopped)
            {
                ai.Character.ExecuteCommand(new MoveCommand(0, 0));
                return;
            }

            steering.MoveToward(ai, ctx, playerPosition, ctx.settings.CanJumpGaps);
        }
    }
}
