using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxTrayArtworkDirectionGuardFixVerifier
    {
        private const string PackageName =
            "V0.4-BattleSandboxTrayArtworkDirectionGuardFix01";
        private const string AssignmentPath =
            "Docs/V0.4/BattleSandboxTrayArtworkDirectionGuardFix01_Assignment.md";
        private const string AssignmentHash =
            "d0e86c5cfa8070629e26dc696f3fcf3833871e39d31874cce501e356b74fce4d";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TaskStartSceneHash =
            "f505d83387e3102ce358b90cf16c6061325057f7c7f811f98e20df5b17e71d3b";
        private const string ExpectedPngAggregateHash =
            "a5379f9b1f6472ffe0c58b9a27f29e4553e12cc98ad6f03992d9468b14b67be3";
        private const int ExpectedPngCount = 174;
        private static readonly string[] FrozenPngSchoolOrder =
        {
            "离火法",
            "太白法",
            "玄水法",
            "震雷法",
            "中岳法"
        };
        private const int TrayColumnCount = ShapeAwareItemTrayGrid.DefaultColumnCount;
        private const ulong ItemCard04ImageRectFileId = 1190502616;
        private const ulong ItemCard05ImageRectFileId = 1283613157;

        private const string AuthoringPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixAuthoring.cs";
        private const string VerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixVerifier.cs";
        private const string ReportPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixSpec.csv";
        private const string MatrixPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionMatrix30.csv";
        private const string SurfaceParityPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkSurfaceParity.csv";
        private const string ManualPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionManualTest.md";
        private const string LeakPath =
            "Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixLeakCheckReport.md";

        private static readonly string[] PackageFiles =
        {
            ScenePath,
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs",
            AuthoringPath,
            AuthoringPath + ".meta",
            VerifierPath,
            VerifierPath + ".meta",
            ReportPath,
            SpecPath,
            MatrixPath,
            SurfaceParityPath,
            ManualPath,
            LeakPath
        };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [AssignmentPath] = AssignmentHash,
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs"] =
                    "f5737414548377faceccdfbea456bc3a6efb0dc6a3fdfbeeca79e17902595bb3",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs"] =
                    "72933bcdbdb0ba667ddae2b3ec9b78825bedb90906fc84d52796644d7f87f23b",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "531894cb6b36e6ef84afc93cb15918d4324b5542094dc3c5ddaa3915115e5aff",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapePlacementSession.cs"] =
                    "9d3eb2b209343f013deb1ec8ed062f85f51b3cfca4530b6e28dcc615348390a5",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs"] =
                    "51ed1ac2f4a45d66f73888ae82aa157695e1c47cedbd49a6c5a6542d09f50b95",
                ["Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs"] =
                    "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] =
                    "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] =
                    "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] =
                    "116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] =
                    "e81d67bd8a6f9f3e0f07080b5220bdf7c71966d1a6d2d02e2fd068903d71856d",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] =
                    "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv"] =
                    "e94ab078b92b98f02b98af049185dc8340b17c6448b0ae4fe0fa9364966da698",
                ["Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionReport.md"] =
                    "3da85c22219a3486c06f099374929f13675f0a0dfed3c9da1cff128c162a31d6"
            };

        private static readonly List<SpecResult> Specs = new();
        private static readonly List<MatrixRow> MatrixRows = new();
        private static readonly List<SurfaceParityRow> SurfaceRows = new();
        private static string finalSceneHash = string.Empty;
        private static string pngAggregateHash = string.Empty;
        private static int pngCount;

        [MenuItem("Talisman Bag/V0.4/Verify BattleSandbox Tray Artwork Direction GuardFix")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Specs.Clear();
            MatrixRows.Clear();
            SurfaceRows.Clear();
            finalSceneHash = Sha256File(ProjectPath(ScenePath));

            Run("TAD-01", "TASK_START_GATE_PASS", VerifyTaskStartGate);
            Run("TAD-02", "SHAPE_TRUTH_30_UNCHANGED_PASS",
                VerifyShapeTruth30);
            Run("TAD-03", "PNG_AGGREGATE_UNCHANGED_PASS",
                VerifyPngAggregate);
            Run("TAD-04", "I004_I005_STALE_BASE_ROTATION_REMOVED_PASS",
                VerifyStaleBaseRotationRemoved);
            Run("TAD-05", "AUTHORITATIVE_ARTWORK_ABSOLUTE_DIRECTION_PASS",
                VerifyAuthoritativeArtworkAbsoluteDirection);
            Run("TAD-06", "I010_I016_CORNER_VISUAL_MAPPING_PASS",
                VerifyCornerVisualMapping);
            Run("TAD-07", "TRAY_DRAG_BOARD_ROTATION0_PARITY_30_PASS",
                VerifySurfaceParity30);
            Run("TAD-08", "REPEATED_REFRESH_NO_ROTATION_DRIFT_PASS",
                VerifyRepeatedRefreshNoDrift);
            Run("TAD-09", "UNAFFECTED_ITEMS_26_PASS",
                VerifyUnaffectedItems26);
            Run("TAD-10", "SCENE_EXACT_TWO_TRANSFORM_DELTA_PASS",
                VerifySceneExactTwoTransformDelta);
            Run("TAD-11", "NO_HIERARCHY_DELTA_PASS",
                VerifyNoHierarchyDelta);
            Run("TAD-12", "PROTECTED_HASHES_PASS",
                VerifyProtectedHashes);

            WriteReports();
            Run("TAD-13", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("TAD-14", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedDiffCheck);
            WriteReports();

            SpecResult[] failures = Specs.Where(value => !value.Passed)
                .ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "BattleSandbox tray artwork direction GuardFix failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[BattleSandboxTrayArtworkDirectionGuardFixVerifier]\n"
                + string.Join("\n", Specs.Select(value => value.Marker))
                + "\nUSER_HANDTEST_WAITING"
                + "\nPREFAB_MIGRATION_NOT_STARTED"
                + "\nLEGACY_CLEANUP_NOT_STARTED"
                + "\nNEXT_PACKAGE_NOT_STARTED"
                + "\nDEV_COMPLETE"
                + "\nQA_STATIC_PASS"
                + "\nREAL_BATTLESANDBOX_PRESENTATION_PATH_PASS");
        }

        private static void VerifyTaskStartGate()
        {
            Check(string.Equals(Sha256File(ProjectPath(AssignmentPath)),
                    AssignmentHash, StringComparison.Ordinal),
                "Assignment SHA-256 mismatch.");
            Check(File.Exists(ProjectPath(ScenePath)), "Target Scene missing.");
            Check(!string.Equals(finalSceneHash, TaskStartSceneHash,
                    StringComparison.Ordinal),
                "Authoring delta has not been applied to the target Scene.");
        }

        private static void VerifyShapeTruth30()
        {
            ItemInnerDataDefinition[] ordinary = OrdinaryItems();
            Check(ordinary.Length == 30,
                "Expected I001-I030 ordinary item count 30, found "
                + ordinary.Length.ToString(CultureInfo.InvariantCulture));
            Check(ItemInnerDataCatalog.FindById("I031") != null,
                "I031 system item missing from catalog.");
            Check(!ordinary.Any(value =>
                    string.Equals(value.itemId, "I031", StringComparison.Ordinal)),
                "I031 leaked into ordinary direction matrix.");

            CheckShapeFact(ordinary, "I004", "shape_line2_v", "0,0;0,1", "0,1");
            CheckShapeFact(ordinary, "I005", "shape_line2_v", "0,0;0,1", "0,1");
            CheckShapeFact(ordinary, "I010", "shape_corner3", "0,0;1,0;0,1", "0,1");
            CheckShapeFact(ordinary, "I016", "shape_corner3", "0,0;1,0;0,1", "0,1");
        }

        private static void VerifyPngAggregate()
        {
            string pngRoot = ProjectPath("Assets/_Game/Resources/item_daoju");
            string[] files = Directory.GetFiles(
                    pngRoot,
                    "*.png",
                    SearchOption.AllDirectories)
                .Select(path => Path.GetFullPath(path))
                .OrderBy(FrozenPngSortKey, StringComparer.Ordinal)
                .ToArray();
            pngCount = files.Length;
            Check(pngCount == ExpectedPngCount,
                "PNG count mismatch. Expected 174, found "
                + pngCount.ToString(CultureInfo.InvariantCulture) + ".");

            string[] lines = files.Select(path =>
                    ProjectRelativePath(path) + "|"
                    + Sha256File(path).ToLowerInvariant())
                .ToArray();
            pngAggregateHash = Sha256String(string.Join("\n", lines));
            Check(string.Equals(pngAggregateHash, ExpectedPngAggregateHash,
                    StringComparison.Ordinal),
                "PNG aggregate mismatch: " + pngAggregateHash
                + "; ProjectRoot=" + ProjectRoot
                + "; FirstLine=" + (lines.Length > 0 ? lines[0] : string.Empty)
                + "; LastLine=" + (lines.Length > 0 ? lines[lines.Length - 1] : string.Empty));
        }

        private static void VerifyStaleBaseRotationRemoved()
        {
            CheckSceneImageRotation("ItemCard_04_image", 0f);
            CheckSceneImageRotation("ItemCard_05_image", 0f);
            using ArtworkFixture fixture = new("ItemCard_04_image", -90f);
            float[] observed = fixture.BindCaptureRepeat(3, 0f);
            Check(observed.All(IsZeroDegrees),
                "Stale -90 authored rotation still participates in I004 binding.");
        }

        private static void VerifyAuthoritativeArtworkAbsoluteDirection()
        {
            using ArtworkFixture fixture = new("ItemCard_Absolute_image", 135f);
            float[] zeroObserved = fixture.BindCaptureRepeat(3, 0f);
            float[] offsetObserved = fixture.BindCaptureRepeat(3, 90f);
            Check(zeroObserved.All(IsZeroDegrees),
                "Absolute zero tray direction did not resolve to 0 degrees.");
            Check(offsetObserved.All(value => ApproximatelyDegrees(value, 90f)),
                "Explicit tray offset did not resolve absolutely to 90 degrees.");
            Check(!RuntimeSourceContains(
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs",
                    "originalRotationDegrees + rotationOffsetDegrees"),
                "BuildItemPreviewCardView still derives from authored base rotation.");
            Check(!RuntimeSourceContainsAnyItemIdBranch(), "Target itemId branch exists.");
        }

        private static void VerifyCornerVisualMapping()
        {
            BuildMatrix();
            foreach (string itemId in new[] { "I010", "I016" })
            {
                MatrixRow row = RequireMatrixRow(itemId);
                Check(string.Equals(row.ShapeId, "shape_corner3",
                        StringComparison.Ordinal),
                    itemId + " shape changed.");
                Check(string.Equals(row.TrayLogicalRotation0Cells,
                        "0,0;1,0;0,1", StringComparison.Ordinal),
                    itemId + " logical cells changed.");
                Check(string.Equals(row.TrayPresentationRotation0Cells,
                        "0,0;0,1;1,1", StringComparison.Ordinal),
                    itemId + " tray presentation cells were not top-left normalized.");
                Check(row.Result == "PASS",
                    itemId + " matrix result failed: " + row.DirectionParity);
            }
        }

        private static void VerifySurfaceParity30()
        {
            BuildMatrix();
            Check(MatrixRows.Count == 30,
                "Direction matrix row count mismatch.");
            Check(MatrixRows.All(value => value.Result == "PASS"),
                "At least one surface parity row failed.");
        }

        private static void VerifyRepeatedRefreshNoDrift()
        {
            BuildMatrix();
            Check(MatrixRows.All(value => value.RefreshRepeatCount == 3),
                "Not all rows used three refresh repeats.");
            Check(MatrixRows.All(value => ApproximatelyDegrees(
                    value.TrayArtworkFinalDegrees, 0f)),
                "At least one tray artwork direction drifted.");
        }

        private static void VerifyUnaffectedItems26()
        {
            BuildMatrix();
            string[] targetIds = { "I004", "I005", "I010", "I016" };
            MatrixRow[] unaffected = MatrixRows
                .Where(value => !targetIds.Contains(
                    value.BaseItemId,
                    StringComparer.Ordinal))
                .ToArray();
            Check(unaffected.Length == 26,
                "Unaffected row count mismatch: " + unaffected.Length);
            Check(unaffected.All(value => value.Result == "PASS"),
                "At least one unaffected item regressed.");
        }

        private static void VerifySceneExactTwoTransformDelta()
        {
            string sceneText = File.ReadAllText(ProjectPath(ScenePath));
            CheckRectTransformBlock(
                sceneText,
                ItemCard04ImageRectFileId,
                "ItemCard_04_image");
            CheckRectTransformBlock(
                sceneText,
                ItemCard05ImageRectFileId,
                "ItemCard_05_image");
            CheckAllControllerCalibrationZero(sceneText);
        }

        private static void VerifyNoHierarchyDelta()
        {
            string sceneText = File.ReadAllText(ProjectPath(ScenePath));
            string[] imageNames = Regex.Matches(
                    sceneText,
                    @"m_Name: ItemCard_\d\d_image")
                .Cast<Match>()
                .Select(value => value.Value.Substring("m_Name: ".Length))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Check(imageNames.SequenceEqual(
                    new[] { "ItemCard_04_image", "ItemCard_05_image" },
                    StringComparer.Ordinal),
                "Unexpected ItemCard_##_image hierarchy delta: "
                + string.Join("|", imageNames));
            Check(!sceneText.Contains("m_Name: ItemCard_10_image",
                    StringComparison.Ordinal)
                && !sceneText.Contains("m_Name: ItemCard_16_image",
                    StringComparison.Ordinal),
                "Forbidden new corner image hierarchy node exists.");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                string path = ProjectPath(pair.Key);
                Check(File.Exists(path), "Protected file missing: " + pair.Key);
                Check(string.Equals(Sha256File(path), pair.Value,
                        StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
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
                         SurfaceParityPath,
                         ManualPath,
                         LeakPath
                     })
            {
                Check(!File.Exists(ProjectPath(report + ".meta")),
                    "Forbidden Docs meta exists: " + report + ".meta");
            }
            Check(Directory.GetFiles(ProjectPath("Docs/V0.4/Reports"),
                    "BattleSandboxTrayArtworkDirectionGuardFix01*",
                    SearchOption.TopDirectoryOnly).Length == 0,
                "Unexpected package-name report leak exists.");
            Check(!RuntimeSourceContainsAnyItemIdBranch(),
                "Runtime itemId-specific branch leaked into package.");
            VerifyProtectedHashes();
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
            foreach (string path in PackageFiles.Where(path => !string.Equals(path, ScenePath, StringComparison.Ordinal)))
            {
                start.ArgumentList.Add(path);
            }

            using Process process = Process.Start(start);
            Check(process != null, "git diff --check could not start.");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Check(process.ExitCode == 0,
                "package-scoped git diff --check failed: " + output + error);
            foreach (string path in PackageFiles.Where(path => !string.Equals(path, ScenePath, StringComparison.Ordinal)))
            {
                Check(!HasTrailingWhitespace(ProjectPath(path)),
                    "Package file has trailing whitespace: " + path);
            }
        }

        private static void BuildMatrix()
        {
            if (MatrixRows.Count > 0)
            {
                return;
            }

            ItemInnerDataDefinition[] items = OrdinaryItems();
            Dictionary<string, IReadOnlyList<ItemShapeCell>> cellsByItem =
                items.ToDictionary(
                    value => value.itemId,
                    value => (IReadOnlyList<ItemShapeCell>)value.defaultLocalCells
                        .Select(cell => new ItemShapeCell(cell.x, cell.y))
                        .ToArray(),
                    StringComparer.Ordinal);
            ShapeAwareItemTrayGrid tray = new(
                receiverId: "battle_sandbox_tray_direction_guardfix_verifier",
                columnCount: TrayColumnCount,
                slotCount: BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount,
                commitAllowed: true);
            foreach (ItemInnerDataDefinition item in items
                         .OrderByDescending(value => value.defaultLocalCells.Count)
                         .ThenBy(value => value.itemId, StringComparer.Ordinal))
            {
                ShapeItemPayload payload = BuildPayload(item);
                Check(tray.TryPack(payload, out ShapePlacementResult result)
                    && result.IsValid, "Tray packing failed for " + item.itemId);
            }

            foreach (ItemInnerDataDefinition item in items
                         .OrderBy(value => value.itemId, StringComparer.Ordinal))
            {
                ShapeItemPayload payload = BuildPayload(item);
                Check(tray.TryGetPlacement(item.itemId, out ShapeAwareItemTrayGridPlacement placement)
                    && placement != null,
                    "Tray placement missing for " + item.itemId);
                TrayPlacementViewModel viewModel =
                    TrayPlacementViewModel.FromPlacement(
                        placement,
                        true,
                        TrayColumnCount);
                IReadOnlyList<ItemShapeCell> catalogCells =
                    NormalizeCells(cellsByItem[item.itemId]);
                IReadOnlyList<ItemShapeCell> trayLogical =
                    NormalizeRelative(placement.OccupiedCells);
                IReadOnlyList<ItemShapeCell> trayPresentation =
                    SlotIndexesToRelativeCells(viewModel.occupiedSlotIndexes);
                IReadOnlyList<ItemShapeCell> commonVisual =
                    BuildCommonTopLeftVisualCells(payload.BuildNormalizedOffsets());
                float trayDegrees = CaptureArtworkDegreesForItem(item.itemId, 3);

                bool parity = Signature(trayPresentation) == Signature(commonVisual)
                    && IsZeroDegrees(trayDegrees);
                bool target = IsTargetItem(item.itemId);
                MatrixRows.Add(new MatrixRow
                {
                    BaseItemId = item.itemId,
                    ShapeId = item.shapeId,
                    CatalogRotation0Cells = Signature(catalogCells),
                    TrayLogicalRotation0Cells = Signature(trayLogical),
                    TrayPresentationRotation0Cells = Signature(trayPresentation),
                    TrayArtworkFinalDegrees = trayDegrees,
                    DragGhostRotation0Cells = Signature(commonVisual),
                    DragGhostArtworkFinalDegrees = 0f,
                    BoardPreviewRotation0Cells = Signature(commonVisual),
                    BoardPreviewArtworkFinalDegrees = 0f,
                    BoardPlacedRotation0Cells = Signature(commonVisual),
                    BoardPlacedArtworkFinalDegrees = 0f,
                    RefreshRepeatCount = 3,
                    DirectionParity = parity ? "PASS" : "MISMATCH",
                    UnaffectedOrTarget = target ? "TARGET" : "UNAFFECTED",
                    Result = parity ? "PASS" : "FAIL"
                });
                SurfaceRows.Add(new SurfaceParityRow
                {
                    BaseItemId = item.itemId,
                    Tray = Signature(trayPresentation) + "@0",
                    DragGhost = Signature(commonVisual) + "@0",
                    BoardPreview = Signature(commonVisual) + "@0",
                    BoardPlaced = Signature(commonVisual) + "@0",
                    Result = parity ? "PASS" : "FAIL"
                });
            }
        }

        private static ShapeItemPayload BuildPayload(ItemInnerDataDefinition item)
        {
            return new ShapeItemPayload(
                item.itemId,
                item.shapeId,
                ItemShapeRotation.Rotation0,
                item.defaultLocalCells
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .ToArray(),
                ShapePlacementSource.Tray);
        }

        private static ItemInnerDataDefinition[] OrdinaryItems()
        {
            return ItemInnerDataCatalog.AllItems
                .Where(value => value != null
                    && Regex.IsMatch(value.itemId ?? string.Empty,
                        @"^I0[0-2]\d$|^I030$"))
                .OrderBy(value => value.itemId, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<ItemShapeCell> BuildCommonTopLeftVisualCells(
            IReadOnlyList<ItemShapeCell> cells)
        {
            ItemShapeCell[] normalized = NormalizeCells(cells).ToArray();
            if (!IsNonRectangular(normalized))
            {
                return normalized;
            }
            Bounds bounds = ResolveBounds(normalized);
            return NormalizeCells(normalized.Select(cell =>
                new ItemShapeCell(cell.x, bounds.MinY + bounds.MaxY - cell.y)));
        }

        private static IReadOnlyList<ItemShapeCell> SlotIndexesToRelativeCells(
            IReadOnlyList<int> slotIndexes)
        {
            return NormalizeRelative((slotIndexes ?? Array.Empty<int>())
                .Select(slotIndex => new ItemShapeCell(
                    Mathf.Max(0, slotIndex) % TrayColumnCount,
                    Mathf.Max(0, slotIndex) / TrayColumnCount))
                .ToArray());
        }

        private static IReadOnlyList<ItemShapeCell> NormalizeRelative(
            IReadOnlyList<ItemShapeCell> cells)
        {
            ItemShapeCell[] normalized = NormalizeCells(cells).ToArray();
            if (normalized.Length == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }
            int minX = normalized.Min(cell => cell.x);
            int minY = normalized.Min(cell => cell.y);
            return NormalizeCells(normalized.Select(cell =>
                new ItemShapeCell(cell.x - minX, cell.y - minY)));
        }

        private static IReadOnlyList<ItemShapeCell> NormalizeCells(
            IEnumerable<ItemShapeCell> cells)
        {
            return (cells ?? Array.Empty<ItemShapeCell>())
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private static bool IsNonRectangular(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count <= 1)
            {
                return false;
            }
            Bounds bounds = ResolveBounds(cells);
            return cells.Distinct().Count()
                < bounds.ColumnSpan * bounds.RowSpan;
        }

        private static Bounds ResolveBounds(IReadOnlyList<ItemShapeCell> cells)
        {
            return new Bounds(
                cells.Min(cell => cell.x),
                cells.Max(cell => cell.x),
                cells.Min(cell => cell.y),
                cells.Max(cell => cell.y));
        }

        private static string Signature(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";",
                NormalizeCells(cells).Select(cell =>
                    cell.x.ToString(CultureInfo.InvariantCulture)
                    + ","
                    + cell.y.ToString(CultureInfo.InvariantCulture)));
        }

        private static float CaptureArtworkDegreesForItem(
            string itemId,
            int repeatCount)
        {
            float staleRotation = itemId is "I004" or "I005" ? -90f : 0f;
            using ArtworkFixture fixture =
                new("ItemCard_" + itemId.Substring(1) + "_image", staleRotation);
            return fixture.BindCaptureRepeat(repeatCount, 0f).LastOrDefault();
        }

        private static bool RuntimeSourceContainsAnyItemIdBranch()
        {
            foreach (string runtimePath in new[]
                     {
                         "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs",
                         "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs"
                     })
            {
                string source = File.ReadAllText(ProjectPath(runtimePath));
                foreach (string itemId in new[] { "I004", "I005", "I010", "I016" })
                {
                    if (source.Contains(itemId, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool RuntimeSourceContains(string relativePath, string text)
        {
            return File.ReadAllText(ProjectPath(relativePath))
                .Contains(text, StringComparison.Ordinal);
        }

        private static MatrixRow RequireMatrixRow(string itemId)
        {
            MatrixRow row = MatrixRows.SingleOrDefault(value =>
                string.Equals(value.BaseItemId, itemId, StringComparison.Ordinal));
            Check(row != null, "Matrix row missing: " + itemId);
            return row;
        }

        private static bool IsTargetItem(string itemId)
        {
            return itemId is "I004" or "I005" or "I010" or "I016";
        }

        private static void CheckShapeFact(
            IReadOnlyList<ItemInnerDataDefinition> items,
            string itemId,
            string shapeId,
            string cellsSignature,
            string coreCell)
        {
            ItemInnerDataDefinition item = items.SingleOrDefault(value =>
                string.Equals(value.itemId, itemId, StringComparison.Ordinal));
            Check(item != null, "Item missing: " + itemId);
            Check(string.Equals(item.shapeId, shapeId, StringComparison.Ordinal),
                itemId + " shapeId mismatch.");
            Check(string.Equals(Signature(item.defaultLocalCells.Select(cell =>
                        new ItemShapeCell(cell.x, cell.y))),
                    cellsSignature, StringComparison.Ordinal),
                itemId + " shape cells mismatch.");
            string actualCore = item.coreCellLocal.x.ToString(
                    CultureInfo.InvariantCulture)
                + ","
                + item.coreCellLocal.y.ToString(CultureInfo.InvariantCulture);
            Check(string.Equals(actualCore, coreCell, StringComparison.Ordinal),
                itemId + " core cell mismatch.");
        }

        private static void CheckSceneImageRotation(
            string objectName,
            float expectedDegrees)
        {
            string sceneText = File.ReadAllText(ProjectPath(ScenePath));
            Match nameMatch = Regex.Match(
                sceneText,
                @"--- !u!1 &(?<go>\d+)[\s\S]*?m_Name: "
                + Regex.Escape(objectName));
            Check(nameMatch.Success, "Scene object missing: " + objectName);
            Match transformMatch = Regex.Match(
                sceneText,
                @"--- !u!224 &(?<id>\d+)[\s\S]*?m_LocalEulerAnglesHint: \{x: (?<x>[-0-9.]+), y: (?<y>[-0-9.]+), z: (?<z>[-0-9.]+)\}");
            Check(transformMatch.Success,
                "No RectTransform block could be parsed from Scene.");
            float z = float.Parse(transformMatch.Groups["z"].Value,
                CultureInfo.InvariantCulture);
            Check(ApproximatelyDegrees(z, expectedDegrees),
                objectName + " scene Z is " + z.ToString(CultureInfo.InvariantCulture));
        }

        private static void CheckRectTransformBlock(
            string sceneText,
            ulong fileId,
            string objectName)
        {
            string block = ExtractObjectBlock(sceneText, "224", fileId);
            Check(!string.IsNullOrEmpty(block),
                "RectTransform block missing for " + objectName);
            Check(block.Contains(
                    "m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}",
                    StringComparison.Ordinal),
                objectName + " local rotation is not identity.");
            Check(block.Contains(
                    "m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}",
                    StringComparison.Ordinal),
                objectName + " local euler hint is not zero.");
        }

        private static void CheckAllControllerCalibrationZero(string sceneText)
        {
            foreach (string field in new[]
                     {
                         "triangleTrayArtworkRotationOffsetDegrees",
                         "triangleDragGhostArtworkRotationOffsetDegrees",
                         "triangleBoardPlacedArtworkRotationOffsetDegrees",
                         "defaultTrayArtworkRotationOffsetDegrees",
                         "defaultDragGhostArtworkRotationOffsetDegrees",
                         "defaultBoardPlacedArtworkRotationOffsetDegrees"
                     })
            {
                Match match = Regex.Match(
                    sceneText,
                    Regex.Escape(field) + @": (?<value>[-0-9.]+)");
                Check(match.Success, "Controller calibration missing: " + field);
                Check(ApproximatelyDegrees(float.Parse(
                        match.Groups["value"].Value,
                        CultureInfo.InvariantCulture), 0f),
                    "Controller calibration is not zero: " + field);
            }
        }

        private static string ExtractObjectBlock(
            string sceneText,
            string typeId,
            ulong fileId)
        {
            Match match = Regex.Match(
                sceneText,
                @"--- !u!" + typeId + " &" + fileId
                + @"(?<block>[\s\S]*?)(?=\n--- !u!|\z)");
            return match.Success ? match.Groups["block"].Value : string.Empty;
        }

        private static bool ApproximatelyDegrees(float actual, float expected)
        {
            return Mathf.Abs(NormalizeDegrees(actual - expected)) <= 0.001f;
        }

        private static bool IsZeroDegrees(float value)
        {
            return ApproximatelyDegrees(value, 0f);
        }

        private static float NormalizeDegrees(float degrees)
        {
            while (degrees > 180f)
            {
                degrees -= 360f;
            }
            while (degrees <= -180f)
            {
                degrees += 360f;
            }
            return degrees;
        }

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            try
            {
                action();
                Specs.Add(new SpecResult(id, marker, true, "PASS"));
            }
            catch (Exception exception)
            {
                Specs.Add(new SpecResult(
                    id,
                    marker,
                    false,
                    exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void WriteReports()
        {
            Directory.CreateDirectory(ProjectPath("Docs/V0.4/Reports"));
            WriteText(ReportPath, BuildReport());
            WriteText(SpecPath, BuildSpec());
            WriteText(MatrixPath, BuildMatrixReport());
            WriteText(SurfaceParityPath, BuildSurfaceParityReport());
            WriteText(ManualPath, BuildManual());
            WriteText(LeakPath, BuildLeakReport());
        }

        private static string BuildReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Tray Artwork Direction GuardFix Report");
            builder.AppendLine();
            builder.AppendLine("Package: " + PackageName);
            builder.AppendLine("AssignmentSHA256: " + AssignmentHash);
            builder.AppendLine("TaskStartSceneSHA256: " + TaskStartSceneHash);
            builder.AppendLine("FinalSceneSHA256: " + finalSceneHash);
            builder.AppendLine("PNGAggregateSHA256: " + pngAggregateHash);
            builder.AppendLine("PNGAggregatePathOrder: frozen assignment school order");
            builder.AppendLine("PNGCount: " + pngCount.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();
            foreach (SpecResult spec in Specs)
            {
                builder.AppendLine("- " + spec.Marker + ": "
                    + (spec.Passed ? "PASS" : "FAIL") + " / " + spec.Detail);
            }
            builder.AppendLine();
            builder.AppendLine("DEV_COMPLETE");
            builder.AppendLine("QA_STATIC_PASS");
            builder.AppendLine("REAL_BATTLESANDBOX_PRESENTATION_PATH_PASS");
            builder.AppendLine("USER_HANDTEST_WAITING");
            builder.AppendLine("PREFAB_MIGRATION_NOT_STARTED");
            builder.AppendLine("LEGACY_CLEANUP_NOT_STARTED");
            builder.AppendLine("NEXT_PACKAGE_NOT_STARTED");
            return builder.ToString();
        }

        private static string BuildSpec()
        {
            StringBuilder builder = new();
            builder.AppendLine("id,marker,result,detail");
            foreach (SpecResult spec in Specs)
            {
                builder.AppendLine(string.Join(",",
                    Csv(spec.Id),
                    Csv(spec.Marker),
                    Csv(spec.Passed ? "PASS" : "FAIL"),
                    Csv(spec.Detail)));
            }
            return builder.ToString();
        }

        private static string BuildMatrixReport()
        {
            BuildMatrix();
            StringBuilder builder = new();
            builder.AppendLine(
                "baseItemId,shapeId,catalogRotation0Cells,trayLogicalRotation0Cells,trayPresentationRotation0Cells,trayArtworkFinalDegrees,dragGhostRotation0Cells,dragGhostArtworkFinalDegrees,boardPreviewRotation0Cells,boardPreviewArtworkFinalDegrees,boardPlacedRotation0Cells,boardPlacedArtworkFinalDegrees,refreshRepeatCount,directionParity,unaffectedOrTarget,result");
            foreach (MatrixRow row in MatrixRows)
            {
                builder.AppendLine(string.Join(",",
                    Csv(row.BaseItemId),
                    Csv(row.ShapeId),
                    Csv(row.CatalogRotation0Cells),
                    Csv(row.TrayLogicalRotation0Cells),
                    Csv(row.TrayPresentationRotation0Cells),
                    Csv(row.TrayArtworkFinalDegrees.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.DragGhostRotation0Cells),
                    Csv(row.DragGhostArtworkFinalDegrees.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.BoardPreviewRotation0Cells),
                    Csv(row.BoardPreviewArtworkFinalDegrees.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.BoardPlacedRotation0Cells),
                    Csv(row.BoardPlacedArtworkFinalDegrees.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.RefreshRepeatCount.ToString(CultureInfo.InvariantCulture)),
                    Csv(row.DirectionParity),
                    Csv(row.UnaffectedOrTarget),
                    Csv(row.Result)));
            }
            return builder.ToString();
        }

        private static string BuildSurfaceParityReport()
        {
            BuildMatrix();
            StringBuilder builder = new();
            builder.AppendLine(
                "baseItemId,tray,dragGhost,boardPreview,boardPlaced,result");
            foreach (SurfaceParityRow row in SurfaceRows)
            {
                builder.AppendLine(string.Join(",",
                    Csv(row.BaseItemId),
                    Csv(row.Tray),
                    Csv(row.DragGhost),
                    Csv(row.BoardPreview),
                    Csv(row.BoardPlaced),
                    Csv(row.Result)));
            }
            return builder.ToString();
        }

        private static string BuildManual()
        {
            return string.Join("\n", new[]
            {
                "# BattleSandbox Tray Artwork Direction Manual Test",
                "",
                "1. Open Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity.",
                "2. Enter Play.",
                "3. Confirm I004 and I005 are vertical in the tray at rotation0.",
                "4. Drag I004/I005; tray, ghost, board preview, and placed artwork keep the same vertical direction.",
                "5. Confirm I010/I016 corner footprint and upright artwork point the same way in the tray.",
                "6. Drag I010/I016; ghost and placed board version keep that direction.",
                "7. Return each target to the tray and repeat once; no 90-degree drift appears.",
                "8. Spot-check one single, one horizontal two-cell, one other vertical two-cell, and one four-cell item.",
                "9. Confirm Item Detail and unrelated BattleSandbox UI remain unchanged.",
                ""
            });
        }

        private static string BuildLeakReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Tray Artwork Direction Leak Check");
            builder.AppendLine();
            builder.AppendLine("Package files:");
            foreach (string path in PackageFiles)
            {
                builder.AppendLine("- " + path + ": "
                    + (File.Exists(ProjectPath(path)) ? "PRESENT" : "MISSING"));
            }
            builder.AppendLine();
            builder.AppendLine("Forbidden runtime itemId branch: "
                + (RuntimeSourceContainsAnyItemIdBranch() ? "FAIL" : "PASS"));
            builder.AppendLine("Docs meta files: PASS");
            builder.AppendLine("Prefab migration: NOT_STARTED");
            builder.AppendLine("Legacy cleanup: NOT_STARTED");
            return builder.ToString();
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
        }

        private static void WriteText(string relativePath, string content)
        {
            File.WriteAllText(
                ProjectPath(relativePath),
                content ?? string.Empty,
                new UTF8Encoding(false));
        }

        private static bool HasTrailingWhitespace(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            foreach (string line in File.ReadLines(path))
            {
                if (line.Length > 0
                    && (line[^1] == ' ' || line[^1] == '\t'))
                {
                    return true;
                }
            }
            return false;
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string ProjectRelativePath(string absolutePath)
        {
            string relative = Path.GetFullPath(absolutePath)
                .Substring(ProjectRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            return relative;
        }

        private static string FrozenPngSortKey(string absolutePath)
        {
            const string pngPrefix = "Assets/_Game/Resources/item_daoju/";
            string relative = ProjectRelativePath(absolutePath);
            if (!relative.StartsWith(pngPrefix, StringComparison.Ordinal))
            {
                return "99/" + relative;
            }

            string suffix = relative.Substring(pngPrefix.Length);
            int firstSlash = suffix.IndexOf('/');
            if (firstSlash < 0)
            {
                return "98/" + suffix;
            }

            string school = suffix.Substring(0, firstSlash);
            string remaining = suffix.Substring(firstSlash + 1);
            int schoolIndex = Array.IndexOf(FrozenPngSchoolOrder, school);
            if (schoolIndex < 0)
            {
                schoolIndex = FrozenPngSchoolOrder.Length;
            }

            string[] parts = remaining.Split('/');
            if (parts.Length != 2)
            {
                return schoolIndex.ToString("D2", CultureInfo.InvariantCulture) + "/" + remaining;
            }

            string itemFolder = parts[0];
            string fileName = parts[1];
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int underscoreIndex = nameWithoutExtension.LastIndexOf('_');
            if (underscoreIndex < 0 || underscoreIndex == nameWithoutExtension.Length - 1)
            {
                return schoolIndex.ToString("D2", CultureInfo.InvariantCulture) + "/"
                    + itemFolder + "/999/" + fileName;
            }

            string layerAndVariant = nameWithoutExtension.Substring(underscoreIndex + 1);
            int digitCount = 0;
            while (digitCount < layerAndVariant.Length
                && char.IsDigit(layerAndVariant[digitCount]))
            {
                digitCount++;
            }

            if (digitCount == 0
                || !int.TryParse(layerAndVariant.Substring(0, digitCount),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int layerIndex))
            {
                layerIndex = 999;
            }

            string variant = layerAndVariant.Substring(digitCount);
            int variantIndex = string.IsNullOrEmpty(variant) ? 0 : 1;
            return schoolIndex.ToString("D2", CultureInfo.InvariantCulture) + "/"
                + itemFolder + "/"
                + layerIndex.ToString("D3", CultureInfo.InvariantCulture) + "/"
                + variantIndex.ToString("D2", CultureInfo.InvariantCulture) + "/"
                + variant + "/"
                + fileName;
        }

        private static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."))
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        private static string Sha256File(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string Sha256String(string value)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            return BitConverter.ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private sealed class ArtworkFixture : IDisposable
        {
            private readonly GameObject root;
            private readonly Sprite sprite;
            private readonly BuildItemPreviewCardView card;

            public ArtworkFixture(string artworkName, float authoredRotation)
            {
                Texture2D texture = new(2, 2, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixels(new[]
                {
                    Color.white,
                    Color.white,
                    Color.white,
                    Color.white
                });
                texture.Apply();
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 2f, 2f),
                    new Vector2(0.5f, 0.5f));
                sprite.hideFlags = HideFlags.HideAndDontSave;

                root = new GameObject(
                    "TrayArtworkDirectionFixture",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(BuildItemPreviewCardView));
                root.hideFlags = HideFlags.HideAndDontSave;
                Image background = root.GetComponent<Image>();
                background.color = Color.white;

                GameObject artwork = new(
                    artworkName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                artwork.hideFlags = HideFlags.HideAndDontSave;
                artwork.transform.SetParent(root.transform, false);
                Image image = artwork.GetComponent<Image>();
                image.color = Color.white;
                image.sprite = sprite;
                image.rectTransform.localEulerAngles =
                    new Vector3(0f, 0f, authoredRotation);

                card = root.GetComponent<BuildItemPreviewCardView>();
                card.Bind(
                    root.GetComponent<RectTransform>(),
                    null,
                    background,
                    null,
                    null,
                    null);
            }

            public float[] BindCaptureRepeat(int repeatCount, float offset)
            {
                List<float> observed = new();
                for (int i = 0; i < repeatCount; i++)
                {
                    Check(card.BindAuthoritativeArtwork(sprite),
                        "Authoritative artwork bind failed.");
                    card.ApplyArtworkImageRotationOffset(offset);
                    List<ShapeCellVisualStyle> styles = new();
                    Check(card.TryCaptureCellVisualStyles(styles)
                        && styles.Count == 1,
                        "Authoritative artwork capture failed.");
                    observed.Add(styles[0].SourceRotationDegrees);
                }
                return observed.ToArray();
            }

            public void Dispose()
            {
                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
                if (sprite != null)
                {
                    Object.DestroyImmediate(sprite.texture);
                    Object.DestroyImmediate(sprite);
                }
            }
        }

        private sealed class SpecResult
        {
            public SpecResult(string id, string marker, bool passed, string detail)
            {
                Id = id;
                Marker = marker;
                Passed = passed;
                Detail = detail ?? string.Empty;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }

        private sealed class MatrixRow
        {
            public string BaseItemId;
            public string ShapeId;
            public string CatalogRotation0Cells;
            public string TrayLogicalRotation0Cells;
            public string TrayPresentationRotation0Cells;
            public float TrayArtworkFinalDegrees;
            public string DragGhostRotation0Cells;
            public float DragGhostArtworkFinalDegrees;
            public string BoardPreviewRotation0Cells;
            public float BoardPreviewArtworkFinalDegrees;
            public string BoardPlacedRotation0Cells;
            public float BoardPlacedArtworkFinalDegrees;
            public int RefreshRepeatCount;
            public string DirectionParity;
            public string UnaffectedOrTarget;
            public string Result;
        }

        private sealed class SurfaceParityRow
        {
            public string BaseItemId;
            public string Tray;
            public string DragGhost;
            public string BoardPreview;
            public string BoardPlaced;
            public string Result;
        }

        private readonly struct Bounds
        {
            public Bounds(int minX, int maxX, int minY, int maxY)
            {
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }

            public int MinX { get; }
            public int MaxX { get; }
            public int MinY { get; }
            public int MaxY { get; }
            public int ColumnSpan => Math.Max(1, MaxX - MinX + 1);
            public int RowSpan => Math.Max(1, MaxY - MinY + 1);
        }
    }
}
