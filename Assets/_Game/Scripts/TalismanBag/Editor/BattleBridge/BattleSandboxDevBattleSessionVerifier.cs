using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.DevSession;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;
using TalismanBag.Items.Resource;
using TalismanBag.V04.CoreLoopLab;

namespace TalismanBag.Editor.BattleBridge
{
    public static class BattleSandboxDevBattleSessionVerifier
    {
        private sealed class TwoBattleFlowResult
        {
            public BattleSandboxDevBattleSessionSnapshot BattleResult;
            public string SessionFingerprint;
        }

        private static readonly IReadOnlyDictionary<string, string> Identity =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "I031", "SPECIAL_I031" },
                { "I007", "wb_i007_orange_404310007" },
                { "I008", "wb_i008_orange_404310008" },
                { "I009", "wb_i009_orange_404310009" },
                { "I010", "wb_i010_orange_404310010" },
                { "I011", "wb_i011_orange_404310011" },
                { "I012", "wb_i012_orange_404310012" }
            };

        public static bool RunAll(out string report)
        {
            List<string> checks = new();
            VerifyControlledUniverse(checks);
            VerifyDamageLineageMatrix(checks);
            VerifyA(checks);
            foreach (string reward in new[] { "I007", "I009", "I012" })
            {
                VerifyBAndC(reward, checks);
            }
            VerifyNianInsufficiencyRecoveryAndDedupe(checks);
            VerifyStartDriftAndReset(checks);
            report = string.Join("\n", checks);
            return true;
        }

        public static int Main()
        {
            try
            {
                bool passed = RunAll(out string report);
                Console.WriteLine(report);
                Console.WriteLine(passed
                    ? "CORELOOP_DEV_SESSION_CORE_TESTS_PASS"
                    : "CORELOOP_DEV_SESSION_CORE_TESTS_FAIL");
                return passed ? 0 : 1;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.ToString());
                return 1;
            }
        }

        private static void VerifyControlledUniverse(List<string> checks)
        {
            string[] expectedInitial =
            {
                "I008", "I010", "I011", "I031"
            };
            string[] expectedRewards = { "I007", "I009", "I012" };
            Require(CoreLoopLabRewardProfile.Shared.CanonicalInitialRoster
                    .SequenceEqual(expectedInitial, StringComparer.Ordinal),
                "INITIAL_ROSTER_MISMATCH");
            Require(CoreLoopLabFixedContent.RewardIds.SequenceEqual(
                    expectedRewards,
                    StringComparer.Ordinal),
                "REWARD_PROFILE_MISMATCH");
            Require(!CoreLoopLabFixedContent.RewardIds.Contains(
                    "I016",
                    StringComparer.Ordinal)
                && !CoreLoopLabFixedContent.RewardIds.Contains(
                    "I022",
                    StringComparer.Ordinal),
                "LEGACY_REWARD_LEAK");
            checks.Add("PASS controlled-seven-identities-no-I016-I022");
        }

        private static void VerifyDamageLineageMatrix(List<string> checks)
        {
            BattleSandboxDevBattleSessionRequest battle1 = Request(
                BattleSandboxDevBattleSessionRequest.ProfileA,
                1,
                string.Empty,
                CoreLoopLabEncounterProfiles.Normal);
            Require(BattleSandboxDevBattleSessionContract.Validate(
                    battle1,
                    true,
                    out string diagnostic),
                diagnostic);
            Require(FactOf(battle1, "I010").BaseDamageUnits == 37L
                && FactOf(battle1, "I010").ResolvedDamageUnits == 43L,
                "I010_BATTLE1_AUTHORITATIVE_PAIR_MISSING");
            VerifyRejectedLineage(
                battle1,
                "I010",
                37L,
                46L,
                "DEV_SESSION_I010_RESOLVED_DAMAGE_MISMATCH");
            VerifyRejectedLineage(
                battle1,
                "I010",
                43L,
                43L,
                "DEV_SESSION_I010_BASE_DAMAGE_MISMATCH");

            VerifyRewardLineage(
                "I007",
                24L,
                29L,
                new[]
                {
                    (24L, 24L, "DEV_SESSION_I007_RESOLVED_DAMAGE_MISMATCH"),
                    (29L, 29L, "DEV_SESSION_I007_BASE_DAMAGE_MISMATCH"),
                    (29L, 24L, "DEV_SESSION_I007_BASE_DAMAGE_MISMATCH")
                });
            VerifyRewardLineage(
                "I009",
                44L,
                53L,
                new[]
                {
                    (44L, 44L, "DEV_SESSION_I009_RESOLVED_DAMAGE_MISMATCH"),
                    (53L, 53L, "DEV_SESSION_I009_BASE_DAMAGE_MISMATCH"),
                    (53L, 44L, "DEV_SESSION_I009_BASE_DAMAGE_MISMATCH")
                });
            VerifyRewardLineage(
                "I012",
                62L,
                76L,
                new[]
                {
                    (62L, 62L, "DEV_SESSION_I012_RESOLVED_DAMAGE_MISMATCH"),
                    (76L, 76L, "DEV_SESSION_I012_BASE_DAMAGE_MISMATCH"),
                    (76L, 62L, "DEV_SESSION_I012_BASE_DAMAGE_MISMATCH")
                });
            checks.Add("PASS authoritative-raw-resolved-lineage-matrix");
        }

        private static void VerifyRewardLineage(
            string reward,
            long expectedBase,
            long expectedResolved,
            IEnumerable<(long Base, long Resolved, string Diagnostic)> negatives)
        {
            BattleSandboxDevBattleSessionRequest request = Request(
                BattleSandboxDevBattleSessionRequest.ProfileB,
                2,
                reward,
                CoreLoopLabEncounterProfiles.Elite);
            Require(BattleSandboxDevBattleSessionContract.Validate(
                    request,
                    true,
                    out string diagnostic),
                diagnostic);
            Require(FactOf(request, reward).BaseDamageUnits == expectedBase
                && FactOf(request, reward).ResolvedDamageUnits
                    == expectedResolved
                && FactOf(request, "I010").BaseDamageUnits == 37L
                && FactOf(request, "I010").ResolvedDamageUnits == 46L,
                "AUTHORITATIVE_PAIR_MISSING_" + reward);
            VerifyRejectedLineage(
                request,
                "I010",
                37L,
                43L,
                "DEV_SESSION_I010_RESOLVED_DAMAGE_MISMATCH");
            foreach (var negative in negatives)
            {
                VerifyRejectedLineage(
                    request,
                    reward,
                    negative.Base,
                    negative.Resolved,
                    negative.Diagnostic);
            }
        }

        private static void VerifyRejectedLineage(
            BattleSandboxDevBattleSessionRequest source,
            string baseItemId,
            long baseDamage,
            long resolvedDamage,
            string expectedDiagnostic)
        {
            BattleSandboxDevBattleSessionRequest mutated = WithDamageFact(
                source,
                baseItemId,
                baseDamage,
                resolvedDamage);
            Require(!BattleSandboxDevBattleSessionContract.Validate(
                    mutated,
                    true,
                    out string diagnostic)
                && string.Equals(
                    diagnostic,
                    expectedDiagnostic,
                    StringComparison.Ordinal),
                "LINEAGE_NEGATIVE_DIAGNOSTIC_MISMATCH|expected="
                + expectedDiagnostic + "|actual=" + diagnostic);
        }

        private static void VerifyA(List<string> checks)
        {
            var request = Request(
                BattleSandboxDevBattleSessionRequest.ProfileA,
                1,
                string.Empty,
                CoreLoopLabEncounterProfiles.Normal);
            Require(!request.RequestsFormalFlow
                && !request.RequestsCampaignFlow
                && !request.RequestsPlayerOrApk,
                "A_FORMAL_WRITE_FLAG");
            BattleSandboxDevBattleSessionEngine engine = new();
            Require(engine.TryStart(request, true, out var start),
                engine.LastDiagnosticCode);
            long revision = start.StartSnapshot.RuntimeRevision;
            Require(engine.TryStart(request, true, out var same)
                && ReferenceEquals(start, same)
                && engine.Current.RuntimeRevision == revision,
                "IDEMPOTENT_START_FAILED");
            Require(engine.TryAdvanceBy(
                    18000,
                    request.SourceSignatures,
                    out var result),
                engine.LastDiagnosticCode);
            Require(result.Victory
                && result.AcceptedEnemyBasicAttackCount >= 2
                && result.AcceptedEnemyBasicAttackCount <= 3
                && result.AcceptedEnemySkillCount == 0
                && result.AcceptedItemApplicationCount > 0,
                "A_NORMAL_RESULT_INVALID");
            checks.Add("PASS A-normal-real-item-enemy-actions-result");
        }

        private static void VerifyBAndC(
            string reward,
            List<string> checks)
        {
            TwoBattleFlowResult b = RunTwoBattleFlow(
                BattleSandboxDevBattleSessionRequest.ProfileB,
                reward);
            TwoBattleFlowResult c = RunTwoBattleFlow(
                BattleSandboxDevBattleSessionRequest.ProfileC,
                reward);
            Require(b.BattleResult.IsTerminal && c.BattleResult.IsTerminal,
                "BC_ELITE_NOT_TERMINAL_" + reward);
            Require(b.BattleResult.AcceptedEnemySkillCount >= 1
                && b.BattleResult.AcceptedEnemyBasicAttackCount >= 4
                && b.BattleResult.AcceptedEnemyBasicAttackCount <= 6,
                "ELITE_ENEMY_CADENCE_INVALID_" + reward);
            Require(b.BattleResult.AcceptedItemApplicationCount > 0
                && b.BattleResult.SelectedRewardBaseItemId == reward,
                "REWARD_ITEM_NOT_APPLIED_" + reward);
            Require(string.Equals(
                    b.BattleResult.NonPresentationLedgerFingerprint,
                    c.BattleResult.NonPresentationLedgerFingerprint,
                    StringComparison.Ordinal),
                "BC_LEDGER_MISMATCH_" + reward);
            Require(string.Equals(
                    b.SessionFingerprint,
                    c.SessionFingerprint,
                    StringComparison.Ordinal),
                "BC_SESSION_LEDGER_MISMATCH_" + reward);
            checks.Add("PASS B-C-" + reward
                + "-Build2-real-application-ledger-equal");
        }

        private static TwoBattleFlowResult RunTwoBattleFlow(
            string profile,
            string reward)
        {
            CoreLoopLabProfile labProfile = profile ==
                BattleSandboxDevBattleSessionRequest.ProfileB
                ? CoreLoopLabProfile.B_TwoBattleRewardLoop
                : CoreLoopLabProfile.C_TwoBattleSemanticPresentation;
            CoreLoopLabSession session = new(labProfile);
            BattleSandboxDevBattleSessionEngine engine = new();
            BattleSandboxDevBattleSessionRequest normal = Request(
                profile,
                1,
                string.Empty,
                CoreLoopLabEncounterProfiles.Normal);
            Require(engine.TryStart(normal, true, out var normalStart)
                && session.AcceptBattleStarted(1, normalStart),
                "TWO_BATTLE_NORMAL_START_REJECTED_" + reward);
            Require(engine.TryAdvanceBy(
                    18000,
                    normal.SourceSignatures,
                    out var normalResult)
                && normalResult.Victory
                && session.AcceptBattleCompleted(1, normalResult)
                && session.Phase == CoreLoopLabPhase.RewardChoice,
                "TWO_BATTLE_NORMAL_RESULT_REJECTED_" + reward);
            Require(session.SelectFixedReward(reward)
                && session.Phase == CoreLoopLabPhase.SecondPreparation,
                "TWO_BATTLE_REWARD_PHASE_REJECTED_" + reward);

            engine.Reset();
            BattleSandboxDevBattleSessionRequest request = Request(
                profile,
                2,
                reward,
                CoreLoopLabEncounterProfiles.Elite);
            Require(request.LiHuoBuild2Active
                && request.LiHuoQualifiedItemCount == 2
                && request.LiHuoSourceBaseItemIds.SequenceEqual(
                    new[] { "I010", reward }
                        .OrderBy(value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal),
                "BUILD2_FACT_INVALID_" + reward);
            Require(engine.Current == null
                && engine.TryStart(request, true, out var eliteStart)
                && eliteStart.StartSnapshot.AcceptedItemApplicationCount == 0
                && eliteStart.StartSnapshot.AcceptedEnemyBasicAttackCount == 0
                && eliteStart.StartSnapshot.AcceptedEnemySkillCount == 0
                && session.AcceptBattleStarted(2, eliteStart),
                engine.LastDiagnosticCode);
            Require(engine.TryAdvanceBy(
                    30000,
                    request.SourceSignatures,
                    out var result),
                engine.LastDiagnosticCode);
            Require(session.AcceptBattleCompleted(2, result)
                && session.Phase == CoreLoopLabPhase.Result,
                "TWO_BATTLE_ELITE_RESULT_REJECTED_" + reward);
            return new TwoBattleFlowResult
            {
                BattleResult = result,
                SessionFingerprint = session.BuildSnapshot()
                    .LogicLedgerFingerprint
            };
        }

        private static void VerifyNianInsufficiencyRecoveryAndDedupe(
            List<string> checks)
        {
            I031NianSourceSnapshot source = NianSource(7);
            BattleSandboxNianResourceEngine engine =
                BattleSandboxNianResourceEngine.Create(source);
            BattleSandboxNianPulseResult previous = null;
            for (int pulse = 1; pulse <= 12; pulse++)
            {
                I031NianCostRequestFact cost = source.FindCostFact("I012");
                previous = engine.ApplyActualItemCostPulse(
                    source,
                    new BattleSandboxNianActualItemCostPulseRequest(
                        BattleSandboxNianActualItemCostPulseRequest
                            .CurrentSchemaId,
                        "I012_EVENT_" + pulse.ToString(
                            CultureInfo.InvariantCulture),
                        7,
                        source.canonicalSignature,
                        "I012",
                        cost.requestFactId,
                        pulse * BattleSandboxNianResourceEngine
                            .PulseIntervalTicks));
                if (pulse == 11)
                {
                    Require(!previous.accepted
                        && previous.reasonCode == "INSUFFICIENT_NIAN",
                        "NIAN_INSUFFICIENCY_NOT_REJECTED");
                }
                if (pulse == 12)
                {
                    Require(previous.accepted,
                        "NIAN_RECOVERY_NOT_ACCEPTED");
                }
            }
            string signature = engine.Current.canonicalSignature;
            I031NianCostRequestFact finalCost = source.FindCostFact("I012");
            BattleSandboxNianPulseResult duplicate =
                engine.ApplyActualItemCostPulse(
                    source,
                    new BattleSandboxNianActualItemCostPulseRequest(
                        BattleSandboxNianActualItemCostPulseRequest
                            .CurrentSchemaId,
                        "I012_EVENT_12",
                        7,
                        source.canonicalSignature,
                        "I012",
                        finalCost.requestFactId,
                        12 * BattleSandboxNianResourceEngine
                            .PulseIntervalTicks));
            Require(!duplicate.accepted
                && duplicate.reasonCode == "DUPLICATE_ITEM_REQUEST_EVENT"
                && engine.Current.canonicalSignature == signature,
                "NIAN_DUPLICATE_MUTATED_STATE");
            checks.Add("PASS I031-plus4-insufficient-recovery-dedupe");
        }

        private static void VerifyStartDriftAndReset(List<string> checks)
        {
            BattleSandboxDevBattleSessionRequest request = Request(
                BattleSandboxDevBattleSessionRequest.ProfileB,
                1,
                string.Empty,
                CoreLoopLabEncounterProfiles.Normal);
            BattleSandboxDevBattleSessionEngine engine = new();
            Require(engine.TryStart(request, true, out _),
                engine.LastDiagnosticCode);
            string before = engine.Current.NonPresentationLedgerFingerprint;
            BattleSandboxDevBattleLiveSignatures drift = new(
                "different-roster",
                request.SourceSignatures.BoardSnapshotSignature,
                request.SourceSignatures.QualifiedBuildSignature,
                request.SourceSignatures.CoreRuntimeSignature,
                request.SourceSignatures.I031SourceSignature);
            Require(!engine.TryAdvanceBy(1, drift, out _)
                && engine.LastDiagnosticCode ==
                    "DEV_SESSION_AUTHORITATIVE_SOURCE_DRIFT"
                && engine.Current.NonPresentationLedgerFingerprint == before,
                "SOURCE_DRIFT_NOT_FAIL_CLOSED");
            var mismatch = Request(
                BattleSandboxDevBattleSessionRequest.ProfileC,
                1,
                string.Empty,
                CoreLoopLabEncounterProfiles.Normal);
            Require(!engine.TryStart(mismatch, true, out _)
                && engine.LastDiagnosticCode ==
                    "DEV_SESSION_DUPLICATE_START_MISMATCH",
                "MISMATCHED_START_NOT_REJECTED");
            engine.Reset();
            Require(engine.Current == null
                && !engine.TryStart(request, true, out _)
                && engine.LastDiagnosticCode ==
                    "DEV_SESSION_STALE_GENERATION_OR_BATTLE",
                "RESET_STALE_START_NOT_REJECTED");
            checks.Add("PASS mismatch-drift-reset-stale-fail-closed");
        }

        private static BattleSandboxDevBattleSessionRequest Request(
            string profile,
            int battleIndex,
            string reward,
            CoreLoopLabEncounterProfile encounterProfile)
        {
            int generation = battleIndex;
            string encounterToken = "deterministic|battle=" + battleIndex;
            BattleSandboxExplicitDevEncounterRequest encounter =
                encounterProfile.BuildRequest(generation, encounterToken);
            I031NianSourceSnapshot source = NianSource(5);
            List<string> bases = new() { "I008", "I010", "I011", "I031" };
            if (battleIndex == 2)
            {
                bases.Add(reward);
            }
            List<BattleSandboxDevBattleItemRequestFact> facts = new()
            {
                Fact(
                    "I010",
                    37L,
                    battleIndex == 1 ? 43L : 46L,
                    1,
                    source)
            };
            if (battleIndex == 2)
            {
                facts.Add(reward switch
                {
                    "I007" => Fact("I007", 24L, 29L, 1, source),
                    "I009" => Fact("I009", 44L, 53L, 1, source),
                    _ => Fact("I012", 62L, 76L, 2, source)
                });
            }
            string mechanics = profile ==
                BattleSandboxDevBattleSessionRequest.ProfileA
                ? BattleSandboxDevBattleSessionRequest.SingleBattleMechanics
                : BattleSandboxDevBattleSessionRequest.TwoBattleMechanics;
            string[] liHuo = battleIndex == 1
                ? new[] { "I010" }
                : new[] { "I010", reward };
            BattleSandboxDevBattleLiveSignatures signatures = new(
                "roster-signature",
                "board-signature",
                "qualified-signature",
                "core-signature",
                source.canonicalSignature);
            return new BattleSandboxDevBattleSessionRequest(
                BattleSandboxDevBattleSessionRequest.CurrentSchemaId,
                true,
                BattleSandboxDevBattleSessionRequest.RequiredProductContext,
                BattleSandboxDevBattleSessionRequest.ApprovedHostContext,
                BattleSandboxDevBattleSessionRequest.ApprovedHostPackageId,
                false,
                false,
                false,
                "deterministic-session",
                5,
                profile,
                mechanics,
                battleIndex,
                battleIndex == 2 ? reward : string.Empty,
                battleIndex == 2 ? Identity[reward] : string.Empty,
                encounter,
                signatures,
                bases,
                bases.Select(value => Identity[value]).ToArray(),
                liHuo.Length,
                battleIndex == 2 ? 2 : 1,
                liHuo,
                battleIndex == 2,
                source,
                facts);
        }

        private static BattleSandboxDevBattleItemRequestFact Fact(
            string baseItemId,
            long baseDamage,
            long resolvedDamage,
            int cells,
            I031NianSourceSnapshot source)
        {
            return new BattleSandboxDevBattleItemRequestFact(
                "REQUEST|" + baseItemId,
                Identity[baseItemId],
                baseItemId,
                "PLACEMENT|" + baseItemId,
                "PROJECTION|" + Identity[baseItemId],
                source.FindCostFact(baseItemId).requestFactId,
                cells,
                baseDamage,
                resolvedDamage);
        }

        private static BattleSandboxDevBattleItemRequestFact FactOf(
            BattleSandboxDevBattleSessionRequest request,
            string baseItemId)
        {
            return request.ItemRequestFacts.Single(value => string.Equals(
                value.SourceBaseItemId,
                baseItemId,
                StringComparison.Ordinal));
        }

        private static BattleSandboxDevBattleSessionRequest WithDamageFact(
            BattleSandboxDevBattleSessionRequest source,
            string baseItemId,
            long baseDamage,
            long resolvedDamage)
        {
            BattleSandboxDevBattleItemRequestFact[] facts =
                source.ItemRequestFacts.Select(value => string.Equals(
                        value.SourceBaseItemId,
                        baseItemId,
                        StringComparison.Ordinal)
                    ? new BattleSandboxDevBattleItemRequestFact(
                        value.RequestId,
                        value.SourceItemInstanceId,
                        value.SourceBaseItemId,
                        value.SourcePlacementId,
                        value.SourceProjectionCanonicalSignature,
                        value.NianCostRequestFactId,
                        value.ShapeCellCount,
                        baseDamage,
                        resolvedDamage)
                    : value).ToArray();
            return new BattleSandboxDevBattleSessionRequest(
                source.SchemaId,
                source.DevOnly,
                source.ProductContext,
                source.HostContext,
                source.HostPackageId,
                source.RequestsFormalFlow,
                source.RequestsCampaignFlow,
                source.RequestsPlayerOrApk,
                source.HostSessionToken,
                source.ResetGeneration,
                source.LabProfileId,
                source.MechanicsProfileId,
                source.BattleIndex,
                source.SelectedRewardBaseItemId,
                source.SelectedRewardIdentityId,
                source.Encounter,
                source.SourceSignatures,
                source.AvailableBaseItemIds,
                source.AvailableIdentityIds,
                source.LiHuoQualifiedItemCount,
                source.LiHuoActiveStagePieceCount,
                source.LiHuoSourceBaseItemIds,
                source.LiHuoBuild2Active,
                source.I031Source,
                facts);
        }

        private static I031NianSourceSnapshot NianSource(int generation)
        {
            return new I031NianSourceSnapshot(
                generation,
                ItemSystemSnapshot.CurrentSchemaVersion,
                "deterministic-item-system",
                true,
                I031Location.Board,
                true,
                true,
                new[]
                {
                    new I031NianCostRequestFact("I007", 2),
                    new I031NianCostRequestFact("I008", 3),
                    new I031NianCostRequestFact("I009", 5),
                    new I031NianCostRequestFact("I010", 3),
                    new I031NianCostRequestFact("I011", 3),
                    new I031NianCostRequestFact("I012", 6)
                },
                Array.Empty<string>());
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
