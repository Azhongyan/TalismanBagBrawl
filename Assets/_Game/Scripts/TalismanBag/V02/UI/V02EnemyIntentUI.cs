using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02EnemyIntentUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text intentText;
        [SerializeField] private Text timerText;

        public void Refresh(EnemyRuntime enemyRuntime)
        {
            if (enemyRuntime == null)
            {
                ClearIntent();
                return;
            }

            float remaining = enemyRuntime.currentCastingSkill != null ? enemyRuntime.currentCastingSkill.castTimer : 0f;
            string text = string.IsNullOrWhiteSpace(enemyRuntime.currentIntentText)
                ? "普通攻击"
                : enemyRuntime.currentIntentText;
            ShowIntent(text, remaining);
        }

        public void ShowIntent(string text, float remainingCastTime)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            SetText(titleText, "敌人意图");
            SetText(intentText, string.IsNullOrWhiteSpace(text) ? "普通攻击" : text);
            SetText(timerText, remainingCastTime > 0f ? $"{remainingCastTime:0.0}s" : string.Empty);
        }

        public void ClearIntent()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            SetText(titleText, string.Empty);
            SetText(intentText, string.Empty);
            SetText(timerText, string.Empty);
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
