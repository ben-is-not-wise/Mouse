
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HackedDesign.UI;
using UnityEngine.UI;

namespace HackedDesign
{
    public class WikiPresenter : AbstractPresenter
    {
        [SerializeField] private WikiButton prefab;

        [SerializeField] private LayoutGroup category;
        [SerializeField] private LayoutGroup Item;
        [SerializeField] private RectTransform Info;
        [SerializeField] private AvatarPresenter avatar;
        [SerializeField] private Image itemSprite;
        [SerializeField] private new Text name;
        [SerializeField] private Text description;

        private InfoCategory selectedCategory = InfoCategory.Person;
        private int selectedItem = 0;

        private WikiButton people;
        private WikiButton corps;
        private WikiButton factions;
        private WikiButton weapons;
        private WikiButton components;
        private readonly List<WikiButton> items = new List<WikiButton>();
        private readonly List<Item> itemData = new List<Item>();

        void Awake()
        {
            this.people = Instantiate(prefab, category.transform);
            this.corps = Instantiate(prefab, category.transform);
            this.factions = Instantiate(prefab, category.transform);
            this.weapons = Instantiate(prefab, category.transform);
            this.components = Instantiate(prefab, category.transform);

            this.people.onClick.AddListener(() => SelectCategory(InfoCategory.Person));
            this.corps.onClick.AddListener(() => SelectCategory(InfoCategory.Corp));
            this.factions.onClick.AddListener(() => SelectCategory(InfoCategory.Faction));
            this.weapons.onClick.AddListener(() => SelectCategory(InfoCategory.Weapon));
            this.components.onClick.AddListener(() => SelectCategory(InfoCategory.Item));
        }

        public void OnEnable()
        {
            Repaint();
        }

        public override void Repaint()
        {
            UpdateCategories();
            UpdateItems();
        }

        private void UpdateCategories()
        {
            this.people.Set("People", 0, InfoRepository.Instance.People.Count);
            this.corps.Set("Corps/Factions", 0, InfoRepository.Instance.Corps.Count);
            //this.factions.Set("Factions", 0, InfoRepository.Instance.Factions.Count);
            this.weapons.Set("Weapons", 0, 12);
            this.components.Set("Components", 0, 12);
            UpdateCategorySelection();
        }

        private void UpdateCategorySelection()
        {
            this.people.Active = this.selectedCategory == InfoCategory.Person;
            this.corps.Active = this.selectedCategory == InfoCategory.Corp;
            this.factions.Active = this.selectedCategory == InfoCategory.Faction;
            this.weapons.Active = this.selectedCategory == InfoCategory.Weapon;
            this.components.Active = this.selectedCategory == InfoCategory.Item;
        }

        private void SelectCategory(InfoCategory category)
        {
            this.selectedCategory = category;
            this.selectedItem = 0;
            UpdateCategorySelection();
            UpdateItems();
        }

        private void SelectItem(int index)
        {
            this.selectedItem = index;
            UpdateItemSelection();
        }

        private void UpdateItemSelection()
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                this.items[i].Active = i == this.selectedItem;
            }

            UpdateInfo();
        }

        private void UpdateInfo()
        {
            var item = this.selectedItem >= 0 && this.selectedItem < this.itemData.Count ? this.itemData[this.selectedItem] : null;

            this.Info.gameObject.SetActive(item != null);

            if (item == null)
            {
                return;
            }

            var person = item as Person;

            this.avatar.gameObject.SetActive(person != null);
            this.itemSprite.gameObject.SetActive(person == null);

            if (person != null)
            {
                this.avatar.Set(person.gender, person.face, person.hair);
            }
            else
            {
                this.itemSprite.sprite = item.icon;
            }

            this.name.text = GetDisplayName(item);
            this.description.text = item.description;
        }

        private static string GetDisplayName(Item item) => string.IsNullOrWhiteSpace(item.displayName) ? item.name : item.displayName;

        private void UpdateItems()
        {
            foreach (Transform child in Item.transform)
            {
                Destroy(child.gameObject);
            }

            this.items.Clear();
            this.itemData.Clear();

            foreach (var item in GetItems(this.selectedCategory).Where(item => !item.HideInWiki))
            {
                var button = Instantiate(prefab, Item.transform);
                var index = this.items.Count;
                button.Set(GetDisplayName(item));
                button.onClick.AddListener(() => SelectItem(index));
                this.items.Add(button);
                this.itemData.Add(item);
            }

            UpdateItemSelection();
        }

        private IEnumerable<Item> GetItems(InfoCategory category)
        {
            return category switch
            {
                InfoCategory.Person => InfoRepository.Instance.People,
                InfoCategory.Corp => InfoRepository.Instance.Corps,
                InfoCategory.Faction => InfoRepository.Instance.Factions,
                InfoCategory.Weapon => InfoRepository.Instance.weapons,
                _ => Enumerable.Empty<Item>()
            };
        }
    }
}
