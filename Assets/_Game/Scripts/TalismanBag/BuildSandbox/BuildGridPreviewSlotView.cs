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
        private BuildGridInteractionPreviewController controller;
        private bool placed;
        private string placedName = string.Empty;

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
            SetColor(valid ? previewValidColor : previewInvalidColor);
            if (labelText != null && !placed)
            {
                labelText.text = valid ? string.Empty : "×";
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
            SetColor(placed ? placedColor : emptyColor);
            if (labelText != null && !placed)
            {
                labelText.text = string.Empty;
            }
        }

        public void SetPlaced(string itemName)
        {
            placed = true;
            placedName = itemName ?? string.Empty;
            SetColor(placedColor);
            if (labelText != null)
            {
                labelText.text = ShortName(placedName);
            }
        }

        public void ClearPlaced()
        {
            placed = false;
            placedName = string.Empty;
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
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
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
