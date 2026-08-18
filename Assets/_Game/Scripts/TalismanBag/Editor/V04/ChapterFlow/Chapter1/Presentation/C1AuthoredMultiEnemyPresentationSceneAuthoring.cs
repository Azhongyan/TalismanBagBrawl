using System;
using System.Linq;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Presentation
{
    public static class C1AuthoredMultiEnemyPresentationSceneAuthoring
    {
        public const string PackageId =
            "V0.4-C1AuthoredMultiEnemyPresentationSlotsWhiteImage01";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";

        private const string MenuPath =
            "Tools/Talisman Bag/Dev Only/V0.4/B-Line Chapter 1/[Authoring] Apply Multi-Enemy Slots + White Image Fix";

        [MenuItem(MenuPath)]
        public static void ApplyMenu()
        {
            ApplyAndSave(throwOnFailure: false);
        }

        public static void ApplyBatch()
        {
            ApplyAndSave(throwOnFailure: true);
        }

        public static bool ApplyAndSave(bool throwOnFailure)
        {
            try
            {
                if (Application.isPlaying)
                {
                    throw new InvalidOperationException(
                        "AUTHORING_REJECTED_WHILE_PLAYING");
                }
                if (Enumerable.Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .Any(value => value.IsValid() && value.isDirty))
                {
                    throw new InvalidOperationException(
                        "AUTHORING_REJECTED_DIRTY_SCENE_OPEN");
                }

                Scene scene = SceneManager.GetActiveScene();
                if (!scene.IsValid()
                    || !string.Equals(
                        scene.path,
                        TargetScenePath,
                        StringComparison.Ordinal))
                {
                    scene = EditorSceneManager.OpenScene(
                        TargetScenePath,
                        OpenSceneMode.Single);
                }
                if (!scene.IsValid()
                    || !string.Equals(
                        scene.path,
                        TargetScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "TARGET_SCENE_NOT_OPEN");
                }

                RectTransform enemyArea = FindUniqueDirectOrNested<
                    RectTransform>(
                    scene,
                    value => string.Equals(
                        value.gameObject.name,
                        "V02EnemyArea",
                        StringComparison.Ordinal),
                    "V02_ENEMY_AREA");
                C1AuthoredBossPresentationSlotMarker bossMarker =
                    enemyArea.GetComponentsInChildren<
                            C1AuthoredBossPresentationSlotMarker>(true)
                        .SingleOrDefault();
                Image bossImage = bossMarker != null
                    ? bossMarker.VisualRenderer
                    : FindUniqueDirectChild<Image>(
                        enemyArea,
                        "Shougunu_1");
                bossMarker ??= Undo.AddComponent<
                    C1AuthoredBossPresentationSlotMarker>(
                    bossImage.gameObject);
                SerializedObject bossMarkerObject = new(bossMarker);
                bossMarkerObject.FindProperty("stableBossSlotId")
                    .stringValue = "BossSlot";
                bossMarkerObject.FindProperty("visualRenderer")
                    .objectReferenceValue = bossImage;
                bossMarkerObject.ApplyModifiedPropertiesWithoutUndo();

                C1AuthoredEnemyPresentationRoot root =
                    enemyArea.GetComponent<
                        C1AuthoredEnemyPresentationRoot>();
                C1AuthoredEnemyPresentationSlotView[] existingViews =
                    enemyArea.GetComponentsInChildren<
                        C1AuthoredEnemyPresentationSlotView>(true);

                Image slot01Image;
                Image slot02Image;
                Image slot03Image;
                if (root != null && existingViews.Length == 3)
                {
                    slot01Image = FindStableSlot(
                        existingViews,
                        "Slot01").VisualRenderer;
                    slot02Image = FindStableSlot(
                        existingViews,
                        "Slot02").VisualRenderer;
                    slot03Image = FindStableSlot(
                        existingViews,
                        "Slot03").VisualRenderer;
                }
                else
                {
                    if (root != null || existingViews.Length != 0)
                    {
                        throw new InvalidOperationException(
                            "PARTIAL_AUTHORED_SLOT_CONTRACT_REJECTED");
                    }
                    slot01Image = FindUniqueDirectChild<Image>(
                        enemyArea,
                        "Enemy");
                    slot02Image = FindUniqueDirectChild<Image>(
                        enemyArea,
                        "Enemy_2");
                    slot03Image = CreateSlot03(
                        enemyArea,
                        slot02Image);
                }

                C1AuthoredEnemyPresentationSlotView slot01 =
                    ConfigureSlot(slot01Image, "Slot01");
                C1AuthoredEnemyPresentationSlotView slot02 =
                    ConfigureSlot(slot02Image, "Slot02");
                C1AuthoredEnemyPresentationSlotView slot03 =
                    ConfigureSlot(slot03Image, "Slot03");
                root ??= Undo.AddComponent<
                    C1AuthoredEnemyPresentationRoot>(
                    enemyArea.gameObject);
                SerializedObject rootObject = new(root);
                SerializedProperty slots =
                    rootObject.FindProperty("ordinarySlots");
                slots.arraySize = 3;
                slots.GetArrayElementAtIndex(0)
                    .objectReferenceValue = slot01;
                slots.GetArrayElementAtIndex(1)
                    .objectReferenceValue = slot02;
                slots.GetArrayElementAtIndex(2)
                    .objectReferenceValue = slot03;
                rootObject.ApplyModifiedPropertiesWithoutUndo();

                Color safeBossColor = bossImage.color;
                safeBossColor.a = 0f;
                bossImage.color = safeBossColor;
                bossImage.enabled = false;
                bossImage.raycastTarget = false;

                EditorUtility.SetDirty(slot01);
                EditorUtility.SetDirty(slot02);
                EditorUtility.SetDirty(slot03);
                EditorUtility.SetDirty(root);
                EditorUtility.SetDirty(bossMarker);
                EditorUtility.SetDirty(bossImage);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(
                        scene,
                        TargetScenePath))
                {
                    throw new InvalidOperationException(
                        "TARGET_SCENE_SAVE_FAILED");
                }

                Debug.Log(
                    "[" + PackageId
                    + "] Authored Slot01/02/03 contract saved; existing Slot01/02 geometry and style preserved; Slot03 authored once; Shougunu empty Image baseline suppressed.");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[" + PackageId + "] "
                    + exception.Message);
                if (throwOnFailure)
                {
                    throw;
                }
                return false;
            }
        }

        private static Image CreateSlot03(
            RectTransform enemyArea,
            Image slot02)
        {
            GameObject clone = UnityEngine.Object.Instantiate(
                slot02.gameObject,
                enemyArea,
                worldPositionStays: false);
            clone.name = "Enemy_3";
            Undo.RegisterCreatedObjectUndo(
                clone,
                "Create C1 Authored Enemy Slot03");
            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.anchoredPosition =
                slot02.rectTransform.anchoredPosition
                + new Vector2(-160f, -40f);
            Image image = clone.GetComponent<Image>();
            image.sprite = null;
            image.enabled = false;
            image.raycastTarget = false;
            clone.SetActive(false);
            return image;
        }

        private static C1AuthoredEnemyPresentationSlotView
            ConfigureSlot(Image image, string displaySlotId)
        {
            C1AuthoredEnemyCalibrationMeshEffect calibration =
                image.GetComponent<
                    C1AuthoredEnemyCalibrationMeshEffect>()
                ?? Undo.AddComponent<
                    C1AuthoredEnemyCalibrationMeshEffect>(
                    image.gameObject);
            C1AuthoredEnemyPresentationSlotView view =
                image.GetComponent<
                    C1AuthoredEnemyPresentationSlotView>()
                ?? Undo.AddComponent<
                    C1AuthoredEnemyPresentationSlotView>(
                    image.gameObject);
            C1AuthoredEnemySelectionOutlineEffect selection =
                image.GetComponent<
                    C1AuthoredEnemySelectionOutlineEffect>()
                ?? Undo.AddComponent<
                    C1AuthoredEnemySelectionOutlineEffect>(
                    image.gameObject);
            selection.enabled = false;
            SerializedObject viewObject = new(view);
            viewObject.FindProperty("displaySlotId").stringValue =
                displaySlotId;
            viewObject.FindProperty("visualRenderer")
                .objectReferenceValue = image;
            viewObject.FindProperty("selectionVisual")
                .objectReferenceValue = selection;
            viewObject.FindProperty("calibrationEffect")
                .objectReferenceValue = calibration;
            viewObject.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static C1AuthoredEnemyPresentationSlotView FindStableSlot(
            C1AuthoredEnemyPresentationSlotView[] views,
            string displaySlotId)
        {
            C1AuthoredEnemyPresentationSlotView[] matches =
                views.Where(value => string.Equals(
                        value.DisplaySlotId,
                        displaySlotId,
                        StringComparison.Ordinal))
                    .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    displaySlotId + "_STABLE_ID_NOT_UNIQUE");
            }
            return matches[0];
        }

        private static T FindUniqueDirectChild<T>(
            Component parent,
            string objectName)
            where T : Component
        {
            T[] matches = parent.GetComponentsInChildren<T>(true)
                .Where(value => value.transform.parent
                    == parent.transform)
                .Where(value => string.Equals(
                    value.gameObject.name,
                    objectName,
                    StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    objectName + "_DIRECT_CHILD_NOT_UNIQUE_"
                    + matches.Length);
            }
            return matches[0];
        }

        private static T FindUniqueDirectOrNested<T>(
            Scene scene,
            Func<T, bool> predicate,
            string diagnosticName)
            where T : Component
        {
            T[] matches = Resources.FindObjectsOfTypeAll<T>()
                .Where(value => value != null
                    && value.gameObject.scene == scene
                    && predicate(value))
                .ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidOperationException(
                    diagnosticName + "_NOT_UNIQUE_"
                    + matches.Length);
            }
            return matches[0];
        }
    }
}
