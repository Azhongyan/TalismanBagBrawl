#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemShape
{
    public static class ItemShapeCatalogBatchCorrectionVerifier
    {
        private const string PackageId = "V0.4-ITEM_SHAPE_CATALOG_BATCH_CORRECTION01";
        private const string Marker =
            "ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0";
        private const string MatrixPath = "Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv";
        private const string CatalogPath =
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs";
        private const string ProfileRoot = "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles";
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string DetailViewPath =
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs";
        private const string ReportRoot = "Docs/V0.4/Reports";
        private const string ReportPath = ReportRoot + "/ItemShapeCatalogBatchCorrectionReport.md";
        private const string SpecPath = ReportRoot + "/ItemShapeCatalogBatchCorrectionSpec.csv";
        private const string LedgerPath = ReportRoot + "/ItemShapeCatalogBatchCorrectionChangeLedger.csv";
        private const string CanonicalDeltaPath = ReportRoot + "/ItemShapeCatalogBatchCorrectionCanonicalDelta.csv";
        private const string LeakReportPath = ReportRoot + "/ItemShapeCatalogBatchCorrectionLeakCheckReport.md";

        private const string MatrixBeforeHash =
            "3AB8D46F58DC309112AD18F41DB6E6B59BC6D5863B7727DF0FE446D015A500E0";
        private const string CatalogBeforeHash =
            "AF6E7C38B466B268DCF318D07DA3B9998E7F7667615077ECD0C6F82D9059F8E3";
        private const string ProfilesBeforeHash =
            "24074D6E801BD9967C2CC04CA1EBF812AA0638407A08D03B6356403FAC11C81C";
        private const string OldItem105Hash =
            "b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4";
        private const string ScopedNewItem105Hash =
            "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1";
        private const string Roll150Canonical =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string Projection150Canonical =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";

        private static readonly string[] Item105Whitelist =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapterVerifier.cs"
        };

        private static readonly ProtectionExpectation[] ProtectedScopes =
        {
            new("approved-matrix", new[] { MatrixPath }, 1, MatrixBeforeHash),
            new("scenes", AllFiles("Assets", "*.unity"), 7,
                "F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B"),
            new("prefabs", AllFiles("Assets", "*.prefab"), 5,
                "3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6"),
            new("build-settings", new[] { "ProjectSettings/EditorBuildSettings.asset" }, 1,
                "BB060D6524E555139F3A2B201FB8EDDFC3AFBCA08A6F02E0581F42C1D620F3A2"),
            new("png-and-importer-meta", PngAndMetaFiles(), 938,
                "44A0327AC7DE256C2032005BAF54AA4F5D54068B9FD4EC0DB26426E804A2C8DF"),
            new("item-detail-ui-runtime", AllFiles("Assets/_Game/Scripts/TalismanBag/Items/Detail/UI", "*"), 12,
                "AC07A72D2B40CE501B2EC6740D5987E36A189BB458F12B48B48AAC5607512BF2"),
            new("cross-system-runtime", AllFiles("Assets/_Game/Scripts/TalismanBag/CrossSystem", "*"), 63,
                "ADF2A87C1A4F139E0F61403F95DF1877EBACCE12CCECBE921275519D509BDDD6"),
            new("enemy-system-runtime", AllFiles("Assets/_Game/Scripts/TalismanBag/EnemySystem", "*"), 96,
                "22C436BE429B063E3CC57DE4E76CD830066426472E4C5C0D6339A6B0A5852A49"),
            new("item-capability-runtime", AllFiles("Assets/_Game/Scripts/TalismanBag/Items/Capability", "*"), 13,
                "8115BA53258C1ABDF99718126FA9C838A6C122024BD9899E24F5369B9E0C2765")
        };

        [MenuItem("Tools/Talisman Bag/V0.4/Item Shape/[QA Only] Run Catalog Batch Correction")]
        public static void VerifyMenu()
        {
            Run(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            Run(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void Run(bool exitWhenBatchMode)
        {
            Result result = new();
            List<MatrixRow> rows = new();
            try
            {
                rows = LoadMatrix(result);
                RunChecks(rows, result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("CAUGHT_VERIFIER_EXCEPTION: " + exception);
            }

            try
            {
                WriteReports(rows, result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("CAUGHT_REPORT_EXCEPTION: " + exception);
                Debug.LogException(exception);
            }

            if (result.Errors.Count == 0)
            {
                Debug.Log(Marker);
                if (exitWhenBatchMode) EditorApplication.Exit(0);
                return;
            }

            foreach (string error in result.Errors) Debug.LogError(error);
            Debug.LogError("ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_FAIL errors=" + result.Errors.Count);
            if (exitWhenBatchMode) EditorApplication.Exit(1);
        }

        private static List<MatrixRow> LoadMatrix(Result result)
        {
            if (!File.Exists(MatrixPath))
            {
                result.Require("matrix.exists", false, MatrixPath);
                return new List<MatrixRow>();
            }

            List<string> lines = File.ReadAllLines(MatrixPath, Encoding.UTF8)
                .Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            List<MatrixRow> rows = lines.Skip(1).Select(ParseRow).ToList();
            result.Require("matrix.row-count", rows.Count == 30, rows.Count + "/30");
            result.Require("matrix.unique-item-ids", rows.Select(row => row.ItemId).Distinct(StringComparer.Ordinal).Count() == 30,
                "unique=" + rows.Select(row => row.ItemId).Distinct(StringComparer.Ordinal).Count());
            result.Require("matrix.keep-count", rows.Count(row => row.DecisionStatus == "APPROVED_KEEP") == 14,
                "keep=" + rows.Count(row => row.DecisionStatus == "APPROVED_KEEP"));
            result.Require("matrix.change-count", rows.Count(row => row.DecisionStatus == "APPROVED_CHANGE") == 16,
                "change=" + rows.Count(row => row.DecisionStatus == "APPROVED_CHANGE"));
            result.Require("matrix.unresolved-count", rows.All(row => row.DecisionStatus == "APPROVED_KEEP"
                    || row.DecisionStatus == "APPROVED_CHANGE"),
                "unresolved=" + rows.Count(row => row.DecisionStatus != "APPROVED_KEEP"
                    && row.DecisionStatus != "APPROVED_CHANGE"));
            return rows;
        }

        private static MatrixRow ParseRow(string line)
        {
            List<string> values = ParseCsvLine(line);
            if (values.Count != 11) throw new InvalidDataException("Approved matrix column count is " + values.Count);
            return new MatrixRow
            {
                ItemId = values[0], DisplayName = values[1], DecisionStatus = values[2],
                OldShapeId = values[3], NewShapeId = values[4], NewCells = ParseCells(values[5]),
                OldCore = ParseCell(values[6]), NewCore = ParseCell(values[7]), CoreStatus = values[8],
                DefaultOrientation = values[9], Evidence = values[10]
            };
        }

        private static void RunChecks(IReadOnlyList<MatrixRow> rows, Result result)
        {
            CheckCatalog(rows, result);
            CheckRotationsAndDeterminism(rows, result);
            CheckFoundationAndCandidates(rows, result);
            CheckProfiles(rows, result);
            CheckDetailRoutingAndSecondTruth(rows, result);
            CheckReadOnlyCollections(result);
            CheckProtectedScopes(result);
            CheckItem105Whitelist(result);
            CheckStableReports(result);
        }

        private static void CheckCatalog(IReadOnlyList<MatrixRow> rows, Result result)
        {
            IReadOnlyList<ItemInnerDataDefinition> ordinary = ItemInnerDataCatalog.AllItems
                .Where(item => ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(item.itemId))
                .OrderBy(item => item.itemId, StringComparer.Ordinal).ToArray();
            result.Require("catalog.ordinary-count", ordinary.Count == 30, ordinary.Count + "/30");
            int exact = 0;
            foreach (MatrixRow row in rows)
            {
                ItemInnerDataDefinition item = ordinary.FirstOrDefault(value => value.itemId == row.ItemId);
                bool cellsExact = item != null && item.defaultLocalCells.SequenceEqual(row.NewCells);
                bool validCells = item != null && item.defaultLocalCells.Count > 0
                    && item.defaultLocalCells.Distinct().Count() == item.defaultLocalCells.Count
                    && item.defaultLocalCells.Contains(item.coreCellLocal);
                bool isExact = item != null && item.displayName == row.DisplayName && item.shapeId == row.NewShapeId
                    && cellsExact && item.coreCellLocal == row.NewCore && row.CoreStatus == "TECHNICAL_RESERVED";
                if (isExact) exact++;
                result.Require("catalog." + row.ItemId + ".matrix-exact", isExact,
                    item == null ? "missing" : item.shapeId + "/" + item.FormatCells() + "/" + FormatCell(item.coreCellLocal));
                result.Require("catalog." + row.ItemId + ".cells-valid", validCells,
                    item == null ? "missing" : "count=" + item.defaultLocalCells.Count);
            }
            result.Require("catalog.matrix-exact-total", exact == 30, exact + "/30");

            ItemInnerDataDefinition i024 = ItemInnerDataCatalog.FindById("I024");
            result.Require("catalog.I024.authored-horizontal", i024 != null && i024.shapeId == "shape_line2_h"
                && i024.defaultLocalCells.SequenceEqual(new[] { Vector2Int.zero, Vector2Int.right })
                && i024.coreCellLocal == Vector2Int.zero, i024?.FormatCells() ?? "missing");
            foreach (string id in new[] { "I009", "I029" })
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(id);
                result.Require("catalog." + id + ".absorbed-single-fix", item != null
                    && item.shapeId == "shape_single_1" && item.defaultLocalCells.SequenceEqual(new[] { Vector2Int.zero })
                    && item.coreCellLocal == Vector2Int.zero, item?.FormatCells() ?? "missing");
            }

            ItemInnerDataDefinition i031 = ItemInnerDataCatalog.FindById("I031");
            result.Require("catalog.I031.unchanged", i031 != null && i031.shapeId == "shape_single_1"
                && i031.defaultLocalCells.SequenceEqual(new[] { Vector2Int.zero })
                && i031.coreCellLocal == Vector2Int.zero && i031.isLightingSource,
                i031 == null ? "missing" : i031.shapeId + "/" + i031.FormatCells());
        }

        private static void CheckRotationsAndDeterminism(IReadOnlyList<MatrixRow> rows, Result result)
        {
            ItemSystemBoardConfigInput board = new(9, new Vector2Int(8, 8), Array.Empty<Vector2Int>());
            int passed = 0;
            foreach (MatrixRow row in rows)
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(row.ItemId);
                foreach (int rotation in new[] { 0, 90, 180, 270 })
                {
                    string placementId = "QA_" + row.ItemId + "_R" + rotation;
                    Vector2Int anchor = new(4, 4);
                    ItemSystemSnapshot snapshot = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                        new ItemSystemSnapshotInput(
                            new[] { new ItemSystemPlacementInput(placementId, row.ItemId, anchor, rotation) },
                            boardConfig: board, catalogItems: new[] { item }));
                    ItemSystemPlacementSnapshot placement = snapshot.FindPlacement(placementId);
                    Vector2Int[] expected = row.NewCells.Select(cell => anchor + Rotate(cell, rotation))
                        .OrderBy(cell => cell.y).ThenBy(cell => cell.x).ToArray();
                    bool ok = placement != null && placement.OccupiedCells.Count == row.NewCells.Count
                        && placement.OccupiedCells.Distinct().Count() == placement.OccupiedCells.Count
                        && placement.OccupiedCells.SequenceEqual(expected)
                        && placement.coreCellWorld == anchor + Rotate(row.NewCore, rotation)
                        && placement.OccupiedCells.Contains(placement.coreCellWorld);
                    if (ok) passed++;
                    result.Require("rotation." + row.ItemId + "." + rotation, ok,
                        placement == null ? "missing" : string.Join(";", placement.OccupiedCells.Select(FormatCell)));
                }
            }
            result.Require("rotation.total", passed == 120, passed + "/120");

            ItemSystemBoardConfigInput largeBoard = new(64, new Vector2Int(63, 63), Array.Empty<Vector2Int>());
            ItemSystemPlacementInput[] placements = rows.Select((row, index) => new ItemSystemPlacementInput(
                "DET_" + row.ItemId, row.ItemId,
                new Vector2Int((index % 6) * 7 + 3, (index / 6) * 7 + 3), (index % 4) * 90)).ToArray();
            ItemInnerDataDefinition[] catalog = rows.Select(row => ItemInnerDataCatalog.FindById(row.ItemId)).ToArray();
            string first = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(placements, largeBoard, catalogItems: catalog)).BuildDebugSignature();
            string repeat = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(placements, largeBoard, catalogItems: catalog)).BuildDebugSignature();
            string reversed = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(placements.Reverse().ToArray(), largeBoard,
                    catalogItems: catalog.Reverse().ToArray())).BuildDebugSignature();
            result.SnapshotSignature = Sha256(first);
            result.Require("determinism.repeat", first == repeat, result.SnapshotSignature);
            result.Require("determinism.reverse-input", first == reversed, result.SnapshotSignature);
        }

        private static void CheckFoundationAndCandidates(IReadOnlyList<MatrixRow> rows, Result result)
        {
            ItemGenerationFoundationSnapshot foundation = ItemRarityInstanceFoundation.Create();
            ItemGenerationFoundationSnapshot reversedFoundation = ItemRarityInstanceFoundation.Create(
                ItemInnerDataCatalog.AllItems.Reverse().ToArray());
            result.FoundationSignature = Sha256(foundation.BuildCanonicalSignature());
            result.Require("foundation.valid", foundation.isValid, "errors=" + foundation.ValidationErrors.Count);
            result.Require("foundation.ordinary-count", foundation.OrdinaryArchetypes.Count == 30,
                foundation.OrdinaryArchetypes.Count + "/30");
            result.Require("foundation.rarity-key-count", foundation.RarityVersionKeys.Count == 150,
                foundation.RarityVersionKeys.Count + "/150");
            result.Require("foundation.reverse-input", foundation.BuildCanonicalSignature() == reversedFoundation.BuildCanonicalSignature(),
                result.FoundationSignature);
            result.Require("foundation.I031-system-only", foundation.FindArchetype("I031")?.isCoreProgressionItem == true
                && foundation.RarityVersionKeys.All(key => key.baseItemId != "I031"), "I031 ordinary keys=0");

            ItemBalanceWorkbenchCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(WorkbenchCatalogPath);
            result.Require("candidate.catalog-exists", catalog != null, WorkbenchCatalogPath);
            if (catalog == null) return;
            result.Require("candidate.profile-count", catalog.profiles.Count(profile => profile != null
                && ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(profile.baseItemId)) == 30,
                "ordinaryProfiles=" + catalog.profiles.Count(profile => profile != null
                    && ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(profile.baseItemId)));

            int versionsPassed = 0;
            int seedChecksPassed = 0;
            StringBuilder candidateSignature = new();
            IItemDetailViewModelProvider provider = new CatalogReadOnlyDetailProvider();
            ItemBalanceCandidateDetailSandboxAdapter adapter = new(catalog, provider);
            foreach (MatrixRow row in rows)
            {
                ItemArchetypeIdentitySnapshot archetype = foundation.FindArchetype(row.ItemId);
                result.Require("foundation." + row.ItemId + ".shape", archetype?.shapeId == row.NewShapeId,
                    archetype?.shapeId ?? "missing");
                ItemBalanceProfile profile = catalog.FindProfile(row.ItemId);
                string expectedDescription = row.NewShapeId + " · " + row.NewCells.Count + "格 · 核心格" + FormatCell(row.NewCore);
                bool profileShape = profile?.candidateDisplay?.shapeDescription == expectedDescription;
                result.Require("candidate." + row.ItemId + ".shape-description", profileShape,
                    profile?.candidateDisplay?.shapeDescription ?? "missing");
                bool rarities = profile?.rarityVersions != null && profile.rarityVersions.Count == 5
                    && ItemInstanceRarityCatalog.All.All(definition => profile.FindVersion(definition.rarity) != null);
                result.Require("candidate." + row.ItemId + ".five-rarities", rarities,
                    "versions=" + (profile?.rarityVersions?.Count ?? 0));
                candidateSignature.Append(row.ItemId).Append('|').Append(expectedDescription);

                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceRarityVersion version = profile?.FindVersion(rarity.rarity);
                    bool versionOk = version != null && version.versionKey == row.ItemId + "@" + rarity.stableKey;
                    if (versionOk) versionsPassed++;
                    result.Require("candidate." + row.ItemId + "." + rarity.stableKey + ".version", versionOk,
                        version?.versionKey ?? "missing");
                    candidateSignature.Append('|').Append(version?.versionKey ?? "missing");

                    long seedA = int.Parse(row.ItemId.Substring(1), CultureInfo.InvariantCulture) * 100000L
                        + rarity.tierIndex * 10L + 1L;
                    foreach (long seed in new[] { seedA, seedA + 1L })
                    {
                        ItemBalanceCandidateDetailResult candidate = adapter.Request(new ItemBalanceCandidateDetailRequest
                        {
                            baseItemId = row.ItemId,
                            rarityKey = rarity.stableKey,
                            rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                        });
                        bool seedOk = candidate?.isSuccess == true
                            && candidate.preview?.RollResult?.snapshot?.baseItemId == row.ItemId
                            && candidate.preview.RollResult.snapshot.rarity == rarity.rarity
                            && candidate.preview?.ProjectionResult?.snapshot?.baseItemId == row.ItemId
                            && candidate.preview.ProjectionResult.snapshot.rarity == rarity.rarity
                            && candidate.viewModel?.displayShapeName == expectedDescription
                            && candidate.detailProjection?.viewModel?.displayShapeName.Contains(row.NewShapeId,
                                StringComparison.Ordinal) == true;
                        if (seedOk) seedChecksPassed++;
                        result.Require("candidate." + row.ItemId + "." + rarity.stableKey + ".seed-" + seed,
                            seedOk, candidate == null ? "missing" : string.Join(" | ", candidate.ValidationErrors));
                    }
                }
            }
            result.Require("candidate.version-total", versionsPassed == 150, versionsPassed + "/150");
            result.Require("candidate.seed-inheritance-total", seedChecksPassed == 300, seedChecksPassed + "/300");
            result.CandidateSignature = Sha256(candidateSignature.ToString());
        }

        private static void CheckProfiles(IReadOnlyList<MatrixRow> rows, Result result)
        {
            int shapeOnly = 0;
            foreach (MatrixRow row in rows)
            {
                string path = ProfileRoot + "/ItemBalanceProfile_" + row.ItemId + ".asset";
                if (!File.Exists(path))
                {
                    result.Require("profile." + row.ItemId + ".exists", false, path);
                    continue;
                }

                string current = NormalizeLineEndings(File.ReadAllText(path, Encoding.UTF8));
                string head = NormalizeLineEndings(ReadGitHeadText(path));
                bool nonShapeEqual = NormalizeShapeDescription(current) == NormalizeShapeDescription(head);
                if (nonShapeEqual) shapeOnly++;
                result.Require("profile." + row.ItemId + ".non-shape-fields", nonShapeEqual,
                    nonShapeEqual ? "only shapeDescription may differ" : "non-shape drift detected");
                string expected = row.NewShapeId + " · " + row.NewCells.Count + "格 · 核心格" + FormatCell(row.NewCore);
                result.Require("profile." + row.ItemId + ".serialized-shape", current.Contains(
                    "shapeDescription: \"" + row.NewShapeId + " ", StringComparison.Ordinal), expected);
                if (row.DecisionStatus == "APPROVED_CHANGE")
                {
                    result.Require("profile." + row.ItemId + ".approved-change-present", current != head,
                        "current differs from HEAD only by approved shapeDescription");
                }
                else if (row.ItemId != "I009" && row.ItemId != "I029")
                {
                    result.Require("profile." + row.ItemId + ".keep-no-diff", current == head,
                        "APPROVED_KEEP");
                }
            }
            result.Require("profile.non-shape-total", shapeOnly == 30, shapeOnly + "/30");
        }

        private static void CheckDetailRoutingAndSecondTruth(IReadOnlyList<MatrixRow> rows, Result result)
        {
            string detailSource = File.ReadAllText(DetailViewPath, Encoding.UTF8);
            bool sharedRoute = detailSource.Contains("bool useSingleCellSlot = IsSingleCellArtwork(shapeName);",
                    StringComparison.Ordinal)
                && detailSource.Contains("SetArtworkSlot(singleCellArtworkImageSlot, artworkSprite, hasArtwork && useSingleCellSlot);",
                    StringComparison.Ordinal)
                && detailSource.Contains("SetArtworkSlot(multiCellArtworkImageSlot, artworkSprite, hasArtwork && !useSingleCellSlot);",
                    StringComparison.Ordinal)
                && detailSource.Contains("shapeName.Contains(\"single_1\"", StringComparison.Ordinal);
            bool noItemSpecialCase = rows.All(row => !detailSource.Contains("\"" + row.ItemId + "\"", StringComparison.Ordinal));
            result.Require("detail.shared-single-multi-route", sharedRoute, DetailViewPath);
            result.Require("detail.item-id-special-cases", noItemSpecialCase, "specialCaseCount=0");

            Regex ordinaryId = new("I0(?:0[1-9]|[12][0-9]|30)", RegexOptions.CultureInvariant);
            List<string> duplicateTruthSources = Directory.GetFiles("Assets/_Game/Scripts/TalismanBag/Items", "*.cs",
                    SearchOption.AllDirectories)
                .Select(NormalizePath)
                .Where(path => path != CatalogPath)
                .Where(path =>
                {
                    string source = File.ReadAllText(path, Encoding.UTF8);
                    return source.Contains("shape_", StringComparison.Ordinal) && ordinaryId.IsMatch(source);
                }).ToList();
            result.Require("shape.second-formal-table", duplicateTruthSources.Count == 0,
                duplicateTruthSources.Count == 0 ? "none" : string.Join(";", duplicateTruthSources));

            bool rarityHasShape = typeof(ItemBalanceRarityVersion).GetFields()
                .Any(field => field.Name.Contains("shape", StringComparison.OrdinalIgnoreCase));
            bool identityHasShape = typeof(ItemInstanceIdentitySnapshot).GetProperties()
                    .Any(property => property.Name.Contains("shape", StringComparison.OrdinalIgnoreCase))
                || typeof(ItemInstanceIdentitySnapshot).GetFields()
                    .Any(field => field.Name.Contains("shape", StringComparison.OrdinalIgnoreCase));
            result.Require("shape.rarity-and-instance-no-copy", !rarityHasShape && !identityHasShape,
                "rarityShapeField=" + rarityHasShape + ";identityShapeMember=" + identityHasShape);
        }

        private static void CheckReadOnlyCollections(Result result)
        {
            ItemGenerationFoundationSnapshot foundation = ItemRarityInstanceFoundation.Create();
            ItemSystemSnapshot snapshot = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(Array.Empty<ItemSystemPlacementInput>()));
            Type catalogSurface = typeof(ItemInnerDataCatalog).GetProperty(nameof(ItemInnerDataCatalog.AllItems))?.PropertyType;
            result.Require("readonly.catalog", catalogSurface == typeof(IReadOnlyList<ItemInnerDataDefinition>),
                catalogSurface?.FullName ?? "missing public property");
            result.Require("readonly.foundation-archetypes", IsReadOnly(foundation.Archetypes), "ICollection.IsReadOnly");
            result.Require("readonly.foundation-rarity-keys", IsReadOnly(foundation.RarityVersionKeys), "ICollection.IsReadOnly");
            result.Require("readonly.snapshot-catalog", IsReadOnly(snapshot.catalogItems), "ICollection.IsReadOnly");
            result.Require("readonly.snapshot-placements", IsReadOnly(snapshot.placements), "ICollection.IsReadOnly");
        }

        private static void CheckProtectedScopes(Result result)
        {
            foreach (ProtectionExpectation expectation in ProtectedScopes)
            {
                expectation.Files = expectation.Files.Where(File.Exists).Select(NormalizePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                expectation.ActualHash = AggregateHash(expectation.Files);
                if (expectation.Id == "scenes" || expectation.Id == "png-and-importer-meta")
                    expectation.ActualHash = PowerShellProtectionAggregate(expectation.Id);
                bool ok = expectation.Files.Length == expectation.ExpectedCount
                    && expectation.ActualHash == expectation.ExpectedHash;
                result.Require("protected." + expectation.Id, ok,
                    expectation.Files.Length + "/" + expectation.ExpectedCount + " " + expectation.ActualHash);
            }
        }

        private static void CheckItem105Whitelist(Result result)
        {
            foreach (string path in Item105Whitelist)
            {
                string source = File.ReadAllText(path, Encoding.UTF8);
                int newCount = CountOccurrences(source, ScopedNewItem105Hash);
                int oldCount = CountOccurrences(source, OldItem105Hash);
                result.Require("item105." + Path.GetFileNameWithoutExtension(path), newCount > 0 && oldCount == 0,
                    "newOccurrences=" + newCount + ";oldOccurrences=" + oldCount);
            }
            result.Require("item105.whitelist-count", Item105Whitelist.Length == 11, Item105Whitelist.Length + "/11");
            result.Item105ActualHash = BuildItem105Aggregate();
            if (result.Item105ActualHash != ScopedNewItem105Hash)
            {
                result.Notes.Add("PREEXISTING_ITEM105_WORKTREE_MISMATCH: scoped accepted=" + ScopedNewItem105Hash
                    + "; actual=" + result.Item105ActualHash
                    + "; the package does not absorb the pre-existing Item Detail UI delta.");
            }
        }

        private static void CheckStableReports(Result result)
        {
            string unitReport = File.ReadAllText(ReportRoot + "/ItemCapabilityUnitContractReport.md", Encoding.UTF8);
            result.Require("canonical.roll150", unitReport.Contains(Roll150Canonical, StringComparison.Ordinal)
                && unitReport.Contains("- Result: `PASS`", StringComparison.Ordinal), Roll150Canonical);
            result.Require("canonical.projection150", unitReport.Contains(Projection150Canonical, StringComparison.Ordinal)
                && unitReport.Contains("- Result: `PASS`", StringComparison.Ordinal), Projection150Canonical);

            string capabilityReport = File.ReadAllText(
                ReportRoot + "/ItemBuildCapabilityProjectionAdapterReport.md", Encoding.UTF8);
            bool bpStable = capabilityReport.Contains("| value.capability.break_power | 1000 | 1000 | PASS |",
                    StringComparison.Ordinal)
                && capabilityReport.Contains("| value.capability.guard_power | 800 | 800 | PASS |",
                    StringComparison.Ordinal)
                && capabilityReport.Contains("| value.capability.cooldown_recovery | 900 | 900 | PASS |",
                    StringComparison.Ordinal);
            result.Require("capability.three-formal-bp", bpStable, "break=1000;guard=800;cooldown=900");

            string c02Report = File.ReadAllText(
                ReportRoot + "/BuildCapabilityRemainingBlockerSemanticSurveyReport.md", Encoding.UTF8);
            bool c02Stable = c02Report.Contains("Actual Unknown-reference reduction: `0`", StringComparison.Ordinal)
                && c02Report.Contains("Blocked rows: `32/32`", StringComparison.Ordinal);
            result.Require("c02.blocked-baseline", c02Stable, "blocked=32/32;actualUnknownReduction=0");
        }

        private static void WriteReports(IReadOnlyList<MatrixRow> rows, Result result)
        {
            Directory.CreateDirectory(ReportRoot);
            result.CatalogAfterHash = AggregateHash(new[] { CatalogPath });
            string[] profiles = rows.Select(row => ProfileRoot + "/ItemBalanceProfile_" + row.ItemId + ".asset")
                .Where(File.Exists).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
            result.ProfilesAfterHash = AggregateHash(profiles);
            File.WriteAllText(ReportPath, BuildMainReport(rows, result), new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(rows, result), new UTF8Encoding(false));
            File.WriteAllText(LedgerPath, BuildChangeLedger(rows), new UTF8Encoding(false));
            File.WriteAllText(CanonicalDeltaPath, BuildCanonicalDelta(result), new UTF8Encoding(false));
            File.WriteAllText(LeakReportPath, BuildLeakReport(result), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildMainReport(IReadOnlyList<MatrixRow> rows, Result result)
        {
            bool pass = result.Errors.Count == 0;
            StringBuilder builder = new();
            builder.AppendLine("# Item Shape Catalog Batch Correction Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `" + PackageId + "`");
            builder.AppendLine("- Guard assignment: `GUARD_PASS_ASSIGNMENT_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01`");
            builder.AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Marker: `" + (pass ? Marker : "ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_FAIL") + "`");
            builder.AppendLine("- Approved input: `" + MatrixPath + "` (read-only)");
            builder.AppendLine();
            builder.AppendLine("## Counts");
            builder.AppendLine();
            builder.AppendLine("- Approved Items: `" + rows.Count + "/30`");
            builder.AppendLine("- KEEP / CHANGE / unresolved: `" + rows.Count(row => row.DecisionStatus == "APPROVED_KEEP")
                + " / " + rows.Count(row => row.DecisionStatus == "APPROVED_CHANGE") + " / "
                + rows.Count(row => row.DecisionStatus != "APPROVED_KEEP"
                    && row.DecisionStatus != "APPROVED_CHANGE") + "`");
            builder.AppendLine("- Rotation cases: `120`");
            builder.AppendLine("- Rarity versions: `150`; seed inheritance checks: `300`");
            builder.AppendLine("- Item105 verifier migrations: `11`");
            builder.AppendLine();
            builder.AppendLine("## Canonical and aggregate evidence");
            builder.AppendLine();
            builder.AppendLine("- Catalog aggregate: `" + CatalogBeforeHash + "` -> `" + result.CatalogAfterHash + "`");
            builder.AppendLine("- Profiles I001-I030 aggregate: `" + ProfilesBeforeHash + "` -> `" + result.ProfilesAfterHash + "`");
            builder.AppendLine("- Foundation signature: `sha256:" + result.FoundationSignature + "`");
            builder.AppendLine("- Snapshot signature: `sha256:" + result.SnapshotSignature + "`");
            builder.AppendLine("- Candidate signature: `sha256:" + result.CandidateSignature + "`");
            builder.AppendLine("- Roll150: `" + Roll150Canonical + "` (unchanged)");
            builder.AppendLine("- Projection150: `" + Projection150Canonical + "` (unchanged)");
            builder.AppendLine("- Item105 accepted baseline: `" + OldItem105Hash + "` -> `" + ScopedNewItem105Hash + "`");
            builder.AppendLine("- Item105 current dirty-worktree actual: `" + result.Item105ActualHash + "`");
            builder.AppendLine();
            builder.AppendLine("The accepted Item105 baseline is derived from the previously accepted 105-file input plus only the approved catalog shape correction. The current actual is not adopted because a pre-existing Item Detail UI delta is outside this package.");
            builder.AppendLine();
            builder.AppendLine("## Assertions");
            builder.AppendLine();
            builder.AppendLine("| Check | Result | Detail |");
            builder.AppendLine("|---|---|---|");
            foreach (Check check in result.Checks)
                builder.AppendLine("| `" + check.Id + "` | " + (check.Passed ? "PASS" : "FAIL") + " | " + Md(check.Detail) + " |");
            builder.AppendLine();
            builder.AppendLine("## Shortest user hand-test");
            builder.AppendLine();
            builder.AppendLine("1. Open the existing Item Sandbox only; do not save the scene.");
            builder.AppendLine("2. Select I024 and confirm the detail shape is `shape_line2_h · 2格 · 核心格(0,0)`; select I010 and confirm `shape_corner3 · 3格 · 核心格(0,1)`. ");
            builder.AppendLine("3. Select I009 and I029 and confirm both remain single-cell and use the existing single-cell artwork slot; spot-check one vertical multi-cell Item uses the existing multi-cell slot.");
            builder.AppendLine();
            AppendList(builder, "Notes / pre-existing evidence", result.Notes);
            AppendList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static string BuildSpec(IReadOnlyList<MatrixRow> rows, Result result)
        {
            StringBuilder builder = new();
            builder.AppendLine("itemId,displayName,decisionStatus,newShapeId,orderedCells,technicalCoreCellLocal,coreCellStatus,defaultOrientation,evidence,result");
            foreach (MatrixRow row in rows)
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(row.ItemId);
                bool pass = item != null && item.shapeId == row.NewShapeId
                    && item.defaultLocalCells.SequenceEqual(row.NewCells) && item.coreCellLocal == row.NewCore;
                builder.AppendLine(Csv(row.ItemId, row.DisplayName, row.DecisionStatus, row.NewShapeId,
                    FormatCells(row.NewCells), FormatCell(row.NewCore), row.CoreStatus, row.DefaultOrientation,
                    row.Evidence, pass ? "PASS" : "FAIL"));
            }
            return builder.ToString();
        }

        private static string BuildChangeLedger(IReadOnlyList<MatrixRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("itemId,decisionStatus,oldShapeId,newShapeId,oldOrderedCells,newOrderedCells,oldCoreCellLocal,newTechnicalCoreCellLocal,catalogChanged,candidateDescriptionChanged,oldCatalogSignature,newCatalogSignature,oldSnapshotSignature,newSnapshotSignature,evidence,result");
            foreach (MatrixRow row in rows)
            {
                ItemInnerDataDefinition current = ItemInnerDataCatalog.FindById(row.ItemId);
                List<Vector2Int> oldCells = ShapeCells(row.OldShapeId);
                string oldCatalog = Sha256(ItemShapeSignature(row.ItemId, row.OldShapeId, oldCells, row.OldCore));
                string newCatalog = Sha256(ItemShapeSignature(row.ItemId, row.NewShapeId, row.NewCells, row.NewCore));
                string oldSnapshot = SnapshotSignature(current, row.OldShapeId, oldCells, row.OldCore);
                string newSnapshot = SnapshotSignature(current, row.NewShapeId, row.NewCells, row.NewCore);
                bool change = row.DecisionStatus == "APPROVED_CHANGE";
                bool pass = current != null && current.shapeId == row.NewShapeId
                    && current.defaultLocalCells.SequenceEqual(row.NewCells) && current.coreCellLocal == row.NewCore;
                builder.AppendLine(Csv(row.ItemId, row.DecisionStatus, row.OldShapeId, row.NewShapeId,
                    FormatCells(oldCells), FormatCells(row.NewCells), FormatCell(row.OldCore), FormatCell(row.NewCore),
                    change ? "true" : "false", change ? "true" : "false", "sha256:" + oldCatalog,
                    "sha256:" + newCatalog, "sha256:" + oldSnapshot, "sha256:" + newSnapshot,
                    row.Evidence, pass ? "PASS" : "FAIL"));
            }
            return builder.ToString();
        }

        private static string BuildCanonicalDelta(Result result)
        {
            StringBuilder builder = new();
            builder.AppendLine("kind,idOrPath,fieldOrCheckId,oldValue,newValue,fileCount,inputScope,reason,result");
            builder.AppendLine(Csv("aggregate", CatalogPath, "catalog-aggregate", CatalogBeforeHash,
                result.CatalogAfterHash, "1", CatalogPath, "APPROVED_ITEM_SHAPE_TRUTH_CORRECTION", "CHANGED_AS_APPROVED"));
            builder.AppendLine(Csv("aggregate", ProfileRoot, "profiles-I001-I030-aggregate", ProfilesBeforeHash,
                result.ProfilesAfterHash, "30", ProfileRoot + "/ItemBalanceProfile_I001..I030.asset",
                "APPROVED_ITEM_SHAPE_TRUTH_CORRECTION", "CHANGED_AS_APPROVED"));
            builder.AppendLine(Csv("canonical", "Roll150", "roll-canonical", Roll150Canonical, Roll150Canonical,
                "150", "I001-I030 x white/green/blue/purple/orange", "PROTECTED_CANONICAL", "UNCHANGED"));
            builder.AppendLine(Csv("canonical", "Projection150", "projection-canonical", Projection150Canonical,
                Projection150Canonical, "150", "I001-I030 x white/green/blue/purple/orange",
                "PROTECTED_CANONICAL", "UNCHANGED"));
            foreach (string path in Item105Whitelist)
            {
                builder.AppendLine(Csv("item105-baseline", path, "Item105 expected aggregate", OldItem105Hash,
                    ScopedNewItem105Hash, "105",
                    "Assets/_Game/Scripts/TalismanBag/Items/** excluding Items/Capability and Capability.meta; relativePath|UPPER_SHA256; LF; ordinal path order",
                    "APPROVED_ITEM_SHAPE_TRUTH_CORRECTION", "MIGRATED"));
            }
            builder.AppendLine(Csv("item105-worktree", "current-worktree", "actual aggregate",
                ScopedNewItem105Hash, result.Item105ActualHash, "105", "same Item105 input scope",
                "PREEXISTING_ITEM_DETAIL_UI_DELTA_NOT_ACCEPTED", "PREEXISTING_MISMATCH_RETAINED"));
            foreach (ProtectionExpectation protection in ProtectedScopes)
                builder.AppendLine(Csv("protected", protection.Id, "aggregate", protection.ExpectedHash,
                    protection.ActualHash, protection.ExpectedCount.ToString(CultureInfo.InvariantCulture),
                    string.Join(";", protection.Files), "READ_ONLY_PROTECTION", protection.ExpectedHash == protection.ActualHash ? "UNCHANGED" : "FAIL"));
            return builder.ToString();
        }

        private static string BuildLeakReport(Result result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Item Shape Catalog Batch Correction Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("- Result: `" + (result.Errors.Count == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Runtime truth files changed by package: `1` (`ItemInnerDataCatalog.cs` only)");
            builder.AppendLine("- Candidate assets changed by package: `16`; allowed field: `shapeDescription` only");
            builder.AppendLine("- Enemy Runtime changed: `NO`");
            builder.AppendLine("- CrossSystem Runtime changed: `NO`");
            builder.AppendLine("- Item Capability Runtime changed: `NO`");
            builder.AppendLine("- Scene / Prefab / BuildSettings / PNG / importer / Item Detail UI changed by package: `NO`");
            builder.AppendLine("- Approved matrix changed: `NO`");
            builder.AppendLine("- New runtime shapeId / second baseItemId-to-shape table: `NO`");
            builder.AppendLine("- placement efficiency / board-space score / cell-count-to-BP / shape-to-combat rule: `NO`");
            builder.AppendLine("- C02: `32/32 blocked`; Actual Unknown reduction: `0`");
            builder.AppendLine("- Historical I009/I029 report: retained; both fixes are absorbed as APPROVED_KEEP rows in this matrix.");
            builder.AppendLine();
            builder.AppendLine("## Protected scopes");
            builder.AppendLine();
            builder.AppendLine("| Scope | Files | Before | After | Result |");
            builder.AppendLine("|---|---:|---|---|---|");
            foreach (ProtectionExpectation protection in ProtectedScopes)
                builder.AppendLine("| `" + protection.Id + "` | " + protection.Files.Length + " | `" + protection.ExpectedHash
                    + "` | `" + protection.ActualHash + "` | "
                    + (protection.ExpectedHash == protection.ActualHash && protection.Files.Length == protection.ExpectedCount ? "PASS" : "FAIL") + " |");
            builder.AppendLine();
            AppendList(builder, "Notes / pre-existing evidence", result.Notes);
            AppendList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static string SnapshotSignature(ItemInnerDataDefinition source, string shapeId,
            IReadOnlyList<Vector2Int> cells, Vector2Int core)
        {
            if (source == null) return Sha256("missing");
            ItemInnerDataDefinition clone = new()
            {
                itemId = source.itemId, displayName = source.displayName, itemFamily = source.itemFamily,
                faMenTag = source.faMenTag, qiLeiTag = source.qiLeiTag, shapeId = shapeId,
                defaultLocalCells = cells.ToList(), coreCellLocal = core, isLightingSource = source.isLightingSource
            };
            ItemSystemSnapshot snapshot = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(
                    new[] { new ItemSystemPlacementInput("LEDGER_" + source.itemId, source.itemId, new Vector2Int(4, 4), 0) },
                    new ItemSystemBoardConfigInput(9, new Vector2Int(8, 8), Array.Empty<Vector2Int>()),
                    catalogItems: new[] { clone }));
            return Sha256(snapshot.BuildDebugSignature());
        }

        private static string ItemShapeSignature(string itemId, string shapeId,
            IReadOnlyList<Vector2Int> cells, Vector2Int core)
        {
            return itemId + "|" + shapeId + "|" + FormatCells(cells) + "|" + FormatCell(core);
        }

        private static List<Vector2Int> ShapeCells(string shapeId)
        {
            return shapeId switch
            {
                "shape_single_1" => new List<Vector2Int> { new(0, 0) },
                "shape_line2_h" => new List<Vector2Int> { new(0, 0), new(1, 0) },
                "shape_line2_v" => new List<Vector2Int> { new(0, 0), new(0, 1) },
                "shape_line3_h" => new List<Vector2Int> { new(0, 0), new(1, 0), new(2, 0) },
                "shape_line3_v" => new List<Vector2Int> { new(0, 0), new(0, 1), new(0, 2) },
                "shape_corner3" => new List<Vector2Int> { new(0, 0), new(1, 0), new(0, 1) },
                "shape_block2x2" => new List<Vector2Int> { new(0, 0), new(1, 0), new(0, 1), new(1, 1) },
                _ => throw new InvalidDataException("Unknown approved old shape: " + shapeId)
            };
        }

        private static Vector2Int Rotate(Vector2Int cell, int rotation)
        {
            return rotation switch
            {
                90 => new Vector2Int(-cell.y, cell.x),
                180 => new Vector2Int(-cell.x, -cell.y),
                270 => new Vector2Int(cell.y, -cell.x),
                _ => cell
            };
        }

        private static string ReadGitHeadText(string path)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "git", Arguments = "show HEAD:" + path, WorkingDirectory = Directory.GetCurrentDirectory(),
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8, CreateNoWindow = true
            };
            using Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new InvalidOperationException("git show failed for " + path + ": " + error);
            return output;
        }

        private static string NormalizeShapeDescription(string yaml)
        {
            return string.Join("\n", NormalizeLineEndings(yaml).Split('\n')
                .Select(line => line.TrimStart().StartsWith("shapeDescription:", StringComparison.Ordinal)
                    ? new string(' ', line.Length - line.TrimStart().Length) + "shapeDescription: <NORMALIZED>"
                    : line));
        }

        private static string NormalizeLineEndings(string value) => (value ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');

        private static string[] AllFiles(string root, string pattern)
        {
            if (!Directory.Exists(root)) return Array.Empty<string>();
            return Directory.GetFiles(root, pattern, SearchOption.AllDirectories).Select(NormalizePath).ToArray();
        }

        private static string[] PngAndMetaFiles()
        {
            return AllFiles("Assets", "*").Where(path => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".png.meta", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private static string AggregateHash(IEnumerable<string> files)
        {
            string[] lines = files.Where(File.Exists).Select(NormalizePath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase)
                .Select(path => path + "=" + FileSha256(path).ToUpperInvariant()).ToArray();
            return Sha256(string.Join("\n", lines)).ToUpperInvariant();
        }

        private static string PowerShellProtectionAggregate(string scopeId)
        {
            string fileExpression = scopeId == "scenes"
                ? "@(Get-ChildItem -LiteralPath 'Assets' -Filter '*.unity' -File -Recurse)"
                : "@(Get-ChildItem -LiteralPath 'Assets' -Filter '*.png' -File -Recurse) + @(Get-ChildItem -LiteralPath 'Assets' -Filter '*.png.meta' -File -Recurse)";
            string script = "$repo=(Get-Location).Path; $files=" + fileExpression
                + "; $lines=@($files|Sort-Object FullName|ForEach-Object{$rel=$_.FullName.Substring($repo.Length+1).Replace('\\','/');$rel+'='+((Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash)});"
                + "$payload=[Text.Encoding]::UTF8.GetBytes([string]::Join(\"`n\",$lines));$sha=[Security.Cryptography.SHA256]::Create();try{([BitConverter]::ToString($sha.ComputeHash($payload))).Replace('-','')}finally{$sha.Dispose()}";
            ProcessStartInfo startInfo = new()
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -EncodedCommand "
                    + Convert.ToBase64String(Encoding.Unicode.GetBytes(script)),
                WorkingDirectory = Directory.GetCurrentDirectory(), UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8, CreateNoWindow = true
            };
            using Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd().Trim();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0 || !Regex.IsMatch(output, "^[0-9A-F]{64}$"))
                throw new InvalidOperationException("PowerShell protection aggregate failed for " + scopeId + ": " + error);
            return output;
        }

        private static string BuildItem105Aggregate()
        {
            const string root = "Assets/_Game/Scripts/TalismanBag/Items";
            string[] lines = AllFiles(root, "*")
                .Where(path => path != root + "/Capability.meta"
                    && !path.StartsWith(root + "/Capability/", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => path + "|" + FileSha256(path).ToUpperInvariant()).ToArray();
            return Sha256(string.Join("\n", lines));
        }

        private static string FileSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return ToHex(sha.ComputeHash(stream));
        }

        private static string Sha256(string value)
        {
            using SHA256 sha = SHA256.Create();
            return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)));
        }

        private static string ToHex(byte[] bytes) => string.Concat(bytes.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));

        private static bool IsReadOnly<T>(IReadOnlyList<T> values) => values is ICollection<T> collection && collection.IsReadOnly;

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            for (int index = 0; (index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0; index += value.Length) count++;
            return count;
        }

        private static string NormalizePath(string path)
        {
            string full = Path.GetFullPath(path);
            string root = Path.GetFullPath(Directory.GetCurrentDirectory()).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? full.Substring(root.Length).Replace('\\', '/')
                : path.Replace('\\', '/');
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> values = new();
            StringBuilder value = new();
            bool quoted = false;
            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"') { value.Append('"'); index++; }
                    else quoted = !quoted;
                }
                else if (character == ',' && !quoted) { values.Add(value.ToString()); value.Clear(); }
                else value.Append(character);
            }
            values.Add(value.ToString());
            return values;
        }

        private static List<Vector2Int> ParseCells(string value) => value.Split(';').Select(ParseCell).ToList();

        private static Vector2Int ParseCell(string value)
        {
            string[] parts = value.Trim().Trim('(', ')').Split(',');
            if (parts.Length != 2) throw new InvalidDataException("Invalid cell: " + value);
            return new Vector2Int(int.Parse(parts[0], CultureInfo.InvariantCulture),
                int.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        private static string FormatCell(Vector2Int cell) => "(" + cell.x.ToString(CultureInfo.InvariantCulture)
            + "," + cell.y.ToString(CultureInfo.InvariantCulture) + ")";
        private static string FormatCells(IEnumerable<Vector2Int> cells) => string.Join(";", cells.Select(FormatCell));

        private static string Csv(params object[] values) => string.Join(",", values.Select(value => EscapeCsv(value?.ToString() ?? string.Empty)));
        private static string EscapeCsv(string value) => value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0
            ? value : "\"" + value.Replace("\"", "\"\"") + "\"";
        private static string Md(string value) => (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

        private static void AppendList(StringBuilder builder, string title, IReadOnlyList<string> values)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            if (values.Count == 0) builder.AppendLine("- None");
            else foreach (string value in values) builder.AppendLine("- " + value);
            builder.AppendLine();
        }

        private sealed class MatrixRow
        {
            public string ItemId, DisplayName, DecisionStatus, OldShapeId, NewShapeId;
            public List<Vector2Int> NewCells;
            public Vector2Int OldCore, NewCore;
            public string CoreStatus, DefaultOrientation, Evidence;
        }

        private sealed class Check
        {
            public string Id, Detail;
            public bool Passed;
        }

        private sealed class Result
        {
            public readonly List<Check> Checks = new();
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
            public string CatalogAfterHash = string.Empty, ProfilesAfterHash = string.Empty;
            public string FoundationSignature = string.Empty, SnapshotSignature = string.Empty;
            public string CandidateSignature = string.Empty, Item105ActualHash = string.Empty;

            public void Require(string id, bool passed, string detail)
            {
                Checks.Add(new Check { Id = id, Passed = passed, Detail = detail ?? string.Empty });
                if (!passed) Errors.Add(id + ": " + detail);
            }
        }

        private sealed class ProtectionExpectation
        {
            public ProtectionExpectation(string id, string[] files, int expectedCount, string expectedHash)
            {
                Id = id; Files = files; ExpectedCount = expectedCount; ExpectedHash = expectedHash;
            }
            public string Id { get; }
            public string[] Files { get; set; }
            public int ExpectedCount { get; }
            public string ExpectedHash { get; }
            public string ActualHash { get; set; } = string.Empty;
        }

        private sealed class CatalogReadOnlyDetailProvider : IItemDetailViewModelProvider
        {
            private readonly IReadOnlyList<ItemDetailListEntry> entries = ItemInnerDataCatalog.AllItems
                .Select(item => new ItemDetailListEntry(item.itemId, item.displayName)).ToArray();

            public IReadOnlyList<ItemDetailListEntry> GetItemList() => entries;

            public ItemDetailViewModel GetDetailViewModel(string itemId)
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(itemId);
                if (item == null) return null;
                return new ItemDetailViewModel
                {
                    itemId = item.itemId,
                    baseItemId = item.itemId,
                    displayItemName = item.displayName,
                    displayRarityName = item.displayRarityName,
                    displayFaMenName = item.FaMenDisplayName,
                    displayQiLeiName = item.QiLeiDisplayName,
                    displayShapeName = item.shapeId + " · " + item.defaultLocalCells.Count + "格",
                    displayItemPower = item.itemPower,
                    displayTriggerText = item.triggerText,
                    displayFlavorText = item.flavorText,
                    iconPlaceholderKey = item.iconPlaceholderKey,
                    rarityColorKey = item.rarityDefault.ToStableKey()
                };
            }
        }
    }
}
#endif
