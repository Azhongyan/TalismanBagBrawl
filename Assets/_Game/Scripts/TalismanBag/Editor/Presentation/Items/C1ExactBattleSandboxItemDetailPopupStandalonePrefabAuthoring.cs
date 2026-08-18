using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Detail.UI;
using TalismanBag.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.Editor.Presentation.Items
{
    public static class
        C1ExactBattleSandboxItemDetailPopupStandalonePrefabAuthoring
    {
        public const string TerminalMarker =
            "C1_EXACT_ITEM_DETAIL_STANDALONE_PREFAB_AUTHORED_PASS";

        // Unity YAML/import round-trips may change the last serialized digits.
        // These tolerances are far below a visible UI pixel/layout change.
        private const float RectValueTolerance = 0.0001f;
        private const float RectRotationToleranceDegrees = 0.001f;
        private const float RectSignatureQuantum = 0.001f;
        private const float RectRotationSignatureQuantum = 0.00001f;

        private const string SourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string SourceRootName = "BuildSandboxPreviewCanvas";
        private const string OutputRootName =
            "C1ExactBattleSandboxItemDetailPopupStandalone";
        private const string OutputPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemDetailPopupStandalone.prefab";
        private const string RetainedPath =
            "MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/ItemDetailPanel";

        private static readonly string[] HeaderTextFields =
        {
            "itemNameText",
            "metaText",
            "powerText",
            "artworkText",
            "artworkKeyText",
            "rarityBadgeText"
        };

        private static readonly string[] DynamicImageFields =
        {
            "artworkImageSlot",
            "rarityBadgeImage",
            "faMenIconImage",
            "qiLeiIconImage"
        };

        private sealed class ProjectionReset
        {
            public int ClearedTextCount;
            public int ClearedSpriteCount;
            public HashSet<string> DynamicTextPaths =
                new HashSet<string>(StringComparer.Ordinal);
            public HashSet<string> DynamicImagePaths =
                new HashSet<string>(StringComparer.Ordinal);
        }

        private sealed class RectGeometrySnapshot
        {
            public string Path = string.Empty;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
        }

        private sealed class ParitySnapshot
        {
            public string[] PanelDirectChildren = Array.Empty<string>();
            public RectGeometrySnapshot[] ChainGeometry =
                Array.Empty<RectGeometrySnapshot>();
            public RectGeometrySnapshot[] AllRectGeometry =
                Array.Empty<RectGeometrySnapshot>();
            public int RetainedObjectCount;
            public int PanelObjectCount;
            public int ComponentCount;
            public int ImageCount;
            public int TextCount;
            public int TmpTextCount;
            public int ScrollRectCount;
            public int MaskCount;
            public int SelectableCount;
            public int ForbiddenAdapterCount;
            public int ExternalReferenceCount;
            public int ExternalPersistentListenerCount;
            public string[] PanelHierarchy = Array.Empty<string>();
            public string[] ComponentOrder = Array.Empty<string>();
            public string[] CanvasProperties = Array.Empty<string>();
            public string[] StaticImageProperties = Array.Empty<string>();
            public string[] DynamicImageStyleProperties = Array.Empty<string>();
            public string[] StaticTextProperties = Array.Empty<string>();
            public string[] DynamicTextStyleProperties = Array.Empty<string>();
            public string[] TmpTextProperties = Array.Empty<string>();
            public string[] ScrollProperties = Array.Empty<string>();
            public string[] MaskProperties = Array.Empty<string>();
            public string[] SelectableProperties = Array.Empty<string>();
        }

        [MenuItem(
            "TalismanBag/V0.4/Items/Author Exact Item Detail Standalone Prefab",
            false,
            2475)]
        public static void AuthorFromMenu()
        {
            AuthorAndValidate();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                AuthorAndValidate();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[ExactItemDetailStandaloneAuthoring] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void AuthorAndValidate()
        {
            Require(Directory.Exists(Path.GetDirectoryName(OutputPrefabPath)),
                "STANDALONE_ITEM_DETAIL_PREFAB_FOLDER_MISSING");
            Require(File.Exists(SourceScenePath),
                "STANDALONE_ITEM_DETAIL_SOURCE_SCENE_MISSING "
                + SourceScenePath);

            Scene sourceScene = EditorSceneManager.OpenScene(
                    SourceScenePath,
                    OpenSceneMode.Single);
                Require(sourceScene.IsValid() && sourceScene.isLoaded,
                    "STANDALONE_ITEM_DETAIL_SOURCE_SCENE_OPEN_FAILED");
                Require(!sourceScene.isDirty,
                    "STANDALONE_ITEM_DETAIL_SOURCE_SCENE_DIRTY_AT_OPEN");

                Transform sourceRoot = FindUniqueSceneTransform(
                    sourceScene,
                    SourceRootName);
                Transform sourcePanel = RequiredPath(sourceRoot, RetainedPath);
                Require(FindSceneTransformsByPath(
                            sourceScene,
                            SourceRootName + "/" + RetainedPath).Count == 1,
                    "STANDALONE_ITEM_DETAIL_SOURCE_PATH_DUPLICATED");
                Require(sourcePanel.GetComponent<ItemDetailPanelView>() != null,
                    "STANDALONE_ITEM_DETAIL_SOURCE_VIEW_MISSING");
                Require(sourcePanel.GetComponent<ItemSandboxDetailPanelView>() != null,
                    "STANDALONE_ITEM_DETAIL_SOURCE_SANDBOX_ADAPTER_MISSING");

                ItemDetailPanelView sourceView = RequiredSingleComponent<
                    ItemDetailPanelView>(sourcePanel.gameObject);
                ProjectionReset sourceProjection = CollectProjectionTargets(
                    sourceRoot,
                    sourceView);
                ParitySnapshot sourceSnapshot = CaptureSnapshot(
                    sourceRoot,
                    sourcePanel,
                    sourceProjection.DynamicTextPaths,
                    sourceProjection.DynamicImagePaths);
                string[] sourcePanelChildren = sourcePanel.Cast<Transform>()
                    .Select(value => value.name)
                    .ToArray();
                Require(sourcePanelChildren.Length == 5,
                    "STANDALONE_ITEM_DETAIL_SOURCE_CHILD_COUNT_INVALID "
                    + sourcePanelChildren.Length);

                Scene previewScene = EditorSceneManager.NewPreviewScene();
                GameObject clone = Object.Instantiate(sourceRoot.gameObject);
                SceneManager.MoveGameObjectToScene(clone, previewScene);
                clone.name = OutputRootName;

            try
            {
                Transform clonePanel = RequiredPath(clone.transform, RetainedPath);
                Transform mobile = RequiredDirectChild(
                    clone.transform,
                    "MobileSafeAreaRoot");
                Transform safe = RequiredDirectChild(mobile, "SafeAreaRoot");
                Transform popup = RequiredDirectChild(safe, "PopupLayer");
                clonePanel = RequiredDirectChild(popup, "ItemDetailPanel");
                KeepOnlyDirectChild(clone.transform, mobile);
                KeepOnlyDirectChild(mobile, safe);
                KeepOnlyDirectChild(safe, popup);
                KeepOnlyDirectChild(popup, clonePanel);

                string safeAreaDifference = NormalizeSafeAreaHelper(
                    clone.transform,
                    mobile);
                List<string> removedOwners = StripForbiddenOwners(clone);
                int clearedExternalReferences =
                    ClearExternalSceneReferences(clone);
                int removedExternalListeners =
                    RemoveExternalPersistentListeners(clone);
                ItemDetailPanelView cloneView = RequiredSingleComponent<
                    ItemDetailPanelView>(
                    clonePanel.gameObject);
                ProjectionReset projectionReset = ClearSampleProjection(
                    clone.transform,
                    cloneView);
                Require(!sourceScene.isDirty,
                    "STANDALONE_ITEM_DETAIL_SOURCE_SCENE_DIRTIED");
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    clone,
                    OutputPrefabPath);
                Require(saved != null,
                    "STANDALONE_ITEM_DETAIL_PREFAB_SAVE_FAILED");
                AssetDatabase.ImportAsset(
                    OutputPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
                GameObject output = PrefabUtility.LoadPrefabContents(
                    OutputPrefabPath);
                try
                {
                    Transform outputPanel = RequiredPath(
                        output.transform,
                        RetainedPath);
                    ItemDetailPanelView outputView =
                        RequiredSingleComponent<ItemDetailPanelView>(
                            outputPanel.gameObject);
                    ProjectionReset outputProjection =
                        CollectProjectionTargets(output.transform, outputView);
                    ParitySnapshot outputSnapshot = CaptureSnapshot(
                        output.transform,
                        outputPanel,
                        outputProjection.DynamicTextPaths,
                        outputProjection.DynamicImagePaths);

                    ValidateOutput(
                        output,
                        outputPanel,
                        sourcePanelChildren,
                        outputProjection);
                    ValidateParity(sourceSnapshot, outputSnapshot);

                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(output);
                }

                Require(!sourceScene.isDirty,
                    "STANDALONE_ITEM_DETAIL_SOURCE_SCENE_DIRTY_AT_END");
                Debug.Log(
                    TerminalMarker
                    + " root=1 chain=Canvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/ItemDetailPanel"
                    + " removedOwners=" + removedOwners.Count
                    + " clearedExternalReferences=" + clearedExternalReferences
                    + " removedExternalListeners=" + removedExternalListeners
                    + " clearedText=" + projectionReset.ClearedTextCount
                    + " clearedSprite=" + projectionReset.ClearedSpriteCount
                    + " safeArea=" + safeAreaDifference
                    + " sourceSceneWrites=0 integrationWrites=0 runtimeWrites=0");
            }
            finally
            {
                Object.DestroyImmediate(clone);
                EditorSceneManager.ClosePreviewScene(previewScene);
            }
        }

        private static Transform FindUniqueSceneTransform(
            Scene scene,
            string exactPath)
        {
            List<Transform> matches = FindSceneTransformsByPath(scene, exactPath);
            Require(matches.Count == 1,
                "STANDALONE_ITEM_DETAIL_SOURCE_ROOT_COUNT_INVALID "
                + matches.Count);
            return matches[0];
        }

        private static List<Transform> FindSceneTransformsByPath(
            Scene scene,
            string exactPath)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => string.Equals(
                    ScenePath(value),
                    exactPath,
                    StringComparison.Ordinal))
                .ToList();
        }

        private static string ScenePath(Transform target)
        {
            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }
            return string.Join("/", names);
        }

        private static Transform RequiredPath(Transform root, string path)
        {
            Transform value = root == null ? null : root.Find(path);
            Require(value != null,
                "STANDALONE_ITEM_DETAIL_REQUIRED_PATH_MISSING " + path);
            return value;
        }

        private static Transform RequiredDirectChild(
            Transform parent,
            string name)
        {
            Transform[] matches = parent.Cast<Transform>()
                .Where(value => string.Equals(
                    value.name,
                    name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "STANDALONE_ITEM_DETAIL_DIRECT_CHILD_COUNT_INVALID "
                + name + "=" + matches.Length);
            return matches[0];
        }

        private static void KeepOnlyDirectChild(
            Transform parent,
            Transform retained)
        {
            Transform[] children = parent.Cast<Transform>().ToArray();
            Require(children.Contains(retained),
                "STANDALONE_ITEM_DETAIL_RETAINED_CHILD_NOT_FOUND "
                + retained.name);
            foreach (Transform child in children)
            {
                if (child != retained)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static string NormalizeSafeAreaHelper(
            Transform root,
            Transform mobile)
        {
            MobileSafeAreaFitter[] fitters = mobile.GetComponents<
                MobileSafeAreaFitter>();
            if (fitters.Length == 0)
            {
                return "Source retained chain has no MobileSafeAreaFitter.";
            }
            Require(fitters.Length == 1,
                "STANDALONE_ITEM_DETAIL_SAFE_AREA_HELPER_COUNT_INVALID "
                + fitters.Length);
            SerializedProperty target = new SerializedObject(fitters[0])
                .FindProperty("target");
            Object referenced = target == null
                ? null
                : target.objectReferenceValue;
            if (referenced != null && !IsInternalReference(root, referenced))
            {
                Object.DestroyImmediate(fitters[0]);
                return "Removed MobileSafeAreaFitter because its target was outside the retained subtree; copied 1080x1920 geometry remains authored.";
            }
            return "Retained MobileSafeAreaFitter; its target is local to the standalone Prefab.";
        }

        private static List<string> StripForbiddenOwners(GameObject root)
        {
            List<string> removed = new List<string>();
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<
                MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }
                string typeName = behaviour.GetType().FullName ?? string.Empty;
                if (!IsForbiddenOwnerType(typeName))
                {
                    continue;
                }
                removed.Add(typeName);
                Object.DestroyImmediate(behaviour);
            }
            Require(removed.Any(value => string.Equals(
                    value,
                    typeof(ItemSandboxDetailPanelView).FullName,
                    StringComparison.Ordinal)),
                "STANDALONE_ITEM_DETAIL_SANDBOX_ADAPTER_NOT_REMOVED");
            return removed.OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static bool IsForbiddenOwnerType(string typeName)
        {
            string value = typeName ?? string.Empty;
            return value.Contains("ItemSandbox")
                   || value.Contains("BuildSandbox")
                   || value.Contains("BattleSandbox")
                   || value.Contains("BuildGridInteractionPreview");
        }

        private static int ClearExternalSceneReferences(GameObject root)
        {
            int cleared = 0;
            foreach (Component component in root.GetComponentsInChildren<
                         Component>(true))
            {
                if (component == null)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                bool changed = false;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (iterator.propertyType
                        != SerializedPropertyType.ObjectReference
                        || ShouldSkipObjectReference(iterator.propertyPath))
                    {
                        continue;
                    }
                    Object value = iterator.objectReferenceValue;
                    if (value == null
                        || EditorUtility.IsPersistent(value)
                        || IsInternalReference(root.transform, value))
                    {
                        continue;
                    }
                    iterator.objectReferenceValue = null;
                    changed = true;
                    cleared++;
                }
                if (changed)
                {
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            return cleared;
        }

        private static int RemoveExternalPersistentListeners(GameObject root)
        {
            int removed = 0;
            foreach (Component component in root.GetComponentsInChildren<
                         Component>(true))
            {
                if (component == null)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                List<string> callArrays = new List<string>();
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (iterator.isArray
                        && iterator.propertyPath.EndsWith(
                            "m_PersistentCalls.m_Calls",
                            StringComparison.Ordinal))
                    {
                        callArrays.Add(iterator.propertyPath);
                    }
                }

                bool changed = false;
                foreach (string path in callArrays.Distinct())
                {
                    SerializedProperty calls = serialized.FindProperty(path);
                    if (calls == null || !calls.isArray)
                    {
                        continue;
                    }
                    for (int index = calls.arraySize - 1; index >= 0; index--)
                    {
                        SerializedProperty call = calls.GetArrayElementAtIndex(
                            index);
                        SerializedProperty target = call.FindPropertyRelative(
                            "m_Target");
                        Object value = target == null
                            ? null
                            : target.objectReferenceValue;
                        if (value != null
                            && IsInternalReference(root.transform, value))
                        {
                            continue;
                        }
                        calls.DeleteArrayElementAtIndex(index);
                        removed++;
                        changed = true;
                    }
                }
                if (changed)
                {
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            return removed;
        }

        private static ProjectionReset CollectProjectionTargets(
            Transform root,
            ItemDetailPanelView view)
        {
            ProjectionReset result = new ProjectionReset();
            SerializedObject serialized = new SerializedObject(view);
            foreach (string field in HeaderTextFields)
            {
                AddReferencedPath<Text>(
                    root,
                    serialized.FindProperty(field),
                    result.DynamicTextPaths);
            }
            SerializedProperty statusTexts = serialized.FindProperty(
                "statusBadgeTexts");
            AddReferencedArrayPaths<Text>(
                root,
                statusTexts,
                result.DynamicTextPaths);

            foreach (string field in DynamicImageFields)
            {
                AddReferencedPath<Image>(
                    root,
                    serialized.FindProperty(field),
                    result.DynamicImagePaths);
            }
            SerializedProperty statusImages = serialized.FindProperty(
                "statusBadgeImages");
            AddReferencedArrayPaths<Image>(
                root,
                statusImages,
                result.DynamicImagePaths);

            foreach (ItemDetailSectionView section in view
                         .GetComponentsInChildren<ItemDetailSectionView>(true))
            {
                SerializedObject sectionSerialized = new SerializedObject(
                    section);
                Text title = sectionSerialized.FindProperty("titleText")
                    ?.objectReferenceValue as Text;
                Image background = sectionSerialized.FindProperty(
                        "backgroundImage")
                    ?.objectReferenceValue as Image;
                Image accent = sectionSerialized.FindProperty("accentImage")
                    ?.objectReferenceValue as Image;

                foreach (Text text in section.GetComponentsInChildren<Text>(true))
                {
                    if (text != null && text != title)
                    {
                        result.DynamicTextPaths.Add(
                            RelativePath(root, text.transform));
                    }
                }
                foreach (Image image in section.GetComponentsInChildren<Image>(
                             true))
                {
                    if (image == null
                        || image == background
                        || image == accent
                        || image.name.IndexOf(
                            "Divider",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }
                    result.DynamicImagePaths.Add(
                        RelativePath(root, image.transform));
                }
            }
            return result;
        }

        private static ProjectionReset ClearSampleProjection(
            Transform root,
            ItemDetailPanelView view)
        {
            ProjectionReset result = CollectProjectionTargets(root, view);
            ILookup<string, Transform> transforms = root
                .GetComponentsInChildren<Transform>(true)
                .ToLookup(
                    value => RelativePath(root, value),
                    value => value,
                    StringComparer.Ordinal);
            foreach (string path in result.DynamicTextPaths)
            {
                foreach (Transform target in transforms[path])
                {
                    Text text = target.GetComponent<Text>();
                    if (text != null && !string.IsNullOrEmpty(text.text))
                    {
                        text.text = string.Empty;
                        result.ClearedTextCount++;
                        EditorUtility.SetDirty(text);
                    }
                }
            }
            foreach (string path in result.DynamicImagePaths)
            {
                foreach (Transform target in transforms[path])
                {
                    Image image = target.GetComponent<Image>();
                    if (image != null && image.sprite != null)
                    {
                        image.sprite = null;
                        result.ClearedSpriteCount++;
                        EditorUtility.SetDirty(image);
                    }
                }
            }
            return result;
        }

        private static void AddReferencedPath<T>(
            Transform root,
            SerializedProperty property,
            HashSet<string> paths)
            where T : Component
        {
            T value = property == null
                ? null
                : property.objectReferenceValue as T;
            if (value != null)
            {
                paths.Add(RelativePath(root, value.transform));
            }
        }

        private static void AddReferencedArrayPaths<T>(
            Transform root,
            SerializedProperty property,
            HashSet<string> paths)
            where T : Component
        {
            if (property == null || !property.isArray)
            {
                return;
            }
            for (int index = 0; index < property.arraySize; index++)
            {
                T value = property.GetArrayElementAtIndex(index)
                    .objectReferenceValue as T;
                if (value != null)
                {
                    paths.Add(RelativePath(root, value.transform));
                }
            }
        }

        private static ParitySnapshot CaptureSnapshot(
            Transform root,
            Transform panel,
            HashSet<string> dynamicTextPaths,
            HashSet<string> dynamicImagePaths)
        {
            List<Transform> retained = RetainedTransforms(root, panel);
            HashSet<Transform> retainedSet = new HashSet<Transform>(retained);
            RectTransform drivenRootRect = ValidateRootCanvasCarrier(root);
            List<Transform> panelTransforms = panel
                .GetComponentsInChildren<Transform>(true).ToList();
            ParitySnapshot result = new ParitySnapshot
            {
                PanelDirectChildren = panel.Cast<Transform>()
                    .Select(value => value.name).ToArray(),
                ChainGeometry = CaptureChainGeometry(root),
                RetainedObjectCount = retained.Count,
                PanelObjectCount = panelTransforms.Count,
                ForbiddenAdapterCount = retained
                    .SelectMany(value => value.GetComponents<MonoBehaviour>())
                    .Count(value => value != null
                        && IsForbiddenOwnerType(
                            value.GetType().FullName ?? string.Empty)),
                ExternalReferenceCount = CountExternalReferences(
                    root,
                    retained),
                ExternalPersistentListenerCount =
                    CountExternalPersistentListeners(root, retained)
            };

            result.PanelHierarchy = NormalizeLines(panelTransforms.Select(value =>
                string.Join("|", new[]
                {
                    RelativePath(panel, value),
                    value.gameObject.activeSelf ? "1" : "0",
                    value.gameObject.layer.ToString(CultureInfo.InvariantCulture),
                    value == panel
                        ? "0"
                        : value.GetSiblingIndex().ToString(
                            CultureInfo.InvariantCulture)
                })));

            List<Component> components = retained
                .SelectMany(value => value.GetComponents<Component>())
                .Where(value => value != null
                    && !IsForbiddenOwnerType(
                        value.GetType().FullName ?? string.Empty))
                .ToList();
            result.ComponentCount = components.Count;
            result.ComponentOrder = NormalizeLines(retained.SelectMany(value =>
                value.GetComponents<Component>()
                    .Where(component => component != null
                        && !IsForbiddenOwnerType(
                            component.GetType().FullName ?? string.Empty))
                    .Select((component, index) =>
                        RelativePath(root, value) + "|" + index + "|"
                        + component.GetType().FullName)));

            RectTransform[] rects = retained.OfType<RectTransform>()
                .Where(value => value != drivenRootRect)
                .ToArray();
            result.AllRectGeometry = rects.Select(value =>
                    CaptureRectGeometry(root, value))
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(RectSignature, StringComparer.Ordinal)
                .ToArray();
            Require(result.ChainGeometry.All(value => value.Path != "$"),
                "STANDALONE_ITEM_DETAIL_DRIVEN_ROOT_PRESENT_IN_CHAIN_GEOMETRY");
            Require(result.AllRectGeometry.All(value => value.Path != "$"),
                "STANDALONE_ITEM_DETAIL_DRIVEN_ROOT_PRESENT_IN_ALL_RECT_GEOMETRY");
            result.CanvasProperties = CaptureComponentProperties(
                root,
                retainedSet,
                components.Where(value => value is Canvas
                    || value is CanvasScaler
                    || value is GraphicRaycaster),
                null,
                null);

            Image[] images = retained.SelectMany(value =>
                    value.GetComponents<Image>())
                .ToArray();
            result.ImageCount = images.Length;
            result.StaticImageProperties = CaptureComponentProperties(
                root,
                retainedSet,
                images.Where(value => !dynamicImagePaths.Contains(
                    RelativePath(root, value.transform))),
                null,
                null);
            result.DynamicImageStyleProperties = CaptureComponentProperties(
                root,
                retainedSet,
                images.Where(value => dynamicImagePaths.Contains(
                    RelativePath(root, value.transform))),
                "m_Sprite",
                null);

            Text[] texts = retained.SelectMany(value => value.GetComponents<Text>())
                .ToArray();
            result.TextCount = texts.Length;
            result.StaticTextProperties = CaptureComponentProperties(
                root,
                retainedSet,
                texts.Where(value => !dynamicTextPaths.Contains(
                    RelativePath(root, value.transform))),
                null,
                null);
            result.DynamicTextStyleProperties = CaptureComponentProperties(
                root,
                retainedSet,
                texts.Where(value => dynamicTextPaths.Contains(
                    RelativePath(root, value.transform))),
                "m_Text",
                null);

            TMP_Text[] tmpTexts = retained.SelectMany(value =>
                    value.GetComponents<TMP_Text>())
                .ToArray();
            result.TmpTextCount = tmpTexts.Length;
            result.TmpTextProperties = CaptureComponentProperties(
                root,
                retainedSet,
                tmpTexts,
                null,
                null);

            ScrollRect[] scrollRects = retained.SelectMany(value =>
                    value.GetComponents<ScrollRect>())
                .ToArray();
            result.ScrollRectCount = scrollRects.Length;
            result.ScrollProperties = CaptureComponentProperties(
                root,
                retainedSet,
                scrollRects,
                null,
                null);

            Component[] masks = retained.SelectMany(value =>
                    value.GetComponents<Component>())
                .Where(value => value is Mask || value is RectMask2D)
                .ToArray();
            result.MaskCount = masks.Length;
            result.MaskProperties = CaptureComponentProperties(
                root,
                retainedSet,
                masks,
                null,
                null);

            Selectable[] selectables = retained.SelectMany(value =>
                    value.GetComponents<Selectable>())
                .ToArray();
            result.SelectableCount = selectables.Length;
            result.SelectableProperties = CaptureComponentProperties(
                root,
                retainedSet,
                selectables,
                null,
                "m_PersistentCalls");
            return result;
        }

        private static List<Transform> RetainedTransforms(
            Transform root,
            Transform panel)
        {
            HashSet<Transform> values = new HashSet<Transform>();
            Transform current = panel;
            while (current != null)
            {
                values.Add(current);
                if (current == root)
                {
                    break;
                }
                current = current.parent;
            }
            Require(values.Contains(root),
                "STANDALONE_ITEM_DETAIL_PANEL_OUTSIDE_ROOT");
            foreach (Transform child in panel.GetComponentsInChildren<Transform>(
                         true))
            {
                values.Add(child);
            }
            return values.OrderBy(value => RelativePath(root, value),
                    StringComparer.Ordinal)
                .ToList();
        }

        private static RectGeometrySnapshot[] CaptureChainGeometry(Transform root)
        {
            string[] paths =
            {
                "MobileSafeAreaRoot",
                "MobileSafeAreaRoot/SafeAreaRoot",
                "MobileSafeAreaRoot/SafeAreaRoot/PopupLayer",
                RetainedPath
            };
            return paths.Select(path =>
            {
                Transform value = RequiredPath(root, path);
                return CaptureRectGeometry(root, value as RectTransform);
            }).ToArray();
        }

        private static RectTransform ValidateRootCanvasCarrier(Transform root)
        {
            Require(root != null && root.parent == null,
                "STANDALONE_ITEM_DETAIL_ROOT_CANVAS_NOT_SCENE_OR_PREFAB_ROOT");
            RectTransform[] rects = root.GetComponents<RectTransform>();
            Canvas[] canvases = root.GetComponents<Canvas>();
            CanvasScaler[] scalers = root.GetComponents<CanvasScaler>();
            GraphicRaycaster[] raycasters = root.GetComponents<
                GraphicRaycaster>();
            Require(rects.Length == 1
                    && canvases.Length == 1
                    && scalers.Length == 1
                    && raycasters.Length == 1,
                "STANDALONE_ITEM_DETAIL_ROOT_CANVAS_CONTRACT_INVALID"
                + " rectTransform=" + rects.Length
                + " canvas=" + canvases.Length
                + " canvasScaler=" + scalers.Length
                + " graphicRaycaster=" + raycasters.Length);
            Require(canvases[0].renderMode == RenderMode.ScreenSpaceOverlay,
                "STANDALONE_ITEM_DETAIL_ROOT_CANVAS_RENDER_MODE_INVALID "
                + canvases[0].renderMode);
            return rects[0];
        }

        private static RectGeometrySnapshot CaptureRectGeometry(
            Transform root,
            RectTransform rect)
        {
            Require(rect != null,
                "STANDALONE_ITEM_DETAIL_RECT_TRANSFORM_MISSING");
            return new RectGeometrySnapshot
            {
                Path = RelativePath(root, rect),
                AnchorMin = rect.anchorMin,
                AnchorMax = rect.anchorMax,
                Pivot = rect.pivot,
                AnchoredPosition = rect.anchoredPosition,
                SizeDelta = rect.sizeDelta,
                LocalScale = rect.localScale,
                LocalRotation = rect.localRotation
            };
        }

        private static string RectSignature(RectGeometrySnapshot geometry)
        {
            Quaternion rotation = CanonicalQuaternion(geometry.LocalRotation);
            return string.Join("|", new[]
            {
                geometry.Path,
                QuantizedVector(geometry.AnchorMin, RectSignatureQuantum),
                QuantizedVector(geometry.AnchorMax, RectSignatureQuantum),
                QuantizedVector(geometry.Pivot, RectSignatureQuantum),
                QuantizedVector(geometry.AnchoredPosition, RectSignatureQuantum),
                QuantizedVector(geometry.SizeDelta, RectSignatureQuantum),
                QuantizedVector(geometry.LocalScale, RectSignatureQuantum),
                QuantizedQuaternion(rotation, RectRotationSignatureQuantum)
            });
        }

        private static string QuantizedVector(Vector2 value, float quantum)
        {
            return Quantized(value.x, quantum)
                   + ","
                   + Quantized(value.y, quantum);
        }

        private static string QuantizedVector(Vector3 value, float quantum)
        {
            return Quantized(value.x, quantum)
                   + ","
                   + Quantized(value.y, quantum)
                   + ","
                   + Quantized(value.z, quantum);
        }

        private static string QuantizedQuaternion(
            Quaternion value,
            float quantum)
        {
            return Quantized(value.x, quantum)
                   + ","
                   + Quantized(value.y, quantum)
                   + ","
                   + Quantized(value.z, quantum)
                   + ","
                   + Quantized(value.w, quantum);
        }

        private static string Quantized(float value, float quantum)
        {
            long bucket = (long)Math.Round(
                value / quantum,
                MidpointRounding.AwayFromZero);
            return bucket.ToString(CultureInfo.InvariantCulture);
        }

        private static Quaternion CanonicalQuaternion(Quaternion value)
        {
            Quaternion normalized = value.normalized;
            bool negate = normalized.w < 0f
                          || (normalized.w == 0f && normalized.z < 0f)
                          || (normalized.w == 0f
                              && normalized.z == 0f
                              && normalized.y < 0f)
                          || (normalized.w == 0f
                              && normalized.z == 0f
                              && normalized.y == 0f
                              && normalized.x < 0f);
            return negate
                ? new Quaternion(
                    -normalized.x,
                    -normalized.y,
                    -normalized.z,
                    -normalized.w)
                : normalized;
        }

        private static string[] CaptureComponentProperties(
            Transform root,
            HashSet<Transform> retained,
            IEnumerable<Component> components,
            string skippedExactProperty,
            string skippedPropertyToken)
        {
            List<string> lines = new List<string>();
            foreach (Component component in components.OrderBy(
                         value => RelativePath(root, value.transform),
                         StringComparer.Ordinal).ThenBy(
                         value => value.GetType().FullName,
                         StringComparer.Ordinal))
            {
                string componentPath = RelativePath(root, component.transform)
                                       + "|" + component.GetType().FullName;
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (ShouldSkipSignatureProperty(iterator.propertyPath)
                        || (!string.IsNullOrEmpty(skippedExactProperty)
                            && string.Equals(
                                iterator.propertyPath,
                                skippedExactProperty,
                                StringComparison.Ordinal))
                        || (!string.IsNullOrEmpty(skippedPropertyToken)
                            && iterator.propertyPath.IndexOf(
                                skippedPropertyToken,
                                StringComparison.Ordinal) >= 0))
                    {
                        continue;
                    }
                    if (iterator.propertyType
                        == SerializedPropertyType.ObjectReference)
                    {
                        Object reference = iterator.objectReferenceValue;
                        if (reference != null
                            && !EditorUtility.IsPersistent(reference)
                            && !IsRetainedReference(retained, reference))
                        {
                            continue;
                        }
                        if (reference == null)
                        {
                            continue;
                        }
                    }
                    lines.Add(componentPath + "|" + iterator.propertyPath
                              + "|" + SerializedValue(root, iterator));
                }
            }
            return NormalizeLines(lines);
        }

        private static bool ShouldSkipSignatureProperty(string propertyPath)
        {
            return string.Equals(propertyPath, "m_Script", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_GameObject", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Father", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Children", StringComparison.Ordinal)
                   || propertyPath.StartsWith(
                       "m_Children.Array",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_CorrespondingSourceObject",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_PrefabInstance",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_PrefabAsset",
                       StringComparison.Ordinal);
        }

        private static string SerializedValue(
            Transform root,
            SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "1" : "0";
                case SerializedPropertyType.Float:
                    return property.doubleValue.ToString(
                        "R",
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return property.stringValue ?? string.Empty;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString("R");
                case SerializedPropertyType.ObjectReference:
                    return StableReference(root, property.objectReferenceValue);
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("R");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("R");
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString("R");
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString("R");
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString("R");
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename ?? string.Empty;
                default:
                    return property.propertyType.ToString();
            }
        }

        private static string StableReference(Transform root, Object value)
        {
            if (value == null)
            {
                return "null";
            }
            if (EditorUtility.IsPersistent(value))
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        value,
                        out string guid,
                        out long localId))
                {
                    return "asset:" + guid + ":"
                           + localId.ToString(CultureInfo.InvariantCulture);
                }
                return "persistent:" + value.GetType().FullName + ":"
                       + value.name;
            }
            Transform reference = ReferencedTransform(value);
            return reference != null && IsInternalTransform(root, reference)
                ? "internal:" + RelativePath(root, reference) + ":"
                  + value.GetType().FullName
                : "external-scene:" + value.GetType().FullName;
        }

        private static void ValidateOutput(
            GameObject root,
            Transform panel,
            string[] expectedPanelChildren,
            ProjectionReset projection)
        {
            Require(string.Equals(
                    root.name,
                    OutputRootName,
                    StringComparison.Ordinal),
                "STANDALONE_ITEM_DETAIL_ROOT_NAME_INVALID");
            Transform mobile = RequiredDirectChild(
                root.transform,
                "MobileSafeAreaRoot");
            Transform safe = RequiredDirectChild(mobile, "SafeAreaRoot");
            Transform popup = RequiredDirectChild(safe, "PopupLayer");
            Require(root.transform.childCount == 1
                    && mobile.childCount == 1
                    && safe.childCount == 1
                    && popup.childCount == 1
                    && popup.GetChild(0) == panel,
                "STANDALONE_ITEM_DETAIL_MINIMUM_CHAIN_INVALID");
            Require(panel.Cast<Transform>().Select(value => value.name)
                    .SequenceEqual(expectedPanelChildren),
                "STANDALONE_ITEM_DETAIL_PANEL_CHILD_ORDER_CHANGED");
            ValidateRootCanvasCarrier(root.transform);

            Image popupImage = RequiredSingleComponent<Image>(
                popup.gameObject);
            Require(!popupImage.raycastTarget
                    && popup.GetComponents<Canvas>().Length == 1,
                "STANDALONE_ITEM_DETAIL_POPUP_LAYER_BEHAVIOR_INVALID");
            Require(root.GetComponentsInChildren<ItemDetailPanelView>(true)
                        .Length == 1
                    && root.GetComponentsInChildren<
                        ItemSandboxDetailPanelView>(true).Length == 0,
                "STANDALONE_ITEM_DETAIL_VIEW_CARRIER_INVALID");
            Require(root.GetComponentsInChildren<Transform>(true).All(value =>
                    !string.Equals(
                        value.name,
                        "EnemyCombatFeedbackFloatingRoot",
                        StringComparison.Ordinal)
                    && !string.Equals(
                        value.name,
                        "BuildSandboxItemInfoPanel_Runtime",
                        StringComparison.Ordinal)),
                "STANDALONE_ITEM_DETAIL_FORBIDDEN_SIBLING_PRESENT");
            ValidateAllowedPresentationBehaviours(root);
            ValidateNoMissingScripts(root);
            ValidateNoExternalSceneReferences(root);
            Require(CountExternalPersistentListeners(
                        root.transform,
                        root.GetComponentsInChildren<Transform>(true).ToList())
                    == 0,
                "STANDALONE_ITEM_DETAIL_EXTERNAL_LISTENER_PRESENT");
            Require(!AssetDatabase.GetDependencies(OutputPrefabPath, true).Any(
                    value => value.EndsWith(
                        ".unity",
                        StringComparison.OrdinalIgnoreCase)),
                "STANDALONE_ITEM_DETAIL_SCENE_DEPENDENCY_PRESENT");

            ILookup<string, Transform> transforms = root
                .GetComponentsInChildren<Transform>(true)
                .ToLookup(
                    value => RelativePath(root.transform, value),
                    value => value,
                    StringComparer.Ordinal);
            foreach (string path in projection.DynamicTextPaths)
            {
                Transform[] targets = transforms[path].ToArray();
                Require(targets.Length > 0
                        && targets.All(target =>
                            target.GetComponent<Text>() != null
                            && string.IsNullOrEmpty(
                                target.GetComponent<Text>().text)),
                    "STANDALONE_ITEM_DETAIL_SAMPLE_TEXT_NOT_CLEARED " + path);
            }
            foreach (string path in projection.DynamicImagePaths)
            {
                Transform[] targets = transforms[path].ToArray();
                Require(targets.Length > 0
                        && targets.All(target =>
                            target.GetComponent<Image>() != null
                            && target.GetComponent<Image>().sprite == null),
                    "STANDALONE_ITEM_DETAIL_SAMPLE_IMAGE_NOT_CLEARED " + path);
            }
        }

        private static void ValidateAllowedPresentationBehaviours(
            GameObject root)
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<
                         MonoBehaviour>(true))
            {
                Require(behaviour != null,
                    "STANDALONE_ITEM_DETAIL_MISSING_BEHAVIOUR");
                Type type = behaviour.GetType();
                string fullName = type.FullName ?? string.Empty;
                string typeNamespace = type.Namespace ?? string.Empty;
                bool allowed = typeNamespace.StartsWith(
                                   "UnityEngine",
                                   StringComparison.Ordinal)
                               || typeNamespace.StartsWith(
                                   "TMPro",
                                   StringComparison.Ordinal)
                               || typeNamespace.StartsWith(
                                   "TalismanBag.Items.Detail.UI",
                                   StringComparison.Ordinal)
                               || string.Equals(
                                   fullName,
                                   typeof(MobileSafeAreaFitter).FullName,
                                   StringComparison.Ordinal);
                Require(allowed && !IsForbiddenOwnerType(fullName),
                    "STANDALONE_ITEM_DETAIL_RUNTIME_OWNER_FORBIDDEN "
                    + fullName);
            }
        }

        private static void ValidateParity(
            ParitySnapshot source,
            ParitySnapshot output)
        {
            Require(source.PanelDirectChildren.SequenceEqual(
                    output.PanelDirectChildren),
                "STANDALONE_ITEM_DETAIL_PARITY_CHILD_ORDER_FAIL");
            ValidateRectGeometrySet(
                "chain",
                source.ChainGeometry,
                output.ChainGeometry);
            Require(source.PanelObjectCount == output.PanelObjectCount
                    && source.RetainedObjectCount == output.RetainedObjectCount,
                "STANDALONE_ITEM_DETAIL_PARITY_OBJECT_COUNT_FAIL");
            Require(source.PanelHierarchy.SequenceEqual(output.PanelHierarchy),
                "STANDALONE_ITEM_DETAIL_PARITY_HIERARCHY_FAIL");
            Require(source.ComponentCount == output.ComponentCount
                    && source.ComponentOrder.SequenceEqual(
                        output.ComponentOrder),
                "STANDALONE_ITEM_DETAIL_PARITY_COMPONENT_ORDER_FAIL");
            ValidateRectGeometrySet(
                "retained",
                source.AllRectGeometry,
                output.AllRectGeometry);
            Require(source.CanvasProperties.SequenceEqual(
                    output.CanvasProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_CANVAS_FAIL");
            Require(source.ImageCount == output.ImageCount
                    && source.StaticImageProperties.SequenceEqual(
                        output.StaticImageProperties)
                    && source.DynamicImageStyleProperties.SequenceEqual(
                        output.DynamicImageStyleProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_IMAGE_FAIL");
            Require(source.TextCount == output.TextCount
                    && source.StaticTextProperties.SequenceEqual(
                        output.StaticTextProperties)
                    && source.DynamicTextStyleProperties.SequenceEqual(
                        output.DynamicTextStyleProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_TEXT_FAIL");
            Require(source.TmpTextCount == output.TmpTextCount
                    && source.TmpTextProperties.SequenceEqual(
                        output.TmpTextProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_TMP_FAIL");
            Require(source.ScrollRectCount == output.ScrollRectCount
                    && source.ScrollProperties.SequenceEqual(
                        output.ScrollProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_SCROLL_FAIL");
            Require(source.MaskCount == output.MaskCount
                    && source.MaskProperties.SequenceEqual(
                        output.MaskProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_MASK_FAIL");
            Require(source.SelectableCount == output.SelectableCount
                    && source.SelectableProperties.SequenceEqual(
                        output.SelectableProperties),
                "STANDALONE_ITEM_DETAIL_PARITY_SELECTABLE_FAIL");
            Require(source.ForbiddenAdapterCount > 0
                    && output.ForbiddenAdapterCount == 0
                    && output.ExternalReferenceCount == 0
                    && output.ExternalPersistentListenerCount == 0,
                "STANDALONE_ITEM_DETAIL_PARITY_EXCLUSION_FAIL");
        }

        private static void ValidateRectGeometrySet(
            string scope,
            RectGeometrySnapshot[] source,
            RectGeometrySnapshot[] output)
        {
            Require(source.Length == output.Length,
                "STANDALONE_ITEM_DETAIL_PARITY_RECT_VALUE_FAIL"
                + " scope=" + scope
                + " path=<set> field=pathCount"
                + " source=" + source.Length
                + " output=" + output.Length
                + " delta=" + Math.Abs(source.Length - output.Length));

            for (int index = 0; index < source.Length; index++)
            {
                RectGeometrySnapshot sourceRect = source[index];
                RectGeometrySnapshot outputRect = output[index];
                Require(string.Equals(
                        sourceRect.Path,
                        outputRect.Path,
                        StringComparison.Ordinal),
                    "STANDALONE_ITEM_DETAIL_PARITY_RECT_VALUE_FAIL"
                    + " scope=" + scope
                    + " pathIndex=" + index
                    + " field=path"
                    + " source=" + sourceRect.Path
                    + " output=" + outputRect.Path
                    + " delta=n/a");

                string path = sourceRect.Path;
                ValidateRectFloat(scope, path, "anchorMin.x",
                    sourceRect.AnchorMin.x, outputRect.AnchorMin.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "anchorMin.y",
                    sourceRect.AnchorMin.y, outputRect.AnchorMin.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "anchorMax.x",
                    sourceRect.AnchorMax.x, outputRect.AnchorMax.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "anchorMax.y",
                    sourceRect.AnchorMax.y, outputRect.AnchorMax.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "pivot.x",
                    sourceRect.Pivot.x, outputRect.Pivot.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "pivot.y",
                    sourceRect.Pivot.y, outputRect.Pivot.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "anchoredPosition.x",
                    sourceRect.AnchoredPosition.x,
                    outputRect.AnchoredPosition.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "anchoredPosition.y",
                    sourceRect.AnchoredPosition.y,
                    outputRect.AnchoredPosition.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "sizeDelta.x",
                    sourceRect.SizeDelta.x, outputRect.SizeDelta.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "sizeDelta.y",
                    sourceRect.SizeDelta.y, outputRect.SizeDelta.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "localScale.x",
                    sourceRect.LocalScale.x, outputRect.LocalScale.x,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "localScale.y",
                    sourceRect.LocalScale.y, outputRect.LocalScale.y,
                    RectValueTolerance);
                ValidateRectFloat(scope, path, "localScale.z",
                    sourceRect.LocalScale.z, outputRect.LocalScale.z,
                    RectValueTolerance);

                float angleDelta = Quaternion.Angle(
                    sourceRect.LocalRotation,
                    outputRect.LocalRotation);
                Require(angleDelta <= RectRotationToleranceDegrees,
                    "STANDALONE_ITEM_DETAIL_PARITY_RECT_VALUE_FAIL"
                    + " scope=" + scope
                    + " path=" + path
                    + " field=localRotation"
                    + " source=" + RawQuaternion(sourceRect.LocalRotation)
                    + " output=" + RawQuaternion(outputRect.LocalRotation)
                    + " deltaDegrees=" + RawFloat(angleDelta)
                    + " toleranceDegrees="
                    + RawFloat(RectRotationToleranceDegrees));
            }
        }

        private static void ValidateRectFloat(
            string scope,
            string path,
            string field,
            float source,
            float output,
            float tolerance)
        {
            float delta = Mathf.Abs(source - output);
            Require(delta <= tolerance,
                "STANDALONE_ITEM_DETAIL_PARITY_RECT_VALUE_FAIL"
                + " scope=" + scope
                + " path=" + path
                + " field=" + field
                + " source=" + RawFloat(source)
                + " output=" + RawFloat(output)
                + " delta=" + RawFloat(delta)
                + " tolerance=" + RawFloat(tolerance));
        }

        private static string RawQuaternion(Quaternion value)
        {
            return "("
                   + RawFloat(value.x) + ","
                   + RawFloat(value.y) + ","
                   + RawFloat(value.z) + ","
                   + RawFloat(value.w) + ")";
        }

        private static string RawFloat(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static int CountExternalReferences(
            Transform root,
            IEnumerable<Transform> transforms)
        {
            int count = 0;
            foreach (Component component in transforms.SelectMany(value =>
                         value.GetComponents<Component>()))
            {
                if (component == null)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (iterator.propertyType
                        != SerializedPropertyType.ObjectReference
                        || ShouldSkipObjectReference(iterator.propertyPath))
                    {
                        continue;
                    }
                    Object value = iterator.objectReferenceValue;
                    if (value != null
                        && !EditorUtility.IsPersistent(value)
                        && !IsInternalReference(root, value))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private static int CountExternalPersistentListeners(
            Transform root,
            IEnumerable<Transform> transforms)
        {
            int count = 0;
            foreach (Component component in transforms.SelectMany(value =>
                         value.GetComponents<Component>()))
            {
                if (component == null)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (!iterator.isArray
                        || !iterator.propertyPath.EndsWith(
                            "m_PersistentCalls.m_Calls",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }
                    for (int index = 0; index < iterator.arraySize; index++)
                    {
                        SerializedProperty target = iterator
                            .GetArrayElementAtIndex(index)
                            .FindPropertyRelative("m_Target");
                        Object value = target == null
                            ? null
                            : target.objectReferenceValue;
                        if (value == null
                            || !IsInternalReference(root, value))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private static void ValidateNoExternalSceneReferences(GameObject root)
        {
            Require(CountExternalReferences(
                        root.transform,
                        root.GetComponentsInChildren<Transform>(true)) == 0,
                "STANDALONE_ITEM_DETAIL_EXTERNAL_SCENE_REFERENCE_PRESENT");
        }

        private static bool ShouldSkipObjectReference(string propertyPath)
        {
            return string.Equals(propertyPath, "m_Script", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_GameObject", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Father", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Children", StringComparison.Ordinal)
                   || propertyPath.StartsWith(
                       "m_Children.Array",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_CorrespondingSourceObject",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_PrefabInstance",
                       StringComparison.Ordinal)
                   || string.Equals(
                       propertyPath,
                       "m_PrefabAsset",
                       StringComparison.Ordinal);
        }

        private static bool IsInternalReference(Transform root, Object value)
        {
            Transform reference = ReferencedTransform(value);
            return reference != null && IsInternalTransform(root, reference);
        }

        private static bool IsRetainedReference(
            HashSet<Transform> retained,
            Object value)
        {
            Transform reference = ReferencedTransform(value);
            return reference != null && retained.Contains(reference);
        }

        private static Transform ReferencedTransform(Object value)
        {
            if (value is GameObject gameObject)
            {
                return gameObject.transform;
            }
            if (value is Component component)
            {
                return component.transform;
            }
            return null;
        }

        private static bool IsInternalTransform(
            Transform root,
            Transform value)
        {
            return value == root || value.IsChildOf(root);
        }

        private static string RelativePath(Transform root, Transform target)
        {
            if (target == root)
            {
                return "$";
            }
            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }
            Require(current == root,
                "STANDALONE_ITEM_DETAIL_REFERENCE_OUTSIDE_ROOT");
            return "$/" + string.Join("/", names);
        }

        private static T RequiredSingleComponent<T>(GameObject target)
            where T : Component
        {
            T[] matches = target.GetComponents<T>();
            Require(matches.Length == 1,
                "STANDALONE_ITEM_DETAIL_COMPONENT_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Length);
            return matches[0];
        }

        private static void ValidateNoMissingScripts(GameObject root)
        {
            int missing = root.GetComponentsInChildren<Transform>(true)
                .Sum(value => GameObjectUtility
                    .GetMonoBehavioursWithMissingScriptCount(value.gameObject));
            Require(missing == 0,
                "STANDALONE_ITEM_DETAIL_MISSING_SCRIPT " + missing);
        }

        private static string[] NormalizeLines(IEnumerable<string> lines)
        {
            return lines.OrderBy(line => line, StringComparer.Ordinal).ToArray();
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
