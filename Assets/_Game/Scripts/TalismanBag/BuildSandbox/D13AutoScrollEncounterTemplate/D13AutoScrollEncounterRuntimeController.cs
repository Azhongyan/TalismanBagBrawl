using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox.D13AutoScrollEncounterTemplate
{
    internal sealed class D13AutoScrollEncounterRuntimeController :
        MonoBehaviour
    {
        private sealed class AuthoredVisibilitySnapshot
        {
            public GameObject Target;
            public bool ActiveSelf;
        }

        private readonly List<GameObject> ownedTransientVfx = new();
        private readonly List<AuthoredVisibilitySnapshot>
            isolatedAuthoredVisuals = new();

        private D13AutoScrollEncounterPresentationProfile profile;
        private RectTransform runtimeRoot;
        private RectTransform authoredStage;
        private Image authoredPlayerImage;
        private bool authoredPlayerEnabled;
        private Color authoredPlayerColor;
        private RectTransform playerRoot;
        private RectTransform enemyRoot;
        private D13AutoScrollActorView playerActor;
        private D13AutoScrollActorView enemyActor;
        private D13AutoScrollHitOverlay playerHitOverlay;
        private D13AutoScrollHitOverlay enemyHitOverlay;
        private Coroutine loopRoutine;
        private Vector2 playerEntryAnchor;
        private Vector2 playerBattleAnchor;
        private Vector2 playerExitAnchor;
        private Vector2 enemyEntryAnchor;
        private Vector2 enemyBattleAnchor;
        private float expectedEnemyFootLine;
        private int baselineOwnedObjectCount;
        private bool initialized;
        private bool anchorsCached;

        public int CompletedLoopCount { get; private set; }

        public int LifecycleFaultCount { get; private set; }

        public float LastAnchorDrift { get; private set; }

        public float LastFootLineDrift { get; private set; }

        public int LiveOwnedObjectCount =>
            runtimeRoot == null
                ? 0
                : runtimeRoot.GetComponentsInChildren<Transform>(true).Length;

        public static bool TryCreateForScene(
            Scene scene,
            out string failure)
        {
            failure = string.Empty;
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.name,
                    D13AutoScrollEncounterPresentationProfile.TargetSceneName,
                    StringComparison.Ordinal))
            {
                failure = "target BattleSandbox Scene is not loaded";
                return false;
            }

            List<Image> candidates = new();
            Image[] images = Resources.FindObjectsOfTypeAll<Image>();
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null
                    || image.gameObject.scene != scene
                    || !image.gameObject.activeInHierarchy
                    || !string.Equals(
                        image.gameObject.name,
                        D13AutoScrollEncounterPresentationProfile
                            .AuthoredPlayerImageName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                candidates.Add(image);
            }

            if (candidates.Count != 1)
            {
                failure = "expected exactly one active authored Image named '"
                    + D13AutoScrollEncounterPresentationProfile
                        .AuthoredPlayerImageName
                    + "', found "
                    + candidates.Count;
                return false;
            }

            Image sourceImage = candidates[0];
            RectTransform sourceRect = sourceImage.rectTransform;
            RectTransform stage = sourceRect.parent as RectTransform;
            if (stage == null
                || !string.Equals(
                    stage.gameObject.name,
                    D13AutoScrollEncounterPresentationProfile.AuthoredStageName,
                    StringComparison.Ordinal))
            {
                failure = "authored player is not an immediate child of '"
                    + D13AutoScrollEncounterPresentationProfile
                        .AuthoredStageName
                    + "'";
                return false;
            }

            Canvas canvas = stage.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                failure = "authored BattleSandbox player has no Canvas";
                return false;
            }

            GameObject rootObject = new(
                "D13AutoScrollEncounterRuntime",
                typeof(RectTransform));
            rootObject.hideFlags = HideFlags.DontSave;
            RectTransform root = rootObject.GetComponent<RectTransform>();
            root.SetParent(stage, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = Vector2.zero;
            root.localScale = Vector3.one;

            D13AutoScrollEncounterRuntimeController controller =
                rootObject.AddComponent<
                    D13AutoScrollEncounterRuntimeController>();
            if (!controller.Initialize(root, stage, sourceImage, canvas))
            {
                failure = "runtime proxy initialization failed";
                Destroy(rootObject);
                return false;
            }

            return true;
        }

        private bool Initialize(
            RectTransform root,
            RectTransform stage,
            Image sourceImage,
            Canvas canvas)
        {
            runtimeRoot = root;
            authoredStage = stage;
            authoredPlayerImage = sourceImage;
            profile = D13AutoScrollEncounterPresentationProfile.Default;
            if (runtimeRoot == null
                || authoredStage == null
                || authoredPlayerImage == null
                || authoredPlayerImage.sprite == null
                || canvas == null)
            {
                return false;
            }

            authoredPlayerEnabled = authoredPlayerImage.enabled;
            authoredPlayerColor = authoredPlayerImage.color;
            if (!CreateActors())
            {
                RestoreAuthoredPlayerDisplay();
                return false;
            }

            CaptureAndHideAuthoredActorVisuals();
            initialized = true;
            authoredPlayerImage.enabled = false;
            Canvas.ForceUpdateCanvases();
            CacheAbsoluteAnchors();
            ResetRoundState();
            baselineOwnedObjectCount = LiveOwnedObjectCount;
            loopRoutine = StartCoroutine(RunEncounterLoop());
            Debug.Log(
                "[D13AutoScrollEncounter] Installed presentation-only "
                + "auto-scroll encounter in "
                + gameObject.scene.name
                + "; no Battle/Reward/Save owner was changed.");
            return true;
        }

        private bool CreateActors()
        {
            RectTransform sourceRect = authoredPlayerImage.rectTransform;
            CreateActorHierarchy(
                "PlayerProxy",
                out playerRoot,
                out RectTransform playerFeedback,
                out RectTransform playerRunCarrier,
                out RectTransform playerVisualRect,
                out RectTransform playerFloatRoot,
                out Image playerImage);
            playerRoot.localScale = sourceRect.localScale;
            playerActor =
                playerRoot.gameObject.AddComponent<D13AutoScrollActorView>();
            playerActor.ConfigureStatic(
                playerRunCarrier,
                playerVisualRect,
                playerImage,
                authoredPlayerImage.sprite,
                sourceRect.sizeDelta,
                sourceRect.pivot,
                authoredPlayerImage.color);
            playerHitOverlay =
                playerRoot.gameObject.AddComponent<D13AutoScrollHitOverlay>();
            playerHitOverlay.Configure(
                playerFeedback,
                playerFloatRoot,
                playerImage,
                profile);

            CreateActorHierarchy(
                "EnemyD13",
                out enemyRoot,
                out RectTransform enemyFeedback,
                out RectTransform enemyRunCarrier,
                out RectTransform enemyVisualRect,
                out RectTransform enemyFloatRoot,
                out Image enemyImage);
            enemyRoot.localScale = Vector3.one;
            enemyActor =
                enemyRoot.gameObject.AddComponent<D13AutoScrollActorView>();
            enemyActor.ConfigureD13(
                enemyRunCarrier,
                enemyVisualRect,
                enemyImage,
                profile);
            enemyHitOverlay =
                enemyRoot.gameObject.AddComponent<D13AutoScrollHitOverlay>();
            enemyHitOverlay.Configure(
                enemyFeedback,
                enemyFloatRoot,
                enemyImage,
                profile);

            return playerActor != null
                && enemyActor != null
                && playerHitOverlay != null
                && enemyHitOverlay != null;
        }

        private void CaptureAndHideAuthoredActorVisuals()
        {
            isolatedAuthoredVisuals.Clear();
            CaptureAuthoredVisual(
                authoredStage.Find("V02PlayerAvatar")?.gameObject);
            CaptureAuthoredVisual(
                authoredStage.Find(
                    "CharacterIdlePreview_IdleAnim")?.gameObject);

            Transform safeArea = authoredStage.parent;
            CaptureAuthoredVisual(
                safeArea == null
                    ? null
                    : safeArea.Find("V02EnemyArea")?.gameObject);
            ApplyAuthoredActorIsolation();
        }

        private void CaptureAuthoredVisual(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            isolatedAuthoredVisuals.Add(new AuthoredVisibilitySnapshot
            {
                Target = target,
                ActiveSelf = target.activeSelf
            });
        }

        private void ApplyAuthoredActorIsolation()
        {
            for (int i = 0; i < isolatedAuthoredVisuals.Count; i++)
            {
                AuthoredVisibilitySnapshot snapshot =
                    isolatedAuthoredVisuals[i];
                if (snapshot.Target != null)
                {
                    snapshot.Target.SetActive(false);
                }
            }
        }

        private void RestoreAuthoredActorVisuals()
        {
            for (int i = 0; i < isolatedAuthoredVisuals.Count; i++)
            {
                AuthoredVisibilitySnapshot snapshot =
                    isolatedAuthoredVisuals[i];
                if (snapshot.Target != null)
                {
                    snapshot.Target.SetActive(snapshot.ActiveSelf);
                }
            }
        }

        private void CreateActorHierarchy(
            string actorName,
            out RectTransform actorRoot,
            out RectTransform feedbackCarrier,
            out RectTransform runCarrier,
            out RectTransform visualRect,
            out RectTransform floatRoot,
            out Image visualImage)
        {
            actorRoot = CreateRect(actorName, runtimeRoot);
            actorRoot.anchorMin = new Vector2(0.5f, 0.5f);
            actorRoot.anchorMax = new Vector2(0.5f, 0.5f);
            actorRoot.pivot = new Vector2(0.5f, 0.5f);
            actorRoot.sizeDelta = Vector2.zero;

            feedbackCarrier = CreateRect(
                actorName + "_HitCarrier",
                actorRoot);
            runCarrier = CreateRect(
                actorName + "_RunSurrogateCarrier",
                feedbackCarrier);
            visualRect = CreateRect(actorName + "_Visual", runCarrier);
            visualImage = visualRect.gameObject.AddComponent<Image>();
            visualImage.raycastTarget = false;
            visualImage.preserveAspect = true;

            floatRoot = CreateRect(actorName + "_FloatRoot", actorRoot);
            floatRoot.anchorMin = new Vector2(0.5f, 0.5f);
            floatRoot.anchorMax = new Vector2(0.5f, 0.5f);
            floatRoot.pivot = new Vector2(0.5f, 0.5f);
            floatRoot.sizeDelta = new Vector2(240f, 260f);
        }

        private static RectTransform CreateRect(
            string objectName,
            RectTransform parent)
        {
            GameObject child = new(objectName, typeof(RectTransform));
            child.hideFlags = HideFlags.DontSave;
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        private void CacheAbsoluteAnchors()
        {
            RectTransform sourceRect = authoredPlayerImage.rectTransform;
            float halfWidth = Mathf.Max(240f, authoredStage.rect.width * 0.5f);
            float sourceY = sourceRect.anchoredPosition.y;
            float sourceScaleX = Mathf.Max(
                0.01f,
                Mathf.Abs(sourceRect.localScale.x));
            float sourceVisualHalfWidth =
                sourceRect.sizeDelta.x * sourceScaleX * 0.5f;

            playerBattleAnchor = new Vector2(
                halfWidth * profile.PlayerAnchorFactor,
                sourceY);
            enemyBattleAnchor = new Vector2(
                halfWidth * profile.EnemyAnchorFactor,
                sourceY - 8f);
            playerEntryAnchor = new Vector2(
                -halfWidth - sourceVisualHalfWidth - profile.OffscreenMargin,
                sourceY);
            playerExitAnchor = new Vector2(
                halfWidth + sourceVisualHalfWidth + profile.OffscreenMargin,
                sourceY);
            enemyEntryAnchor = new Vector2(
                halfWidth + 155f + profile.OffscreenMargin,
                enemyBattleAnchor.y);
            expectedEnemyFootLine = enemyActor.CurrentFootLineOffset;
            anchorsCached = true;
        }

        private IEnumerator RunEncounterLoop()
        {
            yield return null;
            while (enabled && initialized && anchorsCached)
            {
                int loopOrdinal = CompletedLoopCount + 1;
                BeginRound();

                yield return MoveApproach();
                playerActor.SetRunSurrogate(false);
                enemyActor.SetRunSurrogate(false);
                yield return WaitSeconds(profile.ReadySeconds);

                enemyActor.PlayOneShot(
                    D13AutoScrollAnimationSlot.Attack,
                    D13AutoScrollAnimationSlot.Idle);
                yield return WaitForEnemyTrackWithPlayerHits(
                    loopOrdinal,
                    "attack",
                    new[] { 0.72f, 1.72f },
                    new[] { 118, 146 });
                yield return WaitSeconds(profile.InterActionHoldSeconds);

                yield return PlayPlayerStrike(
                    loopOrdinal,
                    "enemy-normal-01",
                    173);
                yield return WaitSeconds(profile.InterActionHoldSeconds);

                enemyActor.PlayOneShot(
                    D13AutoScrollAnimationSlot.Skill,
                    D13AutoScrollAnimationSlot.Idle);
                yield return WaitForEnemyTrackWithPlayerHits(
                    loopOrdinal,
                    "skill",
                    new[] { 1.12f },
                    new[] { 224 });
                yield return WaitSeconds(profile.InterActionHoldSeconds);

                yield return PlayPlayerStrike(
                    loopOrdinal,
                    "enemy-normal-02",
                    191);
                yield return WaitSeconds(profile.InterActionHoldSeconds);

                enemyHitOverlay.ClearTransientFeedback();
                enemyActor.PlayHeavyHit();
                yield return WaitForEnemyTrack();
                yield return WaitSeconds(profile.InterActionHoldSeconds);

                playerHitOverlay.ClearTransientFeedback();
                enemyHitOverlay.ClearTransientFeedback();
                enemyActor.InterruptHeavyHitAndRestore();
                enemyActor.LockDeath();
                yield return WaitForEnemyTrack();

                yield return PlayPresentationOnlyLootBeat();
                enemyRoot.gameObject.SetActive(false);
                yield return MovePlayerToExit();
                yield return ResetAfterCompletedRound(loopOrdinal);
                yield return WaitSeconds(profile.ReplayDelaySeconds);
            }
        }

        private void BeginRound()
        {
            ResetRoundState();
            playerRoot.gameObject.SetActive(true);
            enemyRoot.gameObject.SetActive(true);
            playerActor.SetRunSurrogate(true, 0f);
            enemyActor.SetRunSurrogate(true, 1.4f);
        }

        private IEnumerator MoveApproach()
        {
            float elapsed = 0f;
            float dustTimer = 0f;
            while (elapsed < profile.ApproachSeconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                dustTimer += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(
                    elapsed / profile.ApproachSeconds);
                float eased = Mathf.SmoothStep(0f, 1f, normalized);
                playerRoot.anchoredPosition = Vector2.LerpUnclamped(
                    playerEntryAnchor,
                    playerBattleAnchor,
                    eased);
                enemyRoot.anchoredPosition = Vector2.LerpUnclamped(
                    enemyEntryAnchor,
                    enemyBattleAnchor,
                    eased);

                if (dustTimer >= profile.DustStepInterval)
                {
                    dustTimer = 0f;
                    SpawnDustStep(playerRoot.anchoredPosition, false);
                    SpawnDustStep(enemyRoot.anchoredPosition, true);
                }

                yield return null;
            }

            playerRoot.anchoredPosition = playerBattleAnchor;
            enemyRoot.anchoredPosition = enemyBattleAnchor;
        }

        private IEnumerator WaitForEnemyTrackWithPlayerHits(
            int loopOrdinal,
            string cueFamily,
            float[] cueTimes,
            int[] displayAmounts)
        {
            int nextCue = 0;
            float elapsed = 0f;
            float timeout = 4f;
            while (enemyActor.IsOneShotPlaying && elapsed < timeout)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                while (nextCue < cueTimes.Length
                    && elapsed >= cueTimes[nextCue])
                {
                    string cueId = "loop-"
                        + loopOrdinal
                        + "-player-"
                        + cueFamily
                        + "-"
                        + (nextCue + 1);
                    int amount = nextCue < displayAmounts.Length
                        ? displayAmounts[nextCue]
                        : displayAmounts[displayAmounts.Length - 1];
                    playerHitOverlay.PresentNormalHit(
                        cueId,
                        amount,
                        -1f);
                    nextCue++;
                }

                yield return null;
            }

            if (elapsed >= timeout && enemyActor.IsOneShotPlaying)
            {
                LifecycleFaultCount++;
                Debug.LogWarning(
                    "[D13AutoScrollEncounter] Base track timeout during "
                    + cueFamily
                    + ".");
            }
        }

        private IEnumerator PlayPlayerStrike(
            int loopOrdinal,
            string cueSuffix,
            int displayAmount)
        {
            Vector2 start = playerBattleAnchor;
            Vector2 target = start + new Vector2(35f, 1f);
            float elapsed = 0f;
            bool cuePresented = false;
            while (elapsed < profile.PlayerStrikeSeconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(
                    elapsed / profile.PlayerStrikeSeconds);
                float arc = Mathf.Sin(normalized * Mathf.PI);
                playerRoot.anchoredPosition = Vector2.LerpUnclamped(
                    start,
                    target,
                    arc);
                if (!cuePresented && normalized >= 0.46f)
                {
                    cuePresented = true;
                    enemyHitOverlay.PresentNormalHit(
                        "loop-" + loopOrdinal + "-" + cueSuffix,
                        displayAmount,
                        1f);
                }

                yield return null;
            }

            playerRoot.anchoredPosition = playerBattleAnchor;
        }

        private IEnumerator WaitForEnemyTrack()
        {
            float elapsed = 0f;
            float timeout = 4f;
            while (enemyActor.IsOneShotPlaying && elapsed < timeout)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                yield return null;
            }

            if (elapsed >= timeout && enemyActor.IsOneShotPlaying)
            {
                LifecycleFaultCount++;
                Debug.LogWarning(
                    "[D13AutoScrollEncounter] Base track timeout.");
            }
        }

        private IEnumerator PlayPresentationOnlyLootBeat()
        {
            GameObject lootRootObject = new(
                "D13PresentationOnlyLootBeat",
                typeof(RectTransform),
                typeof(CanvasGroup));
            lootRootObject.hideFlags = HideFlags.DontSave;
            RectTransform lootRoot =
                lootRootObject.GetComponent<RectTransform>();
            lootRoot.SetParent(runtimeRoot, false);
            lootRoot.anchorMin = new Vector2(0.5f, 0.5f);
            lootRoot.anchorMax = new Vector2(0.5f, 0.5f);
            lootRoot.pivot = new Vector2(0.5f, 0f);
            lootRoot.anchoredPosition =
                enemyBattleAnchor + new Vector2(0f, 24f);
            lootRoot.sizeDelta = new Vector2(160f, 110f);
            ownedTransientVfx.Add(lootRootObject);

            Color[] colors =
            {
                new(1f, 0.77f, 0.20f, 1f),
                new(0.98f, 0.45f, 0.14f, 1f),
                new(1f, 0.90f, 0.45f, 1f)
            };
            for (int i = 0; i < colors.Length; i++)
            {
                RectTransform shard = CreateRect(
                    "LootGlint_" + (i + 1),
                    lootRoot);
                shard.anchorMin = new Vector2(0.5f, 0f);
                shard.anchorMax = new Vector2(0.5f, 0f);
                shard.pivot = new Vector2(0.5f, 0.5f);
                shard.anchoredPosition = new Vector2((i - 1) * 32f, 8f);
                shard.sizeDelta = new Vector2(18f, 18f);
                shard.localRotation = Quaternion.Euler(0f, 0f, 45f);
                Image image = shard.gameObject.AddComponent<Image>();
                image.color = colors[i];
                image.raycastTarget = false;
            }

            CanvasGroup group = lootRootObject.GetComponent<CanvasGroup>();
            float elapsed = 0f;
            while (elapsed < profile.LootBeatSeconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(
                    elapsed / profile.LootBeatSeconds);
                float rise = Mathf.Sin(normalized * Mathf.PI * 0.5f);
                lootRoot.anchoredPosition =
                    enemyBattleAnchor + new Vector2(0f, 24f + (58f * rise));
                lootRoot.localScale = Vector3.one
                    * (0.72f + (0.38f * Mathf.Sin(normalized * Mathf.PI)));
                group.alpha = 1f
                    - Mathf.Clamp01((normalized - 0.58f) / 0.42f);
                yield return null;
            }

            ownedTransientVfx.Remove(lootRootObject);
            Destroy(lootRootObject);
        }

        private IEnumerator MovePlayerToExit()
        {
            playerActor.SetRunSurrogate(true, 0.8f);
            Vector2 start = playerRoot.anchoredPosition;
            float elapsed = 0f;
            float dustTimer = 0f;
            while (elapsed < profile.ExitSeconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                dustTimer += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(
                    elapsed / profile.ExitSeconds);
                playerRoot.anchoredPosition = Vector2.LerpUnclamped(
                    start,
                    playerExitAnchor,
                    Mathf.SmoothStep(0f, 1f, normalized));
                if (dustTimer >= profile.DustStepInterval)
                {
                    dustTimer = 0f;
                    SpawnDustStep(playerRoot.anchoredPosition, false);
                }

                yield return null;
            }

            playerRoot.anchoredPosition = playerExitAnchor;
            playerActor.SetRunSurrogate(false);
        }

        private IEnumerator ResetAfterCompletedRound(int loopOrdinal)
        {
            ResetRoundState();
            yield return null;

            LastAnchorDrift = Mathf.Max(
                Vector2.Distance(
                    playerRoot.anchoredPosition,
                    playerEntryAnchor),
                Vector2.Distance(
                    enemyRoot.anchoredPosition,
                    enemyEntryAnchor));
            LastFootLineDrift = Mathf.Abs(
                enemyActor.CurrentFootLineOffset - expectedEnemyFootLine);
            int liveObjects = LiveOwnedObjectCount;
            bool stable = LastAnchorDrift <= 0.01f
                && LastFootLineDrift <= 0.01f
                && liveObjects == baselineOwnedObjectCount
                && playerHitOverlay.LiveFloaterCount == 0
                && enemyHitOverlay.LiveFloaterCount == 0
                && ownedTransientVfx.Count == 0
                && !enemyActor.IsDeathLocked
                && enemyActor.CurrentSlot
                    == D13AutoScrollAnimationSlot.Idle;

            if (!stable)
            {
                LifecycleFaultCount++;
                Debug.LogWarning(
                    "[D13AutoScrollEncounter] Loop "
                    + loopOrdinal
                    + " reset drift/fault: anchor="
                    + LastAnchorDrift.ToString("F3")
                    + ", foot="
                    + LastFootLineDrift.ToString("F3")
                    + ", owned="
                    + liveObjects
                    + "/"
                    + baselineOwnedObjectCount
                    + ".");
            }

            CompletedLoopCount++;
            Debug.Log(
                "[D13AutoScrollEncounter] LOOP_COMPLETE cycle="
                + CompletedLoopCount
                + " anchorDrift="
                + LastAnchorDrift.ToString("F3")
                + " footDrift="
                + LastFootLineDrift.ToString("F3")
                + " owned="
                + liveObjects
                + " faults="
                + LifecycleFaultCount
                + ".");
        }

        private void SpawnDustStep(Vector2 actorPosition, bool mirror)
        {
            GameObject dustObject = new(
                "D13RunSurrogateDust",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            dustObject.hideFlags = HideFlags.DontSave;
            RectTransform dust = dustObject.GetComponent<RectTransform>();
            dust.SetParent(runtimeRoot, false);
            dust.anchorMin = new Vector2(0.5f, 0.5f);
            dust.anchorMax = new Vector2(0.5f, 0.5f);
            dust.pivot = new Vector2(0.5f, 0.5f);
            dust.anchoredPosition = actorPosition
                + new Vector2(mirror ? 18f : -18f, -54f);
            dust.sizeDelta = new Vector2(28f, 10f);
            dust.localRotation = Quaternion.Euler(
                0f,
                0f,
                mirror ? -8f : 8f);
            Image image = dustObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = new Color(0.74f, 0.64f, 0.48f, 0.45f);
            ownedTransientVfx.Add(dustObject);
            StartCoroutine(AnimateDust(dustObject, dust, image));
        }

        private IEnumerator AnimateDust(
            GameObject dustObject,
            RectTransform dust,
            Image image)
        {
            Vector2 start = dust.anchoredPosition;
            float elapsed = 0f;
            const float duration = 0.42f;
            while (dustObject != null && elapsed < duration)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                float normalized = Mathf.Clamp01(elapsed / duration);
                dust.anchoredPosition =
                    start + new Vector2(0f, 10f * normalized);
                dust.localScale = new Vector3(
                    1f + normalized,
                    1f - (0.35f * normalized),
                    1f);
                Color color = image.color;
                color.a = 0.45f * (1f - normalized);
                image.color = color;
                yield return null;
            }

            if (dustObject != null)
            {
                ownedTransientVfx.Remove(dustObject);
                Destroy(dustObject);
            }
        }

        private IEnumerator WaitSeconds(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Mathf.Max(0f, Time.deltaTime);
                yield return null;
            }
        }

        private void ResetRoundState()
        {
            if (!initialized && playerActor == null)
            {
                return;
            }

            playerHitOverlay.ResetRound();
            enemyHitOverlay.ResetRound();
            playerActor.ResetRound();
            enemyActor.ResetRound();
            ClearOwnedTransientVfx();
            playerRoot.anchoredPosition = playerEntryAnchor;
            enemyRoot.anchoredPosition = enemyEntryAnchor;
            playerRoot.localRotation = Quaternion.identity;
            enemyRoot.localRotation = Quaternion.identity;
            playerRoot.gameObject.SetActive(true);
            enemyRoot.gameObject.SetActive(true);
        }

        private void ClearOwnedTransientVfx()
        {
            for (int i = 0; i < ownedTransientVfx.Count; i++)
            {
                GameObject item = ownedTransientVfx[i];
                if (item != null)
                {
                    Destroy(item);
                }
            }

            ownedTransientVfx.Clear();
        }

        private void OnEnable()
        {
            if (!initialized || loopRoutine != null)
            {
                return;
            }

            authoredPlayerImage.enabled = false;
            ApplyAuthoredActorIsolation();
            loopRoutine = StartCoroutine(RunEncounterLoop());
        }

        private void OnDisable()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }

            if (playerHitOverlay != null)
            {
                playerHitOverlay.ResetRound();
            }

            if (enemyHitOverlay != null)
            {
                enemyHitOverlay.ResetRound();
            }

            ClearOwnedTransientVfx();
            RestoreAuthoredPlayerDisplay();
            RestoreAuthoredActorVisuals();
        }

        private void OnDestroy()
        {
            RestoreAuthoredPlayerDisplay();
            RestoreAuthoredActorVisuals();
        }

        private void RestoreAuthoredPlayerDisplay()
        {
            if (authoredPlayerImage == null)
            {
                return;
            }

            authoredPlayerImage.enabled = authoredPlayerEnabled;
            authoredPlayerImage.color = authoredPlayerColor;
        }
    }
}
