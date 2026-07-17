using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class BuildCapabilityReadContractVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string CompleteId = "dev_build_capability_fixture_complete";
        private const string SparseId = "dev_build_capability_fixture_sparse";
        private const string SparseEmptyId = "dev_build_capability_fixture_sparse_empty";
        private const string SourceRevisionId = "revision_fixture_001";

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs"] = "83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs"] = "7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs"] = "777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs"] = "AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs"] = "44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs"] = "ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs"] = "243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs"] = "6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs"] = "49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs"] = "4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/EnemyValidationContentSnapshot.cs"] = "DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/EnemyValidationContentNormalizerCore.cs"] = "67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs"] = "452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizeVerifier.cs"] = "D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs"] = "9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs"] = "33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs"] = "72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs"] = "894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs"] = "4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs"] = "22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs"] = "B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs"] = "A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs"] = "3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs"] = "E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs"] = "56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs"] = "5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs"] = "1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs"] = "2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4"
                });

        private static readonly IReadOnlyDictionary<string, string> LegacyHashes =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs"] = "C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9",
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs"] = "14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060",
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs"] = "A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4"
                });

        private static readonly string[] RuntimeFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs"
        };

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildCapabilityReadContractReport.md",
            "Docs/V0.4/Reports/BuildCapabilityReadContractSpec.csv",
            "Docs/V0.4/Reports/BuildCapabilityReadContractFieldMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityReadContractFixtureRows.csv",
            "Docs/V0.4/Reports/BuildCapabilityReadContractLeakCheckReport.md"
        };

        private static readonly string[] DependencyForbiddenTokens =
        {
            "using UnityEngine",
            "MonoBehaviour",
            "ScriptableObject",
            "GameObject",
            "Transform",
            "Addressables",
            "SceneManager",
            "ItemSystemSnapshot",
            "Inventory",
            "Equipment",
            "Affix",
            "Synergy",
            "TalismanBag.BuildSandbox",
            "TalismanBag.EnemySystem.Normalization",
            "TalismanBag.EnemySystem.Composition",
            "TalismanBag.EnemySystem.SkillPhase",
            "TalismanBag.EnemySystem.PressureWindow",
            "BattleContract",
            "BattleBridge",
            "UnifiedBattlePage",
            "RunFlow",
            "RewardConfig",
            "SaveData",
            "PlayerPrefs",
            "V02FormationGridFrame",
            "DamageText"
        };

        private static readonly string[] EvaluationForbiddenTokens =
        {
            "Evaluator",
            "Evaluate(",
            "MinimumCapabilityBasisPoints",
            "RequirementMatchMode",
            "PlayerHintProjection",
            "DropBias",
            "BossKey",
            "3-10",
            "4-10",
            "Math.Min",
            "Math.Max",
            "Math.Clamp",
            "ValueBasisPoints +",
            "+ ValueBasisPoints",
            "ValueBasisPoints *",
            "RequiredBasisPoints -",
            "- AvailableBasisPoints"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/BuildCapabilityReadContract01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyBatch()
        {
            VerifyAndWrite(IsBatchMode(), "Unity batch");
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
            VerificationFixture fixture = null;
            try
            {
                EnemyMechanicVocabularySnapshot vocabulary =
                    DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput());
                E02BuildCapabilityResolver resolver =
                    new E02BuildCapabilityResolver(vocabulary);
                fixture = CreateFixture(resolver);
                RunCoreChecks(checks, resolver, fixture);
                RunValidationChecks(checks, resolver, fixture);
                RunBaselineChecks(checks, root, vocabulary);
                RunLeakChecks(checks, root);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            if (fixture != null)
            {
                WriteReports(root, mode, checks, fixture);
                RunGeneratedFileChecks(checks, root);
                WriteReports(root, mode, checks, fixture);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine(
                (pass
                    ? "BUILD_CAPABILITY_READ_CONTRACT_PASS "
                    : "BUILD_CAPABILITY_READ_CONTRACT_FAIL ")
                + checks.Count(value => value.Passed)
                + "/"
                + checks.Count);
#if UNITY_EDITOR
            if (exitWhenDone)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "BuildCapabilityReadContract01 verifier failed: "
                    + string.Join(
                        " | ",
                        checks.Where(value => !value.Passed)
                            .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static VerificationFixture CreateFixture(
            E02BuildCapabilityResolver resolver)
        {
            BuildCapabilitySnapshot complete = Snapshot(
                CreateInput(
                    resolver.GetKnownBuildCapabilityKeys(),
                    BuildCapabilityCoverageMode.Complete,
                    CompleteId,
                    false),
                resolver);
            BuildCapabilitySnapshot sparse = Snapshot(
                CreateInput(
                    resolver.GetKnownBuildCapabilityKeys().Take(4).ToArray(),
                    BuildCapabilityCoverageMode.Sparse,
                    SparseId,
                    false),
                resolver);
            BuildCapabilitySnapshot sparseEmpty = Snapshot(
                new BuildCapabilitySnapshotInput(
                    SparseEmptyId,
                    SourceRevisionId,
                    BuildCapabilityCoverageMode.Sparse,
                    Array.Empty<BuildCapabilityValueSnapshot>()),
                resolver);
            CapabilityGapSnapshot[] gaps =
            {
                new CapabilityGapSnapshot(
                    "group.fixture.known",
                    complete.CapabilityValues[0].BuildCapabilityKey,
                    CapabilityValueAvailability.Known,
                    0,
                    4000,
                    4000),
                new CapabilityGapSnapshot(
                    "group.fixture.unknown",
                    complete.CapabilityValues[1].BuildCapabilityKey,
                    CapabilityValueAvailability.Unknown,
                    0,
                    5000,
                    5000),
                new CapabilityGapSnapshot(
                    "group.fixture.ready",
                    complete.CapabilityValues[2].BuildCapabilityKey,
                    CapabilityValueAvailability.Known,
                    7000,
                    5000,
                    0)
            };
            return new VerificationFixture(complete, sparse, sparseEmpty, gaps);
        }

        private static BuildCapabilitySnapshotInput CreateInput(
            IReadOnlyList<string> keys,
            BuildCapabilityCoverageMode mode,
            string snapshotId,
            bool reverse)
        {
            List<BuildCapabilityValueSnapshot> values = new List<BuildCapabilityValueSnapshot>();
            for (int index = 0; index < keys.Count; index++)
            {
                List<BuildCapabilitySourceSummarySnapshot> summaries =
                    new List<BuildCapabilitySourceSummarySnapshot>();
                if (index < 3)
                {
                    summaries.Add(new BuildCapabilitySourceSummarySnapshot(
                        "source.fixture.base." + index.ToString(CultureInfo.InvariantCulture),
                        index + 1,
                        500 + (index * 100),
                        false));
                    summaries.Add(new BuildCapabilitySourceSummarySnapshot(
                        "source.fixture.composite." + index.ToString(CultureInfo.InvariantCulture),
                        index + 2,
                        250 + (index * 100),
                        true));
                }

                values.Add(new BuildCapabilityValueSnapshot(
                    keys[index],
                    index == 0 ? 0 : 1000 + (index * 300),
                    summaries));
            }

            if (reverse)
            {
                values.Reverse();
            }

            return new BuildCapabilitySnapshotInput(
                snapshotId,
                SourceRevisionId,
                mode,
                values);
        }

        private static BuildCapabilitySnapshot Snapshot(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver)
        {
            return DefaultBuildCapabilitySnapshotProvider.Instance.CreateSnapshot(
                input,
                resolver);
        }

        private static void RunCoreChecks(
            List<Check> checks,
            E02BuildCapabilityResolver resolver,
            VerificationFixture fixture)
        {
            Add(checks, "schema.id", BuildCapabilityReadSchema.SchemaId,
                fixture.Complete.SchemaId,
                fixture.Complete.SchemaId == BuildCapabilityReadSchema.SchemaId);
            Add(checks, "schema.version", "1",
                fixture.Complete.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                fixture.Complete.SchemaVersion == 1);
            Add(checks, "scale.minimum", "0",
                BuildCapabilityScale.MinimumValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                BuildCapabilityScale.MinimumValueBasisPoints == 0);
            Add(checks, "scale.maximum", "10000",
                BuildCapabilityScale.MaximumValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                BuildCapabilityScale.MaximumValueBasisPoints == 10000);
            Add(checks, "fixture.knownKeys", "16",
                resolver.GetKnownBuildCapabilityKeys().Count.ToString(CultureInfo.InvariantCulture),
                resolver.GetKnownBuildCapabilityKeys().Count == 16);
            Add(checks, "fixture.completeValues", "16",
                fixture.Complete.CapabilityValues.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Complete.CapabilityValues.Count == 16);
            Add(checks, "fixture.sparseValues", "4",
                fixture.Sparse.CapabilityValues.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Sparse.CapabilityValues.Count == 4);
            Add(checks, "fixture.sparseEmptyValues", "0",
                fixture.SparseEmpty.CapabilityValues.Count.ToString(CultureInfo.InvariantCulture),
                fixture.SparseEmpty.CapabilityValues.Count == 0);
            int sourceSummaries = fixture.Complete.CapabilityValues.Sum(
                value => value.SourceSummaries.Count);
            Add(checks, "fixture.sourceSummaries", "6",
                sourceSummaries.ToString(CultureInfo.InvariantCulture),
                sourceSummaries == 6);
            Add(checks, "fixture.gapShapes", "3",
                fixture.Gaps.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Gaps.Count == 3);
            string[] bandNames = Enum.GetNames(typeof(ReadinessBand));
            Add(checks, "fixture.readinessBands", "Unknown;Blocked;Strained;Ready;Strong",
                string.Join(";", bandNames),
                bandNames.SequenceEqual(
                    new[] { "Unknown", "Blocked", "Strained", "Ready", "Strong" },
                    StringComparer.Ordinal));

            string zeroKey = fixture.Complete.CapabilityValues[0].BuildCapabilityKey;
            bool knownZeroFound = fixture.Complete.TryGetCapabilityValue(
                zeroKey,
                out BuildCapabilityValueSnapshot knownZero);
            Add(checks, "query.knownZero", "true/0",
                knownZeroFound + "/" + (knownZero == null ? "null" : knownZero.ValueBasisPoints.ToString(CultureInfo.InvariantCulture)),
                knownZeroFound && knownZero != null && knownZero.ValueBasisPoints == 0);
            string unknownKey = fixture.Complete.CapabilityValues[8].BuildCapabilityKey;
            bool unknownFound = fixture.Sparse.TryGetCapabilityValue(
                unknownKey,
                out BuildCapabilityValueSnapshot unknownValue);
            Add(checks, "query.unknown", "false/null",
                unknownFound + "/" + (unknownValue == null ? "null" : "value"),
                !unknownFound && unknownValue == null);
            bool ordinalMismatch = fixture.Complete.TryGetCapabilityValue(
                zeroKey.ToUpperInvariant(),
                out _);
            Add(checks, "query.ordinal", "false", ordinalMismatch.ToString(), !ordinalMismatch);
            bool nullLookup = fixture.Complete.TryGetCapabilityValue(null, out _);
            Add(checks, "query.null", "false", nullLookup.ToString(), !nullLookup);

            string signaturePattern = "^sha256:[0-9a-f]{64}$";
            Add(checks, "canonical.format", signaturePattern,
                fixture.Complete.CanonicalSignature,
                Regex.IsMatch(fixture.Complete.CanonicalSignature, signaturePattern));
            BuildCapabilitySnapshot reordered = Snapshot(
                CreateInput(
                    resolver.GetKnownBuildCapabilityKeys(),
                    BuildCapabilityCoverageMode.Complete,
                    CompleteId,
                    true),
                resolver);
            Add(checks, "canonical.orderIndependent", fixture.Complete.CanonicalSignature,
                reordered.CanonicalSignature,
                fixture.Complete.CanonicalSignature == reordered.CanonicalSignature);
            AddSignatureSensitivityChecks(checks, resolver, fixture.Complete);
            AddImmutabilityChecks(checks, resolver, fixture.Complete);
            AddIsolationChecks(checks, fixture);
            AddGapShapeChecks(checks, fixture.Gaps);
        }

        private static void AddSignatureSensitivityChecks(
            ICollection<Check> checks,
            E02BuildCapabilityResolver resolver,
            BuildCapabilitySnapshot baseline)
        {
            List<BuildCapabilityValueSnapshot> changedValues = CloneValues(
                baseline.CapabilityValues);
            BuildCapabilityValueSnapshot original = changedValues[1];
            changedValues[1] = new BuildCapabilityValueSnapshot(
                original.BuildCapabilityKey,
                original.ValueBasisPoints + 1,
                original.SourceSummaries);
            AddSignatureDifference(checks, "canonical.valueSensitive", baseline,
                Snapshot(Input(changedValues, BuildCapabilityCoverageMode.Complete), resolver));

            changedValues = CloneValues(baseline.CapabilityValues);
            original = changedValues[0];
            List<BuildCapabilitySourceSummarySnapshot> summaries = original.SourceSummaries
                .Select(value => new BuildCapabilitySourceSummarySnapshot(
                    value.SourceCategoryId,
                    value.SourceCount,
                    value.ContributionBasisPoints,
                    value.IsConditional))
                .ToList();
            BuildCapabilitySourceSummarySnapshot first = summaries[0];
            summaries[0] = new BuildCapabilitySourceSummarySnapshot(
                first.SourceCategoryId,
                first.SourceCount,
                first.ContributionBasisPoints + 1,
                first.IsConditional);
            changedValues[0] = new BuildCapabilityValueSnapshot(
                original.BuildCapabilityKey,
                original.ValueBasisPoints,
                summaries);
            AddSignatureDifference(checks, "canonical.summarySensitive", baseline,
                Snapshot(Input(changedValues, BuildCapabilityCoverageMode.Complete), resolver));
            AddSignatureDifference(checks, "canonical.coverageSensitive", baseline,
                Snapshot(Input(CloneValues(baseline.CapabilityValues),
                    BuildCapabilityCoverageMode.Sparse), resolver));
            AddSignatureDifference(checks, "canonical.snapshotIdSensitive", baseline,
                Snapshot(Input(CloneValues(baseline.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete, CompleteId + "_changed"), resolver));
            AddSignatureDifference(checks, "canonical.revisionSensitive", baseline,
                Snapshot(new BuildCapabilitySnapshotInput(
                    CompleteId,
                    SourceRevisionId + "_changed",
                    BuildCapabilityCoverageMode.Complete,
                    CloneValues(baseline.CapabilityValues)), resolver));
        }

        private static void AddSignatureDifference(
            ICollection<Check> checks,
            string id,
            BuildCapabilitySnapshot baseline,
            BuildCapabilitySnapshot changed)
        {
            Add(checks, id, "different", changed.CanonicalSignature,
                baseline.CanonicalSignature != changed.CanonicalSignature);
        }

        private static void AddImmutabilityChecks(
            ICollection<Check> checks,
            E02BuildCapabilityResolver resolver,
            BuildCapabilitySnapshot complete)
        {
            List<BuildCapabilityValueSnapshot> mutable = CloneValues(
                complete.CapabilityValues);
            BuildCapabilitySnapshotInput input = Input(
                mutable,
                BuildCapabilityCoverageMode.Complete,
                "dev_build_capability_fixture_defensive");
            mutable.Clear();
            Add(checks, "immutable.inputDefensiveCopy", "16",
                input.CapabilityValues.Count.ToString(CultureInfo.InvariantCulture),
                input.CapabilityValues.Count == 16);
            BuildCapabilitySnapshot snapshot = Snapshot(input, resolver);
            bool valuesReadOnly = IsReadOnly(snapshot.CapabilityValues);
            bool summariesReadOnly = snapshot.CapabilityValues.All(
                value => IsReadOnly(value.SourceSummaries));
            Add(checks, "immutable.values", "true", valuesReadOnly.ToString(), valuesReadOnly);
            Add(checks, "immutable.summaries", "true", summariesReadOnly.ToString(), summariesReadOnly);
            bool addRejected = MutationRejected(snapshot.CapabilityValues);
            Add(checks, "immutable.mutationRejected", "true", addRejected.ToString(), addRejected);
        }

        private static void AddIsolationChecks(
            ICollection<Check> checks,
            VerificationFixture fixture)
        {
            bool allIsolated = new[] { fixture.Complete, fixture.Sparse, fixture.SparseEmpty }
                .All(value => value.DevOnly && !value.IsEnabled && !value.EntersFormalFlow);
            Add(checks, "isolation.fixture", "true", allIsolated.ToString(), allIsolated);
        }

        private static void AddGapShapeChecks(
            ICollection<Check> checks,
            IReadOnlyList<CapabilityGapSnapshot> gaps)
        {
            bool fieldsStable = gaps.All(value =>
                !string.IsNullOrEmpty(value.RequirementGroupId)
                && !string.IsNullOrEmpty(value.BuildCapabilityKey)
                && Enum.IsDefined(
                    typeof(CapabilityValueAvailability),
                    value.CapabilityValueAvailability));
            Add(checks, "gap.shapeFields", "true", fieldsStable.ToString(), fieldsStable);
            Add(checks, "gap.knownAndUnknown", "Known;Unknown",
                string.Join(";", gaps.Select(value => value.CapabilityValueAvailability)
                    .Distinct().OrderBy(value => value).Select(value => value.ToString())),
                gaps.Any(value => value.CapabilityValueAvailability == CapabilityValueAvailability.Known)
                    && gaps.Any(value => value.CapabilityValueAvailability == CapabilityValueAvailability.Unknown));
        }

        private static void RunValidationChecks(
            List<Check> checks,
            E02BuildCapabilityResolver resolver,
            VerificationFixture fixture)
        {
            IBuildCapabilitySnapshotValidator validator =
                DefaultBuildCapabilitySnapshotValidator.Instance;
            ExpectCode(checks, "invalid.nullInput", validator.Validate(null, resolver), "INPUT_NULL");
            ExpectCode(checks, "invalid.nullResolver",
                validator.Validate(Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete), null), "RESOLVER_NULL");
            ExpectCode(checks, "invalid.schemaId",
                validator.Validate(new BuildCapabilitySnapshotInput(
                    CompleteId, SourceRevisionId, BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues), schemaId: "wrong"), resolver),
                "SCHEMA_ID_MISMATCH");
            ExpectCode(checks, "invalid.schemaVersion",
                validator.Validate(new BuildCapabilitySnapshotInput(
                    CompleteId, SourceRevisionId, BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues), schemaVersion: 2), resolver),
                "SCHEMA_VERSION_MISMATCH");
            ExpectCode(checks, "invalid.emptyId",
                validator.Validate(Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete, string.Empty), resolver), "ID_EMPTY");
            ExpectCode(checks, "invalid.idWhitespace",
                validator.Validate(Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete, " id "), resolver), "ID_OUTER_WHITESPACE");
            ExpectCode(checks, "invalid.idPath",
                validator.Validate(Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete, "Assets/path"), resolver), "ID_SENSITIVE_SEMANTIC");
            ExpectCode(checks, "invalid.revisionSlot",
                validator.Validate(new BuildCapabilitySnapshotInput(
                    CompleteId, "save_slot_1", BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues)), resolver), "ID_SENSITIVE_SEMANTIC");
            ExpectCode(checks, "invalid.coverage",
                validator.Validate(Input(CloneValues(fixture.Complete.CapabilityValues),
                    (BuildCapabilityCoverageMode)99), resolver), "COVERAGE_MODE_UNKNOWN");

            List<BuildCapabilityValueSnapshot> values = CloneValues(fixture.Complete.CapabilityValues);
            values[0] = null;
            ExpectCode(checks, "invalid.nullValue", validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver), "CAPABILITY_VALUE_NULL");
            values = CloneValues(fixture.Complete.CapabilityValues);
            values.Add(values[0]);
            ExpectCode(checks, "invalid.duplicateKey", validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver), "BUILD_CAPABILITY_KEY_DUPLICATE");
            values = CloneValues(fixture.Complete.CapabilityValues);
            values[0] = new BuildCapabilityValueSnapshot(string.Empty, 0, null);
            ExpectCode(checks, "invalid.emptyKey", validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver), "BUILD_CAPABILITY_KEY_EMPTY");
            values = CloneValues(fixture.Complete.CapabilityValues);
            values[0] = new BuildCapabilityValueSnapshot("mechanic.basic_pressure", 0, null);
            IReadOnlyList<BuildCapabilityValidationIssue> wrongCategory = validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver);
            ExpectCode(checks, "invalid.wrongKeyCategory", wrongCategory,
                "BUILD_CAPABILITY_KEY_CATEGORY_INVALID");
            ExpectCode(checks, "invalid.unknownKey", wrongCategory,
                "BUILD_CAPABILITY_KEY_UNKNOWN");
            values = CloneValues(fixture.Complete.CapabilityValues);
            BuildCapabilityValueSnapshot firstValue = values[0];
            values[0] = new BuildCapabilityValueSnapshot(
                firstValue.BuildCapabilityKey,
                -1,
                firstValue.SourceSummaries);
            ExpectCode(checks, "invalid.valueLow", validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver),
                "VALUE_BASIS_POINTS_OUT_OF_RANGE");
            values[0] = new BuildCapabilityValueSnapshot(
                firstValue.BuildCapabilityKey,
                10001,
                firstValue.SourceSummaries);
            ExpectCode(checks, "invalid.valueHigh", validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver),
                "VALUE_BASIS_POINTS_OUT_OF_RANGE");

            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.nullSummary",
                new BuildCapabilitySourceSummarySnapshot[] { null }, "SOURCE_SUMMARY_NULL");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.emptySource",
                new[] { new BuildCapabilitySourceSummarySnapshot(string.Empty, 1, 0, false) },
                "SOURCE_CATEGORY_ID_EMPTY");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.sourceWhitespace",
                new[] { new BuildCapabilitySourceSummarySnapshot(" source ", 1, 0, false) },
                "SOURCE_CATEGORY_ID_OUTER_WHITESPACE");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.duplicateSource",
                new[] {
                    new BuildCapabilitySourceSummarySnapshot("source.fixture.duplicate", 1, 0, false),
                    new BuildCapabilitySourceSummarySnapshot("source.fixture.duplicate", 2, 0, true) },
                "SOURCE_CATEGORY_ID_DUPLICATE");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.sourceCount",
                new[] { new BuildCapabilitySourceSummarySnapshot("source.fixture.count", 0, 0, false) },
                "SOURCE_COUNT_INVALID");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.contributionLow",
                new[] { new BuildCapabilitySourceSummarySnapshot("source.fixture.low", 1, -1, false) },
                "CONTRIBUTION_BASIS_POINTS_OUT_OF_RANGE");
            ExpectSourceIssue(checks, validator, resolver, fixture, "invalid.contributionHigh",
                new[] { new BuildCapabilitySourceSummarySnapshot("source.fixture.high", 1, 10001, false) },
                "CONTRIBUTION_BASIS_POINTS_OUT_OF_RANGE");

            values = CloneValues(fixture.Complete.CapabilityValues);
            values.RemoveAt(values.Count - 1);
            IReadOnlyList<BuildCapabilityValidationIssue> missing = validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver);
            ExpectCode(checks, "invalid.completeCount", missing, "COMPLETE_KEY_COUNT_MISMATCH");
            ExpectCode(checks, "invalid.completeMissing", missing, "COMPLETE_KEY_MISSING");
            Add(checks, "valid.sparseSubset", "0",
                validator.Validate(Input(values.Take(3).ToList(),
                    BuildCapabilityCoverageMode.Sparse), resolver).Count.ToString(CultureInfo.InvariantCulture),
                validator.Validate(Input(values.Take(3).ToList(),
                    BuildCapabilityCoverageMode.Sparse), resolver).Count == 0);
            Add(checks, "valid.sparseEmpty", "0",
                validator.Validate(new BuildCapabilitySnapshotInput(
                    SparseEmptyId, SourceRevisionId, BuildCapabilityCoverageMode.Sparse,
                    Array.Empty<BuildCapabilityValueSnapshot>()), resolver).Count.ToString(CultureInfo.InvariantCulture),
                validator.Validate(new BuildCapabilitySnapshotInput(
                    SparseEmptyId, SourceRevisionId, BuildCapabilityCoverageMode.Sparse,
                    Array.Empty<BuildCapabilityValueSnapshot>()), resolver).Count == 0);

            ExpectCode(checks, "invalid.devOnly", validator.Validate(
                new BuildCapabilitySnapshotInput(CompleteId, SourceRevisionId,
                    BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues), devOnly: false), resolver),
                "DEV_ONLY_REQUIRED");
            ExpectCode(checks, "invalid.enabled", validator.Validate(
                new BuildCapabilitySnapshotInput(CompleteId, SourceRevisionId,
                    BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues), isEnabled: true), resolver),
                "ENABLED_FORBIDDEN");
            ExpectCode(checks, "invalid.formalFlow", validator.Validate(
                new BuildCapabilitySnapshotInput(CompleteId, SourceRevisionId,
                    BuildCapabilityCoverageMode.Complete,
                    CloneValues(fixture.Complete.CapabilityValues), entersFormalFlow: true), resolver),
                "FORMAL_FLOW_FORBIDDEN");

            ExpectCode(checks, "invalid.resolverNullKeys", validator.Validate(
                Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete), new StubResolver(null, true)),
                "RESOLVER_KEYS_NULL");
            ExpectResolverCode(checks, validator, fixture, "invalid.resolverEmptyKey",
                new[] { string.Empty }, true, "RESOLVER_KEY_EMPTY");
            string validKey = resolver.GetKnownBuildCapabilityKeys()[0];
            ExpectResolverCode(checks, validator, fixture, "invalid.resolverDuplicate",
                new[] { validKey, validKey }, true, "RESOLVER_KEY_DUPLICATE");
            ExpectResolverCode(checks, validator, fixture, "invalid.resolverCategory",
                new[] { "mechanic.basic_pressure" }, true, "RESOLVER_KEY_CATEGORY_INVALID");
            ExpectResolverCode(checks, validator, fixture, "invalid.resolverUnresolved",
                new[] { "capability.unresolved_fixture" }, false, "RESOLVER_KEY_UNRESOLVED");
            bool providerThrows = false;
            try
            {
                Snapshot(Input(values, BuildCapabilityCoverageMode.Complete), resolver);
            }
            catch (BuildCapabilityValidationException exception)
            {
                providerThrows = exception.Issues.Count > 0;
            }
            Add(checks, "provider.rejectsInvalid", "true", providerThrows.ToString(), providerThrows);
        }

        private static void ExpectSourceIssue(
            ICollection<Check> checks,
            IBuildCapabilitySnapshotValidator validator,
            IBuildCapabilityVocabularyResolver resolver,
            VerificationFixture fixture,
            string id,
            IEnumerable<BuildCapabilitySourceSummarySnapshot> summaries,
            string code)
        {
            List<BuildCapabilityValueSnapshot> values = CloneValues(
                fixture.Complete.CapabilityValues);
            BuildCapabilityValueSnapshot original = values[0];
            values[0] = new BuildCapabilityValueSnapshot(
                original.BuildCapabilityKey,
                original.ValueBasisPoints,
                summaries);
            ExpectCode(checks, id, validator.Validate(
                Input(values, BuildCapabilityCoverageMode.Complete), resolver), code);
        }

        private static void ExpectResolverCode(
            ICollection<Check> checks,
            IBuildCapabilitySnapshotValidator validator,
            VerificationFixture fixture,
            string id,
            IReadOnlyList<string> keys,
            bool resolves,
            string code)
        {
            ExpectCode(checks, id, validator.Validate(
                Input(CloneValues(fixture.Complete.CapabilityValues),
                    BuildCapabilityCoverageMode.Complete),
                new StubResolver(keys, resolves)), code);
        }

        private static void ExpectCode(
            ICollection<Check> checks,
            string id,
            IReadOnlyList<BuildCapabilityValidationIssue> issues,
            string expectedCode)
        {
            string actual = string.Join(";", issues.Select(value => value.Code)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal));
            Add(checks, id, expectedCode, actual,
                issues.Any(value => value.Code == expectedCode));
        }

        private static void RunBaselineChecks(
            List<Check> checks,
            string root,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            Add(checks, "baseline.e02.total", "63",
                vocabulary.Entries.Count.ToString(CultureInfo.InvariantCulture),
                vocabulary.Entries.Count == 63);
            int capabilities = vocabulary.Entries.Count(
                value => value.Category == EnemyVocabularyCategory.BuildCapability);
            Add(checks, "baseline.e02.capabilities", "16",
                capabilities.ToString(CultureInfo.InvariantCulture), capabilities == 16);
            AddReportContains(checks, root, "baseline.e03.carriers",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "11 enemy / 7 boss / 18 total");
            AddReportContains(checks, root, "baseline.e03.profiles",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "10 enemy / 6 boss / 16 total");
            AddReportContains(checks, root, "baseline.e03.maps",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "MapRules: `10`");
            AddReportContains(checks, root, "baseline.e03.carrierBindings",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "Carrier bindings: `17`");
            AddReportContains(checks, root, "baseline.e03.mapBindings",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "MapRule bindings: `30`");
            AddReportContains(checks, root, "baseline.e03.exceptions",
                "Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md",
                "Intentional exceptions: `2`");
            AddReportContains(checks, root, "baseline.e04.fixture",
                "Docs/V0.4/Reports/EncounterCompositionSchemaReport.md",
                "2 Encounter / 4 Wave / 6 Slot");
            AddReportContains(checks, root, "baseline.e05.fixture",
                "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaReport.md",
                "| SkillPattern | 4 |");
            AddReportContains(checks, root, "baseline.e05.sequence",
                "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaReport.md",
                "| SkillSequence | 3 |");
            AddReportContains(checks, root, "baseline.e05.bindingPhasePlan",
                "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaReport.md",
                "| CarrierSkillBinding | 3 |");
            AddReportContains(checks, root, "baseline.e06.fixture",
                "Docs/V0.4/Reports/CounterWindowAndPressureSchemaReport.md",
                "RequirementGroup / Requirement: `7 / 11`");
            AddReportContains(checks, root, "baseline.e06.bindings",
                "Docs/V0.4/Reports/CounterWindowAndPressureSchemaReport.md",
                "`6 / 4 / 5`");
            AddHashChecks(checks, root, "protected", ProtectedHashes);
            AddHashChecks(checks, root, "legacy", LegacyHashes);
        }

        private static void AddReportContains(
            ICollection<Check> checks,
            string root,
            string id,
            string relative,
            string expected)
        {
            string path = Absolute(root, relative);
            string text = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
            bool passed = text.Contains(expected, StringComparison.Ordinal);
            Add(checks, id, expected, passed ? expected : "missing", passed);
        }

        private static void RunLeakChecks(List<Check> checks, string root)
        {
            int dependencyLeaks = 0;
            int evaluationLeaks = 0;
            foreach (string relative in RuntimeFiles)
            {
                string text = File.ReadAllText(Absolute(root, relative));
                foreach (string token in DependencyForbiddenTokens)
                {
                    int count = Count(text, token);
                    dependencyLeaks += count;
                    Add(checks, "leak.dependency." + Path.GetFileName(relative) + "." + SafeId(token),
                        "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
                }
                foreach (string token in EvaluationForbiddenTokens)
                {
                    int count = Count(text, token);
                    evaluationLeaks += count;
                    Add(checks, "leak.evaluation." + Path.GetFileName(relative) + "." + SafeId(token),
                        "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
                }
            }
            Add(checks, "leak.dependency.total", "0",
                dependencyLeaks.ToString(CultureInfo.InvariantCulture), dependencyLeaks == 0);
            Add(checks, "leak.evaluation.total", "0",
                evaluationLeaks.ToString(CultureInfo.InvariantCulture), evaluationLeaks == 0);
        }

        private static void RunGeneratedFileChecks(List<Check> checks, string root)
        {
            int existing = ExpectedPackageFiles.Count(value => File.Exists(Absolute(root, value)));
            Add(checks, "package.expectedFiles", "16",
                existing.ToString(CultureInfo.InvariantCulture), existing == 16);
            string capabilityRoot = Absolute(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead");
            int capabilityFiles = Directory.Exists(capabilityRoot)
                ? Directory.GetFiles(capabilityRoot, "*", SearchOption.TopDirectoryOnly).Length
                : 0;
            Add(checks, "package.capabilityReadFiles", "8",
                capabilityFiles.ToString(CultureInfo.InvariantCulture), capabilityFiles == 8);
            string reports = Absolute(root, ReportRoot);
            int reportFiles = Directory.GetFiles(
                reports,
                "BuildCapabilityReadContract*",
                SearchOption.TopDirectoryOnly).Length;
            Add(checks, "package.reportFiles", "5",
                reportFiles.ToString(CultureInfo.InvariantCulture), reportFiles == 5);
            string editorRoot = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem");
            int editorFiles = Directory.GetFiles(
                editorRoot,
                "BuildCapabilityReadContractVerifier.cs*",
                SearchOption.TopDirectoryOnly).Length;
            Add(checks, "package.editorFiles", "2",
                editorFiles.ToString(CultureInfo.InvariantCulture), editorFiles == 2);

            int trailing = ExpectedPackageFiles
                .Where(value => File.Exists(Absolute(root, value)))
                .Sum(value => CountTrailingWhitespace(Absolute(root, value)));
            Add(checks, "package.trailingWhitespace", "0",
                trailing.ToString(CultureInfo.InvariantCulture), trailing == 0);

            string[] allMeta = Directory.GetFiles(
                Absolute(root, "Assets"),
                "*.meta",
                SearchOption.AllDirectories);
            bool guidPass = true;
            foreach (string relative in ExpectedPackageFiles.Where(
                value => value.EndsWith(".meta", StringComparison.Ordinal)))
            {
                string guid = ReadMetaGuid(Absolute(root, relative));
                int count = allMeta.Count(path => string.Equals(
                    ReadMetaGuid(path),
                    guid,
                    StringComparison.Ordinal));
                bool unique = guid.Length == 32 && count == 1;
                guidPass &= unique;
                Add(checks, "package.guid." + Path.GetFileName(relative), "unique",
                    guid + " x" + count.ToString(CultureInfo.InvariantCulture), unique);
            }
            Add(checks, "package.guidConflicts", "0", guidPass ? "0" : "non-zero", guidPass);

            ProcessResult diff = RunProcess(root, "git", "diff --check");
            Add(checks, "package.gitDiffCheck", "exit 0",
                "exit " + diff.ExitCode.ToString(CultureInfo.InvariantCulture),
                diff.ExitCode == 0);
            string quotedPaths = string.Join(" ", ExpectedPackageFiles.Select(
                value => "\"" + value + "\""));
            ProcessResult tracked = RunProcess(root, "git", "ls-files -- " + quotedPaths);
            int trackedCount = tracked.StandardOutput.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries).Length;
            Add(checks, "package.modifiedExistingFiles", "0",
                trackedCount.ToString(CultureInfo.InvariantCulture),
                tracked.ExitCode == 0 && trackedCount == 0);
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<Check> checks,
            VerificationFixture fixture)
        {
            WriteUtf8(Absolute(root, ReportRoot + "BuildCapabilityReadContractReport.md"),
                BuildMainReport(mode, checks, fixture));
            WriteUtf8(Absolute(root, ReportRoot + "BuildCapabilityReadContractSpec.csv"),
                BuildSpec(checks));
            WriteUtf8(Absolute(root, ReportRoot + "BuildCapabilityReadContractFieldMatrix.csv"),
                BuildFieldMatrix());
            WriteUtf8(Absolute(root, ReportRoot + "BuildCapabilityReadContractFixtureRows.csv"),
                BuildFixtureRows(fixture));
            WriteUtf8(Absolute(root, ReportRoot + "BuildCapabilityReadContractLeakCheckReport.md"),
                BuildLeakReport(checks));
        }

        private static string BuildMainReport(
            string mode,
            IReadOnlyList<Check> checks,
            VerificationFixture fixture)
        {
            int passed = checks.Count(value => value.Passed);
            int summaries = fixture.Complete.CapabilityValues.Sum(value => value.SourceSummaries.Count);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Build Capability Read Contract Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-BuildCapabilityReadContract01`")
                .AppendLine("- Guard marker: `ENEMY_GUARD_ASSIGNMENT_BUILDCAPABILITYREADCONTRACT01`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Schema: `" + fixture.Complete.SchemaId + "` / `" + fixture.Complete.SchemaVersion + "`")
                .AppendLine("- Result: `" + (checks.Count > 0 && checks.All(value => value.Passed) ? "PASS" : "FAIL") + "`")
                .AppendLine("- Checks: `" + passed + "/" + checks.Count + "` passed")
                .AppendLine()
                .AppendLine("## Fixture inventory")
                .AppendLine()
                .AppendLine("- Known BuildCapability keys: `" + fixture.Complete.CapabilityValues.Count + "`")
                .AppendLine("- Complete / Sparse / Sparse Empty snapshots: `1 / 1 / 1`")
                .AppendLine("- CapabilityValue: `" + (fixture.Complete.CapabilityValues.Count + fixture.Sparse.CapabilityValues.Count) + "`")
                .AppendLine("- SourceSummary in Complete snapshot: `" + summaries + "`")
                .AppendLine("- CapabilityGap shapes: `" + fixture.Gaps.Count + "`")
                .AppendLine("- ReadinessBand values: `5`")
                .AppendLine()
                .AppendLine("## Contract semantics")
                .AppendLine()
                .AppendLine("- Complete coverage: `" + Result(checks, "fixture.completeValues") + "`")
                .AppendLine("- Sparse unknown preservation: `" + Result(checks, "query.unknown") + "`")
                .AppendLine("- Known Zero vs Unknown: `" + Result(checks, "query.knownZero") + " / " + Result(checks, "query.unknown") + "`")
                .AppendLine("- Ordinal lookup: `" + Result(checks, "query.ordinal") + "`")
                .AppendLine("- Item-independent dependency scan: `" + Result(checks, "leak.dependency.total") + "`")
                .AppendLine("- CanonicalSignature: `" + fixture.Complete.CanonicalSignature + "`")
                .AppendLine()
                .AppendLine("This package stores externally aggregated abstract capability values only. It does not calculate capabilities, compare requirement thresholds, generate readiness, expose a player projection, or connect runtime systems.")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                builder.Append("| `").Append(EscapeMarkdown(check.Id)).Append("` | `")
                    .Append(EscapeMarkdown(check.Expected)).Append("` | `")
                    .Append(EscapeMarkdown(check.Actual)).Append("` | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            return builder.ToString();
        }

        private static string BuildSpec(IReadOnlyList<Check> checks)
        {
            return CsvTable(
                new[] { "checkId", "expected", "actual", "result" },
                checks.Select(value => new[] {
                    value.Id,
                    value.Expected,
                    value.Actual,
                    value.Passed ? "PASS" : "FAIL" }));
        }

        private static string BuildFieldMatrix()
        {
            string[][] rows =
            {
                new[] { "BuildCapabilitySnapshot", "SchemaId", "string", "1", "Schema identity", "enemy read-only input", "E07", "E08" },
                new[] { "BuildCapabilitySnapshot", "SchemaVersion", "int", "1", "Schema version", "enemy read-only input", "E07", "E08" },
                new[] { "BuildCapabilitySnapshot", "SnapshotId", "string", "1", "Opaque snapshot identity", "enemy read-only input", "external adapter", "E08" },
                new[] { "BuildCapabilitySnapshot", "SourceRevisionId", "string", "1", "Opaque source revision", "enemy read-only input", "external adapter", "E08" },
                new[] { "BuildCapabilitySnapshot", "CoverageMode", "enum", "1", "Complete or Sparse coverage", "enemy read-only input", "external adapter", "E08" },
                new[] { "BuildCapabilitySnapshot", "CapabilityValues", "BuildCapabilityValueSnapshot[]", "0..N", "Known abstract capability values", "developer-only diagnostics", "external adapter", "E08" },
                new[] { "BuildCapabilitySnapshot", "CanonicalSignature", "string", "1", "Deterministic full signature", "developer-only diagnostics", "E07", "validator" },
                new[] { "BuildCapabilityValueSnapshot", "BuildCapabilityKey", "string", "1", "E02 BuildCapability stable key", "enemy read-only input", "E02", "E08" },
                new[] { "BuildCapabilityValueSnapshot", "ValueBasisPoints", "int", "1", "Known value in 0..10000", "enemy read-only input", "external adapter", "E08" },
                new[] { "BuildCapabilityValueSnapshot", "SourceSummaries", "BuildCapabilitySourceSummarySnapshot[]", "0..N", "Aggregated diagnostic provenance", "developer-only diagnostics", "external adapter", "offline reports" },
                new[] { "BuildCapabilitySourceSummarySnapshot", "SourceCategoryId", "string", "1", "Opaque aggregate category", "developer-only diagnostics", "external adapter", "offline reports" },
                new[] { "BuildCapabilitySourceSummarySnapshot", "SourceCount", "int", "1", "Aggregate source count", "developer-only diagnostics", "external adapter", "offline reports" },
                new[] { "BuildCapabilitySourceSummarySnapshot", "ContributionBasisPoints", "int", "1", "Diagnostic contribution only", "developer-only diagnostics", "external adapter", "offline reports" },
                new[] { "BuildCapabilitySourceSummarySnapshot", "IsConditional", "bool", "1", "Conditional aggregate marker", "developer-only diagnostics", "external adapter", "offline reports" },
                new[] { "CapabilityGapSnapshot", "RequirementGroupId", "string", "1", "Future requirement group identity", "developer-only diagnostics", "E08", "offline reports" },
                new[] { "CapabilityGapSnapshot", "BuildCapabilityKey", "string", "1", "Compared abstract capability", "developer-only diagnostics", "E02", "E08" },
                new[] { "CapabilityGapSnapshot", "CapabilityValueAvailability", "enum", "1", "Known or Unknown availability", "developer-only diagnostics", "E08", "offline reports" },
                new[] { "CapabilityGapSnapshot", "AvailableBasisPoints", "int", "1", "Future evaluator output shape", "developer-only diagnostics", "E08", "offline reports" },
                new[] { "CapabilityGapSnapshot", "RequiredBasisPoints", "int", "1", "Future evaluator threshold shape", "developer-only diagnostics", "E06", "E08" },
                new[] { "CapabilityGapSnapshot", "GapBasisPoints", "int", "1", "Future evaluator output shape", "developer-only diagnostics", "E08", "offline reports" },
                new[] { "ReadinessBand", "Value", "enum", "5", "Unknown Blocked Strained Ready Strong", "developer-only diagnostics", "E07 shape", "E08" }
            };
            return CsvTable(
                new[] { "ownerType", "fieldName", "fieldType", "cardinality", "semantic", "visibility", "sourceOfTruth", "futureConsumer" },
                rows);
        }

        private static string BuildFixtureRows(VerificationFixture fixture)
        {
            List<string[]> rows = new List<string[]>();
            AddSnapshotRows(rows, fixture.Complete);
            AddSnapshotRows(rows, fixture.Sparse);
            rows.Add(new[] { "Snapshot", fixture.SparseEmpty.SnapshotId,
                fixture.SparseEmpty.CoverageMode.ToString(), string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, "Unknown", "true" });
            foreach (CapabilityGapSnapshot gap in fixture.Gaps)
            {
                rows.Add(new[] { "CapabilityGap", string.Empty, string.Empty,
                    gap.BuildCapabilityKey,
                    gap.AvailableBasisPoints.ToString(CultureInfo.InvariantCulture),
                    gap.RequirementGroupId,
                    string.Empty,
                    gap.RequiredBasisPoints.ToString(CultureInfo.InvariantCulture),
                    gap.CapabilityValueAvailability.ToString(), "true" });
            }
            return CsvTable(
                new[] { "rowKind", "snapshotId", "coverageMode", "buildCapabilityKey", "valueBasisPoints", "sourceCategoryId", "sourceCount", "contributionBasisPoints", "availability", "fixtureOnly" },
                rows);
        }

        private static void AddSnapshotRows(
            ICollection<string[]> rows,
            BuildCapabilitySnapshot snapshot)
        {
            foreach (BuildCapabilityValueSnapshot value in snapshot.CapabilityValues)
            {
                rows.Add(new[] { "CapabilityValue", snapshot.SnapshotId,
                    snapshot.CoverageMode.ToString(), value.BuildCapabilityKey,
                    value.ValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                    string.Empty, string.Empty, string.Empty, "Known", "true" });
                foreach (BuildCapabilitySourceSummarySnapshot summary in value.SourceSummaries)
                {
                    rows.Add(new[] { "SourceSummary", snapshot.SnapshotId,
                        snapshot.CoverageMode.ToString(), value.BuildCapabilityKey,
                        value.ValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                        summary.SourceCategoryId,
                        summary.SourceCount.ToString(CultureInfo.InvariantCulture),
                        summary.ContributionBasisPoints.ToString(CultureInfo.InvariantCulture),
                        "Known", "true" });
                }
            }
        }

        private static string BuildLeakReport(IReadOnlyList<Check> checks)
        {
            Check[] selected = checks.Where(value =>
                    value.Id.StartsWith("leak.", StringComparison.Ordinal)
                    || value.Id.StartsWith("protected.", StringComparison.Ordinal)
                    || value.Id.StartsWith("legacy.", StringComparison.Ordinal)
                    || value.Id.StartsWith("package.", StringComparison.Ordinal)
                    || value.Id.StartsWith("baseline.", StringComparison.Ordinal))
                .ToArray();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Build Capability Read Contract Leak Check Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-BuildCapabilityReadContract01`")
                .AppendLine("- Result: `" + (selected.Length > 0 && selected.All(value => value.Passed) ? "PASS" : "FAIL") + "`")
                .AppendLine("- Runtime dependency references: `" + Result(checks, "leak.dependency.total") + "`")
                .AppendLine("- Evaluator / threshold / projection code: `" + Result(checks, "leak.evaluation.total") + "`")
                .AppendLine("- E01-E06 protected hashes: `" + CountResult(checks, "protected.") + "`")
                .AppendLine("- Legacy source hashes: `" + CountResult(checks, "legacy.") + "`")
                .AppendLine("- GUID conflicts: `" + Result(checks, "package.guidConflicts") + "`")
                .AppendLine("- Trailing whitespace: `" + Result(checks, "package.trailingWhitespace") + "`")
                .AppendLine()
                .AppendLine("## Detailed checks")
                .AppendLine();
            foreach (Check check in selected)
            {
                builder.Append("- `").Append(check.Id).Append("`: `")
                    .Append(check.Passed ? "PASS" : "FAIL")
                    .Append("` (expected `").Append(EscapeMarkdown(check.Expected))
                    .Append("`, actual `").Append(EscapeMarkdown(check.Actual))
                    .AppendLine("`)");
            }
            return builder.ToString();
        }

        private static BuildCapabilitySnapshotInput Input(
            IEnumerable<BuildCapabilityValueSnapshot> values,
            BuildCapabilityCoverageMode mode,
            string snapshotId = CompleteId)
        {
            return new BuildCapabilitySnapshotInput(
                snapshotId,
                SourceRevisionId,
                mode,
                values);
        }

        private static List<BuildCapabilityValueSnapshot> CloneValues(
            IEnumerable<BuildCapabilityValueSnapshot> values)
        {
            return values.Select(value => value == null
                    ? null
                    : new BuildCapabilityValueSnapshot(
                        value.BuildCapabilityKey,
                        value.ValueBasisPoints,
                        value.SourceSummaries.Select(summary => summary == null
                            ? null
                            : new BuildCapabilitySourceSummarySnapshot(
                                summary.SourceCategoryId,
                                summary.SourceCount,
                                summary.ContributionBasisPoints,
                                summary.IsConditional))))
                .ToList();
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values)
        {
            return values is IList<T> list && list.IsReadOnly;
        }

        private static bool MutationRejected<T>(IReadOnlyList<T> values)
        {
            if (!(values is IList<T> list))
            {
                return true;
            }
            try
            {
                list.Add(default(T));
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static void AddHashChecks(
            ICollection<Check> checks,
            string root,
            string prefix,
            IReadOnlyDictionary<string, string> expected)
        {
            foreach (KeyValuePair<string, string> pair in expected)
            {
                string path = Absolute(root, pair.Key);
                string actual = File.Exists(path) ? Sha256(path) : "missing";
                Add(checks, prefix + "." + Path.GetFileName(pair.Key), pair.Value,
                    actual, string.Equals(pair.Value, actual, StringComparison.Ordinal));
            }
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return string.Concat(sha.ComputeHash(stream)
                    .Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        private static string CsvTable(
            IReadOnlyList<string> headers,
            IEnumerable<IReadOnlyList<string>> rows)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Join(",", headers.Select(Csv)));
            foreach (IReadOnlyList<string> row in rows)
            {
                builder.AppendLine(string.Join(",", row.Select(Csv)));
            }
            return builder.ToString();
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string Result(IReadOnlyList<Check> checks, string id)
        {
            Check check = checks.LastOrDefault(value => value.Id == id);
            return check != null && check.Passed ? "PASS" : "FAIL";
        }

        private static string CountResult(IReadOnlyList<Check> checks, string prefix)
        {
            Check[] selected = checks.Where(value => value.Id.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            return selected.Count(value => value.Passed) + "/" + selected.Length + " PASS";
        }

        private static int Count(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
            {
                return 0;
            }
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int CountTrailingWhitespace(string path)
        {
            if (!File.Exists(path) || path.EndsWith(".meta", StringComparison.Ordinal))
            {
                return 0;
            }
            return File.ReadAllLines(path).Count(line => line.Length > 0
                && (line[line.Length - 1] == ' ' || line[line.Length - 1] == '\t'));
        }

        private static string ReadMetaGuid(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            string line = File.ReadLines(path).FirstOrDefault(
                value => value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring("guid: ".Length).Trim();
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("`", "'")
                .Replace("\r", " ").Replace("\n", " ");
        }

        private static string SafeId(string value)
        {
            return new string((value ?? string.Empty).Select(character =>
                char.IsLetterOrDigit(character) ? character : '_').ToArray());
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static ProcessResult RunProcess(
            string root,
            string fileName,
            string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = root,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    return new ProcessResult(process.ExitCode, output, error);
                }
            }
            catch (Exception exception)
            {
                return new ProcessResult(-1, string.Empty, exception.Message);
            }
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Packages")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static bool IsBatchMode()
        {
#if UNITY_EDITOR
            return Application.isBatchMode;
#else
            return false;
#endif
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

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id ?? string.Empty;
                Expected = expected ?? string.Empty;
                Actual = actual ?? string.Empty;
                Passed = passed;
            }

            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class VerificationFixture
        {
            public VerificationFixture(
                BuildCapabilitySnapshot complete,
                BuildCapabilitySnapshot sparse,
                BuildCapabilitySnapshot sparseEmpty,
                IReadOnlyList<CapabilityGapSnapshot> gaps)
            {
                Complete = complete;
                Sparse = sparse;
                SparseEmpty = sparseEmpty;
                Gaps = gaps;
            }

            public BuildCapabilitySnapshot Complete { get; }
            public BuildCapabilitySnapshot Sparse { get; }
            public BuildCapabilitySnapshot SparseEmpty { get; }
            public IReadOnlyList<CapabilityGapSnapshot> Gaps { get; }
        }

        private sealed class ProcessResult
        {
            public ProcessResult(int exitCode, string standardOutput, string standardError)
            {
                ExitCode = exitCode;
                StandardOutput = standardOutput ?? string.Empty;
                StandardError = standardError ?? string.Empty;
            }

            public int ExitCode { get; }
            public string StandardOutput { get; }
            public string StandardError { get; }
        }

        private sealed class E02BuildCapabilityResolver :
            IBuildCapabilityVocabularyResolver
        {
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly ReadOnlyCollection<string> knownKeys;

            public E02BuildCapabilityResolver(
                EnemyMechanicVocabularySnapshot vocabulary)
            {
                this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
                knownKeys = Array.AsReadOnly(vocabulary.Entries
                    .Where(value => value.Category == EnemyVocabularyCategory.BuildCapability)
                    .Select(value => value.StableKey)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray());
            }

            public bool HasBuildCapabilityKey(string key)
            {
                return key != null && vocabulary.TryGetEntry(
                    EnemyVocabularyCategory.BuildCapability,
                    key,
                    out _);
            }

            public IReadOnlyList<string> GetKnownBuildCapabilityKeys()
            {
                return knownKeys;
            }
        }

        private sealed class StubResolver : IBuildCapabilityVocabularyResolver
        {
            private readonly IReadOnlyList<string> keys;
            private readonly bool resolves;

            public StubResolver(IReadOnlyList<string> keys, bool resolves)
            {
                this.keys = keys;
                this.resolves = resolves;
            }

            public bool HasBuildCapabilityKey(string key)
            {
                return resolves;
            }

            public IReadOnlyList<string> GetKnownBuildCapabilityKeys()
            {
                return keys;
            }
        }
    }
}
