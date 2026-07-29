using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;
using TalismanBag.Items.Resource;

namespace TalismanBag.BattleBridge.NianResource
{
    public sealed class BattleSandboxNianResourceEngine
    {
        public const long PulseIntervalTicks = 1500L;
        public const int NominalPulseCount = 50;
        public const long NominalBattleDurationTicks = 75000L;

        private I031NianSourceSnapshot boundSource;
        private BattleSandboxNianResourceSnapshot current;

        private BattleSandboxNianResourceEngine(I031NianSourceSnapshot source)
        {
            boundSource = source;
            current = EmptySnapshot(
                source.sourceGeneration,
                0,
                source.canonicalSignature);
        }

        public BattleSandboxNianResourceSnapshot Current => current;

        public I031NianCostRequestFact NextCostRequest
        {
            get
            {
                IReadOnlyList<I031NianCostRequestFact> facts = boundSource.CostRequestFacts;
                return facts[current.nextCostOrdinal % facts.Count];
            }
        }

        public static BattleSandboxNianResourceEngine Create(
            I031NianSourceSnapshot source)
        {
            RequireEligibleSource(source);
            return new BattleSandboxNianResourceEngine(source);
        }

        public BattleSandboxNianPulseResult ApplyPulse(
            I031NianSourceSnapshot source,
            BattleSandboxNianPulseRequest request)
        {
            string preflightError = ValidatePulse(source, request);
            if (!string.IsNullOrEmpty(preflightError))
            {
                return RejectWithoutMutation(preflightError, request, NextCostRequest);
            }

            I031NianCostRequestFact cost = NextCostRequest;
            List<BattleSandboxNianLedgerEntry> ledger = current.Ledger.ToList();
            List<string> processed = current.ProcessedPulseEventIds.ToList();

            int beforeGeneration = current.currentNian;
            int afterGeneration = Math.Min(
                BattleSandboxNianResourceSnapshot.MaxNian,
                beforeGeneration + source.generationPerPulse);
            int generated = afterGeneration - beforeGeneration;
            ledger.Add(new BattleSandboxNianLedgerEntry(
                ledger.Count + 1,
                request.battleTick,
                current.resetGeneration,
                BattleSandboxNianLedgerKind.Generation,
                request.pulseEventId,
                cost.requestFactId,
                cost.itemId,
                generated,
                beforeGeneration,
                afterGeneration,
                generated == source.generationPerPulse ? "GENERATED" : "GENERATED_CAPPED"));

            bool enough = afterGeneration >= cost.nianCost;
            int afterSpend = enough ? afterGeneration - cost.nianCost : afterGeneration;
            string reason = enough ? "SPEND_ACCEPTED" : "INSUFFICIENT_NIAN";
            ledger.Add(new BattleSandboxNianLedgerEntry(
                ledger.Count + 1,
                request.battleTick,
                current.resetGeneration,
                enough
                    ? BattleSandboxNianLedgerKind.SpendAccepted
                    : BattleSandboxNianLedgerKind.SpendRejected,
                request.pulseEventId,
                cost.requestFactId,
                cost.itemId,
                enough ? cost.nianCost : 0,
                afterGeneration,
                afterSpend,
                reason));
            processed.Add(request.pulseEventId);

            current = new BattleSandboxNianResourceSnapshot(
                afterSpend,
                current.resetGeneration,
                current.revision + 1,
                current.pulseCount + 1,
                current.totalGenerated + generated,
                current.totalSpent + (enough ? cost.nianCost : 0),
                current.acceptedApplicationCount + (enough ? 1 : 0),
                current.rejectedApplicationCount + (enough ? 0 : 1),
                (current.nextCostOrdinal + 1) % boundSource.CostRequestFacts.Count,
                boundSource.canonicalSignature,
                ledger,
                processed);

            BattleSandboxNianResourceApplication application =
                new BattleSandboxNianResourceApplication(
                    enough
                        ? BattleSandboxNianDecision.Accepted
                        : BattleSandboxNianDecision.Rejected,
                    reason,
                    request.pulseEventId,
                    cost.requestFactId,
                    cost.itemId,
                    cost.nianCost,
                    afterSpend,
                    current.resetGeneration,
                    request.battleTick);
            return new BattleSandboxNianPulseResult(
                enough
                    ? BattleSandboxNianDecision.Accepted
                    : BattleSandboxNianDecision.Rejected,
                reason,
                current,
                application);
        }

        public BattleSandboxNianResourceSnapshot Reset(I031NianSourceSnapshot freshSource)
        {
            RequireEligibleSource(freshSource);
            if (freshSource.sourceGeneration != current.resetGeneration + 1)
            {
                throw new InvalidOperationException("RESET_GENERATION_MUST_INCREMENT_BY_ONE");
            }

            int nextRevision = current.revision + 1;
            boundSource = freshSource;
            current = EmptySnapshot(
                freshSource.sourceGeneration,
                nextRevision,
                freshSource.canonicalSignature);
            return current;
        }

        public static BattleSandboxNianResourceApplication EvaluateSpend(
            int availableNian,
            I031NianCostRequestFact cost,
            string pulseEventId = "OFFLINE_SPEND_PROBE")
        {
            if (cost == null)
            {
                throw new ArgumentNullException(nameof(cost));
            }

            bool enough = availableNian >= cost.nianCost;
            return new BattleSandboxNianResourceApplication(
                enough
                    ? BattleSandboxNianDecision.Accepted
                    : BattleSandboxNianDecision.Rejected,
                enough ? "SPEND_ACCEPTED" : "INSUFFICIENT_NIAN",
                pulseEventId,
                cost.requestFactId,
                cost.itemId,
                cost.nianCost,
                enough ? availableNian - cost.nianCost : availableNian,
                0,
                0L);
        }

        private string ValidatePulse(
            I031NianSourceSnapshot source,
            BattleSandboxNianPulseRequest request)
        {
            if (source == null)
            {
                return "SOURCE_MISSING";
            }

            if (source.status != I031NianSourceStatus.Valid || !source.isEligible)
            {
                return "SOURCE_NOT_ELIGIBLE";
            }

            if (source.sourceGeneration != current.resetGeneration ||
                request?.expectedResetGeneration != current.resetGeneration)
            {
                return "GENERATION_MISMATCH";
            }

            if (!string.Equals(
                    source.itemSystemSchemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal))
            {
                return "SOURCE_ITEM_SYSTEM_SCHEMA_NOT_V2";
            }

            if (!source.isOwned || source.location != I031Location.Board ||
                !source.isLightingSource ||
                !string.Equals(source.itemId, I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal) ||
                !string.Equals(source.specialIdentityId,
                    I031InventoryPlacementContract.SpecialIdentityId,
                    StringComparison.Ordinal) ||
                !string.Equals(source.stablePlacementId,
                    I031InventoryPlacementContract.StablePlacementId,
                    StringComparison.Ordinal))
            {
                return "SOURCE_PLACEMENT_SIGNATURE_INVALID";
            }

            if (!string.Equals(
                    source.canonicalSignature,
                    current.sourceCanonicalSignature,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    request?.expectedSourceCanonicalSignature,
                    current.sourceCanonicalSignature,
                    StringComparison.Ordinal))
            {
                return "SOURCE_SIGNATURE_MISMATCH";
            }

            if (request == null || string.IsNullOrWhiteSpace(request.pulseEventId))
            {
                return "PULSE_EVENT_ID_MISSING";
            }

            if (current.ProcessedPulseEventIds.Contains(
                    request.pulseEventId,
                    StringComparer.Ordinal))
            {
                return "DUPLICATE_PULSE_EVENT";
            }

            if (request.battleTick != (current.pulseCount + 1L) * PulseIntervalTicks)
            {
                return "PULSE_TICK_OUT_OF_SEQUENCE";
            }

            if (!string.Equals(
                    request.expectedCostRequestFactId,
                    NextCostRequest.requestFactId,
                    StringComparison.Ordinal))
            {
                return "COST_REQUEST_FACT_MISMATCH";
            }

            return string.Empty;
        }

        private BattleSandboxNianPulseResult RejectWithoutMutation(
            string reason,
            BattleSandboxNianPulseRequest request,
            I031NianCostRequestFact cost)
        {
            BattleSandboxNianResourceApplication application =
                new BattleSandboxNianResourceApplication(
                    BattleSandboxNianDecision.Rejected,
                    reason,
                    request?.pulseEventId ?? string.Empty,
                    cost?.requestFactId ?? string.Empty,
                    cost?.itemId ?? string.Empty,
                    cost?.nianCost ?? 0,
                    current.currentNian,
                    current.resetGeneration,
                    request?.battleTick ?? 0L);
            return new BattleSandboxNianPulseResult(
                BattleSandboxNianDecision.Rejected,
                reason,
                current,
                application);
        }

        private static BattleSandboxNianResourceSnapshot EmptySnapshot(
            int resetGeneration,
            int revision,
            string sourceCanonicalSignature)
        {
            return new BattleSandboxNianResourceSnapshot(
                BattleSandboxNianResourceSnapshot.InitialNian,
                resetGeneration,
                revision,
                0,
                0,
                0,
                0,
                0,
                0,
                sourceCanonicalSignature,
                Array.Empty<BattleSandboxNianLedgerEntry>(),
                Array.Empty<string>());
        }

        private static void RequireEligibleSource(I031NianSourceSnapshot source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.status != I031NianSourceStatus.Valid ||
                !source.isEligible ||
                source.CostRequestFacts.Count != 6)
            {
                throw new InvalidOperationException("I031_NIAN_SOURCE_NOT_ELIGIBLE");
            }
        }
    }
}
