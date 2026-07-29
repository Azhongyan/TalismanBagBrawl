using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.BuildSandbox;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TalismanBag.BattleBridge.NianResource
{
    public enum BattleSandboxAuthoredTmpStyle
    {
        PrimaryWarm = 0,
        SecondaryCyan = 1
    }

    public sealed class BattleSandboxAcceptedItemPresentationEvent
    {
        private readonly ReadOnlyCollection<ItemShapeCell> occupiedCells;
        private readonly ReadOnlyCollection<ItemShapeCell> i031OccupiedCells;

        public BattleSandboxAcceptedItemPresentationEvent(
            string pulseEventId,
            string nianApplicationId,
            string damageLedgerEventId,
            int resetGeneration,
            long battleTick,
            string sourceBaseItemId,
            string sourceItemInstanceId,
            string sourcePlacementId,
            IEnumerable<ItemShapeCell> occupiedCells,
            IEnumerable<ItemShapeCell> i031OccupiedCells,
            int nianGeneratedDelta,
            int nianSpentDelta,
            int resolvedPreMitigationDamageUnits,
            int shellDamageApplied,
            int hpDamageApplied)
        {
            this.pulseEventId = pulseEventId ?? string.Empty;
            this.nianApplicationId = nianApplicationId ?? string.Empty;
            this.damageLedgerEventId = damageLedgerEventId ?? string.Empty;
            this.resetGeneration = resetGeneration;
            this.battleTick = battleTick;
            this.sourceBaseItemId = sourceBaseItemId ?? string.Empty;
            this.sourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            this.sourcePlacementId = sourcePlacementId ?? string.Empty;
            this.occupiedCells = Array.AsReadOnly(
                (occupiedCells ?? Array.Empty<ItemShapeCell>())
                    .Select(value => new ItemShapeCell(value.x, value.y))
                    .Distinct()
                    .OrderBy(value => value.y)
                    .ThenBy(value => value.x)
                    .ToArray());
            this.i031OccupiedCells = Array.AsReadOnly(
                (i031OccupiedCells ?? Array.Empty<ItemShapeCell>())
                    .Select(value => new ItemShapeCell(value.x, value.y))
                    .Distinct()
                    .OrderBy(value => value.y)
                    .ThenBy(value => value.x)
                    .ToArray());
            this.nianGeneratedDelta = nianGeneratedDelta;
            this.nianSpentDelta = nianSpentDelta;
            this.resolvedPreMitigationDamageUnits =
                resolvedPreMitigationDamageUnits;
            this.shellDamageApplied = shellDamageApplied;
            this.hpDamageApplied = hpDamageApplied;
        }

        public string pulseEventId { get; }
        public string nianApplicationId { get; }
        public string damageLedgerEventId { get; }
        public int resetGeneration { get; }
        public long battleTick { get; }
        public string sourceBaseItemId { get; }
        public string sourceItemInstanceId { get; }
        public string sourcePlacementId { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells => occupiedCells;
        public IReadOnlyList<ItemShapeCell> I031OccupiedCells =>
            i031OccupiedCells;
        public int nianGeneratedDelta { get; }
        public int nianSpentDelta { get; }
        public int resolvedPreMitigationDamageUnits { get; }
        public int shellDamageApplied { get; }
        public int hpDamageApplied { get; }
        public int totalDamageApplied =>
            checked(shellDamageApplied + hpDamageApplied);

        public bool IsAcceptedCorrelation =>
            resetGeneration > 0
            && battleTick > 0L
            && !string.IsNullOrWhiteSpace(pulseEventId)
            && !string.IsNullOrWhiteSpace(nianApplicationId)
            && !string.IsNullOrWhiteSpace(damageLedgerEventId)
            && !string.IsNullOrWhiteSpace(sourceBaseItemId)
            && !string.IsNullOrWhiteSpace(sourceItemInstanceId)
            && !string.IsNullOrWhiteSpace(sourcePlacementId)
            && occupiedCells.Count > 0
            && i031OccupiedCells.Count > 0
            && nianGeneratedDelta >= 0
            && nianSpentDelta > 0
            && resolvedPreMitigationDamageUnits > 0
            && totalDamageApplied > 0;
    }

    [DisallowMultipleComponent]
    public sealed class BattleSandboxAuthoredTmpPresentation : MonoBehaviour
    {
        public const string PrimaryTemplateName = "shanghai";
        public const string SecondaryTemplateName = "shanghai (1)";
        public const float DefaultReadableLifetime = 1.65f;
        public const string RequiredFontAssetName =
            "MFLangSongJianYuan-Regular SDF";
        public const string RequiredSourceFontGuid =
            "6c3353c99530bc748bae0652855b810a";
        public const string RequiredSourceFontAssetName =
            "MFLangSongJianYuan-Regular";
        public const string NianGeneratedPaletteKey =
            "presentation.nian.generated";
        public const string NianSpentPaletteKey =
            "presentation.nian.spent";
        public const string SettlementHpPaletteKey =
            "presentation.settlement.hp";
        public const string SettlementShellPaletteKey =
            "presentation.settlement.shell";
        public const string SettlementBreakPaletteKey =
            "presentation.settlement.break";
        public const string LiHuoBuildFamilyPaletteKey =
            "presentation.build.lihuo";
        public const string TaiBaiBuildFamilyPaletteKeyReserved =
            "presentation.build.taibai.reserved";

        private const string RuntimeRootName =
            "BattleSandboxAuthoredTmpPresentation_Runtime";
        private const int MaxPoolPerStyle = 10;
        private const string ChinesePrewarmCharacters =
            "念力已达上限请求拒绝当前需要伤害护壳破碎重构技能一二三修复"
            + "缚索重击地爆发玩家守骨奴本次未发生按重置链校验失败"
            + "道具产出消耗不足继续战斗实际承受";

        private sealed class FloatingInstance
        {
            public GameObject gameObject;
            public RectTransform rect;
            public TextMeshProUGUI label;
            public CanvasGroup group;
            public BattleSandboxAuthoredTmpStyle style;
            public Vector3 startLocalPosition;
            public Vector3 baseScale;
            public float delay;
            public float lifetime;
            public float startedAt;
        }

        private readonly List<FloatingInstance> active = new();
        private readonly Queue<FloatingInstance> primaryPool = new();
        private readonly Queue<FloatingInstance> secondaryPool = new();
        private readonly HashSet<string> acceptedRoleKeys =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> laneSequences =
            new(StringComparer.Ordinal);

        private TextMeshProUGUI primaryTemplate;
        private TextMeshProUGUI secondaryTemplate;
        private RectTransform runtimeRoot;
        private TMP_FontAsset authoredFontAsset;
        private List<TMP_FontAsset> authoredFallbackSnapshot;
        private TMP_FontAsset runtimeChineseFallback;
        private int currentGeneration;
        private int peakLiveCount;
        private int rejectedDuplicateCount;
        private long anonymousSequence;

        public bool DevOnly => true;
        public bool OwnsBattleTruth => false;
        public bool WritesLayout => false;
        public int CurrentGeneration => currentGeneration;
        public int ActiveCount => active.Count;
        public int PooledCount => primaryPool.Count + secondaryPool.Count;
        public int PeakLiveCount => peakLiveCount;
        public int AcceptedRoleCount => acceptedRoleKeys.Count;
        public int RejectedDuplicateCount => rejectedDuplicateCount;
        public TextMeshProUGUI PrimaryTemplate => primaryTemplate;
        public TextMeshProUGUI SecondaryTemplate => secondaryTemplate;
        public bool HasAuthoredTemplates =>
            primaryTemplate != null && secondaryTemplate != null;
        public bool HasRuntimeChineseFallback =>
            runtimeChineseFallback != null;
        public string AuthoredFontAssetName =>
            authoredFontAsset == null
                ? string.Empty
                : authoredFontAsset.name;
        public int RuntimeChineseGlyphCount =>
            runtimeChineseFallback?.characterTable?.Count ?? 0;

        public void BeginGeneration(int resetGeneration)
        {
            ClearAll();
            currentGeneration = resetGeneration;
            ResolveTemplates();
        }

        public void EndGeneration()
        {
            ClearAll();
            currentGeneration = 0;
        }

        public bool TrySpawn(
            int resetGeneration,
            string eventId,
            string role,
            RectTransform anchor,
            string message,
            BattleSandboxAuthoredTmpStyle style,
            float scale = 1f,
            float delay = 0f,
            float lifetime = DefaultReadableLifetime,
            string itemPaletteKey = null,
            Vector2 localOffset = default)
        {
            if (anchor == null)
            {
                return false;
            }

            Vector3 worldPosition =
                anchor.TransformPoint(anchor.rect.center);
            return TrySpawnAtWorldPosition(
                resetGeneration,
                eventId,
                role,
                worldPosition,
                message,
                style,
                scale,
                delay,
                lifetime,
                itemPaletteKey,
                localOffset);
        }

        public bool TrySpawnAtWorldPosition(
            int resetGeneration,
            string eventId,
            string role,
            Vector3 worldPosition,
            string message,
            BattleSandboxAuthoredTmpStyle style,
            float scale = 1f,
            float delay = 0f,
            float lifetime = DefaultReadableLifetime,
            string itemPaletteKey = null,
            Vector2 localOffset = default)
        {
            if (currentGeneration <= 0
                || resetGeneration != currentGeneration
                || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            ResolveTemplates();
            TextMeshProUGUI template = ResolveTemplate(style);
            if (template == null || !EnsureRuntimeRoot(template))
            {
                return false;
            }
            if (!EnsureRuntimeChineseFallback(template)
                || !EnsureMessageGlyphs(message))
            {
                return false;
            }

            string stableEventId = string.IsNullOrWhiteSpace(eventId)
                ? "anonymous."
                  + (++anonymousSequence).ToString(
                      System.Globalization.CultureInfo.InvariantCulture)
                : eventId.Trim();
            string stableRole = string.IsNullOrWhiteSpace(role)
                ? "default"
                : role.Trim();
            string roleKey = stableEventId + "|" + stableRole;
            if (!acceptedRoleKeys.Add(roleKey))
            {
                rejectedDuplicateCount++;
                return false;
            }

            FloatingInstance instance = Acquire(style, template);
            if (instance == null)
            {
                acceptedRoleKeys.Remove(roleKey);
                return false;
            }

            CopyAuthoredTmpStyle(template, instance.label);
            if (TryResolveItemSourceGradient(
                    itemPaletteKey,
                    out VertexGradient sourceGradient))
            {
                instance.label.color = Color.white;
                instance.label.colorGradientPreset = null;
                instance.label.enableVertexGradient = true;
                instance.label.colorGradient = sourceGradient;
            }
            instance.label.text = message;
            instance.label.raycastTarget = false;
            instance.group.alpha = delay > 0f ? 0f : 1f;
            instance.group.interactable = false;
            instance.group.blocksRaycasts = false;

            RectTransform templateRect = template.rectTransform;
            instance.rect.anchorMin = new Vector2(0.5f, 0.5f);
            instance.rect.anchorMax = new Vector2(0.5f, 0.5f);
            instance.rect.pivot = templateRect.pivot;
            instance.rect.sizeDelta = templateRect.sizeDelta;
            instance.rect.localRotation = Quaternion.identity;
            instance.baseScale =
                templateRect.localScale
                * Mathf.Clamp(scale, 0.45f, 1.35f);

            int lane = NextLane(stableRole);
            Vector3 localPosition =
                runtimeRoot.InverseTransformPoint(worldPosition);
            localPosition.x += (lane - 1) * 28f;
            localPosition.y += lane * 12f;
            localPosition.x += localOffset.x;
            localPosition.y += localOffset.y;
            localPosition.z = 0f;
            instance.startLocalPosition = localPosition;
            instance.rect.localPosition = localPosition;
            instance.rect.localScale = instance.baseScale * 0.82f;
            instance.delay = Mathf.Max(0f, delay);
            instance.lifetime = Mathf.Clamp(lifetime, 1.4f, 1.8f);
            instance.startedAt = Time.unscaledTime;
            instance.gameObject.SetActive(true);
            instance.rect.SetAsLastSibling();
            instance.label.havePropertiesChanged = true;
            instance.label.ForceMeshUpdate(true, true);
            active.Add(instance);
            peakLiveCount = Mathf.Max(peakLiveCount, active.Count);
            return true;
        }

        private void LateUpdate()
        {
            float now = Time.unscaledTime;
            for (int index = active.Count - 1; index >= 0; index--)
            {
                FloatingInstance instance = active[index];
                if (instance?.gameObject == null)
                {
                    active.RemoveAt(index);
                    continue;
                }

                float elapsed = now - instance.startedAt;
                if (elapsed < instance.delay)
                {
                    instance.group.alpha = 0f;
                    continue;
                }

                float progress = Mathf.Clamp01(
                    (elapsed - instance.delay) / instance.lifetime);
                float scale;
                float rise;
                if (progress < 0.16f)
                {
                    float phase = progress / 0.16f;
                    scale = Mathf.Lerp(0.82f, 1.08f, Smooth(phase));
                    rise = Mathf.Lerp(0f, 5f, Smooth(phase));
                }
                else if (progress < 0.34f)
                {
                    float phase = (progress - 0.16f) / 0.18f;
                    scale = Mathf.Lerp(1.08f, 1f, Smooth(phase));
                    rise = Mathf.Lerp(5f, 8f, Smooth(phase));
                }
                else if (progress < 0.74f)
                {
                    float phase = (progress - 0.34f) / 0.40f;
                    scale = 1f;
                    rise = Mathf.Lerp(8f, 46f, Smooth(phase));
                }
                else
                {
                    float phase = (progress - 0.74f) / 0.26f;
                    scale = Mathf.Lerp(1f, 0.97f, phase);
                    rise = Mathf.Lerp(46f, 72f, Smooth(phase));
                }

                instance.rect.localPosition =
                    instance.startLocalPosition + new Vector3(0f, rise, 0f);
                instance.rect.localScale = instance.baseScale * scale;
                instance.group.alpha = progress < 0.72f
                    ? 1f
                    : 1f - Smooth((progress - 0.72f) / 0.28f);
                if (progress >= 1f)
                {
                    RecycleAt(index);
                }
            }
        }

        private FloatingInstance Acquire(
            BattleSandboxAuthoredTmpStyle style,
            TextMeshProUGUI template)
        {
            Queue<FloatingInstance> pool = ResolvePool(style);
            while (pool.Count > 0)
            {
                FloatingInstance candidate = pool.Dequeue();
                if (candidate?.gameObject != null)
                {
                    candidate.style = style;
                    return candidate;
                }
            }

            GameObject clone = Instantiate(
                template.gameObject,
                runtimeRoot,
                false);
            clone.name =
                "AuthoredTmpClone_"
                + (style == BattleSandboxAuthoredTmpStyle.PrimaryWarm
                    ? "Primary"
                    : "Secondary");
            clone.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            TextMeshProUGUI label =
                clone.GetComponent<TextMeshProUGUI>();
            RectTransform rect = clone.GetComponent<RectTransform>();
            if (label == null || rect == null)
            {
                DestroyTransient(clone);
                return null;
            }

            CanvasGroup group = clone.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = clone.AddComponent<CanvasGroup>();
            }
            clone.SetActive(false);
            return new FloatingInstance
            {
                gameObject = clone,
                rect = rect,
                label = label,
                group = group,
                style = style
            };
        }

        private void RecycleAt(int index)
        {
            FloatingInstance instance = active[index];
            active.RemoveAt(index);
            if (instance?.gameObject == null)
            {
                return;
            }

            instance.group.alpha = 0f;
            instance.label.text = string.Empty;
            instance.gameObject.SetActive(false);
            Queue<FloatingInstance> pool = ResolvePool(instance.style);
            if (pool.Count < MaxPoolPerStyle)
            {
                pool.Enqueue(instance);
            }
            else
            {
                DestroyTransient(instance.gameObject);
            }
        }

        private Queue<FloatingInstance> ResolvePool(
            BattleSandboxAuthoredTmpStyle style)
        {
            return style == BattleSandboxAuthoredTmpStyle.PrimaryWarm
                ? primaryPool
                : secondaryPool;
        }

        private int NextLane(string role)
        {
            int sequence = laneSequences.TryGetValue(role, out int current)
                ? current
                : 0;
            laneSequences[role] = sequence + 1;
            return sequence % 3;
        }

        private void ResolveTemplates()
        {
            if (primaryTemplate != null && secondaryTemplate != null)
            {
                return;
            }

            TextMeshProUGUI[] sceneText =
                Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (primaryTemplate == null)
            {
                primaryTemplate = sceneText.FirstOrDefault(value =>
                    string.Equals(
                        value.gameObject.name,
                        PrimaryTemplateName,
                        StringComparison.Ordinal));
            }
            if (secondaryTemplate == null)
            {
                secondaryTemplate = sceneText.FirstOrDefault(value =>
                    string.Equals(
                        value.gameObject.name,
                        SecondaryTemplateName,
                        StringComparison.Ordinal));
            }
        }

        private TextMeshProUGUI ResolveTemplate(
            BattleSandboxAuthoredTmpStyle style)
        {
            return style == BattleSandboxAuthoredTmpStyle.PrimaryWarm
                ? primaryTemplate
                : secondaryTemplate;
        }

        public static bool TryResolveItemSourceGradient(
            string baseItemId,
            out VertexGradient gradient)
        {
            switch ((baseItemId ?? string.Empty).Trim())
            {
                case "I007":
                    gradient = CreateGradient(
                        new Color32(255, 244, 165, 255),
                        new Color32(255, 65, 35, 255));
                    return true;
                case "I008":
                    gradient = CreateGradient(
                        new Color32(255, 251, 181, 255),
                        new Color32(255, 135, 24, 255));
                    return true;
                case "I009":
                    gradient = CreateGradient(
                        new Color32(255, 255, 246, 255),
                        new Color32(232, 31, 44, 255));
                    return true;
                case "I010":
                    gradient = CreateGradient(
                        new Color32(255, 232, 132, 255),
                        new Color32(21, 205, 216, 255));
                    return true;
                case "I011":
                    gradient = CreateGradient(
                        new Color32(255, 208, 111, 255),
                        new Color32(176, 47, 216, 255));
                    return true;
                case "I012":
                    gradient = CreateGradient(
                        new Color32(255, 253, 196, 255),
                        new Color32(255, 44, 24, 255));
                    return true;
                case NianGeneratedPaletteKey:
                    gradient = CreateGradient(
                        new Color32(255, 244, 137, 255),
                        new Color32(27, 222, 180, 255));
                    return true;
                case NianSpentPaletteKey:
                    gradient = CreateGradient(
                        new Color32(255, 182, 61, 255),
                        new Color32(176, 40, 137, 255));
                    return true;
                case SettlementHpPaletteKey:
                    gradient = CreateGradient(
                        new Color32(255, 238, 225, 255),
                        new Color32(153, 24, 31, 255));
                    return true;
                case SettlementShellPaletteKey:
                    gradient = CreateGradient(
                        new Color32(255, 250, 174, 255),
                        new Color32(218, 126, 24, 255));
                    return true;
                case SettlementBreakPaletteKey:
                    gradient = CreateGradient(
                        new Color32(255, 247, 224, 255),
                        new Color32(255, 198, 49, 255));
                    return true;
                default:
                    gradient = default;
                    return false;
            }
        }

        private static VertexGradient CreateGradient(
            Color top,
            Color bottom)
        {
            Color topRight = Color.Lerp(top, Color.white, 0.12f);
            Color bottomRight = Color.Lerp(bottom, top, 0.10f);
            return new VertexGradient(
                top,
                topRight,
                bottom,
                bottomRight);
        }

        private bool EnsureRuntimeChineseFallback(
            TextMeshProUGUI template)
        {
            TMP_FontAsset templateFont = template?.font;
            if (templateFont == null
                || !string.Equals(
                    templateFont.name,
                    RequiredFontAssetName,
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (runtimeChineseFallback != null
                && authoredFontAsset == templateFont)
            {
                return true;
            }

            RestoreRuntimeChineseFallback();
            string sourceGuid =
                templateFont.creationSettings.sourceFontFileGUID
                ?? string.Empty;
            if (!string.Equals(
                    sourceGuid,
                    RequiredSourceFontGuid,
                    StringComparison.Ordinal))
            {
                Debug.LogError(
                    "[I031 Nian Presentation] Authored TMP source font "
                    + "GUID mismatch: "
                    + sourceGuid);
                return false;
            }

            Font sourceFont = ResolveRequiredSourceFont(sourceGuid);
            if (sourceFont == null)
            {
                Debug.LogError(
                    "[I031 Nian Presentation] Exact authored source OTF "
                    + "could not be loaded in the current Player.");
                return false;
            }

            TMP_FontAsset fallback = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                92,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);
            if (fallback == null)
            {
                return false;
            }

            fallback.name =
                RequiredFontAssetName + " Runtime Chinese Fallback";
            fallback.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            if (fallback.material != null)
            {
                fallback.material.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
            if (fallback.atlasTextures != null)
            {
                foreach (Texture2D atlas in fallback.atlasTextures)
                {
                    if (atlas != null)
                    {
                        atlas.hideFlags =
                            HideFlags.DontSaveInEditor
                            | HideFlags.DontSaveInBuild;
                    }
                }
            }

            authoredFontAsset = templateFont;
            authoredFallbackSnapshot =
                templateFont.fallbackFontAssetTable == null
                    ? new List<TMP_FontAsset>()
                    : new List<TMP_FontAsset>(
                        templateFont.fallbackFontAssetTable);
            List<TMP_FontAsset> runtimeFallbacks =
                new(authoredFallbackSnapshot);
            runtimeFallbacks.RemoveAll(value => value == null);
            runtimeFallbacks.Add(fallback);
            templateFont.fallbackFontAssetTable = runtimeFallbacks;
            runtimeChineseFallback = fallback;
            return EnsureMessageGlyphs(ChinesePrewarmCharacters);
        }

        private static Font ResolveRequiredSourceFont(string sourceGuid)
        {
#if UNITY_EDITOR
            string sourcePath =
                UnityEditor.AssetDatabase.GUIDToAssetPath(sourceGuid);
            Font editorFont =
                UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (editorFont != null)
            {
                return editorFont;
            }
#endif
            Font[] loadedFonts = Resources.FindObjectsOfTypeAll<Font>()
                .Where(value => value != null)
                .Distinct()
                .ToArray();
            return loadedFonts.FirstOrDefault(value => string.Equals(
                       value.name,
                       RequiredSourceFontAssetName,
                       StringComparison.Ordinal))
                   ?? loadedFonts.FirstOrDefault(value => string.Equals(
                       value.name,
                       "MFLangSongJianYuan",
                       StringComparison.Ordinal));
        }

        private bool EnsureMessageGlyphs(string message)
        {
            if (runtimeChineseFallback == null)
            {
                return false;
            }

            string nonAscii = new string(
                (message ?? string.Empty)
                    .Where(value => value > 126)
                    .Distinct()
                    .ToArray());
            if (nonAscii.Length == 0)
            {
                return true;
            }

            if (nonAscii.All(value =>
                    runtimeChineseFallback.HasCharacter(
                        value,
                        false,
                        false)))
            {
                return true;
            }

            bool added = runtimeChineseFallback.TryAddCharacters(
                nonAscii,
                out string missingCharacters,
                true);
            bool allAvailableAfterAdd = nonAscii.All(value =>
                runtimeChineseFallback.HasCharacter(
                    value,
                    false,
                    false));
            if ((!added || !string.IsNullOrEmpty(missingCharacters))
                && !allAvailableAfterAdd)
            {
                Debug.LogError(
                    "[I031 Nian Presentation] Exact authored OTF is "
                    + "missing requested glyphs: "
                    + (missingCharacters ?? nonAscii));
                return false;
            }

            return allAvailableAfterAdd;
        }

        private void RestoreRuntimeChineseFallback()
        {
            if (authoredFontAsset != null)
            {
                authoredFontAsset.fallbackFontAssetTable =
                    authoredFallbackSnapshot == null
                        ? new List<TMP_FontAsset>()
                        : new List<TMP_FontAsset>(
                            authoredFallbackSnapshot);
            }

            if (runtimeChineseFallback != null)
            {
                Material runtimeMaterial =
                    runtimeChineseFallback.material;
                Texture2D[] runtimeAtlases =
                    runtimeChineseFallback.atlasTextures == null
                        ? Array.Empty<Texture2D>()
                        : runtimeChineseFallback.atlasTextures.ToArray();
                DestroyTransient(runtimeChineseFallback);
                DestroyTransient(runtimeMaterial);
                foreach (Texture2D atlas in runtimeAtlases)
                {
                    DestroyTransient(atlas);
                }
            }

            runtimeChineseFallback = null;
            authoredFallbackSnapshot = null;
            authoredFontAsset = null;
        }

        private bool EnsureRuntimeRoot(TextMeshProUGUI template)
        {
            if (runtimeRoot != null)
            {
                return true;
            }

            Canvas canvas = template == null ? null : template.canvas;
            Canvas rootCanvas = canvas == null ? null : canvas.rootCanvas;
            RectTransform canvasRect =
                rootCanvas == null
                    ? null
                    : rootCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return false;
            }

            GameObject rootObject = new(
                RuntimeRootName,
                typeof(RectTransform),
                typeof(CanvasGroup));
            rootObject.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            rootObject.transform.SetParent(canvasRect, false);
            runtimeRoot = rootObject.GetComponent<RectTransform>();
            runtimeRoot.anchorMin = Vector2.zero;
            runtimeRoot.anchorMax = Vector2.one;
            runtimeRoot.offsetMin = Vector2.zero;
            runtimeRoot.offsetMax = Vector2.zero;
            runtimeRoot.pivot = new Vector2(0.5f, 0.5f);
            runtimeRoot.localScale = Vector3.one;
            runtimeRoot.SetAsLastSibling();
            CanvasGroup group = rootObject.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            return true;
        }

        private static void CopyAuthoredTmpStyle(
            TextMeshProUGUI source,
            TextMeshProUGUI destination)
        {
            destination.font = source.font;
            destination.fontSharedMaterial = source.fontSharedMaterial;
            destination.fontSize = source.fontSize;
            destination.fontSizeMin = source.fontSizeMin;
            destination.fontSizeMax = source.fontSizeMax;
            destination.enableAutoSizing = source.enableAutoSizing;
            destination.fontStyle = source.fontStyle;
            destination.fontWeight = source.fontWeight;
            destination.alignment = source.alignment;
            destination.color = source.color;
            destination.enableVertexGradient =
                source.enableVertexGradient;
            destination.colorGradient = source.colorGradient;
            destination.colorGradientPreset =
                source.colorGradientPreset;
            destination.characterSpacing = source.characterSpacing;
            destination.wordSpacing = source.wordSpacing;
            destination.lineSpacing = source.lineSpacing;
            destination.paragraphSpacing = source.paragraphSpacing;
            destination.enableWordWrapping =
                source.enableWordWrapping;
            destination.overflowMode = source.overflowMode;
            destination.margin = source.margin;
            destination.richText = source.richText;
            destination.extraPadding = source.extraPadding;
            destination.parseCtrlCharacters =
                source.parseCtrlCharacters;
            destination.geometrySortingOrder =
                source.geometrySortingOrder;
        }

        private void ClearAll()
        {
            active.Clear();
            primaryPool.Clear();
            secondaryPool.Clear();
            acceptedRoleKeys.Clear();
            laneSequences.Clear();
            peakLiveCount = 0;
            rejectedDuplicateCount = 0;
            anonymousSequence = 0L;
            if (runtimeRoot != null)
            {
                runtimeRoot.gameObject.SetActive(false);
                DestroyTransient(runtimeRoot.gameObject);
                runtimeRoot = null;
            }
            RestoreRuntimeChineseFallback();
            primaryTemplate = null;
            secondaryTemplate = null;
        }

        private void OnDisable()
        {
            EndGeneration();
        }

        private static float Smooth(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * (3f - 2f * clamped);
        }

        private static void DestroyTransient(UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(value);
            }
            else
            {
                DestroyImmediate(value);
            }
        }
    }
}
