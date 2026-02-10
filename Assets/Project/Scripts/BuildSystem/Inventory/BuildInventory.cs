using System;
using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.BuildSystem
{
    [DisallowMultipleComponent]
    [MovedFrom(false, "MyShopGame.BuildSystem", null, "BuildInventory")]
    public sealed class BuildInventory : MonoBehaviour
    {
        [Serializable]
        [MovedFrom(false, "MyShopGame.BuildSystem", null, "Entry")]
        public class Entry
        {
            public BuildItemData item;
            public int count;
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        public event Action Changed;

        public IReadOnlyList<Entry> Entries => entries;

        public int GetCount(BuildItemData item)
        {
            var e = Find(item);
            return e?.count ?? 0;
        }

        public void Add(BuildItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return;

            var e = Find(item);
            if (e != null)
                e.count += amount;
            else
                entries.Add(new Entry { item = item, count = amount });

            Changed?.Invoke();
        }

        public bool TryConsume(BuildItemData item, int amount = 1)
        {
            if (item == null || amount <= 0)
                return false;

            var e = Find(item);
            if (e == null)
                return false;

            if (e.count < amount)
                return false;

            e.count -= amount;
            if (e.count <= 0)
                entries.Remove(e);

            Changed?.Invoke();
            return true;
        }

        private Entry Find(BuildItemData item)
        {
            foreach (var e in entries)
            {
                if (e != null && e.item == item)
                    return e;
            }
            return null;
        }
    }
}
