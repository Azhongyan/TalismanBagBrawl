using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
{
    public sealed class BoneSwapRemnantValidationResult
    {
        private BoneSwapRemnantValidationResult(IEnumerable<string> errors)
        {
            string[] normalized = (errors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Errors = new ReadOnlyCollection<string>(normalized);
            IsValid = normalized.Length == 0;
            CanonicalSignature = BoneSwapRemnantCanonical.Hash(
                IsValid ? new[] { "VALID" } : normalized);
        }

        public bool IsValid { get; }
        public IReadOnlyList<string> Errors { get; }
        public string CanonicalSignature { get; }

        public static BoneSwapRemnantValidationResult FromErrors(
            IEnumerable<string> errors)
        {
            return new BoneSwapRemnantValidationResult(errors);
        }

        internal static BoneSwapRemnantValidationResult Valid()
        {
            return new BoneSwapRemnantValidationResult(Array.Empty<string>());
        }
    }

    public static class BoneSwapRemnantRuntimeValidation
    {
        public static BoneSwapRemnantValidationResult ValidateProfile(
            BoneSwapRemnantCopyProfile profile)
        {
            List<string> errors = new List<string>();
            if (profile == null)
            {
                errors.Add("PROFILE_REQUIRED");
                return BoneSwapRemnantValidationResult.FromErrors(errors);
            }

            if (!BoneSwapRemnantCanonical.IsStableId(profile.ProfileId))
            {
                errors.Add("PROFILE_ID_REQUIRED");
            }
            else if (!string.Equals(
                profile.ProfileId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                StringComparison.Ordinal))
            {
                errors.Add("PROFILE_ID_MISMATCH");
            }

            if (!Enum.IsDefined(
                    typeof(BoneSwapRemnantTuningDisposition),
                    profile.TuningDisposition)
                || profile.TuningDisposition
                    != BoneSwapRemnantTuningDisposition.SYNTHETIC_FIXTURE_ONLY)
            {
                errors.Add("PROFILE_NOT_SYNTHETIC_FIXTURE_ONLY");
            }
            if (profile.CopyRatioBasisPoints < 1
                || profile.CopyRatioBasisPoints > 10000)
            {
                errors.Add("COPY_RATIO_OUT_OF_RANGE");
            }
            if (profile.CopyCapDamage <= 0)
            {
                errors.Add("COPY_CAP_NOT_POSITIVE");
            }
            if (profile.TelegraphTicks <= 0L)
            {
                errors.Add("TELEGRAPH_NOT_POSITIVE");
            }
            if (profile.RecoverTicks < 0L)
            {
                errors.Add("RECOVER_NEGATIVE");
            }
            if (!Enum.IsDefined(
                    typeof(BoneSwapRemnantPendingPolicy),
                    profile.PendingPolicy)
                || profile.PendingPolicy
                    != BoneSwapRemnantPendingPolicy.LATEST_WINS_ONE_SLOT)
            {
                errors.Add("PENDING_POLICY_NOT_LATEST_WINS_ONE_SLOT");
            }

            string expectedSignature = BoneSwapRemnantCanonical.Hash(
                profile.ProfileId,
                profile.TuningDisposition.ToString(),
                BoneSwapRemnantCanonical.Number(
                    profile.CopyRatioBasisPoints),
                BoneSwapRemnantCanonical.Number(profile.CopyCapDamage),
                BoneSwapRemnantCanonical.Number(profile.TelegraphTicks),
                BoneSwapRemnantCanonical.Number(profile.RecoverTicks),
                profile.PendingPolicy.ToString());
            if (!string.Equals(
                expectedSignature,
                profile.CanonicalSignature,
                StringComparison.Ordinal))
            {
                errors.Add("PROFILE_CANONICAL_MISMATCH");
            }

            return BoneSwapRemnantValidationResult.FromErrors(errors);
        }

        public static BoneSwapRemnantValidationResult ValidateCreation(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId,
            int resetGeneration,
            long initialBattleTick)
        {
            List<string> errors = new List<string>(
                ValidateProfile(profile).Errors);
            if (!BoneSwapRemnantCanonical.IsStableId(enemyInstanceId))
            {
                errors.Add("ENEMY_INSTANCE_ID_REQUIRED");
            }
            if (resetGeneration < 0)
            {
                errors.Add("RESET_GENERATION_NEGATIVE");
            }
            if (initialBattleTick < 0L)
            {
                errors.Add("INITIAL_TICK_NEGATIVE");
            }
            return BoneSwapRemnantValidationResult.FromErrors(errors);
        }

        public static BoneSwapRemnantValidationResult ValidateSnapshot(
            BoneSwapRemnantRuntimeSnapshot snapshot)
        {
            List<string> errors = new List<string>();
            if (snapshot == null)
            {
                errors.Add("SNAPSHOT_REQUIRED");
                return BoneSwapRemnantValidationResult.FromErrors(errors);
            }

            if (snapshot.SchemaId != BoneSwapRemnantRuntimeContract.SchemaId
                || snapshot.ContentId != BoneSwapRemnantRuntimeContract.ContentId
                || snapshot.OperatorProfileId
                    != BoneSwapRemnantRuntimeContract.OperatorProfileId)
            {
                errors.Add("SNAPSHOT_IDENTITY_MISMATCH");
            }
            if (!snapshot.DevOnly
                || snapshot.IsEnabled
                || snapshot.EntersFormalFlow
                || snapshot.RuntimeBoundToBattle)
            {
                errors.Add("SNAPSHOT_RELEASE_FLAGS_INVALID");
            }
            if (snapshot.ResetGeneration < 0
                || snapshot.Revision < 0L
                || snapshot.CurrentBattleTick < 0L
                || snapshot.NextActionSequence < 1L
                || snapshot.NextCueSequence < 1L
                || snapshot.NextRequestSequence < 1L)
            {
                errors.Add("SNAPSHOT_MONOTONIC_FIELDS_INVALID");
            }
            if (snapshot.PendingPulse != null
                && (snapshot.Profile == null
                    || snapshot.Profile.PendingPolicy
                        != BoneSwapRemnantPendingPolicy.LATEST_WINS_ONE_SLOT))
            {
                errors.Add("SNAPSHOT_PENDING_POLICY_INVALID");
            }
            if (snapshot.State == BoneSwapRemnantRuntimeState.Idle
                && snapshot.ActiveAction != null)
            {
                errors.Add("IDLE_HAS_ACTIVE_ACTION");
            }
            if ((snapshot.State == BoneSwapRemnantRuntimeState.Telegraphing
                    || snapshot.State == BoneSwapRemnantRuntimeState.Recovering)
                && snapshot.ActiveAction == null)
            {
                errors.Add("ACTIVE_STATE_MISSING_ACTION");
            }
            if ((snapshot.State == BoneSwapRemnantRuntimeState.Defeated
                    || snapshot.State == BoneSwapRemnantRuntimeState.Invalid)
                && (snapshot.ActiveAction != null
                    || snapshot.PendingPulse != null))
            {
                errors.Add("TERMINAL_STATE_HAS_PENDING_WORK");
            }
            if (snapshot.EffectRequests.Any(request =>
                    request.TargetRequestKind
                        != BoneSwapRemnantTargetRequestKind.PLAYER_PRIMARY_TARGET
                    || request.Amount <= 0
                    || request.EffectRequestKey
                        != BoneSwapRemnantRuntimeContract.EffectRequestKey))
            {
                errors.Add("EFFECT_REQUEST_INVALID");
            }
            if (snapshot.Cues.Any(cue =>
                cue.Kind == BoneSwapRemnantCueKind.Undefined))
            {
                errors.Add("CUE_KIND_UNDEFINED");
            }
            return BoneSwapRemnantValidationResult.FromErrors(errors);
        }

        internal static BoneSwapRemnantValidationResult ValidateFact(
            BoneSwapResolvedDirectDamageFact fact,
            BoneSwapRemnantStateData state)
        {
            List<string> errors = new List<string>();
            if (fact == null)
            {
                errors.Add("FACT_REQUIRED");
                return BoneSwapRemnantValidationResult.FromErrors(errors);
            }
            if (state.State == BoneSwapRemnantRuntimeState.Invalid)
            {
                errors.Add("OPERATOR_INVALID");
            }
            if (state.State == BoneSwapRemnantRuntimeState.Defeated)
            {
                errors.Add("OPERATOR_DEFEATED");
            }
            if (fact.SchemaId
                != BoneSwapRemnantRuntimeContract.NormalizedFactSchemaId)
            {
                errors.Add("FACT_SCHEMA_MISMATCH");
            }
            if (!BoneSwapRemnantCanonical.IsStableId(fact.SourceEventId))
            {
                errors.Add("SOURCE_EVENT_ID_REQUIRED");
            }
            else if (state.AcceptedEventIds.Contains(
                fact.SourceEventId,
                StringComparer.Ordinal))
            {
                errors.Add("DUPLICATE_EVENT_ID");
            }
            if (fact.ApplicationSequence <= 0L)
            {
                errors.Add("APPLICATION_SEQUENCE_NOT_POSITIVE");
            }
            else if (fact.ApplicationSequence
                <= state.LastAcceptedApplicationSequence)
            {
                errors.Add("STALE_APPLICATION_SEQUENCE");
            }
            if (fact.BattleTick < 0L)
            {
                errors.Add("BATTLE_TICK_NEGATIVE");
            }
            else
            {
                if (fact.BattleTick < state.CurrentBattleTick)
                {
                    errors.Add("FACT_BEFORE_OPERATOR_TICK");
                }
                if (fact.BattleTick <= state.LastAcceptedBattleTick)
                {
                    errors.Add("STALE_BATTLE_TICK");
                }
            }
            if (fact.ResetGeneration != state.ResetGeneration)
            {
                errors.Add("STALE_RESET_GENERATION");
            }
            if (!BoneSwapRemnantCanonical.IsStableId(
                    fact.RecipientEnemyInstanceId))
            {
                errors.Add("RECIPIENT_ENEMY_ID_REQUIRED");
            }
            else if (!string.Equals(
                fact.RecipientEnemyInstanceId,
                state.EnemyInstanceId,
                StringComparison.Ordinal))
            {
                errors.Add("WRONG_RECIPIENT_ENEMY");
            }
            if (!fact.AcceptedByBattleLedger)
            {
                errors.Add("BATTLE_LEDGER_REJECTED");
            }
            if (!Enum.IsDefined(
                    typeof(BoneSwapResolvedDamageSourceKind),
                    fact.SourceKind)
                || fact.SourceKind
                    != BoneSwapResolvedDamageSourceKind
                        .PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE)
            {
                errors.Add("SOURCE_KIND_NOT_DIRECT_SINGLE_TARGET");
            }
            if (fact.ResolvedDamage <= 0)
            {
                errors.Add("RESOLVED_DAMAGE_NOT_POSITIVE");
            }
            return BoneSwapRemnantValidationResult.FromErrors(errors);
        }
    }
}
