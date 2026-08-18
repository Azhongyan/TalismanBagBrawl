using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.Contracts.Battle
{
    public static class BuildEvaluationSnapshotExporter
    {
        public static BuildEvaluationSnapshot FromBuildEvaluation(
            BuildEvaluationResult buildResult,
            CombatModifierBundle modifierBundle = null,
            EffectEventBundle eventBundle = null,
            BuildSandboxPreviewContext context = null,
            string snapshotId = "")
        {
            BuildEvaluationResult safeResult = buildResult ?? new BuildEvaluationResult();
            BuildEvaluationSnapshot snapshot = new()
            {
                snapshotId = string.IsNullOrWhiteSpace(snapshotId) ? "v04_build_evaluation" : snapshotId.Trim(),
                activeSynergies = MapSynergies(safeResult.activeSynergies),
                readinessSummary = ResolveReadinessSummary(safeResult),
                modifierBundleSummary = MapPlayerModifierSummary(modifierBundle),
                eventBundleSummary = MapPlayerEventSummary(eventBundle),
                playerVisibleHints = BuildPlayerHints(safeResult),
                recommendedAction = ResolveRecommendedAction(safeResult),
                devOnly = true,
                sourceDataPath = "BuildSandboxPreviewContext.buildEvaluation"
            };

            AppendDeveloperDiagnostics(snapshot, safeResult, modifierBundle, eventBundle, context);
            return snapshot;
        }

        private static List<BattleActiveSynergySnapshot> MapSynergies(
            IEnumerable<ActiveSynergyResult> synergies)
        {
            return (synergies ?? Enumerable.Empty<ActiveSynergyResult>())
                .Where(synergy => synergy != null)
                .Select(synergy => new BattleActiveSynergySnapshot
                {
                    synergyId = Normalize(synergy.synergyId),
                    displayName = Normalize(synergy.displayName),
                    matchedCount = synergy.matchedCount,
                    activeThresholds = Clean(synergy.activeThresholds),
                    sourceItems = Clean(synergy.sourceItems),
                    placementSatisfied = synergy.placementSatisfied,
                    energySatisfied = synergy.energySatisfied
                })
                .ToList();
        }

        private static string ResolveReadinessSummary(BuildEvaluationResult result)
        {
            if (result == null)
            {
                return "unknown";
            }

            if (result.placementSatisfied && result.energySatisfied)
            {
                return result.HasActiveSynergy ? "ready_with_active_synergy" : "ready_basic";
            }

            if (!result.placementSatisfied && !result.energySatisfied)
            {
                return "placement_and_energy_need_attention";
            }

            return result.placementSatisfied ? "energy_needs_attention" : "placement_needs_attention";
        }

        private static List<string> MapPlayerModifierSummary(CombatModifierBundle bundle)
        {
            return Clean((bundle?.modifiers ?? new List<BuildModifierPreview>())
                .Where(item => item != null)
                .Select(item => "modifier:" + Normalize(item.modifierType)));
        }

        private static List<string> MapPlayerEventSummary(EffectEventBundle bundle)
        {
            return Clean((bundle?.events ?? new List<BuildEventPreview>())
                .Where(item => item != null)
                .Select(item => "event:" + Normalize(item.eventType)));
        }

        private static List<string> BuildPlayerHints(BuildEvaluationResult result)
        {
            List<string> hints = new();
            if (result == null)
            {
                return hints;
            }

            if (!result.placementSatisfied)
            {
                hints.Add("Adjust placement before the next sandbox battle preview.");
            }

            if (!result.energySatisfied)
            {
                hints.Add("Check energy connection before the next sandbox battle preview.");
            }

            if (result.nextThresholdHint?.hasNextThreshold == true)
            {
                hints.Add("Another nearby piece may improve the current build threshold.");
            }

            if (hints.Count == 0)
            {
                hints.Add("Build is ready for sandbox battle preview.");
            }

            return hints;
        }

        private static string ResolveRecommendedAction(BuildEvaluationResult result)
        {
            if (result == null)
            {
                return "inspect_build";
            }

            if (!result.energySatisfied)
            {
                return "check_energy_connection";
            }

            if (!result.placementSatisfied)
            {
                return "adjust_placement";
            }

            return result.HasActiveSynergy ? "try_battle_preview" : "add_synergy_piece";
        }

        private static void AppendDeveloperDiagnostics(
            BuildEvaluationSnapshot snapshot,
            BuildEvaluationResult result,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle,
            BuildSandboxPreviewContext context)
        {
            snapshot.shapeBuildRules = Clean(context?.viewModel?.shapeOccupancy?.invalidReasons);
            snapshot.problemReadinessFullAnswer = BuildProblemReadinessDiagnostics(context);
            snapshot.bossSixKeyFullAnswer = BuildBossKeyDiagnostics(context);
            snapshot.hardSolutionTags = Clean(context?.viewModel?.buildSummary?.buildTags);
            snapshot.requiredSynergy = Clean((result?.missingRequirements ?? new List<SynergyRequirementResult>())
                .Select(requirement => requirement?.synergyId));
            snapshot.requiredAffix = Clean(context?.viewModel?.affixModifier?.affixIds);
            snapshot.requiredStats = Clean((context?.layoutSnapshot?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>())
                .Select(item => item?.itemStat?.statProfileId));
            snapshot.dropBiasWeights = Clean((context?.viewModel?.problemReadiness?.bossRows ?? new List<BossReadinessViewModel>())
                .SelectMany(row => row?.dropBiasIds ?? new List<string>()));

            snapshot.developerOnlyDiagnostics.Add("modifierBundle.devOnly=" + (modifierBundle?.devOnly ?? true));
            snapshot.developerOnlyDiagnostics.Add("modifierBundle.affectsFormalCombat=" + (modifierBundle?.affectsFormalCombat ?? false));
            snapshot.developerOnlyDiagnostics.Add("eventBundle.devOnly=" + (eventBundle?.devOnly ?? true));
            snapshot.developerOnlyDiagnostics.Add("eventBundle.affectsFormalCombat=" + (eventBundle?.affectsFormalCombat ?? false));
            snapshot.developerOnlyDiagnostics.Add("context.devOnly=" + (context?.devOnly ?? true));
            snapshot.developerOnlyDiagnostics.Add("context.writesFormalFlow=" + (context?.writesFormalFlow ?? false));
            snapshot.developerOnlyDiagnostics.Add("context.writesFormalData=" + (context?.writesFormalData ?? false));
        }

        private static List<string> BuildProblemReadinessDiagnostics(BuildSandboxPreviewContext context)
        {
            return Clean((context?.viewModel?.problemReadiness?.bossRows ?? new List<BossReadinessViewModel>())
                .Select(row => row == null
                    ? string.Empty
                    : $"{row.bossProblemId}:{row.satisfiedKeyCount}/{row.keyCount}:ready={row.ready}"));
        }

        private static List<string> BuildBossKeyDiagnostics(BuildSandboxPreviewContext context)
        {
            return Clean((context?.viewModel?.problemReadiness?.bossRows ?? new List<BossReadinessViewModel>())
                .SelectMany(row => row?.keys ?? new List<BossReadinessKeyViewModel>())
                .Select(key => key == null
                    ? string.Empty
                    : $"{key.keyId}:{key.requirementId}:{key.observedScore}/{key.requiredScore}:satisfied={key.satisfied}"));
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

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public static class SandboxBattleResultMapper
    {
        public static BattleResultSnapshot CreateDefaultSandboxResult(
            string requestId = "",
            string roundId = "")
        {
            return new BattleResultSnapshot
            {
                resultId = "sandbox_result_default",
                requestId = Normalize(requestId),
                resultType = BattleResultType.Unknown,
                roundId = Normalize(roundId),
                chapterProgressDelta = "none",
                rewardPreview = new List<string> { "sandbox_only_no_reward" },
                buildPerformanceSummary = "sandbox result only",
                nextRouteHint = "restart_or_change_sandbox_enemy",
                devOnly = true,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        public static BattleResultSnapshot FromRuntimeLoopPreview(
            BattleSandboxRuntimeLoopPreview preview,
            string requestId = "")
        {
            if (preview == null)
            {
                return CreateDefaultSandboxResult(requestId);
            }

            BattleSandboxRuntimeLoopRow resultRow = (preview.rows ?? new List<BattleSandboxRuntimeLoopRow>())
                .LastOrDefault(row => row != null && (row.hasSandboxVictoryResult || row.hasSandboxDefeatResult));
            bool win = resultRow?.hasSandboxVictoryResult == true || preview.hasSandboxVictoryResult;
            bool lose = resultRow?.hasSandboxDefeatResult == true || preview.hasSandboxDefeatResult;
            BattleResultType resultType = win
                ? BattleResultType.Win
                : lose ? BattleResultType.Lose : BattleResultType.Unknown;

            BattleResultSnapshot snapshot = new()
            {
                resultId = "sandbox_result_" + FirstNonEmpty(preview.selectedDevEnemyStageId, requestId, "unknown"),
                requestId = Normalize(requestId),
                resultType = resultType,
                win = win,
                lose = lose,
                abandon = false,
                roundId = Normalize(preview.selectedDevEnemyStageId),
                bossDefeated = win,
                durationSeconds = ResolveDuration(preview, resultRow),
                chapterProgressDelta = "none",
                rewardPreview = new List<string> { "sandbox_only_no_reward" },
                itemDrops = new List<string>(),
                buildPerformanceSummary = Normalize(resultRow?.resultBodyChinese),
                eventSummary = BuildEventSummary(preview, resultRow),
                rewardClaimToken = string.Empty,
                nextRouteHint = Normalize(resultRow?.restartHintChinese),
                devOnly = true,
                shouldWriteSave = false,
                shouldGrantReward = false
            };

            return snapshot;
        }

        private static float ResolveDuration(
            BattleSandboxRuntimeLoopPreview preview,
            BattleSandboxRuntimeLoopRow resultRow)
        {
            if (resultRow != null)
            {
                return resultRow.elapsedSeconds;
            }

            return (preview.rows ?? new List<BattleSandboxRuntimeLoopRow>())
                .Where(row => row != null)
                .Select(row => row.elapsedSeconds)
                .DefaultIfEmpty(0f)
                .Max();
        }

        private static List<string> BuildEventSummary(
            BattleSandboxRuntimeLoopPreview preview,
            BattleSandboxRuntimeLoopRow resultRow)
        {
            List<string> summary = new()
            {
                "resultType=" + (resultRow?.rowKind ?? "unknown"),
                "runtimeRows=" + (preview?.rows?.Count ?? 0),
                "itemTriggerRows=" + (preview?.ItemTriggerRowCount ?? 0),
                "bossCastRows=" + (preview?.BossCastRowCount ?? 0),
                "enemyAttackRows=" + (preview?.EnemyAttackRowCount ?? 0)
            };
            return summary;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return (values ?? Array.Empty<string>())
                .Select(Normalize)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class BattleSnapshotAdapterValidationSnapshot
    {
        public BattleLayoutSnapshot v04LayoutSnapshot;
        public BuildEvaluationSnapshot buildEvaluationSnapshot;
        public BattleResultSnapshot sandboxResultSnapshot;
        public int v04MultiCellErrorCount;
        public int playerDeveloperFieldLeakCount;
        public int sandboxResultDefaultErrorCount;
        public int formalWriteLeakCount;
        public int featureFlagDefaultTrueCount;
        public int devOnlyFormalFlowLeakCount;
        public List<string> validationMessages = new();

        public int TotalLeakCount =>
            playerDeveloperFieldLeakCount
            + formalWriteLeakCount
            + featureFlagDefaultTrueCount
            + devOnlyFormalFlowLeakCount;

        public int TotalErrorCount =>
            v04MultiCellErrorCount
            + sandboxResultDefaultErrorCount
            + TotalLeakCount;

        public bool Passed => TotalErrorCount == 0;
    }

    public static class BattleContractFieldValidator
    {
        private static readonly string[] ForbiddenPlayerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "dropBias",
            "bossSixKeyFullAnswer",
            "problemReadinessFullAnswer",
            "exactWeaknessTiming",
            "validationTargetBuilds",
            "sourceDataPath",
            "raw config",
            "EnemyBossValidationPool",
            "BuildProblemSeedDataset"
        };

        public static BattleSnapshotAdapterValidationSnapshot BuildValidationSnapshot()
        {
            BuildSandboxLayoutSnapshot v04Source =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();
            BattleLayoutSnapshot v04Layout =
                V04BattleLayoutNormalizer.FromBuildSandboxLayout(v04Source, "validation_v04_multi_cell");
            BattleSandboxBuildCombatPreview buildPreview =
                BattleSandboxBuildCombatPreviewBuilder.Build(v04Source, "battle_snapshot_adapter_validation", true);
            BuildEvaluationSnapshot buildSnapshot =
                BuildEvaluationSnapshotExporter.FromBuildEvaluation(
                    buildPreview.context?.buildEvaluation,
                    buildPreview.context?.modifierBundle,
                    buildPreview.context?.eventBundle,
                    buildPreview.context,
                    "validation_build_evaluation");
            BattleSandboxRuntimeLoopPreview runtimePreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(v04Source, "battle_snapshot_adapter_validation");
            BattleResultSnapshot resultSnapshot =
                SandboxBattleResultMapper.FromRuntimeLoopPreview(runtimePreview, "validation_sandbox_request");

            BattleSnapshotAdapterValidationSnapshot snapshot = new()
            {
                v04LayoutSnapshot = v04Layout,
                buildEvaluationSnapshot = buildSnapshot,
                sandboxResultSnapshot = resultSnapshot,
                v04MultiCellErrorCount = CountV04MultiCellErrors(v04Layout),
                playerDeveloperFieldLeakCount = CountPlayerFieldAnswerLeaks(buildSnapshot),
                sandboxResultDefaultErrorCount = CountSandboxResultDefaultErrors(resultSnapshot),
                formalWriteLeakCount = CountFormalWriteLeaks(runtimePreview),
                featureFlagDefaultTrueCount = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue),
                devOnlyFormalFlowLeakCount = CountDevOnlyFormalFlowLeaks(buildPreview, runtimePreview)
            };

            snapshot.validationMessages.Add($"v04PlacedItems={v04Layout.placedItems.Count}");
            snapshot.validationMessages.Add($"v04MultiCellItems={v04Layout.placedItems.Count(item => item.occupiedCells.Count > 1)}");
            snapshot.validationMessages.Add($"activeSynergies={buildSnapshot.activeSynergies.Count}");
            snapshot.validationMessages.Add($"sandboxResultDevOnly={resultSnapshot.devOnly}");
            snapshot.validationMessages.Add($"sandboxShouldWriteSave={resultSnapshot.shouldWriteSave}");
            snapshot.validationMessages.Add($"sandboxShouldGrantReward={resultSnapshot.shouldGrantReward}");
            return snapshot;
        }

        public static int CountV04MultiCellErrors(BattleLayoutSnapshot snapshot)
        {
            if (snapshot == null || snapshot.placedItems == null || snapshot.placedItems.Count == 0)
            {
                return 1;
            }

            int errors = snapshot.devOnly ? 0 : 1;
            errors += snapshot.legacySingleCell ? 1 : 0;
            errors += snapshot.placedItems.Any(item => item != null && item.occupiedCells != null && item.occupiedCells.Count > 1)
                ? 0
                : 1;
            errors += snapshot.placedItems.Count(item =>
                item == null
                || item.occupiedCells == null
                || item.occupiedCells.Count == 0
                || string.IsNullOrWhiteSpace(item.shapeId));
            return errors;
        }

        public static int CountSandboxResultDefaultErrors(BattleResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 1;
            }

            int errors = 0;
            if (!snapshot.devOnly) errors++;
            if (snapshot.shouldWriteSave) errors++;
            if (snapshot.shouldGrantReward) errors++;
            if (!string.Equals(snapshot.chapterProgressDelta, "none", StringComparison.Ordinal)) errors++;
            if (!string.IsNullOrWhiteSpace(snapshot.rewardClaimToken)) errors++;
            return errors;
        }

        public static int CountPlayerFieldAnswerLeaks(BuildEvaluationSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 1;
            }

            List<string> playerValues = new();
            playerValues.Add(snapshot.readinessSummary);
            playerValues.Add(snapshot.recommendedAction);
            playerValues.AddRange(snapshot.playerVisibleHints ?? new List<string>());
            playerValues.AddRange(snapshot.modifierBundleSummary ?? new List<string>());
            playerValues.AddRange(snapshot.eventBundleSummary ?? new List<string>());
            playerValues.AddRange((snapshot.activeSynergies ?? new List<BattleActiveSynergySnapshot>())
                .Select(synergy => synergy?.displayName));

            return playerValues.Count(value => ContainsForbiddenPlayerToken(value));
        }

        public static int CountFormalWriteLeaks(BattleSandboxRuntimeLoopPreview preview)
        {
            if (preview == null)
            {
                return 1;
            }

            int leaks = 0;
            if (preview.runsFormalCombat) leaks++;
            if (preview.callsFormalDamageSettlement) leaks++;
            if (preview.writesFormalFlow) leaks++;
            if (preview.writesFormalSaveData) leaks++;
            if (preview.grantsFormalReward) leaks++;
            if (preview.advancesChapter) leaks++;
            if (preview.touchesFormalSceneUiLayout) leaks++;
            leaks += preview.UiLayoutWriteCount;
            leaks += preview.SettlementLeakCount;
            return leaks;
        }

        public static int CountDevOnlyFormalFlowLeaks(
            BattleSandboxBuildCombatPreview buildPreview,
            BattleSandboxRuntimeLoopPreview runtimePreview)
        {
            int leaks = 0;
            if (buildPreview == null || !buildPreview.devOnly || buildPreview.isEnabled) leaks++;
            if (buildPreview != null)
            {
                leaks += buildPreview.FormalFlowLeakCount;
                leaks += buildPreview.PlayerSideAnswerLeakCount;
            }

            if (runtimePreview == null || !runtimePreview.devOnly || runtimePreview.isEnabled) leaks++;
            if (runtimePreview != null)
            {
                leaks += runtimePreview.FormalLeakCount;
                leaks += runtimePreview.PlayerSideAnswerLeakCount;
            }

            return leaks;
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return ForbiddenPlayerTokens.Any(token =>
                value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
