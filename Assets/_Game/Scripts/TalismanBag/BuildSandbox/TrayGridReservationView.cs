using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class TrayGridReservationView
    {
        private const string CellUnderlayImageName = "CellUnderlayImage";
        private const string CellImageName = "CellImage";
        private const string UnderlayImageName = "UnderlayImage";

        private List<RectTransform> slotRects = new();
        private List<Image> slotImages = new();
        private List<Image> slotUnderlayImages = new();
        private List<Outline> slotOutlines = new();
        private List<Color> slotImageAuthoredColors = new();
        private List<Color> slotOutlineAuthoredColors = new();

        public int SlotCount => Math.Max(slotUnderlayImages.Count, slotOutlines.Count);

        public void Bind(
            IReadOnlyList<RectTransform> traySlotRects,
            IReadOnlyList<Image> traySlotImages,
            IReadOnlyList<Outline> traySlotOutlines)
        {
            slotRects = (traySlotRects ?? Array.Empty<RectTransform>()).ToList();
            slotImages = (traySlotImages ?? Array.Empty<Image>()).ToList();
            slotOutlines = (traySlotOutlines ?? Array.Empty<Outline>()).ToList();
            slotUnderlayImages = ResolveUnderlayImages();
            CacheAuthoredVisuals();
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
            Image slotImage = slotIndex >= 0 && slotIndex < slotUnderlayImages.Count
                ? slotUnderlayImages[slotIndex]
                : null;
            Image rootImage = slotIndex >= 0 && slotIndex < slotImages.Count
                ? slotImages[slotIndex]
                : null;
            Outline slotOutline = slotIndex >= 0 && slotIndex < slotOutlines.Count
                ? slotOutlines[slotIndex]
                : null;

            if (slotImage != null)
            {
                slotImage.color = ResolveAuthoredColor(
                    slotImageAuthoredColors,
                    slotIndex,
                    slotImage.color,
                    reserved);
            }

            if (rootImage != null && rootImage != slotImage)
            {
                Color rootColor = rootImage.color;
                rootColor.a = 0f;
                rootImage.color = rootColor;
            }

            if (slotOutline != null)
            {
                slotOutline.effectColor = ResolveAuthoredColor(
                    slotOutlineAuthoredColors,
                    slotIndex,
                    slotOutline.effectColor,
                    reserved);
            }
        }

        public bool TryCaptureSlotUnderlayStyle(int slotIndex, out ShapeCellVisualStyle style)
        {
            style = null;
            if (slotIndex < 0 || slotIndex >= slotUnderlayImages.Count)
            {
                return false;
            }

            Image image = slotUnderlayImages[slotIndex];
            if (image == null)
            {
                return false;
            }

            Image rootImage = slotIndex >= 0 && slotIndex < slotImages.Count
                ? slotImages[slotIndex]
                : null;
            style = ShapeCellVisualStyle.FromImage(
                    image,
                    captureRectTransform: rootImage != null && image != rootImage)
                ?.WithColor(ResolveAuthoredColor(
                    slotImageAuthoredColors,
                    slotIndex,
                    image.color,
                    reserved: true));
            return style != null;
        }

        private void CacheAuthoredVisuals()
        {
            slotImageAuthoredColors = slotUnderlayImages
                .Select(image => image == null ? Color.white : image.color)
                .ToList();
            slotOutlineAuthoredColors = slotOutlines
                .Select(outline => outline == null ? Color.white : outline.effectColor)
                .ToList();
        }

        private List<Image> ResolveUnderlayImages()
        {
            int count = Math.Max(slotRects.Count, slotImages.Count);
            List<Image> images = new();
            for (int i = 0; i < count; i++)
            {
                RectTransform slotRect = i >= 0 && i < slotRects.Count ? slotRects[i] : null;
                Image fallback = i >= 0 && i < slotImages.Count ? slotImages[i] : null;
                images.Add(ResolveUnderlayImage(slotRect) ?? fallback);
            }

            return images;
        }

        private static Image ResolveUnderlayImage(RectTransform slotRect)
        {
            if (slotRect == null)
            {
                return null;
            }

            Transform direct = slotRect.Find(CellUnderlayImageName)
                ?? slotRect.Find(CellImageName)
                ?? slotRect.Find(UnderlayImageName);
            return direct == null ? null : direct.GetComponent<Image>();
        }

        private static Color ResolveAuthoredColor(
            IReadOnlyList<Color> authoredColors,
            int index,
            Color fallbackColor,
            bool reserved)
        {
            Color color = authoredColors != null
                && index >= 0
                && index < authoredColors.Count
                ? authoredColors[index]
                : fallbackColor;
            color.a = reserved ? Mathf.Max(color.a, 0.0001f) : 0f;
            return color;
        }
    }
}
