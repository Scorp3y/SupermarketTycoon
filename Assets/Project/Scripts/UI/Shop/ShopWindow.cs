using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.UI.Shop
{
    [MovedFrom(false, "MyShopGame.UI.Shop", null, "ShopWindow")]
    public sealed class ShopWindow : MonoBehaviour
    {
        [Header("Data")]
        public List<BuildItemData> catalog = new List<BuildItemData>();

        [Header("Tabs")]
        public ShopTab_Build buildTab;

        [Header("Camera Lock (optional)")]
        [SerializeField] private Behaviour[] cameraBehavioursToDisable;

        private readonly List<(Behaviour b, bool wasEnabled)> _cameraState = new();

        private void Awake()
        {
            if (cameraBehavioursToDisable == null || cameraBehavioursToDisable.Length == 0)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var mainCamController = cam.GetComponent<global::MainCamera>();
                    if (mainCamController != null)
                        cameraBehavioursToDisable = new Behaviour[] { mainCamController };
                }
            }
        }

        private void OnEnable()
        {
            LockCamera(true);
            buildTab?.Bind(catalog);
        }

        private void OnDisable()
        {
            LockCamera(false);
        }

        private void LockCamera(bool locked)
        {
            if (locked)
            {
                _cameraState.Clear();
                if (cameraBehavioursToDisable == null) return;

                for (int i = 0; i < cameraBehavioursToDisable.Length; i++)
                {
                    var b = cameraBehavioursToDisable[i];
                    if (b == null) continue;

                    if (b is Camera || b is AudioListener)
                        continue;

                    _cameraState.Add((b, b.enabled));
                    b.enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < _cameraState.Count; i++)
                {
                    var (b, wasEnabled) = _cameraState[i];
                    if (b == null) continue;
                    b.enabled = wasEnabled;
                }
                _cameraState.Clear();
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
