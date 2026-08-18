using System;
using System.Linq;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Presentation
{
    [InitializeOnLoad]
    public static class C1ThreeSceneJourneyHandtestMenu
    {
        private const string EnableMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Enable And Open Three-Scene Journey";
        private const string DisableMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Disable Three-Scene Journey";
        private const string EnableDiagnosticsMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Enable Journey Diagnostic Panel";
        private const string DisableDiagnosticsMenu =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Manual Only] Disable Journey Diagnostic Panel";
        private const string DiagnosticPreferenceKey =
            "TalismanBag.V04.C1JourneyPresentation.DiagnosticsVisible";
        private static GameObject runtimeRoot;
        private static GameObject panelRoot;
        private static bool playModeExitPending;

        static C1ThreeSceneJourneyHandtestMenu()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += RefreshMenuChecks;
        }

        public static bool IsEnabled => EditorPrefs.GetBool(
            C1JourneyPresentationProfile.EditorPreferenceKey,
            false);
        public static bool IsDiagnosticsEnabled => EditorPrefs.GetBool(
            DiagnosticPreferenceKey,
            false);

        [MenuItem(EnableMenu)]
        private static void EnableAndOpen()
        {
            EditorPrefs.SetBool(
                C1JourneyPresentationProfile.EditorPreferenceKey,
                true);
            EditorPrefs.SetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                true);
            RefreshMenuChecks();

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid()
                && string.Equals(
                    activeScene.path,
                    C1JourneyPresentationProfile.TargetScenePath,
                    StringComparison.Ordinal))
            {
                Debug.Log(
                    "[" + C1JourneyPresentationProfile.PackageId
                    + "] Journey enabled on the already-open target Scene. Its current dirty state was preserved; no open or save was issued. Enter Play manually.");
                return;
            }

            if (Enumerable.Range(0, SceneManager.sceneCount)
                .Select(SceneManager.GetSceneAt)
                .Any(scene => scene.IsValid() && scene.isDirty))
            {
                Debug.LogError(
                    "[" + C1JourneyPresentationProfile.PackageId
                    + "] Journey enabled, but no scene was opened because an Editor scene is dirty.");
                return;
            }

            EditorSceneManager.OpenScene(
                C1JourneyPresentationProfile.TargetScenePath,
                OpenSceneMode.Single);
            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId
                + "] Enter Play manually. Runtime Presentation will be injected without any diagnostic panel.");
        }

        [MenuItem(DisableMenu)]
        private static void Disable()
        {
            EditorPrefs.SetBool(
                C1JourneyPresentationProfile.EditorPreferenceKey,
                false);
            EditorPrefs.SetBool(DiagnosticPreferenceKey, false);
            DestroyInjectedRoots();
            RefreshMenuChecks();
            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId
                + "] Presentation disabled. The Flow runtime was not stopped through panel visibility.");
        }

        [MenuItem(EnableDiagnosticsMenu)]
        private static void EnableDiagnostics()
        {
            EditorPrefs.SetBool(DiagnosticPreferenceKey, true);
            RefreshMenuChecks();
            if (Application.isPlaying)
            {
                TryInjectActiveScene();
            }
            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId
                + "] Journey diagnostic panel explicitly enabled for Editor Play only.");
        }

        [MenuItem(DisableDiagnosticsMenu)]
        private static void DisableDiagnostics()
        {
            EditorPrefs.SetBool(DiagnosticPreferenceKey, false);
            DestroyPanelRoot();
            RefreshMenuChecks();
            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId
                + "] Journey diagnostic PanelRoot removed; Presentation RuntimeRoot remains active.");
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                playModeExitPending = false;
                EditorApplication.delayCall += TryInjectActiveScene;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                playModeExitPending = true;
                C1JourneyPresentationController controller =
                    runtimeRoot?.GetComponent<
                        C1JourneyPresentationController>();
                controller?.GetType()
                    .GetMethod("PrepareForPlayModeExit")
                    ?.Invoke(controller, null);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                playModeExitPending = false;
                runtimeRoot = null;
                panelRoot = null;
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (Application.isPlaying && !playModeExitPending)
            {
                EditorApplication.delayCall += TryInjectActiveScene;
            }
        }

        private static void TryInjectActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!Application.isPlaying
                || playModeExitPending
                || !IsEnabled
                || !scene.IsValid()
                || !string.Equals(
                    scene.path,
                    C1JourneyPresentationProfile.TargetScenePath,
                    StringComparison.Ordinal))
            {
                return;
            }

            V04Chapter1BattleSandboxRuntimeController flowController =
                Resources.FindObjectsOfTypeAll<
                        V04Chapter1BattleSandboxRuntimeController>()
                    .FirstOrDefault(value => value != null
                        && value.gameObject.scene == scene);
            if (flowController == null)
            {
                Debug.LogError(
                    "[" + C1JourneyPresentationProfile.PackageId
                    + "] Existing B-Line Flow Runtime was not injected. Disable/re-enable the Manual Only journey entry, then re-enter Play.");
                return;
            }

            bool dirtyBefore = scene.isDirty;
            runtimeRoot = FindOrCreateRuntimeObject(
                scene,
                C1JourneyPresentationProfile.RuntimeRootName);
            C1JourneyPresentationController presentation =
                runtimeRoot.GetComponent<C1JourneyPresentationController>()
                ?? runtimeRoot.AddComponent<C1JourneyPresentationController>();
            presentation.hideFlags = HideFlags.DontSave;
            presentation.Bind(flowController);
            C1BattleSandboxAuthoredEnemyPresentationAdapter
                authoredEnemyAdapter = runtimeRoot.GetComponent<
                    C1BattleSandboxAuthoredEnemyPresentationAdapter>()
                ?? runtimeRoot.AddComponent<
                    C1BattleSandboxAuthoredEnemyPresentationAdapter>();
            authoredEnemyAdapter.hideFlags = HideFlags.DontSave;
            authoredEnemyAdapter.Bind(flowController);

            if (IsDiagnosticsEnabled)
            {
                panelRoot = FindOrCreateRuntimeObject(
                    scene,
                    C1JourneyPresentationProfile.PanelRootName);
                if (runtimeRoot == panelRoot)
                {
                    Debug.LogError(
                        "[" + C1JourneyPresentationProfile.PackageId
                        + "] PanelRoot and RuntimeRoot must remain separate.");
                    DestroyPanelRoot();
                    return;
                }
                C1JourneyPresentationOverlay overlay =
                    panelRoot.GetComponent<C1JourneyPresentationOverlay>()
                    ?? panelRoot.AddComponent<C1JourneyPresentationOverlay>();
                overlay.hideFlags = HideFlags.DontSave;
                overlay.Bind(presentation);
                overlay.SetDiagnosticsVisible(true);
            }
            else
            {
                DestroyPanelRoot();
            }

            if (scene.isDirty != dirtyBefore)
            {
                Debug.LogError(
                    "[" + C1JourneyPresentationProfile.PackageId
                    + "] Runtime injection dirtied the Scene; roots were removed.");
                DestroyInjectedRoots();
                return;
            }

            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId
                + "] DontSave Presentation RuntimeRoot injected; diagnostic PanelRoot="
                + (IsDiagnosticsEnabled ? "EXPLICIT_EDITOR_OPT_IN" : "NOT_CREATED")
                + "; Flow panel was not forced open; Scene dirty=false.");
        }

        private static GameObject FindOrCreateRuntimeObject(
            Scene scene,
            string objectName)
        {
            GameObject existing = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == scene
                    && string.Equals(
                        value.name,
                        objectName,
                        StringComparison.Ordinal));
            if (existing != null)
            {
                return existing;
            }

            GameObject created = new(objectName)
            {
                hideFlags = HideFlags.HideInHierarchy
                    | HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild
            };
            SceneManager.MoveGameObjectToScene(created, scene);
            return created;
        }

        private static void DestroyInjectedRoots()
        {
            DestroyPanelRoot();
            DestroyByName(C1JourneyPresentationProfile.RuntimeRootName);
            runtimeRoot = null;
        }

        private static void DestroyPanelRoot()
        {
            DestroyByName(C1JourneyPresentationProfile.PanelRootName);
            panelRoot = null;
        }

        private static void DestroyByName(string objectName)
        {
            foreach (GameObject value in Resources
                .FindObjectsOfTypeAll<GameObject>()
                .Where(value => value != null
                    && string.Equals(
                        value.name,
                        objectName,
                        StringComparison.Ordinal))
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
