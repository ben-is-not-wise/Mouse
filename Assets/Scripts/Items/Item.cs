using System.Collections.Generic;
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
        public List<StatRange> randomStats = new();
        [SerializeField] private bool hideInWiki;

        public bool HideInWiki => hideInWiki;

        public virtual IEnumerable<StatRange> RandomStats => randomStats;
        public virtual InfoCategory InfoCategory => InfoCategory.Item;
    }
}
