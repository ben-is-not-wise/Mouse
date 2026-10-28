using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Corp", menuName = "Mouse/Info/Corp")]
    public class Corp : Item
    {
        public override InfoCategory InfoCategory => InfoCategory.Corp;
    }
}
