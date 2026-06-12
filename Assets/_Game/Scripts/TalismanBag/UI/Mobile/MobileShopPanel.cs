using System.Collections.Generic;
using TalismanBag.Economy;
using TalismanBag.Enemies;
using TalismanBag.Run;
using TalismanBag.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileShopPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text jadeText;
        [SerializeField] private Text nextEnemyText;
        [SerializeField] private Text statusText;
        [SerializeField] private MobileShopOptionView[] optionViews;

        public void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void RefreshHeader(SpiritJadeWallet wallet, RoundConfig nextRound, string status)
        {
            if (jadeText != null)
            {
                jadeText.text = $"灵玉：{(wallet != null ? wallet.CurrentJade : 0)}";
            }

            EnemyDefinition enemy = nextRound != null ? nextRound.enemy : null;
            if (nextEnemyText != null)
            {
                nextEnemyText.text = enemy == null
                    ? "下一场：通关"
                    : $"下一场：{nextRound.roundTitle}\n弱点：{enemy.weaknessText}\n危险：{enemy.dangerText}\n推荐：{enemy.recommendedBuildText}";
            }

            if (statusText != null)
            {
                statusText.text = status;
            }
        }

        public void RefreshOptions(IReadOnlyList<ShopOptionRuntime> options, SpiritJadeWallet wallet, System.Action<int> buyCallback)
        {
            if (optionViews == null)
            {
                return;
            }

            for (int i = 0; i < optionViews.Length; i++)
            {
                ShopOptionRuntime option = options != null && i < options.Count ? options[i] : null;
                bool canAfford = wallet == null || option == null || wallet.CanSpend(option.price);
                optionViews[i]?.Bind(i, option, canAfford, buyCallback);
            }
        }
    }
}
