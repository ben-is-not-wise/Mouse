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
               
        [SerializeField] public Action changeActions;
        [SerializeField] public Action dieActions;
        [SerializeField] public Action hitActions;
        [SerializeField] public BarrierIndicator barrier;
        [SerializeField] private Vector3 barrierOffset = new(0f, 1.5f, 0f);
        private CharacterData characterData = new();

        [SerializeField] private List<Hack> repoHacks = new();

        [Header("Debug")]
        [SerializeField] private float visibility;

        void Start()
        {
            CurrentTab = OSTab.Tasks;
        }

        
        public void UpdateBehaviour()
        {
            Health += healFactor * Time.deltaTime;

            if (barrier.OrNull() != null && characterData.barrierSlot != null)
            {
                characterData.barrierSlot.Tick(Time.time);
                characterData.currentBarrierRotation += Time.deltaTime * characterData.barrierSlot.stats.Get(ItemStatKeys.BarrierRotationSpeed);

                barrier.Repaint(characterData.barrierSlot, characterData.currentBarrierRotation, true);
            }
        }

        /// <summary>Shows the barrier in full for a while (called when the character is Pinged).</summary>
        public void RevealBarrier()
        {
            if (barrier.OrNull() != null)
            {
                barrier.Reveal();
            }
        }

        public void ResetBarrierRotation()
        {
            this.characterData.currentBarrierRotation = 0;
        }

        public Vector3 BarrierCentre => barrier.OrNull() != null ? barrier.transform.position : transform.position + barrierOffset;

        /// <summary>The barrier edge facing a world position, allowing for rotation. -1 with no barrier.</summary>
        public int FacingEdge(Vector2 from)
        {
            var slot = characterData.barrierSlot;
            if (slot == null || slot.Faces == 0)
            {
                return -1;
            }

            // Same convention as HexIndicator: vertex 0 on +X, counter-clockwise, edge i spans vertex i to i+1.
            Vector2 direction = from - (Vector2)BarrierCentre;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - characterData.currentBarrierRotation;
            float sector = 360f / slot.Faces;
            return Mathf.Clamp(Mathf.FloorToInt(Mathf.Repeat(angle, 360f) / sector), 0, slot.Faces - 1);
        }

        public bool BreakFacingEdge(Vector2 from)
        {
            var slot = characterData.barrierSlot;
            int edge = FacingEdge(from);
            if (edge < 0)
            {
                return false;
            }
            // Stats so subroutines and hacks can push these up or down; the Barrier asset is the base value.
            float delay = Mathf.Max(0f, slot.stats.Get(ItemStatKeys.BarrierRegrowDelay, slot.definition.RegrowDelay));
            float duration = Mathf.Max(0f, slot.stats.Get(ItemStatKeys.BarrierRegrowTime, slot.definition.RegrowTime));
            return slot.BreakEdge(edge, delay, duration);
        }

        /// <summary>World-space end points of a barrier edge, allowing for rotation.</summary>
        public bool TryGetEdge(int edge, out Vector2 start, out Vector2 end)
        {
            var slot = characterData.barrierSlot;
            if (slot == null || edge < 0 || edge >= slot.Faces)
            {
                start = end = BarrierCentre;
                return false;
            }

            Vector2 centre = BarrierCentre;
            float radius = slot.definition.Radius;
            float sector = 360f / slot.Faces;
            float a0 = (characterData.currentBarrierRotation + edge * sector) * Mathf.Deg2Rad;
            float a1 = a0 + sector * Mathf.Deg2Rad;
            start = centre + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
            end = centre + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
            return true;
        }

        // Characters set up in a prefab without a BarrierIndicator (e.g. the target dummy) get one
        // built at runtime when they start with a barrier.
        private void EnsureBarrierIndicator()
        {
            if (barrier.OrNull() != null || characterData.barrierSlot == null)
            {
                return;
            }

            var go = new GameObject("Barrier");
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = barrierOffset;

            var hex = go.AddComponent<HexIndicator>();
            hex.UseTransformRotation = false;

            barrier = go.AddComponent<BarrierIndicator>();
            barrier.indicator = hex;
        }

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
                        if (characterData.infinityHealth)
                        {
                            this.characterData.health = characterData.maxHealth;
                        }
                        else
                        {
                            dieActions?.Invoke();
                        }
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
        public WeaponInstance MeleeWeapon => this.characterData.meleeSlot;
        public ItemInstance ActiveAction => this.characterData.actionSlots[this.characterData.currentActionSlot];
        public WeaponInstance ActiveActionWeapon => ActiveAction as WeaponInstance;
        public HackInstance ActiveActionHack => ActiveAction as HackInstance;

        /// <summary>The installed Ping hack, fired from its own button. Null if none is installed.</summary>
        public HackInstance PingHack => this.characterData.hackSlots.FirstOrDefault(h => h?.definition is Ping);
        public IReadOnlyList<ItemInstance> ActionSlots => this.characterData.actionSlots;
        public int CurrentActionSlot => this.characterData.currentActionSlot;

        public void SelectAction(int slot)
        {
            if (slot < 0 || slot >= this.characterData.actionSlots.Length || this.characterData.actionSlots[slot] == null)
            {
                return;
            }
            this.characterData.currentActionSlot = slot;
            changeActions?.Invoke();
        }

        public void NextAction()
        {
            int next = NextFilled(this.characterData.actionSlots, this.characterData.currentActionSlot, 1);
            if (next == this.characterData.currentActionSlot)
            {
                return;
            }
            this.characterData.currentActionSlot = next;
            changeActions?.Invoke();
        }

        public void PrevAction()
        {
            int next = NextFilled(this.characterData.actionSlots, this.characterData.currentActionSlot, -1);
            if (next == this.characterData.currentActionSlot)
            {
                return;
            }
            this.characterData.currentActionSlot = next;
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

        public float Visibility
        {
            get => visibility;
            private set
            {
                if (visibility != value)
                {
                    visibility = value;
                    changeActions?.Invoke();
                }
            }
        }

        public void UpdateVisibility(bool crouched, bool moving, bool inShadow = false, float? zoneVisibility = null)
        {
            if (charSettings == null)
            {
                return;
            }

            if (zoneVisibility.HasValue)
            {
                Visibility = zoneVisibility.Value;
                return;
            }

            if (inShadow && crouched && !moving)
            {
                Visibility = charSettings.VisibilityCrouchedInShadow;
                return;
            }

            var baseVisibility = crouched
                ? (moving ? charSettings.VisibilityCrouchedMoving : charSettings.VisibilityCrouched)
                : charSettings.VisibilityNormal;

            Visibility = inShadow ? baseVisibility * charSettings.ShadowVisibilityMultiplier : baseVisibility;
        }

        private void Awake() => this.AutoBind(ref characterData);

        public void Reset()
        {
            characterData.Reset(charSettings);
            UpdateVisibility(false, false);
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
            if (characterData.barrierSlot != null)
            {
                characterData.barrierSlot.definition.Apply(this);
                foreach (var sub in characterData.barrierSlot.installed)
                {
                    if (sub.EnsureNotNull(nameof(sub)))
                    {
                        sub.OnInstall(characterData.barrierSlot.definition);
                    }
                }
                EnsureBarrierIndicator();
            }
            foreach (var instance in characterData.actionSlots.OfType<WeaponInstance>().Append(characterData.meleeSlot))
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

        public HackResult TriggerHack(HackInstance instance, Interactable target, PlayerController player, HackAimLine aimLine = null)
        {
            if (instance?.definition == null)
            {
                return HackResult.NoHack;
            }

            var hack = instance.definition;

            if (Time.time < instance.readyTime)
            {
                return HackResult.CoolingDown;
            }

            if (hack.RequiresTarget)
            {
                if (target == null)
                {
                    return HackResult.NoTarget;
                }

                if ((target.transform.position - player.transform.position).sqrMagnitude > hack.range * hack.range)
                {
                    return HackResult.OutOfRange;
                }
            }

            if (Momentum < hack.puUsage)
            {
                return HackResult.NoEnergy;
            }

            Momentum -= hack.puUsage;
            instance.readyTime = Time.time + hack.puTime;
            var targetObject = target != null ? target.gameObject : null;
            if (hack.RequiresTarget && aimLine != null)
            {
                aimLine.Fire(hack, targetObject, player);
            }
            else
            {
                hack.Trigger(targetObject, player);
            }
            return HackResult.Success;
        }

        public IReadOnlyList<Item> Inventory => this.characterData.inventory;
        public IReadOnlyList<HackInstance> HackSlots => this.characterData.hackSlots;
        public IReadOnlyList<UpgradeInstance> UpgradeSlots => this.characterData.upgradeSlots;
        public BarrierInstance BarrierSlot => this.characterData.barrierSlot;

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
            var actions = this.characterData.actionSlots;
            for (int i = 0; i < actions.Length; i++)
            {
                if (actions[i] == instance)
                {
                    actions[i] = null;
                }
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

        public bool EquipBarrier(int inventorySlot)
        {
            var inv = this.characterData.inventory;
            if (this.characterData.barrierSlot != null)
            {
                return false;
            }
            if (inventorySlot < 0 || inventorySlot >= inv.Length)
            {
                return false;
            }
            if (inv[inventorySlot] is not Barrier barrier)
            {
                return false;
            }

            this.characterData.barrierSlot = new BarrierInstance(barrier);
            inv[inventorySlot] = null;
            barrier.Apply(this);
            changeActions?.Invoke();
            return true;
        }

        public bool UnequipBarrier()
        {
            var inv = this.characterData.inventory;
            var instance = this.characterData.barrierSlot;
            if (instance == null)
            {
                return false;
            }

            int required = 1 + instance.installed.Count(s => s != null);
            if (inv.Count(s => s == null) < required)
            {
                return false;
            }

            instance.definition.Remove(this);
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
            this.characterData.barrierSlot = null;
            changeActions?.Invoke();
            return true;
        }

        public bool InstallBarrierSubroutine(int subroutineSlot, int inventorySlot)
        {
            var instance = this.characterData.barrierSlot;
            var inv = this.characterData.inventory;
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
            if (displaced.EnsureNotNull(nameof(displaced)))
            {
                displaced.OnUninstall(instance.definition);
            }

            inv[inventorySlot] = displaced;
            instance.installed[subroutineSlot] = sub;
            sub.OnInstall(instance.definition);
            changeActions?.Invoke();
            return true;
        }

        public bool UninstallBarrierSubroutine(int subroutineSlot)
        {
            var instance = this.characterData.barrierSlot;
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
            int slot = Array.IndexOf(this.characterData.inventory, null);
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

        public bool EquipAction(int inventorySlot, int actionSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.actionSlots;
            if (actionSlot < 0 || actionSlot >= slots.Length)
            {
                return false;
            }
            if (slots[actionSlot] != null)
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

            slots[actionSlot] = new WeaponInstance(weapon);
            inv[inventorySlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool UnequipAction(int actionSlot)
        {
            var inv = this.characterData.inventory;
            var slots = this.characterData.actionSlots;
            if (actionSlot < 0 || actionSlot >= slots.Length)
            {
                return false;
            }
            if (slots[actionSlot] is not WeaponInstance instance)
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
            slots[actionSlot] = null;
            changeActions?.Invoke();
            return true;
        }

        public bool InstallWeaponMod(int actionSlot, int inventorySlot)
        {
            var inv = this.characterData.inventory;
            if (actionSlot < 0 || actionSlot >= this.characterData.actionSlots.Length)
            {
                return false;
            }
            if (this.characterData.actionSlots[actionSlot] is not WeaponInstance instance)
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

        public bool UninstallWeaponMod(int actionSlot)
        {
            if (actionSlot < 0 || actionSlot >= this.characterData.actionSlots.Length)
            {
                return false;
            }
            if (this.characterData.actionSlots[actionSlot] is not WeaponInstance instance)
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

