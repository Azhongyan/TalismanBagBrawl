using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildSandboxItemInfoPanel : MonoBehaviour
    {
        public const string RuntimeRootName = "BuildSandboxItemInfoPanel_Runtime";
        private const string LegacyRuntimeRootName = "BuildSandboxItemInfoPanel";
        private const string PanelObjectName = "PanelRoot";
        private const string TitleObjectName = "TitleText";
        private const string CloseButtonObjectName = "CloseButton";
        private const string RotateButtonObjectName = "RotateButton";
        private const string BodyViewportObjectName = "BodyViewport";
        private const string BodyContentObjectName = "BodyContent";
        private const string BodyTextObjectName = "BodyText";
        private const string LabelObjectName = "Label";
        public const string CloseButtonLabel = "×";
        public const string RotateButtonLabel = "Rotate";
        private static readonly string[] LegacyHierarchyNames =
        {
            "\u9762\u677f",
            "\u6807\u9898",
            "\u5173\u95ed",
            "\u6b63\u6587\u88c1\u526a",
            "\u6b63\u6587",
            "\u5185\u5bb9",
            "\u6587\u5b57"
        };

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalUi;
        [SerializeField] private bool touchesFormalScene;
        [SerializeField] private bool showsCompleteAnswers;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button rotateButton;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private ScrollRect scrollRect;

        private Action<string> rotateRequested;
        private string selectedItemId = string.Empty;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalFlow => writesFormalFlow;
        public bool WritesFormalUi => writesFormalUi;
        public bool TouchesFormalScene => touchesFormalScene;
        public bool ShowsCompleteAnswers => showsCompleteAnswers;
        public bool IsShowing => panelCanvasGroup != null && panelCanvasGroup.alpha > 0.01f;

        public static BuildSandboxItemInfoPanel FindOrCreateInScene()
        {
            Transform popupLayer = FindTransform("PopupLayer");
            if (popupLayer == null)
            {
                return null;
            }

            GameObject root = FindOrCreateRuntimeRoot(popupLayer);
            BuildSandboxItemInfoPanel panel = root.GetComponent<BuildSandboxItemInfoPanel>();
            if (panel == null)
            {
                panel = root.AddComponent<BuildSandboxItemInfoPanel>();
            }

            panel.EnsureRuntimePanel();
            return panel;
        }

        private void Awake()
        {
            EnsureRuntimePanel();
            Hide();
        }

        private void OnEnable()
        {
            EnsureRuntimePanel();
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            WireRotateButton();
        }

        private void OnDisable()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }

            if (rotateButton != null)
            {
                rotateButton.onClick.RemoveListener(OnRotateClicked);
            }
        }

        public void SetRotateHandler(Action<string> handler)
        {
            rotateRequested = handler;
            EnsureRuntimePanel();
            WireRotateButton();
        }

        public void Show(
            BuildGridInteractionPreviewController.PreviewItem item,
            BuildGridInteractionItemInfoContext context,
            bool rotateEnabled)
        {
            EnsureRuntimePanel();
            if (item == null || panelRoot == null)
            {
                Hide();
                return;
            }

            selectedItemId = item.ItemId;
            SetText(titleText, item.DisplayName);
            SetText(bodyText, BuildPlayerBodyText(item, context));
            SetRotateButtonState(true, rotateEnabled);
            SetVisible(true);

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            panelRoot.transform.SetAsLastSibling();
        }

        public void RefreshIfShowing(
            BuildGridInteractionPreviewController.PreviewItem item,
            BuildGridInteractionItemInfoContext context,
            bool rotateEnabled)
        {
            if (!IsShowing || item == null || !string.Equals(selectedItemId, item.ItemId, StringComparison.Ordinal))
            {
                return;
            }

            Show(item, context, rotateEnabled);
        }

        public void Hide()
        {
            selectedItemId = string.Empty;
            SetRotateButtonState(false, false);
            SetVisible(false);
        }

        public static string BuildPlayerBodyText(
            BuildGridInteractionPreviewController.PreviewItem item,
            BuildGridInteractionItemInfoContext context)
        {
            if (item == null)
            {
                return string.Empty;
            }

            ItemInfoProfile profile = BuildProfile(item);
            BuildGridInteractionItemInfoContext safeContext = context ?? BuildGridInteractionItemInfoContext.Empty;
            StringBuilder builder = new();
            Append(builder, "名称", item.DisplayName);
            Append(builder, "类型", profile.TypeName);
            Append(builder, "品质", profile.Quality);
            Append(builder, "标签", profile.Tags);
            Append(builder, "基础效果", profile.BaseEffect);
            Append(builder, "触发条件", profile.TriggerCondition);
            Append(builder, "冷却/频率", profile.CooldownFrequency);
            Append(builder, "数值", profile.ValueText);
            Append(builder, "词条", profile.AffixText);
            Append(builder, "羁绊贡献", profile.BondContribution);
            Append(builder, "摆放形状", item.ShapeDisplayName);
            Append(builder, "说明", profile.Description);
            builder.AppendLine();
            Append(builder, "多格形状", safeContext.MultiCellShapeText);
            Append(builder, "占用格子", safeContext.OccupiedCellsText);
            Append(builder, "旋转占格", safeContext.RotatedCellsText);
            Append(builder, "词条预览", profile.AffixPreview);
            Append(builder, "效果变动预览", profile.ModifierPreview);
            Append(builder, "事件预览", profile.EventPreview);
            Append(builder, "构筑贡献", profile.BuildContribution);
            Append(builder, "配合线索", profile.PlayerClue);
            return builder.ToString().TrimEnd();
        }

        private void EnsureRuntimePanel()
        {
            if (panelRoot != null && titleText != null && bodyText != null && closeButton != null && rotateButton != null)
            {
                return;
            }

            Transform popupLayer = FindTransform("PopupLayer");
            if (popupLayer == null)
            {
                return;
            }

            GameObject root = FindOrCreateRuntimeRoot(popupLayer);
            BindExisting(root);
            EnsurePanelChildren(root.transform);
        }

        private void BindExisting(GameObject root)
        {
            panelRoot = root;
            panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
            titleText = FindChildText(panelRoot.transform, TitleObjectName)
                ?? FindChildText(panelRoot.transform, "标题");
            bodyText = FindChildText(panelRoot.transform, BodyTextObjectName)
                ?? FindChildText(panelRoot.transform, "内容");
            closeButton = FindChildButton(panelRoot.transform, CloseButtonObjectName)
                ?? FindChildButton(panelRoot.transform, "关闭");
            rotateButton = FindChildButton(panelRoot.transform, RotateButtonObjectName);
            scrollRect = panelRoot.GetComponentInChildren<ScrollRect>(true);
        }

        private void CreateRuntimePanel(Transform popupLayer)
        {
            panelRoot = FindOrCreateRuntimeRoot(popupLayer);
            EnsurePanelChildren(panelRoot.transform);
            Hide();
        }

        private void EnsurePanelChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            panelRoot = root.gameObject;
            CleanupLegacyChildren(root);
            if (panelRoot.name != RuntimeRootName)
            {
                panelRoot.name = RuntimeRootName;
            }

            RectTransform rootRect = panelRoot.GetComponent<RectTransform>();
            if (rootRect == null)
            {
                rootRect = panelRoot.AddComponent<RectTransform>();
            }
            if (panelRoot.GetComponent<CanvasRenderer>() == null)
            {
                panelRoot.AddComponent<CanvasRenderer>();
            }
            Image shade = panelRoot.GetComponent<Image>();
            if (shade == null)
            {
                shade = panelRoot.AddComponent<Image>();
            }
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = panelRoot.AddComponent<CanvasGroup>();
                }
            }

            SetFullStretch(rootRect);
            shade.color = new Color(0f, 0f, 0f, 0.42f);
            shade.raycastTarget = true;

            Transform existingPanel = root.Find(PanelObjectName);
            GameObject panel = existingPanel != null
                ? existingPanel.gameObject
                : new GameObject(PanelObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            panel.transform.SetParent(panelRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            if (panelRect == null)
            {
                panelRect = panel.AddComponent<RectTransform>();
            }
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 610f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panel.AddComponent<Image>();
            }
            panelImage.color = new Color(0.14f, 0.13f, 0.10f, 0.98f);
            Outline outline = panel.GetComponent<Outline>();
            if (outline == null)
            {
                outline = panel.AddComponent<Outline>();
            }
            outline.effectColor = new Color(0.64f, 0.48f, 0.22f, 0.82f);
            outline.effectDistance = new Vector2(2f, -2f);

            titleText = EnsureText(TitleObjectName, panel.transform, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetAnchors(titleText.rectTransform, new Vector2(0.05f, 0.88f), new Vector2(0.75f, 0.97f));

            rotateButton = EnsureButton(RotateButtonObjectName, panel.transform, RotateButtonLabel);
            SetAnchors(rotateButton.GetComponent<RectTransform>(), new Vector2(0.77f, 0.89f), new Vector2(0.88f, 0.97f));
            Text rotateLabel = rotateButton.GetComponentInChildren<Text>(true);
            if (rotateLabel != null)
            {
                rotateLabel.text = RotateButtonLabel;
                rotateLabel.fontSize = 18;
            }

            closeButton = EnsureButton(CloseButtonObjectName, panel.transform, CloseButtonLabel);
            SetAnchors(closeButton.GetComponent<RectTransform>(), new Vector2(0.89f, 0.89f), new Vector2(0.97f, 0.97f));

            Transform existingViewport = panel.transform.Find(BodyViewportObjectName);
            GameObject viewport = existingViewport != null
                ? existingViewport.gameObject
                : new GameObject(BodyViewportObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(panel.transform, false);
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            if (viewportRect == null)
            {
                viewportRect = viewport.AddComponent<RectTransform>();
            }
            SetAnchors(viewportRect, new Vector2(0.05f, 0.06f), new Vector2(0.95f, 0.86f));
            Image viewportImage = viewport.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewport.AddComponent<Image>();
            }
            viewportImage.color = new Color(0.06f, 0.06f, 0.05f, 0.16f);
            if (viewport.GetComponent<RectMask2D>() == null)
            {
                viewport.AddComponent<RectMask2D>();
            }

            Transform existingContent = viewport.transform.Find(BodyContentObjectName);
            GameObject content = existingContent != null
                ? existingContent.gameObject
                : new GameObject(BodyContentObjectName, typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            if (contentRect == null)
            {
                contentRect = content.AddComponent<RectTransform>();
            }
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 780f);

            bodyText = EnsureText(BodyTextObjectName, content.transform, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft);
            SetFullStretch(bodyText.rectTransform);
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Overflow;
            bodyText.lineSpacing = 1.12f;

            scrollRect = panelRoot.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = panelRoot.AddComponent<ScrollRect>();
            }
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 22f;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
            WireRotateButton();
        }

        private void WireRotateButton()
        {
            if (rotateButton == null)
            {
                return;
            }

            rotateButton.onClick.RemoveListener(OnRotateClicked);
            rotateButton.onClick.AddListener(OnRotateClicked);
        }

        private void OnRotateClicked()
        {
            if (string.IsNullOrWhiteSpace(selectedItemId))
            {
                return;
            }

            rotateRequested?.Invoke(selectedItemId);
        }

        private void SetRotateButtonState(bool visible, bool interactable)
        {
            if (rotateButton == null)
            {
                return;
            }

            rotateButton.gameObject.SetActive(visible);
            rotateButton.interactable = visible && interactable;
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot == null)
            {
                return;
            }

            panelRoot.SetActive(true);
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelRoot.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = panelRoot.AddComponent<CanvasGroup>();
                }
            }

            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.interactable = visible;
            panelCanvasGroup.blocksRaycasts = visible;
        }

        private static ItemInfoProfile BuildProfile(BuildGridInteractionPreviewController.PreviewItem item)
        {
            return item.ItemId switch
            {
                "preview_fire_talisman" => new ItemInfoProfile(
                    "攻击符箓",
                    "蓝",
                    "火、符箓、爆发",
                    "对前线目标造成火焰伤害倾向。",
                    "有供能且进入斗法循环后自动触发。",
                    "较快，适合频繁补伤。",
                    "输出偏中，受相邻增益影响更明显。",
                    "火势增强、短促连发",
                    "偏向火符羁绊，靠近法器时更容易形成组合。",
                    "轻薄符纸，适合补空位。",
                    "用于观察单格道具在构筑中的启动手感。",
                    "可能获得额外火势提示，不展示完整答案。",
                    "预览中只表现为伤害倾向变化。",
                    "触发时偏向短促爆发事件。",
                    "补足火系输出位，让相邻线索更容易成立。",
                    "靠近能持续供能或增强符箓的位置，通常更顺手。"),
                "preview_guard_wood" => new ItemInfoProfile(
                    "防护法器",
                    "绿",
                    "守护、法器、阵位",
                    "提供护阵与稳定站位倾向。",
                    "放入棋盘后随阵势循环生效。",
                    "稳定，偏持续。",
                    "防护偏中，重在连接。",
                    "护阵余韵、稳固边角",
                    "偏向守护羁绊，适合连接防护与供能。",
                    "竖向木牌，占用连续两格。",
                    "适合演示竖向多格道具和旋转后的占位变化。",
                    "可能带来更稳的相邻防护提示。",
                    "预览中只表现为防护倾向变化。",
                    "触发时偏向护阵事件。",
                    "帮助构筑形成稳定骨架。",
                    "放在需要承接上下关系的位置，会更容易读出线索。"),
                "preview_cleanse_corner" => new ItemInfoProfile(
                    "辅助符箓",
                    "紫",
                    "净化、符箓、拐角",
                    "提供清除异常与缓解干扰的倾向。",
                    "遇到异常压力时更容易体现价值。",
                    "中等，偏关键时刻。",
                    "功能偏强，直接数值较弱。",
                    "净厄回响、拐角守护",
                    "偏向净化与守护的交界羁绊。",
                    "拐角三格，适合卡住转折位。",
                    "用于观察拐角形状的预览、旋转和重叠反馈。",
                    "可能提示一条净化配合线索。",
                    "预览中只表现为异常缓解倾向。",
                    "触发时偏向解除干扰事件。",
                    "让构筑在受干扰时更不容易断线。",
                    "贴近容易被打断的核心位，通常更有意义。"),
                "preview_stone_core" => new ItemInfoProfile(
                    "材料法芯",
                    "橙",
                    "炉芯、材料、核心",
                    "提供稳定供能与核心支点倾向。",
                    "放入后作为构筑中心参与循环。",
                    "较慢，偏核心。",
                    "单次贡献较高，但占位压力更大。",
                    "炉火余温、核心稳压",
                    "偏向能量与火势的核心羁绊。",
                    "方形四格，占据明显区域。",
                    "用于观察大形状的越界与空间规划。",
                    "可能出现核心词条提示，但不显示完整解法。",
                    "预览中只表现为供能倾向变化。",
                    "触发时偏向构筑稳定事件。",
                    "适合作为周边道具围绕的支点。",
                    "先给它留出空间，再安排小件补线会更清晰。"),
                "preview_energy_incense" => new ItemInfoProfile(
                    "消耗道具",
                    "绿",
                    "供能、消耗、节奏",
                    "短时间提升能量流动倾向。",
                    "放入棋盘后随节奏触发。",
                    "较快，偏节奏调整。",
                    "直接数值较低，节奏价值更高。",
                    "聚能余香、节奏补足",
                    "偏向能量羁绊，适合补齐供能链。",
                    "竖向两格，可旋转为横向占位。",
                    "用于观察同形状不同朝向的占格。",
                    "可能提示供能相关词条。",
                    "预览中只表现为节奏倾向变化。",
                    "触发时偏向供能事件。",
                    "帮助高频道具更稳定地启动。",
                    "靠近需要供能的道具，比单独放置更容易看懂。"),
                "preview_old_bell" => new ItemInfoProfile(
                    "特殊法器",
                    "蓝",
                    "镇邪、特殊、拐角",
                    "提供压制与干扰削弱倾向。",
                    "遇到特殊敌性压力时更有存在感。",
                    "中等，偏应对。",
                    "输出不突出，功能更明显。",
                    "铜铃震荡、镇邪余音",
                    "偏向控制与净化之间的羁绊。",
                    "拐角三格，适合贴边或转折。",
                    "用于观察复杂形状在边界附近的反馈。",
                    "可能提示控制相关词条。",
                    "预览中只表现为压制倾向变化。",
                    "触发时偏向镇邪事件。",
                    "补足对干扰类压力的应对线。",
                    "放在能接触更多邻近道具的位置，线索会更明显。"),
                "preview_thunder_sword" => new ItemInfoProfile(
                    "攻击符箓",
                    "蓝",
                    "雷、剑、符箓",
                    "提供迅捷打击与破势倾向。",
                    "供能后随斗法循环触发。",
                    "较快，偏爆点。",
                    "单次偏中，适合补突破口。",
                    "雷引锋芒、短击连动",
                    "偏向雷符与剑势羁绊。",
                    "单格，适合填补关键空位。",
                    "用于观察小件在大件间的补位。",
                    "可能提示雷系词条。",
                    "预览中只表现为破势倾向变化。",
                    "触发时偏向快速打击事件。",
                    "帮助构筑形成短促爆点。",
                    "贴近能放大攻击节奏的位置，会更容易形成配合。"),
                "preview_soul_seal" => new ItemInfoProfile(
                    "防护法器",
                    "紫",
                    "镇魂、法印、防护",
                    "提供稳固核心与缓冲压力倾向。",
                    "放入棋盘后作为重型防护参与循环。",
                    "较慢，偏稳定。",
                    "防护偏高，占位要求也高。",
                    "镇魂回护、法印稳场",
                    "偏向守护与控制的深层羁绊。",
                    "方形四格，适合做稳固区域。",
                    "用于观察大件旋转无效与空间占用。",
                    "可能提示防护相关词条。",
                    "预览中只表现为稳场倾向变化。",
                    "触发时偏向防护事件。",
                    "让构筑获得更稳的承压中心。",
                    "适合先确定核心位置，再用小件补足边缘。"),
                _ => new ItemInfoProfile(
                    item.Category,
                    "白",
                    "道具、沙盒",
                    "用于预览构筑交互。",
                    "放入棋盘后参与预览。",
                    "稳定。",
                    "数值以模糊区间展示。",
                    "预览词条",
                    "提供基础构筑贡献。",
                    item.ShapeDisplayName,
                    "沙盒预览道具。",
                    "仅显示模糊词条线索。",
                    "仅显示效果倾向。",
                    "仅显示事件倾向。",
                    "帮助理解摆放关系。",
                    "尝试靠近相关类型道具。")
            };
        }

        private static void Append(StringBuilder builder, string label, string value)
        {
            builder.Append(label);
            builder.Append('：');
            builder.AppendLine(string.IsNullOrWhiteSpace(value) ? "暂无" : value);
        }

        private static Transform FindTransform(string objectName)
        {
            Transform[] transforms = FindObjectsOfType<Transform>(true);
            foreach (Transform target in transforms)
            {
                if (target != null && target.name == objectName)
                {
                    return target;
                }
            }

            return null;
        }

        private static GameObject FindOrCreateRuntimeRoot(Transform popupLayer)
        {
            List<Transform> candidates = new();
            foreach (Transform child in popupLayer)
            {
                if (child.name == RuntimeRootName || child.name == LegacyRuntimeRootName)
                {
                    candidates.Add(child);
                }
            }

            Transform keep = candidates.FirstOrDefault(child => child.name == RuntimeRootName)
                ?? candidates.FirstOrDefault();
            if (keep == null)
            {
                GameObject created = new(RuntimeRootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                created.transform.SetParent(popupLayer, false);
                created.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                return created;
            }

            keep.name = RuntimeRootName;
            keep.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            foreach (Transform duplicate in candidates)
            {
                if (duplicate != null && duplicate != keep)
                {
                    SafeDestroy(duplicate.gameObject);
                }
            }

            return keep.gameObject;
        }

        private static void SafeDestroy(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static void CleanupLegacyChildren(Transform root)
        {
            if (root == null)
            {
                return;
            }

            List<GameObject> legacyObjects = new();
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == root)
                {
                    continue;
                }

                if (LegacyHierarchyNames.Any(name => child.name == name))
                {
                    legacyObjects.Add(child.gameObject);
                }
            }

            foreach (GameObject legacyObject in legacyObjects)
            {
                SafeDestroy(legacyObject);
            }
        }

        private static Text FindChildText(Transform parent, string childName)
        {
            Transform child = FindChild(parent, childName);
            return child == null ? null : child.GetComponent<Text>();
        }

        private static Button FindChildButton(Transform parent, string childName)
        {
            Transform child = FindChild(parent, childName);
            return child == null ? null : child.GetComponent<Button>();
        }

        private static Transform FindChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Text EnsureText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                return CreateText(name, parent, value, size, style, alignment);
            }

            Text text = existing.GetComponent<Text>();
            if (text == null)
            {
                text = existing.gameObject.AddComponent<Text>();
            }

            text.text = value ?? text.text ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.91f, 0.88f, 0.76f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Button EnsureButton(string name, Transform parent, string label)
        {
            Transform existing = parent.Find(name);
            GameObject target;
            if (existing == null)
            {
                target = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                target.transform.SetParent(parent, false);
            }
            else
            {
                target = existing.gameObject;
                if (target.GetComponent<RectTransform>() == null)
                {
                    target.AddComponent<RectTransform>();
                }
                if (target.GetComponent<CanvasRenderer>() == null)
                {
                    target.AddComponent<CanvasRenderer>();
                }
                if (target.GetComponent<Image>() == null)
                {
                    target.AddComponent<Image>();
                }
                if (target.GetComponent<Button>() == null)
                {
                    target.AddComponent<Button>();
                }
            }

            Image image = target.GetComponent<Image>();
            image.color = new Color(0.43f, 0.32f, 0.16f, 1f);
            Button button = target.GetComponent<Button>();
            Text text = EnsureText(LabelObjectName, target.transform, label, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetFullStretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            target.transform.SetParent(parent, false);
            Text text = target.GetComponent<Text>();
            text.text = value ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.91f, 0.88f, 0.76f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            target.transform.SetParent(parent, false);
            Image image = target.GetComponent<Image>();
            image.color = new Color(0.43f, 0.32f, 0.16f, 1f);
            Button button = target.GetComponent<Button>();
            Text text = CreateText(LabelObjectName, target.transform, label, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetFullStretch(text.rectTransform);
            return button;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
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

        private static void SetFullStretch(RectTransform rect)
        {
            SetAnchors(rect, Vector2.zero, Vector2.one);
        }

        private readonly struct ItemInfoProfile
        {
            public ItemInfoProfile(
                string typeName,
                string quality,
                string tags,
                string baseEffect,
                string triggerCondition,
                string cooldownFrequency,
                string valueText,
                string affixText,
                string bondContribution,
                string placementShape,
                string description,
                string affixPreview,
                string modifierPreview,
                string eventPreview,
                string buildContribution,
                string playerClue)
            {
                TypeName = typeName;
                Quality = quality;
                Tags = tags;
                BaseEffect = baseEffect;
                TriggerCondition = triggerCondition;
                CooldownFrequency = cooldownFrequency;
                ValueText = valueText;
                AffixText = affixText;
                BondContribution = bondContribution;
                PlacementShape = placementShape;
                Description = description;
                AffixPreview = affixPreview;
                ModifierPreview = modifierPreview;
                EventPreview = eventPreview;
                BuildContribution = buildContribution;
                PlayerClue = playerClue;
            }

            public string TypeName { get; }
            public string Quality { get; }
            public string Tags { get; }
            public string BaseEffect { get; }
            public string TriggerCondition { get; }
            public string CooldownFrequency { get; }
            public string ValueText { get; }
            public string AffixText { get; }
            public string BondContribution { get; }
            public string PlacementShape { get; }
            public string Description { get; }
            public string AffixPreview { get; }
            public string ModifierPreview { get; }
            public string EventPreview { get; }
            public string BuildContribution { get; }
            public string PlayerClue { get; }
        }
    }

    public sealed class BuildGridInteractionItemInfoContext
    {
        public static readonly BuildGridInteractionItemInfoContext Empty = new(
            "形状数据待选择",
            "拖到棋盘后显示具体占格",
            "旋转后显示占格变化");

        public BuildGridInteractionItemInfoContext(
            string multiCellShapeText,
            string occupiedCellsText,
            string rotatedCellsText)
        {
            MultiCellShapeText = multiCellShapeText ?? string.Empty;
            OccupiedCellsText = occupiedCellsText ?? string.Empty;
            RotatedCellsText = rotatedCellsText ?? string.Empty;
        }

        public string MultiCellShapeText { get; }
        public string OccupiedCellsText { get; }
        public string RotatedCellsText { get; }
    }
}
