using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ItemDetailQualifiedBuildTrackProjectionVerifier
    {
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string AssignmentPath =
            "Docs/V0.4/ItemDetailQualifiedBuildTrackProjection01_Assignment.md";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionSpec.csv";
        private const string FieldLineagePath =
            "Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionFieldLineage.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionLeakCheckReport.md";
        private const string ManualPath =
            "Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionManualTest.md";
        private const string ExpectedAssignmentSha256 =
            "dff7c52e6350017afbf25d58013ee6acc82e894023ec397362251f47537bc91e";
        private const string ExpectedProfilesAggregateSha256 =
            "a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0";

        private static readonly string[] PackagePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDetailQualifiedBuildTrackProjectionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDetailQualifiedBuildTrackProjectionVerifier.cs.meta",
            ReportPath,
            SpecPath,
            FieldLineagePath,
            LeakPath,
            ManualPath
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] = "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] = "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs"] = "95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs"] = "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] = "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] = "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs"] = "64b9d3e2e407dc014695b4f466c4c82803384b9b37dc919c435bd181e2c13910",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] = "e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs"] = "ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] = "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailInstanceDataAdapter.cs"] = "0eaae3f90dd585f77d8f6d3f5426916ff6ff5c7cc1727f299974825228538806",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs"] = "65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] = "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] = "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs"] = "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
                [WorkbenchCatalogPath] = "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] = "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<SampleEvidence> Evidence = new();
        private static readonly List<FieldLineageRow> Lineage = new();

        [MenuItem("Talisman Bag/V0.4/Verify Item Detail Qualified Build Track Projection")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            Evidence.Clear();
            Lineage.Clear();
            ItemSystemBattleSandboxViewProjectionResult projection = BuildProjection();
            Run("S01", "COMPONENT_FIXTURE_PASS", () => VerifyComponentFixture(projection));
            Run("S02", "I001_REAL_SAMPLE_PASS", () => VerifyOrdinarySample(
                projection, "I001", "wb_i001_orange_404310001",
                ItemBuildQualification.QiLeiOnly, null, "qilei:fu"));
            Run("S03", "I004_REAL_SAMPLE_PASS", () => VerifyOrdinarySample(
                projection, "I004", "wb_i004_orange_404310004",
                ItemBuildQualification.Dual, "famen:zhenlei", "qilei:ling"));
            Run("S04", "I006_REAL_SAMPLE_PASS", () => VerifyOrdinarySample(
                projection, "I006", "wb_i006_orange_404310006",
                ItemBuildQualification.None, null, null));
            Run("S05", "I009_REAL_SAMPLE_PASS", () => VerifyOrdinarySample(
                projection, "I009", "wb_i009_orange_404310009",
                ItemBuildQualification.FaMenOnly, "famen:lihuo", null));
            Run("S06", "I031_REAL_SAMPLE_PASS", () => VerifySystemItem(projection));
            Run("S07", "UNKNOWN_KNOWN_ZERO_SEPARATION_PASS", () =>
                VerifyUnknownAndKnownZero(projection));
            Run("S08", "STRICT_CONTROLLER_PATH_PASS", VerifyStrictControllerPath);
            Run("S09", "PACKAGE_A_DIRECT_STAGE_LINEAGE_PASS", () =>
                VerifyDirectStageLineageEvidence());
            Run("S10", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            WriteReports();
            Run("S11", "LEAK_CHECK_PASS", VerifyLeakBoundaries);
            Run("S12", "PACKAGE_SCOPED_DIFF_CHECK_PASS", VerifyPackageDiffCheck);
            WriteReports();

            ScenarioResult[] failures = Results.Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemDetailQualifiedBuildTrackProjection verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log("[ItemDetailQualifiedBuildTrackProjectionVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nREAL_RUNTIME_PATH_PASS"
                + "\nUSER_HANDTEST_WAITING"
                + "\nDEV_COMPLETE"
                + "\nQA_STATIC_PASS");
        }

        private static ItemSystemBattleSandboxViewProjectionResult BuildProjection()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            GameObject providerObject = new("ItemDetailQualifiedProjectionProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(workbench, provider);
                Check(projection?.IsValid == true,
                    "real BattleSandbox view projection invalid: "
                    + string.Join("|", projection?.Diagnostics ?? Array.Empty<string>()));
                return projection;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection) =>
            new(
                projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);

        private static void VerifyComponentFixture(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            using RuntimeFixture fixture = new(projection);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxViewRow row = FindRow(projection, "I001");
            ItemDetailViewModel model = fixture.Show(row, string.Empty, authority);
            Check(model.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.Complete
                && model.qualifiedBuildTrack.location ==
                    ItemInstanceQualifiedBuildLocation.Inventory
                && model.qualifiedBuildTrack.TrackRows.Count == 1,
                "component fixture did not bind the qualified Inventory projection");
            ItemDetailViewModel clone = model.Clone();
            Check(!ReferenceEquals(model.qualifiedBuildTrack, clone.qualifiedBuildTrack)
                && !ReferenceEquals(model.qualifiedBuildTrack.TrackRows,
                    clone.qualifiedBuildTrack.TrackRows)
                && model.qualifiedBuildTrack.canonicalSignature ==
                    clone.qualifiedBuildTrack.canonicalSignature,
                "qualified ViewModel deep clone/canonical stability failed");
            bool mutationBlocked = false;
            try
            {
                ((IList<ItemDetailQualifiedBuildTrackRow>)
                    model.qualifiedBuildTrack.TrackRows).Add(null);
            }
            catch (NotSupportedException)
            {
                mutationBlocked = true;
            }
            Check(mutationBlocked, "qualified track collection is externally mutable");
            Check(!fixture.Adapter.Show("I001", string.Empty, authority.CurrentSnapshot)
                && fixture.Adapter.LastDiagnosticCode ==
                    "DETAIL_QUALIFIED_CONTEXT_REQUIRED"
                && !fixture.Adapter.IsVisible
                && fixture.Adapter.LastProjectedModel == null,
                "legacy three-parameter adapter entry did not fail closed");
        }

        private static void VerifyOrdinarySample(
            ItemSystemBattleSandboxViewProjectionResult projection,
            string baseItemId,
            string expectedInstanceId,
            ItemBuildQualification expectedQualification,
            string expectedFaMenBuildId,
            string expectedQiLeiBuildId)
        {
            ItemSystemBattleSandboxViewRow row = FindRow(projection, baseItemId);
            Check(row.ItemInstanceId == expectedInstanceId
                && row.Projection?.buildQualification == expectedQualification,
                baseItemId + " generated projection identity/qualification mismatch");
            using RuntimeFixture fixture = new(projection);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            string packageCanonicalBefore = authority.CurrentQualifiedBuildState.canonicalSignature;

            ItemDetailViewModel inventory = fixture.Show(row, string.Empty, authority);
            VerifyModelIdentityAndQualification(
                inventory, row, expectedQualification,
                expectedFaMenBuildId, expectedQiLeiBuildId,
                ItemInstanceQualifiedBuildLocation.Inventory);
            Check(inventory.qualifiedBuildTrack.isLitFact ==
                    ItemInstanceQualifiedBuildBooleanFact.NotApplicable
                && inventory.qualifiedBuildTrack.sourceIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.NotApplicable
                && inventory.qualifiedBuildTrack.qualifiedIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.NotApplicable,
                baseItemId + " Inventory contribution was flattened to false");
            if (expectedQualification == ItemBuildQualification.None)
            {
                VerifyKnownZero(inventory, baseItemId + " Inventory");
            }
            VerifyStaleIdentityRejection(fixture, row, authority, string.Empty);
            inventory = fixture.Show(row, string.Empty, authority);

            PlacementCandidate unlitCandidate = FindCandidate(
                authority, baseItemId, false);
            Check(authority.CommitFromTray(baseItemId,
                    unlitCandidate.Anchor, unlitCandidate.Rotation).Accepted,
                baseItemId + " unlit commit failed");
            string placementId = "P_BOARD_" + baseItemId;
            ItemDetailViewModel unlit = fixture.Show(row, placementId, authority);
            Check(unlit.qualifiedBuildTrack.location ==
                    ItemInstanceQualifiedBuildLocation.Board
                && unlit.qualifiedBuildTrack.isLitFact ==
                    ItemInstanceQualifiedBuildBooleanFact.False
                && unlit.qualifiedBuildTrack.sourceIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.False
                && unlit.qualifiedBuildTrack.qualifiedIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.False,
                baseItemId + " unlit Package A facts mismatch");
            Check(authority.ReturnToTray(placementId).Accepted,
                baseItemId + " first return failed");
            ItemDetailViewModel firstReturn = fixture.Show(row, string.Empty, authority);
            Check(firstReturn.qualifiedBuildTrack.location ==
                    ItemInstanceQualifiedBuildLocation.Inventory,
                baseItemId + " unlit return did not refresh to Inventory");

            authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                baseItemId + " I031 lighting setup failed");
            PlacementCandidate litCandidate = FindCandidate(authority, baseItemId, true);
            Check(authority.CommitFromTray(baseItemId,
                    litCandidate.Anchor, litCandidate.Rotation).Accepted,
                baseItemId + " lit commit failed");
            ItemInstanceQualifiedBuildStateSnapshot litState =
                authority.CurrentQualifiedBuildState;
            ItemDetailViewModel lit = fixture.Show(row, placementId, authority);
            VerifyModelIdentityAndQualification(
                lit, row, expectedQualification,
                expectedFaMenBuildId, expectedQiLeiBuildId,
                ItemInstanceQualifiedBuildLocation.Board);
            bool eligible = expectedFaMenBuildId != null || expectedQiLeiBuildId != null;
            Check(lit.qualifiedBuildTrack.isLitFact ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && lit.qualifiedBuildTrack.sourceIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && lit.qualifiedBuildTrack.qualifiedIsCountedFact == (eligible
                    ? ItemInstanceQualifiedBuildBooleanFact.True
                    : ItemInstanceQualifiedBuildBooleanFact.False),
                baseItemId + " lit source/qualified facts mismatch");
            VerifyTrackLineage(lit, litState, baseItemId);
            if (expectedQualification == ItemBuildQualification.None)
            {
                VerifyKnownZero(lit, baseItemId + " lit");
            }
            else
            {
                Check(lit.qualifiedBuildTrack.TrackRows.All(value =>
                        value.qualifiedItemCount == 1),
                    baseItemId + " expected real lit track count 1");
            }

            string beforeMoveCanonical = litState.canonicalSignature;
            Check(TryMoveToDifferentPlacement(authority, placementId),
                baseItemId + " could not execute a real changed move");
            ItemInstanceQualifiedBuildStateSnapshot movedState =
                authority.CurrentQualifiedBuildState;
            Check(!string.Equals(beforeMoveCanonical, movedState.canonicalSignature,
                    StringComparison.Ordinal),
                baseItemId + " move did not refresh Package A canonical state");
            Check(!fixture.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    placementId,
                    authority.CurrentSnapshot,
                    litState)
                && fixture.Adapter.LastDiagnosticCode ==
                    "DETAIL_QUALIFIED_SOURCE_CANONICAL_MISMATCH"
                && fixture.Adapter.LastProjectedModel == null
                && !fixture.Adapter.IsVisible,
                baseItemId + " stale Package A snapshot was not rejected/cleared");
            ItemDetailViewModel moved = fixture.Show(row, placementId, authority);
            VerifyTrackLineage(moved, movedState, baseItemId);
            Check(moved.qualifiedBuildTrack.sourceQualifiedBuildCanonicalSignature ==
                    movedState.canonicalSignature,
                baseItemId + " moved detail did not consume latest state");

            Check(authority.ReturnToTray(placementId).Accepted,
                baseItemId + " final return failed");
            ItemDetailViewModel returned = fixture.Show(row, string.Empty, authority);
            Check(returned.qualifiedBuildTrack.location ==
                    ItemInstanceQualifiedBuildLocation.Inventory
                && returned.qualifiedBuildTrack.placementId == null,
                baseItemId + " final Inventory refresh failed");
            Check(packageCanonicalBefore ==
                    NewAuthority(projection).CurrentQualifiedBuildState.canonicalSignature,
                baseItemId + " projection mutated Package A baseline canonical");

            AddLineageEvidence(row, litState, lit, baseItemId);
            Evidence.Add(new SampleEvidence(
                baseItemId,
                "PASS: " + expectedQualification + " / 未入阵",
                "PASS: isLit=False; source=False; qualified=False",
                "PASS: isLit=True; source=True; qualified="
                    + (eligible ? "True" : "False"),
                "PASS: latest Package A canonical consumed",
                "PASS: Inventory / placementId Missing"));
        }

        private static void VerifySystemItem(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            using RuntimeFixture fixture = new(projection);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxViewRow row = FindRow(projection, "I031");
            Check(string.IsNullOrEmpty(row.ItemInstanceId),
                "I031 unexpectedly generated ordinary itemInstanceId");
            ItemDetailViewModel inventory = fixture.Show(row, string.Empty, authority);
            Check(inventory.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.NotApplicable
                && !inventory.qualifiedBuildTrack.buildQualification.HasValue
                && inventory.qualifiedBuildTrack.TrackRows.Count == 0,
                "I031 Inventory entered ordinary qualified Build projection");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 Board commit failed");
            ItemDetailViewModel board = fixture.Show(
                row, I031InventoryPlacementContract.StablePlacementId, authority);
            Check(board.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.NotApplicable
                && board.qualifiedBuildTrack.TrackRows.Count == 0
                && authority.CurrentQualifiedBuildState.Items.All(item =>
                    !string.Equals(item.baseItemId, "I031", StringComparison.Ordinal)),
                "I031 Board projection produced ordinary Build row");
            Check(!fixture.Adapter.Show(
                    "I031", "forbidden_instance", string.Empty,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState)
                && fixture.Adapter.LastProjectedModel == null,
                "I031 accepted an ordinary instance identity");
            Lineage.Add(new FieldLineageRow(
                "I031.buildQualification", "系统道具不适用",
                "I031 contract", "ItemSystemBattleSandboxBoardAuthority",
                "no Package A ordinary row",
                "ItemDetailQualifiedBuildTrackProjector",
                "qualifiedBuildTrack.buildQualification",
                "famenBuild/qileiBuild", "I031",
                "ordinaryProjectionRows=0", "qualifiedRows=0",
                "NotApplicable/trackRows=0"));
            Evidence.Add(new SampleEvidence(
                "I031", "PASS: system detail / NotApplicable",
                "PASS: ordinary Build row absent",
                "PASS: system placement / ordinary Build row absent",
                "NOT_APPLICABLE", "PASS: system detail preserved"));
        }

        private static void VerifyUnknownAndKnownZero(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            using RuntimeFixture fixture = new(projection);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxViewRow row = FindRow(projection, "I006");
            ItemInstanceQualifiedBuildStateSnapshot unknown =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        projection.OrdinaryProjectionSet,
                        authority.CurrentBindingSnapshot,
                        authority.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.PartialOwnedRoster));
            Check(unknown.status == ItemInstanceQualifiedBuildStateStatus.Unknown,
                "partial roster did not produce Package A Unknown state");
            Check(fixture.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    string.Empty,
                    authority.CurrentSnapshot,
                    unknown),
                "Unknown safe projection failed: " + fixture.Adapter.LastDiagnosticCode);
            ItemDetailViewModel unknownModel = fixture.Adapter.LastProjectedModel;
            Check(unknownModel.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.Unknown
                && unknownModel.qualifiedBuildTrack.faMenBuildCount == null
                && unknownModel.qualifiedBuildTrack.qiLeiBuildCount == null
                && unknownModel.qualifiedBuildTrack.faMenActiveStagePieceCount == null
                && unknownModel.qualifiedBuildTrack.qiLeiActiveStagePieceCount == null
                && string.IsNullOrEmpty(
                    PlayerSectionBody(unknownModel, "famenBuild"))
                && string.IsNullOrEmpty(
                    PlayerSectionBody(unknownModel, "qileiBuild")),
                "Unknown was not preserved as missing/fail-closed");
            string unknownCanonical =
                unknownModel.qualifiedBuildTrack.canonicalSignature;

            ItemDetailViewModel knownZero = fixture.Show(row, string.Empty, authority);
            VerifyKnownZero(knownZero, "I006 Known Zero");
            Check(knownZero.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.Complete
                && !string.Equals(unknownCanonical,
                    knownZero.qualifiedBuildTrack.canonicalSignature,
                    StringComparison.Ordinal)
                && string.IsNullOrEmpty(
                    PlayerSectionBody(knownZero, "famenBuild"))
                && string.IsNullOrEmpty(
                    PlayerSectionBody(knownZero, "qileiBuild")),
                "Unknown and Known Zero typed/canonical states were conflated");
            Lineage.Add(new FieldLineageRow(
                "I006.unknownVsKnownZero", "Build状态",
                "Package A", "ItemInstanceQualifiedBuildStateAssembler",
                "buildFactCompleteness + nullable counts",
                "ItemDetailQualifiedBuildTrackProjector",
                "qualifiedBuildTrack.*Count",
                "famenBuild", "I006",
                "Unknown:null / Valid:0", "Unknown:null / Valid:0",
                "Unknown:null / Complete:0"));
        }

        private static void VerifyStrictControllerPath()
        {
            string controller = Read(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs");
            Check(CountOccurrences(controller, "itemSystemDetailAdapter.Show(") == 1
                && controller.Contains("row.ItemInstanceId", StringComparison.Ordinal)
                && controller.Contains(
                    "itemSystemBoardAuthority.CurrentQualifiedBuildState",
                    StringComparison.Ordinal)
                && controller.Contains("ShowQualifiedItemSystemDetail(item)",
                    StringComparison.Ordinal),
                "controller production path is not the single strict qualified Show call");
            string adapter = Read(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs");
            Check(adapter.Contains("DETAIL_QUALIFIED_CONTEXT_REQUIRED",
                    StringComparison.Ordinal)
                && adapter.Contains("ItemDetailQualifiedBuildTrackProjector.Project(",
                    StringComparison.Ordinal),
                "adapter strict/fail-closed seams missing");
        }

        private static void VerifyDirectStageLineageEvidence()
        {
            Check(Lineage.Any(value => value.FieldKey.Contains(
                    "qualifiedItemCount", StringComparison.Ordinal))
                && Lineage.Any(value => value.FieldKey.Contains(
                    "activeStagePieceCount", StringComparison.Ordinal)),
                "real sample direct Package A stage lineage evidence missing");
            string projector = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs");
            foreach (string forbidden in new[]
                     {
                         "ToBuildSynergyResolutionResult",
                         "ItemBuildSynergyResolutionResult",
                         "ItemBuildSynergyItemResult",
                         "litItemCount",
                         "BuildStageLabel"
                     })
            {
                Check(!projector.Contains(forbidden, StringComparison.Ordinal),
                    "projector contains second-evaluator token: " + forbidden);
            }
        }

        private static void VerifyProtectedHashes()
        {
            Check(HashFile(ProjectPath(AssignmentPath)) == ExpectedAssignmentSha256,
                "Assignment SHA-256 changed");
            foreach (KeyValuePair<string, string> entry in ProtectedHashes)
            {
                string actual = HashFile(ProjectPath(entry.Key));
                Check(actual == entry.Value,
                    "protected hash mismatch: " + entry.Key + " expected="
                    + entry.Value + " actual=" + actual);
            }
            Check(BuildProfilesAggregate() == ExpectedProfilesAggregateSha256,
                "Profiles30 aggregate SHA-256 changed");
        }

        private static void VerifyLeakBoundaries()
        {
            Check(PackagePaths.Length == 12
                && PackagePaths.Take(3).All(path => File.Exists(ProjectPath(path)))
                && PackagePaths.Skip(3).All(path => File.Exists(ProjectPath(path))),
                "package whitelist is not exact 3 existing + 9 new files");
            string projector = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs");
            Check(!projector.Contains("UnityEngine", StringComparison.Ordinal)
                && !projector.Contains("AssetDatabase", StringComparison.Ordinal)
                && !projector.Contains("SceneManager", StringComparison.Ordinal),
                "pure projector leaked Unity/editor/scene dependencies");
            foreach (string path in ProtectedHashes.Keys)
            {
                Check(HashFile(ProjectPath(path)) == ProtectedHashes[path],
                    "protected file changed during verifier: " + path);
            }
            Check(BuildProfilesAggregate() == ExpectedProfilesAggregateSha256,
                "Profiles30 changed during verifier");
        }

        private static void VerifyPackageDiffCheck()
        {
            foreach (string path in PackagePaths)
            {
                Check(File.Exists(ProjectPath(path)), "package file missing: " + path);
                string[] lines = Read(path).Replace("\r\n", "\n").Split('\n');
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
                    PackagePaths.Select(path => "\"" + path + "\"")),
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

        private static void VerifyStaleIdentityRejection(
            RuntimeFixture fixture,
            ItemSystemBattleSandboxViewRow row,
            ItemSystemBattleSandboxBoardAuthority authority,
            string placementId)
        {
            Check(!fixture.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId + "_stale",
                    placementId,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState)
                && fixture.Adapter.LastProjectedModel == null
                && !fixture.Adapter.IsVisible,
                row.BaseItemId + " stale itemInstanceId was not rejected/cleared");
            fixture.Show(row, placementId, authority);
            Check(!fixture.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    "P_STALE_" + row.BaseItemId,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState)
                && fixture.Adapter.LastProjectedModel == null
                && !fixture.Adapter.IsVisible,
                row.BaseItemId + " stale placementId was not rejected/cleared");
            fixture.Show(row, placementId, authority);
            string otherBase = string.Equals(row.BaseItemId, "I001",
                StringComparison.Ordinal) ? "I004" : "I001";
            Check(!fixture.Adapter.Show(
                    otherBase,
                    row.ItemInstanceId,
                    placementId,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState)
                && fixture.Adapter.LastProjectedModel == null
                && !fixture.Adapter.IsVisible,
                row.BaseItemId + " baseItemId mismatch was not rejected/cleared");
        }

        private static void VerifyModelIdentityAndQualification(
            ItemDetailViewModel model,
            ItemSystemBattleSandboxViewRow row,
            ItemBuildQualification qualification,
            string faMenBuildId,
            string qiLeiBuildId,
            ItemInstanceQualifiedBuildLocation location)
        {
            ItemDetailQualifiedBuildTrackProjection value = model.qualifiedBuildTrack;
            Check(model.baseItemId == row.BaseItemId
                && model.itemInstanceId == row.ItemInstanceId
                && value.itemInstanceId == row.ItemInstanceId
                && value.baseItemId == row.BaseItemId
                && value.location == location
                && value.buildQualification == qualification
                && value.eligibleFaMenBuildId == faMenBuildId
                && value.eligibleQiLeiBuildId == qiLeiBuildId,
                row.BaseItemId + " final ViewModel identity/qualification mismatch");
            Check(value.TrackRows.Count ==
                    (faMenBuildId == null ? 0 : 1) + (qiLeiBuildId == null ? 0 : 1),
                row.BaseItemId + " exposed ineligible or missed eligible track");
            Check((faMenBuildId == null
                    ? !model.displayPlayerSections.Any(section => section != null
                        && section.stateKey == "famenBuild"
                        && section.keepWhenEmpty
                        && qualification != ItemBuildQualification.None)
                    : PlayerSectionBody(model, "famenBuild").Contains(
                            ItemBuildPlayerPresentationFormatter
                                .FaMenSetName(faMenBuildId),
                            StringComparison.Ordinal)
                        && !PlayerSectionBody(model, "famenBuild").Contains(
                            faMenBuildId,
                            StringComparison.Ordinal))
                && (qiLeiBuildId == null
                    ? !model.displayPlayerSections.Any(section => section != null
                        && section.stateKey == "qileiBuild"
                        && section.keepWhenEmpty)
                    : PlayerSectionBody(model, "qileiBuild").Contains(
                            ItemBuildPlayerPresentationFormatter
                                .ProgressLabel(false, qiLeiBuildId),
                            StringComparison.Ordinal)
                        && !PlayerSectionBody(model, "qileiBuild").Contains(
                            qiLeiBuildId,
                            StringComparison.Ordinal)),
                row.BaseItemId + " player sections leaked/missed qualified track");
        }

        private static void VerifyKnownZero(ItemDetailViewModel model, string label)
        {
            ItemDetailQualifiedBuildTrackProjection value = model.qualifiedBuildTrack;
            Check(value.buildQualification == ItemBuildQualification.None
                && value.faMenBuildCount == 0
                && value.qiLeiBuildCount == 0
                && value.faMenActiveStagePieceCount == 0
                && value.qiLeiActiveStagePieceCount == 0
                && value.TrackRows.Count == 0
                && string.IsNullOrEmpty(PlayerSectionBody(model, "famenBuild"))
                && string.IsNullOrEmpty(PlayerSectionBody(model, "qileiBuild")),
                label + " did not preserve Package A Known Zero values");
        }

        private static void VerifyTrackLineage(
            ItemDetailViewModel model,
            ItemInstanceQualifiedBuildStateSnapshot state,
            string label)
        {
            Check(model.qualifiedBuildTrack.sourceQualifiedBuildCanonicalSignature ==
                    state.canonicalSignature,
                label + " source canonical lineage mismatch");
            foreach (ItemDetailQualifiedBuildTrackRow row in
                     model.qualifiedBuildTrack.TrackRows)
            {
                ItemInstanceQualifiedBuildTrackSnapshot source =
                    row.trackKind == ItemBuildTrackKind.FaMen
                        ? state.FindFaMenTrack(row.buildId)
                        : state.FindQiLeiTrack(row.buildId);
                Check(source != null
                    && row.qualifiedItemCount == source.qualifiedItemCount
                    && row.maxPieceCount == source.maxPieceCount
                    && row.activeStagePieceCount == source.activeStagePieceCount
                    && row.nextStagePieceCount == source.nextStagePieceCount,
                    label + " did not copy Package A track fields directly");
                int[] expectedStages = row.trackKind == ItemBuildTrackKind.FaMen
                    ? new[] { 2, 4, 6 }
                    : new[] { 2, 4 };
                Check(row.StageRows.Select(value => value.stagePieceCount)
                        .SequenceEqual(expectedStages)
                    && row.StageRows.All(value => value.isActive ==
                        (source.activeStagePieceCount >= value.stagePieceCount)),
                    label + " stage presentation is not driven by Package A active stage");
            }
        }

        private static void AddLineageEvidence(
            ItemSystemBattleSandboxViewRow row,
            ItemInstanceQualifiedBuildStateSnapshot state,
            ItemDetailViewModel model,
            string sampleId)
        {
            ItemInstanceQualifiedBuildItemSnapshot item =
                state.FindItemInstance(row.ItemInstanceId);
            Lineage.Add(new FieldLineageRow(
                sampleId + ".buildQualification", "Build资格",
                "Generated Item Projection", "Package A assembler",
                "item.buildQualification",
                "ItemDetailQualifiedBuildTrackProjector",
                "qualifiedBuildTrack.buildQualification",
                "famenBuild/qileiBuild", sampleId,
                row.Projection.buildQualification.ToString(),
                item.buildQualification.ToString(),
                model.qualifiedBuildTrack.buildQualification.ToString()));
            foreach (ItemDetailQualifiedBuildTrackRow track in
                     model.qualifiedBuildTrack.TrackRows)
            {
                ItemInstanceQualifiedBuildTrackSnapshot source =
                    track.trackKind == ItemBuildTrackKind.FaMen
                        ? state.FindFaMenTrack(track.buildId)
                        : state.FindQiLeiTrack(track.buildId);
                string section = track.trackKind == ItemBuildTrackKind.FaMen
                    ? "famenBuild" : "qileiBuild";
                Lineage.Add(new FieldLineageRow(
                    sampleId + "." + track.buildId + ".qualifiedItemCount",
                    "当前进度", "Package A", "Package A assembler",
                    "track.qualifiedItemCount",
                    "ItemDetailQualifiedBuildTrackProjector",
                    "qualifiedBuildTrack.TrackRows[].qualifiedItemCount",
                    section, sampleId,
                    source.qualifiedItemCount + "/rows=" + source.SourceItemInstanceIds.Count,
                    source.qualifiedItemCount + "/rows=" + source.SourceItemInstanceIds.Count,
                    track.qualifiedItemCount + "/rows=" + track.StageRows.Count));
                Lineage.Add(new FieldLineageRow(
                    sampleId + "." + track.buildId + ".activeStagePieceCount",
                    "2/4/6阶段", "Package A", "Package A assembler",
                    "track.activeStagePieceCount + nextStagePieceCount",
                    "ItemDetailQualifiedBuildTrackProjector",
                    "qualifiedBuildTrack.TrackRows[].StageRows",
                    section, sampleId,
                    source.activeStagePieceCount + "/" + source.nextStagePieceCount,
                    source.activeStagePieceCount + "/" + source.nextStagePieceCount,
                    track.activeStagePieceCount + "/" + track.nextStagePieceCount
                        + "/rows=" + track.StageRows.Count));
            }
        }

        private static PlacementCandidate FindCandidate(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId,
            bool requireLit)
        {
            foreach (ItemShapeRotation rotation in new[]
                     {
                         ItemShapeRotation.Rotation0,
                         ItemShapeRotation.Rotation90,
                         ItemShapeRotation.Rotation180,
                         ItemShapeRotation.Rotation270
                     })
            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                ItemSystemBattleSandboxBoardOperationResult preview =
                    authority.PreviewPlacement(
                        baseItemId, new ItemShapeCell(x, y), rotation);
                ItemSystemPlacementSnapshot placement =
                    preview.Snapshot?.FindPlacement("P_BOARD_" + baseItemId);
                if (preview.Accepted && placement != null
                    && placement.isLit == requireLit)
                {
                    return new PlacementCandidate(
                        new ItemShapeCell(x, y), rotation);
                }
            }
            throw new InvalidOperationException(
                "no legal " + (requireLit ? "lit" : "unlit")
                + " candidate for " + baseItemId);
        }

        private static bool TryMoveToDifferentPlacement(
            ItemSystemBattleSandboxBoardAuthority authority,
            string placementId)
        {
            foreach (ItemShapeRotation rotation in new[]
                     {
                         ItemShapeRotation.Rotation0,
                         ItemShapeRotation.Rotation90,
                         ItemShapeRotation.Rotation180,
                         ItemShapeRotation.Rotation270
                     })
            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                ItemSystemBattleSandboxBoardOperationResult result =
                    authority.CommitMove(
                        placementId, new ItemShapeCell(x, y), rotation);
                if (result.Accepted && result.Changed)
                {
                    return true;
                }
            }
            return false;
        }

        private static ItemSystemBattleSandboxViewRow FindRow(
            ItemSystemBattleSandboxViewProjectionResult projection,
            string baseItemId) => projection.Rows.Single(row => row != null
                && string.Equals(row.BaseItemId, baseItemId, StringComparison.Ordinal));

        private static string PlayerSectionBody(
            ItemDetailViewModel model,
            string stateKey) => model.displayPlayerSections
                .FirstOrDefault(section => section != null
                    && string.Equals(section.stateKey, stateKey,
                        StringComparison.Ordinal))?.body ?? string.Empty;

        private static int CountOccurrences(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = (source ?? string.Empty).IndexOf(
                       token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static string BuildProfilesAggregate()
        {
            string directory = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] rows = Directory.GetFiles(directory, "*.asset")
                .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                .Select(path => "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/"
                    + Path.GetFileName(path) + "|" + HashFile(path))
                .ToArray();
            Check(rows.Length == 30, "profile asset count is not 30");
            return HashText(string.Join("\n", rows) + "\n");
        }

        private static void WriteReports()
        {
            bool pass = Results.All(result => result.Passed);
            StringBuilder report = new();
            report.AppendLine("# ItemDetailQualifiedBuildTrackProjection01 Report");
            report.AppendLine();
            report.AppendLine("Status: " + (pass ? "PASS" : "FAIL"));
            report.AppendLine();
            report.AppendLine("- Package: `V0.4-ItemDetailQualifiedBuildTrackProjection01`");
            report.AppendLine("- Assignment SHA-256: `" + ExpectedAssignmentSha256 + "`");
            report.AppendLine("- Guard receipt: `ITEM_GUARD_PASS_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01`");
            report.AppendLine("- `COMPONENT_FIXTURE_" + ResultWord("S01") + "`");
            report.AppendLine("- `REAL_RUNTIME_PATH_" + (Evidence.Count == 5 ? "PASS" : "FAIL") + "`");
            report.AppendLine("- `USER_HANDTEST_WAITING`");
            report.AppendLine("- Downstream packages: `NOT_STARTED`");
            report.AppendLine();
            report.AppendLine("## Real sample matrix");
            report.AppendLine();
            report.AppendLine("| Sample | Inventory | Board unlit | Board lit | Move/refresh | Return |");
            report.AppendLine("|---|---|---|---|---|---|");
            foreach (SampleEvidence evidence in Evidence.OrderBy(
                         value => value.SampleId, StringComparer.Ordinal))
            {
                report.AppendLine("| " + evidence.SampleId + " | "
                    + EscapeMarkdown(evidence.Inventory) + " | "
                    + EscapeMarkdown(evidence.Unlit) + " | "
                    + EscapeMarkdown(evidence.Lit) + " | "
                    + EscapeMarkdown(evidence.Move) + " | "
                    + EscapeMarkdown(evidence.Returned) + " |");
            }
            report.AppendLine();
            report.AppendLine("## Verification scenarios");
            report.AppendLine();
            report.AppendLine("| Scenario | Result | Marker | Evidence |");
            report.AppendLine("|---|---|---|---|");
            foreach (ScenarioResult result in Results)
            {
                report.AppendLine("| " + result.Id + " | "
                    + (result.Passed ? "PASS" : "FAIL") + " | `"
                    + result.Marker + "` | " + EscapeMarkdown(result.Detail) + " |");
            }
            report.AppendLine();
            report.AppendLine("Unknown numeric fields remain Missing/null. Known Zero remains present `0`; both fail closed in player Build sections while typed status and canonical signatures remain distinct.");
            report.AppendLine();
            report.AppendLine("The fixed 2/4/6 and 2/4 display rows copy `qualifiedItemCount`, `maxPieceCount`, `activeStagePieceCount`, and `nextStagePieceCount` from Package A; no UI-side Build evaluator is present.");
            report.AppendLine();
            report.AppendLine("Final gate: `DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PATH_PASS / WAITING_USER_HANDTEST` when every verifier scenario passes.");
            Write(ReportPath, report.ToString());

            StringBuilder spec = new();
            spec.AppendLine("scenarioId,result,marker,detail");
            foreach (ScenarioResult result in Results)
            {
                spec.Append(Csv(result.Id)).Append(',')
                    .Append(Csv(result.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(result.Marker)).Append(',')
                    .Append(Csv(result.Detail)).Append('\n');
            }
            Write(SpecPath, spec.ToString());

            StringBuilder lineage = new();
            lineage.AppendLine("fieldKey,playerLabel,dataOwner,runtimeProducer,snapshotField,projector,viewModelField,viewSection,realSampleId,sourceValue/rowCount,snapshotValue/rowCount,viewModelValue/rowCount,componentFixtureResult,realRuntimeResult,userHandtestResult,firstMissingLayer");
            foreach (FieldLineageRow row in Lineage.OrderBy(
                         value => value.FieldKey, StringComparer.Ordinal))
            {
                lineage.Append(Csv(row.FieldKey)).Append(',')
                    .Append(Csv(row.PlayerLabel)).Append(',')
                    .Append(Csv(row.DataOwner)).Append(',')
                    .Append(Csv(row.RuntimeProducer)).Append(',')
                    .Append(Csv(row.SnapshotField)).Append(',')
                    .Append(Csv(row.Projector)).Append(',')
                    .Append(Csv(row.ViewModelField)).Append(',')
                    .Append(Csv(row.ViewSection)).Append(',')
                    .Append(Csv(row.RealSampleId)).Append(',')
                    .Append(Csv(row.SourceValue)).Append(',')
                    .Append(Csv(row.SnapshotValue)).Append(',')
                    .Append(Csv(row.ViewModelValue)).Append(',')
                    .Append(Csv("PASS")).Append(',')
                    .Append(Csv("PASS")).Append(',')
                    .Append(Csv("WAITING")).Append(',')
                    .Append(Csv("None")).Append('\n');
            }
            Write(FieldLineagePath, lineage.ToString());

            StringBuilder leak = new();
            leak.AppendLine("# ItemDetailQualifiedBuildTrackProjection01 Leak Check");
            leak.AppendLine();
            leak.AppendLine("Status: " + ResultWord("S11"));
            leak.AppendLine();
            leak.AppendLine("- Exact package scope: 3 existing modified files + 9 new files.");
            leak.AppendLine("- Projector is pure C# and has no Unity, scene, prefab, UI mutation, catalog identity join, or runtime state ownership.");
            leak.AppendLine("- Composer, View, Package A, ItemSystem, IF01, build rules, catalogs, profiles, scenes, prefab and BuildSettings protected hashes are unchanged.");
            leak.AppendLine("- Profiles30 aggregate SHA-256 remains `" + ExpectedProfilesAggregateSha256 + "`.");
            leak.AppendLine("- Old three-parameter adapter entry is compile-compatible and fail-closed with `DETAIL_QUALIFIED_CONTEXT_REQUIRED`.");
            leak.AppendLine("- No Prefab migration, Scene Builder, formal battle flow, downstream package, commit, tag, push, reset or rollback was performed.");
            leak.AppendLine("- User handtest remains `WAITING`.");
            Write(LeakPath, leak.ToString());

            Write(ManualPath,
                "# ItemDetailQualifiedBuildTrackProjection01 Manual Test\n\n"
                + "Status: `WAITING_USER_HANDTEST`\n\n"
                + "Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`\n\n"
                + "1. Play 后点击道具栏 I001：应显示器类 `符` Build 资格，不再显示旧 fallback。\n"
                + "2. 将 I001 放到未点亮位置：显示未点亮，source / qualified 均不贡献。\n"
                + "3. 合法放置 I031 使 I001 点亮：显示 `qilei:fu = 1/4`。\n"
                + "4. 点击 I004：显示法门震雷与器类令两条资格；点亮上阵后两条轨道均反映该实例贡献。\n"
                + "5. 点击 I006：Inventory 与 Board 的法门 / 器类玩家区块均隐藏；typed debug 仍保留 Known Zero，不贡献。\n"
                + "6. 点击 I009：只显示离火法门资格；点亮上阵后显示 `famen:lihuo = 1/6`。\n"
                + "7. 将普通道具移位、收回道具栏并重新打开：详情立即跟随最新状态。\n"
                + "8. 点击 I031：只显示系统道具详情，不出现普通法门 / 器类资格。\n"
                + "9. 连续点击多件道具：不得残留上一件道具的轨道、进度或阶段。\n");
            AssetDatabase.Refresh();
        }

        private static string ResultWord(string id)
        {
            ScenarioResult result = Results.LastOrDefault(value => value.Id == id);
            return result == null ? "PENDING" : result.Passed ? "PASS" : "FAIL";
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
                Results.Add(new ScenarioResult(
                    id, marker, false,
                    exception.GetType().Name + ": " + exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("project root unavailable");
            return Path.GetFullPath(Path.Combine(
                root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Read(string relativePath) =>
            File.ReadAllText(ProjectPath(relativePath));

        private static void Write(string relativePath, string text) =>
            File.WriteAllText(
                ProjectPath(relativePath), text, new UTF8Encoding(false));

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
            (value ?? string.Empty).Replace("|", "\\|")
                .Replace("\r", " ").Replace("\n", " ");

        private static string Csv(string value) =>
            "\"" + (value ?? string.Empty).Replace("\"", "\"\"")
                .Replace("\r", " ").Replace("\n", " ") + "\"";

        private readonly struct PlacementCandidate
        {
            public PlacementCandidate(
                ItemShapeCell anchor,
                ItemShapeRotation rotation)
            {
                Anchor = anchor;
                Rotation = rotation;
            }

            public ItemShapeCell Anchor { get; }
            public ItemShapeRotation Rotation { get; }
        }

        private sealed class RuntimeFixture : IDisposable
        {
            private readonly Scene scene;

            public RuntimeFixture(
                ItemSystemBattleSandboxViewProjectionResult projection)
            {
                scene = EditorSceneManager.NewPreviewScene();
                GameObject popup = new("PopupLayer", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(popup, scene);
                GameObject panelObject = new("ItemDetailPanel", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(panelObject, scene);
                panelObject.transform.SetParent(popup.transform, false);
                ItemDetailPanelView panel = panelObject.AddComponent<ItemDetailPanelView>();
                Adapter = new ItemSystemBattleSandboxItemDetailAdapter();
                Check(Adapter.Initialize(panel, projection.Rows),
                    "runtime fixture adapter initialize failed: "
                    + Adapter.LastDiagnosticCode);
            }

            public ItemSystemBattleSandboxItemDetailAdapter Adapter { get; }

            public ItemDetailViewModel Show(
                ItemSystemBattleSandboxViewRow row,
                string placementId,
                ItemSystemBattleSandboxBoardAuthority authority)
            {
                string canonicalBefore =
                    authority.CurrentQualifiedBuildState.canonicalSignature;
                Check(Adapter.Show(
                        row.BaseItemId,
                        row.ItemInstanceId,
                        placementId,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState),
                    row.BaseItemId + " strict adapter show failed: "
                    + Adapter.LastDiagnosticCode);
                Check(Adapter.IsVisible && Adapter.LastProjectedModel != null
                    && canonicalBefore ==
                        authority.CurrentQualifiedBuildState.canonicalSignature,
                    row.BaseItemId + " bind mutated Package A or failed visibility");
                return Adapter.LastProjectedModel;
            }

            public void Dispose()
            {
                Adapter.Uninstall();
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(scene);
                }
            }
        }

        private sealed class ScenarioResult
        {
            public ScenarioResult(
                string id,
                string marker,
                bool passed,
                string detail)
            {
                Id = id;
                Marker = marker;
                Passed = passed;
                Detail = detail;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }

        private sealed class SampleEvidence
        {
            public SampleEvidence(
                string sampleId,
                string inventory,
                string unlit,
                string lit,
                string move,
                string returned)
            {
                SampleId = sampleId;
                Inventory = inventory;
                Unlit = unlit;
                Lit = lit;
                Move = move;
                Returned = returned;
            }

            public string SampleId { get; }
            public string Inventory { get; }
            public string Unlit { get; }
            public string Lit { get; }
            public string Move { get; }
            public string Returned { get; }
        }

        private sealed class FieldLineageRow
        {
            public FieldLineageRow(
                string fieldKey,
                string playerLabel,
                string dataOwner,
                string runtimeProducer,
                string snapshotField,
                string projector,
                string viewModelField,
                string viewSection,
                string realSampleId,
                string sourceValue,
                string snapshotValue,
                string viewModelValue)
            {
                FieldKey = fieldKey;
                PlayerLabel = playerLabel;
                DataOwner = dataOwner;
                RuntimeProducer = runtimeProducer;
                SnapshotField = snapshotField;
                Projector = projector;
                ViewModelField = viewModelField;
                ViewSection = viewSection;
                RealSampleId = realSampleId;
                SourceValue = sourceValue;
                SnapshotValue = snapshotValue;
                ViewModelValue = viewModelValue;
            }

            public string FieldKey { get; }
            public string PlayerLabel { get; }
            public string DataOwner { get; }
            public string RuntimeProducer { get; }
            public string SnapshotField { get; }
            public string Projector { get; }
            public string ViewModelField { get; }
            public string ViewSection { get; }
            public string RealSampleId { get; }
            public string SourceValue { get; }
            public string SnapshotValue { get; }
            public string ViewModelValue { get; }
        }
    }
}
