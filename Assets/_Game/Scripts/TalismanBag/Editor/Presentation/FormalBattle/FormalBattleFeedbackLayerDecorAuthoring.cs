using System;
using System.Linq;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattleFeedbackLayerDecorAuthoring
    {
        public const string TerminalMarker =
            "FORMAL_BATTLE_FEEDBACK_LAYER_PREFAB_PREVIEW_PASS";

        private const string UnifiedShellPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/"
            + "UnifiedBattlePageShell.prefab";
        private const string FeedbackLayerPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
            + "FormalBattleFeedbackLayer.prefab";

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Decor/"
            + "Prefabize And Preview Battle Feedback Layer",
            false,
            2490)]
        public static void AuthorFromMenu()
        {
            ApplyAuthoringPass();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplyAuthoringPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleFeedbackLayerDecorAuthoring] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplyAuthoringPass()
        {
            GameObject shellRoot = PrefabUtility.LoadPrefabContents(
                UnifiedShellPath);
            try
            {
                UnifiedBattlePageShell shell =
                    shellRoot.GetComponent<UnifiedBattlePageShell>();
                Require(shell != null,
                    "FORMAL_FEEDBACK_SHELL_COMPONENT_MISSING");
                Transform existingFeedback = shell.BattleFeedbackLayer;
                Require(existingFeedback != null,
                    "FORMAL_FEEDBACK_LAYER_MISSING");

                string currentAssetPath =
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        existingFeedback.gameObject);
                if (!string.Equals(
                        currentAssetPath,
                        FeedbackLayerPrefabPath,
                        StringComparison.Ordinal))
                {
                    CreateFeedbackLayerPrefab(existingFeedback);
                    existingFeedback = ReplaceShellFeedbackLayer(
                        shellRoot,
                        shell,
                        existingFeedback);
                }

                existingFeedback.gameObject.SetActive(true);
                EnablePreview(existingFeedback.gameObject);

                FormalBattlePresentationRoot presentationRoot = shellRoot
                    .GetComponentsInChildren<FormalBattlePresentationRoot>(true)
                    .SingleOrDefault();
                Require(presentationRoot != null,
                    "FORMAL_FEEDBACK_PRESENTATION_ROOT_MISSING");
                FormalBattleDamageFloatPool damageFloatPool =
                    existingFeedback.GetComponentInChildren<
                        FormalBattleDamageFloatPool>(true);
                FormalBattleCueFxAudioRoot cueFxAudioRoot =
                    existingFeedback.GetComponentInChildren<
                        FormalBattleCueFxAudioRoot>(true);
                Require(damageFloatPool != null && cueFxAudioRoot != null,
                    "FORMAL_FEEDBACK_PREFAB_CHILDREN_MISSING");

                presentationRoot.AssignExternalCompositionForEditor(
                    damageFloatPool,
                    cueFxAudioRoot);
                EditorUtility.SetDirty(presentationRoot);
                EditorUtility.SetDirty(shell);

                Require(shell.CollectMissingRequiredSlots().Count == 0,
                    "FORMAL_FEEDBACK_SHELL_SLOT_BINDING_INVALID");
                Require(presentationRoot.ValidateDownstreamCompositionReferences(
                        out string diagnostic),
                    diagnostic);

                PrefabUtility.SaveAsPrefabAsset(shellRoot, UnifiedShellPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log(TerminalMarker);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(shellRoot);
            }
        }

        private static void CreateFeedbackLayerPrefab(
            Transform sourceFeedback)
        {
            GameObject authoredRoot = new GameObject(
                "FormalBattleFeedbackLayer",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Shadow));
            try
            {
                CopyRectTransform(
                    sourceFeedback as RectTransform,
                    authoredRoot.transform as RectTransform);
                CopyComponent(
                    sourceFeedback.GetComponent<Image>(),
                    authoredRoot.GetComponent<Image>());
                CopyComponent(
                    sourceFeedback.GetComponent<Shadow>(),
                    authoredRoot.GetComponent<Shadow>());
                authoredRoot.SetActive(true);

                for (int index = 0;
                     index < sourceFeedback.childCount;
                     index++)
                {
                    CopyNestedPrefabInstance(
                        sourceFeedback.GetChild(index),
                        authoredRoot.transform,
                        index);
                }

                EnablePreview(authoredRoot);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    authoredRoot,
                    FeedbackLayerPrefabPath);
                Require(saved != null,
                    "FORMAL_FEEDBACK_PREFAB_SAVE_FAILED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(authoredRoot);
            }
        }

        private static Transform ReplaceShellFeedbackLayer(
            GameObject shellRoot,
            UnifiedBattlePageShell shell,
            Transform existingFeedback)
        {
            GameObject feedbackAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                FeedbackLayerPrefabPath);
            Require(feedbackAsset != null,
                "FORMAL_FEEDBACK_PREFAB_ASSET_MISSING");

            Transform parent = existingFeedback.parent;
            int siblingIndex = existingFeedback.GetSiblingIndex();
            RectTransform sourceRect = existingFeedback as RectTransform;
            GameObject instance = PrefabUtility.InstantiatePrefab(
                feedbackAsset,
                parent) as GameObject;
            Require(instance != null,
                "FORMAL_FEEDBACK_PREFAB_INSTANCE_FAILED");

            instance.name = "BattleFeedbackLayer";
            instance.transform.SetSiblingIndex(siblingIndex);
            CopyRectTransform(
                sourceRect,
                instance.transform as RectTransform);
            instance.SetActive(true);

            shell.AssignFormalCompositionForEditor(
                shell.BattlePageRoot,
                shell.EnemyInfoArea,
                instance.transform,
                shell.ItemDetailPopupSlot);
            EditorUtility.SetDirty(shell);
            UnityEngine.Object.DestroyImmediate(existingFeedback.gameObject);
            return instance.transform;
        }

        private static void CopyNestedPrefabInstance(
            Transform source,
            Transform destinationParent,
            int siblingIndex)
        {
            GameObject sourceAsset =
                PrefabUtility.GetCorrespondingObjectFromOriginalSource(
                    source.gameObject);
            Require(sourceAsset != null,
                "FORMAL_FEEDBACK_CHILD_NOT_PREFAB " + source.name);

            GameObject instance = PrefabUtility.InstantiatePrefab(
                sourceAsset,
                destinationParent) as GameObject;
            Require(instance != null,
                "FORMAL_FEEDBACK_CHILD_INSTANCE_FAILED " + source.name);
            instance.name = source.gameObject.name;
            instance.transform.SetSiblingIndex(siblingIndex);
            instance.SetActive(source.gameObject.activeSelf);

            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(source.gameObject);
            if (modifications != null && modifications.Length > 0)
            {
                PrefabUtility.SetPropertyModifications(instance, modifications);
            }
            CopyRectTransform(
                source as RectTransform,
                instance.transform as RectTransform);
        }

        private static void EnablePreview(GameObject root)
        {
            root.SetActive(true);
            FormalBattleDamageFloatPool damageFloatPool =
                root.GetComponentInChildren<FormalBattleDamageFloatPool>(true);
            FormalBattleCueFxAudioRoot cueFxAudioRoot =
                root.GetComponentInChildren<FormalBattleCueFxAudioRoot>(true);
            Require(damageFloatPool != null && cueFxAudioRoot != null,
                "FORMAL_FEEDBACK_PREVIEW_COMPONENTS_MISSING");

            damageFloatPool.SetAuthoringPreviewForEditor(true);
            cueFxAudioRoot.SetAuthoringPreviewForEditor(true);
            RecordHierarchyOverrides(damageFloatPool.gameObject);
            RecordHierarchyOverrides(cueFxAudioRoot.gameObject);
        }

        private static void RecordHierarchyOverrides(GameObject root)
        {
            foreach (Transform value in root
                         .GetComponentsInChildren<Transform>(true))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    value.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(value);
            }
            foreach (Component value in root
                         .GetComponentsInChildren<Component>(true))
            {
                if (value != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(
                        value);
                    EditorUtility.SetDirty(value);
                }
            }
        }

        private static void CopyRectTransform(
            RectTransform source,
            RectTransform destination)
        {
            Require(source != null && destination != null,
                "FORMAL_FEEDBACK_RECT_MISSING");
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.anchoredPosition3D = source.anchoredPosition3D;
            destination.sizeDelta = source.sizeDelta;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;
        }

        private static void CopyComponent<T>(T source, T destination)
            where T : Component
        {
            Require(source != null && destination != null,
                "FORMAL_FEEDBACK_VISUAL_COMPONENT_MISSING "
                + typeof(T).Name);
            EditorUtility.CopySerialized(source, destination);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }
    }
}
