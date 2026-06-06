using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Products;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    public sealed class ProductAssignMode : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ProductInventory inventory;

        [Header("Raycast")]
        [SerializeField] private LayerMask shelfMask;
        [SerializeField, Min(1f)] private float maxDistance = 2000f;
        [SerializeField] private bool blockWhenPointerOverUI = true;

        private readonly List<PlacedShelfStock> _shelves = new List<PlacedShelfStock>();
        private ProductItemData _selectedProduct;

        public bool IsActive => _selectedProduct != null;
        public ProductItemData SelectedProduct => _selectedProduct;

        private void Awake()
        {
            FindMissingRefs();
        }

        private void Update()
        {
            if (!IsActive)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
                return;
            }

            if (!Input.GetMouseButtonDown(0))
                return;

            TryAssignByMouse();
        }

        public void BeginAssign(ProductItemData product)
        {
            if (product == null)
                return;

            _selectedProduct = product;

            CacheShelves();
            RefreshHighlights();
        }

        public void Cancel()
        {
            _selectedProduct = null;
            ClearHighlights();
        }

        private void TryAssignByMouse()
        {
            if (IsPointerBlockedByUI())
                return;

            if (!TryRaycastShelf(out var shelf))
                return;

            if (!shelf.CanAccept(_selectedProduct))
                return;

            if (!shelf.RefillFromInventory(inventory, _selectedProduct))
                return;

            Cancel();
        }

        private bool TryRaycastShelf(out PlacedShelfStock shelf)
        {
            shelf = null;

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return false;

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            int mask = shelfMask.value == 0 ? ~0 : shelfMask.value;

            if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Collide))
                return false;

            shelf = hit.collider.GetComponentInParent<PlacedShelfStock>();
            return shelf != null;
        }

        private void CacheShelves()
        {
            _shelves.Clear();
            _shelves.AddRange(FindObjectsOfType<PlacedShelfStock>(true));
        }

        private void RefreshHighlights()
        {
            foreach (var shelf in _shelves)
                SetShelfHighlight(shelf, shelf != null && shelf.CanAccept(_selectedProduct));
        }

        private void ClearHighlights()
        {
            foreach (var shelf in _shelves)
                SetShelfHighlight(shelf, false);
        }

        private static void SetShelfHighlight(PlacedShelfStock shelf, bool visible)
        {
            if (shelf == null)
                return;

            var highlight = shelf.GetComponent<ShelfHighlight>();
            if (highlight != null)
                highlight.SetVisible(visible);
        }

        private bool IsPointerBlockedByUI()
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }

        private void FindMissingRefs()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (inventory == null)
                inventory = FindObjectOfType<ProductInventory>(true);
        }
    }
}