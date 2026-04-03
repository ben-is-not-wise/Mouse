#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "Mouse/Settings/Character")]
    public class CharacterSettings : ScriptableObject
    {
        [SerializeField] private float attackRate = 0.33f;
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 9;
        [SerializeField] private float crouchSpeed = 2;
        [SerializeField] private float shootDistance = 10.0f;
        [SerializeField] private float meleeDistance = 10.0f;
        [SerializeField] private float interactDistance = 1.0f;
        [SerializeField] private LayerMask attackMask = 0;
        [SerializeField] private LayerMask interactMask = 0;
        [SerializeField] private float alertRadius = 20;
        //[SerializeField] public int maxHealth = 100;
        [SerializeField] private int startingHealth = 100;
        [SerializeField] private int startingAmmo = 6;
        [SerializeField] private bool infiniteAmmo = false;
        [SerializeField] private bool infiniteHealth = false;
        [SerializeField] private List<WeaponSettings> weaponSettings = new();
        [SerializeField] private float momentumAirLoss = 0.5f;
        [SerializeField] private float baseMomentumFactor = 0.1f;
        [SerializeField] private float attackMomentumLoss = 0.5f;
        [SerializeField] private float minMomentum = 0f;
        [SerializeField] private float maxMomentum = 5f;
        [SerializeField] private FXType hitfx = FXType.Blood;
        [SerializeField] private float timeSlowSpeed = 0.5f;
        [SerializeField] private float anticipateDelay = 0.2f;
        [SerializeField] private float knockbackAmount = 1f;
        [SerializeField] private float knockbackTime = 0.2f;
        [SerializeField] private float knockbackFreezeTime = 0.2f;

        public float ShootDistance { get => shootDistance; private set => shootDistance = value; }
        public float AttackRate { get => attackRate; private set => attackRate = value; }
        public float WalkSpeed { get => walkSpeed; private set => walkSpeed = value; }
        public float RunSpeed { get => runSpeed; private set => runSpeed = value; }
        public float CrouchSpeed { get => crouchSpeed; private set => crouchSpeed = value; }
        public float InteractDistance { get => interactDistance; private set => interactDistance = value; }
        public float MeleeDistance { get => meleeDistance; private set => meleeDistance = value; }
        public LayerMask AttackMask { get => attackMask; private set => attackMask = value; }
        public FXType HitFX { get => hitfx; private set => hitfx = value; }
        public LayerMask InteractMask { get => this.interactMask; set => this.interactMask = value; }
        public int StartingHealth { get => this.startingHealth; set => this.startingHealth = value; }
        public float MaxMomentum { get => this.maxMomentum; set => this.maxMomentum = value; }
        public float MinMomentum { get => this.minMomentum; set => this.minMomentum = value; }
        public float AttackMomentumLoss { get => this.attackMomentumLoss; set => this.attackMomentumLoss = value; }
        public float BaseMomentumFactor { get => this.baseMomentumFactor; set => this.baseMomentumFactor = value; }
        public float MomentumLoss { get => this.momentumAirLoss; set => this.momentumAirLoss = value; }
        public List<WeaponSettings> WeaponSettings { get => this.weaponSettings; set => this.weaponSettings = value; }
        public bool InfiniteHealth { get => this.infiniteHealth; set => this.infiniteHealth = value; }
        public bool InfiniteAmmo { get => this.infiniteAmmo; set => this.infiniteAmmo = value; }
        public int StartingAmmo { get => this.startingAmmo; set => this.startingAmmo = value; }
        public float AlertRadius { get => this.alertRadius; set => this.alertRadius = value; }
        public float TimeSlowSpeed { get => this.timeSlowSpeed; set => this.timeSlowSpeed = value; }

        public float AnticipateDelay { get => this.anticipateDelay; private set => this.anticipateDelay = value; }

        public float KnockbackAmount { get => this.knockbackAmount; set => this.knockbackAmount = value; }
        public float KnockbackTime { get => this.knockbackTime; set => this.knockbackTime = value; }
        public float KnockbackFreezeTime { get => this.knockbackFreezeTime; set => this.knockbackFreezeTime = value; }

    }
}