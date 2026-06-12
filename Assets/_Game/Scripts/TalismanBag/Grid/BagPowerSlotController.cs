using System.Collections.Generic;
using TalismanBag.Items;
using TalismanBag.Progression;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.Grid
{
    public sealed class BagPowerSlotController : MonoBehaviour
    {
        [SerializeField] private BagExpansionController expansionController;
        public List<Vector2Int> powerSlotPositions = new() { new Vector2Int(1, 1), new Vector2Int(3, 3) };
        public bool powerSlotsUnlocked;
        public float cooldownMultiplier = 0.85f;

        public void Bind(BagExpansionController controller)
        {
            expansionController = controller;
        }

        public void Initialize(TalismanGridSlotView[] slotViews)
        {
            expansionController?.Initialize(slotViews);
            RefreshState();
        }

        public void UnlockPowerSlots()
        {
            expansionController?.UnlockEnhancedSlots();
            RefreshState();
        }

        public void LockPowerSlots()
        {
            expansionController?.LockEnhancedSlots();
            RefreshState();
        }

        public bool IsPowerSlot(Vector2Int position)
        {
            return expansionController != null && expansionController.IsEnhancedPosition(position);
        }

        public float GetCooldownMultiplier(TalismanItemRuntime item)
        {
            if (expansionController == null)
            {
                return 1f;
            }

            return expansionController.GetCooldownMultiplier(item);
        }

        private void RefreshState()
        {
            powerSlotsUnlocked = expansionController != null && expansionController.IsUnlocked;
        }
    }
}
