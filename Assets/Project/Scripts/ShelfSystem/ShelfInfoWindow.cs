using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RetailEmpireTycoon.Core;

namespace RetailEmpireTycoon.Shelves
{
    [DisallowMultipleComponent]
    public sealed class ShelfInfoWindow : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform panel;

        [SerializeField] private Image productIcon;
        [SerializeField] private TMP_Text productText;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Button closeButton;

        [Header("Empty State")]
        [SerializeField] private Sprite emptyIcon;
        [SerializeField] private string emptyProductText = "EMPTY";

        [Header("Position")]
        [SerializeField] private Vector2 screenOffset = new Vector2(18f, -18f);

        private Camera _uiCamera;

        private void Awake()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (panel == null)
                panel = transform as RectTransform;

            HookCloseButton();
            Hide();
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);
        }

        public void Show(PlacedShelfStock shelf, Vector2 screenPosition)
        {
            if (shelf == null)
                return;

            RefreshView(shelf);
            SetPosition(screenPosition);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HookCloseButton()
        {
            if (closeButton == null)
                return;

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        private void RefreshView(PlacedShelfStock shelf)
        {
            ProductItemData product = shelf.CurrentProduct;

            RefreshIcon(product);
            RefreshTexts(shelf, product);
        }

        private void RefreshIcon(ProductItemData product)
        {
            if (productIcon == null)
                return;

            Sprite sprite = product != null ? product.Icon : emptyIcon;

            productIcon.sprite = sprite;
            productIcon.enabled = sprite != null;
        }

        private void RefreshTexts(PlacedShelfStock shelf, ProductItemData product)
        {
            if (productText != null)
                productText.text = product != null ? product.DisplayName : emptyProductText;

            if (amountText != null)
                amountText.text = shelf.CurrentAmount + "/" + shelf.MaxAmount;
        }

        private void SetPosition(Vector2 screenPosition)
        {
            if (panel == null || canvas == null)
                return;

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
                return;

            _uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition + screenOffset,
                _uiCamera,
                out Vector2 localPoint
            );

            panel.anchoredPosition = localPoint;
        }
    }
}