using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.Result
{
    public sealed class V02RunResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text reasonText;
        [SerializeField] private Text detailText;
        [SerializeField] private Button restartButton;

        public event Action RestartRequested;

        private void Awake()
        {
            restartButton?.onClick.AddListener(() => RestartRequested?.Invoke());
            Hide();
        }

        public void ShowWin(V02RunStatsTracker stats, int totalRounds = 10)
        {
            Show();
            SetText(titleText, "\u901a\u5173\u6210\u529f");
            SetText(summaryText, $"\u4f60\u5b8c\u6210\u4e86 {Mathf.Max(1, totalRounds)} \u573a\u4e3b\u7ebf\u8bd5\u70bc\u3002\n\u8bc4\u4ef7\uff1a\u9635\u6cd5\u5df2\u5177\u5907 V0.2 \u57fa\u7840\u5bf9\u6297\u80fd\u529b\u3002");
            SetText(reasonText, "\u672c\u5c40\u6838\u5fc3\u8868\u73b0");
            SetText(detailText, stats != null ? stats.BuildSummary() : string.Empty);
        }

        public void ShowLose(int roundIndex, string roundTitle, V02FailureReasonResult reason, V02RunStatsTracker stats, int totalRounds = 10)
        {
            Show();
            string title = string.IsNullOrWhiteSpace(roundTitle) ? $"Round {roundIndex}" : $"Round {roundIndex} / {Mathf.Max(1, totalRounds)}  {roundTitle}";
            SetText(titleText, "\u4fee\u58eb\u8d25\u5317");
            SetText(summaryText, $"\u5931\u8d25\u5173\u5361\uff1a{title}");
            SetText(reasonText, $"\u5931\u8d25\u539f\u56e0\uff1a{reason.title}\n{reason.description}\n\n\u5efa\u8bae\uff1a{reason.suggestion}");
            SetText(detailText, stats != null ? stats.BuildSummary() : string.Empty);
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Show()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
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
