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
        private Coroutine interruptTextRoutine;
        private Vector3 enemyBaseLocalPosition;
        private bool hasEnemyBasePosition;
        private bool isShaking;
        private float shakeElapsed;
        private const int ShakeStepCount = 8;
        private const float ShakeOffset = 8f;
        private const float ShakeStepDuration = 0.025f;
        private string currentChargeLabel = "\u8fde\u65a9\u84c4\u529b\u4e2d";

        private void Awake()
        {
            if (enemyFlashTarget != null)
            {
                enemyBaseColor = enemyFlashTarget.color;
            }

            CaptureEnemyBasePosition();

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

        private void LateUpdate()
        {
            if (!isShaking || enemyVisual == null)
            {
                return;
            }

            shakeElapsed += Time.deltaTime;
            float totalDuration = ShakeStepCount * ShakeStepDuration;
            if (shakeElapsed >= totalDuration)
            {
                enemyVisual.localPosition = enemyBaseLocalPosition;
                isShaking = false;
                return;
            }

            int step = Mathf.FloorToInt(shakeElapsed / ShakeStepDuration);
            float x = step % 2 == 0 ? ShakeOffset : -ShakeOffset;
            enemyVisual.localPosition = enemyBaseLocalPosition + new Vector3(x, 0f, 0f);
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
                        chargeText.text = "\u6253\u65ad\uff01";
                        interruptTextRoutine = StartCoroutine(HideInterruptTextRoutine());
                    }

                    Flash(Color.white);
                    Shake();
                    break;
                case BattleEventType.EnemyCharging:
                    if (!string.IsNullOrWhiteSpace(eventData.message))
                    {
                        currentChargeLabel = eventData.message.Contains("\u5fc3\u9b54")
                            ? "\u5fc3\u9b54\u51b2\u51fb\u84c4\u529b\u4e2d"
                            : "\u8fde\u65a9\u84c4\u529b\u4e2d";
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

            if (isShaking)
            {
                enemyVisual.localPosition = enemyBaseLocalPosition;
            }

            enemyBaseLocalPosition = enemyVisual.localPosition;
            hasEnemyBasePosition = true;
            shakeElapsed = 0f;
            isShaking = true;
            enemyVisual.localPosition = enemyBaseLocalPosition + new Vector3(ShakeOffset, 0f, 0f);
        }

        private IEnumerator FlashRoutine(Color color)
        {
            enemyFlashTarget.color = color;
            yield return new WaitForSeconds(0.12f);
            enemyFlashTarget.color = enemyBaseColor;
            flashRoutine = null;
        }

        private void CaptureEnemyBasePosition()
        {
            if (enemyVisual == null || hasEnemyBasePosition)
            {
                return;
            }

            enemyBaseLocalPosition = enemyVisual.localPosition;
            hasEnemyBasePosition = true;
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
