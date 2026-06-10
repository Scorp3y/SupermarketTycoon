using UnityEngine;
using UnityEngine.EventSystems;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    public sealed class ShelfInfoRaycaster : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ShelfInfoWindow infoWindow;
        [SerializeField] private ProductAssignMode assignMode;

        [Header("Raycast")]
        [SerializeField] private LayerMask shelfMask;
        [SerializeField, Min(1f)] private float maxDistance = 2000f;
        [SerializeField] private bool blockWhenPointerOverUI = true;

        private void Awake()
        {
            FindMissingRefs();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideWindow();
                return;
            }

            if (!Input.GetMouseButtonDown(1))
                return;

            if (assignMode != null && assignMode.IsActive)
                return;

            TryShowShelfInfo();
        }

        private void TryShowShelfInfo()
        {
            if (IsPointerBlockedByUI())
                return;

            if (!TryFindShelfUnderMouse(out var shelf))
            {
                HideWindow();
                return;
            }

            infoWindow.Show(shelf, Input.mousePosition);
        }

        private bool TryFindShelfUnderMouse(out PlacedShelfStock shelf)
        {
            shelf = null;

            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return false;

            Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            int mask = shelfMask.value == 0 ? ~0 : shelfMask.value;

            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, mask, QueryTriggerInteraction.Collide);

            if (hits == null || hits.Length == 0)
                return false;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                shelf = hit.collider.GetComponentInParent<PlacedShelfStock>();

                if (shelf != null)
                    return true;
            }

            return false;
        }

        private bool IsPointerBlockedByUI()
        {
            return blockWhenPointerOverUI
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject();
        }

        private void HideWindow()
        {
            if (infoWindow != null)
                infoWindow.Hide();
        }

        private void FindMissingRefs()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (infoWindow == null)
                infoWindow = FindObjectOfType<ShelfInfoWindow>(true);

            if (assignMode == null)
                assignMode = FindObjectOfType<ProductAssignMode>(true);
        }
    }
}