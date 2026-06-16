using System.Collections;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class TalismanCombatUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private Text hpText;
        [SerializeField] private Text shieldText;
        [SerializeField] private Text manaText;
        [SerializeField] private Text enemyFaceText;
        [SerializeField] private Text enemyTitleText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Text stateText;

        [Header("Feedback")]
        [SerializeField] private Graphic hpFlashTarget;
        [SerializeField] private RectTransform enemyVisual;
        [SerializeField] private Graphic enemyFlashTarget;
        [SerializeField] private Graphic shieldFeedback;

        private Color hpBaseColor = Color.white;
        private Color enemyBaseColor = Color.white;
        private Coroutine hpFlashRoutine;
        private Coroutine enemyShakeRoutine;
        private Coroutine shieldRoutine;
        private Vector2 enemyBaseAnchoredPosition;
        private bool hasEnemyBasePosition;

        private void Awake()
        {
            if (hpFlashTarget != null)
            {
                hpBaseColor = hpFlashTarget.color;
            }

            if (enemyFlashTarget != null)
            {
                enemyBaseColor = enemyFlashTarget.color;
            }

            CaptureEnemyBasePosition();

            if (shieldFeedback != null)
            {
                Color color = shieldFeedback.color;
                color.a = 0f;
                shieldFeedback.color = color;
            }
        }

        public void Refresh(CombatStats player, EnemyRuntime enemy, string stateLabel)
        {
            if (player != null)
            {
                if (hpText != null)
                {
                    hpText.text = $"气血 {player.hp}/{player.maxHP}";
                }

                if (shieldText != null)
                {
                    shieldText.text = $"护盾 {player.shield}";
                }

                if (manaText != null)
                {
                    manaText.text = $"灵气 {player.mana}/{player.maxMana}";
                }
            }

            if (enemy?.definition != null)
            {
                SetText(enemyFaceText, enemy.definition.GetAvatarGlyph());
                SetText(enemyTitleText, enemy.definition.GetReadableLabel());

                string bossTag = enemy.isEnraged ? "  狂暴" : string.Empty;
                string chargeTag = enemy.isCharging || enemy.isCastingSkill ? "  蓄力" : string.Empty;
                string enemyShield = enemy.currentShield > 0 ? $"  护盾 {enemy.currentShield}" : string.Empty;
                SetText(enemyHpText, $"{enemy.definition.GetReadableLabel()}：{enemy.currentHp}/{enemy.definition.maxHp}{enemyShield}{bossTag}{chargeTag}");
            }
            else
            {
                SetText(enemyFaceText, "?");
                SetText(enemyTitleText, "敌人未选择");
                SetText(enemyHpText, "敌人：--/--");
            }

            if (stateText != null)
            {
                stateText.text = stateLabel;
            }
        }

        public void FlashHP()
        {
            if (hpFlashTarget == null)
            {
                return;
            }

            if (hpFlashRoutine != null)
            {
                StopCoroutine(hpFlashRoutine);
            }

            hpFlashRoutine = StartCoroutine(FlashGraphic(hpFlashTarget, hpBaseColor, new Color(0.3f, 1f, 0.45f), 0.35f));
        }

        public void ShowShieldFeedback()
        {
            if (shieldFeedback == null)
            {
                return;
            }

            if (shieldRoutine != null)
            {
                StopCoroutine(shieldRoutine);
            }

            shieldRoutine = StartCoroutine(FadeShield());
        }

        public void ShakeEnemy()
        {
            if (enemyVisual == null)
            {
                return;
            }

            CaptureEnemyBasePosition();

            if (enemyShakeRoutine != null)
            {
                StopCoroutine(enemyShakeRoutine);
                enemyVisual.anchoredPosition = enemyBaseAnchoredPosition;
            }

            enemyShakeRoutine = StartCoroutine(ShakeEnemyRoutine());
        }

        private IEnumerator FlashGraphic(Graphic graphic, Color baseColor, Color flashColor, float duration)
        {
            graphic.color = flashColor;
            yield return new WaitForSeconds(duration);
            graphic.color = baseColor;
        }

        private IEnumerator FadeShield()
        {
            Color color = shieldFeedback.color;
            color.a = 0.45f;
            shieldFeedback.color = color;
            yield return new WaitForSeconds(0.45f);
            color.a = 0f;
            shieldFeedback.color = color;
        }

        private IEnumerator ShakeEnemyRoutine()
        {
            Vector2 start = enemyBaseAnchoredPosition;
            if (enemyFlashTarget != null)
            {
                enemyFlashTarget.color = new Color(1f, 0.45f, 0.35f);
            }

            for (int i = 0; i < 8; i++)
            {
                enemyVisual.anchoredPosition = start + new Vector2(i % 2 == 0 ? 8f : -8f, 0f);
                yield return new WaitForSeconds(0.025f);
            }

            enemyVisual.anchoredPosition = start;
            if (enemyFlashTarget != null)
            {
                enemyFlashTarget.color = enemyBaseColor;
            }

            enemyShakeRoutine = null;
        }

        private void CaptureEnemyBasePosition()
        {
            if (enemyVisual == null || hasEnemyBasePosition)
            {
                return;
            }

            enemyBaseAnchoredPosition = enemyVisual.anchoredPosition;
            hasEnemyBasePosition = true;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
