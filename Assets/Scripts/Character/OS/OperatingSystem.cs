using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    public class OperatingSystem : MonoBehaviour
    {
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private CharacterSettings charSettings;
        [SerializeField] private float healFactor = 0;
        [SerializeField] private List<OSHex> osHexList = new List<OSHex>();
                
        [SerializeField] public Action changeActions;
        [SerializeField] public Action dieActions;
        [SerializeField] public Action hitActions;
        [SerializeField] public int oxHexCount = 1;
        private CharacterData characterData = new();

        [SerializeField] private List<Hack> repoHacks = new();

        void Start()
        {
            HideAll();
        }

        public void HideAll()
        {
            foreach(var hex in osHexList)
            {
                hex.Hide();
            }
        }
        public void Show()
        {
            for (int i = 0; i < osHexList.Count; i++)
            {
                osHexList[i].Show();
            }
        }

        public void Hide()
        {
            for (int i = 0; i < osHexList.Count; i++)
            {
                osHexList[i].Hide();
            }
        }

        public void UpdateBehaviour(bool hackMode)
        {
            Health += healFactor * Time.deltaTime;
             for (int i = 0; i < osHexList.Count; i++)
            {
                if(hackMode)
                {
                    osHexList[i].Show();
                }
                else
                {
                    osHexList[i].Hide();
                }
            }
        }

        public float PingRadius => this.characterData.pingRadius;

        public IReadOnlyList<Hack> ActiveHacks => this.characterData.hackSlots.Where(h => h != null).Select(h => h.definition).ToList();
        public List<Hack> RepoHacks { get => repoHacks; set => repoHacks = value; }

        public float Health
        {
            get => this.characterData.health;
            set
            {
                float prevHealth = this.characterData.health;
                this.characterData.health = Mathf.Clamp(value, 0, characterData.maxHealth);
                if (prevHealth != this.characterData.health)
                {
                    changeActions?.Invoke();
                    if (this.characterData.health < prevHealth)
                    {
                        hitActions?.Invoke();
                    }
                    if (characterData.health <= 0)
                    {
                        dieActions?.Invoke();
                    }
                }
            }
        }

        public float MaxHealth
        {
            get => this.characterData.maxHealth;
        }

        public int CurrentMission { get => this.characterData.currentMission; set => this.characterData.currentMission = value; }
        public int KineticLevel => this.characterData.kinetic;
        public int DigitalLevel => this.characterData.digital;
        public int GhostLevel => this.characterData.ghost;
        public WeaponInstance ActiveWeapon => this.characterData.weaponSlots[this.characterData.currentWeaponSlot];

        public WeaponSettings CurrentWeapon => ActiveWeapon?.definition;
        public IReadOnlyList<WeaponInstance> WeaponSlots => this.characterData.weaponSlots;
        public int CurrentWeaponSlot => this.characterData.currentWeaponSlot;

        public void SelectWeapon(WeaponSlotId slot)
        {
            if (this.characterData.weaponSlots[(int)slot] == null)
            {
                return;
            }
            this.characterData.currentWeaponSlot = (int)slot;
            changeActions?.Invoke();
        }

        public void NextWeapon()
        {
            int next = NextFilled(this.characterData.weaponSlots, this.characterData.currentWeaponSlot, 1);
            if (next == this.characterData.currentWeaponSlot)
            {
                return;
            }
            this.characterData.currentWeaponSlot = next;
            changeActions?.Invoke();
        }

        public void PrevWeapon()
        {
            int next = NextFilled(this.characterData.weaponSlots, this.characterData.currentWeaponSlot, -1);
            if (next == this.characterData.currentWeaponSlot)
            {
                return;
            }
            this.characterData.currentWeaponSlot = next;
            changeActions?.Invoke();
        }

        public Hack CurrentHack => this.characterData.hackSlots[this.characterData.currentHackSlot]?.definition;
        public int CurrentHackSlot => this.characterData.currentHackSlot;

        public void NextHack()
        {
            int next = NextFilled(this.characterData.hackSlots, this.characterData.currentHackSlot, 1);
            if (next == this.characterData.currentHackSlot)
            {
                return;
            }
            this.characterData.currentHackSlot = next;
            changeActions?.Invoke();
        }

        public void PrevHack()
        {
            int next = NextFilled(this.characterData.hackSlots, this.characterData.currentHackSlot, -1);
            if (next == this.characterData.currentHackSlot)
            {
                return;
            }
            this.characterData.currentHackSlot = next;
            changeActions?.Invoke();
        }

        private static int NextFilled<T>(T[] slots, int start, int direction) where T : class
        {
            int count = slots.Length;
            for (int step = 1; step <= count; step++)
            {
                int index = ((start + direction * step) % count + count) % count;
                if (slots[index] != null)
                {
                    return index;
                }
            }
            return start;
        }

        public OSTab CurrentTab { 
            get => this.characterData.currentTab; 
            set { 
                this.characterData.currentTab = value; 
                changeActions?.Invoke(); 
            } 
        }

        public int Ammo { 
            get => this.characterData.ammo; 
            set
            {
                this.characterData.ammo = value;
                changeActions?.Invoke();
            }
        }

        public bool Enabled
        {
            get => this.characterData.enabled;
            set {
                this.characterData.enabled = value;
                changeActions?.Invoke();
            }
        }

        public float Momentum
        {
            get => gameSettings.InfiniteMomentum ? this.characterData.maxMomentum : this.characterData.momentum;
            set
            {
                this.characterData.momentum = Mathf.Clamp(value, 0, this.characterData.maxMomentum - this.characterData.preallocatedEnergy);
                changeActions?.Invoke();
            }
        }

        public float MaxMomentum => this.characterData.maxMomentum;

        public float PreallocatedEnergy => this.characterData.preallocatedEnergy;

        private void Awake() => this.AutoBind(ref characterData);

        public void Reset()
        {
            characterData.Reset(charSettings);
            foreach (var instance in characterData.hackSlots)
            {
                if (instance == null)
                {
                    continue;
                }
                foreach (var sub in instance.installed)
                {
                    if(sub.EnsureNotNull(nameof(sub)))
                    {
                        sub.OnInstall(instance.definition);
                    }
                }
            }
            foreach (var instance in characterData.upgradeSlots)
            {
                instance?.definition.Apply(this);
            }
            foreach (var instance in characterData.weaponSlots)
            {
                if (instance == null)
                {
                    continue;
                }
                foreach (var mod in instance.installed)
                {
                    if(mod.EnsureNotNull(nameof(mod)))
                    {
                        mod.OnInstall(instance);
                    }
                    
                }
            }
            changeActions?.Invoke();
        }

        public void Trigger(int slot)
        {
            Debug.Log("os trigger " + slot);

            /*
            var hackSlots = this.characterData.hackSlots;
            if (slot < hackSlots.Length && hackSlots[slot] != null)
            {
                var hack = hackSlots[slot].definition;
                gameData.AddTask(new OSTask(hack.name, hack.puUsage));
                hack.Trigger(null);
            }*/
        }

        public IReadOnlyList<Item> Inventory => this.characterData.inventory;
        public IReadOnlyList<HackInstance> HackSlots => this.characterData.hackSlots;
        public IReadOnlyList<UpgradeInstance> UpgradeSlots => this.characterData.upgradeSlots;

        public bool AddItem(Item item)
        {
            if (item == null)
            {
                return false;
            }
            int slot = Array.IndexOf(this.characterData.inventory, null);
            if (slot < 0)
            {
                return false;
            }
            this.characterData.inventory[slot] = item;
            changeActions?.Invoke();
            return true;
        }

        public bool RemoveItem(int slot)
        {
            if (slot < 0 || slot >= this.characterData.inventory.Length)
            {
                return false;
            }
            if (this.characterData.inventory[slot] == null)
            {
                return false;
            }
            this.characterData.inventory[slot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool MoveItem(int from, int to)
        {
            var inv = this.characterData.inventory;
            if (from < 0 || from >= inv.Length || to < 0 || to >= inv.Length || from == to)
            {
                return false;
            }
            (inv[to], inv[from]) = (inv[from], inv[to]);
            changeActions?.Invoke();
            return true;
        }

        public bool EquipHack(int inventorySlot, int hackSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.hackSlots;
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (hackSlot < 0 || hackSlot >= slots.Length)
            {
                return false;
            }
            if (slots[hackSlot] != null)
            {
                return false;
            }
            if (inv[inventorySlot] is not Hack hack)
            {
                return false;
            }

            slots[hackSlot] = new HackInstance(hack);
            inv[inventorySlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool EquipUpgrade(int inventorySlot, int upgradeSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.upgradeSlots;
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (upgradeSlot < 0 || upgradeSlot >= slots.Length)
            {
                return false;
            }
            if (slots[upgradeSlot] != null)
            {
                return false;
            }
            if (inv[inventorySlot] is not Upgrade upgrade)
            {
                return false;
            }

            slots[upgradeSlot] = new UpgradeInstance(upgrade);
            inv[inventorySlot] = null;
            upgrade.Apply(this);
            changeActions?.Invoke();
            return true;
        }

        public bool UnequipHack(int hackSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.hackSlots;
            if (hackSlot < 0 || hackSlot >= slots.Length)
            {
                return false;
            }
            var instance = slots[hackSlot];
            if (instance == null)
            {
                return false;
            }

            int required = 1 + instance.installed.Count(s => s != null);
            if (inv.Count(s => s == null) < required)
            {
                return false;
            }

            inv[Array.IndexOf(inv, null)] = instance.definition;
            for (int i = 0; i < instance.installed.Length; i++)
            {
                var sub = instance.installed[i];
                if (sub == null)
                {
                    continue;
                }
                sub.OnUninstall(instance.definition);
                inv[Array.IndexOf(inv, null)] = sub;
                instance.installed[i] = null;
            }
            slots[hackSlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool UnequipUpgrade(int upgradeSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.upgradeSlots;
            if (upgradeSlot < 0 || upgradeSlot >= slots.Length)
            {
                return false;
            }
            var instance = slots[upgradeSlot];
            if (instance == null)
            {
                return false;
            }
            int slot = Array.IndexOf(inv, null);
            if (slot < 0)
            {
                return false;
            }
            instance.definition.Remove(this);
            inv[slot] = instance.definition;
            slots[upgradeSlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public IReadOnlyList<Subroutine> InstalledSubroutines(int hackSlot)
        {
            var slots = this.characterData.hackSlots;
            if (hackSlot < 0 || hackSlot >= slots.Length || slots[hackSlot] == null)
            {
                return null;
            }
            return slots[hackSlot].installed;
        }

        public bool InstallSubroutine(int hackSlot, int subroutineSlot, int inventorySlot)
        {
            var slots = this.characterData.hackSlots;
            var inv = this.characterData.inventory;
            if (hackSlot < 0 || hackSlot >= slots.Length)
            {
                return false;
            }
            var instance = slots[hackSlot];
            if (instance == null)
            {
                return false;
            }
            if (subroutineSlot < 0 || subroutineSlot >= instance.installed.Length)
            {
                return false;
            }
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (inv[inventorySlot] is not Subroutine sub)
            {
                return false;
            }

            var displaced = instance.installed[subroutineSlot];
            if(displaced.EnsureNotNull(nameof(displaced)))
            {
                displaced.OnUninstall(instance.definition);
            }
            
            inv[inventorySlot] = displaced;
            instance.installed[subroutineSlot] = sub;
            sub.OnInstall(instance.definition);
            changeActions?.Invoke();
            return true;
        }

        public bool UninstallSubroutine(int hackSlot, int subroutineSlot)
        {
            var slots = this.characterData.hackSlots;
            if (hackSlot < 0 || hackSlot >= slots.Length)
            {
                return false;
            }
            var instance = slots[hackSlot];
            if (instance == null)
            {
                return false;
            }
            if (subroutineSlot < 0 || subroutineSlot >= instance.installed.Length)
            {
                return false;
            }
            var sub = instance.installed[subroutineSlot];
            if (sub == null)
            {
                return false;
            }
            int slot = System.Array.IndexOf(this.characterData.inventory, null);
            if (slot < 0)
            {
                return false;
            }
            sub.OnUninstall(instance.definition);
            this.characterData.inventory[slot] = sub;
            instance.installed[subroutineSlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool EquipWeapon(int inventorySlot)
        {
            var inv = this.characterData.inventory;
            if (this.characterData.weaponSlots[(int)WeaponSlotId.Primary] != null)
            {
                return false;
            }
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (inv[inventorySlot] is not WeaponSettings weapon)
            {
                return false;
            }

            this.characterData.weaponSlots[(int)WeaponSlotId.Primary] = new WeaponInstance(weapon);
            inv[inventorySlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool UnequipWeapon()
        {
            var inv = this.characterData.inventory;
            var instance = this.characterData.weaponSlots[(int)WeaponSlotId.Primary];
            if (instance == null) 
            { 
                return false; 
            }

            int required = 1 + instance.installed.Count(m => m != null);
            if (inv.Count(s => s == null) < required)
            {
                return false;
            }

            inv[System.Array.IndexOf(inv, null)] = instance.definition;
            for (int i = 0; i < instance.installed.Length; i++)
            {
                var mod = instance.installed[i];
                if (mod == null)
                {
                    continue;
                }
                mod.OnUninstall(instance);
                inv[System.Array.IndexOf(inv, null)] = mod;
                instance.installed[i] = null;
            }
            this.characterData.weaponSlots[(int)WeaponSlotId.Primary] = null;
            if (this.characterData.currentWeaponSlot == (int)WeaponSlotId.Primary)
            {
                this.characterData.currentWeaponSlot = (int)WeaponSlotId.Melee;
            }
            changeActions?.Invoke();
            return true;
        }

        public bool InstallWeaponMod(WeaponSlotId slot, int inventorySlot)
        {
            var inv = this.characterData.inventory;
            var instance = this.characterData.weaponSlots[(int)slot];
            if (instance == null)
            {
                return false;
            }
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (inv[inventorySlot] is not WeaponMod mod)
            {
                return false;
            }

            var displaced = instance.installed[0];
            if(displaced.EnsureNotNull(nameof(displaced)))
            {
                displaced.OnUninstall(instance);
            }
            
            inv[inventorySlot] = displaced;
            instance.installed[0] = mod;
            mod.OnInstall(instance);
            changeActions?.Invoke();
            return true;
        }

        public bool UninstallWeaponMod(WeaponSlotId slot)
        {
            var instance = this.characterData.weaponSlots[(int)slot];
            if (instance == null)
            {
                return false;
            }
            var mod = instance.installed[0];
            if (mod == null)
            {
                return false;
            }
            int emptySlot = System.Array.IndexOf(this.characterData.inventory, null);
            if (emptySlot < 0)
            {
                return false;
            }
            mod.OnUninstall(instance);
            this.characterData.inventory[emptySlot] = mod;
            instance.installed[0] = null;
            changeActions?.Invoke();
            return true;
        }

        public void DecreaseHealth(int amount) => Health = Mathf.Max(Health - amount, 0);

        public bool HasAmmo => this.characterData.infiniteAmmo || this.characterData.ammo > 0;

        public void DecreaseAmmo(int amount = 1)
        {
            if (this.characterData.infiniteAmmo)
            {
                return;
            }

            Ammo = Mathf.Max(Ammo - amount, 0);
        }

    }
}

