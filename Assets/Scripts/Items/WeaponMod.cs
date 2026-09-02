namespace HackedDesign
{
    public abstract class WeaponMod : Item
    {
        public abstract void OnInstall(WeaponInstance weapon);
        public abstract void OnUninstall(WeaponInstance weapon);
    }
}
