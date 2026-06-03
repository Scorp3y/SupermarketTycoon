using UnityEngine;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.BuildSystem
{
    [DisallowMultipleComponent]
    [MovedFrom(false, "MyShopGame.BuildSystem", null, "BuildPreview")]
    public sealed class BuildPreview : MonoBehaviour
    {
        private GameObject _instance;
        private Renderer[] _renderers = new Renderer[0];

        private MaterialPropertyBlock _propBlock;

        [Header("Preview Colors")]
        [SerializeField]
        private Color validColor = new Color(0.3f, 1f, 0.3f, 0.35f);

        [SerializeField]
        private Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.35f);

        public void SetItem(BuildItemData item)
        {
            DestroyInstance();

            if (item == null || item.prefab == null)
                return;

            _instance = Instantiate(item.prefab, transform);
            _instance.name = "Preview_" + item.name;

            _renderers = _instance.GetComponentsInChildren<Renderer>(true);

            DisableColliders();

            SetValid(false);
        }

        public void SetPose(Vector3 worldPos, Quaternion rot)
        {
            if (_instance == null)
                return;

            transform.position = worldPos;
            transform.rotation = rot;
        }

        public void SetValid(bool valid)
        {
            if (_propBlock == null)
                _propBlock = new MaterialPropertyBlock();

            Color targetColor = valid ? validColor : invalidColor;

            foreach (var r in _renderers)
            {
                if (r == null)
                    continue;

                r.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_BaseColor", targetColor);
                _propBlock.SetColor("_Color", targetColor);
                r.SetPropertyBlock(_propBlock);
            }
        }

        public void Clear()
        {
            DestroyInstance();
        }

        private void DisableColliders()
        {
            if (_instance == null)
                return;

            var colliders = _instance.GetComponentsInChildren<Collider>(true);

            foreach (var col in colliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        private void DestroyInstance()
        {
            if (_instance == null)
                return;

            Destroy(_instance);

            _instance = null;
            _renderers = new Renderer[0];
        }
    }
}