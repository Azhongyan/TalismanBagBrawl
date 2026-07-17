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
    /// Explicit, one-time authoring that mirrors the user-adjusted BaseStats row layout onto affix rows.
    /// It has no automatic editor callbacks and never saves the Scene.
    /// </summary>
    internal static class ItemDetailAffixManualRowsAuthoring
    {
        private const string MenuPath =
            "TalismanBag/Item Sandbox/Migrate Fixed And Random Affixes To BaseStats Rows (Once)";
        private const string CleanFieldPrefixesMenuPath =
            "TalismanBag/Item Sandbox/Clean Generic Affix Field Prefixes (Manual Only)";
        private const string DaoTraceMenuPath =
            "TalismanBag/Item Sandbox/Migrate Dao Trace To Fixed Affix Row (Once)";
        private const string CoreEffectMenuPath =
            "TalismanBag/Item Sandbox/Migrate Core Effects To Authored Rows (Once)";
        private const string AffixLineSpacingMenuPath =
            "TalismanBag/Item Sandbox/Set Affix And Dao Trace Line Spacing To 1.2 (Once)";
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PanelPath =
            "ItemSandboxCanvas/MobileSafeAreaRoot/ItemSandboxRoot/ItemDetailPanel";
        private const string DetailContentPath = "ItemDetailScrollView/Viewport/DetailContent";
        private const string BaseStatsSectionName = "BaseStatsSection";
        private const string FixedSectionName = "FixedAffixSection";
        private const string RandomSectionName = "RandomAffixSection";
        private const string BaseRowsRootName = "BaseStatRowsRoot";
        private const string BaseRowNamePrefix = "BaseStatRow_";
        private const string BaseTextNamePrefix = "BaseStatText_";
        private const string BaseIconNamePrefix = "BaseStatIconSlot_";
        private const string FixedRowsRootName = "FixedAffixRowsRoot";
        private const string FixedRowNamePrefix = "FixedAffixRow_";
        private const string FixedTextNamePrefix = "FixedAffixText_";
        private const string FixedIconNamePrefix = "FixedAffixIconSlot_";
        private const string RandomRowsRootName = "RandomAffixRowsRoot";
        private const string RandomRowNamePrefix = "RandomAffixRow_";
        private const string RandomTextNamePrefix = "RandomAffixText_";
        private const string RandomIconNamePrefix = "RandomAffixIconSlot_";
        private const string OrangeSectionName = "OrangeGrowthSection";
        private const string DaoTraceRowsRootName = "DaoTraceRowsRoot";
        private const string DaoTraceRowNamePrefix = "DaoTraceRow_";
        private const string DaoTraceTextNamePrefix = "DaoTraceText_";
        private const string DaoTraceIconNamePrefix = "DaoTraceIconSlot_";
        private const string CoreSectionName = "CoreAwakeningSection";
        private const string CoreEffectRowsRootName = "CoreEffectRowsRoot";
        private const string CoreEffectRowNamePrefix = "CoreEffectRow_";
        private const string CoreEffectTextNamePrefix = "CoreEffectText_";
        private const string CoreEffectIconNamePrefix = "CoreEffectIconSlot_";
        private const int FixedRowCount = 2;
        private const int RandomRowCount = 4;
        private const int CoreEffectRowCount = 4;
        private const float SourceAffixLineSpacing = 1.8f;
        private const float DesiredAffixLineSpacing = 1.2f;

        [MenuItem(MenuPath, false, 2212)]
        public static void MigrateAffixRowsFromBaseStatsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before migrating affix rows.");
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
            Transform baseSection = detailContent.Find(BaseStatsSectionName)
                ?? throw new InvalidOperationException("BaseStatsSection not found.");
            Transform fixedSection = detailContent.Find(FixedSectionName)
                ?? throw new InvalidOperationException("FixedAffixSection not found.");
            Transform randomSection = detailContent.Find(RandomSectionName)
                ?? throw new InvalidOperationException("RandomAffixSection not found.");
            Transform baseBody = baseSection.Find("BodyText")
                ?? throw new InvalidOperationException("BaseStatsSection/BodyText not found.");
            Transform fixedBody = fixedSection.Find("BodyText")
                ?? throw new InvalidOperationException("FixedAffixSection/BodyText not found.");
            Transform randomBody = randomSection.Find("BodyText")
                ?? throw new InvalidOperationException("RandomAffixSection/BodyText not found.");
            Transform baseRowsRoot = baseBody.Find(BaseRowsRootName)
                ?? throw new InvalidOperationException(
                    "BaseStatRowsRoot is required. Finish and save the BaseStats manual row layout first.");

            Transform existingFixedRoot = fixedBody.Find(FixedRowsRootName);
            Transform existingRandomRoot = randomBody.Find(RandomRowsRootName);
            if (existingFixedRoot != null || existingRandomRoot != null)
            {
                if (existingFixedRoot == null || existingRandomRoot == null)
                {
                    throw new InvalidOperationException(
                        "Partial affix row migration detected. No existing layout was overwritten.");
                }

                ValidateRows(
                    existingFixedRoot,
                    FixedRowNamePrefix,
                    FixedTextNamePrefix,
                    FixedIconNamePrefix,
                    FixedRowCount);
                ValidateRows(
                    existingRandomRoot,
                    RandomRowNamePrefix,
                    RandomTextNamePrefix,
                    RandomIconNamePrefix,
                    RandomRowCount);
                ValidateSectionOrder(baseSection, fixedSection, randomSection);
                Debug.Log(
                    "ITEM_DETAIL_AFFIX_MANUAL_ROWS_ALREADY_PASS: existing fixed/random row layouts checked only; "
                    + "no RectTransform, hierarchy, content, or Scene save write was performed.");
                return;
            }

            Text fixedSourceText = fixedBody.GetComponent<Text>()
                ?? throw new InvalidOperationException("FixedAffix BodyText Text component missing.");
            Text randomSourceText = randomBody.GetComponent<Text>()
                ?? throw new InvalidOperationException("RandomAffix BodyText Text component missing.");
            string[] fixedLines = SplitExactLines(fixedSourceText.text, FixedRowCount, FixedSectionName);
            string[] randomLines = SplitExactLines(randomSourceText.text, RandomRowCount, RandomSectionName);
            Transform[] fixedIcons = RequireDirectIcons(fixedBody, FixedIconNamePrefix, FixedRowCount);
            Transform[] randomIcons = RequireDirectIcons(randomBody, RandomIconNamePrefix, RandomRowCount);

            RectTransform panelRect = panel as RectTransform
                ?? throw new InvalidOperationException("ItemDetailPanel RectTransform missing.");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Migrate affixes to authored BaseStats rows");

            CreateAffixRows(
                baseRowsRoot,
                fixedBody,
                fixedSourceText,
                fixedLines,
                fixedIcons,
                FixedRowsRootName,
                FixedRowNamePrefix,
                FixedTextNamePrefix,
                FixedIconNamePrefix);
            CreateAffixRows(
                baseRowsRoot,
                randomBody,
                randomSourceText,
                randomLines,
                randomIcons,
                RandomRowsRootName,
                RandomRowNamePrefix,
                RandomTextNamePrefix,
                RandomIconNamePrefix);

            Undo.RecordObject(detailContent, "Reorder ItemDetail affix sections");
            Undo.RecordObject(fixedSection, "Place fixed affixes after BaseStats");
            Undo.RecordObject(randomSection, "Place random affixes after fixed affixes");
            int baseIndex = baseSection.GetSiblingIndex();
            fixedSection.SetSiblingIndex(baseIndex + 1);
            randomSection.SetSiblingIndex(baseIndex + 2);

            ValidateRows(
                fixedBody.Find(FixedRowsRootName),
                FixedRowNamePrefix,
                FixedTextNamePrefix,
                FixedIconNamePrefix,
                FixedRowCount);
            ValidateRows(
                randomBody.Find(RandomRowsRootName),
                RandomRowNamePrefix,
                RandomTextNamePrefix,
                RandomIconNamePrefix,
                RandomRowCount);
            ValidateSectionOrder(baseSection, fixedSection, randomSection);

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = fixedSection.gameObject;
            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_AFFIX_MANUAL_ROWS_PASS: BaseStats -> FixedAffix -> RandomAffix; "
                + "2 fixed rows and 4 random rows now mirror the authored BaseStats row/text/icon RectTransforms; "
                + "original affix icon objects were preserved; Scene remains unsaved for visual review.");
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

        [MenuItem(DaoTraceMenuPath, false, 2213)]
        public static void MigrateDaoTraceToFixedAffixRowOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before migrating the Dao Trace row.");
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
            Transform fixedSection = detailContent.Find(FixedSectionName)
                ?? throw new InvalidOperationException("FixedAffixSection not found.");
            Transform orangeSection = detailContent.Find(OrangeSectionName)
                ?? throw new InvalidOperationException("OrangeGrowthSection not found.");
            Transform fixedBody = fixedSection.Find("BodyText")
                ?? throw new InvalidOperationException("FixedAffixSection/BodyText not found.");
            Transform orangeBody = orangeSection.Find("BodyText")
                ?? throw new InvalidOperationException("OrangeGrowthSection/BodyText not found.");
            Transform fixedRowsRoot = fixedBody.Find(FixedRowsRootName)
                ?? throw new InvalidOperationException("FixedAffixRowsRoot is required as the authored layout source.");

            Transform existingDaoTraceRoot = orangeBody.Find(DaoTraceRowsRootName);
            if (existingDaoTraceRoot != null)
            {
                ValidateRows(
                    existingDaoTraceRoot,
                    DaoTraceRowNamePrefix,
                    DaoTraceTextNamePrefix,
                    DaoTraceIconNamePrefix,
                    1);
                if (existingDaoTraceRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>() == null
                    || orangeBody.GetComponent<ItemDetailAuthoredRowsLayoutElement>() == null
                    || existingDaoTraceRoot.Find(DaoTraceRowNamePrefix + "0")
                        ?.GetComponent<ItemDetailAuthoredTextRowLayoutElement>() == null)
                {
                    throw new InvalidOperationException(
                        "DaoTraceRowsRoot exists but its adaptive row components are incomplete. No layout was overwritten.");
                }

                Debug.Log(
                    "ITEM_DETAIL_DAO_TRACE_ROW_ALREADY_PASS: existing Dao Trace row checked only; "
                    + "no RectTransform, hierarchy, content, sprite, or Scene save write was performed.");
                return;
            }

            Transform fixedRow = fixedRowsRoot.Find(FixedRowNamePrefix + "0")
                ?? throw new InvalidOperationException("FixedAffixRow_0 not found.");
            RectTransform fixedTextRect = fixedRow.Find(FixedTextNamePrefix + "0") as RectTransform
                ?? throw new InvalidOperationException("FixedAffixText_0 not found.");
            RectTransform fixedIconRect = fixedRow.Find(FixedIconNamePrefix + "0") as RectTransform
                ?? throw new InvalidOperationException("FixedAffixIconSlot_0 not found.");
            Text orangeSourceText = orangeBody.GetComponent<Text>()
                ?? throw new InvalidOperationException("OrangeGrowthSection/BodyText Text component missing.");
            string sourceLine = orangeSourceText.text ?? string.Empty;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Migrate Dao Trace to authored fixed-affix row");

            GameObject rootObject = new GameObject(
                DaoTraceRowsRootName,
                typeof(RectTransform),
                typeof(ItemDetailAuthoredRowsVerticalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(rootObject, "Create " + DaoTraceRowsRootName);
            RectTransform daoTraceRowsRoot = rootObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(daoTraceRowsRoot, orangeBody, "Parent " + DaoTraceRowsRootName);
            RectSnapshot.Capture((RectTransform)fixedRowsRoot).Apply(daoTraceRowsRoot);
            ItemDetailAuthoredRowsVerticalLayoutGroup fixedRowsLayout =
                fixedRowsRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>();
            rootObject.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>().ConfigureEditor(
                fixedRowsLayout != null ? fixedRowsLayout.Spacing : 6f,
                fixedRowsLayout == null || fixedRowsLayout.UseChildScale);

            GameObject rowObject = new GameObject(
                DaoTraceRowNamePrefix + "0",
                typeof(RectTransform),
                typeof(ItemDetailAuthoredTextRowLayoutElement));
            Undo.RegisterCreatedObjectUndo(rowObject, "Create authored Dao Trace row");
            RectTransform daoTraceRow = rowObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(daoTraceRow, daoTraceRowsRoot, "Parent authored Dao Trace row");
            RectSnapshot.Capture((RectTransform)fixedRow).Apply(daoTraceRow);

            GameObject textObject = new GameObject(
                DaoTraceTextNamePrefix + "0",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            Undo.RegisterCreatedObjectUndo(textObject, "Create authored Dao Trace text");
            RectTransform daoTraceTextRect = textObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(daoTraceTextRect, daoTraceRow, "Parent authored Dao Trace text");
            RectSnapshot.Capture(fixedTextRect).Apply(daoTraceTextRect);
            Text daoTraceText = textObject.GetComponent<Text>();
            CopyTextStyle(orangeSourceText, daoTraceText);
            daoTraceText.text = sourceLine.TrimStart();

            Transform directIcon = orangeBody.Find(DaoTraceIconNamePrefix + "0");
            RectTransform daoTraceIconRect;
            Image daoTraceIcon;
            if (directIcon != null)
            {
                daoTraceIconRect = directIcon as RectTransform
                    ?? throw new InvalidOperationException("Existing DaoTraceIconSlot_0 is not a RectTransform.");
                daoTraceIcon = directIcon.GetComponent<Image>()
                    ?? throw new InvalidOperationException("Existing DaoTraceIconSlot_0 Image component missing.");
                Undo.SetTransformParent(directIcon, daoTraceRow, "Move existing Dao Trace icon into authored row");
            }
            else
            {
                GameObject iconObject = new GameObject(
                    DaoTraceIconNamePrefix + "0",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                Undo.RegisterCreatedObjectUndo(iconObject, "Create Dao Trace artwork placeholder");
                daoTraceIconRect = iconObject.GetComponent<RectTransform>();
                daoTraceIcon = iconObject.GetComponent<Image>();
                Undo.SetTransformParent(daoTraceIconRect, daoTraceRow, "Parent Dao Trace artwork placeholder");
                daoTraceIcon.color = new Color32(92, 88, 82, 255);
                daoTraceIcon.sprite = null;
                daoTraceIcon.preserveAspect = true;
                daoTraceIcon.raycastTarget = false;
            }

            RectSnapshot.Capture(fixedIconRect).Apply(daoTraceIconRect);
            daoTraceIconRect.name = DaoTraceIconNamePrefix + "0";
            daoTraceIcon.enabled = true;
            daoTraceIcon.gameObject.SetActive(true);
            EditorUtility.SetDirty(daoTraceIcon);

            ItemDetailAuthoredTextRowLayoutElement fixedRowLayout =
                fixedRow.GetComponent<ItemDetailAuthoredTextRowLayoutElement>();
            rowObject.GetComponent<ItemDetailAuthoredTextRowLayoutElement>().ConfigureEditor(
                daoTraceText,
                fixedRowLayout != null ? fixedRowLayout.MinimumHeight : 0f,
                0f);

            ItemDetailAuthoredRowsLayoutElement bodyLayout =
                orangeBody.GetComponent<ItemDetailAuthoredRowsLayoutElement>()
                ?? Undo.AddComponent<ItemDetailAuthoredRowsLayoutElement>(orangeBody.gameObject);
            bodyLayout.ConfigureEditor(daoTraceRowsRoot, DaoTraceRowNamePrefix);
            bodyLayout.MarkAffixDynamicRowsEditor();

            Undo.RecordObject(orangeSourceText, "Replace Dao Trace text with transparent layout scaffold");
            orangeSourceText.text = BuildTransparentLayoutScaffold(new[] { sourceLine });
            EditorUtility.SetDirty(orangeSourceText);

            ValidateRows(
                daoTraceRowsRoot,
                DaoTraceRowNamePrefix,
                DaoTraceTextNamePrefix,
                DaoTraceIconNamePrefix,
                1);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = orangeSection.gameObject;
            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_DAO_TRACE_ROW_PASS: OrangeGrowthSection now mirrors FixedAffixRow_0; "
                + "DaoTraceIconSlot_0 keeps the same authored RectTransform and has no assigned Sprite; "
                + "Scene remains unsaved for visual review.");
        }

        [MenuItem(DaoTraceMenuPath, true)]
        private static bool ValidateDaoTraceMenu()
        {
            return ValidateMenu();
        }

        [MenuItem(CoreEffectMenuPath, false, 2214)]
        public static void MigrateCoreEffectsToAuthoredRowsOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before migrating Core Effect rows.");
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
            Transform fixedSection = detailContent.Find(FixedSectionName)
                ?? throw new InvalidOperationException("FixedAffixSection not found.");
            Transform coreSection = detailContent.Find(CoreSectionName)
                ?? throw new InvalidOperationException("CoreAwakeningSection not found.");
            Transform fixedBody = fixedSection.Find("BodyText")
                ?? throw new InvalidOperationException("FixedAffixSection/BodyText not found.");
            Transform coreBody = coreSection.Find("BodyText")
                ?? throw new InvalidOperationException("CoreAwakeningSection/BodyText not found.");
            Transform fixedRowsRoot = fixedBody.Find(FixedRowsRootName)
                ?? throw new InvalidOperationException("FixedAffixRowsRoot is required as the authored layout source.");

            Transform existingCoreRoot = coreBody.Find(CoreEffectRowsRootName);
            if (existingCoreRoot != null)
            {
                ValidateRows(
                    existingCoreRoot,
                    CoreEffectRowNamePrefix,
                    CoreEffectTextNamePrefix,
                    CoreEffectIconNamePrefix,
                    CoreEffectRowCount);
                if (existingCoreRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>() == null
                    || coreBody.GetComponent<ItemDetailAuthoredRowsLayoutElement>() == null
                    || Enumerable.Range(0, CoreEffectRowCount).Any(index =>
                        existingCoreRoot.Find(CoreEffectRowNamePrefix + index)
                            ?.GetComponent<ItemDetailAuthoredTextRowLayoutElement>() == null))
                {
                    throw new InvalidOperationException(
                        "CoreEffectRowsRoot exists but its adaptive row components are incomplete. No layout was overwritten.");
                }

                Debug.Log(
                    "ITEM_DETAIL_CORE_EFFECT_ROWS_ALREADY_PASS: existing Core Effect rows checked only; "
                    + "no RectTransform, hierarchy, text, sprite, icon size, or Scene save write was performed.");
                return;
            }

            Transform fixedRow = fixedRowsRoot.Find(FixedRowNamePrefix + "0")
                ?? throw new InvalidOperationException("FixedAffixRow_0 not found.");
            RectTransform fixedTextRect = fixedRow.Find(FixedTextNamePrefix + "0") as RectTransform
                ?? throw new InvalidOperationException("FixedAffixText_0 not found.");
            Text coreSourceText = coreBody.GetComponent<Text>()
                ?? throw new InvalidOperationException("CoreAwakeningSection/BodyText Text component missing.");
            string[] sourceLines = SplitNonEmptyLinesExact(
                coreSourceText.text,
                CoreEffectRowCount,
                CoreSectionName);
            Transform[] coreIcons = RequireDirectIcons(
                coreBody,
                CoreEffectIconNamePrefix,
                CoreEffectRowCount);

            RectTransform panelRect = panel as RectTransform
                ?? throw new InvalidOperationException("ItemDetailPanel RectTransform missing.");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();

            RectSnapshot[] iconSnapshots = coreIcons
                .Select(icon => RectSnapshot.Capture(icon as RectTransform
                    ?? throw new InvalidOperationException(icon.name + " is not a RectTransform.")))
                .ToArray();
            float[] iconHeights = coreIcons
                .Select(icon => Mathf.Abs(((RectTransform)icon).rect.height))
                .ToArray();

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Migrate Core Effects to authored rows");

            VerticalLayoutGroup legacyIconLayout = coreBody.GetComponent<VerticalLayoutGroup>();
            if (legacyIconLayout != null && legacyIconLayout.enabled)
            {
                Undo.RecordObject(legacyIconLayout, "Disable legacy Core Effect icon layout");
                legacyIconLayout.enabled = false;
                EditorUtility.SetDirty(legacyIconLayout);
            }

            GameObject rootObject = new GameObject(
                CoreEffectRowsRootName,
                typeof(RectTransform),
                typeof(ItemDetailAuthoredRowsVerticalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(rootObject, "Create " + CoreEffectRowsRootName);
            RectTransform coreRowsRoot = rootObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(coreRowsRoot, coreBody, "Parent " + CoreEffectRowsRootName);
            RectSnapshot.Capture((RectTransform)fixedRowsRoot).Apply(coreRowsRoot);
            ItemDetailAuthoredRowsVerticalLayoutGroup fixedRowsLayout =
                fixedRowsRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>();
            rootObject.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>().ConfigureEditor(
                fixedRowsLayout != null ? fixedRowsLayout.Spacing : 6f,
                false);

            ItemDetailAuthoredTextRowLayoutElement fixedRowLayout =
                fixedRow.GetComponent<ItemDetailAuthoredTextRowLayoutElement>();
            float fixedMinimumHeight = fixedRowLayout != null ? fixedRowLayout.MinimumHeight : 0f;
            for (int index = 0; index < CoreEffectRowCount; index++)
            {
                GameObject rowObject = new GameObject(
                    CoreEffectRowNamePrefix + index,
                    typeof(RectTransform),
                    typeof(ItemDetailAuthoredTextRowLayoutElement));
                Undo.RegisterCreatedObjectUndo(rowObject, "Create authored Core Effect row");
                RectTransform row = rowObject.GetComponent<RectTransform>();
                Undo.SetTransformParent(row, coreRowsRoot, "Parent authored Core Effect row");
                RectSnapshot.Capture((RectTransform)fixedRow).Apply(row);
                row.localScale = Vector3.one;
                EditorUtility.SetDirty(row);

                GameObject textObject = new GameObject(
                    CoreEffectTextNamePrefix + index,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                Undo.RegisterCreatedObjectUndo(textObject, "Create authored Core Effect text");
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                Undo.SetTransformParent(textRect, row, "Parent authored Core Effect text");
                RectSnapshot.Capture(fixedTextRect).Apply(textRect);
                Text rowText = textObject.GetComponent<Text>();
                CopyTextStyle(coreSourceText, rowText);
                rowText.text = sourceLines[index].TrimStart();

                Undo.SetTransformParent(
                    coreIcons[index],
                    row,
                    "Move original Core Effect icon into authored row");
                iconSnapshots[index].Apply((RectTransform)coreIcons[index]);
                coreIcons[index].name = CoreEffectIconNamePrefix + index;
                coreIcons[index].gameObject.SetActive(true);
                EditorUtility.SetDirty(coreIcons[index]);

                rowObject.GetComponent<ItemDetailAuthoredTextRowLayoutElement>().ConfigureEditor(
                    rowText,
                    Mathf.Max(fixedMinimumHeight, iconHeights[index]),
                    0f);
            }

            ItemDetailAuthoredRowsLayoutElement bodyLayout =
                coreBody.GetComponent<ItemDetailAuthoredRowsLayoutElement>()
                ?? Undo.AddComponent<ItemDetailAuthoredRowsLayoutElement>(coreBody.gameObject);
            bodyLayout.ConfigureEditor(coreRowsRoot, CoreEffectRowNamePrefix);
            bodyLayout.MarkAffixDynamicRowsEditor();

            Undo.RecordObject(coreSourceText, "Replace Core Effect text with transparent layout scaffold");
            coreSourceText.text = BuildTransparentLayoutScaffold(sourceLines);
            EditorUtility.SetDirty(coreSourceText);

            ValidateRows(
                coreRowsRoot,
                CoreEffectRowNamePrefix,
                CoreEffectTextNamePrefix,
                CoreEffectIconNamePrefix,
                CoreEffectRowCount);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = coreSection.gameObject;
            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_CORE_EFFECT_ROWS_PASS: CoreAwakeningSection now uses 4 authored rows; "
                + "the four original CoreEffectIconSlot objects, sprites, RectTransform sizes, and local geometry were preserved; "
                + "Scene remains unsaved for visual review.");
        }

        [MenuItem(CoreEffectMenuPath, true)]
        private static bool ValidateCoreEffectMenu()
        {
            return ValidateMenu();
        }

        [MenuItem(AffixLineSpacingMenuPath, false, 2215)]
        public static void SetAffixAndDaoTraceLineSpacingOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before changing authored Text line spacing.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException("Open the ItemSandbox Scene first.");
            }

            Text[] texts = ResolveAffixLineSpacingTexts(scene);
            if (texts.Any(text => !Mathf.Approximately(text.lineSpacing, SourceAffixLineSpacing)))
            {
                throw new InvalidOperationException(
                    "Line-spacing migration requires all 7 authored Text objects to still be exactly 1.8. "
                    + "No value was overwritten.");
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set authored affix line spacing to 1.2");
            foreach (Text text in texts)
            {
                Undo.RecordObject(text, "Set authored affix line spacing to 1.2");
                text.lineSpacing = DesiredAffixLineSpacing;
                EditorUtility.SetDirty(text);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Canvas.ForceUpdateCanvases();
            Debug.Log(
                "ITEM_DETAIL_AFFIX_LINE_SPACING_PASS: FixedAffixText_0..1, RandomAffixText_0..3, "
                + "and DaoTraceText_0 changed from 1.8 to 1.2; Scene remains unsaved for visual review; "
                + "the menu is now disabled and will not overwrite future manual tuning.");
        }

        [MenuItem(AffixLineSpacingMenuPath, true)]
        private static bool ValidateAffixLineSpacingMenu()
        {
            if (!ValidateMenu())
            {
                return false;
            }

            try
            {
                return ResolveAffixLineSpacingTexts(SceneManager.GetActiveScene())
                    .All(text => Mathf.Approximately(text.lineSpacing, SourceAffixLineSpacing));
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        [MenuItem(CleanFieldPrefixesMenuPath, false, 2216)]
        public static void CleanGenericAffixFieldPrefixesOnly()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Exit Play Mode before cleaning affix preview fields.");
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || Normalize(scene.path) != ScenePath)
            {
                throw new InvalidOperationException("Open the ItemSandbox Scene first.");
            }

            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform fixedRoot = panel.Find(
                DetailContentPath + "/" + FixedSectionName + "/BodyText/" + FixedRowsRootName)
                ?? throw new InvalidOperationException("FixedAffixRowsRoot not found.");
            Transform randomRoot = panel.Find(
                DetailContentPath + "/" + RandomSectionName + "/BodyText/" + RandomRowsRootName)
                ?? throw new InvalidOperationException("RandomAffixRowsRoot not found.");

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Clean generic affix field prefixes");
            int changed = 0;
            changed += CleanRowTexts(fixedRoot, FixedRowNamePrefix, FixedTextNamePrefix, FixedRowCount);
            changed += CleanRowTexts(randomRoot, RandomRowNamePrefix, RandomTextNamePrefix, RandomRowCount);
            Undo.CollapseUndoOperations(undoGroup);

            if (changed > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                Canvas.ForceUpdateCanvases();
            }

            Debug.Log(
                "ITEM_DETAIL_AFFIX_FIELD_PREFIX_CLEAN_PASS: changed=" + changed
                + "; only FixedAffixText_0..1 and RandomAffixText_0..3 content was inspected; "
                + "no RectTransform, icon, hierarchy, or Scene save write was performed.");
        }

        [MenuItem(CleanFieldPrefixesMenuPath, true)]
        private static bool ValidateCleanFieldPrefixesMenu()
        {
            return ValidateMenu();
        }

        private static int CleanRowTexts(
            Transform rowsRoot,
            string rowNamePrefix,
            string textNamePrefix,
            int rowCount)
        {
            int changed = 0;
            for (int index = 0; index < rowCount; index++)
            {
                Text text = rowsRoot.Find(rowNamePrefix + index + "/" + textNamePrefix + index)
                    ?.GetComponent<Text>()
                    ?? throw new InvalidOperationException("Missing authored affix row text: " + textNamePrefix + index);
                string sanitized = RemoveGenericAffixPreviewPrefix(text.text);
                if (string.Equals(text.text, sanitized, StringComparison.Ordinal))
                {
                    continue;
                }

                Undo.RecordObject(text, "Clean generic affix field prefix");
                text.text = sanitized;
                EditorUtility.SetDirty(text);
                changed++;
            }

            return changed;
        }

        private static Text[] ResolveAffixLineSpacingTexts(Scene scene)
        {
            Transform panel = FindScenePath(scene, PanelPath)
                ?? throw new InvalidOperationException("ItemDetailPanel Scene root not found: " + PanelPath);
            Transform fixedRoot = panel.Find(
                DetailContentPath + "/" + FixedSectionName + "/BodyText/" + FixedRowsRootName)
                ?? throw new InvalidOperationException("FixedAffixRowsRoot not found.");
            Transform randomRoot = panel.Find(
                DetailContentPath + "/" + RandomSectionName + "/BodyText/" + RandomRowsRootName)
                ?? throw new InvalidOperationException("RandomAffixRowsRoot not found.");
            Transform daoTraceRoot = panel.Find(
                DetailContentPath + "/" + OrangeSectionName + "/BodyText/" + DaoTraceRowsRootName)
                ?? throw new InvalidOperationException("DaoTraceRowsRoot not found.");

            return Enumerable.Range(0, FixedRowCount)
                .Select(index => RequireRowText(
                    fixedRoot,
                    FixedRowNamePrefix,
                    FixedTextNamePrefix,
                    index))
                .Concat(Enumerable.Range(0, RandomRowCount).Select(index => RequireRowText(
                    randomRoot,
                    RandomRowNamePrefix,
                    RandomTextNamePrefix,
                    index)))
                .Concat(new[]
                {
                    RequireRowText(
                        daoTraceRoot,
                        DaoTraceRowNamePrefix,
                        DaoTraceTextNamePrefix,
                        0)
                })
                .ToArray();
        }

        private static Text RequireRowText(
            Transform rowsRoot,
            string rowNamePrefix,
            string textNamePrefix,
            int index)
        {
            return rowsRoot.Find(rowNamePrefix + index + "/" + textNamePrefix + index)
                ?.GetComponent<Text>()
                ?? throw new InvalidOperationException("Missing authored row Text: " + textNamePrefix + index);
        }

        private static void CreateAffixRows(
            Transform baseRowsRoot,
            Transform targetBody,
            Text targetSourceText,
            string[] sourceLines,
            Transform[] targetIcons,
            string rowsRootName,
            string rowNamePrefix,
            string textNamePrefix,
            string iconNamePrefix)
        {
            VerticalLayoutGroup legacyIconLayout = targetBody.GetComponent<VerticalLayoutGroup>();
            if (legacyIconLayout != null && legacyIconLayout.enabled)
            {
                Undo.RecordObject(legacyIconLayout, "Disable legacy affix icon layout");
                legacyIconLayout.enabled = false;
                EditorUtility.SetDirty(legacyIconLayout);
            }

            GameObject rootObject = new GameObject(rowsRootName, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(rootObject, "Create " + rowsRootName);
            RectTransform rowsRoot = rootObject.GetComponent<RectTransform>();
            Undo.SetTransformParent(rowsRoot, targetBody, "Parent " + rowsRootName);
            RectSnapshot.Capture((RectTransform)baseRowsRoot).Apply(rowsRoot);

            for (int index = 0; index < sourceLines.Length; index++)
            {
                Transform baseRow = baseRowsRoot.Find(BaseRowNamePrefix + index)
                    ?? throw new InvalidOperationException("Missing BaseStats row template: " + index);
                RectTransform baseText = baseRow.Find(BaseTextNamePrefix + index) as RectTransform
                    ?? throw new InvalidOperationException("Missing BaseStats text template: " + index);
                RectTransform baseIcon = baseRow.Find(BaseIconNamePrefix + index) as RectTransform
                    ?? throw new InvalidOperationException("Missing BaseStats icon template: " + index);

                GameObject rowObject = new GameObject(rowNamePrefix + index, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(rowObject, "Create authored affix row");
                RectTransform row = rowObject.GetComponent<RectTransform>();
                Undo.SetTransformParent(row, rowsRoot, "Parent authored affix row");
                RectSnapshot.Capture((RectTransform)baseRow).Apply(row);

                GameObject textObject = new GameObject(
                    textNamePrefix + index,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                Undo.RegisterCreatedObjectUndo(textObject, "Create authored affix row text");
                RectTransform textRect = textObject.GetComponent<RectTransform>();
                Undo.SetTransformParent(textRect, row, "Parent authored affix row text");
                RectSnapshot.Capture(baseText).Apply(textRect);
                Text rowText = textObject.GetComponent<Text>();
                CopyTextStyle(targetSourceText, rowText);
                rowText.text = RemoveGenericAffixPreviewPrefix(sourceLines[index]);

                Undo.SetTransformParent(targetIcons[index], row, "Move original affix icon into authored row");
                RectSnapshot.Capture(baseIcon).Apply((RectTransform)targetIcons[index]);
                targetIcons[index].name = iconNamePrefix + index;
                targetIcons[index].gameObject.SetActive(true);
                EditorUtility.SetDirty(targetIcons[index]);
            }

            Undo.RecordObject(targetSourceText, "Replace legacy affix text with transparent layout scaffold");
            targetSourceText.text = BuildTransparentLayoutScaffold(sourceLines);
            EditorUtility.SetDirty(targetSourceText);
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

        private static string[] SplitExactLines(string value, int expected, string sectionName)
        {
            string[] lines = (value ?? string.Empty).Replace("\r", string.Empty).Split('\n');
            if (lines.Length != expected)
            {
                throw new InvalidOperationException(
                    sectionName + " expected exactly " + expected + " authored lines, found " + lines.Length
                    + ". No migration was performed.");
            }

            return lines;
        }

        private static string[] SplitNonEmptyLinesExact(string value, int expected, string sectionName)
        {
            string[] lines = (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(StripRichTextTags(line)))
                .ToArray();
            if (lines.Length != expected)
            {
                throw new InvalidOperationException(
                    sectionName + " expected exactly " + expected + " non-empty authored lines, found "
                    + lines.Length + ". No migration was performed.");
            }

            return lines;
        }

        private static Transform[] RequireDirectIcons(Transform body, string prefix, int count)
        {
            return Enumerable.Range(0, count)
                .Select(index => body.Find(prefix + index)
                    ?? throw new InvalidOperationException("Missing original slot: " + prefix + index))
                .ToArray();
        }

        private static string BuildTransparentLayoutScaffold(string[] lines)
        {
            return string.Join(
                "\n",
                lines.Select(line => "<color=#00000000>" + StripRichTextTags(line) + "</color>"));
        }

        private static string RemoveGenericAffixPreviewPrefix(string value)
        {
            string plain = StripRichTextTags(value).TrimStart();
            if (!plain.StartsWith("固定词条", StringComparison.Ordinal)
                && !plain.StartsWith("随机词条", StringComparison.Ordinal))
            {
                return (value ?? string.Empty).TrimStart();
            }

            int colonIndex = plain.IndexOf('：');
            if (colonIndex < 0)
            {
                colonIndex = plain.IndexOf(':');
            }

            return colonIndex >= 0 && colonIndex + 1 < plain.Length
                ? plain.Substring(colonIndex + 1).TrimStart()
                : plain;
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

        private static void ValidateRows(
            Transform rowsRoot,
            string rowNamePrefix,
            string textNamePrefix,
            string iconNamePrefix,
            int rowCount)
        {
            if (rowsRoot == null)
            {
                throw new InvalidOperationException("Authored affix rows root missing.");
            }

            if (rowsRoot.GetComponentsInChildren<LayoutGroup>(true).Any(group => group.enabled))
            {
                throw new InvalidOperationException(
                    rowsRoot.name + " contains an enabled LayoutGroup; manual RectTransforms must remain authoritative.");
            }

            for (int index = 0; index < rowCount; index++)
            {
                Transform row = rowsRoot.Find(rowNamePrefix + index)
                    ?? throw new InvalidOperationException("Missing authored row: " + rowNamePrefix + index);
                if (row.Find(textNamePrefix + index)?.GetComponent<Text>() == null)
                {
                    throw new InvalidOperationException("Missing authored row Text: " + textNamePrefix + index);
                }

                if (row.Find(iconNamePrefix + index)?.GetComponent<Image>() == null)
                {
                    throw new InvalidOperationException("Missing preserved icon slot: " + iconNamePrefix + index);
                }
            }
        }

        private static void ValidateSectionOrder(
            Transform baseSection,
            Transform fixedSection,
            Transform randomSection)
        {
            int baseIndex = baseSection.GetSiblingIndex();
            if (fixedSection.GetSiblingIndex() != baseIndex + 1
                || randomSection.GetSiblingIndex() != baseIndex + 2)
            {
                throw new InvalidOperationException(
                    "Required section order is BaseStatsSection -> FixedAffixSection -> RandomAffixSection.");
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
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Quaternion localRotation;
            private readonly Vector3 localScale;

            private RectSnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                localRotation = rect.localRotation;
                localScale = rect.localScale;
            }

            public static RectSnapshot Capture(RectTransform rect)
            {
                return new RectSnapshot(rect);
            }

            public void Apply(RectTransform rect)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = pivot;
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = sizeDelta;
                rect.localRotation = localRotation;
                rect.localScale = localScale;
                EditorUtility.SetDirty(rect);
            }
        }
    }
}
