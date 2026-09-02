namespace HackedDesign
{
    public abstract class Subroutine : Item
    {
        public abstract void OnInstall(Hack hack);
        public abstract void OnUninstall(Hack hack);
    }
}
