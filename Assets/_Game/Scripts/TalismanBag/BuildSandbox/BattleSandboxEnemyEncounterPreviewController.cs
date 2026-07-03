using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BattleSandboxEnemyEncounterPreviewController : MonoBehaviour
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool runsFormalCombat;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalSaveData;
        [SerializeField] private bool grantsFormalReward;
        [SerializeField] private bool showsCompleteAnswers;

        [SerializeField] private Text selectorTitle;
        [SerializeField] private Text enemyInfoBlock;
        [SerializeField] private Text mechanicHintBlock;
        [SerializeField] private Text weaknessWindowBlock;
        [SerializeField] private Text readinessPreviewBlock;
        [SerializeField] private Text failureFeedbackBlock;
        [SerializeField] private Text dropBiasHintBlock;
        [SerializeField] private Text testTargetBlock;
        [SerializeField] private Text isolationBlock;
        [SerializeField] private Button previousEnemyButton;
        [SerializeField] private Button nextEnemyButton;
        [SerializeField] private Button previousBossButton;
        [SerializeField] private Button nextBossButton;
        [SerializeField] private Button readinessPreviewButton;

        private BattleSandboxEnemyEncounterPreview preview;
        private readonly List<BattleSandboxEnemyEncounterRow> enemyRows = new();
        private readonly List<BattleSandboxEnemyEncounterRow> bossRows = new();
        private int enemyIndex;
        private int bossIndex;
        private bool showingBoss;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool RunsFormalCombat => runsFormalCombat;
        public bool WritesFormalFlow => writesFormalFlow;
        public bool WritesFormalSaveData => writesFormalSaveData;
        public bool GrantsFormalReward => grantsFormalReward;
        public bool ShowsCompleteAnswers => showsCompleteAnswers;
        public int EnemyOptionCount => enemyRows.Count;
        public int BossOptionCount => bossRows.Count;

        public void Bind(
            Text selector,
            Text info,
            Text mechanic,
            Text weakness,
            Text readiness,
            Text failure,
            Text dropBias,
            Text target,
            Text isolation,
            Button previousEnemy,
            Button nextEnemy,
            Button previousBoss,
            Button nextBoss,
            Button readinessButton)
        {
            selectorTitle = selector;
            enemyInfoBlock = info;
            mechanicHintBlock = mechanic;
            weaknessWindowBlock = weakness;
            readinessPreviewBlock = readiness;
            failureFeedbackBlock = failure;
            dropBiasHintBlock = dropBias;
            testTargetBlock = target;
            isolationBlock = isolation;
            previousEnemyButton = previousEnemy;
            nextEnemyButton = nextEnemy;
            previousBossButton = previousBoss;
            nextBossButton = nextBoss;
            readinessPreviewButton = readinessButton;
        }

        private void Awake()
        {
            InitializePreview();
            BindButtons();
            ShowCurrent();
        }

        private void OnEnable()
        {
            InitializePreview();
            BindButtons();
            ShowCurrent();
        }

        private void OnDisable()
        {
            UnbindButtons();
        }

        public void ShowNextEnemy()
        {
            showingBoss = false;
            if (enemyRows.Count > 0)
            {
                enemyIndex = (enemyIndex + 1) % enemyRows.Count;
            }

            ShowCurrent();
        }

        public void ShowPreviousEnemy()
        {
            showingBoss = false;
            if (enemyRows.Count > 0)
            {
                enemyIndex = (enemyIndex - 1 + enemyRows.Count) % enemyRows.Count;
            }

            ShowCurrent();
        }

        public void ShowNextBoss()
        {
            showingBoss = true;
            if (bossRows.Count > 0)
            {
                bossIndex = (bossIndex + 1) % bossRows.Count;
            }

            ShowCurrent();
        }

        public void ShowPreviousBoss()
        {
            showingBoss = true;
            if (bossRows.Count > 0)
            {
                bossIndex = (bossIndex - 1 + bossRows.Count) % bossRows.Count;
            }

            ShowCurrent();
        }

        public void RefreshReadinessPreview()
        {
            BattleSandboxEnemyEncounterRow row = CurrentRow();
            if (row == null || readinessPreviewBlock == null)
            {
                return;
            }

            readinessPreviewBlock.text = row.readinessPreviewChinese;
        }

        private void InitializePreview()
        {
            if (preview != null)
            {
                return;
            }

            BuildSandboxPreviewContext context = BuildSandboxPreviewContextBuilder.Build(
                new BuildSandboxPreviewContextBuildInput());
            preview = BattleSandboxEnemyEncounterPreviewBuilder.Build(context);
            enemyRows.Clear();
            bossRows.Clear();
            enemyRows.AddRange((preview.rows ?? new List<BattleSandboxEnemyEncounterRow>())
                .Where(row => row != null && row.encounterKind == "enemy"));
            bossRows.AddRange((preview.rows ?? new List<BattleSandboxEnemyEncounterRow>())
                .Where(row => row != null && row.encounterKind == "boss"));
        }

        private void BindButtons()
        {
            UnbindButtons();
            if (previousEnemyButton != null) previousEnemyButton.onClick.AddListener(ShowPreviousEnemy);
            if (nextEnemyButton != null) nextEnemyButton.onClick.AddListener(ShowNextEnemy);
            if (previousBossButton != null) previousBossButton.onClick.AddListener(ShowPreviousBoss);
            if (nextBossButton != null) nextBossButton.onClick.AddListener(ShowNextBoss);
            if (readinessPreviewButton != null) readinessPreviewButton.onClick.AddListener(RefreshReadinessPreview);
        }

        private void UnbindButtons()
        {
            if (previousEnemyButton != null) previousEnemyButton.onClick.RemoveListener(ShowPreviousEnemy);
            if (nextEnemyButton != null) nextEnemyButton.onClick.RemoveListener(ShowNextEnemy);
            if (previousBossButton != null) previousBossButton.onClick.RemoveListener(ShowPreviousBoss);
            if (nextBossButton != null) nextBossButton.onClick.RemoveListener(ShowNextBoss);
            if (readinessPreviewButton != null) readinessPreviewButton.onClick.RemoveListener(RefreshReadinessPreview);
        }

        private void ShowCurrent()
        {
            BattleSandboxEnemyEncounterRow row = CurrentRow();
            if (row == null)
            {
                SetText(selectorTitle, "敌人与首领题目预览");
                SetText(enemyInfoBlock, "暂无开发专用题目。");
                return;
            }

            SetText(selectorTitle, row.selectorLabel);
            SetText(enemyInfoBlock, $"当前题目：{row.chineseDisplayName}");
            SetText(mechanicHintBlock, $"{row.mapMechanicChinese}\n{row.encounterMechanicChinese}\n{row.bossSkillChinese}");
            SetText(weaknessWindowBlock, row.weaknessWindowChinese);
            SetText(readinessPreviewBlock, row.readinessPreviewChinese);
            SetText(failureFeedbackBlock, row.failureFeedbackChinese);
            SetText(dropBiasHintBlock, row.dropBiasAtmosphereChinese);
            SetText(testTargetBlock, row.testTargetChinese);
            SetText(isolationBlock, "开发专用预览：不触发正式战斗、不发奖励、不写存档、不推进章节。");
        }

        private BattleSandboxEnemyEncounterRow CurrentRow()
        {
            if (showingBoss)
            {
                return bossRows.Count == 0 ? null : bossRows[Mathf.Clamp(bossIndex, 0, bossRows.Count - 1)];
            }

            return enemyRows.Count == 0 ? null : enemyRows[Mathf.Clamp(enemyIndex, 0, enemyRows.Count - 1)];
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
