#nullable enable

namespace HackedDesign
{
    public interface IUtilityAction
    {
        float Score(IAi ai, AiContext ctx);
        void Begin(IAi ai);
        void End(IAi ai);
        void Perform(IAi ai, AiContext ctx);
    }
}
