using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildGridPreviewSlotView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;

        private Color emptyColor = new(0.22f, 0.235f, 0.22f, 0.92f);
        private Color previewValidColor = new(0.27f, 0.54f, 0.31f, 0.95f);
        private Color previewInvalidColor = new(0.62f, 0.20f, 0.16f, 0.95f);
        private Color previewLockedColor = new(0.77f, 0.55f, 0.20f, 0.96f);
        private Color placedColor = new(0.44f, 0.35f, 0.18f, 1f);
        private const float ColorTolerance = 0.004f;
        private BuildGridInteractionPreviewController controller;
        private Sprite defaultSprite;
        private Sprite defaultOverrideSprite;
        private Image.Type defaultImageType = Image.Type.Simple;
        private bool defaultPreserveAspect;
        private bool defaultFillCenter = true;
        private Material defaultMaterial;
        private float defaultPixelsPerUnitMultiplier = 1f;
        private bool hasDefaultImageState;
        private bool placed;
        private bool manualBackgroundImageColor;
        private bool placedSuppressLabel;
        private bool placedSuppressBlock;
        private string placedName = string.Empty;
        private ShapeCellVisualStyle placedVisualStyle;

        public int X => x;
        public int Y => y;
        public ItemShapeCell Cell => new(x, y);
        public bool IsPlaced => placed;

        public void Bind(int cellX, int cellY, Image image, Text label = null)
        {
            x = cellX;
            y = cellY;
            backgroundImage = image;
            labelText = label;
            CacheDefaultImageState();
            manualBackgroundImageColor = HasManualBackgroundColor(backgroundImage);
            ClearPlaced();
        }

        public void SetController(BuildGridInteractionPreviewController owner)
        {
            controller = owner;
        }

        public void SetEmptyColor(Color color)
        {
            emptyColor = color;
            if (!placed)
            {
                SetColor(emptyColor);
            }
        }

        public void SetPreview(bool valid)
        {
            SetPreview(valid, previewValidColor);
        }

        public void SetPreview(bool valid, Color validColor)
        {
            SetPreview(valid, validColor, null);
        }

        public void SetPreview(bool valid, Color validColor, ShapeCellVisualStyle visualStyle)
        {
            SetPreview(valid, validColor, visualStyle, suppressBlock: false);
        }

        public void SetPreview(bool valid, Color validColor, ShapeCellVisualStyle visualStyle, bool suppressBlock)
        {
            if (suppressBlock)
            {
                SetColor(emptyColor);
            }
            else if (valid && visualStyle != null)
            {
                ApplyVisualStyle(visualStyle, validColor);
            }
            else
            {
                SetColor(valid ? validColor : previewInvalidColor);
            }

            if (labelText != null && !placed)
            {
                labelText.text = suppressBlock || valid ? string.Empty : "×";
            }
        }

        public void SetLockedPreview()
        {
            SetColor(previewLockedColor);
            if (labelText != null && !placed)
            {
                labelText.text = "虚";
            }
        }

        public void ClearPreview()
        {
            if (placed && placedSuppressBlock)
            {
                SetColor(emptyColor);
            }
            else if (placed && placedVisualStyle != null)
            {
                ApplyVisualStyle(placedVisualStyle, placedColor);
            }
            else
            {
                SetColor(placed ? placedColor : emptyColor);
            }

            if (labelText != null && !placed)
            {
                labelText.text = string.Empty;
            }
        }

        public void SetPlaced(string itemName)
        {
            SetPlaced(itemName, placedColor);
        }

        public void SetPlaced(string itemName, Color itemColor)
        {
            SetPlaced(itemName, itemColor, null, suppressLabel: false);
        }

        public void SetPlaced(string itemName, Color itemColor, ShapeCellVisualStyle visualStyle)
        {
            SetPlaced(itemName, itemColor, visualStyle, suppressLabel: false);
        }

        public void SetPlaced(string itemName, Color itemColor, ShapeCellVisualStyle visualStyle, bool suppressLabel)
        {
            SetPlaced(itemName, itemColor, visualStyle, suppressLabel, suppressBlock: false);
        }

        public void SetPlaced(
            string itemName,
            Color itemColor,
            ShapeCellVisualStyle visualStyle,
            bool suppressLabel,
            bool suppressBlock)
        {
            placed = true;
            placedName = itemName ?? string.Empty;
            placedColor = itemColor;
            placedSuppressLabel = suppressLabel;
            placedSuppressBlock = suppressBlock;
            placedVisualStyle = visualStyle != null && !visualStyle.SpansWholeItem ? visualStyle : null;
            if (placedSuppressBlock)
            {
                SetColor(emptyColor);
            }
            else if (placedVisualStyle != null)
            {
                ApplyVisualStyle(placedVisualStyle, placedColor);
            }
            else
            {
                SetColor(placedColor);
            }

            if (labelText != null)
            {
                labelText.text = placedSuppressLabel || (placedVisualStyle != null && placedVisualStyle.Sprite != null)
                    ? string.Empty
                    : ShortName(placedName);
            }
        }

        public void ClearPlaced()
        {
            placed = false;
            placedName = string.Empty;
            placedSuppressLabel = false;
            placedSuppressBlock = false;
            placedVisualStyle = null;
            SetColor(emptyColor);
            if (labelText != null)
            {
                labelText.text = string.Empty;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            controller?.ConfirmLockedPreviewFromCell(Cell);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            controller?.BeginBoardSlotDrag(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            controller?.UpdateBoardSlotDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            controller?.EndBoardSlotDrag(this, eventData);
        }

        private void SetColor(Color color)
        {
            RestoreDefaultImageTemplate();
            if (backgroundImage != null && !manualBackgroundImageColor)
            {
                backgroundImage.color = color;
            }
        }

        private void ApplyVisualStyle(ShapeCellVisualStyle visualStyle, Color fallbackColor)
        {
            if (visualStyle == null)
            {
                SetColor(fallbackColor);
                return;
            }

            if (visualStyle.SpansWholeItem)
            {
                SetColor(fallbackColor);
                return;
            }

            visualStyle.ApplyTo(backgroundImage, fallbackColor);
        }

        private void CacheDefaultImageState()
        {
            if (backgroundImage == null || hasDefaultImageState)
            {
                return;
            }

            defaultSprite = backgroundImage.sprite;
            defaultOverrideSprite = backgroundImage.overrideSprite;
            defaultImageType = backgroundImage.type;
            defaultPreserveAspect = backgroundImage.preserveAspect;
            defaultFillCenter = backgroundImage.fillCenter;
            defaultMaterial = backgroundImage.material;
            defaultPixelsPerUnitMultiplier = backgroundImage.pixelsPerUnitMultiplier;
            hasDefaultImageState = true;
        }

        private void RestoreDefaultImageTemplate()
        {
            if (backgroundImage == null)
            {
                return;
            }

            CacheDefaultImageState();
            backgroundImage.sprite = defaultSprite;
            backgroundImage.overrideSprite = defaultOverrideSprite;
            backgroundImage.type = defaultImageType;
            backgroundImage.preserveAspect = defaultPreserveAspect;
            backgroundImage.fillCenter = defaultFillCenter;
            backgroundImage.material = defaultMaterial;
            backgroundImage.pixelsPerUnitMultiplier = defaultPixelsPerUnitMultiplier;
        }

        private bool HasManualBackgroundColor(Image image)
        {
            return image != null
                && !IsKnownRuntimeColor(image.color);
        }

        private bool IsKnownRuntimeColor(Color color)
        {
            return Approximately(color, emptyColor)
                || Approximately(color, previewValidColor)
                || Approximately(color, previewInvalidColor)
                || Approximately(color, previewLockedColor)
                || Approximately(color, placedColor);
        }

        private static bool Approximately(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) <= ColorTolerance
                && Mathf.Abs(a.g - b.g) <= ColorTolerance
                && Mathf.Abs(a.b - b.b) <= ColorTolerance
                && Mathf.Abs(a.a - b.a) <= ColorTolerance;
        }

        private static string ShortName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Length <= 2 ? value : value.Substring(0, 2);
        }
    }
}
