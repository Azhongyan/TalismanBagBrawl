using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.BuildSandbox
{
    /// <summary>
    /// Explicit, target-scene-only authoring entrypoint for the one Arrange button.
    /// It deliberately never touches a user's visual or RectTransform edits after creation.
    /// </summary>
    public static class BattleSandboxTrayStableLayoutAndManualSortAuthoring
    {
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string ArrangeButtonName = "TrayArrangeButton";
        private const string MenuPath =
            "Talisman Bag/V0.4/Apply BattleSandbox Tray Stable Layout And Manual Sort";

        [MenuItem(MenuPath)]
        public static void Apply()
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException("Exit Play Mode before authoring TrayArrangeButton.");
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.Equals(activeScene.path, TargetScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Open the target BattleSandbox scene before running this explicit authoring command.");
            }

            BuildGridInteractionPreviewController controller =
                UnityEngine.Object.FindObjectOfType<BuildGridInteractionPreviewController>(true);
            if (controller == null)
            {
                throw new InvalidOperationException("BuildGridInteractionPreviewController is missing.");
            }
            BuildItemTrayPreviewView trayView = ResolveSerializedTrayView(controller);
            ValidateTrayViewParent(activeScene, trayView);

            GameObject[] matches = FindNamedObjects(activeScene, ArrangeButtonName);
            if (matches.Length > 1)
            {
                throw new InvalidOperationException("TrayArrangeButton is duplicated.");
            }

            GameObject buttonRoot = matches.Length == 1
                ? matches[0]
                : CreateButtonRoot(trayView.transform);
            Button button = ValidateButtonRoot(buttonRoot, trayView);

            SerializedObject serializedController = new(controller);
            SerializedProperty buttonProperty = serializedController.FindProperty("trayArrangeButton");
            if (buttonProperty == null)
            {
                throw new InvalidOperationException("Tray arrange button binding field is missing.");
            }
            buttonProperty.objectReferenceValue = button;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        public static void ApplyVerifyAndSaveClosedCleanBatch()
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "Closed-clean authoring cannot run in Play Mode.");
            }

            Scene targetScene = EditorSceneManager.OpenScene(
                TargetScenePath,
                OpenSceneMode.Single);
            Apply();
            BattleSandboxTrayStableLayoutAndManualSortVerifier.Run();
            if (!EditorSceneManager.SaveScene(targetScene))
            {
                throw new InvalidOperationException(
                    "Failed to save the verified BattleSandbox Scene.");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateApply()
        {
            return !Application.isPlaying
                && string.Equals(SceneManager.GetActiveScene().path, TargetScenePath,
                    StringComparison.Ordinal);
        }

        private static BuildItemTrayPreviewView ResolveSerializedTrayView(
            BuildGridInteractionPreviewController controller)
        {
            SerializedObject serializedController = new(controller);
            SerializedProperty trayViewProperty =
                serializedController.FindProperty("itemTrayView");
            BuildItemTrayPreviewView trayView =
                trayViewProperty?.objectReferenceValue as BuildItemTrayPreviewView;
            if (trayView == null)
            {
                throw new InvalidOperationException(
                    "Controller serialized itemTrayView reference is missing.");
            }
            return trayView;
        }

        private static void ValidateTrayViewParent(
            Scene activeScene,
            BuildItemTrayPreviewView trayView)
        {
            if (trayView == null
                || trayView.gameObject.scene != activeScene
                || trayView.GetComponentInParent<Canvas>(true) == null)
            {
                throw new InvalidOperationException(
                    "Serialized itemTrayView must belong to the target Scene and have an existing Canvas ancestor.");
            }
        }

        private static GameObject[] FindNamedObjects(Scene scene, string objectName)
        {
            List<GameObject> matches = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                matches.AddRange(root.GetComponentsInChildren<Transform>(true)
                    .Where(transformValue => transformValue != null
                        && string.Equals(transformValue.name, objectName,
                            StringComparison.Ordinal))
                    .Select(transformValue => transformValue.gameObject));
            }
            return matches.ToArray();
        }

        private static GameObject CreateButtonRoot(Transform parent)
        {
            // This is the complete V1 structure.  Do not add children, layout components, text,
            // sprites, colors, or transform defaults: those belong to the user's authored scene.
            GameObject root = new(ArrangeButtonName, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            Undo.RegisterCreatedObjectUndo(root, "Create Tray Arrange Button");
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            Button button = root.GetComponent<Button>();
            button.targetGraphic = image;
            return root;
        }

        private static Button ValidateButtonRoot(
            GameObject root,
            BuildItemTrayPreviewView trayView)
        {
            Component[] components = root == null
                ? Array.Empty<Component>()
                : root.GetComponents<Component>();
            Image image = root == null ? null : root.GetComponent<Image>();
            Button button = root == null ? null : root.GetComponent<Button>();
            if (root == null || root.transform.parent != trayView.transform
                || root.GetComponent<RectTransform>() == null
                || root.GetComponent<CanvasRenderer>() == null
                || image == null
                || button == null
                || components.Length != 4
                || root.transform.childCount != 0
                || button.targetGraphic != image
                || root.GetComponentInParent<Canvas>(true) == null
                || IsInsideTrayScrollingSubtree(root.transform, trayView))
            {
                throw new InvalidOperationException(
                    "TrayArrangeButton must be one four-component object directly under the serialized itemTrayView and outside its scrolling subtree.");
            }
            return button;
        }

        private static bool IsInsideTrayScrollingSubtree(
            Transform buttonTransform,
            BuildItemTrayPreviewView trayView)
        {
            SerializedObject serializedTrayView = new(trayView);
            RectTransform contentRoot = serializedTrayView.FindProperty("contentRoot")
                ?.objectReferenceValue as RectTransform;
            RectTransform cardLayer = serializedTrayView.FindProperty("itemCardLayer")
                ?.objectReferenceValue as RectTransform;
            ScrollRect scrollRect = serializedTrayView.FindProperty("scrollRect")
                ?.objectReferenceValue as ScrollRect;
            RectTransform viewport = scrollRect == null ? null : scrollRect.viewport;
            return IsSameOrDescendant(buttonTransform, contentRoot)
                || IsSameOrDescendant(buttonTransform, cardLayer)
                || IsSameOrDescendant(buttonTransform, viewport);
        }

        private static bool IsSameOrDescendant(Transform value, Transform ancestor)
        {
            return value != null && ancestor != null
                && (value == ancestor || value.IsChildOf(ancestor));
        }
    }
}
