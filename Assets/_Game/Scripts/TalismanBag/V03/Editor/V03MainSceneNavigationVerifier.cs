#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.Navigation;
using TalismanBag.V04.WorldMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Editor
{
    public static class V03MainSceneNavigationVerifier
    {
        public static void VerifyFromMenu()
        {
            VerifyStatic();
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                VerifyStatic();
                Debug.Log("MAIN_SCENE_NAVIGATION_STATIC_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("MAIN_SCENE_NAVIGATION_STATIC_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private static void VerifyStatic()
        {
            TalismanSceneRoute[] requiredRoutes =
            {
                TalismanSceneRoute.BootEntry,
                TalismanSceneRoute.MainHome,
                TalismanSceneRoute.WorldMap,
                TalismanSceneRoute.TalismanUpgrade
            };

            IReadOnlyList<EditorBuildSettingsScene> buildScenes =
                EditorBuildSettings.scenes;
            Require(buildScenes.Count > 0, "Build Settings scene list is empty.");
            Require(
                buildScenes[0].path ==
                TalismanSceneNavigationOwner.BootEntryScenePath,
                "BootEntry must remain the first Player scene.");
            Require(
                buildScenes
                    .GroupBy(scene => scene.path, StringComparer.Ordinal)
                    .All(group => group.Count() == 1),
                "Build Settings contains duplicate scene paths.");

            foreach (TalismanSceneRoute route in requiredRoutes)
            {
                string path = TalismanSceneNavigationOwner.GetScenePath(route);
                Require(File.Exists(path), "Scene asset is missing: " + path);
                Require(
                    buildScenes.Any(scene => scene.enabled && scene.path == path),
                    "Enabled Player scene is missing: " + path);
            }

            VerifyMainHomeScene();
            VerifyWorldMapScene();
            VerifyUpgradeScene();
        }

        private static void VerifyMainHomeScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                TalismanSceneNavigationOwner.MainHomeScenePath,
                OpenSceneMode.Single);
            Button trialButton = FindSceneComponent<Button>(
                scene,
                "BottomNavTrialButton");
            Require(trialButton != null, "MainHome Trial button is missing.");
            Require(
                FindSceneObjects(scene, "BottomNavTrialButton").Count == 1,
                "MainHome Trial button must be unique.");
            Require(
                trialButton.onClick.GetPersistentEventCount() == 0,
                "MainHome Trial button must not retain a persistent legacy route.");
            Require(!scene.isDirty, "MainHome verification must not dirty the scene.");
        }

        private static void VerifyWorldMapScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                TalismanSceneNavigationOwner.WorldMapScenePath,
                OpenSceneMode.Single);
            Button upgradeButton = FindSceneComponent<Button>(
                scene,
                "WorldMapUpgradeButton");
            Require(upgradeButton != null, "WorldMap Upgrade button is missing.");
            Require(
                FindSceneObjects(scene, "WorldMapUpgradeButton").Count == 1,
                "WorldMap Upgrade button must be unique.");

            V04WorldMapSceneController controller =
                FindSceneComponent<V04WorldMapSceneController>(
                    scene,
                    "V04WorldMapSceneController");
            Require(controller != null, "WorldMap controller is missing.");
            Require(
                controller.ValidateAuthoredBindings(out string diagnostic),
                "WorldMap authored bindings are invalid: " + diagnostic);
            Require(
                FindSceneObjects(scene, "StageNode_1").Count == 1 &&
                FindSceneObjects(scene, "StageNode_10").Count == 1,
                "WorldMap ten-stage authored baseline is incomplete.");
            Require(!scene.isDirty, "WorldMap verification must not dirty the scene.");
        }

        private static void VerifyUpgradeScene()
        {
            Scene scene = EditorSceneManager.OpenScene(
                TalismanSceneNavigationOwner.TalismanUpgradeScenePath,
                OpenSceneMode.Single);
            Button backButton = FindSceneComponent<Button>(
                scene,
                "BottomNav_Home");
            Require(backButton != null, "Upgrade Back button is missing.");
            Require(
                FindSceneObjects(scene, "BottomNav_Home").Count == 1,
                "Upgrade Back button must be unique.");
            Require(
                backButton.onClick.GetPersistentEventCount() == 0,
                "Upgrade Back button must not retain a persistent legacy route.");
            Require(!scene.isDirty, "Upgrade verification must not dirty the scene.");
        }

        private static T FindSceneComponent<T>(Scene scene, string objectName)
            where T : Component
        {
            GameObject target = FindSceneObjects(scene, objectName).SingleOrDefault();
            return target != null ? target.GetComponent<T>() : null;
        }

        private static List<GameObject> FindSceneObjects(Scene scene, string objectName)
        {
            List<GameObject> matches = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Collect(root.transform, objectName, matches);
            }

            return matches;
        }

        private static void Collect(
            Transform current,
            string objectName,
            ICollection<GameObject> matches)
        {
            if (current.name == objectName)
            {
                matches.Add(current.gameObject);
            }

            foreach (Transform child in current)
            {
                Collect(child, objectName, matches);
            }
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
