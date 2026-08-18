using System;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.Items;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BattleBridge.ShougunuPhase1
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(650)]
    public sealed class ShougunuPhase1BattleSandboxSceneBinder :
        MonoBehaviour
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private BuildGridInteractionPreviewController
            gridController;
        [SerializeField] private BattleSandboxRuntimeLoopRuntime legacyRuntimeLoop;
        [SerializeField] private BattleSandboxManaLoopRuntime legacyManaLoop;
        [SerializeField] private BattleSandboxEnemyCombatFeedbackController
            enemyCombatFeedbackController;
        [SerializeField] private ShougunuPhase1BattleSandboxVerticalSliceRuntime
            battleRuntime;
        [SerializeField] private ShougunuPhase1BattleSandboxVisualCueAdapter
            visualCueAdapter;
        [SerializeField] private Text playerHpText;
        [SerializeField] private Image playerHpBar;
        [SerializeField] private Text enemyHpText;
        [SerializeField] private Image enemyHpBar;
        [SerializeField] private Text shellText;
        [SerializeField] private Text stateText;
        [SerializeField] private Button authoredResetButton;

        private bool generationActive;
        private bool legacyRuntimeEnabledBeforeGeneration;
        private bool legacyRuntimeStateCaptured;
        private bool legacyManaEnabledBeforeGeneration;
        private bool legacyManaStateCaptured;
        private bool shellAuthoredActive;
        private bool shellAuthoredStateCaptured;
        private bool resetButtonWired;
        private bool configurationErrorActive;
        private string configurationErrorDetail = string.Empty;
        private BattleSandboxNianResourcePresenter nianPresenter;
        private BattleSandboxAuthoredTmpPresentation
            authoredTmpPresentation;
        private BattleSandboxItemTriggerFeedbackController
            acceptedItemFeedbackController;
        private int generatedNpPresentationCount;
        private int spentNpPresentationCount;
        private bool ordinaryAdmissionActive;
        private bool ordinaryGenerationActive;
        private int ordinaryGeneration;
        private string ordinaryDisplayName = string.Empty;

        public bool DevOnly => devOnly;
        public bool WritesLayout => false;
        public BuildGridInteractionPreviewController GridController =>
            gridController;
        public BattleSandboxRuntimeLoopRuntime LegacyRuntimeLoop =>
            legacyRuntimeLoop;
        public BattleSandboxManaLoopRuntime LegacyManaLoop =>
            legacyManaLoop;
        public BattleSandboxEnemyCombatFeedbackController
            EnemyCombatFeedbackController => enemyCombatFeedbackController;
        public Text PlayerHpText => playerHpText;
        public Image PlayerHpBar => playerHpBar;
        public Text EnemyHpText => enemyHpText;
        public Image EnemyHpBar => enemyHpBar;
        public Text ShellText => shellText;
        public Text StateText => stateText;
        public Button AuthoredResetButton => authoredResetButton;
        public ShougunuPhase1BattleSandboxVisualCueAdapter VisualCueAdapter =>
            visualCueAdapter;
        public ShougunuPhase1BattleSandboxVerticalSliceRuntime BattleRuntime =>
            battleRuntime;
        public BattleSandboxNianResourcePresenter NianPresenter =>
            nianPresenter;
        public BattleSandboxAuthoredTmpPresentation
            AuthoredTmpPresentation => authoredTmpPresentation;
        public BattleSandboxItemTriggerFeedbackController
            AcceptedItemFeedbackController =>
                acceptedItemFeedbackController;
        public int GeneratedNpPresentationCount =>
            generatedNpPresentationCount;
        public int SpentNpPresentationCount =>
            spentNpPresentationCount;
        public bool OrdinaryAdmissionActive =>
            ordinaryAdmissionActive;
        public bool OrdinaryGenerationActive =>
            ordinaryGenerationActive;
        public int OrdinaryGeneration => ordinaryGeneration;

        public bool IsBindingComplete =>
            devOnly
            && gridController != null
            && enemyCombatFeedbackController != null
            && battleRuntime != null
            && visualCueAdapter != null
            && playerHpText != null
            && playerHpBar != null
            && enemyHpText != null
            && enemyHpBar != null
            && shellText != null
            && stateText != null
            && authoredResetButton != null;

        private void Awake()
        {
            PrepareVisibleDevContract();
            WireResetButton();
        }

        private void OnEnable()
        {
            PrepareVisibleDevContract();
            WireResetButton();
        }

        public void BindRuntime(
            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime)
        {
            battleRuntime = runtime;
            WireResetButton();
        }

        public void BeginGeneration(int resetGeneration)
        {
            ordinaryAdmissionActive = false;
            ordinaryGenerationActive = false;
            ordinaryGeneration = 0;
            ordinaryDisplayName = string.Empty;
            configurationErrorActive = false;
            configurationErrorDetail = string.Empty;
            generatedNpPresentationCount = 0;
            spentNpPresentationCount = 0;
            if (!generationActive)
            {
                generationActive = true;
            }

            SuppressLegacyRuntimeLoop();
            SuppressLegacyManaLoop();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(true);
            EnsurePresentation();
            authoredTmpPresentation?.BeginGeneration(resetGeneration);
            visualCueAdapter?.BindPresentation(
                authoredTmpPresentation);
            visualCueAdapter?.BeginGeneration(resetGeneration);
            EnsureNianPresenter();
            nianPresenter?.BindPresentation(
                authoredTmpPresentation);
            nianPresenter?.RestoreAuthoredState();
            ApplyInitialContractValues();
        }

        public void SetOrdinaryAdmissionPreview(
            C1EnemyRuntimeProfileSnapshot profile)
        {
            if (profile == null)
            {
                ReleaseOrdinaryAdmission();
                return;
            }

            if (ordinaryGenerationActive)
            {
                visualCueAdapter?.EndBattleMode();
                acceptedItemFeedbackController?.ClearAll();
                authoredTmpPresentation?.EndGeneration();
                ordinaryGenerationActive = false;
                ordinaryGeneration = 0;
            }
            ordinaryAdmissionActive = true;
            ordinaryDisplayName = profile.DisplayName;
            SuppressLegacyRuntimeLoop();
            SuppressLegacyManaLoop();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(true);
            ApplyOrdinaryValues(
                ordinaryDisplayName,
                profile.MaxHp,
                profile.MaxHp,
                profile.InitialShell,
                profile.ShellMax,
                V04Chapter1ContinuousBattleIntegrationContract
                    .PlayerMaxHpFixture,
                V04Chapter1ContinuousBattleIntegrationContract
                    .PlayerMaxHpFixture,
                profile.RuntimeProfileId,
                C1EnemyLifecycle.Present,
                0L,
                0);
        }

        public void BeginOrdinaryGeneration(
            C1EnemyRuntimeSnapshot enemy,
            int playerCurrentHp)
        {
            if (enemy == null || enemy.ResetGeneration <= 0)
            {
                return;
            }

            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.GetProfiles().FirstOrDefault(
                    value => string.Equals(
                        value.RuntimeProfileId,
                        enemy.RuntimeProfileId,
                        StringComparison.Ordinal));
            ordinaryAdmissionActive = true;
            ordinaryGenerationActive = true;
            ordinaryGeneration = enemy.ResetGeneration;
            ordinaryDisplayName = profile?.DisplayName
                ?? enemy.ContentId;
            configurationErrorActive = false;
            configurationErrorDetail = string.Empty;
            SuppressLegacyRuntimeLoop();
            SuppressLegacyManaLoop();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(true);
            EnsurePresentation();
            authoredTmpPresentation?.BeginGeneration(
                ordinaryGeneration);
            visualCueAdapter?.BindPresentation(
                authoredTmpPresentation);
            visualCueAdapter?.BeginGeneration(ordinaryGeneration);
            ApplyOrdinarySnapshot(enemy, playerCurrentHp, 0L);
        }

        public void ApplyOrdinarySnapshot(
            C1EnemyRuntimeSnapshot enemy,
            int playerCurrentHp,
            long battleTick)
        {
            if (!ordinaryAdmissionActive || enemy == null)
            {
                return;
            }
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.GetProfiles().FirstOrDefault(
                    value => string.Equals(
                        value.RuntimeProfileId,
                        enemy.RuntimeProfileId,
                        StringComparison.Ordinal));
            ordinaryDisplayName = profile?.DisplayName
                ?? ordinaryDisplayName;
            ApplyOrdinaryValues(
                ordinaryDisplayName,
                enemy.CurrentHp,
                enemy.MaxHp,
                enemy.CurrentShell,
                enemy.ShellMax,
                playerCurrentHp,
                V04Chapter1ContinuousBattleIntegrationContract
                    .PlayerMaxHpFixture,
                enemy.RuntimeProfileId,
                enemy.Lifecycle,
                battleTick,
                enemy.AcceptedApplicationCount);
        }

        public bool PresentAcceptedOrdinaryApplication(
            IBattleSandboxAcceptedDamagePresentation presentation)
        {
            if (!ordinaryGenerationActive
                || presentation == null
                || !presentation.IsAcceptedCorrelation
                || presentation.ResetGeneration != ordinaryGeneration
                || visualCueAdapter?.DamageDealtAnchor == null)
            {
                return false;
            }

            EnsurePresentation();
            if (authoredTmpPresentation == null
                || !authoredTmpPresentation.HasAuthoredTemplates
                || acceptedItemFeedbackController == null)
            {
                return false;
            }

            bool vfxAccepted =
                acceptedItemFeedbackController
                    .TryPlayAcceptedPresentation(
                        presentation.EventId,
                        presentation.SourceBaseItemId,
                        presentation.SourceItemInstanceId,
                        presentation.SourcePlacementId,
                        presentation.OccupiedCells,
                        presentation.ResolvedPreMitigationDamageUnits,
                        visualCueAdapter.DamageDealtAnchor,
                        out Vector3 sourceWorldPosition);
            if (!vfxAccepted)
            {
                return false;
            }

            bool sourceNumberAccepted =
                authoredTmpPresentation.TrySpawnAtWorldPosition(
                    presentation.ResetGeneration,
                    presentation.EventId,
                    "ordinary.item.source.resolved_damage",
                    sourceWorldPosition,
                    presentation.ResolvedPreMitigationDamageUnits
                        .ToString(CultureInfo.InvariantCulture),
                    BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                    0.74f,
                    0.04f,
                    BattleSandboxAuthoredTmpPresentation
                        .DefaultReadableLifetime,
                    presentation.SourceBaseItemId,
                    new Vector2(0f, 18f));
            bool shellOnly = presentation.ShellDamageApplied > 0
                && presentation.HpDamageApplied == 0;
            string unit = shellOnly
                ? " SH"
                : presentation.ShellDamageApplied > 0
                    ? " DMG"
                    : " HP";
            bool settlementAccepted =
                authoredTmpPresentation.TrySpawn(
                    presentation.ResetGeneration,
                    presentation.EventId,
                    "ordinary.item.enemy.settlement",
                    visualCueAdapter.DamageDealtAnchor,
                    "-"
                    + presentation.TotalDamageApplied.ToString(
                        CultureInfo.InvariantCulture)
                    + unit,
                    shellOnly
                        ? BattleSandboxAuthoredTmpStyle.SecondaryCyan
                        : BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                    1f,
                    0.32f,
                    BattleSandboxAuthoredTmpPresentation
                        .DefaultReadableLifetime,
                    shellOnly
                        ? BattleSandboxAuthoredTmpPresentation
                            .SettlementShellPaletteKey
                        : BattleSandboxAuthoredTmpPresentation
                            .SettlementHpPaletteKey);
            return sourceNumberAccepted
                && settlementAccepted
                && visualCueAdapter.RegisterAcceptedItemPresentation(
                    presentation.EventId);
        }

        public void ReleaseOrdinaryAdmission()
        {
            if (ordinaryGenerationActive)
            {
                visualCueAdapter?.EndBattleMode();
                acceptedItemFeedbackController?.ClearAll();
                authoredTmpPresentation?.EndGeneration();
            }
            ordinaryAdmissionActive = false;
            ordinaryGenerationActive = false;
            ordinaryGeneration = 0;
            ordinaryDisplayName = string.Empty;
            ApplyInitialContractValues();
            ApplyReadyInstruction();
        }

        public void BeginNianSession(
            BattleSandboxNianResourceSnapshot snapshot)
        {
            EnsureNianPresenter();
            EnsurePresentation();
            nianPresenter?.BeginGeneration(snapshot);
        }

        public void ApplyNianPulse(
            BattleSandboxNianResourceSnapshot snapshot,
            BattleSandboxNianResourceApplication application,
            int generatedAmount)
        {
            EnsureNianPresenter();
            EnsurePresentation();
            nianPresenter?.PresentPulse(
                snapshot,
                application,
                generatedAmount);
        }

        public bool PresentAcceptedItemApplication(
            BattleSandboxAcceptedItemPresentationEvent presentation)
        {
            if (presentation == null
                || !presentation.IsAcceptedCorrelation
                || battleRuntime == null
                || presentation.resetGeneration
                    != battleRuntime.ResetGeneration
                || visualCueAdapter?.DamageDealtAnchor == null)
            {
                return false;
            }

            EnsurePresentation();
            if (authoredTmpPresentation == null
                || !authoredTmpPresentation.HasAuthoredTemplates
                || acceptedItemFeedbackController == null)
            {
                return false;
            }

            bool vfxAccepted =
                acceptedItemFeedbackController
                    .TryPlayAcceptedPresentation(
                        presentation.pulseEventId,
                        presentation.sourceBaseItemId,
                        presentation.sourceItemInstanceId,
                        presentation.sourcePlacementId,
                        presentation.OccupiedCells,
                        presentation.resolvedPreMitigationDamageUnits,
                        visualCueAdapter.DamageDealtAnchor,
                        out Vector3 sourceWorldPosition);
            if (!vfxAccepted)
            {
                return false;
            }

            bool i031AnchorResolved =
                acceptedItemFeedbackController
                    .TryResolveBoardSourceWorldPosition(
                        I031InventoryPlacementContract.ItemId,
                        presentation.I031OccupiedCells,
                        out Vector3 i031WorldPosition);
            if (!i031AnchorResolved)
            {
                return false;
            }

            bool generatedNpAccepted =
                presentation.nianGeneratedDelta == 0
                || authoredTmpPresentation.TrySpawnAtWorldPosition(
                    presentation.resetGeneration,
                    presentation.pulseEventId,
                    "nian.generated.source",
                    i031WorldPosition,
                    "+"
                    + presentation.nianGeneratedDelta.ToString(
                        CultureInfo.InvariantCulture)
                    + " NP",
                    BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                    0.78f,
                    0f,
                    BattleSandboxAuthoredTmpPresentation
                        .DefaultReadableLifetime,
                    BattleSandboxAuthoredTmpPresentation
                        .NianGeneratedPaletteKey);
            bool sourceNumberAccepted =
                authoredTmpPresentation.TrySpawnAtWorldPosition(
                presentation.resetGeneration,
                presentation.pulseEventId,
                "item.source.resolved_damage",
                sourceWorldPosition,
                presentation.resolvedPreMitigationDamageUnits.ToString(
                    CultureInfo.InvariantCulture),
                BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                0.74f,
                0.04f,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime,
                presentation.sourceBaseItemId,
                new Vector2(0f, 18f));

            bool spentNpAccepted =
                authoredTmpPresentation.TrySpawnAtWorldPosition(
                    presentation.resetGeneration,
                    presentation.pulseEventId,
                    "nian.spent.source",
                    sourceWorldPosition,
                    "-"
                    + presentation.nianSpentDelta.ToString(
                        CultureInfo.InvariantCulture)
                    + " NP",
                    BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                    0.72f,
                    0.30f,
                    BattleSandboxAuthoredTmpPresentation
                        .DefaultReadableLifetime,
                    BattleSandboxAuthoredTmpPresentation
                        .NianSpentPaletteKey,
                    new Vector2(0f, -52f));
            bool shellSettlement =
                presentation.shellDamageApplied > 0;
            int appliedAmount = shellSettlement
                ? presentation.shellDamageApplied
                : presentation.hpDamageApplied;
            bool settlementAccepted =
                authoredTmpPresentation.TrySpawn(
                presentation.resetGeneration,
                presentation.pulseEventId,
                "item.enemy.settlement",
                visualCueAdapter.DamageDealtAnchor,
                "-"
                + appliedAmount.ToString(
                    CultureInfo.InvariantCulture)
                + (shellSettlement ? " SH" : " HP"),
                shellSettlement
                    ? BattleSandboxAuthoredTmpStyle.SecondaryCyan
                    : BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                1f,
                0.48f,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime,
                shellSettlement
                    ? BattleSandboxAuthoredTmpPresentation
                        .SettlementShellPaletteKey
                    : BattleSandboxAuthoredTmpPresentation
                        .SettlementHpPaletteKey);
            bool presentationAccepted =
                generatedNpAccepted
                && spentNpAccepted
                && sourceNumberAccepted
                && settlementAccepted
                && visualCueAdapter.RegisterAcceptedItemPresentation(
                    presentation.damageLedgerEventId);
            if (presentationAccepted)
            {
                if (presentation.nianGeneratedDelta > 0)
                {
                    generatedNpPresentationCount++;
                }
                spentNpPresentationCount++;
            }
            return presentationAccepted;
        }

        public void ApplyContext(
            ShougunuPhase1BattleApplicationContext context)
        {
            if (context?.enemy == null || context.player == null)
            {
                return;
            }

            BattleSandboxPlayerCombatantSnapshot player = context.player;
            ShougunuPhase1RuntimeSnapshot enemy = context.enemy;
            if (playerHpText != null)
            {
                playerHpText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "玩家 HP {0}/{1}",
                    player.currentHp,
                    player.maxHp);
            }
            if (playerHpBar != null)
            {
                playerHpBar.fillAmount = player.maxHp <= 0
                    ? 0f
                    : Mathf.Clamp01((float)player.currentHp / player.maxHp);
            }
            if (enemyHpText != null)
            {
                enemyHpText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "守骨奴 HP {0}/{1}",
                    enemy.CurrentHp,
                    enemy.MaxHp);
            }
            if (enemyHpBar != null)
            {
                enemyHpBar.fillAmount = enemy.MaxHp <= 0
                    ? 0f
                    : Mathf.Clamp01((float)enemy.CurrentHp / enemy.MaxHp);
            }
            if (shellText != null)
            {
                shellText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "护壳 {0}/{1} · 第 {2}/{3} 层",
                    enemy.CurrentShell,
                    enemy.ShellLayerMax,
                    enemy.ShellLayerIndex,
                    enemy.MaxSequentialShellLayers);
            }
            if (stateText != null)
            {
                int skillsResolved = enemy.ThresholdOccurrences.Count(value =>
                    value != null && value.Resolved);
                int shellBreaks = context.DisplayEvents.Count(value =>
                    value != null
                    && value.channel ==
                        ShougunuPhase1BattleDisplayChannel.EnemyShellBreak);
                BattleSandboxNianResourceSnapshot nian =
                    battleRuntime?.CurrentNianResourceSnapshot;
                stateText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0:0.0}s · 道具 {1}/50 · 技能 {2}/6 · 普攻 {3}/12 · 破壳 {4}/7 · 念力 {5}/{6} · R重置",
                    context.battleTick / 1000f,
                    enemy.AcceptedDamageApplicationCount,
                    skillsResolved,
                    enemy.BasicResolvedCount,
                    shellBreaks,
                    nian?.currentNian
                        ?? BattleSandboxNianResourceSnapshot.InitialNian,
                    nian?.maxNian
                        ?? BattleSandboxNianResourceSnapshot.MaxNian);
            }
        }

        public void RenderDisplayEvent(
            ShougunuPhase1BattleDisplayEvent displayEvent,
            int currentGeneration)
        {
            visualCueAdapter?.Render(displayEvent, currentGeneration);
        }

        public void ShowStartFacts(
            int requestCount,
            int telemetryCount,
            bool i031Owned,
            bool i031Placed)
        {
            visualCueAdapter?.ShowMajorMessage(
                "守骨奴第一阶段 · 实时道具链已接入",
                1.35f);
            if (stateText != null)
            {
                stateText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "I007-I012 请求 {0} · I031 {1}/{2} · 未执行遥测 {3} · R 重置",
                    requestCount,
                    i031Owned ? "已拥有" : "未拥有",
                    i031Placed ? "在盘" : "在栏",
                    telemetryCount);
            }
        }

        public void ShowNianResourceRejected(
            string diagnosticCode,
            string diagnosticDetail)
        {
            configurationErrorActive = false;
            configurationErrorDetail = diagnosticDetail ?? string.Empty;
            ApplyNianRejectionInstruction();
            visualCueAdapter?.ShowMajorMessage(
                diagnosticCode == "INSUFFICIENT_NIAN"
                    ? "念力不足 · 本次伤害已拒绝"
                    : "念力链校验失败 · 本次伤害已拒绝",
                2.2f);
            Debug.LogWarning(
                "["
                + ShougunuPhase1BattleSandboxVerticalSliceRuntime.PackageId
                + "][" + (diagnosticCode ?? "NIAN_RESOURCE_REJECTED")
                + "] " + (diagnosticDetail ?? string.Empty),
                this);
        }

        public void ShowConfigurationError(
            string diagnosticCode,
            string diagnosticDetail)
        {
            configurationErrorActive = true;
            configurationErrorDetail =
                string.IsNullOrWhiteSpace(diagnosticDetail)
                    ? "道具链未就绪"
                    : diagnosticDetail.Trim();
            ApplyInitialContractValues();
            ApplyConfigurationErrorInstruction();
            visualCueAdapter?.ShowMajorMessage(
                "道具链未就绪 · 返回整备后重开",
                2.4f);
            Debug.LogWarning(
                "["
                + ShougunuPhase1BattleSandboxVerticalSliceRuntime.PackageId
                + "][" + (diagnosticCode ?? "START_REJECTED")
                + "] Current Item authority was rejected closed. Detail="
                + (diagnosticDetail ?? string.Empty),
                this);
        }

        public void ShowSourceChanged()
        {
            configurationErrorActive = true;
            configurationErrorDetail =
                "阵容状态已变化，旧请求与表现已清空";
            ApplyConfigurationErrorInstruction();
            visualCueAdapter?.ShowMajorMessage(
                "旧世代已拒绝 · 请重新开战",
                2.2f);
        }

        public void ShowCompletion(
            ShougunuPhase1BattleApplicationContext context)
        {
            ApplyContext(context);
            visualCueAdapter?.ShowMajorMessage(
                "守骨奴第一阶段 · 击破",
                3f);
        }

        public void EndBattleMode()
        {
            configurationErrorActive = false;
            configurationErrorDetail = string.Empty;
            generatedNpPresentationCount = 0;
            spentNpPresentationCount = 0;
            visualCueAdapter?.EndBattleMode();
            acceptedItemFeedbackController?.ClearAll();
            authoredTmpPresentation?.EndGeneration();
            nianPresenter?.RestoreAuthoredState();
            if (generationActive)
            {
                generationActive = false;
            }
            ApplyInitialContractValues();
            ApplyReadyInstruction();
        }

        private void LateUpdate()
        {
            if (!devOnly)
            {
                return;
            }

            SuppressLegacyRuntimeLoop();
            SuppressLegacyManaLoop();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(true);
            if (ordinaryAdmissionActive)
            {
                return;
            }
            if (battleRuntime?.HasActiveSession == true
                && battleRuntime.CurrentContext != null)
            {
                ApplyContext(battleRuntime.CurrentContext);
                if (battleRuntime.NianResourceBlocked)
                {
                    ApplyNianRejectionInstruction();
                }
                return;
            }

            ApplyInitialContractValues();
            if (configurationErrorActive)
            {
                ApplyConfigurationErrorInstruction();
            }
            else
            {
                ApplyReadyInstruction();
            }
        }

        private void ApplyInitialContractValues()
        {
            if (playerHpText != null)
            {
                playerHpText.text = "玩家 HP 9999/9999";
            }
            if (playerHpBar != null)
            {
                playerHpBar.fillAmount = 1f;
            }
            if (enemyHpText != null)
            {
                enemyHpText.text = "守骨奴 HP 980/980";
            }
            if (enemyHpBar != null)
            {
                enemyHpBar.fillAmount = 1f;
            }
            if (shellText != null)
            {
                shellText.gameObject.SetActive(true);
                shellText.text = "护壳 200/200 · 第 1/7 层";
            }
        }

        private void ApplyOrdinaryValues(
            string displayName,
            int currentHp,
            int maxHp,
            int currentShell,
            int shellMax,
            int playerCurrentHp,
            int playerMaxHp,
            string runtimeProfileId,
            C1EnemyLifecycle lifecycle,
            long battleTick,
            int acceptedApplicationCount)
        {
            string stableName = string.IsNullOrWhiteSpace(displayName)
                ? "C1 Ordinary Enemy"
                : displayName.Trim();
            if (playerHpText != null)
            {
                playerHpText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "玩家 HP {0}/{1}",
                    playerCurrentHp,
                    playerMaxHp);
            }
            if (playerHpBar != null)
            {
                playerHpBar.fillAmount = playerMaxHp <= 0
                    ? 0f
                    : Mathf.Clamp01(
                        (float)playerCurrentHp / playerMaxHp);
            }
            if (enemyHpText != null)
            {
                enemyHpText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} HP {1}/{2}",
                    stableName,
                    currentHp,
                    maxHp);
            }
            if (enemyHpBar != null)
            {
                enemyHpBar.fillAmount = maxHp <= 0
                    ? 0f
                    : Mathf.Clamp01((float)currentHp / maxHp);
            }
            if (shellText != null)
            {
                shellText.gameObject.SetActive(true);
                shellText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "护壳 {0}/{1}",
                    currentShell,
                    shellMax);
            }
            if (stateText != null)
            {
                stateText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "C1普通遇敌 · {0} · {1:0.0}s · 道具结算 {2} · {3}",
                    runtimeProfileId ?? string.Empty,
                    battleTick / 1000f,
                    acceptedApplicationCount,
                    lifecycle);
            }
        }

        private void ApplyReadyInstruction()
        {
            if (stateText != null)
            {
                stateText.text =
                    "守骨奴 P1 · 点底栏「继续战斗」启动 · R 重置";
            }
        }

        private void ApplyConfigurationErrorInstruction()
        {
            if (stateText != null)
            {
                stateText.text = "未开战："
                    + (string.IsNullOrWhiteSpace(configurationErrorDetail)
                        ? "道具链未就绪"
                        : configurationErrorDetail)
                    + "；返回整备修正后再点「继续战斗」";
            }
        }

        private void ApplyNianRejectionInstruction()
        {
            if (stateText != null)
            {
                stateText.text = string.IsNullOrWhiteSpace(
                        configurationErrorDetail)
                    ? "念力链拒绝：本次伤害未发生；按 R 重置"
                    : configurationErrorDetail;
            }
        }

        private void PrepareVisibleDevContract()
        {
            if (!devOnly)
            {
                return;
            }
            SuppressLegacyRuntimeLoop();
            SuppressLegacyManaLoop();
            EnsureNianPresenter();
            EnsurePresentation();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(true);
            if (!shellAuthoredStateCaptured && shellText != null)
            {
                shellAuthoredActive = shellText.gameObject.activeSelf;
                shellAuthoredStateCaptured = true;
            }
            ApplyInitialContractValues();
            ApplyReadyInstruction();
        }

        private void SuppressLegacyRuntimeLoop()
        {
            if (legacyRuntimeLoop == null && gridController != null)
            {
                legacyRuntimeLoop =
                    gridController
                        .GetComponent<BattleSandboxRuntimeLoopRuntime>();
            }
            if (legacyRuntimeLoop == null)
            {
                return;
            }
            if (!legacyRuntimeStateCaptured)
            {
                legacyRuntimeEnabledBeforeGeneration =
                    legacyRuntimeLoop.enabled;
                legacyRuntimeStateCaptured = true;
            }
            if (legacyRuntimeLoop.enabled)
            {
                legacyRuntimeLoop.enabled = false;
                legacyRuntimeLoop.ResetLoop();
            }
        }

        private void SuppressLegacyManaLoop()
        {
            if (legacyManaLoop == null && gridController != null)
            {
                legacyManaLoop =
                    gridController
                        .GetComponent<BattleSandboxManaLoopRuntime>();
            }
            if (legacyManaLoop == null)
            {
                return;
            }
            if (!legacyManaStateCaptured)
            {
                legacyManaEnabledBeforeGeneration = legacyManaLoop.enabled;
                legacyManaStateCaptured = true;
            }
            if (legacyManaLoop.enabled)
            {
                legacyManaLoop.enabled = false;
                legacyManaLoop.ResetLoop();
            }
        }

        private void WireResetButton()
        {
            if (resetButtonWired || authoredResetButton == null)
            {
                return;
            }

            authoredResetButton.onClick.AddListener(HandleResetClicked);
            resetButtonWired = true;
        }

        private void EnsureNianPresenter()
        {
            if (!Application.isPlaying || nianPresenter != null)
            {
                return;
            }

            nianPresenter =
                GetComponent<BattleSandboxNianResourcePresenter>();
            if (nianPresenter == null)
            {
                nianPresenter =
                    gameObject.AddComponent<
                        BattleSandboxNianResourcePresenter>();
                nianPresenter.hideFlags =
                    HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild;
            }
        }

        private void EnsurePresentation()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (authoredTmpPresentation == null)
            {
                authoredTmpPresentation =
                    GetComponent<
                        BattleSandboxAuthoredTmpPresentation>();
                if (authoredTmpPresentation == null)
                {
                    authoredTmpPresentation =
                        gameObject.AddComponent<
                            BattleSandboxAuthoredTmpPresentation>();
                    authoredTmpPresentation.hideFlags =
                        HideFlags.DontSaveInEditor
                        | HideFlags.DontSaveInBuild;
                }
            }

            visualCueAdapter?.BindPresentation(
                authoredTmpPresentation);
            nianPresenter?.BindPresentation(
                authoredTmpPresentation);

            GameObject feedbackHost =
                legacyRuntimeLoop == null
                    ? gridController?.gameObject
                    : legacyRuntimeLoop.gameObject;
            if (acceptedItemFeedbackController == null
                && feedbackHost != null)
            {
                acceptedItemFeedbackController =
                    feedbackHost.GetComponent<
                        BattleSandboxItemTriggerFeedbackController>();
                if (acceptedItemFeedbackController == null)
                {
                    acceptedItemFeedbackController =
                        feedbackHost.AddComponent<
                            BattleSandboxItemTriggerFeedbackController>();
                    acceptedItemFeedbackController.hideFlags =
                        HideFlags.DontSaveInEditor
                        | HideFlags.DontSaveInBuild;
                }
            }

            acceptedItemFeedbackController?.Bind(
                gridController);
        }

        private void HandleResetClicked()
        {
            battleRuntime?.HandleAuthoredBattleStateButton();
        }

        private void OnDisable()
        {
            if (resetButtonWired && authoredResetButton != null)
            {
                authoredResetButton.onClick.RemoveListener(
                    HandleResetClicked);
            }
            resetButtonWired = false;
            configurationErrorActive = false;
            configurationErrorDetail = string.Empty;
            generatedNpPresentationCount = 0;
            spentNpPresentationCount = 0;
            ordinaryAdmissionActive = false;
            ordinaryGenerationActive = false;
            ordinaryGeneration = 0;
            ordinaryDisplayName = string.Empty;
            visualCueAdapter?.EndBattleMode();
            acceptedItemFeedbackController?.ClearAll();
            authoredTmpPresentation?.EndGeneration();
            nianPresenter?.RestoreAuthoredState();
            enemyCombatFeedbackController?.SetRuntimeLoopMode(false);
            generationActive = false;
            if (shellAuthoredStateCaptured && shellText != null)
            {
                shellText.gameObject.SetActive(shellAuthoredActive);
            }
            shellAuthoredStateCaptured = false;
            if (legacyRuntimeStateCaptured && legacyRuntimeLoop != null)
            {
                legacyRuntimeLoop.enabled = false;
            }
            legacyRuntimeStateCaptured = false;
            if (legacyManaStateCaptured && legacyManaLoop != null)
            {
                legacyManaLoop.enabled = false;
            }
            legacyManaStateCaptured = false;
        }
    }
}
