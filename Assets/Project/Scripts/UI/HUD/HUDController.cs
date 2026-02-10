using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.HUD
{
    [MovedFrom(false, "MyShopGame.UI.HUD", null, "HUDController")]
    public sealed class HUDController : MonoBehaviour
    {
        public GameObject shopWindow;
        public GameObject buildInventoryWindow;

        public void ToggleShop()
        {
            if (shopWindow == null)
                return;

            shopWindow.SetActive(!shopWindow.activeSelf);
        }

        public void ToggleBuildInventory()
        {
            if (buildInventoryWindow == null)
                return;

            buildInventoryWindow.SetActive(!buildInventoryWindow.activeSelf);
        }
    }
}
