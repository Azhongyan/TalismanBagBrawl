using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    /// <summary>
    /// One-time, explicit migration of the currently visible BaseStats layout into manual rows.
    /// It has no automatic editor callbacks and never saves the Scene.
    /// </summary>
    internal static class ItemDetailBaseStatsManualRowsAuthoring
    {
        private const string MenuPath =
            "TalismanBag/Item Sandbox/Migrate BaseStats To Manual Rows From Current Non-Play Layout (Once)";
        private const string ApplyGapMenuPath =
            "TalismanBag/Item Sandbox/Set BaseStats Icon-To-Text Gap To 10 (Manual Only)";
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PanelPath =
            "ItemSandboxCanvas/MobileSafeAreaRoot/ItemSandboxRoot/ItemDetailPanel";
        private const string BaseStatsPath =
            "ItemDetailScrollView/Viewport/DetailContent/BaseStatsSection";
        private const string RowsRootName = "BaseStatRowsRoot";
        private const string RowNamePrefix = "BaseStatRow_";
        private const string TextNamePrefix = "BaseStatText_";
        private const string IconNamePrefix = "BaseStatIconSlot_";
        private const string InlineIconNamePrefix = "InlineArrayModifierIcon_";
        private const int RowCount = 8;
        private const float IconToTextGap = 10f;

        [MenuItem(MenuPath, false, 2210)]
        public static void MigrateCurrentNonPlayLayoutOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before migrating BaseStats rows.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException(
                    "Open the ItemSandbox Scene first. This command intentionally does not open or replace a Scene.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform section = panel.Find(BaseStatsPath)
                ?? throw new InvalidOperationException("BaseStatsSection not found: " + BaseStatsPath);
            Transform body = section.Find("BodyText")
                ?? throw new InvalidOperationException("BaseStatsSection/BodyText not found.");

            Transform existingRoot = body.Find(RowsRootName);
            if (existingRoot != null)
            {
                ValidateExistingRows(existingRoot);
                Debug.Log(
                    "ITEM_DETAIL_BASE_STATS_MANUAL_ROWS_ALREADY_PASS: existing row layout checked only; "
                    + "no RectTransform, hierarchy, content, or Scene save write was performed.");
                return;
            }

            Text sourceText = body.GetComponent<Text>()
                ?? throw new InvalidOperationException("BaseStats BodyText Text component missing.");
            RectTransform bodyRect = body as RectTransform
                ?? throw new InvalidOperationException("BaseStats BodyText RectTransform missing.");
            RectTransform panelRect = panel as RectTransform
                ?? throw new InvalidOperationException("ItemDetailPanel RectTransform missing.");

            string[] sourceLines = (sourceText.text ?? string.Empty)
                .Replace("\r", string.Empty)
                .Split('\n');
            if (sourceLines.Length != RowCount)
            {
                throw new InvalidOperationException(
                    "Expected exactly " + RowCount + " authored BaseStats lines, found " + sourceLines.Length
                    + ". No migration was performed.");
            }

            Transform[] icons = Enumerable.Range(0, RowCount)
                .Select(index => body.Find(IconNamePrefix + index)
                    ?? throw new InvalidOperationException("Missing original slot: " + IconNamePrefix + index))
                .ToArray();
            Transform inlineSource = body.Find(InlineIconNamePrefix + "0")
                ?? throw new InvalidOperationException("Missing original slot: " + InlineIconNamePrefix + "0");

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();

            Vector3[] iconWorldPositions = icons.Select(icon => icon.position).ToArray();
            Vector3[] iconBodyPositions = iconWorldPositions
                .Select(bodyRect.InverseTransformPoint)
                .ToArray();
            float[] textLeftOffsets = icons
                .Select(icon => ResolveTextLeftOffset((RectTransform)icon, bodyRect))
                .ToArray();
            Vector3 inlineBodyPosition = bodyRect.InverseTransformPoint(inlineSource.position);
            Vector3 inlineWorldEuler = inlineSource.eulerAngles;
            Vector3 inlineLocalScale = inlineSource.localScale;
            bool inlineWasActive = inlineSource.gameObject.activeSelf;
            RectTransform inlineSourceRect = inlineSource as RectTransform
                ?? throw new InvalidOperationException("InlineArrayModifierIcon_0 RectTransform missing.");
            RectSnapshot inlineRectSnapshot = RectSnapshot.Capture(inlineSourceRect);
            Image inlineSourceImage = inlineSource.GetComponent<Image>()
                ?? throw new InvalidOperationException("InlineArrayModifierIcon_0 Image missing.");
            ImageSnapshot inlineImageSnapshot = ImageSnapshot.Capture(inlineSourceImage);

            float[] rowOffsets = new float[RowCount];
            for (int index = 0; index < RowCount; index++)
            {
                rowOffsets[index] = iconBodyPositions[index].y - iconBodyPositions[0].y;
            }

            float rowHeight = ResolveRowHeight(rowOffsets, sourceText);
            int closestInlineRow = ResolveClosestInlineRow(iconBodyPositions, inlineBodyPosition);

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Migrate BaseStats To Manual Rows");

            VerticalLayoutGroup legacyIconLayout = body.GetComponent<VerticalLayoutGroup>();
            if (legacyIconLayout != null && legacyIconLayout.enabled)
            {
                Undo.RecordObject(legacyIconLayout, "Disable legacy BaseStats icon layout");
                legacyIconLayout.enabled = false;
                EditorUtility.SetDirty(legacyIconLayout);
            }

            GameObject rootObject = new GameObject(RowsRootName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(rootObject, "Create BaseStatRowsRoot");
            RectTransform rowsRoot = rootObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(rowsRoot, body, "Parent BaseStatRowsRoot");
            SetFillParent(rowsRoot);

            RectTransform[] rows = new RectTransform[RowCount];
            for (int index = 0; index < RowCount; index++)
            {
                rows[index] = CreateRow(rowsRoot, index, rowOffsets[index], rowHeight);
                CreateRowText(
                    rows[index],
                    sourceText,
                    sourceLines[index],
                    index,
                    textLeftOffsets[index]);
            }

            for (int index = 0; index < RowCount; index++)
            {
                Undo.SetTransformParent(icons[index], rows[index], "Move original BaseStat icon into authored row");
                icons[index].position = iconWorldPositions[index];
                EditorUtility.SetDirty(icons[index]);
            }

            for (int index = 0; index < RowCount; index++)
            {
                Transform slot;
                if (index == 0)
                {
                    slot = inlineSource;
                    Undo.SetTransformParent(slot, rows[index], "Move original inline array icon into row zero");
                }
                else
                {
                    GameObject slotObject = new GameObject(
                        InlineIconNamePrefix + index,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image));
                    Undo.RegisterCreatedObjectUndo(slotObject, "Create authored inline array icon slot");
                    slot = slotObject.transform;
                    Undo.SetTransformParent(slot, rows[index], "Parent authored inline array icon slot");
                    inlineImageSnapshot.Apply(slotObject.GetComponent<Image>());
                }

                RectTransform slotRect = (RectTransform)slot;
                inlineRectSnapshot.ApplyShape(slotRect);
                Vector3 desiredBodyPosition = inlineBodyPosition;
                desiredBodyPosition.y += rowOffsets[index] - rowOffsets[closestInlineRow];
                slot.position = bodyRect.TransformPoint(desiredBodyPosition);
                slot.eulerAngles = inlineWorldEuler;
                slot.localScale = inlineLocalScale;
                slot.gameObject.SetActive(inlineWasActive && index == closestInlineRow);
                EditorUtility.SetDirty(slotRect);
            }

            Undo.RecordObject(sourceText, "Replace legacy BaseStats display text with transparent layout scaffold");
            sourceText.text = BuildTransparentLayoutScaffold(sourceLines);
            EditorUtility.SetDirty(sourceText);

            ValidateExistingRows(rowsRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = rowsRoot.gameObject;

            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_BASE_STATS_MANUAL_ROWS_PASS: created 8 inspector-authored rows from the current "
                + "non-Play icon positions; preserved BaseStatIconSlot_0..7 identity; disabled only the legacy "
                + "BodyText icon VerticalLayoutGroup; Scene remains unsaved for visual review.");
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            return !EditorApplication.isPlayingOrWillChangePlaymode
                && scene.IsValid()
                && scene.isLoaded
                && Normalize(scene.path) == ScenePath;
        }

        [MenuItem(ApplyGapMenuPath, false, 2211)]
        public static void ApplyIconToTextGapOnly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before adjusting BaseStats text positions.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException("Open the ItemSandbox Scene first.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform body = panel.Find(BaseStatsPath + "/BodyText")
                ?? throw new InvalidOperationException("BaseStatsSection/BodyText not found.");
            Transform rowsRoot = body.Find(RowsRootName)
                ?? throw new InvalidOperationException(
                    "BaseStatRowsRoot does not exist. Run the one-time BaseStats row migration first.");

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set BaseStats icon-to-text gap to 10");
            for (int index = 0; index < RowCount; index++)
            {
                RectTransform row = rowsRoot.Find(RowNamePrefix + index) as RectTransform
                    ?? throw new InvalidOperationException("Missing authored row: " + RowNamePrefix + index);
                RectTransform icon = row.Find(IconNamePrefix + index) as RectTransform
                    ?? throw new InvalidOperationException("Missing authored icon: " + IconNamePrefix + index);
                RectTransform textRect = row.Find(TextNamePrefix + index) as RectTransform
                    ?? throw new InvalidOperationException("Missing authored text: " + TextNamePrefix + index);
                Text text = textRect.GetComponent<Text>()
                    ?? throw new InvalidOperationException("Missing Text component: " + TextNamePrefix + index);

                float leftOffset = ResolveTextLeftOffset(icon, row);
                Undo.RecordObject(textRect, "Set BaseStat text left edge");
                textRect.offsetMin = new Vector2(leftOffset, textRect.offsetMin.y);
                EditorUtility.SetDirty(textRect);

                string trimmed = (text.text ?? string.Empty).TrimStart();
                if (!string.Equals(text.text, trimmed, StringComparison.Ordinal))
                {
                    Undo.RecordObject(text, "Remove legacy BaseStat text padding");
                    text.text = trimmed;
                    EditorUtility.SetDirty(text);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_BASE_STATS_ICON_TEXT_GAP_10_PASS: text left edges now sit 10 UI units "
                + "after their corresponding icon right edges; icons and all other RectTransforms were unchanged; "
                + "Scene remains unsaved for visual review.");
        }

        [MenuItem(ApplyGapMenuPath, true)]
        private static bool ValidateApplyGapMenu()
        {
            return ValidateMenu();
        }

        private static RectTransform CreateRow(
            RectTransform rowsRoot,
            int index,
            float verticalOffset,
            float rowHeight)
        {
            GameObject rowObject = new GameObject(RowNamePrefix + index, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(rowObject, "Create authored BaseStat row");
            RectTransform row = rowObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(row, rowsRoot, "Parent authored BaseStat row");
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, verticalOffset);
            row.sizeDelta = new Vector2(0f, rowHeight);
            return row;
        }

        private static void CreateRowText(
            RectTransform row,
            Text source,
            string line,
            int index,
            float leftOffset)
        {
            GameObject textObject = new GameObject(
                TextNamePrefix + index,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            Undo.RegisterCreatedObjectUndo(textObject, "Create authored BaseStat row text");
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(textRect, row, "Parent authored BaseStat row text");
            SetFillParent(textRect);
            textRect.offsetMin = new Vector2(leftOffset, textRect.offsetMin.y);

            Text target = textObject.GetComponent<Text>();
            CopyTextStyle(source, target);
            target.text = line.TrimStart();
        }

        private static float ResolveTextLeftOffset(RectTransform icon, RectTransform body)
        {
            Vector3[] corners = new Vector3[4];
            icon.GetWorldCorners(corners);
            float iconRight = corners
                .Select(body.InverseTransformPoint)
                .Max(corner => corner.x);
            return iconRight - body.rect.xMin + IconToTextGap;
        }

        private static void CopyTextStyle(Text source, Text target)
        {
            target.font = source.font;
            target.fontStyle = source.fontStyle;
            target.fontSize = source.fontSize;
            target.lineSpacing = source.lineSpacing;
            target.supportRichText = source.supportRichText;
            target.alignment = source.alignment;
            target.alignByGeometry = source.alignByGeometry;
            target.resizeTextForBestFit = source.resizeTextForBestFit;
            target.resizeTextMinSize = source.resizeTextMinSize;
            target.resizeTextMaxSize = source.resizeTextMaxSize;
            target.horizontalOverflow = source.horizontalOverflow;
            target.verticalOverflow = source.verticalOverflow;
            target.color = source.color;
            target.material = source.material;
            target.raycastTarget = source.raycastTarget;
            target.maskable = source.maskable;
        }

        private static void SetFillParent(RectTransform rect)
        {
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static float ResolveRowHeight(float[] rowOffsets, Text sourceText)
        {
            float[] distances = new float[RowCount - 1];
            for (int index = 1; index < RowCount; index++)
            {
                distances[index - 1] = Mathf.Abs(rowOffsets[index] - rowOffsets[index - 1]);
            }

            float authoredPitch = distances.Where(value => value > 0.01f).DefaultIfEmpty(0f).Average();
            float fontPitch = sourceText.fontSize * Mathf.Max(0.1f, sourceText.lineSpacing);
            return Mathf.Max(1f, authoredPitch > 0.01f ? authoredPitch : fontPitch);
        }

        private static int ResolveClosestInlineRow(Vector3[] iconPositions, Vector3 inlinePosition)
        {
            int closest = 0;
            float closestDistance = float.MaxValue;
            for (int index = 0; index < iconPositions.Length; index++)
            {
                float distance = Mathf.Abs(iconPositions[index].y - inlinePosition.y);
                if (distance < closestDistance)
                {
                    closest = index;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private static string BuildTransparentLayoutScaffold(string[] lines)
        {
            return string.Join(
                "\n",
                lines.Select(line => "<color=#00000000>" + StripRichTextTags(line) + "</color>"));
        }

        private static string StripRichTextTags(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length);
            bool insideTag = false;
            foreach (char character in value)
            {
                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (insideTag && character == '>')
                {
                    insideTag = false;
                    continue;
                }

                if (!insideTag)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static void ValidateExistingRows(Transform rowsRoot)
        {
            if (rowsRoot.GetComponentsInChildren<LayoutGroup>(true).Any(group => group.enabled))
            {
                throw new InvalidOperationException(
                    "BaseStatRowsRoot contains an enabled LayoutGroup; manual RectTransforms must remain authoritative.");
            }

            for (int index = 0; index < RowCount; index++)
            {
                Transform row = rowsRoot.Find(RowNamePrefix + index)
                    ?? throw new InvalidOperationException("Missing authored row: " + RowNamePrefix + index);
                if (row.Find(TextNamePrefix + index)?.GetComponent<Text>() == null)
                {
                    throw new InvalidOperationException("Missing authored row Text: " + TextNamePrefix + index);
                }

                if (row.Find(IconNamePrefix + index)?.GetComponent<Image>() == null)
                {
                    throw new InvalidOperationException("Missing preserved icon slot: " + IconNamePrefix + index);
                }

                if (row.Find(InlineIconNamePrefix + index)?.GetComponent<Image>() == null)
                {
                    throw new InvalidOperationException(
                        "Missing authored inline array icon slot: " + InlineIconNamePrefix + index);
                }
            }
        }

        private static Transform FindScenePath(Scene scene, string path)
        {
            string[] segments = path.Split('/');
            GameObject root = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => string.Equals(candidate.name, segments[0], StringComparison.Ordinal));
            if (root == null)
            {
                return null;
            }

            Transform current = root.transform;
            for (int index = 1; index < segments.Length && current != null; index++)
            {
                current = current.Find(segments[index]);
            }

            return current;
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private readonly struct RectSnapshot
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 pivot;
            private readonly Vector2 sizeDelta;

            private RectSnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                sizeDelta = rect.sizeDelta;
            }

            public static RectSnapshot Capture(RectTransform rect)
            {
                return new RectSnapshot(rect);
            }

            public void ApplyShape(RectTransform rect)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = pivot;
                rect.sizeDelta = sizeDelta;
            }
        }

        private readonly struct ImageSnapshot
        {
            private readonly Sprite sprite;
            private readonly Color color;
            private readonly Image.Type type;
            private readonly bool preserveAspect;
            private readonly bool fillCenter;
            private readonly Image.FillMethod fillMethod;
            private readonly float fillAmount;
            private readonly bool fillClockwise;
            private readonly int fillOrigin;
            private readonly Material material;
            private readonly bool raycastTarget;
            private readonly bool maskable;

            private ImageSnapshot(Image image)
            {
                sprite = image.sprite;
                color = image.color;
                type = image.type;
                preserveAspect = image.preserveAspect;
                fillCenter = image.fillCenter;
                fillMethod = image.fillMethod;
                fillAmount = image.fillAmount;
                fillClockwise = image.fillClockwise;
                fillOrigin = image.fillOrigin;
                material = image.material;
                raycastTarget = image.raycastTarget;
                maskable = image.maskable;
            }

            public static ImageSnapshot Capture(Image image)
            {
                return new ImageSnapshot(image);
            }

            public void Apply(Image image)
            {
                image.sprite = sprite;
                image.color = color;
                image.type = type;
                image.preserveAspect = preserveAspect;
                image.fillCenter = fillCenter;
                image.fillMethod = fillMethod;
                image.fillAmount = fillAmount;
                image.fillClockwise = fillClockwise;
                image.fillOrigin = fillOrigin;
                image.material = material;
                image.raycastTarget = raycastTarget;
                image.maskable = maskable;
            }
        }
    }
}
