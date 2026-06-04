using System;
using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;

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

        [SerializeField] private List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;
        public event Action Changed;

        public int GetCount(ProductItemData item)
        {
            return FindEntry(item)?.Count ?? 0;
        }

        public bool Has(ProductItemData item, int amount = 1)
        {
            return GetCount(item) >= amount;
        }

        public void Add(ProductItemData item, int amount)
        {
            if (item == null || amount <= 0)
                return;

            var entry = FindEntry(item);

            if (entry == null)
            {
                entries.Add(new Entry(item, amount));
                NotifyChanged();
                return;
            }

            entry.Add(amount);
            NotifyChanged();
        }

        public bool TryConsume(ProductItemData item, int amount)
        {
            var entry = FindEntry(item);

            if (entry == null || !entry.TryConsume(amount))
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

        private Entry FindEntry(ProductItemData item)
        {
            if (item == null)
                return null;

            return entries.Find(entry => entry != null && entry.Item == item);
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