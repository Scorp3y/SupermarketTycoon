using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Territory;
using UnityEngine.Scripting.APIUpdating;

namespace RetailEmpireTycoon.BuildSystem
{
    [DisallowMultipleComponent]
    [MovedFrom(false, "MyShopGame.BuildSystem", null, "BuildController")]
    public sealed class BuildController : MonoBehaviour
    {
        [Header("Refs")]
        public Camera worldCamera;
        public GridSystem grid;
        public BuildInventory inventory;
        public TerritoryManager territory;
        public BuildPreview preview;
        public event System.Action<BuildItemData> OnPlacedSuccessfully;
        public BuildGridOverlay gridOverlay;

        [Header("State")]
        public BuildMode mode = BuildMode.Normal;

        [Header("Inventory")]
        [Tooltip("If true, placement will consume 1 item from BuildInventory after a successful placement.\n" +
                 "Per current design, this should be OFF until you hook consumption to a confirmed successful placement flow.")]
        [SerializeField] private bool consumeFromInventoryOnPlace = false;

        private BuildItemData _selected;
        private bool _rotated;
        private int _facing;

        private PlacementValidator _validator;

        private void Awake()
        {
            worldCamera ??= Camera.main; 

            grid ??= GetComponent<GridSystem>();
            inventory ??= GetComponent<BuildInventory>();
            territory ??= GetComponent<TerritoryManager>();
            preview ??= GetComponentInChildren<BuildPreview>(true);
            gridOverlay ??= FindObjectOfType<BuildGridOverlay>(true);

            var rules = new List<IPlacementRule>
            {
                new Rule_InsidePurchasedArea(territory, grid),
                new Rule_NoOverlap(grid, grid),
                new Rule_Accessibility(grid, grid),
            };

            _validator = new PlacementValidator(rules); 
        }

        private void Update()
        {
            if (mode != BuildMode.Build) return;

            HandleRotate();
            UpdatePreview();

            if (Input.GetKeyDown(KeyCode.Escape))
                ExitBuildMode();

            if (Input.GetMouseButtonDown(1))
                TryPlace();
        }

        public void EnterBuildMode(BuildItemData item)
        {
            if (item == null) return;

            mode = BuildMode.Build;
            _selected = item;
            _rotated = false;
            _facing = 0;

            preview?.SetItem(item);
            gridOverlay?.Show();
        }

        public void ExitBuildMode()
        {
            mode = BuildMode.Normal;
            _selected = null;
            preview?.Clear();
            gridOverlay?.Hide();
        }

        private void HandleRotate()
        {
            if (_selected == null) return;
            if (!_selected.allowRotation) return;

            var rot = Input.GetKeyDown(KeyCode.Q) ? -1 : Input.GetKeyDown(KeyCode.E) ? 1 : 0;
            if (rot == 0) return;

            _facing = (_facing + rot) % 4;
            if (_facing < 0) _facing += 4;

            _rotated = _facing % 2 != 0;
        }

        private void UpdatePreview()
        {
            if (_selected == null) return;
            if (grid == null) return;
            if (_validator == null) return;

            if (!TryGetMouseCell(out var cell, out _))
                return;

            var worldPos = grid.CellToWorld(cell);
            var rot = Quaternion.Euler(0f, _facing * 90f, 0f);

            var req = new PlacementRequest(_selected, cell, _rotated, _facing);
            var res = _validator.CanPlace(req);

            preview?.SetPose(worldPos, rot);
            preview?.SetValid(res.ok);
        }

        private void TryPlace()
        {
            if (_selected == null) return;
            if (grid == null) return;
            if (_validator == null) return;

            if (!TryGetMouseCell(out var cell, out _))
                return;

            var req = new PlacementRequest(_selected, cell, _rotated, _facing);
            var res = _validator.CanPlace(req);
            if (!res.ok) return;

            SpawnPlaced(req);
            OnPlacedSuccessfully?.Invoke(req.item);

        }

        private void SpawnPlaced(PlacementRequest req)
        {
            if (req.item == null) return;
            if (req.item.prefab == null) return;

            var worldPos = grid.CellToWorld(req.anchorCell);
            var rot = Quaternion.Euler(0f, req.facing * 90f, 0f);

            var go = Instantiate(req.item.prefab, worldPos, rot);
            var placed = go.GetComponent<PlacedObject>() ?? go.AddComponent<PlacedObject>();

            placed.item = req.item;
            placed.anchorCell = req.anchorCell;
            placed.rotated = req.rotated;
            placed.facing = req.facing;

            var cells = new List<Vector3Int>(grid.GetFootprintCells(req.anchorCell, req.item.footprint, req.rotated));
            placed.occupiedCells = cells;

            grid.Occupy(cells);

            inventory?.TryConsume(req.item, 1);

            if (inventory == null || inventory.GetCount(req.item) <= 0)
            {
                ExitBuildMode();
            }
        }

        private bool TryGetMouseCell(out Vector3Int cell, out Vector3 hitPos)
        {
            cell = default;
            hitPos = default;

            if (worldCamera == null) return false;
            if (grid == null) return false;

            var ray = worldCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 2000f))
                return false;

            hitPos = hit.point;
            cell = grid.WorldToCell(hit.point);
            return true;
        }
    }
}
