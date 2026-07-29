#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TalismanBag.EditorTools
{
    public static class AndroidApkBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string OutputPath = "Builds/Android/TalismanBag-V04-BattleSandboxPreview.apk";
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string AuthoredChineseSourceFontPath =
            "Assets/TextMesh Pro/Fonts/MFLangSongJianYuan-Regular.otf";

        [MenuItem("Tools/Talisman Bag/Build/[Guard Only] Build V0.4 BattleSandbox Preview APK [Writes PlayerSettings]")]
        public static void BuildPlaytestApk()
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            var workbench = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                WorkbenchCatalogPath);
            var authoredChineseSourceFont =
                AssetDatabase.LoadAssetAtPath<Font>(
                    AuthoredChineseSourceFontPath);
            if (scene == null
                || workbench == null
                || authoredChineseSourceFont == null)
            {
                Debug.LogError(
                    "BattleSandbox APK build cancelled: target Scene or "
                    + "required runtime dependency is missing.");
                return;
            }

            PlayerSettings.companyName = "Prototype";
            PlayerSettings.productName = "TalismanBagV04BattleSandbox";
            PlayerSettings.bundleVersion = "0.4.0-preview";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            UnityEngine.Object[] previousPreloadedAssets =
                PlayerSettings.GetPreloadedAssets()
                ?? Array.Empty<UnityEngine.Object>();
            bool previousBuildAppBundle =
                EditorUserBuildSettings.buildAppBundle;
            try
            {
                UnityEngine.Object[] buildPreloadedAssets =
                    previousPreloadedAssets
                    .Where(value => value != null)
                    .Concat(new UnityEngine.Object[]
                    {
                        workbench,
                        authoredChineseSourceFont
                    })
                    .Distinct()
                    .ToArray();
                PlayerSettings.SetPreloadedAssets(buildPreloadedAssets);
                EditorUserBuildSettings.buildAppBundle = false;

                BuildReport report = BuildPipeline.BuildPlayer(
                    new BuildPlayerOptions
                    {
                        scenes = new[] { ScenePath },
                        locationPathName = OutputPath,
                        targetGroup = BuildTargetGroup.Android,
                        target = BuildTarget.Android,
                        options = BuildOptions.Development
                    });
                BuildSummary summary = report.summary;
                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError(
                        "BattleSandbox Android playtest APK build failed: "
                        + summary.result + ", errors=" + summary.totalErrors
                        + ", warnings=" + summary.totalWarnings + ".");
                    return;
                }

                Debug.Log(
                    "BattleSandbox Android playtest APK build succeeded: "
                    + OutputPath + ". Runtime Item authority catalog included; "
                    + "authored Chinese TMP source font included; "
                    + "Editor Build Settings were not modified.");
            }
            finally
            {
                PlayerSettings.SetPreloadedAssets(previousPreloadedAssets);
                EditorUserBuildSettings.buildAppBundle =
                    previousBuildAppBundle;
            }
        }
    }
}
#endif
