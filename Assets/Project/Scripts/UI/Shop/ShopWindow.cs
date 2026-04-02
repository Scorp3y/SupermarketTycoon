using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Economy;
using RetailEmpireTycoon.BuildSystem;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Shop
{
    [MovedFrom(false, "MyShopGame.UI.Shop", null, "ShopWindow")]
    public sealed class ShopWindow : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Fallback catalog if you don't use a runtime catalog loader. Keep this filled or your shop will be empty.")]
        public List<BuildItemData> catalog = new List<BuildItemData>();

        [Header("Scene Refs")]
        public MoneyController money;
        public BuildInventory inventory;

        [Header("UI - Main Flow")]
        public GameObject mainCategoriesPanel;   
        public GameObject mainTabsPanel;         
        public GameObject tabsCategoriesPanel;   
        public GameObject categoryViewPanel;     
        public Button backButton;
        public GameObject emptyLabel;

        [Header("UI - List")]
        public Transform listRoot;
        public ShopItemCard cardPrefab;
        public GameObject buildInventoryWindow;

        [Header("State")]
        [SerializeField] private BuildCategory _shopFilter = BuildCategory.Shelf;

        [Header("Legacy (optional)")]
        [Tooltip("Old tab-based shop UI. If set and the new panels are not assigned, this will be used.")]
        public ShopTab_Build buildTab;

        [Header("Camera Lock (optional)")]
        [SerializeField] private Behaviour[] cameraBehavioursToDisable;

        private readonly List<(Behaviour b, bool wasEnabled)> _cameraState = new();

        private void Awake()
        {
            if (cameraBehavioursToDisable == null || cameraBehavioursToDisable.Length == 0)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var mainCamController = cam.GetComponent<global::MainCamera>();
                    if (mainCamController != null)
                        cameraBehavioursToDisable = new Behaviour[] { mainCamController };
                }
            }
        }

        private void OnEnable()
        {
            LockCamera(true);

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(BackToMainTabs);
            }

            ShowMainTabs();

            if (buildInventoryWindow != null)
                buildInventoryWindow.SetActive(false);
        }

        private void OnDisable()
        {
            LockCamera(false);
        }

        private void LockCamera(bool locked)
        {
            if (locked)
            {
                _cameraState.Clear();
                if (cameraBehavioursToDisable == null) return;

                for (int i = 0; i < cameraBehavioursToDisable.Length; i++)
                {
                    var b = cameraBehavioursToDisable[i];
                    if (b == null) continue;

                    if (b is Camera || b is AudioListener)
                        continue;

                    _cameraState.Add((b, b.enabled));
                    b.enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < _cameraState.Count; i++)
                {
                    var (b, wasEnabled) = _cameraState[i];
                    if (b == null) continue;
                    b.enabled = wasEnabled;
                }

                _cameraState.Clear();
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        // =========================
        // STEP 1: Main tabs flow
        // =========================

        public void ShowMainTabs()
        {
            ClearList();

            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (mainTabsPanel != null) mainTabsPanel.SetActive(true);
            if (tabsCategoriesPanel != null) tabsCategoriesPanel.SetActive(false);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(false);
            if (emptyLabel != null) emptyLabel.SetActive(false);
        }

        public void OpenBuildCategories()
        {
            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (mainTabsPanel != null) mainTabsPanel.SetActive(false);
            if (tabsCategoriesPanel != null) tabsCategoriesPanel.SetActive(true);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(false);
            if (emptyLabel != null) emptyLabel.SetActive(false);
        }

        public void BackToMainTabs()
        {
            ShowMainTabs();
        }

        public void Back()
        {
            if (categoryViewPanel.activeSelf)
            {
                if (categoryViewPanel != null) categoryViewPanel.SetActive(false);
                if (tabsCategoriesPanel != null) tabsCategoriesPanel.SetActive(true);
            }
            else
            {
                ShowMainTabs();
            }
        }

        // =========================
        // Build categories
        // =========================

        public void OpenCategory_Shelves()
        {
            OpenCategory(BuildCategory.Shelf);
        }

        public void OpenCategory_Structures()
        {
            OpenCategory(BuildCategory.Structures);
        }


        public void OpenCategory(BuildCategory category)
        {
            _shopFilter = category;

            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (mainTabsPanel != null) mainTabsPanel.SetActive(false);
            if (tabsCategoriesPanel != null) tabsCategoriesPanel.SetActive(true);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(true);

            Refresh();
        }

        public void Refresh()
        {
            ClearList();

            if (listRoot == null || cardPrefab == null)
            {
                if (emptyLabel != null) emptyLabel.SetActive(true);
                return;
            }

            int shown = 0;

            for (int i = 0; i < catalog.Count; i++)
            {
                var it = catalog[i];
                if (it == null) continue;
                if (it.category != _shopFilter) continue;

                var card = Instantiate(cardPrefab, listRoot);
                card.Bind(it, money, inventory);
                shown++;
            }

            if (emptyLabel != null)
                emptyLabel.SetActive(shown == 0);
        }

        private void ClearList()
        {
            if (listRoot == null) return;

            for (int i = listRoot.childCount - 1; i >= 0; i--)
                Destroy(listRoot.GetChild(i).gameObject);
        }

    }
}