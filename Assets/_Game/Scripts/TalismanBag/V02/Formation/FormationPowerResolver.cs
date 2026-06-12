using System;
using System.Collections.Generic;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V02.Rewards;
using UnityEngine;

namespace TalismanBag.V02.Formation
{
    public sealed class FormationPowerResolver : MonoBehaviour
    {
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private TalismanGridSlotView[] slotViews;
        [SerializeField] private Vector2Int formationEyePosition = new(2, 1);
        [SerializeField] private string spiritStoneItemId = "spirit_stone_basic";
        [SerializeField] private float weakCooldownMultiplier = 1.35f;
        [SerializeField] private V02RunModifierState runModifierState;

        public event Action PowerStatesChanged;

        public Vector2Int FormationEyePosition => formationEyePosition;
        public float WeakCooldownMultiplier => weakCooldownMultiplier;
        public V02RunModifierState RunModifierState => runModifierState;

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

        public void RefreshPowerStates()
        {
            if (grid == null)
            {
                return;
            }

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
            return IsItemWeakPowered(item) ? weakCooldownMultiplier : 1f;
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
            foreach (Vector2Int source in spiritStonePositions)
            {
                if (Mathf.Abs(position.x - source.x) <= 1 && Mathf.Abs(position.y - source.y) <= 1)
                {
                    return FormationPowerState.Powered;
                }
            }

            if (runModifierState != null &&
                runModifierState.spiritLinkBetweenStonesUnlocked &&
                IsOnSpiritLink(position, spiritStonePositions))
            {
                return FormationPowerState.Powered;
            }

            int eyeDx = Mathf.Abs(position.x - formationEyePosition.x);
            int eyeDy = Mathf.Abs(position.y - formationEyePosition.y);
            if (runModifierState != null && runModifierState.eyeCoreNineGridUnlocked && eyeDx <= 1 && eyeDy <= 1)
            {
                return FormationPowerState.Powered;
            }

            if (eyeDx + eyeDy == 1)
            {
                return FormationPowerState.Powered;
            }

            if (eyeDx == 1 && eyeDy == 1)
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
