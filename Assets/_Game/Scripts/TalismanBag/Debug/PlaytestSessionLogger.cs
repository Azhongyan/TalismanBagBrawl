using TalismanBag.Feedback;
using UnityEngine;

namespace TalismanBag.Debugging
{
    public sealed class PlaytestSessionLogger : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;

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

        public static void Log(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                UnityEngine.Debug.Log($"[Playtest] {message}");
            }
        }

        private static void LogEvent(BattleEventData eventData)
        {
            switch (eventData.eventType)
            {
                case BattleEventType.BattleWin:
                    Log($"Round Won: {eventData.message}");
                    break;
                case BattleEventType.BattleLose:
                    Log($"Round Lost: {eventData.message}");
                    break;
                case BattleEventType.EnemyInterrupted:
                    Log($"Boss/Charge Interrupted: {eventData.message}");
                    break;
                case BattleEventType.ItemSealed:
                    Log($"Item Sealed: {eventData.targetId}");
                    break;
            }
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            LogEvent(eventData);
        }
    }
}
