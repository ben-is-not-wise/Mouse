using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Mouse/Settings/Game")]
    public class GameSettings : ScriptableObject
    {
        // FIXME: Make these serialized properties
        [SerializeField] private bool debug = false;
        [SerializeField] private bool skipIntro = false;
        [SerializeField] private bool startPistol = false;
        [SerializeField] private bool infiniteMomentum = false;
        [SerializeField] private float fallDeathHeight = 27f;
        [SerializeField] private float fallDamageHeight = 20f;
        [SerializeField] private float shatterMagnitude = 10.1f;
        [SerializeField] private float interactDistance = 2.0f;
        [SerializeField] private float defaultLevelTime = 64f;
        [SerializeField] private float knockbackAmount = 1f;
        [SerializeField] private float knockbackTime = 0.2f;
        [SerializeField] private float knockbackFreezeTime = 0.2f;

        public bool Debug => debug;
        public bool SkipIntro => skipIntro;
        public bool StartPistol => startPistol;
        public bool InfiniteMomentum => infiniteMomentum;
        public float FallDeathHeight => fallDeathHeight;
        public float FallDamageHeight => fallDamageHeight;
        public float ShatterMagnitude => shatterMagnitude;
        public float InteractDistance => interactDistance;
        public float DefaultLevelTime => this.defaultLevelTime;

        public float KnockbackAmount { get => this.knockbackAmount; set => this.knockbackAmount = value; }
        public float KnockbackTime { get => this.knockbackTime; set => this.knockbackTime = value; }
        public float KnockbackFreezeTime { get => this.knockbackFreezeTime; set => this.knockbackFreezeTime = value; }
    }
}
