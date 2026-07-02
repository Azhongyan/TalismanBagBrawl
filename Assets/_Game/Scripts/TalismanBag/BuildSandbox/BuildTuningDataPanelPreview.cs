using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildTuningDataPanelPreview
    {
        public const string PackageName = "V0.4-BuildTuningDataPanelPreview01";
        public const string ReferenceMode = "DeveloperDataOnlyNoFormalUi";

        public string packageName = PackageName;
        public string sourcePreviewContextPackageName = string.Empty;
        public string sourcePreviewBuildId = string.Empty;
        public string referenceMode = ReferenceMode;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerFormalUi;
        public bool writesFormalUi;
        public bool writesFormalScene;
        public bool readsFormalSaveData;
        public bool writesFormalData;
        public bool createsMechanicUiFrame;
        public bool changesHandTunedLayout;
        public bool playerUiShowsEnglishStableKey;
        public bool playerUiShowsFullAnswers;
        public List<BuildTuningDataPanelSection> sections = new();

        public int SectionCount => sections?.Count ?? 0;
        public int FieldCount => sections?.Sum(section => section?.rows?.Count ?? 0) ?? 0;
        public int DeveloperVisibleFieldCount => Rows.Count(row => row.developerVisible);
        public int PlayerVisibleFieldCount => Rows.Count(row => row.playerVisible);
        public int MaskedFieldCount => Rows.Count(row => row.maskedFromPlayer);
        public IReadOnlyList<BuildTuningDataPanelFieldRow> Rows =>
            (sections ?? new List<BuildTuningDataPanelSection>())
            .SelectMany(section => section?.rows ?? new List<BuildTuningDataPanelFieldRow>())
            .ToList();
    }

    [Serializable]
    public sealed class BuildTuningDataPanelSection
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string dataPanelSlot = string.Empty;
        public string sourceViewModelKey = string.Empty;
        public bool developerOnly = true;
        public bool playerVisible;
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public bool createsMechanicUiFrame;
        public List<BuildTuningDataPanelFieldRow> rows = new();
    }

    [Serializable]
    public sealed class BuildTuningDataPanelFieldRow
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string value = string.Empty;
        public string sourceDataPath = string.Empty;
        public string dataPanelSlot = string.Empty;
        public string note = string.Empty;
        public bool developerVisible = true;
        public bool playerVisible;
        public bool maskedFromPlayer;
        public bool reportOnly = true;
        public bool devOnly = true;
    }

    public static class BuildTuningDataPanelPreviewBuilder
    {
        private static readonly DeveloperTuningFieldBinding[] RequiredBindings =
        {
            new("buildSummary", "构筑概览", "BuildSummaryPanelSlot", true),
            new("synergySummary", "协同概览", "SynergyPanelSlot", true),
            new("shapeOccupancy", "格位占用", "ShapeOccupancyPanelSlot", true),
            new("affixModifier", "词缀修正", "AffixModifierPanelSlot", true),
            new("problemReadiness", "题目准备度", "ProblemReadinessPanelSlot", true),
            new("simulationResult", "模拟结果", "SimulationResultPanelSlot", true),
            new("dropBiasPreview", "定向倾向", "ProblemReadinessPanelSlot", true),
            new("bossSixKeyFullAnswer", "Boss六钥匙完整答案", "ProblemReadinessPanelSlot", true)
        };

        public static BuildTuningDataPanelPreview Build(
            BuildSandboxPreviewContext context,
            BattlePageViewSpec spec = null)
        {
            BuildSandboxPreviewContext safeContext = context ?? new BuildSandboxPreviewContext();
            BattlePageViewSpec safeSpec = spec ?? BattlePageViewSpec.CreateDefault();
            List<BuildTuningDataPanelSection> sections = CreateSections(safeSpec);

            BuildTuningDataPanelPreview preview = new()
            {
                packageName = BuildTuningDataPanelPreview.PackageName,
                sourcePreviewContextPackageName = safeContext.packageName ?? string.Empty,
                sourcePreviewBuildId = safeContext.previewBuildId ?? string.Empty,
                devOnly = true,
                isEnabled = false,
                playerFormalUi = false,
                writesFormalUi = false,
                writesFormalScene = false,
                readsFormalSaveData = false,
                writesFormalData = false,
                createsMechanicUiFrame = false,
                changesHandTunedLayout = false,
                playerUiShowsEnglishStableKey = false,
                playerUiShowsFullAnswers = false,
                sections = sections
            };

            BuildSandboxPreviewViewModel viewModel = safeContext.viewModel ?? new BuildSandboxPreviewViewModel();
            BuildProblemSeedDataset dataset = safeContext.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            AddBuildSummary(Find(sections, "buildSummary"), viewModel.buildSummary, safeContext);
            AddSynergySummary(Find(sections, "synergySummary"), viewModel.synergy);
            AddShapeOccupancy(Find(sections, "shapeOccupancy"), viewModel.shapeOccupancy);
            AddAffixModifier(Find(sections, "affixModifier"), viewModel.affixModifier);
            AddProblemReadiness(Find(sections, "problemReadiness"), viewModel.problemReadiness, dataset);
            AddSimulationResult(Find(sections, "simulationResult"), viewModel.simulationResult);
            AddDropBiasPreview(Find(sections, "dropBiasPreview"), dataset);
            AddBossSixKeyFullAnswer(Find(sections, "bossSixKeyFullAnswer"), dataset);
            return preview;
        }

        private static List<BuildTuningDataPanelSection> CreateSections(BattlePageViewSpec spec)
        {
            Dictionary<string, DeveloperTuningFieldBinding> specBindings =
                (spec?.developerTuningPanel?.fields ?? new List<DeveloperTuningFieldBinding>())
                .Where(field => field != null && !string.IsNullOrWhiteSpace(field.englishStableKey))
                .GroupBy(field => field.englishStableKey, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            List<BuildTuningDataPanelSection> sections = new();
            foreach (DeveloperTuningFieldBinding fallback in RequiredBindings)
            {
                DeveloperTuningFieldBinding binding = specBindings.TryGetValue(
                    fallback.englishStableKey,
                    out DeveloperTuningFieldBinding fromSpec)
                    ? fromSpec
                    : fallback;

                sections.Add(new BuildTuningDataPanelSection
                {
                    englishStableKey = binding.englishStableKey,
                    chineseDisplayName = string.IsNullOrWhiteSpace(binding.chineseDisplayName)
                        ? fallback.chineseDisplayName
                        : binding.chineseDisplayName,
                    dataPanelSlot = string.IsNullOrWhiteSpace(binding.dataPanelSlot)
                        ? fallback.dataPanelSlot
                        : binding.dataPanelSlot,
                    sourceViewModelKey = ResolveSourceViewModelKey(binding.englishStableKey),
                    developerOnly = true,
                    playerVisible = false,
                    referenceOnly = true,
                    canWriteFormalUi = false,
                    createsMechanicUiFrame = false
                });
            }

            return sections;
        }

        private static string ResolveSourceViewModelKey(string englishStableKey)
        {
            return englishStableKey switch
            {
                "buildSummary" => nameof(BuildSandboxPreviewViewModel.buildSummary),
                "synergySummary" => nameof(BuildSandboxPreviewViewModel.synergy),
                "shapeOccupancy" => nameof(BuildSandboxPreviewViewModel.shapeOccupancy),
                "affixModifier" => nameof(BuildSandboxPreviewViewModel.affixModifier),
                "problemReadiness" => nameof(BuildSandboxPreviewViewModel.problemReadiness),
                "simulationResult" => nameof(BuildSandboxPreviewViewModel.simulationResult),
                "dropBiasPreview" => nameof(BuildProblemSeedDataset.dropBiases),
                "bossSixKeyFullAnswer" => nameof(BuildProblemSeedDataset.bossProblems),
                _ => englishStableKey ?? string.Empty
            };
        }

        private static BuildTuningDataPanelSection Find(
            IEnumerable<BuildTuningDataPanelSection> sections,
            string englishStableKey)
        {
            return sections.First(section => string.Equals(
                section.englishStableKey,
                englishStableKey,
                StringComparison.Ordinal));
        }

        private static void AddBuildSummary(
            BuildTuningDataPanelSection section,
            BuildSummaryViewModel summary,
            BuildSandboxPreviewContext context)
        {
            BuildSummaryViewModel safeSummary = summary ?? new BuildSummaryViewModel();
            Add(section, "buildSummary.previewBuildId", "预览构筑ID", safeSummary.previewBuildId, "BuildSummary.previewBuildId");
            Add(section, "buildSummary.placedItemCount", "已放置道具数", safeSummary.placedItemCount, "BuildSummary.placedItemCount");
            Add(section, "buildSummary.occupiedCellCount", "已占用格数", safeSummary.occupiedCellCount, "BuildSummary.occupiedCellCount");
            Add(section, "buildSummary.mapRuleCount", "地图规则数", safeSummary.mapRuleCount, "BuildSummary.mapRuleCount");
            Add(section, "buildSummary.enemyProblemCount", "敌人题目数", safeSummary.enemyProblemCount, "BuildSummary.enemyProblemCount");
            Add(section, "buildSummary.bossProblemCount", "Boss题目数", safeSummary.bossProblemCount, "BuildSummary.bossProblemCount");
            Add(section, "buildSummary.activeSynergyCount", "激活协同数", safeSummary.activeSynergyCount, "BuildSummary.activeSynergyCount");
            Add(section, "buildSummary.activeThresholdCount", "激活阈值数", safeSummary.activeThresholdCount, "BuildSummary.activeThresholdCount");
            Add(section, "buildSummary.selectedAffixCount", "选中词缀数", safeSummary.selectedAffixCount, "BuildSummary.selectedAffixCount");
            Add(section, "buildSummary.modifierCount", "修正器数", safeSummary.modifierCount, "BuildSummary.modifierCount");
            Add(section, "buildSummary.eventCount", "事件预览数", safeSummary.eventCount, "BuildSummary.eventCount");
            Add(section, "buildSummary.featureFlagsAllDisabled", "功能开关全部关闭", safeSummary.featureFlagsAllDisabled, "BuildSummary.featureFlagsAllDisabled");
            Add(section, "buildSummary.devOnly", "开发者专用", context?.devOnly ?? true, "BuildSandboxPreviewContext.devOnly");
            Add(section, "buildSummary.isEnabled", "正式启用", context?.isEnabled ?? false, "BuildSandboxPreviewContext.isEnabled");
            Add(section, "buildSummary.readsFormalSaveData", "读取正式存档", context?.readsFormalSaveData ?? false, "BuildSandboxPreviewContext.readsFormalSaveData");
            Add(section, "buildSummary.writesFormalFlow", "写入正式流程", context?.writesFormalFlow ?? false, "BuildSandboxPreviewContext.writesFormalFlow");
            Add(section, "buildSummary.buildTags", "构筑能力标签", Join(safeSummary.buildTags), "BuildSummary.buildTags");
            Add(section, "buildSummary.activeSynergies", "激活协同ID", Join(safeSummary.activeSynergies), "BuildSummary.activeSynergies");
            Add(section, "buildSummary.activeThresholds", "激活阈值ID", Join(safeSummary.activeThresholds), "BuildSummary.activeThresholds");
        }

        private static void AddSynergySummary(BuildTuningDataPanelSection section, SynergyViewModel synergy)
        {
            SynergyViewModel safeSynergy = synergy ?? new SynergyViewModel();
            Add(section, "synergySummary.activeSynergyCount", "激活协同数", safeSynergy.activeSynergyCount, "Synergy.activeSynergyCount");
            Add(section, "synergySummary.activeThresholdCount", "激活阈值数", safeSynergy.activeThresholdCount, "Synergy.activeThresholdCount");
            Add(section, "synergySummary.placementSatisfied", "摆放条件满足", safeSynergy.placementSatisfied, "Synergy.placementSatisfied");
            Add(section, "synergySummary.energySatisfied", "供能条件满足", safeSynergy.energySatisfied, "Synergy.energySatisfied");
            Add(section, "synergySummary.missingRequirementCount", "缺口条件数", safeSynergy.missingRequirementCount, "Synergy.missingRequirementCount", maskedFromPlayer: true);
            Add(section, "synergySummary.nextThresholdHint", "下一阈值提示", safeSynergy.nextThresholdHint, "Synergy.nextThresholdHint", maskedFromPlayer: true);
            Add(section, "synergySummary.activeRows", "协同明细行", FormatSynergyRows(safeSynergy.rows), "Synergy.rows");
            Add(section, "synergySummary.missingRequirementSummaries", "缺口条件明细", Join(safeSynergy.missingRequirementSummaries), "Synergy.missingRequirementSummaries", maskedFromPlayer: true);
        }

        private static void AddShapeOccupancy(BuildTuningDataPanelSection section, ShapeOccupancyViewModel shape)
        {
            ShapeOccupancyViewModel safeShape = shape ?? new ShapeOccupancyViewModel();
            Add(section, "shapeOccupancy.placedItemCount", "已放置道具数", safeShape.placedItemCount, "ShapeOccupancy.placedItemCount");
            Add(section, "shapeOccupancy.occupiedCellCount", "已占用格数", safeShape.occupiedCellCount, "ShapeOccupancy.occupiedCellCount");
            Add(section, "shapeOccupancy.placementSampleCount", "摆放样本数", safeShape.placementSampleCount, "ShapeOccupancy.placementSampleCount");
            Add(section, "shapeOccupancy.validPlacementCount", "合法摆放样本数", safeShape.validPlacementCount, "ShapeOccupancy.validPlacementCount");
            Add(section, "shapeOccupancy.invalidPlacementCount", "非法摆放样本数", safeShape.invalidPlacementCount, "ShapeOccupancy.invalidPlacementCount");
            Add(section, "shapeOccupancy.placementLegal", "当前摆放合法", safeShape.placementLegal, "ShapeOccupancy.placementLegal");
            Add(section, "shapeOccupancy.invalidReasons", "非法原因列表", Join(safeShape.invalidReasons), "ShapeOccupancy.invalidReasons");
            Add(section, "shapeOccupancy.placedItemIds", "已放置道具ID", Join(safeShape.placedItemIds), "ShapeOccupancy.placedItemIds");
        }

        private static void AddAffixModifier(BuildTuningDataPanelSection section, AffixModifierViewModel affix)
        {
            AffixModifierViewModel safeAffix = affix ?? new AffixModifierViewModel();
            Add(section, "affixModifier.affixItemCount", "词缀道具数", safeAffix.affixItemCount, "AffixModifier.affixItemCount");
            Add(section, "affixModifier.rarityCount", "品质档位数", safeAffix.rarityCount, "AffixModifier.rarityCount");
            Add(section, "affixModifier.selectedAffixCount", "选中词缀数", safeAffix.selectedAffixCount, "AffixModifier.selectedAffixCount");
            Add(section, "affixModifier.orangeCoreAffixPresent", "橙色核心词缀存在", safeAffix.orangeCoreAffixPresent, "AffixModifier.orangeCoreAffixPresent");
            Add(section, "affixModifier.bondPlusOnePresent", "羁绊+1词缀存在", safeAffix.bondPlusOnePresent, "AffixModifier.bondPlusOnePresent");
            Add(section, "affixModifier.modifierCount", "修正器数", safeAffix.modifierCount, "AffixModifier.modifierCount");
            Add(section, "affixModifier.eventCount", "事件数", safeAffix.eventCount, "AffixModifier.eventCount");
            Add(section, "affixModifier.modifierPreviewDevOnly", "修正器预览开发者专用", safeAffix.modifierPreviewDevOnly, "AffixModifier.modifierPreviewDevOnly");
            Add(section, "affixModifier.modifierPreviewEnabled", "修正器预览启用", safeAffix.modifierPreviewEnabled, "AffixModifier.modifierPreviewEnabled");
            Add(section, "affixModifier.eventPreviewDevOnly", "事件预览开发者专用", safeAffix.eventPreviewDevOnly, "AffixModifier.eventPreviewDevOnly");
            Add(section, "affixModifier.eventPreviewEnabled", "事件预览启用", safeAffix.eventPreviewEnabled, "AffixModifier.eventPreviewEnabled");
            Add(section, "affixModifier.affectsFormalCombat", "影响正式战斗", safeAffix.affectsFormalCombat, "AffixModifier.affectsFormalCombat");
            Add(section, "affixModifier.rarityIds", "品质ID列表", Join(safeAffix.rarityIds), "AffixModifier.rarityIds");
            Add(section, "affixModifier.affixIds", "词缀ID列表", Join(safeAffix.affixIds), "AffixModifier.affixIds", maskedFromPlayer: true);
            Add(section, "affixModifier.modifierTypes", "修正器类型", Join(safeAffix.modifierTypes), "AffixModifier.modifierTypes");
            Add(section, "affixModifier.eventTypes", "事件类型", Join(safeAffix.eventTypes), "AffixModifier.eventTypes");
        }

        private static void AddProblemReadiness(
            BuildTuningDataPanelSection section,
            ProblemReadinessViewModel readiness,
            BuildProblemSeedDataset dataset)
        {
            ProblemReadinessViewModel safeReadiness = readiness ?? new ProblemReadinessViewModel();
            BuildProblemSeedDataset safeDataset = dataset ?? BuildProblemSeedDataset.CreateDefault();
            Add(section, "problemReadiness.bossReadinessCount", "Boss准备度行数", safeReadiness.bossReadinessCount, "ProblemReadiness.bossReadinessCount");
            Add(section, "problemReadiness.readyBossCount", "已准备Boss数", safeReadiness.readyBossCount, "ProblemReadiness.readyBossCount");
            Add(section, "problemReadiness.totalKeyCount", "钥匙总数", safeReadiness.totalKeyCount, "ProblemReadiness.totalKeyCount");
            Add(section, "problemReadiness.satisfiedKeyCount", "已满足钥匙数", safeReadiness.satisfiedKeyCount, "ProblemReadiness.satisfiedKeyCount");
            Add(section, "problemReadiness.failureReasonCount", "失败原因数", safeReadiness.failureReasonCount, "ProblemReadiness.failureReasonCount");
            Add(section, "problemReadiness.recommendedActionCount", "推荐动作数", safeReadiness.recommendedActionCount, "ProblemReadiness.recommendedActionCount");
            Add(section, "problemReadiness.dropBiasCount", "定向倾向数", safeReadiness.dropBiasCount, "ProblemReadiness.dropBiasCount");
            Add(section, "hardSolutionTags", "硬解标签", FormatEnemyHardSolutionTags(safeDataset), "BuildProblemSeedDataset.enemyProblems.hardSolutionTags", maskedFromPlayer: true);
            Add(section, "requiredSynergy", "必需协同", Join(CollectRequiredSynergy(safeDataset)), "BuildProblemSeedDataset.bossProblems.keyRequirements", maskedFromPlayer: true);
            Add(section, "requiredAffix", "必需词缀", Join(CollectRequiredAffix(safeDataset)), "BuildProblemSeedDataset.bossProblems.keyRequirements", maskedFromPlayer: true);
            Add(section, "requiredStats", "必需属性", Join(CollectRequiredStats(safeDataset)), "BuildProblemSeedDataset.bossProblems.requiredProblemAttributes", maskedFromPlayer: true);

            foreach (BossReadinessViewModel boss in safeReadiness.bossRows ?? new List<BossReadinessViewModel>())
            {
                string bossKey = CleanStableKeySegment(boss?.bossProblemId);
                if (string.IsNullOrWhiteSpace(bossKey))
                {
                    continue;
                }

                Add(section, $"problemReadiness.boss.{bossKey}.ready", $"Boss准备状态：{boss.displayName}", boss.ready, "ProblemReadiness.bossRows.ready");
                Add(section, $"problemReadiness.boss.{bossKey}.satisfiedKeys", $"Boss已满足钥匙：{boss.displayName}", $"{boss.satisfiedKeyCount}/{boss.keyCount}", "ProblemReadiness.bossRows.keys", maskedFromPlayer: true);
                Add(section, $"problemReadiness.boss.{bossKey}.dropBiasIds", $"Boss定向倾向：{boss.displayName}", Join(boss.dropBiasIds), "ProblemReadiness.bossRows.dropBiasIds", maskedFromPlayer: true);
                Add(section, $"problemReadiness.boss.{bossKey}.weaknessWindowIds", $"Boss弱点窗口：{boss.displayName}", Join(boss.weaknessWindowIds), "ProblemReadiness.bossRows.weaknessWindowIds", maskedFromPlayer: true);
            }
        }

        private static void AddSimulationResult(BuildTuningDataPanelSection section, SimulationResultViewModel simulation)
        {
            SimulationResultViewModel safeSimulation = simulation ?? new SimulationResultViewModel();
            Add(section, "simulationResult.scenarioCount", "模拟场景数", safeSimulation.scenarioCount, "SimulationResult.scenarioCount");
            Add(section, "simulationResult.resultCount", "模拟结果数", safeSimulation.resultCount, "SimulationResult.resultCount");
            Add(section, "simulationResult.comparisonCount", "对比项数", safeSimulation.comparisonCount, "SimulationResult.comparisonCount");
            Add(section, "simulationResult.averageWinRate", "平均胜率", safeSimulation.averageWinRate, "SimulationResult.averageWinRate");
            Add(section, "simulationResult.averageClearTimeSeconds", "平均通关秒数", safeSimulation.averageClearTimeSeconds, "SimulationResult.averageClearTimeSeconds");
            Add(section, "simulationResult.bestBuildId", "最佳构筑ID", safeSimulation.bestBuildId, "SimulationResult.bestBuildId");

            foreach (SimulationResultRow row in safeSimulation.rows ?? new List<SimulationResultRow>())
            {
                string buildKey = CleanStableKeySegment(row?.buildId);
                if (string.IsNullOrWhiteSpace(buildKey))
                {
                    continue;
                }

                Add(section, $"simulationResult.row.{buildKey}.winRate", $"胜率：{row.buildName}", row.winRate, "SimulationResult.rows.winRate");
                Add(section, $"simulationResult.row.{buildKey}.clearTimeSeconds", $"通关秒数：{row.buildName}", row.clearTimeSeconds, "SimulationResult.rows.clearTimeSeconds");
                Add(section, $"simulationResult.row.{buildKey}.shieldBreakEfficiency", $"破盾效率：{row.buildName}", row.shieldBreakEfficiency, "SimulationResult.rows.shieldBreakEfficiency");
                Add(section, $"simulationResult.row.{buildKey}.triggeredEventCount", $"触发事件数：{row.buildName}", row.triggeredEventCount, "SimulationResult.rows.triggeredEventCount");
                Add(section, $"simulationResult.row.{buildKey}.failureReason", $"失败原因：{row.buildName}", row.failureReason, "SimulationResult.rows.failureReason");
            }
        }

        private static void AddDropBiasPreview(
            BuildTuningDataPanelSection section,
            BuildProblemSeedDataset dataset)
        {
            BuildProblemSeedDataset safeDataset = dataset ?? BuildProblemSeedDataset.CreateDefault();
            List<DropBiasSeed> drops = safeDataset.dropBiases ?? new List<DropBiasSeed>();
            Add(section, "dropBiasPreview.dropBiasCount", "定向倾向条目数", drops.Count, "BuildProblemSeedDataset.dropBiases");
            Add(section, "dropBiasWeights", "定向倾向权重", FormatDropBiasWeights(drops), "BuildProblemSeedDataset.dropBiases.previewWeight", maskedFromPlayer: true);
            Add(section, "dropBiasPreview.targetBuildTags", "定向构筑标签", Join(drops.SelectMany(drop => drop?.targetBuildTags ?? new List<string>())), "BuildProblemSeedDataset.dropBiases.targetBuildTags", maskedFromPlayer: true);
            Add(section, "dropBiasPreview.targetItemTags", "定向道具标签", Join(drops.SelectMany(drop => drop?.targetItemTags ?? new List<string>())), "BuildProblemSeedDataset.dropBiases.targetItemTags", maskedFromPlayer: true);
            Add(section, "dropBiasPreview.targetAffixIds", "定向词缀ID", Join(drops.SelectMany(drop => drop?.targetAffixIds ?? new List<string>())), "BuildProblemSeedDataset.dropBiases.targetAffixIds", maskedFromPlayer: true);

            foreach (DropBiasSeed drop in drops.Where(drop => drop != null))
            {
                string dropKey = CleanStableKeySegment(drop.dropBiasId);
                Add(section, $"dropBiasPreview.{dropKey}.previewWeight", $"定向倾向权重：{drop.displayName}", drop.previewWeight, "DropBias.previewWeight", maskedFromPlayer: true);
                Add(section, $"dropBiasPreview.{dropKey}.targetBuildTags", $"定向构筑标签：{drop.displayName}", Join(drop.targetBuildTags), "DropBias.targetBuildTags", maskedFromPlayer: true);
                Add(section, $"dropBiasPreview.{dropKey}.targetItemTags", $"定向道具标签：{drop.displayName}", Join(drop.targetItemTags), "DropBias.targetItemTags", maskedFromPlayer: true);
                Add(section, $"dropBiasPreview.{dropKey}.targetAffixIds", $"定向词缀ID：{drop.displayName}", Join(drop.targetAffixIds), "DropBias.targetAffixIds", maskedFromPlayer: true);
            }
        }

        private static void AddBossSixKeyFullAnswer(
            BuildTuningDataPanelSection section,
            BuildProblemSeedDataset dataset)
        {
            BuildProblemSeedDataset safeDataset = dataset ?? BuildProblemSeedDataset.CreateDefault();
            List<BossProblemSeed> bosses = safeDataset.bossProblems ?? new List<BossProblemSeed>();
            Add(section, "bossSixKeyFullAnswer", "Boss六钥匙完整答案", FormatBossFullAnswers(bosses), "BuildProblemSeedDataset.bossProblems.keyRequirements", maskedFromPlayer: true);
            Add(section, "bossSixKeyFullAnswer.bossCount", "Boss答案组数", bosses.Count, "BuildProblemSeedDataset.bossProblems", maskedFromPlayer: true);
            Add(section, "bossSixKeyFullAnswer.totalKeyCount", "Boss钥匙总数", bosses.Sum(boss => boss?.keyRequirements?.Count ?? 0), "BuildProblemSeedDataset.bossProblems.keyRequirements", maskedFromPlayer: true);

            foreach (BossProblemSeed boss in bosses.Where(boss => boss != null))
            {
                string bossKey = CleanStableKeySegment(boss.bossProblemId);
                Add(section, $"bossSixKeyFullAnswer.{bossKey}", $"Boss完整钥匙答案：{boss.displayName}", FormatBossKeys(boss), "BossProblemSeed.keyRequirements", maskedFromPlayer: true);
                Add(section, $"bossSixKeyFullAnswer.{bossKey}.requiredAttributes", $"Boss必需属性：{boss.displayName}", Join(boss.requiredProblemAttributes), "BossProblemSeed.requiredProblemAttributes", maskedFromPlayer: true);
                Add(section, $"bossSixKeyFullAnswer.{bossKey}.minimumKeysRequired", $"Boss最低钥匙数：{boss.displayName}", boss.minimumKeysRequired, "BossProblemSeed.minimumKeysRequired", maskedFromPlayer: true);
            }
        }

        private static void Add(
            BuildTuningDataPanelSection section,
            string englishStableKey,
            string chineseDisplayName,
            object value,
            string sourceDataPath,
            bool maskedFromPlayer = false,
            string note = "")
        {
            if (section == null)
            {
                return;
            }

            section.rows.Add(new BuildTuningDataPanelFieldRow
            {
                englishStableKey = englishStableKey ?? string.Empty,
                chineseDisplayName = chineseDisplayName ?? string.Empty,
                value = FormatValue(value),
                sourceDataPath = sourceDataPath ?? string.Empty,
                dataPanelSlot = section.dataPanelSlot,
                note = note ?? string.Empty,
                developerVisible = true,
                playerVisible = false,
                maskedFromPlayer = maskedFromPlayer,
                reportOnly = true,
                devOnly = true
            });
        }

        private static string FormatSynergyRows(IEnumerable<SynergyPreviewRow> rows)
        {
            return string.Join(
                "; ",
                (rows ?? Enumerable.Empty<SynergyPreviewRow>())
                .Where(row => row != null)
                .Select(row => $"{row.synergyId}:{row.matchedCount} [{Join(row.activeThresholds)}]"));
        }

        private static string FormatEnemyHardSolutionTags(BuildProblemSeedDataset dataset)
        {
            return string.Join(
                "; ",
                (dataset.enemyProblems ?? new List<EnemyProblemSeed>())
                .Where(problem => problem != null)
                .Select(problem => $"{problem.problemType}={Join(problem.hardSolutionTags)}"));
        }

        private static List<string> CollectRequiredSynergy(BuildProblemSeedDataset dataset)
        {
            return Clean((dataset.bossProblems ?? new List<BossProblemSeed>())
                .SelectMany(boss => boss?.keyRequirements ?? new List<BossProblemKeySeed>())
                .Where(key => key != null
                    && (Contains(key.keyId, "synergy")
                        || Contains(key.keyCategory, "羁绊")
                        || Contains(key.requirementId, "jing_")
                        || Contains(key.requirementId, "li_huo")
                        || Contains(key.requirementId, "hu_zhen")
                        || Contains(key.requirementId, "ju_neng")))
                .Select(key => key.requirementId));
        }

        private static List<string> CollectRequiredAffix(BuildProblemSeedDataset dataset)
        {
            return Clean((dataset.bossProblems ?? new List<BossProblemSeed>())
                .SelectMany(boss => boss?.keyRequirements ?? new List<BossProblemKeySeed>())
                .Where(key => key != null
                    && (Contains(key.keyId, "affix") || Contains(key.requirementId, "affix")))
                .Select(key => key.requirementId)
                .Concat((dataset.dropBiases ?? new List<DropBiasSeed>())
                    .SelectMany(drop => drop?.targetAffixIds ?? new List<string>())));
        }

        private static List<string> CollectRequiredStats(BuildProblemSeedDataset dataset)
        {
            return Clean((dataset.bossProblems ?? new List<BossProblemSeed>())
                .Where(boss => boss != null)
                .SelectMany(boss => (boss.requiredProblemAttributes ?? new List<string>())
                    .Concat((boss.keyRequirements ?? new List<BossProblemKeySeed>())
                        .Select(key => key?.problemAttribute))));
        }

        private static string FormatDropBiasWeights(IEnumerable<DropBiasSeed> drops)
        {
            return string.Join(
                "; ",
                (drops ?? Enumerable.Empty<DropBiasSeed>())
                .Where(drop => drop != null)
                .Select(drop => $"{drop.dropBiasId}={drop.previewWeight.ToString("0.###", CultureInfo.InvariantCulture)}"));
        }

        private static string FormatBossFullAnswers(IEnumerable<BossProblemSeed> bosses)
        {
            return string.Join(
                " || ",
                (bosses ?? Enumerable.Empty<BossProblemSeed>())
                .Where(boss => boss != null)
                .Select(boss => $"{boss.bossProblemId}: {FormatBossKeys(boss)}"));
        }

        private static string FormatBossKeys(BossProblemSeed boss)
        {
            return string.Join(
                "; ",
                (boss?.keyRequirements ?? new List<BossProblemKeySeed>())
                .Where(key => key != null)
                .Select(key =>
                    $"{key.keyId}({key.keyCategory}) requirement={key.requirementId}, attribute={key.problemAttribute}, score={key.requiredScore}"));
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => string.Empty,
                bool boolValue => boolValue ? "True" : "False",
                int intValue => intValue.ToString(CultureInfo.InvariantCulture),
                float floatValue => floatValue.ToString("0.####", CultureInfo.InvariantCulture),
                double doubleValue => doubleValue.ToString("0.####", CultureInfo.InvariantCulture),
                _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
            };
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join("; ", Clean(values));
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static bool Contains(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(token)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string CleanStableKeySegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            char[] chars = value
                .Trim()
                .Select(character => char.IsLetterOrDigit(character) || character == '_' || character == '-'
                    ? character
                    : '_')
                .ToArray();
            return new string(chars);
        }
    }
}
