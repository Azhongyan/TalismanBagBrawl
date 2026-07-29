using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemSystemValidatorAndSnapshotVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotLeakCheckReport.md";

        private static readonly DefaultItemSystemSnapshotProvider Provider = DefaultItemSystemSnapshotProvider.Instance;

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemSystemValidatorAndSnapshot01/[Guard Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<SpecRow> rows = new();

            try
            {
                RunVerification(result, rows);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Unhandled verifier exception: " + exception);
            }

            WriteReports(result, rows);
            bool passed = result.Errors.Count == 0;
            if (passed)
            {
                Debug.Log("ItemSystemValidatorAndSnapshot01 verification passed and reports were written.");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    Debug.LogError(error);
                }
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void RunVerification(VerificationResult result, List<SpecRow> rows)
        {
            ItemSystemSnapshot valid = AddProviderCase(
                result,
                rows,
                "standard-valid-layout",
                "I031 source plus three lit ordinary items, one active AP cell, no explicit main Build",
                true,
                Array.Empty<string>(),
                Input(ValidPlacements()),
                snapshot => snapshot.schemaVersion == ItemSystemSnapshot.CurrentSchemaVersion
                    && snapshot.boardSize == 5
                    && snapshot.eyeCell == new Vector2Int(2, 2)
                    && snapshot.catalogItems.Count == 31
                    && snapshot.placements.Count == 4
                    && snapshot.placements.Count(placement => placement.isLit) == 4
                    && snapshot.placements.Any(placement => placement.isArrayBonusActive)
                    && string.IsNullOrWhiteSpace(snapshot.selectedMainBuildId),
                "Covers schema, board, catalog count, source identity, lighting, array bonus, and no auto main Build.");

            CheckDeterministicGeneration(result, rows);
            CheckInputMutationIsolation(result, rows);
            CheckAdditionalDeterminism(result, rows);

            AddProviderCase(result, rows, "duplicate-placement-id", "Two items share P_DUP.", false, new[] { "PLACEMENT_ID_DUPLICATE" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_DUP", "I001", 1, 1),
                P("P_DUP", "I007", 2, 1)
            }));
            AddProviderCase(result, rows, "empty-placement-id", "One placed item has an empty placementId.", false, new[] { "PLACEMENT_ID_EMPTY" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P(string.Empty, "I001", 1, 1)
            }));
            AddProviderCase(result, rows, "out-of-bounds", "I002 line2_h starts at x=4.", false, new[] { "ITEM_OUT_OF_BOUNDS" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I002", "I002", 4, 0)
            }));
            AddProviderCase(result, rows, "placement-overlap", "I001 and I007 both occupy (1,1).", false, new[] { "PLACEMENT_OVERLAP" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I001", "I001", 1, 1),
                P("P_I007", "I007", 1, 1)
            }));
            AddProviderCase(result, rows, "eye-covered", "I001 is placed on fixed eyeCell (2,2).", false, new[] { "EYE_CELL_COVERED" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I001", "I001", 2, 2)
            }));
            AddProviderCase(result, rows, "array-bonus-coordinates-wrong", "AP04 is moved away from the fixed cross.", false, new[] { "ARRAY_BONUS_CELLS_INVALID" }, Input(
                ValidPlacements(),
                new ItemSystemBoardConfigInput(5, new Vector2Int(2, 2), new[]
                {
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 3),
                    new Vector2Int(1, 2),
                    new Vector2Int(0, 4)
                })));
            AddProviderCase(result, rows, "board-state-missing-i031", "Explicit Board state without its stable I031 placement.", false, new[] { "I031_BOARD_PLACEMENT_MISSING" }, Input(new[]
            {
                P("P_I001", "I001", 1, 1),
                P("P_I007", "I007", 2, 1),
                P("P_I013", "I013", 3, 1)
            }, i031State: I031InventoryPlacementContract.OwnedBoard()));
            AddProviderCase(result, rows, "multiple-i031-placement", "Two I031 source placements.", false, new[] { "I031_PLACEMENT_MULTIPLE" }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_SOURCE_B", "I031", 4, 1),
                P("P_I001", "I001", 1, 1)
            }));
            AddProviderCase(result, rows, "ordinary-item-forged-source", "Catalog clone marks I001 as a direct lighting source.", false, new[] { "ORDINARY_ITEM_LIGHTING_SOURCE_FORGED" }, Input(
                ValidPlacements(),
                catalogItems: CreateCatalogWithOrdinarySource("I001")));
            AddProviderCase(result, rows, "main-build-non-explicit-auto", "selectedMainBuildId is supplied from Auto source.", false, new[] { "MAIN_BUILD_NOT_EXPLICIT" }, Input(
                ValidPlacements(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "Auto", 1)));
            AddProviderCase(result, rows, "qilei-selected-as-main-build", "selectedMainBuildId points to qilei:fu.", false, new[] { "MAIN_BUILD_QILEI_SELECTED", "SKILL_MONITOR_VALIDATION" }, Input(
                ValidPlacements(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("qilei:fu", "ItemSystemValidatorAndSnapshotVerifier", 1)));
            AddProviderCase(result, rows, "skill-monitor-fixed-order", "Provider emits BasicAttack / Build2 / Build4 / Build6 in order.", true, Array.Empty<string>(), Input(
                ValidPlacements(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "ItemSystemValidatorAndSnapshotVerifier", 1)),
                snapshot => HasFixedMonitorSlots(snapshot.skillMonitorSnapshot)
                    && snapshot.skillMonitorSnapshot.Slots.All(slot => !slot.isTriggered && slot.cooldown01 == 0f && slot.charge01 == 0f),
                "Four monitor slots are contract-only and carry no runtime trigger/cooldown/charge state.");
            AddProviderCase(result, rows, "duplicate-base-item-excluded-from-build", "Two unique placements use the same ordinary base item I001.", false, new[] { ItemSystemValidationCodes.DuplicateBaseItemPlaced }, Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I001_A", "I001", 1, 1),
                P("P_I001_B", "I001", 2, 1)
            }),
                snapshot =>
                {
                    ItemSystemBuildTrackSnapshot track = snapshot.buildSnapshot.FindFaMenBuild("famen:zhenlei");
                    return track != null
                        && track.litItemCount == 1
                        && track.SourcePlacementIds.SequenceEqual(new[] { "P_I001_A" })
                        && track.SourceItemIds.SequenceEqual(new[] { "I001" });
                },
                "Duplicate ordinary base identity invalidates the snapshot and the later placement cannot contribute to Build counts.");

            AddValidatorCase(result, rows, "unlit-item-counted-in-build", "Forged snapshot marks an unlit ordinary item counted in Build.", false, new[] { "UNLIT_ITEM_COUNTED_IN_BUILD" }, WithPlacements(valid, new[]
            {
                ForgePlacement(valid.FindPlacement("P_I001"), isLit: false, isCountedInBuild: true)
            }));
            AddValidatorCase(result, rows, "unlit-item-array-bonus-active", "Forged snapshot activates AP bonus on an unlit item.", false, new[] { "UNLIT_ITEM_ARRAY_BONUS_ACTIVE" }, WithPlacements(valid, new[]
            {
                ForgePlacement(valid.FindPlacement("P_I007"), isLit: false, isCountedInBuild: false, isArrayBonusActive: true)
            }));
            AddValidatorCase(result, rows, "unopened-core-effect-active", "Forged snapshot has active core effect without unlocked effect.", false, new[] { "CORE_EFFECT_ACTIVE_WHEN_LOCKED" }, WithPlacements(valid, new[]
            {
                ForgePlacement(valid.FindPlacement("P_I001"), activeCoreEffectIds: new[] { "forged_core" }, unlockedCoreEffectIds: Array.Empty<string>())
            }));
            AddValidatorCase(result, rows, "opened-but-unlit-core-effect-active", "Forged snapshot has unlocked and active core effect while unlit.", false, new[] { "CORE_EFFECT_ACTIVE_WHEN_UNLIT" }, WithPlacements(valid, new[]
            {
                ForgePlacement(valid.FindPlacement("P_I001"), isLit: false, isCountedInBuild: false, activeCoreEffectIds: new[] { "forged_core" }, unlockedCoreEffectIds: new[] { "forged_core" })
            }));
            AddValidatorCase(result, rows, "build-snapshot-auto-selected-main", "Forged Build snapshot carries selectedMainBuildId without explicit input.", false, new[] { "BUILD_AUTO_SELECTED_MAIN" }, WithBuildSnapshot(valid, CreateAutoSelectedBuildSnapshot(valid)));
            AddValidatorCase(result, rows, "skill-monitor-slot-count-order-wrong", "Forged monitor snapshot omits the fixed slot order.", false, new[] { "SKILL_MONITOR_SLOT_ORDER_INVALID" }, WithSkillMonitor(valid, CreateWrongOrderMonitor()));
            AddValidatorCase(result, rows, "skill-monitor-runtime-state-forbidden", "Forged monitor snapshot exposes trigger/cooldown/charge runtime state.", false, new[] { "SKILL_MONITOR_RUNTIME_STATE_FORBIDDEN" }, WithSkillMonitor(valid, CreateRuntimeStateMonitor()));
            CheckExternalMutationBlocks(result, rows, valid);
            CheckForgedSnapshotValidatorBranches(result, rows, valid);

            CheckUnlitProviderSafeguards(result, rows);
            CheckDetailProjectionContracts(result, rows);
            CheckLeakScope(result, rows);

            result.Notes.Add("ItemSystemSnapshot v1 provider returns immutable read-only snapshots for catalog, placement, lighting, array bonus, Build, awakening, and skill monitor facts.");
            result.Notes.Add("Invalid provider input returns stable validation error codes instead of throwing unhandled exceptions.");
            result.Notes.Add("Sandbox preview now reads scenario facts from ItemSystemSnapshot and only projects them back to existing greybox views.");
        }

        private static void CheckDeterministicGeneration(VerificationResult result, List<SpecRow> rows)
        {
            ItemSystemSnapshotInput input = Input(
                ValidPlacements().Reverse().ToArray(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "ItemSystemValidatorAndSnapshotVerifier", 7));
            ItemSystemSnapshot first = Provider.CreateSnapshot(input);
            ItemSystemSnapshot second = Provider.CreateSnapshot(input);
            bool same = string.Equals(first.BuildDebugSignature(), second.BuildDebugSignature(), StringComparison.Ordinal);
            bool immutable = CheckSnapshotCollectionsImmutable(first, out string immutableNote);
            AddManualRow(result, rows, "deterministic-same-input-twice", "Same input generated twice.", true, first.isValid, Array.Empty<string>(), Codes(first.validationErrors), same, immutable, same && immutable, "Snapshot signatures match exactly. " + immutableNote);
        }

        private static void CheckInputMutationIsolation(VerificationResult result, List<SpecRow> rows)
        {
            List<ItemSystemPlacementInput> source = ValidPlacements().ToList();
            ItemSystemSnapshotInput input = Input(source);
            ItemSystemSnapshot snapshot = Provider.CreateSnapshot(input);
            string before = snapshot.BuildDebugSignature();
            source.Clear();
            source.Add(P("P_BAD", "I002", 4, 4));
            ItemSystemSnapshot afterSourceMutation = Provider.CreateSnapshot(input);
            bool isolated = string.Equals(before, snapshot.BuildDebugSignature(), StringComparison.Ordinal)
                && string.Equals(before, afterSourceMutation.BuildDebugSignature(), StringComparison.Ordinal)
                && snapshot.placements.Count == 4;
            bool immutable = CheckSnapshotCollectionsImmutable(snapshot, out string immutableNote);
            AddManualRow(result, rows, "snapshot-immutable-after-input-mutation", "Source placement list mutated after input/snapshot creation.", true, snapshot.isValid, Array.Empty<string>(), Codes(snapshot.validationErrors), true, immutable, isolated && immutable, "Old snapshot and cloned input stay isolated from source list mutation. " + immutableNote);
        }

        private static void CheckAdditionalDeterminism(VerificationResult result, List<SpecRow> rows)
        {
            ItemSystemSnapshot normal = Provider.CreateSnapshot(Input(
                ValidPlacements(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "ItemSystemValidatorAndSnapshotVerifier", 11)));
            ItemSystemSnapshot reversed = Provider.CreateSnapshot(Input(
                ValidPlacements().Reverse().ToArray(),
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "ItemSystemValidatorAndSnapshotVerifier", 11)));
            bool reversedSame = string.Equals(normal.BuildDebugSignature(), reversed.BuildDebugSignature(), StringComparison.Ordinal);
            bool reversedImmutable = CheckSnapshotCollectionsImmutable(normal, out string reversedImmutableNote);
            AddManualRow(result, rows, "deterministic-reversed-input", "Valid placements submitted in forward and reverse order.", true, normal.isValid && reversed.isValid, Array.Empty<string>(), Codes(normal.validationErrors.Concat(reversed.validationErrors).ToArray()), reversedSame, reversedImmutable, reversedSame && reversedImmutable, "Canonical ordering ignores source list order. " + reversedImmutableNote);

            ItemSystemSnapshot invalidA = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_DUP", "I001", 1, 1),
                P("P_DUP", "I007", 1, 1)
            }));
            ItemSystemSnapshot invalidB = Provider.CreateSnapshot(Input(new[]
            {
                P("P_DUP", "I007", 1, 1),
                P("P_DUP", "I001", 1, 1),
                P("P_SYSTEM_I031", "I031", 0, 1)
            }));
            bool invalidSame = string.Equals(invalidA.BuildDebugSignature(), invalidB.BuildDebugSignature(), StringComparison.Ordinal);
            bool invalidImmutable = CheckSnapshotCollectionsImmutable(invalidA, out string invalidImmutableNote);
            string[] invalidCodes = Codes(invalidA.validationErrors.Concat(invalidB.validationErrors).ToArray());
            bool invalidHasExpected = CodesContainAll(Codes(new[] { "PLACEMENT_ID_DUPLICATE", "PLACEMENT_OVERLAP" }), invalidCodes);
            AddManualRow(result, rows, "deterministic-invalid-input", "Invalid duplicate/overlap input submitted in different order.", false, invalidA.isValid || invalidB.isValid, new[] { "PLACEMENT_ID_DUPLICATE", "PLACEMENT_OVERLAP" }, invalidCodes, invalidSame, invalidImmutable, !invalidA.isValid && !invalidB.isValid && invalidSame && invalidImmutable && invalidHasExpected, "Invalid snapshots remain deterministic. " + invalidImmutableNote);

            ItemSystemSnapshot changed = WithLightingResults(normal, normal.lightingResults
                .Select(item => string.Equals(item.placementId, "P_I001", StringComparison.Ordinal)
                    ? ForgeLighting(item, litByItemId: "signature-branch-change")
                    : item)
                .ToArray());
            bool signatureCoversBranch = !string.Equals(normal.BuildDebugSignature(), changed.BuildDebugSignature(), StringComparison.Ordinal);
            bool signatureImmutable = CheckSnapshotCollectionsImmutable(changed, out string signatureImmutableNote);
            AddManualRow(result, rows, "canonical-signature-covers-all-branches", "Child lighting-only mutation changes canonical signature.", false, false, new[] { "LIGHTING_SNAPSHOT_MISMATCH" }, new[] { signatureCoversBranch ? "LIGHTING_SNAPSHOT_MISMATCH" : "SIGNATURE_BRANCH_MISSING" }, signatureCoversBranch, signatureImmutable, signatureCoversBranch && signatureImmutable, "Canonical signature includes child branch state. " + signatureImmutableNote);
        }

        private static void CheckExternalMutationBlocks(VerificationResult result, List<SpecRow> rows, ItemSystemSnapshot valid)
        {
            ItemSystemSnapshot awakened = Provider.CreateSnapshot(Input(
                ValidPlacements(),
                awakeningInputs: new[]
                {
                    A("I001", "P_I001", 40),
                    A("I007", "P_I007", 40)
                },
                mainBuildSelectionInput: new ItemMainBuildSelectionInput("famen:zhenlei", "ItemSystemValidatorAndSnapshotVerifier", 21)));
            ItemSystemSnapshot invalid = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_DUP", "I001", 1, 1),
                P("P_DUP", "I007", 1, 1)
            }));

            bool topLevel = CheckTopLevelCollectionMutations(awakened, invalid, out string topNote);
            AddManualRow(result, rows, "external-top-level-collection-mutation-blocked", "Try array casts and IList mutations against all top-level snapshot collections.", true, topLevel, Array.Empty<string>(), Array.Empty<string>(), true, topLevel, topLevel, topNote);

            bool nested = CheckNestedCollectionMutations(awakened, out string nestedNote);
            AddManualRow(result, rows, "external-nested-collection-mutation-blocked", "Try array casts and IList mutations against nested placement/build/awakening collections.", true, nested, Array.Empty<string>(), Array.Empty<string>(), true, nested, nested, nestedNote);
        }

        private static void CheckForgedSnapshotValidatorBranches(VerificationResult result, List<SpecRow> rows, ItemSystemSnapshot valid)
        {
            ItemSystemPlacementSnapshot p001 = valid.FindPlacement("P_I001");
            ItemSystemPlacementSnapshot p013 = valid.FindPlacement("P_I013");

            AddValidatorCase(result, rows, "forged-placement-overlap", "Public snapshot is forged so P_I013 overlaps P_I001.", false, new[] { "PLACEMENT_OVERLAP" }, WithPlacements(valid, new[]
            {
                ForgePlacement(p013, anchorCell: new Vector2Int(1, 1), occupiedCells: new[] { new Vector2Int(1, 1) }, coreCellWorld: new Vector2Int(1, 1))
            }));

            AddValidatorCase(result, rows, "forged-catalog-duplicate-id", "Public snapshot catalog contains duplicate I001.", false, new[] { "CATALOG_ITEM_ID_DUPLICATE" }, CloneSnapshot(valid, catalogItems: CreateDuplicateCatalogSnapshots()));

            AddValidatorCase(result, rows, "forged-ordinary-lighting-source", "Lighting child result marks I001 as a source.", false, new[] { "ORDINARY_ITEM_LIGHTING_SOURCE_FORGED" }, WithLightingResults(valid, ReplaceLighting(valid, "P_I001", item => ForgeLighting(item, isLightingSource: true, isDirectLit: false, isLit: true, litByItemId: string.Empty, litByPlacementId: string.Empty, litDepth: 0))));

            AddValidatorCase(result, rows, "forged-invalid-rotation", "Public snapshot placement rotation is 45 degrees.", false, new[] { "PLACEMENT_ROTATION_INVALID" }, WithPlacements(valid, new[]
            {
                ForgePlacement(p001, rotation: 45)
            }));

            AddValidatorCase(result, rows, "lighting-result-missing", "Lighting child result for P_I001 is removed.", false, new[] { "SNAPSHOT_RESULT_MISSING" }, WithLightingResults(valid, valid.lightingResults.Where(item => item.placementId != "P_I001").ToArray()));

            AddValidatorCase(result, rows, "lighting-result-orphan", "Lighting child result has stale P_ORPHAN.", false, new[] { "SNAPSHOT_RESULT_ORPHAN" }, WithLightingResults(valid, valid.lightingResults.Concat(new[]
            {
                ForgeLighting(valid.lightingResults.First(), placementId: "P_ORPHAN", itemId: "I001")
            }).ToArray()));

            AddValidatorCase(result, rows, "lighting-itemid-mismatch", "Lighting child result itemId differs from placement itemId.", false, new[] { "SNAPSHOT_ITEM_ID_MISMATCH" }, WithLightingResults(valid, ReplaceLighting(valid, "P_I001", item => ForgeLighting(item, itemId: "I002"))));

            AddValidatorCase(result, rows, "lighting-state-mismatch", "Lighting child result changes lit state only.", false, new[] { "LIGHTING_SNAPSHOT_MISMATCH" }, WithLightingResults(valid, ReplaceLighting(valid, "P_I001", item => ForgeLighting(item, isLit: false))));

            AddValidatorCase(result, rows, "array-result-missing", "Array bonus child result for P_I007 is removed.", false, new[] { "SNAPSHOT_RESULT_MISSING" }, WithArrayResults(valid, valid.arrayBonusResults.Where(item => item.placementId != "P_I007").ToArray()));

            AddValidatorCase(result, rows, "array-state-mismatch", "Array bonus child result removes P_I007 AP occupancy only.", false, new[] { "ARRAY_BONUS_SNAPSHOT_MISMATCH" }, WithArrayResults(valid, ReplaceArray(valid, "P_I007", item => ForgeArray(item, occupiedArrayBonusCells: Array.Empty<Vector2Int>(), occupiedArrayBonusCellIds: Array.Empty<string>()))));

            AddValidatorCase(result, rows, "build-result-missing", "Build child item result for P_I001 is removed.", false, new[] { "SNAPSHOT_RESULT_MISSING" }, WithBuildSnapshot(valid, CreateBuildSnapshot(valid, itemResults: valid.buildSnapshot.ItemResults.Where(item => item.placementId != "P_I001").Select(item => ToBuildItemResult(item)).ToArray())));

            AddValidatorCase(result, rows, "build-unlit-counted-only-in-child-snapshot", "Build child result marks P_I013 unlit but counted.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildSnapshot(valid, itemResults: valid.buildSnapshot.ItemResults.Select(item => item.placementId == "P_I013" ? ToBuildItemResult(item, isLit: false, countedInBuild: true) : ToBuildItemResult(item)).ToArray())));

            AddValidatorCase(result, rows, "build-track-count-mismatch", "Build track litItemCount does not match SourcePlacementIds.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildTrackCountMismatchSnapshot(valid)));

            AddValidatorCase(result, rows, "build-track-wrong-tag-source", "Build track SourcePlacementIds includes a counted source from another faMen tag.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildTrackWrongTagSourceSnapshot(valid)));

            AddValidatorCase(result, rows, "build-track-source-itemids-mismatch", "Build track SourcePlacementIds are correct but SourceItemIds are forged.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildTrackSourceItemIdsMismatchSnapshot(valid)));

            AddValidatorCase(result, rows, "build-track-missing-counted-source", "Build track removes a legal counted source and shrinks litItemCount too.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildTrackMissingCountedSourceSnapshot(valid)));

            AddValidatorCase(result, rows, "build-track-kind-or-buildid-mismatch", "Build track keeps sources but forges buildId.", false, new[] { "BUILD_SNAPSHOT_MISMATCH" }, WithBuildSnapshot(valid, CreateBuildTrackBuildIdMismatchSnapshot(valid)));

            AddValidatorCase(result, rows, "awakening-result-missing", "Awakening child result for P_I001 is removed.", false, new[] { "SNAPSHOT_RESULT_MISSING" }, WithAwakeningResults(valid, valid.awakeningResults.Where(item => item.placementId != "P_I001").ToArray()));

            AddValidatorCase(result, rows, "awakening-active-only-in-child-snapshot", "Awakening child result marks a core effect active while placement aggregate remains inactive.", false, new[] { "AWAKENING_SNAPSHOT_MISMATCH" }, WithAwakeningResults(valid, ReplaceAwakening(valid, "P_I001", ForgeAwakeningActiveOnly)));
        }

        private static void CheckUnlitProviderSafeguards(VerificationResult result, List<SpecRow> rows)
        {
            ItemSystemSnapshot unlitBuild = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I013_UNLIT", "I013", 4, 4)
            }));
            bool buildSafe = unlitBuild.FindPlacement("P_I013_UNLIT")?.isCountedInBuild == false;
            bool buildImmutable = CheckSnapshotCollectionsImmutable(unlitBuild, out string buildImmutableNote);
            AddManualRow(result, rows, "provider-unlit-item-excluded-from-build", "Public provider keeps an unlit item out of Build.", true, unlitBuild.isValid, Array.Empty<string>(), Codes(unlitBuild.validationErrors), true, buildImmutable, buildSafe && buildImmutable, "Bad state is not forgeable through provider input. " + buildImmutableNote);

            ItemSystemSnapshot unlitArray = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 0),
                P("P_I007_AP_UNLIT", "I007", 2, 3)
            }));
            bool arraySafe = unlitArray.FindPlacement("P_I007_AP_UNLIT") is { isOnArrayBonusCell: true, isArrayBonusActive: false };
            bool arrayImmutable = CheckSnapshotCollectionsImmutable(unlitArray, out string arrayImmutableNote);
            AddManualRow(result, rows, "provider-unlit-item-cannot-activate-array", "Unlit item sits on AP02 but cannot activate array bonus.", true, unlitArray.isValid, Array.Empty<string>(), Codes(unlitArray.validationErrors), true, arrayImmutable, arraySafe && arrayImmutable, "Array bonus requires isLit. " + arrayImmutableNote);

            ItemSystemSnapshot lockedCore = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I001_LV1", "I001", 1, 1)
            }));
            bool lockedSafe = lockedCore.FindPlacement("P_I001_LV1")?.ActiveCoreEffectIds.Count == 0;
            bool lockedImmutable = CheckSnapshotCollectionsImmutable(lockedCore, out string lockedImmutableNote);
            AddManualRow(result, rows, "provider-unopened-core-not-active", "Lit Lv1 item has no unlocked core effect and no active effect.", true, lockedCore.isValid, Array.Empty<string>(), Codes(lockedCore.validationErrors), true, lockedImmutable, lockedSafe && lockedImmutable, "Core active requires unlocked and lit. " + lockedImmutableNote);

            ItemSystemSnapshot unlitAwakened = Provider.CreateSnapshot(Input(new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I013_UNLIT_LV40", "I013", 4, 4)
            }, awakeningInputs: new[]
            {
                A("I013", "P_I013_UNLIT_LV40", 40)
            }));
            ItemSystemPlacementSnapshot awakenedPlacement = unlitAwakened.FindPlacement("P_I013_UNLIT_LV40");
            bool awakenedSafe = awakenedPlacement != null
                && awakenedPlacement.UnlockedCoreEffectIds.Count > 0
                && awakenedPlacement.ActiveCoreEffectIds.Count == 0;
            bool awakenedImmutable = CheckSnapshotCollectionsImmutable(unlitAwakened, out string awakenedImmutableNote);
            AddManualRow(result, rows, "provider-opened-but-unlit-core-not-active", "Lv40 item is unlocked but unlit.", true, unlitAwakened.isValid, Array.Empty<string>(), Codes(unlitAwakened.validationErrors), true, awakenedImmutable, awakenedSafe && awakenedImmutable, "Unlocked core effect stays inactive while unlit. " + awakenedImmutableNote);
        }

        private static void CheckDetailProjectionContracts(VerificationResult result, List<SpecRow> rows)
        {
            GameObject providerObject = new("ItemSystemValidatorAndSnapshotVerifierProvider");
            ItemInnerDataCatalogProvider catalogProvider = providerObject.AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemSystemSnapshot snapshot = Provider.CreateSnapshot(Input(new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 1),
                    P("P_I001_LIT", "I001", 1, 1),
                    P("P_I007_UNLIT", "I007", 4, 4)
                }, awakeningInputs: new[]
                {
                    A("I031", "P_SYSTEM_I031", 40),
                    A("I001", "P_I001_LIT", 40),
                    A("I007", "P_I007_UNLIT", 40)
                }));
                ItemLightingResolutionResult lighting = snapshot.ToLightingResolutionResult();
                ItemArrayBonusResolutionResult arrayBonus = snapshot.ToArrayBonusResolutionResult();
                ItemBuildSynergyResolutionResult build = snapshot.ToBuildSynergyResolutionResult();
                ItemCoreAwakeningResolutionResult awakening = snapshot.ToCoreAwakeningResolutionResult();
                ItemSkillMonitorResolutionResult monitor = snapshot.ToSkillMonitorResolutionResult();
                bool snapshotImmutable = CheckSnapshotCollectionsImmutable(snapshot, out string snapshotImmutableNote);
                ItemDetailViewModel i001 = catalogProvider.GetDetailViewModel("I001");
                ItemDetailViewModel i007 = catalogProvider.GetDetailViewModel("I007");
                ItemDetailViewModel i031 = catalogProvider.GetDetailViewModel("I031");

                ItemDetailViewModel stale = Compose(i001, ItemDetailProjectionContextKind.PlacedInstance, "P_STALE", lighting, arrayBonus, build, awakening, monitor);
                bool staleOk = stale != null
                    && string.Equals(stale.placementId, "P_STALE", StringComparison.Ordinal)
                    && !stale.statusFlags.isLit
                    && !stale.statusFlags.countedInBuild;
                AddManualRow(result, rows, "stale-placement-id-no-itemid-fallback", "PlacedInstance query uses stale P_STALE for itemId I001.", true, staleOk, Array.Empty<string>(), Array.Empty<string>(), true, snapshotImmutable, staleOk && snapshotImmutable, "Composer keeps stale placement unavailable and never falls back by itemId. " + snapshotImmutableNote);

                ItemDetailViewModel placedLit = Compose(i001, ItemDetailProjectionContextKind.PlacedInstance, "P_I001_LIT", lighting, arrayBonus, build, awakening, monitor);
                ItemDetailViewModel placedUnlit = Compose(i007, ItemDetailProjectionContextKind.PlacedInstance, "P_I007_UNLIT", lighting, arrayBonus, build, awakening, monitor);
                bool isolatedOk = placedLit?.statusFlags.isLit == true
                    && placedUnlit?.statusFlags.isLit == false
                    && placedLit.statusFlags.countedInBuild
                    && !placedUnlit.statusFlags.countedInBuild;
                AddManualRow(result, rows, "distinct-base-placement-state-isolated", "Legal I001 and I007 placements have different lighting/build state.", true, isolatedOk, Array.Empty<string>(), Array.Empty<string>(), true, snapshotImmutable, isolatedOk && snapshotImmutable, "State remains keyed by placementId for legal distinct-base placements. " + snapshotImmutableNote);

                ItemDetailViewModel catalogPreviewI031 = Compose(i031, ItemDetailProjectionContextKind.CatalogPreview, string.Empty, lighting, arrayBonus, build, awakening, monitor);
                bool catalogI031Ok = catalogPreviewI031 != null
                    && string.IsNullOrWhiteSpace(catalogPreviewI031.placementId)
                    && !catalogPreviewI031.statusFlags.isLit
                    && !catalogPreviewI031.statusFlags.countedInBuild;
                AddManualRow(result, rows, "i031-catalog-preview-no-placement-state", "CatalogPreview for I031 while P_SYSTEM_I031 exists.", true, catalogI031Ok, Array.Empty<string>(), Array.Empty<string>(), true, snapshotImmutable, catalogI031Ok && snapshotImmutable, "Catalog preview ignores placement state. " + snapshotImmutableNote);

                ItemDetailViewModel placedI031 = Compose(i031, ItemDetailProjectionContextKind.PlacedInstance, "P_SYSTEM_I031", lighting, arrayBonus, build, awakening, monitor);
                bool placedI031Ok = placedI031 != null
                    && string.Equals(placedI031.placementId, "P_SYSTEM_I031", StringComparison.Ordinal)
                    && placedI031.statusFlags.isLit
                    && !placedI031.statusFlags.countedInBuild;
                AddManualRow(result, rows, "i031-placed-instance-source-boundary", "PlacedInstance for P_SYSTEM_I031/I031.", true, placedI031Ok, Array.Empty<string>(), Array.Empty<string>(), true, snapshotImmutable, placedI031Ok && snapshotImmutable, "I031 is lit source boundary and never counts in Build. " + snapshotImmutableNote);

                bool all31Ok = snapshot.catalogItems.Count == 31
                    && snapshot.catalogItems.All(item =>
                    {
                        ItemDetailViewModel catalogModel = catalogProvider.GetDetailViewModel(item.itemId);
                        ItemDetailViewModel preview = Compose(catalogModel, ItemDetailProjectionContextKind.CatalogPreview, string.Empty, lighting, arrayBonus, build, awakening, monitor);
                        return preview != null
                            && string.Equals(preview.itemId, item.itemId, StringComparison.Ordinal)
                            && string.IsNullOrWhiteSpace(preview.placementId);
                    });
                AddManualRow(result, rows, "catalog-31-full-snapshot-and-detail-projection", "All 31 catalog items projected as CatalogPreview.", true, all31Ok, Array.Empty<string>(), Codes(snapshot.validationErrors), true, snapshotImmutable, all31Ok && snapshotImmutable, "Full catalog snapshot projects without placement leakage. " + snapshotImmutableNote);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static void CheckLeakScope(VerificationResult result, List<SpecRow> rows)
        {
            string[] runtimeFiles =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs",
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs"
            };
            List<string> leaks = new();
            foreach (string path in runtimeFiles)
            {
                if (!File.Exists(path))
                {
                    leaks.Add(path + ":missing");
                    continue;
                }

                string source = File.ReadAllText(path);
                string[] formalTerms =
                {
                    "BattleContract",
                    "UnifiedBattlePage",
                    "RunFlow",
                    "SaveData",
                    "BattleResolver",
                    "BattleBridge",
                    "BattleAdapter",
                    "BuildSettings",
                    "Reward",
                    "Boss"
                };
                foreach (string term in formalTerms)
                {
                    if (source.Contains(term, StringComparison.Ordinal))
                    {
                        leaks.Add(path + ":" + term);
                    }
                }

                if (path.EndsWith("ItemSystemSnapshot.cs", StringComparison.Ordinal)
                    && source.Contains("ItemSandbox", StringComparison.Ordinal))
                {
                    leaks.Add(path + ":ItemSandbox");
                }
            }

            bool clean = leaks.Count == 0;
            AddManualRow(result, rows, "runtime-leak-check", "Runtime snapshot/provider and sandbox preview avoid forbidden formal dependencies.", true, clean, Array.Empty<string>(), leaks.ToArray(), true, clean, clean, clean ? "clean" : string.Join(" | ", leaks));
            result.LeakFindings.AddRange(leaks);
        }

        private static ItemSystemSnapshot AddProviderCase(
            VerificationResult result,
            List<SpecRow> rows,
            string caseId,
            string inputSummary,
            bool expectedIsValid,
            IReadOnlyList<string> expectedErrorCodes,
            ItemSystemSnapshotInput input,
            Func<ItemSystemSnapshot, bool> extraCheck = null,
            string notes = "")
        {
            ItemSystemSnapshot snapshot = Provider.CreateSnapshot(input);
            ItemSystemSnapshot repeat = Provider.CreateSnapshot(input);
            bool deterministic = string.Equals(snapshot.BuildDebugSignature(), repeat.BuildDebugSignature(), StringComparison.Ordinal);
            bool actualIsValid = snapshot.isValid;
            string[] expected = Codes(expectedErrorCodes);
            string[] actual = Codes(snapshot.validationErrors);
            bool codesOk = CodesContainAll(expected, actual);
            bool extraOk = extraCheck?.Invoke(snapshot) ?? true;
            bool immutable = CheckSnapshotCollectionsImmutable(snapshot, out string immutableNote);
            bool pass = actualIsValid == expectedIsValid && deterministic && immutable && codesOk && extraOk;
            AddManualRow(result, rows, caseId, inputSummary, expectedIsValid, actualIsValid, expected, actual, deterministic, immutable, pass, JoinNotes(notes, immutableNote));
            return snapshot;
        }

        private static void AddValidatorCase(
            VerificationResult result,
            List<SpecRow> rows,
            string caseId,
            string inputSummary,
            bool expectedIsValid,
            IReadOnlyList<string> expectedErrorCodes,
            ItemSystemSnapshot snapshot)
        {
            ItemSystemValidator validator = new();
            IReadOnlyList<ItemSystemValidationError> errors = validator.ValidateSnapshot(snapshot);
            bool actualIsValid = errors.Count == 0;
            string[] expected = Codes(expectedErrorCodes);
            string[] actual = Codes(errors);
            bool immutable = CheckSnapshotCollectionsImmutable(snapshot, out string immutableNote);
            bool pass = actualIsValid == expectedIsValid && immutable && CodesContainAll(expected, actual);
            AddManualRow(result, rows, caseId, inputSummary, expectedIsValid, actualIsValid, expected, actual, true, immutable, pass, "Standalone validator catches forged snapshot state. " + immutableNote);
        }

        private static void AddManualRow(
            VerificationResult result,
            List<SpecRow> rows,
            string caseId,
            string inputSummary,
            bool expectedIsValid,
            bool actualIsValid,
            IReadOnlyList<string> expectedErrorCodes,
            IReadOnlyList<string> actualErrorCodes,
            bool deterministic,
            bool immutable,
            bool pass,
            string notes)
        {
            SpecRow row = new()
            {
                caseId = caseId,
                inputSummary = inputSummary,
                expectedIsValid = expectedIsValid,
                actualIsValid = actualIsValid,
                expectedErrorCodes = FormatCodes(expectedErrorCodes),
                actualErrorCodes = FormatCodes(actualErrorCodes),
                snapshotDeterministic = deterministic,
                immutable = immutable,
                result = pass ? "PASS" : "FAIL",
                notes = notes ?? string.Empty
            };
            rows.Add(row);
            if (!pass)
            {
                result.Errors.Add($"{caseId} failed. expectedValid={expectedIsValid}, actualValid={actualIsValid}, expectedCodes={row.expectedErrorCodes}, actualCodes={row.actualErrorCodes}, notes={row.notes}");
            }
        }

        private static ItemSystemSnapshotInput Input(
            IReadOnlyList<ItemSystemPlacementInput> placements,
            ItemSystemBoardConfigInput boardConfig = null,
            IReadOnlyList<ItemCoreAwakeningInput> awakeningInputs = null,
            ItemMainBuildSelectionInput mainBuildSelectionInput = null,
            IReadOnlyList<ItemInnerDataDefinition> catalogItems = null,
            I031InventoryPlacementStateInput i031State = null)
        {
            I031InventoryPlacementStateInput resolvedState = i031State ??
                ((placements ?? Array.Empty<ItemSystemPlacementInput>()).Any(
                    value => value != null && string.Equals(
                        value.itemId,
                        I031InventoryPlacementContract.ItemId,
                        StringComparison.Ordinal))
                    ? I031InventoryPlacementContract.OwnedBoard()
                    : I031InventoryPlacementContract.OwnedInventory());
            return new ItemSystemSnapshotInput(
                placements,
                boardConfig,
                awakeningInputs,
                mainBuildSelectionInput,
                catalogItems,
                new[] { resolvedState });
        }

        private static ItemSystemPlacementInput[] ValidPlacements()
        {
            return new[]
            {
                P("P_SYSTEM_I031", "I031", 0, 1),
                P("P_I001", "I001", 1, 1),
                P("P_I007", "I007", 2, 1),
                P("P_I013", "I013", 3, 1)
            };
        }

        private static ItemSystemPlacementInput P(string placementId, string itemId, int x, int y, int rotation = 0)
        {
            return new ItemSystemPlacementInput(placementId, itemId, new Vector2Int(x, y), rotation);
        }

        private static ItemCoreAwakeningInput A(string itemId, string placementId, int level)
        {
            return new ItemCoreAwakeningInput(itemId, placementId, level, false, "ItemSystemValidatorAndSnapshotVerifier");
        }

        private static IReadOnlyList<ItemInnerDataDefinition> CreateCatalogWithOrdinarySource(string itemId)
        {
            return ItemInnerDataCatalog.AllItems
                .Select(item =>
                {
                    ItemInnerDataDefinition clone = CloneCatalogItem(item);
                    if (string.Equals(clone.itemId, itemId, StringComparison.Ordinal))
                    {
                        clone.isLightingSource = true;
                    }

                    return clone;
                })
                .ToArray();
        }

        private static ItemInnerDataDefinition CloneCatalogItem(ItemInnerDataDefinition item)
        {
            return new ItemInnerDataDefinition
            {
                itemId = item.itemId,
                displayName = item.displayName,
                itemFamily = item.itemFamily,
                faMenTag = item.faMenTag,
                qiLeiTag = item.qiLeiTag,
                shapeId = item.shapeId,
                defaultLocalCells = new List<Vector2Int>(item.defaultLocalCells ?? new List<Vector2Int>()),
                coreCellLocal = item.coreCellLocal,
                displayRarityName = item.displayRarityName,
                rarityDefault = item.rarityDefault,
                allowedRarities = new List<ItemCatalogRarity>(item.allowedRarities ?? new List<ItemCatalogRarity>()),
                basePowerText = item.basePowerText,
                itemPower = item.itemPower,
                primaryStats = (item.primaryStats ?? new List<ItemInnerStatLine>())
                    .Select(stat => new ItemInnerStatLine(stat.label, stat.value, stat.hint))
                    .ToList(),
                triggerText = item.triggerText,
                basicEffectText = item.basicEffectText,
                coreEffectPreviewText = item.coreEffectPreviewText,
                awakeningPreview = item.awakeningPreview,
                fixedAffixPreview = item.fixedAffixPreview,
                randomAffixPreview = item.randomAffixPreview,
                orangeAffixPreview = item.orangeAffixPreview,
                placementHint = item.placementHint,
                flavorText = item.flavorText,
                iconPlaceholderKey = item.iconPlaceholderKey,
                isLightingSource = item.isLightingSource
            };
        }

        private static ItemSystemSnapshot WithPlacements(ItemSystemSnapshot source, IReadOnlyList<ItemSystemPlacementSnapshot> replacements)
        {
            Dictionary<string, ItemSystemPlacementSnapshot> byId = (replacements ?? Array.Empty<ItemSystemPlacementSnapshot>())
                .Where(placement => placement != null)
                .ToDictionary(placement => placement.placementId, placement => placement, StringComparer.Ordinal);
            ItemSystemPlacementSnapshot[] placements = source.placements
                .Select(placement => byId.TryGetValue(placement.placementId, out ItemSystemPlacementSnapshot replacement) ? replacement : placement)
                .ToArray();
            return CloneSnapshot(source, placements: placements);
        }

        private static ItemSystemSnapshot WithBuildSnapshot(ItemSystemSnapshot source, ItemSystemBuildSnapshot buildSnapshot)
        {
            return CloneSnapshot(source, buildSnapshot: buildSnapshot);
        }

        private static ItemSystemSnapshot WithSkillMonitor(ItemSystemSnapshot source, ItemSystemSkillMonitorSnapshot skillMonitor)
        {
            return CloneSnapshot(source, skillMonitorSnapshot: skillMonitor);
        }

        private static ItemSystemSnapshot WithLightingResults(ItemSystemSnapshot source, IReadOnlyList<ItemSystemLightingResultSnapshot> lightingResults)
        {
            return CloneSnapshot(source, lightingResults: lightingResults);
        }

        private static ItemSystemSnapshot WithArrayResults(ItemSystemSnapshot source, IReadOnlyList<ItemSystemArrayBonusResultSnapshot> arrayResults)
        {
            return CloneSnapshot(source, arrayBonusResults: arrayResults);
        }

        private static ItemSystemSnapshot WithAwakeningResults(ItemSystemSnapshot source, IReadOnlyList<ItemSystemAwakeningResultSnapshot> awakeningResults)
        {
            return CloneSnapshot(source, awakeningResults: awakeningResults);
        }

        private static ItemSystemSnapshot CloneSnapshot(
            ItemSystemSnapshot source,
            IReadOnlyList<ItemSystemCatalogItemSnapshot> catalogItems = null,
            IReadOnlyList<ItemSystemPlacementSnapshot> placements = null,
            IReadOnlyList<Vector2Int> litRangeCells = null,
            IReadOnlyList<ItemSystemLightingResultSnapshot> lightingResults = null,
            IReadOnlyList<ItemSystemArrayBonusResultSnapshot> arrayBonusResults = null,
            ItemSystemBuildSnapshot buildSnapshot = null,
            IReadOnlyList<ItemSystemAwakeningResultSnapshot> awakeningResults = null,
            ItemSystemSkillMonitorSnapshot skillMonitorSnapshot = null,
            IReadOnlyList<ItemSystemValidationError> validationErrors = null)
        {
            return new ItemSystemSnapshot(
                source.boardSize,
                source.eyeCell,
                source.ArrayBonusCells,
                catalogItems ?? source.catalogItems,
                placements ?? source.placements,
                litRangeCells ?? source.LitRangeCells,
                lightingResults ?? source.lightingResults,
                arrayBonusResults ?? source.arrayBonusResults,
                buildSnapshot ?? source.buildSnapshot,
                awakeningResults ?? source.awakeningResults,
                skillMonitorSnapshot ?? source.skillMonitorSnapshot,
                source.selectedMainBuildId,
                source.selectedMainBuildIsExplicit,
                source.selectedMainBuildSource,
                validationErrors ?? Array.Empty<ItemSystemValidationError>(),
                source.i031State);
        }

        private static ItemSystemPlacementSnapshot ForgePlacement(
            ItemSystemPlacementSnapshot source,
            Vector2Int? anchorCell = null,
            int? rotation = null,
            IReadOnlyList<Vector2Int> occupiedCells = null,
            Vector2Int? coreCellWorld = null,
            bool? isLightingSource = null,
            bool? isDirectLit = null,
            bool? isLit = null,
            string litByItemId = null,
            string litByPlacementId = null,
            int? litDepth = null,
            IReadOnlyList<Vector2Int> occupiedArrayBonusCells = null,
            bool? isOnArrayBonusCell = null,
            bool? isCountedInBuild = null,
            bool? isArrayBonusActive = null,
            IReadOnlyList<string> unlockedCoreEffectIds = null,
            IReadOnlyList<string> activeCoreEffectIds = null)
        {
            return new ItemSystemPlacementSnapshot(
                source.placementId,
                source.itemId,
                source.displayName,
                anchorCell ?? source.anchorCell,
                rotation ?? source.rotation,
                occupiedCells ?? source.OccupiedCells,
                coreCellWorld ?? source.coreCellWorld,
                isLightingSource ?? source.isLightingSource,
                isDirectLit ?? source.isDirectLit,
                isLit ?? source.isLit,
                litByItemId ?? source.litByItemId,
                litByPlacementId ?? source.litByPlacementId,
                litDepth ?? source.litDepth,
                occupiedArrayBonusCells ?? source.OccupiedArrayBonusCells,
                isOnArrayBonusCell ?? source.isOnArrayBonusCell,
                isArrayBonusActive ?? source.isArrayBonusActive,
                isCountedInBuild ?? source.isCountedInBuild,
                source.inputLevel,
                source.resolvedLevel,
                unlockedCoreEffectIds ?? source.UnlockedCoreEffectIds,
                activeCoreEffectIds ?? source.ActiveCoreEffectIds);
        }

        private static ItemSystemLightingResultSnapshot[] ReplaceLighting(
            ItemSystemSnapshot source,
            string placementId,
            Func<ItemSystemLightingResultSnapshot, ItemSystemLightingResultSnapshot> replacement)
        {
            return source.lightingResults
                .Select(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal) ? replacement(item) : item)
                .ToArray();
        }

        private static ItemSystemArrayBonusResultSnapshot[] ReplaceArray(
            ItemSystemSnapshot source,
            string placementId,
            Func<ItemSystemArrayBonusResultSnapshot, ItemSystemArrayBonusResultSnapshot> replacement)
        {
            return source.arrayBonusResults
                .Select(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal) ? replacement(item) : item)
                .ToArray();
        }

        private static ItemSystemAwakeningResultSnapshot[] ReplaceAwakening(
            ItemSystemSnapshot source,
            string placementId,
            Func<ItemSystemAwakeningResultSnapshot, ItemSystemAwakeningResultSnapshot> replacement)
        {
            return source.awakeningResults
                .Select(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal) ? replacement(item) : item)
                .ToArray();
        }

        private static ItemSystemLightingResultSnapshot ForgeLighting(
            ItemSystemLightingResultSnapshot source,
            string placementId = null,
            string itemId = null,
            IReadOnlyList<Vector2Int> occupiedCells = null,
            Vector2Int? coreCellWorld = null,
            bool? isLightingSource = null,
            bool? isDirectLit = null,
            bool? isLit = null,
            string litByItemId = null,
            string litByPlacementId = null,
            int? litDepth = null)
        {
            return new ItemSystemLightingResultSnapshot(new ItemLightingItemResult(
                itemId ?? source.itemId,
                source.displayName,
                occupiedCells ?? source.OccupiedCells,
                coreCellWorld ?? source.coreCellWorld,
                isLightingSource ?? source.isLightingSource,
                isDirectLit ?? source.isDirectLit,
                isLit ?? source.isLit,
                litByItemId ?? source.litByItemId,
                litDepth ?? source.litDepth,
                placementId ?? source.placementId,
                litByPlacementId ?? source.litByPlacementId));
        }

        private static ItemSystemArrayBonusResultSnapshot ForgeArray(
            ItemSystemArrayBonusResultSnapshot source,
            IReadOnlyList<Vector2Int> occupiedCells = null,
            IReadOnlyList<Vector2Int> occupiedArrayBonusCells = null,
            IReadOnlyList<string> occupiedArrayBonusCellIds = null,
            bool? isLit = null,
            bool? isLightingSource = null)
        {
            return new ItemSystemArrayBonusResultSnapshot(new ItemArrayBonusItemResult(
                source.itemId,
                source.placementId,
                source.displayName,
                occupiedCells ?? source.OccupiedCells,
                occupiedArrayBonusCells ?? source.OccupiedArrayBonusCells,
                occupiedArrayBonusCellIds ?? source.OccupiedArrayBonusCellIds,
                isLit ?? source.isLit,
                isLightingSource ?? source.isLightingSource));
        }

        private static ItemSystemCatalogItemSnapshot[] CreateDuplicateCatalogSnapshots()
        {
            List<ItemInnerDataDefinition> items = ItemInnerDataCatalog.AllItems
                .Select(CloneCatalogItem)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();
            ItemInnerDataDefinition replacement = CloneCatalogItem(items.First(item => item.itemId == "I001"));
            int index = items.FindIndex(item => item.itemId == "I002");
            items[index] = replacement;
            return items.Select(item => new ItemSystemCatalogItemSnapshot(item)).ToArray();
        }

        private static ItemSystemBuildSnapshot CreateBuildSnapshot(
            ItemSystemSnapshot source,
            IReadOnlyList<ItemBuildSynergyItemResult> itemResults = null,
            IReadOnlyList<ItemBuildTrackResult> faMenTracks = null,
            IReadOnlyList<ItemBuildTrackResult> qiLeiTracks = null)
        {
            return new ItemSystemBuildSnapshot(new ItemBuildSynergyResolutionResult(
                faMenTracks ?? source.buildSnapshot.FaMenBuilds.Select(track => ToBuildTrackResult(track)).ToArray(),
                qiLeiTracks ?? source.buildSnapshot.QiLeiBuilds.Select(track => ToBuildTrackResult(track)).ToArray(),
                itemResults ?? source.buildSnapshot.ItemResults.Select(item => ToBuildItemResult(item)).ToArray(),
                source.buildSnapshot.ValidationErrors));
        }

        private static ItemSystemBuildSnapshot CreateBuildTrackCountMismatchSnapshot(ItemSystemSnapshot source)
        {
            ItemBuildTrackResult[] faMenTracks = source.buildSnapshot.FaMenBuilds
                .Select((track, index) => ToBuildTrackResult(track, litItemCount: index == 0 ? track.litItemCount + 1 : track.litItemCount))
                .ToArray();
            return CreateBuildSnapshot(source, faMenTracks: faMenTracks);
        }

        private static ItemSystemBuildSnapshot CreateBuildTrackWrongTagSourceSnapshot(ItemSystemSnapshot source)
        {
            ItemSystemBuildTrackSnapshot target = RequireNonEmptyFaMenTrack(source);
            ItemSystemBuildItemSnapshot wrongSource = source.buildSnapshot.ItemResults
                .Where(item => item.countedInBuild && !string.Equals(item.faMenBuildId, target.buildId, StringComparison.Ordinal))
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .First();
            string[] placementIds = target.SourcePlacementIds.Concat(new[] { wrongSource.placementId }).ToArray();
            ItemBuildTrackResult replacement = ToBuildTrackResult(
                target,
                litItemCount: placementIds.Length,
                sourcePlacementIds: placementIds,
                sourceItemIds: SourceItemIdsForPlacements(source, placementIds));
            return ReplaceFaMenTrack(source, target.buildId, replacement);
        }

        private static ItemSystemBuildSnapshot CreateBuildTrackSourceItemIdsMismatchSnapshot(ItemSystemSnapshot source)
        {
            ItemSystemBuildTrackSnapshot target = RequireNonEmptyFaMenTrack(source);
            string[] forgedItemIds = SourceItemIdsForPlacements(source, target.SourcePlacementIds);
            forgedItemIds[0] = "I999";
            ItemBuildTrackResult replacement = ToBuildTrackResult(
                target,
                sourcePlacementIds: target.SourcePlacementIds,
                sourceItemIds: forgedItemIds);
            return ReplaceFaMenTrack(source, target.buildId, replacement);
        }

        private static ItemSystemBuildSnapshot CreateBuildTrackMissingCountedSourceSnapshot(ItemSystemSnapshot source)
        {
            ItemSystemBuildTrackSnapshot target = RequireNonEmptyFaMenTrack(source);
            string[] reducedPlacementIds = target.SourcePlacementIds.Skip(1).ToArray();
            ItemBuildTrackResult replacement = ToBuildTrackResult(
                target,
                litItemCount: reducedPlacementIds.Length,
                sourcePlacementIds: reducedPlacementIds,
                sourceItemIds: SourceItemIdsForPlacements(source, reducedPlacementIds));
            return ReplaceFaMenTrack(source, target.buildId, replacement);
        }

        private static ItemSystemBuildSnapshot CreateBuildTrackBuildIdMismatchSnapshot(ItemSystemSnapshot source)
        {
            ItemSystemBuildTrackSnapshot target = RequireNonEmptyFaMenTrack(source);
            ItemBuildTrackResult replacement = ToBuildTrackResult(target, buildId: target.buildId + ":forged");
            return ReplaceFaMenTrack(source, target.buildId, replacement);
        }

        private static ItemSystemBuildSnapshot ReplaceFaMenTrack(
            ItemSystemSnapshot source,
            string targetBuildId,
            ItemBuildTrackResult replacement)
        {
            ItemBuildTrackResult[] faMenTracks = source.buildSnapshot.FaMenBuilds
                .Select(track => string.Equals(track.buildId, targetBuildId, StringComparison.Ordinal)
                    ? replacement
                    : ToBuildTrackResult(track))
                .ToArray();
            return CreateBuildSnapshot(source, faMenTracks: faMenTracks);
        }

        private static ItemSystemBuildTrackSnapshot RequireNonEmptyFaMenTrack(ItemSystemSnapshot source)
        {
            return source.buildSnapshot.FaMenBuilds
                .Where(track => track.SourcePlacementIds.Count > 0)
                .OrderBy(track => track.buildId, StringComparer.Ordinal)
                .First();
        }

        private static string[] SourceItemIdsForPlacements(ItemSystemSnapshot source, IReadOnlyList<string> placementIds)
        {
            Dictionary<string, string> itemIdByPlacement = source.buildSnapshot.ItemResults
                .Where(item => !string.IsNullOrWhiteSpace(item.placementId))
                .GroupBy(item => item.placementId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First().itemId, StringComparer.Ordinal);
            return (placementIds ?? Array.Empty<string>())
                .Select(placementId => itemIdByPlacement.TryGetValue(placementId, out string itemId) ? itemId : string.Empty)
                .ToArray();
        }

        private static ItemBuildTrackResult ToBuildTrackResult(
            ItemSystemBuildTrackSnapshot track,
            int? litItemCount = null,
            IReadOnlyList<string> sourcePlacementIds = null,
            IReadOnlyList<string> sourceItemIds = null,
            ItemBuildTrackKind? trackKind = null,
            string buildId = null,
            string stableTag = null)
        {
            return new ItemBuildTrackResult(
                buildId ?? track.buildId,
                trackKind ?? track.trackKind,
                stableTag ?? track.stableTag,
                track.displayName,
                litItemCount ?? track.litItemCount,
                track.maxPieceCount,
                sourcePlacementIds ?? track.SourcePlacementIds,
                sourceItemIds ?? track.SourceItemIds);
        }

        private static ItemBuildSynergyItemResult ToBuildItemResult(
            ItemSystemBuildItemSnapshot item,
            bool? isLit = null,
            bool? countedInBuild = null)
        {
            return new ItemBuildSynergyItemResult
            {
                itemId = item.itemId,
                placementId = item.placementId,
                isLit = isLit ?? item.isLit,
                isLightingSource = item.isLightingSource,
                countedInBuild = countedInBuild ?? item.countedInBuild,
                faMenTag = item.faMenTag,
                qiLeiTag = item.qiLeiTag,
                faMenBuildId = item.faMenBuildId,
                qiLeiBuildId = item.qiLeiBuildId,
                faMenBuildCount = item.faMenBuildCount,
                qiLeiBuildCount = item.qiLeiBuildCount,
                faMenActiveStagePieceCount = item.faMenActiveStagePieceCount,
                qiLeiActiveStagePieceCount = item.qiLeiActiveStagePieceCount
            };
        }

        private static ItemSystemAwakeningResultSnapshot ForgeAwakeningActiveOnly(ItemSystemAwakeningResultSnapshot source)
        {
            ItemCoreAwakeningNodeState[] nodes = source.NodeStates.Count > 0
                ? source.NodeStates
                    .Select((node, index) => ToAwakeningNodeState(node, isUnlocked: index == 0 ? true : node.isUnlocked, isActive: index == 0 ? true : node.isActive))
                    .ToArray()
                : new[]
                {
                    new ItemCoreAwakeningNodeState(
                        source.itemId,
                        "forged_core_active",
                        ItemCoreAwakeningNodeKind.Core1,
                        10,
                        true,
                        true,
                        ItemCoreAwakeningStateKey.Active,
                        ItemCoreAwakeningBlockedReason.None,
                        "Preview Reserved",
                        source.requiredRarityKey)
                };
            return new ItemSystemAwakeningResultSnapshot(new ItemCoreAwakeningItemResult(
                source.itemId,
                source.placementId,
                source.inputLevel,
                source.resolvedLevel,
                source.highRarityUltimatePreview,
                source.requiredRarityKey,
                source.isLit,
                source.isLightingSource,
                source.supportsAwakening,
                source.basicEffectActive,
                nodes,
                source.ValidationErrors,
                source.inputSource));
        }

        private static ItemCoreAwakeningNodeState ToAwakeningNodeState(
            ItemSystemAwakeningNodeSnapshot node,
            bool? isUnlocked = null,
            bool? isActive = null)
        {
            return new ItemCoreAwakeningNodeState(
                node.itemId,
                node.coreEffectId,
                node.nodeKind,
                node.unlockLevel,
                isUnlocked ?? node.isUnlocked,
                isActive ?? node.isActive,
                ParseEnum(node.stateKey, ItemCoreAwakeningStateKey.Locked),
                ParseEnum(node.blockedReason, ItemCoreAwakeningBlockedReason.None),
                node.previewText,
                node.requiredRarityKey);
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, out T parsed) ? parsed : fallback;
        }

        private static bool CheckSnapshotCollectionsImmutable(ItemSystemSnapshot snapshot, out string note)
        {
            List<string> failures = new();
            string before = snapshot.BuildDebugSignature();
            ProbeTopLevelCollections(snapshot, failures);
            ProbeNestedCollections(snapshot, failures);
            string after = snapshot.BuildDebugSignature();
            if (!string.Equals(before, after, StringComparison.Ordinal))
            {
                failures.Add("signature changed after mutation attempts");
            }

            note = failures.Count == 0 ? "readonly mutation probes passed" : string.Join(" | ", failures);
            return failures.Count == 0;
        }

        private static bool CheckTopLevelCollectionMutations(ItemSystemSnapshot valid, ItemSystemSnapshot invalid, out string note)
        {
            List<string> failures = new();
            string validBefore = valid.BuildDebugSignature();
            string invalidBefore = invalid.BuildDebugSignature();
            ProbeTopLevelCollections(valid, failures);
            ProbeCollection("ValidationErrors", invalid.validationErrors, failures);
            if (!string.Equals(validBefore, valid.BuildDebugSignature(), StringComparison.Ordinal))
            {
                failures.Add("valid signature changed after top-level mutation attempts");
            }

            if (!string.Equals(invalidBefore, invalid.BuildDebugSignature(), StringComparison.Ordinal))
            {
                failures.Add("invalid signature changed after validationErrors mutation attempts");
            }

            note = failures.Count == 0 ? "top-level collections reject array casts and IList mutations" : string.Join(" | ", failures);
            return failures.Count == 0;
        }

        private static bool CheckNestedCollectionMutations(ItemSystemSnapshot snapshot, out string note)
        {
            List<string> failures = new();
            string before = snapshot.BuildDebugSignature();
            ProbeNestedCollections(snapshot, failures);
            if (!string.Equals(before, snapshot.BuildDebugSignature(), StringComparison.Ordinal))
            {
                failures.Add("signature changed after nested mutation attempts");
            }

            note = failures.Count == 0 ? "nested collections reject array casts and IList mutations" : string.Join(" | ", failures);
            return failures.Count == 0;
        }

        private static void ProbeTopLevelCollections(ItemSystemSnapshot snapshot, List<string> failures)
        {
            ProbeCollection("CatalogItems", snapshot.catalogItems, failures);
            ProbeCollection("Placements", snapshot.placements, failures);
            ProbeCollection("ArrayBonusCells", snapshot.ArrayBonusCells, failures);
            ProbeCollection("LitRangeCells", snapshot.LitRangeCells, failures);
            ProbeCollection("LightingResults", snapshot.lightingResults, failures);
            ProbeCollection("ArrayBonusResults", snapshot.arrayBonusResults, failures);
            ProbeCollection("AwakeningResults", snapshot.awakeningResults, failures);
            ProbeCollection("Build.FaMenBuilds", snapshot.buildSnapshot.FaMenBuilds, failures);
            ProbeCollection("Build.QiLeiBuilds", snapshot.buildSnapshot.QiLeiBuilds, failures);
            ProbeCollection("Build.ItemResults", snapshot.buildSnapshot.ItemResults, failures);
            ProbeCollection("Build.ValidationErrors", snapshot.buildSnapshot.ValidationErrors, failures);
            ProbeCollection("SkillMonitor.Slots", snapshot.skillMonitorSnapshot.Slots, failures);
            ProbeCollection("SkillMonitor.ValidationErrors", snapshot.skillMonitorSnapshot.ValidationErrors, failures);
            ProbeCollection("ValidationErrors", snapshot.validationErrors, failures);
        }

        private static void ProbeNestedCollections(ItemSystemSnapshot snapshot, List<string> failures)
        {
            ItemSystemPlacementSnapshot placement = snapshot.placements.FirstOrDefault();
            if (placement != null)
            {
                ProbeCollection("Placement.OccupiedCells", placement.OccupiedCells, failures);
                ProbeCollection("Placement.OccupiedArrayBonusCells", snapshot.placements.FirstOrDefault(item => item.OccupiedArrayBonusCells.Count > 0)?.OccupiedArrayBonusCells ?? placement.OccupiedArrayBonusCells, failures);
                ProbeCollection("Placement.UnlockedCoreEffectIds", snapshot.placements.FirstOrDefault(item => item.UnlockedCoreEffectIds.Count > 0)?.UnlockedCoreEffectIds ?? placement.UnlockedCoreEffectIds, failures);
                ProbeCollection("Placement.ActiveCoreEffectIds", snapshot.placements.FirstOrDefault(item => item.ActiveCoreEffectIds.Count > 0)?.ActiveCoreEffectIds ?? placement.ActiveCoreEffectIds, failures);
            }

            ItemSystemLightingResultSnapshot lighting = snapshot.lightingResults.FirstOrDefault();
            if (lighting != null)
            {
                ProbeCollection("LightingResult.OccupiedCells", lighting.OccupiedCells, failures);
            }

            ItemSystemArrayBonusResultSnapshot array = snapshot.arrayBonusResults.FirstOrDefault(item => item.OccupiedArrayBonusCells.Count > 0)
                ?? snapshot.arrayBonusResults.FirstOrDefault();
            if (array != null)
            {
                ProbeCollection("ArrayResult.OccupiedCells", array.OccupiedCells, failures);
                ProbeCollection("ArrayResult.OccupiedArrayBonusCells", array.OccupiedArrayBonusCells, failures);
                ProbeCollection("ArrayResult.OccupiedArrayBonusCellIds", array.OccupiedArrayBonusCellIds, failures);
            }

            ItemSystemBuildTrackSnapshot buildTrack = snapshot.buildSnapshot.FaMenBuilds.FirstOrDefault(track => track.SourcePlacementIds.Count > 0)
                ?? snapshot.buildSnapshot.QiLeiBuilds.FirstOrDefault(track => track.SourcePlacementIds.Count > 0)
                ?? snapshot.buildSnapshot.FaMenBuilds.FirstOrDefault()
                ?? snapshot.buildSnapshot.QiLeiBuilds.FirstOrDefault();
            if (buildTrack != null)
            {
                ProbeCollection("BuildTrack.SourcePlacementIds", buildTrack.SourcePlacementIds, failures);
                ProbeCollection("BuildTrack.SourceItemIds", buildTrack.SourceItemIds, failures);
            }

            ItemSystemAwakeningResultSnapshot awakening = snapshot.awakeningResults.FirstOrDefault(item => item.NodeStates.Count > 0)
                ?? snapshot.awakeningResults.FirstOrDefault();
            if (awakening != null)
            {
                ProbeCollection("Awakening.NodeStates", awakening.NodeStates, failures);
                ProbeCollection("Awakening.ValidationErrors", awakening.ValidationErrors, failures);
                ProbeCollection("Awakening.UnlockedCoreEffectIds", awakening.UnlockedCoreEffectIds, failures);
                ProbeCollection("Awakening.ActiveCoreEffectIds", awakening.ActiveCoreEffectIds, failures);
            }
        }

        private static void ProbeCollection<T>(string label, IReadOnlyList<T> collection, List<string> failures)
        {
            if (collection == null)
            {
                failures.Add(label + ":null");
                return;
            }

            if (collection is T[])
            {
                failures.Add(label + ":cast-to-array-succeeded");
            }

            if (collection is not IList<T> list)
            {
                return;
            }

            T sample = collection.Count > 0 ? collection[0] : default;
            if (collection.Count > 0)
            {
                ExpectMutationRejected(label + ".replace", () => list[0] = sample, failures);
                ExpectMutationRejected(label + ".remove", () => list.Remove(sample), failures);
            }

            ExpectMutationRejected(label + ".add", () => list.Add(sample), failures);
            ExpectMutationRejected(label + ".clear", list.Clear, failures);
        }

        private static void ExpectMutationRejected(string label, Action action, List<string> failures)
        {
            try
            {
                action();
                failures.Add(label + ":mutation-succeeded");
            }
            catch (NotSupportedException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (Exception exception)
            {
                failures.Add(label + ":unexpected-" + exception.GetType().Name);
            }
        }

        private static ItemSystemBuildSnapshot CreateAutoSelectedBuildSnapshot(ItemSystemSnapshot valid)
        {
            ItemBuildSynergyResolutionResult build = valid.ToBuildSynergyResolutionResult();
            build.selectedMainBuildId = "famen:zhenlei";
            return new ItemSystemBuildSnapshot(build);
        }

        private static ItemSystemSkillMonitorSnapshot CreateWrongOrderMonitor()
        {
            return new ItemSystemSkillMonitorSnapshot(new ItemSkillMonitorResolutionResult(
                string.Empty,
                false,
                0,
                0,
                new[]
                {
                    new ItemSkillMonitorSlotSnapshot(ItemSkillMonitorSlotType.Build2, 0, "Build2", string.Empty, string.Empty, 2, ItemSkillTriggerKind.Condition, false, false, "build2", string.Empty, string.Empty, string.Empty)
                },
                Array.Empty<string>()));
        }

        private static ItemSystemSkillMonitorSnapshot CreateRuntimeStateMonitor()
        {
            ItemSkillMonitorSlotSnapshot[] slots =
            {
                Slot(ItemSkillMonitorSlotType.BasicAttack, 0),
                Slot(ItemSkillMonitorSlotType.Build2, 1),
                Slot(ItemSkillMonitorSlotType.Build4, 2),
                Slot(ItemSkillMonitorSlotType.Build6, 3)
            };
            slots[0].isTriggered = true;
            slots[1].cooldown01 = 0.5f;
            slots[2].charge01 = 0.25f;
            return new ItemSystemSkillMonitorSnapshot(new ItemSkillMonitorResolutionResult(
                "famen:zhenlei",
                true,
                1,
                0,
                slots,
                Array.Empty<string>()));
        }

        private static ItemSkillMonitorSlotSnapshot Slot(ItemSkillMonitorSlotType slotType, int order)
        {
            return new ItemSkillMonitorSlotSnapshot(slotType, order, slotType.ToString(), string.Empty, string.Empty, order, ItemSkillTriggerKind.Condition, false, false, slotType.ToString(), string.Empty, string.Empty, string.Empty);
        }

        private static bool HasFixedMonitorSlots(ItemSystemSkillMonitorSnapshot monitor)
        {
            ItemSkillMonitorSlotType[] expected =
            {
                ItemSkillMonitorSlotType.BasicAttack,
                ItemSkillMonitorSlotType.Build2,
                ItemSkillMonitorSlotType.Build4,
                ItemSkillMonitorSlotType.Build6
            };
            return monitor?.Slots != null
                && monitor.Slots.Count == expected.Length
                && monitor.Slots.OrderBy(slot => slot.slotOrder).Select(slot => slot.slotType).SequenceEqual(expected);
        }

        private static ItemDetailViewModel Compose(
            ItemDetailViewModel catalogModel,
            ItemDetailProjectionContextKind contextKind,
            string placementId,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult arrayBonus,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult monitor)
        {
            return ItemDetailProjectionComposer.Compose(catalogModel, contextKind, placementId, lighting, arrayBonus, build, awakening, monitor);
        }

        private static string[] Codes(IReadOnlyList<ItemSystemValidationError> errors)
        {
            return (errors ?? Array.Empty<ItemSystemValidationError>())
                .Where(error => error != null && !string.IsNullOrWhiteSpace(error.code))
                .Select(error => error.code)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToArray();
        }

        private static string[] Codes(IReadOnlyList<string> codes)
        {
            return (codes ?? Array.Empty<string>())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToArray();
        }

        private static string FormatCodes(IReadOnlyList<string> codes)
        {
            string[] normalized = Codes(codes);
            return normalized.Length == 0 ? "None" : string.Join("|", normalized);
        }

        private static bool CodesContainAll(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
        {
            HashSet<string> actualSet = new(Codes(actual), StringComparer.Ordinal);
            return Codes(expected).All(actualSet.Contains);
        }

        private static string JoinNotes(params string[] notes)
        {
            return string.Join(" ", (notes ?? Array.Empty<string>()).Where(note => !string.IsNullOrWhiteSpace(note)));
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<SpecRow> rows)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DetailReportPath) ?? "Docs/V0.4/Reports");
            UTF8Encoding encoding = new(false);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result, rows), encoding);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), encoding);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), encoding);
        }

        private static string BuildDetailReport(VerificationResult result, IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemSystemValidatorAndSnapshot01 Report");
            builder.AppendLine();
            builder.AppendLine($"Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine($"Spec rows: {rows.Count}");
            builder.AppendLine($"Passed rows: {rows.Count(row => row.result == "PASS")}");
            builder.AppendLine($"Failed rows: {rows.Count(row => row.result != "PASS")}");
            builder.AppendLine();
            builder.AppendLine("## Snapshot v1 Contract");
            builder.AppendLine("- schemaVersion is ItemSystemSnapshot.v2 with explicit I031 ownership/location state.");
            builder.AppendLine("- Provider input is cloned; generated snapshots expose read-only collections and deterministic ordering.");
            builder.AppendLine("- Snapshot covers board, eye cell, AP cells, catalog, placements, lighting, array bonus, Build, awakening, skill monitor, selected main Build, and validation errors.");
            builder.AppendLine("- Detail view models remain projections; they are not used as Item System fact sources.");
            builder.AppendLine();
            builder.AppendLine("## Notes");
            foreach (string note in result.Notes)
            {
                builder.AppendLine("- " + note);
            }

            if (result.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## Errors");
                foreach (string error in result.Errors)
                {
                    builder.AppendLine("- " + error);
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Covered Case IDs");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine($"- {row.caseId}: {row.result}");
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,inputSummary,expectedIsValid,actualIsValid,expectedErrorCodes,actualErrorCodes,snapshotDeterministic,immutable,result,notes");
            foreach (SpecRow row in rows)
            {
                builder.Append(Csv(row.caseId)).Append(',')
                    .Append(Csv(row.inputSummary)).Append(',')
                    .Append(row.expectedIsValid ? "TRUE" : "FALSE").Append(',')
                    .Append(row.actualIsValid ? "TRUE" : "FALSE").Append(',')
                    .Append(Csv(row.expectedErrorCodes)).Append(',')
                    .Append(Csv(row.actualErrorCodes)).Append(',')
                    .Append(row.snapshotDeterministic ? "TRUE" : "FALSE").Append(',')
                    .Append(row.immutable ? "TRUE" : "FALSE").Append(',')
                    .Append(Csv(row.result)).Append(',')
                    .Append(Csv(row.notes))
                    .AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemSystemValidatorAndSnapshot01 Leak Check Report");
            builder.AppendLine();
            bool clean = result.LeakFindings.Count == 0 && result.Errors.All(error => !error.Contains("runtime-leak-check", StringComparison.Ordinal));
            builder.AppendLine($"Result: {(clean ? "PASS" : "FAIL")}");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine("- Runtime ItemSystemSnapshot has no ItemSandbox dependency.");
            builder.AppendLine("- Runtime and sandbox preview files avoid forbidden formal flow references: BattleContract, UnifiedBattlePage, RunFlow, SaveData, BuildSettings, Reward, Boss, and formal bridge/adapter/resolver names.");
            builder.AppendLine("- No BuildSettings, V0.3 flow, save, reward, boss, or formal combat page file is modified by this verifier.");
            builder.AppendLine();
            builder.AppendLine("## Findings");
            if (result.LeakFindings.Count == 0)
            {
                builder.AppendLine("- None.");
            }
            else
            {
                foreach (string leak in result.LeakFindings)
                {
                    builder.AppendLine("- " + leak);
                }
            }

            return builder.ToString();
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private sealed class SpecRow
        {
            public string caseId;
            public string inputSummary;
            public bool expectedIsValid;
            public bool actualIsValid;
            public string expectedErrorCodes;
            public string actualErrorCodes;
            public bool snapshotDeterministic;
            public bool immutable;
            public string result;
            public string notes;
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
            public readonly List<string> LeakFindings = new();
        }
    }
}
