using System;
using System.Linq;
using TalismanBag.Items.Detail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    /// <summary>
    /// Explicit one-time binding of authored manual rows to a read-only preferred-height measurement.
    /// It has no automatic editor callbacks and never saves the Scene.
    /// </summary>
    internal static class ItemDetailAuthoredRowsAdaptiveHeightAuthoring
    {
        private const string MenuPath =
            "Tools/Talisman Bag/Dev Only/Item Sandbox/Enable Authored Row Adaptive Heights (Once)";
        private const string DynamicAffixRowsMenuPath =
            "Tools/Talisman Bag/Dev Only/Item Sandbox/Enable Dynamic Affix Row Heights (Once)";
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PanelPath =
            "ItemSandboxCanvas/MobileSafeAreaRoot/ItemSandboxRoot/ItemDetailPanel";
        private const string DetailContentPath = "ItemDetailScrollView/Viewport/DetailContent";
        private const float AffixRowSpacing = 6f;

        private static readonly SectionBinding[] Bindings =
        {
            new("BaseStatsSection", "BaseStatRowsRoot", "BaseStatRow_"),
            new("FixedAffixSection", "FixedAffixRowsRoot", "FixedAffixRow_"),
            new("RandomAffixSection", "RandomAffixRowsRoot", "RandomAffixRow_")
        };

        [MenuItem(MenuPath, false, 2213)]
        public static void EnableAdaptiveHeightsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before enabling authored Row adaptive heights.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException(
                    "Open the ItemSandbox Scene first. This command intentionally does not open or replace a Scene.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform detailContent = panel.Find(DetailContentPath)
                ?? throw new InvalidOperationException("DetailContent not found: " + DetailContentPath);

            BindingTarget[] targets = Bindings.Select(binding => ResolveTarget(detailContent, binding)).ToArray();
            int existingCount = targets.Count(target => target.layoutElement != null);
            if (existingCount == targets.Length)
            {
                ValidateAlreadyApplied(targets);
                Debug.Log(
                    "ITEM_DETAIL_AUTHORED_ROWS_ADAPTIVE_HEIGHT_ALREADY_PASS: components and empty legacy "
                    + "BodyText values checked only; no RectTransform, hierarchy, or Scene save write was performed.");
                return;
            }

            if (existingCount != 0)
            {
                throw new InvalidOperationException(
                    "Partial adaptive-height binding detected. No changes were performed; inspect the three BodyText objects.");
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Enable ItemDetail authored Row adaptive heights");
            foreach (BindingTarget target in targets)
            {
                Undo.RecordObject(target.bodyText, "Clear legacy transparent layout text");
                target.bodyText.text = string.Empty;
                EditorUtility.SetDirty(target.bodyText);

                ItemDetailAuthoredRowsLayoutElement layoutElement =
                    Undo.AddComponent<ItemDetailAuthoredRowsLayoutElement>(target.body.gameObject);
                Undo.RecordObject(layoutElement, "Bind authored Rows preferred-height measurement");
                layoutElement.ConfigureEditor(target.rowsRoot, target.binding.rowNamePrefix);
                EditorUtility.SetDirty(layoutElement);
            }

            NormalizeFixedFirstRowOnce(targets.Single(target =>
                target.binding.sectionName == "FixedAffixSection"));

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = targets[0].body.gameObject;
            Canvas.ForceUpdateCanvases();
            foreach (BindingTarget target in targets)
            {
                target.body.GetComponent<ItemDetailAuthoredRowsLayoutElement>()?.InvalidateLayout();
            }

            Debug.Log(
                "ITEM_DETAIL_AUTHORED_ROWS_ADAPTIVE_HEIGHT_PASS: BaseStats/FixedAffix/RandomAffix legacy "
                + "transparent BodyText content cleared; read-only preferred-height measurement attached; "
                + "FixedAffixRow_0 normalized to the shared top anchor; all other authored Row RectTransforms "
                + "were preserved; Scene remains unsaved for visual review.");
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

        [MenuItem(DynamicAffixRowsMenuPath, false, 2214)]
        public static void EnableDynamicAffixRowHeightsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before enabling dynamic authored affix Rows.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException(
                    "Open the ItemSandbox Scene first. This command intentionally does not open or replace a Scene.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform detailContent = panel.Find(DetailContentPath)
                ?? throw new InvalidOperationException("DetailContent not found: " + DetailContentPath);

            BindingTarget[] targets = Bindings
                .Where(binding => binding.sectionName == "FixedAffixSection"
                    || binding.sectionName == "RandomAffixSection")
                .Select(binding => ResolveTarget(detailContent, binding))
                .ToArray();
            if (targets.Any(target => target.layoutElement == null))
            {
                throw new InvalidOperationException(
                    "Run 'Enable Authored Row Adaptive Heights (Once)' first. No geometry was changed.");
            }

            ValidateAlreadyApplied(targets);
            int initializedCount = targets.Count(target =>
                target.layoutElement.EditorAffixDynamicRowsInitialized);
            if (initializedCount == targets.Length)
            {
                ValidateDynamicAffixRows(targets);
                Debug.Log(
                    "ITEM_DETAIL_AFFIX_ROWS_DYNAMIC_HEIGHT_ALREADY_PASS: one-time marker and dynamic Row "
                    + "bindings checked only; "
                    + "no RectTransform, hierarchy, or Scene save write was performed.");
                return;
            }

            if (initializedCount != 0)
            {
                throw new InvalidOperationException(
                    "Partial dynamic affix Row marker detected. No changes were performed; inspect FixedAffix/RandomAffix Rows.");
            }

            if (targets.Any(HasDynamicAffixComponents))
            {
                throw new InvalidOperationException(
                    "Partial dynamic affix Row components detected without the one-time marker. "
                    + "No changes were performed; inspect FixedAffix/RandomAffix Rows.");
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Enable ItemDetail dynamic affix Row heights");
            foreach (BindingTarget target in targets)
            {
                ItemDetailAuthoredRowsVerticalLayoutGroup rowsLayout =
                    Undo.AddComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>(target.rowsRoot.gameObject);
                Undo.RecordObject(rowsLayout, "Configure affix vertical-only Row layout");
                rowsLayout.ConfigureEditor(AffixRowSpacing, true);
                EditorUtility.SetDirty(rowsLayout);

                foreach (RectTransform row in GetAuthoredRows(target))
                {
                    Text rowText = row.GetComponentsInChildren<Text>(true).FirstOrDefault()
                        ?? throw new InvalidOperationException(row.name + " has no Text child.");
                    ItemDetailAuthoredTextRowLayoutElement rowLayout =
                        Undo.AddComponent<ItemDetailAuthoredTextRowLayoutElement>(row.gameObject);
                    Undo.RecordObject(rowLayout, "Bind " + row.name + " wrapped-text preferred height");
                    rowLayout.ConfigureEditor(rowText, row.rect.height, 0f);
                    EditorUtility.SetDirty(rowLayout);
                }

                Undo.RecordObject(target.layoutElement, "Mark dynamic affix Row binding");
                target.layoutElement.MarkAffixDynamicRowsEditor();
                EditorUtility.SetDirty(target.layoutElement);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = targets[0].rowsRoot.gameObject;
            Canvas.ForceUpdateCanvases();
            foreach (BindingTarget target in targets)
            {
                target.layoutElement.InvalidateLayout();
            }

            Debug.Log(
                "ITEM_DETAIL_AFFIX_ROWS_DYNAMIC_HEIGHT_PASS: FixedAffix and RandomAffix Rows now read each "
                + "field's wrapped preferred height and are arranged by a Y/height-only LayoutGroup with 6 units "
                + "of spacing; X, width, scale, icons, fonts, line spacing, colors, BaseStats, and all other "
                + "sections were preserved; Scene remains unsaved for visual review.");
        }

        [MenuItem(DynamicAffixRowsMenuPath, true)]
        private static bool ValidateDynamicAffixRowsMenu()
        {
            return ValidateMenu();
        }

        private static BindingTarget ResolveTarget(Transform detailContent, SectionBinding binding)
        {
            Transform section = detailContent.Find(binding.sectionName)
                ?? throw new InvalidOperationException(binding.sectionName + " not found.");
            Transform body = section.Find("BodyText")
                ?? throw new InvalidOperationException(binding.sectionName + "/BodyText not found.");
            Text bodyText = body.GetComponent<Text>()
                ?? throw new InvalidOperationException(binding.sectionName + "/BodyText Text component missing.");
            RectTransform rowsRoot = body.Find(binding.rowsRootName) as RectTransform
                ?? throw new InvalidOperationException(
                    binding.sectionName + "/BodyText/" + binding.rowsRootName + " not found.");
            if (rowsRoot.childCount == 0)
            {
                throw new InvalidOperationException(binding.rowsRootName + " contains no authored Rows.");
            }

            return new BindingTarget(
                binding,
                body,
                bodyText,
                rowsRoot,
                body.GetComponent<ItemDetailAuthoredRowsLayoutElement>());
        }

        private static void ValidateAlreadyApplied(BindingTarget[] targets)
        {
            foreach (BindingTarget target in targets)
            {
                if (!string.IsNullOrEmpty(target.bodyText.text))
                {
                    throw new InvalidOperationException(
                        target.binding.sectionName + " legacy BodyText is not empty. No changes were performed.");
                }

                if (target.layoutElement.RowsRoot != target.rowsRoot
                    || !string.Equals(
                        target.layoutElement.RowNamePrefix,
                        target.binding.rowNamePrefix,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        target.binding.sectionName
                        + " adaptive-height binding does not match its authored RowsRoot. No changes were performed.");
                }
            }

            RectTransform fixedFirstRow = targets
                .Single(target => target.binding.sectionName == "FixedAffixSection")
                .rowsRoot.Find("FixedAffixRow_0") as RectTransform;
            if (fixedFirstRow == null
                || !Mathf.Approximately(fixedFirstRow.anchorMin.y, 1f)
                || !Mathf.Approximately(fixedFirstRow.anchorMax.y, 1f))
            {
                throw new InvalidOperationException(
                    "FixedAffixRow_0 is no longer top-anchored. Rerun is check-only and did not overwrite it.");
            }
        }

        private static void NormalizeFixedFirstRowOnce(BindingTarget fixedTarget)
        {
            RectTransform row = fixedTarget.rowsRoot.Find("FixedAffixRow_0") as RectTransform
                ?? throw new InvalidOperationException("FixedAffixRow_0 not found.");
            Undo.RecordObject(row, "Normalize FixedAffixRow_0 top anchor");
            row.anchorMin = new Vector2(row.anchorMin.x, 1f);
            row.anchorMax = new Vector2(row.anchorMax.x, 1f);
            row.pivot = new Vector2(row.pivot.x, 1f);
            row.anchoredPosition = new Vector2(row.anchoredPosition.x, 0f);
            EditorUtility.SetDirty(row);
        }

        private static RectTransform[] GetAuthoredRows(BindingTarget target)
        {
            return Enumerable.Range(0, target.rowsRoot.childCount)
                .Select(index => target.rowsRoot.GetChild(index) as RectTransform)
                .Where(row => row != null
                    && row.name.StartsWith(target.binding.rowNamePrefix, StringComparison.Ordinal))
                .OrderBy(row => row.GetSiblingIndex())
                .ToArray();
        }

        private static bool HasDynamicAffixComponents(BindingTarget target)
        {
            return target.rowsRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>() != null
                || GetAuthoredRows(target)
                    .Any(row => row.GetComponent<ItemDetailAuthoredTextRowLayoutElement>() != null);
        }

        private static void ValidateDynamicAffixRows(BindingTarget[] targets)
        {
            foreach (BindingTarget target in targets)
            {
                ItemDetailAuthoredRowsVerticalLayoutGroup rowsLayout =
                    target.rowsRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>();
                if (rowsLayout == null)
                {
                    throw new InvalidOperationException(
                        target.binding.rowsRootName + " dynamic Y/height-only LayoutGroup is missing.");
                }

                RectTransform[] rows = GetAuthoredRows(target);
                if (rows.Length == 0)
                {
                    throw new InvalidOperationException(target.binding.rowsRootName + " contains no authored Rows.");
                }

                foreach (RectTransform row in rows)
                {
                    ItemDetailAuthoredTextRowLayoutElement rowLayout =
                        row.GetComponent<ItemDetailAuthoredTextRowLayoutElement>();
                    Text rowText = row.GetComponentsInChildren<Text>(true).FirstOrDefault();
                    if (rowLayout == null || rowText == null || rowLayout.RowText != rowText)
                    {
                        throw new InvalidOperationException(
                            row.name + " dynamic wrapped-text height binding is missing or mismatched.");
                    }
                }
            }
        }

        private static Transform FindScenePath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = scene.GetRootGameObjects()
                .Select(root => root.transform)
                .FirstOrDefault(root => root.name == parts[0]);
            for (int index = 1; current != null && index < parts.Length; index++)
            {
                current = current.Find(parts[index]);
            }

            return current;
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private sealed class SectionBinding
        {
            public SectionBinding(string sectionName, string rowsRootName, string rowNamePrefix)
            {
                this.sectionName = sectionName;
                this.rowsRootName = rowsRootName;
                this.rowNamePrefix = rowNamePrefix;
            }

            public string sectionName { get; }
            public string rowsRootName { get; }
            public string rowNamePrefix { get; }
        }

        private sealed class BindingTarget
        {
            public BindingTarget(
                SectionBinding binding,
                Transform body,
                Text bodyText,
                RectTransform rowsRoot,
                ItemDetailAuthoredRowsLayoutElement layoutElement)
            {
                this.binding = binding;
                this.body = body;
                this.bodyText = bodyText;
                this.rowsRoot = rowsRoot;
                this.layoutElement = layoutElement;
            }

            public SectionBinding binding { get; }
            public Transform body { get; }
            public Text bodyText { get; }
            public RectTransform rowsRoot { get; }
            public ItemDetailAuthoredRowsLayoutElement layoutElement { get; }
        }
    }
}
