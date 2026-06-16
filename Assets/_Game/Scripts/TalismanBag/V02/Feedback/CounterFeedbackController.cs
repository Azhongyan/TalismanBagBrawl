using TalismanBag.Feedback;
using UnityEngine;

namespace TalismanBag.V02.Feedback
{
    public enum CounterFeedbackType
    {
        None,
        ShieldBreak,
        Cleanse,
        Unseal,
        SoulSuppress,
        ChainClear,
        FormationProtected,
        GuardReduce,
        CounterFailed
    }

    public sealed class CounterFeedbackController : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;

        public void ShowCounterFeedback(CounterFeedbackType type, Vector3 worldPosition)
        {
            ShowCounterFeedbackAtScreen(type, new Vector2(worldPosition.x, worldPosition.y));
        }

        public void ShowCounterFeedback(CounterFeedbackType type, Transform target)
        {
            Vector3 position = target != null ? target.position : Vector3.zero;
            ShowCounterFeedback(type, position);
        }

        public void ShowCounterFeedbackAtScreen(CounterFeedbackType type, Vector2 screenPosition)
        {
            Emit(type, GetLogText(type), screenPosition);
        }

        public void LogCounterFeedback(CounterFeedbackType type, string detail)
        {
            Emit(type, string.IsNullOrWhiteSpace(detail) ? GetLogText(type) : detail, Vector2.zero);
        }

        private void Emit(CounterFeedbackType type, string message, Vector2 screenPosition)
        {
            eventRouter?.Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, message, type.ToString(), value: 1, screenPosition: screenPosition);
        }

        public static string GetFloatingText(CounterFeedbackType type)
        {
            return type switch
            {
                CounterFeedbackType.ShieldBreak => "\u7834\u76fe\uff01",
                CounterFeedbackType.Cleanse => "\u51c0\u5316\uff01",
                CounterFeedbackType.Unseal => "\u89e3\u5c01\uff01",
                CounterFeedbackType.SoulSuppress => "\u9547\u9b42\u53cd\u5236\uff01",
                CounterFeedbackType.ChainClear => "\u8fde\u9501\u6e05\u573a\uff01",
                CounterFeedbackType.FormationProtected => "\u9635\u773c\u4fdd\u62a4\u6210\u529f\uff01",
                CounterFeedbackType.GuardReduce => "\u62a4\u8eab\u62b5\u6321\uff01",
                CounterFeedbackType.CounterFailed => "\u514b\u5236\u5931\u8d25",
                _ => "\u514b\u5236\uff01"
            };
        }

        public static string GetLogText(CounterFeedbackType type)
        {
            return type switch
            {
                CounterFeedbackType.ShieldBreak => "[\u514b\u5236] \u7834\u76fe\u89e6\u53d1",
                CounterFeedbackType.Cleanse => "[\u514b\u5236] \u51c0\u5316\u89e6\u53d1",
                CounterFeedbackType.Unseal => "[\u514b\u5236] \u89e3\u5c01\u89e6\u53d1",
                CounterFeedbackType.SoulSuppress => "[\u514b\u5236] \u9547\u9b42\u7b26\u53cd\u5236\u5077\u7075",
                CounterFeedbackType.ChainClear => "[\u514b\u5236] \u8fde\u9501\u96f7\u7b26\u89e6\u53d1\u6e05\u573a",
                CounterFeedbackType.FormationProtected => "[\u514b\u5236] \u9635\u773c\u4fdd\u62a4\u6210\u529f",
                CounterFeedbackType.GuardReduce => "[\u514b\u5236] \u62a4\u8eab\u7b26\u524a\u5f31\u7206\u53d1\u4f24\u5bb3",
                CounterFeedbackType.CounterFailed => "[\u514b\u5236] \u514b\u5236\u5931\u8d25",
                _ => "[\u514b\u5236] \u514b\u5236\u89e6\u53d1"
            };
        }
    }
}
