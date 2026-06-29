#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools
{
    [InitializeOnLoad]
    public static class UiHandTuneSnapshotTool
    {
        private const string MenuRoot = "Tools/Talisman Bag/UI Hand Tune/";
        private const string SnapshotSessionKey = "TalismanBag.UiHandTuneSnapshotTool.PendingSnapshot";
        private const string PendingSessionKey = "TalismanBag.UiHandTuneSnapshotTool.HasPendingSnapshot";
        private const string UndoName = "Apply Play UI Snapshot";

        static UiHandTuneSnapshotTool()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall -= ApplyPendingSnapshotIfNeeded;
                EditorApplication.delayCall += ApplyPendingSnapshotIfNeeded;
            }
        }

        [MenuItem(MenuRoot + "Save Selected Play UI To Scene", false, 100)]
        public static void SaveSelectedPlayUiToScene()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog(
                    "Save Play UI To Scene",
                    "Enter Play Mode, adjust UI, select one or more UI roots in the active scene, then run this tool.",
                    "OK");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                EditorUtility.DisplayDialog(
                    "Save Play UI To Scene",
                    "The active scene is not a saved scene asset. Snapshot apply was cancelled.",
                    "OK");
                return;
            }

            List<GameObject> selectedRoots = GetSelectedSceneRoots(scene);
            if (selectedRoots.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Save Play UI To Scene",
                    "Select one or more UI roots from the active scene. Project assets, objects from other scenes, and non-UI roots are ignored.",
                    "OK");
                return;
            }

            SnapshotData snapshot = CaptureSnapshot(scene, selectedRoots);
            if (snapshot.NodeCount <= 0)
            {
                EditorUtility.DisplayDialog(
                    "Save Play UI To Scene",
                    "No UI nodes were captured from the selected roots.",
                    "OK");
                return;
            }

            string rootSummary = string.Join("\n", selectedRoots.Select(root => "- " + GetSceneHierarchyPath(root.transform)));
            bool confirmed = EditorUtility.DisplayDialog(
                "Save Play UI To Scene",
                "This will exit Play Mode and save whitelisted UI fields back to the active scene file.\n\n" +
                "Scene:\n" + scene.path + "\n\n" +
                "Selected root whitelist:\n" + rootSummary + "\n\n" +
                "Included: activeSelf, RectTransform, sibling order, Image, Outline, Text, TMP_Text.\n" +
                "Excluded: Button onClick, delegates, script fields, prefab links, save data, Build Settings, and objects outside the selected roots.",
                "Save And Exit Play",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            string json = JsonUtility.ToJson(snapshot);
            SessionState.SetString(SnapshotSessionKey, json);
            SessionState.SetBool(PendingSessionKey, true);

            Debug.Log(
                "[UIHandTuneSnapshot] SNAPSHOT_CAPTURED " +
                $"scene={scene.path} roots={snapshot.roots.Count} nodes={snapshot.NodeCount}. Exiting Play Mode to apply.");

            EditorApplication.ExitPlaymode();
        }

        [MenuItem(MenuRoot + "Clear Pending Play UI Snapshot", false, 101)]
        public static void ClearPendingSnapshot()
        {
            ClearPendingSession();
            Debug.Log("[UIHandTuneSnapshot] Pending snapshot cleared.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.delayCall -= ApplyPendingSnapshotIfNeeded;
            EditorApplication.delayCall += ApplyPendingSnapshotIfNeeded;
        }

        private static void ApplyPendingSnapshotIfNeeded()
        {
            if (!SessionState.GetBool(PendingSessionKey, false))
            {
                return;
            }

            string json = SessionState.GetString(SnapshotSessionKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                ClearPendingSession();
                Debug.LogWarning("[UIHandTuneSnapshot] Pending snapshot was empty and has been cleared.");
                return;
            }

            SnapshotData snapshot = JsonUtility.FromJson<SnapshotData>(json);
            if (snapshot == null || string.IsNullOrEmpty(snapshot.scenePath))
            {
                ClearPendingSession();
                Debug.LogWarning("[UIHandTuneSnapshot] Pending snapshot was invalid and has been cleared.");
                return;
            }

            Scene scene = FindLoadedScene(snapshot.scenePath);
            if (!scene.IsValid())
            {
                ClearPendingSession();
                Debug.LogError(
                    "[UIHandTuneSnapshot] Snapshot scene is not loaded after Play Mode exit. " +
                    $"scene={snapshot.scenePath}. Snapshot was cleared without applying.");
                return;
            }

            int appliedNodes = 0;
            int skippedNodes = 0;
            int missingComponents = 0;

            foreach (RootSnapshot rootSnapshot in snapshot.roots)
            {
                Transform root = FindTransformByScenePath(scene, rootSnapshot.rootPath);
                if (root == null)
                {
                    skippedNodes += rootSnapshot.nodes.Count;
                    Debug.LogWarning(
                        "[UIHandTuneSnapshot] Root not found in edit scene. " +
                        $"root={rootSnapshot.rootPath}");
                    continue;
                }

                foreach (NodeSnapshot node in rootSnapshot.nodes)
                {
                    Transform target = string.IsNullOrEmpty(node.relativePath)
                        ? root
                        : root.Find(node.relativePath);

                    if (target == null)
                    {
                        skippedNodes++;
                        Debug.LogWarning(
                            "[UIHandTuneSnapshot] Node not found in edit scene. " +
                            $"root={rootSnapshot.rootPath} relativePath={node.relativePath}");
                        continue;
                    }

                    ApplyNodeSnapshot(node, target, ref missingComponents);
                    appliedNodes++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            ClearPendingSession();

            Debug.Log(
                "[UIHandTuneSnapshot] SNAPSHOT_APPLIED " +
                $"scene={scene.path} roots={snapshot.roots.Count} appliedNodes={appliedNodes} " +
                $"skippedNodes={skippedNodes} missingComponents={missingComponents} saved={saved}");
        }

        private static void ClearPendingSession()
        {
            SessionState.EraseString(SnapshotSessionKey);
            SessionState.EraseBool(PendingSessionKey);
        }

        private static List<GameObject> GetSelectedSceneRoots(Scene activeScene)
        {
            GameObject[] selected = Selection.gameObjects ?? Array.Empty<GameObject>();
            List<GameObject> candidates = new();
            foreach (GameObject gameObject in selected)
            {
                if (gameObject == null ||
                    EditorUtility.IsPersistent(gameObject) ||
                    gameObject.scene != activeScene ||
                    !IsUiRootCandidate(gameObject))
                {
                    continue;
                }

                candidates.Add(gameObject);
            }

            candidates = candidates
                .Distinct()
                .OrderBy(gameObject => GetSceneHierarchyPath(gameObject.transform), StringComparer.Ordinal)
                .ToList();

            List<GameObject> roots = new();
            foreach (GameObject candidate in candidates)
            {
                bool isChildOfSelectedRoot = roots.Any(root => candidate.transform.IsChildOf(root.transform));
                if (!isChildOfSelectedRoot)
                {
                    roots.Add(candidate);
                }
            }

            return roots;
        }

        private static bool IsUiRootCandidate(GameObject gameObject)
        {
            return gameObject.GetComponent<RectTransform>() != null ||
                   gameObject.GetComponentInChildren<RectTransform>(true) != null ||
                   gameObject.GetComponent<Canvas>() != null ||
                   gameObject.GetComponentInChildren<Canvas>(true) != null;
        }

        private static SnapshotData CaptureSnapshot(Scene scene, List<GameObject> selectedRoots)
        {
            SnapshotData snapshot = new()
            {
                scenePath = scene.path,
                capturedAtUtc = DateTime.UtcNow.ToString("O")
            };

            foreach (GameObject selectedRoot in selectedRoots)
            {
                Transform root = selectedRoot.transform;
                RootSnapshot rootSnapshot = new()
                {
                    rootPath = GetSceneHierarchyPath(root)
                };

                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform transform in transforms)
                {
                    rootSnapshot.nodes.Add(CaptureNodeSnapshot(root, transform));
                }

                snapshot.roots.Add(rootSnapshot);
            }

            return snapshot;
        }

        private static NodeSnapshot CaptureNodeSnapshot(Transform root, Transform target)
        {
            NodeSnapshot snapshot = new()
            {
                relativePath = GetRelativePath(root, target),
                activeSelf = target.gameObject.activeSelf,
                siblingIndex = target.GetSiblingIndex()
            };

            RectTransform rectTransform = target as RectTransform;
            if (rectTransform != null)
            {
                snapshot.hasRectTransform = true;
                snapshot.rectTransform = RectTransformSnapshot.Capture(rectTransform);
            }

            Image image = target.GetComponent<Image>();
            if (image != null)
            {
                snapshot.hasImage = true;
                snapshot.image = ImageSnapshot.Capture(image);
            }

            Outline outline = target.GetComponent<Outline>();
            if (outline != null)
            {
                snapshot.hasOutline = true;
                snapshot.outline = OutlineSnapshot.Capture(outline);
            }

            Text text = target.GetComponent<Text>();
            if (text != null)
            {
                snapshot.hasText = true;
                snapshot.text = TextSnapshot.Capture(text);
            }

            Component tmpText = FindTmpText(target.gameObject);
            if (tmpText != null)
            {
                snapshot.hasTmpText = true;
                snapshot.tmpText = TmpTextSnapshot.Capture(tmpText);
            }

            return snapshot;
        }

        private static void ApplyNodeSnapshot(NodeSnapshot snapshot, Transform target, ref int missingComponents)
        {
            Undo.RecordObject(target, UndoName);
            Undo.RecordObject(target.gameObject, UndoName);
            target.SetSiblingIndex(GetClampedSiblingIndex(target, snapshot.siblingIndex));
            target.gameObject.SetActive(snapshot.activeSelf);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target.gameObject);

            RectTransform rectTransform = target as RectTransform;
            if (snapshot.hasRectTransform)
            {
                if (rectTransform != null)
                {
                    Undo.RecordObject(rectTransform, UndoName);
                    snapshot.rectTransform.Apply(rectTransform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(rectTransform);
                }
                else
                {
                    missingComponents++;
                }
            }

            if (snapshot.hasImage)
            {
                Image image = target.GetComponent<Image>();
                if (image != null)
                {
                    Undo.RecordObject(image, UndoName);
                    snapshot.image.Apply(image);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(image);
                }
                else
                {
                    missingComponents++;
                }
            }

            if (snapshot.hasOutline)
            {
                Outline outline = target.GetComponent<Outline>();
                if (outline != null)
                {
                    Undo.RecordObject(outline, UndoName);
                    snapshot.outline.Apply(outline);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(outline);
                }
                else
                {
                    missingComponents++;
                }
            }

            if (snapshot.hasText)
            {
                Text text = target.GetComponent<Text>();
                if (text != null)
                {
                    Undo.RecordObject(text, UndoName);
                    snapshot.text.Apply(text);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(text);
                }
                else
                {
                    missingComponents++;
                }
            }

            if (snapshot.hasTmpText)
            {
                Component tmpText = FindTmpText(target.gameObject);
                if (tmpText != null)
                {
                    Undo.RecordObject(tmpText, UndoName);
                    snapshot.tmpText.Apply(tmpText);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(tmpText);
                }
                else
                {
                    missingComponents++;
                }
            }
        }

        private static int GetClampedSiblingIndex(Transform target, int requestedIndex)
        {
            int maxSiblingIndex = target.parent != null
                ? target.parent.childCount - 1
                : target.gameObject.scene.rootCount - 1;

            return Mathf.Clamp(requestedIndex, 0, Mathf.Max(0, maxSiblingIndex));
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == target)
            {
                return string.Empty;
            }

            Stack<string> names = new();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static string GetSceneHierarchyPath(Transform target)
        {
            Stack<string> names = new();
            Transform current = target;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static Transform FindTransformByScenePath(Scene scene, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] parts = path.Split('/');
            if (parts.Length == 0)
            {
                return null;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                if (!string.Equals(rootObject.name, parts[0], StringComparison.Ordinal))
                {
                    continue;
                }

                Transform current = rootObject.transform;
                for (int i = 1; i < parts.Length; i++)
                {
                    current = current.Find(parts[i]);
                    if (current == null)
                    {
                        break;
                    }
                }

                if (current != null)
                {
                    return current;
                }
            }

            return null;
        }

        private static Scene FindLoadedScene(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return scene;
                }
            }

            return default;
        }

        private static Component FindTmpText(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null && component.GetType().FullName == "TMPro.TMP_Text")
                {
                    return component;
                }
            }

            return null;
        }

        [Serializable]
        private sealed class SnapshotData
        {
            public string scenePath;
            public string capturedAtUtc;
            public List<RootSnapshot> roots = new();

            public int NodeCount => roots.Sum(root => root.nodes.Count);
        }

        [Serializable]
        private sealed class RootSnapshot
        {
            public string rootPath;
            public List<NodeSnapshot> nodes = new();
        }

        [Serializable]
        private sealed class NodeSnapshot
        {
            public string relativePath;
            public bool activeSelf;
            public int siblingIndex;
            public bool hasRectTransform;
            public RectTransformSnapshot rectTransform;
            public bool hasImage;
            public ImageSnapshot image;
            public bool hasOutline;
            public OutlineSnapshot outline;
            public bool hasText;
            public TextSnapshot text;
            public bool hasTmpText;
            public TmpTextSnapshot tmpText;
        }

        [Serializable]
        private struct RectTransformSnapshot
        {
            public Vector2Data anchorMin;
            public Vector2Data anchorMax;
            public Vector2Data anchoredPosition;
            public Vector2Data sizeDelta;
            public Vector2Data pivot;
            public Vector3Data localPosition;
            public QuaternionData localRotation;
            public Vector3Data localScale;

            public static RectTransformSnapshot Capture(RectTransform rectTransform)
            {
                return new RectTransformSnapshot
                {
                    anchorMin = new Vector2Data(rectTransform.anchorMin),
                    anchorMax = new Vector2Data(rectTransform.anchorMax),
                    anchoredPosition = new Vector2Data(rectTransform.anchoredPosition),
                    sizeDelta = new Vector2Data(rectTransform.sizeDelta),
                    pivot = new Vector2Data(rectTransform.pivot),
                    localPosition = new Vector3Data(rectTransform.localPosition),
                    localRotation = new QuaternionData(rectTransform.localRotation),
                    localScale = new Vector3Data(rectTransform.localScale)
                };
            }

            public void Apply(RectTransform rectTransform)
            {
                rectTransform.anchorMin = anchorMin.ToVector2();
                rectTransform.anchorMax = anchorMax.ToVector2();
                rectTransform.pivot = pivot.ToVector2();
                rectTransform.anchoredPosition = anchoredPosition.ToVector2();
                rectTransform.sizeDelta = sizeDelta.ToVector2();
                rectTransform.localPosition = localPosition.ToVector3();
                rectTransform.localRotation = localRotation.ToQuaternion();
                rectTransform.localScale = localScale.ToVector3();
            }
        }

        [Serializable]
        private struct ImageSnapshot
        {
            public bool enabled;
            public bool raycastTarget;
            public ColorData color;
            public string spritePath;
            public string spriteName;
            public int type;
            public bool preserveAspect;
            public int fillMethod;
            public float fillAmount;
            public int fillOrigin;
            public bool fillClockwise;

            public static ImageSnapshot Capture(Image image)
            {
                return new ImageSnapshot
                {
                    enabled = image.enabled,
                    raycastTarget = image.raycastTarget,
                    color = new ColorData(image.color),
                    spritePath = image.sprite != null ? AssetDatabase.GetAssetPath(image.sprite) : string.Empty,
                    spriteName = image.sprite != null ? image.sprite.name : string.Empty,
                    type = (int)image.type,
                    preserveAspect = image.preserveAspect,
                    fillMethod = (int)image.fillMethod,
                    fillAmount = image.fillAmount,
                    fillOrigin = image.fillOrigin,
                    fillClockwise = image.fillClockwise
                };
            }

            public void Apply(Image image)
            {
                image.enabled = enabled;
                image.raycastTarget = raycastTarget;
                image.color = color.ToColor();
                image.sprite = LoadSprite(spritePath, spriteName);

                if (Enum.IsDefined(typeof(Image.Type), type))
                {
                    image.type = (Image.Type)type;
                }

                image.preserveAspect = preserveAspect;

                if (Enum.IsDefined(typeof(Image.FillMethod), fillMethod))
                {
                    image.fillMethod = (Image.FillMethod)fillMethod;
                }

                image.fillAmount = fillAmount;
                image.fillOrigin = fillOrigin;
                image.fillClockwise = fillClockwise;
            }
        }

        [Serializable]
        private struct OutlineSnapshot
        {
            public bool enabled;
            public ColorData effectColor;
            public Vector2Data effectDistance;
            public bool useGraphicAlpha;

            public static OutlineSnapshot Capture(Outline outline)
            {
                return new OutlineSnapshot
                {
                    enabled = outline.enabled,
                    effectColor = new ColorData(outline.effectColor),
                    effectDistance = new Vector2Data(outline.effectDistance),
                    useGraphicAlpha = outline.useGraphicAlpha
                };
            }

            public void Apply(Outline outline)
            {
                outline.enabled = enabled;
                outline.effectColor = effectColor.ToColor();
                outline.effectDistance = effectDistance.ToVector2();
                outline.useGraphicAlpha = useGraphicAlpha;
            }
        }

        [Serializable]
        private struct TextSnapshot
        {
            public bool enabled;
            public bool raycastTarget;
            public string text;
            public int fontSize;
            public ColorData color;
            public int alignment;
            public int fontStyle;
            public bool alignByGeometry;
            public bool resizeTextForBestFit;
            public int resizeTextMinSize;
            public int resizeTextMaxSize;
            public int horizontalOverflow;
            public int verticalOverflow;
            public float lineSpacing;
            public bool supportRichText;
            public string fontPath;
            public string fontName;

            public static TextSnapshot Capture(Text text)
            {
                return new TextSnapshot
                {
                    enabled = text.enabled,
                    raycastTarget = text.raycastTarget,
                    text = text.text,
                    fontSize = text.fontSize,
                    color = new ColorData(text.color),
                    alignment = (int)text.alignment,
                    fontStyle = (int)text.fontStyle,
                    alignByGeometry = text.alignByGeometry,
                    resizeTextForBestFit = text.resizeTextForBestFit,
                    resizeTextMinSize = text.resizeTextMinSize,
                    resizeTextMaxSize = text.resizeTextMaxSize,
                    horizontalOverflow = (int)text.horizontalOverflow,
                    verticalOverflow = (int)text.verticalOverflow,
                    lineSpacing = text.lineSpacing,
                    supportRichText = text.supportRichText,
                    fontPath = text.font != null ? AssetDatabase.GetAssetPath(text.font) : string.Empty,
                    fontName = text.font != null ? text.font.name : string.Empty
                };
            }

            public void Apply(Text textComponent)
            {
                textComponent.enabled = enabled;
                textComponent.raycastTarget = raycastTarget;
                textComponent.text = text;
                textComponent.fontSize = fontSize;
                textComponent.color = color.ToColor();
                textComponent.alignment = Enum.IsDefined(typeof(TextAnchor), alignment)
                    ? (TextAnchor)alignment
                    : textComponent.alignment;
                textComponent.fontStyle = Enum.IsDefined(typeof(FontStyle), fontStyle)
                    ? (FontStyle)fontStyle
                    : textComponent.fontStyle;
                textComponent.alignByGeometry = alignByGeometry;
                textComponent.resizeTextForBestFit = resizeTextForBestFit;
                textComponent.resizeTextMinSize = resizeTextMinSize;
                textComponent.resizeTextMaxSize = resizeTextMaxSize;
                textComponent.horizontalOverflow = Enum.IsDefined(typeof(HorizontalWrapMode), horizontalOverflow)
                    ? (HorizontalWrapMode)horizontalOverflow
                    : textComponent.horizontalOverflow;
                textComponent.verticalOverflow = Enum.IsDefined(typeof(VerticalWrapMode), verticalOverflow)
                    ? (VerticalWrapMode)verticalOverflow
                    : textComponent.verticalOverflow;
                textComponent.lineSpacing = lineSpacing;
                textComponent.supportRichText = supportRichText;

                Font font = LoadFont(fontPath, fontName);
                if (font != null)
                {
                    textComponent.font = font;
                }
            }
        }

        [Serializable]
        private struct TmpTextSnapshot
        {
            public bool enabled;
            public bool raycastTarget;
            public string text;
            public float fontSize;
            public ColorData color;
            public int alignment;

            public static TmpTextSnapshot Capture(Component tmpText)
            {
                Graphic graphic = tmpText as Graphic;
                Behaviour behaviour = tmpText as Behaviour;
                Type type = tmpText.GetType();

                return new TmpTextSnapshot
                {
                    enabled = behaviour == null || behaviour.enabled,
                    raycastTarget = graphic != null && graphic.raycastTarget,
                    text = GetPropertyValue(type, tmpText, "text", string.Empty),
                    fontSize = GetPropertyValue(type, tmpText, "fontSize", 0f),
                    color = new ColorData(graphic != null ? graphic.color : Color.white),
                    alignment = GetEnumPropertyValue(type, tmpText, "alignment")
                };
            }

            public void Apply(Component tmpText)
            {
                Graphic graphic = tmpText as Graphic;
                Behaviour behaviour = tmpText as Behaviour;
                Type type = tmpText.GetType();

                if (behaviour != null)
                {
                    behaviour.enabled = enabled;
                }

                if (graphic != null)
                {
                    graphic.raycastTarget = raycastTarget;
                    graphic.color = color.ToColor();
                }

                SetPropertyValue(type, tmpText, "text", text);
                SetPropertyValue(type, tmpText, "fontSize", fontSize);
                SetEnumPropertyValue(type, tmpText, "alignment", alignment);
            }
        }

        [Serializable]
        private struct Vector2Data
        {
            public float x;
            public float y;

            public Vector2Data(Vector2 value)
            {
                x = value.x;
                y = value.y;
            }

            public Vector2 ToVector2()
            {
                return new Vector2(x, y);
            }
        }

        [Serializable]
        private struct Vector3Data
        {
            public float x;
            public float y;
            public float z;

            public Vector3Data(Vector3 value)
            {
                x = value.x;
                y = value.y;
                z = value.z;
            }

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }

        [Serializable]
        private struct QuaternionData
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public QuaternionData(Quaternion value)
            {
                x = value.x;
                y = value.y;
                z = value.z;
                w = value.w;
            }

            public Quaternion ToQuaternion()
            {
                return new Quaternion(x, y, z, w);
            }
        }

        [Serializable]
        private struct ColorData
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public ColorData(Color value)
            {
                r = value.r;
                g = value.g;
                b = value.b;
                a = value.a;
            }

            public Color ToColor()
            {
                return new Color(r, g, b, a);
            }
        }

        private static Sprite LoadSprite(string assetPath, string spriteName)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite sprite && string.Equals(sprite.name, spriteName, StringComparison.Ordinal))
                {
                    return sprite;
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Font LoadFont(string assetPath, string fontName)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            Font font = AssetDatabase.LoadAssetAtPath<Font>(assetPath);
            if (font != null && (string.IsNullOrEmpty(fontName) || string.Equals(font.name, fontName, StringComparison.Ordinal)))
            {
                return font;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Font candidate && string.Equals(candidate.name, fontName, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static T GetPropertyValue<T>(Type type, object instance, string propertyName, T fallback)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanRead)
            {
                return fallback;
            }

            object value = property.GetValue(instance);
            return value is T typedValue ? typedValue : fallback;
        }

        private static void SetPropertyValue<T>(Type type, object instance, string propertyName, T value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            property.SetValue(instance, value);
        }

        private static int GetEnumPropertyValue(Type type, object instance, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanRead || !property.PropertyType.IsEnum)
            {
                return 0;
            }

            object value = property.GetValue(instance);
            return Convert.ToInt32(value);
        }

        private static void SetEnumPropertyValue(Type type, object instance, string propertyName, int value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            if (!Enum.IsDefined(property.PropertyType, value))
            {
                return;
            }

            property.SetValue(instance, Enum.ToObject(property.PropertyType, value));
        }
    }
}
#endif
