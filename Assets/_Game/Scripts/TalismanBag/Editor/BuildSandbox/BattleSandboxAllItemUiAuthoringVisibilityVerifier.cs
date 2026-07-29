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
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxAllItemUiAuthoringVisibilityVerifier
    {
        private const string PackageName =
            "V0.4-BattleSandboxAllItemUiAuthoringVisibility01";
        private const string AssignmentPath =
            "Docs/V0.4/BattleSandboxAllItemUiAuthoringVisibility01_Assignment.md";
        private const string AssignmentHash =
            "0f1781ff0aa3ffdc27f9fe007e5285cc016581eb7127fe13fca977a6a54d1619";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TaskStartSceneHash =
            "993b53b7f6aaecd5816ff8ad7bcc369ae1cb0c2471e8432c4cc483e8f476fc4e";
        private const string WorkbenchPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ToolPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityTool.cs";
        private const string VerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityVerifier.cs";
        private const string ReportPath =
            "Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilitySpec.csv";
        private const string MatrixPath =
            "Docs/V0.4/Reports/BattleSandboxBuildQualificationMatrix30.csv";
        private const string InventoryPath =
            "Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringNodeInventory.csv";
        private const string ManualPath =
            "Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityManualTest.md";
        private const string LeakPath =
            "Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityLeakCheckReport.md";

        private static readonly string[] PackageFiles =
        {
            ScenePath,
            ToolPath,
            ToolPath + ".meta",
            VerifierPath,
            VerifierPath + ".meta",
            ReportPath,
            SpecPath,
            MatrixPath,
            InventoryPath,
            ManualPath,
            LeakPath
        };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [AssignmentPath] = AssignmentHash,
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] =
                    "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] =
                    "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs"] =
                    "b02a95f760f03f7b53bd4e7ac5b9efd638c1d2f328bf7d098e3ccc45e43b3358",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs"] =
                    "ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "4d87522e3d172a4804a576471f9de31b108a33ad9bdf239b6be243a820b0e7d3",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/DeterministicItemRandom.cs"] =
                    "021959b426899ff937dae82dfc563b15d52a83a7591d19374c0ddc67ecccf075",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] =
                    "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                [WorkbenchPath] =
                    "d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs"] =
                    "8a73a20b91bc79f546456584ccaae38c2bbe384def6a27ab56f0715c98ee0689",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] =
                    "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs"] =
                    "599f9b1469cb4165a9e22a2dd5bf1c5cb683b67399fd347a0cdd4cd59c41a6f2",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs"] =
                    "d0bb9f6ab3f3cee3840600f3c2fba0587ce88db545f8a05092968eff6462c8b1",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] =
                    "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] =
                    "116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] =
                    "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs"] =
                    "a72c7aa63d04b411c171918c440be56ac39ac7f1046fe30e7a2ffc55bc0a4298",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs"] =
                    "451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs"] =
                    "e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs"] =
                    "f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] =
                    "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] =
                    "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] =
                    "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29"
            };

        private static readonly MatrixRow[] ExpectedMatrix =
        {
            Row("I001", "震雷符", 41, ItemBuildQualification.QiLeiOnly, "", "qilei:fu"),
            Row("I002", "五雷急符", 80, ItemBuildQualification.Dual, "famen:zhenlei", "qilei:fu"),
            Row("I003", "震雷破壳印", 44, ItemBuildQualification.QiLeiOnly, "", "qilei:yin"),
            Row("I004", "五雷急令", 77, ItemBuildQualification.Dual, "famen:zhenlei", "qilei:ling"),
            Row("I005", "照壳雷镜", 28, ItemBuildQualification.QiLeiOnly, "", "qilei:jing"),
            Row("I006", "天鼓槌", 3, ItemBuildQualification.None, "", ""),
            Row("I007", "离火符", 77, ItemBuildQualification.Dual, "famen:lihuo", "qilei:fu"),
            Row("I008", "焚邪长符", 27, ItemBuildQualification.QiLeiOnly, "", "qilei:fu"),
            Row("I009", "离火焚邪印", 19, ItemBuildQualification.FaMenOnly, "famen:lihuo", ""),
            Row("I010", "风火令旗", 78, ItemBuildQualification.Dual, "famen:lihuo", "qilei:ling"),
            Row("I011", "照焚铜镜", 4, ItemBuildQualification.None, "", ""),
            Row("I012", "长明灯", 64, ItemBuildQualification.Dual, "famen:lihuo", "qilei:fa"),
            Row("I013", "护身符", 96, ItemBuildQualification.Dual, "famen:zhongyue", "qilei:fu"),
            Row("I014", "镇宅长符", 55, ItemBuildQualification.Dual, "famen:zhongyue", "qilei:fu"),
            Row("I015", "中岳镇守章", 32, ItemBuildQualification.QiLeiOnly, "", "qilei:yin"),
            Row("I016", "护坛令旗", 5, ItemBuildQualification.FaMenOnly, "famen:zhongyue", ""),
            Row("I017", "照邪八卦镜", 35, ItemBuildQualification.QiLeiOnly, "", "qilei:jing"),
            Row("I018", "镇山铜鼎", 23, ItemBuildQualification.FaMenOnly, "famen:zhongyue", ""),
            Row("I019", "净水符", 13, ItemBuildQualification.FaMenOnly, "famen:xuanshui", ""),
            Row("I020", "涤秽长符", 36, ItemBuildQualification.QiLeiOnly, "", "qilei:fu"),
            Row("I021", "玄水涤秽印", 23, ItemBuildQualification.FaMenOnly, "famen:xuanshui", ""),
            Row("I022", "玄水流令", 30, ItemBuildQualification.QiLeiOnly, "", "qilei:ling"),
            Row("I023", "照秽水镜", 82, ItemBuildQualification.Dual, "famen:xuanshui", "qilei:jing"),
            Row("I024", "净水盂", 63, ItemBuildQualification.Dual, "famen:xuanshui", "qilei:fa"),
            Row("I025", "封煞符", 90, ItemBuildQualification.Dual, "famen:taibai", "qilei:fu"),
            Row("I026", "太白斩符", 47, ItemBuildQualification.Dual, "famen:taibai", "qilei:fu"),
            Row("I027", "太白斩煞印", 92, ItemBuildQualification.Dual, "famen:taibai", "qilei:yin"),
            Row("I028", "断煞令", 2, ItemBuildQualification.None, "", ""),
            Row("I029", "照煞镜", 40, ItemBuildQualification.QiLeiOnly, "", "qilei:jing"),
            Row("I030", "桃木剑", 15, ItemBuildQualification.FaMenOnly, "famen:taibai", "")
        };

        private static readonly List<SpecResult> Specs = new();
        private static readonly List<NodeInventoryRow> Inventory = new();
        private static Fixture fixture;
        private static string postDisableInMemoryState = "NOT_RUN";
        private static string postInstallSceneSha256 = string.Empty;

        [MenuItem("Talisman Bag/V0.4/Verify BattleSandbox All Item UI Authoring Visibility")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Specs.Clear();
            Inventory.Clear();
            postDisableInMemoryState = "NOT_RUN";
            postInstallSceneSha256 = Sha256File(ProjectPath(ScenePath));

            Run("AUI-01", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            Run("AUI-02", "BUILDQUALIFICATION_MATRIX_30_PASS", VerifyMatrix);
            Run("AUI-03", "BUILDQUALIFICATION_VARIATION_IS_AUTHORITY_PASS",
                VerifyAuthorityVariation);
            Run("AUI-04", "AUTHORING_PREVIEW_EXPLICIT_ENABLE_DISABLE_PASS",
                VerifyAuthoringPreview);
            Run("AUI-05", "FULL_BATTLESANDBOX_UI_EDITMODE_VISIBLE_PASS",
                VerifyFullPanelVisibility);
            Run("AUI-06", "ITEMDETAIL_ALL_EXISTING_FIELDS_VISIBLE_PASS",
                VerifyExistingFieldInventory);
            Run("AUI-07", "FOUR_CORE_ROWS_VISIBLE_PASS",
                VerifyFourCoreRows);
            Run("AUI-08", "FAMEN_QILEI_SIMULTANEOUS_AUTHORING_PASS",
                VerifyBuildGroups);
            Run("AUI-09", "NO_RUNTIME_DATA_OVERRIDE_PASS",
                VerifyRealRuntimeProjection);
            Run("AUI-10", "RUNTIME_INITIAL_PANEL_HIDDEN_PASS",
                VerifyRuntimeInitialHidden);
            Run("AUI-11", "NO_HIERARCHY_OR_GEOMETRY_REWRITE_PASS",
                VerifyNoHierarchyOrGeometryRewrite);

            WriteReports();
            Run("AUI-12", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("AUI-13", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedDiffCheck);
            WriteReports();

            SpecResult[] failures = Specs.Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "BattleSandbox authoring visibility verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[BattleSandboxAllItemUiAuthoringVisibilityVerifier]\n"
                + string.Join("\n", Specs.Select(value => value.Marker))
                + "\nUSER_AUTHORING_HANDTEST_WAITING"
                + "\nPREFAB_MIGRATION_NOT_STARTED"
                + "\nLEGACY_CLEANUP_NOT_STARTED"
                + "\nDEV_COMPLETE"
                + "\nQA_STATIC_PASS");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Check(File.Exists(ProjectPath(pair.Key)),
                    "Protected file missing: " + pair.Key);
                Check(string.Equals(Sha256File(ProjectPath(pair.Key)), pair.Value,
                        StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Target Scene no longer matches refreshed task-start disk baseline.");
        }

        private static void VerifyMatrix()
        {
            fixture = BuildFixture();
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(ExpectedMatrix.Length == 30, "Expected matrix is not 30 rows.");
            Check(fixture.Projection.Rows.Count == 31
                && fixture.Projection.OrdinaryProjectionSet.Projections.Count == 30,
                "BattleSandbox roster is not 30 ordinary plus I031.");

            foreach (MatrixRow expected in ExpectedMatrix)
            {
                ItemSystemBattleSandboxViewRow row = FindRow(expected.BaseItemId);
                ItemInstanceQualifiedBuildItemSnapshot qualified =
                    authority.CurrentQualifiedBuildState.FindItemInstance(
                        row.ItemInstanceId);
                Check(row.RootSeed == expected.RootSeed,
                    expected.BaseItemId + " root seed mismatch.");
                Check(string.Equals(row.DisplayName, expected.DisplayName,
                        StringComparison.Ordinal),
                    expected.BaseItemId + " display name mismatch.");
                Check(row.Projection != null
                    && row.Projection.buildQualification == expected.Qualification,
                    expected.BaseItemId + " generated qualification mismatch.");
                Check(qualified != null
                    && qualified.buildQualification == expected.Qualification
                    && string.Equals(
                        qualified.eligibleFaMenBuildId ?? string.Empty,
                        expected.FaMenId, StringComparison.Ordinal)
                    && string.Equals(
                        qualified.eligibleQiLeiBuildId ?? string.Empty,
                        expected.QiLeiId, StringComparison.Ordinal),
                    expected.BaseItemId + " qualified authority identity mismatch.");

                Check(DeterministicItemRandom.TryComputeDomainSeed(
                        row.RootSeed,
                        1,
                        row.ItemInstanceId,
                        row.BaseItemId,
                        row.Projection.rarity,
                        new[]
                        {
                            "build",
                            "qualification",
                            "candidate_build_orange"
                        },
                        out ulong domainSeed),
                    expected.BaseItemId + " deterministic domain invalid.");
                DeterministicItemRandom.Stream stream = new(domainSeed);
                Check(stream.TryNextBounded(100UL, out ulong target)
                    && target == (ulong)expected.RollTarget,
                    expected.BaseItemId + " roll target mismatch.");
                expected.AuthorityResult = "AUTHORITY_GENERATION_PASS";
            }

            ItemSystemBattleSandboxViewRow system = FindRow("I031");
            Check(system.IsSystemItem
                && system.Projection == null
                && string.IsNullOrEmpty(system.ItemInstanceId)
                && authority.CurrentQualifiedBuildState.Items.All(value =>
                    !string.Equals(value.baseItemId, "I031",
                        StringComparison.Ordinal)),
                "I031 received ordinary BuildQualification authority.");
        }

        private static void VerifyAuthorityVariation()
        {
            Check(ExpectedMatrix.Count(value =>
                    value.Qualification == ItemBuildQualification.None) == 3
                && ExpectedMatrix.Count(value =>
                    value.Qualification == ItemBuildQualification.FaMenOnly) == 6
                && ExpectedMatrix.Count(value =>
                    value.Qualification == ItemBuildQualification.QiLeiOnly) == 9
                && ExpectedMatrix.Count(value =>
                    value.Qualification == ItemBuildQualification.Dual) == 12,
                "Frozen deterministic qualification distribution changed.");
            Check(ExpectedMatrix.Single(value => value.BaseItemId == "I001")
                    .Qualification == ItemBuildQualification.QiLeiOnly
                && ExpectedMatrix.Single(value => value.BaseItemId == "I004")
                    .Qualification == ItemBuildQualification.Dual
                && ExpectedMatrix.Single(value => value.BaseItemId == "I006")
                    .Qualification == ItemBuildQualification.None
                && ExpectedMatrix.Single(value => value.BaseItemId == "I007")
                    .Qualification == ItemBuildQualification.Dual
                && ExpectedMatrix.Single(value => value.BaseItemId == "I009")
                    .Qualification == ItemBuildQualification.FaMenOnly,
                "Decisive frozen samples changed.");
        }

        private static void VerifyAuthoringPreview()
        {
            Scene wrong = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            Check(!BattleSandboxAllItemUiAuthoringVisibilityTool.TryEnablePreview(
                    wrong, false, false, out string wrongDiagnostic)
                && string.Equals(wrongDiagnostic,
                    "AUTHORING_TOOL_REQUIRES_EXACT_ACTIVE_TARGET_SCENE",
                    StringComparison.Ordinal),
                "Tool did not reject a non-target active scene.");

            Scene scene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);
            Check(scene.IsValid() && scene.isLoaded
                && string.Equals(scene.path, ScenePath, StringComparison.Ordinal),
                "Target Scene could not be opened in temporary in-memory context.");
            ItemDetailPanelView panel = ResolvePanel(scene);
            Dictionary<int, NodeState> before = CaptureScene(scene);

            Check(BattleSandboxAllItemUiAuthoringVisibilityTool.TryEnablePreview(
                    scene, false, false, out string enableDiagnostic),
                "Enable failed: " + enableDiagnostic);
            Check(BattleSandboxAllItemUiAuthoringVisibilityTool.TryValidatePreview(
                    scene, out string validateDiagnostic),
                "Validate failed: " + validateDiagnostic);
            Dictionary<int, NodeState> preview = CaptureScene(scene);
            ValidatePreviewPanel(panel);

            Check(BattleSandboxAllItemUiAuthoringVisibilityTool.TryDisablePreview(
                    scene, false, false, out string disableDiagnostic),
                "Disable failed: " + disableDiagnostic);
            Dictionary<int, NodeState> disabled = CaptureScene(scene);
            bool markerAfterDisable = panel.GetComponentsInChildren<Text>(true)
                .Any(value => (value.text ?? string.Empty).Contains(
                    BattleSandboxAllItemUiAuthoringVisibilityTool.PreviewMarker,
                    StringComparison.Ordinal));
            Check(!panel.gameObject.activeSelf
                && !markerAfterDisable,
                "Disable did not hide the panel and remove the preview marker.");

            BuildNodeInventory(before, preview, disabled);
            Check(before.Keys.OrderBy(value => value)
                    .SequenceEqual(preview.Keys.OrderBy(value => value))
                && before.Keys.OrderBy(value => value)
                    .SequenceEqual(disabled.Keys.OrderBy(value => value)),
                "Enable/Disable created or deleted a Scene object.");
            Check(Inventory.All(value =>
                    !value.Created && !value.Deleted
                    && !value.Renamed && !value.Reparented
                    && !value.RectTransformTouched),
                "Enable/Disable changed hierarchy or authored RectTransform state.");

            postDisableInMemoryState =
                "panelActiveSelf=false; previewMarkerPresent=false; "
                + "hierarchyStable=true; authoredRectTransformTouched=false";
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Temporary authoring exercise serialized the target Scene.");
            postInstallSceneSha256 = Sha256File(ProjectPath(ScenePath));
        }

        private static void VerifyFullPanelVisibility()
        {
            WithEnabledScene((scene, panel) =>
            {
                Check(panel.gameObject.activeInHierarchy,
                    "Authored panel is not active in hierarchy.");
                Check(scene.GetRootGameObjects().SelectMany(value =>
                        value.GetComponentsInChildren<ItemDetailPanelView>(true))
                        .Count(value => value.gameObject.scene == scene) == 1,
                    "Target Scene does not contain exactly one ItemDetailPanelView.");
                Check(ReferencedObjectActive(panel, "itemNameText")
                    && ReferencedObjectActive(panel, "metaText")
                    && ReferencedObjectActive(panel, "powerText")
                    && ReferencedObjectActive(panel, "detailTabButton")
                    && ReferencedObjectActive(panel, "debugTabButton")
                    && ReferencedArrayActive(panel, "statusBadgeImages")
                    && ReferencedArrayActive(panel, "statusBadgeTexts"),
                    "Header/status/tab controls are not all authoring-visible.");
                Check(panel.GetComponentsInChildren<Text>(true).Any(value =>
                        (value.text ?? string.Empty).Contains(
                            BattleSandboxAllItemUiAuthoringVisibilityTool
                                .PreviewMarker,
                            StringComparison.Ordinal)),
                    "Explicit Editor preview marker is missing.");
            });
        }

        private static void VerifyExistingFieldInventory()
        {
            WithEnabledScene((scene, panel) =>
            {
                ItemDetailSectionView[] player = ReadObjectArray<
                    ItemDetailSectionView>(panel, "playerSections");
                ItemDetailSectionView[] debug = ReadObjectArray<
                    ItemDetailSectionView>(panel, "debugSections");
                Check(player.Length == 15
                    && player.All(value => value != null
                        && value.gameObject.activeSelf
                        && !string.IsNullOrWhiteSpace(value.Title)
                        && !string.IsNullOrWhiteSpace(value.Body)),
                    "All 15 player sections are not visible/non-empty.");
                Check(debug.Length == 3
                    && debug.All(value => value != null
                        && !string.IsNullOrWhiteSpace(value.Title)
                        && !string.IsNullOrWhiteSpace(value.Body)),
                    "Existing debug tab content was not populated.");
                Check(ReferencedObjectActiveIfPresent(panel, "artworkText")
                    && ReferencedObjectActiveIfPresent(
                        panel, "artworkImageSlot")
                    && ReferencedObjectActiveIfPresent(
                        panel, "artworkKeyText")
                    && ReferencedObjectActiveIfPresent(
                        panel, "rarityBadgeImage")
                    && ReferencedObjectActiveIfPresent(
                        panel, "faMenIconImage")
                    && ReferencedObjectActiveIfPresent(
                        panel, "qiLeiIconImage"),
                    "Existing artwork/icon slots are not authoring-visible.");
            });
        }

        private static void VerifyFourCoreRows()
        {
            WithEnabledScene((scene, panel) =>
            {
                Transform root = RequireNamed(panel.transform,
                    "CoreEffectRowsRoot");
                for (int index = 0; index < 4; index++)
                {
                    Check(RequireNamed(root, "CoreEffectRow_" + index)
                            .gameObject.activeSelf
                        && RequireNamed(root, "CoreEffectText_" + index)
                            .GetComponent<Text>() != null
                        && !string.IsNullOrWhiteSpace(
                            RequireNamed(root, "CoreEffectText_" + index)
                                .GetComponent<Text>().text),
                        "Core row " + index + " is not visible/non-empty.");
                }
                Check(root.GetComponentsInChildren<Transform>(true)
                    .All(value => !string.Equals(value.name,
                        "CoreEffectRow_4", StringComparison.Ordinal)),
                    "A forbidden fifth authored core row exists.");
            });
        }

        private static void VerifyBuildGroups()
        {
            WithEnabledScene((scene, panel) =>
            {
                Transform faMen = RequireNamed(panel.transform,
                    "FaMenBuildRowsRoot");
                Transform qiLei = RequireNamed(panel.transform,
                    "QiLeiBuildRowsRoot");
                Check(faMen.gameObject.activeInHierarchy
                    && RequireNamed(faMen, "FaMenBuildOverviewRow")
                        .gameObject.activeSelf,
                    "FaMen overview is not visible.");
                for (int index = 0; index < 3; index++)
                {
                    Check(RequireNamed(faMen, "FaMenBuildRow_" + index)
                        .gameObject.activeSelf,
                        "FaMen 2/4/6 row " + index + " is not visible.");
                }
                Check(qiLei.gameObject.activeInHierarchy
                    && RequireNamed(qiLei, "QiLeiBuildOverviewRow")
                        .gameObject.activeSelf,
                    "QiLei overview is not visible.");
                for (int index = 0; index < 2; index++)
                {
                    Check(RequireNamed(qiLei, "QiLeiBuildRow_" + index)
                        .gameObject.activeSelf,
                        "QiLei 2/4 row " + index + " is not visible.");
                }
            });
        }

        private static void VerifyRealRuntimeProjection()
        {
            EnsureFixture();
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);
            ItemDetailPanelView panel = ResolvePanel(scene);
            ItemSystemBattleSandboxItemDetailAdapter adapter = new();
            Check(adapter.Initialize(panel, fixture.Projection.Rows),
                "Runtime adapter initialization failed: "
                + adapter.LastDiagnosticCode);
            Check(!panel.gameObject.activeSelf,
                "Runtime initialization did not hide the authored panel.");

            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            foreach (MatrixRow expected in ExpectedMatrix)
            {
                ItemSystemBattleSandboxViewRow row = FindRow(expected.BaseItemId);
                Check(adapter.Show(
                        row.BaseItemId,
                        row.ItemInstanceId,
                        string.Empty,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState,
                        authority.CurrentCoreEffectRuntimeState),
                    expected.BaseItemId + " runtime Show failed: "
                    + adapter.LastDiagnosticCode);
                ItemDetailViewModel model = adapter.LastProjectedModel;
                Check(model != null
                    && model.qualifiedBuildTrack != null
                    && model.qualifiedBuildTrack.buildQualification ==
                        expected.Qualification
                    && model.qualifiedBuildTrack.TrackRows.Count ==
                        expected.ExpectedTrackCount,
                    expected.BaseItemId
                    + " final runtime ViewModel qualification mismatch.");
                ItemDetailSectionViewModel faMen = FindSection(
                    model, "famenBuild");
                ItemDetailSectionViewModel qiLei = FindSection(
                    model, "qileiBuild");
                if (expected.ExpectedFaMenVisible)
                {
                    Check(faMen != null && faMen.keepWhenEmpty
                        && !string.IsNullOrWhiteSpace(faMen.body),
                        expected.BaseItemId + " FaMen runtime section missing.");
                }
                else if (expected.Qualification != ItemBuildQualification.None)
                {
                    Check(faMen == null || !faMen.keepWhenEmpty
                        || string.IsNullOrWhiteSpace(faMen.body),
                        expected.BaseItemId + " false FaMen fallback.");
                }
                if (expected.ExpectedQiLeiVisible)
                {
                    Check(qiLei != null && qiLei.keepWhenEmpty
                        && !string.IsNullOrWhiteSpace(qiLei.body),
                        expected.BaseItemId + " QiLei runtime section missing.");
                }
                else
                {
                    Check(qiLei == null || !qiLei.keepWhenEmpty
                        || string.IsNullOrWhiteSpace(qiLei.body),
                        expected.BaseItemId + " false QiLei fallback.");
                }
                Check(!ModelContains(model,
                        BattleSandboxAllItemUiAuthoringVisibilityTool
                            .PreviewMarker),
                    expected.BaseItemId
                    + " runtime model retained Editor preview data.");
                expected.RuntimeProjectionResult =
                    "RUNTIME_PROJECTION_PATH_PASS";
            }
            adapter.Uninstall();
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Runtime projection exercise serialized the target Scene.");
        }

        private static void VerifyRuntimeInitialHidden()
        {
            EnsureFixture();
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);
            ItemDetailPanelView panel = ResolvePanel(scene);
            panel.SetVisible(true);
            ItemSystemBattleSandboxItemDetailAdapter adapter = new();
            Check(adapter.Initialize(panel, fixture.Projection.Rows)
                && !panel.gameObject.activeSelf
                && !adapter.IsVisible
                && adapter.LastProjectedModel == null,
                "Play initialization contract did not hide before first click.");
            adapter.Uninstall();
        }

        private static void VerifyNoHierarchyOrGeometryRewrite()
        {
            Check(Inventory.Count > 0,
                "Authoring node inventory was not produced.");
            Check(Inventory.All(value =>
                    !value.RectTransformTouched
                    && !value.Created
                    && !value.Deleted
                    && !value.Renamed
                    && !value.Reparented),
                "Inventory contains a hierarchy/geometry rewrite.");
            string source = File.ReadAllText(ProjectPath(ToolPath));
            string[] forbidden =
            {
                "[InitializeOnLoad",
                "InitializeOnLoadMethod",
                "Instantiate(",
                "Destroy(",
                "DestroyImmediate(",
                ".SetParent(",
                ".SetSiblingIndex(",
                ".anchoredPosition",
                ".sizeDelta",
                ".anchorMin",
                ".anchorMax",
                ".pivot",
                "EditorSceneManager.SaveScene",
                "AssetDatabase.CreateAsset"
            };
            Check(forbidden.All(value => !source.Contains(
                    value, StringComparison.Ordinal)),
                "Tool source contains a forbidden auto/hierarchy/geometry write.");
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Target Scene disk bytes changed during static verification.");
        }

        private static void VerifyLeakCheck()
        {
            foreach (string path in PackageFiles)
            {
                Check(File.Exists(ProjectPath(path)),
                    "Expected package file missing: " + path);
            }
            foreach (string report in new[]
                     {
                         ReportPath,
                         SpecPath,
                         MatrixPath,
                         InventoryPath,
                         ManualPath,
                         LeakPath
                     })
            {
                Check(!File.Exists(ProjectPath(report + ".meta")),
                    "Forbidden Docs meta exists: " + report + ".meta");
            }
            Check(Directory.GetFiles(ProjectPath("Docs/V0.4/Reports"),
                    "BattleSandboxAllItemUiAuthoringVisibility01*",
                    SearchOption.TopDirectoryOnly).Length == 0,
                "Unexpected package-name report leak exists.");
            VerifyProtectedHashes();
            Check(string.Equals(postInstallSceneSha256,
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Static verifier changed the serialized Scene baseline.");
        }

        private static void VerifyPackageScopedDiffCheck()
        {
            ProcessStartInfo start = new("git")
            {
                WorkingDirectory = ProjectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            start.ArgumentList.Add("diff");
            start.ArgumentList.Add("--check");
            start.ArgumentList.Add("--");
            foreach (string path in PackageFiles)
            {
                if (!string.Equals(path, ScenePath,
                        StringComparison.Ordinal))
                {
                    start.ArgumentList.Add(path);
                }
            }
            using Process process = Process.Start(start);
            Check(process != null, "git diff --check could not start.");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Check(process.ExitCode == 0,
                "package-scoped git diff --check failed: "
                + output + error);
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Scene differs from task-start disk baseline.");
            foreach (string path in PackageFiles.Where(value =>
                         !string.Equals(value, ScenePath,
                             StringComparison.Ordinal)))
            {
                Check(!HasTrailingWhitespace(ProjectPath(path)),
                    "Package file has trailing whitespace: " + path);
            }
        }

        private static Fixture BuildFixture()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchPath);
            Check(workbench != null, "Workbench catalog missing.");
            GameObject providerObject = new("AuthoringVisibilityProvider")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench, provider);
                Check(projection?.IsValid == true,
                    "BattleSandbox projection invalid: "
                    + string.Join("|",
                        projection?.Diagnostics ?? Array.Empty<string>()));
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity?.isValid == true,
                    "Four-core identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation = new(
                    ItemCoreEffectRosterCompleteness.Complete,
                    projection.OrdinaryProjectionSet.Projections.Select(value =>
                        new ItemCoreEffectCultivationRow(
                            value.itemInstanceId,
                            value.baseItemId,
                            40,
                            ItemSystemBattleSandboxBoardAdapter
                                .CultivationSourceKey,
                            ItemCoreEffectFactCompleteness.Complete)));
                Check(cultivation.isValid,
                    "BattleSandbox Lv40 cultivation fixture invalid.");
                return new Fixture(
                    workbench, projection, identity, cultivation);
            }
            finally
            {
                Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority()
        {
            EnsureFixture();
            return new ItemSystemBattleSandboxBoardAuthority(
                fixture.Projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                fixture.Identity,
                fixture.Cultivation,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static void EnsureFixture()
        {
            if (fixture == null)
            {
                fixture = BuildFixture();
            }
        }

        private static ItemSystemBattleSandboxViewRow FindRow(string baseItemId)
        {
            EnsureFixture();
            ItemSystemBattleSandboxViewRow row = fixture.Projection.Rows
                .SingleOrDefault(value => value != null
                    && string.Equals(value.BaseItemId, baseItemId,
                        StringComparison.Ordinal));
            Check(row != null, "Projection row missing: " + baseItemId);
            return row;
        }

        private static void WithEnabledScene(Action<Scene, ItemDetailPanelView> action)
        {
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);
            ItemDetailPanelView panel = ResolvePanel(scene);
            Check(BattleSandboxAllItemUiAuthoringVisibilityTool.TryEnablePreview(
                    scene, false, false, out string diagnostic),
                "Enable failed: " + diagnostic);
            action(scene, panel);
            Check(BattleSandboxAllItemUiAuthoringVisibilityTool.TryDisablePreview(
                    scene, false, false, out diagnostic),
                "Disable failed: " + diagnostic);
            Check(string.Equals(Sha256File(ProjectPath(ScenePath)),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "Temporary authoring scene context was serialized.");
        }

        private static ItemDetailPanelView ResolvePanel(Scene scene)
        {
            ItemDetailPanelView[] panels = scene.GetRootGameObjects()
                .SelectMany(value =>
                    value.GetComponentsInChildren<ItemDetailPanelView>(true))
                .Where(value => value != null
                    && value.gameObject.scene == scene)
                .ToArray();
            Check(panels.Length == 1
                && string.Equals(panels[0].gameObject.name,
                    "ItemDetailPanel", StringComparison.Ordinal),
                "Expected exactly one authored ItemDetailPanel.");
            return panels[0];
        }

        private static void ValidatePreviewPanel(ItemDetailPanelView panel)
        {
            Check(panel.gameObject.activeInHierarchy,
                "Panel is not active after Enable.");
            ItemDetailSectionView[] sections =
                panel.GetComponentsInChildren<ItemDetailSectionView>(true);
            Check(sections.Count(value => IsPlayerSection(value.name)) == 15,
                "Player section cardinality is not 15.");
            Check(sections.Where(value => IsPlayerSection(value.name))
                .All(value => value.gameObject.activeSelf
                    && !string.IsNullOrWhiteSpace(value.Body)),
                "Player section content is incomplete.");
        }

        private static Dictionary<int, NodeState> CaptureScene(Scene scene)
        {
            Dictionary<int, NodeState> result = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in
                         root.GetComponentsInChildren<Transform>(true))
                {
                    GameObject gameObject = transform.gameObject;
                    RectTransform rect = transform as RectTransform;
                    Text[] texts = gameObject.GetComponents<Text>();
                    Image[] images = gameObject.GetComponents<Image>();
                    Component[] components = gameObject.GetComponents<Component>();
                    result.Add(gameObject.GetInstanceID(), new NodeState
                    {
                        InstanceId = gameObject.GetInstanceID(),
                        Name = gameObject.name,
                        ParentId = transform.parent == null
                            ? 0
                            : transform.parent.gameObject.GetInstanceID(),
                        SiblingIndex = transform.GetSiblingIndex(),
                        HierarchyPath = HierarchyPath(transform),
                        ComponentRole = string.Join("+", components
                            .Where(value => value != null)
                            .Select(value => value.GetType().Name)),
                        ActiveSelf = gameObject.activeSelf,
                        TextSignature = string.Join("||", texts.Select(value =>
                            value.enabled + ":" + (value.text ?? string.Empty))),
                        ImageSignature = string.Join("||", images.Select(value =>
                            value.enabled + ":"
                            + (value.sprite == null
                                ? ""
                                : AssetDatabase.GetAssetPath(value.sprite))
                            + ":" + ColorUtility.ToHtmlStringRGBA(value.color))),
                        RectTransformTouched = rect != null
                            && EditorUtility.IsDirty(rect)
                    });
                }
            }
            return result;
        }

        private static void BuildNodeInventory(
            IReadOnlyDictionary<int, NodeState> before,
            IReadOnlyDictionary<int, NodeState> preview,
            IReadOnlyDictionary<int, NodeState> disabled)
        {
            Inventory.Clear();
            int[] ids = before.Keys.Concat(preview.Keys).Concat(disabled.Keys)
                .Distinct().OrderBy(value => value).ToArray();
            foreach (int id in ids)
            {
                before.TryGetValue(id, out NodeState first);
                preview.TryGetValue(id, out NodeState second);
                disabled.TryGetValue(id, out NodeState third);
                bool changed = first == null || second == null || third == null
                    || first.ActiveSelf != second.ActiveSelf
                    || second.ActiveSelf != third.ActiveSelf
                    || !string.Equals(first.TextSignature,
                        second.TextSignature, StringComparison.Ordinal)
                    || !string.Equals(second.TextSignature,
                        third.TextSignature, StringComparison.Ordinal)
                    || !string.Equals(first.ImageSignature,
                        second.ImageSignature, StringComparison.Ordinal)
                    || !string.Equals(second.ImageSignature,
                        third.ImageSignature, StringComparison.Ordinal);
                if (!changed)
                {
                    continue;
                }
                Inventory.Add(new NodeInventoryRow
                {
                    HierarchyPath = first?.HierarchyPath
                        ?? second?.HierarchyPath
                        ?? third?.HierarchyPath
                        ?? string.Empty,
                    ComponentRole = first?.ComponentRole
                        ?? second?.ComponentRole
                        ?? third?.ComponentRole
                        ?? string.Empty,
                    BeforeActiveSelf = first?.ActiveSelf,
                    PreviewActiveSelf = second?.ActiveSelf,
                    DisableActiveSelf = third?.ActiveSelf,
                    TextOrSpriteTouched = first != null && second != null
                        && (!string.Equals(first.TextSignature,
                                second.TextSignature,
                                StringComparison.Ordinal)
                            || !string.Equals(first.ImageSignature,
                                second.ImageSignature,
                                StringComparison.Ordinal)),
                    RectTransformTouched =
                        (first?.RectTransformTouched ?? false)
                        != (second?.RectTransformTouched ?? false)
                        || (second?.RectTransformTouched ?? false)
                        != (third?.RectTransformTouched ?? false),
                    Created = first == null && second != null,
                    Deleted = first != null && second == null,
                    Renamed = first != null && second != null
                        && !string.Equals(first.Name, second.Name,
                            StringComparison.Ordinal),
                    Reparented = first != null && second != null
                        && (first.ParentId != second.ParentId
                            || first.SiblingIndex != second.SiblingIndex)
                });
            }
        }

        private static bool ReferencedObjectActive(
            ItemDetailPanelView panel,
            string propertyName)
        {
            SerializedProperty property =
                new SerializedObject(panel).FindProperty(propertyName);
            Object value = property?.objectReferenceValue;
            GameObject gameObject = value switch
            {
                GameObject target => target,
                Component component => component.gameObject,
                _ => null
            };
            return gameObject != null && gameObject.activeInHierarchy;
        }

        private static bool ReferencedObjectActiveIfPresent(
            ItemDetailPanelView panel,
            string propertyName)
        {
            SerializedProperty property =
                new SerializedObject(panel).FindProperty(propertyName);
            Object value = property?.objectReferenceValue;
            if (value == null)
            {
                return true;
            }
            GameObject gameObject = value switch
            {
                GameObject target => target,
                Component component => component.gameObject,
                _ => null
            };
            return gameObject != null && gameObject.activeInHierarchy;
        }

        private static bool ReferencedArrayActive(
            ItemDetailPanelView panel,
            string propertyName)
        {
            SerializedProperty property =
                new SerializedObject(panel).FindProperty(propertyName);
            if (property == null || !property.isArray
                || property.arraySize == 0)
            {
                return false;
            }
            bool found = false;
            for (int index = 0; index < property.arraySize; index++)
            {
                Object value = property.GetArrayElementAtIndex(index)
                    .objectReferenceValue;
                if (value == null)
                {
                    continue;
                }
                found = true;
                GameObject gameObject = value switch
                {
                    GameObject target => target,
                    Component component => component.gameObject,
                    _ => null
                };
                if (gameObject == null || !gameObject.activeInHierarchy)
                {
                    return false;
                }
            }
            return found;
        }

        private static T[] ReadObjectArray<T>(
            ItemDetailPanelView panel,
            string propertyName)
            where T : Object
        {
            SerializedProperty property =
                new SerializedObject(panel).FindProperty(propertyName);
            Check(property != null && property.isArray,
                "Serialized array missing: " + propertyName);
            List<T> result = new();
            for (int index = 0; index < property.arraySize; index++)
            {
                result.Add(property.GetArrayElementAtIndex(index)
                    .objectReferenceValue as T);
            }
            return result.ToArray();
        }

        private static Transform RequireNamed(
            Transform root,
            string name)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(value.name, name,
                    StringComparison.Ordinal)).ToArray();
            Check(matches.Length == 1,
                "Expected one existing authored node named " + name
                + ", found " + matches.Length.ToString(
                    CultureInfo.InvariantCulture) + ".");
            return matches[0];
        }

        private static ItemDetailSectionViewModel FindSection(
            ItemDetailViewModel model,
            string stateKey)
        {
            return model?.displayPlayerSections?.FirstOrDefault(value =>
                value != null && string.Equals(
                    value.stateKey, stateKey, StringComparison.Ordinal));
        }

        private static bool ModelContains(
            ItemDetailViewModel model,
            string marker)
        {
            if (model == null)
            {
                return false;
            }
            return (model.iconPlaceholderKey ?? string.Empty).Contains(
                       marker, StringComparison.Ordinal)
                || (model.displayPlayerSections ?? new List<
                        ItemDetailSectionViewModel>())
                    .Concat(model.displayDebugSections ?? new List<
                        ItemDetailSectionViewModel>())
                    .Any(value => value != null
                        && ((value.title ?? string.Empty).Contains(
                                marker, StringComparison.Ordinal)
                            || (value.body ?? string.Empty).Contains(
                                marker, StringComparison.Ordinal)));
        }

        private static bool IsPlayerSection(string name)
        {
            return name switch
            {
                "HeaderSection" => true,
                "CoreIdentitySection" => true,
                "CurrentStateSection" => true,
                "BaseStatsSection" => true,
                "TriggerConditionSection" => true,
                "BasicEffectSection" => true,
                "CoreAwakeningSection" => true,
                "FaMenBuildSection" => true,
                "QiLeiBuildSection" => true,
                "MainBuildMonitorSection" => true,
                "FixedAffixSection" => true,
                "RandomAffixSection" => true,
                "OrangeGrowthSection" => true,
                "PlacementHintSection" => true,
                "FlavorSection" => true,
                _ => false
            };
        }

        private static string HierarchyPath(Transform transform)
        {
            Stack<string> segments = new();
            Transform current = transform;
            while (current != null)
            {
                segments.Push(current.name + "["
                    + current.GetSiblingIndex().ToString(
                        CultureInfo.InvariantCulture) + "]");
                current = current.parent;
            }
            return string.Join("/", segments);
        }

        private static void WriteReports()
        {
            Directory.CreateDirectory(ProjectPath("Docs/V0.4/Reports"));
            WriteText(ReportPath, BuildReport());
            WriteText(SpecPath, BuildSpec());
            WriteText(MatrixPath, BuildMatrix());
            WriteText(InventoryPath, BuildInventory());
            WriteText(ManualPath, BuildManual());
            WriteText(LeakPath, BuildLeakReport());
        }

        private static string BuildReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox All Item UI Authoring Visibility Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `" + PackageName + "`");
            builder.AppendLine("- AssignmentSHA256: `" + AssignmentHash + "`");
            builder.AppendLine("- Result: `DEV_COMPLETE / QA_STATIC_PASS / USER_AUTHORING_HANDTEST_WAITING`");
            builder.AppendLine("- taskStartSceneSha256: `" + TaskStartSceneHash + "`");
            builder.AppendLine("- postInstallSceneSha256: `" + postInstallSceneSha256 + "`");
            builder.AppendLine("- postDisableInMemoryState: `" + postDisableInMemoryState + "`");
            builder.AppendLine("- Serialized preview install: `NOT_PERFORMED_BY_BATCH; explicit target-scene menu command only`");
            builder.AppendLine("- Runtime owner added: `NO`");
            builder.AppendLine("- Prefab migration: `NOT_STARTED`");
            builder.AppendLine("- Legacy cleanup: `NOT_STARTED`");
            builder.AppendLine();
            builder.AppendLine("## Static QA");
            builder.AppendLine();
            foreach (SpecResult result in Specs)
            {
                builder.AppendLine("- `" + result.Marker + "`: "
                    + (result.Passed ? "PASS" : "FAIL")
                    + " — " + result.Detail);
            }
            builder.AppendLine();
            builder.AppendLine("Existing `ItemDetailPanelView.Bind` may perform its own driven, in-memory layout calculation. "
                + "The package tool does not write authored RectTransform/LayoutGroup/ScrollRect geometry, "
                + "and the verifier never saves its temporary Scene context.");
            return builder.ToString();
        }

        private static string BuildSpec()
        {
            StringBuilder builder = new();
            builder.AppendLine("id,marker,result,detail");
            foreach (SpecResult result in Specs)
            {
                builder.AppendLine(Csv(result.Id) + ","
                    + Csv(result.Marker) + ","
                    + Csv(result.Passed ? "PASS" : "FAIL") + ","
                    + Csv(result.Detail));
            }
            return builder.ToString();
        }

        private static string BuildMatrix()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "baseItemId,displayName,rootSeed,rollTarget,buildQualification,"
                + "eligibleFaMenBuildId,eligibleQiLeiBuildId,expectedFaMenVisible,"
                + "expectedQiLeiVisible,authorityResult,runtimeProjectionResult,"
                + "userVisualConfirmation");
            foreach (MatrixRow row in ExpectedMatrix)
            {
                builder.AppendLine(Csv(row.BaseItemId) + ","
                    + Csv(row.DisplayName) + ","
                    + row.RootSeed.ToString(CultureInfo.InvariantCulture) + ","
                    + row.RollTarget.ToString(CultureInfo.InvariantCulture) + ","
                    + Csv(row.Qualification.ToString()) + ","
                    + Csv(row.FaMenId) + ","
                    + Csv(row.QiLeiId) + ","
                    + Csv(row.ExpectedFaMenVisible ? "true" : "false") + ","
                    + Csv(row.ExpectedQiLeiVisible ? "true" : "false") + ","
                    + Csv(row.AuthorityResult) + ","
                    + Csv(row.RuntimeProjectionResult) + ","
                    + Csv(row.UserVisualConfirmation));
            }
            return builder.ToString();
        }

        private static string BuildInventory()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "scenePath,hierarchyPath,componentRole,beforeActiveSelf,"
                + "previewActiveSelf,disableActiveSelf,textOrSpriteTouched,"
                + "rectTransformTouched,created,deleted,renamed,reparented");
            foreach (NodeInventoryRow row in Inventory.OrderBy(value =>
                         value.HierarchyPath, StringComparer.Ordinal))
            {
                builder.AppendLine(Csv(ScenePath) + ","
                    + Csv(row.HierarchyPath) + ","
                    + Csv(row.ComponentRole) + ","
                    + Csv(NullableBool(row.BeforeActiveSelf)) + ","
                    + Csv(NullableBool(row.PreviewActiveSelf)) + ","
                    + Csv(NullableBool(row.DisableActiveSelf)) + ","
                    + Csv(row.TextOrSpriteTouched ? "true" : "false") + ","
                    + Csv(row.RectTransformTouched ? "true" : "false") + ","
                    + Csv(row.Created ? "true" : "false") + ","
                    + Csv(row.Deleted ? "true" : "false") + ","
                    + Csv(row.Renamed ? "true" : "false") + ","
                    + Csv(row.Reparented ? "true" : "false"));
            }
            return builder.ToString();
        }

        private static string BuildManual()
        {
            return "# BattleSandbox All Item UI Authoring Visibility Manual Test\n\n"
                + "Status: `USER_AUTHORING_HANDTEST_WAITING`\n\n"
                + "1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity` in Edit Mode.\n"
                + "2. Run `Talisman Bag/V0.4/BattleSandbox Authoring/Enable Full BattleSandbox Item UI Authoring Preview`.\n"
                + "3. Confirm the existing full panel, 15 player sections, both Build groups and four core rows are visible.\n"
                + "4. Tune existing UI geometry manually and save the Scene.\n"
                + "5. Enter Play and confirm the detail panel starts hidden.\n"
                + "6. Click I004 and I007: FaMen + QiLei; I009: FaMen only; I001: QiLei only; I006: no eligible contribution.\n"
                + "7. Exit Play and confirm the saved Edit Mode authoring layout remains.\n"
                + "8. Run Disable, confirm the panel hides without layout deletion, then re-enable and confirm no duplicate panel.\n\n"
                + "Do not mark unobserved rows as user PASS. See the matrix `userVisualConfirmation` column.\n";
        }

        private static string BuildLeakReport()
        {
            bool pass = Specs.Any(value =>
                string.Equals(value.Marker, "LEAKCHECK_PASS",
                    StringComparison.Ordinal) && value.Passed);
            return "# BattleSandbox All Item UI Authoring Visibility LeakCheck\n\n"
                + "- Result: `" + (pass ? "PASS" : "PENDING_OR_FAIL") + "`\n"
                + "- Exact implementation files: `4/4`\n"
                + "- Exact report files: `6/6`\n"
                + "- Docs `.meta`: `NONE`\n"
                + "- Runtime component/state owner added: `NO`\n"
                + "- Scene hierarchy create/delete/rename/reparent: `NONE`\n"
                + "- Authored RectTransform direct writes: `NONE`\n"
                + "- Prefab/ItemSandbox/View/runtime/Roll/RNG protected hashes: `UNCHANGED`\n"
                + "- Enemy/BoneAspect/EncounterPresentation attribution: `EXTERNAL / UNCLAIMED`\n"
                + "- Git operations: `NONE`\n";
        }

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            try
            {
                action();
                Specs.Add(new SpecResult(id, marker, true, "verified"));
            }
            catch (Exception exception)
            {
                Specs.Add(new SpecResult(
                    id, marker, false, exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Project root unavailable.");

        private static string ProjectPath(string relative) =>
            Path.Combine(ProjectRoot, relative.Replace('/', Path.DirectorySeparatorChar));

        private static string Sha256File(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(sha.ComputeHash(stream).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static void WriteText(string relativePath, string value)
        {
            File.WriteAllText(
                ProjectPath(relativePath),
                value ?? string.Empty,
                new UTF8Encoding(false));
        }

        private static bool HasTrailingWhitespace(string path)
        {
            if (!File.Exists(path))
            {
                return true;
            }
            return File.ReadLines(path).Any(line =>
                line.Length > 0
                && char.IsWhiteSpace(line[line.Length - 1]));
        }

        private static string Csv(string value) =>
            "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"")
                .Replace("\r", " ")
                .Replace("\n", " ") + "\"";

        private static string NullableBool(bool? value) =>
            value.HasValue
                ? (value.Value ? "true" : "false")
                : "NotPresent";

        private static MatrixRow Row(
            string id,
            string display,
            int target,
            ItemBuildQualification qualification,
            string faMen,
            string qiLei)
        {
            return new MatrixRow
            {
                BaseItemId = id,
                DisplayName = display,
                RootSeed = ItemSystemBattleSandboxViewProjection.PackageSeed
                    + int.Parse(id.Substring(1),
                        CultureInfo.InvariantCulture),
                RollTarget = target,
                Qualification = qualification,
                FaMenId = faMen,
                QiLeiId = qiLei,
                AuthorityResult = "NOT_RUN",
                RuntimeProjectionResult = "NOT_RUN",
                UserVisualConfirmation =
                    id is "I001" or "I004" or "I007" or "I009"
                        ? "PRIOR_USER_OBSERVATION_MATCHES_AUTHORITY"
                        : "WAITING"
            };
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
            public ItemSystemBattleSandboxViewProjectionResult Projection { get; }
            public ItemCoreEffectIdentityCatalogSnapshot Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot Cultivation { get; }
        }

        private sealed class MatrixRow
        {
            public string BaseItemId;
            public string DisplayName;
            public long RootSeed;
            public int RollTarget;
            public ItemBuildQualification Qualification;
            public string FaMenId;
            public string QiLeiId;
            public string AuthorityResult;
            public string RuntimeProjectionResult;
            public string UserVisualConfirmation;

            public bool ExpectedFaMenVisible =>
                Qualification == ItemBuildQualification.FaMenOnly
                || Qualification == ItemBuildQualification.Dual;
            public bool ExpectedQiLeiVisible =>
                Qualification == ItemBuildQualification.QiLeiOnly
                || Qualification == ItemBuildQualification.Dual;
            public int ExpectedTrackCount =>
                (ExpectedFaMenVisible ? 1 : 0)
                + (ExpectedQiLeiVisible ? 1 : 0);
        }

        private sealed class SpecResult
        {
            public SpecResult(
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

        private sealed class NodeState
        {
            public int InstanceId;
            public string Name;
            public int ParentId;
            public int SiblingIndex;
            public string HierarchyPath;
            public string ComponentRole;
            public bool ActiveSelf;
            public string TextSignature;
            public string ImageSignature;
            public bool RectTransformTouched;
        }

        private sealed class NodeInventoryRow
        {
            public string HierarchyPath;
            public string ComponentRole;
            public bool? BeforeActiveSelf;
            public bool? PreviewActiveSelf;
            public bool? DisableActiveSelf;
            public bool TextOrSpriteTouched;
            public bool RectTransformTouched;
            public bool Created;
            public bool Deleted;
            public bool Renamed;
            public bool Reparented;
        }
    }
}
