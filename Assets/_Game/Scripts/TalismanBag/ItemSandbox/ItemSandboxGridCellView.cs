using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    public sealed class ItemSandboxGridCellView : MonoBehaviour
    {
        [SerializeField] private Vector2Int cell;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;

        private Action<Vector2Int> onSelected;

        public Vector2Int Cell => cell;
        public bool HasButton => button != null;
        public bool HasLabel => labelText != null;

        public void ConfigureEditor(
            Vector2Int configuredCell,
            Button configuredButton,
            Image configuredBackgroundImage,
            Text configuredLabelText)
        {
            cell = configuredCell;
            button = configuredButton;
            backgroundImage = configuredBackgroundImage;
            labelText = configuredLabelText;
        }

        public void Bind(Action<Vector2Int> selectedHandler)
        {
            onSelected = selectedHandler;
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        public void Show(string label, Color backgroundColor, Color textColor)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (labelText != null)
            {
                labelText.text = label;
                labelText.color = textColor;
            }
        }

        private void HandleClick()
        {
            onSelected?.Invoke(cell);
        }
    }
}
