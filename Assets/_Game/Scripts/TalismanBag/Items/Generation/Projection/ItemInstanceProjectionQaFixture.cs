using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Generation.Projection
{
    /// <summary>
    /// Shared runtime-safe QA fixture for the generated-instance projection contract.
    /// This is test data only; it is not a formal generation or balance catalog.
    /// </summary>
    public sealed class ItemInstanceProjectionQaFixture
    {
        private readonly ReadOnlyCollection<ItemGeneratedInstanceSnapshot> generatedInstances;

        private ItemInstanceProjectionQaFixture(
            ItemGenerationFoundationSnapshot foundation,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreBuildSchema,
            IEnumerable<ItemGeneratedInstanceSnapshot> generatedInstances,
            ItemInstanceProjectionSetSnapshot projectionSet)
        {
            Foundation = foundation;
            StatSchema = statSchema;
            AffixSchema = affixSchema;
            CoreBuildSchema = coreBuildSchema;
            this.generatedInstances = Array.AsReadOnly((generatedInstances ?? Array.Empty<ItemGeneratedInstanceSnapshot>()).ToArray());
            ProjectionSet = projectionSet;
        }

        public string fixtureStatus => ItemGenerationDataStatus.QaFixtureOnly;
        public string balanceStatus => ItemGenerationDataStatus.NotBalanceApproved;
        public string formalGenerationStatus => ItemGenerationDataStatus.NotFormalGenerationData;
        public ItemGenerationFoundationSnapshot Foundation { get; }
        public ItemStatRangeSchemaSnapshot StatSchema { get; }
        public ItemAffixPoolAndRangeSchemaSnapshot AffixSchema { get; }
        public ItemCorePotentialAndBuildEligibilitySchemaSnapshot CoreBuildSchema { get; }
        public IReadOnlyList<ItemGeneratedInstanceSnapshot> GeneratedInstances => generatedInstances;
        public ItemInstanceProjectionSetSnapshot ProjectionSet { get; }

        public static ItemInstanceProjectionQaFixture Create()
        {
            ItemGenerationFoundationSnapshot foundation = ItemRarityInstanceFoundation.Create();
            ItemStatRangeSchemaSnapshot stats = CreateStatSchema();
            ItemAffixPoolAndRangeSchemaSnapshot affixes = CreateAffixSchema();
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot core = CreateCoreSchema();

            ItemInstanceProjectionQaFixture temporary = new(
                foundation,
                stats,
                affixes,
                core,
                Array.Empty<ItemGeneratedInstanceSnapshot>(),
                null);
            ItemGeneratedInstanceSnapshot[] sources =
            {
                temporary.CreateGenerated(ItemInstanceRarity.White, 101L, "QA-I001-white", ItemBuildQualification.None),
                temporary.CreateGenerated(ItemInstanceRarity.Green, 202L, "QA-I001-green-A", ItemBuildQualification.FaMenOnly),
                temporary.CreateGenerated(ItemInstanceRarity.Blue, 303L, "QA-I001-blue", ItemBuildQualification.QiLeiOnly),
                temporary.CreateGenerated(ItemInstanceRarity.Purple, 404L, "QA-I001-purple", ItemBuildQualification.Dual),
                temporary.CreateGenerated(ItemInstanceRarity.Orange, 505L, "QA-I001-orange", ItemBuildQualification.None),
                temporary.CreateGenerated(ItemInstanceRarity.Green, 606L, "QA-I001-green-B", ItemBuildQualification.Dual)
            };
            ItemInstanceProjectionSetResult projected = ItemInstanceProjectionProvider.ProjectSet(
                new ItemInstanceProjectionSetRequest(sources));
            if (!projected.isSuccess || projected.snapshot == null)
            {
                throw new InvalidOperationException("Projection QA fixture failed: " + projected.primaryValidationCode);
            }

            return new ItemInstanceProjectionQaFixture(foundation, stats, affixes, core, sources, projected.snapshot);
        }

        public ItemGeneratedInstanceSnapshot CreateGenerated(
            ItemInstanceRarity rarity,
            long seed,
            string instanceId,
            ItemBuildQualification qualification)
        {
            ItemInstanceIdentityCreationResult identity = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                Foundation, instanceId, "I001", rarity, 1, seed, string.Empty);
            if (!identity.isValid)
            {
                throw new InvalidOperationException("Identity fixture failed: " + identity.primaryValidationCode);
            }

            ItemBuildQualificationRollProfile buildProfile = rarity == ItemInstanceRarity.White
                ? null
                : new ItemBuildQualificationRollProfile(
                    "qa_projection_" + rarity.ToStableKey(),
                    rarity,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemBuildQualificationRollEntry(qualification, 1) });
            ItemInstanceRollResult result = ItemInstanceRollEngine.Generate(new ItemInstanceRollRequest(
                identity.snapshot,
                StatSchema,
                AffixSchema,
                CoreBuildSchema,
                buildProfile,
                ItemGenerationDataStatus.QaFixtureCanonical));
            if (!result.isSuccess)
            {
                throw new InvalidOperationException("Roll fixture failed: " + result.primaryValidationCode);
            }

            if (result.snapshot.buildQualification != qualification)
            {
                throw new InvalidOperationException("Roll fixture qualification mismatch.");
            }

            return result.snapshot;
        }

        private static ItemStatRangeSchemaSnapshot CreateStatSchema()
        {
            ItemStatDefinitionSnapshot[] definitions =
            {
                new("qa_power", "威力", "flat", ItemStatDirection.HigherIsBetter,
                    0, 2, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA),
                new("qa_cooldown", "冷却", "seconds", ItemStatDirection.LowerIsBetter,
                    0, 1, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA)
            };
            ItemStatRangeProfileSnapshot[] profiles =
            {
                new("I001", "qa_power", ItemStatDataMaturity.SEED_DATA,
                    new ItemNumericRangeSnapshot(10, 60), StatRanges((10,14),(20,30),(30,40),(40,50),(50,60))),
                new("I001", "qa_cooldown", ItemStatDataMaturity.SEED_DATA,
                    new ItemNumericRangeSnapshot(1, 10), StatRanges((8,10),(6,9),(4,7),(2,5),(1,3)))
            };
            return ItemStatRangeSchema.Create(definitions, profiles);
        }

        private static ItemAffixPoolAndRangeSchemaSnapshot CreateAffixSchema()
        {
            ItemAffixDefinitionSnapshot[] definitions =
            {
                AffixDefinition("qa_fixed_guard", "固守"),
                AffixDefinition("qa_random_a", "灵息"),
                AffixDefinition("qa_random_b", "破煞"),
                AffixDefinition("qa_random_c", "凝神")
            };
            ItemAffixValueProfileSnapshot[] values = definitions.Select(definition =>
                new ItemAffixValueProfileSnapshot(
                    definition.affixId + "_value",
                    definition.affixId,
                    ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    new ItemAffixNumericRangeSnapshot(1, 15),
                    ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                        new ItemAffixRarityValueRangeSnapshot(rarity.rarity, 1 + index * 3, 3 + index * 3)),
                    definition.direction,
                    definition.decimalPlaces,
                    definition.stepUnits,
                    definition.roundingMode)).ToArray();
            ItemAffixSlotPolicySnapshot slots = new(
                "qa_slots",
                ItemAffixResolutionStatus.Defined,
                ItemGenerationDataStatus.QaFixtureOnly,
                new[]
                {
                    new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed),
                    new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random),
                    new ItemAffixSlotSnapshot("random_02", ItemAffixSlotKind.Random)
                });
            ItemRandomAffixPoolSnapshot pool = new(
                "qa_pool",
                ItemAffixResolutionStatus.Defined,
                ItemGenerationDataStatus.QaFixtureOnly,
                ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                new[]
                {
                    new ItemAffixPoolEntrySnapshot("qa_random_a", "qa_random_a_value", ItemAffixWeightResolutionStatus.Defined, 1),
                    new ItemAffixPoolEntrySnapshot("qa_random_b", "qa_random_b_value", ItemAffixWeightResolutionStatus.Defined, 3),
                    new ItemAffixPoolEntrySnapshot("qa_random_c", "qa_random_c_value", ItemAffixWeightResolutionStatus.Defined, 7)
                },
                new[] { new ItemAffixMutexGroupSnapshot("qa_mutex_ab", new[] { "qa_random_a", "qa_random_b" }) });
            ItemAffixGenerationProfileSnapshot generation = new(
                "I001",
                ItemAffixResolutionStatus.Defined,
                ItemGenerationDataStatus.QaFixtureOnly,
                "qa_slots",
                new[] { new ItemFixedAffixBindingSnapshot("fixed_01", "qa_fixed_guard", "qa_fixed_guard_value") },
                "qa_pool");
            return ItemAffixPoolAndRangeSchema.Create(
                definitions,
                values,
                new[] { slots },
                new[] { pool },
                new[] { generation });
        }

        private static ItemCorePotentialAndBuildEligibilitySchemaSnapshot CreateCoreSchema()
        {
            List<ItemCorePotentialProfileSnapshot> profiles = new();
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                profiles.Add(new ItemCorePotentialProfileSnapshot(
                    "qa_core_i001_" + rarity.stableKey,
                    "I001",
                    rarity.rarity,
                    ItemCorePotentialResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    new[]
                    {
                        new ItemCorePotentialEffectSnapshot(
                            "qa_core_" + rarity.stableKey + "_01",
                            ItemCorePotentialEffectKind.Standard,
                            true),
                        new ItemCorePotentialEffectSnapshot(
                            "qa_core_" + rarity.stableKey + "_02",
                            rarity.rarity == ItemInstanceRarity.Orange
                                ? ItemCorePotentialEffectKind.Ultimate
                                : ItemCorePotentialEffectKind.Standard,
                            false)
                    }));
            }

            return ItemCorePotentialAndBuildEligibilitySchema.Create(
                ItemCorePotentialAndBuildEligibilityCatalog.DefaultCoreRarityPolicies,
                profiles,
                ItemCorePotentialAndBuildEligibilityCatalog.DefaultBuildRarityPolicies);
        }

        private static ItemRarityStatRangeSnapshot[] StatRanges(params (long min, long max)[] values)
        {
            return ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                new ItemRarityStatRangeSnapshot(rarity.rarity, values[index].min, values[index].max)).ToArray();
        }

        private static ItemAffixDefinitionSnapshot AffixDefinition(string id, string displayName)
        {
            return new ItemAffixDefinitionSnapshot(
                id,
                displayName,
                "flat",
                ItemStatDirection.HigherIsBetter,
                0,
                1,
                ItemStatRoundingMode.Nearest,
                ItemGenerationDataStatus.QaFixtureOnly);
        }
    }
}
