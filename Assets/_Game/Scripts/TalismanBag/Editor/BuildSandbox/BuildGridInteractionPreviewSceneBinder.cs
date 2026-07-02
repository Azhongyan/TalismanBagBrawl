#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildGridInteractionPreviewSceneBinder
    {
        private static readonly Color TraySlotColor = new(0.17f, 0.18f, 0.15f, 1f);
        private static readonly Color CardColor = new(0.27f, 0.22f, 0.15f, 1f);
        private static readonly Color TextColor = new(0.90f, 0.87f, 0.76f, 1f);

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildGridInteractionPreview01/[Writes Scene][Manual Only] Bind Grid Interaction Preview")]
        public static void BindGridInteractionPreviewMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "[Writes Scene][Manual Only] Bind Grid Interaction Preview",
                    "This writes only the V04 preview scene interaction binding.\n\n" +
                    BuildSandboxPreviewSceneMarker.ScenePath + "\n\n" +
                    "It does not modify Build Settings or V02/V03 product scenes.",
                    "Bind",
                    "Cancel"))
            {
                return;
            }

            BindGridInteractionPreviewScene();
        }

        public static string BindGridInteractionPreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException("Grid Interaction Preview binding must run in Edit Mode.");
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                Transform root = RequireRoot("BuildSandboxPreviewRoot");
                Transform canvas = RequireRoot("BuildSandboxPreviewCanvas");
                Transform safeArea = RequireChild(canvas, "SafeAreaRoot");
                Transform battleArea = RequireChild(safeArea, "BattleLikePreviewArea");
                RectTransform board = RequireRect(RequireChild(battleArea, "BoardGridPreview"));
                RectTransform tray = RequireRect(RequireChild(battleArea, "ItemTrayPreview"));
                RectTransform selectedInfo = RequireRect(RequireChild(battleArea, "SelectedItemInfo"));
                RectTransform feedback = RequireRect(RequireChild(battleArea, "PlacementFeedback"));
                RectTransform controlBar = RequireRect(RequireChild(safeArea, "DevOnlyControlBar"));

                LocalizeStaticLabels(battleArea, tray, selectedInfo, feedback, controlBar);
                List<BuildGridPreviewSlotView> boardSlots = BindBoardSlots(board);
                BuildItemTrayPreviewView trayView = BindTray(tray);
                BuildPlacementFeedbackView feedbackView = BindFeedback(feedback);
                Text selectedTitle = EnsureText(selectedInfo, "SelectedItemInfoTitle", "道具信息", 18, TextAnchor.UpperCenter);
                Text selectedBody = EnsureText(selectedInfo, "SelectedItemInfoBody", "单击查看信息；弹窗 Rotate 旋转；松手直接放置。", 15, TextAnchor.MiddleCenter);
                Button rotateButton = EnsureControlButton(controlBar, "RotatePreviewButtonSlot", "Popup Rotate", new Vector2(0.62f, 0.18f), new Vector2(0.73f, 0.82f));
                Button resetButton = EnsureControlButton(controlBar, "ResetPreviewButtonSlot", "取消", new Vector2(0.74f, 0.18f), new Vector2(0.85f, 0.82f));
                EnsureControlButton(controlBar, "RunSimulationButtonSlot", "运行模拟", new Vector2(0.50f, 0.18f), new Vector2(0.61f, 0.82f));
                EnsureControlButton(controlBar, "ExportReportButtonSlot", "导出报告", new Vector2(0.86f, 0.18f), new Vector2(0.97f, 0.82f));
                RectTransform ghostRoot = EnsureDragGhost(canvas, out Text ghostText);

                BuildGridInteractionPreviewController controller =
                    EnsureController(root);
                controller.Bind(
                    board,
                    boardSlots,
                    trayView,
                    feedbackView,
                    selectedTitle,
                    selectedBody,
                    resetButton,
                    rotateButton,
                    ghostRoot,
                    ghostText);

                EditorUtility.SetDirty(controller);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, BuildSandboxPreviewSceneMarker.ScenePath))
                {
                    throw new InvalidOperationException("Could not save " + BuildSandboxPreviewSceneMarker.ScenePath);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[V0.4-BuildGridInteractionPreview01] GRID_INTERACTION_BOUND path=" +
                          BuildSandboxPreviewSceneMarker.ScenePath);
                return BuildSandboxPreviewSceneMarker.ScenePath;
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void LocalizeStaticLabels(
            Transform battleArea,
            RectTransform tray,
            RectTransform selectedInfo,
            RectTransform feedback,
            RectTransform controlBar)
        {
            SetTextIfPresent(battleArea, "Title", "拖拽预览区");
            SetTextIfPresent(battleArea, "BoardGridTitle", "棋盘预览");
            SetTextIfPresent(tray, "ItemTrayTitle", "道具栏");
            SetTextIfPresent(selectedInfo, "SelectedItemInfoTitle", "道具信息");
            SetTextIfPresent(selectedInfo, "SelectedItemInfoBody", "单击查看信息；弹窗 Rotate 旋转；松手直接放置。");
            SetTextIfPresent(feedback, "PlacementFeedbackText", "单击道具查看信息；信息弹窗 Rotate 旋转；合法松手直接放置，非法返回托盘。");
            SetTextIfPresent(controlBar, "Title", "构筑沙盒预览（开发专用）");

            SetAnchors(tray, new Vector2(0.09f, 0.07f), new Vector2(0.66f, 0.37f));
            SetAnchors(selectedInfo, new Vector2(0.69f, 0.07f), new Vector2(0.91f, 0.37f));
            SetAnchors(feedback, new Vector2(0.09f, 0.01f), new Vector2(0.91f, 0.06f));
        }

        private static List<BuildGridPreviewSlotView> BindBoardSlots(RectTransform board)
        {
            List<BuildGridPreviewSlotView> slots = new();
            for (int y = 0; y < BuildGridInteractionPreviewController.BoardRows; y++)
            {
                for (int x = 0; x < BuildGridInteractionPreviewController.BoardColumns; x++)
                {
                    int index = y * BuildGridInteractionPreviewController.BoardColumns + x + 1;
                    Transform cell = RequireChild(board, $"BoardGridCell_{index:00}");
                    Image image = EnsureImage(cell.gameObject, new Color(0.22f, 0.235f, 0.22f, 0.92f), raycast: true);
                    Text label = EnsureChildText(cell, "CellLabel", string.Empty, 16, TextAnchor.MiddleCenter);
                    BuildGridPreviewSlotView view = cell.GetComponent<BuildGridPreviewSlotView>();
                    if (view == null)
                    {
                        view = cell.gameObject.AddComponent<BuildGridPreviewSlotView>();
                    }
                    view.Bind(x, y, image, label);
                    EditorUtility.SetDirty(view);
                    slots.Add(view);
                }
            }

            return slots;
        }

        private static BuildItemTrayPreviewView BindTray(RectTransform tray)
        {
            HideLegacyTraySlots(tray);
            RectTransform categoryRoot = EnsureRectChild(tray, "CategoryTabsRoot");
            SetAnchors(categoryRoot, new Vector2(0.04f, 0.74f), new Vector2(0.96f, 0.88f));

            RectTransform viewport = EnsureRectChild(tray, "ItemTrayViewport");
            SetAnchors(viewport, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.72f));
            Image viewportImage = EnsureImage(viewport.gameObject, new Color(0f, 0f, 0f, 0.04f), raycast: true);
            viewportImage.maskable = true;
            if (viewport.GetComponent<RectMask2D>() == null)
            {
                viewport.gameObject.AddComponent<RectMask2D>();
            }

            RectTransform content = EnsureRectChild(viewport, "ItemTrayContent");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 308f);

            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = content.gameObject.AddComponent<GridLayoutGroup>();
            }
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = BuildGridInteractionPreviewController.TrayColumns;
            grid.cellSize = new Vector2(70f, 34f);
            grid.spacing = new Vector2(8f, 4f);
            grid.padding = new RectOffset(8, 8, 6, 6);
            grid.childAlignment = TextAnchor.UpperCenter;
            RectTransform cardLayer = EnsureItemCardLayer(content);

            ScrollRect scrollRect = tray.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = tray.gameObject.AddComponent<ScrollRect>();
            }
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 18f;

            List<Button> categoryButtons = BuildCategoryButtons(categoryRoot);
            List<Text> categoryLabels = categoryButtons
                .Select(button => button.GetComponentInChildren<Text>(true))
                .Where(label => label != null)
                .ToList();
            BuildTraySlots(
                content,
                out List<RectTransform> traySlotRects,
                out List<Image> traySlotImages,
                out List<Outline> traySlotOutlines);
            List<BuildItemPreviewCardView> cards = BuildItemCards(
                content,
                cardLayer,
                BuildGridInteractionPreviewController.CreatePreviewItems().Count);

            BuildItemTrayPreviewView trayView = tray.GetComponent<BuildItemTrayPreviewView>();
            if (trayView == null)
            {
                trayView = tray.gameObject.AddComponent<BuildItemTrayPreviewView>();
            }
            trayView.Bind(
                scrollRect,
                content,
                cardLayer,
                categoryButtons,
                categoryLabels,
                traySlotRects,
                traySlotImages,
                traySlotOutlines,
                cards);
            EditorUtility.SetDirty(trayView);
            return trayView;
        }

        private static List<Button> BuildCategoryButtons(RectTransform categoryRoot)
        {
            List<Button> buttons = new();
            string[] categories = BuildGridInteractionPreviewController.CategoryLabels;
            for (int i = 0; i < categories.Length; i++)
            {
                float width = 1f / categories.Length;
                RectTransform buttonRect = EnsureRectChild(categoryRoot, $"CategoryButton_{i + 1:00}");
                SetAnchors(buttonRect, new Vector2(i * width + 0.006f, 0.08f), new Vector2((i + 1) * width - 0.006f, 0.92f));
                Image image = EnsureImage(buttonRect.gameObject, new Color(0.18f, 0.18f, 0.14f, 1f), raycast: true);
                Button button = buttonRect.GetComponent<Button>();
                if (button == null)
                {
                    button = buttonRect.gameObject.AddComponent<Button>();
                }
                ColorBlock colors = button.colors;
                colors.normalColor = image.color;
                colors.highlightedColor = new Color(0.36f, 0.29f, 0.15f, 1f);
                colors.pressedColor = new Color(0.25f, 0.20f, 0.12f, 1f);
                colors.selectedColor = colors.highlightedColor;
                button.colors = colors;
                EnsureChildText(buttonRect, "Label", categories[i], 14, TextAnchor.MiddleCenter);
                buttons.Add(button);
            }

            return buttons;
        }

        private static RectTransform EnsureItemCardLayer(RectTransform content)
        {
            RectTransform cardLayer = EnsureRectChild(content, "ItemCardLayer");
            cardLayer.anchorMin = Vector2.zero;
            cardLayer.anchorMax = Vector2.one;
            cardLayer.offsetMin = Vector2.zero;
            cardLayer.offsetMax = Vector2.zero;
            cardLayer.localScale = Vector3.one;
            cardLayer.SetAsLastSibling();

            LayoutElement layoutElement = cardLayer.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = cardLayer.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.ignoreLayout = true;
            EditorUtility.SetDirty(cardLayer);
            return cardLayer;
        }

        private static void BuildTraySlots(
            RectTransform content,
            out List<RectTransform> traySlotRects,
            out List<Image> traySlotImages,
            out List<Outline> traySlotOutlines)
        {
            traySlotRects = new List<RectTransform>();
            traySlotImages = new List<Image>();
            traySlotOutlines = new List<Outline>();
            int slotCount = BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows;
            for (int i = 0; i < slotCount; i++)
            {
                RectTransform slot = EnsureRectChild(content, $"TrayGridSlot_{i + 1:00}");
                Image slotImage = EnsureImage(slot.gameObject, TraySlotColor, raycast: false);
                Outline outline = slot.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = slot.gameObject.AddComponent<Outline>();
                }
                outline.effectColor = new Color(0.42f, 0.36f, 0.18f, 0.75f);
                outline.effectDistance = new Vector2(1f, -1f);
                traySlotRects.Add(slot);
                traySlotImages.Add(slotImage);
                traySlotOutlines.Add(outline);

                Transform legacyCard = slot.Find("ItemCard");
                if (legacyCard != null)
                {
                    UnityEngine.Object.DestroyImmediate(legacyCard.gameObject);
                }
            }
        }

        private static List<BuildItemPreviewCardView> BuildItemCards(
            RectTransform content,
            RectTransform cardLayer,
            int itemCount)
        {
            List<BuildItemPreviewCardView> cards = new();
            for (int i = 0; i < itemCount; i++)
            {
                RectTransform card = EnsureDetachedItemCard(content, cardLayer, i);
                RemoveLegacyItemCardChildren(card);
                Image cardImage = EnsureImage(card.gameObject, CardColor, raycast: true);
                EnsureCardOverlayCanvas(card);
                CanvasGroup group = card.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = card.gameObject.AddComponent<CanvasGroup>();
                }
                group.blocksRaycasts = true;
                group.interactable = true;

                Text title = EnsureChildText(card, "Title", string.Empty, 11, TextAnchor.UpperCenter);
                SetAnchors(title.rectTransform, new Vector2(0.04f, 0.64f), new Vector2(0.97f, 0.98f));
                Text category = EnsureChildText(card, "Category", string.Empty, 10, TextAnchor.MiddleCenter);
                SetAnchors(category.rectTransform, new Vector2(0.04f, 0.40f), new Vector2(0.97f, 0.65f));
                Text shape = EnsureChildText(card, "Shape", string.Empty, 10, TextAnchor.UpperCenter);
                SetAnchors(shape.rectTransform, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.38f));

                BuildItemPreviewCardView cardView = card.GetComponent<BuildItemPreviewCardView>();
                if (cardView == null)
                {
                    cardView = card.gameObject.AddComponent<BuildItemPreviewCardView>();
                }
                cardView.Bind(
                    card,
                    group,
                    cardImage,
                    title,
                    category,
                    shape);
                EditorUtility.SetDirty(cardView);
                cards.Add(cardView);
            }

            RemoveExtraDetachedItemCards(cardLayer, cards);
            cardLayer.SetAsLastSibling();
            return cards;
        }

        private static RectTransform EnsureDetachedItemCard(
            RectTransform content,
            RectTransform cardLayer,
            int index)
        {
            string cardName = $"ItemCard_{index + 1:00}";
            RectTransform card = cardLayer.Find(cardName) as RectTransform;
            Transform legacyCard = content.Find($"TrayGridSlot_{index + 1:00}/ItemCard");
            if (card == null && legacyCard is RectTransform legacyCardRect)
            {
                legacyCardRect.SetParent(cardLayer, worldPositionStays: false);
                legacyCardRect.name = cardName;
                card = legacyCardRect;
            }
            else if (legacyCard != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyCard.gameObject);
            }

            return card == null ? EnsureRectChild(cardLayer, cardName) : card;
        }

        private static void RemoveExtraDetachedItemCards(
            RectTransform cardLayer,
            IReadOnlyCollection<BuildItemPreviewCardView> keepCards)
        {
            foreach (BuildItemPreviewCardView card in cardLayer.GetComponentsInChildren<BuildItemPreviewCardView>(true))
            {
                if (card == null || keepCards.Contains(card))
                {
                    continue;
                }

                UnityEngine.Object.DestroyImmediate(card.gameObject);
            }
        }

        private static void RemoveLegacyItemCardChildren(RectTransform card)
        {
            if (card == null)
            {
                return;
            }

            RemoveLegacyChild(card, "ShapeFootprintRoot");
            RemoveLegacyChild(card, "TrayRotateButton");
        }

        private static void RemoveLegacyChild(RectTransform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void EnsureCardOverlayCanvas(RectTransform card)
        {
            Canvas canvas = card.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = card.gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = 30;

            if (card.GetComponent<GraphicRaycaster>() == null)
            {
                card.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static BuildPlacementFeedbackView BindFeedback(RectTransform feedback)
        {
            Image image = EnsureImage(feedback.gameObject, new Color(0.18f, 0.14f, 0.09f, 1f), raycast: false);
            Text text = EnsureChildText(feedback, "PlacementFeedbackText", "单击道具查看信息；信息弹窗 Rotate 旋转；合法松手直接放置，非法返回托盘。", 16, TextAnchor.MiddleCenter);
            BuildPlacementFeedbackView view = feedback.GetComponent<BuildPlacementFeedbackView>();
            if (view == null)
            {
                view = feedback.gameObject.AddComponent<BuildPlacementFeedbackView>();
            }
            view.Bind(text, image);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static Button EnsureControlButton(
            RectTransform controlBar,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform buttonRect = EnsureRectChild(controlBar, name);
            SetAnchors(buttonRect, anchorMin, anchorMax);
            Image image = EnsureImage(buttonRect.gameObject, new Color(0.46f, 0.36f, 0.18f, 1f), raycast: true);
            Button button = buttonRect.GetComponent<Button>();
            if (button == null)
            {
                button = buttonRect.gameObject.AddComponent<Button>();
            }
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.57f, 0.44f, 0.21f, 1f);
            colors.pressedColor = new Color(0.32f, 0.25f, 0.13f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            EnsureChildText(buttonRect, "Label", label, 16, TextAnchor.MiddleCenter);
            return button;
        }

        private static RectTransform EnsureDragGhost(Transform canvas, out Text ghostText)
        {
            RectTransform ghost = EnsureRectChild(canvas, "GridInteractionDragGhost");
            ghost.sizeDelta = new Vector2(130f, 44f);
            ghost.anchorMin = new Vector2(0.5f, 0.5f);
            ghost.anchorMax = new Vector2(0.5f, 0.5f);
            ghost.pivot = new Vector2(0.5f, 0.5f);
            Image image = EnsureImage(ghost.gameObject, new Color(0.34f, 0.26f, 0.13f, 0.88f), raycast: false);
            image.raycastTarget = false;
            CanvasGroup group = ghost.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = ghost.gameObject.AddComponent<CanvasGroup>();
            }
            group.blocksRaycasts = false;
            group.interactable = false;
            ghostText = EnsureChildText(ghost, "Label", string.Empty, 16, TextAnchor.MiddleCenter);
            ghost.gameObject.SetActive(false);
            return ghost;
        }

        private static BuildGridInteractionPreviewController EnsureController(Transform root)
        {
            Transform runtime = root.Find("BuildGridInteractionPreviewRuntime");
            if (runtime == null)
            {
                GameObject runtimeObject = new("BuildGridInteractionPreviewRuntime", typeof(RectTransform));
                runtimeObject.transform.SetParent(root, false);
                runtime = runtimeObject.transform;
            }

            BuildGridInteractionPreviewController controller =
                runtime.GetComponent<BuildGridInteractionPreviewController>();
            if (controller == null)
            {
                controller = runtime.gameObject.AddComponent<BuildGridInteractionPreviewController>();
            }

            return controller;
        }

        private static void HideLegacyTraySlots(Transform tray)
        {
            foreach (Transform child in tray)
            {
                if (child.name.StartsWith("ItemTraySlot_", StringComparison.Ordinal))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static Transform RequireRoot(string rootName)
        {
            GameObject target = GameObject.Find(rootName);
            if (target == null)
            {
                throw new InvalidOperationException($"Missing root: {rootName}");
            }

            return target.transform;
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = FindDeepChild(parent, childName);
            if (child == null)
            {
                throw new InvalidOperationException($"Missing child: {BuildPath(parent)}/{childName}");
            }

            return child;
        }

        private static RectTransform RequireRect(Transform target)
        {
            RectTransform rect = target as RectTransform;
            if (rect == null)
            {
                throw new InvalidOperationException($"Missing RectTransform: {BuildPath(target)}");
            }

            return rect;
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return RequireRect(existing);
            }

            GameObject child = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            return rect;
        }

        private static Text EnsureText(RectTransform parent, string name, string value, int size, TextAnchor alignment)
        {
            Text text = EnsureChildText(parent, name, value, size, alignment);
            SetAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            return text;
        }

        private static Text EnsureChildText(Transform parent, string name, string value, int size, TextAnchor alignment)
        {
            Transform existing = parent.Find(name);
            GameObject textObject;
            if (existing == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(parent, false);
            }
            else
            {
                textObject = existing.gameObject;
                if (textObject.GetComponent<Text>() == null)
                {
                    textObject.AddComponent<Text>();
                }
            }

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            Text text = textObject.GetComponent<Text>();
            text.text = value ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = TextColor;
            text.raycastTarget = false;
            return text;
        }

        private static Image EnsureImage(GameObject target, Color color, bool raycast)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static void SetTextIfPresent(Transform parent, string name, string value)
        {
            Transform child = FindDeepChild(parent, name);
            Text text = child == null ? null : child.GetComponent<Text>();
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }

        private static void SetAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static string BuildPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
#endif
