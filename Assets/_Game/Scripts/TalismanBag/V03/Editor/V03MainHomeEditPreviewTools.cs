#if UNITY_EDITOR
using System;
using TalismanBag.V02.UI;
using TalismanBag.V03.MainHome;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.V03.EditorTools
{
    public static class V03MainHomeEditPreviewTools
    {
        [MenuItem("Tools/Talisman Bag/V0.3/MainHome/Refresh Edit Preview (No Play)")]
        public static void RefreshEditPreview()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != V03NavigationFlow01SceneBuilder.MainHomeScenePath)
            {
                scene = EditorSceneManager.OpenScene(
                    V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                    OpenSceneMode.Single);
            }

            GameObject sceneRoot = FindSceneObject(scene, "MainHomeSceneRoot");
            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            Require(sceneRoot != null, "MainHomeSceneRoot is missing.");
            Require(homeRoot != null, "MainHomeRoot is missing.");

            MainHomeGreyboxPanel homePanel = homeRoot.GetComponent<MainHomeGreyboxPanel>();
            Require(homePanel != null, "MainHomeRoot is missing MainHomeGreyboxPanel.");

            V03MainHomeSceneBootstrap bootstrap =
                sceneRoot.GetComponent<V03MainHomeSceneBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = sceneRoot.AddComponent<V03MainHomeSceneBootstrap>();
            }

            SerializedObject serializedBootstrap = new(bootstrap);
            SerializedProperty homePanelProperty = serializedBootstrap.FindProperty("homePanel");
            Require(homePanelProperty != null, "Bootstrap homePanel field is missing.");
            homePanelProperty.objectReferenceValue = homePanel;
            serializedBootstrap.ApplyModifiedPropertiesWithoutUndo();

            bootstrap.RefreshEditorPreview();
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                "[V0.3-MainHome] EDIT_PREVIEW_REFRESHED_NO_PLAY " +
                "scene=Scene_TalismanBag_V03_MainHome");
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root;
                }

                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == objectName)
                    {
                        return child.gameObject;
                    }
                }
            }

            return null;
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
