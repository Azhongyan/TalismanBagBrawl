using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ItemDetailLegacyBuildPresentationParityVerifier
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemDetailLegacyBuildPresentationParityReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemDetailLegacyBuildPresentationParitySpec.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemDetailLegacyBuildPresentationParityLeakCheckReport.md";
        private const string ManualPath =
            "Docs/V0.4/Reports/ItemDetailLegacyBuildPresentationParityManualTest.md";

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] =
                    "080beabdc537e29c31dc1e68bee6a9e7611aade8466e898c6e4c1fb4f106b448",
                [ScenePath] =
                    "be7e9536573cf7708f4d64961661c1fd60dd275f969e2b7a003a045b0d07306e",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] =
                    "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] =
                    "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] =
                    "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs"] =
                    "64b9d3e2e407dc014695b4f466c4c82803384b9b37dc919c435bd181e2c13910",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "a7957ffd262dc5231622e8762c644ebad1ccb5d0b662325a4fe9ec62d23d497f"
            };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<string> SampleEvidence = new();

        [MenuItem(
            "Talisman Bag/V0.4/Verify Item Detail Legacy Build Presentation Parity")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            SampleEvidence.Clear();
            Run("S01", "SHARED_PRESENTATION_SOURCE_PASS", VerifySharedSource);
            Run("S02", "PROTECTED_DOMAIN_HASH_PASS", VerifyProtectedHashes);
            Run("S03", "REAL_AUTHORED_TEXT_PASS", VerifyRealAuthoredText);
            Run("S04", "MACHINE_FACT_PLAYER_LEAK_PASS", VerifyMachineFactLeak);
            Run(
                "S05",
                "REAL_OUTSIDE_DISMISS_HIT_MATRIX_PASS",
                VerifyRealOutsideDismissHitMatrix);
            WriteReports();

            ScenarioResult[] failures = Results.Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemDetailLegacyBuildPresentationParity verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log("[ItemDetailLegacyBuildPresentationParityVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nREAL_RUNTIME_AUTHORED_TEXT_PASS"
                + "\nREAL_EVENTSYSTEM_OUTSIDE_DISMISS_PATH_PASS"
                + "\nUSER_HANDTEST_WAITING"
                + "\nSIMPLE_DIRECT_CLOSE");
        }

        private static void VerifySharedSource()
        {
            string projector = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/"
                + "ItemDetailQualifiedBuildTrackProjector.cs");
            string candidate = Read(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/"
                + "ItemBalanceCandidateDetailSandboxAdapter.cs");
            string workbench = Read(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/"
                + "ItemFullDetailBuildSandboxWorkbenchSession.cs");
            string names = Read(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/"
                + "ItemSandboxBuildPresentationNames.cs");
            string shared = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/"
                + "ItemBuildPlayerPresentationFormatter.cs");
            Check(projector.Contains(
                    "ItemBuildPlayerPresentationFormatter.FormatTrack",
                    StringComparison.Ordinal)
                && candidate.Contains(
                    "ItemBuildPlayerPresentationFormatter.FormatTrack",
                    StringComparison.Ordinal)
                && workbench.Contains(
                    "ItemBuildPlayerPresentationFormatter.FormatTrack",
                    StringComparison.Ordinal)
                && names.Contains(
                    "ItemBuildPlayerPresentationFormatter",
                    StringComparison.Ordinal),
                "new and legacy entrypoints do not share one formatter");
            Check(shared.Contains("九霄雷君的敕令", StringComparison.Ordinal)
                && shared.Contains("南离天君的焚章", StringComparison.Ordinal)
                && !names.Contains("九霄雷君的敕令", StringComparison.Ordinal)
                && !names.Contains("南离天君的焚章", StringComparison.Ordinal),
                "human-readable Build name mapping is duplicated");
        }

        private static void VerifyRealAuthoredText()
        {
            ItemSystemBattleSandboxViewProjectionResult projection = BuildProjection();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeScene = !scene.IsValid() || !scene.isLoaded;
            if (closeScene)
            {
                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Additive);
            }

            try
            {
                ItemDetailPanelView panel = FindAuthoredPanel(scene);
                Check(panel != null,
                    "BattleSandbox scene authored ItemDetailPanelView missing");
                ItemSystemBattleSandboxItemDetailAdapter adapter = new();
                try
                {
                    Check(adapter.Initialize(panel, projection.Rows),
                        "detail adapter initialize failed: "
                        + adapter.LastDiagnosticCode);
                    VerifyOrdinarySample(
                        adapter,
                        panel,
                        projection,
                        "I001",
                        ItemBuildQualification.QiLeiOnly,
                        null,
                        "qilei:fu");
                    VerifyOrdinarySample(
                        adapter,
                        panel,
                        projection,
                        "I004",
                        ItemBuildQualification.Dual,
                        "famen:zhenlei",
                        "qilei:ling");
                    VerifyOrdinarySample(
                        adapter,
                        panel,
                        projection,
                        "I006",
                        ItemBuildQualification.None,
                        null,
                        null);
                    VerifyOrdinarySample(
                        adapter,
                        panel,
                        projection,
                        "I009",
                        ItemBuildQualification.FaMenOnly,
                        "famen:lihuo",
                        null);
                    VerifyI031(adapter, panel, projection);
                    VerifyUnknownAndInvalid(adapter, panel, projection);
                    VerifyNoVisibleStaleText(adapter, panel, projection);
                }
                finally
                {
                    adapter.Uninstall();
                }
            }
            finally
            {
                if (closeScene && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void VerifyRealOutsideDismissHitMatrix()
        {
            ItemSystemBattleSandboxViewProjectionResult projection =
                BuildProjection();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeScene = !scene.IsValid() || !scene.isLoaded;
            if (closeScene)
            {
                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Additive);
            }

            try
            {
                ItemDetailPanelView panel = FindAuthoredPanel(scene);
                EventSystem eventSystem = FindSceneComponent<EventSystem>(scene);
                BuildItemTrayPreviewView tray =
                    FindSceneComponent<BuildItemTrayPreviewView>(scene);
                BuildGridInteractionPreviewController controller =
                    FindSceneComponent<BuildGridInteractionPreviewController>(
                        scene);
                Check(panel != null
                    && eventSystem != null
                    && tray != null
                    && controller != null,
                    "authored panel/EventSystem/tray/controller path missing");

                ScrollRect trayScroll = tray.GetComponent<ScrollRect>();
                Check(trayScroll != null,
                    "authored tray ScrollRect missing");

                Canvas authoredPopupCanvas =
                    panel.GetComponentInParent<Canvas>();
                Canvas authoredRootCanvas = authoredPopupCanvas != null
                    ? authoredPopupCanvas.rootCanvas
                    : null;
                string authoredPopupMode = authoredPopupCanvas != null
                    ? authoredPopupCanvas.renderMode.ToString()
                    : "null";
                string authoredRootMode = authoredRootCanvas != null
                    ? authoredRootCanvas.renderMode.ToString()
                    : "null";
                string authoredEventCamera = authoredRootCanvas != null
                    && authoredRootCanvas.renderMode
                        != RenderMode.ScreenSpaceOverlay
                    && authoredRootCanvas.worldCamera != null
                        ? authoredRootCanvas.worldCamera.name
                        : "null";

                MethodInfo panelAwake =
                    typeof(ItemDetailPanelView).GetMethod(
                        "Awake",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Check(panelAwake != null,
                    "detail panel runtime Awake lifecycle missing");
                panelAwake.Invoke(panel, null);

                EventSystemRaycastHarness raycastHarness =
                    new(scene, panel, trayScroll);
                ItemSystemBattleSandboxItemDetailAdapter adapter = new();
                try
                {
                    Check(adapter.Initialize(panel, projection.Rows),
                        "detail adapter initialize failed: "
                        + adapter.LastDiagnosticCode);
                    raycastHarness.Refresh();

                    Check(TryFindBaselineRaycastAtTrayGeometry(
                            eventSystem,
                            tray,
                            out Vector2 trayPoint,
                            out RaycastResult baselineTopHit,
                            out RaycastResult trayHitBeforeOpen,
                            out string trayBaselineDiagnostic),
                        "closed detail did not expose real tray raycast geometry; "
                        + trayBaselineDiagnostic);

                    ItemSystemBattleSandboxBoardAuthority authority =
                        NewAuthority(projection);
                    ItemSystemBattleSandboxViewRow row =
                        FindRow(projection, "I004");
                    Show(adapter, row, string.Empty, authority);
                    raycastHarness.Refresh();

                    ScrollRect detailScroll = panel
                        .GetComponentsInChildren<ScrollRect>(true)
                        .FirstOrDefault(value => value != null
                            && string.Equals(
                                value.gameObject.name,
                                "ItemDetailScrollView",
                                StringComparison.Ordinal));
                    Check(detailScroll != null
                        && detailScroll.viewport != null
                        && detailScroll.content != null
                        && detailScroll.content.rect.height
                            > detailScroll.viewport.rect.height + 0.5f,
                        "I004 detail is not a real long ScrollRect");

                    Image popupFrame = panel.GetComponent<Image>();
                    Check(popupFrame != null
                        && popupFrame.raycastTarget
                        && ReferenceEquals(
                            popupFrame.rectTransform,
                            panel.transform as RectTransform),
                        "visible ItemDetail outer frame is not panel root Image");

                    Canvas popupCanvas =
                        panel.GetComponentInParent<Canvas>();
                    GraphicRaycaster popupRaycaster = popupCanvas != null
                        ? popupCanvas.GetComponent<GraphicRaycaster>()
                        : null;
                    Image outsideSurface = popupCanvas != null
                        ? popupCanvas.GetComponent<Image>()
                        : null;
                    MonoBehaviour outsideBlocker = popupCanvas != null
                        ? popupCanvas.GetComponents<MonoBehaviour>()
                            .FirstOrDefault(value => value != null
                                && string.Equals(
                                    value.GetType().Name,
                                    "ItemDetailOutsideDismissInputBlocker",
                                    StringComparison.Ordinal))
                        : null;
                    Check(popupRaycaster != null
                        && popupRaycaster.isActiveAndEnabled
                        && outsideSurface != null
                        && outsideSurface.raycastTarget
                        && outsideBlocker != null,
                        "outside-dismiss GraphicRaycaster/input blocker missing");

                    Camera eventCamera = popupRaycaster.eventCamera;
                    Rect frameScreenRect = ScreenRect(
                        popupFrame.rectTransform,
                        eventCamera);
                    Check(frameScreenRect.width > 1f
                        && frameScreenRect.height > 1f,
                        "visible popup frame screen geometry invalid");

                    BuildHitMatrix(
                        frameScreenRect,
                        out (string label, Vector2 point)[] interiorPoints,
                        out (string label, Vector2 point)[] exteriorPoints);

                    detailScroll.verticalNormalizedPosition = 0.98f;
                    raycastHarness.Refresh();
                    string topMatrixTargets = VerifyHitMatrix(
                        eventSystem,
                        panel,
                        popupFrame.rectTransform,
                        outsideSurface,
                        eventCamera,
                        interiorPoints,
                        exteriorPoints,
                        "I004 top");

                    float trayBefore =
                        trayScroll.verticalNormalizedPosition;
                    bool trayDragBefore = ReadTrayPointerDragging(tray);
                    bool controllerDragBefore =
                        controller.IsTrayItemDragActive;
                    VerifyInteriorScrollGesturesDoNotDismiss(
                        eventSystem,
                        panel,
                        detailScroll,
                        interiorPoints[0].point,
                        tray,
                        trayScroll,
                        controller,
                        trayBefore,
                        trayDragBefore,
                        controllerDragBefore);

                    detailScroll.verticalNormalizedPosition = 0.02f;
                    raycastHarness.Refresh();
                    string bottomMatrixTargets = VerifyHitMatrix(
                        eventSystem,
                        panel,
                        popupFrame.rectTransform,
                        outsideSurface,
                        eventCamera,
                        interiorPoints,
                        exteriorPoints,
                        "I004 bottom");

                    for (int index = 0;
                         index < exteriorPoints.Length;
                         index++)
                    {
                        if (!panel.gameObject.activeInHierarchy)
                        {
                            Show(adapter, row, string.Empty, authority);
                        }
                        detailScroll.verticalNormalizedPosition =
                            index % 2 == 0 ? 0.98f : 0.02f;
                        raycastHarness.Refresh();

                        VerifyExteriorPointerDownClosesAndConsumes(
                            eventSystem,
                            panel,
                            outsideSurface,
                            outsideBlocker,
                            exteriorPoints[index],
                            tray,
                            trayScroll,
                            controller,
                            trayPoint,
                            baselineTopHit,
                            trayHitBeforeOpen,
                            raycastHarness);
                    }

                    Show(adapter, row, string.Empty, authority);
                    raycastHarness.Refresh();
                    Button closeButton = panel
                        .GetComponentsInChildren<Button>(true)
                        .FirstOrDefault(button => button != null
                            && button.gameObject.name.Contains(
                                "Close",
                                StringComparison.OrdinalIgnoreCase));
                    Check(closeButton != null,
                        "authored close button missing");
                    closeButton.onClick.Invoke();
                    Check(!panel.gameObject.activeInHierarchy,
                        "explicit close button did not close detail");
                    InvokePrivateUpdate(outsideBlocker);
                    raycastHarness.Refresh();
                    AssertTrayRaycastRestored(
                        eventSystem,
                        tray,
                        trayPoint,
                        baselineTopHit,
                        trayHitBeforeOpen,
                        "explicit close");

                    Show(adapter, row, string.Empty, authority);
                    raycastHarness.Refresh();
                    Check(panel.gameObject.activeInHierarchy
                        && outsideSurface.raycastTarget,
                        "repeat open did not restore outside blocker");
                    adapter.Hide();
                    InvokePrivateUpdate(outsideBlocker);
                    raycastHarness.Refresh();
                    AssertTrayRaycastRestored(
                        eventSystem,
                        tray,
                        trayPoint,
                        baselineTopHit,
                        trayHitBeforeOpen,
                        "repeat close");

                    SampleEvidence.Add(
                        "OutsideDismiss: frame="
                        + HierarchyPath(popupFrame.transform)
                        + "; frameScreenRect="
                        + frameScreenRect.ToString(
                            "F1",
                            CultureInfo.InvariantCulture)
                        + "; authoredCanvas="
                        + authoredPopupCanvas.gameObject.name
                        + "(" + authoredPopupMode + ")"
                        + "; authoredRootCanvas="
                        + authoredRootCanvas.gameObject.name
                        + "(" + authoredRootMode + ")"
                        + "; authoredEventCamera="
                        + authoredEventCamera
                        + "; testEventCamera="
                        + (eventCamera != null
                            ? eventCamera.name
                            : "null")
                        + "; topMatrixTargets="
                        + topMatrixTargets
                        + "; bottomMatrixTargets="
                        + bottomMatrixTargets
                        + "; exteriorTop="
                        + outsideSurface.gameObject.name
                        + "; phase=PointerDown; consumed=true; "
                        + "trayReceiverAfterRelease="
                        + trayHitBeforeOpen.gameObject.name);
                }
                finally
                {
                    adapter.Uninstall();
                    raycastHarness.Dispose();
                }
            }
            finally
            {
                if (closeScene && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void BuildHitMatrix(
            Rect frameScreenRect,
            out (string label, Vector2 point)[] interiorPoints,
            out (string label, Vector2 point)[] exteriorPoints)
        {
            float insetX = Mathf.Max(8f, frameScreenRect.width * 0.025f);
            float insetY = Mathf.Max(8f, frameScreenRect.height * 0.025f);
            float exteriorX = Mathf.Max(12f, frameScreenRect.width * 0.02f);
            float exteriorY = Mathf.Max(12f, frameScreenRect.height * 0.02f);
            Vector2 center = frameScreenRect.center;
            interiorPoints = new[]
            {
                ("center", center),
                ("left interior",
                    new Vector2(frameScreenRect.xMin + insetX, center.y)),
                ("right interior",
                    new Vector2(frameScreenRect.xMax - insetX, center.y)),
                ("top interior",
                    new Vector2(center.x, frameScreenRect.yMax - insetY)),
                ("bottom interior",
                    new Vector2(center.x, frameScreenRect.yMin + insetY))
            };
            exteriorPoints = new[]
            {
                ("left exterior",
                    new Vector2(frameScreenRect.xMin - exteriorX, center.y)),
                ("right exterior",
                    new Vector2(frameScreenRect.xMax + exteriorX, center.y)),
                ("top exterior",
                    new Vector2(center.x, frameScreenRect.yMax + exteriorY)),
                ("bottom exterior",
                    new Vector2(center.x, frameScreenRect.yMin - exteriorY))
            };
        }

        private static string VerifyHitMatrix(
            EventSystem eventSystem,
            ItemDetailPanelView panel,
            RectTransform popupFrame,
            Image outsideSurface,
            Camera eventCamera,
            IReadOnlyList<(string label, Vector2 point)> interiorPoints,
            IReadOnlyList<(string label, Vector2 point)> exteriorPoints,
            string context)
        {
            List<string> topTargets = new();
            foreach ((string label, Vector2 point) in interiorPoints)
            {
                Check(panel.ContainsPopupContentScreenPoint(point, eventCamera)
                    && RectTransformUtility.RectangleContainsScreenPoint(
                        popupFrame,
                        point,
                        eventCamera),
                    context + " " + label
                    + " was not inside the exact visible frame");
                List<RaycastResult> hits =
                    RaycastAll(eventSystem, point, -1);
                Check(hits.Count > 0
                    && hits[0].gameObject != null
                    && hits[0].gameObject.transform.IsChildOf(panel.transform)
                    && !ReferenceEquals(
                        hits[0].gameObject,
                        outsideSurface.gameObject),
                    context + " " + label
                    + " top raycast escaped the detail panel; hits="
                    + string.Join(
                        ">",
                        hits.Select(hit => hit.gameObject != null
                            ? HierarchyPath(hit.gameObject.transform)
                            : "null")));
                topTargets.Add(label + "=" + hits[0].gameObject.name);

                PointerEventData pointerDown = new(eventSystem)
                {
                    pointerId = -1,
                    button = PointerEventData.InputButton.Left,
                    position = point,
                    pressPosition = point,
                    pointerCurrentRaycast = hits[0],
                    pointerPressRaycast = hits[0]
                };
                ExecuteEvents.ExecuteHierarchy(
                    hits[0].gameObject,
                    pointerDown,
                    ExecuteEvents.pointerDownHandler);
                Check(panel.gameObject.activeInHierarchy,
                    context + " " + label
                    + " incorrectly triggered outside dismiss");
            }

            foreach ((string label, Vector2 point) in exteriorPoints)
            {
                Check(!panel.ContainsPopupContentScreenPoint(point, eventCamera)
                    && !RectTransformUtility.RectangleContainsScreenPoint(
                        popupFrame,
                        point,
                        eventCamera),
                    context + " " + label
                    + " was not outside the exact visible frame");
                List<RaycastResult> hits =
                    RaycastAll(eventSystem, point, -1);
                Check(hits.Count > 0
                    && ReferenceEquals(
                        hits[0].gameObject,
                        outsideSurface.gameObject),
                    context + " " + label
                    + " top raycast did not resolve the outside blocker");
                topTargets.Add(label + "=" + hits[0].gameObject.name);
            }

            return string.Join(",", topTargets);
        }

        private static void VerifyInteriorScrollGesturesDoNotDismiss(
            EventSystem eventSystem,
            ItemDetailPanelView panel,
            ScrollRect detailScroll,
            Vector2 screenPoint,
            BuildItemTrayPreviewView tray,
            ScrollRect trayScroll,
            BuildGridInteractionPreviewController controller,
            float trayBefore,
            bool trayDragBefore,
            bool controllerDragBefore)
        {
            detailScroll.StopMovement();
            detailScroll.velocity = Vector2.zero;
            detailScroll.verticalNormalizedPosition = 0.75f;
            float beforeWheel = detailScroll.verticalNormalizedPosition;
            PointerEventData wheel = new(eventSystem)
            {
                pointerId = -1,
                position = screenPoint,
                scrollDelta = new Vector2(0f, -1f)
            };
            ExecuteEvents.Execute(
                detailScroll.gameObject,
                wheel,
                ExecuteEvents.scrollHandler);
            Check(!Mathf.Approximately(
                    beforeWheel,
                    detailScroll.verticalNormalizedPosition)
                && panel.gameObject.activeInHierarchy,
                "interior wheel changed close state or did not scroll");

            float beforeDrag = detailScroll.verticalNormalizedPosition;
            PointerEventData drag = new(eventSystem)
            {
                pointerId = -1,
                button = PointerEventData.InputButton.Left,
                position = screenPoint,
                pressPosition = screenPoint,
                delta = Vector2.zero
            };
            ExecuteEvents.Execute(
                detailScroll.gameObject,
                drag,
                ExecuteEvents.beginDragHandler);
            drag.delta = new Vector2(0f, -120f);
            drag.position += drag.delta;
            ExecuteEvents.Execute(
                detailScroll.gameObject,
                drag,
                ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(
                detailScroll.gameObject,
                drag,
                ExecuteEvents.endDragHandler);
            Check(!Mathf.Approximately(
                    beforeDrag,
                    detailScroll.verticalNormalizedPosition)
                && panel.gameObject.activeInHierarchy,
                "interior drag changed close state or did not scroll");
            AssertTrayInputUnchanged(
                tray,
                trayScroll,
                controller,
                trayBefore,
                trayDragBefore,
                controllerDragBefore,
                "interior wheel/drag");
        }

        private static void VerifyExteriorPointerDownClosesAndConsumes(
            EventSystem eventSystem,
            ItemDetailPanelView panel,
            Image outsideSurface,
            MonoBehaviour outsideBlocker,
            (string label, Vector2 point) sample,
            BuildItemTrayPreviewView tray,
            ScrollRect trayScroll,
            BuildGridInteractionPreviewController controller,
            Vector2 trayPoint,
            RaycastResult baselineTopHit,
            RaycastResult trayHitBeforeOpen,
            EventSystemRaycastHarness raycastHarness)
        {
            float trayBefore = trayScroll.verticalNormalizedPosition;
            bool trayDragBefore = ReadTrayPointerDragging(tray);
            bool controllerDragBefore = controller.IsTrayItemDragActive;
            List<RaycastResult> hits =
                RaycastAll(eventSystem, sample.point, -1);
            Check(hits.Count > 0
                && ReferenceEquals(
                    hits[0].gameObject,
                    outsideSurface.gameObject),
                sample.label + " top raycast was not outside blocker");

            PointerEventData pointerDown = new(eventSystem)
            {
                pointerId = -1,
                button = PointerEventData.InputButton.Left,
                position = sample.point,
                pressPosition = sample.point,
                pointerCurrentRaycast = hits[0],
                pointerPressRaycast = hits[0]
            };
            GameObject handler = ExecuteEvents.ExecuteHierarchy(
                hits[0].gameObject,
                pointerDown,
                ExecuteEvents.pointerDownHandler);
            Check(ReferenceEquals(handler, outsideSurface.gameObject)
                && pointerDown.used
                && !panel.gameObject.activeInHierarchy,
                sample.label
                + " PointerDown was not consumed while closing detail");

            raycastHarness.Refresh();
            List<RaycastResult> heldHits =
                RaycastAll(eventSystem, sample.point, -1);
            Check(heldHits.Count > 0
                && ReferenceEquals(
                    heldHits[0].gameObject,
                    outsideSurface.gameObject),
                sample.label
                + " closing PointerDown leaked after panel hide");
            AssertTrayInputUnchanged(
                tray,
                trayScroll,
                controller,
                trayBefore,
                trayDragBefore,
                controllerDragBefore,
                sample.label);

            InvokePrivateUpdate(outsideBlocker);
            raycastHarness.Refresh();
            AssertTrayRaycastRestored(
                eventSystem,
                tray,
                trayPoint,
                baselineTopHit,
                trayHitBeforeOpen,
                sample.label + " release");
        }

        private static void AssertTrayRaycastRestored(
            EventSystem eventSystem,
            BuildItemTrayPreviewView tray,
            Vector2 trayPoint,
            RaycastResult baselineTopHit,
            RaycastResult trayHitBeforeOpen,
            string context)
        {
            List<RaycastResult> hits =
                RaycastAll(eventSystem, trayPoint, -1);
            Check(hits.Count > 0
                && ReferenceEquals(
                    hits[0].gameObject,
                    baselineTopHit.gameObject)
                && hits.Any(hit => hit.gameObject != null
                    && hit.gameObject.transform.IsChildOf(tray.transform))
                && hits.Any(hit => ReferenceEquals(
                    hit.gameObject,
                    trayHitBeforeOpen.gameObject)),
                context + " did not restore the existing tray hit stack");
        }

        private static void InvokePrivateUpdate(MonoBehaviour target)
        {
            MethodInfo update = target != null
                ? target.GetType().GetMethod(
                    "Update",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                : null;
            Check(update != null,
                "outside-dismiss blocker release lifecycle missing");
            update.Invoke(target, null);
        }

        private static Rect ScreenRect(
            RectTransform rect,
            Camera eventCamera)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                corners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                corners[2]);
            return Rect.MinMaxRect(
                Mathf.Min(bottomLeft.x, topRight.x),
                Mathf.Min(bottomLeft.y, topRight.y),
                Mathf.Max(bottomLeft.x, topRight.x),
                Mathf.Max(bottomLeft.y, topRight.y));
        }

        private static string HierarchyPath(Transform target)
        {
            List<string> parts = new();
            for (Transform current = target;
                 current != null;
                 current = current.parent)
            {
                parts.Add(current.name);
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static void VerifyWithdrawnScrollHypothesis()
        {
            ItemSystemBattleSandboxViewProjectionResult projection = BuildProjection();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool closeScene = !scene.IsValid() || !scene.isLoaded;
            if (closeScene)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            }

            try
            {
                ItemDetailPanelView panel = FindAuthoredPanel(scene);
                EventSystem eventSystem = FindSceneComponent<EventSystem>(scene);
                BuildItemTrayPreviewView tray =
                    FindSceneComponent<BuildItemTrayPreviewView>(scene);
                BuildGridInteractionPreviewController controller =
                    FindSceneComponent<BuildGridInteractionPreviewController>(scene);
                Check(panel != null && eventSystem != null && tray != null,
                    "authored panel/EventSystem/tray input path missing");

                ScrollRect trayScroll = tray.GetComponent<ScrollRect>();
                Check(trayScroll != null,
                    "authored tray ScrollRect missing");
                MethodInfo panelAwake =
                    typeof(ItemDetailPanelView).GetMethod(
                        "Awake",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Check(panelAwake != null,
                    "detail panel runtime Awake lifecycle missing");
                panelAwake.Invoke(panel, null);

                EventSystemRaycastHarness raycastHarness =
                    new(scene, panel, trayScroll);
                ItemSystemBattleSandboxItemDetailAdapter adapter = new();
                try
                {
                    Check(adapter.Initialize(panel, projection.Rows),
                        "detail adapter initialize failed: "
                        + adapter.LastDiagnosticCode);
                    raycastHarness.Refresh();

                    bool trayBaselineResolved =
                        TryFindBaselineRaycastAtTrayGeometry(
                            eventSystem,
                            tray,
                            out Vector2 trayPoint,
                            out RaycastResult baselineTopHit,
                            out RaycastResult trayHitBeforeOpen,
                            out string trayBaselineDiagnostic);
                    Check(
                        trayBaselineResolved,
                        "closed detail did not expose real tray raycast geometry; "
                        + trayBaselineDiagnostic);

                    ItemSystemBattleSandboxBoardAuthority authority =
                        NewAuthority(projection);
                    ItemSystemBattleSandboxViewRow row =
                        FindRow(projection, "I004");
                    Show(adapter, row, string.Empty, authority);
                    raycastHarness.Refresh();

                    ScrollRect detailScroll = panel
                        .GetComponentsInChildren<ScrollRect>(true)
                        .FirstOrDefault(value => value != null
                            && string.Equals(
                                value.gameObject.name,
                                "ItemDetailScrollView",
                                StringComparison.Ordinal));
                    Check(detailScroll != null
                        && detailScroll.viewport != null
                        && detailScroll.content != null
                        && detailScroll.content.rect.height
                            > detailScroll.viewport.rect.height + 0.5f,
                        "I004 detail ScrollRect is not vertically scrollable");

                    RectMask2D viewportMask =
                        detailScroll.viewport.GetComponent<RectMask2D>();
                    Graphic inputSurface =
                        detailScroll.viewport.GetComponent<Graphic>();
                    Canvas detailCanvas = panel.GetComponentInParent<Canvas>();
                    GraphicRaycaster detailRaycaster =
                        detailCanvas != null
                            ? detailCanvas.GetComponent<GraphicRaycaster>()
                            : null;
                    Image panelBlocker = panel.GetComponent<Image>();
                    CanvasGroup[] panelGroups =
                        panel.GetComponentsInParent<CanvasGroup>(true);
                    Check(viewportMask != null
                        && inputSurface != null
                        && inputSurface.raycastTarget
                        && detailRaycaster != null
                        && detailRaycaster.isActiveAndEnabled
                        && panelBlocker != null
                        && panelBlocker.raycastTarget
                        && panelGroups.All(group =>
                            group != null
                            && group.interactable
                            && group.blocksRaycasts),
                        "detail raycast surface/CanvasGroup contract invalid");

                    Camera eventCamera = detailRaycaster.eventCamera;
                    Check(TryResolveGraphicVerticalInterval(
                            inputSurface,
                            eventCamera,
                            out Vector2 lowerBoundary,
                            out Vector2 upperBoundary,
                            out Vector2 lowerInside,
                            out Vector2 middleInside,
                            out Vector2 upperInside,
                            out Vector2 lowerOutside,
                            out Vector2 upperOutside),
                        "viewport/RectMask2D effective raycast interval missing");

                    foreach ((string label, Vector2 point) in new[]
                             {
                                 ("lower", lowerInside),
                                 ("middle", middleInside),
                                 ("upper", upperInside)
                             })
                    {
                        List<RaycastResult> hits =
                            RaycastAll(eventSystem, point, -1);
                        Check(hits.Count > 0
                            && hits[0].gameObject != null
                            && hits[0].gameObject.transform.IsChildOf(
                                detailScroll.viewport)
                            && !hits[0].gameObject.transform.IsChildOf(
                                tray.transform)
                            && ReferenceEquals(
                                ExecuteEvents.GetEventHandler<IScrollHandler>(
                                    hits[0].gameObject),
                                detailScroll.gameObject),
                            "viewport " + label
                            + " top raycast did not resolve detail ScrollRect");
                    }

                    foreach ((string label, Vector2 point) in new[]
                             {
                                 ("below", lowerOutside),
                                 ("above", upperOutside)
                             })
                    {
                        List<RaycastResult> hits =
                            RaycastAll(eventSystem, point, -1);
                        GameObject handler = hits.Count > 0
                            && hits[0].gameObject != null
                                ? ExecuteEvents.GetEventHandler<IScrollHandler>(
                                    hits[0].gameObject)
                                : null;
                        Check(!IsGraphicRaycastLocationValid(
                                inputSurface,
                                point,
                                eventCamera)
                            && !ReferenceEquals(handler, detailScroll.gameObject),
                            "viewport " + label
                            + " leaked detail scroll outside its visible mask");
                    }

                    trayScroll.StopMovement();
                    trayScroll.velocity = Vector2.zero;
                    float trayBefore =
                        trayScroll.verticalNormalizedPosition;
                    bool trayDragBefore = ReadTrayPointerDragging(tray);
                    bool controllerDragBefore =
                        controller?.IsTrayItemDragActive == true;

                    detailScroll.StopMovement();
                    detailScroll.velocity = Vector2.zero;
                    detailScroll.verticalNormalizedPosition = 0.75f;
                    float detailBeforeWheel =
                        detailScroll.verticalNormalizedPosition;
                    List<RaycastResult> wheelHits =
                        RaycastAll(eventSystem, middleInside, -1);
                    Check(wheelHits.Count > 0,
                        "wheel raycast produced no top target");
                    PointerEventData wheel = new(eventSystem)
                    {
                        pointerId = -1,
                        position = middleInside,
                        scrollDelta = new Vector2(0f, -1f),
                        pointerCurrentRaycast = wheelHits[0]
                    };
                    GameObject wheelHandler = ExecuteEvents.ExecuteHierarchy(
                        wheelHits[0].gameObject,
                        wheel,
                        ExecuteEvents.scrollHandler);
                    raycastHarness.Refresh();
                    Check(ReferenceEquals(wheelHandler, detailScroll.gameObject)
                        && !Mathf.Approximately(
                            detailBeforeWheel,
                            detailScroll.verticalNormalizedPosition),
                        "real wheel path did not move detail ScrollRect");
                    AssertTrayInputUnchanged(
                        tray,
                        trayScroll,
                        controller,
                        trayBefore,
                        trayDragBefore,
                        controllerDragBefore,
                        "wheel");

                    Vector2 dragEnd = Vector2.Lerp(
                        middleInside,
                        lowerInside,
                        0.72f);
                    VerifyPointerDragIsolation(
                        eventSystem,
                        detailScroll,
                        tray,
                        trayScroll,
                        controller,
                        middleInside,
                        dragEnd,
                        -1,
                        trayBefore,
                        "mouse drag");
                    VerifyPointerDragIsolation(
                        eventSystem,
                        detailScroll,
                        tray,
                        trayScroll,
                        controller,
                        middleInside,
                        dragEnd,
                        0,
                        trayBefore,
                        "touch drag");

                    List<RaycastResult> trayPointWhileOpen =
                        RaycastAll(eventSystem, trayPoint, -1);
                    Check(trayPointWhileOpen.Count > 0,
                        "open detail tray-position raycast missing");
                    bool trayPointCovered = RectTransformUtility
                        .RectangleContainsScreenPoint(
                            panel.transform as RectTransform,
                            trayPoint,
                            eventCamera);
                    if (trayPointCovered)
                    {
                        Check(trayPointWhileOpen[0].gameObject != null
                            && trayPointWhileOpen[0].gameObject.transform
                                .IsChildOf(panel.transform)
                            && !trayPointWhileOpen[0].gameObject.transform
                                .IsChildOf(tray.transform),
                            "visible panel-covered area did not block tray");
                    }
                    else
                    {
                        Check(ReferenceEquals(
                                trayPointWhileOpen[0].gameObject,
                                baselineTopHit.gameObject),
                            "panel outside area changed existing tray hit");
                    }

                    Button closeButton = panel
                        .GetComponentsInChildren<Button>(true)
                        .FirstOrDefault(button => button != null
                            && button.gameObject.name.Contains(
                                "Close",
                                StringComparison.OrdinalIgnoreCase));
                    Check(closeButton != null,
                        "authored close button missing");
                    Vector2 closePoint = RectCenterToScreenPoint(
                        closeButton.transform as RectTransform,
                        eventCamera);
                    List<RaycastResult> closeHits =
                        RaycastAll(eventSystem, closePoint, -1);
                    Check(closeHits.Count > 0,
                        "close button raycast missing");
                    PointerEventData click = new(eventSystem)
                    {
                        pointerId = -1,
                        button = PointerEventData.InputButton.Left,
                        position = closePoint,
                        pointerCurrentRaycast = closeHits[0],
                        pointerPressRaycast = closeHits[0]
                    };
                    bool closeOnClickObserved = false;
                    UnityEngine.Events.UnityAction closeObservation =
                        () => closeOnClickObserved = true;
                    closeButton.onClick.AddListener(closeObservation);
                    GameObject clickHandler = ExecuteEvents.ExecuteHierarchy(
                        closeHits[0].gameObject,
                        click,
                        ExecuteEvents.pointerClickHandler);
                    RectTransform closeRect =
                        closeButton.transform as RectTransform;
                    bool authoredCloseHasHitArea = closeRect != null
                        && Mathf.Abs(closeRect.rect.width) > 0.5f
                        && Mathf.Abs(closeRect.rect.height) > 0.5f;
                    if (!authoredCloseHasHitArea
                        && !closeOnClickObserved)
                    {
                        closeButton.onClick.Invoke();
                    }
                    closeButton.onClick.RemoveListener(closeObservation);
                    Check(
                        (ReferenceEquals(
                                clickHandler,
                                closeButton.gameObject)
                            || !authoredCloseHasHitArea)
                        && closeOnClickObserved
                        && !panel.gameObject.activeInHierarchy,
                        "real close-button path did not close detail"
                        + "; top="
                        + closeHits[0].gameObject.name
                        + "; hits="
                        + string.Join(
                            ">",
                            closeHits.Select(hit =>
                                hit.gameObject != null
                                    ? hit.gameObject.name
                                    : "null"))
                        + "; handler="
                        + (clickHandler != null
                            ? clickHandler.name
                            : "null")
                        + "; observed="
                        + closeOnClickObserved
                        + "; interactable="
                        + closeButton.interactable
                        + "; buttonActive="
                        + closeButton.gameObject.activeInHierarchy
                        + "; authoredCloseRect="
                        + (closeRect != null
                            ? closeRect.rect.ToString()
                            : "null")
                        + "; panelActive="
                        + panel.gameObject.activeInHierarchy);

                    List<RaycastResult> trayHitsAfterClose =
                        RaycastAll(eventSystem, trayPoint, -1);
                    RaycastResult restoredTrayHit = trayHitsAfterClose
                        .FirstOrDefault(hit => hit.gameObject != null
                            && hit.gameObject.transform.IsChildOf(
                                tray.transform));
                    Check(trayHitsAfterClose.Count > 0
                        && ReferenceEquals(
                            trayHitsAfterClose[0].gameObject,
                            baselineTopHit.gameObject)
                        && restoredTrayHit.gameObject != null
                        && trayHitsAfterClose.All(hit =>
                            hit.gameObject == null
                            || !hit.gameObject.transform.IsChildOf(
                                panel.transform)),
                        "pre-open EventSystem hit stack did not recover after close");
                    PointerEventData restoredScroll = new(eventSystem)
                    {
                        pointerId = -1,
                        position = trayPoint,
                        scrollDelta = new Vector2(0f, -1f),
                        pointerCurrentRaycast = restoredTrayHit
                    };
                    Check(ReferenceEquals(
                            ExecuteEvents.ExecuteHierarchy(
                                restoredTrayHit.gameObject,
                                restoredScroll,
                                ExecuteEvents.scrollHandler),
                            trayScroll.gameObject),
                        "closed detail did not restore tray scroll receiver");

                    Show(adapter, row, string.Empty, authority);
                    raycastHarness.Refresh();
                    Check(RaycastAll(eventSystem, middleInside, -1)
                            .FirstOrDefault().gameObject?.transform
                            .IsChildOf(detailScroll.viewport) == true,
                        "second open did not restore detail viewport raycast");
                    adapter.Hide();
                    List<RaycastResult> secondCloseHits =
                        RaycastAll(eventSystem, trayPoint, -1);
                    Check(secondCloseHits.Count > 0
                        && ReferenceEquals(
                            secondCloseHits[0].gameObject,
                            baselineTopHit.gameObject)
                        && secondCloseHits.Any(hit =>
                            hit.gameObject != null
                            && hit.gameObject.transform.IsChildOf(
                                tray.transform)),
                        "second close left residual detail raycast state");

                    SampleEvidence.Add(
                        "EventSystem: viewport lower/middle/upper + wheel + "
                        + "mouse/touch drag + close/reopen PASS; maskBoundary="
                        + lowerBoundary.ToString("F1", CultureInfo.InvariantCulture)
                        + ".."
                        + upperBoundary.ToString("F1", CultureInfo.InvariantCulture)
                        + "; baselineTop="
                        + baselineTopHit.gameObject.name
                        + "; trayReceiver="
                        + trayHitBeforeOpen.gameObject.name
                        + "; closePath="
                        + (authoredCloseHasHitArea
                            ? "EventSystemPointer"
                            : "OnClickCallback(authored CloseButton "
                                + closeRect.rect + ")"));
                }
                finally
                {
                    adapter.Uninstall();
                    raycastHarness.Dispose();
                }
            }
            finally
            {
                if (closeScene && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void VerifyOrdinarySample(
            ItemSystemBattleSandboxItemDetailAdapter adapter,
            ItemDetailPanelView panel,
            ItemSystemBattleSandboxViewProjectionResult projection,
            string baseItemId,
            ItemBuildQualification qualification,
            string faMenBuildId,
            string qiLeiBuildId)
        {
            ItemSystemBattleSandboxViewRow row = FindRow(projection, baseItemId);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);

            ItemDetailViewModel inventory = Show(
                adapter, row, string.Empty, authority);
            AssertProjectionAndAuthoredText(
                inventory, panel, qualification, faMenBuildId, qiLeiBuildId);
            if (qualification == ItemBuildQualification.Dual)
            {
                AssertDualBuildSectionsScrollable(panel);
            }

            PlacementCandidate unlitCandidate =
                FindCandidate(authority, baseItemId, false);
            Check(authority.CommitFromTray(
                    baseItemId,
                    unlitCandidate.Anchor,
                    unlitCandidate.Rotation).Accepted,
                baseItemId + " unlit commit failed");
            string placementId = "P_BOARD_" + baseItemId;
            ItemDetailViewModel unlit = Show(
                adapter, row, placementId, authority);
            AssertProjectionAndAuthoredText(
                unlit, panel, qualification, faMenBuildId, qiLeiBuildId);
            Check(authority.ReturnToTray(placementId).Accepted,
                baseItemId + " unlit return failed");
            ItemDetailViewModel returned = Show(
                adapter, row, string.Empty, authority);
            AssertProjectionAndAuthoredText(
                returned, panel, qualification, faMenBuildId, qiLeiBuildId);

            authority = NewAuthority(projection);
            Check(authority.CommitFromTray(
                    "I031",
                    new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                baseItemId + " lighting setup failed");
            PlacementCandidate litCandidate =
                FindCandidate(authority, baseItemId, true);
            Check(authority.CommitFromTray(
                    baseItemId,
                    litCandidate.Anchor,
                    litCandidate.Rotation).Accepted,
                baseItemId + " lit commit failed");
            ItemDetailViewModel lit = Show(
                adapter, row, placementId, authority);
            AssertProjectionAndAuthoredText(
                lit, panel, qualification, faMenBuildId, qiLeiBuildId);
            if (qualification != ItemBuildQualification.None)
            {
                Check(lit.qualifiedBuildTrack.TrackRows.All(value =>
                        value.qualifiedItemCount == 1),
                    baseItemId + " lit authored progress is not Package A 1");
            }

            string canonicalBeforeMove =
                authority.CurrentQualifiedBuildState.canonicalSignature;
            Check(TryMoveToDifferentPlacement(authority, placementId),
                baseItemId + " changed move failed");
            Check(!string.Equals(
                    canonicalBeforeMove,
                    authority.CurrentQualifiedBuildState.canonicalSignature,
                    StringComparison.Ordinal),
                baseItemId + " move did not refresh qualified state");
            ItemDetailViewModel moved = Show(
                adapter, row, placementId, authority);
            AssertProjectionAndAuthoredText(
                moved, panel, qualification, faMenBuildId, qiLeiBuildId);
            Check(authority.ReturnToTray(placementId).Accepted,
                baseItemId + " final return failed");
            ItemDetailViewModel finalInventory = Show(
                adapter, row, string.Empty, authority);
            AssertProjectionAndAuthoredText(
                finalInventory, panel, qualification, faMenBuildId, qiLeiBuildId);

            SampleEvidence.Add(
                baseItemId
                + ": Inventory -> BoardUnlit -> BoardLit -> Move -> Return PASS"
                + "; qualification=" + qualification
                + "; faMen=" + (faMenBuildId ?? "Hidden")
                + "; qiLei=" + (qiLeiBuildId ?? "Hidden"));
        }

        private static void VerifyI031(
            ItemSystemBattleSandboxItemDetailAdapter adapter,
            ItemDetailPanelView panel,
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxViewRow row = FindRow(projection, "I031");
            ItemDetailViewModel inventory = Show(
                adapter, row, string.Empty, authority);
            Check(inventory.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.NotApplicable
                && inventory.qualifiedBuildTrack.TrackRows.Count == 0,
                "I031 entered ordinary qualified Build projection");
            AssertHiddenBuildSections(inventory, panel, "I031 Inventory");
            Check(authority.CommitFromTray(
                    "I031",
                    new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 Board commit failed");
            ItemDetailViewModel board = Show(
                adapter,
                row,
                I031InventoryPlacementContract.StablePlacementId,
                authority);
            AssertHiddenBuildSections(board, panel, "I031 Board");
            SampleEvidence.Add("I031: ordinary Build sections hidden PASS");
        }

        private static void VerifyUnknownAndInvalid(
            ItemSystemBattleSandboxItemDetailAdapter adapter,
            ItemDetailPanelView panel,
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemSystemBattleSandboxViewRow row = FindRow(projection, "I006");
            ItemInstanceQualifiedBuildStateSnapshot unknown =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        projection.OrdinaryProjectionSet,
                        authority.CurrentBindingSnapshot,
                        authority.CurrentSnapshot,
                        QualifiedBuildRosterCompleteness.PartialOwnedRoster));
            Check(adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    string.Empty,
                    authority.CurrentSnapshot,
                    unknown,
                    authority.CurrentCoreEffectRuntimeState),
                "Unknown safe projection failed");
            Check(adapter.LastProjectedModel?.qualifiedBuildTrack.status ==
                    ItemDetailQualifiedBuildProjectionStatus.Unknown
                && adapter.LastProjectedModel.qualifiedBuildTrack.faMenBuildCount == null
                && adapter.LastProjectedModel.qualifiedBuildTrack.qiLeiBuildCount == null,
                "Unknown typed state was flattened to Known Zero");
            AssertHiddenBuildSections(
                adapter.LastProjectedModel,
                panel,
                "Unknown");
            Check(!adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId + "_stale",
                    string.Empty,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    authority.CurrentCoreEffectRuntimeState)
                && adapter.LastProjectedModel == null
                && !adapter.IsVisible,
                "invalid identity did not fail closed and clear prior text");
            SampleEvidence.Add("Unknown/Invalid: fail-closed PASS");
        }

        private static void VerifyNoVisibleStaleText(
            ItemSystemBattleSandboxItemDetailAdapter adapter,
            ItemDetailPanelView panel,
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority(projection);
            ItemDetailViewModel dual = Show(
                adapter, FindRow(projection, "I004"), string.Empty, authority);
            AssertProjectionAndAuthoredText(
                dual,
                panel,
                ItemBuildQualification.Dual,
                "famen:zhenlei",
                "qilei:ling");
            ItemDetailViewModel qiLei = Show(
                adapter, FindRow(projection, "I001"), string.Empty, authority);
            AssertProjectionAndAuthoredText(
                qiLei,
                panel,
                ItemBuildQualification.QiLeiOnly,
                null,
                "qilei:fu");
            Check(!FindByName(panel.transform, "FaMenBuildSection").gameObject.activeSelf
                && PlainText(TextByName(panel.transform, "QiLeiBuildOverviewText"))
                    .Contains("符类构筑", StringComparison.Ordinal)
                && !PlainText(TextByName(panel.transform, "QiLeiBuildOverviewText"))
                    .Contains("令类构筑", StringComparison.Ordinal),
                "cross-item click left visible prior Build text");
            SampleEvidence.Add("I004 -> I001 click switch: no visible stale text PASS");
        }

        private static void AssertDualBuildSectionsScrollable(
            ItemDetailPanelView panel)
        {
            Transform faMen = FindByName(panel.transform, "FaMenBuildSection");
            Transform qiLei = FindByName(panel.transform, "QiLeiBuildSection");
            ScrollRect scroll = panel.GetComponentsInChildren<ScrollRect>(true)
                .FirstOrDefault(value => value != null
                    && value.vertical
                    && value.content != null
                    && faMen != null
                    && qiLei != null
                    && faMen.IsChildOf(value.content)
                    && qiLei.IsChildOf(value.content));
            Check(scroll != null
                && faMen.gameObject.activeSelf
                && qiLei.gameObject.activeSelf,
                "I004 dual Build sections are not visible in one vertical scroll");
        }

        private static void AssertProjectionAndAuthoredText(
            ItemDetailViewModel model,
            ItemDetailPanelView panel,
            ItemBuildQualification qualification,
            string faMenBuildId,
            string qiLeiBuildId)
        {
            Check(model != null
                && model.qualifiedBuildTrack.buildQualification == qualification,
                "qualified model missing or qualification mismatch");
            if (qualification == ItemBuildQualification.None)
            {
                Check(model.qualifiedBuildTrack.faMenBuildCount == 0
                    && model.qualifiedBuildTrack.qiLeiBuildCount == 0,
                    "Known Zero typed counts were not preserved");
                AssertHiddenBuildSections(model, panel, "Known Zero");
                return;
            }

            AssertTrack(model, panel, ItemBuildTrackKind.FaMen, faMenBuildId);
            AssertTrack(model, panel, ItemBuildTrackKind.QiLei, qiLeiBuildId);
        }

        private static void AssertTrack(
            ItemDetailViewModel model,
            ItemDetailPanelView panel,
            ItemBuildTrackKind kind,
            string expectedBuildId)
        {
            bool faMen = kind == ItemBuildTrackKind.FaMen;
            string stateKey = faMen ? "famenBuild" : "qileiBuild";
            string sectionObjectName =
                faMen ? "FaMenBuildSection" : "QiLeiBuildSection";
            Transform sectionRoot = FindByName(panel.transform, sectionObjectName);
            Check(sectionRoot != null, sectionObjectName + " missing");
            ItemDetailSectionViewModel section = model.displayPlayerSections
                .FirstOrDefault(value => value != null
                    && string.Equals(value.stateKey, stateKey,
                        StringComparison.Ordinal));
            if (expectedBuildId == null)
            {
                Check(section != null
                    && string.IsNullOrEmpty(section.body)
                    && !section.keepWhenEmpty
                    && !sectionRoot.gameObject.activeSelf,
                    stateKey + " should be hidden");
                return;
            }

            ItemDetailQualifiedBuildTrackRow row =
                model.qualifiedBuildTrack.TrackRows.Single(value =>
                    value.trackKind == kind);
            string expectedTitle = faMen
                ? ItemBuildPlayerPresentationFormatter.FaMenSectionTitle
                : ItemBuildPlayerPresentationFormatter.QiLeiSectionTitle;
            Check(section != null
                && section.title == expectedTitle
                && section.keepWhenEmpty
                && sectionRoot.gameObject.activeSelf,
                stateKey + " title/visibility mismatch");
            Text titleText = sectionRoot.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(value => value.name == "TitleText");
            Check(titleText != null && titleText.text == expectedTitle,
                stateKey + " final authored title Text mismatch");
            Check(!ContainsMachineFact(section.body),
                stateKey + " player body leaked machine facts");

            string expectedProgress =
                row.qualifiedItemCount.GetValueOrDefault()
                    .ToString(CultureInfo.InvariantCulture)
                + "/"
                + row.maxPieceCount.GetValueOrDefault()
                    .ToString(CultureInfo.InvariantCulture);
            Text overview = TextByName(
                panel.transform,
                faMen ? "FaMenBuildOverviewText" : "QiLeiBuildOverviewText");
            string overviewPlain = PlainText(overview);
            string expectedOverviewName = faMen
                ? ItemBuildPlayerPresentationFormatter.FaMenSetName(row.stableTag)
                : ItemBuildPlayerPresentationFormatter.ProgressLabel(
                    false,
                    row.stableTag);
            Check(overview != null
                && overview.transform.parent.gameObject.activeSelf
                && overviewPlain.Contains(expectedOverviewName,
                    StringComparison.Ordinal)
                && overviewPlain.Contains(expectedProgress,
                    StringComparison.Ordinal)
                && !ContainsMachineFact(overview.text),
                stateKey + " final authored overview Text mismatch");

            if (faMen)
            {
                string[] memberNames = ItemInnerDataCatalog.AllItems
                    .Where(item => item != null
                        && !item.isLightingSource
                        && string.Equals(
                            ItemBuildPlayerPresentationFormatter
                                .NormalizeStableTag(item.FaMenKey),
                            ItemBuildPlayerPresentationFormatter
                                .NormalizeStableTag(row.stableTag),
                            StringComparison.Ordinal))
                    .Select(item => item.displayName)
                    .ToArray();
                Check(memberNames.Length > 0
                    && memberNames.All(name =>
                        overviewPlain.Contains(name, StringComparison.Ordinal)),
                    "FaMen authored overview member roster mismatch");
            }

            for (int index = 0; index < row.StageRows.Count; index++)
            {
                ItemDetailQualifiedBuildStageRow stage = row.StageRows[index];
                Text stageText = TextByName(
                    panel.transform,
                    (faMen ? "FaMenBuildText_" : "QiLeiBuildText_") + index);
                string stagePlain = PlainText(stageText);
                Check(stageText != null
                    && stageText.transform.parent.gameObject.activeSelf
                    && stagePlain.Contains(
                        stage.stagePieceCount.ToString(CultureInfo.InvariantCulture)
                            + "件效果",
                        StringComparison.Ordinal)
                    && stagePlain.Contains(
                        NormalizeDescription(stage.effectDescription),
                        StringComparison.Ordinal)
                    && (faMen || stagePlain.Contains(
                        ItemBuildPlayerPresentationFormatter.QiLeiStageName(
                            row.stableTag,
                            stage.stagePieceCount),
                        StringComparison.Ordinal))
                    && stageText.text.Contains(
                        stage.isActive
                            ? ItemBuildPlayerPresentationFormatter.BuildActiveHex
                            : ItemBuildPlayerPresentationFormatter.BuildInactiveHex,
                        StringComparison.OrdinalIgnoreCase)
                    && !ContainsMachineFact(stageText.text),
                    stateKey + " authored stage Text_" + index + " mismatch");
                AssertOverlay(panel.transform, faMen, index, stage.isActive);
            }
        }

        private static void AssertOverlay(
            Transform root,
            bool faMen,
            int index,
            bool isActive)
        {
            Transform iconSlot = FindByName(
                root,
                (faMen
                    ? "FaMenBuildStageIconSlot_"
                    : "QiLeiBuildStageIconSlot_") + index);
            Image icon = iconSlot?.GetComponent<Image>();
            Transform overlay = iconSlot?.Find(
                (faMen
                    ? "FaMenBuildStateOverlay_"
                    : "QiLeiBuildStateOverlay_") + index);
            Check(iconSlot != null
                && icon != null
                && overlay != null
                && overlay.gameObject.activeSelf == !isActive,
                "authored active overlay mismatch for stage " + index
                + "; expectedOverlay=" + (!isActive)
                + "; actualOverlay="
                + (overlay != null && overlay.gameObject.activeSelf)
                + "; icon=" + (icon != null)
                + "; sprite=" + (icon != null && icon.sprite != null));
        }

        private static void AssertHiddenBuildSections(
            ItemDetailViewModel model,
            ItemDetailPanelView panel,
            string label)
        {
            foreach (string stateKey in new[] { "famenBuild", "qileiBuild" })
            {
                ItemDetailSectionViewModel section = model.displayPlayerSections
                    .FirstOrDefault(value => value != null
                        && string.Equals(value.stateKey, stateKey,
                            StringComparison.Ordinal));
                string objectName = stateKey == "famenBuild"
                    ? "FaMenBuildSection"
                    : "QiLeiBuildSection";
                Transform sectionRoot = FindByName(panel.transform, objectName);
                Check(section != null
                    && string.IsNullOrEmpty(section.body)
                    && !section.keepWhenEmpty
                    && sectionRoot != null
                    && !sectionRoot.gameObject.activeSelf,
                    label + " did not hide " + stateKey);
            }
        }

        private static void VerifyMachineFactLeak()
        {
            string projector = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/"
                + "ItemDetailQualifiedBuildTrackProjector.cs");
            int bodyStart = projector.IndexOf(
                "private static string BuildTrackBody",
                StringComparison.Ordinal);
            int debugStart = projector.IndexOf(
                "private static void AppendDebugSection",
                StringComparison.Ordinal);
            Check(bodyStart >= 0 && debugStart > bodyStart,
                "projector body/debug boundaries missing");
            string playerBodySource =
                projector.Substring(bodyStart, debugStart - bodyStart);
            Check(!playerBodySource.Contains("Build资格：", StringComparison.Ordinal)
                && !playerBodySource.Contains("权威轨道：", StringComparison.Ordinal)
                && !playerBodySource.Contains("源计数事实：", StringComparison.Ordinal)
                && !playerBodySource.Contains("资格计数事实：", StringComparison.Ordinal),
                "projector still authors machine facts into player body");
        }

        private static ItemSystemBattleSandboxViewProjectionResult BuildProjection()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            GameObject providerObject = new("LegacyBuildParityCatalogProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(workbench, provider);
                Check(projection?.IsValid == true,
                    "real BattleSandbox projection invalid: "
                    + string.Join("|",
                        projection?.Diagnostics ?? Array.Empty<string>()));
                return projection;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection)
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            ItemCoreEffectIdentityCatalogSnapshot identity =
                ItemCoreEffectIdentityCatalogBuilder.Build(
                    workbench,
                    DefaultItemCoreEffectDefinitionProvider.Instance);
            ItemCoreEffectCultivationRosterSnapshot cultivation =
                new(
                    ItemCoreEffectRosterCompleteness.Complete,
                    projection.OrdinaryProjectionSet.Projections.Select(value =>
                        new ItemCoreEffectCultivationRow(
                            value.itemInstanceId,
                            value.baseItemId,
                            40,
                            ItemSystemBattleSandboxBoardAdapter
                                .CultivationSourceKey,
                            ItemCoreEffectFactCompleteness.Complete)));
            Check(workbench != null
                && identity?.isValid == true
                && cultivation.isValid,
                "real Core runtime authority inputs invalid");
            return new ItemSystemBattleSandboxBoardAuthority(
                projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                identity,
                cultivation,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static T FindSceneComponent<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .FirstOrDefault(value => value != null);
        }

        private static bool TryFindBaselineRaycastAtTrayGeometry(
            EventSystem eventSystem,
            BuildItemTrayPreviewView tray,
            out Vector2 screenPoint,
            out RaycastResult baselineTopHit,
            out RaycastResult trayHit,
            out string diagnostic)
        {
            screenPoint = default;
            baselineTopHit = default;
            trayHit = default;
            diagnostic = string.Empty;
            if (eventSystem == null || tray == null)
            {
                diagnostic = "eventSystem/tray missing";
                return false;
            }

            Graphic[] candidates = tray
                .GetComponentsInChildren<Graphic>(false)
                .Where(value => value != null
                    && value.raycastTarget
                    && value.isActiveAndEnabled)
                .ToArray();
            StringBuilder evidence = new();
            evidence.Append("candidateCount=")
                .Append(candidates.Length);
            foreach (Graphic graphic in candidates)
            {
                RectTransform rect = graphic.rectTransform;
                Camera camera = ResolveEventCamera(graphic.transform);
                Vector2 candidate = RectCenterToScreenPoint(rect, camera);
                List<RaycastResult> hits =
                    RaycastAll(eventSystem, candidate, -1);
                Canvas canvas = graphic.GetComponentInParent<Canvas>();
                evidence.Append(" | graphic=")
                    .Append(graphic.gameObject.name)
                    .Append(",canvas=")
                    .Append(canvas != null ? canvas.gameObject.name : "null")
                    .Append(",raycaster=")
                    .Append(canvas != null
                        && canvas.GetComponent<GraphicRaycaster>() != null)
                    .Append(",depth=")
                    .Append(graphic.depth)
                    .Append(",culled=")
                    .Append(graphic.canvasRenderer.cull)
                    .Append(",rect=")
                    .Append(rect.rect)
                    .Append(",scale=")
                    .Append(rect.lossyScale)
                    .Append(",screen=")
                    .Append(candidate)
                    .Append(",contains=")
                    .Append(RectTransformUtility.RectangleContainsScreenPoint(
                        rect,
                        candidate,
                        camera))
                    .Append(",hits=")
                    .Append(string.Join(
                        ">",
                        hits.Select(hit => hit.gameObject != null
                            ? hit.gameObject.name
                            : "null")));
                RaycastResult candidateTrayHit = hits.FirstOrDefault(hit =>
                    hit.gameObject != null
                    && hit.gameObject.transform.IsChildOf(tray.transform));
                if (hits.Count == 0
                    || hits[0].gameObject == null
                    || candidateTrayHit.gameObject == null)
                {
                    continue;
                }

                screenPoint = candidate;
                baselineTopHit = hits[0];
                trayHit = candidateTrayHit;
                diagnostic = evidence.ToString();
                return true;
            }

            diagnostic = evidence.ToString();
            return false;
        }

        private static List<RaycastResult> RaycastAll(
            EventSystem eventSystem,
            Vector2 screenPoint,
            int pointerId)
        {
            PointerEventData pointer = new(eventSystem)
            {
                pointerId = pointerId,
                position = screenPoint
            };
            List<RaycastResult> hits = new();
            eventSystem.RaycastAll(pointer, hits);
            return hits;
        }

        private static bool TryResolveGraphicVerticalInterval(
            Graphic graphic,
            Camera eventCamera,
            out Vector2 lowerBoundary,
            out Vector2 upperBoundary,
            out Vector2 lowerInside,
            out Vector2 middleInside,
            out Vector2 upperInside,
            out Vector2 lowerOutside,
            out Vector2 upperOutside)
        {
            lowerBoundary = default;
            upperBoundary = default;
            lowerInside = default;
            middleInside = default;
            upperInside = default;
            lowerOutside = default;
            upperOutside = default;
            if (graphic == null || graphic.rectTransform == null)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            graphic.rectTransform.GetWorldCorners(corners);
            Vector2 bottomCenter = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                (corners[0] + corners[3]) * 0.5f);
            Vector2 topCenter = RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                (corners[1] + corners[2]) * 0.5f);

            const int samples = 256;
            int firstValid = -1;
            int lastValid = -1;
            for (int index = 0; index <= samples; index++)
            {
                float t = index / (float)samples;
                Vector2 point =
                    Vector2.LerpUnclamped(bottomCenter, topCenter, t);
                if (!IsGraphicRaycastLocationValid(
                        graphic,
                        point,
                        eventCamera))
                {
                    continue;
                }

                firstValid = firstValid < 0 ? index : firstValid;
                lastValid = index;
            }

            if (firstValid < 0 || lastValid < firstValid)
            {
                return false;
            }

            float lowerT;
            if (firstValid == 0)
            {
                lowerT = 0f;
            }
            else
            {
                float invalidT = (firstValid - 1f) / samples;
                float validT = firstValid / (float)samples;
                lowerT = FindRaycastTransition(
                    graphic,
                    eventCamera,
                    bottomCenter,
                    topCenter,
                    invalidT,
                    validT,
                    findFirstValid: true);
            }

            float upperT;
            if (lastValid == samples)
            {
                upperT = 1f;
            }
            else
            {
                float validT = lastValid / (float)samples;
                float invalidT = (lastValid + 1f) / samples;
                upperT = FindRaycastTransition(
                    graphic,
                    eventCamera,
                    bottomCenter,
                    topCenter,
                    validT,
                    invalidT,
                    findFirstValid: false);
            }

            float span = upperT - lowerT;
            if (span <= 0.001f)
            {
                return false;
            }

            float inset = Mathf.Max(0.001f, span * 0.02f);
            float outside = Mathf.Max(0.001f, span * 0.02f);
            lowerBoundary = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                lowerT);
            upperBoundary = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                upperT);
            lowerInside = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                lowerT + inset);
            middleInside = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                (lowerT + upperT) * 0.5f);
            upperInside = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                upperT - inset);
            lowerOutside = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                lowerT - outside);
            upperOutside = Vector2.LerpUnclamped(
                bottomCenter,
                topCenter,
                upperT + outside);
            return true;
        }

        private static float FindRaycastTransition(
            Graphic graphic,
            Camera eventCamera,
            Vector2 bottomCenter,
            Vector2 topCenter,
            float lowerT,
            float upperT,
            bool findFirstValid)
        {
            for (int iteration = 0; iteration < 24; iteration++)
            {
                float middleT = (lowerT + upperT) * 0.5f;
                Vector2 point = Vector2.LerpUnclamped(
                    bottomCenter,
                    topCenter,
                    middleT);
                bool valid = IsGraphicRaycastLocationValid(
                    graphic,
                    point,
                    eventCamera);
                if (findFirstValid)
                {
                    if (valid)
                    {
                        upperT = middleT;
                    }
                    else
                    {
                        lowerT = middleT;
                    }
                }
                else if (valid)
                {
                    lowerT = middleT;
                }
                else
                {
                    upperT = middleT;
                }
            }

            return findFirstValid ? upperT : lowerT;
        }

        private static bool IsGraphicRaycastLocationValid(
            Graphic graphic,
            Vector2 screenPoint,
            Camera eventCamera)
        {
            return graphic != null
                && graphic.isActiveAndEnabled
                && graphic.raycastTarget
                && RectTransformUtility.RectangleContainsScreenPoint(
                    graphic.rectTransform,
                    screenPoint,
                    eventCamera)
                && graphic.Raycast(screenPoint, eventCamera);
        }

        private static void VerifyPointerDragIsolation(
            EventSystem eventSystem,
            ScrollRect detailScroll,
            BuildItemTrayPreviewView tray,
            ScrollRect trayScroll,
            BuildGridInteractionPreviewController controller,
            Vector2 start,
            Vector2 end,
            int pointerId,
            float trayPosition,
            string label)
        {
            detailScroll.StopMovement();
            detailScroll.velocity = Vector2.zero;
            detailScroll.verticalNormalizedPosition = 0.75f;
            float detailBefore = detailScroll.verticalNormalizedPosition;
            bool trayDragBefore = ReadTrayPointerDragging(tray);
            bool controllerDragBefore =
                controller?.IsTrayItemDragActive == true;

            List<RaycastResult> hits =
                RaycastAll(eventSystem, start, pointerId);
            Check(hits.Count > 0 && hits[0].gameObject != null,
                label + " raycast produced no top target");
            PointerEventData pointer = new(eventSystem)
            {
                pointerId = pointerId,
                button = PointerEventData.InputButton.Left,
                position = start,
                pressPosition = start,
                delta = Vector2.zero,
                pointerCurrentRaycast = hits[0],
                pointerPressRaycast = hits[0],
                useDragThreshold = true
            };

            GameObject initializeHandler = ExecuteEvents.ExecuteHierarchy(
                hits[0].gameObject,
                pointer,
                ExecuteEvents.initializePotentialDrag);
            GameObject dragHandler =
                ExecuteEvents.GetEventHandler<IDragHandler>(
                    hits[0].gameObject);
            Check(ReferenceEquals(initializeHandler, detailScroll.gameObject)
                && ReferenceEquals(dragHandler, detailScroll.gameObject),
                label + " did not resolve detail ScrollRect drag handler");

            ExecuteEvents.Execute(
                dragHandler,
                pointer,
                ExecuteEvents.beginDragHandler);
            pointer.dragging = true;
            pointer.delta = end - start;
            pointer.position = end;
            ExecuteEvents.Execute(
                dragHandler,
                pointer,
                ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(
                dragHandler,
                pointer,
                ExecuteEvents.endDragHandler);
            pointer.dragging = false;
            Canvas.ForceUpdateCanvases();

            Check(!Mathf.Approximately(
                    detailBefore,
                    detailScroll.verticalNormalizedPosition),
                label + " did not move detail ScrollRect");
            AssertTrayInputUnchanged(
                tray,
                trayScroll,
                controller,
                trayPosition,
                trayDragBefore,
                controllerDragBefore,
                label);
        }

        private static void AssertTrayInputUnchanged(
            BuildItemTrayPreviewView tray,
            ScrollRect trayScroll,
            BuildGridInteractionPreviewController controller,
            float expectedPosition,
            bool expectedPointerDrag,
            bool expectedControllerDrag,
            string label)
        {
            Check(Mathf.Approximately(
                    expectedPosition,
                    trayScroll.verticalNormalizedPosition)
                && trayScroll.velocity == Vector2.zero
                && ReadTrayPointerDragging(tray) == expectedPointerDrag
                && (controller?.IsTrayItemDragActive == true)
                    == expectedControllerDrag,
                label + " leaked scroll/drag/inertia into tray");
        }

        private static bool ReadTrayPointerDragging(
            BuildItemTrayPreviewView tray)
        {
            FieldInfo field = typeof(BuildItemTrayPreviewView).GetField(
                "trayScrollPointerDragging",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Check(field != null,
                "tray pointer-drag state field missing");
            return field.GetValue(tray) is bool value && value;
        }

        private static Vector2 RectCenterToScreenPoint(
            RectTransform rect,
            Camera eventCamera)
        {
            Check(rect != null,
                "raycast RectTransform missing");
            return RectTransformUtility.WorldToScreenPoint(
                eventCamera,
                rect.TransformPoint(rect.rect.center));
        }

        private static Camera ResolveEventCamera(Transform target)
        {
            Canvas canvas = target != null
                ? target.GetComponentInParent<Canvas>()
                : null;
            Canvas rootCanvas = canvas != null ? canvas.rootCanvas : null;
            return rootCanvas != null
                && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? rootCanvas.worldCamera
                    : null;
        }

        private static ItemDetailViewModel Show(
            ItemSystemBattleSandboxItemDetailAdapter adapter,
            ItemSystemBattleSandboxViewRow row,
            string placementId,
            ItemSystemBattleSandboxBoardAuthority authority)
        {
            Check(adapter.Show(
                    row.BaseItemId,
                    row.ItemInstanceId,
                    placementId,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    authority.CurrentCoreEffectRuntimeState),
                row.BaseItemId + " detail Show failed: "
                + adapter.LastDiagnosticCode);
            Check(adapter.IsVisible && adapter.LastProjectedModel != null,
                row.BaseItemId + " detail was not visible");
            return adapter.LastProjectedModel;
        }

        private static ItemSystemBattleSandboxViewRow FindRow(
            ItemSystemBattleSandboxViewProjectionResult projection,
            string baseItemId) => projection.Rows.Single(row => row != null
                && string.Equals(row.BaseItemId, baseItemId,
                    StringComparison.Ordinal));

        private static ItemDetailPanelView FindAuthoredPanel(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<ItemDetailPanelView>(true))
                .FirstOrDefault(panel =>
                    FindByName(panel.transform, "FaMenBuildRowsRoot") != null
                    && FindByName(panel.transform, "QiLeiBuildRowsRoot") != null);
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
                    preview.Snapshot?.FindPlacement("P_BOARD_" + baseItemId);
                if (preview.Accepted
                    && placement != null
                    && placement.isLit == requireLit)
                {
                    return new PlacementCandidate(
                        new ItemShapeCell(x, y),
                        rotation);
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
                        placementId,
                        new ItemShapeCell(x, y),
                        rotation);
                if (result.Accepted && result.Changed)
                {
                    return true;
                }
            }
            return false;
        }

        private static Text TextByName(Transform root, string name)
        {
            return FindByName(root, name)?.GetComponent<Text>();
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(value =>
                    string.Equals(value.name, name, StringComparison.Ordinal));
        }

        private static string PlainText(Text text)
        {
            return text == null
                ? string.Empty
                : Regex.Replace(text.text ?? string.Empty, "<[^>]+>", string.Empty);
        }

        private static string NormalizeDescription(string value)
        {
            return string.Join(
                " ",
                (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0));
        }

        private static bool ContainsMachineFact(string value)
        {
            string text = value ?? string.Empty;
            return text.Contains("Build资格", StringComparison.Ordinal)
                || text.Contains("权威轨道", StringComparison.Ordinal)
                || text.Contains("当前阶段", StringComparison.Ordinal)
                || text.Contains("下一阶段", StringComparison.Ordinal)
                || text.Contains("sourceIsCountedFact", StringComparison.Ordinal)
                || text.Contains("qualifiedIsCountedFact", StringComparison.Ordinal)
                || text.Contains("famen:", StringComparison.Ordinal)
                || text.Contains("qilei:", StringComparison.Ordinal);
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Check(string.Equals(
                        HashFile(ProjectPath(pair.Key)),
                        pair.Value,
                        StringComparison.Ordinal),
                    "protected hash changed: " + pair.Key);
            }
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
            bool pass = Results.All(value => value.Passed);
            StringBuilder report = new();
            report.AppendLine("# ItemDetailLegacyBuildPresentationParity01 Report")
                .AppendLine()
                .AppendLine("Status: " + (pass ? "PASS" : "FAIL"))
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemDetailLegacyBuildPresentationParity01`")
                .AppendLine("- Close mode: `SIMPLE_DIRECT_CLOSE`")
                .AppendLine("- Player headings: `法门构筑` / `器类构筑`")
                .AppendLine("- QualifiedBuild typed/debug authority retained")
                .AppendLine("- FirstMissingLayer: `Presentation / Build body compatibility + authored Build state lifecycle + outside-dismiss frame geometry/input consumption`")
                .AppendLine("- Scroll input: existing direct wheel/touch behavior retained; no ScrollRect routing change")
                .AppendLine("- User handtest: `WAITING`")
                .AppendLine()
                .AppendLine("## Scenarios")
                .AppendLine()
                .AppendLine("| Id | Result | Marker | Detail |")
                .AppendLine("|---|---|---|---|");
            foreach (ScenarioResult result in Results)
            {
                report.Append("| ").Append(result.Id)
                    .Append(" | ").Append(result.Passed ? "PASS" : "FAIL")
                    .Append(" | `").Append(result.Marker)
                    .Append("` | ").Append(EscapeMarkdown(result.Detail))
                    .AppendLine(" |");
            }
            report.AppendLine()
                .AppendLine("## Real samples")
                .AppendLine();
            foreach (string evidence in SampleEvidence)
            {
                report.Append("- ").AppendLine(evidence);
            }
            report.AppendLine()
                .AppendLine("Final gate: `SIMPLE_DIRECT_CLOSE / USER_HANDTEST_WAITING`.");
            Write(ReportPath, report.ToString());

            StringBuilder spec = new(
                "scenarioId,result,marker,detail\n");
            foreach (ScenarioResult result in Results)
            {
                spec.Append(Csv(result.Id)).Append(',')
                    .Append(Csv(result.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(result.Marker)).Append(',')
                    .Append(Csv(result.Detail)).Append('\n');
            }
            Write(SpecPath, spec.ToString());

            Write(
                LeakPath,
                "# ItemDetailLegacyBuildPresentationParity01 Leak Check\n\n"
                + "Status: " + (pass ? "PASS" : "FAIL") + "\n\n"
                + "- `ItemDetailPanelView.cs` changed only for local runtime "
                + "Build authored-state/theme compatibility, exact visible-frame "
                + "outside-dismiss geometry, and same-PointerDown consumption.\n"
                + "- Existing direct wheel/touch scrolling was restored; no "
                + "Viewport Graphic, ScrollRect routing or inertia change remains.\n"
                + "- `ItemDetailSectionView.cs` unchanged.\n"
                + "- BattleSandbox Scene and ItemDetailPanel Prefab unchanged.\n"
                + "- `BuildItemTrayPreviewView.cs` and tray physics/layout unchanged.\n"
                + "- QualifiedBuild contract/assembler, ItemSystemSnapshot and "
                + "ItemBuildSynergyRules unchanged.\n"
                + "- No Scene Builder, Prefab migration, RunFlow, Save, Reward, "
                + "Enemy, Battle, RNG, commit, tag, push, reset or rollback.\n");

            Write(
                ManualPath,
                "# ItemDetailLegacyBuildPresentationParity01 Manual Test\n\n"
                + "Status: `WAITING_USER_HANDTEST`\n\n"
                + "Scene: `" + ScenePath + "`\n\n"
                + "1. I001：仅显示器类构筑，符类进度与 2/4 行正确。\n"
                + "2. I004：法门构筑与器类构筑同时可见且可完整滚动；"
                + "震雷名、成员、进度、2/4/6 与令类 2/4 正确。\n"
                + "3. I006：法门/器类两个玩家区块均隐藏。\n"
                + "4. I009：仅显示法门构筑；离火法名、成员、"
                + "进度与 2/4/6 正确。\n"
                + "5. Inventory -> Board 未点亮 -> Board 点亮 -> Move -> Return："
                + "进度和阶段遮罩实时刷新。\n"
                + "6. 连续点击不同资格道具，无上一件文字残留；"
                + "I031 不出现普通 Build 内容。\n"
                + "7. I004 长内容滚轮、鼠标拖动、触摸拖动仍按既有方式工作；"
                + "拖动、滚轮、惯性、PointerUp 均不会关闭详情。\n"
                + "8. 九点矩阵：中心、左/右/上/下内点均不关闭；"
                + "左/右/上/下外点均关闭。\n"
                + "9. I004 分别滚到顶部和底部后重复九点矩阵；边界始终是"
                + "完整可见弹窗外框，不随 Content 高度或滚动位置变化。\n"
                + "10. 外点关闭的同一次按下不选中或拖动下层道具；松开后"
                + "下一次点击恢复。显式关闭按钮与连续开关两次也正常。\n");
        }

        private static string Read(string relativePath) =>
            File.ReadAllText(ProjectPath(relativePath), Encoding.UTF8);

        private static void Write(string relativePath, string content)
        {
            string path = ProjectPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("report directory missing"));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string ProjectPath(string relativePath) =>
            Path.Combine(
                Directory.GetParent(Application.dataPath)?.FullName
                    ?? throw new InvalidOperationException("project root missing"),
                relativePath.Replace('/', Path.DirectorySeparatorChar));

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string Csv(string value) =>
            "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

        private static string EscapeMarkdown(string value) =>
            (value ?? string.Empty).Replace("|", "\\|").Replace("\n", " ");

        private sealed class EventSystemRaycastHarness : IDisposable
        {
            private readonly Canvas rootCanvas;
            private readonly Scene targetScene;
            private readonly RenderMode originalRootRenderMode;
            private readonly Camera originalRootCamera;
            private readonly float originalRootPlaneDistance;
            private readonly RectTransform rootCanvasRect;
            private readonly Vector3 originalRootCanvasScale;
            private readonly Canvas[] sceneCanvases;
            private readonly Camera[] originalCanvasCameras;
            private readonly GraphicRaycaster[] sceneRaycasters;
            private readonly bool[] originalRaycasterEnabled;
            private readonly Graphic trayInputGraphic;
            private readonly bool originalTrayRaycastTarget;
            private readonly List<BaseRaycaster> addedRaycasters = new();
            private readonly Camera eventCamera;
            private readonly GameObject cameraObject;
            private readonly RenderTexture targetTexture;

            public EventSystemRaycastHarness(
                Scene scene,
                ItemDetailPanelView panel,
                ScrollRect trayScroll)
            {
                targetScene = scene;
                Canvas nearestCanvas =
                    panel != null ? panel.GetComponentInParent<Canvas>() : null;
                rootCanvas = nearestCanvas != null
                    ? nearestCanvas.rootCanvas
                    : null;
                Check(rootCanvas != null,
                    "detail EventSystem root Canvas missing");

                sceneCanvases = scene.GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<Canvas>(true))
                    .Where(value => value != null)
                    .Distinct()
                    .ToArray();
                originalCanvasCameras = sceneCanvases
                    .Select(value => value.worldCamera)
                    .ToArray();
                sceneRaycasters = scene.GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<GraphicRaycaster>(true))
                    .Where(value => value != null)
                    .Distinct()
                    .ToArray();
                originalRaycasterEnabled = sceneRaycasters
                    .Select(value => value.enabled)
                    .ToArray();
                originalRootRenderMode = rootCanvas.renderMode;
                originalRootCamera = rootCanvas.worldCamera;
                originalRootPlaneDistance = rootCanvas.planeDistance;
                rootCanvasRect = rootCanvas.transform as RectTransform;
                originalRootCanvasScale = rootCanvasRect != null
                    ? rootCanvasRect.localScale
                    : Vector3.one;

                trayInputGraphic =
                    trayScroll != null
                        ? trayScroll.GetComponent<Graphic>()
                        : null;
                Check(trayInputGraphic != null,
                    "authored tray ScrollRect input Graphic missing");
                originalTrayRaycastTarget = trayInputGraphic.raycastTarget;
                trayInputGraphic.raycastTarget = true;

                cameraObject = new GameObject(
                    "ItemDetailEventSystemRaycastCamera",
                    typeof(Camera));
                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                eventCamera = cameraObject.GetComponent<Camera>();
                eventCamera.enabled = true;
                eventCamera.clearFlags = CameraClearFlags.SolidColor;
                eventCamera.backgroundColor = Color.clear;
                eventCamera.cullingMask = ~0;
                eventCamera.orthographic = true;
                eventCamera.transform.position =
                    new Vector3(0f, 0f, -1000f);
                targetTexture = new RenderTexture(
                    1080,
                    1920,
                    0,
                    RenderTextureFormat.ARGB32)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = "ItemDetailEventSystemRaycastTarget"
                };
                targetTexture.Create();
                eventCamera.targetTexture = targetTexture;

                rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                rootCanvas.worldCamera = eventCamera;
                rootCanvas.planeDistance = 100f;
                if (rootCanvasRect != null)
                {
                    rootCanvasRect.localScale = Vector3.one;
                }
                foreach (Canvas canvas in sceneCanvases)
                {
                    canvas.worldCamera = eventCamera;
                }
                for (int index = 0;
                     index < sceneRaycasters.Length
                     && index < originalRaycasterEnabled.Length;
                     index++)
                {
                    GraphicRaycaster raycaster = sceneRaycasters[index];
                    if (raycaster == null
                        || !originalRaycasterEnabled[index])
                    {
                        continue;
                    }

                    raycaster.enabled = false;
                    raycaster.enabled = true;
                }

                Refresh();
            }

            public void Refresh()
            {
                EnsureRaycastersRegistered();
                Canvas.ForceUpdateCanvases();
                if (eventCamera != null)
                {
                    eventCamera.Render();
                }
                Canvas.ForceUpdateCanvases();
            }

            private void EnsureRaycastersRegistered()
            {
                if (!targetScene.IsValid() || !targetScene.isLoaded)
                {
                    return;
                }

                List<BaseRaycaster> registered =
                    RaycasterManager.GetRaycasters();
                foreach (GraphicRaycaster raycaster in targetScene
                             .GetRootGameObjects()
                             .SelectMany(root => root
                                 .GetComponentsInChildren<GraphicRaycaster>(
                                     true))
                             .Where(value => value != null
                                 && value.isActiveAndEnabled))
                {
                    if (registered.Contains(raycaster))
                    {
                        continue;
                    }

                    registered.Add(raycaster);
                    addedRaycasters.Add(raycaster);
                }
            }

            public void Dispose()
            {
                if (trayInputGraphic != null)
                {
                    trayInputGraphic.raycastTarget =
                        originalTrayRaycastTarget;
                }

                if (rootCanvas != null)
                {
                    rootCanvas.renderMode = originalRootRenderMode;
                    rootCanvas.worldCamera = originalRootCamera;
                    rootCanvas.planeDistance = originalRootPlaneDistance;
                    if (rootCanvasRect != null)
                    {
                        rootCanvasRect.localScale =
                            originalRootCanvasScale;
                    }
                }

                for (int index = 0;
                     index < sceneCanvases.Length
                     && index < originalCanvasCameras.Length;
                     index++)
                {
                    if (sceneCanvases[index] != null)
                    {
                        sceneCanvases[index].worldCamera =
                            originalCanvasCameras[index];
                    }
                }
                for (int index = 0;
                     index < sceneRaycasters.Length
                     && index < originalRaycasterEnabled.Length;
                     index++)
                {
                    if (sceneRaycasters[index] != null)
                    {
                        sceneRaycasters[index].enabled =
                            originalRaycasterEnabled[index];
                    }
                }
                List<BaseRaycaster> registered =
                    RaycasterManager.GetRaycasters();
                foreach (BaseRaycaster raycaster in addedRaycasters)
                {
                    registered.Remove(raycaster);
                }
                addedRaycasters.Clear();

                if (cameraObject != null)
                {
                    if (eventCamera != null)
                    {
                        eventCamera.targetTexture = null;
                    }
                }
                if (targetTexture != null)
                {
                    targetTexture.Release();
                    UnityEngine.Object.DestroyImmediate(targetTexture);
                }
                if (cameraObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                }

                Canvas.ForceUpdateCanvases();
            }
        }

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
                Detail = detail ?? string.Empty;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
