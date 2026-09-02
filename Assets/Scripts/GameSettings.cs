using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HackedDesign
{
    public enum SkipStage
    {
        None,
        SkipIntro,
        SkipTutLevel,
        SkipAct0
    }

    [CreateAssetMenu(fileName = "GameSettings", menuName = "Mouse/Settings/Game")]
    public class GameSettings : ScriptableObject
    {
        // FIXME: Make these serialized properties
        [SerializeField] private SkipStage skipStage = SkipStage.None;
        [SerializeField] private bool startPistol = false;
        [SerializeField] private bool infiniteMomentum = false;
        [SerializeField] private bool showDamageNumbers = false;
        [SerializeField] private float shatterMagnitude = 10.1f;
        [SerializeField] private float interactDistance = 2.0f;
        [SerializeField] private float defaultLevelTime = 64f;
        [SerializeField] private float knockbackAmount = 1f;
        [SerializeField] private float knockbackTime = 0.2f;
        [SerializeField] private float knockbackFreezeTime = 0.2f;

        public SkipStage SkipStage => skipStage;
        public bool StartPistol => startPistol;
        public bool InfiniteMomentum => infiniteMomentum;
        public bool ShowDamageNumbers => showDamageNumbers;
        public float ShatterMagnitude => shatterMagnitude;
        public float InteractDistance => interactDistance;
        public float DefaultLevelTime => this.defaultLevelTime;

    }
}
