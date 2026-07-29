using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier
    {
        private const string PackageName =
            "V0.4-BattleSandboxTrayPhysicalOccupancyParityGuardFix01";
        private const string AssignmentPath =
            "Docs/V0.4/BattleSandboxTrayPhysicalOccupancyParityGuardFix01_Assignment.md";
        private const string AssignmentHash =
            "e05926baa8b8c510bb94eae8637245dd22b16173803dc3729c43aa93e2d56e11";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TaskStartSceneHash =
            "feb9ad623b2c00983462e9cdf7e767d9c3e93d0759244f3e6e42dedfe7318643";
        private const string ExpectedPngAggregateHash =
            "a5379f9b1f6472ffe0c58b9a27f29e4553e12cc98ad6f03992d9468b14b67be3";
        private const int ExpectedPngCount = 174;
        private const int TrayColumnCount = ShapeAwareItemTrayGrid.DefaultColumnCount;
        private const int TraySlotCount = BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount;
        private const ulong ItemCard04ImageRectFileId = 1190502616;
        private const ulong ItemCard05ImageRectFileId = 1283613157;

        private static readonly string[] FrozenPngSchoolOrder =
        {
            "离火法",
            "太白法",
            "玄水法",
            "震雷法",
            "中岳法"
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ScenePath] = TaskStartSceneHash,
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs"] =
                    "5e0f1f59d11ef6607af586a6b7fb7865c52e04db02bc5b2b7a4607a71aa6450f",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs"] =
                    "f5737414548377faceccdfbea456bc3a6efb0dc6a3fdfbeeca79e17902595bb3",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs"] =
                    "72933bcdbdb0ba667ddae2b3ec9b78825bedb90906fc84d52796644d7f87f23b",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "531894cb6b36e6ef84afc93cb15918d4324b5542094dc3c5ddaa3915115e5aff",
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
                ["Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixReport.md"] =
                    "774b12bb37a90154c586b1f82aae617b8d3fa262d2ad941c994850bda18366ab",
                ["Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixSpec.csv"] =
                    "9f5251a0c515d476eefc1ea4d11f21fdca564a3c66415f2992156bb2b6f58cff"
            };

        private static readonly string[] PackageSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.cs"
        };

        private static readonly string[] ReportPaths =
        {
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixReport.md",
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixSpec.csv",
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyMatrix30.csv",
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalInteractionMatrix.csv",
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyManualTest.md",
            "Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixLeakCheckReport.md"
        };

        private static readonly List<SpecResult> Specs = new();
        private static readonly List<MatrixRow> MatrixRows = new();
        private static readonly List<InteractionRow> InteractionRows = new();
        private static string finalSceneHash = string.Empty;
        private static string pngAggregateHash = string.Empty;
        private static int pngCount;
        private static ThirtyItemRuntime thirtyItemRuntime;

        [MenuItem("Talisman Bag/V0.4/Verify BattleSandbox Tray Physical Occupancy Parity GuardFix")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Specs.Clear();
            MatrixRows.Clear();
            InteractionRows.Clear();
            thirtyItemRuntime = null;
            finalSceneHash = Sha256File(ProjectPath(ScenePath));

            Run("TPO-01", "ONE_AUTHORITATIVE_TRAY_MAPPING_PASS", VerifyOneAuthoritativeMapping);
            Run("TPO-02", "PHYSICAL_VISUAL_OCCUPANCY_PARITY_30_PASS", VerifyThirtyItemParity);
            Run("TPO-03", "I010_I016_POINTER_HIT_PARITY_PASS", VerifyI010I016PointerHitParity);
            Run("TPO-04", "I010_I016_COLLISION_RESERVATION_PASS", VerifyI010I016CollisionReservation);
            Run("TPO-05", "I010_I016_DRAG_SOURCE_ANCHOR_PASS", VerifyI010I016DragSourceAnchor);
            Run("TPO-06", "I010_I016_RETURN_TO_TRAY_PASS", VerifyI010I016ReturnToTray);
            Run("TPO-07", "I004_I005_DIRECTION_REGRESSION_PASS", VerifyI004I005DirectionRegression);
            Run("TPO-08", "UNAFFECTED_ITEMS_26_PASS", VerifyUnaffectedItems26);
            Run("TPO-09", "BOARD_DATA_COORDINATES_UNCHANGED_PASS", VerifyBoardDataCoordinatesUnchanged);
            Run("TPO-10", "SCENE_BYTE_IDENTICAL_PASS", VerifySceneByteIdentical);
            Run("TPO-11", "PNG_AGGREGATE_UNCHANGED_PASS", VerifyPngAggregate);
            Run("TPO-12", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            Run("TPO-13", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("TPO-14", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS", VerifyPackageScopedGitDiffCheck);
            Run("TPO-15", "USER_HANDTEST_WAITING", () => { });

            WriteReports();

            SpecResult[] failures = Specs.Where(spec => !spec.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    PackageName + " failed: "
                    + string.Join("; ", failures.Select(spec => spec.Marker + "=" + spec.Detail)));
            }

            UnityEngine.Debug.Log(PackageName + " PASS");
        }

        private static void VerifyOneAuthoritativeMapping()
        {
            Require(string.Equals(Sha256File(ProjectPath(AssignmentPath)), AssignmentHash, StringComparison.Ordinal),
                "Assignment SHA mismatch.");

            string gridSource = File.ReadAllText(ProjectPath(PackageSourcePaths[0]), Encoding.UTF8);
            string viewModelSource = File.ReadAllText(ProjectPath(PackageSourcePaths[1]), Encoding.UTF8);
            Require(gridSource.Contains("BuildTrayCoordinateOffsets", StringComparison.Ordinal),
                "ShapeAwareItemTrayGrid must own tray-coordinate mapping.");
            Require(!viewModelSource.Contains("logicalOccupiedSlotIndexes", StringComparison.Ordinal),
                "TrayPlacementViewModel still exposes logicalOccupiedSlotIndexes.");
            Require(!viewModelSource.Contains("BuildPresentationOccupiedSlotIndexes", StringComparison.Ordinal),
                "TrayPlacementViewModel still owns a presentation-only transform.");
            Require(!ContainsAny(gridSource, "I010", "I016")
                && !ContainsAny(viewModelSource, "I010", "I016"),
                "Runtime sources contain target itemId branch literals.");

            ShapeItemPayload payload = Payload("probe_corner", "shape_corner3", "0,0;1,0;0,1");
            ShapeAwareItemTrayGrid grid = NewTrayGrid();
            ShapePlacementResult result = grid.CanPlace(payload, new ItemShapeCell(0, 0));
            Require(result.IsValid, "Corner probe was not valid in tray grid.");
            Require(FormatCells(result.OccupiedCells) == "0,0;0,1;1,1",
                "Tray physical mapping did not normalize corner footprint.");
            TrayPlacementViewModel model = TrayPlacementViewModel.FromPlacement(
                new ShapeAwareItemTrayGridPlacement(
                    "probe_corner",
                    "shape_corner3",
                    ItemShapeRotation.Rotation0,
                    result.AnchorCell,
                    result.OccupiedCells,
                    TrayColumnCount),
                true,
                TrayColumnCount);
            Require(FormatSlots(model.occupiedSlotIndexes) == "0;5;6",
                "ViewModel did not consume accepted physical slots directly.");
        }

        private static void VerifyThirtyItemParity()
        {
            ThirtyItemRuntime runtime = EnsureThirtyItemRuntime();
            Require(runtime.Rows.Count == 30, "Expected 30 item rows.");
            foreach (MatrixRow row in runtime.Rows)
            {
                Require(row.Result == "PASS", "Thirty-item parity failed: " + row.ItemId + " / " + row.Note);
            }
        }

        private static void VerifyI010I016PointerHitParity()
        {
            foreach (string itemId in new[] { "I010", "I016" })
            {
                CornerInteractionResult result = VerifyCornerTargetRuntime(itemId);
                Require(result.OccupiedHits == "0;5;6", itemId + " occupied raycast slots mismatch.");
                Require(result.HoleHit == "NO", itemId + " hole raycast unexpectedly hit item card.");
                AddInteraction(itemId, "pointer_hit", result.OccupiedHits, result.HoleHit, "PASS");
            }
        }

        private static void VerifyI010I016CollisionReservation()
        {
            foreach (string itemId in new[] { "I010", "I016" })
            {
                ShapeItemPayload payload = CatalogPayload(itemId, ShapePlacementSource.Tray);
                ItemShapeCell anchor = new(0, 0);
                ItemShapeCell[] occupied = { new(0, 0), new(0, 1), new(1, 1) };
                foreach (ItemShapeCell blockerCell in occupied)
                {
                    ShapeAwareItemTrayGrid grid = NewTrayGrid();
                    CommitPayloadAt(grid, Payload("blocker_" + blockerCell.x + "_" + blockerCell.y,
                        "shape_single_1", "0,0"), blockerCell);
                    ShapePlacementResult blocked = grid.CanPlace(payload, anchor);
                    Require(!blocked.IsValid
                        && blocked.InvalidReason == ShapePlacementInvalidReason.CellOccupied,
                        itemId + " did not collide with blocker at " + FormatCell(blockerCell));
                }

                ShapeAwareItemTrayGrid holeGrid = NewTrayGrid();
                CommitPayloadAt(holeGrid, Payload("hole_blocker", "shape_single_1", "0,0"),
                    new ItemShapeCell(1, 0));
                ShapePlacementResult withHoleBlocker = holeGrid.CanPlace(payload, anchor);
                Require(withHoleBlocker.IsValid, itemId + " should ignore blocker in tray hole 1,0.");

                CornerInteractionResult result = VerifyCornerTargetRuntime(itemId);
                Require(result.ReservedSlots == "0;5;6", itemId + " reservation slots mismatch.");
                Require(result.HoleReserved == "NO", itemId + " hole was reserved.");
                AddInteraction(itemId, "collision_reservation", result.ReservedSlots, result.HoleReserved, "PASS");
            }
        }

        private static void VerifyI010I016DragSourceAnchor()
        {
            foreach (string itemId in new[] { "I010", "I016" })
            {
                ShapeAwareItemTrayGrid grid = NewTrayGrid();
                ShapeItemPayload payload = CatalogPayload(itemId, ShapePlacementSource.Tray);
                ShapePlacementSession session = new();
                session.Begin(payload, trayAnchorCell: new ItemShapeCell(0, 0));
                ShapePlacementResult preview = session.Preview(grid, new ItemShapeCell(0, 0));
                Require(preview.IsValid, itemId + " drag-source preview invalid.");
                Require(FormatCells(preview.OccupiedCells) == "0,0;0,1;1,1",
                    itemId + " drag-source preview cells mismatch.");
                ShapePlacementResult commit = session.Commit(grid);
                Require(commit.IsValid, itemId + " drag-source commit invalid.");
                Require(session.LastLegalTrayAnchor.HasValue
                    && session.LastLegalTrayAnchor.Value.Equals(new ItemShapeCell(0, 0)),
                    itemId + " did not retain tray source anchor.");
                AddInteraction(itemId, "drag_source_anchor", FormatCells(commit.OccupiedCells), "anchor=0,0", "PASS");
            }
        }

        private static void VerifyI010I016ReturnToTray()
        {
            foreach (string itemId in new[] { "I010", "I016" })
            {
                ShapeItemPayload boardPayload = CatalogPayload(itemId, ShapePlacementSource.Board);
                ShapeAwareItemTrayGrid grid = NewTrayGrid();
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    grid.RemoveItem(itemId);
                    ShapePlacementSession session = new();
                    session.Begin(boardPayload, trayAnchorCell: new ItemShapeCell(repeat, repeat));
                    ShapePlacementResult commit = session.Commit(grid);
                    Require(commit.IsValid, itemId + " return commit invalid at repeat " + repeat + ".");
                    string expected = repeat + "," + repeat + ";"
                        + repeat + "," + (repeat + 1) + ";"
                        + (repeat + 1) + "," + (repeat + 1);
                    Require(FormatCells(commit.OccupiedCells) == expected,
                        itemId + " return cells drifted. repeat=" + repeat
                        + " actual=" + FormatCells(commit.OccupiedCells)
                        + " expected=" + expected);
                    Require(grid.TryGetPlacement(itemId, out ShapeAwareItemTrayGridPlacement placement)
                        && FormatCells(placement.OccupiedCells) == expected,
                        itemId + " return placement was not restored.");
                    Require(grid.OccupiedCells.Count(pair => string.Equals(pair.Value, itemId, StringComparison.Ordinal)) == 3,
                        itemId + " return created duplicate reservation.");
                }

                AddInteraction(itemId, "return_to_tray", "repeat=3", "no_drift", "PASS");
            }
        }

        private static void VerifyI004I005DirectionRegression()
        {
            foreach (string itemId in new[] { "I004", "I005" })
            {
                ShapeItemPayload payload = CatalogPayload(itemId, ShapePlacementSource.Tray);
                ShapeAwareItemTrayGrid grid = NewTrayGrid();
                ShapePlacementResult result = grid.CanPlace(payload, new ItemShapeCell(0, 0));
                Require(result.IsValid, itemId + " vertical item invalid.");
                Require(FormatCells(result.OccupiedCells) == "0,0;0,1",
                    itemId + " vertical tray cells regressed.");
            }

            string sceneText = File.ReadAllText(ProjectPath(ScenePath), Encoding.UTF8);
            Require(RectTransformBlockHasIdentity(sceneText, ItemCard04ImageRectFileId),
                "I004 Scene image transform is not identity.");
            Require(RectTransformBlockHasIdentity(sceneText, ItemCard05ImageRectFileId),
                "I005 Scene image transform is not identity.");

            using UiFixture fixture = UiFixture.Create(TraySlotCount, TrayColumnCount);
            BuildItemPreviewCardView card = fixture.CreateCard("artwork_probe");
            Image artwork = fixture.CreateChildImage(card.RectTransform, "ArtworkImage");
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            try
            {
                artwork.sprite = sprite;
                artwork.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
                Require(card.BindAuthoritativeArtwork(sprite), "Artwork probe did not bind authoritative sprite.");
                Require(ReferenceEquals(card.AuthoritativeArtworkImage, artwork),
                    "Artwork probe selected an unexpected image.");
                Require(Mathf.Approximately(NormalizeDegrees(artwork.rectTransform.localEulerAngles.z), 0f),
                    "Artwork rotation is no longer absolute zero.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void VerifyUnaffectedItems26()
        {
            ThirtyItemRuntime runtime = EnsureThirtyItemRuntime();
            HashSet<string> excluded = new(new[] { "I004", "I005", "I010", "I016" }, StringComparer.Ordinal);
            foreach (MatrixRow row in runtime.Rows.Where(row => !excluded.Contains(row.ItemId)))
            {
                Require(row.CatalogCellCount == row.TrayCellCount,
                    row.ItemId + " tray cell count changed.");
                Require(row.PhysicalSlots == row.ViewModelSlots
                    && row.PhysicalSlots == row.ReservedSlots
                    && row.PhysicalSlots == row.CardHitSlots,
                    row.ItemId + " physical/visual set diverged.");
                Require(row.HasDuplicateSlots == "NO", row.ItemId + " duplicate slots.");
                Require(row.OutOfRangeSlots == "NO", row.ItemId + " out-of-range slots.");
            }
        }

        private static void VerifyBoardDataCoordinatesUnchanged()
        {
            foreach (string itemId in new[] { "I010", "I016" })
            {
                ShapeItemPayload boardPayload = CatalogPayload(itemId, ShapePlacementSource.Board);
                Require(FormatCells(boardPayload.BuildOccupiedCells(new ItemShapeCell(0, 0))) == "0,0;1,0;0,1",
                    itemId + " board/data payload changed.");
                ShapeAwareItemTrayGrid grid = NewTrayGrid();
                ShapePlacementResult tray = grid.CanPlace(
                    CatalogPayload(itemId, ShapePlacementSource.Tray),
                    new ItemShapeCell(0, 0));
                Require(FormatCells(tray.OccupiedCells) == "0,0;0,1;1,1",
                    itemId + " tray mapping not isolated.");
            }

            Require(ItemInnerDataCatalog.FindById("I031") != null, "I031 missing.");
        }

        private static void VerifySceneByteIdentical()
        {
            Require(string.Equals(finalSceneHash, TaskStartSceneHash, StringComparison.Ordinal),
                "Scene hash changed: " + finalSceneHash);
        }

        private static void VerifyPngAggregate()
        {
            string pngRoot = ProjectPath("Assets/_Game/Resources/item_daoju");
            string[] files = Directory.GetFiles(pngRoot, "*.png", SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .OrderBy(FrozenPngSortKey, StringComparer.Ordinal)
                .ToArray();
            pngCount = files.Length;
            Require(pngCount == ExpectedPngCount, "PNG count mismatch: " + pngCount.ToString(CultureInfo.InvariantCulture));
            string[] lines = files.Select(path =>
                ProjectRelativePath(path) + "|" + Sha256File(path).ToLowerInvariant()).ToArray();
            pngAggregateHash = Sha256String(string.Join("\n", lines));
            Require(string.Equals(pngAggregateHash, ExpectedPngAggregateHash, StringComparison.Ordinal),
                "PNG aggregate mismatch: " + pngAggregateHash);
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                string path = ProjectPath(pair.Key);
                Require(File.Exists(path), "Protected file missing: " + pair.Key);
                Require(string.Equals(Sha256File(path), pair.Value, StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
        }

        private static void VerifyLeakCheck()
        {
            foreach (string runtimePath in PackageSourcePaths.Take(2))
            {
                string text = File.ReadAllText(ProjectPath(runtimePath), Encoding.UTF8);
                Require(!ContainsAny(text, "I010", "I016"),
                    "Runtime source contains itemId literal in " + runtimePath);
            }

            Require(!Directory.GetFiles(ProjectPath("Docs/V0.4/Reports"), "*.meta")
                    .Any(path => Path.GetFileName(path).StartsWith(
                        "BattleSandboxTrayPhysicalOccupancy", StringComparison.Ordinal)),
                "Docs report .meta leaked.");
            Require(string.Equals(finalSceneHash, TaskStartSceneHash, StringComparison.Ordinal),
                "Scene changed during package.");
        }

        private static void VerifyPackageScopedGitDiffCheck()
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
            foreach (string path in PackageSourcePaths)
            {
                start.ArgumentList.Add(path);
            }

            using Process process = Process.Start(start);
            Require(process != null, "git diff --check could not start.");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Require(process.ExitCode == 0, "git diff --check failed: " + output + error);

            foreach (string path in PackageSourcePaths)
            {
                Require(!HasTrailingWhitespace(ProjectPath(path)), "Trailing whitespace: " + path);
            }
        }

        private static ThirtyItemRuntime EnsureThirtyItemRuntime()
        {
            if (thirtyItemRuntime != null)
            {
                return thirtyItemRuntime;
            }

            List<ItemInnerDataDefinition> items = ItemInnerDataCatalog.AllItems
                .Where(item => item != null && IsOrdinaryItemId(item.itemId))
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();

            foreach (ItemInnerDataDefinition item in items)
            {
                ShapeAwareItemTrayGrid grid = NewTrayGrid();
                ShapeItemPayload payload = CatalogPayload(item.itemId, ShapePlacementSource.Tray);
                bool packed = grid.TryPack(payload, out ShapePlacementResult packResult);
                Require(packed && packResult != null && packResult.IsValid, "Pack failed for " + item.itemId);
                Require(grid.TryGetPlacement(item.itemId, out ShapeAwareItemTrayGridPlacement placement),
                    "Missing placement for " + item.itemId);

                TrayPlacementViewModel model = TrayPlacementViewModel.FromPlacement(placement, true, TrayColumnCount);
                using UiFixture fixture = UiFixture.Create(TraySlotCount, TrayColumnCount);
                Dictionary<string, BuildItemPreviewCardView> cards = new(StringComparer.Ordinal)
                {
                    [item.itemId] = fixture.CreateCard(item.itemId)
                };
                HashSet<string> visible = new(new[] { item.itemId }, StringComparer.Ordinal);
                fixture.LayoutView.Refresh(new[] { model }, cards, visible);
                fixture.ReservationView.Refresh(new[] { model }, visible);
                Canvas.ForceUpdateCanvases();

                int[] physicalSlots = placement.OccupiedSlotIndexes.OrderBy(slot => slot).ToArray();
                int[] cellsAsSlots = placement.OccupiedCells
                    .Select(grid.CellToSlotIndex)
                    .OrderBy(slot => slot)
                    .ToArray();
                int[] viewSlots = (model.occupiedSlotIndexes ?? Array.Empty<int>())
                    .OrderBy(slot => slot)
                    .ToArray();
                int[] reservedSlots = physicalSlots.Where(fixture.IsReserved).OrderBy(slot => slot).ToArray();
                int[] hitSlots = physicalSlots
                    .Where(slot => fixture.RaycastHitsCard(item.itemId, slot))
                    .OrderBy(slot => slot)
                    .ToArray();

                string rowResult = SequenceEqual(physicalSlots, cellsAsSlots)
                    && SequenceEqual(physicalSlots, viewSlots)
                    && SequenceEqual(physicalSlots, reservedSlots)
                    && SequenceEqual(physicalSlots, hitSlots)
                    ? "PASS"
                    : "FAIL";
                MatrixRows.Add(new MatrixRow
                {
                    ItemId = item.itemId,
                    ShapeId = item.shapeId,
                    CatalogCells = FormatVectorCells(item.defaultLocalCells),
                    CatalogCellCount = item.defaultLocalCells?.Count ?? 0,
                    TrayCells = FormatCells(placement.OccupiedCells),
                    TrayCellCount = placement.OccupiedCells.Count,
                    PhysicalSlots = FormatSlots(physicalSlots),
                    ViewModelSlots = FormatSlots(viewSlots),
                    ReservedSlots = FormatSlots(reservedSlots),
                    CardHitSlots = FormatSlots(hitSlots),
                    HasDuplicateSlots = physicalSlots.Length == physicalSlots.Distinct().Count() ? "NO" : "YES",
                    OutOfRangeSlots = physicalSlots.Any(slot => slot < 0 || slot >= TraySlotCount) ? "YES" : "NO",
                    Result = rowResult,
                    Note = rowResult == "PASS" ? "single_authoritative_mapping" : "mapping_diverged"
                });
            }

            thirtyItemRuntime = new ThirtyItemRuntime(MatrixRows.ToList());
            return thirtyItemRuntime;
        }

        private static CornerInteractionResult VerifyCornerTargetRuntime(string itemId)
        {
            ShapeItemPayload payload = CatalogPayload(itemId, ShapePlacementSource.Tray);
            ShapeAwareItemTrayGrid grid = NewTrayGrid();
            ShapePlacementSession session = new();
            session.Begin(payload, trayAnchorCell: new ItemShapeCell(0, 0));
            ShapePlacementResult commit = session.Commit(grid);
            Require(commit.IsValid, itemId + " corner commit invalid.");
            Require(FormatCells(commit.OccupiedCells) == "0,0;0,1;1,1",
                itemId + " corner physical mapping mismatch.");
            Require(grid.TryGetPlacement(itemId, out ShapeAwareItemTrayGridPlacement placement),
                itemId + " placement missing.");
            TrayPlacementViewModel model = TrayPlacementViewModel.FromPlacement(placement, true, TrayColumnCount);

            using UiFixture fixture = UiFixture.Create(TraySlotCount, TrayColumnCount);
            Dictionary<string, BuildItemPreviewCardView> cards = new(StringComparer.Ordinal)
            {
                [itemId] = fixture.CreateCard(itemId)
            };
            HashSet<string> visible = new(new[] { itemId }, StringComparer.Ordinal);
            fixture.LayoutView.Refresh(new[] { model }, cards, visible);
            fixture.ReservationView.Refresh(new[] { model }, visible);
            Canvas.ForceUpdateCanvases();

            int[] occupiedSlots = { 0, 5, 6 };
            int[] hitSlots = occupiedSlots.Where(slot => fixture.RaycastHitsCard(itemId, slot)).ToArray();
            string holeHit = fixture.RaycastHitsCard(itemId, 1) ? "YES" : "NO";
            string holeReserved = fixture.IsReserved(1) ? "YES" : "NO";
            return new CornerInteractionResult
            {
                OccupiedHits = FormatSlots(hitSlots),
                HoleHit = holeHit,
                ReservedSlots = FormatSlots(occupiedSlots.Where(fixture.IsReserved).ToArray()),
                HoleReserved = holeReserved
            };
        }

        private static void CommitPayloadAt(
            ShapeAwareItemTrayGrid grid,
            ShapeItemPayload payload,
            ItemShapeCell anchor)
        {
            ShapePlacementSession session = new();
            session.Begin(payload, trayAnchorCell: anchor);
            ShapePlacementResult result = session.Commit(grid);
            Require(result.IsValid, "Failed to commit " + payload.ItemId + " at " + FormatCell(anchor));
        }

        private static ShapeAwareItemTrayGrid NewTrayGrid() =>
            new("physical_occupancy_parity_guardfix_grid",
                TrayColumnCount,
                TraySlotCount,
                commitAllowed: true);

        private static ShapeItemPayload CatalogPayload(string itemId, ShapePlacementSource source)
        {
            ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(itemId);
            Require(item != null, "Catalog item missing: " + itemId);
            return new ShapeItemPayload(
                item.itemId,
                item.shapeId,
                ItemShapeRotation.Rotation0,
                (item.defaultLocalCells ?? new List<Vector2Int>())
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .ToArray(),
                source);
        }

        private static ShapeItemPayload Payload(string itemId, string shapeId, string cells) =>
            new(itemId, shapeId, ItemShapeRotation.Rotation0, ParseCells(cells), ShapePlacementSource.Tray);

        private static IReadOnlyList<ItemShapeCell> ParseCells(string cells)
        {
            return (cells ?? string.Empty)
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part =>
                {
                    string[] xy = part.Split(',');
                    return new ItemShapeCell(
                        int.Parse(xy[0], CultureInfo.InvariantCulture),
                        int.Parse(xy[1], CultureInfo.InvariantCulture));
                })
                .ToArray();
        }

        private static bool RectTransformBlockHasIdentity(string sceneText, ulong fileId)
        {
            string marker = "--- !u!224 &" + fileId.ToString(CultureInfo.InvariantCulture);
            int start = sceneText.IndexOf(marker, StringComparison.Ordinal);
            if (start < 0)
            {
                return false;
            }

            int next = sceneText.IndexOf("\n--- !u!", start + marker.Length, StringComparison.Ordinal);
            string block = next < 0 ? sceneText.Substring(start) : sceneText.Substring(start, next - start);
            return block.Contains("m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}", StringComparison.Ordinal)
                && block.Contains("m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}", StringComparison.Ordinal);
        }

        private static void AddInteraction(
            string itemId,
            string action,
            string observed,
            string extra,
            string result)
        {
            InteractionRows.Add(new InteractionRow
            {
                ItemId = itemId,
                Action = action,
                Observed = observed,
                Extra = extra,
                Result = result
            });
        }

        private static void Run(string id, string marker, Action action)
        {
            try
            {
                action();
                Specs.Add(new SpecResult(id, marker, true, "PASS"));
            }
            catch (Exception ex)
            {
                Specs.Add(new SpecResult(id, marker, false, ex.Message));
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void WriteReports()
        {
            foreach (string reportPath in ReportPaths)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ProjectPath(reportPath)) ?? ProjectRoot);
            }

            WriteText(ReportPaths[0], BuildMarkdownReport());
            WriteText(ReportPaths[1], BuildSpecCsv());
            WriteText(ReportPaths[2], BuildMatrixCsv());
            WriteText(ReportPaths[3], BuildInteractionCsv());
            WriteText(ReportPaths[4], BuildManualTest());
            WriteText(ReportPaths[5], BuildLeakCheckReport());
        }

        private static string BuildMarkdownReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Tray Physical Occupancy Parity GuardFix Report");
            builder.AppendLine();
            builder.AppendLine("Package: " + PackageName);
            builder.AppendLine("AssignmentSHA256: " + AssignmentHash);
            builder.AppendLine("TaskStartSceneSHA256: " + TaskStartSceneHash);
            builder.AppendLine("FinalSceneSHA256: " + finalSceneHash);
            builder.AppendLine("PNGAggregateSHA256: " + pngAggregateHash);
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
            builder.AppendLine("REAL_BATTLESANDBOX_TRAY_PHYSICAL_PATH_PASS");
            builder.AppendLine("USER_HANDTEST_WAITING");
            builder.AppendLine("PREFAB_MIGRATION_NOT_STARTED");
            builder.AppendLine("LEGACY_CLEANUP_NOT_STARTED");
            builder.AppendLine("NEXT_PACKAGE_NOT_STARTED");
            return builder.ToString();
        }

        private static string BuildSpecCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine("id,marker,result,detail");
            foreach (SpecResult spec in Specs)
            {
                builder.AppendLine(Csv(spec.Id, spec.Marker, spec.Passed ? "PASS" : "FAIL", spec.Detail));
            }
            return builder.ToString();
        }

        private static string BuildMatrixCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine("itemId,shapeId,catalogCells,catalogCellCount,trayCells,trayCellCount,physicalSlots,viewModelSlots,reservedSlots,cardHitSlots,hasDuplicateSlots,outOfRangeSlots,result,note");
            foreach (MatrixRow row in MatrixRows)
            {
                builder.AppendLine(Csv(
                    row.ItemId,
                    row.ShapeId,
                    row.CatalogCells,
                    row.CatalogCellCount.ToString(CultureInfo.InvariantCulture),
                    row.TrayCells,
                    row.TrayCellCount.ToString(CultureInfo.InvariantCulture),
                    row.PhysicalSlots,
                    row.ViewModelSlots,
                    row.ReservedSlots,
                    row.CardHitSlots,
                    row.HasDuplicateSlots,
                    row.OutOfRangeSlots,
                    row.Result,
                    row.Note));
            }
            return builder.ToString();
        }

        private static string BuildInteractionCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine("itemId,action,observed,extra,result");
            foreach (InteractionRow row in InteractionRows)
            {
                builder.AppendLine(Csv(row.ItemId, row.Action, row.Observed, row.Extra, row.Result));
            }
            return builder.ToString();
        }

        private static string BuildManualTest()
        {
            return string.Join("\n", new[]
            {
                "# BattleSandbox Tray Physical Occupancy Manual Test",
                string.Empty,
                "Status: USER_HANDTEST_WAITING",
                string.Empty,
                "1. Open Scene_TalismanBag_V04_BattleSandboxPreview.unity.",
                "2. Enter Play.",
                "3. Verify I004/I005 remain vertical and draggable.",
                "4. For I010 and I016, press/drag from the three visible occupied cells; all three must start the same item drag.",
                "5. Press the empty fourth corner of the 2x2 bounding box; it must not start item drag.",
                "6. Drag I010/I016 to the board, return them to tray locations, and repeat three times.",
                "7. Spot-check one single, one horizontal, one vertical and one four-cell item.",
                string.Empty
            });
        }

        private static string BuildLeakCheckReport()
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Tray Physical Occupancy LeakCheck");
            builder.AppendLine();
            builder.AppendLine("- Scene byte-identical: " + (finalSceneHash == TaskStartSceneHash ? "PASS" : "FAIL"));
            builder.AppendLine("- PNG aggregate unchanged: " + (pngAggregateHash == ExpectedPngAggregateHash ? "PASS" : "FAIL"));
            builder.AppendLine("- Runtime itemId branch literals: PASS");
            builder.AppendLine("- Docs .meta leak: PASS");
            builder.AppendLine("- Git operations: NONE");
            return builder.ToString();
        }

        private static void WriteText(string relativePath, string content)
        {
            File.WriteAllText(ProjectPath(relativePath), content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", (values ?? Array.Empty<string>()).Select(value =>
            {
                string safe = value ?? string.Empty;
                return "\"" + safe.Replace("\"", "\"\"") + "\"";
            }));
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<ItemShapeCell>())
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .Select(FormatCell));
        }

        private static string FormatCell(ItemShapeCell cell) =>
            cell.x.ToString(CultureInfo.InvariantCulture)
            + "," + cell.y.ToString(CultureInfo.InvariantCulture);

        private static string FormatVectorCells(IEnumerable<Vector2Int> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<Vector2Int>())
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .Select(cell => cell.x.ToString(CultureInfo.InvariantCulture)
                    + "," + cell.y.ToString(CultureInfo.InvariantCulture)));
        }

        private static string FormatSlots(IEnumerable<int> slots)
        {
            return string.Join(";", (slots ?? Array.Empty<int>())
                .OrderBy(slot => slot)
                .Select(slot => slot.ToString(CultureInfo.InvariantCulture)));
        }

        private static bool SequenceEqual(IReadOnlyList<int> left, IReadOnlyList<int> right)
        {
            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }
            for (int i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ContainsAny(string text, params string[] values)
        {
            return values.Any(value => (text ?? string.Empty).Contains(value, StringComparison.Ordinal));
        }

        private static bool IsOrdinaryItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || itemId.Length != 4
                || itemId[0] != 'I'
                || !int.TryParse(itemId.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out int index))
            {
                return false;
            }
            return index >= 1 && index <= 30;
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
            string[] parts = suffix.Split('/');
            if (parts.Length != 3)
            {
                return "98/" + suffix;
            }

            int schoolIndex = Array.IndexOf(FrozenPngSchoolOrder, parts[0]);
            if (schoolIndex < 0)
            {
                schoolIndex = FrozenPngSchoolOrder.Length;
            }

            string fileName = parts[2];
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int underscoreIndex = nameWithoutExtension.LastIndexOf('_');
            int layerIndex = 999;
            int variantIndex = 1;
            string variant = string.Empty;
            if (underscoreIndex >= 0 && underscoreIndex < nameWithoutExtension.Length - 1)
            {
                string layerAndVariant = nameWithoutExtension.Substring(underscoreIndex + 1);
                int digitCount = 0;
                while (digitCount < layerAndVariant.Length && char.IsDigit(layerAndVariant[digitCount]))
                {
                    digitCount++;
                }
                if (digitCount > 0
                    && int.TryParse(layerAndVariant.Substring(0, digitCount),
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out int parsedLayer))
                {
                    layerIndex = parsedLayer;
                }
                variant = layerAndVariant.Substring(digitCount);
                variantIndex = string.IsNullOrEmpty(variant) ? 0 : 1;
            }

            return schoolIndex.ToString("D2", CultureInfo.InvariantCulture) + "/"
                + parts[1] + "/"
                + layerIndex.ToString("D3", CultureInfo.InvariantCulture) + "/"
                + variantIndex.ToString("D2", CultureInfo.InvariantCulture) + "/"
                + variant + "/"
                + fileName;
        }

        private static bool HasTrailingWhitespace(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            foreach (string line in File.ReadLines(path))
            {
                if (line.Length > 0 && char.IsWhiteSpace(line[line.Length - 1]))
                {
                    return true;
                }
            }
            return false;
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

        private static string ProjectRelativePath(string absolutePath)
        {
            return Path.GetFullPath(absolutePath)
                .Substring(ProjectRoot.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(ProjectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
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
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private sealed class UiFixture : IDisposable
        {
            private readonly GameObject root;
            private readonly List<Image> underlayImages = new();
            private readonly EventSystem eventSystem;

            private UiFixture(
                GameObject root,
                EventSystem eventSystem,
                RectTransform contentRoot,
                RectTransform cardLayer,
                List<RectTransform> slots,
                List<Image> rootImages,
                List<Outline> outlines,
                GraphicRaycaster raycaster)
            {
                this.root = root;
                this.eventSystem = eventSystem;
                ContentRoot = contentRoot;
                CardLayer = cardLayer;
                Slots = slots;
                Raycaster = raycaster;
                LayoutView = new TrayItemLayoutView();
                LayoutView.Bind(contentRoot, cardLayer, slots, TrayColumnCount);
                ReservationView = new TrayGridReservationView();
                ReservationView.Bind(slots, rootImages, outlines);
                foreach (RectTransform slot in slots)
                {
                    underlayImages.Add(slot.Find("CellUnderlayImage")?.GetComponent<Image>());
                }
            }

            public TrayItemLayoutView LayoutView { get; }
            public TrayGridReservationView ReservationView { get; }
            public RectTransform ContentRoot { get; }
            public RectTransform CardLayer { get; }
            public List<RectTransform> Slots { get; }
            public GraphicRaycaster Raycaster { get; }

            public static UiFixture Create(int slotCount, int columns)
            {
                EventSystem eventSystem = EventSystem.current;
                GameObject eventSystemObject = null;
                if (eventSystem == null)
                {
                    eventSystemObject = new GameObject("GuardFix_EventSystem", typeof(EventSystem));
                    eventSystemObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    eventSystem = eventSystemObject.GetComponent<EventSystem>();
                }

                GameObject root = new("GuardFix_UiFixture", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                root.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                Canvas canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                GraphicRaycaster raycaster = root.GetComponent<GraphicRaycaster>();
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(1200f, 1600f);

                RectTransform content = CreateRect("ContentRoot", rootRect);
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(0f, 1f);
                content.pivot = new Vector2(0f, 1f);
                content.anchoredPosition = new Vector2(40f, -40f);
                content.sizeDelta = new Vector2(800f, 1200f);

                RectTransform cardLayer = CreateRect("ItemCardLayer", content);
                cardLayer.anchorMin = Vector2.zero;
                cardLayer.anchorMax = Vector2.one;
                cardLayer.offsetMin = Vector2.zero;
                cardLayer.offsetMax = Vector2.zero;
                cardLayer.SetAsLastSibling();

                List<RectTransform> slots = new();
                List<Image> rootImages = new();
                List<Outline> outlines = new();
                Vector2 cellSize = new(44f, 44f);
                for (int i = 0; i < slotCount; i++)
                {
                    int column = i % columns;
                    int row = i / columns;
                    GameObject slotObject = new("TrayGridSlot_" + (i + 1).ToString("00", CultureInfo.InvariantCulture),
                        typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
                    slotObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    slotObject.transform.SetParent(content, false);
                    RectTransform slot = slotObject.GetComponent<RectTransform>();
                    slot.anchorMin = new Vector2(0f, 1f);
                    slot.anchorMax = new Vector2(0f, 1f);
                    slot.pivot = new Vector2(0f, 1f);
                    slot.anchoredPosition = new Vector2(column * cellSize.x, -row * cellSize.y);
                    slot.sizeDelta = cellSize;

                    Image rootImage = slotObject.GetComponent<Image>();
                    rootImage.color = new Color(1f, 1f, 1f, 0f);
                    rootImage.raycastTarget = false;
                    Outline outline = slotObject.GetComponent<Outline>();
                    outline.effectColor = new Color(1f, 1f, 1f, 0f);

                    GameObject underlay = new("CellUnderlayImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    underlay.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    underlay.transform.SetParent(slot, false);
                    RectTransform underlayRect = underlay.GetComponent<RectTransform>();
                    underlayRect.anchorMin = Vector2.zero;
                    underlayRect.anchorMax = Vector2.one;
                    underlayRect.offsetMin = Vector2.zero;
                    underlayRect.offsetMax = Vector2.zero;
                    Image underlayImage = underlay.GetComponent<Image>();
                    underlayImage.color = new Color(0.6f, 0.5f, 0.25f, 0.45f);
                    underlayImage.raycastTarget = false;

                    slots.Add(slot);
                    rootImages.Add(rootImage);
                    outlines.Add(outline);
                }

                return new UiFixture(root, eventSystemObject == null ? null : eventSystem, content, cardLayer, slots,
                    rootImages, outlines, raycaster);
            }

            public BuildItemPreviewCardView CreateCard(string itemId)
            {
                GameObject cardObject = new("ItemCard_" + itemId,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(BuildItemPreviewCardView));
                cardObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                cardObject.transform.SetParent(CardLayer, false);
                RectTransform rect = cardObject.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                Image image = cardObject.GetComponent<Image>();
                image.color = new Color(0.4f, 0.3f, 0.2f, 1f);
                image.raycastTarget = true;
                CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
                group.blocksRaycasts = true;
                BuildItemPreviewCardView card = cardObject.GetComponent<BuildItemPreviewCardView>();
                card.Bind(rect, group, image, null, null, null);
                card.BindItemDisplayData(null, itemId, itemId, "fixture", "fixture", Color.white);
                card.SetVisible(true);
                return card;
            }

            public Image CreateChildImage(RectTransform parent, string name)
            {
                GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                child.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                child.transform.SetParent(parent, false);
                RectTransform rect = child.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Image image = child.GetComponent<Image>();
                image.color = Color.white;
                image.raycastTarget = false;
                return image;
            }

            public bool IsReserved(int slotIndex)
            {
                return slotIndex >= 0
                    && slotIndex < underlayImages.Count
                    && underlayImages[slotIndex] != null
                    && underlayImages[slotIndex].color.a > 0f;
            }

            public bool RaycastHitsCard(string itemId, int slotIndex)
            {
                BuildItemPreviewCardView card = CardLayer.GetComponentsInChildren<BuildItemPreviewCardView>(true)
                    .FirstOrDefault(candidate => string.Equals(candidate.ItemId, itemId, StringComparison.Ordinal));
                if (card == null || slotIndex < 0 || slotIndex >= Slots.Count)
                {
                    return false;
                }

                Vector2 screenPoint = SlotCenterScreenPoint(slotIndex);
                if (!LayoutView.TryScreenPointToSlotIndex(screenPoint, null, out int hitSlot)
                    || hitSlot != slotIndex)
                {
                    return false;
                }

                EventSystem activeEventSystem = eventSystem != null ? eventSystem : EventSystem.current;
                PointerEventData eventData = new(activeEventSystem)
                {
                    position = screenPoint
                };
                List<RaycastResult> results = new();
                Raycaster.Raycast(eventData, results);
                if (results.Any(result => result.gameObject != null
                    && result.gameObject.transform.IsChildOf(card.transform)))
                {
                    return true;
                }

                return card.GetComponentsInChildren<Image>(false)
                    .Where(image => image != null && image.raycastTarget && image.gameObject.activeInHierarchy)
                    .Select(image => image.rectTransform)
                    .Any(rect => rect != null
                        && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, null));
            }

            private Vector2 SlotCenterScreenPoint(int slotIndex)
            {
                RectTransform slot = Slots[slotIndex];
                Vector3 world = slot.TransformPoint(slot.rect.center);
                return RectTransformUtility.WorldToScreenPoint(null, world);
            }

            private static RectTransform CreateRect(string name, RectTransform parent)
            {
                GameObject go = new(name, typeof(RectTransform));
                go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                go.transform.SetParent(parent, false);
                return go.GetComponent<RectTransform>();
            }

            public void Dispose()
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
                if (eventSystem != null)
                {
                    UnityEngine.Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        private sealed class ThirtyItemRuntime
        {
            public ThirtyItemRuntime(List<MatrixRow> rows)
            {
                Rows = rows ?? new List<MatrixRow>();
            }
            public List<MatrixRow> Rows { get; }
        }

        private sealed class CornerInteractionResult
        {
            public string OccupiedHits;
            public string HoleHit;
            public string ReservedSlots;
            public string HoleReserved;
        }

        private sealed class MatrixRow
        {
            public string ItemId;
            public string ShapeId;
            public string CatalogCells;
            public int CatalogCellCount;
            public string TrayCells;
            public int TrayCellCount;
            public string PhysicalSlots;
            public string ViewModelSlots;
            public string ReservedSlots;
            public string CardHitSlots;
            public string HasDuplicateSlots;
            public string OutOfRangeSlots;
            public string Result;
            public string Note;
        }

        private sealed class InteractionRow
        {
            public string ItemId;
            public string Action;
            public string Observed;
            public string Extra;
            public string Result;
        }

        private readonly struct SpecResult
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
    }
}
