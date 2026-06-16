#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools
{
    public static class AndroidApkBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity";
        private const string OutputPath = "Builds/Android/TalismanBagPrototype-v0.1.0.apk";

        [MenuItem("Tools/Talisman Bag/Build Android Playtest APK")]
        public static void BuildPlaytestApk()
        {
            PlayerSettings.companyName = "Prototype";
            PlayerSettings.productName = "TalismanBagPrototype";
            PlayerSettings.bundleVersion = "0.1.0";
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
