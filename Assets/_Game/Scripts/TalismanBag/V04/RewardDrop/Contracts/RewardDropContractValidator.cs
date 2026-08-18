using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public sealed class RewardDropValidationError
    {
        public string code { get; }
        public string field { get; }
        public string message { get; }

        public RewardDropValidationError(string code, string field, string message)
        {
            this.code = RewardDropCanonical.NormalizeIdentifier(code);
            this.field = RewardDropCanonical.NormalizeIdentifier(field);
            this.message = (message ?? string.Empty).Trim();
        }
    }

    public sealed class RewardDropValidationResult
    {
        public bool isValid { get; }
        public IReadOnlyList<RewardDropValidationError> errors { get; }

        internal RewardDropValidationResult(IEnumerable<RewardDropValidationError> errors)
        {
            RewardDropValidationError[] copy =
                (errors ?? Enumerable.Empty<RewardDropValidationError>())
                .Where(error => error != null)
                .OrderBy(error => error.code, StringComparer.Ordinal)
                .ThenBy(error => error.field, StringComparer.Ordinal)
                .ThenBy(error => error.message, StringComparer.Ordinal)
                .ToArray();
            ReadOnlyCollection<RewardDropValidationError> readOnly =
                new ReadOnlyCollection<RewardDropValidationError>(copy);
            this.errors = readOnly;
            isValid = copy.Length == 0;
        }
    }

    public static class RewardDropContractValidator
    {
        public const string SchemaId = "RewardDropContractValidator.v1";

        public static RewardDropValidationResult ValidateStageClearFact(StageClearFact fact)
        {
            ValidationCollector collector = new ValidationCollector();
            if (fact == null)
            {
                collector.Add("FACT_NULL", "stageClearFact", "StageClearFact is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                fact.schemaId,
                StageClearFact.SchemaId,
                "stageClearFact.schemaId");
            RequireIdentifier(collector, fact.stageClearId, "stageClearFact.stageClearId");
            RequireIdentifier(collector, fact.battleResultId, "stageClearFact.battleResultId");
            RequireIdentifier(collector, fact.battleRequestId, "stageClearFact.battleRequestId");
            RequireIdentifier(
                collector,
                fact.resultFingerprint,
                "stageClearFact.resultFingerprint");
            RequireIdentifier(collector, fact.chapterId, "stageClearFact.chapterId");
            RequireIdentifier(collector, fact.stageId, "stageClearFact.stageId");
            RequireIdentifier(collector, fact.runSessionId, "stageClearFact.runSessionId");
            if (fact.clearSequence <= 0)
            {
                collector.Add(
                    "CLEAR_SEQUENCE_INVALID",
                    "stageClearFact.clearSequence",
                    "clearSequence must be greater than zero.");
            }

            ValidateLaunchContext(
                collector,
                fact.rewardLaunchContextIdentity,
                false,
                "stageClearFact.rewardLaunchContextIdentity");
            ValidateCanonical(
                collector,
                fact.canonicalPayload,
                fact.canonicalSignature,
                "stageClearFact");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateDropRequest(
            DropRequest request,
            StageClearFact stageClearFact)
        {
            ValidationCollector collector = new ValidationCollector();
            if (request == null)
            {
                collector.Add("REQUEST_NULL", "dropRequest", "DropRequest is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                request.schemaId,
                DropRequest.SchemaId,
                "dropRequest.schemaId");
            RequireIdentifier(collector, request.dropRequestId, "dropRequest.dropRequestId");
            RequireIdentifier(collector, request.sourceFactId, "dropRequest.sourceFactId");
            RequireIdentifier(collector, request.chapterId, "dropRequest.chapterId");
            RequireIdentifier(collector, request.stageId, "dropRequest.stageId");
            RequireIdentifier(collector, request.poolId, "dropRequest.poolId");
            RequireIdentifier(collector, request.poolVersion, "dropRequest.poolVersion");
            RequireIdentifier(
                collector,
                request.rarityTierProfileId,
                "dropRequest.rarityTierProfileId");
            RequireIdentifier(
                collector,
                request.attributeRollBandProfileId,
                "dropRequest.attributeRollBandProfileId");
            RequireIdentifier(
                collector,
                request.rollPolicyVersion,
                "dropRequest.rollPolicyVersion");
            if (request.generationVersion <= 0)
            {
                collector.Add(
                    "GENERATION_VERSION_INVALID",
                    "dropRequest.generationVersion",
                    "generationVersion must be greater than zero.");
            }

            ValidateEnum(
                collector,
                request.sourceFactKind,
                RewardSourceFactKind.Unspecified,
                "SOURCE_FACT_KIND_INVALID",
                "dropRequest.sourceFactKind");
            ValidateEnum(
                collector,
                request.acquisitionMode,
                RewardAcquisitionMode.Unspecified,
                "ACQUISITION_MODE_INVALID",
                "dropRequest.acquisitionMode");
            ValidateLaunchContext(
                collector,
                request.rewardLaunchContextIdentity,
                true,
                "dropRequest.rewardLaunchContextIdentity");

            bool online = request.acquisitionMode == RewardAcquisitionMode.FirstClearOnline
                || request.acquisitionMode == RewardAcquisitionMode.RepeatChallengeOnline;
            bool offline = request.acquisitionMode == RewardAcquisitionMode.OfflinePatrol;
            if (online)
            {
                if (request.sourceFactKind != RewardSourceFactKind.StageClear)
                {
                    collector.Add(
                        "ONLINE_SOURCE_KIND_MISMATCH",
                        "dropRequest.sourceFactKind",
                        "Online acquisition requires StageClear source kind.");
                }

                RequireIdentifier(collector, request.stageClearId, "dropRequest.stageClearId");
                if (!RewardDropCanonical.EqualsOrdinal(
                        request.sourceFactId,
                        request.stageClearId))
                {
                    collector.Add(
                        "ONLINE_SOURCE_ID_MISMATCH",
                        "dropRequest.sourceFactId",
                        "Online sourceFactId must equal stageClearId.");
                }

                if (stageClearFact == null)
                {
                    collector.Add(
                        "STAGE_CLEAR_REQUIRED",
                        "stageClearFact",
                        "Online acquisition requires a supplied StageClearFact.");
                }
                else
                {
                    collector.Append(ValidateStageClearFact(stageClearFact), "stageClearFact.");
                    Match(
                        collector,
                        request.stageClearId,
                        stageClearFact.stageClearId,
                        "REQUEST_STAGE_CLEAR_MISMATCH",
                        "dropRequest.stageClearId");
                    Match(
                        collector,
                        request.chapterId,
                        stageClearFact.chapterId,
                        "REQUEST_CHAPTER_MISMATCH",
                        "dropRequest.chapterId");
                    Match(
                        collector,
                        request.stageId,
                        stageClearFact.stageId,
                        "REQUEST_STAGE_MISMATCH",
                        "dropRequest.stageId");
                    MatchContext(
                        collector,
                        request.rewardLaunchContextIdentity,
                        stageClearFact.rewardLaunchContextIdentity,
                        "REQUEST_CONTEXT_MISMATCH",
                        "dropRequest.rewardLaunchContextIdentity");
                }
            }
            else if (offline)
            {
                if (request.sourceFactKind != RewardSourceFactKind.OfflinePatrolSettlement)
                {
                    collector.Add(
                        "OFFLINE_SOURCE_KIND_MISMATCH",
                        "dropRequest.sourceFactKind",
                        "OfflinePatrol requires OfflinePatrolSettlement source kind.");
                }

                if (request.stageClearId.Length != 0)
                {
                    collector.Add(
                        "OFFLINE_STAGE_CLEAR_FORGERY",
                        "dropRequest.stageClearId",
                        "OfflinePatrol must not carry a StageClear identity.");
                }

                if (stageClearFact != null)
                {
                    collector.Add(
                        "OFFLINE_STAGE_CLEAR_SUPPLIED",
                        "stageClearFact",
                        "OfflinePatrol must not correlate to a StageClearFact.");
                }
            }

            HashSet<string> slotIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < request.rollSlots.Count; index++)
            {
                DropRollSlot slot = request.rollSlots[index];
                string prefix = "dropRequest.rollSlots[" + index + "]";
                if (slot == null)
                {
                    collector.Add("ROLL_SLOT_NULL", prefix, "Roll slot is required.");
                    continue;
                }

                RequireSchema(collector, slot.schemaId, DropRollSlot.SchemaId, prefix + ".schemaId");
                RequireIdentifier(collector, slot.rollSlotId, prefix + ".rollSlotId");
                if (slot.rollSlotId.Length > 0 && !slotIds.Add(slot.rollSlotId))
                {
                    collector.Add(
                        "DUPLICATE_ROLL_SLOT",
                        prefix + ".rollSlotId",
                        "rollSlotId must be unique.");
                }

                if (slot.grantKind != RewardGrantKind.OrdinaryRoll)
                {
                    collector.Add(
                        "ROLL_SLOT_GRANT_KIND_INVALID",
                        prefix + ".grantKind",
                        "P0A roll slots accept only OrdinaryRoll.");
                }

                ValidateCanonical(
                    collector,
                    slot.canonicalPayload,
                    slot.canonicalSignature,
                    prefix);
            }

            ValidateCanonical(
                collector,
                request.canonicalPayload,
                request.canonicalSignature,
                "dropRequest");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateDropRollResult(
            DropRollResult result,
            DropRequest request,
            StageClearFact stageClearFact)
        {
            ValidationCollector collector = new ValidationCollector();
            if (result == null)
            {
                collector.Add("ROLL_RESULT_NULL", "dropRollResult", "DropRollResult is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                result.schemaId,
                DropRollResult.SchemaId,
                "dropRollResult.schemaId");
            RequireIdentifier(
                collector,
                result.dropRollResultId,
                "dropRollResult.dropRollResultId");
            RequireIdentifier(collector, result.dropRequestId, "dropRollResult.dropRequestId");
            RequireIdentifier(collector, result.sourceFactId, "dropRollResult.sourceFactId");
            RequireIdentifier(collector, result.chapterId, "dropRollResult.chapterId");
            RequireIdentifier(collector, result.stageId, "dropRollResult.stageId");
            RequireIdentifier(collector, result.algorithmId, "dropRollResult.algorithmId");
            RequireIdentifier(collector, result.poolId, "dropRollResult.poolId");
            RequireIdentifier(collector, result.poolVersion, "dropRollResult.poolVersion");
            RequireIdentifier(
                collector,
                result.rarityTierProfileId,
                "dropRollResult.rarityTierProfileId");
            RequireIdentifier(
                collector,
                result.attributeRollBandProfileId,
                "dropRollResult.attributeRollBandProfileId");
            RequireIdentifier(
                collector,
                result.rollPolicyVersion,
                "dropRollResult.rollPolicyVersion");
            if (result.generationVersion <= 0)
            {
                collector.Add(
                    "GENERATION_VERSION_INVALID",
                    "dropRollResult.generationVersion",
                    "generationVersion must be greater than zero.");
            }

            ValidateEnum(
                collector,
                result.acquisitionMode,
                RewardAcquisitionMode.Unspecified,
                "ACQUISITION_MODE_INVALID",
                "dropRollResult.acquisitionMode");
            ValidateLaunchContext(
                collector,
                result.rewardLaunchContextIdentity,
                true,
                "dropRollResult.rewardLaunchContextIdentity");

            if (request == null)
            {
                collector.Add(
                    "REQUEST_REQUIRED_FOR_RESULT",
                    "dropRequest",
                    "DropRollResult validation requires its DropRequest.");
            }
            else
            {
                collector.Append(
                    ValidateDropRequest(request, stageClearFact),
                    "dropRequest.");
                Match(
                    collector,
                    result.dropRequestId,
                    request.dropRequestId,
                    "RESULT_REQUEST_MISMATCH",
                    "dropRollResult.dropRequestId");
                Match(
                    collector,
                    result.sourceFactId,
                    request.sourceFactId,
                    "RESULT_SOURCE_MISMATCH",
                    "dropRollResult.sourceFactId");
                Match(
                    collector,
                    result.stageClearId,
                    request.stageClearId,
                    "RESULT_STAGE_CLEAR_MISMATCH",
                    "dropRollResult.stageClearId");
                Match(
                    collector,
                    result.chapterId,
                    request.chapterId,
                    "RESULT_CHAPTER_MISMATCH",
                    "dropRollResult.chapterId");
                Match(
                    collector,
                    result.stageId,
                    request.stageId,
                    "RESULT_STAGE_MISMATCH",
                    "dropRollResult.stageId");
                Match(
                    collector,
                    result.acquisitionMode,
                    request.acquisitionMode,
                    "RESULT_MODE_MISMATCH",
                    "dropRollResult.acquisitionMode");
                Match(
                    collector,
                    result.rootSeed,
                    request.rootSeed,
                    "RESULT_SEED_MISMATCH",
                    "dropRollResult.rootSeed");
                Match(
                    collector,
                    result.generationVersion,
                    request.generationVersion,
                    "RESULT_GENERATION_MISMATCH",
                    "dropRollResult.generationVersion");
                Match(
                    collector,
                    result.poolId,
                    request.poolId,
                    "RESULT_POOL_MISMATCH",
                    "dropRollResult.poolId");
                Match(
                    collector,
                    result.poolVersion,
                    request.poolVersion,
                    "RESULT_POOL_VERSION_MISMATCH",
                    "dropRollResult.poolVersion");
                Match(
                    collector,
                    result.rarityTierProfileId,
                    request.rarityTierProfileId,
                    "RESULT_TIER_PROFILE_MISMATCH",
                    "dropRollResult.rarityTierProfileId");
                Match(
                    collector,
                    result.attributeRollBandProfileId,
                    request.attributeRollBandProfileId,
                    "RESULT_ATTRIBUTE_BAND_MISMATCH",
                    "dropRollResult.attributeRollBandProfileId");
                Match(
                    collector,
                    result.rollPolicyVersion,
                    request.rollPolicyVersion,
                    "RESULT_ROLL_POLICY_MISMATCH",
                    "dropRollResult.rollPolicyVersion");
                MatchContext(
                    collector,
                    result.rewardLaunchContextIdentity,
                    request.rewardLaunchContextIdentity,
                    "RESULT_CONTEXT_MISMATCH",
                    "dropRollResult.rewardLaunchContextIdentity");
            }

            Dictionary<string, DropRollSlot> slots =
                request == null
                    ? new Dictionary<string, DropRollSlot>(StringComparer.Ordinal)
                    : request.rollSlots
                        .Where(slot => slot != null && slot.rollSlotId.Length > 0)
                        .GroupBy(slot => slot.rollSlotId, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            HashSet<string> entryIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> consumedSlotIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> itemInstanceIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < result.entries.Count; index++)
            {
                DropRollEntry entry = result.entries[index];
                string prefix = "dropRollResult.entries[" + index + "]";
                if (entry == null)
                {
                    collector.Add("ROLL_ENTRY_NULL", prefix, "Roll entry is required.");
                    continue;
                }

                RequireSchema(
                    collector,
                    entry.schemaId,
                    DropRollEntry.SchemaId,
                    prefix + ".schemaId");
                RequireIdentifier(collector, entry.rollEntryId, prefix + ".rollEntryId");
                RequireIdentifier(collector, entry.rollSlotId, prefix + ".rollSlotId");
                if (entry.rollEntryId.Length > 0 && !entryIds.Add(entry.rollEntryId))
                {
                    collector.Add(
                        "DUPLICATE_ROLL_ENTRY",
                        prefix + ".rollEntryId",
                        "rollEntryId must be unique.");
                }

                if (entry.rollSlotId.Length > 0 && !consumedSlotIds.Add(entry.rollSlotId))
                {
                    collector.Add(
                        "REUSED_ROLL_SLOT",
                        prefix + ".rollSlotId",
                        "Each roll slot may be consumed exactly once.");
                }

                DropRollSlot slot;
                if (!slots.TryGetValue(entry.rollSlotId, out slot))
                {
                    collector.Add(
                        "ROLL_SLOT_NOT_FOUND",
                        prefix + ".rollSlotId",
                        "Result entry must reference one request roll slot.");
                }
                else if (entry.grantKind != slot.grantKind)
                {
                    collector.Add(
                        "ROLL_ENTRY_GRANT_MISMATCH",
                        prefix + ".grantKind",
                        "Result grant kind must match its request roll slot.");
                }

                if (entry.grantKind != RewardGrantKind.OrdinaryRoll)
                {
                    collector.Add(
                        "ROLL_ENTRY_GRANT_KIND_INVALID",
                        prefix + ".grantKind",
                        "P0A DropRollResult accepts only OrdinaryRoll entries.");
                }

                ValidateSubject(collector, entry.rewardSubjectIdentity, prefix);
                ValidateOrdinarySubject(collector, entry.rewardSubjectIdentity, prefix);
                ValidateInstanceFields(
                    collector,
                    entry.rewardSubjectIdentity,
                    entry.itemInstanceId,
                    entry.instanceCanonicalSignature,
                    prefix);
                if (entry.itemInstanceId.Length > 0
                    && !itemInstanceIds.Add(entry.itemInstanceId))
                {
                    collector.Add(
                        "DUPLICATE_ITEM_INSTANCE",
                        prefix + ".itemInstanceId",
                        "Item instance identity must be unique.");
                }

                if (entry.quantity <= 0)
                {
                    collector.Add(
                        "QUANTITY_INVALID",
                        prefix + ".quantity",
                        "quantity must be greater than zero.");
                }

                ValidateCanonical(
                    collector,
                    entry.canonicalPayload,
                    entry.canonicalSignature,
                    prefix);
            }

            if (request != null && consumedSlotIds.Count != slots.Count)
            {
                collector.Add(
                    "ROLL_SLOT_CONSUMPTION_INCOMPLETE",
                    "dropRollResult.entries",
                    "Every request roll slot must be consumed exactly once.");
            }

            ValidateCanonical(
                collector,
                result.canonicalPayload,
                result.canonicalSignature,
                "dropRollResult");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateRewardResult(
            RewardResult rewardResult,
            DropRequest request,
            DropRollResult dropRollResult,
            StageClearFact stageClearFact)
        {
            ValidationCollector collector = new ValidationCollector();
            if (rewardResult == null)
            {
                collector.Add("REWARD_RESULT_NULL", "rewardResult", "RewardResult is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                rewardResult.schemaId,
                RewardResult.SchemaId,
                "rewardResult.schemaId");
            RequireIdentifier(
                collector,
                rewardResult.rewardResultId,
                "rewardResult.rewardResultId");
            RequireIdentifier(collector, rewardResult.sourceFactId, "rewardResult.sourceFactId");
            RequireIdentifier(collector, rewardResult.dropRequestId, "rewardResult.dropRequestId");
            RequireIdentifier(
                collector,
                rewardResult.dropRollResultId,
                "rewardResult.dropRollResultId");
            RequireIdentifier(collector, rewardResult.chapterId, "rewardResult.chapterId");
            RequireIdentifier(collector, rewardResult.stageId, "rewardResult.stageId");
            ValidateEnum(
                collector,
                rewardResult.acquisitionMode,
                RewardAcquisitionMode.Unspecified,
                "ACQUISITION_MODE_INVALID",
                "rewardResult.acquisitionMode");
            ValidateLaunchContext(
                collector,
                rewardResult.rewardLaunchContextIdentity,
                true,
                "rewardResult.rewardLaunchContextIdentity");

            if (request == null)
            {
                collector.Add(
                    "REQUEST_REQUIRED_FOR_REWARD",
                    "dropRequest",
                    "RewardResult validation requires its DropRequest.");
            }
            else
            {
                Match(
                    collector,
                    rewardResult.dropRequestId,
                    request.dropRequestId,
                    "REWARD_REQUEST_MISMATCH",
                    "rewardResult.dropRequestId");
                Match(
                    collector,
                    rewardResult.sourceFactId,
                    request.sourceFactId,
                    "REWARD_SOURCE_MISMATCH",
                    "rewardResult.sourceFactId");
                Match(
                    collector,
                    rewardResult.stageClearId,
                    request.stageClearId,
                    "REWARD_STAGE_CLEAR_MISMATCH",
                    "rewardResult.stageClearId");
                Match(
                    collector,
                    rewardResult.chapterId,
                    request.chapterId,
                    "REWARD_CHAPTER_MISMATCH",
                    "rewardResult.chapterId");
                Match(
                    collector,
                    rewardResult.stageId,
                    request.stageId,
                    "REWARD_STAGE_MISMATCH",
                    "rewardResult.stageId");
                Match(
                    collector,
                    rewardResult.acquisitionMode,
                    request.acquisitionMode,
                    "REWARD_MODE_MISMATCH",
                    "rewardResult.acquisitionMode");
                MatchContext(
                    collector,
                    rewardResult.rewardLaunchContextIdentity,
                    request.rewardLaunchContextIdentity,
                    "REWARD_CONTEXT_MISMATCH",
                    "rewardResult.rewardLaunchContextIdentity");
            }

            if (dropRollResult == null)
            {
                collector.Add(
                    "ROLL_RESULT_REQUIRED_FOR_REWARD",
                    "dropRollResult",
                    "RewardResult validation requires its DropRollResult.");
            }
            else
            {
                collector.Append(
                    ValidateDropRollResult(dropRollResult, request, stageClearFact),
                    "dropRollResult.");
                Match(
                    collector,
                    rewardResult.dropRollResultId,
                    dropRollResult.dropRollResultId,
                    "REWARD_ROLL_RESULT_MISMATCH",
                    "rewardResult.dropRollResultId");
            }

            Dictionary<string, DropRollEntry> rolledEntries =
                dropRollResult == null
                    ? new Dictionary<string, DropRollEntry>(StringComparer.Ordinal)
                    : dropRollResult.entries
                        .Where(entry => entry != null && entry.rollEntryId.Length > 0)
                        .GroupBy(entry => entry.rollEntryId, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            HashSet<string> consumedRollEntries = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> rewardEntryIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> itemInstanceIds = new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0; index < rewardResult.rewardEntries.Count; index++)
            {
                RewardEntry entry = rewardResult.rewardEntries[index];
                string prefix = "rewardResult.rewardEntries[" + index + "]";
                if (entry == null)
                {
                    collector.Add("REWARD_ENTRY_NULL", prefix, "Reward entry is required.");
                    continue;
                }

                RequireSchema(
                    collector,
                    entry.schemaId,
                    RewardEntry.SchemaId,
                    prefix + ".schemaId");
                RequireIdentifier(collector, entry.rewardEntryId, prefix + ".rewardEntryId");
                RequireIdentifier(
                    collector,
                    entry.sourceDefinitionId,
                    prefix + ".sourceDefinitionId");
                if (entry.sourceRuleVersion <= 0)
                {
                    collector.Add(
                        "SOURCE_RULE_VERSION_INVALID",
                        prefix + ".sourceRuleVersion",
                        "sourceRuleVersion must be greater than zero.");
                }

                if (entry.rewardEntryId.Length > 0
                    && !rewardEntryIds.Add(entry.rewardEntryId))
                {
                    collector.Add(
                        "DUPLICATE_REWARD_ENTRY",
                        prefix + ".rewardEntryId",
                        "rewardEntryId must be unique.");
                }

                ValidateEnum(
                    collector,
                    entry.grantKind,
                    RewardGrantKind.Unspecified,
                    "GRANT_KIND_INVALID",
                    prefix + ".grantKind");
                ValidateSubject(collector, entry.rewardSubjectIdentity, prefix);
                ValidateGrantAndSubject(
                    collector,
                    entry.grantKind,
                    entry.rewardSubjectIdentity,
                    rewardResult.acquisitionMode,
                    stageClearFact,
                    prefix);
                ValidateInstanceFields(
                    collector,
                    entry.rewardSubjectIdentity,
                    entry.itemInstanceId,
                    entry.instanceCanonicalSignature,
                    prefix);

                if (entry.itemInstanceId.Length > 0
                    && !itemInstanceIds.Add(entry.itemInstanceId))
                {
                    collector.Add(
                        "DUPLICATE_ITEM_INSTANCE",
                        prefix + ".itemInstanceId",
                        "Item instance identity must be unique.");
                }

                if (entry.quantity <= 0)
                {
                    collector.Add(
                        "QUANTITY_INVALID",
                        prefix + ".quantity",
                        "quantity must be greater than zero.");
                }

                if (entry.grantKind == RewardGrantKind.OrdinaryRoll)
                {
                    DropRollEntry rolledEntry;
                    if (!rolledEntries.TryGetValue(entry.sourceDefinitionId, out rolledEntry))
                    {
                        collector.Add(
                            "ROLLED_SOURCE_NOT_FOUND",
                            prefix + ".sourceDefinitionId",
                            "Ordinary reward must reference one DropRoll entry.");
                    }
                    else
                    {
                        if (!consumedRollEntries.Add(rolledEntry.rollEntryId))
                        {
                            collector.Add(
                                "ROLLED_SOURCE_REUSED",
                                prefix + ".sourceDefinitionId",
                                "Each DropRoll entry may project once.");
                        }

                        Match(
                            collector,
                            entry.grantKind,
                            rolledEntry.grantKind,
                            "ROLLED_GRANT_CHANGED",
                            prefix + ".grantKind");
                        MatchSubject(
                            collector,
                            entry.rewardSubjectIdentity,
                            rolledEntry.rewardSubjectIdentity,
                            "ROLLED_SUBJECT_CHANGED",
                            prefix + ".rewardSubjectIdentity");
                        Match(
                            collector,
                            entry.itemInstanceId,
                            rolledEntry.itemInstanceId,
                            "ROLLED_INSTANCE_CHANGED",
                            prefix + ".itemInstanceId");
                        Match(
                            collector,
                            entry.instanceCanonicalSignature,
                            rolledEntry.instanceCanonicalSignature,
                            "ROLLED_SIGNATURE_CHANGED",
                            prefix + ".instanceCanonicalSignature");
                        Match(
                            collector,
                            entry.quantity,
                            rolledEntry.quantity,
                            "ROLLED_QUANTITY_CHANGED",
                            prefix + ".quantity");
                    }
                }

                ValidateCanonical(
                    collector,
                    entry.canonicalPayload,
                    entry.canonicalSignature,
                    prefix);
            }

            if (dropRollResult != null && consumedRollEntries.Count != rolledEntries.Count)
            {
                collector.Add(
                    "ROLLED_PROJECTION_INCOMPLETE",
                    "rewardResult.rewardEntries",
                    "Every DropRoll entry must be preserved in RewardResult.");
            }

            ValidateCanonical(
                collector,
                rewardResult.canonicalPayload,
                rewardResult.canonicalSignature,
                "rewardResult");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateClaimIdentity(
            RewardClaimIdentity claim,
            RewardResult rewardResult)
        {
            ValidationCollector collector = new ValidationCollector();
            if (claim == null)
            {
                collector.Add("CLAIM_NULL", "claim", "RewardClaimIdentity is required.");
                return collector.Build();
            }

            RequireSchema(collector, claim.schemaId, RewardClaimIdentity.SchemaId, "claim.schemaId");
            RequireIdentifier(collector, claim.claimId, "claim.claimId");
            RequireIdentifier(collector, claim.rewardResultId, "claim.rewardResultId");
            RequireIdentifier(collector, claim.sourceFactId, "claim.sourceFactId");
            RequireIdentifier(collector, claim.dropRequestId, "claim.dropRequestId");
            RequireIdentifier(collector, claim.claimScopeId, "claim.claimScopeId");
            ValidateEnum(
                collector,
                claim.acquisitionMode,
                RewardAcquisitionMode.Unspecified,
                "ACQUISITION_MODE_INVALID",
                "claim.acquisitionMode");
            ValidateLaunchContext(
                collector,
                claim.rewardLaunchContextIdentity,
                true,
                "claim.rewardLaunchContextIdentity");
            ValidateStageClearIdentityForMode(
                collector,
                claim.acquisitionMode,
                claim.stageClearId,
                "claim.stageClearId");

            if (rewardResult == null)
            {
                collector.Add(
                    "REWARD_REQUIRED_FOR_CLAIM",
                    "rewardResult",
                    "Claim validation requires its RewardResult.");
            }
            else
            {
                Match(
                    collector,
                    claim.rewardResultId,
                    rewardResult.rewardResultId,
                    "CLAIM_REWARD_MISMATCH",
                    "claim.rewardResultId");
                Match(
                    collector,
                    claim.sourceFactId,
                    rewardResult.sourceFactId,
                    "CLAIM_SOURCE_MISMATCH",
                    "claim.sourceFactId");
                Match(
                    collector,
                    claim.stageClearId,
                    rewardResult.stageClearId,
                    "CLAIM_STAGE_CLEAR_MISMATCH",
                    "claim.stageClearId");
                Match(
                    collector,
                    claim.dropRequestId,
                    rewardResult.dropRequestId,
                    "CLAIM_REQUEST_MISMATCH",
                    "claim.dropRequestId");
                Match(
                    collector,
                    claim.acquisitionMode,
                    rewardResult.acquisitionMode,
                    "CLAIM_MODE_MISMATCH",
                    "claim.acquisitionMode");
                MatchContext(
                    collector,
                    claim.rewardLaunchContextIdentity,
                    rewardResult.rewardLaunchContextIdentity,
                    "CLAIM_CONTEXT_MISMATCH",
                    "claim.rewardLaunchContextIdentity");
            }

            ValidateCanonical(collector, claim.canonicalPayload, claim.canonicalSignature, "claim");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateDedupeIdentity(
            RewardDedupeIdentity dedupe,
            RewardResult rewardResult)
        {
            ValidationCollector collector = new ValidationCollector();
            if (dedupe == null)
            {
                collector.Add("DEDUPE_NULL", "dedupe", "RewardDedupeIdentity is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                dedupe.schemaId,
                RewardDedupeIdentity.SchemaId,
                "dedupe.schemaId");
            RequireIdentifier(collector, dedupe.dedupeKey, "dedupe.dedupeKey");
            RequireIdentifier(collector, dedupe.sourceFactId, "dedupe.sourceFactId");
            RequireIdentifier(collector, dedupe.dropRequestId, "dedupe.dropRequestId");
            RequireIdentifier(collector, dedupe.rewardResultId, "dedupe.rewardResultId");
            RequireIdentifier(collector, dedupe.claimScopeId, "dedupe.claimScopeId");
            ValidateEnum(
                collector,
                dedupe.acquisitionMode,
                RewardAcquisitionMode.Unspecified,
                "ACQUISITION_MODE_INVALID",
                "dedupe.acquisitionMode");
            ValidateLaunchContext(
                collector,
                dedupe.rewardLaunchContextIdentity,
                true,
                "dedupe.rewardLaunchContextIdentity");
            ValidateStageClearIdentityForMode(
                collector,
                dedupe.acquisitionMode,
                dedupe.stageClearId,
                "dedupe.stageClearId");

            if (rewardResult == null)
            {
                collector.Add(
                    "REWARD_REQUIRED_FOR_DEDUPE",
                    "rewardResult",
                    "Dedupe validation requires its RewardResult.");
            }
            else
            {
                Match(
                    collector,
                    dedupe.rewardResultId,
                    rewardResult.rewardResultId,
                    "DEDUPE_REWARD_MISMATCH",
                    "dedupe.rewardResultId");
                Match(
                    collector,
                    dedupe.sourceFactId,
                    rewardResult.sourceFactId,
                    "DEDUPE_SOURCE_MISMATCH",
                    "dedupe.sourceFactId");
                Match(
                    collector,
                    dedupe.stageClearId,
                    rewardResult.stageClearId,
                    "DEDUPE_STAGE_CLEAR_MISMATCH",
                    "dedupe.stageClearId");
                Match(
                    collector,
                    dedupe.dropRequestId,
                    rewardResult.dropRequestId,
                    "DEDUPE_REQUEST_MISMATCH",
                    "dedupe.dropRequestId");
                Match(
                    collector,
                    dedupe.acquisitionMode,
                    rewardResult.acquisitionMode,
                    "DEDUPE_MODE_MISMATCH",
                    "dedupe.acquisitionMode");
                MatchContext(
                    collector,
                    dedupe.rewardLaunchContextIdentity,
                    rewardResult.rewardLaunchContextIdentity,
                    "DEDUPE_CONTEXT_MISMATCH",
                    "dedupe.rewardLaunchContextIdentity");
            }

            ValidateCanonical(collector, dedupe.canonicalPayload, dedupe.canonicalSignature, "dedupe");
            return collector.Build();
        }

        public static RewardDropValidationResult ValidateExclusionIdentity(
            RewardExclusionIdentity exclusion)
        {
            ValidationCollector collector = new ValidationCollector();
            if (exclusion == null)
            {
                collector.Add(
                    "EXCLUSION_NULL",
                    "exclusion",
                    "RewardExclusionIdentity is required.");
                return collector.Build();
            }

            RequireSchema(
                collector,
                exclusion.schemaId,
                RewardExclusionIdentity.SchemaId,
                "exclusion.schemaId");
            RequireIdentifier(
                collector,
                exclusion.exclusionPolicyId,
                "exclusion.exclusionPolicyId");
            RequireIdentifier(collector, exclusion.reasonCode, "exclusion.reasonCode");
            ValidateEnum(
                collector,
                exclusion.subjectKind,
                RewardSubjectKind.Unspecified,
                "SUBJECT_KIND_INVALID",
                "exclusion.subjectKind");
            if (exclusion.policyVersion <= 0)
            {
                collector.Add(
                    "POLICY_VERSION_INVALID",
                    "exclusion.policyVersion",
                    "policyVersion must be greater than zero.");
            }

            if (exclusion.prohibitedAcquisitionModes.Count == 0)
            {
                collector.Add(
                    "EXCLUSION_MODE_SET_EMPTY",
                    "exclusion.prohibitedAcquisitionModes",
                    "At least one prohibited mode is required.");
            }

            for (int index = 0; index < exclusion.prohibitedAcquisitionModes.Count; index++)
            {
                ValidateEnum(
                    collector,
                    exclusion.prohibitedAcquisitionModes[index],
                    RewardAcquisitionMode.Unspecified,
                    "ACQUISITION_MODE_INVALID",
                    "exclusion.prohibitedAcquisitionModes[" + index + "]");
            }

            ValidateCanonical(
                collector,
                exclusion.canonicalPayload,
                exclusion.canonicalSignature,
                "exclusion");
            return collector.Build();
        }

        private static void ValidateLaunchContext(
            ValidationCollector collector,
            RewardLaunchContextIdentity context,
            bool requireProductClaimable,
            string field)
        {
            if (context == null)
            {
                collector.Add("LAUNCH_CONTEXT_NULL", field, "Launch context is required.");
                return;
            }

            RequireSchema(
                collector,
                context.schemaId,
                RewardLaunchContextIdentity.SchemaId,
                field + ".schemaId");
            RequireIdentifier(collector, context.launchContextId, field + ".launchContextId");
            ValidateEnum(
                collector,
                context.launchContextKind,
                RewardLaunchContextKind.Unspecified,
                "LAUNCH_CONTEXT_KIND_INVALID",
                field + ".launchContextKind");
            ValidateEnum(
                collector,
                context.persistencePolicy,
                RewardPersistencePolicy.Unspecified,
                "PERSISTENCE_POLICY_INVALID",
                field + ".persistencePolicy");
            if (context.contextVersion <= 0)
            {
                collector.Add(
                    "CONTEXT_VERSION_INVALID",
                    field + ".contextVersion",
                    "contextVersion must be greater than zero.");
            }

            bool campaign = context.SemanticallyEquals(
                RewardLaunchContextIdentity.CampaignNormalLv1);
            bool showcase = context.SemanticallyEquals(
                RewardLaunchContextIdentity.DevShowcaseLv40);
            if (!campaign && !showcase)
            {
                collector.Add(
                    "FROZEN_CONTEXT_IDENTITY_INVALID",
                    field,
                    "Context must match one frozen launch identity and policy pair.");
            }

            if (requireProductClaimable && !campaign)
            {
                collector.Add(
                    "PRODUCT_CONTEXT_REJECTED",
                    field,
                    "Only CAMPAIGN_NORMAL_LV1 is product claimable in P0A.");
            }

            ValidateCanonical(
                collector,
                context.canonicalPayload,
                context.canonicalSignature,
                field);
        }

        private static void ValidateSubject(
            ValidationCollector collector,
            RewardSubjectIdentity subject,
            string prefix)
        {
            if (subject == null)
            {
                collector.Add(
                    "SUBJECT_NULL",
                    prefix + ".rewardSubjectIdentity",
                    "Reward subject identity is required.");
                return;
            }

            RequireSchema(
                collector,
                subject.schemaId,
                RewardSubjectIdentity.SchemaId,
                prefix + ".rewardSubjectIdentity.schemaId");
            RequireIdentifier(
                collector,
                subject.subjectId,
                prefix + ".rewardSubjectIdentity.subjectId");
            RequireIdentifier(
                collector,
                subject.sourceCatalogId,
                prefix + ".rewardSubjectIdentity.sourceCatalogId");
            ValidateEnum(
                collector,
                subject.subjectKind,
                RewardSubjectKind.Unspecified,
                "SUBJECT_KIND_INVALID",
                prefix + ".rewardSubjectIdentity.subjectKind");
            if (subject.classificationVersion <= 0)
            {
                collector.Add(
                    "CLASSIFICATION_VERSION_INVALID",
                    prefix + ".rewardSubjectIdentity.classificationVersion",
                    "classificationVersion must be greater than zero.");
            }

            ValidateCanonical(
                collector,
                subject.canonicalPayload,
                subject.canonicalSignature,
                prefix + ".rewardSubjectIdentity");
        }

        private static void ValidateOrdinarySubject(
            ValidationCollector collector,
            RewardSubjectIdentity subject,
            string prefix)
        {
            if (subject == null)
            {
                return;
            }

            if (subject.subjectKind != RewardSubjectKind.OrdinaryGeneratedItem
                && subject.subjectKind != RewardSubjectKind.OrdinaryResource)
            {
                collector.Add(
                    "SPECIAL_SUBJECT_IN_ORDINARY_ROLL",
                    prefix + ".rewardSubjectIdentity.subjectKind",
                    "OrdinaryRoll accepts only ordinary generated items or resources.");
            }
        }

        private static void ValidateGrantAndSubject(
            ValidationCollector collector,
            RewardGrantKind grantKind,
            RewardSubjectIdentity subject,
            RewardAcquisitionMode mode,
            StageClearFact stageClearFact,
            string prefix)
        {
            if (grantKind == RewardGrantKind.OrdinaryRoll)
            {
                ValidateOrdinarySubject(collector, subject, prefix);
            }
            else if (grantKind == RewardGrantKind.FirstClearFixed)
            {
                if (mode != RewardAcquisitionMode.FirstClearOnline)
                {
                    collector.Add(
                        "FIRST_CLEAR_GRANT_MODE_INVALID",
                        prefix + ".grantKind",
                        "FirstClearFixed is legal only for FirstClearOnline.");
                }

                if (subject != null && subject.subjectKind == RewardSubjectKind.BossExclusive)
                {
                    collector.Add(
                        "BOSS_SUBJECT_GRANT_KIND_INVALID",
                        prefix + ".rewardSubjectIdentity.subjectKind",
                        "BossExclusive requires BossFirstClearGuaranteed.");
                }
            }
            else if (grantKind == RewardGrantKind.BossFirstClearGuaranteed)
            {
                if (mode != RewardAcquisitionMode.FirstClearOnline)
                {
                    collector.Add(
                        "BOSS_FIRST_CLEAR_MODE_INVALID",
                        prefix + ".grantKind",
                        "BossFirstClearGuaranteed requires FirstClearOnline.");
                }

                if (stageClearFact == null || !stageClearFact.isBossStage)
                {
                    collector.Add(
                        "BOSS_STAGE_REQUIRED",
                        prefix + ".grantKind",
                        "BossFirstClearGuaranteed requires a correlated Boss StageClearFact.");
                }

                if (subject != null
                    && subject.subjectKind != RewardSubjectKind.BossExclusive)
                {
                    collector.Add(
                        "BOSS_SUBJECT_REQUIRED",
                        prefix + ".rewardSubjectIdentity.subjectKind",
                        "BossFirstClearGuaranteed requires BossExclusive classification.");
                }
            }
            else if (grantKind == RewardGrantKind.SystemGrant)
            {
                ValidateSpecialGrant(
                    collector,
                    subject,
                    RewardSubjectKind.SystemRhythmSpecial,
                    mode,
                    prefix);
            }
            else if (grantKind == RewardGrantKind.StoryGrant)
            {
                ValidateSpecialGrant(
                    collector,
                    subject,
                    RewardSubjectKind.StoryCritical,
                    mode,
                    prefix);
            }
            else if (grantKind == RewardGrantKind.CosmeticGrant)
            {
                ValidateSpecialGrant(
                    collector,
                    subject,
                    RewardSubjectKind.CosmeticSkin,
                    mode,
                    prefix);
            }
        }

        private static void ValidateSpecialGrant(
            ValidationCollector collector,
            RewardSubjectIdentity subject,
            RewardSubjectKind requiredKind,
            RewardAcquisitionMode mode,
            string prefix)
        {
            if (mode == RewardAcquisitionMode.RepeatChallengeOnline
                || mode == RewardAcquisitionMode.OfflinePatrol)
            {
                collector.Add(
                    "SPECIAL_GRANT_MODE_INVALID",
                    prefix + ".grantKind",
                    "Special grants are forbidden in repeat and offline acquisition.");
            }

            if (subject != null && subject.subjectKind != requiredKind)
            {
                collector.Add(
                    "SPECIAL_GRANT_SUBJECT_MISMATCH",
                    prefix + ".rewardSubjectIdentity.subjectKind",
                    "Special grant kind and explicit subject classification must match.");
            }
        }

        private static void ValidateInstanceFields(
            ValidationCollector collector,
            RewardSubjectIdentity subject,
            string itemInstanceId,
            string instanceCanonicalSignature,
            string prefix)
        {
            if (subject == null)
            {
                return;
            }

            if (subject.subjectKind == RewardSubjectKind.OrdinaryGeneratedItem)
            {
                RequireIdentifier(collector, itemInstanceId, prefix + ".itemInstanceId");
                RequireIdentifier(
                    collector,
                    instanceCanonicalSignature,
                    prefix + ".instanceCanonicalSignature");
            }
            else if (itemInstanceId.Length != 0 || instanceCanonicalSignature.Length != 0)
            {
                collector.Add(
                    "NON_ITEM_INSTANCE_FORGED",
                    prefix + ".itemInstanceId",
                    "Only OrdinaryGeneratedItem may carry Item instance identity.");
            }
        }

        private static void ValidateStageClearIdentityForMode(
            ValidationCollector collector,
            RewardAcquisitionMode mode,
            string stageClearId,
            string field)
        {
            if (mode == RewardAcquisitionMode.FirstClearOnline
                || mode == RewardAcquisitionMode.RepeatChallengeOnline)
            {
                RequireIdentifier(collector, stageClearId, field);
            }
            else if (mode == RewardAcquisitionMode.OfflinePatrol
                && stageClearId.Length != 0)
            {
                collector.Add(
                    "OFFLINE_STAGE_CLEAR_FORGERY",
                    field,
                    "OfflinePatrol identity must not carry a StageClear identity.");
            }
        }

        private static void ValidateCanonical(
            ValidationCollector collector,
            string payload,
            string signature,
            string field)
        {
            if (payload == null || payload.Length == 0)
            {
                collector.Add(
                    "CANONICAL_PAYLOAD_EMPTY",
                    field + ".canonicalPayload",
                    "Canonical payload is required.");
                return;
            }

            string expected = RewardDropCanonical.ComputeSignature(payload);
            if (!RewardDropCanonical.EqualsOrdinal(signature, expected))
            {
                collector.Add(
                    "CANONICAL_SIGNATURE_MISMATCH",
                    field + ".canonicalSignature",
                    "Canonical signature must be lowercase SHA-256 of the UTF-8 payload.");
            }
        }

        private static void RequireIdentifier(
            ValidationCollector collector,
            string value,
            string field)
        {
            if (value == null || value.Length == 0)
            {
                collector.Add("IDENTITY_REQUIRED", field, "Identity is required.");
            }
        }

        private static void RequireSchema(
            ValidationCollector collector,
            string actual,
            string expected,
            string field)
        {
            if (!RewardDropCanonical.EqualsOrdinal(actual, expected))
            {
                collector.Add(
                    "SCHEMA_MISMATCH",
                    field,
                    "Schema ID does not match the frozen contract schema.");
            }
        }

        private static void ValidateEnum<T>(
            ValidationCollector collector,
            T value,
            T unspecified,
            string code,
            string field)
            where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value) || EqualityComparer<T>.Default.Equals(value, unspecified))
            {
                collector.Add(code, field, "Enum value must be defined and non-Unspecified.");
            }
        }

        private static void Match(
            ValidationCollector collector,
            string actual,
            string expected,
            string code,
            string field)
        {
            if (!RewardDropCanonical.EqualsOrdinal(actual, expected))
            {
                collector.Add(code, field, "Correlated string identity mismatch.");
            }
        }

        private static void Match<T>(
            ValidationCollector collector,
            T actual,
            T expected,
            string code,
            string field)
        {
            if (!EqualityComparer<T>.Default.Equals(actual, expected))
            {
                collector.Add(code, field, "Correlated value mismatch.");
            }
        }

        private static void MatchContext(
            ValidationCollector collector,
            RewardLaunchContextIdentity actual,
            RewardLaunchContextIdentity expected,
            string code,
            string field)
        {
            if (actual == null || !actual.SemanticallyEquals(expected))
            {
                collector.Add(code, field, "Launch context identity mismatch.");
            }
        }

        private static void MatchSubject(
            ValidationCollector collector,
            RewardSubjectIdentity actual,
            RewardSubjectIdentity expected,
            string code,
            string field)
        {
            if (actual == null || !actual.SemanticallyEquals(expected))
            {
                collector.Add(code, field, "Reward subject identity mismatch.");
            }
        }

        private sealed class ValidationCollector
        {
            private readonly List<RewardDropValidationError> errors =
                new List<RewardDropValidationError>();

            public void Add(string code, string field, string message)
            {
                errors.Add(new RewardDropValidationError(code, field, message));
            }

            public void Append(RewardDropValidationResult result, string fieldPrefix)
            {
                if (result == null)
                {
                    return;
                }

                for (int index = 0; index < result.errors.Count; index++)
                {
                    RewardDropValidationError error = result.errors[index];
                    Add(error.code, fieldPrefix + error.field, error.message);
                }
            }

            public RewardDropValidationResult Build()
            {
                return new RewardDropValidationResult(errors);
            }
        }
    }
}
