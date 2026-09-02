using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Hack", menuName = "Mouse/PU/Generic Hack")]
    public abstract class Hack : Item
    {
        [SerializeField] public string shortName;
        [SerializeField] public Sprite buttonIcon;
        [SerializeField] public Sprite puIcon;
        [SerializeField] public float puUsage;
        [SerializeField] public float puTime;
        [SerializeField, Range(1, 3)] public int subroutineSlots = 1;
        public abstract void Trigger(GameObject target, PlayerController player);


    }
}
