using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public sealed class C1EnemyPresentationBindingSnapshot
    {
        public C1EnemyPresentationBindingSnapshot(
            string enemyInstanceId,
            string displaySlotId,
            string visualProfileKey,
            C1EnemyPresentationVisualState visualState,
            bool visible,
            bool selected)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            DisplaySlotId = displaySlotId ?? string.Empty;
            VisualProfileKey = visualProfileKey ?? string.Empty;
            VisualState = visualState;
            Visible = visible;
            Selected = selected;
        }

        public string EnemyInstanceId { get; }
        public string DisplaySlotId { get; }
        public string VisualProfileKey { get; }
        public C1EnemyPresentationVisualState VisualState { get; }
        public bool Visible { get; }
        public bool Selected { get; }
        public string BindingKey =>
            EnemyInstanceId + "@" + DisplaySlotId;
    }

    [DisallowMultipleComponent]
    [DefaultExecutionOrder(736)]
    public sealed class
        C1BattleSandboxAuthoredEnemyPresentationAdapter : MonoBehaviour
    {
        private sealed class LoadedClip
        {
            public LoadedClip(
                C1EnemyVisualClipDefinition definition,
                Sprite[] frames)
            {
                Definition = definition;
                Frames = frames ?? Array.Empty<Sprite>();
            }

            public C1EnemyVisualClipDefinition Definition { get; }
            public Sprite[] Frames { get; }
            public float DurationSeconds =>
                Frames.Length
                * Definition.FrameDurationMilliseconds
                / 1000f;

            public Sprite FrameAt(float elapsedSeconds)
            {
                if (Frames.Length == 0)
                {
                    return null;
                }
                int frame = Mathf.FloorToInt(
                    Mathf.Max(0f, elapsedSeconds)
                    * 1000f
                    / Definition.FrameDurationMilliseconds);
                frame = Definition.Loops
                    ? frame % Frames.Length
                    : Mathf.Clamp(frame, 0, Frames.Length - 1);
                return Frames[frame];
            }
        }

        private sealed class ActorPlayback
        {
            public string VisualProfileKey = string.Empty;
            public int ResetGeneration;
            public long LastCueSequence;
            public C1EnemyPresentationVisualState State =
                C1EnemyPresentationVisualState.Idle;
            public float StateStartedAt;
        }

        private const float HitRestartSuppressionSeconds = 0.12f;

        private readonly Dictionary<
            string,
            ActorPlayback> playbacks =
                new(StringComparer.Ordinal);
        private readonly Dictionary<
            string,
            LoadedClip> loadedClips =
                new(StringComparer.Ordinal);
        private readonly List<Sprite> ownedRuntimeSprites = new();
        private readonly List<C1EnemyPresentationBindingSnapshot>
            currentBindings = new();

        private V04Chapter1BattleSandboxRuntimeController flowController;
        private C1AuthoredEnemyPresentationRoot authoredRoot;
        private bool rootSelectionSubscribed;

        public bool IsBound =>
            flowController != null && authoredRoot != null;
        public C1AuthoredEnemyPresentationRoot AuthoredRoot =>
            authoredRoot;
        public IReadOnlyList<C1EnemyPresentationBindingSnapshot>
            CurrentBindings => currentBindings;
        public string LastDiagnostic { get; private set; } =
            "SHOWCASE_ADAPTER_NOT_BOUND";

        public bool TryResolveEnemyDamageAnchor(
            string enemyInstanceId,
            out RectTransform anchor)
        {
            anchor = null;
            if (string.IsNullOrWhiteSpace(enemyInstanceId)
                || authoredRoot == null)
            {
                return false;
            }

            C1AuthoredEnemyPresentationSlotView slot =
                authoredRoot.Slots.FirstOrDefault(value =>
                    value != null
                    && string.Equals(
                        value.BoundEnemyInstanceId,
                        enemyInstanceId,
                        StringComparison.Ordinal)
                    && value.IsRuntimeVisible);
            anchor = slot?.RuntimeDamageAnchor;
            return anchor != null;
        }

        public void Bind(
            V04Chapter1BattleSandboxRuntimeController flow)
        {
            flowController = flow;
            ResolveAuthoredRoot();
        }

        private void Update()
        {
            if (flowController == null)
            {
                flowController = Resources.FindObjectsOfTypeAll<
                        V04Chapter1BattleSandboxRuntimeController>()
                    .SingleOrDefault(value => value != null
                        && value.gameObject.scene == gameObject.scene);
            }
            ResolveAuthoredRoot();
            if (flowController == null || authoredRoot == null)
            {
                currentBindings.Clear();
                LastDiagnostic =
                    "SHOWCASE_FLOW_OR_AUTHORED_ROOT_MISSING";
                return;
            }

            V04Chapter1NormalEnemyBattleAdapter battle =
                flowController.NormalBattle;
            if (flowController.EncounterAdmission?.IsOrdinary != true
                || battle == null)
            {
                authoredRoot.RestoreAuthoredBaseline();
                currentBindings.Clear();
                playbacks.Clear();
                LastDiagnostic =
                    flowController.LastDiagnosticCode
                    == V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3
                        ? C1EnemyRuntimeContract
                            .BoneSwapRemnantHoldStatus
                            + "_NO_ACTOR_NO_"
                            + C1EnemyVisualProfileCatalog
                                .TemporaryRemnantFallbackTag
                        : "NO_ORDINARY_ACTOR_ADMISSION";
                return;
            }

            RenderActorSnapshots(battle);
        }

        private void OnDisable()
        {
            UnsubscribeRootSelection();
            currentBindings.Clear();
            playbacks.Clear();
        }

        private void OnDestroy()
        {
            OnDisable();
            foreach (Sprite sprite in ownedRuntimeSprites)
            {
                if (sprite != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(sprite);
                    }
                    else
                    {
                        DestroyImmediate(sprite);
                    }
                }
            }
            ownedRuntimeSprites.Clear();
            loadedClips.Clear();
        }

        private void ResolveAuthoredRoot()
        {
            if (authoredRoot != null)
            {
                SubscribeRootSelection();
                return;
            }
            C1AuthoredEnemyPresentationRoot[] roots =
                Resources.FindObjectsOfTypeAll<
                        C1AuthoredEnemyPresentationRoot>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (roots.Length != 1)
            {
                LastDiagnostic = roots.Length == 0
                    ? "AUTHORED_PRESENTATION_ROOT_MISSING"
                    : "AUTHORED_PRESENTATION_ROOT_AMBIGUOUS_"
                        + roots.Length;
                return;
            }
            authoredRoot = roots[0];
            if (!authoredRoot.TryInitialize(
                    out string rootDiagnostic))
            {
                LastDiagnostic = rootDiagnostic;
                authoredRoot = null;
                return;
            }
            SubscribeRootSelection();
        }

        private void SubscribeRootSelection()
        {
            if (rootSelectionSubscribed || authoredRoot == null)
            {
                return;
            }
            authoredRoot.TargetSelectionRequested +=
                OnTargetSelectionRequested;
            rootSelectionSubscribed = true;
        }

        private void UnsubscribeRootSelection()
        {
            if (!rootSelectionSubscribed || authoredRoot == null)
            {
                rootSelectionSubscribed = false;
                return;
            }
            authoredRoot.TargetSelectionRequested -=
                OnTargetSelectionRequested;
            rootSelectionSubscribed = false;
        }

        private void OnTargetSelectionRequested(
            string enemyInstanceId)
        {
            bool accepted =
                flowController?.TrySelectCurrentNormalTarget(
                    enemyInstanceId) == true;
            LastDiagnostic = accepted
                ? "TARGET_SELECTION_ACCEPTED_"
                    + enemyInstanceId
                : "TARGET_SELECTION_REJECTED_"
                    + enemyInstanceId;
        }

        private void RenderActorSnapshots(
            V04Chapter1NormalEnemyBattleAdapter battle)
        {
            List<C1EnemyPresentationActorFrame> frames = new();
            currentBindings.Clear();
            HashSet<string> liveInstanceIds =
                new(StringComparer.Ordinal);
            foreach (V04Chapter1NormalEnemyActorSnapshot actor
                     in battle.ActorSnapshots
                         .OrderBy(value => value.Plan.slotOrdinal))
            {
                if (!TryBuildFrame(
                        actor,
                        battle.SelectedTargetEnemyInstanceId,
                        out C1EnemyPresentationActorFrame frame,
                        out string diagnostic))
                {
                    authoredRoot.ClearRuntimePresentation();
                    currentBindings.Clear();
                    LastDiagnostic = diagnostic;
                    return;
                }
                liveInstanceIds.Add(actor.EnemyInstanceId);
                frames.Add(frame);
                currentBindings.Add(
                    new C1EnemyPresentationBindingSnapshot(
                        frame.EnemyInstanceId,
                        frame.DisplaySlotId,
                        frame.VisualProfileKey,
                        frame.VisualState,
                        frame.Visible,
                        frame.Selected));
            }

            foreach (string staleId in playbacks.Keys
                         .Where(value => !liveInstanceIds.Contains(value))
                         .ToArray())
            {
                playbacks.Remove(staleId);
            }
            if (!authoredRoot.TryApply(
                    frames,
                    out string rootDiagnostic))
            {
                LastDiagnostic = "ROUTER_REJECTED_"
                    + rootDiagnostic;
                return;
            }
            LastDiagnostic = "ACTOR_SNAPSHOTS_CONSUMED_"
                + string.Join(
                    "_",
                    currentBindings
                        .OrderBy(value => value.DisplaySlotId,
                            StringComparer.Ordinal)
                        .Select(value => value.BindingKey));
        }

        private bool TryBuildFrame(
            V04Chapter1NormalEnemyActorSnapshot actor,
            string selectedEnemyInstanceId,
            out C1EnemyPresentationActorFrame frame,
            out string diagnostic)
        {
            frame = null;
            diagnostic = string.Empty;
            if (actor?.Plan == null
                || actor.Runtime == null
                || string.IsNullOrWhiteSpace(actor.EnemyInstanceId)
                || string.IsNullOrWhiteSpace(
                    actor.Plan.displaySlotId))
            {
                diagnostic = "ACTOR_SNAPSHOT_IDENTITY_INCOMPLETE";
                return false;
            }
            if (!string.Equals(
                    actor.Runtime.PresentationKey,
                    actor.Plan.visualProfileKey,
                    StringComparison.Ordinal))
            {
                diagnostic =
                    "ACTOR_VISUAL_PROFILE_IDENTITY_MISMATCH_"
                    + actor.EnemyInstanceId;
                return false;
            }
            if (!C1EnemyVisualProfileCatalog.TryGet(
                    actor.Plan.visualProfileKey,
                    out C1EnemyVisualProfileDefinition profile))
            {
                diagnostic = "VISUAL_PROFILE_MISSING_"
                    + actor.Plan.visualProfileKey;
                return false;
            }
            if (!profile.RuntimeRenderable)
            {
                diagnostic = profile.BlockerCode + "_NO_ACTOR_NO_"
                    + C1EnemyVisualProfileCatalog
                        .TemporaryRemnantFallbackTag;
                return false;
            }

            ActorPlayback playback = GetPlayback(actor);
            ObserveLatestCue(playback, actor.Runtime);
            if (!profile.TryGetClip(
                    playback.State,
                    out C1EnemyVisualClipDefinition clipDefinition)
                || !TryLoadClip(
                    profile,
                    clipDefinition,
                    out LoadedClip loadedClip))
            {
                diagnostic = "VISUAL_CLIP_MISSING_"
                    + actor.Plan.visualProfileKey + "_"
                    + playback.State;
                return false;
            }

            float elapsed = Mathf.Max(
                0f,
                Time.unscaledTime - playback.StateStartedAt);
            if (!loadedClip.Definition.Loops
                && playback.State
                    != C1EnemyPresentationVisualState.Death
                && elapsed >= loadedClip.DurationSeconds)
            {
                playback.State =
                    C1EnemyPresentationVisualState.Idle;
                playback.StateStartedAt = Time.unscaledTime;
                profile.TryGetClip(
                    playback.State,
                    out clipDefinition);
                if (!TryLoadClip(
                        profile,
                        clipDefinition,
                        out loadedClip))
                {
                    diagnostic = "IDLE_VISUAL_CLIP_MISSING_"
                        + actor.Plan.visualProfileKey;
                    return false;
                }
                elapsed = 0f;
            }

            bool defeated =
                actor.Runtime.Lifecycle == C1EnemyLifecycle.Defeated;
            bool deathPresentationComplete =
                defeated
                && playback.State
                    == C1EnemyPresentationVisualState.Death
                && elapsed >= loadedClip.DurationSeconds;
            bool visible = !deathPresentationComplete;
            Sprite sprite = visible
                ? loadedClip.FrameAt(elapsed)
                : null;
            frame = new C1EnemyPresentationActorFrame(
                actor.EnemyInstanceId,
                actor.Plan.displaySlotId,
                actor.Plan.visualProfileKey,
                playback.State,
                sprite,
                loadedClip.Definition.Calibration,
                visible,
                actor.IsLiveTargetable,
                string.Equals(
                    selectedEnemyInstanceId,
                    actor.EnemyInstanceId,
                    StringComparison.Ordinal));
            return true;
        }

        private ActorPlayback GetPlayback(
            V04Chapter1NormalEnemyActorSnapshot actor)
        {
            if (!playbacks.TryGetValue(
                    actor.EnemyInstanceId,
                    out ActorPlayback playback))
            {
                playback = new ActorPlayback();
                playbacks.Add(actor.EnemyInstanceId, playback);
            }
            if (playback.ResetGeneration
                    != actor.Runtime.ResetGeneration
                || !string.Equals(
                    playback.VisualProfileKey,
                    actor.Plan.visualProfileKey,
                    StringComparison.Ordinal))
            {
                playback.VisualProfileKey =
                    actor.Plan.visualProfileKey;
                playback.ResetGeneration =
                    actor.Runtime.ResetGeneration;
                playback.LastCueSequence = 0L;
                playback.State =
                    C1EnemyPresentationVisualState.Idle;
                playback.StateStartedAt = Time.unscaledTime;
            }
            return playback;
        }

        private static void ObserveLatestCue(
            ActorPlayback playback,
            C1EnemyRuntimeSnapshot runtime)
        {
            if (playback.State
                == C1EnemyPresentationVisualState.Death)
            {
                return;
            }

            C1EnemyCueSnapshot cue = runtime.LatestCue;
            if (cue == null
                || cue.ResetGeneration
                    != runtime.ResetGeneration
                || cue.CueSequence <= playback.LastCueSequence)
            {
                if (runtime.Lifecycle == C1EnemyLifecycle.Defeated
                    && playback.State
                        != C1EnemyPresentationVisualState.Death)
                {
                    playback.State =
                        C1EnemyPresentationVisualState.Death;
                    playback.StateStartedAt = Time.unscaledTime;
                }
                return;
            }

            C1EnemyPresentationVisualState nextState = cue.Kind switch
            {
                C1EnemyCueKind.BasicAttack =>
                    C1EnemyPresentationVisualState.BasicAttack,
                C1EnemyCueKind.ChargeAttack =>
                    C1EnemyPresentationVisualState.BasicAttack,
                C1EnemyCueKind.Skill =>
                    C1EnemyPresentationVisualState.Skill,
                C1EnemyCueKind.Hit =>
                    C1EnemyPresentationVisualState.Hit,
                C1EnemyCueKind.ShellHit =>
                    C1EnemyPresentationVisualState.Hit,
                C1EnemyCueKind.ShellBreak =>
                    C1EnemyPresentationVisualState.Hit,
                C1EnemyCueKind.Defeated =>
                    C1EnemyPresentationVisualState.Death,
                _ => C1EnemyPresentationVisualState.Idle
            };

            if (nextState == C1EnemyPresentationVisualState.Hit
                && playback.State == C1EnemyPresentationVisualState.Hit
                && Time.unscaledTime - playback.StateStartedAt
                    < HitRestartSuppressionSeconds)
            {
                playback.LastCueSequence = cue.CueSequence;
                return;
            }

            playback.LastCueSequence = cue.CueSequence;
            playback.State = nextState;
            playback.StateStartedAt = Time.unscaledTime;
        }

        private bool TryLoadClip(
            C1EnemyVisualProfileDefinition profile,
            C1EnemyVisualClipDefinition definition,
            out LoadedClip loaded)
        {
            string key = profile.VisualProfileKey + "|"
                + definition.State;
            if (loadedClips.TryGetValue(key, out loaded))
            {
                return loaded.Frames.Length > 0;
            }

            Sprite[] sprites = definition.FrameSequence
                ? LoadSequenceSprites(definition)
                : LoadSingleSprite(definition);
            loaded = new LoadedClip(definition, sprites);
            loadedClips.Add(key, loaded);
            return sprites.Length > 0;
        }

        private Sprite[] LoadSequenceSprites(
            C1EnemyVisualClipDefinition definition)
        {
            Texture2D[] textures = Resources.LoadAll<Texture2D>(
                    definition.ResourcePath)
                .Where(value => value != null)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            if (definition.SelectedFrameOrdinals != null
                && definition.SelectedFrameOrdinals.Count > 0)
            {
                Dictionary<int, Texture2D> byOrdinal =
                    textures.Select((value, index) =>
                            new KeyValuePair<int, Texture2D>(
                                index + 1,
                                value))
                        .ToDictionary(
                            value => value.Key,
                            value => value.Value);
                return definition.SelectedFrameOrdinals
                    .Select(value =>
                        byOrdinal.TryGetValue(
                            value,
                            out Texture2D texture)
                            ? CreateRuntimeSprite(texture, definition)
                            : null)
                    .Where(value => value != null)
                    .ToArray();
            }
            return textures.Select(value =>
                    CreateRuntimeSprite(value, definition))
                .Where(value => value != null)
                .ToArray();
        }

        private Sprite[] LoadSingleSprite(
            C1EnemyVisualClipDefinition definition)
        {
            Sprite imported = Resources.Load<Sprite>(
                definition.ResourcePath);
            if (imported != null)
            {
                return new[] { imported };
            }
            Texture2D texture = Resources.Load<Texture2D>(
                definition.ResourcePath);
            Sprite runtime = CreateRuntimeSprite(
                texture,
                definition);
            return runtime != null
                ? new[] { runtime }
                : Array.Empty<Sprite>();
        }

        private Sprite CreateRuntimeSprite(
            Texture2D texture,
            C1EnemyVisualClipDefinition definition)
        {
            if (texture == null)
            {
                return null;
            }
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                definition.Calibration.NormalizedPivot,
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = definition.ResourcePath.Replace('/', '_')
                + "_" + texture.name + "_C1Presentation";
            sprite.hideFlags = HideFlags.DontSave;
            ownedRuntimeSprites.Add(sprite);
            return sprite;
        }
    }
}
