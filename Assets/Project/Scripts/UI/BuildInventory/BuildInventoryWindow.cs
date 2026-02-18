using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.BuildSystem;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Windows
{
    [MovedFrom(false, "MyShopGame.UI.Windows", null, "BuildInventoryWindow")]
    public sealed class BuildInventoryWindow : MonoBehaviour
    {
        [Header("Refs")]
        public BuildInventory inventory;
        public BuildController buildController;

        [Header("UI - Panels (как в магазине)")]
        public GameObject mainCategoriesPanel;
        public GameObject categoryViewPanel;    
        public GameObject emptyLabel;          

        [Header("UI - List")]
        public Transform listRoot;
        public BuildInventoryItemRow rowPrefab;

        [Header("Filter")]
        public BuildCategory currentFilter = BuildCategory.Shelf;

        private readonly List<BuildInventoryItemRow> _rows = new();

        private void OnEnable()
        {
            if (inventory != null)
                inventory.Changed += Refresh;

            ShowCategories();
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.Changed -= Refresh;
        }

        public void ShowCategories()
        {
            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(false);
            if (emptyLabel != null) emptyLabel.SetActive(false);
        }

        public void ShowShelves()
        {
            currentFilter = BuildCategory.Shelf;
            OpenCategory();
        }

        private void OpenCategory()
        {
            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(false);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(true);

            Refresh();
        }

        public void Refresh()
        {
            Clear();

            if (inventory == null || rowPrefab == null || listRoot == null)
            {
                if (emptyLabel != null) emptyLabel.SetActive(true);
                return;
            }

            int shown = 0;

            foreach (var e in inventory.Entries)
            {
                if (e == null || e.item == null || e.count <= 0)
                    continue;

                if (e.item.category != currentFilter)
                    continue;

                var row = Instantiate(rowPrefab, listRoot);

                row.Bind(
                    e.item,
                    e.count,
                    buildController,
                    onPlace: () => gameObject.SetActive(false)
                );

                _rows.Add(row);
                shown++;
            }

            if (emptyLabel != null)
                emptyLabel.SetActive(shown == 0);
        }

        private void Clear()
        {
            foreach (var r in _rows)
                if (r != null) Destroy(r.gameObject);

            _rows.Clear();
        }
    }
}
