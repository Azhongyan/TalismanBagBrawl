using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class
        ItemFourCoreProjectionDetailAndBaselineMigrationVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/ItemFourCoreProjectionDetailAndBaselineMigration01_Assignment.md";
        private const string WorkbenchPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string BattleScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationSpec.csv";
        private const string LineagePath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailFieldLineage.csv";
        private const string SamplePath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailRealSampleMatrix.csv";
        private const string LedgerPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailBaselineMigrationLedger.csv";
        private const string ManualPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailManualTest.md";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationLeakCheckReport.md";
        private const string AssignmentHash =
            "fe496f2125379638a7a72040bd358a46fb12f83c216a152f64c20de8eb714c1a";
        private const string ProfileAggregate =
            "71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143";

        private static readonly ItemCoreAwakeningNodeKind[] FormalOrder =
        {
            ItemCoreAwakeningNodeKind.Core1,
            ItemCoreAwakeningNodeKind.Core2,
            ItemCoreAwakeningNodeKind.Core3,
            ItemCoreAwakeningNodeKind.Ultimate
        };

        private static readonly string[] ExistingWhitelist =
        {
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"
        };

        private static readonly string[] NewWhitelist =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs.meta",
            ReportPath,
            SpecPath,
            LineagePath,
            SamplePath,
            LedgerPath,
            ManualPath,
            LeakPath
        };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [AssignmentPath] = AssignmentHash,
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] = "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs"] = "599f9b1469cb4165a9e22a2dd5bf1c5cb683b67399fd347a0cdd4cd59c41a6f2",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] = "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] = "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] = "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] = "116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204",
                [BattleScenePath] = "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] = "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"] = "15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs"] = "a72c7aa63d04b411c171918c440be56ac39ac7f1046fe30e7a2ffc55bc0a4298",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs"] = "451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs"] = "f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs"] = "e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] = "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] = "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                [WorkbenchPath] = "d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed",
                ["Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv"] = "96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284",
                ["Docs/V0.4/Reports/ItemFourCoreAwakeningIdentityMatrix120.csv"] = "136d25056ad342aa7c396a21ed2b31f4f9bb9252dd15b69d39840a5006b11d45"
            };

        private static readonly IReadOnlyDictionary<string, string>
            HistoricalHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Docs/V0.4/ItemCoreAwakeningCore4NodeExpansion01_Assignment.md"] = "8f9af4b60b601f64a59fe8741c95792d8222857b66403843b70340ed46e7a03f",
                ["Docs/V0.4/ItemCoreEffectIdentityAndRuntimeStateContract01_Assignment.md"] = "34d4816ea5e43d64583767ddda6ae448295a8ec5fdeb7a5d7ba299b34c0db756",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs"] = "55963d1d5e2ac661ca7b2627778c92fd5f7e993d50bfa9bfcf28d918332050f0",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs"] = "c6513768d1be22c15594d790d494d77331b561a947974d7d38d74fa7aadfa479",
                ["Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv"] = "c5867237b758b71f90368e3c10c54a68b22d9b7f6ea549ae692df38826a13f32"
            };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<SampleRow> Samples = new();
        private static Fixture fixture;

        [MenuItem("Talisman Bag/V0.4/Verify Item Four Core Projection Detail And Baseline Migration")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            Samples.Clear();
            fixture = BuildFixture();

            Run("P3-01", "COMPONENT_FIXTURE_PASS",
                VerifyComponentAndAuthority);
            Run("P3-02", "FOUR_CORE_VISIBLE_COUNT_12334_PASS",
                VerifyRarityVisibility);
            Run("P3-03", "INVENTORY_BOARD_CORE_STATE_PASS",
                VerifyInventoryBoardTransitions);
            Run("P3-04", "REAL_RUNTIME_PATH_PASS",
                VerifyRealRuntimeSamples);
            Run("P3-05", "BUILD_TRACK_COEXISTENCE_PASS",
                VerifyBuildTrackCoexistence);
            Run("P3-06", "ITEMSANDBOX_FOUR_CORE_BASELINE_PASS",
                VerifyItemSandboxBaseline);
            Run("P3-07", "ULTIMATE_FOURTH_ROW_PASS",
                VerifyUltimateFourthRow);
            Run("P3-08", "UNKNOWN_INVALID_STALE_REJECTION_PASS",
                VerifyUnknownAndStaleRejection);
            Run("P3-09", "I031_EXCLUSION_PASS",
                VerifyI031Exclusion);
            Run("P3-10", "SCENE_AUTHORED_PANEL_BIND_PASS",
                VerifySceneAuthoredPanelBind);
            Run("P3-11", "CANONICAL_IMMUTABLE_PASS",
                VerifyCanonicalAndImmutability);
            Run("P3-12", "P1_P2_PROTECTED_HASHES_PASS",
                VerifyP1P2Hashes);
            Run("P3-13", "PRESENTATION_PROTECTED_HASHES_PASS",
                VerifyPresentationHashes);

            WriteReports();
            Run("P3-14", "LEAKCHECK_PASS",
                VerifyLeakCheck);
            Run("P3-15", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedDiffCheck);
            WriteReports();

            ScenarioResult[] failures = Results
                .Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "P3 verifier failed: "
                    + string.Join(
                        "; ",
                        failures.Select(value =>
                            value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[ItemFourCoreProjectionDetailAndBaselineMigrationVerifier]\n"
                + string.Join(
                    "\n",
                    Results.Select(value => value.Marker))
                + "\nUSER_HANDTEST_WAITING"
                + "\nPREFAB_MIGRATION_NOT_STARTED"
                + "\nDEV_COMPLETE"
                + "\nQA_STATIC_PASS"
                + "\nNEXT_PACKAGE_NOT_STARTED");
        }

        private static Fixture BuildFixture()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<
                    ItemBalanceWorkbenchCatalog>(WorkbenchPath);
            Check(workbench != null, "Workbench catalog missing.");
            GameObject providerObject =
                new("P3ProjectionProvider");
            providerObject.hideFlags =
                HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<
                        ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench, provider);
                Check(projection?.IsValid == true,
                    "BattleSandbox projection invalid: "
                    + string.Join(
                        "|",
                        projection?.Diagnostics ??
                        Array.Empty<string>()));
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity?.isValid == true,
                    "P2 identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    new(
                        ItemCoreEffectRosterCompleteness.Complete,
                        projection.OrdinaryProjectionSet.Projections
                            .Select(value =>
                                new ItemCoreEffectCultivationRow(
                                    value.itemInstanceId,
                                    value.baseItemId,
                                    40,
                                    ItemSystemBattleSandboxBoardAdapter
                                        .CultivationSourceKey,
                                    ItemCoreEffectFactCompleteness
                                        .Complete)));
                Check(cultivation.isValid,
                    "P2 cultivation roster invalid.");
                return new Fixture(
                    workbench,
                    projection,
                    identity,
                    cultivation);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(
                    providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority
            NewAuthority()
        {
            return new ItemSystemBattleSandboxBoardAuthority(
                fixture.Projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                fixture.Identity,
                fixture.Cultivation,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static void VerifyComponentAndAuthority()
        {
            ItemBalanceProfile[] profiles =
                (fixture.Workbench.profiles ??
                    new List<ItemBalanceProfile>())
                .Where(value => value != null)
                .OrderBy(value => value.baseItemId,
                    StringComparer.Ordinal)
                .ToArray();
            Check(profiles.Length == 30
                && profiles.Sum(value =>
                    (value.coreCandidates ??
                        new List<ItemBalanceCoreCandidate>())
                    .Count(candidate => candidate != null)) == 120,
                "P1 Candidate authority is not 30x4.");
            Check(fixture.Identity.schemaId ==
                    ItemCoreEffectIdentityCatalogSnapshot
                        .CurrentSchemaId
                && fixture.Identity.Rows.Count == 120
                && fixture.Identity.Rows.All(value =>
                    value.nodeKind !=
                        ItemCoreAwakeningNodeKind.Core4),
                "P2 identity authority is not v2/120/four-core.");
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            Check(authority.CurrentCoreEffectRuntimeState.isValid
                && authority.CurrentCoreEffectRuntimeState.schemaId ==
                    ItemInstanceCoreEffectRuntimeStateSnapshot
                        .CurrentSchemaId
                && authority.CurrentCoreEffectRuntimeState.Items.Count ==
                    30
                && authority.CurrentCoreEffectRuntimeState.Items.All(
                    value => value.CoreEffectRows.Count == 4
                        && value.CoreEffectRows.All(row =>
                            row.nodeKind !=
                                ItemCoreAwakeningNodeKind.Core4)),
                "Real P2 runtime authority is not v2/30x4.");
        }

        private static void VerifyRarityVisibility()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            (string key, int count)[] cases =
            {
                ("white", 1),
                ("green", 2),
                ("blue", 3),
                ("purple", 3),
                ("orange", 4)
            };
            foreach ((string key, int count) in cases)
            {
                ItemBalanceCandidateDetailResult candidate =
                    Candidate("I001", key);
                ItemInstanceCoreEffectRuntimeStateSnapshot runtime =
                    BuildInventoryRuntime(
                        candidate.detailProjection.projection,
                        authority.CurrentSnapshot);
                ItemDetailQualifiedCoreEffectProjectionResult result =
                    ItemDetailQualifiedCoreEffectProjector.Project(
                        candidate.viewModel,
                        candidate.baseItemId,
                        candidate.detailProjection.projection
                            .itemInstanceId,
                        string.Empty,
                        authority.CurrentSnapshot,
                        runtime);
                Check(result.IsSuccess
                    && result.ViewModel.displayCoreEffects.Count ==
                        count
                    && result.ViewModel.qualifiedCoreEffect.Rows.Count ==
                        4,
                    key + " visible row count mismatch.");
                Samples.Add(new SampleRow(
                    "Rarity-" + key,
                    candidate.baseItemId,
                    "Inventory",
                    result.ViewModel.qualifiedBuildTrack.status
                        .ToString(),
                    result.ViewModel.qualifiedCoreEffect.status
                        .ToString(),
                    result.ViewModel.displayCoreEffects.Count,
                    result.ViewModel.qualifiedCoreEffect
                        .canonicalSignature));
            }
        }

        private static void VerifyInventoryBoardTransitions()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            ItemSystemBattleSandboxViewRow row =
                FindRow("I001");
            using RuntimePanel fixturePanel =
                new(fixture.Projection.Rows);

            ItemDetailViewModel inventory =
                fixturePanel.Show(row, string.Empty, authority);
            Check(inventory.displayCoreEffects.Count == 4
                && inventory.qualifiedCoreEffect.location ==
                    ItemCoreEffectRuntimeLocation.Inventory
                && inventory.qualifiedCoreEffect.Rows.All(value =>
                    value.litFact ==
                        ItemCoreEffectBooleanFact.NotApplicable
                    && value.activeFact ==
                        ItemCoreEffectBooleanFact.NotApplicable)
                && inventory.displayCoreEffects.All(value =>
                    !string.IsNullOrWhiteSpace(value.title)
                    && !string.IsNullOrWhiteSpace(value.body)),
                "Inventory static/fact presentation mismatch.");
            Record("Inventory", inventory);

            PlacementCandidate unlit =
                FindCandidate(authority, "I001", false);
            Check(authority.CommitFromTray(
                    "I001",
                    unlit.Anchor,
                    unlit.Rotation).Accepted,
                "I001 unlit placement failed.");
            ItemDetailViewModel boardUnlit =
                fixturePanel.Show(
                    row, "P_BOARD_I001", authority);
            Check(boardUnlit.displayCoreEffects.Count == 4
                && boardUnlit.qualifiedCoreEffect.location ==
                    ItemCoreEffectRuntimeLocation.Board
                && boardUnlit.qualifiedCoreEffect.Rows.All(value =>
                    value.litFact ==
                        ItemCoreEffectBooleanFact.KnownFalse
                    && value.activeFact ==
                        ItemCoreEffectBooleanFact.KnownFalse)
                && CoreStates(boardUnlit).All(value =>
                    value ==
                    ItemDetailCoreEffectRowState
                        .UnlockedInactive),
                "Board unlit presentation mismatch.");
            Record("BoardUnlit", boardUnlit);

            Check(authority.CommitFromTray(
                    "I031",
                    new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 source placement failed.");
            PlacementCandidate lit =
                FindCandidate(authority, "I001", true);
            Check(authority.CommitMove(
                    "P_BOARD_I001",
                    lit.Anchor,
                    lit.Rotation).Accepted,
                "I001 move-to-lit failed.");
            ItemDetailViewModel boardLit =
                fixturePanel.Show(
                    row, "P_BOARD_I001", authority);
            Check(boardLit.qualifiedCoreEffect.Rows.All(value =>
                    value.activeFact ==
                        ItemCoreEffectBooleanFact.KnownTrue)
                && CoreStates(boardLit).All(value =>
                    value ==
                    ItemDetailCoreEffectRowState.Active),
                "Board lit facts were not projected directly.");
            Record("BoardLitAfterMove", boardLit);

            Check(authority.ReturnToTray(
                    "P_BOARD_I001").Accepted,
                "I001 return failed.");
            ItemDetailViewModel returned =
                fixturePanel.Show(row, string.Empty, authority);
            Check(returned.placementId == string.Empty
                && returned.qualifiedCoreEffect.placementId == null
                && returned.qualifiedCoreEffect.location ==
                    ItemCoreEffectRuntimeLocation.Inventory
                && returned.qualifiedCoreEffect.Rows.All(value =>
                    value.litFact ==
                        ItemCoreEffectBooleanFact.NotApplicable
                    && value.activeFact ==
                        ItemCoreEffectBooleanFact.NotApplicable),
                "Return retained stale placement/lit/active state.");
            Record("ReturnToInventory", returned);
        }

        private static void VerifyRealRuntimeSamples()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            using RuntimePanel panel =
                new(fixture.Projection.Rows);
            foreach (string baseItemId in new[]
                     {
                         "I001", "I004", "I006", "I009"
                     })
            {
                ItemDetailViewModel model =
                    panel.Show(
                        FindRow(baseItemId),
                        string.Empty,
                        authority);
                Check(model.qualifiedCoreEffect.status ==
                        ItemDetailQualifiedCoreEffectProjectionStatus
                            .Complete
                    && model.displayCoreEffects.Count == 4
                    && model.qualifiedCoreEffect
                        .sourceRuntimeCanonicalSignature ==
                        authority.CurrentCoreEffectRuntimeState
                            .canonicalSignature,
                    baseItemId
                    + " did not consume current real runtime.");
                Record(baseItemId + "-Inventory", model);
            }
            string controller = File.ReadAllText(ProjectPath(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"));
            Check(controller.Contains(
                    "itemSystemBoardAuthority.CurrentSnapshot",
                    StringComparison.Ordinal)
                && controller.Contains(
                    "itemSystemBoardAuthority.CurrentQualifiedBuildState",
                    StringComparison.Ordinal)
                && controller.Contains(
                    "itemSystemBoardAuthority.CurrentCoreEffectRuntimeState",
                    StringComparison.Ordinal),
                "Controller does not pass all three current snapshots.");
        }

        private static void VerifyBuildTrackCoexistence()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            using RuntimePanel panel =
                new(fixture.Projection.Rows);
            (string item, ItemBuildQualification qualification,
                int tracks)[] cases =
            {
                ("I001", ItemBuildQualification.QiLeiOnly, 1),
                ("I004", ItemBuildQualification.Dual, 2),
                ("I006", ItemBuildQualification.None, 0),
                ("I009", ItemBuildQualification.FaMenOnly, 1)
            };
            foreach ((string item,
                         ItemBuildQualification qualification,
                         int tracks) in cases)
            {
                ItemDetailViewModel model = panel.Show(
                    FindRow(item), string.Empty, authority);
                Check(model.qualifiedBuildTrack.status ==
                        ItemDetailQualifiedBuildProjectionStatus
                            .Complete
                    && model.qualifiedBuildTrack
                        .buildQualification == qualification
                    && model.qualifiedBuildTrack.TrackRows.Count ==
                        tracks
                    && model.qualifiedCoreEffect.status ==
                        ItemDetailQualifiedCoreEffectProjectionStatus
                            .Complete
                    && model.qualifiedCoreEffect.Rows.Count == 4,
                    item + " Build/core coexistence mismatch.");
            }
        }

        private static void VerifyItemSandboxBaseline()
        {
            ItemBalanceCandidateDetailResult orange =
                Candidate("I001", "orange");
            string[] ids = orange.viewModel.displayCoreEffects
                .Select(value => value.stateKey).ToArray();
            string prefix = "candidate_core_i001";
            Check(ids.SequenceEqual(
                    new[]
                    {
                        prefix + "_01",
                        prefix + "_02",
                        prefix + "_03",
                        prefix + "_ultimate"
                    },
                    StringComparer.Ordinal),
                "ItemSandbox rows are not exact 01/02/03/Ultimate.");
            Check(orange.viewModel.displayCoreEffects.All(value =>
                    !value.stateKey.Contains(
                        "_04", StringComparison.Ordinal))
                && !(orange.viewModel.displayOrangeAffix ??
                    string.Empty).Contains(
                    orange.viewModel.displayCoreEffects[3].title,
                    StringComparison.Ordinal)
                && orange.viewModel.displayPlayerSections.All(
                    value => value == null
                        || !(value.title ?? string.Empty).Contains(
                            "终极核心",
                            StringComparison.Ordinal)),
                "Ultimate remains coupled to orange/Dao presentation.");

            string workbench = File.ReadAllText(ProjectPath(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs"));
            Check(workbench.Contains(
                    "FallbackNodeLevels = { 10, 20, 30, 40 }",
                    StringComparison.Ordinal)
                && workbench.Contains(
                    "prefix + \"_ultimate\"",
                    StringComparison.Ordinal)
                && !workbench.Contains(
                    "index >= 4",
                    StringComparison.Ordinal)
                && !workbench.Contains(
                    "(index + 1) * 10",
                    StringComparison.Ordinal)
                && !workbench.Contains(
                    "FallbackNodeLevels = { 10, 20, 30, 40, 40 }",
                    StringComparison.Ordinal),
                "Workbench still contains positional five-node drift.");
        }

        private static void VerifyUltimateFourthRow()
        {
            ItemBalanceCandidateDetailResult orange =
                Candidate("I004", "orange");
            ItemDetailTextLine fourth =
                orange.viewModel.displayCoreEffects.ElementAtOrDefault(3);
            Check(orange.viewModel.displayCoreEffects.Count == 4
                && fourth != null
                && string.Equals(
                    fourth.stateKey,
                    "candidate_core_i004_ultimate",
                    StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(fourth.title)
                && !string.IsNullOrWhiteSpace(fourth.body),
                "Ultimate is not the authored fourth core row.");
        }

        private static void VerifyUnknownAndStaleRejection()
        {
            ItemSystemBattleSandboxBoardAuthority unknownAuthority =
                new(
                    fixture.Projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    fixture.Identity,
                    null,
                    DefaultRealLayoutResilienceEvaluationPipeline
                        .Instance);
            using RuntimePanel panel =
                new(fixture.Projection.Rows);
            ItemSystemBattleSandboxViewRow row =
                FindRow("I001");
            ItemDetailViewModel unknown =
                panel.Show(row, string.Empty, unknownAuthority);
            Check(unknown.qualifiedCoreEffect.status ==
                    ItemDetailQualifiedCoreEffectProjectionStatus
                        .Unknown
                && unknown.qualifiedCoreEffect.cultivationLevel ==
                    null
                && unknown.displayCoreEffects.Count == 0
                && string.Equals(
                    unknown.displayAwakeningStatusText,
                    ItemDetailQualifiedCoreEffectProjector
                        .SafeUnavailableText,
                    StringComparison.Ordinal)
                && unknown.qualifiedCoreEffect.Rows.All(value =>
                    value.unlockedFact ==
                        ItemCoreEffectBooleanFact.Unknown
                    && value.activeFact ==
                        ItemCoreEffectBooleanFact.Unknown),
                "Unknown became a fake zero/false/locked state.");

            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            ItemInstanceCoreEffectRuntimeStateSnapshot stale =
                authority.CurrentCoreEffectRuntimeState;
            PlacementCandidate unlit =
                FindCandidate(authority, "I001", false);
            Check(authority.CommitFromTray(
                    "I001",
                    unlit.Anchor,
                    unlit.Rotation).Accepted,
                "Stale fixture placement failed.");
            Check(!panel.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    "P_BOARD_I001",
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    stale)
                && !panel.Adapter.IsVisible
                && panel.Adapter.LastProjectedModel == null,
                "Stale runtime canonical was not rejected.");
            Check(!panel.Adapter.Show(
                    row.BaseItemId,
                    "wrong-instance",
                    "P_BOARD_I001",
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    authority.CurrentCoreEffectRuntimeState)
                && panel.Adapter.LastProjectedModel == null,
                "Stale instance identity was not rejected.");
            Check(!panel.Adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    "wrong-placement",
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    authority.CurrentCoreEffectRuntimeState)
                && panel.Adapter.LastProjectedModel == null,
                "Stale placement identity was not rejected.");
        }

        private static void VerifyI031Exclusion()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            using RuntimePanel panel =
                new(fixture.Projection.Rows);
            ItemDetailViewModel system = panel.Show(
                FindRow("I031"), string.Empty, authority);
            Check(system.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus
                        .NotApplicable
                && system.qualifiedCoreEffect.status ==
                    ItemDetailQualifiedCoreEffectProjectionStatus
                        .NotApplicable
                && system.qualifiedCoreEffect.Rows.Count == 0
                && system.displayCoreEffects.Count == 0
                && authority.CurrentCoreEffectRuntimeState.Items.All(
                    value => !string.Equals(
                        value.baseItemId,
                        "I031",
                        StringComparison.Ordinal)),
                "I031 received fabricated ordinary core rows.");
        }

        private static void VerifySceneAuthoredPanelBind()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(
                    BattleScenePath,
                    OpenSceneMode.Additive);
                ItemDetailPanelView[] views =
                    scene.GetRootGameObjects()
                    .SelectMany(value =>
                        value.GetComponentsInChildren<
                            ItemDetailPanelView>(true))
                    .Where(value => value != null)
                    .ToArray();
                Check(views.Length == 1,
                    "Scene does not contain exactly one authored panel.");
                ItemSystemBattleSandboxItemDetailAdapter adapter =
                    new();
                Check(adapter.Initialize(
                        views[0],
                        fixture.Projection.Rows),
                    "Scene panel adapter initialize failed: "
                    + adapter.LastDiagnosticCode);
                ItemSystemBattleSandboxBoardAuthority authority =
                    NewAuthority();
                ItemSystemBattleSandboxViewRow row =
                    FindRow("I001");
                Check(adapter.Show(
                        row.BaseItemId,
                        row.ItemInstanceId,
                        string.Empty,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState,
                        authority.CurrentCoreEffectRuntimeState),
                    "Scene-authored Bind failed: "
                    + adapter.LastDiagnosticCode);
                ItemDetailViewModel model =
                    adapter.LastProjectedModel;
                Check(model != null
                    && model.displayCoreEffects.Count == 4
                    && model.displayCoreEffects.All(value =>
                        !string.IsNullOrWhiteSpace(value.title)
                        && !string.IsNullOrWhiteSpace(value.body))
                    && CoreStates(model).Count == 4,
                    "Scene-authored Bind lost core content/state.");
                adapter.Uninstall();
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(
                        scene, true);
                }
            }
        }

        private static void VerifyCanonicalAndImmutability()
        {
            ItemSystemBattleSandboxBoardAuthority authority =
                NewAuthority();
            using RuntimePanel panel =
                new(fixture.Projection.Rows);
            ItemDetailViewModel model = panel.Show(
                FindRow("I001"), string.Empty, authority);
            ItemDetailQualifiedCoreEffectProjection source =
                model.qualifiedCoreEffect;
            ItemDetailQualifiedCoreEffectProjection reversed =
                new(
                    source.status,
                    source.completeness,
                    source.itemInstanceId,
                    source.baseItemId,
                    source.placementId,
                    source.location,
                    source.rarity,
                    source.cultivationLevel,
                    source.cultivationLevelSource,
                    source.cultivationFactCompleteness,
                    source.isLitFact,
                    source.Rows.Reverse(),
                    source.diagnosticCode,
                    source.safeUnavailableReason,
                    source.sourceRuntimeCanonicalSignature);
            ItemDetailViewModel clone = model.Clone();
            Check(source.canonicalSignature ==
                    reversed.canonicalSignature
                && clone.qualifiedCoreEffect
                    .canonicalSignature ==
                    source.canonicalSignature
                && !ReferenceEquals(
                    clone.qualifiedCoreEffect,
                    source)
                && !ReferenceEquals(
                    clone.qualifiedCoreEffect.Rows,
                    source.Rows)
                && MutationBlocked(source.Rows)
                && MutationBlocked(
                    source.Rows[0].SourceIdentities),
                "Canonical order or immutable collection contract failed.");
        }

        private static void VerifyP1P2Hashes()
        {
            foreach (KeyValuePair<string, string> pair in
                     ProtectedHashes.Where(value =>
                         value.Key.Contains(
                             "/Items/Balance/",
                             StringComparison.Ordinal)
                         || value.Key.Contains(
                             "/Items/Awakening/",
                             StringComparison.Ordinal)
                         || value.Key.Contains(
                             "/Items/Build/Qualified/",
                             StringComparison.Ordinal)
                         || string.Equals(
                             value.Key,
                             WorkbenchPath,
                             StringComparison.Ordinal)
                         || value.Key.Contains(
                             "ItemCandidateCoreEffects120",
                             StringComparison.Ordinal)
                         || value.Key.Contains(
                             "ItemFourCoreAwakeningIdentityMatrix120",
                             StringComparison.Ordinal)))
            {
                VerifyHash(pair);
            }
            string profilesRoot = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] profiles = Directory.GetFiles(
                    profilesRoot,
                    "*.asset",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(
                    value => Path.GetFileName(value),
                    StringComparer.Ordinal)
                .ToArray();
            string aggregate = Sha256Text(string.Join(
                "\n",
                profiles.Select(value =>
                    Path.GetFileName(value)
                    + "|" + Sha256File(value))));
            Check(profiles.Length == 30
                && string.Equals(
                    aggregate,
                    ProfileAggregate,
                    StringComparison.Ordinal),
                "P1 profile aggregate changed.");
        }

        private static void VerifyPresentationHashes()
        {
            foreach (KeyValuePair<string, string> pair in
                     ProtectedHashes.Where(value =>
                         !value.Key.Contains(
                             "/Items/Balance/",
                             StringComparison.Ordinal)
                         && !value.Key.Contains(
                             "/Items/Awakening/",
                             StringComparison.Ordinal)
                         && !value.Key.Contains(
                             "/Items/Build/Qualified/",
                             StringComparison.Ordinal)
                         && !string.Equals(
                             value.Key,
                             WorkbenchPath,
                             StringComparison.Ordinal)
                         && !value.Key.Contains(
                             "ItemCandidateCoreEffects120",
                             StringComparison.Ordinal)
                         && !value.Key.Contains(
                             "ItemFourCoreAwakeningIdentityMatrix120",
                             StringComparison.Ordinal)))
            {
                VerifyHash(pair);
            }
            foreach (KeyValuePair<string, string> pair in
                     HistoricalHashes)
            {
                VerifyHash(pair);
            }
        }

        private static void VerifyLeakCheck()
        {
            Check(ExistingWhitelist.Concat(NewWhitelist)
                    .All(value => File.Exists(
                        ProjectPath(value))),
                "Exact P3 whitelist is incomplete.");
            Check(!Directory.GetFiles(
                    ProjectPath("Docs/V0.4/Reports"),
                    "ItemFourCoreProjectionDetail*.meta",
                    SearchOption.TopDirectoryOnly).Any(),
                "Docs report .meta side effect exists.");
            string projector = File.ReadAllText(ProjectPath(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs"));
            foreach (string forbidden in new[]
                     {
                         "SaveData",
                         "RunFlow",
                         "Reward",
                         "EnemySystem",
                         "BoneAspect",
                         "EncounterPresentation",
                         "ResolveActiveFact",
                         "using TalismanBag.Items.Detail.UI"
                     })
            {
                Check(!projector.Contains(
                        forbidden,
                        StringComparison.Ordinal),
                    "Forbidden projector dependency: "
                    + forbidden);
            }
            Check(ExistingWhitelist.Concat(NewWhitelist)
                    .All(value =>
                        !value.Contains(
                            "EnemySystem",
                            StringComparison.Ordinal)
                        && !value.Contains(
                            "BoneAspect",
                            StringComparison.Ordinal)
                        && !value.EndsWith(
                            ".unity",
                            StringComparison.Ordinal)
                        && !value.EndsWith(
                            ".prefab",
                            StringComparison.Ordinal)),
                "Presentation/parallel path leaked into whitelist.");
        }

        private static void VerifyPackageScopedDiffCheck()
        {
            foreach (string path in ExistingWhitelist.Concat(
                         NewWhitelist))
            {
                string[] lines = File.ReadAllText(
                        ProjectPath(path))
                    .Replace("\r\n", "\n")
                    .Split('\n');
                Check(lines.All(value =>
                        !value.EndsWith(
                            " ", StringComparison.Ordinal)
                        && !value.EndsWith(
                            "\t", StringComparison.Ordinal)
                        && !value.StartsWith(
                            "<<<<<<<",
                            StringComparison.Ordinal)
                        && !value.StartsWith(
                            ">>>>>>>",
                            StringComparison.Ordinal)),
                    "Whitespace/merge marker issue: " + path);
            }

            ProcessStartInfo start = new()
            {
                FileName = "git",
                WorkingDirectory = ProjectPath(string.Empty),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            start.ArgumentList.Add("diff");
            start.ArgumentList.Add("--check");
            start.ArgumentList.Add("--");
            foreach (string path in ExistingWhitelist)
            {
                start.ArgumentList.Add(path);
            }
            using Process process = Process.Start(start);
            string output = process.StandardOutput.ReadToEnd()
                + process.StandardError.ReadToEnd();
            process.WaitForExit();
            Check(process.ExitCode == 0,
                "git diff --check failed: " + output);
        }

        private static ItemBalanceCandidateDetailResult Candidate(
            string baseItemId,
            string rarityKey)
        {
            GameObject providerObject =
                new("P3CandidateProvider_" + rarityKey);
            providerObject.hideFlags =
                HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<
                        ItemInnerDataCatalogProvider>();
                ItemBalanceCandidateDetailSandboxAdapter adapter =
                    new(fixture.Workbench, provider);
                ItemBalanceCandidateDetailResult result =
                    adapter.Request(
                        new ItemBalanceCandidateDetailRequest
                        {
                            baseItemId = baseItemId,
                            rarityKey = rarityKey,
                            rootSeedText = "404319001"
                        });
                Check(result?.isSuccess == true,
                    baseItemId + "/" + rarityKey
                    + " Candidate detail failed.");
                return result;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(
                    providerObject);
            }
        }

        private static ItemInstanceCoreEffectRuntimeStateSnapshot
            BuildInventoryRuntime(
                ItemInstanceProjectionContractSnapshot projection,
                ItemSystemSnapshot itemSystem)
        {
            ItemCoreEffectIdentityRow[] identities =
                fixture.Identity.Rows.Where(value =>
                    string.Equals(
                        value.baseItemId,
                        projection.baseItemId,
                        StringComparison.Ordinal))
                .OrderBy(value => NodeRank(value.nodeKind))
                .ToArray();
            ItemInstanceCoreEffectRuntimeRow[] rows =
                identities.Select(value =>
                    new ItemInstanceCoreEffectRuntimeRow(
                        projection.baseItemId,
                        projection.itemInstanceId,
                        value.nodeKind,
                        value.candidateDefinitionId,
                        value.awakeningNodeId,
                        value.displayName,
                        value.description,
                        value.awakeningUnlockLevel,
                        value.requiredRarity,
                        value.requiredRarityKey,
                        Fact(projection.EligibleCoreEffectIds
                            .Contains(
                                value.candidateDefinitionId,
                                StringComparer.Ordinal)),
                        Fact(projection.VisibleCoreEffectIds
                            .Contains(
                                value.candidateDefinitionId,
                                StringComparer.Ordinal)),
                        ItemCoreEffectBooleanFact.KnownTrue,
                        ItemCoreEffectBooleanFact.NotApplicable,
                        ItemCoreEffectBooleanFact.NotApplicable,
                        ItemCoreEffectFactCompleteness.Complete,
                        new[]
                        {
                            value.candidateSourceIdentity,
                            value.awakeningSourceIdentity,
                            value.candidatePayloadIdentity
                        })).ToArray();
            ItemInstanceCoreEffectRuntimeItemSnapshot item =
                new(
                    projection.itemInstanceId,
                    projection.baseItemId,
                    projection.rarity,
                    ItemCoreEffectRuntimeLocation.Inventory,
                    null,
                    40,
                    ItemSystemBattleSandboxBoardAdapter
                        .CultivationSourceKey,
                    ItemCoreEffectFactCompleteness.Complete,
                    ItemCoreEffectBooleanFact.NotApplicable,
                    rows,
                    ItemCoreEffectFactCompleteness.Complete);
            ItemInstanceCoreEffectRuntimeStateSnapshot runtime =
                new(
                    ItemCoreEffectRuntimeStateStatus.Valid,
                    ItemCoreEffectRosterCompleteness.Complete,
                    fixture.Identity.canonicalSignature,
                    fixture.Cultivation.canonicalSignature,
                    null,
                    null,
                    Sha256Text(itemSystem.BuildDebugSignature()),
                    new[] { item },
                    Array.Empty<
                        ItemCoreEffectRuntimeStateValidationError>());
            Check(runtime.isValid,
                "Component runtime fixture invalid: "
                + string.Join(
                    "|",
                    runtime.ValidationErrors.Select(value =>
                        value.code)));
            return runtime;
        }

        private static ItemSystemBattleSandboxViewRow FindRow(
            string baseItemId)
        {
            ItemSystemBattleSandboxViewRow[] rows =
                fixture.Projection.Rows.Where(value => value != null
                    && string.Equals(
                        value.BaseItemId,
                        baseItemId,
                        StringComparison.Ordinal)).ToArray();
            Check(rows.Length == 1,
                "View row cardinality mismatch: " + baseItemId);
            return rows[0];
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
                        baseItemId,
                        new ItemShapeCell(x, y),
                        rotation);
                ItemSystemPlacementSnapshot placement =
                    preview.Snapshot?.FindPlacement(
                        string.Equals(
                            baseItemId,
                            "I031",
                            StringComparison.Ordinal)
                            ? "P_SYSTEM_I031"
                            : "P_BOARD_" + baseItemId);
                if (preview.Accepted
                    && placement != null
                    && (string.Equals(
                            baseItemId,
                            "I031",
                            StringComparison.Ordinal)
                        || placement.isLit == requireLit))
                {
                    return new PlacementCandidate(
                        new ItemShapeCell(x, y),
                        rotation);
                }
            }
            throw new InvalidOperationException(
                "No legal "
                + (requireLit ? "lit" : "unlit")
                + " placement for " + baseItemId);
        }

        private static IReadOnlyList<ItemDetailCoreEffectRowState>
            CoreStates(ItemDetailViewModel model)
        {
            return (model.displayPlayerSections ??
                    new List<ItemDetailSectionViewModel>())
                .Single(value => value != null
                    && string.Equals(
                        value.stateKey,
                        "coreEffect",
                        StringComparison.Ordinal))
                .coreEffectRowStates;
        }

        private static void Record(
            string scenario,
            ItemDetailViewModel model)
        {
            Samples.Add(new SampleRow(
                scenario,
                model.baseItemId,
                model.qualifiedCoreEffect.location.ToString(),
                model.qualifiedBuildTrack.status.ToString(),
                model.qualifiedCoreEffect.status.ToString(),
                model.displayCoreEffects.Count,
                model.qualifiedCoreEffect.canonicalSignature));
        }

        private static void WriteReports()
        {
            string resultLines = string.Join(
                "\n",
                Results.Select(value =>
                    "- " + value.Marker + ": "
                    + (value.Passed ? "PASS" : "FAIL")
                    + " — " + value.Detail));
            File.WriteAllText(
                ProjectPath(ReportPath),
                "# Item Four Core Projection Detail And Baseline Migration\n\n"
                + "- Package: `V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01`\n"
                + "- Assignment SHA-256: `" + AssignmentHash + "`\n"
                + "- Result: `DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PATH_PASS / USER_HANDTEST_WAITING`\n"
                + "- Prefab migration: `NOT_STARTED`\n\n"
                + "## Verifier results\n\n"
                + resultLines + "\n\n"
                + "The P3 projector is immutable and read-only. It consumes P2 facts, preserves the accepted Build projection, and does not create a second runtime owner.\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(SpecPath),
                "id,marker,result,detail\n"
                + string.Join(
                    "\n",
                    Results.Select(value =>
                        Csv(value.Id) + ","
                        + Csv(value.Marker) + ","
                        + Csv(value.Passed ? "PASS" : "FAIL")
                        + "," + Csv(value.Detail)))
                + "\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(LineagePath),
                "outputField,sourceAuthority,join,transformation\n"
                + "qualifiedCoreEffect,ItemInstanceCoreEffectRuntimeStateSnapshot.v2,itemInstanceId+baseItemId+placementId/location,immutable read-only copy\n"
                + "displayCoreEffects,Runtime CoreEffectRows,visibleFact KnownTrue in explicit Core1/Core2/Core3/Ultimate order,displayName+description only\n"
                + "coreEffectRowStates,Runtime unlockedFact+activeFact,exact projected row identity,Known facts mapped to existing row-state enum\n"
                + "qualifiedBuildTrack,ItemInstanceQualifiedBuildStateSnapshot.v1,unchanged accepted projector output,preserved clone\n"
                + "LastProjectedModel,CurrentSnapshot+CurrentQualifiedBuildState+CurrentCoreEffectRuntimeState,one adapter call,fail closed on stale identity/canonical\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(SamplePath),
                "scenario,baseItemId,location,buildStatus,coreStatus,visibleRows,coreCanonical\n"
                + string.Join(
                    "\n",
                    Samples.Select(value =>
                        Csv(value.Scenario) + ","
                        + Csv(value.BaseItemId) + ","
                        + Csv(value.Location) + ","
                        + Csv(value.BuildStatus) + ","
                        + Csv(value.CoreStatus) + ","
                        + value.VisibleRows.ToString(
                            CultureInfo.InvariantCulture)
                        + "," + Csv(value.Canonical)))
                + "\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(LedgerPath),
                "evidence,status,retention,execution\n"
                + "V0.4-ItemCoreAwakeningCore4NodeExpansion01,SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C,HISTORICAL_EVIDENCE_RETAINED,NOT_EXECUTED_BY_P3\n"
                + "V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01,SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C,HISTORICAL_EVIDENCE_RETAINED,NOT_EXECUTED_BY_P3\n"
                + "CoreAwakeningPreviewVerifier,SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C,HISTORICAL_EVIDENCE_RETAINED,NOT_EXECUTED_BY_P3\n"
                + "ItemCoreEffectIdentityAndRuntimeStateContractVerifier,SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C,HISTORICAL_EVIDENCE_RETAINED,NOT_EXECUTED_BY_P3\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(ManualPath),
                "# P3 BattleSandbox manual test\n\n"
                + "Status: `USER_HANDTEST_WAITING`\n\n"
                + "Open `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`, enter Play Mode, then:\n\n"
                + "1. Click Inventory I001: four Orange core rows are unlocked/inactive and QiLei Build remains visible.\n"
                + "2. Place I001 unlit: the same four rows remain unlocked/inactive.\n"
                + "3. Place or move I031 so I001 is lit: all four rows become active and Build progress refreshes.\n"
                + "4. Move I001 out of lighting: rows return to unlocked/inactive.\n"
                + "5. Return I001 to Inventory: placement/lit/active residue is absent.\n"
                + "6. Click I004: Dual Build tracks and four core rows coexist.\n"
                + "7. Click I006: Build is None/Known Zero and four core rows remain.\n"
                + "8. Click I009: FaMen-only Build and four core rows coexist.\n"
                + "9. Click I031: system detail has no ordinary Build/core rows.\n"
                + "10. Rapidly alternate I001/I004/I006/I009/I031 and confirm no stale row/text/icon state.\n\n"
                + "Do not start Prefab migration from this checklist.\n",
                new UTF8Encoding(false));

            File.WriteAllText(
                ProjectPath(LeakPath),
                "# P3 LeakCheck\n\n"
                + "- Exact existing whitelist: 5 files\n"
                + "- Exact new implementation/verifier files: 4 files\n"
                + "- Exact reports: 7 files\n"
                + "- Scene/Prefab/View/Composer/Build projector/BoardAuthority: protected hashes\n"
                + "- P1/P2 authority: protected hashes and profile aggregate\n"
                + "- EnemySystem/BoneAspect/EncounterPresentation: external read-only\n"
                + "- Docs `.meta`: forbidden\n"
                + "- Builder/historical verifier execution: none\n"
                + "- Prefab migration: `NOT_STARTED`\n"
                + "- Result: `" + (Results.Any(value =>
                    !value.Passed) ? "FAIL" : "PASS") + "`\n",
                new UTF8Encoding(false));
        }

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(
                    id, marker, true, "PASS"));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id,
                    marker,
                    false,
                    exception.GetType().Name + ": "
                    + exception.Message));
            }
        }

        private static void Check(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void VerifyHash(
            KeyValuePair<string, string> pair)
        {
            string absolute = ProjectPath(pair.Key);
            Check(File.Exists(absolute),
                "Protected path missing: " + pair.Key);
            Check(string.Equals(
                    Sha256File(absolute),
                    pair.Value,
                    StringComparison.Ordinal),
                "Protected hash mismatch: " + pair.Key);
        }

        private static bool MutationBlocked<T>(
            IReadOnlyList<T> values)
        {
            try
            {
                ((IList<T>)values).Add(default);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static ItemCoreEffectBooleanFact Fact(
            bool value) =>
            value
                ? ItemCoreEffectBooleanFact.KnownTrue
                : ItemCoreEffectBooleanFact.KnownFalse;

        private static int NodeRank(
            ItemCoreAwakeningNodeKind kind) =>
            kind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => 0,
                ItemCoreAwakeningNodeKind.Core2 => 1,
                ItemCoreAwakeningNodeKind.Core3 => 2,
                ItemCoreAwakeningNodeKind.Ultimate => 3,
                _ => int.MaxValue
            };

        private static string ProjectPath(
            string relative)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                (relative ?? string.Empty).Replace(
                    '/', Path.DirectorySeparatorChar)));
        }

        private static string Sha256File(
            string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(
                sha.ComputeHash(stream).Select(value =>
                    value.ToString(
                        "x2",
                        CultureInfo.InvariantCulture)));
        }

        private static string Sha256Text(
            string value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(
                sha.ComputeHash(
                    Encoding.UTF8.GetBytes(
                        value ?? string.Empty))
                .Select(item => item.ToString(
                    "x2",
                    CultureInfo.InvariantCulture)));
        }

        private static string Csv(
            string value) =>
            "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"")
                .Replace("\r", " ")
                .Replace("\n", " ") + "\"";

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

        private sealed class RuntimePanel : IDisposable
        {
            private readonly Scene scene;

            public RuntimePanel(
                IReadOnlyList<ItemSystemBattleSandboxViewRow> rows)
            {
                scene = EditorSceneManager.NewPreviewScene();
                GameObject popup = new(
                    "PopupLayer",
                    typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(
                    popup, scene);
                GameObject panelObject = new(
                    "ItemDetailPanel",
                    typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(
                    panelObject, scene);
                panelObject.transform.SetParent(
                    popup.transform, false);
                ItemDetailPanelView panel =
                    panelObject.AddComponent<
                        ItemDetailPanelView>();
                Adapter =
                    new ItemSystemBattleSandboxItemDetailAdapter();
                Check(Adapter.Initialize(panel, rows),
                    "Runtime adapter initialize failed: "
                    + Adapter.LastDiagnosticCode);
            }

            public ItemSystemBattleSandboxItemDetailAdapter
                Adapter { get; }

            public ItemDetailViewModel Show(
                ItemSystemBattleSandboxViewRow row,
                string placementId,
                ItemSystemBattleSandboxBoardAuthority authority)
            {
                Check(Adapter.Show(
                        row.BaseItemId,
                        row.ItemInstanceId,
                        placementId,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState,
                        authority.CurrentCoreEffectRuntimeState),
                    row.BaseItemId + " strict Show failed: "
                    + Adapter.LastDiagnosticCode);
                Check(Adapter.IsVisible
                    && Adapter.LastProjectedModel != null,
                    row.BaseItemId
                    + " detail did not bind.");
                return Adapter.LastProjectedModel;
            }

            public void Dispose()
            {
                Adapter.Uninstall();
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.ClosePreviewScene(
                        scene);
                }
            }
        }

        private sealed class Fixture
        {
            public Fixture(
                ItemBalanceWorkbenchCatalog workbench,
                ItemSystemBattleSandboxViewProjectionResult projection,
                ItemCoreEffectIdentityCatalogSnapshot identity,
                ItemCoreEffectCultivationRosterSnapshot cultivation)
            {
                Workbench = workbench;
                Projection = projection;
                Identity = identity;
                Cultivation = cultivation;
            }

            public ItemBalanceWorkbenchCatalog Workbench { get; }
            public ItemSystemBattleSandboxViewProjectionResult
                Projection { get; }
            public ItemCoreEffectIdentityCatalogSnapshot
                Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot
                Cultivation { get; }
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

        private sealed class SampleRow
        {
            public SampleRow(
                string scenario,
                string baseItemId,
                string location,
                string buildStatus,
                string coreStatus,
                int visibleRows,
                string canonical)
            {
                Scenario = scenario;
                BaseItemId = baseItemId;
                Location = location;
                BuildStatus = buildStatus;
                CoreStatus = coreStatus;
                VisibleRows = visibleRows;
                Canonical = canonical;
            }

            public string Scenario { get; }
            public string BaseItemId { get; }
            public string Location { get; }
            public string BuildStatus { get; }
            public string CoreStatus { get; }
            public int VisibleRows { get; }
            public string Canonical { get; }
        }
    }
}
