#nullable enable

namespace HackedDesign
{
    public class UtilityBrain
    {
        private readonly IUtilityAction[] actions;
        private readonly float hysteresis;
        private IUtilityAction? current;

        public UtilityBrain(IUtilityAction[] actions, float hysteresis = 0.1f)
        {
            this.actions = actions;
            this.hysteresis = hysteresis;
        }

        public void Tick(IAi ai, AiContext ctx)
        {
            IUtilityAction? best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var action in actions)
            {
                float score = action.Score(ai, ctx);
                if (action == current && score > 0f)
                {
                    score += hysteresis;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            if (best != current)
            {
                current?.End(ai);
                current = best;
                current?.Begin(ai);
            }

            current?.Perform(ai, ctx);
        }
    }
}
