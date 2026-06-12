using System.Collections.Generic;
using TalismanBag.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileBattleLogPanel : MonoBehaviour
    {
        [SerializeField] private Text logText;
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private int maxLines = 3;

        private readonly List<LogLine> lines = new();

        private readonly struct LogLine
        {
            public readonly string Text;
            public readonly int Priority;

            public LogLine(string text, BattleLogCategory category)
            {
                Text = text;
                Priority = GetPriority(category);
            }
        }

        private void Awake()
        {
            if (eventRouter != null)
            {
                eventRouter.BattleEvent += OnBattleEvent;
            }
        }

        private void OnDestroy()
        {
            if (eventRouter != null)
            {
                eventRouter.BattleEvent -= OnBattleEvent;
            }
        }

        public void AddLog(string message)
        {
            AddLog(message, BattleLogCategory.Normal);
        }

        private void AddLog(string message, BattleLogCategory category)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            lines.Insert(0, new LogLine(message, category));
            while (lines.Count > Mathf.Max(1, maxLines))
            {
                RemoveLowestPriorityLine();
            }

            Refresh();
        }

        public void Clear()
        {
            lines.Clear();
            Refresh();
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            if (!string.IsNullOrWhiteSpace(eventData.message))
            {
                AddLog($"{GetPrefix(eventData.logCategory)} {eventData.message}", eventData.logCategory);
            }
        }

        private void Refresh()
        {
            if (logText != null)
            {
                List<string> visibleLines = new();
                foreach (LogLine line in lines)
                {
                    visibleLines.Add(line.Text);
                }

                logText.text = string.Join("\n", visibleLines);
            }
        }

        private void RemoveLowestPriorityLine()
        {
            int worstPriority = int.MinValue;
            for (int i = 0; i < lines.Count; i++)
            {
                worstPriority = Mathf.Max(worstPriority, lines[i].Priority);
            }

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Priority == worstPriority)
                {
                    lines.RemoveAt(i);
                    return;
                }
            }
        }

        private static string GetPrefix(BattleLogCategory category)
        {
            return category switch
            {
                BattleLogCategory.Danger => "[危险]",
                BattleLogCategory.Counter => "[克制]",
                BattleLogCategory.Seal => "[封印]",
                BattleLogCategory.Result => "[胜负]",
                BattleLogCategory.Damage => "[伤害]",
                BattleLogCategory.Mana => "[灵气]",
                BattleLogCategory.Defense => "[护盾]",
                BattleLogCategory.Heal => "[治疗]",
                BattleLogCategory.Combo => "[阵法]",
                _ => "[信息]"
            };
        }

        private static int GetPriority(BattleLogCategory category)
        {
            return category switch
            {
                BattleLogCategory.Danger => 0,
                BattleLogCategory.Counter => 0,
                BattleLogCategory.Seal => 0,
                BattleLogCategory.Result => 0,
                BattleLogCategory.Damage => 1,
                BattleLogCategory.Defense => 2,
                BattleLogCategory.Heal => 2,
                BattleLogCategory.Combo => 2,
                BattleLogCategory.Mana => 3,
                _ => 4
            };
        }
    }
}
