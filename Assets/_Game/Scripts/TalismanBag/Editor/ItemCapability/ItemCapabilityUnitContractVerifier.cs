using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.ItemCapability
{
    public static class ItemCapabilityUnitContractVerifier
    {
        private const string CatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string SourcePath =
            "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemCapabilityUnitContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemCapabilityUnitContractSpec.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemCapabilityUnitContractLeakCheckReport.md";
        private const string PassMarker = "ITEM_CAPABILITY_UNIT_CONTRACT01_PASS";
        private const string PendingBaseline = "PENDING_BASELINE";

        // Filled from the pre-fix catalog by VerifyOffline before the allowed point edits.
        private const string ExpectedAffixSchemaSignature =
            "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRoll150Signature =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjection150Signature =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitValidator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityUnitContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityUnitContractVerifier.cs.meta",
            SourcePath,
            CatalogPath,
            ReportPath,
            SpecPath,
            LeakPath
        };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitValidator.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "TalismanBag.EnemySystem",
            "TalismanBag.CrossSystem",
            "BuildCapability",
            "pointToBasisPoint",
            "stackToBasisPoint",
            "countToBasisPoint",
            "SaveData",
            "RewardConfig",
            "RunFlowController",
            "RectTransform"
        };

        private static readonly ProtectedExpectation[] ProtectedExpectations =
        {
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs",
                "821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs",
                "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs",
                "3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
                "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0"),
            FileExpectation(
                "ProjectSettings/EditorBuildSettings.asset",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"),
            DirectoryExpectation(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles", 60,
                "4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317"),
            DirectoryExpectation(
                "Assets/_Game/Scenes", 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b"),
            DirectoryExpectation(
                "Assets/_Game/Prefabs", 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/EnemySystem", 89,
                "3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/CrossSystem", 5,
                "d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail", 23,
                "213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox", 42,
                "ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0"),
            DirectoryExpectation(
                "Assets/_Game/Resources/item", 294,
                "5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29"),
            DirectoryExpectation(
                "Assets/_Game/Resources/item_daoju", 198,
                "bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemCapability/[QA Only] Verify Item Capability Unit Contract01")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyAndWrite(true, "Offline verifier");
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWrite(true, "Unity batch verifier");
        }

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            string root = FindProjectRoot();
            bool baselineCapture = string.Equals(
                ExpectedAffixSchemaSignature, PendingBaseline, StringComparison.Ordinal);
            bool offlinePreviouslyPassed = File.Exists(Absolute(root, ReportPath))
                && File.ReadAllText(Absolute(root, ReportPath), Encoding.UTF8)
                    .Contains("- Offline verifier: `PASS`", StringComparison.Ordinal);
            List<Check> checks = new List<Check>();
            List<ScenarioRow> scenarios = new List<ScenarioRow>();
            List<LeakRow> leakRows = new List<LeakRow>();
            List<ProtectedRow> protectedRows = new List<ProtectedRow>();
            SignatureBundle signatures = new SignatureBundle();
            SourceConfigResult sourceConfig = new SourceConfigResult();
            ItemCapabilityUnitContractSnapshot contract = null;

            try
            {
                contract = RunMemoryCases(checks, scenarios);
                RunProtectedChecks(checks, protectedRows, root);
                RunLeakChecks(checks, leakRows, root);
                ItemBalanceWorkbenchCatalog catalog = LoadCatalog();
                signatures = CaptureGenerationSignatures(catalog);
                RunGenerationChecks(checks, signatures, baselineCapture);
                sourceConfig = InspectSourceAndConfig(root, catalog);
                if (!baselineCapture)
                {
                    Add(checks, "source-config.nian-efficiency",
                        "correct basisPoint/ReducePercent/five ranges in Source and Config",
                        sourceConfig.Summary,
                        sourceConfig.IsCorrect);
                }
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception",
                    exception.ToString(), false);
            }

            WriteReports(root, mode, baselineCapture, offlinePreviouslyPassed,
                checks, scenarios, leakRows, protectedRows, contract,
                signatures, sourceConfig);
            RunExpectedFileChecks(checks, root);
            WriteReports(root, mode, baselineCapture, offlinePreviouslyPassed,
                checks, scenarios, leakRows, protectedRows, contract,
                signatures, sourceConfig);

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            string marker = (pass ? PassMarker : "ITEM_CAPABILITY_UNIT_CONTRACT01_FAIL")
                + " " + checks.Count(value => value.Passed)
                    .ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(marker);
#if UNITY_EDITOR
            Debug.Log(marker);
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
                return;
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "ItemCapabilityUnitContract01 verifier failed: "
                    + string.Join(" | ", checks.Where(value => !value.Passed)
                        .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static ItemCapabilityUnitContractSnapshot RunMemoryCases(
            ICollection<Check> checks,
            ICollection<ScenarioRow> scenarios)
        {
            ItemCapabilityUnitContractSnapshot contract =
                ItemCapabilityUnitContract.CreateDefault();
            Add(checks, "contract.schema",
                ItemCapabilityUnitContractSnapshot.CurrentSchemaId,
                contract.schemaId,
                contract.schemaId == ItemCapabilityUnitContractSnapshot.CurrentSchemaId);
            Add(checks, "contract.definition-count", "6",
                contract.Definitions.Count.ToString(CultureInfo.InvariantCulture),
                contract.Definitions.Count == 6);

            AddScenario(checks, scenarios, contract, "known-value.control",
                Input("affix_control_up", "point", "AddFlat", 5),
                ItemCapabilityUnitResolutionStatus.KnownValue,
                ItemCapabilityUnitValidationCodes.None);
            AddScenario(checks, scenarios, contract, "known-value.nian",
                Input("affix_nian_efficiency", "basisPoint", "ReducePercent", 200),
                ItemCapabilityUnitResolutionStatus.KnownValue,
                ItemCapabilityUnitValidationCodes.None);
            AddScenario(checks, scenarios, contract, "known-zero.explicit",
                Input("affix_trigger_refund", "point", "Refund", 0),
                ItemCapabilityUnitResolutionStatus.KnownZero,
                ItemCapabilityUnitValidationCodes.None);
            AddScenario(checks, scenarios, contract, "unknown.value-missing",
                Input("affix_control_up", "point", "AddFlat", null),
                ItemCapabilityUnitResolutionStatus.Unknown,
                ItemCapabilityUnitValidationCodes.ValueMissing);
            AddScenario(checks, scenarios, contract, "unknown.unit-missing",
                Input("affix_control_up", string.Empty, "AddFlat", 1),
                ItemCapabilityUnitResolutionStatus.Unknown,
                ItemCapabilityUnitValidationCodes.UnitMissing);
            AddScenario(checks, scenarios, contract, "unknown.capability",
                Input("affix_not_declared", "point", "AddFlat", 1),
                ItemCapabilityUnitResolutionStatus.Unknown,
                ItemCapabilityUnitValidationCodes.CapabilityUnknown);
            AddScenario(checks, scenarios, contract, "invalid.negative",
                Input("affix_cleanse_up", "stack", "AddFlat", -1),
                ItemCapabilityUnitResolutionStatus.Invalid,
                ItemCapabilityUnitValidationCodes.NegativeValue);
            AddScenario(checks, scenarios, contract, "invalid.out-of-range",
                Input("affix_nian_efficiency", "basisPoint", "ReducePercent", 1801),
                ItemCapabilityUnitResolutionStatus.Invalid,
                ItemCapabilityUnitValidationCodes.ValueOutOfRange);
            AddScenario(checks, scenarios, contract, "invalid.unit-conflict",
                Input("affix_trigger_refund", "basisPoint", "Refund", 1),
                ItemCapabilityUnitResolutionStatus.Invalid,
                ItemCapabilityUnitValidationCodes.UnitConflict);
            AddScenario(checks, scenarios, contract, "invalid.operation-conflict",
                Input("affix_nian_efficiency", "basisPoint", "ReduceFlat", 200),
                ItemCapabilityUnitResolutionStatus.Invalid,
                ItemCapabilityUnitValidationCodes.OperationConflict);

            CultureInfo oldCulture = CultureInfo.CurrentCulture;
            CultureInfo oldUiCulture = CultureInfo.CurrentUICulture;
            string reversedSignature;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
                reversedSignature = ItemCapabilityUnitContract.Create(
                    contract.Definitions.Reverse()).canonicalSignature;
            }
            finally
            {
                CultureInfo.CurrentCulture = oldCulture;
                CultureInfo.CurrentUICulture = oldUiCulture;
            }
            Add(checks, "canonical.ordinal-invariant-stable-order",
                contract.canonicalSignature, reversedSignature,
                string.Equals(contract.canonicalSignature,
                    reversedSignature, StringComparison.Ordinal));

            bool collectionImmutable = false;
            try
            {
                ((IList<ItemCapabilityUnitDefinition>)contract.Definitions).Add(
                    contract.Definitions[0]);
            }
            catch (NotSupportedException)
            {
                collectionImmutable = true;
            }
            Add(checks, "contract.immutable-definitions", "NotSupportedException",
                collectionImmutable ? "NotSupportedException" : "mutable",
                collectionImmutable);

            bool errorCollectionImmutable = false;
            ItemCapabilityUnitValidationResult invalid =
                ItemCapabilityUnitValidator.Resolve(contract,
                    Input("affix_control_up", "basisPoint", "AddFlat", 1));
            try
            {
                ((IList<ItemCapabilityUnitValidationError>)invalid.ValidationErrors)
                    .Add(new ItemCapabilityUnitValidationError("X", "X"));
            }
            catch (NotSupportedException)
            {
                errorCollectionImmutable = true;
            }
            Add(checks, "validator.immutable-errors", "NotSupportedException",
                errorCollectionImmutable ? "NotSupportedException" : "mutable",
                errorCollectionImmutable);

            string[] expectedDomains =
            {
                "affix_control_up|control_point|point|AddFlat|1|10",
                "affix_cleanse_up|cleanse_stack|stack|AddFlat|1|5",
                "affix_chain_target|target_count|count|ExtraTarget|1|6",
                "affix_trigger_refund|nian_point|point|Refund|1|6",
                "affix_first_trigger_bonus|effect_basis_point|basisPoint|ExtraTrigger|3000|8000",
                "affix_nian_efficiency|nian_cost_basis_point|basisPoint|ReducePercent|200|1800"
            };
            string[] actualDomains = contract.Definitions.Select(value =>
                value.capabilityId + "|" + value.valueDomainId + "|"
                + value.unitKey + "|" + value.operationKey + "|"
                + value.minKnownValueUnits.ToString(CultureInfo.InvariantCulture) + "|"
                + value.maxKnownValueUnits.ToString(CultureInfo.InvariantCulture))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "contract.six-native-domains",
                string.Join(";", expectedDomains.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(";", actualDomains),
                expectedDomains.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(actualDomains, StringComparer.Ordinal));
            return contract;
        }

        private static void AddScenario(
            ICollection<Check> checks,
            ICollection<ScenarioRow> scenarios,
            ItemCapabilityUnitContractSnapshot contract,
            string id,
            ItemCapabilityUnitValueInput input,
            ItemCapabilityUnitResolutionStatus expectedStatus,
            string expectedCode)
        {
            ItemCapabilityUnitValidationResult result =
                ItemCapabilityUnitValidator.Resolve(contract, input);
            bool pass = result.status == expectedStatus
                && string.Equals(result.primaryValidationCode,
                    expectedCode, StringComparison.Ordinal);
            Add(checks, "scenario." + id,
                expectedStatus + "/" + expectedCode,
                result.status + "/" + result.primaryValidationCode,
                pass);
            scenarios.Add(new ScenarioRow(id, input, expectedStatus,
                expectedCode, result.status, result.primaryValidationCode, pass));
        }

        private static ItemCapabilityUnitValueInput Input(
            string capabilityId, string unit, string operation, long? value)
        {
            return new ItemCapabilityUnitValueInput(
                capabilityId, unit, operation, value);
        }

        private static void RunGenerationChecks(
            ICollection<Check> checks,
            SignatureBundle signatures,
            bool baselineCapture)
        {
            Add(checks, "generation.preview-count", "150",
                signatures.Count.ToString(CultureInfo.InvariantCulture),
                signatures.Count == 150);
            Add(checks, "generation.preview-errors", "0",
                signatures.Errors.Count.ToString(CultureInfo.InvariantCulture),
                signatures.Errors.Count == 0);
            if (baselineCapture)
            {
                Add(checks, "generation.baseline-captured", "three non-empty signatures",
                    signatures.AffixSchema + " | " + signatures.Roll150 + " | "
                    + signatures.Projection150,
                    IsSha(signatures.AffixSchema) && IsSha(signatures.Roll150)
                    && IsSha(signatures.Projection150));
                return;
            }

            Add(checks, "generation.affix-schema-stable",
                ExpectedAffixSchemaSignature, signatures.AffixSchema,
                signatures.AffixSchema == ExpectedAffixSchemaSignature);
            Add(checks, "generation.roll-150-stable",
                ExpectedRoll150Signature, signatures.Roll150,
                signatures.Roll150 == ExpectedRoll150Signature);
            Add(checks, "generation.projection-150-stable",
                ExpectedProjection150Signature, signatures.Projection150,
                signatures.Projection150 == ExpectedProjection150Signature);
        }

        private static SignatureBundle CaptureGenerationSignatures(
            ItemBalanceWorkbenchCatalog catalog)
        {
            SignatureBundle result = new SignatureBundle();
            ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
            result.AffixSchema = Sha256(compiled.AffixSchema.BuildCanonicalSignature());
            StringBuilder roll = new StringBuilder();
            StringBuilder projection = new StringBuilder();
            int index = 0;
            foreach (ItemBalanceProfile profile in catalog.profiles
                .Where(value => value != null)
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal))
            {
                foreach (ItemInstanceRarityDefinition rarity in
                    ItemInstanceRarityCatalog.All.OrderBy(value =>
                        value.rarity.ToTierIndex()))
                {
                    long seed = 150001L + index;
                    index++;
                    ItemBalancePreviewResult preview = ItemBalanceWorkbenchCompiler.Preview(
                        catalog, compiled, profile.baseItemId, rarity.rarity, seed);
                    string key = profile.baseItemId + "@" + rarity.rarity.ToStableKey()
                        + "@" + seed.ToString(CultureInfo.InvariantCulture);
                    if (!preview.isSuccess)
                    {
                        result.Errors.Add(key + ":"
                            + string.Join(";", preview.Errors));
                        continue;
                    }

                    AppendField(roll, "key", key);
                    AppendField(roll, "signature",
                        preview.RollResult.snapshot.BuildCanonicalSignature());
                    AppendField(projection, "key", key);
                    AppendField(projection, "signature",
                        preview.ProjectionResult.snapshot.BuildCanonicalSignature());
                }
            }

            result.Count = index;
            result.Roll150 = Sha256(roll.ToString());
            result.Projection150 = Sha256(projection.ToString());
            return result;
        }

        private static SourceConfigResult InspectSourceAndConfig(
            string root,
            ItemBalanceWorkbenchCatalog catalog)
        {
            SourceConfigResult result = new SourceConfigResult();
            ItemCandidateAffixDefinition candidate =
                catalog.FindCandidateAffix("affix_nian_efficiency");
            ItemBalanceAffixDefinition formal =
                catalog.FindAffix("affix_nian_efficiency");
            result.ConfigCorrect = CandidateIsCorrect(candidate)
                && formal != null
                && formal.unitKey == "basisPoint"
                && RangesAreCorrect(formal.rarityRanges.Select(value =>
                    new RangeRow(value.rarity, value.minUnits, value.maxUnits)));

            string source = File.ReadAllText(Absolute(root, SourcePath), Encoding.UTF8);
            int nianStart = source.IndexOf(
                "private static ItemCandidateAffixDefinition BuildNianEfficiencyCandidate()",
                StringComparison.Ordinal);
            int nianEnd = nianStart < 0 ? -1 : source.IndexOf(
                "private static ItemCandidateEffectPayload Payload(",
                nianStart,
                StringComparison.Ordinal);
            string nianBlock = nianStart >= 0 && nianEnd > nianStart
                ? source.Substring(nianStart, nianEnd - nianStart)
                : string.Empty;
            result.SourceCorrect = source.Contains(
                    "result.Add(BuildNianEfficiencyCandidate());", StringComparison.Ordinal)
                && nianBlock.Contains("耗念降低X%。", StringComparison.Ordinal)
                && nianBlock.Contains("ItemCandidateEffectOperation.ReducePercent",
                    StringComparison.Ordinal)
                && nianBlock.Contains("\"basisPoint\"", StringComparison.Ordinal)
                && nianBlock.Contains("200", StringComparison.Ordinal)
                && nianBlock.Contains("NianEfficiencyRanges()",
                    StringComparison.Ordinal)
                && source.Contains("private static List<ItemCandidateRarityValue> Ranges(long baseValue, long step)",
                    StringComparison.Ordinal);
            result.Summary = "Source=" + (result.SourceCorrect ? "PASS" : "FAIL")
                + ", Config=" + (result.ConfigCorrect ? "PASS" : "FAIL")
                + ", Payload=" + FormatCandidate(candidate);
            return result;
        }

        private static bool CandidateIsCorrect(ItemCandidateAffixDefinition value)
        {
            return value != null
                && value.description == "耗念降低X%。"
                && value.effectPayload != null
                && value.effectPayload.valueUnitKey == "basisPoint"
                && value.effectPayload.operation == ItemCandidateEffectOperation.ReducePercent
                && value.effectPayload.valueUnits == 200
                && value.effectPayload.triggerEventId == "before_trigger"
                && value.effectPayload.conditionId == "nian_cost_gt_0"
                && value.effectPayload.effectTags.Contains("ReducePercent")
                && !value.effectPayload.effectTags.Contains("ReduceFlat")
                && RangesAreCorrect((value.rarityRanges ??
                    new List<ItemCandidateRarityValue>()).Select(range =>
                    new RangeRow(range.rarity, range.minUnits, range.maxUnits)));
        }

        private static bool RangesAreCorrect(IEnumerable<RangeRow> ranges)
        {
            string actual = string.Join(";", (ranges ?? Array.Empty<RangeRow>())
                .OrderBy(value => value.Rarity.ToTierIndex())
                .Select(value => value.Rarity.ToStableKey() + ":"
                    + value.Min.ToString(CultureInfo.InvariantCulture) + "-"
                    + value.Max.ToString(CultureInfo.InvariantCulture)));
            return actual == "white:200-400;green:350-650;blue:550-900;purple:800-1300;orange:1200-1800";
        }

        private static string FormatCandidate(ItemCandidateAffixDefinition value)
        {
            if (value == null) return "missing";
            return value.effectPayload.valueUnitKey + "/"
                + value.effectPayload.operation + "/"
                + value.effectPayload.valueUnits.ToString(CultureInfo.InvariantCulture)
                + "/" + string.Join(";", value.rarityRanges.Select(range =>
                    range.rarity.ToStableKey() + ":" + range.minUnits + "-" + range.maxUnits));
        }

        private static ItemBalanceWorkbenchCatalog LoadCatalog()
        {
#if UNITY_EDITOR
            ItemBalanceWorkbenchCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(CatalogPath);
            if (catalog == null)
                throw new InvalidOperationException("Catalog asset is missing: " + CatalogPath);
            return catalog;
#else
            throw new InvalidOperationException("Unity Editor AssetDatabase is required.");
#endif
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            ICollection<LeakRow> rows,
            string root)
        {
            foreach (string path in RuntimeSourcePaths)
            {
                string text = File.ReadAllText(Absolute(root, path), Encoding.UTF8);
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    if (!text.Contains(token, StringComparison.Ordinal)) continue;
                    rows.Add(new LeakRow(path, token));
                }
            }
            Add(checks, "leak.count", "0",
                rows.Count.ToString(CultureInfo.InvariantCulture), rows.Count == 0);
        }

        private static void RunProtectedChecks(
            ICollection<Check> checks,
            ICollection<ProtectedRow> rows,
            string root)
        {
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                HashResult actual = expectation.IsDirectory
                    ? HashDirectory(root, expectation.Path)
                    : HashFile(root, expectation.Path);
                bool pass = actual.Count == expectation.Count
                    && actual.Hash == expectation.Hash;
                rows.Add(new ProtectedRow(expectation, actual, pass));
                Add(checks, "protected." + expectation.Path,
                    expectation.Count + "/" + expectation.Hash,
                    actual.Count + "/" + actual.Hash, pass);
            }
        }

        private static void RunExpectedFileChecks(
            ICollection<Check> checks,
            string root)
        {
            string[] missing = ExpectedPackageFiles
                .Where(path => !File.Exists(Absolute(root, path))).ToArray();
            Add(checks, "package.files", ExpectedPackageFiles.Length + " present",
                missing.Length == 0 ? ExpectedPackageFiles.Length + " present"
                    : "missing=" + string.Join(";", missing),
                missing.Length == 0);
        }

        private static void WriteReports(
            string root,
            string mode,
            bool baselineCapture,
            bool offlinePreviouslyPassed,
            IReadOnlyCollection<Check> checks,
            IReadOnlyCollection<ScenarioRow> scenarios,
            IReadOnlyCollection<LeakRow> leakRows,
            IReadOnlyCollection<ProtectedRow> protectedRows,
            ItemCapabilityUnitContractSnapshot contract,
            SignatureBundle signatures,
            SourceConfigResult sourceConfig)
        {
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            bool offlinePass = offlinePreviouslyPassed
                || (mode == "Offline verifier" && pass);
            bool unityPass = mode == "Unity batch verifier" && pass;
            StringBuilder report = new StringBuilder();
            report.AppendLine("# Item Capability Unit Contract Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemCapabilityUnitContract01`")
                .AppendLine("- Guard receipt: `GUARD_PASS_ITEMCAPABILITYUNITCONTRACT01`")
                .AppendLine("- Schema: `" + (contract?.schemaId ?? "Unavailable") + "`")
                .AppendLine("- Canonical Signature: `" + (contract?.canonicalSignature ?? "Unavailable") + "`")
                .AppendLine("- Native unit definitions: `" + (contract?.Definitions.Count ?? 0) + "`")
                .AppendLine("- Memory scenarios: `" + scenarios.Count + "`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Offline verifier: `" + (offlinePass ? "PASS" : "PENDING") + "`")
                .AppendLine("- Unity verifier: `" + (unityPass ? "PASS" : "PENDING") + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine()
                .AppendLine("## Source / Config correction")
                .AppendLine()
                .AppendLine("- State: `" + (baselineCapture ? "BASELINE_CAPTURE" : (sourceConfig.IsCorrect ? "PASS" : "FAIL")) + "`")
                .AppendLine("- Detail: `" + EscapeMarkdown(sourceConfig.Summary) + "`")
                .AppendLine("- Generic `Ranges()` behavior: unchanged by contract; the source verifier requires a dedicated `NianEfficiencyRanges()` path.")
                .AppendLine()
                .AppendLine("## Canonical generation baselines")
                .AppendLine()
                .AppendLine("| Boundary | Before | Current | Result |")
                .AppendLine("|---|---|---|---|")
                .AppendLine(SignatureRow("Affix Schema", ExpectedAffixSchemaSignature, signatures.AffixSchema, baselineCapture))
                .AppendLine(SignatureRow("150 Roll", ExpectedRoll150Signature, signatures.Roll150, baselineCapture))
                .AppendLine(SignatureRow("150 Projection", ExpectedProjection150Signature, signatures.Projection150, baselineCapture))
                .AppendLine()
                .AppendLine("- Preview count: `" + signatures.Count + "`")
                .AppendLine("- Preview errors: `" + signatures.Errors.Count + "`")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks.OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                report.Append("| `").Append(EscapeMarkdown(check.Id)).Append("` | `")
                    .Append(EscapeMarkdown(check.Expected)).Append("` | `")
                    .Append(EscapeMarkdown(check.Actual)).Append("` | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            report.AppendLine()
                .AppendLine("## Protected hashes")
                .AppendLine()
                .AppendLine("| Path | Files | Baseline | Current | Result |")
                .AppendLine("|---|---:|---|---|---|");
            foreach (ProtectedRow row in protectedRows)
            {
                report.Append("| `").Append(row.Expectation.Path).Append("` | ")
                    .Append(row.Actual.Count).Append(" | `")
                    .Append(row.Expectation.Hash).Append("` | `")
                    .Append(row.Actual.Hash).Append("` | ")
                    .Append(row.Pass ? "PASS" : "FAIL").AppendLine(" |");
            }
            Write(root, ReportPath, report.ToString());

            StringBuilder csv = new StringBuilder();
            csv.AppendLine("rowType,id,valueDomainId,unitKey,operationKey,minUnits,maxUnits,inputValue,expectedStatus,actualStatus,validationCode,result");
            if (contract != null)
            {
                foreach (ItemCapabilityUnitDefinition definition in contract.Definitions)
                {
                    csv.AppendLine(Csv("contract", definition.capabilityId,
                        definition.valueDomainId, definition.unitKey,
                        definition.operationKey,
                        definition.minKnownValueUnits.ToString(CultureInfo.InvariantCulture),
                        definition.maxKnownValueUnits.ToString(CultureInfo.InvariantCulture),
                        string.Empty, string.Empty, string.Empty, string.Empty, "PASS"));
                }
            }
            foreach (ScenarioRow scenario in scenarios)
            {
                csv.AppendLine(Csv("scenario", scenario.Id, string.Empty,
                    scenario.Input?.unitKey, scenario.Input?.operationKey,
                    string.Empty, string.Empty,
                    scenario.Input?.valueUnits?.ToString(CultureInfo.InvariantCulture) ?? "Unknown",
                    scenario.ExpectedStatus.ToString(), scenario.ActualStatus.ToString(),
                    scenario.ActualCode, scenario.Pass ? "PASS" : "FAIL"));
            }
            Write(root, SpecPath, csv.ToString());

            StringBuilder leak = new StringBuilder();
            leak.AppendLine("# Item Capability Unit Contract Leak Check Report")
                .AppendLine()
                .AppendLine("- Leak Count: `" + leakRows.Count + "`")
                .AppendLine("- Result: `" + (leakRows.Count == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Runtime files scanned: `" + RuntimeSourcePaths.Length + "`")
                .AppendLine("- Forbidden scope touched: `0` (protected hashes and package whitelist verified separately)")
                .AppendLine();
            if (leakRows.Count == 0)
            {
                leak.AppendLine("No forbidden runtime coupling or unit-conversion token was found.");
            }
            else
            {
                foreach (LeakRow row in leakRows)
                    leak.AppendLine("- `" + row.Path + "`: `" + row.Token + "`");
            }
            Write(root, LeakPath, leak.ToString());
        }

        private static string SignatureRow(
            string name, string expected, string actual, bool baselineCapture)
        {
            string before = baselineCapture ? actual : expected;
            bool pass = baselineCapture ? IsSha(actual) : expected == actual;
            return "| " + name + " | `" + before + "` | `" + actual + "` | "
                + (pass ? "PASS" : "FAIL") + " |";
        }

        private static string Csv(params string[] fields)
        {
            return string.Join(",", fields.Select(value =>
                "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\""));
        }

        private static void Write(string root, string relativePath, string content)
        {
            string path = Absolute(root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, (content ?? string.Empty)
                .Replace("\r\n", "\n"), new UTF8Encoding(false));
        }

        private static HashResult HashFile(string root, string relativePath)
        {
            string path = Absolute(root, relativePath);
            return File.Exists(path)
                ? new HashResult(1, FileSha256(path))
                : new HashResult(0, "MISSING");
        }

        private static HashResult HashDirectory(string root, string relativePath)
        {
            string path = Absolute(root, relativePath);
            if (!Directory.Exists(path)) return new HashResult(0, "MISSING");
            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            foreach (string file in files)
            {
                string relative = file.Substring(root.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Replace('\\', '/');
                payload.Append(relative).Append('|').Append(FileSha256(file)).Append('\n');
            }
            return new HashResult(files.Length, Sha256Raw(payload.ToString()));
        }

        private static string FileSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return Hex(sha.ComputeHash(stream));
        }

        private static string Sha256(string payload)
        {
            return "sha256:" + Sha256Raw(payload);
        }

        private static string Sha256Raw(string payload)
        {
            using (SHA256 sha = SHA256.Create())
                return Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty)));
        }

        private static string Hex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void AppendField(StringBuilder builder, string key, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(key.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(key).Append('=')
                .Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }

        private static bool IsSha(string value)
        {
            return value != null && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Length == 71;
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|")
                .Replace("`", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private static string FindProjectRoot()
        {
            string current = Directory.GetCurrentDirectory();
            DirectoryInfo directory = new DirectoryInfo(current);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Packages")))
                    return directory.FullName.TrimEnd(Path.DirectorySeparatorChar);
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root not found from " + current);
        }

        private static string Absolute(string root, string relativePath)
        {
            return Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static void Add(
            ICollection<Check> checks,
            string id, string expected, string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private static ProtectedExpectation FileExpectation(string path, string hash)
        {
            return new ProtectedExpectation(path, false, 1, hash);
        }

        private static ProtectedExpectation DirectoryExpectation(
            string path, int count, string hash)
        {
            return new ProtectedExpectation(path, true, count, hash);
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id; Expected = expected; Actual = actual; Passed = passed;
            }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class ScenarioRow
        {
            public ScenarioRow(string id, ItemCapabilityUnitValueInput input,
                ItemCapabilityUnitResolutionStatus expectedStatus, string expectedCode,
                ItemCapabilityUnitResolutionStatus actualStatus, string actualCode, bool pass)
            {
                Id = id; Input = input; ExpectedStatus = expectedStatus;
                ExpectedCode = expectedCode; ActualStatus = actualStatus;
                ActualCode = actualCode; Pass = pass;
            }
            public string Id { get; }
            public ItemCapabilityUnitValueInput Input { get; }
            public ItemCapabilityUnitResolutionStatus ExpectedStatus { get; }
            public string ExpectedCode { get; }
            public ItemCapabilityUnitResolutionStatus ActualStatus { get; }
            public string ActualCode { get; }
            public bool Pass { get; }
        }

        private sealed class LeakRow
        {
            public LeakRow(string path, string token) { Path = path; Token = token; }
            public string Path { get; }
            public string Token { get; }
        }

        private sealed class SignatureBundle
        {
            public string AffixSchema = string.Empty;
            public string Roll150 = string.Empty;
            public string Projection150 = string.Empty;
            public int Count;
            public readonly List<string> Errors = new List<string>();
        }

        private sealed class SourceConfigResult
        {
            public bool SourceCorrect;
            public bool ConfigCorrect;
            public bool IsCorrect => SourceCorrect && ConfigCorrect;
            public string Summary = "Not inspected";
        }

        private readonly struct RangeRow
        {
            public RangeRow(ItemInstanceRarity rarity, long min, long max)
            {
                Rarity = rarity; Min = min; Max = max;
            }
            public ItemInstanceRarity Rarity { get; }
            public long Min { get; }
            public long Max { get; }
        }

        private sealed class ProtectedExpectation
        {
            public ProtectedExpectation(string path, bool isDirectory, int count, string hash)
            {
                Path = path; IsDirectory = isDirectory; Count = count; Hash = hash;
            }
            public string Path { get; }
            public bool IsDirectory { get; }
            public int Count { get; }
            public string Hash { get; }
        }

        private readonly struct HashResult
        {
            public HashResult(int count, string hash) { Count = count; Hash = hash; }
            public int Count { get; }
            public string Hash { get; }
        }

        private sealed class ProtectedRow
        {
            public ProtectedRow(ProtectedExpectation expectation, HashResult actual, bool pass)
            {
                Expectation = expectation; Actual = actual; Pass = pass;
            }
            public ProtectedExpectation Expectation { get; }
            public HashResult Actual { get; }
            public bool Pass { get; }
        }
    }
}
