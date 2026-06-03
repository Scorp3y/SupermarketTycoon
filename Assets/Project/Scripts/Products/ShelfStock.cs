using UnityEngine;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.Products
{
    [DisallowMultipleComponent]
    [MovedFrom(false, "MyShopGame.Products", null, "ShelfStock")]
    public sealed class ShelfStock : MonoBehaviour
    {
        public ProductData currentProduct;
        public int currentAmount;
        public int maxAmount = 40;

        public float Fill01 => maxAmount <= 0 ? 0f : Mathf.Clamp01((float)currentAmount / maxAmount);

        public void SetAmount(int amount)
        {
            currentAmount = Mathf.Clamp(amount, 0, maxAmount);

            var shelf = GetComponent<ShelfComponent>();
            shelf?.SetFill(Fill01);
        }
    }
}
