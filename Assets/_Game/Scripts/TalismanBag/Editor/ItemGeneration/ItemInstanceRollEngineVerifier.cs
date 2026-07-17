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
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemInstanceRollEngineVerifier
    {
        private const string PackageName = "V0.4-ItemInstanceRollEngine01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemInstanceRollEngineReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemInstanceRollEngineSpec.csv";
        private const string DeterminismPath = "Docs/V0.4/Reports/ItemInstanceRollEngineDeterminism.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemInstanceRollEngineLeakCheckReport.md";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling";
        private const string PassMarker = "ITEM_INSTANCE_ROLL_ENGINE01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemInstanceRollEngine01/[QA Only] Run Engine")]
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
            Verification result = new();
            List<SpecRow> specs = new();
            List<DeterminismRow> determinism = new();
            List<LeakRow> leaks = new();
            ItemInstanceRollResult sample = null;
            try
            {
                Fixture fixture = Fixture.Create();
                sample = RunGoldenAndDeterminism(fixture, specs, determinism, result);
                RunGenerationCases(fixture, specs, result);
                RunFailureCases(fixture, specs, result);
                RunReadOnlyAndIsolationCases(fixture, specs, result);
                RunLeakChecks(leaks, result);
                RunHistoricalRegressions(result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Verifier threw an unhandled exception: " + exception);
            }

            bool passed = result.Errors.Count == 0
                && specs.Count >= 72
                && specs.All(row => row.result == "PASS")
                && determinism.All(row => row.result == "PASS")
                && leaks.All(row => row.count == 0);
            WriteReports(sample, specs, determinism, leaks, result, passed);
            if (passed)
            {
                Debug.Log(PassMarker + $" specs={specs.Count} determinism={determinism.Count}");
            }
            else
            {
                Debug.LogError("ITEM_INSTANCE_ROLL_ENGINE01_FAIL\n" + string.Join("\n", result.Errors));
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static ItemInstanceRollResult RunGoldenAndDeterminism(
            Fixture fixture,
            List<SpecRow> specs,
            List<DeterminismRow> determinism,
            Verification result)
        {
            Check(specs, result, "fnv-empty", "golden", "CBF29CE484222325",
                Hex(DeterministicItemRandom.Fnv1A64Utf8(string.Empty)));
            Check(specs, result, "fnv-a", "golden", "AF63DC4C8601EC8C",
                Hex(DeterministicItemRandom.Fnv1A64Utf8("a")));
            Check(specs, result, "fnv-hello", "golden", "A430D84680AABD0B",
                Hex(DeterministicItemRandom.Fnv1A64Utf8("hello")));
            Check(specs, result, "splitmix-state-0", "golden", "E220A8397B1DCDAF",
                Hex(DeterministicItemRandom.SplitMix64(0UL)));
            DeterministicItemRandom.Stream vectorStream = new(0UL);
            Check(specs, result, "splitmix-stream-1", "golden", "E220A8397B1DCDAF", Hex(vectorStream.NextUInt64()));
            Check(specs, result, "splitmix-stream-2", "golden", "6E789E6AA1B965F4", Hex(vectorStream.NextUInt64()));
            Check(specs, result, "splitmix-stream-3", "golden", "06C45D188009454F", Hex(vectorStream.NextUInt64()));
            Check(specs, result, "splitmix-wraparound-vector", "golden", "E4D971771B652C20",
                Hex(DeterministicItemRandom.SplitMix64(ulong.MaxValue)));

            ItemInstanceIdentitySnapshot identity = fixture.Identity(ItemInstanceRarity.Green, 42L, "QA-I001-green");
            bool domainOk = DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed,
                identity.generationVersion, identity.itemInstanceId, identity.baseItemId,
                identity.rarity, new[] { "stat", "qa_power" }, out ulong domainSeed);
            Check(specs, result, "domain-hash-vector", "golden", "54AE2882BEECC218",
                domainOk ? Hex(domainSeed) : "INVALID");
            Check(specs, result, "segmented-domain-golden", "golden", "54AE2882BEECC218",
                domainOk ? Hex(domainSeed) : "INVALID");

            bool collisionAOk = DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed,
                identity.generationVersion, identity.itemInstanceId, identity.baseItemId,
                identity.rarity, new[] { "fixed-affix", "a/b", "c", "qa_value", "value" },
                out ulong collisionASeed);
            bool collisionBOk = DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed,
                identity.generationVersion, identity.itemInstanceId, identity.baseItemId,
                identity.rarity, new[] { "fixed-affix", "a", "b/c", "qa_value", "value" },
                out ulong collisionBSeed);
            Check(specs, result, "domain-segments-collision-separated", "domain",
                "8A5DD51B6600C50E/0A300F4D71EB3380",
                collisionAOk && collisionBOk && collisionASeed != collisionBSeed
                    ? Hex(collisionASeed) + "/" + Hex(collisionBSeed)
                    : "INVALID");

            bool nullSegmentAccepted = DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed,
                identity.generationVersion, identity.itemInstanceId, identity.baseItemId,
                identity.rarity, new string[] { "stat", null }, out _);
            CheckBool(specs, result, "domain-null-segment-invalid", "domain", !nullSegmentAccepted);
            bool emptySegmentAccepted = DeterministicItemRandom.TryComputeDomainSeed(identity.rootSeed,
                identity.generationVersion, identity.itemInstanceId, identity.baseItemId,
                identity.rarity, new[] { "stat", "" }, out _);
            CheckBool(specs, result, "domain-empty-segment-invalid", "domain", !emptySegmentAccepted);

            determinism.Add(new DeterminismRow("domain-golden", 42L, identity.itemInstanceId,
                "stat|qa_power", "54AE2882BEECC218", domainOk ? Hex(domainSeed) : "INVALID", 1,
                domainOk && Hex(domainSeed) == "54AE2882BEECC218" ? "PASS" : "FAIL"));

            ItemInstanceRollRequest request = fixture.Request(identity);
            ItemInstanceRollResult first = ItemInstanceRollEngine.Generate(request);
            string expected = first.snapshot?.BuildCanonicalSignature() ?? string.Empty;
            bool stable = first.isSuccess;
            for (int index = 0; index < 1000 && stable; index++)
            {
                ItemInstanceRollResult repeat = ItemInstanceRollEngine.Generate(request);
                stable = repeat.isSuccess
                    && string.Equals(expected, repeat.snapshot.BuildCanonicalSignature(), StringComparison.Ordinal);
            }

            CheckBool(specs, result, "same-input-1000", "determinism", stable);
            determinism.Add(new DeterminismRow("repeat-1000", identity.rootSeed, identity.itemInstanceId,
                "full-snapshot", Compact(expected), stable ? Compact(expected) : "MISMATCH", 1000,
                stable ? "PASS" : "FAIL"));

            ItemInstanceRollResult reversed = ItemInstanceRollEngine.Generate(fixture.Request(identity, reverse: true));
            CheckBool(specs, result, "input-collections-reversed", "determinism",
                reversed.isSuccess && reversed.snapshot.BuildCanonicalSignature() == expected);

            ItemInstanceRollResult differentInstance = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 42L, "QA-I001-green-B")));
            CheckBool(specs, result, "instance-domain-separated", "determinism",
                differentInstance.isSuccess
                && differentInstance.snapshot.BuildCanonicalSignature() != expected);

            HashSet<string> seedResults = new(StringComparer.Ordinal);
            for (long seed = 0; seed < 16; seed++)
            {
                ItemInstanceRollResult generated = ItemInstanceRollEngine.Generate(
                    fixture.Request(fixture.Identity(ItemInstanceRarity.Green, seed, "QA-I001-green")));
                if (generated.isSuccess)
                {
                    seedResults.Add(OutcomeKey(generated.snapshot));
                }
            }
            CheckBool(specs, result, "multiple-seeds-diverge", "determinism", seedResults.Count >= 2);

            foreach (long seed in new[] { long.MinValue, 0L, long.MaxValue })
            {
                ItemInstanceIdentitySnapshot edgeIdentity = fixture.Identity(
                    ItemInstanceRarity.Green, seed, "QA-I001-edge-" + seed.ToString(CultureInfo.InvariantCulture));
                ItemInstanceRollResult edgeA = ItemInstanceRollEngine.Generate(fixture.Request(edgeIdentity));
                ItemInstanceRollResult edgeB = ItemInstanceRollEngine.Generate(fixture.Request(edgeIdentity));
                bool edgeStable = edgeA.isSuccess && edgeB.isSuccess
                    && edgeA.snapshot.BuildCanonicalSignature() == edgeB.snapshot.BuildCanonicalSignature();
                CheckBool(specs, result, "seed-edge-" + seed.ToString(CultureInfo.InvariantCulture),
                    "determinism", edgeStable);
                determinism.Add(new DeterminismRow("seed-edge", seed, edgeIdentity.itemInstanceId,
                    "full-snapshot", edgeA.snapshot == null ? "SUCCESS" : Compact(edgeA.snapshot.BuildCanonicalSignature()),
                    edgeB.snapshot == null ? "FAIL" : Compact(edgeB.snapshot.BuildCanonicalSignature()), 2,
                    edgeStable ? "PASS" : "FAIL"));
            }

            return first;
        }

        private static void RunGenerationCases(Fixture fixture, List<SpecRow> specs, Verification result)
        {
            ItemInstanceRollResult green = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 42L, "QA-GREEN")));
            CheckBool(specs, result, "green-generation-success", "generation", green.isSuccess);
            CheckBool(specs, result, "qa-status-in-output", "isolation",
                green.snapshot?.generationDataStatus == ItemGenerationDataStatus.QaFixtureCanonical);
            CheckBool(specs, result, "algorithm-id-v1", "generation",
                green.snapshot?.generationAlgorithmId == DeterministicItemRandom.AlgorithmId);
            CheckBool(specs, result, "two-stats-generated", "stat", green.snapshot?.GeneratedStats.Count == 2);
            CheckBool(specs, result, "stat-ranges-and-steps", "stat",
                green.isSuccess && green.snapshot.GeneratedStats.All(stat => fixture.StatValueValid(stat, ItemInstanceRarity.Green)));
            CheckBool(specs, result, "fixed-affix-type-stable", "fixed-affix",
                green.isSuccess && green.snapshot.GeneratedAffixes.Any(affix => affix.slotKind == ItemAffixSlotKind.Fixed
                    && affix.affixId == "qa_fixed_guard"));
            CheckBool(specs, result, "all-affix-values-valid", "affix",
                green.isSuccess && green.snapshot.GeneratedAffixes.All(affix => fixture.AffixValueValid(affix, ItemInstanceRarity.Green)));
            string[] randomIds = green.snapshot?.GeneratedAffixes
                .Where(affix => affix.slotKind == ItemAffixSlotKind.Random)
                .Select(affix => affix.affixId).ToArray() ?? Array.Empty<string>();
            CheckBool(specs, result, "random-results-from-pool", "random-affix",
                randomIds.Length == 2 && randomIds.All(fixture.RandomAffixIds.Contains));
            CheckBool(specs, result, "disallow-duplicate-effective", "random-affix",
                randomIds.Distinct(StringComparer.Ordinal).Count() == randomIds.Length);
            CheckBool(specs, result, "mutex-effective", "random-affix",
                !(randomIds.Contains("qa_random_a") && randomIds.Contains("qa_random_b")));
            CheckBool(specs, result, "core-potential-projected", "core",
                green.snapshot?.GeneratedCorePotential?.EligibleCoreEffectIds.Count == 2
                && green.snapshot.GeneratedCorePotential.VisibleCoreEffectIds.Count == 1);
            CheckBool(specs, result, "build-result-from-profile", "build",
                green.isSuccess && fixture.BuildProfile(ItemInstanceRarity.Green).Entries
                    .Any(entry => entry.qualification == green.snapshot.buildQualification));

            foreach (ItemInstanceRarity rarity in ItemInstanceRarityCatalog.All.Select(item => item.rarity))
            {
                ItemInstanceIdentitySnapshot identity = fixture.Identity(rarity, 99L, "QA-FIVE-" + rarity.ToStableKey());
                ItemInstanceRollResult generated = ItemInstanceRollEngine.Generate(fixture.Request(identity));
                CheckBool(specs, result, "rarity-generates-" + rarity.ToStableKey(), "stat",
                    generated.isSuccess
                    && generated.snapshot.GeneratedStats.All(stat => fixture.StatValueValid(stat, rarity)));
            }

            ItemInstanceRollResult whiteA = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.White, 17L, "QA-WHITE"),
                    buildProfileOverride: null));
            ItemInstanceRollResult whiteB = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.White, 17L, "QA-WHITE"),
                    buildProfileOverride: new ItemBuildQualificationRollProfile("ignored",
                        ItemInstanceRarity.Orange, "bad", Array.Empty<ItemBuildQualificationRollEntry>())));
            CheckBool(specs, result, "white-build-none", "build",
                whiteA.isSuccess && whiteA.snapshot.buildQualification == ItemBuildQualification.None);
            CheckBool(specs, result, "white-build-branch-consumes-no-input", "build",
                whiteA.isSuccess && whiteB.isSuccess
                && whiteA.snapshot.BuildCanonicalSignature() == whiteB.snapshot.BuildCanonicalSignature());

            ItemInstanceRollResult allowDuplicate = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 22L, "QA-ALLOW"),
                    affixOverride: fixture.AffixSchema(ItemAffixRepeatPolicy.AllowDuplicateAffix)));
            CheckBool(specs, result, "allow-duplicate-structure-executes", "random-affix", allowDuplicate.isSuccess);

            ItemInstanceRollResult addedUnrelated = ItemInstanceRollEngine.Generate(
                fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 42L, "QA-GREEN"),
                    statOverride: fixture.StatSchema(addUnrelated: true)));
            CheckBool(specs, result, "unrelated-stat-does-not-disturb", "domain",
                addedUnrelated.isSuccess && SameGeneratedBranches(green.snapshot, addedUnrelated.snapshot));
        }

        private static void RunFailureCases(Fixture fixture, List<SpecRow> specs, Verification result)
        {
            ExpectFailure(specs, result, "request-null", "request", ItemInstanceRollEngine.Generate(null),
                ItemInstanceRollValidationCodes.RequestNull);
            ExpectFailure(specs, result, "identity-null", "request",
                ItemInstanceRollEngine.Generate(new ItemInstanceRollRequest(null, fixture.Stats,
                    fixture.Affixes, fixture.Core, fixture.BuildProfile(ItemInstanceRarity.Green),
                    ItemGenerationDataStatus.QaFixtureCanonical)), ItemInstanceRollValidationCodes.IdentityNull);

            ItemInstanceIdentityCreationResult unsupportedIdentity = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                fixture.Foundation, "QA-V2", "I001", ItemInstanceRarity.Green, 2, 1L);
            ExpectFailure(specs, result, "generation-version-unsupported", "request",
                ItemInstanceRollEngine.Generate(fixture.Request(unsupportedIdentity.snapshot)),
                ItemInstanceRollValidationCodes.GenerationVersionUnsupported);
            ExpectFailure(specs, result, "generation-status-empty", "request",
                ItemInstanceRollEngine.Generate(fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 1L, "QA-EMPTY"),
                    statusOverride: string.Empty)), ItemInstanceRollValidationCodes.GenerationDataStatusEmpty);
            ExpectFailure(specs, result, "generation-status-invalid", "request",
                ItemInstanceRollEngine.Generate(fixture.Request(fixture.Identity(ItemInstanceRarity.Green, 1L, "QA-BADSTATUS"),
                    statusOverride: "FORMAL")), ItemInstanceRollValidationCodes.GenerationDataStatusInvalid);

            ItemInstanceIdentitySnapshot green = fixture.Identity(ItemInstanceRarity.Green, 4L, "QA-FAIL");
            ExpectFailure(specs, result, "stat-schema-null", "stat",
                ItemInstanceRollEngine.Generate(fixture.Request(green, statOverride: null, forceStatOverride: true)),
                ItemInstanceRollValidationCodes.StatSchemaNull);
            ExpectFailure(specs, result, "formal-empty-defaults-fail", "isolation",
                ItemInstanceRollEngine.Generate(new ItemInstanceRollRequest(green,
                    ItemStatRangeSchema.Create(Array.Empty<ItemStatDefinitionSnapshot>(), Array.Empty<ItemStatRangeProfileSnapshot>()),
                    ItemAffixPoolAndRangeCatalog.CreateDefaultSchema(),
                    ItemCorePotentialAndBuildEligibilityCatalog.CreateDefaultSchema(), null,
                    ItemGenerationDataStatus.QaFixtureCanonical)), ItemInstanceRollValidationCodes.StatProfileMissing);
            ExpectFailure(specs, result, "affix-schema-null", "affix",
                ItemInstanceRollEngine.Generate(fixture.Request(green, affixOverride: null, forceAffixOverride: true)),
                ItemInstanceRollValidationCodes.AffixSchemaNull);
            ExpectFailure(specs, result, "core-schema-null", "core",
                ItemInstanceRollEngine.Generate(fixture.Request(green, coreOverride: null, forceCoreOverride: true)),
                ItemInstanceRollValidationCodes.CoreSchemaNull);

            ExpectFailure(specs, result, "candidate-set-exhausted", "random-affix",
                ItemInstanceRollEngine.Generate(fixture.Request(green, affixOverride: fixture.ExhaustionAffixSchema())),
                ItemInstanceRollValidationCodes.AffixCandidateSetEmpty);
            ExpectFailure(specs, result, "affix-weight-overflow", "random-affix",
                ItemInstanceRollEngine.Generate(fixture.Request(green, affixOverride: fixture.OverflowAffixSchema())),
                ItemInstanceRollValidationCodes.AffixWeightSumOverflow);
            ExpectFailure(specs, result, "affix-weight-zero", "random-affix",
                ItemInstanceRollEngine.Generate(fixture.Request(green, affixOverride: fixture.InvalidWeightAffixSchema())),
                ItemInstanceRollValidationCodes.AffixWeightInvalid);

            ExpectFailure(specs, result, "build-profile-missing", "build",
                ItemInstanceRollEngine.Generate(fixture.Request(green, buildProfileOverride: null, forceBuildOverride: true)),
                ItemInstanceRollValidationCodes.BuildRollProfileMissing);
            ExpectFailure(specs, result, "build-profile-rarity-mismatch", "build",
                ItemInstanceRollEngine.Generate(fixture.Request(green,
                    buildProfileOverride: fixture.BuildProfile(ItemInstanceRarity.Blue))),
                ItemInstanceRollValidationCodes.BuildRollProfileRarityMismatch);
            ExpectFailure(specs, result, "build-entry-invalid", "build",
                ItemInstanceRollEngine.Generate(fixture.Request(green,
                    buildProfileOverride: new ItemBuildQualificationRollProfile("bad", ItemInstanceRarity.Green,
                        ItemGenerationDataStatus.QaFixtureOnly,
                        new[] { new ItemBuildQualificationRollEntry(ItemBuildQualification.Unresolved, 1) }))),
                ItemInstanceRollValidationCodes.BuildRollEntryInvalid);
            ExpectFailure(specs, result, "build-null-entry-invalid", "build",
                ItemInstanceRollEngine.Generate(fixture.Request(green,
                    buildProfileOverride: new ItemBuildQualificationRollProfile("null-entry",
                        ItemInstanceRarity.Green, ItemGenerationDataStatus.QaFixtureOnly,
                        new ItemBuildQualificationRollEntry[]
                        {
                            new ItemBuildQualificationRollEntry(ItemBuildQualification.None, 1),
                            null
                        }))), ItemInstanceRollValidationCodes.BuildRollEntryInvalid);
            ExpectFailure(specs, result, "build-weight-overflow", "build",
                ItemInstanceRollEngine.Generate(fixture.Request(green,
                    buildProfileOverride: new ItemBuildQualificationRollProfile("overflow", ItemInstanceRarity.Green,
                        ItemGenerationDataStatus.QaFixtureOnly, new[]
                        {
                            new ItemBuildQualificationRollEntry(ItemBuildQualification.None, long.MaxValue),
                            new ItemBuildQualificationRollEntry(ItemBuildQualification.FaMenOnly, long.MaxValue),
                            new ItemBuildQualificationRollEntry(ItemBuildQualification.QiLeiOnly, long.MaxValue)
                        }))), ItemInstanceRollValidationCodes.BuildWeightSumOverflow);

            ItemInstanceIdentitySnapshot mismatchedCoreIdentity = fixture.Identity(ItemInstanceRarity.Green, 4L,
                "QA-CORE-MISMATCH", "qa_core_i002_green");
            ExpectFailure(specs, result, "core-identity-mismatch", "core",
                ItemInstanceRollEngine.Generate(fixture.Request(mismatchedCoreIdentity)),
                ItemInstanceRollValidationCodes.CoreProfileIdentityMismatch);

            CheckRangeFailure(specs, result, "range-invalid", 5, 4, 1, ItemInstanceRollValidationCodes.RangeInvalid);
            CheckRangeFailure(specs, result, "range-step-invalid", 0, 10, 0, ItemInstanceRollValidationCodes.RangeStepInvalid);
            CheckRangeFailure(specs, result, "range-step-misaligned", 0, 10, 3, ItemInstanceRollValidationCodes.RangeStepInvalid);
            CheckRangeFailure(specs, result, "range-cardinality-unsupported", long.MinValue, long.MaxValue, 1,
                ItemInstanceRollValidationCodes.RangeCardinalityUnsupported);
        }

        private static void RunReadOnlyAndIsolationCases(Fixture fixture, List<SpecRow> specs, Verification result)
        {
            ItemInstanceIdentitySnapshot identity = fixture.Identity(ItemInstanceRarity.Green, 54L, "QA-READONLY");
            ItemInstanceRollResult generated = ItemInstanceRollEngine.Generate(fixture.Request(identity));
            CheckBool(specs, result, "result-stats-readonly", "readonly", RejectMutation(generated.snapshot?.GeneratedStats));
            CheckBool(specs, result, "result-affixes-readonly", "readonly", RejectMutation(generated.snapshot?.GeneratedAffixes));
            CheckBool(specs, result, "core-eligible-readonly", "readonly",
                RejectMutation(generated.snapshot?.GeneratedCorePotential?.EligibleCoreEffectIds));
            CheckBool(specs, result, "errors-readonly", "readonly",
                RejectMutation(ItemInstanceRollEngine.Generate(null).ValidationErrors));
            CheckBool(specs, result, "failure-has-no-snapshot", "failure",
                ItemInstanceRollEngine.Generate(null).snapshot == null);
            CheckBool(specs, result, "snapshot-properties-get-only", "readonly",
                typeof(ItemGeneratedInstanceSnapshot).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .All(property => property.SetMethod == null));

            string[] forbidden =
            {
                "placementId", "isLit", "countedInBuild", "unlockedCoreEffectIds",
                "activeCoreEffectIds", "itemPower"
            };
            string[] names = typeof(ItemGeneratedInstanceSnapshot).GetProperties().Select(property => property.Name)
                .Concat(typeof(ItemGeneratedCorePotentialSnapshot).GetProperties().Select(property => property.Name))
                .ToArray();
            CheckBool(specs, result, "forbidden-output-fields-absent", "contract",
                forbidden.All(value => !names.Contains(value, StringComparer.OrdinalIgnoreCase)));

            List<ItemStatDefinitionSnapshot> mutableDefinitions = fixture.StatDefinitions().ToList();
            List<ItemStatRangeProfileSnapshot> mutableProfiles = fixture.StatProfiles().ToList();
            ItemStatRangeSchemaSnapshot immutableStats = ItemStatRangeSchema.Create(mutableDefinitions, mutableProfiles);
            string before = immutableStats.BuildCanonicalSignature();
            mutableDefinitions.Clear();
            mutableProfiles.Clear();
            CheckBool(specs, result, "request-schema-source-not-mutated", "readonly",
                immutableStats.BuildCanonicalSignature() == before);

            ItemInstanceRollResult baseline = ItemInstanceRollEngine.Generate(fixture.Request(identity));
            ItemInstanceRollResult buildChanged = ItemInstanceRollEngine.Generate(fixture.Request(identity,
                buildProfileOverride: new ItemBuildQualificationRollProfile("qa_build_green_alt",
                    ItemInstanceRarity.Green, ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemBuildQualificationRollEntry(ItemBuildQualification.Dual, 1) })));
            CheckBool(specs, result, "build-domain-does-not-disturb-stats-affixes", "domain",
                baseline.isSuccess && buildChanged.isSuccess
                && SameStatsAndAffixes(baseline.snapshot, buildChanged.snapshot));
        }

        private static void RunLeakChecks(List<LeakRow> rows, Verification result)
        {
            string source = string.Join("\n", Directory.Exists(RuntimeRoot)
                ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText)
                : Array.Empty<string>());
            LeakDefinition[] definitions =
            {
                new("System.Random", "System.Random"), new("UnityEngine.Random", "UnityEngine.Random"),
                new("Random.Range", "Random.Range"), new("Guid.NewGuid", "Guid.NewGuid"),
                new("DateTime.Now", "DateTime.Now"), new("DateTime.UtcNow", "DateTime.UtcNow"),
                new("Environment.TickCount", "Environment.TickCount"), new("string.GetHashCode", "string.GetHashCode"),
                new("Reward", "Reward"), new("RunFlow", "RunFlow"), new("SaveData", "SaveData"),
                new("Boss", "Boss"), new("Battle Resolver", "BattleResolver"), new("Battle Bridge", "BattleBridge"),
                new("ItemBuildSynergyResolver", "ItemBuildSynergyResolver"),
                new("ItemCoreAwakeningResolver", "ItemCoreAwakeningResolver"), new("BuildSandbox", "BuildSandbox"),
                new("DropTable", "DropTable"), new("LootTable", "LootTable"), new("Scene", "Scene"),
                new("Prefab", "Prefab"), new("BuildSettings", "BuildSettings"), new("placementId", "placementId"),
                new("isLit", "isLit"), new("countedInBuild", "countedInBuild"),
                new("unlockedCoreEffectIds", "unlockedCoreEffectIds"),
                new("activeCoreEffectIds", "activeCoreEffectIds"), new("itemPower", "itemPower"),
                new("PlayerPrefs", "PlayerPrefs"), new("AssetDatabase", "AssetDatabase"), new("UnityEditor", "UnityEditor")
            };
            foreach (LeakDefinition definition in definitions)
            {
                int count = Count(source, definition.token);
                rows.Add(new LeakRow(definition.category, count));
                if (count != 0)
                {
                    result.Errors.Add($"LeakCheck '{definition.category}' found {count} occurrence(s).");
                }
            }
        }

        private static void RunHistoricalRegressions(Verification result)
        {
            try
            {
                ItemAffixPoolAndRangeSchemaVerifier.VerifyMenu();
            }
            catch (Exception exception)
            {
                result.Errors.Add("Historical generation verifier threw: " + exception.Message);
            }

            Regression[] regressions =
            {
                new("Foundation", "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md", "- Verification: PASS"),
                new("StatRange", "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md", "- Overall: PASS"),
                new("CorePotential", "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md", "- Overall: PASS"),
                new("AffixSchema", "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaReport.md", "- Result: PASS"),
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
                bool passed = File.Exists(regression.path) && File.ReadAllText(regression.path).Contains(regression.marker);
                result.Regressions.Add(regression.name + ": " + (passed ? "PASS" : "FAIL"));
                if (!passed) result.Errors.Add("Historical regression report is not PASS: " + regression.name);
            }

            result.FoundationCount = CsvRows("Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv");
            result.StatCount = CsvRows("Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv");
            result.CoreCount = CsvRows("Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv");
            result.AffixCount = CsvRows("Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv");
            if (result.FoundationCount != 159) result.Errors.Add("Foundation Spec must remain 159.");
            if (result.StatCount != 39) result.Errors.Add("StatRange Spec must remain 39.");
            if (result.CoreCount != 69) result.Errors.Add("CorePotential Spec must remain 69.");
            if (result.AffixCount != 88) result.Errors.Add("AffixSchema Spec must remain 88.");
        }

        private static void WriteReports(ItemInstanceRollResult sample, IReadOnlyList<SpecRow> specs,
            IReadOnlyList<DeterminismRow> determinism, IReadOnlyList<LeakRow> leaks,
            Verification result, bool passed)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath, BuildReport(sample, specs, determinism, leaks, result, passed), new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(specs), new UTF8Encoding(false));
            File.WriteAllText(DeterminismPath, BuildDeterminism(determinism), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeak(leaks, passed), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(ItemInstanceRollResult sample, IReadOnlyList<SpecRow> specs,
            IReadOnlyList<DeterminismRow> determinism, IReadOnlyList<LeakRow> leaks,
            Verification result, bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInstanceRollEngine01 Report").AppendLine()
                .AppendLine($"- Package: `{PackageName}`").AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(passed ? PassMarker : "ITEM_INSTANCE_ROLL_ENGINE01_FAIL")}`")
                .AppendLine($"- Algorithm ID: `{DeterministicItemRandom.AlgorithmId}`")
                .AppendLine($"- generationVersion: `{DeterministicItemRandom.SupportedGenerationVersion}`")
                .AppendLine("- Domain hash: `FNV-1a 64-bit over explicit length-prefixed UTF-8 fields and segment count`")
                .AppendLine("- PRNG: `SplitMix64`").AppendLine("- Bounded integer: `rejection sampling before modulo`")
                .AppendLine("- Seed fields: `algorithmId / rootSeed / generationVersion / itemInstanceId / baseItemId / rarity / domainSegmentCount / domainSegments`")
                .AppendLine($"- QA data status: `{ItemGenerationDataStatus.QaFixtureCanonical}`")
                .AppendLine($"- Spec: {specs.Count}/{specs.Count(row => row.result == "PASS")} PASS")
                .AppendLine($"- Determinism: {determinism.Count}/{determinism.Count(row => row.result == "PASS")} PASS")
                .AppendLine($"- Same-input repeat: {(specs.Any(row => row.caseId == "same-input-1000" && row.result == "PASS") ? "1000/1000 PASS" : "FAIL")}")
                .AppendLine($"- Sample stats: {sample?.snapshot?.GeneratedStats.Count ?? 0}")
                .AppendLine($"- Sample fixed affixes: {sample?.snapshot?.GeneratedAffixes.Count(item => item.slotKind == ItemAffixSlotKind.Fixed) ?? 0}")
                .AppendLine($"- Sample random affixes: {sample?.snapshot?.GeneratedAffixes.Count(item => item.slotKind == ItemAffixSlotKind.Random) ?? 0}")
                .AppendLine($"- Sample Build qualification: `{sample?.snapshot?.buildQualification.ToString() ?? "None"}`")
                .AppendLine("- Core potential: read-only eligible/visible projection; no unlock or activation state.")
                .AppendLine("- Mutex/repeat: legal candidates are filtered once, then one weighted selection is performed.")
                .AppendLine("- Formal integration: none; no formal generation Catalog, acquisition, persistence, cultivation, combat, detail UI, or Build counting connection.")
                .AppendLine().AppendLine("## Golden Vectors").AppendLine()
                .AppendLine("- FNV empty: `CBF29CE484222325`; FNV a: `AF63DC4C8601EC8C`; FNV hello: `A430D84680AABD0B`.")
                .AppendLine("- SplitMix64 stream from state 0: `E220A8397B1DCDAF / 6E789E6AA1B965F4 / 06C45D188009454F`.")
                .AppendLine("- SplitMix64 wraparound from `ulong.MaxValue`: `E4D971771B652C20`.")
                .AppendLine("- Segmented domain vector `[stat, qa_power]`: `54AE2882BEECC218`.")
                .AppendLine().AppendLine("## Historical Regressions").AppendLine();
            foreach (string regression in result.Regressions) builder.AppendLine("- " + regression);
            builder.AppendLine($"- Foundation: {result.FoundationCount}/159")
                .AppendLine($"- StatRange: {result.StatCount}/39")
                .AppendLine($"- CorePotential: {result.CoreCount}/69")
                .AppendLine($"- AffixSchema: {result.AffixCount}/88")
                .AppendLine().AppendLine("## LeakCheck").AppendLine()
                .AppendLine($"- Categories: {leaks.Count}; total leaks: {leaks.Sum(row => row.count)}")
                .AppendLine().AppendLine("## Errors").AppendLine();
            if (result.Errors.Count == 0) builder.AppendLine("- None.");
            else foreach (string error in result.Errors) builder.AppendLine("- " + error);
            return builder.ToString();
        }

        private static string BuildSpec(IEnumerable<SpecRow> rows)
        {
            StringBuilder builder = new("caseId,category,expected,actual,validationCode,result\n");
            foreach (SpecRow row in rows) builder.Append(Csv(row.caseId)).Append(',').Append(Csv(row.category))
                .Append(',').Append(Csv(row.expected)).Append(',').Append(Csv(row.actual)).Append(',')
                .Append(Csv(row.code)).Append(',').Append(Csv(row.result)).Append('\n');
            return builder.ToString();
        }

        private static string BuildDeterminism(IEnumerable<DeterminismRow> rows)
        {
            StringBuilder builder = new("caseId,rootSeed,itemInstanceId,domain,expected,actual,repeatCount,result\n");
            foreach (DeterminismRow row in rows) builder.Append(Csv(row.caseId)).Append(',')
                .Append(row.rootSeed.ToString(CultureInfo.InvariantCulture)).Append(',').Append(Csv(row.itemInstanceId))
                .Append(',').Append(Csv(row.domain)).Append(',').Append(Csv(row.expected)).Append(',')
                .Append(Csv(row.actual)).Append(',').Append(row.repeatCount.ToString(CultureInfo.InvariantCulture))
                .Append(',').Append(Csv(row.result)).Append('\n');
            return builder.ToString();
        }

        private static string BuildLeak(IEnumerable<LeakRow> rows, bool passed)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemInstanceRollEngine01 LeakCheck Report").AppendLine()
                .AppendLine($"- Runtime scan root: `{RuntimeRoot}`")
                .AppendLine($"- Result: {(passed && rows.All(row => row.count == 0) ? "PASS" : "FAIL")}")
                .AppendLine().AppendLine("| Category | Count |").AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows) builder.AppendLine($"| {row.category} | {row.count} |");
            return builder.ToString();
        }

        private static void ExpectFailure(List<SpecRow> rows, Verification result, string caseId,
            string category, ItemInstanceRollResult actual, string expectedCode)
        {
            bool pass = actual != null && !actual.isSuccess && actual.snapshot == null
                && actual.primaryValidationCode == expectedCode;
            rows.Add(new SpecRow(caseId, category, expectedCode, actual?.primaryValidationCode ?? "null",
                actual?.primaryValidationCode ?? "null", pass ? "PASS" : "FAIL"));
            if (!pass) result.Errors.Add($"Failure case '{caseId}' expected {expectedCode}, actual {actual?.primaryValidationCode ?? "null"}.");
        }

        private static void CheckRangeFailure(List<SpecRow> rows, Verification result, string caseId,
            long min, long max, long step, string expectedCode)
        {
            bool success = ItemInstanceRollEngine.TryGetRangeCardinality(min, max, step, out _, out string code);
            bool pass = !success && code == expectedCode;
            rows.Add(new SpecRow(caseId, "range", expectedCode, code, code, pass ? "PASS" : "FAIL"));
            if (!pass) result.Errors.Add("Range case failed: " + caseId);
        }

        private static void Check(List<SpecRow> rows, Verification result, string caseId,
            string category, string expected, string actual)
        {
            bool pass = string.Equals(expected, actual, StringComparison.Ordinal);
            rows.Add(new SpecRow(caseId, category, expected, actual,
                ItemInstanceRollValidationCodes.None, pass ? "PASS" : "FAIL"));
            if (!pass) result.Errors.Add($"Case '{caseId}' expected {expected}, actual {actual}.");
        }

        private static void CheckBool(List<SpecRow> rows, Verification result, string caseId,
            string category, bool pass)
        {
            rows.Add(new SpecRow(caseId, category, "true", pass ? "true" : "false",
                ItemInstanceRollValidationCodes.None, pass ? "PASS" : "FAIL"));
            if (!pass) result.Errors.Add("Boolean case failed: " + caseId);
        }

        private static bool RejectMutation<T>(IReadOnlyList<T> values)
        {
            if (values == null) return false;
            try { ((IList)values).Clear(); return false; }
            catch (NotSupportedException) { return true; }
            catch (InvalidCastException) { return true; }
        }

        private static bool SameGeneratedBranches(ItemGeneratedInstanceSnapshot left,
            ItemGeneratedInstanceSnapshot right)
        {
            return SameStatsAndAffixes(left, right)
                && left.buildQualification == right.buildQualification
                && left.GeneratedCorePotential.EligibleCoreEffectIds
                    .SequenceEqual(right.GeneratedCorePotential.EligibleCoreEffectIds);
        }

        private static bool SameStatsAndAffixes(ItemGeneratedInstanceSnapshot left,
            ItemGeneratedInstanceSnapshot right)
        {
            return left != null && right != null
                && left.GeneratedStats.Select(item => item.statId + ":" + item.rawUnits)
                    .SequenceEqual(right.GeneratedStats.Select(item => item.statId + ":" + item.rawUnits))
                && left.GeneratedAffixes.Select(item => item.slotId + ":" + item.affixId + ":" + item.rawUnits)
                    .SequenceEqual(right.GeneratedAffixes.Select(item => item.slotId + ":" + item.affixId + ":" + item.rawUnits));
        }

        private static string OutcomeKey(ItemGeneratedInstanceSnapshot snapshot)
        {
            return string.Join("|", snapshot.GeneratedStats.Select(item => item.rawUnits)) + "/"
                + string.Join("|", snapshot.GeneratedAffixes.Select(item => item.affixId + ":" + item.rawUnits))
                + "/" + snapshot.buildQualification;
        }

        private static string Hex(ulong value) => value.ToString("X16", CultureInfo.InvariantCulture);
        private static string Compact(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            string oneLine = value.Replace("\r", string.Empty).Replace("\n", " / ");
            return oneLine.Length <= 180 ? oneLine : oneLine.Substring(0, 180) + "...";
        }
        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static int CsvRows(string path) => File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;
        private static int Count(string source, string token)
        {
            int count = 0, index = 0;
            while (!string.IsNullOrEmpty(token) && (index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            { count++; index += token.Length; }
            return count;
        }

        private sealed class Fixture
        {
            public ItemGenerationFoundationSnapshot Foundation { get; private set; }
            public ItemStatRangeSchemaSnapshot Stats { get; private set; }
            public ItemAffixPoolAndRangeSchemaSnapshot Affixes { get; private set; }
            public ItemCorePotentialAndBuildEligibilitySchemaSnapshot Core { get; private set; }
            public readonly HashSet<string> RandomAffixIds = new(new[] { "qa_random_a", "qa_random_b", "qa_random_c" }, StringComparer.Ordinal);

            public static Fixture Create()
            {
                Fixture fixture = new();
                fixture.Foundation = ItemRarityInstanceFoundation.Create();
                fixture.Stats = fixture.StatSchema();
                fixture.Affixes = fixture.AffixSchema(ItemAffixRepeatPolicy.DisallowDuplicateAffix);
                fixture.Core = fixture.CoreSchema();
                return fixture;
            }

            public ItemInstanceIdentitySnapshot Identity(ItemInstanceRarity rarity, long seed, string instanceId,
                string coreProfileId = "")
            {
                return ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(Foundation, instanceId, "I001",
                    rarity, 1, seed, coreProfileId).snapshot;
            }

            public ItemInstanceRollRequest Request(ItemInstanceIdentitySnapshot identity, bool reverse = false,
                ItemStatRangeSchemaSnapshot statOverride = null, bool forceStatOverride = false,
                ItemAffixPoolAndRangeSchemaSnapshot affixOverride = null, bool forceAffixOverride = false,
                ItemCorePotentialAndBuildEligibilitySchemaSnapshot coreOverride = null, bool forceCoreOverride = false,
                ItemBuildQualificationRollProfile buildProfileOverride = null, bool forceBuildOverride = false,
                string statusOverride = null)
            {
                ItemStatRangeSchemaSnapshot stats = forceStatOverride ? statOverride : statOverride ?? (reverse ? StatSchema(reverse: true) : Stats);
                ItemAffixPoolAndRangeSchemaSnapshot affixes = forceAffixOverride ? affixOverride : affixOverride ?? (reverse ? AffixSchema(ItemAffixRepeatPolicy.DisallowDuplicateAffix, true) : Affixes);
                ItemCorePotentialAndBuildEligibilitySchemaSnapshot core = forceCoreOverride ? coreOverride : coreOverride ?? (reverse ? CoreSchema(true) : Core);
                ItemBuildQualificationRollProfile build = forceBuildOverride ? buildProfileOverride
                    : buildProfileOverride ?? (identity?.rarity == ItemInstanceRarity.White ? null : BuildProfile(identity?.rarity ?? ItemInstanceRarity.Green));
                return new ItemInstanceRollRequest(identity, stats, affixes, core, build,
                    statusOverride ?? ItemGenerationDataStatus.QaFixtureCanonical);
            }

            public ItemBuildQualificationRollProfile BuildProfile(ItemInstanceRarity rarity)
            {
                return new ItemBuildQualificationRollProfile("qa_build_" + rarity.ToStableKey(), rarity,
                    ItemGenerationDataStatus.QaFixtureOnly, new[]
                    {
                        new ItemBuildQualificationRollEntry(ItemBuildQualification.None, 1),
                        new ItemBuildQualificationRollEntry(ItemBuildQualification.FaMenOnly, 3),
                        new ItemBuildQualificationRollEntry(ItemBuildQualification.QiLeiOnly, 5),
                        new ItemBuildQualificationRollEntry(ItemBuildQualification.Dual, 2)
                    });
            }

            public IEnumerable<ItemStatDefinitionSnapshot> StatDefinitions()
            {
                return new[]
                {
                    new ItemStatDefinitionSnapshot("qa_power", "QA Power", "flat", ItemStatDirection.HigherIsBetter,
                        0, 2, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA),
                    new ItemStatDefinitionSnapshot("qa_cooldown", "QA Cooldown", "flat", ItemStatDirection.LowerIsBetter,
                        0, 1, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA)
                };
            }

            public IEnumerable<ItemStatRangeProfileSnapshot> StatProfiles()
            {
                return new[]
                {
                    new ItemStatRangeProfileSnapshot("I001", "qa_power", ItemStatDataMaturity.SEED_DATA,
                        new ItemNumericRangeSnapshot(10, 60), StatRanges((10,14),(20,30),(30,40),(40,50),(50,60))),
                    new ItemStatRangeProfileSnapshot("I001", "qa_cooldown", ItemStatDataMaturity.SEED_DATA,
                        new ItemNumericRangeSnapshot(1, 10), StatRanges((8,10),(6,9),(4,7),(2,5),(1,3)))
                };
            }

            public ItemStatRangeSchemaSnapshot StatSchema(bool reverse = false, bool addUnrelated = false)
            {
                List<ItemStatDefinitionSnapshot> definitions = StatDefinitions().ToList();
                List<ItemStatRangeProfileSnapshot> profiles = StatProfiles().ToList();
                if (addUnrelated)
                {
                    definitions.Add(new ItemStatDefinitionSnapshot("qa_unrelated", "QA Unrelated", "flat",
                        ItemStatDirection.Neutral, 0, 1, ItemStatRoundingMode.Nearest, ItemStatDataMaturity.SEED_DATA));
                    profiles.Add(new ItemStatRangeProfileSnapshot("I002", "qa_unrelated", ItemStatDataMaturity.SEED_DATA,
                        new ItemNumericRangeSnapshot(1, 5), StatRanges((1,1),(2,2),(3,3),(4,4),(5,5))));
                }
                return ItemStatRangeSchema.Create(reverse ? definitions.AsEnumerable().Reverse() : definitions,
                    reverse ? profiles.AsEnumerable().Reverse() : profiles);
            }

            public ItemAffixPoolAndRangeSchemaSnapshot AffixSchema(ItemAffixRepeatPolicy repeat, bool reverse = false)
            {
                ItemAffixDefinitionSnapshot[] definitions = AffixDefinitions();
                ItemAffixValueProfileSnapshot[] values = AffixProfiles();
                ItemAffixSlotPolicySnapshot slots = SlotPolicy();
                ItemRandomAffixPoolSnapshot pool = Pool(repeat, new[]
                {
                    Entry("qa_random_a", 1), Entry("qa_random_b", 3), Entry("qa_random_c", 7)
                }, new[] { new ItemAffixMutexGroupSnapshot("qa_mutex_ab", new[] { "qa_random_a", "qa_random_b" }) });
                ItemAffixGenerationProfileSnapshot generation = Generation();
                return ItemAffixPoolAndRangeSchema.Create(reverse ? definitions.Reverse() : definitions,
                    reverse ? values.Reverse() : values, new[] { slots }, new[] { pool }, new[] { generation });
            }

            public ItemAffixPoolAndRangeSchemaSnapshot ExhaustionAffixSchema()
            {
                return ItemAffixPoolAndRangeSchema.Create(AffixDefinitions(), AffixProfiles(), new[] { SlotPolicy() },
                    new[] { Pool(ItemAffixRepeatPolicy.DisallowDuplicateAffix, new[] { Entry("qa_random_c", 1) }, Array.Empty<ItemAffixMutexGroupSnapshot>()) },
                    new[] { Generation() });
            }

            public ItemAffixPoolAndRangeSchemaSnapshot OverflowAffixSchema()
            {
                return ItemAffixPoolAndRangeSchema.Create(AffixDefinitions(), AffixProfiles(), new[] { SlotPolicy() },
                    new[] { Pool(ItemAffixRepeatPolicy.AllowDuplicateAffix, new[]
                    {
                        Entry("qa_random_a", long.MaxValue), Entry("qa_random_b", long.MaxValue),
                        Entry("qa_random_c", long.MaxValue)
                    }, Array.Empty<ItemAffixMutexGroupSnapshot>()) }, new[] { Generation() });
            }

            public ItemAffixPoolAndRangeSchemaSnapshot InvalidWeightAffixSchema()
            {
                return ItemAffixPoolAndRangeSchema.Create(AffixDefinitions(), AffixProfiles(), new[] { SlotPolicy() },
                    new[] { Pool(ItemAffixRepeatPolicy.AllowDuplicateAffix, new[] { Entry("qa_random_a", 0) },
                        Array.Empty<ItemAffixMutexGroupSnapshot>()) }, new[] { Generation() });
            }

            public ItemCorePotentialAndBuildEligibilitySchemaSnapshot CoreSchema(bool reverse = false)
            {
                List<ItemCorePotentialProfileSnapshot> profiles = new();
                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    List<ItemCorePotentialEffectSnapshot> effects = new()
                    {
                        new ItemCorePotentialEffectSnapshot("qa_core_" + rarity.stableKey + "_01",
                            ItemCorePotentialEffectKind.Standard, true),
                        new ItemCorePotentialEffectSnapshot("qa_core_" + rarity.stableKey + "_02",
                            rarity.rarity == ItemInstanceRarity.Orange ? ItemCorePotentialEffectKind.Ultimate : ItemCorePotentialEffectKind.Standard, false)
                    };
                    profiles.Add(new ItemCorePotentialProfileSnapshot("qa_core_i001_" + rarity.stableKey,
                        "I001", rarity.rarity, ItemCorePotentialResolutionStatus.Defined,
                        ItemGenerationDataStatus.QaFixtureOnly, effects));
                }
                profiles.Add(new ItemCorePotentialProfileSnapshot("qa_core_i002_green", "I002",
                    ItemInstanceRarity.Green, ItemCorePotentialResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly,
                    new[] { new ItemCorePotentialEffectSnapshot("qa_i002_core", ItemCorePotentialEffectKind.Standard, true) }));
                IEnumerable<ItemCoreRarityPolicySnapshot> corePolicies = ItemCorePotentialAndBuildEligibilityCatalog.DefaultCoreRarityPolicies;
                IEnumerable<ItemBuildQualificationRarityPolicySnapshot> buildPolicies = ItemCorePotentialAndBuildEligibilityCatalog.DefaultBuildRarityPolicies;
                return ItemCorePotentialAndBuildEligibilitySchema.Create(
                    reverse ? corePolicies.Reverse() : corePolicies,
                    reverse ? profiles.AsEnumerable().Reverse() : profiles,
                    reverse ? buildPolicies.Reverse() : buildPolicies);
            }

            public bool StatValueValid(ItemGeneratedStatSnapshot stat, ItemInstanceRarity rarity)
            {
                ItemStatRangeProfileSnapshot profile = Stats.QueryProfile("I001", stat.statId).value;
                ItemStatDefinitionSnapshot definition = Stats.QueryStatDefinition(stat.statId).value;
                ItemRarityStatRangeSnapshot range = profile?.RarityRanges.FirstOrDefault(item => item.rarity == rarity);
                return range != null && definition != null && stat.rawUnits >= range.minUnits
                    && stat.rawUnits <= range.maxUnits
                    && unchecked((ulong)stat.rawUnits - (ulong)range.minUnits) % unchecked((ulong)definition.stepUnits) == 0UL;
            }

            public bool AffixValueValid(ItemGeneratedAffixSnapshot affix, ItemInstanceRarity rarity)
            {
                ItemAffixValueProfileSnapshot profile = Affixes.QueryAffixValueProfile(affix.affixValueProfileId).value;
                ItemAffixRarityValueRangeSnapshot range = profile?.RarityRanges.FirstOrDefault(item => item.rarity == rarity);
                return range != null && affix.rawUnits >= range.minUnits && affix.rawUnits <= range.maxUnits
                    && unchecked((ulong)affix.rawUnits - (ulong)range.minUnits) % unchecked((ulong)profile.stepUnits) == 0UL;
            }

            private static ItemRarityStatRangeSnapshot[] StatRanges(params (long min, long max)[] values)
            {
                return ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                    new ItemRarityStatRangeSnapshot(rarity.rarity, values[index].min, values[index].max)).ToArray();
            }

            private static ItemAffixDefinitionSnapshot[] AffixDefinitions()
            {
                return new[]
                {
                    AffixDefinition("qa_fixed_guard"), AffixDefinition("qa_random_a"),
                    AffixDefinition("qa_random_b"), AffixDefinition("qa_random_c")
                };
            }

            private static ItemAffixDefinitionSnapshot AffixDefinition(string id)
            {
                return new ItemAffixDefinitionSnapshot(id, id, "flat", ItemStatDirection.HigherIsBetter,
                    0, 1, ItemStatRoundingMode.Nearest, ItemGenerationDataStatus.QaFixtureOnly);
            }

            private static ItemAffixValueProfileSnapshot[] AffixProfiles()
            {
                return AffixDefinitions().Select(definition => new ItemAffixValueProfileSnapshot(
                    definition.affixId + "_value", definition.affixId, ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, new ItemAffixNumericRangeSnapshot(1, 15),
                    ItemInstanceRarityCatalog.All.Select((rarity, index) =>
                        new ItemAffixRarityValueRangeSnapshot(rarity.rarity, 1 + index * 3, 3 + index * 3)),
                    definition.direction, definition.decimalPlaces, definition.stepUnits, definition.roundingMode)).ToArray();
            }

            private static ItemAffixSlotPolicySnapshot SlotPolicy()
            {
                return new ItemAffixSlotPolicySnapshot("qa_slots", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, new[]
                    {
                        new ItemAffixSlotSnapshot("fixed_01", ItemAffixSlotKind.Fixed),
                        new ItemAffixSlotSnapshot("random_01", ItemAffixSlotKind.Random),
                        new ItemAffixSlotSnapshot("random_02", ItemAffixSlotKind.Random)
                    });
            }

            private static ItemAffixPoolEntrySnapshot Entry(string id, long weight)
            {
                return new ItemAffixPoolEntrySnapshot(id, id + "_value",
                    ItemAffixWeightResolutionStatus.Defined, weight);
            }

            private static ItemRandomAffixPoolSnapshot Pool(ItemAffixRepeatPolicy repeat,
                IEnumerable<ItemAffixPoolEntrySnapshot> entries, IEnumerable<ItemAffixMutexGroupSnapshot> mutex)
            {
                return new ItemRandomAffixPoolSnapshot("qa_pool", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, repeat, entries, mutex);
            }

            private static ItemAffixGenerationProfileSnapshot Generation()
            {
                return new ItemAffixGenerationProfileSnapshot("I001", ItemAffixResolutionStatus.Defined,
                    ItemGenerationDataStatus.QaFixtureOnly, "qa_slots", new[]
                    {
                        new ItemFixedAffixBindingSnapshot("fixed_01", "qa_fixed_guard", "qa_fixed_guard_value")
                    }, "qa_pool");
            }
        }

        private sealed class Verification
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public int FoundationCount, StatCount, CoreCount, AffixCount;
        }
        private readonly struct SpecRow
        {
            public SpecRow(string caseId, string category, string expected, string actual, string code, string result)
            { this.caseId = caseId; this.category = category; this.expected = expected; this.actual = actual; this.code = code; this.result = result; }
            public readonly string caseId, category, expected, actual, code, result;
        }
        private readonly struct DeterminismRow
        {
            public DeterminismRow(string caseId, long rootSeed, string itemInstanceId, string domain,
                string expected, string actual, int repeatCount, string result)
            { this.caseId = caseId; this.rootSeed = rootSeed; this.itemInstanceId = itemInstanceId; this.domain = domain; this.expected = expected; this.actual = actual; this.repeatCount = repeatCount; this.result = result; }
            public readonly string caseId, itemInstanceId, domain, expected, actual, result;
            public readonly long rootSeed;
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
