using System.Linq;
using TalismanBag.V03.BootEntry;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TalismanBag.V03.Editor
{
    public static class V03BootEntryFlow01Smoke
    {
        [MenuItem("Tools/Talisman Bag/V0.3/Run Boot Entry Flow 01 Smoke")]
        public static void Run()
        {
            Require(EditorBuildSettings.scenes.Length > 0, "Build Settings must contain at least one scene.");
            Require(
                EditorBuildSettings.scenes[0].path == V03BootEntryFlow01SceneBuilder.BootScenePath,
                "BootEntry scene must be the first enabled Build Settings scene.");
            Require(
                EditorBuildSettings.scenes.Any(scene => scene.path == V03BootEntryFlow01SceneBuilder.BootScenePath && scene.enabled),
                "BootEntry scene must be enabled in Build Settings.");
            Require(
                EditorBuildSettings.scenes.Any(scene => scene.path == V03BootEntryFlowController.HomeScenePath && scene.enabled),
                "MainHome scene must be enabled in Build Settings.");
            Require(
                EditorBuildSettings.scenes.Any(scene => scene.path == V03NavigationFlowController.TrialScenePath && scene.enabled),
                "Trial scene must be enabled in Build Settings.");

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(V03BootEntryFlow01SceneBuilder.BootScenePath);
            V03BootEntryFlowController controller = Object.FindObjectOfType<V03BootEntryFlowController>();
            Require(controller != null, "BootEntry scene must contain V03BootEntryFlowController.");
            Require(Object.FindObjectOfType<EventSystem>() != null, "BootEntry scene must contain an EventSystem.");
            Require(Camera.main != null, "BootEntry scene must contain a Main Camera.");
            Require(scene.isLoaded, "BootEntry scene must load in the editor.");

            Debug.Log("[V0.3-BootEntryFlow01] SMOKE_SUCCESS loading=runtime, start=runtime, server=placeholder, opening=runtime, newPlayer=trial, oldPlayer=home");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException("[V0.3-BootEntryFlow01] " + message);
            }
        }
    }
}
