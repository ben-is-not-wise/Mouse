
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

        [SerializeField] private Text slogan;
        [SerializeField] private Text description;

        private InfoCategory selectedCategory = InfoCategory.Person;
        private int selectedItem = 0;

        private WikiButton people;
        private WikiButton corps;
        private WikiButton factions;
        private WikiButton weapons;
        private WikiButton components;

        private WikiButton hacks;
        private readonly List<WikiButton> items = new List<WikiButton>();
        private readonly List<Item> itemData = new List<Item>();

        void Awake()
        {
            this.people = Instantiate(prefab, category.transform);
            this.corps = Instantiate(prefab, category.transform);
            this.factions = Instantiate(prefab, category.transform);
            this.weapons = Instantiate(prefab, category.transform);
            this.components = Instantiate(prefab, category.transform);
            this.hacks = Instantiate(prefab, category.transform);

            this.people.onClick.AddListener(() => SelectCategory(InfoCategory.Person));
            this.corps.onClick.AddListener(() => SelectCategory(InfoCategory.Corp));
            this.factions.onClick.AddListener(() => SelectCategory(InfoCategory.Faction));
            this.weapons.onClick.AddListener(() => SelectCategory(InfoCategory.Weapon));
            this.components.onClick.AddListener(() => SelectCategory(InfoCategory.Item));
            this.hacks.onClick.AddListener(() => SelectCategory(InfoCategory.Hack));
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
            this.corps.Set("Corps", 0, InfoRepository.Instance.Corps.Count);
            this.factions.Set("Factions", 0, InfoRepository.Instance.Factions.Count);
            this.weapons.Set("Weapons", 0, 12);
            this.components.Set("Components", 0, 12);
            this.hacks.Set("Hacks", 0, InfoRepository.Instance.Hacks.Count);
            UpdateCategorySelection();
        }

        private void UpdateCategorySelection()
        {
            this.people.Active = this.selectedCategory == InfoCategory.Person;
            this.corps.Active = this.selectedCategory == InfoCategory.Corp;
            this.factions.Active = this.selectedCategory == InfoCategory.Faction;
            this.weapons.Active = this.selectedCategory == InfoCategory.Weapon;
            this.components.Active = this.selectedCategory == InfoCategory.Item;
            this.hacks.Active = this.selectedCategory == InfoCategory.Hack;
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

            switch(item)
            {
                case Person person:
                    this.avatar.gameObject.SetActive(true);
                    this.itemSprite.gameObject.SetActive(false);
                    this.avatar.Set(person.gender, person.face, person.hair);
                    this.name.text = GetDisplayName(item);
                    this.description.text = item.description;
                    this.slogan.text = person.corp;
                    break;
                case Corp corp:
                    this.avatar.gameObject.SetActive(false);
                    this.itemSprite.gameObject.SetActive(true);
                    this.itemSprite.sprite = corp.icon;
                    this.name.text = GetDisplayName(corp);
                    this.description.text = corp.description;
                    this.slogan.text = corp.slogan;
                    break;
            }
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

