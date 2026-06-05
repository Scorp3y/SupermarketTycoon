using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Economy;
using RetailEmpireTycoon.Products;

namespace RetailEmpireTycoon.UI.Products
{
    public sealed class ProductShopWindow : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ProductCatalog catalog;
        [SerializeField] private List<ProductItemData> fallbackProducts = new();

        [Header("Refs")]
        [SerializeField] private MoneyController money;
        [SerializeField] private ProductInventory inventory;

        [Header("UI")]
        [SerializeField] private Transform listRoot;
        [SerializeField] private ProductShopItemCard cardPrefab;
        [SerializeField] private GameObject emptyLabel;

        [Header("Filter")]
        [SerializeField] private ProductCategory currentFilter = ProductCategory.Any;

        private readonly List<ProductShopItemCard> _cards = new();

        private void Awake()
        {
            FindMissingRefs();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void ShowAll()
        {
            SetFilter(ProductCategory.Any);
        }

        public void ShowDrinks()
        {
            SetFilter(ProductCategory.Drinks);
        }

        public void ShowFood()
        {
            SetFilter(ProductCategory.Food);
        }

        public void ShowSnacks()
        {
            SetFilter(ProductCategory.Snacks);
        }

        public void SetFilter(ProductCategory category)
        {
            currentFilter = category;
            Refresh();
        }

        public void Refresh()
        {
            Clear();

            if (!CanBuildList())
            {
                SetEmpty(true);
                return;
            }

            int created = CreateCards();
            SetEmpty(created == 0);
        }

        private void FindMissingRefs()
        {
            if (catalog == null)
                catalog = FindObjectOfType<ProductCatalog>(true);

            if (money == null)
                money = FindObjectOfType<MoneyController>(true);

            if (inventory == null)
                inventory = FindObjectOfType<ProductInventory>(true);
        }

        private bool CanBuildList()
        {
            return listRoot != null && cardPrefab != null;
        }

        private int CreateCards()
        {
            int created = 0;
            IReadOnlyList<ProductItemData> products = GetProducts();

            foreach (var product in products)
            {
                if (!ShouldShow(product))
                    continue;

                var card = Instantiate(cardPrefab, listRoot);
                card.Bind(product, money, inventory);

                _cards.Add(card);
                created++;
            }

            return created;
        }

        private IReadOnlyList<ProductItemData> GetProducts()
        {
            if (catalog != null && catalog.Products != null && catalog.Products.Count > 0)
                return catalog.Products;

            return fallbackProducts;
        }

        private bool ShouldShow(ProductItemData product)
        {
            if (product == null)
                return false;

            return currentFilter == ProductCategory.Any || product.Category == currentFilter;
        }

        private void SetEmpty(bool visible)
        {
            if (emptyLabel != null)
                emptyLabel.SetActive(visible);
        }

        private void Clear()
        {
            foreach (var card in _cards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }

            _cards.Clear();

            if (listRoot == null)
                return;

            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
        }
    }
}