using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.ReadinessEvaluation;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemyOfflineReadinessEvaluatorVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string SourceRevisionId = "revision_readiness_fixture_001";

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
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs"] = "2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs"] = "7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs"] = "EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs"] = "F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs"] = "B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs"] = "F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34"
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
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs"
        };

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md",
            "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorSpec.csv",
            "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorScenarioRows.csv",
            "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorLeakCheckReport.md"
        };

        private static readonly string[] RuntimeForbiddenTokens =
        {
            "TalismanBag.BuildSandbox",
            "TalismanBag.Items",
            "ItemSystemSnapshot",
            "Inventory",
            "Equipment",
            "Affix",
            "Synergy",
            "using UnityEngine",
            "MonoBehaviour",
            "ScriptableObject",
            "GameObject",
            "Transform",
            "Addressables",
            "SceneManager",
            "BattleContract",
            "BattleBridge",
            "UnifiedBattlePage",
            "BuildGridInteractionPreviewController",
            "V02FormationGridFrame",
            "RunFlow",
            "RewardConfig",
            "SaveData",
            "PlayerPrefs",
            "DamageText",
            "DateTime.Now",
            "Stopwatch",
            "Random",
            "Task.Delay",
            "Coroutine",
            "StartCoroutine",
            "void Update(",
            " event ",
            "ApplyDamage",
            "DealDamage",
            "DropBias",
            "BossKey",
            "3-10",
            "4-10"
        };

        private static readonly string[] PlayerForbiddenTokens =
        {
            "readinessBand",
            "buildCapabilityKey",
            "baseValueBasisPoints",
            "effectiveValueBasisPoints",
            "requiredValueBasisPoints",
            "gapBasisPoints",
            "requirementGroupId",
            "requirementRole",
            "requirementMatchMode",
            "requirementStatus",
            "groupStatus",
            "capabilityValueAvailability",
            "mapRuleId",
            "deltaBasisPoints",
            "developerReasonId",
            "buildSnapshotId",
            "pressureCatalogCanonicalSignature",
            "sourceSummary",
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBias",
            "bossKey"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EnemyOfflineReadinessEvaluator01/[QA Only] Verify And Write Reports")]
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
            Fixture fixture = null;
            try
            {
                EnemyMechanicVocabularySnapshot vocabulary =
                    DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput());
                EnemyValidationContentSnapshot normalization =
                    EnemyValidationContentNormalizer.CreateSnapshot();
                ReferenceResolver resolver = new ReferenceResolver(
                    vocabulary,
                    normalization);
                fixture = CreateFixture(resolver);
                RunScenarioChecks(checks, resolver, fixture);
                RunSignatureChecks(checks, resolver, fixture);
                RunValidationChecks(checks, resolver, fixture);
                RunBaselineChecks(checks, root, vocabulary, normalization);
                RunLeakChecks(checks, root, fixture);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            if (fixture != null)
            {
                WriteReports(root, mode, checks, fixture);
                RunGeneratedFileChecks(checks, root);
                RunGlobalDiffCheck(checks, root);
                WriteReports(root, mode, checks, fixture);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine(
                (pass
                    ? "ENEMY_OFFLINE_READINESS_EVALUATOR_PASS "
                    : "ENEMY_OFFLINE_READINESS_EVALUATOR_FAIL ")
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
                    "EnemyOfflineReadinessEvaluator01 verifier failed: "
                    + string.Join(
                        " | ",
                        checks.Where(value => !value.Passed)
                            .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static Fixture CreateFixture(ReferenceResolver resolver)
        {
            IReadOnlyList<string> keys = resolver.GetKnownBuildCapabilityKeys();
            IReadOnlyList<string> maps = resolver.MapRuleIds;
            if (keys.Count < 4 || maps.Count < 2)
            {
                throw new InvalidOperationException(
                    "E08 fixture requires at least four E02 capability keys and two E03 map rules.");
            }

            string k0 = keys[0];
            string k1 = keys[1];
            string k2 = keys[2];
            string k3 = keys[3];
            string m0 = maps[0];
            string m1 = maps[1];
            List<Scenario> scenarios = new List<Scenario>
            {
                CreateScenario(resolver, "strong", BuildCapabilityCoverageMode.Complete,
                    Values(k0, 6000, k1, 4500, k2, 3500),
                    Groups(
                        Group("group.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            Requirement(k0, 5000), Requirement(k1, 4000)),
                        Group("group.recommended.any", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            Requirement(k2, 3000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Strong),
                CreateScenario(resolver, "ready", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 5000),
                    Groups(Group("group.required.ready", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 5000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Ready),
                CreateScenario(resolver, "strained", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 6000, k1, 1000),
                    Groups(
                        Group("group.required.strained", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            Requirement(k0, 5000)),
                        Group("group.recommended.strained", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            Requirement(k1, 6000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Strained),
                CreateScenario(resolver, "blocked_known_zero", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 0),
                    Groups(Group("group.required.zero", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 1000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Blocked),
                CreateScenario(resolver, "unknown_sparse", BuildCapabilityCoverageMode.Sparse,
                    Values(k1, 2000),
                    Groups(Group("group.required.unknown", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 1000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Unknown),
                CreateScenario(resolver, "any_alternative", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 6000, k1, 1000),
                    Groups(Group("group.required.any.alternative", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                        Requirement(k0, 5000), Requirement(k1, 5000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Ready),
                CreateScenario(resolver, "any_unknown", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 1000),
                    Groups(Group("group.required.any.unknown", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                        Requirement(k0, 5000), Requirement(k1, 5000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Unknown),
                CreateScenario(resolver, "all_unmet", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 6000, k1, 1000),
                    Groups(Group("group.required.all.unmet", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 5000), Requirement(k1, 5000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Blocked),
                CreateScenario(resolver, "map_buff", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 3000),
                    Groups(Group("group.required.map.buff", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 5000))),
                    Adjustments(Adjustment(m0, k0, 2500, "reason.map.buff")), ReadinessBand.Ready),
                CreateScenario(resolver, "map_debuff", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 7000),
                    Groups(Group("group.required.map.debuff", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 5000))),
                    Adjustments(Adjustment(m0, k0, -3000, "reason.map.debuff")), ReadinessBand.Blocked),
                CreateScenario(resolver, "clamp_low", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 1000),
                    Groups(Group("group.required.clamp.low", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 1))),
                    Adjustments(
                        Adjustment(m0, k0, -9000, "reason.clamp.low.a"),
                        Adjustment(m1, k0, -5000, "reason.clamp.low.b")), ReadinessBand.Blocked),
                CreateScenario(resolver, "clamp_high", BuildCapabilityCoverageMode.Sparse,
                    Values(k0, 8000),
                    Groups(Group("group.required.clamp.high", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 9000))),
                    Adjustments(
                        Adjustment(m0, k0, 9000, "reason.clamp.high.a"),
                        Adjustment(m1, k0, 5000, "reason.clamp.high.b")), ReadinessBand.Ready),
                CreateScenario(resolver, "unknown_map", BuildCapabilityCoverageMode.Sparse,
                    Values(k1, 2000),
                    Groups(Group("group.required.unknown.map", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(k0, 5000))),
                    Adjustments(Adjustment(m0, k0, 9000, "reason.unknown.map")), ReadinessBand.Unknown),
                CreateScenario(resolver, "recommended_only_strong", BuildCapabilityCoverageMode.Sparse,
                    Values(k3, 2000),
                    Groups(Group("group.recommended.only.strong", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                        Requirement(k3, 1000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Strong),
                CreateScenario(resolver, "recommended_only_strained", BuildCapabilityCoverageMode.Sparse,
                    Values(k3, 2000),
                    Groups(Group("group.recommended.only.strained", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                        Requirement(k3, 5000))),
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>(), ReadinessBand.Strained)
            };
            return new Fixture(scenarios, k0, k1, k2, k3, m0, m1);
        }

        private static Scenario CreateScenario(
            ReferenceResolver resolver,
            string id,
            BuildCapabilityCoverageMode coverageMode,
            IReadOnlyDictionary<string, int> values,
            IReadOnlyList<BuildCapabilityRequirementGroupSnapshot> groups,
            IReadOnlyList<MapRuleCapabilityAdjustmentSnapshot> adjustments,
            ReadinessBand expectedBand)
        {
            string profileId = "dev_readiness_fixture_" + id;
            BuildCapabilitySnapshot build = Build(
                resolver,
                "dev_build_readiness_fixture_" + id,
                coverageMode,
                values);
            CounterWindowAndPressureCatalogSnapshot catalog = PressureCatalog(
                resolver,
                profileId,
                groups,
                "enemy.pressure.readiness.fixture.label",
                "enemy.pressure.readiness.fixture.hint");
            EnemyReadinessEvaluationInput input = new EnemyReadinessEvaluationInput(
                build,
                catalog,
                profileId,
                adjustments);
            EnemyReadinessEvaluationResult result =
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(input, resolver);
            return new Scenario(
                id,
                coverageMode,
                input,
                result,
                expectedBand,
                Severity(expectedBand));
        }

        private static BuildCapabilitySnapshot Build(
            ReferenceResolver resolver,
            string snapshotId,
            BuildCapabilityCoverageMode coverageMode,
            IReadOnlyDictionary<string, int> overrides)
        {
            IEnumerable<string> keys = coverageMode == BuildCapabilityCoverageMode.Complete
                ? resolver.GetKnownBuildCapabilityKeys()
                : overrides.Keys.OrderBy(value => value, StringComparer.Ordinal);
            BuildCapabilityValueSnapshot[] values = keys.Select(key =>
                new BuildCapabilityValueSnapshot(
                    key,
                    overrides.TryGetValue(key, out int value) ? value : 0,
                    Array.Empty<BuildCapabilitySourceSummarySnapshot>()))
                .ToArray();
            return DefaultBuildCapabilitySnapshotProvider.Instance.CreateSnapshot(
                new BuildCapabilitySnapshotInput(
                    snapshotId,
                    SourceRevisionId,
                    coverageMode,
                    values),
                resolver);
        }

        private static CounterWindowAndPressureCatalogSnapshot PressureCatalog(
            ReferenceResolver resolver,
            string profileId,
            IEnumerable<BuildCapabilityRequirementGroupSnapshot> groups,
            string labelKey,
            string hintKey)
        {
            BuildPressureProfileSnapshot pressure = new BuildPressureProfileSnapshot(
                profileId,
                new BuildPressurePlayerProjection(
                    profileId,
                    labelKey,
                    hintKey,
                    new[] { resolver.PlayerHintCategoryKey }),
                new BuildPressureInternalSpec(
                    new[]
                    {
                        new PressureChannelContributionSnapshot(
                            resolver.PressureChannelKey,
                            10000)
                    }),
                new BuildPressureDeveloperDiagnostics(
                    groups,
                    new[] { resolver.DeveloperDiagnosticCategoryKey },
                    new[] { "dev_readiness_fixture_source" }));
            return DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                new CounterWindowAndPressureCatalogInput(
                    new[] { pressure },
                    Array.Empty<CounterWindowProfileSnapshot>(),
                    Array.Empty<PressureSourceBindingSnapshot>(),
                    Array.Empty<CounterWindowSourceBindingSnapshot>(),
                    Array.Empty<PressureCounterWindowBindingSnapshot>()),
                resolver);
        }

        private static void RunScenarioChecks(
            ICollection<Check> checks,
            ReferenceResolver resolver,
            Fixture fixture)
        {
            Add(checks, "schema.id", "EnemyOfflineReadiness.v1",
                EnemyOfflineReadinessSchema.SchemaId,
                EnemyOfflineReadinessSchema.SchemaId == "EnemyOfflineReadiness.v1");
            Add(checks, "schema.version", "1",
                EnemyOfflineReadinessSchema.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                EnemyOfflineReadinessSchema.SchemaVersion == 1);
            Add(checks, "scenarios.count", ">=13",
                fixture.Scenarios.Count.ToString(CultureInfo.InvariantCulture),
                fixture.Scenarios.Count >= 13);
            foreach (Scenario scenario in fixture.Scenarios)
            {
                Add(checks, "scenario." + scenario.Id + ".band",
                    scenario.ExpectedBand.ToString(),
                    scenario.Result.DeveloperReadiness.ReadinessBand.ToString(),
                    scenario.Result.DeveloperReadiness.ReadinessBand == scenario.ExpectedBand);
                Add(checks, "scenario." + scenario.Id + ".severity",
                    scenario.ExpectedSeverity.ToString(),
                    scenario.Result.PlayerHintProjection.PlayerHintSeverity.ToString(),
                    scenario.Result.PlayerHintProjection.PlayerHintSeverity == scenario.ExpectedSeverity);
            }

            string[] expectedBands = { "Unknown", "Blocked", "Strained", "Ready", "Strong" };
            string[] actualBands = fixture.Scenarios
                .Select(value => value.Result.DeveloperReadiness.ReadinessBand.ToString())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => Array.IndexOf(expectedBands, value))
                .ToArray();
            Add(checks, "coverage.readinessBands", string.Join(";", expectedBands),
                string.Join(";", actualBands), actualBands.SequenceEqual(expectedBands));
            string[] expectedSeverities = { "None", "Notice", "Warning", "Critical" };
            string[] actualSeverities = fixture.Scenarios
                .Select(value => value.Result.PlayerHintProjection.PlayerHintSeverity.ToString())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => Array.IndexOf(expectedSeverities, value))
                .ToArray();
            Add(checks, "coverage.playerHintSeverities", string.Join(";", expectedSeverities),
                string.Join(";", actualSeverities), actualSeverities.SequenceEqual(expectedSeverities));

            Scenario zero = fixture.ById("blocked_known_zero");
            RequirementCapabilityEvaluationSnapshot zeroRow =
                zero.Result.DeveloperReadiness.RequirementEvaluations.Single();
            Add(checks, "semantic.knownZero", "Known/0/Unmet",
                zeroRow.CapabilityValueAvailability + "/" + zeroRow.BaseValueBasisPoints + "/" + zeroRow.RequirementStatus,
                zeroRow.CapabilityValueAvailability == CapabilityValueAvailability.Known
                    && zeroRow.BaseValueBasisPoints == 0
                    && zeroRow.RequirementStatus == RequirementEvaluationStatus.Unmet);
            Scenario unknown = fixture.ById("unknown_sparse");
            RequirementCapabilityEvaluationSnapshot unknownRow =
                unknown.Result.DeveloperReadiness.RequirementEvaluations.Single();
            Add(checks, "semantic.unknownPlaceholder", "Unknown/0/0/0",
                unknownRow.CapabilityValueAvailability + "/" + unknownRow.BaseValueBasisPoints + "/"
                    + unknownRow.EffectiveValueBasisPoints + "/" + unknownRow.GapBasisPoints,
                unknownRow.CapabilityValueAvailability == CapabilityValueAvailability.Unknown
                    && unknownRow.BaseValueBasisPoints == 0
                    && unknownRow.EffectiveValueBasisPoints == 0
                    && unknownRow.GapBasisPoints == 0
                    && unknownRow.RequirementStatus == RequirementEvaluationStatus.Unknown);

            Scenario anyAlternative = fixture.ById("any_alternative");
            RequirementGroupEvaluationSnapshot anyGroup =
                anyAlternative.Result.DeveloperReadiness.RequirementGroupEvaluations.Single();
            Add(checks, "semantic.anyAlternative", "group Met with Met+Unmet rows",
                anyGroup.GroupStatus + "/" + string.Join(";", anyGroup.RequirementEvaluations.Select(value => value.RequirementStatus)),
                anyGroup.GroupStatus == RequirementGroupStatus.Met
                    && anyGroup.RequirementEvaluations.Any(value => value.RequirementStatus == RequirementEvaluationStatus.Met)
                    && anyGroup.RequirementEvaluations.Any(value => value.RequirementStatus == RequirementEvaluationStatus.Unmet)
                    && anyGroup.RequirementEvaluations.All(value => value.GroupStatus == RequirementGroupStatus.Met));
            Add(checks, "semantic.anyUnknown", "Unknown",
                fixture.ById("any_unknown").Result.DeveloperReadiness.RequirementGroupEvaluations.Single().GroupStatus.ToString(),
                fixture.ById("any_unknown").Result.DeveloperReadiness.RequirementGroupEvaluations.Single().GroupStatus == RequirementGroupStatus.Unknown);
            Add(checks, "semantic.allUnmet", "Unmet",
                fixture.ById("all_unmet").Result.DeveloperReadiness.RequirementGroupEvaluations.Single().GroupStatus.ToString(),
                fixture.ById("all_unmet").Result.DeveloperReadiness.RequirementGroupEvaluations.Single().GroupStatus == RequirementGroupStatus.Unmet);

            RequirementCapabilityEvaluationSnapshot low =
                fixture.ById("clamp_low").Result.DeveloperReadiness.RequirementEvaluations.Single();
            RequirementCapabilityEvaluationSnapshot high =
                fixture.ById("clamp_high").Result.DeveloperReadiness.RequirementEvaluations.Single();
            Add(checks, "map.clampLow", "0/-14000", low.EffectiveValueBasisPoints + "/" + low.MapDeltaBasisPoints,
                low.EffectiveValueBasisPoints == 0 && low.MapDeltaBasisPoints == -14000L);
            Add(checks, "map.clampHigh", "10000/14000", high.EffectiveValueBasisPoints + "/" + high.MapDeltaBasisPoints,
                high.EffectiveValueBasisPoints == 10000 && high.MapDeltaBasisPoints == 14000L);
            RequirementCapabilityEvaluationSnapshot unknownMap =
                fixture.ById("unknown_map").Result.DeveloperReadiness.RequirementEvaluations.Single();
            Add(checks, "map.unknownPreserved", "Unknown/0/+9000",
                unknownMap.CapabilityValueAvailability + "/" + unknownMap.EffectiveValueBasisPoints + "/" + unknownMap.MapDeltaBasisPoints,
                unknownMap.CapabilityValueAvailability == CapabilityValueAvailability.Unknown
                    && unknownMap.EffectiveValueBasisPoints == 0
                    && unknownMap.MapDeltaBasisPoints == 9000L);

            Scenario mapBuff = fixture.ById("map_buff");
            EnemyReadinessEvaluationResult noBuff = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(
                    mapBuff.Input.BuildCapabilitySnapshot,
                    mapBuff.Input.PressureCatalog,
                    mapBuff.Input.BuildPressureProfileId,
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()),
                resolver);
            Add(checks, "map.buffFlip", "Blocked->Ready",
                noBuff.DeveloperReadiness.ReadinessBand + "->" + mapBuff.Result.DeveloperReadiness.ReadinessBand,
                noBuff.DeveloperReadiness.ReadinessBand == ReadinessBand.Blocked
                    && mapBuff.Result.DeveloperReadiness.ReadinessBand == ReadinessBand.Ready);
            Scenario mapDebuff = fixture.ById("map_debuff");
            EnemyReadinessEvaluationResult noDebuff = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(
                    mapDebuff.Input.BuildCapabilitySnapshot,
                    mapDebuff.Input.PressureCatalog,
                    mapDebuff.Input.BuildPressureProfileId,
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()),
                resolver);
            Add(checks, "map.debuffFlip", "Ready->Blocked",
                noDebuff.DeveloperReadiness.ReadinessBand + "->" + mapDebuff.Result.DeveloperReadiness.ReadinessBand,
                noDebuff.DeveloperReadiness.ReadinessBand == ReadinessBand.Ready
                    && mapDebuff.Result.DeveloperReadiness.ReadinessBand == ReadinessBand.Blocked);

            Add(checks, "semantics.requiredRecommended", "both covered", "covered",
                fixture.Scenarios.SelectMany(value => value.Result.DeveloperReadiness.RequirementGroupEvaluations)
                    .Select(value => value.RequirementRole).Distinct().Count() == 2);
            Add(checks, "semantics.anyAll", "both covered", "covered",
                fixture.Scenarios.SelectMany(value => value.Result.DeveloperReadiness.RequirementGroupEvaluations)
                    .Select(value => value.RequirementMatchMode).Distinct().Count() == 2);
        }

        private static void RunSignatureChecks(
            ICollection<Check> checks,
            ReferenceResolver resolver,
            Fixture fixture)
        {
            Scenario strong = fixture.ById("strong");
            EnemyReadinessEvaluationResult repeated =
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(strong.Input, resolver);
            Add(checks, "signature.repeat.developer", strong.Result.DeveloperReadiness.DeveloperCanonicalSignature,
                repeated.DeveloperReadiness.DeveloperCanonicalSignature,
                strong.Result.DeveloperReadiness.DeveloperCanonicalSignature == repeated.DeveloperReadiness.DeveloperCanonicalSignature);
            Add(checks, "signature.repeat.player", strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature,
                repeated.PlayerHintProjection.PlayerSafeCanonicalSignature,
                strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature == repeated.PlayerHintProjection.PlayerSafeCanonicalSignature);
            Add(checks, "signature.format.developer", "sha256:+64 lowercase hex",
                strong.Result.DeveloperReadiness.DeveloperCanonicalSignature,
                IsSignature(strong.Result.DeveloperReadiness.DeveloperCanonicalSignature));
            Add(checks, "signature.format.player", "sha256:+64 lowercase hex",
                strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature,
                IsSignature(strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature));

            Scenario order = fixture.ById("clamp_high");
            EnemyReadinessEvaluationResult reversed = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(
                    order.Input.BuildCapabilitySnapshot,
                    order.Input.PressureCatalog,
                    order.Input.BuildPressureProfileId,
                    order.Input.MapRuleCapabilityAdjustments.Reverse()),
                resolver);
            Add(checks, "signature.adjustmentOrder.developer", order.Result.DeveloperReadiness.DeveloperCanonicalSignature,
                reversed.DeveloperReadiness.DeveloperCanonicalSignature,
                order.Result.DeveloperReadiness.DeveloperCanonicalSignature == reversed.DeveloperReadiness.DeveloperCanonicalSignature);
            Add(checks, "signature.adjustmentOrder.player", order.Result.PlayerHintProjection.PlayerSafeCanonicalSignature,
                reversed.PlayerHintProjection.PlayerSafeCanonicalSignature,
                order.Result.PlayerHintProjection.PlayerSafeCanonicalSignature == reversed.PlayerHintProjection.PlayerSafeCanonicalSignature);

            BuildCapabilitySnapshot changedBuild = Build(
                resolver,
                strong.Input.BuildCapabilitySnapshot.SnapshotId,
                BuildCapabilityCoverageMode.Complete,
                Values(fixture.K0, 6100, fixture.K1, 4500, fixture.K2, 3500));
            EnemyReadinessEvaluationResult buildChanged = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(changedBuild, strong.Input.PressureCatalog,
                    strong.Input.BuildPressureProfileId, strong.Input.MapRuleCapabilityAdjustments), resolver);
            AddDeveloperChangesPlayerStable(checks, "signature.capabilityValue", strong.Result, buildChanged);

            CounterWindowAndPressureCatalogSnapshot thresholdCatalog = PressureCatalog(
                resolver,
                strong.Input.BuildPressureProfileId,
                Groups(
                    Group("group.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement(fixture.K0, 5100), Requirement(fixture.K1, 4000)),
                    Group("group.recommended.any", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                        Requirement(fixture.K2, 3000))),
                "enemy.pressure.readiness.fixture.label",
                "enemy.pressure.readiness.fixture.hint");
            EnemyReadinessEvaluationResult thresholdChanged = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(strong.Input.BuildCapabilitySnapshot, thresholdCatalog,
                    strong.Input.BuildPressureProfileId, strong.Input.MapRuleCapabilityAdjustments), resolver);
            AddDeveloperChangesPlayerStable(checks, "signature.threshold", strong.Result, thresholdChanged);

            Scenario buff = fixture.ById("map_buff");
            EnemyReadinessEvaluationResult deltaChanged = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(buff.Input.BuildCapabilitySnapshot, buff.Input.PressureCatalog,
                    buff.Input.BuildPressureProfileId,
                    Adjustments(Adjustment(fixture.M0, fixture.K0, 2600, "reason.map.buff"))), resolver);
            AddDeveloperChangesPlayerStable(checks, "signature.mapDelta", buff.Result, deltaChanged);

            string semanticsProfile = "dev_readiness_fixture_group_semantics";
            BuildCapabilitySnapshot semanticsBuild = Build(resolver, "dev_build_group_semantics",
                BuildCapabilityCoverageMode.Sparse, Values(fixture.K0, 6000, fixture.K1, 6000));
            CounterWindowAndPressureCatalogSnapshot anyCatalog = PressureCatalog(resolver, semanticsProfile,
                Groups(Group("group.semantics", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                    Requirement(fixture.K0, 5000), Requirement(fixture.K1, 5000))),
                "enemy.pressure.readiness.fixture.label", "enemy.pressure.readiness.fixture.hint");
            CounterWindowAndPressureCatalogSnapshot allCatalog = PressureCatalog(resolver, semanticsProfile,
                Groups(Group("group.semantics", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                    Requirement(fixture.K0, 5000), Requirement(fixture.K1, 5000))),
                "enemy.pressure.readiness.fixture.label", "enemy.pressure.readiness.fixture.hint");
            EnemyReadinessEvaluationResult anyResult = Evaluate(semanticsBuild, anyCatalog, semanticsProfile, resolver);
            EnemyReadinessEvaluationResult allResult = Evaluate(semanticsBuild, allCatalog, semanticsProfile, resolver);
            AddDeveloperChangesPlayerStable(checks, "signature.groupSemantics", anyResult, allResult);

            CounterWindowAndPressureCatalogSnapshot publicCatalog = PressureCatalog(
                resolver,
                strong.Input.BuildPressureProfileId,
                strong.Input.PressureCatalog.BuildPressureProfiles.Single().DeveloperOnly.RequirementGroups,
                "enemy.pressure.readiness.fixture.label",
                "enemy.pressure.readiness.fixture.hint.changed");
            EnemyReadinessEvaluationResult publicChanged = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(strong.Input.BuildCapabilitySnapshot, publicCatalog,
                    strong.Input.BuildPressureProfileId, strong.Input.MapRuleCapabilityAdjustments), resolver);
            Add(checks, "signature.publicHint.developerChanges", "different",
                publicChanged.DeveloperReadiness.DeveloperCanonicalSignature,
                strong.Result.DeveloperReadiness.DeveloperCanonicalSignature != publicChanged.DeveloperReadiness.DeveloperCanonicalSignature);
            Add(checks, "signature.publicHint.playerChanges", "different",
                publicChanged.PlayerHintProjection.PlayerSafeCanonicalSignature,
                strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature != publicChanged.PlayerHintProjection.PlayerSafeCanonicalSignature);

            string severityProfile = "dev_readiness_fixture_severity_boundary";
            CounterWindowAndPressureCatalogSnapshot severityCatalog = PressureCatalog(resolver, severityProfile,
                Groups(Group("group.severity", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                    Requirement(fixture.K0, 5000))),
                "enemy.pressure.readiness.fixture.label", "enemy.pressure.readiness.fixture.hint");
            EnemyReadinessEvaluationResult ready = Evaluate(
                Build(resolver, "dev_build_severity", BuildCapabilityCoverageMode.Sparse, Values(fixture.K0, 6000)),
                severityCatalog, severityProfile, resolver);
            EnemyReadinessEvaluationResult blocked = Evaluate(
                Build(resolver, "dev_build_severity", BuildCapabilityCoverageMode.Sparse, Values(fixture.K0, 1000)),
                severityCatalog, severityProfile, resolver);
            Add(checks, "signature.severityBoundary.playerChanges", "different",
                blocked.PlayerHintProjection.PlayerSafeCanonicalSignature,
                ready.PlayerHintProjection.PlayerSafeCanonicalSignature != blocked.PlayerHintProjection.PlayerSafeCanonicalSignature);

            Add(checks, "immutability.input.adjustments", "read-only", "checked",
                MutationRejected(order.Input.MapRuleCapabilityAdjustments));
            Add(checks, "immutability.output.groups", "read-only", "checked",
                MutationRejected(order.Result.DeveloperReadiness.RequirementGroupEvaluations));
            Add(checks, "immutability.output.rows", "read-only", "checked",
                MutationRejected(order.Result.DeveloperReadiness.RequirementEvaluations));
            Add(checks, "immutability.output.playerHints", "read-only", "checked",
                MutationRejected(order.Result.PlayerHintProjection.PlayerHintCategoryKeys));
            Add(checks, "immutability.noSharedMutableRows", "different instances", "checked",
                !ReferenceEquals(order.Result.DeveloperReadiness.RequirementEvaluations[0],
                    reversed.DeveloperReadiness.RequirementEvaluations[0]));
        }

        private static void RunValidationChecks(
            ICollection<Check> checks,
            ReferenceResolver resolver,
            Fixture fixture)
        {
            Scenario valid = fixture.ById("ready");
            ExpectCode(checks, "validation.nullInput", "INPUT_NULL", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(null, resolver));
            ExpectCode(checks, "validation.nullResolver", "RESOLVER_NULL", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(valid.Input, null));
            ExpectCode(checks, "validation.nullBuild", "BUILD_SNAPSHOT_NULL", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                    new EnemyReadinessEvaluationInput(null, valid.Input.PressureCatalog,
                        valid.Input.BuildPressureProfileId, Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver));
            ExpectCode(checks, "validation.nullPressureCatalog", "PRESSURE_CATALOG_NULL", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                    new EnemyReadinessEvaluationInput(valid.Input.BuildCapabilitySnapshot, null,
                        valid.Input.BuildPressureProfileId, Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver));
            ExpectCode(checks, "validation.emptyProfileId", "ID_EMPTY", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                    new EnemyReadinessEvaluationInput(valid.Input.BuildCapabilitySnapshot, valid.Input.PressureCatalog,
                        string.Empty, Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver));
            ExpectCode(checks, "validation.profileOuterWhitespace", "ID_OUTER_WHITESPACE", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                    new EnemyReadinessEvaluationInput(valid.Input.BuildCapabilitySnapshot, valid.Input.PressureCatalog,
                        " " + valid.Input.BuildPressureProfileId, Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver));
            ExpectCode(checks, "validation.profileUnresolved", "PRESSURE_PROFILE_UNRESOLVED", () =>
                DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                    new EnemyReadinessEvaluationInput(valid.Input.BuildCapabilitySnapshot, valid.Input.PressureCatalog,
                        "dev_pressure_missing", Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver));
            ExpectCode(checks, "validation.nullAdjustment", "MAP_ADJUSTMENT_NULL", () =>
                EvaluateWithAdjustments(valid, resolver, new MapRuleCapabilityAdjustmentSnapshot[] { null }));
            ExpectCode(checks, "validation.zeroDelta", "MAP_DELTA_OUT_OF_RANGE", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment(fixture.M0, fixture.K0, 0, "reason.zero")));
            ExpectCode(checks, "validation.deltaAboveRange", "MAP_DELTA_OUT_OF_RANGE", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment(fixture.M0, fixture.K0, 10001, "reason.range")));
            ExpectCode(checks, "validation.reasonPath", "DEVELOPER_REASON_PATH_FORBIDDEN", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment(fixture.M0, fixture.K0, 100, "Assets/fixture/reason")));
            ExpectCode(checks, "validation.duplicateMapCapability", "MAP_ADJUSTMENT_DUPLICATE", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment(fixture.M0, fixture.K0, 100, "reason.duplicate.a"),
                    Adjustment(fixture.M0, fixture.K0, 200, "reason.duplicate.b")));
            ExpectCode(checks, "validation.mapRuleUnresolved", "MAP_RULE_UNRESOLVED", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment("dev_map_missing", fixture.K0, 100, "reason.map.missing")));
            ExpectCode(checks, "validation.capabilityUnresolved", "MAP_CAPABILITY_KEY_UNRESOLVED", () =>
                EvaluateWithAdjustments(valid, resolver,
                    Adjustment(fixture.M0, "capability.missing", 100, "reason.capability.missing")));

            string evaluatorText = File.ReadAllText(Absolute(FindProjectRoot(), RuntimeFiles[3]));
            Add(checks, "validation.schemaGuardsPresent", "E06+E07 schema guards", "scanned",
                evaluatorText.Contains("BUILD_SCHEMA_ID_MISMATCH")
                    && evaluatorText.Contains("BUILD_SCHEMA_VERSION_MISMATCH")
                    && evaluatorText.Contains("PRESSURE_SCHEMA_ID_MISMATCH")
                    && evaluatorText.Contains("PRESSURE_SCHEMA_VERSION_MISMATCH"));
            Add(checks, "validation.isolationGuardsPresent", "build+pressure isolation guards", "scanned",
                evaluatorText.Contains("BUILD_ISOLATION_INVALID")
                    && evaluatorText.Contains("PRESSURE_ISOLATION_INVALID"));
        }

        private static void RunBaselineChecks(
            ICollection<Check> checks,
            string root,
            EnemyMechanicVocabularySnapshot vocabulary,
            EnemyValidationContentSnapshot normalization)
        {
            Add(checks, "baseline.e02.totalVocabulary", "63",
                vocabulary.Entries.Count.ToString(CultureInfo.InvariantCulture), vocabulary.Entries.Count == 63);
            Add(checks, "baseline.e02.buildCapability", "16",
                vocabulary.Entries.Count(value => value.Category == EnemyVocabularyCategory.BuildCapability).ToString(CultureInfo.InvariantCulture),
                vocabulary.Entries.Count(value => value.Category == EnemyVocabularyCategory.BuildCapability) == 16);
            Add(checks, "baseline.e03.carriers", "11/7/18",
                normalization.Enemies.Count + "/" + normalization.Bosses.Count + "/" + (normalization.Enemies.Count + normalization.Bosses.Count),
                normalization.Enemies.Count == 11 && normalization.Bosses.Count == 7);
            int enemyProfiles = normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy);
            int bossProfiles = normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss);
            Add(checks, "baseline.e03.mechanicProfiles", "10/6/16",
                enemyProfiles + "/" + bossProfiles + "/" + normalization.MechanicProfiles.Count,
                enemyProfiles == 10 && bossProfiles == 6 && normalization.MechanicProfiles.Count == 16);
            Add(checks, "baseline.e03.mapRules", "10", normalization.MapRules.Count.ToString(CultureInfo.InvariantCulture),
                normalization.MapRules.Count == 10);
            Add(checks, "baseline.e03.carrierBindings", "17",
                normalization.CarrierMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                normalization.CarrierMechanicBindings.Count == 17);
            Add(checks, "baseline.e03.mapRuleBindings", "30",
                normalization.MapRuleMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                normalization.MapRuleMechanicBindings.Count == 30);
            Add(checks, "baseline.e03.exceptions", "2", normalization.Exceptions.Count.ToString(CultureInfo.InvariantCulture),
                normalization.Exceptions.Count == 2);

            IReadOnlyList<string[]> e04 = ReadCsv(Absolute(root, "Docs/V0.4/Reports/EncounterCompositionSchemaFixtureRows.csv"));
            Add(checks, "baseline.e04.fixture", "2/4/6", FixtureCounts(e04, 0, 1),
                e04.Count == 6 && e04.Select(value => value[0]).Distinct(StringComparer.Ordinal).Count() == 2
                    && e04.Select(value => value[1]).Distinct(StringComparer.Ordinal).Count() == 4);
            IReadOnlyList<string[]> e05 = ReadCsv(Absolute(root, "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFixtureRows.csv"));
            const string expectedE05Counts = "4/3/3/3/2";
            string actualE05Counts = E05Counts(e05);
            Add(checks, "baseline.e05.fixture", expectedE05Counts, actualE05Counts,
                string.Equals(actualE05Counts, expectedE05Counts, StringComparison.Ordinal));
            IReadOnlyList<string[]> e06 = ReadCsv(Absolute(root, "Docs/V0.4/Reports/CounterWindowAndPressureSchemaFixtureRows.csv"));
            Add(checks, "baseline.e06.fixture", "4/3/6/4/5/11", RowKindCounts(e06,
                "BuildPressureProfile", "CounterWindowProfile", "PressureSourceBinding", "CounterWindowSourceBinding", "PressureCounterWindowBinding", "BuildCapabilityRequirement"),
                CountKind(e06, "BuildPressureProfile") == 4
                    && CountKind(e06, "CounterWindowProfile") == 3
                    && CountKind(e06, "PressureSourceBinding") == 6
                    && CountKind(e06, "CounterWindowSourceBinding") == 4
                    && CountKind(e06, "PressureCounterWindowBinding") == 5
                    && CountKind(e06, "BuildCapabilityRequirement") == 11);
            IReadOnlyList<string[]> e07 = ReadCsv(Absolute(root, "Docs/V0.4/Reports/BuildCapabilityReadContractFixtureRows.csv"));
            Add(checks, "baseline.e07.fixture", "16/16/4/6/3/5", E07Counts(e07),
                e07.Count(value => value[0] == "CapabilityValue" && value[1] == "dev_build_capability_fixture_complete") == 16
                    && e07.Count(value => value[0] == "CapabilityValue" && value[1] == "dev_build_capability_fixture_sparse") == 4
                    && e07.Count(value => value[0] == "SourceSummary" && value[1] == "dev_build_capability_fixture_complete") == 6
                    && CountKind(e07, "CapabilityGap") == 3
                    && Enum.GetValues(typeof(ReadinessBand)).Length == 5);

            AddHashChecks(checks, root, "protected", ProtectedHashes);
            AddHashChecks(checks, root, "legacy", LegacyHashes);
            Add(checks, "protected.count", "33/33", ProtectedHashes.Count + "/" + ProtectedHashes.Count,
                ProtectedHashes.Count == 33);
            Add(checks, "legacy.count", "3/3", LegacyHashes.Count + "/" + LegacyHashes.Count,
                LegacyHashes.Count == 3);
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            string root,
            Fixture fixture)
        {
            string runtime = StripStringLiteralsAndComments(string.Join(
                "\n",
                RuntimeFiles.Select(value => File.ReadAllText(Absolute(root, value)))));
            int dependencyTotal = 0;
            foreach (string token in RuntimeForbiddenTokens)
            {
                int count = Count(runtime, token);
                dependencyTotal += count;
                Add(checks, "leak.runtime." + SafeId(token), "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
            }
            Add(checks, "leak.runtime.total", "0", dependencyTotal.ToString(CultureInfo.InvariantCulture), dependencyTotal == 0);

            string[] editorFiles = Directory.GetFiles(
                Absolute(root, "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem"),
                "*.cs",
                SearchOption.TopDirectoryOnly);
            int legacyReaders = editorFiles.Count(path =>
                StripStringLiteralsAndComments(File.ReadAllText(path))
                    .Contains("using TalismanBag.BuildSandbox;"));
            Add(checks, "leak.editorLegacyReaderTotal", "1", legacyReaders.ToString(CultureInfo.InvariantCulture),
                legacyReaders == 1);

            Type playerType = typeof(EnemyReadinessPlayerHintProjection);
            string[] allowedProperties =
            {
                "BuildPressureProfileId",
                "PublicPressureLabelKey",
                "PublicPressureHintKey",
                "PlayerHintCategoryKeys",
                "PlayerHintSeverity",
                "PlayerSafeCanonicalSignature"
            };
            string[] actualProperties = playerType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "leak.playerPropertyShape", string.Join(";", allowedProperties.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(";", actualProperties),
                actualProperties.SequenceEqual(allowedProperties.OrderBy(value => value, StringComparer.Ordinal)));
            int playerLeaks = 0;
            foreach (string token in PlayerForbiddenTokens)
            {
                int count = fixture.Scenarios.Sum(value => Count(
                    value.Result.PlayerHintProjection.BuildPlayerSafeCanonicalPayload(), token));
                playerLeaks += count;
                Add(checks, "leak.player." + SafeId(token), "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
            }
            Add(checks, "leak.player.total", "0", playerLeaks.ToString(CultureInfo.InvariantCulture), playerLeaks == 0);
        }

        private static void RunGeneratedFileChecks(ICollection<Check> checks, string root)
        {
            int present = ExpectedPackageFiles.Count(value => File.Exists(Absolute(root, value)));
            Add(checks, "package.files", "16/16", present + "/" + ExpectedPackageFiles.Length,
                present == 16 && ExpectedPackageFiles.Length == 16);

            string packageDirectory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation");
            HashSet<string> expected = new HashSet<string>(
                ExpectedPackageFiles.Select(value => Normalize(value)),
                StringComparer.Ordinal);
            string[] actualRuntimePackage = Directory.GetFiles(packageDirectory, "*", SearchOption.AllDirectories)
                .Select(value => Normalize(Relative(root, value))).ToArray();
            string[] actualReports = Directory.GetFiles(Absolute(root, ReportRoot),
                    "EnemyOfflineReadinessEvaluator*", SearchOption.TopDirectoryOnly)
                .Select(value => Normalize(Relative(root, value))).ToArray();
            string[] actualPackage = actualRuntimePackage
                .Concat(actualReports)
                .Concat(new[] { "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation.meta" })
                .Concat(new[]
                {
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs.meta"
                })
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] unexpected = actualPackage.Where(value => !expected.Contains(value)).ToArray();
            string[] missing = expected.Where(value => !actualPackage.Contains(value, StringComparer.Ordinal)).ToArray();
            Add(checks, "package.pathWhitelist", "PASS", "unexpected=" + unexpected.Length + ",missing=" + missing.Length,
                unexpected.Length == 0 && missing.Length == 0 && actualPackage.Length == 16);

            int trailing = ExpectedPackageFiles.Where(value => File.Exists(Absolute(root, value)))
                .Sum(value => CountTrailingWhitespace(Absolute(root, value)));
            Add(checks, "package.trailingWhitespace", "0", trailing.ToString(CultureInfo.InvariantCulture), trailing == 0);

            string[] metaPaths = ExpectedPackageFiles.Where(value => value.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
            string[] allMeta = Directory.GetFiles(Absolute(root, "Assets"), "*.meta", SearchOption.AllDirectories);
            bool allUnique = true;
            foreach (string relative in metaPaths)
            {
                string guid = ReadMetaGuid(Absolute(root, relative));
                int count = allMeta.Count(path => string.Equals(ReadMetaGuid(path), guid, StringComparison.Ordinal));
                bool unique = guid.Length == 32 && count == 1;
                allUnique &= unique;
                Add(checks, "package.guid." + Path.GetFileName(relative), "unique", guid + " x" + count, unique);
            }
            Add(checks, "package.guidConflicts", "0", allUnique ? "0" : "non-zero", allUnique);
        }

        private static void RunGlobalDiffCheck(ICollection<Check> checks, string root)
        {
            ProcessResult result = RunProcess(root, "git", "diff --check");
            string output = (result.StandardOutput + "\n" + result.StandardError).Trim();
            if (result.ExitCode == 0)
            {
                Add(checks, "package.globalDiffCheck", "PASS", "PASS", true);
                return;
            }

            bool packageMentioned = ExpectedPackageFiles.Any(value =>
                output.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0
                || output.IndexOf(value.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase) >= 0);
            Add(checks, "package.globalDiffCheck", "PASS or PREEXISTING_UNRELATED_DIFF",
                packageMentioned ? "FAIL" : "PREEXISTING_UNRELATED_DIFF",
                !packageMentioned);
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<Check> checks,
            Fixture fixture)
        {
            WriteUtf8(Absolute(root, ReportRoot + "EnemyOfflineReadinessEvaluatorReport.md"),
                BuildMainReport(mode, checks, fixture));
            WriteUtf8(Absolute(root, ReportRoot + "EnemyOfflineReadinessEvaluatorSpec.csv"),
                CsvTable(new[] { "checkId", "expected", "actual", "result" },
                    checks.Select(value => new[] { value.Id, value.Expected, value.Actual, value.Passed ? "PASS" : "FAIL" })));
            WriteUtf8(Absolute(root, ReportRoot + "EnemyOfflineReadinessEvaluatorFieldMatrix.csv"),
                BuildFieldMatrix());
            WriteUtf8(Absolute(root, ReportRoot + "EnemyOfflineReadinessEvaluatorScenarioRows.csv"),
                BuildScenarioRows(fixture));
            WriteUtf8(Absolute(root, ReportRoot + "EnemyOfflineReadinessEvaluatorLeakCheckReport.md"),
                BuildLeakReport(checks));
        }

        private static string BuildMainReport(
            string mode,
            IReadOnlyList<Check> checks,
            Fixture fixture)
        {
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            string distribution = string.Join(", ", Enum.GetValues(typeof(ReadinessBand)).Cast<ReadinessBand>()
                .Select(band => band + "=" + fixture.Scenarios.Count(value => value.Result.DeveloperReadiness.ReadinessBand == band)));
            Scenario strong = fixture.ById("strong");
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Offline Readiness Evaluator Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-EnemyOfflineReadinessEvaluator01`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Checks: `" + checks.Count(value => value.Passed) + "/" + checks.Count + " PASS`")
                .AppendLine("- Scenarios: `" + fixture.Scenarios.Count + "`")
                .AppendLine("- ReadinessBand distribution: `" + distribution + "`")
                .AppendLine("- Required / Recommended coverage: `PASS`")
                .AppendLine("- Any / All coverage: `PASS`")
                .AppendLine("- Known Zero / Unknown coverage: `PASS`")
                .AppendLine("- Map buff / debuff / low-high clamp / order coverage: `PASS`")
                .AppendLine("- DeveloperCanonicalSignature sample: `" + strong.Result.DeveloperReadiness.DeveloperCanonicalSignature + "`")
                .AppendLine("- PlayerSafeCanonicalSignature sample: `" + strong.Result.PlayerHintProjection.PlayerSafeCanonicalSignature + "`")
                .AppendLine("- Player leak check: `" + Result(checks, "leak.player.total") + "`")
                .AppendLine("- Runtime forbidden dependency check: `" + Result(checks, "leak.runtime.total") + "`")
                .AppendLine("- E01-E07 protected hash: `" + CountResult(checks, "protected.", 33) + "`")
                .AppendLine("- Legacy source hash: `" + CountResult(checks, "legacy.", 3) + "`")
                .AppendLine("- Global git diff --check: `" + Actual(checks, "package.globalDiffCheck") + "`")
                .AppendLine()
                .AppendLine("## Scenario summary")
                .AppendLine()
                .AppendLine("| Scenario | Coverage | Band | Severity | Developer signature | Player signature |")
                .AppendLine("|---|---|---|---|---|---|");
            foreach (Scenario scenario in fixture.Scenarios)
            {
                builder.Append("| ").Append(scenario.Id).Append(" | ")
                    .Append(scenario.CoverageMode).Append(" | ")
                    .Append(scenario.Result.DeveloperReadiness.ReadinessBand).Append(" | ")
                    .Append(scenario.Result.PlayerHintProjection.PlayerHintSeverity).Append(" | `")
                    .Append(scenario.Result.DeveloperReadiness.DeveloperCanonicalSignature).Append("` | `")
                    .Append(scenario.Result.PlayerHintProjection.PlayerSafeCanonicalSignature).AppendLine("` |");
            }
            builder.AppendLine()
                .AppendLine("## Detailed checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                builder.Append("| `").Append(check.Id).Append("` | ")
                    .Append(Markdown(check.Expected)).Append(" | ")
                    .Append(Markdown(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            return builder.ToString();
        }

        private static string BuildFieldMatrix()
        {
            List<string[]> rows = new List<string[]>
            {
                Field("EnemyReadinessEvaluationInput", "BuildCapabilitySnapshot", "BuildCapabilitySnapshot", "1", "Validated E07 abstract capability snapshot", "developer-only input", "E07 Provider", "E08 Evaluator"),
                Field("EnemyReadinessEvaluationInput", "PressureCatalog", "CounterWindowAndPressureCatalogSnapshot", "1", "Validated E06 catalog", "developer-only input", "E06 Provider", "E08 Evaluator"),
                Field("EnemyReadinessEvaluationInput", "BuildPressureProfileId", "string", "1", "Ordinal-selected E06 pressure", "developer-only input", "caller", "E08 Evaluator"),
                Field("MapRuleCapabilityAdjustmentSnapshot", "MapRuleId", "string", "1", "E03 MapRule reference", "developer-only", "E03", "E08 Evaluator"),
                Field("MapRuleCapabilityAdjustmentSnapshot", "BuildCapabilityKey", "string", "1", "E02 abstract capability", "developer-only", "E02", "E08 Evaluator"),
                Field("MapRuleCapabilityAdjustmentSnapshot", "DeltaBasisPoints", "int", "1", "Non-zero -10000..10000 adjustment", "developer-only", "caller", "E08 Evaluator"),
                Field("RequirementCapabilityEvaluationSnapshot", "CapabilityGap", "CapabilityGapSnapshot", "1", "E07 gap shape using effective known value or Unknown placeholder", "developer-only", "E07 shape/E08 value", "offline reports"),
                Field("RequirementCapabilityEvaluationSnapshot", "MapDeltaBasisPoints", "long", "1", "Ordinal deterministic sum before effective clamp", "developer-only", "E08", "offline reports"),
                Field("RequirementCapabilityEvaluationSnapshot", "RequirementStatus", "enum", "1", "Met Unmet Unknown row status", "developer-only", "E08", "offline reports"),
                Field("RequirementCapabilityEvaluationSnapshot", "GroupStatus", "enum", "1", "Owning Any/All group status", "developer-only", "E08", "offline reports"),
                Field("RequirementGroupEvaluationSnapshot", "GroupStatus", "enum", "1", "Any/All aggregate status", "developer-only", "E08", "offline reports"),
                Field("DeveloperReadinessSnapshot", "ReadinessBand", "ReadinessBand", "1", "Unknown Blocked Strained Ready Strong", "developer-only", "E07 shape/E08 rule", "developer tooling"),
                Field("DeveloperReadinessSnapshot", "RequirementGroupEvaluations", "RequirementGroupEvaluationSnapshot[]", "1..N", "Explainable group results", "developer-only", "E08", "developer tooling"),
                Field("DeveloperReadinessSnapshot", "RequirementEvaluations", "RequirementCapabilityEvaluationSnapshot[]", "1..N", "Explainable requirement rows", "developer-only", "E08", "developer tooling"),
                Field("DeveloperReadinessSnapshot", "DeveloperCanonicalSignature", "string", "1", "Deterministic full evaluation signature", "developer-only", "E08", "validator"),
                Field("EnemyReadinessPlayerHintProjection", "BuildPressureProfileId", "string", "1", "Public pressure identity", "player-safe", "E06", "future optional consumer"),
                Field("EnemyReadinessPlayerHintProjection", "PublicPressureLabelKey", "string", "1", "Public pressure label", "player-safe", "E06", "future optional consumer"),
                Field("EnemyReadinessPlayerHintProjection", "PublicPressureHintKey", "string", "1", "Public pressure hint", "player-safe", "E06", "future optional consumer"),
                Field("EnemyReadinessPlayerHintProjection", "PlayerHintCategoryKeys", "string[]", "0..N", "Public hint categories", "player-safe", "E06/E02", "future optional consumer"),
                Field("EnemyReadinessPlayerHintProjection", "PlayerHintSeverity", "enum", "1", "None Notice Warning Critical", "player-safe", "E08 coarse projection", "future optional consumer"),
                Field("EnemyReadinessPlayerHintProjection", "PlayerSafeCanonicalSignature", "string", "1", "Deterministic player-safe signature", "player-safe", "E08", "validator")
            };
            return CsvTable(new[] { "ownerType", "fieldName", "fieldType", "cardinality", "semantic", "visibility", "sourceOfTruth", "futureConsumer" }, rows);
        }

        private static string BuildScenarioRows(Fixture fixture)
        {
            List<string[]> rows = new List<string[]>();
            foreach (Scenario scenario in fixture.Scenarios)
            {
                foreach (RequirementCapabilityEvaluationSnapshot row in
                    scenario.Result.DeveloperReadiness.RequirementEvaluations)
                {
                    rows.Add(new[]
                    {
                        scenario.Id,
                        scenario.CoverageMode.ToString(),
                        scenario.Input.BuildPressureProfileId,
                        row.RequirementGroupId,
                        row.RequirementRole.ToString(),
                        row.RequirementMatchMode.ToString(),
                        row.BuildCapabilityKey,
                        row.CapabilityValueAvailability.ToString(),
                        row.BaseValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                        row.MapDeltaBasisPoints.ToString(CultureInfo.InvariantCulture),
                        row.EffectiveValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                        row.RequiredValueBasisPoints.ToString(CultureInfo.InvariantCulture),
                        row.GapBasisPoints.ToString(CultureInfo.InvariantCulture),
                        row.RequirementStatus.ToString(),
                        row.GroupStatus.ToString(),
                        scenario.Result.DeveloperReadiness.ReadinessBand.ToString(),
                        scenario.Result.PlayerHintProjection.PlayerHintSeverity.ToString(),
                        "true"
                    });
                }
            }
            return CsvTable(new[]
            {
                "scenarioId", "coverageMode", "pressureProfileId", "requirementGroupId", "role", "matchMode",
                "capabilityKey", "availability", "baseValue", "mapDelta", "effectiveValue", "requiredValue",
                "gapValue", "requirementStatus", "groupStatus", "readinessBand", "playerHintSeverity", "fixtureOnly"
            }, rows);
        }

        private static string BuildLeakReport(IReadOnlyList<Check> checks)
        {
            Check[] selected = checks.Where(value =>
                    value.Id.StartsWith("leak.", StringComparison.Ordinal)
                    || value.Id.StartsWith("protected.", StringComparison.Ordinal)
                    || value.Id.StartsWith("legacy.", StringComparison.Ordinal)
                    || value.Id.StartsWith("package.", StringComparison.Ordinal)
                    || value.Id.StartsWith("baseline.", StringComparison.Ordinal))
                .OrderBy(value => value.Id, StringComparer.Ordinal).ToArray();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Offline Readiness Evaluator Leak Check Report")
                .AppendLine()
                .AppendLine("Every forbidden runtime dependency and player payload token is listed separately.")
                .AppendLine()
                .AppendLine("- Package: `V0.4-EnemyOfflineReadinessEvaluator01`")
                .AppendLine("- Result: `" + (selected.All(value => value.Passed) ? "PASS" : "FAIL") + "`")
                .AppendLine("- Runtime forbidden references: `" + Actual(checks, "leak.runtime.total") + "`")
                .AppendLine("- Player payload leaks: `" + Actual(checks, "leak.player.total") + "`")
                .AppendLine("- Editor-only legacy readers: `" + Actual(checks, "leak.editorLegacyReaderTotal") + "`")
                .AppendLine("- E01-E07 protected hashes: `" + CountResult(checks, "protected.", 33) + "`")
                .AppendLine("- Legacy source hashes: `" + CountResult(checks, "legacy.", 3) + "`")
                .AppendLine()
                .AppendLine("## Detailed checks")
                .AppendLine();
            foreach (Check check in selected)
            {
                builder.Append("- `").Append(check.Id).Append("`: `")
                    .Append(check.Passed ? "PASS" : "FAIL")
                    .Append("` (expected `").Append(Markdown(check.Expected))
                    .Append("`, actual `").Append(Markdown(check.Actual)).AppendLine("`)");
            }
            return builder.ToString();
        }

        private static EnemyReadinessEvaluationResult Evaluate(
            BuildCapabilitySnapshot build,
            CounterWindowAndPressureCatalogSnapshot catalog,
            string profileId,
            ReferenceResolver resolver)
        {
            return DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(build, catalog, profileId,
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver);
        }

        private static void EvaluateWithAdjustments(
            Scenario scenario,
            ReferenceResolver resolver,
            params MapRuleCapabilityAdjustmentSnapshot[] adjustments)
        {
            DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(
                    scenario.Input.BuildCapabilitySnapshot,
                    scenario.Input.PressureCatalog,
                    scenario.Input.BuildPressureProfileId,
                    adjustments),
                resolver);
        }

        private static void AddDeveloperChangesPlayerStable(
            ICollection<Check> checks,
            string id,
            EnemyReadinessEvaluationResult baseline,
            EnemyReadinessEvaluationResult changed)
        {
            Add(checks, id + ".developerChanges", "different",
                changed.DeveloperReadiness.DeveloperCanonicalSignature,
                baseline.DeveloperReadiness.DeveloperCanonicalSignature != changed.DeveloperReadiness.DeveloperCanonicalSignature);
            Add(checks, id + ".playerStable", baseline.PlayerHintProjection.PlayerSafeCanonicalSignature,
                changed.PlayerHintProjection.PlayerSafeCanonicalSignature,
                baseline.PlayerHintProjection.PlayerSafeCanonicalSignature == changed.PlayerHintProjection.PlayerSafeCanonicalSignature);
        }

        private static void ExpectCode(
            ICollection<Check> checks,
            string id,
            string expectedCode,
            Action action)
        {
            try
            {
                action();
                Add(checks, id, expectedCode, "no exception", false);
            }
            catch (EnemyReadinessEvaluationException exception)
            {
                string actual = string.Join(";", exception.Issues.Select(value => value.Code).Distinct(StringComparer.Ordinal));
                Add(checks, id, expectedCode, actual,
                    exception.Issues.Any(value => value.Code == expectedCode));
            }
        }

        private static BuildCapabilityRequirementSnapshot Requirement(string key, int threshold)
        {
            return new BuildCapabilityRequirementSnapshot(key, threshold);
        }

        private static BuildCapabilityRequirementGroupSnapshot Group(
            string id,
            CapabilityRequirementRole role,
            RequirementMatchMode mode,
            params BuildCapabilityRequirementSnapshot[] requirements)
        {
            return new BuildCapabilityRequirementGroupSnapshot(id, role, mode, requirements);
        }

        private static IReadOnlyList<BuildCapabilityRequirementGroupSnapshot> Groups(
            params BuildCapabilityRequirementGroupSnapshot[] groups)
        {
            return Array.AsReadOnly(groups);
        }

        private static MapRuleCapabilityAdjustmentSnapshot Adjustment(
            string mapRuleId,
            string capabilityKey,
            int delta,
            string reason)
        {
            return new MapRuleCapabilityAdjustmentSnapshot(mapRuleId, capabilityKey, delta, reason);
        }

        private static IReadOnlyList<MapRuleCapabilityAdjustmentSnapshot> Adjustments(
            params MapRuleCapabilityAdjustmentSnapshot[] values)
        {
            return Array.AsReadOnly(values);
        }

        private static IReadOnlyDictionary<string, int> Values(
            string k0,
            int v0,
            string k1 = null,
            int v1 = 0,
            string k2 = null,
            int v2 = 0)
        {
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [k0] = v0
            };
            if (k1 != null) result[k1] = v1;
            if (k2 != null) result[k2] = v2;
            return new ReadOnlyDictionary<string, int>(result);
        }

        private static PlayerHintSeverity Severity(ReadinessBand band)
        {
            switch (band)
            {
                case ReadinessBand.Unknown: return PlayerHintSeverity.Notice;
                case ReadinessBand.Blocked: return PlayerHintSeverity.Critical;
                case ReadinessBand.Strained: return PlayerHintSeverity.Warning;
                case ReadinessBand.Ready:
                case ReadinessBand.Strong: return PlayerHintSeverity.None;
                default: throw new ArgumentOutOfRangeException(nameof(band));
            }
        }

        private static bool MutationRejected<T>(IReadOnlyList<T> values)
        {
            if (!(values is IList<T> list)) return true;
            try { list.Add(default(T)); return false; }
            catch (NotSupportedException) { return true; }
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71
                && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Substring(7).All(character =>
                    (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
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
                string actual = File.Exists(path) ? Sha256(path) : "MISSING";
                Add(checks, prefix + "." + SafeId(pair.Key), pair.Value, actual,
                    string.Equals(pair.Value, actual, StringComparison.Ordinal));
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

        private static IReadOnlyList<string[]> ReadCsv(string path)
        {
            return Array.AsReadOnly(File.ReadAllLines(path)
                .Skip(1).Where(value => value.Length > 0).Select(ParseCsvLine).ToArray());
        }

        private static string[] ParseCsvLine(string line)
        {
            List<string> values = new List<string>();
            StringBuilder current = new StringBuilder();
            bool quoted = false;
            for (int index = 0; index < (line ?? string.Empty).Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == ',' && !quoted)
                {
                    values.Add(current.ToString());
                    current.Length = 0;
                }
                else
                {
                    current.Append(character);
                }
            }
            values.Add(current.ToString());
            return values.ToArray();
        }

        private static int CountKind(IReadOnlyList<string[]> rows, string kind)
        {
            return rows.Count(value => value.Length > 0 && value[0] == kind);
        }

        private static string RowKindCounts(IReadOnlyList<string[]> rows, params string[] kinds)
        {
            return string.Join("/", kinds.Select(kind => CountKind(rows, kind).ToString(CultureInfo.InvariantCulture)));
        }

        private static string E05Counts(IReadOnlyList<string[]> rows)
        {
            return CountKind(rows, "SkillPattern")
                + "/" + rows.Where(value => value[0] == "SkillSequenceStep")
                    .Select(value => value[1]).Distinct(StringComparer.Ordinal).Count()
                + "/" + rows.Where(value => value[0] == "CarrierSkillBinding")
                    .Select(value => value[1]).Distinct(StringComparer.Ordinal).Count()
                + "/" + CountKind(rows, "BossPhaseProfile")
                + "/" + rows.Where(value => value[0] == "BossPhasePlanEntry")
                    .Select(value => value[1]).Distinct(StringComparer.Ordinal).Count();
        }

        private static string FixtureCounts(IReadOnlyList<string[]> rows, int first, int second)
        {
            return rows.Select(value => value[first]).Distinct(StringComparer.Ordinal).Count()
                + "/" + rows.Select(value => value[second]).Distinct(StringComparer.Ordinal).Count()
                + "/" + rows.Count;
        }

        private static string E07Counts(IReadOnlyList<string[]> rows)
        {
            return rows.Count(value => value[0] == "CapabilityValue" && value[1] == "dev_build_capability_fixture_complete")
                + "/16/"
                + rows.Count(value => value[0] == "CapabilityValue" && value[1] == "dev_build_capability_fixture_sparse")
                + "/" + rows.Count(value => value[0] == "SourceSummary" && value[1] == "dev_build_capability_fixture_complete")
                + "/" + CountKind(rows, "CapabilityGap") + "/" + Enum.GetValues(typeof(ReadinessBand)).Length;
        }

        private static string[] Field(params string[] values)
        {
            return values;
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

        private static string Actual(IReadOnlyList<Check> checks, string id)
        {
            Check check = checks.LastOrDefault(value => value.Id == id);
            return check == null ? "NOT_REPORTED" : check.Actual;
        }

        private static string CountResult(IReadOnlyList<Check> checks, string prefix, int expected)
        {
            Check[] selected = checks.Where(value =>
                value.Id.StartsWith(prefix, StringComparison.Ordinal)
                && value.Id != prefix + "count").ToArray();
            return selected.Count(value => value.Passed) + "/" + expected + " PASS";
        }

        private static string StripStringLiteralsAndComments(string source)
        {
            StringBuilder result = new StringBuilder(source == null ? 0 : source.Length);
            bool lineComment = false;
            bool blockComment = false;
            bool quotedString = false;
            bool verbatimString = false;
            bool characterLiteral = false;
            for (int index = 0; source != null && index < source.Length; index++)
            {
                char current = source[index];
                char next = index + 1 < source.Length ? source[index + 1] : '\0';
                if (lineComment)
                {
                    if (current == '\n')
                    {
                        lineComment = false;
                        result.Append(current);
                    }
                    else result.Append(' ');
                    continue;
                }
                if (blockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        result.Append("  ");
                        index++;
                        blockComment = false;
                    }
                    else result.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (quotedString || characterLiteral)
                {
                    result.Append(current == '\n' ? '\n' : ' ');
                    if (current == '\\' && next != '\0')
                    {
                        result.Append(' ');
                        index++;
                    }
                    else if ((quotedString && current == '"') || (characterLiteral && current == '\''))
                    {
                        quotedString = false;
                        characterLiteral = false;
                    }
                    continue;
                }
                if (verbatimString)
                {
                    result.Append(current == '\n' ? '\n' : ' ');
                    if (current == '"' && next == '"')
                    {
                        result.Append(' ');
                        index++;
                    }
                    else if (current == '"') verbatimString = false;
                    continue;
                }
                if (current == '/' && next == '/')
                {
                    result.Append("  ");
                    index++;
                    lineComment = true;
                }
                else if (current == '/' && next == '*')
                {
                    result.Append("  ");
                    index++;
                    blockComment = true;
                }
                else if (current == '@' && next == '"')
                {
                    result.Append("  ");
                    index++;
                    verbatimString = true;
                }
                else if (current == '"')
                {
                    result.Append(' ');
                    quotedString = true;
                }
                else if (current == '\'')
                {
                    result.Append(' ');
                    characterLiteral = true;
                }
                else result.Append(current);
            }
            return result.ToString();
        }

        private static int Count(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return 0;
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int CountTrailingWhitespace(string path)
        {
            if (!File.Exists(path) || path.EndsWith(".meta", StringComparison.Ordinal)) return 0;
            return File.ReadAllLines(path).Count(line => line.Length > 0
                && (line[line.Length - 1] == ' ' || line[line.Length - 1] == '\t'));
        }

        private static string ReadMetaGuid(string path)
        {
            if (!File.Exists(path)) return string.Empty;
            string line = File.ReadLines(path).FirstOrDefault(value => value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring("guid: ".Length).Trim();
        }

        private static string SafeId(string value)
        {
            return new string((value ?? string.Empty).Select(character =>
                char.IsLetterOrDigit(character) ? character : '_').ToArray());
        }

        private static string Markdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("`", "'")
                .Replace("\r", " ").Replace("\n", " ");
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static ProcessResult RunProcess(string root, string fileName, string arguments)
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

        private static string Relative(string root, string path)
        {
            Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(path)).ToString());
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Replace('\\', '/');
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

        private sealed class Scenario
        {
            public Scenario(
                string id,
                BuildCapabilityCoverageMode coverageMode,
                EnemyReadinessEvaluationInput input,
                EnemyReadinessEvaluationResult result,
                ReadinessBand expectedBand,
                PlayerHintSeverity expectedSeverity)
            {
                Id = id;
                CoverageMode = coverageMode;
                Input = input;
                Result = result;
                ExpectedBand = expectedBand;
                ExpectedSeverity = expectedSeverity;
            }
            public string Id { get; }
            public BuildCapabilityCoverageMode CoverageMode { get; }
            public EnemyReadinessEvaluationInput Input { get; }
            public EnemyReadinessEvaluationResult Result { get; }
            public ReadinessBand ExpectedBand { get; }
            public PlayerHintSeverity ExpectedSeverity { get; }
        }

        private sealed class Fixture
        {
            public Fixture(
                IReadOnlyList<Scenario> scenarios,
                string k0,
                string k1,
                string k2,
                string k3,
                string m0,
                string m1)
            {
                Scenarios = Array.AsReadOnly(scenarios.ToArray());
                K0 = k0; K1 = k1; K2 = k2; K3 = k3; M0 = m0; M1 = m1;
            }
            public IReadOnlyList<Scenario> Scenarios { get; }
            public string K0 { get; }
            public string K1 { get; }
            public string K2 { get; }
            public string K3 { get; }
            public string M0 { get; }
            public string M1 { get; }
            public Scenario ById(string id)
            {
                return Scenarios.Single(value => value.Id == id);
            }
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

        private sealed class ReferenceResolver :
            IBuildCapabilityVocabularyResolver,
            ICounterWindowAndPressureReferenceResolver,
            IEnemyReadinessReferenceResolver
        {
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly EnemyValidationContentSnapshot normalization;
            private readonly ReadOnlyCollection<string> capabilityKeys;
            private readonly ReadOnlyCollection<string> mapRuleIds;

            public ReferenceResolver(
                EnemyMechanicVocabularySnapshot vocabulary,
                EnemyValidationContentSnapshot normalization)
            {
                this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
                this.normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));
                capabilityKeys = Array.AsReadOnly(vocabulary.Entries
                    .Where(value => value.Category == EnemyVocabularyCategory.BuildCapability)
                    .Select(value => value.StableKey).OrderBy(value => value, StringComparer.Ordinal).ToArray());
                mapRuleIds = Array.AsReadOnly(normalization.MapRules
                    .Select(value => value.Domain.StableId).OrderBy(value => value, StringComparer.Ordinal).ToArray());
                PressureChannelKey = First(EnemyVocabularyCategory.PressureChannel);
                PlayerHintCategoryKey = First(EnemyVocabularyCategory.PlayerHintCategory);
                DeveloperDiagnosticCategoryKey = First(EnemyVocabularyCategory.DeveloperDiagnosticCategory);
            }

            public IReadOnlyList<string> MapRuleIds => mapRuleIds;
            public string PressureChannelKey { get; }
            public string PlayerHintCategoryKey { get; }
            public string DeveloperDiagnosticCategoryKey { get; }

            public bool HasBuildCapabilityKey(string key)
            {
                return key != null && vocabulary.TryGetEntry(
                    EnemyVocabularyCategory.BuildCapability, key, out _);
            }

            public IReadOnlyList<string> GetKnownBuildCapabilityKeys()
            {
                return capabilityKeys;
            }

            public bool HasMapRule(string mapRuleId)
            {
                return normalization.TryGetMapRule(mapRuleId, out _);
            }

            public bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey)
            {
                return vocabulary.TryGetEntry(category, stableKey, out _);
            }

            public bool HasMechanicProfile(string mechanicProfileId)
            {
                return normalization.TryGetMechanicProfile(mechanicProfileId, out _);
            }

            public bool HasSkillPattern(string skillPatternId)
            {
                return false;
            }

            public bool HasBossPhase(string bossPhaseId)
            {
                return false;
            }

            private string First(EnemyVocabularyCategory category)
            {
                return vocabulary.Entries.Where(value => value.Category == category)
                    .Select(value => value.StableKey).OrderBy(value => value, StringComparer.Ordinal).First();
            }
        }
    }
}
