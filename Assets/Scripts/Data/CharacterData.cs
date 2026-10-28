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
        public const int ActionSlotCount = 8;

        public WeaponInstance meleeSlot;
        public ItemInstance[] actionSlots = new ItemInstance[ActionSlotCount];
        public int currentActionSlot = 0;
        public int currentHackSlot = 0;

        public Item[] inventory = new Item[InventorySize];
        public HackInstance[] hackSlots = new HackInstance[HackSlotCount];
        public UpgradeInstance[] upgradeSlots = new UpgradeInstance[UpgradeSlotCount];
        public BarrierInstance barrierSlot;
        public float currentBarrierRotation = 0;

        public void Reset(CharacterSettings settings)
        {
            health = settings.StartingHealth;
            ammo = settings.StartingAmmo;
            infiniteAmmo = settings.InfiniteAmmo;
            infinityHealth = settings.InfiniteHealth;
            meleeSlot = settings.DefaultMelee != null ? new WeaponInstance(settings.DefaultMelee) : null;
            System.Array.Clear(actionSlots, 0, actionSlots.Length);
            actionSlots[0] = settings.StartingWeapon != null ? new WeaponInstance(settings.StartingWeapon) : null;
            actionSlots[1] = settings.StartingGrenade != null ? new WeaponInstance(settings.StartingGrenade) : null;
            currentActionSlot = System.Math.Max(0, System.Array.FindIndex(actionSlots, a => a != null));
            maxHealth = settings.StartingHealth;
            maxMomentum = settings.MaxMomentum;
            momentum = 0.0f;

            System.Array.Clear(inventory, 0, inventory.Length);
            System.Array.Clear(hackSlots, 0, hackSlots.Length);
            System.Array.Clear(upgradeSlots, 0, upgradeSlots.Length);
            barrierSlot = null;

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

            var startBarrier = settings.StartingBarrier;
            if (startBarrier != null && startBarrier.barrier != null)
            {
                var instance = new BarrierInstance(startBarrier.barrier);
                if (startBarrier.subroutines != null)
                {
                    for (int j = 0; j < startBarrier.subroutines.Length && j < instance.installed.Length; j++)
                    {
                        instance.installed[j] = startBarrier.subroutines[j];
                    }
                }
                barrierSlot = instance;
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
        Tasks,
        Body,
        Stash,
        Data,
        Shop,
        Music,
        Shutdown
    }
}
