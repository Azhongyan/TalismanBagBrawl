using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemSystemBattleSandboxItemDetailAdapter
    {
        private readonly Dictionary<string, ItemSystemBattleSandboxViewRow> rowsById =
            new(StringComparer.Ordinal);

        private ItemDetailPanelView panelView;
        private string currentBaseItemId = string.Empty;
        private string currentItemInstanceId = string.Empty;
        private string currentPlacementId = string.Empty;
        private int visibleSinceFrame = -1;

        public bool IsInitialized => panelView != null;
        public bool IsVisible => IsInitialized && panelView.gameObject.activeSelf;
        public string CurrentBaseItemId => currentBaseItemId;
        public string CurrentItemInstanceId => currentItemInstanceId;
        public string CurrentPlacementId => currentPlacementId;
        public int VisibleSinceFrame => visibleSinceFrame;
        public string LastDiagnosticCode { get; private set; } = string.Empty;
        public int ShowGeneration { get; private set; }
        public ItemDetailViewModel LastProjectedModel { get; private set; }
        public Sprite LastBoundSprite { get; private set; }
        public GameObject AuthoredPanelInstance => panelView != null
            ? panelView.gameObject
            : null;

        public bool Initialize(
            ItemDetailPanelView authoredScenePanel,
            IReadOnlyList<ItemSystemBattleSandboxViewRow> rows)
        {
            if (IsInitialized)
            {
                return Fail("DETAIL_ALREADY_INITIALIZED");
            }
            if (!IsExactAuthoredScenePanel(authoredScenePanel))
            {
                return Fail("DETAIL_AUTHORED_PANEL_INVALID");
            }

            authoredScenePanel.SetPreserveAuthoredVisualStyle(true);
            authoredScenePanel.SetVisible(false);

            ItemSystemBattleSandboxViewRow[] ordered = (rows ??
                    Array.Empty<ItemSystemBattleSandboxViewRow>())
                .Where(row => row != null)
                .OrderBy(row => row.Ordinal)
                .ToArray();
            string[] expected = Enumerable.Range(1, 31)
                .Select(index => "I" + index.ToString("000",
                    CultureInfo.InvariantCulture))
                .ToArray();
            if (ordered.Length != expected.Length
                || !ordered.Select(row => row.BaseItemId)
                    .SequenceEqual(expected, StringComparer.Ordinal)
                || ordered.Any(row => row.CreateBaseDetailModelClone() == null
                    || row.ResolveSprite(false) == null
                    || (row.IsSystemItem && row.ResolveSprite(true) == null)))
            {
                return Fail("DETAIL_ROW_DEPENDENCY_INVALID");
            }

            try
            {
                rowsById.Clear();
                foreach (ItemSystemBattleSandboxViewRow row in ordered)
                {
                    rowsById.Add(row.BaseItemId, row);
                }
                panelView = authoredScenePanel;
                LastDiagnosticCode = "NONE";
                return true;
            }
            catch
            {
                authoredScenePanel.SetVisible(false);
                rowsById.Clear();
                panelView = null;
                return Fail("DETAIL_INSTALL_EXCEPTION");
            }
        }

        public bool Show(
            string baseItemId,
            string placementId,
            ItemSystemSnapshot authoritativeSnapshot)
        {
            return FailAndHide("DETAIL_QUALIFIED_CONTEXT_REQUIRED");
        }

        public bool Show(
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot authoritativeSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot authoritativeQualifiedBuildState)
        {
            return FailAndHide("DETAIL_CORE_RUNTIME_CONTEXT_REQUIRED");
        }

        public bool Show(
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot authoritativeSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot authoritativeQualifiedBuildState,
            ItemInstanceCoreEffectRuntimeStateSnapshot authoritativeCoreEffectRuntimeState)
        {
            if (!IsInitialized)
            {
                return FailAndHide("DETAIL_NOT_INITIALIZED");
            }
            if (!IsCompleteV2Snapshot(authoritativeSnapshot))
            {
                return FailAndHide("DETAIL_SNAPSHOT_INVALID");
            }
            if (string.IsNullOrWhiteSpace(baseItemId)
                || !rowsById.TryGetValue(baseItemId,
                    out ItemSystemBattleSandboxViewRow row)
                || row == null)
            {
                return FailAndHide("DETAIL_BASE_ITEM_UNKNOWN");
            }
            string exactItemInstanceId = itemInstanceId ?? string.Empty;
            if (row.IsSystemItem
                ? exactItemInstanceId.Length != 0
                : !string.Equals(row.ItemInstanceId, exactItemInstanceId,
                    StringComparison.Ordinal))
            {
                return FailAndHide("DETAIL_ITEM_INSTANCE_IDENTITY_INVALID");
            }

            string exactPlacementId = placementId ?? string.Empty;
            ItemSystemPlacementSnapshot[] basePlacements =
                authoritativeSnapshot.placements
                    .Where(value => value != null && string.Equals(
                        value.itemId, baseItemId, StringComparison.Ordinal))
                    .ToArray();
            ItemSystemPlacementSnapshot placement = null;
            if (exactPlacementId.Length == 0)
            {
                if (basePlacements.Length != 0
                    || (row.IsSystemItem
                        && authoritativeSnapshot.i031State.location
                            != I031Location.Inventory))
                {
                    return FailAndHide("DETAIL_TRAY_IDENTITY_INVALID");
                }
            }
            else
            {
                ItemSystemPlacementSnapshot[] exact = basePlacements
                    .Where(value => string.Equals(value.placementId,
                        exactPlacementId, StringComparison.Ordinal))
                    .ToArray();
                if (basePlacements.Length != 1 || exact.Length != 1
                    || (row.IsSystemItem
                        && (authoritativeSnapshot.i031State.location
                                != I031Location.Board
                            || !string.Equals(exactPlacementId,
                                I031InventoryPlacementContract.StablePlacementId,
                                StringComparison.Ordinal))))
                {
                    return FailAndHide("DETAIL_PLACEMENT_IDENTITY_INVALID");
                }
                placement = exact[0];
            }

            ItemDetailViewModel baseModel = row.CreateBaseDetailModelClone();
            Sprite sprite = row.ResolveSprite(placement?.isLit == true);
            if (baseModel == null || sprite == null)
            {
                return FailAndHide("DETAIL_RUNTIME_DEPENDENCY_MISSING");
            }

            ItemDetailViewModel projected;
            try
            {
                projected = ItemDetailProjectionComposer.Compose(
                    baseModel,
                    exactPlacementId.Length == 0
                        ? ItemDetailProjectionContextKind.CatalogPreview
                        : ItemDetailProjectionContextKind.PlacedInstance,
                    exactPlacementId,
                    authoritativeSnapshot.ToLightingResolutionResult(),
                    authoritativeSnapshot.ToArrayBonusResolutionResult(),
                    authoritativeSnapshot.ToBuildSynergyResolutionResult(),
                    authoritativeSnapshot.ToCoreAwakeningResolutionResult(),
                    authoritativeSnapshot.ToSkillMonitorResolutionResult());
            }
            catch
            {
                return FailAndHide("DETAIL_PROJECTION_EXCEPTION");
            }

            if (projected == null
                || !string.Equals(projected.placementId, exactPlacementId,
                    StringComparison.Ordinal))
            {
                return FailAndHide("DETAIL_PROJECTION_INVALID");
            }

            ItemDetailQualifiedBuildTrackProjectionResult qualifiedProjection =
                ItemDetailQualifiedBuildTrackProjector.Project(
                    projected,
                    row.BaseItemId,
                    exactItemInstanceId,
                    exactPlacementId,
                    authoritativeSnapshot,
                    authoritativeQualifiedBuildState);
            if (qualifiedProjection?.IsSuccess != true
                || qualifiedProjection.ViewModel == null)
            {
                return FailAndHide(qualifiedProjection?.DiagnosticCode
                    ?? "DETAIL_QUALIFIED_PROJECTION_INVALID");
            }

            ItemDetailQualifiedCoreEffectProjectionResult coreProjection =
                ItemDetailQualifiedCoreEffectProjector.Project(
                    qualifiedProjection.ViewModel,
                    row.BaseItemId,
                    exactItemInstanceId,
                    exactPlacementId,
                    authoritativeSnapshot,
                    authoritativeCoreEffectRuntimeState);
            if (coreProjection?.IsSuccess != true
                || coreProjection.ViewModel == null)
            {
                return FailAndHide(coreProjection?.DiagnosticCode
                    ?? "DETAIL_QUALIFIED_CORE_PROJECTION_INVALID");
            }

            ItemDetailViewModel finalModel = coreProjection.ViewModel;
            try
            {
                panelView.Bind(finalModel, sprite);
                panelView.SetVisible(true);
                visibleSinceFrame = Time.frameCount;
            }
            catch
            {
                return FailAndHide("DETAIL_BIND_EXCEPTION");
            }
            currentBaseItemId = row.BaseItemId;
            currentItemInstanceId = exactItemInstanceId;
            currentPlacementId = exactPlacementId;
            LastProjectedModel = finalModel.Clone();
            LastBoundSprite = sprite;
            ShowGeneration++;
            LastDiagnosticCode = coreProjection.DiagnosticCode;
            return true;
        }

        public void Hide()
        {
            if (panelView != null)
            {
                panelView.SetVisible(false);
            }
            visibleSinceFrame = -1;
            currentBaseItemId = string.Empty;
            currentItemInstanceId = string.Empty;
            currentPlacementId = string.Empty;
            LastProjectedModel = null;
            LastBoundSprite = null;
        }

        public bool ContainsVisibleContentScreenPoint(
            Vector2 screenPosition,
            Camera eventCamera = null)
        {
            return panelView != null
                && panelView.gameObject.activeInHierarchy
                && panelView.ContainsPopupContentScreenPoint(screenPosition, eventCamera);
        }

        public void Uninstall()
        {
            Hide();
            panelView = null;
            rowsById.Clear();
            LastDiagnosticCode = string.Empty;
        }

        private bool Fail(string code)
        {
            LastDiagnosticCode = code ?? "DETAIL_INVALID";
            return false;
        }

        private bool FailAndHide(string code)
        {
            Hide();
            return Fail(code);
        }

        private static bool IsCompleteV2Snapshot(ItemSystemSnapshot snapshot)
        {
            return snapshot != null
                && snapshot.isValid
                && string.Equals(snapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal)
                && snapshot.i031State != null
                && snapshot.i031State.ownershipCompleteness
                    == I031OwnershipCompleteness.Complete
                && snapshot.i031State.isOwned
                && snapshot.i031State.location != I031Location.Unknown;
        }

        private static bool IsExactAuthoredScenePanel(
            ItemDetailPanelView authoredScenePanel)
        {
            if (authoredScenePanel == null
                || !string.Equals(authoredScenePanel.gameObject.name,
                    "ItemDetailPanel", StringComparison.Ordinal))
            {
                return false;
            }

            Transform panelTransform = authoredScenePanel.transform;
            Transform popupLayer = panelTransform.parent;
            if (popupLayer == null
                || !string.Equals(popupLayer.name, "PopupLayer",
                    StringComparison.Ordinal)
                || popupLayer.Cast<Transform>().Count(child => string.Equals(
                    child.name, "ItemDetailPanel", StringComparison.Ordinal)) != 1
                || panelTransform.GetComponents<ItemDetailPanelView>().Length != 1)
            {
                return false;
            }

            UnityEngine.SceneManagement.Scene scene = panelTransform.gameObject.scene;
            if (!scene.IsValid())
            {
                return false;
            }

            ItemDetailPanelView[] sceneViews = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ItemDetailPanelView>(true))
                .Where(view => view != null)
                .ToArray();
            return sceneViews.Length == 1
                && ReferenceEquals(sceneViews[0], authoredScenePanel);
        }
    }
}
