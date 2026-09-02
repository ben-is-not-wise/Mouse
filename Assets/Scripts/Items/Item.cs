using UnityEngine;

namespace HackedDesign
{
    [CreateAssetMenu(fileName = "Item", menuName = "Mouse/Items/Item")]
    public class Item : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
    }
}
