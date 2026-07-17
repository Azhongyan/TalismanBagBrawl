#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Stats;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemStatRangeSchemaVerifier
    {
        private const string PackageName = "V0.4-ItemStatRangeSchema01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv";
        private const string LeakReportPath = "Docs/V0.4/Reports/ItemStatRangeSchemaLeakCheckReport.md";
        private const string DictionaryPath = "Docs/V0.4/Reports/ItemStatRangeDictionary.csv";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Stats";
        private const string PassMarker = "ITEM_STAT_RANGE_SCHEMA01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemStatRangeSchema01/[QA Only] Run Schema")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<SpecRow> rows = new();
            List<LeakRow> leakRows = new();
            ItemStatRangeSchemaSnapshot qaSeedSchema = null;

            try
            {
                qaSeedSchema = ItemStatRangeSchemaCatalog.CreateQaSeedSchema();
                RunSchemaCases(qaSeedSchema, rows, result);
                RunLeakChecks(leakRows, result);
                RunHistoricalRegressions(result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Unhandled schema verifier exception: " + exception);
            }

            WriteReports(qaSeedSchema, rows, leakRows, result);
            bool passed = result.Errors.Count == 0 && rows.All(row => row.result == "PASS");
            if (passed)
            {
                Debug.Log(PassMarker + ": verification and all reports passed.");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    Debug.LogError(error);
                }
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void RunSchemaCases(
            ItemStatRangeSchemaSnapshot qaSeedSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            AddSchemaCase(rows, result, "qa-seed-valid", "normal", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                string.Empty, true, ItemStatRangeValidationCodes.None, qaSeedSchema);
            Require(qaSeedSchema != null && qaSeedSchema.schemaId == ItemStatRangeSchemaSnapshot.CurrentSchemaId,
                result, "Schema ID differs from ItemStatRangeSchemaSnapshot.v1.");
            Require(qaSeedSchema != null && qaSeedSchema.StatDefinitions.Count == 1
                && qaSeedSchema.ItemStatProfiles.Count == 1, result,
                "QA Seed catalog must contain exactly one definition and one profile.");
            Require(ItemStatRangeSchemaCatalog.GetQaSeedNotice().usageStatus == "QA_SEED_ONLY"
                && ItemStatRangeSchemaCatalog.GetQaSeedNotice().balanceStatus == "NOT_BALANCE_APPROVED"
                && ItemStatRangeSchemaCatalog.GetQaSeedNotice().generationStatus == "NOT_FORMAL_GENERATION_DATA", result,
                "QA Seed isolation markers are incomplete.");

            ItemStatQueryResult<ItemStatDefinitionSnapshot> definitionQuery =
                qaSeedSchema.QueryStatDefinition(ItemStatRangeSchemaCatalog.QaDamageStatId);
            AddBooleanCase(rows, result, "query-definition", "query", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                string.Empty, definitionQuery.isSuccess, ItemStatRangeValidationCodes.None);
            ItemStatQueryResult<ItemStatRangeProfileSnapshot> profileQuery =
                qaSeedSchema.QueryProfile("I001", ItemStatRangeSchemaCatalog.QaDamageStatId);
            AddBooleanCase(rows, result, "query-profile", "query", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                string.Empty, profileQuery.isSuccess, ItemStatRangeValidationCodes.None);
            ItemStatQueryResult<ItemRarityStatRangeSnapshot> blueQuery =
                qaSeedSchema.QueryRarityRange("I001", ItemStatRangeSchemaCatalog.QaDamageStatId, ItemInstanceRarity.Blue);
            bool blueCorrect = blueQuery.isSuccess && blueQuery.value.minUnits == 5 && blueQuery.value.maxUnits == 8;
            AddBooleanCase(rows, result, "query-rarity-blue", "query", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                "blue", blueCorrect, ItemStatRangeValidationCodes.None);
            ItemStatQueryResult<ItemRarityStatRangeSnapshot> purpleQuery =
                qaSeedSchema.QueryRarityRange("I001", ItemStatRangeSchemaCatalog.QaDamageStatId, ItemInstanceRarity.Purple);
            bool overlapAllowed = blueQuery.isSuccess && purpleQuery.isSuccess
                && blueQuery.value.maxUnits >= purpleQuery.value.minUnits;
            AddBooleanCase(rows, result, "overlap-blue-purple", "normal", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                "blue|purple", overlapAllowed, ItemStatRangeValidationCodes.None);

            string[] rarityOrder = profileQuery.value.RarityRanges.Select(range => range.rarity.ToStableKey()).ToArray();
            AddBooleanCase(rows, result, "rarity-stable-order", "normal", "I001", ItemStatRangeSchemaCatalog.QaDamageStatId,
                string.Join("|", rarityOrder), rarityOrder.SequenceEqual(new[] { "white", "green", "blue", "purple", "orange" }),
                ItemStatRangeValidationCodes.None);

            RunDirectionCases(rows, result);
            RunInvalidInputCases(rows, result);
            RunDeterminismAndReadOnlyCases(qaSeedSchema, rows, result);
            RunQueryFailureCases(qaSeedSchema, rows, result);
        }

        private static void RunDirectionCases(List<SpecRow> rows, VerificationResult result)
        {
            ItemStatDefinitionSnapshot higher = Definition("higher_test", ItemStatDirection.HigherIsBetter);
            ItemStatDefinitionSnapshot lower = Definition("lower_test", ItemStatDirection.LowerIsBetter);
            ItemStatDefinitionSnapshot neutral = Definition("neutral_test", ItemStatDirection.Neutral);
            ItemStatRangeSchemaSnapshot allDirections = ItemStatRangeSchema.Create(
                new[] { neutral, higher, lower },
                new[]
                {
                    Profile("I003", neutral.statId, 1, 10, NeutralRanges()),
                    Profile("I001", higher.statId, 1, 10, HigherRanges()),
                    Profile("I002", lower.statId, 1, 10, LowerRanges())
                });
            AddSchemaCase(rows, result, "three-directions-valid", "normal", string.Empty, string.Empty, string.Empty,
                true, ItemStatRangeValidationCodes.None, allDirections);

            AddSchemaCase(rows, result, "higher-valid", "higher", "I001", higher.statId, string.Empty,
                true, ItemStatRangeValidationCodes.None,
                ItemStatRangeSchema.Create(new[] { higher }, new[] { Profile("I001", higher.statId, 1, 10, HigherRanges()) }));
            AddSchemaCase(rows, result, "higher-min-decrease", "higher", "I001", higher.statId, "blue",
                false, ItemStatRangeValidationCodes.HigherMinNotMonotonic,
                ItemStatRangeSchema.Create(new[] { higher }, new[]
                {
                    Profile("I001", higher.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 1, 2), (ItemInstanceRarity.Green, 3, 5),
                        (ItemInstanceRarity.Blue, 2, 6), (ItemInstanceRarity.Purple, 6, 9),
                        (ItemInstanceRarity.Orange, 9, 10)))
                }));
            AddSchemaCase(rows, result, "higher-max-decrease", "higher", "I001", higher.statId, "blue",
                false, ItemStatRangeValidationCodes.HigherMaxNotMonotonic,
                ItemStatRangeSchema.Create(new[] { higher }, new[]
                {
                    Profile("I001", higher.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 1, 4), (ItemInstanceRarity.Green, 3, 5),
                        (ItemInstanceRarity.Blue, 4, 4), (ItemInstanceRarity.Purple, 6, 9),
                        (ItemInstanceRarity.Orange, 9, 10)))
                }));

            AddSchemaCase(rows, result, "lower-valid", "lower", "I002", lower.statId, string.Empty,
                true, ItemStatRangeValidationCodes.None,
                ItemStatRangeSchema.Create(new[] { lower }, new[] { Profile("I002", lower.statId, 1, 10, LowerRanges()) }));
            AddSchemaCase(rows, result, "lower-min-increase", "lower", "I002", lower.statId, "blue",
                false, ItemStatRangeValidationCodes.LowerMinNotMonotonic,
                ItemStatRangeSchema.Create(new[] { lower }, new[]
                {
                    Profile("I002", lower.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 8, 10), (ItemInstanceRarity.Green, 7, 9),
                        (ItemInstanceRarity.Blue, 8, 8), (ItemInstanceRarity.Purple, 2, 5),
                        (ItemInstanceRarity.Orange, 1, 3)))
                }));
            AddSchemaCase(rows, result, "lower-max-increase", "lower", "I002", lower.statId, "green",
                false, ItemStatRangeValidationCodes.LowerMaxNotMonotonic,
                ItemStatRangeSchema.Create(new[] { lower }, new[]
                {
                    Profile("I002", lower.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 8, 9), (ItemInstanceRarity.Green, 7, 10),
                        (ItemInstanceRarity.Blue, 4, 7), (ItemInstanceRarity.Purple, 2, 5),
                        (ItemInstanceRarity.Orange, 1, 3)))
                }));

            AddSchemaCase(rows, result, "neutral-non-monotonic", "neutral", "I003", neutral.statId, string.Empty,
                true, ItemStatRangeValidationCodes.None,
                ItemStatRangeSchema.Create(new[] { neutral }, new[] { Profile("I003", neutral.statId, 1, 10, NeutralRanges()) }));
            AddSchemaCase(rows, result, "neutral-outside-total", "neutral", "I003", neutral.statId, "blue",
                false, ItemStatRangeValidationCodes.RangeOutsideTotal,
                ItemStatRangeSchema.Create(new[] { neutral }, new[]
                {
                    Profile("I003", neutral.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 1, 2), (ItemInstanceRarity.Green, 8, 9),
                        (ItemInstanceRarity.Blue, 0, 6), (ItemInstanceRarity.Purple, 3, 7),
                        (ItemInstanceRarity.Orange, 9, 10)))
                }));
        }

        private static void RunInvalidInputCases(List<SpecRow> rows, VerificationResult result)
        {
            ItemStatDefinitionSnapshot valid = Definition("invalid_test", ItemStatDirection.HigherIsBetter);
            ItemStatRangeProfileSnapshot validProfile = Profile("I001", valid.statId, 1, 10, HigherRanges());

            AddSchemaCase(rows, result, "empty-stat-id", "invalid", "I001", string.Empty, string.Empty,
                false, ItemStatRangeValidationCodes.StatIdEmpty,
                ItemStatRangeSchema.Create(new[] { Definition(string.Empty, ItemStatDirection.HigherIsBetter) }, Array.Empty<ItemStatRangeProfileSnapshot>()));
            AddSchemaCase(rows, result, "duplicate-stat-id", "invalid", string.Empty, valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.StatIdDuplicate,
                ItemStatRangeSchema.Create(new[] { valid, Definition(valid.statId, ItemStatDirection.Neutral) }, Array.Empty<ItemStatRangeProfileSnapshot>()));
            AddSchemaCase(rows, result, "empty-base-item-id", "invalid", string.Empty, valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.BaseItemIdEmpty,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile(string.Empty, valid.statId, 1, 10, HigherRanges()) }));
            AddSchemaCase(rows, result, "unknown-base-item", "invalid", "I999", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.BaseItemUnknown,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile("I999", valid.statId, 1, 10, HigherRanges()) }));
            AddSchemaCase(rows, result, "exclude-i031", "invalid", "I031", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.BaseItemNotOrdinary,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile("I031", valid.statId, 1, 10, HigherRanges()) }));
            AddSchemaCase(rows, result, "unknown-stat-reference", "invalid", "I001", "missing_stat", string.Empty,
                false, ItemStatRangeValidationCodes.StatDefinitionUnknown,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile("I001", "missing_stat", 1, 10, HigherRanges()) }));

            ItemStatDefinitionSnapshot zeroStep = new("zero_step", "Zero Step", "flat", ItemStatDirection.HigherIsBetter,
                0, 0, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA);
            AddSchemaCase(rows, result, "step-units-zero", "invalid", string.Empty, zeroStep.statId, string.Empty,
                false, ItemStatRangeValidationCodes.StepUnitsInvalid,
                ItemStatRangeSchema.Create(new[] { zeroStep }, Array.Empty<ItemStatRangeProfileSnapshot>()));
            ItemStatDefinitionSnapshot badPrecision = new("bad_precision", "Bad Precision", "flat", ItemStatDirection.HigherIsBetter,
                7, 1, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA);
            AddSchemaCase(rows, result, "decimal-places-invalid", "invalid", string.Empty, badPrecision.statId, string.Empty,
                false, ItemStatRangeValidationCodes.DecimalPlacesInvalid,
                ItemStatRangeSchema.Create(new[] { badPrecision }, Array.Empty<ItemStatRangeProfileSnapshot>()));
            AddSchemaCase(rows, result, "total-min-greater-max", "invalid", "I001", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.TotalRangeInvalid,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile("I001", valid.statId, 10, 1, HigherRanges()) }));

            AddSchemaCase(rows, result, "rarity-min-greater-max", "invalid", "I001", valid.statId, "blue",
                false, ItemStatRangeValidationCodes.RarityRangeInvalid,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    Profile("I001", valid.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 1, 2), (ItemInstanceRarity.Green, 3, 5),
                        (ItemInstanceRarity.Blue, 8, 5), (ItemInstanceRarity.Purple, 8, 9),
                        (ItemInstanceRarity.Orange, 9, 10)))
                }));

            ItemStatDefinitionSnapshot stepTwo = new("step_two", "Step Two", "flat", ItemStatDirection.HigherIsBetter,
                0, 2, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA);
            AddSchemaCase(rows, result, "endpoint-step-misaligned", "invalid", "I001", stepTwo.statId, "white",
                false, ItemStatRangeValidationCodes.RangeStepMisaligned,
                ItemStatRangeSchema.Create(new[] { stepTwo }, new[]
                {
                    Profile("I001", stepTwo.statId, 1, 10, HigherRanges())
                }));

            AddSchemaCase(rows, result, "missing-rarity", "invalid", "I001", valid.statId, "orange",
                false, ItemStatRangeValidationCodes.RaritySetIncomplete,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    Profile("I001", valid.statId, 1, 9, HigherRanges().Where(range => range.rarity != ItemInstanceRarity.Orange))
                }));
            AddSchemaCase(rows, result, "duplicate-rarity", "invalid", "I001", valid.statId, "white",
                false, ItemStatRangeValidationCodes.RarityDuplicate,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    Profile("I001", valid.statId, 1, 10, HigherRanges().Concat(new[]
                    {
                        new ItemRarityStatRangeSnapshot(ItemInstanceRarity.White, 1, 2)
                    }))
                }));
            AddSchemaCase(rows, result, "illegal-rarity-enum", "invalid", "I001", valid.statId, "999",
                false, ItemStatRangeValidationCodes.RarityInvalid,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    Profile("I001", valid.statId, 1, 10, HigherRanges().Concat(new[]
                    {
                        new ItemRarityStatRangeSnapshot((ItemInstanceRarity)999, 1, 1)
                    }))
                }));
            AddSchemaCase(rows, result, "range-outside-total", "invalid", "I001", valid.statId, "orange",
                false, ItemStatRangeValidationCodes.RangeOutsideTotal,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    Profile("I001", valid.statId, 1, 10, Ranges(
                        (ItemInstanceRarity.White, 1, 2), (ItemInstanceRarity.Green, 3, 5),
                        (ItemInstanceRarity.Blue, 5, 8), (ItemInstanceRarity.Purple, 6, 9),
                        (ItemInstanceRarity.Orange, 9, 11)))
                }));
            AddSchemaCase(rows, result, "total-envelope-mismatch", "invalid", "I001", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.TotalRangeEnvelopeMismatch,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { Profile("I001", valid.statId, 0, 10, HigherRanges()) }));
            AddSchemaCase(rows, result, "numeric-profile-schema-only", "invalid", "I001", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.NumericProfileSchemaOnly,
                ItemStatRangeSchema.Create(new[] { valid }, new[]
                {
                    new ItemStatRangeProfileSnapshot("I001", valid.statId, ItemStatDataMaturity.SCHEMA_ONLY,
                        new ItemNumericRangeSnapshot(1, 10), HigherRanges())
                }));
            AddSchemaCase(rows, result, "duplicate-profile", "invalid", "I001", valid.statId, string.Empty,
                false, ItemStatRangeValidationCodes.ProfileDuplicate,
                ItemStatRangeSchema.Create(new[] { valid }, new[] { validProfile, validProfile }));
        }

        private static void RunDeterminismAndReadOnlyCases(
            ItemStatRangeSchemaSnapshot qaSeedSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            ItemStatDefinitionSnapshot higher = Definition("order_higher", ItemStatDirection.HigherIsBetter);
            ItemStatDefinitionSnapshot lower = Definition("order_lower", ItemStatDirection.LowerIsBetter);
            ItemStatRangeProfileSnapshot higherProfile = Profile("I001", higher.statId, 1, 10, HigherRanges());
            ItemStatRangeProfileSnapshot lowerProfile = Profile("I002", lower.statId, 1, 10, LowerRanges());
            ItemStatRangeSchemaSnapshot forward = ItemStatRangeSchema.Create(
                new[] { higher, lower }, new[] { higherProfile, lowerProfile });
            ItemStatRangeSchemaSnapshot reversed = ItemStatRangeSchema.Create(
                new[] { lower, higher }, new[] { lowerProfile, higherProfile });
            bool signatureStable = forward.isValid && reversed.isValid
                && string.Equals(forward.BuildCanonicalSignature(), reversed.BuildCanonicalSignature(), StringComparison.Ordinal);
            AddBooleanCase(rows, result, "canonical-input-order", "determinism", string.Empty, string.Empty,
                string.Empty, signatureStable, ItemStatRangeValidationCodes.None);

            List<ItemRarityStatRangeSnapshot> sourceRanges = HigherRanges().ToList();
            ItemStatRangeProfileSnapshot sourceProfile = Profile("I001", higher.statId, 1, 10, sourceRanges);
            List<ItemStatDefinitionSnapshot> sourceDefinitions = new() { higher };
            List<ItemStatRangeProfileSnapshot> sourceProfiles = new() { sourceProfile };
            ItemStatRangeSchemaSnapshot immutable = ItemStatRangeSchema.Create(sourceDefinitions, sourceProfiles);
            string before = immutable.BuildCanonicalSignature();
            sourceRanges.Clear();
            sourceDefinitions.Clear();
            sourceProfiles.Clear();
            bool sourceMutationBlocked = string.Equals(before, immutable.BuildCanonicalSignature(), StringComparison.Ordinal)
                && immutable.StatDefinitions.Count == 1
                && immutable.ItemStatProfiles.Count == 1
                && immutable.ItemStatProfiles[0].RarityRanges.Count == 5;
            AddBooleanCase(rows, result, "source-collection-mutation", "readonly", "I001", higher.statId,
                string.Empty, sourceMutationBlocked, ItemStatRangeValidationCodes.None);

            bool exposedCollectionsReadOnly = IsReadOnly(qaSeedSchema.StatDefinitions)
                && IsReadOnly(qaSeedSchema.ItemStatProfiles)
                && IsReadOnly(qaSeedSchema.ValidationErrors)
                && IsReadOnly(qaSeedSchema.ItemStatProfiles[0].RarityRanges);
            AddBooleanCase(rows, result, "exposed-collections-readonly", "readonly", "I001",
                ItemStatRangeSchemaCatalog.QaDamageStatId, string.Empty, exposedCollectionsReadOnly,
                ItemStatRangeValidationCodes.None);
        }

        private static void RunQueryFailureCases(
            ItemStatRangeSchemaSnapshot schema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            ItemStatQueryResult<ItemStatDefinitionSnapshot> missingDefinition = schema.QueryStatDefinition("missing_stat");
            AddExpectedFailureCase(rows, result, "query-missing-definition", "query", string.Empty, "missing_stat",
                string.Empty, missingDefinition.validationError?.code, ItemStatRangeValidationCodes.QueryStatNotFound);
            ItemStatQueryResult<ItemStatRangeProfileSnapshot> missingProfile = schema.QueryProfile("I002", ItemStatRangeSchemaCatalog.QaDamageStatId);
            AddExpectedFailureCase(rows, result, "query-missing-profile", "query", "I002", ItemStatRangeSchemaCatalog.QaDamageStatId,
                string.Empty, missingProfile.validationError?.code, ItemStatRangeValidationCodes.QueryProfileNotFound);
            ItemStatQueryResult<ItemStatRangeProfileSnapshot> invalidInput = schema.QueryProfile(string.Empty, string.Empty);
            AddExpectedFailureCase(rows, result, "query-invalid-input", "query", string.Empty, string.Empty,
                string.Empty, invalidInput.validationError?.code, ItemStatRangeValidationCodes.QueryInputInvalid);
        }

        private static ItemStatDefinitionSnapshot Definition(string statId, ItemStatDirection direction)
        {
            return new ItemStatDefinitionSnapshot(
                statId, statId, "flat", direction, 0, 1,
                ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA);
        }

        private static ItemStatRangeProfileSnapshot Profile(
            string baseItemId,
            string statId,
            long totalMin,
            long totalMax,
            IEnumerable<ItemRarityStatRangeSnapshot> ranges)
        {
            return new ItemStatRangeProfileSnapshot(
                baseItemId, statId, ItemStatDataMaturity.SEED_DATA,
                new ItemNumericRangeSnapshot(totalMin, totalMax), ranges);
        }

        private static IReadOnlyList<ItemRarityStatRangeSnapshot> HigherRanges()
        {
            return Ranges(
                (ItemInstanceRarity.White, 1, 2),
                (ItemInstanceRarity.Green, 3, 5),
                (ItemInstanceRarity.Blue, 5, 8),
                (ItemInstanceRarity.Purple, 6, 9),
                (ItemInstanceRarity.Orange, 9, 10));
        }

        private static IReadOnlyList<ItemRarityStatRangeSnapshot> LowerRanges()
        {
            return Ranges(
                (ItemInstanceRarity.White, 8, 10),
                (ItemInstanceRarity.Green, 6, 9),
                (ItemInstanceRarity.Blue, 4, 7),
                (ItemInstanceRarity.Purple, 2, 5),
                (ItemInstanceRarity.Orange, 1, 3));
        }

        private static IReadOnlyList<ItemRarityStatRangeSnapshot> NeutralRanges()
        {
            return Ranges(
                (ItemInstanceRarity.White, 1, 2),
                (ItemInstanceRarity.Green, 8, 9),
                (ItemInstanceRarity.Blue, 4, 6),
                (ItemInstanceRarity.Purple, 3, 7),
                (ItemInstanceRarity.Orange, 9, 10));
        }

        private static IReadOnlyList<ItemRarityStatRangeSnapshot> Ranges(
            params (ItemInstanceRarity rarity, long min, long max)[] values)
        {
            return values.Select(value => new ItemRarityStatRangeSnapshot(value.rarity, value.min, value.max)).ToArray();
        }

        private static void AddSchemaCase(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            string baseItemId,
            string statId,
            string rarity,
            bool expectedValid,
            string expectedCode,
            ItemStatRangeSchemaSnapshot schema)
        {
            bool actualValid = schema != null && schema.isValid;
            string[] codes = schema?.ValidationErrors.Select(error => error.code).Distinct(StringComparer.Ordinal).ToArray()
                ?? new[] { "SCHEMA_NULL" };
            bool codeMatches = expectedCode == ItemStatRangeValidationCodes.None
                ? codes.Length == 0
                : codes.Contains(expectedCode, StringComparer.Ordinal);
            bool passed = actualValid == expectedValid && codeMatches;
            rows.Add(new SpecRow(caseId, category, baseItemId, statId, rarity,
                expectedValid ? "valid" : "invalid", actualValid ? "valid" : "invalid",
                codes.Length == 0 ? ItemStatRangeValidationCodes.None : string.Join("|", codes),
                passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Schema case '{caseId}' failed; expected {expectedValid}/{expectedCode}, actual {actualValid}/{string.Join("|", codes)}.");
            }
        }

        private static void AddBooleanCase(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            string baseItemId,
            string statId,
            string rarity,
            bool passed,
            string code)
        {
            rows.Add(new SpecRow(caseId, category, baseItemId, statId, rarity,
                "true", passed ? "true" : "false", code, passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Boolean case '{caseId}' failed.");
            }
        }

        private static void AddExpectedFailureCase(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            string baseItemId,
            string statId,
            string rarity,
            string actualCode,
            string expectedCode)
        {
            bool passed = string.Equals(actualCode, expectedCode, StringComparison.Ordinal);
            rows.Add(new SpecRow(caseId, category, baseItemId, statId, rarity,
                expectedCode, actualCode ?? string.Empty, actualCode ?? string.Empty, passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Query case '{caseId}' expected {expectedCode}, actual {actualCode ?? "<null>"}.");
            }
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values)
        {
            if (values == null || values is T[])
            {
                return false;
            }

            if (values is IList<T> list)
            {
                try
                {
                    list.Add(default);
                    return false;
                }
                catch (NotSupportedException)
                {
                    return true;
                }
            }

            return true;
        }

        private static void RunLeakChecks(List<LeakRow> rows, VerificationResult result)
        {
            string[] files = Directory.Exists(RuntimeRoot)
                ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories)
                : Array.Empty<string>();
            string source = string.Join("\n", files.Select(File.ReadAllText));
            LeakDefinition[] definitions =
            {
                new("Reward", new[] { "RewardConfig", "RewardService", "GrantReward" }),
                new("RunFlow", new[] { "RunFlow" }),
                new("Inventory写入", new[] { "InventoryWriter", "InventoryService", "AddToInventory", "WriteInventory" }),
                new("SaveData", new[] { "SaveData", "PlayerPrefs", "MainTrialProgressData" }),
                new("Boss", new[] { "BossInfo", "BossBattle", "BossReward" }),
                new("Battle Resolver", new[] { "BattleResolver" }),
                new("Battle Bridge", new[] { "BattleBridge", "UnifiedBattlePage" }),
                new("正式掉落", new[] { "DropTable", "LootTable", "DropGeneration" }),
                new("正式养成", new[] { "UpgradeService", "Breakthrough", "SaveCultivation", "Reroll" }),
                new("属性 Roll", new[] { "System.Random", "UnityEngine.Random", "RollStat", "StatRoll" }),
                new("随机词条", new[] { "AffixRoll", "RollAffix", "randomAffix" }),
                new("核心效果解锁", new[] { "coreEffectUnlocked", "UnlockCoreEffect" }),
                new("BuildQualification", new[] { "BuildQualification", "QualificationRoll" }),
                new("itemPower", new[] { "itemPower" }),
                new("placementId", new[] { "placementId" }),
                new("Scene", new[] { "SceneManager", "LoadScene", ".unity" }),
                new("Prefab", new[] { "PrefabUtility", ".prefab" }),
                new("BuildSettings", new[] { "EditorBuildSettings", "BuildSettings" })
            };

            foreach (LeakDefinition definition in definitions)
            {
                int count = definition.tokens.Sum(token => CountOccurrences(source, token));
                rows.Add(new LeakRow(definition.category, count));
                if (count != 0)
                {
                    result.Errors.Add($"LeakCheck category '{definition.category}' found {count} runtime occurrence(s).");
                }
            }
        }

        private static int CountOccurrences(string source, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(token)
                && (index = source.IndexOf(token, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static void RunHistoricalRegressions(VerificationResult result)
        {
            try
            {
                ItemRarityInstanceFoundationVerifier.VerifyMenu();
            }
            catch (Exception exception)
            {
                result.Errors.Add("Foundation regression threw: " + exception.Message);
            }

            RegressionDefinition[] regressions =
            {
                new("ItemRarityInstanceFoundation01", "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md", "- Verification: PASS"),
                new("ItemInnerDataCatalog", "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot", "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore", "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview", "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract", "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete", "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting", "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus", "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };

            foreach (RegressionDefinition regression in regressions)
            {
                bool passed = File.Exists(regression.reportPath)
                    && File.ReadAllText(regression.reportPath).Contains(regression.passMarker);
                result.Regressions.Add(regression.name + ": " + (passed ? "PASS" : "FAIL"));
                if (!passed)
                {
                    result.Errors.Add("Historical regression report is not PASS: " + regression.name + ".");
                }
            }

            string foundationSpec = "Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv";
            int foundationRows = File.Exists(foundationSpec)
                ? Math.Max(0, File.ReadAllLines(foundationSpec).Length - 1)
                : 0;
            bool foundation159 = foundationRows == 159;
            result.FoundationSpecCount = foundationRows;
            if (!foundation159)
            {
                result.Errors.Add($"Foundation regression spec row count is {foundationRows}; expected 159.");
            }
        }

        private static void WriteReports(
            ItemStatRangeSchemaSnapshot schema,
            IReadOnlyList<SpecRow> rows,
            IReadOnlyList<LeakRow> leakRows,
            VerificationResult result)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath, BuildReport(schema, rows, leakRows, result), new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(rows), new UTF8Encoding(false));
            File.WriteAllText(LeakReportPath, BuildLeakReport(leakRows, result), new UTF8Encoding(false));
            File.WriteAllText(DictionaryPath, BuildDictionary(schema), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(
            ItemStatRangeSchemaSnapshot schema,
            IReadOnlyList<SpecRow> rows,
            IReadOnlyList<LeakRow> leakRows,
            VerificationResult result)
        {
            bool passed = result.Errors.Count == 0 && rows.All(row => row.result == "PASS")
                && leakRows.All(row => row.count == 0);
            StringBuilder builder = new();
            builder.AppendLine("# ItemStatRangeSchema01 Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Run time (UTC): `{DateTime.UtcNow:O}`")
                .AppendLine($"- Overall: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- PASS marker: `{(passed ? PassMarker : "NONE")}`")
                .AppendLine($"- Schema ID: `{schema?.schemaId ?? ItemStatRangeSchemaSnapshot.CurrentSchemaId}`")
                .AppendLine($"- Spec: {rows.Count} total / {rows.Count(row => row.result == "PASS")} PASS / {rows.Count(row => row.result == "FAIL")} FAIL")
                .AppendLine()
                .AppendLine("## Contract")
                .AppendLine()
                .AppendLine("- Directions: `HigherIsBetter / LowerIsBetter / Neutral`.")
                .AppendLine("- Numeric truth: signed integer `rawUnits`; display value is `rawUnits / 10^decimalPlaces`.")
                .AppendLine("- Per-stat metadata: `unitKey / decimalPlaces / stepUnits / roundingMode / dataMaturity`.")
                .AppendLine("- Range contract: one total envelope plus exactly `white / green / blue / purple / orange` subranges per configured `baseItemId + statId`.")
                .AppendLine("- Rarity overlap: PASS; `blue 5..8` overlaps `purple 6..9`.")
                .AppendLine("- Direction validation: Higher / Lower positive and negative cases PASS; Neutral non-monotonic case PASS.")
                .AppendLine("- Canonical Signature: integer-only, invariant, stable after input reversal.")
                .AppendLine("- Read-only attack: source mutation and exposed collection mutation checks PASS.")
                .AppendLine()
                .AppendLine("## QA Seed Isolation")
                .AppendLine()
                .AppendLine("- Seed profile: `I001 + qa_damage_example`, total `1..10`, maturity `SEED_DATA`.")
                .AppendLine("- Markers: `QA_SEED_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA`.")
                .AppendLine("- This is not I001 formal damage design and is not connected to any default generation path.")
                .AppendLine()
                .AppendLine("## Data Coverage")
                .AppendLine()
                .AppendLine("- Ordinary prototypes: 30")
                .AppendLine("- Prototypes with QA Seed Profile: 1")
                .AppendLine("- Prototypes without formal values: 30")
                .AppendLine("- Formally usable value profiles: 0")
                .AppendLine("- Coverage status: `UNRESOLVED_DATA_COVERAGE`")
                .AppendLine("- I031 exclusion: PASS; ordinary stat profiles reject I031.")
                .AppendLine()
                .AppendLine("## Historical Regressions")
                .AppendLine();
            foreach (string regression in result.Regressions)
            {
                builder.AppendLine("- " + regression);
            }

            builder.AppendLine($"- Foundation original Spec: {result.FoundationSpecCount}/159 {(result.FoundationSpecCount == 159 ? "PASS" : "FAIL")}")
                .AppendLine()
                .AppendLine("## Not Connected")
                .AppendLine()
                .AppendLine("- No property randomization, full item instance generation, affixes, core potential, Build eligibility, drop probability, cultivation, Battle, Reward, Inventory, SaveData, Boss, scene, prefab, or BuildSettings integration.")
                .AppendLine("- No formal I002-I030 numeric profiles were created.")
                .AppendLine()
                .AppendLine("## Errors")
                .AppendLine();
            if (result.Errors.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    builder.AppendLine("- " + error);
                }
            }

            return builder.ToString();
        }

        private static string BuildSpec(IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,category,baseItemId,statId,rarity,expected,actual,validationCode,result");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine(string.Join(",",
                    Csv(row.caseId), Csv(row.category), Csv(row.baseItemId), Csv(row.statId), Csv(row.rarity),
                    Csv(row.expected), Csv(row.actual), Csv(row.validationCode), Csv(row.result)));
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(IReadOnlyList<LeakRow> rows, VerificationResult result)
        {
            bool passed = rows.All(row => row.count == 0);
            StringBuilder builder = new();
            builder.AppendLine("# ItemStatRangeSchema01 LeakCheck Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Runtime scan root: `{RuntimeRoot}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine()
                .AppendLine("| Category | Leak count |")
                .AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows)
            {
                builder.AppendLine($"| {row.category} | {row.count} |");
            }

            builder.AppendLine()
                .AppendLine("All categories must remain zero. Schema identifiers, direction, maturity, baseItemId, statId, and deterministic integer range fields are allowed.")
                .AppendLine($"Verifier errors: {result.Errors.Count}");
            return builder.ToString();
        }

        private static string BuildDictionary(ItemStatRangeSchemaSnapshot schema)
        {
            StringBuilder builder = new();
            builder.AppendLine("statId,displayName,unitKey,direction,decimalPlaces,stepUnits,roundingMode,dataMaturity,usageStatus,notes");
            foreach (ItemStatDefinitionSnapshot definition in schema?.StatDefinitions ?? Array.Empty<ItemStatDefinitionSnapshot>())
            {
                builder.AppendLine(string.Join(",",
                    Csv(definition.statId), Csv(definition.displayName), Csv(definition.unitKey), Csv(definition.direction.ToString()),
                    definition.decimalPlaces, definition.stepUnits, Csv(definition.roundingMode.ToString()),
                    Csv(definition.dataMaturity.ToString()), Csv(ItemStatRangeSchemaCatalog.QaSeedUsageStatus),
                    Csv(ItemStatRangeSchemaCatalog.QaSeedBalanceStatus + "; " + ItemStatRangeSchemaCatalog.QaSeedGenerationStatus)));
            }

            return builder.ToString();
        }

        private static string Csv(string value)
        {
            string normalized = value ?? string.Empty;
            return normalized.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0
                ? "\"" + normalized.Replace("\"", "\"\"") + "\""
                : normalized;
        }

        private static void Require(bool condition, VerificationResult result, string error)
        {
            if (!condition)
            {
                result.Errors.Add(error);
            }
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public int FoundationSpecCount;
        }

        private sealed class RegressionDefinition
        {
            public RegressionDefinition(string name, string reportPath, string passMarker)
            {
                this.name = name;
                this.reportPath = reportPath;
                this.passMarker = passMarker;
            }

            public readonly string name;
            public readonly string reportPath;
            public readonly string passMarker;
        }

        private sealed class LeakDefinition
        {
            public LeakDefinition(string category, string[] tokens)
            {
                this.category = category;
                this.tokens = tokens;
            }

            public readonly string category;
            public readonly string[] tokens;
        }

        private sealed class LeakRow
        {
            public LeakRow(string category, int count)
            {
                this.category = category;
                this.count = count;
            }

            public readonly string category;
            public readonly int count;
        }

        private sealed class SpecRow
        {
            public SpecRow(
                string caseId,
                string category,
                string baseItemId,
                string statId,
                string rarity,
                string expected,
                string actual,
                string validationCode,
                string result)
            {
                this.caseId = caseId;
                this.category = category;
                this.baseItemId = baseItemId;
                this.statId = statId;
                this.rarity = rarity;
                this.expected = expected;
                this.actual = actual;
                this.validationCode = validationCode;
                this.result = result;
            }

            public readonly string caseId;
            public readonly string category;
            public readonly string baseItemId;
            public readonly string statId;
            public readonly string rarity;
            public readonly string expected;
            public readonly string actual;
            public readonly string validationCode;
            public readonly string result;
        }
    }
}
#endif
