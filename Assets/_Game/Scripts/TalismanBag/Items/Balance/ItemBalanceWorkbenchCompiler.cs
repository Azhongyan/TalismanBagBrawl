using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.DropSandbox;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Balance
{
    public sealed class ItemBalanceCompiledData
    {
        internal ItemBalanceCompiledData(
            ItemGenerationFoundationSnapshot foundation,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema,
            IEnumerable<ItemBuildQualificationRollProfile> buildProfiles,
            IEnumerable<ItemDropRarityWeightProfileSnapshot> dropProfiles)
        {
            Foundation = foundation;
            StatSchema = statSchema;
            AffixSchema = affixSchema;
            CoreBuildSchema = coreBuildSchema;
            BuildProfiles = Array.AsReadOnly((buildProfiles ?? Array.Empty<ItemBuildQualificationRollProfile>())
                .OrderBy(value => value.rarity.ToTierIndex()).ToArray());
            DropProfiles = Array.AsReadOnly((dropProfiles ?? Array.Empty<ItemDropRarityWeightProfileSnapshot>())
                .ToArray());
        }

        public ItemGenerationFoundationSnapshot Foundation { get; }
        public ItemStatRangeSchemaSnapshot StatSchema { get; }
        public ItemAffixPoolAndRangeSchemaSnapshot AffixSchema { get; }
        public ItemCorePotentialAndBuildEligibilitySchemaSnapshot CoreBuildSchema { get; }
        public IReadOnlyList<ItemBuildQualificationRollProfile> BuildProfiles { get; }
        public IReadOnlyList<ItemDropRarityWeightProfileSnapshot> DropProfiles { get; }

        public ItemBuildQualificationRollProfile FindBuildProfile(ItemInstanceRarity rarity)
        {
            return BuildProfiles.FirstOrDefault(value => value.rarity == rarity);
        }
    }

    public sealed class ItemBalancePreviewDimension
    {
        public ItemBalancePreviewDimension(string dimensionId, double value, IEnumerable<string> contributors)
        {
            this.dimensionId = dimensionId ?? string.Empty;
            this.value = value;
            Contributors = Array.AsReadOnly((contributors ?? Array.Empty<string>()).ToArray());
        }

        public string dimensionId { get; }
        public double value { get; }
        public IReadOnlyList<string> Contributors { get; }
    }

    public sealed class ItemBalancePreviewResult
    {
        internal ItemBalancePreviewResult(
            ItemInstanceRollResult rollResult,
            ItemInstanceProjectionResult projectionResult,
            IEnumerable<ItemBalancePreviewDimension> dimensions,
            IEnumerable<string> errors)
        {
            RollResult = rollResult;
            ProjectionResult = projectionResult;
            Dimensions = Array.AsReadOnly((dimensions ?? Array.Empty<ItemBalancePreviewDimension>()).ToArray());
            Errors = Array.AsReadOnly((errors ?? Array.Empty<string>()).ToArray());
        }

        public bool isSuccess => Errors.Count == 0 && RollResult?.isSuccess == true
            && ProjectionResult?.isSuccess == true;
        public ItemInstanceRollResult RollResult { get; }
        public ItemInstanceProjectionResult ProjectionResult { get; }
        public IReadOnlyList<ItemBalancePreviewDimension> Dimensions { get; }
        public IReadOnlyList<string> Errors { get; }
    }

    public static class ItemBalanceWorkbenchCompiler
    {
        private const string SlotPolicyId = "candidate_slot_policy_1_fixed_1_random";

        public static ItemBalanceCompiledData Compile(ItemBalanceWorkbenchCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            ItemGenerationFoundationSnapshot foundation = ItemRarityInstanceFoundation.Create();
            ItemStatRangeSchemaSnapshot statSchema = CompileStats(catalog);
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema = CompileAffixes(catalog);
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreSchema = CompileCoreBuild(catalog);
            ItemBuildQualificationRollProfile[] buildProfiles = CompileBuildProfiles(catalog).ToArray();
            ItemDropRarityWeightProfileSnapshot[] dropProfiles = CompileDropProfiles(catalog).ToArray();
            return new ItemBalanceCompiledData(foundation, statSchema, affixSchema, coreSchema,
                buildProfiles, dropProfiles);
        }

        public static ItemBalancePreviewResult Preview(
            ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceCompiledData compiled,
            string baseItemId,
            ItemInstanceRarity rarity,
            long rootSeed)
        {
            List<string> errors = new();
            if (catalog == null || compiled == null)
            {
                errors.Add("Catalog or compiled data is missing.");
                return new ItemBalancePreviewResult(null, null, Array.Empty<ItemBalancePreviewDimension>(), errors);
            }

            ItemBalanceProfile profile = catalog.FindProfile(baseItemId);
            ItemBalanceRarityVersion version = profile?.FindVersion(rarity);
            if (profile == null || version == null)
            {
                errors.Add($"Candidate version '{baseItemId}@{rarity.ToStableKey()}' is missing.");
                return new ItemBalancePreviewResult(null, null, Array.Empty<ItemBalancePreviewDimension>(), errors);
            }

            string itemInstanceId = string.Join("_", "wb", baseItemId.ToLowerInvariant(),
                rarity.ToStableKey(), rootSeed.ToString(CultureInfo.InvariantCulture).Replace('-', 'n'));
            ItemInstanceIdentityCreationResult identity = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                compiled.Foundation, itemInstanceId, baseItemId, rarity,
                DeterministicItemRandom.SupportedGenerationVersion, rootSeed,
                version.cultivationPotentialProfileId);
            if (!identity.isValid)
            {
                errors.AddRange(identity.ValidationErrors.Select(value => value.code + ": " + value.message));
                return new ItemBalancePreviewResult(null, null, Array.Empty<ItemBalancePreviewDimension>(), errors);
            }

            ItemBuildQualificationRollProfile buildProfile = rarity == ItemInstanceRarity.White
                ? null
                : compiled.FindBuildProfile(rarity);
            ItemInstanceRollRequest request = new(
                identity.snapshot,
                compiled.StatSchema,
                compiled.AffixSchema,
                compiled.CoreBuildSchema,
                buildProfile,
                ItemGenerationDataStatus.QaFixtureCanonical);
            ItemInstanceRollResult roll = ItemInstanceRollEngine.Generate(request);
            if (!roll.isSuccess)
            {
                errors.AddRange(roll.ValidationErrors.Select(value => value.code + ": " + value.message));
                return new ItemBalancePreviewResult(roll, null, Array.Empty<ItemBalancePreviewDimension>(), errors);
            }

            ItemInstanceProjectionResult projection = ItemInstanceProjectionProvider.Project(
                new ItemInstanceProjectionRequest(roll.snapshot));
            if (!projection.isSuccess)
            {
                errors.AddRange(projection.ValidationErrors.Select(value => value.code + ": " + value.message));
            }

            return new ItemBalancePreviewResult(roll, projection,
                CompileDimensions(catalog, roll.snapshot), errors);
        }

        private static ItemStatRangeSchemaSnapshot CompileStats(ItemBalanceWorkbenchCatalog catalog)
        {
            ItemStatDefinitionSnapshot[] definitions = catalog.statDefinitions.Where(value => value != null)
                .Select(value => new ItemStatDefinitionSnapshot(value.statId, value.displayName, value.unitKey,
                    value.direction, value.decimalPlaces, value.stepUnits, value.roundingMode,
                    ItemStatDataMaturity.BALANCE_CANDIDATE))
                .ToArray();
            List<ItemStatRangeProfileSnapshot> profiles = new();
            foreach (ItemBalanceProfile profile in catalog.profiles.Where(value => value != null))
            {
                foreach (string statId in profile.rarityVersions.Where(value => value != null)
                    .SelectMany(value => value.statRanges ?? new List<ItemBalanceRange>())
                    .Where(value => value != null)
                    .Select(value => value.statId)
                    .Distinct(StringComparer.Ordinal))
                {
                    List<ItemRarityStatRangeSnapshot> ranges = new();
                    foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                    {
                        ItemBalanceRange source = profile.FindVersion(rarity.rarity)?.FindRange(statId);
                        if (source != null)
                            ranges.Add(new ItemRarityStatRangeSnapshot(rarity.rarity, source.minUnits, source.maxUnits));
                    }

                    if (ranges.Count == 0) continue;
                    profiles.Add(new ItemStatRangeProfileSnapshot(profile.baseItemId, statId,
                        ItemStatDataMaturity.BALANCE_CANDIDATE,
                        new ItemNumericRangeSnapshot(ranges.Min(value => value.minUnits),
                            ranges.Max(value => value.maxUnits)), ranges));
                }
            }

            return ItemStatRangeSchema.Create(definitions, profiles);
        }

        private static ItemAffixPoolAndRangeSchemaSnapshot CompileAffixes(ItemBalanceWorkbenchCatalog catalog)
        {
            ItemAffixDefinitionSnapshot[] definitions = catalog.affixDefinitions.Where(value => value != null)
                .Select(value => new ItemAffixDefinitionSnapshot(value.affixId, value.displayName, value.unitKey,
                    value.direction, value.decimalPlaces, value.stepUnits, value.roundingMode,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate)).ToArray();
            ItemAffixValueProfileSnapshot[] values = catalog.affixDefinitions.Where(value => value != null)
                .Select(value =>
                {
                    ItemAffixRarityValueRangeSnapshot[] ranges = value.rarityRanges.Where(range => range != null)
                        .Select(range => new ItemAffixRarityValueRangeSnapshot(range.rarity,
                            range.minUnits, range.maxUnits)).ToArray();
                    return new ItemAffixValueProfileSnapshot("candidate_value_" + value.affixId,
                        value.affixId, ItemAffixResolutionStatus.Defined,
                        ItemBalanceWorkbenchCatalog.BalanceCandidate,
                        new ItemAffixNumericRangeSnapshot(ranges.Min(range => range.minUnits),
                            ranges.Max(range => range.maxUnits)), ranges, value.direction,
                        value.decimalPlaces, value.stepUnits, value.roundingMode);
                }).ToArray();
            ItemAffixSlotPolicySnapshot[] slots =
            {
                new(SlotPolicyId, ItemAffixResolutionStatus.Defined,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate,
                    new[]
                    {
                        new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed),
                        new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random)
                    })
            };
            List<ItemRandomAffixPoolSnapshot> pools = new();
            List<ItemAffixGenerationProfileSnapshot> generation = new();
            foreach (ItemBalanceProfile profile in catalog.profiles.Where(value => value != null))
            {
                pools.Add(new ItemRandomAffixPoolSnapshot(profile.randomPoolId,
                    ItemAffixResolutionStatus.Defined, ItemBalanceWorkbenchCatalog.BalanceCandidate,
                    ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                    profile.randomAffixes.Where(value => value != null && value.weight > 0)
                        .Select(value => new ItemAffixPoolEntrySnapshot(value.affixId,
                            "candidate_value_" + value.affixId,
                            ItemAffixWeightResolutionStatus.Defined, value.weight)),
                    Array.Empty<ItemAffixMutexGroupSnapshot>()));
                generation.Add(new ItemAffixGenerationProfileSnapshot(profile.baseItemId,
                    ItemAffixResolutionStatus.Defined, ItemBalanceWorkbenchCatalog.BalanceCandidate,
                    SlotPolicyId,
                    new[] { new ItemFixedAffixBindingSnapshot("fixed_01", profile.fixedAffixId,
                        "candidate_value_" + profile.fixedAffixId) }, profile.randomPoolId));
            }

            return ItemAffixPoolAndRangeSchema.Create(definitions, values, slots, pools, generation);
        }

        private static ItemCorePotentialAndBuildEligibilitySchemaSnapshot CompileCoreBuild(
            ItemBalanceWorkbenchCatalog catalog)
        {
            ItemCoreRarityPolicySnapshot[] corePolicies = ItemInstanceRarityCatalog.All.Select(value =>
                new ItemCoreRarityPolicySnapshot(value.rarity, ItemCorePotentialResolutionStatus.Defined,
                    value.rarity == ItemInstanceRarity.Orange,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate)).ToArray();
            List<ItemCorePotentialProfileSnapshot> profiles = new();
            foreach (ItemBalanceProfile item in catalog.profiles.Where(value => value != null))
            {
                foreach (ItemBalanceRarityVersion version in item.rarityVersions.Where(value => value != null))
                {
                    Dictionary<string, ItemBalanceCoreCandidate> candidates = item.coreCandidates
                        .Where(value => value != null)
                        .GroupBy(value => value.coreEffectId, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
                    profiles.Add(new ItemCorePotentialProfileSnapshot(
                        version.cultivationPotentialProfileId, item.baseItemId, version.rarity,
                        ItemCorePotentialResolutionStatus.Defined,
                        ItemBalanceWorkbenchCatalog.BalanceCandidate,
                        version.eligibleCoreEffectIds.Select(id => new ItemCorePotentialEffectSnapshot(id,
                            candidates.TryGetValue(id, out ItemBalanceCoreCandidate candidate) && candidate.isUltimate
                                ? ItemCorePotentialEffectKind.Ultimate
                                : ItemCorePotentialEffectKind.Standard,
                            version.visibleCoreEffectIds.Contains(id))),
                        version.visibleCoreEffectIds));
                }
            }

            ItemBuildQualificationRarityPolicySnapshot[] buildPolicies = ItemInstanceRarityCatalog.All.Select(value =>
                new ItemBuildQualificationRarityPolicySnapshot(value.rarity,
                    value.rarity == ItemInstanceRarity.White
                        ? ItemBuildQualificationPolicyMode.LockedNone
                        : ItemBuildQualificationPolicyMode.ProbabilityUnresolved,
                    string.Empty,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate)).ToArray();
            return ItemCorePotentialAndBuildEligibilitySchema.Create(corePolicies, profiles, buildPolicies);
        }

        private static IEnumerable<ItemBuildQualificationRollProfile> CompileBuildProfiles(
            ItemBalanceWorkbenchCatalog catalog)
        {
            foreach (ItemBalanceBuildPolicy policy in catalog.buildPolicies.Where(value => value != null))
            {
                if (policy.rarity == ItemInstanceRarity.White) continue;
                yield return new ItemBuildQualificationRollProfile(BuildProfileId(policy.rarity), policy.rarity,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    ItemBalanceWorkbenchCatalog.ToBuildEntries(policy));
            }
        }

        private static IEnumerable<ItemDropRarityWeightProfileSnapshot> CompileDropProfiles(
            ItemBalanceWorkbenchCatalog catalog)
        {
            foreach (ItemBalanceDropSegment segment in catalog.dropSegments.Where(value => value != null))
            {
                yield return new ItemDropRarityWeightProfileSnapshot(
                    $"candidate_drop_{segment.minStage}_{(segment.openEnded ? "plus" : segment.maxStage.ToString(CultureInfo.InvariantCulture))}",
                    ItemBalanceWorkbenchCatalog.BalanceCandidate,
                    new[]
                    {
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.White, segment.whiteWeight),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, segment.greenWeight),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Blue, segment.blueWeight),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Purple, segment.purpleWeight),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Orange, segment.orangeWeight)
                    }.Where(value => value.weightUnits > 0));
            }
        }

        private static IEnumerable<ItemBalancePreviewDimension> CompileDimensions(
            ItemBalanceWorkbenchCatalog catalog,
            ItemGeneratedInstanceSnapshot snapshot)
        {
            Dictionary<string, long> stats = snapshot.GeneratedStats.ToDictionary(value => value.statId,
                value => value.rawUnits, StringComparer.Ordinal);
            foreach (IGrouping<string, ItemBalancePreviewCoefficient> group in catalog.previewCoefficients
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.dimensionId))
                .GroupBy(value => value.dimensionId, StringComparer.Ordinal))
            {
                double total = 0d;
                List<string> contributors = new();
                foreach (ItemBalancePreviewCoefficient coefficient in group)
                {
                    if (!stats.TryGetValue(coefficient.statId, out long raw)) continue;
                    double contribution = raw * coefficient.coefficient;
                    total += contribution;
                    contributors.Add(coefficient.statId + "=" + contribution.ToString("0.##", CultureInfo.InvariantCulture));
                }

                yield return new ItemBalancePreviewDimension(group.Key, total, contributors);
            }
        }

        private static string BuildProfileId(ItemInstanceRarity rarity)
        {
            return "candidate_build_" + rarity.ToStableKey();
        }
    }
}
