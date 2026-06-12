using System.Collections;
using TalismanBag.Feedback;
using TalismanBag.V02.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Feedback
{
    public sealed class FloatingCombatText : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private RectTransform floatingRoot;
        [SerializeField] private Font font;
        [SerializeField] private Vector2 playerFallbackPosition = new(-700f, 120f);
        [SerializeField] private Vector2 enemyFallbackPosition = new(700f, 120f);
        [SerializeField] private Vector2 manaFallbackPosition = new(0f, 480f);

        private void Awake()
        {
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

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

        public void TestFloatingText()
        {
            Spawn("灵气 +12", manaFallbackPosition, new Color(0.35f, 0.78f, 1f));
            Spawn("+20", playerFallbackPosition, new Color(0.45f, 1f, 0.48f));
            Spawn("-18", enemyFallbackPosition, new Color(1f, 0.45f, 0.35f));
            Spawn("封印！", Vector2.zero, new Color(0.75f, 0.75f, 0.75f));
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            string text = GetText(eventData);
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Vector2 position = GetPosition(eventData);
            Spawn(text, position, GetColor(eventData));
        }

        private string GetText(BattleEventData eventData)
        {
            return eventData.eventType switch
            {
                BattleEventType.ManaGenerated => eventData.value > 0 ? $"灵气 +{eventData.value}" : string.Empty,
                BattleEventType.ManaSpent => eventData.value > 0 ? $"灵气 -{eventData.value}" : string.Empty,
                BattleEventType.DamageDealt => eventData.message.Contains("暴击") ? $"暴击 -{eventData.value}" : $"-{eventData.value}",
                BattleEventType.DamageTaken => $"-{eventData.value}",
                BattleEventType.ShieldGained => $"护盾 +{eventData.value}",
                BattleEventType.HealReceived => $"+{eventData.value}",
                BattleEventType.EnemyInterrupted => "打断！",
                BattleEventType.EnemyEnraged => "狂暴！",
                BattleEventType.EnemyCountered => TryGetCounterFloatingText(eventData.sourceId),
                BattleEventType.ItemSealed => "封印！",
                BattleEventType.ItemUnsealed => "解除",
                _ => string.Empty
            };
        }

        private static string TryGetCounterFloatingText(string sourceId)
        {
            return System.Enum.TryParse(sourceId, out CounterFeedbackType type)
                ? CounterFeedbackController.GetFloatingText(type)
                : string.Empty;
        }

        private Vector2 GetPosition(BattleEventData eventData)
        {
            if (eventData.screenPosition != Vector2.zero)
            {
                return eventData.screenPosition;
            }

            return eventData.eventType switch
            {
                BattleEventType.ManaGenerated or BattleEventType.ManaSpent => manaFallbackPosition,
                BattleEventType.DamageDealt or BattleEventType.EnemyCountered or BattleEventType.EnemyInterrupted or BattleEventType.EnemyEnraged => enemyFallbackPosition,
                BattleEventType.DamageTaken or BattleEventType.ShieldGained or BattleEventType.HealReceived => playerFallbackPosition,
                _ => Vector2.zero
            };
        }

        private static Color GetColor(BattleEventData eventData)
        {
            return eventData.eventType switch
            {
                BattleEventType.ManaGenerated => new Color(0.35f, 0.78f, 1f),
                BattleEventType.ManaSpent => new Color(0.55f, 0.45f, 0.9f),
                BattleEventType.DamageDealt => new Color(1f, 0.48f, 0.3f),
                BattleEventType.DamageTaken => new Color(1f, 0.35f, 0.35f),
                BattleEventType.ShieldGained => new Color(1f, 0.82f, 0.35f),
                BattleEventType.HealReceived => new Color(0.45f, 1f, 0.48f),
                BattleEventType.EnemyInterrupted => Color.white,
                BattleEventType.EnemyEnraged => new Color(1f, 0.32f, 0.22f),
                BattleEventType.EnemyCountered => new Color(1f, 0.95f, 0.45f),
                BattleEventType.ItemSealed => new Color(0.78f, 0.78f, 0.78f),
                _ => Color.white
            };
        }

        private void Spawn(string content, Vector2 anchoredPosition, Color color)
        {
            if (floatingRoot == null)
            {
                return;
            }

            GameObject textObject = new("FloatingText", typeof(RectTransform), typeof(Text), typeof(CanvasGroup));
            textObject.transform.SetParent(floatingRoot, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(180f, 40f);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;

            StartCoroutine(Animate(textObject, rect, textObject.GetComponent<CanvasGroup>()));
        }

        private IEnumerator Animate(GameObject textObject, RectTransform rect, CanvasGroup canvasGroup)
        {
            float elapsed = 0f;
            Vector2 start = rect.anchoredPosition;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.8f);
                rect.anchoredPosition = start + new Vector2(0f, t * 42f);
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            Destroy(textObject);
        }
    }
}
