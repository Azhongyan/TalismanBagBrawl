#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Stats;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemAffixPoolAndRangeSchemaVerifier
    {
        private const string PackageName = "V0.4-ItemAffixPoolAndRangeSchema01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaLeakCheckReport.md";
        private const string InventoryPath = "Docs/V0.4/Reports/ItemAffixSchemaInventory.csv";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes";
        private const string PassMarker = "ITEM_AFFIX_POOL_AND_RANGE_SCHEMA01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemAffixPoolAndRangeSchema01/[QA Only] Run Schema")]
        public static void VerifyMenu() => VerifyAndWriteReports(false);

        public static void VerifyStaticBatch() => VerifyAndWriteReports(Application.isBatchMode);

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<SpecRow> rows = new();
            List<LeakRow> leakRows = new();
            ItemAffixPoolAndRangeSchemaSnapshot defaultSchema = null;
            ItemAffixPoolAndRangeSchemaSnapshot fixtureSchema = null;
            try
            {
                defaultSchema = ItemAffixPoolAndRangeCatalog.CreateDefaultSchema();
                fixtureSchema = RunSchemaCases(defaultSchema, rows, result);
                RunLeakChecks(leakRows, result);
                RunHistoricalRegressions(result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Verifier threw an unhandled exception: " + exception);
            }

            if (rows.Count < 60) result.Errors.Add($"Spec count is {rows.Count}; expected at least 60.");
            bool passed = result.Errors.Count == 0 && rows.All(row => row.result == "PASS")
                && leakRows.All(row => row.count == 0);
            WriteReports(defaultSchema, fixtureSchema, rows, leakRows, result, passed);
            if (passed) Debug.Log(PassMarker + $" specs={rows.Count}");
            else Debug.LogError("ITEM_AFFIX_POOL_AND_RANGE_SCHEMA01_FAIL\n" + string.Join("\n", result.Errors));
            if (exitWhenBatchMode) EditorApplication.Exit(passed ? 0 : 1);
        }

        private static ItemAffixPoolAndRangeSchemaSnapshot RunSchemaCases(
            ItemAffixPoolAndRangeSchemaSnapshot defaultSchema, List<SpecRow> rows, VerificationResult result)
        {
            AddBool(rows, result, "default-schema-valid", "default-catalog", defaultSchema?.isValid == true);
            AddBool(rows, result, "default-schema-id", "default-catalog", defaultSchema?.schemaId == ItemAffixPoolAndRangeSchemaSnapshot.CurrentSchemaId);
            AddBool(rows, result, "default-definitions-zero", "default-catalog", defaultSchema?.AffixDefinitions.Count == 0);
            AddBool(rows, result, "default-value-profiles-zero", "default-catalog", defaultSchema?.AffixValueProfiles.Count == 0);
            AddBool(rows, result, "default-slot-policies-zero", "default-catalog", defaultSchema?.SlotPolicies.Count == 0);
            AddBool(rows, result, "default-random-pools-zero", "default-catalog", defaultSchema?.RandomPools.Count == 0);
            AddBool(rows, result, "default-item-profiles-zero", "default-catalog", defaultSchema?.ItemGenerationProfiles.Count == 0);
            AddBool(rows, result, "default-fixed-bindings-zero", "default-catalog", CountBindings(defaultSchema) == 0);
            AddBool(rows, result, "default-defined-weights-zero", "default-catalog", CountDefinedWeights(defaultSchema) == 0);
            AddBool(rows, result, "default-mutex-groups-zero", "default-catalog", CountMutex(defaultSchema) == 0);
            ItemAffixPoolAndRangeCatalogNotice notice = ItemAffixPoolAndRangeCatalog.GetCatalogNotice();
            AddBool(rows, result, "catalog-schema-only", "default-catalog", notice.dataMaturityKey == "SCHEMA_ONLY");
            AddBool(rows, result, "catalog-unresolved-coverage", "default-catalog", notice.coverageStatus == "UNRESOLVED_DATA_COVERAGE");
            AddBool(rows, result, "fixture-marker", "fixture-isolation", notice.fixtureStatus == "QA_FIXTURE_ONLY");
            AddBool(rows, result, "balance-marker", "fixture-isolation", notice.balanceStatus == "NOT_BALANCE_APPROVED");
            AddBool(rows, result, "generation-marker", "fixture-isolation", notice.generationDataStatus == "NOT_FORMAL_GENERATION_DATA");

            Fixture fixture = CreateFixture();
            ItemAffixPoolAndRangeSchemaSnapshot fixtureSchema = fixture.Create();
            Expect(rows, result, "qa-fixture-valid", "fixture", true, ItemAffixPoolAndRangeValidationCodes.None, fixtureSchema);
            AddBool(rows, result, "qa-fixture-fixed-slot", "fixture", fixtureSchema.SlotPolicies.Single().Slots.Count(x => x.slotKind == ItemAffixSlotKind.Fixed) == 1);
            AddBool(rows, result, "qa-fixture-random-slot", "fixture", fixtureSchema.SlotPolicies.Single().Slots.Count(x => x.slotKind == ItemAffixSlotKind.Random) == 1);
            AddBool(rows, result, "qa-fixture-relative-weight-not-percent", "fixture", fixtureSchema.RandomPools.Single().Entries.Sum(x => x.weightUnits) != 100);
            AddBool(rows, result, "qa-fixture-mutex", "fixture", fixtureSchema.RandomPools.Single().MutexGroups.Count == 1);
            AddBool(rows, result, "qa-fixture-repeat-policy", "fixture", fixtureSchema.RandomPools.Single().repeatPolicy == ItemAffixRepeatPolicy.AllowDuplicateAffix);
            AddBool(rows, result, "fixed-binding-has-no-value", "fixed-affix", !HasProperty(typeof(ItemFixedAffixBindingSnapshot), "rolledValue"));

            Expect(rows, result, "range-higher-valid", "value-range", true, ItemAffixPoolAndRangeValidationCodes.None,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher()));
            Expect(rows, result, "range-lower-valid", "value-range", true, ItemAffixPoolAndRangeValidationCodes.None,
                RangeSchema(ItemStatDirection.LowerIsBetter, RangesLower()));
            Expect(rows, result, "range-neutral-valid", "value-range", true, ItemAffixPoolAndRangeValidationCodes.None,
                RangeSchema(ItemStatDirection.Neutral, RangesNeutral()));
            AddBool(rows, result, "range-overlap-allowed", "value-range", RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher()).isValid);
            Expect(rows, result, "range-total-invalid", "value-range", false, ItemAffixPoolAndRangeValidationCodes.TotalRangeInvalid,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher(), new ItemAffixNumericRangeSnapshot(10, 1)));
            ItemAffixRarityValueRangeSnapshot[] outside = RangesHigher(); outside[4] = new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Orange, 9, 11);
            Expect(rows, result, "range-outside-total", "value-range", false, ItemAffixPoolAndRangeValidationCodes.RangeOutsideTotal,
                RangeSchema(ItemStatDirection.HigherIsBetter, outside));
            Expect(rows, result, "range-envelope-mismatch", "value-range", false, ItemAffixPoolAndRangeValidationCodes.TotalRangeEnvelopeMismatch,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher(), new ItemAffixNumericRangeSnapshot(0, 10)));
            Expect(rows, result, "step-invalid", "value-range", false, ItemAffixPoolAndRangeValidationCodes.StepUnitsInvalid,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher(), null, 0));
            Expect(rows, result, "step-misaligned", "value-range", false, ItemAffixPoolAndRangeValidationCodes.RangeStepMisaligned,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher(), null, 2));
            Expect(rows, result, "rarity-missing", "value-range", false, ItemAffixPoolAndRangeValidationCodes.RaritySetIncomplete,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher().Take(4).ToArray()));
            ItemAffixRarityValueRangeSnapshot[] duplicateRarity = RangesHigher().Concat(new[] { new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.White, 1, 2) }).ToArray();
            Expect(rows, result, "rarity-duplicate", "value-range", false, ItemAffixPoolAndRangeValidationCodes.RarityDuplicate,
                RangeSchema(ItemStatDirection.HigherIsBetter, duplicateRarity));
            ItemAffixRarityValueRangeSnapshot[] invalidRarity = RangesHigher(); invalidRarity[0] = new ItemAffixRarityValueRangeSnapshot((ItemInstanceRarity)99, 1, 2);
            Expect(rows, result, "rarity-invalid", "value-range", false, ItemAffixPoolAndRangeValidationCodes.RarityInvalid,
                RangeSchema(ItemStatDirection.HigherIsBetter, invalidRarity));
            ItemAffixRarityValueRangeSnapshot[] highMin = RangesHigher(); highMin[2] = new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 2, 8);
            Expect(rows, result, "higher-min-monotonic", "value-range", false, ItemAffixPoolAndRangeValidationCodes.HigherMinNotMonotonic,
                RangeSchema(ItemStatDirection.HigherIsBetter, highMin));
            ItemAffixRarityValueRangeSnapshot[] highMax = RangesHigher(); highMax[2] = new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 5, 4);
            Expect(rows, result, "higher-max-monotonic", "value-range", false, ItemAffixPoolAndRangeValidationCodes.HigherMaxNotMonotonic,
                RangeSchema(ItemStatDirection.HigherIsBetter, highMax));
            ItemAffixRarityValueRangeSnapshot[] lowMin = RangesLower(); lowMin[2] = new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 8, 8);
            Expect(rows, result, "lower-min-monotonic", "value-range", false, ItemAffixPoolAndRangeValidationCodes.LowerMinNotMonotonic,
                RangeSchema(ItemStatDirection.LowerIsBetter, lowMin));
            ItemAffixRarityValueRangeSnapshot[] lowMax = RangesLower(); lowMax[2] = new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 5, 10);
            Expect(rows, result, "lower-max-monotonic", "value-range", false, ItemAffixPoolAndRangeValidationCodes.LowerMaxNotMonotonic,
                RangeSchema(ItemStatDirection.LowerIsBetter, lowMax));
            Expect(rows, result, "unresolved-profile-has-range", "value-range", false, ItemAffixPoolAndRangeValidationCodes.UnresolvedValueProfileHasRange,
                RangeSchema(ItemStatDirection.HigherIsBetter, RangesHigher(), null, 1, ItemAffixResolutionStatus.Unresolved));
            Fixture mismatchContract = CreateFixture(); mismatchContract.ValueProfiles[0] = Profile("qa_affix_value_profile_01", "qa_fixed_break_example", ItemStatDirection.HigherIsBetter, RangesHigher(), 2);
            Expect(rows, result, "profile-definition-contract-mismatch", "value-range", false, ItemAffixPoolAndRangeValidationCodes.ValueProfileDefinitionMismatch, mismatchContract.Create());

            Fixture unknownFixedAffix = CreateFixture(); unknownFixedAffix.GenerationProfiles[0] = Generation("I001", "unknown", "qa_affix_value_profile_01", "fixed_01");
            Expect(rows, result, "fixed-unknown-affix", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.AffixDefinitionUnknown, unknownFixedAffix.Create());
            Fixture unknownFixedValue = CreateFixture(); unknownFixedValue.GenerationProfiles[0] = Generation("I001", "qa_fixed_break_example", "unknown", "fixed_01");
            Expect(rows, result, "fixed-unknown-value-profile", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.ValueProfileUnknown, unknownFixedValue.Create());
            Fixture mismatchFixedValue = CreateFixture(); mismatchFixedValue.GenerationProfiles[0] = Generation("I001", "qa_fixed_break_example", "qa_random_affix_a_value", "fixed_01");
            Expect(rows, result, "fixed-value-profile-affix-mismatch", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.ValueProfileAffixMismatch, mismatchFixedValue.Create());
            Fixture unknownFixedSlot = CreateFixture(); unknownFixedSlot.GenerationProfiles[0] = Generation("I001", "qa_fixed_break_example", "qa_affix_value_profile_01", "unknown");
            Expect(rows, result, "fixed-unknown-slot", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.FixedBindingSlotUnknown, unknownFixedSlot.Create());
            Fixture randomFixedSlot = CreateFixture(); randomFixedSlot.GenerationProfiles[0] = Generation("I001", "qa_fixed_break_example", "qa_affix_value_profile_01", "random_01");
            Expect(rows, result, "fixed-binding-random-slot", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.FixedBindingSlotNotFixed, randomFixedSlot.Create());
            Fixture duplicateBinding = CreateFixture(); duplicateBinding.GenerationProfiles[0] = new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", "qa_slot_policy_01", new[] { Binding("fixed_01"), Binding("fixed_01") }, "qa_random_pool_01");
            Expect(rows, result, "fixed-binding-duplicate", "fixed-affix", false, ItemAffixPoolAndRangeValidationCodes.FixedBindingDuplicate, duplicateBinding.Create());

            AddBool(rows, result, "unresolved-entry-weight-zero-structure", "random-pool", new ItemAffixPoolEntrySnapshot("a", "v", ItemAffixWeightResolutionStatus.Unresolved, 0).weightUnits == 0);
            AddBool(rows, result, "defined-relative-weight-positive", "random-pool", fixtureSchema.RandomPools.Single().Entries.All(x => x.weightUnits > 0));
            Fixture unresolvedWeight = CreateFixture(); unresolvedWeight.Pools[0] = Pool(Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Unresolved, 1));
            Expect(rows, result, "unresolved-weight-nonzero", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.UnresolvedWeightNonZero, unresolvedWeight.Create());
            Fixture definedWeightZero = CreateFixture(); definedWeightZero.Pools[0] = Pool(Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Defined, 0));
            Expect(rows, result, "defined-weight-zero", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.DefinedWeightNotPositive, definedWeightZero.Create());
            Fixture duplicateEntry = CreateFixture(); duplicateEntry.Pools[0] = Pool(Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Defined, 1), Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Defined, 2));
            Expect(rows, result, "pool-entry-duplicate", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.PoolEntryDuplicate, duplicateEntry.Create());
            Fixture mutexUnknown = CreateFixture(); mutexUnknown.Pools[0] = PoolWithMutex(new[] { "qa_random_affix_a", "unknown" });
            Expect(rows, result, "mutex-unknown-member", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.MutexMemberUnknown, mutexUnknown.Create());
            Fixture mutexSmall = CreateFixture(); mutexSmall.Pools[0] = PoolWithMutex(new[] { "qa_random_affix_a" });
            Expect(rows, result, "mutex-too-small", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.MutexGroupTooSmall, mutexSmall.Create());
            Fixture mutexDuplicate = CreateFixture(); mutexDuplicate.Pools[0] = PoolWithMutex(new[] { "qa_random_affix_a", "qa_random_affix_a" });
            Expect(rows, result, "mutex-duplicate-member", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.MutexMemberDuplicate, mutexDuplicate.Create());
            Fixture unresolvedPoolContent = CreateFixture(); unresolvedPoolContent.Pools[0] = new ItemRandomAffixPoolSnapshot("qa_random_pool_01", ItemAffixResolutionStatus.Unresolved, "QA_FIXTURE_ONLY", ItemAffixRepeatPolicy.Unresolved, new[] { Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Unresolved, 0) }, Array.Empty<ItemAffixMutexGroupSnapshot>());
            Expect(rows, result, "unresolved-pool-content", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.UnresolvedPoolHasContent, unresolvedPoolContent.Create());
            Fixture definedRepeatUnresolved = CreateFixture(); definedRepeatUnresolved.Pools[0] = new ItemRandomAffixPoolSnapshot("qa_random_pool_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", ItemAffixRepeatPolicy.Unresolved, CreatePoolEntries(), Array.Empty<ItemAffixMutexGroupSnapshot>());
            Expect(rows, result, "defined-repeat-unresolved", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.DefinedPoolRepeatPolicyUnresolved, definedRepeatUnresolved.Create());
            Fixture invalidRepeat = CreateFixture(); invalidRepeat.Pools[0] = new ItemRandomAffixPoolSnapshot("qa_random_pool_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", (ItemAffixRepeatPolicy)99, CreatePoolEntries(), Array.Empty<ItemAffixMutexGroupSnapshot>());
            Expect(rows, result, "repeat-policy-invalid", "random-pool", false, ItemAffixPoolAndRangeValidationCodes.RepeatPolicyInvalid, invalidRepeat.Create());

            Expect(rows, result, "item-profile-i001-valid", "item-profile", true, ItemAffixPoolAndRangeValidationCodes.None, fixtureSchema);
            ExpectProfileId(rows, result, "item-profile-i031", "I031", ItemAffixPoolAndRangeValidationCodes.BaseItemNotOrdinary);
            ExpectProfileId(rows, result, "item-profile-i999", "I999", ItemAffixPoolAndRangeValidationCodes.BaseItemUnknown);
            ExpectProfileId(rows, result, "item-profile-empty", "", ItemAffixPoolAndRangeValidationCodes.BaseItemIdEmpty);
            Fixture duplicateItem = CreateFixture(); duplicateItem.GenerationProfiles.Add(duplicateItem.GenerationProfiles[0]);
            Expect(rows, result, "item-profile-duplicate", "item-profile", false, ItemAffixPoolAndRangeValidationCodes.GenerationProfileDuplicate, duplicateItem.Create());
            Fixture unknownPolicy = CreateFixture(); unknownPolicy.GenerationProfiles[0] = new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", "unknown", Array.Empty<ItemFixedAffixBindingSnapshot>(), "qa_random_pool_01");
            Expect(rows, result, "item-profile-unknown-policy", "item-profile", false, ItemAffixPoolAndRangeValidationCodes.SlotPolicyUnknown, unknownPolicy.Create());
            Fixture unknownPool = CreateFixture(); unknownPool.GenerationProfiles[0] = new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", "qa_slot_policy_01", new[] { Binding("fixed_01") }, "unknown");
            Expect(rows, result, "item-profile-unknown-pool", "item-profile", false, ItemAffixPoolAndRangeValidationCodes.RandomPoolUnknown, unknownPool.Create());
            Fixture unresolvedItemContent = CreateFixture(); unresolvedItemContent.GenerationProfiles[0] = new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Unresolved, "QA_FIXTURE_ONLY", "qa_slot_policy_01", new[] { Binding("fixed_01") }, "qa_random_pool_01");
            Expect(rows, result, "unresolved-item-profile-content", "item-profile", false, ItemAffixPoolAndRangeValidationCodes.UnresolvedGenerationProfileHasContent, unresolvedItemContent.Create());
            Fixture poolWithoutRandomSlot = CreateFixture(); poolWithoutRandomSlot.SlotPolicies[0] = new ItemAffixSlotPolicySnapshot("qa_slot_policy_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", new[] { new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed) });
            Expect(rows, result, "pool-without-random-slot", "item-profile", false, ItemAffixPoolAndRangeValidationCodes.RandomPoolSlotMismatch, poolWithoutRandomSlot.Create());

            AddBool(rows, result, "query-affix-success", "query", fixtureSchema.QueryAffixDefinition("qa_random_affix_a").isSuccess);
            AddBool(rows, result, "query-value-success", "query", fixtureSchema.QueryAffixValueProfile("qa_random_affix_a_value").isSuccess);
            AddBool(rows, result, "query-slot-success", "query", fixtureSchema.QuerySlotPolicy("qa_slot_policy_01").isSuccess);
            AddBool(rows, result, "query-pool-success", "query", fixtureSchema.QueryRandomPool("qa_random_pool_01").isSuccess);
            AddBool(rows, result, "query-item-success", "query", fixtureSchema.QueryItemGenerationProfile("I001").isSuccess);
            AddQueryFailure(rows, result, "query-affix-missing", fixtureSchema.QueryAffixDefinition("missing").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QueryAffixNotFound);
            AddQueryFailure(rows, result, "query-value-missing", fixtureSchema.QueryAffixValueProfile("missing").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QueryValueProfileNotFound);
            AddQueryFailure(rows, result, "query-slot-missing", fixtureSchema.QuerySlotPolicy("missing").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QuerySlotPolicyNotFound);
            AddQueryFailure(rows, result, "query-pool-missing", fixtureSchema.QueryRandomPool("missing").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QueryPoolNotFound);
            AddQueryFailure(rows, result, "query-item-missing", fixtureSchema.QueryItemGenerationProfile("I030").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QueryGenerationProfileNotFound);
            AddQueryFailure(rows, result, "query-input-empty", fixtureSchema.QueryAffixDefinition("").validationError?.code, ItemAffixPoolAndRangeValidationCodes.QueryInputInvalid);

            AddBool(rows, result, "canonical-input-reversal", "determinism", fixtureSchema.BuildCanonicalSignature() == fixture.Create(true).BuildCanonicalSignature());
            AddBool(rows, result, "source-mutation-isolated", "immutability", CheckSourceMutation());
            AddBool(rows, result, "top-level-read-only", "immutability", RejectMutation(fixtureSchema.AffixDefinitions));
            AddBool(rows, result, "rarity-ranges-read-only", "immutability", RejectMutation(fixtureSchema.AffixValueProfiles[0].RarityRanges));
            AddBool(rows, result, "slots-read-only", "immutability", RejectMutation(fixtureSchema.SlotPolicies[0].Slots));
            AddBool(rows, result, "entries-read-only", "immutability", RejectMutation(fixtureSchema.RandomPools[0].Entries));
            AddBool(rows, result, "mutex-members-read-only", "immutability", RejectMutation(fixtureSchema.RandomPools[0].MutexGroups[0].AffixIds));
            AddBool(rows, result, "bindings-read-only", "immutability", RejectMutation(fixtureSchema.ItemGenerationProfiles[0].FixedAffixBindings));
            AddBool(rows, result, "no-generation-result-fields", "reflection", CheckForbiddenFields());
            AddBool(rows, result, "multiple-value-profiles-per-affix", "schema", CheckMultipleProfilesPerAffix());
            AddBool(rows, result, "invalid-path-no-throw", "stability", CheckNoThrow());
            return fixtureSchema;
        }

        private static Fixture CreateFixture()
        {
            Fixture f = new();
            f.Definitions.Add(Definition("qa_fixed_break_example"));
            f.Definitions.Add(Definition("qa_random_affix_a"));
            f.Definitions.Add(Definition("qa_random_affix_b"));
            f.Definitions.Add(Definition("qa_random_affix_c"));
            f.ValueProfiles.Add(Profile("qa_affix_value_profile_01", "qa_fixed_break_example", ItemStatDirection.HigherIsBetter, RangesHigher()));
            f.ValueProfiles.Add(Profile("qa_random_affix_a_value", "qa_random_affix_a", ItemStatDirection.HigherIsBetter, RangesHigher()));
            f.ValueProfiles.Add(Profile("qa_random_affix_b_value", "qa_random_affix_b", ItemStatDirection.HigherIsBetter, RangesHigher()));
            f.ValueProfiles.Add(Profile("qa_random_affix_c_value", "qa_random_affix_c", ItemStatDirection.HigherIsBetter, RangesHigher()));
            f.SlotPolicies.Add(new ItemAffixSlotPolicySnapshot("qa_slot_policy_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", new[] { new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed), new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random) }));
            f.Pools.Add(PoolWithMutex(new[] { "qa_random_affix_a", "qa_random_affix_b" }));
            f.GenerationProfiles.Add(Generation("I001", "qa_fixed_break_example", "qa_affix_value_profile_01", "fixed_01"));
            return f;
        }

        private static ItemAffixDefinitionSnapshot Definition(string id, ItemStatDirection direction = ItemStatDirection.HigherIsBetter, long step = 1) => new(id, id, "percent", direction, 0, step, ItemStatRoundingMode.Nearest, "QA_FIXTURE_ONLY");
        private static ItemAffixValueProfileSnapshot Profile(string id, string affixId, ItemStatDirection direction, ItemAffixRarityValueRangeSnapshot[] ranges, long step = 1, ItemAffixResolutionStatus status = ItemAffixResolutionStatus.Defined, ItemAffixNumericRangeSnapshot total = null) => new(id, affixId, status, "QA_FIXTURE_ONLY", total ?? new ItemAffixNumericRangeSnapshot(1, 10), ranges, direction, 0, step, ItemStatRoundingMode.Nearest);
        private static ItemAffixRarityValueRangeSnapshot[] RangesHigher() => new[] { new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.White, 1, 2), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Green, 3, 5), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 5, 8), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Purple, 6, 9), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Orange, 9, 10) };
        private static ItemAffixRarityValueRangeSnapshot[] RangesLower() => new[] { new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.White, 9, 10), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Green, 6, 9), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 5, 8), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Purple, 3, 5), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Orange, 1, 2) };
        private static ItemAffixRarityValueRangeSnapshot[] RangesNeutral() => new[] { new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.White, 1, 10), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Green, 2, 3), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Blue, 1, 2), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Purple, 8, 9), new ItemAffixRarityValueRangeSnapshot(ItemInstanceRarity.Orange, 4, 5) };
        private static ItemAffixPoolEntrySnapshot Entry(string affix, string profile, ItemAffixWeightResolutionStatus status, long weight) => new(affix, profile, status, weight);
        private static ItemAffixPoolEntrySnapshot[] CreatePoolEntries() => new[] { Entry("qa_random_affix_a", "qa_random_affix_a_value", ItemAffixWeightResolutionStatus.Defined, 1), Entry("qa_random_affix_b", "qa_random_affix_b_value", ItemAffixWeightResolutionStatus.Defined, 2), Entry("qa_random_affix_c", "qa_random_affix_c_value", ItemAffixWeightResolutionStatus.Defined, 4) };
        private static ItemRandomAffixPoolSnapshot Pool(params ItemAffixPoolEntrySnapshot[] entries) => new("qa_random_pool_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", ItemAffixRepeatPolicy.AllowDuplicateAffix, entries, Array.Empty<ItemAffixMutexGroupSnapshot>());
        private static ItemRandomAffixPoolSnapshot PoolWithMutex(IEnumerable<string> ids) => new("qa_random_pool_01", ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", ItemAffixRepeatPolicy.AllowDuplicateAffix, CreatePoolEntries(), new[] { new ItemAffixMutexGroupSnapshot("qa_mutex_01", ids) });
        private static ItemFixedAffixBindingSnapshot Binding(string slotId) => new(slotId, "qa_fixed_break_example", "qa_affix_value_profile_01");
        private static ItemAffixGenerationProfileSnapshot Generation(string baseId, string affix, string value, string slot) => new(baseId, ItemAffixResolutionStatus.Defined, "QA_FIXTURE_ONLY", "qa_slot_policy_01", new[] { new ItemFixedAffixBindingSnapshot(slot, affix, value) }, "qa_random_pool_01");

        private static ItemAffixPoolAndRangeSchemaSnapshot RangeSchema(ItemStatDirection direction, ItemAffixRarityValueRangeSnapshot[] ranges, ItemAffixNumericRangeSnapshot total = null, long step = 1, ItemAffixResolutionStatus status = ItemAffixResolutionStatus.Defined)
        { return ItemAffixPoolAndRangeSchema.Create(new[] { Definition("range_affix", direction, step) }, new[] { Profile("range_profile", "range_affix", direction, ranges, step, status, total) }, Array.Empty<ItemAffixSlotPolicySnapshot>(), Array.Empty<ItemRandomAffixPoolSnapshot>(), Array.Empty<ItemAffixGenerationProfileSnapshot>()); }

        private static void ExpectProfileId(List<SpecRow> rows, VerificationResult result, string caseId, string baseId, string code)
        { Fixture f = CreateFixture(); f.GenerationProfiles[0] = Generation(baseId, "qa_fixed_break_example", "qa_affix_value_profile_01", "fixed_01"); Expect(rows, result, caseId, "item-profile", false, code, f.Create()); }
        private static void Expect(List<SpecRow> rows, VerificationResult result, string caseId, string category, bool expectedValid, string expectedCode, ItemAffixPoolAndRangeSchemaSnapshot schema)
        { string[] codes = schema?.ValidationErrors.Select(e => e.code).Distinct(StringComparer.Ordinal).ToArray() ?? new[] { "SCHEMA_NULL" }; bool codeMatch = expectedCode == ItemAffixPoolAndRangeValidationCodes.None ? codes.Length == 0 : codes.Contains(expectedCode, StringComparer.Ordinal); bool pass = schema?.isValid == expectedValid && codeMatch; rows.Add(new SpecRow(caseId, category, expectedValid ? "valid" : expectedCode, schema?.isValid == true ? "valid" : string.Join("|", codes), string.Join("|", codes), pass ? "PASS" : "FAIL")); if (!pass) result.Errors.Add($"Schema case '{caseId}' failed: {string.Join("|", codes)}"); }
        private static void AddBool(List<SpecRow> rows, VerificationResult result, string caseId, string category, bool pass)
        { rows.Add(new SpecRow(caseId, category, "true", pass ? "true" : "false", ItemAffixPoolAndRangeValidationCodes.None, pass ? "PASS" : "FAIL")); if (!pass) result.Errors.Add("Boolean case failed: " + caseId); }
        private static void AddQueryFailure(List<SpecRow> rows, VerificationResult result, string caseId, string actual, string expected)
        { bool pass = actual == expected; rows.Add(new SpecRow(caseId, "query", expected, actual ?? "null", actual ?? "null", pass ? "PASS" : "FAIL")); if (!pass) result.Errors.Add("Query case failed: " + caseId); }

        private static bool CheckSourceMutation()
        { List<ItemAffixDefinitionSnapshot> definitions = new() { Definition("source_a") }; ItemAffixPoolAndRangeSchemaSnapshot schema = ItemAffixPoolAndRangeSchema.Create(definitions, Array.Empty<ItemAffixValueProfileSnapshot>(), Array.Empty<ItemAffixSlotPolicySnapshot>(), Array.Empty<ItemRandomAffixPoolSnapshot>(), Array.Empty<ItemAffixGenerationProfileSnapshot>()); definitions.Clear(); definitions.Add(Definition("source_b")); return schema.AffixDefinitions.Count == 1 && schema.AffixDefinitions[0].affixId == "source_a"; }
        private static bool RejectMutation<T>(IReadOnlyList<T> values)
        { try { ((IList)values).Clear(); return false; } catch (NotSupportedException) { return true; } catch (InvalidCastException) { return true; } }
        private static bool HasProperty(Type type, string name) => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        private static bool CheckForbiddenFields()
        { string[] forbidden = { "rolledAffixIds", "rolledAffixes", "rolledValue", "selectedAffixId", "selectedPoolEntry", "randomSeed", "rootSeed", "rerollCount", "guaranteedAffix", "pityCounter", "actualProbability", "normalizedChance" }; Type[] types = typeof(ItemAffixPoolAndRangeSchemaSnapshot).Assembly.GetTypes().Where(t => t.Namespace == "TalismanBag.Items.Generation.Affixes").ToArray(); return forbidden.All(name => types.All(t => t.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).All(m => !string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)))); }
        private static bool CheckMultipleProfilesPerAffix()
        { ItemAffixDefinitionSnapshot d = Definition("multi"); ItemAffixPoolAndRangeSchemaSnapshot s = ItemAffixPoolAndRangeSchema.Create(new[] { d }, new[] { Profile("multi_1", "multi", ItemStatDirection.HigherIsBetter, RangesHigher()), Profile("multi_2", "multi", ItemStatDirection.HigherIsBetter, RangesHigher()) }, Array.Empty<ItemAffixSlotPolicySnapshot>(), Array.Empty<ItemRandomAffixPoolSnapshot>(), Array.Empty<ItemAffixGenerationProfileSnapshot>()); return s.isValid && s.AffixValueProfiles.Count == 2; }
        private static bool CheckNoThrow()
        { try { ItemAffixPoolAndRangeSchemaSnapshot s = ItemAffixPoolAndRangeSchema.Create(new ItemAffixDefinitionSnapshot[] { null }, new ItemAffixValueProfileSnapshot[] { null }, new ItemAffixSlotPolicySnapshot[] { null }, new ItemRandomAffixPoolSnapshot[] { null }, new ItemAffixGenerationProfileSnapshot[] { null }); return !s.isValid && !s.QueryAffixDefinition(null).isSuccess; } catch { return false; } }

        private static int CountBindings(ItemAffixPoolAndRangeSchemaSnapshot schema) => schema?.ItemGenerationProfiles.Sum(x => x.FixedAffixBindings.Count) ?? -1;
        private static int CountDefinedWeights(ItemAffixPoolAndRangeSchemaSnapshot schema) => schema?.RandomPools.Sum(x => x.Entries.Count(e => e.weightResolutionStatus == ItemAffixWeightResolutionStatus.Defined)) ?? -1;
        private static int CountMutex(ItemAffixPoolAndRangeSchemaSnapshot schema) => schema?.RandomPools.Sum(x => x.MutexGroups.Count) ?? -1;

        private static void RunLeakChecks(List<LeakRow> rows, VerificationResult result)
        {
            string text = string.Join("\n", Directory.Exists(RuntimeRoot) ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText) : Array.Empty<string>());
            LeakDefinition[] definitions =
            {
                new("Reward", "Reward"), new("RunFlow", "RunFlow"), new("Inventory", "Inventory"),
                new("SaveData", "SaveData"), new("Boss", "Boss"), new("Battle Resolver", "BattleResolver"),
                new("Battle Bridge", "BattleBridge"), new("ItemBuildSynergyResolver", "ItemBuildSynergyResolver"),
                new("ItemCoreAwakeningResolver", "ItemCoreAwakeningResolver"), new("System.Random", "System.Random"),
                new("UnityEngine.Random", "UnityEngine.Random"), new("QualificationRoll", "QualificationRoll"),
                new("AffixRoll", "AffixRoll"), new("RollAffix", "RollAffix"), new("rolledAffix", "rolledAffix"),
                new("rolledValue", "rolledValue"), new("selectedAffix", "selectedAffix"), new("reroll", "reroll"),
                new("pity", "pity"), new("guaranteedAffix", "guaranteedAffix"), new("actualProbability", "actualProbability"),
                new("normalizedChance", "normalizedChance"), new("placementId", "placementId"), new("isLit", "isLit"),
                new("countedInBuild", "countedInBuild"), new("itemPower", "itemPower"), new("Scene", "UnityEngine.SceneManagement"),
                new("Prefab", "Prefab"), new("BuildSettings", "BuildSettings"), new("BuildSandbox reference", "TalismanBag.BuildSandbox")
            };
            foreach (LeakDefinition definition in definitions) { int count = CountOrdinal(text, definition.token); rows.Add(new LeakRow(definition.category, count)); if (count != 0) result.Errors.Add($"LeakCheck '{definition.category}' found {count} occurrence(s)."); }
        }

        private static void RunHistoricalRegressions(VerificationResult result)
        {
            try { ItemRarityInstanceFoundationVerifier.VerifyMenu(); ItemStatRangeSchemaVerifier.VerifyMenu(); ItemCorePotentialAndBuildEligibilitySchemaVerifier.VerifyMenu(); }
            catch (Exception exception) { result.Errors.Add("Historical generation verifier threw: " + exception.Message); }
            RegressionDefinition[] regressions =
            {
                new("Foundation", "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md", "- Verification: PASS"),
                new("StatRange", "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md", "- Overall: PASS"),
                new("CorePotential", "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md", "- Overall: PASS"),
                new("ItemInnerDataCatalog", "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot", "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore", "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview", "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract", "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete", "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting", "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus", "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };
            foreach (RegressionDefinition regression in regressions) { bool pass = File.Exists(regression.path) && File.ReadAllText(regression.path).Contains(regression.marker); result.Regressions.Add(regression.name + ": " + (pass ? "PASS" : "FAIL")); if (!pass) result.Errors.Add("Historical regression report is not PASS: " + regression.name); }
            result.FoundationSpecCount = CsvRows("Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv");
            result.StatSpecCount = CsvRows("Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv");
            result.CoreSpecCount = CsvRows("Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv");
            if (result.FoundationSpecCount != 159) result.Errors.Add($"Foundation spec count {result.FoundationSpecCount}, expected 159.");
            if (result.StatSpecCount != 39) result.Errors.Add($"StatRange spec count {result.StatSpecCount}, expected 39.");
            if (result.CoreSpecCount != 69) result.Errors.Add($"CorePotential spec count {result.CoreSpecCount}, expected 69.");
        }

        private static void WriteReports(ItemAffixPoolAndRangeSchemaSnapshot schema, ItemAffixPoolAndRangeSchemaSnapshot fixture, IReadOnlyList<SpecRow> rows, IReadOnlyList<LeakRow> leaks, VerificationResult result, bool passed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath, BuildReport(schema, fixture, rows, leaks, result, passed), new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(rows), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeakReport(leaks, passed), new UTF8Encoding(false));
            File.WriteAllText(InventoryPath, BuildInventory(schema), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(ItemAffixPoolAndRangeSchemaSnapshot schema, ItemAffixPoolAndRangeSchemaSnapshot fixture, IReadOnlyList<SpecRow> rows, IReadOnlyList<LeakRow> leaks, VerificationResult result, bool passed)
        {
            StringBuilder b = new();
            b.AppendLine("# ItemAffixPoolAndRangeSchema01 Report").AppendLine()
                .AppendLine($"- Package: `{PackageName}`").AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(passed ? PassMarker : "ITEM_AFFIX_POOL_AND_RANGE_SCHEMA01_FAIL")}`")
                .AppendLine($"- Schema ID: `{schema?.schemaId ?? "null"}`").AppendLine($"- Spec: {rows.Count}/{rows.Count} {(rows.All(x => x.result == "PASS") ? "PASS" : "FAIL")}")
                .AppendLine($"- Canonical Signature: `{Compact(schema?.BuildCanonicalSignature())}`").AppendLine()
                .AppendLine("## Default Runtime Catalog").AppendLine()
                .AppendLine($"- Affix Definition: {schema?.AffixDefinitions.Count ?? -1}").AppendLine($"- Value Profile: {schema?.AffixValueProfiles.Count ?? -1}")
                .AppendLine($"- Slot Policy: {schema?.SlotPolicies.Count ?? -1}").AppendLine($"- Random Pool: {schema?.RandomPools.Count ?? -1}")
                .AppendLine($"- Item Affix Profile: {schema?.ItemGenerationProfiles.Count ?? -1}").AppendLine($"- Fixed Binding: {CountBindings(schema)}")
                .AppendLine($"- Defined Weight: {CountDefinedWeights(schema)}").AppendLine($"- Mutex Group: {CountMutex(schema)}")
                .AppendLine("- Status: `SCHEMA_ONLY / UNRESOLVED_DATA_COVERAGE`").AppendLine()
                .AppendLine("## Contract").AppendLine()
                .AppendLine("- Fixed affix fixes the affix type; it does not fix or generate the numeric value.")
                .AppendLine("- Random pool, fixed/random slots, relative weights, mutex groups, and repeat policies are structural only.")
                .AppendLine("- QA fixture is memory-only inside the verifier and is marked `QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA`.")
                .AppendLine("- Existing BuildSandbox affix/rarity code was read only as Code Survey background; no value, evaluator, namespace, or config was promoted.")
                .AppendLine("- Slot counts, formal weights, formal mutex groups, repeat conclusions, 30 item mappings, and formal values remain unresolved.")
                .AppendLine("- No affix selection, value generation, reroll, pity, probability normalization, or seed consumption is implemented.")
                .AppendLine("- Input reversal, source mutation, exposed collection mutation, stable queries, and reflection field checks are covered.").AppendLine()
                .AppendLine("## Fixture Structure").AppendLine()
                .AppendLine($"- Definitions: {fixture?.AffixDefinitions.Count ?? -1}; Profiles: {fixture?.AffixValueProfiles.Count ?? -1}; Slots: {fixture?.SlotPolicies.Sum(x => x.Slots.Count) ?? -1}; Pool Entries: {fixture?.RandomPools.Sum(x => x.Entries.Count) ?? -1}").AppendLine()
                .AppendLine("## Historical Regressions").AppendLine();
            foreach (string regression in result.Regressions) b.AppendLine("- " + regression);
            b.AppendLine($"- Foundation: {result.FoundationSpecCount}/159").AppendLine($"- StatRange: {result.StatSpecCount}/39").AppendLine($"- CorePotential: {result.CoreSpecCount}/69")
                .AppendLine().AppendLine("## LeakCheck").AppendLine().AppendLine($"- Categories: {leaks.Count}; total leaks: {leaks.Sum(x => x.count)}")
                .AppendLine().AppendLine("## Errors").AppendLine();
            if (result.Errors.Count == 0) b.AppendLine("- None."); else foreach (string error in result.Errors) b.AppendLine("- " + error);
            return b.ToString();
        }

        private static string BuildSpec(IEnumerable<SpecRow> rows)
        { StringBuilder b = new("caseId,category,expected,actual,validationCodes,result\n"); foreach (SpecRow row in rows) b.Append(Csv(row.caseId)).Append(',').Append(Csv(row.category)).Append(',').Append(Csv(row.expected)).Append(',').Append(Csv(row.actual)).Append(',').Append(Csv(row.codes)).Append(',').Append(Csv(row.result)).Append('\n'); return b.ToString(); }
        private static string BuildLeakReport(IEnumerable<LeakRow> rows, bool passed)
        { StringBuilder b = new(); b.AppendLine("# ItemAffixPoolAndRangeSchema01 LeakCheck Report").AppendLine().AppendLine($"- Package: `{PackageName}`").AppendLine($"- Runtime scan root: `{RuntimeRoot}`").AppendLine($"- Result: {(passed && rows.All(x => x.count == 0) ? "PASS" : "FAIL")}").AppendLine().AppendLine("| Category | Count |").AppendLine("| --- | ---: |"); foreach (LeakRow row in rows) b.AppendLine($"| {row.category} | {row.count} |"); return b.ToString(); }
        private static string BuildInventory(ItemAffixPoolAndRangeSchemaSnapshot schema)
        { Type[] types = typeof(ItemAffixPoolAndRangeSchemaSnapshot).Assembly.GetTypes().Where(t => t.Namespace == "TalismanBag.Items.Generation.Affixes" && t.IsPublic).OrderBy(t => t.Name, StringComparer.Ordinal).ToArray(); StringBuilder b = new("typeName,typeKind,publicProperties,defaultFormalCount,notes\n"); foreach (Type type in types) { string kind = type.IsEnum ? "enum" : type.IsAbstract && type.IsSealed ? "static" : "snapshot"; string properties = string.Join("|", type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).OrderBy(x => x, StringComparer.Ordinal)); string count = type == typeof(ItemAffixDefinitionSnapshot) ? (schema?.AffixDefinitions.Count ?? -1).ToString(CultureInfo.InvariantCulture) : type == typeof(ItemAffixValueProfileSnapshot) ? (schema?.AffixValueProfiles.Count ?? -1).ToString(CultureInfo.InvariantCulture) : type == typeof(ItemAffixSlotPolicySnapshot) ? (schema?.SlotPolicies.Count ?? -1).ToString(CultureInfo.InvariantCulture) : type == typeof(ItemRandomAffixPoolSnapshot) ? (schema?.RandomPools.Count ?? -1).ToString(CultureInfo.InvariantCulture) : type == typeof(ItemAffixGenerationProfileSnapshot) ? (schema?.ItemGenerationProfiles.Count ?? -1).ToString(CultureInfo.InvariantCulture) : ""; b.Append(Csv(type.Name)).Append(',').Append(Csv(kind)).Append(',').Append(Csv(properties)).Append(',').Append(Csv(count)).Append(',').Append(Csv("Read-only schema; no generated result state.")).Append('\n'); } return b.ToString(); }

        private static int CountOrdinal(string text, string token) { int count = 0, index = 0; while (!string.IsNullOrEmpty(token) && (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0) { count++; index += token.Length; } return count; }
        private static int CsvRows(string path) => File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;
        private static string Compact(string value) { if (string.IsNullOrEmpty(value)) return string.Empty; string oneLine = value.Replace("\r", "").Replace("\n", " / "); return oneLine.Length <= 180 ? oneLine : oneLine.Substring(0, 180) + "..."; }
        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

        private sealed class Fixture
        {
            public readonly List<ItemAffixDefinitionSnapshot> Definitions = new();
            public readonly List<ItemAffixValueProfileSnapshot> ValueProfiles = new();
            public readonly List<ItemAffixSlotPolicySnapshot> SlotPolicies = new();
            public readonly List<ItemRandomAffixPoolSnapshot> Pools = new();
            public readonly List<ItemAffixGenerationProfileSnapshot> GenerationProfiles = new();
            public ItemAffixPoolAndRangeSchemaSnapshot Create(bool reverse = false) => ItemAffixPoolAndRangeSchema.Create(reverse ? Definitions.AsEnumerable().Reverse() : Definitions, reverse ? ValueProfiles.AsEnumerable().Reverse() : ValueProfiles, reverse ? SlotPolicies.AsEnumerable().Reverse() : SlotPolicies, reverse ? Pools.AsEnumerable().Reverse() : Pools, reverse ? GenerationProfiles.AsEnumerable().Reverse() : GenerationProfiles);
        }
        private sealed class VerificationResult { public readonly List<string> Errors = new(); public readonly List<string> Regressions = new(); public int FoundationSpecCount; public int StatSpecCount; public int CoreSpecCount; }
        private readonly struct SpecRow { public SpecRow(string caseId, string category, string expected, string actual, string codes, string result) { this.caseId = caseId; this.category = category; this.expected = expected; this.actual = actual; this.codes = codes; this.result = result; } public readonly string caseId, category, expected, actual, codes, result; }
        private readonly struct LeakRow { public LeakRow(string category, int count) { this.category = category; this.count = count; } public readonly string category; public readonly int count; }
        private readonly struct LeakDefinition { public LeakDefinition(string category, string token) { this.category = category; this.token = token; } public readonly string category, token; }
        private readonly struct RegressionDefinition { public RegressionDefinition(string name, string path, string marker) { this.name = name; this.path = path; this.marker = marker; } public readonly string name, path, marker; }
    }
}
#endif
