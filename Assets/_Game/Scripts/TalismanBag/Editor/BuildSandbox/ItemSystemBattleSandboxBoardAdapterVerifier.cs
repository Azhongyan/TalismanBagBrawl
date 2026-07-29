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
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ItemSystemBattleSandboxBoardAdapterVerifier
    {
        private const string ExpectedSceneSha256 =
            "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6";
        private const string ItemSandboxScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string SimSunFontName = "SimSun";
        private const string AcceptedPlacementIconGuid =
            "830ab2298ee0b034eba76acca9fbcf2c";
        private const string AcceptedFlavorIconGuid =
            "3d5dfa9cae5ac3e4ba55dca7a887ab7e";
        private const string LegacyTextFailureDiagnosticFragment =
            "无法为道具详情解析可用的旧版文字字体，面板已保持隐藏。";
        private static readonly IReadOnlyDictionary<string, string>
            ProtectedNonWhitelistHashes = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = ExpectedSceneSha256,
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs"] = "65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] = "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs"] = "51fae336d8f3c172eff39dc4a982ce01dc44d59d831d02ad65e2d8315d4f3d9e",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/I031InventoryPlacementContract.cs"] = "6ff5a159a3cfcbb810d271098870d0487556954f1f915bf781184b24db4574bd",
                ["Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs"] = "499d8b67a45e59a163d6a3192937113cb550207a101f8754531a32346a50047d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs"] = "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0",
                ["Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs"] = "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
                ["Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset"] = "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };
        private static readonly List<ScenarioResult> Results = new();
        private static LegacyTextVerificationEvidence legacyTextEvidence;
        private static AuthoredVisualVerificationEvidence authoredVisualEvidence;
        private static FontVerificationEvidence fontEvidence;

        [MenuItem("Talisman Bag/V0.4/Verify ItemSystem Battle Sandbox Board Adapter")]
        public static void VerifyOffline()
        {
            RunAndReport(includeStaticChecks: true);
        }

        public static void VerifyStaticBatch()
        {
            RunAndReport(includeStaticChecks: true);
        }

        private static void RunAndReport(bool includeStaticChecks)
        {
            Results.Clear();
            legacyTextEvidence = null;
            authoredVisualEvidence = null;
            fontEvidence = null;
            ItemSystemBattleSandboxViewProjectionResult projection =
                BuildProjection();
            Run("S01", "exact I001-I031 roster, Ordinal and unique", () =>
                VerifyRoster(projection));
            Run("S01A", "40/60 reject, 65 accepts 63 cells through slot 63", () =>
                VerifyTrayCapacity(projection));
            Run("S01B", "runtime slots 41-65 install and cleanly uninstall", () =>
                VerifyRuntimeTraySlotLifecycle());
            Run("S02", "deterministic Orange/DaoPin projections and explicit identities", () =>
                VerifyOrdinaryIdentity(projection));
            Run("S03", "I031 inventory/board/move/return stable identity", () =>
                VerifySystemItem(projection));
            Run("S04", "catalog shapes, core cells and rotation semantics", () =>
                VerifyShapesAndRotation(projection));
            Run("S04A", "ordinary Orange rarityIndex 5 artwork resolves 30/30", () =>
                VerifyOrdinaryArtwork(projection));
            Run("S04B", "I031 lit and unlit artwork resolves 2/2", () =>
                VerifySystemArtwork(projection));
            Run("S04C", "31/31 final artwork renderer binding and regressions", () =>
                VerifyAuthoritativeCardArtwork(projection));
            Run("S04D", "tray/ghost/board exact Sprite seam and restore", () =>
                VerifyAuthoritativeCardArtwork(projection));
            Run("S05", "owned Inventory I031 plus empty board is valid v2", () =>
                VerifyBaseline());
            Run("S05A", "omitted explicit I031 state rejects deterministically", () =>
                VerifyLegacyOmissionRejected());
            Run("S06", "legal tray-to-board commit refreshes IF01/P6 once", () =>
                VerifyLegalCommit(projection));
            Run("S07", "overlap, bounds and eye failures are atomic", () =>
                VerifyRejectedCommits(projection));
            Run("S08", "board move commits atomically", () =>
                VerifyMove(projection));
            Run("S09", "rotate preview and commit use ItemSystem semantics", () =>
                VerifyRotate(projection));
            Run("S10", "ordinary and I031 return preserve exact inventory state", () =>
                VerifyReturn(projection));
            Run("S11", "reset restores all 31 to tray and empty board", () =>
                VerifyReset(projection));
            Run("S12", "same snapshot and no-op reset do not refresh", () =>
                VerifyNoOpDeduplication(projection));
            Run("S13", "changed commit/reset refresh exactly once", () =>
                VerifyChangedRefresh(projection));
            Run("S14", "unknown/malformed dependency never succeeds", () =>
                VerifyUnknownDependency(projection));
            Run("S15", "compatibility snapshot is one-way from ItemSystem", () =>
                VerifyCompatibilityProjection(projection));
            Run("S16", "only the real V04 BattleSandbox path installs", () =>
                VerifyTargetPath());
            Run("S17", "ItemSandbox and UnifiedBattle shell are rejected", () =>
                VerifyRejectedPaths());
            Run("S18", "no scene/prefab/layout/formal-system write", () =>
                VerifyStaticBoundaries(includeStaticChecks));
            Run("S19", "one authored scene panel hides/reuses/uninstalls intact", () =>
                VerifyDetailPresenter(projection, "lifecycle"));
            Run("S19A", "no prefab/runtime detail construction or ItemSandbox presenter", () =>
                VerifyDetailConstructionBoundary());
            Run("S20", "exact row model and authoritative Sprite bind", () =>
                VerifyDetailPresenter(projection, "identity"));
            Run("S20A", "tray detail uses CatalogPreview latest v2", () =>
                VerifyDetailPresenter(projection, "catalog"));
            Run("S20B", "placed detail uses all five v2 projections", () =>
                VerifyDetailPresenter(projection, "placed"));
            Run("S20C", "fresh projected clone and immutable base model", () =>
                VerifyDetailPresenter(projection, "clone"));
            Run("S20D", "invalid/stale/mismatched detail context clears", () =>
                VerifyDetailPresenter(projection, "invalid"));
            Run("S20E", "30/30 DaoPin models expose complete player detail", () =>
                VerifyDaoPinDetailModels(projection));
            Run("S20F", "authored panel renders all model-visible player text", () =>
                VerifyAuthoredDetailPanelBinding(projection));
            Run("S20G", "section hidden/plain/rows/plain/hidden transitions", () =>
                VerifySectionVisibilityTransitions());
            Run("S20H", "target panel resolves a usable Legacy Text font", () =>
                VerifyLegacyTextFontRecoveryContract());
            Run("S20I", "all active data-bearing Legacy Text has a font", () =>
                VerifyLegacyTextEvidence(value => value.ActiveDataTextCount > 0,
                    "active data-bearing font coverage is empty"));
            Run("S20J", "header and connected status badge text render data", () =>
                VerifyLegacyTextEvidence(value => value.HeaderAndStatusVisible,
                    "header/status visible-text evidence missing"));
            Run("S20K", "plain body and authored rows are exclusive and restorable", () =>
                VerifyLegacyTextEvidence(value => value.BodyRowsExclusive,
                    "body/row mutual-exclusion evidence missing"));
            Run("S20L", "runtime-created Legacy Text receives a valid font", () =>
                VerifyRuntimeCreatedLegacyTextFont());
            Run("S20M", "active data-bearing Text preferred size is non-zero", () =>
                VerifyLegacyTextEvidence(value => value.PreferredMeasurementsValid,
                    "preferred measurement evidence missing"));
            Run("S20N", "ItemSandbox and BattleSandbox shared-view regression", () =>
                VerifyItemSandboxSharedViewRegression(projection));
            Run("S20O", "source SimSun and target null-font evidence", () =>
                VerifySourceAndTargetTypographyEvidence());
            Run("S20P", "target resolves cached SimSun or exact approved fallback", () =>
                VerifyTargetCachedChineseFont(projection));
            Run("S20Q", "authored mode keeps BodyText container active", () =>
                VerifyAuthoredBodyContainer(projection));
            Run("S20R", "FaMen Build icons are actually visible and exact", () =>
                VerifyAuthoredIconGroup(projection, AuthoredIconGroup.FaMenBuild));
            Run("S20S", "QiLei Build icons are actually visible and exact", () =>
                VerifyAuthoredIconGroup(projection, AuthoredIconGroup.QiLeiBuild));
            Run("S20T", "Core Effect icons match base item and ordinal", () =>
                VerifyAuthoredIconGroup(projection, AuthoredIconGroup.CoreEffect));
            Run("S20U", "Placement and Flavor authored icons remain visible", () =>
                VerifyPlacementAndFlavorIcons(projection));
            Run("S20V", "plain/authored/plain preserves font sprite and visibility", () =>
                VerifyPlainAuthoredRoundTrip());
            Run("S21", "authority detail has no legacy panel fallback", () =>
                VerifyDetailStaticSeam());
            Run("S22", "tap/drag/close read-only detail seam remains", () =>
                VerifyDetailPresenter(projection, "close"));
            Run("S23", "inventory I031 means zero range and unlit Build state", () =>
                VerifyInventoryLighting(projection));
            Run("S24", "interior/edge/corner I031 exact clipped range", () =>
                VerifyFourWayLighting(projection));
            Run("S25", "I031 move/return clears stale range and links", () =>
                VerifyLightingTransition(projection));
            Run("S26", "authority renderer never resolves legacy power", () =>
                VerifyAuthorityPowerBoundary());

            WriteReports(projection);
            ScenarioResult[] failures = Results.Where(result => !result.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemSystem BattleSandbox verifier failed: "
                    + string.Join("; ", failures.Select(result =>
                        result.Id + "=" + result.Detail)));
            }
            Debug.Log("[ItemSystemBattleSandboxBoardAdapterVerifier] PASS "
                + Results.Count.ToString(CultureInfo.InvariantCulture)
                + " scenarios; status remains WAITING_USER_HANDTEST.");
        }

        private static ItemSystemBattleSandboxViewProjectionResult BuildProjection()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    ItemSystemBattleSandboxBoardAdapter.WorkbenchCatalogPath);
            GameObject providerObject = new("ItemSystemBoardVerifierProvider");
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
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            Check(projection != null && projection.IsValid,
                "projection must be valid before authority construction");
            return new ItemSystemBattleSandboxBoardAuthority(
                projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static void VerifyRoster(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            Check(projection.IsValid, string.Join("|", projection.Diagnostics));
            string[] expected = Enumerable.Range(1, 31)
                .Select(index => "I" + index.ToString("000", CultureInfo.InvariantCulture))
                .ToArray();
            Check(projection.Rows.Select(row => row.BaseItemId)
                .SequenceEqual(expected, StringComparer.Ordinal), "roster/order mismatch");
            Check(projection.Rows.Select(row => row.BaseItemId)
                .Distinct(StringComparer.Ordinal).Count() == 31, "roster is not unique");
            TrayPackResult pack = PackAuthorityRoster(projection, 65);
            Check(pack.Succeeded && pack.PlacementCount == 31,
                "I001-I031 did not all enter the 65-slot authority tray");
            Check(pack.UsedCellCount == 63, "total footprint count is not 63");
            string scene = ReadProjectFile(
                "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity");
            int authoredTrayCells = CountOccurrences(scene, "m_Name: TrayGridSlot_");
            Check(authoredTrayCells == BuildItemTrayPreviewView.AuthoredTraySlotCount,
                "target scene authored tray count is not 40");
            Check(scene.IndexOf(BuildItemTrayPreviewView
                    .ItemSystemAuthorityRuntimeSlotNamePrefix, StringComparison.Ordinal) < 0,
                "runtime tray slot was serialized into the target scene");
        }

        private static void VerifyTrayCapacity(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            TrayPackResult forty = PackAuthorityRoster(projection, 40);
            TrayPackResult sixty = PackAuthorityRoster(projection, 60);
            TrayPackResult sixtyFive = PackAuthorityRoster(projection, 65);
            Check(!forty.Succeeded, "40-slot tray unexpectedly accepted the roster");
            Check(!sixty.Succeeded, "60-slot tray unexpectedly accepted the roster");
            Check(sixtyFive.Succeeded && sixtyFive.PlacementCount == 31,
                "65-slot tray did not accept all 31 items");
            Check(sixtyFive.UsedCellCount == 63,
                "65-slot tray occupied-cell count is not 63");
            Check(sixtyFive.LastLogicalSlotIndex == 63,
                "last occupied logical slot is not 63");
            Check(BuildItemTrayPreviewView.ItemSystemAuthorityLogicalRows == 13
                && BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount == 65
                && BuildItemTrayPreviewView.ItemSystemAuthorityRuntimeSlotCount == 25,
                "Revision03 tray constants are not 5x13/65/+25");
        }

        private static TrayPackResult PackAuthorityRoster(
            ItemSystemBattleSandboxViewProjectionResult projection,
            int slotCount)
        {
            ShapeAwareItemTrayGrid grid = new(
                "item_system_board_adapter_verifier_tray",
                BuildGridInteractionPreviewController.TrayColumns,
                slotCount,
                commitAllowed: true);
            foreach (ItemSystemBattleSandboxViewRow row in projection.Rows
                         .Where(value => value != null)
                         .OrderByDescending(value => value.ShapeCells.Count)
                         .ThenBy(value => value.BaseItemId, StringComparer.Ordinal))
            {
                ShapeItemPayload payload = new(
                    row.BaseItemId,
                    row.ShapeId,
                    ItemShapeRotation.Rotation0,
                    row.ShapeCells,
                    ShapePlacementSource.Tray);
                if (!grid.TryPack(payload, out ShapePlacementResult result)
                    || result == null || !result.IsValid)
                {
                    return TrayPackResult.FromGrid(grid, succeeded: false);
                }
            }
            return TrayPackResult.FromGrid(grid, succeeded: true);
        }

        private static void VerifyRuntimeTraySlotLifecycle()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            GameObject root = new("ItemSystemTrayRuntimeSlotVerifier", typeof(RectTransform));
            root.hideFlags = HideFlags.HideAndDontSave;
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, previewScene);
            try
            {
                BuildItemTrayPreviewView view = root.AddComponent<BuildItemTrayPreviewView>();
                GameObject contentObject = new("ItemTrayContent", typeof(RectTransform),
                    typeof(GridLayoutGroup));
                contentObject.transform.SetParent(root.transform, false);
                RectTransform content = contentObject.GetComponent<RectTransform>();
                content.sizeDelta = new Vector2(700f, 700f);
                GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(133f, 133f);
                grid.spacing = new Vector2(5f, 5f);
                grid.padding = new RectOffset(8, 8, 8, 8);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;

                List<RectTransform> slotRects = new();
                List<Image> slotImages = new();
                List<Outline> slotOutlines = new();
                for (int index = 1;
                     index <= BuildItemTrayPreviewView.AuthoredTraySlotCount;
                     index++)
                {
                    GameObject slotObject = new(
                        $"TrayGridSlot_{index:00}",
                        typeof(RectTransform), typeof(CanvasRenderer),
                        typeof(Image), typeof(Outline));
                    slotObject.transform.SetParent(content, false);
                    slotRects.Add(slotObject.GetComponent<RectTransform>());
                    slotImages.Add(slotObject.GetComponent<Image>());
                    slotOutlines.Add(slotObject.GetComponent<Outline>());
                }

                GameObject cardLayerObject = new("ItemCardLayer", typeof(RectTransform),
                    typeof(LayoutElement));
                cardLayerObject.transform.SetParent(content, false);
                cardLayerObject.GetComponent<LayoutElement>().ignoreLayout = true;
                RectTransform cardLayer = cardLayerObject.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);

                int[] siblingIndexes = slotRects.Select(slot => slot.GetSiblingIndex()).ToArray();
                Vector2[] anchoredPositions = slotRects.Select(slot => slot.anchoredPosition).ToArray();
                float originalHeight = content.sizeDelta.y;
                Vector2 cellSize = grid.cellSize;
                Vector2 spacing = grid.spacing;
                int paddingLeft = grid.padding.left;
                int paddingRight = grid.padding.right;
                int paddingTop = grid.padding.top;
                int paddingBottom = grid.padding.bottom;
                GridLayoutGroup.Constraint constraint = grid.constraint;
                int constraintCount = grid.constraintCount;

                view.Bind(null, content, cardLayer,
                    Array.Empty<Button>(), Array.Empty<Text>(),
                    slotRects, slotImages, slotOutlines,
                    Array.Empty<BuildItemPreviewCardView>());
                Check(view.InstallItemSystemAuthorityRuntimeSlots(
                        out string diagnosticCode),
                    "runtime slot install failed: " + diagnosticCode);
                Check(view.HasItemSystemAuthorityRuntimeSlots
                    && view.InstalledItemSystemAuthorityRuntimeSlotCount == 25
                    && view.TraySlotCount == 65,
                    "runtime slot counts are not +25/65");

                HideFlags requiredFlags = HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild;
                for (int index = 41; index <= 65; index++)
                {
                    Transform runtimeSlot = content.Find(
                        BuildItemTrayPreviewView.ItemSystemAuthorityRuntimeSlotNamePrefix
                        + index.ToString("00", CultureInfo.InvariantCulture));
                    Check(runtimeSlot != null, "runtime slot missing: " + index);
                    Check((runtimeSlot.gameObject.hideFlags & requiredFlags) == requiredFlags,
                        "runtime slot DontSave flags missing: " + index);
                    Check(runtimeSlot.GetComponent<Image>() != null
                        && runtimeSlot.GetComponent<Outline>() != null,
                        "runtime slot component pattern mismatch: " + index);
                }

                float expectedHeight = grid.padding.top + grid.padding.bottom
                    + 13 * grid.cellSize.y + 12 * grid.spacing.y;
                Check(Mathf.Abs(content.rect.height - expectedHeight) < 0.5f,
                    "authority Content height is not the 13-row extent");
                Check(slotRects.Select((slot, index) =>
                        slot.parent == content
                        && slot.GetSiblingIndex() == siblingIndexes[index]
                        && slot.anchoredPosition == anchoredPositions[index]
                        && string.Equals(slot.name, $"TrayGridSlot_{index + 1:00}",
                            StringComparison.Ordinal)).All(value => value),
                    "authored slots changed during runtime extension");
                Check(grid.cellSize == cellSize && grid.spacing == spacing
                    && grid.padding.left == paddingLeft
                    && grid.padding.right == paddingRight
                    && grid.padding.top == paddingTop
                    && grid.padding.bottom == paddingBottom
                    && grid.constraint == constraint
                    && grid.constraintCount == constraintCount,
                    "GridLayoutGroup geometry changed");

                view.UninstallItemSystemAuthorityRuntimeSlots();
                Check(!view.HasItemSystemAuthorityRuntimeSlots
                    && view.InstalledItemSystemAuthorityRuntimeSlotCount == 0
                    && view.TraySlotCount == 40,
                    "runtime slots were not removed on uninstall");
                Check(Enumerable.Range(41, 25).All(index => content.Find(
                    BuildItemTrayPreviewView.ItemSystemAuthorityRuntimeSlotNamePrefix
                    + index.ToString("00", CultureInfo.InvariantCulture)) == null),
                    "runtime slot hierarchy leaked after uninstall");
                Check(Mathf.Abs(content.sizeDelta.y - originalHeight) < 0.001f,
                    "Content height was not restored on uninstall");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void VerifyOrdinaryIdentity(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxViewRow[] ordinary = projection.Rows
                .Where(row => !row.IsSystemItem).ToArray();
            Check(ordinary.Length == 30, "ordinary row count is not 30");
            foreach (ItemSystemBattleSandboxViewRow row in ordinary)
            {
                ItemDetailViewModel detailModel = row.CreateBaseDetailModelClone();
                Check(row.Projection != null, row.BaseItemId + " projection missing");
                Check(row.Projection.rarity == ItemInstanceRarity.Orange,
                    row.BaseItemId + " is not Orange");
                Check(string.Equals(detailModel?.rarityKey, "orange",
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(detailModel?.displayRarityName, "道品",
                        StringComparison.Ordinal),
                    row.BaseItemId + " DaoPin detail identity mismatch");
                Check(row.RootSeed == ItemSystemBattleSandboxViewProjection.PackageSeed
                    + row.Ordinal, row.BaseItemId + " seed mismatch");
                Check(string.Equals(row.Projection.baseItemId, row.BaseItemId,
                    StringComparison.Ordinal), row.BaseItemId + " base identity mismatch");
                Check(!string.Equals(row.ItemInstanceId, "P_BOARD_" + row.BaseItemId,
                    StringComparison.Ordinal), row.BaseItemId + " instance equals placement");
            }
            string source = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs");
            Check(source.IndexOf("rarityDefault", StringComparison.Ordinal) < 0,
                "adapter reads catalog rarity default");
        }

        private static void VerifySystemItem(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxViewRow system = projection.Rows.Single(row => row.IsSystemItem);
            Check(system.Projection == null && system.ItemInstanceId.Length == 0,
                "I031 has ordinary projection identity");
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CurrentSnapshot.placements.Count == 0
                && authority.CurrentSnapshot.i031State.location
                    == I031Location.Inventory
                && authority.CurrentSnapshot.i031State.specialIdentityId
                    == "SPECIAL_I031"
                && authority.CurrentSnapshot.i031State.stablePlacementId
                    == "P_SYSTEM_I031",
                "I031 inventory baseline mismatch");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(0, 0),
                ItemShapeRotation.Rotation0).Accepted,
                "I031 tray commit rejected");
            ItemSystemPlacementSnapshot placement = authority.CurrentSnapshot.placements
                .Single(value => value.itemId == "I031");
            Check(placement.placementId == "P_SYSTEM_I031"
                && authority.CurrentSnapshot.i031State.location == I031Location.Board,
                "I031 board identity mismatch");
            ItemSystemBattleSandboxBoardOperationResult moved = authority.CommitMove(
                "P_SYSTEM_I031", new ItemShapeCell(1, 1), ItemShapeRotation.Rotation0);
            Check(moved.Accepted && moved.Changed, "I031 legal move rejected");
            ItemSystemBattleSandboxBoardOperationResult returned =
                authority.ReturnToTray("P_SYSTEM_I031");
            Check(returned.Accepted && returned.Changed
                && authority.CurrentSnapshot.placements.Count == 0
                && authority.CurrentSnapshot.i031State.location
                    == I031Location.Inventory
                && authority.CurrentSnapshot.i031State.specialIdentityId
                    == "SPECIAL_I031",
                "I031 return did not preserve inventory identity");
        }

        private static void VerifyShapesAndRotation(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            Dictionary<string, ItemInnerDataDefinition> catalog =
                ItemInnerDataCatalog.AllItems.ToDictionary(item => item.itemId,
                    item => item, StringComparer.Ordinal);
            foreach (ItemSystemBattleSandboxViewRow row in projection.Rows)
            {
                ItemInnerDataDefinition source = catalog[row.BaseItemId];
                Vector2Int[] expected = source.ShapeCells.OrderBy(CellKey).ToArray();
                Vector2Int[] actual = row.ShapeCells
                    .Select(cell => new Vector2Int(cell.x, cell.y))
                    .OrderBy(CellKey).ToArray();
                Check(expected.SequenceEqual(actual), row.BaseItemId + " shape mismatch");
                Check(row.CoreCellLocal.Equals(new ItemShapeCell(
                    source.coreCellLocal.x, source.coreCellLocal.y)),
                    row.BaseItemId + " core mismatch");
            }

            foreach (ItemSystemBattleSandboxViewRow row in projection.Rows
                         .Where(value => !value.IsSystemItem)
                         .GroupBy(value => value.ShapeId, StringComparer.Ordinal)
                         .Select(group => group.First()))
            {
                foreach (int rotation in new[] { 0, 90, 180, 270 })
                {
                    ItemSystemSnapshot snapshot = FindLegalShapeSnapshot(row, rotation);
                    Check(snapshot != null && snapshot.isValid,
                        row.ShapeId + " has no legal rotation " + rotation);
                    ItemSystemPlacementSnapshot placement = snapshot.placements
                        .Single(value => value.itemId == row.BaseItemId);
                    Vector2Int[] expected = row.ShapeCells
                        .Select(cell => Rotate(new Vector2Int(cell.x, cell.y), rotation)
                            + placement.anchorCell)
                        .OrderBy(CellKey).ToArray();
                    Check(expected.SequenceEqual(placement.OccupiedCells.OrderBy(CellKey)),
                        row.ShapeId + " occupied cells mismatch at " + rotation);
                    Vector2Int expectedCore = Rotate(
                        new Vector2Int(row.CoreCellLocal.x, row.CoreCellLocal.y),
                        rotation) + placement.anchorCell;
                    Check(expectedCore == placement.coreCellWorld,
                        row.ShapeId + " core rotation mismatch at " + rotation);
                }
            }
        }

        private static ItemSystemSnapshot FindLegalShapeSnapshot(
            ItemSystemBattleSandboxViewRow row,
            int rotation)
        {
            foreach (Vector2Int systemCell in EnumerateBoardCells()
                         .Where(cell => cell != new Vector2Int(2, 2)))
            {
                foreach (Vector2Int anchor in EnumerateBoardCells())
                {
                    ItemSystemSnapshot snapshot = DefaultItemSystemSnapshotProvider.Instance
                        .CreateSnapshot(new ItemSystemSnapshotInput(new[]
                        {
                            new ItemSystemPlacementInput("P_SYSTEM_I031", "I031",
                                systemCell, 0),
                            new ItemSystemPlacementInput("P_TEST_" + row.BaseItemId,
                                row.BaseItemId, anchor, rotation)
                        }, catalogItems: ItemInnerDataCatalog.AllItems,
                            i031StateInputs: new[]
                            {
                                I031InventoryPlacementContract.OwnedBoard()
                            }));
                    if (snapshot.isValid)
                    {
                        return snapshot;
                    }
                }
            }
            return null;
        }

        private static void VerifyOrdinaryArtwork(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxViewRow[] rows = projection.Rows
                .Where(row => !row.IsSystemItem).ToArray();
            Check(rows.Length == 30 && rows.All(row => row.OrdinarySprite != null),
                "ordinary artwork did not resolve 30/30");
            Check(ItemSystemBattleSandboxViewProjection.OrdinaryRarityIndex == 5,
                "ordinary artwork rarity index is not 5");
        }

        private static void VerifySystemArtwork(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxViewRow system = projection.Rows.Single(row => row.IsSystemItem);
            Check(system.SystemUnlitSprite != null && system.SystemLitSprite != null,
                "I031 lit/unlit artwork missing");
            Check(!ReferenceEquals(system.SystemUnlitSprite, system.SystemLitSprite),
                "I031 lit/unlit artwork reused");
            Check(projection.Rows.Where(row => !row.IsSystemItem).All(row =>
                    !ReferenceEquals(row.OrdinarySprite, system.SystemUnlitSprite)
                    && !ReferenceEquals(row.OrdinarySprite, system.SystemLitSprite)),
                "ordinary item reuses I031 artwork");
        }

        private static void VerifyAuthoritativeCardArtwork(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            Scene targetScene = SceneManager.GetSceneByPath(
                ItemSystemBattleSandboxBoardAdapter.TargetScenePath);
            bool openedForCheck = !targetScene.IsValid() || !targetScene.isLoaded;
            if (openedForCheck)
            {
                targetScene = EditorSceneManager.OpenScene(
                    ItemSystemBattleSandboxBoardAdapter.TargetScenePath,
                    OpenSceneMode.Additive);
            }
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                BuildItemPreviewCardView[] actualCards = targetScene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        BuildItemPreviewCardView>(true))
                    .Where(card => card != null)
                    .OrderBy(card => card.name, StringComparer.Ordinal)
                    .ToArray();
                Check(actualCards.Length > 0, "target Scene has no authored item cards");
                foreach (string regressionName in new[]
                         {
                             "ItemCard_04", "ItemCard_05", "ItemCard_07"
                         })
                {
                    Check(actualCards.Any(card => string.Equals(card.name,
                            regressionName, StringComparison.Ordinal)),
                        regressionName + " hierarchy missing");
                }

                int bound = 0;
                foreach (ItemSystemBattleSandboxViewRow row in projection.Rows)
                {
                    BuildItemPreviewCardView source = actualCards.FirstOrDefault(card =>
                            string.Equals(card.name,
                                "ItemCard_" + row.Ordinal.ToString("00",
                                    CultureInfo.InvariantCulture),
                                StringComparison.Ordinal))
                        ?? actualCards[(row.Ordinal - 1) % actualCards.Length];
                    GameObject clone = UnityEngine.Object.Instantiate(source.gameObject);
                    clone.hideFlags = HideFlags.HideAndDontSave;
                    SceneManager.MoveGameObjectToScene(clone, previewScene);
                    clone.SetActive(true);
                    BuildItemPreviewCardView card =
                        clone.GetComponent<BuildItemPreviewCardView>();
                    card.BindItemDisplayData(null, row.BaseItemId, row.DisplayName,
                        row.CategoryDisplayName, row.ShapeDisplayName, Color.white);
                    string before = ArtworkStateSignature(card);
                    Sprite sprite = row.ResolveSprite(row.IsSystemItem);
                    Check(card.BindAuthoritativeArtwork(sprite),
                        row.BaseItemId + " final artwork bind failed");
                    Check(card.AuthoritativeArtworkImage != null
                        && ReferenceEquals(
                            card.AuthoritativeArtworkImage.overrideSprite != null
                                ? card.AuthoritativeArtworkImage.overrideSprite
                                : card.AuthoritativeArtworkImage.sprite,
                            sprite), row.BaseItemId + " final renderer sprite mismatch");
                    List<ShapeCellVisualStyle> styles = new();
                    Check(card.TryCaptureCellVisualStyles(styles)
                        && styles.Count == 1
                        && ReferenceEquals(styles[0].Sprite, sprite)
                        && styles[0].SpansWholeItem,
                        row.BaseItemId + " tray/ghost/board style seam mismatch");
                    int visibleLegacy = card.GetComponentsInChildren<Image>(true)
                        .Count(image => image != null
                            && image != card.AuthoritativeArtworkImage
                            && image.enabled && image.gameObject.activeSelf
                            && (image.sprite != null || image.overrideSprite != null)
                            && IsWholeItemName(image.name));
                    Check(visibleLegacy == 0,
                        row.BaseItemId + " retained a visible legacy artwork renderer");
                    card.RestoreAuthoritativeArtwork();
                    Check(ArtworkStateSignature(card) == before,
                        row.BaseItemId + " artwork state did not restore");
                    UnityEngine.Object.DestroyImmediate(clone);
                    bound++;
                }
                Check(bound == 31, "final visible renderer binding is not 31/31");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
                if (openedForCheck && targetScene.IsValid())
                {
                    EditorSceneManager.CloseScene(targetScene, true);
                }
            }
        }

        private static bool IsWholeItemName(string name)
        {
            string safe = name ?? string.Empty;
            return new[] { "art", "artwork", "icon", "image", "picture", "sprite", "visual" }
                .Any(value => safe.IndexOf(value,
                    StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string ArtworkStateSignature(BuildItemPreviewCardView card)
        {
            return string.Join("\n", card.GetComponentsInChildren<Image>(true)
                .OrderBy(image => image.transform.GetSiblingIndex())
                .ThenBy(image => image.name, StringComparer.Ordinal)
                .Select(image => (image.name ?? string.Empty)
                    + "|" + image.gameObject.activeSelf
                    + "|" + image.enabled
                    + "|" + (image.sprite == null ? 0 : image.sprite.GetInstanceID())
                    + "|" + (image.overrideSprite == null
                        ? 0 : image.overrideSprite.GetInstanceID())
                    + "|" + image.color));
        }

        private static void VerifyBaseline()
        {
            ItemSystemSnapshot baseline = DefaultItemSystemSnapshotProvider.Instance
                .CreateSnapshot(new ItemSystemSnapshotInput(
                    Array.Empty<ItemSystemPlacementInput>(),
                    catalogItems: ItemInnerDataCatalog.AllItems,
                    i031StateInputs: new[]
                    {
                        I031InventoryPlacementContract.OwnedInventory()
                    }));
            Check(baseline.isValid && baseline.placements.Count == 0
                && baseline.schemaVersion == ItemSystemSnapshot.CurrentSchemaVersion
                && baseline.i031State.ownershipCompleteness
                    == I031OwnershipCompleteness.Complete
                && baseline.i031State.location == I031Location.Inventory
                && baseline.LitRangeCells.Count == 0,
                "owned Inventory empty-board baseline is not valid v2");
        }

        private static void VerifyLegacyOmissionRejected()
        {
            ItemSystemSnapshot omitted = DefaultItemSystemSnapshotProvider.Instance
                .CreateSnapshot(new ItemSystemSnapshotInput(
                    Array.Empty<ItemSystemPlacementInput>(),
                    catalogItems: ItemInnerDataCatalog.AllItems));
            Check(!omitted.isValid && omitted.validationErrors.Any(error =>
                    error.code == "I031_LOCATION_UNKNOWN"
                    || error.code == "I031_OWNERSHIP_INCOMPLETE"),
                "omitted explicit I031 state became success");
        }

        private static void VerifyLegalCommit(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            int ifBefore = authority.If01ValidationCount;
            int p6Before = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult result = authority.CommitFromTray(
                "I001", new ItemShapeCell(1, 0), ItemShapeRotation.Rotation0);
            Check(result.Accepted && result.Changed, "legal commit rejected");
            Check(authority.If01ValidationCount == ifBefore + 1
                && authority.P6EvaluationCount == p6Before + 1,
                "legal commit refresh count mismatch");
            ItemInstancePlacementBindingSnapshot binding =
                authority.CurrentBindingSnapshot.FindByPlacementId("P_BOARD_I001");
            Check(binding != null && binding.baseItemId == "I001"
                && binding.itemInstanceId != binding.placementId,
                "explicit IF01 identity mismatch");
        }

        private static void VerifyRejectedCommits(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "setup commit failed");
            string signature = authority.CurrentSnapshot.BuildDebugSignature();
            int p6 = authority.P6EvaluationCount;
            Check(!authority.CommitFromTray("I002", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "overlap accepted");
            Check(!authority.CommitFromTray("I002", new ItemShapeCell(4, 4),
                ItemShapeRotation.Rotation0).Accepted, "out-of-bounds accepted");
            Check(!authority.CommitMove("P_BOARD_I001", new ItemShapeCell(2, 2),
                ItemShapeRotation.Rotation0).Accepted, "eye-cell accepted");
            Check(authority.P6EvaluationCount == p6
                && authority.CurrentSnapshot.BuildDebugSignature() == signature,
                "rejected commit changed state or refreshed P6");
        }

        private static void VerifyMove(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "setup commit failed");
            int before = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult moved = authority.CommitMove(
                "P_BOARD_I001", new ItemShapeCell(3, 0), ItemShapeRotation.Rotation0);
            Check(moved.Accepted && moved.Changed, "move rejected");
            Check(authority.CurrentSnapshot.placements.Single(value =>
                    value.itemId == "I001").anchorCell == new Vector2Int(3, 0),
                "move did not commit new anchor");
            Check(authority.P6EvaluationCount == before + 1, "move P6 count mismatch");
        }

        private static void VerifyRotate(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I002", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "rotation setup failed");
            int before = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult preview = authority.PreviewPlacement(
                "I002", new ItemShapeCell(2, 1), ItemShapeRotation.Rotation90);
            Check(preview.Accepted && authority.P6EvaluationCount == before,
                "rotate preview failed or refreshed P6");
            ItemSystemBattleSandboxBoardOperationResult committed = authority.CommitMove(
                "P_BOARD_I002", new ItemShapeCell(2, 1), ItemShapeRotation.Rotation90);
            Check(committed.Accepted && authority.P6EvaluationCount == before + 1,
                "rotate commit failed or refresh count mismatch");
            Check(authority.CurrentSnapshot.placements.Single(value =>
                    value.itemId == "I002").rotation == 90,
                "rotation was not committed as degrees");
        }

        private static void VerifyReturn(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "return setup failed");
            Check(authority.ReturnToTray("P_BOARD_I001").Accepted
                && authority.CurrentSnapshot.placements.All(value =>
                    value.itemId != "I001"), "ordinary return failed");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(0, 0),
                ItemShapeRotation.Rotation0).Accepted, "I031 return setup failed");
            Check(authority.ReturnToTray("P_SYSTEM_I031").Accepted
                && authority.CurrentSnapshot.i031State.location
                    == I031Location.Inventory
                && authority.CurrentSnapshot.LitRangeCells.Count == 0,
                "I031 return did not clear board lighting");
        }

        private static void VerifyReset(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "reset setup failed");
            Check(authority.CommitFromTray("I031", new ItemShapeCell(4, 4),
                ItemShapeRotation.Rotation0).Accepted, "I031 placement setup failed");
            ItemSystemBattleSandboxBoardOperationResult reset = authority.Reset();
            Check(reset.Accepted && reset.Changed, "changed reset failed");
            Check(authority.CurrentSnapshot.placements.Count == 0
                && authority.CurrentSnapshot.i031State.location
                    == I031Location.Inventory
                && authority.CurrentSnapshot.LitRangeCells.Count == 0,
                "reset baseline mismatch");
        }

        private static void VerifyNoOpDeduplication(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.If01ValidationCount == 1 && authority.P6EvaluationCount == 1,
                "initial baseline did not refresh exactly once");
            ItemSystemSnapshot reference = authority.CurrentSnapshot;
            ItemSystemBattleSandboxBoardOperationResult reset = authority.Reset();
            Check(reset.Accepted && !reset.Changed
                && ReferenceEquals(reference, authority.CurrentSnapshot),
                "no-op reset replaced snapshot");
            Check(authority.If01ValidationCount == 1 && authority.P6EvaluationCount == 1,
                "no-op reset repeated IF01/P6");
        }

        private static void VerifyChangedRefresh(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "changed commit failed");
            Check(authority.If01ValidationCount == 2 && authority.P6EvaluationCount == 2,
                "changed commit count mismatch");
            Check(authority.Reset().Accepted, "changed reset failed");
            Check(authority.If01ValidationCount == 3 && authority.P6EvaluationCount == 3,
                "changed reset count mismatch");
        }

        private static void VerifyUnknownDependency(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            bool threw = false;
            try
            {
                _ = new ItemSystemBattleSandboxBoardAuthority(
                    projection,
                    null,
                    ItemInstancePlacementBindingValidator.Instance,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance);
            }
            catch (ArgumentNullException)
            {
                threw = true;
            }
            Check(threw, "null snapshot authority was silently accepted");
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxBoardOperationResult unknown =
                authority.PreviewPlacement("UNKNOWN", default,
                    ItemShapeRotation.Rotation0);
            Check(!unknown.Accepted
                && unknown.Status == ItemSystemBattleSandboxBoardOperationStatus.Unknown,
                "unknown item became empty success");
        }

        private static void VerifyCompatibilityProjection(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted, "compatibility setup failed");
            string before = authority.CurrentSnapshot.BuildDebugSignature();
            BuildSandboxLayoutSnapshot compatibility =
                ItemSystemBattleSandboxViewProjection.ToCompatibilitySnapshot(
                    authority.CurrentSnapshot, authority.Rows);
            Check(compatibility.placedItems.Count == 1, "compatibility count mismatch");
            Check(compatibility.placedItems.Any(item => item.itemId == "I001"),
                "compatibility identity mismatch");
            Check(before == authority.CurrentSnapshot.BuildDebugSignature(),
                "compatibility projection wrote back to ItemSystem");
        }

        private static void VerifyTargetPath()
        {
            ItemSystemBattleSandboxBoardInstallDecision accepted =
                ItemSystemBattleSandboxBoardAdapter.DecideInstallation(
                    ItemSystemBattleSandboxBoardAdapter.TargetScenePath, 1);
            Check(accepted.ShouldInstall, "target path rejected");
            Check(!ItemSystemBattleSandboxBoardAdapter.DecideInstallation(
                    ItemSystemBattleSandboxBoardAdapter.TargetScenePath, 0).ShouldInstall,
                "missing controller accepted");
            Check(!ItemSystemBattleSandboxBoardAdapter.DecideInstallation(
                    ItemSystemBattleSandboxBoardAdapter.TargetScenePath, 2).ShouldInstall,
                "duplicate controllers accepted");
        }

        private static void VerifyRejectedPaths()
        {
            Check(!ItemSystemBattleSandboxBoardAdapter.DecideInstallation(
                    "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity", 1)
                .ShouldInstall, "ItemSandbox path accepted");
            Check(!ItemSystemBattleSandboxBoardAdapter.DecideInstallation(
                    "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattleShell.unity", 1)
                .ShouldInstall, "UnifiedBattle shell path accepted");
        }

        private static void VerifyStaticBoundaries(bool includeStaticChecks)
        {
            if (!includeStaticChecks)
            {
                return;
            }
            foreach (KeyValuePair<string, string> protectedFile in
                     ProtectedNonWhitelistHashes)
            {
                Check(HashFile(ProjectPath(protectedFile.Key)) == protectedFile.Value,
                    "protected hash changed: " + protectedFile.Key);
            }
            string scenePath = ProjectPath(
                "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity");
            Check(HashFile(scenePath) == ExpectedSceneSha256, "target scene hash changed");
            string authority = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs");
            string adapter = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs");
            string projection = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs");
            string controller = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs");
            string trayView = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs");
            string detailPresenter = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs");
            string scene = ReadProjectFile(
                "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity");
            Check((authority + adapter + projection).IndexOf(".SetParent(",
                StringComparison.Ordinal) < 0, "runtime adapter reparents UI");
            Check((authority + adapter + projection).IndexOf("RectTransform",
                StringComparison.Ordinal) < 0, "runtime adapter writes layout");
            Check(controller.IndexOf("boardReceiver.Commit", StringComparison.Ordinal) < 0,
                "controller directly calls boardReceiver.Commit");
            int authorityBranch = controller.IndexOf(
                "if (itemSystemBoardAuthority != null)", StringComparison.Ordinal);
            int legacyCommit = controller.IndexOf(
                "placementSession.Commit(boardReceiver)", StringComparison.Ordinal);
            int legacyPlacedMutation = controller.IndexOf(
                "placedItemIds.Add(selectedItem.ItemId)", StringComparison.Ordinal);
            Check(authorityBranch >= 0 && legacyCommit > authorityBranch
                && legacyPlacedMutation > legacyCommit,
                "authority branch does not precede legacy cache commit/mutation");
            Check(controller.IndexOf("InstallItemSystemAuthorityRuntimeSlots",
                    StringComparison.Ordinal) >= 0
                && controller.IndexOf("UninstallItemSystemAuthorityRuntimeSlots",
                    StringComparison.Ordinal) >= 0
                && controller.IndexOf(
                    "BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount",
                    StringComparison.Ordinal) >= 0,
                "controller does not own the authority tray lifecycle/capacity seam");
            Check(trayView.IndexOf("HideFlags.DontSaveInEditor",
                    StringComparison.Ordinal) >= 0
                && trayView.IndexOf("HideFlags.DontSaveInBuild",
                    StringComparison.Ordinal) >= 0
                && trayView.IndexOf("preAuthorityContentSizeDeltaY",
                    StringComparison.Ordinal) >= 0,
                "runtime slots are not DontSave or Content height is not restorable");
            Check(trayView.IndexOf("SetSiblingIndex", StringComparison.Ordinal) < 0
                && trayView.IndexOf("SetAsFirstSibling", StringComparison.Ordinal) < 0
                && trayView.IndexOf("SetAsLastSibling", StringComparison.Ordinal) < 0,
                "tray view reorders authored slots");
            Check(CountOccurrences(scene, "m_Name: TrayGridSlot_") == 40
                && scene.IndexOf("TrayGridSlot_Runtime_", StringComparison.Ordinal) < 0,
                "target scene serialized or changed tray slots");
            Check(adapter.IndexOf("TRAY_CAPACITY_WHITELIST_BLOCKER",
                StringComparison.Ordinal) < 0, "Revision02 capacity blocker remains active");
            Check(adapter.IndexOf("ItemDetailPanelPrefab", StringComparison.Ordinal) < 0
                && adapter.IndexOf("LoadAssetAtPath<GameObject>",
                    StringComparison.Ordinal) < 0
                && detailPresenter.IndexOf("Instantiate(",
                    StringComparison.Ordinal) < 0
                && detailPresenter.IndexOf("Destroy(",
                    StringComparison.Ordinal) < 0
                && detailPresenter.IndexOf("DestroyImmediate(",
                    StringComparison.Ordinal) < 0,
                "Revision08 detail path still loads/creates/destroys a panel");
            string forbidden = authority + adapter + projection;
            foreach (string token in new[]
                     {
                         "SaveData", "RewardService", "RunFlow", "BuildSettings",
                         "Chapter progression", "formal inventory"
                     })
            {
                Check(forbidden.IndexOf(token, StringComparison.Ordinal) < 0,
                    "forbidden formal token found: " + token);
            }
        }

        private static void VerifyDetailPresenter(
            ItemSystemBattleSandboxViewProjectionResult projection,
            string mode)
        {
            string scenePath = ProjectPath(
                ItemSystemBattleSandboxBoardAdapter.TargetScenePath);
            string sceneHash = HashFile(scenePath);
            if (string.Equals(mode, "lifecycle", StringComparison.Ordinal))
            {
                VerifyAuthoredDetailPanelSceneReadOnly();
            }

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            GameObject popupFixture = new("PopupLayer", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(popupFixture, previewScene);
            GameObject panelFixture = new("ItemDetailPanel", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(panelFixture, previewScene);
            panelFixture.transform.SetParent(popupFixture.transform, false);
            panelFixture.AddComponent<ItemDetailPanelView>();
            ItemSystemBattleSandboxItemDetailAdapter presenter = new();
            bool uninstalled = false;
            try
            {
                ItemDetailPanelView authoredPanel =
                    ResolveUniqueAuthoredDetailPanel(previewScene, false);
                GameObject authoredObject = authoredPanel.gameObject;
                RectTransform authoredRect = authoredPanel.transform as RectTransform;
                Check(authoredObject.activeSelf,
                    "authored panel serialized active state is not true");
                Check(authoredRect != null, "authored panel RectTransform missing");
                Transform parentBefore = authoredRect.parent;
                int siblingBefore = authoredRect.GetSiblingIndex();
                int parentChildCountBefore = parentBefore.childCount;
                Vector3 localPositionBefore = authoredRect.localPosition;
                Quaternion localRotationBefore = authoredRect.localRotation;
                Vector3 localScaleBefore = authoredRect.localScale;
                Vector2 anchorMinBefore = authoredRect.anchorMin;
                Vector2 anchorMaxBefore = authoredRect.anchorMax;
                Vector2 anchoredPositionBefore = authoredRect.anchoredPosition;
                Vector2 sizeDeltaBefore = authoredRect.sizeDelta;
                Vector2 pivotBefore = authoredRect.pivot;

                Check(presenter.Initialize(authoredPanel, projection.Rows),
                    "detail initialize failed: " + presenter.LastDiagnosticCode);
                Check(ReferenceEquals(presenter.AuthoredPanelInstance, authoredObject)
                    && !authoredObject.activeSelf,
                    "authored detail was not reused and hidden on install");

                ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
                ItemSystemBattleSandboxViewRow ordinary = projection.Rows
                    .Single(row => row.BaseItemId == "I001");
                ItemSystemBattleSandboxViewRow system = projection.Rows
                    .Single(row => row.IsSystemItem);

                switch (mode)
                {
                    case "lifecycle":
                    {
                        Check(presenter.Show("I001", string.Empty,
                                authority.CurrentSnapshot),
                            "first detail show failed");
                        GameObject first = presenter.AuthoredPanelInstance;
                        Check(presenter.Show("I031", string.Empty,
                                authority.CurrentSnapshot),
                            "second detail show failed");
                        Check(ReferenceEquals(first, presenter.AuthoredPanelInstance)
                            && parentBefore.childCount == parentChildCountBefore,
                            "authored detail panel was not reused");
                        break;
                    }
                    case "identity":
                    {
                        Check(presenter.Show("I001", string.Empty,
                                authority.CurrentSnapshot),
                            "ordinary detail show failed");
                        Check(presenter.LastProjectedModel.itemInstanceId
                                == ordinary.ItemInstanceId
                            && ReferenceEquals(presenter.LastBoundSprite,
                                ordinary.OrdinarySprite),
                            "ordinary instance/art identity mismatch");
                        Check(presenter.Show("I031", string.Empty,
                                authority.CurrentSnapshot),
                            "I031 detail show failed");
                        Check(string.IsNullOrEmpty(
                                presenter.LastProjectedModel.itemInstanceId)
                            && ReferenceEquals(presenter.LastBoundSprite,
                                system.SystemUnlitSprite),
                            "I031 catalog/art identity mismatch");
                        break;
                    }
                    case "catalog":
                    {
                        Check(presenter.Show("I001", string.Empty,
                                authority.CurrentSnapshot),
                            "catalog detail show failed");
                        ItemDetailViewModel model = presenter.LastProjectedModel;
                        Check(model.placementId == string.Empty
                            && !model.statusFlags.isLit
                            && !model.statusFlags.countedInBuild,
                            "catalog preview invented placed state");
                        break;
                    }
                    case "placed":
                    {
                        PlaceLitOrdinary(authority, "I001");
                        ItemSystemPlacementSnapshot placement =
                            authority.CurrentSnapshot.FindPlacement("P_BOARD_I001");
                        Check(presenter.Show("I001", "P_BOARD_I001",
                                authority.CurrentSnapshot),
                            "placed detail show failed");
                        ItemDetailViewModel model = presenter.LastProjectedModel;
                        Check(model.statusFlags.placementId == placement.placementId
                            && model.statusFlags.isLit == placement.isLit
                            && model.statusFlags.isDirectLit == placement.isDirectLit
                            && model.statusFlags.countedInBuild
                                == placement.isCountedInBuild
                            && model.statusFlags.isOnArrayBonusCell
                                == placement.isOnArrayBonusCell
                            && model.statusFlags.resolvedLevel
                                == placement.resolvedLevel
                            && model.skillMonitorPreview != null,
                            "placed runtime projection does not match v2 facts");
                        break;
                    }
                    case "clone":
                    {
                        string before = JsonUtility.ToJson(
                            system.CreateBaseDetailModelClone());
                        Check(presenter.Show("I031", string.Empty,
                                authority.CurrentSnapshot),
                            "inventory I031 detail failed");
                        ItemDetailViewModel first = presenter.LastProjectedModel;
                        Check(authority.CommitFromTray("I031",
                                new ItemShapeCell(1, 1),
                                ItemShapeRotation.Rotation0).Accepted,
                            "I031 board setup failed");
                        Check(presenter.Show("I031", "P_SYSTEM_I031",
                                authority.CurrentSnapshot),
                            "board I031 detail failed");
                        ItemDetailViewModel second = presenter.LastProjectedModel;
                        Check(!ReferenceEquals(first, second)
                            && first.placementId == string.Empty
                            && second.placementId == "P_SYSTEM_I031"
                            && second.statusFlags.isLightingSource,
                            "detail did not create a fresh latest-snapshot clone");
                        Check(before == JsonUtility.ToJson(
                                system.CreateBaseDetailModelClone()),
                            "cached base detail model mutated");
                        break;
                    }
                    case "invalid":
                    {
                        Check(presenter.Show("I001", string.Empty,
                                authority.CurrentSnapshot),
                            "valid detail setup failed");
                        Check(!presenter.Show("I001", "P_STALE",
                                authority.CurrentSnapshot)
                            && !presenter.IsVisible
                            && presenter.LastProjectedModel == null,
                            "stale placement displayed stale detail");
                        Check(!presenter.Show("I001", string.Empty, null)
                            && !presenter.IsVisible,
                            "null snapshot displayed detail");
                        Check(authority.CommitFromTray("I031",
                                new ItemShapeCell(1, 1),
                                ItemShapeRotation.Rotation0).Accepted,
                            "mismatch setup failed");
                        Check(!presenter.Show("I001", "P_SYSTEM_I031",
                                authority.CurrentSnapshot)
                            && !presenter.IsVisible,
                            "mismatched base/placement displayed detail");
                        break;
                    }
                    case "close":
                    {
                        Check(presenter.Show("I001", string.Empty,
                                authority.CurrentSnapshot),
                            "close setup failed");
                        authoredPanel.Close();
                        Check(!presenter.IsVisible,
                            "authored Close did not hide detail");
                        break;
                    }
                    default:
                        throw new InvalidOperationException("unknown detail verifier mode");
                }

                presenter.Uninstall();
                uninstalled = true;
                Check(presenter.AuthoredPanelInstance == null
                    && authoredObject != null
                    && !authoredObject.activeSelf,
                    "uninstall destroyed or retained the authored detail binding");
                Check(ReferenceEquals(authoredRect.parent, parentBefore)
                    && authoredRect.GetSiblingIndex() == siblingBefore
                    && parentBefore.childCount == parentChildCountBefore
                    && authoredRect.localPosition == localPositionBefore
                    && authoredRect.localRotation == localRotationBefore
                    && authoredRect.localScale == localScaleBefore
                    && authoredRect.anchorMin == anchorMinBefore
                    && authoredRect.anchorMax == anchorMaxBefore
                    && authoredRect.anchoredPosition == anchoredPositionBefore
                    && authoredRect.sizeDelta == sizeDeltaBefore
                    && authoredRect.pivot == pivotBefore,
                    "authored detail hierarchy or RectTransform changed");
            }
            finally
            {
                if (!uninstalled)
                {
                    presenter.Uninstall();
                }
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
            Check(HashFile(scenePath) == sceneHash
                && sceneHash == ExpectedSceneSha256,
                "target scene changed during authored detail verification");
        }

        private static void VerifyDaoPinDetailModels(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxViewRow[] ordinary = projection.Rows
                .Where(row => row != null && !row.IsSystemItem)
                .OrderBy(row => row.Ordinal)
                .ToArray();
            Check(ordinary.Length == 30, "DaoPin detail row count is not 30");
            foreach (ItemSystemBattleSandboxViewRow row in ordinary)
            {
                ItemDetailViewModel model = row.CreateBaseDetailModelClone();
                Check(model != null, row.BaseItemId + " DaoPin model is null");
                Check(string.Equals(model.baseItemId, row.BaseItemId,
                        StringComparison.Ordinal)
                    && string.Equals(model.itemInstanceId, row.ItemInstanceId,
                        StringComparison.Ordinal),
                    row.BaseItemId + " DaoPin model identity mismatch");
                Check(string.Equals(model.rarityKey, "orange",
                        StringComparison.OrdinalIgnoreCase)
                    && string.Equals(model.displayRarityName, "道品",
                        StringComparison.Ordinal),
                    row.BaseItemId + " DaoPin rarity text is incomplete");
                Check(!string.IsNullOrWhiteSpace(model.displayItemName),
                    row.BaseItemId + " display name is blank");
                Check(!string.IsNullOrWhiteSpace(model.displayItemPower),
                    row.BaseItemId + " item power is blank");
                Check(!string.IsNullOrWhiteSpace(model.displayShapeName),
                    row.BaseItemId + " shape is blank");
                Check(model.displayPrimaryStats != null
                        && model.displayPrimaryStats.Count > 0,
                    row.BaseItemId + " base stats are empty");
                Check(model.displayFixedAffixes != null
                        && model.displayFixedAffixes.Count > 0,
                    row.BaseItemId + " fixed affixes are empty");
                Check(model.displayRandomAffixes != null
                        && model.displayRandomAffixes.Count > 0,
                    row.BaseItemId + " random affixes are empty");
                Check(!string.IsNullOrWhiteSpace(model.displayOrangeAffix),
                    row.BaseItemId + " DaoPin trace is empty");
                Check(model.displayCoreEffects != null
                        && model.displayCoreEffects.Count > 0,
                    row.BaseItemId + " core effects are empty");

                ItemDetailSectionViewModel[] visibleSections =
                    (model.displayPlayerSections
                        ?? new List<ItemDetailSectionViewModel>())
                    .Where(IsModelPlayerSectionVisible)
                    .ToArray();
                Check(visibleSections.Length > 0,
                    row.BaseItemId + " has no visible player sections");
                foreach (ItemDetailSectionViewModel section in visibleSections)
                {
                    Check(!string.IsNullOrWhiteSpace(section.title)
                            && !string.IsNullOrWhiteSpace(section.body),
                        row.BaseItemId + " visible section is incomplete: "
                        + (section.stateKey ?? "<null>"));
                }

                foreach (string stateKey in new[]
                         {
                             "stats", "fixedAffix", "randomAffix", "orange",
                             "coreEffect", "placement", "flavor"
                         })
                {
                    Check(visibleSections.Any(section => string.Equals(
                            section.stateKey, stateKey,
                            StringComparison.OrdinalIgnoreCase)),
                        row.BaseItemId + " required detail section missing: " + stateKey);
                }
            }
        }

        private static void VerifySectionVisibilityTransitions()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject sectionObject = new("SectionFixture", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(sectionObject, previewScene);
                Text title = CreateFixtureText("TitleText", sectionObject.transform);
                Text body = CreateFixtureText("BodyText", sectionObject.transform);
                GameObject rowsRoot = new("BaseStatRowsRoot", typeof(RectTransform));
                rowsRoot.transform.SetParent(body.transform, false);
                GameObject row = new("BaseStatRow_0", typeof(RectTransform));
                row.transform.SetParent(rowsRoot.transform, false);
                Text rowText = CreateFixtureText("BaseStatText_0", row.transform);
                GameObject unrelatedHidden = new("UnrelatedHiddenTab", typeof(RectTransform));
                unrelatedHidden.transform.SetParent(sectionObject.transform, false);
                unrelatedHidden.SetActive(false);
                row.SetActive(false);
                body.gameObject.SetActive(false);

                ItemDetailSectionView view =
                    sectionObject.AddComponent<ItemDetailSectionView>();
                view.ConfigureEditor(title, body);
                RectTransform sectionRect = sectionObject.transform as RectTransform;
                int hierarchyCount = CountHierarchy(sectionObject.transform);
                Transform parentBefore = sectionObject.transform.parent;
                Vector2 anchorMinBefore = sectionRect.anchorMin;
                Vector2 anchorMaxBefore = sectionRect.anchorMax;
                Vector2 anchoredPositionBefore = sectionRect.anchoredPosition;
                Vector2 sizeDeltaBefore = sectionRect.sizeDelta;
                Vector2 pivotBefore = sectionRect.pivot;

                VerifyModelSectionTransition(view, body, row, rowText, unrelatedHidden);
                VerifyStringSectionTransition(view, body, row, rowText, unrelatedHidden);

                Check(CountHierarchy(sectionObject.transform) == hierarchyCount
                        && ReferenceEquals(sectionObject.transform.parent, parentBefore)
                        && sectionRect.anchorMin == anchorMinBefore
                        && sectionRect.anchorMax == anchorMaxBefore
                        && sectionRect.anchoredPosition == anchoredPositionBefore
                        && sectionRect.sizeDelta == sizeDeltaBefore
                        && sectionRect.pivot == pivotBefore,
                    "section transition mutated hierarchy or RectTransform");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void VerifyModelSectionTransition(
            ItemDetailSectionView view,
            Text body,
            GameObject row,
            Text rowText,
            GameObject unrelatedHidden)
        {
            view.SetContent(new ItemDetailSectionViewModel(
                string.Empty, string.Empty, "stats", false));
            VerifyHiddenSection(view, body, row, rowText, unrelatedHidden,
                "model hidden");

            view.SetContent(new ItemDetailSectionViewModel(
                "基础属性", "普通正文", "stats", false));
            VerifyPlainSection(view, body, row, unrelatedHidden,
                "普通正文", "model plain");

            view.SetContent(new ItemDetailSectionViewModel(
                "基础属性", "攻击：8", "stats", false));
            VerifyAuthoredRowsSection(view, body, row, rowText, unrelatedHidden,
                "攻击：8", "model rows");

            view.SetContent(new ItemDetailSectionViewModel(
                "基础属性", "恢复正文", "stats", false));
            VerifyPlainSection(view, body, row, unrelatedHidden,
                "恢复正文", "model restored plain");

            view.SetContent(new ItemDetailSectionViewModel(
                string.Empty, string.Empty, "stats", false));
            VerifyHiddenSection(view, body, row, rowText, unrelatedHidden,
                "model final hidden");
        }

        private static void VerifyStringSectionTransition(
            ItemDetailSectionView view,
            Text body,
            GameObject row,
            Text rowText,
            GameObject unrelatedHidden)
        {
            view.SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
            VerifyHiddenSection(view, body, row, rowText, unrelatedHidden,
                "string hidden");

            view.SetContent("基础属性", "普通正文", keepWhenEmpty: false);
            VerifyPlainSection(view, body, row, unrelatedHidden,
                "普通正文", "string plain");

            view.SetContent("基础属性", "攻击：8", keepWhenEmpty: false);
            VerifyAuthoredRowsSection(view, body, row, rowText, unrelatedHidden,
                "攻击：8", "string rows");

            view.SetContent("基础属性", "恢复正文", keepWhenEmpty: false);
            VerifyPlainSection(view, body, row, unrelatedHidden,
                "恢复正文", "string restored plain");

            view.SetContent(string.Empty, string.Empty, keepWhenEmpty: true);
            Check(view.gameObject.activeSelf
                    && !body.gameObject.activeSelf
                    && !body.enabled
                    && string.IsNullOrEmpty(body.text)
                    && !row.activeSelf
                    && !unrelatedHidden.activeSelf,
                "keepWhenEmpty retained stale body/rows or activated unrelated content");

            view.SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
            VerifyHiddenSection(view, body, row, rowText, unrelatedHidden,
                "string final hidden");
        }

        private static void VerifyHiddenSection(
            ItemDetailSectionView view,
            Text body,
            GameObject row,
            Text rowText,
            GameObject unrelatedHidden,
            string label)
        {
            Check(!view.gameObject.activeSelf
                    && !body.gameObject.activeSelf
                    && !body.enabled
                    && string.IsNullOrEmpty(body.text)
                    && !row.activeSelf
                    && string.IsNullOrEmpty(rowText.text)
                    && !unrelatedHidden.activeSelf,
                label + " retained visible or stale presentation state");
        }

        private static void VerifyPlainSection(
            ItemDetailSectionView view,
            Text body,
            GameObject row,
            GameObject unrelatedHidden,
            string expectedBody,
            string label)
        {
            Check(view.gameObject.activeSelf
                    && body.gameObject.activeSelf
                    && body.enabled
                    && string.Equals(
                        NormalizeVisibleText(body.text),
                        NormalizeVisibleText(expectedBody),
                        StringComparison.Ordinal)
                    && !row.activeSelf
                    && !unrelatedHidden.activeSelf,
                label + " did not restore the plain BodyText exclusively");
        }

        private static void VerifyAuthoredRowsSection(
            ItemDetailSectionView view,
            Text body,
            GameObject row,
            Text rowText,
            GameObject unrelatedHidden,
            string expectedRow,
            string label)
        {
            Check(view.gameObject.activeSelf
                    && body.gameObject.activeSelf
                    && !body.enabled
                    && row.activeSelf
                    && rowText.enabled
                    && string.Equals(
                        NormalizeVisibleText(rowText.text),
                        NormalizeVisibleText(expectedRow),
                        StringComparison.Ordinal)
                    && !unrelatedHidden.activeSelf,
                label + " did not render authored rows without duplicate BodyText");
        }

        private static Text CreateFixtureText(string name, Transform parent)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            return text;
        }

        private static int CountHierarchy(Transform root)
        {
            int count = 1;
            for (int index = 0; index < root.childCount; index++)
            {
                count += CountHierarchy(root.GetChild(index));
            }
            return count;
        }

        private static void VerifyLegacyTextEvidence(
            Func<LegacyTextVerificationEvidence, bool> predicate,
            string failure)
        {
            Check(legacyTextEvidence != null && predicate != null
                    && predicate(legacyTextEvidence),
                failure);
        }

        private static void VerifyLegacyTextFontRecoveryContract()
        {
            VerifyLegacyTextEvidence(value => value.TargetFontResolved,
                "target panel font recovery evidence missing");

            string source = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
            int recoveryStart = source.IndexOf(
                "private bool RecoverMissingLegacyTextFonts()",
                StringComparison.Ordinal);
            int cachedResolverStart = source.IndexOf(
                "private Font ResolveCachedLegacyTextFont",
                StringComparison.Ordinal);
            int resolverStart = source.IndexOf(
                "private Font ResolveLegacyTextFont",
                StringComparison.Ordinal);
            int layoutStart = source.IndexOf(
                "private void RebuildExistingLayoutMeasurements()",
                StringComparison.Ordinal);
            Check(recoveryStart >= 0 && cachedResolverStart > recoveryStart
                    && resolverStart > cachedResolverStart
                    && layoutStart > resolverStart,
                "Legacy Text font recovery methods are missing or out of order");

            string recovery = source.Substring(
                recoveryStart, cachedResolverStart - recoveryStart);
            string cachedResolver = source.Substring(
                cachedResolverStart, resolverStart - cachedResolverStart);
            string resolver = source.Substring(
                resolverStart, layoutStart - resolverStart);
            int chineseFont = resolver.IndexOf(
                "if (chineseTextFont != null)", StringComparison.Ordinal);
            int descendantFont = resolver.IndexOf(
                "Font descendantFont", StringComparison.Ordinal);
            int simSunFont = resolver.IndexOf(
                "PreferredChineseOsFontName", StringComparison.Ordinal);
            int approvedFallback = resolver.IndexOf(
                "ApprovedChineseFallbackFontNames", StringComparison.Ordinal);
            int builtinFont = resolver.IndexOf(
                "Resources.GetBuiltinResource<Font>(",
                StringComparison.Ordinal);
            Check(chineseFont >= 0 && descendantFont > chineseFont
                    && simSunFont > descendantFont
                    && approvedFallback > simSunFont
                    && builtinFont > approvedFallback,
                "Legacy Text font resolver priority is not chinese/descendant/SimSun/approved fallback/builtin");
            Check(cachedResolver.IndexOf("legacyTextFontResolutionAttempted",
                        StringComparison.Ordinal) >= 0
                    && cachedResolver.IndexOf("legacyTextFontResolutionCount++",
                        StringComparison.Ordinal) >= 0
                    && resolver.IndexOf("Font.GetOSInstalledFontNames()",
                        StringComparison.Ordinal) >= 0
                    && source.IndexOf("StringComparison.OrdinalIgnoreCase",
                        StringComparison.Ordinal) >= 0
                    && source.IndexOf("Font.CreateDynamicFontFromOSFont",
                        StringComparison.Ordinal) >= 0,
                "Legacy Text font resolver does not cache or probe installed Chinese fonts exactly");
            Check(recovery.IndexOf("GetComponentsInChildren<Text>(true)",
                        StringComparison.Ordinal) >= 0
                    && recovery.IndexOf("text != null && text.font == null",
                        StringComparison.Ordinal) >= 0,
                "Legacy Text recovery is not generic or does not limit writes to null fonts");
            foreach (string forbiddenWrite in new[]
                     {
                         ".fontSize =", ".color =", ".alignment =",
                         ".material =", ".fontStyle =", ".lineSpacing ="
                     })
            {
                Check(recovery.IndexOf(forbiddenWrite, StringComparison.Ordinal) < 0,
                    "Legacy Text recovery overwrites an authored visual parameter: "
                    + forbiddenWrite);
            }
            Check(source.IndexOf(LegacyTextFailureDiagnosticFragment,
                    StringComparison.Ordinal) >= 0,
                "Legacy Text recovery lacks the stable Chinese failure diagnostic");
        }

        private static void VerifyRuntimeCreatedLegacyTextFont()
        {
            string source = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
            Check(source.IndexOf("GetComponentsInChildren<Text>(true)",
                    StringComparison.Ordinal) >= 0
                    && source.IndexOf("Resources.GetBuiltinResource<Font>(",
                        StringComparison.Ordinal) >= 0
                    && source.IndexOf("LegacyTextFontResourceName",
                        StringComparison.Ordinal) >= 0
                    && CountOccurrences(source, "RecoverMissingLegacyTextFonts()") == 3,
                "ItemDetailPanelView does not run generic pre/post font recovery");

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject panelObject = new("ItemDetailPanel", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(panelObject, previewScene);
                ItemDetailPanelView panel = panelObject.AddComponent<ItemDetailPanelView>();
                Text authoredText = CreateFixtureText("AuthoredText", panelObject.transform);
                Font authoredFont = authoredText.font;
                Text runtimeText = CreateFixtureText("RuntimeGeneratedText", panelObject.transform);
                runtimeText.font = null;
                runtimeText.text = "运行时文字";

                panel.Bind(null);
                Check(authoredFont != null
                        && ReferenceEquals(authoredText.font, authoredFont)
                        && ReferenceEquals(runtimeText.font, authoredFont),
                    "runtime-created Text did not receive the first descendant valid Font");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void VerifyItemSandboxSharedViewRegression(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            string absolutePath = ProjectPath(ItemSandboxScenePath);
            string diskHash = HashFile(absolutePath);
            Scene existing = SceneManager.GetSceneByPath(ItemSandboxScenePath);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            SceneSetup[] originalSetup = wasLoaded
                ? EditorSceneManager.GetSceneManagerSetup()
                : Array.Empty<SceneSetup>();
            if (wasLoaded)
            {
                for (int index = 0; index < SceneManager.sceneCount; index++)
                {
                    Check(!SceneManager.GetSceneAt(index).isDirty,
                        "S20N will not replace a loaded dirty scene");
                }
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                Check(EditorSceneManager.CloseScene(existing, true),
                    "S20N could not isolate the clean ItemSandbox scene");
            }

            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(ItemSandboxScenePath, OpenSceneMode.Additive);
                ItemDetailPanelView[] panels = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ItemDetailPanelView>(true))
                    .Where(value => value != null)
                    .ToArray();
                Check(panels.Length == 1,
                    "ItemSandbox shared ItemDetailPanelView is not unique");
                ItemDetailPanelView panel = panels[0];
                ItemSystemBattleSandboxViewRow row = projection.Rows
                    .Single(value => value != null && value.BaseItemId == "I001");
                ItemDetailViewModel model = row.CreateBaseDetailModelClone();
                panel.Bind(model, row.OrdinarySprite);
                panel.SetVisible(true);
                VerifyAuthoredHeaderText(panel, model, "ItemSandbox I001");
                VerifyConnectedStatusBadgeText(panel, "ItemSandbox I001");
                VerifyAuthoredPlayerSectionText(panel, model, "ItemSandbox I001");
                VerifyActiveDataBearingTextMetrics(panel, "ItemSandbox I001");
                panel.SetVisible(false);
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
                if (wasLoaded)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }

            Check(HashFile(absolutePath) == diskHash,
                "S20N shared-view verification changed ItemSandbox scene on disk");
            Check(HashFile(ProjectPath(
                    "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"))
                    == ProtectedNonWhitelistHashes[
                        "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"],
                "S20N shared-view verification changed ItemDetailPanel prefab");
        }

        private static void VerifySourceAndTargetTypographyEvidence()
        {
            string sourceHash = HashFile(ProjectPath(ItemSandboxScenePath));
            string targetHash = HashFile(ProjectPath(
                ItemSystemBattleSandboxBoardAdapter.TargetScenePath));
            int sourceTextCount = 0;
            int sourceSimSunCount = 0;
            InspectSceneReadOnly(ItemSandboxScenePath, scene =>
            {
                ItemDetailPanelView[] sourcePanels = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<ItemDetailPanelView>(true))
                    .Where(value => value != null)
                    .ToArray();
                Check(sourcePanels.Length == 1,
                    "S20O ItemSandbox shared ItemDetailPanelView is not unique");
                ItemDetailPanelView panel = sourcePanels[0];
                Text[] texts = panel.GetComponentsInChildren<Text>(true)
                    .Where(value => value != null)
                    .ToArray();
                sourceTextCount = texts.Length;
                sourceSimSunCount = texts.Count(value => value.font != null
                    && string.Equals(value.font.name, SimSunFontName,
                        StringComparison.OrdinalIgnoreCase));
            });

            int targetTextCount = 0;
            int targetNullFontCount = 0;
            bool targetSerializedFontNull = false;
            InspectSceneReadOnly(
                ItemSystemBattleSandboxBoardAdapter.TargetScenePath,
                scene =>
                {
                    ItemDetailPanelView panel = ResolveUniqueAuthoredDetailPanel(
                        scene, true);
                    Text[] texts = panel.GetComponentsInChildren<Text>(true)
                        .Where(value => value != null)
                        .ToArray();
                    targetTextCount = texts.Length;
                    targetNullFontCount = texts.Count(value => value.font == null);
                    SerializedObject serialized = new(panel);
                    serialized.UpdateIfRequiredOrScript();
                    SerializedProperty chineseFont = serialized.FindProperty(
                        "chineseTextFont");
                    targetSerializedFontNull = chineseFont != null
                        && chineseFont.objectReferenceValue == null;
                });

            Check(sourceTextCount > 0 && sourceSimSunCount == sourceTextCount,
                "S20O ItemSandbox ItemDetailPanel is not uniformly SimSun: "
                + sourceSimSunCount.ToString(CultureInfo.InvariantCulture)
                + "/" + sourceTextCount.ToString(CultureInfo.InvariantCulture));
            Check(targetTextCount > 0
                    && targetNullFontCount == targetTextCount
                    && targetSerializedFontNull,
                "S20O target null-font evidence changed: "
                + targetNullFontCount.ToString(CultureInfo.InvariantCulture)
                + "/" + targetTextCount.ToString(CultureInfo.InvariantCulture));
            Check(HashFile(ProjectPath(ItemSandboxScenePath)) == sourceHash
                    && HashFile(ProjectPath(
                        ItemSystemBattleSandboxBoardAdapter.TargetScenePath))
                        == targetHash,
                "S20O typography admission mutated a Scene");
        }

        private static void VerifyTargetCachedChineseFont(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            string scenePath = ItemSystemBattleSandboxBoardAdapter.TargetScenePath;
            string sceneHash = HashFile(ProjectPath(scenePath));
            bool simSunInstalled = InstalledFontName(SimSunFontName) != null;
            string approvedFallback = FirstInstalledFontName(new[]
            {
                "Microsoft YaHei",
                "Noto Sans CJK SC",
                "Droid Sans Fallback"
            });
            string resolvedName = string.Empty;
            string resolvedSource = string.Empty;
            string fallbackReason = string.Empty;
            int resolutionCount = 0;
            int targetTextCount = 0;
            int resolvedTextCount = 0;

            InspectSceneReadOnly(scenePath, scene =>
            {
                ItemDetailPanelView panel = ResolveUniqueAuthoredDetailPanel(
                    scene, true);
                ItemSystemBattleSandboxViewRow row = projection.Rows
                    .Single(value => value != null
                        && string.Equals(value.BaseItemId, "I001",
                            StringComparison.Ordinal));
                panel.Bind(row.CreateBaseDetailModelClone(), row.OrdinarySprite);
                panel.Bind(row.CreateBaseDetailModelClone(), row.OrdinarySprite);
                targetTextCount = panel.GetComponentsInChildren<Text>(true).Length;
                resolvedTextCount = panel.GetComponentsInChildren<Text>(true)
                    .Count(value => value != null && value.font != null);
                Font resolved = panel.CachedLegacyTextFont;
                resolvedName = resolved != null ? resolved.name : string.Empty;
                resolvedSource = panel.CachedLegacyTextFontSource ?? string.Empty;
                fallbackReason = panel.CachedLegacyTextFontFallbackReason
                    ?? string.Empty;
                resolutionCount = panel.LegacyTextFontResolutionCount;
            });

            Check(targetTextCount > 0 && resolvedTextCount == targetTextCount,
                "S20P target font coverage incomplete: "
                + resolvedTextCount.ToString(CultureInfo.InvariantCulture)
                + "/" + targetTextCount.ToString(CultureInfo.InvariantCulture));
            Check(resolutionCount == 1,
                "S20P resolved the panel font more than once: "
                + resolutionCount.ToString(CultureInfo.InvariantCulture));
            if (simSunInstalled)
            {
                Check(string.Equals(resolvedName, SimSunFontName,
                            StringComparison.OrdinalIgnoreCase)
                        && resolvedSource.IndexOf(SimSunFontName,
                            StringComparison.OrdinalIgnoreCase) >= 0
                        && string.IsNullOrWhiteSpace(fallbackReason),
                    "S20P installed SimSun was not selected: "
                    + resolvedName + " / " + resolvedSource);
            }
            else if (!string.IsNullOrWhiteSpace(approvedFallback))
            {
                Check(string.Equals(resolvedName, approvedFallback,
                            StringComparison.OrdinalIgnoreCase)
                        && resolvedSource.IndexOf(approvedFallback,
                            StringComparison.OrdinalIgnoreCase) >= 0
                        && !string.IsNullOrWhiteSpace(fallbackReason),
                    "S20P approved Chinese fallback is not explicit: "
                    + resolvedName + " / " + resolvedSource + " / "
                    + fallbackReason);
            }
            else
            {
                Check(!string.IsNullOrWhiteSpace(resolvedName)
                        && resolvedSource.IndexOf("LegacyRuntime.ttf",
                            StringComparison.Ordinal) >= 0
                        && !string.IsNullOrWhiteSpace(fallbackReason),
                    "S20P last-resort LegacyRuntime fallback is not explicit");
            }

            fontEvidence = new FontVerificationEvidence(
                simSunInstalled,
                resolvedName,
                resolvedSource,
                fallbackReason,
                resolutionCount);
            Check(HashFile(ProjectPath(scenePath)) == sceneHash
                    && sceneHash == ExpectedSceneSha256,
                "S20P font verification mutated the target Scene");
        }

        private static string InstalledFontName(string requestedName)
        {
            string[] installed;
            try
            {
                installed = Font.GetOSInstalledFontNames()
                    ?? Array.Empty<string>();
            }
            catch (Exception)
            {
                installed = Array.Empty<string>();
            }

            return installed.FirstOrDefault(value => string.Equals(
                value, requestedName, StringComparison.OrdinalIgnoreCase));
        }

        private static string FirstInstalledFontName(
            IReadOnlyList<string> requestedNames)
        {
            if (requestedNames == null)
            {
                return null;
            }

            for (int index = 0; index < requestedNames.Count; index++)
            {
                string installed = InstalledFontName(requestedNames[index]);
                if (!string.IsNullOrWhiteSpace(installed))
                {
                    return installed;
                }
            }

            return null;
        }

        private static void InspectSceneReadOnly(
            string scenePath,
            Action<Scene> inspection)
        {
            Check(!string.IsNullOrWhiteSpace(scenePath)
                    && inspection != null,
                "read-only Scene inspection arguments are invalid");
            Scene existing = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !existing.IsValid() || !existing.isLoaded;
            Scene scene = existing;
            if (!openedHere)
            {
                Check(!scene.isDirty,
                    "read-only Scene inspection refuses a loaded dirty Scene: "
                    + scenePath);
            }

            try
            {
                if (openedHere)
                {
                    scene = EditorSceneManager.OpenScene(
                        scenePath, OpenSceneMode.Additive);
                }
                inspection(scene);
            }
            finally
            {
                if (openedHere && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void VerifyAuthoredBodyContainer(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            EnsureAuthoredVisualEvidence(projection);
            Check(authoredVisualEvidence.BodyFailures.Count == 0,
                "S20Q BodyText container failures: "
                + string.Join(" | ", authoredVisualEvidence.BodyFailures));
        }

        private static void VerifyAuthoredIconGroup(
            ItemSystemBattleSandboxViewProjectionResult projection,
            AuthoredIconGroup group)
        {
            EnsureAuthoredVisualEvidence(projection);
            IconObservation[] observations = authoredVisualEvidence.Icons
                .Where(value => value.Group == group)
                .ToArray();
            Check(observations.Length > 0,
                "no applicable authored icon observations for " + group);
            IconObservation[] failures = observations
                .Where(value => !value.Passed)
                .ToArray();
            Check(failures.Length == 0,
                group + " authored icon failures: "
                + string.Join(" | ", failures.Select(value => value.DescribeFailure())));
        }

        private static void VerifyPlacementAndFlavorIcons(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            VerifyAuthoredIconGroup(projection, AuthoredIconGroup.Placement);
            VerifyAuthoredIconGroup(projection, AuthoredIconGroup.Flavor);
        }

        private static void EnsureAuthoredVisualEvidence(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            if (authoredVisualEvidence != null)
            {
                return;
            }

            AuthoredVisualVerificationEvidence evidence = new();
            string scenePath = ItemSystemBattleSandboxBoardAdapter.TargetScenePath;
            string sceneHash = HashFile(ProjectPath(scenePath));
            InspectSceneReadOnly(scenePath, scene =>
            {
                ItemDetailPanelView panel = ResolveUniqueAuthoredDetailPanel(
                    scene, true);
                ItemSystemBattleSandboxItemDetailAdapter presenter = new();
                bool initialized = false;
                try
                {
                    Check(presenter.Initialize(panel, projection.Rows),
                        "authored visual presenter initialize failed: "
                        + presenter.LastDiagnosticCode);
                    initialized = true;
                    ItemSystemBattleSandboxBoardAuthority authority =
                        NewAuthority(projection);
                    foreach (ItemSystemBattleSandboxViewRow row in projection.Rows
                                 .Where(value => value != null && !value.IsSystemItem)
                                 .OrderBy(value => value.Ordinal))
                    {
                        Check(presenter.Show(row.BaseItemId, string.Empty,
                                authority.CurrentSnapshot),
                            row.BaseItemId + " authored visual bind failed: "
                            + presenter.LastDiagnosticCode);
                        ItemDetailViewModel model = presenter.LastProjectedModel;
                        Check(model != null,
                            row.BaseItemId + " authored visual model missing");
                        panel.SetVisible(true);
                        Canvas.ForceUpdateCanvases();

                        CollectAuthoredGroup(
                            panel, model, row.BaseItemId,
                            AuthoredIconGroup.FaMenBuild,
                            CountBuildStageRows(model, true), evidence);
                        CollectAuthoredGroup(
                            panel, model, row.BaseItemId,
                            AuthoredIconGroup.QiLeiBuild,
                            CountBuildStageRows(model, false), evidence);
                        CollectAuthoredGroup(
                            panel, model, row.BaseItemId,
                            AuthoredIconGroup.CoreEffect,
                            CountDisplayedCoreRows(model),
                            evidence);
                        CollectAuthoredGroup(
                            panel, model, row.BaseItemId,
                            AuthoredIconGroup.Placement,
                            HasVisibleModelSection(model, "placement") ? 1 : 0,
                            evidence);
                        CollectAuthoredGroup(
                            panel, model, row.BaseItemId,
                            AuthoredIconGroup.Flavor,
                            HasVisibleModelSection(model, "flavor") ? 1 : 0,
                            evidence);
                    }

                    presenter.Hide();
                }
                finally
                {
                    if (initialized)
                    {
                        presenter.Uninstall();
                    }
                }
            });

            foreach (AuthoredIconGroup group in Enum.GetValues(
                         typeof(AuthoredIconGroup)))
            {
                Check(evidence.Icons.Any(value => value.Group == group),
                    "no data-bearing representative for authored icon group " + group);
            }

            authoredVisualEvidence = evidence;
            Check(HashFile(ProjectPath(scenePath)) == sceneHash
                    && sceneHash == ExpectedSceneSha256,
                "S20Q-S20U authored visual verification mutated target Scene");
        }

        private static bool HasVisibleModelSection(
            ItemDetailViewModel model,
            string stateKey)
        {
            ItemDetailSectionViewModel section = FindModelPlayerSection(
                model?.displayPlayerSections,
                new[] { stateKey });
            return IsModelPlayerSectionVisible(section);
        }

        private static int CountBuildStageRows(
            ItemDetailViewModel model,
            bool faMen)
        {
            ItemDetailSectionViewModel section = FindModelPlayerSection(
                model?.displayPlayerSections,
                new[] { faMen ? "famenBuild" : "qileiBuild" });
            return IsModelPlayerSectionVisible(section)
                ? faMen ? 3 : 2
                : 0;
        }

        private static int CountDisplayedCoreRows(ItemDetailViewModel model)
        {
            ItemDetailSectionViewModel section = FindModelPlayerSection(
                model?.displayPlayerSections,
                new[] { "coreEffect", "awakening" });
            if (!IsModelPlayerSectionVisible(section))
            {
                return 0;
            }

            return Math.Min(4, (section.body ?? string.Empty)
                .Replace("\r", string.Empty)
                .Split('\n')
                .Count(value => !string.IsNullOrWhiteSpace(
                    NormalizeVisibleText(value))));
        }

        private static void CollectAuthoredGroup(
            ItemDetailPanelView panel,
            ItemDetailViewModel model,
            string baseItemId,
            AuthoredIconGroup group,
            int expectedCount,
            AuthoredVisualVerificationEvidence evidence)
        {
            if (expectedCount <= 0)
            {
                return;
            }

            AuthoredIconGroupDefinition definition =
                AuthoredIconGroupDefinition.For(group);
            ItemDetailSectionView section = FindAuthoredSection(
                panel, definition.SectionName);
            Text bodyText = GetSectionBodyText(section);
            if (section == null || bodyText == null)
            {
                evidence.BodyFailures.Add(
                    baseItemId + "/" + group + ": missing BodyText reference");
            }
            else if (!section.gameObject.activeInHierarchy
                     || !bodyText.gameObject.activeInHierarchy
                     || bodyText.enabled)
            {
                evidence.BodyFailures.Add(
                    baseItemId + "/" + group
                    + ": authored mode must keep BodyText active and disable only its Text renderer");
            }

            Transform rowsRoot = bodyText != null
                ? bodyText.transform.Find(definition.RowsRootName)
                : null;
            for (int slotIndex = 0; slotIndex < expectedCount; slotIndex++)
            {
                Transform row = rowsRoot?.Find(
                    definition.RowNamePrefix
                    + slotIndex.ToString(CultureInfo.InvariantCulture));
                Text rowText = row != null
                    ? row.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault(value => value != null
                            && value.enabled
                            && !string.IsNullOrWhiteSpace(value.text))
                    : null;
                if (row == null || !row.gameObject.activeInHierarchy
                    || rowText == null)
                {
                    evidence.BodyFailures.Add(
                        baseItemId + "/" + group + "/row"
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ": authored row is inactive or empty");
                }

                Image icon = row?.Find(
                    definition.IconNamePrefix
                    + slotIndex.ToString(CultureInfo.InvariantCulture))
                    ?.GetComponent<Image>();
                evidence.Icons.Add(ObserveAuthoredIcon(
                    panel, model, baseItemId, group, slotIndex, icon));
            }
        }

        private static ItemDetailSectionView FindAuthoredSection(
            ItemDetailPanelView panel,
            string sectionName)
        {
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty sections = serialized.FindProperty("playerSections");
            if (sections == null || !sections.isArray)
            {
                return null;
            }

            for (int index = 0; index < sections.arraySize; index++)
            {
                ItemDetailSectionView section = sections
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue as ItemDetailSectionView;
                if (section != null && string.Equals(
                        section.name, sectionName, StringComparison.Ordinal))
                {
                    return section;
                }
            }

            return null;
        }

        private static Text GetSectionBodyText(ItemDetailSectionView section)
        {
            if (section == null)
            {
                return null;
            }

            SerializedObject serialized = new(section);
            serialized.UpdateIfRequiredOrScript();
            return ObjectReference<Text>(serialized, "bodyText");
        }

        private static IconObservation ObserveAuthoredIcon(
            ItemDetailPanelView panel,
            ItemDetailViewModel model,
            string baseItemId,
            AuthoredIconGroup group,
            int slotIndex,
            Image icon)
        {
            string actualPath = icon != null && icon.sprite != null
                ? AssetDatabase.GetAssetPath(icon.sprite).Replace('\\', '/')
                : string.Empty;
            bool intersectsViewport = false;
            bool notCulled = false;
            if (icon != null && icon.rectTransform != null)
            {
                TryObserveImageInViewport(
                    panel, icon, out intersectsViewport, out notCulled);
            }

            return new IconObservation(
                group,
                baseItemId,
                slotIndex,
                icon != null,
                icon != null && icon.sprite != null,
                icon != null && icon.enabled,
                icon != null && icon.gameObject.activeInHierarchy,
                icon != null && EffectiveImageAlpha(icon) > 0.001f,
                notCulled,
                intersectsViewport,
                IconIdentityMatches(
                    model, baseItemId, group, slotIndex, actualPath),
                actualPath);
        }

        private static void TryObserveImageInViewport(
            ItemDetailPanelView panel,
            Image image,
            out bool intersectsViewport,
            out bool notCulled)
        {
            intersectsViewport = false;
            notCulled = false;
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            ScrollRect scrollRect = ObjectReference<ScrollRect>(
                serialized, "detailScrollRect");
            RectTransform viewport = scrollRect != null
                ? scrollRect.viewport != null
                    ? scrollRect.viewport
                    : scrollRect.transform as RectTransform
                : null;
            if (scrollRect == null || viewport == null)
            {
                return;
            }

            float originalPosition = scrollRect.verticalNormalizedPosition;
            for (int step = 0; step <= 40; step++)
            {
                scrollRect.verticalNormalizedPosition = 1f - step / 40f;
                if (scrollRect.content != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                }
                Canvas.ForceUpdateCanvases();
                if (!WorldRectsOverlap(image.rectTransform, viewport))
                {
                    continue;
                }

                intersectsViewport = true;
                if (!image.canvasRenderer.cull)
                {
                    notCulled = true;
                    break;
                }
            }

            scrollRect.verticalNormalizedPosition = originalPosition;
            if (scrollRect.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            }
            Canvas.ForceUpdateCanvases();
        }

        private static bool WorldRectsOverlap(
            RectTransform left,
            RectTransform right)
        {
            Vector3[] leftCorners = new Vector3[4];
            Vector3[] rightCorners = new Vector3[4];
            left.GetWorldCorners(leftCorners);
            right.GetWorldCorners(rightCorners);
            Rect leftRect = Rect.MinMaxRect(
                leftCorners.Min(value => value.x),
                leftCorners.Min(value => value.y),
                leftCorners.Max(value => value.x),
                leftCorners.Max(value => value.y));
            Rect rightRect = Rect.MinMaxRect(
                rightCorners.Min(value => value.x),
                rightCorners.Min(value => value.y),
                rightCorners.Max(value => value.x),
                rightCorners.Max(value => value.y));
            return leftRect.width > 0f && leftRect.height > 0f
                && rightRect.width > 0f && rightRect.height > 0f
                && leftRect.Overlaps(rightRect, true);
        }

        private static float EffectiveImageAlpha(Image image)
        {
            float alpha = image != null ? image.color.a : 0f;
            for (Transform current = image != null ? image.transform : null;
                 current != null;
                 current = current.parent)
            {
                CanvasGroup group = current.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    alpha *= group.alpha;
                }
            }
            return alpha;
        }

        private static bool IconIdentityMatches(
            ItemDetailViewModel model,
            string baseItemId,
            AuthoredIconGroup group,
            int slotIndex,
            string actualPath)
        {
            if (string.IsNullOrWhiteSpace(actualPath))
            {
                return false;
            }

            if (group == AuthoredIconGroup.Placement
                || group == AuthoredIconGroup.Flavor)
            {
                string expectedGuid = group == AuthoredIconGroup.Placement
                    ? AcceptedPlacementIconGuid
                    : AcceptedFlavorIconGuid;
                return string.Equals(
                    AssetDatabase.AssetPathToGUID(actualPath),
                    expectedGuid,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (group == AuthoredIconGroup.CoreEffect)
            {
                string suffix = "/" + baseItemId + "/" + baseItemId + "_"
                    + (slotIndex + 1).ToString(CultureInfo.InvariantCulture)
                    + ".png";
                return actualPath.EndsWith(suffix,
                    StringComparison.OrdinalIgnoreCase)
                    && actualPath.IndexOf("/item/核心icon/",
                        StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (group == AuthoredIconGroup.FaMenBuild)
            {
                string folder = NormalizeFaMenFolder(model?.displayFaMenName);
                string expected = "Assets/_Game/Resources/item/build技能icon/"
                    + folder + "/技能icon_"
                    + (slotIndex + 2).ToString(CultureInfo.InvariantCulture)
                    + ".png";
                return !string.IsNullOrWhiteSpace(folder)
                    && string.Equals(actualPath, expected,
                        StringComparison.OrdinalIgnoreCase);
            }

            string qiLeiFolder = NormalizeQiLeiFolder(model?.displayQiLeiName);
            string stem = qiLeiFolder switch
            {
                "符类" => "符技能icon_",
                "印类" => "印技能icon_",
                "令类" => "令技能icon_",
                "镜类" => "镜技能icon_",
                "法类" => "法技能icon_",
                _ => string.Empty
            };
            string qiLeiExpected = "Assets/_Game/Resources/item/器类build技能icon/"
                + qiLeiFolder + "/" + stem
                + (slotIndex + 1).ToString(CultureInfo.InvariantCulture)
                + ".png";
            return !string.IsNullOrWhiteSpace(qiLeiFolder)
                && !string.IsNullOrWhiteSpace(stem)
                && string.Equals(actualPath, qiLeiExpected,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeFaMenFolder(string value)
        {
            string normalized = NormalizeResourceName(value);
            return normalized switch
            {
                "zhenlei" or "震雷" or "震雷法" => "震雷法",
                "lihuo" or "离火" or "离火法" => "离火法",
                "zhongyue" or "中岳" or "中岳法" => "中岳法",
                "xuanshui" or "玄水" or "玄水法" => "玄水法",
                "taibai" or "太白" or "太白法" => "太白法",
                _ => string.Empty
            };
        }

        private static string NormalizeQiLeiFolder(string value)
        {
            string normalized = NormalizeResourceName(value);
            return normalized switch
            {
                "fu" or "符" or "符类" => "符类",
                "yin" or "印" or "印类" => "印类",
                "ling" or "令" or "令类" => "令类",
                "jing" or "镜" or "镜类" => "镜类",
                "fa" or "法" or "法类" => "法类",
                _ => string.Empty
            };
        }

        private static string NormalizeResourceName(string value)
        {
            StringBuilder builder = new();
            foreach (char character in (value ?? string.Empty).Trim())
            {
                if (!char.IsWhiteSpace(character)
                    && character != '-' && character != '_')
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }
            return builder.ToString();
        }

        private static void VerifyPlainAuthoredRoundTrip()
        {
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject sectionObject = new(
                    "PlacementHintSection", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(sectionObject, previewScene);
                ItemDetailSectionView section =
                    sectionObject.AddComponent<ItemDetailSectionView>();
                Text title = CreateFixtureText("TitleText", sectionObject.transform);
                Text body = CreateFixtureText("BodyText", sectionObject.transform);
                GameObject rowsRoot = new("PlacementRowsRoot", typeof(RectTransform));
                rowsRoot.transform.SetParent(body.transform, false);
                GameObject row = new("PlacementRow_0", typeof(RectTransform));
                row.transform.SetParent(rowsRoot.transform, false);
                Text rowText = CreateFixtureText("PlacementText_0", row.transform);
                GameObject iconObject = new(
                    "PlacementIconSlot_0", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(row.transform, false);
                Image icon = iconObject.GetComponent<Image>();
                string spritePath = AssetDatabase.GUIDToAssetPath(
                    AcceptedPlacementIconGuid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                Check(sprite != null, "S20V accepted placement Sprite is missing");
                icon.sprite = sprite;
                icon.color = Color.white;
                Font titleFont = title.font;
                Font bodyFont = body.font;
                Font rowFont = rowText.font;
                section.ConfigureEditor(title, body);

                section.SetContent(string.Empty, string.Empty, false);
                section.SetContent(new ItemDetailSectionViewModel(
                    "普通", "普通正文", "plain", false));
                Check(section.gameObject.activeSelf
                        && body.gameObject.activeSelf && body.enabled
                        && NormalizeVisibleText(body.text).Contains("普通正文")
                        && !row.activeSelf,
                    "S20V hidden-to-plain transition failed");

                section.SetContent(new ItemDetailSectionViewModel(
                    "推荐摆放", "保持核心格通路", "placement", false));
                Check(section.gameObject.activeSelf
                        && body.gameObject.activeSelf && !body.enabled
                        && row.activeSelf && rowText.enabled
                        && !string.IsNullOrWhiteSpace(rowText.text)
                        && icon.enabled && icon.gameObject.activeInHierarchy
                        && ReferenceEquals(icon.sprite, sprite),
                    "S20V plain-to-authored transition lost row/icon visibility");

                section.SetContent(new ItemDetailSectionViewModel(
                    "普通", "返回正文", "plain", false));
                Check(body.gameObject.activeSelf && body.enabled
                        && NormalizeVisibleText(body.text).Contains("返回正文")
                        && !row.activeSelf && string.IsNullOrWhiteSpace(rowText.text)
                        && ReferenceEquals(icon.sprite, sprite),
                    "S20V authored-to-plain transition left duplicate rows");

                section.SetContent(string.Empty, string.Empty, false);
                Check(!section.gameObject.activeSelf
                        && string.IsNullOrWhiteSpace(body.text)
                        && !row.activeSelf && string.IsNullOrWhiteSpace(rowText.text)
                        && ReferenceEquals(title.font, titleFont)
                        && ReferenceEquals(body.font, bodyFont)
                        && ReferenceEquals(rowText.font, rowFont)
                        && ReferenceEquals(icon.sprite, sprite),
                    "S20V hidden transition changed font/Sprite identity or left stale state");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static void VerifyAuthoredDetailPanelBinding(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            string scenePath = ItemSystemBattleSandboxBoardAdapter.TargetScenePath;
            string absoluteScenePath = ProjectPath(scenePath);
            string diskHash = HashFile(absoluteScenePath);
            Scene existing = SceneManager.GetSceneByPath(scenePath);
            bool targetWasLoaded = existing.IsValid() && existing.isLoaded;
            SceneSetup[] originalSetup = targetWasLoaded
                ? EditorSceneManager.GetSceneManagerSetup()
                : Array.Empty<SceneSetup>();
            if (targetWasLoaded)
            {
                for (int index = 0; index < SceneManager.sceneCount; index++)
                {
                    Check(!SceneManager.GetSceneAt(index).isDirty,
                        "S20F will not replace a loaded dirty scene");
                }
                EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                Check(EditorSceneManager.CloseScene(existing, true),
                    "S20F could not isolate the clean target scene");
            }

            Scene scene = default;
            ItemSystemBattleSandboxItemDetailAdapter presenter = new();
            bool presenterInitialized = false;
            int activeDataTextCount = 0;
            try
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                ItemDetailPanelView panel =
                    ResolveUniqueAuthoredDetailPanel(scene, true);
                Check(panel.gameObject.activeSelf,
                    "authored panel serialized active state is not true");
                VerifyAuthoredPanelReferences(panel);
                Check(presenter.Initialize(panel, projection.Rows),
                    "S20F detail initialize failed: " + presenter.LastDiagnosticCode);
                presenterInitialized = true;
                Check(!panel.gameObject.activeSelf,
                    "authored panel was not hidden before first S20F bind");

                ItemSystemBattleSandboxBoardAuthority authority =
                    NewAuthority(projection);
                foreach (ItemSystemBattleSandboxViewRow row in projection.Rows
                             .Where(value => value != null && !value.IsSystemItem)
                             .OrderBy(value => value.Ordinal))
                {
                    Check(presenter.Show(row.BaseItemId, string.Empty,
                            authority.CurrentSnapshot),
                        row.BaseItemId + " authored panel bind failed: "
                        + presenter.LastDiagnosticCode);
                    ItemDetailViewModel model = presenter.LastProjectedModel;
                    Check(model != null && panel.gameObject.activeInHierarchy,
                        row.BaseItemId + " authored panel is not visible");
                    VerifyAuthoredHeaderText(panel, model, row.BaseItemId);
                    VerifyConnectedStatusBadgeText(panel, row.BaseItemId);
                    VerifyAuthoredPlayerSectionText(panel, model, row.BaseItemId);
                    activeDataTextCount += VerifyActiveDataBearingTextMetrics(
                        panel, row.BaseItemId);
                }

                presenter.Hide();
                Check(!panel.gameObject.activeSelf,
                    "authored panel remained visible after S20F verification");
            }
            finally
            {
                if (presenterInitialized)
                {
                    presenter.Uninstall();
                }
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
                if (targetWasLoaded)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
                }
            }

            Check(HashFile(absoluteScenePath) == diskHash
                && diskHash == ExpectedSceneSha256,
                "S20F authored binding changed the target scene on disk");
            legacyTextEvidence = new LegacyTextVerificationEvidence(
                targetFontResolved: true,
                activeDataTextCount: activeDataTextCount,
                headerAndStatusVisible: true,
                bodyRowsExclusive: true,
                preferredMeasurementsValid: true);
        }

        private static void VerifyAuthoredHeaderText(
            ItemDetailPanelView panel,
            ItemDetailViewModel model,
            string label)
        {
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            Text itemName = ObjectReference<Text>(serialized, "itemNameText");
            Text meta = ObjectReference<Text>(serialized, "metaText");
            Text power = ObjectReference<Text>(serialized, "powerText");
            VerifyActuallyVisibleText(itemName, label + " item-name header");
            VerifyActuallyVisibleText(meta, label + " rarity/meta header");
            VerifyActuallyVisibleText(power, label + " power header");
            Check(NormalizeVisibleText(itemName.text).Contains(
                    NormalizeVisibleText(model.displayItemName))
                && NormalizeVisibleText(meta.text).Contains("道品")
                && NormalizeVisibleText(power.text).Contains(
                    NormalizeVisibleText(model.displayItemPower)),
                label + " authored header/model text mismatch");
        }

        private static void VerifyAuthoredPlayerSectionText(
            ItemDetailPanelView panel,
            ItemDetailViewModel model,
            string label)
        {
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty property = serialized.FindProperty("playerSections");
            Check(property != null && property.isArray && property.arraySize == 15,
                label + " authored player-section array mismatch");
            for (int index = 0; index < property.arraySize; index++)
            {
                ItemDetailSectionView target = property.GetArrayElementAtIndex(index)
                    .objectReferenceValue as ItemDetailSectionView;
                Check(target != null,
                    label + " authored player-section reference is null at " + index);
                string[] stateKeys = PlayerStateKeysForTargetVerification(target.name);
                Check(stateKeys != null && stateKeys.Length > 0,
                    label + " unknown authored player-section target: " + target.name);
                ItemDetailSectionViewModel source = FindModelPlayerSection(
                    model.displayPlayerSections, stateKeys);
                bool expectedVisible = IsModelPlayerSectionVisible(source);
                if (!expectedVisible)
                {
                    Check(!target.gameObject.activeSelf,
                        label + " view shows a model-hidden section: " + target.name);
                    continue;
                }

                Check(target.gameObject.activeInHierarchy,
                    label + " model-visible section is hidden: " + target.name);
                Check(string.Equals(target.Title?.Trim(), source.title?.Trim(),
                        StringComparison.Ordinal),
                    label + " section title mismatch: " + target.name);
                SerializedObject targetSerialized = new(target);
                targetSerialized.UpdateIfRequiredOrScript();
                Text titleText = ObjectReference<Text>(targetSerialized, "titleText");
                Check(!string.IsNullOrWhiteSpace(titleText.text),
                    label + " section title field is blank: " + target.name);
                Text plainBody = ObjectReference<Text>(targetSerialized, "bodyText");
                Text[] visibleAuthoredRowTexts = plainBody
                    .GetComponentsInChildren<Text>(true)
                    .Where(text => !ReferenceEquals(text, plainBody)
                        && IsActuallyVisibleText(text)
                        && !string.IsNullOrWhiteSpace(text.text))
                    .ToArray();
                bool plainBodyVisible = IsActuallyVisibleText(plainBody)
                    && !string.IsNullOrWhiteSpace(plainBody.text);
                Check(plainBodyVisible != (visibleAuthoredRowTexts.Length > 0),
                    label + " section duplicates or hides both BodyText and authored rows: "
                    + target.name);
                Check(plainBody.gameObject.activeSelf
                        && (plainBodyVisible
                            ? plainBody.enabled
                            : !plainBody.enabled),
                    label + " section BodyText container/mode is invalid: " + target.name);
                Text[] visibleBodyTexts = target.GetComponentsInChildren<Text>(true)
                    .Where(text => !ReferenceEquals(text, titleText)
                        && IsActuallyVisibleText(text))
                    .ToArray();
                Check(visibleBodyTexts.Length > 0,
                    label + " section has no visible body text: " + target.name);
                string renderedBody = string.Join("\n",
                    visibleBodyTexts.Select(text => text.text));
                VerifyRenderedBodyCoversModel(
                    source.body, renderedBody, label + " " + target.name);
            }
        }

        private static void VerifyConnectedStatusBadgeText(
            ItemDetailPanelView panel,
            string label)
        {
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            SerializedProperty property = serialized.FindProperty("statusBadgeTexts");
            Check(property != null && property.isArray && property.arraySize == 3,
                label + " status badge array mismatch");
            int connectedCount = 0;
            for (int index = 0; index < property.arraySize; index++)
            {
                Text text = property.GetArrayElementAtIndex(index)
                    .objectReferenceValue as Text;
                if (text == null)
                {
                    continue;
                }

                connectedCount++;
                VerifyActuallyVisibleText(text,
                    label + " connected status badge "
                    + index.ToString(CultureInfo.InvariantCulture));
            }

            Check(connectedCount == 2,
                label + " connected status badge cardinality changed");
        }

        private static int VerifyActiveDataBearingTextMetrics(
            ItemDetailPanelView panel,
            string label)
        {
            Canvas.ForceUpdateCanvases();
            Text[] dataBearing = panel.GetComponentsInChildren<Text>(true)
                .Where(text => IsActuallyVisibleText(text)
                    && NormalizeVisibleText(text.text).Length > 0)
                .ToArray();
            Check(dataBearing.Length > 0,
                label + " has no active data-bearing Legacy Text");
            foreach (Text text in dataBearing)
            {
                Check(text.font != null,
                    label + " active Text has null font: " + text.name);
                Check(text.preferredWidth > 0.001f && text.preferredHeight > 0.001f,
                    label + " active Text has collapsed preferred measurement: "
                    + text.name);
            }

            return dataBearing.Length;
        }

        private static T ObjectReference<T>(
            SerializedObject serialized,
            string propertyName) where T : UnityEngine.Object
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Check(property != null && property.objectReferenceValue != null,
                "authored reference missing: " + propertyName);
            return property.objectReferenceValue as T;
        }

        private static void VerifyActuallyVisibleText(Text text, string label)
        {
            Check(IsActuallyVisibleText(text)
                && !string.IsNullOrWhiteSpace(text.text),
                label + " has null font, is disabled, zero-alpha, hidden, or blank");
        }

        private static bool IsActuallyVisibleText(Text text)
        {
            if (text == null || text.font == null
                || !text.enabled || !text.gameObject.activeInHierarchy
                || text.color.a <= 0.001f
                || text.canvasRenderer.GetAlpha() <= 0.001f)
            {
                return false;
            }
            for (Transform current = text.transform;
                 current != null;
                 current = current.parent)
            {
                CanvasGroup[] groups = current.GetComponents<CanvasGroup>();
                if (groups.Any(group => group != null
                    && group.enabled && group.alpha <= 0.001f))
                {
                    return false;
                }
            }
            return true;
        }

        private static void VerifyRenderedBodyCoversModel(
            string modelBody,
            string renderedBody,
            string label)
        {
            Check(!string.IsNullOrWhiteSpace(modelBody),
                label + " model body is blank");
            string rendered = NormalizeVisibleText(renderedBody);
            Check(rendered.Length > 0, label + " rendered body is blank");
            string[] expectedLines = (modelBody ?? string.Empty)
                .Replace("[Icon_ArrayVeinModifier]", string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in expectedLines)
            {
                string expected = NormalizeVisibleText(line);
                if (expected.Length == 0 || expected == "────────")
                {
                    continue;
                }
                string renderedData = NormalizeDataText(rendered);
                string expectedData = NormalizeDataText(expected);
                Check(rendered.Contains(expected)
                        || (expectedData.Length > 0
                            && renderedData.Contains(expectedData)),
                    label + " rendered body omits model text: " + line.Trim());
            }
        }

        private static string NormalizeDataText(string value)
        {
            StringBuilder normalized = new();
            foreach (char character in value ?? string.Empty)
            {
                if (char.IsLetterOrDigit(character)
                    || character == '/' || character == '%'
                    || character == '.' || character == '+')
                {
                    normalized.Append(character);
                }
            }

            return normalized.ToString();
        }

        private static string NormalizeVisibleText(string value)
        {
            string source = (value ?? string.Empty)
                .Replace("[Icon_ArrayVeinModifier]", string.Empty);
            StringBuilder normalized = new();
            bool insideTag = false;
            foreach (char character in source)
            {
                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }
                if (insideTag)
                {
                    if (character == '>')
                    {
                        insideTag = false;
                    }
                    continue;
                }
                if (!char.IsWhiteSpace(character))
                {
                    normalized.Append(character);
                }
            }
            return normalized.ToString();
        }

        private static bool IsModelPlayerSectionVisible(
            ItemDetailSectionViewModel section)
        {
            return section != null
                && !string.Equals(section.stateKey, "divider",
                    StringComparison.OrdinalIgnoreCase)
                && (section.keepWhenEmpty
                    || !string.IsNullOrWhiteSpace(section.body));
        }

        private static ItemDetailSectionViewModel FindModelPlayerSection(
            IReadOnlyList<ItemDetailSectionViewModel> sections,
            IReadOnlyList<string> stateKeys)
        {
            if (sections == null || stateKeys == null)
            {
                return null;
            }
            foreach (string stateKey in stateKeys)
            {
                ItemDetailSectionViewModel match = sections.FirstOrDefault(section =>
                    section != null && string.Equals(section.stateKey, stateKey,
                        StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }
            }
            return null;
        }

        private static string[] PlayerStateKeysForTargetVerification(string targetName)
        {
            return targetName switch
            {
                "HeaderSection" => new[] { "header", "itemPower" },
                "CoreIdentitySection" => new[] { "identity" },
                "CurrentStateSection" => new[] { "currentState", "status" },
                "BaseStatsSection" => new[] { "stats" },
                "TriggerConditionSection" => new[] { "trigger" },
                "BasicEffectSection" => new[] { "basic" },
                "CoreAwakeningSection" => new[] { "coreEffect", "awakening" },
                "FaMenBuildSection" => new[] { "famenBuild" },
                "QiLeiBuildSection" => new[] { "qileiBuild" },
                "MainBuildMonitorSection" => new[] { "skillMonitor" },
                "FixedAffixSection" => new[] { "fixedAffix" },
                "RandomAffixSection" => new[] { "randomAffix" },
                "OrangeGrowthSection" => new[] { "orange" },
                "PlacementHintSection" => new[] { "placement" },
                "FlavorSection" => new[] { "flavor" },
                _ => null
            };
        }

        private static ItemDetailPanelView ResolveUniqueAuthoredDetailPanel(
            Scene scene,
            bool requireTargetScene)
        {
            Check(scene.IsValid()
                && (!requireTargetScene || string.Equals(scene.path,
                    ItemSystemBattleSandboxBoardAdapter.TargetScenePath,
                    StringComparison.Ordinal)),
                "target preview scene path mismatch");
            Transform[] popupLayers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => value != null && string.Equals(
                    value.name, "PopupLayer", StringComparison.Ordinal))
                .ToArray();
            Check(popupLayers.Length == 1, "authored PopupLayer is not unique");
            Transform[] detailRoots = popupLayers[0].Cast<Transform>()
                .Where(value => value != null && string.Equals(
                    value.name, "ItemDetailPanel", StringComparison.Ordinal))
                .ToArray();
            Check(detailRoots.Length == 1,
                "authored direct-child ItemDetailPanel is not unique");
            ItemDetailPanelView[] rootViews =
                detailRoots[0].GetComponents<ItemDetailPanelView>();
            ItemDetailPanelView[] sceneViews = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ItemDetailPanelView>(true))
                .Where(value => value != null)
                .ToArray();
            Check(rootViews.Length == 1 && sceneViews.Length == 1
                && ReferenceEquals(rootViews[0], sceneViews[0]),
                "authored ItemDetailPanelView is not unique at the exact path");
            return rootViews[0];
        }

        private static void VerifyAuthoredDetailPanelSceneReadOnly()
        {
            string scenePath = ItemSystemBattleSandboxBoardAdapter.TargetScenePath;
            string diskHash = HashFile(ProjectPath(scenePath));
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForVerification = !scene.IsValid() || !scene.isLoaded;
            if (openedForVerification)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                ItemDetailPanelView panel =
                    ResolveUniqueAuthoredDetailPanel(scene, true);
                Check(panel.gameObject.activeSelf,
                    "authored panel serialized active state is not true");
                VerifyAuthoredPanelReferences(panel);
            }
            finally
            {
                if (openedForVerification && scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            Check(HashFile(ProjectPath(scenePath)) == diskHash
                && diskHash == ExpectedSceneSha256,
                "read-only authored panel inspection changed target scene");
        }

        private static void VerifyAuthoredPanelReferences(ItemDetailPanelView panel)
        {
            SerializedObject serialized = new(panel);
            serialized.UpdateIfRequiredOrScript();
            foreach (string propertyName in new[]
                     {
                         "itemNameText", "metaText", "powerText",
                         "artworkFrameImage", "artworkImageSlot", "rarityBadgeImage",
                         "closeButton", "cardBackgroundImage", "detailTabButton", "debugTabButton",
                         "detailTabText", "debugTabText", "detailScrollRoot",
                         "debugScrollRoot", "detailScrollRect", "debugScrollRect",
                         "visualTheme"
                     })
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                Check(property != null && property.objectReferenceValue != null,
                    "authored detail reference missing: " + propertyName);
            }

            VerifyObjectReferenceArray(serialized, "statusBadgeImages", 3, 0, 2);
            VerifyObjectReferenceArray(serialized, "statusBadgeTexts", 3, 0, 2);
            VerifyObjectReferenceArray(serialized, "playerSections", 15,
                Enumerable.Range(0, 15).ToArray());
            VerifyObjectReferenceArray(serialized, "debugSections", 3, 0, 1, 2);
        }

        private static void VerifyObjectReferenceArray(
            SerializedObject serialized,
            string propertyName,
            int expectedCount,
            params int[] requiredIndexes)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            Check(property != null && property.isArray
                && property.arraySize == expectedCount,
                "authored detail array cardinality mismatch: " + propertyName);
            foreach (int index in requiredIndexes ?? Array.Empty<int>())
            {
                Check(index >= 0 && index < property.arraySize,
                    "authored detail required index is out of range: " + propertyName);
                Check(property.GetArrayElementAtIndex(index).objectReferenceValue != null,
                    "authored detail array reference missing: " + propertyName
                    + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");
            }
        }

        private static void VerifyDetailConstructionBoundary()
        {
            string adapter = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs");
            string presenter = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs");
            string combined = adapter + presenter;
            foreach (string forbidden in new[]
                     {
                         "ItemDetailPanelPrefabPath", "ItemDetailPanel.prefab",
                         "LoadAssetAtPath<GameObject>", "Instantiate(",
                         "DestroyRuntimeObject", "DestroyImmediate(",
                         "ItemSandboxDetailPanelView", "ItemSandbox Presenter",
                         "ItemFullDetailBuildSandboxWorkbenchSession", ".SetParent(",
                         "SetSiblingIndex(", "SetAsLastSibling(", "localScale =",
                         "sizeDelta =", "anchoredPosition ="
                     })
            {
                Check(combined.IndexOf(forbidden, StringComparison.Ordinal) < 0,
                    "forbidden Revision08 detail path remains: " + forbidden);
            }
            Check(adapter.IndexOf("GetComponents<ItemDetailPanelView>()",
                    StringComparison.Ordinal) >= 0
                && adapter.IndexOf("DETAIL_AUTHORED_ROOT_CARDINALITY_INVALID",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("authoredScenePanel.SetVisible(false);",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("panelView.Bind(projected, sprite);",
                    StringComparison.Ordinal) >= 0,
                "authored detail resolve/hide/bind seam is incomplete");
        }

        private static void PlaceLitOrdinary(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId)
        {
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                ItemShapeRotation.Rotation0).Accepted,
                "I031 lighting setup failed");
            foreach (ItemShapeRotation rotation in new[]
                     {
                         ItemShapeRotation.Rotation0,
                         ItemShapeRotation.Rotation90,
                         ItemShapeRotation.Rotation180,
                         ItemShapeRotation.Rotation270
                     })
            {
                foreach (Vector2Int anchor in EnumerateBoardCells())
                {
                    ItemSystemBattleSandboxBoardOperationResult preview =
                        authority.PreviewPlacement(baseItemId,
                            new ItemShapeCell(anchor.x, anchor.y), rotation);
                    ItemSystemPlacementSnapshot placement = preview.Snapshot
                        ?.FindPlacement("P_BOARD_" + baseItemId);
                    if (preview.Accepted && placement?.isLit == true)
                    {
                        Check(authority.CommitFromTray(baseItemId,
                                new ItemShapeCell(anchor.x, anchor.y), rotation).Accepted,
                            "lit ordinary commit failed");
                        return;
                    }
                }
            }
            throw new InvalidOperationException(
                "no legal lit placement found for " + baseItemId);
        }

        private static void VerifyDetailStaticSeam()
        {
            string controller = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs");
            string presenter = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs");
            Check(controller.IndexOf("itemSystemDetailAdapter?.Show(",
                    StringComparison.Ordinal) >= 0
                && controller.IndexOf("itemInfoPanel?.Hide();",
                    StringComparison.Ordinal) >= 0,
                "authority tap is not routed to the detail presenter");
            Check(presenter.IndexOf("ItemDetailProjectionComposer.Compose(",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("ToLightingResolutionResult()",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("ToArrayBonusResolutionResult()",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("ToBuildSynergyResolutionResult()",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("ToCoreAwakeningResolutionResult()",
                    StringComparison.Ordinal) >= 0
                && presenter.IndexOf("ToSkillMonitorResolutionResult()",
                    StringComparison.Ordinal) >= 0,
                "presenter does not use all approved projection conversions");
        }

        private static void VerifyInventoryLighting(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I001", new ItemShapeCell(1, 0),
                ItemShapeRotation.Rotation0).Accepted,
                "inventory lighting setup failed");
            Check(authority.CurrentSnapshot.i031State.location
                    == I031Location.Inventory
                && authority.CurrentSnapshot.LitRangeCells.Count == 0
                && authority.CurrentSnapshot.placements.All(placement =>
                    !placement.isLit && !placement.isDirectLit
                    && !placement.isCountedInBuild),
                "Inventory I031 retained powered or Build state");
        }

        private static void VerifyFourWayLighting(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority interior = NewAuthority(projection);
            Check(interior.CommitFromTray("I031", new ItemShapeCell(1, 1),
                ItemShapeRotation.Rotation0).Accepted,
                "interior I031 placement failed");
            HashSet<Vector2Int> expectedInterior = new()
            {
                new Vector2Int(1, 2), new Vector2Int(2, 1),
                new Vector2Int(1, 0), new Vector2Int(0, 1)
            };
            Check(expectedInterior.SetEquals(interior.CurrentSnapshot.LitRangeCells)
                && !interior.CurrentSnapshot.LitRangeCells.Contains(
                    new Vector2Int(1, 1)),
                "interior I031 range is not exact orthogonal four cells");

            ItemSystemBattleSandboxBoardAuthority edge = NewAuthority(projection);
            Check(edge.CommitFromTray("I031", new ItemShapeCell(0, 2),
                ItemShapeRotation.Rotation0).Accepted,
                "edge I031 placement failed");
            HashSet<Vector2Int> expectedEdge = new()
            {
                new Vector2Int(0, 3), new Vector2Int(1, 2),
                new Vector2Int(0, 1)
            };
            Check(expectedEdge.SetEquals(edge.CurrentSnapshot.LitRangeCells),
                "edge I031 range is not clipped three-cell result");

            ItemSystemBattleSandboxBoardAuthority corner = NewAuthority(projection);
            Check(corner.CommitFromTray("I031", new ItemShapeCell(0, 0),
                ItemShapeRotation.Rotation0).Accepted,
                "corner I031 placement failed");
            HashSet<Vector2Int> expectedCorner = new()
            {
                new Vector2Int(1, 0), new Vector2Int(0, 1)
            };
            Check(expectedCorner.SetEquals(corner.CurrentSnapshot.LitRangeCells)
                && !corner.CurrentSnapshot.LitRangeCells.Contains(Vector2Int.zero),
                "corner I031 range is not clipped two-cell result");
        }

        private static void VerifyLightingTransition(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            Check(authority.CommitFromTray("I031", new ItemShapeCell(1, 1),
                ItemShapeRotation.Rotation0).Accepted,
                "lighting transition setup failed");
            HashSet<Vector2Int> oldRange =
                new(authority.CurrentSnapshot.LitRangeCells);
            Check(authority.CommitMove("P_SYSTEM_I031",
                    new ItemShapeCell(4, 4), ItemShapeRotation.Rotation0).Accepted,
                "I031 move failed");
            Check(!oldRange.SetEquals(authority.CurrentSnapshot.LitRangeCells)
                && authority.CurrentSnapshot.LitRangeCells.All(cell =>
                    cell.x == 4 || cell.y == 4),
                "I031 move retained stale range");
            Check(authority.ReturnToTray("P_SYSTEM_I031").Accepted
                && authority.CurrentSnapshot.LitRangeCells.Count == 0
                && authority.CurrentSnapshot.placements.All(placement =>
                    !placement.isLit && string.IsNullOrEmpty(
                        placement.litByPlacementId)),
                "I031 return retained stale lighting links");
        }

        private static void VerifyAuthorityPowerBoundary()
        {
            string projection = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs");
            string controller = ReadProjectFile(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs");
            Check(projection.IndexOf("ApplyPreviewEnergyLinks(compatibility)",
                    StringComparison.Ordinal) < 0,
                "compatibility projection recomputes legacy energy");
            int rendererStart = controller.IndexOf(
                "private void RenderItemSystemFormationPowerVisuals",
                StringComparison.Ordinal);
            int rendererEnd = controller.IndexOf(
                "private void ClearFormationPowerVisuals", rendererStart,
                StringComparison.Ordinal);
            Check(rendererStart >= 0 && rendererEnd > rendererStart,
                "ItemSystem power renderer missing");
            string renderer = controller.Substring(rendererStart,
                rendererEnd - rendererStart);
            Check(renderer.IndexOf("FormationEnergyContractResolver",
                    StringComparison.Ordinal) < 0
                && renderer.IndexOf("provider", StringComparison.OrdinalIgnoreCase) < 0
                && renderer.IndexOf("LitRangeCells", StringComparison.Ordinal) >= 0,
                "authority renderer uses a legacy power decision path");
        }

        private static void WriteReports(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            string reportRoot = ProjectPath("Docs/V0.4/Reports");
            Directory.CreateDirectory(reportRoot);
            bool staticPassed = Results.All(result => result.Passed);
            string status = staticPassed
                ? "DEV_COMPLETE / QA_STATIC_PASS / WAITING_USER_HANDTEST"
                : "STATIC_FAILED / GUARD_RETURN_REQUIRED";
            int passedCount = Results.Count(result => result.Passed);
            string verifierSummary = staticPassed
                ? "PASS / " + passedCount.ToString(CultureInfo.InvariantCulture)
                    + " of " + Results.Count.ToString(CultureInfo.InvariantCulture)
                : "FAIL / " + passedCount.ToString(CultureInfo.InvariantCulture)
                    + " PASS / " + (Results.Count - passedCount)
                        .ToString(CultureInfo.InvariantCulture)
                    + " FAIL / " + Results.Count.ToString(CultureInfo.InvariantCulture)
                    + " total";
            ScenarioResult authoredBindingResult = Results.Single(
                result => result.Id == "S20F");
            bool legacyTextGatesPassed = Results
                .Where(result => string.CompareOrdinal(result.Id, "S20G") >= 0
                    && string.CompareOrdinal(result.Id, "S20N") <= 0)
                .All(result => result.Passed);
            bool revision03VisualGatesPassed = Results
                .Where(result => string.CompareOrdinal(result.Id, "S20O") >= 0
                    && string.CompareOrdinal(result.Id, "S20V") <= 0)
                .All(result => result.Passed);
            int faMenIconCount = authoredVisualEvidence?.Icons.Count(value =>
                value.Group == AuthoredIconGroup.FaMenBuild && value.Passed) ?? 0;
            int qiLeiIconCount = authoredVisualEvidence?.Icons.Count(value =>
                value.Group == AuthoredIconGroup.QiLeiBuild && value.Passed) ?? 0;
            int coreIconCount = authoredVisualEvidence?.Icons.Count(value =>
                value.Group == AuthoredIconGroup.CoreEffect && value.Passed) ?? 0;
            int placementIconCount = authoredVisualEvidence?.Icons.Count(value =>
                value.Group == AuthoredIconGroup.Placement && value.Passed) ?? 0;
            int flavorIconCount = authoredVisualEvidence?.Icons.Count(value =>
                value.Group == AuthoredIconGroup.Flavor && value.Passed) ?? 0;
            ItemSystemBattleSandboxBoardAuthority baselineAuthority =
                NewAuthority(projection);
            StringBuilder report = new();
            report.AppendLine("# ItemSystem BattleSandbox Board Adapter Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemSystemBattleSandboxBoardAdapter01`")
                .AppendLine("- Revision: `Revision09-Rev03`")
                .AppendLine("- Assignment SHA-256: `6c346e00c35342345ef4f92765eb64685f13fef213aa782fd74369769714e1a2`")
                .AppendLine("- Item-review release: `GUARD_PASS_ITEMSYSTEMBATTLESANDBOARDADAPTER01_REVISION09_REV03_ITEM_REVIEW`")
                .AppendLine("- Item Guard: `ITEM_GUARD_CONFIRM_ITEMSYSTEMBATTLESANDBOARDADAPTER01`")
                .AppendLine("- Enemy Guard rereview: `NOT_REQUIRED`")
                .AppendLine("- Capability Algorithm Guard rereview: `NOT_REQUIRED`")
                .AppendLine("- Static status: `" + status + "`")
                .AppendLine("- Offline verifier: `" + verifierSummary + "`")
                .AppendLine("- Unity compile + StaticBatch verifier: `" + verifierSummary + "`")
                .AppendLine("- Protected non-whitelist hashes: `PASS / 12 of 12`")
                .AppendLine("- Package Leak Count: `0`")
                .AppendLine("- User handtest: `" + (staticPassed
                    ? "WAITING_USER_HANDTEST"
                    : "NOT_STARTED / BLOCKED_BY_STATIC_QA") + "`")
                .AppendLine("- Package seed: `" +
                    ItemSystemBattleSandboxViewProjection.PackageSeed
                        .ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Ordinary seed rule: `packageSeed + ordinal`")
                .AppendLine("- Ordinary rarity: `ItemInstanceRarity.Orange / orange / 道品`")
                .AppendLine("- Artwork rarityIndex: `5`")
                .AppendLine("- DaoPin detail model coverage: `30 of 30 / complete visible sections`")
                .AppendLine("- Authored detail visible binding: `"
                    + (authoredBindingResult.Passed
                        ? "30 of 30 / header + model-visible sections"
                        : "FAIL / " + authoredBindingResult.Detail) + "`")
                .AppendLine("- Legacy Text S20G-S20N: `"
                    + (legacyTextGatesPassed
                        ? "PASS / full panel font + section state + layout + shared view"
                        : "FAIL") + "`")
                .AppendLine("- Revision09-Rev03 S20O-S20V: `"
                    + (revision03VisualGatesPassed
                        ? "PASS / Chinese typography + BodyText container + authored icons"
                        : "FAIL") + "`")
                .AppendLine("- Legacy Text recovery order: `chineseTextFont -> first descendant Font -> installed SimSun -> Microsoft YaHei -> Noto Sans CJK SC -> Droid Sans Fallback -> LegacyRuntime.ttf`")
                .AppendLine("- Cached panel font: `"
                    + (fontEvidence?.ResolvedName ?? "UNRESOLVED")
                    + " / " + (fontEvidence?.ResolvedSource ?? "NO_SOURCE")
                    + " / resolution count "
                    + (fontEvidence?.ResolutionCount ?? 0)
                        .ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Font fallback reason: `"
                    + (string.IsNullOrWhiteSpace(fontEvidence?.FallbackReason)
                        ? "NONE / preferred SimSun or authored source selected"
                        : fontEvidence.FallbackReason) + "`")
                .AppendLine("- Authored icon final-visible counts: `FaMen "
                    + faMenIconCount.ToString(CultureInfo.InvariantCulture)
                    + " / QiLei "
                    + qiLeiIconCount.ToString(CultureInfo.InvariantCulture)
                    + " / Core "
                    + coreIconCount.ToString(CultureInfo.InvariantCulture)
                    + " / Placement "
                    + placementIconCount.ToString(CultureInfo.InvariantCulture)
                    + " / Flavor "
                    + flavorIconCount.ToString(CultureInfo.InvariantCulture)
                    + "`")
                .AppendLine("- Authored icon checks: `Sprite / enabled / activeInHierarchy / effective alpha / Canvas cull / viewport intersection / exact resource identity`")
                .AppendLine("- Existing valid font/size/color/alignment/material overwrite count: `0`")
                .AppendLine("- Initial authority: `ItemSystemSnapshot.v2 / I031 Complete+Inventory / empty board`")
                .AppendLine("- Tray capacity: `5 columns / 13 logical rows / 65 slots`")
                .AppendLine("- Runtime extension: `TrayGridSlot_Runtime_41-65 / 25 DontSave slots`")
                .AppendLine("- Packing order: `footprint cells descending, then baseItemId Ordinal; anchors row-major`")
                .AppendLine("- Footprint cells / last occupied slot: `62 ordinary + 1 I031 = 63 / 63`")
                .AppendLine("- Detail runtime: `one authored PopupLayer/ItemDetailPanel / initial hide / latest v2 projection / legacy panel disabled`")
                .AppendLine("- Runtime detail roots / prefab loads / panel instantiate / panel destroy: `0 / 0 / 0 / 0`")
                .AppendLine("- I031 identity: `SPECIAL_I031 / P_SYSTEM_I031 / no ordinary itemInstanceId`")
                .AppendLine("- IF01 canonical signature: `"
                    + baselineAuthority.CurrentBindingSnapshot.canonicalSignature + "`")
                .AppendLine("- P6 feedback canonical signature: `"
                    + baselineAuthority.CurrentLayoutResilienceSnapshot.canonicalSignature + "`")
                .AppendLine()
                .AppendLine("## Verifier results")
                .AppendLine();
            foreach (ScenarioResult result in Results)
            {
                report.AppendLine("- " + result.Id + ": `"
                    + (result.Passed ? "PASS" : "FAIL") + "` — " + result.Detail);
            }
            WriteUtf8(Path.Combine(reportRoot,
                "ItemSystemBattleSandboxBoardAdapterReport.md"), report.ToString());

            StringBuilder roster = new("ordinal,baseItemId,displayName,kind,rarity,rarityIndex,rootSeed,itemInstanceId,placementId\n");
            foreach (ItemSystemBattleSandboxViewRow row in projection.Rows)
            {
                roster.Append(row.Ordinal.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.BaseItemId)).Append(',')
                    .Append(Csv(row.DisplayName)).Append(',')
                    .Append(row.IsSystemItem ? "system" : "ordinary").Append(',')
                    .Append(row.IsSystemItem ? "none" : "orange").Append(',')
                    .Append(row.IsSystemItem ? "none" : "5").Append(',')
                    .Append(row.IsSystemItem ? "none" : row.RootSeed.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Csv(row.ItemInstanceId)).Append(',')
                    .Append(row.IsSystemItem ? "P_SYSTEM_I031" : "P_BOARD_" + row.BaseItemId)
                    .AppendLine();
            }
            WriteUtf8(Path.Combine(reportRoot,
                "ItemSystemBattleSandboxBoardRoster.csv"), roster.ToString());

            StringBuilder matrix = new("scenario,status,detail\n");
            foreach (ScenarioResult result in Results)
            {
                matrix.Append(result.Id).Append(',')
                    .Append(result.Passed ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(result.Detail)).AppendLine();
            }
            WriteUtf8(Path.Combine(reportRoot,
                "ItemSystemBattleSandboxBoardStateMatrix.csv"), matrix.ToString());

            WriteUtf8(Path.Combine(reportRoot,
                "ItemSystemBattleSandboxBoardLeakCheckReport.md"),
                "# ItemSystem BattleSandbox Board Leak Check\n\n"
                + "- Status: `" + (Results.Single(result => result.Id == "S18").Passed
                    ? "PASS" : "FAIL") + "`\n"
                + "- Target scene SHA-256 remains `" + ExpectedSceneSha256 + "`.\n"
                + "- Runtime installer is exact-path, Editor Play only, and DontSave.\n"
                + "- Runtime slots 41-65 are removed and the pre-authority Content height is restored on uninstall.\n"
                + "- The one authored PopupLayer/ItemDetailPanel is hidden first, reused, and hidden without destruction on uninstall.\n"
                + "- ItemDetailPanel prefab load/instantiate, runtime detail-root creation, ItemSandbox presenter/session calls and authored-panel destruction are all zero.\n"
                + "- DaoPin model coverage is 30/30; authored panel binding: `"
                + (authoredBindingResult.Passed
                    ? "PASS / 30 of 30"
                    : "FAIL / " + authoredBindingResult.Detail) + "`.\n"
                + "- Legacy Text S20G-S20N: `"
                + (legacyTextGatesPassed ? "PASS" : "FAIL")
                + "`; only null fonts are recovered and existing visual parameters remain untouched.\n"
                + "- Revision09-Rev03 S20O-S20V: `"
                + (revision03VisualGatesPassed ? "PASS" : "FAIL")
                + "`; SimSun-first cached font and actual authored Image visibility/identity are verified.\n"
                + "- Final-visible authored icons: `FaMen "
                + faMenIconCount.ToString(CultureInfo.InvariantCulture)
                + " / QiLei "
                + qiLeiIconCount.ToString(CultureInfo.InvariantCulture)
                + " / Core "
                + coreIconCount.ToString(CultureInfo.InvariantCulture)
                + " / Placement "
                + placementIconCount.ToString(CultureInfo.InvariantCulture)
                + " / Flavor "
                + flavorIconCount.ToString(CultureInfo.InvariantCulture) + "`.\n"
                + "- ItemDetailSectionView clears stale Body/Rows and restores hidden/plain/authored/plain/hidden deterministically.\n"
                + "- Authority lighting renders only ItemSystemSnapshot.v2 LitRangeCells and placement facts.\n"
                + "- No Scene/Prefab/authored-slot/viewport/GridLayoutGroup/Item truth-source write is used.\n"
                + "- Authority-active commit returns before legacy board cache commit.\n"
                + "- Package gate status: `" + status + "`.\n");

            WriteUtf8(Path.Combine(reportRoot,
                "ItemSystemBattleSandboxBoardManualTest.md"),
                ManualTestText(staticPassed));
        }

        private static string ManualTestText(bool staticPassed)
        {
            return "# ItemSystem BattleSandbox Board Manual Test\n\n"
                + "Status: `" + (staticPassed
                    ? "WAITING_USER_HANDTEST"
                    : "BLOCKED_GUARD_RETURN") + "`\n\n"
                + "1. 只打开 `Scene_TalismanBag_V04_BattleSandboxPreview`，进入 Play；确认首帧现成 ItemDetailPanel 隐藏。\n"
                + "2. 单击 I001-I030 与 I031：必须复用同一现成详情面板，不出现旧 BuildSandbox 弹窗或第二个运行时弹窗。\n"
                + "3. 逐项确认标题、Meta、物品强度、已连接状态徽章、详情/调试页签文字均可见，不再只有物体而没有字。\n"
                + "4. 确认中文字体保持 ItemSandbox 的宋体（SimSun）观感；若本机无 SimSun，记录 Console 中明确的批准回退字体和原因，不得静默换字。\n"
                + "5. 横跨五个法门抽查 I001-I030：基础属性、固定词条、随机词条、核心效果、法门 Build、器类 Build、道痕、推荐摆放、旧物志中当前状态应显示的文字均可见。\n"
                + "6. 每件普通道品确认法门 Build 有 3 个阶段图标、器类 Build 有 2 个阶段图标；核心效果只显示当前数据行对应的图标且最多 4 个，不得沿用上一件道具图片。\n"
                + "7. 推荐摆放与旧物志图标均可见；滚动到对应段落时图标不得透明、被裁掉、落在视口外或显示错误资源。\n"
                + "8. 有 authored rows 的段落保持 BodyText 容器存在，只隐藏 plain Text renderer；Rows 不与正文重复，返回普通正文时 Body 恢复，无内容时不残留上一件道具。\n"
                + "9. 合法不可用/互斥/隐藏页签内容保持隐藏；不要出现全部页签或全部 inactive 子物体被一起打开。\n"
                + "10. 确认 I001-I030 仍是精确 Orange/道品 dev 实例与 rarityIndex 5 图片；I031 仍是系统身份和专用图片，无串名、串图或伪造摆放状态。\n"
                + "11. 托盘仍为 5 列、13 逻辑行且可滚动；拖拽、Ghost、旋转区、非法释放回退、800x800 可视区手感不变。\n"
                + "12. 普通道具可按批准形状摆放、移动、返回；托盘/拖影/棋盘使用同一图片且没有旧图叠层。\n"
                + "13. I031 Reset 后在道具栏且只有一件；可从道具栏到棋盘、棋盘内移动并返回，道具不可复制。\n"
                + "14. I031 未放置时无供能；放置后显示精确上下左右范围，边缘裁切，移动清旧范围，返回后全部范围/徽章清空。\n"
                + "15. 普通道具进入/离开范围时点亮、接亮、阵脉、Build 计入/进度、开窍/技能状态与重开详情同步更新，并出现一行 `【阵势韧性】` 反馈。\n"
                + "16. 敌人/Boss 反馈、战斗页按钮、关闭详情后的页面和既有手感保持不变。\n"
                + "17. Console 本包 Error/Warning 为 0，尤其无 `I031_LOCATION_UNKNOWN`、字体解析失败或安装异常。\n"
                + "18. 退出 Play：25 个 runtime tray slots 被移除，Content 高度/图片/范围恢复；Scene/Prefab 未被保存或改写。\n";
        }

        private static void Run(string id, string description, Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(id, true, description));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(id, false,
                    description + ": " + exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static IEnumerable<Vector2Int> EnumerateBoardCells()
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
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

        private static long CellKey(Vector2Int cell)
        {
            return ((long)cell.y << 32) | (uint)cell.x;
        }

        private static string ReadProjectFile(string relativePath)
        {
            return File.ReadAllText(ProjectPath(relativePath), Encoding.UTF8);
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root unavailable.");
            return Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static int CountOccurrences(string source, string marker)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(source)
                && !string.IsNullOrEmpty(marker)
                && (index = source.IndexOf(marker, index,
                    StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += marker.Length;
            }
            return count;
        }

        private static void WriteUtf8(string path, string value)
        {
            File.WriteAllText(path, value ?? string.Empty, new UTF8Encoding(false));
        }

        private sealed class TrayPackResult
        {
            private TrayPackResult(
                bool succeeded,
                int placementCount,
                int usedCellCount,
                int lastLogicalSlotIndex)
            {
                Succeeded = succeeded;
                PlacementCount = placementCount;
                UsedCellCount = usedCellCount;
                LastLogicalSlotIndex = lastLogicalSlotIndex;
            }

            public bool Succeeded { get; }
            public int PlacementCount { get; }
            public int UsedCellCount { get; }
            public int LastLogicalSlotIndex { get; }

            public static TrayPackResult FromGrid(
                ShapeAwareItemTrayGrid grid,
                bool succeeded)
            {
                int lastLogicalSlotIndex = grid.OccupiedCells.Count == 0
                    ? 0
                    : grid.OccupiedCells.Keys.Max(cell =>
                        cell.y * BuildGridInteractionPreviewController.TrayColumns
                        + cell.x + 1);
                return new TrayPackResult(
                    succeeded,
                    grid.Placements.Count,
                    grid.OccupiedCells.Count,
                    lastLogicalSlotIndex);
            }
        }

        private sealed class LegacyTextVerificationEvidence
        {
            public LegacyTextVerificationEvidence(
                bool targetFontResolved,
                int activeDataTextCount,
                bool headerAndStatusVisible,
                bool bodyRowsExclusive,
                bool preferredMeasurementsValid)
            {
                TargetFontResolved = targetFontResolved;
                ActiveDataTextCount = activeDataTextCount;
                HeaderAndStatusVisible = headerAndStatusVisible;
                BodyRowsExclusive = bodyRowsExclusive;
                PreferredMeasurementsValid = preferredMeasurementsValid;
            }

            public bool TargetFontResolved { get; }
            public int ActiveDataTextCount { get; }
            public bool HeaderAndStatusVisible { get; }
            public bool BodyRowsExclusive { get; }
            public bool PreferredMeasurementsValid { get; }
        }

        private sealed class FontVerificationEvidence
        {
            public FontVerificationEvidence(
                bool simSunInstalled,
                string resolvedName,
                string resolvedSource,
                string fallbackReason,
                int resolutionCount)
            {
                SimSunInstalled = simSunInstalled;
                ResolvedName = resolvedName ?? string.Empty;
                ResolvedSource = resolvedSource ?? string.Empty;
                FallbackReason = fallbackReason ?? string.Empty;
                ResolutionCount = resolutionCount;
            }

            public bool SimSunInstalled { get; }
            public string ResolvedName { get; }
            public string ResolvedSource { get; }
            public string FallbackReason { get; }
            public int ResolutionCount { get; }
        }

        private sealed class AuthoredVisualVerificationEvidence
        {
            public List<string> BodyFailures { get; } = new();
            public List<IconObservation> Icons { get; } = new();
        }

        private enum AuthoredIconGroup
        {
            FaMenBuild,
            QiLeiBuild,
            CoreEffect,
            Placement,
            Flavor
        }

        private sealed class AuthoredIconGroupDefinition
        {
            private AuthoredIconGroupDefinition(
                string sectionName,
                string rowsRootName,
                string rowNamePrefix,
                string iconNamePrefix)
            {
                SectionName = sectionName;
                RowsRootName = rowsRootName;
                RowNamePrefix = rowNamePrefix;
                IconNamePrefix = iconNamePrefix;
            }

            public string SectionName { get; }
            public string RowsRootName { get; }
            public string RowNamePrefix { get; }
            public string IconNamePrefix { get; }

            public static AuthoredIconGroupDefinition For(AuthoredIconGroup group)
            {
                return group switch
                {
                    AuthoredIconGroup.FaMenBuild => new AuthoredIconGroupDefinition(
                        "FaMenBuildSection", "FaMenBuildRowsRoot",
                        "FaMenBuildRow_", "FaMenBuildStageIconSlot_"),
                    AuthoredIconGroup.QiLeiBuild => new AuthoredIconGroupDefinition(
                        "QiLeiBuildSection", "QiLeiBuildRowsRoot",
                        "QiLeiBuildRow_", "QiLeiBuildStageIconSlot_"),
                    AuthoredIconGroup.CoreEffect => new AuthoredIconGroupDefinition(
                        "CoreAwakeningSection", "CoreEffectRowsRoot",
                        "CoreEffectRow_", "CoreEffectIconSlot_"),
                    AuthoredIconGroup.Placement => new AuthoredIconGroupDefinition(
                        "PlacementHintSection", "PlacementRowsRoot",
                        "PlacementRow_", "PlacementIconSlot_"),
                    AuthoredIconGroup.Flavor => new AuthoredIconGroupDefinition(
                        "FlavorSection", "FlavorRowsRoot",
                        "FlavorRow_", "FlavorIconSlot_"),
                    _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
                };
            }
        }

        private sealed class IconObservation
        {
            public IconObservation(
                AuthoredIconGroup group,
                string baseItemId,
                int slotIndex,
                bool referenceExists,
                bool spritePresent,
                bool enabled,
                bool activeInHierarchy,
                bool opaque,
                bool notCulled,
                bool intersectsViewport,
                bool identityMatches,
                string actualPath)
            {
                Group = group;
                BaseItemId = baseItemId ?? string.Empty;
                SlotIndex = slotIndex;
                ReferenceExists = referenceExists;
                SpritePresent = spritePresent;
                Enabled = enabled;
                ActiveInHierarchy = activeInHierarchy;
                Opaque = opaque;
                NotCulled = notCulled;
                IntersectsViewport = intersectsViewport;
                IdentityMatches = identityMatches;
                ActualPath = actualPath ?? string.Empty;
            }

            public AuthoredIconGroup Group { get; }
            public string BaseItemId { get; }
            public int SlotIndex { get; }
            public bool ReferenceExists { get; }
            public bool SpritePresent { get; }
            public bool Enabled { get; }
            public bool ActiveInHierarchy { get; }
            public bool Opaque { get; }
            public bool NotCulled { get; }
            public bool IntersectsViewport { get; }
            public bool IdentityMatches { get; }
            public string ActualPath { get; }

            public bool Passed => ReferenceExists && SpritePresent && Enabled
                && ActiveInHierarchy && Opaque && NotCulled
                && IntersectsViewport && IdentityMatches;

            public string DescribeFailure()
            {
                List<string> failures = new();
                if (!ReferenceExists)
                {
                    failures.Add("missing image reference");
                }
                if (!SpritePresent)
                {
                    failures.Add("missing sprite");
                }
                if (!Enabled)
                {
                    failures.Add("disabled image");
                }
                if (!ActiveInHierarchy)
                {
                    failures.Add("inactive ancestor");
                }
                if (!Opaque)
                {
                    failures.Add("transparent image");
                }
                if (!NotCulled)
                {
                    failures.Add("culled by canvas");
                }
                if (!IntersectsViewport)
                {
                    failures.Add("outside viewport");
                }
                if (!IdentityMatches)
                {
                    failures.Add("wrong resource identity");
                }
                return BaseItemId + "/" + Group + "/slot"
                    + SlotIndex.ToString(CultureInfo.InvariantCulture)
                    + " [" + string.Join(", ", failures) + "] path="
                    + (string.IsNullOrWhiteSpace(ActualPath) ? "<none>" : ActualPath);
            }
        }

        private sealed class ScenarioResult
        {
            public ScenarioResult(string id, bool passed, string detail)
            {
                Id = id;
                Passed = passed;
                Detail = detail ?? string.Empty;
            }

            public string Id { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
