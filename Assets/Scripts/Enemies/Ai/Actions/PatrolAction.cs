#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public class PatrolAction : IUtilityAction
    {
        private const int MaxPhaseOffset = 10;

        private bool isRoaming;
        private float startPhaseChange;
        private int facing = 1;

        public float Score(IAi ai, AiContext ctx) => 0.1f;

        public void Begin(IAi ai)
        {
            facing = Random.value < 0.5f ? 1 : -1;
            ai.Character.ExecuteCommand(new FacingCommand(0, facing));
            ai.Character.ExecuteCommand(new WalkCommand(true));
            ai.Character.ExecuteCommand(new AimCommand(false));
            isRoaming = Random.value < 0.5f;
            startPhaseChange = Time.time + Random.Range(0, MaxPhaseOffset);
            ai.Icon.Hide();
        }

        public void End(IAi ai) { }

        public void Perform(IAi ai, AiContext ctx)
        {
            if (ctx.settings.Stationary)
            {
                return;
            }

            if (startPhaseChange + ctx.settings.RoamTime < Time.time)
            {
                isRoaming = !isRoaming;
                startPhaseChange = Time.time;

                if (Random.value < 0.33f)
                {
                    facing = ctx.facing * -1;
                    ai.Character.ExecuteCommand(new FacingCommand(0, facing));
                }
            }

            // Flyers ignore drops; they only turn around at walls and hold altitude via hover steering.
            bool blocked = ctx.flying ? ctx.wallInFront : (ctx.wallInFront || ctx.dropInFront);

            if (blocked)
            {
                facing = ctx.facing * -1;
                ai.Character.ExecuteCommand(new FacingCommand(0, facing));
            }

            float move = (isRoaming && !blocked) ? ctx.facing : 0;
            float climb = ctx.flying ? AiSteering.HoverClimb(ctx) : 0;
            ai.Character.ExecuteCommand(new MoveCommand(move, climb));
        }
    }
}
