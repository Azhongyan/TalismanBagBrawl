using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class BoneSwapRemnantRuntimeOperatorVerifier
    {
        private const string PassMarker =
            "C1_BONE_SWAP_REMNANT_RUNTIME_OPERATOR01_PASS";
#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Enemy/Verify Bone Swap Remnant Runtime Operator")]
        private static void RunFromMenu()
        {
            int exitCode = Execute();
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "Bone Swap Remnant runtime operator verification failed.");
            }
        }
#endif

        public static void RunFromCommandLine()
        {
            int exitCode = Execute();
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
#endif
        }

#if !UNITY_EDITOR
        public static int Main(string[] args)
        {
            return Execute();
        }
#endif

        private static int Execute()
        {
            VerificationOutput output = new VerificationOutput();
            RunIdentityAndSchema(output);
            RunNominalLifecycle(output);
            RunFormulaSensitivity(output);
            RunPendingLatestWins(output);
            RunNegativeFixtures(output);
            RunComputedZero(output);
            RunDefeatAndReset(output);
            RunDeterminismAndDefensiveCopies(output);
            if (output.Errors.Count == 0)
            {
                Console.WriteLine(PassMarker);
#if UNITY_EDITOR
                Debug.Log(PassMarker);
#endif
                return 0;
            }

            foreach (string error in output.Errors)
            {
                Console.Error.WriteLine(error);
            }
            return 1;
        }

        private static void RunIdentityAndSchema(VerificationOutput output)
        {
            output.Check(
                "identity.schema",
                BoneSwapRemnantRuntimeContract.SchemaId
                    == "BoneSwapRemnantRuntimeOperator.v1");
            output.Check(
                "identity.content",
                BoneSwapRemnantRuntimeContract.ContentId
                    == "bone_aspect_enemy_c1_03_bone_swap_remnant");
            output.Check(
                "identity.operator_profile",
                BoneSwapRemnantRuntimeContract.OperatorProfileId
                    == "bone_aspect.runtime.c1.bone_swap_remnant.operator.v1");
            output.Check(
                "identity.action",
                BoneSwapRemnantRuntimeContract.ActionId
                    == "c1.bone_swap_remnant.return_last_direct_pulse");
            output.Check(
                "identity.effect_request",
                BoneSwapRemnantRuntimeContract.EffectRequestKey
                    == "battle.effect_request.direct_player_damage");
            output.Check(
                "identity.release_flags",
                BoneSwapRemnantRuntimeContract.DevOnly
                    && !BoneSwapRemnantRuntimeContract.IsEnabled
                    && !BoneSwapRemnantRuntimeContract.EntersFormalFlow
                    && !BoneSwapRemnantRuntimeContract.RuntimeBoundToBattle);

            string[] factProperties = PublicPropertyNames(
                typeof(BoneSwapResolvedDirectDamageFact));
            string[] expectedFactProperties =
            {
                "AcceptedByBattleLedger",
                "ApplicationSequence",
                "BattleTick",
                "RecipientEnemyInstanceId",
                "ResetGeneration",
                "ResolvedDamage",
                "SchemaId",
                "SourceEventId",
                "SourceKind"
            };
            output.Check(
                "fact.allowed_fields_only",
                factProperties.SequenceEqual(expectedFactProperties),
                string.Join("|", factProperties));

            string[] requestProperties = PublicPropertyNames(
                typeof(BoneSwapRemnantEffectRequestSnapshot));
            string[] expectedRequestProperties =
            {
                "ActionExecutionId",
                "Amount",
                "BattleResolveTick",
                "CanonicalSignature",
                "EffectRequestKey",
                "EnemyInstanceId",
                "RequestId",
                "RequestSequence",
                "ResetGeneration",
                "Revision",
                "SourcePulseEventId",
                "TargetRequestKind"
            };
            output.Check(
                "request.neutral_fields_only",
                requestProperties.SequenceEqual(expectedRequestProperties),
                string.Join("|", requestProperties));

            Type[] immutableTypes =
            {
                typeof(BoneSwapRemnantCopyProfile),
                typeof(BoneSwapResolvedDirectDamageFact),
                typeof(BoneSwapRemnantRuntimeSnapshot),
                typeof(BoneSwapRemnantCapturedPulseSnapshot),
                typeof(BoneSwapRemnantActionSnapshot),
                typeof(BoneSwapRemnantCueSnapshot),
                typeof(BoneSwapRemnantEffectRequestSnapshot),
                typeof(BoneSwapRemnantTransitionResult),
                typeof(BoneSwapRemnantValidationResult)
            };
            output.Check(
                "types.required_immutable_properties",
                immutableTypes.All(type => type.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance)
                    .All(property => !property.CanWrite)),
                immutableTypes.Length.ToString(CultureInfo.InvariantCulture));
        }

        private static void RunNominalLifecycle(VerificationOutput output)
        {
            BoneSwapRemnantCopyProfile profile = Profile(5000, 100, 10L, 5L);
            BoneSwapRemnantRuntimeOperator runtime = Runtime(profile, "nominal");
            output.Check("nominal.creation.valid", runtime.CreationValidation.IsValid);
            output.Check(
                "nominal.initial_idle",
                runtime.Snapshot.State == BoneSwapRemnantRuntimeState.Idle);

            BoneSwapRemnantTransitionResult recorded =
                runtime.RecordResolvedDirectDamage(Fact(
                    "pulse-n1", 1L, 0L, 1, "nominal", true,
                    BoneSwapResolvedDamageSourceKind
                        .PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE,
                    180));
            output.Check(
                "nominal.record.telegraph",
                recorded.Accepted
                    && recorded.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Telegraphing
                    && recorded.Snapshot.ActiveAction != null
                    && recorded.Snapshot.ActiveAction.CapturedPulse
                        .BoundedReturnDamage == 90
                    && recorded.EmittedCues.Count == 1
                    && recorded.EmittedCues[0].Kind
                        == BoneSwapRemnantCueKind.CopyTelegraph);
            output.Trace("nominal", "record", recorded);

            BoneSwapRemnantTransitionResult beforeResolve = runtime.AdvanceTo(9L);
            output.Check(
                "nominal.before_resolve.no_request",
                beforeResolve.Snapshot.EffectRequests.Count == 0
                    && beforeResolve.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Telegraphing);
            output.Trace("nominal", "before_resolve", beforeResolve);

            BoneSwapRemnantTransitionResult resolved = runtime.AdvanceTo(10L);
            BoneSwapRemnantEffectRequestSnapshot request =
                resolved.EmittedRequests.Single();
            output.Check(
                "nominal.resolve.one_neutral_request",
                resolved.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Recovering
                    && resolved.EmittedRequests.Count == 1
                    && resolved.EmittedCues.Count == 1
                    && resolved.EmittedCues[0].Kind
                        == BoneSwapRemnantCueKind.CopyReturnRequested
                    && request.Amount == 90
                    && request.TargetRequestKind
                        == BoneSwapRemnantTargetRequestKind.PLAYER_PRIMARY_TARGET
                    && request.SourcePulseEventId == "pulse-n1"
                    && request.BattleResolveTick == 10L);
            output.Trace("nominal", "resolve", resolved);

            string resolvedSignature = resolved.Snapshot.CanonicalSignature;
            BoneSwapRemnantTransitionResult duplicateAdvance = runtime.AdvanceTo(10L);
            output.Check(
                "nominal.resolve.exactly_once",
                duplicateAdvance.EmittedRequests.Count == 0
                    && duplicateAdvance.EmittedCues.Count == 0
                    && duplicateAdvance.Snapshot.EffectRequests.Count == 1
                    && duplicateAdvance.Snapshot.CanonicalSignature
                        == resolvedSignature);

            BoneSwapRemnantTransitionResult recovered = runtime.AdvanceTo(15L);
            output.Check(
                "nominal.recover.idle",
                recovered.Snapshot.State == BoneSwapRemnantRuntimeState.Idle
                    && recovered.Snapshot.ActiveAction == null
                    && recovered.Snapshot.PendingPulse == null);
            output.Check(
                "nominal.snapshot.valid",
                BoneSwapRemnantRuntimeValidation.ValidateSnapshot(
                    recovered.Snapshot).IsValid);
            output.Trace("nominal", "idle", recovered);
        }

        private static void RunFormulaSensitivity(VerificationOutput output)
        {
            int uncapped = ResolveAmount(
                Profile(5000, 100, 3L, 1L),
                "formula-a",
                180);
            int capped = ResolveAmount(
                Profile(2500, 30, 3L, 1L),
                "formula-b",
                200);
            int secondCap = ResolveAmount(
                Profile(8000, 50, 3L, 1L),
                "formula-c",
                100);
            int maximum = ResolveAmount(
                Profile(10000, int.MaxValue, 1L, 0L),
                "formula-max",
                int.MaxValue);
            output.Check(
                "formula.uncapped_floor",
                uncapped == 90,
                uncapped.ToString(CultureInfo.InvariantCulture));
            output.Check(
                "formula.cap_hit",
                capped == 30 && secondCap == 50,
                capped + "/" + secondCap);
            output.Check(
                "formula.profile_field_sensitive",
                uncapped != capped && capped != secondCap,
                uncapped + "/" + capped + "/" + secondCap);
            output.Check(
                "formula.overflow_safe_integer_boundary",
                maximum == int.MaxValue,
                maximum.ToString(CultureInfo.InvariantCulture));
        }

        private static void RunPendingLatestWins(VerificationOutput output)
        {
            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                Profile(5000, 100, 10L, 5L),
                "pending");
            BoneSwapRemnantTransitionResult first =
                runtime.RecordResolvedDirectDamage(Fact(
                    "pending-1", 1L, 0L, 1, "pending", true,
                    DirectKind(), 100));
            string activeSignature =
                first.Snapshot.ActiveAction.CapturedPulse.CanonicalSignature;
            runtime.RecordResolvedDirectDamage(Fact(
                "pending-2", 2L, 1L, 1, "pending", true,
                DirectKind(), 110));
            runtime.RecordResolvedDirectDamage(Fact(
                "pending-3", 3L, 2L, 1, "pending", true,
                DirectKind(), 120));
            BoneSwapRemnantTransitionResult fourth =
                runtime.RecordResolvedDirectDamage(Fact(
                    "pending-4", 4L, 3L, 1, "pending", true,
                    DirectKind(), 130));
            output.Check(
                "pending.three_newer_collapse_latest",
                fourth.Snapshot.PendingPulse != null
                    && fourth.Snapshot.PendingPulse.SourceEventId == "pending-4"
                    && fourth.Snapshot.ActiveAction.CapturedPulse.CanonicalSignature
                        == activeSignature
                    && fourth.Snapshot.ActiveAction.CapturedPulse.SourceEventId
                        == "pending-1");
            output.Trace("pending", "latest_slot", fourth);

            BoneSwapRemnantTransitionResult firstResolve = runtime.AdvanceTo(10L);
            output.Check(
                "pending.active_immutable_request",
                firstResolve.EmittedRequests.Single().SourcePulseEventId
                    == "pending-1"
                    && firstResolve.EmittedRequests.Single().Amount == 50);
            BoneSwapRemnantTransitionResult recoveryEnd = runtime.AdvanceTo(15L);
            output.Check(
                "pending.starts_only_after_recovery",
                recoveryEnd.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Telegraphing
                    && recoveryEnd.Snapshot.ActiveAction.StartTick == 15L
                    && recoveryEnd.Snapshot.ActiveAction.ResolveTick == 25L
                    && recoveryEnd.Snapshot.ActiveAction.CapturedPulse.SourceEventId
                        == "pending-4"
                    && recoveryEnd.Snapshot.PendingPulse == null);
            output.Trace("pending", "second_telegraph", recoveryEnd);
            BoneSwapRemnantTransitionResult secondResolve = runtime.AdvanceTo(25L);
            output.Check(
                "pending.latest_request_once",
                secondResolve.EmittedRequests.Count == 1
                    && secondResolve.EmittedRequests[0].SourcePulseEventId
                        == "pending-4"
                    && secondResolve.EmittedRequests[0].Amount == 65
                    && secondResolve.Snapshot.EffectRequests.Count == 2);
            output.Trace("pending", "second_resolve", secondResolve);
        }

        private static void RunNegativeFixtures(VerificationOutput output)
        {
            BoneSwapRemnantRuntimeOperator sequence = Runtime(
                Profile(5000, 100, 10L, 5L), "sequence");
            sequence.RecordResolvedDirectDamage(Fact(
                "seq-accepted", 10L, 10L, 1, "sequence", true,
                DirectKind(), 100));
            NegativeTransition(
                output, "duplicate_event", "DUPLICATE_EVENT_ID", sequence,
                () => sequence.RecordResolvedDirectDamage(Fact(
                    "seq-accepted", 11L, 11L, 1, "sequence", true,
                    DirectKind(), 100)));
            NegativeTransition(
                output, "stale_sequence", "STALE_APPLICATION_SEQUENCE", sequence,
                () => sequence.RecordResolvedDirectDamage(Fact(
                    "seq-stale", 10L, 12L, 1, "sequence", true,
                    DirectKind(), 100)));
            NegativeTransition(
                output, "stale_tick", "STALE_BATTLE_TICK", sequence,
                () => sequence.RecordResolvedDirectDamage(Fact(
                    "tick-stale", 11L, 10L, 1, "sequence", true,
                    DirectKind(), 100)));

            NegativeFact(output, "stale_generation", "STALE_RESET_GENERATION",
                Fact("n-gen", 1L, 0L, 0, "negative", true, DirectKind(), 10));
            NegativeFact(output, "ledger_rejected", "BATTLE_LEDGER_REJECTED",
                Fact("n-ledger", 1L, 0L, 1, "negative", false, DirectKind(), 10));
            NegativeFact(output, "wrong_recipient", "WRONG_RECIPIENT_ENEMY",
                Fact("n-recipient", 1L, 0L, 1, "other", true, DirectKind(), 10));
            NegativeFact(output, "undefined_source_kind",
                "SOURCE_KIND_NOT_DIRECT_SINGLE_TARGET",
                Fact("n-kind", 1L, 0L, 1, "negative", true,
                    (BoneSwapResolvedDamageSourceKind)99, 10));
            NegativeFact(output, "zero_damage", "RESOLVED_DAMAGE_NOT_POSITIVE",
                Fact("n-zero", 1L, 0L, 1, "negative", true, DirectKind(), 0));
            NegativeFact(output, "negative_damage", "RESOLVED_DAMAGE_NOT_POSITIVE",
                Fact("n-negative", 1L, 0L, 1, "negative", true, DirectKind(), -1));
            NegativeFact(output, "missing_schema", "FACT_SCHEMA_MISMATCH",
                new BoneSwapResolvedDirectDamageFact(
                    string.Empty, "n-schema", 1L, 0L, 1, "negative", true,
                    DirectKind(), 10));
            NegativeFact(output, "missing_event", "SOURCE_EVENT_ID_REQUIRED",
                Fact(string.Empty, 1L, 0L, 1, "negative", true, DirectKind(), 10));
            NegativeFact(output, "missing_recipient", "RECIPIENT_ENEMY_ID_REQUIRED",
                Fact("n-no-recipient", 1L, 0L, 1, string.Empty, true,
                    DirectKind(), 10));

            CheckInvalidProfile(output, "profile_null", null, "PROFILE_REQUIRED");
            CheckInvalidProfile(output, "profile_id", new BoneSwapRemnantCopyProfile(
                "wrong", BoneSwapRemnantTuningDisposition.SYNTHETIC_FIXTURE_ONLY,
                5000, 100, 10L, 5L,
                BoneSwapRemnantPendingPolicy.LATEST_WINS_ONE_SLOT),
                "PROFILE_ID_MISMATCH");
            CheckInvalidProfile(output, "profile_disposition",
                new BoneSwapRemnantCopyProfile(
                    BoneSwapRemnantRuntimeContract.OperatorProfileId,
                    (BoneSwapRemnantTuningDisposition)99,
                    5000, 100, 10L, 5L,
                    BoneSwapRemnantPendingPolicy.LATEST_WINS_ONE_SLOT),
                "PROFILE_NOT_SYNTHETIC_FIXTURE_ONLY");
            CheckInvalidProfile(output, "ratio_zero", Profile(0, 100, 10L, 5L),
                "COPY_RATIO_OUT_OF_RANGE");
            CheckInvalidProfile(output, "ratio_high", Profile(10001, 100, 10L, 5L),
                "COPY_RATIO_OUT_OF_RANGE");
            CheckInvalidProfile(output, "cap_zero", Profile(5000, 0, 10L, 5L),
                "COPY_CAP_NOT_POSITIVE");
            CheckInvalidProfile(output, "telegraph_zero", Profile(5000, 100, 0L, 5L),
                "TELEGRAPH_NOT_POSITIVE");
            CheckInvalidProfile(output, "recover_negative", Profile(5000, 100, 10L, -1L),
                "RECOVER_NEGATIVE");
            CheckInvalidProfile(output, "pending_undefined",
                new BoneSwapRemnantCopyProfile(
                    BoneSwapRemnantRuntimeContract.OperatorProfileId,
                    BoneSwapRemnantTuningDisposition.SYNTHETIC_FIXTURE_ONLY,
                    5000, 100, 10L, 5L,
                    BoneSwapRemnantPendingPolicy.Undefined),
                "PENDING_POLICY_NOT_LATEST_WINS_ONE_SLOT");

            BoneSwapRemnantRuntimeOperator overflow = Runtime(
                Profile(5000, 100, 10L, 5L), "overflow");
            NegativeTransition(
                output, "timing_overflow", "TIMING_OVERFLOW", overflow,
                () => overflow.RecordResolvedDirectDamage(Fact(
                    "n-overflow", 1L, long.MaxValue - 5L, 1, "overflow",
                    true, DirectKind(), 100)));
        }

        private static void RunComputedZero(VerificationOutput output)
        {
            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                Profile(1, 100, 2L, 0L), "zero-return");
            runtime.RecordResolvedDirectDamage(Fact(
                "zero-return-pulse", 1L, 0L, 1, "zero-return", true,
                DirectKind(), 1));
            BoneSwapRemnantTransitionResult resolved = runtime.AdvanceTo(2L);
            output.Check(
                "computed_zero.no_request_fail_closed",
                resolved.Diagnostic == "COMPUTED_RETURN_NON_POSITIVE"
                    && resolved.EmittedRequests.Count == 0
                    && resolved.Snapshot.EffectRequests.Count == 0
                    && resolved.EmittedCues.All(cue => cue.Kind
                        != BoneSwapRemnantCueKind.CopyReturnRequested)
                    && resolved.Snapshot.DeveloperDiagnostics.Contains(
                        "COMPUTED_RETURN_NON_POSITIVE",
                        StringComparer.Ordinal));
            output.Trace("computed_zero", "resolve", resolved);
        }

        private static void RunDefeatAndReset(VerificationOutput output)
        {
            BoneSwapRemnantCopyProfile profile = Profile(5000, 100, 10L, 5L);
            BoneSwapRemnantRuntimeOperator defeated = Runtime(profile, "defeat");
            defeated.RecordResolvedDirectDamage(Fact(
                "defeat-active", 1L, 0L, 1, "defeat", true,
                DirectKind(), 100));
            defeated.RecordResolvedDirectDamage(Fact(
                "defeat-pending", 2L, 1L, 1, "defeat", true,
                DirectKind(), 120));
            BoneSwapRemnantTransitionResult defeat =
                defeated.Defeat("defeat-event", 1, 2L);
            output.Check(
                "defeat.cancels_active_and_pending",
                defeat.Accepted
                    && defeat.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Defeated
                    && defeat.Snapshot.ActiveAction == null
                    && defeat.Snapshot.PendingPulse == null
                    && defeat.EmittedCues.Single().Kind
                        == BoneSwapRemnantCueKind.Defeated);
            int requestsAtDefeat = defeat.Snapshot.EffectRequests.Count;
            BoneSwapRemnantTransitionResult afterDefeat = defeated.AdvanceTo(100L);
            output.Check(
                "defeat.no_later_request",
                afterDefeat.Snapshot.EffectRequests.Count == requestsAtDefeat
                    && afterDefeat.EmittedRequests.Count == 0
                    && !afterDefeat.StateChanged);
            output.Trace("defeat", "cancelled", defeat);

            BoneSwapRemnantRuntimeOperator reset = Runtime(profile, "reset");
            reset.RecordResolvedDirectDamage(Fact(
                "reset-reusable", 1L, 0L, 1, "reset", true,
                DirectKind(), 100));
            reset.AdvanceTo(10L);
            BoneSwapRemnantRuntimeSnapshot beforeReset = reset.Snapshot;
            BoneSwapRemnantTransitionResult resetResult =
                reset.Reset(profile, "reset", 0L);
            output.Check(
                "reset.clears_and_increments_generation",
                resetResult.Accepted
                    && resetResult.Snapshot.ResetGeneration == 2
                    && resetResult.Snapshot.Revision > beforeReset.Revision
                    && resetResult.Snapshot.State
                        == BoneSwapRemnantRuntimeState.Idle
                    && resetResult.Snapshot.LatestCapturedPulse == null
                    && resetResult.Snapshot.ActiveAction == null
                    && resetResult.Snapshot.PendingPulse == null
                    && resetResult.Snapshot.EffectRequests.Count == 0
                    && resetResult.Snapshot.AcceptedEventIds.Count == 0
                    && resetResult.Snapshot.Cues.Count == 1
                    && resetResult.Snapshot.Cues[0].Kind
                        == BoneSwapRemnantCueKind.Reset
                    && resetResult.Snapshot.NextCueSequence
                        > beforeReset.NextCueSequence - 1L
                    && resetResult.Snapshot.NextRequestSequence
                        >= beforeReset.NextRequestSequence);
            NegativeTransition(
                output, "reset_rejects_old_generation", "STALE_RESET_GENERATION",
                reset,
                () => reset.RecordResolvedDirectDamage(Fact(
                    "old-generation", 2L, 1L, 1, "reset", true,
                    DirectKind(), 100)));
            BoneSwapRemnantTransitionResult reused =
                reset.RecordResolvedDirectDamage(Fact(
                    "reset-reusable", 1L, 0L, 2, "reset", true,
                    DirectKind(), 100));
            output.Check(
                "reset.clears_dedupe_for_new_generation",
                reused.Accepted
                    && reused.Snapshot.LastAcceptedApplicationSequence == 1L
                    && reused.Snapshot.ResetGeneration == 2);
            output.Trace("reset", "new_generation", reused);
        }

        private static void RunDeterminismAndDefensiveCopies(
            VerificationOutput output)
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
            string first;
            string repeated;
            string alternateCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                first = DeterministicFixtureSignature();
                repeated = DeterministicFixtureSignature();
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
                alternateCulture = DeterministicFixtureSignature();
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
            output.Check(
                "canonical.repeat_and_culture",
                first == repeated && first == alternateCulture,
                first);

            List<string> callerErrors = new List<string> { "zeta", "alpha" };
            BoneSwapRemnantValidationResult ordered =
                BoneSwapRemnantValidationResult.FromErrors(callerErrors);
            BoneSwapRemnantValidationResult reversed =
                BoneSwapRemnantValidationResult.FromErrors(
                    callerErrors.AsEnumerable().Reverse());
            callerErrors.Clear();
            output.Check(
                "canonical.reversed_input",
                ordered.CanonicalSignature == reversed.CanonicalSignature
                    && ordered.Errors.SequenceEqual(
                        new[] { "alpha", "zeta" }));
            output.Check(
                "defensive.caller_mutation_isolated",
                ordered.Errors.Count == 2);

            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                Profile(5000, 100, 2L, 1L), "defensive");
            BoneSwapRemnantTransitionResult transition =
                runtime.RecordResolvedDirectDamage(Fact(
                    "defensive-pulse", 1L, 0L, 1, "defensive", true,
                    DirectKind(), 100));
            bool idsReadOnly = MutationBlocked(
                transition.Snapshot.AcceptedEventIds as IList<string>,
                "mutate");
            bool cuesReadOnly = MutationBlocked(
                transition.EmittedCues as IList<BoneSwapRemnantCueSnapshot>,
                null);
            output.Check(
                "defensive.snapshot_and_transition_collections",
                idsReadOnly && cuesReadOnly
                    && transition.Snapshot.AcceptedEventIds.Count == 1
                    && transition.EmittedCues.Count == 1);

            BoneSwapRemnantCopyProfile baseProfile = Profile(5000, 100, 2L, 1L);
            BoneSwapRemnantCopyProfile changedRatio = Profile(5001, 100, 2L, 1L);
            BoneSwapRemnantCopyProfile changedCap = Profile(5000, 101, 2L, 1L);
            BoneSwapRemnantCopyProfile changedTiming = Profile(5000, 100, 3L, 1L);
            output.Check(
                "canonical.profile_fields_sensitive",
                new[]
                {
                    baseProfile.CanonicalSignature,
                    changedRatio.CanonicalSignature,
                    changedCap.CanonicalSignature,
                    changedTiming.CanonicalSignature
                }.Distinct(StringComparer.Ordinal).Count() == 4);
        }

        private static BoneSwapRemnantRuntimeOperator Runtime(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId)
        {
            return BoneSwapRemnantRuntimeOperator.Create(
                profile,
                enemyInstanceId,
                1,
                0L);
        }

        private static BoneSwapRemnantCopyProfile Profile(
            int ratio,
            int cap,
            long telegraph,
            long recover)
        {
            return new BoneSwapRemnantCopyProfile(
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                BoneSwapRemnantTuningDisposition.SYNTHETIC_FIXTURE_ONLY,
                ratio,
                cap,
                telegraph,
                recover,
                BoneSwapRemnantPendingPolicy.LATEST_WINS_ONE_SLOT);
        }

        private static BoneSwapResolvedDirectDamageFact Fact(
            string eventId,
            long sequence,
            long tick,
            int generation,
            string recipient,
            bool accepted,
            BoneSwapResolvedDamageSourceKind sourceKind,
            int damage)
        {
            return new BoneSwapResolvedDirectDamageFact(
                BoneSwapRemnantRuntimeContract.NormalizedFactSchemaId,
                eventId,
                sequence,
                tick,
                generation,
                recipient,
                accepted,
                sourceKind,
                damage);
        }

        private static BoneSwapResolvedDamageSourceKind DirectKind()
        {
            return BoneSwapResolvedDamageSourceKind
                .PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE;
        }

        private static int ResolveAmount(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId,
            int damage)
        {
            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                profile,
                enemyInstanceId);
            runtime.RecordResolvedDirectDamage(Fact(
                "formula-pulse", 1L, 0L, 1, enemyInstanceId, true,
                DirectKind(), damage));
            return runtime.AdvanceTo(profile.TelegraphTicks)
                .EmittedRequests.Single().Amount;
        }

        private static void NegativeFact(
            VerificationOutput output,
            string caseId,
            string expected,
            BoneSwapResolvedDirectDamageFact fact)
        {
            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                Profile(5000, 100, 10L, 5L), "negative");
            NegativeTransition(
                output,
                caseId,
                expected,
                runtime,
                () => runtime.RecordResolvedDirectDamage(fact));
        }

        private static void NegativeTransition(
            VerificationOutput output,
            string caseId,
            string expected,
            BoneSwapRemnantRuntimeOperator runtime,
            Func<BoneSwapRemnantTransitionResult> action)
        {
            BoneSwapRemnantRuntimeSnapshot before = runtime.Snapshot;
            BoneSwapRemnantTransitionResult result = action();
            bool passed = !result.Accepted
                && !result.StateChanged
                && result.Diagnostic.Contains(expected)
                && result.Snapshot.CanonicalSignature == before.CanonicalSignature
                && result.EmittedRequests.Count == 0
                && result.EmittedCues.Count == 0;
            output.Negative(
                caseId,
                expected,
                result.Diagnostic,
                result.StateChanged ? 1 : 0,
                result.EmittedRequests.Count,
                result.EmittedCues.Count,
                passed);
        }

        private static void CheckInvalidProfile(
            VerificationOutput output,
            string caseId,
            BoneSwapRemnantCopyProfile profile,
            string expected)
        {
            BoneSwapRemnantRuntimeOperator runtime =
                BoneSwapRemnantRuntimeOperator.Create(
                    profile,
                    "invalid-profile",
                    1,
                    0L);
            bool passed = !runtime.CreationValidation.IsValid
                && runtime.CreationValidation.Errors.Contains(
                    expected,
                    StringComparer.Ordinal)
                && runtime.Snapshot.State == BoneSwapRemnantRuntimeState.Invalid
                && runtime.Snapshot.ActiveAction == null
                && runtime.Snapshot.PendingPulse == null
                && runtime.Snapshot.EffectRequests.Count == 0;
            output.Negative(
                caseId,
                expected,
                string.Join(";", runtime.CreationValidation.Errors),
                0,
                0,
                0,
                passed);
        }

        private static string DeterministicFixtureSignature()
        {
            BoneSwapRemnantRuntimeOperator runtime = Runtime(
                Profile(3333, 77, 7L, 3L), "deterministic");
            runtime.RecordResolvedDirectDamage(Fact(
                "det-1", 1L, 0L, 1, "deterministic", true,
                DirectKind(), 300));
            runtime.RecordResolvedDirectDamage(Fact(
                "det-2", 2L, 1L, 1, "deterministic", true,
                DirectKind(), 150));
            runtime.AdvanceTo(7L);
            runtime.AdvanceTo(10L);
            runtime.AdvanceTo(17L);
            return runtime.Snapshot.CanonicalSignature;
        }

        private static bool MutationBlocked<T>(IList<T> list, T value)
        {
            if (list == null)
            {
                return false;
            }
            try
            {
                list.Add(value);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static string[] PublicPropertyNames(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        private sealed class VerificationOutput
        {
            public readonly List<string> Errors = new List<string>();
            public readonly List<string[]> SpecRows = new List<string[]>();
            public readonly List<string[]> TraceRows = new List<string[]>();
            public readonly List<string[]> NegativeRows = new List<string[]>();
            public void Check(string id, bool passed, string detail = "")
            {
                SpecRows.Add(new[] { id, passed ? "PASS" : "FAIL", detail });
                if (!passed)
                {
                    Errors.Add(id);
                }
            }

            public void Negative(
                string caseId,
                string expected,
                string actual,
                int stateChanged,
                int requestDelta,
                int cueDelta,
                bool passed)
            {
                NegativeRows.Add(new[]
                {
                    caseId,
                    expected,
                    actual,
                    stateChanged.ToString(CultureInfo.InvariantCulture),
                    requestDelta.ToString(CultureInfo.InvariantCulture),
                    cueDelta.ToString(CultureInfo.InvariantCulture),
                    passed ? "PASS" : "FAIL"
                });
                if (!passed)
                {
                    Errors.Add("negative." + caseId);
                }
            }

            public void Trace(
                string scenario,
                string step,
                BoneSwapRemnantTransitionResult transition)
            {
                BoneSwapRemnantRuntimeSnapshot snapshot = transition.Snapshot;
                TraceRows.Add(new[]
                {
                    scenario,
                    step,
                    snapshot.CurrentBattleTick.ToString(CultureInfo.InvariantCulture),
                    snapshot.State.ToString(),
                    snapshot.ActiveAction == null
                        ? string.Empty
                        : snapshot.ActiveAction.CapturedPulse.SourceEventId,
                    snapshot.PendingPulse == null
                        ? string.Empty
                        : snapshot.PendingPulse.SourceEventId,
                    snapshot.EffectRequests.Count.ToString(CultureInfo.InvariantCulture),
                    snapshot.Cues.Count.ToString(CultureInfo.InvariantCulture),
                    transition.Diagnostic,
                    snapshot.CanonicalSignature
                });
            }
        }
    }
}
