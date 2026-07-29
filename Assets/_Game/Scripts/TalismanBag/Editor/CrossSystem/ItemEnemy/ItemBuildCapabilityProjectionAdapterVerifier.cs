using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Stats;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class ItemBuildCapabilityProjectionAdapterVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string MainReportName =
            "ItemBuildCapabilityProjectionAdapterReport.md";
        private const string FieldMapName =
            "ItemBuildCapabilityProjectionFieldMap.csv";
        private const string FixtureRowsName =
            "ItemBuildCapabilityProjectionFixtureRows.csv";
        private const string UnknownsName =
            "ItemBuildCapabilityProjectionUnknowns.csv";
        private const string LeakReportName =
            "ItemBuildCapabilityProjectionLeakCheckReport.md";

        private const string ExpectedItemHash =
            "7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const int ExpectedItemFileCount = 105;
        private const int ExpectedEnemyFileCount = 89;

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs.meta",
            ReportRoot + MainReportName,
            ReportRoot + FieldMapName,
            ReportRoot + FixtureRowsName,
            ReportRoot + UnknownsName,
            ReportRoot + LeakReportName
        };

        private static readonly string[] PackageSourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/ItemBuildCapabilityProjectionAdapter01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyBatch()
        {
            VerifyAndWrite(true, "Unity batch");
        }

        public static void VerifyOffline()
        {
            VerifyAndWrite(false, "Pure C# same-source offline verifier");
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyOffline();
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            string root = FindProjectRoot();
            List<Check> checks = new List<Check>();
            List<LeakRow> leakRows = new List<LeakRow>();
            Fixture fixture = null;
            try
            {
                fixture = CreateFixture();
                RunContractChecks(checks, fixture);
                RunSemanticChecks(checks, fixture);
                RunDeterminismAndReadOnlyChecks(checks, fixture);
                RunProtectedHashChecks(checks, root, fixture);
                RunLeakChecks(checks, leakRows, root);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception",
                    exception.ToString(), false);
            }

            if (fixture != null)
            {
                WriteReports(root, mode, checks, leakRows, fixture);
                RunExpectedFileChecks(checks, root);
                WriteReports(root, mode, checks, leakRows, fixture);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine(
                (pass
                    ? "ITEM_BUILD_CAPABILITY_PROJECTION_ADAPTER_PASS "
                    : "ITEM_BUILD_CAPABILITY_PROJECTION_ADAPTER_FAIL ")
                + checks.Count(value => value.Passed)
                + "/"
                + checks.Count.ToString(CultureInfo.InvariantCulture));

#if UNITY_EDITOR
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "ItemBuildCapabilityProjectionAdapter01 verifier failed: "
                    + string.Join(" | ", checks.Where(value => !value.Passed)
                        .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static Fixture CreateFixture()
        {
            DefaultItemEnemyBuildCapabilityVocabularyResolver resolver =
                new DefaultItemEnemyBuildCapabilityVocabularyResolver();
            DefaultItemBuildCapabilityProjectionAdapter adapter =
                new DefaultItemBuildCapabilityProjectionAdapter(resolver);
            ItemAffixPoolAndRangeSchemaSnapshot mappedSchema =
                CreateMappedAffixSchema("basisPoint");

            ItemInstanceProjectionContractSnapshot supportedA = CreateProjection(
                "QA-C01-orange-A",
                "I001",
                ItemInstanceRarity.Orange,
                new[] { new StatSpec("damage", 45) },
                new[]
                {
                    new AffixSpec("random_01", "affix_break_up", 700),
                    new AffixSpec("random_02", "affix_guard_up", 800),
                    new AffixSpec("fixed_01", "affix_cooldown_reduction", 900),
                    new AffixSpec("random_03", "affix_unmapped_stable", 55)
                },
                new[] { "core_stable_eligible" },
                new[] { "core_stable_eligible" },
                ItemBuildQualification.Dual);
            ItemInstanceProjectionContractSnapshot supportedB = CreateProjection(
                "QA-C01-green-B",
                "I001",
                ItemInstanceRarity.Green,
                Array.Empty<StatSpec>(),
                new[] { new AffixSpec("random_01", "affix_break_up", 300) },
                Array.Empty<string>(),
                Array.Empty<string>(),
                ItemBuildQualification.FaMenOnly);
            ItemInstanceProjectionContractSnapshot negative = CreateProjection(
                "QA-C01-negative",
                "I002",
                ItemInstanceRarity.Blue,
                Array.Empty<StatSpec>(),
                new[] { new AffixSpec("random_01", "affix_break_up", -1) },
                Array.Empty<string>(),
                Array.Empty<string>(),
                ItemBuildQualification.None);
            ItemInstanceProjectionContractSnapshot saturatingA = CreateProjection(
                "QA-C01-saturation-A",
                "I003",
                ItemInstanceRarity.Purple,
                Array.Empty<StatSpec>(),
                new[] { new AffixSpec("random_01", "affix_break_up", 7000) },
                Array.Empty<string>(),
                Array.Empty<string>(),
                ItemBuildQualification.None);
            ItemInstanceProjectionContractSnapshot saturatingB = CreateProjection(
                "QA-C01-saturation-B",
                "I004",
                ItemInstanceRarity.White,
                Array.Empty<StatSpec>(),
                new[] { new AffixSpec("random_01", "affix_break_up", 6000) },
                Array.Empty<string>(),
                Array.Empty<string>(),
                ItemBuildQualification.None);

            ItemInstanceProjectionQaFixture itemFixture =
                ItemInstanceProjectionQaFixture.Create();
            string supportedInputBefore = InputFingerprint(
                new[] { supportedA, supportedB }, mappedSchema);
            ItemBuildCapabilityProjectionInput supportedInput =
                new ItemBuildCapabilityProjectionInput(
                    new[] { supportedA, supportedB },
                    mappedSchema);
            ItemBuildCapabilityProjectionResult supported =
                adapter.Project(supportedInput);
            ItemBuildCapabilityProjectionResult supportedRepeat =
                adapter.Project(supportedInput);
            string supportedInputAfter = InputFingerprint(
                new[] { supportedA, supportedB }, mappedSchema);

            ItemBuildCapabilityProjectionResult empty = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    Array.Empty<ItemInstanceProjectionContractSnapshot>(),
                    mappedSchema));
            ItemBuildCapabilityProjectionResult singleBase = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    new[] { itemFixture.ProjectionSet.Projections.First() },
                    mappedSchema));
            ItemBuildCapabilityProjectionResult multiRarity = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    itemFixture.ProjectionSet.Projections,
                    itemFixture.AffixSchema));
            ItemBuildCapabilityProjectionResult missingSchema = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    new[] { supportedA },
                    null));
            ItemBuildCapabilityProjectionResult missingProjection = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    null,
                    mappedSchema));
            ItemBuildCapabilityProjectionResult wrongUnit = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    new[] { supportedA },
                    CreateMappedAffixSchema("flat")));
            ItemBuildCapabilityProjectionResult negativeResult = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    new[] { negative },
                    mappedSchema));
            ItemBuildCapabilityProjectionResult saturated = adapter.Project(
                new ItemBuildCapabilityProjectionInput(
                    new[] { saturatingA, saturatingB },
                    mappedSchema));

            List<FixtureRow> rows = new List<FixtureRow>
            {
                Row("empty-item-set", "Empty complete projection set", empty, 0,
                    "Explicit Known Zero for all unit-safe supported rules."),
                Row("single-base-item", "Single base Item", singleBase, 1,
                    "One QA projection; mapped affixes absent under complete mapped schema."),
                Row("multiple-items", "Multiple Item instances", supported, 2,
                    "Two instances contribute deterministically."),
                Row("same-base-different-instance", "Same baseItemId, distinct itemInstanceId", supported, 2,
                    "QA-C01-orange-A and QA-C01-green-B both use I001."),
                Row("five-rarities", "White through Orange", multiRarity,
                    itemFixture.ProjectionSet.Projections.Count,
                    "Shared Item QA fixture covers all five rarity tiers."),
                Row("stats-present", "Stats present", supported, 2,
                    "damage flat stat remains unmapped; no BP conversion guessed."),
                Row("affixes-present", "Affixes present", supported, 2,
                    "Three direct BP rules plus one unmapped stable affix."),
                Row("core-effect-present", "Eligible and visible Core Effect", supported, 2,
                    "Core key is diagnosed Unknown without a locked rule."),
                Row("build-qualification", "Dual and FaMenOnly qualification", supported, 2,
                    "Categorical qualification is not converted to BP."),
                Row("missing-stable-facts", "Missing Affix schema and ItemSystem facts", missingSchema, 1,
                    "Supported rules become Sparse omissions."),
                Row("explicit-known-zero", "Complete empty sources", empty, 0,
                    "Zero is emitted, not inferred from missing data."),
                Row("unmapped-stable-key", "affix_unmapped_stable", supported, 2,
                    "Stable unknown key is reported without name guessing."),
                Row("missing-projection-set", "Null projection collection", missingProjection, 0,
                    "Missing collection remains Unknown, distinct from empty."),
                Row("unit-mismatch", "Mapped key with non-BP unit", wrongUnit, 1,
                    "No flat-to-BP conversion is invented."),
                Row("negative-source", "Negative BP source", negativeResult, 1,
                    "Positive capability scale does not clamp negative input to zero."),
                Row("saturating-sum", "Two BP sources exceed maximum", saturated, 2,
                    "Deterministic sum saturates at BuildCapabilityScale maximum.")
            };

            return new Fixture(
                resolver,
                adapter,
                mappedSchema,
                supportedInput,
                supported,
                supportedRepeat,
                empty,
                multiRarity,
                missingSchema,
                missingProjection,
                wrongUnit,
                negativeResult,
                saturated,
                supportedInputBefore,
                supportedInputAfter,
                rows);
        }

        private static void RunContractChecks(
            ICollection<Check> checks,
            Fixture fixture)
        {
            IReadOnlyList<string> keys = fixture.Resolver.GetKnownBuildCapabilityKeys();
            Add(checks, "resolver.authoritative-key-count", "16",
                keys.Count.ToString(CultureInfo.InvariantCulture), keys.Count == 16);
            Add(checks, "resolver.unique-ordinal-keys", "16",
                keys.Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                keys.Distinct(StringComparer.Ordinal).Count() == keys.Count);
            Add(checks, "field-map.every-resolver-key", keys.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Supported.FieldMap.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Supported.FieldMap.Count == keys.Count
                    && fixture.Supported.FieldMap.Select(value => value.TargetCapabilityKey)
                        .SequenceEqual(keys.OrderBy(value => value, StringComparer.Ordinal),
                            StringComparer.Ordinal));
            Add(checks, "source-contract.schema",
                ItemInstanceProjectionContractSnapshot.CurrentSchemaId,
                ItemInstanceProjectionContractSnapshot.CurrentSchemaId,
                ItemInstanceProjectionContractSnapshot.CurrentSchemaId ==
                    "ItemInstanceProjectionContractSnapshot.v1");
            Add(checks, "target-contract.schema", "BuildCapabilityReadContract.v1",
                fixture.Supported.Snapshot.SchemaId,
                fixture.Supported.Snapshot.SchemaId == BuildCapabilityReadSchema.SchemaId);
            Add(checks, "mapping.version",
                DefaultItemBuildCapabilityProjectionAdapter.MappingVersion,
                fixture.Supported.MappingVersion,
                fixture.Supported.MappingVersion ==
                    DefaultItemBuildCapabilityProjectionAdapter.MappingVersion);
            Add(checks, "snapshot.coverage", "Sparse",
                fixture.Supported.Snapshot.CoverageMode.ToString(),
                fixture.Supported.Snapshot.CoverageMode ==
                    BuildCapabilityCoverageMode.Sparse);
            Add(checks, "snapshot.protected-flags", "true/false/false",
                fixture.Supported.Snapshot.DevOnly + "/"
                + fixture.Supported.Snapshot.IsEnabled + "/"
                + fixture.Supported.Snapshot.EntersFormalFlow,
                fixture.Supported.Snapshot.DevOnly
                    && !fixture.Supported.Snapshot.IsEnabled
                    && !fixture.Supported.Snapshot.EntersFormalFlow);
        }

        private static void RunSemanticChecks(
            ICollection<Check> checks,
            Fixture fixture)
        {
            StatusCounts supportedCounts = Counts(fixture.Supported);
            Add(checks, "mapping.supported-counts", "3/0/11/2",
                supportedCounts.ToString(),
                supportedCounts.Supported == 3
                    && supportedCounts.KnownZero == 0
                    && supportedCounts.Unknown == 11
                    && supportedCounts.OutOfScope == 2);
            StatusCounts emptyCounts = Counts(fixture.Empty);
            Add(checks, "mapping.empty-counts", "0/3/11/2",
                emptyCounts.ToString(),
                emptyCounts.Supported == 0
                    && emptyCounts.KnownZero == 3
                    && emptyCounts.Unknown == 11
                    && emptyCounts.OutOfScope == 2);

            CheckValue(checks, fixture.Supported, "capability.break_power", 1000);
            CheckValue(checks, fixture.Supported, "capability.guard_power", 800);
            CheckValue(checks, fixture.Supported, "capability.cooldown_recovery", 900);
            CheckValue(checks, fixture.Saturated, "capability.break_power", 10000);

            bool emptyZeros = fixture.Empty.Snapshot.CapabilityValues.Count == 3
                && fixture.Empty.Snapshot.CapabilityValues.All(value =>
                    value.ValueBasisPoints == 0)
                && fixture.Empty.FieldMap.Count(value => value.MappingStatus ==
                    ItemBuildCapabilityMappingStatus.KNOWN_ZERO) == 3;
            Add(checks, "known-zero.explicit", "3 emitted zero values",
                fixture.Empty.Snapshot.CapabilityValues.Count + " values",
                emptyZeros);

            bool missingSchemaUnknown = fixture.MissingSchema.Snapshot.CapabilityValues.Count == 0
                && fixture.MissingSchema.FieldMap.Count(value =>
                    value.MappingStatus ==
                        ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED) == 14
                && fixture.MissingSchema.UnknownDiagnostics.Any(value =>
                    value.Code == "ITEM_AFFIX_SCHEMA_MISSING");
            Add(checks, "unknown.missing-schema", "Sparse omission",
                fixture.MissingSchema.Snapshot.CapabilityValues.Count + " values",
                missingSchemaUnknown);

            bool missingProjectionUnknown =
                fixture.MissingProjection.Snapshot.CapabilityValues.Count == 0
                && fixture.MissingProjection.UnknownDiagnostics.Any(value =>
                    value.Code == "ITEM_PROJECTION_SET_MISSING");
            Add(checks, "unknown.missing-projection", "Sparse omission",
                fixture.MissingProjection.Snapshot.CapabilityValues.Count + " values",
                missingProjectionUnknown);

            bool wrongUnitUnknown = !fixture.WrongUnit.Snapshot.TryGetCapabilityValue(
                    "capability.break_power", out _)
                && fixture.WrongUnit.UnknownDiagnostics.Any(value =>
                    value.Code == "ITEM_AFFIX_UNIT_NOT_BASIS_POINT"
                    && value.TargetCapabilityKey == "capability.break_power");
            Add(checks, "unknown.unit-mismatch", "break omitted",
                wrongUnitUnknown ? "break omitted" : "break was emitted",
                wrongUnitUnknown);

            bool negativeUnknown = !fixture.Negative.Snapshot.TryGetCapabilityValue(
                    "capability.break_power", out _)
                && fixture.Negative.UnknownDiagnostics.Any(value =>
                    value.Code == "ITEM_AFFIX_NEGATIVE_BASIS_POINT_UNSUPPORTED");
            Add(checks, "unknown.negative-not-zero", "break omitted",
                negativeUnknown ? "break omitted" : "break was emitted",
                negativeUnknown);

            bool unmappedKeys = fixture.Supported.UnknownDiagnostics.Any(value =>
                    value.Code == "UNMAPPED_STABLE_STAT_KEY"
                    && value.SourceStableKey == "damage")
                && fixture.Supported.UnknownDiagnostics.Any(value =>
                    value.Code == "UNMAPPED_STABLE_AFFIX_KEY"
                    && value.SourceStableKey == "affix_unmapped_stable")
                && fixture.Supported.UnknownDiagnostics.Any(value =>
                    value.Code == "UNMAPPED_VISIBLE_CORE_EFFECT_KEY")
                && fixture.Supported.UnknownDiagnostics.Any(value =>
                    value.Code == "BUILD_QUALIFICATION_RULE_UNCONFIRMED")
                && fixture.Supported.UnknownDiagnostics.Any(value =>
                    value.Code == "ITEM_SYSTEM_BUILD_FACTS_MISSING");
            Add(checks, "unknown.stable-source-diagnostics", "all categories present",
                unmappedKeys ? "all categories present" : "one or more missing",
                unmappedKeys);

            bool range = fixture.Supported.Snapshot.CapabilityValues.All(value =>
                    value.ValueBasisPoints >= BuildCapabilityScale.MinimumValueBasisPoints
                    && value.ValueBasisPoints <= BuildCapabilityScale.MaximumValueBasisPoints)
                && fixture.Saturated.Snapshot.CapabilityValues.All(value =>
                    value.ValueBasisPoints >= BuildCapabilityScale.MinimumValueBasisPoints
                    && value.ValueBasisPoints <= BuildCapabilityScale.MaximumValueBasisPoints);
            Add(checks, "values.range", "0..10000", range ? "0..10000" : "out of range", range);
        }

        private static void RunDeterminismAndReadOnlyChecks(
            ICollection<Check> checks,
            Fixture fixture)
        {
            Add(checks, "determinism.canonical-signature",
                fixture.Supported.Snapshot.CanonicalSignature,
                fixture.SupportedRepeat.Snapshot.CanonicalSignature,
                fixture.Supported.Snapshot.CanonicalSignature ==
                    fixture.SupportedRepeat.Snapshot.CanonicalSignature);
            Add(checks, "determinism.source-revision",
                fixture.Supported.SourceRevisionId,
                fixture.SupportedRepeat.SourceRevisionId,
                fixture.Supported.SourceRevisionId ==
                    fixture.SupportedRepeat.SourceRevisionId);
            Add(checks, "determinism.unknown-diagnostics",
                DiagnosticsSignature(fixture.Supported),
                DiagnosticsSignature(fixture.SupportedRepeat),
                DiagnosticsSignature(fixture.Supported) ==
                    DiagnosticsSignature(fixture.SupportedRepeat));
            Add(checks, "input.immutable",
                fixture.SupportedInputBefore,
                fixture.SupportedInputAfter,
                fixture.SupportedInputBefore == fixture.SupportedInputAfter);
            Add(checks, "fixture.minimum-count", ">=11",
                fixture.Rows.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Rows.Count >= 11);
            Add(checks, "fixture.rarity-coverage", "5",
                fixture.MultiRarity == null ? "0" : "5",
                fixture.MultiRarity != null);
        }

        private static void RunProtectedHashChecks(
            ICollection<Check> checks,
            string root,
            Fixture fixture)
        {
            fixture.ItemHash = AggregateHash(
                root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            fixture.EnemyHash = AggregateHash(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            Add(checks, "protected.item.file-count",
                ExpectedItemFileCount.ToString(CultureInfo.InvariantCulture),
                fixture.ItemHash.FileCount.ToString(CultureInfo.InvariantCulture),
                fixture.ItemHash.FileCount == ExpectedItemFileCount);
            Add(checks, "protected.item.hash", ExpectedItemHash,
                fixture.ItemHash.Hash,
                fixture.ItemHash.Hash == ExpectedItemHash);
            Add(checks, "protected.enemy.file-count",
                ExpectedEnemyFileCount.ToString(CultureInfo.InvariantCulture),
                fixture.EnemyHash.FileCount.ToString(CultureInfo.InvariantCulture),
                fixture.EnemyHash.FileCount == ExpectedEnemyFileCount);
            Add(checks, "protected.enemy.hash", ExpectedEnemyHash,
                fixture.EnemyHash.Hash,
                fixture.EnemyHash.Hash == ExpectedEnemyHash);
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            ICollection<LeakRow> leakRows,
            string root)
        {
            string[] forbiddenTokens =
            {
                "EnemyOfflineReadiness" + "Evaluator",
                "TalismanBag.Battle",
                "TalismanBag.Board",
                "BattleContract",
                "UnifiedBattlePage",
                "RunFlow",
                "SaveData",
                "PlayerPrefs",
                "DropBias",
                "RewardConfig",
                "SceneManager",
                "PrefabUtility",
                "AssetDatabase.CreateAsset"
            };
            foreach (string relative in PackageSourceFiles)
            {
                string text = StripCommentsAndStringLiterals(
                    File.ReadAllText(Absolute(root, relative)));
                foreach (string token in forbiddenTokens)
                {
                    int count = CountOrdinal(text, token);
                    if (count > 0)
                    {
                        leakRows.Add(new LeakRow(relative, token, count));
                    }
                }
            }

            string runtimeText = File.ReadAllText(Absolute(root, PackageSourceFiles[0]));
            foreach (string token in new[]
            {
                "using UnityEngine",
                "MonoBehaviour",
                "ScriptableObject",
                "GameObject",
                "RectTransform",
                "Button",
                ".unity",
                ".prefab"
            })
            {
                int count = CountOrdinal(runtimeText, token);
                if (count > 0)
                {
                    leakRows.Add(new LeakRow(PackageSourceFiles[0], token, count));
                }
            }

            Add(checks, "leak.count", "0",
                leakRows.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture),
                leakRows.Count == 0);
        }

        private static string StripCommentsAndStringLiterals(string source)
        {
            string text = source ?? string.Empty;
            StringBuilder output = new StringBuilder(text.Length);
            bool lineComment = false;
            bool blockComment = false;
            bool regularString = false;
            bool verbatimString = false;
            bool character = false;

            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                char next = index + 1 < text.Length ? text[index + 1] : '\0';

                if (lineComment)
                {
                    if (current == '\n')
                    {
                        lineComment = false;
                        output.Append('\n');
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    continue;
                }

                if (blockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        output.Append("  ");
                        index++;
                        blockComment = false;
                    }
                    else
                    {
                        output.Append(current == '\n' ? '\n' : ' ');
                    }
                    continue;
                }

                if (regularString)
                {
                    if (current == '\\' && next != '\0')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '"')
                    {
                        output.Append(' ');
                        regularString = false;
                    }
                    else
                    {
                        output.Append(current == '\n' ? '\n' : ' ');
                    }
                    continue;
                }

                if (verbatimString)
                {
                    if (current == '"' && next == '"')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '"')
                    {
                        output.Append(' ');
                        verbatimString = false;
                    }
                    else
                    {
                        output.Append(current == '\n' ? '\n' : ' ');
                    }
                    continue;
                }

                if (character)
                {
                    if (current == '\\' && next != '\0')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '\'')
                    {
                        output.Append(' ');
                        character = false;
                    }
                    else
                    {
                        output.Append(' ');
                    }
                    continue;
                }

                if (current == '/' && next == '/')
                {
                    output.Append("  ");
                    index++;
                    lineComment = true;
                }
                else if (current == '/' && next == '*')
                {
                    output.Append("  ");
                    index++;
                    blockComment = true;
                }
                else if (current == '@' && next == '"')
                {
                    output.Append("  ");
                    index++;
                    verbatimString = true;
                }
                else if (current == '"')
                {
                    output.Append(' ');
                    regularString = true;
                }
                else if (current == '\'')
                {
                    output.Append(' ');
                    character = true;
                }
                else
                {
                    output.Append(current);
                }
            }

            return output.ToString();
        }

        private static void RunExpectedFileChecks(
            ICollection<Check> checks,
            string root)
        {
            string[] missing = ExpectedPackageFiles
                .Where(relative => !File.Exists(Absolute(root, relative)))
                .ToArray();
            Add(checks, "package.expected-files", "all present",
                missing.Length == 0 ? "all present" : string.Join(";", missing),
                missing.Length == 0);
        }

        private static void CheckValue(
            ICollection<Check> checks,
            ItemBuildCapabilityProjectionResult result,
            string key,
            int expected)
        {
            bool found = result.Snapshot.TryGetCapabilityValue(
                key,
                out BuildCapabilityValueSnapshot value);
            Add(checks, "value." + key, expected.ToString(CultureInfo.InvariantCulture),
                found ? value.ValueBasisPoints.ToString(CultureInfo.InvariantCulture) : "Unknown",
                found && value.ValueBasisPoints == expected);
        }

        private static StatusCounts Counts(
            ItemBuildCapabilityProjectionResult result)
        {
            return new StatusCounts(
                result.FieldMap.Count(value => value.MappingStatus ==
                    ItemBuildCapabilityMappingStatus.SUPPORTED),
                result.FieldMap.Count(value => value.MappingStatus ==
                    ItemBuildCapabilityMappingStatus.KNOWN_ZERO),
                result.FieldMap.Count(value => value.MappingStatus ==
                    ItemBuildCapabilityMappingStatus.UNKNOWN_NOT_MAPPED),
                result.FieldMap.Count(value => value.MappingStatus ==
                    ItemBuildCapabilityMappingStatus.OUT_OF_SCOPE));
        }

        private static ItemAffixPoolAndRangeSchemaSnapshot CreateMappedAffixSchema(
            string breakUnit)
        {
            ItemAffixDefinitionSnapshot[] definitions =
            {
                Definition("affix_break_up", breakUnit),
                Definition("affix_guard_up", "basisPoint"),
                Definition("affix_cooldown_reduction", "basisPoint")
            };
            return ItemAffixPoolAndRangeSchema.Create(
                definitions,
                Array.Empty<ItemAffixValueProfileSnapshot>(),
                Array.Empty<ItemAffixSlotPolicySnapshot>(),
                Array.Empty<ItemRandomAffixPoolSnapshot>(),
                Array.Empty<ItemAffixGenerationProfileSnapshot>());
        }

        private static ItemAffixDefinitionSnapshot Definition(
            string affixId,
            string unit)
        {
            return new ItemAffixDefinitionSnapshot(
                affixId,
                affixId,
                unit,
                ItemStatDirection.HigherIsBetter,
                0,
                1,
                ItemStatRoundingMode.Nearest,
                "QA_FIXTURE_ONLY");
        }

        private static ItemInstanceProjectionContractSnapshot CreateProjection(
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            IEnumerable<StatSpec> stats,
            IEnumerable<AffixSpec> affixes,
            IEnumerable<string> eligibleCoreIds,
            IEnumerable<string> visibleCoreIds,
            ItemBuildQualification qualification)
        {
            ItemInstanceProjectionStatSnapshot[] statRows =
                (stats ?? Array.Empty<StatSpec>()).Select(value =>
                    Construct<ItemInstanceProjectionStatSnapshot>(
                        value.StatId,
                        value.RawUnits)).ToArray();
            ItemInstanceProjectionAffixSnapshot[] affixRows =
                (affixes ?? Array.Empty<AffixSpec>()).Select(value =>
                    Construct<ItemInstanceProjectionAffixSnapshot>(
                        value.SlotId,
                        ItemAffixSlotKind.Random,
                        value.AffixId,
                        value.AffixId + "_value",
                        value.RawUnits)).ToArray();

            return Construct<ItemInstanceProjectionContractSnapshot>(
                "ItemGeneratedInstanceSnapshot.v1",
                "QA_ITEM_ENEMY_PROJECTION",
                "QA_FIXTURE_ONLY",
                itemInstanceId,
                baseItemId,
                rarity,
                rarity.ToStableKey(),
                1,
                101L,
                "qa_core_profile",
                statRows,
                affixRows,
                (eligibleCoreIds ?? Array.Empty<string>()).ToArray(),
                (visibleCoreIds ?? Array.Empty<string>()).ToArray(),
                qualification,
                "qa-source-" + itemInstanceId);
        }

        private static T Construct<T>(params object[] args) where T : class
        {
            try
            {
                return (T)Activator.CreateInstance(
                    typeof(T),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    args,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "QA fixture could not invoke the internal immutable snapshot constructor for "
                    + typeof(T).FullName + ".",
                    exception);
            }
        }

        private static string InputFingerprint(
            IEnumerable<ItemInstanceProjectionContractSnapshot> items,
            ItemAffixPoolAndRangeSchemaSnapshot schema)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ItemInstanceProjectionContractSnapshot item in
                (items ?? Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal))
            {
                builder.Append(item.BuildCanonicalSignature()).Append('\n');
            }

            builder.Append(schema == null ? "<missing>" : schema.BuildCanonicalSignature());
            return Sha256(Encoding.UTF8.GetBytes(builder.ToString()), false);
        }

        private static string DiagnosticsSignature(
            ItemBuildCapabilityProjectionResult result)
        {
            return string.Join("\n", result.UnknownDiagnostics.Select(value =>
                value.Code + "|" + value.TargetCapabilityKey + "|"
                + value.SourceStableKey + "|" + value.Detail));
        }

        private static FixtureRow Row(
            string id,
            string scenario,
            ItemBuildCapabilityProjectionResult result,
            int itemCount,
            string notes)
        {
            return new FixtureRow(
                id,
                scenario,
                itemCount,
                Counts(result),
                string.Join(";", result.Snapshot.CapabilityValues.Select(value =>
                    value.BuildCapabilityKey + "="
                    + value.ValueBasisPoints.ToString(CultureInfo.InvariantCulture))),
                result.Snapshot.CanonicalSignature,
                result.UnknownDiagnostics.Count,
                notes);
        }

        private static AggregateHashResult AggregateHash(
            string root,
            string relativeDirectory)
        {
            string absoluteDirectory = Absolute(root, relativeDirectory);
            string[] files = Directory.GetFiles(
                    absoluteDirectory,
                    "*",
                    SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            string payload = string.Join("\n", files.Select(file =>
                Relative(root, file) + "|" + Sha256(File.ReadAllBytes(file), true)));
            return new AggregateHashResult(
                files.Length,
                Sha256(Encoding.UTF8.GetBytes(payload), false));
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<Check> checks,
            IReadOnlyList<LeakRow> leakRows,
            Fixture fixture)
        {
            string reportDirectory = Absolute(root, ReportRoot);
            Directory.CreateDirectory(reportDirectory);
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            StatusCounts supported = Counts(fixture.Supported);
            StatusCounts empty = Counts(fixture.Empty);

            StringBuilder main = new StringBuilder();
            main.AppendLine("# Item Build Capability Projection Adapter Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemBuildCapabilityProjectionAdapter01`")
                .AppendLine("- Guard receipt: `GUARD_PASS_ITEMBUILDCAPABILITYPROJECTIONADAPTER01`")
                .AppendLine("- Source contract: `" +
                    ItemInstanceProjectionContractSnapshot.CurrentSchemaId + "`")
                .AppendLine("- Target contract: `" + BuildCapabilityReadSchema.SchemaId + "`")
                .AppendLine("- Mapping version: `" +
                    DefaultItemBuildCapabilityProjectionAdapter.MappingVersion + "`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Known capability keys: `" +
                    fixture.Resolver.GetKnownBuildCapabilityKeys().Count + "`")
                .AppendLine("- Representative mapping counts (SUPPORTED / KNOWN_ZERO / UNKNOWN_NOT_MAPPED / OUT_OF_SCOPE): `"
                    + supported + "`")
                .AppendLine("- Empty complete-source counts: `" + empty + "`")
                .AppendLine("- Fixture rows: `" + fixture.Rows.Count + "`")
                .AppendLine("- Determinism signature: `" +
                    fixture.Supported.Snapshot.CanonicalSignature + "`")
                .AppendLine("- Input immutable: `" +
                    (fixture.SupportedInputBefore == fixture.SupportedInputAfter ? "PASS" : "FAIL") + "`")
                .AppendLine("- Item protected hash: `" +
                    (fixture.ItemHash == null ? "NOT_RUN" : fixture.ItemHash.Hash) + "`")
                .AppendLine("- Enemy protected hash: `" +
                    (fixture.EnemyHash == null ? "NOT_RUN" : fixture.EnemyHash.Hash) + "`")
                .AppendLine("- Formal-system leak count: `" +
                    leakRows.Sum(value => value.Count) + "`")
                .AppendLine("- Unity validation status: `" +
                    (mode == "Unity batch" ? (pass ? "PASS" : "FAIL") : "PENDING_UNITY_BATCH") + "`")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                main.Append("| ").Append(Md(check.Id)).Append(" | ")
                    .Append(Md(check.Expected)).Append(" | ")
                    .Append(Md(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            WriteUtf8(Path.Combine(reportDirectory, MainReportName), main.ToString());

            StringBuilder fieldMap = new StringBuilder();
            fieldMap.AppendLine("targetCapabilityKey,mappingStatus,sourceContract,sourceFieldOrStableKey,calculationRuleId,availabilityPolicy,valueScale,sourceCategoryId,notes");
            foreach (ItemBuildCapabilityProjectionFieldMapSnapshot row in
                fixture.Supported.FieldMap)
            {
                fieldMap.AppendLine(Csv(row.TargetCapabilityKey,
                    row.MappingStatus.ToString(), row.SourceContract,
                    row.SourceFieldOrStableKey, row.CalculationRuleId,
                    row.AvailabilityPolicy, row.ValueScale,
                    row.SourceCategoryId, row.Notes));
            }
            WriteUtf8(Path.Combine(reportDirectory, FieldMapName), fieldMap.ToString());

            StringBuilder fixtures = new StringBuilder();
            fixtures.AppendLine("fixtureId,scenario,itemCount,supportedCount,knownZeroCount,unknownNotMappedCount,outOfScopeCount,knownValues,unknownDiagnosticCount,canonicalSignature,notes");
            foreach (FixtureRow row in fixture.Rows)
            {
                fixtures.AppendLine(Csv(row.Id, row.Scenario,
                    row.ItemCount.ToString(CultureInfo.InvariantCulture),
                    row.Counts.Supported.ToString(CultureInfo.InvariantCulture),
                    row.Counts.KnownZero.ToString(CultureInfo.InvariantCulture),
                    row.Counts.Unknown.ToString(CultureInfo.InvariantCulture),
                    row.Counts.OutOfScope.ToString(CultureInfo.InvariantCulture),
                    row.KnownValues,
                    row.UnknownCount.ToString(CultureInfo.InvariantCulture),
                    row.CanonicalSignature,
                    row.Notes));
            }
            WriteUtf8(Path.Combine(reportDirectory, FixtureRowsName), fixtures.ToString());

            StringBuilder unknowns = new StringBuilder();
            unknowns.AppendLine("code,targetCapabilityKey,sourceStableKey,detail");
            foreach (ItemBuildCapabilityUnknownDiagnosticSnapshot row in
                fixture.Supported.UnknownDiagnostics)
            {
                unknowns.AppendLine(Csv(row.Code, row.TargetCapabilityKey,
                    row.SourceStableKey, row.Detail));
            }
            WriteUtf8(Path.Combine(reportDirectory, UnknownsName), unknowns.ToString());

            StringBuilder leak = new StringBuilder();
            leak.AppendLine("# Item Build Capability Projection Leak Check Report")
                .AppendLine()
                .AppendLine("- Result: `" + (leakRows.Count == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak Count: `" + leakRows.Sum(value => value.Count) + "`")
                .AppendLine("- Scope: new CrossSystem runtime adapter and QA verifier sources only.")
                .AppendLine("- Battle / Board / Runtime flow / Scene / Prefab / Config connection: `0`")
                .AppendLine("- Enemy readiness evaluation calls: `0`")
                .AppendLine()
                .AppendLine("| File | Token | Count |")
                .AppendLine("|---|---|---|");
            if (leakRows.Count == 0)
            {
                leak.AppendLine("| — | — | 0 |");
            }
            else
            {
                foreach (LeakRow row in leakRows)
                {
                    leak.Append("| ").Append(Md(row.File)).Append(" | ")
                        .Append(Md(row.Token)).Append(" | ")
                        .Append(row.Count).AppendLine(" |");
                }
            }
            WriteUtf8(Path.Combine(reportDirectory, LeakReportName), leak.ToString());
        }

        private static void WriteUtf8(string path, string text)
        {
            File.WriteAllText(path, text ?? string.Empty, new UTF8Encoding(false));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", (values ?? Array.Empty<string>()).Select(value =>
            {
                string safe = value ?? string.Empty;
                return "\"" + safe.Replace("\"", "\"\"") + "\"";
            }));
        }

        private static string Md(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|")
                .Replace("\r", " ").Replace("\n", " ");
        }

        private static int CountOrdinal(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
            {
                return 0;
            }

            int count = 0;
            int offset = 0;
            while ((offset = text.IndexOf(token, offset,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }

            return count;
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string format = uppercase ? "X2" : "x2";
                return string.Concat(sha256.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
            }
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(current.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(current.FullName, "Packages")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root,
                (relative ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string absolute)
        {
            string prefix = root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return absolute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? absolute.Substring(prefix.Length).Replace('\\', '/')
                : absolute.Replace('\\', '/');
        }

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class Fixture
        {
            public Fixture(
                DefaultItemEnemyBuildCapabilityVocabularyResolver resolver,
                DefaultItemBuildCapabilityProjectionAdapter adapter,
                ItemAffixPoolAndRangeSchemaSnapshot mappedSchema,
                ItemBuildCapabilityProjectionInput supportedInput,
                ItemBuildCapabilityProjectionResult supported,
                ItemBuildCapabilityProjectionResult supportedRepeat,
                ItemBuildCapabilityProjectionResult empty,
                ItemBuildCapabilityProjectionResult multiRarity,
                ItemBuildCapabilityProjectionResult missingSchema,
                ItemBuildCapabilityProjectionResult missingProjection,
                ItemBuildCapabilityProjectionResult wrongUnit,
                ItemBuildCapabilityProjectionResult negative,
                ItemBuildCapabilityProjectionResult saturated,
                string supportedInputBefore,
                string supportedInputAfter,
                IReadOnlyList<FixtureRow> rows)
            {
                Resolver = resolver;
                Adapter = adapter;
                MappedSchema = mappedSchema;
                SupportedInput = supportedInput;
                Supported = supported;
                SupportedRepeat = supportedRepeat;
                Empty = empty;
                MultiRarity = multiRarity;
                MissingSchema = missingSchema;
                MissingProjection = missingProjection;
                WrongUnit = wrongUnit;
                Negative = negative;
                Saturated = saturated;
                SupportedInputBefore = supportedInputBefore;
                SupportedInputAfter = supportedInputAfter;
                Rows = rows;
            }

            public DefaultItemEnemyBuildCapabilityVocabularyResolver Resolver { get; }
            public DefaultItemBuildCapabilityProjectionAdapter Adapter { get; }
            public ItemAffixPoolAndRangeSchemaSnapshot MappedSchema { get; }
            public ItemBuildCapabilityProjectionInput SupportedInput { get; }
            public ItemBuildCapabilityProjectionResult Supported { get; }
            public ItemBuildCapabilityProjectionResult SupportedRepeat { get; }
            public ItemBuildCapabilityProjectionResult Empty { get; }
            public ItemBuildCapabilityProjectionResult MultiRarity { get; }
            public ItemBuildCapabilityProjectionResult MissingSchema { get; }
            public ItemBuildCapabilityProjectionResult MissingProjection { get; }
            public ItemBuildCapabilityProjectionResult WrongUnit { get; }
            public ItemBuildCapabilityProjectionResult Negative { get; }
            public ItemBuildCapabilityProjectionResult Saturated { get; }
            public string SupportedInputBefore { get; }
            public string SupportedInputAfter { get; }
            public IReadOnlyList<FixtureRow> Rows { get; }
            public AggregateHashResult ItemHash { get; set; }
            public AggregateHashResult EnemyHash { get; set; }
        }

        private sealed class FixtureRow
        {
            public FixtureRow(
                string id,
                string scenario,
                int itemCount,
                StatusCounts counts,
                string knownValues,
                string canonicalSignature,
                int unknownCount,
                string notes)
            {
                Id = id;
                Scenario = scenario;
                ItemCount = itemCount;
                Counts = counts;
                KnownValues = knownValues;
                CanonicalSignature = canonicalSignature;
                UnknownCount = unknownCount;
                Notes = notes;
            }

            public string Id { get; }
            public string Scenario { get; }
            public int ItemCount { get; }
            public StatusCounts Counts { get; }
            public string KnownValues { get; }
            public string CanonicalSignature { get; }
            public int UnknownCount { get; }
            public string Notes { get; }
        }

        private sealed class StatusCounts
        {
            public StatusCounts(int supported, int knownZero, int unknown, int outOfScope)
            {
                Supported = supported;
                KnownZero = knownZero;
                Unknown = unknown;
                OutOfScope = outOfScope;
            }

            public int Supported { get; }
            public int KnownZero { get; }
            public int Unknown { get; }
            public int OutOfScope { get; }

            public override string ToString()
            {
                return Supported + "/" + KnownZero + "/" + Unknown + "/" + OutOfScope;
            }
        }

        private sealed class StatSpec
        {
            public StatSpec(string statId, long rawUnits)
            {
                StatId = statId;
                RawUnits = rawUnits;
            }

            public string StatId { get; }
            public long RawUnits { get; }
        }

        private sealed class AffixSpec
        {
            public AffixSpec(string slotId, string affixId, long rawUnits)
            {
                SlotId = slotId;
                AffixId = affixId;
                RawUnits = rawUnits;
            }

            public string SlotId { get; }
            public string AffixId { get; }
            public long RawUnits { get; }
        }

        private sealed class AggregateHashResult
        {
            public AggregateHashResult(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }

        private sealed class LeakRow
        {
            public LeakRow(string file, string token, int count)
            {
                File = file;
                Token = token;
                Count = count;
            }

            public string File { get; }
            public string Token { get; }
            public int Count { get; }
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }

            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }
    }
}
