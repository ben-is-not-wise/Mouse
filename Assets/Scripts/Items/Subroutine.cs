namespace HackedDesign
{
    public abstract class Subroutine : Item
    {
        public abstract void OnInstall(Item host);
        public abstract void OnUninstall(Item host);
    }
}
