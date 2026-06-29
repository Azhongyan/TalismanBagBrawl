using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class HomeHotspotView : MonoBehaviour
    {
        private static readonly HashSet<string> WarnedMissingKeys = new();

        [SerializeField] private Image background;
        [SerializeField] private Image visualImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text stateText;
        [SerializeField] private Button button;

        private HomeHotspotConfig config;
        private Action<HomeHotspotConfig> clickCallback;
        private bool buttonBound;

        public HomeHotspotConfig Config => config;

        private void OnDestroy()
        {
            if (buttonBound)
            {
                button?.onClick.RemoveListener(InvokeClick);
            }
        }

        public void Bind(HomeHotspotConfig value, Action<HomeHotspotConfig> onClick)
        {
            config = value;
            clickCallback = onClick;
            BindButton();

            if (config == null || config.state == HomeHotspotState.Hidden)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            name = $"HomeHotspot_{config.hotspotId}";
            SetText(titleText, config.displayName);
            SetText(stateText, GetStateLabel(config.state));
            ApplyStateStyle(config.state);
            ApplySpriteIfAvailable(visualImage, config.visualKey);
            ApplySpriteIfAvailable(iconImage, config.iconKey);
        }

        public static HomeHotspotView CreateRuntime(
            Transform parent,
            Vector2 position,
            Vector2 size,
            bool compact = false)
        {
            GameObject root = new("HomeHotspot", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            root.transform.SetParent(parent, false);

            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image background = root.GetComponent<Image>();
            background.color = new Color(0.24f, 0.27f, 0.26f, 0.98f);

            Outline outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(0.58f, 0.62f, 0.56f, 0.82f);
            outline.effectDistance = new Vector2(2f, -2f);

            HomeHotspotView view = root.AddComponent<HomeHotspotView>();
            view.background = background;
            view.button = root.GetComponent<Button>();

            float visualHeight = compact ? size.y - 18f : Mathf.Max(36f, size.y * 0.42f);
            view.visualImage = CreateImage(
                "VisualFallback",
                root.transform,
                new Vector2(0f, -8f),
                new Vector2(size.x - 18f, visualHeight),
                new Color(0.18f, 0.2f, 0.2f, 1f));
            view.iconImage = CreateImage(
                "IconFallback",
                root.transform,
                new Vector2(-(size.x * 0.5f) + 22f, compact ? -9f : -(visualHeight + 12f)),
                new Vector2(compact ? 18f : 24f, compact ? 18f : 24f),
                new Color(0.44f, 0.47f, 0.45f, 1f),
                new Vector2(0f, 1f));

            view.titleText = CreateText(
                "Title",
                root.transform,
                string.Empty,
                compact ? 19 : 22,
                FontStyle.Bold,
                Color.white,
                compact ? new Vector2(8f, -4f) : new Vector2(12f, -(visualHeight + 7f)),
                compact ? new Vector2(size.x - 18f, size.y - 8f) : new Vector2(size.x - 24f, 34f),
                compact ? TextAnchor.MiddleCenter : TextAnchor.UpperCenter);

            view.stateText = CreateText(
                "State",
                root.transform,
                string.Empty,
                compact ? 14 : 15,
                FontStyle.Normal,
                new Color(0.82f, 0.84f, 0.8f),
                compact ? new Vector2(0f, -(size.y - 22f)) : new Vector2(0f, -(size.y - 25f)),
                new Vector2(size.x - 18f, 24f),
                TextAnchor.MiddleCenter);

            view.BindButton();
            return view;
        }

        private void BindButton()
        {
            if (buttonBound || button == null)
            {
                return;
            }

            button.onClick.AddListener(InvokeClick);
            buttonBound = true;
        }

        private void InvokeClick()
        {
            if (config == null || config.state == HomeHotspotState.Hidden)
            {
                return;
            }

            clickCallback?.Invoke(config);
        }

        private void ApplyStateStyle(HomeHotspotState state)
        {
            if (button != null)
            {
                button.interactable = true;
            }
        }

        private static void ApplySpriteIfAvailable(Image image, string key)
        {
            if (image == null)
            {
                return;
            }

            Sprite sprite = null;
            string safeKey = string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim();
            if (!string.IsNullOrEmpty(safeKey))
            {
                sprite = Resources.Load<Sprite>(safeKey);
                if (sprite == null && WarnedMissingKeys.Add(safeKey))
                {
                    Debug.LogWarning($"[MainHome] Missing visual resource '{safeKey}'. Greybox fallback is used.");
                }
            }

            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }

        private static string GetStateLabel(HomeHotspotState state)
        {
            return state switch
            {
                HomeHotspotState.Available => "可前往",
                HomeHotspotState.Locked => "未解锁",
                HomeHotspotState.ComingSoon => "即将开放",
                _ => string.Empty
            };
        }

        private static Image CreateImage(
            string objectName,
            Transform parent,
            Vector2 position,
            Vector2 size,
            Color color,
            Vector2? pivot = null)
        {
            GameObject imageObject = new(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            Vector2 safePivot = pivot ?? new Vector2(0.5f, 1f);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = safePivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color,
            Vector2 position,
            Vector2 size,
            TextAnchor alignment)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value ?? string.Empty;
            }
        }
    }
}
