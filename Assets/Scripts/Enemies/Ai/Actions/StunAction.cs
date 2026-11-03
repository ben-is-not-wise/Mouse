#nullable enable

namespace HackedDesign
{
    public class StunAction : IUtilityAction
    {
        public float Score(IAi ai, AiContext ctx) => 0f;

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
