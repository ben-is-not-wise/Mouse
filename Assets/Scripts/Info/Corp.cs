using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Corp", menuName = "Mouse/Info/Corp")]
    public class Corp : Item
    {
        [Tooltip("Marketing line shown under the corp's name, e.g. \"Bloom again.\"")]
        public string slogan;
        [Tooltip("Owning corp. Leave empty for a root brand such as Sakura.")]
        public Corp parent;

        public override InfoCategory InfoCategory => InfoCategory.Corp;
    }
}
