#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "UtilityBrainSettings", menuName = "Mouse/Settings/Utility Brain")]
    public class UtilityBrainSettings : ScriptableObject
    {
        [SerializeField] private List<UtilityActionType> actions = new() { UtilityActionType.Attack, UtilityActionType.Investigate, UtilityActionType.Patrol };
        [SerializeField] private float hysteresis = 0.1f;

        public UtilityBrain Build()
        {
            var built = new IUtilityAction[actions.Count];
            for (int i = 0; i < actions.Count; i++)
            {
                built[i] = CreateAction(actions[i]);
            }
            return new UtilityBrain(built, hysteresis);
        }

        private static IUtilityAction CreateAction(UtilityActionType type) => type switch
        {
            UtilityActionType.Patrol => new PatrolAction(),
            UtilityActionType.Sleep => new SleepAction(),
            UtilityActionType.Investigate => new InvestigateAction(),
            UtilityActionType.Attack => new AttackAction(),
            UtilityActionType.Advance => new AdvanceAction(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
