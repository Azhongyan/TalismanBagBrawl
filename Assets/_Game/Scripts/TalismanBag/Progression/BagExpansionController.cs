using System.Collections.Generic;
using TalismanBag.Items;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.Progression
{
    public sealed class BagExpansionController : MonoBehaviour
    {
        [SerializeField] private TalismanGridSlotView[] slotViews;
        [SerializeField] private Vector2Int[] enhancedPositions = { new(1, 1), new(3, 3) };

        private readonly HashSet<Vector2Int> unlockedEnhancedSlots = new();

        public bool IsUnlocked => unlockedEnhancedSlots.Count > 0;

        public void Initialize(TalismanGridSlotView[] slots)
        {
            slotViews = slots;
            RefreshSlotViews();
        }

        public void UnlockEnhancedSlots()
        {
            foreach (Vector2Int position in enhancedPositions)
            {
                unlockedEnhancedSlots.Add(position);
            }

            RefreshSlotViews();
        }

        public void LockEnhancedSlots()
        {
            unlockedEnhancedSlots.Clear();
            RefreshSlotViews();
        }

        public bool IsEnhancedPosition(Vector2Int position)
        {
            return unlockedEnhancedSlots.Contains(position);
        }

        public float GetCooldownMultiplier(TalismanItemRuntime item)
        {
            if (item == null || item.definition == null || !item.isPlaced || !IsEnhancedPosition(item.gridPosition))
            {
                return 1f;
            }

            return item.definition.itemType == TalismanItemType.PassiveTool ? 1f : 0.85f;
        }

        private void RefreshSlotViews()
        {
            if (slotViews == null)
            {
                return;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView != null)
                {
                    slotView.SetEnhanced(IsEnhancedPosition(slotView.GridPosition));
                }
            }
        }
    }
}
