using System;
using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Grid
{
    public sealed class TalismanBagGrid : MonoBehaviour
    {
        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;

        private TalismanItemRuntime[,] cells;
        private readonly List<TalismanItemRuntime> placedItems = new();

        public event Action GridChanged;

        public int Width => width;
        public int Height => height;

        private void Awake()
        {
            Initialize(width, height);
        }

        public void Initialize(int gridWidth, int gridHeight)
        {
            width = Mathf.Max(1, gridWidth);
            height = Mathf.Max(1, gridHeight);
            cells = new TalismanItemRuntime[width, height];
            placedItems.Clear();
            GridChanged?.Invoke();
        }

        public void NotifyChanged()
        {
            GridChanged?.Invoke();
        }

        public void EnsureSize(int gridWidth, int gridHeight, bool notify = true)
        {
            int targetWidth = Mathf.Max(width, Mathf.Max(1, gridWidth));
            int targetHeight = Mathf.Max(height, Mathf.Max(1, gridHeight));
            if (cells != null && targetWidth == width && targetHeight == height)
            {
                return;
            }

            TalismanItemRuntime[,] previousCells = cells;
            int previousWidth = width;
            int previousHeight = height;
            width = targetWidth;
            height = targetHeight;
            cells = new TalismanItemRuntime[width, height];

            if (previousCells != null)
            {
                int copyWidth = Mathf.Min(previousWidth, width);
                int copyHeight = Mathf.Min(previousHeight, height);
                for (int x = 0; x < copyWidth; x++)
                {
                    for (int y = 0; y < copyHeight; y++)
                    {
                        cells[x, y] = previousCells[x, y];
                    }
                }
            }

            List<TalismanItemRuntime> previousPlacedItems = new(placedItems);
            placedItems.Clear();
            foreach (TalismanItemRuntime item in previousPlacedItems)
            {
                if (item == null || !item.isPlaced || !IsInside(item.gridPosition))
                {
                    continue;
                }

                Vector2Int position = item.gridPosition;
                if (cells[position.x, position.y] == null)
                {
                    cells[position.x, position.y] = item;
                }

                if (!placedItems.Contains(item))
                {
                    placedItems.Add(item);
                }
            }

            if (notify)
            {
                GridChanged?.Invoke();
            }
        }

        public bool CanPlaceItem(Vector2Int position)
        {
            return IsInside(position) && cells[position.x, position.y] == null;
        }

        public bool PlaceItem(TalismanItemRuntime item, Vector2Int position)
        {
            if (item == null || item.definition == null || !CanPlaceItem(position))
            {
                return false;
            }

            RemoveItem(item, false);
            cells[position.x, position.y] = item;
            item.gridPosition = position;
            item.isPlaced = true;

            if (!placedItems.Contains(item))
            {
                placedItems.Add(item);
            }

            GridChanged?.Invoke();
            return true;
        }

        public void RemoveItem(TalismanItemRuntime item)
        {
            RemoveItem(item, true);
        }

        public TalismanItemRuntime GetItemAt(Vector2Int position)
        {
            return IsInside(position) ? cells[position.x, position.y] : null;
        }

        public List<TalismanItemRuntime> GetAdjacentItems(Vector2Int position)
        {
            List<TalismanItemRuntime> adjacent = new();
            AddIfPresent(adjacent, position + Vector2Int.up);
            AddIfPresent(adjacent, position + Vector2Int.down);
            AddIfPresent(adjacent, position + Vector2Int.left);
            AddIfPresent(adjacent, position + Vector2Int.right);
            return adjacent;
        }

        public List<TalismanItemRuntime> GetAllPlacedItems()
        {
            placedItems.RemoveAll(item => item == null || !item.isPlaced);
            return new List<TalismanItemRuntime>(placedItems);
        }

        public bool HasAdjacentItemType(Vector2Int position, TalismanItemType itemType)
        {
            foreach (TalismanItemRuntime item in GetAdjacentItems(position))
            {
                if (item.definition.itemType == itemType)
                {
                    return true;
                }
            }

            return false;
        }

        public int CountAdjacentItemType(Vector2Int position, TalismanItemType itemType)
        {
            int count = 0;
            foreach (TalismanItemRuntime item in GetAdjacentItems(position))
            {
                if (item.definition.itemType == itemType)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasAdjacentItemId(Vector2Int position, string itemId)
        {
            foreach (TalismanItemRuntime item in GetAdjacentItems(position))
            {
                if (item.definition.itemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = null;
                }
            }

            foreach (TalismanItemRuntime item in placedItems)
            {
                item.gridPosition = new Vector2Int(-1, -1);
                item.isPlaced = false;
            }

            placedItems.Clear();
            GridChanged?.Invoke();
        }

        private void RemoveItem(TalismanItemRuntime item, bool notify)
        {
            if (item == null)
            {
                return;
            }

            if (IsInside(item.gridPosition) && cells[item.gridPosition.x, item.gridPosition.y] == item)
            {
                cells[item.gridPosition.x, item.gridPosition.y] = null;
            }

            placedItems.Remove(item);
            item.gridPosition = new Vector2Int(-1, -1);
            item.isPlaced = false;

            if (notify)
            {
                GridChanged?.Invoke();
            }
        }

        private bool IsInside(Vector2Int position)
        {
            return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
        }

        private void AddIfPresent(List<TalismanItemRuntime> items, Vector2Int position)
        {
            TalismanItemRuntime item = GetItemAt(position);
            if (item != null)
            {
                items.Add(item);
            }
        }
    }
}
