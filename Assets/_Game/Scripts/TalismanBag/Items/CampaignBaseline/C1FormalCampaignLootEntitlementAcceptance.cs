using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;

namespace TalismanBag.Items.CampaignBaseline
{
    public static class C1FormalCampaignLootEntitlementAcceptanceContract
    {
        public const string CommandSchemaId = "C1FormalCampaignLootEntitlementCommand.v2";
        public const string ReceiptSchemaId = "C1FormalCampaignLootAcceptedEntitlementReceipt.v2";
        public const string HistorySchemaId = "C1FormalCampaignLootEntitlementHistorySnapshot.v2";
        public const string AcceptanceAlgorithmId = "canonical-generated-item-acceptance-v1";
        public const string SourceKind = "CANONICAL_CAMPAIGN_DIRECT_LOOT";
    }

    public static class C1FormalCampaignLootEntitlementDiagnosticCodes
    {
        public const string GrantRequired = "CAMPAIGN_LOOT_GRANT_REQUIRED";
        public const string GrantContractInvalid = "CAMPAIGN_LOOT_GRANT_CONTRACT_INVALID";
        public const string MaterializationMismatch = "CAMPAIGN_LOOT_GENERATED_ITEM_MISMATCH";
        public const string EntitlementConflict = "CAMPAIGN_LOOT_ENTITLEMENT_CONFLICT";
        public const string ItemInstanceConflict = "CAMPAIGN_LOOT_ITEM_INSTANCE_CONFLICT";
        public const string ClaimConflict = "CAMPAIGN_LOOT_CLAIM_CONFLICT";
        public const string DedupeConflict = "CAMPAIGN_LOOT_DEDUPE_CONFLICT";
        public const string GrantConflict = "CAMPAIGN_LOOT_GRANT_CONFLICT";
        public const string StageClearConflict = "CAMPAIGN_LOOT_STAGE_CLEAR_CONFLICT";
    }

    public sealed class C1FormalCampaignLootEntitlementCommand
    {
        public C1FormalCampaignLootEntitlementCommand(
            string sessionToken, long resetGeneration, string commandId,
            string expectedPriorSessionCanonicalSignature,
            C1CampaignDirectLootGrantBundle releasedGrantBundle)
        {
            schemaId = C1FormalCampaignLootEntitlementAcceptanceContract.CommandSchemaId;
            this.sessionToken = C1FormalItemCanonical.Normalize(sessionToken);
            this.resetGeneration = resetGeneration;
            this.commandId = C1FormalItemCanonical.Normalize(commandId);
            this.expectedPriorSessionCanonicalSignature =
                C1FormalItemCanonical.Normalize(expectedPriorSessionCanonicalSignature);
            this.releasedGrantBundle = releasedGrantBundle;
            canonicalPayload = string.Join("|", schemaId, this.sessionToken,
                resetGeneration.ToString(CultureInfo.InvariantCulture), this.commandId,
                this.expectedPriorSessionCanonicalSignature,
                releasedGrantBundle?.canonicalSignature ?? string.Empty);
            canonicalSignature = canonicalPayload;
        }
        public string schemaId { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string commandId { get; }
        public string expectedPriorSessionCanonicalSignature { get; }
        public C1CampaignDirectLootGrantBundle releasedGrantBundle { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1FormalCampaignLootAcceptedEntitlementReceipt
    {
        internal C1FormalCampaignLootAcceptedEntitlementReceipt(
            string acceptedCommandId, C1CampaignDirectLootGrantBundle grant)
        {
            ItemGeneratedInstanceSnapshot item = grant?.generatedItem
                ?? throw new ArgumentNullException(nameof(grant));
            schemaId = C1FormalCampaignLootEntitlementAcceptanceContract.ReceiptSchemaId;
            this.acceptedCommandId = C1FormalItemCanonical.Normalize(acceptedCommandId);
            acceptedDropOrdinal = grant.acceptedDropOrdinal;
            authoritativeRewardGrantIdentity = grant.authoritativeRewardGrantIdentity;
            grantCanonicalSignature = grant.canonicalSignature;
            selectedBaseItemId = grant.selectedBaseItemId;
            itemInstanceId = item.itemInstanceId;
            instanceCanonicalSignature = item.BuildCanonicalSignature();
            entitlementIdentity = grant.authoritativeRewardGrantIdentity;
            entitlementCanonicalSignature = grant.canonicalSignature;
            claimId = grant.claimIdentity.claimId;
            claimCanonicalSignature = grant.claimIdentity.canonicalSignature;
            dedupeKey = grant.dedupeIdentity.dedupeKey;
            dedupeCanonicalSignature = grant.dedupeIdentity.canonicalSignature;
            stageClearId = grant.stageClearFact.stageClearId;
            stageClearCanonicalSignature = grant.stageClearFact.canonicalSignature;
            dropRequestId = grant.dropRequest.dropRequestId;
            dropRequestCanonicalSignature = grant.dropRequest.canonicalSignature;
            dropRollResultId = grant.dropRollResult.dropRollResultId;
            dropRollResultCanonicalSignature = grant.dropRollResult.canonicalSignature;
            rewardResultId = grant.rewardResult.rewardResultId;
            rewardResultCanonicalSignature = grant.rewardResult.canonicalSignature;
            poolId = grant.dropRequest.poolId;
            rarityVersionIdentity = item.baseItemId + "@" + item.rarity.ToStableKey();
            profileCanonicalSignature = rarityVersionIdentity;
            poolCanonicalSignature = grant.dropRequest.poolVersion;
            itemCatalogCanonicalSignature = C1CampaignDirectLootPoolAndPolicy.CanonicalPoolId;
            materializationLineageCanonicalSignature = instanceCanonicalSignature;
            canonicalPayload = string.Join("|", schemaId, grantCanonicalSignature,
                itemInstanceId, instanceCanonicalSignature, stageClearId);
            canonicalSignature = canonicalPayload;
        }

        private C1FormalCampaignLootAcceptedEntitlementReceipt(
            C1FormalCampaignLootAcceptedEntitlementReceipt source)
        {
            schemaId = source.schemaId;
            acceptedCommandId = source.acceptedCommandId;
            acceptedDropOrdinal = source.acceptedDropOrdinal;
            authoritativeRewardGrantIdentity = source.authoritativeRewardGrantIdentity;
            grantCanonicalSignature = source.grantCanonicalSignature;
            selectedBaseItemId = source.selectedBaseItemId;
            itemInstanceId = source.itemInstanceId;
            instanceCanonicalSignature = source.instanceCanonicalSignature;
            entitlementIdentity = source.entitlementIdentity;
            entitlementCanonicalSignature = source.entitlementCanonicalSignature;
            claimId = source.claimId;
            claimCanonicalSignature = source.claimCanonicalSignature;
            dedupeKey = source.dedupeKey;
            dedupeCanonicalSignature = source.dedupeCanonicalSignature;
            stageClearId = source.stageClearId;
            stageClearCanonicalSignature = source.stageClearCanonicalSignature;
            dropRequestId = source.dropRequestId;
            dropRequestCanonicalSignature = source.dropRequestCanonicalSignature;
            dropRollResultId = source.dropRollResultId;
            dropRollResultCanonicalSignature = source.dropRollResultCanonicalSignature;
            rewardResultId = source.rewardResultId;
            rewardResultCanonicalSignature = source.rewardResultCanonicalSignature;
            poolId = source.poolId;
            rarityVersionIdentity = source.rarityVersionIdentity;
            profileCanonicalSignature = source.profileCanonicalSignature;
            poolCanonicalSignature = source.poolCanonicalSignature;
            itemCatalogCanonicalSignature = source.itemCatalogCanonicalSignature;
            materializationLineageCanonicalSignature = source.materializationLineageCanonicalSignature;
            canonicalPayload = source.canonicalPayload;
            canonicalSignature = source.canonicalSignature;
        }

        public string schemaId { get; }
        public string acceptedCommandId { get; }
        public long acceptedDropOrdinal { get; }
        public string authoritativeRewardGrantIdentity { get; }
        public string grantCanonicalSignature { get; }
        public string selectedBaseItemId { get; }
        public string itemInstanceId { get; }
        public string instanceCanonicalSignature { get; }
        public string entitlementIdentity { get; }
        public string entitlementCanonicalSignature { get; }
        public string claimId { get; }
        public string claimCanonicalSignature { get; }
        public string dedupeKey { get; }
        public string dedupeCanonicalSignature { get; }
        public string stageClearId { get; }
        public string stageClearCanonicalSignature { get; }
        public string dropRequestId { get; }
        public string dropRequestCanonicalSignature { get; }
        public string dropRollResultId { get; }
        public string dropRollResultCanonicalSignature { get; }
        public string rewardResultId { get; }
        public string rewardResultCanonicalSignature { get; }
        public string poolId { get; }
        public string rarityVersionIdentity { get; }
        public string profileCanonicalSignature { get; }
        public string poolCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public string materializationLineageCanonicalSignature { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }
        internal C1FormalCampaignLootAcceptedEntitlementReceipt Clone() => new(this);
    }

    public sealed class C1FormalCampaignLootEntitlementHistorySnapshot
    {
        private readonly ReadOnlyCollection<C1FormalCampaignLootAcceptedEntitlementReceipt>
            acceptedReceipts;
        internal C1FormalCampaignLootEntitlementHistorySnapshot(
            IEnumerable<C1FormalCampaignLootAcceptedEntitlementReceipt> receipts)
        {
            schemaId = C1FormalCampaignLootEntitlementAcceptanceContract.HistorySchemaId;
            acceptedReceipts = Array.AsReadOnly((receipts
                ?? Enumerable.Empty<C1FormalCampaignLootAcceptedEntitlementReceipt>())
                .Where(value => value != null).Select(value => value.Clone()).ToArray());
            canonicalPayload = string.Join("|", schemaId,
                string.Join(",", acceptedReceipts.Select(value => value.canonicalSignature)));
            canonicalSignature = canonicalPayload;
        }
        public string schemaId { get; }
        public int acceptedCount => acceptedReceipts.Count;
        public IReadOnlyList<C1FormalCampaignLootAcceptedEntitlementReceipt> AcceptedReceipts => acceptedReceipts;
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }
        internal static C1FormalCampaignLootEntitlementHistorySnapshot Empty() =>
            new(Array.Empty<C1FormalCampaignLootAcceptedEntitlementReceipt>());
        internal C1FormalCampaignLootEntitlementHistorySnapshot Append(
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt) =>
            new(acceptedReceipts.Concat(new[] { receipt }));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByCanonicalSignature(string v) => Find(x => string.Equals(x.canonicalSignature, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByGrantCanonicalSignature(string v) => Find(x => string.Equals(x.grantCanonicalSignature, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByGrantIdentity(string v) => Find(x => string.Equals(x.authoritativeRewardGrantIdentity, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByEntitlementIdentity(string v) => Find(x => string.Equals(x.entitlementIdentity, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByEntitlementCanonicalSignature(string v) => Find(x => string.Equals(x.entitlementCanonicalSignature, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByItemInstanceId(string v) => Find(x => string.Equals(x.itemInstanceId, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByClaimId(string v) => Find(x => string.Equals(x.claimId, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByClaimCanonicalSignature(string v) => Find(x => string.Equals(x.claimCanonicalSignature, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByDedupeKey(string v) => Find(x => string.Equals(x.dedupeKey, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByDedupeCanonicalSignature(string v) => Find(x => string.Equals(x.dedupeCanonicalSignature, v, StringComparison.Ordinal));
        public C1FormalCampaignLootAcceptedEntitlementReceipt FindByStageClearId(string v) => Find(x => string.Equals(x.stageClearId, v, StringComparison.Ordinal));
        private C1FormalCampaignLootAcceptedEntitlementReceipt Find(
            Func<C1FormalCampaignLootAcceptedEntitlementReceipt, bool> predicate)
        {
            C1FormalCampaignLootAcceptedEntitlementReceipt value = acceptedReceipts.FirstOrDefault(predicate);
            return value?.Clone();
        }
    }

    public sealed class C1FormalCampaignLootEntitlementAcceptanceResult
    {
        private C1FormalCampaignLootEntitlementAcceptanceResult(
            bool accepted, bool changed, string diagnosticCode,
            string diagnosticMessage, C1FormalItemSessionSnapshot snapshot,
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt,
            C1FormalCampaignLootEntitlementHistorySnapshot history)
        {
            this.accepted = accepted;
            this.changed = changed;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
            this.snapshot = snapshot;
            acceptedEntitlementReceipt = receipt?.Clone();
            entitlementHistory = history;
        }
        public bool accepted { get; }
        public bool changed { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }
        public C1FormalItemSessionSnapshot snapshot { get; }
        public C1FormalCampaignLootAcceptedEntitlementReceipt acceptedEntitlementReceipt { get; }
        public C1FormalCampaignLootEntitlementHistorySnapshot entitlementHistory { get; }
        internal static C1FormalCampaignLootEntitlementAcceptanceResult Accepted(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt,
            C1FormalCampaignLootEntitlementHistorySnapshot history,
            bool changed, bool duplicate) => new(true, changed,
                duplicate ? C1FormalItemSessionDiagnosticCodes.DuplicateAcceptedNoOp
                    : C1FormalItemSessionDiagnosticCodes.None,
                string.Empty, snapshot, receipt, history);
        internal static C1FormalCampaignLootEntitlementAcceptanceResult Rejected(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalCampaignLootEntitlementHistorySnapshot history,
            string code, string message) =>
            new(false, false, code, message, snapshot, null, history);
    }

    internal static class C1FormalCampaignLootEntitlementValidator
    {
        internal static bool TryValidateReleasedGrant(
            C1FormalCampaignLootEntitlementCommand command,
            CanonicalItemDefinitionResolver resolver,
            out C1FormalItemOrdinaryInstanceSnapshot ordinaryInstance,
            out C1FormalCampaignLootAcceptedEntitlementReceipt receipt,
            out string code, out string message)
        {
            ordinaryInstance = null;
            receipt = null;
            C1CampaignDirectLootGrantBundle grant = command?.releasedGrantBundle;
            ItemGeneratedInstanceSnapshot item = grant?.generatedItem;
            CanonicalItemDefinition definition = grant?.itemDefinition;
            if (grant == null || item == null || definition == null)
                return Fail(C1FormalCampaignLootEntitlementDiagnosticCodes.GrantRequired,
                    "Canonical generated Item grant is required.", out code, out message);
            if (!grant.IsCanonicalValidV2())
                return Fail(C1FormalCampaignLootEntitlementDiagnosticCodes.GrantContractInvalid,
                    "Canonical generated Item grant is invalid.", out code, out message);
            if (resolver == null
                || !ReferenceEquals(resolver.GetDefinition(item.baseItemId), definition)
                || item.identity == null
                || !string.Equals(item.baseItemId, grant.selectedBaseItemId, StringComparison.Ordinal)
                || !string.Equals(command.sessionToken, grant.stageClearFact.runSessionId, StringComparison.Ordinal))
                return Fail(C1FormalCampaignLootEntitlementDiagnosticCodes.MaterializationMismatch,
                    "Generated Item identity does not match the canonical catalog or session.",
                    out code, out message);
            string instanceSignature = item.BuildCanonicalSignature();
            ordinaryInstance = new C1FormalItemOrdinaryInstanceSnapshot(
                item.identity,
                item.identity.BuildCanonicalSignature(),
                item.baseItemId + "@" + item.rarity.ToStableKey(),
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolId,
                C1FormalCampaignLootEntitlementAcceptanceContract.SourceKind,
                instanceSignature,
                item,
                definition);
            receipt = new C1FormalCampaignLootAcceptedEntitlementReceipt(command.commandId, grant);
            code = C1FormalItemSessionDiagnosticCodes.None;
            message = string.Empty;
            return true;
        }
        private static bool Fail(string diagnosticCode, string diagnosticMessage,
            out string code, out string message)
        {
            code = diagnosticCode;
            message = diagnosticMessage;
            return false;
        }
    }
}
