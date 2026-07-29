using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Resource;

namespace TalismanBag.Contracts.Battle
{
    public enum BattleSandboxNianDecision
    {
        Rejected = 0,
        Accepted = 1
    }

    public enum BattleSandboxNianLedgerKind
    {
        Generation = 0,
        SpendAccepted = 1,
        SpendRejected = 2
    }

    public sealed class BattleSandboxNianPulseRequest
    {
        public BattleSandboxNianPulseRequest(
            string pulseEventId,
            int expectedResetGeneration,
            string expectedSourceCanonicalSignature,
            string expectedCostRequestFactId,
            long battleTick)
        {
            this.pulseEventId = pulseEventId ?? string.Empty;
            this.expectedResetGeneration = expectedResetGeneration;
            this.expectedSourceCanonicalSignature = expectedSourceCanonicalSignature ?? string.Empty;
            this.expectedCostRequestFactId = expectedCostRequestFactId ?? string.Empty;
            this.battleTick = battleTick;
        }

        public string pulseEventId { get; }
        public int expectedResetGeneration { get; }
        public string expectedSourceCanonicalSignature { get; }
        public string expectedCostRequestFactId { get; }
        public long battleTick { get; }
    }

    public sealed class BattleSandboxNianResourceApplication
    {
        public BattleSandboxNianResourceApplication(
            BattleSandboxNianDecision decision,
            string reasonCode,
            string pulseEventId,
            string costRequestFactId,
            string itemId,
            int nianCost,
            int balanceAfter,
            int resetGeneration,
            long battleTick)
        {
            this.decision = decision;
            this.reasonCode = reasonCode ?? string.Empty;
            this.pulseEventId = pulseEventId ?? string.Empty;
            this.costRequestFactId = costRequestFactId ?? string.Empty;
            this.itemId = itemId ?? string.Empty;
            this.nianCost = nianCost;
            this.balanceAfter = balanceAfter;
            this.resetGeneration = resetGeneration;
            this.battleTick = battleTick;
            applicationId = decision == BattleSandboxNianDecision.Accepted
                ? "NIAN_APPLICATION|" + resetGeneration.ToString(CultureInfo.InvariantCulture) +
                  "|" + this.pulseEventId + "|" + this.costRequestFactId
                : string.Empty;
        }

        public BattleSandboxNianDecision decision { get; }
        public bool accepted => decision == BattleSandboxNianDecision.Accepted;
        public string reasonCode { get; }
        public string applicationId { get; }
        public string pulseEventId { get; }
        public string costRequestFactId { get; }
        public string itemId { get; }
        public int nianCost { get; }
        public int balanceAfter { get; }
        public int resetGeneration { get; }
        public long battleTick { get; }
    }

    public sealed class BattleSandboxNianLedgerEntry
    {
        public BattleSandboxNianLedgerEntry(
            int sequence,
            long battleTick,
            int resetGeneration,
            BattleSandboxNianLedgerKind kind,
            string pulseEventId,
            string costRequestFactId,
            string itemId,
            int amount,
            int balanceBefore,
            int balanceAfter,
            string reasonCode)
        {
            this.sequence = sequence;
            this.battleTick = battleTick;
            this.resetGeneration = resetGeneration;
            this.kind = kind;
            this.pulseEventId = pulseEventId ?? string.Empty;
            this.costRequestFactId = costRequestFactId ?? string.Empty;
            this.itemId = itemId ?? string.Empty;
            this.amount = amount;
            this.balanceBefore = balanceBefore;
            this.balanceAfter = balanceAfter;
            this.reasonCode = reasonCode ?? string.Empty;
        }

        public int sequence { get; }
        public long battleTick { get; }
        public int resetGeneration { get; }
        public BattleSandboxNianLedgerKind kind { get; }
        public string pulseEventId { get; }
        public string costRequestFactId { get; }
        public string itemId { get; }
        public int amount { get; }
        public int balanceBefore { get; }
        public int balanceAfter { get; }
        public string reasonCode { get; }
    }

    public sealed class BattleSandboxNianResourceSnapshot
    {
        public const string SchemaVersion = "BattleSandboxNianResourceSnapshot.v1";
        public const string ResourceKey = "nian";
        public const int MaxNian = 100;
        public const int InitialNian = 20;

        private readonly ReadOnlyCollection<BattleSandboxNianLedgerEntry> ledger;
        private readonly ReadOnlyCollection<string> processedPulseEventIds;

        public BattleSandboxNianResourceSnapshot(
            int currentNian,
            int resetGeneration,
            int revision,
            int pulseCount,
            int totalGenerated,
            int totalSpent,
            int acceptedApplicationCount,
            int rejectedApplicationCount,
            int nextCostOrdinal,
            string sourceCanonicalSignature,
            IReadOnlyList<BattleSandboxNianLedgerEntry> ledger,
            IReadOnlyList<string> processedPulseEventIds)
        {
            schemaVersion = SchemaVersion;
            resourceKey = ResourceKey;
            maxNian = MaxNian;
            initialNian = InitialNian;
            this.currentNian = currentNian;
            this.resetGeneration = resetGeneration;
            this.revision = revision;
            this.pulseCount = pulseCount;
            this.totalGenerated = totalGenerated;
            this.totalSpent = totalSpent;
            this.acceptedApplicationCount = acceptedApplicationCount;
            this.rejectedApplicationCount = rejectedApplicationCount;
            this.nextCostOrdinal = nextCostOrdinal;
            this.sourceCanonicalSignature = sourceCanonicalSignature ?? string.Empty;
            this.ledger = new ReadOnlyCollection<BattleSandboxNianLedgerEntry>(
                (ledger ?? Array.Empty<BattleSandboxNianLedgerEntry>())
                .Where(value => value != null)
                .Select(value => new BattleSandboxNianLedgerEntry(
                    value.sequence,
                    value.battleTick,
                    value.resetGeneration,
                    value.kind,
                    value.pulseEventId,
                    value.costRequestFactId,
                    value.itemId,
                    value.amount,
                    value.balanceBefore,
                    value.balanceAfter,
                    value.reasonCode))
                .ToList());
            this.processedPulseEventIds = new ReadOnlyCollection<string>(
                (processedPulseEventIds ?? Array.Empty<string>()).ToList());
            canonicalSignature = NianCanonical.Sha256(BuildCanonicalText());
        }

        public string schemaVersion { get; }
        public bool devOnly => true;
        public bool formalRuntimeAuthority => false;
        public string resourceKey { get; }
        public int maxNian { get; }
        public int initialNian { get; }
        public int currentNian { get; }
        public int resetGeneration { get; }
        public int revision { get; }
        public int pulseCount { get; }
        public int totalGenerated { get; }
        public int totalSpent { get; }
        public int acceptedApplicationCount { get; }
        public int rejectedApplicationCount { get; }
        public int nextCostOrdinal { get; }
        public string sourceCanonicalSignature { get; }
        public IReadOnlyList<BattleSandboxNianLedgerEntry> Ledger => ledger;
        public IReadOnlyList<string> ProcessedPulseEventIds => processedPulseEventIds;
        public string canonicalSignature { get; }

        private string BuildCanonicalText()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(schemaVersion).Append('|')
                .Append(resourceKey).Append('|')
                .Append(maxNian.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(initialNian.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(currentNian.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(resetGeneration.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(revision.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(pulseCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(totalGenerated.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(totalSpent.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(acceptedApplicationCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(rejectedApplicationCount.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(nextCostOrdinal.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(sourceCanonicalSignature);
            foreach (BattleSandboxNianLedgerEntry entry in ledger)
            {
                builder.Append("|L:")
                    .Append(entry.sequence.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.battleTick.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.resetGeneration.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append((int)entry.kind).Append(':')
                    .Append(entry.pulseEventId).Append(':')
                    .Append(entry.costRequestFactId).Append(':')
                    .Append(entry.itemId).Append(':')
                    .Append(entry.amount.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.balanceBefore.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.balanceAfter.ToString(CultureInfo.InvariantCulture)).Append(':')
                    .Append(entry.reasonCode);
            }

            foreach (string eventId in processedPulseEventIds)
            {
                builder.Append("|P:").Append(eventId);
            }

            return builder.ToString();
        }
    }

    public sealed class BattleSandboxNianPulseResult
    {
        public BattleSandboxNianPulseResult(
            BattleSandboxNianDecision decision,
            string reasonCode,
            BattleSandboxNianResourceSnapshot snapshot,
            BattleSandboxNianResourceApplication resourceApplication)
        {
            this.decision = decision;
            this.reasonCode = reasonCode ?? string.Empty;
            this.snapshot = snapshot;
            this.resourceApplication = resourceApplication;
        }

        public BattleSandboxNianDecision decision { get; }
        public string reasonCode { get; }
        public BattleSandboxNianResourceSnapshot snapshot { get; }
        public BattleSandboxNianResourceApplication resourceApplication { get; }
        public bool accepted => resourceApplication?.accepted == true;
    }
}
