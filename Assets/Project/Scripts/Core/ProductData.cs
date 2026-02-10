using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.Core
{
    [CreateAssetMenu(menuName = "Retail Empire Tycoon/Product", fileName = "Product_")]
    [MovedFrom(false, "MyShopGame.Core", null, "ProductData")]
    public class ProductData : ScriptableObject
    {
        public string productId;
        public string displayName;
        public Sprite icon;
        public ProductCategory category = ProductCategory.Any;

        [Header("Economy")]
        public int buyCost;
        public int sellPrice;

        [Header("World")]
        public GameObject worldPrefab;
        public ScriptableObject visualToken;
    }
}
