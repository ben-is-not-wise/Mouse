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
        [SerializeField] private float healFactor = 0;
        [SerializeField] private List<OSHex> osHexList = new List<OSHex>();
                
        [SerializeField] public Action changeActions;
        [SerializeField] public Action dieActions;
        [SerializeField] public Action hitActions;
        [SerializeField] public int oxHexCount = 1;
        private CharacterData characterData = new();

        [SerializeField] private List<Hack> activeHacks = new();
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

        public void UpdateBehaviour()
        {
            Health += healFactor * Time.deltaTime;
             for (int i = 0; i < osHexList.Count; i++)
            {
                if(Game.Instance.HackMode)
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

        public List<Hack> ActiveHacks => activeHacks;
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
        public int StealthLevel => this.characterData.stealth;
        public WeaponSettings CurrentWeapon => this.characterData.weapons[this.characterData.currentWeaponSlot];

        public int GetWeaponSlotByName(string name) => this.characterData.weapons.FindIndex(x => x.name == name);

        public void SetWeapon(int slot) => this.characterData.currentWeaponSlot = slot;

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

        public float Momentum
        {
            get => Game.Instance.GameSettings.InfiniteMomentum ? this.characterData.maxMomentum : this.characterData.momentum;
            set
            {
                this.characterData.momentum = Mathf.Clamp(value, 0, this.characterData.maxMomentum - this.characterData.preallocatedEnergy);
                changeActions?.Invoke();
            }
        }

        public float MaxMomentum => this.characterData.maxMomentum;

        public float PreallocatedEnergy
        {
            get => this.characterData.preallocatedEnergy;
        }

        private void Awake() => this.AutoBind(ref characterData);

        public void Reset(CharacterSettings settings)
        {
            characterData.Reset(settings);
            changeActions?.Invoke();
        }

        public void Trigger(int slot)
        {
            Debug.Log("os trigger " + slot);

            /*
            if (activeHacks.Count > slot)
            {
                gameData.AddTask(new OSTask(activeHacks[slot].name, activeHacks[slot].puUsage));
                activeHacks[slot].Trigger(null);
            }*/
        }

        public void DecreaseHealth(int amount) => Health = Mathf.Max(Health - amount, 0);

        public bool HasAmmo => this.characterData.infiniteAmmo || this.characterData.ammo > 0;

        public void DecreaseAmmo(int amount = 1) => Ammo = Mathf.Max(Ammo - amount, 0);

    }
}

