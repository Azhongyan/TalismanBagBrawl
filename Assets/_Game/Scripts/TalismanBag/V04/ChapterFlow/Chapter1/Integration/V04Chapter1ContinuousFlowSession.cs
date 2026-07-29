using System;
using System.Globalization;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public sealed class V04Chapter1ContinuousFlowSession
    {
        private const string SessionId = "bline.c1.continuous.session.1";
        private long transitionSequence;
        private long requestSequence;

        public V04Chapter1ContinuousFlowSession()
        {
            Snapshot = V04ChapterFlowStateSnapshot.CreateDisabled();
        }

        public V04ChapterFlowStateSnapshot Snapshot { get; private set; }
        public BattleStartRequest ActiveRequest { get; private set; }
        public string LastDiagnosticCode { get; private set; } = "NOT_STARTED";

        public bool StartChapter1()
        {
            Snapshot = V04ChapterFlowStateSnapshot.CreateDisabled();
            ActiveRequest = null;
            transitionSequence = 0L;
            requestSequence = 0L;
            return Apply(new V04ChapterFlowAction
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
        }

        public BattleStartRequest RequestCurrentBattle(
            string enemyProfileId,
            string seedId)
        {
            string requestId = StableId("battle_request", ++requestSequence);
            bool accepted = Apply(new V04ChapterFlowAction
            {
                actionType = V04ChapterFlowActionType.RequestBattle,
                stageId = Snapshot.currentStageId,
                requestId = requestId,
                enemyProfileId = enemyProfileId ?? string.Empty,
                seedId = seedId ?? string.Empty
            });
            return accepted ? ActiveRequest : null;
        }

        public bool ConfirmBattleStarted(BattleStartRequest request)
        {
            return request != null
                && Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.ConfirmBattleStarted,
                    stageId = request.stageId,
                    roundId = request.roundId,
                    requestId = request.requestId
                });
        }

        public bool AcceptBattleResult(BattleResultSnapshot result)
        {
            return result != null
                && Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    stageId = Snapshot.currentStageId,
                    battleResult = result
                });
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

            if (Snapshot.phase == V04ChapterFlowPhase.DropCandidatePreview)
            {
                bool continued = Apply(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType
                        .ContinueFromDropCandidatePreview
                });
                if (continued && Snapshot.phase == V04ChapterFlowPhase.BossGate)
                {
                    return OpenBossGate();
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
            return Apply(new V04ChapterFlowAction
            {
                actionType = V04ChapterFlowActionType.ResetSession,
                sessionId = SessionId
            });
        }

        public V04ChapterFlowTransitionResult Probe(
            V04ChapterFlowAction action)
        {
            if (action == null)
            {
                return V04ChapterFlowReducer.Reduce(Snapshot, null, true);
            }
            if (string.IsNullOrWhiteSpace(action.transitionId))
            {
                action.transitionId = StableId(
                    "probe_transition",
                    transitionSequence + 1L);
            }
            return V04ChapterFlowReducer.Reduce(Snapshot, action, true);
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
            LastDiagnosticCode = result?.diagnosticCode ?? "REDUCER_NULL";
            if (result?.accepted != true || result.snapshot == null)
            {
                return false;
            }
            Snapshot = result.snapshot;
            if (result.battleStartRequest != null)
            {
                ActiveRequest = result.battleStartRequest;
            }
            else if (Snapshot.phase != V04ChapterFlowPhase.BattleRequested
                && Snapshot.phase != V04ChapterFlowPhase.BattleRunning)
            {
                ActiveRequest = null;
            }
            return true;
        }
    }
}
