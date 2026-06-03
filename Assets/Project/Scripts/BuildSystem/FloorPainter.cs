using System.Collections.Generic;
using UnityEngine;
using RetailEmpireTycoon.Core;
using RetailEmpireTycoon.Territory;

namespace RetailEmpireTycoon.BuildSystem
{
    public sealed class FloorPainter : MonoBehaviour
    {
        public GridSystem grid;
        public TerritoryManager territory;

        [Header("Floor Tile Visual")]
        public GameObject floorTilePrefab;
        public Transform floorRoot;

        [Header("Preview")]
        public Transform previewRoot;
        public Material previewValidMaterial;
        public Material previewInvalidMaterial;

        private readonly Dictionary<Vector3Int, GameObject> _tiles = new();
        private readonly List<GameObject> _previewTiles = new();

        public void ClearPreview()
        {
            foreach (var t in _previewTiles)
            {
                if (t != null)
                    Destroy(t);
            }

            _previewTiles.Clear();
        }

        public List<Vector3Int> GetRectCells(Vector3Int a, Vector3Int b)
        {
            var result = new List<Vector3Int>();

            int minX = Mathf.Min(a.x, b.x);
            int maxX = Mathf.Max(a.x, b.x);
            int minZ = Mathf.Min(a.z, b.z);
            int maxZ = Mathf.Max(a.z, b.z);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    result.Add(new Vector3Int(x, 0, z));
                }
            }

            return result;
        }

        public bool AreCellsValid(List<Vector3Int> cells)
        {
            if (cells == null || cells.Count == 0)
                return false;

            foreach (var cell in cells)
            {
                if (territory != null && !territory.IsCellPurchased(cell))
                    return false;
            }

            return true;
        }

        public void ShowPreview(List<Vector3Int> cells, bool valid)
        {
            ClearPreview();

            if (grid == null || floorTilePrefab == null || cells == null)
                return;

            Material mat = valid ? previewValidMaterial : previewInvalidMaterial;

            foreach (var cell in cells)
            {
                Vector3 pos = grid.CellToWorld(cell);
                pos.y += 0.025f;

                var tile = Instantiate(floorTilePrefab, pos, Quaternion.identity, previewRoot);
                tile.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);

                var r = tile.GetComponentInChildren<Renderer>();
                if (r != null && mat != null)
                    r.sharedMaterial = mat;

                _previewTiles.Add(tile);
            }
        }

        public void PaintCells(List<Vector3Int> cells, BuildItemData item)
        {
            if (grid == null || item == null || item.floorMaterial == null || floorTilePrefab == null)
                return;

            foreach (var cell in cells)
            {
                if (territory != null && !territory.IsCellPurchased(cell))
                    continue;

                if (_tiles.TryGetValue(cell, out var oldTile) && oldTile != null)
                    Destroy(oldTile);

                Vector3 pos = grid.CellToWorld(cell);
                pos.y += 0.01f;

                var tile = Instantiate(floorTilePrefab, pos, Quaternion.identity, floorRoot);
                tile.transform.localScale = new Vector3(grid.cellSize, 0.01f, grid.cellSize);

                var r = tile.GetComponentInChildren<Renderer>();
                if (r != null)
                    r.sharedMaterial = item.floorMaterial;

                var marker = tile.GetComponent<PlacedFloorTile>() ?? tile.AddComponent<PlacedFloorTile>();
                marker.item = item;
                marker.cell = cell;

                _tiles[cell] = tile;
            }
        }

        public List<FloorTileSaveData> BuildSaveData()
        {
            var result = new List<FloorTileSaveData>();

            foreach (var pair in _tiles)
            {
                var cell = pair.Key;
                var tile = pair.Value;

                if (tile == null)
                    continue;

                var marker = tile.GetComponent<PlacedFloorTile>();
                if (marker == null || marker.item == null)
                    continue;

                result.Add(new FloorTileSaveData
                {
                    itemId = marker.item.id,
                    x = cell.x,
                    z = cell.z
                });
            }

            return result;
        }

        public void ApplySaveData(List<FloorTileSaveData> data, BuildItemCatalog catalog)
        {
            ClearPlacedFloors();

            if (data == null || catalog == null)
                return;

            foreach (var d in data)
            {
                var item = catalog.GetById(d.itemId);
                if (item == null)
                    continue;

                var cell = new Vector3Int(d.x, 0, d.z);
                PaintCells(new List<Vector3Int> { cell }, item);
            }
        }

        private void ClearPlacedFloors()
        {
            foreach (var pair in _tiles)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
            }

            _tiles.Clear();

            if (floorRoot != null)
            {
                for (int i = floorRoot.childCount - 1; i >= 0; i--)
                    Destroy(floorRoot.GetChild(i).gameObject);
            }
        }

    }


}