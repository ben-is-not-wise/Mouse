using System.Collections.Generic;

namespace HackedDesign
{
    public class CharacterData
    {
        private List<OSTask> puTaskList = new();
        public List<OSTask> PuTaskList { get => puTaskList; set => puTaskList = value; }
        public string saveName;

        public bool enabled = false;
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
        public int ghost = 1;

        public OSTab currentTab;

        public Dictionary<int, int> hacks = new();

        public const int InventorySize = 16;
        public const int HackSlotCount = 4;
        public const int UpgradeSlotCount = 4;
        public const int WeaponSlotCount = 3;

        public WeaponInstance[] weaponSlots = new WeaponInstance[WeaponSlotCount];
        public int currentWeaponSlot = (int)WeaponSlotId.Melee;
        public int currentHackSlot = 0;

        public Item[] inventory = new Item[InventorySize];
        public HackInstance[] hackSlots = new HackInstance[HackSlotCount];
        public UpgradeInstance[] upgradeSlots = new UpgradeInstance[UpgradeSlotCount];

        public void Reset(CharacterSettings settings)
        {
            health = settings.StartingHealth;
            ammo = settings.StartingAmmo;
            infiniteAmmo = settings.InfiniteAmmo;
            infinityHealth = settings.InfiniteHealth;
            weaponSlots[(int)WeaponSlotId.Melee] = settings.DefaultMelee != null ? new WeaponInstance(settings.DefaultMelee) : null;
            weaponSlots[(int)WeaponSlotId.Primary] = settings.StartingWeapon != null ? new WeaponInstance(settings.StartingWeapon) : null;
            weaponSlots[(int)WeaponSlotId.Grenade] = settings.StartingGrenade != null ? new WeaponInstance(settings.StartingGrenade) : null;
            currentWeaponSlot = weaponSlots[(int)WeaponSlotId.Primary] != null ? (int)WeaponSlotId.Primary : (int)WeaponSlotId.Melee;
            maxHealth = settings.StartingHealth;
            maxMomentum = settings.MaxMomentum;
            momentum = 0.0f;

            System.Array.Clear(inventory, 0, inventory.Length);
            System.Array.Clear(hackSlots, 0, hackSlots.Length);
            System.Array.Clear(upgradeSlots, 0, upgradeSlots.Length);

            var startInventory = settings.StartingInventory;
            for (int i = 0; i < startInventory.Count && i < inventory.Length; i++)
            {
                inventory[i] = startInventory[i];
            }

            var startHacks = settings.StartingHacks;
            for (int i = 0; i < startHacks.Count && i < hackSlots.Length; i++)
            {
                var loadout = startHacks[i];
                if (loadout == null || loadout.hack == null)
                {
                    continue;
                }
                var instance = new HackInstance(loadout.hack);
                if (loadout.subroutines != null)
                {
                    for (int j = 0; j < loadout.subroutines.Length && j < instance.installed.Length; j++)
                    {
                        instance.installed[j] = loadout.subroutines[j];
                    }
                }
                hackSlots[i] = instance;
            }

            currentHackSlot = System.Math.Max(0, System.Array.FindIndex(hackSlots, h => h != null));

            var startUpgrades = settings.StartingUpgrades;
            for (int i = 0; i < startUpgrades.Count && i < upgradeSlots.Length; i++)
            {
                if (startUpgrades[i] != null)
                {
                    upgradeSlots[i] = new UpgradeInstance(startUpgrades[i]);
                }
            }
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
