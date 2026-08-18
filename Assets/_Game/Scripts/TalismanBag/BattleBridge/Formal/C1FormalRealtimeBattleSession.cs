using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.CampaignLoot;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.BattleBridge.Formal
{
    public sealed class C1FormalRealtimeBattleSession
    {
        private const int AdvanceIterationLimit = 10000;
        private const long CanonicalEnemyStartupPhaseMilliseconds = 400L;

        private static readonly string[] AutomaticPool15StartupTriggerIds =
        {
            "on_layout_evaluate",
            "on_lit"
        };

        private static readonly string[] AutomaticPool15FirstItemTriggerIds =
        {
            "on_direct_lit",
            "on_first_hit"
        };

        private static readonly HashSet<string> AutomaticPool15SupportedTriggerIds =
            new HashSet<string>(new[]
            {
                "on_layout_evaluate",
                "on_lit",
                "on_direct_lit",
                "on_first_hit",
                "on_hit",
                "on_consecutive_trigger",
                "on_damaged",
                "on_kill_flaw"
            }, StringComparer.Ordinal);

        private readonly HashSet<string> usedSessionIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> usedSessionTokens =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> invalidatedSessionTokens =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> acceptedApplicationEventIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<MutableActor> actors = new List<MutableActor>();
        private readonly List<MutableAction> actions = new List<MutableAction>();
        private readonly List<C1FormalRealtimeBattleCue> cueLedger =
            new List<C1FormalRealtimeBattleCue>();
        private readonly List<C1FormalRealtimeBattleItemFactSnapshot>
            activeItemFacts =
                new List<C1FormalRealtimeBattleItemFactSnapshot>();
        private readonly List<
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
            activePool15ItemFacts =
                new List<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>();
        private readonly List<
            C1FormalRealtimeBattleEffectApplicationSnapshot>
            pool15ApplicationLedger =
                new List<C1FormalRealtimeBattleEffectApplicationSnapshot>();
        private readonly List<C1FormalRealtimeBattleEffectCue> pool15CueLedger =
            new List<C1FormalRealtimeBattleEffectCue>();
        private readonly List<
            C1FormalRealtimeBattleActionProgressMutationSnapshot>
            actionProgressMutationLedger =
                new List<
                    C1FormalRealtimeBattleActionProgressMutationSnapshot>();
        private readonly List<C1FormalRealtimeBattleCue>
            actionProgressCueLedger =
                new List<C1FormalRealtimeBattleCue>();
        private readonly List<C1FormalRealtimeBattleNianTransactionSnapshot>
            nianTransactionLedger =
                new List<C1FormalRealtimeBattleNianTransactionSnapshot>();
        private readonly Dictionary<string, long> effectState =
            new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<string, PendingPool15Application>
            pendingPool15Applications =
                new Dictionary<string, PendingPool15Application>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, string> acceptedPool15Events =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> automaticPool15TriggerIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> acceptedRefreshCanonicals =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, C1FormalRealtimeBattleItemRefreshResult>
            acceptedRefreshResults =
                new Dictionary<string, C1FormalRealtimeBattleItemRefreshResult>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, long> canonicalEffectRuntimeState =
            new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly HashSet<string> acceptedCanonicalEffectEvents =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<PlayerBattleStatusState> playerStatuses =
            new List<PlayerBattleStatusState>();
        private readonly List<EnemySkillStatusState> enemySkillStatuses =
            new List<EnemySkillStatusState>();

        private C1FormalRealtimeBattleSessionRequest request;
        private C1FormalRealtimeBattleResolvedEncounter profile;
        private C1FormalRealtimeBattleTerminalResult terminalResult;
        private string phase = C1FormalRealtimeBattleSessionContract.PhaseInactive;
        private string currentTargetActorId = string.Empty;
        private long sessionGeneration;
        private long currentResetGeneration;
        private long lastLaunchGeneration;
        private long battleTimeMs;
        private long cueSequence;
        private int actionSequence;
        private int applicationSequence;
        private int playerHp;
        private long playerGuard;
        private int currentNian;
        private long nianTransactionSequence;
        private C1FormalRealtimeBattleNianCapacitySnapshot nianCapacity;
        private long nextNianGenerationAtBattleTimeMs;
        private long pool15ApplicationSequence;
        private int acceptedItemApplicationCount;
        private int enemyApplicationCount;
        private bool active;
        private bool paused;
        private bool terminal;
        private bool automaticPool15StartupEvaluated;
        private string boundItemBattleInputCanonical = string.Empty;
        private string boundArrangementCanonicalSignature = string.Empty;
        private string boundPool15ItemInputCanonical = string.Empty;
        private string boundPool15PoolCanonicalSignature = string.Empty;
        private string boundPool15SourceItemCatalogCanonicalSignature =
            string.Empty;
        private string boundPool15CandidateProfileId = string.Empty;
        private string boundPool15CandidateDataMaturity = string.Empty;
        private string boundPool15CandidateProfileCanonicalSignature =
            string.Empty;

        public bool IsActive => active;
        public bool IsPaused => paused;
        public bool IsTerminal => terminal;
        public long CurrentResetGeneration => currentResetGeneration;
        public long CurrentSessionGeneration => sessionGeneration;
        public long BattleTimeMs => battleTimeMs;
        public string CurrentTargetActorId => currentTargetActorId;
        public C1FormalRealtimeBattleTerminalResult TerminalResult =>
            terminalResult;
        public IReadOnlyList<C1FormalRealtimeBattleCue> CueLedger =>
            Array.AsReadOnly(cueLedger.ToArray());
        public IReadOnlyList<C1FormalRealtimeBattleEffectApplicationSnapshot>
            Pool15ApplicationLedger => Array.AsReadOnly(
                pool15ApplicationLedger.ToArray());
        public IReadOnlyList<C1FormalRealtimeBattleEffectCue> Pool15CueLedger =>
            Array.AsReadOnly(pool15CueLedger.ToArray());
        public IReadOnlyList<
            C1FormalRealtimeBattleActionProgressMutationSnapshot>
            ActionProgressMutationLedger => Array.AsReadOnly(
                actionProgressMutationLedger.ToArray());
        public IReadOnlyList<C1FormalRealtimeBattleCue>
            ActionProgressCueLedger => Array.AsReadOnly(
                actionProgressCueLedger.ToArray());
        public IReadOnlyList<C1FormalRealtimeBattleNianTransactionSnapshot>
            NianTransactionLedger => Array.AsReadOnly(
                nianTransactionLedger.ToArray());
        public IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot>
            StatusSnapshots => BuildStatusSnapshots();

        public int GetPlayerStatusStackCount(string statusKey)
        {
            PlayerBattleStatusState status = playerStatuses.FirstOrDefault(
                value => value != null
                    && EqualsOrdinal(value.StatusKey, statusKey));
            EnemySkillStatusState enemyStatus = enemySkillStatuses
                .FirstOrDefault(value => value != null
                    && value.TargetActor == null
                    && !value.Removed
                    && EqualsOrdinal(value.StatusKey, statusKey));
            return Math.Max(status?.StackCount ?? 0,
                enemyStatus?.StackCount ?? 0);
        }

        public static bool TryResolveDiscreteProgressDueTime(
            string operatorId,
            string nativeUnitId,
            long currentBattleTimeMs,
            long beforeDueBattleTimeMs,
            long sourceNativeIntervalMs,
            out long afterDueBattleTimeMs)
        {
            afterDueBattleTimeMs = 0L;
            if (!EqualsOrdinal(nativeUnitId, "turn")
                || currentBattleTimeMs < 0L
                || beforeDueBattleTimeMs < currentBattleTimeMs
                || sourceNativeIntervalMs <= 0L)
            {
                return false;
            }

            if (EqualsOrdinal(
                    operatorId,
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .AdvanceItemTriggerProgressStep))
            {
                if (beforeDueBattleTimeMs <= currentBattleTimeMs)
                {
                    return false;
                }

                long reduced = beforeDueBattleTimeMs < sourceNativeIntervalMs
                    ? 0L
                    : beforeDueBattleTimeMs - sourceNativeIntervalMs;
                afterDueBattleTimeMs = Math.Max(currentBattleTimeMs, reduced);
                return afterDueBattleTimeMs <= beforeDueBattleTimeMs;
            }

            if (!EqualsOrdinal(
                    operatorId,
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .DelayEnemyCastProgressStep))
            {
                return false;
            }

            try
            {
                afterDueBattleTimeMs = checked(
                    beforeDueBattleTimeMs + sourceNativeIntervalMs);
                return afterDueBattleTimeMs > beforeDueBattleTimeMs;
            }
            catch (OverflowException)
            {
                afterDueBattleTimeMs = 0L;
                return false;
            }
        }

        public bool TryStart(
            C1FormalRealtimeBattleSessionRequest startRequest,
            out C1FormalRealtimeBattleSessionStartSnapshot startSnapshot)
        {
            if (active)
            {
                startSnapshot = RejectedStart(
                    C1FormalRealtimeBattleErrorCodes.SessionAlreadyActive,
                    "An active live Battle Session must reset before replacement.");
                return false;
            }

            if (!TryValidateRequest(
                    startRequest,
                    out C1FormalRealtimeBattleResolvedEncounter resolvedProfile,
                    out C1FormalRealtimeBattleError error))
            {
                startSnapshot = RejectedStart(error.errorCode, error.message);
                return false;
            }

            request = startRequest;
            profile = resolvedProfile;
            sessionGeneration++;
            lastLaunchGeneration = request.launchGeneration;
            active = true;
            paused = false;
            terminal = false;
            terminalResult = null;
            phase = C1FormalRealtimeBattleSessionContract.PhaseRunning;
            battleTimeMs = 0L;
            cueSequence = 0L;
            actionSequence = 0;
            applicationSequence = 0;
            playerHp = C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp;
            playerGuard = 0L;
            nianCapacity = request.nianCapacityFact;
            currentNian = nianCapacity.initialNian;
            nianTransactionSequence = 0L;
            nextNianGenerationAtBattleTimeMs =
                nianCapacity.generationIntervalMilliseconds;
            pool15ApplicationSequence = 0L;
            acceptedItemApplicationCount = 0;
            enemyApplicationCount = 0;
            currentTargetActorId = string.Empty;
            actors.Clear();
            actions.Clear();
            cueLedger.Clear();
            acceptedApplicationEventIds.Clear();
            activeItemFacts.Clear();
            activeItemFacts.AddRange(request.itemFacts);
            activePool15ItemFacts.Clear();
            activePool15ItemFacts.AddRange(request.pool15ActiveItemFacts
                .OrderBy(fact => fact.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(fact => fact.baseItemId, StringComparer.Ordinal));
            pool15ApplicationLedger.Clear();
            pool15CueLedger.Clear();
            actionProgressMutationLedger.Clear();
            actionProgressCueLedger.Clear();
            nianTransactionLedger.Clear();
            effectState.Clear();
            pendingPool15Applications.Clear();
            acceptedPool15Events.Clear();
            automaticPool15TriggerIds.Clear();
            acceptedRefreshCanonicals.Clear();
            acceptedRefreshResults.Clear();
            canonicalEffectRuntimeState.Clear();
            acceptedCanonicalEffectEvents.Clear();
            playerStatuses.Clear();
            enemySkillStatuses.Clear();
            automaticPool15StartupEvaluated = false;
            boundItemBattleInputCanonical = request.itemBattleInputCanonical;
            boundArrangementCanonicalSignature =
                request.arrangementCanonicalSignature;
            boundPool15ItemInputCanonical = request.pool15ItemInputCanonical;
            boundPool15PoolCanonicalSignature =
                request.pool15PoolCanonicalSignature;
            boundPool15SourceItemCatalogCanonicalSignature =
                request.pool15SourceItemCatalogCanonicalSignature;
            boundPool15CandidateProfileId = request.pool15CandidateProfileId;
            boundPool15CandidateDataMaturity =
                request.pool15CandidateDataMaturity;
            boundPool15CandidateProfileCanonicalSignature =
                request.pool15CandidateProfileCanonicalSignature;

            IReadOnlyList<C1FormalRealtimeBattleResolvedActor> actorRows =
                profile.actors;
            for (int index = 0; index < actorRows.Count; index++)
            {
                actors.Add(new MutableActor(actorRows[index], index));
            }

            List<C1FormalRealtimeBattleCue> initialCues =
                new List<C1FormalRealtimeBattleCue>();
            Emit(
                initialCues,
                C1FormalRealtimeBattleCueKinds.SessionStarted,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.sessionId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                string.Empty,
                100,
                string.Empty);
            foreach (MutableActor actor in actors)
            {
                Emit(
                    initialCues,
                    C1FormalRealtimeBattleCueKinds.ActorSpawnVisible,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    actor.Source.actorBalanceId,
                    actor.Source.actorBalanceId,
                    actor.StableOrder,
                    playerHp,
                    playerHp,
                    actor.CurrentHp,
                    actor.CurrentHp,
                    0,
                    0,
                    string.Empty,
                    90,
                    string.Empty);
            }

            InitializeEnemySkills(initialCues);

            MutableActor firstTarget = FirstLivingActor();
            if (firstTarget != null)
            {
                EmitTargetChanged(initialCues, firstTarget, string.Empty);
            }

            ApplyCanonicalStartupEffects(initialCues);

            for (int index = 0; index < request.itemFacts.Count; index++)
            {
                C1FormalRealtimeBattleItemFactSnapshot item =
                    request.itemFacts[index];
                MutableAction action = ScheduleItem(
                    item,
                    index,
                    item.actionCostFact.firstOffsetMilliseconds);
                EmitScheduled(initialCues, action);
            }

            C1FormalRealtimeBattleResolvedAttackWave wave = profile.attackWave;
            int canonicalEnemyPhaseIndex = 0;
            foreach (MutableActor actor in actors.Where(value =>
                         value.CurrentHp > 0))
            {
                bool hasCanonicalAttack =
                    actor.Source.primaryAttackDamage > 0
                    && actor.Source.primaryAttackFirstDueMilliseconds > 0L
                    && actor.Source.primaryAttackIntervalMilliseconds > 0L;
                MutableAction enemy = hasCanonicalAttack
                    ? ScheduleCanonicalEnemy(
                        actor,
                        checked(
                            actor.Source.primaryAttackFirstDueMilliseconds
                            + canonicalEnemyPhaseIndex
                            * CanonicalEnemyStartupPhaseMilliseconds))
                    : ScheduleEnemy(
                        wave,
                        actor,
                        SecondsToMilliseconds(wave.firstResolveSeconds));
                EmitScheduled(initialCues, enemy);
                if (hasCanonicalAttack)
                {
                    canonicalEnemyPhaseIndex++;
                }
            }

            startSnapshot = new C1FormalRealtimeBattleSessionStartSnapshot(
                true,
                Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                Snapshot(),
                initialCues);
            return true;
        }

        public bool TryPause(out C1FormalRealtimeBattleTickResult tickResult)
        {
            return TryPause(
                request == null ? string.Empty : request.sessionId,
                request == null ? string.Empty : request.sessionToken,
                sessionGeneration,
                currentResetGeneration,
                out tickResult);
        }

        public bool TryPause(
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (!TryValidateActiveEnvelope(
                    sessionId,
                    sessionToken,
                    expectedSessionGeneration,
                    expectedResetGeneration,
                    out C1FormalRealtimeBattleError error))
            {
                tickResult = RejectedTick(error.errorCode, error.message);
                return false;
            }

            if (terminal || paused)
            {
                tickResult = RejectedTick(
                    terminal
                        ? C1FormalRealtimeBattleErrorCodes.TickAfterTerminal
                        : C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    terminal
                        ? "A terminal session cannot pause."
                        : "The live Battle Session is already paused.");
                return false;
            }

            paused = true;
            phase = C1FormalRealtimeBattleSessionContract.PhasePaused;
            List<C1FormalRealtimeBattleCue> emitted =
                new List<C1FormalRealtimeBattleCue>();
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.SessionPaused,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.sessionId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                string.Empty,
                100,
                string.Empty);
            tickResult = AcceptedTick(emitted);
            return true;
        }

        public bool TryResume(out C1FormalRealtimeBattleTickResult tickResult)
        {
            return TryResume(
                request == null ? string.Empty : request.sessionId,
                request == null ? string.Empty : request.sessionToken,
                sessionGeneration,
                currentResetGeneration,
                out tickResult);
        }

        public bool TryResume(
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (!TryValidateActiveEnvelope(
                    sessionId,
                    sessionToken,
                    expectedSessionGeneration,
                    expectedResetGeneration,
                    out C1FormalRealtimeBattleError error))
            {
                tickResult = RejectedTick(error.errorCode, error.message);
                return false;
            }

            if (terminal || !paused)
            {
                tickResult = RejectedTick(
                    terminal
                        ? C1FormalRealtimeBattleErrorCodes.TickAfterTerminal
                        : C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    terminal
                        ? "A terminal session cannot resume."
                        : "The live Battle Session is not paused.");
                return false;
            }

            paused = false;
            phase = C1FormalRealtimeBattleSessionContract.PhaseRunning;
            List<C1FormalRealtimeBattleCue> emitted =
                new List<C1FormalRealtimeBattleCue>();
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.SessionResumed,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.sessionId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                string.Empty,
                100,
                string.Empty);
            tickResult = AcceptedTick(emitted);
            return true;
        }

        public bool TryExecutePool15Event(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            out C1FormalRealtimeBattlePool15EventResult eventResult)
        {
            return TryExecutePool15EventCore(
                eventRequest,
                null,
                out eventResult);
        }

        private bool TryExecutePool15EventCore(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            ISet<string> sourceItemInstanceFilter,
            out C1FormalRealtimeBattlePool15EventResult eventResult)
        {
            if (eventRequest == null)
            {
                eventResult = RejectedPool15Event(
                    null,
                    C1FormalRealtimeBattleErrorCodes.RequestNull,
                    "The formal Pool15 event request is null.");
                return false;
            }

            if (!EqualsOrdinal(
                    eventRequest.schemaId,
                    C1FormalRealtimeBattleSessionContract
                        .Pool15EventRequestSchemaId))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.SchemaMismatch,
                    "The formal Pool15 event schema is unsupported.");
                return false;
            }

            if (!TryValidateActiveEnvelope(
                    eventRequest.sessionId,
                    eventRequest.sessionToken,
                    eventRequest.expectedSessionGeneration,
                    eventRequest.expectedResetGeneration,
                    out C1FormalRealtimeBattleError envelopeError))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    envelopeError.errorCode,
                    envelopeError.message);
                return false;
            }

            if (terminal || paused)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    terminal
                        ? C1FormalRealtimeBattleErrorCodes.TickAfterTerminal
                        : C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    terminal
                        ? "A terminal Session rejects Pool15 events."
                        : "A paused Session rejects unscheduled Pool15 events.");
                return false;
            }

            if (eventRequest.expectedBattleTimeMs != battleTimeMs)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.Pool15EventTimeStale,
                    "The Pool15 event Battle time is stale.");
                return false;
            }

            if (string.IsNullOrEmpty(eventRequest.eventId)
                || string.IsNullOrEmpty(eventRequest.triggerId))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.Pool15EventRejected,
                    "eventId and triggerId are required.");
                return false;
            }

            if (acceptedPool15Events.ContainsKey(eventRequest.eventId))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.Pool15EventDuplicate,
                    "The formal Pool15 event identity was already accepted.");
                return false;
            }

            if (string.IsNullOrEmpty(boundPool15ItemInputCanonical))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                    "No validated Pool15 entitlement snapshot is bound.");
                return false;
            }

            if (activePool15ItemFacts.Count == 0)
            {
                acceptedPool15Events.Add(
                    eventRequest.eventId,
                    eventRequest.canonicalSignature);
                eventResult = new C1FormalRealtimeBattlePool15EventResult(
                    true,
                    Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                    eventRequest.eventId,
                    0,
                    0,
                    Snapshot(),
                    Array.Empty<
                        C1FormalRealtimeBattleEffectApplicationSnapshot>(),
                    Array.Empty<C1FormalRealtimeBattleEffectCue>());
                return true;
            }

            if (activePool15ItemFacts.All(fact =>
                    fact != null && fact.HasImmutableCombatFact))
            {
                return TryExecuteImmutableCombatFactEventCore(
                    eventRequest,
                    sourceItemInstanceFilter,
                    out eventResult);
            }

            C1CampaignLootPool15BattleFoundationResult foundationResult =
                C1CampaignLootPool15BattleEffectFamilyFoundation.TryCreate();
            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            if (foundationResult == null
                || !foundationResult.accepted
                || foundationResult.foundation == null
                || poolResult == null
                || !poolResult.accepted
                || poolResult.snapshot == null
                || !EqualsOrdinal(
                    boundPool15PoolCanonicalSignature,
                    foundationResult.foundation.poolCanonicalSignature)
                || !EqualsOrdinal(
                    boundPool15SourceItemCatalogCanonicalSignature,
                    foundationResult.foundation
                        .sourceItemCatalogCanonicalSignature)
                || !EqualsOrdinal(
                    boundPool15CandidateProfileId,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .ProfileId)
                || !EqualsOrdinal(
                    boundPool15CandidateDataMaturity,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .DataMaturity)
                || candidates.entersFormalFlow
                || !EqualsOrdinal(
                    boundPool15CandidateProfileCanonicalSignature,
                    candidates.canonicalSignature))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes
                        .Pool15FoundationRejected,
                    "Pool15 foundation, profile maturity, or protected signature drifted.");
                return false;
            }

            C1CampaignLootEligibleItemPool15Snapshot pool = poolResult.snapshot;
            List<Pool15SourcePlan> triggerPlans = activePool15ItemFacts
                .Select(fact => CreatePool15SourcePlan(
                    fact,
                    pool.FindProfile(fact.baseItemId),
                    candidates.Find(fact.baseItemId)))
                .Where(plan => plan != null
                    && EqualsOrdinal(
                        plan.Descriptor.triggerId,
                        eventRequest.triggerId)
                    && (sourceItemInstanceFilter == null
                        || sourceItemInstanceFilter.Contains(
                            plan.Fact.itemInstanceId)))
                .OrderBy(
                    plan => plan.Fact.itemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(plan => plan.Fact.baseItemId, StringComparer.Ordinal)
                .ToList();
            if (triggerPlans.Count == 0)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes
                        .Pool15EventTriggerRejected,
                    "No active Pool15 descriptor accepts this trigger.");
                return false;
            }

            List<Pool15SourcePlan> plans = triggerPlans.Where(plan =>
                    eventRequest.satisfiedConditionIds.Contains(
                        plan.Descriptor.conditionId,
                        StringComparer.Ordinal))
                .ToList();
            if (plans.Count != triggerPlans.Count)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes
                        .Pool15EventConditionRejected,
                    "An active Pool15 trigger is missing its immutable condition.");
                return false;
            }

            if (!TryValidateActionProgressRelations(
                    eventRequest,
                    plans,
                    out C1FormalRealtimeBattleError relationError))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    relationError.errorCode,
                    relationError.message);
                return false;
            }

            List<C1CampaignLootPool15BattleSourceRequest> sources =
                new List<C1CampaignLootPool15BattleSourceRequest>();
            foreach (Pool15SourcePlan plan in plans)
            {
                if (!TryResolvePool15Target(
                        plan.Descriptor,
                        out string targetIdentity,
                        out int targetStableOrder,
                        out long? percentageBase,
                        out string targetError))
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15AtomicityRejected,
                        targetError);
                    return false;
                }

                plan.TargetIdentity = targetIdentity;
                plan.TargetStableOrder = targetStableOrder;
                plan.PercentageBaseValueUnits = percentageBase;
                sources.Add(new C1CampaignLootPool15BattleSourceRequest(
                    plan.Fact.itemInstanceId,
                    plan.Fact.sourceFactCanonicalSignature,
                    plan.Fact.baseItemId,
                    plan.Descriptor.targetScopeId,
                    targetIdentity,
                    percentageBase));
            }

            C1CampaignLootPool15BattleExecutionResult foundationExecution =
                foundationResult.foundation.Execute(
                    new C1CampaignLootPool15BattleExecutionRequest(
                        eventRequest.eventId,
                        eventRequest.triggerId,
                        battleTimeMs,
                        eventRequest.satisfiedConditionIds,
                        sources));
            if (foundationExecution == null || !foundationExecution.accepted)
            {
                string diagnostic = foundationExecution == null
                    || foundationExecution.Rejections.Count == 0
                        ? C1FormalRealtimeBattleErrorCodes
                            .Pool15FoundationRejected
                        : foundationExecution.Rejections[0].diagnosticCode;
                eventResult = RejectedPool15Event(
                    eventRequest,
                    diagnostic,
                    "The Pool15 foundation rejected the formal event atomically.");
                return false;
            }

            Dictionary<string, long> preview = new Dictionary<string, long>(
                effectState,
                StringComparer.Ordinal);
            List<PendingPool15Application> pending =
                new List<PendingPool15Application>();
            foreach (C1CampaignLootPool15BattleEffectApplication application in
                     foundationExecution.Applications)
            {
                Pool15SourcePlan plan = plans.Single(value => EqualsOrdinal(
                        value.Fact.itemInstanceId,
                        application.sourceItemInstanceId)
                    && EqualsOrdinal(
                        value.Fact.sourceFactCanonicalSignature,
                        application.sourceFactCanonicalSignature));
                if (!TryMapPool15Effect(
                        application,
                        out string stateKey,
                        out string mappingError))
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .UnsupportedFormalEffectMapping,
                        mappingError);
                    return false;
                }

                string storageKey = EffectStorageKey(
                    stateKey,
                    plan.TargetIdentity,
                    application.nativeUnitId);
                if (!TryCreateActionProgressPlan(
                        eventRequest,
                        application,
                        plan,
                        out ActionProgressPlan actionProgressPlan,
                        out C1FormalRealtimeBattleError progressError))
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        progressError.errorCode,
                        progressError.message);
                    return false;
                }
                long before = preview.TryGetValue(storageKey, out long value)
                    ? value
                    : 0L;
                try
                {
                    preview[storageKey] = checked(
                        before + application.appliedAmount);
                    if (EqualsOrdinal(stateKey, "player.guard"))
                    {
                        checked
                        {
                            long _ = playerGuard + application.appliedAmount;
                        }
                    }
                }
                catch (OverflowException)
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15AtomicityRejected,
                        "The Pool15 state delta overflowed atomically.");
                    return false;
                }

                pending.Add(new PendingPool15Application(
                    application,
                    plan,
                    stateKey,
                    storageKey,
                    actionProgressPlan));
            }

            if (actions.Count + pending.Count >
                C1FormalRealtimeBattleSessionContract.MaxScheduledActionEntries)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.EventBufferExceeded,
                    "The Pool15 event cannot fit the bounded action ledger.");
                return false;
            }

            List<C1FormalRealtimeBattleEffectApplicationSnapshot> resolved =
                new List<C1FormalRealtimeBattleEffectApplicationSnapshot>();
            List<C1FormalRealtimeBattleEffectCue> emitted =
                new List<C1FormalRealtimeBattleEffectCue>();
            foreach (PendingPool15Application row in pending)
            {
                MutableAction action = SchedulePool15Effect(row);
                pendingPool15Applications.Add(action.ActionId, row);
            }

            acceptedPool15Events.Add(
                eventRequest.eventId,
                eventRequest.canonicalSignature);
            foreach (MutableAction action in actions.Where(row =>
                         !row.Executed
                         && row.DueBattleTimeMs == battleTimeMs
                         && EqualsOrdinal(
                             row.ActionKind,
                             C1FormalRealtimeBattleScheduledActionKinds
                                 .Pool15EffectResolve))
                     .OrderBy(row => row.SourceStableOrder)
                     .ThenBy(row => row.StableOrder)
                     .ToArray())
            {
                ProcessPool15Action(
                    action,
                    new List<C1FormalRealtimeBattleCue>(),
                    resolved,
                    emitted);
            }

            eventResult = new C1FormalRealtimeBattlePool15EventResult(
                true,
                Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                eventRequest.eventId,
                pending.Count,
                pending.Count,
                Snapshot(),
                resolved,
                emitted);
            return true;
        }

        private bool TryExecuteImmutableCombatFactEventCore(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            ISet<string> sourceItemInstanceFilter,
            out C1FormalRealtimeBattlePool15EventResult eventResult)
        {
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[] facts =
                activePool15ItemFacts.Where(fact =>
                        EqualsOrdinal(fact.triggerId, eventRequest.triggerId)
                        && (sourceItemInstanceFilter == null
                            || sourceItemInstanceFilter.Contains(
                                fact.itemInstanceId)))
                    .OrderBy(fact => fact.itemInstanceId, StringComparer.Ordinal)
                    .ThenBy(fact => fact.baseItemId, StringComparer.Ordinal)
                    .ToArray();
            if (facts.Length == 0)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes
                        .Pool15EventTriggerRejected,
                    "No active immutable combatFact accepts this trigger.");
                return false;
            }

            if (facts.Any(fact => !eventRequest.satisfiedConditionIds.Contains(
                    fact.activationConditionId,
                    StringComparer.Ordinal)))
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes
                        .Pool15EventConditionRejected,
                    "An immutable combatFact condition was not satisfied.");
                return false;
            }

            List<PendingPool15Application> pending =
                new List<PendingPool15Application>();
            Dictionary<string, long> preview = new Dictionary<string, long>(
                effectState,
                StringComparer.Ordinal);
            for (int sourceOrdinal = 0;
                 sourceOrdinal < facts.Length;
                 sourceOrdinal++)
            {
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact =
                    facts[sourceOrdinal];
                C1FormalRealtimeBattleCombatComponentSnapshot[] components =
                    fact.components.Where(component => component != null)
                        .ToArray();
                bool supportedGuard = components.Length == 1
                    && EqualsOrdinal(components[0].operationId, "AddFlat")
                    && EqualsOrdinal(components[0].targetStatId, "guard")
                    && EqualsOrdinal(components[0].nativeUnitId, "flat")
                    && components[0].signedAmount > 0L;
                if (!supportedGuard)
                {
                    C1FormalRealtimeBattleCombatComponentSnapshot first =
                        components.FirstOrDefault();
                    string shape = first == null
                        ? fact.baseItemId + "/missing-component"
                        : fact.baseItemId + "/" + first.operationId + "/"
                          + first.targetStatId + "/" + first.nativeUnitId;
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .UnsupportedFormalEffectMapping,
                        "No consuming formal Battle operator exists for "
                        + shape + ".");
                    return false;
                }

                C1FormalRealtimeBattleCombatComponentSnapshot component =
                    components[0];
                long amount = Math.Min(
                    component.signedAmount,
                    component.absoluteCapPerAcceptedTrigger);
                Pool15SourcePlan plan = new Pool15SourcePlan(
                    fact,
                    null,
                    null)
                {
                    TargetIdentity = C1FormalRealtimeBattleSessionContract
                        .PlayerActorId,
                    TargetStableOrder = -1,
                    PercentageBaseValueUnits = null
                };
                C1CampaignLootPool15BattleEffectApplication application =
                    new C1CampaignLootPool15BattleEffectApplication(
                        sourceOrdinal + 1L,
                        eventRequest.eventId,
                        fact.itemInstanceId,
                        fact.sourceFactCanonicalSignature,
                        fact.baseItemId,
                        fact.familyId,
                        fact.familyVariantId,
                        fact.triggerId,
                        fact.activationConditionId,
                        component.operationId,
                        component.targetStatId,
                        fact.targetScopeId,
                        plan.TargetIdentity,
                        amount,
                        amount,
                        component.nativeUnitId,
                        battleTimeMs,
                        checked(battleTimeMs + sourceOrdinal
                            * C1CampaignLootPool15BattleEffectFamilyFoundation
                                .SourceStaggerMs),
                        fact.cueIdentity);
                if (!TryMapPool15Effect(
                        application,
                        out string stateKey,
                        out string mappingError))
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .UnsupportedFormalEffectMapping,
                        mappingError);
                    return false;
                }

                string storageKey = EffectStorageKey(
                    stateKey,
                    plan.TargetIdentity,
                    component.nativeUnitId);
                long before = preview.TryGetValue(storageKey, out long current)
                    ? current
                    : 0L;
                try
                {
                    preview[storageKey] = checked(before + amount);
                    if (EqualsOrdinal(stateKey, "player.guard"))
                    {
                        checked
                        {
                            long _ = playerGuard + amount;
                        }
                    }
                }
                catch (OverflowException)
                {
                    eventResult = RejectedPool15Event(
                        eventRequest,
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15AtomicityRejected,
                        "The immutable combatFact state delta overflowed atomically.");
                    return false;
                }

                pending.Add(new PendingPool15Application(
                    application,
                    plan,
                    stateKey,
                    storageKey,
                    null));
            }

            if (actions.Count + pending.Count >
                C1FormalRealtimeBattleSessionContract.MaxScheduledActionEntries)
            {
                eventResult = RejectedPool15Event(
                    eventRequest,
                    C1FormalRealtimeBattleErrorCodes.EventBufferExceeded,
                    "The immutable combatFact event cannot fit the action ledger.");
                return false;
            }

            List<C1FormalRealtimeBattleEffectApplicationSnapshot> resolved =
                new List<C1FormalRealtimeBattleEffectApplicationSnapshot>();
            List<C1FormalRealtimeBattleEffectCue> emitted =
                new List<C1FormalRealtimeBattleEffectCue>();
            foreach (PendingPool15Application row in pending)
            {
                MutableAction action = SchedulePool15Effect(row);
                pendingPool15Applications.Add(action.ActionId, row);
            }

            acceptedPool15Events.Add(
                eventRequest.eventId,
                eventRequest.canonicalSignature);
            foreach (MutableAction action in actions.Where(row =>
                         !row.Executed
                         && row.DueBattleTimeMs == battleTimeMs
                         && EqualsOrdinal(
                             row.ActionKind,
                             C1FormalRealtimeBattleScheduledActionKinds
                                 .Pool15EffectResolve))
                     .OrderBy(row => row.SourceStableOrder)
                     .ThenBy(row => row.StableOrder)
                     .ToArray())
            {
                ProcessPool15Action(
                    action,
                    new List<C1FormalRealtimeBattleCue>(),
                    resolved,
                    emitted);
            }

            eventResult = new C1FormalRealtimeBattlePool15EventResult(
                true,
                Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                eventRequest.eventId,
                pending.Count,
                pending.Count,
                Snapshot(),
                resolved,
                emitted);
            return true;
        }

        public bool TryRefreshItemsWhilePaused(
            C1FormalRealtimeBattleItemRefreshRequest refreshRequest,
            out C1FormalRealtimeBattleItemRefreshResult refreshResult)
        {
            if (refreshRequest == null)
            {
                refreshResult = RejectedRefresh(
                    null,
                    C1FormalRealtimeBattleErrorCodes.RequestNull,
                    "The live Item refresh request is null.");
                return false;
            }

            if (!EqualsOrdinal(
                    refreshRequest.schemaId,
                    C1FormalRealtimeBattleSessionContract
                        .ItemRefreshRequestSchemaId))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    C1FormalRealtimeBattleErrorCodes.SchemaMismatch,
                    "The live Item refresh schema is unsupported.");
                return false;
            }

            if (string.IsNullOrEmpty(refreshRequest.refreshCommandId))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    C1FormalRealtimeBattleErrorCodes.RefreshCommandIdRequired,
                    "refreshCommandId is required.");
                return false;
            }

            if (acceptedRefreshCanonicals.TryGetValue(
                    refreshRequest.refreshCommandId,
                    out string acceptedCanonical))
            {
                if (!EqualsOrdinal(
                        acceptedCanonical,
                        refreshRequest.canonicalSignature))
                {
                    refreshResult = RejectedRefresh(
                        refreshRequest,
                        C1FormalRealtimeBattleErrorCodes.RefreshCommandConflict,
                        "refreshCommandId was already accepted with another canonical.");
                    return false;
                }

                C1FormalRealtimeBattleItemRefreshResult acceptedResult =
                    acceptedRefreshResults[refreshRequest.refreshCommandId];
                C1FormalRealtimeBattleSessionStateSnapshot current = active
                    ? Snapshot()
                    : null;
                if (current == null
                    || acceptedResult.stateSnapshot == null
                    || !EqualsOrdinal(
                        current.canonicalSignature,
                        acceptedResult.stateSnapshot.canonicalSignature))
                {
                    refreshResult = RejectedRefresh(
                        refreshRequest,
                        C1FormalRealtimeBattleErrorCodes.RefreshStateStale,
                        "An accepted refresh duplicate no longer targets the same state.");
                    return false;
                }

                refreshResult = acceptedResult;
                return true;
            }

            if (!TryValidateActiveEnvelope(
                    refreshRequest.sessionId,
                    refreshRequest.sessionToken,
                    refreshRequest.expectedSessionGeneration,
                    refreshRequest.expectedResetGeneration,
                    out C1FormalRealtimeBattleError envelopeError))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    envelopeError.errorCode,
                    envelopeError.message);
                return false;
            }

            if (terminal || !paused)
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    terminal
                        ? C1FormalRealtimeBattleErrorCodes.TickAfterTerminal
                        : C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    terminal
                        ? "A terminal Session cannot refresh live Item facts."
                        : "Live Item facts may refresh only while authoritatively paused.");
                return false;
            }

            C1FormalRealtimeBattleSessionStateSnapshot priorState = Snapshot();
            if (refreshRequest.expectedPausedBattleTimeMs != battleTimeMs
                || !EqualsOrdinal(
                    refreshRequest.expectedPriorStateCanonicalSignature,
                    priorState.canonicalSignature))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    C1FormalRealtimeBattleErrorCodes.RefreshStateStale,
                    "The paused Battle time or prior-state canonical is stale.");
                return false;
            }

            if (!TryValidateRefreshItemFacts(
                    refreshRequest,
                    out C1FormalRealtimeBattleError itemError))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    itemError.errorCode,
                    itemError.message);
                return false;
            }

            if (!TryValidateRefreshPool15Facts(
                    refreshRequest,
                    out C1FormalRealtimeBattleError pool15Error))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    pool15Error.errorCode,
                    pool15Error.message);
                return false;
            }

            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[]
                nextPool15Facts = refreshRequest.replacePool15ActiveFacts
                    ? refreshRequest.activePool15ItemFacts
                        .OrderBy(
                            fact => fact.itemInstanceId,
                            StringComparer.Ordinal)
                        .ThenBy(
                            fact => fact.baseItemId,
                            StringComparer.Ordinal)
                        .ToArray()
                    : activePool15ItemFacts.ToArray();
            Dictionary<string, C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
                nextPool15ByInstance = nextPool15Facts.ToDictionary(
                    fact => fact.itemInstanceId,
                    fact => fact,
                    StringComparer.Ordinal);
            List<MutableAction> canceledFuturePool15Actions =
                new List<MutableAction>();
            if (refreshRequest.replacePool15ActiveFacts)
            {
                foreach (MutableAction action in actions.Where(row =>
                             !row.Executed
                             && EqualsOrdinal(
                                 row.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .Pool15EffectResolve)))
                {
                    if (action.DueBattleTimeMs <= battleTimeMs
                        || !pendingPool15Applications.TryGetValue(
                            action.ActionId,
                            out PendingPool15Application pending))
                    {
                        refreshResult = RejectedRefresh(
                            refreshRequest,
                            C1FormalRealtimeBattleErrorCodes.RefreshStateStale,
                            "A pending Pool15 effect is stale or missing its authoritative payload.");
                        return false;
                    }

                    if (!nextPool15ByInstance.TryGetValue(
                            pending.Application.sourceItemInstanceId,
                            out C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
                                nextFact)
                        || !EqualsOrdinal(
                            nextFact.sourceFactCanonicalSignature,
                            pending.Application.sourceFactCanonicalSignature))
                    {
                        canceledFuturePool15Actions.Add(action);
                    }
                }
            }

            List<MutableAction> futureItemActions = actions.Where(action =>
                    !action.Executed
                    && (EqualsOrdinal(
                            action.ActionKind,
                            C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger)
                        || EqualsOrdinal(
                            action.ActionKind,
                            C1FormalRealtimeBattleScheduledActionKinds
                                .ItemUnlitDiagnostic)))
                .ToList();
            if (futureItemActions
                    .Where(action => EqualsOrdinal(
                        action.ActionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger))
                    .GroupBy(action => action.SourceId, StringComparer.Ordinal)
                    .Any(group => group.Count() > 1))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    C1FormalRealtimeBattleErrorCodes.RefreshInputRejected,
                    "The live Session contains duplicate future Item actions.");
                return false;
            }

            Dictionary<string, C1FormalRealtimeBattleItemFactSnapshot> priorFacts =
                activeItemFacts.ToDictionary(
                    fact => fact.sourceId,
                    fact => fact,
                    StringComparer.Ordinal);
            Dictionary<string, C1FormalRealtimeBattleItemFactSnapshot> nextFacts =
                refreshRequest.activeItemFacts.ToDictionary(
                    fact => fact.sourceId,
                    fact => fact,
                    StringComparer.Ordinal);
            List<MutableAction> retained = new List<MutableAction>();
            List<MutableAction> canceled = new List<MutableAction>();
            foreach (MutableAction action in futureItemActions)
            {
                if (EqualsOrdinal(
                        action.ActionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger)
                    && priorFacts.TryGetValue(
                        action.SourceId,
                        out C1FormalRealtimeBattleItemFactSnapshot priorFact)
                    && nextFacts.TryGetValue(
                        action.SourceId,
                        out C1FormalRealtimeBattleItemFactSnapshot nextFact)
                    && EqualsOrdinal(
                        priorFact.canonicalSignature,
                        nextFact.canonicalSignature))
                {
                    retained.Add(action);
                }
                else
                {
                    canceled.Add(action);
                }
            }

            HashSet<string> retainedSources = new HashSet<string>(
                retained.Select(action => action.SourceId),
                StringComparer.Ordinal);
            C1FormalRealtimeBattleItemFactSnapshot[] factsToSchedule =
                refreshRequest.activeItemFacts
                    .Where(fact => !retainedSources.Contains(fact.sourceId))
                    .ToArray();
            if (actions.Count + factsToSchedule.Length >
                C1FormalRealtimeBattleSessionContract.MaxScheduledActionEntries
                || factsToSchedule.Any(fact =>
                    fact.cooldownMs > long.MaxValue - battleTimeMs))
            {
                refreshResult = RejectedRefresh(
                    refreshRequest,
                    C1FormalRealtimeBattleErrorCodes.RefreshActionCapacityExceeded,
                    "The refreshed Item schedule cannot fit the bounded future-action ledger.");
                return false;
            }

            foreach (MutableAction action in canceled)
            {
                action.MarkExecuted(
                    false,
                    "ITEM_REFRESH_CANCELED_FUTURE_ACTION",
                    string.Empty);
            }

            foreach (MutableAction action in canceledFuturePool15Actions)
            {
                action.MarkExecuted(
                    false,
                    "POOL15_REFRESH_CANCELED_FUTURE_EFFECT",
                    string.Empty);
                pendingPool15Applications.Remove(action.ActionId);
            }

            activeItemFacts.Clear();
            activeItemFacts.AddRange(refreshRequest.activeItemFacts);
            nianCapacity = refreshRequest.nianCapacityFact;
            boundItemBattleInputCanonical =
                refreshRequest.itemBattleInputCanonical;
            boundArrangementCanonicalSignature =
                refreshRequest.nianCapacityFact.arrangementCanonicalSignature;
            if (refreshRequest.replacePool15ActiveFacts)
            {
                activePool15ItemFacts.Clear();
                activePool15ItemFacts.AddRange(nextPool15Facts);
                boundPool15ItemInputCanonical =
                    refreshRequest.pool15ItemInputCanonical;
                boundPool15PoolCanonicalSignature =
                    refreshRequest.pool15PoolCanonicalSignature;
                boundPool15SourceItemCatalogCanonicalSignature =
                    refreshRequest.pool15SourceItemCatalogCanonicalSignature;
                boundPool15CandidateProfileId =
                    refreshRequest.pool15CandidateProfileId;
                boundPool15CandidateDataMaturity =
                    refreshRequest.pool15CandidateDataMaturity;
                boundPool15CandidateProfileCanonicalSignature =
                    refreshRequest.pool15CandidateProfileCanonicalSignature;
                C1CampaignLootEligibleItemPool15Snapshot refreshedPool =
                    activePool15ItemFacts.Count == 0
                        ? null
                        : C1CampaignLootEligibleItemPool15Carrier.Resolve()
                            ?.snapshot;
                HashSet<string> retainedAutomaticSourceTriggers =
                    new HashSet<string>(activePool15ItemFacts.Select(fact =>
                        AutomaticPool15SourceTriggerKey(
                            fact.HasImmutableCombatFact
                                ? fact.triggerId
                                : refreshedPool?.FindProfile(fact.baseItemId)
                                    ?.triggerId,
                            fact)), StringComparer.Ordinal);
                automaticPool15TriggerIds.IntersectWith(
                    retainedAutomaticSourceTriggers);
                automaticPool15StartupEvaluated = false;
            }
            List<C1FormalRealtimeBattleCue> emitted =
                new List<C1FormalRealtimeBattleCue>();
            for (int index = 0; index < activeItemFacts.Count; index++)
            {
                C1FormalRealtimeBattleItemFactSnapshot fact = activeItemFacts[index];
                if (retainedSources.Contains(fact.sourceId))
                {
                    continue;
                }

                MutableAction scheduled = ScheduleItem(
                    fact,
                    index,
                    checked(battleTimeMs
                        + fact.actionCostFact.firstOffsetMilliseconds));
                EmitScheduled(emitted, scheduled);
            }

            refreshResult = new C1FormalRealtimeBattleItemRefreshResult(
                true,
                Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                request.sessionId,
                sessionGeneration,
                currentResetGeneration,
                battleTimeMs,
                refreshRequest.itemBattleInputCanonical,
                refreshRequest.refreshCommandId,
                canceled.Count,
                retained.Count,
                factsToSchedule.Length,
                activeItemFacts.Count,
                Snapshot(),
                emitted);
            acceptedRefreshCanonicals.Add(
                refreshRequest.refreshCommandId,
                refreshRequest.canonicalSignature);
            acceptedRefreshResults.Add(
                refreshRequest.refreshCommandId,
                refreshResult);
            return true;
        }

        public bool TryAdvanceBy(
            long deltaMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            return TryAdvanceBy(
                request == null ? string.Empty : request.sessionId,
                request == null ? string.Empty : request.sessionToken,
                sessionGeneration,
                currentResetGeneration,
                deltaMs,
                out tickResult);
        }

        public bool TryAdvanceBy(
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            long deltaMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (!TryValidateActiveEnvelope(
                    sessionId,
                    sessionToken,
                    expectedSessionGeneration,
                    expectedResetGeneration,
                    out C1FormalRealtimeBattleError error))
            {
                tickResult = RejectedTick(error.errorCode, error.message);
                return false;
            }

            if (deltaMs <= 0L || deltaMs > long.MaxValue - battleTimeMs)
            {
                tickResult = RejectedTick(
                    C1FormalRealtimeBattleErrorCodes.DeltaInvalid,
                    "deltaMs must be positive and must not overflow Battle time.");
                return false;
            }

            return TryAdvanceToCore(battleTimeMs + deltaMs, out tickResult);
        }

        public bool TryAdvanceTo(
            long targetBattleTimeMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (!TryValidateActiveEnvelope(
                    request == null ? string.Empty : request.sessionId,
                    request == null ? string.Empty : request.sessionToken,
                    sessionGeneration,
                    currentResetGeneration,
                    out C1FormalRealtimeBattleError error))
            {
                tickResult = RejectedTick(error.errorCode, error.message);
                return false;
            }

            return TryAdvanceToCore(targetBattleTimeMs, out tickResult);
        }

        public bool TryReset(
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (!TryValidateActiveEnvelope(
                    sessionId,
                    sessionToken,
                    expectedSessionGeneration,
                    expectedResetGeneration,
                    out C1FormalRealtimeBattleError error))
            {
                tickResult = RejectedTick(error.errorCode, error.message);
                return false;
            }

            List<C1FormalRealtimeBattleCue> emitted =
                new List<C1FormalRealtimeBattleCue>();
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.SessionReset,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.sessionId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                string.Empty,
                100,
                string.Empty);
            string invalidatedToken = request.sessionToken;
            usedSessionIds.Add(request.sessionId);
            usedSessionTokens.Add(request.sessionToken);
            invalidatedSessionTokens.Add(invalidatedToken);
            currentResetGeneration++;
            active = false;
            paused = false;
            terminal = false;
            terminalResult = null;
            phase = C1FormalRealtimeBattleSessionContract.PhaseReset;
            battleTimeMs = 0L;
            playerHp = C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp;
            playerGuard = 0L;
            currentNian = 0;
            nianTransactionSequence = 0L;
            nianCapacity = null;
            nextNianGenerationAtBattleTimeMs = 0L;
            pool15ApplicationSequence = 0L;
            acceptedItemApplicationCount = 0;
            enemyApplicationCount = 0;
            currentTargetActorId = string.Empty;
            actors.Clear();
            actions.Clear();
            cueLedger.Clear();
            acceptedApplicationEventIds.Clear();
            activeItemFacts.Clear();
            activePool15ItemFacts.Clear();
            pool15ApplicationLedger.Clear();
            pool15CueLedger.Clear();
            actionProgressMutationLedger.Clear();
            actionProgressCueLedger.Clear();
            nianTransactionLedger.Clear();
            effectState.Clear();
            pendingPool15Applications.Clear();
            acceptedPool15Events.Clear();
            automaticPool15TriggerIds.Clear();
            acceptedRefreshCanonicals.Clear();
            acceptedRefreshResults.Clear();
            canonicalEffectRuntimeState.Clear();
            acceptedCanonicalEffectEvents.Clear();
            playerStatuses.Clear();
            enemySkillStatuses.Clear();
            automaticPool15StartupEvaluated = false;
            boundItemBattleInputCanonical = string.Empty;
            boundArrangementCanonicalSignature = string.Empty;
            boundPool15ItemInputCanonical = string.Empty;
            boundPool15PoolCanonicalSignature = string.Empty;
            boundPool15SourceItemCatalogCanonicalSignature = string.Empty;
            boundPool15CandidateProfileId = string.Empty;
            boundPool15CandidateDataMaturity = string.Empty;
            boundPool15CandidateProfileCanonicalSignature = string.Empty;
            request = null;
            profile = null;
            C1FormalRealtimeBattleSessionStateSnapshot resetSnapshot =
                new C1FormalRealtimeBattleSessionStateSnapshot(
                    true,
                    C1FormalRealtimeBattleErrorCodes.None,
                    string.Empty,
                    sessionGeneration,
                    currentResetGeneration,
                    phase,
                    false,
                    false,
                    0L,
                    new C1FormalRealtimeBattlePlayerSnapshot(
                        C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp,
                        C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp),
                    Array.Empty<C1FormalRealtimeBattleActorSnapshot>(),
                    Array.Empty<C1FormalRealtimeBattleScheduledActionSnapshot>(),
                    0,
                    0,
                    cueSequence);
            tickResult = new C1FormalRealtimeBattleTickResult(
                true,
                Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty),
                resetSnapshot,
                emitted,
                null);
            return true;
        }

        public bool TryGetSnapshot(
            out C1FormalRealtimeBattleSessionStateSnapshot snapshot)
        {
            if (!active)
            {
                snapshot = new C1FormalRealtimeBattleSessionStateSnapshot(
                    true,
                    C1FormalRealtimeBattleErrorCodes.None,
                    string.Empty,
                    sessionGeneration,
                    currentResetGeneration,
                    phase,
                    false,
                    false,
                    0L,
                    new C1FormalRealtimeBattlePlayerSnapshot(
                        C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp,
                        C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp),
                    Array.Empty<C1FormalRealtimeBattleActorSnapshot>(),
                    Array.Empty<C1FormalRealtimeBattleScheduledActionSnapshot>(),
                    0,
                    0,
                    cueSequence);
                return false;
            }

            snapshot = Snapshot();
            return true;
        }

        private bool TryAdvanceToCore(
            long targetBattleTimeMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            if (terminal)
            {
                tickResult = RejectedTick(
                    C1FormalRealtimeBattleErrorCodes.TickAfterTerminal,
                    "A terminal live Battle Session rejects further ticks.");
                return false;
            }

            if (paused)
            {
                tickResult = RejectedTick(
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    "A paused live Battle Session does not advance hidden time.");
                return false;
            }

            if (targetBattleTimeMs <= battleTimeMs)
            {
                tickResult = RejectedTick(
                    C1FormalRealtimeBattleErrorCodes.StaleTick,
                    "targetBattleTimeMs must be strictly monotonic.");
                return false;
            }

            List<C1FormalRealtimeBattleCue> emitted =
                new List<C1FormalRealtimeBattleCue>();
            if (!automaticPool15StartupEvaluated)
            {
                automaticPool15StartupEvaluated = true;
                ProduceAutomaticPool15Triggers(
                    AutomaticPool15StartupTriggerIds,
                    emitted);
                EmitUnsupportedAutomaticPool15Diagnostics(emitted);
            }
            int iterations = 0;
            while (!terminal)
            {
                MutableAction next = actions
                    .Where(action => !action.Executed)
                    .OrderBy(action => action.DueBattleTimeMs)
                    .ThenBy(action => ActionPriority(action.ActionKind))
                    .ThenBy(action => action.SourceStableOrder)
                    .ThenBy(action => action.StableOrder)
                    .FirstOrDefault();
                bool generationDue = nianCapacity != null
                    && nextNianGenerationAtBattleTimeMs > battleTimeMs
                    && nextNianGenerationAtBattleTimeMs
                        <= targetBattleTimeMs
                    && (next == null
                        || nextNianGenerationAtBattleTimeMs
                            <= next.DueBattleTimeMs);
                if (!generationDue
                    && (next == null
                        || next.DueBattleTimeMs > targetBattleTimeMs))
                {
                    break;
                }

                iterations++;
                if (iterations > AdvanceIterationLimit)
                {
                    phase = C1FormalRealtimeBattleSessionContract.PhaseFaulted;
                    terminal = true;
                    tickResult = RejectedTick(
                        C1FormalRealtimeBattleErrorCodes
                            .SimulationGuardExceeded,
                        "The live Battle action loop exceeded its deterministic guard.");
                    return false;
                }

                if (generationDue)
                {
                    battleTimeMs = nextNianGenerationAtBattleTimeMs;
                    ApplyI031NianGeneration(emitted);
                    nextNianGenerationAtBattleTimeMs = checked(
                        nextNianGenerationAtBattleTimeMs
                        + nianCapacity.generationIntervalMilliseconds);
                    continue;
                }

                battleTimeMs = next.DueBattleTimeMs;
                List<MutableAction> due = actions
                    .Where(action => !action.Executed
                                     && action.DueBattleTimeMs == battleTimeMs)
                    .OrderBy(action => ActionPriority(action.ActionKind))
                    .ThenBy(action => action.SourceStableOrder)
                    .ThenBy(action => action.StableOrder)
                    .ToList();
                foreach (MutableAction action in due)
                {
                    if (terminal)
                    {
                        break;
                    }

                    if (action.Executed
                        || action.DueBattleTimeMs != battleTimeMs)
                    {
                        continue;
                    }

                    if (string.Equals(
                            action.ActionKind,
                            C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                            StringComparison.Ordinal))
                    {
                        ProcessItemAction(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .Pool15EffectResolve,
                                 StringComparison.Ordinal))
                    {
                        ProcessPool15Action(action, emitted, null, null);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemyBasicWave,
                                 StringComparison.Ordinal))
                    {
                        ProcessEnemyAction(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemySkill,
                                 StringComparison.Ordinal))
                    {
                        ProcessEnemySkillAction(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemySkillStatusTick,
                                 StringComparison.Ordinal))
                    {
                        ProcessEnemySkillStatusTick(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemySkillStatusExpire,
                                 StringComparison.Ordinal))
                    {
                        ProcessEnemySkillStatusExpire(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .StatusTick,
                                 StringComparison.Ordinal))
                    {
                        ProcessStatusTick(action, emitted);
                    }
                    else if (string.Equals(
                                 action.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .StatusExpire,
                                 StringComparison.Ordinal))
                    {
                        ProcessStatusExpire(action, emitted);
                    }
                    else
                    {
                        action.MarkExecuted(
                            false,
                            C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                            string.Empty);
                    }
                }
            }

            if (!terminal)
            {
                battleTimeMs = targetBattleTimeMs;
            }

            tickResult = AcceptedTick(emitted);
            return terminalResult == null || terminalResult.accepted;
        }

        private void ProduceAutomaticPool15Triggers(
            IEnumerable<string> triggerIds,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (activePool15ItemFacts.Count == 0
                || triggerIds == null
                || request == null
                || terminal
                || paused)
            {
                return;
            }

            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootEligibleItemPool15Snapshot pool =
                poolResult == null || !poolResult.accepted
                    ? null
                    : poolResult.snapshot;
            foreach (string triggerId in triggerIds
                         .Where(value => !string.IsNullOrWhiteSpace(value))
                         .Distinct(StringComparer.Ordinal)
                         .OrderBy(value => value, StringComparer.Ordinal))
            {
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[] matching =
                    activePool15ItemFacts.Where(fact =>
                        pool != null
                        && EqualsOrdinal(
                            fact.HasImmutableCombatFact
                                ? fact.triggerId
                                : pool.FindProfile(fact.baseItemId)?.triggerId,
                            triggerId)
                        && !IsFormalNianRefundFact(fact)
                        && !IsFormalShellBreakModifierFact(fact)
                        && !IsFormalExtraTargetFact(fact)
                        && !IsFormalEnemyCastDelayFact(fact)
                        && !automaticPool15TriggerIds.Contains(
                            AutomaticPool15SourceTriggerKey(
                                triggerId,
                                fact)))
                    .OrderBy(fact => fact.itemInstanceId, StringComparer.Ordinal)
                    .ThenBy(fact => fact.baseItemId, StringComparer.Ordinal)
                    .ToArray();
                if (matching.Length == 0)
                {
                    continue;
                }

                string[] conditions = matching.Select(fact =>
                        fact.HasImmutableCombatFact
                            ? fact.activationConditionId
                            : pool.FindProfile(fact.baseItemId)?.conditionId
                        ?? string.Empty)
                    .Where(value => value.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                string eventId = request.sessionId + ":auto:pool15:"
                    + sessionGeneration.ToString(
                        CultureInfo.InvariantCulture) + ":" + triggerId + ":"
                    + boundItemBattleInputCanonical;
                C1FormalRealtimeBattlePool15EventRequest automaticRequest =
                    new C1FormalRealtimeBattlePool15EventRequest(
                        C1FormalRealtimeBattleSessionContract
                            .Pool15EventRequestSchemaId,
                        request.sessionId,
                        request.sessionToken,
                        sessionGeneration,
                        currentResetGeneration,
                        eventId,
                        battleTimeMs,
                        triggerId,
                        conditions,
                        Array.Empty<
                            C1FormalRealtimeBattleActionProgressRelationSnapshot>());
                int cueCountBefore = cueLedger.Count;
                foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact
                         in matching)
                {
                    automaticPool15TriggerIds.Add(
                        AutomaticPool15SourceTriggerKey(triggerId, fact));
                }
                if (!TryExecutePool15EventCore(
                        automaticRequest,
                        new HashSet<string>(matching.Select(fact =>
                            fact.itemInstanceId), StringComparer.Ordinal),
                        out C1FormalRealtimeBattlePool15EventResult result))
                {
                    foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
                             fact in matching)
                    {
                        EmitPool15NotExecuted(
                            emitted,
                            fact,
                            triggerId,
                            pool.FindProfile(fact.baseItemId)?.conditionId,
                            result == null
                                ? C1FormalRealtimeBattleErrorCodes
                                    .Pool15EventRejected
                                : result.errorCode);
                    }
                    continue;
                }

                foreach (C1FormalRealtimeBattleCue cue in cueLedger
                             .Skip(cueCountBefore)
                             .ToArray())
                {
                    emitted.Add(cue);
                }
            }
        }

        private static string AutomaticPool15SourceTriggerKey(
            string triggerId,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact)
        {
            return string.Join("|", new[]
            {
                triggerId ?? string.Empty,
                fact == null ? string.Empty : fact.itemInstanceId,
                fact == null
                    ? string.Empty
                    : fact.sourceFactCanonicalSignature
            });
        }

        private void EmitUnsupportedAutomaticPool15Diagnostics(
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (activePool15ItemFacts.Count == 0)
            {
                return;
            }

            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootEligibleItemPool15Snapshot pool =
                poolResult == null || !poolResult.accepted
                    ? null
                    : poolResult.snapshot;
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     activePool15ItemFacts)
            {
                C1CampaignLootItemProfileSnapshot descriptor =
                    pool?.FindProfile(fact.baseItemId);
                string triggerId = fact.HasImmutableCombatFact
                    ? fact.triggerId
                    : descriptor?.triggerId;
                string conditionId = fact.HasImmutableCombatFact
                    ? fact.activationConditionId
                    : descriptor?.conditionId;
                if (!string.IsNullOrEmpty(triggerId)
                    && AutomaticPool15SupportedTriggerIds.Contains(
                        triggerId))
                {
                    continue;
                }

                EmitPool15NotExecuted(
                    emitted,
                    fact,
                    triggerId,
                    conditionId,
                    string.IsNullOrEmpty(triggerId)
                        ? C1FormalRealtimeBattleErrorCodes
                            .Pool15SnapshotRejected
                        : C1FormalRealtimeBattleErrorCodes
                            .Pool15EventTriggerRejected);
            }
        }

        private void EmitPool15NotExecuted(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
            string triggerId,
            string conditionId,
            string diagnosticCode)
        {
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.Pool15EffectNotExecuted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                fact == null ? string.Empty : fact.itemInstanceId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                (fact == null ? string.Empty : fact.baseItemId) + "|"
                + (triggerId ?? string.Empty) + "|"
                + (conditionId ?? string.Empty) + "|"
                + (diagnosticCode ?? string.Empty),
                30,
                string.Empty,
                fact == null ? string.Empty : fact.itemInstanceId,
                fact == null ? string.Empty : fact.baseItemId,
                string.Empty,
                string.Empty);
        }

        private void ProcessPool15Action(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emittedLegacy,
            ICollection<C1FormalRealtimeBattleEffectApplicationSnapshot>
                resolvedApplications,
            ICollection<C1FormalRealtimeBattleEffectCue> emittedEffects)
        {
            if (!pendingPool15Applications.TryGetValue(
                    action.ActionId,
                    out PendingPool15Application pending))
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.Pool15EventRejected,
                    string.Empty);
                return;
            }

            string targetIdentity = pending.Plan.TargetIdentity;
            int targetStableOrder = pending.Plan.TargetStableOrder;
            if (targetStableOrder >= 0)
            {
                MutableActor target = actors.FirstOrDefault(actor =>
                    EqualsOrdinal(
                        actor.Source.actorBalanceId,
                        targetIdentity)
                    && actor.CurrentHp > 0);
                if (target == null)
                {
                    target = FirstLivingActor();
                }

                if (target == null)
                {
                    action.MarkExecuted(
                        false,
                        "POOL15_TARGET_DEAD_NO_RETARGET",
                        string.Empty);
                    pendingPool15Applications.Remove(action.ActionId);
                    return;
                }

                targetIdentity = target.Source.actorBalanceId;
                targetStableOrder = target.StableOrder;
            }

            string storageKey = EffectStorageKey(
                pending.StateKey,
                targetIdentity,
                pending.Application.nativeUnitId);
            MutableAction progressTarget = null;
            long progressAfterDueBattleTimeMs = 0L;
            if (pending.ProgressPlan != null
                && !TryPreflightActionProgressCommit(
                    pending.ProgressPlan,
                    out progressTarget,
                    out progressAfterDueBattleTimeMs,
                    out C1FormalRealtimeBattleError progressError))
            {
                action.MarkExecuted(
                    false,
                    progressError.errorCode,
                    string.Empty);
                pendingPool15Applications.Remove(action.ActionId);
                return;
            }

            long before = EqualsOrdinal(pending.StateKey, "player.guard")
                ? playerGuard
                : effectState.TryGetValue(storageKey, out long current)
                    ? current
                    : 0L;
            long after;
            try
            {
                after = checked(before + pending.Application.appliedAmount);
            }
            catch (OverflowException)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.Pool15AtomicityRejected,
                    string.Empty);
                pendingPool15Applications.Remove(action.ActionId);
                return;
            }

            if (pool15ApplicationSequence == long.MaxValue)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressAtomicityRejected,
                    string.Empty);
                pendingPool15Applications.Remove(action.ActionId);
                return;
            }

            long nextApplicationSequence = pool15ApplicationSequence + 1L;
            C1FormalRealtimeBattleEffectApplicationSnapshot resolved =
                new C1FormalRealtimeBattleEffectApplicationSnapshot(
                    nextApplicationSequence,
                    pending.Application.eventId,
                    pending.Plan.Fact.itemInstanceId,
                    pending.Application.sourceBaseItemId,
                    pending.Plan.Fact.sourceFactCanonicalSignature,
                    pending.Application.familyId,
                    pending.Application.familyVariantId,
                    pending.Application.triggerId,
                    pending.Application.conditionId,
                    pending.Application.operationId,
                    pending.Application.targetStatId,
                    pending.Application.targetScopeId,
                    targetIdentity,
                    targetStableOrder,
                    pending.Application.requestedAmount,
                    pending.Application.appliedAmount,
                    pending.Application.nativeUnitId,
                    pending.Plan.PercentageBaseValueUnits,
                    before,
                    after,
                    request.pool15CandidateProfileId,
                    request.pool15CandidateDataMaturity,
                    request.pool15CandidateProfileCanonicalSignature,
                    pending.Application.triggerBattleTimeMs,
                    pending.Application.resolveBattleTimeMs,
                    pending.Application.cueIdentity,
                    pending.Application.canonicalSignature);

            C1FormalRealtimeBattleActionProgressMutationSnapshot mutation = null;
            if (pending.ProgressPlan != null)
            {
                mutation = new
                    C1FormalRealtimeBattleActionProgressMutationSnapshot(
                        pending.ProgressPlan.OperatorId,
                        resolved.eventId,
                        pending.ProgressPlan.SourceItemInstanceId,
                        progressTarget.ActionId,
                        progressTarget.ActionKind,
                        progressTarget.SourceId,
                        "turn",
                        1,
                        1,
                        pending.ProgressPlan.ExpectedBeforeDueBattleTimeMs,
                        progressAfterDueBattleTimeMs,
                        pending.ProgressPlan.SourceNativeIntervalMs,
                        pending.ProgressPlan.RelationCanonicalSignature);
                if (!progressTarget.TryReschedule(
                        pending.ProgressPlan.ExpectedBeforeDueBattleTimeMs,
                        progressAfterDueBattleTimeMs))
                {
                    action.MarkExecuted(
                        false,
                        C1FormalRealtimeBattleErrorCodes.ActionProgressStale,
                        string.Empty);
                    pendingPool15Applications.Remove(action.ActionId);
                    return;
                }
            }

            effectState[storageKey] = after;
            if (EqualsOrdinal(pending.StateKey, "player.guard"))
            {
                playerGuard = after;
            }

            pool15ApplicationSequence = nextApplicationSequence;
            pool15ApplicationLedger.Add(resolved);
            if (mutation != null)
            {
                actionProgressMutationLedger.Add(mutation);
            }
            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                resolved.eventId + ":" + resolved.sourceItemInstanceId);
            if (mutation != null)
            {
                Emit(
                    emittedLegacy,
                    C1FormalRealtimeBattleCueKinds.ActionProgressMutated,
                    C1FormalRealtimeBattleSessionContract.BattleOwner,
                    mutation.sourceItemInstanceId,
                    mutation.targetActionId,
                    progressTarget.StableOrder,
                    playerHp,
                    playerHp,
                    0,
                    0,
                    mutation.requestedStepCount,
                    mutation.appliedStepCount,
                    mutation.operatorId + "|" + mutation.targetActionKind
                    + "|" + mutation.beforeDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + mutation.afterDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture) + "|interval="
                    + mutation.sourceNativeIntervalMs.ToString(
                        CultureInfo.InvariantCulture) + "|"
                    + mutation.nativeUnitId + "|"
                    + mutation.canonicalSignature,
                    82,
                    action.AcceptedApplicationEventId,
                    resolved.sourceItemInstanceId,
                    resolved.sourceBaseItemId,
                    resolved.familyId,
                    resolved.familyVariantId);
                actionProgressCueLedger.Add(
                    cueLedger[cueLedger.Count - 1]);
            }
            Emit(
                emittedLegacy,
                C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                resolved.sourceItemInstanceId,
                resolved.targetIdentity,
                resolved.targetStableOrder,
                playerHp,
                playerHp,
                0,
                0,
                ToCueAmount(resolved.requestedAmount),
                ToCueAmount(resolved.appliedAmount),
                resolved.sourceBaseItemId + "|" + resolved.familyId + "|"
                + resolved.nativeUnitId + "|"
                + resolved.targetBefore.ToString(CultureInfo.InvariantCulture)
                + ">" + resolved.targetAfter.ToString(
                    CultureInfo.InvariantCulture),
                80,
                action.AcceptedApplicationEventId,
                resolved.sourceItemInstanceId,
                resolved.sourceBaseItemId,
                resolved.familyId,
                resolved.familyVariantId);
            C1FormalRealtimeBattleEffectCue effectCue =
                new C1FormalRealtimeBattleEffectCue(cueSequence, resolved);
            pool15CueLedger.Add(effectCue);
            resolvedApplications?.Add(resolved);
            emittedEffects?.Add(effectCue);
            pendingPool15Applications.Remove(action.ActionId);
        }

        private void ProcessItemAction(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            C1FormalRealtimeBattleItemFactSnapshot item = activeItemFacts
                .FirstOrDefault(fact => EqualsOrdinal(
                    fact.sourceId,
                    action.SourceId));
            if (item == null)
            {
                action.MarkExecuted(
                    false,
                    "ITEM_REFRESH_STALE_ACTION_REJECTED",
                    string.Empty);
                return;
            }
            MutableActor firstTarget = FirstLivingActor();
            if (firstTarget == null)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    string.Empty);
                return;
            }
            CanonicalEffectSource targetingSource;
            firstTarget = ResolveCanonicalActionTarget(
                item,
                firstTarget,
                out targetingSource);

            string applicationId = NewApplicationId();
            if (!acceptedApplicationEventIds.Add(applicationId))
            {
                CompleteFault(
                    C1FormalRealtimeBattleErrorCodes
                        .DuplicateAcceptedApplication,
                    emitted);
                return;
            }

            List<DamageSegment> segments = BuildDirectDamageSegments(
                item,
                firstTarget,
                out int totalApplied);
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[]
                extraTargetSources = PendingExtraTargetSources(item);
            List<ExtraTargetAssignment> extraTargetAssignments =
                BuildExtraTargetAssignments(
                    item,
                    firstTarget,
                    segments,
                    extraTargetSources);
            CanonicalBasicMutationPlan canonicalBasicPlan =
                BuildCanonicalBasicMutationPlan(
                    item,
                    firstTarget,
                    segments);
            CanonicalStatusApplicationPlan canonicalStatusPlan =
                BuildCanonicalStatusApplicationPlan(
                    item,
                    firstTarget,
                    segments);

            if (!TryCommitNianSpend(
                    item,
                    applicationId,
                    emitted))
            {
                acceptedApplicationEventIds.Remove(applicationId);
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.NianInsufficient,
                    string.Empty);
                MutableAction retry = ScheduleItem(
                    item,
                    action.SourceStableOrder,
                    NextNianAvailabilityTime(action.DueBattleTimeMs));
                EmitScheduled(emitted, retry);
                return;
            }

            foreach (DamageSegment segment in segments)
            {
                segment.Actor.CurrentHp = segment.After;
                segment.Actor.CurrentShell = segment.ShellAfter;
                if (segment.BrokeShell)
                {
                    segment.Actor.ShellBroken = true;
                }
            }
            foreach (ExtraTargetAssignment assignment in
                     extraTargetAssignments)
            {
                DamageSegment segment = assignment.Segment;
                segment.Actor.CurrentHp = segment.After;
                segment.Actor.CurrentShell = segment.ShellAfter;
                if (segment.BrokeShell)
                {
                    segment.Actor.ShellBroken = true;
                }
            }
            CommitCanonicalBasicMutation(canonicalBasicPlan);
            CommitCanonicalStatusApplication(
                canonicalStatusPlan,
                applicationId,
                emitted);
            ApplyCanonicalActionEffects(
                item,
                firstTarget,
                targetingSource,
                applicationId,
                emitted);
            int baseCleanseApplied = ApplyCanonicalBaseCleanse(
                item,
                firstTarget,
                applicationId,
                emitted);
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     extraTargetSources)
            {
                automaticPool15TriggerIds.Add(
                    AutomaticPool15SourceTriggerKey(fact.triggerId, fact));
            }

            if (!string.Equals(
                    currentTargetActorId,
                    firstTarget.Source.actorBalanceId,
                    StringComparison.Ordinal))
            {
                EmitTargetChanged(emitted, firstTarget, applicationId, item);
            }

            acceptedItemApplicationCount++;
            action.MarkExecuted(true, C1FormalRealtimeBattleErrorCodes.None,
                applicationId);
            DamageSegment primarySegment = segments.FirstOrDefault(
                segment => ReferenceEquals(segment.Actor, firstTarget))
                ?? BuildTargetedDamageSegment(
                    item,
                    firstTarget,
                    0,
                    false);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                item.sourceId,
                firstTarget.Source.actorBalanceId,
                firstTarget.StableOrder,
                playerHp,
                playerHp,
                primarySegment.Before,
                primarySegment.After,
                item.directDamage,
                totalApplied,
                CanonicalMutationSummary(
                    canonicalBasicPlan,
                    canonicalStatusPlan),
                80,
                applicationId,
                item.itemInstanceId,
                item.baseItemId,
                string.Empty,
                string.Empty,
                primarySegment.ShellBefore,
                primarySegment.ShellAfter,
                primarySegment.TotalShellApplied,
                false,
                string.Empty,
                string.Empty,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant);
            long requestedBaseCleanse = Math.Max(
                0L,
                ReadCanonicalGeneratedStat(
                    item.generatedInstance,
                    "cleanse"));
            if (requestedBaseCleanse > 0L)
            {
                EmitApplicationFeedback(
                    emitted,
                    item,
                    null,
                    applicationId,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Cleanse,
                    requestedBaseCleanse,
                    baseCleanseApplied);
            }
            foreach (DamageSegment segment in segments)
            {
                if (!string.Equals(
                        currentTargetActorId,
                        segment.Actor.Source.actorBalanceId,
                        StringComparison.Ordinal))
                {
                    EmitTargetChanged(emitted, segment.Actor, applicationId, item);
                }

                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.HitAccepted,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    item.sourceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    item.directDamage,
                    segment.Applied,
                    string.Empty,
                    75,
                    applicationId,
                    item.itemInstanceId,
                    item.baseItemId,
                    string.Empty,
                    string.Empty,
                    segment.ShellBefore,
                    segment.ShellAfter,
                    segment.TotalShellApplied,
                    false,
                    string.Empty,
                    string.Empty);
                if (segment.BaseShellApplied > 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        item.sourceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        segment.Before,
                        item.directDamage,
                        segment.BaseShellApplied,
                        "shell|" + segment.ShellBefore.ToString(
                            CultureInfo.InvariantCulture) + ">"
                        + segment.ShellAfterBase.ToString(
                            CultureInfo.InvariantCulture),
                        78,
                        applicationId,
                        item.itemInstanceId,
                        item.baseItemId,
                        string.Empty,
                        string.Empty,
                        segment.ShellBefore,
                        segment.ShellAfterBase,
                        segment.BaseShellApplied,
                        false,
                        string.Empty,
                        string.Empty,
                        triggerKind:
                            C1FormalRealtimeBattleFeedbackTriggerKinds
                                .ItemTrigger,
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds
                                .ShellDamage);
                }

                CommitShellModifierContributions(
                    segment,
                    applicationId,
                    emitted);
                if (segment.BrokeShell)
                {
                    EmitShellBroken(
                        segment,
                        item,
                        null,
                        applicationId,
                        emitted);
                }

                if (segment.Applied <= 0)
                {
                    continue;
                }

                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    item.directDamage,
                    segment.Applied,
                    string.Empty,
                    70,
                    applicationId,
                    item.itemInstanceId,
                    item.baseItemId,
                    string.Empty,
                    string.Empty,
                    triggerKind:
                        C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                    deliveryKind:
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                    resultKind:
                        C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    item.sourceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    item.directDamage,
                    segment.Applied,
                    "-" + segment.Applied.ToString(CultureInfo.InvariantCulture),
                    65,
                    applicationId,
                    item.itemInstanceId,
                    item.baseItemId,
                    string.Empty,
                    string.Empty,
                    triggerKind:
                        C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                    deliveryKind:
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                    resultKind:
                        C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                if (segment.After <= 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        0,
                        item.directDamage,
                        segment.Applied,
                        string.Empty,
                        95,
                        applicationId,
                        item.itemInstanceId,
                        item.baseItemId,
                        string.Empty,
                        string.Empty);
                    CancelEnemyActionsForDefeatedActor(
                        segment.Actor,
                        applicationId);
                    RemoveAllStatuses(
                        segment.Actor,
                        "TARGET_DEFEATED",
                        applicationId,
                        emitted);
                }
            }

            foreach (ExtraTargetAssignment assignment in
                     extraTargetAssignments)
            {
                EmitExtraTargetApplication(
                    assignment,
                    item,
                    applicationId,
                    emitted);
            }

            EmitCanonicalBasicMutationCues(
                canonicalBasicPlan,
                item,
                applicationId,
                emitted);

            foreach (DamageSegment segment in segments.Where(value =>
                         value.After > 0 && value.Applied > 0))
            {
                TriggerEnemySkills(
                    segment.Actor,
                    C1FormalEnemySkillTriggerTypes.OnDamaged,
                    segment.Applied,
                    applicationId,
                    emitted);
            }
            if (terminal)
            {
                return;
            }

            ProduceAutomaticPool15Triggers(
                new[] { "on_hit" },
                emitted);
            ApplyFormalEnemyCastDelays(applicationId, emitted);
            if (acceptedItemApplicationCount == 1)
            {
                ProduceAutomaticPool15Triggers(
                    AutomaticPool15FirstItemTriggerIds,
                    emitted);
            }
            else if (acceptedItemApplicationCount >= 2)
            {
                ApplyQualifyingI002NianRefunds(applicationId, emitted);
                if (acceptedItemApplicationCount == 2)
                {
                    ProduceAutomaticPool15Triggers(
                        new[] { "on_consecutive_trigger" },
                        emitted);
                }
            }

            if (segments.Any(segment => segment.After <= 0))
            {
                ProduceAutomaticPool15Triggers(
                    new[] { "on_kill_flaw" },
                    emitted);
            }

            if (TotalEnemyHp() <= 0)
            {
                CompleteTerminal(true, emitted);
                return;
            }

            MutableAction next = ScheduleItem(
                item,
                action.SourceStableOrder,
                checked(action.DueBattleTimeMs + action.IntervalMs));
            EmitScheduled(emitted, next);
        }

        private CanonicalBasicMutationPlan BuildCanonicalBasicMutationPlan(
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor authoritativeTarget,
            IEnumerable<DamageSegment> directDamageSegments)
        {
            if (item?.generatedInstance == null
                || item.canonicalDefinition == null
                || authoritativeTarget == null)
            {
                return null;
            }

            DamageSegment directTargetSegment = (directDamageSegments
                ?? Enumerable.Empty<DamageSegment>()).FirstOrDefault(
                segment => ReferenceEquals(
                    segment.Actor,
                    authoritativeTarget));
            int shellBeforeBreak = directTargetSegment == null
                ? authoritativeTarget.CurrentShell
                : directTargetSegment.ShellAfter;
            long requestedBreak = Math.Max(
                0L,
                ReadCanonicalGeneratedStat(item.generatedInstance, "break"));
            int breakApplied = checked((int)Math.Min(
                shellBeforeBreak,
                Math.Min(requestedBreak, int.MaxValue)));

            long requestedGuard = Math.Max(
                0L,
                ReadCanonicalGeneratedStat(item.generatedInstance, "guard"));
            long guardAfter = checked(playerGuard + requestedGuard);

            int playerMaxHp = C1FormalRealtimeBattleSessionContract
                .StartingPlayerMaxHp;
            long requestedHeal = Math.Max(
                0L,
                ReadCanonicalGeneratedStat(item.generatedInstance, "heal"));
            int healApplied = checked((int)Math.Min(
                Math.Max(0, playerMaxHp - playerHp),
                Math.Min(requestedHeal, int.MaxValue)));

            return new CanonicalBasicMutationPlan(
                authoritativeTarget,
                shellBeforeBreak,
                shellBeforeBreak - breakApplied,
                requestedBreak,
                breakApplied,
                playerGuard,
                guardAfter,
                requestedGuard,
                playerHp,
                playerHp + healApplied,
                requestedHeal,
                healApplied);
        }

        private IEnumerable<CanonicalEffectSource> CanonicalEffectSources()
        {
            foreach (C1FormalRealtimeBattleItemFactSnapshot item in
                     activeItemFacts.Where(value => value != null
                         && value.isLit
                         && value.canonicalDefinition?.combatEffect != null)
                         .OrderBy(value => value.itemInstanceId,
                             StringComparer.Ordinal))
            {
                if (C1FormalRealtimeBattleCanonicalEffectOperator.TryResolve(
                        item.canonicalDefinition.combatEffect,
                        out C1FormalRealtimeBattleCanonicalEffectPlan plan))
                {
                    yield return new CanonicalEffectSource(item, plan);
                }

                foreach (ItemGeneratedAffixSnapshot generatedAffix in
                         item.generatedInstance?.GeneratedAffixes
                         ?? Array.Empty<ItemGeneratedAffixSnapshot>())
                {
                    CanonicalItemEffectDefinition effect = item
                        .canonicalDefinition.GetGeneratedAffixEffect(
                            generatedAffix.affixId);
                    if (!IsCleanseGeneratedAffixEffect(effect)
                        || !C1FormalRealtimeBattleCanonicalEffectOperator
                            .TryResolve(
                                effect,
                                out C1FormalRealtimeBattleCanonicalEffectPlan
                                    generatedPlan))
                    {
                        continue;
                    }

                    yield return new CanonicalEffectSource(
                        item,
                        generatedPlan,
                        generatedAffix.rawUnits);
                }
            }
        }

        private static bool IsCleanseGeneratedAffixEffect(
            CanonicalItemEffectDefinition effect)
        {
            return effect != null
                   && (EqualsOrdinal(effect.triggerEventId, "on_cleanse")
                       || EqualsOrdinal(effect.targetStatId, "cleanse"));
        }

        private void ApplyCanonicalStartupEffects(
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (CanonicalEffectSource source in CanonicalEffectSources())
            {
                string trigger = source.Plan.TriggerEventId;
                if (EqualsOrdinal(trigger, "on_direct_lit")
                    && source.Item.isDirectLit
                    && source.Plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.ExtraTarget)
                {
                    SetCanonicalState(CanonicalArmKey(source), 1L);
                    continue;
                }

                long requestedGuard = 0L;
                if (source.Plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.PlayerGuardFlat)
                {
                    if ((EqualsOrdinal(trigger, "on_direct_lit")
                         && source.Item.isDirectLit)
                        || (EqualsOrdinal(trigger, "on_turn_start")
                            && source.Item.isLit))
                    {
                        requestedGuard = CanonicalEffectValue(source);
                    }
                }
                else if (source.Plan.MutationKind ==
                         C1FormalRealtimeBattleCanonicalMutationKind
                             .PlayerGuardPercent
                         && EqualsOrdinal(trigger, "on_layout_evaluate"))
                {
                    int adjacentCount = Math.Min(
                        CountAdjacentItems(source.Item, null, true),
                        checked((int)Math.Max(
                            1L,
                            source.Plan.SecondaryValueUnits)));
                    long basisPoints = checked(
                        CanonicalEffectValue(source) * adjacentCount);
                    requestedGuard = C1FormalRealtimeBattleCanonicalEffectOperator
                        .ApplyBasisPoints(
                            C1FormalRealtimeBattleSessionContract
                                .StartingPlayerMaxHp,
                            basisPoints);
                }

                if (requestedGuard > 0L)
                {
                    string applicationId = NewApplicationId();
                    acceptedApplicationEventIds.Add(applicationId);
                    ApplyCanonicalGuard(
                        source,
                        requestedGuard,
                        "startup|" + trigger,
                        applicationId,
                        emitted);
                }
            }
        }

        private MutableActor ResolveCanonicalActionTarget(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor fallback,
            out CanonicalEffectSource targetingSource)
        {
            targetingSource = null;
            if (!IsCanonicalFamily(actingItem, "taibai"))
            {
                return fallback;
            }

            targetingSource = CanonicalEffectSources().FirstOrDefault(source =>
                source.Plan.MutationKind ==
                C1FormalRealtimeBattleCanonicalMutationKind.TargetingOverride
                && GetCanonicalState(CanonicalArmKey(source)) > 0L);
            if (targetingSource == null)
            {
                return fallback;
            }

            return actors.Where(actor => actor.CurrentHp > 0)
                       .OrderBy(actor => actor.CurrentHp)
                       .ThenBy(actor => actor.StableOrder)
                       .FirstOrDefault()
                   ?? fallback;
        }

        private void ApplyCanonicalActionEffects(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target,
            CanonicalEffectSource targetingSource,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (actingItem == null || target == null)
            {
                return;
            }

            if (targetingSource != null)
            {
                SetCanonicalState(CanonicalArmKey(targetingSource), 0L);
                EmitCanonicalEffectAccepted(
                    targetingSource,
                    actingItem,
                    target,
                    applicationId,
                    1L,
                    1L,
                    "target-override",
                    emitted);
            }

            bool targetHasFlaw = GetCanonicalState(
                CanonicalActorStateKey("flaw", target)) > 0L;
            foreach (string eventId in CanonicalActionEventIds(
                         actingItem,
                         target))
            {
                ApplyCanonicalEvent(
                    eventId,
                    actingItem,
                    target,
                    applicationId,
                    emitted);
            }

            ApplyCanonicalArmedExtraTargets(
                actingItem,
                target,
                applicationId,
                emitted);
            ConsumeCanonicalEmber(
                actingItem,
                target,
                applicationId,
                emitted);

            if (target.CurrentHp <= 0 && targetHasFlaw)
            {
                ApplyCanonicalEvent(
                    "on_kill_flaw",
                    actingItem,
                    target,
                    applicationId,
                    emitted);
                SetCanonicalState(
                    CanonicalActorStateKey("flaw", target),
                    0L);
            }
        }

        private IReadOnlyList<string> CanonicalActionEventIds(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target)
        {
            if (actingItem?.canonicalDefinition == null || target == null)
            {
                return Array.Empty<string>();
            }

            List<string> events = new List<string>
            {
                "on_action_commit",
                "on_hit"
            };
            bool targetHadShield = target.Source.hasShell
                                   && (target.CurrentShell > 0
                                       || target.ShellBroken);
            if (targetHadShield)
            {
                events.Add("on_first_hit_shielded");
                events.Add("on_reveal_shield");
            }
            if (IsCanonicalFamily(actingItem, "zhenlei"))
            {
                events.Add("on_zhenlei_trigger");
            }
            if (FindBurnStatus(target) != null)
            {
                events.Add("on_hit_burning");
            }
            if (EqualsOrdinal(
                    actingItem.canonicalDefinition.qiLeiKey,
                    "jing"))
            {
                events.Add("on_mirror_trigger");
            }
            if (IsCanonicalFamily(actingItem, "taibai"))
            {
                events.Add("on_slash");
                if (GetCanonicalState(
                        CanonicalActorStateKey("flaw", target)) > 0L)
                {
                    events.Add("on_hit_flaw");
                }
            }
            if (EqualsOrdinal(
                    actingItem.canonicalDefinition.qiLeiKey,
                    "ling"))
            {
                events.Add("on_command_trigger");
            }
            if (acceptedItemApplicationCount > 0)
            {
                events.Add("on_consecutive_trigger");
            }
            return events.Distinct(StringComparer.Ordinal).ToArray();
        }

        private void ApplyCanonicalEvent(
            string eventId,
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => EqualsOrdinal(
                             value.Plan.TriggerEventId,
                             eventId)))
            {
                if (IsItemOwnedCanonicalEvent(eventId)
                    && !ReferenceEquals(source.Item, actingItem))
                {
                    continue;
                }

                string receiptKey = applicationId + "|" + eventId + "|"
                    + source.Item.itemInstanceId + "|" + source.Plan.EffectId;
                if (!acceptedCanonicalEffectEvents.Add(receiptKey))
                {
                    continue;
                }

                long effectValue = CanonicalEffectValue(source);
                switch (source.Plan.MutationKind)
                {
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .EnemyShellPercent:
                    {
                        if (target == null || target.CurrentShell <= 0)
                        {
                            break;
                        }
                        long requested = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                Math.Max(1, actingItem?.directDamage ?? 0),
                                effectValue);
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            target,
                            requested,
                            true,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .DamageToBreak:
                    {
                        if (target == null || target.CurrentShell <= 0)
                        {
                            break;
                        }
                        long requested = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                Math.Max(1, actingItem?.directDamage ?? 0),
                                effectValue);
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            target,
                            requested,
                            true,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .ExtraBreakPercent:
                    {
                        string countKey = CanonicalCounterKey(source, eventId);
                        long count = GetCanonicalState(countKey) + 1L;
                        SetCanonicalState(countKey, count);
                        if (count % 3L != 0L || target == null)
                        {
                            break;
                        }
                        long requested = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                Math.Max(1, actingItem?.directDamage ?? 0),
                                effectValue);
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            target,
                            requested,
                            true,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerNianRefund:
                        ApplyCanonicalNianRefund(
                            source,
                            checked((int)Math.Min(effectValue, int.MaxValue)),
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .ExtraBurnTrigger:
                    {
                        BattleStatusState burn = FindBurnStatus(target);
                        if (burn == null)
                        {
                            break;
                        }
                        int potency = burn.Contributions.Sum(
                            value => value.TickPotency);
                        long requested = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                potency,
                                source.Plan.SecondaryValueUnits);
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            target,
                            requested,
                            false,
                            eventId,
                            applicationId,
                            emitted);
                        long extension = checked(
                            Math.Max(1L, source.Plan.ValueUnits)
                            * Math.Max(1L, actingItem.cooldownMs));
                        burn.ExpireAtBattleTimeMs = checked(
                            burn.ExpireAtBattleTimeMs + extension);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .BurnConversion:
                    {
                        BattleStatusState burn = FindBurnStatus(target);
                        MutableActor secondary = actors.Where(actor =>
                                actor.CurrentHp > 0
                                && !ReferenceEquals(actor, target))
                            .OrderBy(actor => Math.Abs(
                                actor.StableOrder - target.StableOrder))
                            .ThenBy(actor => actor.StableOrder)
                            .FirstOrDefault();
                        if (burn == null || secondary == null)
                        {
                            break;
                        }
                        int potency = burn.Contributions
                            .OrderBy(value => value.AppliedAtBattleTimeMs)
                            .Select(value => value.TickPotency)
                            .FirstOrDefault();
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            secondary,
                            Math.Max(1, potency),
                            false,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .DirectDamagePercent:
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .ExtraDamagePercent:
                    {
                        if (target == null)
                        {
                            break;
                        }
                        if (source.Plan.MutationKind ==
                            C1FormalRealtimeBattleCanonicalMutationKind
                                .ExtraDamagePercent)
                        {
                            long threshold = source.Plan.SecondaryValueUnits;
                            if (threshold > 0L
                                && (long)target.CurrentHp * 10000L
                                > (long)target.Source.maxHp * threshold)
                            {
                                break;
                            }
                        }
                        long requested = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                Math.Max(1, actingItem?.directDamage ?? 0),
                                effectValue);
                        ApplyCanonicalDamage(
                            source,
                            actingItem,
                            target,
                            requested,
                            false,
                            eventId,
                            applicationId,
                            emitted);
                        if (targetHasCanonicalFlaw(target))
                        {
                            SetCanonicalState(
                                CanonicalActorStateKey("flaw", target),
                                Math.Max(0L, GetCanonicalState(
                                    CanonicalActorStateKey("flaw", target))
                                    - 1L));
                        }
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerGuardFlat:
                        ApplyCanonicalGuard(
                            source,
                            effectValue,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerGuardPercent:
                        ApplyCanonicalGuard(
                            source,
                            C1FormalRealtimeBattleCanonicalEffectOperator
                                .ApplyBasisPoints(
                                    C1FormalRealtimeBattleSessionContract
                                        .StartingPlayerMaxHp,
                                    effectValue),
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .DamageToGuard:
                    {
                        if (!EqualsOrdinal(eventId, "on_mirror_trigger"))
                        {
                            break;
                        }

                        long armedGuard =
                            C1FormalRealtimeBattleCanonicalEffectOperator
                                .ApplyBasisPoints(
                                    Math.Max(
                                        1,
                                        actingItem?.directDamage ?? 0),
                                    effectValue);
                        if (armedGuard > 0L)
                        {
                            SetCanonicalState(
                                CanonicalArmKey(source),
                                armedGuard);
                        }
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .HealToGuard:
                        ApplyCanonicalCleanseHealToGuard(
                            source,
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind.Cleanse:
                    {
                        if (!EqualsOrdinal(eventId, "on_cleanse")
                            || (EqualsOrdinal(
                                    source.Plan.ConditionId,
                                    "same_target_chain")
                                && GetCanonicalState(
                                    CanonicalCleanseChainKey(source)) < 2L))
                        {
                            break;
                        }

                        int requested = checked((int)Math.Min(
                            Math.Max(0L, effectValue),
                            int.MaxValue));
                        int removed = CleansePlayerStatusStacks(
                            requested,
                            source.Item.itemInstanceId,
                            source.Item.baseItemId,
                            applicationId,
                            emitted);
                        EmitCanonicalEffectAccepted(
                            source,
                            null,
                            null,
                            applicationId,
                            requested,
                            removed,
                            "cleanse-extra",
                            emitted);
                        break;
                    }
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .DebuffToHeal:
                        ApplyCanonicalHeal(
                            source,
                            checked((int)Math.Min(
                                Math.Max(1L, effectValue),
                                int.MaxValue)),
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerHealFlat:
                        if (EqualsOrdinal(
                                source.Plan.ConditionId,
                                "water_charge_3"))
                        {
                            string chargeKey = CanonicalWaterChargeKey(source);
                            long charges = GetCanonicalState(chargeKey);
                            if (charges < 3L)
                            {
                                break;
                            }
                            SetCanonicalState(chargeKey, charges - 3L);
                        }
                        ApplyCanonicalHeal(
                            source,
                            checked((int)Math.Min(effectValue, int.MaxValue)),
                            eventId,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .EnemyCastDelay:
                        if (ApplyCanonicalEnemyCastDelay(
                                source,
                                effectValue,
                                applicationId,
                                emitted)
                            && target != null)
                        {
                            SetCanonicalState(
                                CanonicalActorStateKey("flaw", target),
                                GetCanonicalState(
                                    CanonicalActorStateKey("flaw", target))
                                + 1L);
                        }
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .CooldownReduction:
                        ApplyCanonicalAdjacentCooldownAdvance(
                            source,
                            effectValue,
                            applicationId,
                            emitted);
                        break;
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .TargetingOverride:
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .ExtraTarget:
                    case C1FormalRealtimeBattleCanonicalMutationKind
                        .EmberTrigger:
                        SetCanonicalState(CanonicalArmKey(source),
                            Math.Max(1L, effectValue));
                        break;
                }
            }
        }

        private int ApplyCanonicalBaseCleanse(
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor target,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            long generatedCleanse = ReadCanonicalGeneratedStat(
                item?.generatedInstance,
                "cleanse");
            int requested = checked((int)Math.Min(
                Math.Max(0L, generatedCleanse),
                int.MaxValue));
            int removed = CleansePlayerStatusStacks(
                requested,
                item?.itemInstanceId,
                item?.baseItemId,
                applicationId,
                emitted);
            if (removed <= 0)
            {
                return 0;
            }

            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => ReferenceEquals(value.Item, item)
                             && EqualsOrdinal(
                                 value.Plan.TriggerEventId,
                                 "on_cleanse")))
            {
                if (EqualsOrdinal(
                        source.Plan.ConditionId,
                        "same_target_chain"))
                {
                    string chainKey = CanonicalCleanseChainKey(source);
                    SetCanonicalState(
                        chainKey,
                        GetCanonicalState(chainKey) + 1L);
                }
                else if (EqualsOrdinal(
                             source.Plan.ConditionId,
                             "water_charge_3"))
                {
                    string chargeKey = CanonicalWaterChargeKey(source);
                    SetCanonicalState(
                        chargeKey,
                        GetCanonicalState(chargeKey) + 1L);
                }
            }

            ApplyCanonicalEvent(
                "on_cleanse",
                item,
                target,
                applicationId,
                emitted);
            return removed;
        }

        private bool HasCleanseablePlayerStatus()
        {
            return playerStatuses.Any(value => value != null
                && value.Cleanseable
                && value.StackCount > 0)
                || enemySkillStatuses.Any(value => value != null
                    && !value.Removed
                    && value.TargetActor == null
                    && value.Definition.Cleanseable
                    && value.StackCount > 0);
        }

        private int CleansePlayerStatusStacks(
            int requestedStacks,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            int remaining = Math.Max(0, requestedStacks);
            int removed = 0;
            foreach (PlayerBattleStatusState status in playerStatuses
                         .Where(value => value != null
                             && value.Cleanseable
                             && value.StackCount > 0)
                         .OrderBy(value => value.AppliedAtBattleTimeMs)
                         .ThenBy(value => value.StatusKey, StringComparer.Ordinal)
                         .ToArray())
            {
                if (remaining <= 0)
                {
                    break;
                }

                int before = status.StackCount;
                int applied = Math.Min(before, remaining);
                status.StackCount -= applied;
                remaining -= applied;
                removed += applied;
                Emit(
                    emitted,
                    status.StackCount == 0
                        ? C1FormalRealtimeBattleCueKinds.StatusRemoved
                        : C1FormalRealtimeBattleCueKinds.StatusRefreshed,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    sourceItemInstanceId ?? string.Empty,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    -1,
                    playerHp,
                    playerHp,
                    0,
                    0,
                    before,
                    applied,
                    status.StatusKey + "|cleanse|stack="
                    + before.ToString(CultureInfo.InvariantCulture) + ">"
                    + status.StackCount.ToString(
                        CultureInfo.InvariantCulture),
                    76,
                    applicationId,
                    sourceItemInstanceId ?? string.Empty,
                    sourceBaseItemId ?? string.Empty,
                    status.StatusFamilyKey,
                    status.StatusKey);
            }

            foreach (EnemySkillStatusState status in enemySkillStatuses
                         .Where(value => value != null
                             && !value.Removed
                             && value.TargetActor == null
                             && value.Definition.Cleanseable
                             && value.StackCount > 0)
                         .OrderBy(value => value.AppliedAtBattleTimeMs)
                         .ThenBy(value => value.StatusKey,
                             StringComparer.Ordinal)
                         .ToArray())
            {
                if (remaining <= 0)
                {
                    break;
                }

                int before = status.StackCount;
                int applied = Math.Min(before, remaining);
                status.Contributions.RemoveRange(0, applied);
                remaining -= applied;
                removed += applied;
                if (status.StackCount == 0)
                {
                    RemoveEnemySkillStatus(
                        status,
                        "CLEANSED_BY:"
                        + (sourceItemInstanceId ?? string.Empty),
                        applicationId,
                        emitted);
                }
                else
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.StatusRefreshed,
                        C1FormalRealtimeBattleSessionContract.PlayerOwner,
                        sourceItemInstanceId ?? string.Empty,
                        C1FormalRealtimeBattleSessionContract.PlayerActorId,
                        -1,
                        playerHp,
                        playerHp,
                        0,
                        0,
                        before,
                        applied,
                        status.StatusKey + "|cleanse|stack="
                        + before.ToString(CultureInfo.InvariantCulture) + ">"
                        + status.StackCount.ToString(
                            CultureInfo.InvariantCulture),
                        76,
                        applicationId,
                        sourceItemInstanceId ?? string.Empty,
                        sourceBaseItemId ?? string.Empty,
                        status.StatusFamilyKey,
                        status.StatusKey);
                }
            }

            playerStatuses.RemoveAll(value => value == null
                || value.StackCount <= 0);
            return removed;
        }

        private void ApplyCanonicalCleanseHealToGuard(
            CanonicalEffectSource source,
            string eventId,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || !EqualsOrdinal(eventId, "on_cleanse"))
            {
                return;
            }

            long generatedHeal = ReadCanonicalGeneratedStat(
                source.Item.generatedInstance,
                "heal");
            int requestedHeal = checked((int)Math.Min(
                Math.Max(generatedHeal, source.Plan.SecondaryValueUnits),
                int.MaxValue));
            int missing = Math.Max(
                0,
                C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp
                - playerHp);
            int healApplied = Math.Min(requestedHeal, missing);
            if (healApplied > 0)
            {
                ApplyCanonicalHeal(
                    source,
                    healApplied,
                    "cleanse-heal",
                    applicationId,
                    emitted);
            }

            int overflow = requestedHeal - healApplied;
            long guard = C1FormalRealtimeBattleCanonicalEffectOperator
                .ApplyBasisPoints(
                    overflow,
                    CanonicalEffectValue(source));
            ApplyCanonicalGuard(
                source,
                guard,
                "cleanse-overheal-to-guard",
                applicationId,
                emitted);
        }

        private void ApplyCanonicalArmedExtraTargets(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor primaryTarget,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => value.Plan.MutationKind ==
                             C1FormalRealtimeBattleCanonicalMutationKind
                                 .ExtraTarget))
            {
                long remaining = GetCanonicalState(CanonicalArmKey(source));
                bool matchingAction = EqualsOrdinal(
                        source.Plan.ConditionId,
                        "next_zhenlei_trigger")
                    ? IsCanonicalFamily(actingItem, "zhenlei")
                    : EqualsOrdinal(source.Plan.ConditionId, "next_slash")
                      && IsCanonicalFamily(actingItem, "taibai");
                if (remaining <= 0L || !matchingAction)
                {
                    continue;
                }

                MutableActor extraTarget = actors.Where(actor =>
                        actor.CurrentHp > 0
                        && !ReferenceEquals(actor, primaryTarget))
                    .OrderBy(actor => Math.Abs(
                        actor.StableOrder - primaryTarget.StableOrder))
                    .ThenBy(actor => actor.StableOrder)
                    .FirstOrDefault();
                if (extraTarget == null)
                {
                    continue;
                }

                long applied = ApplyCanonicalDamage(
                    source,
                    actingItem,
                    extraTarget,
                    Math.Max(1, actingItem.directDamage),
                    false,
                    "armed-extra-target",
                    applicationId,
                    emitted);
                if (applied > 0L)
                {
                    SetCanonicalState(
                        CanonicalArmKey(source),
                        remaining - 1L);
                }
            }
        }

        private void ConsumeCanonicalEmber(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (FindBurnStatus(target) == null)
            {
                return;
            }

            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => value.Plan.MutationKind ==
                             C1FormalRealtimeBattleCanonicalMutationKind
                                 .EmberTrigger))
            {
                long charges = GetCanonicalState(CanonicalArmKey(source));
                if (charges <= 0L)
                {
                    continue;
                }

                long applied = ApplyCanonicalDamage(
                    source,
                    actingItem,
                    target,
                    Math.Max(1, actingItem.directDamage),
                    false,
                    "ember-consumed",
                    applicationId,
                    emitted);
                if (applied > 0L)
                {
                    SetCanonicalState(CanonicalArmKey(source), charges - 1L);
                }
            }
        }

        private int ResolveCanonicalNianCost(
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            int baseCost,
            out CanonicalEffectSource reductionSource)
        {
            reductionSource = CanonicalEffectSources().FirstOrDefault(source =>
                ReferenceEquals(source.Item, actingItem)
                && source.Plan.MutationKind ==
                C1FormalRealtimeBattleCanonicalMutationKind.NianCostReduction
                && CountAdjacentItems(
                    source.Item,
                    item => IsCanonicalFamily(item, "lihuo"),
                    false) > 0);
            if (reductionSource == null)
            {
                return baseCost;
            }

            int reduction = checked((int)Math.Min(
                CanonicalEffectValue(reductionSource),
                baseCost));
            return baseCost - reduction;
        }

        private void ApplyCanonicalAdjacentCooldownAdvance(
            CanonicalEffectSource source,
            long steps,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || steps <= 0L)
            {
                return;
            }

            long totalAdvance = 0L;
            foreach (C1FormalRealtimeBattleItemFactSnapshot adjacentItem in
                     activeItemFacts.Where(item => item != null
                             && item.isLit
                             && !ReferenceEquals(item, source.Item)
                             && AreCanonicalItemsAdjacent(
                                 source.Item,
                                 item))
                         .OrderBy(item => item.itemInstanceId,
                             StringComparer.Ordinal))
            {
                MutableAction targetAction = actions.Where(action =>
                        !action.Executed
                        && action.DueBattleTimeMs > battleTimeMs
                        && EqualsOrdinal(
                            action.ActionKind,
                            C1FormalRealtimeBattleScheduledActionKinds
                                .ItemTrigger)
                        && EqualsOrdinal(
                            action.SourceId,
                            adjacentItem.sourceId)
                        && action.IntervalMs > 0L)
                    .OrderBy(action => action.DueBattleTimeMs)
                    .ThenBy(action => action.StableOrder)
                    .FirstOrDefault();
                if (targetAction == null)
                {
                    continue;
                }

                long before = targetAction.DueBattleTimeMs;
                long after = before;
                for (long step = 0L; step < steps; step++)
                {
                    if (!TryResolveDiscreteProgressDueTime(
                            C1FormalRealtimeBattleActionProgressOperatorIds
                                .AdvanceItemTriggerProgressStep,
                            "turn",
                            battleTimeMs,
                            after,
                            targetAction.IntervalMs,
                            out long advanced))
                    {
                        break;
                    }
                    after = advanced;
                }

                if (after >= before
                    || !targetAction.TryReschedule(before, after))
                {
                    continue;
                }
                totalAdvance = checked(totalAdvance + before - after);
            }

            if (totalAdvance > 0L)
            {
                EmitCanonicalEffectAccepted(
                    source,
                    null,
                    null,
                    applicationId,
                    steps,
                    totalAdvance,
                    "adjacent-cooldown-advance",
                    emitted);
            }
        }

        private int ResolveCanonicalStatusTickDamage(
            BattleStatusState status,
            BattleStatusContribution contribution,
            out CanonicalEffectSource modifier)
        {
            modifier = null;
            long requested = contribution.TickPotency;
            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => value.Plan.MutationKind ==
                             C1FormalRealtimeBattleCanonicalMutationKind
                                 .DirectDamagePercent
                             && EqualsOrdinal(
                                 value.Plan.TriggerEventId,
                                 "on_tick")))
            {
                int stackCount = Math.Min(
                    status.StackCount,
                    checked((int)Math.Max(
                        1L,
                        source.Plan.SecondaryValueUnits)));
                long basisPoints = checked(
                    CanonicalEffectValue(source) * stackCount);
                requested = checked(requested
                    + C1FormalRealtimeBattleCanonicalEffectOperator
                        .ApplyBasisPoints(
                            contribution.TickPotency,
                            basisPoints));
                modifier = source;
            }
            return checked((int)Math.Min(requested, int.MaxValue));
        }

        private void ApplyCanonicalStatusExpiredEffects(
            BattleStatusState status,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (status == null)
            {
                return;
            }

            ApplyCanonicalEvent(
                "on_burn_expire",
                null,
                status.Target,
                applicationId,
                emitted);
            ApplyCanonicalEvent(
                "on_effect_end",
                null,
                status.Target,
                applicationId,
                emitted);
        }

        private void ApplyCanonicalPlayerDamagedEffects(
            int requestedDamage,
            int appliedDamage,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (CanonicalEffectSource source in CanonicalEffectSources()
                         .Where(value => EqualsOrdinal(
                                 value.Plan.TriggerEventId,
                                 "on_damaged")
                             || EqualsOrdinal(
                                 value.Plan.TriggerEventId,
                                 "on_ally_damaged")
                             || (value.Plan.MutationKind ==
                                 C1FormalRealtimeBattleCanonicalMutationKind
                                     .DamageToGuard
                                 && GetCanonicalState(
                                     CanonicalArmKey(value)) > 0L)))
            {
                long guard = 0L;
                if (source.Plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.PlayerGuardFlat
                    && playerGuard < CanonicalEffectValue(source))
                {
                    guard = CanonicalEffectValue(source);
                }
                else if (source.Plan.MutationKind ==
                         C1FormalRealtimeBattleCanonicalMutationKind
                             .DamageToGuard)
                {
                    string armKey = CanonicalArmKey(source);
                    long armedGuard = GetCanonicalState(armKey);
                    if (armedGuard > 0L)
                    {
                        guard = armedGuard;
                        SetCanonicalState(armKey, 0L);
                    }
                    else
                    {
                        guard = C1FormalRealtimeBattleCanonicalEffectOperator
                            .ApplyBasisPoints(
                                Math.Max(requestedDamage, appliedDamage),
                                CanonicalEffectValue(source));
                    }
                }

                if (guard > 0L)
                {
                    ApplyCanonicalGuard(
                        source,
                        guard,
                        source.Plan.TriggerEventId,
                        applicationId,
                        emitted);
                }
            }
        }

        private long ApplyCanonicalDamage(
            CanonicalEffectSource source,
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target,
            long requestedDamage,
            bool shellOnly,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || target == null || requestedDamage <= 0L
                || target.CurrentHp <= 0)
            {
                return 0L;
            }

            int requested = checked((int)Math.Min(
                requestedDamage,
                int.MaxValue));
            int shellBefore = target.CurrentShell;
            int hpBefore = target.CurrentHp;
            int shellApplied = Math.Min(shellBefore, requested);
            int remaining = shellOnly ? 0 : requested - shellApplied;
            int hpApplied = Math.Min(hpBefore, remaining);
            target.CurrentShell -= shellApplied;
            target.CurrentHp -= hpApplied;
            if (shellBefore > 0 && target.CurrentShell == 0)
            {
                target.ShellBroken = true;
            }
            long applied = checked((long)shellApplied + hpApplied);
            if (applied <= 0L)
            {
                return 0L;
            }

            EmitCanonicalEffectAccepted(
                source,
                actingItem,
                target,
                applicationId,
                requested,
                applied,
                reason,
                emitted);
            if (shellApplied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    source.Item.sourceId,
                    target.Source.actorBalanceId,
                    target.StableOrder,
                    playerHp,
                    playerHp,
                    hpBefore,
                    target.CurrentHp,
                    requested,
                    shellApplied,
                    "canonical-effect|" + reason,
                    78,
                    applicationId,
                    source.Item.itemInstanceId,
                    source.Item.baseItemId,
                    "CANONICAL_EFFECT",
                    source.Plan.EffectId,
                    shellBefore,
                    target.CurrentShell,
                    shellApplied,
                    target.ShellBroken,
                    target.ShellBroken
                        ? target.Source.shellBrokenStateId
                        : string.Empty,
                    target.ShellBroken
                        ? target.Source.shellBreakCounterWindowId
                        : string.Empty);
            }
            if (hpApplied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    source.Item.sourceId,
                    target.Source.actorBalanceId,
                    target.StableOrder,
                    playerHp,
                    playerHp,
                    hpBefore,
                    target.CurrentHp,
                    requested,
                    hpApplied,
                    "canonical-effect|" + reason,
                    70,
                    applicationId,
                    source.Item.itemInstanceId,
                    source.Item.baseItemId,
                    "CANONICAL_EFFECT",
                    source.Plan.EffectId);
            }
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                source.Item.sourceId,
                target.Source.actorBalanceId,
                target.StableOrder,
                playerHp,
                playerHp,
                hpBefore,
                target.CurrentHp,
                requested,
                ToCueAmount(applied),
                "-" + applied.ToString(CultureInfo.InvariantCulture),
                65,
                applicationId,
                source.Item.itemInstanceId,
                source.Item.baseItemId,
                "CANONICAL_EFFECT",
                source.Plan.EffectId);
            return applied;
        }

        private void ApplyCanonicalGuard(
            CanonicalEffectSource source,
            long requestedGuard,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || requestedGuard <= 0L)
            {
                return;
            }
            long before = playerGuard;
            playerGuard = checked(playerGuard + requestedGuard);
            effectState[EffectStorageKey(
                "player.guard",
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                "flat")] = playerGuard;
            EmitCanonicalEffectAccepted(
                source,
                null,
                null,
                applicationId,
                requestedGuard,
                playerGuard - before,
                reason,
                emitted);
            EmitApplicationFeedback(
                emitted,
                source.Item,
                null,
                applicationId,
                C1FormalRealtimeBattleApplicationFeedbackKinds.Guard,
                requestedGuard,
                playerGuard - before);
        }

        private void ApplyCanonicalHeal(
            CanonicalEffectSource source,
            int requestedHeal,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || requestedHeal <= 0)
            {
                return;
            }
            int before = playerHp;
            playerHp = Math.Min(
                C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp,
                playerHp + requestedHeal);
            int applied = playerHp - before;
            EmitApplicationFeedback(
                emitted,
                source.Item,
                null,
                applicationId,
                C1FormalRealtimeBattleApplicationFeedbackKinds.Heal,
                requestedHeal,
                applied);
            if (applied <= 0)
            {
                return;
            }
            EmitCanonicalEffectAccepted(
                source,
                null,
                null,
                applicationId,
                requestedHeal,
                applied,
                reason,
                emitted);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.PlayerHpChanged,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                source.Item.sourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                requestedHeal,
                applied,
                "+" + applied.ToString(CultureInfo.InvariantCulture),
                70,
                applicationId,
                source.Item.itemInstanceId,
                source.Item.baseItemId,
                "CANONICAL_EFFECT",
                source.Plan.EffectId);
        }

        private void ApplyCanonicalNianRefund(
            CanonicalEffectSource source,
            int requested,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || requested <= 0)
            {
                return;
            }
            int applied = nianCapacity == null
                ? 0
                : Math.Min(
                    requested,
                    Math.Max(0, nianCapacity.maxNian - currentNian));
            EmitApplicationFeedback(
                emitted,
                source.Item,
                null,
                applicationId,
                C1FormalRealtimeBattleApplicationFeedbackKinds.Nian,
                requested,
                applied);
            if (applied <= 0)
            {
                return;
            }
            int before = currentNian;
            currentNian += applied;
            C1FormalRealtimeBattleNianTransactionSnapshot transaction =
                NewNianTransaction(
                    source.Item.itemInstanceId,
                    source.Item.baseItemId,
                    applicationId,
                    C1FormalRealtimeBattleNianTransactionKinds.Refund,
                    requested,
                    applied,
                    before,
                    currentNian);
            nianTransactionLedger.Add(transaction);
            EmitNianTransaction(
                emitted,
                transaction,
                "CANONICAL_EFFECT",
                source.Plan.EffectId);
        }

        private void ApplyI031NianGeneration(
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            int requested = nianCapacity.generationAmount;
            int applied = Math.Min(
                requested,
                Math.Max(0, nianCapacity.maxNian - currentNian));
            if (applied <= 0)
            {
                return;
            }

            int before = currentNian;
            currentNian += applied;
            string eventId = request.sessionId
                + ":i031-nian-generation:"
                + battleTimeMs.ToString(CultureInfo.InvariantCulture);
            C1FormalRealtimeBattleNianTransactionSnapshot transaction =
                NewNianTransaction(
                    nianCapacity.sourceItemInstanceId,
                    nianCapacity.sourceBaseItemId,
                    eventId,
                    C1FormalRealtimeBattleNianTransactionKinds.Generation,
                    requested,
                    applied,
                    before,
                    currentNian);
            nianTransactionLedger.Add(transaction);
            EmitNianTransaction(
                emitted,
                transaction,
                nianCapacity.sourceBaseItemId,
                nianCapacity.sourceRevision);
        }

        private bool ApplyCanonicalEnemyCastDelay(
            CanonicalEffectSource source,
            long steps,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || steps <= 0L)
            {
                return false;
            }
            MutableAction targetAction = actions.Where(action =>
                    !action.Executed
                    && action.DueBattleTimeMs > battleTimeMs
                    && IsCastProgressCapable(action)
                    && action.IntervalMs > 0L)
                .OrderBy(action => action.DueBattleTimeMs)
                .ThenBy(action => action.StableOrder)
                .FirstOrDefault();
            if (targetAction == null)
            {
                EmitApplicationFeedback(
                    emitted,
                    source.Item,
                    null,
                    applicationId,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Control,
                    steps,
                    0L);
                return false;
            }
            long before = targetAction.DueBattleTimeMs;
            long after = checked(before + targetAction.IntervalMs * steps);
            if (!targetAction.TryReschedule(before, after))
            {
                return false;
            }
            EmitCanonicalEffectAccepted(
                source,
                null,
                null,
                applicationId,
                steps,
                after - before,
                "enemy-cast-delay|" + before.ToString(
                    CultureInfo.InvariantCulture) + ">"
                + after.ToString(CultureInfo.InvariantCulture),
                emitted);
            EmitApplicationFeedback(
                emitted,
                source.Item,
                targetAction.SourceStableOrder < 0
                    ? null
                    : actors.FirstOrDefault(actor =>
                        actor.StableOrder == targetAction.SourceStableOrder),
                applicationId,
                C1FormalRealtimeBattleApplicationFeedbackKinds.Control,
                steps,
                steps);
            return true;
        }

        private void EmitCanonicalEffectAccepted(
            CanonicalEffectSource source,
            C1FormalRealtimeBattleItemFactSnapshot actingItem,
            MutableActor target,
            string applicationId,
            long requested,
            long applied,
            string reason,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || applied <= 0L)
            {
                return;
            }
            string family = "CANONICAL_EFFECT";
            if (CanonicalItemPresentationIdentityResolver.TryResolve(
                    source.Item.canonicalDefinition,
                    out string resolvedFamily,
                    out _,
                    out _))
            {
                family = resolvedFamily;
            }
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                source.Item.sourceId,
                target == null
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : target.Source.actorBalanceId,
                target == null ? -1 : target.StableOrder,
                playerHp,
                playerHp,
                target == null ? 0 : target.CurrentHp,
                target == null ? 0 : target.CurrentHp,
                ToCueAmount(requested),
                ToCueAmount(applied),
                "canonical-effect|" + source.Plan.EffectId + "|" + reason,
                82,
                applicationId,
                source.Item.itemInstanceId,
                source.Item.baseItemId,
                family,
                source.Plan.EffectId);
        }

        private int CountAdjacentItems(
            C1FormalRealtimeBattleItemFactSnapshot source,
            Func<C1FormalRealtimeBattleItemFactSnapshot, bool> predicate,
            bool horizontalOnly)
        {
            return activeItemFacts.Count(item => item != null
                && item.isLit
                && !ReferenceEquals(item, source)
                && (predicate == null || predicate(item))
                && AreCanonicalItemsAdjacent(source, item, horizontalOnly));
        }

        private bool AreCanonicalItemsAdjacent(
            C1FormalRealtimeBattleItemFactSnapshot left,
            C1FormalRealtimeBattleItemFactSnapshot right,
            bool horizontalOnly = false)
        {
            if (left?.canonicalDefinition == null
                || right?.canonicalDefinition == null)
            {
                return false;
            }
            foreach (string leftCell in CanonicalOccupiedCells(left))
            {
                string[] parts = leftCell.Split(':');
                int x = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int y = int.Parse(parts[1], CultureInfo.InvariantCulture);
                foreach (string rightCell in CanonicalOccupiedCells(right))
                {
                    string[] otherParts = rightCell.Split(':');
                    int otherX = int.Parse(
                        otherParts[0],
                        CultureInfo.InvariantCulture);
                    int otherY = int.Parse(
                        otherParts[1],
                        CultureInfo.InvariantCulture);
                    int dx = Math.Abs(x - otherX);
                    int dy = Math.Abs(y - otherY);
                    if ((horizontalOnly && dx == 1 && dy == 0)
                        || (!horizontalOnly && dx + dy == 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private IEnumerable<string> CanonicalOccupiedCells(
            C1FormalRealtimeBattleItemFactSnapshot item)
        {
            int normalizedRotation = ((item.rotation % 360) + 360) % 360;
            foreach (var cell in item.canonicalDefinition.ShapeCells)
            {
                int x = cell.x;
                int y = cell.y;
                int rotatedX = normalizedRotation == 90 ? -y
                    : normalizedRotation == 180 ? -x
                    : normalizedRotation == 270 ? y
                    : x;
                int rotatedY = normalizedRotation == 90 ? x
                    : normalizedRotation == 180 ? -y
                    : normalizedRotation == 270 ? -x
                    : y;
                yield return (item.anchorX + rotatedX).ToString(
                        CultureInfo.InvariantCulture)
                    + ":" + (item.anchorY + rotatedY).ToString(
                        CultureInfo.InvariantCulture);
            }
        }

        private static bool IsItemOwnedCanonicalEvent(string eventId)
        {
            return EqualsOrdinal(eventId, "on_action_commit")
                || EqualsOrdinal(eventId, "on_hit")
                || EqualsOrdinal(eventId, "on_first_hit_shielded")
                || EqualsOrdinal(eventId, "on_reveal_shield")
                || EqualsOrdinal(eventId, "on_hit_burning")
                || EqualsOrdinal(eventId, "on_slash")
                || EqualsOrdinal(eventId, "on_hit_flaw")
                || EqualsOrdinal(eventId, "on_command_trigger")
                || EqualsOrdinal(eventId, "on_consecutive_trigger")
                || EqualsOrdinal(eventId, "on_cleanse");
        }

        private static bool IsCanonicalFamily(
            C1FormalRealtimeBattleItemFactSnapshot item,
            string family)
        {
            return item?.canonicalDefinition != null
                && EqualsOrdinal(item.canonicalDefinition.faMenKey, family);
        }

        private static long CanonicalEffectValue(CanonicalEffectSource source)
        {
            if (source?.GeneratedValueUnits > 0L)
            {
                return source.GeneratedValueUnits;
            }
            return Math.Max(0L, source?.Plan.ValueUnits ?? 0L);
        }

        private BattleStatusState FindBurnStatus(MutableActor actor)
        {
            return actor?.Statuses.FirstOrDefault(status =>
                !status.Removed
                && (EqualsOrdinal(status.StatusKey, "afterglow")
                    || EqualsOrdinal(
                        status.StatusFamilyKey,
                        "PERIODIC_DAMAGE")));
        }

        private bool targetHasCanonicalFlaw(MutableActor target)
        {
            return GetCanonicalState(
                CanonicalActorStateKey("flaw", target)) > 0L;
        }

        private long GetCanonicalState(string key)
        {
            return key != null
                   && canonicalEffectRuntimeState.TryGetValue(
                       key,
                       out long value)
                ? value
                : 0L;
        }

        private void SetCanonicalState(string key, long value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                canonicalEffectRuntimeState[key] = Math.Max(0L, value);
            }
        }

        private static string CanonicalArmKey(CanonicalEffectSource source)
        {
            return "canonical|arm|" + source.Item.itemInstanceId + "|"
                + source.Plan.EffectId;
        }

        private static string CanonicalCleanseChainKey(
            CanonicalEffectSource source)
        {
            return "canonical|cleanse-chain|"
                + source.Item.itemInstanceId + "|" + source.Plan.EffectId;
        }

        private static string CanonicalWaterChargeKey(
            CanonicalEffectSource source)
        {
            return "canonical|water-charge|"
                + source.Item.itemInstanceId + "|" + source.Plan.EffectId;
        }

        private static string CanonicalCounterKey(
            CanonicalEffectSource source,
            string eventId)
        {
            return "canonical|count|" + source.Item.itemInstanceId + "|"
                + source.Plan.EffectId + "|" + eventId;
        }

        private static string CanonicalActorStateKey(
            string state,
            MutableActor actor)
        {
            return "canonical|actor|" + state + "|"
                + (actor == null
                    ? string.Empty
                    : actor.Source.actorBalanceId);
        }

        private void CommitCanonicalBasicMutation(
            CanonicalBasicMutationPlan plan)
        {
            if (plan == null)
            {
                return;
            }

            if (plan.BreakApplied > 0)
            {
                plan.Target.CurrentShell = plan.ShellAfterBreak;
                if (plan.ShellBeforeBreak > 0
                    && plan.ShellAfterBreak == 0)
                {
                    plan.Target.ShellBroken = true;
                }
            }

            if (plan.GuardApplied > 0L)
            {
                playerGuard = plan.PlayerGuardAfter;
            }

            if (plan.HealApplied > 0)
            {
                playerHp = plan.PlayerHpAfter;
            }
        }

        private void EmitCanonicalBasicMutationCues(
            CanonicalBasicMutationPlan plan,
            C1FormalRealtimeBattleItemFactSnapshot item,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (plan == null || item == null)
            {
                return;
            }

            if (plan.BreakApplied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    item.sourceId,
                    plan.Target.Source.actorBalanceId,
                    plan.Target.StableOrder,
                    playerHp,
                    playerHp,
                    plan.Target.CurrentHp,
                    plan.Target.CurrentHp,
                    plan.BreakApplied,
                    plan.BreakApplied,
                    "canonical-basic|break|"
                    + plan.ShellBeforeBreak.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + plan.ShellAfterBreak.ToString(
                        CultureInfo.InvariantCulture),
                    78,
                    applicationId,
                    item.itemInstanceId,
                    item.baseItemId,
                    "CANONICAL_BASIC_STAT",
                    "break",
                    plan.ShellBeforeBreak,
                    plan.ShellAfterBreak,
                    plan.BreakApplied,
                    plan.ShellBeforeBreak > 0
                        && plan.ShellAfterBreak == 0,
                    plan.Target.Source.shellBrokenStateId,
                    plan.Target.Source.shellBreakCounterWindowId);

                if (plan.ShellBeforeBreak > 0
                    && plan.ShellAfterBreak == 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorShellBroken,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        item.sourceId,
                        plan.Target.Source.actorBalanceId,
                        plan.Target.StableOrder,
                        playerHp,
                        playerHp,
                        plan.Target.CurrentHp,
                        plan.Target.CurrentHp,
                        plan.BreakApplied,
                        plan.BreakApplied,
                        plan.Target.Source.shellBrokenStateId + "|"
                        + plan.Target.Source.shellBreakCounterWindowId,
                        96,
                        applicationId,
                        item.itemInstanceId,
                        item.baseItemId,
                        "CANONICAL_BASIC_STAT",
                        "break",
                        plan.ShellBeforeBreak,
                        0,
                        plan.BreakApplied,
                        true,
                        plan.Target.Source.shellBrokenStateId,
                        plan.Target.Source.shellBreakCounterWindowId);
                }
            }

            if (plan.RequestedBreak > 0L)
            {
                EmitApplicationFeedback(
                    emitted,
                    item,
                    plan.Target,
                    applicationId,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Shell,
                    plan.RequestedBreak,
                    plan.BreakApplied);
            }

            if (plan.GuardApplied > 0L)
            {
                EmitApplicationFeedback(
                    emitted,
                    item,
                    null,
                    applicationId,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Guard,
                    plan.GuardApplied,
                    plan.GuardApplied);
            }

            if (plan.HealApplied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.PlayerHpChanged,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    item.sourceId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    -1,
                    plan.PlayerHpBefore,
                    plan.PlayerHpAfter,
                    0,
                    0,
                    plan.HealApplied,
                    plan.HealApplied,
                    "+" + plan.HealApplied.ToString(
                        CultureInfo.InvariantCulture),
                    70,
                    applicationId,
                    item.itemInstanceId,
                    item.baseItemId,
                    "CANONICAL_BASIC_STAT",
                    "heal");
            }
            if (plan.RequestedHeal > 0L)
            {
                EmitApplicationFeedback(
                    emitted,
                    item,
                    null,
                    applicationId,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Heal,
                    plan.RequestedHeal,
                    plan.HealApplied);
            }
        }

        private static string CanonicalBasicMutationSummary(
            CanonicalBasicMutationPlan plan)
        {
            if (plan == null)
            {
                return string.Empty;
            }

            return string.Join(";", new[]
            {
                plan.BreakApplied > 0
                    ? "break=" + plan.BreakApplied.ToString(
                        CultureInfo.InvariantCulture)
                    : string.Empty,
                plan.GuardApplied > 0L
                    ? "guard=" + plan.GuardApplied.ToString(
                        CultureInfo.InvariantCulture)
                    : string.Empty,
                plan.HealApplied > 0
                    ? "heal=" + plan.HealApplied.ToString(
                        CultureInfo.InvariantCulture)
                    : string.Empty
            }.Where(value => value.Length > 0));
        }

        private CanonicalStatusApplicationPlan
            BuildCanonicalStatusApplicationPlan(
                C1FormalRealtimeBattleItemFactSnapshot item,
                MutableActor authoritativeTarget,
                IEnumerable<DamageSegment> directDamageSegments)
        {
            CanonicalItemEffectDefinition effect =
                item?.canonicalDefinition?.combatEffect;
            if (item == null
                || !item.isLit
                || authoritativeTarget == null
                || effect == null
                || !effect.hasCompleteStatusSemantics
                || !EqualsOrdinal(
                    effect.triggerEventId,
                    "on_action_commit")
                || !EqualsOrdinal(effect.conditionId, "is_lit")
                || !EqualsOrdinal(
                    effect.targetSelector,
                    "effect_primary_target")
                || !EqualsOrdinal(effect.lifetimePolicy, "TIMED")
                || !EqualsOrdinal(
                    effect.reapplyPolicy,
                    "ADD_STACK_AND_REFRESH")
                || !EqualsOrdinal(effect.timeUnitKey, "MILLISECOND")
                || !EqualsOrdinal(
                    effect.tickSchedulePolicy,
                    "PRESERVE_ON_REFRESH")
                || !EqualsOrdinal(
                    effect.tickDamageFormulaId,
                    "SOURCE_RESOLVED_DAMAGE_RATIO_BASIS_POINTS")
                || !EqualsOrdinal(
                    effect.consumeRule,
                    "NO_AUTO_CONSUME")
                || !EqualsOrdinal(
                    effect.expireRule,
                    "REMOVE_ENTIRE_STATUS")
                || !EqualsOrdinal(
                    effect.cleanseRule,
                    "REMOVE_ENTIRE_STATUS")
                || effect.canCrit
                || effect.countsAsHit
                || effect.autoConsumeAtMaxStack)
            {
                return null;
            }

            DamageSegment targetSegment = (directDamageSegments
                    ?? Enumerable.Empty<DamageSegment>())
                .FirstOrDefault(segment => ReferenceEquals(
                    segment.Actor,
                    authoritativeTarget));
            if (targetSegment != null && targetSegment.After <= 0)
            {
                return null;
            }

            int tickPotency = checked((int)Math.Max(
                1L,
                (long)item.directDamage
                * effect.tickDamageRatioBasisPoints / 10000L));
            BattleStatusState existing = authoritativeTarget.Statuses
                .FirstOrDefault(status => !status.Removed
                    && EqualsOrdinal(status.StatusKey, effect.statusKey));
            long expireAtBattleTimeMs = checked(
                battleTimeMs + effect.durationUnits);
            bool addsContribution = existing == null
                || existing.StackCount < effect.stackLimit;
            bool refreshes = existing != null
                && expireAtBattleTimeMs > existing.ExpireAtBattleTimeMs;
            return new CanonicalStatusApplicationPlan(
                item,
                authoritativeTarget,
                effect,
                existing,
                tickPotency,
                expireAtBattleTimeMs,
                addsContribution,
                refreshes);
        }

        private bool CommitCanonicalStatusApplication(
            CanonicalStatusApplicationPlan plan,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (plan == null
                || !plan.HasMutation
                || plan.Target.CurrentHp <= 0)
            {
                return false;
            }

            BattleStatusState status = plan.Existing;
            bool created = status == null;
            if (created)
            {
                status = new BattleStatusState(
                    plan.Target,
                    plan.Effect,
                    checked(battleTimeMs
                        + plan.Effect.firstTickDelayUnits),
                    plan.ExpireAtBattleTimeMs);
                plan.Target.Statuses.Add(status);
                status.TickAction = ScheduleStatusAction(
                    status,
                    C1FormalRealtimeBattleScheduledActionKinds.StatusTick,
                    status.NextTickAtBattleTimeMs);
                status.ExpireAction = ScheduleStatusAction(
                    status,
                    C1FormalRealtimeBattleScheduledActionKinds.StatusExpire,
                    status.ExpireAtBattleTimeMs);
            }

            if (plan.AddsContribution)
            {
                status.Contributions.Add(new BattleStatusContribution(
                    plan.Item.itemInstanceId,
                    plan.Item.baseItemId,
                    plan.TickPotency,
                    battleTimeMs));
            }

            if (!created && plan.Refreshes)
            {
                long before = status.ExpireAtBattleTimeMs;
                status.ExpireAtBattleTimeMs = plan.ExpireAtBattleTimeMs;
                if (status.ExpireAction == null
                    || !status.ExpireAction.TryReschedule(
                        before,
                        status.ExpireAtBattleTimeMs))
                {
                    status.ExpireAction = ScheduleStatusAction(
                        status,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .StatusExpire,
                        status.ExpireAtBattleTimeMs);
                }
            }

            Emit(
                emitted,
                created
                    ? C1FormalRealtimeBattleCueKinds.StatusApplied
                    : C1FormalRealtimeBattleCueKinds.StatusRefreshed,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                plan.Item.sourceId,
                plan.Target.Source.actorBalanceId,
                plan.Target.StableOrder,
                playerHp,
                playerHp,
                plan.Target.CurrentHp,
                plan.Target.CurrentHp,
                plan.TickPotency,
                plan.AddsContribution ? 1 : 0,
                status.StatusKey + "|stack="
                + status.StackCount.ToString(CultureInfo.InvariantCulture)
                + "|expire=" + status.ExpireAtBattleTimeMs.ToString(
                    CultureInfo.InvariantCulture),
                76,
                applicationId,
                plan.Item.itemInstanceId,
                plan.Item.baseItemId,
                status.StatusFamilyKey,
                status.StatusKey,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind: created
                    ? C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply
                    : C1FormalRealtimeBattleFeedbackResultKinds.DebuffRefresh);
            return true;
        }

        private MutableAction ScheduleStatusAction(
            BattleStatusState status,
            string actionKind,
            long dueBattleTimeMs)
        {
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                actionKind,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                status.RuntimeId,
                0,
                0L,
                status.Target.StableOrder,
                false,
                false,
                string.Empty,
                string.Empty);
            AddAction(action);
            return action;
        }

        private BattleStatusState FindStatus(string runtimeId)
        {
            return actors.SelectMany(actor => actor.Statuses)
                .FirstOrDefault(status => !status.Removed
                    && EqualsOrdinal(status.RuntimeId, runtimeId));
        }

        private static string CanonicalMutationSummary(
            CanonicalBasicMutationPlan basicPlan,
            CanonicalStatusApplicationPlan statusPlan)
        {
            return string.Join(";", new[]
            {
                CanonicalBasicMutationSummary(basicPlan),
                statusPlan != null && statusPlan.HasMutation
                    ? "status=" + statusPlan.Effect.statusKey
                    : string.Empty
            }.Where(value => value.Length > 0));
        }

        private void ProcessStatusTick(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            BattleStatusState status = FindStatus(action.SourceId);
            if (status == null || status.Target.CurrentHp <= 0)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    string.Empty);
                return;
            }

            string applicationId = NewApplicationId();
            acceptedApplicationEventIds.Add(applicationId);
            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                applicationId);
            status.TickAction = null;
            BattleStatusContribution[] contributions = status.Contributions
                .OrderBy(value => value.AppliedAtBattleTimeMs)
                .ThenBy(value => value.SourceItemInstanceId,
                    StringComparer.Ordinal)
                .ToArray();
            foreach (BattleStatusContribution contribution in contributions)
            {
                if (status.Target.CurrentHp <= 0)
                {
                    break;
                }

                CanonicalEffectSource tickModifier;
                int requestedTickDamage = ResolveCanonicalStatusTickDamage(
                    status,
                    contribution,
                    out tickModifier);
                DamageSegment segment = BuildTargetedDamageSegment(
                    null,
                    status.Target,
                    requestedTickDamage,
                    false);
                int totalStateApplied = checked(
                    segment.TotalShellApplied + segment.Applied);
                if (totalStateApplied <= 0)
                {
                    continue;
                }

                segment.Actor.CurrentShell = segment.ShellAfter;
                segment.Actor.CurrentHp = segment.After;
                if (segment.BrokeShell)
                {
                    segment.Actor.ShellBroken = true;
                }

                if (tickModifier != null)
                {
                    EmitCanonicalEffectAccepted(
                        tickModifier,
                        null,
                        segment.Actor,
                        applicationId,
                        contribution.TickPotency,
                        requestedTickDamage - contribution.TickPotency,
                        "status-tick|" + status.StatusKey,
                        emitted);
                }

                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.StatusTickAccepted,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    contribution.SourceItemInstanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    contribution.TickPotency,
                    totalStateApplied,
                    status.StatusKey + "|stack="
                    + status.StackCount.ToString(
                        CultureInfo.InvariantCulture),
                    76,
                    applicationId,
                    contribution.SourceItemInstanceId,
                    contribution.SourceBaseItemId,
                    status.StatusFamilyKey,
                    status.StatusKey,
                    segment.ShellBefore,
                    segment.ShellAfter,
                    segment.TotalShellApplied,
                    segment.BrokeShell,
                    segment.BrokeShell
                        ? segment.Actor.Source.shellBrokenStateId
                        : string.Empty,
                    segment.BrokeShell
                        ? segment.Actor.Source.shellBreakCounterWindowId
                        : string.Empty,
                    triggerKind:
                        C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger,
                    deliveryKind:
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic);
                if (segment.BaseShellApplied > 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        contribution.SourceItemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        segment.Before,
                        contribution.TickPotency,
                        segment.BaseShellApplied,
                        "status|" + status.StatusKey + "|shell|"
                        + segment.ShellBefore.ToString(
                            CultureInfo.InvariantCulture) + ">"
                        + segment.ShellAfter.ToString(
                            CultureInfo.InvariantCulture),
                        78,
                        applicationId,
                        contribution.SourceItemInstanceId,
                        contribution.SourceBaseItemId,
                        status.StatusFamilyKey,
                        status.StatusKey,
                        segment.ShellBefore,
                        segment.ShellAfter,
                        segment.BaseShellApplied,
                        segment.BrokeShell,
                        segment.BrokeShell
                            ? segment.Actor.Source.shellBrokenStateId
                            : string.Empty,
                        segment.BrokeShell
                            ? segment.Actor.Source.shellBreakCounterWindowId
                            : string.Empty,
                        triggerKind:
                            C1FormalRealtimeBattleFeedbackTriggerKinds
                                .StatusTrigger,
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds
                                .ShellDamage);
                }

                if (segment.BrokeShell)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorShellBroken,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        contribution.SourceItemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        segment.Before,
                        contribution.TickPotency,
                        segment.TotalShellApplied,
                        status.StatusKey,
                        96,
                        applicationId,
                        contribution.SourceItemInstanceId,
                        contribution.SourceBaseItemId,
                        status.StatusFamilyKey,
                        status.StatusKey,
                        segment.ShellBefore,
                        0,
                        segment.TotalShellApplied,
                        true,
                        segment.Actor.Source.shellBrokenStateId,
                        segment.Actor.Source.shellBreakCounterWindowId,
                        triggerKind:
                            C1FormalRealtimeBattleFeedbackTriggerKinds
                                .StatusTrigger,
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds
                                .ShellDamage);
                }

                if (segment.Applied > 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        contribution.SourceItemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        segment.After,
                        contribution.TickPotency,
                        segment.Applied,
                        status.StatusKey,
                        70,
                        applicationId,
                        contribution.SourceItemInstanceId,
                        contribution.SourceBaseItemId,
                        status.StatusFamilyKey,
                        status.StatusKey,
                        triggerKind:
                            C1FormalRealtimeBattleFeedbackTriggerKinds
                                .StatusTrigger,
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                        C1FormalRealtimeBattleSessionContract.PlayerOwner,
                        contribution.SourceItemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        segment.After,
                        contribution.TickPotency,
                        segment.Applied,
                        "-" + segment.Applied.ToString(
                            CultureInfo.InvariantCulture),
                        65,
                        applicationId,
                        contribution.SourceItemInstanceId,
                        contribution.SourceBaseItemId,
                        status.StatusFamilyKey,
                        status.StatusKey,
                        triggerKind:
                            C1FormalRealtimeBattleFeedbackTriggerKinds
                                .StatusTrigger,
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
                }

                if (segment.After <= 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        contribution.SourceItemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        0,
                        contribution.TickPotency,
                        segment.Applied,
                        status.StatusKey,
                        95,
                        applicationId,
                        contribution.SourceItemInstanceId,
                        contribution.SourceBaseItemId,
                        status.StatusFamilyKey,
                        status.StatusKey);
                    CancelEnemyActionsForDefeatedActor(
                        segment.Actor,
                        applicationId);
                    RemoveAllStatuses(
                        segment.Actor,
                        "TARGET_DEFEATED",
                        applicationId,
                        emitted);
                }
            }

            if (TotalEnemyHp() <= 0)
            {
                CompleteTerminal(true, emitted);
                return;
            }

            if (!status.Removed)
            {
                long nextTick = checked(
                    battleTimeMs + status.TickIntervalMs);
                status.NextTickAtBattleTimeMs = nextTick;
                if (nextTick <= status.ExpireAtBattleTimeMs)
                {
                    status.TickAction = ScheduleStatusAction(
                        status,
                        C1FormalRealtimeBattleScheduledActionKinds.StatusTick,
                        nextTick);
                }
            }
        }

        private void ProcessStatusExpire(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            BattleStatusState status = FindStatus(action.SourceId);
            if (status == null)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    string.Empty);
                return;
            }

            string applicationId = NewApplicationId();
            acceptedApplicationEventIds.Add(applicationId);
            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                applicationId);
            status.ExpireAction = null;
            ApplyCanonicalStatusExpiredEffects(
                status,
                applicationId,
                emitted);
            RemoveStatus(status, "EXPIRED", applicationId, emitted);
        }

        private void RemoveAllStatuses(
            MutableActor actor,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (BattleStatusState status in actor.Statuses
                         .Where(value => !value.Removed)
                         .ToArray())
            {
                RemoveStatus(status, reason, applicationId, emitted);
            }
        }

        private void RemoveStatus(
            BattleStatusState status,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (status == null || status.Removed)
            {
                return;
            }

            BattleStatusContribution source = status.Contributions
                .OrderBy(value => value.AppliedAtBattleTimeMs)
                .ThenBy(value => value.SourceItemInstanceId,
                    StringComparer.Ordinal)
                .FirstOrDefault();
            status.Removed = true;
            if (status.TickAction != null && !status.TickAction.Executed)
            {
                status.TickAction.MarkExecuted(
                    false,
                    "STATUS_REMOVED",
                    applicationId);
            }
            if (status.ExpireAction != null
                && !status.ExpireAction.Executed)
            {
                status.ExpireAction.MarkExecuted(
                    false,
                    "STATUS_REMOVED",
                    applicationId);
            }

            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.StatusRemoved,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                source == null
                    ? status.RuntimeId
                    : source.SourceItemInstanceId,
                status.Target.Source.actorBalanceId,
                status.Target.StableOrder,
                playerHp,
                playerHp,
                status.Target.CurrentHp,
                status.Target.CurrentHp,
                status.StackCount,
                status.StackCount,
                status.StatusKey + "|" + reason,
                74,
                applicationId,
                source == null
                    ? string.Empty
                    : source.SourceItemInstanceId,
                source == null ? string.Empty : source.SourceBaseItemId,
                status.StatusFamilyKey,
                status.StatusKey);
            status.Target.Statuses.Remove(status);
        }

        private bool TryCleanseStatus(
            MutableActor actor,
            string statusKey,
            string sourceItemInstanceId,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            BattleStatusState status = actor?.Statuses.FirstOrDefault(
                value => !value.Removed
                    && EqualsOrdinal(value.StatusKey, statusKey));
            if (status == null
                || !status.Effect.EffectTags.Contains(
                    "Cleanseable",
                    StringComparer.Ordinal))
            {
                return false;
            }

            RemoveStatus(
                status,
                "CLEANSED_BY:" + (sourceItemInstanceId ?? string.Empty),
                applicationId,
                emitted);
            return true;
        }

        private List<DamageSegment> BuildDirectDamageSegments(
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor authoritativeTarget,
            out int totalBaseApplied)
        {
            int remaining = item.directDamage;
            totalBaseApplied = 0;
            List<DamageSegment> segments = new List<DamageSegment>();
            foreach (MutableActor actor in actors)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (actor.CurrentHp <= 0)
                {
                    continue;
                }

                DamageSegment segment = BuildTargetedDamageSegment(
                    item,
                    actor,
                    remaining,
                    ReferenceEquals(actor, authoritativeTarget));
                remaining -= checked(
                    segment.BaseShellApplied + segment.Applied);
                totalBaseApplied = checked(
                    totalBaseApplied + segment.BaseShellApplied
                    + segment.Applied);
                segments.Add(segment);
            }

            return segments;
        }

        private DamageSegment BuildTargetedDamageSegment(
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor actor,
            int requestedDamage,
            bool includeShellModifiers)
        {
            int remaining = requestedDamage;
            int shellBefore = actor.CurrentShell;
            int baseShellApplied = Math.Min(shellBefore, remaining);
            int shellAfterBase = shellBefore - baseShellApplied;
            remaining -= baseShellApplied;
            List<ShellModifierContribution> shellModifiers =
                includeShellModifiers
                    ? BuildShellModifierContributions(
                        actor,
                        baseShellApplied,
                        shellAfterBase)
                    : new List<ShellModifierContribution>();
            int shellAfter = shellModifiers.Count == 0
                ? shellAfterBase
                : shellModifiers[shellModifiers.Count - 1].ShellAfter;
            int before = actor.CurrentHp;
            int applied = Math.Min(before, remaining);
            return new DamageSegment(
                actor,
                before,
                before - applied,
                applied,
                shellBefore,
                shellAfterBase,
                shellAfter,
                baseShellApplied,
                shellModifiers);
        }

        private C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[]
            PendingExtraTargetSources(
                C1FormalRealtimeBattleItemFactSnapshot item)
        {
            if (item?.actionCostFact == null
                || !EqualsOrdinal(
                    item.actionCostFact.actionDefinitionId,
                    C1Lv1FormalItemActionCostCatalog.ActionDefinitionId))
            {
                return Array.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>();
            }

            return activePool15ItemFacts
                .Where(IsFormalExtraTargetFact)
                .Where(fact => !automaticPool15TriggerIds.Contains(
                    AutomaticPool15SourceTriggerKey(fact.triggerId, fact)))
                .OrderBy(fact => fact.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(
                    fact => fact.sourceFactCanonicalSignature,
                    StringComparer.Ordinal)
                .ToArray();
        }

        private List<ExtraTargetAssignment> BuildExtraTargetAssignments(
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor primaryTarget,
            IEnumerable<DamageSegment> primarySegments,
            IEnumerable<
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot> sources)
        {
            List<ExtraTargetAssignment> assignments =
                new List<ExtraTargetAssignment>();
            HashSet<MutableActor> alreadyHit = new HashSet<MutableActor>(
                (primarySegments ?? Enumerable.Empty<DamageSegment>())
                    .Select(segment => segment.Actor));
            alreadyHit.Add(primaryTarget);
            List<MutableActor> candidates = actors
                .Where(actor => actor.CurrentHp > 0
                    && !alreadyHit.Contains(actor))
                .OrderBy(actor => Math.Abs(
                    actor.StableOrder - primaryTarget.StableOrder))
                .ThenBy(actor => actor.StableOrder)
                .ToList();
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     sources ?? Enumerable.Empty<
                         C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>())
            {
                C1FormalRealtimeBattleCombatComponentSnapshot component =
                    fact.components.Single(value => value != null
                        && EqualsOrdinal(value.operationId, "ExtraTarget")
                        && EqualsOrdinal(value.targetStatId, "targetCount")
                        && EqualsOrdinal(value.nativeUnitId, "count"));
                long boundedTargetCount = Math.Min(
                    component.signedAmount,
                    component.absoluteCapPerAcceptedTrigger);
                int targetCount = checked((int)Math.Min(
                    boundedTargetCount,
                    candidates.Count));
                for (int index = 0; index < targetCount; index++)
                {
                    MutableActor target = candidates[0];
                    candidates.RemoveAt(0);
                    DamageSegment segment = BuildTargetedDamageSegment(
                        item,
                        target,
                        item.directDamage,
                        true);
                    if (segment.BaseShellApplied + segment.Applied <= 0)
                    {
                        continue;
                    }

                    assignments.Add(new ExtraTargetAssignment(
                        fact,
                        component,
                        segment));
                }
            }

            return assignments;
        }

        private List<ShellModifierContribution>
            BuildShellModifierContributions(
                MutableActor target,
                int baseShellPortion,
                int shellAfterBase)
        {
            List<ShellModifierContribution> contributions =
                new List<ShellModifierContribution>();
            if (target == null
                || !target.Source.hasShell
                || target.ShellBroken
                || baseShellPortion <= 0
                || shellAfterBase <= 0)
            {
                return contributions;
            }

            long cumulativeBasisPoints = 0L;
            long priorRoundedExtra = 0L;
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     activePool15ItemFacts
                         .Where(IsFormalShellBreakModifierFact)
                         .OrderBy(
                             value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .ThenBy(
                             value => value.sourceFactCanonicalSignature,
                             StringComparer.Ordinal))
            {
                C1FormalRealtimeBattleCombatComponentSnapshot component =
                    fact.components.Single(value => value != null
                        && EqualsOrdinal(value.operationId, "AddPercent")
                        && EqualsOrdinal(value.targetStatId, "break")
                        && EqualsOrdinal(value.nativeUnitId, "basisPoint"));
                long requestedBasisPoints = component.signedAmount;
                long cappedBasisPoints = component.absoluteCapPerAcceptedTrigger
                    > 0L
                    ? Math.Min(
                        requestedBasisPoints,
                        component.absoluteCapPerAcceptedTrigger)
                    : requestedBasisPoints;
                cumulativeBasisPoints = checked(
                    cumulativeBasisPoints + cappedBasisPoints);
                if (!C1CampaignLootPool15BattleFixedPoint.TryApplyBasisPoints(
                        baseShellPortion,
                        cumulativeBasisPoints,
                        out long roundedExtra))
                {
                    continue;
                }

                long requestedContribution = Math.Max(
                    0L,
                    roundedExtra - priorRoundedExtra);
                int shellBefore = shellAfterBase - checked((int)Math.Min(
                    priorRoundedExtra,
                    shellAfterBase));
                int applied = checked((int)Math.Min(
                    requestedContribution,
                    shellBefore));
                priorRoundedExtra = roundedExtra;
                if (applied <= 0)
                {
                    continue;
                }

                contributions.Add(new ShellModifierContribution(
                    fact,
                    component,
                    baseShellPortion,
                    shellBefore,
                    shellBefore - applied,
                    applied));
            }

            return contributions;
        }

        private void CommitShellModifierContributions(
            DamageSegment segment,
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (ShellModifierContribution contribution in
                     segment.ShellModifiers)
            {
                pool15ApplicationSequence++;
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact =
                    contribution.Fact;
                C1FormalRealtimeBattleEffectApplicationSnapshot resolved =
                    new C1FormalRealtimeBattleEffectApplicationSnapshot(
                        pool15ApplicationSequence,
                        acceptedApplicationEventId + ":shell",
                        fact.itemInstanceId,
                        fact.baseItemId,
                        fact.sourceFactCanonicalSignature,
                        fact.familyId,
                        fact.familyVariantId,
                        fact.triggerId,
                        fact.activationConditionId,
                        contribution.Component.operationId,
                        contribution.Component.targetStatId,
                        fact.targetScopeId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        contribution.Component.signedAmount,
                        contribution.AppliedShell,
                        contribution.Component.nativeUnitId,
                        contribution.PercentageBase,
                        contribution.ShellBefore,
                        contribution.ShellAfter,
                        string.Empty,
                        fact.maturity,
                        fact.sourceProfileCanonicalSignature,
                        battleTimeMs,
                        battleTimeMs,
                        fact.cueIdentity,
                        fact.combatFactCanonicalSignature);
                pool15ApplicationLedger.Add(resolved);
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    fact.itemInstanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    ToCueAmount(contribution.Component.signedAmount),
                    contribution.AppliedShell,
                    fact.baseItemId + "|shell-break|"
                    + contribution.ShellBefore.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + contribution.ShellAfter.ToString(
                        CultureInfo.InvariantCulture),
                    81,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId,
                    contribution.ShellBefore,
                    contribution.ShellAfter,
                    contribution.AppliedShell,
                    false,
                    string.Empty,
                    string.Empty);
                pool15CueLedger.Add(
                    new C1FormalRealtimeBattleEffectCue(cueSequence, resolved));
            }
        }

        private void EmitExtraTargetApplication(
            ExtraTargetAssignment assignment,
            C1FormalRealtimeBattleItemFactSnapshot directItem,
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact =
                assignment.Fact;
            DamageSegment segment = assignment.Segment;

            pool15ApplicationSequence++;
            long targetBefore = checked(
                (long)segment.Before + segment.ShellBefore);
            long targetAfterBase = checked(
                (long)segment.After + segment.ShellAfterBase);
            C1FormalRealtimeBattleEffectApplicationSnapshot resolved =
                new C1FormalRealtimeBattleEffectApplicationSnapshot(
                    pool15ApplicationSequence,
                    acceptedApplicationEventId + ":extra-target:"
                    + fact.itemInstanceId + ":"
                    + segment.Actor.StableOrder.ToString(
                        CultureInfo.InvariantCulture),
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.sourceFactCanonicalSignature,
                    fact.familyId,
                    fact.familyVariantId,
                    fact.triggerId,
                    fact.activationConditionId,
                    assignment.Component.operationId,
                    assignment.Component.targetStatId,
                    fact.targetScopeId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    assignment.Component.signedAmount,
                    1L,
                    assignment.Component.nativeUnitId,
                    0L,
                    targetBefore,
                    targetAfterBase,
                    string.Empty,
                    fact.maturity,
                    fact.sourceProfileCanonicalSignature,
                    battleTimeMs,
                    battleTimeMs,
                    fact.cueIdentity,
                    fact.combatFactCanonicalSignature);
            pool15ApplicationLedger.Add(resolved);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                fact.itemInstanceId,
                segment.Actor.Source.actorBalanceId,
                segment.Actor.StableOrder,
                playerHp,
                playerHp,
                segment.Before,
                segment.After,
                ToCueAmount(assignment.Component.signedAmount),
                1,
                fact.baseItemId + "|extra-target|baseDamage="
                + directItem.directDamage.ToString(
                    CultureInfo.InvariantCulture),
                81,
                acceptedApplicationEventId,
                fact.itemInstanceId,
                fact.baseItemId,
                fact.familyId,
                fact.familyVariantId,
                segment.ShellBefore,
                segment.ShellAfterBase,
                segment.BaseShellApplied,
                false,
                string.Empty,
                string.Empty);
            pool15CueLedger.Add(
                new C1FormalRealtimeBattleEffectCue(cueSequence, resolved));

            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.HitAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                fact.itemInstanceId,
                segment.Actor.Source.actorBalanceId,
                segment.Actor.StableOrder,
                playerHp,
                playerHp,
                segment.Before,
                segment.After,
                directItem.directDamage,
                segment.Applied,
                string.Empty,
                75,
                acceptedApplicationEventId,
                fact.itemInstanceId,
                fact.baseItemId,
                fact.familyId,
                fact.familyVariantId,
                segment.ShellBefore,
                segment.ShellAfter,
                segment.TotalShellApplied,
                false,
                string.Empty,
                string.Empty);
            if (segment.BaseShellApplied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    fact.itemInstanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.Before,
                    directItem.directDamage,
                    segment.BaseShellApplied,
                    "shell|" + segment.ShellBefore.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + segment.ShellAfterBase.ToString(
                        CultureInfo.InvariantCulture),
                    78,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId,
                    segment.ShellBefore,
                    segment.ShellAfterBase,
                    segment.BaseShellApplied,
                    false,
                    string.Empty,
                    string.Empty);
            }

            CommitShellModifierContributions(
                segment,
                acceptedApplicationEventId,
                emitted);
            if (segment.BrokeShell)
            {
                EmitShellBroken(
                    segment,
                    directItem,
                    fact,
                    acceptedApplicationEventId,
                    emitted);
            }

            if (segment.Applied > 0)
            {
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    fact.itemInstanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    directItem.directDamage,
                    segment.Applied,
                    string.Empty,
                    70,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId);
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    fact.itemInstanceId,
                    segment.Actor.Source.actorBalanceId,
                    segment.Actor.StableOrder,
                    playerHp,
                    playerHp,
                    segment.Before,
                    segment.After,
                    directItem.directDamage,
                    segment.Applied,
                    "-" + segment.Applied.ToString(
                        CultureInfo.InvariantCulture),
                    65,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId);
                if (segment.After <= 0)
                {
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        fact.itemInstanceId,
                        segment.Actor.Source.actorBalanceId,
                        segment.Actor.StableOrder,
                        playerHp,
                        playerHp,
                        segment.Before,
                        0,
                        directItem.directDamage,
                        segment.Applied,
                        string.Empty,
                        95,
                        acceptedApplicationEventId,
                        fact.itemInstanceId,
                        fact.baseItemId,
                        fact.familyId,
                        fact.familyVariantId);
                    CancelEnemyActionsForDefeatedActor(
                        segment.Actor,
                        acceptedApplicationEventId);
                    RemoveAllStatuses(
                        segment.Actor,
                        "TARGET_DEFEATED",
                        acceptedApplicationEventId,
                        emitted);
                }
            }

            if (segment.After > 0 && segment.Applied > 0)
            {
                TriggerEnemySkills(
                    segment.Actor,
                    C1FormalEnemySkillTriggerTypes.OnDamaged,
                    segment.Applied,
                    acceptedApplicationEventId,
                    emitted);
            }
        }

        private void EmitShellBroken(
            DamageSegment segment,
            C1FormalRealtimeBattleItemFactSnapshot directItem,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
                directPool15Origin,
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            ShellModifierContribution modifier = segment.ShellModifiers
                .LastOrDefault(value => value.ShellAfter == 0);
            string sourceId = modifier == null
                ? directPool15Origin == null
                    ? directItem.sourceId
                    : directPool15Origin.itemInstanceId
                : modifier.Fact.itemInstanceId;
            string sourceItemInstanceId = modifier == null
                ? directPool15Origin == null
                    ? directItem.itemInstanceId
                    : directPool15Origin.itemInstanceId
                : modifier.Fact.itemInstanceId;
            string sourceBaseItemId = modifier == null
                ? directPool15Origin == null
                    ? directItem.baseItemId
                    : directPool15Origin.baseItemId
                : modifier.Fact.baseItemId;
            string familyId = modifier == null
                ? directPool15Origin == null
                    ? string.Empty
                    : directPool15Origin.familyId
                : modifier.Fact.familyId;
            string variantId = modifier == null
                ? directPool15Origin == null
                    ? string.Empty
                    : directPool15Origin.familyVariantId
                : modifier.Fact.familyVariantId;
            int before = modifier == null
                ? segment.ShellBefore
                : modifier.ShellBefore;
            int applied = modifier == null
                ? segment.BaseShellApplied
                : modifier.AppliedShell;
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.ActorShellBroken,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                sourceId,
                segment.Actor.Source.actorBalanceId,
                segment.Actor.StableOrder,
                playerHp,
                playerHp,
                segment.Before,
                segment.After,
                applied,
                applied,
                segment.Actor.Source.shellBrokenStateId + "|"
                + segment.Actor.Source.shellBreakCounterWindowId,
                96,
                acceptedApplicationEventId,
                sourceItemInstanceId,
                sourceBaseItemId,
                familyId,
                variantId,
                before,
                0,
                applied,
                true,
                segment.Actor.Source.shellBrokenStateId,
                segment.Actor.Source.shellBreakCounterWindowId);
        }

        private bool TryCommitNianSpend(
            C1FormalRealtimeBattleItemFactSnapshot item,
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            C1FormalRealtimeBattleItemActionCostSnapshot cost =
                item == null ? null : item.actionCostFact;
            if (nianCapacity == null
                || cost == null
                || !EqualsOrdinal(cost.resourceKey, nianCapacity.resourceKey)
                || cost.cost <= 0)
            {
                return false;
            }

            CanonicalEffectSource reductionSource;
            int effectiveCost = ResolveCanonicalNianCost(
                item,
                cost.cost,
                out reductionSource);
            if (currentNian < effectiveCost)
            {
                return false;
            }

            if (effectiveCost == 0)
            {
                EmitCanonicalEffectAccepted(
                    reductionSource,
                    item,
                    null,
                    acceptedApplicationEventId,
                    cost.cost,
                    cost.cost,
                    "nian-cost|" + cost.cost.ToString(
                        CultureInfo.InvariantCulture) + ">0",
                    emitted);
                return true;
            }

            int before = currentNian;
            int after = before - effectiveCost;
            C1FormalRealtimeBattleNianTransactionSnapshot transaction =
                NewNianTransaction(
                    item.itemInstanceId,
                    item.baseItemId,
                    acceptedApplicationEventId,
                    C1FormalRealtimeBattleNianTransactionKinds.Spend,
                    effectiveCost,
                    effectiveCost,
                    before,
                    after);
            currentNian = after;
            nianTransactionLedger.Add(transaction);
            EmitNianTransaction(emitted, transaction, string.Empty, string.Empty);
            if (reductionSource != null)
            {
                EmitCanonicalEffectAccepted(
                    reductionSource,
                    item,
                    null,
                    acceptedApplicationEventId,
                    cost.cost,
                    cost.cost - effectiveCost,
                    "nian-cost|" + cost.cost.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + effectiveCost.ToString(CultureInfo.InvariantCulture),
                    emitted);
            }
            return true;
        }

        private void ApplyQualifyingI002NianRefunds(
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            IEnumerable<C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
                sources = activeItemFacts
                    .Where(item => item.combatFact != null)
                    .Select(item => item.combatFact)
                    .Concat(activePool15ItemFacts)
                    .GroupBy(fact => fact.itemInstanceId, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .Where(IsFormalNianRefundFact)
                    .OrderBy(fact => fact.itemInstanceId, StringComparer.Ordinal);
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     sources)
            {
                string triggerKey = AutomaticPool15SourceTriggerKey(
                    fact.triggerId,
                    fact) + "|" + acceptedApplicationEventId;
                if (automaticPool15TriggerIds.Contains(triggerKey))
                {
                    continue;
                }

                C1FormalRealtimeBattleCombatComponentSnapshot component =
                    fact.components.Single(value =>
                        EqualsOrdinal(value.operationId, "Refund")
                        && EqualsOrdinal(value.targetStatId, "nian")
                        && EqualsOrdinal(value.nativeUnitId, "point"));
                int requested = checked((int)Math.Min(
                    component.signedAmount,
                    component.absoluteCapPerAcceptedTrigger));
                int applied = Math.Min(
                    requested,
                    Math.Max(0, nianCapacity.maxNian - currentNian));
                automaticPool15TriggerIds.Add(triggerKey);
                if (applied <= 0)
                {
                    continue;
                }

                int before = currentNian;
                int after = before + applied;
                C1FormalRealtimeBattleNianTransactionSnapshot transaction =
                    NewNianTransaction(
                        fact.itemInstanceId,
                        fact.baseItemId,
                        acceptedApplicationEventId,
                        C1FormalRealtimeBattleNianTransactionKinds.Refund,
                        requested,
                        applied,
                        before,
                        after);
                currentNian = after;
                nianTransactionLedger.Add(transaction);
                EmitNianTransaction(
                    emitted,
                    transaction,
                    fact.familyId,
                    fact.familyVariantId);
            }
        }

        private void ApplyFormalEnemyCastDelays(
            string acceptedApplicationEventId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     activePool15ItemFacts
                         .Where(IsFormalEnemyCastDelayFact)
                         .OrderBy(
                             value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .ThenBy(
                             value => value.sourceFactCanonicalSignature,
                             StringComparer.Ordinal))
            {
                string triggerKey = AutomaticPool15SourceTriggerKey(
                    fact.triggerId,
                    fact) + "|" + acceptedApplicationEventId;
                if (automaticPool15TriggerIds.Contains(triggerKey))
                {
                    continue;
                }

                MutableAction target = actions.Where(action =>
                        !action.Executed
                        && action.DueBattleTimeMs > battleTimeMs
                        && IsCastProgressCapable(action)
                        && action.IntervalMs > 0L)
                    .OrderBy(action => action.DueBattleTimeMs)
                    .ThenBy(action => action.SourceStableOrder)
                    .ThenBy(action => action.StableOrder)
                    .FirstOrDefault();
                if (target == null)
                {
                    continue;
                }

                C1FormalRealtimeBattleCombatComponentSnapshot component =
                    fact.components.Single(value => value != null
                        && EqualsOrdinal(value.operationId, "ReduceFlat")
                        && EqualsOrdinal(value.targetStatId, "castProgress")
                        && EqualsOrdinal(value.nativeUnitId, "turn"));
                int requestedStepCount = checked((int)Math.Min(
                    Math.Abs(component.signedAmount),
                    component.absoluteCapPerAcceptedTrigger));
                long beforeDueBattleTimeMs = target.DueBattleTimeMs;
                if (requestedStepCount != 1
                    || pool15ApplicationSequence == long.MaxValue
                    || !TryResolveDiscreteProgressDueTime(
                        C1FormalRealtimeBattleActionProgressOperatorIds
                            .DelayEnemyCastProgressStep,
                        "turn",
                        battleTimeMs,
                        beforeDueBattleTimeMs,
                        target.IntervalMs,
                        out long afterDueBattleTimeMs))
                {
                    continue;
                }

                long nextSequence = pool15ApplicationSequence + 1L;
                string eventId = acceptedApplicationEventId
                    + ":enemy-cast-delay:" + fact.itemInstanceId;
                C1FormalRealtimeBattleEffectApplicationSnapshot application =
                    new C1FormalRealtimeBattleEffectApplicationSnapshot(
                        nextSequence,
                        eventId,
                        fact.itemInstanceId,
                        fact.baseItemId,
                        fact.sourceFactCanonicalSignature,
                        fact.familyId,
                        fact.familyVariantId,
                        fact.triggerId,
                        fact.activationConditionId,
                        component.operationId,
                        component.targetStatId,
                        fact.targetScopeId,
                        target.SourceId,
                        target.StableOrder,
                        requestedStepCount,
                        requestedStepCount,
                        component.nativeUnitId,
                        0L,
                        beforeDueBattleTimeMs,
                        afterDueBattleTimeMs,
                        string.Empty,
                        fact.maturity,
                        fact.sourceProfileCanonicalSignature,
                        battleTimeMs,
                        battleTimeMs,
                        fact.cueIdentity,
                        fact.combatFactCanonicalSignature);
                C1FormalRealtimeBattleActionProgressMutationSnapshot mutation =
                    new C1FormalRealtimeBattleActionProgressMutationSnapshot(
                        C1FormalRealtimeBattleActionProgressOperatorIds
                            .DelayEnemyCastProgressStep,
                        eventId,
                        fact.itemInstanceId,
                        target.ActionId,
                        target.ActionKind,
                        target.SourceId,
                        component.nativeUnitId,
                        requestedStepCount,
                        requestedStepCount,
                        beforeDueBattleTimeMs,
                        afterDueBattleTimeMs,
                        target.IntervalMs,
                        string.Empty);
                if (!target.TryReschedule(
                        beforeDueBattleTimeMs,
                        afterDueBattleTimeMs))
                {
                    continue;
                }

                automaticPool15TriggerIds.Add(triggerKey);
                pool15ApplicationSequence = nextSequence;
                pool15ApplicationLedger.Add(application);
                actionProgressMutationLedger.Add(mutation);
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.ActionProgressMutated,
                    C1FormalRealtimeBattleSessionContract.BattleOwner,
                    fact.itemInstanceId,
                    target.ActionId,
                    target.StableOrder,
                    playerHp,
                    playerHp,
                    0,
                    0,
                    requestedStepCount,
                    requestedStepCount,
                    mutation.operatorId + "|" + mutation.targetActionKind
                    + "|" + beforeDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + afterDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture) + "|interval="
                    + target.IntervalMs.ToString(
                        CultureInfo.InvariantCulture) + "|turn|"
                    + mutation.canonicalSignature,
                    82,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId);
                actionProgressCueLedger.Add(cueLedger[cueLedger.Count - 1]);
                Emit(
                    emitted,
                    C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    fact.itemInstanceId,
                    target.ActionId,
                    target.StableOrder,
                    playerHp,
                    playerHp,
                    0,
                    0,
                    requestedStepCount,
                    requestedStepCount,
                    fact.baseItemId + "|enemy-cast-delay|"
                    + beforeDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture) + ">"
                    + afterDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture),
                    80,
                    acceptedApplicationEventId,
                    fact.itemInstanceId,
                    fact.baseItemId,
                    fact.familyId,
                    fact.familyVariantId);
                pool15CueLedger.Add(
                    new C1FormalRealtimeBattleEffectCue(
                        cueSequence,
                        application));
            }
        }

        private C1FormalRealtimeBattleNianTransactionSnapshot NewNianTransaction(
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string acceptedApplicationEventId,
            string transactionKind,
            int requestedAmount,
            int appliedAmount,
            int before,
            int after)
        {
            nianTransactionSequence++;
            return new C1FormalRealtimeBattleNianTransactionSnapshot(
                nianTransactionSequence,
                request.sessionId,
                sessionGeneration,
                currentResetGeneration,
                sourceItemInstanceId,
                sourceBaseItemId,
                acceptedApplicationEventId,
                battleTimeMs,
                transactionKind,
                requestedAmount,
                appliedAmount,
                before,
                after);
        }

        private static bool IsFormalNianRefundFact(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact)
        {
            return fact != null
                && fact.HasImmutableCombatFact
                && EqualsOrdinal(fact.triggerId, "on_consecutive_trigger")
                && EqualsOrdinal(
                    fact.activationConditionId,
                    "within_2_turns")
                && fact.components.Count(component => component != null
                    && EqualsOrdinal(component.operationId, "Refund")
                    && EqualsOrdinal(component.targetStatId, "nian")
                    && EqualsOrdinal(component.nativeUnitId, "point")
                    && component.signedAmount > 0L
                    && component.absoluteCapPerAcceptedTrigger > 0L) == 1;
        }

        private static bool IsFormalShellBreakModifierFact(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact)
        {
            return fact != null
                && fact.HasImmutableCombatFact
                && EqualsOrdinal(fact.baseItemId, "I003")
                && EqualsOrdinal(fact.triggerId, "on_hit")
                && EqualsOrdinal(
                    fact.activationConditionId,
                    "target_has_shield")
                && fact.components.Count(component => component != null
                    && EqualsOrdinal(component.operationId, "AddPercent")
                    && EqualsOrdinal(component.targetStatId, "break")
                    && EqualsOrdinal(component.nativeUnitId, "basisPoint")
                    && component.signedAmount > 0L
                    && component.absoluteCapPerAcceptedTrigger > 0L) == 1;
        }

        private static bool IsFormalExtraTargetFact(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact)
        {
            return fact != null
                && fact.HasImmutableCombatFact
                && EqualsOrdinal(fact.baseItemId, "I004")
                && EqualsOrdinal(fact.triggerId, "on_direct_lit")
                && EqualsOrdinal(
                    fact.activationConditionId,
                    "next_zhenlei_trigger")
                && fact.components.Count(component => component != null
                    && EqualsOrdinal(component.operationId, "ExtraTarget")
                    && EqualsOrdinal(component.targetStatId, "targetCount")
                    && EqualsOrdinal(component.nativeUnitId, "count")
                    && component.signedAmount > 0L
                    && component.absoluteCapPerAcceptedTrigger > 0L) == 1;
        }

        private static bool IsFormalEnemyCastDelayFact(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact)
        {
            return fact != null
                && fact.HasImmutableCombatFact
                && EqualsOrdinal(fact.baseItemId, "I025")
                && EqualsOrdinal(fact.triggerId, "on_hit")
                && EqualsOrdinal(
                    fact.activationConditionId,
                    "target_casting")
                && fact.components.Count(component => component != null
                    && EqualsOrdinal(component.operationId, "ReduceFlat")
                    && EqualsOrdinal(component.targetStatId, "castProgress")
                    && EqualsOrdinal(component.nativeUnitId, "turn")
                    && component.signedAmount == -1L
                    && component.absoluteCapPerAcceptedTrigger == 1L
                    && EqualsOrdinal(
                        component.dependencyId,
                        C1CampaignPool15BaseCombatFacts
                            .DelayEnemyCastProgressStepDependencyId)) == 1;
        }

        private void ProcessEnemyAction(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            MutableActor source = FindActor(
                action.SourceId,
                action.SourceStableOrder);
            if (source == null || source.CurrentHp <= 0)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleCueKinds.ActorDefeated,
                    string.Empty);
                return;
            }

            if (!ApplyEnemyDamageToPlayer(action, emitted)
                || playerHp <= 0)
            {
                return;
            }

            TriggerEnemySkills(
                source,
                C1FormalEnemySkillTriggerTypes.OnBasicAttackHit,
                0,
                action.AcceptedApplicationEventId,
                emitted);

            MutableAction next = source.Source.primaryAttackDamage > 0
                ? ScheduleCanonicalEnemy(
                    source,
                    checked(action.DueBattleTimeMs + action.IntervalMs),
                    action.RequestedDamage)
                : ScheduleEnemy(
                    profile.attackWave,
                    source,
                    checked(action.DueBattleTimeMs + action.IntervalMs),
                    action.RequestedDamage);
            EmitScheduled(emitted, next);
        }

        private void InitializeEnemySkills(
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            foreach (MutableActor actor in actors)
            {
                foreach (C1FormalEnemySkillDefinition skill in
                         actor.Source.skills)
                {
                    if (EqualsOrdinal(
                            skill.ActivationType,
                            C1FormalEnemySkillActivationTypes.Passive)
                        && EqualsOrdinal(
                            skill.TriggerType,
                            C1FormalEnemySkillTriggerTypes.OnSpawn))
                    {
                        ExecuteEnemySkill(
                            actor,
                            skill,
                            0,
                            string.Empty,
                            emitted);
                        continue;
                    }

                    if (!EqualsOrdinal(
                            skill.ActivationType,
                            C1FormalEnemySkillActivationTypes.Active)
                        || !EqualsOrdinal(
                            skill.TriggerType,
                            C1FormalEnemySkillTriggerTypes.Timer))
                    {
                        continue;
                    }

                    MutableAction action = ScheduleEnemySkill(
                        actor,
                        skill,
                        checked(skill.InitialDelayMilliseconds
                            + skill.CastTimeMilliseconds
                            + actor.StableOrder
                            * CanonicalEnemyStartupPhaseMilliseconds));
                    EmitScheduled(emitted, action);
                }
            }
        }

        private void ProcessEnemySkillAction(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            MutableActor source = FindActor(
                action.SourceId,
                action.SourceStableOrder);
            C1FormalEnemySkillDefinition skill = source?.Source.skills
                .FirstOrDefault(value => EqualsOrdinal(
                    value.SkillId,
                    action.SourceActionId));
            if (source == null || source.CurrentHp <= 0 || skill == null)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleCueKinds.ActorDefeated,
                    string.Empty);
                return;
            }

            string applicationId = ExecuteEnemySkill(
                source,
                skill,
                0,
                action.ActionId,
                emitted);
            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                applicationId);
            if (terminal)
            {
                return;
            }

            MutableAction next = ScheduleEnemySkill(
                source,
                skill,
                checked(action.DueBattleTimeMs
                    + skill.CooldownMilliseconds));
            EmitScheduled(emitted, next);
        }

        private void TriggerEnemySkills(
            MutableActor source,
            string triggerType,
            int incomingAppliedDamage,
            string sourceApplicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null || source.CurrentHp <= 0)
            {
                return;
            }

            foreach (C1FormalEnemySkillDefinition skill in source.Source.skills
                         .Where(value => EqualsOrdinal(
                             value.ActivationType,
                             C1FormalEnemySkillActivationTypes.Passive)
                             && EqualsOrdinal(
                                 value.TriggerType,
                                 triggerType)))
            {
                ExecuteEnemySkill(
                    source,
                    skill,
                    incomingAppliedDamage,
                    sourceApplicationId,
                    emitted);
            }
        }

        private string ExecuteEnemySkill(
            MutableActor source,
            C1FormalEnemySkillDefinition skill,
            int incomingAppliedDamage,
            string sourceApplicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (source == null
                || skill == null
                || source.CurrentHp <= 0
                || !skill.Effects.Any(effect =>
                    CanExecuteEnemySkillEffect(
                        source,
                        effect,
                        incomingAppliedDamage)))
            {
                return string.Empty;
            }

            string applicationId = NewApplicationId();
            acceptedApplicationEventIds.Add(applicationId);
            int requested = 0;
            int applied = 0;
            foreach (C1FormalEnemySkillEffectDefinition effect in
                     skill.Effects)
            {
                if (EqualsOrdinal(
                        effect.EffectKind,
                        C1FormalEnemySkillEffectKinds.DirectDamage))
                {
                    requested = checked(requested + effect.FlatValue);
                    applied = checked(applied + ApplyEnemySkillDamageToPlayer(
                        source.Source.actorBalanceId,
                        source.StableOrder,
                        skill.SkillId,
                        effect.FlatValue,
                        applicationId,
                        string.Empty,
                        "ENEMY_SKILL",
                        skill.SkillId,
                        C1FormalRealtimeBattleFeedbackTriggerKinds.Skill,
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                        emitted));
                }
                else if (EqualsOrdinal(
                             effect.EffectKind,
                             C1FormalEnemySkillEffectKinds.Shield))
                {
                    int before = source.CurrentShell;
                    source.CurrentShell = Math.Min(
                        source.Source.maxShell,
                        checked(source.CurrentShell + effect.FlatValue));
                    source.ShellBroken = source.Source.hasShell
                        && source.CurrentShell <= 0;
                    int shellApplied = source.CurrentShell - before;
                    requested = checked(requested + effect.FlatValue);
                    applied = checked(applied + shellApplied);
                    Emit(
                        emitted,
                        C1FormalRealtimeBattleCueKinds.ActorShellChanged,
                        C1FormalRealtimeBattleSessionContract.EnemyOwner,
                        source.Source.actorBalanceId,
                        source.Source.actorBalanceId,
                        source.StableOrder,
                        playerHp,
                        playerHp,
                        source.CurrentHp,
                        source.CurrentHp,
                        effect.FlatValue,
                        shellApplied,
                        before.ToString(CultureInfo.InvariantCulture) + ">"
                        + source.CurrentShell.ToString(
                            CultureInfo.InvariantCulture),
                        78,
                        applicationId,
                        string.Empty,
                        string.Empty,
                        "ENEMY_SKILL",
                        skill.SkillId,
                        sourceStableOrder: source.StableOrder,
                        triggerKind: FeedbackTriggerForSkill(skill),
                        deliveryKind:
                            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                        resultKind:
                            C1FormalRealtimeBattleFeedbackResultKinds.ShellGain);
                }
                else if (EqualsOrdinal(
                             effect.EffectKind,
                             C1FormalEnemySkillEffectKinds.ApplyStatus))
                {
                    ApplyEnemySkillStatus(
                        source,
                        skill,
                        effect.Status,
                        applicationId,
                        emitted);
                }
                else if (EqualsOrdinal(
                             effect.EffectKind,
                             C1FormalEnemySkillEffectKinds.ReflectDamage))
                {
                    EnemySkillStatusState required = FindEnemySkillStatus(
                        source,
                        effect.RequiredStatusKey);
                    if (required == null || incomingAppliedDamage <= 0)
                    {
                        continue;
                    }

                    int reflected = checked((int)Math.Min(
                        effect.CapValue,
                        Math.Max(
                            1L,
                            (long)incomingAppliedDamage
                            * effect.RatioBasisPointsPerStack
                            * required.StackCount / 10000L)));
                    if (reflected <= 0)
                    {
                        continue;
                    }

                    requested = checked(requested + reflected);
                    applied = checked(applied + ApplyEnemySkillDamageToPlayer(
                        source.Source.actorBalanceId,
                        source.StableOrder,
                        skill.SkillId,
                        reflected,
                        applicationId,
                        string.Empty,
                        required.StatusFamilyKey,
                        required.StatusKey,
                        C1FormalRealtimeBattleFeedbackTriggerKinds.Passive,
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                        emitted));
                }

                if (terminal)
                {
                    break;
                }
            }

            string cueKind = EqualsOrdinal(
                    skill.ActivationType,
                    C1FormalEnemySkillActivationTypes.Passive)
                ? C1FormalRealtimeBattleCueKinds.EnemyPassiveTriggered
                : C1FormalRealtimeBattleCueKinds.EnemySkillAccepted;
            bool targetsPlayer = EqualsOrdinal(
                skill.TargetSelector,
                C1FormalEnemySkillTargetSelectors.Player);
            Emit(
                emitted,
                cueKind,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                source.Source.actorBalanceId,
                targetsPlayer
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : source.Source.actorBalanceId,
                targetsPlayer ? -1 : source.StableOrder,
                playerHp,
                playerHp,
                source.CurrentHp,
                source.CurrentHp,
                requested,
                applied,
                skill.PresentationCueKey + "|triggerSource="
                + (sourceApplicationId ?? string.Empty),
                82,
                applicationId,
                string.Empty,
                string.Empty,
                "ENEMY_SKILL",
                skill.SkillId,
                sourceStableOrder: source.StableOrder,
                triggerKind: FeedbackTriggerForSkill(skill),
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant);
            return applicationId;
        }

        private bool CanExecuteEnemySkillEffect(
            MutableActor source,
            C1FormalEnemySkillEffectDefinition effect,
            int incomingAppliedDamage)
        {
            if (effect == null)
            {
                return false;
            }

            if (EqualsOrdinal(
                    effect.EffectKind,
                    C1FormalEnemySkillEffectKinds.DirectDamage))
            {
                return effect.FlatValue > 0 && playerHp > 0;
            }
            if (EqualsOrdinal(
                    effect.EffectKind,
                    C1FormalEnemySkillEffectKinds.Shield))
            {
                return effect.FlatValue > 0
                    && source.CurrentShell < source.Source.maxShell;
            }
            if (EqualsOrdinal(
                    effect.EffectKind,
                    C1FormalEnemySkillEffectKinds.ApplyStatus))
            {
                return effect.Status != null;
            }
            if (!EqualsOrdinal(
                    effect.EffectKind,
                    C1FormalEnemySkillEffectKinds.ReflectDamage)
                || incomingAppliedDamage <= 0)
            {
                return false;
            }

            EnemySkillStatusState status = FindEnemySkillStatus(
                source,
                effect.RequiredStatusKey);
            return status != null && status.StackCount > 0;
        }

        private static string FeedbackTriggerForSkill(
            C1FormalEnemySkillDefinition skill)
        {
            return skill != null && EqualsOrdinal(
                    skill.ActivationType,
                    C1FormalEnemySkillActivationTypes.Passive)
                ? C1FormalRealtimeBattleFeedbackTriggerKinds.Passive
                : C1FormalRealtimeBattleFeedbackTriggerKinds.Skill;
        }

        private void EmitPlayerGuardDamage(
            string sourceId,
            int sourceStableOrder,
            int requestedDamage,
            int appliedGuardDamage,
            long guardBefore,
            long guardAfter,
            string applicationId,
            string effectFamilyId,
            string effectVariantId,
            string triggerKind,
            string deliveryKind,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (appliedGuardDamage <= 0)
            {
                return;
            }

            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.PlayerGuardChanged,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                sourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                requestedDamage,
                appliedGuardDamage,
                "guard|" + guardBefore.ToString(
                    CultureInfo.InvariantCulture) + ">"
                + guardAfter.ToString(CultureInfo.InvariantCulture),
                72,
                applicationId,
                string.Empty,
                string.Empty,
                effectFamilyId,
                effectVariantId,
                sourceStableOrder: sourceStableOrder,
                triggerKind: triggerKind,
                deliveryKind: deliveryKind,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage);
        }

        private int ApplyEnemySkillDamageToPlayer(
            string sourceActorId,
            int sourceStableOrder,
            string sourceSkillId,
            int requestedDamage,
            string applicationId,
            string acceptedCueKind,
            string effectFamilyId,
            string effectVariantId,
            string feedbackTriggerKind,
            string feedbackDeliveryKind,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (requestedDamage <= 0 || playerHp <= 0)
            {
                return 0;
            }

            int before = playerHp;
            long absorbedByGuard = Math.Min(playerGuard, requestedDamage);
            if (absorbedByGuard > 0L)
            {
                long guardBefore = playerGuard;
                playerGuard -= absorbedByGuard;
                effectState[EffectStorageKey(
                    "player.guard",
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    "flat")] = playerGuard;
                EmitPlayerGuardDamage(
                    sourceActorId,
                    sourceStableOrder,
                    requestedDamage,
                    checked((int)absorbedByGuard),
                    guardBefore,
                    playerGuard,
                    applicationId,
                    effectFamilyId,
                    effectVariantId,
                    feedbackTriggerKind,
                    feedbackDeliveryKind,
                    emitted);
            }

            int remaining = requestedDamage - checked((int)absorbedByGuard);
            int applied = Math.Min(playerHp, remaining);
            playerHp -= applied;
            enemyApplicationCount++;
            if (!string.IsNullOrEmpty(acceptedCueKind))
            {
                Emit(
                    emitted,
                    acceptedCueKind,
                    C1FormalRealtimeBattleSessionContract.EnemyOwner,
                    sourceActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    -1,
                    before,
                    playerHp,
                    0,
                    0,
                    requestedDamage,
                    applied,
                    string.Empty,
                    80,
                    applicationId,
                    string.Empty,
                    string.Empty,
                    effectFamilyId,
                    effectVariantId,
                    sourceStableOrder: sourceStableOrder,
                    triggerKind: feedbackTriggerKind,
                    deliveryKind: feedbackDeliveryKind,
                    resultKind:
                        C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            }
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.HitAccepted,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                sourceActorId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                requestedDamage,
                applied,
                string.Empty,
                75,
                applicationId,
                string.Empty,
                string.Empty,
                effectFamilyId,
                effectVariantId,
                sourceStableOrder: sourceStableOrder,
                triggerKind: feedbackTriggerKind,
                deliveryKind: feedbackDeliveryKind,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.PlayerHpChanged,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                requestedDamage,
                applied,
                string.Empty,
                70,
                applicationId,
                string.Empty,
                string.Empty,
                effectFamilyId,
                effectVariantId,
                sourceStableOrder: sourceStableOrder,
                triggerKind: feedbackTriggerKind,
                deliveryKind: feedbackDeliveryKind,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                sourceActorId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                requestedDamage,
                applied,
                "-" + applied.ToString(CultureInfo.InvariantCulture),
                65,
                applicationId,
                string.Empty,
                string.Empty,
                effectFamilyId,
                effectVariantId,
                sourceStableOrder: sourceStableOrder,
                triggerKind: feedbackTriggerKind,
                deliveryKind: feedbackDeliveryKind,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);

            ApplyCanonicalPlayerDamagedEffects(
                requestedDamage,
                applied,
                applicationId,
                emitted);
            if (playerHp > 0)
            {
                ProduceAutomaticPool15Triggers(
                    new[] { "on_damaged" },
                    emitted);
            }
            else
            {
                CompleteTerminal(false, emitted);
            }
            return applied;
        }

        private void ApplyEnemySkillStatus(
            MutableActor source,
            C1FormalEnemySkillDefinition skill,
            C1FormalEnemyStatusDefinition definition,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (definition == null)
            {
                return;
            }

            MutableActor targetActor = EqualsOrdinal(
                    skill.TargetSelector,
                    C1FormalEnemySkillTargetSelectors.Self)
                ? source
                : null;
            EnemySkillStatusState status = enemySkillStatuses
                .FirstOrDefault(value => !value.Removed
                    && ReferenceEquals(value.TargetActor, targetActor)
                    && EqualsOrdinal(
                        value.StatusKey,
                        definition.StatusKey));
            bool created = status == null;
            long expireAt = checked(
                battleTimeMs + definition.DurationMilliseconds);
            if (created)
            {
                status = new EnemySkillStatusState(
                    targetActor,
                    definition,
                    expireAt,
                    battleTimeMs);
                enemySkillStatuses.Add(status);
            }

            bool stackAdded = status.StackCount < status.MaxStack;
            if (stackAdded)
            {
                status.Contributions.Add(
                    new EnemySkillStatusContribution(
                        source.Source.actorBalanceId,
                        source.Source.contentId,
                        source.StableOrder,
                        definition.ValuePerStack,
                        battleTimeMs));
            }

            long previousExpiry = status.ExpireAtBattleTimeMs;
            status.ExpireAtBattleTimeMs = expireAt;
            if (created)
            {
                if (definition.TickIntervalMilliseconds > 0L)
                {
                    status.NextTickAtBattleTimeMs = checked(
                        battleTimeMs
                        + definition.TickIntervalMilliseconds);
                    status.TickAction = ScheduleEnemySkillStatusAction(
                        status,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemySkillStatusTick,
                        status.NextTickAtBattleTimeMs);
                }
                status.ExpireAction = ScheduleEnemySkillStatusAction(
                    status,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .EnemySkillStatusExpire,
                    status.ExpireAtBattleTimeMs);
            }
            else if (status.ExpireAction == null
                     || !status.ExpireAction.TryReschedule(
                         previousExpiry,
                         status.ExpireAtBattleTimeMs))
            {
                status.ExpireAction = ScheduleEnemySkillStatusAction(
                    status,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .EnemySkillStatusExpire,
                    status.ExpireAtBattleTimeMs);
            }

            Emit(
                emitted,
                created
                    ? C1FormalRealtimeBattleCueKinds.StatusApplied
                    : C1FormalRealtimeBattleCueKinds.StatusRefreshed,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                source.Source.actorBalanceId,
                targetActor == null
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : targetActor.Source.actorBalanceId,
                targetActor == null ? -1 : targetActor.StableOrder,
                playerHp,
                playerHp,
                targetActor == null ? 0 : targetActor.CurrentHp,
                targetActor == null ? 0 : targetActor.CurrentHp,
                1,
                stackAdded ? 1 : 0,
                status.StatusKey + "|stack="
                + status.StackCount.ToString(CultureInfo.InvariantCulture)
                + "|expire=" + status.ExpireAtBattleTimeMs.ToString(
                    CultureInfo.InvariantCulture),
                76,
                applicationId,
                string.Empty,
                string.Empty,
                status.StatusFamilyKey,
                status.StatusKey,
                sourceStableOrder: source.StableOrder,
                triggerKind: FeedbackTriggerForSkill(skill),
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind: definition.Cleanseable
                    ? (created
                        ? C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply
                        : C1FormalRealtimeBattleFeedbackResultKinds
                            .DebuffRefresh)
                    : (created
                        ? C1FormalRealtimeBattleFeedbackResultKinds.BuffApply
                        : C1FormalRealtimeBattleFeedbackResultKinds
                            .BuffRefresh));
        }

        private void ProcessEnemySkillStatusTick(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            EnemySkillStatusState status = enemySkillStatuses
                .FirstOrDefault(value => !value.Removed
                    && EqualsOrdinal(value.RuntimeId, action.SourceId));
            if (status == null
                || status.TargetActor != null
                || !EqualsOrdinal(
                    status.Definition.BehaviorKind,
                    C1FormalEnemyStatusBehaviorKinds.PeriodicDamage))
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    string.Empty);
                return;
            }

            string lastApplicationId = string.Empty;
            foreach (EnemySkillStatusContribution contribution in
                     status.Contributions.ToArray())
            {
                string applicationId = NewApplicationId();
                acceptedApplicationEventIds.Add(applicationId);
                ApplyEnemySkillDamageToPlayer(
                    contribution.SourceActorId,
                    contribution.SourceStableOrder,
                    status.StatusKey,
                    contribution.TickPotency,
                    applicationId,
                    C1FormalRealtimeBattleCueKinds.StatusTickAccepted,
                    status.StatusFamilyKey,
                    status.StatusKey,
                    C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                    emitted);
                lastApplicationId = applicationId;
                if (terminal)
                {
                    break;
                }
            }

            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                lastApplicationId);
            if (terminal)
            {
                return;
            }

            long next = checked(
                battleTimeMs + status.Definition.TickIntervalMilliseconds);
            if (next <= status.ExpireAtBattleTimeMs)
            {
                status.NextTickAtBattleTimeMs = next;
                status.TickAction = ScheduleEnemySkillStatusAction(
                    status,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .EnemySkillStatusTick,
                    next);
            }
            else
            {
                status.NextTickAtBattleTimeMs = 0L;
                status.TickAction = null;
            }
        }

        private void ProcessEnemySkillStatusExpire(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            EnemySkillStatusState status = enemySkillStatuses
                .FirstOrDefault(value => !value.Removed
                    && EqualsOrdinal(value.RuntimeId, action.SourceId));
            if (status == null)
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    string.Empty);
                return;
            }

            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            RemoveEnemySkillStatus(
                status,
                "EXPIRED",
                string.Empty,
                emitted);
        }

        private MutableAction ScheduleEnemySkillStatusAction(
            EnemySkillStatusState status,
            string actionKind,
            long dueBattleTimeMs)
        {
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                actionKind,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                status.RuntimeId,
                0,
                0L,
                status.TargetActor == null
                    ? -1
                    : status.TargetActor.StableOrder,
                false,
                false,
                string.Empty,
                string.Empty,
                status.StatusKey,
                status.TargetActor == null
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : status.TargetActor.Source.actorBalanceId,
                status.TargetActor == null
                    ? -1
                    : status.TargetActor.StableOrder);
            AddAction(action);
            return action;
        }

        private EnemySkillStatusState FindEnemySkillStatus(
            MutableActor targetActor,
            string statusKey)
        {
            return enemySkillStatuses.FirstOrDefault(value => !value.Removed
                && ReferenceEquals(value.TargetActor, targetActor)
                && EqualsOrdinal(value.StatusKey, statusKey));
        }

        private void RemoveEnemySkillStatus(
            EnemySkillStatusState status,
            string reason,
            string applicationId,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            if (status == null || status.Removed)
            {
                return;
            }

            status.Removed = true;
            if (status.TickAction != null && !status.TickAction.Executed)
            {
                status.TickAction.MarkExecuted(
                    false,
                    reason,
                    applicationId);
            }
            if (status.ExpireAction != null
                && !status.ExpireAction.Executed)
            {
                status.ExpireAction.MarkExecuted(
                    false,
                    reason,
                    applicationId);
            }

            EnemySkillStatusContribution source =
                status.Contributions.FirstOrDefault();
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.StatusRemoved,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                source == null ? status.RuntimeId : source.SourceActorId,
                status.TargetActor == null
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : status.TargetActor.Source.actorBalanceId,
                status.TargetActor == null
                    ? -1
                    : status.TargetActor.StableOrder,
                playerHp,
                playerHp,
                status.TargetActor == null
                    ? 0
                    : status.TargetActor.CurrentHp,
                status.TargetActor == null
                    ? 0
                    : status.TargetActor.CurrentHp,
                status.StackCount,
                status.StackCount,
                status.StatusKey + "|" + reason,
                74,
                applicationId,
                string.Empty,
                string.Empty,
                status.StatusFamilyKey,
                status.StatusKey,
                sourceStableOrder: source == null
                    ? -1
                    : source.SourceStableOrder,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Delayed,
                resultKind: status.Definition.Cleanseable
                    ? C1FormalRealtimeBattleFeedbackResultKinds.DebuffRemove
                    : C1FormalRealtimeBattleFeedbackResultKinds.BuffRemove);
            enemySkillStatuses.Remove(status);
        }

        private bool ApplyEnemyDamageToPlayer(
            MutableAction action,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            string applicationId = NewApplicationId();
            if (!acceptedApplicationEventIds.Add(applicationId))
            {
                CompleteFault(
                    C1FormalRealtimeBattleErrorCodes
                        .DuplicateAcceptedApplication,
                    emitted);
                return false;
            }

            int before = playerHp;
            long absorbedByGuard = Math.Min(
                playerGuard,
                action.RequestedDamage);
            if (absorbedByGuard > 0L)
            {
                long guardBefore = playerGuard;
                playerGuard -= absorbedByGuard;
                effectState[EffectStorageKey(
                    "player.guard",
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    "flat")] = playerGuard;
                EmitPlayerGuardDamage(
                    action.SourceId,
                    action.SourceStableOrder,
                    action.RequestedDamage,
                    checked((int)absorbedByGuard),
                    guardBefore,
                    playerGuard,
                    applicationId,
                    "ENEMY_ACTION",
                    action.SourceActionId,
                    C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                    emitted);
            }

            int remainingDamage = action.RequestedDamage
                - checked((int)absorbedByGuard);
            int applied = Math.Min(playerHp, remainingDamage);
            playerHp -= applied;
            enemyApplicationCount++;
            action.MarkExecuted(
                true,
                C1FormalRealtimeBattleErrorCodes.None,
                applicationId);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                action.SourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                action.RequestedDamage,
                applied,
                string.Empty,
                80,
                applicationId,
                string.Empty,
                string.Empty,
                string.IsNullOrEmpty(action.SourceActionId)
                    ? string.Empty
                    : "ENEMY_ACTION",
                action.SourceActionId,
                sourceStableOrder: action.SourceStableOrder,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.HitAccepted,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                action.SourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                action.RequestedDamage,
                applied,
                string.Empty,
                75,
                applicationId,
                string.Empty,
                string.Empty,
                string.IsNullOrEmpty(action.SourceActionId)
                    ? string.Empty
                    : "ENEMY_ACTION",
                action.SourceActionId,
                sourceStableOrder: action.SourceStableOrder,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.PlayerHpChanged,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                action.RequestedDamage,
                applied,
                string.Empty,
                70,
                applicationId,
                string.Empty,
                string.Empty,
                string.IsNullOrEmpty(action.SourceActionId)
                    ? string.Empty
                    : "ENEMY_ACTION",
                action.SourceActionId,
                sourceStableOrder: action.SourceStableOrder,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                action.SourceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                before,
                playerHp,
                0,
                0,
                action.RequestedDamage,
                applied,
                "-" + applied.ToString(CultureInfo.InvariantCulture),
                65,
                applicationId,
                string.Empty,
                string.Empty,
                string.IsNullOrEmpty(action.SourceActionId)
                    ? string.Empty
                    : "ENEMY_ACTION",
                action.SourceActionId,
                sourceStableOrder: action.SourceStableOrder,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind:
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage);

            ApplyCanonicalPlayerDamagedEffects(
                action.RequestedDamage,
                applied,
                applicationId,
                emitted);

            if (playerHp > 0)
            {
                ProduceAutomaticPool15Triggers(
                    new[] { "on_damaged" },
                    emitted);
            }

            if (playerHp <= 0)
            {
                CompleteTerminal(false, emitted);
            }
            return true;
        }

        private void CancelEnemyActionsForDefeatedActor(
            MutableActor actor,
            string sourceApplicationId)
        {
            foreach (MutableAction action in actions.Where(value =>
                         !value.Executed
                         && (string.Equals(
                                 value.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemyBasicWave,
                                 StringComparison.Ordinal)
                             || string.Equals(
                                 value.ActionKind,
                                 C1FormalRealtimeBattleScheduledActionKinds
                                     .EnemySkill,
                                 StringComparison.Ordinal))
                         && value.SourceStableOrder == actor.StableOrder
                         && EqualsOrdinal(
                             value.SourceId,
                             actor.Source.actorBalanceId)))
            {
                action.MarkExecuted(
                    false,
                    C1FormalRealtimeBattleCueKinds.ActorDefeated,
                    string.Empty);
            }

            foreach (EnemySkillStatusState status in enemySkillStatuses
                         .Where(value => !value.Removed
                             && ReferenceEquals(value.TargetActor, actor))
                         .ToArray())
            {
                status.Removed = true;
                if (status.TickAction != null
                    && !status.TickAction.Executed)
                {
                    status.TickAction.MarkExecuted(
                        false,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        sourceApplicationId);
                }
                if (status.ExpireAction != null
                    && !status.ExpireAction.Executed)
                {
                    status.ExpireAction.MarkExecuted(
                        false,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        sourceApplicationId);
                }
                enemySkillStatuses.Remove(status);
            }

        }

        private void CompleteTerminal(
            bool win,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            terminal = true;
            paused = false;
            phase = win
                ? C1FormalRealtimeBattleSessionContract.PhaseVictory
                : C1FormalRealtimeBattleSessionContract.PhaseDefeat;
            foreach (MutableAction action in actions.Where(row => !row.Executed))
            {
                action.MarkExecuted(
                    false,
                    "TERMINAL_INVALIDATED_PENDING_ACTION",
                    string.Empty);
            }
            bool liveTerminalOwned = profile != null;
            bool terminalAccepted = liveTerminalOwned;
            C1FormalRealtimeBattleError terminalError = terminalAccepted
                ? Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty)
                : Error(
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    "Live terminal profile ownership is unavailable.");
            if (!terminalAccepted)
            {
                phase = C1FormalRealtimeBattleSessionContract.PhaseFaulted;
            }

            BattleResultSnapshot battleResult = BuildBattleResult(win);
            terminalResult = new C1FormalRealtimeBattleTerminalResult(
                terminalAccepted,
                terminalError,
                request.sessionId,
                request.stageId,
                battleTimeMs,
                win,
                !win,
                PlayerSnapshot(),
                ActorSnapshots(),
                acceptedItemApplicationCount,
                enemyApplicationCount,
                battleResult);
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.BattleTerminal,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.stageId,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                win ? "VICTORY" : "DEFEAT",
                100,
                string.Empty);
            usedSessionIds.Add(request.sessionId);
            usedSessionTokens.Add(request.sessionToken);
        }

        private void CompleteFault(
            string errorCode,
            ICollection<C1FormalRealtimeBattleCue> emitted)
        {
            terminal = true;
            paused = false;
            phase = C1FormalRealtimeBattleSessionContract.PhaseFaulted;
            terminalResult = new C1FormalRealtimeBattleTerminalResult(
                false,
                Error(errorCode, "Live Battle Session failed closed."),
                request.sessionId,
                request.stageId,
                battleTimeMs,
                false,
                false,
                PlayerSnapshot(),
                ActorSnapshots(),
                acceptedItemApplicationCount,
                enemyApplicationCount,
                BuildBattleResult(false));
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.BattleTerminal,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                errorCode,
                string.Empty,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                "FAULTED",
                100,
                string.Empty);
        }

        private BattleResultSnapshot BuildBattleResult(bool win)
        {
            return new BattleResultSnapshot
            {
                resultId = request.sessionId + ":result",
                requestId = request.sessionId,
                resultType = win
                    ? BattleResultType.Win
                    : playerHp <= 0 ? BattleResultType.Lose : BattleResultType.Unknown,
                win = win,
                lose = playerHp <= 0,
                abandon = false,
                roundId = request.stageId,
                bossDefeated = false,
                durationSeconds = battleTimeMs / 1000f,
                chapterProgressDelta = string.Empty,
                rewardPreview = new List<string>(),
                itemDrops = new List<string>(),
                buildPerformanceSummary = string.Join(";", new[]
                {
                    "stageId=" + request.stageId,
                    "durationMs=" + battleTimeMs.ToString(
                        CultureInfo.InvariantCulture),
                    "acceptedItemApplications="
                    + acceptedItemApplicationCount.ToString(
                        CultureInfo.InvariantCulture),
                    "enemyApplications=" + enemyApplicationCount.ToString(
                        CultureInfo.InvariantCulture)
                }),
                eventSummary = cueLedger.Select(cue =>
                    cue.cueId + "|" + cue.cueKind).ToList(),
                rewardClaimToken = string.Empty,
                nextRouteHint = string.Empty,
                devOnly = false,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        private static bool IsCanonicalItemFactValid(
            C1FormalRealtimeBattleItemFactSnapshot item,
            string expectedResourceKey)
        {
            ItemGeneratedInstanceSnapshot generated =
                item == null ? null : item.generatedInstance;
            CanonicalItemDefinition definition =
                item == null ? null : item.canonicalDefinition;
            C1FormalRealtimeBattleItemActionCostSnapshot cost =
                item == null ? null : item.actionCostFact;
            if (item == null
                || generated == null
                || generated.identity == null
                || definition == null
                || !definition.isOrdinaryDropEligible
                || definition.combatEffect == null
                || string.IsNullOrEmpty(definition.combatEffect.effectId)
                || string.IsNullOrEmpty(
                    definition.combatEffect.triggerEventId)
                || string.IsNullOrEmpty(definition.combatEffect.conditionId)
                || string.IsNullOrEmpty(
                    definition.combatEffect.targetSelector)
                || string.IsNullOrEmpty(
                    definition.combatEffect.targetStatId)
                || string.IsNullOrEmpty(
                    definition.combatEffect.valueUnitKey)
                || !item.isLit
                || !EqualsOrdinal(item.sourceId, item.itemInstanceId)
                || !EqualsOrdinal(
                    generated.itemInstanceId,
                    item.itemInstanceId)
                || !EqualsOrdinal(generated.baseItemId, item.baseItemId)
                || !EqualsOrdinal(definition.baseItemId, item.baseItemId)
                || !EqualsOrdinal(
                    item.itemCatalogCanonicalSignature,
                    CanonicalItemCatalogContract.CatalogId)
                || cost == null
                || !EqualsOrdinal(cost.sourceItemInstanceId,
                    item.itemInstanceId)
                || !EqualsOrdinal(cost.baseItemId, item.baseItemId)
                || !EqualsOrdinal(cost.rarityVersionKey,
                    item.rarityVersionKey)
                || !EqualsOrdinal(cost.actionDefinitionId,
                    "canonical-item-action")
                || !EqualsOrdinal(cost.resourceKey, expectedResourceKey)
                || !EqualsOrdinal(cost.sourceRevision,
                    CanonicalItemCatalogContract.CatalogVersion)
                || !EqualsOrdinal(cost.sourceProfileRevision,
                    CanonicalItemCatalogContract.CatalogVersion)
                || !EqualsOrdinal(cost.sourceProfileCanonicalSignature,
                    CanonicalItemCatalogContract.CatalogId)
                || !EqualsOrdinal(cost.sourceProjectionCanonicalSignature,
                    item.projectionCanonicalSignature))
            {
                return false;
            }

            long rolledDamage = ReadCanonicalGeneratedStat(
                generated,
                "damage");
            long rolledNianCost = ReadCanonicalGeneratedStat(
                generated,
                "nianCost");
            long rolledCooldownTurns = ReadCanonicalGeneratedStat(
                generated,
                "cooldown");
            return rolledDamage >= 0L
                && rolledDamage <= int.MaxValue
                && item.directDamage == checked((int)rolledDamage)
                && rolledNianCost > 0L
                && rolledNianCost <= int.MaxValue
                && cost.cost == checked((int)rolledNianCost)
                && rolledCooldownTurns > 0L
                && rolledCooldownTurns <= long.MaxValue / 400L
                && item.cooldownMs == rolledCooldownTurns * 400L
                && cost.cadenceMilliseconds == item.cooldownMs
                && cost.firstOffsetMilliseconds == item.cooldownMs;
        }

        private static long ReadCanonicalGeneratedStat(
            ItemGeneratedInstanceSnapshot generated,
            string statId)
        {
            ItemGeneratedStatSnapshot row = generated?.GeneratedStats?
                .SingleOrDefault(value => EqualsOrdinal(
                    value.statId,
                    statId));
            return row == null ? 0L : row.rawUnits;
        }

        private bool TryValidateRequest(
            C1FormalRealtimeBattleSessionRequest candidate,
            out C1FormalRealtimeBattleResolvedEncounter resolvedProfile,
            out C1FormalRealtimeBattleError error)
        {
            resolvedProfile = null;
            if (candidate == null)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.RequestNull,
                    "The live Battle Session request is null.");
                return false;
            }

            if (!EqualsOrdinal(
                    candidate.schemaId,
                    C1FormalRealtimeBattleSessionContract.RequestSchemaId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SchemaMismatch,
                    "The live Battle Session schema is unsupported.");
                return false;
            }

            bool stageOne = EqualsOrdinal(candidate.stageId, "1-1");
            bool stageTwo = EqualsOrdinal(candidate.stageId, "1-2");
            bool stageThreeToFive =
                C1FormalRealtimeBattleEncounterResolver.IsStage3To5(
                    candidate.stageId);
            if (!EqualsOrdinal(
                    candidate.productContext,
                    C1FormalRealtimeBattleSessionContract.ProductContext)
                || !EqualsOrdinal(
                    candidate.chapterId,
                    C1FormalItemSessionContract.ChapterId)
                || (!stageOne && !stageTwo && !stageThreeToFive)
                || string.IsNullOrWhiteSpace(candidate.balanceProfileId)
                || string.IsNullOrWhiteSpace(candidate.encounterVariantId)
                || string.IsNullOrEmpty(candidate.launchId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ContextRejected,
                    "Only a valid CAMPAIGN_NORMAL_LV1 stage 1-1 through 1-5 envelope is accepted.");
                return false;
            }

            if (string.IsNullOrEmpty(candidate.sessionId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SessionIdRequired,
                    "sessionId is required.");
                return false;
            }

            if (string.IsNullOrEmpty(candidate.sessionToken))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SessionTokenRequired,
                    "sessionToken is required.");
                return false;
            }

            if (candidate.launchGeneration <= 0L)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.LaunchGenerationInvalid,
                    "launchGeneration must be positive.");
                return false;
            }

            if (candidate.launchGeneration <= lastLaunchGeneration)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.LaunchGenerationStale,
                    "launchGeneration is stale for this live Session authority.");
                return false;
            }

            if (candidate.resetGeneration != currentResetGeneration)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ResetGenerationMismatch,
                    "resetGeneration is stale.");
                return false;
            }

            if (usedSessionIds.Contains(candidate.sessionId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.DuplicateSessionId,
                    "sessionId already belonged to an accepted Session.");
                return false;
            }

            if (invalidatedSessionTokens.Contains(candidate.sessionToken))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SessionTokenInvalidated,
                    "sessionToken was invalidated by reset or unbind.");
                return false;
            }

            if (usedSessionTokens.Contains(candidate.sessionToken))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.DuplicateSessionToken,
                    "sessionToken already belonged to an accepted Session.");
                return false;
            }

            if (string.IsNullOrEmpty(candidate.itemBattleInputCanonical)
                || candidate.itemFacts == null
                || candidate.itemFacts.Count >
                    C1FormalRealtimeBattleSessionContract
                        .MaxCumulativeItemInstanceCount
                || candidate.itemFacts.Any(item => item == null)
                || candidate.itemFacts.Any(item =>
                    string.IsNullOrEmpty(item.itemInstanceId))
                || candidate.itemFacts.Select(item => item.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count()
                    != candidate.itemFacts.Count
                || candidate.itemFacts.Select(item => item.sourceId)
                    .Distinct(StringComparer.Ordinal).Count()
                    != candidate.itemFacts.Count)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected,
                    "The cumulative formal Item input is missing or its derived direct-effect facts are invalid.");
                return false;
            }

            if (!TryValidateNianCapacity(
                    candidate.nianCapacityFact,
                    candidate.productContext,
                    candidate.itemBattleInputCanonical,
                    candidate.arrangementCanonicalSignature,
                    out error))
            {
                return false;
            }

            if (!EqualsOrdinal(
                    candidate.expectedItemCatalogCanonical,
                    CanonicalItemCatalogContract.CatalogId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ItemCatalogMismatch,
                    "The live Item facts do not match the accepted Item catalog.");
                return false;
            }

            for (int index = 0; index < candidate.itemFacts.Count; index++)
            {
                C1FormalRealtimeBattleItemFactSnapshot item =
                    candidate.itemFacts[index];
                if (!IsCanonicalItemFactValid(
                        item,
                        candidate.nianCapacityFact.resourceKey))
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes.ItemInputRejected,
                        "A live Item row does not match its accepted immutable projection.");
                    return false;
                }
            }

            if (!TryValidatePool15RequestFacts(candidate, out error))
            {
                return false;
            }

            if (stageThreeToFive
                && !EqualsOrdinal(
                    candidate.expectedItemCatalogCanonical,
                    CanonicalItemCatalogContract.CatalogId)
                && (string.IsNullOrEmpty(candidate.pool15ItemInputCanonical)
                    || candidate.pool15ActiveItemFacts.Count >
                    C1FormalRealtimeBattleSessionContract
                        .MaxPool15ActiveSourceCount))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                    "Stage 1-3 to 1-5 requires the released bounded Pool15 envelope.");
                return false;
            }

            if (!C1FormalRealtimeBattleEncounterResolver.TryResolve(
                    candidate,
                    out resolvedProfile,
                    out error))
            {
                return false;
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private static bool TryValidatePool15RequestFacts(
            C1FormalRealtimeBattleSessionRequest candidate,
            out C1FormalRealtimeBattleError error)
        {
            if (EqualsOrdinal(
                    candidate.expectedItemCatalogCanonical,
                    CanonicalItemCatalogContract.CatalogId))
            {
                bool canonicalEnvelope = EqualsOrdinal(
                        candidate.pool15ItemInputCanonical,
                        candidate.itemBattleInputCanonical)
                    && candidate.pool15ActiveItemFacts.Count == 0
                    && string.IsNullOrEmpty(
                        candidate.pool15PoolCanonicalSignature)
                    && string.IsNullOrEmpty(
                        candidate.pool15SourceItemCatalogCanonicalSignature)
                    && string.IsNullOrEmpty(candidate.pool15CandidateProfileId)
                    && string.IsNullOrEmpty(
                        candidate.pool15CandidateDataMaturity)
                    && string.IsNullOrEmpty(
                        candidate.pool15CandidateProfileCanonicalSignature);
                error = canonicalEnvelope
                    ? Error(
                        C1FormalRealtimeBattleErrorCodes.None,
                        string.Empty)
                    : Error(
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15SnapshotRejected,
                        "The canonical Item request contains a legacy effect envelope.");
                return canonicalEnvelope;
            }

            bool bound = !string.IsNullOrEmpty(
                candidate.pool15ItemInputCanonical);
            if (!bound)
            {
                bool empty = candidate.pool15ActiveItemFacts.Count == 0
                    && string.IsNullOrEmpty(
                        candidate.pool15PoolCanonicalSignature)
                    && string.IsNullOrEmpty(
                        candidate.pool15SourceItemCatalogCanonicalSignature)
                    && string.IsNullOrEmpty(
                        candidate.pool15CandidateProfileId)
                    && string.IsNullOrEmpty(
                        candidate.pool15CandidateDataMaturity)
                    && string.IsNullOrEmpty(
                        candidate.pool15CandidateProfileCanonicalSignature);
                error = empty
                    ? Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty)
                    : Error(
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15SnapshotRejected,
                        "A partial Pool15 envelope is not accepted.");
                return empty;
            }

            C1CampaignLootPool15BattleFoundationResult foundationResult =
                C1CampaignLootPool15BattleEffectFamilyFoundation.TryCreate();
            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootPool15BattleCandidateProfileSnapshot profiles =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            if (foundationResult == null
                || !foundationResult.accepted
                || foundationResult.foundation == null
                || poolResult == null
                || !poolResult.accepted
                || poolResult.snapshot == null
                || profiles.entersFormalFlow
                || !EqualsOrdinal(
                    candidate.pool15ItemInputCanonical,
                    candidate.itemBattleInputCanonical)
                || !EqualsOrdinal(
                    candidate.pool15PoolCanonicalSignature,
                    foundationResult.foundation.poolCanonicalSignature)
                || !EqualsOrdinal(
                    candidate.pool15SourceItemCatalogCanonicalSignature,
                    foundationResult.foundation
                        .sourceItemCatalogCanonicalSignature)
                || !EqualsOrdinal(
                    candidate.pool15CandidateProfileId,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .ProfileId)
                || !EqualsOrdinal(
                    candidate.pool15CandidateDataMaturity,
                    C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                        .DataMaturity)
                || !EqualsOrdinal(
                    candidate.pool15CandidateProfileCanonicalSignature,
                    profiles.canonicalSignature)
                || candidate.pool15ActiveItemFacts == null
                || candidate.pool15ActiveItemFacts.Count >
                    C1FormalRealtimeBattleSessionContract
                        .MaxPool15ActiveSourceCount
                || candidate.pool15ActiveItemFacts.Any(row => row == null)
                || candidate.pool15ActiveItemFacts.Select(
                        row => row.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count()
                != candidate.pool15ActiveItemFacts.Count)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                    "The formal Session Pool15 envelope is stale or duplicated.");
                return false;
            }

            C1CampaignLootEligibleItemPool15Snapshot pool = poolResult.snapshot;
            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact in
                     candidate.pool15ActiveItemFacts)
            {
                bool immutableCombatFact = fact.HasImmutableCombatFact
                    && !string.IsNullOrEmpty(
                        fact.sourceProfileCanonicalSignature)
                    && !string.IsNullOrEmpty(
                        fact.combatFactCanonicalSignature)
                    && fact.components.All(component => component != null
                        && !string.IsNullOrEmpty(component.operationId)
                        && !string.IsNullOrEmpty(component.targetStatId)
                        && !string.IsNullOrEmpty(component.nativeUnitId));
                C1CampaignLootItemProfileSnapshot descriptor = immutableCombatFact
                    ? null
                    : pool.FindProfile(fact.baseItemId);
                C1CampaignLootPool15BattleCandidateProfile profile =
                    immutableCombatFact
                        ? null
                        : profiles.Find(fact.baseItemId);
                if (string.IsNullOrEmpty(fact.itemInstanceId)
                    || string.IsNullOrEmpty(fact.sourceFactCanonicalSignature)
                    || !fact.HasValidPlacedLitSourceFactCanonical
                    || (!immutableCombatFact
                        && (descriptor == null
                            || profile == null
                            || !EqualsOrdinal(
                                fact.descriptorCanonicalSignature,
                                descriptor.profileCanonicalSignature)
                            || !EqualsOrdinal(
                                fact.candidateProfileCanonicalSignature,
                                profile.canonicalSignature))))
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes
                            .Pool15SnapshotRejected,
                        "An active Pool15 fact is unknown, stale, or inconsistent.");
                    return false;
                }
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private static bool TryValidateNianCapacity(
            C1FormalRealtimeBattleNianCapacitySnapshot capacity,
            string expectedProductContext,
            string expectedItemInputCanonical,
            string expectedArrangementCanonical,
            out C1FormalRealtimeBattleError error)
        {
            bool accepted = capacity != null
                && EqualsOrdinal(
                    capacity.schemaId,
                    C1FormalRealtimeBattleSessionContract.NianCapacitySchemaId)
                && EqualsOrdinal(
                    capacity.productContext,
                    expectedProductContext)
                && !string.IsNullOrEmpty(capacity.resourceKey)
                && capacity.initialNian >= 0
                && capacity.maxNian > 0
                && capacity.initialNian <= capacity.maxNian
                && capacity.generationAmount > 0
                && capacity.generationIntervalMilliseconds > 0L
                && !string.IsNullOrEmpty(capacity.sourceRevision)
                && !string.IsNullOrEmpty(capacity.sourceProfileId)
                && !string.IsNullOrEmpty(capacity.sourceItemInstanceId)
                && !string.IsNullOrEmpty(capacity.sourceBaseItemId)
                && !string.IsNullOrEmpty(capacity.stablePlacementId)
                && !string.IsNullOrEmpty(
                    capacity.itemSessionCanonicalSignature)
                && EqualsOrdinal(
                    capacity.arrangementCanonicalSignature,
                    expectedArrangementCanonical)
                && !string.IsNullOrEmpty(
                    capacity.itemSystemCanonicalSignature)
                && !string.IsNullOrEmpty(capacity.canonicalSignature);
            error = accepted
                ? Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty)
                : Error(
                    C1FormalRealtimeBattleErrorCodes.NianCapacityRejected,
                    "The formal Nian capacity fact is missing, stale, or invalid.");
            return accepted;
        }

        private static bool TryValidateActionCost(
            C1FormalRealtimeBattleItemFactSnapshot item,
            string expectedResourceKey)
        {
            C1FormalRealtimeBattleItemActionCostSnapshot cost =
                item == null ? null : item.actionCostFact;
            return cost != null
                && EqualsOrdinal(
                    cost.schemaId,
                    C1FormalRealtimeBattleSessionContract.ItemActionCostSchemaId)
                && EqualsOrdinal(cost.sourceItemInstanceId, item.itemInstanceId)
                && EqualsOrdinal(cost.baseItemId, item.baseItemId)
                && EqualsOrdinal(cost.rarityVersionKey, item.rarityVersionKey)
                && EqualsOrdinal(cost.resourceKey, expectedResourceKey)
                && cost.cost > 0
                && !string.IsNullOrEmpty(cost.actionDefinitionId)
                && !string.IsNullOrEmpty(cost.sourceRevision)
                && !string.IsNullOrEmpty(cost.sourceProfileRevision)
                && !string.IsNullOrEmpty(
                    cost.sourceProfileCanonicalSignature)
                && EqualsOrdinal(
                    cost.sourceProjectionCanonicalSignature,
                    item.projectionCanonicalSignature)
                && cost.cadenceMilliseconds == item.cooldownMs
                && cost.firstOffsetMilliseconds > 0L;
        }

        private static bool TryGetI002DirectDamage(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
            out int directDamage)
        {
            directDamage = 0;
            if (fact == null || !fact.HasImmutableCombatFact)
            {
                return false;
            }

            C1FormalRealtimeBattleCombatComponentSnapshot[] rows =
                fact.components.Where(component =>
                        component != null
                        && EqualsOrdinal(
                            component.operationId,
                            "ApplyDirectDamage")
                        && EqualsOrdinal(component.targetStatId, "damage")
                        && EqualsOrdinal(component.nativeUnitId, "flat"))
                    .ToArray();
            if (rows.Length != 1
                || rows[0].signedAmount <= 0L
                || rows[0].signedAmount > int.MaxValue)
            {
                return false;
            }

            directDamage = checked((int)rows[0].signedAmount);
            return true;
        }

        private bool TryValidateRefreshItemFacts(
            C1FormalRealtimeBattleItemRefreshRequest candidate,
            out C1FormalRealtimeBattleError error)
        {
            if (string.IsNullOrEmpty(candidate.itemBattleInputCanonical)
                || string.IsNullOrEmpty(
                    candidate.expectedItemSessionCanonicalSignature)
                || string.IsNullOrEmpty(
                    candidate.expectedItemCatalogCanonicalSignature)
                || candidate.activeItemFacts == null
                || candidate.activeItemFacts.Count >
                    C1FormalRealtimeBattleSessionContract
                        .MaxCumulativeItemInstanceCount
                || candidate.activeItemFacts.Any(fact => fact == null)
                || candidate.activeItemFacts.Any(fact =>
                    string.IsNullOrEmpty(fact.itemInstanceId))
                || candidate.activeItemFacts.Select(fact => fact.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count()
                != candidate.activeItemFacts.Count
                || candidate.activeItemFacts.Select(fact => fact.sourceId)
                    .Distinct(StringComparer.Ordinal).Count()
                != candidate.activeItemFacts.Count)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.RefreshInputRejected,
                    "The refreshed Item input is missing or contains duplicate facts.");
                return false;
            }

            if (!TryValidateNianCapacity(
                    candidate.nianCapacityFact,
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    candidate.itemBattleInputCanonical,
                    candidate.nianCapacityFact == null
                        ? string.Empty
                        : candidate.nianCapacityFact
                            .arrangementCanonicalSignature,
                    out error)
                || request == null
                || request.nianCapacityFact == null
                || !EqualsOrdinal(
                    candidate.nianCapacityFact.sourceItemInstanceId,
                    request.nianCapacityFact.sourceItemInstanceId)
                || !EqualsOrdinal(
                    candidate.nianCapacityFact.resourceKey,
                    request.nianCapacityFact.resourceKey)
                || candidate.nianCapacityFact.initialNian
                    != request.nianCapacityFact.initialNian
                || candidate.nianCapacityFact.maxNian
                    != request.nianCapacityFact.maxNian
                || candidate.nianCapacityFact.generationAmount
                    != request.nianCapacityFact.generationAmount
                || candidate.nianCapacityFact.generationIntervalMilliseconds
                    != request.nianCapacityFact
                        .generationIntervalMilliseconds)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.NianCapacityRejected,
                    "The refreshed formal Nian capacity changed its source or bounds.");
                return false;
            }

            if (!EqualsOrdinal(
                    candidate.expectedItemCatalogCanonicalSignature,
                    request.expectedItemCatalogCanonical)
                || !EqualsOrdinal(
                    candidate.expectedItemCatalogCanonicalSignature,
                    CanonicalItemCatalogContract.CatalogId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ItemCatalogMismatch,
                    "The refreshed Item facts do not match the active Item catalog.");
                return false;
            }

            foreach (C1FormalRealtimeBattleItemFactSnapshot item in
                     candidate.activeItemFacts)
            {
                if (!IsCanonicalItemFactValid(
                        item,
                        candidate.nianCapacityFact.resourceKey))
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes.RefreshInputRejected,
                        "A refreshed active Item fact is unknown, unlit, or stale.");
                    return false;
                }
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private bool TryValidateRefreshPool15Facts(
            C1FormalRealtimeBattleItemRefreshRequest candidate,
            out C1FormalRealtimeBattleError error)
        {
            if (!candidate.replacePool15ActiveFacts)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.None,
                    string.Empty);
                return true;
            }

            C1FormalRealtimeBattleSessionRequest pool15Candidate =
                new C1FormalRealtimeBattleSessionRequest(
                    request.schemaId,
                    request.productContext,
                    request.chapterId,
                    request.stageId,
                    request.balanceProfileId,
                    request.encounterVariantId,
                    request.launchId,
                    request.launchGeneration,
                    request.sessionId,
                    request.sessionToken,
                    request.resetGeneration,
                    candidate.itemBattleInputCanonical,
                    request.arrangementCanonicalSignature,
                    request.expectedItemCatalogCanonical,
                    request.expectedEnemyCatalogCanonical,
                    candidate.activeItemFacts,
                    candidate.pool15ItemInputCanonical,
                    candidate.pool15PoolCanonicalSignature,
                    candidate.pool15SourceItemCatalogCanonicalSignature,
                    candidate.pool15CandidateProfileId,
                    candidate.pool15CandidateDataMaturity,
                    candidate.pool15CandidateProfileCanonicalSignature,
                    candidate.activePool15ItemFacts,
                    candidate.nianCapacityFact);
            return TryValidatePool15RequestFacts(pool15Candidate, out error);
        }

        private bool TryValidateActiveEnvelope(
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            out C1FormalRealtimeBattleError error)
        {
            if (!active || request == null)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    "No active live Battle Session exists.");
                return false;
            }

            if (!EqualsOrdinal(sessionId, request.sessionId)
                || !EqualsOrdinal(sessionToken, request.sessionToken))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SessionEnvelopeMismatch,
                    "The tick envelope does not own the active Session.");
                return false;
            }

            if (expectedSessionGeneration != sessionGeneration)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.SessionGenerationStale,
                    "The tick sessionGeneration is stale.");
                return false;
            }

            if (expectedResetGeneration != currentResetGeneration)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ResetGenerationMismatch,
                    "The tick resetGeneration is stale.");
                return false;
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private static Pool15SourcePlan CreatePool15SourcePlan(
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
            C1CampaignLootItemProfileSnapshot descriptor,
            C1CampaignLootPool15BattleCandidateProfile candidate)
        {
            if (fact == null
                || descriptor == null
                || candidate == null
                || !EqualsOrdinal(
                    fact.descriptorCanonicalSignature,
                    descriptor.profileCanonicalSignature)
                || !EqualsOrdinal(
                    fact.candidateProfileCanonicalSignature,
                    candidate.canonicalSignature))
            {
                return null;
            }

            return new Pool15SourcePlan(fact, descriptor, candidate);
        }

        private bool TryValidateActionProgressRelations(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            IReadOnlyList<Pool15SourcePlan> plans,
            out C1FormalRealtimeBattleError error)
        {
            IReadOnlyList<C1FormalRealtimeBattleActionProgressRelationSnapshot>
                relations = eventRequest.progressRelations;
            if (relations == null
                || relations.Any(row => row == null)
                || relations.Select(row => row.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count()
                != relations.Count)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressRelationRejected,
                    "Progress relations are null or duplicated.");
                return false;
            }

            Pool15SourcePlan[] advancePlans = plans.Where(
                    IsAdvanceItemTriggerProgressPlan)
                .ToArray();
            if (relations.Count != advancePlans.Length)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressRelationRejected,
                    "I022 requires exactly one relation and unrelated relations are rejected.");
                return false;
            }

            foreach (Pool15SourcePlan plan in advancePlans)
            {
                if (relations.Count(row => EqualsOrdinal(
                        row.sourceItemInstanceId,
                        plan.Fact.itemInstanceId)) != 1)
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes
                            .ActionProgressRelationRejected,
                        "The I022 source does not own exactly one relation.");
                    return false;
                }
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private bool TryCreateActionProgressPlan(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            C1CampaignLootPool15BattleEffectApplication application,
            Pool15SourcePlan sourcePlan,
            out ActionProgressPlan progressPlan,
            out C1FormalRealtimeBattleError error)
        {
            progressPlan = null;
            if (!EqualsOrdinal(application.nativeUnitId, "turn"))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.None,
                    string.Empty);
                return true;
            }

            if (application.requestedAmount != 1L
                || application.appliedAmount != 1L)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressUnsupported,
                    "Discrete progress operators require exactly one native turn step.");
                return false;
            }

            if (IsAdvanceItemTriggerProgressPlan(sourcePlan))
            {
                C1FormalRealtimeBattleActionProgressRelationSnapshot relation =
                    eventRequest.progressRelations.Single(row => EqualsOrdinal(
                        row.sourceItemInstanceId,
                        sourcePlan.Fact.itemInstanceId));
                return TryCreateItemTriggerProgressPlan(
                    sourcePlan,
                    relation,
                    out progressPlan,
                    out error);
            }

            if (IsDelayEnemyCastProgressPlan(sourcePlan))
            {
                return TryCreateEnemyCastProgressPlan(
                    sourcePlan,
                    out progressPlan,
                    out error);
            }

            error = Error(
                C1FormalRealtimeBattleErrorCodes.ActionProgressUnsupported,
                "The native turn effect has no accepted discrete Battle operator.");
            return false;
        }

        private bool TryCreateItemTriggerProgressPlan(
            Pool15SourcePlan sourcePlan,
            C1FormalRealtimeBattleActionProgressRelationSnapshot relation,
            out ActionProgressPlan progressPlan,
            out C1FormalRealtimeBattleError error)
        {
            progressPlan = null;
            bool relationEnvelopeAccepted = relation != null
                && !string.IsNullOrEmpty(boundItemBattleInputCanonical)
                && !string.IsNullOrEmpty(
                    boundArrangementCanonicalSignature)
                && EqualsOrdinal(
                    relation.itemBattleInputCanonical,
                    boundItemBattleInputCanonical)
                && EqualsOrdinal(
                    relation.arrangementCanonicalSignature,
                    boundArrangementCanonicalSignature)
                && EqualsOrdinal(
                    relation.sourceItemInstanceId,
                    sourcePlan.Fact.itemInstanceId)
                && EqualsOrdinal(
                    relation.relationId,
                    "adjacent_lit_item")
                && relation.eligibleTargetSourceIds != null
                && relation.eligibleTargetSourceIds.Count > 0
                && relation.eligibleTargetSourceIds.All(
                    value => !string.IsNullOrEmpty(value))
                && relation.eligibleTargetSourceIds.Distinct(
                        StringComparer.Ordinal).Count()
                    == relation.eligibleTargetSourceIds.Count
                && !relation.eligibleTargetSourceIds.Contains(
                    sourcePlan.Fact.itemInstanceId,
                    StringComparer.Ordinal);
            if (!relationEnvelopeAccepted)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressRelationRejected,
                    "The I022 adjacency relation is missing, stale, self-targeting, or duplicated.");
                return false;
            }

            List<MutableAction> candidates = new List<MutableAction>();
            foreach (string sourceId in relation.eligibleTargetSourceIds)
            {
                if (!activeItemFacts.Any(fact => EqualsOrdinal(
                        fact.sourceId,
                        sourceId)))
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes
                            .ActionProgressTargetRejected,
                        "An I022 candidate is not a current placed and lit Item fact.");
                    return false;
                }

                MutableAction[] owned = actions.Where(action =>
                        !action.Executed
                        && action.DueBattleTimeMs > battleTimeMs
                        && EqualsOrdinal(
                            action.ActionKind,
                            C1FormalRealtimeBattleScheduledActionKinds
                                .ItemTrigger)
                        && EqualsOrdinal(action.SourceId, sourceId))
                    .ToArray();
                if (owned.Length != 1 || owned[0].IntervalMs <= 0L)
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes
                            .ActionProgressTargetRejected,
                        "An I022 candidate must own exactly one future Item trigger action.");
                    return false;
                }

                candidates.Add(owned[0]);
            }

            MutableAction target = candidates
                .OrderBy(action => action.DueBattleTimeMs)
                .ThenBy(action => action.SourceStableOrder)
                .ThenBy(action => action.StableOrder)
                .First();
            if (!TryResolveDiscreteProgressDueTime(
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .AdvanceItemTriggerProgressStep,
                    "turn",
                    battleTimeMs,
                    target.DueBattleTimeMs,
                    target.IntervalMs,
                    out long _))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressAtomicityRejected,
                    "The I022 target due time or source interval is invalid.");
                return false;
            }

            progressPlan = new ActionProgressPlan(
                C1FormalRealtimeBattleActionProgressOperatorIds
                    .AdvanceItemTriggerProgressStep,
                sourcePlan.Fact.itemInstanceId,
                target,
                relation.canonicalSignature);
            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private bool TryCreateEnemyCastProgressPlan(
            Pool15SourcePlan sourcePlan,
            out ActionProgressPlan progressPlan,
            out C1FormalRealtimeBattleError error)
        {
            progressPlan = null;
            MutableAction target = actions.Where(action =>
                    !action.Executed
                    && action.DueBattleTimeMs > battleTimeMs
                    && IsCastProgressCapable(action)
                    && action.IntervalMs > 0L)
                .OrderBy(action => action.DueBattleTimeMs)
                .ThenBy(action => action.SourceStableOrder)
                .ThenBy(action => action.StableOrder)
                .FirstOrDefault();
            if (target == null)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressTargetRejected,
                    "I025 requires one future cast-progress-capable enemy action.");
                return false;
            }

            if (!TryResolveDiscreteProgressDueTime(
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .DelayEnemyCastProgressStep,
                    "turn",
                    battleTimeMs,
                    target.DueBattleTimeMs,
                    target.IntervalMs,
                    out long _))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressAtomicityRejected,
                    "The I025 target due time or source interval is invalid.");
                return false;
            }

            progressPlan = new ActionProgressPlan(
                C1FormalRealtimeBattleActionProgressOperatorIds
                    .DelayEnemyCastProgressStep,
                sourcePlan.Fact.itemInstanceId,
                target,
                string.Empty);
            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private static bool IsAdvanceItemTriggerProgressPlan(
            Pool15SourcePlan plan)
        {
            return plan != null
                && EqualsOrdinal(plan.Descriptor.familyId,
                    "POSITIONAL_ADJACENCY")
                && EqualsOrdinal(plan.Descriptor.operationId, "ReduceFlat")
                && EqualsOrdinal(plan.Descriptor.targetStatId, "cooldown")
                && EqualsOrdinal(plan.Descriptor.nativeUnitId, "turn")
                && EqualsOrdinal(
                    plan.Descriptor.conditionId,
                    "adjacent_lit_item");
        }

        private static bool IsDelayEnemyCastProgressPlan(Pool15SourcePlan plan)
        {
            return plan != null
                && EqualsOrdinal(plan.Descriptor.familyId, "ENEMY_CONTROL")
                && EqualsOrdinal(plan.Descriptor.operationId, "ReduceFlat")
                && EqualsOrdinal(
                    plan.Descriptor.targetStatId,
                    "castProgress")
                && EqualsOrdinal(plan.Descriptor.nativeUnitId, "turn")
                && EqualsOrdinal(
                    plan.Descriptor.conditionId,
                    "target_casting");
        }

        private static bool IsCastProgressCapable(MutableAction action)
        {
            return action != null
                && EqualsOrdinal(
                    action.ActionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave);
        }

        private bool TryPreflightActionProgressCommit(
            ActionProgressPlan plan,
            out MutableAction target,
            out long afterDueBattleTimeMs,
            out C1FormalRealtimeBattleError error)
        {
            target = actions.SingleOrDefault(action => EqualsOrdinal(
                action.ActionId,
                plan.TargetActionId));
            afterDueBattleTimeMs = 0L;
            if (target == null
                || target.Executed
                || target.DueBattleTimeMs < battleTimeMs
                || target.DueBattleTimeMs !=
                    plan.ExpectedBeforeDueBattleTimeMs
                || target.IntervalMs != plan.SourceNativeIntervalMs
                || !EqualsOrdinal(target.ActionKind, plan.TargetActionKind)
                || !EqualsOrdinal(target.SourceId, plan.TargetSourceId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ActionProgressStale,
                    "The selected pending action changed after progress preflight.");
                return false;
            }

            bool kindAccepted = EqualsOrdinal(
                    plan.OperatorId,
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .AdvanceItemTriggerProgressStep)
                ? EqualsOrdinal(
                    target.ActionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger)
                : EqualsOrdinal(
                    plan.OperatorId,
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .DelayEnemyCastProgressStep)
                    && IsCastProgressCapable(target);
            if (!kindAccepted
                || !TryResolveDiscreteProgressDueTime(
                    plan.OperatorId,
                    "turn",
                    battleTimeMs,
                    target.DueBattleTimeMs,
                    target.IntervalMs,
                    out afterDueBattleTimeMs))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes
                        .ActionProgressAtomicityRejected,
                    "The pending action cannot accept one native progress step.");
                return false;
            }

            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private bool TryResolvePool15Target(
            C1CampaignLootItemProfileSnapshot descriptor,
            out string targetIdentity,
            out int targetStableOrder,
            out long? percentageBaseValueUnits,
            out string error)
        {
            bool enemyTarget = EqualsOrdinal(
                    descriptor.familyId,
                    "TARGET_SPREAD")
                || EqualsOrdinal(descriptor.familyId, "ENEMY_CONTROL");
            MutableActor actor = FirstLivingActor();
            if (enemyTarget)
            {
                if (actor == null)
                {
                    targetIdentity = string.Empty;
                    targetStableOrder = -1;
                    percentageBaseValueUnits = null;
                    error = "A live enemy target is required.";
                    return false;
                }

                targetIdentity = actor.Source.actorBalanceId;
                targetStableOrder = actor.StableOrder;
            }
            else
            {
                targetIdentity = C1FormalRealtimeBattleSessionContract
                    .PlayerActorId;
                targetStableOrder = -1;
            }

            percentageBaseValueUnits = null;
            if (EqualsOrdinal(descriptor.nativeUnitId, "basisPoint"))
            {
                if (EqualsOrdinal(descriptor.targetStatId, "guard"))
                {
                    percentageBaseValueUnits =
                        C1FormalRealtimeBattleSessionContract
                            .StartingPlayerMaxHp;
                }
                else if (EqualsOrdinal(descriptor.targetStatId, "damage")
                         || EqualsOrdinal(
                             descriptor.targetStatId,
                             "break"))
                {
                    if (actor == null)
                    {
                        error = "A live actor percentage base is required.";
                        return false;
                    }

                    percentageBaseValueUnits = actor.CurrentHp;
                }
                else
                {
                    error = "No authoritative percentage base mapping exists.";
                    return false;
                }
            }

            error = string.Empty;
            return !string.IsNullOrEmpty(targetIdentity)
                && !string.IsNullOrEmpty(descriptor.targetScopeId);
        }

        private static bool TryMapPool15Effect(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            stateKey = string.Empty;
            error = string.Empty;
            if (application == null)
            {
                error = "A Foundation application is required.";
                return false;
            }

            switch (application.familyId)
            {
                case "DIRECT_TEMPO":
                    return TryMapDirectTempo(application, out stateKey, out error);
                case "TARGET_SPREAD":
                    return TryMapTargetSpread(application, out stateKey, out error);
                case "GUARD_SUSTAIN":
                    return TryMapGuardSustain(application, out stateKey, out error);
                case "ENEMY_CONTROL":
                    return TryMapEnemyControl(application, out stateKey, out error);
                case "POSITIONAL_ADJACENCY":
                    return TryMapPositionalAdjacency(
                        application,
                        out stateKey,
                        out error);
                default:
                    error = "The Foundation family has no formal mapping.";
                    return false;
            }
        }

        private static bool TryMapDirectTempo(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            if (EffectShape(application, "Refund", "nian", "point"))
            {
                stateKey = "player.directTempo.nianRefund";
                error = string.Empty;
                return true;
            }
            if (EffectShape(application, "AddFlat", "burn", "stack"))
            {
                stateKey = "player.directTempo.burnStacks";
                error = string.Empty;
                return true;
            }
            if (EffectShape(
                    application,
                    "AddPercent",
                    "damage",
                    "basisPoint"))
            {
                stateKey = "player.directTempo.damageModifier";
                error = string.Empty;
                return true;
            }

            stateKey = string.Empty;
            error = "DIRECT_TEMPO semantic mapping is unsupported.";
            return false;
        }

        private static bool TryMapTargetSpread(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            if (EffectShape(
                    application,
                    "ExtraTarget",
                    "targetCount",
                    "count"))
            {
                stateKey = "enemy.targetSpread.targetCount";
                error = string.Empty;
                return true;
            }
            if (EffectShape(application, "Convert", "burn", "stack"))
            {
                stateKey = "enemy.targetSpread.convertedBurn";
                error = string.Empty;
                return true;
            }

            stateKey = string.Empty;
            error = "TARGET_SPREAD semantic mapping is unsupported.";
            return false;
        }

        private static bool TryMapGuardSustain(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            if (EffectShape(application, "AddFlat", "guard", "flat"))
            {
                stateKey = "player.guard";
                error = string.Empty;
                return true;
            }
            if (EffectShape(application, "ExtraTrigger", "heal", "flat"))
            {
                stateKey = "player.guardSustain.healExtraTrigger";
                error = string.Empty;
                return true;
            }

            stateKey = string.Empty;
            error = "GUARD_SUSTAIN semantic mapping is unsupported.";
            return false;
        }

        private static bool TryMapEnemyControl(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            if (EffectShape(
                    application,
                    "AddPercent",
                    "break",
                    "basisPoint"))
            {
                stateKey = "enemy.control.breakModifier";
                error = string.Empty;
                return true;
            }
            if (EffectShape(
                    application,
                    "ReduceFlat",
                    "castProgress",
                    "turn"))
            {
                stateKey = "enemy.control.castDelayTurns";
                error = string.Empty;
                return true;
            }
            if (EffectShape(
                    application,
                    "Override",
                    "targeting",
                    "count"))
            {
                stateKey = "enemy.control.targetingOverride";
                error = string.Empty;
                return true;
            }

            stateKey = string.Empty;
            error = "ENEMY_CONTROL semantic mapping is unsupported.";
            return false;
        }

        private static bool TryMapPositionalAdjacency(
            C1CampaignLootPool15BattleEffectApplication application,
            out string stateKey,
            out string error)
        {
            if (EffectShape(
                    application,
                    "ReduceFlat",
                    "nianCost",
                    "point"))
            {
                stateKey = "player.positional.nianCostReduction";
                error = string.Empty;
                return true;
            }
            if (EffectShape(
                    application,
                    "AddPercent",
                    "guard",
                    "basisPoint"))
            {
                stateKey = "player.positional.guardModifier";
                error = string.Empty;
                return true;
            }
            if (EffectShape(
                    application,
                    "ReduceFlat",
                    "cooldown",
                    "turn"))
            {
                stateKey = "player.positional.cooldownReductionTurns";
                error = string.Empty;
                return true;
            }

            stateKey = string.Empty;
            error = "POSITIONAL_ADJACENCY semantic mapping is unsupported.";
            return false;
        }

        private static bool EffectShape(
            C1CampaignLootPool15BattleEffectApplication application,
            string operationId,
            string targetStatId,
            string nativeUnitId)
        {
            return EqualsOrdinal(application.operationId, operationId)
                && EqualsOrdinal(application.targetStatId, targetStatId)
                && EqualsOrdinal(application.nativeUnitId, nativeUnitId);
        }

        private MutableAction SchedulePool15Effect(
            PendingPool15Application pending)
        {
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                pending.Application.resolveBattleTimeMs,
                C1FormalRealtimeBattleScheduledActionKinds.Pool15EffectResolve,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                pending.Application.sourceItemInstanceId,
                0,
                0L,
                checked((int)pending.Application.sequence),
                false,
                false,
                string.Empty,
                string.Empty);
            AddAction(action);
            return action;
        }

        private MutableAction ScheduleItem(
            C1FormalRealtimeBattleItemFactSnapshot item,
            int itemOrder,
            long dueBattleTimeMs)
        {
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                item.sourceId,
                item.directDamage,
                item.cooldownMs,
                itemOrder,
                false,
                false,
                string.Empty,
                string.Empty);
            AddAction(action);
            return action;
        }

        private long NextNianAvailabilityTime(long failedAtBattleTimeMs)
        {
            if (nextNianGenerationAtBattleTimeMs > failedAtBattleTimeMs)
            {
                return nextNianGenerationAtBattleTimeMs;
            }

            return checked(
                failedAtBattleTimeMs
                + nianCapacity.generationIntervalMilliseconds);
        }

        private MutableAction ScheduleEnemy(
            C1FormalRealtimeBattleResolvedAttackWave wave,
            MutableActor source,
            long dueBattleTimeMs,
            int? requestedDamageOverride = null)
        {
            int requestedDamage = requestedDamageOverride
                ?? ResolveInitialEnemyActionDamage(wave, source);
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                source.Source.actorBalanceId,
                requestedDamage,
                SecondsToMilliseconds(wave.intervalSeconds),
                source.StableOrder,
                false,
                false,
                string.Empty,
                string.Empty,
                wave.effectRequestKey,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1);
            AddAction(action);
            return action;
        }

        private MutableAction ScheduleCanonicalEnemy(
            MutableActor source,
            long dueBattleTimeMs,
            int? requestedDamageOverride = null)
        {
            int requestedDamage = requestedDamageOverride
                ?? source.Source.primaryAttackDamage;
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                source.Source.actorBalanceId,
                requestedDamage,
                source.Source.primaryAttackIntervalMilliseconds,
                source.StableOrder,
                false,
                false,
                string.Empty,
                string.Empty,
                source.Source.primaryAttackEffectRequestKey,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1);
            AddAction(action);
            return action;
        }

        private MutableAction ScheduleEnemySkill(
            MutableActor source,
            C1FormalEnemySkillDefinition skill,
            long dueBattleTimeMs)
        {
            int requestedDamage = skill.Effects
                .Where(effect => effect != null
                    && EqualsOrdinal(
                        effect.EffectKind,
                        C1FormalEnemySkillEffectKinds.DirectDamage))
                .Sum(effect => effect.FlatValue);
            bool targetsPlayer = EqualsOrdinal(
                skill.TargetSelector,
                C1FormalEnemySkillTargetSelectors.Player);
            MutableAction action = new MutableAction(
                NewActionId(),
                actionSequence,
                dueBattleTimeMs,
                C1FormalRealtimeBattleScheduledActionKinds.EnemySkill,
                C1FormalRealtimeBattleSessionContract.EnemyOwner,
                source.Source.actorBalanceId,
                requestedDamage,
                skill.CooldownMilliseconds,
                source.StableOrder,
                false,
                false,
                string.Empty,
                string.Empty,
                skill.SkillId,
                targetsPlayer
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : source.Source.actorBalanceId,
                targetsPlayer ? -1 : source.StableOrder);
            AddAction(action);
            return action;
        }

        private int ResolveInitialEnemyActionDamage(
            C1FormalRealtimeBattleResolvedAttackWave wave,
            MutableActor source)
        {
            int totalWaveDamage = decimal.ToInt32(
                wave.damagePerApplication);
            List<MutableActor> orderedActors = actors
                .OrderBy(actor => actor.StableOrder)
                .ToList();
            int sourceIndex = orderedActors.FindIndex(actor =>
                ReferenceEquals(actor, source));
            if (sourceIndex < 0 || orderedActors.Count == 0)
            {
                throw new InvalidOperationException(
                    C1FormalRealtimeBattleErrorCodes
                        .EnemyActionBattleContractRequired);
            }

            int sharedDamage = totalWaveDamage / orderedActors.Count;
            int remainder = totalWaveDamage % orderedActors.Count;
            return sharedDamage + (sourceIndex < remainder ? 1 : 0);
        }

        private void AddAction(MutableAction action)
        {
            if (actions.Count >=
                C1FormalRealtimeBattleSessionContract.MaxScheduledActionEntries)
            {
                throw new InvalidOperationException(
                    C1FormalRealtimeBattleErrorCodes.EventBufferExceeded);
            }

            actions.Add(action);
        }

        private string NewActionId()
        {
            actionSequence++;
            return request.sessionId + ":action:"
                + actionSequence.ToString("D4", CultureInfo.InvariantCulture);
        }

        private string NewApplicationId()
        {
            applicationSequence++;
            return request.sessionId + ":application:"
                + applicationSequence.ToString("D4", CultureInfo.InvariantCulture);
        }

        private void EmitScheduled(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            MutableAction action)
        {
            bool itemAction = string.Equals(
                    action.ActionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                    StringComparison.Ordinal);
            bool enemyAction = string.Equals(
                action.ActionKind,
                C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                StringComparison.Ordinal);
            bool enemySkill = string.Equals(
                action.ActionKind,
                C1FormalRealtimeBattleScheduledActionKinds.EnemySkill,
                StringComparison.Ordinal);
            C1FormalRealtimeBattleItemFactSnapshot itemOrigin = itemAction
                ? activeItemFacts.FirstOrDefault(fact => EqualsOrdinal(
                    fact.sourceId,
                    action.SourceId))
                : null;
            Emit(
                emitted,
                itemAction
                    ? C1FormalRealtimeBattleCueKinds.ItemTriggerScheduled
                    : enemySkill
                        ? C1FormalRealtimeBattleCueKinds.EnemySkillScheduled
                    : C1FormalRealtimeBattleCueKinds.EnemyAttackScheduled,
                action.SourceOwner,
                action.SourceId,
                enemyAction || enemySkill
                    ? action.TargetActorId
                    : currentTargetActorId,
                enemyAction || enemySkill
                    ? action.TargetStableOrder
                    : FirstLivingActor() == null
                        ? -1
                        : FirstLivingActor().StableOrder,
                playerHp,
                playerHp,
                0,
                0,
                action.RequestedDamage,
                0,
                action.DueBattleTimeMs.ToString(CultureInfo.InvariantCulture),
                40,
                string.Empty,
                itemOrigin == null ? string.Empty : itemOrigin.itemInstanceId,
                itemOrigin == null ? string.Empty : itemOrigin.baseItemId,
                enemyAction
                    ? "ENEMY_ACTION"
                    : enemySkill
                        ? "ENEMY_SKILL"
                        : string.Empty,
                enemyAction || enemySkill
                    ? action.SourceActionId
                    : string.Empty);
        }

        private void EmitTargetChanged(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            MutableActor target,
            string acceptedApplicationEventId)
        {
            EmitTargetChanged(
                emitted,
                target,
                acceptedApplicationEventId,
                (C1FormalRealtimeBattleItemFactSnapshot)null);
        }

        private void EmitTargetChanged(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            MutableActor target,
            string acceptedApplicationEventId,
            C1FormalRealtimeBattleItemFactSnapshot itemOrigin)
        {
            currentTargetActorId = target.Source.actorBalanceId;
            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.TargetChanged,
                C1FormalRealtimeBattleSessionContract.BattleOwner,
                request.sessionId,
                target.Source.actorBalanceId,
                target.StableOrder,
                playerHp,
                playerHp,
                target.CurrentHp,
                target.CurrentHp,
                0,
                0,
                string.Empty,
                85,
                acceptedApplicationEventId,
                itemOrigin == null ? string.Empty : itemOrigin.itemInstanceId,
                itemOrigin == null ? string.Empty : itemOrigin.baseItemId,
                string.Empty,
                string.Empty);
        }

        private void EmitApplicationFeedback(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            C1FormalRealtimeBattleItemFactSnapshot item,
            MutableActor target,
            string applicationId,
            string feedbackKind,
            long requested,
            long applied)
        {
            if (item == null
                || string.IsNullOrWhiteSpace(feedbackKind)
                || requested <= 0L)
            {
                return;
            }

            string payload;
            if (string.Equals(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Heal,
                    StringComparison.Ordinal))
            {
                payload = "HP +" + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else if (string.Equals(
                         feedbackKind,
                         C1FormalRealtimeBattleApplicationFeedbackKinds.Guard,
                         StringComparison.Ordinal))
            {
                payload = "GUARD +" + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else if (string.Equals(
                         feedbackKind,
                         C1FormalRealtimeBattleApplicationFeedbackKinds.Cleanse,
                         StringComparison.Ordinal))
            {
                payload = "CLEANSE " + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else if (string.Equals(
                         feedbackKind,
                         C1FormalRealtimeBattleApplicationFeedbackKinds.Nian,
                         StringComparison.Ordinal))
            {
                payload = "NIAN +" + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else if (string.Equals(
                         feedbackKind,
                         C1FormalRealtimeBattleApplicationFeedbackKinds.Control,
                         StringComparison.Ordinal))
            {
                payload = "DELAY +" + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else if (string.Equals(
                         feedbackKind,
                         C1FormalRealtimeBattleApplicationFeedbackKinds.Shell,
                         StringComparison.Ordinal))
            {
                payload = "SHELL -" + applied.ToString(
                    CultureInfo.InvariantCulture);
            }
            else
            {
                return;
            }

            Emit(
                emitted,
                C1FormalRealtimeBattleCueKinds.ApplicationFeedbackPayload,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                item.sourceId,
                target == null
                    ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                    : target.Source.actorBalanceId,
                target == null ? -1 : target.StableOrder,
                playerHp,
                playerHp,
                target == null ? 0 : target.CurrentHp,
                target == null ? 0 : target.CurrentHp,
                ToCueAmount(requested),
                ToCueAmount(applied),
                payload,
                66,
                applicationId,
                item.itemInstanceId,
                item.baseItemId,
                "APPLICATION_FEEDBACK",
                feedbackKind,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                deliveryKind:
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind: FeedbackResultForApplication(feedbackKind));
        }

        private static string FeedbackResultForApplication(
            string feedbackKind)
        {
            if (EqualsOrdinal(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Heal))
            {
                return C1FormalRealtimeBattleFeedbackResultKinds.Heal;
            }
            if (EqualsOrdinal(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Guard))
            {
                return C1FormalRealtimeBattleFeedbackResultKinds.GuardGain;
            }
            if (EqualsOrdinal(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Cleanse))
            {
                return C1FormalRealtimeBattleFeedbackResultKinds.Cleanse;
            }
            if (EqualsOrdinal(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Nian))
            {
                return C1FormalRealtimeBattleFeedbackResultKinds.NianGain;
            }
            if (EqualsOrdinal(
                    feedbackKind,
                    C1FormalRealtimeBattleApplicationFeedbackKinds.Control))
            {
                return C1FormalRealtimeBattleFeedbackResultKinds.Control;
            }

            return C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage;
        }

        private void Emit(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId)
        {
            Emit(
                emitted,
                cueKind,
                sourceOwner,
                sourceId,
                targetActorId,
                targetStableOrder,
                playerHpBefore,
                playerHpAfter,
                actorHpBefore,
                actorHpAfter,
                requestedDamage,
                appliedDamage,
                floatPayload,
                presentationPriority,
                acceptedApplicationEventId,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        private void Emit(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string effectFamilyId,
            string effectVariantId,
            int shellBefore = 0,
            int shellAfter = 0,
            int shellAppliedAmount = 0,
            bool shellBroken = false,
            string shellBrokenStateId = "",
            string shellBreakCounterWindowId = "",
            int sourceStableOrder = -1,
            string triggerKind = "",
            string deliveryKind = "",
            string resultKind = "")
        {
            cueSequence++;
            C1FormalRealtimeBattleCue cue = new C1FormalRealtimeBattleCue(
                request.sessionId + ":cue:"
                + cueSequence.ToString("D6", CultureInfo.InvariantCulture),
                cueSequence,
                battleTimeMs,
                cueKind,
                sourceOwner,
                sourceId,
                targetActorId,
                targetStableOrder,
                playerHpBefore,
                playerHpAfter,
                actorHpBefore,
                actorHpAfter,
                requestedDamage,
                appliedDamage,
                floatPayload,
                presentationPriority,
                acceptedApplicationEventId,
                sourceItemInstanceId,
                sourceBaseItemId,
                effectFamilyId,
                effectVariantId,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                string.Empty,
                shellBefore,
                shellAfter,
                shellAppliedAmount,
                shellBroken,
                shellBrokenStateId,
                shellBreakCounterWindowId,
                sourceStableOrder,
                triggerKind,
                deliveryKind,
                resultKind);
            cueLedger.Add(cue);
            if (cueLedger.Count >
                C1FormalRealtimeBattleSessionContract.MaxCueLedgerEntries)
            {
                cueLedger.RemoveAt(0);
            }

            emitted.Add(cue);
        }

        private void EmitNianTransaction(
            ICollection<C1FormalRealtimeBattleCue> emitted,
            C1FormalRealtimeBattleNianTransactionSnapshot transaction,
            string effectFamilyId,
            string effectVariantId)
        {
            cueSequence++;
            C1FormalRealtimeBattleCue cue = new C1FormalRealtimeBattleCue(
                request.sessionId + ":cue:"
                + cueSequence.ToString("D6", CultureInfo.InvariantCulture),
                cueSequence,
                battleTimeMs,
                C1FormalRealtimeBattleCueKinds.NianResourceChanged,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                transaction.sourceItemInstanceId,
                C1FormalRealtimeBattleSessionContract.PlayerActorId,
                -1,
                playerHp,
                playerHp,
                0,
                0,
                0,
                0,
                transaction.transactionKind + "|"
                + transaction.before.ToString(CultureInfo.InvariantCulture)
                + ">" + transaction.after.ToString(
                    CultureInfo.InvariantCulture),
                85,
                transaction.acceptedApplicationEventId,
                transaction.sourceItemInstanceId,
                transaction.sourceBaseItemId,
                effectFamilyId,
                effectVariantId,
                nianCapacity.resourceKey,
                transaction.transactionKind,
                transaction.requestedAmount,
                transaction.appliedAmount,
                transaction.before,
                transaction.after,
                transaction.canonicalSignature,
                triggerKind:
                    C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                deliveryKind: EqualsOrdinal(
                        transaction.transactionKind,
                        C1FormalRealtimeBattleNianTransactionKinds.Generation)
                    ? C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic
                    : C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
                resultKind: EqualsOrdinal(
                        transaction.transactionKind,
                        C1FormalRealtimeBattleNianTransactionKinds.Spend)
                    ? C1FormalRealtimeBattleFeedbackResultKinds.NianSpend
                    : C1FormalRealtimeBattleFeedbackResultKinds.NianGain);
            cueLedger.Add(cue);
            if (cueLedger.Count >
                C1FormalRealtimeBattleSessionContract.MaxCueLedgerEntries)
            {
                cueLedger.RemoveAt(0);
            }

            emitted.Add(cue);
        }

        private C1FormalRealtimeBattleSessionStateSnapshot Snapshot()
        {
            return new C1FormalRealtimeBattleSessionStateSnapshot(
                true,
                terminalResult == null
                    ? C1FormalRealtimeBattleErrorCodes.None
                    : terminalResult.errorCode,
                request == null ? string.Empty : request.sessionId,
                sessionGeneration,
                currentResetGeneration,
                phase,
                paused,
                terminal,
                battleTimeMs,
                PlayerSnapshot(),
                ActorSnapshots(),
                actions.Select(action => action.Snapshot()),
                acceptedItemApplicationCount,
                enemyApplicationCount,
                cueSequence,
                activePool15ItemFacts.Count,
                ActivePool15SourceCanonicalSignature(),
                EffectStateSnapshot(),
                pool15ApplicationLedger.Count,
                pool15CueLedger.Count,
                NianStateSnapshot(),
                BuildStatusSnapshots());
        }

        private IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot>
            BuildStatusSnapshots()
        {
            IEnumerable<C1FormalRealtimeBattleStatusSnapshot> playerRows =
                playerStatuses
                    .Where(status => status != null && status.StackCount > 0)
                    .Select(status => status.Snapshot());
            IEnumerable<C1FormalRealtimeBattleStatusSnapshot> actorRows = actors
                .SelectMany(actor => actor.Statuses)
                .Where(status => !status.Removed)
                .Select(status => status.Snapshot());
            IEnumerable<C1FormalRealtimeBattleStatusSnapshot> enemySkillRows =
                enemySkillStatuses
                    .Where(status => status != null
                        && !status.Removed
                        && status.StackCount > 0)
                    .Select(status => status.Snapshot());
            return Array.AsReadOnly(playerRows
                .Concat(actorRows)
                .Concat(enemySkillRows)
                .OrderBy(status => status.targetStableOrder)
                .ThenBy(status => status.statusKey, StringComparer.Ordinal)
                .ToArray());
        }

        private C1FormalRealtimeBattleNianStateSnapshot NianStateSnapshot()
        {
            return nianCapacity == null
                ? null
                : new C1FormalRealtimeBattleNianStateSnapshot(
                    nianCapacity.resourceKey,
                    nianCapacity.initialNian,
                    nianCapacity.maxNian,
                    currentNian,
                    nianCapacity.generationAmount,
                    nianCapacity.generationIntervalMilliseconds,
                    terminal ? 0L : nextNianGenerationAtBattleTimeMs,
                    nianCapacity.sourceItemInstanceId,
                    nianCapacity.sourceBaseItemId,
                    nianCapacity.canonicalSignature,
                    nianTransactionLedger.Count);
        }

        private string ActivePool15SourceCanonicalSignature()
        {
            return C1FormalRealtimeBattleCanonical.Hash(string.Join(
                "\n",
                activePool15ItemFacts
                    .OrderBy(
                        fact => fact.itemInstanceId,
                        StringComparer.Ordinal)
                    .ThenBy(
                        fact => fact.baseItemId,
                        StringComparer.Ordinal)
                    .Select(fact => fact.canonicalSignature)));
        }

        private C1FormalRealtimeBattlePlayerSnapshot PlayerSnapshot()
        {
            return new C1FormalRealtimeBattlePlayerSnapshot(
                C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp,
                playerHp,
                playerGuard);
        }

        private C1FormalRealtimeBattleEffectStateSnapshot EffectStateSnapshot()
        {
            List<C1FormalRealtimeBattleEffectStateValueSnapshot> values =
                new List<C1FormalRealtimeBattleEffectStateValueSnapshot>();
            foreach (KeyValuePair<string, long> pair in effectState)
            {
                string[] parts = pair.Key.Split('\u001f');
                if (parts.Length != 3)
                {
                    continue;
                }

                values.Add(
                    new C1FormalRealtimeBattleEffectStateValueSnapshot(
                        parts[0],
                        parts[1],
                        parts[2],
                        pair.Value));
            }

            return new C1FormalRealtimeBattleEffectStateSnapshot(
                playerGuard,
                values);
        }

        private IReadOnlyList<C1FormalRealtimeBattleActorSnapshot> ActorSnapshots()
        {
            return actors.Select(actor => actor.Snapshot()).ToArray();
        }

        private C1FormalRealtimeBattleTickResult AcceptedTick(
            IEnumerable<C1FormalRealtimeBattleCue> emitted)
        {
            return new C1FormalRealtimeBattleTickResult(
                terminalResult == null || terminalResult.accepted,
                terminalResult == null
                    ? Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty)
                    : terminalResult.error,
                Snapshot(),
                emitted,
                terminalResult);
        }

        private C1FormalRealtimeBattleSessionStartSnapshot RejectedStart(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleSessionStartSnapshot(
                false,
                Error(code, message),
                null,
                Array.Empty<C1FormalRealtimeBattleCue>());
        }

        private C1FormalRealtimeBattleTickResult RejectedTick(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleTickResult(
                false,
                Error(code, message),
                active ? Snapshot() : null,
                Array.Empty<C1FormalRealtimeBattleCue>(),
                terminalResult);
        }

        private C1FormalRealtimeBattleItemRefreshResult RejectedRefresh(
            C1FormalRealtimeBattleItemRefreshRequest refreshRequest,
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleItemRefreshResult(
                false,
                Error(code, message),
                request == null
                    ? refreshRequest == null ? string.Empty : refreshRequest.sessionId
                    : request.sessionId,
                sessionGeneration,
                currentResetGeneration,
                battleTimeMs,
                refreshRequest == null
                    ? string.Empty
                    : refreshRequest.itemBattleInputCanonical,
                string.Empty,
                0,
                0,
                0,
                activeItemFacts.Count,
                active ? Snapshot() : null,
                Array.Empty<C1FormalRealtimeBattleCue>());
        }

        private C1FormalRealtimeBattlePool15EventResult RejectedPool15Event(
            C1FormalRealtimeBattlePool15EventRequest eventRequest,
            string code,
            string message)
        {
            return new C1FormalRealtimeBattlePool15EventResult(
                false,
                Error(code, message),
                eventRequest == null ? string.Empty : eventRequest.eventId,
                0,
                0,
                active ? Snapshot() : null,
                Array.Empty<
                    C1FormalRealtimeBattleEffectApplicationSnapshot>(),
                Array.Empty<C1FormalRealtimeBattleEffectCue>());
        }

        private MutableActor FirstLivingActor()
        {
            return actors.FirstOrDefault(actor => actor.CurrentHp > 0);
        }

        private MutableActor FindActor(
            string actorBalanceId,
            int stableOrder)
        {
            return actors.FirstOrDefault(actor =>
                actor.StableOrder == stableOrder
                && EqualsOrdinal(
                    actor.Source.actorBalanceId,
                    actorBalanceId));
        }

        private int TotalEnemyHp()
        {
            return actors.Sum(actor => Math.Max(0, actor.CurrentHp));
        }

        private static int ActionPriority(string actionKind)
        {
            if (string.Equals(
                    actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .Pool15EffectResolve,
                    StringComparison.Ordinal))
            {
                return 0;
            }

            if (string.Equals(
                actionKind,
                C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                StringComparison.Ordinal))
            {
                return 1;
            }

            if (string.Equals(
                    actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.StatusTick,
                    StringComparison.Ordinal)
                || string.Equals(
                    actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .EnemySkillStatusTick,
                    StringComparison.Ordinal))
            {
                return 2;
            }

            if (string.Equals(
                    actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.StatusExpire,
                    StringComparison.Ordinal)
                || string.Equals(
                    actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds
                        .EnemySkillStatusExpire,
                    StringComparison.Ordinal))
            {
                return 3;
            }

            return 5;
        }

        private static string EffectStorageKey(
            string stateKey,
            string targetIdentity,
            string nativeUnitId)
        {
            return (stateKey ?? string.Empty) + "\u001f"
                + (targetIdentity ?? string.Empty) + "\u001f"
                + (nativeUnitId ?? string.Empty);
        }

        private static int ToCueAmount(long value)
        {
            if (value > int.MaxValue)
            {
                return int.MaxValue;
            }
            if (value < int.MinValue)
            {
                return int.MinValue;
            }

            return (int)value;
        }

        private static long SecondsToMilliseconds(decimal seconds)
        {
            decimal milliseconds = seconds * 1000m;
            if (milliseconds < 0m
                || decimal.Truncate(milliseconds) != milliseconds
                || milliseconds > long.MaxValue)
            {
                throw new InvalidOperationException("NON_INTEGER_BATTLE_TIME");
            }

            return decimal.ToInt64(milliseconds);
        }

        private static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static C1FormalRealtimeBattleError Error(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleError(code, message);
        }

        private sealed class Pool15SourcePlan
        {
            internal Pool15SourcePlan(
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
                C1CampaignLootItemProfileSnapshot descriptor,
                C1CampaignLootPool15BattleCandidateProfile candidate)
            {
                Fact = fact;
                Descriptor = descriptor;
                Candidate = candidate;
            }

            internal C1FormalRealtimeBattlePool15ActiveItemFactSnapshot Fact
            {
                get;
            }
            internal C1CampaignLootItemProfileSnapshot Descriptor { get; }
            internal C1CampaignLootPool15BattleCandidateProfile Candidate
            {
                get;
            }
            internal string TargetIdentity { get; set; } = string.Empty;
            internal int TargetStableOrder { get; set; } = -1;
            internal long? PercentageBaseValueUnits { get; set; }
        }

        private sealed class PendingPool15Application
        {
            internal PendingPool15Application(
                C1CampaignLootPool15BattleEffectApplication application,
                Pool15SourcePlan plan,
                string stateKey,
                string storageKey,
                ActionProgressPlan progressPlan)
            {
                Application = application;
                Plan = plan;
                StateKey = stateKey ?? string.Empty;
                StorageKey = storageKey ?? string.Empty;
                ProgressPlan = progressPlan;
            }

            internal C1CampaignLootPool15BattleEffectApplication Application
            {
                get;
            }
            internal Pool15SourcePlan Plan { get; }
            internal string StateKey { get; }
            internal string StorageKey { get; }
            internal ActionProgressPlan ProgressPlan { get; }
        }

        private sealed class ActionProgressPlan
        {
            internal ActionProgressPlan(
                string operatorId,
                string sourceItemInstanceId,
                MutableAction target,
                string relationCanonicalSignature)
            {
                OperatorId = operatorId ?? string.Empty;
                SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
                TargetActionId = target == null
                    ? string.Empty
                    : target.ActionId;
                TargetActionKind = target == null
                    ? string.Empty
                    : target.ActionKind;
                TargetSourceId = target == null
                    ? string.Empty
                    : target.SourceId;
                ExpectedBeforeDueBattleTimeMs = target == null
                    ? -1L
                    : target.DueBattleTimeMs;
                SourceNativeIntervalMs = target == null
                    ? -1L
                    : target.IntervalMs;
                RelationCanonicalSignature =
                    relationCanonicalSignature ?? string.Empty;
            }

            internal string OperatorId { get; }
            internal string SourceItemInstanceId { get; }
            internal string TargetActionId { get; }
            internal string TargetActionKind { get; }
            internal string TargetSourceId { get; }
            internal long ExpectedBeforeDueBattleTimeMs { get; }
            internal long SourceNativeIntervalMs { get; }
            internal string RelationCanonicalSignature { get; }
        }

        private sealed class CanonicalEffectSource
        {
            internal CanonicalEffectSource(
                C1FormalRealtimeBattleItemFactSnapshot item,
                C1FormalRealtimeBattleCanonicalEffectPlan plan,
                long generatedValueUnits = 0L)
            {
                Item = item;
                Plan = plan;
                GeneratedValueUnits = generatedValueUnits;
            }

            internal C1FormalRealtimeBattleItemFactSnapshot Item { get; }
            internal C1FormalRealtimeBattleCanonicalEffectPlan Plan { get; }
            internal long GeneratedValueUnits { get; }
        }

        private sealed class PlayerBattleStatusState
        {
            internal PlayerBattleStatusState(
                string statusKey,
                string statusFamilyKey,
                bool cleanseable,
                int maxStack,
                long appliedAtBattleTimeMs)
            {
                StatusKey = statusKey ?? string.Empty;
                StatusFamilyKey = statusFamilyKey ?? string.Empty;
                Cleanseable = cleanseable;
                MaxStack = Math.Max(1, maxStack);
                AppliedAtBattleTimeMs = Math.Max(0L, appliedAtBattleTimeMs);
            }

            internal string StatusKey { get; }
            internal string StatusFamilyKey { get; }
            internal bool Cleanseable { get; }
            internal int MaxStack { get; }
            internal long AppliedAtBattleTimeMs { get; }
            internal int StackCount { get; set; }

            internal C1FormalRealtimeBattleStatusSnapshot Snapshot()
            {
                return new C1FormalRealtimeBattleStatusSnapshot(
                    StatusKey,
                    StatusFamilyKey,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    -1,
                    StackCount,
                    MaxStack,
                    0L,
                    0L,
                    Array.Empty<
                        C1FormalRealtimeBattleStatusContributionSnapshot>());
            }
        }

        private sealed class MutableActor
        {
            internal MutableActor(
                C1FormalRealtimeBattleResolvedActor source,
                int stableOrder)
            {
                Source = source;
                StableOrder = stableOrder;
                CurrentHp = source.maxHp;
                CurrentShell = source.initialShell;
                ShellBroken = source.hasShell && source.initialShell <= 0;
            }

            internal C1FormalRealtimeBattleResolvedActor Source { get; }
            internal int StableOrder { get; }
            internal int CurrentHp { get; set; }
            internal int CurrentShell { get; set; }
            internal bool ShellBroken { get; set; }
            internal List<BattleStatusState> Statuses { get; } =
                new List<BattleStatusState>();

            internal C1FormalRealtimeBattleActorSnapshot Snapshot()
            {
                return new C1FormalRealtimeBattleActorSnapshot(
                    Source.actorBalanceId,
                    Source.contentId,
                    Source.runtimeProfileId,
                    Source.operatorProfileId,
                    StableOrder,
                    Source.occurrenceOrdinal,
                    Source.maxHp,
                    CurrentHp,
                    Source.actionIds,
                    Source.effectRequestKeys,
                    Source.downstreamConditions,
                    Source.canonicalSignature,
                    Source.hasShell,
                    Source.maxShell,
                    CurrentShell,
                    ShellBroken,
                    Source.shellBreakTargetId,
                    ShellBroken ? Source.shellBrokenStateId : string.Empty,
                    ShellBroken
                        ? Source.shellBreakCounterWindowId
                        : string.Empty);
            }
        }

        private sealed class CanonicalStatusApplicationPlan
        {
            internal CanonicalStatusApplicationPlan(
                C1FormalRealtimeBattleItemFactSnapshot item,
                MutableActor target,
                CanonicalItemEffectDefinition effect,
                BattleStatusState existing,
                int tickPotency,
                long expireAtBattleTimeMs,
                bool addsContribution,
                bool refreshes)
            {
                Item = item;
                Target = target;
                Effect = effect;
                Existing = existing;
                TickPotency = tickPotency;
                ExpireAtBattleTimeMs = expireAtBattleTimeMs;
                AddsContribution = addsContribution;
                Refreshes = refreshes;
            }

            internal C1FormalRealtimeBattleItemFactSnapshot Item { get; }
            internal MutableActor Target { get; }
            internal CanonicalItemEffectDefinition Effect { get; }
            internal BattleStatusState Existing { get; }
            internal int TickPotency { get; }
            internal long ExpireAtBattleTimeMs { get; }
            internal bool AddsContribution { get; }
            internal bool Refreshes { get; }
            internal bool HasMutation => Existing == null
                                         || AddsContribution
                                         || Refreshes;
        }

        private sealed class BattleStatusState
        {
            internal BattleStatusState(
                MutableActor target,
                CanonicalItemEffectDefinition effect,
                long nextTickAtBattleTimeMs,
                long expireAtBattleTimeMs)
            {
                Target = target;
                Effect = effect;
                RuntimeId = target.Source.actorBalanceId + "|"
                    + effect.statusKey;
                StatusKey = effect.statusKey;
                StatusFamilyKey = effect.statusFamilyKey;
                MaxStack = effect.stackLimit;
                TickIntervalMs = effect.tickIntervalUnits;
                NextTickAtBattleTimeMs = nextTickAtBattleTimeMs;
                ExpireAtBattleTimeMs = expireAtBattleTimeMs;
            }

            internal string RuntimeId { get; }
            internal string StatusKey { get; }
            internal string StatusFamilyKey { get; }
            internal MutableActor Target { get; }
            internal CanonicalItemEffectDefinition Effect { get; }
            internal int MaxStack { get; }
            internal long TickIntervalMs { get; }
            internal List<BattleStatusContribution> Contributions { get; } =
                new List<BattleStatusContribution>();
            internal int StackCount => Contributions.Count;
            internal long NextTickAtBattleTimeMs { get; set; }
            internal long ExpireAtBattleTimeMs { get; set; }
            internal MutableAction TickAction { get; set; }
            internal MutableAction ExpireAction { get; set; }
            internal bool Removed { get; set; }

            internal C1FormalRealtimeBattleStatusSnapshot Snapshot()
            {
                return new C1FormalRealtimeBattleStatusSnapshot(
                    StatusKey,
                    StatusFamilyKey,
                    Target.Source.actorBalanceId,
                    Target.StableOrder,
                    StackCount,
                    MaxStack,
                    NextTickAtBattleTimeMs,
                    ExpireAtBattleTimeMs,
                    Contributions.Select(value => value.Snapshot()));
            }
        }

        private sealed class BattleStatusContribution
        {
            internal BattleStatusContribution(
                string sourceItemInstanceId,
                string sourceBaseItemId,
                int tickPotency,
                long appliedAtBattleTimeMs)
            {
                SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
                SourceBaseItemId = sourceBaseItemId ?? string.Empty;
                TickPotency = tickPotency;
                AppliedAtBattleTimeMs = appliedAtBattleTimeMs;
            }

            internal string SourceItemInstanceId { get; }
            internal string SourceBaseItemId { get; }
            internal int TickPotency { get; }
            internal long AppliedAtBattleTimeMs { get; }

            internal C1FormalRealtimeBattleStatusContributionSnapshot
                Snapshot()
            {
                return new C1FormalRealtimeBattleStatusContributionSnapshot(
                    SourceItemInstanceId,
                    SourceBaseItemId,
                    TickPotency,
                    AppliedAtBattleTimeMs);
            }
        }

        private sealed class EnemySkillStatusState
        {
            internal EnemySkillStatusState(
                MutableActor targetActor,
                C1FormalEnemyStatusDefinition definition,
                long expireAtBattleTimeMs,
                long appliedAtBattleTimeMs)
            {
                TargetActor = targetActor;
                Definition = definition;
                RuntimeId = (targetActor == null
                        ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                        : targetActor.Source.actorBalanceId)
                    + "|enemy-skill|" + definition.StatusKey;
                ExpireAtBattleTimeMs = expireAtBattleTimeMs;
                AppliedAtBattleTimeMs = appliedAtBattleTimeMs;
            }

            internal string RuntimeId { get; }
            internal MutableActor TargetActor { get; }
            internal C1FormalEnemyStatusDefinition Definition { get; }
            internal string StatusKey => Definition.StatusKey;
            internal string StatusFamilyKey => Definition.StatusFamilyKey;
            internal int MaxStack => Definition.MaxStacks;
            internal int StackCount => Contributions.Count;
            internal long AppliedAtBattleTimeMs { get; }
            internal long NextTickAtBattleTimeMs { get; set; }
            internal long ExpireAtBattleTimeMs { get; set; }
            internal List<EnemySkillStatusContribution> Contributions { get; } =
                new List<EnemySkillStatusContribution>();
            internal MutableAction TickAction { get; set; }
            internal MutableAction ExpireAction { get; set; }
            internal bool Removed { get; set; }

            internal C1FormalRealtimeBattleStatusSnapshot Snapshot()
            {
                return new C1FormalRealtimeBattleStatusSnapshot(
                    StatusKey,
                    StatusFamilyKey,
                    TargetActor == null
                        ? C1FormalRealtimeBattleSessionContract.PlayerActorId
                        : TargetActor.Source.actorBalanceId,
                    TargetActor == null ? -1 : TargetActor.StableOrder,
                    StackCount,
                    MaxStack,
                    NextTickAtBattleTimeMs,
                    ExpireAtBattleTimeMs,
                    Contributions.Select(value => value.Snapshot()));
            }
        }

        private sealed class EnemySkillStatusContribution
        {
            internal EnemySkillStatusContribution(
                string sourceActorId,
                string sourceContentId,
                int sourceStableOrder,
                int tickPotency,
                long appliedAtBattleTimeMs)
            {
                SourceActorId = sourceActorId ?? string.Empty;
                SourceContentId = sourceContentId ?? string.Empty;
                SourceStableOrder = sourceStableOrder;
                TickPotency = tickPotency;
                AppliedAtBattleTimeMs = appliedAtBattleTimeMs;
            }

            internal string SourceActorId { get; }
            internal string SourceContentId { get; }
            internal int SourceStableOrder { get; }
            internal int TickPotency { get; }
            internal long AppliedAtBattleTimeMs { get; }

            internal C1FormalRealtimeBattleStatusContributionSnapshot
                Snapshot()
            {
                return new C1FormalRealtimeBattleStatusContributionSnapshot(
                    SourceActorId,
                    SourceContentId,
                    TickPotency,
                    AppliedAtBattleTimeMs);
            }
        }

        private sealed class MutableAction
        {
            internal MutableAction(
                string actionId,
                int stableOrder,
                long dueBattleTimeMs,
                string actionKind,
                string sourceOwner,
                string sourceId,
                int requestedDamage,
                long intervalMs,
                int sourceStableOrder,
                bool executed,
                bool accepted,
                string diagnosticCode,
                string acceptedApplicationEventId,
                string sourceActionId = "",
                string targetActorId = "",
                int targetStableOrder = -1)
            {
                ActionId = actionId;
                StableOrder = stableOrder;
                DueBattleTimeMs = dueBattleTimeMs;
                ActionKind = actionKind;
                SourceOwner = sourceOwner;
                SourceId = sourceId;
                RequestedDamage = requestedDamage;
                IntervalMs = intervalMs;
                SourceStableOrder = sourceStableOrder;
                Executed = executed;
                Accepted = accepted;
                DiagnosticCode = diagnosticCode;
                AcceptedApplicationEventId = acceptedApplicationEventId;
                SourceActionId = sourceActionId ?? string.Empty;
                TargetActorId = targetActorId ?? string.Empty;
                TargetStableOrder = targetStableOrder;
            }

            internal string ActionId { get; }
            internal int StableOrder { get; }
            internal long DueBattleTimeMs { get; private set; }
            internal string ActionKind { get; }
            internal string SourceOwner { get; }
            internal string SourceId { get; }
            internal int RequestedDamage { get; private set; }
            internal long IntervalMs { get; }
            internal int SourceStableOrder { get; }
            internal bool Executed { get; private set; }
            internal bool Accepted { get; private set; }
            internal string DiagnosticCode { get; private set; }
            internal string AcceptedApplicationEventId { get; private set; }
            internal string SourceActionId { get; }
            internal string TargetActorId { get; }
            internal int TargetStableOrder { get; }

            internal void MarkExecuted(
                bool accepted,
                string diagnosticCode,
                string acceptedApplicationEventId)
            {
                Executed = true;
                Accepted = accepted;
                DiagnosticCode = diagnosticCode ?? string.Empty;
                AcceptedApplicationEventId =
                    acceptedApplicationEventId ?? string.Empty;
            }

            internal bool TryReschedule(
                long expectedBeforeDueBattleTimeMs,
                long afterDueBattleTimeMs)
            {
                if (Executed
                    || DueBattleTimeMs != expectedBeforeDueBattleTimeMs
                    || afterDueBattleTimeMs < 0L)
                {
                    return false;
                }

                DueBattleTimeMs = afterDueBattleTimeMs;
                return true;
            }

            internal bool TryReplaceRequestedDamage(int requestedDamage)
            {
                if (Executed || requestedDamage <= 0)
                {
                    return false;
                }

                RequestedDamage = requestedDamage;
                return true;
            }

            internal C1FormalRealtimeBattleScheduledActionSnapshot Snapshot()
            {
                return new C1FormalRealtimeBattleScheduledActionSnapshot(
                    ActionId,
                    StableOrder,
                    DueBattleTimeMs,
                    ActionKind,
                    SourceOwner,
                    SourceId,
                    RequestedDamage,
                    Executed,
                    Accepted,
                    DiagnosticCode,
                    AcceptedApplicationEventId);
            }
        }

        private sealed class DamageSegment
        {
            internal DamageSegment(
                MutableActor actor,
                int before,
                int after,
                int applied,
                int shellBefore,
                int shellAfterBase,
                int shellAfter,
                int baseShellApplied,
                IEnumerable<ShellModifierContribution> shellModifiers)
            {
                Actor = actor;
                Before = before;
                After = after;
                Applied = applied;
                ShellBefore = shellBefore;
                ShellAfterBase = shellAfterBase;
                ShellAfter = shellAfter;
                BaseShellApplied = baseShellApplied;
                ShellModifiers = Array.AsReadOnly((shellModifiers
                    ?? Enumerable.Empty<ShellModifierContribution>())
                    .ToArray());
            }

            internal MutableActor Actor { get; }
            internal int Before { get; }
            internal int After { get; }
            internal int Applied { get; }
            internal int ShellBefore { get; }
            internal int ShellAfterBase { get; }
            internal int ShellAfter { get; }
            internal int BaseShellApplied { get; }
            internal IReadOnlyList<ShellModifierContribution> ShellModifiers
            {
                get;
            }
            internal int ModifierShellApplied => ShellModifiers.Sum(
                value => value.AppliedShell);
            internal int TotalShellApplied => checked(
                BaseShellApplied + ModifierShellApplied);
            internal bool BrokeShell => ShellBefore > 0 && ShellAfter == 0;
        }

        private sealed class CanonicalBasicMutationPlan
        {
            internal CanonicalBasicMutationPlan(
                MutableActor target,
                int shellBeforeBreak,
                int shellAfterBreak,
                long requestedBreak,
                int breakApplied,
                long playerGuardBefore,
                long playerGuardAfter,
                long guardApplied,
                int playerHpBefore,
                int playerHpAfter,
                long requestedHeal,
                int healApplied)
            {
                Target = target;
                ShellBeforeBreak = shellBeforeBreak;
                ShellAfterBreak = shellAfterBreak;
                RequestedBreak = requestedBreak;
                BreakApplied = breakApplied;
                PlayerGuardBefore = playerGuardBefore;
                PlayerGuardAfter = playerGuardAfter;
                GuardApplied = guardApplied;
                PlayerHpBefore = playerHpBefore;
                PlayerHpAfter = playerHpAfter;
                RequestedHeal = requestedHeal;
                HealApplied = healApplied;
            }

            internal MutableActor Target { get; }
            internal int ShellBeforeBreak { get; }
            internal int ShellAfterBreak { get; }
            internal long RequestedBreak { get; }
            internal int BreakApplied { get; }
            internal long PlayerGuardBefore { get; }
            internal long PlayerGuardAfter { get; }
            internal long GuardApplied { get; }
            internal int PlayerHpBefore { get; }
            internal int PlayerHpAfter { get; }
            internal long RequestedHeal { get; }
            internal int HealApplied { get; }
            internal bool HasMutation => BreakApplied > 0
                || GuardApplied > 0L
                || HealApplied > 0;
        }

        private sealed class ShellModifierContribution
        {
            internal ShellModifierContribution(
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
                C1FormalRealtimeBattleCombatComponentSnapshot component,
                long percentageBase,
                int shellBefore,
                int shellAfter,
                int appliedShell)
            {
                Fact = fact;
                Component = component;
                PercentageBase = percentageBase;
                ShellBefore = shellBefore;
                ShellAfter = shellAfter;
                AppliedShell = appliedShell;
            }

            internal C1FormalRealtimeBattlePool15ActiveItemFactSnapshot Fact
            {
                get;
            }
            internal C1FormalRealtimeBattleCombatComponentSnapshot Component
            {
                get;
            }
            internal long PercentageBase { get; }
            internal int ShellBefore { get; }
            internal int ShellAfter { get; }
            internal int AppliedShell { get; }
        }

        private sealed class ExtraTargetAssignment
        {
            internal ExtraTargetAssignment(
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot fact,
                C1FormalRealtimeBattleCombatComponentSnapshot component,
                DamageSegment segment)
            {
                Fact = fact;
                Component = component;
                Segment = segment;
            }

            internal C1FormalRealtimeBattlePool15ActiveItemFactSnapshot Fact
            {
                get;
            }
            internal C1FormalRealtimeBattleCombatComponentSnapshot Component
            {
                get;
            }
            internal DamageSegment Segment { get; }
        }
    }
}
