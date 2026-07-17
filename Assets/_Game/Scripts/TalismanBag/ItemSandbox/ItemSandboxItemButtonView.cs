using System;
using TalismanBag.Items.Detail;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    public sealed class ItemSandboxItemButtonView : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private Text nameText;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new(0.72f, 0.55f, 0.34f, 0.34f);
        [SerializeField] private Color selectedColor = new(0.98f, 0.79f, 0.38f, 0.82f);

        private Action<string> onSelected;

        public string ItemId => itemId;
        public string DisplayName => nameText != null ? nameText.text : string.Empty;

        public void ConfigureEditor(
            string configuredItemId,
            Text configuredNameText,
            Button configuredButton,
            Image configuredBackgroundImage)
        {
            itemId = configuredItemId;
            nameText = configuredNameText;
            button = configuredButton;
            backgroundImage = configuredBackgroundImage;
        }

        public void Bind(ItemDetailListEntry entry, Action<string> selectedHandler)
        {
            if (entry == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            itemId = entry.itemId;
            onSelected = selectedHandler;

            if (nameText != null)
            {
                nameText.text = entry.displayItemName;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }
        }

        private void HandleClick()
        {
            onSelected?.Invoke(itemId);
        }
    }
}
