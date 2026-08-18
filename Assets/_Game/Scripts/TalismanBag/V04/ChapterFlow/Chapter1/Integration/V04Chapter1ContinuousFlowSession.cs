using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.V04.ChapterFlow.Chapter1;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public enum V04Chapter1WaveDefeatDisposition
    {
        Rejected = 0,
        NextWaveReady = 1,
        StageResultReady = 2
    }

    public sealed class V04Chapter1ContinuousFlowSession
    {
        private const string SessionId = "bline.c1.continuous.session.1";
        private long transitionSequence;
        private long requestSequence;
        private int waveResetSequence;
        private bool finalWaveResultPending;
        private readonly List<string> completedWaveIds = new();

        public V04Chapter1ContinuousFlowSession()
        {
            Snapshot = V04ChapterFlowStateSnapshot.CreateDisabled();
        }

        public V04ChapterFlowStateSnapshot Snapshot { get; private set; }
        public BattleStartRequest ActiveRequest { get; private set; }
        public V04Chapter1WavePlanDefinition CurrentWavePlan
        {
            get;
            private set;
        }
        public V04Chapter1WaveEntryDefinition CurrentWaveEntry =>
            CurrentWavePlan?.actors.FirstOrDefault();
        public int CurrentWaveResetGeneration { get; private set; }
        public IReadOnlyList<string> CompletedWaveIds =>
            completedWaveIds.ToArray();

        // Compatibility alias. Values are WavePlan IDs, not the retired
        // sequential actor-as-wave IDs.
        public IReadOnlyList<string> CompletedWaveEntryIds =>
            CompletedWaveIds;

        public bool HasActiveWave =>
            Snapshot?.phase == V04ChapterFlowPhase.BattleRunning
            && CurrentWavePlan?.HasAllRuntimeProfiles == true
            && !finalWaveResultPending;
        public string LastDiagnosticCode { get; private set; } =
            "NOT_STARTED";

        public bool StartChapter1()
        {
            Snapshot = V04ChapterFlowStateSnapshot.CreateDisabled();
            ActiveRequest = null;
            transitionSequence = 0L;
            requestSequence = 0L;
            ClearWaveOwnership();
            bool started = Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.OpenSession,
                    sessionId = SessionId
                })
                && Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.SelectChapter,
                    chapterId = V04Chapter1EncounterManifest.ChapterId
                })
                && Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.EnterPrepare
                });
            return started && BindFirstWaveForCurrentStage();
        }

        public BattleStartRequest RequestCurrentBattle(
            string enemyProfileId,
            string seedId)
        {
            string projectedProfileId = enemyProfileId ?? string.Empty;
            if (IsOrdinaryStage(Snapshot?.currentStageId))
            {
                if (CurrentWavePlan == null)
                {
                    LastDiagnosticCode = "CURRENT_WAVE_PLAN_MISSING";
                    return null;
                }
                if (!CurrentWavePlan.HasAllRuntimeProfiles)
                {
                    LastDiagnosticCode =
                        V04Chapter1StageWaveEncounterTruth
                            .RuntimeProfileMissingHeldByBad3;
                    return null;
                }
                bool callerProfileAccepted =
                    string.IsNullOrWhiteSpace(projectedProfileId)
                    || CurrentWavePlan.actors.Any(value => string.Equals(
                        value.runtimeProfileId,
                        projectedProfileId,
                        StringComparison.Ordinal));
                if (!callerProfileAccepted)
                {
                    LastDiagnosticCode =
                        "CURRENT_WAVE_RUNTIME_PROFILE_MISMATCH";
                    return null;
                }

                // BattleStartRequest still has one legacy enemyProfileId
                // field. It is a compatibility projection only; ActorPlan is
                // the runtime identity authority.
                projectedProfileId =
                    CurrentWavePlan.CompatibilityRequestProfileId;
            }

            string requestId = StableId(
                "battle_request",
                ++requestSequence);
            bool accepted = Apply(new V04ChapterFlowAction
            {
                actionType = V04ChapterFlowActionType.RequestBattle,
                stageId = Snapshot.currentStageId,
                requestId = requestId,
                enemyProfileId = projectedProfileId,
                seedId = seedId ?? string.Empty
            });
            return accepted ? ActiveRequest : null;
        }

        public BattleStartRequest ProjectCurrentWaveBattleRequest()
        {
            if (Snapshot?.phase != V04ChapterFlowPhase.BattleRunning
                || ActiveRequest == null
                || CurrentWavePlan == null
                || CurrentWavePlan.isBoss
                || !CurrentWavePlan.HasAllRuntimeProfiles)
            {
                LastDiagnosticCode = CurrentWavePlan?.HasHeldRuntime == true
                    ? V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3
                    : "CURRENT_WAVE_REQUEST_PROJECTION_NOT_AVAILABLE";
                return null;
            }

            V04ChapterStageDefinition stage =
                V04ChapterFlowManifest.FindStage(
                    CurrentWavePlan.stageId);
            if (!V04ChapterFlowBattleContractProjection.TryCreateRequest(
                    stage,
                    ActiveRequest.requestId,
                    CurrentWavePlan.CompatibilityRequestProfileId,
                    ActiveRequest.seedId,
                    out BattleStartRequest projection,
                    out string diagnosticCode))
            {
                LastDiagnosticCode = diagnosticCode;
                return null;
            }
            LastDiagnosticCode = "CURRENT_WAVE_REQUEST_PROJECTED";
            return projection;
        }

        public bool ConfirmBattleStarted(BattleStartRequest request)
        {
            return request != null
                && Apply(new V04ChapterFlowAction
                {
                    actionType =
                        V04ChapterFlowActionType.ConfirmBattleStarted,
                    stageId = request.stageId,
                    roundId = request.roundId,
                    requestId = request.requestId
                });
        }

        public bool AcceptBattleResult(BattleResultSnapshot result)
        {
            if (result == null)
            {
                return false;
            }
            if (IsOrdinaryStage(Snapshot?.currentStageId)
                && !finalWaveResultPending)
            {
                LastDiagnosticCode =
                    "STAGE_RESULT_REJECTED_BEFORE_FINAL_WAVE";
                return false;
            }
            bool accepted = Apply(new V04ChapterFlowAction
                {
                    actionType =
                        V04ChapterFlowActionType.AcceptBattleResult,
                    stageId = Snapshot.currentStageId,
                    battleResult = result
                });
            if (accepted)
            {
                finalWaveResultPending = false;
            }
            return accepted;
        }

        public V04Chapter1WaveDefeatDisposition AcceptCurrentWaveDefeat(
            string waveIdentity,
            int resetGeneration)
        {
            bool identityMatches = CurrentWavePlan != null
                && (string.Equals(
                        CurrentWavePlan.waveId,
                        waveIdentity,
                        StringComparison.Ordinal)
                    || CurrentWavePlan.actors.Any(value => string.Equals(
                        value.waveEntryId,
                        waveIdentity,
                        StringComparison.Ordinal)));
            if (Snapshot?.phase != V04ChapterFlowPhase.BattleRunning
                || ActiveRequest == null
                || CurrentWavePlan == null
                || CurrentWavePlan.isBoss
                || finalWaveResultPending
                || !identityMatches
                || CurrentWaveResetGeneration != resetGeneration
                || completedWaveIds.Contains(CurrentWavePlan.waveId))
            {
                LastDiagnosticCode = "CURRENT_WAVE_DEFEAT_REJECTED";
                return V04Chapter1WaveDefeatDisposition.Rejected;
            }

            V04Chapter1StageWavePlanDefinition stagePlan =
                V04Chapter1StageWaveEncounterTruth.FindPlan(
                    CurrentWavePlan.stageId);
            int currentIndex = CurrentWavePlan.waveOrdinal - 1;
            if (stagePlan == null
                || currentIndex < 0
                || currentIndex >= stagePlan.waves.Count
                || !ReferenceEquals(
                    stagePlan.waves[currentIndex],
                    CurrentWavePlan))
            {
                LastDiagnosticCode =
                    "CURRENT_WAVE_PLAN_CORRELATION_REJECTED";
                return V04Chapter1WaveDefeatDisposition.Rejected;
            }

            completedWaveIds.Add(CurrentWavePlan.waveId);
            if (currentIndex + 1 < stagePlan.waves.Count)
            {
                CurrentWavePlan = stagePlan.waves[currentIndex + 1];
                CurrentWaveResetGeneration = NextWaveResetGeneration();
                LastDiagnosticCode =
                    CurrentWavePlan.HasAllRuntimeProfiles
                        ? "NEXT_WAVE_READY"
                        : V04Chapter1StageWaveEncounterTruth
                            .RuntimeProfileMissingHeldByBad3;
                return V04Chapter1WaveDefeatDisposition.NextWaveReady;
            }

            finalWaveResultPending = true;
            LastDiagnosticCode = "FINAL_WAVE_STAGE_RESULT_READY";
            return V04Chapter1WaveDefeatDisposition.StageResultReady;
        }

        public bool ContinueFromCurrentBoundary()
        {
            if (Snapshot.phase == V04ChapterFlowPhase.Settlement)
            {
                return Apply(new V04ChapterFlowAction
                {
                    actionType =
                        V04ChapterFlowActionType.ContinueFromSettlement
                });
            }

            if (Snapshot.phase ==
                V04ChapterFlowPhase.DropCandidatePreview)
            {
                bool continued = Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType
                        .ContinueFromDropCandidatePreview
                });
                if (continued
                    && Snapshot.phase == V04ChapterFlowPhase.BossGate)
                {
                    continued = OpenBossGate();
                }
                if (continued
                    && (Snapshot.phase == V04ChapterFlowPhase.Prepare
                        || Snapshot.phase ==
                            V04ChapterFlowPhase.BossGate))
                {
                    return BindFirstWaveForCurrentStage();
                }
                return continued;
            }

            LastDiagnosticCode = "CONTINUE_BOUNDARY_NOT_AVAILABLE";
            return false;
        }

        public bool OpenBossGate()
        {
            return Apply(new V04ChapterFlowAction
            {
                actionType = V04ChapterFlowActionType.OpenBossGate
            });
        }

        public bool ConfirmManualBossChallenge()
        {
            return Apply(new V04ChapterFlowAction
            {
                actionType =
                    V04ChapterFlowActionType.ConfirmManualBossChallenge
            });
        }

        public bool Reset()
        {
            ActiveRequest = null;
            bool reset = Apply(new V04ChapterFlowAction
            {
                actionType = V04ChapterFlowActionType.ResetSession,
                sessionId = SessionId
            });
            if (reset)
            {
                ClearWaveOwnership();
            }
            return reset;
        }

        internal bool ConfigureValidationStageFixture(
            string stageId,
            IEnumerable<string> completedStageIds)
        {
            V04Chapter1StageWavePlanDefinition stagePlan =
                V04Chapter1StageWaveEncounterTruth.FindPlan(stageId);
            if (Snapshot?.phase != V04ChapterFlowPhase.Prepare
                || stagePlan == null
                || stagePlan.waves.Count == 0)
            {
                LastDiagnosticCode =
                    "VALIDATION_STAGE_FIXTURE_REJECTED";
                return false;
            }

            V04ChapterFlowStateSnapshot fixture = Snapshot.Clone();
            fixture.currentStageId = stageId;
            fixture.completedStageIds =
                (completedStageIds ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            fixture.activeBattleRequestId = string.Empty;
            fixture.pendingResultId = string.Empty;
            fixture.lastBattleOutcome =
                V04ChapterFlowBattleOutcome.Unknown;
            fixture.phase = V04ChapterFlowPhase.Prepare;
            Snapshot = fixture;
            ActiveRequest = null;
            ClearWaveOwnership();
            bool bound = BindFirstWaveForCurrentStage();
            LastDiagnosticCode = bound
                ? "VALIDATION_STAGE_FIXTURE_READY"
                : LastDiagnosticCode;
            return bound;
        }

        public V04ChapterFlowTransitionResult Probe(
            V04ChapterFlowAction action)
        {
            if (action == null)
            {
                return V04ChapterFlowReducer.Reduce(
                    Snapshot,
                    null,
                    true);
            }
            if (string.IsNullOrWhiteSpace(action.transitionId))
            {
                action.transitionId = StableId(
                    "probe_transition",
                    transitionSequence + 1L);
            }
            return V04ChapterFlowReducer.Reduce(
                Snapshot,
                action,
                true);
        }

        public static string StableId(string category, long sequence)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "bline.c1.{0}.{1:D4}",
                category ?? "fact",
                sequence);
        }

        public static string StateFingerprint(
            V04ChapterFlowStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return "NULL";
            }
            return string.Join("|", new[]
            {
                snapshot.sessionId ?? string.Empty,
                snapshot.currentChapterId ?? string.Empty,
                snapshot.currentStageId ?? string.Empty,
                snapshot.phase.ToString(),
                snapshot.activeBattleRequestId ?? string.Empty,
                snapshot.pendingResultId ?? string.Empty,
                (snapshot.completedStageIds?.Count ?? 0)
                    .ToString(CultureInfo.InvariantCulture),
                (snapshot.unlockedChapterIds?.Count ?? 0)
                    .ToString(CultureInfo.InvariantCulture),
                (snapshot.acceptedResults?.Count ?? 0)
                    .ToString(CultureInfo.InvariantCulture)
            });
        }

        private bool Apply(V04ChapterFlowAction action)
        {
            action.transitionId = StableId(
                "transition",
                ++transitionSequence);
            V04ChapterFlowTransitionResult result =
                V04ChapterFlowReducer.Reduce(
                    Snapshot,
                    action,
                    featureEnabled: true);
            LastDiagnosticCode =
                result?.diagnosticCode ?? "REDUCER_NULL";
            if (result?.accepted != true || result.snapshot == null)
            {
                return false;
            }
            Snapshot = result.snapshot;
            if (result.battleStartRequest != null)
            {
                ActiveRequest = result.battleStartRequest;
            }
            else if (Snapshot.phase !=
                    V04ChapterFlowPhase.BattleRequested
                && Snapshot.phase !=
                    V04ChapterFlowPhase.BattleRunning)
            {
                ActiveRequest = null;
            }
            return true;
        }

        private bool BindFirstWaveForCurrentStage()
        {
            V04Chapter1StageWavePlanDefinition stagePlan =
                V04Chapter1StageWaveEncounterTruth.FindPlan(
                    Snapshot?.currentStageId);
            if (stagePlan == null || stagePlan.waves.Count == 0)
            {
                CurrentWavePlan = null;
                CurrentWaveResetGeneration = 0;
                LastDiagnosticCode =
                    "CURRENT_STAGE_WAVE_PLAN_MISSING";
                return false;
            }

            CurrentWavePlan = stagePlan.waves[0];
            finalWaveResultPending = false;
            CurrentWaveResetGeneration =
                Snapshot.phase == V04ChapterFlowPhase.Prepare
                && !CurrentWavePlan.isBoss
                    ? NextWaveResetGeneration()
                    : 0;
            LastDiagnosticCode = CurrentWavePlan.isBoss
                ? "BOSS_GATE_WAVE_PLANNED_MANUAL_CHALLENGE_REQUIRED"
                : CurrentWavePlan.HasAllRuntimeProfiles
                    ? "FIRST_WAVE_READY"
                    : V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3;
            return true;
        }

        private int NextWaveResetGeneration()
        {
            if (waveResetSequence == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "Wave reset generation exhausted.");
            }
            return ++waveResetSequence;
        }

        private void ClearWaveOwnership()
        {
            CurrentWavePlan = null;
            CurrentWaveResetGeneration = 0;
            finalWaveResultPending = false;
            completedWaveIds.Clear();
        }

        private static bool IsOrdinaryStage(string stageId)
        {
            V04ChapterStageDefinition stage =
                V04ChapterFlowManifest.FindStage(stageId);
            return stage != null
                && string.Equals(
                    stage.chapterId,
                    V04Chapter1EncounterManifest.ChapterId,
                    StringComparison.Ordinal)
                && !stage.isBossStage;
        }
    }
}
