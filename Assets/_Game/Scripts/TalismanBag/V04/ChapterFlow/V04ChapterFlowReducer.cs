using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.ChapterFlow
{
    public static class V04ChapterFlowReducer
    {
        public const string SchemaId = "V04ChapterFlowReducer.v1";

        public static V04ChapterFlowTransitionResult Reduce(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action,
            bool featureEnabled)
        {
            if (state == null || action == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidAction);
            }

            if (!featureEnabled)
            {
                return Reject(state, V04ChapterFlowDiagnostics.FeatureDisabled);
            }

            if (string.IsNullOrWhiteSpace(action.transitionId))
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidTransitionId);
            }

            if (action.actionType == V04ChapterFlowActionType.AcceptBattleResult)
            {
                V04ChapterFlowTransitionResult duplicate = CheckDuplicateResult(state, action.battleResult);
                if (duplicate != null)
                {
                    return duplicate;
                }
            }

            return action.actionType switch
            {
                V04ChapterFlowActionType.OpenSession => OpenSession(state, action),
                V04ChapterFlowActionType.SelectChapter => SelectChapter(state, action),
                V04ChapterFlowActionType.EnterPrepare => EnterPrepare(state, action),
                V04ChapterFlowActionType.RequestBattle => RequestBattle(state, action),
                V04ChapterFlowActionType.ConfirmBattleStarted => ConfirmBattleStarted(state, action),
                V04ChapterFlowActionType.AcceptBattleResult => AcceptBattleResult(state, action),
                V04ChapterFlowActionType.ContinueFromSettlement => ContinueFromSettlement(state, action),
                V04ChapterFlowActionType.ContinueFromDropCandidatePreview =>
                    ContinueFromDropCandidatePreview(state, action),
                V04ChapterFlowActionType.OpenBossGate => OpenBossGate(state, action),
                V04ChapterFlowActionType.ReturnToPrepareFromBossGate =>
                    ReturnToPrepareFromBossGate(state, action),
                V04ChapterFlowActionType.ConfirmManualBossChallenge =>
                    ConfirmManualBossChallenge(state, action),
                V04ChapterFlowActionType.ReturnToChapterSelect =>
                    ReturnToChapterSelect(state, action),
                V04ChapterFlowActionType.ResetSession => ResetSession(state, action),
                _ => Reject(state, V04ChapterFlowDiagnostics.InvalidAction)
            };
        }

        private static V04ChapterFlowTransitionResult OpenSession(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.Disabled)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            string sessionId = Normalize(action.sessionId);
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidSessionId);
            }

            V04ChapterFlowStateSnapshot next = CreateFreshSession(sessionId, action.transitionId);
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult SelectChapter(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.ChapterSelect)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterDefinition chapter = V04ChapterFlowManifest.FindChapter(action.chapterId);
            if (chapter == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownChapter);
            }

            if (!(state.unlockedChapterIds ?? new List<string>())
                .Contains(chapter.chapterId, StringComparer.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.ChapterLocked);
            }

            V04ChapterStageDefinition firstIncomplete =
                V04ChapterFlowManifest.FindFirstIncompleteStage(
                    chapter.chapterId,
                    state.completedStageIds);
            if (firstIncomplete == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.currentChapterId = chapter.chapterId;
            next.currentStageId = firstIncomplete.stageId;
            next.lastTransitionId = action.transitionId.Trim();
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult EnterPrepare(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.ChapterSelect)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            if (V04ChapterFlowManifest.FindChapter(state.currentChapterId) == null
                || V04ChapterFlowManifest.FindStage(state.currentStageId) == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.Prepare;
            next.lastTransitionId = action.transitionId.Trim();
            OpenStageEntryHooks(next, V04ChapterFlowManifest.FindStage(next.currentStageId));
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult RequestBattle(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (stage == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            bool legalNormalRequest =
                state.phase == V04ChapterFlowPhase.Prepare && !stage.isBossStage;
            bool legalBossRequest =
                state.phase == V04ChapterFlowPhase.BossChallengeReady && stage.isBossStage;
            if (!legalNormalRequest && !legalBossRequest)
            {
                return Reject(
                    state,
                    stage.isBossStage
                        ? V04ChapterFlowDiagnostics.BossManualChallengeRequired
                        : V04ChapterFlowDiagnostics.InvalidPhase);
            }

            if (!string.IsNullOrWhiteSpace(action.stageId)
                && !string.Equals(Normalize(action.stageId), stage.stageId, StringComparison.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.StageMismatch);
            }

            if (!V04ChapterFlowBattleContractProjection.TryCreateRequest(
                    stage,
                    action.requestId,
                    action.enemyProfileId,
                    action.seedId,
                    out BattleStartRequest request,
                    out string diagnosticCode))
            {
                return Reject(state, diagnosticCode);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.BattleRequested;
            next.activeBattleRequestId = request.requestId;
            next.pendingResultId = string.Empty;
            next.lastBattleOutcome = V04ChapterFlowBattleOutcome.Unknown;
            next.lastTransitionId = action.transitionId.Trim();
            return Accept(next, request);
        }

        private static V04ChapterFlowTransitionResult ConfirmBattleStarted(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.BattleRequested)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            if (!string.Equals(
                    Normalize(action.requestId),
                    state.activeBattleRequestId,
                    StringComparison.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.RequestMismatch);
            }

            if (!string.Equals(
                    Normalize(action.stageId),
                    state.currentStageId,
                    StringComparison.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.StageMismatch);
            }

            if (!string.Equals(
                    Normalize(action.roundId),
                    state.currentStageId,
                    StringComparison.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.RoundMismatch);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.BattleRunning;
            next.lastTransitionId = action.transitionId.Trim();
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult AcceptBattleResult(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.BattleRunning)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            V04BattleResultInterpretation interpretation =
                V04ChapterFlowBattleContractProjection.InterpretResult(
                    state,
                    stage,
                    action.stageId,
                    action.battleResult);
            if (!interpretation.accepted)
            {
                return Reject(
                    state,
                    string.IsNullOrWhiteSpace(interpretation.diagnosticCode)
                        ? V04ChapterFlowDiagnostics.ResultRejected
                        : interpretation.diagnosticCode,
                    interpretation.diagnostics);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.Settlement;
            next.pendingResultId = action.battleResult.resultId.Trim();
            next.lastBattleOutcome = interpretation.outcome;
            next.lastTransitionId = action.transitionId.Trim();
            next.acceptedResults.Add(new V04AcceptedBattleResultRecord
            {
                resultId = action.battleResult.resultId.Trim(),
                requestId = action.battleResult.requestId.Trim(),
                stageId = stage.stageId,
                fingerprint = interpretation.resultFingerprint
            });
            AddUnique(next.openedHookIds, stage.afterResultTutorialHookId);
            AddUnique(next.openedHookIds, stage.afterResultStoryHookId);
            return Accept(next, null, interpretation.diagnostics);
        }

        private static V04ChapterFlowTransitionResult ContinueFromSettlement(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.Settlement
                || string.IsNullOrWhiteSpace(state.pendingResultId))
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (stage == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.lastTransitionId = action.transitionId.Trim();
            if (state.lastBattleOutcome == V04ChapterFlowBattleOutcome.Win)
            {
                if (!stage.isBossStage)
                {
                    next.phase = V04ChapterFlowPhase.DropCandidatePreview;
                    return Accept(next);
                }

                AddUnique(next.completedStageIds, stage.stageId);
                AddUnique(next.openedHookIds, stage.afterBossResultTutorialHookId);
                AddUnique(next.openedHookIds, stage.afterBossResultStoryHookId);
                AddUnique(next.openedHookIds, stage.beforeNextChapterUnlockTutorialHookId);
                AddUnique(next.openedHookIds, stage.beforeNextChapterUnlockStoryHookId);
                ClearBattle(next);

                V04ChapterDefinition chapter = V04ChapterFlowManifest.FindChapter(stage.chapterId);
                V04ChapterDefinition nextChapter = V04ChapterFlowManifest.FindNextChapter(chapter);
                if (nextChapter == null)
                {
                    next.phase = V04ChapterFlowPhase.RunComplete;
                    return Accept(next);
                }

                AddUnique(next.unlockedChapterIds, nextChapter.chapterId);
                next.phase = V04ChapterFlowPhase.ChapterComplete;
                return Accept(next);
            }

            if (state.lastBattleOutcome == V04ChapterFlowBattleOutcome.Lose)
            {
                ClearBattle(next);
                next.phase = V04ChapterFlowPhase.Prepare;
                return Accept(next);
            }

            if (state.lastBattleOutcome == V04ChapterFlowBattleOutcome.Abandon)
            {
                ClearBattle(next);
                next.currentChapterId = string.Empty;
                next.currentStageId = string.Empty;
                next.phase = V04ChapterFlowPhase.ChapterSelect;
                return Accept(next);
            }

            return Reject(state, V04ChapterFlowDiagnostics.ResultRejected);
        }

        private static V04ChapterFlowTransitionResult ContinueFromDropCandidatePreview(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.DropCandidatePreview
                || state.lastBattleOutcome != V04ChapterFlowBattleOutcome.Win)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (stage == null || stage.isBossStage)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterStageDefinition nextStage = V04ChapterFlowManifest.FindNextStage(stage);
            if (nextStage == null)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            AddUnique(next.completedStageIds, stage.stageId);
            AddUnique(next.openedHookIds, stage.dropCandidateHookId);
            ClearBattle(next);
            next.currentStageId = nextStage.stageId;
            next.lastTransitionId = action.transitionId.Trim();
            if (stage.stopBeforeBossAfterWin)
            {
                next.phase = V04ChapterFlowPhase.BossGate;
            }
            else
            {
                next.phase = V04ChapterFlowPhase.Prepare;
                OpenStageEntryHooks(next, nextStage);
            }

            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult OpenBossGate(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.BossGate
                && state.phase != V04ChapterFlowPhase.Prepare)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition bossStage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (bossStage == null || !bossStage.isBossStage)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterStageDefinition gateStage =
                V04ChapterFlowManifest.FindStage(bossStage.chapterIndex + "-9");
            if (gateStage == null
                || !(state.completedStageIds ?? new List<string>())
                    .Contains(gateStage.stageId, StringComparer.Ordinal))
            {
                return Reject(state, V04ChapterFlowDiagnostics.BossGateNotOpened);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.BossGate;
            next.lastTransitionId = action.transitionId.Trim();
            AddUnique(next.openedHookIds, gateStage.beforeBossTutorialHookId);
            AddUnique(next.openedHookIds, gateStage.beforeBossStoryHookId);
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult ReturnToPrepareFromBossGate(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.BossGate)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (stage == null || !stage.isBossStage)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.Prepare;
            next.lastTransitionId = action.transitionId.Trim();
            OpenStageEntryHooks(next, stage);
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult ConfirmManualBossChallenge(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.BossGate)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(state.currentStageId);
            if (stage == null || !stage.isBossStage || !stage.requiresManualBossChallenge)
            {
                return Reject(state, V04ChapterFlowDiagnostics.UnknownStage);
            }

            V04ChapterStageDefinition gateStage =
                V04ChapterFlowManifest.FindStage(stage.chapterIndex + "-9");
            bool gateHookOpened = (state.openedHookIds ?? new List<string>())
                .Contains(gateStage.beforeBossTutorialHookId, StringComparer.Ordinal);
            if (!gateHookOpened)
            {
                return Reject(state, V04ChapterFlowDiagnostics.BossGateNotOpened);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.BossChallengeReady;
            next.lastTransitionId = action.transitionId.Trim();
            AddUnique(next.openedHookIds, stage.beforeBossChallengeTutorialHookId);
            AddUnique(next.openedHookIds, stage.beforeBossChallengeStoryHookId);
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult ReturnToChapterSelect(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            if (state.phase != V04ChapterFlowPhase.ChapterComplete)
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidPhase);
            }

            V04ChapterFlowStateSnapshot next = state.Clone();
            next.phase = V04ChapterFlowPhase.ChapterSelect;
            next.currentChapterId = string.Empty;
            next.currentStageId = string.Empty;
            next.lastTransitionId = action.transitionId.Trim();
            return Accept(next);
        }

        private static V04ChapterFlowTransitionResult ResetSession(
            V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action)
        {
            string sessionId = Normalize(action.sessionId);
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Reject(state, V04ChapterFlowDiagnostics.InvalidSessionId);
            }

            return Accept(CreateFreshSession(sessionId, action.transitionId));
        }

        private static V04ChapterFlowTransitionResult CheckDuplicateResult(
            V04ChapterFlowStateSnapshot state,
            BattleResultSnapshot result)
        {
            string resultId = Normalize(result?.resultId);
            if (string.IsNullOrWhiteSpace(resultId))
            {
                return null;
            }

            V04AcceptedBattleResultRecord existing =
                (state.acceptedResults ?? new List<V04AcceptedBattleResultRecord>())
                    .FirstOrDefault(
                        record => record != null
                            && string.Equals(record.resultId, resultId, StringComparison.Ordinal));
            if (existing == null)
            {
                return null;
            }

            string fingerprint =
                V04ChapterFlowBattleContractProjection.ComputeResultFingerprint(result);
            return string.Equals(existing.fingerprint, fingerprint, StringComparison.Ordinal)
                ? Reject(state, V04ChapterFlowDiagnostics.DuplicateResultNoOp)
                : Reject(state, V04ChapterFlowDiagnostics.ConflictingDuplicateResult);
        }

        private static V04ChapterFlowStateSnapshot CreateFreshSession(
            string sessionId,
            string transitionId)
        {
            return new V04ChapterFlowStateSnapshot
            {
                schemaId = V04ChapterFlowStateSnapshot.SchemaId,
                sessionId = sessionId,
                phase = V04ChapterFlowPhase.ChapterSelect,
                unlockedChapterIds = new List<string>
                {
                    V04ChapterFlowManifest.Chapters[0].chapterId
                },
                devOnly = true,
                isEnabled = false,
                formalFlow = false,
                lastTransitionId = transitionId.Trim()
            };
        }

        private static void OpenStageEntryHooks(
            V04ChapterFlowStateSnapshot state,
            V04ChapterStageDefinition stage)
        {
            if (state == null || stage == null)
            {
                return;
            }

            AddUnique(state.openedHookIds, stage.beforeChapterTutorialHookId);
            AddUnique(state.openedHookIds, stage.beforeChapterStoryHookId);
            AddUnique(state.openedHookIds, stage.beforeStageTutorialHookId);
            AddUnique(state.openedHookIds, stage.beforeStageStoryHookId);
        }

        private static void ClearBattle(V04ChapterFlowStateSnapshot state)
        {
            state.activeBattleRequestId = string.Empty;
            state.pendingResultId = string.Empty;
            state.lastBattleOutcome = V04ChapterFlowBattleOutcome.Unknown;
        }

        private static void AddUnique(ICollection<string> values, string value)
        {
            string normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized) || values == null)
            {
                return;
            }

            if (!values.Contains(normalized))
            {
                values.Add(normalized);
            }
        }

        private static V04ChapterFlowTransitionResult Accept(
            V04ChapterFlowStateSnapshot snapshot,
            BattleStartRequest request = null,
            IEnumerable<string> diagnostics = null)
        {
            return new V04ChapterFlowTransitionResult
            {
                accepted = true,
                changed = true,
                diagnosticCode = V04ChapterFlowDiagnostics.Accepted,
                diagnostics = (diagnostics ?? Array.Empty<string>()).ToList(),
                snapshot = snapshot,
                battleStartRequest = request
            };
        }

        private static V04ChapterFlowTransitionResult Reject(
            V04ChapterFlowStateSnapshot snapshot,
            string diagnosticCode,
            IEnumerable<string> diagnostics = null)
        {
            List<string> rows = (diagnostics ?? Array.Empty<string>()).ToList();
            if (rows.Count == 0 && !string.IsNullOrWhiteSpace(diagnosticCode))
            {
                rows.Add(diagnosticCode);
            }

            return new V04ChapterFlowTransitionResult
            {
                accepted = false,
                changed = false,
                diagnosticCode = diagnosticCode ?? string.Empty,
                diagnostics = rows,
                snapshot = snapshot,
                battleStartRequest = null
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
