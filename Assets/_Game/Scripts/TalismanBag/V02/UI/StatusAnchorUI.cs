using System.Collections.Generic;
using TalismanBag.V02.Status;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TalismanBag.V02.UI
{
    public sealed class StatusAnchorUI : MonoBehaviour
    {
        private const string StatusIconNamePrefix = "StatusIcon_";
        private const string IconImageName = "Icon";
        private const string CountdownFillName = "CountdownFill";
        private const string GlyphTextName = "Glyph";
        private const string StackTextName = "Stack";
        private const string CountdownTextName = "Countdown";

        [SerializeField] private StatusEffectController controller;
        [SerializeField] private RectTransform iconRoot;
        [SerializeField] private StatusTooltipPanel tooltipPanel;
        [SerializeField] private int maxVisibleIcons = 6;
        [SerializeField] private Vector2 iconSize = new(42f, 42f);
        [SerializeField] private float spacing = 6f;
        [SerializeField] private TextAnchor iconAlignment = TextAnchor.MiddleLeft;
        [SerializeField] private bool filterByPolarity;
        [SerializeField] private StatusPolarity polarityFilter = StatusPolarity.Debuff;

        private readonly List<StatusIconView> iconViews = new();
        private Font font;

        public StatusEffectController Controller => controller;
        public RectTransform IconRoot => iconRoot;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EditorApplication.delayCall -= EnsureEditorPlaceholders;
            EditorApplication.delayCall += EnsureEditorPlaceholders;
        }

        private void EnsureEditorPlaceholders()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            // Manual UI targets must exist before Play and keep designer-placed rects.
            EnsureIconRoot();
            EnsureIconCount(Mathf.Max(1, maxVisibleIcons));
            HideAll();
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
#endif

        private void Awake()
        {
            EnsureIconRoot();
            EnsureIconCount(Mathf.Max(1, maxVisibleIcons));
            Bind(controller);
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.StatusesChanged -= Refresh;
            }
        }

        private void Update()
        {
            foreach (StatusIconView view in iconViews)
            {
                if (view != null && view.gameObject.activeSelf)
                {
                    view.Refresh();
                }
            }
        }

        public void Bind(StatusEffectController nextController)
        {
            if (controller == nextController)
            {
                Refresh();
                return;
            }

            if (controller != null)
            {
                controller.StatusesChanged -= Refresh;
            }

            controller = nextController;
            if (controller != null)
            {
                controller.StatusesChanged += Refresh;
            }

            Refresh();
        }

        public void BindTooltip(StatusTooltipPanel tooltip)
        {
            tooltipPanel = tooltip;
            foreach (StatusIconView view in iconViews)
            {
                if (view != null)
                {
                    view.SetTooltipPanel(tooltipPanel);
                }
            }
        }

        public void SetIconAlignment(TextAnchor alignment)
        {
            iconAlignment = alignment;
            ApplyLayoutSettings();
        }

        public void SetPolarityFilter(bool enabled, StatusPolarity polarity)
        {
            filterByPolarity = enabled;
            polarityFilter = polarity;
            Refresh();
        }

        public Vector2 GetFloatingTextPosition(RectTransform feedbackRoot)
        {
            RectTransform rect = iconRoot != null ? iconRoot : GetComponent<RectTransform>();
            if (rect == null || feedbackRoot == null)
            {
                return Vector2.zero;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(feedbackRoot, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }

        public void Refresh()
        {
            EnsureIconRoot();
            if (controller == null)
            {
                HideAll();
                return;
            }

            List<StatusEffectRuntime> statuses = GetFilteredStatuses();
            int max = Mathf.Max(1, maxVisibleIcons);
            int normalCount = statuses.Count > max ? max - 1 : statuses.Count;
            EnsureIconCount(max);

            for (int i = 0; i < iconViews.Count; i++)
            {
                StatusIconView view = iconViews[i];
                if (view == null)
                {
                    continue;
                }

                if (i < normalCount)
                {
                    view.gameObject.SetActive(true);
                    view.Bind(statuses[i]);
                }
                else if (statuses.Count > max && i == normalCount)
                {
                    view.gameObject.SetActive(true);
                    view.BindOverflow(statuses.Count - normalCount);
                }
                else
                {
                    view.Clear();
                }
            }
        }

        private void HideAll()
        {
            foreach (StatusIconView view in iconViews)
            {
                view?.Clear();
            }
        }

        private List<StatusEffectRuntime> GetFilteredStatuses()
        {
            List<StatusEffectRuntime> statuses = controller.GetVisibleStatuses();
            if (!filterByPolarity)
            {
                return statuses;
            }

            statuses.RemoveAll(status => status?.definition == null || status.definition.polarity != polarityFilter);
            return statuses;
        }

        private void EnsureIconRoot()
        {
            if (iconRoot == null)
            {
                RectTransform rect = GetComponent<RectTransform>();
                if (rect == null)
                {
                    rect = gameObject.AddComponent<RectTransform>();
                }

                iconRoot = rect;
            }

            ApplyLayoutSettings();
        }

        private void ApplyLayoutSettings()
        {
            if (iconRoot == null)
            {
                return;
            }

            if (iconRoot.GetComponent<HorizontalLayoutGroup>() == null)
            {
                iconRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            HorizontalLayoutGroup layout = iconRoot.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = iconAlignment;
            layout.spacing = spacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void EnsureIconCount(int count)
        {
            RefreshIconViewCache();

            while (iconViews.Count < count)
            {
                iconViews.Add(CreateIconView(iconViews.Count));
            }

            for (int i = 0; i < iconViews.Count; i++)
            {
                if (iconViews[i] != null)
                {
                    ConfigureIconView(iconViews[i], i, false);
                }
            }
        }

        private void RefreshIconViewCache()
        {
            iconViews.Clear();
            if (iconRoot == null)
            {
                return;
            }

            StatusIconView[] existingViews = iconRoot.GetComponentsInChildren<StatusIconView>(true);
            List<StatusIconView> directChildren = new(existingViews.Length);
            foreach (StatusIconView view in existingViews)
            {
                if (view != null && view.transform.parent == iconRoot)
                {
                    directChildren.Add(view);
                }
            }

            directChildren.Sort((left, right) => left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex()));
            iconViews.AddRange(directChildren);
        }

        private StatusIconView CreateIconView(int index)
        {
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            GameObject iconObject = new($"StatusIcon_{index + 1}", typeof(RectTransform), typeof(LayoutElement), typeof(Image), typeof(StatusIconView));
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(iconObject, "Create status icon placeholder");
            }
#endif
            iconObject.transform.SetParent(iconRoot, false);

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.sizeDelta = iconSize;

            LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = iconSize.x;
            layoutElement.preferredHeight = iconSize.y;

            StatusIconView view = iconObject.GetComponent<StatusIconView>();
            ConfigureIconView(view, index, true);
            view.Clear();
            return view;
        }

        private void ConfigureIconView(StatusIconView view, int index, bool createdIcon)
        {
            if (view == null)
            {
                return;
            }

            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            GameObject iconObject = view.gameObject;
            if (createdIcon)
            {
                iconObject.name = $"{StatusIconNamePrefix}{index + 1}";
            }

            RectTransform rect = iconObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = iconObject.AddComponent<RectTransform>();
            }

            if (createdIcon)
            {
                rect.sizeDelta = iconSize;
            }

            LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = iconObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = iconSize.x;
            layoutElement.preferredHeight = iconSize.y;

            Image background = iconObject.GetComponent<Image>();
            if (background == null)
            {
                background = iconObject.AddComponent<Image>();
            }

            background.color = new Color(0.18f, 0.2f, 0.18f, 0.92f);

            Image iconImage = GetOrCreateImage(IconImageName, iconObject.transform, out bool iconImageCreated);
            if (iconImageCreated)
            {
                RectTransform imageRect = iconImage.rectTransform;
                imageRect.anchorMin = Vector2.zero;
                imageRect.anchorMax = Vector2.one;
                imageRect.offsetMin = new Vector2(6f, 6f);
                imageRect.offsetMax = new Vector2(-6f, -6f);
            }

            iconImage.enabled = false;

            Image countdownFill = GetOrCreateImage(CountdownFillName, iconObject.transform, out bool countdownFillCreated);
            if (countdownFillCreated)
            {
                RectTransform countdownFillRect = countdownFill.rectTransform;
                countdownFillRect.anchorMin = Vector2.zero;
                countdownFillRect.anchorMax = Vector2.one;
                countdownFillRect.offsetMin = new Vector2(2f, 2f);
                countdownFillRect.offsetMax = new Vector2(-2f, -2f);
            }

            countdownFill.color = new Color(1f, 1f, 1f, 0.28f);
            countdownFill.type = Image.Type.Filled;
            countdownFill.fillMethod = Image.FillMethod.Radial360;
            countdownFill.fillOrigin = (int)Image.Origin360.Top;
            countdownFill.fillClockwise = true;
            countdownFill.fillAmount = 0f;
            countdownFill.raycastTarget = false;
            countdownFill.enabled = false;

            Text glyph = GetOrCreateText(GlyphTextName, iconObject.transform, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, out bool glyphCreated);
            if (glyphCreated)
            {
                Stretch(glyph.rectTransform, Vector2.zero);
            }

            Text stack = GetOrCreateText(StackTextName, iconObject.transform, 13, FontStyle.Bold, Color.white, TextAnchor.UpperRight, out bool stackCreated);
            if (stackCreated)
            {
                Stretch(stack.rectTransform, new Vector2(-3f, -1f));
            }

            Text countdown = GetOrCreateText(CountdownTextName, iconObject.transform, 12, FontStyle.Bold, Color.white, TextAnchor.LowerCenter, out bool countdownCreated);
            if (countdownCreated)
            {
                RectTransform countdownRect = countdown.rectTransform;
                countdownRect.anchorMin = new Vector2(0f, 0f);
                countdownRect.anchorMax = new Vector2(1f, 0f);
                countdownRect.pivot = new Vector2(0.5f, 0f);
                countdownRect.anchoredPosition = new Vector2(0f, 2f);
                countdownRect.sizeDelta = new Vector2(-4f, 15f);
            }

            view.Setup(background, iconImage, countdownFill, glyph, stack, countdown, tooltipPanel);
        }

        private Image GetOrCreateImage(string name, Transform parent, out bool created)
        {
            created = false;
            Transform child = parent.Find(name);
            GameObject imageObject;
            if (child == null)
            {
                imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(imageObject, "Create status icon child");
                }
#endif
                imageObject.transform.SetParent(parent, false);
                created = true;
            }
            else
            {
                imageObject = child.gameObject;
            }

            Image image = imageObject.GetComponent<Image>();
            if (image == null)
            {
                image = imageObject.AddComponent<Image>();
            }

            image.raycastTarget = false;
            return image;
        }

        private Text GetOrCreateText(string name, Transform parent, int size, FontStyle style, Color color, TextAnchor anchor, out bool created)
        {
            created = false;
            Transform child = parent.Find(name);
            GameObject textObject;
            if (child == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(textObject, "Create status icon text");
                }
#endif
                textObject.transform.SetParent(parent, false);
                created = true;
            }
            else
            {
                textObject = child.gameObject;
            }

            Text text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            if (text.font == null)
            {
                text.font = font;
            }

            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 offset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offset;
            rect.offsetMax = -offset;
        }
    }
}
