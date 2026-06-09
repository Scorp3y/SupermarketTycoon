using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Economy;
using RetailEmpireTycoon.BuildSystem;
using RetailEmpireTycoon.Products;
using RetailEmpireTycoon.UI.Products;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Shop
{
    [MovedFrom(false, "MyShopGame.UI.Shop", null, "ShopWindow")]
    public sealed class ShopWindow : MonoBehaviour
    {
        private enum ViewMode
        {
            MainTabs,
            BuildCategories,
            BuildItems,
            Products
        }

        [Header("Build Data")]
        [SerializeField] private List<BuildItemData> buildCatalog = new();

        [Header("Product Data")]
        [SerializeField] private List<ProductItemData> productCatalog = new();

        [Header("Scene Refs")]
        [SerializeField] private MoneyController money;
        [SerializeField] private BuildInventory buildInventory;
        [SerializeField] private ProductInventory productInventory;

        [Header("UI - Main Flow")]
        [SerializeField] private GameObject mainCategoriesPanel;
        [SerializeField] private GameObject mainTabsPanel;
        [SerializeField] private GameObject tabsCategoriesPanel;
        [SerializeField] private GameObject categoryViewPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject emptyLabel;

        [Header("UI - List")]
        [SerializeField] private Transform listRoot;
        [SerializeField] private ShopItemCard buildCardPrefab;
        [SerializeField] private ProductShopItemCard productCardPrefab;
        [SerializeField] private GameObject buildInventoryWindow;

        [Header("State")]
        [SerializeField] private BuildCategory buildFilter = BuildCategory.Shelf;

        [Header("Camera Lock")]
        [SerializeField] private Behaviour[] cameraBehavioursToDisable;

        private readonly List<(Behaviour behaviour, bool wasEnabled)> _cameraState = new();
        private ViewMode _viewMode = ViewMode.MainTabs;

        private void Awake()
        {
            FindMissingRefs();
            PrepareCameraLock();
        }

        private void OnEnable()
        {
            FindMissingRefs();
            LockCamera(true);
            HookBackButton();
            ShowMainTabs();

            if (buildInventoryWindow != null)
                buildInventoryWindow.SetActive(false);
        }

        private void OnDisable()
        {
            LockCamera(false);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void ShowMainTabs()
        {
            _viewMode = ViewMode.MainTabs;

            ClearList();
            SetPanel(mainCategoriesPanel, true);
            SetPanel(mainTabsPanel, true);
            SetPanel(tabsCategoriesPanel, false);
            SetPanel(categoryViewPanel, false);
            SetEmpty(false);
        }

        public void OpenBuildCategories()
        {
            _viewMode = ViewMode.BuildCategories;

            ClearList();
            SetPanel(mainCategoriesPanel, true);
            SetPanel(mainTabsPanel, false);
            SetPanel(tabsCategoriesPanel, true);
            SetPanel(categoryViewPanel, false);
            SetEmpty(false);
        }

        public void OpenProducts()
        {
            _viewMode = ViewMode.Products;

            SetPanel(mainCategoriesPanel, true);
            SetPanel(mainTabsPanel, false);
            SetPanel(tabsCategoriesPanel, false);
            SetPanel(categoryViewPanel, true);

            RefreshProducts();
        }

        public void BackToMainTabs()
        {
            ShowMainTabs();
        }

        public void Back()
        {
            ShowMainTabs();
        }

        public void OpenCategory_Shelves()
        {
            OpenCategory(BuildCategory.Shelf);
        }

        public void OpenCategory_Structures()
        {
            OpenCategory(BuildCategory.Structures);
        }

        public void OpenCategory_Decoration()
        {
            OpenCategory(BuildCategory.Decoration);
        }

        public void OpenCategory(BuildCategory category)
        {
            _viewMode = ViewMode.BuildItems;
            buildFilter = category;

            SetPanel(mainCategoriesPanel, true);
            SetPanel(mainTabsPanel, false);
            SetPanel(tabsCategoriesPanel, true);
            SetPanel(categoryViewPanel, true);

            RefreshBuildItems();
        }

        public void Refresh()
        {
            switch (_viewMode)
            {
                case ViewMode.Products:
                    RefreshProducts();
                    break;

                case ViewMode.BuildItems:
                    RefreshBuildItems();
                    break;

                case ViewMode.BuildCategories:
                    OpenBuildCategories();
                    break;

                default:
                    ShowMainTabs();
                    break;
            }
        }

        private void RefreshBuildItems()
        {
            ClearList();

            if (listRoot == null || buildCardPrefab == null)
            {
                SetEmpty(true);
                Debug.LogWarning("[ShopWindow] Build list root or build card prefab is missing.");
                return;
            }

            int shown = 0;

            foreach (var item in buildCatalog)
            {
                if (item == null || item.category != buildFilter)
                    continue;

                var card = Instantiate(buildCardPrefab, listRoot);
                card.Bind(item, money, buildInventory);
                shown++;
            }

            SetEmpty(shown == 0);
        }

        private void RefreshProducts()
        {
            ClearList();

            if (listRoot == null || productCardPrefab == null)
            {
                SetEmpty(true);
                Debug.LogWarning("[ShopWindow] Product list root or product card prefab is missing.");
                return;
            }

            if (productCatalog.Count == 0)
                Debug.LogWarning("[ShopWindow] Product catalog is empty. Add ProductItemData assets to Product Data.");

            int shown = 0;

            foreach (var product in productCatalog)
            {
                if (product == null)
                    continue;

                var card = Instantiate(productCardPrefab, listRoot);
                card.Bind(product, money, productInventory);
                shown++;
            }

            SetEmpty(shown == 0);
        }

        private void ClearList()
        {
            if (listRoot == null)
                return;

            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
        }

        private void FindMissingRefs()
        {
            if (money == null)
                money = FindObjectOfType<MoneyController>(true);

            if (buildInventory == null)
                buildInventory = FindObjectOfType<BuildInventory>(true);

            if (productInventory == null)
                productInventory = FindObjectOfType<ProductInventory>(true);
        }

        private void PrepareCameraLock()
        {
            if (cameraBehavioursToDisable != null && cameraBehavioursToDisable.Length > 0)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            var mainCamera = cam.GetComponent<global::MainCamera>();
            if (mainCamera == null)
                return;

            cameraBehavioursToDisable = new Behaviour[] { mainCamera };
        }

        private void HookBackButton()
        {
            if (backButton == null)
                return;

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToMainTabs);
        }

        private void LockCamera(bool locked)
        {
            if (locked)
            {
                SaveAndDisableCameraBehaviours();
                return;
            }

            RestoreCameraBehaviours();
        }

        private void SaveAndDisableCameraBehaviours()
        {
            _cameraState.Clear();

            if (cameraBehavioursToDisable == null)
                return;

            foreach (var behaviour in cameraBehavioursToDisable)
            {
                if (behaviour == null)
                    continue;

                if (behaviour is Camera || behaviour is AudioListener)
                    continue;

                _cameraState.Add((behaviour, behaviour.enabled));
                behaviour.enabled = false;
            }
        }

        private void RestoreCameraBehaviours()
        {
            foreach (var state in _cameraState)
            {
                if (state.behaviour == null)
                    continue;

                state.behaviour.enabled = state.wasEnabled;
            }

            _cameraState.Clear();
        }

        private static void SetPanel(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }

        private void SetEmpty(bool visible)
        {
            if (emptyLabel != null)
                emptyLabel.SetActive(visible);
        }
    }
}