using TalismanBag.Enemies;
using TalismanBag.Grid;
using TalismanBag.Items;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class TutorialHintController : MonoBehaviour
    {
        [SerializeField] private Text hintText;
        [SerializeField] private TalismanBagGrid grid;

        private const string SpiritStoneId = "spirit_stone_basic";
        private const string FireTalismanId = "fire_talisman_basic";
        private bool fireSpiritHintShown;

        private void Awake()
        {
            if (grid != null)
            {
                grid.GridChanged += OnGridChanged;
            }
        }

        private void OnDestroy()
        {
            if (grid != null)
            {
                grid.GridChanged -= OnGridChanged;
            }
        }

        public void ShowRoundHint(int roundIndex, EnemyDefinition enemy, string fallbackHint)
        {
            if (hintText == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(fallbackHint))
            {
                hintText.text = fallbackHint;
                return;
            }

            hintText.text = roundIndex switch
            {
                1 => "把火符放在聚灵石旁边，火符会释放得更快。",
                2 => "鬼怪害怕雷法、桃木和驱邪类法器。",
                3 => "剑修蓄力连斩时，可以用雷符打断。",
                4 => "邪修会封印阵位，不要把所有核心道具挤在一起。",
                7 => "Boss 会吸灵、封印和蓄力，稳定灵气、防御和雷符打断都很重要。",
                _ => enemy != null ? $"观察 {enemy.GetReadableLabel()} 的弱点后再调整阵盘。" : "观察敌人弱点后再调整阵盘。"
            };
        }

        private void OnGridChanged()
        {
            if (fireSpiritHintShown || grid == null || hintText == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null || item.definition.itemId != FireTalismanId)
                {
                    continue;
                }

                if (grid.HasAdjacentItemId(item.gridPosition, SpiritStoneId))
                {
                    fireSpiritHintShown = true;
                    hintText.text = "火灵连发已激活：火符触发速度提高。";
                    return;
                }
            }
        }
    }
}
