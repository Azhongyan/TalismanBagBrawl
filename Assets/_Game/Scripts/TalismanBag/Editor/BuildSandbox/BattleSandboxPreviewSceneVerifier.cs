#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxPreviewSceneVerifier
    {
        private static readonly string[] RequiredTopLevelRoots =
        {
            "EventSystem",
            "BuildSandboxPreviewRoot",
            "BuildSandboxPreviewCanvas"
        };

        private static readonly string[] RequiredPanels =
        {
            "SafeAreaRoot",
            "BattleLikePreviewArea",
            "ProblemSelectorPanel",
            "BuildSandboxDataPanelDock",
            "DevOnlyControlBar",
            "PopupLayer"
        };

        private static readonly string[] RequiredBattleAreaSlots =
        {
            "BoardGridPreview",
            "ItemTrayPreview",
            "SelectedItemInfo",
            "PlacementFeedback"
        };

        private static readonly string[] RequiredProblemSlots =
        {
            "MapRuleDropdownSlot",
            "EnemyProblemDropdownSlot",
            "BossProblemDropdownSlot",
            "DevChapterDropdownSlot"
        };

        private static readonly string[] RequiredDataSlots =
        {
            "BuildSummaryPanelSlot",
            "SynergyPanelSlot",
            "ShapeOccupancyPanelSlot",
            "AffixModifierPanelSlot",
            "ProblemReadinessPanelSlot",
            "SimulationResultPanelSlot"
        };

        private static readonly string[] RequiredControlSlots =
        {
            "RunSimulationButtonSlot",
            "ResetPreviewButtonSlot",
            "ExportReportButtonSlot"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Preview Scene 01");
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string sceneAbsolutePath = Path.Combine(projectRoot, BuildSandboxPreviewSceneMarker.ScenePath);

            if (!File.Exists(sceneAbsolutePath))
            {
                report.AddError(
                    "BATTLE_SANDBOX_PREVIEW_SCENE_MISSING",
                    "Preview scene file is missing.",
                    BuildSandboxPreviewSceneMarker.ScenePath);
                return report;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                ValidateSceneIdentity(report, scene);
                ValidateTopLevelRoots(report, scene);
                ValidateMarker(report);
                ValidateCanvas(report);
                ValidateFeatureFlags(report);
                ValidateBuildSettingsIsolation(report);

                if (scene.isDirty)
                {
                    report.AddError(
                        "BATTLE_SANDBOX_VERIFY_DIRTY_SCENE",
                        "Verifier must not leave the preview scene dirty.",
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path
                    && File.Exists(Path.Combine(projectRoot, previousScene.path)))
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }

            return report;
        }

        public static string[] GetRequiredObjectPaths()
        {
            return RequiredTopLevelRoots
                .Concat(new[] { "BuildSandboxPreviewCanvas/SafeAreaRoot" })
                .Concat(RequiredPanels.Select(panel => "BuildSandboxPreviewCanvas/SafeAreaRoot/" + panel)
                    .Where(path => !path.EndsWith("/SafeAreaRoot", StringComparison.Ordinal)))
                .Concat(RequiredBattleAreaSlots.Select(slot => "BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/" + slot))
                .Concat(RequiredProblemSlots.Select(slot => "BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/" + slot))
                .Concat(RequiredDataSlots.Select(slot => "BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/" + slot))
                .Concat(RequiredControlSlots.Select(slot => "BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/" + slot))
                .ToArray();
        }

        private static void ValidateSceneIdentity(BuildSandboxValidationReport report, Scene scene)
        {
            if (scene.path != BuildSandboxPreviewSceneMarker.ScenePath)
            {
                report.AddError(
                    "BATTLE_SANDBOX_PREVIEW_SCENE_PATH_MISMATCH",
                    $"Preview scene path mismatch: {scene.path}",
                    BuildSandboxPreviewSceneMarker.ScenePath);
                return;
            }

            report.AddInfo(
                "BATTLE_SANDBOX_PREVIEW_SCENE_EXISTS",
                "Preview scene file exists and opened.",
                BuildSandboxPreviewSceneMarker.ScenePath);
        }

        private static void ValidateTopLevelRoots(BuildSandboxValidationReport report, Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (string rootName in RequiredTopLevelRoots)
            {
                int count = roots.Count(root => root.name == rootName);
                if (count == 1)
                {
                    report.AddInfo("BATTLE_SANDBOX_ROOT_PRESENT", rootName, rootName);
                }
                else
                {
                    report.AddError("BATTLE_SANDBOX_ROOT_COUNT_INVALID", $"{rootName} count={count}, expected=1.", rootName);
                }
            }

            if (UnityEngine.Object.FindObjectOfType<EventSystem>(true) == null)
            {
                report.AddError("BATTLE_SANDBOX_EVENTSYSTEM_MISSING", "EventSystem is missing.", "EventSystem");
            }
        }

        private static void ValidateMarker(BuildSandboxValidationReport report)
        {
            BuildSandboxPreviewSceneMarker[] markers =
                UnityEngine.Object.FindObjectsOfType<BuildSandboxPreviewSceneMarker>(true);
            if (markers.Length != 1)
            {
                report.AddError(
                    "BATTLE_SANDBOX_MARKER_COUNT_INVALID",
                    $"Expected one BuildSandboxPreviewSceneMarker; found {markers.Length}.",
                    nameof(BuildSandboxPreviewSceneMarker));
                return;
            }

            BuildSandboxPreviewSceneMarker marker = markers[0];
            if (!marker.DevOnly)
            {
                report.AddError("BATTLE_SANDBOX_MARKER_DEVONLY_FALSE", "Preview scene marker must keep devOnly=true.", marker.name);
            }

            if (marker.IsEnabled)
            {
                report.AddError("BATTLE_SANDBOX_MARKER_ENABLED_TRUE", "Preview scene marker must keep isEnabled=false.", marker.name);
            }

            if (marker.ConnectedToFormalFlow)
            {
                report.AddError("BATTLE_SANDBOX_MARKER_FORMAL_FLOW_TRUE", "Preview scene marker must not connect to formal flow.", marker.name);
            }

            if (marker.DevOnly && !marker.IsEnabled && !marker.ConnectedToFormalFlow)
            {
                report.AddInfo("BATTLE_SANDBOX_MARKER_ISOLATED", "Marker is devOnly, disabled, and not connected.", marker.name);
            }
        }

        private static void ValidateCanvas(BuildSandboxValidationReport report)
        {
            Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            if (canvases.Length != 1)
            {
                report.AddError("BATTLE_SANDBOX_CANVAS_COUNT_INVALID", $"Expected one Canvas; found {canvases.Length}.", "BuildSandboxPreviewCanvas");
                return;
            }

            Canvas canvas = canvases[0];
            if (canvas.name != "BuildSandboxPreviewCanvas")
            {
                report.AddError("BATTLE_SANDBOX_CANVAS_NAME_INVALID", $"Unexpected Canvas name: {canvas.name}.", canvas.name);
            }

            if (canvas.GetComponent<CanvasScaler>() == null)
            {
                report.AddError("BATTLE_SANDBOX_CANVAS_SCALER_MISSING", "CanvasScaler is missing.", canvas.name);
            }

            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                report.AddError("BATTLE_SANDBOX_GRAPHIC_RAYCASTER_MISSING", "GraphicRaycaster is missing.", canvas.name);
            }

            Transform safeArea = RequireChild(report, canvas.transform, "SafeAreaRoot");
            if (safeArea == null)
            {
                return;
            }

            foreach (string panel in RequiredPanels.Where(panel => panel != "SafeAreaRoot"))
            {
                RequireChild(report, safeArea, panel);
            }

            Transform battleArea = FindDeepChild(safeArea, "BattleLikePreviewArea");
            foreach (string slot in RequiredBattleAreaSlots)
            {
                RequireChild(report, battleArea, slot);
            }

            Transform problemPanel = FindDeepChild(safeArea, "ProblemSelectorPanel");
            foreach (string slot in RequiredProblemSlots)
            {
                RequireChild(report, problemPanel, slot);
            }

            Transform dataDock = FindDeepChild(safeArea, "BuildSandboxDataPanelDock");
            foreach (string slot in RequiredDataSlots)
            {
                RequireChild(report, dataDock, slot);
            }

            Transform controlBar = FindDeepChild(safeArea, "DevOnlyControlBar");
            foreach (string slot in RequiredControlSlots)
            {
                Transform controlSlot = RequireChild(report, controlBar, slot);
                if (controlSlot != null && controlSlot.GetComponent<Button>() == null)
                {
                    report.AddError("BATTLE_SANDBOX_CONTROL_BUTTON_MISSING", $"{slot} must have Button.", BuildPath(controlSlot));
                }
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError("BATTLE_SANDBOX_FEATURE_FLAG_TRUE", $"{flag.Key} default must remain false.", nameof(BuildSandboxFeatureFlags));
                }
                else
                {
                    report.AddInfo("BATTLE_SANDBOX_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
                }
            }
        }

        private static void ValidateBuildSettingsIsolation(BuildSandboxValidationReport report)
        {
            bool containsPreviewScene = EditorBuildSettings.scenes.Any(scene =>
                scene != null && string.Equals(scene.path, BuildSandboxPreviewSceneMarker.ScenePath, StringComparison.Ordinal));
            if (containsPreviewScene)
            {
                report.AddError(
                    "BATTLE_SANDBOX_BUILDSETTINGS_CONTAINS_PREVIEW",
                    "Preview scene must not be added to Build Settings in this package.",
                    "ProjectSettings/EditorBuildSettings.asset");
                return;
            }

            report.AddInfo(
                "BATTLE_SANDBOX_BUILDSETTINGS_UNTOUCHED",
                "Preview scene is not present in Build Settings.",
                "ProjectSettings/EditorBuildSettings.asset");
        }

        private static Transform RequireChild(BuildSandboxValidationReport report, Transform parent, string childName)
        {
            if (parent == null)
            {
                report.AddError("BATTLE_SANDBOX_PARENT_MISSING", $"Cannot find child {childName}; parent missing.", childName);
                return null;
            }

            Transform child = FindDeepChild(parent, childName);
            if (child == null)
            {
                report.AddError("BATTLE_SANDBOX_CHILD_MISSING", $"{BuildPath(parent)} missing {childName}.", childName);
                return null;
            }

            if (child.GetComponent<RectTransform>() == null && child.GetComponent<EventSystem>() == null)
            {
                report.AddError("BATTLE_SANDBOX_RECT_MISSING", $"{BuildPath(child)} must have RectTransform.", BuildPath(child));
            }

            report.AddInfo("BATTLE_SANDBOX_CHILD_PRESENT", BuildPath(child), BuildPath(child));
            return child;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static string BuildPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
#endif
