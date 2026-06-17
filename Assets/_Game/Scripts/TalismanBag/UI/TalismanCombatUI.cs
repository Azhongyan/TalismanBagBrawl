using System.Collections;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class TalismanCombatUI : MonoBehaviour
    {
        private const string HpBarName = "PlayerHPBar";
        private const string HpFillName = "Fill";
        private const string ManaBarName = "PlayerManaBar";
        private const string EnemyHpBarName = "EnemyHPBar";
        private const string EnemyEnergyBarName = "EnemyEnergyBar";

        [Header("Texts")]
        [SerializeField] private Text hpText;
        [SerializeField] private Text shieldText;
        [SerializeField] private Text manaText;
        [SerializeField] private Text enemyFaceText;
        [SerializeField] private Text enemyTitleText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Text stateText;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image manaFillImage;
        [SerializeField] private Image enemyHpFillImage;
        [SerializeField] private Image enemyEnergyFillImage;

        [Header("Feedback")]
        [SerializeField] private Graphic hpFlashTarget;
        [SerializeField] private RectTransform enemyVisual;
        [SerializeField] private Graphic enemyFlashTarget;
        [SerializeField] private Graphic playerHitFeedback;
        [SerializeField] private Graphic shieldFeedback;

        private Color hpBaseColor = Color.white;
        private Color enemyBaseColor = Color.white;
        private Color playerHitBaseColor = Color.white;
        private Coroutine hpFlashRoutine;
        private Coroutine playerHitRoutine;
        private Coroutine enemyShakeRoutine;
        private Coroutine shieldRoutine;
        private RectTransform hpBarRoot;
        private RectTransform manaBarRoot;
        private RectTransform enemyHpBarRoot;
        private RectTransform enemyEnergyBarRoot;
        private bool generatedHpBar;
        private bool generatedManaBar;
        private bool generatedEnemyHpBar;
        private bool generatedEnemyEnergyBar;
        private Vector2 enemyBaseAnchoredPosition;
        private bool hasEnemyBasePosition;

        private void Awake()
        {
            EnsureCombatBars(Application.isPlaying);

            if (hpFlashTarget != null)
            {
                hpBaseColor = hpFlashTarget.color;
            }

            if (enemyFlashTarget != null)
            {
                enemyBaseColor = enemyFlashTarget.color;
            }

            CaptureEnemyBasePosition();

            if (playerHitFeedback != null)
            {
                playerHitBaseColor = playerHitFeedback.color;
                playerHitFeedback.raycastTarget = false;
                Color color = playerHitBaseColor;
                color.a = 0f;
                playerHitFeedback.color = color;
            }

            if (shieldFeedback != null)
            {
                Color color = shieldFeedback.color;
                color.a = 0f;
                shieldFeedback.color = color;
            }
        }

        public void Refresh(CombatStats player, EnemyRuntime enemy, string stateLabel)
        {
            EnsureCombatBars(Application.isPlaying);
            if (player != null)
            {
                if (hpText != null)
                {
                    hpText.text = $"{player.hp}/{player.maxHP} 气血";
                }

                if (hpFillImage != null)
                {
                    SetBarFill(hpFillImage, player.maxHP > 0 ? player.hp / (float)player.maxHP : 0f);
                }

                if (shieldText != null)
                {
                    shieldText.text = $"护盾 {player.shield}";
                }

                if (manaText != null)
                {
                    manaText.text = $"{player.mana}/{player.maxMana} 灵气";
                }

                SetBarFill(manaFillImage, player.maxMana > 0 ? player.mana / (float)player.maxMana : 0f);
            }

            if (enemy?.definition != null)
            {
                SetText(enemyFaceText, enemy.definition.GetAvatarGlyph());
                SetText(enemyTitleText, enemy.definition.GetReadableLabel());

                SetText(enemyHpText, $"{enemy.currentHp}/{enemy.definition.maxHp} 气血");
                SetBarFill(enemyHpFillImage, enemy.definition.maxHp > 0 ? enemy.currentHp / (float)enemy.definition.maxHp : 0f, true);
                bool showEnemyReadiness = ShouldShowEnemyReadinessBar(enemy);
                SetEnemyEnergyBarVisible(showEnemyReadiness);
                SetBarFill(enemyEnergyFillImage, showEnemyReadiness ? GetEnemyEnergyProgress(enemy) : 0f);
            }
            else
            {
                SetText(enemyFaceText, "?");
                SetText(enemyTitleText, "敌人未选择");
                SetText(enemyHpText, "--/-- 气血");
                SetBarFill(enemyHpFillImage, 0f, true);
                SetEnemyEnergyBarVisible(false);
                SetBarFill(enemyEnergyFillImage, 0f);
            }

            if (stateText != null)
            {
                stateText.text = stateLabel;
            }
        }

        private void EnsureCombatBars(bool runtimeFallback)
        {
            EnsureHpBar(runtimeFallback);
            EnsureStatusBar(ref manaFillImage, ref manaBarRoot, ref generatedManaBar, manaText, ManaBarName,
                new Color(0.232f, 0.291f, 0.447f, 0.78f), new Color(0.467f, 0.635f, 0.83f, 0.95f), Vector2.zero, -1f, runtimeFallback);
            EnsureStatusBar(ref enemyHpFillImage, ref enemyHpBarRoot, ref generatedEnemyHpBar, enemyHpText, EnemyHpBarName,
                new Color(0.18f, 0.025f, 0.025f, 0.78f), new Color(0.491f, 0.098f, 0.088f, 0.95f), Vector2.zero, -1f, runtimeFallback);
            EnsureStatusBar(ref enemyEnergyFillImage, ref enemyEnergyBarRoot, ref generatedEnemyEnergyBar, enemyHpText, EnemyEnergyBarName,
                new Color(0.232f, 0.291f, 0.447f, 0.78f), new Color(0.467f, 0.635f, 0.83f, 0.95f), new Vector2(0f, -50f), -1f, runtimeFallback);
        }

        private void EnsureHpBar(bool runtimeFallback)
        {
            if (hpText == null)
            {
                return;
            }

            if (hpFillImage == null)
            {
                hpFillImage = FindExistingHpFill();
            }

            if (hpFillImage == null)
            {
                RectTransform hpRect = hpText.rectTransform;
                Transform parent = hpRect.parent;
                if (parent == null)
                {
                    return;
                }

                GameObject barObject = new(HpBarName, typeof(RectTransform), typeof(LayoutElement), typeof(Image));
                barObject.transform.SetParent(parent, false);
                LayoutElement layoutElement = barObject.GetComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                Image background = barObject.GetComponent<Image>();
                background.color = new Color(0.18f, 0.025f, 0.025f, 0.78f);
                background.raycastTarget = false;

                hpBarRoot = barObject.GetComponent<RectTransform>();
                barObject.transform.SetSiblingIndex(hpRect.GetSiblingIndex());
                CopyHpTextRectToBar();

                GameObject fillObject = new(HpFillName, typeof(RectTransform), typeof(Image));
                fillObject.transform.SetParent(barObject.transform, false);
                RectTransform fillRect = fillObject.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = new Vector2(3f, 3f);
                fillRect.offsetMax = new Vector2(-3f, -3f);

                hpFillImage = fillObject.GetComponent<Image>();
                hpFillImage.color = new Color(0.491f, 0.098f, 0.088f, 0.95f);
                hpFillImage.raycastTarget = false;
                hpFillImage.type = Image.Type.Filled;
                hpFillImage.fillMethod = Image.FillMethod.Horizontal;
                hpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                SetBarFill(hpFillImage, 1f);
                generatedHpBar = runtimeFallback;
            }
            else if (hpBarRoot == null)
            {
                hpBarRoot = hpFillImage.transform.parent as RectTransform;
            }

            if (generatedHpBar)
            {
                SyncHpBarRect();
            }
        }

        private Image FindExistingHpFill()
        {
            RectTransform hpRect = hpText.rectTransform;
            Transform parent = hpRect.parent;
            if (parent == null)
            {
                return null;
            }

            Transform bar = parent.Find(HpBarName);
            if (bar == null)
            {
                return null;
            }

            Transform fill = bar.Find(HpFillName);
            hpBarRoot = bar as RectTransform;
            if (fill == null)
            {
                GameObject fillObject = new(HpFillName, typeof(RectTransform), typeof(Image));
                fillObject.transform.SetParent(bar, false);
                RectTransform fillRect = fillObject.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = new Vector2(3f, 3f);
                fillRect.offsetMax = new Vector2(-3f, -3f);
                fill = fillObject.transform;
            }

            Image fillImage = fill.GetComponent<Image>();
            if (fillImage == null)
            {
                fillImage = fill.gameObject.AddComponent<Image>();
            }

            fillImage.color = new Color(0.491f, 0.098f, 0.088f, 0.95f);
            fillImage.raycastTarget = false;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            hpFillImage = fillImage;
            SetBarFill(hpFillImage, 1f);
            return hpFillImage;
        }

        private void EnsureStatusBar(
            ref Image fillImage,
            ref RectTransform barRoot,
            ref bool generatedBar,
            Text anchorText,
            string barName,
            Color backgroundColor,
            Color fillColor,
            Vector2 anchoredOffset,
            float heightOverride,
            bool runtimeFallback)
        {
            if (anchorText == null)
            {
                return;
            }

            if (fillImage == null)
            {
                fillImage = FindExistingStatusFill(anchorText, barName, ref barRoot, fillColor);
            }

            if (fillImage == null)
            {
                Transform parent = anchorText.rectTransform.parent;
                if (parent == null)
                {
                    return;
                }

                GameObject barObject = new(barName, typeof(RectTransform), typeof(LayoutElement), typeof(Image));
                barObject.transform.SetParent(parent, false);
                LayoutElement layoutElement = barObject.GetComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                Image background = barObject.GetComponent<Image>();
                background.color = backgroundColor;
                background.raycastTarget = false;

                barRoot = barObject.GetComponent<RectTransform>();
                barObject.transform.SetSiblingIndex(anchorText.rectTransform.GetSiblingIndex());
                CopyTextRectToBar(anchorText, barRoot, anchoredOffset, heightOverride);

                GameObject fillObject = new(HpFillName, typeof(RectTransform), typeof(Image));
                fillObject.transform.SetParent(barObject.transform, false);
                RectTransform fillRect = fillObject.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = new Vector2(3f, 3f);
                fillRect.offsetMax = new Vector2(-3f, -3f);

                fillImage = fillObject.GetComponent<Image>();
                ConfigureBarFill(fillImage, fillColor);
                SetBarFill(fillImage, 0f);
                generatedBar = runtimeFallback;
            }
            else if (barRoot == null)
            {
                barRoot = fillImage.transform.parent as RectTransform;
            }

            if (generatedBar)
            {
                CopyTextRectToBar(anchorText, barRoot, anchoredOffset, heightOverride);
            }
        }

        private Image FindExistingStatusFill(Text anchorText, string barName, ref RectTransform barRoot, Color fillColor)
        {
            Transform parent = anchorText.rectTransform.parent;
            if (parent == null)
            {
                return null;
            }

            Transform bar = parent.Find(barName);
            if (bar == null)
            {
                return null;
            }

            Transform fill = bar.Find(HpFillName);
            barRoot = bar as RectTransform;
            if (fill == null)
            {
                return null;
            }

            Image image = fill.GetComponent<Image>();
            if (image == null)
            {
                image = fill.gameObject.AddComponent<Image>();
            }

            ConfigureBarFill(image, fillColor);
            return image;
        }

        private static void ConfigureBarFill(Image image, Color fillColor)
        {
            if (image == null)
            {
                return;
            }

            image.color = fillColor;
            image.raycastTarget = false;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        private void SetBarFill(Image fillImage, float amount, bool fillFromRight = false)
        {
            if (fillImage == null)
            {
                return;
            }

            float safeAmount = Mathf.Clamp01(amount);
            fillImage.fillAmount = safeAmount;
            if (fillImage.type == Image.Type.Filled && fillImage.fillMethod == Image.FillMethod.Horizontal)
            {
                fillImage.fillOrigin = fillFromRight
                    ? (int)Image.OriginHorizontal.Right
                    : (int)Image.OriginHorizontal.Left;
            }

            RectTransform fillRect = fillImage.rectTransform;
            if (fillRect == null)
            {
                return;
            }

            if (fillImage.sprite == null)
            {
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.pivot = new Vector2(fillFromRight ? 1f : 0f, 0.5f);
                fillRect.localScale = new Vector3(safeAmount, 1f, 1f);
                return;
            }

            fillRect.localScale = Vector3.one;
        }

        private void SetEnemyEnergyBarVisible(bool visible)
        {
            GameObject barObject = enemyEnergyBarRoot != null
                ? enemyEnergyBarRoot.gameObject
                : enemyEnergyFillImage != null && enemyEnergyFillImage.transform.parent != null
                    ? enemyEnergyFillImage.transform.parent.gameObject
                    : null;

            if (barObject != null && barObject.activeSelf != visible)
            {
                barObject.SetActive(visible);
            }
        }

        private void CopyHpTextRectToBar()
        {
            if (hpText == null || hpBarRoot == null)
            {
                return;
            }

            RectTransform source = hpText.rectTransform;
            hpBarRoot.anchorMin = source.anchorMin;
            hpBarRoot.anchorMax = source.anchorMax;
            hpBarRoot.pivot = source.pivot;
            hpBarRoot.anchoredPosition = source.anchoredPosition;
            hpBarRoot.sizeDelta = source.sizeDelta;
            hpBarRoot.localScale = Vector3.one;
        }

        private static void CopyTextRectToBar(Text sourceText, RectTransform targetBar, Vector2 anchoredOffset, float heightOverride)
        {
            if (sourceText == null || targetBar == null)
            {
                return;
            }

            RectTransform source = sourceText.rectTransform;
            targetBar.anchorMin = source.anchorMin;
            targetBar.anchorMax = source.anchorMax;
            targetBar.pivot = source.pivot;
            targetBar.anchoredPosition = source.anchoredPosition + anchoredOffset;
            Vector2 size = source.sizeDelta;
            if (heightOverride > 0f)
            {
                size.y = heightOverride;
            }

            targetBar.sizeDelta = size;
            targetBar.localScale = Vector3.one;
        }

        private void SyncHpBarRect()
        {
            CopyHpTextRectToBar();
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

        public void FlashPlayerHit()
        {
            if (playerHitFeedback == null)
            {
                return;
            }

            playerHitFeedback.gameObject.SetActive(true);
            playerHitFeedback.enabled = true;
            playerHitFeedback.raycastTarget = false;
            playerHitFeedback.transform.SetAsLastSibling();

            if (playerHitRoutine != null)
            {
                StopCoroutine(playerHitRoutine);
            }

            playerHitRoutine = StartCoroutine(FadePlayerHit());
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

            if (enemyShakeRoutine != null)
            {
                StopCoroutine(enemyShakeRoutine);
                enemyVisual.anchoredPosition = enemyBaseAnchoredPosition;
            }

            enemyBaseAnchoredPosition = enemyVisual.anchoredPosition;
            hasEnemyBasePosition = true;
            enemyShakeRoutine = StartCoroutine(ShakeEnemyRoutine());
        }

        private IEnumerator FlashGraphic(Graphic graphic, Color baseColor, Color flashColor, float duration)
        {
            graphic.color = flashColor;
            yield return new WaitForSeconds(duration);
            graphic.color = baseColor;
        }

        private IEnumerator FadePlayerHit()
        {
            Color color = new Color(1f, 0.28f, 0.22f, 0.65f);
            playerHitFeedback.color = color;
            playerHitFeedback.SetVerticesDirty();
            playerHitFeedback.SetMaterialDirty();
            yield return new WaitForSeconds(0.28f);
            color = playerHitBaseColor;
            color.a = 0f;
            playerHitFeedback.color = color;
            playerHitFeedback.SetVerticesDirty();
            playerHitFeedback.SetMaterialDirty();
            playerHitRoutine = null;
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

        private static float GetEnemyEnergyProgress(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return 0f;
            }

            if (enemy.currentCastingSkill?.definition != null && enemy.currentCastingSkill.isCasting)
            {
                float duration = Mathf.Max(0.01f, enemy.currentCastingSkill.definition.castTime);
                return Mathf.Clamp01(1f - enemy.currentCastingSkill.castTimer / duration);
            }

            if (enemy.isCharging && enemy.definition != null && enemy.definition.chargeDuration > 0f)
            {
                float duration = Mathf.Max(0.01f, enemy.definition.chargeDuration);
                return Mathf.Clamp01(1f - enemy.chargeTimer / duration);
            }

            return 0f;
        }

        private static bool ShouldShowEnemyReadinessBar(EnemyRuntime enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            if (enemy.currentCastingSkill?.definition != null && enemy.currentCastingSkill.isCasting)
            {
                return true;
            }

            return enemy.isCharging && enemy.definition != null && enemy.definition.chargeDuration > 0f;
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
