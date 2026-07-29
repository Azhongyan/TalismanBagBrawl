#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemSandboxAndroidVisualBuildMenu
    {
        private const string MenuPath = "TalismanBag/Item Sandbox/Build Android Visual Test APK (Manual Only)";
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string OutputPath = "Builds/Android/ItemSandboxVisual/Scene_TalismanBag_V04_ItemSandbox.apk";

        [MenuItem(MenuPath, false, 2300)]
        private static void BuildAndroidVisualTestApk()
        {
            if (!TryValidateBuildRequest(out var validationMessage))
            {
                EditorUtility.DisplayDialog("ItemSandbox APK Build", validationMessage, "OK");
                return;
            }

            if (!TrySaveActiveItemSandboxScene())
            {
                return;
            }

            var projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                EditorUtility.DisplayDialog("ItemSandbox APK Build", "Unable to resolve the Unity project root.", "OK");
                return;
            }

            var absoluteOutputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputPath));
            var outputDirectory = Path.GetDirectoryName(absoluteOutputPath);
            if (string.IsNullOrEmpty(outputDirectory))
            {
                EditorUtility.DisplayDialog("ItemSandbox APK Build", "Unable to resolve the APK output directory.", "OK");
                return;
            }

            Directory.CreateDirectory(outputDirectory);

            var previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                // This visual-test command must produce an APK, not an Android App Bundle.
                // Restore the user's editor preference immediately after the synchronous build.
                EditorUserBuildSettings.buildAppBundle = false;

                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = absoluteOutputPath,
                    targetGroup = BuildTargetGroup.Android,
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                });

                var summary = report.summary;
                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError(
                        $"ItemSandbox Android visual-test APK build failed: {summary.result}. " +
                        $"Errors={summary.totalErrors}, Warnings={summary.totalWarnings}.");
                    EditorUtility.DisplayDialog(
                        "ItemSandbox APK Build Failed",
                        $"Result: {summary.result}\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\n\nCheck the Unity Console for details.",
                        "OK");
                    return;
                }

                Debug.Log(
                    $"ItemSandbox Android visual-test APK build succeeded. " +
                    $"Scene={ScenePath}, Output={absoluteOutputPath}, Size={summary.totalSize} bytes. " +
                    "EditorBuildSettings scene list was not modified.");
                EditorUtility.RevealInFinder(absoluteOutputPath);
                EditorUtility.DisplayDialog(
                    "ItemSandbox APK Build Succeeded",
                    $"APK:\n{absoluteOutputPath}\n\nOnly Scene_TalismanBag_V04_ItemSandbox was included. Build Settings were not modified.",
                    "OK");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "ItemSandbox APK Build Failed",
                    "The build threw an exception. Check the Unity Console for details.",
                    "OK");
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateBuildAndroidVisualTestApk()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode
                   && !EditorApplication.isCompiling
                   && !EditorApplication.isUpdating;
        }

        private static bool TryValidateBuildRequest(out string message)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                message = "Exit Play Mode before building the visual-test APK.";
                return false;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                message = "Wait for Unity compilation/import to finish, then run the menu again.";
                return false;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                message = "Android is not the active build target. Open File > Build Settings, select Android, and click Switch Platform first.";
                return false;
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                message = "Android Build Support is unavailable in this Unity installation.";
                return false;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                message = $"The ItemSandbox scene was not found:\n{ScenePath}";
                return false;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (!string.Equals(activeScene.path, ScenePath, StringComparison.OrdinalIgnoreCase))
            {
                message = $"Open the ItemSandbox scene before building:\n{ScenePath}";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static bool TrySaveActiveItemSandboxScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isDirty)
            {
                return true;
            }

            var confirmed = EditorUtility.DisplayDialog(
                "Save ItemSandbox And Build",
                "Unity reports that Scene_TalismanBag_V04_ItemSandbox has changes. " +
                "Edit Mode layout components can mark the scene dirty again after a manual save.\n\n" +
                "Save the current ItemSandbox scene once and continue building the APK?",
                "Save And Build",
                "Cancel");
            if (!confirmed)
            {
                return false;
            }

            if (!EditorSceneManager.SaveScene(activeScene))
            {
                EditorUtility.DisplayDialog(
                    "ItemSandbox APK Build",
                    "Unity could not save the ItemSandbox scene. The APK build was cancelled.",
                    "OK");
                return false;
            }

            if (activeScene.isDirty)
            {
                Debug.LogWarning(
                    "ItemSandbox became dirty again immediately after SaveScene, likely because an Edit Mode layout component recalculated. " +
                    "The APK build will use the scene version that was just written to disk.");
            }

            return true;
        }
    }
}
#endif
