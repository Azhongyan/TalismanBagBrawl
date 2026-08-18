using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.Items.CampaignBaseline;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalItemArrangementPrefabAuthoring
    {
        public const string BoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab";
        public const string TrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab";
        public const string CardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemCard.prefab";
        public const string TerminalMarker =
            "C1_FORMAL_ITEM_ARRANGEMENT_PREFABS_AUTHORED_PASS";

        private const string I001ArtworkPath =
            "Assets/_Game/Resources/item_daoju/震雷法/I001/I001_1.png";
        private const string I002ArtworkPath =
            "Assets/_Game/Resources/item_daoju/震雷法/I002/I002_1.png";
        private const string I031UnlitArtworkPath =
            "Assets/_Game/Resources/item_daoju/聚念石/聚念石_未激活.png";
        private const string I031LitArtworkPath =
            "Assets/_Game/Resources/item_daoju/聚念石/聚念石_激活.png";

        [MenuItem(
            "TalismanBag/V0.4/Items/Author C1 Formal Item Arrangement Prefabs",
            false,
            2400)]
        public static void AuthorAndValidateBatch()
        {
            string[] targets = { BoardPrefabPath, TrayPrefabPath, CardPrefabPath };
            string existing = targets.FirstOrDefault(File.Exists);
            if (!string.IsNullOrEmpty(existing))
            {
                throw new InvalidOperationException(
                    "C1_FORMAL_ITEM_PREFAB_TARGET_ALREADY_EXISTS " + existing);
            }

            Sprite i001 = RequireSprite(I001ArtworkPath);
            Sprite i002 = RequireSprite(I002ArtworkPath);
            Sprite i031Unlit = RequireSprite(I031UnlitArtworkPath);
            Sprite i031Lit = RequireSprite(I031LitArtworkPath);
            CreateCardPrefab(i001, i002, i031Unlit, i031Lit);
            CreateBoardPrefab();
            CreateTrayPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateAll();
            Debug.Log(TerminalMarker
                      + " / Board=25 cells+3 authored cards"
                      + " / Tray=3 authored cards"
                      + " / Card=final artwork refs+preserveAspect");
        }

        private static void CreateCardPrefab(
            Sprite i001,
            Sprite i002,
            Sprite i031Unlit,
            Sprite i031Lit)
        {
            GameObject root = NewUiObject("C1FormalItemCard");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(120f, 168f);
                Image background = root.AddComponent<Image>();
                background.color = new Color(0.09f, 0.11f, 0.16f, 0.98f);
                background.raycastTarget = true;
                CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
                C1FormalItemCardView view = root.AddComponent<C1FormalItemCardView>();

                GameObject artworkObject = NewUiObject("Artwork", root.transform);
                RectTransform artworkRect = artworkObject.GetComponent<RectTransform>();
                Stretch(artworkRect, new Vector2(8f, 30f), new Vector2(-8f, -8f));
                Image artwork = artworkObject.AddComponent<Image>();
                artwork.sprite = i001;
                artwork.color = Color.white;
                artwork.preserveAspect = true;
                artwork.raycastTarget = false;
                AspectRatioFitter aspect = artworkObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspect.aspectRatio = 1f;

                GameObject indicatorObject = NewUiObject("AuthoritativeLitIndicator", root.transform);
                RectTransform indicatorRect = indicatorObject.GetComponent<RectTransform>();
                indicatorRect.anchorMin = new Vector2(1f, 1f);
                indicatorRect.anchorMax = new Vector2(1f, 1f);
                indicatorRect.pivot = new Vector2(1f, 1f);
                indicatorRect.anchoredPosition = new Vector2(-8f, -8f);
                indicatorRect.sizeDelta = new Vector2(18f, 18f);
                Image indicator = indicatorObject.AddComponent<Image>();
                indicator.color = new Color(0.35f, 1f, 0.55f, 1f);
                indicator.raycastTarget = false;

                GameObject labelObject = NewUiObject("IdentityLabel", root.transform);
                RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0f, 5f);
                labelRect.sizeDelta = new Vector2(-12f, 24f);
                Text label = labelObject.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = 14;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = Color.white;
                label.text = "I001@white";
                label.raycastTarget = false;

                view.AssignForEditor(
                    rootRect,
                    artwork,
                    indicator,
                    label,
                    canvasGroup,
                    aspect,
                    i001,
                    i002,
                    i031Unlit,
                    i031Lit);
                PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateBoardPrefab()
        {
            GameObject cardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (cardAsset == null) throw new InvalidOperationException("CARD_PREFAB_MISSING");
            GameObject root = NewUiObject("C1FormalItemBoard");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(650f, 650f);
                Image background = root.AddComponent<Image>();
                background.color = new Color(0.055f, 0.065f, 0.09f, 0.98f);
                background.raycastTarget = false;
                C1FormalItemBoardView view = root.AddComponent<C1FormalItemBoardView>();

                GameObject gridObject = NewUiObject("AuthoredGrid5x5", root.transform);
                RectTransform gridRect = gridObject.GetComponent<RectTransform>();
                Stretch(gridRect, new Vector2(10f, 10f), new Vector2(-10f, -10f));
                List<Image> cells = new List<Image>(25);
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        GameObject cellObject = NewUiObject(
                            "Cell_" + x + "_" + y,
                            gridObject.transform);
                        RectTransform cellRect = cellObject.GetComponent<RectTransform>();
                        cellRect.anchorMin = new Vector2(x / 5f, y / 5f);
                        cellRect.anchorMax = new Vector2((x + 1) / 5f, (y + 1) / 5f);
                        cellRect.offsetMin = new Vector2(2f, 2f);
                        cellRect.offsetMax = new Vector2(-2f, -2f);
                        Image cellImage = cellObject.AddComponent<Image>();
                        cellImage.color = x == 2 && y == 2
                            ? new Color(0.3f, 0.12f, 0.12f, 0.9f)
                            : new Color(0.18f, 0.2f, 0.25f, 0.9f);
                        cellImage.raycastTarget = false;
                        cells.Add(cellImage);
                    }
                }

                GameObject cardLayer = NewUiObject("AuthoredCardLayer", root.transform);
                RectTransform cardLayerRect = cardLayer.GetComponent<RectTransform>();
                Stretch(cardLayerRect, new Vector2(10f, 10f), new Vector2(-10f, -10f));
                C1FormalItemCardView[] cards = new C1FormalItemCardView[3];
                for (int index = 0; index < cards.Length; index++)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cardAsset);
                    instance.name = "BoardCard_" + index;
                    instance.transform.SetParent(cardLayer.transform, false);
                    RectTransform rect = instance.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(index / 5f, 0f);
                    rect.anchorMax = new Vector2((index + 1) / 5f, 1f / 5f);
                    rect.offsetMin = new Vector2(3f, 3f);
                    rect.offsetMax = new Vector2(-3f, -3f);
                    cards[index] = instance.GetComponent<C1FormalItemCardView>();
                }

                view.AssignForEditor(rootRect, gridRect, cells.ToArray(), cards);
                PrefabUtility.SaveAsPrefabAsset(root, BoardPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateTrayPrefab()
        {
            GameObject cardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            if (cardAsset == null) throw new InvalidOperationException("CARD_PREFAB_MISSING");
            GameObject root = NewUiObject("C1FormalItemTray");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(720f, 230f);
                Image background = root.AddComponent<Image>();
                background.color = new Color(0.065f, 0.075f, 0.105f, 0.98f);
                background.raycastTarget = true;
                C1FormalItemTrayView view = root.AddComponent<C1FormalItemTrayView>();

                GameObject cardLayer = NewUiObject("AuthoredCardSlots", root.transform);
                RectTransform layerRect = cardLayer.GetComponent<RectTransform>();
                Stretch(layerRect, new Vector2(16f, 16f), new Vector2(-16f, -16f));
                C1FormalItemCardView[] cards = new C1FormalItemCardView[3];
                for (int index = 0; index < cards.Length; index++)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cardAsset);
                    instance.name = "TrayCard_" + index;
                    instance.transform.SetParent(cardLayer.transform, false);
                    RectTransform rect = instance.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(index / 3f, 0f);
                    rect.anchorMax = new Vector2((index + 1) / 3f, 1f);
                    rect.offsetMin = new Vector2(8f, 4f);
                    rect.offsetMax = new Vector2(-8f, -4f);
                    cards[index] = instance.GetComponent<C1FormalItemCardView>();
                }

                view.AssignForEditor(rootRect, cards);
                PrefabUtility.SaveAsPrefabAsset(root, TrayPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void ValidateAll()
        {
            ValidatePrefab<C1FormalItemCardView>(CardPrefabPath, 1, view =>
                view.ValidateAuthoredReferences());
            ValidatePrefab<C1FormalItemBoardView>(BoardPrefabPath, 1, view =>
                view.ValidateAuthoredReferences());
            ValidatePrefab<C1FormalItemTrayView>(TrayPrefabPath, 1, view =>
                view.ValidateAuthoredReferences());

            GameObject board = PrefabUtility.LoadPrefabContents(BoardPrefabPath);
            try
            {
                Transform grid = board.transform.Find("AuthoredGrid5x5");
                Require(grid != null && grid.childCount == 25
                        && Enumerable.Range(0, grid.childCount)
                            .All(index => grid.GetChild(index).GetComponent<Image>() != null),
                    "BOARD_CELL_COUNT_INVALID");
                Require(board.GetComponentsInChildren<C1FormalItemCardView>(true)
                        .Length == 3,
                    "BOARD_CARD_REFERENCE_COUNT_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(board);
            }

            GameObject tray = PrefabUtility.LoadPrefabContents(TrayPrefabPath);
            try
            {
                Require(tray.GetComponentsInChildren<C1FormalItemCardView>(true)
                        .Length == 3,
                    "TRAY_CARD_REFERENCE_COUNT_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(tray);
            }

            string[] exact = { BoardPrefabPath, TrayPrefabPath, CardPrefabPath };
            string[] actual = Directory.GetFiles(
                    "Assets/_Game/Prefabs/TalismanBag/Items",
                    "C1FormalItem*.prefab",
                    SearchOption.TopDirectoryOnly)
                .Select(path => path.Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Require(actual.SequenceEqual(exact.OrderBy(path => path,
                    StringComparer.Ordinal), StringComparer.Ordinal),
                "C1_FORMAL_ITEM_PREFAB_SET_NOT_EXACT");
        }

        private static void ValidatePrefab<T>(
            string path,
            int expectedRootCount,
            Func<T, bool> validate)
            where T : Component
        {
            Require(File.Exists(path), "PREFAB_FILE_MISSING " + path);
            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            Require(!dependencies.Any(value => value.EndsWith(
                    ".unity", StringComparison.OrdinalIgnoreCase)),
                "PREFAB_SCENE_DEPENDENCY_FORBIDDEN " + path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "PREFAB_MISSING_SCRIPT " + path);
                T[] views = root.GetComponents<T>();
                Require(views.Length == expectedRootCount,
                    "PREFAB_ROOT_COMPONENT_COUNT_INVALID " + path);
                Require(views.All(validate),
                    "PREFAB_AUTHORED_REFERENCE_INVALID " + path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Sprite RequireSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new InvalidOperationException("FINAL_ITEM_ARTWORK_MISSING " + path);
            return sprite;
        }

        private static GameObject NewUiObject(string name, Transform parent = null)
        {
            GameObject value = new GameObject(name, typeof(RectTransform));
            if (parent != null) value.transform.SetParent(parent, false);
            return value;
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition) throw new InvalidOperationException(diagnostic);
        }
    }
}
