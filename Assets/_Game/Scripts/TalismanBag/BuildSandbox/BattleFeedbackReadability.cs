using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleFeedbackReadabilityPreview
    {
        public const string PackageName = "V0.4-BattleFeedbackReadability01";
        public const string PreviewBuildId = "battle_feedback_readability_preview";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = PreviewBuildId;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsCurrentBoardSnapshot = true;
        public bool readsBuildCombatPreview = true;
        public bool readsRuntimeLoopPreview = true;
        public bool readsFormationEnergyContract = true;
        public bool readsLegacyItemBehavior = true;
        public bool writesExistingFeedbackText = true;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public bool writesSceneLayout;
        public bool playerUiShowsFullAnswers;
        public int sourceBuildCombatPreviewRowCount;
        public int sourceRuntimeLoopRowCount;
        public List<BattleFeedbackReadabilityRow> rows = new();
        public List<BattleFeedbackChannelMapRow> channelMap = new();

        public int RowCount => rows?.Count(row => row != null) ?? 0;
        public int ChannelMapRowCount => channelMap?.Count(row => row != null) ?? 0;
        public int PlayerTextLeakCount => BattleFeedbackReadabilityBuilder.CountPlayerTextLeaks(this);
        public int FormalFlowLeakCount => rows?.Count(row => row != null && row.FormalLeak) ?? 1;
        public int FeatureFlagDefaultTrueCount => BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        public int TotalLeakCount => PlayerTextLeakCount + FormalFlowLeakCount + FeatureFlagDefaultTrueCount + ScopeLeakCount;
        public int ScopeLeakCount => CountScopeLeaks();
        public bool LeakCheckPass => TotalLeakCount == 0;
        public bool DevOnlyIsolationPass => ScopeLeakCount == 0;

        public int EnergyStateCoverageCount => rows?
            .Where(row => row != null && string.Equals(row.category, BattleFeedbackReadabilityCategories.EnergyFeedback, StringComparison.Ordinal))
            .Select(row => row.energyState)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .Count() ?? 0;

        public bool CoversEnergyNone => CoversEnergyState(nameof(EnergyState.None));
        public bool CoversEnergyWeakPulse => CoversEnergyState(nameof(EnergyState.WeakPulse));
        public bool CoversEnergyPowered => CoversEnergyState(nameof(EnergyState.Powered));
        public bool CoversEnergySuppressed => CoversEnergyState(nameof(EnergyState.Suppressed));

        public int LegacyItemTriggerFeedbackCount => rows?
            .Count(row => row != null && row.legacyItemBehavior && row.playerVisible) ?? 0;

        public bool HasDamageFeedback => HasTriggerFamily("damage");
        public bool HasShieldFeedback => HasTriggerFamily("shield");
        public bool HasHealFeedback => HasTriggerFamily("heal");
        public bool HasCleanseFeedback => HasTriggerFamily("cleanse");
        public bool HasControlFeedback => HasTriggerFamily("control");
        public bool HasBossCastFeedback => rows?.Any(row => row != null && row.feedbackKey.StartsWith("bossCast", StringComparison.Ordinal)) ?? false;
        public bool HasMechanicFloatingFeedback => rows?.Any(row => row != null && row.usesFloatingFeedback) ?? false;
        public bool HasBuildFuzzyFeedback => rows?.Any(row => row != null && string.Equals(row.category, BattleFeedbackReadabilityCategories.BuildFeedback, StringComparison.Ordinal) && row.fuzzyHint) ?? false;
        public bool HasFailureFuzzyHint => rows?.Any(row => row != null && string.Equals(row.category, BattleFeedbackReadabilityCategories.FailureHint, StringComparison.Ordinal) && row.fuzzyHint) ?? false;

        public IReadOnlyDictionary<string, int> CategoryCounts =>
            (rows ?? new List<BattleFeedbackReadabilityRow>())
            .Where(row => row != null)
            .GroupBy(row => row.category ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        private bool CoversEnergyState(string state)
        {
            return rows?.Any(row =>
                row != null
                && string.Equals(row.category, BattleFeedbackReadabilityCategories.EnergyFeedback, StringComparison.Ordinal)
                && string.Equals(row.energyState, state, StringComparison.Ordinal)) ?? false;
        }

        private bool HasTriggerFamily(string family)
        {
            return rows?.Any(row =>
                row != null
                && string.Equals(row.category, BattleFeedbackReadabilityCategories.ItemTriggerFeedback, StringComparison.Ordinal)
                && string.Equals(row.triggerFamily, family, StringComparison.Ordinal)) ?? false;
        }

        private int CountScopeLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!readsCurrentBoardSnapshot) leaks++;
            if (!readsBuildCombatPreview) leaks++;
            if (!readsRuntimeLoopPreview) leaks++;
            if (!readsFormationEnergyContract) leaks++;
            if (!readsLegacyItemBehavior) leaks++;
            if (!writesExistingFeedbackText) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (writesFormalReward) leaks++;
            if (advancesChapter) leaks++;
            if (opensFeatureFlag) leaks++;
            if (writesSceneLayout) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            return leaks;
        }
    }

    [Serializable]
    public sealed class BattleFeedbackReadabilityRow
    {
        public string category = string.Empty;
        public string feedbackKey = string.Empty;
        public string feedbackKind = string.Empty;
        public string energyState = string.Empty;
        public string triggerFamily = string.Empty;
        public string playerTextChinese = string.Empty;
        public string stateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string reuseChannel = string.Empty;
        public string playerSurface = string.Empty;
        public string sourceDataPath = string.Empty;
        public string developerDataPanelFieldKey = string.Empty;
        public bool playerVisible = true;
        public bool developerVisible = true;
        public bool developerFieldsHiddenFromPlayer = true;
        public bool fuzzyHint = true;
        public bool legacyItemBehavior;
        public bool usesBossStateText;
        public bool usesCastBar;
        public bool usesFloatingFeedback = true;
        public bool usesCombatLog = true;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerShowsCompleteAnswer;
        public bool writesFormalUi;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public bool writesSceneLayout;

        public bool PlayerTextLeak => BattleFeedbackReadabilityBuilder.CountPlayerTextLeaks(this) > 0;

        public bool FormalLeak =>
            !devOnly
            || isEnabled
            || playerShowsCompleteAnswer
            || writesFormalUi
            || writesFormalFlow
            || writesFormalSaveData
            || writesFormalReward
            || advancesChapter
            || opensFeatureFlag
            || writesSceneLayout;
    }

    [Serializable]
    public sealed class BattleFeedbackChannelMapRow
    {
        public string feedbackKey = string.Empty;
        public string category = string.Empty;
        public string feedbackKind = string.Empty;
        public string reuseChannel = string.Empty;
        public string playerSurface = string.Empty;
        public string sourceDataPath = string.Empty;
        public string developerDataPanelFieldKey = string.Empty;
        public bool playerVisible = true;
        public bool developerVisible = true;
        public bool developerFieldsHiddenFromPlayer = true;
        public bool writesFormalUi;
        public bool writesFormalFlow;
        public bool writesSceneLayout;
    }

    public static class BattleFeedbackReadabilityCategories
    {
        public const string EnergyFeedback = "EnergyFeedback";
        public const string ItemTriggerFeedback = "ItemTriggerFeedback";
        public const string BossFeedback = "BossFeedback";
        public const string BuildFeedback = "BuildFeedback";
        public const string FailureHint = "FailureHint";
    }

    public static class BattleFeedbackReadabilityBuilder
    {
        public const string PackageName = BattleFeedbackReadabilityPreview.PackageName;
        public const string PreviewBuildId = BattleFeedbackReadabilityPreview.PreviewBuildId;

        private const string SourceReadability = "BattleFeedbackReadabilityPreview";

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "dropBias",
            "dropBiasWeights",
            "bossSixKeyFullAnswer",
            "keyRequirements",
            "previewWeight",
            "BuildProblemSeedDataset",
            "EnemyBossValidationPool",
            "Build",
            "Boss",
            "WeakPulse",
            "Powered",
            "Suppressed",
            "None",
            "solution",
            "required",
            "硬解",
            "答案清单",
            "权重",
            "六钥匙"
        };

        public static BattleFeedbackReadabilityPreview BuildDefaultPreview()
        {
            BuildSandboxLayoutSnapshot snapshot =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();
            return Build(snapshot, PreviewBuildId);
        }

        public static BattleFeedbackReadabilityPreview Build(
            BuildSandboxLayoutSnapshot snapshot,
            string sourcePreviewBuildId = PreviewBuildId)
        {
            BuildSandboxLayoutSnapshot safeSnapshot = snapshot
                ?? BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();
            BattleSandboxBuildCombatPreview buildCombatPreview =
                BattleSandboxBuildCombatPreviewBuilder.Build(
                    safeSnapshot,
                    string.IsNullOrWhiteSpace(sourcePreviewBuildId)
                        ? PreviewBuildId
                        : sourcePreviewBuildId,
                    usesCurrentBoardSnapshot: true);
            BattleSandboxRuntimeLoopPreview runtimeLoopPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    safeSnapshot,
                    string.IsNullOrWhiteSpace(sourcePreviewBuildId)
                        ? PreviewBuildId
                        : sourcePreviewBuildId,
                    BattleSandboxRuntimeLoopPreviewBuilder.ResolveDevEnemyScenario(0),
                    allowDefaultLayoutFallbackWhenBoardEmpty: true);

            List<BattleFeedbackReadabilityRow> rows = new();
            AddEnergyRows(rows);
            AddItemTriggerRows(rows);
            AddBossRows(rows);
            AddBuildRows(rows);
            AddFailureRows(rows);

            return new BattleFeedbackReadabilityPreview
            {
                packageName = PackageName,
                sourcePreviewBuildId = string.IsNullOrWhiteSpace(sourcePreviewBuildId)
                    ? PreviewBuildId
                    : sourcePreviewBuildId,
                devOnly = true,
                isEnabled = false,
                readsCurrentBoardSnapshot = true,
                readsBuildCombatPreview = true,
                readsRuntimeLoopPreview = true,
                readsFormationEnergyContract = true,
                readsLegacyItemBehavior = true,
                writesExistingFeedbackText = true,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false,
                writesSceneLayout = false,
                playerUiShowsFullAnswers = false,
                sourceBuildCombatPreviewRowCount = buildCombatPreview?.rows?.Count ?? 0,
                sourceRuntimeLoopRowCount = runtimeLoopPreview?.rows?.Count ?? 0,
                rows = rows,
                channelMap = BuildChannelMap(rows)
            };
        }

        public static int CountPlayerTextLeaks(BattleFeedbackReadabilityPreview preview)
        {
            return (preview?.rows ?? new List<BattleFeedbackReadabilityRow>())
                .Where(row => row != null)
                .Sum(CountPlayerTextLeaks);
        }

        public static int CountPlayerTextLeaks(BattleFeedbackReadabilityRow row)
        {
            int leaks = 0;
            foreach (string value in PlayerTextFields(row))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    leaks++;
                    continue;
                }

                if (!ContainsNonAscii(value) || ContainsLatin(value))
                {
                    leaks++;
                }

                if (ForbiddenPlayerAnswerTokens.Any(token =>
                        value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    leaks++;
                }
            }

            return leaks;
        }

        public static IEnumerable<string> PlayerTextFields(BattleFeedbackReadabilityRow row)
        {
            if (row == null)
            {
                yield break;
            }

            yield return row.playerTextChinese;
            yield return row.stateLineChinese;
            yield return row.castSkillLineChinese;
            yield return row.floatingTextChinese;
            yield return row.combatLogLineChinese;
        }

        public static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        public static bool ContainsNonAscii(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character > 127);
        }

        private static void AddEnergyRows(ICollection<BattleFeedbackReadabilityRow> rows)
        {
            rows.Add(Row(
                BattleFeedbackReadabilityCategories.EnergyFeedback,
                "energy.none",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "供能断开，道具沉寂。",
                "机制反馈：供能断开",
                "灵力未起",
                "道具沉寂",
                "【供能】供能断开，道具沉寂。",
                "BattleHint",
                "CombatLogText",
                SourceReadability + ".energy.none",
                "formationCorePowerRange",
                energyState: nameof(EnergyState.None),
                usesBossStateText: false,
                usesCastBar: false));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.EnergyFeedback,
                "energy.weakPulse",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "符阵微亮，效果较弱。",
                "机制反馈：阵眼微亮",
                "灵力微动",
                "符阵微亮",
                "【供能】符阵微亮，效果较弱。",
                "FloatingCombatText",
                "MechanicFloatingText",
                SourceReadability + ".energy.weakPulse",
                "formationCorePowerRange",
                energyState: nameof(EnergyState.WeakPulse),
                usesBossStateText: false,
                usesCastBar: false));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.EnergyFeedback,
                "energy.powered",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "聚能石连通，符阵供能稳定。",
                "机制反馈：供能稳定",
                "符阵供能稳定",
                "灵力扩散",
                "【供能】聚能石连通，符阵供能稳定。",
                "FloatingCombatText",
                "MechanicFloatingText",
                SourceReadability + ".energy.powered",
                "formationCorePowerRange",
                energyState: nameof(EnergyState.Powered),
                usesBossStateText: false,
                usesCastBar: false));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.EnergyFeedback,
                "energy.suppressed",
                BattleSandboxEnemyCombatFeedbackKinds.BossState,
                "灵力被压住，道具暂不回应。",
                "机制反馈：阵势受压",
                "灵力被压住",
                "道具无光",
                "【供能】灵力被压住，道具暂不回应。",
                "BossInfoPanel",
                "BossStateText",
                SourceReadability + ".energy.suppressed",
                "formationCorePowerRange",
                energyState: nameof(EnergyState.Suppressed),
                usesBossStateText: true,
                usesCastBar: false));
        }

        private static void AddItemTriggerRows(ICollection<BattleFeedbackReadabilityRow> rows)
        {
            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.damage",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "火符燃起，造成火焰伤害。",
                "道具反馈：火符燃起",
                "火焰回响",
                "火符燃起",
                "【道具】火符燃起，造成火焰伤害。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".legacy.damage",
                "legacyItemBehavior",
                triggerFamily: "damage",
                legacyItemBehavior: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.shield",
                BattleSandboxEnemyCombatFeedbackKinds.BossState,
                "护身符撑开，护盾回稳。",
                "玩家：护盾回稳",
                "护阵撑开",
                "护盾回稳",
                "【道具】护身符撑开，护盾回稳。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".legacy.shield",
                "legacyItemBehavior",
                triggerFamily: "shield",
                legacyItemBehavior: true,
                usesBossStateText: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.heal",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "丹药化开，气血回稳。",
                "玩家：气血回稳",
                "回气入身",
                "气血回稳",
                "【道具】丹药化开，气血回稳。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".itemStat.heal",
                "itemStatCombatPreview",
                triggerFamily: "heal",
                legacyItemBehavior: false));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.cleanse",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "净化符亮起，污痕退散。",
                "机制反馈：污痕退散",
                "净化回响",
                "污痕退散",
                "【道具】净化符亮起，污痕退散。",
                "FloatingCombatText",
                "MechanicFloatingText",
                SourceReadability + ".legacy.cleanse",
                "legacyItemBehavior",
                triggerFamily: "cleanse",
                legacyItemBehavior: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.control",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "镇魂符压住节奏。",
                "机制反馈：节奏被压住",
                "镇魂回响",
                "节奏放缓",
                "【道具】镇魂符压住节奏。",
                "FloatingCombatText",
                "MechanicFloatingText",
                SourceReadability + ".legacy.control",
                "legacyItemBehavior",
                triggerFamily: "control",
                legacyItemBehavior: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.ItemTriggerFeedback,
                "itemTrigger.rhythm",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "醒符香散开，节奏略微加快。",
                "道具反馈：节奏回响",
                "香气散开",
                "节奏微快",
                "【道具】醒符香散开，节奏略微加快。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".legacy.rhythm",
                "legacyItemBehavior",
                triggerFamily: "rhythm",
                legacyItemBehavior: true));
        }

        private static void AddBossRows(ICollection<BattleFeedbackReadabilityRow> rows)
        {
            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BossFeedback,
                "bossCast.start",
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                "首领开始蓄力，施法条亮起。",
                "首领：开始蓄力",
                "施法条亮起",
                "施法预兆",
                "【施法】首领开始蓄力，施法条亮起。",
                "CastBar",
                "BossCastBar",
                SourceReadability + ".bossCast.start",
                "bossCastState",
                usesBossStateText: true,
                usesCastBar: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BossFeedback,
                "bossCast.charging",
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                "首领正在蓄力。",
                "首领：持续蓄力",
                "施法中",
                "蓄力中",
                "【施法】首领正在蓄力。",
                "CastBar",
                "BossCastBar",
                SourceReadability + ".bossCast.charging",
                "bossCastState",
                usesBossStateText: true,
                usesCastBar: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BossFeedback,
                "bossCast.release",
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                "首领释放冲击，阵面承压。",
                "首领：冲击落下",
                "施法释放",
                "阵面承压",
                "【施法】首领释放冲击，阵面承压。",
                "CastBar",
                "BossCastBar",
                SourceReadability + ".bossCast.release",
                "bossCastState",
                usesBossStateText: true,
                usesCastBar: true));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BossFeedback,
                "boss.weaknessWindow",
                BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                "首领露出短暂破绽。",
                "首领：短暂破绽",
                "破绽窗口",
                "护势松动",
                "【破绽】首领露出短暂破绽。",
                "DamageFeedback",
                "MechanicFloatingText",
                SourceReadability + ".boss.weaknessWindow",
                "bossWeaknessWindow",
                usesBossStateText: true,
                usesCastBar: false));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BossFeedback,
                "boss.pressure",
                BattleSandboxEnemyCombatFeedbackKinds.EnemyState,
                "首领攻势压近边线。",
                "首领：攻势压近",
                "压力升高",
                "边线承压",
                "【压力】首领攻势压近边线。",
                "BossInfoPanel",
                "BossStateText",
                SourceReadability + ".boss.pressure",
                "bossPressure",
                usesBossStateText: true,
                usesCastBar: false));
        }

        private static void AddBuildRows(ICollection<BattleFeedbackReadabilityRow> rows)
        {
            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BuildFeedback,
                "build.effective",
                BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                "这套阵势已有稳定回应。",
                "阵势反馈：回应稳定",
                "阵势成形",
                "回应稳定",
                "【阵势】这套阵势已有稳定回应。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".build.effective",
                "maskedBuildReadiness"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BuildFeedback,
                "build.partial",
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "这套阵势有回应，但节奏不稳。",
                "阵势反馈：节奏不稳",
                "阵势半亮",
                "节奏不稳",
                "【阵势】这套阵势有回应，但节奏不稳。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".build.partial",
                "maskedBuildReadiness"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BuildFeedback,
                "build.missingCounter",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "这套阵势缺少破盾手段。",
                "阵势反馈：破盾不足",
                "护势难退",
                "护盾仍厚",
                "【阵势】这套阵势缺少破盾手段。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".build.missingCounter",
                "maskedBuildGap"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BuildFeedback,
                "build.overloaded",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "阵面过满，关键道具容易沉寂。",
                "阵势反馈：阵面过满",
                "阵位拥挤",
                "道具沉寂",
                "【阵势】阵面过满，关键道具容易沉寂。",
                "BattleHint",
                "CombatLogText",
                SourceReadability + ".build.overloaded",
                "maskedBuildGap"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.BuildFeedback,
                "build.suppressed",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "当前阵势被压制，供能回响偏弱。",
                "阵势反馈：被压制",
                "供能偏弱",
                "回响偏弱",
                "【阵势】当前阵势被压制，供能回响偏弱。",
                "BattleHint",
                "CombatLogText",
                SourceReadability + ".build.suppressed",
                "maskedBuildGap"));
        }

        private static void AddFailureRows(ICollection<BattleFeedbackReadabilityRow> rows)
        {
            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.damageTooLow",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "伤害压力偏低，首领护势退得很慢。",
                "失败提示：伤害偏低",
                "护势难退",
                "压制不足",
                "【提示】伤害压力偏低，首领护势退得很慢。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".failure.damageTooLow",
                "maskedFailureHint"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.shieldBreakMissing",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "这套阵势缺少破盾手段。",
                "失败提示：破盾不足",
                "护盾未破",
                "护盾仍厚",
                "【提示】这套阵势缺少破盾手段。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".failure.shieldBreakMissing",
                "maskedFailureHint"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.cleanseMissing",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "当前构筑对净化压力不足。",
                "失败提示：净化不足",
                "污痕未退",
                "净化偏弱",
                "【提示】当前构筑对净化压力不足。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".failure.cleanseMissing",
                "maskedFailureHint"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.controlMissing",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "控制回响不足，首领节奏没有被压住。",
                "失败提示：控制不足",
                "节奏未断",
                "首领续势",
                "【提示】控制回响不足，首领节奏没有被压住。",
                "CombatLog",
                "CombatLogText",
                SourceReadability + ".failure.controlMissing",
                "maskedFailureHint"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.energyDisconnected",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "供能断开，道具沉寂。",
                "失败提示：供能断开",
                "灵力未起",
                "道具沉寂",
                "【提示】供能断开，道具沉寂。",
                "BattleHint",
                "CombatLogText",
                SourceReadability + ".failure.energyDisconnected",
                "maskedFailureHint"));

            rows.Add(Row(
                BattleFeedbackReadabilityCategories.FailureHint,
                "failure.keyItemUnpowered",
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                "关键道具没有亮起，先看供能连通。",
                "失败提示：关键道具沉寂",
                "供能未接",
                "道具无光",
                "【提示】关键道具没有亮起，先看供能连通。",
                "BattleHint",
                "CombatLogText",
                SourceReadability + ".failure.keyItemUnpowered",
                "maskedFailureHint"));
        }

        private static BattleFeedbackReadabilityRow Row(
            string category,
            string feedbackKey,
            string feedbackKind,
            string playerTextChinese,
            string stateLineChinese,
            string castSkillLineChinese,
            string floatingTextChinese,
            string combatLogLineChinese,
            string reuseChannel,
            string playerSurface,
            string sourceDataPath,
            string developerDataPanelFieldKey,
            string energyState = "",
            string triggerFamily = "",
            bool legacyItemBehavior = false,
            bool usesBossStateText = false,
            bool usesCastBar = false)
        {
            return new BattleFeedbackReadabilityRow
            {
                category = category ?? string.Empty,
                feedbackKey = feedbackKey ?? string.Empty,
                feedbackKind = feedbackKind ?? string.Empty,
                energyState = energyState ?? string.Empty,
                triggerFamily = triggerFamily ?? string.Empty,
                playerTextChinese = playerTextChinese ?? string.Empty,
                stateLineChinese = stateLineChinese ?? string.Empty,
                castSkillLineChinese = castSkillLineChinese ?? string.Empty,
                floatingTextChinese = floatingTextChinese ?? string.Empty,
                combatLogLineChinese = combatLogLineChinese ?? string.Empty,
                reuseChannel = reuseChannel ?? string.Empty,
                playerSurface = playerSurface ?? string.Empty,
                sourceDataPath = sourceDataPath ?? string.Empty,
                developerDataPanelFieldKey = developerDataPanelFieldKey ?? string.Empty,
                playerVisible = true,
                developerVisible = true,
                developerFieldsHiddenFromPlayer = true,
                fuzzyHint = true,
                legacyItemBehavior = legacyItemBehavior,
                usesBossStateText = usesBossStateText,
                usesCastBar = usesCastBar,
                usesFloatingFeedback = true,
                usesCombatLog = true,
                devOnly = true,
                isEnabled = false,
                playerShowsCompleteAnswer = false,
                writesFormalUi = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false,
                writesSceneLayout = false
            };
        }

        private static List<BattleFeedbackChannelMapRow> BuildChannelMap(
            IEnumerable<BattleFeedbackReadabilityRow> rows)
        {
            return (rows ?? Array.Empty<BattleFeedbackReadabilityRow>())
                .Where(row => row != null)
                .Select(row => new BattleFeedbackChannelMapRow
                {
                    feedbackKey = row.feedbackKey,
                    category = row.category,
                    feedbackKind = row.feedbackKind,
                    reuseChannel = row.reuseChannel,
                    playerSurface = row.playerSurface,
                    sourceDataPath = row.sourceDataPath,
                    developerDataPanelFieldKey = row.developerDataPanelFieldKey,
                    playerVisible = row.playerVisible,
                    developerVisible = row.developerVisible,
                    developerFieldsHiddenFromPlayer = row.developerFieldsHiddenFromPlayer,
                    writesFormalUi = row.writesFormalUi,
                    writesFormalFlow = row.writesFormalFlow,
                    writesSceneLayout = row.writesSceneLayout
                })
                .ToList();
        }
    }
}
