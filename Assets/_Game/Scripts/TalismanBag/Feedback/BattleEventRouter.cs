using System;
using UnityEngine;

namespace TalismanBag.Feedback
{
    public enum BattleEventType
    {
        LogMessage,
        ManaGenerated,
        ManaSpent,
        DamageDealt,
        DamageTaken,
        ShieldGained,
        HealReceived,
        ComboActivated,
        ComboDeactivated,
        EnemyCharging,
        EnemyInterrupted,
        EnemyEnraged,
        ItemTriggered,
        ItemSealed,
        ItemUnsealed,
        EnemyCountered,
        BattleWin,
        BattleLose
    }

    public enum BattleLogCategory
    {
        Normal,
        Mana,
        Damage,
        Defense,
        Heal,
        Combo,
        Danger,
        Counter,
        Seal,
        Result
    }

    public struct BattleEventData
    {
        public BattleEventType eventType;
        public BattleLogCategory logCategory;
        public string sourceId;
        public string targetId;
        public int value;
        public Vector2 screenPosition;
        public Vector2 targetScreenPosition;
        public string message;
    }

    public sealed class BattleEventRouter : MonoBehaviour
    {
        public event Action<BattleEventData> BattleEvent;

        public void Emit(BattleEventData eventData)
        {
            BattleEvent?.Invoke(eventData);
        }

        public void Emit(
            BattleEventType eventType,
            BattleLogCategory category,
            string message,
            string sourceId = "",
            string targetId = "",
            int value = 0,
            Vector2 screenPosition = default,
            Vector2 targetScreenPosition = default)
        {
            BattleEvent?.Invoke(new BattleEventData
            {
                eventType = eventType,
                logCategory = category,
                message = message,
                sourceId = sourceId,
                targetId = targetId,
                value = value,
                screenPosition = screenPosition,
                targetScreenPosition = targetScreenPosition
            });
        }
    }
}
