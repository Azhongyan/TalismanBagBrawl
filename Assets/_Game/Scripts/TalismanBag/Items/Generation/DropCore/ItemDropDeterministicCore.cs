using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Generation.DropCore
{
    public static class ItemDropDeterministicCore
    {
        public static ItemDropDeterministicCoreResult Generate(
            ItemDropDeterministicCoreRequest request)
        {
            try
            {
                return GenerateValidated(request);
            }
            catch (Exception)
            {
                return Failure(
                    ItemDropCoreValidationCodes.InstanceRollFailed,
                    "Core generation stopped after a validation failure; no partial snapshot was returned.");
            }
        }

        private static ItemDropDeterministicCoreResult GenerateValidated(
            ItemDropDeterministicCoreRequest request)
        {
            ItemDropDeterministicCoreResult requestValidation = ValidateRequest(request);
            if (requestValidation != null)
            {
                return requestValidation;
            }

            ItemDropDeterministicCoreResult candidateValidation = ValidateCandidates(
                request,
                out ItemDropCoreCandidateEntry[] candidates,
                out ulong candidateWeightTotal);
            if (candidateValidation != null)
            {
                return candidateValidation;
            }

            ItemDropDeterministicCoreResult rarityValidation = ValidateRarityTiers(
                request,
                out ItemDropRarityTierEntry[] rarityTiers,
                out ulong rarityWeightTotal);
            if (rarityValidation != null)
            {
                return rarityValidation;
            }

            if (!ItemDropDeterministicDomain.TryCreateStream(
                    request.rootSeed,
                    request.generationVersion,
                    request.requestId,
                    request.sourceIdentity.sourceContextId,
                    request.sourceIdentity.sourceKey,
                    request.sourceIdentity.sourceOrdinal,
                    new[] { "drop", "candidate", request.candidatePoolId },
                    out DeterministicItemRandom.Stream candidateStream)
                || !candidateStream.TryNextBounded(candidateWeightTotal, out ulong candidateTarget))
            {
                return Failure(
                    ItemDropCoreValidationCodes.DropDomainInvalid,
                    "Candidate selection domain is invalid.");
            }

            string selectedBaseItemId = SelectCandidate(candidates, candidateTarget).baseItemId;
            ItemInstanceRarity selectedRarity;
            if (request.rarityTierProfile.mode == ItemDropRarityTierMode.Fixed)
            {
                selectedRarity = rarityTiers[0].rarity;
            }
            else
            {
                if (!ItemDropDeterministicDomain.TryCreateStream(
                        request.rootSeed,
                        request.generationVersion,
                        request.requestId,
                        request.sourceIdentity.sourceContextId,
                        request.sourceIdentity.sourceKey,
                        request.sourceIdentity.sourceOrdinal,
                        new[] { "drop", "rarity", request.rarityTierProfileId },
                        out DeterministicItemRandom.Stream rarityStream)
                    || !rarityStream.TryNextBounded(rarityWeightTotal, out ulong rarityTarget))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.DropDomainInvalid,
                        "Rarity-tier selection domain is invalid.");
                }

                selectedRarity = SelectRarity(rarityTiers, rarityTarget).rarity;
            }

            ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> coreResult =
                request.coreBuildSchema.QueryCorePotentialProfile(selectedBaseItemId, selectedRarity);
            if (!coreResult.isSuccess
                || coreResult.value.resolutionStatus != ItemCorePotentialResolutionStatus.Defined)
            {
                return Failure(
                    ItemDropCoreValidationCodes.CoreProfileMissing,
                    $"A Defined core profile is required for '{selectedBaseItemId}@{selectedRarity.ToStableKey()}'.");
            }

            ItemBuildQualificationRollProfile buildProfile = null;
            if (selectedRarity != ItemInstanceRarity.White)
            {
                ItemBuildQualificationRollProfile[] matches =
                    request.BuildQualificationRollProfiles
                        .Where(profile => profile != null && profile.rarity == selectedRarity)
                        .ToArray();
                if (matches.Length == 0)
                {
                    return Failure(
                        ItemDropCoreValidationCodes.BuildRollProfileMissing,
                        $"A Build roll profile is required for '{selectedRarity.ToStableKey()}'.");
                }

                if (matches.Length > 1)
                {
                    return Failure(
                        ItemDropCoreValidationCodes.BuildRollProfileDuplicate,
                        $"More than one Build roll profile matches '{selectedRarity.ToStableKey()}'.");
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
                return Failure(
                    ItemDropCoreValidationCodes.IdentityCreationFailed,
                    "Item instance identity creation failed: "
                    + identityResult.primaryValidationCode
                    + ".");
            }

            ItemInstanceRollResult rollResult = ItemInstanceRollEngine.Generate(
                new ItemInstanceRollRequest(
                    identityResult.snapshot,
                    request.statSchema,
                    request.affixSchema,
                    request.coreBuildSchema,
                    buildProfile,
                    ItemGenerationDataStatus.QaFixtureCanonical));
            if (!rollResult.isSuccess)
            {
                return Failure(
                    ItemDropCoreValidationCodes.InstanceRollFailed,
                    "Item instance roll failed: " + rollResult.primaryValidationCode + ".");
            }

            ItemDropDeterministicCoreSnapshot snapshot = new(
                request,
                request.candidatePool.BuildCanonicalSignature(),
                request.rarityTierProfile.BuildCanonicalSignature(),
                selectedBaseItemId,
                selectedRarity,
                rollResult.snapshot);
            return new ItemDropDeterministicCoreResult(
                snapshot,
                Array.Empty<ItemDropCoreValidationError>());
        }

        private static ItemDropDeterministicCoreResult ValidateRequest(
            ItemDropDeterministicCoreRequest request)
        {
            if (request == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.RequestNull,
                    "Core request is null.");
            }

            if (string.IsNullOrWhiteSpace(request.requestId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RequestIdEmpty,
                    "requestId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.itemInstanceId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.ItemInstanceIdEmpty,
                    "itemInstanceId is required.");
            }

            if (request.generationVersion
                != ItemDropDeterministicDomain.SupportedGenerationVersion)
            {
                return Failure(
                    ItemDropCoreValidationCodes.GenerationVersionUnsupported,
                    "Only generationVersion 1 is supported.");
            }

            if (!string.Equals(
                    request.dataStatus,
                    ItemDropCoreDataStatus.DevOnlyCandidate,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropCoreValidationCodes.DataStatusInvalid,
                    "Core execution requires the DEV-only candidate identity.");
            }

            if (request.sourceIdentity == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.SourceIdentityNull,
                    "Source identity is null.");
            }

            if (string.IsNullOrWhiteSpace(request.sourceIdentity.sourceContextId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.SourceContextIdEmpty,
                    "sourceContextId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.sourceIdentity.sourceKey))
            {
                return Failure(
                    ItemDropCoreValidationCodes.SourceKeyEmpty,
                    "sourceKey is required.");
            }

            if (request.sourceIdentity.sourceOrdinal <= 0)
            {
                return Failure(
                    ItemDropCoreValidationCodes.SourceOrdinalInvalid,
                    "sourceOrdinal must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.candidatePoolId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.CandidatePoolIdentityEmpty,
                    "candidatePoolId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.rarityTierProfileId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierProfileIdentityEmpty,
                    "rarityTierProfileId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.attributeRollBandProfileId))
            {
                return Failure(
                    ItemDropCoreValidationCodes.AttributeRollBandProfileIdentityEmpty,
                    "attributeRollBandProfileId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.rollPolicyVersion))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RollPolicyVersionEmpty,
                    "rollPolicyVersion is required.");
            }

            if (!string.Equals(
                    request.rollPolicyVersion,
                    DeterministicItemRandom.AlgorithmId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RollPolicyVersionUnsupported,
                    "The supplied roll policy version is unsupported.");
            }

            if (request.foundation == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.FoundationNull,
                    "Item foundation is null.");
            }

            if (!request.foundation.isValid)
            {
                return Failure(
                    ItemDropCoreValidationCodes.FoundationInvalid,
                    "Item foundation contains validation errors.");
            }

            if (request.statSchema == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.StatSchemaMissing,
                    "Stat schema is required.");
            }

            if (request.affixSchema == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.AffixSchemaMissing,
                    "Affix schema is required.");
            }

            if (request.coreBuildSchema == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.CoreSchemaMissing,
                    "Core/Build schema is required.");
            }

            return null;
        }

        private static ItemDropDeterministicCoreResult ValidateCandidates(
            ItemDropDeterministicCoreRequest request,
            out ItemDropCoreCandidateEntry[] candidates,
            out ulong totalWeight)
        {
            candidates = Array.Empty<ItemDropCoreCandidateEntry>();
            totalWeight = 0UL;
            ItemDropCoreCandidatePool pool = request.candidatePool;
            if (pool == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.CandidatePoolNull,
                    "Candidate pool is null.");
            }

            if (string.IsNullOrWhiteSpace(pool.candidatePoolId)
                || !string.Equals(
                    pool.candidatePoolId,
                    request.candidatePoolId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropCoreValidationCodes.CandidatePoolIdMismatch,
                    "Candidate pool identity must match the request.");
            }

            if (pool.Candidates.Count == 0)
            {
                return Failure(
                    ItemDropCoreValidationCodes.CandidatePoolEmpty,
                    "Candidate pool must not be empty.");
            }

            foreach (ItemDropCoreCandidateEntry candidate in pool.Candidates)
            {
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.baseItemId))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.CandidateEntryInvalid,
                        "Every candidate requires a baseItemId.");
                }

                if (candidate.candidateKind
                    != ItemDropCoreCandidateKind.OrdinaryGeneratedItem)
                {
                    return Failure(
                        ItemDropCoreValidationCodes.CandidateKindInvalid,
                        "Only ordinary generated candidates are accepted.");
                }

                if (string.Equals(
                        candidate.baseItemId,
                        ItemRarityInstanceFoundation.CoreProgressionBaseItemId,
                        StringComparison.Ordinal))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.I031Forbidden,
                        "I031 is excluded from ordinary candidate selection.");
                }

                if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(candidate.baseItemId))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.CandidateBaseItemUnknown,
                        $"Unknown ordinary candidate '{candidate.baseItemId}'.");
                }

                if (candidate.weightUnits <= 0)
                {
                    return Failure(
                        ItemDropCoreValidationCodes.CandidateWeightInvalid,
                        $"Candidate '{candidate.baseItemId}' weightUnits must be positive.");
                }
            }

            if (pool.Candidates
                .Where(candidate => candidate != null)
                .GroupBy(candidate => candidate.baseItemId, StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
            {
                return Failure(
                    ItemDropCoreValidationCodes.CandidateDuplicate,
                    "Candidate baseItemId values must be unique.");
            }

            candidates = pool.Candidates
                .OrderBy(candidate => candidate.baseItemId, StringComparer.Ordinal)
                .ToArray();
            foreach (ItemDropCoreCandidateEntry candidate in candidates)
            {
                if (!TryAddWeight(ref totalWeight, candidate.weightUnits))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.CandidateWeightSumOverflow,
                        "Candidate weight sum exceeds ulong capacity.");
                }
            }

            return null;
        }

        private static ItemDropDeterministicCoreResult ValidateRarityTiers(
            ItemDropDeterministicCoreRequest request,
            out ItemDropRarityTierEntry[] entries,
            out ulong totalWeight)
        {
            entries = Array.Empty<ItemDropRarityTierEntry>();
            totalWeight = 0UL;
            ItemDropRarityTierProfile profile = request.rarityTierProfile;
            if (profile == null)
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierProfileNull,
                    "Rarity-tier profile is null.");
            }

            if (string.IsNullOrWhiteSpace(profile.rarityTierProfileId)
                || !string.Equals(
                    profile.rarityTierProfileId,
                    request.rarityTierProfileId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierProfileIdMismatch,
                    "Rarity-tier profile identity must match the request.");
            }

            if (profile.mode != ItemDropRarityTierMode.Fixed
                && profile.mode != ItemDropRarityTierMode.Weighted)
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierModeInvalid,
                    "Rarity-tier mode must be Fixed or Weighted.");
            }

            if (profile.Entries.Count == 0)
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierProfileEmpty,
                    "Rarity-tier profile must not be empty.");
            }

            if (profile.mode == ItemDropRarityTierMode.Fixed
                && profile.Entries.Count != 1)
            {
                return Failure(
                    ItemDropCoreValidationCodes.FixedRarityTierEntryCountInvalid,
                    "A Fixed rarity-tier profile must contain exactly one entry.");
            }

            foreach (ItemDropRarityTierEntry entry in profile.Entries)
            {
                if (entry == null
                    || !ItemInstanceRarityCatalog.TryGetDefinition(entry.rarity, out _))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.RarityTierEntryInvalid,
                        "Every rarity-tier entry must use a supported rarity.");
                }

                if (entry.weightUnits <= 0)
                {
                    return Failure(
                        ItemDropCoreValidationCodes.RarityTierWeightInvalid,
                        "Rarity-tier weightUnits must be positive.");
                }
            }

            if (profile.Entries
                .Where(entry => entry != null)
                .GroupBy(entry => entry.rarity)
                .Any(group => group.Count() > 1))
            {
                return Failure(
                    ItemDropCoreValidationCodes.RarityTierDuplicate,
                    "Rarity-tier entries must be unique.");
            }

            entries = profile.Entries
                .OrderBy(entry => entry.rarity.ToTierIndex())
                .ThenBy(entry => (int)entry.rarity)
                .ToArray();
            foreach (ItemDropRarityTierEntry entry in entries)
            {
                if (!TryAddWeight(ref totalWeight, entry.weightUnits))
                {
                    return Failure(
                        ItemDropCoreValidationCodes.RarityTierWeightSumOverflow,
                        "Rarity-tier weight sum exceeds ulong capacity.");
                }
            }

            return null;
        }

        private static ItemDropCoreCandidateEntry SelectCandidate(
            IReadOnlyList<ItemDropCoreCandidateEntry> entries,
            ulong target)
        {
            ulong cursor = 0UL;
            foreach (ItemDropCoreCandidateEntry entry in entries)
            {
                cursor += unchecked((ulong)entry.weightUnits);
                if (target < cursor)
                {
                    return entry;
                }
            }

            return entries[entries.Count - 1];
        }

        private static ItemDropRarityTierEntry SelectRarity(
            IReadOnlyList<ItemDropRarityTierEntry> entries,
            ulong target)
        {
            ulong cursor = 0UL;
            foreach (ItemDropRarityTierEntry entry in entries)
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

        private static ItemDropDeterministicCoreResult Failure(
            string code,
            string message)
        {
            return new ItemDropDeterministicCoreResult(
                null,
                new[] { new ItemDropCoreValidationError(code, message) });
        }
    }
}
