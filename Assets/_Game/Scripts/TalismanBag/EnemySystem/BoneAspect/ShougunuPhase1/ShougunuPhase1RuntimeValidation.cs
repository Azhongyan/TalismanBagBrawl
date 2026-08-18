using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public static class ShougunuPhase1RuntimeValidation
    {
        public static IReadOnlyList<string> Validate(ShougunuPhase1RuntimeSnapshot snapshot)
        {
            List<string> errors = new List<string>();
            if (snapshot == null)
            {
                errors.Add("STATE_NULL");
                return errors.AsReadOnly();
            }

            Require(errors, snapshot.SchemaId == ShougunuPhase1RuntimeContract.SchemaId,
                "SCHEMA_ID_MISMATCH");
            Require(errors, snapshot.SchemaVersion == ShougunuPhase1RuntimeContract.SchemaVersion,
                "SCHEMA_VERSION_MISMATCH");
            Require(errors, snapshot.ContentId == ShougunuPhase1RuntimeContract.ContentId,
                "CONTENT_ID_MISMATCH");
            Require(errors, snapshot.PhaseId == ShougunuPhase1RuntimeContract.PhaseId,
                "PHASE_ID_MISMATCH");
            Require(errors, snapshot.DevOnly, "DEV_ONLY_REQUIRED");
            Require(errors, !snapshot.IsEnabled, "IS_ENABLED_MUST_BE_FALSE");
            Require(errors, !snapshot.EntersFormalFlow, "FORMAL_FLOW_MUST_BE_FALSE");
            Require(errors, !snapshot.RuntimeBoundToBattle, "RUNTIME_BINDING_MUST_BE_FALSE");
            Require(errors, snapshot.MaxHp == ShougunuPhase1RuntimeContract.MaxHp,
                "MAX_HP_MISMATCH");
            Require(errors, snapshot.CurrentHp >= 0 && snapshot.CurrentHp <= snapshot.MaxHp,
                "HP_OUT_OF_RANGE");
            Require(errors,
                snapshot.ShellLayerIndex >= 1
                    && snapshot.ShellLayerIndex <= snapshot.MaxSequentialShellLayers,
                "SHELL_LAYER_INDEX_OUT_OF_RANGE");
            Require(errors,
                snapshot.CurrentShell >= 0 && snapshot.CurrentShell <= snapshot.ShellLayerMax,
                "SHELL_OUT_OF_RANGE");
            Require(errors,
                snapshot.AcceptedApplicationEventIds.Distinct(StringComparer.Ordinal).Count()
                    == snapshot.AcceptedApplicationEventIds.Count,
                "APPLICATION_EVENT_ID_DUPLICATE");
            Require(errors,
                snapshot.ThresholdOccurrences.Select(item => item.ThresholdId)
                    .Distinct(StringComparer.Ordinal).Count()
                    == snapshot.ThresholdOccurrences.Count,
                "THRESHOLD_TRIGGER_DUPLICATE");
            Require(errors,
                snapshot.Cues.Select(item => item.CueId).Distinct(StringComparer.Ordinal).Count()
                    == snapshot.Cues.Count,
                "CUE_ID_DUPLICATE");
            Require(errors,
                snapshot.Cues.Select(item => item.CueSequence)
                    .SequenceEqual(snapshot.Cues.Select(item => item.CueSequence).OrderBy(item => item)),
                "CUE_SEQUENCE_NOT_MONOTONIC");

            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn)
            {
                Require(errors, snapshot.CurrentShell > 0, "SHELL_STATE_REQUIRES_POSITIVE_SHELL");
                Require(errors, snapshot.Targetable && !snapshot.Vulnerable,
                    "SHELL_STATE_TARGET_FLAGS_INVALID");
            }
            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed)
            {
                Require(errors, snapshot.CurrentShell == 0, "CORE_EXPOSE_REQUIRES_ZERO_SHELL");
                Require(errors, snapshot.Targetable && snapshot.Vulnerable,
                    "CORE_EXPOSE_TARGET_FLAGS_INVALID");
                Require(errors,
                    snapshot.CoreExposeEndTick - snapshot.CoreExposeStartTick
                        == ShougunuPhase1RuntimeContract.CoreExposeDurationTicks,
                    "CORE_EXPOSE_DURATION_MISMATCH");
            }
            if (snapshot.ShellLayerIndex == snapshot.MaxSequentialShellLayers
                && snapshot.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed)
            {
                Require(errors, !snapshot.RecoveryAvailable,
                    "FINAL_LAYER_RECOVERY_MUST_BE_FALSE");
            }
            return errors.AsReadOnly();
        }

        public static string ValidateApplication(
            ShougunuPhase1RuntimeSnapshot snapshot,
            ShougunuPhase1BattleApplication application)
        {
            if (snapshot == null) return "STATE_NULL";
            if (application == null) return "APPLICATION_NULL";
            if (string.IsNullOrWhiteSpace(application.ApplicationEventId))
                return "APPLICATION_EVENT_ID_REQUIRED";
            if (application.ApplicationSequence < 0L) return "APPLICATION_SEQUENCE_NEGATIVE";
            if (application.BattleTick < 0L) return "BATTLE_TICK_NEGATIVE";
            if (application.ShellDamageApplied < 0 || application.HpDamageApplied < 0)
                return "DAMAGE_NEGATIVE";
            if (!application.AcceptedByBattleLedger) return "BATTLE_LEDGER_REJECTED";
            if (application.TotalAppliedDamage == 0) return "ZERO_APPLIED_DAMAGE";
            if (application.NominalPulseMagnitude <= 0) return "NOMINAL_PULSE_REQUIRED";
            if (application.TotalAppliedDamage > application.NominalPulseMagnitude)
                return "APPLIED_DAMAGE_EXCEEDS_NOMINAL_PULSE";
            if (!string.Equals(
                application.EnemyInstanceId,
                snapshot.EnemyInstanceId,
                StringComparison.Ordinal))
                return "ENEMY_INSTANCE_MISMATCH";
            if (application.ResetGeneration != snapshot.ResetGeneration)
                return "RESET_GENERATION_MISMATCH";
            if (application.ApplicationSequence <= snapshot.LastAcceptedApplicationSequence)
                return "APPLICATION_SEQUENCE_NOT_INCREASING";
            if (application.BattleTick < snapshot.LastAcceptedBattleTick)
                return "BATTLE_TICK_STALE";
            if (snapshot.AcceptedApplicationEventIds.Contains(
                application.ApplicationEventId,
                StringComparer.Ordinal))
                return "APPLICATION_EVENT_DUPLICATE";
            if (snapshot.Lifecycle == ShougunuPhase1LifecycleState.Invalid
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Defeated
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.DefeatCandidate
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Presence
                || snapshot.Lifecycle == ShougunuPhase1LifecycleState.Recovering)
                return "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH";
            if (application.ShellDamageApplied > 0
                && snapshot.Lifecycle != ShougunuPhase1LifecycleState.LayeredShellOn)
                return "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH";
            if (application.HpDamageApplied > 0
                && snapshot.Lifecycle != ShougunuPhase1LifecycleState.CoreExposed
                && !(snapshot.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn
                    && application.ShellDamageApplied == snapshot.CurrentShell))
                return "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH";
            if (application.HpDamageApplied > 0
                && application.ShellDamageApplied > 0
                && application.ShellDamageApplied != snapshot.CurrentShell)
                return "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH";
            if (application.ShellDamageApplied > snapshot.CurrentShell)
                return "SHELL_DAMAGE_EXCEEDS_STATE";
            if (application.HpDamageApplied > snapshot.CurrentHp)
                return "HP_DAMAGE_EXCEEDS_STATE";
            return string.Empty;
        }

        public static IReadOnlyList<string> ValidateCatalog()
        {
            List<string> errors = new List<string>();
            IReadOnlyList<ShougunuPhase1ActionPatternSnapshot> patterns =
                ShougunuPhase1ActionPatternCatalog.GetPatterns();
            Require(errors, patterns.Count == 4, "ACTION_PATTERN_COUNT_MISMATCH");
            Require(errors,
                patterns.Select(item => item.ActionPatternId)
                    .Distinct(StringComparer.Ordinal).Count() == 4,
                "ACTION_PATTERN_ID_DUPLICATE");
            Require(errors,
                ShougunuPhase1ActionPatternCatalog.GetThresholdsDescending().Count == 6,
                "THRESHOLD_COUNT_MISMATCH");
            Require(errors,
                patterns.All(item => item.ResolveOffsetTicks
                    == item.PreCastTicks + item.TelegraphTicks + item.CastTicks),
                "ACTION_TIMING_SUM_MISMATCH");
            return errors.AsReadOnly();
        }

        private static void Require(List<string> errors, bool condition, string error)
        {
            if (!condition)
            {
                errors.Add(error);
            }
        }
    }
}
