using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemSandboxV04BoardCellView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [SerializeField] private Vector2Int cell;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;

        private ItemSandboxV04BoardFullDetailAdapter adapter;

        public Vector2Int Cell => cell;
        public RectTransform RectTransform => transform as RectTransform;
        public Image BackgroundImage => backgroundImage;
        public Text LabelText => labelText;

#if UNITY_EDITOR
        public void ConfigureEditor(Vector2Int configuredCell, Image configuredBackground, Text configuredLabel)
        {
            cell = configuredCell;
            backgroundImage = configuredBackground;
            labelText = configuredLabel;
        }
#endif

        public void Bind(ItemSandboxV04BoardFullDetailAdapter owner)
        {
            adapter = owner;
        }

        public void Show(string text, Color background, Color foreground)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = background;
                backgroundImage.raycastTarget = true;
            }

            if (labelText != null)
            {
                labelText.text = text ?? string.Empty;
                labelText.color = foreground;
                labelText.raycastTarget = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            adapter?.SelectBoardCell(cell);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            adapter?.BeginBoardDrag(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            adapter?.UpdateBoardDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            adapter?.EndBoardDrag(this, eventData);
        }
    }
}
