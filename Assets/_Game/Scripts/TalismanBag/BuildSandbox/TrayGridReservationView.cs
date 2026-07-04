using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class TrayGridReservationView
    {
        private static readonly Color IdleTraySlotColor = new(0.17f, 0.18f, 0.15f, 1f);
        private static readonly Color ReservedTraySlotColor = new(0.29f, 0.23f, 0.14f, 1f);
        private static readonly Color IdleTraySlotOutlineColor = new(0.42f, 0.36f, 0.18f, 0.75f);
        private static readonly Color ReservedTraySlotOutlineColor = new(0.74f, 0.55f, 0.22f, 0.95f);
        private const float ColorTolerance = 0.004f;

        private List<Image> slotImages = new();
        private List<Outline> slotOutlines = new();
        private List<bool> manualSlotImageColors = new();
        private List<bool> manualSlotOutlineColors = new();

        public int SlotCount => Math.Max(slotImages.Count, slotOutlines.Count);

        public void Bind(
            IReadOnlyList<Image> traySlotImages,
            IReadOnlyList<Outline> traySlotOutlines)
        {
            slotImages = (traySlotImages ?? Array.Empty<Image>()).ToList();
            slotOutlines = (traySlotOutlines ?? Array.Empty<Outline>()).ToList();
            CacheManualVisualOverrides();
        }

        public void Refresh(
            IReadOnlyList<TrayPlacementViewModel> placements,
            ISet<string> visibleItemIds)
        {
            HashSet<int> reservedSlots = new();
            foreach (TrayPlacementViewModel placement in placements ?? Array.Empty<TrayPlacementViewModel>())
            {
                if (placement == null
                    || !placement.isValid
                    || string.IsNullOrWhiteSpace(placement.itemId)
                    || (visibleItemIds != null && !visibleItemIds.Contains(placement.itemId)))
                {
                    continue;
                }

                foreach (int slotIndex in placement.occupiedSlotIndexes ?? Array.Empty<int>())
                {
                    if (slotIndex >= 0)
                    {
                        reservedSlots.Add(slotIndex);
                    }
                }
            }

            int slotCount = SlotCount;
            for (int i = 0; i < slotCount; i++)
            {
                SetSlotReserved(i, reservedSlots.Contains(i));
            }
        }

        private void SetSlotReserved(int slotIndex, bool reserved)
        {
            Image slotImage = slotIndex >= 0 && slotIndex < slotImages.Count
                ? slotImages[slotIndex]
                : null;
            Outline slotOutline = slotIndex >= 0 && slotIndex < slotOutlines.Count
                ? slotOutlines[slotIndex]
                : null;

            if (slotImage != null)
            {
                if (!IsManualOverride(manualSlotImageColors, slotIndex))
                {
                    slotImage.color = reserved ? ReservedTraySlotColor : IdleTraySlotColor;
                }
            }

            if (slotOutline != null)
            {
                if (!IsManualOverride(manualSlotOutlineColors, slotIndex))
                {
                    slotOutline.effectColor = reserved ? ReservedTraySlotOutlineColor : IdleTraySlotOutlineColor;
                }
            }
        }

        private void CacheManualVisualOverrides()
        {
            manualSlotImageColors = slotImages
                .Select(image => image != null && !IsKnownTraySlotColor(image.color))
                .ToList();
            manualSlotOutlineColors = slotOutlines
                .Select(outline => outline != null && !IsKnownTraySlotOutlineColor(outline.effectColor))
                .ToList();
        }

        private static bool IsManualOverride(IReadOnlyList<bool> overrides, int index)
        {
            return overrides != null
                && index >= 0
                && index < overrides.Count
                && overrides[index];
        }

        private static bool IsKnownTraySlotColor(Color color)
        {
            return Approximately(color, IdleTraySlotColor)
                || Approximately(color, ReservedTraySlotColor);
        }

        private static bool IsKnownTraySlotOutlineColor(Color color)
        {
            return Approximately(color, IdleTraySlotOutlineColor)
                || Approximately(color, ReservedTraySlotOutlineColor);
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) <= ColorTolerance
                && Mathf.Abs(a.g - b.g) <= ColorTolerance
                && Mathf.Abs(a.b - b.b) <= ColorTolerance
                && Mathf.Abs(a.a - b.a) <= ColorTolerance;
        }
    }
}
