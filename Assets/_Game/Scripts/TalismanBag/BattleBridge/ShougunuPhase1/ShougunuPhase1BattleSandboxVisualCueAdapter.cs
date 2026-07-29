using System;
using System.Collections.Generic;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BattleBridge.ShougunuPhase1
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(700)]
    public sealed class ShougunuPhase1BattleSandboxVisualCueAdapter :
        MonoBehaviour
    {
        private sealed class FloatingTextInstance
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
            public float startedAt;
            public float lifetime;
        }

        [SerializeField] private bool devOnly = true;
        [SerializeField] private RectTransform floatingTextAnchors;
        [SerializeField] private RectTransform damageDealtAnchor;
        [SerializeField] private RectTransform damageTakenAnchor;
        [SerializeField] private RectTransform shieldBreakAnchor;
        [SerializeField] private RectTransform statusDamageAnchor;
        [SerializeField] private RectTransform mechanicFloatingRoot;
        [SerializeField] private Text mechanicFloatingText;
        [SerializeField] private CanvasGroup mechanicFloatingCanvasGroup;
        [SerializeField] private Image playerHitFeedback;
        [SerializeField] private GameObject shougunuRoot;

        private readonly List<FloatingTextInstance> floatingTexts = new();
        private readonly HashSet<string> correlatedItemLedgerEventIds =
            new(StringComparer.Ordinal);
        private ShougunuPhase1VisualPrototypeController visualController;
        private BattleSandboxAuthoredTmpPresentation authoredTmpPresentation;
        private int currentGeneration;
        private long lastItemHitTick = -1L;
        private int rejectedStaleEventCount;
        private bool authoredStateCaptured;
        private Transform floatingFeedbackRoot;
        private bool floatingFeedbackRootAuthoredActive;
        private bool floatingAnchorsAuthoredActive;
        private bool damageDealtAnchorAuthoredActive;
        private bool damageTakenAnchorAuthoredActive;
        private bool shieldBreakAnchorAuthoredActive;
        private bool statusDamageAnchorAuthoredActive;
        private bool mechanicRootAuthoredActive;
        private string mechanicAuthoredText = string.Empty;
        private float mechanicAuthoredAlpha = 1f;
        private Color playerHitAuthoredColor = Color.clear;
        private float majorCueUntil;
        private float majorCueStartedAt;
        private float playerHitUntil;
        private long presentationSequence;

        public bool DevOnly => devOnly;
        public bool OwnsBattleTruth => false;
        public int CurrentGeneration => currentGeneration;
        public int RejectedStaleEventCount => rejectedStaleEventCount;
        public RectTransform FloatingTextAnchors => floatingTextAnchors;
        public RectTransform DamageDealtAnchor => damageDealtAnchor;
        public RectTransform DamageTakenAnchor => damageTakenAnchor;
        public RectTransform ShieldBreakAnchor => shieldBreakAnchor;
        public RectTransform StatusDamageAnchor => statusDamageAnchor;
        public RectTransform MechanicFloatingRoot => mechanicFloatingRoot;
        public Text MechanicFloatingText => mechanicFloatingText;
        public Image PlayerHitFeedback => playerHitFeedback;
        public GameObject ShougunuRoot => shougunuRoot;
        public BattleSandboxAuthoredTmpPresentation AuthoredTmpPresentation =>
            authoredTmpPresentation;

        public bool IsBindingComplete =>
            devOnly
            && floatingTextAnchors != null
            && damageDealtAnchor != null
            && damageTakenAnchor != null
            && shieldBreakAnchor != null
            && statusDamageAnchor != null
            && mechanicFloatingRoot != null
            && mechanicFloatingText != null
            && mechanicFloatingCanvasGroup != null
            && playerHitFeedback != null
            && shougunuRoot != null;

        private void Awake()
        {
            CaptureAuthoredState();
        }

        public void BeginGeneration(int resetGeneration)
        {
            CaptureAuthoredState();
            currentGeneration = resetGeneration;
            lastItemHitTick = -1L;
            rejectedStaleEventCount = 0;
            correlatedItemLedgerEventIds.Clear();
            presentationSequence = 0L;
            ClearTransientPresentation();
            EnsureFloatingAnchorActivity();
            ResolveVisualController();
            TryPlayPresentationAction(
                ShougunuPhase1PresentationAction.Idle);
        }

        public void BindPresentation(
            BattleSandboxAuthoredTmpPresentation presentation)
        {
            authoredTmpPresentation = presentation;
        }

        public bool RegisterAcceptedItemPresentation(
            string damageLedgerEventId)
        {
            return !string.IsNullOrWhiteSpace(damageLedgerEventId)
                && correlatedItemLedgerEventIds.Add(
                    damageLedgerEventId.Trim());
        }

        public void Render(
            ShougunuPhase1BattleDisplayEvent displayEvent,
            int expectedGeneration)
        {
            if (displayEvent == null
                || !AcceptsGeneration(displayEvent, expectedGeneration)
                || expectedGeneration != currentGeneration)
            {
                rejectedStaleEventCount++;
                return;
            }
            if (!displayEvent.playerVisible || displayEvent.developerOnly)
            {
                return;
            }

            ResolveVisualController();
            switch (displayEvent.channel)
            {
                case ShougunuPhase1BattleDisplayChannel
                    .ItemToEnemyShellDamage:
                    if (correlatedItemLedgerEventIds.Contains(
                            displayEvent.ledgerEventId))
                    {
                        PlayItemHitOncePerTick(displayEvent.battleTick);
                        break;
                    }
                    SpawnFloatingText(
                        damageDealtAnchor,
                        "-" + displayEvent.amount + " SH",
                        0.92f,
                        BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                        BattleSandboxAuthoredTmpPresentation
                            .SettlementShellPaletteKey);
                    PlayItemHitOncePerTick(displayEvent.battleTick);
                    break;
                case ShougunuPhase1BattleDisplayChannel
                    .ItemToEnemyHpDamage:
                    if (correlatedItemLedgerEventIds.Contains(
                            displayEvent.ledgerEventId))
                    {
                        PlayItemHitOncePerTick(displayEvent.battleTick);
                        break;
                    }
                    SpawnFloatingText(
                        damageDealtAnchor,
                        "-" + displayEvent.amount + " HP",
                        0.96f,
                        BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                        BattleSandboxAuthoredTmpPresentation
                            .SettlementHpPaletteKey);
                    PlayItemHitOncePerTick(displayEvent.battleTick);
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemyShellBreak:
                    SpawnFloatingText(
                        shieldBreakAnchor,
                        "BREAK",
                        1.15f,
                        BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                        BattleSandboxAuthoredTmpPresentation
                            .SettlementBreakPaletteKey,
                        1.15f);
                    TryPlayPresentationAction(
                        ShougunuPhase1PresentationAction.ShellBreak);
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemyCoreExpose:
                    ShowMajorMessage("核心暴露 · 6秒窗口", 1.25f);
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemyRecover:
                    ShowMajorMessage("护壳重构", 1.05f);
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemyDefeated:
                    ShowMajorMessage("守骨奴第一阶段 · 击破", 3f);
                    TryPlayPresentationAction(
                        ShougunuPhase1PresentationAction.Defeated);
                    break;
                case ShougunuPhase1BattleDisplayChannel
                    .EnemyToPlayerDamage:
                    SpawnFloatingText(
                        damageTakenAnchor,
                        "-" + displayEvent.amount + " HP",
                        1f,
                        BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                        BattleSandboxAuthoredTmpPresentation
                            .SettlementHpPaletteKey);
                    TriggerPlayerHitFeedback();
                    break;
                case ShougunuPhase1BattleDisplayChannel
                    .EnemyToPlayerStatusOrArea:
                    SpawnFloatingText(
                        statusDamageAnchor,
                        "-" + displayEvent.amount + " HP",
                        1.2f,
                        BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                        BattleSandboxAuthoredTmpPresentation
                            .SettlementHpPaletteKey);
                    ShowMajorMessage("地缚爆发", 1.15f);
                    TriggerPlayerHitFeedback();
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemySkillIntent:
                    RenderEnemyIntent(displayEvent.sourceId);
                    break;
                case ShougunuPhase1BattleDisplayChannel.EnemySkillResolved:
                    RenderEnemyResolved(
                        displayEvent.sourceId,
                        displayEvent.amount);
                    break;
            }
        }

        public void ShowMajorMessage(string message, float duration)
        {
            CaptureAuthoredState();
            if (authoredTmpPresentation != null
                && currentGeneration > 0
                && mechanicFloatingRoot != null)
            {
                presentationSequence++;
                bool spawned = authoredTmpPresentation.TrySpawn(
                    currentGeneration,
                    "battle.major.g"
                    + currentGeneration
                    + "."
                    + presentationSequence,
                    "battle.major",
                    mechanicFloatingRoot,
                    message,
                    BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                    0.82f,
                    0f,
                    Mathf.Clamp(duration, 1.4f, 1.8f));
                if (!spawned)
                {
                    Debug.LogError(
                        "[I031 Nian Presentation] Authored TMP major cue "
                        + "was rejected: "
                        + (message ?? string.Empty));
                }
                return;
            }
            if (mechanicFloatingRoot == null || mechanicFloatingText == null)
            {
                return;
            }

            mechanicFloatingText.text = message ?? string.Empty;
            if (mechanicFloatingCanvasGroup != null)
            {
                mechanicFloatingCanvasGroup.alpha = 1f;
            }
            mechanicFloatingRoot.gameObject.SetActive(true);
            majorCueStartedAt = Time.unscaledTime;
            majorCueUntil =
                majorCueStartedAt + Mathf.Max(0.25f, duration);
        }

        public void EndBattleMode()
        {
            ClearTransientPresentation();
            TryPlayPresentationAction(
                ShougunuPhase1PresentationAction.Idle);
            RestoreFloatingAnchorActivity();
            currentGeneration = 0;
            lastItemHitTick = -1L;
            correlatedItemLedgerEventIds.Clear();
        }

        public static bool AcceptsGeneration(
            ShougunuPhase1BattleDisplayEvent displayEvent,
            int expectedGeneration)
        {
            return displayEvent != null
                && expectedGeneration > 0
                && displayEvent.resetGeneration == expectedGeneration;
        }

        private void RenderEnemyIntent(string actionPatternId)
        {
            if (string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.BasicAttack,
                    StringComparison.Ordinal))
            {
                TryPlayPresentationAction(
                    ShougunuPhase1PresentationAction.BasicAttack);
                return;
            }
            if (string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair,
                    StringComparison.Ordinal))
            {
                ShowMajorMessage("技能一 · 护壳修复", 1.05f);
                TryPlayPresentationAction(
                    ShougunuPhase1PresentationAction.Skill1);
                return;
            }
            if (string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                    StringComparison.Ordinal))
            {
                ShowMajorMessage("技能二 · 缚索重击", 1.05f);
                TryPlayPresentationAction(
                    ShougunuPhase1PresentationAction.Skill2);
                return;
            }
            if (string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst,
                    StringComparison.Ordinal))
            {
                ShowMajorMessage("技能三 · 地缚爆发", 1.15f);
                TryPlayPresentationAction(
                    ShougunuPhase1PresentationAction.Skill3);
            }
        }

        private void RenderEnemyResolved(string actionPatternId, int amount)
        {
            if (string.Equals(
                    actionPatternId,
                    ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair,
                    StringComparison.Ordinal))
            {
                ShowMajorMessage("护壳修复 +" + amount, 1.05f);
            }
            else if (string.Equals(
                actionPatternId,
                ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                StringComparison.Ordinal))
            {
                ShowMajorMessage("缚索重击", 0.9f);
            }
        }

        private void PlayItemHitOncePerTick(long battleTick)
        {
            if (battleTick == lastItemHitTick)
            {
                return;
            }

            lastItemHitTick = battleTick;
            TryPlayPresentationAction(
                ShougunuPhase1PresentationAction.Hit);
        }

        private void TriggerPlayerHitFeedback()
        {
            if (playerHitFeedback == null)
            {
                return;
            }

            Color visible = playerHitAuthoredColor;
            visible.a = 0.72f;
            playerHitFeedback.color = visible;
            playerHitUntil = Time.unscaledTime + 0.24f;
        }

        private void SpawnFloatingText(
            RectTransform anchor,
            string message,
            float lifetime,
            BattleSandboxAuthoredTmpStyle style =
                BattleSandboxAuthoredTmpStyle.PrimaryWarm,
            string paletteKey = null,
            float scale = 1f)
        {
            EnsureFloatingAnchorActivity();
            if (authoredTmpPresentation != null
                && currentGeneration > 0
                && anchor != null)
            {
                presentationSequence++;
                bool spawned = authoredTmpPresentation.TrySpawn(
                    currentGeneration,
                    "battle.float.g"
                    + currentGeneration
                    + "."
                    + presentationSequence,
                    "battle.float",
                    anchor,
                    message,
                    style,
                    scale,
                    0f,
                    Mathf.Clamp(lifetime, 1.4f, 1.8f),
                    paletteKey);
                if (!spawned)
                {
                    Debug.LogError(
                        "[I031 Nian Presentation] Authored TMP combat cue "
                        + "was rejected: "
                        + (message ?? string.Empty));
                }
                return;
            }
            if (anchor == null || mechanicFloatingText == null)
            {
                return;
            }

            GameObject textObject = new(
                "ShougunuP1FloatingText_Runtime",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(CanvasGroup));
            textObject.hideFlags =
                HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            textObject.layer = anchor.gameObject.layer;
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.SetParent(anchor, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = mechanicFloatingText.rectTransform.sizeDelta;

            Text text = textObject.GetComponent<Text>();
            text.font = mechanicFloatingText.font;
            text.fontSize = mechanicFloatingText.fontSize;
            text.fontStyle = mechanicFloatingText.fontStyle;
            text.alignment = mechanicFloatingText.alignment;
            text.alignByGeometry = mechanicFloatingText.alignByGeometry;
            text.resizeTextForBestFit =
                mechanicFloatingText.resizeTextForBestFit;
            text.resizeTextMinSize = mechanicFloatingText.resizeTextMinSize;
            text.resizeTextMaxSize = mechanicFloatingText.resizeTextMaxSize;
            text.horizontalOverflow =
                mechanicFloatingText.horizontalOverflow;
            text.verticalOverflow = mechanicFloatingText.verticalOverflow;
            text.lineSpacing = mechanicFloatingText.lineSpacing;
            text.supportRichText = mechanicFloatingText.supportRichText;
            text.material = mechanicFloatingText.material;
            text.color = mechanicFloatingText.color;
            text.raycastTarget = false;
            text.text = message ?? string.Empty;

            CanvasGroup group = textObject.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
            floatingTexts.Add(new FloatingTextInstance
            {
                gameObject = textObject,
                rectTransform = rect,
                canvasGroup = group,
                startedAt = Time.unscaledTime,
                lifetime = Mathf.Max(0.25f, lifetime)
            });
        }

        private void LateUpdate()
        {
            float now = Time.unscaledTime;
            for (int index = floatingTexts.Count - 1; index >= 0; index--)
            {
                FloatingTextInstance instance = floatingTexts[index];
                if (instance?.gameObject == null)
                {
                    floatingTexts.RemoveAt(index);
                    continue;
                }

                float progress = Mathf.Clamp01(
                    (now - instance.startedAt) / instance.lifetime);
                instance.rectTransform.anchoredPosition =
                    new Vector2(0f, progress * 42f);
                instance.canvasGroup.alpha =
                    1f - Mathf.SmoothStep(0.58f, 1f, progress);
                if (progress >= 1f)
                {
                    Destroy(instance.gameObject);
                    floatingTexts.RemoveAt(index);
                }
            }

            if (majorCueUntil > now)
            {
                if (mechanicFloatingRoot != null)
                {
                    mechanicFloatingRoot.gameObject.SetActive(true);
                }
                if (mechanicFloatingCanvasGroup != null)
                {
                    float duration = Mathf.Max(
                        0.01f,
                        majorCueUntil - majorCueStartedAt);
                    float progress =
                        (now - majorCueStartedAt) / duration;
                    mechanicFloatingCanvasGroup.alpha =
                        1f - Mathf.SmoothStep(0.72f, 1f, progress);
                }
            }
            else if (majorCueUntil > 0f)
            {
                HideMajorCueForBattle();
            }

            if (playerHitFeedback != null)
            {
                if (playerHitUntil > now)
                {
                    float remaining = Mathf.Clamp01(
                        (playerHitUntil - now) / 0.24f);
                    Color color = playerHitAuthoredColor;
                    color.a = Mathf.Lerp(
                        playerHitAuthoredColor.a,
                        0.72f,
                        remaining);
                    playerHitFeedback.color = color;
                }
                else if (playerHitUntil > 0f)
                {
                    playerHitFeedback.color = playerHitAuthoredColor;
                    playerHitUntil = 0f;
                }
            }
        }

        private void ResolveVisualController()
        {
            if (visualController == null && shougunuRoot != null)
            {
                visualController =
                    shougunuRoot
                        .GetComponent<ShougunuPhase1VisualPrototypeController>();
            }
        }

        private bool TryPlayPresentationAction(
            ShougunuPhase1PresentationAction action)
        {
            ResolveVisualController();
            return visualController != null
                && visualController.TryPlayPresentationAction(action);
        }

        private void EnsureFloatingAnchorActivity()
        {
            CaptureAuthoredState();
            if (floatingFeedbackRoot != null)
            {
                floatingFeedbackRoot.gameObject.SetActive(true);
            }
            SetActive(floatingTextAnchors, true);
            SetActive(damageDealtAnchor, true);
            SetActive(damageTakenAnchor, true);
            SetActive(shieldBreakAnchor, true);
            SetActive(statusDamageAnchor, true);
        }

        private void RestoreFloatingAnchorActivity()
        {
            if (!authoredStateCaptured)
            {
                return;
            }
            SetActive(
                statusDamageAnchor,
                statusDamageAnchorAuthoredActive);
            SetActive(
                shieldBreakAnchor,
                shieldBreakAnchorAuthoredActive);
            SetActive(
                damageTakenAnchor,
                damageTakenAnchorAuthoredActive);
            SetActive(
                damageDealtAnchor,
                damageDealtAnchorAuthoredActive);
            SetActive(
                floatingTextAnchors,
                floatingAnchorsAuthoredActive);
            if (floatingFeedbackRoot != null)
            {
                floatingFeedbackRoot.gameObject.SetActive(
                    floatingFeedbackRootAuthoredActive);
            }
        }

        private static void SetActive(Component component, bool active)
        {
            if (component != null)
            {
                component.gameObject.SetActive(active);
            }
        }

        private void CaptureAuthoredState()
        {
            if (authoredStateCaptured)
            {
                return;
            }

            mechanicRootAuthoredActive = mechanicFloatingRoot != null
                && mechanicFloatingRoot.gameObject.activeSelf;
            floatingFeedbackRoot = floatingTextAnchors == null
                ? null
                : floatingTextAnchors.parent;
            floatingFeedbackRootAuthoredActive =
                floatingFeedbackRoot != null
                && floatingFeedbackRoot.gameObject.activeSelf;
            floatingAnchorsAuthoredActive = floatingTextAnchors != null
                && floatingTextAnchors.gameObject.activeSelf;
            damageDealtAnchorAuthoredActive = damageDealtAnchor != null
                && damageDealtAnchor.gameObject.activeSelf;
            damageTakenAnchorAuthoredActive = damageTakenAnchor != null
                && damageTakenAnchor.gameObject.activeSelf;
            shieldBreakAnchorAuthoredActive = shieldBreakAnchor != null
                && shieldBreakAnchor.gameObject.activeSelf;
            statusDamageAnchorAuthoredActive = statusDamageAnchor != null
                && statusDamageAnchor.gameObject.activeSelf;
            mechanicAuthoredText =
                mechanicFloatingText == null
                    ? string.Empty
                    : mechanicFloatingText.text;
            mechanicAuthoredAlpha =
                mechanicFloatingCanvasGroup == null
                    ? 1f
                    : mechanicFloatingCanvasGroup.alpha;
            playerHitAuthoredColor =
                playerHitFeedback == null
                    ? Color.clear
                    : playerHitFeedback.color;
            authoredStateCaptured = true;
        }

        private void ClearTransientPresentation()
        {
            for (int index = floatingTexts.Count - 1; index >= 0; index--)
            {
                if (floatingTexts[index]?.gameObject != null)
                {
                    Destroy(floatingTexts[index].gameObject);
                }
            }
            floatingTexts.Clear();
            playerHitUntil = 0f;
            if (playerHitFeedback != null)
            {
                playerHitFeedback.color = playerHitAuthoredColor;
            }
            HideMajorCueForBattle();
        }

        private void HideMajorCueForBattle()
        {
            majorCueUntil = 0f;
            majorCueStartedAt = 0f;
            if (mechanicFloatingText != null)
            {
                mechanicFloatingText.text = mechanicAuthoredText;
            }
            if (mechanicFloatingCanvasGroup != null)
            {
                mechanicFloatingCanvasGroup.alpha = mechanicAuthoredAlpha;
            }
            if (mechanicFloatingRoot != null)
            {
                mechanicFloatingRoot.gameObject.SetActive(false);
            }
        }

        private void RestoreAuthoredState()
        {
            if (!authoredStateCaptured)
            {
                return;
            }
            if (mechanicFloatingText != null)
            {
                mechanicFloatingText.text = mechanicAuthoredText;
            }
            if (mechanicFloatingCanvasGroup != null)
            {
                mechanicFloatingCanvasGroup.alpha = mechanicAuthoredAlpha;
            }
            if (mechanicFloatingRoot != null)
            {
                mechanicFloatingRoot.gameObject.SetActive(
                    mechanicRootAuthoredActive);
            }
            if (playerHitFeedback != null)
            {
                playerHitFeedback.color = playerHitAuthoredColor;
            }
        }

        private void OnDisable()
        {
            ClearTransientPresentation();
            TryPlayPresentationAction(
                ShougunuPhase1PresentationAction.Idle);
            RestoreFloatingAnchorActivity();
            RestoreAuthoredState();
        }
    }
}
