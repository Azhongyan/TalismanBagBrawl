using System.Collections.Generic;
using System.Text;
using TalismanBag.Combo;
using TalismanBag.Feedback;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class ComboStatusUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text comboText;
        [SerializeField] private ComboHighlightController highlightController;

        public void Refresh(ComboResolver resolver)
        {
            if (comboText == null)
            {
                return;
            }

            if (resolver == null)
            {
                comboText.text = "当前阵法\n未形成有效阵法";
                return;
            }

            List<string> combos = resolver.GetActiveComboIds();
            if (combos.Count == 0)
            {
                comboText.text = "当前阵法\n未形成有效阵法";
                return;
            }

            StringBuilder builder = new("当前阵法");
            int shown = 0;
            foreach (string comboId in combos)
            {
                builder.Append('\n');
                builder.Append(ComboResolver.GetComboDisplayName(comboId));
                builder.Append("：");
                builder.Append(GetComboDescription(comboId));
                shown++;
                if (shown >= 3)
                {
                    break;
                }
            }

            comboText.text = builder.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightController?.SetEmphasizeActiveCombos(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightController?.SetEmphasizeActiveCombos(false);
        }

        private static string GetComboDescription(string comboId)
        {
            return comboId switch
            {
                ComboResolver.FireSpirit => "火符加速",
                ComboResolver.ShieldPill => "治疗提高",
                ComboResolver.ThunderSeal => "雷符暴击",
                ComboResolver.FireSword => "飞剑附火",
                ComboResolver.ExorcismArray => "克制鬼怪",
                ComboResolver.WaterPill => "回气更快",
                _ => string.Empty
            };
        }
    }
}
