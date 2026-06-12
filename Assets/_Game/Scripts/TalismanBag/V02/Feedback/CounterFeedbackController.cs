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
                CounterFeedbackType.ShieldBreak => "破盾！",
                CounterFeedbackType.Cleanse => "净化！",
                CounterFeedbackType.Unseal => "解封！",
                CounterFeedbackType.SoulSuppress => "镇魂反制！",
                CounterFeedbackType.ChainClear => "连锁清场！",
                CounterFeedbackType.FormationProtected => "阵眼保护成功！",
                CounterFeedbackType.GuardReduce => "护身抵挡！",
                CounterFeedbackType.CounterFailed => "克制失败",
                _ => "克制！"
            };
        }

        public static string GetLogText(CounterFeedbackType type)
        {
            return type switch
            {
                CounterFeedbackType.ShieldBreak => "[克制] 破盾触发",
                CounterFeedbackType.Cleanse => "[克制] 净化触发",
                CounterFeedbackType.Unseal => "[克制] 解封触发",
                CounterFeedbackType.SoulSuppress => "[克制] 镇魂符反制偷灵",
                CounterFeedbackType.ChainClear => "[克制] 连锁雷符触发清场",
                CounterFeedbackType.FormationProtected => "[克制] 阵眼保护成功",
                CounterFeedbackType.GuardReduce => "[克制] 护身符削弱爆发伤害",
                CounterFeedbackType.CounterFailed => "[克制] 克制失败",
                _ => "[克制] 克制触发"
            };
        }
    }
}
