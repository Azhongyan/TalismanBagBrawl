using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
{
    public sealed class BoneSwapRemnantRuntimeOperator
    {
        private readonly BoneSwapRemnantStateData _state;

        private BoneSwapRemnantRuntimeOperator(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId,
            int resetGeneration,
            long initialBattleTick)
        {
            CreationValidation =
                BoneSwapRemnantRuntimeValidation.ValidateCreation(
                    profile,
                    enemyInstanceId,
                    resetGeneration,
                    initialBattleTick);
            _state = new BoneSwapRemnantStateData
            {
                Profile = profile,
                EnemyInstanceId = enemyInstanceId ?? string.Empty,
                State = CreationValidation.IsValid
                    ? BoneSwapRemnantRuntimeState.Idle
                    : BoneSwapRemnantRuntimeState.Invalid,
                ResetGeneration = Math.Max(0, resetGeneration),
                Revision = CreationValidation.IsValid ? 1L : 0L,
                CurrentBattleTick = Math.Max(0L, initialBattleTick)
            };
            if (!CreationValidation.IsValid)
            {
                _state.DeveloperDiagnostics.AddRange(
                    CreationValidation.Errors);
            }
        }

        public BoneSwapRemnantValidationResult CreationValidation { get; private set; }
        public BoneSwapRemnantRuntimeSnapshot Snapshot
        {
            get { return new BoneSwapRemnantRuntimeSnapshot(_state); }
        }

        public static BoneSwapRemnantRuntimeOperator Create(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId,
            int resetGeneration,
            long initialBattleTick)
        {
            return new BoneSwapRemnantRuntimeOperator(
                profile,
                enemyInstanceId,
                resetGeneration,
                initialBattleTick);
        }

        public BoneSwapRemnantTransitionResult RecordResolvedDirectDamage(
            BoneSwapResolvedDirectDamageFact fact)
        {
            BoneSwapRemnantRuntimeSnapshot before = Snapshot;
            BoneSwapRemnantValidationResult validation =
                BoneSwapRemnantRuntimeValidation.ValidateFact(fact, _state);
            if (!validation.IsValid)
            {
                return Rejected(string.Join(";", validation.Errors));
            }

            long startTick = Math.Max(_state.CurrentBattleTick, fact.BattleTick);
            long resolveTick;
            long recoveryEndTick;
            if (!BoneSwapRemnantMath.TryAddTicks(
                    startTick,
                    _state.Profile.TelegraphTicks,
                    out resolveTick)
                || !BoneSwapRemnantMath.TryAddTicks(
                    resolveTick,
                    _state.Profile.RecoverTicks,
                    out recoveryEndTick))
            {
                return Rejected("TIMING_OVERFLOW");
            }

            int boundedReturnDamage = BoneSwapRemnantMath.ComputeReturnAmount(
                fact.ResolvedDamage,
                _state.Profile.CopyRatioBasisPoints,
                _state.Profile.CopyCapDamage);
            BoneSwapRemnantCapturedPulseSnapshot pulse =
                new BoneSwapRemnantCapturedPulseSnapshot(
                    fact.SourceEventId,
                    fact.ApplicationSequence,
                    fact.BattleTick,
                    fact.ResetGeneration,
                    fact.ResolvedDamage,
                    boundedReturnDamage);

            _state.CurrentBattleTick = startTick;
            _state.Revision++;
            _state.LastAcceptedApplicationSequence = fact.ApplicationSequence;
            _state.LastAcceptedBattleTick = fact.BattleTick;
            _state.AcceptedPulseCount++;
            _state.AcceptedEventIds.Add(fact.SourceEventId);
            _state.LatestCapturedPulse = pulse;

            List<BoneSwapRemnantCueSnapshot> emittedCues =
                new List<BoneSwapRemnantCueSnapshot>();
            string diagnostic;
            if (_state.State == BoneSwapRemnantRuntimeState.Idle)
            {
                StartAction(
                    pulse,
                    startTick,
                    resolveTick,
                    recoveryEndTick,
                    emittedCues);
                diagnostic = "ACCEPTED_ACTIVE";
            }
            else
            {
                _state.PendingPulse = pulse;
                diagnostic = "ACCEPTED_PENDING_LATEST";
            }

            return Accepted(
                before,
                diagnostic,
                emittedCues,
                Array.Empty<BoneSwapRemnantEffectRequestSnapshot>());
        }

        public BoneSwapRemnantTransitionResult AdvanceTo(long battleTick)
        {
            BoneSwapRemnantRuntimeSnapshot before = Snapshot;
            if (_state.State == BoneSwapRemnantRuntimeState.Invalid)
            {
                return Rejected("OPERATOR_INVALID");
            }
            if (battleTick < 0L)
            {
                return Rejected("BATTLE_TICK_NEGATIVE");
            }
            if (battleTick < _state.CurrentBattleTick)
            {
                return Rejected("NON_MONOTONIC_ADVANCE_TICK");
            }
            if (_state.State == BoneSwapRemnantRuntimeState.Defeated)
            {
                return new BoneSwapRemnantTransitionResult(
                    true,
                    false,
                    "DEFEATED_NO_PENDING_WORK",
                    before,
                    Array.Empty<BoneSwapRemnantCueSnapshot>(),
                    Array.Empty<BoneSwapRemnantEffectRequestSnapshot>());
            }

            List<BoneSwapRemnantCueSnapshot> emittedCues =
                new List<BoneSwapRemnantCueSnapshot>();
            List<BoneSwapRemnantEffectRequestSnapshot> emittedRequests =
                new List<BoneSwapRemnantEffectRequestSnapshot>();
            string diagnostic = "ADVANCED";
            if (battleTick > _state.CurrentBattleTick)
            {
                _state.CurrentBattleTick = battleTick;
                _state.Revision++;
            }

            bool continueProcessing = true;
            while (continueProcessing)
            {
                continueProcessing = false;
                if (_state.State == BoneSwapRemnantRuntimeState.Telegraphing
                    && _state.ActiveAction != null
                    && battleTick >= _state.ActiveAction.ResolveTick)
                {
                    _state.Revision++;
                    BoneSwapRemnantCapturedPulseSnapshot pulse =
                        _state.ActiveAction.CapturedPulse;
                    if (pulse.BoundedReturnDamage > 0)
                    {
                        BoneSwapRemnantEffectRequestSnapshot request =
                            EmitRequest(_state.ActiveAction, pulse);
                        emittedRequests.Add(request);
                        emittedCues.Add(EmitCue(
                            BoneSwapRemnantCueKind.CopyReturnRequested,
                            _state.ActiveAction.ResolveTick,
                            pulse.SourceEventId));
                        _state.ActiveAction =
                            _state.ActiveAction.MarkRequestEmitted();
                    }
                    else
                    {
                        diagnostic = "COMPUTED_RETURN_NON_POSITIVE";
                        if (!_state.DeveloperDiagnostics.Contains(
                            diagnostic,
                            StringComparer.Ordinal))
                        {
                            _state.DeveloperDiagnostics.Add(diagnostic);
                        }
                    }
                    _state.State = BoneSwapRemnantRuntimeState.Recovering;
                    continueProcessing = true;
                }

                if (_state.State == BoneSwapRemnantRuntimeState.Recovering
                    && _state.ActiveAction != null
                    && battleTick >= _state.ActiveAction.RecoveryEndTick)
                {
                    long nextStartTick = _state.ActiveAction.RecoveryEndTick;
                    BoneSwapRemnantCapturedPulseSnapshot pending =
                        _state.PendingPulse;
                    _state.Revision++;
                    _state.ActiveAction = null;
                    _state.PendingPulse = null;
                    if (pending == null)
                    {
                        _state.State = BoneSwapRemnantRuntimeState.Idle;
                    }
                    else if (!TryStartPending(
                        pending,
                        nextStartTick,
                        emittedCues))
                    {
                        diagnostic = "TIMING_OVERFLOW";
                        _state.State = BoneSwapRemnantRuntimeState.Idle;
                        if (!_state.DeveloperDiagnostics.Contains(
                            diagnostic,
                            StringComparer.Ordinal))
                        {
                            _state.DeveloperDiagnostics.Add(diagnostic);
                        }
                    }
                    else
                    {
                        continueProcessing = true;
                    }
                }
            }

            return Accepted(
                before,
                diagnostic,
                emittedCues,
                emittedRequests);
        }

        public BoneSwapRemnantTransitionResult Defeat(
            string sourceEventId,
            int resetGeneration,
            long battleTick)
        {
            BoneSwapRemnantRuntimeSnapshot before = Snapshot;
            if (_state.State == BoneSwapRemnantRuntimeState.Invalid)
            {
                return Rejected("OPERATOR_INVALID");
            }
            if (_state.State == BoneSwapRemnantRuntimeState.Defeated)
            {
                return Rejected("ALREADY_DEFEATED");
            }
            if (!BoneSwapRemnantCanonical.IsStableId(sourceEventId))
            {
                return Rejected("SOURCE_EVENT_ID_REQUIRED");
            }
            if (_state.AcceptedEventIds.Contains(
                sourceEventId,
                StringComparer.Ordinal))
            {
                return Rejected("DUPLICATE_EVENT_ID");
            }
            if (resetGeneration != _state.ResetGeneration)
            {
                return Rejected("STALE_RESET_GENERATION");
            }
            if (battleTick < _state.CurrentBattleTick)
            {
                return Rejected("NON_MONOTONIC_DEFEAT_TICK");
            }

            _state.CurrentBattleTick = battleTick;
            _state.Revision++;
            _state.AcceptedEventIds.Add(sourceEventId);
            _state.ActiveAction = null;
            _state.PendingPulse = null;
            _state.State = BoneSwapRemnantRuntimeState.Defeated;
            BoneSwapRemnantCueSnapshot cue = EmitCue(
                BoneSwapRemnantCueKind.Defeated,
                battleTick,
                sourceEventId);
            return Accepted(
                before,
                "DEFEATED_PENDING_CANCELLED",
                new[] { cue },
                Array.Empty<BoneSwapRemnantEffectRequestSnapshot>());
        }

        public BoneSwapRemnantTransitionResult Reset(
            BoneSwapRemnantCopyProfile profile,
            string enemyInstanceId,
            long battleTick)
        {
            BoneSwapRemnantRuntimeSnapshot before = Snapshot;
            BoneSwapRemnantValidationResult profileValidation =
                BoneSwapRemnantRuntimeValidation.ValidateProfile(profile);
            if (!profileValidation.IsValid)
            {
                return Rejected(string.Join(";", profileValidation.Errors));
            }
            if (!BoneSwapRemnantCanonical.IsStableId(enemyInstanceId))
            {
                return Rejected("ENEMY_INSTANCE_ID_REQUIRED");
            }
            if (!string.Equals(
                enemyInstanceId,
                _state.EnemyInstanceId,
                StringComparison.Ordinal))
            {
                return Rejected("ENEMY_INSTANCE_ID_REBIND_FORBIDDEN");
            }
            if (battleTick < 0L)
            {
                return Rejected("RESET_TICK_NEGATIVE");
            }
            if (_state.ResetGeneration == int.MaxValue
                || _state.Revision == long.MaxValue)
            {
                return Rejected("RESET_COUNTER_OVERFLOW");
            }

            _state.Profile = profile;
            _state.ResetGeneration++;
            _state.Revision++;
            _state.CurrentBattleTick = battleTick;
            _state.LastAcceptedApplicationSequence = 0L;
            _state.LastAcceptedBattleTick = -1L;
            _state.AcceptedPulseCount = 0L;
            _state.LatestCapturedPulse = null;
            _state.ActiveAction = null;
            _state.PendingPulse = null;
            _state.AcceptedEventIds.Clear();
            _state.Cues.Clear();
            _state.Requests.Clear();
            _state.DeveloperDiagnostics.Clear();
            _state.State = BoneSwapRemnantRuntimeState.Idle;
            CreationValidation = BoneSwapRemnantValidationResult.Valid();
            BoneSwapRemnantCueSnapshot cue = EmitCue(
                BoneSwapRemnantCueKind.Reset,
                battleTick,
                string.Empty);
            return Accepted(
                before,
                "RESET_ACCEPTED",
                new[] { cue },
                Array.Empty<BoneSwapRemnantEffectRequestSnapshot>());
        }

        private void StartAction(
            BoneSwapRemnantCapturedPulseSnapshot pulse,
            long startTick,
            long resolveTick,
            long recoveryEndTick,
            ICollection<BoneSwapRemnantCueSnapshot> emittedCues)
        {
            long actionSequence = _state.NextActionSequence++;
            string executionId = string.Format(
                CultureInfo.InvariantCulture,
                "bone-swap-action-g{0}-s{1}",
                _state.ResetGeneration,
                actionSequence);
            _state.ActiveAction = new BoneSwapRemnantActionSnapshot(
                executionId,
                actionSequence,
                startTick,
                resolveTick,
                recoveryEndTick,
                false,
                pulse);
            _state.State = BoneSwapRemnantRuntimeState.Telegraphing;
            emittedCues.Add(EmitCue(
                BoneSwapRemnantCueKind.CopyTelegraph,
                startTick,
                pulse.SourceEventId));
        }

        private bool TryStartPending(
            BoneSwapRemnantCapturedPulseSnapshot pulse,
            long startTick,
            ICollection<BoneSwapRemnantCueSnapshot> emittedCues)
        {
            long resolveTick;
            long recoveryEndTick;
            if (!BoneSwapRemnantMath.TryAddTicks(
                    startTick,
                    _state.Profile.TelegraphTicks,
                    out resolveTick)
                || !BoneSwapRemnantMath.TryAddTicks(
                    resolveTick,
                    _state.Profile.RecoverTicks,
                    out recoveryEndTick))
            {
                return false;
            }
            StartAction(
                pulse,
                startTick,
                resolveTick,
                recoveryEndTick,
                emittedCues);
            return true;
        }

        private BoneSwapRemnantCueSnapshot EmitCue(
            BoneSwapRemnantCueKind kind,
            long battleTick,
            string sourcePulseEventId)
        {
            long sequence = _state.NextCueSequence++;
            string cueId = string.Format(
                CultureInfo.InvariantCulture,
                "bone-swap-cue-g{0}-s{1}",
                _state.ResetGeneration,
                sequence);
            BoneSwapRemnantCueSnapshot cue = new BoneSwapRemnantCueSnapshot(
                cueId,
                sequence,
                kind,
                _state.ResetGeneration,
                _state.Revision,
                _state.EnemyInstanceId,
                battleTick,
                sourcePulseEventId);
            _state.Cues.Add(cue);
            return cue;
        }

        private BoneSwapRemnantEffectRequestSnapshot EmitRequest(
            BoneSwapRemnantActionSnapshot action,
            BoneSwapRemnantCapturedPulseSnapshot pulse)
        {
            long sequence = _state.NextRequestSequence++;
            string requestId = string.Format(
                CultureInfo.InvariantCulture,
                "bone-swap-request-g{0}-s{1}",
                _state.ResetGeneration,
                sequence);
            BoneSwapRemnantEffectRequestSnapshot request =
                new BoneSwapRemnantEffectRequestSnapshot(
                    requestId,
                    sequence,
                    action.ExecutionId,
                    _state.EnemyInstanceId,
                    action.ResolveTick,
                    pulse.BoundedReturnDamage,
                    BoneSwapRemnantTargetRequestKind.PLAYER_PRIMARY_TARGET,
                    _state.ResetGeneration,
                    _state.Revision,
                    pulse.SourceEventId);
            _state.Requests.Add(request);
            return request;
        }

        private BoneSwapRemnantTransitionResult Accepted(
            BoneSwapRemnantRuntimeSnapshot before,
            string diagnostic,
            IEnumerable<BoneSwapRemnantCueSnapshot> cues,
            IEnumerable<BoneSwapRemnantEffectRequestSnapshot> requests)
        {
            BoneSwapRemnantRuntimeSnapshot after = Snapshot;
            return new BoneSwapRemnantTransitionResult(
                true,
                !string.Equals(
                    before.CanonicalSignature,
                    after.CanonicalSignature,
                    StringComparison.Ordinal),
                diagnostic,
                after,
                cues,
                requests);
        }

        private BoneSwapRemnantTransitionResult Rejected(string diagnostic)
        {
            return new BoneSwapRemnantTransitionResult(
                false,
                false,
                diagnostic,
                Snapshot,
                Array.Empty<BoneSwapRemnantCueSnapshot>(),
                Array.Empty<BoneSwapRemnantEffectRequestSnapshot>());
        }
    }
}
