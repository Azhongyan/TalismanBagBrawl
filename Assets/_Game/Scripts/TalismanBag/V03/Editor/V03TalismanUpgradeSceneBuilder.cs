#if UNITY_EDITOR
using System;
using System.Linq;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V03.Forge;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TalismanBag.V03.EditorTools
{
    public static class V03TalismanUpgradeSceneBuilder
    {
        private const string RootName = "TalismanUpgradeSceneRoot";

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Build Upgrade Scene")]
        public static void BuildSceneBatch()
        {
            if (!ConfirmSceneWrite(
                    "Build Upgrade Scene",
                    "This Guard Only tool rebuilds and saves:\n" +
                    V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "It generates the editable upgrade UI, writes the scene, and may update Build Settings.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

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

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[QA Only] Verify Upgrade Scene")]
        public static void VerifyStaticBatch()
        {
            EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Rebuild Editable Upgrade Preview")]
        public static void RebuildEditablePreviewInOpenScene()
        {
            if (!ConfirmSceneWrite(
                    "Rebuild Editable Upgrade Preview",
                    "This Guard Only tool rewrites the editable preview UI in the open upgrade scene.\n\n" +
                    "Target scene:\n" + V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "It writes generated UI under V03TalismanUpgradePageRoot and saves the scene.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

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

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Bind Upgrade Runtime Lock Services")]
        public static void BindRuntimeLockServicesBatch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(
                    "[V0.3-BootGuideUpgradeRuntimeLock01] Bind Upgrade Runtime Lock Services must run in Edit Mode. " +
                    "Exit Play Mode and rerun the menu item.");
                return;
            }

            if (!ConfirmSceneWrite(
                    "Bind Upgrade Runtime Lock Services",
                    "Use only for Scene_TalismanBag_V03_TalismanUpgrade, in Edit Mode.\n\n" +
                    "This writes scene nodes if missing:\n" +
                    "- V03Upgrade_ResourceService\n" +
                    "- V03Upgrade_UpgradeService\n" +
                    "- V03Upgrade_MainTrialFlowService\n" +
                    "- EventSystem\n\n" +
                    "Target scene:\n" + V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "Run only with Guard or user confirmation, after saving or backing up open work.\n" +
                    "After it finishes, press Ctrl+S once to make the scene save state explicit."))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            GameObject root = scene
                .GetRootGameObjects()
                .FirstOrDefault(gameObject => gameObject.name == RootName);
            Require(root != null, "Upgrade scene root is missing.");
            Require(
                root.GetComponent<V03TalismanUpgradeSceneController>() != null,
                "Upgrade scene controller is missing.");

            _ = EnsureSceneService<ResourceService>(root.transform, "V03Upgrade_ResourceService");
            _ = EnsureSceneService<UpgradeService>(root.transform, "V03Upgrade_UpgradeService");
            _ = EnsureSceneService<MainTrialFlowService>(root.transform, "V03Upgrade_MainTrialFlowService");
            _ = EnsureEventSystem(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save upgrade runtime-lock service nodes.");
            AssetDatabase.SaveAssets();
            Debug.Log("[V0.3-BootGuideUpgradeRuntimeLock01] UPGRADE_RUNTIME_LOCK_SCENE_NODES_BOUND");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Bind Upgrade Runtime Lock Services", true)]
        private static bool CanBindRuntimeLockServicesBatch()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
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
            Require(
                UnityEngine.Object.FindObjectOfType<ResourceService>(true) != null,
                "Upgrade scene must contain a scene-authored ResourceService.");
            Require(
                UnityEngine.Object.FindObjectOfType<UpgradeService>(true) != null,
                "Upgrade scene must contain a scene-authored UpgradeService.");
            Require(
                UnityEngine.Object.FindObjectOfType<MainTrialFlowService>(true) != null,
                "Upgrade scene must contain a scene-authored MainTrialFlowService.");
            Require(
                UnityEngine.Object.FindObjectOfType<EventSystem>(true) != null,
                "Upgrade scene must contain a scene-authored EventSystem.");
            Require(!scene.isDirty, "Static verification must not leave scene dirty.");
            VerifyBuildSettings();
        }

        private static EventSystem EnsureEventSystem(Transform root)
        {
            EventSystem existing = UnityEngine.Object.FindObjectOfType<EventSystem>(true);
            if (existing != null)
            {
                return existing;
            }

            Transform existingObject = FindDeepChild(root, "EventSystem");
            GameObject eventSystemObject = existingObject != null ? existingObject.gameObject : new GameObject("EventSystem");
            if (eventSystemObject.transform.parent == null)
            {
                eventSystemObject.transform.SetParent(root, false);
            }

            EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
            eventSystem ??= eventSystemObject.AddComponent<EventSystem>();
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            return eventSystem;
        }

        private static T EnsureSceneService<T>(Transform root, string objectName) where T : Component
        {
            T existing = UnityEngine.Object.FindObjectOfType<T>(true);
            if (existing != null)
            {
                return existing;
            }

            Transform existingObject = FindDeepChild(root, objectName);
            GameObject serviceObject = existingObject != null ? existingObject.gameObject : new GameObject(objectName);
            if (serviceObject.transform.parent == null)
            {
                serviceObject.transform.SetParent(root, false);
            }

            T service = serviceObject.GetComponent<T>();
            return service != null ? service : serviceObject.AddComponent<T>();
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(objectName))
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

        private static bool ConfirmSceneWrite(string title, string message)
        {
            return EditorUtility.DisplayDialog(
                "[Writes Scene][Guard Only] " + title,
                message,
                "Proceed",
                "Cancel");
        }
    }
}
#endif
