#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools
{
    public static class AndroidApkBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string OutputPath = "Builds/Android/TalismanBag-V04-BattleSandboxPreview.apk";

        [MenuItem("Tools/Talisman Bag/Build/[Guard Only] Build V0.4 BattleSandbox Preview APK [Writes PlayerSettings]")]
        public static void BuildPlaytestApk()
        {
            PlayerSettings.companyName = "Prototype";
            PlayerSettings.productName = "TalismanBagV04BattleSandbox";
            PlayerSettings.bundleVersion = "0.4.0-preview";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            BuildPipeline.BuildPlayer(
                new[] { ScenePath },
                OutputPath,
                BuildTarget.Android,
                BuildOptions.None);

            Debug.Log($"Android playtest APK build requested: {OutputPath}");
        }
    }
}
#endif
