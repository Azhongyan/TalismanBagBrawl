using TalismanBag.V02.Feedback;
using UnityEngine;

namespace TalismanBag.Feedback
{
    public sealed class FloatingCombatTextAnchorLayout : MonoBehaviour
    {
        [Header("Core Numbers")]
        [SerializeField] private RectTransform damageDealtAnchor;
        [SerializeField] private RectTransform damageTakenAnchor;
        [SerializeField] private RectTransform manaGeneratedAnchor;
        [SerializeField] private RectTransform manaSpentAnchor;
        [SerializeField] private RectTransform shieldGainedAnchor;
        [SerializeField] private RectTransform healReceivedAnchor;

        [Header("Counter Feedback")]
        [SerializeField] private RectTransform shieldBreakAnchor;
        [SerializeField] private RectTransform cleanseAnchor;
        [SerializeField] private RectTransform unsealAnchor;
        [SerializeField] private RectTransform soulSuppressAnchor;
        [SerializeField] private RectTransform chainClearAnchor;
        [SerializeField] private RectTransform formationProtectedAnchor;
        [SerializeField] private RectTransform guardReduceAnchor;
        [SerializeField] private RectTransform counterFailedAnchor;

        [Header("Status Feedback")]
        [SerializeField] private RectTransform enemyInterruptedAnchor;
        [SerializeField] private RectTransform enemyEnragedAnchor;
        [SerializeField] private RectTransform itemSealedAnchor;
        [SerializeField] private RectTransform itemUnsealedAnchor;

        private void Awake()
        {
            HideEditorAnchors();
        }

        public void HideEditorAnchors()
        {
            Hide(damageDealtAnchor);
            Hide(damageTakenAnchor);
            Hide(manaGeneratedAnchor);
            Hide(manaSpentAnchor);
            Hide(shieldGainedAnchor);
            Hide(healReceivedAnchor);
            Hide(shieldBreakAnchor);
            Hide(cleanseAnchor);
            Hide(unsealAnchor);
            Hide(soulSuppressAnchor);
            Hide(chainClearAnchor);
            Hide(formationProtectedAnchor);
            Hide(guardReduceAnchor);
            Hide(counterFailedAnchor);
            Hide(enemyInterruptedAnchor);
            Hide(enemyEnragedAnchor);
            Hide(itemSealedAnchor);
            Hide(itemUnsealedAnchor);
        }

        public bool TryGetPosition(BattleEventData eventData, out Vector2 position)
        {
            RectTransform anchor = ResolveAnchor(eventData);
            if (anchor != null)
            {
                position = anchor.anchoredPosition;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        public bool TryGetPreviewPosition(BattleEventType eventType, string sourceId, out Vector2 position)
        {
            return TryGetPosition(new BattleEventData { eventType = eventType, sourceId = sourceId }, out position);
        }

        private RectTransform ResolveAnchor(BattleEventData eventData)
        {
            return eventData.eventType switch
            {
                BattleEventType.ManaGenerated => manaGeneratedAnchor,
                BattleEventType.ManaSpent => manaSpentAnchor,
                BattleEventType.DamageDealt => damageDealtAnchor,
                BattleEventType.DamageTaken => damageTakenAnchor,
                BattleEventType.ShieldGained => shieldGainedAnchor,
                BattleEventType.HealReceived => healReceivedAnchor,
                BattleEventType.EnemyInterrupted => enemyInterruptedAnchor,
                BattleEventType.EnemyEnraged => enemyEnragedAnchor,
                BattleEventType.ItemSealed => itemSealedAnchor,
                BattleEventType.ItemUnsealed => itemUnsealedAnchor,
                BattleEventType.EnemyCountered => ResolveCounterAnchor(eventData.sourceId),
                _ => null
            };
        }

        private RectTransform ResolveCounterAnchor(string sourceId)
        {
            if (!System.Enum.TryParse(sourceId, out CounterFeedbackType type))
            {
                return null;
            }

            return type switch
            {
                CounterFeedbackType.ShieldBreak => shieldBreakAnchor,
                CounterFeedbackType.Cleanse => cleanseAnchor,
                CounterFeedbackType.Unseal => unsealAnchor,
                CounterFeedbackType.SoulSuppress => soulSuppressAnchor,
                CounterFeedbackType.ChainClear => chainClearAnchor,
                CounterFeedbackType.FormationProtected => formationProtectedAnchor,
                CounterFeedbackType.GuardReduce => guardReduceAnchor,
                CounterFeedbackType.CounterFailed => counterFailedAnchor,
                _ => null
            };
        }

        private static void Hide(RectTransform anchor)
        {
            if (anchor != null)
            {
                anchor.gameObject.SetActive(false);
            }
        }
    }
}
