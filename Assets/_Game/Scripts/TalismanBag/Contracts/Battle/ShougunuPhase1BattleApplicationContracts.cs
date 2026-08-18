using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.Items.Combat;

namespace TalismanBag.Contracts.Battle
{
    public enum ShougunuPhase1BattleApplicationDecision
    {
        Accepted = 0,
        Rejected = 1,
        IgnoredDiagnosticOnly = 2
    }

    public enum ShougunuPhase1BattleLedgerEventKind
    {
        ItemApplication = 0,
        EnemyActionIntent = 1,
        EnemyActionResolve = 2,
        EnemyLifecycle = 3,
        PlayerEffectApplication = 4,
        RejectedOrIgnored = 5,
        Reset = 6,
        Defeat = 7
    }

    public enum ShougunuPhase1BattleDisplaySourceKind
    {
        Item = 0,
        Enemy = 1,
        Battle = 2
    }

    public enum ShougunuPhase1BattleDisplayChannel
    {
        ItemToEnemyShellDamage = 0,
        ItemToEnemyHpDamage = 1,
        EnemyShellBreak = 2,
        EnemyCoreExpose = 3,
        EnemyRecover = 4,
        EnemyDefeated = 5,
        EnemyToPlayerDamage = 6,
        EnemyToPlayerStatusOrArea = 7,
        EnemySkillIntent = 8,
        EnemySkillResolved = 9,
        RejectedOrNotExecutedDeveloperOnly = 10
    }

    public enum ShougunuPhase1BattleAnchorRoute
    {
        DamageDealtAnchor = 0,
        ShieldBreakAnchor = 1,
        DamageTakenAnchor = 2,
        StatusDamageAnchor = 3,
        MechanicFloatingText = 4,
        PlayerHitFeedback = 5
    }

    public sealed class BattleSandboxPlayerCombatantSnapshot
    {
        public const string CurrentSchemaId =
            "BattleSandboxPlayerCombatantSnapshot.v1";
        public const int FixtureMaxHp = 9999;

        public BattleSandboxPlayerCombatantSnapshot(
            int resetGeneration,
            long revision,
            string playerActorId,
            int currentHp,
            int shield,
            bool defeated,
            string lastAcceptedEnemyEffectId,
            int acceptedEnemyEffectCount,
            IEnumerable<string> developerDiagnostics)
        {
            schemaId = CurrentSchemaId;
            devOnly = true;
            this.resetGeneration = resetGeneration;
            this.revision = revision;
            this.playerActorId = BattleContractCanonical.Required(playerActorId);
            maxHp = FixtureMaxHp;
            this.currentHp = Math.Max(0, Math.Min(maxHp, currentHp));
            this.shield = Math.Max(0, shield);
            this.defeated = defeated || this.currentHp == 0;
            this.lastAcceptedEnemyEffectId =
                BattleContractCanonical.Optional(lastAcceptedEnemyEffectId);
            this.acceptedEnemyEffectCount = Math.Max(0, acceptedEnemyEffectCount);
            this.developerDiagnostics = BattleContractReadOnly.FreezeText(
                developerDiagnostics, false);
            canonicalSignature = BattleContractCanonical.Hash(string.Join("|", new[]
            {
                schemaId,
                BattleContractCanonical.Bool(devOnly),
                BattleContractCanonical.Int(resetGeneration),
                BattleContractCanonical.Long(revision),
                this.playerActorId,
                BattleContractCanonical.Int(maxHp),
                BattleContractCanonical.Int(this.currentHp),
                BattleContractCanonical.Int(this.shield),
                BattleContractCanonical.Bool(this.defeated),
                this.lastAcceptedEnemyEffectId ?? string.Empty,
                BattleContractCanonical.Int(this.acceptedEnemyEffectCount),
                string.Join(";", this.developerDiagnostics)
            }));
        }

        public string schemaId { get; }
        public bool devOnly { get; }
        public int resetGeneration { get; }
        public long revision { get; }
        public string playerActorId { get; }
        public int maxHp { get; }
        public int currentHp { get; }
        public int shield { get; }
        public bool defeated { get; }
        public string lastAcceptedEnemyEffectId { get; }
        public int acceptedEnemyEffectCount { get; }
        public string canonicalSignature { get; }
        public IReadOnlyList<string> developerDiagnostics { get; }
    }

    public sealed class BattleSandboxPlayerEffectApplicationResult
    {
        public BattleSandboxPlayerEffectApplicationResult(
            string ledgerEventId,
            string enemyEffectId,
            string actionPatternId,
            int resetGeneration,
            long battleTick,
            ShougunuPhase1BattleApplicationDecision decision,
            string reason,
            int requestedDamage,
            int shieldDamageApplied,
            int hpDamageApplied,
            int beforeHp,
            int afterHp,
            bool defeated,
            BattleSandboxPlayerCombatantSnapshot player)
        {
            this.ledgerEventId = BattleContractCanonical.Required(ledgerEventId);
            this.enemyEffectId = BattleContractCanonical.Required(enemyEffectId);
            this.actionPatternId = BattleContractCanonical.Required(actionPatternId);
            this.resetGeneration = resetGeneration;
            this.battleTick = battleTick;
            this.decision = decision;
            this.reason = reason ?? string.Empty;
            this.requestedDamage = requestedDamage;
            this.shieldDamageApplied = shieldDamageApplied;
            this.hpDamageApplied = hpDamageApplied;
            this.beforeHp = beforeHp;
            this.afterHp = afterHp;
            this.defeated = defeated;
            this.player = player;
        }

        public string ledgerEventId { get; }
        public string enemyEffectId { get; }
        public string actionPatternId { get; }
        public int resetGeneration { get; }
        public long battleTick { get; }
        public ShougunuPhase1BattleApplicationDecision decision { get; }
        public string reason { get; }
        public int requestedDamage { get; }
        public int shieldDamageApplied { get; }
        public int hpDamageApplied { get; }
        public int beforeHp { get; }
        public int afterHp { get; }
        public bool defeated { get; }
        public BattleSandboxPlayerCombatantSnapshot player { get; }
    }

    public sealed class ShougunuPhase1ItemRequestSourceFacts
    {
        public ShougunuPhase1ItemRequestSourceFacts(
            string schemaId,
            ItemCombatEffectRequestSnapshotStatus status,
            bool devOnly,
            bool entersFormalBattle,
            string canonicalSignature)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.status = status;
            this.devOnly = devOnly;
            this.entersFormalBattle = entersFormalBattle;
            this.canonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string schemaId { get; }
        public ItemCombatEffectRequestSnapshotStatus status { get; }
        public bool devOnly { get; }
        public bool entersFormalBattle { get; }
        public string canonicalSignature { get; }

        public static ShougunuPhase1ItemRequestSourceFacts FromSnapshot(
            ItemCombatEffectRequestSnapshot snapshot)
        {
            return snapshot == null
                ? new ShougunuPhase1ItemRequestSourceFacts(
                    string.Empty,
                    ItemCombatEffectRequestSnapshotStatus.Invalid,
                    false,
                    true,
                    string.Empty)
                : new ShougunuPhase1ItemRequestSourceFacts(
                    snapshot.schemaId,
                    snapshot.status,
                    snapshot.devOnly,
                    snapshot.entersFormalBattle,
                    snapshot.canonicalSignature);
        }
    }

    public sealed class ShougunuPhase1ItemApplicationResult
    {
        public ShougunuPhase1ItemApplicationResult(
            string ledgerEventId,
            string requestEventId,
            string applicationEventId,
            string sourceRequestId,
            string sourceBaseItemId,
            string sourceItemInstanceId,
            string sourcePlacementId,
            string targetActorId,
            int resetGeneration,
            long battleTick,
            long applicationSequence,
            long expectedEnemyRevision,
            ShougunuPhase1BattleApplicationDecision decision,
            string reason,
            int requestedDamage,
            int shellDamageApplied,
            int hpDamageApplied,
            ShougunuPhase1RuntimeSnapshot enemy)
        {
            this.ledgerEventId = BattleContractCanonical.Required(ledgerEventId);
            this.requestEventId = BattleContractCanonical.Required(requestEventId);
            this.applicationEventId = BattleContractCanonical.Required(applicationEventId);
            this.sourceRequestId = sourceRequestId ?? string.Empty;
            this.sourceBaseItemId = sourceBaseItemId ?? string.Empty;
            this.sourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            this.sourcePlacementId = sourcePlacementId ?? string.Empty;
            this.targetActorId = targetActorId ?? string.Empty;
            this.resetGeneration = resetGeneration;
            this.battleTick = battleTick;
            this.applicationSequence = applicationSequence;
            this.expectedEnemyRevision = expectedEnemyRevision;
            this.decision = decision;
            this.reason = reason ?? string.Empty;
            this.requestedDamage = requestedDamage;
            this.shellDamageApplied = shellDamageApplied;
            this.hpDamageApplied = hpDamageApplied;
            this.enemy = enemy;
        }

        public string ledgerEventId { get; }
        public string requestEventId { get; }
        public string applicationEventId { get; }
        public string sourceRequestId { get; }
        public string sourceBaseItemId { get; }
        public string sourceItemInstanceId { get; }
        public string sourcePlacementId { get; }
        public string targetActorId { get; }
        public int resetGeneration { get; }
        public long battleTick { get; }
        public long applicationSequence { get; }
        public long expectedEnemyRevision { get; }
        public ShougunuPhase1BattleApplicationDecision decision { get; }
        public string reason { get; }
        public int requestedDamage { get; }
        public int shellDamageApplied { get; }
        public int hpDamageApplied { get; }
        public ShougunuPhase1RuntimeSnapshot enemy { get; }
        public bool accepted =>
            decision == ShougunuPhase1BattleApplicationDecision.Accepted;
    }

    public sealed class ShougunuPhase1EnemyActionApplicationResult
    {
        public ShougunuPhase1EnemyActionApplicationResult(
            string ledgerEventId,
            string executionId,
            string actionPatternId,
            string effectRequestId,
            int resetGeneration,
            long battleTick,
            ShougunuPhase1BattleApplicationDecision decision,
            string reason,
            int playerDamageApplied,
            int enemyShellRepairApplied,
            BattleSandboxPlayerCombatantSnapshot player,
            ShougunuPhase1RuntimeSnapshot enemy)
        {
            this.ledgerEventId = BattleContractCanonical.Required(ledgerEventId);
            this.executionId = executionId ?? string.Empty;
            this.actionPatternId = actionPatternId ?? string.Empty;
            this.effectRequestId = effectRequestId ?? string.Empty;
            this.resetGeneration = resetGeneration;
            this.battleTick = battleTick;
            this.decision = decision;
            this.reason = reason ?? string.Empty;
            this.playerDamageApplied = playerDamageApplied;
            this.enemyShellRepairApplied = enemyShellRepairApplied;
            this.player = player;
            this.enemy = enemy;
        }

        public string ledgerEventId { get; }
        public string executionId { get; }
        public string actionPatternId { get; }
        public string effectRequestId { get; }
        public int resetGeneration { get; }
        public long battleTick { get; }
        public ShougunuPhase1BattleApplicationDecision decision { get; }
        public string reason { get; }
        public int playerDamageApplied { get; }
        public int enemyShellRepairApplied { get; }
        public BattleSandboxPlayerCombatantSnapshot player { get; }
        public ShougunuPhase1RuntimeSnapshot enemy { get; }
        public bool accepted =>
            decision == ShougunuPhase1BattleApplicationDecision.Accepted;
    }

    public sealed class ShougunuPhase1BattleDisplayEvent
    {
        public ShougunuPhase1BattleDisplayEvent(
            string displayEventId,
            string ledgerEventId,
            long battleTick,
            int resetGeneration,
            long sequence,
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
            this.displayEventId = BattleContractCanonical.Required(displayEventId);
            this.ledgerEventId = BattleContractCanonical.Required(ledgerEventId);
            this.battleTick = battleTick;
            this.resetGeneration = resetGeneration;
            this.sequence = sequence;
            this.sourceKind = sourceKind;
            this.sourceId = sourceId ?? string.Empty;
            this.targetActorId = targetActorId ?? string.Empty;
            this.channel = channel;
            this.amount = amount;
            this.messageKey = messageKey ?? string.Empty;
            this.priority = priority;
            this.aggregationKey = aggregationKey ?? string.Empty;
            this.anchorRoute = anchorRoute;
            this.colorPolicy = colorPolicy ?? string.Empty;
            this.playerVisible = playerVisible;
            this.developerOnly = developerOnly;
        }

        public string displayEventId { get; }
        public string ledgerEventId { get; }
        public long battleTick { get; }
        public int resetGeneration { get; }
        public long sequence { get; }
        public ShougunuPhase1BattleDisplaySourceKind sourceKind { get; }
        public string sourceId { get; }
        public string targetActorId { get; }
        public ShougunuPhase1BattleDisplayChannel channel { get; }
        public int amount { get; }
        public string messageKey { get; }
        public int priority { get; }
        public string aggregationKey { get; }
        public ShougunuPhase1BattleAnchorRoute anchorRoute { get; }
        public string colorPolicy { get; }
        public bool playerVisible { get; }
        public bool developerOnly { get; }
    }

    public sealed class ShougunuPhase1BattleLedgerEvent
    {
        public ShougunuPhase1BattleLedgerEvent(
            string ledgerEventId,
            long sequence,
            long battleTick,
            int resetGeneration,
            ShougunuPhase1BattleLedgerEventKind eventKind,
            ShougunuPhase1BattleApplicationDecision decision,
            string sourceId,
            string targetActorId,
            string reason,
            int amount,
            string canonicalEvidence)
        {
            this.ledgerEventId = BattleContractCanonical.Required(ledgerEventId);
            this.sequence = sequence;
            this.battleTick = battleTick;
            this.resetGeneration = resetGeneration;
            this.eventKind = eventKind;
            this.decision = decision;
            this.sourceId = sourceId ?? string.Empty;
            this.targetActorId = targetActorId ?? string.Empty;
            this.reason = reason ?? string.Empty;
            this.amount = amount;
            this.canonicalEvidence = canonicalEvidence ?? string.Empty;
        }

        public string ledgerEventId { get; }
        public long sequence { get; }
        public long battleTick { get; }
        public int resetGeneration { get; }
        public ShougunuPhase1BattleLedgerEventKind eventKind { get; }
        public ShougunuPhase1BattleApplicationDecision decision { get; }
        public string sourceId { get; }
        public string targetActorId { get; }
        public string reason { get; }
        public int amount { get; }
        public string canonicalEvidence { get; }
    }

    public sealed class ShougunuPhase1BattleApplicationLedger
    {
        public const string CurrentSchemaId =
            "ShougunuPhase1BattleApplicationLedger.v1";

        public ShougunuPhase1BattleApplicationLedger(
            int resetGeneration,
            IEnumerable<ShougunuPhase1BattleLedgerEvent> events)
        {
            schemaId = CurrentSchemaId;
            this.resetGeneration = resetGeneration;
            Events = BattleContractReadOnly.Freeze(events);
            acceptedCount = Events.Count(value =>
                value.decision == ShougunuPhase1BattleApplicationDecision.Accepted);
            rejectedOrIgnoredCount = Events.Count - acceptedCount;
            canonicalSignature = BattleContractCanonical.Hash(string.Join("\n",
                Events.Select(value => string.Join("|", new[]
                {
                    value.ledgerEventId,
                    BattleContractCanonical.Long(value.sequence),
                    BattleContractCanonical.Long(value.battleTick),
                    BattleContractCanonical.Int(value.resetGeneration),
                    value.eventKind.ToString(),
                    value.decision.ToString(),
                    value.sourceId,
                    value.targetActorId,
                    value.reason,
                    BattleContractCanonical.Int(value.amount),
                    value.canonicalEvidence
                }))));
        }

        public string schemaId { get; }
        public int resetGeneration { get; }
        public int acceptedCount { get; }
        public int rejectedOrIgnoredCount { get; }
        public IReadOnlyList<ShougunuPhase1BattleLedgerEvent> Events { get; }
        public string canonicalSignature { get; }
    }

    public sealed class ShougunuPhase1BattleApplicationContext
    {
        public const string CurrentSchemaId =
            "ShougunuPhase1BattleApplicationContext.v1";

        public ShougunuPhase1BattleApplicationContext(
            int resetGeneration,
            long revision,
            long battleTick,
            int ticksPerSecond,
            long itemPulseIntervalTicks,
            long nextItemPulseTick,
            int nextItemOrdinal,
            string sourceItemSnapshotCanonicalSignature,
            ShougunuPhase1RuntimeSnapshot enemy,
            BattleSandboxPlayerCombatantSnapshot player,
            ShougunuPhase1BattleApplicationLedger ledger,
            IEnumerable<ShougunuPhase1BattleDisplayEvent> displayEvents)
        {
            schemaId = CurrentSchemaId;
            devOnly = true;
            entersFormalBattle = false;
            this.resetGeneration = resetGeneration;
            this.revision = revision;
            this.battleTick = battleTick;
            this.ticksPerSecond = ticksPerSecond;
            this.itemPulseIntervalTicks = itemPulseIntervalTicks;
            this.nextItemPulseTick = nextItemPulseTick;
            this.nextItemOrdinal = nextItemOrdinal;
            this.sourceItemSnapshotCanonicalSignature =
                sourceItemSnapshotCanonicalSignature ?? string.Empty;
            this.enemy = enemy;
            this.player = player;
            this.ledger = ledger;
            DisplayEvents = BattleContractReadOnly.Freeze(displayEvents);
            canonicalSignature = BattleContractCanonical.Hash(string.Join("|", new[]
            {
                schemaId,
                BattleContractCanonical.Int(resetGeneration),
                BattleContractCanonical.Long(revision),
                BattleContractCanonical.Long(battleTick),
                BattleContractCanonical.Long(nextItemPulseTick),
                BattleContractCanonical.Int(nextItemOrdinal),
                this.sourceItemSnapshotCanonicalSignature,
                enemy == null ? string.Empty : enemy.CanonicalSignature,
                player == null ? string.Empty : player.canonicalSignature,
                ledger == null ? string.Empty : ledger.canonicalSignature,
                BattleContractCanonical.Int(DisplayEvents.Count)
            }));
        }

        public string schemaId { get; }
        public bool devOnly { get; }
        public bool entersFormalBattle { get; }
        public int resetGeneration { get; }
        public long revision { get; }
        public long battleTick { get; }
        public int ticksPerSecond { get; }
        public long itemPulseIntervalTicks { get; }
        public long nextItemPulseTick { get; }
        public int nextItemOrdinal { get; }
        public string sourceItemSnapshotCanonicalSignature { get; }
        public ShougunuPhase1RuntimeSnapshot enemy { get; }
        public BattleSandboxPlayerCombatantSnapshot player { get; }
        public ShougunuPhase1BattleApplicationLedger ledger { get; }
        public IReadOnlyList<ShougunuPhase1BattleDisplayEvent> DisplayEvents { get; }
        public string canonicalSignature { get; }
    }

    public sealed class ShougunuPhase1BattleApplicationTraceRow
    {
        public ShougunuPhase1BattleApplicationTraceRow(
            long applicationSequence,
            long battleTick,
            string sourceBaseItemId,
            string sourceItemInstanceId,
            string requestId,
            int resolvedPreMitigationDamageUnits,
            int shellDamageApplied,
            int hpDamageApplied,
            string enemyLifecycle,
            int enemyCurrentHp,
            int enemyCurrentShell,
            int basicEarned,
            int basicResolved,
            int basicDebt,
            string thresholdOccurrences,
            string activeActionId,
            string activeActionStatus,
            int playerCurrentHp,
            int displayEventCount,
            bool accepted,
            string reason)
        {
            this.applicationSequence = applicationSequence;
            this.battleTick = battleTick;
            this.sourceBaseItemId = sourceBaseItemId ?? string.Empty;
            this.sourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            this.requestId = requestId ?? string.Empty;
            this.resolvedPreMitigationDamageUnits =
                resolvedPreMitigationDamageUnits;
            this.shellDamageApplied = shellDamageApplied;
            this.hpDamageApplied = hpDamageApplied;
            this.enemyLifecycle = enemyLifecycle ?? string.Empty;
            this.enemyCurrentHp = enemyCurrentHp;
            this.enemyCurrentShell = enemyCurrentShell;
            this.basicEarned = basicEarned;
            this.basicResolved = basicResolved;
            this.basicDebt = basicDebt;
            this.thresholdOccurrences = thresholdOccurrences ?? string.Empty;
            this.activeActionId = activeActionId ?? string.Empty;
            this.activeActionStatus = activeActionStatus ?? string.Empty;
            this.playerCurrentHp = playerCurrentHp;
            this.displayEventCount = displayEventCount;
            this.accepted = accepted;
            this.reason = reason ?? string.Empty;
        }

        public long applicationSequence { get; }
        public long battleTick { get; }
        public string sourceBaseItemId { get; }
        public string sourceItemInstanceId { get; }
        public string requestId { get; }
        public int resolvedPreMitigationDamageUnits { get; }
        public int shellDamageApplied { get; }
        public int hpDamageApplied { get; }
        public string enemyLifecycle { get; }
        public int enemyCurrentHp { get; }
        public int enemyCurrentShell { get; }
        public int basicEarned { get; }
        public int basicResolved { get; }
        public int basicDebt { get; }
        public string thresholdOccurrences { get; }
        public string activeActionId { get; }
        public string activeActionStatus { get; }
        public int playerCurrentHp { get; }
        public int displayEventCount { get; }
        public bool accepted { get; }
        public string reason { get; }
    }

    public sealed class ShougunuPhase1BattleApplicationTrace
    {
        public const string CurrentSchemaId =
            "ShougunuPhase1BattleApplicationTrace.v1";

        public ShougunuPhase1BattleApplicationTrace(
            ShougunuPhase1BattleApplicationContext finalContext,
            IEnumerable<ShougunuPhase1BattleApplicationTraceRow> rows,
            IEnumerable<BattleSandboxPlayerEffectApplicationResult> playerResults,
            IEnumerable<ShougunuPhase1EnemyActionApplicationResult> enemyActionResults)
        {
            schemaId = CurrentSchemaId;
            this.finalContext = finalContext;
            Rows = BattleContractReadOnly.Freeze(rows);
            PlayerResults = BattleContractReadOnly.Freeze(playerResults);
            EnemyActionResults = BattleContractReadOnly.Freeze(enemyActionResults);
            canonicalSignature = BattleContractCanonical.Hash(string.Join("|", new[]
            {
                schemaId,
                finalContext == null ? string.Empty : finalContext.canonicalSignature,
                BattleContractCanonical.Int(Rows.Count),
                BattleContractCanonical.Int(PlayerResults.Count),
                BattleContractCanonical.Int(EnemyActionResults.Count)
            }));
        }

        public string schemaId { get; }
        public ShougunuPhase1BattleApplicationContext finalContext { get; }
        public IReadOnlyList<ShougunuPhase1BattleApplicationTraceRow> Rows { get; }
        public IReadOnlyList<BattleSandboxPlayerEffectApplicationResult> PlayerResults { get; }
        public IReadOnlyList<ShougunuPhase1EnemyActionApplicationResult> EnemyActionResults { get; }
        public string canonicalSignature { get; }
    }

    internal static class BattleContractReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Enumerable.Empty<T>()).ToArray());
        }

        public static ReadOnlyCollection<string> FreezeText(
            IEnumerable<string> values,
            bool sort)
        {
            IEnumerable<string> query = (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal);
            if (sort)
            {
                query = query.OrderBy(value => value, StringComparer.Ordinal);
            }
            return Array.AsReadOnly(query.ToArray());
        }
    }

    internal static class BattleContractCanonical
    {
        public static string Required(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static string Optional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static string Bool(bool value) { return value ? "true" : "false"; }
        public static string Int(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        public static string Long(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in digest)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }
                return builder.ToString();
            }
        }
    }
}
