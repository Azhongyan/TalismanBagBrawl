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
using TalismanBag.EnemySystem.RequirementChannel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    /// <summary>
    /// Read-only verifier for the isolated requirement-channel applicability schema.
    /// Offline and Unity entry points intentionally execute this same assertion set.
    /// </summary>
    public static class EnemyRequirementChannelApplicabilitySchemaContractVerifier
    {
        private const string PackageName =
            "V0.4-EnemyRequirementChannelApplicabilitySchemaContract01";
        private const string GuardReceipt =
            "GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01";
        private const string EnemyGuardReceipt =
            "ENEMY_GUARD_CONFIRM_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01";
        private const string AlgorithmGuardReceipt =
            "CAPABILITY_ALGORITHM_GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01";
        private const string ExpectedCanonicalSignature =
            "sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326";
        private const string ExpectedHead =
            "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba";

        private const string ExpectedItemHash =
            "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1";
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
        private const string ExpectedE06Hash =
            "c91d77499c07e6a6b828d8fb2e21c08096d656c6395e94159a0a547fdf3e5569";
        private const string ExpectedE07Hash =
            "b4be0330a411537e3e8e16968c2d45a5cdbbc688d9b588044ffe7e15daa505df";
        private const string ExpectedE08Hash =
            "45a40d4993e1bf6bb78f1ad14e6ac9dfe84c0c5a69ebffc5502fabb3815821d4";
        private const string ExpectedE10Hash =
            "3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be";
        private const string ExpectedScenesHash =
            "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsHash =
            "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash =
            "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";
        private const string ExpectedN01ASignature =
            "sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79";
        private const string ExpectedIF01Signature =
            "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedIF02Signature =
            "sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673";
        private const string ExpectedIF03Signature =
            "sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a";
        private const string ExpectedAffixSignature =
            "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRoll150Signature =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjection150Signature =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";
        private const string ExpectedPreexistingCatalogHash =
            "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45";
        private const string ExpectedPreexistingCandidateHash =
            "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb";

        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReportPath = ReportDirectory +
            "/EnemyRequirementChannelApplicabilitySchemaContractReport.md";
        private const string SpecPath = ReportDirectory +
            "/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv";
        private const string FieldPath = ReportDirectory +
            "/EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv";
        private const string FixturePath = ReportDirectory +
            "/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv";
        private const string LeakPath = ReportDirectory +
            "/EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityPrimitives.cs",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityPrimitives.cs.meta",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilitySnapshots.cs",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilitySnapshots.cs.meta",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityValidation.cs",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs.meta",
            MainReportPath,
            SpecPath,
            FieldPath,
            FixturePath,
            LeakPath
        };

        private static readonly string[] RuntimePaths =
        {
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityPrimitives.cs",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilitySnapshots.cs",
            RuntimeDirectory + "/EnemyRequirementChannelApplicabilityValidation.cs"
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

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/[QA Only] Verify Requirement Channel Applicability Schema")]
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

        private static void VerifyOrThrow(string mode)
        {
            string root = FindProjectRoot();
            ProtectionSnapshot before = CaptureProtection(root);
            List<Check> checks = new List<Check>();

            VerifySchema(checks);
            VerifyTypeShapes(checks);
            VerifyValidation(checks);
            IReadOnlyList<EnemyRequirementChannelApplicabilitySnapshot> fixtures =
                VerifyFixtures(checks);
            VerifyCanonical(checks, fixtures);
            VerifyImmutability(checks);
            VerifyOutputsAndLeaks(root, checks, fixtures);
            VerifyBaselineBehavior(root, checks);
            VerifyRepositoryBoundary(root, checks);

            ProtectionSnapshot after = CaptureProtection(root);
            VerifyProtection(checks, before, after);

            Check[] failed = checks.Where(value => !value.Passed).ToArray();
            string canonical = fixtures[0].CanonicalSignature;
            Console.WriteLine(
                "ENEMY_REQUIREMENT_CHANNEL_APPLICABILITY_SCHEMA_CONTRACT01 " +
                (failed.Length == 0 ? "PASS" : "FAIL") +
                " checks=" + checks.Count.ToString(CultureInfo.InvariantCulture) +
                " mode=" + mode);
            Console.WriteLine("CANONICAL_SIGNATURE=" + canonical);
            Console.WriteLine("FIXTURE_SNAPSHOTS=3 FIXTURE_REQUIREMENTS=9 FIXTURE_ROWS=27");
            Console.WriteLine("VALID_COMBINATIONS=15 INVALID_COMBINATIONS=21");
            Console.WriteLine("LEAK_COUNT=" +
                checks.Count(value => value.Id.StartsWith("leak.", StringComparison.Ordinal) &&
                    !value.Passed).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("PROTECTED_E06=" + before.E06.Hash);
            Console.WriteLine("PROTECTED_E07=" + before.E07.Hash);
            Console.WriteLine("PROTECTED_E08=" + before.E08.Hash);
            Console.WriteLine("PROTECTED_E10=" + before.E10.Hash);

            foreach (Check failure in failed)
            {
                Console.Error.WriteLine(
                    failure.Id + " expected=" + failure.Expected + " actual=" +
                    failure.Actual);
            }

            if (failed.Length > 0)
            {
                throw new InvalidOperationException(
                    failed.Length.ToString(CultureInfo.InvariantCulture) +
                    " verifier assertion(s) failed.");
            }
        }

        private static void VerifySchema(ICollection<Check> checks)
        {
            Add(checks, "schema.id", "EnemyRequirementChannelApplicability.v1",
                EnemyRequirementChannelApplicabilitySchema.SchemaId,
                EnemyRequirementChannelApplicabilitySchema.SchemaId ==
                    "EnemyRequirementChannelApplicability.v1");
            Add(checks, "schema.version", "1",
                EnemyRequirementChannelApplicabilitySchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                EnemyRequirementChannelApplicabilitySchema.SchemaVersion == 1);

            AssertEnum<EnemyRequirementChannel>(checks, "channel",
                new[] { "ContinuousBP", "StructuralPredicate", "RuntimeEventSignal" },
                new[] { 1, 2, 3 });
            AssertEnum<EnemyRequirementApplicabilityState>(checks, "state",
                new[] { "Applicable", "Unknown", "NotApplicable", "NotInChannel" },
                new[] { 1, 2, 3, 4 });
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
                Enum.IsDefined(typeof(T), 0).ToString(), !Enum.IsDefined(typeof(T), 0));
        }

        private static void VerifyTypeShapes(ICollection<Check> checks)
        {
            AssertProperties(checks, "shape.row",
                typeof(EnemyRequirementChannelApplicabilityRowSnapshot),
                "ApplicabilityState", "DeclaredChannel", "RequirementId");
            AssertProperties(checks, "shape.input",
                typeof(EnemyRequirementChannelApplicabilitySnapshotInput),
                "EvaluationChannel", "Rows", "SchemaId", "SchemaVersion", "SnapshotId");
            AssertProperties(checks, "shape.snapshot",
                typeof(EnemyRequirementChannelApplicabilitySnapshot),
                "CanonicalSignature", "EvaluationChannel", "Rows", "SchemaId",
                "SchemaVersion", "SnapshotId");

            Type[] publicTypes =
            {
                typeof(EnemyRequirementChannelApplicabilitySchema),
                typeof(EnemyRequirementChannel),
                typeof(EnemyRequirementApplicabilityState),
                typeof(EnemyRequirementChannelApplicabilityRowSnapshot),
                typeof(EnemyRequirementChannelApplicabilitySnapshotInput),
                typeof(EnemyRequirementChannelApplicabilitySnapshot),
                typeof(EnemyRequirementChannelApplicabilityValidationIssue),
                typeof(EnemyRequirementChannelApplicabilityValidationException),
                typeof(IEnemyRequirementChannelApplicabilitySnapshotProvider),
                typeof(IEnemyRequirementChannelApplicabilitySnapshotValidator),
                typeof(DefaultEnemyRequirementChannelApplicabilitySnapshotProvider),
                typeof(DefaultEnemyRequirementChannelApplicabilitySnapshotValidator)
            };
            Add(checks, "shape.public-types", "12",
                publicTypes.Distinct().Count().ToString(CultureInfo.InvariantCulture),
                publicTypes.Distinct().Count() == 12);
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] expected)
        {
            string[] actual = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] sortedExpected = expected.OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Add(checks, id + ".exact", string.Join("|", sortedExpected),
                string.Join("|", actual), sortedExpected.SequenceEqual(actual));
            Add(checks, id + ".read-only", "0",
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Count(value => value.SetMethod != null).ToString(CultureInfo.InvariantCulture),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .All(value => value.SetMethod == null));
        }

        private static void VerifyValidation(ICollection<Check> checks)
        {
            IEnemyRequirementChannelApplicabilitySnapshotValidator validator =
                DefaultEnemyRequirementChannelApplicabilitySnapshotValidator.Instance;
            IEnemyRequirementChannelApplicabilitySnapshotProvider provider =
                DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance;

            AssertInvalid(checks, "validation.null-input", validator, null, "INPUT_NULL");
            AssertInvalid(checks, "validation.schema-id", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0], "wrong", 1),
                "SCHEMA_ID_MISMATCH");
            AssertInvalid(checks, "validation.schema-version", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0],
                    EnemyRequirementChannelApplicabilitySchema.SchemaId, 2),
                "SCHEMA_VERSION_MISMATCH");
            AssertInvalid(checks, "validation.snapshot-empty", validator,
                Input(string.Empty, EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]),
                "SNAPSHOT_ID_EMPTY");
            AssertInvalid(checks, "validation.snapshot-whitespace", validator,
                Input(" dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]),
                "SNAPSHOT_ID_OUTER_WHITESPACE");
            AssertInvalid(checks, "validation.eval-zero", validator,
                Input("dev_snapshot", (EnemyRequirementChannel)0,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]),
                "EVALUATION_CHANNEL_UNDEFINED");
            AssertInvalid(checks, "validation.eval-undefined", validator,
                Input("dev_snapshot", (EnemyRequirementChannel)99,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]),
                "EVALUATION_CHANNEL_UNDEFINED");
            AssertInvalid(checks, "validation.null-row", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[] { null }),
                "ROW_NULL");
            AssertInvalid(checks, "validation.requirement-empty", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row(string.Empty, EnemyRequirementChannel.ContinuousBP,
                        EnemyRequirementApplicabilityState.Applicable)),
                "REQUIREMENT_ID_EMPTY");
            AssertInvalid(checks, "validation.requirement-whitespace", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement ", EnemyRequirementChannel.ContinuousBP,
                        EnemyRequirementApplicabilityState.Applicable)),
                "REQUIREMENT_ID_OUTER_WHITESPACE");
            AssertInvalid(checks, "validation.duplicate-ordinal", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", EnemyRequirementChannel.ContinuousBP,
                            EnemyRequirementApplicabilityState.Applicable),
                    Row("dev_requirement", EnemyRequirementChannel.ContinuousBP,
                            EnemyRequirementApplicabilityState.Unknown)),
                "REQUIREMENT_ID_DUPLICATE");
            AssertValid(checks, "validation.case-distinct", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", EnemyRequirementChannel.ContinuousBP,
                            EnemyRequirementApplicabilityState.Applicable),
                    Row("DEV_REQUIREMENT", EnemyRequirementChannel.ContinuousBP,
                            EnemyRequirementApplicabilityState.Unknown)));
            AssertInvalid(checks, "validation.declared-zero", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", (EnemyRequirementChannel)0,
                        EnemyRequirementApplicabilityState.NotInChannel)),
                "DECLARED_CHANNEL_UNDEFINED");
            AssertInvalid(checks, "validation.declared-undefined", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", (EnemyRequirementChannel)99,
                        EnemyRequirementApplicabilityState.NotInChannel)),
                "DECLARED_CHANNEL_UNDEFINED");
            AssertInvalid(checks, "validation.state-zero", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", EnemyRequirementChannel.ContinuousBP,
                        (EnemyRequirementApplicabilityState)0)),
                "APPLICABILITY_STATE_UNDEFINED");
            AssertInvalid(checks, "validation.state-undefined", validator,
                Input("dev_snapshot", EnemyRequirementChannel.ContinuousBP,
                    Row("dev_requirement", EnemyRequirementChannel.ContinuousBP,
                        (EnemyRequirementApplicabilityState)99)),
                "APPLICABILITY_STATE_UNDEFINED");

            int validCombinations = 0;
            int invalidCombinations = 0;
            foreach (EnemyRequirementChannel evaluation in Channels())
            {
                foreach (EnemyRequirementChannel declared in Channels())
                {
                    foreach (EnemyRequirementApplicabilityState state in States())
                    {
                        bool expectedValid = evaluation == declared
                            ? state != EnemyRequirementApplicabilityState.NotInChannel
                            : state == EnemyRequirementApplicabilityState.NotInChannel;
                        EnemyRequirementChannelApplicabilitySnapshotInput input = Input(
                            "dev_combination_snapshot", evaluation,
                            Row("dev_combination_requirement", declared, state));
                        bool actualValid = validator.Validate(input).Count == 0;
                        Add(checks,
                            "combination." + evaluation + "." + declared + "." + state,
                            expectedValid.ToString(), actualValid.ToString(),
                            expectedValid == actualValid);
                        if (expectedValid)
                        {
                            validCombinations++;
                        }
                        else
                        {
                            invalidCombinations++;
                        }
                    }
                }
            }
            Add(checks, "combination.valid-count", "15",
                validCombinations.ToString(CultureInfo.InvariantCulture),
                validCombinations == 15);
            Add(checks, "combination.invalid-count", "21",
                invalidCombinations.ToString(CultureInfo.InvariantCulture),
                invalidCombinations == 21);

            EnemyRequirementChannelApplicabilitySnapshot empty = provider.CreateSnapshot(
                Input("dev_empty_snapshot", EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]));
            Add(checks, "validation.empty-rows-valid", "0",
                empty.Rows.Count.ToString(CultureInfo.InvariantCulture), empty.Rows.Count == 0);

            bool threw = false;
            try
            {
                provider.CreateSnapshot(Input(string.Empty,
                    EnemyRequirementChannel.ContinuousBP,
                    new EnemyRequirementChannelApplicabilityRowSnapshot[0]));
            }
            catch (EnemyRequirementChannelApplicabilityValidationException exception)
            {
                threw = exception.Issues.Any(value => value.Code == "SNAPSHOT_ID_EMPTY");
                AddThrowsNotSupported(checks, "validation.exception-issues-readonly",
                    delegate
                    {
                        ((IList<EnemyRequirementChannelApplicabilityValidationIssue>)
                            exception.Issues).Add(new EnemyRequirementChannelApplicabilityValidationIssue(
                                "x", "x", "x"));
                    });
            }
            Add(checks, "validation.provider-rejects", "True", threw.ToString(), threw);
        }

        private static IReadOnlyList<EnemyRequirementChannelApplicabilitySnapshot>
            VerifyFixtures(ICollection<Check> checks)
        {
            List<EnemyRequirementChannelApplicabilitySnapshot> snapshots =
                Channels().Select(CreateFixture).ToList();
            Add(checks, "fixture.snapshot-count", "3",
                snapshots.Count.ToString(CultureInfo.InvariantCulture), snapshots.Count == 3);
            Add(checks, "fixture.rows-per-snapshot", "9|9|9",
                string.Join("|", snapshots.Select(value => value.Rows.Count)),
                snapshots.All(value => value.Rows.Count == 9));
            string[] ids = snapshots.SelectMany(value => value.Rows)
                .Select(value => value.RequirementId).Distinct(StringComparer.Ordinal).ToArray();
            Add(checks, "fixture.requirement-count", "9",
                ids.Length.ToString(CultureInfo.InvariantCulture), ids.Length == 9);
            Add(checks, "fixture.total-rows", "27",
                snapshots.Sum(value => value.Rows.Count).ToString(CultureInfo.InvariantCulture),
                snapshots.Sum(value => value.Rows.Count) == 27);

            Dictionary<EnemyRequirementApplicabilityState, int> counts = snapshots
                .SelectMany(value => value.Rows).GroupBy(value => value.ApplicabilityState)
                .ToDictionary(value => value.Key, value => value.Count());
            AssertCount(checks, counts, EnemyRequirementApplicabilityState.Applicable, 3);
            AssertCount(checks, counts, EnemyRequirementApplicabilityState.Unknown, 3);
            AssertCount(checks, counts, EnemyRequirementApplicabilityState.NotApplicable, 3);
            AssertCount(checks, counts, EnemyRequirementApplicabilityState.NotInChannel, 18);

            foreach (EnemyRequirementChannelApplicabilitySnapshot snapshot in snapshots)
            {
                Add(checks, "fixture.sorted." + snapshot.EvaluationChannel, "True",
                    snapshot.Rows.Select(value => value.RequirementId)
                        .SequenceEqual(snapshot.Rows.Select(value => value.RequirementId)
                            .OrderBy(value => value, StringComparer.Ordinal)).ToString(),
                    snapshot.Rows.Select(value => value.RequirementId)
                        .SequenceEqual(snapshot.Rows.Select(value => value.RequirementId)
                            .OrderBy(value => value, StringComparer.Ordinal)));
                Add(checks, "fixture.same-channel-states." + snapshot.EvaluationChannel,
                    "Applicable|Unknown|NotApplicable",
                    string.Join("|", snapshot.Rows
                        .Where(value => value.DeclaredChannel == snapshot.EvaluationChannel)
                        .Select(value => value.ApplicabilityState)
                        .OrderBy(value => (int)value)),
                    snapshot.Rows.Count(value =>
                        value.DeclaredChannel == snapshot.EvaluationChannel &&
                        value.ApplicabilityState !=
                            EnemyRequirementApplicabilityState.NotInChannel) == 3);
                Add(checks, "fixture.cross-channel-not-in." + snapshot.EvaluationChannel,
                    "6", snapshot.Rows.Count(value => value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotInChannel)
                        .ToString(CultureInfo.InvariantCulture),
                    snapshot.Rows.Count(value => value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotInChannel) == 6);
            }
            return snapshots.AsReadOnly();
        }

        private static void VerifyCanonical(
            ICollection<Check> checks,
            IReadOnlyList<EnemyRequirementChannelApplicabilitySnapshot> fixtures)
        {
            EnemyRequirementChannelApplicabilitySnapshot baseline = fixtures[0];
            Add(checks, "canonical.expected", ExpectedCanonicalSignature,
                baseline.CanonicalSignature,
                baseline.CanonicalSignature == ExpectedCanonicalSignature);
            Add(checks, "canonical.format", "sha256:+64 lowercase hex",
                baseline.CanonicalSignature,
                IsCanonicalSignature(baseline.CanonicalSignature));
            Add(checks, "canonical.repeat", baseline.CanonicalSignature,
                CreateFixture(EnemyRequirementChannel.ContinuousBP).CanonicalSignature,
                baseline.CanonicalSignature ==
                    CreateFixture(EnemyRequirementChannel.ContinuousBP).CanonicalSignature);

            EnemyRequirementChannelApplicabilitySnapshot reversed =
                DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                    .CreateSnapshot(Input(
                        FixtureSnapshotId(EnemyRequirementChannel.ContinuousBP),
                        EnemyRequirementChannel.ContinuousBP,
                        FixtureRows(EnemyRequirementChannel.ContinuousBP).Reverse().ToArray()));
            Add(checks, "canonical.input-order-insensitive", baseline.CanonicalSignature,
                reversed.CanonicalSignature,
                baseline.CanonicalSignature == reversed.CanonicalSignature);

            AssertSignatureDifferent(checks, "canonical.snapshot-id-sensitive", baseline,
                Input(FixtureSnapshotId(EnemyRequirementChannel.ContinuousBP) + "_changed",
                    EnemyRequirementChannel.ContinuousBP,
                    FixtureRows(EnemyRequirementChannel.ContinuousBP).ToArray()));
            AssertSignatureDifferent(checks, "canonical.evaluation-channel-sensitive",
                baseline, CreateFixture(EnemyRequirementChannel.StructuralPredicate));

            List<EnemyRequirementChannelApplicabilityRowSnapshot> changedId =
                FixtureRows(EnemyRequirementChannel.ContinuousBP).ToList();
            EnemyRequirementChannelApplicabilityRowSnapshot first = changedId[0];
            changedId[0] = Row(first.RequirementId + "_changed", first.DeclaredChannel,
                first.ApplicabilityState);
            AssertSignatureDifferent(checks, "canonical.requirement-id-sensitive", baseline,
                Input(FixtureSnapshotId(EnemyRequirementChannel.ContinuousBP),
                    EnemyRequirementChannel.ContinuousBP, changedId.ToArray()));

            List<EnemyRequirementChannelApplicabilityRowSnapshot> changedDeclared =
                FixtureRows(EnemyRequirementChannel.ContinuousBP).ToList();
            int crossIndex = changedDeclared.FindIndex(value => value.DeclaredChannel ==
                EnemyRequirementChannel.StructuralPredicate);
            first = changedDeclared[crossIndex];
            changedDeclared[crossIndex] = Row(first.RequirementId,
                EnemyRequirementChannel.RuntimeEventSignal,
                EnemyRequirementApplicabilityState.NotInChannel);
            AssertSignatureDifferent(checks, "canonical.declared-channel-sensitive", baseline,
                Input(FixtureSnapshotId(EnemyRequirementChannel.ContinuousBP),
                    EnemyRequirementChannel.ContinuousBP, changedDeclared.ToArray()));

            List<EnemyRequirementChannelApplicabilityRowSnapshot> changedState =
                FixtureRows(EnemyRequirementChannel.ContinuousBP).ToList();
            int sameIndex = changedState.FindIndex(value => value.ApplicabilityState ==
                EnemyRequirementApplicabilityState.Applicable);
            first = changedState[sameIndex];
            changedState[sameIndex] = Row(first.RequirementId, first.DeclaredChannel,
                EnemyRequirementApplicabilityState.Unknown);
            AssertSignatureDifferent(checks, "canonical.state-sensitive", baseline,
                Input(FixtureSnapshotId(EnemyRequirementChannel.ContinuousBP),
                    EnemyRequirementChannel.ContinuousBP, changedState.ToArray()));
        }

        private static void VerifyImmutability(ICollection<Check> checks)
        {
            List<EnemyRequirementChannelApplicabilityRowSnapshot> source =
                FixtureRows(EnemyRequirementChannel.ContinuousBP).ToList();
            EnemyRequirementChannelApplicabilitySnapshotInput input = Input(
                "dev_immutability_snapshot", EnemyRequirementChannel.ContinuousBP,
                source.ToArray());
            source.Clear();
            Add(checks, "immutability.input-defensive-copy", "9",
                input.Rows.Count.ToString(CultureInfo.InvariantCulture), input.Rows.Count == 9);
            EnemyRequirementChannelApplicabilitySnapshot snapshot =
                DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                    .CreateSnapshot(input);
            string signature = snapshot.CanonicalSignature;
            source.Add(Row("dev_late_mutation", EnemyRequirementChannel.ContinuousBP,
                EnemyRequirementApplicabilityState.Applicable));
            Add(checks, "immutability.snapshot-source-mutation", signature,
                snapshot.CanonicalSignature, signature == snapshot.CanonicalSignature &&
                    snapshot.Rows.Count == 9);
            AddThrowsNotSupported(checks, "immutability.input-rows-readonly", delegate
            {
                ((IList<EnemyRequirementChannelApplicabilityRowSnapshot>)input.Rows)
                    .Add(Row("dev_illegal", EnemyRequirementChannel.ContinuousBP,
                        EnemyRequirementApplicabilityState.Applicable));
            });
            AddThrowsNotSupported(checks, "immutability.snapshot-rows-readonly", delegate
            {
                ((IList<EnemyRequirementChannelApplicabilityRowSnapshot>)snapshot.Rows)
                    .Clear();
            });
        }

        private static void VerifyOutputsAndLeaks(
            string root,
            ICollection<Check> checks,
            IReadOnlyList<EnemyRequirementChannelApplicabilitySnapshot> fixtures)
        {
            foreach (string path in OutputPaths)
            {
                Add(checks, "output.exists." + Path.GetFileName(path), "True",
                    File.Exists(Absolute(root, path)).ToString(), File.Exists(Absolute(root, path)));
            }
            Add(checks, "output.count", "14",
                OutputPaths.Length.ToString(CultureInfo.InvariantCulture),
                OutputPaths.Length == 14 && OutputPaths.All(path => File.Exists(Absolute(root, path))));

            string main = Read(root, MainReportPath);
            AddContains(checks, "report.package", main, PackageName);
            AddContains(checks, "report.guard", main, GuardReceipt);
            AddContains(checks, "report.enemy-guard", main, EnemyGuardReceipt);
            AddContains(checks, "report.algorithm-guard", main, AlgorithmGuardReceipt);
            AddContains(checks, "report.signature", main, ExpectedCanonicalSignature);
            string reportItemHash = MarkdownTableCodeValue(
                main, "| Item105 existing scope | 105 |");
            Add(checks, "report.item105.exact", ExpectedItemHash, reportItemHash,
                string.Equals(reportItemHash, ExpectedItemHash, StringComparison.Ordinal));
            Add(checks, "report.item105.sha256-format", "64 lowercase hex",
                reportItemHash.Length.ToString(CultureInfo.InvariantCulture) + " chars",
                IsLowercaseSha256Hash(reportItemHash));
            AddContains(checks, "report.no-migration", main, "Real requirement migration rows: `0`");
            AddContains(checks, "report.no-behavior", main, "E06/E07/E08/E10 behavior changes: `0`");

            string[] spec = CsvDataRows(root, SpecPath);
            string[] fields = CsvDataRows(root, FieldPath);
            string[] fixtureRows = CsvDataRows(root, FixturePath);
            Add(checks, "report.spec.nonempty", "True", (spec.Length > 20).ToString(),
                spec.Length > 20);
            Add(checks, "report.field.rows", "14",
                fields.Length.ToString(CultureInfo.InvariantCulture), fields.Length == 14);
            Add(checks, "report.fixture.rows", "27",
                fixtureRows.Length.ToString(CultureInfo.InvariantCulture),
                fixtureRows.Length == 27);
            Add(checks, "report.fixture.expected-valid", "27",
                fixtureRows.Count(value => value.EndsWith(",true,true",
                    StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture),
                fixtureRows.Count(value => value.EndsWith(",true,true",
                    StringComparison.Ordinal)) == 27);

            string leak = Read(root, LeakPath);
            AddContains(checks, "report.leak.total", leak, "Leak Count: `0`");
            foreach (string section in new[]
            {
                "Runtime dependency leak", "Real requirement migration", "Readiness mutation",
                "BuildCapabilityRead mutation", "Runtime Producer connection", "Item dependency",
                "Scene and Prefab mutation", "GUID collision", "Trailing whitespace"
            })
            {
                AddContains(checks, "report.leak.section." + section, leak, section);
            }

            string runtime = string.Join("\n", RuntimePaths.Select(path => Read(root, path)));
            string[] forbiddenRuntimeTokens =
            {
                "using UnityEngine", "TalismanBag.EnemySystem.PressureWindow",
                "TalismanBag.EnemySystem.CapabilityRead",
                "TalismanBag.EnemySystem.ReadinessEvaluation",
                "TalismanBag.EnemySystem.SeedData", "TalismanBag.Items",
                "TalismanBag.CrossSystem", "TalismanBag.Battle", "TalismanBag.Board",
                "MonoBehaviour", "ScriptableObject", "BuildCapabilityKey",
                "RequirementGroupId", "MinimumCapabilityBasisPoints", "ValueBasisPoints",
                "ReadinessBand", "PlayerSafeSignature"
            };
            foreach (string token in forbiddenRuntimeTokens)
            {
                Add(checks, "leak.runtime." + token, "0",
                    CountOrdinal(runtime, token).ToString(CultureInfo.InvariantCulture),
                    CountOrdinal(runtime, token) == 0);
            }

            string fixtureText = Read(root, FixturePath);
            foreach (string token in new[]
            {
                "capability.placement_shape", "capability.debuff_counter",
                "capability.interrupt_timing", "capability.spirit_lock",
                "dev_encounter_3_10", "dev_encounter_4_10", "E06", "E10"
            })
            {
                Add(checks, "leak.fixture." + token, "0",
                    CountOrdinal(fixtureText, token).ToString(CultureInfo.InvariantCulture),
                    CountOrdinal(fixtureText, token) == 0);
            }
            Add(checks, "leak.fixture-prefix", "27",
                fixtures.SelectMany(value => value.Rows)
                    .Count(value => value.RequirementId.StartsWith(
                        "dev_requirement_channel_fixture_", StringComparison.Ordinal))
                    .ToString(CultureInfo.InvariantCulture),
                fixtures.SelectMany(value => value.Rows).All(value =>
                    value.RequirementId.StartsWith(
                        "dev_requirement_channel_fixture_", StringComparison.Ordinal)));
        }

        private static void VerifyBaselineBehavior(string root, ICollection<Check> checks)
        {
            string[] c02Rows = CsvDataRows(root,
                "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv");
            Add(checks, "baseline.c02.rows", "32",
                c02Rows.Length.ToString(CultureInfo.InvariantCulture), c02Rows.Length == 32);
            Add(checks, "baseline.c02.blocked", "32",
                c02Rows.Count(value => value.Contains("BLOCKED_BY_UNKNOWN"))
                    .ToString(CultureInfo.InvariantCulture),
                c02Rows.Count(value => value.Contains("BLOCKED_BY_UNKNOWN")) == 32);

            string[] impactRows = CsvDataRows(root,
                "Docs/V0.4/Reports/BuildCapabilityNormalizationC02ImpactMatrix.csv");
            Add(checks, "baseline.n01a.impact-rows", "32",
                impactRows.Length.ToString(CultureInfo.InvariantCulture),
                impactRows.Length == 32);
            Add(checks, "baseline.n01a.actual-reduction-zero", "32",
                impactRows.Count(value => value.Contains("SURVEY_ACTUAL_REDUCTION_IS_ZERO"))
                    .ToString(CultureInfo.InvariantCulture),
                impactRows.Count(value => value.Contains(
                    "SURVEY_ACTUAL_REDUCTION_IS_ZERO")) == 32);
            AddContains(checks, "baseline.n01a.signature",
                Read(root, "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md"),
                ExpectedN01ASignature);
            string protectedSignatureEvidence = Read(root,
                "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md");
            AddContains(checks, "baseline.n01a.actual-unknown-reduction",
                protectedSignatureEvidence, "Actual Unknown-reference reduction: `0`");
            AddContains(checks, "baseline.n01a.remaining-unknown",
                protectedSignatureEvidence, "Remaining decisive Unknown references: `64`");
            AddContains(checks, "baseline.n01a.blocked-rows",
                protectedSignatureEvidence, "Blocked rows: `32/32`");
            AddContains(checks, "protected.signature.if01", protectedSignatureEvidence,
                ExpectedIF01Signature);
            AddContains(checks, "protected.signature.if02", protectedSignatureEvidence,
                ExpectedIF02Signature);
            AddContains(checks, "protected.signature.if03", protectedSignatureEvidence,
                ExpectedIF03Signature);
            AddContains(checks, "protected.signature.affix", protectedSignatureEvidence,
                ExpectedAffixSignature);
            AddContains(checks, "protected.signature.roll150", protectedSignatureEvidence,
                ExpectedRoll150Signature);
            AddContains(checks, "protected.signature.projection150", protectedSignatureEvidence,
                ExpectedProjection150Signature);
        }

        private static void VerifyRepositoryBoundary(string root, ICollection<Check> checks)
        {
            string head = RunGit(root, "rev-parse HEAD").Trim();
            Add(checks, "repo.head", ExpectedHead, head, head == ExpectedHead);
            string tree = RunGit(root, "ls-tree -r --name-only HEAD").Replace('\\', '/');
            foreach (string path in OutputPaths)
            {
                Add(checks, "repo.new." + Path.GetFileName(path), "False",
                    TreeContains(tree, path).ToString(), !TreeContains(tree, path));
            }

            string[] modified = RunGit(root, "diff --name-only").Split(
                new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Replace('\\', '/')).ToArray();
            string[] acceptedModified =
            {
                "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset",
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"
            };
            Add(checks, "repo.existing-modified-preexisting-only", string.Join("|", acceptedModified),
                string.Join("|", modified),
                modified.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                    acceptedModified.OrderBy(value => value, StringComparer.Ordinal)));
            Add(checks, "repo.preexisting.catalog-preserved", ExpectedPreexistingCatalogHash,
                FileHash(root, acceptedModified[0]),
                FileHash(root, acceptedModified[0]) == ExpectedPreexistingCatalogHash);
            Add(checks, "repo.preexisting.candidate-preserved", ExpectedPreexistingCandidateHash,
                FileHash(root, acceptedModified[1]),
                FileHash(root, acceptedModified[1]) == ExpectedPreexistingCandidateHash);

            int whitespaceLeaks = 0;
            foreach (string path in OutputPaths)
            {
                string[] lines = File.ReadAllLines(Absolute(root, path), Encoding.UTF8);
                whitespaceLeaks += lines.Count(value => value.Length > 0 &&
                    (value.EndsWith(" ", StringComparison.Ordinal) ||
                     value.EndsWith("\t", StringComparison.Ordinal)));
            }
            Add(checks, "leak.trailing-whitespace", "0",
                whitespaceLeaks.ToString(CultureInfo.InvariantCulture), whitespaceLeaks == 0);

            string[] newMetaPaths = OutputPaths.Where(value => value.EndsWith(".meta",
                StringComparison.Ordinal)).ToArray();
            string[] newGuids = newMetaPaths.Select(path => ReadGuid(Absolute(root, path)))
                .ToArray();
            Add(checks, "leak.guid.new-unique", "5",
                newGuids.Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                newGuids.Length == 5 &&
                    newGuids.Distinct(StringComparer.Ordinal).Count() == 5);
            string[] allGuids = Directory.GetFiles(root, "*.meta", SearchOption.AllDirectories)
                .Select(ReadGuid).Where(value => value.Length > 0).ToArray();
            Add(checks, "leak.guid.project-collision", "0",
                newGuids.Count(guid => allGuids.Count(value => value == guid) != 1)
                    .ToString(CultureInfo.InvariantCulture),
                newGuids.All(guid => allGuids.Count(value => value == guid) == 1));
        }

        private static void VerifyProtection(
            ICollection<Check> checks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            AddProtection(checks, "item", before.Item, after.Item, 105, ExpectedItemHash);
            AddProtection(checks, "enemy", before.Enemy, after.Enemy, 89, ExpectedEnemyHash);
            AddProtection(checks, "c01", before.C01, after.C01, 9, ExpectedC01Hash);
            AddProtection(checks, "c02", before.C02, after.C02, 9, ExpectedC02Hash);
            AddProtection(checks, "c02a", before.C02A, after.C02A, 7, ExpectedC02AHash);
            AddProtection(checks, "n01", before.N01, after.N01, 8, ExpectedN01Hash);
            AddProtection(checks, "c02a-d1", before.C02AD1, after.C02AD1, 7,
                ExpectedC02AD1Hash);
            AddProtection(checks, "n01a", before.N01A, after.N01A, 8, ExpectedN01AHash);
            AddProtection(checks, "e06", before.E06, after.E06, 16, ExpectedE06Hash);
            AddProtection(checks, "e07", before.E07, after.E07, 16, ExpectedE07Hash);
            AddProtection(checks, "e08", before.E08, after.E08, 16, ExpectedE08Hash);
            AddProtection(checks, "e10", before.E10, after.E10, 21, ExpectedE10Hash);
            AddProtection(checks, "scenes", before.Scenes, after.Scenes, 14,
                ExpectedScenesHash);
            AddProtection(checks, "prefabs", before.Prefabs, after.Prefabs, 16,
                ExpectedPrefabsHash);
            Add(checks, "protected.build-settings.accepted", ExpectedBuildSettingsHash,
                before.BuildSettings, before.BuildSettings == ExpectedBuildSettingsHash);
            Add(checks, "protected.build-settings.before-after", before.BuildSettings,
                after.BuildSettings, before.BuildSettings == after.BuildSettings);
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(
                AggregateItem(root), AggregateEnemyExisting(root),
                AggregateFiles(root, C01Files, false),
                AggregateFiles(root, C02Files, false),
                AggregateFiles(root, C02AFiles, false),
                AggregateFiles(root, N01Files, true),
                AggregateFiles(root, C02AD1Files, true),
                AggregateFiles(root, N01AFiles, true),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs",
                    "CounterWindowAndPressureSchema", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs",
                    "BuildCapabilityReadContract", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs",
                    "EnemyOfflineReadinessEvaluator", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs",
                    "DevEncounterSeed", 8),
                AggregateDirectory(root, "Assets/_Game/Scenes", true),
                AggregateDirectory(root, "Assets/_Game/Prefabs", true),
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset"));
        }

        private static AggregateHash AggregatePackage(
            string root,
            string runtimeDirectory,
            string runtimeMeta,
            string verifier,
            string reportPrefix,
            int expectedReportCount)
        {
            List<string> paths = Directory.GetFiles(Absolute(root, runtimeDirectory), "*",
                    SearchOption.AllDirectories).Select(value => Relative(root, value)).ToList();
            paths.Add(runtimeMeta);
            paths.Add(verifier);
            paths.Add(verifier + ".meta");
            string[] reports = Directory.GetFiles(Absolute(root, ReportDirectory),
                    reportPrefix + "*", SearchOption.TopDirectoryOnly)
                .Select(value => Relative(root, value)).ToArray();
            if (reports.Length != expectedReportCount)
            {
                paths.Add("__unexpected_report_count_" + reports.Length);
            }
            paths.AddRange(reports);
            return AggregateFiles(root, paths, true);
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items");
            string excludedDirectory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            string[] paths = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(value => !value.StartsWith(excludedDirectory,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, excludedMeta, StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)).ToArray();
            return AggregateFiles(root, paths, false);
        }

        private static AggregateHash AggregateEnemyExisting(string root)
        {
            string directory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            string excludedDirectory = Absolute(root, RuntimeDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root, RuntimeDirectory + ".meta");
            string[] paths = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .Where(value => !value.StartsWith(excludedDirectory,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(value, excludedMeta, StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)).ToArray();
            return AggregateFiles(root, paths, false);
        }

        private static AggregateHash AggregateDirectory(
            string root,
            string relativeDirectory,
            bool trailingNewline)
        {
            return AggregateFiles(root,
                Directory.GetFiles(Absolute(root, relativeDirectory), "*",
                    SearchOption.AllDirectories).Select(value => Relative(root, value)),
                trailingNewline);
        }

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailingNewline)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailingNewline ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                string path = ordered[index];
                payload.Append(path.Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, path, !trailingNewline));
                if (trailingNewline || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static EnemyRequirementChannelApplicabilitySnapshot CreateFixture(
            EnemyRequirementChannel evaluation)
        {
            return DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                .CreateSnapshot(Input(FixtureSnapshotId(evaluation), evaluation,
                    FixtureRows(evaluation).ToArray()));
        }

        private static IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot>
            FixtureRows(EnemyRequirementChannel evaluation)
        {
            yield return FixtureRow("bp_applicable", EnemyRequirementChannel.ContinuousBP,
                EnemyRequirementApplicabilityState.Applicable, evaluation);
            yield return FixtureRow("bp_unknown", EnemyRequirementChannel.ContinuousBP,
                EnemyRequirementApplicabilityState.Unknown, evaluation);
            yield return FixtureRow("bp_not_applicable", EnemyRequirementChannel.ContinuousBP,
                EnemyRequirementApplicabilityState.NotApplicable, evaluation);
            yield return FixtureRow("structural_applicable",
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementApplicabilityState.Applicable, evaluation);
            yield return FixtureRow("structural_unknown",
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementApplicabilityState.Unknown, evaluation);
            yield return FixtureRow("structural_not_applicable",
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementApplicabilityState.NotApplicable, evaluation);
            yield return FixtureRow("runtime_applicable",
                EnemyRequirementChannel.RuntimeEventSignal,
                EnemyRequirementApplicabilityState.Applicable, evaluation);
            yield return FixtureRow("runtime_unknown",
                EnemyRequirementChannel.RuntimeEventSignal,
                EnemyRequirementApplicabilityState.Unknown, evaluation);
            yield return FixtureRow("runtime_not_applicable",
                EnemyRequirementChannel.RuntimeEventSignal,
                EnemyRequirementApplicabilityState.NotApplicable, evaluation);
        }

        private static EnemyRequirementChannelApplicabilityRowSnapshot FixtureRow(
            string suffix,
            EnemyRequirementChannel declared,
            EnemyRequirementApplicabilityState ownState,
            EnemyRequirementChannel evaluation)
        {
            return Row("dev_requirement_channel_fixture_" + suffix, declared,
                declared == evaluation ? ownState :
                    EnemyRequirementApplicabilityState.NotInChannel);
        }

        private static string FixtureSnapshotId(EnemyRequirementChannel channel)
        {
            switch (channel)
            {
                case EnemyRequirementChannel.ContinuousBP:
                    return "dev_requirement_channel_fixture_snapshot_continuous_bp";
                case EnemyRequirementChannel.StructuralPredicate:
                    return "dev_requirement_channel_fixture_snapshot_structural_predicate";
                case EnemyRequirementChannel.RuntimeEventSignal:
                    return "dev_requirement_channel_fixture_snapshot_runtime_event_signal";
                default:
                    throw new ArgumentOutOfRangeException("channel");
            }
        }

        private static EnemyRequirementChannelApplicabilityRowSnapshot Row(
            string id,
            EnemyRequirementChannel channel,
            EnemyRequirementApplicabilityState state)
        {
            return new EnemyRequirementChannelApplicabilityRowSnapshot(id, channel, state);
        }

        private static EnemyRequirementChannelApplicabilitySnapshotInput Input(
            string snapshotId,
            EnemyRequirementChannel channel,
            params EnemyRequirementChannelApplicabilityRowSnapshot[] rows)
        {
            return Input(snapshotId, channel, rows,
                EnemyRequirementChannelApplicabilitySchema.SchemaId,
                EnemyRequirementChannelApplicabilitySchema.SchemaVersion);
        }

        private static EnemyRequirementChannelApplicabilitySnapshotInput Input(
            string snapshotId,
            EnemyRequirementChannel channel,
            IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot> rows,
            string schemaId,
            int schemaVersion)
        {
            return new EnemyRequirementChannelApplicabilitySnapshotInput(
                snapshotId, channel, rows, schemaId, schemaVersion);
        }

        private static EnemyRequirementChannel[] Channels()
        {
            return new[]
            {
                EnemyRequirementChannel.ContinuousBP,
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementChannel.RuntimeEventSignal
            };
        }

        private static EnemyRequirementApplicabilityState[] States()
        {
            return new[]
            {
                EnemyRequirementApplicabilityState.Applicable,
                EnemyRequirementApplicabilityState.Unknown,
                EnemyRequirementApplicabilityState.NotApplicable,
                EnemyRequirementApplicabilityState.NotInChannel
            };
        }

        private static void AssertValid(
            ICollection<Check> checks,
            string id,
            IEnemyRequirementChannelApplicabilitySnapshotValidator validator,
            EnemyRequirementChannelApplicabilitySnapshotInput input)
        {
            int count = validator.Validate(input).Count;
            Add(checks, id, "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
        }

        private static void AssertInvalid(
            ICollection<Check> checks,
            string id,
            IEnemyRequirementChannelApplicabilitySnapshotValidator validator,
            EnemyRequirementChannelApplicabilitySnapshotInput input,
            string expectedCode)
        {
            IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> issues =
                validator.Validate(input);
            string actual = string.Join("|", issues.Select(value => value.Code));
            Add(checks, id, expectedCode, actual,
                issues.Any(value => value.Code == expectedCode));
        }

        private static void AssertSignatureDifferent(
            ICollection<Check> checks,
            string id,
            EnemyRequirementChannelApplicabilitySnapshot baseline,
            EnemyRequirementChannelApplicabilitySnapshotInput changed)
        {
            EnemyRequirementChannelApplicabilitySnapshot snapshot =
                DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                    .CreateSnapshot(changed);
            Add(checks, id, "different", snapshot.CanonicalSignature,
                baseline.CanonicalSignature != snapshot.CanonicalSignature);
        }

        private static void AssertSignatureDifferent(
            ICollection<Check> checks,
            string id,
            EnemyRequirementChannelApplicabilitySnapshot baseline,
            EnemyRequirementChannelApplicabilitySnapshot changed)
        {
            Add(checks, id, "different", changed.CanonicalSignature,
                baseline.CanonicalSignature != changed.CanonicalSignature);
        }

        private static bool IsCanonicalSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                IsLowercaseSha256Hash(value.Substring(7));
        }

        private static bool IsLowercaseSha256Hash(string value)
        {
            return value != null && value.Length == 64 && value.All(character =>
                (character >= '0' && character <= '9') ||
                (character >= 'a' && character <= 'f'));
        }

        private static string MarkdownTableCodeValue(string source, string rowPrefix)
        {
            string row = (source ?? string.Empty).Split(new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(value => value.StartsWith(rowPrefix,
                    StringComparison.Ordinal));
            if (row == null)
            {
                return string.Empty;
            }

            int start = row.IndexOf('`');
            int end = start < 0 ? -1 : row.IndexOf('`', start + 1);
            return start >= 0 && end > start
                ? row.Substring(start + 1, end - start - 1)
                : string.Empty;
        }

        private static void AssertCount(
            ICollection<Check> checks,
            IDictionary<EnemyRequirementApplicabilityState, int> counts,
            EnemyRequirementApplicabilityState state,
            int expected)
        {
            int actual = counts.ContainsKey(state) ? counts[state] : 0;
            Add(checks, "fixture.state." + state,
                expected.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture), actual == expected);
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

        private static string[] CsvDataRows(string root, string path)
        {
            return File.ReadAllLines(Absolute(root, path), Encoding.UTF8).Skip(1)
                .Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
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

        private static bool TreeContains(string tree, string path)
        {
            return ("\n" + tree.Trim() + "\n").IndexOf(
                "\n" + path.Replace('\\', '/') + "\n", StringComparison.Ordinal) >= 0;
        }

        private static string ReadGuid(string path)
        {
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            string line = File.ReadLines(path).FirstOrDefault(value =>
                value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring(6).Trim();
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
                    throw new InvalidOperationException("git " + arguments + " failed: " + error);
                }
                return output;
            }
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static string FileHash(string root, string path)
        {
            return FileHash(root, path, false);
        }

        private static string FileHash(string root, string path, bool uppercase)
        {
            return Sha256(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                string value = string.Concat(sha.ComputeHash(bytes ?? new byte[0])
                    .Select(item => item.ToString(uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
                return value;
            }
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
                        Directory.Exists(Path.Combine(current.FullName, "ProjectSettings")) &&
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
            return Path.Combine(root,
                (path ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));
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
            Add(checks, id, expected, source == null ? "<null>" : "present/absent",
                source != null && source.IndexOf(expected, StringComparison.Ordinal) >= 0);
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
                Id = id;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }

            public string Id { get; private set; }
            public string Expected { get; private set; }
            public string Actual { get; private set; }
            public bool Passed { get; private set; }
        }

        private sealed class AggregateHash : IEquatable<AggregateHash>
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; private set; }
            public string Hash { get; private set; }

            public bool Equals(AggregateHash other)
            {
                return other != null && FileCount == other.FileCount && Hash == other.Hash;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as AggregateHash);
            }

            public override int GetHashCode()
            {
                return (FileCount * 397) ^ (Hash == null ? 0 : Hash.GetHashCode());
            }
        }

        private sealed class ProtectionSnapshot
        {
            public ProtectionSnapshot(
                AggregateHash item, AggregateHash enemy, AggregateHash c01,
                AggregateHash c02, AggregateHash c02A, AggregateHash n01,
                AggregateHash c02AD1, AggregateHash n01A, AggregateHash e06,
                AggregateHash e07, AggregateHash e08, AggregateHash e10,
                AggregateHash scenes, AggregateHash prefabs, string buildSettings)
            {
                Item = item;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
                C02A = c02A;
                N01 = n01;
                C02AD1 = c02AD1;
                N01A = n01A;
                E06 = e06;
                E07 = e07;
                E08 = e08;
                E10 = e10;
                Scenes = scenes;
                Prefabs = prefabs;
                BuildSettings = buildSettings;
            }

            public AggregateHash Item { get; private set; }
            public AggregateHash Enemy { get; private set; }
            public AggregateHash C01 { get; private set; }
            public AggregateHash C02 { get; private set; }
            public AggregateHash C02A { get; private set; }
            public AggregateHash N01 { get; private set; }
            public AggregateHash C02AD1 { get; private set; }
            public AggregateHash N01A { get; private set; }
            public AggregateHash E06 { get; private set; }
            public AggregateHash E07 { get; private set; }
            public AggregateHash E08 { get; private set; }
            public AggregateHash E10 { get; private set; }
            public AggregateHash Scenes { get; private set; }
            public AggregateHash Prefabs { get; private set; }
            public string BuildSettings { get; private set; }
        }
    }
}
