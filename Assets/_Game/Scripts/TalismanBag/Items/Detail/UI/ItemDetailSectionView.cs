using System;
using System.Collections.Generic;
using TalismanBag.Items.Detail;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Items.Detail.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemDetailSectionView : MonoBehaviour
    {
        private const string ArrayModifierIconToken = "[Icon_ArrayVeinModifier]";
        private const string BaseStatIconSlotNamePrefix = "BaseStatIconSlot_";
        private const string BaseStatRowsRootName = "BaseStatRowsRoot";
        private const string BaseStatRowNamePrefix = "BaseStatRow_";
        private const string BaseStatTextNamePrefix = "BaseStatText_";
        private const string InlineArrayModifierIconNamePrefix = "InlineArrayModifierIcon_";
        private const string FixedAffixIconSlotNamePrefix = "FixedAffixIconSlot_";
        private const string RandomAffixIconSlotNamePrefix = "RandomAffixIconSlot_";
        private const string FixedAffixRowsRootName = "FixedAffixRowsRoot";
        private const string FixedAffixRowNamePrefix = "FixedAffixRow_";
        private const string FixedAffixTextNamePrefix = "FixedAffixText_";
        private const string RandomAffixRowsRootName = "RandomAffixRowsRoot";
        private const string RandomAffixRowNamePrefix = "RandomAffixRow_";
        private const string RandomAffixTextNamePrefix = "RandomAffixText_";
        private const string DaoTraceIconSlotNamePrefix = "DaoTraceIconSlot_";
        private const string CoreEffectIconSlotNamePrefix = "CoreEffectIconSlot_";
        private const string FaMenBuildStageIconSlotNamePrefix = "FaMenBuildStageIconSlot_";
        private const string QiLeiBuildStageIconSlotNamePrefix = "QiLeiBuildStageIconSlot_";
        private const string SkillMonitorIconSlotNamePrefix = "SkillMonitorIconSlot_";
        private const int SectionTitleFontSize = 23;
        private const int SectionBodyFontSize = 20;
        private const int SectionDetailFontSize = 19;
        private const int SectionInactiveFontSize = 18;
        private const int SectionEmphasisFontSize = 21;
        private const float ArrayModifierIconSize = 16f;
        private const float LineIconSlotDefaultSize = 14f;
        private const string SectionDividerName = "SectionDivider";
        private const string DividerArtSlotName = "DividerArtSlot";
        private const float SmallDividerDefaultHeight = 3f;
        private const float LargeDividerDefaultHeight = 8f;
        private const bool ShowSectionChrome = false;

        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image accentImage;
        [SerializeField] private ItemDetailVisualTheme visualTheme;
        [SerializeField, Min(0)] private int lineIconTextPaddingSpaces = 6;

        private static Sprite cachedArrayModifierIconSprite;
        private static readonly Dictionary<string, Sprite> CachedBaseStatIconSprites =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Sprite> CachedAffixIconSprites =
            new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private readonly List<Image> arrayModifierIconImages = new();
        private Color rarityValueColor = new Color32(135, 182, 106, 255);
        private bool currentSectionIsDivider;
        private bool currentDividerIsLarge;
        private string currentRenderedBody = string.Empty;
        private string currentRarityKey = string.Empty;
        private string currentRarityDisplayName = string.Empty;

        private string LineIconTextPadding => new string(' ', Mathf.Max(0, lineIconTextPaddingSpaces));

        public string Title => titleText != null ? titleText.text : string.Empty;
        public string Body => bodyText != null && HasAuthoredContentRows()
            ? currentRenderedBody
            : bodyText != null ? bodyText.text : string.Empty;

        private void Awake()
        {
            ApplySectionChromeVisibility();
        }

#if UNITY_EDITOR
        public void ConfigureEditor(
            Text configuredTitleText,
            Text configuredBodyText,
            Image configuredBackgroundImage = null,
            Image configuredAccentImage = null)
        {
            titleText = configuredTitleText;
            bodyText = configuredBodyText;
            backgroundImage = configuredBackgroundImage;
            accentImage = configuredAccentImage;
            ApplyTypography();
        }

        public void ConfigureThemeEditor(ItemDetailVisualTheme configuredTheme)
        {
            visualTheme = configuredTheme;
        }
#endif

        public void SetVisualTheme(ItemDetailVisualTheme configuredTheme)
        {
            visualTheme = configuredTheme;
        }

        public void SetRarityColor(Color rarityColor)
        {
            rarityValueColor = rarityColor;
        }

        public void SetRarityPresentation(
            Color rarityColor,
            string rarityKey,
            string rarityDisplayName)
        {
            rarityValueColor = rarityColor;
            currentRarityKey = rarityKey ?? string.Empty;
            currentRarityDisplayName = rarityDisplayName ?? string.Empty;
        }

        public void SetContent(ItemDetailSectionViewModel model)
        {
            if (model == null)
            {
                SetContent(string.Empty, string.Empty, keepWhenEmpty: false);
                return;
            }

            SetSectionDisplayMode(model.stateKey);
            bool hasBody = !string.IsNullOrWhiteSpace(model.body);
            gameObject.SetActive(hasBody || model.keepWhenEmpty);
            ApplySectionChromeVisibility();
            if (titleText != null)
            {
                titleText.text = model.title ?? string.Empty;
            }

            if (bodyText != null)
            {
                string styledBody = StyleBody(model.stateKey, model.body);
                currentRenderedBody = RemoveArrayModifierIconToken(styledBody);
                bool usesBaseStatRows = ApplyAuthoredBaseStatRows(model.stateKey, styledBody);
                bool usesAffixRows = ApplyAuthoredAffixRows(model.stateKey, styledBody);
                bool usesAuthoredRows = usesBaseStatRows || usesAffixRows;
                if (!usesAuthoredRows)
                {
                    bodyText.text = currentRenderedBody;
                    ApplyBaseStatIconSlots(model.stateKey, styledBody);
                }

                if (!usesAffixRows)
                {
                    ApplyAffixIconSlots(model.stateKey, styledBody);
                }

                ApplyDetailLineIconSlots(model.stateKey, styledBody);
                ApplyInlineArrayModifierIcons(styledBody);
            }
        }

        public void SetContent(string title, string body, bool keepWhenEmpty = false)
        {
            SetSectionDisplayMode(string.Empty);
            bool hasBody = !string.IsNullOrWhiteSpace(body);
            gameObject.SetActive(hasBody || keepWhenEmpty);
            ApplySectionChromeVisibility();

            if (titleText != null)
            {
                titleText.text = title ?? string.Empty;
            }

            if (bodyText != null)
            {
                currentRenderedBody = RemoveArrayModifierIconToken(body);
                string authoredRowStateKey = ResolveAuthoredRowStateKey();
                bool usesBaseStatRows = ApplyAuthoredBaseStatRows(authoredRowStateKey, body);
                bool usesAffixRows = ApplyAuthoredAffixRows(authoredRowStateKey, body);
                bool usesAuthoredRows = usesBaseStatRows || usesAffixRows;
                if (!usesAuthoredRows)
                {
                    bodyText.text = currentRenderedBody;
                    ApplyBaseStatIconSlots(string.Empty, body);
                }

                if (!usesAffixRows)
                {
                    ApplyAffixIconSlots(string.Empty, body);
                }

                ApplyDetailLineIconSlots(string.Empty, body);
                ApplyInlineArrayModifierIcons(body);
            }
        }

        private void SetSectionDisplayMode(string stateKey)
        {
            string key = stateKey ?? string.Empty;
            currentSectionIsDivider = key.Contains("Divider", StringComparison.OrdinalIgnoreCase);
            currentDividerIsLarge = key.Contains("largeDivider", StringComparison.OrdinalIgnoreCase);
        }

        private void ApplySectionChromeVisibility()
        {
            SetGraphicVisible(titleText, ShowSectionChrome);
            SetGraphicVisible(accentImage, ShowSectionChrome);
            ApplySectionContainerVisibility();
            ApplyEditableDividerVisibility();
        }

        private static void SetGraphicVisible(Graphic graphic, bool visible)
        {
            if (graphic != null && graphic.gameObject.activeSelf != visible)
            {
                graphic.gameObject.SetActive(visible);
            }
        }

        private static void SetTransformVisible(Transform target, bool visible)
        {
            if (target != null && target.gameObject.activeSelf != visible)
            {
                target.gameObject.SetActive(visible);
            }
        }

        private void ApplySectionContainerVisibility()
        {
            if (backgroundImage != null)
            {
                backgroundImage.enabled = !currentSectionIsDivider;
            }

            Outline outline = GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = !currentSectionIsDivider;
            }
        }

        private void ApplyEditableDividerVisibility()
        {
            Transform sectionDivider = transform.Find(SectionDividerName);
            if (sectionDivider != null)
            {
                if (currentSectionIsDivider)
                {
                    SetTransformVisible(sectionDivider, true);
                    SetTransformVisible(transform.Find(DividerArtSlotName), false);
                }

                return;
            }

            ApplyDividerArtSlotVisibility();
        }

        private void ApplyDividerArtSlotVisibility()
        {
            if (!currentSectionIsDivider)
            {
                return;
            }

            Image dividerImage = FindDividerArtSlotImage();
            if (dividerImage == null)
            {
                return;
            }

            if (!dividerImage.gameObject.activeSelf)
            {
                dividerImage.gameObject.SetActive(true);
            }

        }

        private Image FindDividerArtSlotImage()
        {
            Transform slot = transform.Find(DividerArtSlotName);
            return slot == null ? null : slot.GetComponent<Image>();
        }

        private void ApplyVisualState(string stateKey, string body)
        {
            string key = stateKey ?? string.Empty;
            string content = body ?? string.Empty;
            bool indicatesActive = ContainsAny(content, "已点亮", "已激活", "已开窍且可生效", "可生效", "Monitoring");
            bool indicatesInactive = ContainsAny(content, "未点亮", "未开窍", "暂不生效", "状态不可用", "未接入", "Locked");

            Color background = ThemeSecondaryCard;
            Color accent = ThemeTitle;
            Color title = ThemeTitle;

            if (key.StartsWith("debug", StringComparison.OrdinalIgnoreCase))
            {
                background = ThemePage;
                accent = ThemeWeak;
                title = ThemeSecondaryText;
            }
            else if (key.Equals("orange", StringComparison.OrdinalIgnoreCase))
            {
                background = ThemeCard;
                accent = ThemeRarityOrange;
                title = ThemeTitle;
            }
            else if (key.Contains("Build", StringComparison.OrdinalIgnoreCase)
                || key.Equals("skillMonitor", StringComparison.OrdinalIgnoreCase))
            {
                background = ThemeSecondaryCard;
                accent = indicatesActive ? ThemeBuildActive : indicatesInactive ? ThemeBuildInactive : ThemeBuildNearActive;
            }
            else if (key.Contains("Affix", StringComparison.OrdinalIgnoreCase))
            {
                background = ThemeSecondaryCard;
                accent = rarityValueColor;
            }
            else if (key.Equals("header", StringComparison.OrdinalIgnoreCase)
                || key.Equals("identity", StringComparison.OrdinalIgnoreCase)
                || key.Equals("stats", StringComparison.OrdinalIgnoreCase))
            {
                background = ThemeCard;
            }
            else if (indicatesActive && !indicatesInactive)
            {
                background = ThemeSecondaryCard;
                accent = ThemeBuildActive;
            }
            else if (indicatesInactive)
            {
                background = ThemeSecondaryCard;
                accent = ThemeRestriction;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = background;
            }

            if (accentImage != null)
            {
                accentImage.color = accent;
            }

            if (titleText != null)
            {
                titleText.color = title;
            }

            if (bodyText != null)
            {
                bodyText.color = ThemeBody;
            }
        }

#if UNITY_EDITOR
        private void ApplyTypography()
        {
            if (titleText != null)
            {
                titleText.supportRichText = true;
            }

            if (bodyText != null)
            {
                bodyText.supportRichText = true;
            }
        }
#endif

        private string StyleBody(string stateKey, string body)
        {
            if (string.IsNullOrWhiteSpace(body)
                || (stateKey ?? string.Empty).StartsWith("debug", StringComparison.OrdinalIgnoreCase))
            {
                return body ?? string.Empty;
            }

            string[] lines = body.Replace("\r", string.Empty).Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                lines[index] = StyleLine(stateKey ?? string.Empty, lines[index], index);
            }

            return string.Join("\n", lines);
        }

        private bool ApplyAuthoredBaseStatRows(string stateKey, string body)
        {
            Transform rowsRoot = GetBaseStatRowsRoot();
            if (rowsRoot == null)
            {
                return false;
            }

            bool showStats = string.Equals(
                stateKey ?? string.Empty,
                "stats",
                StringComparison.OrdinalIgnoreCase);
            if (!showStats || string.IsNullOrWhiteSpace(body))
            {
                SetAuthoredBaseStatRowsVisibleFrom(rowsRoot, 0, false);
                RefreshAuthoredRowsPreferredHeight();
                return true;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                string line = rawLines[lineIndex];
                if (!IsBaseStatIconLine(line))
                {
                    continue;
                }

                Transform row = rowsRoot.Find(BaseStatRowNamePrefix + slotIndex);
                if (row == null)
                {
                    break;
                }

                Text rowText = row.Find(BaseStatTextNamePrefix + slotIndex)?.GetComponent<Text>();
                if (rowText != null)
                {
                    rowText.text = RemoveArrayModifierIconToken(line).TrimStart();
                }

                Image icon = row.Find(BaseStatIconSlotNamePrefix + slotIndex)?.GetComponent<Image>();
                if (icon != null)
                {
                    Sprite resolvedSprite = ResolveBaseStatIconSprite(line);
                    if (resolvedSprite != null)
                    {
                        icon.sprite = resolvedSprite;
                    }

                    icon.gameObject.SetActive(icon.sprite != null);
                }

                if (!row.gameObject.activeSelf)
                {
                    row.gameObject.SetActive(true);
                }

                slotIndex++;
            }

            SetAuthoredBaseStatRowsVisibleFrom(rowsRoot, slotIndex, false);
            RefreshAuthoredRowsPreferredHeight();
            return true;
        }

        private Transform GetBaseStatRowsRoot()
        {
            return bodyText != null ? bodyText.transform.Find(BaseStatRowsRootName) : null;
        }

        private bool HasAuthoredContentRows()
        {
            if (bodyText == null)
            {
                return false;
            }

            return bodyText.transform.Find(BaseStatRowsRootName) != null
                || bodyText.transform.Find(FixedAffixRowsRootName) != null
                || bodyText.transform.Find(RandomAffixRowsRootName) != null;
        }

        private string ResolveAuthoredRowStateKey()
        {
            if (bodyText == null)
            {
                return string.Empty;
            }

            if (bodyText.transform.Find(BaseStatRowsRootName) != null)
            {
                return "stats";
            }

            if (bodyText.transform.Find(FixedAffixRowsRootName) != null)
            {
                return "fixedAffix";
            }

            return bodyText.transform.Find(RandomAffixRowsRootName) != null
                ? "randomAffix"
                : string.Empty;
        }

        private bool ApplyAuthoredAffixRows(string stateKey, string body)
        {
            if (bodyText == null)
            {
                return false;
            }

            Transform rowsRoot = bodyText.transform.Find(FixedAffixRowsRootName);
            bool fixedAffix = rowsRoot != null;
            string expectedStateKey = "fixedAffix";
            string rowNamePrefix = FixedAffixRowNamePrefix;
            string textNamePrefix = FixedAffixTextNamePrefix;
            string iconNamePrefix = FixedAffixIconSlotNamePrefix;
            if (rowsRoot == null)
            {
                rowsRoot = bodyText.transform.Find(RandomAffixRowsRootName);
                fixedAffix = false;
                expectedStateKey = "randomAffix";
                rowNamePrefix = RandomAffixRowNamePrefix;
                textNamePrefix = RandomAffixTextNamePrefix;
                iconNamePrefix = RandomAffixIconSlotNamePrefix;
            }

            if (rowsRoot == null)
            {
                return false;
            }

            if (!string.Equals(
                    stateKey ?? string.Empty,
                    expectedStateKey,
                    StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(body))
            {
                SetAuthoredAffixRowsVisibleFrom(rowsRoot, rowNamePrefix, 0, false);
                RefreshAuthoredRowsPreferredHeight(rowsRoot);
                return true;
            }

            Sprite raritySprite = ResolveAffixIconSprite(fixedAffix);
            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                string line = rawLines[lineIndex];
                if (!IsAffixIconLine(line))
                {
                    continue;
                }

                Transform row = rowsRoot.Find(rowNamePrefix + slotIndex);
                if (row == null)
                {
                    break;
                }

                Text rowText = row.Find(textNamePrefix + slotIndex)?.GetComponent<Text>();
                if (rowText != null)
                {
                    rowText.text = line.TrimStart();
                }

                row.GetComponent<ItemDetailAuthoredTextRowLayoutElement>()?.InvalidateLayout();

                Image icon = row.Find(iconNamePrefix + slotIndex)?.GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = raritySprite;
                    icon.enabled = raritySprite != null;
                    icon.gameObject.SetActive(raritySprite != null);
                }

                if (!row.gameObject.activeSelf)
                {
                    row.gameObject.SetActive(true);
                }

                slotIndex++;
            }

            SetAuthoredAffixRowsVisibleFrom(rowsRoot, rowNamePrefix, slotIndex, false);
            RefreshAuthoredRowsPreferredHeight(rowsRoot);
            return true;
        }

        private void RefreshAuthoredRowsPreferredHeight(Transform rowsRoot = null)
        {
            if (rowsRoot is RectTransform rowsRootRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rowsRootRect);
            }

            bodyText?.GetComponent<ItemDetailAuthoredRowsLayoutElement>()?.InvalidateLayout();

            if (transform is RectTransform sectionRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(sectionRect);
                if (sectionRect.parent is RectTransform contentRect)
                {
                    LayoutRebuilder.MarkLayoutForRebuild(contentRect);
                }
            }
        }

        private static void SetAuthoredAffixRowsVisibleFrom(
            Transform rowsRoot,
            string rowNamePrefix,
            int firstRow,
            bool visible)
        {
            if (rowsRoot == null)
            {
                return;
            }

            for (int index = 0; index < rowsRoot.childCount; index++)
            {
                Transform row = rowsRoot.GetChild(index);
                if (row == null || !row.name.StartsWith(rowNamePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = row.name.Substring(rowNamePrefix.Length);
                if (!int.TryParse(suffix, out int rowIndex) || rowIndex < firstRow)
                {
                    continue;
                }

                if (row.gameObject.activeSelf != visible)
                {
                    row.gameObject.SetActive(visible);
                }
            }
        }

        private Sprite ResolveAffixIconSprite(bool fixedAffix)
        {
            string raritySuffix = ResolveAffixRaritySuffix();
            string resourcePath = "item/词条icon/"
                + (fixedAffix ? "固定词条icon_" : "随机词条icon_")
                + raritySuffix;
            if (!CachedAffixIconSprites.TryGetValue(resourcePath, out Sprite sprite) || sprite == null)
            {
                sprite = Resources.Load<Sprite>(resourcePath);
                CachedAffixIconSprites[resourcePath] = sprite;
            }

            return sprite;
        }

        private string ResolveAffixRaritySuffix()
        {
            string key = (currentRarityKey + " " + currentRarityDisplayName).Trim().ToLowerInvariant();
            if (key.Contains("orange") || key.Contains("cheng") || key.Contains("道") || key.Contains("橙"))
            {
                return "道";
            }

            if (key.Contains("purple") || key.Contains("zi") || key.Contains("玄") || key.Contains("紫"))
            {
                return "玄";
            }

            if (key.Contains("blue") || key.Contains("lan") || key.Contains("灵") || key.Contains("藍")
                || key.Contains("蓝"))
            {
                return "灵";
            }

            if (key.Contains("green") || key.Contains("qing") || key.Contains("良") || key.Contains("青")
                || key.Contains("绿"))
            {
                return "良";
            }

            return "凡";
        }

        private static void SetAuthoredBaseStatRowsVisibleFrom(
            Transform rowsRoot,
            int firstRow,
            bool visible)
        {
            if (rowsRoot == null)
            {
                return;
            }

            for (int index = 0; index < rowsRoot.childCount; index++)
            {
                Transform row = rowsRoot.GetChild(index);
                if (row == null || !row.name.StartsWith(BaseStatRowNamePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = row.name.Substring(BaseStatRowNamePrefix.Length);
                if (!int.TryParse(suffix, out int rowIndex) || rowIndex < firstRow)
                {
                    continue;
                }

                if (row.gameObject.activeSelf != visible)
                {
                    row.gameObject.SetActive(visible);
                }
            }
        }

        private void ApplyBaseStatIconSlots(string stateKey, string body)
        {
            if (bodyText == null)
            {
                return;
            }

            if (!string.Equals(stateKey ?? string.Empty, "stats", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(body))
            {
                HideBaseStatIconSlotsFrom(0);
                return;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            float lineHeight = Mathf.Max(
                LineIconSlotDefaultSize,
                bodyText.fontSize * Mathf.Max(0.1f, bodyText.lineSpacing) + bodyText.fontSize * 0.25f);

            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                if (!IsBaseStatIconLine(rawLines[lineIndex]))
                {
                    continue;
                }

                Image slot = EnsureLineIconSlot(
                    BaseStatIconSlotNamePrefix,
                    slotIndex,
                    lineIndex,
                    lineHeight,
                    out _);
                if (slot != null)
                {
                    Sprite resolvedSprite = ResolveBaseStatIconSprite(rawLines[lineIndex]);
                    if (resolvedSprite != null)
                    {
                        slot.sprite = resolvedSprite;
                    }

                    if (!slot.gameObject.activeSelf)
                    {
                        slot.gameObject.SetActive(true);
                    }
                }

                slotIndex++;
            }

            HideBaseStatIconSlotsFrom(slotIndex);
        }

        private Sprite ResolveBaseStatIconSprite(string line)
        {
            string plain = StripRichTextTags(line ?? string.Empty).TrimStart();
            int colonIndex = FindStatColonIndex(plain);
            string label = colonIndex > 0 ? plain.Substring(0, colonIndex) : plain;
            if (label.Contains("阵脉", StringComparison.Ordinal))
            {
                return ResolveArrayModifierIconSprite();
            }

            string resourcePath = BaseStatIconResourcePath(label);
            if (string.IsNullOrEmpty(resourcePath))
            {
                return null;
            }

            if (!CachedBaseStatIconSprites.TryGetValue(resourcePath, out Sprite sprite) || sprite == null)
            {
                sprite = Resources.Load<Sprite>(resourcePath);
                CachedBaseStatIconSprites[resourcePath] = sprite;
            }

            return sprite;
        }

        private static string BaseStatIconResourcePath(string label)
        {
            string value = label ?? string.Empty;
            if (value.Contains("伤害", StringComparison.Ordinal))
            {
                return "item/基础属性icon/伤害";
            }

            if (value.Contains("攻击", StringComparison.Ordinal))
            {
                return "item/基础属性icon/攻击";
            }

            if (value.Contains("控制", StringComparison.Ordinal)
                || value.Contains("净化", StringComparison.Ordinal))
            {
                return "item/基础属性icon/控制";
            }

            if (value.Contains("破盾", StringComparison.Ordinal)
                || value.Contains("破势", StringComparison.Ordinal))
            {
                return "item/基础属性icon/破盾";
            }

            if (value.Contains("冷却", StringComparison.Ordinal)
                || value.Contains("持续", StringComparison.Ordinal))
            {
                return "item/基础属性icon/冷却";
            }

            if (value.Contains("耗念", StringComparison.Ordinal)
                || value.Contains("念力消耗", StringComparison.Ordinal))
            {
                return "item/基础属性icon/耗念";
            }

            if (value.Contains("护势", StringComparison.Ordinal)
                || value.Contains("回复", StringComparison.Ordinal))
            {
                return "item/基础属性icon/护势";
            }

            return string.Empty;
        }

        private void ApplyAffixIconSlots(string stateKey, string body)
        {
            if (bodyText == null)
            {
                return;
            }

            string key = stateKey ?? string.Empty;
            string activePrefix = string.Empty;
            string inactivePrefix = string.Empty;
            if (string.Equals(key, "fixedAffix", StringComparison.OrdinalIgnoreCase))
            {
                activePrefix = FixedAffixIconSlotNamePrefix;
                inactivePrefix = RandomAffixIconSlotNamePrefix;
            }
            else if (string.Equals(key, "randomAffix", StringComparison.OrdinalIgnoreCase))
            {
                activePrefix = RandomAffixIconSlotNamePrefix;
                inactivePrefix = FixedAffixIconSlotNamePrefix;
            }
            else
            {
                HideLineIconSlotsFrom(FixedAffixIconSlotNamePrefix, 0);
                HideLineIconSlotsFrom(RandomAffixIconSlotNamePrefix, 0);
                return;
            }

            HideLineIconSlotsFrom(inactivePrefix, 0);
            if (string.IsNullOrWhiteSpace(body))
            {
                HideLineIconSlotsFrom(activePrefix, 0);
                return;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            float lineHeight = Mathf.Max(
                LineIconSlotDefaultSize,
                bodyText.fontSize * Mathf.Max(0.1f, bodyText.lineSpacing) + bodyText.fontSize * 0.25f);

            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                if (!IsAffixIconLine(rawLines[lineIndex]))
                {
                    continue;
                }

                Image slot = EnsureLineIconSlot(
                    activePrefix,
                    slotIndex,
                    lineIndex,
                    lineHeight,
                    out _);
                if (slot != null && !slot.gameObject.activeSelf)
                {
                    slot.gameObject.SetActive(true);
                }

                slotIndex++;
            }

            HideLineIconSlotsFrom(activePrefix, slotIndex);
        }

        private void ApplyDetailLineIconSlots(string stateKey, string body)
        {
            string key = stateKey ?? string.Empty;
            if (string.Equals(key, "orange", StringComparison.OrdinalIgnoreCase))
            {
                HideDetailLineIconSlotsExcept(DaoTraceIconSlotNamePrefix);
                ApplyLineIconSlots(
                    DaoTraceIconSlotNamePrefix,
                    body,
                    (line, _) => IsAnyNonEmptyIconLine(line));
                return;
            }

            if (string.Equals(key, "coreEffect", StringComparison.OrdinalIgnoreCase))
            {
                HideDetailLineIconSlotsExcept(CoreEffectIconSlotNamePrefix);
                ApplyLineIconSlots(
                    CoreEffectIconSlotNamePrefix,
                    body,
                    (line, slotIndex) => slotIndex < 4 && IsAnyNonEmptyIconLine(line));
                return;
            }

            if (string.Equals(key, "famenBuild", StringComparison.OrdinalIgnoreCase))
            {
                HideDetailLineIconSlotsExcept(FaMenBuildStageIconSlotNamePrefix);
                ApplyLineIconSlots(
                    FaMenBuildStageIconSlotNamePrefix,
                    body,
                    (line, _) => IsBuildStageIconLine(line, true));
                return;
            }

            if (string.Equals(key, "qileiBuild", StringComparison.OrdinalIgnoreCase))
            {
                HideDetailLineIconSlotsExcept(QiLeiBuildStageIconSlotNamePrefix);
                ApplyLineIconSlots(
                    QiLeiBuildStageIconSlotNamePrefix,
                    body,
                    (line, _) => IsBuildStageIconLine(line, false));
                return;
            }

            if (string.Equals(key, "skillMonitor", StringComparison.OrdinalIgnoreCase))
            {
                HideDetailLineIconSlotsExcept(SkillMonitorIconSlotNamePrefix);
                ApplyLineIconSlots(
                    SkillMonitorIconSlotNamePrefix,
                    body,
                    (line, slotIndex) => slotIndex < 4 && IsAnyNonEmptyIconLine(line));
                return;
            }

            HideDetailLineIconSlotsExcept();
        }

        private void ApplyLineIconSlots(
            string slotNamePrefix,
            string body,
            Func<string, int, bool> shouldUseLine)
        {
            if (bodyText == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(body) || shouldUseLine == null)
            {
                HideLineIconSlotsFrom(slotNamePrefix, 0);
                return;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            float lineHeight = Mathf.Max(
                LineIconSlotDefaultSize,
                bodyText.fontSize * Mathf.Max(0.1f, bodyText.lineSpacing) + bodyText.fontSize * 0.25f);

            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                if (!shouldUseLine(rawLines[lineIndex], slotIndex))
                {
                    continue;
                }

                Image slot = EnsureLineIconSlot(
                    slotNamePrefix,
                    slotIndex,
                    lineIndex,
                    lineHeight,
                    out _);
                if (slot != null && !slot.gameObject.activeSelf)
                {
                    slot.gameObject.SetActive(true);
                }

                slotIndex++;
            }

            HideLineIconSlotsFrom(slotNamePrefix, slotIndex);
        }

        private void HideDetailLineIconSlotsExcept(params string[] activePrefixes)
        {
            HideLineIconSlotsFromUnlessActive(DaoTraceIconSlotNamePrefix, activePrefixes);
            HideLineIconSlotsFromUnlessActive(CoreEffectIconSlotNamePrefix, activePrefixes);
            HideLineIconSlotsFromUnlessActive(FaMenBuildStageIconSlotNamePrefix, activePrefixes);
            HideLineIconSlotsFromUnlessActive(QiLeiBuildStageIconSlotNamePrefix, activePrefixes);
            HideLineIconSlotsFromUnlessActive(SkillMonitorIconSlotNamePrefix, activePrefixes);
        }

        private void HideLineIconSlotsFromUnlessActive(string slotNamePrefix, string[] activePrefixes)
        {
            if (IsActivePrefix(slotNamePrefix, activePrefixes))
            {
                return;
            }

            HideLineIconSlotsFrom(slotNamePrefix, 0);
        }

        private static bool IsActivePrefix(string slotNamePrefix, string[] activePrefixes)
        {
            if (activePrefixes == null)
            {
                return false;
            }

            for (int index = 0; index < activePrefixes.Length; index++)
            {
                if (string.Equals(slotNamePrefix, activePrefixes[index], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private Image EnsureLineIconSlot(
            string slotNamePrefix,
            int slotIndex,
            int lineIndex,
            float lineHeight,
            out bool created)
        {
            created = false;
            if (bodyText == null)
            {
                return null;
            }

            string slotName = slotNamePrefix + slotIndex;
            Transform slotTransform = bodyText.transform.Find(slotName);
            if (slotTransform == null)
            {
                return null;
            }

            Image image = slotTransform.GetComponent<Image>();
            return image;
        }

        private void HideBaseStatIconSlotsFrom(int firstUnusedSlot)
        {
            HideLineIconSlotsFrom(BaseStatIconSlotNamePrefix, firstUnusedSlot);
        }

        private void HideLineIconSlotsFrom(string slotNamePrefix, int firstUnusedSlot)
        {
            if (bodyText == null)
            {
                return;
            }

            for (int index = 0; index < bodyText.transform.childCount; index++)
            {
                Transform child = bodyText.transform.GetChild(index);
                if (child == null
                    || !child.name.StartsWith(slotNamePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = child.name.Substring(slotNamePrefix.Length);
                if (!int.TryParse(suffix, out int slotIndex) || slotIndex < firstUnusedSlot)
                {
                    continue;
                }

                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static bool IsBaseStatIconLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string plain = StripRichTextTags(line);
            int contentIndex = 0;
            while (contentIndex < plain.Length && char.IsWhiteSpace(plain[contentIndex]))
            {
                contentIndex++;
            }

            if (contentIndex >= plain.Length)
            {
                return false;
            }

            string content = plain.Substring(contentIndex);
            return FindStatColonIndex(content) > 0;
        }

        private static bool IsAffixIconLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string plain = StripRichTextTags(line);
            return !string.IsNullOrWhiteSpace(plain);
        }

        private static bool IsAnyNonEmptyIconLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string plain = StripRichTextTags(line);
            return !string.IsNullOrWhiteSpace(plain);
        }

        private static bool IsBuildStageIconLine(string line, bool faMen)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string plain = StripRichTextTags(line).TrimStart();
            if (plain.Length == 0)
            {
                return false;
            }

            return faMen
                ? StartsWithBuildStage(plain, "2") || StartsWithBuildStage(plain, "4") || StartsWithBuildStage(plain, "6")
                : StartsWithBuildStage(plain, "2") || StartsWithBuildStage(plain, "4");
        }

        private static bool StartsWithBuildStage(string plain, string stage)
        {
            return plain.StartsWith(stage, StringComparison.Ordinal)
                || plain.StartsWith("(" + stage, StringComparison.Ordinal)
                || plain.StartsWith("（" + stage, StringComparison.Ordinal);
        }

        private void ApplyInlineArrayModifierIcons(string body)
        {
            ClearArrayModifierIcons();
            if (bodyText == null)
            {
                return;
            }

            if (ApplyAuthoredBaseStatInlineArrayModifierIcons(body))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(body)
                || !body.Contains(ArrayModifierIconToken, StringComparison.Ordinal))
            {
                return;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                string plainLine = StripRichTextTags(rawLines[lineIndex]);
                int tokenIndex = plainLine.IndexOf(ArrayModifierIconToken, StringComparison.Ordinal);
                if (tokenIndex < 0)
                {
                    continue;
                }

                bool inactiveLine = IsInactiveArrayModifierLine(rawLines[lineIndex]);
                Transform slot = bodyText.transform.Find(
                    InlineArrayModifierIconNamePrefix + arrayModifierIconImages.Count);
                Image icon = slot != null ? slot.GetComponent<Image>() : null;
                if (icon == null)
                {
                    continue;
                }

                icon.sprite = ResolveArrayModifierIconSprite();
                icon.color = inactiveLine ? ThemeBuildInactive : ThemeArrayModifier;
                icon.gameObject.SetActive(icon.sprite != null);
                arrayModifierIconImages.Add(icon);
            }
        }

        private bool ApplyAuthoredBaseStatInlineArrayModifierIcons(string body)
        {
            Transform rowsRoot = GetBaseStatRowsRoot();
            if (rowsRoot == null)
            {
                return false;
            }

            for (int index = 0; index < rowsRoot.childCount; index++)
            {
                Transform row = rowsRoot.GetChild(index);
                if (row == null || !row.name.StartsWith(BaseStatRowNamePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string suffix = row.name.Substring(BaseStatRowNamePrefix.Length);
                Transform slot = row.Find(InlineArrayModifierIconNamePrefix + suffix);
                if (slot != null && slot.gameObject.activeSelf)
                {
                    slot.gameObject.SetActive(false);
                }
            }

            if (string.IsNullOrWhiteSpace(body)
                || !body.Contains(ArrayModifierIconToken, StringComparison.Ordinal))
            {
                return true;
            }

            string[] rawLines = body.Replace("\r", string.Empty).Split('\n');
            int slotIndex = 0;
            for (int lineIndex = 0; lineIndex < rawLines.Length; lineIndex++)
            {
                string line = rawLines[lineIndex];
                if (!IsBaseStatIconLine(line))
                {
                    continue;
                }

                if (line.Contains(ArrayModifierIconToken, StringComparison.Ordinal))
                {
                    Transform row = rowsRoot.Find(BaseStatRowNamePrefix + slotIndex);
                    Image icon = row != null
                        ? row.Find(InlineArrayModifierIconNamePrefix + slotIndex)?.GetComponent<Image>()
                        : null;
                    if (icon != null)
                    {
                        icon.sprite = ResolveArrayModifierIconSprite();
                        icon.color = IsInactiveArrayModifierLine(line)
                            ? ThemeBuildInactive
                            : ThemeArrayModifier;
                        icon.gameObject.SetActive(icon.sprite != null);
                        arrayModifierIconImages.Add(icon);
                    }
                }

                slotIndex++;
            }

            return true;
        }

        private void ClearArrayModifierIcons()
        {
            foreach (Image icon in arrayModifierIconImages)
            {
                if (icon != null)
                {
                    icon.gameObject.SetActive(false);
                }
            }

            arrayModifierIconImages.Clear();
        }

        private Sprite ResolveArrayModifierIconSprite()
        {
            if (visualTheme != null && visualTheme.arrayModifierIcon != null)
            {
                return visualTheme.arrayModifierIcon;
            }

            if (cachedArrayModifierIconSprite == null)
            {
                cachedArrayModifierIconSprite = Resources.Load<Sprite>("item/阵脉icon/阵脉icon_点亮");
            }

            return cachedArrayModifierIconSprite;
        }

        private static string RemoveArrayModifierIconToken(string value)
        {
            return string.IsNullOrEmpty(value)
                ? value ?? string.Empty
                : value.Replace(ArrayModifierIconToken, "  ");
        }

        private static bool IsInactiveArrayModifierLine(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value.Contains("<color=#8A8A8A>", StringComparison.OrdinalIgnoreCase);
        }

        private static string StripRichTextTags(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new(value.Length);
            bool insideTag = false;
            foreach (char character in value)
            {
                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (character == '>')
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

        private string StyleLine(string stateKey, string line, int lineIndex)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return line ?? string.Empty;
            }

            int contentIndex = 0;
            while (contentIndex < line.Length && char.IsWhiteSpace(line[contentIndex]))
            {
                contentIndex++;
            }

            string indent = line.Substring(0, contentIndex);
            string content = line.Substring(contentIndex);
            content = RemoveGenericAffixFieldPrefix(stateKey, content);
            bool detailLineHasIcon = ShouldApplyDetailLineIconTextPadding(stateKey, content, lineIndex);
            string detailLinePadding = detailLineHasIcon ? LineIconTextPadding : string.Empty;
            if (stateKey.Contains("Divider", StringComparison.OrdinalIgnoreCase))
            {
                return indent + Rich(content, ThemeWeak, SectionDetailFontSize, false);
            }

            if (stateKey.Contains("Build", StringComparison.OrdinalIgnoreCase)
                || stateKey.Equals("skillMonitor", StringComparison.OrdinalIgnoreCase))
            {
                if (content.Contains("<color=", StringComparison.OrdinalIgnoreCase)
                    || content.Contains("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    return indent + detailLinePadding + content;
                }

                if (ContainsAny(content, "未激活", "Locked", "未计入"))
                {
                    return indent + detailLinePadding + Rich(content, ThemeBuildInactive, SectionInactiveFontSize, false);
                }

                if (ContainsAny(content, "已激活", "Monitoring"))
                {
                    return indent + detailLinePadding + Rich(content, ThemeBuildActive, SectionDetailFontSize, true);
                }

                return indent + detailLinePadding + Rich(content, ThemeBuildNearActive, SectionBodyFontSize, true);
            }

            if (ContainsAny(content, "不提供", "需要聚念石", "未点亮", "不可用", "未接入", "暂不生效"))
            {
                return indent + Rich(content, ThemeRestriction, SectionDetailFontSize, false);
            }

            if (stateKey.Equals("stats", StringComparison.OrdinalIgnoreCase))
            {
                int colonIndex = FindStatColonIndex(content);
                if (colonIndex > 0)
                {
                    string label = content.Substring(0, colonIndex + 1);
                    string value = content.Substring(colonIndex + 1);
                    bool emphasized = label.Contains("伤害", StringComparison.Ordinal)
                        || label.Contains("净化", StringComparison.Ordinal);
                    Color valueColor = emphasized ? rarityValueColor : ThemeBody;
                    int valueSize = emphasized ? SectionEmphasisFontSize : SectionBodyFontSize;
                    int hintIndex = value.IndexOf('（');
                    string mainValue = hintIndex >= 0 ? value.Substring(0, hintIndex) : value;
                    string hint = hintIndex >= 0 ? value.Substring(hintIndex) : string.Empty;
                    return indent
                        + LineIconTextPadding
                        + Rich(label, ThemeBody, SectionBodyFontSize, false)
                        + Rich(mainValue, valueColor, valueSize, emphasized)
                        + (string.IsNullOrEmpty(hint) ? string.Empty : Rich(hint, ThemeSecondaryText, SectionInactiveFontSize, false));
                }
            }

            if (stateKey.Equals("fixedAffix", StringComparison.OrdinalIgnoreCase)
                || stateKey.Equals("randomAffix", StringComparison.OrdinalIgnoreCase))
            {
                int colonIndex = FindStatColonIndex(content);
                if (colonIndex > 0)
                {
                    string label = content.Substring(0, colonIndex + 1);
                    string detail = content.Substring(colonIndex + 1);
                    return indent
                        + LineIconTextPadding
                        + Rich(label, rarityValueColor, SectionDetailFontSize, true)
                        + Rich(detail, ThemeBody, SectionDetailFontSize, false);
                }

                return indent
                    + LineIconTextPadding
                    + Rich(content, ThemeBody, SectionDetailFontSize, false);
            }

            if (stateKey.Equals("awakening", StringComparison.OrdinalIgnoreCase)
                || stateKey.Equals("coreEffect", StringComparison.OrdinalIgnoreCase)
                || stateKey.Equals("basic", StringComparison.OrdinalIgnoreCase)
                || stateKey.Contains("Affix", StringComparison.OrdinalIgnoreCase)
                || stateKey.Equals("orange", StringComparison.OrdinalIgnoreCase))
            {
                int colonIndex = FindStatColonIndex(content);
                if (colonIndex > 0)
                {
                    string label = content.Substring(0, colonIndex + 1);
                    string detail = content.Substring(colonIndex + 1);
                    return indent
                        + detailLinePadding
                        + Rich(label, rarityValueColor, SectionDetailFontSize, true)
                        + Rich(detail, ThemeBody, SectionDetailFontSize, false);
                }
            }

            int size = stateKey.Equals("trigger", StringComparison.OrdinalIgnoreCase)
                ? SectionDetailFontSize
                : SectionBodyFontSize;
            return indent + detailLinePadding + Rich(content, ThemeBody, size, false);
        }

        private static string RemoveGenericAffixFieldPrefix(string stateKey, string content)
        {
            string key = stateKey ?? string.Empty;
            bool fixedAffix = string.Equals(key, "fixedAffix", StringComparison.OrdinalIgnoreCase);
            bool randomAffix = string.Equals(key, "randomAffix", StringComparison.OrdinalIgnoreCase);
            if ((!fixedAffix && !randomAffix) || string.IsNullOrEmpty(content))
            {
                return content ?? string.Empty;
            }

            string genericPrefix = fixedAffix ? "固定词条" : "随机词条";
            if (!content.StartsWith(genericPrefix, StringComparison.Ordinal))
            {
                return content;
            }

            int colonIndex = FindStatColonIndex(content);
            return colonIndex >= 0 && colonIndex + 1 < content.Length
                ? content.Substring(colonIndex + 1).TrimStart()
                : content.Substring(genericPrefix.Length).TrimStart(' ', '·', '・', '-', '—');
        }

        private static bool ShouldApplyDetailLineIconTextPadding(string stateKey, string content, int lineIndex)
        {
            string key = stateKey ?? string.Empty;
            if (string.Equals(key, "orange", StringComparison.OrdinalIgnoreCase))
            {
                return IsAnyNonEmptyIconLine(content);
            }

            if (string.Equals(key, "coreEffect", StringComparison.OrdinalIgnoreCase))
            {
                return lineIndex >= 0 && lineIndex < 4 && IsAnyNonEmptyIconLine(content);
            }

            if (string.Equals(key, "famenBuild", StringComparison.OrdinalIgnoreCase))
            {
                return IsBuildStageIconLine(content, true);
            }

            if (string.Equals(key, "qileiBuild", StringComparison.OrdinalIgnoreCase))
            {
                return IsBuildStageIconLine(content, false);
            }

            return false;
        }

        private static int FindStatColonIndex(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }

            int fullWidth = value.IndexOf('\uFF1A');
            if (fullWidth >= 0)
            {
                return fullWidth;
            }

            return value.IndexOf(':');
        }

        private Color ThemePage => visualTheme != null ? visualTheme.pageBackground : ItemDetailVisualThemeDefaults.PageBackground;
        private Color ThemeCard => visualTheme != null ? visualTheme.cardBackground : ItemDetailVisualThemeDefaults.CardBackground;
        private Color ThemeSecondaryCard => visualTheme != null ? visualTheme.secondaryCardBackground : ItemDetailVisualThemeDefaults.SecondaryCardBackground;
        private Color ThemeTitle => visualTheme != null ? visualTheme.titleText : ItemDetailVisualThemeDefaults.TitleText;
        private Color ThemeBody => visualTheme != null ? visualTheme.bodyText : ItemDetailVisualThemeDefaults.BodyText;
        private Color ThemeSecondaryText => visualTheme != null ? visualTheme.secondaryText : ItemDetailVisualThemeDefaults.SecondaryText;
        private Color ThemeWeak => visualTheme != null ? visualTheme.weakText : ItemDetailVisualThemeDefaults.WeakText;
        private Color ThemeRestriction => visualTheme != null ? visualTheme.restrictionText : ItemDetailVisualThemeDefaults.RestrictionText;
        private Color ThemeBuildActive => visualTheme != null ? visualTheme.buildActive : ItemDetailVisualThemeDefaults.BuildActive;
        private Color ThemeBuildNearActive => visualTheme != null ? visualTheme.buildNearActive : ItemDetailVisualThemeDefaults.BuildNearActive;
        private Color ThemeBuildInactive => visualTheme != null ? visualTheme.buildInactive : ItemDetailVisualThemeDefaults.BuildInactive;
        private Color ThemeArrayModifier => visualTheme != null ? visualTheme.arrayModifierColor : ItemDetailVisualThemeDefaults.ArrayModifierColor;
        private Color ThemeRarityOrange => visualTheme != null ? visualTheme.rarityOrange : ItemDetailVisualThemeDefaults.RarityOrange;

        private static string Rich(string text, Color color, int size, bool bold)
        {
            string value = bold ? $"<b>{text}</b>" : text;
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{value}</color>";
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrEmpty(value) || tokens == null)
            {
                return false;
            }

            foreach (string token in tokens)
            {
                if (!string.IsNullOrEmpty(token) && value.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#if false // ITEMDETAIL_LEGACY_AUTO_PREVIEW_DISABLED: replaced by explicit manual-only authoring.
namespace TalismanBag.EditorTools.ItemSandbox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TalismanBag.ItemSandbox;
    using TalismanBag.Items.Detail;
    using TalismanBag.Items.Detail.UI;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using Object = UnityEngine.Object;

    [InitializeOnLoad]
    internal static class ItemDetailPanelEditModeAuthoringPreviewFromSectionView
    {
        private const string ItemSandboxScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string ItemDetailPanelPrefabPath = "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        private const string FullDaoPreviewMenuPath = "TalismanBag/Item Sandbox/Show Full Dao Detail Edit Preview";
        private const string ArrayModifierIconToken = "[Icon_ArrayVeinModifier]";

        static ItemDetailPanelEditModeAuthoringPreviewFromSectionView()
        {
            EditorApplication.delayCall += ApplyOpenContextPreview;
            EditorSceneManager.sceneOpened += (_, _) => EditorApplication.delayCall += ApplyOpenContextPreview;
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    EditorApplication.delayCall += ApplyOpenContextPreview;
                }
            };
        }

        [MenuItem(FullDaoPreviewMenuPath)]
        private static void ApplyMenuPreview()
        {
            ApplyOpenContextPreview(force: true);
        }

        private static void ApplyOpenContextPreview()
        {
            ApplyOpenContextPreview(force: false);
        }

        private static void ApplyOpenContextPreview(bool force)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            List<ItemDetailPanelView> targets = FindAuthoringTargets();
            if (targets.Count == 0)
            {
                return;
            }

            ItemDetailViewModel preview = BuildFullDaoPreviewModel();
            if (preview == null)
            {
                return;
            }

            foreach (ItemDetailPanelView view in targets)
            {
                if (view == null)
                {
                    continue;
                }

                SetVisibleForAuthoring(view.transform);
                view.Bind(preview.Clone());
                view.ShowPlayerDetailTab();
                UnlockDetailContentManualWidths(view.transform);
                RebuildLayout(view.transform as RectTransform);
                EditorUtility.SetDirty(view);
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && string.Equals(NormalizePath(activeScene.path), ItemSandboxScenePath, StringComparison.Ordinal))
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            if (force)
            {
                Debug.Log($"[ItemDetailPanelEditModeAuthoringPreview] Applied full Dao authoring preview to {targets.Count} ItemDetailPanelView object(s).");
            }
        }

        private static List<ItemDetailPanelView> FindAuthoringTargets()
        {
            List<ItemDetailPanelView> targets = new();
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null
                && string.Equals(NormalizePath(prefabStage.assetPath), ItemDetailPanelPrefabPath, StringComparison.Ordinal))
            {
                targets.AddRange(prefabStage.prefabContentsRoot.GetComponentsInChildren<ItemDetailPanelView>(true));
                return targets;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !string.Equals(NormalizePath(activeScene.path), ItemSandboxScenePath, StringComparison.Ordinal))
            {
                return targets;
            }

            targets.AddRange(Object.FindObjectsOfType<ItemDetailPanelView>(true)
                .Where(view => view != null && view.gameObject.scene == activeScene));
            return targets;
        }

        private static ItemDetailViewModel BuildFullDaoPreviewModel()
        {
            ItemDetailViewModel realPreview = TryBuildFromCurrentSandboxProviders();
            ItemDetailViewModel fullAuthoringPreview = BuildFallbackFullDaoPreviewModel();
            if (realPreview == null)
            {
                return fullAuthoringPreview;
            }

            realPreview.displayPlayerSections = fullAuthoringPreview.displayPlayerSections;
            realPreview.displayDebugSections = fullAuthoringPreview.displayDebugSections;
            realPreview.statusFlags = fullAuthoringPreview.statusFlags;
            return realPreview;
        }

        private static ItemDetailViewModel TryBuildFromCurrentSandboxProviders()
        {
            ItemBalanceCandidateDetailSandboxProvider provider = FindCurrentSceneProvider();
            if (provider == null || provider.WorkbenchCatalog == null)
            {
                return null;
            }

            try
            {
                ItemBalanceCandidateDetailSandboxAdapter candidateAdapter =
                    new(provider.WorkbenchCatalog, provider);
                ItemFullDetailBuildSandboxWorkbenchSession session =
                    new(candidateAdapter, provider);
                session.SetSandboxLevel(40);
                if (session.LoadValidationLayout(out _))
                {
                    session.SelectMainBuild("famen:zhenlei", "EditModeAuthoringPreview");
                    ItemFullDetailWorkbenchInstance selected = session.Instances.FirstOrDefault(instance =>
                            string.Equals(instance.baseItemId, "I001", StringComparison.Ordinal))
                        ?? session.Instances.FirstOrDefault();
                    if (selected != null)
                    {
                        session.SelectInstance(selected.itemInstanceId);
                    }

                    ItemDetailViewModel placedPreview = session.BuildSelectedDetail();
                    if (placedPreview != null)
                    {
                        return placedPreview;
                    }
                }

                return session.PreviewCandidate("I001", "orange", provider.RootSeed);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[ItemDetailPanelEditModeAuthoringPreview] Falling back to static Dao preview. " + exception.Message);
                return null;
            }
        }

        private static ItemBalanceCandidateDetailSandboxProvider FindCurrentSceneProvider()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return null;
            }

            return Object.FindObjectsOfType<ItemBalanceCandidateDetailSandboxProvider>(true)
                .FirstOrDefault(provider => provider != null && provider.gameObject.scene == activeScene);
        }

        private static ItemDetailViewModel BuildFallbackFullDaoPreviewModel()
        {
            return new ItemDetailViewModel
            {
                itemId = "I001",
                baseItemId = "I001",
                itemInstanceId = "EDIT_MODE_DAO_PREVIEW_I001",
                placementId = "EDIT_MODE_AUTHORING_PREVIEW",
                rarityKey = "orange",
                rarityColorKey = "orange",
                displayItemName = "五雷急令",
                displayRarityName = "道品",
                displayFaMenName = "震雷法",
                displayQiLeiName = "令",
                displayShapeName = "竖排三格",
                displayItemPower = "999",
                iconPlaceholderKey = "EDIT_MODE_DAO_PREVIEW",
                statusFlags = new ItemDetailStatusFlags
                {
                    placementId = "EDIT_MODE_AUTHORING_PREVIEW",
                    isLit = true,
                    isDirectLit = true,
                    isOnArrayBonusCell = true,
                    isArrayBonusActive = true,
                    countedInBuild = true,
                    itemLevel = 40,
                    inputLevel = 40,
                    resolvedLevel = 40,
                    unlockedCoreEffectCount = 4,
                    activeCoreEffectCount = 4
                },
                displayPlayerSections = new List<ItemDetailSectionViewModel>
                {
                    Section("基础属性", "  伤害：128\n  控制强度：27点\n  破盾：18%\n  念力返还：4点 " + ArrayModifierIconToken + " +2点\n  冷却：3.2秒", "stats"),
                    Section("固定词条", "  控制强度增加+7点\n  引雷伤害+18点 " + ArrayModifierIconToken + " +2点", "fixedAffix"),
                    Section("随机词条", "  触发后返还念力+4点\n  破盾提高+5%", "randomAffix"),
                    Section("道痕", "  道痕/终极核心：雷痕贯壳，破盾提高+12%", "orange"),
                    Divider("smallDividerBeforeCore"),
                    Section("核心效果", "  震雷符·初识：“引雷”的基础效果获得第一层强化，破盾提高+5%\n  震雷符·入门：引雷命中后追加雷痕，伤害+12点\n  震雷符·贯通：雷痕目标受到额外击穿，控制强度+7点\n  震雷符·终式：五雷破壳触发时，返还念力+4点", "coreEffect"),
                    Divider("largeDividerBeforeBuild"),
                    Section("法门构筑", "  九霄雷君的敕令\n  法门构筑：6/6\n  -五雷急令\n  -照壳雷镜\n  -伏雷法印\n  -鸣雷符箓\n  -电母令文\n  -破壳雷章\n  2件效果：\n  雷痕效果提高；破盾提高+5%\n  4件效果：\n  震雷击穿提高；控制强度+7点\n  6件效果：\n  五雷破壳生效；伤害+18点", "famenBuild"),
                    Divider("smallDividerBetweenBuilds"),
                    Section("器类构筑", "  令类构筑：4/4\n  2件效果：\n  令势相合；念力返还+4点\n  4件效果：\n  令文成局；额外触发+1次 " + ArrayModifierIconToken + " +1次", "qileiBuild"),
                    Divider("largeDividerBeforePlacement"),
                    Section("推荐摆放", "  推荐靠近阵眼或阵脉格；已占阵脉格时显示专色加成。", "placement"),
                    Section("旧物日记", "  旧木匣里压着一张被雷火烫卷的急令，背面还有林照灯早年写下的借雷口诀。", "flavor"),
                    Hidden("trigger"),
                    Hidden("basic")
                },
                displayDebugSections = new List<ItemDetailSectionViewModel>
                {
                    Section("Debug Identity", "editModePreview: true\nsource: fallback", "debugIdentity"),
                    Section("Debug Build", "selectedMainBuildId: famen:zhenlei", "debugBuild"),
                    Section("Debug Validation", "BALANCE_CANDIDATE\nNOT_LIVE_LOCKED", "debugValidation")
                }
            };
        }

        private static ItemDetailSectionViewModel Section(string title, string body, string stateKey)
        {
            return new ItemDetailSectionViewModel(title, body, stateKey, true);
        }

        private static ItemDetailSectionViewModel Divider(string stateKey)
        {
            return new ItemDetailSectionViewModel(string.Empty, string.Empty, stateKey, true);
        }

        private static ItemDetailSectionViewModel Hidden(string stateKey)
        {
            return new ItemDetailSectionViewModel(string.Empty, string.Empty, stateKey, false);
        }

        private static void SetVisibleForAuthoring(Transform target)
        {
            Transform current = target;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                    EditorUtility.SetDirty(current.gameObject);
                }

                current = current.parent;
            }
        }

        private static void UnlockDetailContentManualWidths(Transform root)
        {
            if (root == null)
            {
                return;
            }

            UnlockScrollContentManualWidths(root.Find("ItemDetailScrollView/Viewport/DetailContent"));
            UnlockScrollContentManualWidths(root.Find("ItemDebugScrollView/Viewport/DebugContent"));
        }

        private static void UnlockScrollContentManualWidths(Transform content)
        {
            if (content == null)
            {
                return;
            }

            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>();
            if (contentLayout != null)
            {
                contentLayout.childControlWidth = false;
                contentLayout.childForceExpandWidth = false;
                EditorUtility.SetDirty(contentLayout);
            }

            ContentSizeFitter contentFitter = content.GetComponent<ContentSizeFitter>();
            if (contentFitter != null)
            {
                contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                EditorUtility.SetDirty(contentFitter);
            }

            RectTransform contentRect = content as RectTransform;
            float fallbackWidth = ResolveFallbackWidth(contentRect);
            ConvertHorizontalStretchToManualWidth(contentRect, fallbackWidth);

            RectTransform[] descendants = content.GetComponentsInChildren<RectTransform>(true);
            foreach (RectTransform rectTransform in descendants)
            {
                if (rectTransform == null || rectTransform == contentRect)
                {
                    continue;
                }

                ConvertHorizontalStretchToManualWidth(rectTransform, fallbackWidth);
            }
        }

        private static float ResolveFallbackWidth(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0f;
            }

            float width = rectTransform.rect.width;
            if (width > 0f)
            {
                return width;
            }

            RectTransform parent = rectTransform.parent as RectTransform;
            return parent != null ? Mathf.Max(0f, parent.rect.width) : 0f;
        }

        private static void ConvertHorizontalStretchToManualWidth(RectTransform rectTransform, float fallbackWidth)
        {
            if (rectTransform == null
                || Mathf.Approximately(rectTransform.anchorMin.x, rectTransform.anchorMax.x))
            {
                return;
            }

            float width = rectTransform.rect.width;
            if (width <= 0f)
            {
                width = fallbackWidth;
            }

            float left = rectTransform.offsetMin.x;
            rectTransform.anchorMin = new Vector2(0f, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(0f, rectTransform.anchorMax.y);
            rectTransform.pivot = new Vector2(0f, rectTransform.pivot.y);
            rectTransform.anchoredPosition = new Vector2(left, rectTransform.anchoredPosition.y);
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            EditorUtility.SetDirty(rectTransform);
        }

        private static void RebuildLayout(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }
    }
}
#endif
