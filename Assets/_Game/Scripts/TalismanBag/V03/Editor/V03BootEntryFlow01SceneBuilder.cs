using System.Collections.Generic;
using System.Linq;
using TalismanBag.V03.BootEntry;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TalismanBag.V03.Editor
{
    public static class V03BootEntryFlow01SceneBuilder
    {
        public const string BootScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity";
        private const string BootSceneName = "Scene_TalismanBag_V03_BootEntry";

        [MenuItem("Tools/Talisman Bag/V0.3/Build Boot Entry Flow 01 Scene")]
        public static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = BootSceneName;

            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.047f, 0.043f, 1f);
            camera.orthographic = true;

            GameObject bootRoot = new("V03BootEntryFlow01", typeof(V03BootEntryFlowController));
            bootRoot.transform.position = Vector3.zero;

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("[V0.3-BootEntryFlow01] BOOT_ENTRY_SCENE_BUILT path=" + BootScenePath);
        }

        private static void EnsureBuildSettings()
        {
            string[] requiredPaths =
            {
                BootScenePath,
                V03BootEntryFlowController.HomeScenePath,
                V03NavigationFlowController.TrialScenePath
            };

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(scene => scene != null && !requiredPaths.Contains(scene.path))
                .ToList();

            List<EditorBuildSettingsScene> orderedScenes = requiredPaths
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToList();
            orderedScenes.AddRange(scenes);
            EditorBuildSettings.scenes = orderedScenes.ToArray();
        }
    }
}


