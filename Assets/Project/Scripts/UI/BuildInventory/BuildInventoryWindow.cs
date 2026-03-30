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
        public GameObject shopWindow;

        [Header("UI")]
        public GameObject mainCategoriesPanel;
        public GameObject categoryViewPanel;
        public GameObject emptyLabel;

        [Header("UI - List")]
        public Transform listRoot;
        public BuildInventoryItemRow rowPrefab;

        [Header("Filter")]
        public BuildCategory currentFilter = BuildCategory.Shelf;

        [Header("Camera Lock (optional)")]
        [SerializeField] private Behaviour[] cameraBehavioursToDisable;

        private readonly List<BuildInventoryItemRow> _rows = new();
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
            if (shopWindow != null)
                shopWindow.SetActive(false);

            if (inventory != null)
                inventory.Changed += Refresh;

            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (categoryViewPanel != null) categoryViewPanel.SetActive(true);

            LockCamera(true);
            Refresh();
        }

        private void OnDisable()
        {
            if (inventory != null)
                inventory.Changed -= Refresh;

            LockCamera(false);
        }

        public void ShowShelves()
        {
            currentFilter = BuildCategory.Shelf;
            Refresh();
        }

       /* public void ShowProducts()
        {
            currentFilter = BuildCategory.Product;
            Refresh();
        }*/

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
            {
                if (r != null)
                    Destroy(r.gameObject);
            }

            _rows.Clear();
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
    }
}