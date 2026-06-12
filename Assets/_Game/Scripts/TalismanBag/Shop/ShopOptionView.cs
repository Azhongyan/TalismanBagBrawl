using System;
using TalismanBag.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Shop
{
    public sealed class ShopOptionView : MonoBehaviour
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text buyText;

        private TalismanItemDefinition definition;
        private Action<TalismanItemDefinition> onSelected;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(Select);
            }
        }

        public void Bind(TalismanItemDefinition itemDefinition, Action<TalismanItemDefinition> selectCallback)
        {
            Bind(itemDefinition, 0, true, itemDefinition == null, selectCallback);
        }

        public void Bind(TalismanItemDefinition itemDefinition, int price, bool canAfford, bool soldOut, Action<TalismanItemDefinition> selectCallback)
        {
            definition = itemDefinition;
            onSelected = selectCallback;

            if (background != null)
            {
                background.color = definition != null && !soldOut ? definition.uiColor : Color.gray;
            }

            if (icon != null)
            {
                icon.sprite = definition != null ? definition.icon : null;
                icon.enabled = icon.sprite != null;
            }

            if (nameText != null)
            {
                nameText.text = soldOut ? "已售出" : definition != null ? $"{definition.displayName} Lv1" : "-";
            }

            if (descriptionText != null)
            {
                descriptionText.text = soldOut ? string.Empty : definition != null ? definition.description : string.Empty;
            }

            if (priceText != null)
            {
                priceText.text = soldOut || definition == null ? string.Empty : $"价格 {price}";
            }

            if (buyText != null)
            {
                buyText.text = soldOut ? "已售出" : canAfford ? "购买" : "灵玉不足";
            }

            if (selectButton != null)
            {
                selectButton.interactable = definition != null && canAfford && !soldOut;
            }
        }

        private void Select()
        {
            if (definition != null)
            {
                onSelected?.Invoke(definition);
            }
        }
    }
}
