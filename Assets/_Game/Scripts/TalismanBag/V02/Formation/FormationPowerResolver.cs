using System;
using System.Collections.Generic;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.V02.Balance;
using TalismanBag.UI;
using TalismanBag.V02.Rewards;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.Formation
{
    public sealed class FormationPowerResolver : MonoBehaviour
    {
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private TalismanGridSlotView[] slotViews;
        [SerializeField] private Vector2Int formationEyePosition = new(2, 2);
        [SerializeField] private string spiritStoneItemId = "spirit_stone_basic";
        [SerializeField] private float weakCooldownMultiplier = 1.35f;
        [SerializeField] private V02FormationBalanceConfig formationBalanceConfig;
        [SerializeField] private V02RunModifierState runModifierState;

        public event Action PowerStatesChanged;

        private bool hasDiscoveredSlotViews;

        public Vector2Int FormationEyePosition => formationEyePosition;
        public V02FormationBalanceConfig FormationBalanceConfig => formationBalanceConfig;
        public float WeakCooldownMultiplier => formationBalanceConfig != null ? formationBalanceConfig.weakPowerCooldownMultiplier : weakCooldownMultiplier;
        public V02RunModifierState RunModifierState => runModifierState;
        public bool UnpoweredTalismansCanTrigger => formationBalanceConfig != null && formationBalanceConfig.unpoweredTalismansCanTrigger;
        public float DefaultStealEnergyDisableDuration => formationBalanceConfig != null ? formationBalanceConfig.stealEnergyDisableDuration : 3f;
        public float DefaultSealDuration => formationBalanceConfig != null ? formationBalanceConfig.sealDuration : 3f;

        public static Vector2Int GetDefaultEyeCorePosition(int columns, int rows)
        {
            int x = (Mathf.Max(1, columns) - 1) / 2;
            int y = (Mathf.Max(1, rows) - 1) / 2;
            return new Vector2Int(x, y);
        }

        private void OnEnable()
        {
            if (grid != null)
            {
                grid.GridChanged += RefreshPowerStates;
            }

            if (runModifierState != null)
            {
                runModifierState.ModifiersChanged += RefreshPowerStates;
            }
        }

        private void Start()
        {
            RefreshPowerStates();
        }

        private void OnDisable()
        {
            if (grid != null)
            {
                grid.GridChanged -= RefreshPowerStates;
            }

            if (runModifierState != null)
            {
                runModifierState.ModifiersChanged -= RefreshPowerStates;
            }
        }

        public void Bind(TalismanBagGrid bagGrid, TalismanGridSlotView[] slots)
        {
            if (grid != null)
            {
                grid.GridChanged -= RefreshPowerStates;
            }

            grid = bagGrid;
            slotViews = slots;
            hasDiscoveredSlotViews = false;

            if (isActiveAndEnabled && grid != null)
            {
                grid.GridChanged += RefreshPowerStates;
            }

            RefreshPowerStates();
        }

        public void BindRunModifierState(V02RunModifierState modifierState)
        {
            if (runModifierState != null)
            {
                runModifierState.ModifiersChanged -= RefreshPowerStates;
            }

            runModifierState = modifierState;

            if (isActiveAndEnabled && runModifierState != null)
            {
                runModifierState.ModifiersChanged += RefreshPowerStates;
            }

            RefreshPowerStates();
        }

        public void BindFormationBalanceConfig(V02FormationBalanceConfig config)
        {
            formationBalanceConfig = config;
            RefreshPowerStates();
        }

        public void RefreshPowerStates()
        {
            if (grid == null)
            {
                return;
            }

            EnsureSlotViewsCurrent();
            NormalizeSlotGridPositions();
            EnsureGridCoversSlotViews();
            NormalizeFormationEyePosition();

            List<TalismanItemRuntime> placedItems = grid.GetAllPlacedItems();
            List<Vector2Int> spiritStonePositions = new();
            foreach (TalismanItemRuntime item in placedItems)
            {
                if (IsSpiritStone(item))
                {
                    spiritStonePositions.Add(item.gridPosition);
                }
            }

            foreach (TalismanItemRuntime item in placedItems)
            {
                if (item == null || item.definition == null)
                {
                    continue;
                }

                item.powerState = IsSpiritStone(item)
                    ? FormationPowerState.Powered
                    : ResolveCellPowerState(item.gridPosition, spiritStonePositions);
            }

            RefreshSlotViews(spiritStonePositions);
            PowerStatesChanged?.Invoke();
        }

        public FormationPowerState GetPowerState(TalismanItemRuntime item)
        {
            if (item == null || item.definition == null)
            {
                return FormationPowerState.Unpowered;
            }

            if (IsSpiritStone(item))
            {
                return FormationPowerState.Powered;
            }

            return item.powerState;
        }

        public bool IsItemPowered(TalismanItemRuntime item)
        {
            return GetPowerState(item) != FormationPowerState.Unpowered;
        }

        public bool IsItemWeakPowered(TalismanItemRuntime item)
        {
            return GetPowerState(item) == FormationPowerState.WeakPowered;
        }

        public float GetCooldownMultiplier(TalismanItemRuntime item)
        {
            return IsItemWeakPowered(item) ? WeakCooldownMultiplier : 1f;
        }

        public int GetPoweredItemCount()
        {
            int count = 0;
            foreach (TalismanItemRuntime item in GetPlacedItems())
            {
                if (IsItemPowered(item))
                {
                    count++;
                }
            }

            return count;
        }

        public int GetPlacedTalismanCount()
        {
            return GetPlacedItems().Count;
        }

        public int GetWeakPoweredItemCount()
        {
            int count = 0;
            foreach (TalismanItemRuntime item in GetPlacedItems())
            {
                if (IsItemWeakPowered(item))
                {
                    count++;
                }
            }

            return count;
        }

        public List<TalismanItemRuntime> GetUnpoweredItems()
        {
            List<TalismanItemRuntime> result = new();
            foreach (TalismanItemRuntime item in GetPlacedItems())
            {
                if (GetPowerState(item) == FormationPowerState.Unpowered)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public List<TalismanItemRuntime> GetPoweredItems()
        {
            List<TalismanItemRuntime> result = new();
            foreach (TalismanItemRuntime item in GetPlacedItems())
            {
                if (IsItemPowered(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public bool CanAcceptItemAt(Vector2Int position)
        {
            return position != formationEyePosition;
        }

        private List<TalismanItemRuntime> GetPlacedItems()
        {
            return grid != null ? grid.GetAllPlacedItems() : new List<TalismanItemRuntime>();
        }

        private void EnsureSlotViewsCurrent()
        {
            if (hasDiscoveredSlotViews && SlotViewsCoverKnownBounds(slotViews))
            {
                return;
            }

            TalismanGridSlotView[] discovered = FindObjectsOfType<TalismanGridSlotView>(true);
            if (discovered == null || discovered.Length == 0)
            {
                hasDiscoveredSlotViews = true;
                return;
            }

            List<TalismanGridSlotView> matchingSlots = new();
            foreach (TalismanGridSlotView slotView in discovered)
            {
                if (slotView == null)
                {
                    continue;
                }

                if (slotView.Grid == null || slotView.Grid == grid)
                {
                    matchingSlots.Add(slotView);
                }
            }

            if (matchingSlots.Count > CountSlots(slotViews) || !SlotViewsCoverKnownBounds(slotViews))
            {
                slotViews = matchingSlots.ToArray();
            }

            hasDiscoveredSlotViews = true;
        }

        private bool SlotViewsCoverKnownBounds(TalismanGridSlotView[] slots)
        {
            if (slots == null || slots.Length == 0)
            {
                return false;
            }

            int maxX = -1;
            int maxY = -1;
            foreach (TalismanGridSlotView slotView in slots)
            {
                if (slotView == null)
                {
                    continue;
                }

                Vector2Int position = slotView.GridPosition;
                maxX = Mathf.Max(maxX, position.x);
                maxY = Mathf.Max(maxY, position.y);
            }

            return grid == null || maxX + 1 >= grid.Width && maxY + 1 >= grid.Height;
        }

        private static int CountSlots(TalismanGridSlotView[] slots)
        {
            if (slots == null)
            {
                return 0;
            }

            int count = 0;
            foreach (TalismanGridSlotView slotView in slots)
            {
                if (slotView != null)
                {
                    count++;
                }
            }

            return count;
        }

        private void EnsureGridCoversSlotViews()
        {
            if (grid == null || slotViews == null)
            {
                return;
            }

            int requiredWidth = 0;
            int requiredHeight = 0;
            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView == null)
                {
                    continue;
                }

                Vector2Int position = slotView.GridPosition;
                requiredWidth = Mathf.Max(requiredWidth, position.x + 1);
                requiredHeight = Mathf.Max(requiredHeight, position.y + 1);
            }

            if (requiredWidth > 0 && requiredHeight > 0)
            {
                grid.EnsureSize(requiredWidth, requiredHeight, false);
            }
        }

        private void NormalizeFormationEyePosition()
        {
            if (grid == null)
            {
                return;
            }

            Vector2Int centeredEye = GetDefaultEyeCorePosition(grid.Width, grid.Height);
            if (formationEyePosition != centeredEye)
            {
                formationEyePosition = centeredEye;
            }
        }

        private void NormalizeSlotGridPositions()
        {
            if (grid == null || slotViews == null || slotViews.Length == 0)
            {
                return;
            }

            Dictionary<Transform, List<TalismanGridSlotView>> slotsByParent = new();
            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView == null || slotView.transform.parent == null)
                {
                    continue;
                }

                Transform parent = slotView.transform.parent;
                if (!slotsByParent.TryGetValue(parent, out List<TalismanGridSlotView> group))
                {
                    group = new List<TalismanGridSlotView>();
                    slotsByParent[parent] = group;
                }

                group.Add(slotView);
            }

            Transform gridParent = null;
            List<TalismanGridSlotView> gridSlots = null;
            foreach (KeyValuePair<Transform, List<TalismanGridSlotView>> pair in slotsByParent)
            {
                if (gridSlots == null || pair.Value.Count > gridSlots.Count)
                {
                    gridParent = pair.Key;
                    gridSlots = pair.Value;
                }
            }

            if (gridParent == null || gridSlots == null || gridSlots.Count == 0)
            {
                return;
            }

            gridSlots.Sort((left, right) => left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex()));

            int columns = Mathf.Max(1, grid.Width);
            int rows = Mathf.Max(1, grid.Height);
            GridLayoutGroup layout = gridParent.GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                if (layout.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                {
                    columns = Mathf.Max(1, layout.constraintCount);
                    rows = Mathf.Max(rows, Mathf.CeilToInt(gridSlots.Count / (float)columns));
                }
                else if (layout.constraint == GridLayoutGroup.Constraint.FixedRowCount)
                {
                    rows = Mathf.Max(1, layout.constraintCount);
                    columns = Mathf.Max(columns, Mathf.CeilToInt(gridSlots.Count / (float)rows));
                }
            }

            if (columns <= 0 || rows <= 0)
            {
                return;
            }

            grid.EnsureSize(columns, rows, false);

            for (int i = 0; i < gridSlots.Count; i++)
            {
                TalismanGridSlotView slotView = gridSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                int rawColumn;
                int rawRow;
                if (layout != null && layout.startAxis == GridLayoutGroup.Axis.Vertical)
                {
                    rawColumn = i / rows;
                    rawRow = i % rows;
                }
                else
                {
                    rawColumn = i % columns;
                    rawRow = i / columns;
                }

                int visualColumn = rawColumn;
                int visualRowFromTop = rawRow;
                GridLayoutGroup.Corner startCorner = layout != null ? layout.startCorner : GridLayoutGroup.Corner.UpperLeft;
                if (startCorner == GridLayoutGroup.Corner.UpperRight || startCorner == GridLayoutGroup.Corner.LowerRight)
                {
                    visualColumn = columns - 1 - rawColumn;
                }

                if (startCorner == GridLayoutGroup.Corner.LowerLeft || startCorner == GridLayoutGroup.Corner.LowerRight)
                {
                    visualRowFromTop = rows - 1 - rawRow;
                }

                Vector2Int position = new(Mathf.Clamp(visualColumn, 0, columns - 1), Mathf.Clamp(rows - 1 - visualRowFromTop, 0, rows - 1));
                if (slotView.Grid != grid || slotView.GridPosition != position)
                {
                    slotView.Initialize(grid, position);
                }
            }
        }

        private void RefreshSlotViews(List<Vector2Int> spiritStonePositions)
        {
            if (slotViews == null)
            {
                return;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView == null)
                {
                    continue;
                }

                bool isEye = slotView.GridPosition == formationEyePosition;
                slotView.SetFormationEye(isEye);
                slotView.SetFormationPowerState(isEye
                    ? FormationPowerState.Powered
                    : ResolveCellPowerState(slotView.GridPosition, spiritStonePositions));
            }
        }

        private FormationPowerState ResolveCellPowerState(Vector2Int position, List<Vector2Int> spiritStonePositions)
        {
            if (formationBalanceConfig == null || formationBalanceConfig.spiritStoneNineGridPowerEnabled)
            {
                foreach (Vector2Int source in spiritStonePositions)
                {
                    if (Mathf.Abs(position.x - source.x) <= 1 && Mathf.Abs(position.y - source.y) <= 1)
                    {
                        return FormationPowerState.Powered;
                    }
                }
            }

            if (runModifierState != null &&
                runModifierState.spiritLinkBetweenStonesUnlocked &&
                (formationBalanceConfig == null || formationBalanceConfig.spiritLinkBetweenStonesEnabled) &&
                IsOnSpiritLink(position, spiritStonePositions))
            {
                return FormationPowerState.Powered;
            }

            int eyeDx = Mathf.Abs(position.x - formationEyePosition.x);
            int eyeDy = Mathf.Abs(position.y - formationEyePosition.y);
            if (runModifierState != null &&
                runModifierState.eyeCoreNineGridUnlocked &&
                (formationBalanceConfig == null || formationBalanceConfig.upgradedEyeNineGridEnabled) &&
                eyeDx <= 1 && eyeDy <= 1)
            {
                return FormationPowerState.Powered;
            }

            if ((formationBalanceConfig == null || formationBalanceConfig.formationEyeCrossPowerEnabled) && eyeDx + eyeDy == 1)
            {
                return FormationPowerState.Powered;
            }

            if ((formationBalanceConfig == null || formationBalanceConfig.formationEyeDiagonalWeakPowerEnabled) && eyeDx == 1 && eyeDy == 1)
            {
                return FormationPowerState.WeakPowered;
            }

            return FormationPowerState.Unpowered;
        }

        private bool IsSpiritStone(TalismanItemRuntime item)
        {
            return item?.definition != null && item.definition.itemId == spiritStoneItemId;
        }

        private static bool IsOnSpiritLink(Vector2Int position, List<Vector2Int> spiritStonePositions)
        {
            if (spiritStonePositions == null || spiritStonePositions.Count < 2)
            {
                return false;
            }

            for (int i = 0; i < spiritStonePositions.Count; i++)
            {
                for (int j = i + 1; j < spiritStonePositions.Count; j++)
                {
                    Vector2Int a = spiritStonePositions[i];
                    Vector2Int b = spiritStonePositions[j];
                    if (a.x == b.x && position.x == a.x && IsBetween(position.y, a.y, b.y))
                    {
                        return true;
                    }

                    if (a.y == b.y && position.y == a.y && IsBetween(position.x, a.x, b.x))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsBetween(int value, int a, int b)
        {
            return value >= Mathf.Min(a, b) && value <= Mathf.Max(a, b);
        }
    }
}
