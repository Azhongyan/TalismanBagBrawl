using System;
using TalismanBag.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileShopOptionView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text buyText;
        [SerializeField] private Button buyButton;

        private int optionIndex;
        private Action<int> onBuy;

        private void Awake()
        {
            buyButton?.onClick.AddListener(() => onBuy?.Invoke(optionIndex));
        }

        public void Bind(int index, ShopOptionRuntime option, bool canAfford, Action<int> buyCallback)
        {
            optionIndex = index;
            onBuy = buyCallback;

            bool available = option != null && option.item != null && !option.soldOut;
            if (background != null)
            {
                background.color = available ? option.item.uiColor : Color.gray;
            }

            if (icon != null)
            {
                icon.sprite = available ? option.item.icon : null;
                icon.enabled = icon.sprite != null;
            }

            if (nameText != null)
            {
                nameText.text = available ? option.item.displayName : "已售出";
            }

            if (priceText != null)
            {
                priceText.text = available ? $"{option.price} 灵玉" : string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = available ? option.item.description : string.Empty;
            }

            if (buyText != null)
            {
                buyText.text = !available ? "已售出" : canAfford ? "购买" : "灵玉不足";
            }

            if (buyButton != null)
            {
                buyButton.interactable = available && canAfford;
            }
        }
    }
}
