using System.Collections;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TalismanBag.UI
{
    public sealed class TalismanCombatUI : MonoBehaviour
    {
        private const string HpBarName = "PlayerHPBar";
        private const string HpFillName = "Fill";

        [Header("Texts")]
        [SerializeField] private Text hpText;
        [SerializeField] private Text shieldText;
        [SerializeField] private Text manaText;
        [SerializeField] private Text enemyFaceText;
        [SerializeField] private Text enemyTitleText;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Text stateText;
        [SerializeField] private Image hpFillImage;

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
        private RectTransform hpBarRoot;
        private bool generatedHpBar;
        private Vector2 enemyBaseAnchoredPosition;
        private bool hasEnemyBasePosition;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorApplication.delayCall -= EnsureHpBarInEditor;
            EditorApplication.delayCall += EnsureHpBarInEditor;
        }

        private void EnsureHpBarInEditor()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            // Manual UI targets must exist before Play and keep designer-placed rects.
            EnsureHpBar(false);
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif

        private void Awake()
        {
            EnsureHpBar(Application.isPlaying);

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
            EnsureHpBar(Application.isPlaying);
            if (player != null)
            {
                if (hpText != null)
                {
                    hpText.text = $"气血 {player.hp}/{player.maxHP}";
                }

                if (hpFillImage != null)
                {
                    SetHpFill(player.maxHP > 0 ? player.hp / (float)player.maxHP : 0f);
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
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(barObject, "Create player HP bar");
                }
#endif
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
                hpFillImage.color = new Color(0.86f, 0.08f, 0.06f, 0.95f);
                hpFillImage.raycastTarget = false;
                hpFillImage.type = Image.Type.Filled;
                hpFillImage.fillMethod = Image.FillMethod.Horizontal;
                hpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                SetHpFill(1f);
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
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(fillObject, "Create player HP fill");
                }
#endif
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

            fillImage.color = new Color(0.86f, 0.08f, 0.06f, 0.95f);
            fillImage.raycastTarget = false;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            hpFillImage = fillImage;
            SetHpFill(1f);
            return hpFillImage;
        }

        private void SetHpFill(float amount)
        {
            if (hpFillImage == null)
            {
                return;
            }

            float safeAmount = Mathf.Clamp01(amount);
            hpFillImage.fillAmount = safeAmount;

            RectTransform fillRect = hpFillImage.rectTransform;
            if (fillRect == null)
            {
                return;
            }

            if (hpFillImage.sprite == null)
            {
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.pivot = new Vector2(0f, 0.5f);
                fillRect.localScale = new Vector3(safeAmount, 1f, 1f);
                return;
            }

            fillRect.localScale = Vector3.one;
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
