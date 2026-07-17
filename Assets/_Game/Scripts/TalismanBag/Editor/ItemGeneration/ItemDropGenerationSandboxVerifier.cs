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
using TalismanBag.Items.Generation.DropSandbox;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemDropGenerationSandboxVerifier
    {
        private const string PackageName = "V0.4-ItemDropGenerationSandbox01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemDropGenerationSandboxReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemDropGenerationSandboxSpec.csv";
        private const string DeterminismPath = "Docs/V0.4/Reports/ItemDropGenerationSandboxDeterminism.csv";
        private const string SamplesPath = "Docs/V0.4/Reports/ItemDropGenerationSandboxSamples.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemDropGenerationSandboxLeakCheckReport.md";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/DropSandbox";
        private const string PassMarker = "ITEM_DROP_GENERATION_SANDBOX01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemDropGenerationSandbox01/[QA Only] Run Sandbox")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            Verification verification = new();
            List<SpecRow> specs = new();
            List<DeterminismRow> determinism = new();
            List<SampleRow> samples = new();
            List<LeakRow> leaks = new();
            ItemDropGenerationResult sample = null;
            try
            {
                Fixture fixture = Fixture.Create();
                sample = RunSuccessAndGoldenCases(fixture, specs, determinism, samples, verification);
                RunCandidateAndEarlyStageCases(fixture, specs, samples, verification);
                RunHigherStageCases(fixture, specs, samples, verification);
                RunFailureCases(fixture, specs, verification);
                RunDeterminismAndReadOnlyCases(fixture, specs, determinism, verification);
                RunLeakChecks(leaks, verification);
                RunHistoricalRegressions(verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add("Verifier threw an unhandled exception: " + exception);
            }

            bool passed = verification.Errors.Count == 0
                && specs.Count >= 60
                && specs.All(row => row.result == "PASS")
                && determinism.All(row => row.result == "PASS")
                && leaks.All(row => row.count == 0);
            WriteReports(sample, specs, determinism, samples, leaks, verification, passed);
            if (passed)
            {
                Debug.Log(PassMarker + $" specs={specs.Count} determinism={determinism.Count} samples={samples.Count}");
            }
            else
            {
                Debug.LogError("ITEM_DROP_GENERATION_SANDBOX01_FAIL\n" + string.Join("\n", verification.Errors));
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static ItemDropGenerationResult RunSuccessAndGoldenCases(
            Fixture fixture,
            List<SpecRow> specs,
            List<DeterminismRow> determinism,
            List<SampleRow> samples,
            Verification verification)
        {
            ItemDropGenerationResult sample = ItemDropGenerationSandbox.Generate(fixture.Request());
            CheckBool(specs, verification, "basic-success", "basic", sample.isSuccess);
            CheckBool(specs, verification, "drop-schema-v1", "basic",
                sample.snapshot?.schemaId == ItemDropGenerationSnapshot.CurrentSchemaId);
            CheckBool(specs, verification, "drop-algorithm-v1", "basic",
                sample.snapshot?.dropAlgorithmId == DeterministicItemDropRandom.AlgorithmId);
            CheckBool(specs, verification, "qa-status-output", "isolation",
                sample.snapshot?.generationDataStatus == ItemGenerationDataStatus.QaFixtureCanonical);
            CheckBool(specs, verification, "complete-generated-instance", "basic",
                sample.snapshot?.generatedInstance?.schemaId == ItemGeneratedInstanceSnapshot.CurrentSchemaId);
            CheckBool(specs, verification, "canonical-signature-stable", "basic",
                sample.isSuccess && sample.snapshot.BuildCanonicalSignature()
                    == ItemDropGenerationSandbox.Generate(fixture.Request()).snapshot?.BuildCanonicalSignature());

            bool candidateOk = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-001", "CTX-001", "qa_stage_clear", 1,
                new[] { "drop", "candidate", "qa_pool_30" }, out ulong candidateSeed);
            Check(specs, verification, "candidate-domain-golden", "golden",
                "6DEE1D6E50111E1B", candidateOk ? Hex(candidateSeed) : "INVALID");
            determinism.Add(new DeterminismRow("candidate-domain-golden", 42L, 1,
                "drop|candidate|qa_pool_30", "6DEE1D6E50111E1B",
                candidateOk ? Hex(candidateSeed) : "INVALID", 1,
                candidateOk && Hex(candidateSeed) == "6DEE1D6E50111E1B" ? "PASS" : "FAIL"));

            bool rarityOk = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-001", "CTX-001", "qa_stage_clear", 11,
                new[] { "drop", "rarity", "qa_rarity_11_plus" }, out ulong raritySeed);
            Check(specs, verification, "rarity-domain-golden", "golden",
                "7882C5BB16185E78", rarityOk ? Hex(raritySeed) : "INVALID");
            determinism.Add(new DeterminismRow("rarity-domain-golden", 42L, 11,
                "drop|rarity|qa_rarity_11_plus", "7882C5BB16185E78",
                rarityOk ? Hex(raritySeed) : "INVALID", 1,
                rarityOk && Hex(raritySeed) == "7882C5BB16185E78" ? "PASS" : "FAIL"));

            bool collisionA = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-001", "CTX-001", "qa_stage_clear", 1,
                new[] { "drop", "candidate", "a/b", "c" }, out ulong collisionASeed);
            bool collisionB = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-001", "CTX-001", "qa_stage_clear", 1,
                new[] { "drop", "candidate", "a", "b/c" }, out ulong collisionBSeed);
            string collisionActual = collisionA && collisionB
                ? Hex(collisionASeed) + "/" + Hex(collisionBSeed)
                : "INVALID";
            Check(specs, verification, "segmented-domain-collision-vector", "golden",
                "57B30C3DC752992A/12F4959138EDF7BC", collisionActual);
            CheckBool(specs, verification, "segmented-domain-collision-separated", "domain",
                collisionA && collisionB && collisionASeed != collisionBSeed);
            CheckBool(specs, verification, "domain-empty-segment-rejected", "domain",
                !DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                    "DROP-001", "CTX-001", "qa_stage_clear", 1,
                    new[] { "drop", string.Empty }, out _));
            return sample;
        }

        private static void RunCandidateAndEarlyStageCases(
            Fixture fixture,
            List<SpecRow> specs,
            List<SampleRow> samples,
            Verification verification)
        {
            string[] expected = Enumerable.Range(1, 30).Select(index => $"I{index:000}").ToArray();
            CheckBool(specs, verification, "candidate-pool-exact-i001-i030", "candidate",
                fixture.Candidate30.Candidates.Select(entry => entry.baseItemId)
                    .OrderBy(id => id, StringComparer.Ordinal).SequenceEqual(expected));
            CheckBool(specs, verification, "candidate-pool-count-30", "candidate",
                fixture.Candidate30.Candidates.Count == 30);
            CheckBool(specs, verification, "candidate-pool-no-i031", "candidate",
                fixture.Candidate30.Candidates.All(entry => entry.baseItemId != "I031"));

            bool allOrdinary = true;
            for (long seed = 0; seed < 64; seed++)
            {
                ItemDropGenerationResult generated = ItemDropGenerationSandbox.Generate(
                    fixture.Request(seed: seed, dropId: "DROP-CAND-" + seed,
                        instanceId: "INSTANCE-CAND-" + seed));
                allOrdinary &= generated.isSuccess
                    && ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(generated.snapshot.selectedBaseItemId);
                if (seed < 30 && generated.isSuccess)
                {
                    samples.Add(Sample("candidate", generated));
                }
            }
            CheckBool(specs, verification, "candidate-many-seeds-stay-i001-i030", "candidate", allOrdinary);

            for (int stage = 1; stage <= 10; stage++)
            {
                ItemDropGenerationResult generated = ItemDropGenerationSandbox.Generate(
                    fixture.Request(stage: stage, seed: stage * 101L,
                        dropId: "DROP-STAGE-" + stage, instanceId: "INSTANCE-STAGE-" + stage));
                CheckBool(specs, verification, "stage-" + stage + "-locked-white", "early-stage",
                    generated.isSuccess
                    && generated.snapshot.selectedRarity == ItemInstanceRarity.White
                    && generated.snapshot.generatedInstance.buildQualification == ItemBuildQualification.None);
            }

            bool manySeedsWhite = true;
            for (long seed = 100; seed < 180; seed++)
            {
                ItemDropGenerationResult generated = ItemDropGenerationSandbox.Generate(
                    fixture.Request(stage: (int)(seed % 10L) + 1, seed: seed,
                        dropId: "DROP-WHITE-" + seed, instanceId: "INSTANCE-WHITE-" + seed));
                manySeedsWhite &= generated.isSuccess
                    && generated.snapshot.selectedRarity == ItemInstanceRarity.White
                    && generated.snapshot.generatedInstance.buildQualification == ItemBuildQualification.None;
            }
            CheckBool(specs, verification, "stage-1-10-many-seeds-white", "early-stage", manySeedsWhite);
            CheckBool(specs, verification, "early-stage-policy-source", "early-stage",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 10)).snapshot?.rarityPolicySource
                    == ItemDropRarityPolicySources.LockedStage1To10White);
            CheckBool(specs, verification, "early-stage-rarity-profile-id-empty", "early-stage",
                string.IsNullOrEmpty(ItemDropGenerationSandbox.Generate(fixture.Request(stage: 1)).snapshot?.rarityWeightProfileId));
            ExpectFailure(specs, verification, "early-stage-profile-injection", "early-stage",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 1,
                    rarity: fixture.Rarity, forceRarity: true)),
                ItemDropGenerationValidationCodes.EarlyStageRarityProfileForbidden);
        }

        private static void RunHigherStageCases(
            Fixture fixture,
            List<SpecRow> specs,
            List<SampleRow> samples,
            Verification verification)
        {
            ExpectFailure(specs, verification, "stage11-no-profile", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11, rarity: null, forceRarity: true)),
                ItemDropGenerationValidationCodes.RarityPolicyUnresolved);

            HashSet<ItemInstanceRarity> profileRarities = new(fixture.Rarity.Entries.Select(entry => entry.rarity));
            bool outputFromProfile = true;
            bool higherBuildMatched = true;
            for (long seed = 0; seed < 80; seed++)
            {
                ItemDropGenerationResult generated = ItemDropGenerationSandbox.Generate(
                    fixture.Request(stage: 11, seed: seed, dropId: "DROP-HIGH-" + seed,
                        instanceId: "INSTANCE-HIGH-" + seed));
                outputFromProfile &= generated.isSuccess && profileRarities.Contains(generated.snapshot.selectedRarity);
                if (generated.isSuccess && generated.snapshot.selectedRarity != ItemInstanceRarity.White)
                {
                    ItemBuildQualification expected = fixture.Builds.Single(profile =>
                        profile.rarity == generated.snapshot.selectedRarity).Entries.Single().qualification;
                    higherBuildMatched &= generated.snapshot.generatedInstance.buildQualification == expected;
                }
                if (seed < 10 && generated.isSuccess)
                {
                    samples.Add(Sample("stage11", generated));
                }
            }
            CheckBool(specs, verification, "stage11-output-from-profile", "rarity", outputFromProfile);
            CheckBool(specs, verification, "stage11-higher-build-profile-matched", "build", higherBuildMatched);

            ItemDropRarityWeightProfileSnapshot greenOnly = fixture.RarityProfile(
                new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, 1));
            ItemDropGenerationResult green = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 11, rarity: greenOnly, forceRarity: true));
            CheckBool(specs, verification, "stage11-green-uses-green-build-profile", "build",
                green.isSuccess && green.snapshot.generatedInstance.buildQualification == ItemBuildQualification.FaMenOnly);
            ExpectFailure(specs, verification, "stage11-green-build-profile-missing", "build",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: greenOnly, forceRarity: true,
                    builds: Array.Empty<ItemBuildQualificationRollProfile>(), forceBuilds: true)),
                ItemDropGenerationValidationCodes.BuildRollProfileMissing);
            ExpectFailure(specs, verification, "stage11-green-build-profile-duplicate", "build",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: greenOnly, forceRarity: true,
                    builds: new[] { fixture.Builds[0], fixture.Builds[0] }, forceBuilds: true)),
                ItemDropGenerationValidationCodes.BuildRollProfileDuplicate);
        }

        private static void RunFailureCases(Fixture fixture, List<SpecRow> specs, Verification verification)
        {
            ExpectFailure(specs, verification, "request-null", "request",
                ItemDropGenerationSandbox.Generate(null), ItemDropGenerationValidationCodes.RequestNull);
            ExpectFailure(specs, verification, "drop-request-id-empty", "request",
                ItemDropGenerationSandbox.Generate(fixture.Request(dropId: string.Empty)),
                ItemDropGenerationValidationCodes.DropRequestIdEmpty);
            ExpectFailure(specs, verification, "item-instance-id-empty", "request",
                ItemDropGenerationSandbox.Generate(fixture.Request(instanceId: string.Empty)),
                ItemDropGenerationValidationCodes.ItemInstanceIdEmpty);
            ExpectFailure(specs, verification, "generation-version-unsupported", "request",
                ItemDropGenerationSandbox.Generate(fixture.Request(version: 2)),
                ItemDropGenerationValidationCodes.GenerationVersionUnsupported);
            ExpectFailure(specs, verification, "generation-status-invalid", "isolation",
                ItemDropGenerationSandbox.Generate(fixture.Request(status: "FORMAL")),
                ItemDropGenerationValidationCodes.GenerationDataStatusInvalid);
            ExpectFailure(specs, verification, "foundation-null", "foundation",
                ItemDropGenerationSandbox.Generate(fixture.Request(foundation: null, forceFoundation: true)),
                ItemDropGenerationValidationCodes.FoundationNull);
            ItemGenerationFoundationSnapshot invalidFoundation =
                ItemRarityInstanceFoundation.Create(Array.Empty<ItemInnerDataDefinition>());
            ExpectFailure(specs, verification, "foundation-invalid", "foundation",
                ItemDropGenerationSandbox.Generate(fixture.Request(
                    foundation: invalidFoundation, forceFoundation: true)),
                ItemDropGenerationValidationCodes.FoundationInvalid);

            ExpectFailure(specs, verification, "source-null", "source",
                ItemDropGenerationSandbox.Generate(fixture.Request(source: null, forceSource: true)),
                ItemDropGenerationValidationCodes.SourceContextNull);
            ExpectFailure(specs, verification, "source-context-id-empty", "source",
                ItemDropGenerationSandbox.Generate(fixture.Request(source: fixture.Source(1, contextId: string.Empty),
                    forceSource: true)), ItemDropGenerationValidationCodes.SourceContextIdEmpty);
            ExpectFailure(specs, verification, "source-key-empty", "source",
                ItemDropGenerationSandbox.Generate(fixture.Request(source: fixture.Source(1, sourceKey: string.Empty),
                    forceSource: true)), ItemDropGenerationValidationCodes.SourceKeyEmpty);
            ExpectFailure(specs, verification, "stage-number-invalid", "source",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 0)),
                ItemDropGenerationValidationCodes.StageNumberInvalid);
            ExpectFailure(specs, verification, "source-status-invalid", "source",
                ItemDropGenerationSandbox.Generate(fixture.Request(source: fixture.Source(1, maturity: "FORMAL"),
                    forceSource: true)), ItemDropGenerationValidationCodes.SourceContextDataStatusInvalid);

            ExpectFailure(specs, verification, "candidate-pool-null", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: null, forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidatePoolNull);
            ItemDropCandidatePoolSnapshot wrongPool = fixture.CandidatePool("wrong_pool",
                new ItemDropCandidateEntrySnapshot("I001", 1));
            ExpectFailure(specs, verification, "candidate-pool-id-mismatch", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: wrongPool, forceCandidate: true,
                    source: fixture.Source(1, candidatePoolId: "qa_pool_30"), forceSource: true)),
                ItemDropGenerationValidationCodes.CandidatePoolIdMismatch);
            ExpectFailure(specs, verification, "candidate-pool-status-invalid", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate:
                    new ItemDropCandidatePoolSnapshot("qa_pool_30", "FORMAL",
                        new[] { new ItemDropCandidateEntrySnapshot("I001", 1) }), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidatePoolDataStatusInvalid);
            ExpectFailure(specs, verification, "candidate-pool-empty", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30"),
                    forceCandidate: true)), ItemDropGenerationValidationCodes.CandidatePoolEmpty);
            ExpectFailure(specs, verification, "candidate-null-entry", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot[] { null }), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateBaseItemEmpty);
            ExpectFailure(specs, verification, "candidate-empty-id", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot(string.Empty, 1)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateBaseItemEmpty);
            ExpectFailure(specs, verification, "candidate-i031", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I031", 1)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.I031Forbidden);
            ExpectFailure(specs, verification, "candidate-i999", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I999", 1)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateBaseItemUnknown);
            ExpectFailure(specs, verification, "candidate-duplicate", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I001", 1),
                    new ItemDropCandidateEntrySnapshot("I001", 2)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateDuplicate);
            ExpectFailure(specs, verification, "candidate-zero-weight", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I001", 0)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateWeightInvalid);
            ExpectFailure(specs, verification, "candidate-negative-weight", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I001", -1)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateWeightInvalid);
            ExpectFailure(specs, verification, "candidate-weight-overflow", "candidate",
                ItemDropGenerationSandbox.Generate(fixture.Request(candidate: fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I001", long.MaxValue),
                    new ItemDropCandidateEntrySnapshot("I002", long.MaxValue),
                    new ItemDropCandidateEntrySnapshot("I003", long.MaxValue)), forceCandidate: true)),
                ItemDropGenerationValidationCodes.CandidateWeightSumOverflow);

            ExpectFailure(specs, verification, "rarity-profile-id-mismatch", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    source: fixture.Source(11, rarityProfileId: "wrong"), forceSource: true)),
                ItemDropGenerationValidationCodes.RarityProfileIdMismatch);
            ExpectFailure(specs, verification, "rarity-profile-status-invalid", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: new ItemDropRarityWeightProfileSnapshot("qa_rarity_11_plus", "FORMAL",
                        fixture.Rarity.Entries), forceRarity: true)),
                ItemDropGenerationValidationCodes.RarityProfileDataStatusInvalid);
            ExpectFailure(specs, verification, "rarity-profile-empty", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(), forceRarity: true)),
                ItemDropGenerationValidationCodes.RarityEntryInvalid);
            ExpectFailure(specs, verification, "rarity-null-entry", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(new ItemDropRarityWeightEntrySnapshot[] { null }),
                    forceRarity: true)), ItemDropGenerationValidationCodes.RarityEntryInvalid);
            ExpectFailure(specs, verification, "rarity-duplicate", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, 1),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, 2)), forceRarity: true)),
                ItemDropGenerationValidationCodes.RarityEntryDuplicate);
            ExpectFailure(specs, verification, "rarity-illegal-enum", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(new ItemDropRarityWeightEntrySnapshot((ItemInstanceRarity)999, 1)),
                    forceRarity: true)), ItemDropGenerationValidationCodes.RarityEntryInvalid);
            ExpectFailure(specs, verification, "rarity-zero-weight", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, 0)),
                    forceRarity: true)), ItemDropGenerationValidationCodes.RarityWeightInvalid);
            ExpectFailure(specs, verification, "rarity-negative-weight", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, -1)),
                    forceRarity: true)), ItemDropGenerationValidationCodes.RarityWeightInvalid);
            ExpectFailure(specs, verification, "rarity-weight-overflow", "rarity",
                ItemDropGenerationSandbox.Generate(fixture.Request(stage: 11,
                    rarity: fixture.RarityProfile(
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, long.MaxValue),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Blue, long.MaxValue),
                        new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Purple, long.MaxValue)),
                    forceRarity: true)), ItemDropGenerationValidationCodes.RarityWeightSumOverflow);

            ItemDropGenerationRequest emptyDataRequest = new("DROP-EMPTY-DATA", "INSTANCE-EMPTY-DATA", 7L, 1,
                ItemGenerationDataStatus.QaFixtureCanonical, fixture.Foundation, fixture.Source(1), fixture.CandidateOne,
                null, ItemStatRangeSchema.Create(Array.Empty<ItemStatDefinitionSnapshot>(),
                    Array.Empty<ItemStatRangeProfileSnapshot>()),
                ItemAffixPoolAndRangeCatalog.CreateDefaultSchema(),
                ItemCorePotentialAndBuildEligibilityCatalog.CreateDefaultSchema(),
                Array.Empty<ItemBuildQualificationRollProfile>());
            ExpectFailure(specs, verification, "default-formal-empty-data-cannot-generate", "isolation",
                ItemDropGenerationSandbox.Generate(emptyDataRequest),
                ItemDropGenerationValidationCodes.CoreProfileMissing);

            ItemDropGenerationRequest engineFailure = new("DROP-ENGINE-FAIL", "INSTANCE-ENGINE-FAIL", 7L, 1,
                ItemGenerationDataStatus.QaFixtureCanonical, fixture.Foundation, fixture.Source(1), fixture.CandidateOne,
                null, ItemStatRangeSchema.Create(Array.Empty<ItemStatDefinitionSnapshot>(),
                    Array.Empty<ItemStatRangeProfileSnapshot>()), fixture.Affixes, fixture.Core, fixture.Builds);
            ItemDropGenerationResult failed = ItemDropGenerationSandbox.Generate(engineFailure);
            ExpectFailure(specs, verification, "instance-roll-failure", "failure", failed,
                ItemDropGenerationValidationCodes.InstanceRollFailed);
            CheckBool(specs, verification, "failure-never-returns-partial-snapshot", "failure",
                !failed.isSuccess && failed.snapshot == null);
        }

        private static void RunDeterminismAndReadOnlyCases(
            Fixture fixture,
            List<SpecRow> specs,
            List<DeterminismRow> determinism,
            Verification verification)
        {
            ItemDropGenerationRequest request = fixture.Request(stage: 11);
            ItemDropGenerationResult first = ItemDropGenerationSandbox.Generate(request);
            string expected = first.snapshot?.BuildCanonicalSignature() ?? string.Empty;
            bool repeatStable = first.isSuccess;
            for (int index = 0; index < 1000 && repeatStable; index++)
            {
                ItemDropGenerationResult repeat = ItemDropGenerationSandbox.Generate(request);
                repeatStable = repeat.isSuccess
                    && repeat.snapshot.BuildCanonicalSignature() == expected;
            }
            CheckBool(specs, verification, "same-input-repeat-1000", "determinism", repeatStable);
            determinism.Add(new DeterminismRow("repeat-1000", request.rootSeed, 11, "full-request",
                Compact(expected), repeatStable ? Compact(expected) : "MISMATCH", 1000,
                repeatStable ? "PASS" : "FAIL"));

            ItemDropCandidatePoolSnapshot reversedCandidates = new(fixture.Candidate30.poolId,
                fixture.Candidate30.dataMaturityKey, fixture.Candidate30.Candidates.Reverse());
            ItemDropGenerationResult candidateBaseline = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 1));
            ItemDropGenerationResult reversedCandidateResult = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 1, candidate: reversedCandidates, forceCandidate: true,
                    source: fixture.Source(1, candidatePoolId: reversedCandidates.poolId), forceSource: true));
            CheckBool(specs, verification, "candidate-input-reversal-stable", "determinism",
                candidateBaseline.isSuccess && reversedCandidateResult.isSuccess
                && reversedCandidateResult.snapshot.BuildCanonicalSignature()
                    == candidateBaseline.snapshot.BuildCanonicalSignature());

            ItemDropRarityWeightProfileSnapshot reversedRarity = new(fixture.Rarity.profileId,
                fixture.Rarity.dataMaturityKey, fixture.Rarity.Entries.Reverse());
            ItemDropGenerationResult reversedRarityResult = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 11, rarity: reversedRarity, forceRarity: true));
            CheckBool(specs, verification, "rarity-input-reversal-stable", "determinism",
                reversedRarityResult.isSuccess && reversedRarityResult.snapshot.BuildCanonicalSignature() == expected);

            ItemDropGenerationResult reversedBuildResult = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 11, builds: fixture.Builds.Reverse(), forceBuilds: true));
            CheckBool(specs, verification, "build-profile-input-reversal-stable", "determinism",
                reversedBuildResult.isSuccess && reversedBuildResult.snapshot.BuildCanonicalSignature() == expected);

            ItemDropGenerationResult differentInstance = ItemDropGenerationSandbox.Generate(
                fixture.Request(stage: 11, instanceId: "INSTANCE-OTHER"));
            CheckBool(specs, verification, "instance-id-separates-instance-domain", "determinism",
                differentInstance.isSuccess
                && differentInstance.snapshot.selectedBaseItemId == first.snapshot.selectedBaseItemId
                && differentInstance.snapshot.selectedRarity == first.snapshot.selectedRarity
                && differentInstance.snapshot.generatedInstance.BuildCanonicalSignature()
                    != first.snapshot.generatedInstance.BuildCanonicalSignature());

            bool requestA = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-001", "CTX-001", "qa_stage_clear", 11,
                new[] { "drop", "candidate", "qa_pool_30" }, out ulong requestSeedA);
            bool requestB = DeterministicItemDropRandom.TryComputeDomainSeed(42L, 1,
                "DROP-OTHER", "CTX-001", "qa_stage_clear", 11,
                new[] { "drop", "candidate", "qa_pool_30" }, out ulong requestSeedB);
            CheckBool(specs, verification, "drop-request-id-separates-drop-domain", "determinism",
                requestA && requestB && requestSeedA != requestSeedB);

            foreach (long seed in new[] { long.MinValue, 0L, long.MaxValue })
            {
                ItemDropGenerationRequest edge = fixture.Request(stage: 11, seed: seed,
                    dropId: "DROP-EDGE-" + seed.ToString(CultureInfo.InvariantCulture),
                    instanceId: "INSTANCE-EDGE-" + seed.ToString(CultureInfo.InvariantCulture));
                ItemDropGenerationResult edgeA = ItemDropGenerationSandbox.Generate(edge);
                ItemDropGenerationResult edgeB = ItemDropGenerationSandbox.Generate(edge);
                bool stable = edgeA.isSuccess && edgeB.isSuccess
                    && edgeA.snapshot.BuildCanonicalSignature() == edgeB.snapshot.BuildCanonicalSignature();
                CheckBool(specs, verification, "root-seed-edge-" + seed.ToString(CultureInfo.InvariantCulture),
                    "determinism", stable);
                determinism.Add(new DeterminismRow("root-seed-edge", seed, 11, "full-request",
                    edgeA.snapshot == null ? "SUCCESS" : Compact(edgeA.snapshot.BuildCanonicalSignature()),
                    edgeB.snapshot == null ? "FAIL" : Compact(edgeB.snapshot.BuildCanonicalSignature()),
                    2, stable ? "PASS" : "FAIL"));
            }

            CheckBool(specs, verification, "candidate-collection-readonly", "readonly",
                RejectMutation(request.candidatePool.Candidates));
            CheckBool(specs, verification, "rarity-collection-readonly", "readonly",
                RejectMutation(request.rarityWeightProfile.Entries));
            CheckBool(specs, verification, "build-profile-collection-readonly", "readonly",
                RejectMutation(request.BuildQualificationRollProfiles));
            CheckBool(specs, verification, "validation-errors-readonly", "readonly",
                RejectMutation(ItemDropGenerationSandbox.Generate(null).ValidationErrors));
            CheckBool(specs, verification, "snapshot-properties-get-only", "readonly",
                typeof(ItemDropGenerationSnapshot).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .All(property => property.SetMethod == null));

            List<ItemDropCandidateEntrySnapshot> mutableCandidates = fixture.Candidate30.Candidates.ToList();
            List<ItemBuildQualificationRollProfile> mutableBuilds = fixture.Builds.ToList();
            ItemDropGenerationRequest immutableRequest = fixture.Request(stage: 1,
                candidate: new ItemDropCandidatePoolSnapshot("qa_pool_30",
                    ItemGenerationDataStatus.QaFixtureOnly, mutableCandidates), forceCandidate: true,
                builds: mutableBuilds, forceBuilds: true);
            ItemDropGenerationResult immutableBefore = ItemDropGenerationSandbox.Generate(immutableRequest);
            mutableCandidates.Clear();
            mutableBuilds.Clear();
            ItemDropGenerationResult immutableAfter = ItemDropGenerationSandbox.Generate(immutableRequest);
            CheckBool(specs, verification, "source-mutation-does-not-change-request", "readonly",
                immutableBefore.isSuccess && immutableAfter.isSuccess
                && immutableBefore.snapshot.BuildCanonicalSignature()
                    == immutableAfter.snapshot.BuildCanonicalSignature());

            string[] forbidden =
            {
                "placementId", "isLit", "countedInBuild", "unlockedCoreEffectIds",
                "activeCoreEffectIds", "itemPower"
            };
            string[] names = typeof(ItemDropGenerationSnapshot).GetProperties()
                .Select(property => property.Name).ToArray();
            CheckBool(specs, verification, "forbidden-output-fields-absent", "contract",
                forbidden.All(name => !names.Contains(name, StringComparer.OrdinalIgnoreCase)));
        }

        private static void RunLeakChecks(List<LeakRow> rows, Verification verification)
        {
            string source = string.Join("\n", Directory.Exists(RuntimeRoot)
                ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText)
                : Array.Empty<string>());
            LeakDefinition[] definitions =
            {
                new("System.Random", "System.Random"), new("UnityEngine.Random", "UnityEngine.Random"),
                new("Guid.NewGuid", "Guid.NewGuid"), new("DateTime.Now", "DateTime.Now"),
                new("DateTime.UtcNow", "DateTime.UtcNow"), new("Environment.TickCount", "Environment.TickCount"),
                new("string.GetHashCode", "string.GetHashCode"), new("Reward", "Reward"),
                new("RunFlow", "RunFlow"), new("Inventory", "Inventory"), new("SaveData", "SaveData"),
                new("PlayerPrefs", "PlayerPrefs"), new("Boss", "Boss"),
                new("Battle Resolver", "BattleResolver"), new("Battle Bridge", "BattleBridge"),
                new("ItemBuildSynergyResolver", "ItemBuildSynergyResolver"),
                new("ItemCoreAwakeningResolver", "ItemCoreAwakeningResolver"),
                new("BuildSandbox", "BuildSandbox"), new("DropTable", "DropTable"),
                new("LootTable", "LootTable"), new("Scene", "Scene"), new("Prefab", "Prefab"),
                new("BuildSettings", "BuildSettings"), new("placementId", "placementId"),
                new("isLit", "isLit"), new("countedInBuild", "countedInBuild"),
                new("unlockedCoreEffectIds", "unlockedCoreEffectIds"),
                new("activeCoreEffectIds", "activeCoreEffectIds"), new("itemPower", "itemPower"),
                new("AssetDatabase", "AssetDatabase"), new("UnityEditor", "UnityEditor")
            };
            foreach (LeakDefinition definition in definitions)
            {
                int count = Count(source, definition.token);
                rows.Add(new LeakRow(definition.category, count));
                if (count != 0)
                {
                    verification.Errors.Add($"LeakCheck '{definition.category}' found {count} occurrence(s).");
                }
            }
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
                bool passed = File.Exists(regression.path)
                    && File.ReadAllText(regression.path).Contains(regression.marker);
                verification.Regressions.Add(regression.name + ": " + (passed ? "PASS" : "FAIL"));
                if (!passed)
                {
                    verification.Errors.Add("Historical regression report is not PASS: " + regression.name + ".");
                }
            }

            verification.FoundationCount = CsvRows("Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv");
            verification.StatCount = CsvRows("Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv");
            verification.CoreCount = CsvRows("Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv");
            verification.AffixCount = CsvRows("Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv");
            verification.RollCount = CsvRows("Docs/V0.4/Reports/ItemInstanceRollEngineSpec.csv");
            if (verification.FoundationCount != 159) verification.Errors.Add("Foundation Spec must remain 159.");
            if (verification.StatCount != 39) verification.Errors.Add("StatRange Spec must remain 39.");
            if (verification.CoreCount != 69) verification.Errors.Add("CorePotential Spec must remain 69.");
            if (verification.AffixCount != 88) verification.Errors.Add("AffixSchema Spec must remain 88.");
            if (verification.RollCount != 72) verification.Errors.Add("RollEngine Spec must remain 72.");
        }

        private static void WriteReports(
            ItemDropGenerationResult sample,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<DeterminismRow> determinism,
            IReadOnlyList<SampleRow> samples,
            IReadOnlyList<LeakRow> leaks,
            Verification verification,
            bool passed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath, BuildReport(sample, specs, determinism, samples, leaks,
                verification, passed), new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(specs), new UTF8Encoding(false));
            File.WriteAllText(DeterminismPath, BuildDeterminism(determinism), new UTF8Encoding(false));
            File.WriteAllText(SamplesPath, BuildSamples(samples), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeak(leaks, passed), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(
            ItemDropGenerationResult sample,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<DeterminismRow> determinism,
            IReadOnlyList<SampleRow> samples,
            IReadOnlyList<LeakRow> leaks,
            Verification verification,
            bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDropGenerationSandbox01 Report").AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Run time (UTC): `{DateTime.UtcNow:O}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(passed ? PassMarker : "ITEM_DROP_GENERATION_SANDBOX01_FAIL")}`")
                .AppendLine($"- Algorithm ID: `{DeterministicItemDropRandom.AlgorithmId}`")
                .AppendLine("- generationVersion: `1`")
                .AppendLine("- Stage 1-10 rarity policy: `LOCKED_STAGE_1_10_WHITE`; no rarity stream is created.")
                .AppendLine("- Ordinary candidate pool: exactly `I001-I030`; QA relative weights only.")
                .AppendLine("- I031 exclusion: explicit failure with `I031_FORBIDDEN`.")
                .AppendLine("- Stage 11+ rarity policy: verifier-memory `QA_WEIGHT_PROFILE`; absent profile is unresolved.")
                .AppendLine("- Candidate domain: `[drop, candidate, candidatePoolId]`.")
                .AppendLine("- Rarity domain: `[drop, rarity, rarityWeightProfileId]`.")
                .AppendLine("- Domain encoding: length-prefixed UTF-8 fields plus segment count; bounded selection uses rejection sampling.")
                .AppendLine($"- Generated instance schema: `{sample?.snapshot?.generatedInstance?.schemaId ?? "None"}`")
                .AppendLine("- Instance generation: calls ItemInstanceRollEngine; any engine failure returns no drop snapshot.")
                .AppendLine($"- QA data status: `{ItemGenerationDataStatus.QaFixtureCanonical}`")
                .AppendLine($"- Spec: {specs.Count}/{specs.Count(row => row.result == "PASS")} PASS")
                .AppendLine($"- Determinism: {determinism.Count}/{determinism.Count(row => row.result == "PASS")} PASS")
                .AppendLine($"- Samples: {samples.Count}")
                .AppendLine($"- Same-input repeat: {(specs.Any(row => row.caseId == "same-input-repeat-1000" && row.result == "PASS") ? "1000/1000 PASS" : "FAIL")}")
                .AppendLine("- Golden candidate domain: `6DEE1D6E50111E1B`.")
                .AppendLine("- Golden rarity domain: `7882C5BB16185E78`.")
                .AppendLine("- Segmented collision vector: `57B30C3DC752992A / 12F4959138EDF7BC`.")
                .AppendLine("- Default formal generation data: empty and unable to generate.")
                .AppendLine("- Formal system connection: none; this package remains an isolated QA sandbox.")
                .AppendLine().AppendLine("## Historical Regressions").AppendLine();
            foreach (string regression in verification.Regressions) builder.AppendLine("- " + regression);
            builder.AppendLine($"- Foundation: {verification.FoundationCount}/159")
                .AppendLine($"- StatRange: {verification.StatCount}/39")
                .AppendLine($"- CorePotential: {verification.CoreCount}/69")
                .AppendLine($"- AffixSchema: {verification.AffixCount}/88")
                .AppendLine($"- RollEngine: {verification.RollCount}/72")
                .AppendLine().AppendLine("## LeakCheck").AppendLine()
                .AppendLine($"- Categories: {leaks.Count}; total leaks: {leaks.Sum(row => row.count)}")
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

        private static string BuildDeterminism(IEnumerable<DeterminismRow> rows)
        {
            StringBuilder builder = new("caseId,rootSeed,stageNumber,domain,expected,actual,repeatCount,result\n");
            foreach (DeterminismRow row in rows)
            {
                builder.Append(Csv(row.caseId)).Append(',')
                    .Append(row.rootSeed.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(row.stageNumber.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.domain)).Append(',').Append(Csv(row.expected)).Append(',')
                    .Append(Csv(row.actual)).Append(',')
                    .Append(row.repeatCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.result)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildSamples(IEnumerable<SampleRow> rows)
        {
            StringBuilder builder = new("category,dropRequestId,stageNumber,rootSeed,selectedBaseItemId,selectedRarity,buildQualification,generatedSchema,result\n");
            foreach (SampleRow row in rows)
            {
                builder.Append(Csv(row.category)).Append(',').Append(Csv(row.dropRequestId)).Append(',')
                    .Append(row.stageNumber).Append(',').Append(row.rootSeed).Append(',')
                    .Append(Csv(row.selectedBaseItemId)).Append(',').Append(Csv(row.selectedRarity)).Append(',')
                    .Append(Csv(row.buildQualification)).Append(',').Append(Csv(row.generatedSchema)).Append(',')
                    .Append(Csv(row.result)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildLeak(IEnumerable<LeakRow> rows, bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDropGenerationSandbox01 LeakCheck Report").AppendLine()
                .AppendLine($"- Runtime scan root: `{RuntimeRoot}`")
                .AppendLine($"- Result: {(passed && rows.All(row => row.count == 0) ? "PASS" : "FAIL")}")
                .AppendLine().AppendLine("| Category | Count |").AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows) builder.AppendLine($"| {row.category} | {row.count} |");
            return builder.ToString();
        }

        private static SampleRow Sample(string category, ItemDropGenerationResult result)
        {
            return new SampleRow(category, result.snapshot.dropRequestId, result.snapshot.stageNumber,
                result.snapshot.rootSeed, result.snapshot.selectedBaseItemId,
                result.snapshot.selectedRarity.ToStableKey(),
                result.snapshot.generatedInstance.buildQualification.ToString(),
                result.snapshot.generatedInstance.schemaId, result.isSuccess ? "PASS" : "FAIL");
        }

        private static void ExpectFailure(List<SpecRow> rows, Verification verification,
            string caseId, string category, ItemDropGenerationResult actual, string expectedCode)
        {
            bool passed = actual != null && !actual.isSuccess && actual.snapshot == null
                && actual.primaryValidationCode == expectedCode;
            rows.Add(new SpecRow(caseId, category, expectedCode,
                actual?.primaryValidationCode ?? "null", actual?.primaryValidationCode ?? "null",
                passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                verification.Errors.Add($"Failure case '{caseId}' expected {expectedCode}, actual {actual?.primaryValidationCode ?? "null"}.");
            }
        }

        private static void CheckBool(List<SpecRow> rows, Verification verification,
            string caseId, string category, bool passed)
        {
            rows.Add(new SpecRow(caseId, category, "true", passed ? "true" : "false",
                ItemDropGenerationValidationCodes.None, passed ? "PASS" : "FAIL"));
            if (!passed) verification.Errors.Add("Boolean case failed: " + caseId + ".");
        }

        private static void Check(List<SpecRow> rows, Verification verification,
            string caseId, string category, string expected, string actual)
        {
            bool passed = string.Equals(expected, actual, StringComparison.Ordinal);
            rows.Add(new SpecRow(caseId, category, expected, actual,
                ItemDropGenerationValidationCodes.None, passed ? "PASS" : "FAIL"));
            if (!passed) verification.Errors.Add($"Case '{caseId}' expected {expected}, actual {actual}.");
        }

        private static bool RejectMutation<T>(IReadOnlyList<T> values)
        {
            if (values == null) return false;
            try { ((IList)values).Clear(); return false; }
            catch (NotSupportedException) { return true; }
            catch (InvalidCastException) { return true; }
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(token)
                && (index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int CsvRows(string path) =>
            File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;

        private static string Hex(ulong value) => value.ToString("X16", CultureInfo.InvariantCulture);
        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static string Compact(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            string oneLine = value.Replace("\r", string.Empty).Replace("\n", " / ");
            return oneLine.Length <= 180 ? oneLine : oneLine.Substring(0, 180) + "...";
        }

        private sealed class Fixture
        {
            public ItemGenerationFoundationSnapshot Foundation { get; private set; }
            public ItemStatRangeSchemaSnapshot Stats { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot Affixes { get; private set; }
            public ItemCorePotentialAndBuildEligibilitySchemaSnapshot Core { get; private set; }
            public ItemDropCandidatePoolSnapshot Candidate30 { get; private set; }
            public ItemDropCandidatePoolSnapshot CandidateOne { get; private set; }
            public ItemDropRarityWeightProfileSnapshot Rarity { get; private set; }
            public IReadOnlyList<ItemBuildQualificationRollProfile> Builds { get; private set; }

            public static Fixture Create()
            {
                Fixture fixture = new();
                fixture.Foundation = ItemRarityInstanceFoundation.Create();
                fixture.Stats = fixture.CreateStats();
                fixture.Affixes = fixture.CreateAffixes();
                fixture.Core = fixture.CreateCore();
                fixture.Candidate30 = fixture.CandidatePool("qa_pool_30",
                    Enumerable.Range(1, 30).Select(index =>
                        new ItemDropCandidateEntrySnapshot($"I{index:000}", index + 1)).ToArray());
                fixture.CandidateOne = fixture.CandidatePool("qa_pool_30",
                    new ItemDropCandidateEntrySnapshot("I001", 1));
                fixture.Rarity = fixture.RarityProfile(
                    new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.White, 1),
                    new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Green, 2),
                    new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Blue, 3),
                    new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Purple, 4),
                    new ItemDropRarityWeightEntrySnapshot(ItemInstanceRarity.Orange, 5));
                fixture.Builds = Array.AsReadOnly(new[]
                {
                    Build(ItemInstanceRarity.Green, ItemBuildQualification.FaMenOnly),
                    Build(ItemInstanceRarity.Blue, ItemBuildQualification.QiLeiOnly),
                    Build(ItemInstanceRarity.Purple, ItemBuildQualification.Dual),
                    Build(ItemInstanceRarity.Orange, ItemBuildQualification.None)
                });
                return fixture;
            }

            public ItemDropGenerationRequest Request(
                int stage = 1,
                long seed = 42L,
                string dropId = "DROP-001",
                string instanceId = "INSTANCE-001",
                ItemDropCandidatePoolSnapshot candidate = null,
                bool forceCandidate = false,
                ItemDropRarityWeightProfileSnapshot rarity = null,
                bool forceRarity = false,
                ItemDropSourceContextSnapshot source = null,
                bool forceSource = false,
                ItemGenerationFoundationSnapshot foundation = null,
                bool forceFoundation = false,
                IEnumerable<ItemBuildQualificationRollProfile> builds = null,
                bool forceBuilds = false,
                string status = null,
                int version = 1)
            {
                ItemDropCandidatePoolSnapshot resolvedCandidate = forceCandidate ? candidate
                    : candidate ?? (stage > 10 ? CandidateOne : Candidate30);
                ItemDropRarityWeightProfileSnapshot resolvedRarity = forceRarity ? rarity
                    : rarity ?? (stage > 10 ? Rarity : null);
                ItemDropSourceContextSnapshot resolvedSource = forceSource ? source
                    : source ?? Source(stage, candidatePoolId: resolvedCandidate?.poolId ?? "qa_pool_30",
                        rarityProfileId: stage > 10 ? resolvedRarity?.profileId ?? "qa_rarity_11_plus" : string.Empty);
                return new ItemDropGenerationRequest(dropId, instanceId, seed, version,
                    status ?? ItemGenerationDataStatus.QaFixtureCanonical,
                    forceFoundation ? foundation : foundation ?? Foundation,
                    resolvedSource, resolvedCandidate, resolvedRarity, Stats, Affixes, Core,
                    forceBuilds ? builds : builds ?? Builds);
            }

            public ItemDropSourceContextSnapshot Source(
                int stage,
                string contextId = "CTX-001",
                string sourceKey = "qa_stage_clear",
                string candidatePoolId = "qa_pool_30",
                string rarityProfileId = null,
                string maturity = ItemGenerationDataStatus.QaFixtureOnly)
            {
                return new ItemDropSourceContextSnapshot(contextId, sourceKey, stage, candidatePoolId,
                    rarityProfileId ?? (stage > 10 ? "qa_rarity_11_plus" : string.Empty), maturity);
            }

            public ItemDropCandidatePoolSnapshot CandidatePool(string id,
                params ItemDropCandidateEntrySnapshot[] entries)
            {
                return new ItemDropCandidatePoolSnapshot(id, ItemGenerationDataStatus.QaFixtureOnly,
                    entries ?? Array.Empty<ItemDropCandidateEntrySnapshot>());
            }

            public ItemDropRarityWeightProfileSnapshot RarityProfile(
                params ItemDropRarityWeightEntrySnapshot[] entries)
            {
                return new ItemDropRarityWeightProfileSnapshot("qa_rarity_11_plus",
                    ItemGenerationDataStatus.QaFixtureOnly,
                    entries ?? Array.Empty<ItemDropRarityWeightEntrySnapshot>());
            }

            private ItemStatRangeSchemaSnapshot CreateStats()
            {
                ItemStatDefinitionSnapshot definition = new("qa_drop_stat", "QA Drop Stat", "flat",
                    ItemStatDirection.HigherIsBetter, 0, 1, ItemStatRoundingMode.Nearest,
                    ItemStatDataMaturity.SEED_DATA);
                ItemRarityStatRangeSnapshot[] ranges = ItemInstanceRarityCatalog.All
                    .Select((rarity, index) => new ItemRarityStatRangeSnapshot(rarity.rarity,
                        index + 1, index + 1)).ToArray();
                ItemStatRangeProfileSnapshot[] profiles = Enumerable.Range(1, 30)
                    .Select(index => new ItemStatRangeProfileSnapshot($"I{index:000}", definition.statId,
                        ItemStatDataMaturity.SEED_DATA, new ItemNumericRangeSnapshot(1, 5), ranges))
                    .ToArray();
                return ItemStatRangeSchema.Create(new[] { definition }, profiles);
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateAffixes()
            {
                ItemAffixSlotPolicySnapshot slots = new("qa_drop_empty_slots",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    Array.Empty<ItemAffixSlotSnapshot>());
                ItemAffixGenerationProfileSnapshot[] profiles = Enumerable.Range(1, 30)
                    .Select(index => new ItemAffixGenerationProfileSnapshot($"I{index:000}",
                        ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                        slots.slotPolicyId, Array.Empty<ItemFixedAffixBindingSnapshot>(), string.Empty))
                    .ToArray();
                return ItemAffixPoolAndRangeSchema.Create(Array.Empty<ItemAffixDefinitionSnapshot>(),
                    Array.Empty<ItemAffixValueProfileSnapshot>(), new[] { slots },
                    Array.Empty<ItemRandomAffixPoolSnapshot>(), profiles);
            }

            private ItemCorePotentialAndBuildEligibilitySchemaSnapshot CreateCore()
            {
                List<ItemCorePotentialProfileSnapshot> profiles = new();
                for (int index = 1; index <= 30; index++)
                {
                    profiles.Add(CoreProfile($"I{index:000}", ItemInstanceRarity.White));
                }
                foreach (ItemInstanceRarity rarity in new[]
                {
                    ItemInstanceRarity.Green, ItemInstanceRarity.Blue,
                    ItemInstanceRarity.Purple, ItemInstanceRarity.Orange
                })
                {
                    profiles.Add(CoreProfile("I001", rarity));
                }
                return ItemCorePotentialAndBuildEligibilitySchema.Create(
                    ItemCorePotentialAndBuildEligibilityCatalog.DefaultCoreRarityPolicies,
                    profiles,
                    ItemCorePotentialAndBuildEligibilityCatalog.DefaultBuildRarityPolicies);
            }

            private static ItemCorePotentialProfileSnapshot CoreProfile(string baseItemId,
                ItemInstanceRarity rarity)
            {
                return new ItemCorePotentialProfileSnapshot(
                    "qa_core_" + baseItemId.ToLowerInvariant() + "_" + rarity.ToStableKey(),
                    baseItemId, rarity, ItemCorePotentialResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    Array.Empty<ItemCorePotentialEffectSnapshot>());
            }

            private static ItemBuildQualificationRollProfile Build(ItemInstanceRarity rarity,
                ItemBuildQualification qualification)
            {
                return new ItemBuildQualificationRollProfile("qa_drop_build_" + rarity.ToStableKey(),
                    rarity, ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemBuildQualificationRollEntry(qualification, 1) });
            }
        }

        private sealed class Verification
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public int FoundationCount, StatCount, CoreCount, AffixCount, RollCount;
        }

        private readonly struct SpecRow
        {
            public SpecRow(string caseId, string category, string expected, string actual,
                string code, string result)
            {
                this.caseId = caseId; this.category = category; this.expected = expected;
                this.actual = actual; this.code = code; this.result = result;
            }
            public readonly string caseId, category, expected, actual, code, result;
        }

        private readonly struct DeterminismRow
        {
            public DeterminismRow(string caseId, long rootSeed, int stageNumber, string domain,
                string expected, string actual, int repeatCount, string result)
            {
                this.caseId = caseId; this.rootSeed = rootSeed; this.stageNumber = stageNumber;
                this.domain = domain; this.expected = expected; this.actual = actual;
                this.repeatCount = repeatCount; this.result = result;
            }
            public readonly string caseId, domain, expected, actual, result;
            public readonly long rootSeed;
            public readonly int stageNumber, repeatCount;
        }

        private readonly struct SampleRow
        {
            public SampleRow(string category, string dropRequestId, int stageNumber, long rootSeed,
                string selectedBaseItemId, string selectedRarity, string buildQualification,
                string generatedSchema, string result)
            {
                this.category = category; this.dropRequestId = dropRequestId;
                this.stageNumber = stageNumber; this.rootSeed = rootSeed;
                this.selectedBaseItemId = selectedBaseItemId; this.selectedRarity = selectedRarity;
                this.buildQualification = buildQualification; this.generatedSchema = generatedSchema;
                this.result = result;
            }
            public readonly string category, dropRequestId, selectedBaseItemId, selectedRarity,
                buildQualification, generatedSchema, result;
            public readonly int stageNumber;
            public readonly long rootSeed;
        }

        private readonly struct LeakRow
        {
            public LeakRow(string category, int count) { this.category = category; this.count = count; }
            public readonly string category;
            public readonly int count;
        }

        private readonly struct LeakDefinition
        {
            public LeakDefinition(string category, string token) { this.category = category; this.token = token; }
            public readonly string category, token;
        }

        private readonly struct Regression
        {
            public Regression(string name, string path, string marker)
            { this.name = name; this.path = path; this.marker = marker; }
            public readonly string name, path, marker;
        }
    }
}
#endif
