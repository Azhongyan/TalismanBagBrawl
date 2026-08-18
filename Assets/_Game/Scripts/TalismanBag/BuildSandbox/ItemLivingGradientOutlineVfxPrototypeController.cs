using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.Items;
using TalismanBag.Items.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(650)]
    public sealed class
        ItemLivingGradientOutlineVfxPrototypeController :
            MonoBehaviour
    {
        public const KeyCode OutlineToggleKey = KeyCode.F10;
        public const KeyCode ControlledPreviewTriggerKey =
            KeyCode.F11;
        private const string BoardArtworkNamePrefix =
            "BoardPlacedArtwork_";

        private sealed class RecentTrigger
        {
            public int serial;
            public float startedAt;
            public float expiresAt;
            public ItemLivingGradientOutlineBuildFamily family;
            public IReadOnlyList<ItemShapeCell> occupiedCells;
            public string pulseEventId;
        }

        private sealed class DesiredTarget
        {
            public Image source;
            public string itemId;
            public string surfaceId;
            public ItemLivingGradientOutlineBuildFamily family;
            public bool persistent;
            public RecentTrigger trigger;
            public bool isPreviewTarget;
        }

        private sealed class ActiveBinding
        {
            public ItemLivingGradientOutlineVfx effect;
            public Image source;
            public string itemId;
            public ItemLivingGradientOutlineBuildFamily family;
            public int appliedTriggerSerial;
        }

        private static bool bootstrapRegistered;
        private static ItemLivingGradientOutlineProfile
            activeProfile;

        private readonly ItemLivingGradientOutlineAcceptedEventGate
            acceptedEventGate = new();
        private readonly ItemLivingGradientOutlineMaterialLibrary
            materialLibrary = new();
        private readonly Dictionary<int, ActiveBinding> activeBindings =
            new();
        private readonly List<ItemLivingGradientOutlineVfx> pool = new();
        private readonly Dictionary<string, RecentTrigger>
            recentTriggers = new(StringComparer.Ordinal);

        private ShougunuPhase1BattleSandboxVerticalSliceRuntime
            battleRuntime;
        private IBattleSandboxAcceptedDamagePresentationSource
            acceptedPresentationSource;
        private BuildGridInteractionPreviewController gridController;
        private ItemLivingGradientOutlineProfile profile;
        private bool initialized;
        private bool outlineEnabled = true;
        private int observedGeneration;
        private string observedPresentationSourceId = string.Empty;
        private int triggerSerial;
        private int realAcceptedTriggerCount;
        private int controlledPreviewTriggerCount;
        private int ignoredAcceptedEventCount;
        private int peakActiveEffectCount;
        private int lifetimeEffectRootCount;
        private float nextRebindAt;
        private string lastObservedAcceptedKey = string.Empty;
        private string lastAcceptedPulseEventId = string.Empty;
        private string lastDiagnostic = "NOT_INITIALIZED";

        public static ItemLivingGradientOutlineProfile ActiveProfile =>
            activeProfile;
        public bool DevOnly => true;
        public bool OwnsBattleTruth => false;
        public bool AcknowledgesAcceptedEvents => false;
        public bool WritesSceneLayout => false;
        public bool UsesUnscaledPresentationTime => true;
        public bool Initialized => initialized;
        public bool OutlineEnabled => outlineEnabled;
        public int ActiveEffectCount => activeBindings.Count;
        public int PooledEffectCount => pool.Count;
        public int RuntimeEffectRootCount =>
            activeBindings.Count + pool.Count;
        public int ActiveGraphicCount =>
            activeBindings.Values.Sum(value =>
                value?.effect == null
                    ? 0
                    : value.effect.AddedGraphicCount);
        public int SharedMaterialCount =>
            materialLibrary.MaterialCount;
        public int RealAcceptedTriggerCount =>
            realAcceptedTriggerCount;
        public int ControlledPreviewTriggerCount =>
            controlledPreviewTriggerCount;
        public int IgnoredAcceptedEventCount =>
            ignoredAcceptedEventCount;
        public int EventGateAcceptedCount =>
            acceptedEventGate.AcceptedCount;
        public int EventGateRejectedCount =>
            acceptedEventGate.RejectedCount;
        public int PeakActiveEffectCount =>
            peakActiveEffectCount;
        public int LifetimeEffectRootCount =>
            lifetimeEffectRootCount;
        public int ActiveEffectCap =>
            profile?.activeEffectCap ?? 0;
        public string PreviewTargetItemId =>
            profile?.previewTargetItemId ?? string.Empty;
        public string LastAcceptedPulseEventId =>
            lastAcceptedPulseEventId;
        public string LastDiagnostic => lastDiagnostic;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBootstrap()
        {
            if (bootstrapRegistered)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
            bootstrapRegistered = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToCurrentScene()
        {
            TryAttach(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            TryAttach(scene);
        }

        private static void TryAttach(Scene scene)
        {
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.name,
                    ItemLivingGradientOutlineProfile.TargetSceneName,
                    StringComparison.Ordinal))
            {
                return;
            }

            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                Resources.FindObjectsOfTypeAll<
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime>()
                    .FirstOrDefault(value => value != null
                        && value.gameObject.scene == scene);
            if (runtime == null)
            {
                Debug.LogWarning(
                    "["
                    + ItemLivingGradientOutlineProfile.PackageId
                    + "] P3 runtime was not found; outline VFX stays "
                    + "disabled and Battle continues.");
                return;
            }
            if (runtime.GetComponent<
                    ItemLivingGradientOutlineVfxPrototypeController>()
                == null)
            {
                runtime.gameObject.AddComponent<
                    ItemLivingGradientOutlineVfxPrototypeController>();
            }
        }

        public void SetOutlineEnabled(bool enabled)
        {
            outlineEnabled = enabled;
            if (!outlineEnabled)
            {
                ReleaseAllActive();
                lastDiagnostic = "OUTLINE_DISABLED";
            }
            else
            {
                nextRebindAt = 0f;
                lastDiagnostic = "OUTLINE_ENABLED";
            }
        }

        public bool PlayControlledTriggerPreview()
        {
            if (!initialized
                || profile == null
                || string.IsNullOrWhiteSpace(
                    profile.previewTargetItemId))
            {
                return false;
            }
            ItemLivingGradientOutlineBuildFamily family =
                profile.ResolveBuildFamily(
                    profile.previewTargetBuildFamilyStableKey);
            if (family
                != ItemLivingGradientOutlineBuildFamily.LiHuo)
            {
                return false;
            }

            controlledPreviewTriggerCount++;
            RegisterTrigger(
                profile.previewTargetItemId,
                family,
                Array.Empty<ItemShapeCell>(),
                "CONTROLLED_PREVIEW_"
                + controlledPreviewTriggerCount,
                Time.unscaledTime);
            lastDiagnostic = "CONTROLLED_TRIGGER_PREVIEW";
            return true;
        }

        private void Awake()
        {
            if (!string.Equals(
                    gameObject.scene.name,
                    ItemLivingGradientOutlineProfile.TargetSceneName,
                    StringComparison.Ordinal))
            {
                enabled = false;
                return;
            }
            Initialize();
        }

        private void OnEnable()
        {
            if (!initialized)
            {
                Initialize();
            }
            nextRebindAt = 0f;
        }

        private void Initialize()
        {
            battleRuntime = GetComponent<
                ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
            gridController = battleRuntime?.GridController;
            profile = ItemLivingGradientOutlineProfile.Load();
            activeProfile = profile;
            if (battleRuntime == null
                || gridController == null
                || !materialLibrary.Initialize(profile))
            {
                initialized = false;
                lastDiagnostic =
                    "SAFE_DISABLED_BINDING_OR_MATERIAL_MISSING";
                return;
            }

            acceptedPresentationSource =
                ResolveAcceptedPresentationSource();
            observedGeneration =
                acceptedPresentationSource?.PresentationGeneration ?? 0;
            observedPresentationSourceId =
                acceptedPresentationSource?.PresentationSourceId
                ?? string.Empty;
            acceptedEventGate.Reset(observedGeneration);
            lastObservedAcceptedKey = string.Empty;
            initialized = true;
            lastDiagnostic = "READY";
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(OutlineToggleKey))
            {
                SetOutlineEnabled(!outlineEnabled);
                Debug.Log(
                    "["
                    + ItemLivingGradientOutlineProfile.PackageId
                    + "] F10 outline="
                    + outlineEnabled);
            }
            if (Input.GetKeyDown(ControlledPreviewTriggerKey))
            {
                PlayControlledTriggerPreview();
            }
            if (!initialized)
            {
                return;
            }

            if (battleRuntime == null
                || gridController == null)
            {
                battleRuntime = GetComponent<
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
                gridController = battleRuntime?.GridController;
                if (battleRuntime == null || gridController == null)
                {
                    lastDiagnostic = "BINDING_LOST_SAFE_DISABLED";
                    ReleaseAllActive();
                    return;
                }
            }

            acceptedPresentationSource =
                ResolveAcceptedPresentationSource();
            int generation =
                acceptedPresentationSource?.PresentationGeneration ?? 0;
            string sourceId =
                acceptedPresentationSource?.PresentationSourceId
                ?? string.Empty;
            if (generation != observedGeneration
                || !string.Equals(
                    sourceId,
                    observedPresentationSourceId,
                    StringComparison.Ordinal))
            {
                observedGeneration = generation;
                observedPresentationSourceId = sourceId;
                acceptedEventGate.Reset(generation);
                lastObservedAcceptedKey = string.Empty;
                recentTriggers.Clear();
                ReleaseAllActive();
                nextRebindAt = 0f;
                lastDiagnostic = "GENERATION_RESET";
            }

            ObserveAcceptedPresentation();
            float now = Time.unscaledTime;
            PruneExpiredTriggers(now);
            if (!outlineEnabled)
            {
                return;
            }
            if (now < nextRebindAt)
            {
                return;
            }

            nextRebindAt =
                now + profile.rebindIntervalSeconds;
            RefreshBindings(now);
        }

        private void ObserveAcceptedPresentation()
        {
            IBattleSandboxAcceptedDamagePresentation accepted =
                acceptedPresentationSource
                    ?.LastAcceptedDamagePresentation;
            if (accepted == null)
            {
                return;
            }

            string observedKey =
                ItemLivingGradientOutlineAcceptedEventGate.BuildKey(
                    accepted.ResetGeneration,
                    accepted.BattleTick,
                    accepted.EventId);
            if (string.Equals(
                    observedKey,
                    lastObservedAcceptedKey,
                    StringComparison.Ordinal))
            {
                return;
            }
            lastObservedAcceptedKey = observedKey;
            if (!acceptedEventGate.TryAccept(
                    accepted,
                    acceptedPresentationSource
                        .PresentationGeneration))
            {
                ignoredAcceptedEventCount++;
                lastDiagnostic =
                    "ACCEPTED_EVENT_IGNORED_"
                    + acceptedEventGate.LastRejectReason;
                return;
            }

            ItemCombatEffectRequestRow request =
                ResolveAcceptedRequest(accepted);
            ItemLivingGradientOutlineBuildFamily family =
                profile.ResolveBuildFamily(
                    request?.sourceFaMenTag);
            // TaiBai is deliberately a typed palette reservation only in
            // V1. No TaiBai event behavior is routed by this prototype.
            if (family
                != ItemLivingGradientOutlineBuildFamily.LiHuo)
            {
                ignoredAcceptedEventCount++;
                lastDiagnostic =
                    family
                        == ItemLivingGradientOutlineBuildFamily
                            .TaiBaiReserved
                        ? "TAIBAI_EVENT_ROUTING_RESERVED"
                        : "NON_LIHUO_ACCEPTED_EVENT_IGNORED";
                return;
            }

            realAcceptedTriggerCount++;
            lastAcceptedPulseEventId = accepted.EventId;
            RegisterTrigger(
                accepted.SourceBaseItemId,
                family,
                accepted.OccupiedCells,
                accepted.EventId,
                Time.unscaledTime);
            lastDiagnostic = "REAL_ACCEPTED_TRIGGER_PLAYING";
        }

        private ItemCombatEffectRequestRow ResolveAcceptedRequest(
            IBattleSandboxAcceptedDamagePresentation accepted)
        {
            ItemCombatEffectRequestSnapshot snapshot =
                acceptedPresentationSource
                    ?.CurrentPresentationItemRequestSnapshot;
            if (accepted == null || snapshot?.Requests == null)
            {
                return null;
            }
            return snapshot.Requests.FirstOrDefault(value =>
                value != null
                && string.Equals(
                    value.sourceItemInstanceId,
                    accepted.SourceItemInstanceId,
                    StringComparison.Ordinal)
                && string.Equals(
                    value.sourceBaseItemId,
                    accepted.SourceBaseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    value.sourcePlacementId,
                    accepted.SourcePlacementId,
                    StringComparison.Ordinal));
        }

        private void RegisterTrigger(
            string itemId,
            ItemLivingGradientOutlineBuildFamily family,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            string pulseEventId,
            float now)
        {
            string stableItemId = (itemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(stableItemId))
            {
                return;
            }
            triggerSerial++;
            RecentTrigger trigger = new()
            {
                serial = triggerSerial,
                startedAt = now,
                expiresAt = now + profile.triggerSettleSeconds,
                family = family,
                occupiedCells = (occupiedCells
                        ?? Array.Empty<ItemShapeCell>())
                    .Select(value =>
                        new ItemShapeCell(value.x, value.y))
                    .Distinct()
                    .ToArray(),
                pulseEventId = pulseEventId ?? string.Empty
            };
            recentTriggers[stableItemId] = trigger;
            foreach (ActiveBinding binding in activeBindings.Values
                         .Where(value => value != null
                             && string.Equals(
                                 value.itemId,
                                 stableItemId,
                                 StringComparison.Ordinal)))
            {
                binding.effect.PlayTrigger(now);
                binding.appliedTriggerSerial = trigger.serial;
            }
            nextRebindAt = 0f;
        }

        private void PruneExpiredTriggers(float now)
        {
            foreach (string key in recentTriggers
                         .Where(pair => pair.Value == null
                             || now >= pair.Value.expiresAt)
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                recentTriggers.Remove(key);
            }
        }

        private void RefreshBindings(float now)
        {
            List<DesiredTarget> desired =
                BuildDesiredTargets();
            int cap = profile.activeEffectCap;
            desired = desired
                .Where(value => value?.source != null)
                .GroupBy(value => value.source.GetInstanceID())
                .Select(group => group.First())
                .OrderByDescending(value => value.trigger != null)
                .ThenByDescending(value => value.isPreviewTarget)
                .ThenByDescending(value =>
                    string.Equals(
                        value.surfaceId,
                        "Board",
                        StringComparison.Ordinal))
                .ThenByDescending(value => value.persistent)
                .Take(cap)
                .ToList();

            HashSet<int> desiredIds = new(
                desired.Select(value =>
                    value.source.GetInstanceID()));
            foreach (int sourceId in activeBindings.Keys
                         .Where(value => !desiredIds.Contains(value))
                         .ToArray())
            {
                ReleaseBinding(sourceId);
            }

            foreach (DesiredTarget target in desired)
            {
                ApplyTarget(target, now);
            }
            peakActiveEffectCount = Mathf.Max(
                peakActiveEffectCount,
                activeBindings.Count);
            lastDiagnostic =
                activeBindings.Count > 0
                    ? "BINDINGS_ACTIVE"
                    : "NO_ELIGIBLE_VISIBLE_ARTWORK";
        }

        private List<DesiredTarget> BuildDesiredTargets()
        {
            Dictionary<string, ItemLivingGradientOutlineBuildFamily>
                persistentItems =
                    new(StringComparer.Ordinal);
            string previewTarget =
                (profile.previewTargetItemId ?? string.Empty).Trim();
            ItemLivingGradientOutlineBuildFamily previewFamily =
                profile.ResolveBuildFamily(
                    profile.previewTargetBuildFamilyStableKey);
            if (!string.IsNullOrWhiteSpace(previewTarget)
                && previewFamily
                    == ItemLivingGradientOutlineBuildFamily.LiHuo)
            {
                persistentItems[previewTarget] = previewFamily;
            }

            ItemSystemSnapshot snapshot =
                gridController.CurrentItemSystemBoardSnapshot;
            foreach (ItemSystemPlacementSnapshot placement in
                     snapshot?.placements
                     ?? Array.Empty<ItemSystemPlacementSnapshot>())
            {
                if (placement == null
                    || !placement.isLit
                    || string.IsNullOrWhiteSpace(placement.itemId))
                {
                    continue;
                }
                ItemLivingGradientOutlineBuildFamily family =
                    ResolveBuildFamilyForItem(placement.itemId);
                if (family
                    == ItemLivingGradientOutlineBuildFamily.LiHuo)
                {
                    persistentItems[placement.itemId] = family;
                }
            }
            foreach (KeyValuePair<string, RecentTrigger> pair in
                     recentTriggers)
            {
                if (pair.Value?.family
                    == ItemLivingGradientOutlineBuildFamily.LiHuo)
                {
                    if (!persistentItems.ContainsKey(pair.Key))
                    {
                        persistentItems.Add(
                            pair.Key, pair.Value.family);
                    }
                }
            }

            List<DesiredTarget> result = new();
            AppendTrayTargets(
                persistentItems,
                previewTarget,
                result);
            AppendBoardTargets(
                persistentItems,
                previewTarget,
                snapshot,
                result);
            return result;
        }

        private void AppendTrayTargets(
            IReadOnlyDictionary<
                string,
                ItemLivingGradientOutlineBuildFamily> items,
            string previewTarget,
            ICollection<DesiredTarget> result)
        {
            foreach (BuildItemPreviewCardView card in
                     Resources.FindObjectsOfTypeAll<
                         BuildItemPreviewCardView>())
            {
                if (card == null
                    || card.gameObject.scene != gameObject.scene
                    || !items.TryGetValue(
                        card.ItemId ?? string.Empty,
                        out ItemLivingGradientOutlineBuildFamily family)
                    || !card.HasAuthoritativeArtworkRenderLease
                    || card.UsesFallbackArtworkRenderLease
                    || !card.IsArtworkEffectivelyRendering)
                {
                    continue;
                }

                Image artwork = card.AuthoritativeArtworkImage;
                if (!IsSafeTrayArtwork(artwork))
                {
                    continue;
                }
                recentTriggers.TryGetValue(
                    card.ItemId, out RecentTrigger trigger);
                result.Add(new DesiredTarget
                {
                    source = artwork,
                    itemId = card.ItemId,
                    surfaceId = "Tray",
                    family = family,
                    persistent = items.ContainsKey(card.ItemId),
                    trigger = trigger,
                    isPreviewTarget = string.Equals(
                        card.ItemId,
                        previewTarget,
                        StringComparison.Ordinal)
                });
            }
        }

        private void AppendBoardTargets(
            IReadOnlyDictionary<
                string,
                ItemLivingGradientOutlineBuildFamily> items,
            string previewTarget,
            ItemSystemSnapshot snapshot,
            ICollection<DesiredTarget> result)
        {
            foreach (KeyValuePair<
                         string,
                         ItemLivingGradientOutlineBuildFamily> pair in items)
            {
                ItemSystemPlacementSnapshot placement =
                    snapshot?.placements?.FirstOrDefault(value =>
                        value != null
                        && string.Equals(
                            value.itemId,
                            pair.Key,
                            StringComparison.Ordinal));
                recentTriggers.TryGetValue(
                    pair.Key, out RecentTrigger trigger);
                IReadOnlyList<ItemShapeCell> cells =
                    placement != null
                        ? placement.OccupiedCells
                            .Select(value =>
                                new ItemShapeCell(value.x, value.y))
                            .ToArray()
                        : trigger?.occupiedCells
                            ?? Array.Empty<ItemShapeCell>();
                if (!gridController.TryResolveBoardItemFeedbackAnchor(
                        pair.Key,
                        cells,
                        out RectTransform artworkRect,
                        out _,
                        out _,
                        out _)
                    || artworkRect == null
                    || !artworkRect.gameObject.activeInHierarchy
                    || !artworkRect.name.StartsWith(
                        BoardArtworkNamePrefix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                Image artwork = artworkRect.GetComponent<Image>();
                if (!IsSafeBoardArtwork(artwork))
                {
                    continue;
                }
                result.Add(new DesiredTarget
                {
                    source = artwork,
                    itemId = pair.Key,
                    surfaceId = "Board",
                    family = pair.Value,
                    persistent =
                        placement?.isLit == true
                        || string.Equals(
                            pair.Key,
                            previewTarget,
                            StringComparison.Ordinal),
                    trigger = trigger,
                    isPreviewTarget = string.Equals(
                        pair.Key,
                        previewTarget,
                        StringComparison.Ordinal)
                });
            }
        }

        private void ApplyTarget(
            DesiredTarget target,
            float now)
        {
            int sourceId = target.source.GetInstanceID();
            if (activeBindings.TryGetValue(
                    sourceId, out ActiveBinding existing))
            {
                if (existing.family != target.family
                    || existing.effect == null
                    || !existing.effect.SyncFromSource())
                {
                    ReleaseBinding(sourceId);
                }
                else
                {
                    existing.effect.SetPersistent(target.persistent);
                    ApplyTriggerIfNeeded(existing, target.trigger, now);
                    return;
                }
            }

            ItemLivingGradientOutlinePaletteProfile palette =
                profile.ResolvePalette(target.family);
            if (!materialLibrary.TryGet(
                    target.family,
                    out ItemLivingGradientOutlineMaterialSet materials)
                || palette == null)
            {
                return;
            }
            ItemLivingGradientOutlineVfx effect = AcquireEffect();
            if (effect == null
                || !effect.Bind(
                    target.source,
                    target.itemId,
                    target.surfaceId,
                    target.family,
                    palette,
                    materials))
            {
                if (effect != null)
                {
                    ReturnToPool(effect);
                }
                return;
            }

            ActiveBinding binding = new()
            {
                effect = effect,
                source = target.source,
                itemId = target.itemId,
                family = target.family
            };
            activeBindings[sourceId] = binding;
            effect.SetPersistent(target.persistent);
            ApplyTriggerIfNeeded(binding, target.trigger, now);
        }

        private void ApplyTriggerIfNeeded(
            ActiveBinding binding,
            RecentTrigger trigger,
            float now)
        {
            if (trigger == null
                || binding.appliedTriggerSerial == trigger.serial)
            {
                return;
            }
            binding.effect.PlayTrigger(trigger.startedAt);
            binding.effect.EvaluateAt(now);
            binding.appliedTriggerSerial = trigger.serial;
        }

        private ItemLivingGradientOutlineVfx AcquireEffect()
        {
            if (pool.Count > 0)
            {
                int last = pool.Count - 1;
                ItemLivingGradientOutlineVfx effect = pool[last];
                pool.RemoveAt(last);
                return effect;
            }
            if (RuntimeEffectRootCount >= profile.activeEffectCap)
            {
                return null;
            }

            ItemLivingGradientOutlineVfx created =
                ItemLivingGradientOutlineVfx.CreatePooled(
                    transform,
                    lifetimeEffectRootCount);
            lifetimeEffectRootCount++;
            return created;
        }

        private void ReleaseBinding(int sourceId)
        {
            if (!activeBindings.TryGetValue(
                    sourceId, out ActiveBinding binding))
            {
                return;
            }
            activeBindings.Remove(sourceId);
            if (binding?.effect == null)
            {
                return;
            }
            ReturnToPool(binding.effect);
        }

        private void ReturnToPool(
            ItemLivingGradientOutlineVfx effect)
        {
            effect.ReleaseToPool(transform);
            if (!pool.Contains(effect))
            {
                pool.Add(effect);
            }
        }

        private void ReleaseAllActive()
        {
            foreach (int sourceId in activeBindings.Keys.ToArray())
            {
                ReleaseBinding(sourceId);
            }
        }

        private ItemLivingGradientOutlineBuildFamily
            ResolveBuildFamilyForItem(string itemId)
        {
            ItemCombatEffectRequestSnapshot snapshot =
                acceptedPresentationSource
                    ?.CurrentPresentationItemRequestSnapshot;
            ItemCombatEffectRequestRow request =
                snapshot?.Requests?.FirstOrDefault(value =>
                    value != null
                    && string.Equals(
                        value.sourceBaseItemId,
                        itemId,
                        StringComparison.Ordinal));
            ItemLivingGradientOutlineBuildFamily family =
                profile.ResolveBuildFamily(
                    request?.sourceFaMenTag);
            if (family
                == ItemLivingGradientOutlineBuildFamily.Unknown
                && string.Equals(
                    itemId,
                    profile.previewTargetItemId,
                    StringComparison.Ordinal))
            {
                family = profile.ResolveBuildFamily(
                    profile.previewTargetBuildFamilyStableKey);
            }
            return family;
        }

        private IBattleSandboxAcceptedDamagePresentationSource
            ResolveAcceptedPresentationSource()
        {
            IBattleSandboxAcceptedDamagePresentationSource[] sources =
                Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .OfType<
                        IBattleSandboxAcceptedDamagePresentationSource>()
                    .Where(value =>
                        value.HasActivePresentationSession)
                    .ToArray();
            return sources.Length == 1
                ? sources[0]
                : null;
        }

        private static bool IsSafeTrayArtwork(Image image)
        {
            return ItemLivingGradientOutlineVfx
                    .IsValidAuthoritativeArtwork(image)
                && image.gameObject.activeInHierarchy
                && image.enabled
                && image.color.a > 0.004f;
        }

        private static bool IsSafeBoardArtwork(Image image)
        {
            if (!ItemLivingGradientOutlineVfx
                    .IsValidAuthoritativeArtwork(image)
                || !image.gameObject.activeInHierarchy
                || !image.enabled
                || image.color.a <= 0.004f)
            {
                return false;
            }
            string name = image.gameObject.name;
            return name.StartsWith(
                    BoardArtworkNamePrefix,
                    StringComparison.Ordinal)
                && name.IndexOf(
                    "Feedback",
                    StringComparison.OrdinalIgnoreCase) < 0
                && name.IndexOf(
                    "Fx",
                    StringComparison.OrdinalIgnoreCase) < 0;
        }

        private void OnDisable()
        {
            CleanupRuntimeState();
        }

        private void OnDestroy()
        {
            CleanupRuntimeState();
            if (ReferenceEquals(activeProfile, profile))
            {
                activeProfile = null;
            }
        }

        private void CleanupRuntimeState()
        {
            ReleaseAllActive();
            foreach (ItemLivingGradientOutlineVfx effect in pool
                         .Where(value => value != null)
                         .ToArray())
            {
                effect.ReleaseToPool(transform);
                if (Application.isPlaying)
                {
                    Destroy(effect.gameObject);
                }
                else
                {
                    DestroyImmediate(effect.gameObject);
                }
            }
            pool.Clear();
            recentTriggers.Clear();
            materialLibrary.Dispose();
            initialized = false;
        }
    }
}
