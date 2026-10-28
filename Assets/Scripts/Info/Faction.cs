using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Faction", menuName = "Mouse/Info/Faction")]
    public class Faction : Item
    {
        public override InfoCategory InfoCategory => InfoCategory.Faction;
    }
}
