#nullable enable
using UnityEngine;

namespace HackedDesign
{
    public class UtilityBrain
    {
        private readonly IUtilityAction[] actions;
        private readonly float hysteresis;
        private IUtilityAction? current;
        private IUtilityAction? forced;
        private float forcedUntil;

        public UtilityBrain(IUtilityAction[] actions, float hysteresis = 0.1f)
        {
            this.actions = actions;
            this.hysteresis = hysteresis;
        }

        public bool IsForced => this.forced != null && Time.time < this.forcedUntil;

        public void Force(IUtilityAction action, float duration)
        {
            this.forced = action;
            this.forcedUntil = Time.time + duration;
        }

        public void Tick(IAi ai, AiContext ctx)
        {
            if (this.forced != null && !this.IsForced)
            {
                this.forced = null;
            }

            var best = this.forced ?? this.Choose(ai, ctx);

            if (best != this.current)
            {
                this.current?.End(ai);
                this.current = best;
                this.current?.Begin(ai);
            }

            this.current?.Perform(ai, ctx);
        }

        private IUtilityAction? Choose(IAi ai, AiContext ctx)
        {
            IUtilityAction? best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var action in this.actions)
            {
                float score = action.Score(ai, ctx);
                if (action == this.current && score > 0f)
                {
                    score += this.hysteresis;
                }
                if (score > bestScore)
                {
                    bestScore = score;
                    best = action;
                }
            }

            return best;
        }
    }
}
