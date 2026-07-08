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
        private const string CellUnderlayImageName = "CellUnderlayImage";
        private const string CellImageName = "CellImage";
        private const string UnderlayImageName = "UnderlayImage";

        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;

        private Color previewValidColor = new(0.27f, 0.54f, 0.31f, 0.95f);
        private Color previewInvalidColor = new(0.62f, 0.20f, 0.16f, 0.95f);
        private Color placedColor = new(0.44f, 0.35f, 0.18f, 1f);
        private BuildGridInteractionPreviewController controller;
        private Sprite defaultSprite;
        private Sprite defaultOverrideSprite;
        private Image.Type defaultImageType = Image.Type.Simple;
        private bool defaultPreserveAspect;
        private bool defaultFillCenter = true;
        private Material defaultMaterial;
        private float defaultPixelsPerUnitMultiplier = 1f;
        private Color defaultImageColor = Color.white;
        private bool hasDefaultImageState;
        private Image cellUnderlayImage;
        private bool placed;
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
            cellUnderlayImage = ResolveCellUnderlayImage();
            hasDefaultImageState = false;
            CacheDefaultImageState();
            ClearPlaced();
        }

        public void SetController(BuildGridInteractionPreviewController owner)
        {
            controller = owner;
        }

        public void SetEmptyColor(Color color)
        {
            if (!placed)
            {
                SetCellUnderlayVisible(false);
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
                SetCellUnderlayVisible(true);
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
            SetCellUnderlayVisible(true);
            if (labelText != null && !placed)
            {
                labelText.text = "虚";
            }
        }

        public void ClearPreview()
        {
            if (placed && placedSuppressBlock)
            {
                SetCellUnderlayVisible(true);
            }
            else if (placed && placedVisualStyle != null)
            {
                ApplyVisualStyle(placedVisualStyle, placedColor);
            }
            else
            {
                if (placed)
                {
                    SetColor(placedColor);
                }
                else
                {
                    SetCellUnderlayVisible(false);
                }
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
                SetCellUnderlayVisible(true);
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
            SetCellUnderlayVisible(false);
            if (labelText != null)
            {
                labelText.text = string.Empty;
            }
        }

        public bool TryCaptureCellUnderlayStyle(out ShapeCellVisualStyle style)
        {
            style = null;
            Image targetImage = TargetImage;
            if (targetImage == null)
            {
                return false;
            }

            CacheDefaultImageState();
            style = ShapeCellVisualStyle.FromImage(targetImage)?.WithColor(ResolveUnderlayColor(visible: true));
            return style != null;
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
            Image targetImage = TargetImage;
            if (targetImage != null)
            {
                targetImage.color = color;
            }
        }

        private void SetCellUnderlayVisible(bool visible)
        {
            RestoreDefaultImageTemplate();
            Image targetImage = TargetImage;
            if (targetImage != null)
            {
                targetImage.color = ResolveUnderlayColor(visible);
            }

            if (cellUnderlayImage != null
                && backgroundImage != null
                && backgroundImage != cellUnderlayImage)
            {
                Color parentColor = backgroundImage.color;
                parentColor.a = 0f;
                backgroundImage.color = parentColor;
            }
        }

        private Color ResolveUnderlayColor(bool visible)
        {
            Color color = defaultImageColor;
            color.a = visible ? Mathf.Max(color.a, 0.0001f) : 0f;
            return color;
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
                SetCellUnderlayVisible(true);
                return;
            }

            visualStyle.ApplyTo(TargetImage, fallbackColor);
        }

        private void CacheDefaultImageState()
        {
            Image targetImage = TargetImage;
            if (targetImage == null || hasDefaultImageState)
            {
                return;
            }

            defaultSprite = targetImage.sprite;
            defaultOverrideSprite = targetImage.overrideSprite;
            defaultImageType = targetImage.type;
            defaultPreserveAspect = targetImage.preserveAspect;
            defaultFillCenter = targetImage.fillCenter;
            defaultMaterial = targetImage.material;
            defaultPixelsPerUnitMultiplier = targetImage.pixelsPerUnitMultiplier;
            defaultImageColor = targetImage.color;
            hasDefaultImageState = true;
        }

        private void RestoreDefaultImageTemplate()
        {
            Image targetImage = TargetImage;
            if (targetImage == null)
            {
                return;
            }

            CacheDefaultImageState();
            targetImage.sprite = defaultSprite;
            targetImage.overrideSprite = defaultOverrideSprite;
            targetImage.type = defaultImageType;
            targetImage.preserveAspect = defaultPreserveAspect;
            targetImage.fillCenter = defaultFillCenter;
            targetImage.material = defaultMaterial;
            targetImage.pixelsPerUnitMultiplier = defaultPixelsPerUnitMultiplier;
        }

        private Image TargetImage
        {
            get
            {
                if (cellUnderlayImage == null)
                {
                    cellUnderlayImage = ResolveCellUnderlayImage();
                }

                return cellUnderlayImage == null ? backgroundImage : cellUnderlayImage;
            }
        }

        private Image ResolveCellUnderlayImage()
        {
            Transform direct = transform.Find(CellUnderlayImageName)
                ?? transform.Find(CellImageName)
                ?? transform.Find(UnderlayImageName);
            return direct == null ? null : direct.GetComponent<Image>();
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
