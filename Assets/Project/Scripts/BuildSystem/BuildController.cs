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
        public FloorPainter floorPainter;

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
            floorPainter ??= FindObjectOfType<FloorPainter>(true);

            var rules = new List<IPlacementRule>
            {
                new Rule_InsidePurchasedArea(territory, grid),
                new Rule_NoOverlap(grid, grid),
                new Rule_Accessibility(grid, grid),
            };

            _validator = new PlacementValidator(rules); 
        }


        private bool _isPaintingFloor;
        private Vector3Int _floorStartCell;
        private List<Vector3Int> _floorPreviewCells = new();

        private void Update()
        {
            if (mode != BuildMode.Build) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                floorPainter?.ClearPreview();
                ExitBuildMode();
                return;
            }

            if (_selected != null && _selected.placementKind == PlacementKind.Floor)
            {
                HandleFloorPaintMode();
                return;
            }

            HandleRotate();
            UpdatePreview();

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

            if (item.placementKind == PlacementKind.Floor)
                preview?.Clear();
            else
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

            if (territory != null && !territory.IsCellPurchased(cell))
            {
                preview?.SetValid(false);
                return;
            }
        }

        private void TryPlace()
        {
            if (_selected == null) return;
            if (grid == null) return;
            if (_validator == null) return;

            if (!TryGetMouseCell(out var cell, out _))
                return;

            if (_selected.placementKind == PlacementKind.Floor)
            {
                if (floorPainter != null)
                {
                    var cells = new List<Vector3Int> { cell };

                    if (floorPainter.AreCellsValid(cells) && inventory != null && inventory.GetCount(_selected) >= 1)
                    {
                        floorPainter.PaintCells(cells, _selected);
                        inventory.TryConsume(_selected, 1);

                        if (inventory.GetCount(_selected) <= 0)
                            ExitBuildMode();
                    }
                }

                return;
            }

            var req = new PlacementRequest(_selected, cell, _rotated, _facing);
            var res = _validator.CanPlace(req);
            if (!res.ok) return;

            if (territory != null && !territory.IsCellPurchased(cell))
                return;

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

            var cells = new List<Vector3Int>(grid.GetFootprintCells(req.anchorCell, req.item.footprint, req.rotated, req.item.pivotOffset));
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

        private void HandleFloorPaintMode()
        {
            if (!_isPaintingFloor)
            {
                if (TryGetMouseCell(out var hoverCell, out _))
                {
                    var cells = new List<Vector3Int> { hoverCell };

                    bool validArea = floorPainter.AreCellsValid(cells);
                    bool enoughItems = inventory.GetCount(_selected) >= 1;

                    floorPainter.ShowPreview(cells, validArea && enoughItems);
                }
            }

            if (_selected == null || floorPainter == null || inventory == null)
                return;

            if (Input.GetMouseButtonDown(1))
            {
                if (!TryGetMouseCell(out _floorStartCell, out _))
                    return;

                _isPaintingFloor = true;
            }

            if (_isPaintingFloor && Input.GetMouseButton(1))
            {
                if (!TryGetMouseCell(out var currentCell, out _))
                    return;

                _floorPreviewCells = floorPainter.GetRectCells(_floorStartCell, currentCell);

                bool validArea = floorPainter.AreCellsValid(_floorPreviewCells);
                bool enoughItems = inventory.GetCount(_selected) >= _floorPreviewCells.Count;

                floorPainter.ShowPreview(_floorPreviewCells, validArea && enoughItems);
            }

            if (_isPaintingFloor && Input.GetMouseButtonUp(1))
            {
                _isPaintingFloor = false;

                if (_floorPreviewCells == null || _floorPreviewCells.Count == 0)
                {
                    floorPainter.ClearPreview();
                    return;
                }

                bool validArea = floorPainter.AreCellsValid(_floorPreviewCells);
                bool enoughItems = inventory.GetCount(_selected) >= _floorPreviewCells.Count;

                if (validArea && enoughItems)
                {
                    floorPainter.PaintCells(_floorPreviewCells, _selected);
                    inventory.TryConsume(_selected, _floorPreviewCells.Count);

                    if (inventory.GetCount(_selected) <= 0)
                        ExitBuildMode();
                }

                floorPainter.ClearPreview();
            }
        }

        public List<PlacedBuildSaveData> BuildPlacedSaveData()
        {
            var result = new List<PlacedBuildSaveData>();

            var placedObjects = FindObjectsOfType<PlacedObject>(true);

            foreach (var placed in placedObjects)
            {
                if (placed == null || placed.item == null)
                    continue;

                result.Add(new PlacedBuildSaveData
                {
                    itemId = placed.item.id,
                    x = placed.anchorCell.x,
                    z = placed.anchorCell.z,
                    rotated = placed.rotated,
                    facing = placed.facing
                });
            }

            return result;
        }

        public void ApplyPlacedSaveData(List<PlacedBuildSaveData> data, BuildItemCatalog catalog)
        {
            ClearPlacedObjects();

            if (grid != null)
                grid.ClearAll();

            if (data == null || catalog == null)
                return;

            foreach (var d in data)
            {
                var item = catalog.GetById(d.itemId);
                if (item == null || item.prefab == null)
                    continue;

                var cell = new Vector3Int(d.x, 0, d.z);
                var req = new PlacementRequest(item, cell, d.rotated, d.facing);

                SpawnPlacedFromSave(req);
            }
        }

        private void ClearPlacedObjects()
        {
            var placedObjects = FindObjectsOfType<PlacedObject>(true);

            foreach (var placed in placedObjects)
            {
                if (placed != null)
                    Destroy(placed.gameObject);
            }
        }

        private void SpawnPlacedFromSave(PlacementRequest req)
        {
            if (req.item == null) return;
            if (req.item.prefab == null) return;
            if (grid == null) return;

            var worldPos = grid.CellToWorld(req.anchorCell);
            var rot = Quaternion.Euler(0f, req.facing * 90f, 0f);

            var go = Instantiate(req.item.prefab, worldPos, rot);
            var placed = go.GetComponent<PlacedObject>() ?? go.AddComponent<PlacedObject>();

            placed.item = req.item;
            placed.anchorCell = req.anchorCell;
            placed.rotated = req.rotated;
            placed.facing = req.facing;

            var cells = new List<Vector3Int>(
                grid.GetFootprintCells(req.anchorCell, req.item.footprint, req.rotated, req.item.pivotOffset)
            );

            placed.occupiedCells = cells;
            grid.Occupy(cells);
        }
    }


}
