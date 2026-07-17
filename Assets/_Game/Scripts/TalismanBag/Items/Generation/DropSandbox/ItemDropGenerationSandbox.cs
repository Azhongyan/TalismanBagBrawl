using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.DropSandbox
{
    public static class ItemDropGenerationSandbox
    {
        public static ItemDropGenerationResult Generate(ItemDropGenerationRequest request)
        {
            try
            {
                return GenerateCore(request);
            }
            catch (Exception)
            {
                return Failure(ItemDropGenerationValidationCodes.InstanceRollFailed,
                    "Drop generation stopped after an internal validation failure; no partial snapshot was returned.");
            }
        }

        private static ItemDropGenerationResult GenerateCore(ItemDropGenerationRequest request)
        {
            if (request == null)
            {
                return Failure(ItemDropGenerationValidationCodes.RequestNull,
                    "Item drop generation request is null.");
            }

            if (string.IsNullOrWhiteSpace(request.dropRequestId))
            {
                return Failure(ItemDropGenerationValidationCodes.DropRequestIdEmpty,
                    "dropRequestId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.itemInstanceId))
            {
                return Failure(ItemDropGenerationValidationCodes.ItemInstanceIdEmpty,
                    "itemInstanceId is required.");
            }

            if (request.generationVersion != DeterministicItemDropRandom.SupportedGenerationVersion)
            {
                return Failure(ItemDropGenerationValidationCodes.GenerationVersionUnsupported,
                    "Only generationVersion 1 is supported by this drop algorithm.");
            }

            if (!string.Equals(request.generationDataStatus,
                ItemGenerationDataStatus.QaFixtureCanonical, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.GenerationDataStatusInvalid,
                    "Drop generation accepts only QA fixture data with all three isolation markers.");
            }

            if (request.foundation == null)
            {
                return Failure(ItemDropGenerationValidationCodes.FoundationNull,
                    "Item generation foundation is null.");
            }

            if (!request.foundation.isValid)
            {
                return Failure(ItemDropGenerationValidationCodes.FoundationInvalid,
                    "Item generation foundation contains validation errors.");
            }

            ItemDropGenerationResult sourceValidation = ValidateSource(request.sourceContext);
            if (sourceValidation != null)
            {
                return sourceValidation;
            }

            ItemDropGenerationResult candidateValidation = ValidateCandidates(
                request.sourceContext, request.candidatePool, out ItemDropCandidateEntrySnapshot[] candidates,
                out ulong candidateWeightTotal);
            if (candidateValidation != null)
            {
                return candidateValidation;
            }

            if (!DeterministicItemDropRandom.TryCreateStream(request.rootSeed, request.generationVersion,
                request.dropRequestId, request.sourceContext.sourceContextId, request.sourceContext.sourceKey,
                request.sourceContext.stageNumber,
                new[] { "drop", "candidate", request.candidatePool.poolId },
                out DeterministicItemRandom.Stream candidateStream)
                || !candidateStream.TryNextBounded(candidateWeightTotal, out ulong candidateTarget))
            {
                return Failure(ItemDropGenerationValidationCodes.DropDomainInvalid,
                    "Candidate selection domain is invalid.");
            }

            string selectedBaseItemId = SelectCandidate(candidates, candidateTarget).baseItemId;
            ItemDropGenerationResult rarityValidation = SelectRarity(request,
                out ItemInstanceRarity selectedRarity, out string rarityPolicySource,
                out string rarityWeightProfileId);
            if (rarityValidation != null)
            {
                return rarityValidation;
            }

            if (request.coreBuildSchema == null)
            {
                return Failure(ItemDropGenerationValidationCodes.CoreProfileMissing,
                    "Core/Build schema is null.");
            }

            ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> coreResult =
                request.coreBuildSchema.QueryCorePotentialProfile(selectedBaseItemId, selectedRarity);
            if (!coreResult.isSuccess
                || coreResult.value.resolutionStatus != ItemCorePotentialResolutionStatus.Defined)
            {
                return Failure(ItemDropGenerationValidationCodes.CoreProfileMissing,
                    $"A Defined core potential profile is required for '{selectedBaseItemId}@{selectedRarity.ToStableKey()}'.");
            }

            ItemBuildQualificationRollProfile buildProfile = null;
            if (selectedRarity != ItemInstanceRarity.White)
            {
                ItemBuildQualificationRollProfile[] matches = request.BuildQualificationRollProfiles
                    .Where(profile => profile != null && profile.rarity == selectedRarity)
                    .ToArray();
                if (matches.Length == 0)
                {
                    return Failure(ItemDropGenerationValidationCodes.BuildRollProfileMissing,
                        $"A QA Build roll profile is required for '{selectedRarity.ToStableKey()}'.");
                }

                if (matches.Length > 1)
                {
                    return Failure(ItemDropGenerationValidationCodes.BuildRollProfileDuplicate,
                        $"More than one QA Build roll profile matches '{selectedRarity.ToStableKey()}'.");
                }

                buildProfile = matches[0];
            }

            ItemInstanceIdentityCreationResult identityResult =
                ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    request.foundation,
                    request.itemInstanceId,
                    selectedBaseItemId,
                    selectedRarity,
                    request.generationVersion,
                    request.rootSeed,
                    coreResult.value.cultivationPotentialProfileId);
            if (!identityResult.isValid)
            {
                return Failure(ItemDropGenerationValidationCodes.IdentityCreationFailed,
                    "Item instance identity creation failed: " + identityResult.primaryValidationCode + ".");
            }

            ItemInstanceRollResult rollResult = ItemInstanceRollEngine.Generate(new ItemInstanceRollRequest(
                identityResult.snapshot,
                request.statSchema,
                request.affixSchema,
                request.coreBuildSchema,
                buildProfile,
                request.generationDataStatus));
            if (!rollResult.isSuccess)
            {
                return Failure(ItemDropGenerationValidationCodes.InstanceRollFailed,
                    "Item instance roll failed: " + rollResult.primaryValidationCode + ".");
            }

            ItemDropGenerationSnapshot snapshot = new(
                request.generationDataStatus,
                request.dropRequestId,
                request.sourceContext,
                rarityPolicySource,
                rarityWeightProfileId,
                request.rootSeed,
                request.generationVersion,
                selectedBaseItemId,
                selectedRarity,
                rollResult.snapshot);
            return new ItemDropGenerationResult(snapshot,
                Array.Empty<ItemDropGenerationValidationError>());
        }

        private static ItemDropGenerationResult ValidateSource(ItemDropSourceContextSnapshot source)
        {
            if (source == null)
            {
                return Failure(ItemDropGenerationValidationCodes.SourceContextNull,
                    "Drop source context is null.");
            }

            if (string.IsNullOrWhiteSpace(source.sourceContextId))
            {
                return Failure(ItemDropGenerationValidationCodes.SourceContextIdEmpty,
                    "sourceContextId is required.");
            }

            if (string.IsNullOrWhiteSpace(source.sourceKey))
            {
                return Failure(ItemDropGenerationValidationCodes.SourceKeyEmpty,
                    "sourceKey is required.");
            }

            if (source.stageNumber <= 0)
            {
                return Failure(ItemDropGenerationValidationCodes.StageNumberInvalid,
                    "stageNumber must be greater than zero.");
            }

            if (!string.Equals(source.dataMaturityKey,
                ItemGenerationDataStatus.QaFixtureOnly, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.SourceContextDataStatusInvalid,
                    "Drop source context must be marked QA_FIXTURE_ONLY.");
            }

            return null;
        }

        private static ItemDropGenerationResult ValidateCandidates(
            ItemDropSourceContextSnapshot source,
            ItemDropCandidatePoolSnapshot pool,
            out ItemDropCandidateEntrySnapshot[] candidates,
            out ulong totalWeight)
        {
            candidates = Array.Empty<ItemDropCandidateEntrySnapshot>();
            totalWeight = 0UL;
            if (pool == null)
            {
                return Failure(ItemDropGenerationValidationCodes.CandidatePoolNull,
                    "Candidate pool is null.");
            }

            if (string.IsNullOrWhiteSpace(pool.poolId)
                || !string.Equals(pool.poolId, source.candidatePoolId, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.CandidatePoolIdMismatch,
                    "Candidate pool ID must exactly match the source context.");
            }

            if (!string.Equals(pool.dataMaturityKey,
                ItemGenerationDataStatus.QaFixtureOnly, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.CandidatePoolDataStatusInvalid,
                    "Candidate pool must be marked QA_FIXTURE_ONLY.");
            }

            if (pool.Candidates.Count == 0)
            {
                return Failure(ItemDropGenerationValidationCodes.CandidatePoolEmpty,
                    "Candidate pool must not be empty.");
            }

            foreach (ItemDropCandidateEntrySnapshot candidate in pool.Candidates)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.baseItemId))
                {
                    return Failure(ItemDropGenerationValidationCodes.CandidateBaseItemEmpty,
                        "Every candidate requires a baseItemId.");
                }

                if (string.Equals(candidate.baseItemId,
                    ItemRarityInstanceFoundation.CoreProgressionBaseItemId, StringComparison.Ordinal))
                {
                    return Failure(ItemDropGenerationValidationCodes.I031Forbidden,
                        "I031 is excluded from ordinary drop candidates.");
                }

                if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(candidate.baseItemId))
                {
                    return Failure(ItemDropGenerationValidationCodes.CandidateBaseItemUnknown,
                        $"Unknown ordinary candidate '{candidate.baseItemId}'.");
                }

                if (candidate.weightUnits <= 0)
                {
                    return Failure(ItemDropGenerationValidationCodes.CandidateWeightInvalid,
                        $"Candidate '{candidate.baseItemId}' weightUnits must be positive.");
                }
            }

            if (pool.Candidates.Where(candidate => candidate != null)
                .GroupBy(candidate => candidate.baseItemId, StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
            {
                return Failure(ItemDropGenerationValidationCodes.CandidateDuplicate,
                    "Candidate baseItemId values must be unique.");
            }

            candidates = pool.Candidates.OrderBy(candidate => candidate.baseItemId, StringComparer.Ordinal).ToArray();
            foreach (ItemDropCandidateEntrySnapshot candidate in candidates)
            {
                if (!TryAddWeight(ref totalWeight, candidate.weightUnits))
                {
                    return Failure(ItemDropGenerationValidationCodes.CandidateWeightSumOverflow,
                        "Candidate weight sum exceeds ulong capacity.");
                }
            }

            return null;
        }

        private static ItemDropGenerationResult SelectRarity(
            ItemDropGenerationRequest request,
            out ItemInstanceRarity selectedRarity,
            out string rarityPolicySource,
            out string rarityWeightProfileId)
        {
            selectedRarity = ItemInstanceRarity.White;
            rarityPolicySource = string.Empty;
            rarityWeightProfileId = string.Empty;
            if (request.sourceContext.stageNumber <= 10)
            {
                if (request.rarityWeightProfile != null)
                {
                    return Failure(ItemDropGenerationValidationCodes.EarlyStageRarityProfileForbidden,
                        "Stages 1-10 reject rarity profiles and do not create a rarity stream.");
                }

                rarityPolicySource = ItemDropRarityPolicySources.LockedStage1To10White;
                return null;
            }

            ItemDropRarityWeightProfileSnapshot profile = request.rarityWeightProfile;
            if (profile == null)
            {
                return Failure(ItemDropGenerationValidationCodes.RarityPolicyUnresolved,
                    "Stages 11+ require an explicit QA rarity weight profile.");
            }

            if (string.IsNullOrWhiteSpace(profile.profileId)
                || !string.Equals(profile.profileId,
                    request.sourceContext.rarityWeightProfileId, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityProfileIdMismatch,
                    "Rarity profile ID must exactly match the source context.");
            }

            if (!string.Equals(profile.dataMaturityKey,
                ItemGenerationDataStatus.QaFixtureOnly, StringComparison.Ordinal))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityProfileDataStatusInvalid,
                    "Rarity profile must be marked QA_FIXTURE_ONLY.");
            }

            if (profile.Entries.Count == 0 || profile.Entries.Any(entry => entry == null))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityEntryInvalid,
                    "Rarity profile must contain non-null entries.");
            }

            if (profile.Entries.Any(entry => !ItemInstanceRarityCatalog.TryGetDefinition(entry.rarity, out _)))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityEntryInvalid,
                    "Rarity profile contains an unsupported rarity.");
            }

            if (profile.Entries.GroupBy(entry => entry.rarity).Any(group => group.Count() > 1))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityEntryDuplicate,
                    "Rarity entries must be unique.");
            }

            if (profile.Entries.Any(entry => entry.weightUnits <= 0))
            {
                return Failure(ItemDropGenerationValidationCodes.RarityWeightInvalid,
                    "Rarity weightUnits must be positive.");
            }

            ulong totalWeight = 0UL;
            ItemDropRarityWeightEntrySnapshot[] entries = profile.Entries
                .OrderBy(entry => entry.rarity.ToTierIndex())
                .ThenBy(entry => (int)entry.rarity)
                .ToArray();
            foreach (ItemDropRarityWeightEntrySnapshot entry in entries)
            {
                if (!TryAddWeight(ref totalWeight, entry.weightUnits))
                {
                    return Failure(ItemDropGenerationValidationCodes.RarityWeightSumOverflow,
                        "Rarity weight sum exceeds ulong capacity.");
                }
            }

            if (!DeterministicItemDropRandom.TryCreateStream(request.rootSeed, request.generationVersion,
                request.dropRequestId, request.sourceContext.sourceContextId, request.sourceContext.sourceKey,
                request.sourceContext.stageNumber,
                new[] { "drop", "rarity", profile.profileId },
                out DeterministicItemRandom.Stream rarityStream)
                || !rarityStream.TryNextBounded(totalWeight, out ulong target))
            {
                return Failure(ItemDropGenerationValidationCodes.DropDomainInvalid,
                    "Rarity selection domain is invalid.");
            }

            selectedRarity = SelectRarity(entries, target).rarity;
            rarityPolicySource = ItemDropRarityPolicySources.QaWeightProfile;
            rarityWeightProfileId = profile.profileId;
            return null;
        }

        private static ItemDropCandidateEntrySnapshot SelectCandidate(
            IReadOnlyList<ItemDropCandidateEntrySnapshot> entries,
            ulong target)
        {
            ulong cursor = 0UL;
            foreach (ItemDropCandidateEntrySnapshot entry in entries)
            {
                cursor += unchecked((ulong)entry.weightUnits);
                if (target < cursor)
                {
                    return entry;
                }
            }

            return entries[entries.Count - 1];
        }

        private static ItemDropRarityWeightEntrySnapshot SelectRarity(
            IReadOnlyList<ItemDropRarityWeightEntrySnapshot> entries,
            ulong target)
        {
            ulong cursor = 0UL;
            foreach (ItemDropRarityWeightEntrySnapshot entry in entries)
            {
                cursor += unchecked((ulong)entry.weightUnits);
                if (target < cursor)
                {
                    return entry;
                }
            }

            return entries[entries.Count - 1];
        }

        private static bool TryAddWeight(ref ulong sum, long weightUnits)
        {
            if (weightUnits <= 0)
            {
                return false;
            }

            ulong weight = unchecked((ulong)weightUnits);
            if (ulong.MaxValue - sum < weight)
            {
                return false;
            }

            sum += weight;
            return true;
        }

        private static ItemDropGenerationResult Failure(string code, string message)
        {
            return new ItemDropGenerationResult(null,
                new[] { new ItemDropGenerationValidationError(code, message) });
        }
    }
}
