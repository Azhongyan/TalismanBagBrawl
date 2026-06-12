using System.Collections.Generic;
using TalismanBag.Combo;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.Feedback
{
    public sealed class ComboHighlightController : MonoBehaviour
    {
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private ComboResolver comboResolver;
        [SerializeField] private TalismanGridSlotView[] slotViews;

        private bool forceAllHighlights;
        private bool emphasizeActiveCombos;

        private void Awake()
        {
            if (comboResolver != null)
            {
                comboResolver.CombosChanged += RefreshHighlights;
            }
        }

        private void OnDestroy()
        {
            if (comboResolver != null)
            {
                comboResolver.CombosChanged -= RefreshHighlights;
            }
        }

        public void SetEmphasizeActiveCombos(bool value)
        {
            emphasizeActiveCombos = value;
            RefreshHighlights();
        }

        public void ToggleComboHighlight()
        {
            forceAllHighlights = !forceAllHighlights;
            RefreshHighlights();
        }

        public void RefreshHighlights()
        {
            if (grid == null || slotViews == null)
            {
                return;
            }

            HashSet<string> combos = new(comboResolver != null ? comboResolver.GetActiveComboIds() : new List<string>());
            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView == null)
                {
                    continue;
                }

                TalismanItemRuntime item = grid.GetItemAt(slotView.GridPosition);
                bool highlighted = forceAllHighlights || item != null && IsItemInActiveCombo(item, combos);
                slotView.SetComboHighlight(highlighted);
                if (emphasizeActiveCombos && highlighted)
                {
                    slotView.Flash();
                }
            }
        }

        private bool IsItemInActiveCombo(TalismanItemRuntime item, HashSet<string> combos)
        {
            string itemId = item.definition.itemId;
            Vector2Int position = item.gridPosition;
            return combos.Contains(ComboResolver.FireSpirit) && IsPair(itemId, position, "fire_talisman_basic", "spirit_stone_basic") ||
                   combos.Contains(ComboResolver.ShieldPill) && IsPair(itemId, position, "qi_pill_basic", "shield_talisman_basic") ||
                   combos.Contains(ComboResolver.ThunderSeal) && IsPair(itemId, position, "thunder_talisman_basic", "seal_basic") ||
                   combos.Contains(ComboResolver.FireSword) && IsPair(itemId, position, "sword_pill_basic", "fire_talisman_basic") ||
                   combos.Contains(ComboResolver.ExorcismArray) && IsPair(itemId, position, "peach_wood_basic", "exorcism_bell_basic") ||
                   combos.Contains(ComboResolver.WaterPill) && IsPair(itemId, position, "water_talisman_basic", "qi_pill_basic");
        }

        private bool IsPair(string itemId, Vector2Int position, string a, string b)
        {
            return itemId == a && grid.HasAdjacentItemId(position, b) ||
                   itemId == b && grid.HasAdjacentItemId(position, a);
        }
    }
}
