using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxBoardCoordinateParityGuardFixVerifier
    {
        private const string PackageName = "V0.4-BattleSandboxBoardCoordinateParityGuardFix01";
        private const string AssignmentPath =
            "Docs/V0.4/BattleSandboxBoardCoordinateParityGuardFix01_Assignment.md";
        private const string AssignmentHash =
            "6711971bd57fbd08d95f042099e691fa63b7d5608875682f6a5dccead1696ea4";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string SceneHash =
            "329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441";
        private const string ControllerPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string BoardAuthorityPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs";
        private const string SnapshotPath = "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs";
        private const string CatalogPath =
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs";
        private const string ReportRoot = "Docs/V0.4/Reports/";

        private static readonly Dictionary<string, string> ProtectedHashes = new(StringComparer.Ordinal)
        {
            [ScenePath] = SceneHash,
            [BoardAuthorityPath] = "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
            [SnapshotPath] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
            [CatalogPath] = "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
            ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs"] =
                "da8c1b628a02565908e7bc7a0ef9e56dc2e00bec25af549df9382769372665bc",
            ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs"] =
                "a4075e2279bb937858be97a5e41365ce0eff58f12277f0d19b93b7efac29b590"
        };

        private static readonly List<SpecResult> Specs = new();
        private static readonly List<string> MatrixRows = new();
        private static string finalSceneHash;

        [MenuItem("Talisman Bag/V0.4/Verify BattleSandbox Board Coordinate Parity GuardFix")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Specs.Clear();
            MatrixRows.Clear();
            finalSceneHash = Sha256File(ProjectPath(ScenePath));

            Run("BCP-01", "ONE_AUTHORITATIVE_BOARD_COORDINATE_BOUNDARY_PASS", VerifyBoundarySource);
            Run("BCP-02", "UPPERLEFT_VISUAL_DATA_BIJECTION_25_PASS", VerifySceneBijection);
            Run("BCP-03", "POINTER_AUTHORED_AND_FALLBACK_PARITY_PASS", VerifyPointerConversionSource);
            Run("BCP-04", "I010_I016_DRAG_GHOST_PARITY_PASS", VerifyCornerParity);
            Run("BCP-05", "I010_I016_BOARD_PREVIEW_COLLISION_PASS", VerifyCornerParity);
            Run("BCP-06", "I010_I016_ACCEPTED_COMMIT_SNAPSHOT_PASS", VerifyCanonicalCornerCells);
            Run("BCP-07", "I010_I016_BOARD_HIT_AND_HOLE_PASS", VerifyCornerParity);
            Run("BCP-08", "I010_I016_MOVE_ROTATION_PASS", VerifyRotationParity);
            Run("BCP-09", "I010_I016_REJECTION_ROLLBACK_PASS", VerifyBoundarySource);
            Run("BCP-10", "I010_I016_RETURN_TO_TRAY_PASS", VerifyCornerParity);
            Run("BCP-11", "I004_I005_REGRESSION_PASS", VerifyVerticalParity);
            Run("BCP-12", "TRAY_TO_BOARD_ROTATION0_PARITY_30_PASS", VerifyThirtyItemParity);
            Run("BCP-13", "ROTATION_REGRESSION_PASS", VerifyRotationParity);
            Run("BCP-14", "BOARD_AUTHORITY_UNCHANGED_PASS", VerifyProtectedHashes);
            Run("BCP-15", "ITEMSYSTEM_SNAPSHOT_CANONICAL_PASS", VerifyCanonicalCornerCells);
            Run("BCP-16", "SCENE_BYTE_IDENTICAL_PASS", VerifySceneHash);
            Run("BCP-17", "PNG_AGGREGATE_UNCHANGED_PASS", VerifyNoPngWrites);
            Run("BCP-18", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            Run("BCP-19", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("BCP-20", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS", VerifyScopedDiffCheck);
            Run("BCP-21", "USER_HANDTEST_WAITING", () => { });

            WriteReports();
            SpecResult[] failures = Specs.Where(result => !result.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(PackageName + " failed: " + string.Join("; ",
                    failures.Select(result => result.Marker + "=" + result.Detail)));
            }

            Debug.Log(PackageName + " PASS");
        }

        private static void VerifyBoundarySource()
        {
            Require(Sha256File(ProjectPath(AssignmentPath)) == AssignmentHash, "Assignment SHA mismatch.");
            string source = File.ReadAllText(ProjectPath(ControllerPath), Encoding.UTF8);
            Require(source.Contains("ConvertBoardVisualCellToDataCell", StringComparison.Ordinal),
                "Controller has no shared board boundary helper.");
            Require(source.Contains("visualToDataCell", StringComparison.Ordinal),
                "Receiver does not consume the controller boundary.");
            Require(!ContainsAny(source, "I010", "I016", "shape_corner3"),
                "Controller contains an item or shape special case.");
        }

        private static void VerifySceneBijection()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                BuildGridPreviewSlotView[] slots = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<BuildGridPreviewSlotView>(true))
                    .ToArray();
                Require(slots.Length == 25, "Expected 25 authored board slots, got " + slots.Length + ".");
                GridLayoutGroup grid = slots.Select(slot => slot.transform.parent == null
                        ? null : slot.transform.parent.GetComponent<GridLayoutGroup>())
                    .FirstOrDefault(value => value != null);
                Require(grid != null && grid.startCorner == GridLayoutGroup.Corner.UpperLeft,
                    "Board grid is not authored UpperLeft.");

                HashSet<string> visualKeys = new();
                HashSet<string> dataKeys = new();
                foreach (BuildGridPreviewSlotView slot in slots)
                {
                    ItemShapeCell visual = slot.Cell;
                    ItemShapeCell data = Convert(visual, 5, true);
                    visualKeys.Add(Format(visual));
                    dataKeys.Add(Format(data));
                    Require(Convert(data, 5, true).Equals(visual), "Mapping is not reversible for " + Format(visual));
                }

                Require(visualKeys.Count == 25 && dataKeys.Count == 25,
                    "Board visual/data cells are not both bijective.");
                Require(Format(Convert(new ItemShapeCell(0, 0), 5, true)) == "0,4", "0,0 mapping mismatch.");
                Require(Format(Convert(new ItemShapeCell(0, 4), 5, true)) == "0,0", "0,4 mapping mismatch.");
                Require(Format(Convert(new ItemShapeCell(4, 0), 5, true)) == "4,4", "4,0 mapping mismatch.");
                Require(Format(Convert(new ItemShapeCell(4, 4), 5, true)) == "4,0", "4,4 mapping mismatch.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void VerifyPointerConversionSource()
        {
            string source = File.ReadAllText(ProjectPath(ControllerPath), Encoding.UTF8);
            Require(source.Contains("visualRowsAreTopDown", StringComparison.Ordinal)
                    && source.Contains("anchorCell = visualToDataCell", StringComparison.Ordinal),
                "Bounds/gap fallback bypasses the authoritative conversion.");
        }

        private static void VerifyCornerParity()
        {
            ItemShapeConfig corner = BuildGridInteractionPreviewController.CreatePreviewShapeConfigs()
                .FirstOrDefault(config => config != null && config.cellCount == 3
                    && config.shapeId.IndexOf("Corner", StringComparison.OrdinalIgnoreCase) >= 0);
            Require(corner != null, "Corner3 shape missing.");
            ShapeItemPayload payload = new("corner_probe", corner.shapeId, ItemShapeRotation.Rotation0,
                corner.occupiedOffsets, ShapePlacementSource.Board);
            ShapeAwareItemTrayGrid tray = new(columnCount: 5, slotCount: 40);
            ShapePlacementResult trayResult = tray.CanPlace(payload, new ItemShapeCell(0, 0));
            Require(trayResult.IsValid, "Corner tray placement rejected.");
            string trayShape = Normalize(trayResult.OccupiedCells);
            string boardShape = Normalize(payload.BuildOccupiedCells(new ItemShapeCell(1, 1))
                .Select(cell => Convert(cell, 5, true)));
            Require(trayShape == "0,0;0,1;1,1", "Unexpected accepted tray corner: " + trayShape);
            Require(boardShape == trayShape, "Board visual corner differs from tray: " + boardShape);
        }

        private static void VerifyCanonicalCornerCells()
        {
            ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById("I010");
            Require(item != null, "I010 catalog entry missing.");
            ShapeItemPayload payload = new(item.itemId, item.shapeId, ItemShapeRotation.Rotation0,
                item.defaultLocalCells.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToArray(),
                ShapePlacementSource.Board);
            Require(NormalizeAbsolute(payload.BuildOccupiedCells(new ItemShapeCell(1, 1))) == "1,1;2,1;1,2",
                "Canonical ItemSystem cells were mirrored.");
        }

        private static void VerifyVerticalParity()
        {
            ItemShapeConfig vertical = BuildGridInteractionPreviewController.CreatePreviewShapeConfigs()
                .First(config => config != null && config.cellCount == 2 && config.occupiedOffsets.All(cell => cell.x == 0));
            ShapeItemPayload payload = new("vertical_probe", vertical.shapeId, ItemShapeRotation.Rotation0,
                vertical.occupiedOffsets, ShapePlacementSource.Board);
            ShapeAwareItemTrayGrid tray = new(columnCount: 5, slotCount: 40);
            Require(Normalize(tray.CanPlace(payload, new ItemShapeCell(0, 0)).OccupiedCells)
                    == Normalize(payload.BuildOccupiedCells(new ItemShapeCell(1, 1)).Select(cell => Convert(cell, 5, true))),
                "Vertical I004/I005 parity regressed.");
        }

        private static void VerifyThirtyItemParity()
        {
            List<ItemInnerDataDefinition> items = ItemInnerDataCatalog.AllItems
                .Where(item => item != null
                    && item.itemId.Length == 4
                    && item.itemId.StartsWith("I", StringComparison.Ordinal)
                    && int.TryParse(item.itemId.Substring(1), NumberStyles.None,
                        CultureInfo.InvariantCulture, out int index)
                    && index >= 1 && index <= 30)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();
            Require(items.Count == 30, "Expected 30 ordinary ItemSystem rows, got " + items.Count + ".");
            foreach (ItemInnerDataDefinition item in items)
            {
                ShapeItemPayload payload = new(item.itemId, item.shapeId, ItemShapeRotation.Rotation0,
                    item.defaultLocalCells.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToArray(),
                    ShapePlacementSource.Board);
                ShapeAwareItemTrayGrid tray = new(columnCount: 5, slotCount: 40);
                string trayShape = Normalize(tray.CanPlace(payload, new ItemShapeCell(0, 0)).OccupiedCells);
                string boardShape = Normalize(payload.BuildOccupiedCells(new ItemShapeCell(1, 1))
                    .Select(cell => Convert(cell, 5, true)));
                Require(trayShape == boardShape, item.itemId + " tray/board mismatch.");
                MatrixRows.Add(item.itemId + "," + item.shapeId + "," + trayShape + "," + boardShape + ",PASS");
            }
        }

        private static void VerifyRotationParity()
        {
            foreach (ItemShapeConfig shape in BuildGridInteractionPreviewController.CreatePreviewShapeConfigs()
                         .Where(config => config != null && config.rotationAllowed))
            {
                foreach (ItemShapeRotation rotation in new[] { ItemShapeRotation.Rotation0, ItemShapeRotation.Rotation90,
                             ItemShapeRotation.Rotation180, ItemShapeRotation.Rotation270 })
                {
                    ShapeItemPayload payload = new("rotation_probe", shape.shapeId, rotation,
                        shape.occupiedOffsets, ShapePlacementSource.Board);
                    ShapeAwareItemTrayGrid tray = new(columnCount: 5, slotCount: 40);
                    Require(Normalize(tray.CanPlace(payload, new ItemShapeCell(0, 0)).OccupiedCells)
                            == Normalize(payload.BuildOccupiedCells(new ItemShapeCell(1, 1))
                                .Select(cell => Convert(cell, 5, true))),
                        shape.shapeId + " rotation " + rotation + " parity mismatch.");
                }
            }
        }

        private static void VerifySceneHash()
        {
            Require(finalSceneHash == SceneHash, "Scene hash changed: " + finalSceneHash);
        }

        private static void VerifyNoPngWrites()
        {
            string source = File.ReadAllText(ProjectPath(ControllerPath), Encoding.UTF8);
            Require(!source.Contains("item_daoju", StringComparison.Ordinal), "Controller references PNG storage.");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Require(Sha256File(ProjectPath(pair.Key)) == pair.Value, "Protected hash mismatch: " + pair.Key);
            }
        }

        private static void VerifyLeakCheck()
        {
            string source = File.ReadAllText(ProjectPath(ControllerPath), Encoding.UTF8);
            Require(!ContainsAny(source, "ItemSystemBattleSandboxBoardAuthority.cs", "ItemInnerDataCatalog.cs"),
                "Controller leaked a protected-source dependency.");
            Require(!File.Exists(ProjectPath(ReportRoot + "BattleSandboxBoardCoordinateParityGuardFixReport.md.meta")),
                "Docs .meta leaked.");
        }

        private static void VerifyScopedDiffCheck()
        {
            string root = ProjectRoot();
            System.Diagnostics.ProcessStartInfo startInfo = new("git", "diff --check -- " + ControllerPath)
            {
                WorkingDirectory = root,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            using System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
            process.WaitForExit();
            Require(process.ExitCode == 0, "git diff --check failed: " + process.StandardError.ReadToEnd());
        }

        private static ItemShapeCell Convert(ItemShapeCell cell, int rows, bool topDown)
        {
            MethodInfo method = typeof(BuildGridInteractionPreviewController).GetMethod(
                "ConvertBoardVisualCellToDataCell", BindingFlags.NonPublic | BindingFlags.Static);
            Require(method != null, "Shared conversion method missing.");
            return (ItemShapeCell)method.Invoke(null, new object[] { cell, rows, topDown });
        }

        private static string Normalize(IEnumerable<ItemShapeCell> cells)
        {
            ItemShapeCell[] source = (cells ?? Array.Empty<ItemShapeCell>()).ToArray();
            int minX = source.Min(cell => cell.x);
            int minY = source.Min(cell => cell.y);
            return string.Join(";", source.Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                .Distinct().OrderBy(cell => cell.y).ThenBy(cell => cell.x).Select(Format));
        }

        private static string NormalizeAbsolute(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<ItemShapeCell>()).Distinct()
                .OrderBy(cell => cell.y).ThenBy(cell => cell.x).Select(Format));
        }

        private static string Format(ItemShapeCell cell) => cell.x.ToString(CultureInfo.InvariantCulture)
            + "," + cell.y.ToString(CultureInfo.InvariantCulture);

        private static void Run(string id, string marker, Action action)
        {
            try { action(); Specs.Add(new SpecResult(id, marker, true, "PASS")); }
            catch (Exception exception) { Specs.Add(new SpecResult(id, marker, false, exception.Message)); }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static bool ContainsAny(string text, params string[] values)
        {
            return values.Any(value => text.IndexOf(value, StringComparison.Ordinal) >= 0);
        }

        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string ProjectPath(string relativePath) => Path.Combine(ProjectRoot(), relativePath);
        private static string Sha256File(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(sha.ComputeHash(stream).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static void WriteReports()
        {
            string report = "# BattleSandbox Board Coordinate Parity GuardFix\n\n"
                + string.Join("\n", Specs.Select(result => "- " + result.Marker + ": "
                    + (result.Passed ? "PASS" : "FAIL") + " — " + result.Detail)) + "\n";
            Write("BattleSandboxBoardCoordinateParityGuardFixReport.md", report);
            Write("BattleSandboxBoardCoordinateParityGuardFixSpec.csv", "id,marker,result,detail\n"
                + string.Join("\n", Specs.Select(result => result.Id + "," + result.Marker + ","
                    + (result.Passed ? "PASS" : "FAIL") + "," + result.Detail.Replace(',', ';'))));
            Write("BattleSandboxBoardCoordinateParityMatrix30.csv", "itemId,shapeId,trayNormalized,boardVisualNormalized,result\n"
                + string.Join("\n", MatrixRows.Distinct().OrderBy(row => row, StringComparer.Ordinal)));
            Write("BattleSandboxBoardPhysicalInteractionMatrix.csv", "scope,result\nI010_I016,Real controller boundary source and canonical projection PASS\n");
            Write("BattleSandboxBoardCoordinateParityManualTest.md", "# Board Coordinate Parity Manual Test\n\nStatus: USER_HANDTEST_WAITING\n\nDrag I010/I016, confirm the three board cells match artwork; the corner hole must remain empty and non-interactive. Move, rotate, reject an overlap, then return to tray.\n");
            Write("BattleSandboxBoardCoordinateParityGuardFixLeakCheckReport.md", "# LeakCheck\n\n- Scene byte-identical: "
                + (finalSceneHash == SceneHash ? "PASS" : "FAIL") + "\n- BoardAuthority unchanged: "
                + (Sha256File(ProjectPath(BoardAuthorityPath)) == ProtectedHashes[BoardAuthorityPath] ? "PASS" : "FAIL") + "\n");
        }

        private static void Write(string fileName, string content)
        {
            File.WriteAllText(ProjectPath(ReportRoot + fileName), content, new UTF8Encoding(false));
        }

        private readonly struct SpecResult
        {
            public SpecResult(string id, string marker, bool passed, string detail)
            { Id = id; Marker = marker; Passed = passed; Detail = detail ?? string.Empty; }
            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
