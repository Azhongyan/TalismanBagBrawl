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
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/Enable And Open Continuous Handtest";
        private const string DisableMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/Disable Continuous Handtest";
        private const string EnableDiagnosticsMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Enable Continuous Diagnostic Panel";
        private const string DisableDiagnosticsMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Disable Continuous Diagnostic Panel";
        private const string DiagnosticPreferenceKey =
            "TalismanBag.V04.BLineChapter1.DiagnosticsVisible";
        private const string InjectedPanelName =
            "V04_BLine_C1_ContinuousHandtest_Panel";
        private static GameObject injectedRoot;
        private static GameObject injectedPanel;

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
        public static bool IsDiagnosticsEnabled =>
            EditorPrefs.GetBool(DiagnosticPreferenceKey, false);

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
            EditorPrefs.SetBool(DiagnosticPreferenceKey, false);
            DestroyInjectedRoot();
            RefreshMenuChecks();
            Debug.Log(
                "B-Line Chapter 1 continuous handtest disabled; no progression was written.");
        }

        [MenuItem(EnableDiagnosticsMenu)]
        private static void EnableDiagnostics()
        {
            EditorPrefs.SetBool(DiagnosticPreferenceKey, true);
            RefreshMenuChecks();
            if (Application.isPlaying)
            {
                TryInject(SceneManager.GetActiveScene());
            }
            Debug.Log(
                "B-Line diagnostic panel explicitly enabled for Editor Play only.");
        }

        [MenuItem(DisableDiagnosticsMenu)]
        private static void DisableDiagnostics()
        {
            EditorPrefs.SetBool(DiagnosticPreferenceKey, false);
            DestroyInjectedPanel();
            RefreshMenuChecks();
            Debug.Log(
                "B-Line diagnostic PanelRoot removed; RuntimeRoot/controller remain active.");
        }

        private static void OnPlayModeChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                TryInject(SceneManager.GetActiveScene());
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
            bool dirtyBefore = scene.isDirty;
            V04Chapter1BattleSandboxRuntimeController controller;
            if (existing.Length > 0)
            {
                injectedRoot = existing[0];
                controller = injectedRoot.GetComponent<
                    V04Chapter1BattleSandboxRuntimeController>();
                if (controller == null)
                {
                    Debug.LogError(
                        "B-Line runtime root exists without its Runtime Controller; injection stopped.");
                    return;
                }
            }
            else
            {
                injectedRoot = new GameObject(
                    V04Chapter1ContinuousBattleIntegrationContract
                        .InjectedRootName);
                injectedRoot.hideFlags = HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild
                    | HideFlags.HideInHierarchy;
                SceneManager.MoveGameObjectToScene(injectedRoot, scene);
                controller = injectedRoot.AddComponent<
                    V04Chapter1BattleSandboxRuntimeController>();
                controller.hideFlags = HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild;
            }

            if (injectedRoot.GetComponent<
                    V04Chapter1BattleSandboxOverlay>() != null)
            {
                Debug.LogError(
                    "B-Line Overlay must not cohost the Runtime Controller root.");
                DestroyInjectedRoot();
                return;
            }

            if (IsDiagnosticsEnabled)
            {
                injectedPanel = Resources.FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(value => value != null
                        && value.name == InjectedPanelName
                        && value.scene == scene);
                if (injectedPanel == null)
                {
                    injectedPanel = new GameObject(InjectedPanelName);
                    injectedPanel.hideFlags = HideFlags.DontSaveInEditor
                        | HideFlags.DontSaveInBuild
                        | HideFlags.HideInHierarchy;
                    SceneManager.MoveGameObjectToScene(injectedPanel, scene);
                }
                V04Chapter1BattleSandboxOverlay overlay =
                    injectedPanel.GetComponent<
                        V04Chapter1BattleSandboxOverlay>()
                    ?? injectedPanel.AddComponent<
                        V04Chapter1BattleSandboxOverlay>();
                overlay.hideFlags = HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild;
                overlay.Bind(controller);
                overlay.SetDiagnosticsVisible(true);
                overlay.SetPanelVisible(true);
            }
            else
            {
                DestroyInjectedPanel();
            }
            if (scene.isDirty != dirtyBefore)
            {
                Debug.LogError(
                    "B-Line Chapter 1 injection dirtied the target scene; handtest must stop.");
                DestroyInjectedRoot();
                return;
            }
            Debug.Log(
                "Injected DontSave B-Line RuntimeRoot/controller; diagnostic PanelRoot="
                + (IsDiagnosticsEnabled
                    ? "EXPLICIT_EDITOR_OPT_IN"
                    : "NOT_CREATED")
                + ".");
        }

        private static void DestroyInjectedRoot()
        {
            DestroyInjectedPanel();
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

        private static void DestroyInjectedPanel()
        {
            if (injectedPanel != null)
            {
                UnityEngine.Object.DestroyImmediate(injectedPanel);
                injectedPanel = null;
            }
            foreach (GameObject value in Resources
                .FindObjectsOfTypeAll<GameObject>()
                .Where(value => value != null
                    && value.name == InjectedPanelName)
                .ToArray())
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }

        private static void RefreshMenuChecks()
        {
            Menu.SetChecked(EnableMenu, IsEnabled);
            Menu.SetChecked(DisableMenu, !IsEnabled);
            Menu.SetChecked(
                EnableDiagnosticsMenu,
                IsDiagnosticsEnabled);
            Menu.SetChecked(
                DisableDiagnosticsMenu,
                !IsDiagnosticsEnabled);
        }
    }
}
