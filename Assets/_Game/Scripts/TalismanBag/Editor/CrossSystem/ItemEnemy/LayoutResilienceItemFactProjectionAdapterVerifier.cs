using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class LayoutResilienceItemFactProjectionAdapterVerifier
    {
        private const string PackageName =
            "V0.4-LayoutResilienceItemFactProjectionAdapter01";
        private const string GuardReceipt =
            "GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01";
        private const string ItemGuardReceipt =
            "ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01";
        private const string AlgorithmGuardReceipt =
            "CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01";
        private const string ExpectedHead =
            "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba";
        private const string ExpectedCanonicalSignature =
            "sha256:06f84e83e10b05ca52225f2cc2b5afcc595c1cf34d86095a531b20a61f383382";
        private const string ExpectedItemHash =
            "b505eff26f246a2dfb34f04bfda8c9727e405ce8e02af5c48587ff4f880776d8";
        private const string ExpectedItemSnapshotHash =
            "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827";
        private const string ExpectedIF01Hash =
            "2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8";
        private const string ExpectedIF01Signature =
            "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedN01CHash =
            "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b";
        private const string ExpectedN01CSignature =
            "sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash =
            "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const string ExpectedC02Hash =
            "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4";
        private const string ExpectedC02AHash =
            "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb";
        private const string ExpectedN01Hash =
            "03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52";
        private const string ExpectedC02AD1Hash =
            "c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0";
        private const string ExpectedN01AHash =
            "705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de";
        private const string ExpectedN01BHash =
            "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316";
        private const string ExpectedScenesHash =
            "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsHash =
            "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash =
            "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";
        private const string ExpectedPreexistingCatalogHash =
            "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45";
        private const string ExpectedPreexistingCandidateHash =
            "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb";

        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReport = ReportDirectory +
            "/LayoutResilienceItemFactProjectionAdapterReport.md";
        private const string SpecReport = ReportDirectory +
            "/LayoutResilienceItemFactProjectionAdapterSpec.csv";
        private const string FieldReport = ReportDirectory +
            "/LayoutResilienceItemFactProjectionAdapterFieldMatrix.csv";
        private const string FixtureReport = ReportDirectory +
            "/LayoutResilienceItemFactProjectionAdapterFixtureCases.csv";
        private const string LeakReport = ReportDirectory +
            "/LayoutResilienceItemFactProjectionAdapterLeakCheckReport.md";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionPrimitives.cs",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionPrimitives.cs.meta",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionAdapter.cs",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionAdapter.cs.meta",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionValidation.cs",
            RuntimeDirectory + "/LayoutResilienceItemFactProjectionValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs.meta",
            MainReport,
            SpecReport,
            FieldReport,
            FixtureReport,
            LeakReport
        };

        private static readonly string[] C01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionAdapterReport.md",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionFixtureRows.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionUnknowns.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionLeakCheckReport.md"
        };

        private static readonly string[] C02Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemEnemyMatchupSimulationReport.md",
            "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupLeakCheckReport.md"
        };

        private static readonly string[] C02AFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyReport.md",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingSourceEvidence.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingDecisionList.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyLeakCheckReport.md"
        };

        private static readonly string[] N01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyReport.md",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationSourceEligibilityMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleCandidates.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationC02ImpactMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationUserDecisionSheet.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyLeakCheckReport.md"
        };

        private static readonly string[] C02AD1Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionReport.md",
            "Docs/V0.4/Reports/ItemCapabilityRuleCandidates.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleImpactMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionSheet.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionLeakCheckReport.md"
        };

        private static readonly string[] N01AFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSourceEvidence.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerC02Impact.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerUserDecisionSheet.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerLeakCheckReport.md"
        };

        private static readonly string[] N01BFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md"
        };

        private static readonly string[] IF01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractReport.md",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractSpec.csv",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractLeakCheckReport.md"
        };

        private static readonly string[] N01CFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs.meta",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFieldMatrix.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFixtureCases.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/[QA Only] Verify Layout Item Fact Projection")]
        public static void VerifyMenu()
        {
            VerifyOrThrow("Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyOrThrow("Pure C# offline verifier");
        }

        public static void VerifyStaticBatch()
        {
            VerifyOrThrow("Unity batch compile/verifier");
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

        public static string ComputeI031V2CanonicalForMigration()
        {
            return DefaultLayoutResilienceItemFactProjectionAdapter.Instance
                .Project(BaseItem(false), BaseBinding(false))
                .CanonicalSignature;
        }

        private static void VerifyOrThrow(string mode)
        {
            string root = FindProjectRoot();
            ProtectionSnapshot before = CaptureProtection(root);
            List<Check> checks = new List<Check>();
            VerifySchema(checks);
            List<Scenario> scenarios = VerifyFixtures(checks);
            VerifyMappingDeterminismAndImmutability(checks, scenarios);
            VerifyReportsAndLeaks(root, checks, scenarios);
            VerifyRepositoryBoundary(root, checks);
            ProtectionSnapshot after = CaptureProtection(root);
            VerifyProtection(checks, before, after);

            Check[] failed = checks.Where(value => !value.Passed).ToArray();
            string signature = scenarios[0].Result.CanonicalSignature;
            Console.WriteLine("LAYOUT_RESILIENCE_ITEM_FACT_PROJECTION_ADAPTER01 " +
                (failed.Length == 0 ? "PASS" : "FAIL") + " checks=" +
                checks.Count.ToString(CultureInfo.InvariantCulture) + " mode=" + mode);
            Console.WriteLine("CANONICAL_SIGNATURE=" + signature);
            Console.WriteLine("SCENARIOS=" + scenarios.Count.ToString(
                CultureInfo.InvariantCulture) + " COMPLETE=" + scenarios.Count(value =>
                value.Result.Status == LayoutResilienceItemFactProjectionStatus.Complete) +
                " UNKNOWN=" + scenarios.Count(value => value.Result.Status ==
                    LayoutResilienceItemFactProjectionStatus.Unknown) +
                " INVALID=" + scenarios.Count(value => value.Result.Status ==
                    LayoutResilienceItemFactProjectionStatus.Invalid));
            Console.WriteLine("FACT_ROWS=" + scenarios.Sum(value =>
                value.Result.BuildFacts == null ? 0 :
                    value.Result.BuildFacts.PlacementRows.Count));
            Console.WriteLine("N01C_EVALUATOR_CALLS=0 PRESSURE_ROWS=0 LEAK_COUNT=0");
            if (failed.Length > 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine,
                    failed.Select(value => value.Id + " expected=" + value.Expected +
                        " actual=" + value.Actual)));
            }
        }

        private static void VerifySchema(ICollection<Check> checks)
        {
            Add(checks, "schema.id", "LayoutResilienceItemFactProjectionAdapter.v1",
                LayoutResilienceItemFactProjectionSchema.SchemaId,
                LayoutResilienceItemFactProjectionSchema.SchemaId ==
                    "LayoutResilienceItemFactProjectionAdapter.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceItemFactProjectionSchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                LayoutResilienceItemFactProjectionSchema.SchemaVersion == 1);
            AssertEnum<LayoutResilienceItemFactProjectionStatus>(checks, "status",
                new[] { "Complete", "Unknown", "Invalid" }, new[] { 1, 2, 3 });
            AssertProperties(checks, "issue",
                typeof(LayoutResilienceItemFactProjectionIssue),
                "Code", "Status", "ItemInstanceId", "PlacementId", "ItemId",
                "Message");
            AssertProperties(checks, "result",
                typeof(LayoutResilienceItemFactProjectionResult),
                "SchemaId", "SchemaVersion", "Status", "BuildFactsCompleteness",
                "BuildFacts", "Issues", "CanonicalSignature");
            Add(checks, "adapter.interface", "True",
                typeof(ILayoutResilienceItemFactProjectionAdapter).IsAssignableFrom(
                    typeof(DefaultLayoutResilienceItemFactProjectionAdapter)).ToString(),
                typeof(ILayoutResilienceItemFactProjectionAdapter).IsAssignableFrom(
                    typeof(DefaultLayoutResilienceItemFactProjectionAdapter)));
        }

        private static List<Scenario> VerifyFixtures(ICollection<Check> checks)
        {
            DefaultLayoutResilienceItemFactProjectionAdapter adapter =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance;
            List<Scenario> scenarios = new List<Scenario>();

            ItemSystemSnapshot baseItem = BaseItem(false);
            ItemInstancePlacementBindingContractSnapshot baseBinding = BaseBinding(false);
            AddScenario(scenarios, "dev_projection_case_01_complete",
                adapter.Project(baseItem, baseBinding),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 3, string.Empty);
            AddScenario(scenarios, "dev_projection_case_02_reversed",
                adapter.Project(BaseItem(true), BaseBinding(true)),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 3, string.Empty);

            ItemSystemSnapshot sameBase = Item(
                new[] { Catalog("dev_item_shared", V(0, 0)) },
                new[]
                {
                    Placement("dev_place_shared_a", "dev_item_shared", V(0, 0), 0,
                        new[] { V(0, 0) }, V(0, 0), true, true),
                    Placement("dev_place_shared_b", "dev_item_shared", V(1, 0), 0,
                        new[] { V(1, 0) }, V(1, 0), true, true)
                });
            AddScenario(scenarios, "dev_projection_case_03_same_base_multiple",
                adapter.Project(sameBase, Binding(ItemInstancePlacementBindingStatus.Valid,
                    new[] { "dev_instance_shared_a", "dev_instance_shared_b" },
                    new[] { "dev_place_shared_a", "dev_place_shared_b" },
                    new[] { "dev_item_shared", "dev_item_shared" })),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 2, string.Empty);

            AddScenario(scenarios, "dev_projection_case_04_empty",
                adapter.Project(Item(Array.Empty<ItemSystemCatalogItemSnapshot>(),
                    Array.Empty<ItemSystemPlacementSnapshot>()), null),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 0, string.Empty);

            ItemSystemSnapshot i031Only = Item(
                new[] { Catalog("I031", V(0, 0)) },
                new[] { Placement("P_SYSTEM_I031", "I031", V(0, 0), 0,
                    new[] { V(0, 0) }, V(0, 0), true, true) });
            AddScenario(scenarios, "dev_projection_case_05_i031_only",
                adapter.Project(i031Only, null),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 1, string.Empty);

            ItemSystemSnapshot uncounted = Item(
                new[] { Catalog("dev_item_uncounted", V(0, 0)) },
                new[] { Placement("dev_place_uncounted", "dev_item_uncounted",
                    V(1, 1), 0, new[] { V(1, 1) }, V(1, 1), false, false) });
            AddScenario(scenarios, "dev_projection_case_06_all_uncounted",
                adapter.Project(uncounted, Binding(ItemInstancePlacementBindingStatus.Valid,
                    new[] { "dev_instance_uncounted" },
                    new[] { "dev_place_uncounted" },
                    new[] { "dev_item_uncounted" })),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 1, string.Empty);

            AddScenario(scenarios, "dev_projection_case_07_item_missing",
                adapter.Project(null, null),
                LayoutResilienceItemFactProjectionStatus.Unknown,
                LayoutResilienceInputCompleteness.Incomplete, 0,
                LayoutResilienceItemFactProjectionValidationCodes.ItemSourceMissing);

            ItemSystemSnapshot oneOrdinary = Item(
                new[] { Catalog("dev_item_single", V(0, 0)) },
                new[] { Placement("dev_place_single", "dev_item_single", V(0, 0),
                    0, new[] { V(0, 0) }, V(0, 0), true, true) });
            AddScenario(scenarios, "dev_projection_case_08_binding_missing",
                adapter.Project(oneOrdinary, null),
                LayoutResilienceItemFactProjectionStatus.Unknown,
                LayoutResilienceInputCompleteness.Incomplete, 1,
                LayoutResilienceItemFactProjectionValidationCodes.BindingSourceMissing);

            ItemInstancePlacementBindingValidationError unknownBindingError =
                new ItemInstancePlacementBindingValidationError(
                    ItemInstancePlacementBindingValidationCodes.BindingRowMissing,
                    ItemInstancePlacementBindingStatus.Unknown,
                    string.Empty, "dev_place_single", "dev_item_single",
                    "Synthetic missing binding.");
            AddScenario(scenarios, "dev_projection_case_09_if01_unknown",
                adapter.Project(oneOrdinary, Binding(
                    ItemInstancePlacementBindingStatus.Unknown,
                    Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(),
                    new[] { unknownBindingError })),
                LayoutResilienceItemFactProjectionStatus.Unknown,
                LayoutResilienceInputCompleteness.Incomplete, 1,
                LayoutResilienceItemFactProjectionValidationCodes.BindingSourceUnknown);

            AddScenario(scenarios, "dev_projection_case_10_orphan_binding",
                adapter.Project(oneOrdinary, Binding(
                    ItemInstancePlacementBindingStatus.Valid,
                    new[] { "dev_instance_single", "dev_instance_orphan" },
                    new[] { "dev_place_single", "dev_place_orphan" },
                    new[] { "dev_item_single", "dev_item_orphan" })),
                LayoutResilienceItemFactProjectionStatus.Unknown,
                LayoutResilienceInputCompleteness.Incomplete, 1,
                LayoutResilienceItemFactProjectionValidationCodes.BindingPlacementOrphan);

            ItemSystemSnapshot invalidItem = Item(
                new[] { Catalog("dev_item_invalid", V(0, 0)) },
                new[] { Placement("dev_place_invalid", "dev_item_invalid", V(0, 0),
                    0, new[] { V(0, 0) }, V(0, 0), true, true) },
                new[] { ItemSystemValidationError.Error("DEV_INVALID",
                    "dev_place_invalid", "dev_item_invalid", "Synthetic invalid item.") });
            AddScenario(scenarios, "dev_projection_case_11_item_invalid",
                adapter.Project(invalidItem, null),
                LayoutResilienceItemFactProjectionStatus.Invalid,
                LayoutResilienceInputCompleteness.Incomplete, 0,
                LayoutResilienceItemFactProjectionValidationCodes.ItemSourceInvalid);

            ItemInstancePlacementBindingValidationError invalidBindingError =
                new ItemInstancePlacementBindingValidationError(
                    ItemInstancePlacementBindingValidationCodes
                        .BindingItemInstanceIdDuplicate,
                    ItemInstancePlacementBindingStatus.Invalid,
                    "dev_instance_duplicate", "dev_place_single", "dev_item_single",
                    "Synthetic duplicate identity.");
            AddScenario(scenarios, "dev_projection_case_12_if01_invalid_duplicate",
                adapter.Project(oneOrdinary, Binding(
                    ItemInstancePlacementBindingStatus.Invalid,
                    new[] { "dev_instance_duplicate", "dev_instance_duplicate" },
                    new[] { "dev_place_single", "dev_place_other" },
                    new[] { "dev_item_single", "dev_item_other" },
                    new[] { invalidBindingError })),
                LayoutResilienceItemFactProjectionStatus.Invalid,
                LayoutResilienceInputCompleteness.Incomplete, 0,
                LayoutResilienceItemFactProjectionValidationCodes
                    .BindingItemInstanceIdDuplicate);

            AddScenario(scenarios, "dev_projection_case_13_base_mismatch",
                adapter.Project(oneOrdinary, Binding(
                    ItemInstancePlacementBindingStatus.Valid,
                    new[] { "dev_instance_single" },
                    new[] { "dev_place_single" },
                    new[] { "dev_item_wrong" })),
                LayoutResilienceItemFactProjectionStatus.Invalid,
                LayoutResilienceInputCompleteness.Incomplete, 0,
                LayoutResilienceItemFactProjectionValidationCodes
                    .BindingBaseItemIdMismatch);

            AddScenario(scenarios, "dev_projection_case_14_i031_forgery",
                adapter.Project(i031Only, Binding(
                    ItemInstancePlacementBindingStatus.Valid,
                    new[] { "dev_instance_i031_forbidden" },
                    new[] { "P_SYSTEM_I031" },
                    new[] { "I031" })),
                LayoutResilienceItemFactProjectionStatus.Invalid,
                LayoutResilienceInputCompleteness.Incomplete, 0,
                LayoutResilienceItemFactProjectionValidationCodes
                    .I031OrdinaryBindingForbidden);

            AddScenario(scenarios, "dev_projection_case_15_immutable_culture",
                adapter.Project(BaseItem(false), BaseBinding(false)),
                LayoutResilienceItemFactProjectionStatus.Complete,
                LayoutResilienceInputCompleteness.Complete, 3, string.Empty);

            foreach (Scenario scenario in scenarios)
            {
                Add(checks, "fixture." + scenario.Id + ".status",
                    scenario.ExpectedStatus.ToString(), scenario.Result.Status.ToString(),
                    scenario.Result.Status == scenario.ExpectedStatus);
                Add(checks, "fixture." + scenario.Id + ".completeness",
                    scenario.ExpectedCompleteness.ToString(),
                    scenario.Result.BuildFactsCompleteness.ToString(),
                    scenario.Result.BuildFactsCompleteness ==
                        scenario.ExpectedCompleteness);
                int actualRows = scenario.Result.BuildFacts == null ? 0 :
                    scenario.Result.BuildFacts.PlacementRows.Count;
                Add(checks, "fixture." + scenario.Id + ".rows",
                    scenario.ExpectedRows.ToString(CultureInfo.InvariantCulture),
                    actualRows.ToString(CultureInfo.InvariantCulture),
                    actualRows == scenario.ExpectedRows);
                Add(checks, "fixture." + scenario.Id + ".code", scenario.ExpectedCode,
                    string.Join("|", scenario.Result.Issues.Select(value => value.Code)),
                    string.IsNullOrEmpty(scenario.ExpectedCode) || scenario.Result.Issues
                        .Any(value => value.Code == scenario.ExpectedCode));
                Add(checks, "fixture." + scenario.Id + ".result-valid", "0",
                    LayoutResilienceItemFactProjectionValidation.ValidateResult(
                        scenario.Result).Count.ToString(CultureInfo.InvariantCulture),
                    LayoutResilienceItemFactProjectionValidation.ValidateResult(
                        scenario.Result).Count == 0);
                Add(checks, "fixture." + scenario.Id + ".signature", "sha256:64lower",
                    scenario.Result.CanonicalSignature,
                    IsSignature(scenario.Result.CanonicalSignature));
            }

            Add(checks, "fixtures.count", "15", scenarios.Count.ToString(
                CultureInfo.InvariantCulture), scenarios.Count == 15);
            Add(checks, "fixtures.complete", "7", scenarios.Count(value =>
                value.Result.Status == LayoutResilienceItemFactProjectionStatus.Complete)
                .ToString(CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Result.Status ==
                    LayoutResilienceItemFactProjectionStatus.Complete) == 7);
            Add(checks, "fixtures.unknown", "4", scenarios.Count(value =>
                value.Result.Status == LayoutResilienceItemFactProjectionStatus.Unknown)
                .ToString(CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Result.Status ==
                    LayoutResilienceItemFactProjectionStatus.Unknown) == 4);
            Add(checks, "fixtures.invalid", "4", scenarios.Count(value =>
                value.Result.Status == LayoutResilienceItemFactProjectionStatus.Invalid)
                .ToString(CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Result.Status ==
                    LayoutResilienceItemFactProjectionStatus.Invalid) == 4);
            Add(checks, "fixture.fixed-signature", ExpectedCanonicalSignature,
                scenarios[0].Result.CanonicalSignature,
                scenarios[0].Result.CanonicalSignature == ExpectedCanonicalSignature);
            return scenarios;
        }

        private static void VerifyMappingDeterminismAndImmutability(
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            LayoutResilienceItemFactProjectionResult complete = scenarios[0].Result;
            LayoutResilienceItemFactProjectionResult reversed = scenarios[1].Result;
            Add(checks, "determinism.reverse", complete.CanonicalSignature,
                reversed.CanonicalSignature,
                complete.CanonicalSignature == reversed.CanonicalSignature);
            Add(checks, "determinism.idempotent", complete.CanonicalSignature,
                scenarios[14].Result.CanonicalSignature,
                complete.CanonicalSignature == scenarios[14].Result.CanonicalSignature);

            LayoutResilienceBuildFactSnapshot build = complete.BuildFacts;
            Add(checks, "mapping.board", "5", build.BoardSize.ToString(),
                build.BoardSize == 5);
            Add(checks, "mapping.eye", "2:2", build.BaselineEyeCell.ToString(),
                build.BaselineEyeCell.X == 2 && build.BaselineEyeCell.Y == 2);
            LayoutResiliencePlacedItemFactSnapshot ordinary = build.PlacementRows
                .Single(value => value.PlacementId == "dev_place_a");
            Add(checks, "mapping.placementId", "dev_place_a", ordinary.PlacementId,
                ordinary.PlacementId == "dev_place_a");
            Add(checks, "mapping.itemId", "dev_item_a", ordinary.ItemId,
                ordinary.ItemId == "dev_item_a");
            Add(checks, "mapping.anchor", "0:0", ordinary.AnchorCell.ToString(),
                ordinary.AnchorCell.X == 0 && ordinary.AnchorCell.Y == 0);
            Add(checks, "mapping.rotation", "0", ordinary.Rotation.ToString(
                CultureInfo.InvariantCulture), ordinary.Rotation == 0);
            Add(checks, "mapping.shape", "0:0|1:0", string.Join("|",
                ordinary.ShapeCells.Select(value => value.ToString())),
                CellsEqual(ordinary.ShapeCells, new[] { Cell(0, 0), Cell(1, 0) }));
            Add(checks, "mapping.occupied", "0:0|1:0", string.Join("|",
                ordinary.OccupiedCells.Select(value => value.ToString())),
                CellsEqual(ordinary.OccupiedCells, new[] { Cell(0, 0), Cell(1, 0) }));
            Add(checks, "mapping.core", "0:0", ordinary.CoreCellWorld.ToString(),
                ordinary.CoreCellWorld.X == 0 && ordinary.CoreCellWorld.Y == 0);
            Add(checks, "mapping.lit", "True", ordinary.IsLit.ToString(),
                ordinary.IsLit);
            Add(checks, "mapping.counted", "True", ordinary.IsCountedInBuild.ToString(),
                ordinary.IsCountedInBuild);

            LayoutResiliencePlacedItemFactSnapshot i031 = build.PlacementRows.Single(value =>
                value.ItemId == "I031");
            Add(checks, "i031.lit-copy", "False", i031.IsLit.ToString(), !i031.IsLit);
            Add(checks, "i031.counted-copy", "False",
                i031.IsCountedInBuild.ToString(), !i031.IsCountedInBuild);
            Add(checks, "i031.no-instance-field", "False",
                typeof(LayoutResiliencePlacedItemFactSnapshot).GetProperties(
                    BindingFlags.Public | BindingFlags.Instance).Any(value =>
                    value.Name == "ItemInstanceId").ToString(),
                !typeof(LayoutResiliencePlacedItemFactSnapshot).GetProperties(
                    BindingFlags.Public | BindingFlags.Instance).Any(value =>
                    value.Name == "ItemInstanceId"));

            LayoutResilienceItemFactProjectionResult changedFact =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    BaseItem(false, true, true), BaseBinding(false));
            Add(checks, "signature.fact-sensitive", "different",
                changedFact.CanonicalSignature,
                changedFact.CanonicalSignature != complete.CanonicalSignature);
            LayoutResiliencePlacedItemFactSnapshot changedI031 = changedFact.BuildFacts
                .PlacementRows.Single(value => value.ItemId == "I031");
            Add(checks, "i031.lit-nondefault-copy", "True",
                changedI031.IsLit.ToString(), changedI031.IsLit);
            Add(checks, "i031.counted-nondefault-copy", "True",
                changedI031.IsCountedInBuild.ToString(),
                changedI031.IsCountedInBuild);
            LayoutResilienceItemFactProjectionResult changedInstance =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    BaseItem(false), Binding(ItemInstancePlacementBindingStatus.Valid,
                        new[] { "dev_instance_a_changed", "dev_instance_b" },
                        new[] { "dev_place_a", "dev_place_b" },
                        new[] { "dev_item_a", "dev_item_b" }));
            Add(checks, "signature.binding-sensitive", "different",
                changedInstance.CanonicalSignature,
                changedInstance.CanonicalSignature != complete.CanonicalSignature);

            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
                string cultureSignature =
                    DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                        BaseItem(false), BaseBinding(false)).CanonicalSignature;
                Add(checks, "determinism.culture", complete.CanonicalSignature,
                    cultureSignature, cultureSignature == complete.CanonicalSignature);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }

            AddThrowsNotSupported(checks, "immutable.placement-rows", () =>
                ((IList)build.PlacementRows).Add(ordinary));
            AddThrowsNotSupported(checks, "immutable.shape-cells", () =>
                ((IList)ordinary.ShapeCells).Add(Cell(4, 4)));
            LayoutResilienceItemFactProjectionResult unknown = scenarios[7].Result;
            AddThrowsNotSupported(checks, "immutable.issues", () =>
                ((IList)unknown.Issues).Add(unknown.Issues[0]));

            List<Vector2Int> mutableShape = new List<Vector2Int> { V(0, 0) };
            ItemInnerDataDefinition definition = Definition("dev_item_defensive",
                mutableShape);
            ItemSystemCatalogItemSnapshot catalog =
                new ItemSystemCatalogItemSnapshot(definition);
            List<ItemSystemPlacementSnapshot> mutablePlacements =
                new List<ItemSystemPlacementSnapshot>
                {
                    Placement("dev_place_defensive", "dev_item_defensive", V(0, 0),
                        0, new[] { V(0, 0) }, V(0, 0), true, true)
                };
            ItemSystemSnapshot defensiveItem = Item(new[] { catalog }, mutablePlacements);
            mutableShape.Add(V(1, 0));
            mutablePlacements.Clear();
            LayoutResilienceItemFactProjectionResult defensive =
                DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                    defensiveItem, Binding(ItemInstancePlacementBindingStatus.Valid,
                        new[] { "dev_instance_defensive" },
                        new[] { "dev_place_defensive" },
                        new[] { "dev_item_defensive" }));
            Add(checks, "immutable.source-defensive", "1:1", defensive.BuildFacts
                .PlacementRows.Count + ":" + defensive.BuildFacts.PlacementRows[0]
                    .ShapeCells.Count,
                defensive.BuildFacts.PlacementRows.Count == 1 &&
                defensive.BuildFacts.PlacementRows[0].ShapeCells.Count == 1);
        }

        private static void VerifyReportsAndLeaks(
            string root,
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            foreach (string path in OutputPaths)
            {
                Add(checks, "output.exists." + path, "True",
                    File.Exists(Absolute(root, path)).ToString(),
                    File.Exists(Absolute(root, path)));
            }
            string main = Read(root, MainReport);
            foreach (string token in new[]
            {
                PackageName, GuardReceipt, ItemGuardReceipt, AlgorithmGuardReceipt,
                "LayoutResilienceItemFactProjectionAdapter.v1", "15/15",
                "Complete: `7`", "Unknown: `4`", "Invalid: `4`",
                ExpectedCanonicalSignature, ExpectedIF01Signature,
                ExpectedN01CSignature, "N01C Evaluator calls: `0`",
                "Pressure rows: `0`", "Existing file modifications: `0`",
                "C02 blocked rows unchanged: `32/32`",
                "Actual Unknown reduction: `0`", "Leak Count: `0`"
            })
            {
                AddContains(checks, "report.main." + token, main, token);
            }
            Add(checks, "report.spec.rows", ">=45", CsvRows(root, SpecReport).Length
                .ToString(CultureInfo.InvariantCulture),
                CsvRows(root, SpecReport).Length >= 45);
            Add(checks, "report.field.rows", ">=20", CsvRows(root, FieldReport).Length
                .ToString(CultureInfo.InvariantCulture),
                CsvRows(root, FieldReport).Length >= 20);
            Add(checks, "report.fixture.rows", "15", CsvRows(root, FixtureReport).Length
                .ToString(CultureInfo.InvariantCulture),
                CsvRows(root, FixtureReport).Length == 15);
            foreach (Scenario scenario in scenarios)
            {
                AddContains(checks, "report.fixture." + scenario.Id,
                    Read(root, FixtureReport), scenario.Id);
            }
            string leak = Read(root, LeakReport);
            foreach (string token in new[]
            {
                "Leak Count: `0`", "Private Item state", "I031 identity forgery",
                "Pressure authoring", "N01C Evaluator", "Requirement mapping",
                "Readiness mapping", "Board/Map/Battle/Scene/UI",
                "BP/score/threshold", "Runtime Producer", "Package allowlist",
                "GUID collision", "Trailing whitespace"
            })
            {
                AddContains(checks, "report.leak." + token, leak, token);
            }

            string runtime = string.Join("\n", Directory.GetFiles(
                    Absolute(root, RuntimeDirectory), "*.cs", SearchOption.TopDirectoryOnly)
                .OrderBy(value => value, StringComparer.Ordinal)
                .Select(value => File.ReadAllText(value, Encoding.UTF8)));
            foreach (string token in new[]
            {
                "DefaultLayoutResilienceStructuralPredicateEvaluator",
                "AutoCombatController", "TalismanItemRuntime", "MonoBehaviour",
                "ScriptableObject", "BuildDebugSignature", "SerializedObject",
                "ItemSystemSnapshotProvider", "ItemSystemValidator",
                "ItemInstancePlacementBindingValidator", "RequirementId",
                "ReadinessBand", "BuildCapability", "BasisPoint", "Threshold",
                "UnityEditor", "SceneManagement", "GameObject", "RectTransform"
            })
            {
                Add(checks, "leak.runtime." + token, "0",
                    CountOrdinal(runtime, token).ToString(CultureInfo.InvariantCulture),
                    CountOrdinal(runtime, token) == 0);
            }
            Add(checks, "leak.evaluator-call", "0",
                CountOrdinal(runtime, ".Evaluate(").ToString(CultureInfo.InvariantCulture),
                CountOrdinal(runtime, ".Evaluate(") == 0);
            Add(checks, "leak.pressure-object", "0",
                CountOrdinal(runtime, "new LayoutPressureSnapshot").ToString(
                    CultureInfo.InvariantCulture),
                CountOrdinal(runtime, "new LayoutPressureSnapshot") == 0);
            Add(checks, "leak.real-id", "0",
                CountOrdinal(Read(root, FixtureReport), "requirement.").ToString(
                    CultureInfo.InvariantCulture),
                CountOrdinal(Read(root, FixtureReport), "requirement.") == 0);

            string[] c02Rows = CsvRows(root,
                "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv");
            int blockedRows = c02Rows.Count(value => value.IndexOf(
                "BLOCKED_BY_UNKNOWN", StringComparison.Ordinal) >= 0);
            Add(checks, "baseline.c02.rows", "32", c02Rows.Length.ToString(
                CultureInfo.InvariantCulture), c02Rows.Length == 32);
            Add(checks, "baseline.c02.blocked", "32", blockedRows.ToString(
                CultureInfo.InvariantCulture), blockedRows == 32);
            Add(checks, "baseline.actual-unknown-reduction", "0",
                (32 - blockedRows).ToString(CultureInfo.InvariantCulture),
                32 - blockedRows == 0);
        }

        private static void VerifyRepositoryBoundary(
            string root,
            ICollection<Check> checks)
        {
            Add(checks, "repo.head", ExpectedHead, RunGit(root, "rev-parse HEAD").Trim(),
                RunGit(root, "rev-parse HEAD").Trim() == ExpectedHead);
            string[] modified = Lines(RunGit(root, "diff --name-only"));
            string[] accepted =
            {
                "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset",
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"
            };
            Add(checks, "repo.preexisting-modified-count", "2",
                modified.Length.ToString(CultureInfo.InvariantCulture),
                modified.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                    accepted.OrderBy(value => value, StringComparer.Ordinal)));
            Add(checks, "repo.existing-file-modifications", "0",
                modified.Except(accepted, StringComparer.Ordinal).Count().ToString(
                    CultureInfo.InvariantCulture),
                !modified.Except(accepted, StringComparer.Ordinal).Any());
            Add(checks, "repo.preexisting.catalog", ExpectedPreexistingCatalogHash,
                FileHash(root, accepted[0], false),
                FileHash(root, accepted[0], false) == ExpectedPreexistingCatalogHash);
            Add(checks, "repo.preexisting.candidate", ExpectedPreexistingCandidateHash,
                FileHash(root, accepted[1], false),
                FileHash(root, accepted[1], false) == ExpectedPreexistingCandidateHash);
            string trackedOutputs = RunGit(root, "ls-files -- " + string.Join(" ",
                OutputPaths.Select(value => "\"" + value + "\""))).Trim();
            Add(checks, "repo.outputs-untracked", "", trackedOutputs,
                trackedOutputs.Length == 0);
            string diffCheck = RunGit(root, "diff --check").Trim();
            Add(checks, "repo.diff-check", "", diffCheck, diffCheck.Length == 0);

            string[] currentGuids = OutputPaths.Where(value =>
                    value.EndsWith(".meta", StringComparison.Ordinal))
                .Select(value => ReadGuid(Absolute(root, value))).ToArray();
            Add(checks, "repo.guid.nonempty", "5", currentGuids.Count(value =>
                value.Length == 32).ToString(CultureInfo.InvariantCulture),
                currentGuids.Length == 5 && currentGuids.All(value => value.Length == 32));
            string[] allGuids = Directory.GetFiles(Absolute(root, "Assets"), "*.meta",
                    SearchOption.AllDirectories).Select(ReadGuid)
                .Where(value => value.Length > 0).ToArray();
            int collisions = currentGuids.Count(guid =>
                allGuids.Count(value => value == guid) != 1);
            Add(checks, "repo.guid.collisions", "0", collisions.ToString(
                CultureInfo.InvariantCulture), collisions == 0);

            int trailingWhitespace = OutputPaths.Where(path =>
                    path.EndsWith(".cs", StringComparison.Ordinal) ||
                    path.EndsWith(".md", StringComparison.Ordinal) ||
                    path.EndsWith(".csv", StringComparison.Ordinal) ||
                    path.EndsWith(".meta", StringComparison.Ordinal))
                .SelectMany(path => File.ReadAllLines(Absolute(root, path), Encoding.UTF8))
                .Count(line => line.Length > 0 && char.IsWhiteSpace(line[line.Length - 1]));
            Add(checks, "repo.trailing-whitespace", "0", trailingWhitespace.ToString(
                CultureInfo.InvariantCulture), trailingWhitespace == 0);
        }

        private static ItemSystemSnapshot BaseItem(
            bool reverse,
            bool i031Lit = false,
            bool i031Counted = false)
        {
            ItemSystemCatalogItemSnapshot[] catalog =
            {
                Catalog("dev_item_a", V(0, 0), V(1, 0)),
                Catalog("dev_item_b", V(0, 0)),
                Catalog("I031", V(0, 0))
            };
            ItemSystemPlacementSnapshot[] placements =
            {
                Placement("dev_place_a", "dev_item_a", V(0, 0), 0,
                    new[] { V(0, 0), V(1, 0) }, V(0, 0), true, true),
                Placement("dev_place_b", "dev_item_b", V(4, 4), 270,
                    new[] { V(4, 4) }, V(4, 4), true, true),
                Placement("P_SYSTEM_I031", "I031", V(3, 3), 0,
                    new[] { V(3, 3) }, V(3, 3), i031Lit, i031Counted)
            };
            if (reverse)
            {
                Array.Reverse(catalog);
                Array.Reverse(placements);
            }
            return Item(catalog, placements);
        }

        private static ItemInstancePlacementBindingContractSnapshot BaseBinding(
            bool reverse)
        {
            string[] instances = { "dev_instance_a", "dev_instance_b" };
            string[] placements = { "dev_place_a", "dev_place_b" };
            string[] bases = { "dev_item_a", "dev_item_b" };
            if (reverse)
            {
                Array.Reverse(instances);
                Array.Reverse(placements);
                Array.Reverse(bases);
            }
            return Binding(ItemInstancePlacementBindingStatus.Valid,
                instances, placements, bases);
        }

        private static ItemInstancePlacementBindingContractSnapshot Binding(
            ItemInstancePlacementBindingStatus status,
            IEnumerable<string> instances,
            IEnumerable<string> placements,
            IEnumerable<string> bases,
            IEnumerable<ItemInstancePlacementBindingValidationError> errors = null)
        {
            return LayoutResilienceItemFactProjectionValidation
                .CreateBindingContractForFixture(
                    status, instances, placements, bases, errors);
        }

        private static ItemSystemSnapshot Item(
            IEnumerable<ItemSystemCatalogItemSnapshot> catalog,
            IEnumerable<ItemSystemPlacementSnapshot> placements,
            IEnumerable<ItemSystemValidationError> errors = null)
        {
            return new ItemSystemSnapshot(
                5,
                V(2, 2),
                Array.Empty<Vector2Int>(),
                (catalog ?? Array.Empty<ItemSystemCatalogItemSnapshot>()).ToArray(),
                (placements ?? Array.Empty<ItemSystemPlacementSnapshot>()).ToArray(),
                Array.Empty<Vector2Int>(),
                null,
                null,
                null,
                null,
                null,
                string.Empty,
                false,
                string.Empty,
                (errors ?? Array.Empty<ItemSystemValidationError>()).ToArray(),
                new I031InventoryPlacementStateSnapshot(
                    I031InventoryPlacementContract.ItemId,
                    I031InventoryPlacementContract.SpecialIdentityId,
                    I031InventoryPlacementContract.StablePlacementId,
                    I031OwnershipCompleteness.Complete,
                    I031Location.Board,
                    true));
        }

        private static ItemSystemCatalogItemSnapshot Catalog(
            string itemId,
            params Vector2Int[] cells)
        {
            return new ItemSystemCatalogItemSnapshot(
                Definition(itemId, cells == null
                    ? new List<Vector2Int>()
                    : cells.ToList()));
        }

        private static ItemInnerDataDefinition Definition(
            string itemId,
            List<Vector2Int> cells)
        {
            return new ItemInnerDataDefinition
            {
                itemId = itemId,
                defaultLocalCells = cells ?? new List<Vector2Int>(),
                coreCellLocal = cells != null && cells.Count > 0
                    ? cells[0]
                    : Vector2Int.zero
            };
        }

        private static ItemSystemPlacementSnapshot Placement(
            string placementId,
            string itemId,
            Vector2Int anchor,
            int rotation,
            IReadOnlyList<Vector2Int> occupied,
            Vector2Int core,
            bool lit,
            bool counted)
        {
            return new ItemSystemPlacementSnapshot(
                placementId, itemId, itemId, anchor, rotation, occupied, core,
                false, lit, lit, string.Empty, string.Empty, 0,
                Array.Empty<Vector2Int>(), false, false, counted, 1, 1,
                Array.Empty<string>(), Array.Empty<string>());
        }

        private static Vector2Int V(int x, int y)
        {
            return new Vector2Int(x, y);
        }

        private static LayoutCellCoordinate Cell(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static bool CellsEqual(
            IEnumerable<LayoutCellCoordinate> left,
            IEnumerable<LayoutCellCoordinate> right)
        {
            return left.Select(value => value.ToString()).SequenceEqual(
                right.Select(value => value.ToString()), StringComparer.Ordinal);
        }

        private static void AddScenario(
            ICollection<Scenario> scenarios,
            string id,
            LayoutResilienceItemFactProjectionResult result,
            LayoutResilienceItemFactProjectionStatus status,
            LayoutResilienceInputCompleteness completeness,
            int rows,
            string code)
        {
            scenarios.Add(new Scenario(id, result, status, completeness, rows, code));
        }

        private static void VerifyProtection(
            ICollection<Check> checks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            AddProtection(checks, "item", before.Item, after.Item, 107,
                ExpectedItemHash);
            Add(checks, "protected.item-snapshot.accepted", ExpectedItemSnapshotHash,
                before.ItemSnapshot, before.ItemSnapshot == ExpectedItemSnapshotHash);
            Add(checks, "protected.item-snapshot.before-after", before.ItemSnapshot,
                after.ItemSnapshot, before.ItemSnapshot == after.ItemSnapshot);
            AddProtection(checks, "if01", before.IF01, after.IF01, 11,
                ExpectedIF01Hash);
            AddProtection(checks, "n01c", before.N01C, after.N01C, 16,
                ExpectedN01CHash);
            AddProtection(checks, "enemy", before.Enemy, after.Enemy, 89,
                ExpectedEnemyHash);
            AddProtection(checks, "c01", before.C01, after.C01, 9,
                ExpectedC01Hash);
            AddProtection(checks, "c02", before.C02, after.C02, 9,
                ExpectedC02Hash);
            AddProtection(checks, "c02a", before.C02A, after.C02A, 7,
                ExpectedC02AHash);
            AddProtection(checks, "n01", before.N01, after.N01, 8,
                ExpectedN01Hash);
            AddProtection(checks, "c02a-d1", before.C02AD1, after.C02AD1, 7,
                ExpectedC02AD1Hash);
            AddProtection(checks, "n01a", before.N01A, after.N01A, 8,
                ExpectedN01AHash);
            AddProtection(checks, "n01b", before.N01B, after.N01B, 14,
                ExpectedN01BHash);
            AddProtection(checks, "scenes", before.Scenes, after.Scenes, 14,
                ExpectedScenesHash);
            AddProtection(checks, "prefabs", before.Prefabs, after.Prefabs, 16,
                ExpectedPrefabsHash);
            Add(checks, "protected.build-settings.accepted",
                ExpectedBuildSettingsHash, before.BuildSettings,
                before.BuildSettings == ExpectedBuildSettingsHash);
            Add(checks, "protected.build-settings.before-after", before.BuildSettings,
                after.BuildSettings, before.BuildSettings == after.BuildSettings);
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string id,
            AggregateHash before,
            AggregateHash after,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id + ".count",
                expectedCount.ToString(CultureInfo.InvariantCulture),
                before.FileCount.ToString(CultureInfo.InvariantCulture),
                before.FileCount == expectedCount);
            Add(checks, "protected." + id + ".accepted", expectedHash, before.Hash,
                before.Hash == expectedHash);
            Add(checks, "protected." + id + ".before-after", before.Hash, after.Hash,
                before.Equals(after));
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(
                AggregateItem(root),
                FileHash(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs",
                    false),
                AggregateFiles(root, IF01Files, true),
                AggregateFiles(root, N01CFiles, true),
                AggregateEnemyExisting(root),
                AggregateFiles(root, C01Files, false),
                AggregateFiles(root, C02Files, false),
                AggregateFiles(root, C02AFiles, false),
                AggregateFiles(root, N01Files, true),
                AggregateFiles(root, C02AD1Files, true),
                AggregateFiles(root, N01AFiles, true),
                AggregateFiles(root, N01BFiles, true),
                AggregateDirectory(root, "Assets/_Game/Scenes", true),
                AggregateDirectory(root, "Assets/_Game/Prefabs", true),
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false));
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                SearchOption.AllDirectories).Where(value =>
                    !value.StartsWith(excluded, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, excludedMeta,
                        StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)), false);
        }

        private static AggregateHash AggregateEnemyExisting(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                SearchOption.AllDirectories).Where(value =>
                    !value.StartsWith(excluded, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, excludedMeta,
                        StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)), false);
        }

        private static AggregateHash AggregateDirectory(
            string root,
            string directory,
            bool trailing)
        {
            return AggregateFiles(root, Directory.GetFiles(Absolute(root, directory), "*",
                SearchOption.AllDirectories).Select(value => Relative(root, value)),
                trailing);
        }

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailing)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailing ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailing));
                if (trailing || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static string FileHash(string root, string path, bool uppercase)
        {
            return Sha256(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
            }
        }

        private static string RunGit(string root, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        "git " + arguments + " failed: " + error);
                }
                return output;
            }
        }

        private static void AssertEnum<T>(
            ICollection<Check> checks,
            string id,
            string[] names,
            int[] values)
        {
            string[] actualNames = Enum.GetNames(typeof(T));
            int[] actualValues = Enum.GetValues(typeof(T)).Cast<object>()
                .Select(value => Convert.ToInt32(value, CultureInfo.InvariantCulture))
                .ToArray();
            Add(checks, "enum." + id + ".names", string.Join("|", names),
                string.Join("|", actualNames), names.SequenceEqual(actualNames));
            Add(checks, "enum." + id + ".values", string.Join("|", values),
                string.Join("|", actualValues), values.SequenceEqual(actualValues));
            Add(checks, "enum." + id + ".zero-absent", "False",
                Enum.IsDefined(typeof(T), 0).ToString(),
                !Enum.IsDefined(typeof(T), 0));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] expected)
        {
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] sorted = expected.OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            Add(checks, "properties." + id + ".exact", string.Join("|", sorted),
                string.Join("|", actual), sorted.SequenceEqual(actual));
            Add(checks, "properties." + id + ".readonly", "0",
                properties.Count(value => value.SetMethod != null).ToString(
                    CultureInfo.InvariantCulture),
                properties.All(value => value.SetMethod == null));
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Substring(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static void AddThrowsNotSupported(
            ICollection<Check> checks,
            string id,
            Action action)
        {
            bool threw = false;
            try
            {
                action();
            }
            catch (NotSupportedException)
            {
                threw = true;
            }
            Add(checks, id, "True", threw.ToString(), threw);
        }

        private static string[] CsvRows(string root, string path)
        {
            return File.ReadAllLines(Absolute(root, path), Encoding.UTF8).Skip(1)
                .Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        }

        private static string[] Lines(string value)
        {
            return (value ?? string.Empty).Replace("\r", string.Empty)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static int CountOrdinal(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = (source ?? string.Empty).IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static string ReadGuid(string path)
        {
            string line = File.ReadLines(path).FirstOrDefault(value =>
                value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring(6).Trim();
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static string FindProjectRoot()
        {
            foreach (string seed in new[]
            {
                Directory.GetCurrentDirectory(), AppContext.BaseDirectory
            })
            {
                DirectoryInfo current = new DirectoryInfo(seed);
                while (current != null)
                {
                    if (Directory.Exists(Path.Combine(current.FullName, "Assets")) &&
                        Directory.Exists(Path.Combine(current.FullName,
                            "ProjectSettings")) &&
                        Directory.Exists(Path.Combine(current.FullName, "Packages")))
                    {
                        return current.FullName;
                    }
                    current = current.Parent;
                }
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.Combine(root, (path ?? string.Empty).Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string path)
        {
            Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(path)).ToString())
                .Replace('\\', '/');
        }

        private static void AddContains(
            ICollection<Check> checks,
            string id,
            string source,
            string expected)
        {
            bool passed = (source ?? string.Empty).IndexOf(expected,
                StringComparison.Ordinal) >= 0;
            Add(checks, id, expected, passed ? "present" : "absent", passed);
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

        private readonly struct Check
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

        private sealed class Scenario
        {
            public Scenario(
                string id,
                LayoutResilienceItemFactProjectionResult result,
                LayoutResilienceItemFactProjectionStatus expectedStatus,
                LayoutResilienceInputCompleteness expectedCompleteness,
                int expectedRows,
                string expectedCode)
            {
                Id = id;
                Result = result;
                ExpectedStatus = expectedStatus;
                ExpectedCompleteness = expectedCompleteness;
                ExpectedRows = expectedRows;
                ExpectedCode = expectedCode;
            }

            public string Id { get; }
            public LayoutResilienceItemFactProjectionResult Result { get; }
            public LayoutResilienceItemFactProjectionStatus ExpectedStatus { get; }
            public LayoutResilienceInputCompleteness ExpectedCompleteness { get; }
            public int ExpectedRows { get; }
            public string ExpectedCode { get; }
        }

        private readonly struct AggregateHash : IEquatable<AggregateHash>
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }

            public bool Equals(AggregateHash other)
            {
                return FileCount == other.FileCount && Hash == other.Hash;
            }
        }

        private sealed class ProtectionSnapshot
        {
            public ProtectionSnapshot(
                AggregateHash item,
                string itemSnapshot,
                AggregateHash if01,
                AggregateHash n01c,
                AggregateHash enemy,
                AggregateHash c01,
                AggregateHash c02,
                AggregateHash c02a,
                AggregateHash n01,
                AggregateHash c02ad1,
                AggregateHash n01a,
                AggregateHash n01b,
                AggregateHash scenes,
                AggregateHash prefabs,
                string buildSettings)
            {
                Item = item;
                ItemSnapshot = itemSnapshot;
                IF01 = if01;
                N01C = n01c;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
                C02A = c02a;
                N01 = n01;
                C02AD1 = c02ad1;
                N01A = n01a;
                N01B = n01b;
                Scenes = scenes;
                Prefabs = prefabs;
                BuildSettings = buildSettings;
            }

            public AggregateHash Item { get; }
            public string ItemSnapshot { get; }
            public AggregateHash IF01 { get; }
            public AggregateHash N01C { get; }
            public AggregateHash Enemy { get; }
            public AggregateHash C01 { get; }
            public AggregateHash C02 { get; }
            public AggregateHash C02A { get; }
            public AggregateHash N01 { get; }
            public AggregateHash C02AD1 { get; }
            public AggregateHash N01A { get; }
            public AggregateHash N01B { get; }
            public AggregateHash Scenes { get; }
            public AggregateHash Prefabs { get; }
            public string BuildSettings { get; }
        }
    }
}
