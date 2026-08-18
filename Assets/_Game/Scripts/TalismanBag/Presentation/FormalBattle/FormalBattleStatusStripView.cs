using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    public enum FormalBattleStatusStripAlignment
    {
        Left = 0,
        Right = 1
    }

    [DisallowMultipleComponent]
    public sealed class FormalBattleStatusStripView : MonoBehaviour
    {
        [SerializeField] private FormalBattleStatusSlotView[] slots =
            Array.Empty<FormalBattleStatusSlotView>();
        [SerializeField] private FormalBattleStatusStripAlignment alignment =
            FormalBattleStatusStripAlignment.Left;

        [NonSerialized] private RectTransform leadingPinnedRoot;
        [NonSerialized] private Vector2 leadingPinnedBaseline;
        [NonSerialized] private Vector2[] slotPositionBaselines =
            Array.Empty<Vector2>();

        public int AuthoredSlotCount => slots == null ? 0 : slots.Length;
        public FormalBattleStatusStripAlignment Alignment => alignment;

        public bool Bind(
            IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot> statuses,
            FormalBattlePresentationProfile profile,
            long battleTimeMs,
            out string diagnostic)
        {
            Clear();
            if (!ValidateAuthoredReferences() || profile == null)
            {
                diagnostic = "FORMAL_STATUS_STRIP_REFERENCE_INVALID";
                return false;
            }

            int statusCount = statuses == null ? 0 : statuses.Count;
            int rowCapacity = AuthoredSlotCount / 2;
            if (statusCount > AuthoredSlotCount)
            {
                diagnostic = "FORMAL_STATUS_STRIP_CAPACITY_EXCEEDED";
                return false;
            }

            for (int index = 0; index < statusCount; index++)
            {
                C1FormalRealtimeBattleStatusSnapshot status = statuses[index];
                if (status == null
                    || status.stackCount <= 0
                    || !profile.TryGetStatusVisualStyle(
                        status.statusKey,
                        status.statusFamilyKey,
                        out FormalBattleStatusVisualStyle style))
                {
                    diagnostic = "FORMAL_STATUS_VISUAL_STYLE_MISSING";
                    Clear();
                    return false;
                }

                int slotIndex = ResolveCompactSlotIndex(
                    index,
                    rowCapacity);
                string stackText = status.stackCount > 1
                    ? "×" + status.stackCount.ToString(
                        CultureInfo.InvariantCulture)
                    : string.Empty;
                string durationText = FormatDuration(
                    status.expireAtBattleTimeMs,
                    battleTimeMs);
                if (!slots[slotIndex].Show(
                    style.Icon,
                    style.ForegroundColor,
                    stackText,
                    durationText))
                {
                    diagnostic = "FORMAL_STATUS_SLOT_BIND_REJECTED";
                    Clear();
                    return false;
                }
            }

            diagnostic = "FORMAL_STATUS_STRIP_BOUND";
            return true;
        }

        public void Clear()
        {
            if (slots == null)
            {
                return;
            }

            foreach (FormalBattleStatusSlotView slot in slots)
            {
                if (slot != null)
                {
                    slot.Clear();
                }
            }
        }

        public bool ArrangeWithLeadingPinned(
            RectTransform pinnedRoot,
            bool pinnedVisible)
        {
            if (!ValidateAuthoredReferences() || pinnedRoot == null)
            {
                return false;
            }

            if (leadingPinnedRoot != pinnedRoot
                || slotPositionBaselines.Length != AuthoredSlotCount)
            {
                leadingPinnedRoot = pinnedRoot;
                leadingPinnedBaseline = pinnedRoot.anchoredPosition;
                slotPositionBaselines = slots
                    .Select(value => value.RectTransform.anchoredPosition)
                    .ToArray();
            }

            pinnedRoot.anchoredPosition = leadingPinnedBaseline;
            int rowCapacity = AuthoredSlotCount / 2;
            for (int compactIndex = 0;
                 compactIndex < slots.Length;
                 compactIndex++)
            {
                int slotIndex = ResolveCompactSlotIndex(
                    compactIndex,
                    rowCapacity);
                int anchorCompactIndex = compactIndex
                                         + (pinnedVisible ? 1 : 0);
                slots[slotIndex].RectTransform.anchoredPosition =
                    anchorCompactIndex == 0
                    ? leadingPinnedBaseline
                    : slotPositionBaselines[ResolveCompactSlotIndex(
                        anchorCompactIndex - 1,
                        rowCapacity)];
            }
            return true;
        }

        public bool ValidateAuthoredReferences()
        {
            int count = AuthoredSlotCount;
            return count >= 2
                   && count % 2 == 0
                   && slots.All(value => value != null
                       && value.ValidateAuthoredReferences())
                   && slots.Distinct().Count() == count;
        }

        private static string FormatDuration(
            long expireAtBattleTimeMs,
            long battleTimeMs)
        {
            if (expireAtBattleTimeMs <= 0L)
            {
                return string.Empty;
            }

            long remaining = Math.Max(
                0L,
                expireAtBattleTimeMs - battleTimeMs);
            long seconds = Math.Max(1L, (remaining + 999L) / 1000L);
            return seconds.ToString(CultureInfo.InvariantCulture) + "s";
        }

        private int ResolveCompactSlotIndex(
            int compactIndex,
            int rowCapacity)
        {
            int rowIndex = compactIndex / rowCapacity;
            int indexWithinRow = compactIndex % rowCapacity;
            return rowIndex * rowCapacity
                   + (alignment == FormalBattleStatusStripAlignment.Right
                       ? rowCapacity - 1 - indexWithinRow
                       : indexWithinRow);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            FormalBattleStatusSlotView[] configuredSlots)
        {
            slots = configuredSlots
                ?? Array.Empty<FormalBattleStatusSlotView>();
        }

        public void AssignAlignmentForEditor(
            FormalBattleStatusStripAlignment configuredAlignment)
        {
            alignment = configuredAlignment;
        }

        public FormalBattleStatusSlotView[] GetSlotsForEditor()
        {
            return (slots ?? Array.Empty<FormalBattleStatusSlotView>())
                .ToArray();
        }

        public int ResolveCompactSlotIndexForEditor(int compactIndex)
        {
            return ResolveCompactSlotIndex(
                compactIndex,
                AuthoredSlotCount / 2);
        }

        public void AssignForEditor(
            GameObject[] configuredSlotRoots,
            Image[] configuredSlotBackgrounds,
            Image[] configuredSlotIcons,
            TMP_Text[] configuredSlotLabels,
            TMP_Text[] configuredSlotStackLabels,
            TMP_Text[] configuredSlotDurationLabels)
        {
            int count = configuredSlotRoots == null
                ? 0
                : configuredSlotRoots.Length;
            if (count == 0
                || configuredSlotBackgrounds == null
                || configuredSlotBackgrounds.Length != count
                || configuredSlotIcons == null
                || configuredSlotIcons.Length != count
                || configuredSlotLabels == null
                || configuredSlotLabels.Length != count
                || configuredSlotStackLabels == null
                || configuredSlotStackLabels.Length != count
                || configuredSlotDurationLabels == null
                || configuredSlotDurationLabels.Length != count)
            {
                slots = Array.Empty<FormalBattleStatusSlotView>();
                return;
            }

            FormalBattleStatusSlotView[] configuredSlots =
                new FormalBattleStatusSlotView[count];
            for (int index = 0; index < count; index++)
            {
                GameObject root = configuredSlotRoots[index];
                if (root == null)
                {
                    slots = Array.Empty<FormalBattleStatusSlotView>();
                    return;
                }
                FormalBattleStatusSlotView slot =
                    root.GetComponent<FormalBattleStatusSlotView>();
                if (slot == null)
                {
                    slot = root.AddComponent<FormalBattleStatusSlotView>();
                }
                slot.AssignForEditor(
                    configuredSlotBackgrounds[index],
                    configuredSlotIcons[index],
                    configuredSlotLabels[index],
                    configuredSlotStackLabels[index],
                    configuredSlotDurationLabels[index]);
                configuredSlots[index] = slot;
            }
            slots = configuredSlots;
        }
#endif
    }
}
