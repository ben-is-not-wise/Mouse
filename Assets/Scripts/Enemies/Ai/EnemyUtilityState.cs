#nullable enable

namespace HackedDesign
{
    public class EnemyUtilityState : IEnemyState
    {
        private readonly IAi ai;
        private readonly UtilityBrain brain;

        public bool IsAlive => true;

        public EnemyUtilityState(IAi ai, UtilityBrain brain)
        {
            this.ai = ai;
            this.brain = brain;
        }

        public void Begin() { }

        public void End() => this.ai.Icon.Hide();

        public void UpdateBehaviour(AiContext ctx) => brain.Tick(ai, ctx);
    }
}
