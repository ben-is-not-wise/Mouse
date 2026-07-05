#nullable enable

namespace HackedDesign
{
    public static class DefaultBrain
    {
        public static UtilityBrain Create() => new UtilityBrain(new IUtilityAction[]
        {
            new AttackAction(),
            new InvestigateAction(),
            new PatrolAction(),
        });
    }
}
