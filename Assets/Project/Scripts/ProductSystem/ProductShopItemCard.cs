using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Economy;
using RetailEmpireTycoon.Products;

namespace RetailEmpireTycoon.UI.Products
{
    public sealed class ProductShopItemCard : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_Text boxAmountText;
        [SerializeField] private TMP_Text ownedAmountText;
        [SerializeField] private Button buyButton;

        private ProductItemData _item;
        private MoneyController _money;
        private ProductInventory _inventory;

        public void Bind(ProductItemData item, MoneyController money, ProductInventory inventory)
        {
            Unsubscribe();

            _item = item;
            _money = money;
            _inventory = inventory;

            Subscribe();
            HookButton();
            RefreshView();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_money != null)
                _money.Changed += HandleMoneyChanged;

            if (_inventory != null)
                _inventory.Changed += RefreshView;
        }

        private void Unsubscribe()
        {
            if (_money != null)
                _money.Changed -= HandleMoneyChanged;

            if (_inventory != null)
                _inventory.Changed -= RefreshView;
        }

        private void HookButton()
        {
            if (buyButton == null)
                return;

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(Buy);
        }

        private void HandleMoneyChanged(int value)
        {
            RefreshButtonState();
        }

        private void RefreshView()
        {
            RefreshTexts();
            RefreshIcon();
            RefreshButtonState();
        }

        private void RefreshTexts()
        {
            if (nameText != null)
                nameText.text = _item != null ? _item.DisplayName : "Unknown";

            if (priceText != null)
                priceText.text = _item != null ? "$" + _item.BuyPrice : "$0";

            if (boxAmountText != null)
                boxAmountText.text = _item != null ? "QTY: " + _item.BoxAmount : "QTY: 0";

            if (ownedAmountText != null)
                ownedAmountText.text = GetOwnedText();
        }

        private string GetOwnedText()
        {
            if (_item == null || _inventory == null)
                return "OWNED: 0";

            return "OWNED: " + _inventory.GetCount(_item);
        }

        private void RefreshIcon()
        {
            if (icon == null)
                return;

            icon.sprite = _item != null ? _item.Icon : null;
            icon.enabled = icon.sprite != null;
        }

        private void RefreshButtonState()
        {
            if (buyButton == null)
                return;

            buyButton.interactable = CanBuy();
        }

        private bool CanBuy()
        {
            return _item != null
                && _money != null
                && _inventory != null
                && _money.CanSpend(_item.BuyPrice);
        }

        private void Buy()
        {
            if (!CanBuy())
                return;

            if (!_money.TrySpend(_item.BuyPrice))
                return;

            _inventory.Add(_item, _item.BoxAmount);
            RefreshView();
        }
    }
}