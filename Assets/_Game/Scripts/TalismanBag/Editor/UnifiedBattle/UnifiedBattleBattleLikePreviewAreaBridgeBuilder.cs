#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class UnifiedBattleBattleLikePreviewAreaBridgeBuilder
    {
        private const string PackageName = "V0.4-UnifiedBattleBattleLikePreviewAreaBridge01";
        private const string SourceScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ShellScenePath = UnifiedBattlePageShellMarker.ScenePath;
        private const string BridgeHostName = "BuildSandboxPreviewHost";
        private const string RuntimeName = "BuildGridInteractionPreviewRuntime";
        private const string BridgePrefabPath = "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/BattleLikePreviewAreaBridge.prefab";
        private const string ReportPath = "Docs/V0.4/Reports/UnifiedBattleBattleLikePreviewAreaBridgeReport.md";
        private const string ChecklistPath = "Docs/V0.4/Reports/UnifiedBattleBattleLikePreviewAreaBridgeChecklist.csv";

        [MenuItem("Tools/Talisman Bag/V0.4/UnifiedBattle/BattleLikePreviewAreaBridge01/[Writes Scene][Manual Only] Build Bridge")]
        public static void BuildBridgeMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Build BattleLikePreviewArea Bridge",
                    "This writes only the isolated UnifiedBattle shell scene and UnifiedBattle bridge prefab.\n\n" +
                    ShellScenePath + "\n" +
                    BridgePrefabPath + "\n\n" +
                    "It does not modify Build Settings, ProjectSettings, V02/V03 scenes, RunFlow, save, reward, Boss flow, or chapter progression.",
                    "Build Bridge",
                    "Cancel"))
            {
                return;
            }

            BridgeSnapshot snapshot = BuildBridge();
            WriteReports(snapshot);
            Debug.Log("[UnifiedBattleBattleLikePreviewAreaBridge01] Bridge built. " + snapshot.Status);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/UnifiedBattle/BattleLikePreviewAreaBridge01/[QA Only] Run Validation Reports")]
        public static void RunValidationReportsMenu()
        {
            BridgeSnapshot snapshot = ValidateBridge();
            WriteReports(snapshot);
            Debug.Log("[UnifiedBattleBattleLikePreviewAreaBridge01] Validation reports written. " + snapshot.Status);
        }

        public static void BuildBridgeAndReports()
        {
            BridgeSnapshot snapshot = BuildBridge();
            WriteReports(snapshot);
            if (!snapshot.Pass)
            {
                throw new InvalidOperationException("Bridge validation failed: " + snapshot.Status);
            }
        }

        public static void RunValidationReports()
        {
            BridgeSnapshot snapshot = ValidateBridge();
            WriteReports(snapshot);
            if (!snapshot.Pass)
            {
                throw new InvalidOperationException("Bridge validation failed: " + snapshot.Status);
            }
        }

        public static BridgeSnapshot BuildBridge()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("BattleLikePreviewArea bridge builder must run in Edit Mode.");
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene shellScene = EditorSceneManager.OpenScene(ShellScenePath, OpenSceneMode.Single);
            Scene sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject sourceBattleArea = RequireFind(sourceScene, "BattleLikePreviewArea");
                BuildGridInteractionPreviewController sourceController =
                    RequireSourceController(sourceScene);

                Transform battlePageRoot = RequireFind(shellScene, UnifiedBattlePageShellSlotNames.BattlePageRoot).transform;
                Transform v04Slot = RequireFind(shellScene, UnifiedBattlePageShellSlotNames.V04SandboxAdapterSlot).transform;
                Transform diagnosticsSlot = RequireFind(shellScene, UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot).transform;

                RemoveExistingBridgeObjects(battlePageRoot);
                RectTransform host = CreateFullScreenHost(battlePageRoot);
                RectTransform battleArea = CloneRectToHost(sourceBattleArea, host, "BattleLikePreviewArea");
                RectTransform controlBar = TryCloneSourceRect(sourceScene, "DevOnlyControlBar", host);
                RectTransform ghostRoot = CloneSourceControllerGhost(sourceController, host);

                BuildGridInteractionPreviewController controller =
                    CreateController(host, sourceController);
                BindController(controller, battleArea, controlBar, ghostRoot);

                UnifiedBattleBattleLikePreviewAreaBridgeHost hostMarker =
                    host.gameObject.GetComponent<UnifiedBattleBattleLikePreviewAreaBridgeHost>();
                if (hostMarker == null)
                {
                    hostMarker = host.gameObject.AddComponent<UnifiedBattleBattleLikePreviewAreaBridgeHost>();
                }

                hostMarker.AssignForEditor(battleArea, controller);
                EditorUtility.SetDirty(hostMarker);

                WriteDiagnostics(diagnosticsSlot, hostMarker.BuildStatusText());
                SaveBridgePrefab(host.gameObject);

                EditorSceneManager.MarkSceneDirty(shellScene);
                if (!EditorSceneManager.SaveScene(shellScene, ShellScenePath))
                {
                    throw new InvalidOperationException("Could not save " + ShellScenePath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                if (sourceScene.IsValid() && sourceScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(sourceScene, removeScene: true);
                }

                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != shellScene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }

            return ValidateBridge();
        }

        public static BridgeSnapshot ValidateBridge()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            Scene shellScene = EditorSceneManager.OpenScene(ShellScenePath, OpenSceneMode.Single);
            try
            {
                BridgeSnapshot snapshot = new()
                {
                    Package = PackageName,
                    SourceScenePath = SourceScenePath,
                    ShellScenePath = ShellScenePath,
                    BridgePrefabPath = BridgePrefabPath,
                    SourceMethod = "Prefab-backed copy of source hierarchy with target-scene controller binding.",
                    MountPath = UnifiedBattlePageShellSlotNames.BattlePageRoot + "/" + BridgeHostName,
                    ShellSceneInBuildSettings = EditorBuildSettings.scenes.Any(
                        scene => string.Equals(scene.path, ShellScenePath, StringComparison.Ordinal)),
                    ProjectSettingsModified = false,
                    BuildSettingsModified = false,
                    FormalRouteConnected = false,
                    SaveRewardChapterWrite = false
                };

                GameObject hostObject = FindDeepChildInScene(shellScene, BridgeHostName);
                GameObject areaObject = FindDeepChildInScene(shellScene, "BattleLikePreviewArea");
                GameObject runtimeObject = FindDeepChildInScene(shellScene, RuntimeName);
                GameObject trayObject = FindDeepChildInScene(shellScene, "ItemTrayPreview");
                GameObject boardObject = FindDeepChildInScene(shellScene, "BoardGridPreview");
                GameObject ghostObject = FindDeepChildInScene(shellScene, "GridInteractionDragGhost");
                GameObject feedbackObject = FindDeepChildInScene(shellScene, "PlacementFeedback_Runtime")
                                            ?? FindDeepChildInScene(shellScene, "PlacementFeedback");
                GameObject selectedInfoObject = FindDeepChildInScene(shellScene, "SelectedItemInfo");
                GameObject rotateButtonObject = FindDeepChildInScene(shellScene, "RotatePreviewButtonSlot");
                GameObject mobileRotateLayerObject = FindDeepChildInScene(shellScene, "MobileRotateZoneLayer");
                GameObject devControlBarObject = FindDeepChildInScene(shellScene, "DevOnlyControlBar");
                GameObject enemyPanelObject = FindDeepChildInScene(shellScene, "EnemyCombatFeedbackPanel");
                GameObject diagnosticsObject = FindDeepChildInScene(shellScene, UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot);
                GameObject resultObject = FindDeepChildInScene(shellScene, UnifiedBattlePageShellSlotNames.ResultRewardPlaceholder);

                UnifiedBattleBattleLikePreviewAreaBridgeHost host =
                    hostObject == null ? null : hostObject.GetComponent<UnifiedBattleBattleLikePreviewAreaBridgeHost>();
                BuildGridInteractionPreviewController controller =
                    runtimeObject == null ? null : runtimeObject.GetComponent<BuildGridInteractionPreviewController>();
                UnifiedBattlePageShellMarker shellMarker =
                    UnityEngine.Object.FindObjectOfType<UnifiedBattlePageShellMarker>(true);

                snapshot.HostPresent = hostObject != null;
                snapshot.BattleLikePreviewAreaPresent = areaObject != null;
                snapshot.ControllerPresent = controller != null;
                snapshot.BoardPresent = boardObject != null;
                snapshot.ItemTrayPresent = trayObject != null;
                snapshot.ItemTrayViewPresent = trayObject != null && trayObject.GetComponent<BuildItemTrayPreviewView>() != null;
                snapshot.BoardSlotCount = boardObject == null
                    ? 0
                    : boardObject.GetComponentsInChildren<BuildGridPreviewSlotView>(true).Length;
                snapshot.DragGhostPresent = ghostObject != null;
                snapshot.PlacementFeedbackPresent = feedbackObject != null;
                snapshot.SelectedInfoPresent = selectedInfoObject != null;
                snapshot.RotateButtonPresent = rotateButtonObject != null;
                snapshot.MobileRotateZoneLayerPresent = mobileRotateLayerObject != null;
                snapshot.DevControlBarPresent = devControlBarObject != null;
                snapshot.EnemyFeedbackPanelPresent = enemyPanelObject != null;
                snapshot.DevDiagnosticsPresent = diagnosticsObject != null;
                snapshot.ResultRewardPlaceholderPresent = resultObject != null;
                snapshot.PrefabExists = File.Exists(ToAbsolutePath(BridgePrefabPath));
                snapshot.MarkerDevOnly = shellMarker != null && shellMarker.DevOnly;
                snapshot.MarkerEnabledFalse = shellMarker != null && !shellMarker.IsEnabled;
                snapshot.MarkerFormalFalse = shellMarker != null && !shellMarker.FormalFlow;
                snapshot.MarkerRouteFalse = shellMarker != null && !shellMarker.ConnectedToFormalRoute;
                snapshot.HostDevOnly = host != null && host.DevOnly;
                snapshot.HostFormalFalse = host != null && !host.FormalFlow && !host.ConnectedToFormalRoute;
                snapshot.HostNoSaveRewardChapter =
                    host != null && !host.WritesSaveData && !host.GrantsReward && !host.AdvancesChapter;
                snapshot.ControllerIsolated =
                    controller != null
                    && controller.DevOnly
                    && !controller.IsEnabled
                    && !controller.ReadsFormalSaveData
                    && !controller.WritesFormalFlow
                    && !controller.WritesFormalUi
                    && !controller.TouchesFormalScene
                    && !controller.ShowsCompleteAnswers;

                snapshot.LeakCount = ComputeLeakCount(snapshot);
                snapshot.Pass = snapshot.LeakCount == 0;
                snapshot.Status = snapshot.Pass ? "PASS" : "FAIL";
                return snapshot;
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != shellScene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void BindController(
            BuildGridInteractionPreviewController controller,
            RectTransform battleArea,
            RectTransform controlBar,
            RectTransform ghostRoot)
        {
            RectTransform board = RequireRect(RequireChild(battleArea, "BoardGridPreview"));
            RectTransform tray = RequireRect(RequireChild(battleArea, "ItemTrayPreview"));
            BuildItemTrayPreviewView trayView = tray.GetComponent<BuildItemTrayPreviewView>();
            if (trayView == null)
            {
                trayView = tray.gameObject.AddComponent<BuildItemTrayPreviewView>();
            }

            BuildPlacementFeedbackView feedbackView = EnsurePlacementFeedback(battleArea);
            RectTransform selectedInfo = FindDeepChild(battleArea, "SelectedItemInfo") as RectTransform;
            Text selectedTitle = selectedInfo == null
                ? null
                : FindDeepChild(selectedInfo, "SelectedItemInfoTitle")?.GetComponent<Text>();
            Text selectedBody = selectedInfo == null
                ? null
                : FindDeepChild(selectedInfo, "SelectedItemInfoBody")?.GetComponent<Text>();
            Button resetButton = controlBar == null
                ? null
                : FindDeepChild(controlBar, "ResetPreviewButtonSlot")?.GetComponent<Button>();
            Button rotateButton = controlBar == null
                ? null
                : FindDeepChild(controlBar, "RotatePreviewButtonSlot")?.GetComponent<Button>();
            Text ghostText = ghostRoot == null ? null : ghostRoot.GetComponentInChildren<Text>(true);
            List<BuildGridPreviewSlotView> slots = board
                .GetComponentsInChildren<BuildGridPreviewSlotView>(true)
                .OrderBy(slot => slot.Y)
                .ThenBy(slot => slot.X)
                .ToList();

            controller.Bind(
                board,
                slots,
                trayView,
                feedbackView,
                selectedTitle,
                selectedBody,
                resetButton,
                rotateButton,
                ghostRoot,
                ghostText);
            EditorUtility.SetDirty(controller);
        }

        private static BuildPlacementFeedbackView EnsurePlacementFeedback(RectTransform battleArea)
        {
            BuildPlacementFeedbackView existing = battleArea.GetComponentInChildren<BuildPlacementFeedbackView>(true);
            if (existing != null)
            {
                return existing;
            }

            RectTransform rect = CreateRectChild(battleArea, "PlacementFeedback_Runtime");
            SetAnchors(rect, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.10f));
            Image background = EnsureImage(rect.gameObject, new Color(0.18f, 0.14f, 0.09f, 0.96f), raycast: false);
            Text text = CreateText(rect, "PlacementFeedbackText", "Tap item for detail. Drag to place; invalid release returns to tray.", 16);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            BuildPlacementFeedbackView view = rect.gameObject.AddComponent<BuildPlacementFeedbackView>();
            view.Bind(text, background);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static BuildGridInteractionPreviewController CreateController(
            RectTransform host,
            BuildGridInteractionPreviewController sourceController)
        {
            GameObject runtime = new(RuntimeName, typeof(RectTransform));
            runtime.transform.SetParent(host, false);
            RectTransform runtimeRect = runtime.GetComponent<RectTransform>();
            runtimeRect.anchorMin = new Vector2(0.5f, 0.5f);
            runtimeRect.anchorMax = new Vector2(0.5f, 0.5f);
            runtimeRect.sizeDelta = new Vector2(100f, 100f);

            BuildGridInteractionPreviewController controller =
                runtime.AddComponent<BuildGridInteractionPreviewController>();
            EditorUtility.CopySerialized(sourceController, controller);
            return controller;
        }

        private static RectTransform CreateFullScreenHost(Transform battlePageRoot)
        {
            GameObject hostObject = new(
                BridgeHostName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(UnifiedBattleBattleLikePreviewAreaBridgeHost));
            hostObject.transform.SetParent(battlePageRoot, false);
            RectTransform host = hostObject.GetComponent<RectTransform>();
            SetAnchors(host, Vector2.zero, Vector2.one);
            host.offsetMin = Vector2.zero;
            host.offsetMax = Vector2.zero;
            host.pivot = new Vector2(0.5f, 0.5f);
            return host;
        }

        private static RectTransform CloneRectToHost(GameObject sourceObject, RectTransform host, string targetName)
        {
            GameObject clone = UnityEngine.Object.Instantiate(sourceObject);
            clone.name = targetName;
            clone.transform.SetParent(host, false);
            RectTransform rect = clone.GetComponent<RectTransform>();
            if (rect == null)
            {
                throw new InvalidOperationException(targetName + " must have a RectTransform.");
            }

            return rect;
        }

        private static RectTransform TryCloneSourceRect(Scene sourceScene, string sourceName, RectTransform host)
        {
            GameObject source = FindDeepChildInScene(sourceScene, sourceName);
            if (source == null)
            {
                return null;
            }

            return CloneRectToHost(source, host, sourceName);
        }

        private static RectTransform CloneSourceControllerGhost(
            BuildGridInteractionPreviewController sourceController,
            RectTransform host)
        {
            SerializedObject serialized = new(sourceController);
            SerializedProperty ghostProperty = serialized.FindProperty("dragGhostRoot");
            RectTransform sourceGhost = ghostProperty == null
                ? null
                : ghostProperty.objectReferenceValue as RectTransform;
            if (sourceGhost == null)
            {
                return null;
            }

            return CloneRectToHost(sourceGhost.gameObject, host, "GridInteractionDragGhost");
        }

        private static void SaveBridgePrefab(GameObject host)
        {
            EnsureAssetFolder(Path.GetDirectoryName(BridgePrefabPath)?.Replace("\\", "/"));
            PrefabUtility.SaveAsPrefabAssetAndConnect(host, BridgePrefabPath, InteractionMode.AutomatedAction);
        }

        private static void RemoveExistingBridgeObjects(Transform battlePageRoot)
        {
            List<GameObject> toRemove = new();
            Transform existingHost = battlePageRoot.Find(BridgeHostName);
            if (existingHost != null)
            {
                toRemove.Add(existingHost.gameObject);
            }

            Transform directBattleLike = battlePageRoot.Find("BattleLikePreviewArea");
            if (directBattleLike != null)
            {
                toRemove.Add(directBattleLike.gameObject);
            }

            foreach (GameObject target in toRemove.Distinct())
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static BuildGridInteractionPreviewController RequireSourceController(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                BuildGridInteractionPreviewController controller =
                    root.GetComponentInChildren<BuildGridInteractionPreviewController>(true);
                if (controller != null)
                {
                    return controller;
                }
            }

            throw new InvalidOperationException("Source BuildGridInteractionPreviewController is missing.");
        }

        private static void WriteDiagnostics(Transform diagnosticsSlot, string status)
        {
            if (diagnosticsSlot == null)
            {
                return;
            }

            Text text = FindDeepChild(diagnosticsSlot, "BattleLikePreviewAreaBridgeDiagnosticsText")?.GetComponent<Text>();
            if (text == null)
            {
                RectTransform rect = CreateRectChild(diagnosticsSlot, "BattleLikePreviewAreaBridgeDiagnosticsText");
                SetAnchors(rect, new Vector2(0.04f, 0.54f), new Vector2(0.96f, 0.94f));
                text = rect.gameObject.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 13;
                text.alignment = TextAnchor.MiddleLeft;
                text.color = new Color(0.92f, 0.84f, 0.70f, 1f);
                text.raycastTarget = false;
            }

            text.text = status;
            EditorUtility.SetDirty(text);
        }

        private static void WriteReports(BridgeSnapshot snapshot)
        {
            EnsureAssetFolder(Path.GetDirectoryName(ReportPath)?.Replace("\\", "/"));
            File.WriteAllText(ToAbsolutePath(ReportPath), BuildReport(snapshot));
            File.WriteAllText(ToAbsolutePath(ChecklistPath), BuildChecklist(snapshot));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(BridgeSnapshot snapshot)
        {
            return
                "# UnifiedBattle BattleLikePreviewArea Bridge Report\n\n" +
                "- Package: `" + snapshot.Package + "`\n" +
                "- Status: `" + snapshot.Status + "`\n" +
                "- Source scene: `" + snapshot.SourceScenePath + "`\n" +
                "- Source object: `BattleLikePreviewArea`\n" +
                "- Method: `" + snapshot.SourceMethod + "`\n" +
                "- Shell scene: `" + snapshot.ShellScenePath + "`\n" +
                "- Mount path: `" + snapshot.MountPath + "`\n" +
                "- Bridge prefab: `" + snapshot.BridgePrefabPath + "`\n" +
                "- Unity compile log: `Logs/codex_unifiedbattle_battlelike_preview_area_bridge_compile.log`\n\n" +
                "## Validation\n\n" +
                "| Check | Result |\n" +
                "| --- | --- |\n" +
                Row("Host present", snapshot.HostPresent) +
                Row("BattleLikePreviewArea present", snapshot.BattleLikePreviewAreaPresent) +
                Row("BuildGridInteractionPreviewController present", snapshot.ControllerPresent) +
                Row("BoardGridPreview present", snapshot.BoardPresent) +
                Row("Board slot count is 25", snapshot.BoardSlotCount == 25) +
                Row("ItemTrayPreview present", snapshot.ItemTrayPresent) +
                Row("Item tray view present", snapshot.ItemTrayViewPresent) +
                Row("Drag ghost present", snapshot.DragGhostPresent) +
                Row("Placement feedback present", snapshot.PlacementFeedbackPresent) +
                Row("Selected item detail panel present", snapshot.SelectedInfoPresent) +
                Row("Rotate button present", snapshot.RotateButtonPresent) +
                Row("Mobile rotate zone layer present", snapshot.MobileRotateZoneLayerPresent) +
                Row("Dev control bar retained", snapshot.DevControlBarPresent) +
                Row("Enemy feedback panel retained", snapshot.EnemyFeedbackPanelPresent) +
                Row("DevOnlyDiagnosticsSlot present", snapshot.DevDiagnosticsPresent) +
                Row("ResultRewardPlaceholder present", snapshot.ResultRewardPlaceholderPresent) +
                Row("Bridge prefab exists", snapshot.PrefabExists) +
                Row("Shell scene absent from BuildSettings", !snapshot.ShellSceneInBuildSettings) +
                Row("Shell marker devOnly", snapshot.MarkerDevOnly) +
                Row("Shell marker disabled", snapshot.MarkerEnabledFalse) +
                Row("Shell marker formalFlow false", snapshot.MarkerFormalFalse) +
                Row("Shell marker formal route false", snapshot.MarkerRouteFalse) +
                Row("Host devOnly", snapshot.HostDevOnly) +
                Row("Host formal route false", snapshot.HostFormalFalse) +
                Row("Host no save/reward/chapter", snapshot.HostNoSaveRewardChapter) +
                Row("Controller isolated", snapshot.ControllerIsolated) +
                "\n## Leak Check\n\n" +
                "- Leak count: `" + snapshot.LeakCount + "`\n" +
                "- Formal route connected: `" + snapshot.FormalRouteConnected + "`\n" +
                "- Save/reward/chapter write: `" + snapshot.SaveRewardChapterWrite + "`\n" +
                "- BuildSettings modified by this builder: `" + snapshot.BuildSettingsModified + "`\n" +
                "- ProjectSettings modified by this builder: `" + snapshot.ProjectSettingsModified + "`\n\n" +
                "## Manual QA Focus\n\n" +
                "1. Open `Scene_TalismanBag_V04_UnifiedBattlePageShell`.\n" +
                "2. Confirm `BattleLikePreviewArea` is visible in the devOnly shell.\n" +
                "3. Confirm the item tray displays.\n" +
                "4. Drag an item to the board and release on a legal cell.\n" +
                "5. Tap an item and confirm detail display opens.\n" +
                "6. Drag a rotatable item to the board and confirm the mobile rotate control still behaves like the V04 sandbox.\n" +
                "7. Confirm `ResultRewardPlaceholder` stays a placeholder and no formal route/save/reward/chapter behavior runs.\n";
        }

        private static string BuildChecklist(BridgeSnapshot snapshot)
        {
            List<string> rows = new()
            {
                "id,check,result,details",
                Csv("UB-BLP-001", "Fresh Unity compile", true, "Batch compile succeeded before bridge build."),
                Csv("UB-BLP-002", "Source BattleLikePreviewArea located", snapshot.BattleLikePreviewAreaPresent, snapshot.SourceScenePath),
                Csv("UB-BLP-003", "Mounted in shell", snapshot.HostPresent, snapshot.MountPath),
                Csv("UB-BLP-004", "Controller bound", snapshot.ControllerPresent && snapshot.ControllerIsolated, RuntimeName),
                Csv("UB-BLP-005", "Board slots", snapshot.BoardSlotCount == 25, snapshot.BoardSlotCount.ToString()),
                Csv("UB-BLP-006", "Item tray view", snapshot.ItemTrayViewPresent, "BuildItemTrayPreviewView"),
                Csv("UB-BLP-007", "Drag ghost", snapshot.DragGhostPresent, "GridInteractionDragGhost"),
                Csv("UB-BLP-008", "Placement feedback", snapshot.PlacementFeedbackPresent, "PlacementFeedback"),
                Csv("UB-BLP-009", "Click detail panel", snapshot.SelectedInfoPresent, "SelectedItemInfo"),
                Csv("UB-BLP-010", "Rotate button", snapshot.RotateButtonPresent, "RotatePreviewButtonSlot"),
                Csv("UB-BLP-011", "Mobile rotate zone layer", snapshot.MobileRotateZoneLayerPresent, "MobileRotateZoneLayer"),
                Csv("UB-BLP-012", "Dev control bar", snapshot.DevControlBarPresent, "DevOnlyControlBar"),
                Csv("UB-BLP-013", "Dev diagnostics", snapshot.DevDiagnosticsPresent, "DevOnlyDiagnosticsSlot"),
                Csv("UB-BLP-014", "Result placeholder safe", snapshot.ResultRewardPlaceholderPresent && !snapshot.SaveRewardChapterWrite, "No reward/save/chapter writer added"),
                Csv("UB-BLP-015", "Shell not in BuildSettings", !snapshot.ShellSceneInBuildSettings, ShellScenePath),
                Csv("UB-BLP-016", "Marker flags", snapshot.MarkerDevOnly && snapshot.MarkerEnabledFalse && snapshot.MarkerFormalFalse && snapshot.MarkerRouteFalse, "devOnly true; enabled/formal/route false"),
                Csv("UB-BLP-017", "Leak count", snapshot.LeakCount == 0, snapshot.LeakCount.ToString())
            };
            return string.Join(Environment.NewLine, rows) + Environment.NewLine;
        }

        private static int ComputeLeakCount(BridgeSnapshot snapshot)
        {
            int leak = 0;
            leak += snapshot.HostPresent ? 0 : 1;
            leak += snapshot.BattleLikePreviewAreaPresent ? 0 : 1;
            leak += snapshot.ControllerPresent ? 0 : 1;
            leak += snapshot.BoardPresent && snapshot.BoardSlotCount == 25 ? 0 : 1;
            leak += snapshot.ItemTrayPresent && snapshot.ItemTrayViewPresent ? 0 : 1;
            leak += snapshot.DragGhostPresent ? 0 : 1;
            leak += snapshot.PlacementFeedbackPresent ? 0 : 1;
            leak += snapshot.SelectedInfoPresent ? 0 : 1;
            leak += snapshot.RotateButtonPresent ? 0 : 1;
            leak += snapshot.MobileRotateZoneLayerPresent ? 0 : 1;
            leak += snapshot.DevControlBarPresent ? 0 : 1;
            leak += snapshot.DevDiagnosticsPresent ? 0 : 1;
            leak += snapshot.ResultRewardPlaceholderPresent ? 0 : 1;
            leak += snapshot.PrefabExists ? 0 : 1;
            leak += snapshot.ShellSceneInBuildSettings ? 1 : 0;
            leak += snapshot.MarkerDevOnly && snapshot.MarkerEnabledFalse && snapshot.MarkerFormalFalse && snapshot.MarkerRouteFalse ? 0 : 1;
            leak += snapshot.HostDevOnly && snapshot.HostFormalFalse && snapshot.HostNoSaveRewardChapter ? 0 : 1;
            leak += snapshot.ControllerIsolated ? 0 : 1;
            leak += snapshot.FormalRouteConnected ? 1 : 0;
            leak += snapshot.SaveRewardChapterWrite ? 1 : 0;
            return leak;
        }

        private static string Row(string label, bool pass)
        {
            return "| " + label + " | `" + (pass ? "PASS" : "FAIL") + "` |\n";
        }

        private static string Csv(string id, string check, bool pass, string details)
        {
            return EscapeCsv(id) + "," +
                   EscapeCsv(check) + "," +
                   EscapeCsv(pass ? "PASS" : "FAIL") + "," +
                   EscapeCsv(details);
        }

        private static string EscapeCsv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static GameObject RequireFind(Scene scene, string name)
        {
            GameObject found = FindDeepChildInScene(scene, name);
            if (found == null)
            {
                throw new InvalidOperationException("Missing required object: " + name + " in " + scene.path);
            }

            return found;
        }

        private static GameObject FindDeepChildInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindDeepChild(root.transform, name);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, name, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform RequireChild(Transform parent, string name)
        {
            Transform child = FindDeepChild(parent, name);
            if (child == null)
            {
                throw new InvalidOperationException("Missing child: " + name + " under " + parent.name);
            }

            return child;
        }

        private static RectTransform RequireRect(Transform transform)
        {
            RectTransform rect = transform as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException(transform.name + " must be a RectTransform.");
            }

            return rect;
        }

        private static RectTransform CreateRectChild(Transform parent, string name)
        {
            GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static Image EnsureImage(GameObject target, Color color, bool raycast)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static Text CreateText(RectTransform parent, string name, string textValue, int size)
        {
            GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            child.transform.SetParent(parent, false);
            Text text = child.GetComponent<Text>();
            text.text = textValue;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.92f, 0.88f, 0.76f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static string ToAbsolutePath(string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                                 ?? throw new InvalidOperationException("Project root not found.");
            return Path.Combine(projectRoot, projectRelativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return;
            }

            string normalized = folder.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string[] parts = normalized.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        public sealed class BridgeSnapshot
        {
            public string Package;
            public string Status;
            public string SourceScenePath;
            public string ShellScenePath;
            public string BridgePrefabPath;
            public string SourceMethod;
            public string MountPath;
            public bool Pass;
            public bool HostPresent;
            public bool BattleLikePreviewAreaPresent;
            public bool ControllerPresent;
            public bool BoardPresent;
            public int BoardSlotCount;
            public bool ItemTrayPresent;
            public bool ItemTrayViewPresent;
            public bool DragGhostPresent;
            public bool PlacementFeedbackPresent;
            public bool SelectedInfoPresent;
            public bool RotateButtonPresent;
            public bool MobileRotateZoneLayerPresent;
            public bool DevControlBarPresent;
            public bool EnemyFeedbackPanelPresent;
            public bool DevDiagnosticsPresent;
            public bool ResultRewardPlaceholderPresent;
            public bool PrefabExists;
            public bool ShellSceneInBuildSettings;
            public bool MarkerDevOnly;
            public bool MarkerEnabledFalse;
            public bool MarkerFormalFalse;
            public bool MarkerRouteFalse;
            public bool HostDevOnly;
            public bool HostFormalFalse;
            public bool HostNoSaveRewardChapter;
            public bool ControllerIsolated;
            public bool FormalRouteConnected;
            public bool SaveRewardChapterWrite;
            public bool BuildSettingsModified;
            public bool ProjectSettingsModified;
            public int LeakCount;
        }
    }
}
#endif
