#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.EditorTools
{
    [InitializeOnLoad]
    public static class V03NavigationFlow01PlayModeVerifier
    {
        private const string VerificationKey =
            "TalismanBag.V03.NavigationFlow01.PlayModeVerification";
        private const string ExitKey =
            "TalismanBag.V03.NavigationFlow01.ExitAfterPlayMode";

        private static int frameCount;
        private static int stage;
        private static int trialSceneFrames;

        static V03NavigationFlow01PlayModeVerifier()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("Tools/Talisman Bag/V0.3/NavigationFlow01/Verify PlayMode")]
        public static void VerifyPlayModeBatch()
        {
            BeginVerification();
        }

        public static void VerifyPlayModeAndExit()
        {
            BeginVerification();
        }

        private static void BeginVerification()
        {
            Require(!EditorApplication.isPlaying, "PlayMode verification is already running.");
            EditorSceneManager.OpenScene(
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                OpenSceneMode.Single);
            V03NavigationFlow01SceneBuilder.VerifyStaticScene();

            frameCount = 0;
            stage = 0;
            trialSceneFrames = 0;
            SessionState.SetBool(VerificationKey, true);
            SessionState.SetBool(ExitKey, false);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode &&
                SessionState.GetBool(VerificationKey, false))
            {
                frameCount = 0;
                stage = 0;
                trialSceneFrames = 0;
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode &&
                SessionState.GetBool(ExitKey, false))
            {
                SessionState.EraseBool(ExitKey);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
        }

        private static void OnEditorUpdate()
        {
            if (!SessionState.GetBool(VerificationKey, false) ||
                !EditorApplication.isPlaying)
            {
                return;
            }

            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.path ==
                    TalismanBag.V03.Navigation.V03NavigationFlowController.TrialScenePath)
                {
                    trialSceneFrames++;
                    if (trialSceneFrames >= 3)
                    {
                        VerifyTrialScene(activeScene);
                        CompleteVerification();
                    }

                    return;
                }

                frameCount++;
                RunNavigationStep(activeScene);
            }
            catch (Exception exception)
            {
                SessionState.EraseBool(VerificationKey);
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }

        private static void RunNavigationStep(Scene scene)
        {
            Require(
                scene.path == V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                "Navigation verification loaded an unexpected scene.");

            if (frameCount < 2)
            {
                return;
            }

            switch (stage)
            {
                case 0:
                    VerifyPageState(scene, "MainHomeRoot", null, true);
                    VerifyHomeHotspotBoundary(scene);
                    ClickButton(scene, "HomeHotspot_Refine");
                    stage++;
                    break;
                case 1:
                    VerifyPageState(scene, "RefinePageRoot", "RefinePageRoot", true);
                    ClickButton(scene, "BottomNavExploreButton");
                    stage++;
                    break;
                case 2:
                    VerifyPageState(scene, "ExplorePageRoot", "ExplorePageRoot", true);
                    ClickButton(scene, "BottomNavMoreButton");
                    stage++;
                    break;
                case 3:
                    VerifyPageState(scene, "MorePageRoot", "MorePageRoot", true);
                    ClickButton(scene, "BottomNavHomeButton");
                    stage++;
                    break;
                case 4:
                    VerifyPageState(scene, "MainHomeRoot", null, true);
                    VerifyHomeHotspotBoundary(scene);
                    ClickButton(scene, "HomeHotspot_Refine");
                    stage++;
                    break;
                case 5:
                    VerifyPageState(scene, "RefinePageRoot", "RefinePageRoot", true);
                    ClickButton(scene, "BottomNavTrialButton");
                    stage++;
                    break;
            }
        }

        private static void VerifyHomeHotspotBoundary(Scene scene)
        {
            Require(FindSceneObject(scene, "HomeHotspot_DreamSign") != null,
                "DreamSign must remain a main home hotspot.");

            GameObject bottomNav = FindSceneObject(scene, "SecondaryBottomNavRoot");
            Require(bottomNav != null, "SecondaryBottomNavRoot is missing.");
            string[] navLabels = bottomNav
                .GetComponentsInChildren<Text>(true)
                .OrderBy(text =>
                {
                    RectTransform parentRect = text.transform.parent as RectTransform;
                    return parentRect != null ? parentRect.anchoredPosition.x : 0f;
                })
                .Select(text => text.text)
                .ToArray();
            string[] expectedLabels = { "首页", "探索", "试练", "锻造", "更多" };
            Require(navLabels.SequenceEqual(expectedLabels),
                "Bottom nav order must be 首页 / 探索 / 试练 / 锻造 / 更多.");
            Require(!navLabels.Any(label =>
                    label.Contains("梦签") ||
                    label.Contains("背包") ||
                    label.Contains("BattleBackpack")),
                "Bottom nav must not expose DreamSign or backpack.");
        }

        private static void VerifyPageState(
            Scene scene,
            string expectedVisibleRoot,
            string expectedSecondaryRoot,
            bool expectBottomNav)
        {
            string[] secondaryRoots =
            {
                "RefinePageRoot",
                "ExplorePageRoot",
                "MorePageRoot"
            };

            GameObject homeRoot = FindSceneObject(scene, "MainHomeRoot");
            GameObject bottomNav = FindSceneObject(scene, "SecondaryBottomNavRoot");
            Require(homeRoot != null && bottomNav != null, "Navigation roots are missing.");
            Require(
                homeRoot.activeInHierarchy == (expectedVisibleRoot == "MainHomeRoot"),
                "MainHomeRoot visibility does not match the expected navigation state.");
            Require(bottomNav.activeInHierarchy == expectBottomNav,
                "SecondaryBottomNavRoot visibility does not match the expected state.");

            foreach (string rootName in secondaryRoots)
            {
                GameObject root = FindSceneObject(scene, rootName);
                Require(root != null, $"{rootName} is missing at runtime.");
                Require(
                    root.activeInHierarchy == (rootName == expectedSecondaryRoot),
                    $"{rootName} visibility does not match the expected navigation state.");
            }
        }

        private static void VerifyTrialScene(Scene scene)
        {
            Require(
                scene.name == TalismanBag.V03.Navigation.V03NavigationFlowController.TrialSceneName,
                "Trial navigation did not enter the contracted V02 scene.");
            Require(FindAllSceneObjects(scene, "Canvas").Length == 1,
                "V02 trial scene must contain exactly one Canvas.");
            Require(FindAllSceneObjects(scene, "EventSystem").Length == 1,
                "V02 trial scene must contain exactly one EventSystem.");
            Require(scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Sum(transform =>
                        GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            transform.gameObject)) == 0,
                "V02 trial scene contains Missing Script components.");

            Debug.Log(
                "[V0.3-NavigationFlow01] NF01_PLAYMODE_SUCCESS " +
                "homeFirstFrame=true refine=true explore=true more=true homeReturn=true " +
                "bottomTrial=true loadMode=Single duplicateCanvas=false duplicateEventSystem=false");
        }

        private static void CompleteVerification()
        {
            SessionState.EraseBool(VerificationKey);
            SessionState.SetBool(ExitKey, Application.isBatchMode);
            EditorApplication.isPlaying = false;
        }

        private static void ClickButton(Scene scene, string objectName)
        {
            GameObject buttonObject = FindSceneObject(scene, objectName);
            Require(buttonObject != null, $"Button '{objectName}' is missing.");
            Button button = buttonObject.GetComponent<Button>();
            Require(button != null, $"Object '{objectName}' does not contain a Button.");
            Require(button.isActiveAndEnabled && button.interactable,
                $"Button '{objectName}' is not interactable.");
            button.onClick.Invoke();
        }

        private static GameObject FindSceneObject(Scene scene, string objectName)
        {
            return FindAllSceneObjects(scene, objectName).FirstOrDefault();
        }

        private static GameObject[] FindAllSceneObjects(Scene scene, string objectName)
        {
            return scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(transform => transform.name == objectName)
                .Select(transform => transform.gameObject)
                .ToArray();
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
