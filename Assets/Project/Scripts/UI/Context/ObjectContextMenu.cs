using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Context
{
    [MovedFrom(false, "MyShopGame.UI.Context", null, "ObjectContextMenu")]
    public sealed class ObjectContextMenu : MonoBehaviour
    {
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
