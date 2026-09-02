namespace HackedDesign
{
    public abstract class Upgrade : Item
    {
        public abstract void Apply(OperatingSystem os);
        public abstract void Remove(OperatingSystem os);
    }
}
