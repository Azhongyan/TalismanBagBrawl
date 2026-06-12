using System.Collections;
using System.Collections.Generic;
using TalismanBag.Feedback;
using TalismanBag.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Feedback
{
    public sealed class TalismanTriggerFeedback : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;

        private readonly Dictionary<DraggableTalismanItemView, Coroutine> runningFeedback = new();
        private readonly Dictionary<DraggableTalismanItemView, Vector3> baseScales = new();
        private readonly Dictionary<DraggableTalismanItemView, Color> baseColors = new();

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

        public void TriggerAllFeedback()
        {
            foreach (DraggableTalismanItemView view in FindObjectsOfType<DraggableTalismanItemView>())
            {
                PlayFeedback(view, GetFlashColor(view.Definition != null ? view.Definition.itemId : ""));
            }
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            if (string.IsNullOrEmpty(eventData.sourceId))
            {
                return;
            }

            if (eventData.eventType != BattleEventType.ItemTriggered)
            {
                return;
            }

            foreach (DraggableTalismanItemView view in FindObjectsOfType<DraggableTalismanItemView>())
            {
                if (view.Definition != null && view.Definition.itemId == eventData.sourceId)
                {
                    PlayFeedback(view, GetFlashColor(eventData.sourceId));
                    return;
                }
            }
        }

        private void PlayFeedback(DraggableTalismanItemView view, Color color)
        {
            if (view == null)
            {
                return;
            }

            Image image = view.GetComponent<Image>();
            RectTransform rect = view.GetComponent<RectTransform>();
            if (image == null || rect == null)
            {
                return;
            }

            if (!baseScales.ContainsKey(view))
            {
                baseScales[view] = rect.localScale;
            }

            if (!baseColors.ContainsKey(view))
            {
                baseColors[view] = image.color;
            }

            if (runningFeedback.TryGetValue(view, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }

            rect.localScale = baseScales[view];
            image.color = baseColors[view];
            runningFeedback[view] = StartCoroutine(FlashView(view, color));
        }

        private IEnumerator FlashView(DraggableTalismanItemView view, Color color)
        {
            Image image = view.GetComponent<Image>();
            RectTransform rect = view.GetComponent<RectTransform>();
            if (image == null || rect == null)
            {
                yield break;
            }

            Color original = baseColors.TryGetValue(view, out Color storedColor) ? storedColor : image.color;
            Vector3 originalScale = baseScales.TryGetValue(view, out Vector3 storedScale) ? storedScale : rect.localScale;
            image.color = color;
            rect.localScale = originalScale * 1.15f;
            yield return new WaitForSeconds(0.15f);
            image.color = original;
            rect.localScale = originalScale;
            runningFeedback.Remove(view);
        }

        private static Color GetFlashColor(string itemId)
        {
            return itemId switch
            {
                "spirit_stone_basic" => new Color(0.28f, 0.7f, 1f),
                "fire_talisman_basic" => new Color(1f, 0.35f, 0.1f),
                "thunder_talisman_basic" => new Color(0.92f, 0.88f, 1f),
                "shield_talisman_basic" => new Color(1f, 0.82f, 0.28f),
                "qi_pill_basic" => new Color(0.45f, 1f, 0.48f),
                "sword_pill_basic" => new Color(0.88f, 0.92f, 1f),
                "exorcism_bell_basic" => new Color(1f, 0.85f, 0.18f),
                "water_talisman_basic" => new Color(0.25f, 0.95f, 0.86f),
                _ => Color.white
            };
        }
    }
}
