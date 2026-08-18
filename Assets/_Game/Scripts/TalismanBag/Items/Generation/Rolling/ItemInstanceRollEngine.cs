using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Generation.Rolling
{
    public static class ItemInstanceRollEngine
    {
        public static ItemInstanceRollResult Generate(ItemInstanceRollRequest request)
        {
            List<ItemInstanceRollValidationError> errors = new();
            if (request == null)
            {
                return Failure(ItemInstanceRollValidationCodes.RequestNull,
                    "Item instance roll request is null.");
            }

            if (request.identity == null)
            {
                return Failure(ItemInstanceRollValidationCodes.IdentityNull,
                    "Item instance identity is null.");
            }

            ItemInstanceIdentitySnapshot identity = request.identity;
            if (identity.generationVersion != DeterministicItemRandom.SupportedGenerationVersion)
            {
                return Failure(ItemInstanceRollValidationCodes.GenerationVersionUnsupported,
                    $"generationVersion {identity.generationVersion} is unsupported; only version 1 is accepted.");
            }

            if (string.IsNullOrWhiteSpace(request.generationDataStatus))
            {
                return Failure(ItemInstanceRollValidationCodes.GenerationDataStatusEmpty,
                    "generationDataStatus is required.");
            }

            if (!string.Equals(request.generationDataStatus,
                    ItemGenerationDataStatus.QaFixtureCanonical, StringComparison.Ordinal)
                && !string.Equals(request.generationDataStatus,
                    ItemGenerationDataStatus.PlaytestV1Canonical, StringComparison.Ordinal))
            {
                return Failure(ItemInstanceRollValidationCodes.GenerationDataStatusInvalid,
                    "Generation data status must be QA fixture data or the canonical PLAYTEST_V1 catalog.");
            }

            if (!IsIdentityValid(identity))
            {
                return Failure(ItemInstanceRollValidationCodes.FoundationIdentityInvalid,
                    "Identity must use ItemInstanceIdentitySnapshot.v1, a non-empty distinct instance/base identity, an ordinary I001-I030 base, and a supported rarity.");
            }

            if (request.statSchema == null)
            {
                return Failure(ItemInstanceRollValidationCodes.StatSchemaNull, "Stat schema is null.");
            }

            if (!request.statSchema.isValid)
            {
                return Failure(ItemInstanceRollValidationCodes.StatSchemaInvalid,
                    "Stat schema contains validation errors: " + JoinCodes(request.statSchema.ValidationErrors.Select(error => error.code)));
            }

            if (request.affixSchema == null)
            {
                return Failure(ItemInstanceRollValidationCodes.AffixSchemaNull, "Affix schema is null.");
            }

            if (!request.affixSchema.isValid)
            {
                bool weightInvalid = request.affixSchema.ValidationErrors.Any(error =>
                    error.code == ItemAffixPoolAndRangeValidationCodes.DefinedWeightNotPositive
                    || error.code == ItemAffixPoolAndRangeValidationCodes.UnresolvedWeightNonZero
                    || error.code == ItemAffixPoolAndRangeValidationCodes.DefinedPoolHasUnresolvedWeight);
                return Failure(weightInvalid
                        ? ItemInstanceRollValidationCodes.AffixWeightInvalid
                        : ItemInstanceRollValidationCodes.AffixSchemaInvalid,
                    "Affix schema contains validation errors: " + JoinCodes(request.affixSchema.ValidationErrors.Select(error => error.code)));
            }

            if (request.coreBuildSchema == null)
            {
                return Failure(ItemInstanceRollValidationCodes.CoreSchemaNull, "Core/Build schema is null.");
            }

            if (!request.coreBuildSchema.isValid)
            {
                return Failure(ItemInstanceRollValidationCodes.CoreSchemaInvalid,
                    "Core/Build schema contains validation errors: " + JoinCodes(request.coreBuildSchema.ValidationErrors.Select(error => error.code)));
            }

            List<ItemGeneratedStatSnapshot> stats = GenerateStats(identity, request.statSchema, errors);
            if (errors.Count > 0)
            {
                return new ItemInstanceRollResult(null, errors);
            }

            List<ItemGeneratedAffixSnapshot> affixes = GenerateAffixes(identity, request.affixSchema, errors);
            if (errors.Count > 0)
            {
                return new ItemInstanceRollResult(null, errors);
            }

            ItemGeneratedCorePotentialSnapshot core = GenerateCore(identity, request.coreBuildSchema, errors);
            if (errors.Count > 0)
            {
                return new ItemInstanceRollResult(null, errors);
            }

            ItemBuildQualification qualification = GenerateBuildQualification(
                identity, request.coreBuildSchema, request.buildQualificationRollProfile, errors);
            if (errors.Count > 0)
            {
                return new ItemInstanceRollResult(null, errors);
            }

            ItemGeneratedInstanceSnapshot snapshot = new(
                request.generationDataStatus, identity, stats, affixes, core, qualification);
            return new ItemInstanceRollResult(snapshot, Array.Empty<ItemInstanceRollValidationError>());
        }

        public static bool TryGetRangeCardinality(
            long minUnits,
            long maxUnits,
            long stepUnits,
            out ulong cardinality,
            out string validationCode)
        {
            cardinality = 0UL;
            validationCode = ItemInstanceRollValidationCodes.None;
            if (minUnits > maxUnits)
            {
                validationCode = ItemInstanceRollValidationCodes.RangeInvalid;
                return false;
            }

            if (stepUnits <= 0)
            {
                validationCode = ItemInstanceRollValidationCodes.RangeStepInvalid;
                return false;
            }

            ulong span = unchecked((ulong)maxUnits - (ulong)minUnits);
            ulong step = unchecked((ulong)stepUnits);
            if (span % step != 0UL)
            {
                validationCode = ItemInstanceRollValidationCodes.RangeStepInvalid;
                return false;
            }

            ulong quotient = span / step;
            if (quotient == ulong.MaxValue)
            {
                validationCode = ItemInstanceRollValidationCodes.RangeCardinalityUnsupported;
                return false;
            }

            cardinality = quotient + 1UL;
            return cardinality != 0UL;
        }

        private static List<ItemGeneratedStatSnapshot> GenerateStats(
            ItemInstanceIdentitySnapshot identity,
            ItemStatRangeSchemaSnapshot schema,
            List<ItemInstanceRollValidationError> errors)
        {
            ItemStatRangeProfileSnapshot[] profiles = schema.ItemStatProfiles
                .Where(profile => string.Equals(profile.baseItemId, identity.baseItemId, StringComparison.Ordinal))
                .OrderBy(profile => profile.statId, StringComparer.Ordinal)
                .ToArray();
            if (profiles.Length == 0)
            {
                Add(errors, ItemInstanceRollValidationCodes.StatProfileMissing,
                    $"No Defined stat profile exists for baseItemId '{identity.baseItemId}'.");
                return new List<ItemGeneratedStatSnapshot>();
            }

            List<ItemGeneratedStatSnapshot> output = new();
            foreach (ItemStatRangeProfileSnapshot profile in profiles)
            {
                ItemStatQueryResult<ItemStatDefinitionSnapshot> definitionResult =
                    schema.QueryStatDefinition(profile.statId);
                if (!definitionResult.isSuccess || profile.dataMaturity == ItemStatDataMaturity.SCHEMA_ONLY)
                {
                    Add(errors, ItemInstanceRollValidationCodes.StatProfileMissing,
                        $"Stat profile '{profile.baseItemId}@{profile.statId}' is not generation-ready.");
                    return output;
                }

                ItemRarityStatRangeSnapshot range = profile.RarityRanges
                    .FirstOrDefault(candidate => candidate.rarity == identity.rarity);
                if (range == null)
                {
                    Add(errors, ItemInstanceRollValidationCodes.StatRarityRangeMissing,
                        $"Stat profile '{profile.baseItemId}@{profile.statId}' has no '{identity.rarity.ToStableKey()}' range.");
                    return output;
                }

                if (!TryRollRange(identity, new[] { "stat", profile.statId }, range.minUnits, range.maxUnits,
                    definitionResult.value.stepUnits, out long rawUnits, out string code))
                {
                    Add(errors, code, $"Stat range roll failed for '{profile.statId}'.");
                    return output;
                }

                output.Add(new ItemGeneratedStatSnapshot(profile.statId, rawUnits));
            }

            return output;
        }

        private static List<ItemGeneratedAffixSnapshot> GenerateAffixes(
            ItemInstanceIdentitySnapshot identity,
            ItemAffixPoolAndRangeSchemaSnapshot schema,
            List<ItemInstanceRollValidationError> errors)
        {
            ItemAffixQueryResult<ItemAffixGenerationProfileSnapshot> generationResult =
                schema.QueryItemGenerationProfile(identity.baseItemId);
            if (!generationResult.isSuccess
                || generationResult.value.resolutionStatus != ItemAffixResolutionStatus.Defined)
            {
                Add(errors, ItemInstanceRollValidationCodes.AffixGenerationProfileMissing,
                    $"Defined affix generation profile for '{identity.baseItemId}' is missing.");
                return new List<ItemGeneratedAffixSnapshot>();
            }

            ItemAffixGenerationProfileSnapshot generation = generationResult.value;
            ItemAffixQueryResult<ItemAffixSlotPolicySnapshot> slotResult =
                schema.QuerySlotPolicy(generation.slotPolicyId);
            if (!slotResult.isSuccess || slotResult.value.resolutionStatus != ItemAffixResolutionStatus.Defined)
            {
                Add(errors, ItemInstanceRollValidationCodes.AffixSlotPolicyMissing,
                    $"Defined affix slot policy '{generation.slotPolicyId}' is missing.");
                return new List<ItemGeneratedAffixSnapshot>();
            }

            ItemAffixSlotSnapshot[] slots = slotResult.value.Slots
                .Where(slot => slot != null)
                .OrderBy(slot => slot.slotId, StringComparer.Ordinal)
                .ToArray();
            List<ItemGeneratedAffixSnapshot> output = new();
            List<string> selectedAffixIds = new();

            foreach (ItemAffixSlotSnapshot slot in slots.Where(slot => slot.slotKind == ItemAffixSlotKind.Fixed))
            {
                ItemFixedAffixBindingSnapshot binding = generation.FixedAffixBindings
                    .SingleOrDefault(candidate => string.Equals(candidate.slotId, slot.slotId, StringComparison.Ordinal));
                if (binding == null)
                {
                    Add(errors, ItemInstanceRollValidationCodes.AffixSlotPolicyMissing,
                        $"Fixed slot '{slot.slotId}' has no binding.");
                    return output;
                }

                if (!TryGenerateAffixValue(identity, schema, slot.slotId, ItemAffixSlotKind.Fixed,
                    binding.affixId, binding.affixValueProfileId,
                    new[] { "fixed-affix", slot.slotId, binding.affixId,
                        binding.affixValueProfileId, "value" },
                    out ItemGeneratedAffixSnapshot generated, out ItemInstanceRollValidationError error))
                {
                    errors.Add(error);
                    return output;
                }

                output.Add(generated);
                selectedAffixIds.Add(generated.affixId);
            }

            ItemAffixSlotSnapshot[] randomSlots = slots
                .Where(slot => slot.slotKind == ItemAffixSlotKind.Random)
                .ToArray();
            if (randomSlots.Length == 0)
            {
                return output;
            }

            ItemAffixQueryResult<ItemRandomAffixPoolSnapshot> poolResult =
                schema.QueryRandomPool(generation.randomPoolId);
            if (!poolResult.isSuccess || poolResult.value.resolutionStatus != ItemAffixResolutionStatus.Defined)
            {
                Add(errors, ItemInstanceRollValidationCodes.AffixPoolMissing,
                    $"Defined random affix pool '{generation.randomPoolId}' is missing.");
                return output;
            }

            ItemRandomAffixPoolSnapshot pool = poolResult.value;
            if (!TrySumAffixWeights(pool.Entries, out _))
            {
                Add(errors, pool.Entries.Any(entry => entry == null
                        || entry.weightResolutionStatus != ItemAffixWeightResolutionStatus.Defined
                        || entry.weightUnits <= 0)
                        ? ItemInstanceRollValidationCodes.AffixWeightInvalid
                        : ItemInstanceRollValidationCodes.AffixWeightSumOverflow,
                    $"Random affix pool '{pool.poolId}' has invalid or overflowing weights.");
                return output;
            }

            foreach (ItemAffixSlotSnapshot slot in randomSlots)
            {
                ItemAffixPoolEntrySnapshot[] candidates = pool.Entries
                    .Where(entry => entry != null)
                    .Where(entry => pool.repeatPolicy != ItemAffixRepeatPolicy.DisallowDuplicateAffix
                        || !selectedAffixIds.Contains(entry.affixId, StringComparer.Ordinal))
                    .Where(entry => !ConflictsWithSelected(entry.affixId, selectedAffixIds, pool.MutexGroups))
                    .OrderBy(entry => entry.affixId, StringComparer.Ordinal)
                    .ThenBy(entry => entry.affixValueProfileId, StringComparer.Ordinal)
                    .ToArray();
                if (candidates.Length == 0)
                {
                    Add(errors, ItemInstanceRollValidationCodes.AffixCandidateSetEmpty,
                        $"Random slot '{slot.slotId}' has no legal candidate after repeat and mutex filtering.");
                    return output;
                }

                if (!TrySumAffixWeights(candidates, out ulong totalWeight))
                {
                    Add(errors, ItemInstanceRollValidationCodes.AffixWeightSumOverflow,
                        $"Random slot '{slot.slotId}' candidate weight sum overflowed.");
                    return output;
                }

                if (!TryCreateStream(identity,
                    new[] { "random-affix", pool.poolId, slot.slotId, "select" },
                    out DeterministicItemRandom.Stream stream)
                    || !stream.TryNextBounded(totalWeight, out ulong target))
                {
                    Add(errors, ItemInstanceRollValidationCodes.CanonicalDomainInvalid,
                        $"Random slot '{slot.slotId}' selection domain is invalid.");
                    return output;
                }

                ItemAffixPoolEntrySnapshot selected = SelectWeighted(candidates, target);
                if (!TryGenerateAffixValue(identity, schema, slot.slotId, ItemAffixSlotKind.Random,
                    selected.affixId, selected.affixValueProfileId,
                    new[] { "random-affix", pool.poolId, slot.slotId, selected.affixId,
                        selected.affixValueProfileId, "value" },
                    out ItemGeneratedAffixSnapshot generated, out ItemInstanceRollValidationError error))
                {
                    errors.Add(error);
                    return output;
                }

                output.Add(generated);
                selectedAffixIds.Add(generated.affixId);
            }

            return output;
        }

        private static ItemGeneratedCorePotentialSnapshot GenerateCore(
            ItemInstanceIdentitySnapshot identity,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot schema,
            List<ItemInstanceRollValidationError> errors)
        {
            ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> result =
                string.IsNullOrWhiteSpace(identity.cultivationPotentialProfileId)
                    ? schema.QueryCorePotentialProfile(identity.baseItemId, identity.rarity)
                    : schema.QueryCorePotentialProfile(identity.cultivationPotentialProfileId);
            if (!result.isSuccess || result.value.resolutionStatus != ItemCorePotentialResolutionStatus.Defined)
            {
                Add(errors, ItemInstanceRollValidationCodes.CoreProfileMissing,
                    "A unique Defined core potential profile is required.");
                return null;
            }

            ItemCorePotentialProfileSnapshot profile = result.value;
            if (!string.Equals(profile.baseItemId, identity.baseItemId, StringComparison.Ordinal))
            {
                Add(errors, ItemInstanceRollValidationCodes.CoreProfileIdentityMismatch,
                    $"Core profile '{profile.cultivationPotentialProfileId}' belongs to '{profile.baseItemId}', not '{identity.baseItemId}'.");
                return null;
            }

            if (profile.rarity != identity.rarity)
            {
                Add(errors, ItemInstanceRollValidationCodes.CoreProfileRarityMismatch,
                    $"Core profile '{profile.cultivationPotentialProfileId}' rarity does not match the instance.");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(identity.cultivationPotentialProfileId)
                && !string.Equals(profile.cultivationPotentialProfileId,
                    identity.cultivationPotentialProfileId, StringComparison.Ordinal))
            {
                Add(errors, ItemInstanceRollValidationCodes.CoreProfileIdentityMismatch,
                    "Explicit cultivationPotentialProfileId did not resolve exactly.");
                return null;
            }

            return new ItemGeneratedCorePotentialSnapshot(profile.cultivationPotentialProfileId,
                profile.EligibleCoreEffectIds, profile.VisibleCoreEffectIds);
        }

        private static ItemBuildQualification GenerateBuildQualification(
            ItemInstanceIdentitySnapshot identity,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot schema,
            ItemBuildQualificationRollProfile rollProfile,
            List<ItemInstanceRollValidationError> errors)
        {
            ItemCorePotentialQueryResult<ItemBuildQualificationRarityPolicySnapshot> policyResult =
                schema.QueryBuildRarityPolicy(identity.rarity);
            if (!policyResult.isSuccess)
            {
                Add(errors, ItemInstanceRollValidationCodes.BuildPolicyUnresolved,
                    $"Build qualification policy for '{identity.rarity.ToStableKey()}' is missing.");
                return ItemBuildQualification.Unresolved;
            }

            if (identity.rarity == ItemInstanceRarity.White)
            {
                if (policyResult.value.policyMode != ItemBuildQualificationPolicyMode.LockedNone)
                {
                    Add(errors, ItemInstanceRollValidationCodes.BuildPolicyUnresolved,
                        "White rarity Build qualification policy must be LockedNone.");
                    return ItemBuildQualification.Unresolved;
                }

                return ItemBuildQualification.None;
            }

            if (rollProfile == null)
            {
                Add(errors, ItemInstanceRollValidationCodes.BuildRollProfileMissing,
                    "Higher rarity generation requires an explicit Build qualification roll profile.");
                return ItemBuildQualification.Unresolved;
            }

            if (rollProfile.rarity != identity.rarity)
            {
                Add(errors, ItemInstanceRollValidationCodes.BuildRollProfileRarityMismatch,
                    $"Build roll profile '{rollProfile.profileId}' rarity does not match the instance.");
                return ItemBuildQualification.Unresolved;
            }

            bool hasNullEntry = rollProfile.Entries.Any(entry => entry == null);
            ItemBuildQualificationRollEntry[] nonNullEntries = rollProfile.Entries
                .Where(entry => entry != null)
                .ToArray();
            if (string.IsNullOrWhiteSpace(rollProfile.profileId)
                || (!string.Equals(rollProfile.dataMaturityKey,
                        ItemGenerationDataStatus.QaFixtureOnly, StringComparison.Ordinal)
                    && !string.Equals(rollProfile.dataMaturityKey,
                        ItemGenerationDataStatus.PlaytestV1Canonical, StringComparison.Ordinal))
                || rollProfile.Entries.Count == 0
                || hasNullEntry
                || nonNullEntries.GroupBy(entry => entry.qualification).Any(group => group.Count() > 1)
                || nonNullEntries.Any(entry => !Enum.IsDefined(typeof(ItemBuildQualification), entry.qualification)
                    || entry.qualification == ItemBuildQualification.Unresolved
                    || entry.weightUnits <= 0))
            {
                Add(errors, ItemInstanceRollValidationCodes.BuildRollEntryInvalid,
                    $"Build roll profile '{rollProfile.profileId}' contains invalid identity, maturity, result, duplicate, or weight data.");
                return ItemBuildQualification.Unresolved;
            }

            if (!TrySumBuildWeights(rollProfile.Entries, out ulong totalWeight))
            {
                Add(errors, ItemInstanceRollValidationCodes.BuildWeightSumOverflow,
                    $"Build roll profile '{rollProfile.profileId}' weight sum overflowed.");
                return ItemBuildQualification.Unresolved;
            }

            if (!TryCreateStream(identity,
                new[] { "build", "qualification", rollProfile.profileId },
                out DeterministicItemRandom.Stream stream)
                || !stream.TryNextBounded(totalWeight, out ulong target))
            {
                Add(errors, ItemInstanceRollValidationCodes.CanonicalDomainInvalid,
                    "Build qualification domain is invalid.");
                return ItemBuildQualification.Unresolved;
            }

            ulong cursor = 0UL;
            foreach (ItemBuildQualificationRollEntry entry in rollProfile.Entries)
            {
                cursor += unchecked((ulong)entry.weightUnits);
                if (target < cursor)
                {
                    return entry.qualification;
                }
            }

            Add(errors, ItemInstanceRollValidationCodes.BuildRollEntryInvalid,
                "Build qualification selection did not resolve to a profile entry.");
            return ItemBuildQualification.Unresolved;
        }

        private static bool TryGenerateAffixValue(
            ItemInstanceIdentitySnapshot identity,
            ItemAffixPoolAndRangeSchemaSnapshot schema,
            string slotId,
            ItemAffixSlotKind slotKind,
            string affixId,
            string valueProfileId,
            IReadOnlyList<string> domainSegments,
            out ItemGeneratedAffixSnapshot generated,
            out ItemInstanceRollValidationError error)
        {
            generated = null;
            error = null;
            ItemAffixQueryResult<ItemAffixValueProfileSnapshot> valueResult =
                schema.QueryAffixValueProfile(valueProfileId);
            if (!valueResult.isSuccess
                || valueResult.value.resolutionStatus != ItemAffixResolutionStatus.Defined
                || !string.Equals(valueResult.value.affixId, affixId, StringComparison.Ordinal))
            {
                error = Error(ItemInstanceRollValidationCodes.AffixValueProfileMissing,
                    $"Defined affix value profile '{valueProfileId}' for '{affixId}' is missing or mismatched.");
                return false;
            }

            ItemAffixValueProfileSnapshot profile = valueResult.value;
            ItemAffixRarityValueRangeSnapshot range = profile.RarityRanges
                .FirstOrDefault(candidate => candidate.rarity == identity.rarity);
            if (range == null)
            {
                error = Error(ItemInstanceRollValidationCodes.AffixRarityRangeMissing,
                    $"Affix value profile '{valueProfileId}' has no '{identity.rarity.ToStableKey()}' range.");
                return false;
            }

            if (!TryRollRange(identity, domainSegments, range.minUnits, range.maxUnits, profile.stepUnits,
                out long rawUnits, out string code))
            {
                error = Error(code, $"Affix value range roll failed for '{affixId}'.");
                return false;
            }

            generated = new ItemGeneratedAffixSnapshot(slotId, slotKind, affixId, valueProfileId, rawUnits);
            return true;
        }

        private static bool TryRollRange(
            ItemInstanceIdentitySnapshot identity,
            IReadOnlyList<string> domainSegments,
            long minUnits,
            long maxUnits,
            long stepUnits,
            out long rawUnits,
            out string validationCode)
        {
            rawUnits = 0L;
            if (!TryGetRangeCardinality(minUnits, maxUnits, stepUnits,
                out ulong cardinality, out validationCode))
            {
                return false;
            }

            if (!TryCreateStream(identity, domainSegments, out DeterministicItemRandom.Stream stream)
                || !stream.TryNextBounded(cardinality, out ulong index))
            {
                validationCode = ItemInstanceRollValidationCodes.CanonicalDomainInvalid;
                return false;
            }

            ulong offset = index * unchecked((ulong)stepUnits);
            rawUnits = unchecked((long)(unchecked((ulong)minUnits + offset)));
            return true;
        }

        private static bool TryCreateStream(
            ItemInstanceIdentitySnapshot identity,
            IReadOnlyList<string> domainSegments,
            out DeterministicItemRandom.Stream stream)
        {
            stream = null;
            if (!DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed, identity.generationVersion,
                identity.itemInstanceId, identity.baseItemId, identity.rarity, domainSegments, out ulong seed))
            {
                return false;
            }

            stream = new DeterministicItemRandom.Stream(seed);
            return true;
        }

        private static bool TrySumAffixWeights(
            IEnumerable<ItemAffixPoolEntrySnapshot> entries,
            out ulong sum)
        {
            sum = 0UL;
            foreach (ItemAffixPoolEntrySnapshot entry in entries ?? Array.Empty<ItemAffixPoolEntrySnapshot>())
            {
                if (entry == null
                    || entry.weightResolutionStatus != ItemAffixWeightResolutionStatus.Defined
                    || entry.weightUnits <= 0
                    || !TryAddPositiveWeight(ref sum, entry.weightUnits))
                {
                    return false;
                }
            }

            return sum > 0UL;
        }

        private static bool TrySumBuildWeights(
            IEnumerable<ItemBuildQualificationRollEntry> entries,
            out ulong sum)
        {
            sum = 0UL;
            foreach (ItemBuildQualificationRollEntry entry in entries ?? Array.Empty<ItemBuildQualificationRollEntry>())
            {
                if (entry == null || entry.weightUnits <= 0 || !TryAddPositiveWeight(ref sum, entry.weightUnits))
                {
                    return false;
                }
            }

            return sum > 0UL;
        }

        private static bool TryAddPositiveWeight(ref ulong sum, long weightUnits)
        {
            ulong weight = unchecked((ulong)weightUnits);
            if (ulong.MaxValue - sum < weight)
            {
                return false;
            }

            sum += weight;
            return true;
        }

        private static ItemAffixPoolEntrySnapshot SelectWeighted(
            IReadOnlyList<ItemAffixPoolEntrySnapshot> candidates,
            ulong target)
        {
            ulong cursor = 0UL;
            for (int index = 0; index < candidates.Count; index++)
            {
                cursor += unchecked((ulong)candidates[index].weightUnits);
                if (target < cursor)
                {
                    return candidates[index];
                }
            }

            return candidates[candidates.Count - 1];
        }

        private static bool ConflictsWithSelected(
            string candidateAffixId,
            IReadOnlyCollection<string> selectedAffixIds,
            IEnumerable<ItemAffixMutexGroupSnapshot> groups)
        {
            foreach (ItemAffixMutexGroupSnapshot group in groups ?? Array.Empty<ItemAffixMutexGroupSnapshot>())
            {
                if (group != null
                    && group.AffixIds.Contains(candidateAffixId, StringComparer.Ordinal)
                    && group.AffixIds.Any(member => selectedAffixIds.Contains(member)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsIdentityValid(ItemInstanceIdentitySnapshot identity)
        {
            return string.Equals(identity.schemaId, ItemInstanceIdentitySnapshot.CurrentSchemaId, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(identity.itemInstanceId)
                && !string.IsNullOrWhiteSpace(identity.baseItemId)
                && !string.Equals(identity.itemInstanceId, identity.baseItemId, StringComparison.Ordinal)
                && ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(identity.baseItemId)
                && ItemInstanceRarityCatalog.TryGetDefinition(identity.rarity, out _);
        }

        private static string JoinCodes(IEnumerable<string> codes)
        {
            return string.Join("|", (codes ?? Array.Empty<string>())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(code => code, StringComparer.Ordinal));
        }

        private static ItemInstanceRollResult Failure(string code, string message)
        {
            return new ItemInstanceRollResult(null, new[] { Error(code, message) });
        }

        private static void Add(List<ItemInstanceRollValidationError> errors, string code, string message)
        {
            errors.Add(Error(code, message));
        }

        private static ItemInstanceRollValidationError Error(string code, string message)
        {
            return new ItemInstanceRollValidationError(code, message);
        }
    }
}
