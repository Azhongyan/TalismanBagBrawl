#if UNITY_EDITOR
using System;
using System.Linq;
using TalismanBag.Navigation;
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
        private static int completedCycles;
        private static int mainHomeLoadCount;
        private static int worldMapSceneFrames;
        private static int upgradeSceneFrames;
        private static double bootDeadline;

        static V03NavigationFlow01PlayModeVerifier()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
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
            TalismanBag.V03.Editor.V03MainSceneNavigationVerifier.VerifyFromMenu();
            EditorSceneManager.OpenScene(
                TalismanSceneNavigationOwner.BootEntryScenePath,
                OpenSceneMode.Single);

            frameCount = 0;
            stage = -1;
            completedCycles = 0;
            bootDeadline = EditorApplication.timeSinceStartup + 5d;
            mainHomeLoadCount = 0;
            worldMapSceneFrames = 0;
            upgradeSceneFrames = 0;
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
                stage = -1;
                completedCycles = 0;
                bootDeadline = EditorApplication.timeSinceStartup + 5d;
                mainHomeLoadCount = 0;
                worldMapSceneFrames = 0;
                upgradeSceneFrames = 0;
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
                if (activeScene.path == TalismanSceneNavigationOwner.BootEntryScenePath)
                {
                    VerifyBootEntryScene(activeScene);
                    return;
                }

                if (activeScene.path == TalismanSceneNavigationOwner.WorldMapScenePath)
                {
                    worldMapSceneFrames++;
                    if (worldMapSceneFrames >= 3)
                    {
                        VerifyWorldMapScene(activeScene);
                    }

                    return;
                }

                if (activeScene.path ==
                    TalismanSceneNavigationOwner.TalismanUpgradeScenePath)
                {
                    upgradeSceneFrames++;
                    if (upgradeSceneFrames >= 3)
                    {
                        VerifyUpgradeScene(activeScene);
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
                    ClickButton(scene, "BottomNavTrialButton");
                    frameCount = 0;
                    worldMapSceneFrames = 0;
                    stage = 1;
                    break;
                case 4:
                    VerifyPageState(scene, "MainHomeRoot", null, true);
                    VerifyHomeHotspotBoundary(scene);
                    completedCycles++;
                    if (completedCycles >= 3)
                    {
                        Require(mainHomeLoadCount == 4,
                            "BootEntry and three WorldMap returns must load MainHome exactly four times.");
                        Debug.Log(
                            "[MainSceneNavigation] PLAYMODE_SUCCESS cycles=3 " +
                            "bootEntryMainHome=true mainHomeLoadEvents=4 " +
                            "mainHomeTrial=true worldMapUpgrade=true " +
                            "upgradeBackWorldMap=true worldMapBackMainHome=true " +
                            "duplicateRequestRejected=true loadMode=Single");
                        CompleteVerification();
                        return;
                    }

                    stage = 0;
                    frameCount = 0;
                    break;
            }
        }

        private static void VerifyBootEntryScene(Scene scene)
        {
            if (stage == 0)
            {
                Require(EditorApplication.timeSinceStartup < bootDeadline,
                    "BootEntry did not leave after its MainHome request was accepted.");
                return;
            }

            Require(stage == -1, "BootEntry reloaded after navigation had already started.");
            GameObject startButtonObject = FindSceneObject(scene, "StartGameButton");
            Button startButton = startButtonObject != null
                ? startButtonObject.GetComponent<Button>()
                : null;
            Require(startButton != null, "BootEntry StartGameButton is missing.");
            if (!startButton.gameObject.activeInHierarchy || !startButton.interactable)
            {
                Require(EditorApplication.timeSinceStartup < bootDeadline,
                    "BootEntry StartGamePage did not become active.");
                return;
            }

            startButton.onClick.Invoke();
            startButton.onClick.Invoke();
            bootDeadline = EditorApplication.timeSinceStartup + 5d;
            frameCount = 0;
            stage = 0;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SessionState.GetBool(VerificationKey, false) &&
                scene.path == TalismanSceneNavigationOwner.MainHomeScenePath)
            {
                mainHomeLoadCount++;
            }
        }

        private static void VerifyHomeHotspotBoundary(Scene scene)
        {
            Require(FindSceneObject(scene, "HomeHotspot_DreamSign") != null,
                "DreamSign must remain a main home hotspot.");

            GameObject bottomNav = FindSceneObject(scene, "BottomNavBar_Root");
            Require(bottomNav != null, "BottomNavBar_Root is missing.");
            string[] navLabels = bottomNav
                .GetComponentsInChildren<Text>(true)
                .OrderBy(text =>
                {
                    RectTransform parentRect = text.transform.parent as RectTransform;
                    return parentRect != null ? parentRect.anchoredPosition.x : 0f;
                })
                .Select(text => text.text)
                .ToArray();
            string[] expectedLabels = { "首页", "养成", "试炼", "探索", "更多" };
            Require(navLabels.SequenceEqual(expectedLabels),
                "Bottom nav order must be 首页 / 养成 / 试炼 / 探索 / 更多.");
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
            GameObject bottomNav = FindSceneObject(scene, "BottomNavBar_Root");
            Require(homeRoot != null && bottomNav != null, "Navigation roots are missing.");
            Require(
                homeRoot.activeInHierarchy == (expectedVisibleRoot == "MainHomeRoot"),
                "MainHomeRoot visibility does not match the expected navigation state.");
            Require(bottomNav.activeInHierarchy == expectBottomNav,
                "BottomNavBar_Root visibility does not match the expected state.");
            Require(expectedSecondaryRoot == null,
                "MainHome RuntimeLock does not support legacy secondary page roots.");

            foreach (string rootName in secondaryRoots)
            {
                Require(
                    FindSceneObject(scene, rootName) == null,
                    $"{rootName} must remain absent under MainHome RuntimeLock.");
            }
        }

        private static void VerifyWorldMapScene(Scene scene)
        {
            Require(
                scene.name == TalismanSceneNavigationOwner.WorldMapSceneName,
                "Explore navigation did not enter the contracted WorldMap scene.");
            Require(CountComponentsInScene<Canvas>(scene) == 1,
                "WorldMap scene must contain exactly one Canvas.");
            Require(CountComponentsInScene<UnityEngine.EventSystems.EventSystem>(scene) == 1,
                "WorldMap scene must contain exactly one EventSystem.");

            if (stage == 1)
            {
                Button upgradeButton = GetButton(scene, "WorldMapUpgradeButton");
                upgradeButton.onClick.Invoke();
                upgradeButton.onClick.Invoke();
                worldMapSceneFrames = 0;
                upgradeSceneFrames = 0;
                stage = 2;
                return;
            }

            if (stage == 3)
            {
                ClickButton(scene, "BackButton");
                worldMapSceneFrames = 0;
                frameCount = 0;
                stage = 4;
            }
        }

        private static void VerifyUpgradeScene(Scene scene)
        {
            Require(
                scene.name == TalismanSceneNavigationOwner.TalismanUpgradeSceneName,
                "Refine navigation did not enter the contracted V03 upgrade scene.");
            Require(CountComponentsInScene<Canvas>(scene) == 1,
                "Upgrade scene must contain exactly one Canvas.");
            if (stage == 2)
            {
                ClickButton(scene, "BottomNav_Home");
                upgradeSceneFrames = 0;
                worldMapSceneFrames = 0;
                stage = 3;
            }
        }

        private static void CompleteVerification()
        {
            SessionState.EraseBool(VerificationKey);
            SessionState.SetBool(ExitKey, Application.isBatchMode);
            EditorApplication.isPlaying = false;
        }

        private static void ClickButton(Scene scene, string objectName)
        {
            Button button = GetButton(scene, objectName);
            button.onClick.Invoke();
        }

        private static Button GetButton(Scene scene, string objectName)
        {
            GameObject buttonObject = FindSceneObject(scene, objectName);
            Require(buttonObject != null, $"Button '{objectName}' is missing.");
            Button button = buttonObject.GetComponent<Button>();
            Require(button != null, $"Object '{objectName}' does not contain a Button.");
            Require(button.isActiveAndEnabled && button.interactable,
                $"Button '{objectName}' is not interactable.");
            return button;
        }

        private static int CountComponentsInScene<T>(Scene scene)
            where T : Component
        {
            return scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Count();
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
