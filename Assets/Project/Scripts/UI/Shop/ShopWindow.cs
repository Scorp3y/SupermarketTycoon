using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Shop
{
    [MovedFrom(false, "MyShopGame.UI.Shop", null, "ShopWindow")]
    public sealed class ShopWindow : MonoBehaviour
    {
        [Header("Data")]
        public List<BuildItemData> catalog = new List<BuildItemData>();

        [Header("Tabs")]
        public ShopTab_Build buildTab;

        private void OnEnable()
        {
            buildTab?.Bind(catalog);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
