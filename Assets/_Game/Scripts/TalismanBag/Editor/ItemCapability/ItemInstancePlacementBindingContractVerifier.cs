using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        private const string PassMarker =
            "ITEM_INSTANCE_PLACEMENT_BINDING_CONTRACT01_PASS";

#if UNITY_EDITOR
        public static void VerifyMenu()
        {
            Verify(false);
        }
#endif

        public static void VerifyOffline()
        {
            Verify(true);
        }

        public static void VerifyBatch()
        {
            Verify(true);
        }

        public static int Main(string[] args)
        {
            try
            {
                Verify(false);
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static void Verify(bool exitWhenDone)
        {
            List<Check> checks = new List<Check>();
            List<ScenarioRow> scenarios = new List<ScenarioRow>();

            try
            {
                RunScenarios(checks, scenarios);
                RunContractShapeChecks(checks);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

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
            ICollection<ScenarioRow> scenarios)
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
            string representativeSignature = legal.snapshot.canonicalSignature;
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
                category,
                result.status));
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
                string category,
                ItemInstancePlacementBindingStatus actualStatus)
            {
                Category = category;
                ActualStatus = actualStatus;
            }

            public string Category { get; }
            public ItemInstancePlacementBindingStatus ActualStatus { get; }
        }
    }
}
