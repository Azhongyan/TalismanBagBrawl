using System.Collections;
using TalismanBag.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Feedback
{
    public sealed class EnemyFeedbackController : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private RectTransform enemyVisual;
        [SerializeField] private Graphic enemyFlashTarget;
        [SerializeField] private Image chargeFill;
        [SerializeField] private Text chargeText;

        private Color enemyBaseColor = Color.white;
        private Coroutine flashRoutine;
        private Coroutine shakeRoutine;
        private Coroutine interruptTextRoutine;
        private string currentChargeLabel = "连斩蓄力中";

        private void Awake()
        {
            if (enemyFlashTarget != null)
            {
                enemyBaseColor = enemyFlashTarget.color;
            }

            if (eventRouter != null)
            {
                eventRouter.BattleEvent += OnBattleEvent;
            }

            SetChargeProgress(0f, false);
        }

        private void OnDestroy()
        {
            if (eventRouter != null)
            {
                eventRouter.BattleEvent -= OnBattleEvent;
            }
        }

        public void SetChargeProgress(float progress, bool visible)
        {
            SetChargeProgress(progress, visible, currentChargeLabel);
        }

        public void SetChargeProgress(float progress, bool visible, string chargeLabel)
        {
            float safeProgress = Mathf.Clamp01(progress);
            if (chargeFill != null)
            {
                chargeFill.transform.parent.gameObject.SetActive(visible);
                chargeFill.fillAmount = safeProgress;
            }

            if (chargeText != null)
            {
                chargeText.gameObject.SetActive(visible);
                chargeText.text = visible ? $"{chargeLabel} {Mathf.RoundToInt(safeProgress * 100f)}%" : string.Empty;
            }
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            switch (eventData.eventType)
            {
                case BattleEventType.DamageDealt:
                    Flash(new Color(1f, 0.45f, 0.35f));
                    Shake();
                    break;
                case BattleEventType.EnemyCountered:
                    Flash(Color.white);
                    Shake();
                    break;
                case BattleEventType.EnemyEnraged:
                    Flash(new Color(1f, 0.24f, 0.18f));
                    Shake();
                    break;
                case BattleEventType.EnemyInterrupted:
                    SetChargeProgress(0f, false);
                    if (chargeText != null)
                    {
                        if (interruptTextRoutine != null)
                        {
                            StopCoroutine(interruptTextRoutine);
                        }

                        chargeText.gameObject.SetActive(true);
                        chargeText.text = "打断！";
                        interruptTextRoutine = StartCoroutine(HideInterruptTextRoutine());
                    }
                    Flash(Color.white);
                    Shake();
                    break;
                case BattleEventType.EnemyCharging:
                    if (!string.IsNullOrWhiteSpace(eventData.message))
                    {
                        currentChargeLabel = eventData.message.Contains("心魔") ? "心魔冲击蓄力中" : "连斩蓄力中";
                        Flash(new Color(1f, 0.54f, 0.2f));
                    }

                    SetChargeProgress(eventData.value / 100f, true, currentChargeLabel);
                    break;
            }
        }

        private void Flash(Color color)
        {
            if (enemyFlashTarget == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private void Shake()
        {
            if (enemyVisual == null)
            {
                return;
            }

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator FlashRoutine(Color color)
        {
            enemyFlashTarget.color = color;
            yield return new WaitForSeconds(0.12f);
            enemyFlashTarget.color = enemyBaseColor;
            flashRoutine = null;
        }

        private IEnumerator ShakeRoutine()
        {
            Vector2 start = enemyVisual.anchoredPosition;
            for (int i = 0; i < 8; i++)
            {
                enemyVisual.anchoredPosition = start + new Vector2(i % 2 == 0 ? 8f : -8f, 0f);
                yield return new WaitForSeconds(0.025f);
            }

            enemyVisual.anchoredPosition = start;
        }

        private IEnumerator HideInterruptTextRoutine()
        {
            yield return new WaitForSeconds(0.45f);
            if (chargeText != null)
            {
                chargeText.gameObject.SetActive(false);
                chargeText.text = string.Empty;
            }
        }
    }
}
