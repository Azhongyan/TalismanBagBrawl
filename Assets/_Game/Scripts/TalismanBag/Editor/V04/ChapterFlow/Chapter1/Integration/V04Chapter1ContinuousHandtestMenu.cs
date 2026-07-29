using System;
using System.Linq;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Integration
{
    [InitializeOnLoad]
    public static class V04Chapter1ContinuousHandtestMenu
    {
        private const string EnableMenu =
            "Tools/TalismanBag/V0.4/B-Line Chapter 1/Enable And Open Continuous Handtest";
        private const string DisableMenu =
            "Tools/TalismanBag/V0.4/B-Line Chapter 1/Disable Continuous Handtest";
        private static GameObject injectedRoot;

        static V04Chapter1ContinuousHandtestMenu()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += RefreshMenuChecks;
        }

        public static bool IsEnabled =>
            EditorPrefs.GetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                false);

        [MenuItem(EnableMenu)]
        private static void EnableAndOpen()
        {
            EditorPrefs.SetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                true);
            RefreshMenuChecks();

            if (Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Any(scene => scene.IsValid() && scene.isDirty))
            {
                Debug.LogError(
                    "B-Line Chapter 1 handtest enabled, but target scene was not opened because an Editor scene is dirty.");
                return;
            }
            EditorSceneManager.OpenScene(
                V04Chapter1ContinuousBattleIntegrationContract
                    .TargetScenePath,
                OpenSceneMode.Single);
            Debug.Log(
                "B-Line Chapter 1 continuous handtest enabled. Enter Play Mode manually.");
        }

        [MenuItem(DisableMenu)]
        private static void Disable()
        {
            EditorPrefs.SetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                false);
            DestroyInjectedRoot();
            RefreshMenuChecks();
            Debug.Log(
                "B-Line Chapter 1 continuous handtest disabled; no progression was written.");
        }

        private static void OnPlayModeChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                TryInject(SceneManager.GetActiveScene());
            }
            else if (state == PlayModeStateChange.ExitingPlayMode
                || state == PlayModeStateChange.EnteredEditMode)
            {
                DestroyInjectedRoot();
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (Application.isPlaying)
            {
                TryInject(scene);
            }
        }

        private static void TryInject(Scene scene)
        {
            if (!Application.isPlaying
                || !IsEnabled
                || !scene.IsValid()
                || !string.Equals(
                    scene.path,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TargetScenePath,
                    StringComparison.Ordinal))
            {
                return;
            }
            GameObject[] existing = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(value => value != null
                    && value.name ==
                        V04Chapter1ContinuousBattleIntegrationContract
                            .InjectedRootName
                    && value.scene == scene)
                .ToArray();
            if (existing.Length > 0)
            {
                injectedRoot = existing[0];
                return;
            }

            bool dirtyBefore = scene.isDirty;
            injectedRoot = new GameObject(
                V04Chapter1ContinuousBattleIntegrationContract
                    .InjectedRootName);
            injectedRoot.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild
                | HideFlags.HideInHierarchy;
            SceneManager.MoveGameObjectToScene(injectedRoot, scene);
            V04Chapter1BattleSandboxRuntimeController controller =
                injectedRoot.AddComponent<
                    V04Chapter1BattleSandboxRuntimeController>();
            controller.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            V04Chapter1BattleSandboxOverlay overlay =
                injectedRoot.AddComponent<
                    V04Chapter1BattleSandboxOverlay>();
            overlay.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            overlay.Bind(controller);
            if (scene.isDirty != dirtyBefore)
            {
                Debug.LogError(
                    "B-Line Chapter 1 injection dirtied the target scene; handtest must stop.");
                DestroyInjectedRoot();
                return;
            }
            Debug.Log(
                "Injected one DontSave B-Line Chapter 1 continuous handtest root.");
        }

        private static void DestroyInjectedRoot()
        {
            if (injectedRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(injectedRoot);
                injectedRoot = null;
            }
            foreach (GameObject value in Resources
                .FindObjectsOfTypeAll<GameObject>()
                .Where(value => value != null
                    && value.name ==
                        V04Chapter1ContinuousBattleIntegrationContract
                            .InjectedRootName)
                .ToArray())
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }

        private static void RefreshMenuChecks()
        {
            Menu.SetChecked(EnableMenu, IsEnabled);
            Menu.SetChecked(DisableMenu, !IsEnabled);
        }
    }
}
