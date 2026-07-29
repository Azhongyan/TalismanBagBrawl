using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BattleSandboxItemTriggerFeedbackController : MonoBehaviour
    {
        public const string PackageName = "V0.4-BattleSandboxItemTriggerFeedbackFx01";
        public const string LiHuoBuildFamilyPresentationKey =
            "build.lihuo";
        public const string TaiBaiBuildFamilyPresentationKeyReserved =
            "build.taibai.reserved";
        public const string FireProjectileCarrierKey =
            "carrier.fire_projectile";
        public const string SwordQiSlashCarrierKey =
            "carrier.sword_qi_slash";
        public const string HeavySealDropCarrierKey =
            "carrier.heavy_seal_drop";
        public const string TaiBaiCarrierFamilyKeyReserved =
            "carrier.family.taibai.reserved";

        private const string FxLayerName = "BattleSandboxItemTriggerFeedbackFxLayer";
        private const string ZhaoShaMirrorFramesResourcesPath = "anim/\u7167\u715e\u955c_VFX_RGBA_9\u5e27/frames";
        private const float TargetPulseDuration = 0.28f;
        private const float FlashDuration = 0.36f;
        private const float VfxDuration = 0.48f;
        private const float FloatingDuration = 0.86f;
        private const float PassiveFeedbackGlobalInterval = 1.45f;
        private const float PassiveFeedbackItemInterval = 2.6f;
        private const float ActiveSuppressesPassiveSeconds = 0.35f;

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool playResourceSequenceVfx = true;
        [SerializeField] private bool preferInspectorSequenceFrameSlots = true;
        [SerializeField] private List<Sprite> inspectorSequenceFrameSlots = new();
        [SerializeField] private string resourceSequenceFramesPath = ZhaoShaMirrorFramesResourcesPath;
        [SerializeField] private string resourceSequenceTargetItemId = "preview_fire_talisman";
        [SerializeField] private string resourceSequenceTargetFeedbackKind = "damage";
        [SerializeField] private bool resourceSequenceRequiresSingleCell = true;
        [SerializeField, Min(1f)] private float resourceSequenceFramesPerSecond = 18f;
        [SerializeField, Min(0.1f)] private float resourceSequenceScale = 1.35f;
        [SerializeField] private bool suppressProceduralVfxWhenResourceSequencePlays = true;

        private readonly List<GameObject> transientObjects = new();
        private readonly List<Sprite> generatedSequenceSprites = new();
        private readonly Dictionary<RectTransform, Vector3> pulsedTargetBaseScales = new();
        private readonly Dictionary<string, float> passiveFeedbackLastPlayTimes = new();
        private readonly HashSet<string> acceptedPresentationEventIds =
            new(StringComparer.Ordinal);
        private BuildGridInteractionPreviewController gridController;
        private Font runtimeFont;
        private List<Sprite> cachedResourceSequenceFrames;
        private string cachedResourceSequenceFramesPath = string.Empty;
        private float lastActiveFeedbackTime = -100f;
        private float lastPassiveFeedbackTime = -100f;
        private int rejectedAcceptedPresentationEventCount;
        private int acceptedCompleteLegacyPresentationCount;
        private int acceptedCarrierPresentationCount;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public int AcceptedPresentationEventCount =>
            acceptedPresentationEventIds.Count;
        public int RejectedAcceptedPresentationEventCount =>
            rejectedAcceptedPresentationEventCount;
        public int AcceptedCompleteLegacyPresentationCount =>
            acceptedCompleteLegacyPresentationCount;
        public int AcceptedCarrierPresentationCount =>
            acceptedCarrierPresentationCount;
        public int ActiveTransientObjectCount =>
            transientObjects.Count(value => value != null);

        public void Bind(BuildGridInteractionPreviewController controller)
        {
            gridController = controller;
        }

        public void Play(BattleSandboxRuntimeLoopRow row)
        {
            PlayResolvedPresentation(row, true);
        }

        private void PlayResolvedPresentation(
            BattleSandboxRuntimeLoopRow row,
            bool spawnLegacyFloatingText)
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

            bool playedResourceSequence = TryPlayResourceSequenceVfx(
                row,
                fxLayer,
                anchoredPosition,
                sizeDelta,
                isPassive);
            if (!playedResourceSequence || !suppressProceduralVfxWhenResourceSequencePlays)
            {
                StartCoroutine(SpawnSkillVfx(fxLayer, anchoredPosition, sizeDelta, color, row.boardItemTriggerFeedbackKind, isPassive));
            }

            if (spawnLegacyFloatingText)
            {
                StartCoroutine(SpawnFloatingText(
                    fxLayer,
                    anchoredPosition,
                    sizeDelta,
                    row.boardItemTriggerFeedbackTextChinese,
                    color,
                    isPassive));
            }
        }

        public bool TryPlayAcceptedPresentation(
            string eventId,
            string sourceBaseItemId,
            string sourceItemInstanceId,
            string sourcePlacementId,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            int resolvedPreMitigationDamageUnits,
            RectTransform enemyHitAnchor,
            out Vector3 sourceWorldPosition)
        {
            sourceWorldPosition = Vector3.zero;
            if (gridController == null
                || enemyHitAnchor == null
                || string.IsNullOrWhiteSpace(eventId)
                || string.IsNullOrWhiteSpace(sourceBaseItemId)
                || string.IsNullOrWhiteSpace(sourceItemInstanceId)
                || string.IsNullOrWhiteSpace(sourcePlacementId)
                || occupiedCells == null
                || occupiedCells.Count == 0
                || resolvedPreMitigationDamageUnits <= 0)
            {
                return false;
            }

            string stableEventId = eventId.Trim();
            if (acceptedPresentationEventIds.Contains(stableEventId))
            {
                rejectedAcceptedPresentationEventCount++;
                return false;
            }

            if (!gridController.TryResolveBoardItemFeedbackAnchor(
                    sourceBaseItemId.Trim(),
                    occupiedCells,
                    out RectTransform itemArtworkRect,
                    out RectTransform feedbackLayer,
                    out Vector2 anchoredPosition,
                    out Vector2 sizeDelta))
            {
                return false;
            }

            sourceWorldPosition = itemArtworkRect != null
                ? RectWorldCenter(itemArtworkRect)
                : feedbackLayer.TransformPoint(anchoredPosition);
            Canvas sourceCanvas =
                (itemArtworkRect == null
                    ? feedbackLayer
                    : itemArtworkRect)
                .GetComponentInParent<Canvas>();
            Canvas targetCanvas =
                enemyHitAnchor.GetComponentInParent<Canvas>();
            Canvas rootCanvas = targetCanvas == null
                ? sourceCanvas?.rootCanvas
                : targetCanvas.rootCanvas;
            RectTransform presentationRoot =
                rootCanvas == null
                    ? null
                    : rootCanvas.transform as RectTransform;
            if (presentationRoot == null)
            {
                return false;
            }

            acceptedPresentationEventIds.Add(stableEventId);
            BattleSandboxRuntimeLoopRow completeLegacyPresentationRequest =
                new()
            {
                rowId = stableEventId,
                rowKind = "enemyHp",
                itemId = sourceBaseItemId.Trim(),
                playsBoardItemTriggerFeedback = true,
                boardItemTriggerFeedbackChannel = "active",
                boardItemTriggerFeedbackKind =
                    ResolveAcceptedBuildVfxKind(
                        sourceBaseItemId),
                boardItemTriggerFeedbackTextChinese =
                    resolvedPreMitigationDamageUnits.ToString(
                        System.Globalization.CultureInfo.InvariantCulture),
                boardItemTriggerFeedbackValue =
                    resolvedPreMitigationDamageUnits,
                boardItemTriggerOccupiedCells = occupiedCells
                    .Select(value => new ItemShapeCell(value.x, value.y))
                    .ToList()
            };
            PlayResolvedPresentation(
                completeLegacyPresentationRequest,
                false);
            acceptedCompleteLegacyPresentationCount++;
            AcceptedCarrierKind carrierKind =
                ResolveAcceptedCarrierKind(sourceBaseItemId);
            StartCoroutine(SpawnAcceptedCarrierVfx(
                stableEventId,
                presentationRoot,
                sourceWorldPosition,
                RectWorldCenter(enemyHitAnchor),
                carrierKind,
                ResolveColor(completeLegacyPresentationRequest
                    .boardItemTriggerFeedbackKind)));
            acceptedCarrierPresentationCount++;
            return true;
        }

        public bool TryResolveBoardSourceWorldPosition(
            string sourceBaseItemId,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            out Vector3 sourceWorldPosition)
        {
            sourceWorldPosition = Vector3.zero;
            if (gridController == null
                || string.IsNullOrWhiteSpace(sourceBaseItemId)
                || occupiedCells == null
                || occupiedCells.Count == 0
                || !gridController.TryResolveBoardItemFeedbackAnchor(
                    sourceBaseItemId.Trim(),
                    occupiedCells,
                    out RectTransform itemArtworkRect,
                    out RectTransform feedbackLayer,
                    out Vector2 anchoredPosition,
                    out Vector2 _))
            {
                return false;
            }

            sourceWorldPosition = itemArtworkRect != null
                ? RectWorldCenter(itemArtworkRect)
                : feedbackLayer.TransformPoint(anchoredPosition);
            return true;
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
            acceptedPresentationEventIds.Clear();
            rejectedAcceptedPresentationEventCount = 0;
            acceptedCompleteLegacyPresentationCount = 0;
            acceptedCarrierPresentationCount = 0;
            lastActiveFeedbackTime = -100f;
            lastPassiveFeedbackTime = -100f;
            for (int i = transientObjects.Count - 1; i >= 0; i--)
            {
                if (transientObjects[i] != null)
                {
                    transientObjects[i].SetActive(false);
                }
                DestroyTransient(transientObjects[i]);
            }

            transientObjects.Clear();
        }

        private void OnDestroy()
        {
            DestroyGeneratedSequenceSprites();
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

        private IEnumerator SpawnAcceptedCarrierVfx(
            string eventId,
            RectTransform presentationRoot,
            Vector3 sourceWorldPosition,
            Vector3 targetWorldPosition,
            AcceptedCarrierKind carrierKind,
            Color sourceColor)
        {
            if (presentationRoot == null)
            {
                yield break;
            }

            GameObject root = CreateUiObject(
                "AcceptedSkillCarrier_"
                + ResolveCarrierPresentationKey(carrierKind)
                + "_"
                + SanitizeName(eventId),
                presentationRoot);
            transientObjects.Add(root);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = ResolveCarrierSize(carrierKind);
            rect.position = sourceWorldPosition;
            rect.localRotation = Quaternion.identity;
            rect.SetAsLastSibling();

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            BuildCarrierVisual(
                rect,
                carrierKind,
                sourceColor);

            const float duration = 0.96f;
            const float sourceActivationEnd = 0.19f;
            const float carrierEnd = 0.72f;
            float elapsed = 0f;
            while (elapsed < duration
                && root != null
                && rect != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                if (progress < sourceActivationEnd)
                {
                    float phase =
                        Smooth01(progress / sourceActivationEnd);
                    if (carrierKind == AcceptedCarrierKind.HeavySealDrop)
                    {
                        rect.position =
                            targetWorldPosition + Vector3.up * 138f;
                        rect.localScale =
                            Vector3.one * Mathf.Lerp(0.76f, 0.94f, phase);
                        group.alpha = 0f;
                    }
                    else
                    {
                        rect.position = sourceWorldPosition;
                        rect.localScale =
                            Vector3.one * Mathf.Lerp(0.58f, 1.08f, phase);
                        group.alpha = Mathf.Lerp(0f, 0.96f, phase);
                    }
                }
                else if (progress < carrierEnd)
                {
                    float phase =
                        Smooth01(
                            (progress - sourceActivationEnd)
                            / (carrierEnd - sourceActivationEnd));
                    AnimateCarrierTravel(
                        rect,
                        carrierKind,
                        sourceWorldPosition,
                        targetWorldPosition,
                        phase);
                    group.alpha = 1f;
                }
                else
                {
                    float phase =
                        Smooth01(
                            (progress - carrierEnd)
                            / (1f - carrierEnd));
                    rect.position = targetWorldPosition;
                    AnimateCarrierImpact(
                        rect,
                        carrierKind,
                        phase);
                    group.alpha = 1f - phase;
                }

                yield return null;
            }

            transientObjects.Remove(root);
            DestroyTransient(root);
        }

        private static void AnimateCarrierTravel(
            RectTransform rect,
            AcceptedCarrierKind carrierKind,
            Vector3 sourceWorldPosition,
            Vector3 targetWorldPosition,
            float phase)
        {
            if (rect == null)
            {
                return;
            }

            switch (carrierKind)
            {
                case AcceptedCarrierKind.SwordQiSlash:
                {
                    Vector3 control =
                        (sourceWorldPosition + targetWorldPosition) * 0.5f
                        + Vector3.up * 42f;
                    Vector3 next = QuadraticBezier(
                        sourceWorldPosition,
                        control,
                        targetWorldPosition,
                        phase);
                    Vector3 tangent = phase < 0.98f
                        ? QuadraticBezier(
                            sourceWorldPosition,
                            control,
                            targetWorldPosition,
                            Mathf.Min(1f, phase + 0.02f)) - next
                        : targetWorldPosition - next;
                    rect.position = next;
                    rect.localEulerAngles = new Vector3(
                        0f,
                        0f,
                        Mathf.Atan2(tangent.y, tangent.x)
                        * Mathf.Rad2Deg);
                    rect.localScale =
                        Vector3.one * Mathf.Lerp(0.82f, 1.16f, phase);
                    break;
                }
                case AcceptedCarrierKind.HeavySealDrop:
                {
                    Vector3 dropStart =
                        targetWorldPosition + Vector3.up * 138f;
                    rect.position = Vector3.Lerp(
                        dropStart,
                        targetWorldPosition,
                        phase * phase);
                    rect.localEulerAngles =
                        new Vector3(0f, 0f, Mathf.Lerp(-4f, 3f, phase));
                    rect.localScale = new Vector3(
                        Mathf.Lerp(0.88f, 1.18f, phase),
                        Mathf.Lerp(0.72f, 1.12f, phase),
                        1f);
                    break;
                }
                default:
                {
                    Vector3 control =
                        (sourceWorldPosition + targetWorldPosition) * 0.5f
                        + Vector3.up * 68f;
                    rect.position = QuadraticBezier(
                        sourceWorldPosition,
                        control,
                        targetWorldPosition,
                        phase);
                    rect.localEulerAngles =
                        new Vector3(0f, 0f, phase * 118f);
                    rect.localScale =
                        Vector3.one * Mathf.Lerp(0.88f, 0.70f, phase);
                    break;
                }
            }
        }

        private static void AnimateCarrierImpact(
            RectTransform rect,
            AcceptedCarrierKind carrierKind,
            float phase)
        {
            if (rect == null)
            {
                return;
            }

            switch (carrierKind)
            {
                case AcceptedCarrierKind.SwordQiSlash:
                    rect.localScale = new Vector3(
                        Mathf.Lerp(0.94f, 1.72f, phase),
                        Mathf.Lerp(0.94f, 1.26f, phase),
                        1f);
                    rect.localEulerAngles =
                        new Vector3(0f, 0f, Mathf.Lerp(-18f, 24f, phase));
                    break;
                case AcceptedCarrierKind.HeavySealDrop:
                    rect.localScale = new Vector3(
                        Mathf.Lerp(1.18f, 1.64f, phase),
                        Mathf.Lerp(1.12f, 0.72f, phase),
                        1f);
                    rect.localEulerAngles = Vector3.zero;
                    break;
                default:
                    rect.localScale =
                        Vector3.one * Mathf.Lerp(0.70f, 1.84f, phase);
                    rect.localEulerAngles =
                        new Vector3(0f, 0f, phase * 76f);
                    break;
            }
        }

        private void BuildCarrierVisual(
            RectTransform parent,
            AcceptedCarrierKind carrierKind,
            Color sourceColor)
        {
            Color cinnabar = Color.Lerp(
                sourceColor,
                new Color(0.94f, 0.08f, 0.04f, 1f),
                0.42f);
            Color flameOrange =
                new(1f, 0.38f, 0.06f, 1f);
            Color brightGold =
                new(1f, 0.84f, 0.24f, 1f);

            switch (carrierKind)
            {
                case AcceptedCarrierKind.SwordQiSlash:
                    CreateCarrierPart(
                        parent,
                        "SwordQiCore",
                        new Vector2(112f, 8f),
                        Vector2.zero,
                        0f,
                        brightGold);
                    CreateCarrierPart(
                        parent,
                        "SwordQiEdge",
                        new Vector2(96f, 3f),
                        new Vector2(-8f, 7f),
                        -5f,
                        Color.Lerp(sourceColor, Color.white, 0.34f));
                    CreateCarrierPart(
                        parent,
                        "SwordQiCrossImpact",
                        new Vector2(72f, 5f),
                        Vector2.zero,
                        58f,
                        flameOrange);
                    break;
                case AcceptedCarrierKind.HeavySealDrop:
                    CreateCarrierPart(
                        parent,
                        "SealTop",
                        new Vector2(84f, 7f),
                        new Vector2(0f, 38f),
                        0f,
                        brightGold);
                    CreateCarrierPart(
                        parent,
                        "SealBottom",
                        new Vector2(84f, 7f),
                        new Vector2(0f, -38f),
                        0f,
                        cinnabar);
                    CreateCarrierPart(
                        parent,
                        "SealLeft",
                        new Vector2(7f, 84f),
                        new Vector2(-38f, 0f),
                        0f,
                        flameOrange);
                    CreateCarrierPart(
                        parent,
                        "SealRight",
                        new Vector2(7f, 84f),
                        new Vector2(38f, 0f),
                        0f,
                        brightGold);
                    CreateCarrierPart(
                        parent,
                        "SealWeightCore",
                        new Vector2(54f, 12f),
                        Vector2.zero,
                        45f,
                        sourceColor);
                    CreateCarrierPart(
                        parent,
                        "SealImpactBar",
                        new Vector2(116f, 8f),
                        new Vector2(0f, -44f),
                        0f,
                        brightGold);
                    break;
                default:
                    CreateCarrierPart(
                        parent,
                        "FireCore",
                        new Vector2(30f, 30f),
                        Vector2.zero,
                        45f,
                        brightGold);
                    CreateCarrierPart(
                        parent,
                        "FireTrailA",
                        new Vector2(74f, 10f),
                        new Vector2(-40f, -4f),
                        -7f,
                        flameOrange);
                    CreateCarrierPart(
                        parent,
                        "FireTrailB",
                        new Vector2(56f, 6f),
                        new Vector2(-34f, 13f),
                        14f,
                        cinnabar);
                    CreateCarrierPart(
                        parent,
                        "FireSpark",
                        new Vector2(14f, 14f),
                        new Vector2(22f, -18f),
                        45f,
                        sourceColor);
                    break;
            }
        }

        private void CreateCarrierPart(
            RectTransform parent,
            string name,
            Vector2 size,
            Vector2 offset,
            float rotation,
            Color color)
        {
            GameObject part = CreateUiObject(name, parent);
            RectTransform partRect =
                part.GetComponent<RectTransform>();
            partRect.anchorMin = new Vector2(0.5f, 0.5f);
            partRect.anchorMax = new Vector2(0.5f, 0.5f);
            partRect.pivot = new Vector2(0.5f, 0.5f);
            partRect.anchoredPosition = offset;
            partRect.sizeDelta = size;
            partRect.localEulerAngles =
                new Vector3(0f, 0f, rotation);
            Image image = part.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
        }

        private static Vector2 ResolveCarrierSize(
            AcceptedCarrierKind carrierKind)
        {
            return carrierKind switch
            {
                AcceptedCarrierKind.SwordQiSlash =>
                    new Vector2(132f, 92f),
                AcceptedCarrierKind.HeavySealDrop =>
                    new Vector2(132f, 132f),
                _ => new Vector2(124f, 90f)
            };
        }

        private bool TryPlayResourceSequenceVfx(
            BattleSandboxRuntimeLoopRow row,
            RectTransform layer,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            bool isPassive)
        {
            if (!playResourceSequenceVfx || isPassive || row == null || layer == null)
            {
                return false;
            }

            if (resourceSequenceRequiresSingleCell
                && (row.boardItemTriggerOccupiedCells == null || row.boardItemTriggerOccupiedCells.Count != 1))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(resourceSequenceTargetItemId)
                && !string.Equals(row.itemId, resourceSequenceTargetItemId.Trim(), StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(resourceSequenceTargetFeedbackKind)
                && !string.Equals(row.boardItemTriggerFeedbackKind, resourceSequenceTargetFeedbackKind.Trim(), StringComparison.Ordinal))
            {
                return false;
            }

            List<Sprite> frames = ResolveResourceSequenceFrames();
            if (frames.Count == 0)
            {
                return false;
            }

            StartCoroutine(SpawnResourceSequenceVfx(
                layer,
                anchoredPosition,
                sizeDelta,
                frames));
            return true;
        }

        private IEnumerator SpawnResourceSequenceVfx(
            RectTransform layer,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            IReadOnlyList<Sprite> frames)
        {
            if (layer == null || frames == null || frames.Count == 0)
            {
                yield break;
            }

            GameObject root = CreateUiObject("ItemTriggerResourceSequenceVfx", layer);
            transientObjects.Add(root);
            RectTransform rect = root.GetComponent<RectTransform>();
            SetTopLeftAnchor(rect);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = ResolveSafeSize(sizeDelta, 92f) * Mathf.Max(0.1f, resourceSequenceScale);
            rect.localScale = Vector3.one;

            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            Image image = root.AddComponent<Image>();
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.color = Color.white;

            float frameDuration = 1f / Mathf.Max(1f, resourceSequenceFramesPerSecond);
            float elapsed = 0f;
            float totalDuration = frameDuration * frames.Count;
            int frameIndex = -1;
            while (elapsed < totalDuration && root != null && image != null)
            {
                int nextFrameIndex = Mathf.Clamp(Mathf.FloorToInt(elapsed / frameDuration), 0, frames.Count - 1);
                if (nextFrameIndex != frameIndex)
                {
                    frameIndex = nextFrameIndex;
                    image.sprite = frames[frameIndex];
                }

                float normalized = totalDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / totalDuration);
                rect.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.08f, Mathf.Sin(normalized * Mathf.PI));
                group.alpha = normalized < 0.82f ? 1f : Mathf.Lerp(1f, 0f, (normalized - 0.82f) / 0.18f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transientObjects.Remove(root);
            DestroyTransient(root);
        }

        private List<Sprite> ResolveResourceSequenceFrames()
        {
            if (TryResolveInspectorSequenceFrames(out List<Sprite> inspectorFrames))
            {
                return inspectorFrames;
            }

            string path = string.IsNullOrWhiteSpace(resourceSequenceFramesPath)
                ? string.Empty
                : resourceSequenceFramesPath.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                return new List<Sprite>();
            }

            if (cachedResourceSequenceFrames != null
                && string.Equals(cachedResourceSequenceFramesPath, path, StringComparison.Ordinal))
            {
                return cachedResourceSequenceFrames;
            }

            cachedResourceSequenceFramesPath = path;
            cachedResourceSequenceFrames = LoadResourceSequenceSprites(path);
            return cachedResourceSequenceFrames;
        }

        private bool TryResolveInspectorSequenceFrames(out List<Sprite> frames)
        {
            frames = new List<Sprite>();
            if (!preferInspectorSequenceFrameSlots || inspectorSequenceFrameSlots == null)
            {
                return false;
            }

            for (int i = 0; i < inspectorSequenceFrameSlots.Count; i++)
            {
                Sprite frame = inspectorSequenceFrameSlots[i];
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }

            return frames.Count > 0;
        }

        private List<Sprite> LoadResourceSequenceSprites(string path)
        {
            List<Sprite> frames = new(Resources.LoadAll<Sprite>(path) ?? Array.Empty<Sprite>());
            frames.RemoveAll(sprite => sprite == null);
            if (frames.Count == 0)
            {
                foreach (Texture2D texture in Resources.LoadAll<Texture2D>(path) ?? Array.Empty<Texture2D>())
                {
                    if (texture == null)
                    {
                        continue;
                    }

                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    sprite.name = texture.name;
                    sprite.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    generatedSequenceSprites.Add(sprite);
                    frames.Add(sprite);
                }
            }

            frames.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.name, right.name));
            return frames;
        }

        private void DestroyGeneratedSequenceSprites()
        {
            for (int i = generatedSequenceSprites.Count - 1; i >= 0; i--)
            {
                DestroyTransient(generatedSequenceSprites[i]);
            }

            generatedSequenceSprites.Clear();
            cachedResourceSequenceFrames = null;
            cachedResourceSequenceFramesPath = string.Empty;
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

        private static Vector3 RectWorldCenter(RectTransform rect)
        {
            return rect == null
                ? Vector3.zero
                : rect.TransformPoint(rect.rect.center);
        }

        private static Vector3 QuadraticBezier(
            Vector3 start,
            Vector3 control,
            Vector3 end,
            float progress)
        {
            float t = Mathf.Clamp01(progress);
            float inverse = 1f - t;
            return inverse * inverse * start
                + 2f * inverse * t * control
                + t * t * end;
        }

        private static float Smooth01(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * (3f - 2f * clamped);
        }

        private static Color ResolveColor(string kind)
        {
            switch (kind ?? string.Empty)
            {
                case "lihuo.i007":
                    return new Color(1f, 0.25f, 0.12f, 1f);
                case "lihuo.i008":
                    return new Color(1f, 0.48f, 0.10f, 1f);
                case "lihuo.i009":
                    return new Color(0.96f, 0.15f, 0.18f, 1f);
                case "lihuo.i010":
                    return new Color(0.22f, 0.78f, 0.72f, 1f);
                case "lihuo.i011":
                    return new Color(0.68f, 0.20f, 0.76f, 1f);
                case "lihuo.i012":
                    return new Color(1f, 0.70f, 0.16f, 1f);
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

        private static string ResolveAcceptedBuildVfxKind(
            string sourceBaseItemId)
        {
            return (sourceBaseItemId ?? string.Empty).Trim() switch
            {
                "I007" => "lihuo.i007",
                "I008" => "lihuo.i008",
                "I009" => "lihuo.i009",
                "I010" => "lihuo.i010",
                "I011" => "lihuo.i011",
                "I012" => "lihuo.i012",
                _ => "damage"
            };
        }

        public static string ResolveAcceptedCarrierPresentationKey(
            string sourceBaseItemId)
        {
            return ResolveCarrierPresentationKey(
                ResolveAcceptedCarrierKind(sourceBaseItemId));
        }

        private static AcceptedCarrierKind ResolveAcceptedCarrierKind(
            string sourceBaseItemId)
        {
            // Presentation-only table. It does not infer gameplay from
            // localized names and never participates in damage resolution.
            return (sourceBaseItemId ?? string.Empty).Trim() switch
            {
                "I007" => AcceptedCarrierKind.FireProjectile,
                "I008" => AcceptedCarrierKind.SwordQiSlash,
                "I009" => AcceptedCarrierKind.HeavySealDrop,
                "I010" => AcceptedCarrierKind.FireProjectile,
                "I011" => AcceptedCarrierKind.SwordQiSlash,
                "I012" => AcceptedCarrierKind.HeavySealDrop,
                _ => AcceptedCarrierKind.FireProjectile
            };
        }

        private static string ResolveCarrierPresentationKey(
            AcceptedCarrierKind carrierKind)
        {
            return carrierKind switch
            {
                AcceptedCarrierKind.SwordQiSlash =>
                    SwordQiSlashCarrierKey,
                AcceptedCarrierKind.HeavySealDrop =>
                    HeavySealDropCarrierKey,
                _ => FireProjectileCarrierKey
            };
        }

        private enum AcceptedCarrierKind
        {
            FireProjectile = 0,
            SwordQiSlash = 1,
            HeavySealDrop = 2
        }

        private static void DestroyTransient(UnityEngine.Object obj)
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
