#if UNITY_EDITOR
using System;
using System.Linq;
using TalismanBag.V03.Forge;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.V03.EditorTools
{
    public static class V03TalismanUpgradeSceneBuilder
    {
        private const string RootName = "TalismanUpgradeSceneRoot";

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/Build Upgrade Scene")]
        public static void BuildSceneBatch()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new(RootName);
            V03TalismanUpgradeSceneController controller = root.AddComponent<V03TalismanUpgradeSceneController>();
            controller.RebuildEditablePreview();

            Require(
                EditorSceneManager.SaveScene(scene, V03NavigationFlowController.UpgradeScenePath),
                "Could not save V03 talisman upgrade scene.");

            AppendBuildSettingsScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            VerifyStaticScene();

            Debug.Log("[V0.3-ForgeFirstUpgradeGuide01] UPGRADE_SCENE_BUILD_SUCCESS");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/Verify Upgrade Scene")]
        public static void VerifyStaticBatch()
        {
            EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/Rebuild Editable Upgrade Preview")]
        public static void RebuildEditablePreviewInOpenScene()
        {
            V03TalismanUpgradeSceneController controller =
                UnityEngine.Object.FindObjectOfType<V03TalismanUpgradeSceneController>(true);
            Require(controller != null, "Upgrade scene controller is missing.");

            controller.RebuildEditablePreview();
            Scene scene = controller.gameObject.scene;
            Require(scene.IsValid(), "Upgrade scene is invalid.");
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save editable upgrade scene preview.");
            Debug.Log("[V0.3-ForgeFirstUpgradeGuide01] UPGRADE_EDITABLE_PREVIEW_REBUILD_SUCCESS");
        }

        public static void VerifyStaticScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != V03NavigationFlowController.UpgradeScenePath)
            {
                scene = EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            }

            GameObject root = scene
                .GetRootGameObjects()
                .FirstOrDefault(gameObject => gameObject.name == RootName);
            Require(root != null, "Upgrade scene root is missing.");
            Require(
                root.GetComponent<V03TalismanUpgradeSceneController>() != null,
                "Upgrade scene controller is missing.");
            Require(
                root.GetComponents<V03TalismanUpgradeSceneController>().Length == 1,
                "Upgrade scene must contain exactly one upgrade controller on the root.");
            Require(!scene.isDirty, "Static verification must not leave scene dirty.");
            VerifyBuildSettings();
        }

        private static void AppendBuildSettingsScenes()
        {
            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            string[] paths = current.Select(scene => scene.path).ToArray();
            string[] expected =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            if (paths.SequenceEqual(expected))
            {
                return;
            }

            string[] missingUpgradeOnly =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            Require(
                paths.SequenceEqual(missingUpgradeOnly),
                "Build Settings contains an unexpected scene list; refusing to reorder or overwrite it.");

            EditorBuildSettings.scenes = new[]
            {
                current[0],
                current[1],
                new EditorBuildSettingsScene(V03NavigationFlowController.UpgradeScenePath, true),
                current[2]
            };
        }

        private static void VerifyBuildSettings()
        {
            string[] expected =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            Require(
                EditorBuildSettings.scenes.Select(scene => scene.path).SequenceEqual(expected),
                "Build Settings order does not match the ForgeFirstUpgradeGuide01 contract.");
            Require(
                EditorBuildSettings.scenes.All(scene => scene.enabled),
                "All ForgeFirstUpgradeGuide01 Build Settings scenes must remain enabled.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
#endif
