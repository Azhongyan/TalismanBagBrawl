using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxBuildCombatPreview
    {
        public const string PackageName = "V0.4-BattleSandboxBuildCombatPreview01";
        public const string PreviewBuildId = "battle_sandbox_build_combat_preview";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = PreviewBuildId;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsCurrentBoardSnapshot = true;
        public bool readsDevOnlyEnemyBossProblem = true;
        public bool calculatesSynergyPreview = true;
        public bool calculatesModifierPreview = true;
        public bool calculatesReadinessPreview = true;
        public bool calculatesShapeBuildRulePreview = true;
        public bool writesBossStateShortLine = true;
        public bool writesCastBar = true;
        public bool writesMechanicFloatingText = true;
        public bool writesCombatFeedbackText = true;
        public bool runsFormalCombat;
        public bool callsFormalDamageSettlement;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public bool playerUiShowsFullAnswers;
        public BuildSandboxPreviewContext context = new();
        public BattleSandboxEnemyCombatFeedbackPreview feedbackPreview = new();
        public BattleSandboxShapeBuildRulePreview shapeBuildRulePreview = new();
        public List<BattleSandboxBuildCombatPreviewRow> rows = new();

        public int PreviewScenarioCount => 1;
        public int PlacedItemSnapshotCount => context?.layoutSnapshot?.placedItems?.Count ?? 0;
        public int SynergyMatchCount => context?.buildEvaluation?.activeSynergies?.Count ?? 0;
        public int ModifierBundleCount => context?.modifierBundle?.Count ?? 0;
        public int EffectEventCount => context?.eventBundle?.Count ?? 0;
        public int MechanicFeedbackCount => feedbackPreview?.MechanicFeedbackRowCount ?? 0;
        public int ShapeBuildRuleDefinitionCount => shapeBuildRulePreview?.RuleDefinitionCount ?? 0;
        public int ShapeBuildRuleMatchCount => shapeBuildRulePreview?.MatchedRuleCount ?? 0;
        public int ShapeBuildRuleFeedbackRowCount => shapeBuildRulePreview?.FeedbackRowCount ?? 0;
        public int BossReadinessCount => context?.viewModel?.problemReadiness?.bossReadinessCount ?? 0;
        public int ReadyBossCount => context?.viewModel?.problemReadiness?.readyBossCount ?? 0;
        public int PlayerSideAnswerLeakCount => rows?.Count(row => row != null && row.playerSideAnswerLeak) ?? 1;
        public int FormalFlowLeakCount => CountFormalFlowLeaks();
        public int FeatureFlagDefaultTrueCount => BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        public bool FeatureFlagsAllDisabled => BuildSandboxFeatureFlags.AreAllDefaultsDisabled();
        public bool DevOnlyIsolationPass => devOnly
            && !isEnabled
            && context != null
            && context.devOnly
            && !context.isEnabled
            && shapeBuildRulePreview != null
            && shapeBuildRulePreview.DevOnlyIsolationPass
            && feedbackPreview != null
            && feedbackPreview.devOnly
            && !feedbackPreview.isEnabled;

        private int CountFormalFlowLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!readsCurrentBoardSnapshot) leaks++;
            if (!readsDevOnlyEnemyBossProblem) leaks++;
            if (!calculatesSynergyPreview) leaks++;
            if (!calculatesModifierPreview) leaks++;
            if (!calculatesReadinessPreview) leaks++;
            if (!calculatesShapeBuildRulePreview) leaks++;
            if (runsFormalCombat) leaks++;
            if (callsFormalDamageSettlement) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (grantsFormalReward) leaks++;
            if (advancesChapter) leaks++;
            if (opensFeatureFlag) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            if (context == null
                || !context.devOnly
                || context.isEnabled
                || context.readsFormalSaveData
                || context.writesFormalFlow
                || context.writesFormalData
                || context.touchesFormalScene)
            {
                leaks++;
            }

            leaks += feedbackPreview?.FormalLeakCount ?? 1;
            leaks += shapeBuildRulePreview?.FormalLeakCount ?? 1;
            leaks += rows?.Count(row => row == null || row.formalFlowLeak) ?? 1;
            leaks += FeatureFlagDefaultTrueCount;
            return leaks;
        }
    }

    [Serializable]
    public sealed class BattleSandboxBuildCombatPreviewRow
    {
        public string scenarioId = BattleSandboxBuildCombatPreview.PreviewBuildId;
        public string feedbackId = string.Empty;
        public string feedbackKind = string.Empty;
        public string stateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string sourceDataPath = string.Empty;
        public string developerDataPanelFieldKey = string.Empty;
        public int placedItemSnapshotCount;
        public int activeSynergyCount;
        public int modifierBundleCount;
        public int effectEventCount;
        public int bossReadinessCount;
        public int readyBossCount;
        public bool playerSideAnswerLeak;
        public bool formalFlowLeak;
    }

    public static class BattleSandboxBuildCombatPreviewBuilder
    {
        public const string PackageName = BattleSandboxBuildCombatPreview.PackageName;
        public const string PreviewBuildId = BattleSandboxBuildCombatPreview.PreviewBuildId;

        private const string SourceDataPath = "BuildSandboxBuildCombatPreview.currentV04Board";
        private const string CurrentBoardSourceId = "current_v04_sandbox_board";
        private const string DefaultSampleSourceId = "sample_v04_sandbox_board";

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "dropBias",
            "Boss",
            "keyRequirements",
            "previewWeight",
            "BuildProblemSeedDataset",
            "EnemyBossValidationPool",
            "bossSixKeyFullAnswer",
            "required",
            "solution"
        };

        public static BattleSandboxBuildCombatPreview BuildFromCurrentBoard(
            BuildGridInteractionPreviewController gridController)
        {
            BuildSandboxLayoutSnapshot snapshot = gridController == null
                ? BuildDefaultPreviewLayoutSnapshot()
                : gridController.BuildCurrentLayoutSnapshot();
            return Build(
                snapshot,
                gridController == null ? DefaultSampleSourceId : CurrentBoardSourceId,
                usesCurrentBoardSnapshot: true);
        }

        public static BattleSandboxBuildCombatPreview BuildDefaultPreview()
        {
            return Build(
                BuildDefaultPreviewLayoutSnapshot(),
                DefaultSampleSourceId,
                usesCurrentBoardSnapshot: true);
        }

        public static BattleSandboxBuildCombatPreview Build(
            BuildSandboxLayoutSnapshot layoutSnapshot,
            string previewBuildId = PreviewBuildId,
            bool usesCurrentBoardSnapshot = true)
        {
            BuildSandboxPreviewContext context = BuildContext(layoutSnapshot, previewBuildId);
            BuildTuningDataPanelPreview dataPanel = BuildTuningDataPanelPreviewBuilder.Build(context);
            MechanicHintFeedbackPreview hintPreview =
                MechanicHintFeedbackPreviewBuilder.Build(context, dataPanel: dataPanel);
            BattleSandboxEnemyCombatFeedbackPreview feedbackPreview =
                BattleSandboxEnemyCombatFeedbackBuilder.Build(context, hintPreview, dataPanel);
            BattleSandboxShapeBuildRulePreview shapeBuildRulePreview =
                BattleSandboxShapeBuildRulePreviewBuilder.Evaluate(context.layoutSnapshot);

            IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> shapeBuildRows =
                BattleSandboxShapeBuildRulePreviewBuilder.BuildFeedbackRows(shapeBuildRulePreview);
            IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> buildRows = BuildBuildAwareFeedbackRows(context);
            feedbackPreview.rows.InsertRange(0, buildRows);
            feedbackPreview.rows.InsertRange(0, shapeBuildRows);
            feedbackPreview.sourcePreviewBuildId = context.previewBuildId;

            BattleSandboxBuildCombatPreview preview = new()
            {
                packageName = PackageName,
                sourcePreviewBuildId = context.previewBuildId,
                devOnly = true,
                isEnabled = false,
                readsCurrentBoardSnapshot = usesCurrentBoardSnapshot,
                readsDevOnlyEnemyBossProblem = true,
                calculatesSynergyPreview = true,
                calculatesModifierPreview = true,
                calculatesReadinessPreview = true,
                calculatesShapeBuildRulePreview = true,
                writesBossStateShortLine = true,
                writesCastBar = true,
                writesMechanicFloatingText = true,
                writesCombatFeedbackText = true,
                runsFormalCombat = false,
                callsFormalDamageSettlement = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false,
                playerUiShowsFullAnswers = false,
                context = context,
                feedbackPreview = feedbackPreview,
                shapeBuildRulePreview = shapeBuildRulePreview
            };
            preview.rows = BuildReportRows(preview);
            return preview;
        }

        public static BuildSandboxPreviewContext BuildContext(
            BuildSandboxLayoutSnapshot layoutSnapshot,
            string previewBuildId = PreviewBuildId)
        {
            BuildSandboxLayoutSnapshot snapshot = CloneAndNormalizeSnapshot(layoutSnapshot);
            ApplyPreviewEnergyLinks(snapshot);

            using (PreviewConfigSet configs = PreviewConfigSet.Create())
            {
                BuildEvaluationResult buildEvaluation =
                    SynergyEvaluator.Evaluate(snapshot, configs.SynergyConfigs);
                AffixRarityEvaluationResult affixEvaluation =
                    AffixRarityEvaluator.Evaluate(snapshot, configs.RarityConfigs, configs.AffixConfigs);
                CombatModifierBundle modifierBundle = ModifierEventBridge.BuildCombatModifierBundle(
                    buildEvaluation,
                    affixEvaluation,
                    NormalizePreviewBuildId(previewBuildId));
                EffectEventBundle eventBundle = ModifierEventBridge.BuildEffectEventBundle(
                    buildEvaluation,
                    affixEvaluation,
                    NormalizePreviewBuildId(previewBuildId));

                return BuildSandboxPreviewContextBuilder.Build(new BuildSandboxPreviewContextBuildInput
                {
                    previewBuildId = NormalizePreviewBuildId(previewBuildId),
                    problemSeedDataset = BuildProblemSeedDataset.CreateDefault(),
                    layoutSnapshot = snapshot,
                    buildEvaluation = buildEvaluation,
                    affixEvaluation = affixEvaluation,
                    modifierBundle = modifierBundle,
                    eventBundle = eventBundle,
                    simulationReport = new BuildSimulationBenchmarkReport(),
                    enemyBossValidationPool = EnemyBossValidationPool.CreateDefault(),
                    devChapterContentPool = DevChapterContentPool.CreateDefault(),
                    ledgerProgress = new List<LedgerBuildTaskProgressPreview>()
                });
            }
        }

        public static BuildSandboxLayoutSnapshot BuildDefaultPreviewLayoutSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_energy_incense",
                "Vertical2",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(0, 1), new ItemShapeCell(0, 2) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_fire_talisman",
                "Single1",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(1, 1) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_thunder_sword",
                "Single1",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(1, 0) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_x2_wood_talisman",
                "Vertical2",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(4, 0), new ItemShapeCell(4, 1) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_guard_wood",
                "Vertical2",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(1, 2), new ItemShapeCell(1, 3) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_cleanse_corner",
                "Corner3",
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(2, 1), new ItemShapeCell(3, 1), new ItemShapeCell(2, 2) }));
            snapshot.placedItems.Add(CreatePlacedItemSnapshot(
                "preview_stone_core",
                "Square4",
                ItemShapeRotation.Rotation0,
                new[]
                {
                    new ItemShapeCell(3, 3),
                    new ItemShapeCell(4, 3),
                    new ItemShapeCell(3, 4),
                    new ItemShapeCell(4, 4)
                }));
            ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        public static BuildSandboxPlacedItemSnapshot CreatePlacedItemSnapshot(
            string itemId,
            string shapeId,
            ItemShapeRotation rotation,
            IEnumerable<ItemShapeCell> occupiedCells)
        {
            List<ItemShapeCell> cells = NormalizeCells(occupiedCells);
            ItemShapeCell anchor = cells.Count == 0
                ? default
                : cells.OrderBy(cell => cell.x).ThenBy(cell => cell.y).First();

            return new BuildSandboxPlacedItemSnapshot
            {
                itemId = itemId ?? string.Empty,
                shapeId = shapeId ?? string.Empty,
                anchorCell = anchor,
                occupiedCells = cells,
                rotation = rotation,
                tags = ResolvePreviewTags(itemId, shapeId).ToList(),
                isPowered = false,
                energySourceId = string.Empty,
                affixList = ResolvePreviewAffixes(itemId).ToList(),
                rarity = ResolvePreviewRarity(itemId)
            };
        }

        public static void ApplyPreviewEnergyLinks(BuildSandboxLayoutSnapshot snapshot)
        {
            List<BuildSandboxPlacedItemSnapshot> items = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();
            List<BuildSandboxPlacedItemSnapshot> providers = items
                .Where(item => HasAnyTag(item, "energy_preview", "ju_neng", "energy"))
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                item.isPowered = HasAnyTag(item, "energy_preview", "ju_neng", "energy");
                item.energySourceId = item.isPowered ? item.itemId : string.Empty;
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item.isPowered)
                {
                    continue;
                }

                BuildSandboxPlacedItemSnapshot provider = providers
                    .FirstOrDefault(candidate => AreAdjacent(item, candidate));
                if (provider == null)
                {
                    continue;
                }

                item.isPowered = true;
                item.energySourceId = provider.itemId ?? string.Empty;
            }
        }

        public static int CountPlayerTextLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return preview?.rows?.Count(row => row != null && row.playerSideAnswerLeak) ?? 1;
        }

        private static BuildSandboxLayoutSnapshot CloneAndNormalizeSnapshot(
            BuildSandboxLayoutSnapshot layoutSnapshot)
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            foreach (BuildSandboxPlacedItemSnapshot item in layoutSnapshot?.placedItems
                         ?? new List<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null || string.IsNullOrWhiteSpace(item.itemId))
                {
                    continue;
                }

                BuildSandboxPlacedItemSnapshot copy = new()
                {
                    itemId = item.itemId ?? string.Empty,
                    shapeId = item.shapeId ?? string.Empty,
                    anchorCell = item.anchorCell,
                    occupiedCells = NormalizeCells(item.occupiedCells),
                    rotation = item.rotation,
                    tags = ResolveMergedTags(item).ToList(),
                    isPowered = item.isPowered,
                    energySourceId = item.energySourceId ?? string.Empty,
                    affixList = ResolveMergedAffixes(item).ToList(),
                    rarity = string.IsNullOrWhiteSpace(item.rarity)
                        ? ResolvePreviewRarity(item.itemId)
                        : item.rarity
                };

                if (copy.occupiedCells.Count == 0)
                {
                    copy.occupiedCells.Add(copy.anchorCell);
                }

                snapshot.placedItems.Add(copy);
            }

            return snapshot;
        }

        private static IEnumerable<string> ResolveMergedTags(BuildSandboxPlacedItemSnapshot item)
        {
            HashSet<string> tags = new(StringComparer.Ordinal);
            foreach (string tag in item?.tags ?? new List<string>())
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag.Trim());
                }
            }

            foreach (string tag in ResolvePreviewTags(item?.itemId, item?.shapeId))
            {
                tags.Add(tag);
            }

            return tags.OrderBy(tag => tag, StringComparer.Ordinal);
        }

        private static IEnumerable<string> ResolveMergedAffixes(BuildSandboxPlacedItemSnapshot item)
        {
            List<string> explicitAffixes = (item?.affixList ?? new List<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
            return explicitAffixes.Count > 0
                ? explicitAffixes
                : ResolvePreviewAffixes(item?.itemId);
        }

        private static IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> BuildBuildAwareFeedbackRows(
            BuildSandboxPreviewContext context)
        {
            BuildSummaryViewModel summary = context?.viewModel?.buildSummary ?? new BuildSummaryViewModel();
            ProblemReadinessViewModel readiness =
                context?.viewModel?.problemReadiness ?? new ProblemReadinessViewModel();
            string mechanicFloating = ResolveMechanicFloatingText(context);
            bool hasReadyWindow = readiness.readyBossCount > 0;

            return new[]
            {
                CreateFeedbackRow(
                    "buildCombat.boardState",
                    BattleSandboxEnemyCombatFeedbackKinds.BossState,
                    ResolveStateLine(summary, readiness),
                    "\u9996\u9886\u6b63\u5728\u84c4\u529b",
                    "\u9635\u52bf\u56de\u54cd",
                    ResolveStateLog(summary, readiness),
                    "BossInfoPanel",
                    "Current board build state short line",
                    SourceDataPath + ".layoutSnapshot",
                    "requiredStats",
                    usesCastBar: false,
                    usesBossInfo: true,
                    2.4f),
                CreateFeedbackRow(
                    "buildCombat.castPreview",
                    BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                    "\u9996\u9886\uff1a\u51c6\u5907\u91cd\u538b\u9635\u7ebf",
                    "\u65bd\u6cd5\u4e2d\uff1a\u538b\u9635\u51b2\u51fb",
                    "\u65bd\u6cd5\u9884\u5146",
                    "\u3010\u65bd\u6cd5\u3011\u538b\u9635\u5373\u5c06\u843d\u4e0b",
                    "V02EnemyIntentUI",
                    "Cast bar mirrors existing enemy intent timing",
                    SourceDataPath + ".modifierBundle",
                    "requiredSynergy",
                    usesCastBar: true,
                    usesBossInfo: true,
                    3.2f),
                CreateFeedbackRow(
                    "buildCombat.mechanicFeedback",
                    BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                    "\u673a\u5236\u53cd\u9988\uff1a\u7b26\u4f4d\u4ea7\u751f\u56de\u54cd",
                    "\u673a\u5236\u53d8\u5316",
                    mechanicFloating,
                    "\u3010\u673a\u5236\u3011\u53ea\u663e\u793a\u573a\u4e0a\u73b0\u8c61",
                    "FloatingCombatText",
                    "Mechanic phenomenon only",
                    SourceDataPath + ".synergyModifierReadiness",
                    "hardSolutionTags",
                    usesCastBar: false,
                    usesBossInfo: false,
                    2.2f),
                CreateFeedbackRow(
                    "buildCombat.readinessFeedback",
                    hasReadyWindow
                        ? BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow
                        : BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                    hasReadyWindow
                        ? "\u9996\u9886\uff1a\u7834\u7efd\u7a97\u53e3\u51fa\u73b0"
                        : "\u53cd\u9988\uff1a\u77ed\u677f\u88ab\u653e\u5927",
                    hasReadyWindow
                        ? "\u7834\u7efd\u7a97\u53e3"
                        : "\u538b\u529b\u53cd\u9988",
                    hasReadyWindow
                        ? "\u62a4\u52bf\u88c2\u5f00"
                        : "\u9635\u9762\u627f\u538b",
                    hasReadyWindow
                        ? "\u3010\u7834\u7efd\u3011\u6293\u4f4f\u77ed\u6682\u8282\u594f"
                        : "\u3010\u53cd\u9988\u3011\u573a\u4e0a\u538b\u529b\u4e0a\u5347",
                    "BattleHint",
                    "Readiness outcome masked as combat feedback",
                    SourceDataPath + ".problemReadiness",
                    "bossSixKeyFullAnswer",
                    usesCastBar: false,
                    usesBossInfo: false,
                    2.6f)
            };
        }

        private static BattleSandboxEnemyCombatFeedbackRow CreateFeedbackRow(
            string feedbackId,
            string feedbackKind,
            string stateLine,
            string castSkillLine,
            string floatingText,
            string combatLogLine,
            string reuseSource,
            string reusePattern,
            string sourcePath,
            string developerKey,
            bool usesCastBar,
            bool usesBossInfo,
            float castDurationSeconds)
        {
            return new BattleSandboxEnemyCombatFeedbackRow
            {
                feedbackId = feedbackId ?? string.Empty,
                feedbackKind = feedbackKind ?? string.Empty,
                bossDisplayNameChinese = "\u9996\u9886",
                stateLineChinese = stateLine ?? string.Empty,
                castSkillLineChinese = castSkillLine ?? string.Empty,
                floatingTextChinese = floatingText ?? string.Empty,
                combatLogLineChinese = combatLogLine ?? string.Empty,
                reuseSourceComponent = reuseSource ?? string.Empty,
                reuseLanguagePattern = reusePattern ?? string.Empty,
                sourceDataPath = sourcePath ?? string.Empty,
                developerDataPanelFieldKey = developerKey ?? string.Empty,
                castDurationSeconds = Mathf.Max(0.6f, castDurationSeconds),
                devOnly = true,
                isEnabled = false,
                playerVisible = true,
                developerPanelVisible = true,
                playerShowsCompleteAnswer = false,
                usesFloatingCombatTextLanguage = true,
                usesEnemyCastBarLanguage = usesCastBar,
                usesBossInfoLanguage = usesBossInfo,
                runsFormalCombat = false,
                callsFormalDamageSettlement = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false
            };
        }

        private static List<BattleSandboxBuildCombatPreviewRow> BuildReportRows(
            BattleSandboxBuildCombatPreview preview)
        {
            List<BattleSandboxBuildCombatPreviewRow> rows = new();
            foreach (BattleSandboxEnemyCombatFeedbackRow row in preview?.feedbackPreview?.rows
                         ?? new List<BattleSandboxEnemyCombatFeedbackRow>())
            {
                if (row == null)
                {
                    continue;
                }

                rows.Add(new BattleSandboxBuildCombatPreviewRow
                {
                    scenarioId = preview.sourcePreviewBuildId,
                    feedbackId = row.feedbackId,
                    feedbackKind = row.feedbackKind,
                    stateLineChinese = row.stateLineChinese,
                    castSkillLineChinese = row.castSkillLineChinese,
                    floatingTextChinese = row.floatingTextChinese,
                    combatLogLineChinese = row.combatLogLineChinese,
                    sourceDataPath = row.sourceDataPath,
                    developerDataPanelFieldKey = row.developerDataPanelFieldKey,
                    placedItemSnapshotCount = preview.PlacedItemSnapshotCount,
                    activeSynergyCount = preview.SynergyMatchCount,
                    modifierBundleCount = preview.ModifierBundleCount,
                    effectEventCount = preview.EffectEventCount,
                    bossReadinessCount = preview.BossReadinessCount,
                    readyBossCount = preview.ReadyBossCount,
                    playerSideAnswerLeak = ContainsPlayerLeak(row),
                    formalFlowLeak = row.FormalLeak
                });
            }

            return rows;
        }

        private static string ResolveStateLine(
            BuildSummaryViewModel summary,
            ProblemReadinessViewModel readiness)
        {
            if ((summary?.placedItemCount ?? 0) <= 0)
            {
                return "\u9996\u9886\uff1a\u9635\u9762\u5c1a\u672a\u6210\u5f62";
            }

            if ((readiness?.readyBossCount ?? 0) > 0)
            {
                return "\u9996\u9886\uff1a\u62a4\u52bf\u51fa\u73b0\u7834\u7efd";
            }

            if ((summary?.activeSynergyCount ?? 0) > 0)
            {
                return "\u9996\u9886\uff1a\u9635\u52bf\u6b63\u5728\u5171\u9e23";
            }

            return "\u9996\u9886\uff1a\u90aa\u6c14\u7ee7\u7eed\u805a\u62e2";
        }

        private static string ResolveStateLog(
            BuildSummaryViewModel summary,
            ProblemReadinessViewModel readiness)
        {
            if ((summary?.placedItemCount ?? 0) <= 0)
            {
                return "\u3010\u72b6\u6001\u3011\u573a\u4e0a\u8fd8\u6ca1\u6709\u9635\u52bf\u53cd\u9988";
            }

            if ((readiness?.readyBossCount ?? 0) > 0)
            {
                return "\u3010\u72b6\u6001\u3011\u62a4\u52bf\u5f00\u59cb\u677e\u52a8";
            }

            return "\u3010\u72b6\u6001\u3011\u538b\u529b\u6b63\u5728\u6c47\u805a";
        }

        private static string ResolveMechanicFloatingText(BuildSandboxPreviewContext context)
        {
            List<string> modifierTypes = context?.modifierBundle?.modifiers?
                .Where(modifier => modifier != null)
                .Select(modifier => modifier.modifierType ?? string.Empty)
                .ToList() ?? new List<string>();

            if (modifierTypes.Contains(ModifierEventBridge.ShieldBreakBonus, StringComparer.Ordinal))
            {
                return "\u62a4\u52bf\u88c2\u5f00";
            }

            if (modifierTypes.Contains(ModifierEventBridge.CleanseBonus, StringComparer.Ordinal))
            {
                return "\u6c61\u75d5\u9000\u6563";
            }

            if (modifierTypes.Contains(ModifierEventBridge.ControlDurationBonus, StringComparer.Ordinal))
            {
                return "\u77ed\u6682\u505c\u6ede";
            }

            if (modifierTypes.Contains(ModifierEventBridge.EnergyReturnBonus, StringComparer.Ordinal))
            {
                return "\u7075\u6c14\u56de\u6d41";
            }

            if (modifierTypes.Contains(ModifierEventBridge.DamageBonus, StringComparer.Ordinal))
            {
                return "\u706b\u5149\u538b\u9635";
            }

            return "\u9635\u9762\u8bd5\u63a2";
        }

        private static IEnumerable<string> ResolvePreviewTags(string itemId, string shapeId)
        {
            HashSet<string> tags = new(StringComparer.Ordinal)
            {
                "shape",
                "placement",
                "formation",
                "build_item"
            };

            string safeShape = string.IsNullOrWhiteSpace(shapeId) ? string.Empty : shapeId.Trim();
            if (!string.IsNullOrWhiteSpace(safeShape))
            {
                tags.Add("shape_" + safeShape.ToLowerInvariant());
            }

            string id = (itemId ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Contains("wood_talisman") || id.Contains("guard_wood"))
            {
                tags.Add("hu_zhen");
                tags.Add("guard");
                tags.Add("ward");
                tags.Add("defense_preview");
                tags.Add("ward_preview");
                tags.Add("shieldBonus");
            }

            if (id.Contains("fire"))
            {
                tags.Add("lihuo");
                tags.Add("fire");
                tags.Add("clear");
                tags.Add("damage_preview");
                tags.Add("talisman_preview");
            }

            if (id.Contains("thunder"))
            {
                tags.Add("jing_lei");
                tags.Add("thunder");
                tags.Add("break");
                tags.Add("shieldBreak");
                tags.Add("shield_break");
                tags.Add("damage_preview");
                tags.Add("talisman_preview");
            }

            if (id.Contains("cleanse"))
            {
                tags.Add("jing_e");
                tags.Add("cleanse");
                tags.Add("purify");
                tags.Add("purifying");
                tags.Add("talisman_preview");
                tags.Add("cleanse_preview");
                tags.Add("control_preview");
                tags.Add("ward_preview");
            }

            if (id.Contains("stone_core"))
            {
                tags.Add("orange");
                tags.Add("core");
                tags.Add("burst");
                tags.Add("anchor");
                tags.Add("orange_core_preview");
                tags.Add("core_preview");
                tags.Add("bond_preview");
                tags.Add("synergy_preview");
                tags.Add("damage_preview");
                tags.Add("defense_preview");
            }

            if (id.Contains("energy"))
            {
                tags.Add("consumable_preview");
                tags.Add("rhythm_preview");
            }

            if (id.Contains("old_bell"))
            {
                tags.Add("zhen_hun");
                tags.Add("control");
                tags.Add("interrupt");
                tags.Add("control_preview");
                tags.Add("ward_preview");
            }

            if (id.Contains("soul_seal"))
            {
                tags.Add("zhen_hun");
                tags.Add("control");
                tags.Add("interrupt");
                tags.Add("control_preview");
                tags.Add("defense_preview");
                tags.Add("bond_preview");
                tags.Add("synergy_preview");
            }

            return tags.OrderBy(tag => tag, StringComparer.Ordinal);
        }

        private static IEnumerable<string> ResolvePreviewAffixes(string itemId)
        {
            string id = (itemId ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Contains("fire"))
            {
                return new[] { "bs_affix_lihuo_spark" };
            }

            if (id.Contains("thunder"))
            {
                return new[] { "bs_affix_break_boost", "bs_affix_lihuo_spark" };
            }

            if (id.Contains("wood_talisman"))
            {
                return new[] { "bs_affix_guardian_ward" };
            }

            if (id.Contains("guard_wood"))
            {
                return new[] { "bs_affix_guardian_ward", "bs_affix_focus_gather" };
            }

            if (id.Contains("cleanse"))
            {
                return new[] { "bs_affix_purifying_seal", "bs_affix_control_hold" };
            }

            if (id.Contains("stone_core"))
            {
                return new[] { "bs_affix_orange_core", "bs_affix_bond_plus_one", "bs_affix_guardian_ward" };
            }

            if (id.Contains("old_bell"))
            {
                return new[] { "bs_affix_control_hold", "bs_affix_purifying_seal" };
            }

            if (id.Contains("soul_seal"))
            {
                return new[] { "bs_affix_control_hold", "bs_affix_bond_plus_one" };
            }

            return Array.Empty<string>();
        }

        private static string ResolvePreviewRarity(string itemId)
        {
            string id = (itemId ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Contains("stone_core"))
            {
                return "orange";
            }

            if (id.Contains("old_bell") || id.Contains("soul_seal"))
            {
                return "purple";
            }

            if (id.Contains("thunder") || id.Contains("cleanse") || id.Contains("guard_wood"))
            {
                return "blue";
            }

            if (id.Contains("fire") || id.Contains("energy") || id.Contains("wood_talisman"))
            {
                return "green";
            }

            return "white";
        }

        private static bool ContainsPlayerLeak(BattleSandboxEnemyCombatFeedbackRow row)
        {
            if (row == null)
            {
                return true;
            }

            return PlayerTextFields(row).Any(value =>
                string.IsNullOrWhiteSpace(value)
                || ContainsLatin(value)
                || ForbiddenPlayerAnswerTokens.Any(token =>
                    value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private static IEnumerable<string> PlayerTextFields(BattleSandboxEnemyCombatFeedbackRow row)
        {
            yield return row.bossDisplayNameChinese;
            yield return row.stateLineChinese;
            yield return row.castSkillLineChinese;
            yield return row.floatingTextChinese;
            yield return row.combatLogLineChinese;
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        private static bool HasAnyTag(BuildSandboxPlacedItemSnapshot item, params string[] tags)
        {
            return (item?.tags ?? new List<string>()).Any(itemTag =>
                tags.Any(tag => string.Equals(itemTag, tag, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool AreAdjacent(
            BuildSandboxPlacedItemSnapshot a,
            BuildSandboxPlacedItemSnapshot b)
        {
            if (a == null || b == null || string.Equals(a.itemId, b.itemId, StringComparison.Ordinal))
            {
                return false;
            }

            foreach (ItemShapeCell aCell in a.occupiedCells ?? new List<ItemShapeCell>())
            {
                foreach (ItemShapeCell bCell in b.occupiedCells ?? new List<ItemShapeCell>())
                {
                    int distance = Math.Abs(aCell.x - bCell.x) + Math.Abs(aCell.y - bCell.y);
                    if (distance == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static List<ItemShapeCell> NormalizeCells(IEnumerable<ItemShapeCell> cells)
        {
            List<ItemShapeCell> result = new();
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (ItemShapeCell cell in cells ?? Enumerable.Empty<ItemShapeCell>())
            {
                string key = cell.x + ":" + cell.y;
                if (seen.Add(key))
                {
                    result.Add(cell);
                }
            }

            result.Sort((left, right) =>
            {
                int compareX = left.x.CompareTo(right.x);
                return compareX != 0 ? compareX : left.y.CompareTo(right.y);
            });
            return result;
        }

        private static string NormalizePreviewBuildId(string previewBuildId)
        {
            return string.IsNullOrWhiteSpace(previewBuildId)
                ? PreviewBuildId
                : previewBuildId.Trim();
        }

        private sealed class PreviewConfigSet : IDisposable
        {
            public List<SynergyConfig> SynergyConfigs { get; } = new();
            public List<RarityTierConfig> RarityConfigs { get; } = new();
            public List<AffixConfig> AffixConfigs { get; } = new();

            public static PreviewConfigSet Create()
            {
                PreviewConfigSet configs = new();
                configs.CreateSynergies();
                configs.CreateRarities();
                configs.CreateAffixes();
                return configs;
            }

            public void Dispose()
            {
                DestroyObjects(SynergyConfigs);
                DestroyObjects(RarityConfigs);
                DestroyObjects(AffixConfigs);
            }

            private void CreateSynergies()
            {
                SynergyConfigs.Add(CreateSynergy(
                    "buildsandbox_synergy_lihuo_clear",
                    "Lihuo Clear",
                    new[] { "lihuo", "damage_preview" },
                    "damage_preview"));
                SynergyConfigs.Add(CreateSynergy(
                    "buildsandbox_synergy_hu_zhen_guard",
                    "Guard Ward",
                    new[] { "hu_zhen", "defense_preview" },
                    "defense_preview"));
                SynergyConfigs.Add(CreateSynergy(
                    "buildsandbox_synergy_jing_e_cleanse",
                    "Cleanse Control",
                    new[] { "jing_e", "cleanse_preview", "control_preview" },
                    "cleanse_preview"));
                SynergyConfigs.Add(CreateSynergy(
                    "buildsandbox_synergy_jing_lei_break",
                    "Thunder Break",
                    new[] { "jing_lei", "break", "damage_preview" },
                    "damage_preview"));
                SynergyConfigs.Add(CreateSynergy(
                    "buildsandbox_synergy_ju_neng_energy",
                    "Energy Stability",
                    new[] { "ju_neng", "energy_preview" },
                    "energy_preview"));
            }

            private void CreateRarities()
            {
                RarityConfigs.Add(CreateRarity("white", 0, 1, 100, 1f));
                RarityConfigs.Add(CreateRarity("green", 1, 2, 80, 1.1f));
                RarityConfigs.Add(CreateRarity("blue", 2, 3, 60, 1.2f));
                RarityConfigs.Add(CreateRarity("purple", 3, 4, 40, 1.35f));
                RarityConfigs.Add(CreateRarity("orange", 4, 5, 20, 1.6f));
            }

            private void CreateAffixes()
            {
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_lihuo_spark",
                    "damage",
                    new[] { "lihuo", "damage_preview" },
                    new[] { "green", "blue", "purple", "orange" },
                    8,
                    16,
                    "lihuo_spark_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_break_boost",
                    "break",
                    new[] { "jing_lei", "break", "damage_preview" },
                    new[] { "blue", "purple", "orange" },
                    7,
                    15,
                    "break_boost_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_guardian_ward",
                    "guard",
                    new[] { "hu_zhen", "defense_preview", "ward_preview" },
                    new[] { "green", "blue", "purple", "orange" },
                    6,
                    14,
                    "guardian_ward_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_purifying_seal",
                    "cleanse",
                    new[] { "jing_e", "cleanse_preview", "purifying" },
                    new[] { "blue", "purple", "orange" },
                    6,
                    14,
                    "purifying_seal_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_focus_gather",
                    "energy",
                    new[] { "energy_preview", "ju_neng", "powered" },
                    new[] { "green", "blue", "purple", "orange" },
                    5,
                    12,
                    "focus_gather_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_control_hold",
                    "control",
                    new[] { "control_preview", "zhen_hun", "interrupt" },
                    new[] { "blue", "purple", "orange" },
                    5,
                    12,
                    "control_hold_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_orange_core",
                    "core",
                    new[] { "orange_core_preview", "core_preview", "core" },
                    new[] { "orange" },
                    12,
                    20,
                    "orange_core_preview"));
                AffixConfigs.Add(CreateAffix(
                    "bs_affix_bond_plus_one",
                    "bond",
                    new[] { "bond_preview", "synergy_preview" },
                    new[] { "purple", "orange" },
                    8,
                    16,
                    "bond_plus_one_preview"));
            }

            private static SynergyConfig CreateSynergy(
                string synergyId,
                string displayName,
                IEnumerable<string> requiredTags,
                string placementTag)
            {
                SynergyConfig config = ScriptableObject.CreateInstance<SynergyConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.synergyId = synergyId;
                config.displayName = displayName;
                config.requiredTags = requiredTags.ToList();
                config.thresholds = new List<SynergyThresholdConfig>
                {
                    CreateThreshold(2),
                    CreateThreshold(4),
                    CreateThreshold(6),
                    CreateThreshold(8)
                };
                config.placementConditions = new List<PlacementConditionConfig>
                {
                    new()
                    {
                        conditionId = synergyId + ".adjacent",
                        conditionType = "adjacent",
                        requiredTag = placementTag,
                        requiredCount = 1,
                        devOnly = true,
                        isEnabled = false
                    }
                };
                config.energyConditions = new List<EnergyConditionConfig>();
                config.devOnly = true;
                config.isEnabled = false;
                return config;
            }

            private static SynergyThresholdConfig CreateThreshold(int pieceCount)
            {
                return new SynergyThresholdConfig
                {
                    pieceCount = pieceCount,
                    tierLabel = pieceCount + "-piece",
                    effectToken = "battle_sandbox_build_preview",
                    effectSummary = "BuildSandbox combat preview only. No formal combat modifier is emitted.",
                    devOnly = true,
                    isEnabled = false
                };
            }

            private static RarityTierConfig CreateRarity(
                string rarityId,
                int tierIndex,
                int slots,
                int weight,
                float multiplier)
            {
                RarityTierConfig config = ScriptableObject.CreateInstance<RarityTierConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.rarityId = rarityId;
                config.displayName = rarityId;
                config.tierIndex = tierIndex;
                config.affixSlotCount = slots;
                config.rollWeight = weight;
                config.previewPowerMultiplier = multiplier;
                config.visualKey = "battle_sandbox_" + rarityId;
                config.devOnly = true;
                config.isEnabled = false;
                return config;
            }

            private static AffixConfig CreateAffix(
                string affixId,
                string group,
                IEnumerable<string> requiredTags,
                IEnumerable<string> allowedRarities,
                int minRoll,
                int maxRoll,
                string resultToken)
            {
                AffixConfig config = ScriptableObject.CreateInstance<AffixConfig>();
                config.hideFlags = HideFlags.HideAndDontSave;
                config.affixId = affixId;
                config.displayName = affixId;
                config.affixGroup = group;
                config.requiredTags = requiredTags.ToList();
                config.allowedRarities = allowedRarities.ToList();
                config.minRoll = minRoll;
                config.maxRoll = maxRoll;
                config.previewResultToken = resultToken;
                config.previewSummary = "BuildSandbox combat preview only. No formal damage is emitted.";
                config.devOnly = true;
                config.isEnabled = false;
                return config;
            }

            private static void DestroyObjects<T>(IEnumerable<T> objects)
                where T : UnityEngine.Object
            {
                foreach (T target in objects ?? Enumerable.Empty<T>())
                {
                    if (target == null)
                    {
                        continue;
                    }

                    if (Application.isPlaying)
                    {
                        Object.Destroy(target);
                    }
                    else
                    {
                        Object.DestroyImmediate(target);
                    }
                }
            }
        }
    }
}
