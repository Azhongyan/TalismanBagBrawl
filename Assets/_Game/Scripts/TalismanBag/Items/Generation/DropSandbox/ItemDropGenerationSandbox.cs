using System;
using System.Linq;
using TalismanBag.Items.Generation.DropCore;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.DropSandbox
{
    public static class ItemDropGenerationSandbox
    {
        private const string QaAttributeRollBandProfileId =
            "QA_DROP_ATTRIBUTE_ROLL_BAND_V1";

        public static ItemDropGenerationResult Generate(ItemDropGenerationRequest request)
        {
            try
            {
                return GenerateAdapter(request);
            }
            catch (Exception)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.InstanceRollFailed,
                    "Drop generation stopped after an internal validation failure; no partial snapshot was returned.");
            }
        }

        private static ItemDropGenerationResult GenerateAdapter(
            ItemDropGenerationRequest request)
        {
            ItemDropGenerationResult legacyValidation = ValidateLegacyRequest(request);
            if (legacyValidation != null)
            {
                return legacyValidation;
            }

            ItemDropCoreCandidatePool corePool = request.candidatePool == null
                ? null
                : new ItemDropCoreCandidatePool(
                    request.candidatePool.poolId,
                    request.candidatePool.Candidates.Select(candidate =>
                        candidate == null
                            ? null
                            : new ItemDropCoreCandidateEntry(
                                candidate.baseItemId,
                                ItemDropCoreCandidateKind.OrdinaryGeneratedItem,
                                candidate.weightUnits)));

            ItemDropRarityTierProfile coreRarity;
            string rarityPolicySource;
            string projectedRarityProfileId;
            if (request.sourceContext.stageNumber <= 10)
            {
                coreRarity = new ItemDropRarityTierProfile(
                    ItemDropRarityPolicySources.LockedStage1To10White,
                    ItemDropRarityTierMode.Fixed,
                    new[] { new ItemDropRarityTierEntry(ItemInstanceRarity.White, 1) });
                rarityPolicySource = ItemDropRarityPolicySources.LockedStage1To10White;
                projectedRarityProfileId = string.Empty;
            }
            else
            {
                coreRarity = request.rarityWeightProfile == null
                    ? null
                    : new ItemDropRarityTierProfile(
                        request.rarityWeightProfile.profileId,
                        ItemDropRarityTierMode.Weighted,
                        request.rarityWeightProfile.Entries.Select(entry =>
                            entry == null
                                ? null
                                : new ItemDropRarityTierEntry(
                                    entry.rarity,
                                    entry.weightUnits)));
                rarityPolicySource = ItemDropRarityPolicySources.QaWeightProfile;
                projectedRarityProfileId =
                    request.rarityWeightProfile?.profileId ?? string.Empty;
            }

            ItemDropDeterministicCoreRequest coreRequest = new(
                request.dropRequestId,
                request.itemInstanceId,
                request.rootSeed,
                request.generationVersion,
                ItemDropCoreDataStatus.DevOnlyCandidate,
                new ItemDropCoreSourceIdentity(
                    request.sourceContext.sourceContextId,
                    request.sourceContext.sourceKey,
                    request.sourceContext.stageNumber),
                request.sourceContext.candidatePoolId,
                coreRarity?.rarityTierProfileId ?? string.Empty,
                QaAttributeRollBandProfileId,
                DeterministicItemRandom.AlgorithmId,
                request.foundation,
                corePool,
                coreRarity,
                request.statSchema,
                request.affixSchema,
                request.coreBuildSchema,
                request.BuildQualificationRollProfiles);

            ItemDropDeterministicCoreResult coreResult =
                ItemDropDeterministicCore.Generate(coreRequest);
            if (!coreResult.isSuccess)
            {
                return Failure(
                    MapValidationCode(coreResult.primaryValidationCode),
                    coreResult.ValidationErrors.Count == 0
                        ? "Core generation failed."
                        : coreResult.ValidationErrors[0].message);
            }

            ItemDropDeterministicCoreSnapshot coreSnapshot = coreResult.snapshot;
            ItemDropGenerationSnapshot snapshot = new(
                request.generationDataStatus,
                request.dropRequestId,
                request.sourceContext,
                rarityPolicySource,
                projectedRarityProfileId,
                request.rootSeed,
                request.generationVersion,
                coreSnapshot.selectedBaseItemId,
                coreSnapshot.selectedRarity,
                coreSnapshot.generatedInstance);
            return new ItemDropGenerationResult(
                snapshot,
                Array.Empty<ItemDropGenerationValidationError>());
        }

        private static ItemDropGenerationResult ValidateLegacyRequest(
            ItemDropGenerationRequest request)
        {
            if (request == null)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.RequestNull,
                    "Item drop generation request is null.");
            }

            if (string.IsNullOrWhiteSpace(request.dropRequestId))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.DropRequestIdEmpty,
                    "dropRequestId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.itemInstanceId))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.ItemInstanceIdEmpty,
                    "itemInstanceId is required.");
            }

            if (request.generationVersion
                != DeterministicItemDropRandom.SupportedGenerationVersion)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.GenerationVersionUnsupported,
                    "Only generationVersion 1 is supported by this drop algorithm.");
            }

            if (!string.Equals(
                    request.generationDataStatus,
                    ItemGenerationDataStatus.QaFixtureCanonical,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.GenerationDataStatusInvalid,
                    "Drop generation accepts only QA fixture data with all three isolation markers.");
            }

            if (request.foundation == null)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.FoundationNull,
                    "Item generation foundation is null.");
            }

            if (!request.foundation.isValid)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.FoundationInvalid,
                    "Item generation foundation contains validation errors.");
            }

            if (request.sourceContext == null)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.SourceContextNull,
                    "Drop source context is null.");
            }

            if (string.IsNullOrWhiteSpace(request.sourceContext.sourceContextId))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.SourceContextIdEmpty,
                    "sourceContextId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.sourceContext.sourceKey))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.SourceKeyEmpty,
                    "sourceKey is required.");
            }

            if (request.sourceContext.stageNumber <= 0)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.StageNumberInvalid,
                    "stageNumber must be greater than zero.");
            }

            if (!string.Equals(
                    request.sourceContext.dataMaturityKey,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.SourceContextDataStatusInvalid,
                    "Drop source context must be marked QA_FIXTURE_ONLY.");
            }

            if (request.candidatePool == null)
            {
                return Failure(
                    ItemDropGenerationValidationCodes.CandidatePoolNull,
                    "Candidate pool is null.");
            }

            if (string.IsNullOrWhiteSpace(request.candidatePool.poolId)
                || !string.Equals(
                    request.candidatePool.poolId,
                    request.sourceContext.candidatePoolId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.CandidatePoolIdMismatch,
                    "Candidate pool ID must exactly match the source context.");
            }

            if (!string.Equals(
                    request.candidatePool.dataMaturityKey,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropGenerationValidationCodes.CandidatePoolDataStatusInvalid,
                    "Candidate pool must be marked QA_FIXTURE_ONLY.");
            }

            if (request.sourceContext.stageNumber <= 10)
            {
                if (request.rarityWeightProfile != null)
                {
                    return Failure(
                        ItemDropGenerationValidationCodes.EarlyStageRarityProfileForbidden,
                        "Stages 1-10 reject rarity profiles and do not create a rarity stream.");
                }
            }
            else
            {
                if (request.rarityWeightProfile == null)
                {
                    return Failure(
                        ItemDropGenerationValidationCodes.RarityPolicyUnresolved,
                        "Stages 11+ require an explicit QA rarity weight profile.");
                }

                if (string.IsNullOrWhiteSpace(request.rarityWeightProfile.profileId)
                    || !string.Equals(
                        request.rarityWeightProfile.profileId,
                        request.sourceContext.rarityWeightProfileId,
                        StringComparison.Ordinal))
                {
                    return Failure(
                        ItemDropGenerationValidationCodes.RarityProfileIdMismatch,
                        "Rarity profile ID must exactly match the source context.");
                }

                if (!string.Equals(
                        request.rarityWeightProfile.dataMaturityKey,
                        ItemGenerationDataStatus.QaFixtureOnly,
                        StringComparison.Ordinal))
                {
                    return Failure(
                        ItemDropGenerationValidationCodes.RarityProfileDataStatusInvalid,
                        "Rarity profile must be marked QA_FIXTURE_ONLY.");
                }
            }

            return null;
        }

        private static string MapValidationCode(string code)
        {
            switch (code)
            {
                case ItemDropCoreValidationCodes.RequestNull:
                    return ItemDropGenerationValidationCodes.RequestNull;
                case ItemDropCoreValidationCodes.RequestIdEmpty:
                    return ItemDropGenerationValidationCodes.DropRequestIdEmpty;
                case ItemDropCoreValidationCodes.ItemInstanceIdEmpty:
                    return ItemDropGenerationValidationCodes.ItemInstanceIdEmpty;
                case ItemDropCoreValidationCodes.GenerationVersionUnsupported:
                    return ItemDropGenerationValidationCodes.GenerationVersionUnsupported;
                case ItemDropCoreValidationCodes.DataStatusInvalid:
                    return ItemDropGenerationValidationCodes.GenerationDataStatusInvalid;
                case ItemDropCoreValidationCodes.FoundationNull:
                    return ItemDropGenerationValidationCodes.FoundationNull;
                case ItemDropCoreValidationCodes.FoundationInvalid:
                    return ItemDropGenerationValidationCodes.FoundationInvalid;
                case ItemDropCoreValidationCodes.SourceIdentityNull:
                    return ItemDropGenerationValidationCodes.SourceContextNull;
                case ItemDropCoreValidationCodes.SourceContextIdEmpty:
                    return ItemDropGenerationValidationCodes.SourceContextIdEmpty;
                case ItemDropCoreValidationCodes.SourceKeyEmpty:
                    return ItemDropGenerationValidationCodes.SourceKeyEmpty;
                case ItemDropCoreValidationCodes.SourceOrdinalInvalid:
                    return ItemDropGenerationValidationCodes.StageNumberInvalid;
                case ItemDropCoreValidationCodes.CandidatePoolNull:
                    return ItemDropGenerationValidationCodes.CandidatePoolNull;
                case ItemDropCoreValidationCodes.CandidatePoolIdentityEmpty:
                case ItemDropCoreValidationCodes.CandidatePoolIdMismatch:
                    return ItemDropGenerationValidationCodes.CandidatePoolIdMismatch;
                case ItemDropCoreValidationCodes.CandidatePoolEmpty:
                    return ItemDropGenerationValidationCodes.CandidatePoolEmpty;
                case ItemDropCoreValidationCodes.CandidateEntryInvalid:
                    return ItemDropGenerationValidationCodes.CandidateBaseItemEmpty;
                case ItemDropCoreValidationCodes.CandidateDuplicate:
                    return ItemDropGenerationValidationCodes.CandidateDuplicate;
                case ItemDropCoreValidationCodes.CandidateBaseItemUnknown:
                case ItemDropCoreValidationCodes.CandidateKindInvalid:
                    return ItemDropGenerationValidationCodes.CandidateBaseItemUnknown;
                case ItemDropCoreValidationCodes.I031Forbidden:
                    return ItemDropGenerationValidationCodes.I031Forbidden;
                case ItemDropCoreValidationCodes.CandidateWeightInvalid:
                    return ItemDropGenerationValidationCodes.CandidateWeightInvalid;
                case ItemDropCoreValidationCodes.CandidateWeightSumOverflow:
                    return ItemDropGenerationValidationCodes.CandidateWeightSumOverflow;
                case ItemDropCoreValidationCodes.RarityTierProfileNull:
                    return ItemDropGenerationValidationCodes.RarityPolicyUnresolved;
                case ItemDropCoreValidationCodes.RarityTierProfileIdentityEmpty:
                case ItemDropCoreValidationCodes.RarityTierProfileIdMismatch:
                    return ItemDropGenerationValidationCodes.RarityProfileIdMismatch;
                case ItemDropCoreValidationCodes.RarityTierProfileEmpty:
                case ItemDropCoreValidationCodes.RarityTierEntryInvalid:
                case ItemDropCoreValidationCodes.FixedRarityTierEntryCountInvalid:
                case ItemDropCoreValidationCodes.RarityTierModeInvalid:
                    return ItemDropGenerationValidationCodes.RarityEntryInvalid;
                case ItemDropCoreValidationCodes.RarityTierDuplicate:
                    return ItemDropGenerationValidationCodes.RarityEntryDuplicate;
                case ItemDropCoreValidationCodes.RarityTierWeightInvalid:
                    return ItemDropGenerationValidationCodes.RarityWeightInvalid;
                case ItemDropCoreValidationCodes.RarityTierWeightSumOverflow:
                    return ItemDropGenerationValidationCodes.RarityWeightSumOverflow;
                case ItemDropCoreValidationCodes.DropDomainInvalid:
                    return ItemDropGenerationValidationCodes.DropDomainInvalid;
                case ItemDropCoreValidationCodes.CoreSchemaMissing:
                case ItemDropCoreValidationCodes.CoreProfileMissing:
                    return ItemDropGenerationValidationCodes.CoreProfileMissing;
                case ItemDropCoreValidationCodes.BuildRollProfileMissing:
                    return ItemDropGenerationValidationCodes.BuildRollProfileMissing;
                case ItemDropCoreValidationCodes.BuildRollProfileDuplicate:
                    return ItemDropGenerationValidationCodes.BuildRollProfileDuplicate;
                case ItemDropCoreValidationCodes.IdentityCreationFailed:
                    return ItemDropGenerationValidationCodes.IdentityCreationFailed;
                case ItemDropCoreValidationCodes.AttributeRollBandProfileIdentityEmpty:
                case ItemDropCoreValidationCodes.RollPolicyVersionEmpty:
                case ItemDropCoreValidationCodes.RollPolicyVersionUnsupported:
                case ItemDropCoreValidationCodes.StatSchemaMissing:
                case ItemDropCoreValidationCodes.AffixSchemaMissing:
                case ItemDropCoreValidationCodes.InstanceRollFailed:
                default:
                    return ItemDropGenerationValidationCodes.InstanceRollFailed;
            }
        }

        private static ItemDropGenerationResult Failure(string code, string message)
        {
            return new ItemDropGenerationResult(
                null,
                new[] { new ItemDropGenerationValidationError(code, message) });
        }
    }
}
