namespace HackedDesign
{
    public class EmptyState : AbstractState
    {
        public override bool PlayerActionAllowed => false;
        public override bool Battle => false;
    }
}
