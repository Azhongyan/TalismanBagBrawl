using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public sealed class V04Chapter1NormalEnemyBattleAdapter
    {
        private readonly IItemSystemBattleSandboxBoardAuthority authority;
        private readonly ItemCombatEffectRequestSnapshot itemRequests;
        private readonly ItemCombatEffectRequestRow[] damageRows;
        private readonly HashSet<string> handledEnemyRequestIds =
            new(StringComparer.Ordinal);
        private readonly string acceptedItemSystemSignature;
        private BattleStartRequest battleRequest;
        private C1EnemyRuntimeSnapshot enemy;
        private long currentTick;
        private long nextItemTick =
            V04Chapter1ContinuousBattleIntegrationContract
                .ItemApplicationCadenceTicks;
        private long applicationSequence;
        private int nextDamageOrdinal;
        private int playerCurrentHp =
            V04Chapter1ContinuousBattleIntegrationContract.PlayerMaxHpFixture;
        private int outgoingRequestsResolved;
        private int fieldAreaObservedCount;
        private int counterWindowObservedCount;
        private bool sourceRejected;

        private V04Chapter1NormalEnemyBattleAdapter(
            IItemSystemBattleSandboxBoardAuthority authority,
            ItemCombatEffectRequestSnapshot itemRequests,
            C1EnemyRuntimeSnapshot enemy)
        {
            this.authority = authority;
            this.itemRequests = itemRequests;
            this.enemy = enemy;
            damageRows = itemRequests.Requests
                .Where(IsAcceptedDamageRow)
                .OrderBy(value => value.sourceItemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.sourcePlacementId,
                    StringComparer.Ordinal)
                .ThenBy(value => (int)value.requestKind)
                .ToArray();
            acceptedItemSystemSignature =
                authority.CurrentSnapshot.BuildDebugSignature();
            LastDiagnosticCode = "PREPARED";
        }

        public C1EnemyRuntimeSnapshot Enemy => enemy;
        public bool IsPrepared => battleRequest == null && !sourceRejected;
        public bool IsRunning => battleRequest != null
            && enemy?.Lifecycle == C1EnemyLifecycle.Active
            && !sourceRejected;
        public bool IsDefeated =>
            enemy?.Lifecycle == C1EnemyLifecycle.Defeated;
        public int PlayerCurrentHp => playerCurrentHp;
        public string LastDiagnosticCode { get; private set; }
        public string LastDiagnosticDetail { get; private set; } = string.Empty;
        public int AcceptedItemApplicationCount =>
            enemy?.AcceptedApplicationCount ?? 0;
        public int OutgoingRequestsResolved => outgoingRequestsResolved;
        public int FieldAreaObservedCount => fieldAreaObservedCount;
        public int CounterWindowObservedCount => counterWindowObservedCount;
        public int FieldAreaExecutionCount => 0;
        public int CounterWindowExecutionCount => 0;
        public int FormalMechanicGrantCount => 0;
        public string ItemRequestCanonicalSignature =>
            itemRequests?.canonicalSignature ?? string.Empty;

        public static bool TryPrepare(
            IItemSystemBattleSandboxBoardAuthority authority,
            C1EnemyRuntimeProfileSnapshot profile,
            int resetGeneration,
            out V04Chapter1NormalEnemyBattleAdapter adapter,
            out string diagnostic)
        {
            adapter = null;
            diagnostic = string.Empty;
            if (authority == null)
            {
                diagnostic = "ITEM_AUTHORITY_ABSENT";
                return false;
            }
            if (profile == null)
            {
                diagnostic = "C1_RUNTIME_PROFILE_MISSING";
                return false;
            }
            if (authority.CurrentSnapshot?.isValid != true
                || authority.CurrentBindingSnapshot?.isValid != true
                || authority.CurrentQualifiedBuildState?.isValid != true
                || authority.CurrentCoreEffectRuntimeState == null)
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
                    diagnostic = "ITEM_DIRECT_DAMAGE_REQUEST_MISSING";
                    return false;
                }

                string enemyInstanceId =
                    V04Chapter1ContinuousFlowSession.StableId(
                        "normal_enemy",
                        resetGeneration);
                C1EnemyRuntimeSnapshot present =
                    C1EnemyRuntimeReducer.CreatePresent(
                        profile.RuntimeProfileId,
                        enemyInstanceId,
                        resetGeneration,
                        0L);
                C1EnemyTransitionResult activated =
                    C1EnemyRuntimeReducer.Activate(present, 0L);
                if (!activated.Accepted
                    || activated.Snapshot?.Targetable != true)
                {
                    diagnostic = "C1_RUNTIME_TARGET_NOT_ACTIVE";
                    return false;
                }

                adapter = new V04Chapter1NormalEnemyBattleAdapter(
                    authority,
                    assembled,
                    activated.Snapshot);
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

        public bool BindAcceptedBattleRequest(BattleStartRequest request)
        {
            if (!IsPrepared || request == null
                || request.isBossStage
                || !string.Equals(
                    request.enemyProfileId,
                    enemy.RuntimeProfileId,
                    StringComparison.Ordinal)
                || !SourceAuthorityIsCurrent())
            {
                LastDiagnosticCode = "BATTLE_REQUEST_BIND_REJECTED";
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

            while (IsRunning && nextItemTick <= targetTick)
            {
                currentTick = nextItemTick;
                AdvanceEnemyScheduler(currentTick);
                ResolveEnemyRequests();
                ApplyNextRealItemDamage(currentTick);
                nextItemTick +=
                    V04Chapter1ContinuousBattleIntegrationContract
                        .ItemApplicationCadenceTicks;
            }
            currentTick = Math.Max(currentTick, targetTick);
            if (IsDefeated)
            {
                ObserveNonExecutingRequests();
                LastDiagnosticCode = "NORMAL_ENEMY_TERMINAL_DEFEATED";
            }
        }

        public BattleResultSnapshot CreateTerminalResult(long resultSequence)
        {
            if (!IsDefeated || battleRequest == null || sourceRejected)
            {
                LastDiagnosticCode = "TERMINAL_RESULT_NOT_AVAILABLE";
                return null;
            }
            return CreateTerminalWinResult(
                battleRequest,
                resultSequence,
                currentTick,
                enemy.RuntimeProfileId);
        }

        public V04Chapter1NormalBattleFacts CaptureFacts()
        {
            return new V04Chapter1NormalBattleFacts
            {
                enemy = enemy,
                playerCurrentHp = playerCurrentHp,
                battleTick = currentTick,
                acceptedItemApplications =
                    enemy?.AcceptedApplicationCount ?? 0,
                outgoingRequestsResolved = outgoingRequestsResolved,
                fieldAreaObservedCount = fieldAreaObservedCount,
                counterWindowObservedCount = counterWindowObservedCount,
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
                resultId = V04Chapter1ContinuousFlowSession.StableId(
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
                    "REAL_ITEM_DAMAGE / " + (runtimeProfileId ?? string.Empty),
                eventSummary = new List<string>
                {
                    V04Chapter1ContinuousBattleIntegrationContract.CadenceLabels,
                    V04Chapter1ContinuousBattleIntegrationContract.ResolverLabels,
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

        private void AdvanceEnemyScheduler(long tick)
        {
            C1EnemyTransitionResult scheduled =
                C1EnemyActionScheduler.Advance(
                    enemy,
                    tick,
                    reactiveCuePending: false);
            if (scheduled.Accepted && scheduled.Snapshot != null)
            {
                enemy = scheduled.Snapshot;
            }
        }

        private void ResolveEnemyRequests()
        {
            foreach (C1EnemyEffectRequestSnapshot request
                in enemy.PendingRequests
                    .OrderBy(value => value.RequestSequence))
            {
                if (!handledEnemyRequestIds.Add(request.RequestId))
                {
                    continue;
                }
                if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    StringComparison.Ordinal))
                {
                    playerCurrentHp = Math.Max(
                        0,
                        playerCurrentHp
                        - V04Chapter1ContinuousBattleIntegrationContract
                            .ShatteredHostOutgoingDamageFixture);
                    outgoingRequestsResolved++;
                }
                else if (string.Equals(
                    request.RequestKey,
                    C1EnemyRuntimeContract.ChargeAttackRequest,
                    StringComparison.Ordinal))
                {
                    playerCurrentHp = Math.Max(
                        0,
                        playerCurrentHp
                        - V04Chapter1ContinuousBattleIntegrationContract
                            .PorcelainHoundOutgoingDamageFixture);
                    outgoingRequestsResolved++;
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
                    C1EnemyRuntimeContract.ShellBreakCounterWindow,
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
            ItemCombatEffectRequestRow row =
                damageRows[nextDamageOrdinal++ % damageRows.Length];
            int damage = (int)Math.Min(
                int.MaxValue,
                row.resolvedPreMitigationDamageUnits);
            if (damage <= 0)
            {
                return;
            }

            if (enemy.CurrentShell > 0)
            {
                int shellDamage = Math.Min(enemy.CurrentShell, damage);
                if (!ApplyDamage(
                    tick,
                    0,
                    shellDamage,
                    row.requestId,
                    "shell"))
                {
                    return;
                }
                damage -= shellDamage;
                ResolveEnemyRequests();
                if (damage > 0 && enemy.Lifecycle == C1EnemyLifecycle.Active)
                {
                    ApplyDamage(
                        tick + 1L,
                        Math.Min(enemy.CurrentHp, damage),
                        0,
                        row.requestId,
                        "hp_after_shell");
                }
                return;
            }

            ApplyDamage(
                tick,
                Math.Min(enemy.CurrentHp, damage),
                0,
                row.requestId,
                "hp");
        }

        private bool ApplyDamage(
            long tick,
            int hpDamage,
            int shellDamage,
            string sourceRequestId,
            string channel)
        {
            long sequence = ++applicationSequence;
            C1EnemyBattleApplicationResult application = new(
                V04Chapter1ContinuousFlowSession.StableId(
                    "item_application_" + channel,
                    sequence),
                sequence,
                enemy.ResetGeneration,
                tick,
                enemy.EnemyInstanceId,
                acceptedByBattleLedger: true,
                actualHpDeltaApplied: hpDamage,
                actualShellDeltaApplied: shellDamage,
                sourceRequestId: sourceRequestId);
            C1EnemyTransitionResult applied =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    enemy,
                    application);
            if (!applied.Accepted || applied.Snapshot == null)
            {
                LastDiagnosticCode = "C1_APPLICATION_REJECTED";
                LastDiagnosticDetail = applied.Error;
                return false;
            }
            enemy = applied.Snapshot;
            currentTick = Math.Max(currentTick, tick);
            return true;
        }

        private bool SourceAuthorityIsCurrent()
        {
            if (authority?.CurrentSnapshot?.isValid != true
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
                        itemRequests.sourceProjectionSetCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceItemSystemCanonicalSignature,
                        itemRequests.sourceItemSystemCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceQualifiedBuildCanonicalSignature,
                        itemRequests.sourceQualifiedBuildCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        fresh.sourceCoreRuntimeCanonicalSignature,
                        itemRequests.sourceCoreRuntimeCanonicalSignature,
                        StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
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
