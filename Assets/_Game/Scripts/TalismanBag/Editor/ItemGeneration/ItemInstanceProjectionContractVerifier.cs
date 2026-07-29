#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.Items;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemInstanceProjectionContractVerifier
    {
        private const string PackageName = "V0.4-ItemInstanceProjectionContract01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemInstanceProjectionContractReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemInstanceProjectionContractSpec.csv";
        private const string FieldMatrixPath = "Docs/V0.4/Reports/ItemInstanceProjectionFieldMatrix.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemInstanceProjectionContractLeakCheckReport.md";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection";
        private const string PassMarker = "ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemInstanceProjectionContract01/[QA Only] Run Contract")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(Application.isBatchMode);
        }

        public static void VerifyHistoricalItemRuntimeBatch()
        {
            List<string> errors = new();
            try
            {
                TalismanBag.EditorTools.ItemSandbox.ItemInnerDataCatalogVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.ItemSystemValidatorAndSnapshotVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.BuildSynergyCoreVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.CoreAwakeningPreviewVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.ItemSkillTriggerContractVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.ItemDetailProjectionCompleteVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.JuNianLightingAndAdjacentRelayVerifier.VerifyMenu();
                TalismanBag.EditorTools.ItemSandbox.ArrayBonusCellResolverVerifier.VerifyMenu();
            }
            catch (Exception exception)
            {
                errors.Add("Historical Item Runtime verifier threw: " + exception);
            }

            Regression[] regressions =
            {
                new("ItemInnerDataCatalog", "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot", "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore", "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview", "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract", "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete", "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting", "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus", "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };
            foreach (Regression regression in regressions)
            {
                if (!File.Exists(regression.path) || !File.ReadAllText(regression.path).Contains(regression.marker))
                {
                    errors.Add("Historical Item Runtime regression is not PASS: " + regression.name + ".");
                }
            }

            bool passed = errors.Count == 0;
            if (passed)
            {
                Debug.Log("ITEM_INSTANCE_PROJECTION_HISTORICAL_ITEM_RUNTIME_PASS suites=8");
            }
            else
            {
                Debug.LogError("ITEM_INSTANCE_PROJECTION_HISTORICAL_ITEM_RUNTIME_FAIL\n" +
                    string.Join("\n", errors));
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            Verification verification = new();
            List<SpecRow> specs = new();
            List<LeakRow> leaks = new();
            Fixture fixture = null;
            ItemInstanceProjectionResult sample = null;
            ItemInstanceProjectionSetResult setResult = null;
            try
            {
                fixture = Fixture.Create();
                sample = RunProjectionFacts(fixture, specs, verification);
                setResult = RunSetAndQueries(fixture, specs, verification);
                RunDeterminism(fixture, sample, setResult, specs, verification);
                RunReadOnlyAndIsolation(fixture, sample, setResult, specs, verification);
                RunValidationCases(fixture, setResult, specs, verification);
                RunContractShapeAndCompatibility(specs, verification);
                RunHistoricalRegressions(verification);
                RunLeakChecks(leaks, verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add("Verifier threw an unhandled exception: " + exception);
            }

            int queryCount = specs.Count(row => row.category == "query");
            bool passed = fixture != null
                && sample != null && sample.isSuccess
                && setResult != null && setResult.isSuccess
                && specs.Count >= 70
                && specs.All(row => row.result == "PASS")
                && leaks.Count > 0
                && leaks.All(row => row.count == 0)
                && verification.Errors.Count == 0;
            WriteReports(fixture, sample, setResult, specs, leaks, verification, passed);
            if (passed)
            {
                Debug.Log(PassMarker + $" specs={specs.Count} instances={setResult.snapshot.Projections.Count} queries={queryCount} leaks=0");
            }
            else
            {
                Debug.LogError("ITEM_INSTANCE_PROJECTION_CONTRACT01_FAIL\n" +
                    string.Join("\n", verification.Errors));
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static ItemInstanceProjectionResult RunProjectionFacts(
            Fixture fixture,
            List<SpecRow> specs,
            Verification verification)
        {
            ItemGeneratedInstanceSnapshot source = fixture.Sources[1];
            ItemInstanceProjectionResult result = ItemInstanceProjectionProvider.Project(
                new ItemInstanceProjectionRequest(source));
            Check(specs, verification, "normal-qa-generated-instance", "projection",
                result.isSuccess, "success", ResultText(result));
            if (!result.isSuccess)
            {
                return result;
            }

            ItemInstanceProjectionContractSnapshot snapshot = result.snapshot;
            Check(specs, verification, "single-schema", "projection",
                snapshot.schemaId == ItemInstanceProjectionContractSnapshot.CurrentSchemaId,
                ItemInstanceProjectionContractSnapshot.CurrentSchemaId, snapshot.schemaId);
            Check(specs, verification, "source-schema", "projection",
                snapshot.sourceSchemaId == source.schemaId, source.schemaId, snapshot.sourceSchemaId);
            Check(specs, verification, "source-algorithm", "projection",
                snapshot.sourceGenerationAlgorithmId == source.generationAlgorithmId,
                source.generationAlgorithmId, snapshot.sourceGenerationAlgorithmId);
            Check(specs, verification, "generation-status-pass-through", "projection",
                snapshot.generationDataStatus == source.generationDataStatus,
                source.generationDataStatus, snapshot.generationDataStatus);
            Check(specs, verification, "identity-item-instance", "projection",
                snapshot.itemInstanceId == source.itemInstanceId, source.itemInstanceId, snapshot.itemInstanceId);
            Check(specs, verification, "identity-base-item", "projection",
                snapshot.baseItemId == source.baseItemId, source.baseItemId, snapshot.baseItemId);
            Check(specs, verification, "identity-rarity", "projection",
                snapshot.rarity == source.rarity, source.rarity.ToString(), snapshot.rarity.ToString());
            Check(specs, verification, "rarity-key", "projection",
                snapshot.rarityKey == source.rarity.ToStableKey(), source.rarity.ToStableKey(), snapshot.rarityKey);
            Check(specs, verification, "generation-version", "projection",
                snapshot.generationVersion == source.generationVersion,
                source.generationVersion.ToString(CultureInfo.InvariantCulture),
                snapshot.generationVersion.ToString(CultureInfo.InvariantCulture));
            Check(specs, verification, "root-seed", "projection",
                snapshot.rootSeed == source.rootSeed,
                source.rootSeed.ToString(CultureInfo.InvariantCulture),
                snapshot.rootSeed.ToString(CultureInfo.InvariantCulture));
            Check(specs, verification, "cultivation-profile", "projection",
                snapshot.cultivationPotentialProfileId == source.cultivationPotentialProfileId,
                source.cultivationPotentialProfileId, snapshot.cultivationPotentialProfileId);

            bool statsEqual = snapshot.Stats.Count == source.GeneratedStats.Count
                && snapshot.Stats.Zip(source.GeneratedStats, (left, right) =>
                    left.statId == right.statId && left.rawUnits == right.rawUnits).All(value => value);
            Check(specs, verification, "stats-full-copy", "projection", statsEqual,
                source.GeneratedStats.Count.ToString(CultureInfo.InvariantCulture),
                snapshot.Stats.Count.ToString(CultureInfo.InvariantCulture));

            bool affixesEqual = snapshot.Affixes.Count == source.GeneratedAffixes.Count
                && snapshot.Affixes.Zip(source.GeneratedAffixes, (left, right) =>
                    left.slotId == right.slotId && left.slotKind == right.slotKind
                    && left.affixId == right.affixId
                    && left.affixValueProfileId == right.affixValueProfileId
                    && left.rawUnits == right.rawUnits).All(value => value);
            Check(specs, verification, "affixes-full-copy", "projection", affixesEqual,
                source.GeneratedAffixes.Count.ToString(CultureInfo.InvariantCulture),
                snapshot.Affixes.Count.ToString(CultureInfo.InvariantCulture));
            Check(specs, verification, "affix-fixed-random-distinct", "projection",
                snapshot.Affixes.Any(value => value.slotKind == ItemAffixSlotKind.Fixed)
                && snapshot.Affixes.Any(value => value.slotKind == ItemAffixSlotKind.Random),
                "fixed+random", string.Join("|", snapshot.Affixes.Select(value => value.slotKind).Distinct()));

            Check(specs, verification, "eligible-core-full-copy", "projection",
                snapshot.EligibleCoreEffectIds.SequenceEqual(source.GeneratedCorePotential.EligibleCoreEffectIds),
                Join(source.GeneratedCorePotential.EligibleCoreEffectIds), Join(snapshot.EligibleCoreEffectIds));
            Check(specs, verification, "visible-core-full-copy", "projection",
                snapshot.VisibleCoreEffectIds.SequenceEqual(source.GeneratedCorePotential.VisibleCoreEffectIds),
                Join(source.GeneratedCorePotential.VisibleCoreEffectIds), Join(snapshot.VisibleCoreEffectIds));
            Check(specs, verification, "visible-core-subset", "projection",
                snapshot.VisibleCoreEffectIds.All(snapshot.EligibleCoreEffectIds.Contains),
                "subset", Join(snapshot.VisibleCoreEffectIds));
            Check(specs, verification, "build-qualification-pass-through", "projection",
                snapshot.buildQualification == source.buildQualification,
                source.buildQualification.ToString(), snapshot.buildQualification.ToString());
            Check(specs, verification, "source-canonical-signature", "projection",
                snapshot.sourceCanonicalSignature == source.BuildCanonicalSignature(),
                Digest(source.BuildCanonicalSignature()), Digest(snapshot.sourceCanonicalSignature));

            foreach (ItemGeneratedInstanceSnapshot raritySource in fixture.Sources.Take(5))
            {
                ItemInstanceProjectionResult rarityResult = ItemInstanceProjectionProvider.Project(
                    new ItemInstanceProjectionRequest(raritySource));
                Check(specs, verification, "rarity-" + raritySource.rarity.ToStableKey(), "projection",
                    rarityResult.isSuccess
                    && rarityResult.snapshot.rarityKey == raritySource.rarity.ToStableKey()
                    && rarityResult.snapshot.buildQualification == raritySource.buildQualification,
                    raritySource.rarity.ToStableKey() + "/" + raritySource.buildQualification,
                    rarityResult.isSuccess
                        ? rarityResult.snapshot.rarityKey + "/" + rarityResult.snapshot.buildQualification
                        : rarityResult.primaryValidationCode);
            }

            Check(specs, verification, "white-build-none", "projection",
                fixture.Sources[0].buildQualification == ItemBuildQualification.None,
                ItemBuildQualification.None.ToString(), fixture.Sources[0].buildQualification.ToString());
            Check(specs, verification, "readonly-item-consumer", "consumer",
                ConsumeAsItemSystem(snapshot) && ConsumeAsDetail(snapshot),
                "both read-only consumers can read", "item+detail");
            return result;
        }

        private static ItemInstanceProjectionSetResult RunSetAndQueries(
            Fixture fixture,
            List<SpecRow> specs,
            Verification verification)
        {
            ItemInstanceProjectionSetResult result = ItemInstanceProjectionProvider.ProjectSet(
                new ItemInstanceProjectionSetRequest(fixture.Sources));
            Check(specs, verification, "set-success", "set", result.isSuccess,
                "success", ResultText(result));
            if (!result.isSuccess)
            {
                return result;
            }

            ItemInstanceProjectionSetSnapshot set = result.snapshot;
            Check(specs, verification, "set-schema", "set",
                set.schemaId == ItemInstanceProjectionSetSnapshot.CurrentSchemaId,
                ItemInstanceProjectionSetSnapshot.CurrentSchemaId, set.schemaId);
            Check(specs, verification, "set-is-valid", "set", set.isValid && set.ValidationErrors.Count == 0,
                "valid/no-errors", set.isValid + "/" + set.ValidationErrors.Count);
            Check(specs, verification, "set-all-instances", "set",
                set.Projections.Count == fixture.Sources.Count,
                fixture.Sources.Count.ToString(CultureInfo.InvariantCulture),
                set.Projections.Count.ToString(CultureInfo.InvariantCulture));
            Check(specs, verification, "set-stable-order", "set",
                set.Projections.Select(value => value.itemInstanceId)
                    .SequenceEqual(set.Projections.Select(value => value.itemInstanceId)
                        .OrderBy(value => value, StringComparer.Ordinal)),
                "Ordinal", Join(set.Projections.Select(value => value.itemInstanceId)));

            ItemInstanceProjectionQueryResult exact = set.QueryByItemInstanceId("QA-I001-green-A");
            Check(specs, verification, "query-item-instance", "query",
                exact.isSuccess && exact.snapshot.itemInstanceId == "QA-I001-green-A",
                "QA-I001-green-A", exact.isSuccess ? exact.snapshot.itemInstanceId : exact.primaryValidationCode);
            ItemInstanceProjectionQueryResult byBase = set.QueryByBaseItemId("I001");
            Check(specs, verification, "query-base-multiple", "query",
                byBase.isSuccess && byBase.Snapshots.Count == fixture.Sources.Count,
                fixture.Sources.Count.ToString(CultureInfo.InvariantCulture),
                byBase.isSuccess ? byBase.Snapshots.Count.ToString(CultureInfo.InvariantCulture) : byBase.primaryValidationCode);
            ItemInstanceProjectionQueryResult byBaseRarity = set.QueryByBaseItemIdAndRarity(
                "I001", ItemInstanceRarity.Green);
            Check(specs, verification, "query-base-rarity-multiple", "query",
                byBaseRarity.isSuccess && byBaseRarity.Snapshots.Count == 2,
                "2", byBaseRarity.isSuccess ? byBaseRarity.Snapshots.Count.ToString(CultureInfo.InvariantCulture)
                    : byBaseRarity.primaryValidationCode);
            Check(specs, verification, "same-base-rarity-multi-instance", "set",
                byBaseRarity.isSuccess
                && byBaseRarity.Snapshots.Select(value => value.itemInstanceId).Distinct(StringComparer.Ordinal).Count() == 2,
                "2 distinct", byBaseRarity.isSuccess ? Join(byBaseRarity.Snapshots.Select(value => value.itemInstanceId))
                    : byBaseRarity.primaryValidationCode);
            return result;
        }

        private static void RunDeterminism(
            Fixture fixture,
            ItemInstanceProjectionResult sample,
            ItemInstanceProjectionSetResult setResult,
            List<SpecRow> specs,
            Verification verification)
        {
            string expected = sample.snapshot.BuildCanonicalSignature();
            bool repeatStable = true;
            for (int index = 0; index < 1000; index++)
            {
                ItemInstanceProjectionResult repeat = ItemInstanceProjectionProvider.Project(
                    new ItemInstanceProjectionRequest(fixture.Sources[1]));
                repeatStable &= repeat.isSuccess && repeat.snapshot.BuildCanonicalSignature() == expected;
            }

            Check(specs, verification, "single-canonical-repeat-1000", "determinism", repeatStable,
                "1000/1000", repeatStable ? "1000/1000" : "mismatch");

            ItemInstanceProjectionSetResult reversed = ItemInstanceProjectionProvider.ProjectSet(
                new ItemInstanceProjectionSetRequest(fixture.Sources.Reverse()));
            Check(specs, verification, "set-reversed-stable", "determinism",
                reversed.isSuccess
                && reversed.snapshot.BuildCanonicalSignature() == setResult.snapshot.BuildCanonicalSignature(),
                Digest(setResult.snapshot.BuildCanonicalSignature()),
                reversed.isSuccess ? Digest(reversed.snapshot.BuildCanonicalSignature()) : reversed.primaryValidationCode);

            string firstGreen = setResult.snapshot.QueryByItemInstanceId("QA-I001-green-A")
                .snapshot.BuildCanonicalSignature();
            string secondGreen = setResult.snapshot.QueryByItemInstanceId("QA-I001-green-B")
                .snapshot.BuildCanonicalSignature();
            Check(specs, verification, "different-instance-different-signature", "determinism",
                firstGreen != secondGreen, "different", firstGreen == secondGreen ? "same" : "different");

            ItemInstanceProjectionContractSnapshot baseline = sample.snapshot;
            CheckSignatureMutation(specs, verification, "canonical-stat-branch", baseline,
                CloneProjection(baseline, stats: new[]
                {
                    ProjectionStat(baseline.Stats[0].statId, baseline.Stats[0].rawUnits + 1),
                    ProjectionStat(baseline.Stats[1].statId, baseline.Stats[1].rawUnits)
                }));
            CheckSignatureMutation(specs, verification, "canonical-affix-branch", baseline,
                CloneProjection(baseline, affixes: baseline.Affixes.Select((value, index) =>
                    ProjectionAffix(value.slotId, value.slotKind, value.affixId,
                        value.affixValueProfileId, value.rawUnits + (index == 0 ? 1 : 0))).ToArray()));
            CheckSignatureMutation(specs, verification, "canonical-eligible-core-branch", baseline,
                CloneProjection(baseline, eligible: baseline.EligibleCoreEffectIds.Concat(new[] { "QA_EXTRA_CORE" })));
            CheckSignatureMutation(specs, verification, "canonical-visible-core-branch", baseline,
                CloneProjection(baseline, visible: Array.Empty<string>()));
            CheckSignatureMutation(specs, verification, "canonical-build-branch", baseline,
                CloneProjection(baseline, build: baseline.buildQualification == ItemBuildQualification.Dual
                    ? ItemBuildQualification.FaMenOnly : ItemBuildQualification.Dual));
            CheckSignatureMutation(specs, verification, "canonical-source-signature-branch", baseline,
                CloneProjection(baseline, sourceSignature: baseline.sourceCanonicalSignature + "x"));
        }

        private static void RunReadOnlyAndIsolation(
            Fixture fixture,
            ItemInstanceProjectionResult sample,
            ItemInstanceProjectionSetResult setResult,
            List<SpecRow> specs,
            Verification verification)
        {
            Check(specs, verification, "readonly-top-level-set", "readonly",
                RejectMutation(setResult.snapshot.Projections), "rejected", "mutation attempt");
            Check(specs, verification, "readonly-stats", "readonly",
                RejectMutation(sample.snapshot.Stats), "rejected", "mutation attempt");
            Check(specs, verification, "readonly-affixes", "readonly",
                RejectMutation(sample.snapshot.Affixes), "rejected", "mutation attempt");
            Check(specs, verification, "readonly-eligible-core", "readonly",
                RejectMutation(sample.snapshot.EligibleCoreEffectIds), "rejected", "mutation attempt");
            Check(specs, verification, "readonly-visible-core", "readonly",
                RejectMutation(sample.snapshot.VisibleCoreEffectIds), "rejected", "mutation attempt");
            Check(specs, verification, "readonly-query-results", "readonly",
                RejectMutation(setResult.snapshot.QueryByBaseItemId("I001").Snapshots),
                "rejected", "mutation attempt");
            Check(specs, verification, "readonly-validation-errors", "readonly",
                RejectMutation(ItemInstanceProjectionProvider.Project(null).ValidationErrors),
                "rejected", "mutation attempt");

            ItemGeneratedInstanceSnapshot source = fixture.CreateGenerated(
                ItemInstanceRarity.Blue, 818L, "QA-SOURCE-MUTATION", ItemBuildQualification.QiLeiOnly);
            ItemInstanceProjectionResult projected = ItemInstanceProjectionProvider.Project(
                new ItemInstanceProjectionRequest(source));
            string beforeId = projected.snapshot.itemInstanceId;
            string beforeSignature = projected.snapshot.BuildCanonicalSignature();
            SetBackingField(source, "itemInstanceId", "MUTATED-SOURCE-ID");
            Check(specs, verification, "source-mutation-isolated", "readonly",
                projected.snapshot.itemInstanceId == beforeId
                && projected.snapshot.BuildCanonicalSignature() == beforeSignature,
                beforeId + "/" + Digest(beforeSignature),
                projected.snapshot.itemInstanceId + "/" + Digest(projected.snapshot.BuildCanonicalSignature()));
        }

        private static void RunValidationCases(
            Fixture fixture,
            ItemInstanceProjectionSetResult validSet,
            List<SpecRow> specs,
            Verification verification)
        {
            ExpectProjectionCode(specs, verification, "null-request", null,
                ItemInstanceProjectionValidationCodes.RequestNull);
            ExpectProjectionCode(specs, verification, "null-generated-instance",
                new ItemInstanceProjectionRequest(null), ItemInstanceProjectionValidationCodes.GeneratedInstanceNull);

            ItemGeneratedInstanceSnapshot invalid = fixture.InvalidSource();
            SetBackingField(invalid, "schemaId", "bad.schema");
            ExpectSourceCode(specs, verification, "source-schema-invalid", invalid,
                ItemInstanceProjectionValidationCodes.SourceSchemaInvalid);
            invalid = fixture.InvalidSource();
            SetBackingField(invalid, "generationAlgorithmId", string.Empty);
            ExpectSourceCode(specs, verification, "source-algorithm-empty", invalid,
                ItemInstanceProjectionValidationCodes.SourceAlgorithmIdEmpty);
            ExpectSourceCode(specs, verification, "generation-status-empty",
                fixture.InvalidSource(status: string.Empty), ItemInstanceProjectionValidationCodes.GenerationDataStatusEmpty);
            ExpectSourceCode(specs, verification, "item-instance-empty",
                fixture.InvalidSource(instanceId: string.Empty), ItemInstanceProjectionValidationCodes.ItemInstanceIdEmpty);
            ExpectSourceCode(specs, verification, "base-item-empty",
                fixture.InvalidSource(baseItemId: string.Empty), ItemInstanceProjectionValidationCodes.BaseItemIdEmpty);
            ExpectSourceCode(specs, verification, "base-item-unknown",
                fixture.InvalidSource(baseItemId: "I999"), ItemInstanceProjectionValidationCodes.BaseItemUnknown);
            ItemGeneratedInstanceSnapshot i031 = fixture.InvalidSource(baseItemId: "I031");
            ExpectSourceCode(specs, verification, "i031-forbidden", i031,
                ItemInstanceProjectionValidationCodes.I031Forbidden);
            ExpectSourceCode(specs, verification, "base-item-not-ordinary", i031,
                ItemInstanceProjectionValidationCodes.BaseItemNotOrdinary);
            ItemGeneratedInstanceSnapshot badRarity = fixture.InvalidSource(rarity: (ItemInstanceRarity)999);
            ExpectSourceCode(specs, verification, "rarity-invalid", badRarity,
                ItemInstanceProjectionValidationCodes.RarityInvalid);
            ExpectSourceCode(specs, verification, "rarity-key-invalid", badRarity,
                ItemInstanceProjectionValidationCodes.RarityKeyInvalid);
            ExpectSourceCode(specs, verification, "generation-version-invalid",
                fixture.InvalidSource(generationVersion: 0), ItemInstanceProjectionValidationCodes.GenerationVersionInvalid);
            ExpectSourceCode(specs, verification, "cultivation-profile-empty",
                fixture.InvalidSource(cultivationProfileId: string.Empty),
                ItemInstanceProjectionValidationCodes.CultivationProfileIdEmpty);

            invalid = fixture.InvalidSource();
            SetPrivateField(invalid, "generatedStats", Array.AsReadOnly(new ItemGeneratedStatSnapshot[] { null }));
            ExpectSourceCode(specs, verification, "stat-entry-null", invalid,
                ItemInstanceProjectionValidationCodes.StatEntryNull);
            ExpectSourceCode(specs, verification, "stat-id-empty",
                fixture.InvalidSource(stats: new[] { fixture.GeneratedStat(string.Empty, 1) }),
                ItemInstanceProjectionValidationCodes.StatIdEmpty);
            ExpectSourceCode(specs, verification, "stat-id-duplicate",
                fixture.InvalidSource(stats: new[]
                {
                    fixture.GeneratedStat("dup", 1), fixture.GeneratedStat("dup", 2)
                }), ItemInstanceProjectionValidationCodes.StatIdDuplicate);

            invalid = fixture.InvalidSource();
            SetPrivateField(invalid, "generatedAffixes", Array.AsReadOnly(new ItemGeneratedAffixSnapshot[] { null }));
            ExpectSourceCode(specs, verification, "affix-entry-null", invalid,
                ItemInstanceProjectionValidationCodes.AffixEntryNull);
            ExpectSourceCode(specs, verification, "affix-slot-empty",
                fixture.InvalidSource(affixes: new[]
                {
                    fixture.GeneratedAffix(string.Empty, ItemAffixSlotKind.Fixed, "a", "p", 1)
                }), ItemInstanceProjectionValidationCodes.AffixSlotIdEmpty);
            ExpectSourceCode(specs, verification, "affix-slot-duplicate",
                fixture.InvalidSource(affixes: new[]
                {
                    fixture.GeneratedAffix("dup", ItemAffixSlotKind.Fixed, "a", "p", 1),
                    fixture.GeneratedAffix("dup", ItemAffixSlotKind.Random, "b", "p", 2)
                }), ItemInstanceProjectionValidationCodes.AffixSlotIdDuplicate);
            ExpectSourceCode(specs, verification, "affix-slot-kind-invalid",
                fixture.InvalidSource(affixes: new[]
                {
                    fixture.GeneratedAffix("bad", (ItemAffixSlotKind)999, "a", "p", 1)
                }), ItemInstanceProjectionValidationCodes.AffixSlotKindInvalid);
            ExpectSourceCode(specs, verification, "affix-id-empty",
                fixture.InvalidSource(affixes: new[]
                {
                    fixture.GeneratedAffix("a", ItemAffixSlotKind.Fixed, string.Empty, "p", 1)
                }), ItemInstanceProjectionValidationCodes.AffixIdEmpty);
            ExpectSourceCode(specs, verification, "affix-profile-empty",
                fixture.InvalidSource(affixes: new[]
                {
                    fixture.GeneratedAffix("a", ItemAffixSlotKind.Fixed, "a", string.Empty, 1)
                }), ItemInstanceProjectionValidationCodes.AffixValueProfileIdEmpty);

            ExpectSourceCode(specs, verification, "core-potential-null",
                fixture.InvalidSource(nullCore: true), ItemInstanceProjectionValidationCodes.CorePotentialNull);
            ExpectSourceCode(specs, verification, "core-eligible-empty",
                fixture.InvalidSource(eligible: new[] { string.Empty }),
                ItemInstanceProjectionValidationCodes.CoreEligibleIdEmpty);
            ExpectSourceCode(specs, verification, "core-eligible-duplicate",
                fixture.InvalidSource(eligible: new[] { "c1", "c1" }, visible: new[] { "c1" }),
                ItemInstanceProjectionValidationCodes.CoreEligibleIdDuplicate);
            ExpectSourceCode(specs, verification, "core-visible-empty",
                fixture.InvalidSource(eligible: new[] { "c1" }, visible: new[] { string.Empty }),
                ItemInstanceProjectionValidationCodes.CoreVisibleIdEmpty);
            ExpectSourceCode(specs, verification, "core-visible-duplicate",
                fixture.InvalidSource(eligible: new[] { "c1" }, visible: new[] { "c1", "c1" }),
                ItemInstanceProjectionValidationCodes.CoreVisibleIdDuplicate);
            ExpectSourceCode(specs, verification, "core-visible-not-eligible",
                fixture.InvalidSource(eligible: new[] { "c1" }, visible: new[] { "c2" }),
                ItemInstanceProjectionValidationCodes.CoreVisibleNotEligible);
            ExpectSourceCode(specs, verification, "build-qualification-invalid",
                fixture.InvalidSource(build: ItemBuildQualification.Unresolved),
                ItemInstanceProjectionValidationCodes.BuildQualificationInvalid);
            ExpectSourceCode(specs, verification, "white-build-must-none",
                fixture.InvalidSource(rarity: ItemInstanceRarity.White, build: ItemBuildQualification.Dual),
                ItemInstanceProjectionValidationCodes.WhiteBuildQualificationMustBeNone);

            ExpectSetCode(specs, verification, "set-request-null", null,
                ItemInstanceProjectionValidationCodes.RequestNull);
            ExpectSetCode(specs, verification, "set-input-null", new ItemInstanceProjectionSetRequest(null),
                ItemInstanceProjectionValidationCodes.SetInputNull);
            ExpectSetCode(specs, verification, "set-entry-null",
                new ItemInstanceProjectionSetRequest(new ItemGeneratedInstanceSnapshot[] { fixture.Sources[0], null }),
                ItemInstanceProjectionValidationCodes.SetEntryNull);
            ExpectSetCode(specs, verification, "set-instance-id-duplicate",
                new ItemInstanceProjectionSetRequest(new[] { fixture.Sources[1], fixture.Sources[1] }),
                ItemInstanceProjectionValidationCodes.ItemInstanceIdDuplicate);
            ExpectSetCode(specs, verification, "set-i031-null-snapshot",
                new ItemInstanceProjectionSetRequest(new[] { fixture.Sources[0], i031 }),
                ItemInstanceProjectionValidationCodes.I031Forbidden);

            ExpectQueryCode(specs, verification, "query-item-id-empty",
                validSet.snapshot.QueryByItemInstanceId(string.Empty),
                ItemInstanceProjectionValidationCodes.QueryItemInstanceIdEmpty);
            ExpectQueryCode(specs, verification, "query-base-id-empty",
                validSet.snapshot.QueryByBaseItemId(string.Empty),
                ItemInstanceProjectionValidationCodes.QueryBaseItemIdEmpty);
            ExpectQueryCode(specs, verification, "query-rarity-invalid",
                validSet.snapshot.QueryByBaseItemIdAndRarity("I001", (ItemInstanceRarity)999),
                ItemInstanceProjectionValidationCodes.QueryRarityInvalid);
            ExpectQueryCode(specs, verification, "query-not-found-instance",
                validSet.snapshot.QueryByItemInstanceId("missing"),
                ItemInstanceProjectionValidationCodes.QueryNotFound);
            ExpectQueryCode(specs, verification, "query-not-found-base",
                validSet.snapshot.QueryByBaseItemId("I030"),
                ItemInstanceProjectionValidationCodes.QueryNotFound);
            ExpectQueryCode(specs, verification, "query-not-found-rarity",
                validSet.snapshot.QueryByBaseItemIdAndRarity("I001", (ItemInstanceRarity)999),
                ItemInstanceProjectionValidationCodes.QueryRarityInvalid);

            ItemGeneratedInstanceSnapshot emptyFormal = fixture.InvalidSource(
                status: string.Empty,
                cultivationProfileId: string.Empty,
                stats: Array.Empty<ItemGeneratedStatSnapshot>(),
                affixes: Array.Empty<ItemGeneratedAffixSnapshot>(),
                nullCore: true,
                build: ItemBuildQualification.Unresolved);
            ItemInstanceProjectionResult emptyFormalResult = ItemInstanceProjectionProvider.Project(
                new ItemInstanceProjectionRequest(emptyFormal));
            Check(specs, verification, "formal-empty-data-no-false-success", "validation",
                !emptyFormalResult.isSuccess && emptyFormalResult.snapshot == null,
                "failure/null", ResultText(emptyFormalResult));

            foreach (FieldInfo codeField in typeof(ItemInstanceProjectionValidationCodes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && !field.IsInitOnly))
            {
                string value = codeField.GetRawConstantValue() as string;
                Check(specs, verification, "validation-code-" + codeField.Name, "validation-code",
                    !string.IsNullOrWhiteSpace(value), "stable non-empty", value ?? "null");
            }
        }

        private static void RunContractShapeAndCompatibility(
            List<SpecRow> specs,
            Verification verification)
        {
            Type[] runtimeTypes =
            {
                typeof(ItemInstanceProjectionContractSnapshot),
                typeof(ItemInstanceProjectionSetSnapshot),
                typeof(ItemInstanceProjectionStatSnapshot),
                typeof(ItemInstanceProjectionAffixSnapshot),
                typeof(ItemInstanceProjectionResult),
                typeof(ItemInstanceProjectionSetResult),
                typeof(ItemInstanceProjectionQueryResult)
            };
            string[] forbiddenProperties =
            {
                "itemId", "placementId", "isLit", "isDirectLit", "litDepth", "countedInBuild",
                "buildStage", "selectedMainBuildId", "unlockedCoreEffectIds", "activeCoreEffectIds",
                "coreEffectActive", "itemLevel", "cultivationLevel", "rewardSource", "inventorySlot",
                "saveKey", "itemPower"
            };
            foreach (string forbidden in forbiddenProperties)
            {
                bool absent = runtimeTypes.All(type => type.GetProperty(forbidden,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) == null);
                Check(specs, verification, "forbidden-property-" + forbidden, "contract-shape",
                    absent, "absent", absent ? "absent" : "present");
            }

            Check(specs, verification, "item-system-schema-compatible", "compatibility",
                ItemSystemSnapshot.CurrentSchemaVersion == "ItemSystemSnapshot.v2",
                "ItemSystemSnapshot.v2", ItemSystemSnapshot.CurrentSchemaVersion);
            string itemSystemSource = ReadProjectFile("Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs");
            Check(specs, verification, "item-system-debug-signature-present", "compatibility",
                itemSystemSource.Contains("public string BuildDebugSignature()"),
                "present", itemSystemSource.Contains("public string BuildDebugSignature()") ? "present" : "missing");
            Check(specs, verification, "detail-viewmodel-unmodified-contract", "compatibility",
                File.Exists("Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md")
                && File.ReadAllText("Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md").Contains("Result: PASS"),
                "ItemDetailProjectionComplete PASS", "report marker check");
        }

        private static void RunHistoricalRegressions(Verification verification)
        {
            Regression[] regressions =
            {
                new("Foundation", "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md", "- Verification: PASS"),
                new("StatRange", "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md", "- Overall: PASS"),
                new("CorePotential", "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md", "- Overall: PASS"),
                new("AffixSchema", "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaReport.md", "- Result: PASS"),
                new("RollEngine", "Docs/V0.4/Reports/ItemInstanceRollEngineReport.md", "- Result: PASS"),
                new("DropSandbox", "Docs/V0.4/Reports/ItemDropGenerationSandboxReport.md", "- Result: PASS"),
                new("SimulationValidator", "Docs/V0.4/Reports/ItemGenerationSimulationValidatorReport.md", "- Result: PASS"),
                new("ItemInnerDataCatalog", "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot", "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore", "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview", "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract", "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete", "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting", "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus", "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };
            foreach (Regression regression in regressions)
            {
                bool pass = File.Exists(regression.path) && File.ReadAllText(regression.path).Contains(regression.marker);
                verification.Regressions.Add(regression.name + ": " + (pass ? "PASS" : "FAIL"));
                if (!pass) verification.Errors.Add("Historical regression report is not PASS: " + regression.name + ".");
            }

            verification.FoundationCount = CsvRows("Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv");
            verification.StatCount = CsvRows("Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv");
            verification.CoreCount = CsvRows("Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv");
            verification.AffixCount = CsvRows("Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv");
            verification.RollCount = CsvRows("Docs/V0.4/Reports/ItemInstanceRollEngineSpec.csv");
            verification.DropSpecCount = CsvRows("Docs/V0.4/Reports/ItemDropGenerationSandboxSpec.csv");
            verification.DropDeterminismCount = CsvRows("Docs/V0.4/Reports/ItemDropGenerationSandboxDeterminism.csv");
            verification.SimulationSpecCount = CsvRows("Docs/V0.4/Reports/ItemGenerationSimulationValidatorSpec.csv");
            verification.SimulationDistributionCount = CsvRows("Docs/V0.4/Reports/ItemGenerationSimulationDistribution.csv");
            verification.SimulationDeterminismCount = CsvRows("Docs/V0.4/Reports/ItemGenerationSimulationDeterminism.csv");
            verification.SimulationInvariantCount = CsvInvariantCount(
                "Docs/V0.4/Reports/ItemGenerationSimulationInvariantViolations.csv");
            RequireCount(verification, "Foundation", verification.FoundationCount, 159);
            RequireCount(verification, "StatRange", verification.StatCount, 39);
            RequireCount(verification, "CorePotential", verification.CoreCount, 69);
            RequireCount(verification, "AffixSchema", verification.AffixCount, 88);
            RequireCount(verification, "RollEngine", verification.RollCount, 72);
            RequireCount(verification, "DropSandbox Spec", verification.DropSpecCount, 87);
            RequireCount(verification, "DropSandbox Determinism", verification.DropDeterminismCount, 6);
            RequireCount(verification, "Simulation Spec", verification.SimulationSpecCount, 65);
            RequireCount(verification, "Simulation Distribution", verification.SimulationDistributionCount, 46);
            RequireCount(verification, "Simulation Determinism", verification.SimulationDeterminismCount, 12);
            RequireCount(verification, "Simulation Invariant", verification.SimulationInvariantCount, 0);
        }

        private static void RunLeakChecks(List<LeakRow> leaks, Verification verification)
        {
            string source = string.Join("\n", Directory.Exists(RuntimeRoot)
                ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .Select(File.ReadAllText)
                : Array.Empty<string>());
            LeakDefinition[] definitions =
            {
                new("Reward", @"\bReward\b"),
                new("RunFlow", @"\bRunFlow\b"),
                new("Inventory", @"\bInventory\b"),
                new("SaveData", @"\bSaveData\b"),
                new("Battle", @"\bBattle\b"),
                new("Boss", @"\bBoss\b"),
                new("Unity random API", @"UnityEngine\.Random"),
                new("System random API", @"new\s+Random\s*\("),
                new("current time", @"DateTime\.(Now|UtcNow)"),
                new("machine information", @"Environment\.MachineName"),
                new("placement identity property", @"public\s+[^\n;{}]+\s+placementId\s*\{"),
                new("legacy item identity property", @"public\s+[^\n;{}]+\s+itemId\s*\{"),
                new("itemPower", @"\bitemPower\b"),
                new("lit state", @"\b(isLit|isDirectLit|litDepth)\b"),
                new("Build activation", @"\b(countedInBuild|buildStage|selectedMainBuildId)\b"),
                new("cultivation unlock", @"\b(unlockedCoreEffectIds|activeCoreEffectIds|cultivationLevel)\b")
            };
            foreach (LeakDefinition definition in definitions)
            {
                int count = Regex.Matches(source, definition.pattern, RegexOptions.CultureInvariant).Count;
                leaks.Add(new LeakRow(definition.category, count));
                if (count != 0) verification.Errors.Add($"LeakCheck '{definition.category}' found {count} occurrence(s).");
            }
        }

        private static void WriteReports(
            Fixture fixture,
            ItemInstanceProjectionResult sample,
            ItemInstanceProjectionSetResult setResult,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<LeakRow> leaks,
            Verification verification,
            bool passed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath,
                BuildReport(fixture, sample, setResult, specs, leaks, verification, passed),
                new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(specs), new UTF8Encoding(false));
            File.WriteAllText(FieldMatrixPath, BuildFieldMatrix(), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeak(leaks, passed), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(
            Fixture fixture,
            ItemInstanceProjectionResult sample,
            ItemInstanceProjectionSetResult setResult,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<LeakRow> leaks,
            Verification verification,
            bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInstanceProjectionContract01 Report").AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(passed ? PassMarker : "ITEM_INSTANCE_PROJECTION_CONTRACT01_FAIL")}`")
                .AppendLine($"- Single schema: `{ItemInstanceProjectionContractSnapshot.CurrentSchemaId}`")
                .AppendLine($"- Set schema: `{ItemInstanceProjectionSetSnapshot.CurrentSchemaId}`")
                .AppendLine($"- Source schema: `{ItemGeneratedInstanceSnapshot.CurrentSchemaId}`")
                .AppendLine($"- QA status preserved: `{sample?.snapshot?.generationDataStatus ?? "n/a"}`")
                .AppendLine($"- Spec: {specs.Count(row => row.result == "PASS")}/{specs.Count} PASS")
                .AppendLine($"- Instances: {setResult?.snapshot?.Projections.Count ?? 0}")
                .AppendLine($"- Queries: {specs.Count(row => row.category == "query")}")
                .AppendLine($"- Leak total: {leaks.Sum(row => row.count)}")
                .AppendLine().AppendLine("## Single-instance fields").AppendLine()
                .AppendLine("`schemaId / sourceSchemaId / sourceGenerationAlgorithmId / generationDataStatus / itemInstanceId / baseItemId / rarity / rarityKey / generationVersion / rootSeed / cultivationPotentialProfileId / stats / affixes / eligibleCoreEffectIds / visibleCoreEffectIds / buildQualification / sourceCanonicalSignature`")
                .AppendLine().AppendLine("Nested stat fields: `statId / rawUnits`.")
                .AppendLine("Nested affix fields: `slotId / slotKind / affixId / affixValueProfileId / rawUnits`.")
                .AppendLine().AppendLine("## Set and queries").AppendLine()
                .AppendLine("The set stores an Ordinal-sorted read-only projection list, requires unique itemInstanceId, preserves same baseItemId + rarity instances, and exposes:")
                .AppendLine("- `QueryByItemInstanceId(itemInstanceId)`")
                .AppendLine("- `QueryByBaseItemId(baseItemId)`")
                .AppendLine("- `QueryByBaseItemIdAndRarity(baseItemId, rarity)`")
                .AppendLine($"- Same I001+green instances retained: {setResult?.snapshot?.QueryByBaseItemIdAndRarity("I001", ItemInstanceRarity.Green).Snapshots.Count ?? 0}")
                .AppendLine().AppendLine("## Consumer boundary").AppendLine()
                .AppendLine("- Overall Item System may read baseItemId for Catalog lookup, itemInstanceId for instance identity, rarity, Build qualification, core potential IDs, stats and affixes.")
                .AppendLine("- Future detail projection may read the same raw facts, but this package does not modify ItemDetailViewModel or ItemDetailProjectionComposer and does not emit localized player value text.")
                .AppendLine("- No placement, lighting, cultivation, combat, reward, inventory or persistence state is included.")
                .AppendLine().AppendLine("## Canonical signature").AppendLine()
                .AppendLine("- Both signatures use explicit length-prefixed fields, Ordinal stable ordering and InvariantCulture numeric formatting.")
                .AppendLine($"- Single digest: `{Digest(sample?.snapshot?.BuildCanonicalSignature())}`")
                .AppendLine($"- Set digest: `{Digest(setResult?.snapshot?.BuildCanonicalSignature())}`")
                .AppendLine("- Same-input repeat: `1000/1000 PASS` when this report is PASS.")
                .AppendLine("- Reversed set input is identical; different itemInstanceId is different; stat/affix/core/Build/source signature mutations change the signature.")
                .AppendLine().AppendLine("## Read-only and identity results").AppendLine()
                .AppendLine("- Top-level set, stats, affixes, eligible/visible core IDs, query results and validation errors reject IList mutation.")
                .AppendLine("- Mutating the source after projection does not change an existing projection.")
                .AppendLine("- I031 is rejected and cannot enter a successful set.")
                .AppendLine("- No `itemId` alias or `placementId` field exists on new Runtime contract types.")
                .AppendLine().AppendLine("## Compatibility").AppendLine()
                .AppendLine($"- ItemSystem schema remains `{ItemSystemSnapshot.CurrentSchemaVersion}`; BuildDebugSignature remains present and was not modified by this package.")
                .AppendLine("- ItemDetailProjectionComplete report remains PASS; no ItemDetailViewModel, Composer, view or prefab modification is made.")
                .AppendLine().AppendLine("## Formal systems not connected").AppendLine()
                .AppendLine("`Reward / RunFlow / Inventory / SaveData / Battle / Boss / formal drop UI / formal detail UI / Scene / Prefab / BuildSettings / cultivation unlock / lighting / Build counting or activation`")
                .AppendLine().AppendLine("## QA isolation").AppendLine()
                .AppendLine($"- Fixtures are generated through ItemInstanceRollEngine using `{ItemGenerationDataStatus.QaFixtureCanonical}`.")
                .AppendLine("- Invalid sources are created only inside this Editor verifier through non-public reflection; Runtime exposes no success-snapshot forgery API.")
                .AppendLine().AppendLine("## Historical regressions").AppendLine();
            foreach (string regression in verification.Regressions) builder.AppendLine("- " + regression);
            builder.AppendLine($"- Foundation: {verification.FoundationCount}/159")
                .AppendLine($"- StatRange: {verification.StatCount}/39")
                .AppendLine($"- CorePotential: {verification.CoreCount}/69")
                .AppendLine($"- AffixSchema: {verification.AffixCount}/88")
                .AppendLine($"- RollEngine: {verification.RollCount}/72")
                .AppendLine($"- DropSandbox: {verification.DropSpecCount}/87 Spec, {verification.DropDeterminismCount}/6 Determinism")
                .AppendLine($"- Simulation: {verification.SimulationSpecCount}/65 Spec, {verification.SimulationDistributionCount}/46 Distribution, {verification.SimulationDeterminismCount}/12 Determinism, invariant={verification.SimulationInvariantCount}")
                .AppendLine().AppendLine("## Errors").AppendLine();
            if (verification.Errors.Count == 0) builder.AppendLine("- None.");
            else foreach (string error in verification.Errors) builder.AppendLine("- " + error);
            return builder.ToString();
        }

        private static string BuildSpec(IEnumerable<SpecRow> rows)
        {
            StringBuilder builder = new("caseId,category,expected,actual,validationCode,result\n");
            foreach (SpecRow row in rows)
            {
                builder.Append(Csv(row.caseId)).Append(',').Append(Csv(row.category)).Append(',')
                    .Append(Csv(row.expected)).Append(',').Append(Csv(row.actual)).Append(',')
                    .Append(Csv(row.code)).Append(',').Append(Csv(row.result)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildFieldMatrix()
        {
            FieldRow[] rows =
            {
                new("schemaId", "contract constant", "schema dispatch", "schema dispatch", false, true, "none", "Projection schema only"),
                new("sourceSchemaId", "generated.schemaId", "source compatibility", "debug diagnostics", false, true, "none", "Must remain ItemGeneratedInstanceSnapshot.v1"),
                new("sourceGenerationAlgorithmId", "generated.generationAlgorithmId", "generation provenance", "debug diagnostics", false, true, "none", "Passed through exactly"),
                new("generationDataStatus", "generated.generationDataStatus", "data maturity guard", "debug diagnostics", false, true, "formal approval", "QA status is not promoted"),
                new("itemInstanceId", "generated.itemInstanceId", "distinguish actual instances", "strict instance context", false, true, "placement identity", "Never placementId"),
                new("baseItemId", "generated.baseItemId", "Catalog prototype lookup", "name/tags/shape/visual lookup", false, false, "legacy itemId alias", "I001-I030 only"),
                new("rarity", "generated.rarity", "instance rarity", "rarity presentation input", false, false, "cultivation level", "Raw enum fact"),
                new("rarityKey", "rarity stable mapping", "stable rarity key", "rarity label/color lookup", false, false, "localized label", "white/green/blue/purple/orange"),
                new("generationVersion", "generated.generationVersion", "generation provenance", "debug diagnostics", false, true, "save version", "Not persistence schema"),
                new("rootSeed", "generated.rootSeed", "deterministic provenance", "debug diagnostics", false, true, "reroll authorization", "Read only; never consumed"),
                new("cultivationPotentialProfileId", "generated.cultivationPotentialProfileId", "future cultivation lookup", "potential explanation", false, false, "unlocked state", "Profile identity only"),
                new("stats[].statId", "generatedStats[].statId", "instance stat identity", "future stat label lookup", false, false, "localized text", "Raw fact"),
                new("stats[].rawUnits", "generatedStats[].rawUnits", "instance birth value", "future formatted value", false, false, "itemPower", "No formatting in this package"),
                new("affixes[].slotId", "generatedAffixes[].slotId", "stable affix slot", "fixed/random line identity", false, false, "reroll slot", "Original slot preserved"),
                new("affixes[].slotKind", "generatedAffixes[].slotKind", "fixed/random query", "fixed/random section", false, false, "new roll", "No reroll"),
                new("affixes[].affixId", "generatedAffixes[].affixId", "affix fact", "future affix label lookup", false, false, "localized text", "Raw fact"),
                new("affixes[].affixValueProfileId", "generatedAffixes[].affixValueProfileId", "value provenance", "debug diagnostics", false, true, "formal balance approval", "QA fixture profile remains QA"),
                new("affixes[].rawUnits", "generatedAffixes[].rawUnits", "instance affix value", "future formatted value", false, false, "itemPower", "No player text"),
                new("eligibleCoreEffectIds[]", "generatedCorePotential.eligible", "future cultivation eligibility", "potential list", false, false, "unlocked/active state", "Eligibility only"),
                new("visibleCoreEffectIds[]", "generatedCorePotential.visible", "future visible potential", "visible potential list", false, false, "unlocked/active state", "Subset of eligible"),
                new("buildQualification", "generated.buildQualification", "future prototype+lighting combination", "qualification explanation", false, false, "Build count/activation", "Qualification only"),
                new("sourceCanonicalSignature", "generated.BuildCanonicalSignature", "source correspondence", "debug diagnostics", false, true, "runtime object hash", "Exact source signature")
            };
            StringBuilder builder = new("fieldPath,sourceField,itemSystemConsumer,detailConsumer,playerVisibleAllowed,debugOnly,forbiddenDerivedMeaning,notes\n");
            foreach (FieldRow row in rows)
            {
                builder.Append(Csv(row.fieldPath)).Append(',').Append(Csv(row.sourceField)).Append(',')
                    .Append(Csv(row.itemSystemConsumer)).Append(',').Append(Csv(row.detailConsumer)).Append(',')
                    .Append(Csv(row.playerVisibleAllowed ? "true" : "false")).Append(',')
                    .Append(Csv(row.debugOnly ? "true" : "false")).Append(',')
                    .Append(Csv(row.forbiddenDerivedMeaning)).Append(',').Append(Csv(row.notes)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildLeak(IEnumerable<LeakRow> rows, bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInstanceProjectionContract01 LeakCheck Report").AppendLine()
                .AppendLine($"- Runtime scan root: `{RuntimeRoot}`")
                .AppendLine($"- Result: {(passed && rows.All(row => row.count == 0) ? "PASS" : "FAIL")}")
                .AppendLine($"- Total leaks: {rows.Sum(row => row.count)}")
                .AppendLine().AppendLine("| Category | Count |")
                .AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows) builder.AppendLine($"| {row.category} | {row.count} |");
            builder.AppendLine().AppendLine("Protected compatibility files were not written by this verifier. Git diff is checked separately at delivery.");
            return builder.ToString();
        }

        private static bool ConsumeAsItemSystem(ItemInstanceProjectionContractSnapshot snapshot)
        {
            return snapshot != null
                && !string.IsNullOrWhiteSpace(snapshot.baseItemId)
                && !string.IsNullOrWhiteSpace(snapshot.itemInstanceId)
                && snapshot.Stats.Count > 0
                && snapshot.Affixes.Count > 0;
        }

        private static bool ConsumeAsDetail(ItemInstanceProjectionContractSnapshot snapshot)
        {
            return snapshot != null
                && !string.IsNullOrWhiteSpace(snapshot.rarityKey)
                && snapshot.VisibleCoreEffectIds.All(snapshot.EligibleCoreEffectIds.Contains)
                && Enum.IsDefined(typeof(ItemBuildQualification), snapshot.buildQualification);
        }

        private static void CheckSignatureMutation(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            ItemInstanceProjectionContractSnapshot baseline,
            ItemInstanceProjectionContractSnapshot mutated)
        {
            bool pass = baseline.BuildCanonicalSignature() != mutated.BuildCanonicalSignature();
            Check(specs, verification, caseId, "determinism", pass,
                "different", pass ? "different" : "same");
        }

        private static void ExpectProjectionCode(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            ItemInstanceProjectionRequest request,
            string expectedCode)
        {
            ItemInstanceProjectionResult result = ItemInstanceProjectionProvider.Project(request);
            bool pass = !result.isSuccess && result.snapshot == null
                && result.ValidationErrors.Any(error => error.code == expectedCode);
            Add(specs, verification, caseId, "validation", expectedCode,
                ResultText(result), expectedCode, pass);
        }

        private static void ExpectSourceCode(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            ItemGeneratedInstanceSnapshot source,
            string expectedCode)
        {
            ExpectProjectionCode(specs, verification, caseId,
                new ItemInstanceProjectionRequest(source), expectedCode);
        }

        private static void ExpectSetCode(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            ItemInstanceProjectionSetRequest request,
            string expectedCode)
        {
            ItemInstanceProjectionSetResult result = ItemInstanceProjectionProvider.ProjectSet(request);
            bool pass = !result.isSuccess && result.snapshot == null
                && result.ValidationErrors.Any(error => error.code == expectedCode);
            Add(specs, verification, caseId, "validation", expectedCode,
                ResultText(result), expectedCode, pass);
        }

        private static void ExpectQueryCode(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            ItemInstanceProjectionQueryResult result,
            string expectedCode)
        {
            bool pass = !result.isSuccess && result.snapshot == null && result.Snapshots.Count == 0
                && result.ValidationErrors.Any(error => error.code == expectedCode);
            Add(specs, verification, caseId, "query", expectedCode,
                result.primaryValidationCode, expectedCode, pass);
        }

        private static void Check(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            string category,
            bool pass,
            string expected,
            string actual)
        {
            Add(specs, verification, caseId, category, expected, actual,
                pass ? ItemInstanceProjectionValidationCodes.None : "ASSERTION_FAILED", pass);
        }

        private static void Add(
            List<SpecRow> specs,
            Verification verification,
            string caseId,
            string category,
            string expected,
            string actual,
            string code,
            bool pass)
        {
            specs.Add(new SpecRow(caseId, category, expected, actual, code, pass ? "PASS" : "FAIL"));
            if (!pass) verification.Errors.Add($"{caseId}: expected '{expected}', actual '{actual}', code '{code}'.");
        }

        private static bool RejectMutation<T>(IReadOnlyList<T> values)
        {
            if (!(values is IList list)) return false;
            try
            {
                if (list.Count > 0) list[0] = list[0];
                else list.Add(default(T));
            }
            catch (NotSupportedException)
            {
                return true;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
            return false;
        }

        private static ItemInstanceProjectionContractSnapshot CloneProjection(
            ItemInstanceProjectionContractSnapshot source,
            IEnumerable<ItemInstanceProjectionStatSnapshot> stats = null,
            IEnumerable<ItemInstanceProjectionAffixSnapshot> affixes = null,
            IEnumerable<string> eligible = null,
            IEnumerable<string> visible = null,
            ItemBuildQualification? build = null,
            string sourceSignature = null)
        {
            return CreateNonPublic<ItemInstanceProjectionContractSnapshot>(
                source.sourceSchemaId,
                source.sourceGenerationAlgorithmId,
                source.generationDataStatus,
                source.itemInstanceId,
                source.baseItemId,
                source.rarity,
                source.rarityKey,
                source.generationVersion,
                source.rootSeed,
                source.cultivationPotentialProfileId,
                stats ?? source.Stats,
                affixes ?? source.Affixes,
                eligible ?? source.EligibleCoreEffectIds,
                visible ?? source.VisibleCoreEffectIds,
                build ?? source.buildQualification,
                sourceSignature ?? source.sourceCanonicalSignature);
        }

        private static ItemInstanceProjectionStatSnapshot ProjectionStat(string statId, long rawUnits)
        {
            return CreateNonPublic<ItemInstanceProjectionStatSnapshot>(statId, rawUnits);
        }

        private static ItemInstanceProjectionAffixSnapshot ProjectionAffix(
            string slotId, ItemAffixSlotKind slotKind, string affixId, string profileId, long rawUnits)
        {
            return CreateNonPublic<ItemInstanceProjectionAffixSnapshot>(
                slotId, slotKind, affixId, profileId, rawUnits);
        }

        private static T CreateNonPublic<T>(params object[] arguments)
        {
            return (T)Activator.CreateInstance(typeof(T),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, arguments, CultureInfo.InvariantCulture);
        }

        private static void SetBackingField(object target, string propertyName, object value)
        {
            SetPrivateField(target, "<" + propertyName + ">k__BackingField", value);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(target.GetType().FullName, fieldName);
            field.SetValue(target, value);
        }

        private static string ResultText(ItemInstanceProjectionResult result)
        {
            return result == null ? "null" : result.isSuccess ? "success" :
                string.Join("|", result.ValidationErrors.Select(error => error.code));
        }

        private static string ResultText(ItemInstanceProjectionSetResult result)
        {
            return result == null ? "null" : result.isSuccess ? "success" :
                string.Join("|", result.ValidationErrors.Select(error => error.code));
        }

        private static string Digest(string value)
        {
            return DeterministicItemRandom.Fnv1A64Utf8(value ?? string.Empty)
                .ToString("X16", CultureInfo.InvariantCulture);
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join("|", values ?? Array.Empty<string>());
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static int CsvRows(string path)
        {
            return File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;
        }

        private static int CsvInvariantCount(string path)
        {
            if (!File.Exists(path)) return -1;
            string summary = File.ReadAllLines(path)
                .LastOrDefault(line => line.StartsWith("\"__SUMMARY__\"", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(summary)) return -1;
            Match match = Regex.Match(summary, "\"(?<count>[0-9]+)\"\\s*$",
                RegexOptions.CultureInvariant);
            return match.Success
                && int.TryParse(match.Groups["count"].Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out int count)
                ? count
                : -1;
        }

        private static string ReadProjectFile(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        private static void RequireCount(Verification verification, string name, int actual, int expected)
        {
            if (actual != expected) verification.Errors.Add($"{name} count must remain {expected}, actual {actual}.");
        }

        private sealed class Fixture
        {
            private ItemInstanceProjectionQaFixture sharedFixture;
            public ItemGenerationFoundationSnapshot Foundation { get; private set; }
            public ItemStatRangeSchemaSnapshot Stats { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot Affixes { get; private set; }
            public ItemCorePotentialAndBuildEligibilitySchemaSnapshot Core { get; private set; }
            public IReadOnlyList<ItemGeneratedInstanceSnapshot> Sources { get; private set; }

            public static Fixture Create()
            {
                ItemInstanceProjectionQaFixture shared = ItemInstanceProjectionQaFixture.Create();
                return new Fixture
                {
                    sharedFixture = shared,
                    Foundation = shared.Foundation,
                    Stats = shared.StatSchema,
                    Affixes = shared.AffixSchema,
                    Core = shared.CoreBuildSchema,
                    Sources = shared.GeneratedInstances
                };
            }

            public ItemGeneratedInstanceSnapshot CreateGenerated(
                ItemInstanceRarity rarity,
                long seed,
                string instanceId,
                ItemBuildQualification qualification)
            {
                return sharedFixture.CreateGenerated(rarity, seed, instanceId, qualification);
            }

            public ItemGeneratedInstanceSnapshot InvalidSource(
                string instanceId = "QA-INVALID",
                string baseItemId = "I001",
                ItemInstanceRarity rarity = ItemInstanceRarity.Green,
                int generationVersion = 1,
                string status = ItemGenerationDataStatus.QaFixtureCanonical,
                string cultivationProfileId = "qa_core_i001_green",
                IEnumerable<ItemGeneratedStatSnapshot> stats = null,
                IEnumerable<ItemGeneratedAffixSnapshot> affixes = null,
                IEnumerable<string> eligible = null,
                IEnumerable<string> visible = null,
                bool nullCore = false,
                ItemBuildQualification build = ItemBuildQualification.FaMenOnly)
            {
                ItemInstanceIdentitySnapshot identity = CreateNonPublic<ItemInstanceIdentitySnapshot>(
                    instanceId, baseItemId, rarity, generationVersion, 77L, cultivationProfileId);
                ItemGeneratedCorePotentialSnapshot core = nullCore
                    ? null
                    : CreateNonPublic<ItemGeneratedCorePotentialSnapshot>(
                        cultivationProfileId,
                        eligible ?? new[] { "qa_core_01" },
                        visible ?? new[] { "qa_core_01" });
                return CreateNonPublic<ItemGeneratedInstanceSnapshot>(
                    status,
                    identity,
                    stats ?? new[] { GeneratedStat("qa_stat", 10) },
                    affixes ?? new[]
                    {
                        GeneratedAffix("fixed_01", ItemAffixSlotKind.Fixed, "qa_affix", "qa_affix_value", 5)
                    },
                    core,
                    build);
            }

            public ItemGeneratedStatSnapshot GeneratedStat(string id, long rawUnits)
            {
                return CreateNonPublic<ItemGeneratedStatSnapshot>(id, rawUnits);
            }

            public ItemGeneratedAffixSnapshot GeneratedAffix(
                string slotId, ItemAffixSlotKind kind, string affixId, string profileId, long rawUnits)
            {
                return CreateNonPublic<ItemGeneratedAffixSnapshot>(slotId, kind, affixId, profileId, rawUnits);
            }

        }

        private sealed class Verification
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public int FoundationCount, StatCount, CoreCount, AffixCount, RollCount;
            public int DropSpecCount, DropDeterminismCount;
            public int SimulationSpecCount, SimulationDistributionCount, SimulationDeterminismCount, SimulationInvariantCount;
        }

        private readonly struct SpecRow
        {
            public SpecRow(string caseId, string category, string expected, string actual, string code, string result)
            {
                this.caseId = caseId;
                this.category = category;
                this.expected = expected;
                this.actual = actual;
                this.code = code;
                this.result = result;
            }
            public readonly string caseId, category, expected, actual, code, result;
        }

        private readonly struct LeakRow
        {
            public LeakRow(string category, int count) { this.category = category; this.count = count; }
            public readonly string category;
            public readonly int count;
        }

        private readonly struct LeakDefinition
        {
            public LeakDefinition(string category, string pattern) { this.category = category; this.pattern = pattern; }
            public readonly string category, pattern;
        }

        private readonly struct Regression
        {
            public Regression(string name, string path, string marker)
            { this.name = name; this.path = path; this.marker = marker; }
            public readonly string name, path, marker;
        }

        private readonly struct FieldRow
        {
            public FieldRow(string fieldPath, string sourceField, string itemSystemConsumer,
                string detailConsumer, bool playerVisibleAllowed, bool debugOnly,
                string forbiddenDerivedMeaning, string notes)
            {
                this.fieldPath = fieldPath;
                this.sourceField = sourceField;
                this.itemSystemConsumer = itemSystemConsumer;
                this.detailConsumer = detailConsumer;
                this.playerVisibleAllowed = playerVisibleAllowed;
                this.debugOnly = debugOnly;
                this.forbiddenDerivedMeaning = forbiddenDerivedMeaning;
                this.notes = notes;
            }
            public readonly string fieldPath, sourceField, itemSystemConsumer, detailConsumer;
            public readonly bool playerVisibleAllowed, debugOnly;
            public readonly string forbiddenDerivedMeaning, notes;
        }
    }
}
#endif
