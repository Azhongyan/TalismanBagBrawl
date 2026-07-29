using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Combat;

namespace TalismanBag.BattleBridge.ShougunuPhase1
{
    public static class ShougunuPhase1BattleApplicationEngine
    {
        public const string PackageId =
            "V0.4-ShougunuPhase1BattleApplicationContract01";
        public const string EnemyInstanceId = "shougunu.phase1.dev.enemy";
        public const string PlayerActorId = "battle.sandbox.player.dev";
        public const int TicksPerSecond = 1000;
        public const long ItemPulseIntervalTicks = 1500L;
        public const long NominalDurationTicks = 75000L;
        public const int NominalAcceptedItemApplications = 50;

        // Explicitly non-formal devOnly readability/survival fixtures.
        public const int BasicAttackPlayerDamageFixture = 12;
        public const int Skill2PlayerDamageFixture = 24;
        public const int Skill3PlayerDamageFixture = 36;

        private static readonly string[] OrderedItemIds =
        {
            "I007", "I008", "I009", "I010", "I011", "I012"
        };

        private static readonly int[] OrderedTerminalMagnitudes =
        {
            29, 42, 53, 46, 55, 76
        };

        public static ShougunuPhase1BattleApplicationSession Create(
            ItemCombatEffectRequestSnapshot itemSnapshot,
            int resetGeneration = 1)
        {
            return new ShougunuPhase1BattleApplicationSession(
                itemSnapshot,
                resetGeneration);
        }

        internal static IReadOnlyList<string> ItemIds =>
            Array.AsReadOnly(OrderedItemIds);

        internal static IReadOnlyList<int> TerminalMagnitudes =>
            Array.AsReadOnly(OrderedTerminalMagnitudes);
    }

    public static class BattleSandboxPlayerCombatantReducer
    {
        public static BattleSandboxPlayerCombatantSnapshot ApplyEnemyDamage(
            BattleSandboxPlayerCombatantSnapshot player,
            int resetGeneration,
            string enemyEffectId,
            int requestedDamage)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }
            if (resetGeneration <= 0
                || player.resetGeneration != resetGeneration)
            {
                throw new InvalidOperationException(
                    "Player reset generation mismatch.");
            }
            if (requestedDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedDamage),
                    "Player damage cannot be negative.");
            }
            if (requestedDamage == 0 || player.defeated)
            {
                return player;
            }

            int shieldDamage = Math.Min(player.shield, requestedDamage);
            int hpDamage = Math.Min(
                player.currentHp,
                requestedDamage - shieldDamage);
            int afterHp = player.currentHp - hpDamage;
            return new BattleSandboxPlayerCombatantSnapshot(
                resetGeneration,
                player.revision + 1L,
                player.playerActorId,
                afterHp,
                player.shield - shieldDamage,
                afterHp == 0,
                enemyEffectId,
                player.acceptedEnemyEffectCount + 1,
                player.developerDiagnostics);
        }
    }

    public sealed class ShougunuPhase1BattleApplicationSession
    {
        private readonly ItemCombatEffectRequestSnapshot _itemSnapshot;
        private readonly ShougunuPhase1ItemRequestSourceFacts _sourceFacts;
        private readonly ReadOnlyCollection<ItemCombatEffectRequestRow> _orderedRows;
        private readonly HashSet<string> _acceptedRequestEventIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _acceptedApplicationEventIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<ShougunuPhase1BattleLedgerEvent> _ledgerEvents =
            new List<ShougunuPhase1BattleLedgerEvent>();
        private readonly List<ShougunuPhase1BattleDisplayEvent> _displayEvents =
            new List<ShougunuPhase1BattleDisplayEvent>();
        private readonly List<ShougunuPhase1BattleApplicationTraceRow> _traceRows =
            new List<ShougunuPhase1BattleApplicationTraceRow>();
        private readonly List<BattleSandboxPlayerEffectApplicationResult>
            _playerResults =
                new List<BattleSandboxPlayerEffectApplicationResult>();
        private readonly List<ShougunuPhase1EnemyActionApplicationResult>
            _enemyActionResults =
                new List<ShougunuPhase1EnemyActionApplicationResult>();

        private ShougunuPhase1RuntimeSnapshot _enemy;
        private BattleSandboxPlayerCombatantSnapshot _player;
        private int _resetGeneration;
        private long _revision;
        private long _battleTick;
        private long _nextItemPulseTick;
        private int _nextItemOrdinal;
        private long _nextApplicationSequence;
        private long _nextLedgerSequence;
        private long _nextDisplaySequence;
        private long _lastObservedCueSequence;

        internal ShougunuPhase1BattleApplicationSession(
            ItemCombatEffectRequestSnapshot itemSnapshot,
            int resetGeneration)
        {
            if (resetGeneration <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resetGeneration),
                    "Reset generation must be positive.");
            }

            _itemSnapshot = itemSnapshot ??
                throw new ArgumentNullException(nameof(itemSnapshot));
            _sourceFacts =
                ShougunuPhase1ItemRequestSourceFacts.FromSnapshot(itemSnapshot);
            _orderedRows = SelectTerminalRows(itemSnapshot);
            _resetGeneration = resetGeneration;
            _revision = 1L;
            _battleTick = 0L;
            _nextItemPulseTick =
                ShougunuPhase1BattleApplicationEngine.ItemPulseIntervalTicks;
            _nextItemOrdinal = 0;
            _nextApplicationSequence = 1L;
            _nextLedgerSequence = 1L;
            _nextDisplaySequence = 1L;

            ShougunuPhase1RuntimeSnapshot presence =
                ShougunuPhase1RuntimeReducer.CreatePresence(
                    ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                    resetGeneration,
                    0L);
            ShougunuPhase1TransitionResult activated =
                ShougunuPhase1RuntimeReducer.ActivateInitialShell(presence, 0L);
            if (!activated.Accepted)
            {
                throw new InvalidOperationException(
                    "Enemy initial shell activation failed: "
                    + activated.RejectionReason);
            }
            _enemy = activated.State;
            _lastObservedCueSequence = _enemy.LatestCue == null
                ? 0L
                : _enemy.LatestCue.CueSequence;
            _player = NewPlayer(resetGeneration, 1L);
        }

        public IReadOnlyList<ItemCombatEffectRequestRow> TerminalRows =>
            _orderedRows;

        public ShougunuPhase1BattleApplicationContext Current =>
            BuildContext();

        public ShougunuPhase1BattleApplicationTrace RunNominalTrace()
        {
            if (_battleTick != 0L
                || _enemy.AcceptedDamageApplicationCount != 0
                || _traceRows.Count != 0)
            {
                throw new InvalidOperationException(
                    "Nominal trace requires a fresh Battle application session.");
            }

            AdvanceTo(
                ShougunuPhase1BattleApplicationEngine.NominalDurationTicks);
            return new ShougunuPhase1BattleApplicationTrace(
                BuildContext(),
                _traceRows,
                _playerResults,
                _enemyActionResults);
        }

        public ShougunuPhase1BattleApplicationContext AdvanceTo(long battleTick)
        {
            if (battleTick < _battleTick)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(battleTick),
                    "Battle clock cannot move backwards.");
            }

            for (long tick = _battleTick + 1L; tick <= battleTick; tick++)
            {
                _battleTick = tick;
                AdvanceLifecycle(tick);
                AdvanceAction(tick);
                if (tick == _nextItemPulseTick)
                {
                    ApplyScheduledPulse(tick);
                    _nextItemPulseTick +=
                        ShougunuPhase1BattleApplicationEngine
                            .ItemPulseIntervalTicks;
                }
            }

            if (battleTick
                    == ShougunuPhase1BattleApplicationEngine
                        .NominalDurationTicks
                && _enemy.Lifecycle
                    == ShougunuPhase1LifecycleState.DefeatCandidate
                && _enemy.BasicResolvedCount
                    == ShougunuPhase1BattleApplicationEngine
                        .NominalAcceptedItemApplications
                        / ShougunuPhase1RuntimeContract
                            .BasicDamageApplicationInterval
                && _enemy.BasicDebt == 0
                && _enemy.ThresholdOccurrences.All(value => value.Resolved)
                && _enemy.ActiveAction == null)
            {
                ShougunuPhase1TransitionResult defeat =
                    ShougunuPhase1RuntimeReducer.RequestDefeat(
                        _enemy,
                        battleTick);
                if (defeat.Accepted)
                {
                    ShougunuPhase1RuntimeSnapshot before = _enemy;
                    _enemy = defeat.State;
                    _revision++;
                    string ledgerId = AddLedger(
                        ShougunuPhase1BattleLedgerEventKind.Defeat,
                        ShougunuPhase1BattleApplicationDecision.Accepted,
                        battleTick,
                        "defeat-request",
                        _enemy.EnemyInstanceId,
                        string.Empty,
                        0,
                        _enemy.CanonicalSignature);
                    CaptureLifecycleCues(before, _enemy, ledgerId);
                }
            }
            return BuildContext();
        }

        public ShougunuPhase1ItemApplicationResult ApplyItemRequest(
            ShougunuPhase1ItemRequestSourceFacts sourceFacts,
            ItemCombatEffectRequestRow row,
            string requestEventId,
            string applicationEventId,
            string targetActorId,
            string targetContentId,
            string targetPhaseId,
            int resetGeneration,
            long battleTick,
            long applicationSequence,
            long expectedEnemyRevision)
        {
            string reason = ValidateItemRequest(
                sourceFacts,
                row,
                requestEventId,
                applicationEventId,
                targetActorId,
                targetContentId,
                targetPhaseId,
                resetGeneration,
                battleTick,
                applicationSequence,
                expectedEnemyRevision);
            if (!string.IsNullOrEmpty(reason))
            {
                return RejectItem(
                    row,
                    requestEventId,
                    applicationEventId,
                    targetActorId,
                    resetGeneration,
                    battleTick,
                    applicationSequence,
                    expectedEnemyRevision,
                    reason);
            }

            int requestedDamage = checked(
                (int)row.resolvedPreMitigationDamageUnits);
            int shellDamageApplied = 0;
            int hpDamageApplied = 0;
            if (_enemy.CurrentShell > 0)
            {
                shellDamageApplied = Math.Min(
                    _enemy.CurrentShell,
                    requestedDamage);
                int remaining = requestedDamage - shellDamageApplied;
                if (shellDamageApplied == _enemy.CurrentShell
                    && remaining > 0)
                {
                    hpDamageApplied = Math.Min(_enemy.CurrentHp, remaining);
                }
            }
            else
            {
                hpDamageApplied = Math.Min(_enemy.CurrentHp, requestedDamage);
            }

            ShougunuPhase1RuntimeSnapshot before = _enemy;
            ShougunuPhase1TransitionResult reduced =
                ShougunuPhase1RuntimeReducer.ApplyBattleApplication(
                    _enemy,
                    new ShougunuPhase1BattleApplication(
                        applicationEventId,
                        applicationSequence,
                        battleTick,
                        resetGeneration,
                        targetActorId,
                        true,
                        shellDamageApplied,
                        hpDamageApplied,
                        string.Empty,
                        requestedDamage));
            if (!reduced.Accepted)
            {
                return RejectItem(
                    row,
                    requestEventId,
                    applicationEventId,
                    targetActorId,
                    resetGeneration,
                    battleTick,
                    applicationSequence,
                    expectedEnemyRevision,
                    "ENEMY_REDUCER_" + reduced.RejectionReason);
            }

            _enemy = reduced.State;
            _acceptedRequestEventIds.Add(requestEventId);
            _acceptedApplicationEventIds.Add(applicationEventId);
            _revision++;
            string ledgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.ItemApplication,
                ShougunuPhase1BattleApplicationDecision.Accepted,
                battleTick,
                row.requestId,
                targetActorId,
                string.Empty,
                shellDamageApplied + hpDamageApplied,
                row.sourceProjectionCanonicalSignature);
            if (shellDamageApplied > 0)
            {
                AddDisplay(
                    ledgerId,
                    battleTick,
                    ShougunuPhase1BattleDisplaySourceKind.Item,
                    row.sourceBaseItemId,
                    targetActorId,
                    ShougunuPhase1BattleDisplayChannel
                        .ItemToEnemyShellDamage,
                    shellDamageApplied,
                    "battle.item.enemy_shell_damage",
                    60,
                    row.sourceItemInstanceId + ".shell",
                    ShougunuPhase1BattleAnchorRoute.DamageDealtAnchor,
                    "item.damage.shell",
                    true,
                    false);
            }
            if (hpDamageApplied > 0)
            {
                AddDisplay(
                    ledgerId,
                    battleTick,
                    ShougunuPhase1BattleDisplaySourceKind.Item,
                    row.sourceBaseItemId,
                    targetActorId,
                    ShougunuPhase1BattleDisplayChannel.ItemToEnemyHpDamage,
                    hpDamageApplied,
                    "battle.item.enemy_hp_damage",
                    70,
                    row.sourceItemInstanceId + ".hp",
                    ShougunuPhase1BattleAnchorRoute.DamageDealtAnchor,
                    "item.damage.hp",
                    true,
                    false);
            }
            CaptureLifecycleCues(before, _enemy, ledgerId);
            return new ShougunuPhase1ItemApplicationResult(
                ledgerId,
                requestEventId,
                applicationEventId,
                row.requestId,
                row.sourceBaseItemId,
                row.sourceItemInstanceId,
                row.sourcePlacementId,
                targetActorId,
                resetGeneration,
                battleTick,
                applicationSequence,
                expectedEnemyRevision,
                ShougunuPhase1BattleApplicationDecision.Accepted,
                string.Empty,
                requestedDamage,
                shellDamageApplied,
                hpDamageApplied,
                _enemy);
        }

        public ShougunuPhase1BattleLedgerEvent ObserveUnsupportedTelemetry(
            ItemCombatEffectUnsupportedTelemetry telemetry,
            long battleTick)
        {
            string sourceId = telemetry == null
                ? "unsupported.telemetry.null"
                : telemetry.sourceEffectId;
            string reason = telemetry == null
                ? "UNSUPPORTED_TELEMETRY_NULL"
                : "UNSUPPORTED_TELEMETRY_DIAGNOSTIC_ONLY:"
                    + telemetry.disposition + ":"
                    + telemetry.magnitudePresence;
            string ledgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.RejectedOrIgnored,
                ShougunuPhase1BattleApplicationDecision
                    .IgnoredDiagnosticOnly,
                battleTick,
                sourceId,
                _enemy.EnemyInstanceId,
                reason,
                0,
                telemetry == null ? string.Empty : telemetry.reasonCode);
            AddDisplay(
                ledgerId,
                battleTick,
                ShougunuPhase1BattleDisplaySourceKind.Battle,
                sourceId,
                _enemy.EnemyInstanceId,
                ShougunuPhase1BattleDisplayChannel
                    .RejectedOrNotExecutedDeveloperOnly,
                0,
                "battle.item.unsupported_telemetry",
                0,
                sourceId,
                ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                "developer.diagnostic",
                false,
                true);
            return _ledgerEvents[_ledgerEvents.Count - 1];
        }

        public ShougunuPhase1BattleApplicationContext Reset(long battleTick)
        {
            int newGeneration = _resetGeneration + 1;
            ShougunuPhase1TransitionResult reset =
                ShougunuPhase1RuntimeReducer.Reset(
                    _enemy,
                    newGeneration,
                    battleTick);
            if (!reset.Accepted)
            {
                throw new InvalidOperationException(
                    "Enemy reset failed: " + reset.RejectionReason);
            }
            ShougunuPhase1TransitionResult activated =
                ShougunuPhase1RuntimeReducer.ActivateInitialShell(
                    reset.State,
                    battleTick);
            if (!activated.Accepted)
            {
                throw new InvalidOperationException(
                    "Enemy activation after reset failed: "
                    + activated.RejectionReason);
            }

            _resetGeneration = newGeneration;
            _revision++;
            _battleTick = 0L;
            _nextItemPulseTick =
                ShougunuPhase1BattleApplicationEngine.ItemPulseIntervalTicks;
            _nextItemOrdinal = 0;
            _nextApplicationSequence = 1L;
            _nextLedgerSequence = 1L;
            _nextDisplaySequence = 1L;
            _acceptedRequestEventIds.Clear();
            _acceptedApplicationEventIds.Clear();
            _ledgerEvents.Clear();
            _displayEvents.Clear();
            _traceRows.Clear();
            _playerResults.Clear();
            _enemyActionResults.Clear();
            _enemy = activated.State;
            _lastObservedCueSequence = _enemy.LatestCue == null
                ? 0L
                : _enemy.LatestCue.CueSequence;
            _player = NewPlayer(
                newGeneration,
                _player == null ? 1L : _player.revision + 1L);
            AddLedger(
                ShougunuPhase1BattleLedgerEventKind.Reset,
                ShougunuPhase1BattleApplicationDecision.Accepted,
                0L,
                "battle.reset",
                _enemy.EnemyInstanceId,
                string.Empty,
                0,
                _enemy.CanonicalSignature);
            return BuildContext();
        }

        private void ApplyScheduledPulse(long battleTick)
        {
            ItemCombatEffectRequestRow row = _orderedRows[_nextItemOrdinal];
            long sequence = _nextApplicationSequence++;
            string sequenceText = sequence.ToString(
                "D3",
                CultureInfo.InvariantCulture);
            string requestEventId = "battle.request.g"
                + _resetGeneration.ToString(CultureInfo.InvariantCulture)
                + "." + sequenceText + "." + row.requestId;
            string applicationEventId = "battle.application.g"
                + _resetGeneration.ToString(CultureInfo.InvariantCulture)
                + "." + sequenceText;
            long expectedRevision = _enemy.Revision;
            ShougunuPhase1ItemApplicationResult result = ApplyItemRequest(
                _sourceFacts,
                row,
                requestEventId,
                applicationEventId,
                _enemy.EnemyInstanceId,
                _enemy.ContentId,
                _enemy.PhaseId,
                _resetGeneration,
                battleTick,
                sequence,
                expectedRevision);
            _nextItemOrdinal =
                (_nextItemOrdinal + 1) % _orderedRows.Count;

            // Threshold intent is offered immediately after the accepted pulse.
            AdvanceAction(battleTick);
            ShougunuPhase1PendingActionSnapshot action = _enemy.ActiveAction;
            _traceRows.Add(new ShougunuPhase1BattleApplicationTraceRow(
                sequence,
                battleTick,
                row.sourceBaseItemId,
                row.sourceItemInstanceId,
                row.requestId,
                checked((int)row.resolvedPreMitigationDamageUnits),
                result.shellDamageApplied,
                result.hpDamageApplied,
                _enemy.Lifecycle.ToString(),
                _enemy.CurrentHp,
                _enemy.CurrentShell,
                _enemy.BasicEarnedCount,
                _enemy.BasicResolvedCount,
                _enemy.BasicDebt,
                string.Join(";",
                    _enemy.ThresholdOccurrences.Select(value =>
                        value.ThresholdId + ":"
                        + (value.Resolved ? "Resolved" : "Pending"))),
                action == null ? string.Empty : action.ActionPatternId,
                action == null ? string.Empty : action.Status.ToString(),
                _player.currentHp,
                _displayEvents.Count,
                result.accepted,
                result.reason));
        }

        private void AdvanceLifecycle(long battleTick)
        {
            for (int guard = 0; guard < 3; guard++)
            {
                ShougunuPhase1RuntimeSnapshot before = _enemy;
                ShougunuPhase1TransitionResult result =
                    ShougunuPhase1RuntimeReducer.AdvanceBattleTick(
                        _enemy,
                        battleTick);
                if (!result.Accepted)
                {
                    return;
                }
                _enemy = result.State;
                _revision++;
                string ledgerId = AddLedger(
                    ShougunuPhase1BattleLedgerEventKind.EnemyLifecycle,
                    ShougunuPhase1BattleApplicationDecision.Accepted,
                    battleTick,
                    "enemy.lifecycle",
                    _enemy.EnemyInstanceId,
                    string.Empty,
                    0,
                    _enemy.CanonicalSignature);
                CaptureLifecycleCues(before, _enemy, ledgerId);
            }
            throw new InvalidOperationException(
                "Enemy lifecycle transition guard exceeded.");
        }

        private void AdvanceAction(long battleTick)
        {
            ShougunuPhase1RuntimeSnapshot before = _enemy;
            ShougunuPhase1PendingActionSnapshot beforeAction =
                before.ActiveAction;
            bool resolving = beforeAction != null
                && beforeAction.Status == ShougunuPhase1ActionStatus.Windup
                && battleTick >= beforeAction.ResolveTick;
            bool battleAcceptsResolve = !resolving
                || CanApplyEnemyAction(beforeAction.ActionPatternId);
            ShougunuPhase1TransitionResult result =
                ShougunuPhase1ActionScheduler.Advance(
                    before,
                    battleTick,
                    battleAcceptsResolve);
            if (!result.Accepted)
            {
                if (resolving && !battleAcceptsResolve)
                {
                    AddRejectedAction(beforeAction, battleTick,
                        "BATTLE_REJECTED_ACTION_RESOLVE");
                }
                return;
            }

            _enemy = result.State;
            _revision++;
            ShougunuPhase1PendingActionSnapshot afterAction =
                _enemy.ActiveAction;
            if (beforeAction == null && afterAction != null)
            {
                string ledgerId = AddLedger(
                    ShougunuPhase1BattleLedgerEventKind.EnemyActionIntent,
                    ShougunuPhase1BattleApplicationDecision.Accepted,
                    battleTick,
                    afterAction.ActionPatternId,
                    _player.playerActorId,
                    string.Empty,
                    0,
                    afterAction.ExecutionId);
                AddDisplay(
                    ledgerId,
                    battleTick,
                    ShougunuPhase1BattleDisplaySourceKind.Enemy,
                    afterAction.ActionPatternId,
                    _player.playerActorId,
                    ShougunuPhase1BattleDisplayChannel.EnemySkillIntent,
                    0,
                    "battle.enemy.action_intent",
                    80,
                    afterAction.ExecutionId,
                    ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                    "enemy.intent",
                    true,
                    false);
            }
            else if (beforeAction != null
                && beforeAction.Status == ShougunuPhase1ActionStatus.Windup
                && afterAction != null
                && afterAction.Status == ShougunuPhase1ActionStatus.Resolved)
            {
                ApplyResolvedEnemyAction(
                    before,
                    _enemy,
                    afterAction,
                    battleTick);
            }
        }

        private bool CanApplyEnemyAction(string actionPatternId)
        {
            if (_player == null || _player.defeated)
            {
                return false;
            }
            ShougunuPhase1ActionPatternSnapshot pattern =
                ShougunuPhase1ActionPatternCatalog.GetPattern(actionPatternId);
            if (pattern == null)
            {
                return false;
            }
            return string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.BasicAttack,
                    StringComparison.Ordinal)
                || string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair,
                    StringComparison.Ordinal)
                || string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                    StringComparison.Ordinal)
                || string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst,
                    StringComparison.Ordinal);
        }

        private void ApplyResolvedEnemyAction(
            ShougunuPhase1RuntimeSnapshot before,
            ShougunuPhase1RuntimeSnapshot after,
            ShougunuPhase1PendingActionSnapshot action,
            long battleTick)
        {
            ShougunuPhase1ActionPatternSnapshot pattern =
                ShougunuPhase1ActionPatternCatalog.GetPattern(
                    action.ActionPatternId);
            int playerDamage = FixtureDamage(action.ActionPatternId);
            int repairApplied = Math.Max(
                0,
                after.CurrentShell - before.CurrentShell);
            string ledgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.EnemyActionResolve,
                ShougunuPhase1BattleApplicationDecision.Accepted,
                battleTick,
                action.ActionPatternId,
                playerDamage > 0
                    ? _player.playerActorId
                    : after.EnemyInstanceId,
                string.Empty,
                playerDamage > 0 ? playerDamage : repairApplied,
                action.ExecutionId);
            if (playerDamage > 0)
            {
                ApplyPlayerDamage(
                    ledgerId,
                    pattern.EffectRequestId,
                    action.ActionPatternId,
                    playerDamage,
                    battleTick);
            }

            ShougunuPhase1BattleDisplayChannel resolvedChannel =
                string.Equals(
                    action.ActionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst,
                    StringComparison.Ordinal)
                    ? ShougunuPhase1BattleDisplayChannel
                        .EnemyToPlayerStatusOrArea
                    : ShougunuPhase1BattleDisplayChannel.EnemySkillResolved;
            AddDisplay(
                ledgerId,
                battleTick,
                ShougunuPhase1BattleDisplaySourceKind.Enemy,
                action.ActionPatternId,
                playerDamage > 0
                    ? _player.playerActorId
                    : after.EnemyInstanceId,
                resolvedChannel,
                playerDamage > 0 ? playerDamage : repairApplied,
                "battle.enemy.action_resolved",
                90,
                action.ExecutionId,
                string.Equals(
                    action.ActionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst,
                    StringComparison.Ordinal)
                    ? ShougunuPhase1BattleAnchorRoute.StatusDamageAnchor
                    : ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                "enemy.action.resolved",
                true,
                false);
            _enemyActionResults.Add(
                new ShougunuPhase1EnemyActionApplicationResult(
                    ledgerId,
                    action.ExecutionId,
                    action.ActionPatternId,
                    pattern.EffectRequestId,
                    _resetGeneration,
                    battleTick,
                    ShougunuPhase1BattleApplicationDecision.Accepted,
                    string.Empty,
                    playerDamage,
                    repairApplied,
                    _player,
                    _enemy));
            _lastObservedCueSequence = Math.Max(
                _lastObservedCueSequence,
                _enemy.LatestCue == null
                    ? _lastObservedCueSequence
                    : _enemy.LatestCue.CueSequence);
        }

        private void ApplyPlayerDamage(
            string actionLedgerId,
            string effectRequestId,
            string actionPatternId,
            int requestedDamage,
            long battleTick)
        {
            BattleSandboxPlayerCombatantSnapshot before = _player;
            _player = BattleSandboxPlayerCombatantReducer.ApplyEnemyDamage(
                before,
                _resetGeneration,
                effectRequestId,
                requestedDamage);
            int shieldDamage = before.shield - _player.shield;
            int hpDamage = before.currentHp - _player.currentHp;
            string playerLedgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.PlayerEffectApplication,
                ShougunuPhase1BattleApplicationDecision.Accepted,
                battleTick,
                effectRequestId,
                _player.playerActorId,
                string.Empty,
                hpDamage + shieldDamage,
                actionLedgerId);
            _playerResults.Add(
                new BattleSandboxPlayerEffectApplicationResult(
                    playerLedgerId,
                    effectRequestId,
                    actionPatternId,
                    _resetGeneration,
                    battleTick,
                    ShougunuPhase1BattleApplicationDecision.Accepted,
                    string.Empty,
                    requestedDamage,
                    shieldDamage,
                    hpDamage,
                    before.currentHp,
                    _player.currentHp,
                    _player.defeated,
                    _player));
            AddDisplay(
                playerLedgerId,
                battleTick,
                ShougunuPhase1BattleDisplaySourceKind.Enemy,
                actionPatternId,
                _player.playerActorId,
                ShougunuPhase1BattleDisplayChannel.EnemyToPlayerDamage,
                hpDamage + shieldDamage,
                "battle.enemy.player_damage",
                100,
                actionPatternId + ".player",
                string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                    StringComparison.Ordinal)
                    ? ShougunuPhase1BattleAnchorRoute.PlayerHitFeedback
                    : ShougunuPhase1BattleAnchorRoute.DamageTakenAnchor,
                "enemy.damage.player",
                true,
                false);
        }

        private void AddRejectedAction(
            ShougunuPhase1PendingActionSnapshot action,
            long battleTick,
            string reason)
        {
            string source = action == null
                ? "enemy.action.unknown"
                : action.ActionPatternId;
            string ledgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.RejectedOrIgnored,
                ShougunuPhase1BattleApplicationDecision.Rejected,
                battleTick,
                source,
                _player.playerActorId,
                reason,
                0,
                action == null ? string.Empty : action.ExecutionId);
            AddDisplay(
                ledgerId,
                battleTick,
                ShougunuPhase1BattleDisplaySourceKind.Battle,
                source,
                _player.playerActorId,
                ShougunuPhase1BattleDisplayChannel
                    .RejectedOrNotExecutedDeveloperOnly,
                0,
                "battle.enemy.action_rejected",
                0,
                source,
                ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                "developer.diagnostic",
                false,
                true);
            _enemyActionResults.Add(
                new ShougunuPhase1EnemyActionApplicationResult(
                    ledgerId,
                    action == null ? string.Empty : action.ExecutionId,
                    source,
                    string.Empty,
                    _resetGeneration,
                    battleTick,
                    ShougunuPhase1BattleApplicationDecision.Rejected,
                    reason,
                    0,
                    0,
                    _player,
                    _enemy));
        }

        private string ValidateItemRequest(
            ShougunuPhase1ItemRequestSourceFacts sourceFacts,
            ItemCombatEffectRequestRow row,
            string requestEventId,
            string applicationEventId,
            string targetActorId,
            string targetContentId,
            string targetPhaseId,
            int resetGeneration,
            long battleTick,
            long applicationSequence,
            long expectedEnemyRevision)
        {
            if (sourceFacts == null) return "SOURCE_FACTS_NULL";
            if (!string.Equals(
                sourceFacts.schemaId,
                ItemCombatEffectRequestSnapshot.CurrentSchemaId,
                StringComparison.Ordinal))
                return "ITEM_SCHEMA_MISMATCH";
            if (sourceFacts.status
                != ItemCombatEffectRequestSnapshotStatus.Valid)
                return "ITEM_SNAPSHOT_NOT_VALID";
            if (!sourceFacts.devOnly) return "ITEM_SNAPSHOT_NOT_DEV_ONLY";
            if (sourceFacts.entersFormalBattle)
                return "FORMAL_ITEM_REQUEST_REJECTED";
            if (row == null) return "ITEM_REQUEST_ROW_NULL";
            if (row.resolvedPreMitigationDamageUnits < 0)
                return "NEGATIVE_DAMAGE_REJECTED";
            if (row.resolvedPreMitigationDamageUnits == 0)
                return "ZERO_DAMAGE_REJECTED";
            if (row.resolvedPreMitigationDamageUnits > int.MaxValue)
                return "DAMAGE_EXCEEDS_INT_RANGE";
            if (row.requestKind
                    != ItemCombatEffectRequestKind.DirectFlatDamage
                || row.targetRequestKind
                    != ItemCombatEffectTargetRequestKind
                        .SingleHostileDamageableRuntimeActor
                || !row.requiresBattleTargetValidation
                || row.magnitudeCompleteness
                    != ItemCombatEffectMagnitudeCompleteness.Complete)
                return "UNSUPPORTED_ITEM_DAMAGE_REQUEST";
            if (row.isLitFact
                    != ItemInstanceQualifiedBuildBooleanFact.True
                || row.sourceIsCountedFact
                    != ItemInstanceQualifiedBuildBooleanFact.True)
                return "ITEM_ACCEPTED_TRUTH_REQUIRED";
            if (!_orderedRows.Any(value =>
                    string.Equals(
                        value.requestId,
                        row.requestId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.sourceItemInstanceId,
                        row.sourceItemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.sourcePlacementId,
                        row.sourcePlacementId,
                        StringComparison.Ordinal)
                    && value.resolvedPreMitigationDamageUnits
                        == row.resolvedPreMitigationDamageUnits
                    && value.isLitFact == row.isLitFact
                    && value.sourceIsCountedFact
                        == row.sourceIsCountedFact
                    && value.qualifiedIsCountedFact
                        == row.qualifiedIsCountedFact
                    && string.Equals(
                        value.sourceProjectionCanonicalSignature,
                        row.sourceProjectionCanonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.sourcePlacementCanonicalFacts,
                        row.sourcePlacementCanonicalFacts,
                        StringComparison.Ordinal)))
                return "ITEM_ROW_NOT_FROM_TERMINAL_SNAPSHOT";
            if (string.IsNullOrWhiteSpace(requestEventId))
                return "REQUEST_EVENT_ID_REQUIRED";
            if (string.IsNullOrWhiteSpace(applicationEventId))
                return "APPLICATION_EVENT_ID_REQUIRED";
            if (_acceptedRequestEventIds.Contains(requestEventId))
                return "DUPLICATE_REQUEST_REJECTED";
            if (_acceptedApplicationEventIds.Contains(applicationEventId)
                || _enemy.AcceptedApplicationEventIds.Contains(
                    applicationEventId,
                    StringComparer.Ordinal))
                return "DUPLICATE_APPLICATION_REJECTED";
            if (resetGeneration != _resetGeneration
                || resetGeneration != _enemy.ResetGeneration)
                return "STALE_RESET_GENERATION_REJECTED";
            if (expectedEnemyRevision != _enemy.Revision)
                return "STALE_ENEMY_REVISION_REJECTED";
            if (applicationSequence <=
                _enemy.LastAcceptedApplicationSequence)
                return "STALE_APPLICATION_SEQUENCE_REJECTED";
            if (battleTick < _enemy.LastAcceptedBattleTick)
                return "STALE_BATTLE_TICK_REJECTED";
            if (!string.Equals(
                    targetActorId,
                    _enemy.EnemyInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    targetContentId,
                    ShougunuPhase1RuntimeContract.ContentId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    targetPhaseId,
                    ShougunuPhase1RuntimeContract.PhaseId,
                    StringComparison.Ordinal))
                return "INVALID_TARGET_REJECTED";
            if (!_enemy.DevOnly
                || _enemy.EntersFormalFlow
                || _enemy.RuntimeBoundToBattle
                || _enemy.CurrentHp <= 0
                || !_enemy.Targetable
                || (_enemy.Lifecycle
                    != ShougunuPhase1LifecycleState.LayeredShellOn
                    && _enemy.Lifecycle
                    != ShougunuPhase1LifecycleState.CoreExposed))
                return "DAMAGE_CHANNEL_LIFECYCLE_REJECTED";
            return string.Empty;
        }

        private ShougunuPhase1ItemApplicationResult RejectItem(
            ItemCombatEffectRequestRow row,
            string requestEventId,
            string applicationEventId,
            string targetActorId,
            int resetGeneration,
            long battleTick,
            long applicationSequence,
            long expectedEnemyRevision,
            string reason)
        {
            string ledgerId = AddLedger(
                ShougunuPhase1BattleLedgerEventKind.RejectedOrIgnored,
                ShougunuPhase1BattleApplicationDecision.Rejected,
                battleTick,
                row == null ? "item.request.null" : row.requestId,
                targetActorId,
                reason,
                0,
                applicationEventId);
            AddDisplay(
                ledgerId,
                battleTick,
                ShougunuPhase1BattleDisplaySourceKind.Battle,
                row == null ? "item.request.null" : row.requestId,
                targetActorId,
                ShougunuPhase1BattleDisplayChannel
                    .RejectedOrNotExecutedDeveloperOnly,
                0,
                "battle.item.request_rejected",
                0,
                applicationEventId,
                ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                "developer.diagnostic",
                false,
                true);
            return new ShougunuPhase1ItemApplicationResult(
                ledgerId,
                requestEventId,
                applicationEventId,
                row == null ? string.Empty : row.requestId,
                row == null ? string.Empty : row.sourceBaseItemId,
                row == null ? string.Empty : row.sourceItemInstanceId,
                row == null ? string.Empty : row.sourcePlacementId,
                targetActorId,
                resetGeneration,
                battleTick,
                applicationSequence,
                expectedEnemyRevision,
                ShougunuPhase1BattleApplicationDecision.Rejected,
                reason,
                row == null
                    || row.resolvedPreMitigationDamageUnits < int.MinValue
                    || row.resolvedPreMitigationDamageUnits > int.MaxValue
                    ? 0
                    : (int)row.resolvedPreMitigationDamageUnits,
                0,
                0,
                _enemy);
        }

        private void CaptureLifecycleCues(
            ShougunuPhase1RuntimeSnapshot before,
            ShougunuPhase1RuntimeSnapshot after,
            string ledgerEventId)
        {
            foreach (ShougunuPhase1CueSnapshot cue in after.Cues
                .Where(value => value.CueSequence > _lastObservedCueSequence)
                .OrderBy(value => value.CueSequence))
            {
                ShougunuPhase1BattleDisplayChannel channel;
                string message;
                ShougunuPhase1BattleAnchorRoute anchor;
                if (cue.CueKind == ShougunuPhase1CueKind.ShellBreak)
                {
                    channel =
                        ShougunuPhase1BattleDisplayChannel.EnemyShellBreak;
                    message = "battle.enemy.shell_break";
                    anchor =
                        ShougunuPhase1BattleAnchorRoute.ShieldBreakAnchor;
                }
                else if (cue.CueKind == ShougunuPhase1CueKind.CoreExpose)
                {
                    channel =
                        ShougunuPhase1BattleDisplayChannel.EnemyCoreExpose;
                    message = "battle.enemy.core_expose";
                    anchor =
                        ShougunuPhase1BattleAnchorRoute.MechanicFloatingText;
                }
                else if (cue.CueKind == ShougunuPhase1CueKind.Recover)
                {
                    channel = ShougunuPhase1BattleDisplayChannel.EnemyRecover;
                    message = "battle.enemy.recover";
                    anchor =
                        ShougunuPhase1BattleAnchorRoute.MechanicFloatingText;
                }
                else if (cue.CueKind == ShougunuPhase1CueKind.Defeated)
                {
                    channel =
                        ShougunuPhase1BattleDisplayChannel.EnemyDefeated;
                    message = "battle.enemy.defeated";
                    anchor =
                        ShougunuPhase1BattleAnchorRoute.MechanicFloatingText;
                }
                else
                {
                    _lastObservedCueSequence = Math.Max(
                        _lastObservedCueSequence,
                        cue.CueSequence);
                    continue;
                }
                AddDisplay(
                    ledgerEventId,
                    cue.BattleTick,
                    ShougunuPhase1BattleDisplaySourceKind.Enemy,
                    cue.SourceEventId,
                    after.EnemyInstanceId,
                    channel,
                    0,
                    message,
                    95,
                    cue.CueId,
                    anchor,
                    "enemy.mechanic",
                    true,
                    false);
                _lastObservedCueSequence = Math.Max(
                    _lastObservedCueSequence,
                    cue.CueSequence);
            }
        }

        private string AddLedger(
            ShougunuPhase1BattleLedgerEventKind eventKind,
            ShougunuPhase1BattleApplicationDecision decision,
            long battleTick,
            string sourceId,
            string targetActorId,
            string reason,
            int amount,
            string evidence)
        {
            long sequence = _nextLedgerSequence++;
            string ledgerId = "battle.ledger.g"
                + _resetGeneration.ToString(CultureInfo.InvariantCulture)
                + "." + sequence.ToString("D4", CultureInfo.InvariantCulture);
            _ledgerEvents.Add(new ShougunuPhase1BattleLedgerEvent(
                ledgerId,
                sequence,
                battleTick,
                _resetGeneration,
                eventKind,
                decision,
                sourceId,
                targetActorId,
                reason,
                amount,
                evidence));
            return ledgerId;
        }

        private void AddDisplay(
            string ledgerEventId,
            long battleTick,
            ShougunuPhase1BattleDisplaySourceKind sourceKind,
            string sourceId,
            string targetActorId,
            ShougunuPhase1BattleDisplayChannel channel,
            int amount,
            string messageKey,
            int priority,
            string aggregationKey,
            ShougunuPhase1BattleAnchorRoute anchorRoute,
            string colorPolicy,
            bool playerVisible,
            bool developerOnly)
        {
            long sequence = _nextDisplaySequence++;
            _displayEvents.Add(new ShougunuPhase1BattleDisplayEvent(
                "battle.display.g"
                    + _resetGeneration.ToString(CultureInfo.InvariantCulture)
                    + "." + sequence.ToString(
                        "D4",
                        CultureInfo.InvariantCulture),
                ledgerEventId,
                battleTick,
                _resetGeneration,
                sequence,
                sourceKind,
                sourceId,
                targetActorId,
                channel,
                amount,
                messageKey,
                priority,
                aggregationKey,
                anchorRoute,
                colorPolicy,
                playerVisible,
                developerOnly));
        }

        private ShougunuPhase1BattleApplicationContext BuildContext()
        {
            return new ShougunuPhase1BattleApplicationContext(
                _resetGeneration,
                _revision,
                _battleTick,
                ShougunuPhase1BattleApplicationEngine.TicksPerSecond,
                ShougunuPhase1BattleApplicationEngine
                    .ItemPulseIntervalTicks,
                _nextItemPulseTick,
                _nextItemOrdinal,
                _itemSnapshot.canonicalSignature,
                _enemy,
                _player,
                new ShougunuPhase1BattleApplicationLedger(
                    _resetGeneration,
                    _ledgerEvents),
                _displayEvents);
        }

        private static BattleSandboxPlayerCombatantSnapshot NewPlayer(
            int resetGeneration,
            long revision)
        {
            return new BattleSandboxPlayerCombatantSnapshot(
                resetGeneration,
                revision,
                ShougunuPhase1BattleApplicationEngine.PlayerActorId,
                BattleSandboxPlayerCombatantSnapshot.FixtureMaxHp,
                0,
                false,
                null,
                0,
                new[]
                {
                    "devOnly fixture: max/current/reset HP=9999",
                    "damage and Defeated remain enabled"
                });
        }

        private static int FixtureDamage(string actionPatternId)
        {
            if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.BasicAttack,
                StringComparison.Ordinal))
            {
                return ShougunuPhase1BattleApplicationEngine
                    .BasicAttackPlayerDamageFixture;
            }
            if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                StringComparison.Ordinal))
            {
                return ShougunuPhase1BattleApplicationEngine
                    .Skill2PlayerDamageFixture;
            }
            if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst,
                StringComparison.Ordinal))
            {
                return ShougunuPhase1BattleApplicationEngine
                    .Skill3PlayerDamageFixture;
            }
            return 0;
        }

        private static ReadOnlyCollection<ItemCombatEffectRequestRow>
            SelectTerminalRows(ItemCombatEffectRequestSnapshot snapshot)
        {
            if (!string.Equals(
                    snapshot.schemaId,
                    ItemCombatEffectRequestSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                || snapshot.status
                    != ItemCombatEffectRequestSnapshotStatus.Valid
                || !snapshot.devOnly
                || snapshot.entersFormalBattle)
            {
                throw new InvalidOperationException(
                    "Terminal Item request snapshot facts are invalid.");
            }

            List<ItemCombatEffectRequestRow> rows =
                new List<ItemCombatEffectRequestRow>();
            for (int index = 0;
                 index < ShougunuPhase1BattleApplicationEngine.ItemIds.Count;
                 index++)
            {
                string itemId =
                    ShougunuPhase1BattleApplicationEngine.ItemIds[index];
                ItemCombatEffectRequestRow[] matches = snapshot.Requests
                    .Where(value => string.Equals(
                        value.sourceBaseItemId,
                        itemId,
                        StringComparison.Ordinal))
                    .ToArray();
                if (matches.Length != 1)
                {
                    throw new InvalidOperationException(
                        "Terminal Item request row cardinality mismatch: "
                        + itemId);
                }
                ItemCombatEffectRequestRow row = matches[0];
                rows.Add(row);
            }
            return Array.AsReadOnly(rows.ToArray());
        }
    }
}
