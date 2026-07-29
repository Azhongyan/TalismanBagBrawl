using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemInstanceQualifiedBuildStateAdapterVerifier
    {
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterSpec.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterLeakCheckReport.md";

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] = "e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs"] = "ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs"] = "da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] = "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] = "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Potential/ItemCorePotentialAndBuildEligibilitySchema.cs"] = "0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99",
                ["Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs"] = "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs"] = "64b9d3e2e407dc014695b4f466c4c82803384b9b37dc919c435bd181e2c13910",
                ["Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset"] = "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] = "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] = "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] = "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] = "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };

        private static readonly List<ScenarioResult> Results = new();

        [MenuItem("Talisman Bag/V0.4/Verify Item Instance Qualified Build State Adapter")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            ItemSystemBattleSandboxViewProjectionResult projection = BuildProjection();
            Run("S01", "COMPONENT_FIXTURE_PASS", () => VerifyContractFixture(projection));
            Run("S02", "REAL_RUNTIME_STATE_ASSEMBLY_PASS", () => VerifyRuntimeBaseline(projection));
            Run("S03", "I001 real sample PASS", () => VerifySample(
                projection, "I001", "wb_i001_orange_404310001",
                ItemBuildQualification.QiLeiOnly, null, "qilei:fu"));
            Run("S04", "I004 real sample PASS", () => VerifySample(
                projection, "I004", "wb_i004_orange_404310004",
                ItemBuildQualification.Dual, "famen:zhenlei", "qilei:ling"));
            Run("S05", "I006 real sample PASS", () => VerifySample(
                projection, "I006", "wb_i006_orange_404310006",
                ItemBuildQualification.None, null, null));
            Run("S06", "I009 real sample PASS", () => VerifySample(
                projection, "I009", "wb_i009_orange_404310009",
                ItemBuildQualification.FaMenOnly, "famen:lihuo", null));
            Run("S07", "I031 excluded PASS", () => VerifyI031Excluded(projection));
            Run("S08", "Inventory/Board/unlit/lit/return transitions PASS", () =>
                VerifyTransitionSummary(projection));
            Run("S09", "Unknown/Invalid semantics PASS", () =>
                VerifyUnknownAndInvalidSemantics(projection));
            Run("S09A", "Unknown build facts expose no value PASS", () =>
                VerifyUnknownBuildFacts(projection));
            Run("S09B", "Known Zero PASS", () =>
                VerifyKnownZero(projection));
            Run("S09C", "Build fact completeness conflicts rejected PASS",
                VerifyBuildFactCompletenessConflicts);
            Run("S10", "sourceIsCountedFact and qualifiedIsCountedFact separation PASS", () =>
                VerifySourceQualifiedSeparation(projection));
            Run("S11", "atomic commit and rollback-on-failure PASS", () =>
                VerifyAtomicRollback(projection));
            Run("S12", "no-op identity/call-count stability PASS", () =>
                VerifyNoOp(projection));
            Run("S13", "external mutation blocked PASS", () =>
                VerifyExternalMutationBlocked(projection));
            Run("S14", "Ordinal determinism/canonical signature PASS", () =>
                VerifyDeterminismAndExactSources(projection));
            Run("S14A", "Unknown/Known Zero canonical distinction PASS", () =>
                VerifyUnknownKnownZeroCanonicalDistinction(projection));
            Run("S15", "existing Workbench Build2/4/6 regression PASS", VerifyWorkbenchRegression);
            Run("S16", "Roll/Projection/IF01/ItemSystem/P6 protected signatures PASS", () =>
                VerifyProtectedSignatures(projection));
            Run("S17", "LeakCheck PASS", VerifyLeakBoundaries);
            WriteReports();
            Run("S18", "git diff --check PASS", VerifyPackageDiffCheck);
            WriteReports();
            ScenarioResult[] failures = Results.Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemInstanceQualifiedBuildStateAdapter verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }
            Debug.Log("[ItemInstanceQualifiedBuildStateAdapterVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nUSER_HANDTEST_NOT_APPLICABLE\nPackageB=NOT_STARTED");
        }

        private static ItemSystemBattleSandboxViewProjectionResult BuildProjection()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            GameObject providerObject = new("QualifiedBuildVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                return ItemSystemBattleSandboxViewProjection.Build(workbench, provider);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection,
            IItemInstanceQualifiedBuildStateAssembler assembler = null)
        {
            Check(projection?.IsValid == true,
                "real BattleSandbox projection is invalid: "
                + string.Join("|", projection?.Diagnostics ?? Array.Empty<string>()));
            return assembler == null
                ? new ItemSystemBattleSandboxBoardAuthority(
                    projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance)
                : new ItemSystemBattleSandboxBoardAuthority(
                    projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    assembler,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static void VerifyContractFixture(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            Check(projection.IsValid, "ViewProjection invalid");
            Check(projection.OrdinaryProjectionSet != null
                && projection.OrdinaryProjectionSet.isValid
                && projection.OrdinaryProjectionSet.Projections.Count == 30,
                "complete ordinary ProjectionSet missing");
            Check(projection.OrdinaryProjectionSet.Projections
                    .Select(value => value.baseItemId)
                    .SequenceEqual(Enumerable.Range(1, 30).Select(index =>
                        "I" + index.ToString("000", CultureInfo.InvariantCulture)),
                        StringComparer.Ordinal),
                "ProjectionSet is not exact I001-I030");
            Check(projection.OrdinaryProjectionSet.Projections
                    .Select(value => value.itemInstanceId)
                    .SequenceEqual(projection.OrdinaryProjectionSet.Projections
                        .Select(value => value.itemInstanceId)
                        .OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal),
                "ProjectionSet is not Ordinal by itemInstanceId");
            Check(typeof(IItemInstanceQualifiedBuildStateAssembler).GetMethod(
                    "Assemble") != null
                && ItemInstanceQualifiedBuildStateSnapshot.CurrentSchemaId ==
                    "ItemInstanceQualifiedBuildStateSnapshot.v1",
                "stateless assembler/schema seam missing");
        }

        private static void VerifyRuntimeBaseline(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemInstanceQualifiedBuildStateSnapshot state =
                authority.CurrentQualifiedBuildState;
            Check(state?.isValid == true && state.Items.Count == 30,
                "initial qualified state invalid");
            Check(state.Items.All(value =>
                    value.location == ItemInstanceQualifiedBuildLocation.Inventory
                    && value.placementId == null
                    && value.placementFactCompleteness ==
                        ItemInstanceQualifiedBuildFactCompleteness.NotApplicable
                    && value.isLitFact ==
                        ItemInstanceQualifiedBuildBooleanFact.NotApplicable
                    && value.sourceIsCountedFact ==
                        ItemInstanceQualifiedBuildBooleanFact.NotApplicable
                    && value.qualifiedIsCountedFact ==
                        ItemInstanceQualifiedBuildBooleanFact.NotApplicable),
                "empty-board roster did not classify exact Inventory facts");
            Check(state.sourceBindingCanonicalSignature ==
                    authority.CurrentBindingSnapshot.canonicalSignature
                && !string.IsNullOrWhiteSpace(state.sourceProjectionSetIdentity)
                && !string.IsNullOrWhiteSpace(state.sourceItemSystemCanonicalSignature)
                && state.canonicalSignature.StartsWith("sha256:",
                    StringComparison.Ordinal),
                "source identities/canonical signature missing");
        }

        private static void VerifySample(
            ItemSystemBattleSandboxViewProjectionResult projection,
            string baseItemId,
            string expectedInstanceId,
            ItemBuildQualification qualification,
            string expectedFaMenBuildId,
            string expectedQiLeiBuildId)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemInstanceQualifiedBuildItemSnapshot inventory =
                FindByBase(authority.CurrentQualifiedBuildState, baseItemId);
            Check(inventory.itemInstanceId == expectedInstanceId
                && inventory.buildQualification == qualification
                && inventory.location == ItemInstanceQualifiedBuildLocation.Inventory,
                baseItemId + " Inventory identity/qualification mismatch");
            Check(inventory.eligibleFaMenBuildId == expectedFaMenBuildId
                && inventory.eligibleQiLeiBuildId == expectedQiLeiBuildId,
                baseItemId + " eligibility mismatch");
            CommitOrdinary(authority, baseItemId, false);
            ItemInstanceQualifiedBuildItemSnapshot unlit =
                FindByBase(authority.CurrentQualifiedBuildState, baseItemId);
            Check(unlit.location == ItemInstanceQualifiedBuildLocation.Board
                && unlit.isLitFact == ItemInstanceQualifiedBuildBooleanFact.False
                && unlit.sourceIsCountedFact == ItemInstanceQualifiedBuildBooleanFact.False
                && unlit.qualifiedIsCountedFact == ItemInstanceQualifiedBuildBooleanFact.False,
                baseItemId + " unlit facts mismatch");
            Check(authority.ReturnToTray("P_BOARD_" + baseItemId).Accepted,
                baseItemId + " return failed");
            ItemInstanceQualifiedBuildItemSnapshot returned =
                FindByBase(authority.CurrentQualifiedBuildState, baseItemId);
            Check(returned.location == ItemInstanceQualifiedBuildLocation.Inventory
                && returned.buildQualification == qualification,
                baseItemId + " return changed qualification/location");

            authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 lighting setup failed");
            CommitOrdinary(authority, baseItemId, true);
            ItemInstanceQualifiedBuildItemSnapshot lit =
                FindByBase(authority.CurrentQualifiedBuildState, baseItemId);
            bool anyEligible = expectedFaMenBuildId != null || expectedQiLeiBuildId != null;
            Check(lit.isLitFact == ItemInstanceQualifiedBuildBooleanFact.True
                && lit.sourceIsCountedFact == ItemInstanceQualifiedBuildBooleanFact.True
                && lit.qualifiedIsCountedFact == (anyEligible
                    ? ItemInstanceQualifiedBuildBooleanFact.True
                    : ItemInstanceQualifiedBuildBooleanFact.False),
                baseItemId + " lit source/qualified facts mismatch");
            Check(lit.contributesToFaMen == (expectedFaMenBuildId == null
                    ? ItemInstanceQualifiedBuildBooleanFact.False
                    : ItemInstanceQualifiedBuildBooleanFact.True)
                && lit.contributesToQiLei == (expectedQiLeiBuildId == null
                    ? ItemInstanceQualifiedBuildBooleanFact.False
                    : ItemInstanceQualifiedBuildBooleanFact.True),
                baseItemId + " qualified contribution mismatch");
            VerifySingleSource(authority.CurrentQualifiedBuildState,
                lit, expectedFaMenBuildId, expectedQiLeiBuildId);
        }

        private static void VerifySingleSource(
            ItemInstanceQualifiedBuildStateSnapshot state,
            ItemInstanceQualifiedBuildItemSnapshot item,
            string faMenBuildId,
            string qiLeiBuildId)
        {
            Check(item.buildFactCompleteness ==
                    ItemInstanceQualifiedBuildFactCompleteness.Complete
                && item.faMenBuildCount.HasValue
                && item.qiLeiBuildCount.HasValue
                && item.faMenActiveStagePieceCount.HasValue
                && item.qiLeiActiveStagePieceCount.HasValue
                && item.faMenBuildCount.Value >= 0
                && item.qiLeiBuildCount.Value >= 0
                && item.faMenActiveStagePieceCount.Value >= 0
                && item.qiLeiActiveStagePieceCount.Value >= 0,
                "complete Build facts are not fully present and non-negative");
            if (faMenBuildId != null)
            {
                ItemInstanceQualifiedBuildTrackSnapshot track = state.FindFaMenTrack(faMenBuildId);
                Check(track?.qualifiedItemCount == 1
                    && track.SourceItemInstanceIds.SequenceEqual(
                        new[] { item.itemInstanceId }, StringComparer.Ordinal)
                    && item.faMenBuildCount == track.qualifiedItemCount
                    && item.faMenActiveStagePieceCount == track.activeStagePieceCount,
                    "FaMen exact source identity mismatch");
            }
            else
            {
                Check(item.faMenBuildCount == 0
                    && item.faMenActiveStagePieceCount == 0,
                    "ineligible FaMen facts are not Known Zero");
            }
            if (qiLeiBuildId != null)
            {
                ItemInstanceQualifiedBuildTrackSnapshot track = state.FindQiLeiTrack(qiLeiBuildId);
                Check(track?.qualifiedItemCount == 1
                    && track.SourceItemInstanceIds.SequenceEqual(
                        new[] { item.itemInstanceId }, StringComparer.Ordinal)
                    && item.qiLeiBuildCount == track.qualifiedItemCount
                    && item.qiLeiActiveStagePieceCount == track.activeStagePieceCount,
                    "QiLei exact source identity mismatch");
            }
            else
            {
                Check(item.qiLeiBuildCount == 0
                    && item.qiLeiActiveStagePieceCount == 0,
                    "ineligible QiLei facts are not Known Zero");
            }
        }

        private static void VerifyI031Excluded(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CurrentQualifiedBuildState.Items.All(value =>
                    !string.Equals(value.baseItemId, "I031", StringComparison.Ordinal))
                && authority.CurrentQualifiedBuildState.FindPlacement("P_SYSTEM_I031") == null,
                "I031 entered ordinary qualified state");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted
                && authority.CurrentQualifiedBuildState.Items.Count == 30,
                "I031 board transition changed ordinary row count");
        }

        private static void VerifyTransitionSummary(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            string signature0 = authority.CurrentQualifiedBuildState.canonicalSignature;
            CommitOrdinary(authority, "I001", false);
            Check(signature0 != authority.CurrentQualifiedBuildState.canonicalSignature,
                "Board transition did not refresh state");
            Check(authority.ReturnToTray("P_BOARD_I001").Accepted,
                "return transition failed");
            Check(authority.CurrentQualifiedBuildState.canonicalSignature == signature0,
                "return-to-Inventory did not restore canonical state");
        }

        private static void VerifyUnknownAndInvalidSemantics(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority baseline = NewAuthority(projection);
            ItemInstanceProjectionSetSnapshot fullSet = projection.OrdinaryProjectionSet;
            ItemInstanceQualifiedBuildStateAssembler assembler =
                ItemInstanceQualifiedBuildStateAssembler.Instance;
            Check(assembler.Assemble(new ItemInstanceQualifiedBuildStateInput(
                    null, baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                    QualifiedBuildRosterCompleteness.CompleteOwnedRoster)).status ==
                ItemInstanceQualifiedBuildStateStatus.Unknown,
                "null ProjectionSet did not become Unknown");
            ItemInstanceQualifiedBuildStateSnapshot unknownRoster = assembler.Assemble(
                new ItemInstanceQualifiedBuildStateInput(
                    fullSet, baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                    QualifiedBuildRosterCompleteness.Unknown));
            Check(unknownRoster.status == ItemInstanceQualifiedBuildStateStatus.Unknown
                && unknownRoster.Items.All(value => value.location !=
                    ItemInstanceQualifiedBuildLocation.Inventory),
                "Unknown roster inferred Inventory");
            ItemInstanceProjectionSetSnapshot set29 =
                CreateProjectionSet(fullSet.Projections.Take(29));
            Check(assembler.Assemble(new ItemInstanceQualifiedBuildStateInput(
                    set29, baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                    QualifiedBuildRosterCompleteness.PartialOwnedRoster)).status ==
                ItemInstanceQualifiedBuildStateStatus.Unknown,
                "29/30 partial roster did not become Unknown");
            CheckInvalid(assembler, set29, baseline.CurrentBindingSnapshot,
                baseline.CurrentSnapshot, "29/30 CompleteOwnedRoster");
            CheckInvalid(assembler, CreateProjectionSet(
                    fullSet.Projections.Concat(new[] { fullSet.Projections[0] })),
                baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot, "31/30 roster");
            ItemInstanceProjectionContractSnapshot baseDuplicate = CloneProjection(
                fullSet.Projections[1], "duplicate_base_identity",
                fullSet.Projections[0].baseItemId);
            CheckInvalid(assembler, CreateProjectionSet(
                    fullSet.Projections.Take(29).Concat(new[] { baseDuplicate })),
                baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                "duplicate baseItemId");
            ItemInstanceProjectionContractSnapshot i031 =
                CloneProjection(fullSet.Projections[29], baseItemId: "I031");
            CheckInvalid(assembler, CreateProjectionSet(
                    fullSet.Projections.Take(29).Concat(new[] { i031 })),
                baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                "I031 ordinary projection");
            ItemInstanceProjectionContractSnapshot unresolved = CloneProjection(
                fullSet.Projections[0], qualification: ItemBuildQualification.Unresolved);
            CheckInvalid(assembler, CreateProjectionSet(
                    new[] { unresolved }.Concat(fullSet.Projections.Skip(1))),
                baseline.CurrentBindingSnapshot, baseline.CurrentSnapshot,
                "unresolved qualification");

            ItemSystemBattleSandboxBoardAuthority board = NewAuthority(projection);
            CommitOrdinary(board, "I001", false);
            ItemInstanceQualifiedBuildItemSnapshot placed =
                FindByBase(board.CurrentQualifiedBuildState, "I001");
            ItemInstanceProjectionContractSnapshot p1 =
                fullSet.QueryByItemInstanceId(placed.itemInstanceId).snapshot;
            CheckInvalid(assembler, fullSet,
                CreateBindingSnapshot(Array.Empty<ItemInstancePlacementBindingSnapshot>()),
                board.CurrentSnapshot, "Board placement without IF01 binding");
            ItemInstancePlacementBindingSnapshot ghost = CreateBindingRow(
                p1.itemInstanceId, "P_GHOST", p1.baseItemId);
            CheckInvalid(assembler, fullSet, CreateBindingSnapshot(new[] { ghost }),
                baseline.CurrentSnapshot, "IF01 binding without Board placement");
            ItemInstancePlacementBindingSnapshot mismatch = CreateBindingRow(
                p1.itemInstanceId, placed.placementId, "I002");
            CheckInvalid(assembler, fullSet, CreateBindingSnapshot(new[] { mismatch }),
                board.CurrentSnapshot, "Projection/IF01/ItemSystem mismatch");
            ItemInstancePlacementBindingSnapshot i031Binding = CreateBindingRow(
                "FORGED_I031", "P_SYSTEM_I031", "I031");
            CheckInvalid(assembler, fullSet, CreateBindingSnapshot(new[] { i031Binding }),
                baseline.CurrentSnapshot, "I031 ordinary binding");
            ItemSystemSnapshot duplicatePlacement = CloneItemSystemWithPlacements(
                board.CurrentSnapshot,
                board.CurrentSnapshot.placements.Concat(new[] {
                    board.CurrentSnapshot.placements.First(value => value.itemId == "I001") }));
            CheckInvalid(assembler, fullSet, board.CurrentBindingSnapshot,
                duplicatePlacement, "duplicate placement");
        }

        private static void VerifyUnknownBuildFacts(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority baseline = NewAuthority(projection);
            ItemInstanceQualifiedBuildStateSnapshot unknownRoster =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        projection.OrdinaryProjectionSet,
                        baseline.CurrentBindingSnapshot,
                        baseline.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.Unknown));
            CheckUnknownBuildRows(unknownRoster, 30, "Unknown roster");

            ItemInstanceProjectionSetSnapshot partialSet = CreateProjectionSet(
                projection.OrdinaryProjectionSet.Projections.Take(29));
            ItemInstanceQualifiedBuildStateSnapshot partialRoster =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        partialSet, baseline.CurrentBindingSnapshot,
                        baseline.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.PartialOwnedRoster));
            CheckUnknownBuildRows(partialRoster, 29, "Partial roster");
        }

        private static void CheckUnknownBuildRows(
            ItemInstanceQualifiedBuildStateSnapshot state,
            int expectedCount,
            string name)
        {
            Check(state.status == ItemInstanceQualifiedBuildStateStatus.Unknown
                && state.Items.Count == expectedCount
                && state.Items.All(value =>
                    value.location != ItemInstanceQualifiedBuildLocation.Inventory
                    && value.buildFactCompleteness ==
                        ItemInstanceQualifiedBuildFactCompleteness.Unknown
                    && !value.faMenBuildCount.HasValue
                    && !value.qiLeiBuildCount.HasValue
                    && !value.faMenActiveStagePieceCount.HasValue
                    && !value.qiLeiActiveStagePieceCount.HasValue),
                name + " exposed Inventory inference or numeric zero Build facts");
        }

        private static void VerifyKnownZero(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            CheckKnownZero(FindByBase(authority.CurrentQualifiedBuildState, "I006"),
                "I006 Inventory");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "Known Zero lighting setup failed");
            CommitOrdinary(authority, "I006", true);
            ItemInstanceQualifiedBuildItemSnapshot lit =
                FindByBase(authority.CurrentQualifiedBuildState, "I006");
            Check(lit.qualifiedIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.False,
                "I006 None qualification became a qualified contribution");
            CheckKnownZero(lit, "I006 Board lit");
        }

        private static void CheckKnownZero(
            ItemInstanceQualifiedBuildItemSnapshot item,
            string name)
        {
            Check(item.buildQualification == ItemBuildQualification.None
                && item.buildFactCompleteness ==
                    ItemInstanceQualifiedBuildFactCompleteness.Complete
                && item.faMenBuildCount.HasValue && item.faMenBuildCount.Value == 0
                && item.qiLeiBuildCount.HasValue && item.qiLeiBuildCount.Value == 0
                && item.faMenActiveStagePieceCount.HasValue
                && item.faMenActiveStagePieceCount.Value == 0
                && item.qiLeiActiveStagePieceCount.HasValue
                && item.qiLeiActiveStagePieceCount.Value == 0,
                name + " did not preserve legal Known Zero Build facts");
        }

        private static void VerifyBuildFactCompletenessConflicts()
        {
            CheckKnownZero(CreateBuildFactRow(
                ItemInstanceQualifiedBuildFactCompleteness.Complete,
                0, 0, 0, 0), "direct Complete fixture");
            ItemInstanceQualifiedBuildItemSnapshot unknown = CreateBuildFactRow(
                ItemInstanceQualifiedBuildFactCompleteness.Unknown,
                null, null, null, null);
            ItemInstanceQualifiedBuildItemSnapshot notApplicable = CreateBuildFactRow(
                ItemInstanceQualifiedBuildFactCompleteness.NotApplicable,
                null, null, null, null);
            Check(!unknown.faMenBuildCount.HasValue
                && !unknown.qiLeiBuildCount.HasValue
                && !unknown.faMenActiveStagePieceCount.HasValue
                && !unknown.qiLeiActiveStagePieceCount.HasValue
                && !notApplicable.faMenBuildCount.HasValue
                && !notApplicable.qiLeiBuildCount.HasValue
                && !notApplicable.faMenActiveStagePieceCount.HasValue
                && !notApplicable.qiLeiActiveStagePieceCount.HasValue,
                "Unknown/NotApplicable fixtures exposed numeric values");

            CheckBuildFactConstructionRejected(
                ItemInstanceQualifiedBuildFactCompleteness.Complete,
                null, 0, 0, 0, "Complete with a missing value");
            CheckBuildFactConstructionRejected(
                ItemInstanceQualifiedBuildFactCompleteness.Unknown,
                0, null, null, null, "Unknown with a numeric value");
            CheckBuildFactConstructionRejected(
                ItemInstanceQualifiedBuildFactCompleteness.NotApplicable,
                null, 0, null, null, "NotApplicable with a numeric value");
            CheckBuildFactConstructionRejected(
                ItemInstanceQualifiedBuildFactCompleteness.Complete,
                -1, 0, 0, 0, "negative value");
        }

        private static ItemInstanceQualifiedBuildItemSnapshot CreateBuildFactRow(
            ItemInstanceQualifiedBuildFactCompleteness completeness,
            int? faMenCount,
            int? qiLeiCount,
            int? faMenStageCount,
            int? qiLeiStageCount)
        {
            return CreateNonPublic<ItemInstanceQualifiedBuildItemSnapshot>(
                "FACT_TEST_INSTANCE", "I006", ItemInstanceRarity.White,
                ItemBuildQualification.None,
                ItemInstanceQualifiedBuildLocation.Unknown, null,
                ItemInstanceQualifiedBuildFactCompleteness.Unknown,
                ItemInstanceQualifiedBuildBooleanFact.Unknown,
                ItemInstanceQualifiedBuildBooleanFact.Unknown,
                ItemInstanceQualifiedBuildBooleanFact.Unknown,
                ItemInstanceQualifiedBuildBooleanFact.Unknown,
                ItemInstanceQualifiedBuildBooleanFact.Unknown,
                null, null, completeness,
                faMenCount, qiLeiCount, faMenStageCount, qiLeiStageCount);
        }

        private static void CheckBuildFactConstructionRejected(
            ItemInstanceQualifiedBuildFactCompleteness completeness,
            int? faMenCount,
            int? qiLeiCount,
            int? faMenStageCount,
            int? qiLeiStageCount,
            string name)
        {
            bool rejected = false;
            try
            {
                CreateBuildFactRow(completeness, faMenCount, qiLeiCount,
                    faMenStageCount, qiLeiStageCount);
            }
            catch (TargetInvocationException exception)
            {
                rejected = exception.InnerException is ArgumentException;
            }
            Check(rejected, name + " was not rejected");
        }

        private static void VerifySourceQualifiedSeparation(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted, "lighting setup failed");
            CommitOrdinary(authority, "I006", true);
            ItemInstanceQualifiedBuildItemSnapshot none =
                FindByBase(authority.CurrentQualifiedBuildState, "I006");
            Check(none.sourceIsCountedFact == ItemInstanceQualifiedBuildBooleanFact.True
                && none.qualifiedIsCountedFact == ItemInstanceQualifiedBuildBooleanFact.False,
                "None qualification overwrote source layout fact");
        }

        private static void VerifyAtomicRollback(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            FailAfterInitialAssembler failing = new();
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection, failing);
            ItemSystemSnapshot snapshot = authority.CurrentSnapshot;
            ItemInstancePlacementBindingContractSnapshot binding = authority.CurrentBindingSnapshot;
            ItemInstanceQualifiedBuildStateSnapshot qualified =
                authority.CurrentQualifiedBuildState;
            LayoutResilienceBattleSandboxPlaytestSnapshot p6 =
                authority.CurrentLayoutResilienceSnapshot;
            int if01 = authority.If01ValidationCount;
            int q = authority.QualifiedBuildAssemblyCount;
            int p = authority.P6EvaluationCount;
            PlacementCandidate candidate = FindCandidate(authority, "I001", false);
            ItemSystemBattleSandboxBoardOperationResult rejected =
                authority.CommitFromTray("I001", candidate.Anchor, candidate.Rotation);
            Check(!rejected.Accepted
                && ReferenceEquals(snapshot, authority.CurrentSnapshot)
                && ReferenceEquals(binding, authority.CurrentBindingSnapshot)
                && ReferenceEquals(qualified, authority.CurrentQualifiedBuildState)
                && ReferenceEquals(p6, authority.CurrentLayoutResilienceSnapshot),
                "failed qualified assembly partially committed state");
            Check(authority.If01ValidationCount == if01 + 1
                && authority.QualifiedBuildAssemblyCount == q + 1
                && authority.P6EvaluationCount == p,
                "failed qualified assembly executed P6 or wrong count");
        }

        private static void VerifyNoOp(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemSnapshot item = authority.CurrentSnapshot;
            ItemInstancePlacementBindingContractSnapshot binding = authority.CurrentBindingSnapshot;
            ItemInstanceQualifiedBuildStateSnapshot qualified = authority.CurrentQualifiedBuildState;
            LayoutResilienceBattleSandboxPlaytestSnapshot p6 =
                authority.CurrentLayoutResilienceSnapshot;
            int if01 = authority.If01ValidationCount;
            int q = authority.QualifiedBuildAssemblyCount;
            int p = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult result = authority.Reset();
            Check(result.Accepted && !result.Changed
                && ReferenceEquals(item, authority.CurrentSnapshot)
                && ReferenceEquals(binding, authority.CurrentBindingSnapshot)
                && ReferenceEquals(qualified, authority.CurrentQualifiedBuildState)
                && ReferenceEquals(p6, authority.CurrentLayoutResilienceSnapshot)
                && authority.If01ValidationCount == if01
                && authority.QualifiedBuildAssemblyCount == q
                && authority.P6EvaluationCount == p,
                "no-op rebuilt/replaced state or incremented call counts");
        }

        private static void VerifyExternalMutationBlocked(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemInstanceQualifiedBuildStateSnapshot state =
                NewAuthority(projection).CurrentQualifiedBuildState;
            CheckMutationBlocked((IList<ItemInstanceQualifiedBuildItemSnapshot>)state.Items,
                state.Items[0]);
            CheckMutationBlocked((IList<ItemInstanceQualifiedBuildTrackSnapshot>)state.FaMenTracks,
                state.FaMenTracks[0]);
            CheckMutationBlocked((IList<ItemInstanceQualifiedBuildTrackSnapshot>)state.QiLeiTracks,
                state.QiLeiTracks[0]);
        }

        private static void VerifyDeterminismAndExactSources(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "determinism lighting setup failed");
            CommitOrdinary(authority, "I004", true);
            ItemInstanceQualifiedBuildItemSnapshot first =
                FindByBase(authority.CurrentQualifiedBuildState, "I004");
            string targetBuild = first.eligibleFaMenBuildId;
            string secondId = null;
            foreach (ItemInstanceQualifiedBuildItemSnapshot candidate in
                     authority.CurrentQualifiedBuildState.Items
                         .Where(value => value.location ==
                            ItemInstanceQualifiedBuildLocation.Inventory
                            && string.Equals(value.eligibleFaMenBuildId, targetBuild,
                                StringComparison.Ordinal))
                         .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal))
            {
                if (TryCommitOrdinary(authority, candidate.baseItemId, true))
                {
                    secondId = candidate.itemInstanceId;
                    break;
                }
            }
            Check(secondId != null, "no second exact FaMen source fit the board");
            ItemInstanceQualifiedBuildTrackSnapshot track =
                authority.CurrentQualifiedBuildState.FindFaMenTrack(targetBuild);
            string[] expectedSources = new[] { first.itemInstanceId, secondId }
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Check(track.qualifiedItemCount == 2
                && track.SourceItemInstanceIds.SequenceEqual(
                    expectedSources, StringComparer.Ordinal),
                "multi-item track did not expose exact Ordinal sources");
            ItemInstanceProjectionSetSnapshot reversed =
                CreateProjectionSet(projection.OrdinaryProjectionSet.Projections.Reverse());
            ItemInstanceQualifiedBuildStateSnapshot reordered =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        reversed, authority.CurrentBindingSnapshot,
                        authority.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.CompleteOwnedRoster));
            Check(reordered.isValid
                && reordered.canonicalSignature ==
                    authority.CurrentQualifiedBuildState.canonicalSignature,
                "reversed input changed canonical signature");
        }

        private static void VerifyUnknownKnownZeroCanonicalDistinction(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority baseline = NewAuthority(projection);
            ItemInstanceQualifiedBuildStateSnapshot knownZero =
                baseline.CurrentQualifiedBuildState;
            ItemInstanceQualifiedBuildStateSnapshot unknown =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        projection.OrdinaryProjectionSet,
                        baseline.CurrentBindingSnapshot,
                        baseline.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.Unknown));
            CheckKnownZero(FindByBase(knownZero, "I006"),
                "canonical Known Zero fixture");
            Check(unknown.canonicalSignature != knownZero.canonicalSignature,
                "Unknown and Known Zero canonical signatures are equal");

            string unknownPayload = unknown.BuildCanonicalPayload();
            string knownPayload = knownZero.BuildCanonicalPayload();
            foreach (string field in new[]
                     {
                         "item.faMenBuildCount",
                         "item.qiLeiBuildCount",
                         "item.faMenActiveStagePieceCount",
                         "item.qiLeiActiveStagePieceCount"
                     })
            {
                Check(unknownPayload.Contains(
                        CanonicalRecord(field + ".presence", "Missing"),
                        StringComparison.Ordinal)
                    && !unknownPayload.Contains(
                        CanonicalRecord(field, "0"), StringComparison.Ordinal),
                    "Unknown canonical payload serialized " + field + " as zero");
                Check(knownPayload.Contains(
                        CanonicalRecord(field + ".presence", "Present"),
                        StringComparison.Ordinal)
                    && knownPayload.Contains(
                        CanonicalRecord(field, "0"), StringComparison.Ordinal),
                    "Known Zero canonical payload omitted presence/value for " + field);
            }

            ItemInstanceProjectionSetSnapshot reversed = CreateProjectionSet(
                projection.OrdinaryProjectionSet.Projections.Reverse());
            ItemInstanceQualifiedBuildStateSnapshot reordered =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        reversed, baseline.CurrentBindingSnapshot,
                        baseline.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.CompleteOwnedRoster));
            Check(reordered.canonicalSignature == knownZero.canonicalSignature,
                "reversed complete Known Zero input changed canonical signature");
        }

        private static void VerifyWorkbenchRegression()
        {
            ItemSystemSnapshot source = BuildThresholdFixture();
            Dictionary<string, ItemBuildQualification> qualifications =
                Enumerable.Range(1, 6).ToDictionary(
                    index => "P" + index.ToString(CultureInfo.InvariantCulture),
                    index => index <= 4 ? ItemBuildQualification.Dual
                        : ItemBuildQualification.FaMenOnly,
                    StringComparer.Ordinal);
            ItemBuildSynergyResolutionResult qualified =
                ItemInstanceQualifiedBuildContributionResolver.Resolve(source, qualifications);
            Check(qualified.ValidationErrors.Count == 0,
                string.Join("|", qualified.ValidationErrors));
            ItemBuildTrackResult faMen = qualified.FindFaMenBuild("famen:zhenlei");
            ItemBuildTrackResult qiLei = qualified.FindQiLeiBuild("qilei:ling");
            Check(faMen?.litItemCount == 6 && faMen.activeStagePieceCount == 6
                && faMen.build2Active && faMen.build4Active && faMen.build6Active,
                "shared resolver regressed FaMen Build2/4/6");
            Check(qiLei?.litItemCount == 4 && qiLei.activeStagePieceCount == 4
                && qiLei.build2Active && qiLei.build4Active && !qiLei.build6Active,
                "shared resolver regressed QiLei Build2/4");
            string workbench = Read("Assets/_Game/Scripts/TalismanBag/ItemSandbox/"
                + "ItemFullDetailBuildSandboxWorkbenchSession.cs");
            int start = workbench.IndexOf(
                "public static class ItemFullDetailQualifiedBuildAdapter",
                StringComparison.Ordinal);
            int end = workbench.IndexOf(
                "public static class ItemFullDetailCandidateAwakeningAdapter",
                start, StringComparison.Ordinal);
            string seam = workbench.Substring(start, end - start);
            Check(seam.Contains("ItemInstanceQualifiedBuildContributionResolver.Resolve",
                    StringComparison.Ordinal)
                && !seam.Contains("allowFaMen", StringComparison.Ordinal)
                && !seam.Contains("BuildTracks", StringComparison.Ordinal),
                "Workbench retains an independent qualification filter");
        }

        private static void VerifyProtectedSignatures(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            foreach (KeyValuePair<string, string> entry in ProtectedHashes)
                Check(HashFile(ProjectPath(entry.Key)) == entry.Value,
                    "protected hash changed: " + entry.Key);
            Check(BuildProfilesAggregate() ==
                    "a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0",
                "Profiles30 aggregate changed");
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            string projectionBefore = string.Join("\n",
                projection.OrdinaryProjectionSet.Projections.Select(value =>
                    value.BuildCanonicalSignature()));
            string bindingBefore = authority.CurrentBindingSnapshot.canonicalSignature;
            string itemBefore = authority.CurrentSnapshot.BuildDebugSignature();
            string p6Before = authority.CurrentLayoutResilienceSnapshot.canonicalSignature;
            ItemInstanceQualifiedBuildStateSnapshot assembled =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        projection.OrdinaryProjectionSet,
                        authority.CurrentBindingSnapshot,
                        authority.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.CompleteOwnedRoster));
            Check(assembled.isValid
                && projectionBefore == string.Join("\n",
                    projection.OrdinaryProjectionSet.Projections.Select(value =>
                        value.BuildCanonicalSignature()))
                && bindingBefore == authority.CurrentBindingSnapshot.canonicalSignature
                && itemBefore == authority.CurrentSnapshot.BuildDebugSignature()
                && p6Before == authority.CurrentLayoutResilienceSnapshot.canonicalSignature,
                "assembler mutated a protected source signature");
        }

        private static void VerifyLeakBoundaries()
        {
            string qualifiedSources = string.Join("\n", new[]
            {
                Read("Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"),
                Read("Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"),
                Read("Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs")
            });
            foreach (string token in new[]
                     {
                         "UnityEngine", "UnityEditor", "SceneManager", "AssetDatabase",
                         "ItemDetailViewModel", "ItemDetailPanelView", "ItemDetailSectionView",
                         "SaveData", "RunFlow", "RewardConfig"
                     })
                Check(!qualifiedSources.Contains(token, StringComparison.Ordinal),
                    "runtime qualified module leaked forbidden dependency: " + token);
        }

        private static void VerifyPackageDiffCheck()
        {
            string[] packagePaths =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemInstanceQualifiedBuildStateAdapterVerifier.cs",
                ReportPath,
                SpecPath,
                LeakPath
            };
            foreach (string path in packagePaths)
            {
                string text = Read(path);
                Check(!text.Contains("\u9225?", StringComparison.Ordinal),
                    "mojibake separator remains in " + path);
                string[] lines = text.Replace("\r\n", "\n").Split('\n');
                Check(lines.All(line => !line.EndsWith(" ", StringComparison.Ordinal)
                    && !line.EndsWith("\t", StringComparison.Ordinal)),
                    "trailing whitespace remains in " + path);
                Check(lines.All(line => !line.StartsWith("<<<<<<<", StringComparison.Ordinal)
                    && !line.StartsWith("=======", StringComparison.Ordinal)
                    && !line.StartsWith(">>>>>>>", StringComparison.Ordinal)),
                    "merge marker remains in " + path);
            }

            System.Diagnostics.ProcessStartInfo start = new()
            {
                FileName = "git",
                Arguments = "diff --check -- " + string.Join(" ",
                    packagePaths.Select(path => "\"" + path + "\"")),
                WorkingDirectory = ProjectPath("."),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using System.Diagnostics.Process process =
                System.Diagnostics.Process.Start(start);
            Check(process != null, "git diff --check process did not start");
            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            Check(process.WaitForExit(30000), "git diff --check timed out");
            Check(process.ExitCode == 0,
                "git diff --check failed: " + standardOutput + standardError);
        }

        private static ItemInstanceQualifiedBuildItemSnapshot FindByBase(
            ItemInstanceQualifiedBuildStateSnapshot state, string baseItemId)
        {
            ItemInstanceQualifiedBuildItemSnapshot[] matches = state.Items
                .Where(value => string.Equals(value.baseItemId, baseItemId,
                    StringComparison.Ordinal)).ToArray();
            Check(matches.Length == 1, "qualified row cardinality mismatch for " + baseItemId);
            return matches[0];
        }

        private static void CommitOrdinary(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId, bool requireLit)
        {
            PlacementCandidate candidate = FindCandidate(authority, baseItemId, requireLit);
            Check(authority.CommitFromTray(baseItemId, candidate.Anchor,
                    candidate.Rotation).Accepted, "commit failed for " + baseItemId);
        }

        private static bool TryCommitOrdinary(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId, bool requireLit)
        {
            if (!TryFindCandidate(authority, baseItemId, requireLit,
                    out PlacementCandidate candidate)) return false;
            return authority.CommitFromTray(baseItemId, candidate.Anchor,
                candidate.Rotation).Accepted;
        }

        private static PlacementCandidate FindCandidate(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId, bool requireLit)
        {
            Check(TryFindCandidate(authority, baseItemId, requireLit,
                    out PlacementCandidate candidate),
                "no legal " + (requireLit ? "lit" : "unlit") + " candidate for " + baseItemId);
            return candidate;
        }

        private static bool TryFindCandidate(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId, bool requireLit,
            out PlacementCandidate candidate)
        {
            foreach (ItemShapeRotation rotation in new[]
                     {
                         ItemShapeRotation.Rotation0, ItemShapeRotation.Rotation90,
                         ItemShapeRotation.Rotation180, ItemShapeRotation.Rotation270
                     })
            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                ItemSystemBattleSandboxBoardOperationResult preview =
                    authority.PreviewPlacement(baseItemId, new ItemShapeCell(x, y), rotation);
                ItemSystemPlacementSnapshot placement =
                    preview.Snapshot?.FindPlacement("P_BOARD_" + baseItemId);
                if (preview.Accepted && placement != null && placement.isLit == requireLit)
                {
                    candidate = new PlacementCandidate(new ItemShapeCell(x, y), rotation);
                    return true;
                }
            }
            candidate = default;
            return false;
        }

        private static void CheckInvalid(
            IItemInstanceQualifiedBuildStateAssembler assembler,
            ItemInstanceProjectionSetSnapshot set,
            ItemInstancePlacementBindingContractSnapshot binding,
            ItemSystemSnapshot itemSystem, string name)
        {
            ItemInstanceQualifiedBuildStateSnapshot result = assembler.Assemble(
                new ItemInstanceQualifiedBuildStateInput(
                    set, binding, itemSystem,
                    QualifiedBuildRosterCompleteness.CompleteOwnedRoster));
            Check(result.status == ItemInstanceQualifiedBuildStateStatus.Invalid,
                name + " did not become Invalid: "
                + string.Join("|", result.ValidationErrors.Select(value =>
                    value.ToDiagnosticString())));
        }

        private static ItemInstanceProjectionSetSnapshot CreateProjectionSet(
            IEnumerable<ItemInstanceProjectionContractSnapshot> values) =>
            CreateNonPublic<ItemInstanceProjectionSetSnapshot>(
                values.ToArray(), Array.Empty<ItemInstanceProjectionValidationError>());

        private static ItemInstanceProjectionContractSnapshot CloneProjection(
            ItemInstanceProjectionContractSnapshot source,
            string itemInstanceId = null, string baseItemId = null,
            ItemBuildQualification? qualification = null) =>
            CreateNonPublic<ItemInstanceProjectionContractSnapshot>(
                source.sourceSchemaId, source.sourceGenerationAlgorithmId,
                source.generationDataStatus, itemInstanceId ?? source.itemInstanceId,
                baseItemId ?? source.baseItemId, source.rarity, source.rarityKey,
                source.generationVersion, source.rootSeed,
                source.cultivationPotentialProfileId, source.Stats, source.Affixes,
                source.EligibleCoreEffectIds, source.VisibleCoreEffectIds,
                qualification ?? source.buildQualification,
                source.sourceCanonicalSignature);

        private static ItemInstancePlacementBindingSnapshot CreateBindingRow(
            string itemInstanceId, string placementId, string baseItemId) =>
            CreateNonPublic<ItemInstancePlacementBindingSnapshot>(
                itemInstanceId, placementId, baseItemId);

        private static ItemInstancePlacementBindingContractSnapshot CreateBindingSnapshot(
            IEnumerable<ItemInstancePlacementBindingSnapshot> rows) =>
            CreateNonPublic<ItemInstancePlacementBindingContractSnapshot>(
                ItemInstancePlacementBindingStatus.Valid, rows.ToArray(),
                Array.Empty<ItemInstancePlacementBindingValidationError>());

        private static T CreateNonPublic<T>(params object[] arguments) =>
            (T)Activator.CreateInstance(typeof(T),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, arguments, CultureInfo.InvariantCulture);

        private static ItemSystemSnapshot CloneItemSystemWithPlacements(
            ItemSystemSnapshot source,
            IEnumerable<ItemSystemPlacementSnapshot> placements) =>
            new ItemSystemSnapshot(
                source.boardSize, source.eyeCell, source.ArrayBonusCells,
                source.catalogItems, placements.ToArray(), source.LitRangeCells,
                source.lightingResults, source.arrayBonusResults, source.buildSnapshot,
                source.awakeningResults, source.skillMonitorSnapshot,
                source.selectedMainBuildId, source.selectedMainBuildIsExplicit,
                source.selectedMainBuildSource,
                Array.Empty<ItemSystemValidationError>(), source.i031State);

        private static ItemSystemSnapshot BuildThresholdFixture()
        {
            string[] placementIds = Enumerable.Range(1, 6)
                .Select(index => "P" + index.ToString(CultureInfo.InvariantCulture)).ToArray();
            string[] itemIds = Enumerable.Range(1, 6)
                .Select(index => "I" + index.ToString("000", CultureInfo.InvariantCulture)).ToArray();
            ItemBuildTrackResult faMen = new(
                "famen:zhenlei", ItemBuildTrackKind.FaMen, "zhenlei", "震雷法",
                6, 6, placementIds, itemIds);
            ItemBuildTrackResult qiLei = new(
                "qilei:ling", ItemBuildTrackKind.QiLei, "ling", "灵",
                4, 4, placementIds.Take(4).ToArray(), itemIds.Take(4).ToArray());
            ItemBuildSynergyItemResult[] buildItems = Enumerable.Range(0, 6)
                .Select(index => new ItemBuildSynergyItemResult
                {
                    itemId = itemIds[index], placementId = placementIds[index],
                    isLit = true, countedInBuild = true,
                    faMenTag = "zhenlei", qiLeiTag = "ling",
                    faMenBuildId = "famen:zhenlei", qiLeiBuildId = "qilei:ling",
                    faMenBuildCount = 6, qiLeiBuildCount = 4,
                    faMenActiveStagePieceCount = 6,
                    qiLeiActiveStagePieceCount = 4
                }).ToArray();
            ItemSystemPlacementSnapshot[] placements = Enumerable.Range(0, 6)
                .Select(index => new ItemSystemPlacementSnapshot(
                    placementIds[index], itemIds[index], itemIds[index],
                    Vector2Int.zero, 0, Array.Empty<Vector2Int>(), Vector2Int.zero,
                    false, true, true, "I031", "P_SYSTEM_I031", 1,
                    Array.Empty<Vector2Int>(), false, false, true, 1, 1,
                    Array.Empty<string>(), Array.Empty<string>())).ToArray();
            return new ItemSystemSnapshot(
                5, new Vector2Int(2, 2), Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(), placements,
                Array.Empty<Vector2Int>(), Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                new ItemSystemBuildSnapshot(new ItemBuildSynergyResolutionResult(
                    new[] { faMen }, new[] { qiLei }, buildItems, Array.Empty<string>())),
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                new ItemSystemSkillMonitorSnapshot(null), string.Empty, false,
                string.Empty, Array.Empty<ItemSystemValidationError>(),
                new I031InventoryPlacementStateSnapshot(
                    "I031", "SPECIAL_I031", "P_SYSTEM_I031",
                    I031OwnershipCompleteness.Complete, I031Location.Board, true));
        }

        private static void CheckMutationBlocked<T>(IList<T> values, T sample)
        {
            bool blocked = false;
            try { values.Add(sample); }
            catch (NotSupportedException) { blocked = true; }
            Check(blocked, "external collection mutation was not blocked");
        }

        private static string BuildProfilesAggregate()
        {
            string directory = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] rows = Directory.GetFiles(directory, "*.asset")
                .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .Select(path => "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/"
                    + Path.GetFileName(path) + "|" + HashFile(path)).ToArray();
            Check(rows.Length == 30, "profile asset count is not 30");
            return HashText(string.Join("\n", rows) + "\n");
        }

        private static void WriteReports()
        {
            string status = Results.All(value => value.Passed) ? "PASS" : "FAIL";
            StringBuilder report = new();
            report.AppendLine("# ItemInstanceQualifiedBuildStateAdapter01 Report");
            report.AppendLine();
            report.AppendLine("Status: " + status);
            report.AppendLine();
            report.AppendLine("Schema: ItemInstanceQualifiedBuildStateSnapshot.v1");
            report.AppendLine();
            report.AppendLine("User handtest: NOT_APPLICABLE_FOR_PACKAGE_A");
            report.AppendLine();
            report.AppendLine("Package B: NOT_STARTED");
            report.AppendLine();
            report.AppendLine("The Item Detail panel does not display this new state. "
                + "That projection is reserved for a separately assigned Package B.");
            report.AppendLine();
            report.AppendLine("Unknown Build counts: null (no numeric value).");
            report.AppendLine();
            report.AppendLine("Known Zero Build counts: present nullable values equal to 0.");
            report.AppendLine();
            report.AppendLine("Canonical payload: buildFactCompleteness plus explicit nullable presence and value.");
            report.AppendLine();
            report.AppendLine("| Scenario | Result | Evidence |");
            report.AppendLine("|---|---|---|");
            foreach (ScenarioResult result in Results)
                report.AppendLine("| " + result.Id + " | "
                    + (result.Passed ? "PASS" : "FAIL") + " | "
                    + EscapeMarkdown(result.Marker + " - " + result.Detail) + " |");
            Write(ReportPath, report.ToString());
            StringBuilder spec = new();
            spec.AppendLine("scenarioId,result,marker,detail");
            foreach (ScenarioResult result in Results)
                spec.Append(Csv(result.Id)).Append(',')
                    .Append(Csv(result.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(result.Marker)).Append(',')
                    .Append(Csv(result.Detail)).Append('\n');
            Write(SpecPath, spec.ToString());
            StringBuilder leak = new();
            leak.AppendLine("# ItemInstanceQualifiedBuildStateAdapter01 Leak Check");
            leak.AppendLine();
            leak.AppendLine("Status: " + status);
            leak.AppendLine();
            leak.AppendLine("- Runtime module is pure C# and has no Unity/UI/Scene/Prefab dependency.");
            leak.AppendLine("- ItemSystemSnapshot.v2, IF01, Roll, Build rules, Item Detail, scenes, prefab and BuildSettings protected hashes are unchanged.");
            leak.AppendLine("- I031 is excluded from ordinary ProjectionSet and qualified rows.");
            leak.AppendLine("- Workbench consumes the shared Package A qualification resolver.");
            leak.AppendLine("- Unknown/NotApplicable Build counts are null; Complete counts are present and non-negative.");
            leak.AppendLine("- Canonical signatures serialize nullable presence and distinguish Unknown from Known Zero.");
            leak.AppendLine("- Task-start BoardAuthority, ViewProjection and WorkbenchSession SHA-256 values are protected.");
            leak.AppendLine("- Package B and Item Detail prefab migration are not started.");
            leak.AppendLine("- User handtest is not applicable for Package A.");
            Write(LeakPath, leak.ToString());
            AssetDatabase.Refresh();
        }

        private static void Run(string id, string marker, Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(id, marker, true, "verified"));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(id, marker, false,
                    exception.GetType().Name + ": " + exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("project root unavailable");
            return Path.GetFullPath(Path.Combine(root,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }
        private static string Read(string relativePath) =>
            File.ReadAllText(ProjectPath(relativePath));
        private static void Write(string relativePath, string text) =>
            File.WriteAllText(ProjectPath(relativePath), text, new UTF8Encoding(false));
        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
        private static string HashText(string text)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    new UTF8Encoding(false).GetBytes(text ?? string.Empty)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
        private static string EscapeMarkdown(string value) =>
            (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ")
                .Replace("\n", " ");
        private static string Csv(string value) =>
            "\"" + (value ?? string.Empty).Replace("\"", "\"\"")
                .Replace("\r", " ").Replace("\n", " ") + "\"";
        private static string CanonicalRecord(string key, string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            return safeKey.Length.ToString(CultureInfo.InvariantCulture)
                + ":" + safeKey + "="
                + safeValue.Length.ToString(CultureInfo.InvariantCulture)
                + ":" + safeValue + "\n";
        }

        private readonly struct PlacementCandidate
        {
            public PlacementCandidate(ItemShapeCell anchor, ItemShapeRotation rotation)
            { Anchor = anchor; Rotation = rotation; }
            public ItemShapeCell Anchor { get; }
            public ItemShapeRotation Rotation { get; }
        }

        private sealed class FailAfterInitialAssembler :
            IItemInstanceQualifiedBuildStateAssembler
        {
            private int callCount;
            public ItemInstanceQualifiedBuildStateSnapshot Assemble(
                ItemInstanceQualifiedBuildStateInput input)
            {
                callCount++;
                return callCount == 1
                    ? ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(input)
                    : null;
            }
        }

        private sealed class ScenarioResult
        {
            public ScenarioResult(string id, string marker, bool passed, string detail)
            { Id = id; Marker = marker; Passed = passed; Detail = detail; }
            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
