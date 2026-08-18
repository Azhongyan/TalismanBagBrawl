using System;
using System.Collections.Generic;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.ChapterFlow
{
    public enum V04ChapterFlowPhase
    {
        Disabled = 0,
        ChapterSelect = 1,
        Prepare = 2,
        BattleRequested = 3,
        BattleRunning = 4,
        Settlement = 5,
        DropCandidatePreview = 6,
        BossGate = 7,
        BossChallengeReady = 8,
        ChapterComplete = 9,
        RunComplete = 10
    }

    public enum V04ChapterFlowActionType
    {
        OpenSession = 0,
        SelectChapter = 1,
        EnterPrepare = 2,
        RequestBattle = 3,
        ConfirmBattleStarted = 4,
        AcceptBattleResult = 5,
        ContinueFromSettlement = 6,
        ContinueFromDropCandidatePreview = 7,
        OpenBossGate = 8,
        ReturnToPrepareFromBossGate = 9,
        ConfirmManualBossChallenge = 10,
        ReturnToChapterSelect = 11,
        ResetSession = 12
    }

    public enum V04ChapterFlowBattleOutcome
    {
        Unknown = 0,
        Win = 1,
        Lose = 2,
        Abandon = 3
    }

    public enum V04BossResolutionMode
    {
        None = 0,
        RouteCompletion = 1,
        YieldAndAllowPassage = 2
    }

    [Serializable]
    public sealed class V04ChapterFlowAction
    {
        public V04ChapterFlowActionType actionType;
        public string transitionId = string.Empty;
        public string sessionId = string.Empty;
        public string chapterId = string.Empty;
        public string stageId = string.Empty;
        public string roundId = string.Empty;
        public string requestId = string.Empty;
        public string enemyProfileId = string.Empty;
        public string seedId = string.Empty;
        public BattleResultSnapshot battleResult;
    }

    [Serializable]
    public sealed class V04ChapterFlowTransitionResult
    {
        public bool accepted;
        public bool changed;
        public string diagnosticCode = string.Empty;
        public List<string> diagnostics = new();
        public V04ChapterFlowStateSnapshot snapshot;
        public BattleStartRequest battleStartRequest;
    }

    [Serializable]
    public sealed class V04ChapterFlowValidationIssue
    {
        public string assertionId = string.Empty;
        public string category = string.Empty;
        public string expected = string.Empty;
        public string actual = string.Empty;
        public string message = string.Empty;
    }

    [Serializable]
    public sealed class V04ChapterFlowValidationResult
    {
        public List<V04ChapterFlowValidationIssue> issues = new();

        public bool Passed => issues.Count == 0;

        public void Add(
            string assertionId,
            string category,
            string expected,
            string actual,
            string message)
        {
            issues.Add(new V04ChapterFlowValidationIssue
            {
                assertionId = assertionId ?? string.Empty,
                category = category ?? string.Empty,
                expected = expected ?? string.Empty,
                actual = actual ?? string.Empty,
                message = message ?? string.Empty
            });
        }
    }

    public static class V04ChapterFlowDiagnostics
    {
        public const string Accepted = "BLINE_FLOW_ACCEPTED";
        public const string FeatureDisabled = "BLINE_FLOW_FEATURE_DISABLED";
        public const string InvalidAction = "BLINE_FLOW_INVALID_ACTION";
        public const string InvalidTransitionId = "BLINE_FLOW_INVALID_TRANSITION_ID";
        public const string InvalidSessionId = "BLINE_FLOW_INVALID_SESSION_ID";
        public const string InvalidPhase = "BLINE_FLOW_INVALID_PHASE";
        public const string UnknownChapter = "BLINE_FLOW_UNKNOWN_CHAPTER";
        public const string ChapterLocked = "BLINE_FLOW_CHAPTER_LOCKED";
        public const string UnknownStage = "BLINE_FLOW_UNKNOWN_STAGE";
        public const string StageMismatch = "BLINE_FLOW_STAGE_MISMATCH";
        public const string RoundMismatch = "BLINE_FLOW_ROUND_MISMATCH";
        public const string RequestMismatch = "BLINE_FLOW_REQUEST_MISMATCH";
        public const string ResultRejected = "BLINE_FLOW_RESULT_REJECTED";
        public const string DuplicateResultNoOp = "BLINE_FLOW_DUPLICATE_RESULT_NOOP";
        public const string ConflictingDuplicateResult = "BLINE_FLOW_CONFLICTING_DUPLICATE_RESULT";
        public const string BossManualChallengeRequired = "BLINE_FLOW_BOSS_MANUAL_CHALLENGE_REQUIRED";
        public const string BossGateNotOpened = "BLINE_FLOW_BOSS_GATE_NOT_OPENED";
    }
}
