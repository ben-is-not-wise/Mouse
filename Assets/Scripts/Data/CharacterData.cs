using System.Collections.Generic;

namespace HackedDesign
{
    public class CharacterData
    {
        private List<OSTask> puTaskList = new();
        public List<OSTask> PuTaskList { get => puTaskList; set => puTaskList = value; }
        public string saveName;
        public int maxHealth = 100;
        public float health = 0;
        public int ammo = 0;
        public int ram = 100;
        public int maxRam = 100;
        public int pingRadius = 10;
        public bool infiniteAmmo = false;
        public bool infinityHealth = false;
        public int currentMission = 2;
        public float momentumFactor = 0.05f;
        public float momentum = 0.0f;
        public float maxMomentum = 5.0f;
        public float preallocatedEnergy = 2.0f;

        public int kinetic = 1;
        public int digital = 1;
        public int stealth = 1;

        public List<WeaponSettings> weapons = new List<WeaponSettings>();

        public int currentWeaponSlot = 0;

        public OSTab currentTab;

        public Dictionary<int, int> hacks = new();

        public void Reset(CharacterSettings settings)
        {
            health = settings.StartingHealth;
            ammo = settings.StartingAmmo;
            infiniteAmmo = settings.InfiniteAmmo;
            infinityHealth = settings.InfiniteHealth;
            weapons.AddRange(settings.WeaponSettings);
            currentWeaponSlot = 0;
            maxHealth = settings.StartingHealth;
            maxMomentum = settings.MaxMomentum;
            momentum = 0.0f;
        }
    }

    public class OSTask

    {
        private readonly string name;
        private readonly float amount;

        public OSTask(string name, float amount)
        {
            this.name = name;
            this.amount = amount;
        }

        public float Amount => amount;

        public string Name => name;
    }

    public enum OSTab
    {
        Character,
        Inventory,
        Shop,
        Music,
        Info
    }
}
