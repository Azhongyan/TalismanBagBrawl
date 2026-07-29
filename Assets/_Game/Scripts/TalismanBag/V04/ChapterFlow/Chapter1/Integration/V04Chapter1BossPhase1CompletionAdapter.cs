using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public static class V04Chapter1BossPhase1CompletionAdapter
    {
        public const string RequiredBossItemChain =
            "I007 / I008 / I009 / I010 / I011 / I012 + owned and placed I031";

        public static bool TryValidateBossReadiness(
            IItemSystemBattleSandboxBoardAuthority authority,
            out string diagnostic)
        {
            List<string> missing = new();
            diagnostic = string.Empty;
            if (authority == null)
            {
                diagnostic =
                    "Boss readiness blocked: exactly one current BattleSandbox Item authority is required. "
                    + "Remain at BossGate, arrange "
                    + RequiredBossItemChain
                    + ", then retry Manual Challenge.";
                return false;
            }

            if (authority.CurrentSnapshot?.isValid != true)
            {
                missing.Add("valid ItemSystem snapshot");
            }
            if (authority.CurrentBindingSnapshot?.isValid != true)
            {
                missing.Add("valid placement binding snapshot");
            }
            if (authority.CurrentQualifiedBuildState?.isValid != true)
            {
                missing.Add("valid qualified-build snapshot");
            }
            if (authority.CurrentCoreEffectRuntimeState?.isValid != true)
            {
                missing.Add("valid core-runtime snapshot");
            }
            if (missing.Count > 0)
            {
                diagnostic = BuildBlockedDiagnostic(missing);
                return false;
            }

            ItemCombatEffectRequestSnapshot snapshot;
            try
            {
                ItemInstanceProjectionSetSnapshot projectionSet = new(
                    authority.Rows
                        .Where(row => row != null && !row.IsSystemItem)
                        .Select(row => row.Projection)
                        .Where(value => value != null)
                        .OrderBy(
                            value => value.itemInstanceId,
                            StringComparer.Ordinal),
                    Array.Empty<ItemInstanceProjectionValidationError>());
                snapshot =
                    ItemCombatEffectRequestAssembler.Instance.Assemble(
                        projectionSet,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState,
                        authority.CurrentCoreEffectRuntimeState);
            }
            catch (Exception exception)
            {
                diagnostic =
                    "Boss readiness blocked: real Item request assembly failed ("
                    + exception.GetType().Name
                    + "). Remain at BossGate and repair the board before retry.";
                return false;
            }

            if (snapshot?.status !=
                ItemCombatEffectRequestSnapshotStatus.Valid)
            {
                string validationCodes = snapshot == null
                    ? "snapshot missing"
                    : string.Join(
                        ", ",
                        snapshot.ValidationErrors
                            .Where(value => value != null)
                            .Select(value => value.code)
                            .Distinct(StringComparer.Ordinal)
                            .Take(4));
                missing.Add(
                    "valid ItemCombatEffectRequestSnapshot.v1"
                    + (string.IsNullOrWhiteSpace(validationCodes)
                        ? string.Empty
                        : " [" + validationCodes + "]"));
            }
            else
            {
                foreach (string itemId in
                    ShougunuPhase1BattleApplicationEngine.ItemIds)
                {
                    ItemCombatEffectRequestRow[] rows = snapshot.Requests
                        .Where(value => value != null
                            && string.Equals(
                                value.sourceBaseItemId,
                                itemId,
                                StringComparison.Ordinal)
                            && value.requestKind ==
                                ItemCombatEffectRequestKind.DirectFlatDamage)
                        .ToArray();
                    if (rows.Length != 1)
                    {
                        missing.Add(
                            itemId + " positive real request count="
                            + rows.Length + " (expected 1)");
                    }
                    else if (rows[0]
                        .resolvedPreMitigationDamageUnits <= 0)
                    {
                        missing.Add(
                            itemId + " resolved real damage must be > 0");
                    }
                }
            }

            if (authority.CurrentSnapshot?.i031State?.isOwned != true)
            {
                missing.Add("I031 must be owned");
            }
            if (authority.CurrentSnapshot?.i031State?.isPlaced != true)
            {
                missing.Add("I031 must be placed on the board");
            }
            if (missing.Count > 0)
            {
                diagnostic = BuildBlockedDiagnostic(missing);
                return false;
            }

            diagnostic =
                "Boss readiness accepted: "
                + RequiredBossItemChain
                + "; the authored Battle surface may now be invoked exactly once.";
            return true;
        }

        public static bool IsApprovedTemporaryTerminal(
            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime)
        {
            return runtime != null
                && runtime.DevOnly
                && runtime.IsEnabled
                && runtime.Completed
                && runtime.CurrentContext?.enemy?.Lifecycle ==
                    ShougunuPhase1LifecycleState.Defeated;
        }

        public static BattleResultSnapshot Resolve(
            BattleStartRequest request,
            long resultSequence,
            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime)
        {
            return IsApprovedTemporaryTerminal(runtime)
                ? CreateApprovedTemporaryResult(request, resultSequence)
                : null;
        }

        public static BattleResultSnapshot CreateApprovedTemporaryResult(
            BattleStartRequest request,
            long resultSequence)
        {
            if (request == null || !request.isBossStage
                || request.stageId != "1-10")
            {
                return null;
            }
            return new BattleResultSnapshot
            {
                resultId = V04Chapter1ContinuousFlowSession.StableId(
                    "phase1_temporary_result",
                    resultSequence),
                requestId = request.requestId,
                resultType = BattleResultType.Win,
                win = true,
                lose = false,
                abandon = false,
                roundId = request.roundId,
                bossDefeated = true,
                durationSeconds = 0f,
                chapterProgressDelta = string.Empty,
                rewardPreview = new List<string>(),
                itemDrops = new List<string>(),
                buildPerformanceSummary =
                    "completionResolverSlotId="
                    + V04Chapter1ContinuousBattleIntegrationContract
                        .CompletionResolverSlotId,
                eventSummary = new List<string>
                {
                    V04Chapter1ContinuousBattleIntegrationContract
                        .Phase1DevVerticalSlice,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .NotContentFinal,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .NotFormalBossCompletion,
                    "replacementMode="
                        + V04Chapter1ContinuousBattleIntegrationContract
                            .ReplacementMode,
                    "chapterFlowTruthChanged=false",
                    "fullBossCompletionClaimed=false",
                    "formalBossCompletionGranted=false"
                },
                rewardClaimToken = string.Empty,
                nextRouteHint = string.Empty,
                devOnly = true,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        private static string BuildBlockedDiagnostic(
            IEnumerable<string> missing)
        {
            return "Boss readiness blocked; ChapterFlow remains at BossGate. Missing: "
                + string.Join("; ", missing ?? Array.Empty<string>())
                + ". Keep arranging the real Item board for "
                + RequiredBossItemChain
                + ", then retry Manual Challenge.";
        }
    }
}
