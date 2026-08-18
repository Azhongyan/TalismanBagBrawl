using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.CoreLoopLab
{
    [DisallowMultipleComponent]
    public sealed class CoreLoopLabPresentationAdapter : MonoBehaviour
    {
        private const float SemanticVisibleSeconds = 0.62f;
        private const float HitResponseSeconds = 0.13f;
        private const float SpriteFrameSeconds = 1f / 12f;
        private const string D13Root = "anim/Enemy/d1/d1_3/";

        private enum EnemyVisualState
        {
            Idle,
            Attack,
            Skill,
            Hit,
            Death
        }

        [SerializeField] private CanvasGroup coarsePresentationLayer;
        [SerializeField] private C1AuthoredEnemySelectionOutlineEffect
            selectedTargetOutline;
        [SerializeField] private RectTransform enemyHitTarget;
        [SerializeField] private CanvasGroup semanticFloatingGroup;
        [SerializeField] private Text semanticFloatingText;

        private readonly Dictionary<EnemyVisualState, Sprite[]> visualFrames =
            new();
        private readonly List<Sprite> ownedVisualSprites = new();
        private CoreLoopLabProfile profile;
        private Image enemyVisualSlot;
        private Sprite authoredEnemySprite;
        private bool authoredEnemyPreserveAspect;
        private bool hasCapturedAuthoredEnemyState;
        private int observedCueSequence;
        private float semanticTimer;
        private float hitTimer;
        private Vector3 enemyBaseScale = Vector3.one;
        private bool hasEnemyBaseScale;
        private EnemyVisualState visualState;
        private int visualFrameIndex;
        private float visualFrameTimer;
        private bool visualBindingReady;

        public bool OwnsBattleTruth => false;
        public bool SuppressesCoarsePresentation =>
            profile == CoreLoopLabProfile.C_TwoBattleSemanticPresentation;

        public void BindAuthoredScene(
            CanvasGroup coarseLayer,
            C1AuthoredEnemySelectionOutlineEffect targetOutline,
            RectTransform hitTarget,
            CanvasGroup semanticGroup,
            Text semanticText)
        {
            coarsePresentationLayer = coarseLayer;
            selectedTargetOutline = targetOutline;
            enemyHitTarget = hitTarget;
            semanticFloatingGroup = semanticGroup;
            semanticFloatingText = semanticText;
        }

        public void ApplyProfile(CoreLoopLabProfile selectedProfile)
        {
            profile = selectedProfile;
            bool semanticProfile = SuppressesCoarsePresentation;
            if (coarsePresentationLayer != null)
            {
                coarsePresentationLayer.alpha = semanticProfile ? 0f : 1f;
                coarsePresentationLayer.interactable = false;
                coarsePresentationLayer.blocksRaycasts = false;
            }

            if (selectedTargetOutline != null)
            {
                selectedTargetOutline.enabled = false;
            }

            SetSemanticVisible(false);
            RestoreHitTarget();
        }

        public void BeginBattle(
            BattleSandboxDevBattleSessionAcceptedStart acceptedStart,
            string enemyVisualProfileKey)
        {
            EndBattle();
            if (acceptedStart?.StartSnapshot == null
                || !string.Equals(
                    acceptedStart.StartSnapshot.EnemyIdentity,
                    CoreLoopLabEncounterProfiles.EnemyIdentityD13,
                    StringComparison.Ordinal)
                || !string.Equals(
                    enemyVisualProfileKey,
                    CoreLoopLabEncounterProfiles.D13VisualProfileKey,
                    StringComparison.Ordinal))
            {
                Debug.LogError(
                    "[CoreLoopLab] D13_VISUAL_BINDING_REJECTED",
                    this);
                return;
            }

            enemyVisualSlot = enemyHitTarget == null
                ? null
                : enemyHitTarget.GetComponent<Image>();
            if (enemyVisualSlot != null && !hasCapturedAuthoredEnemyState)
            {
                authoredEnemySprite = enemyVisualSlot.sprite;
                authoredEnemyPreserveAspect = enemyVisualSlot.preserveAspect;
                hasCapturedAuthoredEnemyState = true;
            }
            if (enemyVisualSlot == null || !LoadD13Frames())
            {
                Debug.LogError(
                    "[CoreLoopLab] AUTHORED_D13_IMAGE_SLOT_OR_FRAMES_MISSING",
                    this);
                return;
            }

            visualBindingReady = true;
            observedCueSequence = 0;
            semanticTimer = 0f;
            hitTimer = 0f;
            CaptureEnemyBaseScale();
            PlayVisualState(EnemyVisualState.Idle);
            if (selectedTargetOutline != null)
            {
                selectedTargetOutline.enabled = SuppressesCoarsePresentation;
            }

            ConsumeAcceptedSessionCues(acceptedStart.StartSnapshot);
        }

        public void Tick(
            float deltaTime,
            BattleSandboxDevBattleSessionSnapshot acceptedSnapshot)
        {
            float safeDelta = Mathf.Max(0f, deltaTime);
            if (visualBindingReady && acceptedSnapshot != null)
            {
                ConsumeAcceptedSessionCues(acceptedSnapshot);
                UpdateSpriteSequence(safeDelta);
            }

            UpdateSemanticFade(safeDelta);
            UpdateHitResponse(safeDelta);
        }

        public void EndBattle()
        {
            observedCueSequence = 0;
            visualBindingReady = false;
            if (enemyVisualSlot != null && hasCapturedAuthoredEnemyState)
            {
                enemyVisualSlot.sprite = authoredEnemySprite;
                enemyVisualSlot.preserveAspect = authoredEnemyPreserveAspect;
            }
            visualFrames.Clear();
            DestroyOwnedVisualSprites();
            if (selectedTargetOutline != null)
            {
                selectedTargetOutline.enabled = false;
            }

            SetSemanticVisible(false);
            RestoreHitTarget();
        }

        private void ConsumeAcceptedSessionCues(
            BattleSandboxDevBattleSessionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            foreach (BattleSandboxDevBattlePresentationCue cue
                     in snapshot.PresentationCues
                         .Where(value => value != null
                             && value.Sequence > observedCueSequence)
                         .OrderBy(value => value.Sequence))
            {
                observedCueSequence = cue.Sequence;
                PresentAcceptedCue(cue);
            }
        }

        private void PresentAcceptedCue(
            BattleSandboxDevBattlePresentationCue cue)
        {
            if (cue == null)
            {
                return;
            }

            switch (cue.Kind)
            {
                case BattleSandboxDevBattleCueKind.BattleStarted:
                    PlayVisualState(EnemyVisualState.Idle);
                    return;
                case BattleSandboxDevBattleCueKind.Build2Activated:
                    if (SuppressesCoarsePresentation)
                    {
                        PresentText("BUILD2 ACTIVE | " + cue.SourceBaseItemId);
                    }
                    return;
                case BattleSandboxDevBattleCueKind.NianGenerated:
                    if (SuppressesCoarsePresentation)
                    {
                        PresentText("I031 | +" + cue.NianDelta + " NP");
                    }
                    return;
                case BattleSandboxDevBattleCueKind.ItemApplicationAccepted:
                    PlayVisualState(EnemyVisualState.Hit);
                    TriggerHitResponse();
                    PresentText(SuppressesCoarsePresentation
                        ? cue.SourceBaseItemId + " | "
                            + cue.NianDelta + " NP | "
                            + cue.EnemyHpDelta + " HP"
                        : cue.EnemyHpDelta + " HP");
                    return;
                case BattleSandboxDevBattleCueKind.ItemApplicationRejected:
                    if (SuppressesCoarsePresentation)
                    {
                        PresentText(cue.SourceBaseItemId + " | NP INSUFFICIENT");
                    }
                    return;
                case BattleSandboxDevBattleCueKind.EnemyBasicAttack:
                case BattleSandboxDevBattleCueKind.EnemySkill:
                    PlayVisualState(cue.Kind ==
                        BattleSandboxDevBattleCueKind.EnemySkill
                            ? EnemyVisualState.Skill
                            : EnemyVisualState.Attack);
                    PresentText((SuppressesCoarsePresentation
                            ? cue.Kind ==
                                BattleSandboxDevBattleCueKind.EnemySkill
                                ? "ENEMY SKILL | "
                                : "ENEMY BASIC | "
                            : string.Empty)
                        + cue.PlayerShieldDelta + " SH | "
                        + cue.PlayerHpDelta + " HP");
                    return;
                case BattleSandboxDevBattleCueKind.BattleVictory:
                    PlayVisualState(EnemyVisualState.Death);
                    return;
                case BattleSandboxDevBattleCueKind.BattleDefeat:
                case BattleSandboxDevBattleCueKind.BattleTimeout:
                    return;
            }
        }

        private void PresentText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            if (semanticFloatingText != null)
            {
                semanticFloatingText.text = text;
            }

            semanticTimer = SemanticVisibleSeconds;
            SetSemanticVisible(true);
        }

        private void TriggerHitResponse()
        {
            hitTimer = HitResponseSeconds;
            if (enemyHitTarget != null)
            {
                CaptureEnemyBaseScale();
                enemyHitTarget.localScale = Vector3.Scale(
                    enemyBaseScale,
                    new Vector3(0.95f, 0.95f, 1f));
            }
        }

        private bool LoadD13Frames()
        {
            visualFrames.Clear();
            DestroyOwnedVisualSprites();
            try
            {
                bool idleLoaded = LoadFrames(
                    EnemyVisualState.Idle,
                    "d1_3_Idle/frames");
                bool attackLoaded = LoadFrames(
                    EnemyVisualState.Attack,
                    "d1_3_Attack/frames");
                bool skillLoaded = LoadFrames(
                    EnemyVisualState.Skill,
                    "d1_3_Skill/frames");
                bool hitLoaded = LoadFrames(
                    EnemyVisualState.Hit,
                    "d1_3_Hit/frames");
                bool deathLoaded = LoadFrames(
                    EnemyVisualState.Death,
                    "d1_3_Death/frames");
                if (idleLoaded
                    && attackLoaded
                    && skillLoaded
                    && hitLoaded
                    && deathLoaded)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // The stable caller diagnostic reports the failed binding.
            }

            visualFrames.Clear();
            DestroyOwnedVisualSprites();
            return false;
        }

        private bool LoadFrames(EnemyVisualState state, string relativePath)
        {
            Texture2D[] textures = (Resources
                .LoadAll<Texture2D>(D13Root + relativePath)
                    ?? Array.Empty<Texture2D>())
                .Where(value => value != null)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            List<Sprite> frames = new(textures.Length);
            for (int index = 0; index < textures.Length; index++)
            {
                Texture2D texture = textures[index];
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                if (sprite == null)
                {
                    return false;
                }

                sprite.name = "CoreLoopLabOwned_" + state + "_"
                    + texture.name;
                sprite.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                ownedVisualSprites.Add(sprite);
                frames.Add(sprite);
            }

            Sprite[] loadedFrames = frames.ToArray();
            visualFrames[state] = loadedFrames;
            return loadedFrames.Length > 0;
        }

        private void PlayVisualState(EnemyVisualState state)
        {
            if (!visualBindingReady
                || !visualFrames.TryGetValue(state, out Sprite[] frames)
                || frames.Length == 0)
            {
                return;
            }

            visualState = state;
            visualFrameIndex = 0;
            visualFrameTimer = 0f;
            enemyVisualSlot.sprite = frames[0];
            enemyVisualSlot.preserveAspect = true;
        }

        private void UpdateSpriteSequence(float deltaTime)
        {
            if (!visualFrames.TryGetValue(visualState, out Sprite[] frames)
                || frames.Length == 0 || enemyVisualSlot == null)
            {
                return;
            }

            visualFrameTimer += deltaTime;
            while (visualFrameTimer >= SpriteFrameSeconds)
            {
                visualFrameTimer -= SpriteFrameSeconds;
                visualFrameIndex++;
                if (visualFrameIndex >= frames.Length)
                {
                    if (visualState == EnemyVisualState.Idle)
                    {
                        visualFrameIndex = 0;
                    }
                    else if (visualState == EnemyVisualState.Death)
                    {
                        visualFrameIndex = frames.Length - 1;
                    }
                    else
                    {
                        PlayVisualState(EnemyVisualState.Idle);
                        return;
                    }
                }

                enemyVisualSlot.sprite = frames[visualFrameIndex];
            }
        }

        private void UpdateSemanticFade(float deltaTime)
        {
            if (semanticFloatingGroup == null || semanticTimer <= 0f)
            {
                return;
            }

            semanticTimer = Mathf.Max(0f, semanticTimer - deltaTime);
            semanticFloatingGroup.alpha = Mathf.Clamp01(
                semanticTimer
                / Mathf.Max(0.01f, SemanticVisibleSeconds * 0.38f));
            if (semanticTimer <= 0f)
            {
                SetSemanticVisible(false);
            }
        }

        private void UpdateHitResponse(float deltaTime)
        {
            if (enemyHitTarget == null || hitTimer <= 0f)
            {
                return;
            }

            hitTimer = Mathf.Max(0f, hitTimer - deltaTime);
            float normalized = 1f - hitTimer / HitResponseSeconds;
            float scale = Mathf.Lerp(0.95f, 1f, normalized);
            enemyHitTarget.localScale = Vector3.Scale(
                enemyBaseScale,
                new Vector3(scale, scale, 1f));
            if (hitTimer <= 0f)
            {
                RestoreHitTarget();
            }
        }

        private void CaptureEnemyBaseScale()
        {
            if (enemyHitTarget == null || hasEnemyBaseScale)
            {
                return;
            }

            enemyBaseScale = enemyHitTarget.localScale;
            hasEnemyBaseScale = true;
        }

        private void RestoreHitTarget()
        {
            if (enemyHitTarget != null && hasEnemyBaseScale)
            {
                enemyHitTarget.localScale = enemyBaseScale;
            }

            hitTimer = 0f;
        }

        private void SetSemanticVisible(bool visible)
        {
            if (semanticFloatingGroup == null)
            {
                return;
            }

            semanticFloatingGroup.alpha = visible ? 1f : 0f;
            semanticFloatingGroup.interactable = false;
            semanticFloatingGroup.blocksRaycasts = false;
        }

        private void OnDisable()
        {
            EndBattle();
        }

        private void OnDestroy()
        {
            EndBattle();
        }

        private void DestroyOwnedVisualSprites()
        {
            for (int index = 0; index < ownedVisualSprites.Count; index++)
            {
                Sprite sprite = ownedVisualSprites[index];
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }

            ownedVisualSprites.Clear();
        }
    }
}
