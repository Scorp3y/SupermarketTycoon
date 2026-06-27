using System;
using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.SaveSystem;

namespace RetailEmpireTycoon.Products
{
    [DisallowMultipleComponent]
    public sealed class ProductInventory : MonoBehaviour
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private ProductItemData item;
            [SerializeField, Min(0)] private int count;

            public ProductItemData Item => item;
            public int Count => count;

            public Entry(ProductItemData item, int count)
            {
                this.item = item;
                this.count = Mathf.Max(0, count);
            }

            public void Add(int amount)
            {
                count += Mathf.Max(0, amount);
            }

            public bool TryConsume(int amount)
            {
                if (amount <= 0 || count < amount)
                    return false;

                count -= amount;
                return true;
            }
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;
        public event Action Changed;

        public int GetCount(ProductItemData item)
        {
            Entry entry = FindEntry(item);
            return entry != null ? entry.Count : 0;
        }

        public bool Has(ProductItemData item, int amount = 1)
        {
            return GetCount(item) >= amount;
        }

        public void Add(ProductItemData item, int amount)
        {
            if (item == null || amount <= 0)
                return;

            Entry entry = FindEntry(item);

            if (entry == null)
                entries.Add(new Entry(item, amount));
            else
                entry.Add(amount);

            NotifyChanged();
        }

        public bool TryConsume(ProductItemData item, int amount)
        {
            Entry entry = FindEntry(item);

            if (entry == null)
                return false;

            if (!entry.TryConsume(amount))
                return false;

            RemoveEmptyEntries();
            NotifyChanged();

            return true;
        }

        public void Clear()
        {
            entries.Clear();
            NotifyChanged();
        }

        public List<ProductInventorySaveEntry> BuildSaveData()
        {
            List<ProductInventorySaveEntry> data = new List<ProductInventorySaveEntry>();

            foreach (Entry entry in entries)
            {
                if (entry == null || entry.Item == null)
                    continue;

                if (string.IsNullOrWhiteSpace(entry.Item.Id))
                    continue;

                if (entry.Count <= 0)
                    continue;

                data.Add(new ProductInventorySaveEntry
                {
                    productId = entry.Item.Id,
                    count = entry.Count
                });
            }

            return data;
        }

        public void ApplySaveData(List<ProductInventorySaveEntry> data, ProductCatalog catalog)
        {
            entries.Clear();

            if (data == null || catalog == null)
            {
                NotifyChanged();
                return;
            }

            foreach (ProductInventorySaveEntry entryData in data)
            {
                if (entryData == null)
                    continue;

                if (entryData.count <= 0)
                    continue;

                ProductItemData product = catalog.GetById(entryData.productId);

                if (product == null)
                {
                    Debug.LogWarning("[ProductInventory] Product not found by id: " + entryData.productId, this);
                    continue;
                }

                entries.Add(new Entry(product, entryData.count));
            }

            RemoveEmptyEntries();
            NotifyChanged();
        }

        private Entry FindEntry(ProductItemData item)
        {
            if (item == null)
                return null;

            foreach (Entry entry in entries)
            {
                if (entry != null && entry.Item == item)
                    return entry;
            }

            return null;
        }

        private void RemoveEmptyEntries()
        {
            entries.RemoveAll(entry => entry == null || entry.Item == null || entry.Count <= 0);
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}