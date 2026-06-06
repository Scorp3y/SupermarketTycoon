using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Products;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    public sealed class PlacedShelfStock : MonoBehaviour
    {
        [Header("Allowed Products")]
        [SerializeField] private List<ProductItemData> allowedProducts = new List<ProductItemData>();

        [Header("Stock")]
        [SerializeField] private ProductItemData currentProduct;
        [SerializeField, Min(0)] private int currentAmount;
        [SerializeField, Min(1)] private int maxAmount = 24;

        [Header("Visual")]
        [SerializeField] private GameObject emptyIcon;

        public ProductItemData CurrentProduct => currentProduct;
        public int CurrentAmount => currentAmount;
        public int MaxAmount => maxAmount;
        public IReadOnlyList<ProductItemData> AllowedProducts => allowedProducts;

        private void OnEnable()
        {
            RefreshVisual();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            currentAmount = Mathf.Clamp(currentAmount, 0, maxAmount);
            maxAmount = Mathf.Max(1, maxAmount);
        }
#endif

        public bool CanAccept(ProductItemData product)
        {
            if (product == null)
                return false;

            if (!IsProductAllowed(product))
                return false;

            if (IsFull)
                return false;

            return IsEmpty || currentProduct == product;
        }

        public bool RefillFromInventory(ProductInventory inventory, ProductItemData product)
        {
            if (inventory == null || !CanAccept(product))
                return false;

            int available = inventory.GetCount(product);
            int amountToTake = Mathf.Min(available, FreeSpace);

            if (amountToTake <= 0)
                return false;

            if (!inventory.TryConsume(product, amountToTake))
                return false;

            currentProduct = product;
            currentAmount += amountToTake;

            RefreshVisual();
            return true;
        }

        public bool IsProductAllowed(ProductItemData product)
        {
            return product != null && allowedProducts.Contains(product);
        }

        public void RefreshVisual()
        {
            if (emptyIcon != null)
                emptyIcon.SetActive(IsEmpty);
        }

        public string GetProductName()
        {
            return currentProduct != null ? currentProduct.DisplayName : "None";
        }

        private bool IsEmpty => currentAmount <= 0;
        private bool IsFull => currentAmount >= maxAmount;
        private int FreeSpace => Mathf.Max(0, maxAmount - currentAmount);
    }
}