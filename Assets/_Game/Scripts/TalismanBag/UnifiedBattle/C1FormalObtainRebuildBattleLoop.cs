using System;
using System.Globalization;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.V04.Campaign.Chapter1;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.WorldMap;

namespace TalismanBag.UnifiedBattle
{
    public enum C1FormalObtainRebuildBattlePhase
    {
        Inactive = 0,
        StageOneBattle = 1,
        StageOneResult = 2,
        RebuildForStageTwo = 3,
        StageTwoBattle = 4,
        StageTwoResult = 5,
        RewardClaimed = 6
    }

    // Compatibility projection for callers that still render the accepted
    // Reward contract pieces. The authority remains the direct-loot bundle.
    public sealed class C1FormalFirstClearReceipt
    {
        internal C1FormalFirstClearReceipt(
            C1CampaignStageRewardProgressResult accepted)
        {
            stageClearFact = accepted?.grant?.stageClearFact;
            dropRequest = accepted?.grant?.dropRequest;
            dropRollResult = accepted?.grant?.dropRollResult;
            rewardResult = accepted?.grant?.rewardResult;
            claimIdentity = accepted?.grant?.claimIdentity;
            dedupeIdentity = accepted?.grant?.dedupeIdentity;
            dedupePolicyId = string.Empty;
        }

        public StageClearFact stageClearFact { get; }
        public DropRequest dropRequest { get; }
        public DropRollResult dropRollResult { get; }
        public RewardResult rewardResult { get; }
        public RewardClaimIdentity claimIdentity { get; }
        public RewardDedupeIdentity dedupeIdentity { get; }
        public string dedupePolicyId { get; }
    }

    public sealed class C1FormalObtainRebuildBattleLoop
    {
        public const string CreatedDiagnostic =
            "C1_FORMAL_LOOP_LIVE_SESSION_STARTED";
        public const string LiveTickAcceptedDiagnostic =
            "C1_FORMAL_LOOP_LIVE_TICK_ACCEPTED";
        public const string RealtimePauseAcceptedDiagnostic =
            "C1_FORMAL_LOOP_REALTIME_PAUSE_ACCEPTED";
        public const string RealtimeResumeAcceptedDiagnostic =
            "C1_FORMAL_LOOP_REALTIME_RESUME_ACCEPTED";
        public const string RealtimeItemRefreshAcceptedDiagnostic =
            "C1_FORMAL_LOOP_REALTIME_ITEM_REFRESH_ACCEPTED";
        public const string EntitlementAcceptedDiagnostic =
            "C1_FORMAL_LOOP_DIRECT_LOOT_ITEM_COMPLETION_ACCEPTED";
        public const string StageTwoAcceptedDiagnostic =
            "C1_FORMAL_LOOP_STAGE_RESULT_ACCEPTED";
        public const string StageTwoStartedDiagnostic =
            "C1_FORMAL_LOOP_NEXT_STAGE_LIVE_SESSION_STARTED";
        public const string DuplicateAcceptedDiagnostic =
            "C1_FORMAL_LOOP_DUPLICATE_ACCEPTED_NO_OP";
        public const string RewardClaimedDiagnostic =
            "C1_FORMAL_LOOP_REWARD_CLAIMED";
        public const string RebuildStartedDiagnostic =
            "C1_FORMAL_LOOP_REBUILD_STARTED";
        public const string PhaseRejectedDiagnostic =
            "C1_FORMAL_LOOP_PHASE_REJECTED";
        public const string ContextRejectedDiagnostic =
            "C1_FORMAL_LOOP_CONTEXT_REJECTED";
        public const string StageTwoConfigRejectedDiagnostic =
            "C1_FORMAL_LOOP_NEXT_STAGE_CONFIG_REJECTED";
        public const string ReturnRequiredDiagnostic =
            "C1_FORMAL_LOOP_RETURN_WORLD_MAP_REQUIRED";

        private C1FormalRealtimeBattleSessionAdapter realtimeBattleAdapter;
        private C1FormalItemSessionAuthority itemAuthority;
        private C1CampaignStageClearRewardProgressBridge rewardProgressBridge;
        private BattleLaunchContext initialContext;
        private BattleLaunchContext currentContext;
        private BattleLaunchContext stageTwoContext;
        private C1FormalFirstClearReceipt firstClearReceipt;
        private C1CampaignStageRewardProgressResult lastRewardProgress;
        private C1FormalRealtimeBattleSessionStartSnapshot currentRealtimeStart;
        private C1FormalRealtimeBattleSessionStateSnapshot realtimeState;
        private C1FormalRealtimeBattleTickResult lastRealtimeTick;
        private C1FormalRealtimeBattleTerminalResult stageOneRealtimeTerminal;
        private C1FormalRealtimeBattleTerminalResult stageTwoRealtimeTerminal;
        private C1FormalRealtimeBattleTerminalResult currentRealtimeTerminal;
        private C1FormalObtainRebuildBattlePhase phase;
        private readonly bool replayRun;
        private string pendingNextStageId = string.Empty;
        private bool returnWorldMapAfterRebuild;

        private C1FormalObtainRebuildBattleLoop(
            BattleLaunchContext acceptedContext,
            C1FormalRealtimeBattleSessionAdapter configuredRealtimeBattleAdapter,
            C1FormalItemSessionAuthority configuredItemAuthority,
            C1CampaignStageClearRewardProgressBridge configuredBridge,
            C1FormalRealtimeBattleSessionStartSnapshot acceptedStart,
            bool replayRun)
        {
            initialContext = acceptedContext;
            currentContext = acceptedContext;
            realtimeBattleAdapter = configuredRealtimeBattleAdapter;
            itemAuthority = configuredItemAuthority;
            rewardProgressBridge = configuredBridge;
            currentRealtimeStart = acceptedStart;
            realtimeState = acceptedStart.stateSnapshot;
            this.replayRun = replayRun;
            phase = C1FormalObtainRebuildBattlePhase.StageOneBattle;
        }

        public C1FormalObtainRebuildBattlePhase Phase => phase;
        public C1FormalItemSessionAuthority ItemAuthority => itemAuthority;
        public BattleLaunchContext StageOneContext => initialContext;
        public BattleLaunchContext StageTwoContext => stageTwoContext;
        public BattleLaunchContext CurrentContext => currentContext;
        public string CurrentStageId => currentContext == null
            ? string.Empty
            : currentContext.StageId;
        public string PendingNextStageId => pendingNextStageId;
        public bool IsReplayRun => replayRun;
        public bool ShouldReturnWorldMapAfterRebuild =>
            returnWorldMapAfterRebuild;
        public C1FormalFirstClearReceipt FirstClearReceipt => firstClearReceipt;
        public C1CampaignStageRewardProgressResult LastRewardProgress =>
            lastRewardProgress;
        public C1FormalRealtimeBattleSessionStartSnapshot CurrentRealtimeStart =>
            currentRealtimeStart;
        public C1FormalRealtimeBattleSessionStateSnapshot RealtimeState =>
            realtimeState;
        public C1FormalRealtimeBattleTickResult LastRealtimeTick =>
            lastRealtimeTick;
        public C1FormalRealtimeBattleTerminalResult StageOneRealtimeTerminal =>
            stageOneRealtimeTerminal;
        public C1FormalRealtimeBattleTerminalResult StageTwoRealtimeTerminal =>
            stageTwoRealtimeTerminal;
        public C1FormalRealtimeBattleTerminalResult CurrentRealtimeTerminal =>
            currentRealtimeTerminal;
        public bool IsRealtimeBattlePaused =>
            realtimeBattleAdapter != null && realtimeBattleAdapter.IsPaused;

        public static bool TryCreate(
            BattleLaunchContext consumedContext,
            Chapter1CampaignStageConfig consumedConfig,
            CanonicalItemDefinitionResolver canonicalItemResolver,
            out C1FormalObtainRebuildBattleLoop loop,
            out string diagnostic)
        {
            loop = null;
            string configDiagnostic = string.Empty;
            if (canonicalItemResolver == null || consumedContext == null
                || consumedConfig == null
                || !consumedConfig.TryValidate(out configDiagnostic)
                || !consumedConfig.MatchesContext(consumedContext)
                || !consumedConfig.RouteEnabled
                || !consumedConfig.BattleContentReady
                || !string.Equals(
                    consumedContext.ProductContext,
                    Chapter1CampaignStageConfig.ProductContextId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    consumedContext.ChapterId,
                    Chapter1CampaignStageConfig.ChapterIdValue,
                    StringComparison.Ordinal))
            {
                diagnostic = ContextRejectedDiagnostic + " " +
                    (string.IsNullOrEmpty(configDiagnostic)
                        ? "INVALID_STAGE_ENVELOPE"
                        : configDiagnostic);
                return false;
            }

            string campaignRunSessionId = StableId(
                consumedContext,
                "campaign-run-session");
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    campaignRunSessionId,
                    consumedContext.Generation,
                    canonicalItemResolver);
            if (creation == null || !creation.isSuccess
                || creation.authority == null)
            {
                diagnostic = creation == null
                    ? "C1_FORMAL_LOOP_ITEM_AUTHORITY_RESULT_MISSING"
                    : creation.diagnosticCode;
                return false;
            }

            V04CampaignStageCompletionRepository completion =
                new V04CampaignStageCompletionRepository();
            bool replay = completion.Load().IsCleared(consumedContext.StageId);
            if (!C1CampaignStageClearRewardProgressBridge.TryCreate(
                    creation.authority,
                    completion,
                    campaignRunSessionId,
                    consumedContext.Generation,
                    out C1CampaignStageClearRewardProgressBridge bridge,
                    out diagnostic))
            {
                return false;
            }

            C1FormalRealtimeBattleSessionAdapter realtimeAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            if (!TryStartRealtime(
                    consumedContext,
                    creation.authority,
                    bridge,
                    realtimeAdapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot start,
                    out diagnostic))
            {
                realtimeAdapter.Unbind();
                return false;
            }

            loop = new C1FormalObtainRebuildBattleLoop(
                consumedContext,
                realtimeAdapter,
                creation.authority,
                bridge,
                start,
                replay);
            diagnostic = CreatedDiagnostic;
            return true;
        }

        public bool TryAdvanceBy(long deltaMs, out string diagnostic)
        {
            if (!IsBattlePhase() || realtimeBattleAdapter == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            if (!realtimeBattleAdapter.TryAdvanceBy(
                    deltaMs,
                    out C1FormalRealtimeBattleTickResult tick)
                || tick == null || !tick.accepted || tick.stateSnapshot == null)
            {
                lastRealtimeTick = tick;
                realtimeState = tick == null ? realtimeState : tick.stateSnapshot;
                diagnostic = tick == null
                    ? "C1_FORMAL_LOOP_LIVE_TICK_RESULT_MISSING"
                    : tick.errorCode;
                return false;
            }

            lastRealtimeTick = tick;
            realtimeState = tick.stateSnapshot;
            if (tick.terminalResult == null)
            {
                diagnostic = LiveTickAcceptedDiagnostic;
                return true;
            }

            C1FormalRealtimeBattleTerminalResult terminal = tick.terminalResult;
            if (!terminal.accepted || terminal.battleResultSnapshot == null
                || terminal.shouldWriteSave || terminal.shouldGrantReward
                || terminal.win == terminal.lose
                || !string.Equals(
                    terminal.stageId,
                    CurrentStageId,
                    StringComparison.Ordinal))
            {
                diagnostic = terminal.errorCode;
                return false;
            }

            currentRealtimeTerminal = terminal;
            bool initialBattle = phase ==
                C1FormalObtainRebuildBattlePhase.StageOneBattle;
            if (initialBattle)
            {
                stageOneRealtimeTerminal = terminal;
                phase = C1FormalObtainRebuildBattlePhase.StageOneResult;
            }
            else
            {
                stageTwoRealtimeTerminal = terminal;
                phase = C1FormalObtainRebuildBattlePhase.StageTwoResult;
            }

            if (terminal.win && rewardProgressBridge != null)
            {
                return TryClaimCurrentVictoryReward(out diagnostic);
            }

            diagnostic = terminal.win
                ? StageTwoAcceptedDiagnostic
                : "C1_FORMAL_LOOP_DEFEAT_ACCEPTED";
            return true;
        }

        public bool TryPauseRealtimeBattle(
            out C1FormalRealtimeBattleTickResult tick,
            out string diagnostic)
        {
            tick = null;
            if (!IsBattlePhase() || realtimeBattleAdapter == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            if (!realtimeBattleAdapter.TryPause(out tick) || tick == null
                || !tick.accepted || tick.stateSnapshot == null
                || !tick.stateSnapshot.paused)
            {
                diagnostic = tick == null
                    ? "C1_FORMAL_LOOP_REALTIME_PAUSE_RESULT_MISSING"
                    : tick.errorCode;
                return false;
            }

            lastRealtimeTick = tick;
            realtimeState = tick.stateSnapshot;
            diagnostic = RealtimePauseAcceptedDiagnostic;
            return true;
        }

        public bool TryResumeRealtimeBattle(
            out C1FormalRealtimeBattleTickResult tick,
            out string diagnostic)
        {
            tick = null;
            if (!IsBattlePhase() || realtimeBattleAdapter == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            if (!realtimeBattleAdapter.TryResume(out tick) || tick == null
                || !tick.accepted || tick.stateSnapshot == null
                || tick.stateSnapshot.paused)
            {
                diagnostic = tick == null
                    ? "C1_FORMAL_LOOP_REALTIME_RESUME_RESULT_MISSING"
                    : tick.errorCode;
                return false;
            }

            lastRealtimeTick = tick;
            realtimeState = tick.stateSnapshot;
            diagnostic = RealtimeResumeAcceptedDiagnostic;
            return true;
        }

        public bool TryRefreshRealtimeItemsWhilePaused(
            string refreshCommandId,
            out C1FormalRealtimeBattleItemRefreshResult refreshResult,
            out string diagnostic)
        {
            refreshResult = null;
            diagnostic = string.Empty;
            if (!IsBattlePhase() || realtimeBattleAdapter == null
                || itemAuthority == null || rewardProgressBridge == null
                || !IsRealtimeBattlePaused
                || !rewardProgressBridge.TryCreateCumulativeBattleInput(
                    out C1FormalItemBattleInputSnapshot itemSnapshot,
                    out diagnostic))
            {
                diagnostic = string.IsNullOrEmpty(diagnostic)
                    ? PhaseRejectedDiagnostic
                    : diagnostic;
                return false;
            }

            if (!realtimeBattleAdapter
                    .TryRefreshFromCumulativeItemSnapshotWhilePaused(
                    itemSnapshot,
                    itemAuthority.Current.canonicalSignature,
                    refreshCommandId,
                    out refreshResult)
                || refreshResult == null || !refreshResult.accepted
                || refreshResult.stateSnapshot == null
                || !refreshResult.stateSnapshot.paused)
            {
                realtimeState = refreshResult == null
                    ? realtimeState
                    : refreshResult.stateSnapshot;
                diagnostic = refreshResult == null
                    ? "C1_FORMAL_LOOP_REALTIME_ITEM_REFRESH_RESULT_MISSING"
                    : refreshResult.errorCode;
                return false;
            }

            realtimeState = refreshResult.stateSnapshot;
            diagnostic = RealtimeItemRefreshAcceptedDiagnostic;
            return true;
        }

        public bool TryClaimFirstClearReward(out string diagnostic)
        {
            return TryClaimCurrentVictoryReward(out diagnostic);
        }

        public bool TryClaimCurrentVictoryReward(out string diagnostic)
        {
            if ((phase == C1FormalObtainRebuildBattlePhase.RewardClaimed
                 || phase ==
                    C1FormalObtainRebuildBattlePhase.RebuildForStageTwo)
                && lastRewardProgress != null)
            {
                diagnostic = DuplicateAcceptedDiagnostic;
                return true;
            }

            bool resultPhase = phase ==
                    C1FormalObtainRebuildBattlePhase.StageOneResult
                || phase == C1FormalObtainRebuildBattlePhase.StageTwoResult;
            if (!resultPhase || currentRealtimeTerminal == null
                || !currentRealtimeTerminal.win || rewardProgressBridge == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            if (!rewardProgressBridge.TryAcceptVictory(
                    currentContext,
                    currentRealtimeTerminal,
                    out C1CampaignStageRewardProgressResult accepted,
                    out diagnostic))
            {
                return false;
            }

            lastRewardProgress = accepted;
            firstClearReceipt = new C1FormalFirstClearReceipt(accepted);
            if (!TryResolveFormalSuccessor(
                    CurrentStageId,
                    out pendingNextStageId))
            {
                diagnostic = StageTwoConfigRejectedDiagnostic
                    + " CURRENT_STAGE_ID_INVALID";
                return false;
            }
            returnWorldMapAfterRebuild = string.IsNullOrEmpty(
                pendingNextStageId);
            phase = C1FormalObtainRebuildBattlePhase.RewardClaimed;
            diagnostic = accepted.itemChanged
                ? RewardClaimedDiagnostic
                : DuplicateAcceptedDiagnostic;
            return true;
        }

        public bool TryBeginRewardArrangement(out string diagnostic)
        {
            if (phase == C1FormalObtainRebuildBattlePhase.RebuildForStageTwo
                && lastRewardProgress != null)
            {
                diagnostic = DuplicateAcceptedDiagnostic;
                return true;
            }

            if (phase != C1FormalObtainRebuildBattlePhase.RewardClaimed
                || lastRewardProgress == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            phase = C1FormalObtainRebuildBattlePhase.RebuildForStageTwo;
            diagnostic = RebuildStartedDiagnostic;
            return true;
        }

        public bool TryStartStageTwo(out string diagnostic)
        {
            return TryStartNextStage(out diagnostic);
        }

        public bool TryStartNextStage(out string diagnostic)
        {
            if (phase == C1FormalObtainRebuildBattlePhase.StageTwoBattle)
            {
                diagnostic = DuplicateAcceptedDiagnostic;
                return true;
            }

            if (phase != C1FormalObtainRebuildBattlePhase.RebuildForStageTwo
                || itemAuthority == null || rewardProgressBridge == null
                || lastRewardProgress == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            if (returnWorldMapAfterRebuild
                || string.IsNullOrEmpty(pendingNextStageId))
            {
                diagnostic = ReturnRequiredDiagnostic;
                return false;
            }

            if (!Chapter1CampaignStageCatalog.TryGet(
                    pendingNextStageId,
                    out Chapter1CampaignStageConfig nextConfig,
                    out string configDiagnostic)
                || nextConfig == null
                || !nextConfig.TryValidate(out configDiagnostic)
                || !nextConfig.RouteEnabled || !nextConfig.BattleContentReady
                || currentContext.Generation == long.MaxValue)
            {
                diagnostic = StageTwoConfigRejectedDiagnostic + " " +
                             configDiagnostic;
                return false;
            }

            BattleLaunchContext nextContext = nextConfig.CreateLaunchContext(
                StableId(currentContext, "stage-" + pendingNextStageId + "-launch"),
                StableId(currentContext, "stage-" + pendingNextStageId + "-token"),
                currentContext.Generation + 1L);
            realtimeBattleAdapter.Unbind();
            if (!TryStartRealtime(
                    nextContext,
                    itemAuthority,
                    rewardProgressBridge,
                    realtimeBattleAdapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot start,
                    out diagnostic))
            {
                return false;
            }

            currentContext = nextContext;
            stageTwoContext = nextContext;
            stageTwoRealtimeTerminal = null;
            currentRealtimeTerminal = null;
            currentRealtimeStart = start;
            realtimeState = start.stateSnapshot;
            lastRealtimeTick = null;
            lastRewardProgress = null;
            firstClearReceipt = null;
            pendingNextStageId = string.Empty;
            returnWorldMapAfterRebuild = false;
            phase = C1FormalObtainRebuildBattlePhase.StageTwoBattle;
            diagnostic = StageTwoStartedDiagnostic;
            return true;
        }

        public void Reset()
        {
            if (realtimeBattleAdapter != null)
                realtimeBattleAdapter.Unbind();

            realtimeBattleAdapter = null;
            itemAuthority = null;
            rewardProgressBridge = null;
            initialContext = null;
            currentContext = null;
            stageTwoContext = null;
            firstClearReceipt = null;
            lastRewardProgress = null;
            currentRealtimeStart = null;
            realtimeState = null;
            lastRealtimeTick = null;
            stageOneRealtimeTerminal = null;
            stageTwoRealtimeTerminal = null;
            currentRealtimeTerminal = null;
            pendingNextStageId = string.Empty;
            returnWorldMapAfterRebuild = false;
            phase = C1FormalObtainRebuildBattlePhase.Inactive;
        }

        private bool IsBattlePhase()
        {
            return phase == C1FormalObtainRebuildBattlePhase.StageOneBattle
                || phase == C1FormalObtainRebuildBattlePhase.StageTwoBattle;
        }

        private static bool TryStartRealtime(
            BattleLaunchContext context,
            C1FormalItemSessionAuthority authority,
            C1CampaignStageClearRewardProgressBridge bridge,
            C1FormalRealtimeBattleSessionAdapter adapter,
            out C1FormalRealtimeBattleSessionStartSnapshot start,
            out string diagnostic)
        {
            start = null;
            if (!bridge.TryCreateCumulativeBattleInput(
                    out C1FormalItemBattleInputSnapshot itemInput,
                    out diagnostic)
                || !adapter.TryStartFromCumulativeItemSnapshot(
                    context,
                    itemInput,
                    authority.Current.canonicalSignature,
                    StableId(context, "live-session"),
                    StableId(context, "live-token"),
                    out start)
                || start == null || !start.accepted
                || start.stateSnapshot == null || start.stateSnapshot.terminal
                || start.stateSnapshot.battleTimeMs != 0L)
            {
                diagnostic = start == null
                    ? (string.IsNullOrEmpty(diagnostic)
                        ? "C1_FORMAL_LOOP_REALTIME_SESSION_MISSING"
                        : diagnostic)
                    : start.errorCode;
                return false;
            }

            diagnostic = CreatedDiagnostic;
            return true;
        }

        private static int StageIndex(string stageId)
        {
            if (string.IsNullOrEmpty(stageId) || stageId.Length != 3
                || stageId[0] != '1' || stageId[1] != '-'
                || stageId[2] < '1' || stageId[2] > '5')
                return -1;
            return stageId[2] - '0';
        }

        private static bool TryResolveFormalSuccessor(
            string currentStageId,
            out string nextStageId)
        {
            int stageIndex = StageIndex(currentStageId);
            if (stageIndex < 1 || stageIndex > 5)
            {
                nextStageId = string.Empty;
                return false;
            }

            nextStageId = stageIndex < 5
                ? "1-" + (stageIndex + 1).ToString(
                    CultureInfo.InvariantCulture)
                : string.Empty;
            return true;
        }

        private static string StableId(
            BattleLaunchContext context,
            string suffix)
        {
            return string.Join(".", new[]
            {
                "c1.formal.loop",
                context.LaunchId,
                context.Generation.ToString(CultureInfo.InvariantCulture),
                suffix
            });
        }
    }
}
