using System.Collections;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Feedback
{
    public sealed class FloatingCombatText : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private RectTransform floatingRoot;
        [SerializeField] private FloatingCombatTextAnchorLayout anchorLayout;
        [SerializeField] private GameObject editorPreviewObject;
        [SerializeField] private RectTransform damageDealtPreviewAnchor;
        [SerializeField] private RectTransform damageTakenPreviewAnchor;
        [SerializeField] private RectTransform manaSpentPreviewAnchor;
        [SerializeField] private RectTransform shieldBreakPreviewAnchor;
        [SerializeField] private Font font;
        [SerializeField] private Vector2 playerFallbackPosition = new(-700f, 120f);
        [SerializeField] private Vector2 enemyFallbackPosition = new(700f, 120f);
        [SerializeField] private Vector2 manaFallbackPosition = new(0f, 480f);

        private void Awake()
        {
            if (anchorLayout == null && floatingRoot != null)
            {
                anchorLayout = floatingRoot.GetComponentInChildren<FloatingCombatTextAnchorLayout>(true);
            }

            anchorLayout?.HideEditorAnchors();
            HideEditorPreviewObject(editorPreviewObject);
            HideEditorPreviewAnchor(damageDealtPreviewAnchor);
            HideEditorPreviewAnchor(damageTakenPreviewAnchor);
            HideEditorPreviewAnchor(manaSpentPreviewAnchor);
            HideEditorPreviewAnchor(shieldBreakPreviewAnchor);

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
            Spawn("\u7075\u6c14 +12", GetPreviewPosition(BattleEventType.ManaGenerated, string.Empty, manaFallbackPosition), new Color(0.35f, 0.78f, 1f));
            Spawn("\u7075\u6c14 -8", GetPreviewPosition(BattleEventType.ManaSpent, string.Empty, manaFallbackPosition), new Color(0.55f, 0.45f, 0.9f));
            Spawn("\u53d7\u51fb -12", GetPreviewPosition(BattleEventType.DamageTaken, string.Empty, playerFallbackPosition), new Color(1f, 0.35f, 0.35f));
            Spawn("\u62a4\u76fe +16", GetPreviewPosition(BattleEventType.ShieldGained, string.Empty, playerFallbackPosition), new Color(1f, 0.82f, 0.35f));
            Spawn("\u4f24\u5bb3 -18", GetPreviewPosition(BattleEventType.DamageDealt, string.Empty, enemyFallbackPosition), new Color(1f, 0.45f, 0.35f));
            Spawn("\u7834\u76fe\uff01", GetPreviewPosition(BattleEventType.EnemyCountered, CounterFeedbackType.ShieldBreak.ToString(), enemyFallbackPosition), new Color(1f, 0.95f, 0.45f));
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
                BattleEventType.ManaGenerated => eventData.value > 0 ? $"\u7075\u6c14 +{eventData.value}" : string.Empty,
                BattleEventType.ManaSpent => eventData.value > 0 ? $"\u7075\u6c14 -{eventData.value}" : string.Empty,
                BattleEventType.DamageDealt => eventData.message.Contains("\u66b4\u51fb") ? $"\u66b4\u51fb -{eventData.value}" : $"-{eventData.value}",
                BattleEventType.DamageTaken => eventData.sourceId == StatusEffectIds.StatusDamage ? $"毒火 -{eventData.value}" : $"-{eventData.value}",
                BattleEventType.ShieldGained => $"\u62a4\u76fe +{eventData.value}",
                BattleEventType.HealReceived => $"+{eventData.value}",
                BattleEventType.EnemyInterrupted => "\u6253\u65ad\uff01",
                BattleEventType.EnemyEnraged => "\u72c2\u66b4\uff01",
                BattleEventType.EnemyCountered => TryGetCounterFloatingText(eventData.sourceId),
                BattleEventType.ItemSealed => "\u5c01\u5370\uff01",
                BattleEventType.ItemUnsealed => "\u89e3\u9664",
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
            if (anchorLayout != null && anchorLayout.TryGetPosition(eventData, out Vector2 layoutPosition))
            {
                return layoutPosition;
            }

            if (TryGetPreviewAnchorPosition(eventData, out Vector2 previewPosition))
            {
                return previewPosition;
            }

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

        private bool TryGetPreviewAnchorPosition(BattleEventData eventData, out Vector2 position)
        {
            if (eventData.sourceId == StatusEffectIds.StatusDamage)
            {
                position = Vector2.zero;
                return false;
            }

            RectTransform anchor = eventData.eventType switch
            {
                BattleEventType.DamageDealt => damageDealtPreviewAnchor,
                BattleEventType.DamageTaken => damageTakenPreviewAnchor,
                BattleEventType.ManaSpent => manaSpentPreviewAnchor,
                BattleEventType.EnemyCountered when IsShieldBreak(eventData) => shieldBreakPreviewAnchor,
                _ => null
            };

            return TryGetAnchorPosition(anchor, out position);
        }

        private Vector2 GetPreviewPosition(BattleEventType eventType, string sourceId, Vector2 fallback)
        {
            if (anchorLayout != null && anchorLayout.TryGetPreviewPosition(eventType, sourceId, out Vector2 layoutPosition))
            {
                return layoutPosition;
            }

            return eventType switch
            {
                BattleEventType.DamageDealt => GetAnchorPosition(damageDealtPreviewAnchor, fallback),
                BattleEventType.DamageTaken => GetAnchorPosition(damageTakenPreviewAnchor, fallback),
                BattleEventType.ManaSpent => GetAnchorPosition(manaSpentPreviewAnchor, fallback),
                BattleEventType.EnemyCountered when sourceId == CounterFeedbackType.ShieldBreak.ToString() => GetAnchorPosition(shieldBreakPreviewAnchor, fallback),
                _ => fallback
            };
        }

        private static bool IsShieldBreak(BattleEventData eventData)
        {
            return System.Enum.TryParse(eventData.sourceId, out CounterFeedbackType type) &&
                   type == CounterFeedbackType.ShieldBreak;
        }

        private static Vector2 GetAnchorPosition(RectTransform anchor, Vector2 fallback)
        {
            return TryGetAnchorPosition(anchor, out Vector2 position) ? position : fallback;
        }

        private static bool TryGetAnchorPosition(RectTransform anchor, out Vector2 position)
        {
            if (anchor != null)
            {
                position = anchor.anchoredPosition;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        private static void HideEditorPreviewObject(GameObject previewObject)
        {
            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
        }

        private static void HideEditorPreviewAnchor(RectTransform anchor)
        {
            if (anchor != null)
            {
                anchor.gameObject.SetActive(false);
            }
        }

        private static Color GetColor(BattleEventData eventData)
        {
            return eventData.eventType switch
            {
                BattleEventType.ManaGenerated => new Color(0.35f, 0.78f, 1f),
                BattleEventType.ManaSpent => new Color(0.55f, 0.45f, 0.9f),
                BattleEventType.DamageDealt => new Color(1f, 0.48f, 0.3f),
                BattleEventType.DamageTaken => eventData.sourceId == StatusEffectIds.StatusDamage ? new Color(0.72f, 1f, 0.42f) : new Color(1f, 0.35f, 0.35f),
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
