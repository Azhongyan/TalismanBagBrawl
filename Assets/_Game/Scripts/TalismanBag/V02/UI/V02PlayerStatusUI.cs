using TalismanBag.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text statusText;

        public void Refresh(CombatStats stats)
        {
            if (stats == null || stats.poisonStacks <= 0 && stats.burnStacks <= 0)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }

                SetText(statusText, string.Empty);
                return;
            }

            if (panel != null)
            {
                panel.SetActive(true);
            }

            SetText(statusText, $"中毒：{stats.poisonStacks}  燃烧：{stats.burnStacks}");
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
