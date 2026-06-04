using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;

namespace RetailEmpireTycoon.Products
{
    [DisallowMultipleComponent]
    public sealed class ProductCatalog : MonoBehaviour
    {
        [SerializeField] private List<ProductItemData> products = new();

        private readonly Dictionary<string, ProductItemData> _byId = new();

        public IReadOnlyList<ProductItemData> Products => products;

        private void Awake()
        {
            RebuildCache();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            products.RemoveAll(item => item == null);
        }
#endif

        public ProductItemData GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _byId.TryGetValue(id, out var product) ? product : null;
        }

        public bool Contains(ProductItemData product)
        {
            return product != null && products.Contains(product);
        }

        private void RebuildCache()
        {
            _byId.Clear();

            foreach (var product in products)
            {
                if (product == null)
                    continue;

                if (string.IsNullOrWhiteSpace(product.Id))
                {
                    Debug.LogWarning($"[ProductCatalog] Product '{product.name}' has empty Id.");
                    continue;
                }

                if (_byId.ContainsKey(product.Id))
                {
                    Debug.LogWarning($"[ProductCatalog] Duplicate product Id: {product.Id}");
                    continue;
                }

                _byId.Add(product.Id, product);
            }
        }
    }
}