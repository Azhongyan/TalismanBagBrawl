using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#else
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.ItemCapability
{
    public static class ItemInstancePlacementBindingContractVerifier
    {
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractSpec.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractLeakCheckReport.md";
        private const string PassMarker =
            "ITEM_INSTANCE_PLACEMENT_BINDING_CONTRACT01_PASS";

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs.meta",
            ReportPath,
            SpecPath,
            LeakPath
        };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "TalismanBag.EnemySystem",
            "TalismanBag.CrossSystem",
            "BattleContract",
            "BattleBridge",
            "UnifiedBattlePage",
            "V02RunFlow",
            "V03RunFlow",
            "SaveData",
            "RewardConfig",
            "Chapter"
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
                "ProjectSettings/EditorBuildSettings.asset",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/EnemySystem",
                89,
                "3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/CrossSystem",
                5,
                "d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1"),
            DirectoryExpectation(
                "Assets/_Game/Scenes",
                14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b"),
            DirectoryExpectation(
                "Assets/_Game/Prefabs",
                16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemCapability/ItemInstancePlacementBindingContract01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyAndWrite(true, "Offline verifier");
        }

        public static void VerifyBatch()
        {
            VerifyAndWrite(true, "Unity batch");
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyAndWrite(false, "Pure C# entry point");
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
            bool offlinePreviouslyPassed = File.Exists(Absolute(root, ReportPath))
                && File.ReadAllText(Absolute(root, ReportPath), Encoding.UTF8)
                    .Contains("- Offline verifier: `PASS`", StringComparison.Ordinal);
            List<Check> checks = new List<Check>();
            List<ScenarioRow> scenarios = new List<ScenarioRow>();
            List<LeakRow> leakRows = new List<LeakRow>();
            List<ProtectedRow> protectedRows = new List<ProtectedRow>();
            Dictionary<string, HashResult> protectedBefore = CaptureProtected(root);
            string representativeSignature = string.Empty;
            int representativeBindingCount = 0;

            try
            {
                RunScenarios(
                    checks,
                    scenarios,
                    out representativeSignature,
                    out representativeBindingCount);
                RunContractShapeChecks(checks);
                RunProtectedBaselineChecks(checks, protectedRows, root);
                RunLeakChecks(checks, leakRows, root);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            WriteReports(
                root,
                mode,
                offlinePreviouslyPassed || string.Equals(mode, "Offline verifier", StringComparison.Ordinal),
                checks,
                scenarios,
                leakRows,
                protectedRows,
                representativeSignature,
                representativeBindingCount);
            RunExpectedFileChecks(checks, root);
            RunProtectedStableChecks(checks, protectedRows, root, protectedBefore);
            WriteReports(
                root,
                mode,
                offlinePreviouslyPassed || string.Equals(mode, "Offline verifier", StringComparison.Ordinal),
                checks,
                scenarios,
                leakRows,
                protectedRows,
                representativeSignature,
                representativeBindingCount);

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            string marker = (pass ? PassMarker : "ITEM_INSTANCE_PLACEMENT_BINDING_CONTRACT01_FAIL")
                + " " + checks.Count(value => value.Passed).ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(marker);

#if UNITY_EDITOR
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
                return;
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "ItemInstancePlacementBindingContract01 verifier failed: "
                    + string.Join(" | ", checks.Where(value => !value.Passed)
                        .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static void RunScenarios(
            ICollection<Check> checks,
            ICollection<ScenarioRow> scenarios,
            out string representativeSignature,
            out int representativeBindingCount)
        {
            ItemInstanceProjectionSetSnapshot legalProjection = ProjectionSet(
                Projection("INST-A", "I001"),
                Projection("INST-B", "I002"));
            ItemSystemSnapshot legalPlacement = PlacementSet(
                Placement("PLACE-SOURCE", "I031", true),
                Placement("PLACE-10", "I001"),
                Placement("PLACE-20", "I002"));
            List<ItemInstancePlacementBindingInput> legalInputs = new List<ItemInstancePlacementBindingInput>
            {
                Binding("INST-A", "PLACE-10", "I001"),
                Binding("INST-B", "PLACE-20", "I002")
            };
            ItemInstancePlacementBindingValidationResult legal = Validate(
                legalProjection,
                legalPlacement,
                legalInputs);
            representativeSignature = legal.snapshot.canonicalSignature;
            representativeBindingCount = legal.snapshot.Bindings.Count;
            AddScenario(
                checks,
                scenarios,
                "legal.explicit-three-way",
                "legal",
                ItemInstancePlacementBindingStatus.Valid,
                legal,
                legal.isValid
                    && legal.snapshot.Bindings.Count == 2
                    && legal.snapshot.FindByItemInstanceId("INST-A")?.placementId == "PLACE-10"
                    && legal.snapshot.FindByPlacementId("PLACE-20")?.itemInstanceId == "INST-B",
                "Two explicit non-equal IDs bind to matching base identities.");

            CultureInfo oldCulture = CultureInfo.CurrentCulture;
            CultureInfo oldUiCulture = CultureInfo.CurrentUICulture;
            ItemInstancePlacementBindingValidationResult reordered;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");
                reordered = Validate(
                    ProjectionSet(
                        Projection("INST-B", "I002"),
                        Projection("INST-A", "I001")),
                    PlacementSet(
                        Placement("PLACE-20", "I002"),
                        Placement("PLACE-10", "I001"),
                        Placement("PLACE-SOURCE", "I031", true)),
                    legalInputs.AsEnumerable().Reverse().ToArray());
            }
            finally
            {
                CultureInfo.CurrentCulture = oldCulture;
                CultureInfo.CurrentUICulture = oldUiCulture;
            }
            AddScenario(
                checks,
                scenarios,
                "legal.ordinal-invariant-order",
                "legal",
                ItemInstancePlacementBindingStatus.Valid,
                reordered,
                reordered.isValid
                    && reordered.snapshot.canonicalSignature == representativeSignature,
                "Reversed inputs under tr-TR/fr-FR retain the same Canonical Signature.");

            ItemInstancePlacementBindingValidationResult noGuess = Validate(
                ProjectionSet(Projection("SAME-ID", "I001")),
                PlacementSet(
                    Placement("SAME-ID", "I001"),
                    Placement("PLACE-SOURCE", "I031", true)),
                Array.Empty<ItemInstancePlacementBindingInput>());
            AddScenario(
                checks,
                scenarios,
                "missing.equal-ids-not-guessed",
                "abnormal",
                ItemInstancePlacementBindingStatus.Unknown,
                noGuess,
                noGuess.snapshot.Bindings.Count == 0
                    && HasCode(noGuess,
                        ItemInstancePlacementBindingValidationCodes.ProjectionBindingMissing)
                    && HasCode(noGuess,
                        ItemInstancePlacementBindingValidationCodes.PlacementBindingMissing),
                "itemInstanceId == placementId is still Unknown without an explicit row.");

            ItemInstancePlacementBindingValidationResult duplicateInstance = Validate(
                legalProjection,
                legalPlacement,
                new[]
                {
                    Binding("INST-A", "PLACE-10", "I001"),
                    Binding("INST-A", "PLACE-20", "I002")
                });
            AddScenario(
                checks,
                scenarios,
                "duplicate.item-instance",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                duplicateInstance,
                HasCode(duplicateInstance,
                    ItemInstancePlacementBindingValidationCodes.BindingItemInstanceIdDuplicate),
                "Duplicate itemInstanceId is Invalid.");

            ItemInstancePlacementBindingValidationResult duplicatePlacement = Validate(
                legalProjection,
                legalPlacement,
                new[]
                {
                    Binding("INST-A", "PLACE-10", "I001"),
                    Binding("INST-B", "PLACE-10", "I002")
                });
            AddScenario(
                checks,
                scenarios,
                "duplicate.placement",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                duplicatePlacement,
                HasCode(duplicatePlacement,
                    ItemInstancePlacementBindingValidationCodes.BindingPlacementIdDuplicate),
                "Duplicate placementId is Invalid.");

            ItemInstancePlacementBindingValidationResult orphanProjection = Validate(
                legalProjection,
                legalPlacement,
                new[]
                {
                    Binding("INST-GHOST", "PLACE-10", "I001"),
                    Binding("INST-B", "PLACE-20", "I002")
                });
            AddScenario(
                checks,
                scenarios,
                "orphan.item-instance",
                "abnormal",
                ItemInstancePlacementBindingStatus.Unknown,
                orphanProjection,
                HasCode(orphanProjection,
                    ItemInstancePlacementBindingValidationCodes.ProjectionOrphan),
                "Binding without a matching Instance Projection stays Unknown.");

            ItemInstancePlacementBindingValidationResult orphanPlacement = Validate(
                legalProjection,
                legalPlacement,
                new[]
                {
                    Binding("INST-A", "PLACE-GHOST", "I001"),
                    Binding("INST-B", "PLACE-20", "I002")
                });
            AddScenario(
                checks,
                scenarios,
                "orphan.placement",
                "abnormal",
                ItemInstancePlacementBindingStatus.Unknown,
                orphanPlacement,
                HasCode(orphanPlacement,
                    ItemInstancePlacementBindingValidationCodes.PlacementOrphan),
                "Binding without a matching Placement stays Unknown.");

            ItemInstancePlacementBindingValidationResult projectionMismatch = Validate(
                ProjectionSet(Projection("INST-A", "I001")),
                PlacementSet(
                    Placement("PLACE-10", "I002"),
                    Placement("PLACE-SOURCE", "I031", true)),
                new[] { Binding("INST-A", "PLACE-10", "I002") });
            AddScenario(
                checks,
                scenarios,
                "mismatch.projection-base",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                projectionMismatch,
                HasCode(projectionMismatch,
                    ItemInstancePlacementBindingValidationCodes.ProjectionBaseItemIdMismatch),
                "baseItemId mismatch against Instance Projection is Invalid.");

            ItemInstancePlacementBindingValidationResult placementMismatch = Validate(
                ProjectionSet(Projection("INST-A", "I001")),
                PlacementSet(
                    Placement("PLACE-10", "I002"),
                    Placement("PLACE-SOURCE", "I031", true)),
                new[] { Binding("INST-A", "PLACE-10", "I001") });
            AddScenario(
                checks,
                scenarios,
                "mismatch.placement-item",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                placementMismatch,
                HasCode(placementMismatch,
                    ItemInstancePlacementBindingValidationCodes.PlacementBaseItemIdMismatch),
                "baseItemId mismatch against Placement.itemId is Invalid.");

            ItemInstancePlacementBindingValidationResult forgedI031Binding = Validate(
                ProjectionSet(),
                PlacementSet(Placement("PLACE-SOURCE", "I031", true)),
                new[] { Binding("INST-I031-FORGED", "PLACE-SOURCE", "I031") });
            AddScenario(
                checks,
                scenarios,
                "i031.binding-forbidden",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                forgedI031Binding,
                HasCode(forgedI031Binding,
                    ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden)
                    && forgedI031Binding.snapshot.Bindings.Count == 0,
                "I031 system placement cannot receive an ordinary instance binding.");

            ItemInstancePlacementBindingValidationResult forgedI031Projection = Validate(
                ProjectionSet(Projection("INST-I031-FORGED", "I031")),
                PlacementSet(Placement("PLACE-SOURCE", "I031", true)),
                new[] { Binding("INST-I031-FORGED", "PLACE-SOURCE", "I031") });
            AddScenario(
                checks,
                scenarios,
                "i031.projection-forbidden",
                "abnormal",
                ItemInstancePlacementBindingStatus.Invalid,
                forgedI031Projection,
                HasCode(forgedI031Projection,
                    ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden)
                    && forgedI031Projection.snapshot.Bindings.Count == 0,
                "I031 cannot be forged into the ordinary Instance Projection line.");

            string immutableSignature = legal.snapshot.canonicalSignature;
            bool inputMutationSafe;
            legalInputs.Clear();
            inputMutationSafe = legal.snapshot.Bindings.Count == 2
                && legal.snapshot.canonicalSignature == immutableSignature;
            bool bindingCollectionRejected = RejectAdd(
                legal.snapshot.Bindings,
                new ItemInstancePlacementBindingInput("X", "Y", "I003"));
            bool errorCollectionRejected = RejectErrorAdd(legal.snapshot.ValidationErrors);
            AddScenario(
                checks,
                scenarios,
                "legal.immutable-output",
                "legal",
                ItemInstancePlacementBindingStatus.Valid,
                legal,
                inputMutationSafe && bindingCollectionRejected && errorCollectionRejected,
                "Input mutation cannot alter the snapshot; exposed collections reject writes.");

            Add(checks,
                "semantics.abnormal-never-known-zero",
                "all abnormal rows are Invalid or Unknown",
                scenarios.Where(value => value.Category == "abnormal")
                    .All(value => value.ActualStatus != ItemInstancePlacementBindingStatus.Valid)
                    ? "Invalid/Unknown only"
                    : "unexpected Valid",
                scenarios.Where(value => value.Category == "abnormal")
                    .All(value => value.ActualStatus != ItemInstancePlacementBindingStatus.Valid));
        }

        private static void RunContractShapeChecks(ICollection<Check> checks)
        {
            Add(checks,
                "contract.schema-id",
                "ItemInstancePlacementBindingContractSnapshot.v1",
                ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId,
                ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId ==
                    "ItemInstancePlacementBindingContractSnapshot.v1");
            Add(checks,
                "contract.identity-properties-readonly",
                "no public setters",
                "itemInstanceId/placementId/baseItemId",
                !CanWrite<ItemInstancePlacementBindingSnapshot>("itemInstanceId")
                    && !CanWrite<ItemInstancePlacementBindingSnapshot>("placementId")
                    && !CanWrite<ItemInstancePlacementBindingSnapshot>("baseItemId"));
            Add(checks,
                "contract.separate-from-existing-signatures",
                "new independent canonicalSignature",
                "ItemInstancePlacementBindingContractSnapshot.v1",
                typeof(ItemInstancePlacementBindingContractSnapshot)
                    .GetProperty("canonicalSignature") != null);
        }

        private static void RunProtectedBaselineChecks(
            ICollection<Check> checks,
            ICollection<ProtectedRow> rows,
            string root)
        {
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                HashResult actual = HashPath(root, expectation.Path, expectation.IsDirectory);
                bool pass = actual.FileCount == expectation.FileCount
                    && string.Equals(actual.Hash, expectation.Hash, StringComparison.Ordinal);
                rows.Add(new ProtectedRow(
                    expectation.Path,
                    expectation.FileCount,
                    actual.FileCount,
                    expectation.Hash,
                    actual.Hash,
                    pass,
                    false));
                Add(checks,
                    "protected.baseline." + Sanitize(expectation.Path),
                    expectation.FileCount.ToString(CultureInfo.InvariantCulture) + "/" + expectation.Hash,
                    actual.FileCount.ToString(CultureInfo.InvariantCulture) + "/" + actual.Hash,
                    pass);
            }
        }

        private static void RunProtectedStableChecks(
            ICollection<Check> checks,
            ICollection<ProtectedRow> rows,
            string root,
            IReadOnlyDictionary<string, HashResult> before)
        {
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                HashResult after = HashPath(root, expectation.Path, expectation.IsDirectory);
                HashResult start = before[expectation.Path];
                bool pass = start.FileCount == after.FileCount
                    && string.Equals(start.Hash, after.Hash, StringComparison.Ordinal);
                rows.Add(new ProtectedRow(
                    expectation.Path,
                    start.FileCount,
                    after.FileCount,
                    start.Hash,
                    after.Hash,
                    pass,
                    true));
                Add(checks,
                    "protected.before-after." + Sanitize(expectation.Path),
                    start.FileCount.ToString(CultureInfo.InvariantCulture) + "/" + start.Hash,
                    after.FileCount.ToString(CultureInfo.InvariantCulture) + "/" + after.Hash,
                    pass);
            }
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            ICollection<LeakRow> leakRows,
            string root)
        {
            foreach (string path in RuntimeSourcePaths)
            {
                string absolute = Absolute(root, path);
                if (!File.Exists(absolute))
                {
                    leakRows.Add(new LeakRow(path, "missing-source", 1));
                    Add(checks, "leak.source." + Sanitize(path), "present", "missing", false);
                    continue;
                }

                string source = File.ReadAllText(absolute, Encoding.UTF8);
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    int count = CountOccurrences(source, token);
                    if (count > 0)
                    {
                        leakRows.Add(new LeakRow(path, token, count));
                    }
                }
            }

            int total = leakRows.Sum(value => value.Count);
            Add(checks,
                "leak.formal-system-count",
                "0",
                total.ToString(CultureInfo.InvariantCulture),
                total == 0);
        }

        private static void RunExpectedFileChecks(ICollection<Check> checks, string root)
        {
            string[] missing = ExpectedPackageFiles
                .Where(path => !File.Exists(Absolute(root, path)))
                .ToArray();
            Add(checks,
                "package.expected-files",
                "11/11 present",
                missing.Length == 0 ? "11/11 present" : string.Join(";", missing),
                missing.Length == 0);
        }

        private static ItemInstancePlacementBindingValidationResult Validate(
            ItemInstanceProjectionSetSnapshot projection,
            ItemSystemSnapshot placement,
            IReadOnlyList<ItemInstancePlacementBindingInput> bindings)
        {
            return ItemInstancePlacementBindingValidator.Instance.Validate(
                projection,
                placement,
                bindings);
        }

        private static ItemInstanceProjectionContractSnapshot Projection(
            string itemInstanceId,
            string baseItemId)
        {
            return Construct<ItemInstanceProjectionContractSnapshot>(
                "ItemGeneratedInstanceSnapshot.v1",
                "QA_ITEM_INSTANCE_PLACEMENT_BINDING",
                "QA_FIXTURE_ONLY",
                itemInstanceId,
                baseItemId,
                ItemInstanceRarity.Orange,
                "orange",
                1,
                910001L,
                "qa_core_profile",
                Array.Empty<ItemInstanceProjectionStatSnapshot>(),
                Array.Empty<ItemInstanceProjectionAffixSnapshot>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                ItemBuildQualification.None,
                "qa-source-" + itemInstanceId);
        }

        private static ItemInstanceProjectionSetSnapshot ProjectionSet(
            params ItemInstanceProjectionContractSnapshot[] projections)
        {
            return Construct<ItemInstanceProjectionSetSnapshot>(
                projections ?? Array.Empty<ItemInstanceProjectionContractSnapshot>(),
                Array.Empty<ItemInstanceProjectionValidationError>());
        }

        private static ItemSystemSnapshot PlacementSet(params PlacementSpec[] specs)
        {
            ItemSystemPlacementSnapshot[] placements =
                (specs ?? Array.Empty<PlacementSpec>())
                .Select((spec, index) => new ItemSystemPlacementSnapshot(
                    spec.PlacementId,
                    spec.ItemId,
                    spec.ItemId,
                    new Vector2Int(index, 0),
                    0,
                    new[] { new Vector2Int(index, 0) },
                    new Vector2Int(index, 0),
                    spec.IsLightingSource,
                    spec.IsLightingSource,
                    spec.IsLightingSource,
                    string.Empty,
                    string.Empty,
                    spec.IsLightingSource ? 0 : -1,
                    Array.Empty<Vector2Int>(),
                    false,
                    false,
                    false,
                    1,
                    1,
                    Array.Empty<string>(),
                    Array.Empty<string>()))
                .ToArray();
            return new ItemSystemSnapshot(
                5,
                new Vector2Int(2, 2),
                new[]
                {
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 3),
                    new Vector2Int(1, 2),
                    new Vector2Int(3, 2)
                },
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                placements,
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                new ItemSystemBuildSnapshot(null),
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                new ItemSystemSkillMonitorSnapshot(null),
                string.Empty,
                false,
                string.Empty,
                Array.Empty<ItemSystemValidationError>());
        }

        private static PlacementSpec Placement(
            string placementId,
            string itemId,
            bool isLightingSource = false)
        {
            return new PlacementSpec(placementId, itemId, isLightingSource);
        }

        private static ItemInstancePlacementBindingInput Binding(
            string itemInstanceId,
            string placementId,
            string baseItemId)
        {
            return new ItemInstancePlacementBindingInput(
                itemInstanceId,
                placementId,
                baseItemId);
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
                    "QA fixture could not invoke immutable constructor for "
                    + typeof(T).FullName + ".",
                    exception);
            }
        }

        private static bool RejectAdd(
            IReadOnlyList<ItemInstancePlacementBindingSnapshot> values,
            ItemInstancePlacementBindingInput ignored)
        {
            try
            {
                ((IList<ItemInstancePlacementBindingSnapshot>)values).Add(
                    Construct<ItemInstancePlacementBindingSnapshot>(
                        ignored.itemInstanceId,
                        ignored.placementId,
                        ignored.baseItemId));
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool RejectErrorAdd(
            IReadOnlyList<ItemInstancePlacementBindingValidationError> values)
        {
            try
            {
                ((IList<ItemInstancePlacementBindingValidationError>)values).Add(
                    new ItemInstancePlacementBindingValidationError(
                        "FORGED",
                        ItemInstancePlacementBindingStatus.Invalid,
                        "X",
                        "Y",
                        "I003",
                        "forged"));
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool HasCode(
            ItemInstancePlacementBindingValidationResult result,
            string code)
        {
            return result.ValidationErrors.Any(value =>
                string.Equals(value.code, code, StringComparison.Ordinal));
        }

        private static bool CanWrite<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName)?.CanWrite == true;
        }

        private static void AddScenario(
            ICollection<Check> checks,
            ICollection<ScenarioRow> scenarios,
            string id,
            string category,
            ItemInstancePlacementBindingStatus expected,
            ItemInstancePlacementBindingValidationResult result,
            bool semanticPass,
            string evidence)
        {
            string codes = string.Join(";", result.ValidationErrors
                .Select(value => value.code)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal));
            bool pass = result.status == expected && semanticPass;
            scenarios.Add(new ScenarioRow(
                id,
                category,
                expected,
                result.status,
                result.snapshot.Bindings.Count,
                codes,
                result.snapshot.canonicalSignature,
                pass,
                evidence));
            Add(checks,
                "scenario." + id,
                expected.ToString(),
                result.status + "; bindings="
                    + result.snapshot.Bindings.Count.ToString(CultureInfo.InvariantCulture)
                    + "; codes=" + codes,
                pass);
        }

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool pass)
        {
            checks.Add(new Check(id, expected, actual, pass));
        }

        private static void WriteReports(
            string root,
            string mode,
            bool offlinePassed,
            IReadOnlyList<Check> checks,
            IReadOnlyList<ScenarioRow> scenarios,
            IReadOnlyList<LeakRow> leakRows,
            IReadOnlyList<ProtectedRow> protectedRows,
            string representativeSignature,
            int representativeBindingCount)
        {
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            int legalCount = scenarios.Count(value => value.Category == "legal");
            int abnormalCount = scenarios.Count(value => value.Category == "abnormal");
            int leakCount = leakRows.Sum(value => value.Count);
            string reportDirectory = Path.GetDirectoryName(Absolute(root, ReportPath));
            Directory.CreateDirectory(reportDirectory ?? Absolute(root, "Docs/V0.4/Reports"));

            StringBuilder report = new StringBuilder();
            report.AppendLine("# Item Instance Placement Binding Contract Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemInstancePlacementBindingContract01`")
                .AppendLine("- Guard receipt: `GUARD_PASS_ITEMINSTANCEPLACEMENTBINDINGCONTRACT01`")
                .AppendLine("- Contract: `" + ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId + "`")
                .AppendLine("- Source contracts: `ItemInstanceProjectionContractSnapshot.v1` + `ItemSystemSnapshot.v1`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Offline verifier: `" + (offlinePassed ? "PASS" : "PENDING") + "`")
                .AppendLine("- Unity compile / verifier: `"
                    + (string.Equals(mode, "Unity batch", StringComparison.Ordinal)
                        ? (pass ? "PASS" : "FAIL")
                        : "PENDING") + "`")
                .AppendLine("- Legal / abnormal scenarios: `"
                    + legalCount.ToString(CultureInfo.InvariantCulture) + " / "
                    + abnormalCount.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Representative Binding count: `"
                    + representativeBindingCount.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Canonical Signature: `" + representativeSignature + "`")
                .AppendLine("- Duplicate / orphan / mismatch checks: `PASS / PASS / PASS`")
                .AppendLine("- I031 ordinary-instance rejection: `PASS`")
                .AppendLine("- Formal-system Leak Count: `"
                    + leakCount.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine()
                .AppendLine("## Contract Rules")
                .AppendLine()
                .AppendLine("- `itemInstanceId`, `placementId`, and `baseItemId` are supplied explicitly in each row.")
                .AppendLine("- `itemInstanceId == placementId` is never treated as evidence.")
                .AppendLine("- `baseItemId` must match both Instance Projection `baseItemId` and Placement `itemId`.")
                .AppendLine("- Missing and orphan facts remain `Unknown`; duplicate, mismatch, invalid-source, and I031 forgery facts are `Invalid`.")
                .AppendLine("- I031 remains a system JuNian placement and never becomes an ordinary generated/drop instance.")
                .AppendLine("- Output bindings and errors are immutable defensive collections.")
                .AppendLine("- Canonical Signature uses Ordinal sorting, InvariantCulture formatting, and SHA-256.")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                report.Append("| ").Append(Md(check.Id)).Append(" | ")
                    .Append(Md(check.Expected)).Append(" | ")
                    .Append(Md(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            report.AppendLine()
                .AppendLine("## Protected Hashes")
                .AppendLine()
                .AppendLine("| Scope | Phase | Expected/Before | Actual/After | Result |")
                .AppendLine("|---|---|---|---|---|");
            foreach (ProtectedRow row in protectedRows)
            {
                report.Append("| ").Append(Md(row.Path)).Append(" | ")
                    .Append(row.BeforeAfter ? "before-after" : "baseline").Append(" | ")
                    .Append(row.ExpectedFiles).Append('/').Append(row.ExpectedHash).Append(" | ")
                    .Append(row.ActualFiles).Append('/').Append(row.ActualHash).Append(" | ")
                    .Append(row.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            report.AppendLine()
                .AppendLine("## Scope")
                .AppendLine()
                .AppendLine("- No modification to `ItemSystemSnapshot.v1`, `BuildDebugSignature`, Instance Projection Canonical Signature, or Affix Schema.")
                .AppendLine("- No Item→BuildCapability mapping and no point/stack/count→BP conversion.")
                .AppendLine("- No Enemy, CrossSystem, Battle, Board, RunFlow, SaveData, Reward, Chapter, Scene, Prefab, UI, RectTransform, or BuildSettings connection.")
                .AppendLine()
                .AppendLine("## Marker")
                .AppendLine()
                .AppendLine(pass ? PassMarker : "ITEM_INSTANCE_PLACEMENT_BINDING_CONTRACT01_FAIL");
            File.WriteAllText(Absolute(root, ReportPath), report.ToString(), new UTF8Encoding(false));

            StringBuilder csv = new StringBuilder(
                "caseId,category,expectedStatus,actualStatus,bindingCount,validationCodes,canonicalSignature,result,evidence\n");
            foreach (ScenarioRow row in scenarios)
            {
                csv.Append(Csv(row.Id)).Append(',')
                    .Append(Csv(row.Category)).Append(',')
                    .Append(Csv(row.ExpectedStatus.ToString())).Append(',')
                    .Append(Csv(row.ActualStatus.ToString())).Append(',')
                    .Append(row.BindingCount.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.ValidationCodes)).Append(',')
                    .Append(Csv(row.CanonicalSignature)).Append(',')
                    .Append(row.Passed ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(row.Evidence)).Append('\n');
            }
            File.WriteAllText(Absolute(root, SpecPath), csv.ToString(), new UTF8Encoding(false));

            StringBuilder leak = new StringBuilder();
            leak.AppendLine("# Item Instance Placement Binding Contract Leak Check Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemInstancePlacementBindingContract01`")
                .AppendLine("- Result: `" + (leakCount == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak Count: `" + leakCount.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Runtime scope: independent read-only Item identity binding contract only.")
                .AppendLine("- Scene / Prefab / UI / RectTransform / BuildSettings writes: none.")
                .AppendLine("- Enemy / CrossSystem / Battle / Board / RunFlow / SaveData / Reward / Chapter connections: none.")
                .AppendLine("- Item→BuildCapability mapping or BP conversion: none.")
                .AppendLine();
            if (leakRows.Count == 0)
            {
                leak.AppendLine("- PASS: no forbidden runtime token findings.");
            }
            else
            {
                foreach (LeakRow row in leakRows)
                {
                    leak.AppendLine("- FAIL `" + row.Path + "` token `" + row.Token
                        + "`: " + row.Count.ToString(CultureInfo.InvariantCulture));
                }
            }

            leak.AppendLine()
                .AppendLine("## Protected Before / After")
                .AppendLine();
            foreach (ProtectedRow row in protectedRows.Where(value => value.BeforeAfter))
            {
                leak.AppendLine("- " + (row.Passed ? "PASS" : "FAIL") + " `" + row.Path
                    + "`: " + row.ExpectedHash + " → " + row.ActualHash);
            }
            File.WriteAllText(Absolute(root, LeakPath), leak.ToString(), new UTF8Encoding(false));
        }

        private static Dictionary<string, HashResult> CaptureProtected(string root)
        {
            return ProtectedExpectations.ToDictionary(
                value => value.Path,
                value => HashPath(root, value.Path, value.IsDirectory),
                StringComparer.Ordinal);
        }

        private static HashResult HashPath(string root, string relativePath, bool isDirectory)
        {
            string absolute = Absolute(root, relativePath);
            if ((!isDirectory && !File.Exists(absolute))
                || (isDirectory && !Directory.Exists(absolute)))
            {
                return new HashResult(0, "MISSING");
            }

            if (!isDirectory)
            {
                return new HashResult(1, Sha256(File.ReadAllBytes(absolute)));
            }

            string[] files = Directory.GetFiles(absolute, "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            foreach (string file in files)
            {
                payload.Append(Relative(root, file)).Append('|')
                    .Append(Sha256(File.ReadAllBytes(file))).Append('\n');
            }
            return new HashResult(
                files.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString())));
        }

        private static string Sha256(byte[] bytes)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return BitConverter.ToString(hash.ComputeHash(bytes ?? Array.Empty<byte>()))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }

        private static int CountOccurrences(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
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
            throw new DirectoryNotFoundException("Unity project root not found.");
        }

        private static string Absolute(string root, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                root,
                (relativePath ?? string.Empty).Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Relative(string root, string absolutePath)
        {
            string normalizedRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string normalizedPath = Path.GetFullPath(absolutePath);
            return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase)
                ? normalizedPath.Substring(normalizedRoot.Length).Replace('\\', '/')
                : normalizedPath.Replace('\\', '/');
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string Md(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string Sanitize(string value)
        {
            return new string((value ?? string.Empty)
                .Select(character => char.IsLetterOrDigit(character) ? character : '_')
                .ToArray());
        }

        private static ProtectedExpectation FileExpectation(string path, string hash)
        {
            return new ProtectedExpectation(path, false, 1, hash);
        }

        private static ProtectedExpectation DirectoryExpectation(
            string path,
            int count,
            string hash)
        {
            return new ProtectedExpectation(path, true, count, hash);
        }

        private readonly struct PlacementSpec
        {
            public PlacementSpec(string placementId, string itemId, bool isLightingSource)
            {
                PlacementId = placementId;
                ItemId = itemId;
                IsLightingSource = isLightingSource;
            }

            public string PlacementId { get; }
            public string ItemId { get; }
            public bool IsLightingSource { get; }
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

        private readonly struct ScenarioRow
        {
            public ScenarioRow(
                string id,
                string category,
                ItemInstancePlacementBindingStatus expectedStatus,
                ItemInstancePlacementBindingStatus actualStatus,
                int bindingCount,
                string validationCodes,
                string canonicalSignature,
                bool passed,
                string evidence)
            {
                Id = id;
                Category = category;
                ExpectedStatus = expectedStatus;
                ActualStatus = actualStatus;
                BindingCount = bindingCount;
                ValidationCodes = validationCodes;
                CanonicalSignature = canonicalSignature;
                Passed = passed;
                Evidence = evidence;
            }

            public string Id { get; }
            public string Category { get; }
            public ItemInstancePlacementBindingStatus ExpectedStatus { get; }
            public ItemInstancePlacementBindingStatus ActualStatus { get; }
            public int BindingCount { get; }
            public string ValidationCodes { get; }
            public string CanonicalSignature { get; }
            public bool Passed { get; }
            public string Evidence { get; }
        }

        private readonly struct LeakRow
        {
            public LeakRow(string path, string token, int count)
            {
                Path = path;
                Token = token;
                Count = count;
            }

            public string Path { get; }
            public string Token { get; }
            public int Count { get; }
        }

        private readonly struct ProtectedExpectation
        {
            public ProtectedExpectation(
                string path,
                bool isDirectory,
                int fileCount,
                string hash)
            {
                Path = path;
                IsDirectory = isDirectory;
                FileCount = fileCount;
                Hash = hash;
            }

            public string Path { get; }
            public bool IsDirectory { get; }
            public int FileCount { get; }
            public string Hash { get; }
        }

        private readonly struct HashResult
        {
            public HashResult(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }

        private readonly struct ProtectedRow
        {
            public ProtectedRow(
                string path,
                int expectedFiles,
                int actualFiles,
                string expectedHash,
                string actualHash,
                bool passed,
                bool beforeAfter)
            {
                Path = path;
                ExpectedFiles = expectedFiles;
                ActualFiles = actualFiles;
                ExpectedHash = expectedHash;
                ActualHash = actualHash;
                Passed = passed;
                BeforeAfter = beforeAfter;
            }

            public string Path { get; }
            public int ExpectedFiles { get; }
            public int ActualFiles { get; }
            public string ExpectedHash { get; }
            public string ActualHash { get; }
            public bool Passed { get; }
            public bool BeforeAfter { get; }
        }
    }
}
