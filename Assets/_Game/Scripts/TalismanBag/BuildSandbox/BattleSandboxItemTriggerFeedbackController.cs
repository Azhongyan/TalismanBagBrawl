using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BattleSandboxItemTriggerFeedbackController : MonoBehaviour
    {
        public const string PackageName = "V0.4-BattleSandboxItemTriggerFeedbackFx01";

        private const string FxLayerName = "BattleSandboxItemTriggerFeedbackFxLayer";
        private const float TargetPulseDuration = 0.28f;
        private const float FlashDuration = 0.36f;
        private const float VfxDuration = 0.48f;
        private const float FloatingDuration = 0.86f;
        private const float PassiveFeedbackGlobalInterval = 1.45f;
        private const float PassiveFeedbackItemInterval = 2.6f;
        private const float ActiveSuppressesPassiveSeconds = 0.35f;

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;

        private readonly List<GameObject> transientObjects = new();
        private readonly Dictionary<RectTransform, Vector3> pulsedTargetBaseScales = new();
        private readonly Dictionary<string, float> passiveFeedbackLastPlayTimes = new();
        private BuildGridInteractionPreviewController gridController;
        private Font runtimeFont;
        private float lastActiveFeedbackTime = -100f;
        private float lastPassiveFeedbackTime = -100f;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;

        public void Bind(BuildGridInteractionPreviewController controller)
        {
            gridController = controller;
        }

        public void Play(BattleSandboxRuntimeLoopRow row)
        {
            if (row == null
                || !row.playsBoardItemTriggerFeedback
                || gridController == null
                || string.IsNullOrWhiteSpace(row.boardItemTriggerFeedbackTextChinese))
            {
                return;
            }

            if (!gridController.TryResolveBoardItemFeedbackAnchor(
                    row.itemId,
                    row.boardItemTriggerOccupiedCells,
                    out RectTransform itemArtworkRect,
                    out RectTransform feedbackLayer,
                    out Vector2 anchoredPosition,
                    out Vector2 sizeDelta))
            {
                return;
            }

            bool isPassive = IsPassiveChannel(row.boardItemTriggerFeedbackChannel);
            if (isPassive && !TryReservePassiveFeedbackSlot(row))
            {
                return;
            }

            if (!isPassive)
            {
                lastActiveFeedbackTime = Time.time;
            }

            Color color = ResolveColor(row.boardItemTriggerFeedbackKind);
            RectTransform fxLayer = EnsureFxLayer(feedbackLayer);
            if (fxLayer == null)
            {
                return;
            }

            if (itemArtworkRect != null)
            {
                StartCoroutine(PulseTarget(itemArtworkRect, isPassive));
                StartCoroutine(SpawnFlashOnTarget(itemArtworkRect, color, isPassive));
            }
            else
            {
                StartCoroutine(SpawnFlashAtAnchor(fxLayer, anchoredPosition, sizeDelta, color, isPassive));
            }

            StartCoroutine(SpawnSkillVfx(fxLayer, anchoredPosition, sizeDelta, color, row.boardItemTriggerFeedbackKind, isPassive));
            StartCoroutine(SpawnFloatingText(
                fxLayer,
                anchoredPosition,
                sizeDelta,
                row.boardItemTriggerFeedbackTextChinese,
                color,
                isPassive));
        }

        public void ClearAll()
        {
            StopAllCoroutines();
            foreach (KeyValuePair<RectTransform, Vector3> pair in pulsedTargetBaseScales)
            {
                if (pair.Key != null)
                {
                    pair.Key.localScale = pair.Value;
                }
            }

            pulsedTargetBaseScales.Clear();
            passiveFeedbackLastPlayTimes.Clear();
            lastActiveFeedbackTime = -100f;
            lastPassiveFeedbackTime = -100f;
            for (int i = transientObjects.Count - 1; i >= 0; i--)
            {
                DestroyTransient(transientObjects[i]);
            }

            transientObjects.Clear();
        }

        private IEnumerator PulseTarget(RectTransform target, bool isPassive)
        {
            if (target == null)
            {
                yield break;
            }

            if (!pulsedTargetBaseScales.TryGetValue(target, out Vector3 baseScale))
            {
                baseScale = target.localScale;
                pulsedTargetBaseScales[target] = baseScale;
            }

            float elapsed = 0f;
            float duration = isPassive ? TargetPulseDuration * 0.78f : TargetPulseDuration;
            float intensity = isPassive ? 0.09f : 0.18f;
            while (elapsed < duration && target != null)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float punch = Mathf.Sin(normalized * Mathf.PI) * intensity;
                target.localScale = baseScale * (1f + punch);
                yield return null;
            }

            if (target != null)
            {
                target.localScale = baseScale;
            }

            pulsedTargetBaseScales.Remove(target);
        }

        private IEnumerator SpawnFlashOnTarget(RectTransform target, Color color, bool isPassive)
        {
            if (target == null)
            {
                yield break;
            }

            GameObject obj = CreateUiObject("ItemTriggerFlash", target);
            RectTransform rect = obj.GetComponent<RectTransform>();
            StretchToParent(rect);
            Image image = obj.AddComponent<Image>();
            image.raycastTarget = false;
            StartCoroutine(AnimateFlash(obj, rect, image, color, isPassive));
            yield return null;
        }

        private IEnumerator SpawnFlashAtAnchor(
            RectTransform layer,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color,
            bool isPassive)
        {
            GameObject obj = CreateUiObject("ItemTriggerFallbackFlash", layer);
            RectTransform rect = obj.GetComponent<RectTransform>();
            SetTopLeftAnchor(rect);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = ResolveSafeSize(sizeDelta, 72f);
            Image image = obj.AddComponent<Image>();
            image.raycastTarget = false;
            StartCoroutine(AnimateFlash(obj, rect, image, color, isPassive));
            yield return null;
        }

        private IEnumerator AnimateFlash(GameObject obj, RectTransform rect, Image image, Color color, bool isPassive)
        {
            transientObjects.Add(obj);
            float elapsed = 0f;
            Vector3 baseScale = rect == null ? Vector3.one : rect.localScale;
            float duration = isPassive ? FlashDuration * 0.72f : FlashDuration;
            float alpha = isPassive ? 0.42f : 0.72f;
            float endScale = isPassive ? 1.10f : 1.20f;
            while (elapsed < duration && obj != null && rect != null && image != null)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                Color target = Color.Lerp(Color.white, color, 0.35f);
                target.a = Mathf.Lerp(alpha, 0f, normalized);
                image.color = target;
                rect.localScale = baseScale * Mathf.Lerp(1.02f, endScale, normalized);
                yield return null;
            }

            transientObjects.Remove(obj);
            DestroyTransient(obj);
        }

        private IEnumerator SpawnSkillVfx(
            RectTransform layer,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color,
            string kind,
            bool isPassive)
        {
            GameObject root = CreateUiObject("ItemTriggerSkillVfx_" + SanitizeName(kind), layer);
            transientObjects.Add(root);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            SetTopLeftAnchor(rootRect);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = ResolveSafeSize(sizeDelta, 88f) * (isPassive ? 0.94f : 1.18f);
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            float alphaScale = isPassive ? 0.64f : 1f;
            CreateVfxImage(rootRect, "Core", color, 0f, 0.68f * alphaScale, 0.82f);
            CreateVfxImage(rootRect, "SlashA", color, 28f, 0.36f * alphaScale, 1.12f);
            CreateVfxImage(rootRect, "SlashB", color, -32f, 0.28f * alphaScale, 0.92f);

            float elapsed = 0f;
            float duration = isPassive ? VfxDuration * 0.70f : VfxDuration;
            float startScale = isPassive ? 0.82f : 0.72f;
            float endScale = isPassive ? 1.18f : 1.46f;
            while (elapsed < duration && root != null && rootRect != null)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                rootRect.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, normalized);
                rootRect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-5f, isPassive ? 8f : 16f, normalized));
                if (group != null)
                {
                    group.alpha = Mathf.Sin(normalized * Mathf.PI);
                }

                yield return null;
            }

            transientObjects.Remove(root);
            DestroyTransient(root);
        }

        private IEnumerator SpawnFloatingText(
            RectTransform layer,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            string text,
            Color color,
            bool isPassive)
        {
            GameObject obj = CreateUiObject("ItemTriggerFloatingText", layer);
            transientObjects.Add(obj);
            RectTransform rect = obj.GetComponent<RectTransform>();
            SetTopLeftAnchor(rect);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition + new Vector2(0f, Mathf.Max(30f, sizeDelta.y * (isPassive ? 0.46f : 0.58f)));
            rect.sizeDelta = new Vector2(Mathf.Max(150f, sizeDelta.x + 64f), 44f);

            Text label = obj.AddComponent<Text>();
            label.font = ResolveRuntimeFont();
            label.text = text ?? string.Empty;
            label.fontSize = isPassive ? 21 : 24;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.74f);
            outline.effectDistance = new Vector2(1.8f, -1.8f);

            CanvasGroup group = obj.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            Vector2 start = rect.anchoredPosition;
            float elapsed = 0f;
            float duration = isPassive ? FloatingDuration * 0.72f : FloatingDuration;
            float rise = isPassive ? 38f : 54f;
            while (elapsed < duration && obj != null && rect != null && label != null)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = start + new Vector2(0f, Mathf.Lerp(0f, rise, normalized));
                rect.localScale = Vector3.one * Mathf.Lerp(isPassive ? 0.90f : 0.92f, isPassive ? 1.02f : 1.08f, Mathf.Sin(normalized * Mathf.PI));
                Color textColor = color;
                textColor.a = normalized < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (normalized - 0.72f) / 0.28f);
                label.color = textColor;
                if (group != null)
                {
                    group.alpha = textColor.a;
                }

                yield return null;
            }

            transientObjects.Remove(obj);
            DestroyTransient(obj);
        }

        private RectTransform EnsureFxLayer(RectTransform parent)
        {
            if (parent == null)
            {
                return null;
            }

            Transform existing = parent.Find(FxLayerName);
            GameObject target = existing == null
                ? new GameObject(FxLayerName, typeof(RectTransform), typeof(CanvasGroup))
                : existing.gameObject;
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(parent, false);
            RectTransform rect = target.GetComponent<RectTransform>();
            StretchToParent(rect);
            rect.SetAsLastSibling();
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
            }

            return rect;
        }

        private GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject obj = new(name, typeof(RectTransform), typeof(CanvasRenderer));
            obj.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private void CreateVfxImage(
            RectTransform parent,
            string name,
            Color color,
            float rotation,
            float alpha,
            float scale)
        {
            GameObject obj = CreateUiObject(name, parent);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = parent.sizeDelta * scale;
            rect.localEulerAngles = new Vector3(0f, 0f, rotation);
            Image image = obj.AddComponent<Image>();
            image.raycastTarget = false;
            Color imageColor = color;
            imageColor.a = alpha;
            image.color = imageColor;
        }

        private bool TryReservePassiveFeedbackSlot(BattleSandboxRuntimeLoopRow row)
        {
            float now = Time.time;
            if (now - lastActiveFeedbackTime < ActiveSuppressesPassiveSeconds)
            {
                return false;
            }

            if (now - lastPassiveFeedbackTime < PassiveFeedbackGlobalInterval)
            {
                return false;
            }

            string key = $"{row?.itemId ?? string.Empty}:{row?.boardItemTriggerFeedbackKind ?? string.Empty}";
            if (passiveFeedbackLastPlayTimes.TryGetValue(key, out float lastPlayed)
                && now - lastPlayed < PassiveFeedbackItemInterval)
            {
                return false;
            }

            passiveFeedbackLastPlayTimes[key] = now;
            lastPassiveFeedbackTime = now;
            return true;
        }

        private static bool IsPassiveChannel(string channel)
        {
            return string.Equals(channel, "passive", System.StringComparison.Ordinal);
        }

        private Font ResolveRuntimeFont()
        {
            if (runtimeFont == null)
            {
                runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return runtimeFont;
        }

        private static Color ResolveColor(string kind)
        {
            switch (kind ?? string.Empty)
            {
                case "damage":
                    return new Color(1f, 0.36f, 0.16f, 1f);
                case "shieldBreak":
                    return new Color(1f, 0.86f, 0.30f, 1f);
                case "shield":
                    return new Color(0.40f, 0.86f, 1f, 1f);
                case "cleanse":
                    return new Color(0.72f, 1f, 0.78f, 1f);
                case "control":
                    return new Color(0.78f, 0.54f, 1f, 1f);
                case "mana":
                    return new Color(0.48f, 1f, 0.92f, 1f);
                case "manaBoost":
                    return new Color(0.62f, 1f, 0.70f, 1f);
                case "cooldownBoost":
                    return new Color(0.56f, 0.82f, 1f, 1f);
                case "damageBoost":
                    return new Color(1f, 0.64f, 0.24f, 1f);
                case "shieldBreakBoost":
                    return new Color(1f, 0.92f, 0.42f, 1f);
                case "shieldBoost":
                    return new Color(0.62f, 0.94f, 1f, 1f);
                case "cleanseBoost":
                    return new Color(0.78f, 1f, 0.72f, 1f);
                case "controlBoost":
                    return new Color(0.84f, 0.66f, 1f, 1f);
                case "weakPulse":
                    return new Color(0.92f, 0.78f, 1f, 1f);
                case "suppressed":
                case "notPowered":
                case "manaShortage":
                    return new Color(0.76f, 0.78f, 0.82f, 1f);
                default:
                    return new Color(0.86f, 1f, 0.52f, 1f);
            }
        }

        private static Vector2 ResolveSafeSize(Vector2 sizeDelta, float fallback)
        {
            float width = Mathf.Max(fallback, Mathf.Abs(sizeDelta.x));
            float height = Mathf.Max(fallback, Mathf.Abs(sizeDelta.y));
            return new Vector2(width, height);
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Default";
            }

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private static void SetTopLeftAnchor(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
        }

        private static void StretchToParent(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
        }

        private static void DestroyTransient(GameObject obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }
    }
}
