using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.V04.ChapterFlow.Chapter1;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public sealed class V04Chapter1NormalEnemyActorSnapshot
    {
        internal V04Chapter1NormalEnemyActorSnapshot(
            V04Chapter1ActorPlanDefinition plan,
            C1EnemyRuntimeSnapshot runtime)
        {
            Plan = plan;
            Runtime = runtime;
        }

        public V04Chapter1ActorPlanDefinition Plan { get; }
        public C1EnemyRuntimeSnapshot Runtime { get; }
        public string EnemyInstanceId =>
            Runtime?.EnemyInstanceId ?? string.Empty;
        public bool IsLiveTargetable =>
            Runtime?.Lifecycle == C1EnemyLifecycle.Active
            && Runtime.Targetable;
    }

    public sealed class V04Chapter1NormalEnemyBattleAdapter
    {
        private sealed class ActorRuntimeState
        {
            public ActorRuntimeState(
                V04Chapter1ActorPlanDefinition plan,
                C1EnemyRuntimeSnapshot runtime)
            {
                Plan = plan;
                Runtime = runtime;
            }

            public V04Chapter1ActorPlanDefinition Plan { get; }
            public C1EnemyRuntimeSnapshot Runtime { get; set; }
            public readonly HashSet<string> HandledRequestIds =
                new(StringComparer.Ordinal);
        }

        private readonly IItemSystemBattleSandboxBoardAuthority authority;
        private readonly ItemCombatEffectRequestSnapshot itemRequests;
        private readonly ItemCombatEffectRequestRow[] damageRows;
        private readonly List<ActorRuntimeState> actors;
        private readonly Queue<
                C1OrdinaryEncounterApplicationPresentationEvent>
            pendingPresentationEvents = new();
        private readonly Queue<
                C1OrdinaryEncounterEnemyToPlayerPresentationEvent>
            pendingEnemyToPlayerPresentationEvents = new();
        private readonly string acceptedItemSystemSignature;
        private readonly bool validationFixture;
        private BattleStartRequest battleRequest;
        private long currentTick;
        private long nextItemTick =
            V04Chapter1ContinuousBattleIntegrationContract
                .ItemApplicationCadenceTicks;
        private long applicationSequence;
        private long presentationSequence;
        private int nextDamageOrdinal;
        private int playerCurrentHp =
            V04Chapter1ContinuousBattleIntegrationContract
                .PlayerMaxHpFixture;
        private int outgoingRequestsResolved;
        private int fieldAreaObservedCount;
        private int counterWindowObservedCount;
        private bool sourceRejected;
        private string selectedTargetEnemyInstanceId = string.Empty;

        private V04Chapter1NormalEnemyBattleAdapter(
            IItemSystemBattleSandboxBoardAuthority authority,
            ItemCombatEffectRequestSnapshot itemRequests,
            IEnumerable<ActorRuntimeState> actors,
            bool validationFixture)
        {
            this.authority = authority;
            this.itemRequests = itemRequests;
            this.actors = (actors ?? Array.Empty<ActorRuntimeState>())
                .OrderBy(value => value.Plan.slotOrdinal)
                .ThenBy(value => value.Plan.waveEntryId,
                    StringComparer.Ordinal)
                .ToList();
            this.validationFixture = validationFixture;
            damageRows = itemRequests?.Requests
                .Where(IsAcceptedDamageRow)
                .OrderBy(value => value.sourceItemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.sourcePlacementId,
                    StringComparer.Ordinal)
                .ThenBy(value => (int)value.requestKind)
                .ToArray()
                ?? Array.Empty<ItemCombatEffectRequestRow>();
            acceptedItemSystemSignature =
                authority?.CurrentSnapshot?.BuildDebugSignature()
                ?? string.Empty;
            EnsureDeterministicSelection();
            LastDiagnosticCode = "PREPARED";
        }

        public C1EnemyRuntimeSnapshot Enemy =>
            FindState(selectedTargetEnemyInstanceId)?.Runtime
            ?? actors.FirstOrDefault()?.Runtime;
        public IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot>
            ActorSnapshots => actors.Select(value =>
                new V04Chapter1NormalEnemyActorSnapshot(
                    value.Plan,
                    value.Runtime)).ToArray();
        public IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot>
            ActiveActors => actors
                .Where(IsLiveTargetable)
                .Select(value =>
                    new V04Chapter1NormalEnemyActorSnapshot(
                        value.Plan,
                        value.Runtime))
                .ToArray();
        public IReadOnlyList<C1EncounterAdmissionActorSnapshot>
            AdmissionActors => actors.Select(value =>
                new C1EncounterAdmissionActorSnapshot(
                    value.Plan.waveEntryId,
                    value.Runtime?.EnemyInstanceId,
                    value.Plan.enemyContentId,
                    value.Plan.runtimeProfileId,
                    value.Plan.visualProfileKey,
                    value.Plan.occurrenceOrdinal,
                    value.Plan.displaySlotId,
                    value.Plan.slotOrdinal,
                    value.Runtime?.ResetGeneration ?? 0))
                .ToArray();
        public string SelectedTargetEnemyInstanceId =>
            selectedTargetEnemyInstanceId;
        public int WaveResetGeneration =>
            actors.FirstOrDefault()?.Runtime?.ResetGeneration ?? 0;
        public bool IsPrepared =>
            battleRequest == null
            && actors.Count > 0
            && !sourceRejected;
        public bool IsRunning =>
            battleRequest != null
            && actors.Any(IsLiveTargetable)
            && !sourceRejected;
        public bool IsDefeated =>
            actors.Count > 0
            && actors.All(value =>
                value.Runtime?.Lifecycle ==
                    C1EnemyLifecycle.Defeated);
        public int PlayerCurrentHp => playerCurrentHp;
        public string LastDiagnosticCode { get; private set; }
        public string LastDiagnosticDetail { get; private set; } =
            string.Empty;
        public int AcceptedItemApplicationCount =>
            actors.Sum(value =>
                value.Runtime?.AcceptedApplicationCount ?? 0);
        public int OutgoingRequestsResolved =>
            outgoingRequestsResolved;
        public int FieldAreaObservedCount => fieldAreaObservedCount;
        public int CounterWindowObservedCount =>
            counterWindowObservedCount;
        public int FieldAreaExecutionCount => 0;
        public int CounterWindowExecutionCount => 0;
        public int FormalMechanicGrantCount => 0;
        public long CurrentTick => currentTick;
        public string ItemRequestCanonicalSignature =>
            itemRequests?.canonicalSignature ?? string.Empty;
        public ItemCombatEffectRequestSnapshot ItemRequestSnapshot =>
            itemRequests;
        public C1OrdinaryEncounterApplicationPresentationEvent
            LastAcceptedPresentation { get; private set; }
        public int PublishedPresentationCount { get; private set; }
        public int PendingPresentationCount =>
            pendingPresentationEvents.Count;
        public int PendingEnemyToPlayerPresentationCount =>
            pendingEnemyToPlayerPresentationEvents.Count;

        public bool TrySelectTarget(string enemyInstanceId)
        {
            ActorRuntimeState requested = FindState(enemyInstanceId);
            if (requested == null || !IsLiveTargetable(requested))
            {
                EnsureDeterministicSelection();
                LastDiagnosticCode = "TARGET_SELECTION_REJECTED";
                return false;
            }

            selectedTargetEnemyInstanceId =
                requested.Runtime.EnemyInstanceId;
            LastDiagnosticCode = "TARGET_SELECTION_ACCEPTED";
            return true;
        }

        public bool TryDequeuePresentation(
            out C1OrdinaryEncounterApplicationPresentationEvent
                presentation)
        {
            if (pendingPresentationEvents.Count == 0)
            {
                presentation = null;
                return false;
            }
            presentation = pendingPresentationEvents.Dequeue();
            return true;
        }

        public bool TryDequeueEnemyToPlayerPresentation(
            out C1OrdinaryEncounterEnemyToPlayerPresentationEvent
                presentation)
        {
            if (pendingEnemyToPlayerPresentationEvents.Count == 0)
            {
                presentation = null;
                return false;
            }
            presentation =
                pendingEnemyToPlayerPresentationEvents.Dequeue();
            return true;
        }

        public static bool TryPrepare(
            IItemSystemBattleSandboxBoardAuthority authority,
            V04Chapter1WavePlanDefinition wave,
            int resetGeneration,
            out V04Chapter1NormalEnemyBattleAdapter adapter,
            out string diagnostic)
        {
            adapter = null;
            diagnostic = string.Empty;
            if (!TryValidateWavePlan(
                    wave,
                    out IReadOnlyList<
                        C1EnemyRuntimeProfileSnapshot> profiles,
                    out diagnostic))
            {
                return false;
            }
            if (authority == null)
            {
                diagnostic = "ITEM_AUTHORITY_ABSENT";
                return false;
            }
            if (!IsAuthorityValid(authority))
            {
                diagnostic = "ITEM_AUTHORITY_SNAPSHOT_INVALID";
                return false;
            }

            try
            {
                ItemCombatEffectRequestSnapshot assembled =
                    AssembleFresh(authority);
                if (assembled == null
                    || assembled.status !=
                        ItemCombatEffectRequestSnapshotStatus.Valid)
                {
                    diagnostic = "ITEM_REQUEST_INVALID";
                    return false;
                }
                if (!assembled.Requests.Any(IsAcceptedDamageRow))
                {
                    diagnostic =
                        "ITEM_DIRECT_DAMAGE_REQUEST_MISSING";
                    return false;
                }
                if (!TryCreateActorStates(
                        wave,
                        profiles,
                        resetGeneration,
                        out List<ActorRuntimeState> states,
                        out diagnostic))
                {
                    return false;
                }

                adapter = new V04Chapter1NormalEnemyBattleAdapter(
                    authority,
                    assembled,
                    states,
                    validationFixture: false);
                diagnostic = "PREPARED";
                return true;
            }
            catch (Exception exception)
            {
                diagnostic = "NORMAL_BATTLE_PREPARE_REJECTED_"
                    + exception.GetType().Name;
                return false;
            }
        }

        // Backward-compatible single-actor probe. Runtime flow uses the
        // WavePlan overload above.
        public static bool TryPrepare(
            IItemSystemBattleSandboxBoardAuthority authority,
            C1EnemyRuntimeProfileSnapshot profile,
            int resetGeneration,
            out V04Chapter1NormalEnemyBattleAdapter adapter,
            out string diagnostic)
        {
            adapter = null;
            diagnostic = string.Empty;
            if (profile == null)
            {
                diagnostic = "C1_RUNTIME_PROFILE_MISSING";
                return false;
            }
            V04Chapter1WaveEntryDefinition actor =
                new(
                    "compat.single_actor.wave_entry",
                    "compat.single_actor.wave",
                    "compat",
                    1,
                    profile.ContentId,
                    profile.RuntimeProfileId,
                    profile.PresentationKey,
                    1,
                    "Slot01",
                    1,
                    V04Chapter1WaveRuntimeStatus.Available,
                    false);
            V04Chapter1WavePlanDefinition wave =
                new(
                    actor.waveId,
                    actor.stageId,
                    1,
                    new[] { actor });
            return TryPrepare(
                authority,
                wave,
                resetGeneration,
                out adapter,
                out diagnostic);
        }

        public static bool TryCreateValidationFixture(
            V04Chapter1WavePlanDefinition wave,
            int resetGeneration,
            out V04Chapter1NormalEnemyBattleAdapter adapter,
            out string diagnostic)
        {
            adapter = null;
            if (!TryValidateWavePlan(
                    wave,
                    out IReadOnlyList<
                        C1EnemyRuntimeProfileSnapshot> profiles,
                    out diagnostic))
            {
                return false;
            }
            if (!TryCreateActorStates(
                    wave,
                    profiles,
                    resetGeneration,
                    out List<ActorRuntimeState> states,
                    out diagnostic))
            {
                return false;
            }
            adapter = new V04Chapter1NormalEnemyBattleAdapter(
                null,
                null,
                states,
                validationFixture: true);
            diagnostic = "VALIDATION_FIXTURE_PREPARED";
            return true;
        }

        public bool ApplyDirectDamageForValidation(
            int damage,
            long tick)
        {
            if (!validationFixture
                || damage <= 0
                || tick < currentTick)
            {
                return false;
            }
            ActorRuntimeState target = SelectedLiveState();
            if (target == null)
            {
                return false;
            }
            bool accepted = ApplyDirectDamage(
                target,
                damage,
                tick,
                "validation.direct_damage",
                null);
            if (accepted)
            {
                LastDiagnosticCode =
                    "VALIDATION_DIRECT_DAMAGE_ACCEPTED";
            }
            return accepted;
        }

        public bool BindAcceptedBattleRequest(
            BattleStartRequest request)
        {
            bool projectedProfileMatches = request != null
                && (string.IsNullOrWhiteSpace(
                        request.enemyProfileId)
                    || actors.Any(value => string.Equals(
                        value.Runtime?.RuntimeProfileId,
                        request.enemyProfileId,
                        StringComparison.Ordinal)));
            if (!IsPrepared
                || request == null
                || request.isBossStage
                || !projectedProfileMatches
                || !SourceAuthorityIsCurrent())
            {
                LastDiagnosticCode =
                    "BATTLE_REQUEST_BIND_REJECTED";
                return false;
            }
            battleRequest = request;
            LastDiagnosticCode = "RUNNING";
            return true;
        }

        public void AdvanceTo(long targetTick)
        {
            if (!IsRunning || targetTick < currentTick)
            {
                return;
            }
            if (!SourceAuthorityIsCurrent())
            {
                sourceRejected = true;
                LastDiagnosticCode =
                    "ITEM_SOURCE_CHANGED_STALE_GENERATION_REJECTED";
                LastDiagnosticDetail =
                    "Real Item authority/source signatures changed during Battle.";
                return;
            }

            while (IsRunning)
            {
                long nextActionTick =
                    NextEnemySchedulerMilestone();
                long nextBattleTick = Math.Min(
                    nextItemTick,
                    nextActionTick);
                if (nextBattleTick > targetTick)
                {
                    break;
                }

                currentTick = nextBattleTick;
                if (nextActionTick == nextBattleTick)
                {
                    AdvanceEnemySchedulers(currentTick);
                    ResolveEnemyRequests();
                }
                if (IsRunning && nextItemTick == nextBattleTick)
                {
                    ApplyNextRealItemDamage(currentTick);
                    nextItemTick = checked(
                        nextItemTick
                        + V04Chapter1ContinuousBattleIntegrationContract
                            .ItemApplicationCadenceTicks);
                }
            }
            currentTick = Math.Max(currentTick, targetTick);
            if (IsDefeated)
            {
                selectedTargetEnemyInstanceId = string.Empty;
                ObserveNonExecutingRequests();
                LastDiagnosticCode =
                    "NORMAL_WAVE_TERMINAL_DEFEATED";
            }
        }

        public BattleResultSnapshot CreateTerminalResult(
            long resultSequence)
        {
            if (!IsDefeated
                || battleRequest == null
                || sourceRejected)
            {
                LastDiagnosticCode =
                    "TERMINAL_RESULT_NOT_AVAILABLE";
                return null;
            }
            string runtimeSet = string.Join(
                "+",
                actors.Select(value =>
                        value.Runtime?.RuntimeProfileId
                        ?? string.Empty)
                    .Distinct(StringComparer.Ordinal));
            return CreateTerminalWinResult(
                battleRequest,
                resultSequence,
                currentTick,
                runtimeSet);
        }

        public V04Chapter1NormalBattleFacts CaptureFacts()
        {
            return new V04Chapter1NormalBattleFacts
            {
                enemy = Enemy,
                enemies = actors.Select(value => value.Runtime).ToArray(),
                selectedTargetEnemyInstanceId =
                    selectedTargetEnemyInstanceId,
                playerCurrentHp = playerCurrentHp,
                battleTick = currentTick,
                acceptedItemApplications =
                    AcceptedItemApplicationCount,
                outgoingRequestsResolved =
                    outgoingRequestsResolved,
                fieldAreaObservedCount = fieldAreaObservedCount,
                counterWindowObservedCount =
                    counterWindowObservedCount,
                fieldAreaExecutionCount = 0,
                counterWindowExecutionCount = 0,
                formalMechanicGrantCount = 0,
                itemRequestCanonicalSignature =
                    itemRequests?.canonicalSignature ?? string.Empty
            };
        }

        public static BattleResultSnapshot CreateTerminalWinResult(
            BattleStartRequest request,
            long resultSequence,
            long battleTick,
            string runtimeProfileId)
        {
            if (request == null || request.isBossStage)
            {
                return null;
            }
            return new BattleResultSnapshot
            {
                resultId =
                    V04Chapter1ContinuousFlowSession.StableId(
                        "normal_result",
                        resultSequence),
                requestId = request.requestId,
                resultType = BattleResultType.Win,
                win = true,
                lose = false,
                abandon = false,
                roundId = request.roundId,
                bossDefeated = false,
                durationSeconds = battleTick / 1000f,
                chapterProgressDelta = string.Empty,
                rewardPreview = new List<string>(),
                itemDrops = new List<string>(),
                buildPerformanceSummary =
                    "REAL_ITEM_DAMAGE / "
                    + (runtimeProfileId ?? string.Empty),
                eventSummary = new List<string>
                {
                    V04Chapter1ContinuousBattleIntegrationContract
                        .CadenceLabels,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .ResolverLabels,
                    "FORMAL_DROP_ROLLS=0",
                    "FORMAL_DROP_GRANTS=0",
                    "INVENTORY_WRITES=0",
                    "SAVE_WRITES=0"
                },
                rewardClaimToken = string.Empty,
                nextRouteHint = string.Empty,
                devOnly = true,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        private static bool TryValidateWavePlan(
            V04Chapter1WavePlanDefinition wave,
            out IReadOnlyList<C1EnemyRuntimeProfileSnapshot> profiles,
            out string diagnostic)
        {
            profiles = Array.Empty<C1EnemyRuntimeProfileSnapshot>();
            diagnostic = string.Empty;
            if (wave == null
                || wave.isBoss
                || wave.actors.Count < 1
                || wave.actors.Count > 3)
            {
                diagnostic = "ORDINARY_WAVE_PLAN_INVALID";
                return false;
            }

            // Whole-wave preflight occurs before any Runtime is created.
            if (wave.actors.Any(value =>
                value.runtimeStatus ==
                    V04Chapter1WaveRuntimeStatus.HeldByBad3
                || !value.HasRuntimeProfile))
            {
                diagnostic =
                    V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3;
                return false;
            }

            IReadOnlyList<C1EnemyRuntimeProfileSnapshot> catalog =
                C1EnemyRuntimeCatalog.GetProfiles();
            List<C1EnemyRuntimeProfileSnapshot> resolved = new();
            foreach (V04Chapter1ActorPlanDefinition actor in
                wave.actors.OrderBy(value => value.slotOrdinal))
            {
                C1EnemyRuntimeProfileSnapshot profile =
                    catalog.SingleOrDefault(value =>
                        string.Equals(
                            value.ContentId,
                            actor.enemyContentId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            value.RuntimeProfileId,
                            actor.runtimeProfileId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            value.PresentationKey,
                            actor.visualProfileKey,
                            StringComparison.Ordinal));
                if (profile == null)
                {
                    diagnostic =
                        "CURRENT_WAVE_RUNTIME_PROFILE_MISMATCH";
                    return false;
                }
                resolved.Add(profile);
            }
            profiles = resolved;
            return true;
        }

        private static bool TryCreateActorStates(
            V04Chapter1WavePlanDefinition wave,
            IReadOnlyList<C1EnemyRuntimeProfileSnapshot> profiles,
            int resetGeneration,
            out List<ActorRuntimeState> states,
            out string diagnostic)
        {
            states = new List<ActorRuntimeState>();
            diagnostic = string.Empty;
            if (resetGeneration <= 0
                || wave == null
                || profiles == null
                || profiles.Count != wave.actors.Count)
            {
                diagnostic =
                    "WAVE_ACTOR_RUNTIME_GENERATION_INVALID";
                return false;
            }

            List<ActorRuntimeState> staged = new();
            for (int index = 0; index < wave.actors.Count; index++)
            {
                V04Chapter1ActorPlanDefinition plan =
                    wave.actors[index];
                C1EnemyRuntimeProfileSnapshot profile =
                    profiles[index];
                string enemyInstanceId = string.Format(
                    CultureInfo.InvariantCulture,
                    "v04.c1.enemy_instance.{0}.g{1:D4}",
                    plan.waveEntryId,
                    resetGeneration);
                C1EnemyRuntimeSnapshot present =
                    C1EnemyRuntimeReducer.CreatePresent(
                        profile.RuntimeProfileId,
                        enemyInstanceId,
                        resetGeneration,
                        0L);
                C1EnemyTransitionResult activated =
                    C1EnemyRuntimeReducer.Activate(
                        present,
                        0L);
                if (!activated.Accepted
                    || activated.Snapshot?.Targetable != true)
                {
                    diagnostic =
                        "C1_RUNTIME_TARGET_NOT_ACTIVE";
                    return false;
                }
                staged.Add(new ActorRuntimeState(
                    plan,
                    activated.Snapshot));
            }

            if (staged.Select(value =>
                    value.Runtime.EnemyInstanceId)
                .Distinct(StringComparer.Ordinal).Count()
                != staged.Count)
            {
                diagnostic = "ENEMY_INSTANCE_ID_DUPLICATE";
                return false;
            }
            states = staged;
            return true;
        }

        private void AdvanceEnemySchedulers(long tick)
        {
            foreach (ActorRuntimeState actor in actors
                .Where(IsLiveTargetable)
                .OrderBy(value => value.Plan.slotOrdinal))
            {
                C1EnemyTransitionResult scheduled =
                    C1EnemyActionScheduler.Advance(
                        actor.Runtime,
                        tick,
                        reactiveCuePending: false);
                if (scheduled.Accepted
                    && scheduled.Snapshot != null)
                {
                    actor.Runtime = scheduled.Snapshot;
                }
            }
        }

        private long NextEnemySchedulerMilestone()
        {
            long next = long.MaxValue;
            foreach (ActorRuntimeState actor in actors
                .Where(IsLiveTargetable))
            {
                C1EnemyActiveActionSnapshot action =
                    actor.Runtime.ActiveAction;
                long candidate = action == null
                    ? Math.Max(
                        currentTick,
                        actor.Runtime.NextActionDueTick)
                    : action.Status ==
                        C1EnemyActionStatus.Telegraph
                        ? Math.Max(
                            currentTick,
                            action.ResolveTick)
                        : Math.Max(
                            currentTick,
                            action.RecoveryEndTick);
                next = Math.Min(next, candidate);
            }
            return next;
        }

        private void ResolveEnemyRequests()
        {
            foreach (ActorRuntimeState actor in actors
                .OrderBy(value => value.Plan.slotOrdinal))
            {
                ResolveEnemyRequests(actor);
            }
        }

        private void ResolveEnemyRequests(ActorRuntimeState actor)
        {
            if (actor?.Runtime == null)
            {
                return;
            }
            foreach (C1EnemyEffectRequestSnapshot request in
                actor.Runtime.PendingRequests
                    .OrderBy(value => value.RequestSequence))
            {
                if (!actor.HandledRequestIds.Add(request.RequestId))
                {
                    continue;
                }
                if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract
                        .DirectPlayerDamageRequest,
                    StringComparison.Ordinal))
                {
                    int hpBefore = playerCurrentHp;
                    playerCurrentHp = Math.Max(
                        0,
                        playerCurrentHp
                        - V04Chapter1ContinuousBattleIntegrationContract
                            .ShatteredHostOutgoingDamageFixture);
                    outgoingRequestsResolved++;
                    PublishEnemyToPlayerPresentation(
                        actor,
                        request,
                        hpBefore,
                        playerCurrentHp);
                }
                else if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract.ChargeAttackRequest,
                    StringComparison.Ordinal))
                {
                    int hpBefore = playerCurrentHp;
                    playerCurrentHp = Math.Max(
                        0,
                        playerCurrentHp
                        - V04Chapter1ContinuousBattleIntegrationContract
                            .PorcelainHoundOutgoingDamageFixture);
                    outgoingRequestsResolved++;
                    PublishEnemyToPlayerPresentation(
                        actor,
                        request,
                        hpBefore,
                        playerCurrentHp);
                }
                else if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract.PostDefeatFieldRequest,
                    StringComparison.Ordinal))
                {
                    fieldAreaObservedCount++;
                }
                else if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract
                        .ShellBreakCounterWindow,
                    StringComparison.Ordinal))
                {
                    counterWindowObservedCount++;
                }
            }
        }

        private void ObserveNonExecutingRequests()
        {
            ResolveEnemyRequests();
        }

        private void ApplyNextRealItemDamage(long tick)
        {
            if (damageRows.Length == 0 || !IsRunning)
            {
                return;
            }
            ActorRuntimeState target = SelectedLiveState();
            if (target == null)
            {
                LastDiagnosticCode = "LIVE_TARGET_MISSING";
                return;
            }
            ItemCombatEffectRequestRow row =
                damageRows[nextDamageOrdinal++ % damageRows.Length];
            int damage = (int)Math.Min(
                int.MaxValue,
                row.resolvedPreMitigationDamageUnits);
            if (damage <= 0)
            {
                return;
            }
            ApplyDirectDamage(
                target,
                damage,
                tick,
                row.requestId,
                row);
        }

        private bool ApplyDirectDamage(
            ActorRuntimeState target,
            int damage,
            long tick,
            string sourceRequestId,
            ItemCombatEffectRequestRow presentationRow)
        {
            if (target == null
                || !IsLiveTargetable(target)
                || damage <= 0)
            {
                return false;
            }

            C1EnemyRuntimeSnapshot before = target.Runtime;
            List<string> reducerApplicationIds = new();
            long appliedTick = tick;
            if (target.Runtime.CurrentShell > 0)
            {
                int shellDamage = Math.Min(
                    target.Runtime.CurrentShell,
                    damage);
                if (!ApplyDamage(
                        target,
                        tick,
                        0,
                        shellDamage,
                        sourceRequestId,
                        "shell",
                        out string shellApplicationId))
                {
                    return false;
                }
                reducerApplicationIds.Add(shellApplicationId);
                damage -= shellDamage;
                ResolveEnemyRequests(target);
                if (damage > 0
                    && target.Runtime.Lifecycle ==
                        C1EnemyLifecycle.Active)
                {
                    if (ApplyDamage(
                            target,
                            tick + 1L,
                            Math.Min(
                                target.Runtime.CurrentHp,
                                damage),
                            0,
                            sourceRequestId,
                            "hp_after_shell",
                            out string hpApplicationId))
                    {
                        reducerApplicationIds.Add(
                            hpApplicationId);
                        appliedTick = tick + 1L;
                    }
                }
            }
            else if (ApplyDamage(
                target,
                tick,
                Math.Min(target.Runtime.CurrentHp, damage),
                0,
                sourceRequestId,
                "hp",
                out string applicationId))
            {
                reducerApplicationIds.Add(applicationId);
            }
            else
            {
                return false;
            }

            ResolveEnemyRequests(target);
            if (target.Runtime.Lifecycle ==
                C1EnemyLifecycle.Defeated)
            {
                EnsureDeterministicSelection();
            }
            if (presentationRow != null)
            {
                PublishAcceptedPresentation(
                    presentationRow,
                    before,
                    target.Runtime,
                    appliedTick,
                    reducerApplicationIds);
            }
            return reducerApplicationIds.Count > 0;
        }

        private bool ApplyDamage(
            ActorRuntimeState target,
            long tick,
            int hpDamage,
            int shellDamage,
            string sourceRequestId,
            string channel,
            out string applicationId)
        {
            long sequence = ++applicationSequence;
            applicationId =
                V04Chapter1ContinuousFlowSession.StableId(
                    "item_application_" + channel,
                    sequence);
            C1EnemyBattleApplicationResult application = new(
                applicationId,
                sequence,
                target.Runtime.ResetGeneration,
                tick,
                target.Runtime.EnemyInstanceId,
                acceptedByBattleLedger: true,
                actualHpDeltaApplied: hpDamage,
                actualShellDeltaApplied: shellDamage,
                sourceRequestId: sourceRequestId);
            C1EnemyTransitionResult applied =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    target.Runtime,
                    application);
            if (!applied.Accepted || applied.Snapshot == null)
            {
                LastDiagnosticCode = "C1_APPLICATION_REJECTED";
                LastDiagnosticDetail = applied.Error;
                applicationId = string.Empty;
                return false;
            }
            target.Runtime = applied.Snapshot;
            currentTick = Math.Max(currentTick, tick);
            return true;
        }

        private void PublishAcceptedPresentation(
            ItemCombatEffectRequestRow row,
            C1EnemyRuntimeSnapshot before,
            C1EnemyRuntimeSnapshot after,
            long battleTick,
            IReadOnlyCollection<string> reducerApplicationIds)
        {
            if (row == null
                || before == null
                || after == null
                || reducerApplicationIds == null
                || reducerApplicationIds.Count == 0)
            {
                return;
            }

            ItemSystemPlacementSnapshot placement =
                authority.CurrentSnapshot?.FindPlacement(
                    row.sourcePlacementId);
            C1EnemyCueSnapshot cue = after.Cues
                .Where(value => value != null
                    && value.CueSequence
                        > (before.LatestCue?.CueSequence ?? 0L))
                .OrderByDescending(value =>
                    CuePresentationPriority(value.Kind))
                .ThenByDescending(value => value.CueSequence)
                .FirstOrDefault();
            int resolvedDamage = (int)Math.Min(
                int.MaxValue,
                row.resolvedPreMitigationDamageUnits);
            C1OrdinaryEncounterApplicationPresentationEvent presentation =
                new(
                    V04Chapter1ContinuousFlowSession.StableId(
                        "ordinary_item_presentation",
                        ++presentationSequence),
                    presentationSequence,
                    reducerApplicationIds,
                    after.ResetGeneration,
                    battleTick,
                    row.requestId,
                    row.sourceBaseItemId,
                    row.sourceItemInstanceId,
                    row.sourcePlacementId,
                    placement?.OccupiedCells?.Select(value =>
                        new ItemShapeCell(value.x, value.y)),
                    resolvedDamage,
                    before.CurrentHp,
                    after.CurrentHp,
                    before.CurrentShell,
                    after.CurrentShell,
                    cue?.CueId ?? string.Empty,
                    cue?.CueSequence ?? 0L,
                    after);
            if (!presentation.IsAcceptedCorrelation)
            {
                LastDiagnosticCode =
                    "NORMAL_PRESENTATION_CORRELATION_REJECTED";
                LastDiagnosticDetail =
                    "Battle truth was accepted, but its read-only "
                    + "presentation correlation was incomplete.";
                return;
            }

            LastAcceptedPresentation = presentation;
            pendingPresentationEvents.Enqueue(presentation);
            PublishedPresentationCount++;
        }

        private void PublishEnemyToPlayerPresentation(
            ActorRuntimeState actor,
            C1EnemyEffectRequestSnapshot request,
            int hpBefore,
            int hpAfter)
        {
            if (actor?.Runtime == null
                || request == null
                || hpBefore <= hpAfter)
            {
                return;
            }

            C1OrdinaryEncounterEnemyToPlayerPresentationEvent presentation =
                new(
                    V04Chapter1ContinuousFlowSession.StableId(
                        "ordinary_enemy_to_player",
                        ++presentationSequence),
                    presentationSequence,
                    actor.Runtime.ResetGeneration,
                    request.BattleTick,
                    actor.Runtime.EnemyInstanceId,
                    request.RequestId,
                    request.RequestKey,
                    request.SourceIdentity,
                    hpBefore,
                    hpAfter);
            if (!presentation.IsAcceptedCorrelation)
            {
                LastDiagnosticCode =
                    "NORMAL_PLAYER_PRESENTATION_CORRELATION_REJECTED";
                LastDiagnosticDetail =
                    "Enemy request resolved in Battle truth, but the "
                    + "read-only player-damage presentation correlation "
                    + "was incomplete.";
                return;
            }

            pendingEnemyToPlayerPresentationEvents.Enqueue(presentation);
        }

        private ActorRuntimeState SelectedLiveState()
        {
            EnsureDeterministicSelection();
            return FindState(selectedTargetEnemyInstanceId);
        }

        private void EnsureDeterministicSelection()
        {
            ActorRuntimeState selected =
                FindState(selectedTargetEnemyInstanceId);
            if (selected != null && IsLiveTargetable(selected))
            {
                return;
            }
            selectedTargetEnemyInstanceId = actors
                .Where(IsLiveTargetable)
                .OrderBy(value => value.Plan.slotOrdinal)
                .ThenBy(value => value.Plan.waveEntryId,
                    StringComparer.Ordinal)
                .Select(value => value.Runtime.EnemyInstanceId)
                .FirstOrDefault()
                ?? string.Empty;
        }

        private ActorRuntimeState FindState(string enemyInstanceId)
        {
            if (string.IsNullOrWhiteSpace(enemyInstanceId))
            {
                return null;
            }
            return actors.SingleOrDefault(value => string.Equals(
                value.Runtime?.EnemyInstanceId,
                enemyInstanceId,
                StringComparison.Ordinal));
        }

        private static bool IsLiveTargetable(ActorRuntimeState actor)
        {
            return actor?.Runtime?.Lifecycle ==
                    C1EnemyLifecycle.Active
                && actor.Runtime.Targetable;
        }

        private static int CuePresentationPriority(
            C1EnemyCueKind kind)
        {
            return kind switch
            {
                C1EnemyCueKind.Defeated => 5,
                C1EnemyCueKind.Hit => 4,
                C1EnemyCueKind.ShellHit => 3,
                C1EnemyCueKind.ShellBreak => 2,
                _ => 1
            };
        }

        private bool SourceAuthorityIsCurrent()
        {
            if (validationFixture)
            {
                return true;
            }
            if (!IsAuthorityValid(authority)
                || !string.Equals(
                    acceptedItemSystemSignature,
                    authority.CurrentSnapshot.BuildDebugSignature(),
                    StringComparison.Ordinal))
            {
                return false;
            }
            try
            {
                ItemCombatEffectRequestSnapshot fresh =
                    AssembleFresh(authority);
                return fresh?.status ==
                        ItemCombatEffectRequestSnapshotStatus.Valid
                    && string.Equals(
                        fresh.canonicalSignature,
                        itemRequests.canonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceProjectionSetCanonicalSignature,
                        itemRequests
                            .sourceProjectionSetCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceItemSystemCanonicalSignature,
                        itemRequests
                            .sourceItemSystemCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceQualifiedBuildCanonicalSignature,
                        itemRequests
                            .sourceQualifiedBuildCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceCoreRuntimeCanonicalSignature,
                        itemRequests
                            .sourceCoreRuntimeCanonicalSignature,
                        StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsAuthorityValid(
            IItemSystemBattleSandboxBoardAuthority authority)
        {
            return authority?.CurrentSnapshot?.isValid == true
                && authority.CurrentBindingSnapshot?.isValid == true
                && authority.CurrentQualifiedBuildState?.isValid == true
                && authority.CurrentCoreEffectRuntimeState != null;
        }

        private static ItemCombatEffectRequestSnapshot AssembleFresh(
            IItemSystemBattleSandboxBoardAuthority authority)
        {
            ItemInstanceProjectionSetSnapshot projectionSet = new(
                authority.Rows
                    .Where(row => row != null && !row.IsSystemItem)
                    .Select(row => row.Projection)
                    .Where(value => value != null)
                    .OrderBy(value => value.itemInstanceId,
                        StringComparer.Ordinal),
                Array.Empty<ItemInstanceProjectionValidationError>());
            return ItemCombatEffectRequestAssembler.Instance.Assemble(
                projectionSet,
                authority.CurrentSnapshot,
                authority.CurrentQualifiedBuildState,
                authority.CurrentCoreEffectRuntimeState);
        }

        private static bool IsAcceptedDamageRow(
            ItemCombatEffectRequestRow row)
        {
            return row != null
                && row.requestKind ==
                    ItemCombatEffectRequestKind.DirectFlatDamage
                && row.resolvedPreMitigationDamageUnits > 0;
        }
    }
}
