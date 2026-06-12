using System.Collections;
using TalismanBag.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.VFX
{
    public sealed class ManaFlowView : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private RectTransform flowRoot;
        [SerializeField] private Color flowColor = new(0.24f, 0.72f, 1f, 0.85f);

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

        public void Play(Vector2 from, Vector2 to)
        {
            if (flowRoot == null || from == to)
            {
                return;
            }

            GameObject line = new("ManaFlow", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            line.transform.SetParent(flowRoot, false);
            RectTransform rect = line.GetComponent<RectTransform>();
            Vector2 delta = to - from;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = from + delta * 0.5f;
            rect.sizeDelta = new Vector2(delta.magnitude, 7f);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            Image image = line.GetComponent<Image>();
            image.color = flowColor;
            image.raycastTarget = false;

            StartCoroutine(Animate(line, line.GetComponent<CanvasGroup>()));
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            if (eventData.eventType == BattleEventType.ManaGenerated && eventData.targetScreenPosition != Vector2.zero)
            {
                Play(eventData.screenPosition, eventData.targetScreenPosition);
            }
        }

        private IEnumerator Animate(GameObject line, CanvasGroup canvasGroup)
        {
            float elapsed = 0f;
            while (elapsed < 0.45f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.45f);
                canvasGroup.alpha = 1f - t;
                line.transform.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 0.35f, t), 1f);
                yield return null;
            }

            Destroy(line);
        }
    }
}
