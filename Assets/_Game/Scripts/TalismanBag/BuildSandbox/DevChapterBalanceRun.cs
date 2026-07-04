using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class DevChapterBalanceRun
    {
        public const string PackageName = "V0.4-DevChapterBalanceRun01";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsDevOnlyBattleSandboxSnapshots = true;
        public bool readsBuildProblemSeedData = true;
        public bool readsEnemyBossValidationPool = true;
        public bool readsBuildCombatPreview = true;
        public bool readsShapeBuildRulePreview = true;
        public bool writesDeveloperDataPanelFields = true;
        public bool writesReports = true;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsFullAnswers;
        public bool createsFormalChapterEntry;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool advancesChapterProgress;
        public bool opensFeatureFlag;
        public bool touchesV02OrV03Scene;
        public bool touchesCurrentV04RectTransform;
        public List<DevChapterBalanceRunStage> stages = new();

        public int StageCount => stages?.Count(stage => stage != null) ?? 0;
        public int Chapter310StageCount => CountChapter("3-10");
        public int Chapter410StageCount => CountChapter("4-10");
        public int EasyStageCount => CountDifficulty(DevChapterBalanceRunBuilder.DifficultyTooEasy);
        public int FitStageCount => CountDifficulty(DevChapterBalanceRunBuilder.DifficultyFit);
        public int HardStageCount => CountDifficulty(DevChapterBalanceRunBuilder.DifficultyTooHard);
        public int PlayerSideAnswerLeakCount => stages?.Count(stage => stage == null || stage.PlayerSideAnswerLeak) ?? 1;
        public int FormalFlowLeakCount => CountFormalLeaks();
        public int FeatureFlagDefaultTrueCount => BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);

        public bool DevOnlyIsolationPass =>
            devOnly
            && !isEnabled
            && readsDevOnlyBattleSandboxSnapshots
            && readsBuildProblemSeedData
            && readsEnemyBossValidationPool
            && readsBuildCombatPreview
            && readsShapeBuildRulePreview
            && writesDeveloperDataPanelFields
            && writesReports
            && playerUiChineseOnly
            && !playerUiShowsFullAnswers
            && PlayerSideAnswerLeakCount == 0
            && FormalFlowLeakCount == 0
            && FeatureFlagDefaultTrueCount == 0;

        private int CountChapter(string label)
        {
            return stages?.Count(stage => stage != null
                && string.Equals(stage.devChapterLabel, label, StringComparison.Ordinal)) ?? 0;
        }

        private int CountDifficulty(string difficulty)
        {
            return stages?.Count(stage => stage != null
                && string.Equals(stage.difficultyTendencyChinese, difficulty, StringComparison.Ordinal)) ?? 0;
        }

        private int CountFormalLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!readsDevOnlyBattleSandboxSnapshots) leaks++;
            if (!readsBuildProblemSeedData) leaks++;
            if (!readsEnemyBossValidationPool) leaks++;
            if (!readsBuildCombatPreview) leaks++;
            if (!readsShapeBuildRulePreview) leaks++;
            if (!writesDeveloperDataPanelFields) leaks++;
            if (!writesReports) leaks++;
            if (!playerUiChineseOnly) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            if (createsFormalChapterEntry) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (writesFormalReward) leaks++;
            if (advancesChapterProgress) leaks++;
            if (opensFeatureFlag) leaks++;
            if (touchesV02OrV03Scene) leaks++;
            if (touchesCurrentV04RectTransform) leaks++;
            leaks += stages?.Count(stage => stage == null || stage.FormalFlowLeak) ?? 1;
            leaks += FeatureFlagDefaultTrueCount;
            return leaks;
        }
    }

    [Serializable]
    public sealed class DevChapterBalanceRunStage
    {
        public string stageId = string.Empty;
        public string devChapterLabel = string.Empty;
        public string chapterLabelChinese = string.Empty;
        public string previewBuildId = string.Empty;
        public string recommendedTestTargetChinese = string.Empty;
        public string currentBuildFeedbackChinese = string.Empty;
        public string bossMechanicFeedbackChinese = string.Empty;
        public string difficultyTendencyChinese = string.Empty;
        public string tuningSuggestionChinese = string.Empty;
        public string playerBattleFeedbackChinese = string.Empty;
        public string mapRuleId = string.Empty;
        public string enemyProblemId = string.Empty;
        public string bossProblemId = string.Empty;
        public string bossProblemDisplayNameChinese = string.Empty;
        public string bossProfileId = string.Empty;
        public string shapeRuleKey = string.Empty;
        public string shapeRuleNameChinese = string.Empty;
        public string buildProblemSourcePath = "BuildProblemSeedData";
        public string enemyBossSourcePath = "EnemyBossValidationPool";
        public string buildCombatPreviewSourcePath = "BattleSandboxBuildCombatPreview";
        public string shapeRulePreviewSourcePath = "BattleSandboxShapeBuildRulePreview";
        public int placedItemSnapshotCount;
        public int activeSynergyCount;
        public int modifierBundleCount;
        public int effectEventCount;
        public int shapeBuildRuleMatchCount;
        public int bossReadinessCount;
        public int readyBossCount;
        public int readinessSatisfiedKeyCount;
        public int readinessTotalKeyCount;
        public float readinessRatio;
        public float simulatedWinRate;
        public float simulatedClearTimeSeconds;
        public float shieldBreakEfficiency;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerVisible = true;
        public bool developerPanelVisible = true;
        public bool playerShowsCompleteAnswer;
        public bool createsFormalChapterEntry;
        public bool runsFormalCombat;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool advancesChapterProgress;
        public bool opensFeatureFlag;
        public bool touchesV02OrV03Scene;
        public bool touchesCurrentV04RectTransform;
        public BattleSandboxBuildCombatPreview combatPreview = new();
        public BattleSandboxShapeBuildRulePreview shapeBuildRulePreview = new();
        public BuildSimulationScenario simulationScenario = new();
        public BuildSimulationResult simulationResult = new();
        public List<DevChapterBalanceDeveloperPanelField> developerPanelFields = new();

        public bool PlayerSideAnswerLeak =>
            PlayerTextFields().Any(value =>
                string.IsNullOrWhiteSpace(value)
                || ContainsLatin(value)
                || ContainsForbiddenPlayerToken(value));

        public bool FormalFlowLeak =>
            !devOnly
            || isEnabled
            || !playerVisible
            || !developerPanelVisible
            || playerShowsCompleteAnswer
            || createsFormalChapterEntry
            || runsFormalCombat
            || writesFormalFlow
            || writesFormalSaveData
            || writesFormalReward
            || advancesChapterProgress
            || opensFeatureFlag
            || touchesV02OrV03Scene
            || touchesCurrentV04RectTransform
            || (combatPreview == null || combatPreview.FormalFlowLeakCount > 0)
            || (shapeBuildRulePreview == null || shapeBuildRulePreview.FormalLeakCount > 0)
            || (simulationScenario == null || !simulationScenario.devOnly || simulationScenario.isEnabled || simulationScenario.entersFormalFlow)
            || (simulationResult == null || !simulationResult.devOnly || simulationResult.isEnabled || simulationResult.entersFormalFlow || simulationResult.affectsFormalCombat)
            || (developerPanelFields == null
                || developerPanelFields.Count == 0
                || developerPanelFields.Any(field => field == null || field.PlayerLeak));

        private IEnumerable<string> PlayerTextFields()
        {
            yield return chapterLabelChinese;
            yield return recommendedTestTargetChinese;
            yield return currentBuildFeedbackChinese;
            yield return bossMechanicFeedbackChinese;
            yield return difficultyTendencyChinese;
            yield return tuningSuggestionChinese;
            yield return playerBattleFeedbackChinese;
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string[] tokens =
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
                "BuildProblemSeedData",
                "BuildProblemSeedDataset",
                "EnemyBossValidationPool",
                "bossSixKeyFullAnswer",
                "solution",
                "required",
                "answer",
                "答案",
                "解法",
                "权重",
                "六钥匙",
                "题目",
                "准备度"
            };
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    [Serializable]
    public sealed class DevChapterBalanceDeveloperPanelField
    {
        public string fieldKey = string.Empty;
        public string labelChinese = string.Empty;
        public string value = string.Empty;
        public string sourceDataPath = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool developerVisible = true;
        public bool playerVisible;
        public bool maskedFromPlayer = true;

        public bool PlayerLeak =>
            !devOnly
            || isEnabled
            || !developerVisible
            || playerVisible
            || !maskedFromPlayer;
    }

    public static class DevChapterBalanceRunBuilder
    {
        public const string DifficultyTooEasy = "过易";
        public const string DifficultyFit = "合适";
        public const string DifficultyTooHard = "过难";

        private const string BuildProblemSource = "BuildProblemSeedData.CreateDefault";
        private const string EnemyBossSource = "EnemyBossValidationPool.CreateDefault";
        private const string BuildCombatPreviewSource = "BattleSandboxBuildCombatPreviewBuilder.Build";
        private const string ShapeRulePreviewSource = "BattleSandboxShapeBuildRulePreviewBuilder.Evaluate";

        public static DevChapterBalanceRun BuildDefaultRun()
        {
            BuildProblemSeedDataset problemSeedDataset = BuildProblemSeedDataset.CreateDefault();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();

            DevChapterBalanceRun run = new()
            {
                packageName = DevChapterBalanceRun.PackageName,
                devOnly = true,
                isEnabled = false,
                readsDevOnlyBattleSandboxSnapshots = true,
                readsBuildProblemSeedData = true,
                readsEnemyBossValidationPool = true,
                readsBuildCombatPreview = true,
                readsShapeBuildRulePreview = true,
                writesDeveloperDataPanelFields = true,
                writesReports = true,
                playerUiChineseOnly = true,
                playerUiShowsFullAnswers = false,
                createsFormalChapterEntry = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                advancesChapterProgress = false,
                opensFeatureFlag = false,
                touchesV02OrV03Scene = false,
                touchesCurrentV04RectTransform = false
            };

            foreach (StageSpec spec in CreateDefaultStageSpecs())
            {
                run.stages.Add(BuildStage(spec, problemSeedDataset, enemyBossPool));
            }

            return run;
        }

        private static DevChapterBalanceRunStage BuildStage(
            StageSpec spec,
            BuildProblemSeedDataset problemSeedDataset,
            EnemyBossValidationPool enemyBossPool)
        {
            BuildSandboxLayoutSnapshot snapshot = spec.CreateSnapshot();
            BattleSandboxBuildCombatPreview combatPreview =
                BattleSandboxBuildCombatPreviewBuilder.Build(snapshot, spec.previewBuildId, usesCurrentBoardSnapshot: true);
            BattleSandboxShapeBuildRulePreview shapePreview =
                combatPreview.shapeBuildRulePreview ?? BattleSandboxShapeBuildRulePreviewBuilder.Evaluate(snapshot);
            BattleSandboxShapeBuildRuleMatch shapeMatch = FindShapeMatch(shapePreview, spec.shapeRuleKey);
            BossProblemSeed bossProblem = problemSeedDataset.FindBoss(spec.bossProblemId);
            MapRuleSeed mapRule = (problemSeedDataset.mapRules ?? new List<MapRuleSeed>()).FirstOrDefault(rule =>
                string.Equals(rule.mapRuleId, spec.mapRuleId, StringComparison.Ordinal));
            EnemyProblemSeed enemyProblem = (problemSeedDataset.enemyProblems ?? new List<EnemyProblemSeed>()).FirstOrDefault(problem =>
                string.Equals(problem.problemType, spec.enemyProblemId, StringComparison.Ordinal));
            BuildSandboxBossProfile bossProfile = enemyBossPool.FindBoss(spec.bossProfileId);
            BossReadinessViewModel readinessRow = FindReadinessRow(combatPreview, spec.bossProblemId);
            BuildSimulationScenario scenario = BuildSimulationScenario(spec, combatPreview, bossProfile, mapRule);
            BuildSimulationResult simulationResult = BuildSimulationRunner.RunSingle(scenario);

            int satisfiedKeyCount = readinessRow?.satisfiedKeyCount ?? 0;
            int totalKeyCount = readinessRow?.keyCount ?? 0;
            float readinessRatio = totalKeyCount <= 0 ? 0f : Round01(satisfiedKeyCount / (float)totalKeyCount);
            string difficulty = ResolveDifficulty(spec, readinessRow, simulationResult, readinessRatio);
            string buildFeedback = ResolveBuildFeedback(shapeMatch, combatPreview);
            string bossFeedback = ResolveBossFeedback(
                bossProblem,
                bossProfile,
                mapRule,
                readinessRow,
                simulationResult,
                spec.fallbackBossNameChinese);
            string tuningSuggestion = ResolveTuningSuggestion(difficulty, spec, readinessRow, simulationResult);

            DevChapterBalanceRunStage stage = new()
            {
                stageId = spec.stageId,
                devChapterLabel = spec.devChapterLabel,
                chapterLabelChinese = spec.chapterLabelChinese,
                previewBuildId = spec.previewBuildId,
                recommendedTestTargetChinese = spec.recommendedTestTargetChinese,
                currentBuildFeedbackChinese = buildFeedback,
                bossMechanicFeedbackChinese = bossFeedback,
                difficultyTendencyChinese = difficulty,
                tuningSuggestionChinese = tuningSuggestion,
                playerBattleFeedbackChinese = ResolvePlayerBattleFeedback(buildFeedback, bossFeedback, difficulty),
                mapRuleId = mapRule?.mapRuleId ?? spec.mapRuleId,
                enemyProblemId = enemyProblem?.problemType ?? spec.enemyProblemId,
                bossProblemId = bossProblem?.bossProblemId ?? spec.bossProblemId,
                bossProblemDisplayNameChinese = bossProblem?.displayName ?? spec.fallbackBossNameChinese,
                bossProfileId = bossProfile?.bossId ?? spec.bossProfileId,
                shapeRuleKey = shapeMatch?.englishStableKey ?? spec.shapeRuleKey,
                shapeRuleNameChinese = shapeMatch?.chineseDisplayName ?? spec.shapeRuleNameChinese,
                buildProblemSourcePath = BuildProblemSource,
                enemyBossSourcePath = EnemyBossSource,
                buildCombatPreviewSourcePath = BuildCombatPreviewSource,
                shapeRulePreviewSourcePath = ShapeRulePreviewSource,
                placedItemSnapshotCount = combatPreview.PlacedItemSnapshotCount,
                activeSynergyCount = combatPreview.SynergyMatchCount,
                modifierBundleCount = combatPreview.ModifierBundleCount,
                effectEventCount = combatPreview.EffectEventCount,
                shapeBuildRuleMatchCount = shapePreview.MatchedRuleCount,
                bossReadinessCount = combatPreview.BossReadinessCount,
                readyBossCount = combatPreview.ReadyBossCount,
                readinessSatisfiedKeyCount = satisfiedKeyCount,
                readinessTotalKeyCount = totalKeyCount,
                readinessRatio = readinessRatio,
                simulatedWinRate = simulationResult.simulatedWinRate,
                simulatedClearTimeSeconds = simulationResult.simulatedClearTimeSeconds,
                shieldBreakEfficiency = simulationResult.shieldBreakEfficiency,
                devOnly = true,
                isEnabled = false,
                playerVisible = true,
                developerPanelVisible = true,
                playerShowsCompleteAnswer = false,
                createsFormalChapterEntry = false,
                runsFormalCombat = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                advancesChapterProgress = false,
                opensFeatureFlag = false,
                touchesV02OrV03Scene = false,
                touchesCurrentV04RectTransform = false,
                combatPreview = combatPreview,
                shapeBuildRulePreview = shapePreview,
                simulationScenario = scenario,
                simulationResult = simulationResult
            };
            stage.developerPanelFields = BuildDeveloperFields(stage, readinessRow, shapeMatch, bossProfile).ToList();
            return stage;
        }

        private static IReadOnlyList<StageSpec> CreateDefaultStageSpecs()
        {
            return new[]
            {
                new StageSpec(
                    "dev_balance_3_10_guard_wall",
                    "3-10",
                    "三之十验证",
                    "dev_balance_3_10_guard_wall",
                    "验证竖向护阵墙能否稳定挡住连续敲阵。",
                    "dev_map_copper_bell_night",
                    "dev_enemy_caster_problem",
                    "dev_boss_bronze_formation_general",
                    "叩阵铜将",
                    "dev_boss_burst_huzhen",
                    "vertical_defense_wall",
                    "竖向护阵墙",
                    0.12f,
                    "上调敲阵频率或缩短首轮缓冲，观察护阵是否仍能稳住。",
                    "维持当前敲阵压力，重点观察护阵浮字与读条节奏。",
                    "降低连续敲阵密度，先保证护阵反馈能被稳定看见。",
                    BuildGuardWallSnapshot),
                new StageSpec(
                    "dev_balance_3_10_cleanse_corner",
                    "3-10",
                    "三之十验证",
                    "dev_balance_3_10_cleanse_corner",
                    "验证拐角净化阵面对污染扩散时的反馈强度。",
                    "dev_map_bluestone_damp",
                    "dev_enemy_polluted_tile_problem",
                    "dev_boss_dirty_dream_mother",
                    "秽签梦母",
                    "dev_boss_debuff_jinge",
                    "corner_cleanse_array",
                    "拐角净化阵",
                    -0.05f,
                    "提高污染扩散速度或延后净化窗口，避免测试过早结束。",
                    "保持污染节奏，继续比较净化浮字与首领状态变化。",
                    "降低污染叠加速度，或给测试构筑补一段供能缓冲。",
                    BuildCleanseCornerSnapshot),
                new StageSpec(
                    "dev_balance_4_10_furnace_core",
                    "4-10",
                    "四之十验证",
                    "dev_balance_4_10_furnace_core",
                    "验证炉芯核心阵能否撑住供能干扰和短窗爆发。",
                    "dev_map_furnace_ash_fall",
                    "dev_enemy_spirit_thief_problem",
                    "dev_boss_spirit_thief_core",
                    "偷灵炉心",
                    "dev_boss_energy_juneng",
                    "furnace_core_array",
                    "炉芯核心阵",
                    0.08f,
                    "增加偷灵间隔压迫，检查炉芯供能是否仍然过稳。",
                    "维持当前偷灵节奏，继续观察炉芯核心阵反馈差异。",
                    "降低偷灵频率，或把首轮干扰延后到第二轮后。",
                    BuildFurnaceCoreSnapshot),
                new StageSpec(
                    "dev_balance_4_10_thunder_fire_cross",
                    "4-10",
                    "四之十验证",
                    "dev_balance_4_10_thunder_fire_cross",
                    "验证雷火交错阵面对复合阵眼时是否短板过大。",
                    "dev_map_bluestone_crack",
                    "dev_enemy_formation_eye_problem",
                    "dev_boss_black_furnace_complex_eye",
                    "黑炉复合阵眼",
                    "dev_boss_hybrid_combo",
                    "thunder_fire_cross_array",
                    "雷火交错阵",
                    0.2f,
                    "提高复合阵眼耐压或压缩爆发窗口，确认输出不会过早碾压。",
                    "保持当前复合压力，继续观察雷火反馈与短板提示是否清楚。",
                    "降低复合阵眼前两轮压力，或补一件护阵测试件再验证。",
                    BuildThunderFireCrossSnapshot)
            };
        }

        private static BuildSandboxLayoutSnapshot BuildGuardWallSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(Item("preview_energy_incense", "Vertical2", Cell(0, 0), Cell(0, 1)));
            snapshot.placedItems.Add(Item("preview_guard_wood", "Vertical2", Cell(0, 2), Cell(0, 3)));
            snapshot.placedItems.Add(Item("preview_x2_wood_talisman", "Vertical2", Cell(1, 1), Cell(1, 2)));
            snapshot.placedItems.Add(Item("preview_old_bell", "Single1", Cell(1, 3)));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BuildSandboxLayoutSnapshot BuildCleanseCornerSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(Item("preview_energy_incense", "Vertical2", Cell(1, 0), Cell(1, 1)));
            snapshot.placedItems.Add(Item("preview_cleanse_corner", "Corner3", Cell(2, 1), Cell(3, 1), Cell(2, 2)));
            snapshot.placedItems.Add(Item("preview_soul_seal", "Single1", Cell(1, 2)));
            snapshot.placedItems.Add(Item("preview_guard_wood", "Vertical2", Cell(3, 2), Cell(3, 3)));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BuildSandboxLayoutSnapshot BuildFurnaceCoreSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(Item("preview_energy_incense", "Vertical2", Cell(1, 1), Cell(1, 2)));
            snapshot.placedItems.Add(Item("preview_stone_core", "Square4", Cell(2, 1), Cell(3, 1), Cell(2, 2), Cell(3, 2)));
            snapshot.placedItems.Add(Item("preview_fire_talisman", "Single1", Cell(2, 3)));
            snapshot.placedItems.Add(Item("preview_thunder_sword", "Single1", Cell(3, 3)));
            snapshot.placedItems.Add(Item("preview_old_bell", "Single1", Cell(1, 3)));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BuildSandboxLayoutSnapshot BuildThunderFireCrossSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            snapshot.placedItems.Add(Item("preview_fire_talisman", "Single1", Cell(2, 2)));
            snapshot.placedItems.Add(Item("preview_thunder_sword", "Single1", Cell(3, 2)));
            snapshot.placedItems.Add(Item("preview_energy_incense", "Vertical2", Cell(2, 3), Cell(2, 4)));
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        private static BuildSandboxPlacedItemSnapshot Item(
            string itemId,
            string shapeId,
            params ItemShapeCell[] cells)
        {
            return BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                itemId,
                shapeId,
                ItemShapeRotation.Rotation0,
                cells);
        }

        private static ItemShapeCell Cell(int x, int y)
        {
            return new ItemShapeCell(x, y);
        }

        private static BattleSandboxShapeBuildRuleMatch FindShapeMatch(
            BattleSandboxShapeBuildRulePreview preview,
            string shapeRuleKey)
        {
            return (preview?.matches ?? new List<BattleSandboxShapeBuildRuleMatch>())
                .FirstOrDefault(match => match != null
                    && string.Equals(match.englishStableKey, shapeRuleKey, StringComparison.Ordinal));
        }

        private static BossReadinessViewModel FindReadinessRow(
            BattleSandboxBuildCombatPreview preview,
            string bossProblemId)
        {
            return (preview?.context?.viewModel?.problemReadiness?.bossRows
                    ?? new List<BossReadinessViewModel>())
                .FirstOrDefault(row => row != null
                    && string.Equals(row.bossProblemId, bossProblemId, StringComparison.Ordinal));
        }

        private static BuildSimulationScenario BuildSimulationScenario(
            StageSpec spec,
            BattleSandboxBuildCombatPreview combatPreview,
            BuildSandboxBossProfile bossProfile,
            MapRuleSeed mapRule)
        {
            BuildSandboxPreviewContext context = combatPreview?.context ?? new BuildSandboxPreviewContext();
            List<string> validationTags = new();
            validationTags.AddRange(bossProfile?.validationTags ?? new List<string>());
            validationTags.AddRange(mapRule?.affectedTags ?? new List<string>());
            validationTags.Add(spec.devChapterLabel);
            validationTags.Add(spec.shapeRuleKey);

            return new BuildSimulationScenario
            {
                buildId = spec.previewBuildId,
                displayName = spec.stageId,
                devOnly = true,
                isEnabled = false,
                enemyType = bossProfile?.bossMechanic ?? "devOnly_balance_boss",
                bossMechanic = bossProfile?.bossMechanic ?? "devOnly_balance_mechanic",
                itemRarity = "sandbox_mixed",
                buildItemIds = context.layoutSnapshot?.placedItems?
                    .Select(item => item?.itemId)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                affixCombination = context.affixEvaluation?.affixIds?
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                energyCondition = "devOnly_balance_energy_snapshot",
                placementRelation = spec.shapeRuleKey,
                enemyProfileId = string.Empty,
                enemyChineseRole = string.Empty,
                bossProfileId = bossProfile?.bossId ?? spec.bossProfileId,
                bossChineseRole = bossProfile?.chineseRole ?? string.Empty,
                validationTags = validationTags
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                recommendedSynergies = bossProfile?.recommendedSynergies?
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToList() ?? new List<string>(),
                simulatorReadable = bossProfile == null || bossProfile.simulatorReadable,
                entersFormalFlow = false,
                batchSeed = spec.stageId.GetHashCode(),
                layoutSnapshot = context.layoutSnapshot ?? new BuildSandboxLayoutSnapshot(),
                buildEvaluation = context.buildEvaluation ?? new BuildEvaluationResult(),
                affixEvaluation = context.affixEvaluation ?? new AffixRarityEvaluationResult(),
                modifierBundle = context.modifierBundle ?? new CombatModifierBundle(),
                eventBundle = context.eventBundle ?? new EffectEventBundle(),
                referencesFormalEnemyPool = false,
                referencesFormalBossPool = false,
                readsFormalPlayerData = false,
                notes = "devOnly balance run estimate; disabled; no formal chapter entry."
            };
        }

        private static string ResolveDifficulty(
            StageSpec spec,
            BossReadinessViewModel readinessRow,
            BuildSimulationResult simulationResult,
            float readinessRatio)
        {
            bool ready = readinessRow?.ready ?? false;
            float winRate = simulationResult?.simulatedWinRate ?? 0f;
            float pressureAdjustedWinRate = winRate - spec.pressureBias;

            if (!ready || readinessRatio < 0.5f || pressureAdjustedWinRate < 0.34f)
            {
                return DifficultyTooHard;
            }

            if (readinessRatio >= 0.85f && pressureAdjustedWinRate >= 0.56f)
            {
                return DifficultyTooEasy;
            }

            return DifficultyFit;
        }

        private static string ResolveBuildFeedback(
            BattleSandboxShapeBuildRuleMatch shapeMatch,
            BattleSandboxBuildCombatPreview combatPreview)
        {
            string shapeLog = shapeMatch != null && shapeMatch.isMatched
                ? shapeMatch.combatLogLineChinese
                : "【阵势】目标阵形尚未稳定触发。";
            string boardLog = (combatPreview?.rows ?? new List<BattleSandboxBuildCombatPreviewRow>())
                .FirstOrDefault(row => row != null
                    && string.Equals(row.feedbackKind, BattleSandboxEnemyCombatFeedbackKinds.BossState, StringComparison.Ordinal))
                ?.combatLogLineChinese;
            if (string.IsNullOrWhiteSpace(boardLog))
            {
                boardLog = "【反馈】当前构筑已写入战斗预览。";
            }

            return $"{shapeLog}；{boardLog}";
        }

        private static string ResolveBossFeedback(
            BossProblemSeed bossProblem,
            BuildSandboxBossProfile bossProfile,
            MapRuleSeed mapRule,
            BossReadinessViewModel readinessRow,
            BuildSimulationResult simulationResult,
            string fallbackBossNameChinese)
        {
            string bossName = ChineseOnly(bossProblem?.displayName, fallbackBossNameChinese);

            string mapPressure = ChineseOnly(
                mapRule?.warningText,
                "场地会干扰供能与摆放，请观察阵面节奏。");

            bool ready = readinessRow?.ready ?? false;
            string readinessText = ready
                ? "破绽窗口已经出现"
                : "短板会在读条后被放大";
            string timingText = (simulationResult?.simulatedWinRate ?? 0f) >= 0.52f
                ? "战斗节奏仍可观察"
                : "战斗节奏偏紧";

            return $"{bossName}：{readinessText}，{timingText}。{mapPressure}";
        }

        private static string ChineseOnly(string value, string fallback)
        {
            string safeFallback = string.IsNullOrWhiteSpace(fallback) ? "测试首领" : fallback;
            if (string.IsNullOrWhiteSpace(value) || ContainsLatin(value) || ContainsForbiddenPlayerToken(value))
            {
                return safeFallback;
            }

            return value;
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string[] tokens =
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
                "BuildProblemSeedData",
                "BuildProblemSeedDataset",
                "EnemyBossValidationPool",
                "bossSixKeyFullAnswer",
                "solution",
                "required",
                "answer",
                "答案",
                "解法",
                "权重",
                "六钥匙",
                "题目",
                "准备度"
            };
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string ResolveTuningSuggestion(
            string difficulty,
            StageSpec spec,
            BossReadinessViewModel readinessRow,
            BuildSimulationResult simulationResult)
        {
            if (string.Equals(difficulty, DifficultyTooEasy, StringComparison.Ordinal))
            {
                return "当前太简单，" + spec.tooEasySuggestionChinese;
            }

            if (string.Equals(difficulty, DifficultyTooHard, StringComparison.Ordinal))
            {
                return "当前太难，" + spec.tooHardSuggestionChinese;
            }

            return "当前合适，" + spec.fitSuggestionChinese;
        }

        private static string ResolvePlayerBattleFeedback(
            string buildFeedback,
            string bossFeedback,
            string difficulty)
        {
            string pressure = string.Equals(difficulty, DifficultyTooHard, StringComparison.Ordinal)
                ? "压力偏高"
                : string.Equals(difficulty, DifficultyTooEasy, StringComparison.Ordinal)
                    ? "压力偏低"
                    : "压力适中";
            return $"{pressure}。{buildFeedback}；{bossFeedback}";
        }

        private static IEnumerable<DevChapterBalanceDeveloperPanelField> BuildDeveloperFields(
            DevChapterBalanceRunStage stage,
            BossReadinessViewModel readinessRow,
            BattleSandboxShapeBuildRuleMatch shapeMatch,
            BuildSandboxBossProfile bossProfile)
        {
            yield return Field("devChapterLabel", "验证关标签", stage.devChapterLabel, "DevChapterBalanceRun.stage");
            yield return Field("mapRuleId", "地图规则编号", stage.mapRuleId, BuildProblemSource);
            yield return Field("enemyProblemId", "敌人问题编号", stage.enemyProblemId, BuildProblemSource);
            yield return Field("bossProblemId", "首领问题编号", stage.bossProblemId, BuildProblemSource);
            yield return Field("bossProfileId", "首领验证编号", bossProfile?.bossId ?? stage.bossProfileId, EnemyBossSource);
            yield return Field("shapeRuleKey", "形态规则编号", shapeMatch?.englishStableKey ?? stage.shapeRuleKey, ShapeRulePreviewSource);
            yield return Field("previewBuildId", "预览构筑编号", stage.previewBuildId, BuildCombatPreviewSource);
            yield return Field("readinessKeyRatio", "关键项命中率", $"{stage.readinessSatisfiedKeyCount}/{stage.readinessTotalKeyCount}", BuildProblemSource);
            yield return Field("simulatedWinRate", "预估胜率", stage.simulatedWinRate.ToString("0.000"), nameof(BuildSimulationRunner));
            yield return Field("simulatedClearTime", "预估时长", stage.simulatedClearTimeSeconds.ToString("0.0"), nameof(BuildSimulationRunner));
            yield return Field("difficultyTendency", "难度倾向", stage.difficultyTendencyChinese, "DevChapterBalanceRun.difficulty");
            if (readinessRow != null)
            {
                yield return Field("readyBossWindow", "破绽窗口", readinessRow.ready.ToString(), BuildProblemSource);
            }
        }

        private static DevChapterBalanceDeveloperPanelField Field(
            string key,
            string labelChinese,
            string value,
            string source)
        {
            return new DevChapterBalanceDeveloperPanelField
            {
                fieldKey = key ?? string.Empty,
                labelChinese = labelChinese ?? string.Empty,
                value = value ?? string.Empty,
                sourceDataPath = source ?? string.Empty,
                devOnly = true,
                isEnabled = false,
                developerVisible = true,
                playerVisible = false,
                maskedFromPlayer = true
            };
        }

        private static float Round01(float value)
        {
            return (float)Math.Round(Clamp01(value), 4);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }

        private sealed class StageSpec
        {
            public StageSpec(
                string stageId,
                string devChapterLabel,
                string chapterLabelChinese,
                string previewBuildId,
                string recommendedTestTargetChinese,
                string mapRuleId,
                string enemyProblemId,
                string bossProblemId,
                string fallbackBossNameChinese,
                string bossProfileId,
                string shapeRuleKey,
                string shapeRuleNameChinese,
                float pressureBias,
                string tooEasySuggestionChinese,
                string fitSuggestionChinese,
                string tooHardSuggestionChinese,
                Func<BuildSandboxLayoutSnapshot> createSnapshot)
            {
                this.stageId = stageId;
                this.devChapterLabel = devChapterLabel;
                this.chapterLabelChinese = chapterLabelChinese;
                this.previewBuildId = previewBuildId;
                this.recommendedTestTargetChinese = recommendedTestTargetChinese;
                this.mapRuleId = mapRuleId;
                this.enemyProblemId = enemyProblemId;
                this.bossProblemId = bossProblemId;
                this.fallbackBossNameChinese = fallbackBossNameChinese;
                this.bossProfileId = bossProfileId;
                this.shapeRuleKey = shapeRuleKey;
                this.shapeRuleNameChinese = shapeRuleNameChinese;
                this.pressureBias = pressureBias;
                this.tooEasySuggestionChinese = tooEasySuggestionChinese;
                this.fitSuggestionChinese = fitSuggestionChinese;
                this.tooHardSuggestionChinese = tooHardSuggestionChinese;
                this.createSnapshot = createSnapshot;
            }

            public readonly string stageId;
            public readonly string devChapterLabel;
            public readonly string chapterLabelChinese;
            public readonly string previewBuildId;
            public readonly string recommendedTestTargetChinese;
            public readonly string mapRuleId;
            public readonly string enemyProblemId;
            public readonly string bossProblemId;
            public readonly string fallbackBossNameChinese;
            public readonly string bossProfileId;
            public readonly string shapeRuleKey;
            public readonly string shapeRuleNameChinese;
            public readonly float pressureBias;
            public readonly string tooEasySuggestionChinese;
            public readonly string fitSuggestionChinese;
            public readonly string tooHardSuggestionChinese;
            private readonly Func<BuildSandboxLayoutSnapshot> createSnapshot;

            public BuildSandboxLayoutSnapshot CreateSnapshot()
            {
                return createSnapshot?.Invoke() ?? new BuildSandboxLayoutSnapshot();
            }
        }
    }
}
