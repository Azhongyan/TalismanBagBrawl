using System.Collections.Generic;
using System.Text;
using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V02.Tags;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02TalismanTooltipUI : MonoBehaviour
    {
        private const string SpiritStoneId = "spirit_stone_basic";
        private const string FireTalismanId = "fire_talisman_basic";
        private const string ShieldTalismanId = "shield_talisman_basic";
        private const string QiPillId = "qi_pill_basic";
        private const string ThunderTalismanId = "thunder_talisman_basic";
        private const string SealId = "seal_basic";
        private const string SwordPillId = "sword_pill_basic";
        private const string ExorcismBellId = "exorcism_bell_basic";
        private const string WaterTalismanId = "water_talisman_basic";
        private const string ChainThunderId = "chain_thunder_talisman_basic";
        private const string PurifyId = "purify_talisman_basic";
        private const string SoulSuppressId = "soul_suppress_talisman_basic";

        [Header("Popup Root")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private bool closeOnOutsideClick = true;

        [Header("Adjustable Groups")]
        [SerializeField] private GameObject functionGroup;
        [SerializeField] private Text functionText;
        [SerializeField] private GameObject valueGroup;
        [SerializeField] private Text valueText;
        [SerializeField] private GameObject upgradeGroup;
        [SerializeField] private Text upgradeText;
        [SerializeField] private GameObject counterGroup;
        [SerializeField] private Text counterText;

        [Header("Selection Outline")]
        [SerializeField] private Color selectedOutlineColor = new(1f, 0.82f, 0.28f, 1f);
        [SerializeField] private Vector2 selectedOutlineDistance = new(6f, -6f);

        private readonly List<RaycastResult> raycastResults = new();
        private TalismanItemDefinition selectedDefinition;
        private DraggableTalismanItemView selectedItemView;
        private RectTransform panelRect;
        private Canvas parentCanvas;
        private CanvasGroup panelCanvasGroup;

        public TalismanItemDefinition SelectedDefinition => selectedDefinition;

        private void Awake()
        {
            CachePopupReferences();
        }

        private void Start()
        {
            Clear();
        }

        private void OnEnable()
        {
            DraggableTalismanItemView.ItemClicked += OnItemClicked;
            DraggableTalismanItemView.ItemDragStarted += OnItemDragStarted;
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Clear);
            }
        }

        private void OnDisable()
        {
            DraggableTalismanItemView.ItemClicked -= OnItemClicked;
            DraggableTalismanItemView.ItemDragStarted -= OnItemDragStarted;
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Clear);
            }

            SetSelectedItem(null);
        }

        private void Update()
        {
            if (!closeOnOutsideClick || !IsPanelOpen() || !TryGetPrimaryPointerDown(out Vector2 pointerPosition))
            {
                return;
            }

            if (IsPointerInsidePanel(pointerPosition) || IsPointerOverTalisman(pointerPosition))
            {
                return;
            }

            Clear();
        }

        public void Show(TalismanItemDefinition definition)
        {
            selectedDefinition = definition;
            SetSelectedItem(null);

            if (definition == null)
            {
                HidePopup();
                return;
            }

            ShowDefinition(definition, 1);
        }

        public void Show(DraggableTalismanItemView itemView)
        {
            TalismanItemDefinition definition = itemView != null ? itemView.Definition : null;
            if (definition == null)
            {
                Clear();
                return;
            }

            selectedDefinition = definition;
            SetSelectedItem(itemView);
            ShowDefinition(definition, Mathf.Max(1, itemView.Level));
        }

        public void Clear()
        {
            selectedDefinition = null;
            SetSelectedItem(null);
            HidePopup();
        }

        public void PrintSelectedTags()
        {
            Debug.Log(TalismanTagUtility.BuildDebugSummary(selectedDefinition));
        }

        private void OnItemClicked(DraggableTalismanItemView view)
        {
            Show(view);
        }

        private void OnItemDragStarted(DraggableTalismanItemView view)
        {
            Clear();
        }

        private void ShowDefinition(TalismanItemDefinition definition, int level)
        {
            if (panel != null)
            {
                SetPanelVisible(true);
            }

            SetText(titleText, $"{definition.displayName}  Lv{level}");

            bool hasStructuredGroups = functionText != null || valueText != null || upgradeText != null || counterText != null;
            if (bodyText != null)
            {
                bodyText.gameObject.SetActive(!hasStructuredGroups);
                if (!hasStructuredGroups)
                {
                    SetText(bodyText, BuildFallbackText(definition, level));
                }
            }

            SetGroup(functionGroup, functionText, BuildFunctionText(definition));
            SetGroup(valueGroup, valueText, BuildValueText(definition, level));
            SetGroup(upgradeGroup, upgradeText, BuildUpgradeText(definition, level));
            SetGroup(counterGroup, counterText, BuildCounterText(definition, level));
        }

        private void HidePopup()
        {
            if (panel != null)
            {
                SetPanelVisible(false);
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (panel == null)
            {
                return;
            }

            panel.SetActive(true);
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panel.GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = panel.AddComponent<CanvasGroup>();
                }
            }

            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.interactable = visible;
            panelCanvasGroup.blocksRaycasts = visible;
        }

        private void SetSelectedItem(DraggableTalismanItemView itemView)
        {
            if (selectedItemView == itemView)
            {
                return;
            }

            if (selectedItemView != null)
            {
                selectedItemView.SetSelectedVisual(false, selectedOutlineColor, selectedOutlineDistance);
            }

            selectedItemView = itemView;
            if (selectedItemView != null)
            {
                selectedItemView.SetSelectedVisual(true, selectedOutlineColor, selectedOutlineDistance);
            }
        }

        private string BuildFallbackText(TalismanItemDefinition definition, int level)
        {
            StringBuilder builder = new();
            builder.AppendLine(BuildFunctionText(definition));
            builder.AppendLine();
            builder.AppendLine(BuildValueText(definition, level));
            builder.AppendLine();
            builder.AppendLine(BuildUpgradeText(definition, level));
            builder.AppendLine();
            builder.AppendLine(BuildCounterText(definition, level));
            return builder.ToString().TrimEnd();
        }

        private string BuildFunctionText(TalismanItemDefinition definition)
        {
            StringBuilder builder = new();
            builder.AppendLine($"\u5143\u7d20\uff1a{TalismanTagUtility.GetElementTagName(definition.elementTag)}");
            builder.AppendLine($"\u529f\u80fd\uff1a{TalismanTagUtility.JoinFunctionTags(definition)}");
            builder.AppendLine($"\u6548\u679c\uff1a{TalismanTagUtility.GetEffectTypeName(definition.effectType)}");
            builder.AppendLine($"\u4f9b\u80fd\uff1a{GetFormationPowerText(definition)}");
            builder.AppendLine($"\u5b9a\u4f4d\uff1a{FirstText(definition.shortRoleDescription, definition.description, "\u672a\u586b\u5199")}");
            return builder.ToString().TrimEnd();
        }

        private string BuildValueText(TalismanItemDefinition definition, int level)
        {
            StringBuilder builder = new();
            builder.AppendLine($"\u5f53\u524d\uff1a{GetOutputLine(definition, level)}");
            builder.AppendLine($"\u8017\u7075\uff1a{GetManaCost(definition, level)}");
            builder.AppendLine($"\u51b7\u5374\uff1a{GetCooldown(definition):0.##}s");

            if (definition.baseValue > 0)
            {
                builder.AppendLine($"\u57fa\u7840\u6570\u503c\uff1a{definition.baseValue}");
            }

            builder.AppendLine($"\u5360\u683c\uff1a{definition.width}x{definition.height}");
            return builder.ToString().TrimEnd();
        }

        private string BuildUpgradeText(TalismanItemDefinition definition, int level)
        {
            StringBuilder builder = new();
            string levelTwoOutput = GetOutputLine(definition, 2);
            builder.AppendLine(level >= 2 ? $"\u5df2\u8fbe Lv2\uff1a{levelTwoOutput}" : $"Lv2\uff1a{levelTwoOutput}");
            builder.AppendLine($"\u8017\u7075\u53d8\u5316\uff1a{GetManaCost(definition, level)} -> {GetManaCost(definition, 2)}");
            builder.AppendLine($"\u5347\u7ea7\u6536\u76ca\uff1a{GetUpgradeLine(definition, level)}");
            return builder.ToString().TrimEnd();
        }

        private string BuildCounterText(TalismanItemDefinition definition, int level)
        {
            StringBuilder builder = new();
            builder.AppendLine($"\u514b\u5236\u6807\u7b7e\uff1a{TalismanTagUtility.JoinCounterTags(definition)}");
            builder.AppendLine($"\u514b\u5236\u8bf4\u660e\uff1a{FirstText(definition.counterDescription, "\u65e0")}");
            builder.AppendLine($"\u5e03\u9635\u5224\u65ad\uff1a{FirstText(GetSpecialLine(definition, level), "\u65e0\u989d\u5916\u9700\u6c42")}");
            return builder.ToString().TrimEnd();
        }

        private static string GetOutputLine(TalismanItemDefinition definition, int level)
        {
            return definition.itemId switch
            {
                SpiritStoneId => $"\u4ea7\u7075 +{(level >= 2 ? 18 : 12)}",
                FireTalismanId => $"\u4f24\u5bb3 {GetFireDamage(level)}",
                ThunderTalismanId => $"\u4f24\u5bb3 {GetThunderDamage(level)}",
                SwordPillId => $"\u4f24\u5bb3 {GetSwordDamage(level)}",
                ShieldTalismanId => $"\u62a4\u76fe +{(level >= 2 ? 28 : 18)}",
                QiPillId => $"\u6cbb\u7597 +{(level >= 2 ? 32 : 20)}",
                WaterTalismanId => $"\u6cbb\u7597 +{(level >= 2 ? 18 : 10)}",
                ChainThunderId => $"\u8fde\u9501\u4f24\u5bb3 {GetChainThunderDamage(level)}",
                SoulSuppressId => $"\u4f24\u5bb3 {GetSoulSuppressDamage(level)}",
                PurifyId => "\u51c0\u5316\u5f02\u5e38 / \u89e3\u5c01",
                SealId => level >= 2 ? "\u76f8\u90bb\u706b\u7b26\uff1a\u4f24\u5bb3 +6\uff1b\u76f8\u90bb\u96f7\u7b26\uff1a45% \u66b4\u51fb x2" : "\u76f8\u90bb\u706b\u7b26\uff1a\u4f24\u5bb3 +3\uff1b\u76f8\u90bb\u96f7\u7b26\uff1a30% \u66b4\u51fb x2",
                ExorcismBellId => $"\u9a71\u90aa\u4f24\u5bb3 {GetScaledValue(definition.baseValue, level)}",
                _ => GetOutputLineByEffect(definition, level)
            };
        }

        private static string GetOutputLineByEffect(TalismanItemDefinition definition, int level)
        {
            int value = GetScaledValue(definition.baseValue, level);
            return definition.effectType switch
            {
                EffectType.DealDamage => $"\u4f24\u5bb3 {value}",
                EffectType.ApplyBurn => $"\u707c\u70e7 / \u4f24\u5bb3 {value}",
                EffectType.ChainDamage => $"\u8fde\u9501\u4f24\u5bb3 {value}",
                EffectType.GainShield => $"\u62a4\u76fe +{value}",
                EffectType.Heal => $"\u6cbb\u7597 +{value}",
                EffectType.GenerateEnergy => $"\u4ea7\u7075 +{value}",
                EffectType.CleanseStatus => "\u51c0\u5316\u5f02\u5e38",
                EffectType.SuppressGhost => $"\u9547\u9b42\u4f24\u5bb3 {value}",
                EffectType.EnhanceAdjacent => "\u5f3a\u5316\u76f8\u90bb",
                _ => definition.baseValue > 0 ? $"\u6570\u503c {value}" : "\u65e0\u76f4\u63a5\u6570\u503c"
            };
        }

        private static string GetSpecialLine(TalismanItemDefinition definition, int level)
        {
            return definition.itemId switch
            {
                FireTalismanId => $"\u76f8\u90bb\u6cd5\u5370 +{(level >= 2 ? 6 : 3)} \u4f24\u5bb3\uff1b\u76f8\u90bb\u805a\u7075\u77f3\u52a0\u901f",
                ThunderTalismanId => "\u7834\u76fe\u503e\u5411\uff1b\u6253\u65ad\u84c4\u529b\uff1b\u5bf9\u9b3c\u602a x1.5\uff1b\u76f8\u90bb\u6cd5\u5370\u53ef\u66b4\u51fb x2",
                SwordPillId => $"\u76f8\u90bb\u706b\u7b26 +{(level >= 2 ? 8 : 5)} \u4f24\u5bb3\uff1b\u706b\u5251\u6d41\u53ef\u653e\u5927",
                ShieldTalismanId => "\u62a4\u76fe\u53d7\u62a4\u8eab\u7b26\u5f3a\u5316\u5956\u52b1\u653e\u5927",
                QiPillId => level >= 2 ? "\u76f8\u90bb\u62a4\u8eab\u7b26\u65f6\u6cbb\u7597 +40" : "\u76f8\u90bb\u62a4\u8eab\u7b26\u65f6\u6cbb\u7597 +28",
                WaterTalismanId => "\u76f8\u90bb\u4e39\u836f\u53ef\u7f29\u77ed\u4e39\u836f\u51b7\u5374",
                ChainThunderId => "\u514b\u5236\u7fa4\u602a / \u53ec\u5524",
                SoulSuppressId => "\u88ab\u4f9b\u80fd\u65f6\uff0c\u53ef\u53cd\u5236\u5077\u7075\u7c7b\u6280\u80fd\uff0c\u964d\u4f4e\u9635\u773c\u6216\u805a\u7075\u77f3\u88ab\u5e72\u6270\u98ce\u9669\uff1b\u547d\u4e2d\u9b3c\u602a / \u5077\u7075\u5f31\u70b9\u65f6\u4f24\u5bb3\u7ea6 x1.4",
                PurifyId => "\u9488\u5bf9\u4e2d\u6bd2\u3001\u707c\u70e7\u3001\u5c01\u5370",
                SealId => "\u53ea\u5f3a\u5316\u76f8\u90bb\u706b\u7b26 / \u96f7\u7b26\uff0c\u4e0d\u76f4\u63a5\u9020\u6210\u4f24\u5bb3",
                SpiritStoneId => "\u4f9b\u80fd\u6e90\uff1a\u4e0d\u9700\u9635\u6cd5\u4f9b\u80fd",
                _ => string.Empty
            };
        }

        private static string GetUpgradeLine(TalismanItemDefinition definition, int currentLevel)
        {
            if (currentLevel >= 2)
            {
                return "\u5df2\u8fbe\u5230\u5f53\u524d\u9a8c\u8bc1\u7248\u6700\u9ad8\u7b49\u7ea7";
            }

            return definition.itemId switch
            {
                SpiritStoneId => "\u4ea7\u7075 +6",
                FireTalismanId => "\u4f24\u5bb3 +8\uff0c\u6cd5\u5370\u76f8\u90bb\u5956\u52b1 +3",
                ThunderTalismanId => "\u4f24\u5bb3 +10\uff0c\u8017\u7075 +2",
                SwordPillId => "\u4f24\u5bb3 +6\uff0c\u706b\u7b26\u76f8\u90bb\u5956\u52b1 +3",
                ShieldTalismanId => "\u62a4\u76fe +10\uff0c\u8017\u7075 +2",
                QiPillId => "\u6cbb\u7597 +12\uff0c\u8017\u7075 +2",
                WaterTalismanId => "\u6cbb\u7597 +8",
                ChainThunderId => "\u8fde\u9501\u4f24\u5bb3 +8",
                SoulSuppressId => "\u4f24\u5bb3 +6",
                SealId => "\u706b\u7b26\u52a0\u6210 +3\uff1b\u96f7\u7b26\u66b4\u51fb\u7387 +15%",
                _ => definition.baseValue > 0 ? $"\u6570\u503c\u7ea6 +{GetScaledValue(definition.baseValue, 2) - definition.baseValue}" : "\u4e3b\u8981\u63d0\u5347\u529f\u80fd\u8868\u73b0"
            };
        }

        private static int GetManaCost(TalismanItemDefinition definition, int level)
        {
            return definition.itemId switch
            {
                FireTalismanId => level >= 2 ? 12 : 10,
                ThunderTalismanId => level >= 2 ? 18 : 16,
                ShieldTalismanId => level >= 2 ? 10 : 8,
                QiPillId => level >= 2 ? 14 : 12,
                _ => Mathf.Max(0, definition.manaCost)
            };
        }

        private static float GetCooldown(TalismanItemDefinition definition)
        {
            return Mathf.Max(0.1f, definition.baseCooldown);
        }

        private static string GetFormationPowerText(TalismanItemDefinition definition)
        {
            if (!definition.requiresFormationPower)
            {
                return "\u4e0d\u9700\u8981";
            }

            return definition.energyRequired > 0 ? $"\u9700\u8981 {definition.energyRequired}" : "\u9700\u8981";
        }

        private static int GetFireDamage(int level) => level >= 2 ? 20 : 12;
        private static int GetThunderDamage(int level) => level >= 2 ? 28 : 18;
        private static int GetSwordDamage(int level) => level >= 2 ? 14 : 8;
        private static int GetChainThunderDamage(int level) => level >= 2 ? 22 : 14;
        private static int GetSoulSuppressDamage(int level) => level >= 2 ? 16 : 10;

        private static int GetScaledValue(int baseValue, int level)
        {
            return level >= 2 ? Mathf.RoundToInt(baseValue * 1.5f) : baseValue;
        }

        private bool IsPanelOpen()
        {
            if (panel == null || !panel.activeInHierarchy)
            {
                return false;
            }

            return panelCanvasGroup == null || panelCanvasGroup.alpha > 0.01f;
        }

        private bool TryGetPrimaryPointerDown(out Vector2 pointerPosition)
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                    {
                        pointerPosition = touch.position;
                        return true;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                pointerPosition = Input.mousePosition;
                return true;
            }

            pointerPosition = Vector2.zero;
            return false;
        }

        private bool IsPointerInsidePanel(Vector2 pointerPosition)
        {
            CachePopupReferences();
            if (panelRect == null)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(panelRect, pointerPosition, GetEventCamera());
        }

        private bool IsPointerOverTalisman(Vector2 pointerPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData eventData = new(EventSystem.current)
            {
                position = pointerPosition
            };

            raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            for (int i = 0; i < raycastResults.Count; i++)
            {
                if (raycastResults[i].gameObject.GetComponentInParent<DraggableTalismanItemView>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private Camera GetEventCamera()
        {
            CachePopupReferences();
            return parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;
        }

        private void CachePopupReferences()
        {
            if (panelRect == null && panel != null)
            {
                panelRect = panel.GetComponent<RectTransform>();
            }

            if (parentCanvas == null && panel != null)
            {
                parentCanvas = panel.GetComponentInParent<Canvas>();
            }
        }

        private static void SetGroup(GameObject group, Text text, string value)
        {
            if (group != null)
            {
                group.SetActive(true);
            }

            SetText(text, value);
        }

        private static string FirstText(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.text = value;
            }
        }
    }
}
