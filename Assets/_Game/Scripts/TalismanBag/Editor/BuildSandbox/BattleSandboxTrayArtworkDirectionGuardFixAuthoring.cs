using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxTrayArtworkDirectionGuardFixAuthoring
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TaskStartSceneHash =
            "f505d83387e3102ce358b90cf16c6061325057f7c7f811f98e20df5b17e71d3b";
        private const ulong ItemCard04ImageRectFileId = 1190502616;
        private const ulong ItemCard05ImageRectFileId = 1283613157;

        [MenuItem("Talisman Bag/V0.4/Apply BattleSandbox Tray Artwork Direction GuardFix")]
        public static void ApplyStaticBatch()
        {
            try
            {
                Apply();
                Debug.Log(
                    "[BattleSandboxTrayArtworkDirectionGuardFixAuthoring]\n"
                    + "TASK_START_GATE_PASS\n"
                    + "SCENE_EXACT_TWO_TRANSFORM_DELTA_PASS");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[BattleSandboxTrayArtworkDirectionGuardFixAuthoring] "
                    + exception);
                throw;
            }
        }

        private static void Apply()
        {
            string absoluteScenePath = ProjectPath(ScenePath);
            Check(File.Exists(absoluteScenePath), "Target Scene missing.");
            Check(string.Equals(Sha256File(absoluteScenePath),
                    TaskStartSceneHash, StringComparison.Ordinal),
                "GUARD_RETURN_BATTLESANDBOXTRAYARTWORKDIRECTIONGUARDFIX01_TASK_START_DRIFT");

            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            NormalizeNamedImageRect(
                scene,
                "ItemCard_04_image",
                ItemCard04ImageRectFileId);
            NormalizeNamedImageRect(
                scene,
                "ItemCard_05_image",
                ItemCard05ImageRectFileId);

            Check(EditorSceneManager.SaveScene(scene),
                "Failed to save target Scene.");
        }

        private static void NormalizeNamedImageRect(
            Scene scene,
            string objectName,
            ulong expectedRectFileId)
        {
            RectTransform[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<RectTransform>(true))
                .Where(rect => rect != null
                    && string.Equals(rect.name, objectName, StringComparison.Ordinal))
                .ToArray();
            Check(matches.Length == 1,
                "Expected exactly one RectTransform named " + objectName
                + ", found " + matches.Length + ".");

            RectTransform rectTransform = matches[0];
            GlobalObjectId globalId =
                GlobalObjectId.GetGlobalObjectIdSlow(rectTransform);
            Check(globalId.targetObjectId == expectedRectFileId,
                objectName + " RectTransform fileID mismatch. Expected "
                + expectedRectFileId + ", found " + globalId.targetObjectId
                + ".");

            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localEulerAngles = Vector3.zero;
            SerializedObject serialized = new(rectTransform);
            SerializedProperty eulerHint =
                serialized.FindProperty("m_LocalEulerAnglesHint");
            if (eulerHint != null)
            {
                eulerHint.vector3Value = Vector3.zero;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(rectTransform);
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Sha256File(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
