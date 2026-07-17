#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.DropSandbox;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemGenerationSimulationValidator
    {
        private const string PackageName = "V0.4-ItemGenerationSimulationValidator01-GuardFix01";
        private const string SimulationSchemaId = "ItemGenerationSimulationResult.v1";
        private const string SimulationSuiteSchemaId = "ItemGenerationSimulationSuiteResult.v1";
        private const string RequestSchemaId = "ItemGenerationSimulationRequest.v1";
        private const string SimulationVersion = "1";
        private const string ThresholdMarker = "QA_DISTRIBUTION_TEST_THRESHOLD_ONLY";
        private const string PassMarker = "ITEM_GENERATION_SIMULATION_VALIDATOR01_GUARDFIX01_PASS";
        private const string FailMarker = "ITEM_GENERATION_SIMULATION_VALIDATOR01_GUARDFIX01_FAIL";
        private const string ReportPath = "Docs/V0.4/Reports/ItemGenerationSimulationValidatorReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemGenerationSimulationValidatorSpec.csv";
        private const string DistributionPath = "Docs/V0.4/Reports/ItemGenerationSimulationDistribution.csv";
        private const string ViolationPath = "Docs/V0.4/Reports/ItemGenerationSimulationInvariantViolations.csv";
        private const string DeterminismPath = "Docs/V0.4/Reports/ItemGenerationSimulationDeterminism.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemGenerationSimulationLeakCheckReport.md";
        private const string SourcePath = "Assets/_Game/Scripts/TalismanBag/Editor/ItemGeneration/ItemGenerationSimulationValidator.cs";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemGenerationSimulationValidator01/[QA Only] Run Simulation")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(Application.isBatchMode);
        }

        public static void VerifyFullHistoricalBatch()
        {
            ItemRarityInstanceFoundationVerifier.VerifyMenu();
            ItemStatRangeSchemaVerifier.VerifyMenu();
            ItemCorePotentialAndBuildEligibilitySchemaVerifier.VerifyMenu();
            ItemAffixPoolAndRangeSchemaVerifier.VerifyMenu();
            ItemInstanceRollEngineVerifier.VerifyMenu();
            ItemDropGenerationSandboxVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.ItemInnerDataCatalogVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.ItemSystemValidatorAndSnapshotVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.BuildSynergyCoreVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.CoreAwakeningPreviewVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.ItemSkillTriggerContractVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.ItemDetailProjectionCompleteVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.JuNianLightingAndAdjacentRelayVerifier.VerifyMenu();
            TalismanBag.EditorTools.ItemSandbox.ArrayBonusCellResolverVerifier.VerifyMenu();
            VerifyAndWriteReports(Application.isBatchMode);
        }

        public sealed class SimulationRequest
        {
            public SimulationRequest(string schemaId, string simulationVersion, string fixtureStatus,
                string scenarioId, int sampleCount, ulong firstRootSeed)
            {
                this.schemaId = schemaId ?? string.Empty;
                this.simulationVersion = simulationVersion ?? string.Empty;
                this.fixtureStatus = fixtureStatus ?? string.Empty;
                this.scenarioId = scenarioId ?? string.Empty;
                this.sampleCount = sampleCount;
                this.firstRootSeed = firstRootSeed;
            }

            public string schemaId { get; }
            public string simulationVersion { get; }
            public string fixtureStatus { get; }
            public string scenarioId { get; }
            public int sampleCount { get; }
            public ulong firstRootSeed { get; }
        }

        public sealed class SimulationDistributionRow
        {
            public SimulationDistributionRow(string scenarioId, string dimensionKey, string bucketKey,
                long weight, int observedCount, double observedRatio, double expectedCount,
                double expectedRatio, double absoluteDelta, double zScore, bool isPass)
            {
                this.scenarioId = scenarioId ?? string.Empty;
                this.dimensionKey = dimensionKey ?? string.Empty;
                this.bucketKey = bucketKey ?? string.Empty;
                this.weight = weight;
                this.observedCount = observedCount;
                this.observedRatio = observedRatio;
                this.expectedCount = expectedCount;
                this.expectedRatio = expectedRatio;
                this.absoluteDelta = absoluteDelta;
                this.zScore = zScore;
                this.isPass = isPass;
            }

            public string scenarioId { get; }
            public string dimensionKey { get; }
            public string bucketKey { get; }
            public long weight { get; }
            public int observedCount { get; }
            public double observedRatio { get; }
            public double expectedCount { get; }
            public double expectedRatio { get; }
            public double absoluteDelta { get; }
            public double zScore { get; }
            public bool isPass { get; }
        }

        public sealed class SimulationInvariantViolation
        {
            public SimulationInvariantViolation(string scenarioId, int sampleIndex, ulong rootSeed,
                string dropRequestId, string itemInstanceId, string baseItemId, string rarity,
                string validationCode, string message)
            {
                this.scenarioId = scenarioId ?? string.Empty;
                this.sampleIndex = sampleIndex;
                this.rootSeed = rootSeed;
                this.dropRequestId = dropRequestId ?? string.Empty;
                this.itemInstanceId = itemInstanceId ?? string.Empty;
                this.baseItemId = baseItemId ?? string.Empty;
                this.rarity = rarity ?? string.Empty;
                this.validationCode = validationCode ?? string.Empty;
                this.message = message ?? string.Empty;
            }

            public string scenarioId { get; }
            public int sampleIndex { get; }
            public ulong rootSeed { get; }
            public string dropRequestId { get; }
            public string itemInstanceId { get; }
            public string baseItemId { get; }
            public string rarity { get; }
            public string validationCode { get; }
            public string message { get; }
        }

        public sealed class SimulationExpectedFailureRow
        {
            public SimulationExpectedFailureRow(string scenarioId, string expectedCode,
                string actualCode, bool snapshotIsNull, bool isPass)
            {
                this.scenarioId = scenarioId ?? string.Empty;
                this.expectedCode = expectedCode ?? string.Empty;
                this.actualCode = actualCode ?? string.Empty;
                this.snapshotIsNull = snapshotIsNull;
                this.isPass = isPass;
            }

            public string scenarioId { get; }
            public string expectedCode { get; }
            public string actualCode { get; }
            public bool snapshotIsNull { get; }
            public bool isPass { get; }
        }

        public sealed class SimulationResult
        {
            private readonly ReadOnlyCollection<SimulationDistributionRow> distributionRowValues;
            private readonly ReadOnlyCollection<SimulationInvariantViolation> invariantViolationValues;
            private readonly ReadOnlyCollection<SimulationExpectedFailureRow> expectedFailureRowValues;

            internal SimulationResult(string scenarioId, int sampleCount, int successCount, int failureCount,
                ulong firstRootSeed, IEnumerable<SimulationDistributionRow> distributionRows,
                IEnumerable<SimulationInvariantViolation> invariantViolations,
                IEnumerable<SimulationExpectedFailureRow> expectedFailureRows, string canonicalSignature)
            {
                schemaId = SimulationSchemaId;
                simulationVersion = SimulationVersion;
                fixtureStatus = ItemGenerationDataStatus.QaFixtureCanonical;
                this.scenarioId = scenarioId ?? string.Empty;
                this.sampleCount = sampleCount;
                this.successCount = successCount;
                this.failureCount = failureCount;
                this.firstRootSeed = firstRootSeed;
                generationVersion = DeterministicItemRandom.SupportedGenerationVersion;
                distributionRowValues = Freeze(distributionRows);
                invariantViolationValues = Freeze(invariantViolations);
                expectedFailureRowValues = Freeze(expectedFailureRows);
                this.canonicalSignature = canonicalSignature ?? string.Empty;
            }

            public string schemaId { get; }
            public string simulationVersion { get; }
            public string fixtureStatus { get; }
            public string scenarioId { get; }
            public int sampleCount { get; }
            public int successCount { get; }
            public int failureCount { get; }
            public ulong firstRootSeed { get; }
            public int generationVersion { get; }
            public IReadOnlyList<SimulationDistributionRow> distributionRows => distributionRowValues;
            public IReadOnlyList<SimulationInvariantViolation> invariantViolations => invariantViolationValues;
            public IReadOnlyList<SimulationExpectedFailureRow> expectedFailureRows => expectedFailureRowValues;
            public string canonicalSignature { get; }

            private static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> rows) =>
                Array.AsReadOnly((rows ?? Array.Empty<T>()).ToArray());
        }

        public sealed class SimulationSuiteResult
        {
            private readonly ReadOnlyCollection<SimulationResult> scenarioResultValues;
            private readonly ReadOnlyCollection<SimulationExpectedFailureRow> expectedFailureRowValues;

            internal SimulationSuiteResult(IEnumerable<SimulationResult> scenarioResults,
                IEnumerable<SimulationExpectedFailureRow> expectedFailureRows, string canonicalSignature)
            {
                schemaId = SimulationSuiteSchemaId;
                simulationVersion = SimulationVersion;
                fixtureStatus = ItemGenerationDataStatus.QaFixtureCanonical;
                scenarioResultValues = Array.AsReadOnly((scenarioResults ?? Array.Empty<SimulationResult>()).ToArray());
                expectedFailureRowValues = Array.AsReadOnly((expectedFailureRows
                    ?? Array.Empty<SimulationExpectedFailureRow>()).ToArray());
                this.canonicalSignature = canonicalSignature ?? string.Empty;
            }

            public string schemaId { get; }
            public string simulationVersion { get; }
            public string fixtureStatus { get; }
            public IReadOnlyList<SimulationResult> scenarioResults => scenarioResultValues;
            public IReadOnlyList<SimulationExpectedFailureRow> expectedFailureRows => expectedFailureRowValues;
            public string canonicalSignature { get; }
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            Verification verification = new();
            List<SpecRow> specs = new();
            List<DeterminismRow> determinism = new();
            List<LeakRow> leaks = new();
            List<ScenarioRun> runs = new();
            List<SimulationExpectedFailureRow> expectedFailures = new();
            SimulationSuiteResult suite = null;
            try
            {
                Fixture fixture = Fixture.Create();
                RunInputContractSpecs(fixture, specs, expectedFailures, verification);
                RunLargeSampleScenarios(fixture, runs, specs, determinism, verification);
                RunPolicyAndFailureSpecs(fixture, specs, expectedFailures, verification);
                suite = new SimulationSuiteResult(runs.Select(run => run.Result), expectedFailures,
                    BuildAggregateSignature(runs, expectedFailures));
                RunReadOnlySpecs(runs, suite, specs, verification);
                RunHistoricalRegressions(verification);
                RunLeakChecks(leaks, verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add("Unhandled verifier exception: " + exception);
            }

            List<SimulationDistributionRow> distributions = runs.SelectMany(run => run.Result.distributionRows)
                .OrderBy(row => row.scenarioId, StringComparer.Ordinal)
                .ThenBy(row => row.dimensionKey, StringComparer.Ordinal)
                .ThenBy(row => row.bucketKey, StringComparer.Ordinal)
                .ToList();
            List<SimulationInvariantViolation> violations = runs.SelectMany(run => run.Result.invariantViolations)
                .OrderBy(row => row.scenarioId, StringComparer.Ordinal)
                .ThenBy(row => row.sampleIndex)
                .ThenBy(row => row.validationCode, StringComparer.Ordinal)
                .ToList();
            string canonicalSignature = BuildAggregateSignature(runs, expectedFailures);
            suite = new SimulationSuiteResult(runs.Select(run => run.Result), expectedFailures,
                canonicalSignature);
            bool passed = verification.Errors.Count == 0
                && specs.Count >= 35
                && specs.All(row => row.result == "PASS")
                && determinism.All(row => row.result == "PASS")
                && distributions.All(row => row.isPass)
                && violations.Count == 0
                && expectedFailures.Count == 11 && expectedFailures.All(row => row.isPass)
                && leaks.All(row => row.count == 0);

            WriteReports(runs, suite, specs, distributions, violations, expectedFailures, determinism,
                leaks, verification, canonicalSignature, passed);
            if (passed)
            {
                Debug.Log(PassMarker + $" specs={specs.Count} scenarios={runs.Count} "
                    + $"samples={verification.ExecutedSampleCount} violations=0");
            }
            else
            {
                Debug.LogError(FailMarker + "\n" + string.Join("\n", verification.Errors));
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void RunInputContractSpecs(Fixture fixture, List<SpecRow> specs,
            List<SimulationExpectedFailureRow> expectedFailures, Verification verification)
        {
            CheckValidation(specs, verification, "simulation-request-null", null, "SIMULATION_REQUEST_NULL");
            CheckValidation(specs, verification, "simulation-schema-missing",
                new SimulationRequest(string.Empty, SimulationVersion, ItemGenerationDataStatus.QaFixtureCanonical,
                    "qa", 1, 0UL), "SIMULATION_SCHEMA_INVALID");
            CheckValidation(specs, verification, "simulation-version-invalid",
                new SimulationRequest(RequestSchemaId, "2", ItemGenerationDataStatus.QaFixtureCanonical,
                    "qa", 1, 0UL), "SIMULATION_VERSION_UNSUPPORTED");
            CheckValidation(specs, verification, "simulation-sample-count-invalid",
                new SimulationRequest(RequestSchemaId, SimulationVersion, ItemGenerationDataStatus.QaFixtureCanonical,
                    "qa", 0, 0UL), "SIMULATION_SAMPLE_COUNT_INVALID");
            CheckValidation(specs, verification, "simulation-fixture-status-invalid",
                new SimulationRequest(RequestSchemaId, SimulationVersion, "FORMAL", "qa", 1, 0UL),
                "SIMULATION_FIXTURE_STATUS_INVALID");

            ExpectFailure(specs, expectedFailures, verification, "drop-request-null",
                ItemDropGenerationSandbox.Generate(null), ItemDropGenerationValidationCodes.RequestNull);
            ItemDropGenerationRequest versionInvalid = fixture.Request("input-version-invalid", 0, 1UL,
                InputVariant.None, generationVersion: 2);
            ExpectFailure(specs, expectedFailures, verification, "drop-generation-version-invalid",
                ItemDropGenerationSandbox.Generate(versionInvalid),
                ItemDropGenerationValidationCodes.GenerationVersionUnsupported);
            ItemDropGenerationRequest statusInvalid = fixture.Request("input-status-invalid", 0, 1UL,
                InputVariant.None, status: ItemGenerationDataStatus.QaFixtureOnly);
            ExpectFailure(specs, expectedFailures, verification, "drop-fixture-status-invalid",
                ItemDropGenerationSandbox.Generate(statusInvalid),
                ItemDropGenerationValidationCodes.GenerationDataStatusInvalid);
        }

        private static string ValidateSimulationRequest(SimulationRequest request)
        {
            if (request == null) return "SIMULATION_REQUEST_NULL";
            if (!string.Equals(request.schemaId, RequestSchemaId, StringComparison.Ordinal))
                return "SIMULATION_SCHEMA_INVALID";
            if (!string.Equals(request.simulationVersion, SimulationVersion, StringComparison.Ordinal))
                return "SIMULATION_VERSION_UNSUPPORTED";
            if (request.sampleCount <= 0) return "SIMULATION_SAMPLE_COUNT_INVALID";
            if (!string.Equals(request.fixtureStatus, ItemGenerationDataStatus.QaFixtureCanonical,
                StringComparison.Ordinal)) return "SIMULATION_FIXTURE_STATUS_INVALID";
            if (string.IsNullOrWhiteSpace(request.scenarioId)) return "SIMULATION_SCENARIO_ID_EMPTY";
            return "NONE";
        }

        private static void RunLargeSampleScenarios(Fixture fixture, List<ScenarioRun> runs,
            List<SpecRow> specs, List<DeterminismRow> determinism, Verification verification)
        {
            ScenarioRun early = RunScenario(fixture, "A_STAGE_1_10_WHITE_LOCK", 10000, 0UL,
                ScenarioKind.EarlyWhite, InputVariant.None);
            AddRun(runs, early, specs, verification, "early-stage-white-lock");
            CheckBool(specs, verification, "white-build-none", "build",
                early.Result.successCount == 10000 && early.Result.invariantViolations.Count == 0);
            CheckBool(specs, verification, "early-stage-i031-leak-zero", "i031",
                early.Accumulator.I031LeakCount == 0);

            ScenarioRun candidate = RunScenario(fixture, "B_CANDIDATE_EQUAL_30", 30000, 100000UL,
                ScenarioKind.CandidateEqual, InputVariant.None);
            AddRun(runs, candidate, specs, verification, "candidate-equal-distribution");
            CheckBool(specs, verification, "candidate-all-30-covered", "candidate",
                candidate.Accumulator.ObservedBaseIds.Count == 30
                && Enumerable.Range(1, 30).All(index => candidate.Accumulator.ObservedBaseIds.Contains($"I{index:000}")));
            CheckBool(specs, verification, "candidate-no-unknown-id", "candidate",
                candidate.Accumulator.ObservedBaseIds.All(ItemRarityInstanceFoundation.IsOrdinaryBaseItemId));
            CheckBool(specs, verification, "candidate-i031-leak-zero", "i031",
                candidate.Accumulator.I031LeakCount == 0);

            ScenarioRun candidateReplay = RunScenario(fixture, "B_CANDIDATE_EQUAL_30", 30000, 100000UL,
                ScenarioKind.CandidateEqual, InputVariant.None);
            CheckDeterminism(determinism, verification, "same-complete-input-replay", candidate, candidateReplay,
                "same complete simulation input", 30000);

            ScenarioRun candidateReversed = RunScenario(fixture, "B_CANDIDATE_EQUAL_30", 30000, 100000UL,
                ScenarioKind.CandidateEqual, InputVariant.Candidate);
            CheckDeterminism(determinism, verification, "candidate-entry-reversal", candidate, candidateReversed,
                "candidate entries reversed", 30000);

            ScenarioRun weightedCandidate = RunScenario(fixture, "B_CANDIDATE_WEIGHTED_QA", 30000, 200000UL,
                ScenarioKind.CandidateWeighted, InputVariant.None);
            AddRun(runs, weightedCandidate, specs, verification, "candidate-weighted-distribution");
            CheckBool(specs, verification, "candidate-weight-order-observed", "candidate",
                weightedCandidate.Accumulator.BucketCount("candidate", "I003")
                > weightedCandidate.Accumulator.BucketCount("candidate", "I002")
                && weightedCandidate.Accumulator.BucketCount("candidate", "I002")
                > weightedCandidate.Accumulator.BucketCount("candidate", "I001"));

            ScenarioRun rarity = RunScenario(fixture, "C_STAGE_11_RARITY_AND_RICH_ROLL", 30000, 300000UL,
                ScenarioKind.RarityRich, InputVariant.None);
            AddRun(runs, rarity, specs, verification, "rarity-and-rich-roll-distribution");
            CheckBool(specs, verification, "range-endpoints-covered", "range",
                rarity.Accumulator.RangeMinimumObserved && rarity.Accumulator.RangeMaximumObserved);
            CheckBool(specs, verification, "stat-affix-minimum-endpoint", "range",
                rarity.Accumulator.RangeMinimumObserved);
            CheckBool(specs, verification, "stat-affix-maximum-endpoint", "range",
                rarity.Accumulator.RangeMaximumObserved);
            CheckBool(specs, verification, "stat-affix-step-alignment", "range",
                rarity.Accumulator.RangeChecks > 0
                && !rarity.Result.invariantViolations.Any(row => row.validationCode.Contains("RANGE", StringComparison.Ordinal)));
            CheckBool(specs, verification, "fixed-affix-value-varies", "affix",
                rarity.Accumulator.FixedAffixValues.Count > 1);
            CheckBool(specs, verification, "random-affix-weight-distribution", "affix",
                rarity.Result.distributionRows.Where(row => row.dimensionKey == "randomAffix").All(row => row.isPass));
            CheckBool(specs, verification, "zero-weight-bucket-observed-zero", "affix",
                rarity.Result.distributionRows.Any(row => row.dimensionKey == "randomAffix"
                    && row.bucketKey == Fixture.ZeroWeightAffix && row.weight == 0 && row.observedCount == 0));
            CheckBool(specs, verification, "core-build-boundaries", "core-build",
                rarity.Accumulator.CoreChecks > 0 && rarity.Accumulator.BuildChecks > 0);
            CheckBool(specs, verification, "higher-rarity-build-profile-match", "build",
                rarity.Accumulator.BuildChecks == rarity.Result.successCount);
            CheckBool(specs, verification, "core-eligible-visible-boundary", "core",
                rarity.Accumulator.CoreChecks == rarity.Result.successCount);
            CheckBool(specs, verification, "ultimate-orange-only", "core",
                !rarity.Result.invariantViolations.Any(row => row.validationCode == "CORE_POTENTIAL_INVALID"));

            ScenarioRun rarityReversed = RunScenario(fixture, "C_STAGE_11_RARITY_AND_RICH_ROLL", 30000,
                300000UL, ScenarioKind.RarityRich, InputVariant.Rarity);
            CheckDeterminism(determinism, verification, "rarity-entry-reversal", rarity, rarityReversed,
                "rarity entries reversed", 30000);

            ScenarioRun buildBase = RunScenario(fixture, "L_BUILD_ORDER_STABILITY", 4096, 400000UL,
                ScenarioKind.RarityRich, InputVariant.None);
            ScenarioRun buildReversed = RunScenario(fixture, "L_BUILD_ORDER_STABILITY", 4096, 400000UL,
                ScenarioKind.RarityRich, InputVariant.Build);
            CheckDeterminism(determinism, verification, "build-entry-reversal", buildBase, buildReversed,
                "Build profile entries reversed", 4096);

            ScenarioRun affixBase = RunScenario(fixture, "L_AFFIX_ORDER_STABILITY", 4096, 410000UL,
                ScenarioKind.RarityRich, InputVariant.None);
            ScenarioRun affixReversed = RunScenario(fixture, "L_AFFIX_ORDER_STABILITY", 4096, 410000UL,
                ScenarioKind.RarityRich, InputVariant.Affix);
            CheckDeterminism(determinism, verification, "affix-entry-reversal", affixBase, affixReversed,
                "affix pool entries reversed", 4096);

            ScenarioRun mutex = RunScenario(fixture, "G_MUTEX_RULES", 5000, 500000UL,
                ScenarioKind.Mutex, InputVariant.None);
            AddRun(runs, mutex, specs, verification, "mutex-rules");
            CheckBool(specs, verification, "mutex-both-branches-live", "mutex",
                mutex.Accumulator.MutexAffixASeen && mutex.Accumulator.MutexAffixBSeen);
            CheckBool(specs, verification, "duplicate-disallowed", "repeat",
                mutex.Accumulator.RepeatChecks == mutex.Result.successCount
                && !mutex.Result.invariantViolations.Any(row => row.validationCode == "AFFIX_DUPLICATE_FORBIDDEN"));

            ScenarioRun duplicate = RunScenario(fixture, "H_ALLOW_DUPLICATE", 512, 600000UL,
                ScenarioKind.AllowDuplicate, InputVariant.None);
            AddRun(runs, duplicate, specs, verification, "allow-duplicate");
            CheckBool(specs, verification, "duplicate-observed", "repeat",
                duplicate.Accumulator.LegalDuplicateCount > 0);
            CheckBool(specs, verification, "duplicate-independent-values", "repeat",
                duplicate.Accumulator.DuplicateDifferentValueCount > 0);

            RunExactBuildProfileScenarios(fixture, runs, specs, verification);
            IReadOnlyDictionary<string, EndpointBucket> endpointBuckets = MergeEndpointBuckets(runs);
            IReadOnlyDictionary<string, HashSet<long>> fixedValueBuckets = MergeFixedValueBuckets(runs);
            IReadOnlyDictionary<string, Dictionary<long, ulong>> fixedValueSeedBuckets =
                MergeFixedValueSeedBuckets(runs);
            verification.EndpointBucketCount = endpointBuckets.Count;
            verification.EndpointCoveredCount = endpointBuckets.Values.Count(bucket => bucket.isCovered);
            verification.EndpointMissingCount = endpointBuckets.Count - verification.EndpointCoveredCount;
            CheckBool(specs, verification, "endpoint-buckets-present", "range-bucket",
                endpointBuckets.Count > 0);
            CheckBool(specs, verification, "endpoint-buckets-all-minimum-covered", "range-bucket",
                endpointBuckets.Values.All(bucket => bucket.minObserved));
            CheckBool(specs, verification, "endpoint-buckets-all-maximum-covered", "range-bucket",
                endpointBuckets.Values.All(bucket => bucket.maxObserved));
            CheckBool(specs, verification, "endpoint-buckets-step-violation-zero", "range-bucket",
                endpointBuckets.Values.All(bucket => bucket.stepViolationCount == 0));
            CheckBool(specs, verification, "fixed-affix-same-profile-rarity-multiple-values", "affix-bucket",
                fixedValueBuckets.Count > 0 && fixedValueBuckets.Values.All(values => values.Count >= 2)
                && fixedValueSeedBuckets.Values.All(valueSeeds =>
                    valueSeeds.Count >= 2 && valueSeeds.Values.Distinct().Count() >= 2));

            RunExtremeAndBatchDeterminism(fixture, candidate, determinism, verification);
            verification.ExecutedSampleCount = runs.Sum(run => run.Result.sampleCount)
                + candidateReplay.Result.sampleCount + candidateReversed.Result.sampleCount
                + rarityReversed.Result.sampleCount + buildBase.Result.sampleCount + buildReversed.Result.sampleCount
                + affixBase.Result.sampleCount + affixReversed.Result.sampleCount + 30000 + 25;
        }

        private static void RunExactBuildProfileScenarios(Fixture fixture, List<ScenarioRun> runs,
            List<SpecRow> specs, Verification verification)
        {
            (ScenarioKind kind, ItemInstanceRarity rarity, string scenarioId, ulong seed)[] cases =
            {
                (ScenarioKind.BuildExactGreen, ItemInstanceRarity.Green, "M_BUILD_EXACT_GREEN", 700000UL),
                (ScenarioKind.BuildExactBlue, ItemInstanceRarity.Blue, "M_BUILD_EXACT_BLUE", 701000UL),
                (ScenarioKind.BuildExactPurple, ItemInstanceRarity.Purple, "M_BUILD_EXACT_PURPLE", 702000UL),
                (ScenarioKind.BuildExactOrange, ItemInstanceRarity.Orange, "M_BUILD_EXACT_ORANGE", 703000UL)
            };
            foreach ((ScenarioKind kind, ItemInstanceRarity rarity, string scenarioId, ulong seed) item in cases)
            {
                ScenarioRun run = RunScenario(fixture, item.scenarioId, 512, item.seed, item.kind,
                    InputVariant.None);
                AddRun(runs, run, specs, verification,
                    "build-exact-" + item.rarity.ToStableKey() + "-profile");
                verification.BuildExactResults.Add(new BuildExactResult(item.rarity.ToStableKey(),
                    fixture.ExactBuildProfileId(item.rarity), fixture.ExactBuildQualification(item.rarity),
                    run.Accumulator.BuildExactMatchCount, run.Accumulator.BuildExactMismatchCount));
            }

            ItemDropGenerationRequest swappedRequest = fixture.Request("build-swapped", 0, 704000UL,
                InputVariant.None, ScenarioKind.BuildExactGreen,
                buildOverride: fixture.SwappedExactBuildProfiles());
            ItemDropGenerationResult swappedResult = ItemDropGenerationSandbox.Generate(swappedRequest);
            CheckBool(specs, verification, "build-profile-swapped-rejected", "build-exact",
                !IsExactBuildMatch(fixture, swappedRequest, swappedResult, ItemInstanceRarity.Green));

            ItemDropGenerationRequest mismatchedRequest = fixture.Request("build-mismatched", 0, 704001UL,
                InputVariant.None, ScenarioKind.BuildExactGreen,
                buildOverride: new[] { fixture.MislabelledExactBuildProfile(ItemInstanceRarity.Green,
                    ItemInstanceRarity.Blue) });
            ItemDropGenerationResult mismatchedResult = ItemDropGenerationSandbox.Generate(mismatchedRequest);
            CheckBool(specs, verification, "build-profile-mismatched-rejected", "build-exact",
                !IsExactBuildMatch(fixture, mismatchedRequest, mismatchedResult, ItemInstanceRarity.Green));

            ItemDropGenerationRequest missingRequest = fixture.Request("build-corresponding-missing", 0,
                704002UL, InputVariant.None, ScenarioKind.BuildExactGreen,
                buildOverride: fixture.ExactBuildProfiles().Where(profile =>
                    profile.rarity != ItemInstanceRarity.Green));
            ItemDropGenerationResult missingResult = ItemDropGenerationSandbox.Generate(missingRequest);
            CheckBool(specs, verification, "build-profile-corresponding-missing-rejected", "build-exact",
                missingResult != null && !missingResult.isSuccess && missingResult.snapshot == null
                && missingResult.primaryValidationCode == ItemDropGenerationValidationCodes.BuildRollProfileMissing);
        }

        private static bool IsExactBuildMatch(Fixture fixture, ItemDropGenerationRequest request,
            ItemDropGenerationResult generated, ItemInstanceRarity rarity)
        {
            return generated != null && generated.isSuccess
                && IsExactBuildMatch(fixture, request, generated.snapshot, rarity);
        }

        private static bool IsExactBuildMatch(Fixture fixture, ItemDropGenerationRequest request,
            ItemDropGenerationSnapshot snapshot, ItemInstanceRarity rarity)
        {
            ItemBuildQualificationRollProfile[] matches = request?.BuildQualificationRollProfiles
                .Where(profile => profile != null && profile.rarity == rarity).ToArray()
                ?? Array.Empty<ItemBuildQualificationRollProfile>();
            return snapshot?.generatedInstance != null && snapshot.selectedRarity == rarity && matches.Length == 1
                && matches[0].profileId == fixture.ExactBuildProfileId(rarity)
                && matches[0].dataMaturityKey == ItemGenerationDataStatus.QaFixtureOnly
                && matches[0].Entries.Count == 1
                && matches[0].Entries[0].qualification == fixture.ExactBuildQualification(rarity)
                && snapshot.generatedInstance.buildQualification == fixture.ExactBuildQualification(rarity);
        }

        private static ScenarioRun RunScenario(Fixture fixture, string scenarioId, int sampleCount,
            ulong firstRootSeed, ScenarioKind kind, InputVariant variant, int startIndex = 0)
        {
            ScenarioAccumulator accumulator = new(scenarioId);
            StableHash outcomeHash = new();
            List<string> outcomeRows = new(sampleCount);
            for (int offset = 0; offset < sampleCount; offset++)
            {
                int sampleIndex = startIndex + offset;
                ulong rootSeed = unchecked(firstRootSeed + (ulong)sampleIndex);
                ItemDropGenerationRequest request = fixture.Request(scenarioId, sampleIndex, rootSeed,
                    variant, kind);
                ItemDropGenerationResult generated;
                try
                {
                    generated = ItemDropGenerationSandbox.Generate(request);
                }
                catch (Exception exception)
                {
                    accumulator.FailureCount++;
                    accumulator.AddViolation(sampleIndex, rootSeed, request, null,
                        "UNHANDLED_EXCEPTION", exception.GetType().FullName + ": " + exception.Message);
                    outcomeHash.Append(sampleIndex.ToString(CultureInfo.InvariantCulture));
                    string exceptionOutcome = "EXCEPTION|" + exception.GetType().FullName;
                    outcomeHash.Append(exceptionOutcome);
                    outcomeRows.Add(exceptionOutcome);
                    continue;
                }

                accumulator.TotalSamples++;
                if (generated == null || !generated.isSuccess || generated.snapshot == null
                    || generated.snapshot.generatedInstance == null)
                {
                    accumulator.FailureCount++;
                    string code = generated?.primaryValidationCode ?? "NULL_RESULT";
                    string message = generated?.ValidationErrors.FirstOrDefault()?.message
                        ?? "Generation returned no complete snapshot.";
                    accumulator.AddViolation(sampleIndex, rootSeed, request, generated?.snapshot, code, message);
                    outcomeHash.Append(sampleIndex.ToString(CultureInfo.InvariantCulture));
                    string failureOutcome = "FAIL|" + code;
                    outcomeHash.Append(failureOutcome);
                    outcomeRows.Add(failureOutcome);
                    continue;
                }

                accumulator.SuccessCount++;
                ItemDropGenerationSnapshot drop = generated.snapshot;
                ItemGeneratedInstanceSnapshot item = drop.generatedInstance;
                accumulator.ObservedBaseIds.Add(drop.selectedBaseItemId);
                if (string.Equals(drop.selectedBaseItemId,
                    ItemRarityInstanceFoundation.CoreProgressionBaseItemId, StringComparison.Ordinal))
                {
                    accumulator.I031LeakCount++;
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "I031_LEAK", "I031 appeared in an ordinary generation result.");
                }

                ValidateCommonSnapshot(fixture, accumulator, sampleIndex, rootSeed, request, drop);
                ValidateScenarioSnapshot(fixture, accumulator, sampleIndex, rootSeed, request, drop, kind);
                RecordDistributionBuckets(accumulator, drop, kind);
                outcomeHash.Append(sampleIndex.ToString(CultureInfo.InvariantCulture));
                outcomeHash.Append(rootSeed.ToString(CultureInfo.InvariantCulture));
                string canonical = drop.BuildCanonicalSignature();
                outcomeHash.Append(canonical);
                StableHash rowHash = new();
                rowHash.Append(canonical);
                outcomeRows.Add(rowHash.HexDigest);
            }

            ValidateEndpointCoverage(accumulator);
            ValidateFixedAffixMultiValueCoverage(accumulator);
            List<SimulationDistributionRow> distributionRows = BuildDistributionRows(accumulator, kind);
            string scenarioSignature = BuildScenarioSignature(scenarioId, sampleCount, firstRootSeed,
                accumulator, distributionRows, outcomeHash.HexDigest);
            SimulationResult result = new(scenarioId, sampleCount, accumulator.SuccessCount,
                accumulator.FailureCount, firstRootSeed, distributionRows, accumulator.Violations,
                Array.Empty<SimulationExpectedFailureRow>(), scenarioSignature);
            return new ScenarioRun(result, accumulator, outcomeHash.HexDigest, outcomeRows);
        }

        private static void ValidateCommonSnapshot(Fixture fixture, ScenarioAccumulator accumulator,
            int sampleIndex, ulong rootSeed, ItemDropGenerationRequest request,
            ItemDropGenerationSnapshot drop)
        {
            ItemGeneratedInstanceSnapshot item = drop.generatedInstance;
            if (!string.Equals(drop.schemaId, ItemDropGenerationSnapshot.CurrentSchemaId, StringComparison.Ordinal)
                || !string.Equals(item.schemaId, ItemGeneratedInstanceSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                || !string.Equals(drop.generationDataStatus, ItemGenerationDataStatus.QaFixtureCanonical,
                    StringComparison.Ordinal)
                || !string.Equals(item.generationDataStatus, ItemGenerationDataStatus.QaFixtureCanonical,
                    StringComparison.Ordinal)
                || !string.Equals(item.itemInstanceId, request.itemInstanceId, StringComparison.Ordinal)
                || !string.Equals(drop.dropRequestId, request.dropRequestId, StringComparison.Ordinal)
                || item.generationVersion != DeterministicItemRandom.SupportedGenerationVersion
                || unchecked((ulong)item.rootSeed) != rootSeed)
            {
                accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                    "SNAPSHOT_INCOMPLETE", "Generated snapshot identity, schema, version, seed, or QA status is incomplete.");
            }

            ItemCorePotentialQueryResult<ItemCorePotentialProfileSnapshot> coreQuery =
                fixture.Core.QueryCorePotentialProfile(item.baseItemId, item.rarity);
            accumulator.CoreChecks++;
            bool coreValid = coreQuery.isSuccess && item.GeneratedCorePotential != null
                && item.GeneratedCorePotential.EligibleCoreEffectIds.All(id =>
                    coreQuery.value.EligibleCoreEffectIds.Contains(id, StringComparer.Ordinal))
                && item.GeneratedCorePotential.VisibleCoreEffectIds.All(id =>
                    item.GeneratedCorePotential.EligibleCoreEffectIds.Contains(id, StringComparer.Ordinal));
            if (item.rarity != ItemInstanceRarity.Orange && item.GeneratedCorePotential != null)
            {
                coreValid &= !item.GeneratedCorePotential.EligibleCoreEffectIds.Any(id =>
                    coreQuery.value.PotentialEffects.Any(effect => effect.coreEffectId == id
                        && effect.effectKind == ItemCorePotentialEffectKind.Ultimate));
            }
            if (!coreValid)
            {
                accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                    "CORE_POTENTIAL_INVALID", "Generated core eligible/visible output exceeded the matching QA Core Profile.");
            }

                accumulator.BuildChecks++;
            bool buildValid = Enum.IsDefined(typeof(ItemBuildQualification), item.buildQualification)
                && item.buildQualification != ItemBuildQualification.Unresolved
                && (item.rarity != ItemInstanceRarity.White
                    || item.buildQualification == ItemBuildQualification.None);
            if (!buildValid)
            {
                accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                    "BUILD_QUALIFICATION_INVALID", "Generated BuildQualification violated the rarity boundary.");
            }

            ValidateGeneratedRanges(accumulator, sampleIndex, rootSeed, request, drop);
        }

        private static void ValidateScenarioSnapshot(Fixture fixture, ScenarioAccumulator accumulator,
            int sampleIndex, ulong rootSeed, ItemDropGenerationRequest request,
            ItemDropGenerationSnapshot drop, ScenarioKind kind)
        {
            ItemGeneratedInstanceSnapshot item = drop.generatedInstance;
            if (kind == ScenarioKind.EarlyWhite)
            {
                bool valid = drop.stageNumber >= 1 && drop.stageNumber <= 10
                    && drop.selectedRarity == ItemInstanceRarity.White
                    && item.buildQualification == ItemBuildQualification.None
                    && string.Equals(drop.rarityPolicySource,
                        ItemDropRarityPolicySources.LockedStage1To10White, StringComparison.Ordinal)
                    && string.IsNullOrEmpty(drop.rarityWeightProfileId);
                if (!valid)
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "EARLY_STAGE_WHITE_LOCK_FAILED", "Stage 1-10 did not remain white/None without a rarity profile.");
                }
            }

            if (kind == ScenarioKind.RarityRich)
            {
                ItemGeneratedAffixSnapshot fixedAffix = item.GeneratedAffixes
                    .FirstOrDefault(affix => affix.slotKind == ItemAffixSlotKind.Fixed);
                if (fixedAffix == null || !string.Equals(fixedAffix.affixId, Fixture.FixedAffixId,
                    StringComparison.Ordinal))
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "FIXED_AFFIX_ID_INVALID", "The fixed slot did not keep its configured affixId.");
                }
                else
                {
                    accumulator.FixedAffixValues.Add(fixedAffix.rawUnits);
                }
            }

            if (IsBuildExactScenario(kind))
            {
                ItemInstanceRarity expectedRarity = ExactRarity(kind);
                bool exact = IsExactBuildMatch(fixture, request, drop, expectedRarity);
                if (exact)
                {
                    accumulator.BuildExactMatchCount++;
                }
                else
                {
                    accumulator.BuildExactMismatchCount++;
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "BUILD_PROFILE_EXACT_MATCH_FAILED",
                        $"Expected '{fixture.ExactBuildProfileId(expectedRarity)}' -> "
                        + $"'{fixture.ExactBuildQualification(expectedRarity)}' for "
                        + $"'{expectedRarity.ToStableKey()}'.");
                }
            }

            if (kind == ScenarioKind.Mutex)
            {
                string[] ids = item.GeneratedAffixes.Select(affix => affix.affixId).ToArray();
                accumulator.MutexChecks++;
                accumulator.MutexAffixASeen |= ids.Contains(Fixture.MutexAffixA, StringComparer.Ordinal);
                accumulator.MutexAffixBSeen |= ids.Contains(Fixture.MutexAffixB, StringComparer.Ordinal);
                if (ids.Contains(Fixture.MutexAffixA, StringComparer.Ordinal)
                    && ids.Contains(Fixture.MutexAffixB, StringComparer.Ordinal))
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "AFFIX_MUTEX_VIOLATION", "Mutually exclusive affixes appeared in one instance.");
                }
                if (ids.GroupBy(id => id, StringComparer.Ordinal).Any(group => group.Count() > 1))
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "AFFIX_DUPLICATE_FORBIDDEN", "DisallowDuplicateAffix produced a duplicate affixId.");
                }
                accumulator.RepeatChecks++;
            }

            if (kind == ScenarioKind.AllowDuplicate)
            {
                ItemGeneratedAffixSnapshot[] random = item.GeneratedAffixes
                    .Where(affix => affix.slotKind == ItemAffixSlotKind.Random).ToArray();
                accumulator.RepeatChecks++;
                if (random.Length == 2 && random[0].affixId == random[1].affixId)
                {
                    accumulator.LegalDuplicateCount++;
                    if (random[0].rawUnits != random[1].rawUnits)
                    {
                        accumulator.DuplicateDifferentValueCount++;
                    }
                }
                else
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "AFFIX_DUPLICATE_EXPECTED", "AllowDuplicateAffix fixture did not preserve two duplicate rolls.");
                }
            }
        }

        private static void ValidateGeneratedRanges(ScenarioAccumulator accumulator,
            int sampleIndex, ulong rootSeed, ItemDropGenerationRequest request,
            ItemDropGenerationSnapshot drop)
        {
            ItemGeneratedInstanceSnapshot item = drop.generatedInstance;
            foreach (ItemGeneratedStatSnapshot stat in item.GeneratedStats)
            {
                accumulator.RangeChecks++;
                ItemStatQueryResult<ItemStatRangeProfileSnapshot> profile = request.statSchema.QueryProfile(item.baseItemId,
                    stat.statId);
                ItemStatQueryResult<ItemStatDefinitionSnapshot> definition = request.statSchema.QueryStatDefinition(stat.statId);
                ItemRarityStatRangeSnapshot range = profile.value?.RarityRanges
                    .FirstOrDefault(row => row.rarity == item.rarity);
                bool stepAligned = definition.isSuccess && range != null && definition.value.stepUnits > 0
                    && stat.rawUnits >= range.minUnits
                    && unchecked((ulong)stat.rawUnits - (ulong)range.minUnits)
                        % unchecked((ulong)definition.value.stepUnits) == 0UL;
                bool valid = profile.isSuccess && definition.isSuccess && range != null
                    && !string.IsNullOrWhiteSpace(definition.value.unitKey)
                    && Enum.IsDefined(typeof(ItemStatRoundingMode), definition.value.roundingMode)
                    && definition.value.decimalPlaces >= 0 && definition.value.stepUnits > 0
                    && stat.rawUnits >= range.minUnits && stat.rawUnits <= range.maxUnits
                    && unchecked((ulong)stat.rawUnits - (ulong)range.minUnits)
                        % unchecked((ulong)definition.value.stepUnits) == 0UL;
                if (!valid)
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "STAT_RANGE_INVALID", $"Stat '{stat.statId}' violated its current-rarity range or step.");
                }
                else
                {
                    accumulator.RangeMinimumObserved |= stat.rawUnits == range.minUnits;
                    accumulator.RangeMaximumObserved |= stat.rawUnits == range.maxUnits;
                }
                if (range != null)
                {
                    accumulator.ObserveEndpoint("stat", item.baseItemId + "@" + stat.statId,
                        item.rarity.ToStableKey(), range.minUnits, range.maxUnits, stat.rawUnits, stepAligned);
                }
            }

            foreach (ItemGeneratedAffixSnapshot affix in item.GeneratedAffixes)
            {
                accumulator.RangeChecks++;
                ItemAffixQueryResult<ItemAffixValueProfileSnapshot> profile =
                    request.affixSchema.QueryAffixValueProfile(affix.affixValueProfileId);
                ItemAffixRarityValueRangeSnapshot range = profile.value?.RarityRanges
                    .FirstOrDefault(row => row.rarity == item.rarity);
                bool stepAligned = profile.isSuccess && range != null && profile.value.stepUnits > 0
                    && affix.rawUnits >= range.minUnits
                    && unchecked((ulong)affix.rawUnits - (ulong)range.minUnits)
                        % unchecked((ulong)profile.value.stepUnits) == 0UL;
                bool valid = profile.isSuccess && range != null
                    && !string.IsNullOrWhiteSpace(profile.value.affixId)
                    && Enum.IsDefined(typeof(ItemStatRoundingMode), profile.value.roundingMode)
                    && profile.value.decimalPlaces >= 0 && profile.value.stepUnits > 0
                    && affix.rawUnits >= range.minUnits && affix.rawUnits <= range.maxUnits
                    && unchecked((ulong)affix.rawUnits - (ulong)range.minUnits)
                        % unchecked((ulong)profile.value.stepUnits) == 0UL;
                if (!valid)
                {
                    accumulator.AddViolation(sampleIndex, rootSeed, request, drop,
                        "AFFIX_RANGE_INVALID", $"Affix '{affix.affixId}' violated its current-rarity range or step.");
                }
                else
                {
                    accumulator.RangeMinimumObserved |= affix.rawUnits == range.minUnits;
                    accumulator.RangeMaximumObserved |= affix.rawUnits == range.maxUnits;
                }
                if (range != null)
                {
                    accumulator.ObserveEndpoint("affix", affix.affixValueProfileId,
                        item.rarity.ToStableKey(), range.minUnits, range.maxUnits, affix.rawUnits, stepAligned);
                    if (affix.slotKind == ItemAffixSlotKind.Fixed)
                    {
                        accumulator.ObserveFixedValue(affix.affixId, affix.affixValueProfileId,
                            item.rarity.ToStableKey(), affix.rawUnits, rootSeed);
                    }
                }
            }
        }

        private static void RecordDistributionBuckets(ScenarioAccumulator accumulator,
            ItemDropGenerationSnapshot drop, ScenarioKind kind)
        {
            if (kind == ScenarioKind.CandidateEqual || kind == ScenarioKind.CandidateWeighted)
            {
                accumulator.Increment("candidate", drop.selectedBaseItemId);
            }
            if (kind == ScenarioKind.RarityRich)
            {
                accumulator.Increment("rarity", drop.selectedRarity.ToStableKey());
                ItemGeneratedAffixSnapshot random = drop.generatedInstance.GeneratedAffixes
                    .SingleOrDefault(affix => affix.slotKind == ItemAffixSlotKind.Random);
                if (random != null) accumulator.Increment("randomAffix", random.affixId);
                accumulator.Increment("buildQualification", drop.generatedInstance.buildQualification.ToString());
            }
        }

        private static void ValidateEndpointCoverage(ScenarioAccumulator accumulator)
        {
            foreach (EndpointBucket bucket in accumulator.EndpointBuckets.Values
                .OrderBy(row => row.StableKey, StringComparer.Ordinal))
            {
                if (!bucket.minObserved || !bucket.maxObserved || bucket.stepViolationCount != 0)
                {
                    accumulator.AddSyntheticViolation("ENDPOINT_COVERAGE_MISSING",
                        $"Endpoint bucket '{bucket.StableKey}' samples={bucket.sampleCount} "
                        + $"minObserved={bucket.minObserved} maxObserved={bucket.maxObserved} "
                        + $"stepViolationCount={bucket.stepViolationCount}.");
                }
            }
        }

        private static void ValidateFixedAffixMultiValueCoverage(ScenarioAccumulator accumulator)
        {
            foreach (KeyValuePair<string, HashSet<long>> bucket in accumulator.FixedValueBuckets
                .OrderBy(row => row.Key, StringComparer.Ordinal))
            {
                bool distinctSeeds = accumulator.FixedValueSeedBuckets.TryGetValue(bucket.Key,
                    out Dictionary<long, ulong> valueSeeds) && valueSeeds.Values.Distinct().Count() >= 2;
                if (bucket.Value.Count < 2 || !distinctSeeds)
                {
                    accumulator.AddSyntheticViolation("FIXED_AFFIX_VALUE_NOT_RANDOMIZED",
                        $"Fixed affix bucket '{bucket.Key}' produced {bucket.Value.Count} distinct value(s) "
                        + $"across {(valueSeeds == null ? 0 : valueSeeds.Values.Distinct().Count())} distinct seed(s); expected at least 2.");
                }
            }
        }

        private static IReadOnlyDictionary<string, EndpointBucket> MergeEndpointBuckets(
            IEnumerable<ScenarioRun> runs)
        {
            Dictionary<string, EndpointBucket> merged = new(StringComparer.Ordinal);
            foreach (EndpointBucket source in runs.SelectMany(run => run.Accumulator.EndpointBuckets.Values))
            {
                if (!merged.TryGetValue(source.StableKey, out EndpointBucket target))
                {
                    target = new EndpointBucket(source.valueKind, source.profileId, source.rarity,
                        source.minUnits, source.maxUnits);
                    merged.Add(source.StableKey, target);
                }
                target.Merge(source);
            }
            return new ReadOnlyDictionary<string, EndpointBucket>(merged);
        }

        private static IReadOnlyDictionary<string, HashSet<long>> MergeFixedValueBuckets(
            IEnumerable<ScenarioRun> runs)
        {
            Dictionary<string, HashSet<long>> merged = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, HashSet<long>> source in runs
                .SelectMany(run => run.Accumulator.FixedValueBuckets))
            {
                if (!merged.TryGetValue(source.Key, out HashSet<long> target))
                {
                    target = new HashSet<long>();
                    merged.Add(source.Key, target);
                }
                target.UnionWith(source.Value);
            }
            return new ReadOnlyDictionary<string, HashSet<long>>(merged);
        }

        private static IReadOnlyDictionary<string, Dictionary<long, ulong>> MergeFixedValueSeedBuckets(
            IEnumerable<ScenarioRun> runs)
        {
            Dictionary<string, Dictionary<long, ulong>> merged = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Dictionary<long, ulong>> source in runs
                .SelectMany(run => run.Accumulator.FixedValueSeedBuckets))
            {
                if (!merged.TryGetValue(source.Key, out Dictionary<long, ulong> target))
                {
                    target = new Dictionary<long, ulong>();
                    merged.Add(source.Key, target);
                }
                foreach (KeyValuePair<long, ulong> valueSeed in source.Value)
                {
                    if (!target.ContainsKey(valueSeed.Key)) target.Add(valueSeed.Key, valueSeed.Value);
                }
            }
            return new ReadOnlyDictionary<string, Dictionary<long, ulong>>(merged);
        }

        private static List<SimulationDistributionRow> BuildDistributionRows(
            ScenarioAccumulator accumulator, ScenarioKind kind)
        {
            List<DistributionDefinition> definitions = new();
            if (kind == ScenarioKind.CandidateEqual)
            {
                definitions.Add(new DistributionDefinition("candidate",
                    Enumerable.Range(1, 30).ToDictionary(index => $"I{index:000}", _ => 1L,
                        StringComparer.Ordinal)));
            }
            else if (kind == ScenarioKind.CandidateWeighted)
            {
                definitions.Add(new DistributionDefinition("candidate", new Dictionary<string, long>(StringComparer.Ordinal)
                {
                    ["I001"] = 1, ["I002"] = 3, ["I003"] = 6
                }));
            }
            else if (kind == ScenarioKind.RarityRich)
            {
                definitions.Add(new DistributionDefinition("rarity", new Dictionary<string, long>(StringComparer.Ordinal)
                {
                    ["white"] = 1, ["green"] = 2, ["blue"] = 3, ["purple"] = 4, ["orange"] = 5
                }));
                definitions.Add(new DistributionDefinition("randomAffix",
                    new Dictionary<string, long>(StringComparer.Ordinal)
                    {
                        [Fixture.RandomAffixA] = 1,
                        [Fixture.RandomAffixB] = 3,
                        [Fixture.RandomAffixC] = 6,
                        [Fixture.ZeroWeightAffix] = 0
                    }));
                definitions.Add(new DistributionDefinition("buildQualification",
                    new Dictionary<string, long>(StringComparer.Ordinal)
                    {
                        [ItemBuildQualification.None.ToString()] = 24,
                        [ItemBuildQualification.FaMenOnly.ToString()] = 28,
                        [ItemBuildQualification.QiLeiOnly.ToString()] = 42,
                        [ItemBuildQualification.Dual.ToString()] = 56
                    }));
            }

            List<SimulationDistributionRow> rows = new();
            foreach (DistributionDefinition definition in definitions)
            {
                long totalWeight = definition.Weights.Values.Where(weight => weight > 0).Sum();
                int observedTotal = definition.Weights.Keys.Sum(bucket =>
                    accumulator.BucketCount(definition.DimensionKey, bucket));
                foreach (KeyValuePair<string, long> bucket in definition.Weights.OrderBy(pair => pair.Key,
                    StringComparer.Ordinal))
                {
                    int observed = accumulator.BucketCount(definition.DimensionKey, bucket.Key);
                    double expectedRatio = bucket.Value <= 0 || totalWeight <= 0
                        ? 0d : (double)bucket.Value / totalWeight;
                    double expected = observedTotal * expectedRatio;
                    double observedRatio = observedTotal == 0 ? 0d : (double)observed / observedTotal;
                    double delta = Math.Abs(observed - expected);
                    double denominator = Math.Sqrt(observedTotal * expectedRatio * (1d - expectedRatio));
                    double zScore = bucket.Value == 0 ? (observed == 0 ? 0d : double.PositiveInfinity)
                        : denominator <= 0d ? (observed == expected ? 0d : double.PositiveInfinity)
                        : delta / denominator;
                    bool expectedLargeEnough = bucket.Value == 0 || expected >= 100d;
                    bool pass = expectedLargeEnough && (bucket.Value == 0 ? observed == 0 : zScore <= 6d);
                    rows.Add(new SimulationDistributionRow(accumulator.ScenarioId, definition.DimensionKey,
                        bucket.Key, bucket.Value, observed, observedRatio, expected, expectedRatio,
                        delta, zScore, pass));
                }
                if (observedTotal != accumulator.SuccessCount)
                {
                    accumulator.AddSyntheticViolation("DISTRIBUTION_TOTAL_MISMATCH",
                        $"{definition.DimensionKey} observed total {observedTotal} != successCount {accumulator.SuccessCount}.");
                }
            }
            return rows;
        }

        private static void RunExtremeAndBatchDeterminism(Fixture fixture, ScenarioRun candidate,
            List<DeterminismRow> determinism, Verification verification)
        {
            ulong[] seeds = { 0UL, 1UL, unchecked((ulong)long.MaxValue), unchecked((ulong)long.MinValue), ulong.MaxValue };
            foreach (ulong seed in seeds)
            {
                ItemDropGenerationResult first = ItemDropGenerationSandbox.Generate(
                    fixture.Request("L_EXTREME_SEED", 0, seed, InputVariant.None, ScenarioKind.CandidateEqual));
                ItemDropGenerationResult second = ItemDropGenerationSandbox.Generate(
                    fixture.Request("L_EXTREME_SEED", 0, seed, InputVariant.None, ScenarioKind.CandidateEqual));
                bool pass = first.isSuccess && second.isSuccess
                    && first.snapshot.BuildCanonicalSignature() == second.snapshot.BuildCanonicalSignature();
                AddDeterminism(determinism, verification, "extreme-seed-" + seed.ToString("X16",
                    CultureInfo.InvariantCulture), seed, "extreme rootSeed", first.snapshot?.BuildCanonicalSignature(),
                    second.snapshot?.BuildCanonicalSignature(), 1, pass);
            }

            StableHash wrapHashA = new();
            StableHash wrapHashB = new();
            ulong firstWrap = ulong.MaxValue - 2UL;
            for (int index = 0; index < 6; index++)
            {
                ulong seed = unchecked(firstWrap + (ulong)index);
                ItemDropGenerationResult a = ItemDropGenerationSandbox.Generate(
                    fixture.Request("L_ULONG_WRAP", index, seed, InputVariant.None, ScenarioKind.CandidateEqual));
                ItemDropGenerationResult b = ItemDropGenerationSandbox.Generate(
                    fixture.Request("L_ULONG_WRAP", index, seed, InputVariant.None, ScenarioKind.CandidateEqual));
                wrapHashA.Append(a.snapshot?.BuildCanonicalSignature() ?? "FAIL|" + a.primaryValidationCode);
                wrapHashB.Append(b.snapshot?.BuildCanonicalSignature() ?? "FAIL|" + b.primaryValidationCode);
            }
            AddDeterminism(determinism, verification, "ulong-sequence-wrap", firstWrap, "unchecked ulong wrap",
                wrapHashA.HexDigest, wrapHashB.HexDigest, 6, wrapHashA.HexDigest == wrapHashB.HexDigest);

            ScenarioRun batchA = RunScenario(fixture, "B_CANDIDATE_EQUAL_30", 15000, 100000UL,
                ScenarioKind.CandidateEqual, InputVariant.None, 0);
            ScenarioRun batchB = RunScenario(fixture, "B_CANDIDATE_EQUAL_30", 15000, 100000UL,
                ScenarioKind.CandidateEqual, InputVariant.None, 15000);
            bool batchPass = batchA.Result.failureCount == 0 && batchB.Result.failureCount == 0
                && batchA.Result.invariantViolations.Count == 0 && batchB.Result.invariantViolations.Count == 0
                && candidate.Result.failureCount == 0
                && candidate.OutcomeRows.SequenceEqual(batchA.OutcomeRows.Concat(batchB.OutcomeRows));
            StableHash combined = new();
            foreach (string row in batchA.OutcomeRows.Concat(batchB.OutcomeRows)) combined.Append(row);
            StableHash expected = new();
            foreach (string row in candidate.OutcomeRows) expected.Append(row);
            AddDeterminism(determinism, verification, "batched-vs-single-by-seed", 100000UL,
                "two 15000 batches aggregated by seed", expected.HexDigest, combined.HexDigest, 30000, batchPass);
        }

        private static void RunPolicyAndFailureSpecs(Fixture fixture, List<SpecRow> specs,
            List<SimulationExpectedFailureRow> expectedFailures, Verification verification)
        {
            ItemDropGenerationRequest missingRarity = fixture.Request("missing-rarity", 0, 1UL,
                InputVariant.None, ScenarioKind.RarityRich, forceRarityNull: true);
            ExpectFailure(specs, expectedFailures, verification, "stage11-rarity-policy-unresolved",
                ItemDropGenerationSandbox.Generate(missingRarity),
                ItemDropGenerationValidationCodes.RarityPolicyUnresolved);

            ItemDropGenerationRequest i031 = fixture.Request("i031-injected", 0, 2UL,
                InputVariant.None, ScenarioKind.CandidateEqual,
                candidateOverride: fixture.CandidatePool("qa_equal_30",
                    new ItemDropCandidateEntrySnapshot(ItemRarityInstanceFoundation.CoreProgressionBaseItemId, 1)));
            ExpectFailure(specs, expectedFailures, verification, "i031-injection-explicit-failure",
                ItemDropGenerationSandbox.Generate(i031), ItemDropGenerationValidationCodes.I031Forbidden);

            ItemDropGenerationRequest capacity = fixture.Request("repeat-capacity", 0, 3UL,
                InputVariant.None, ScenarioKind.RarityRich, affixOverride: fixture.CapacityExhaustionAffixes);
            ExpectFailure(specs, expectedFailures, verification, "repeat-pool-capacity-insufficient",
                ItemDropGenerationSandbox.Generate(capacity), ItemDropGenerationValidationCodes.InstanceRollFailed);

            ItemDropGenerationRequest mutex = fixture.Request("mutex-exhaustion", 0, 4UL,
                InputVariant.None, ScenarioKind.RarityRich, affixOverride: fixture.MutexExhaustionAffixes);
            ExpectFailure(specs, expectedFailures, verification, "mutex-exhaustion-explicit-failure",
                ItemDropGenerationSandbox.Generate(mutex), ItemDropGenerationValidationCodes.InstanceRollFailed);

            ItemDropGenerationRequest zeroWeight = fixture.Request("zero-weight", 0, 5UL,
                InputVariant.None, ScenarioKind.RarityRich, affixOverride: fixture.ZeroWeightAffixes);
            ExpectFailure(specs, expectedFailures, verification, "zero-weight-entry-rejected-and-never-generated",
                ItemDropGenerationSandbox.Generate(zeroWeight), ItemDropGenerationValidationCodes.InstanceRollFailed);

            ItemDropGenerationRequest missingBuild = fixture.Request("missing-build", 0, 6UL,
                InputVariant.None, ScenarioKind.RarityRich, buildOverride: Array.Empty<ItemBuildQualificationRollProfile>());
            ExpectFailure(specs, expectedFailures, verification, "higher-rarity-build-profile-missing",
                ItemDropGenerationSandbox.Generate(missingBuild),
                ItemDropGenerationValidationCodes.BuildRollProfileMissing);

            ItemBuildQualificationRollProfile duplicate = fixture.BuildProfile(ItemInstanceRarity.Green,
                InputVariant.None);
            ItemDropGenerationRequest duplicateBuild = fixture.Request("duplicate-build", 0, 7UL,
                InputVariant.None, ScenarioKind.RarityRich,
                rarityOverride: fixture.SingleRarity(ItemInstanceRarity.Green),
                buildOverride: new[] { duplicate, duplicate });
            ExpectFailure(specs, expectedFailures, verification, "higher-rarity-build-profile-duplicate",
                ItemDropGenerationSandbox.Generate(duplicateBuild),
                ItemDropGenerationValidationCodes.BuildRollProfileDuplicate);

            ItemDropGenerationRequest defaultFormalEmpty = new("formal-empty", "formal-instance", 8L, 1,
                ItemGenerationDataStatus.QaFixtureCanonical, fixture.Foundation,
                fixture.Source(11, "qa_one", "qa_rarity_11_plus"), fixture.CandidateOne,
                fixture.RarityProfile(InputVariant.None), null, null, null,
                Array.Empty<ItemBuildQualificationRollProfile>());
            ExpectFailure(specs, expectedFailures, verification, "default-formal-empty-cannot-pass",
                ItemDropGenerationSandbox.Generate(defaultFormalEmpty),
                ItemDropGenerationValidationCodes.CoreProfileMissing);

            CheckBool(specs, verification, "failure-snapshot-null", "failure",
                expectedFailures.All(row => row.snapshotIsNull));
            CheckBool(specs, verification, "expected-failure-codes-exact", "failure",
                expectedFailures.All(row => row.expectedCode == row.actualCode));
        }

        private static void RunReadOnlySpecs(List<ScenarioRun> runs, SimulationSuiteResult suite,
            List<SpecRow> specs, Verification verification)
        {
            SimulationResult sample = runs.FirstOrDefault()?.Result;
            CheckBool(specs, verification, "distribution-collection-read-only", "readonly",
                sample != null && RejectMutation(sample.distributionRows));
            CheckBool(specs, verification, "violation-collection-read-only", "readonly",
                sample != null && RejectMutation(sample.invariantViolations));
            CheckBool(specs, verification, "expected-failure-collection-read-only", "readonly",
                suite != null && suite.expectedFailureRows.Count == 11
                && RejectMutation(suite.expectedFailureRows));
            CheckBool(specs, verification, "expected-failure-suite-nonempty-count-11", "readonly",
                suite != null && suite.expectedFailureRows.Count == 11);
            CheckBool(specs, verification, "expected-failure-suite-fields-complete", "readonly",
                suite != null && suite.expectedFailureRows.All(row =>
                    !string.IsNullOrWhiteSpace(row.expectedCode)
                    && !string.IsNullOrWhiteSpace(row.actualCode) && row.snapshotIsNull && row.isPass));
            CheckBool(specs, verification, "expected-failure-suite-canonical-stable", "canonical",
                suite != null && suite.canonicalSignature == BuildAggregateSignature(runs,
                    suite.expectedFailureRows));
            CheckBool(specs, verification, "scenario-result-suite-read-only", "readonly",
                suite != null && suite.scenarioResults.Count == runs.Count
                && RejectMutation(suite.scenarioResults));
            CheckBool(specs, verification, "canonical-signature-no-runtime-metadata", "canonical",
                runs.All(run => !run.Result.canonicalSignature.Contains("elapsed", StringComparison.OrdinalIgnoreCase)
                    && !run.Result.canonicalSignature.Contains("machine", StringComparison.OrdinalIgnoreCase)
                    && !run.Result.canonicalSignature.Contains("time", StringComparison.OrdinalIgnoreCase)));
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
                bool pass = File.Exists(regression.path)
                    && File.ReadAllText(regression.path).Contains(regression.marker);
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
            if (verification.FoundationCount != 159) verification.Errors.Add("Foundation Spec must remain 159.");
            if (verification.StatCount != 39) verification.Errors.Add("StatRange Spec must remain 39.");
            if (verification.CoreCount != 69) verification.Errors.Add("CorePotential Spec must remain 69.");
            if (verification.AffixCount != 88) verification.Errors.Add("AffixSchema Spec must remain 88.");
            if (verification.RollCount != 72) verification.Errors.Add("RollEngine Spec must remain 72.");
            if (verification.DropSpecCount != 87) verification.Errors.Add("DropSandbox Spec must remain 87.");
            if (verification.DropDeterminismCount != 6) verification.Errors.Add("DropSandbox Determinism must remain 6.");
        }

        private static void RunLeakChecks(List<LeakRow> rows, Verification verification)
        {
            string source = File.Exists(SourcePath) ? File.ReadAllText(SourcePath) : string.Empty;
            string codeOnly = StripCommentsAndStrings(source);
            LeakDefinition[] definitions =
            {
                new("Reward connection", "Reward"), new("RunFlow connection", "RunFlow"),
                new("Inventory write", "Inventory"), new("SaveData write", "SaveData"),
                new("Battle connection", "BattleResolver"), new("Boss reward", "BossReward"),
                new("Formal drop UI", "DropUi"), new("Scene modification", "EditorSceneManager"),
                new("Prefab modification", "PrefabUtility"), new("BuildSettings modification", "EditorBuildSettings"),
                new("Formal Catalog write", "CreateAsset"), new("Unity random", "UnityEngine.Random"),
                new("System random", "System.Random"), new("Guid random", "Guid.NewGuid"),
                new("Current clock", "DateTime.Now"), new("Tick clock", "Environment.TickCount"),
                new("Mathematics random", "Unity.Mathematics.Random"), new("itemPower", "itemPower"),
                new("hidden quality", "qualityScore"), new("rarity multiplier", "rarityMultiplier"),
                new("cultivation unlock", "unlockedCoreEffectIds"), new("battle lighting", "isLit"),
                new("formal pity", "Pity"), new("formal source table", "DropTable")
            };
            foreach (LeakDefinition definition in definitions)
            {
                int count = Count(codeOnly, definition.token);
                rows.Add(new LeakRow(definition.category, count));
                if (count != 0)
                    verification.Errors.Add($"LeakCheck '{definition.category}' found {count} occurrence(s).");
            }
        }

        private static string StripCommentsAndStrings(string source)
        {
            if (string.IsNullOrEmpty(source)) return string.Empty;
            StringBuilder output = new(source.Length);
            bool lineComment = false, blockComment = false, quoted = false, verbatim = false, character = false;
            for (int index = 0; index < source.Length; index++)
            {
                char current = source[index];
                char next = index + 1 < source.Length ? source[index + 1] : '\0';
                if (lineComment)
                {
                    if (current == '\n') { lineComment = false; output.Append('\n'); }
                    else output.Append(' ');
                    continue;
                }
                if (blockComment)
                {
                    if (current == '*' && next == '/') { blockComment = false; output.Append("  "); index++; }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (quoted)
                {
                    if (verbatim && current == '"' && next == '"') { output.Append("  "); index++; continue; }
                    if ((!verbatim && current == '\\' && next != '\0')) { output.Append("  "); index++; continue; }
                    if (current == '"') { quoted = false; verbatim = false; }
                    output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (character)
                {
                    if (current == '\\' && next != '\0') { output.Append("  "); index++; continue; }
                    if (current == '\'') character = false;
                    output.Append(' ');
                    continue;
                }
                if (current == '/' && next == '/') { lineComment = true; output.Append("  "); index++; continue; }
                if (current == '/' && next == '*') { blockComment = true; output.Append("  "); index++; continue; }
                if (current == '@' && next == '"') { quoted = true; verbatim = true; output.Append("  "); index++; continue; }
                if (current == '"') { quoted = true; output.Append(' '); continue; }
                if (current == '\'') { character = true; output.Append(' '); continue; }
                output.Append(current);
            }
            return output.ToString();
        }

        private static void WriteReports(IReadOnlyList<ScenarioRun> runs, SimulationSuiteResult suite,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<SimulationDistributionRow> distributions,
            IReadOnlyList<SimulationInvariantViolation> violations,
            IReadOnlyList<SimulationExpectedFailureRow> expectedFailures,
            IReadOnlyList<DeterminismRow> determinism, IReadOnlyList<LeakRow> leaks,
            Verification verification, string canonicalSignature, bool passed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            UTF8Encoding utf8 = new(false);
            File.WriteAllText(ReportPath, BuildReport(runs, suite, specs, distributions, violations,
                expectedFailures, determinism, leaks, verification, canonicalSignature, passed), utf8);
            File.WriteAllText(SpecPath, BuildSpec(specs), utf8);
            File.WriteAllText(DistributionPath, BuildDistribution(distributions), utf8);
            File.WriteAllText(ViolationPath, BuildViolations(violations), utf8);
            File.WriteAllText(DeterminismPath, BuildDeterminism(determinism), utf8);
            File.WriteAllText(LeakPath, BuildLeak(leaks, passed), utf8);
            AssetDatabase.Refresh();
        }

        private static string BuildReport(IReadOnlyList<ScenarioRun> runs, SimulationSuiteResult suite,
            IReadOnlyList<SpecRow> specs,
            IReadOnlyList<SimulationDistributionRow> distributions,
            IReadOnlyList<SimulationInvariantViolation> violations,
            IReadOnlyList<SimulationExpectedFailureRow> expectedFailures,
            IReadOnlyList<DeterminismRow> determinism, IReadOnlyList<LeakRow> leaks,
            Verification verification, string canonicalSignature, bool passed)
        {
            int scenarioSamples = runs.Sum(run => run.Result.sampleCount);
            double maxCandidateZ = MaxZ(distributions.Where(row => row.dimensionKey == "candidate"));
            double maxRarityZ = MaxZ(distributions.Where(row => row.dimensionKey == "rarity"));
            int rangeChecks = runs.Sum(run => run.Accumulator.RangeChecks);
            int mutexChecks = runs.Sum(run => run.Accumulator.MutexChecks);
            int repeatChecks = runs.Sum(run => run.Accumulator.RepeatChecks);
            int i031Leaks = runs.Sum(run => run.Accumulator.I031LeakCount);
            IReadOnlyDictionary<string, EndpointBucket> endpointBuckets = MergeEndpointBuckets(runs);
            IReadOnlyDictionary<string, HashSet<long>> fixedValueBuckets = MergeFixedValueBuckets(runs);
            IReadOnlyDictionary<string, Dictionary<long, ulong>> fixedValueSeedBuckets =
                MergeFixedValueSeedBuckets(runs);
            StringBuilder builder = new();
            builder.AppendLine("# ItemGenerationSimulationValidator01 Report").AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(passed ? PassMarker : FailMarker)}`")
                .AppendLine($"- Result schema: `{SimulationSchemaId}`")
                .AppendLine($"- Suite schema: `{suite?.schemaId ?? SimulationSuiteSchemaId}`")
                .AppendLine($"- simulationVersion: `{SimulationVersion}`")
                .AppendLine($"- generationVersion: `{DeterministicItemRandom.SupportedGenerationVersion}`")
                .AppendLine($"- fixtureStatus: `{ItemGenerationDataStatus.QaFixtureCanonical}`")
                .AppendLine("- Isolation markers: `QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA`")
                .AppendLine("- Notice: 仅用于验证算法分布，不代表正式游戏概率或数值。")
                .AppendLine($"- Threshold marker: `{ThresholdMarker}`; zScore <= 6; positive expectedCount >= 100.")
                .AppendLine("- Execution path: `ItemDropGenerationSandbox -> ItemInstanceRollEngine -> ItemGeneratedInstanceSnapshot`.")
                .AppendLine("- Scope: Editor/QA-only in-memory simulation and report export.")
                .AppendLine("- Not connected: Reward, RunFlow, Inventory, SaveData, Battle, Boss reward, formal drop UI, formal Catalog, pity or source tables.")
                .AppendLine($"- Scenarios: {runs.Count}")
                .AppendLine($"- Accepted scenario samples: {scenarioSamples}")
                .AppendLine($"- Total real generation calls including replay/reversal/batches: {verification.ExecutedSampleCount}")
                .AppendLine($"- Spec: {specs.Count(row => row.result == "PASS")}/{specs.Count} PASS")
                .AppendLine($"- Expected failures: {expectedFailures.Count(row => row.isPass)}/{expectedFailures.Count} PASS")
                .AppendLine($"- Suite expectedFailureRows: {suite?.expectedFailureRows.Count ?? 0}; external Clear attack: {(suite != null && RejectMutation(suite.expectedFailureRows) ? "PASS" : "FAIL")}")
                .AppendLine($"- Determinism: {determinism.Count(row => row.result == "PASS")}/{determinism.Count} PASS")
                .AppendLine($"- Candidate max zScore: {FormatDouble(maxCandidateZ)}")
                .AppendLine($"- Rarity max zScore: {FormatDouble(maxRarityZ)}")
                .AppendLine($"- Stat/affix range checks: {rangeChecks}; violations: {violations.Count(row => row.validationCode.Contains("RANGE", StringComparison.Ordinal))}")
                .AppendLine($"- Endpoint buckets: {endpointBuckets.Count}; covered: {endpointBuckets.Values.Count(bucket => bucket.isCovered)}; missing: {endpointBuckets.Values.Count(bucket => !bucket.isCovered)}")
                .AppendLine($"- Mutex checks: {mutexChecks}; violations: {violations.Count(row => row.validationCode.Contains("MUTEX", StringComparison.Ordinal))}")
                .AppendLine($"- Repeat checks: {repeatChecks}; violations: {violations.Count(row => row.validationCode.Contains("DUPLICATE", StringComparison.Ordinal))}")
                .AppendLine($"- I031 ordinary leak count: {i031Leaks}")
                .AppendLine($"- Invariant violations: {violations.Count}")
                .AppendLine($"- Canonical Signature: `{canonicalSignature}`")
                .AppendLine().AppendLine("## Scenario Results").AppendLine()
                .AppendLine("| Scenario | Samples | Success | Failure | Signature |")
                .AppendLine("| --- | ---: | ---: | ---: | --- |");
            foreach (ScenarioRun run in runs.OrderBy(run => run.Result.scenarioId, StringComparer.Ordinal))
            {
                builder.AppendLine($"| {run.Result.scenarioId} | {run.Result.sampleCount} | {run.Result.successCount} | {run.Result.failureCount} | `{run.Result.canonicalSignature}` |");
            }
            builder.AppendLine().AppendLine("## Exact Build Profile Match").AppendLine()
                .AppendLine("| Rarity | Required Profile | Required Qualification | Matched Samples | Mismatches |")
                .AppendLine("| --- | --- | --- | ---: | ---: |");
            foreach (BuildExactResult row in verification.BuildExactResults)
            {
                builder.AppendLine($"| {row.rarity} | `{row.profileId}` | {row.qualification} | {row.matchCount} | {row.mismatchCount} |");
            }
            builder.AppendLine().AppendLine("- Swapped, mismatched, and missing corresponding profiles are independent rejecting specs.")
                .AppendLine().AppendLine("## Endpoint Buckets").AppendLine()
                .AppendLine("| valueKind | profileId | rarity | minObserved | maxObserved | stepViolationCount | sampleCount |")
                .AppendLine("| --- | --- | --- | --- | --- | ---: | ---: |");
            foreach (EndpointBucket bucket in endpointBuckets.Values
                .OrderBy(bucket => bucket.StableKey, StringComparer.Ordinal))
            {
                builder.AppendLine($"| {bucket.valueKind} | `{bucket.profileId}` | {bucket.rarity} | {bucket.minObserved.ToString().ToLowerInvariant()} | {bucket.maxObserved.ToString().ToLowerInvariant()} | {bucket.stepViolationCount} | {bucket.sampleCount} |");
            }
            builder.AppendLine().AppendLine("## Fixed Affix Same-Profile Same-Rarity Values").AppendLine()
                .AppendLine("| affixId + affixValueProfileId + rarity | Distinct Count | Value@FirstRootSeed Proof |")
                .AppendLine("| --- | ---: | --- |");
            foreach (KeyValuePair<string, HashSet<long>> bucket in fixedValueBuckets
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                fixedValueSeedBuckets.TryGetValue(bucket.Key, out Dictionary<long, ulong> valueSeeds);
                string proof = valueSeeds == null ? string.Empty : string.Join("; ", valueSeeds
                    .OrderBy(pair => pair.Key).Select(pair => pair.Key.ToString(CultureInfo.InvariantCulture)
                        + "@" + pair.Value.ToString(CultureInfo.InvariantCulture)));
                builder.AppendLine($"| `{bucket.Key}` | {bucket.Value.Count} | `{proof}` |");
            }
            builder.AppendLine().AppendLine("## Expected Failure Suite Rows").AppendLine()
                .AppendLine("| Scenario | expectedCode | actualCode | snapshotIsNull | isPass |")
                .AppendLine("| --- | --- | --- | --- | --- |");
            foreach (SimulationExpectedFailureRow row in suite?.expectedFailureRows
                ?? Array.Empty<SimulationExpectedFailureRow>())
            {
                builder.AppendLine($"| {row.scenarioId} | `{row.expectedCode}` | `{row.actualCode}` | {row.snapshotIsNull.ToString().ToLowerInvariant()} | {row.isPass.ToString().ToLowerInvariant()} |");
            }
            builder.AppendLine().AppendLine("## Historical Regressions").AppendLine();
            foreach (string regression in verification.Regressions) builder.AppendLine("- " + regression);
            builder.AppendLine($"- Foundation: {verification.FoundationCount}/159 PASS")
                .AppendLine($"- StatRange: {verification.StatCount}/39 PASS")
                .AppendLine($"- CorePotential: {verification.CoreCount}/69 PASS")
                .AppendLine($"- AffixSchema: {verification.AffixCount}/88 PASS")
                .AppendLine($"- RollEngine: {verification.RollCount}/72 PASS")
                .AppendLine($"- DropSandbox: {verification.DropSpecCount}/87 Spec, {verification.DropDeterminismCount}/6 Determinism PASS")
                .AppendLine().AppendLine("## LeakCheck").AppendLine()
                .AppendLine($"- Categories: {leaks.Count}; total leaks: {leaks.Sum(row => row.count)}")
                .AppendLine("- Scene / Prefab / BuildSettings modifications by this verifier: 0.")
                .AppendLine().AppendLine("## Errors").AppendLine();
            if (verification.Errors.Count == 0) builder.AppendLine("- None.");
            else foreach (string error in verification.Errors) builder.AppendLine("- " + error);
            return builder.ToString();
        }

        private static string BuildSpec(IEnumerable<SpecRow> rows)
        {
            StringBuilder builder = new("caseId,category,expected,actual,validationCode,result\n");
            foreach (SpecRow row in rows.OrderBy(row => row.caseId, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.caseId)).Append(',').Append(Csv(row.category)).Append(',')
                    .Append(Csv(row.expected)).Append(',').Append(Csv(row.actual)).Append(',')
                    .Append(Csv(row.code)).Append(',').Append(Csv(row.result)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildDistribution(IEnumerable<SimulationDistributionRow> rows)
        {
            StringBuilder builder = new("scenarioId,dimensionKey,bucketKey,weight,observedCount,observedRatio,expectedCount,expectedRatio,absoluteDelta,zScore,isPass\n");
            foreach (SimulationDistributionRow row in rows.OrderBy(row => row.scenarioId, StringComparer.Ordinal)
                .ThenBy(row => row.dimensionKey, StringComparer.Ordinal).ThenBy(row => row.bucketKey, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.scenarioId)).Append(',').Append(Csv(row.dimensionKey)).Append(',')
                    .Append(Csv(row.bucketKey)).Append(',').Append(row.weight).Append(',')
                    .Append(row.observedCount).Append(',').Append(FormatDouble(row.observedRatio)).Append(',')
                    .Append(FormatDouble(row.expectedCount)).Append(',').Append(FormatDouble(row.expectedRatio)).Append(',')
                    .Append(FormatDouble(row.absoluteDelta)).Append(',').Append(FormatDouble(row.zScore)).Append(',')
                    .Append(row.isPass ? "true" : "false").Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildViolations(IEnumerable<SimulationInvariantViolation> rows)
        {
            SimulationInvariantViolation[] ordered = rows.OrderBy(row => row.scenarioId, StringComparer.Ordinal)
                .ThenBy(row => row.sampleIndex).ThenBy(row => row.validationCode, StringComparer.Ordinal).ToArray();
            StringBuilder builder = new("scenarioId,sampleIndex,rootSeed,dropRequestId,itemInstanceId,baseItemId,rarity,validationCode,message\n");
            foreach (SimulationInvariantViolation row in ordered)
            {
                builder.Append(Csv(row.scenarioId)).Append(',').Append(row.sampleIndex).Append(',')
                    .Append(row.rootSeed.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.dropRequestId)).Append(',').Append(Csv(row.itemInstanceId)).Append(',')
                    .Append(Csv(row.baseItemId)).Append(',').Append(Csv(row.rarity)).Append(',')
                    .Append(Csv(row.validationCode)).Append(',').Append(Csv(row.message)).Append('\n');
            }
            builder.Append(Csv("__SUMMARY__")).Append(",-1,0,,,,,")
                .Append(Csv("TOTAL_VIOLATIONS")).Append(',').Append(Csv(ordered.Length.ToString(CultureInfo.InvariantCulture))).Append('\n');
            return builder.ToString();
        }

        private static string BuildDeterminism(IEnumerable<DeterminismRow> rows)
        {
            StringBuilder builder = new("caseId,firstRootSeed,domain,expectedSignature,actualSignature,repeatCount,result\n");
            foreach (DeterminismRow row in rows.OrderBy(row => row.caseId, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.caseId)).Append(',').Append(row.firstRootSeed.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.domain)).Append(',').Append(Csv(row.expectedSignature)).Append(',')
                    .Append(Csv(row.actualSignature)).Append(',').Append(row.repeatCount).Append(',')
                    .Append(Csv(row.result)).Append('\n');
            }
            return builder.ToString();
        }

        private static string BuildLeak(IEnumerable<LeakRow> rows, bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemGenerationSimulationValidator01 LeakCheck Report").AppendLine()
                .AppendLine($"- Scan source: `{SourcePath}`")
                .AppendLine("- Scanner ignores comments and string literals; counts executable identifiers only.")
                .AppendLine($"- Result: {(passed && rows.All(row => row.count == 0) ? "PASS" : "FAIL")}")
                .AppendLine("- Package writes reports only; Scene / Prefab / BuildSettings writes: 0.")
                .AppendLine().AppendLine("| Category | Count |").AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows) builder.AppendLine($"| {row.category} | {row.count} |");
            return builder.ToString();
        }

        private static void AddRun(List<ScenarioRun> runs, ScenarioRun run, List<SpecRow> specs,
            Verification verification, string caseId)
        {
            runs.Add(run);
            CheckBool(specs, verification, caseId, "scenario",
                run.Result.successCount == run.Result.sampleCount
                && run.Result.failureCount == 0
                && run.Result.invariantViolations.Count == 0
                && run.Result.distributionRows.All(row => row.isPass));
        }

        private static void CheckDeterminism(List<DeterminismRow> rows, Verification verification,
            string caseId, ScenarioRun expected, ScenarioRun actual, string domain, int repeatCount)
        {
            bool pass = expected.OutcomeSignature == actual.OutcomeSignature
                && expected.Result.canonicalSignature == actual.Result.canonicalSignature;
            AddDeterminism(rows, verification, caseId, expected.Result.firstRootSeed, domain,
                expected.Result.canonicalSignature, actual.Result.canonicalSignature, repeatCount, pass);
        }

        private static void AddDeterminism(List<DeterminismRow> rows, Verification verification,
            string caseId, ulong firstRootSeed, string domain, string expected, string actual,
            int repeatCount, bool pass)
        {
            rows.Add(new DeterminismRow(caseId, firstRootSeed, domain, Compact(expected), Compact(actual),
                repeatCount, pass ? "PASS" : "FAIL"));
            if (!pass) verification.Errors.Add("Determinism case failed: " + caseId + ".");
        }

        private static void CheckValidation(List<SpecRow> specs, Verification verification,
            string caseId, SimulationRequest request, string expectedCode)
        {
            string actual = ValidateSimulationRequest(request);
            Check(specs, verification, caseId, "input", expectedCode, actual, actual);
        }

        private static void ExpectFailure(List<SpecRow> specs,
            List<SimulationExpectedFailureRow> expectedFailures, Verification verification,
            string caseId, ItemDropGenerationResult actual, string expectedCode)
        {
            string actualCode = actual?.primaryValidationCode ?? "NULL_RESULT";
            bool snapshotNull = actual?.snapshot == null;
            bool pass = actual != null && !actual.isSuccess && snapshotNull
                && string.Equals(actualCode, expectedCode, StringComparison.Ordinal);
            expectedFailures.Add(new SimulationExpectedFailureRow(caseId, expectedCode, actualCode,
                snapshotNull, pass));
            specs.Add(new SpecRow(caseId, "expected-failure", expectedCode, actualCode,
                actualCode, pass ? "PASS" : "FAIL"));
            if (!pass)
                verification.Errors.Add($"Expected failure '{caseId}' expected {expectedCode}, actual {actualCode}.");
        }

        private static void CheckBool(List<SpecRow> rows, Verification verification,
            string caseId, string category, bool pass)
        {
            rows.Add(new SpecRow(caseId, category, "true", pass ? "true" : "false", "NONE",
                pass ? "PASS" : "FAIL"));
            if (!pass) verification.Errors.Add("Boolean spec failed: " + caseId + ".");
        }

        private static void Check(List<SpecRow> rows, Verification verification,
            string caseId, string category, string expected, string actual, string code)
        {
            bool pass = string.Equals(expected, actual, StringComparison.Ordinal);
            rows.Add(new SpecRow(caseId, category, expected, actual, code, pass ? "PASS" : "FAIL"));
            if (!pass) verification.Errors.Add($"Spec '{caseId}' expected {expected}, actual {actual}.");
        }

        private static bool RejectMutation<T>(IReadOnlyList<T> values)
        {
            if (values == null) return false;
            try { ((IList)values).Clear(); return false; }
            catch (NotSupportedException) { return true; }
            catch (InvalidCastException) { return true; }
        }

        private static string BuildScenarioSignature(string scenarioId, int sampleCount, ulong firstRootSeed,
            ScenarioAccumulator accumulator, IEnumerable<SimulationDistributionRow> rows,
            string outcomeSignature)
        {
            StableHash hash = new();
            hash.Append(SimulationSchemaId);
            hash.Append(SimulationVersion);
            hash.Append(ItemGenerationDataStatus.QaFixtureCanonical);
            hash.Append(scenarioId);
            hash.Append(sampleCount.ToString(CultureInfo.InvariantCulture));
            hash.Append(firstRootSeed.ToString(CultureInfo.InvariantCulture));
            hash.Append(accumulator.SuccessCount.ToString(CultureInfo.InvariantCulture));
            hash.Append(accumulator.FailureCount.ToString(CultureInfo.InvariantCulture));
            hash.Append(outcomeSignature);
            foreach (SimulationDistributionRow row in rows.OrderBy(row => row.dimensionKey, StringComparer.Ordinal)
                .ThenBy(row => row.bucketKey, StringComparer.Ordinal))
            {
                hash.Append(row.dimensionKey); hash.Append(row.bucketKey); hash.Append(row.weight.ToString(CultureInfo.InvariantCulture));
                hash.Append(row.observedCount.ToString(CultureInfo.InvariantCulture)); hash.Append(FormatDouble(row.expectedCount));
                hash.Append(FormatDouble(row.zScore)); hash.Append(row.isPass ? "1" : "0");
            }
            foreach (EndpointBucket bucket in accumulator.EndpointBuckets.Values
                .OrderBy(bucket => bucket.StableKey, StringComparer.Ordinal))
            {
                hash.Append(bucket.StableKey);
                hash.Append(bucket.minObserved ? "1" : "0");
                hash.Append(bucket.maxObserved ? "1" : "0");
                hash.Append(bucket.stepViolationCount.ToString(CultureInfo.InvariantCulture));
                hash.Append(bucket.sampleCount.ToString(CultureInfo.InvariantCulture));
            }
            foreach (KeyValuePair<string, HashSet<long>> bucket in accumulator.FixedValueBuckets
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                hash.Append(bucket.Key);
                foreach (long value in bucket.Value.OrderBy(value => value))
                    hash.Append(value.ToString(CultureInfo.InvariantCulture));
            }
            foreach (KeyValuePair<string, Dictionary<long, ulong>> bucket in accumulator.FixedValueSeedBuckets
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                hash.Append(bucket.Key);
                foreach (KeyValuePair<long, ulong> valueSeed in bucket.Value.OrderBy(pair => pair.Key))
                {
                    hash.Append(valueSeed.Key.ToString(CultureInfo.InvariantCulture));
                    hash.Append(valueSeed.Value.ToString(CultureInfo.InvariantCulture));
                }
            }
            foreach (SimulationInvariantViolation violation in accumulator.Violations
                .OrderBy(row => row.sampleIndex).ThenBy(row => row.validationCode, StringComparer.Ordinal))
            {
                hash.Append(violation.sampleIndex.ToString(CultureInfo.InvariantCulture));
                hash.Append(violation.validationCode); hash.Append(violation.message);
            }
            return hash.HexDigest;
        }

        private static string BuildAggregateSignature(IEnumerable<ScenarioRun> runs,
            IEnumerable<SimulationExpectedFailureRow> failures)
        {
            StableHash hash = new();
            hash.Append(SimulationSchemaId);
            hash.Append(SimulationSuiteSchemaId);
            hash.Append(SimulationVersion);
            foreach (ScenarioRun run in runs.OrderBy(run => run.Result.scenarioId, StringComparer.Ordinal))
            {
                hash.Append(run.Result.scenarioId);
                hash.Append(run.Result.canonicalSignature);
            }
            foreach (SimulationExpectedFailureRow failure in failures.OrderBy(row => row.scenarioId,
                StringComparer.Ordinal))
            {
                hash.Append(failure.scenarioId); hash.Append(failure.expectedCode);
                hash.Append(failure.actualCode); hash.Append(failure.snapshotIsNull ? "1" : "0");
                hash.Append(failure.isPass ? "1" : "0");
            }
            return hash.HexDigest;
        }

        private static double MaxZ(IEnumerable<SimulationDistributionRow> rows)
        {
            double[] values = rows.Where(row => !double.IsInfinity(row.zScore) && !double.IsNaN(row.zScore))
                .Select(row => row.zScore).ToArray();
            return values.Length == 0 ? 0d : values.Max();
        }

        private static string FormatDouble(double value) =>
            double.IsPositiveInfinity(value) ? "Infinity" : value.ToString("0.000000", CultureInfo.InvariantCulture);
        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static int CsvRows(string path) => File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;
        private static string Compact(string value)
        {
            string safe = (value ?? string.Empty).Replace("\r", string.Empty).Replace("\n", " / ");
            return safe.Length <= 128 ? safe : safe.Substring(0, 128);
        }
        private static int Count(string source, string token)
        {
            int count = 0, index = 0;
            while (!string.IsNullOrEmpty(token)
                && (index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            { count++; index += token.Length; }
            return count;
        }

        [Flags]
        private enum InputVariant { None = 0, Candidate = 1, Rarity = 2, Build = 4, Affix = 8 }
        private enum ScenarioKind
        {
            EarlyWhite, CandidateEqual, CandidateWeighted, RarityRich, Mutex, AllowDuplicate,
            BuildExactGreen, BuildExactBlue, BuildExactPurple, BuildExactOrange
        }

        private static bool IsBuildExactScenario(ScenarioKind kind) =>
            kind == ScenarioKind.BuildExactGreen || kind == ScenarioKind.BuildExactBlue
            || kind == ScenarioKind.BuildExactPurple || kind == ScenarioKind.BuildExactOrange;

        private static ItemInstanceRarity ExactRarity(ScenarioKind kind)
        {
            switch (kind)
            {
                case ScenarioKind.BuildExactGreen: return ItemInstanceRarity.Green;
                case ScenarioKind.BuildExactBlue: return ItemInstanceRarity.Blue;
                case ScenarioKind.BuildExactPurple: return ItemInstanceRarity.Purple;
                case ScenarioKind.BuildExactOrange: return ItemInstanceRarity.Orange;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind,
                    "Scenario is not an exact Build Profile fixture.");
            }
        }

        private sealed class ScenarioRun
        {
            public ScenarioRun(SimulationResult result, ScenarioAccumulator accumulator, string outcomeSignature,
                IEnumerable<string> outcomeRows)
            { Result = result; Accumulator = accumulator; OutcomeSignature = outcomeSignature ?? string.Empty;
                OutcomeRows = Array.AsReadOnly((outcomeRows ?? Array.Empty<string>()).ToArray()); }
            public SimulationResult Result { get; }
            public ScenarioAccumulator Accumulator { get; }
            public string OutcomeSignature { get; }
            public IReadOnlyList<string> OutcomeRows { get; }
        }

        private sealed class ScenarioAccumulator
        {
            private readonly Dictionary<string, Dictionary<string, int>> buckets =
                new(StringComparer.Ordinal);
            public ScenarioAccumulator(string scenarioId) { ScenarioId = scenarioId; }
            public string ScenarioId { get; }
            public int TotalSamples, SuccessCount, FailureCount, RangeChecks, MutexChecks, RepeatChecks,
                CoreChecks, BuildChecks, BuildExactMatchCount, BuildExactMismatchCount, I031LeakCount,
                LegalDuplicateCount, DuplicateDifferentValueCount;
            public bool RangeMinimumObserved, RangeMaximumObserved, MutexAffixASeen, MutexAffixBSeen;
            public readonly HashSet<string> ObservedBaseIds = new(StringComparer.Ordinal);
            public readonly HashSet<long> FixedAffixValues = new();
            public readonly Dictionary<string, EndpointBucket> EndpointBuckets = new(StringComparer.Ordinal);
            public readonly Dictionary<string, HashSet<long>> FixedValueBuckets = new(StringComparer.Ordinal);
            public readonly Dictionary<string, Dictionary<long, ulong>> FixedValueSeedBuckets =
                new(StringComparer.Ordinal);
            public readonly List<SimulationInvariantViolation> Violations = new();

            public void Increment(string dimension, string bucket)
            {
                if (!buckets.TryGetValue(dimension, out Dictionary<string, int> values))
                { values = new Dictionary<string, int>(StringComparer.Ordinal); buckets.Add(dimension, values); }
                values[bucket] = values.TryGetValue(bucket, out int count) ? count + 1 : 1;
            }
            public int BucketCount(string dimension, string bucket) =>
                buckets.TryGetValue(dimension, out Dictionary<string, int> values)
                && values.TryGetValue(bucket, out int count) ? count : 0;
            public void ObserveEndpoint(string valueKind, string profileId, string rarity,
                long minUnits, long maxUnits, long value, bool stepAligned)
            {
                string key = EndpointBucket.BuildStableKey(valueKind, profileId, rarity);
                if (!EndpointBuckets.TryGetValue(key, out EndpointBucket bucket))
                {
                    bucket = new EndpointBucket(valueKind, profileId, rarity, minUnits, maxUnits);
                    EndpointBuckets.Add(key, bucket);
                }
                bucket.Observe(value, stepAligned);
            }
            public void ObserveFixedValue(string affixId, string profileId, string rarity, long value,
                ulong rootSeed)
            {
                string key = affixId + "|" + profileId + "|" + rarity;
                if (!FixedValueBuckets.TryGetValue(key, out HashSet<long> values))
                {
                    values = new HashSet<long>();
                    FixedValueBuckets.Add(key, values);
                }
                values.Add(value);
                if (!FixedValueSeedBuckets.TryGetValue(key, out Dictionary<long, ulong> seeds))
                {
                    seeds = new Dictionary<long, ulong>();
                    FixedValueSeedBuckets.Add(key, seeds);
                }
                if (!seeds.ContainsKey(value)) seeds.Add(value, rootSeed);
                FixedAffixValues.Add(value);
            }
            public void AddViolation(int sampleIndex, ulong rootSeed, ItemDropGenerationRequest request,
                ItemDropGenerationSnapshot snapshot, string code, string message)
            {
                Violations.Add(new SimulationInvariantViolation(ScenarioId, sampleIndex, rootSeed,
                    request?.dropRequestId ?? string.Empty, request?.itemInstanceId ?? string.Empty,
                    snapshot?.selectedBaseItemId ?? string.Empty,
                    snapshot == null ? string.Empty : snapshot.selectedRarity.ToStableKey(), code, message));
            }
            public void AddSyntheticViolation(string code, string message) =>
                Violations.Add(new SimulationInvariantViolation(ScenarioId, -1, 0UL, string.Empty,
                    string.Empty, string.Empty, string.Empty, code, message));
        }

        private sealed class EndpointBucket
        {
            public EndpointBucket(string valueKind, string profileId, string rarity,
                long minUnits, long maxUnits)
            {
                this.valueKind = valueKind ?? string.Empty;
                this.profileId = profileId ?? string.Empty;
                this.rarity = rarity ?? string.Empty;
                this.minUnits = minUnits;
                this.maxUnits = maxUnits;
            }

            public string valueKind { get; }
            public string profileId { get; }
            public string rarity { get; }
            public long minUnits { get; }
            public long maxUnits { get; }
            public bool minObserved { get; private set; }
            public bool maxObserved { get; private set; }
            public int stepViolationCount { get; private set; }
            public int sampleCount { get; private set; }
            public string StableKey => BuildStableKey(valueKind, profileId, rarity);
            public bool isCovered => minObserved && maxObserved && stepViolationCount == 0 && sampleCount > 0;

            public void Observe(long value, bool stepAligned)
            {
                sampleCount++;
                minObserved |= value == minUnits;
                maxObserved |= value == maxUnits;
                if (!stepAligned) stepViolationCount++;
            }

            public void Merge(EndpointBucket other)
            {
                if (other == null) return;
                sampleCount += other.sampleCount;
                minObserved |= other.minObserved;
                maxObserved |= other.maxObserved;
                stepViolationCount += other.stepViolationCount;
            }

            public static string BuildStableKey(string valueKind, string profileId, string rarity) =>
                (valueKind ?? string.Empty) + "|" + (profileId ?? string.Empty) + "|" + (rarity ?? string.Empty);
        }

        private sealed class DistributionDefinition
        {
            public DistributionDefinition(string dimensionKey, Dictionary<string, long> weights)
            { DimensionKey = dimensionKey; Weights = weights; }
            public string DimensionKey { get; }
            public Dictionary<string, long> Weights { get; }
        }

        private sealed class StableHash
        {
            private const ulong Offset = 14695981039346656037UL;
            private const ulong Prime = 1099511628211UL;
            private ulong value = Offset;
            public void Append(string text)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
                AppendUInt32(unchecked((uint)bytes.Length));
                foreach (byte item in bytes) { value = unchecked((value ^ item) * Prime); }
            }
            private void AppendUInt32(uint number)
            {
                for (int shift = 0; shift < 32; shift += 8)
                    value = unchecked((value ^ (byte)(number >> shift)) * Prime);
            }
            public string HexDigest => value.ToString("X16", CultureInfo.InvariantCulture);
        }

        private sealed class Fixture
        {
            public const string FixedAffixId = "qa_fixed_guard";
            public const string RandomAffixA = "qa_random_a";
            public const string RandomAffixB = "qa_random_b";
            public const string RandomAffixC = "qa_random_c";
            public const string ZeroWeightAffix = "qa_random_zero";
            public const string MutexAffixA = "qa_mutex_a";
            public const string MutexAffixB = "qa_mutex_b";

            public ItemGenerationFoundationSnapshot Foundation { get; private set; }
            public ItemStatRangeSchemaSnapshot Stats { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot RichAffixes { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot MutexAffixes { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot AllowDuplicateAffixes { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot CapacityExhaustionAffixes { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot MutexExhaustionAffixes { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot ZeroWeightAffixes { get; private set; }
            public ItemCorePotentialAndBuildEligibilitySchemaSnapshot Core { get; private set; }
            public ItemDropCandidatePoolSnapshot CandidateEqual { get; private set; }
            public ItemDropCandidatePoolSnapshot CandidateWeighted { get; private set; }
            public ItemDropCandidatePoolSnapshot CandidateOne { get; private set; }

            public static Fixture Create()
            {
                Fixture fixture = new();
                fixture.Foundation = ItemRarityInstanceFoundation.Create();
                fixture.Stats = fixture.CreateStats();
                fixture.RichAffixes = fixture.CreateRichAffixes(InputVariant.None);
                fixture.MutexAffixes = fixture.CreateMutexAffixes();
                fixture.AllowDuplicateAffixes = fixture.CreateAllowDuplicateAffixes();
                fixture.CapacityExhaustionAffixes = fixture.CreateCapacityExhaustionAffixes();
                fixture.MutexExhaustionAffixes = fixture.CreateMutexExhaustionAffixes();
                fixture.ZeroWeightAffixes = fixture.CreateZeroWeightAffixes();
                fixture.Core = fixture.CreateCore();
                fixture.CandidateEqual = fixture.CandidatePool("qa_equal_30",
                    Enumerable.Range(1, 30).Select(index =>
                        new ItemDropCandidateEntrySnapshot($"I{index:000}", 1)).ToArray());
                fixture.CandidateWeighted = fixture.CandidatePool("qa_weighted_3",
                    new ItemDropCandidateEntrySnapshot("I001", 1),
                    new ItemDropCandidateEntrySnapshot("I002", 3),
                    new ItemDropCandidateEntrySnapshot("I003", 6));
                fixture.CandidateOne = fixture.CandidatePool("qa_one",
                    new ItemDropCandidateEntrySnapshot("I001", 1));
                return fixture;
            }

            public ItemDropGenerationRequest Request(string scenarioId, int sampleIndex, ulong rootSeed,
                InputVariant variant, ScenarioKind kind = ScenarioKind.CandidateEqual,
                int generationVersion = 1, string status = null,
                ItemDropCandidatePoolSnapshot candidateOverride = null,
                ItemDropRarityWeightProfileSnapshot rarityOverride = null,
                bool forceRarityNull = false,
                ItemAffixPoolAndRangeSchemaSnapshot affixOverride = null,
                IEnumerable<ItemBuildQualificationRollProfile> buildOverride = null)
            {
                int stage = kind == ScenarioKind.EarlyWhite ? sampleIndex % 10 + 1
                    : kind == ScenarioKind.CandidateEqual || kind == ScenarioKind.CandidateWeighted ? 1 : 11;
                ItemDropCandidatePoolSnapshot candidate = candidateOverride ?? (kind == ScenarioKind.CandidateWeighted
                    ? CandidateWeighted : kind == ScenarioKind.CandidateEqual || kind == ScenarioKind.EarlyWhite
                        ? CandidateEqual : CandidateOne);
                if ((variant & InputVariant.Candidate) != 0)
                    candidate = CandidatePool(candidate.poolId, candidate.Candidates.Reverse().ToArray());

                ItemDropRarityWeightProfileSnapshot rarity = null;
                if (stage > 10 && !forceRarityNull)
                {
                    rarity = rarityOverride ?? (IsBuildExactScenario(kind)
                        ? SingleRarity(ExactRarity(kind))
                        : kind == ScenarioKind.Mutex || kind == ScenarioKind.AllowDuplicate
                            ? SingleRarity(ItemInstanceRarity.Green) : RarityProfile(variant));
                }

                ItemAffixPoolAndRangeSchemaSnapshot affixes = affixOverride
                    ?? (kind == ScenarioKind.Mutex ? MutexAffixes
                        : kind == ScenarioKind.AllowDuplicate ? AllowDuplicateAffixes
                        : (variant & InputVariant.Affix) != 0 ? CreateRichAffixes(InputVariant.Affix) : RichAffixes);
                IEnumerable<ItemBuildQualificationRollProfile> builds = buildOverride
                    ?? (IsBuildExactScenario(kind) ? ExactBuildProfiles() : BuildProfiles(variant));
                ItemDropSourceContextSnapshot source = Source(stage, candidate.poolId,
                    stage > 10 ? rarity?.profileId ?? "qa_rarity_11_plus" : string.Empty);
                string normalizedScenario = new string((scenarioId ?? "scenario")
                    .Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray());
                string suffix = sampleIndex.ToString("D6", CultureInfo.InvariantCulture);
                return new ItemDropGenerationRequest("SIM-" + normalizedScenario + "-" + suffix,
                    "ITEM-" + normalizedScenario + "-" + suffix,
                    unchecked((long)rootSeed), generationVersion,
                    status ?? ItemGenerationDataStatus.QaFixtureCanonical,
                    Foundation, source, candidate, rarity, Stats, affixes, Core, builds);
            }

            public ItemDropSourceContextSnapshot Source(int stage, string poolId, string rarityProfileId) =>
                new("CTX-QA-SIM", "qa_simulation", stage, poolId, rarityProfileId,
                    ItemGenerationDataStatus.QaFixtureOnly);

            public ItemDropCandidatePoolSnapshot CandidatePool(string id,
                params ItemDropCandidateEntrySnapshot[] entries) =>
                new(id, ItemGenerationDataStatus.QaFixtureOnly,
                    entries ?? Array.Empty<ItemDropCandidateEntrySnapshot>());

            public ItemDropRarityWeightProfileSnapshot RarityProfile(InputVariant variant)
            {
                ItemDropRarityWeightEntrySnapshot[] entries =
                {
                    new(ItemInstanceRarity.White, 1), new(ItemInstanceRarity.Green, 2),
                    new(ItemInstanceRarity.Blue, 3), new(ItemInstanceRarity.Purple, 4),
                    new(ItemInstanceRarity.Orange, 5)
                };
                if ((variant & InputVariant.Rarity) != 0) Array.Reverse(entries);
                return new ItemDropRarityWeightProfileSnapshot("qa_rarity_11_plus",
                    ItemGenerationDataStatus.QaFixtureOnly, entries);
            }

            public ItemDropRarityWeightProfileSnapshot SingleRarity(ItemInstanceRarity rarity) =>
                new("qa_rarity_11_plus", ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemDropRarityWeightEntrySnapshot(rarity, 1) });

            public ItemBuildQualificationRollProfile BuildProfile(ItemInstanceRarity rarity,
                InputVariant variant)
            {
                ItemBuildQualificationRollEntry[] entries =
                {
                    new(ItemBuildQualification.None, 1),
                    new(ItemBuildQualification.FaMenOnly, 2),
                    new(ItemBuildQualification.QiLeiOnly, 3),
                    new(ItemBuildQualification.Dual, 4)
                };
                if ((variant & InputVariant.Build) != 0) Array.Reverse(entries);
                return new ItemBuildQualificationRollProfile("qa_build_" + rarity.ToStableKey(), rarity,
                    ItemGenerationDataStatus.QaFixtureOnly, entries);
            }

            private IEnumerable<ItemBuildQualificationRollProfile> BuildProfiles(InputVariant variant)
            {
                ItemBuildQualificationRollProfile[] profiles =
                {
                    BuildProfile(ItemInstanceRarity.Green, variant),
                    BuildProfile(ItemInstanceRarity.Blue, variant),
                    BuildProfile(ItemInstanceRarity.Purple, variant),
                    BuildProfile(ItemInstanceRarity.Orange, variant)
                };
                return (variant & InputVariant.Build) != 0 ? profiles.Reverse() : profiles;
            }

            public string ExactBuildProfileId(ItemInstanceRarity rarity) =>
                "qa_exact_build_" + rarity.ToStableKey();

            public ItemBuildQualification ExactBuildQualification(ItemInstanceRarity rarity)
            {
                switch (rarity)
                {
                    case ItemInstanceRarity.Green: return ItemBuildQualification.FaMenOnly;
                    case ItemInstanceRarity.Blue: return ItemBuildQualification.QiLeiOnly;
                    case ItemInstanceRarity.Purple: return ItemBuildQualification.Dual;
                    case ItemInstanceRarity.Orange: return ItemBuildQualification.None;
                    default: throw new ArgumentOutOfRangeException(nameof(rarity), rarity,
                        "Exact Build Profile fixtures only cover green through orange.");
                }
            }

            public ItemBuildQualificationRollProfile ExactBuildProfile(ItemInstanceRarity rarity) =>
                new(ExactBuildProfileId(rarity), rarity, ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemBuildQualificationRollEntry(ExactBuildQualification(rarity), 1) });

            public ItemBuildQualificationRollProfile[] ExactBuildProfiles() =>
                new[]
                {
                    ExactBuildProfile(ItemInstanceRarity.Green),
                    ExactBuildProfile(ItemInstanceRarity.Blue),
                    ExactBuildProfile(ItemInstanceRarity.Purple),
                    ExactBuildProfile(ItemInstanceRarity.Orange)
                };

            public ItemBuildQualificationRollProfile MislabelledExactBuildProfile(
                ItemInstanceRarity declaredRarity, ItemInstanceRarity fixtureSourceRarity) =>
                new(ExactBuildProfileId(fixtureSourceRarity), declaredRarity,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemBuildQualificationRollEntry(
                        ExactBuildQualification(fixtureSourceRarity), 1) });

            public ItemBuildQualificationRollProfile[] SwappedExactBuildProfiles() =>
                new[]
                {
                    MislabelledExactBuildProfile(ItemInstanceRarity.Green, ItemInstanceRarity.Blue),
                    MislabelledExactBuildProfile(ItemInstanceRarity.Blue, ItemInstanceRarity.Purple),
                    MislabelledExactBuildProfile(ItemInstanceRarity.Purple, ItemInstanceRarity.Orange),
                    MislabelledExactBuildProfile(ItemInstanceRarity.Orange, ItemInstanceRarity.Green)
                };

            private ItemStatRangeSchemaSnapshot CreateStats()
            {
                ItemStatDefinitionSnapshot power = new("qa_power", "QA Power", "flat",
                    ItemStatDirection.HigherIsBetter, 1, 2, ItemStatRoundingMode.Nearest,
                    ItemStatDataMaturity.SEED_DATA);
                ItemStatDefinitionSnapshot tempo = new("qa_tempo", "QA Tempo", "turn",
                    ItemStatDirection.LowerIsBetter, 0, 2, ItemStatRoundingMode.Floor,
                    ItemStatDataMaturity.SEED_DATA);
                List<ItemStatRangeProfileSnapshot> profiles = new();
                for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
                {
                    profiles.Add(new ItemStatRangeProfileSnapshot($"I{itemIndex:000}", power.statId,
                        ItemStatDataMaturity.SEED_DATA, new ItemNumericRangeSnapshot(2, 22),
                        HigherRarityStatRanges()));
                    if (itemIndex == 1)
                    {
                        profiles.Add(new ItemStatRangeProfileSnapshot("I001", tempo.statId,
                            ItemStatDataMaturity.SEED_DATA, new ItemNumericRangeSnapshot(2, 22),
                            LowerRarityStatRanges()));
                    }
                }
                return ItemStatRangeSchema.Create(new[] { power, tempo }, profiles);
            }

            private static ItemRarityStatRangeSnapshot[] HigherRarityStatRanges() =>
                ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                    new ItemRarityStatRangeSnapshot(rarity.rarity, 2 + index * 4, 6 + index * 4)).ToArray();

            private static ItemRarityStatRangeSnapshot[] LowerRarityStatRanges() =>
                ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                    new ItemRarityStatRangeSnapshot(rarity.rarity, 18 - index * 4, 22 - index * 4)).ToArray();

            private ItemCorePotentialAndBuildEligibilitySchemaSnapshot CreateCore()
            {
                List<ItemCorePotentialProfileSnapshot> profiles = new();
                for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
                {
                    string baseId = $"I{itemIndex:000}";
                    foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                    {
                        if (itemIndex != 1 && rarity.rarity != ItemInstanceRarity.White) continue;
                        List<ItemCorePotentialEffectSnapshot> effects = new()
                        {
                            new ItemCorePotentialEffectSnapshot("qa_core_" + baseId.ToLowerInvariant()
                                + "_" + rarity.stableKey + "_visible", ItemCorePotentialEffectKind.Standard, true),
                            new ItemCorePotentialEffectSnapshot("qa_core_" + baseId.ToLowerInvariant()
                                + "_" + rarity.stableKey + "_eligible", ItemCorePotentialEffectKind.Standard, false)
                        };
                        if (rarity.rarity == ItemInstanceRarity.Orange)
                            effects.Add(new ItemCorePotentialEffectSnapshot("qa_core_i001_orange_ultimate",
                                ItemCorePotentialEffectKind.Ultimate, false));
                        profiles.Add(new ItemCorePotentialProfileSnapshot("qa_core_" + baseId.ToLowerInvariant()
                            + "_" + rarity.stableKey, baseId, rarity.rarity,
                            ItemCorePotentialResolutionStatus.Defined,
                            ItemGenerationDataStatus.QaFixtureOnly, effects));
                    }
                }
                return ItemCorePotentialAndBuildEligibilitySchema.Create(
                    ItemCorePotentialAndBuildEligibilityCatalog.DefaultCoreRarityPolicies,
                    profiles, ItemCorePotentialAndBuildEligibilityCatalog.DefaultBuildRarityPolicies);
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateRichAffixes(InputVariant variant)
            {
                ItemAffixDefinitionSnapshot[] definitions = Definitions(FixedAffixId, RandomAffixA,
                    RandomAffixB, RandomAffixC, ZeroWeightAffix);
                ItemAffixValueProfileSnapshot[] values = definitions.Select(ValueProfile).ToArray();
                ItemAffixSlotPolicySnapshot richSlots = new("qa_rich_slots",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed),
                        new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random) });
                ItemAffixSlotPolicySnapshot emptySlots = new("qa_empty_slots",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    Array.Empty<ItemAffixSlotSnapshot>());
                ItemAffixPoolEntrySnapshot[] entries =
                {
                    Entry(RandomAffixA, 1), Entry(RandomAffixB, 3), Entry(RandomAffixC, 6)
                };
                if ((variant & InputVariant.Affix) != 0) Array.Reverse(entries);
                ItemRandomAffixPoolSnapshot pool = new("qa_rich_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                    entries, Array.Empty<ItemAffixMutexGroupSnapshot>());
                List<ItemAffixGenerationProfileSnapshot> profiles = new()
                {
                    new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Defined,
                        ItemGenerationDataStatus.QaFixtureOnly, richSlots.slotPolicyId,
                        new[] { new ItemFixedAffixBindingSnapshot("fixed_01", FixedAffixId,
                            FixedAffixId + "_value") }, pool.poolId)
                };
                for (int index = 2; index <= 30; index++)
                {
                    profiles.Add(new ItemAffixGenerationProfileSnapshot($"I{index:000}",
                        ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                        emptySlots.slotPolicyId, Array.Empty<ItemFixedAffixBindingSnapshot>(), string.Empty));
                }
                IEnumerable<ItemAffixDefinitionSnapshot> orderedDefinitions = (variant & InputVariant.Affix) != 0
                    ? definitions.Reverse() : definitions;
                IEnumerable<ItemAffixValueProfileSnapshot> orderedValues = (variant & InputVariant.Affix) != 0
                    ? values.Reverse() : values;
                IEnumerable<ItemAffixGenerationProfileSnapshot> orderedProfiles = (variant & InputVariant.Affix) != 0
                    ? profiles.AsEnumerable().Reverse() : profiles;
                return ItemAffixPoolAndRangeSchema.Create(orderedDefinitions, orderedValues,
                    new[] { richSlots, emptySlots }, new[] { pool }, orderedProfiles);
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateMutexAffixes()
            {
                string support = "qa_mutex_support";
                ItemAffixDefinitionSnapshot[] definitions = Definitions(MutexAffixA, MutexAffixB, support);
                ItemAffixSlotPolicySnapshot slots = RandomSlots("qa_mutex_slots", 2);
                ItemRandomAffixPoolSnapshot pool = new("qa_mutex_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                    new[] { Entry(MutexAffixA, 1), Entry(MutexAffixB, 1), Entry(support, 1) },
                    new[] { new ItemAffixMutexGroupSnapshot("qa_mutex_ab",
                        new[] { MutexAffixA, MutexAffixB }) });
                return SingleItemAffixSchema(definitions, definitions.Select(ValueProfile), slots, pool,
                    Array.Empty<ItemFixedAffixBindingSnapshot>());
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateAllowDuplicateAffixes()
            {
                ItemAffixDefinitionSnapshot[] definitions = Definitions(RandomAffixA);
                ItemAffixSlotPolicySnapshot slots = RandomSlots("qa_duplicate_slots", 2);
                ItemRandomAffixPoolSnapshot pool = new("qa_duplicate_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, ItemAffixRepeatPolicy.AllowDuplicateAffix,
                    new[] { Entry(RandomAffixA, 1) }, Array.Empty<ItemAffixMutexGroupSnapshot>());
                return SingleItemAffixSchema(definitions, definitions.Select(ValueProfile), slots, pool,
                    Array.Empty<ItemFixedAffixBindingSnapshot>());
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateCapacityExhaustionAffixes()
            {
                ItemAffixDefinitionSnapshot[] definitions = Definitions(RandomAffixA);
                ItemAffixSlotPolicySnapshot slots = RandomSlots("qa_capacity_slots", 2);
                ItemRandomAffixPoolSnapshot pool = new("qa_capacity_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                    new[] { Entry(RandomAffixA, 1) }, Array.Empty<ItemAffixMutexGroupSnapshot>());
                return SingleItemAffixSchema(definitions, definitions.Select(ValueProfile), slots, pool,
                    Array.Empty<ItemFixedAffixBindingSnapshot>());
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateMutexExhaustionAffixes()
            {
                ItemAffixDefinitionSnapshot[] definitions = Definitions(MutexAffixA, MutexAffixB);
                ItemAffixSlotPolicySnapshot slots = new("qa_mutex_exhaustion_slots",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed),
                        new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random) });
                ItemRandomAffixPoolSnapshot pool = new("qa_mutex_exhaustion_pool",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    ItemAffixRepeatPolicy.DisallowDuplicateAffix, new[] { Entry(MutexAffixB, 1) },
                    new[] { new ItemAffixMutexGroupSnapshot("qa_mutex_exhaustion",
                        new[] { MutexAffixA, MutexAffixB }) });
                return SingleItemAffixSchema(definitions, definitions.Select(ValueProfile), slots, pool,
                    new[] { new ItemFixedAffixBindingSnapshot("fixed_01", MutexAffixA,
                        MutexAffixA + "_value") });
            }

            private ItemAffixPoolAndRangeSchemaSnapshot CreateZeroWeightAffixes()
            {
                ItemAffixDefinitionSnapshot[] definitions = Definitions(RandomAffixA, ZeroWeightAffix);
                ItemAffixSlotPolicySnapshot slots = RandomSlots("qa_zero_weight_slots", 1);
                ItemRandomAffixPoolSnapshot pool = new("qa_zero_weight_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, ItemAffixRepeatPolicy.DisallowDuplicateAffix,
                    new[] { Entry(RandomAffixA, 1), Entry(ZeroWeightAffix, 0) },
                    Array.Empty<ItemAffixMutexGroupSnapshot>());
                return SingleItemAffixSchema(definitions, definitions.Select(ValueProfile), slots, pool,
                    Array.Empty<ItemFixedAffixBindingSnapshot>());
            }

            private static ItemAffixPoolAndRangeSchemaSnapshot SingleItemAffixSchema(
                IEnumerable<ItemAffixDefinitionSnapshot> definitions,
                IEnumerable<ItemAffixValueProfileSnapshot> values,
                ItemAffixSlotPolicySnapshot slots, ItemRandomAffixPoolSnapshot pool,
                IEnumerable<ItemFixedAffixBindingSnapshot> fixedBindings)
            {
                ItemAffixGenerationProfileSnapshot generation = new("I001",
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    slots.slotPolicyId, fixedBindings, pool.poolId);
                return ItemAffixPoolAndRangeSchema.Create(definitions, values,
                    new[] { slots }, new[] { pool }, new[] { generation });
            }

            private static ItemAffixSlotPolicySnapshot RandomSlots(string id, int count) =>
                new(id, ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    Enumerable.Range(1, count).Select(index =>
                        new ItemAffixSlotSnapshot("random_" + index.ToString("00", CultureInfo.InvariantCulture),
                            ItemAffixSlotKind.Random)));

            private static ItemAffixDefinitionSnapshot[] Definitions(params string[] ids) =>
                ids.Select(id => new ItemAffixDefinitionSnapshot(id, id, "flat",
                    ItemStatDirection.HigherIsBetter, 1, 2, ItemStatRoundingMode.Nearest,
                    ItemGenerationDataStatus.QaFixtureOnly)).ToArray();

            private static ItemAffixValueProfileSnapshot ValueProfile(ItemAffixDefinitionSnapshot definition) =>
                new(definition.affixId + "_value", definition.affixId,
                    ItemAffixResolutionStatus.Defined, ItemGenerationDataStatus.QaFixtureOnly,
                    new ItemAffixNumericRangeSnapshot(2, 22),
                    ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                        new ItemAffixRarityValueRangeSnapshot(rarity.rarity, 2 + index * 4, 6 + index * 4)),
                    definition.direction, definition.decimalPlaces, definition.stepUnits, definition.roundingMode);

            private static ItemAffixPoolEntrySnapshot Entry(string affixId, long weight) =>
                new(affixId, affixId + "_value", ItemAffixWeightResolutionStatus.Defined, weight);
        }

        private sealed class Verification
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public readonly List<BuildExactResult> BuildExactResults = new();
            public int FoundationCount, StatCount, CoreCount, AffixCount, RollCount,
                DropSpecCount, DropDeterminismCount, ExecutedSampleCount,
                EndpointBucketCount, EndpointCoveredCount, EndpointMissingCount;
        }

        private readonly struct BuildExactResult
        {
            public BuildExactResult(string rarity, string profileId,
                ItemBuildQualification qualification, int matchCount, int mismatchCount)
            {
                this.rarity = rarity;
                this.profileId = profileId;
                this.qualification = qualification;
                this.matchCount = matchCount;
                this.mismatchCount = mismatchCount;
            }
            public readonly string rarity, profileId;
            public readonly ItemBuildQualification qualification;
            public readonly int matchCount, mismatchCount;
        }

        private readonly struct SpecRow
        {
            public SpecRow(string caseId, string category, string expected, string actual,
                string code, string result)
            { this.caseId = caseId; this.category = category; this.expected = expected; this.actual = actual; this.code = code; this.result = result; }
            public readonly string caseId, category, expected, actual, code, result;
        }

        private readonly struct DeterminismRow
        {
            public DeterminismRow(string caseId, ulong firstRootSeed, string domain,
                string expectedSignature, string actualSignature, int repeatCount, string result)
            { this.caseId = caseId; this.firstRootSeed = firstRootSeed; this.domain = domain; this.expectedSignature = expectedSignature; this.actualSignature = actualSignature; this.repeatCount = repeatCount; this.result = result; }
            public readonly string caseId, domain, expectedSignature, actualSignature, result;
            public readonly ulong firstRootSeed;
            public readonly int repeatCount;
        }

        private readonly struct LeakRow
        {
            public LeakRow(string category, int count) { this.category = category; this.count = count; }
            public readonly string category; public readonly int count;
        }
        private readonly struct LeakDefinition
        {
            public LeakDefinition(string category, string token) { this.category = category; this.token = token; }
            public readonly string category, token;
        }
        private readonly struct Regression
        {
            public Regression(string name, string path, string marker) { this.name = name; this.path = path; this.marker = marker; }
            public readonly string name, path, marker;
        }
    }
}
#endif
