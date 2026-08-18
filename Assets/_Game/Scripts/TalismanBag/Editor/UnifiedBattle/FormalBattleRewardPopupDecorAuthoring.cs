using System;
using System.IO;
using TMPro;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.Reward;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.UnifiedBattle
{
    public static class FormalBattleRewardPopupDecorAuthoring
    {
        private const string UnifiedPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string RewardPopupPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/FormalBattleRewardPopup.prefab";
        private const string FontPath =
            "Assets/_Game/Fonts/SourceHanSansSC-Heavy SDF.asset";
        private static readonly string[] BackgroundPaths =
        {
            "Assets/_Game/Resources/item/弹窗背景/background-凡品.png",
            "Assets/_Game/Resources/item/弹窗背景/background-良品.png",
            "Assets/_Game/Resources/item/弹窗背景/background-灵品.png",
            "Assets/_Game/Resources/item/弹窗背景/background-玄品.png",
            "Assets/_Game/Resources/item/弹窗背景/background-道品.png"
        };
        private const string SuccessMarker =
            "FORMAL_BATTLE_REWARD_POPUP_AND_ARRANGE_VISUAL_PASS";

        [MenuItem(
            "TalismanBag/V0.4/Decor/Author Reward Popup And Arrange Visual")]
        public static void AuthorFromMenu()
        {
            Apply();
            Debug.Log(SuccessMarker);
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                Apply();
                Debug.Log(SuccessMarker);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Apply()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                FontPath);
            Require(font != null, "FORMAL_REWARD_POPUP_FONT_MISSING");
            Sprite[] backgrounds = new Sprite[BackgroundPaths.Length];
            for (int index = 0; index < BackgroundPaths.Length; index++)
            {
                backgrounds[index] = AssetDatabase.LoadAssetAtPath<Sprite>(
                    BackgroundPaths[index]);
                Require(backgrounds[index] != null,
                    "FORMAL_REWARD_POPUP_BACKGROUND_MISSING "
                    + BackgroundPaths[index]);
            }

            EnsureFolder(Path.GetDirectoryName(RewardPopupPrefabPath)
                ?.Replace('\\', '/'));
            AuthorPopupPrefab(font, backgrounds);
            MountPopupIntoUnifiedShell();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                RewardPopupPrefabPath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(
                UnifiedPrefabPath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);
            ValidatePersistedResult();
        }

        private static void AuthorPopupPrefab(
            TMP_FontAsset font,
            Sprite[] backgrounds)
        {
            GameObject root = NewUiObject("FormalBattleRewardPopup", null);
            try
            {
                Stretch((RectTransform)root.transform);
                CanvasGroup canvasGroup = root.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                FormalBattleRewardPopupView view =
                    root.AddComponent<FormalBattleRewardPopupView>();

                Image backdrop = CreateImage(
                    "ModalBackdrop",
                    root.transform,
                    new Color(0.025f, 0.02f, 0.015f, 0.72f));
                backdrop.raycastTarget = true;
                Stretch((RectTransform)backdrop.transform);

                RectTransform panel = (RectTransform)NewUiObject(
                    "RewardPanel",
                    root.transform).transform;
                Pin(panel, Vector2.one * 0.5f, new Vector2(0f, 10f),
                    new Vector2(520f, 760f));

                Image panelBackground = CreateImage(
                    "PanelBackground",
                    panel,
                    Color.white);
                panelBackground.sprite = backgrounds[0];
                panelBackground.preserveAspect = false;
                panelBackground.raycastTarget = true;
                Stretch((RectTransform)panelBackground.transform);

                Image rarityAccent = CreateImage(
                    "RarityAccent",
                    panel,
                    new Color(0.82f, 0.75f, 0.61f, 0.98f));
                Pin((RectTransform)rarityAccent.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 245f),
                    new Vector2(320f, 5f));

                TMP_Text phase = CreateText(
                    "RewardPhase",
                    panel,
                    font,
                    "战斗胜利",
                    29f,
                    new Color(0.93f, 0.85f, 0.68f));
                Pin((RectTransform)phase.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 292f),
                    new Vector2(400f, 52f));

                Image artworkFrame = CreateImage(
                    "ArtworkFrame",
                    panel,
                    new Color(0.27f, 0.25f, 0.2f, 0.92f));
                Pin((RectTransform)artworkFrame.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 112f),
                    new Vector2(236f, 236f));
                Outline frameOutline = artworkFrame.gameObject
                    .AddComponent<Outline>();
                frameOutline.effectColor = new Color(
                    0.79f, 0.65f, 0.37f, 0.9f);
                frameOutline.effectDistance = new Vector2(3f, -3f);

                Image artwork = CreateImage(
                    "RewardArtwork",
                    artworkFrame.transform,
                    Color.white);
                artwork.preserveAspect = true;
                artwork.enabled = false;
                Stretch((RectTransform)artwork.transform,
                    new Vector2(14f, 14f),
                    new Vector2(-14f, -14f));

                TMP_Text itemName = CreateText(
                    "RewardItemName",
                    panel,
                    font,
                    "奖励尚未领取",
                    38f,
                    new Color(0.97f, 0.93f, 0.84f));
                Pin((RectTransform)itemName.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -42f),
                    new Vector2(420f, 58f));

                TMP_Text rarity = CreateText(
                    "RewardRarity",
                    panel,
                    font,
                    "本次战利等待确认",
                    24f,
                    new Color(0.83f, 0.74f, 0.57f));
                Pin((RectTransform)rarity.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -88f),
                    new Vector2(400f, 38f));

                Image effectPlate = CreateImage(
                    "EffectPlate",
                    panel,
                    new Color(0.035f, 0.035f, 0.035f, 0.62f));
                Pin((RectTransform)effectPlate.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -170f),
                    new Vector2(410f, 118f));

                TMP_Text effect = CreateText(
                    "RewardEffect",
                    effectPlate.transform,
                    font,
                    "点击领取后，本次真实奖励才会进入道具栏。",
                    23f,
                    new Color(0.92f, 0.9f, 0.84f));
                effect.enableWordWrapping = true;
                effect.alignment = TextAlignmentOptions.Center;
                Stretch((RectTransform)effect.transform,
                    new Vector2(20f, 12f),
                    new Vector2(-20f, -12f));

                TMP_Text instance = CreateText(
                    "RewardInstance",
                    panel,
                    font,
                    "每场胜利仅领取一次",
                    18f,
                    new Color(0.66f, 0.65f, 0.61f));
                Pin((RectTransform)instance.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -250f),
                    new Vector2(400f, 30f));

                TMP_Text hint = CreateText(
                    "RewardHint",
                    panel,
                    font,
                    "领取与整理是两个独立步骤",
                    20f,
                    new Color(0.82f, 0.76f, 0.64f));
                hint.enableWordWrapping = true;
                Pin((RectTransform)hint.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -286f),
                    new Vector2(420f, 32f));

                Image actionImage = CreateImage(
                    "RewardAction",
                    panel,
                    new Color(0.36f, 0.21f, 0.085f, 1f));
                actionImage.raycastTarget = true;
                Pin((RectTransform)actionImage.transform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -335f),
                    new Vector2(286f, 72f));
                Button actionButton = actionImage.gameObject.AddComponent<Button>();
                actionButton.targetGraphic = actionImage;
                actionButton.transition = Selectable.Transition.ColorTint;
                ColorBlock colors = actionButton.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.9f, 0.68f, 1f);
                colors.pressedColor = new Color(0.78f, 0.64f, 0.42f, 1f);
                colors.disabledColor = new Color(0.42f, 0.4f, 0.36f, 0.6f);
                colors.colorMultiplier = 1f;
                actionButton.colors = colors;
                Outline buttonOutline = actionImage.gameObject
                    .AddComponent<Outline>();
                buttonOutline.effectColor = new Color(
                    0.86f, 0.67f, 0.33f, 0.95f);
                buttonOutline.effectDistance = new Vector2(2f, -2f);

                TMP_Text actionLabel = CreateText(
                    "ActionLabel",
                    actionImage.transform,
                    font,
                    "领取奖励",
                    28f,
                    new Color(0.98f, 0.93f, 0.81f));
                Stretch((RectTransform)actionLabel.transform,
                    new Vector2(8f, 4f),
                    new Vector2(-8f, -4f));

                view.AssignForEditor(
                    canvasGroup,
                    panel,
                    backdrop,
                    panelBackground,
                    rarityAccent,
                    artworkFrame,
                    artwork,
                    phase,
                    itemName,
                    rarity,
                    effect,
                    instance,
                    hint,
                    actionButton,
                    actionLabel,
                    backgrounds[0],
                    backgrounds[1],
                    backgrounds[2],
                    backgrounds[3],
                    backgrounds[4]);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_REWARD_POPUP_PREFAB_REFERENCE_INVALID");

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    RewardPopupPrefabPath);
                Require(saved != null,
                    "FORMAL_REWARD_POPUP_PREFAB_SAVE_FAILED");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void MountPopupIntoUnifiedShell()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(UnifiedPrefabPath);
            Require(root != null, "FORMAL_REWARD_POPUP_SHELL_LOAD_FAILED");
            try
            {
                UnifiedBattlePageShell shell =
                    root.GetComponent<UnifiedBattlePageShell>();
                UnifiedBattleFormalSceneHost host =
                    root.GetComponent<UnifiedBattleFormalSceneHost>();
                Require(shell != null && host != null,
                    "FORMAL_REWARD_POPUP_SHELL_ROOT_INVALID");

                FormalBattleRewardPopupView[] existing =
                    root.GetComponentsInChildren<FormalBattleRewardPopupView>(
                        true);
                Require(existing.Length <= 1,
                    "FORMAL_REWARD_POPUP_DUPLICATE count=" + existing.Length);
                FormalBattleRewardPopupView view;
                if (existing.Length == 1)
                {
                    view = existing[0];
                }
                else
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        RewardPopupPrefabPath);
                    Require(prefab != null,
                        "FORMAL_REWARD_POPUP_PREFAB_RELOAD_FAILED");
                    GameObject instance = PrefabUtility.InstantiatePrefab(
                        prefab,
                        root.transform) as GameObject;
                    Require(instance != null,
                        "FORMAL_REWARD_POPUP_MOUNT_FAILED");
                    view = instance.GetComponent<FormalBattleRewardPopupView>();
                }

                Require(view != null && view.ValidateAuthoredReferences(),
                    "FORMAL_REWARD_POPUP_MOUNT_REFERENCE_INVALID");
                view.gameObject.name = "FormalBattleRewardPopup";
                view.transform.SetParent(root.transform, false);
                view.transform.SetAsLastSibling();
                Stretch((RectTransform)view.transform);
                host.AssignRewardPopupForEditor(view);
                EditorUtility.SetDirty(host);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedPrefabPath);
                Require(saved != null,
                    "FORMAL_REWARD_POPUP_SHELL_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidatePersistedResult()
        {
            GameObject popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                RewardPopupPrefabPath);
            FormalBattleRewardPopupView popup = popupPrefab == null
                ? null
                : popupPrefab.GetComponent<FormalBattleRewardPopupView>();
            Require(popup != null && popup.ValidateAuthoredReferences(),
                "FORMAL_REWARD_POPUP_PERSISTED_PREFAB_INVALID");

            GameObject shellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                UnifiedPrefabPath);
            Require(shellPrefab != null,
                "FORMAL_REWARD_POPUP_PERSISTED_SHELL_MISSING");
            FormalBattleRewardPopupView[] views =
                shellPrefab.GetComponentsInChildren<FormalBattleRewardPopupView>(
                    true);
            UnifiedBattleFormalSceneHost host =
                shellPrefab.GetComponent<UnifiedBattleFormalSceneHost>();
            Require(views.Length == 1
                    && views[0].transform.parent == shellPrefab.transform
                    && views[0].ValidateAuthoredReferences()
                    && host != null,
                "FORMAL_REWARD_POPUP_PERSISTED_BINDING_INVALID");
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                "FORMAL_REWARD_POPUP_PERSISTED_BINDING_INVALID " + diagnostic);
        }

        private static GameObject NewUiObject(string name, Transform parent)
        {
            GameObject value = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer));
            if (parent != null)
            {
                value.transform.SetParent(parent, false);
            }
            return value;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color)
        {
            GameObject value = NewUiObject(name, parent);
            Image image = value.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            TMP_FontAsset font,
            string text,
            float fontSize,
            Color color)
        {
            GameObject value = NewUiObject(name, parent);
            TextMeshProUGUI label = value.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text ?? string.Empty;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            return label;
        }

        private static void Pin(
            RectTransform rect,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void Stretch(RectTransform rect)
        {
            Stretch(rect, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
        }

        private static void EnsureFolder(string assetPath)
        {
            string normalized = (assetPath ?? string.Empty).Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = Path.GetDirectoryName(normalized)
                ?.Replace('\\', '/');
            string leaf = Path.GetFileName(normalized);
            Require(!string.IsNullOrWhiteSpace(parent)
                    && !string.IsNullOrWhiteSpace(leaf),
                "FORMAL_REWARD_POPUP_FOLDER_INVALID " + normalized);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
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
