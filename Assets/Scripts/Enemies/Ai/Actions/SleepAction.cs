#nullable enable

namespace HackedDesign
{
    public class SleepAction : IUtilityAction
    {
        public float Score(IAi ai, AiContext ctx)
        {
            if (ctx.canSeePlayer || ctx.canHearPlayer || ctx.hasSeenPlayer || ctx.hasSeenDeadEnemies)
            {
                return 0f;
            }

            return 0.2f;
        }

        public void Begin(IAi ai)
        {
            ai.Character.ExecuteCommand(new WalkCommand(false));
            ai.Character.ExecuteCommand(new AimCommand(false));
            ai.Character.ExecuteCommand(new MoveCommand(0, 0));
            ai.Icon.Hide();
        }

        public void End(IAi ai) { }

        public void Perform(IAi ai, AiContext ctx) => ai.Character.ExecuteCommand(new MoveCommand(0, 0));
    }
}
