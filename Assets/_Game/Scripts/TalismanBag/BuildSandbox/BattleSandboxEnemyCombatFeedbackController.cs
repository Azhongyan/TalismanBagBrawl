using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BattleSandboxEnemyCombatFeedbackController : MonoBehaviour
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool runsFormalCombat;
        [SerializeField] private bool callsFormalDamageSettlement;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalSaveData;
        [SerializeField] private bool grantsFormalReward;
        [SerializeField] private bool advancesChapter;
        [SerializeField] private bool opensFeatureFlag;
        [SerializeField] private bool showsCompleteAnswers;

        [SerializeField] private Text previewTitleText;
        [SerializeField] private Text bossStateText;
        [SerializeField] private Text bossSkillText;
        [SerializeField] private Text castTimerText;
        [SerializeField] private Image castFillImage;
        [SerializeField] private Text mechanicFloatingText;
        [SerializeField] private CanvasGroup mechanicFloatingCanvasGroup;
        [SerializeField] private Text combatLogText;
        [SerializeField] private Text controlStatusText;
        [SerializeField] private Button previousFeedbackButton;
        [SerializeField] private Button nextFeedbackButton;
        [SerializeField] private Button triggerFloatingButton;

        private readonly List<BattleSandboxEnemyCombatFeedbackRow> rows = new();
        private BattleSandboxBuildCombatPreview buildCombatPreview;
        private BattleSandboxEnemyCombatFeedbackPreview preview;
        private Vector2 floatingStartPosition;
        private float rowTimer;
        private float floatingTimer;
        private int rowIndex;
        private bool buttonsBound;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool RunsFormalCombat => runsFormalCombat;
        public bool CallsFormalDamageSettlement => callsFormalDamageSettlement;
        public bool WritesFormalFlow => writesFormalFlow;
        public bool WritesFormalSaveData => writesFormalSaveData;
        public bool GrantsFormalReward => grantsFormalReward;
        public bool AdvancesChapter => advancesChapter;
        public bool OpensFeatureFlag => opensFeatureFlag;
        public bool ShowsCompleteAnswers => showsCompleteAnswers;
        public int PlayerVisibleRowCount => rows.Count;
        public BattleSandboxBuildCombatPreview CurrentBuildCombatPreview => buildCombatPreview;

        public void Bind(
            Text title,
            Text state,
            Text skill,
            Text timer,
            Image castFill,
            Text floatingText,
            CanvasGroup floatingCanvasGroup,
            Text logText,
            Text statusText,
            Button previousButton,
            Button nextButton,
            Button triggerButton)
        {
            previewTitleText = title;
            bossStateText = state;
            bossSkillText = skill;
            castTimerText = timer;
            castFillImage = castFill;
            mechanicFloatingText = floatingText;
            mechanicFloatingCanvasGroup = floatingCanvasGroup;
            combatLogText = logText;
            controlStatusText = statusText;
            previousFeedbackButton = previousButton;
            nextFeedbackButton = nextButton;
            triggerFloatingButton = triggerButton;
            CacheFloatingStartPosition();
        }

        private void Awake()
        {
            InitializePreview();
            CacheFloatingStartPosition();
            BindButtons();
            ShowCurrent(restartFloating: true);
        }

        private void OnEnable()
        {
            InitializePreview();
            CacheFloatingStartPosition();
            BindButtons();
            ShowCurrent(restartFloating: true);
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        private void Update()
        {
            if (rows.Count == 0)
            {
                SetText(controlStatusText, "暂无战斗反馈行");
                return;
            }

            BattleSandboxEnemyCombatFeedbackRow row = CurrentRow();
            float duration = Mathf.Max(0.6f, row?.castDurationSeconds ?? 2.4f);
            rowTimer += Time.deltaTime;
            if (rowTimer >= duration)
            {
                rowIndex = (rowIndex + 1) % rows.Count;
                ShowCurrent(restartFloating: true);
                return;
            }

            RefreshCastTimer(duration);
            RefreshFloatingAnimation();
        }

        public void ShowPreviousFeedback()
        {
            if (rows.Count > 0)
            {
                rowIndex = (rowIndex - 1 + rows.Count) % rows.Count;
            }

            ShowCurrent(restartFloating: true);
        }

        public void ShowNextFeedback()
        {
            if (rows.Count > 0)
            {
                rowIndex = (rowIndex + 1) % rows.Count;
            }

            ShowCurrent(restartFloating: true);
        }

        public void TriggerFloatingFeedback()
        {
            floatingTimer = 0f;
            ShowFloatingText(CurrentRow());
        }

        public void RestartBattleModePreview()
        {
            InitializePreview(forceRebuild: true);
            CacheFloatingStartPosition();
            rowIndex = FindFirstCastBarRowIndex();
            ShowCurrent(restartFloating: true);
        }

        private void InitializePreview(bool forceRebuild = false)
        {
            if (preview != null && !forceRebuild)
            {
                return;
            }

            BuildGridInteractionPreviewController gridController =
                FindObjectOfType<BuildGridInteractionPreviewController>(true);
            buildCombatPreview =
                BattleSandboxBuildCombatPreviewBuilder.BuildFromCurrentBoard(gridController);
            preview = buildCombatPreview.feedbackPreview;

            rows.Clear();
            rows.AddRange((preview.rows ?? new List<BattleSandboxEnemyCombatFeedbackRow>())
                .Where(row => row != null && row.playerVisible));
            rowIndex = rows.Count == 0 ? 0 : Mathf.Clamp(rowIndex, 0, rows.Count - 1);
        }

        private int FindFirstCastBarRowIndex()
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] != null && rows[i].usesEnemyCastBarLanguage)
                {
                    return i;
                }
            }

            return 0;
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            if (previousFeedbackButton != null) previousFeedbackButton.onClick.AddListener(ShowPreviousFeedback);
            if (nextFeedbackButton != null) nextFeedbackButton.onClick.AddListener(ShowNextFeedback);
            if (triggerFloatingButton != null) triggerFloatingButton.onClick.AddListener(TriggerFloatingFeedback);
            buttonsBound = true;
        }

        private void UnbindButtons()
        {
            if (!buttonsBound)
            {
                return;
            }

            if (previousFeedbackButton != null) previousFeedbackButton.onClick.RemoveListener(ShowPreviousFeedback);
            if (nextFeedbackButton != null) nextFeedbackButton.onClick.RemoveListener(ShowNextFeedback);
            if (triggerFloatingButton != null) triggerFloatingButton.onClick.RemoveListener(TriggerFloatingFeedback);
            buttonsBound = false;
        }

        private void ShowCurrent(bool restartFloating)
        {
            BattleSandboxEnemyCombatFeedbackRow row = CurrentRow();
            rowTimer = 0f;
            if (row == null)
            {
                SetText(previewTitleText, "战斗反馈预览");
                SetText(bossStateText, "暂无首领状态");
                SetText(bossSkillText, "暂无施法预兆");
                SetText(castTimerText, string.Empty);
                SetText(combatLogText, string.Empty);
                SetText(controlStatusText, "暂无反馈");
                SetFill(0f, Color.white);
                ShowFloatingText(null);
                return;
            }

            SetText(previewTitleText, "战斗反馈预览");
            SetText(bossStateText, row.stateLineChinese);
            SetText(bossSkillText, row.castSkillLineChinese);
            SetText(combatLogText, row.combatLogLineChinese);
            SetText(controlStatusText, $"{rowIndex + 1}/{rows.Count}  只显示状态、施法、机制短句");
            SetFill(row.usesEnemyCastBarLanguage ? 1f : 0f, ColorForKind(row.feedbackKind));
            RefreshCastTimer(Mathf.Max(0.6f, row.castDurationSeconds));

            if (restartFloating)
            {
                floatingTimer = 0f;
                ShowFloatingText(row);
            }
        }

        private void RefreshCastTimer(float duration)
        {
            BattleSandboxEnemyCombatFeedbackRow row = CurrentRow();
            if (row == null || !row.usesEnemyCastBarLanguage)
            {
                SetText(castTimerText, string.Empty);
                SetFill(0f, ColorForKind(row?.feedbackKind));
                return;
            }

            float remaining = Mathf.Max(0f, duration - rowTimer);
            float fillAmount = Mathf.Clamp01(remaining / duration);
            SetText(castTimerText, $"{remaining:0.0}秒");
            SetFill(fillAmount, ColorForKind(row.feedbackKind));
        }

        private void RefreshFloatingAnimation()
        {
            if (mechanicFloatingText == null)
            {
                return;
            }

            floatingTimer += Time.deltaTime;
            float normalized = Mathf.Clamp01(floatingTimer / 0.8f);
            RectTransform rect = mechanicFloatingText.rectTransform;
            rect.anchoredPosition = floatingStartPosition + new Vector2(0f, normalized * 42f);
            if (mechanicFloatingCanvasGroup != null)
            {
                mechanicFloatingCanvasGroup.alpha = 1f - normalized;
            }

            if (floatingTimer > 1.3f)
            {
                floatingTimer = 0f;
                ShowFloatingText(CurrentRow());
            }
        }

        private void ShowFloatingText(BattleSandboxEnemyCombatFeedbackRow row)
        {
            if (mechanicFloatingText == null)
            {
                return;
            }

            mechanicFloatingText.text = row == null ? string.Empty : row.floatingTextChinese;
            mechanicFloatingText.color = ColorForKind(row?.feedbackKind);
            mechanicFloatingText.rectTransform.anchoredPosition = floatingStartPosition;
            if (mechanicFloatingCanvasGroup != null)
            {
                mechanicFloatingCanvasGroup.alpha = row == null ? 0f : 1f;
            }
        }

        private BattleSandboxEnemyCombatFeedbackRow CurrentRow()
        {
            if (rows.Count == 0)
            {
                return null;
            }

            return rows[Mathf.Clamp(rowIndex, 0, rows.Count - 1)];
        }

        private void CacheFloatingStartPosition()
        {
            if (mechanicFloatingText != null)
            {
                floatingStartPosition = mechanicFloatingText.rectTransform.anchoredPosition;
            }
        }

        private void SetFill(float amount, Color color)
        {
            if (castFillImage == null)
            {
                return;
            }

            castFillImage.fillAmount = amount;
            castFillImage.color = color;
        }

        private static Color ColorForKind(string kind)
        {
            return kind switch
            {
                BattleSandboxEnemyCombatFeedbackKinds.BossState => new Color(1f, 0.82f, 0.35f),
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast => new Color(1f, 0.48f, 0.30f),
                BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow => new Color(1f, 0.95f, 0.45f),
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback => new Color(1f, 0.35f, 0.35f),
                BattleSandboxEnemyCombatFeedbackKinds.EnemyState => new Color(0.86f, 0.94f, 1f),
                _ => new Color(0.72f, 1f, 0.42f)
            };
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
