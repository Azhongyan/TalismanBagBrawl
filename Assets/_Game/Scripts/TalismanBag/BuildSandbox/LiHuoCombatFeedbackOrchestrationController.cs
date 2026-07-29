using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.Combat;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public enum LiHuoCombatFeedbackOutcomeKind
    {
        HpDirectDamage = 0,
        ShellDamage = 1,
        ShellBreak = 2,
        NpGain = 3
    }

    public enum LiHuoCombatFeedbackBuildFamily
    {
        Unknown = 0,
        LiHuo = 1,
        TaiBaiReserved = 2
    }

    /// <summary>
    /// Pure presentation deduplication gate. It deliberately knows nothing
    /// about Battle acceptance, acknowledgement or settlement.
    /// </summary>
    public sealed class LiHuoCombatFeedbackCueGate
    {
        private readonly HashSet<string> acceptedIds =
            new(StringComparer.Ordinal);
        private int generation;
        private long lastAcceptedTick = -1L;

        public int AcceptedCount => acceptedIds.Count;
        public int Generation => generation;
        public long LastAcceptedTick => lastAcceptedTick;

        public bool TryAccept(
            int expectedGeneration,
            long battleTick,
            string eventId)
        {
            if (expectedGeneration <= 0
                || battleTick <= 0L
                || string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            if (generation != expectedGeneration)
            {
                Reset(expectedGeneration);
            }

            string stableId = eventId.Trim();
            if (battleTick < lastAcceptedTick
                || !acceptedIds.Add(stableId))
            {
                return false;
            }

            lastAcceptedTick = battleTick;
            return true;
        }

        public void Reset(int nextGeneration = 0)
        {
            generation = Mathf.Max(0, nextGeneration);
            lastAcceptedTick = -1L;
            acceptedIds.Clear();
        }
    }

    [Serializable]
    public sealed class LiHuoCombatFeedbackReplaceableProfile
    {
        public string profileId =
            "lihuo.combat.feedback.devonly.v1";
        public string buildFamilyStableKey = "lihuo";
        public string taiBaiReservedStableKey = "taibai";
        public string atlasResourcePath =
            "V04/LiHuoCombatFeedbackDevOnly/lihuo_vfx_atlas_alpha_v1";
        public string optionalUiMaterialResourcePath = string.Empty;
        public string sourceSfxResourcePath = string.Empty;
        public string travelSfxResourcePath = string.Empty;
        public string hpImpactSfxResourcePath = string.Empty;
        public string shellImpactSfxResourcePath = string.Empty;
        public int atlasColumns = 4;
        public int atlasRows = 4;
        public int poolSize = 64;
        public float sourceDurationSeconds = 0.075f;
        public float travelDelaySeconds = 0.035f;
        public float travelDurationSeconds = 0.17f;
        public float impactEmphasisSeconds = 0.045f;
        public float afterglowSeconds = 0.58f;
        public float sourceVolume = 0.13f;
        public float travelVolume = 0.09f;
        public float impactVolume = 0.24f;
        public string hpCoreHex = "#E73020";
        public string hpMidHex = "#FF6A20";
        public string hpHighlightHex = "#FFD768";
        public string shellCoreHex = "#EAA42A";
        public string shellMidHex = "#FFD45B";
        public string shellHighlightHex = "#FFF3D5";
        public string npCoreHex = "#28D9C4";
        public string npHighlightHex = "#B8FFF2";
    }

    /// <summary>
    /// Dev-only, replaceable combat-presentation sink. It consumes the
    /// accepted event published by P3 after Battle has already settled it.
    /// Presentation can fail or be disabled without changing Battle truth.
    ///
    /// The full-resolution 4x4 atlas is intentionally prototype-only. It is
    /// loaded once, sliced into runtime Sprite wrappers once and never copied
    /// per event. Production should replace this with an authored VFX profile.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(900)]
    public sealed class LiHuoCombatFeedbackOrchestrationController :
        MonoBehaviour
    {
        public const string PackageId =
            "V0.4-LiHuoCombatFeedbackOrchestrationVerticalSlice01";
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string ProfileResourcesPath =
            "V04/LiHuoCombatFeedbackDevOnly/lihuo_combat_feedback_profile";
        public const string RuntimeRootName =
            "LiHuoCombatFeedbackOrchestration_Runtime";
        public const string ShougunuDevControlsName =
            "ShougunuPhase1_DevOnlyControls";
        public const KeyCode AbToggleKey = KeyCode.F7;
        public const KeyCode CleanModeToggleKey = KeyCode.F8;

        private sealed class PooledVisual
        {
            public GameObject gameObject;
            public RectTransform rect;
            public Image image;
            public CanvasGroup group;
            public int ownerSerial;
        }

        private sealed class ParticleMotion
        {
            public PooledVisual visual;
            public Vector2 direction;
            public float speed;
            public float rotationSpeed;
            public float scale;
            public float delay;
            public float drag;
        }

        private static bool bootstrapRegistered;

        private readonly LiHuoCombatFeedbackCueGate cueGate = new();
        private readonly List<PooledVisual> pool = new();
        private readonly List<PooledVisual> activeVisuals = new();
        private readonly List<Sprite> atlasSprites = new();
        private readonly List<Sprite> ownedSprites = new();
        private readonly List<Texture2D> ownedTextures = new();
        private readonly List<AudioClip> ownedAudioClips = new();
        private readonly List<GameObject> cleanSuppressedObjects = new();

        private ShougunuPhase1BattleSandboxVerticalSliceRuntime battleRuntime;
        private ShougunuPhase1BattleSandboxSceneBinder sceneBinder;
        private ShougunuPhase1BattleSandboxVisualCueAdapter visualCueAdapter;
        private BattleSandboxItemTriggerFeedbackController
            legacyItemFeedback;
        private ShougunuPhase1VisualPrototypeController
            shougunuPresentation;
        private BattleSandboxAuthoredTmpPresentation authoredTmpPresentation;
        private LiHuoCombatFeedbackReplaceableProfile profile;

        private RectTransform runtimeRoot;
        private RectTransform backLayer;
        private RectTransform frontLayer;
        private Texture2D loadedAtlas;
        private Sprite softCircleSprite;
        private Sprite diamondSprite;
        private Material optionalUiMaterial;

        private AudioSource sourceAudio;
        private AudioSource travelAudio;
        private AudioSource impactAudio;
        private AudioClip sourceClip;
        private AudioClip travelClip;
        private AudioClip hpImpactClip;
        private AudioClip shellImpactClip;

        private Coroutine activeChain;
        private int activeChainSerial;
        private int observedGeneration;
        private int enhancedAcceptedCount;
        private int rejectedCueCount;
        private int interruptedChainCount;
        private int peakActiveVisualCount;
        private int lifetimeSpriteAllocationCount;
        private bool initialized;
        private bool enhancedMode = true;
        private bool cleanMode;
        private bool warnedBinding;
        private bool warnedResources;
        private float bindingWaitStartedAt;
        private bool previousCleanObjectState;
        private GameObject cleanTarget;
        private string lastAcceptedEventId = string.Empty;
        private string lastDiagnostic = "NOT_INITIALIZED";
        private LiHuoCombatFeedbackOutcomeKind lastOutcome;
        private LiHuoCombatFeedbackBuildFamily lastBuildFamily;

        public bool DevOnly => true;
        public bool OwnsBattleTruth => false;
        public bool AcknowledgesBattleEvents => false;
        public bool WritesSceneLayout => false;
        public bool EnhancedMode => enhancedMode;
        public bool CleanMode => cleanMode;
        public bool Initialized => initialized;
        public int AcceptedPresentationCount => enhancedAcceptedCount;
        public int RejectedCueCount => rejectedCueCount;
        public int InterruptedChainCount => interruptedChainCount;
        public int ActiveVisualCount =>
            activeVisuals.Count(value => value?.gameObject != null
                && value.gameObject.activeSelf);
        public int PeakActiveVisualCount => peakActiveVisualCount;
        public int LifetimeSpriteAllocationCount =>
            lifetimeSpriteAllocationCount;
        public int RuntimePoolSize => pool.Count;
        public int CueGateAcceptedCount => cueGate.AcceptedCount;
        public int CurrentGeneration => observedGeneration;
        public string LastAcceptedEventId => lastAcceptedEventId;
        public string LastDiagnostic => lastDiagnostic;
        public LiHuoCombatFeedbackOutcomeKind LastOutcome => lastOutcome;
        public LiHuoCombatFeedbackBuildFamily LastBuildFamily =>
            lastBuildFamily;
        public string LoadedProfileId => profile?.profileId ?? string.Empty;
        public string LoadedAtlasName =>
            loadedAtlas == null ? string.Empty : loadedAtlas.name;
        public bool HasTypedShougunuPresentationFacade =>
            shougunuPresentation != null;
        public bool HasRequiredAuthoredTmpBaseline =>
            authoredTmpPresentation != null
            && authoredTmpPresentation.HasAuthoredTemplates
            && string.Equals(
                authoredTmpPresentation.AuthoredFontAssetName,
                BattleSandboxAuthoredTmpPresentation
                    .RequiredFontAssetName,
                StringComparison.Ordinal);
        public int GeneratedNpPresentationCount =>
            sceneBinder?.GeneratedNpPresentationCount ?? 0;
        public int SpentNpPresentationCount =>
            sceneBinder?.SpentNpPresentationCount ?? 0;
        public int AuthoredTmpAcceptedRoleCount =>
            authoredTmpPresentation?.AcceptedRoleCount ?? 0;
        public bool UsesItemArtworkAsProjectile => false;
        public bool UsesUnscaledTime => true;
        public bool HasPerLoopSpriteAllocation => false;

        /// <summary>
        /// Presentation-only QA/handtest switch. It changes neither Battle
        /// state nor authored layout and is intentionally absent in Players
        /// unless this devOnly component is present.
        /// </summary>
        public void SetPresentationModes(
            bool useEnhancedPresentation,
            bool useCleanDisplay)
        {
            if (enhancedMode != useEnhancedPresentation)
            {
                enhancedMode = useEnhancedPresentation;
                CancelActiveChain("PUBLIC_AB_TOGGLE");
            }
            if (cleanMode != useCleanDisplay)
            {
                SetCleanMode(useCleanDisplay);
            }
        }

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
                    TargetSceneName,
                    StringComparison.Ordinal))
            {
                return;
            }

            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                Resources
                    .FindObjectsOfTypeAll<
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime>()
                    .FirstOrDefault(value => value != null
                        && value.gameObject.scene == scene);
            if (runtime == null)
            {
                Debug.LogWarning(
                    "[" + PackageId + "] P3 BattleSandbox runtime was "
                    + "not found; authored presentation remains unchanged.");
                return;
            }

            if (runtime.GetComponent<
                    LiHuoCombatFeedbackOrchestrationController>() == null)
            {
                runtime.gameObject.AddComponent<
                    LiHuoCombatFeedbackOrchestrationController>();
            }
        }

        private void Awake()
        {
            if (!string.Equals(
                    gameObject.scene.name,
                    TargetSceneName,
                    StringComparison.Ordinal))
            {
                enabled = false;
                return;
            }

            battleRuntime = GetComponent<
                ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
            profile = LoadProfile();
            ResolveBindings();
            if (!TryInitializePresentation())
            {
                lastDiagnostic = "INITIALIZATION_PENDING";
            }
        }

        private void OnEnable()
        {
            bindingWaitStartedAt = Time.unscaledTime;
            ResolveBindings();
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(AbToggleKey))
            {
                enhancedMode = !enhancedMode;
                CancelActiveChain("AB_TOGGLE");
                lastDiagnostic = enhancedMode
                    ? "ENHANCED_MODE"
                    : "LEGACY_MODE";
                Debug.Log(
                    "[" + PackageId + "] F7 A/B = "
                    + (enhancedMode ? "enhanced" : "legacy"));
            }

            if (Input.GetKeyDown(CleanModeToggleKey))
            {
                SetCleanMode(!cleanMode);
            }

            if (!ResolveBindings())
            {
                if (!warnedBinding
                    && Time.unscaledTime - bindingWaitStartedAt >= 3f)
                {
                    warnedBinding = true;
                    Debug.LogWarning(
                        "[" + PackageId + "] Required public "
                        + "presentation bindings are not ready; Battle "
                        + "continues with authored presentation.");
                }
                return;
            }

            if (!initialized && !TryInitializePresentation())
            {
                return;
            }

            int generation = battleRuntime.ResetGeneration;
            if (generation != observedGeneration)
            {
                observedGeneration = generation;
                cueGate.Reset(generation);
                CancelActiveChain("GENERATION_CHANGED");
                lastAcceptedEventId = string.Empty;
            }

            if (!enhancedMode
                || !battleRuntime.HasActiveSession
                || generation <= 0)
            {
                return;
            }

            BattleSandboxAcceptedItemPresentationEvent accepted =
                battleRuntime.LastAcceptedItemPresentation;
            if (accepted == null
                || string.Equals(
                    accepted.pulseEventId,
                    lastAcceptedEventId,
                    StringComparison.Ordinal))
            {
                return;
            }

            TryConsumeAcceptedEvent(accepted);
        }

        private void TryConsumeAcceptedEvent(
            BattleSandboxAcceptedItemPresentationEvent accepted)
        {
            if (accepted == null
                || !accepted.IsAcceptedCorrelation
                || accepted.resetGeneration != observedGeneration)
            {
                rejectedCueCount++;
                lastDiagnostic = "ACCEPTED_CORRELATION_REJECTED";
                return;
            }

            ItemCombatEffectRequestRow request =
                ResolveAcceptedRequest(accepted);
            LiHuoCombatFeedbackBuildFamily family =
                ResolveBuildFamily(
                    request?.sourceFaMenTag,
                    profile?.buildFamilyStableKey,
                    profile?.taiBaiReservedStableKey);
            if (family != LiHuoCombatFeedbackBuildFamily.LiHuo)
            {
                rejectedCueCount++;
                lastAcceptedEventId = accepted.pulseEventId;
                lastDiagnostic = family
                    == LiHuoCombatFeedbackBuildFamily.TaiBaiReserved
                        ? "TAIBAI_PROFILE_RESERVED_NOT_IMPLEMENTED"
                        : "NON_LIHUO_EVENT_IGNORED";
                return;
            }

            bool shellBreak = HasCorrelatedShellBreak(accepted);
            LiHuoCombatFeedbackOutcomeKind outcome =
                ResolveOutcomeKind(accepted, shellBreak);
            if (!cueGate.TryAccept(
                    accepted.resetGeneration,
                    accepted.battleTick,
                    accepted.pulseEventId))
            {
                rejectedCueCount++;
                lastDiagnostic = "MONOTONIC_OR_DUPLICATE_REJECTED";
                return;
            }

            if (!TryResolveEndpoints(
                    accepted,
                    out Vector3 sourceWorld,
                    out Vector3 targetWorld))
            {
                rejectedCueCount++;
                lastAcceptedEventId = accepted.pulseEventId;
                lastDiagnostic = "PRESENTATION_ENDPOINT_MISSING";
                return;
            }

            // P3 has already accepted and published the presentation event.
            // Removing the legacy transient here is presentation-only A/B:
            // it never changes the accepted event, ledger or Battle display.
            legacyItemFeedback?.ClearAll();
            CancelActiveChain("NEW_ACCEPTED_EVENT");

            lastAcceptedEventId = accepted.pulseEventId;
            lastOutcome = outcome;
            lastBuildFamily = family;
            enhancedAcceptedCount++;
            activeChainSerial++;
            int serial = activeChainSerial;
            activeChain = StartCoroutine(PlayAcceptedChain(
                serial,
                accepted,
                outcome,
                sourceWorld,
                targetWorld));
            lastDiagnostic = "CHAIN_PLAYING";
        }

        private IEnumerator PlayAcceptedChain(
            int serial,
            BattleSandboxAcceptedItemPresentationEvent accepted,
            LiHuoCombatFeedbackOutcomeKind outcome,
            Vector3 sourceWorld,
            Vector3 targetWorld)
        {
            Color core = ResolveCoreColor(outcome);
            Color middle = ResolveMiddleColor(outcome);
            Color highlight = ResolveHighlightColor(outcome);
            int seed = StableHash(accepted.pulseEventId);

            PlayBeat(
                sourceAudio,
                sourceClip,
                profile.sourceVolume,
                seed,
                0);
            PooledVisual sourcePulse = ShowVisual(
                serial,
                backLayer,
                AtlasSprite(0),
                sourceWorld,
                new Vector2(116f, 116f),
                WithAlpha(core, 0.72f),
                0f);
            PooledVisual sourceHalo = ShowVisual(
                serial,
                frontLayer,
                AtlasSprite(1),
                sourceWorld,
                new Vector2(88f, 88f),
                WithAlpha(highlight, 0.52f),
                0f);

            float elapsed = 0f;
            float sourceDuration =
                Mathf.Clamp(profile.sourceDurationSeconds, 0.04f, 0.12f);
            while (elapsed < sourceDuration
                && ChainIsCurrent(serial))
            {
                elapsed += Time.unscaledDeltaTime;
                float phase = Mathf.Clamp01(elapsed / sourceDuration);
                AnimateVisual(
                    sourcePulse,
                    sourceWorld,
                    new Vector2(
                        Mathf.Lerp(86f, 142f, Smooth(phase)),
                        Mathf.Lerp(86f, 142f, Smooth(phase))),
                    WithAlpha(core, Mathf.Lerp(0.76f, 0.18f, phase)),
                    phase * 54f);
                AnimateVisual(
                    sourceHalo,
                    sourceWorld,
                    new Vector2(
                        Mathf.Lerp(62f, 108f, phase),
                        Mathf.Lerp(62f, 108f, phase)),
                    WithAlpha(highlight, Mathf.Sin(phase * Mathf.PI) * 0.72f),
                    -phase * 38f);
                yield return null;
            }
            ReleaseVisual(sourcePulse);
            ReleaseVisual(sourceHalo);

            float travelDelay =
                Mathf.Clamp(profile.travelDelaySeconds, 0f, 0.08f);
            elapsed = 0f;
            while (elapsed < travelDelay && ChainIsCurrent(serial))
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            PlayBeat(
                travelAudio,
                travelClip,
                profile.travelVolume,
                seed,
                1);

            PooledVisual projectile = ShowVisual(
                serial,
                frontLayer,
                AtlasSprite(4),
                sourceWorld,
                new Vector2(112f, 70f),
                WithAlpha(Color.white, 0.96f),
                0f);
            List<PooledVisual> trail = new();
            for (int index = 0; index < 8; index++)
            {
                PooledVisual trailNode = ShowVisual(
                    serial,
                    backLayer,
                    softCircleSprite,
                    sourceWorld,
                    new Vector2(46f, 18f),
                    Color.clear,
                    0f);
                trail.Add(trailNode);
            }

            Vector3 control =
                (sourceWorld + targetWorld) * 0.5f
                + Vector3.up * Mathf.Clamp(
                    Vector3.Distance(sourceWorld, targetWorld) * 0.10f,
                    34f,
                    86f);
            float travelDuration =
                Mathf.Clamp(profile.travelDurationSeconds, 0.11f, 0.26f);
            elapsed = 0f;
            while (elapsed < travelDuration
                && ChainIsCurrent(serial))
            {
                elapsed += Time.unscaledDeltaTime;
                float phase = Mathf.Clamp01(elapsed / travelDuration);
                float eased = Smooth(phase);
                Vector3 position = QuadraticBezier(
                    sourceWorld,
                    control,
                    targetWorld,
                    eased);
                float nextPhase = Mathf.Min(1f, eased + 0.02f);
                Vector3 tangent = QuadraticBezier(
                    sourceWorld,
                    control,
                    targetWorld,
                    nextPhase) - position;
                float angle =
                    Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                projectile.image.sprite = AtlasSprite(
                    4 + Mathf.Clamp(
                        Mathf.FloorToInt(phase * 4f),
                        0,
                        3));
                AnimateVisual(
                    projectile,
                    position,
                    new Vector2(
                        Mathf.Lerp(104f, 144f, phase),
                        Mathf.Lerp(62f, 82f, phase)),
                    Color.white,
                    angle);
                for (int index = 0; index < trail.Count; index++)
                {
                    float back = (index + 1f) * 0.045f;
                    float sample = Mathf.Clamp01(eased - back);
                    Vector3 trailPosition = QuadraticBezier(
                        sourceWorld,
                        control,
                        targetWorld,
                        sample);
                    float alpha = Mathf.Clamp01(
                        phase * 4f - index * 0.16f)
                        * (1f - index / 9f)
                        * 0.52f;
                    AnimateVisual(
                        trail[index],
                        trailPosition,
                        new Vector2(
                            Mathf.Lerp(54f, 22f, index / 8f),
                            Mathf.Lerp(20f, 8f, index / 8f)),
                        WithAlpha(
                            Color.Lerp(middle, highlight, index / 8f),
                            alpha),
                        angle);
                }
                yield return null;
            }
            ReleaseVisual(projectile);
            ReleaseVisuals(trail);

            PlayBeat(
                impactAudio,
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? hpImpactClip
                    : shellImpactClip,
                profile.impactVolume,
                seed,
                2);

            List<PooledVisual> impactNodes = new();
            List<ParticleMotion> particles = new();
            SpawnImpact(
                serial,
                outcome,
                targetWorld,
                seed,
                core,
                middle,
                highlight,
                impactNodes,
                particles);

            float emphasis =
                Mathf.Clamp(profile.impactEmphasisSeconds, 0.035f, 0.055f);
            elapsed = 0f;
            while (elapsed < emphasis && ChainIsCurrent(serial))
            {
                elapsed += Time.unscaledDeltaTime;
                float phase = Mathf.Clamp01(elapsed / emphasis);
                AnimateImpactNodes(
                    impactNodes,
                    targetWorld,
                    outcome,
                    phase,
                    core,
                    middle,
                    highlight,
                    true);
                AnimateParticles(
                    particles,
                    targetWorld,
                    elapsed,
                    emphasis,
                    0f);
                yield return null;
            }

            float afterglow =
                Mathf.Clamp(profile.afterglowSeconds, 0.42f, 0.80f);
            elapsed = 0f;
            while (elapsed < afterglow && ChainIsCurrent(serial))
            {
                elapsed += Time.unscaledDeltaTime;
                float phase = Mathf.Clamp01(elapsed / afterglow);
                AnimateImpactNodes(
                    impactNodes,
                    targetWorld,
                    outcome,
                    phase,
                    core,
                    middle,
                    highlight,
                    false);
                AnimateParticles(
                    particles,
                    targetWorld,
                    elapsed,
                    afterglow,
                    emphasis);
                yield return null;
            }

            ReleaseVisuals(impactNodes);
            foreach (ParticleMotion particle in particles)
            {
                ReleaseVisual(particle?.visual);
            }
            if (ChainIsCurrent(serial))
            {
                activeChain = null;
                lastDiagnostic = "CHAIN_COMPLETE";
            }
        }

        private void SpawnImpact(
            int serial,
            LiHuoCombatFeedbackOutcomeKind outcome,
            Vector3 targetWorld,
            int seed,
            Color core,
            Color middle,
            Color highlight,
            List<PooledVisual> impactNodes,
            List<ParticleMotion> particles)
        {
            bool shell =
                outcome == LiHuoCombatFeedbackOutcomeKind.ShellDamage
                || outcome == LiHuoCombatFeedbackOutcomeKind.ShellBreak;
            bool shellBreak =
                outcome == LiHuoCombatFeedbackOutcomeKind.ShellBreak;

            impactNodes.Add(ShowVisual(
                serial,
                backLayer,
                softCircleSprite,
                targetWorld,
                shell
                    ? new Vector2(226f, 166f)
                    : new Vector2(196f, 196f),
                WithAlpha(core, shell ? 0.26f : 0.34f),
                0f));
            impactNodes.Add(ShowVisual(
                serial,
                frontLayer,
                shell ? AtlasSprite(11) : AtlasSprite(10),
                targetWorld,
                shell
                    ? new Vector2(
                        shellBreak ? 260f : 212f,
                        shellBreak ? 182f : 148f)
                    : new Vector2(214f, 214f),
                WithAlpha(Color.white, 0.94f),
                0f));
            impactNodes.Add(ShowVisual(
                serial,
                frontLayer,
                shell ? AtlasSprite(15) : AtlasSprite(8),
                targetWorld + Vector3.down * (shell ? 44f : 20f),
                shell
                    ? new Vector2(216f, 88f)
                    : new Vector2(152f, 152f),
                WithAlpha(
                    shell ? highlight : middle,
                    shell ? 0.68f : 0.78f),
                0f));

            int particleCount = shell
                ? (shellBreak ? 18 : 12)
                : 14;
            for (int index = 0; index < particleCount; index++)
            {
                float angle = DeterministicRange(
                    seed,
                    index * 7 + 3,
                    shell ? 8f : -32f,
                    shell ? 172f : 212f);
                float radians = angle * Mathf.Deg2Rad;
                Vector2 direction =
                    new(Mathf.Cos(radians), Mathf.Sin(radians));
                float speed = DeterministicRange(
                    seed,
                    index * 7 + 4,
                    shell ? 82f : 58f,
                    shell ? 188f : 154f);
                float scale = DeterministicRange(
                    seed,
                    index * 7 + 5,
                    0.70f,
                    1.26f);
                Sprite sprite = shell
                    ? diamondSprite
                    : (index % 4 == 0
                        ? AtlasSprite(14)
                        : softCircleSprite);
                Vector2 size = shell
                    ? new Vector2(12f, 26f) * scale
                    : (index % 4 == 0
                        ? new Vector2(46f, 46f) * scale
                        : new Vector2(12f, 12f) * scale);
                Color color = Color.Lerp(
                    middle,
                    highlight,
                    DeterministicRange(seed, index * 7 + 6, 0f, 1f));
                PooledVisual visual = ShowVisual(
                    serial,
                    frontLayer,
                    sprite,
                    targetWorld,
                    size,
                    WithAlpha(color, shell ? 0.92f : 0.78f),
                    angle);
                particles.Add(new ParticleMotion
                {
                    visual = visual,
                    direction = direction,
                    speed = speed,
                    rotationSpeed = DeterministicRange(
                        seed,
                        index * 7 + 7,
                        -240f,
                        240f),
                    scale = scale,
                    delay = DeterministicRange(
                        seed,
                        index * 7 + 8,
                        0f,
                        0.08f),
                    drag = DeterministicRange(
                        seed,
                        index * 7 + 9,
                        0.62f,
                        0.86f)
                });
            }
        }

        private void AnimateImpactNodes(
            IReadOnlyList<PooledVisual> nodes,
            Vector3 targetWorld,
            LiHuoCombatFeedbackOutcomeKind outcome,
            float phase,
            Color core,
            Color middle,
            Color highlight,
            bool emphasis)
        {
            if (nodes == null || nodes.Count < 3)
            {
                return;
            }

            bool shell =
                outcome == LiHuoCombatFeedbackOutcomeKind.ShellDamage
                || outcome == LiHuoCombatFeedbackOutcomeKind.ShellBreak;
            float fade = emphasis
                ? Mathf.Lerp(0.72f, 1f, Smooth(phase))
                : 1f - Smooth(phase);
            float expand = emphasis
                ? Mathf.Lerp(0.78f, 1.08f, Smooth(phase))
                : Mathf.Lerp(1.08f, shell ? 1.58f : 1.42f, Smooth(phase));
            AnimateVisual(
                nodes[0],
                targetWorld,
                new Vector2(210f, shell ? 154f : 210f) * expand,
                WithAlpha(core, (shell ? 0.28f : 0.36f) * fade),
                shell ? -phase * 12f : phase * 22f);
            AnimateVisual(
                nodes[1],
                targetWorld,
                new Vector2(shell ? 230f : 204f, shell ? 164f : 204f)
                    * expand,
                WithAlpha(Color.white, 0.94f * fade),
                (shell ? 34f : -28f) * phase);
            AnimateVisual(
                nodes[2],
                targetWorld + Vector3.down * (shell ? 44f : 20f),
                new Vector2(shell ? 214f : 150f, shell ? 88f : 150f)
                    * Mathf.Lerp(0.86f, 1.28f, Smooth(phase)),
                WithAlpha(
                    shell ? highlight : middle,
                    (shell ? 0.72f : 0.82f) * fade),
                shell ? phase * 8f : -phase * 20f);
        }

        private static void AnimateParticles(
            IReadOnlyList<ParticleMotion> particles,
            Vector3 origin,
            float elapsed,
            float duration,
            float carriedTime)
        {
            if (particles == null)
            {
                return;
            }

            foreach (ParticleMotion particle in particles)
            {
                if (particle?.visual == null)
                {
                    continue;
                }

                float localTime = Mathf.Max(
                    0f,
                    elapsed + carriedTime - particle.delay);
                float normalized = Mathf.Clamp01(
                    localTime / Mathf.Max(0.01f, duration + carriedTime));
                float distance =
                    particle.speed
                    * localTime
                    * Mathf.Lerp(1f, particle.drag, normalized);
                Vector2 gravity =
                    Vector2.down * 42f * localTime * localTime;
                Vector3 position =
                    origin
                    + (Vector3)(particle.direction * distance + gravity);
                Vector2 baseSize =
                    particle.visual.image.sprite == null
                    || particle.visual.image.sprite.name
                        .IndexOf(
                            "Diamond",
                            StringComparison.OrdinalIgnoreCase) < 0
                        ? new Vector2(18f, 18f) * particle.scale
                        : new Vector2(12f, 26f) * particle.scale;
                Color color = particle.visual.image.color;
                color.a = (1f - Smooth(normalized))
                    * Mathf.Clamp01(1f - particle.delay * 3f);
                AnimateVisual(
                    particle.visual,
                    position,
                    baseSize * Mathf.Lerp(1f, 0.56f, normalized),
                    color,
                    particle.visual.rect.localEulerAngles.z
                        + particle.rotationSpeed
                        * Time.unscaledDeltaTime);
            }
        }

        private bool TryResolveEndpoints(
            BattleSandboxAcceptedItemPresentationEvent accepted,
            out Vector3 sourceWorld,
            out Vector3 targetWorld)
        {
            sourceWorld = Vector3.zero;
            targetWorld = Vector3.zero;
            RectTransform targetAnchor =
                visualCueAdapter?.DamageDealtAnchor;
            if (legacyItemFeedback == null
                || targetAnchor == null
                || !legacyItemFeedback
                    .TryResolveBoardSourceWorldPosition(
                        accepted.sourceBaseItemId,
                        accepted.OccupiedCells,
                        out sourceWorld))
            {
                return false;
            }

            targetWorld =
                targetAnchor.TransformPoint(targetAnchor.rect.center);
            return true;
        }

        private ItemCombatEffectRequestRow ResolveAcceptedRequest(
            BattleSandboxAcceptedItemPresentationEvent accepted)
        {
            ItemCombatEffectRequestSnapshot snapshot =
                battleRuntime?.CurrentItemRequestSnapshot;
            if (accepted == null || snapshot?.Requests == null)
            {
                return null;
            }

            return snapshot.Requests.FirstOrDefault(value =>
                value != null
                && string.Equals(
                    value.sourceItemInstanceId,
                    accepted.sourceItemInstanceId,
                    StringComparison.Ordinal)
                && string.Equals(
                    value.sourceBaseItemId,
                    accepted.sourceBaseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    value.sourcePlacementId,
                    accepted.sourcePlacementId,
                    StringComparison.Ordinal));
        }

        private bool HasCorrelatedShellBreak(
            BattleSandboxAcceptedItemPresentationEvent accepted)
        {
            IReadOnlyList<ShougunuPhase1BattleDisplayEvent> events =
                battleRuntime?.CurrentContext?.DisplayEvents;
            return accepted != null
                && events != null
                && events.Any(value => value != null
                    && value.resetGeneration == accepted.resetGeneration
                    && value.battleTick == accepted.battleTick
                    && string.Equals(
                        value.ledgerEventId,
                        accepted.damageLedgerEventId,
                        StringComparison.Ordinal)
                    && value.channel
                        == ShougunuPhase1BattleDisplayChannel
                            .EnemyShellBreak);
        }

        public static LiHuoCombatFeedbackOutcomeKind ResolveOutcomeKind(
            BattleSandboxAcceptedItemPresentationEvent accepted,
            bool correlatedShellBreak)
        {
            if (correlatedShellBreak)
            {
                return LiHuoCombatFeedbackOutcomeKind.ShellBreak;
            }
            if (accepted?.shellDamageApplied > 0)
            {
                return LiHuoCombatFeedbackOutcomeKind.ShellDamage;
            }
            return LiHuoCombatFeedbackOutcomeKind.HpDirectDamage;
        }

        public static LiHuoCombatFeedbackBuildFamily ResolveBuildFamily(
            string faMenStableKey,
            string liHuoStableKey = null,
            string taiBaiStableKey = null)
        {
            string stable = (faMenStableKey ?? string.Empty).Trim();
            string liHuo = string.IsNullOrWhiteSpace(liHuoStableKey)
                ? ItemFaMenTag.Lihuo.ToStableKey()
                : liHuoStableKey.Trim();
            string taiBai = string.IsNullOrWhiteSpace(taiBaiStableKey)
                ? ItemFaMenTag.Taibai.ToStableKey()
                : taiBaiStableKey.Trim();
            if (string.Equals(stable, liHuo, StringComparison.Ordinal))
            {
                return LiHuoCombatFeedbackBuildFamily.LiHuo;
            }
            if (string.Equals(stable, taiBai, StringComparison.Ordinal))
            {
                return LiHuoCombatFeedbackBuildFamily.TaiBaiReserved;
            }
            return LiHuoCombatFeedbackBuildFamily.Unknown;
        }

        private bool ResolveBindings()
        {
            battleRuntime ??= GetComponent<
                ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
            sceneBinder ??= battleRuntime?.SceneBinder;
            visualCueAdapter ??= sceneBinder?.VisualCueAdapter;
            legacyItemFeedback ??=
                sceneBinder?.AcceptedItemFeedbackController;
            authoredTmpPresentation ??=
                sceneBinder?.AuthoredTmpPresentation;
            if (shougunuPresentation == null)
            {
                shougunuPresentation =
                    Resources
                        .FindObjectsOfTypeAll<
                            ShougunuPhase1VisualPrototypeController>()
                        .FirstOrDefault(value => value != null
                            && value.gameObject.scene
                                == gameObject.scene);
            }

            return battleRuntime != null
                && sceneBinder != null
                && visualCueAdapter != null
                && legacyItemFeedback != null
                && visualCueAdapter.DamageDealtAnchor != null;
        }

        private bool TryInitializePresentation()
        {
            if (initialized)
            {
                return true;
            }
            if (!ResolveBindings())
            {
                return false;
            }

            Canvas canvas =
                visualCueAdapter.DamageDealtAnchor
                    .GetComponentInParent<Canvas>();
            RectTransform canvasRect =
                canvas?.rootCanvas?.transform as RectTransform;
            if (canvasRect == null
                || !LoadAtlas()
                || !BuildRuntimeRoot(canvasRect)
                || !BuildPool())
            {
                WarnResourcesOnce(
                    "Required atlas/canvas presentation resources are "
                    + "unavailable; authored Battle presentation remains.");
                return false;
            }

            CreateProceduralSprites();
            ResolveAudio();
            initialized = softCircleSprite != null
                && diamondSprite != null
                && pool.Count > 0;
            if (initialized)
            {
                lifetimeSpriteAllocationCount =
                    ownedSprites.Count;
                lastDiagnostic = "READY";
                Debug.Log(
                    "[" + PackageId + "] READY"
                    + " profile=" + profile.profileId
                    + " atlas=" + loadedAtlas.name
                    + " sprites=" + lifetimeSpriteAllocationCount
                    + " pool=" + pool.Count
                    + " F7=A/B F8=clean");
            }
            return initialized;
        }

        private LiHuoCombatFeedbackReplaceableProfile LoadProfile()
        {
            LiHuoCombatFeedbackReplaceableProfile fallback = new();
            TextAsset text = Resources.Load<TextAsset>(
                ProfileResourcesPath);
            if (text == null || string.IsNullOrWhiteSpace(text.text))
            {
                WarnResourcesOnce(
                    "Replaceable JSON profile was not found; safe "
                    + "procedural defaults are active.");
                return fallback;
            }

            try
            {
                LiHuoCombatFeedbackReplaceableProfile loaded =
                    JsonUtility.FromJson<
                        LiHuoCombatFeedbackReplaceableProfile>(text.text);
                return loaded ?? fallback;
            }
            catch (Exception exception)
            {
                WarnResourcesOnce(
                    "Replaceable JSON profile could not be parsed ("
                    + exception.GetType().Name
                    + "); safe procedural defaults are active.");
                return fallback;
            }
        }

        private bool LoadAtlas()
        {
            loadedAtlas = Resources.Load<Texture2D>(
                profile.atlasResourcePath);
            if (loadedAtlas == null
                || loadedAtlas.width <= 0
                || loadedAtlas.height <= 0)
            {
                return false;
            }

            int columns = Mathf.Clamp(profile.atlasColumns, 1, 8);
            int rows = Mathf.Clamp(profile.atlasRows, 1, 8);
            if (loadedAtlas.width < columns
                || loadedAtlas.height < rows)
            {
                return false;
            }

            for (int topRow = 0; topRow < rows; topRow++)
            {
                int bottomRow = rows - 1 - topRow;
                for (int column = 0; column < columns; column++)
                {
                    int xMin = Mathf.RoundToInt(
                        column * loadedAtlas.width / (float)columns);
                    int xMax = Mathf.RoundToInt(
                        (column + 1)
                        * loadedAtlas.width
                        / (float)columns);
                    int yMin = Mathf.RoundToInt(
                        bottomRow * loadedAtlas.height / (float)rows);
                    int yMax = Mathf.RoundToInt(
                        (bottomRow + 1)
                        * loadedAtlas.height
                        / (float)rows);
                    Sprite sprite = Sprite.Create(
                        loadedAtlas,
                        new Rect(
                            xMin,
                            yMin,
                            Mathf.Max(1, xMax - xMin),
                            Mathf.Max(1, yMax - yMin)),
                        new Vector2(0.5f, 0.5f),
                        100f,
                        0u,
                        SpriteMeshType.FullRect);
                    sprite.name = "LiHuoAtlas_"
                        + (topRow * columns + column).ToString(
                            "D2",
                            CultureInfo.InvariantCulture);
                    sprite.hideFlags =
                        HideFlags.DontSaveInEditor
                        | HideFlags.DontSaveInBuild;
                    atlasSprites.Add(sprite);
                    ownedSprites.Add(sprite);
                }
            }

            optionalUiMaterial =
                string.IsNullOrWhiteSpace(
                    profile.optionalUiMaterialResourcePath)
                    ? null
                    : Resources.Load<Material>(
                        profile.optionalUiMaterialResourcePath.Trim());
            return atlasSprites.Count >= 16;
        }

        private bool BuildRuntimeRoot(RectTransform canvasRect)
        {
            if (runtimeRoot != null)
            {
                return true;
            }

            GameObject root = new(
                RuntimeRootName,
                typeof(RectTransform),
                typeof(CanvasGroup));
            root.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            root.layer = canvasRect.gameObject.layer;
            root.transform.SetParent(canvasRect, false);
            runtimeRoot = root.GetComponent<RectTransform>();
            Stretch(runtimeRoot);
            runtimeRoot.SetAsLastSibling();
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            backLayer = CreateLayer("BackVfx", runtimeRoot);
            frontLayer = CreateLayer("FrontVfx", runtimeRoot);
            return backLayer != null && frontLayer != null;
        }

        private bool BuildPool()
        {
            if (pool.Count > 0)
            {
                return true;
            }

            int count = Mathf.Clamp(profile.poolSize, 40, 96);
            for (int index = 0; index < count; index++)
            {
                GameObject obj = new(
                    "LiHuoPooledVfx_"
                    + index.ToString("D2", CultureInfo.InvariantCulture),
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(CanvasGroup));
                obj.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                obj.layer = runtimeRoot.gameObject.layer;
                obj.transform.SetParent(frontLayer, false);
                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
                Image image = obj.GetComponent<Image>();
                image.raycastTarget = false;
                image.color = Color.clear;
                image.material = optionalUiMaterial;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
                obj.SetActive(false);
                pool.Add(new PooledVisual
                {
                    gameObject = obj,
                    rect = rect,
                    image = image,
                    group = group
                });
            }
            return pool.Count == count;
        }

        private void CreateProceduralSprites()
        {
            if (softCircleSprite != null && diamondSprite != null)
            {
                return;
            }

            softCircleSprite = CreateProceduralSprite(
                "LiHuoSoftCircle",
                64,
                (x, y) =>
                {
                    float nx = (x + 0.5f) / 64f * 2f - 1f;
                    float ny = (y + 0.5f) / 64f * 2f - 1f;
                    float distance = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = Mathf.Clamp01(1f - distance);
                    alpha = alpha * alpha * (3f - 2f * alpha);
                    return new Color(1f, 1f, 1f, alpha);
                });
            diamondSprite = CreateProceduralSprite(
                "LiHuoBoneDiamond",
                32,
                (x, y) =>
                {
                    float nx = Mathf.Abs((x + 0.5f) / 32f * 2f - 1f);
                    float ny = Mathf.Abs((y + 0.5f) / 32f * 2f - 1f);
                    float edge = Mathf.Clamp01(
                        (1f - (nx + ny)) * 8f);
                    return new Color(1f, 1f, 1f, edge);
                });
        }

        private Sprite CreateProceduralSprite(
            string name,
            int size,
            Func<int, int, Color> sample)
        {
            Texture2D texture = new(
                size,
                size,
                TextureFormat.RGBA32,
                false,
                true)
            {
                name = name + "_Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags =
                    HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild
            };
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = sample(x, y);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            ownedTextures.Add(texture);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect);
            sprite.name = name;
            sprite.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            ownedSprites.Add(sprite);
            return sprite;
        }

        private void ResolveAudio()
        {
            sourceClip = LoadOptionalAudio(
                profile.sourceSfxResourcePath)
                ?? CreateSourceBeat();
            travelClip = LoadOptionalAudio(
                profile.travelSfxResourcePath)
                ?? CreateTravelBeat();
            hpImpactClip = LoadOptionalAudio(
                profile.hpImpactSfxResourcePath)
                ?? CreateImpactBeat(false);
            shellImpactClip = LoadOptionalAudio(
                profile.shellImpactSfxResourcePath)
                ?? CreateImpactBeat(true);

            sourceAudio = CreateAudioSource("SourceBeat");
            travelAudio = CreateAudioSource("TravelBeat");
            impactAudio = CreateAudioSource("ImpactBeat");
        }

        private AudioClip LoadOptionalAudio(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? null
                : Resources.Load<AudioClip>(path.Trim());
        }

        private AudioSource CreateAudioSource(string suffix)
        {
            AudioSource source = runtimeRoot.gameObject.AddComponent<
                AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.priority = 180;
            source.name = "LiHuo" + suffix;
            return source;
        }

        private AudioClip CreateSourceBeat()
        {
            return CreateProceduralClip(
                "LiHuo_Source_Procedural",
                0.105f,
                (time, normalized, random) =>
                {
                    float envelope =
                        Mathf.Sin(normalized * Mathf.PI)
                        * (1f - normalized);
                    float tone = Mathf.Sin(
                        2f * Mathf.PI
                        * Mathf.Lerp(760f, 420f, normalized)
                        * time);
                    return envelope * (tone * 0.72f + random * 0.10f);
                });
        }

        private AudioClip CreateTravelBeat()
        {
            return CreateProceduralClip(
                "LiHuo_Travel_Procedural",
                0.19f,
                (time, normalized, random) =>
                {
                    float envelope =
                        Mathf.Sin(normalized * Mathf.PI)
                        * (1f - normalized * 0.45f);
                    float chirp = Mathf.Sin(
                        2f * Mathf.PI
                        * Mathf.Lerp(290f, 980f, normalized)
                        * time);
                    return envelope * (chirp * 0.20f + random * 0.16f);
                });
        }

        private AudioClip CreateImpactBeat(bool shell)
        {
            return CreateProceduralClip(
                shell
                    ? "LiHuo_ShellImpact_Procedural"
                    : "LiHuo_HpImpact_Procedural",
                shell ? 0.27f : 0.235f,
                (time, normalized, random) =>
                {
                    float envelope =
                        Mathf.Exp(-normalized * (shell ? 5.2f : 6.4f));
                    float low = Mathf.Sin(
                        2f * Mathf.PI
                        * Mathf.Lerp(
                            shell ? 145f : 118f,
                            58f,
                            normalized)
                        * time);
                    float crack = random
                        * Mathf.Clamp01(1f - normalized * 1.5f);
                    return envelope
                        * (low * (shell ? 0.66f : 0.72f)
                            + crack * (shell ? 0.24f : 0.18f));
                });
        }

        private AudioClip CreateProceduralClip(
            string name,
            float duration,
            Func<float, float, float, float> sample)
        {
            const int sampleRate = 32000;
            int count = Mathf.Max(
                1,
                Mathf.CeilToInt(sampleRate * duration));
            float[] samples = new float[count];
            uint state = (uint)StableHash(name);
            for (int index = 0; index < count; index++)
            {
                state = state * 1664525u + 1013904223u;
                float random =
                    ((state >> 8) & 0x00FFFFFF) / 8388607.5f - 1f;
                float time = index / (float)sampleRate;
                float normalized = index / (float)Mathf.Max(1, count - 1);
                samples[index] = Mathf.Clamp(
                    sample(time, normalized, random),
                    -0.88f,
                    0.88f);
            }

            AudioClip clip = AudioClip.Create(
                name,
                count,
                1,
                sampleRate,
                false);
            clip.SetData(samples, 0);
            clip.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            ownedAudioClips.Add(clip);
            return clip;
        }

        private void PlayBeat(
            AudioSource source,
            AudioClip clip,
            float volume,
            int seed,
            int beat)
        {
            if (source == null || clip == null)
            {
                return;
            }

            source.Stop();
            source.pitch = DeterministicRange(
                seed,
                90 + beat,
                0.96f,
                1.035f);
            source.PlayOneShot(
                clip,
                Mathf.Clamp(volume, 0f, 0.42f));
        }

        private PooledVisual ShowVisual(
            int ownerSerial,
            RectTransform parent,
            Sprite sprite,
            Vector3 worldPosition,
            Vector2 size,
            Color color,
            float rotation)
        {
            PooledVisual visual = pool.FirstOrDefault(value =>
                value?.gameObject != null
                && !value.gameObject.activeSelf);
            if (visual == null)
            {
                return null;
            }

            visual.ownerSerial = ownerSerial;
            visual.rect.SetParent(parent ?? frontLayer, false);
            visual.rect.position = worldPosition;
            visual.rect.sizeDelta = size;
            visual.rect.localScale = Vector3.one;
            visual.rect.localEulerAngles =
                new Vector3(0f, 0f, rotation);
            visual.image.sprite = sprite;
            visual.image.type = Image.Type.Simple;
            visual.image.preserveAspect = false;
            visual.image.color = color;
            visual.image.material = optionalUiMaterial;
            visual.group.alpha = 1f;
            visual.gameObject.SetActive(true);
            visual.rect.SetAsLastSibling();
            if (!activeVisuals.Contains(visual))
            {
                activeVisuals.Add(visual);
            }
            peakActiveVisualCount = Mathf.Max(
                peakActiveVisualCount,
                ActiveVisualCount);
            return visual;
        }

        private static void AnimateVisual(
            PooledVisual visual,
            Vector3 worldPosition,
            Vector2 size,
            Color color,
            float rotation)
        {
            if (visual?.gameObject == null
                || !visual.gameObject.activeSelf)
            {
                return;
            }
            visual.rect.position = worldPosition;
            visual.rect.sizeDelta = size;
            visual.rect.localEulerAngles =
                new Vector3(0f, 0f, rotation);
            visual.image.color = color;
        }

        private void ReleaseVisual(PooledVisual visual)
        {
            if (visual?.gameObject == null)
            {
                return;
            }
            visual.group.alpha = 0f;
            visual.image.color = Color.clear;
            visual.image.sprite = null;
            visual.rect.localScale = Vector3.one;
            visual.rect.localRotation = Quaternion.identity;
            visual.ownerSerial = 0;
            visual.gameObject.SetActive(false);
            activeVisuals.Remove(visual);
        }

        private void ReleaseVisuals(
            IEnumerable<PooledVisual> visuals)
        {
            if (visuals == null)
            {
                return;
            }
            foreach (PooledVisual visual in visuals.ToArray())
            {
                ReleaseVisual(visual);
            }
        }

        private void CancelActiveChain(string reason)
        {
            bool hadActive = activeChain != null
                || ActiveVisualCount > 0;
            if (activeChain != null)
            {
                StopCoroutine(activeChain);
                activeChain = null;
            }
            foreach (PooledVisual visual in activeVisuals.ToArray())
            {
                ReleaseVisual(visual);
            }
            sourceAudio?.Stop();
            travelAudio?.Stop();
            impactAudio?.Stop();
            if (hadActive
                && !string.Equals(
                    reason,
                    "GENERATION_CHANGED",
                    StringComparison.Ordinal))
            {
                interruptedChainCount++;
            }
        }

        private bool ChainIsCurrent(int serial)
        {
            return enabled
                && enhancedMode
                && serial == activeChainSerial;
        }

        private void SetCleanMode(bool active)
        {
            cleanMode = active;
            if (active)
            {
                cleanTarget = Resources
                    .FindObjectsOfTypeAll<GameObject>()
                    .FirstOrDefault(value => value != null
                        && value.scene == gameObject.scene
                        && string.Equals(
                            value.name,
                            ShougunuDevControlsName,
                            StringComparison.Ordinal));
                if (cleanTarget != null)
                {
                    previousCleanObjectState =
                        cleanTarget.activeSelf;
                    cleanSuppressedObjects.Add(cleanTarget);
                    cleanTarget.SetActive(false);
                }
            }
            else
            {
                RestoreCleanMode();
            }
            Debug.Log(
                "[" + PackageId + "] F8 clean mode = "
                + (cleanMode ? "ON" : "OFF"));
        }

        private void RestoreCleanMode()
        {
            if (cleanTarget != null)
            {
                cleanTarget.SetActive(previousCleanObjectState);
            }
            cleanTarget = null;
            cleanSuppressedObjects.Clear();
            cleanMode = false;
        }

        private Color ResolveCoreColor(
            LiHuoCombatFeedbackOutcomeKind outcome)
        {
            return ParseColor(
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? profile.hpCoreHex
                    : outcome == LiHuoCombatFeedbackOutcomeKind.NpGain
                        ? profile.npCoreHex
                        : profile.shellCoreHex,
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? new Color(0.91f, 0.12f, 0.08f)
                    : new Color(0.93f, 0.60f, 0.12f));
        }

        private Color ResolveMiddleColor(
            LiHuoCombatFeedbackOutcomeKind outcome)
        {
            return ParseColor(
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? profile.hpMidHex
                    : outcome == LiHuoCombatFeedbackOutcomeKind.NpGain
                        ? profile.npCoreHex
                        : profile.shellMidHex,
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? new Color(1f, 0.36f, 0.08f)
                    : new Color(1f, 0.79f, 0.22f));
        }

        private Color ResolveHighlightColor(
            LiHuoCombatFeedbackOutcomeKind outcome)
        {
            return ParseColor(
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? profile.hpHighlightHex
                    : outcome == LiHuoCombatFeedbackOutcomeKind.NpGain
                        ? profile.npHighlightHex
                        : profile.shellHighlightHex,
                outcome == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                    ? new Color(1f, 0.84f, 0.36f)
                    : new Color(1f, 0.95f, 0.82f));
        }

        private Sprite AtlasSprite(int index)
        {
            return index >= 0 && index < atlasSprites.Count
                ? atlasSprites[index]
                : softCircleSprite;
        }

        private static RectTransform CreateLayer(
            string name,
            RectTransform parent)
        {
            GameObject obj = new(
                name,
                typeof(RectTransform),
                typeof(CanvasGroup));
            obj.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            obj.layer = parent.gameObject.layer;
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            Stretch(rect);
            CanvasGroup group = obj.GetComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static Vector3 QuadraticBezier(
            Vector3 start,
            Vector3 control,
            Vector3 end,
            float phase)
        {
            float t = Mathf.Clamp01(phase);
            float inverse = 1f - t;
            return inverse * inverse * start
                + 2f * inverse * t * control
                + t * t * end;
        }

        private static float Smooth(float value)
        {
            float clamped = Mathf.Clamp01(value);
            return clamped * clamped * (3f - 2f * clamped);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        private static Color ParseColor(
            string html,
            Color fallback)
        {
            return ColorUtility.TryParseHtmlString(
                html ?? string.Empty,
                out Color parsed)
                ? parsed
                : fallback;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                foreach (char character in value ?? string.Empty)
                {
                    hash = hash * 31 + character;
                }
                return hash;
            }
        }

        private static float DeterministicRange(
            int seed,
            int salt,
            float minimum,
            float maximum)
        {
            unchecked
            {
                uint state = (uint)(seed ^ (salt * 486187739));
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                float unit = (state & 0x00FFFFFF) / 16777215f;
                return Mathf.Lerp(minimum, maximum, unit);
            }
        }

        private void WarnResourcesOnce(string message)
        {
            if (warnedResources)
            {
                return;
            }
            warnedResources = true;
            Debug.LogWarning("[" + PackageId + "] " + message);
        }

        private void OnDisable()
        {
            CancelActiveChain("DISABLE");
            RestoreCleanMode();
            cueGate.Reset();
            observedGeneration = 0;
            lastAcceptedEventId = string.Empty;
        }

        private void OnDestroy()
        {
            CancelActiveChain("DESTROY");
            RestoreCleanMode();
            foreach (Sprite sprite in ownedSprites.Distinct().ToArray())
            {
                DestroyTransient(sprite);
            }
            foreach (Texture2D texture in ownedTextures.ToArray())
            {
                DestroyTransient(texture);
            }
            foreach (AudioClip clip in ownedAudioClips.ToArray())
            {
                DestroyTransient(clip);
            }
            ownedSprites.Clear();
            ownedTextures.Clear();
            ownedAudioClips.Clear();
            atlasSprites.Clear();
            pool.Clear();
            activeVisuals.Clear();
            if (runtimeRoot != null)
            {
                DestroyTransient(runtimeRoot.gameObject);
                runtimeRoot = null;
            }
            // loadedAtlas, optional material and optional resource audio are
            // Resources-owned and are intentionally never destroyed here.
            loadedAtlas = null;
            optionalUiMaterial = null;
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
