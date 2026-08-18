using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Presentation.Items;
using UnityEngine;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattlePresentationRoot : MonoBehaviour
    {
        public const string SourceCarrierReferenceMissingDiagnostic =
            "FORMAL_PRESENTATION_SOURCE_CARRIER_REFERENCE_MISSING";
        public const string SourceCarrierReferencesValidDiagnostic =
            "FORMAL_PRESENTATION_SOURCE_CARRIER_REFERENCES_VALID";
        public const string ExternalDamageFloatPoolMissingDiagnostic =
            "FORMAL_PRESENTATION_EXTERNAL_DAMAGE_FLOAT_POOL_MISSING";
        public const string ExternalCueFxAudioRootMissingDiagnostic =
            "FORMAL_PRESENTATION_EXTERNAL_CUE_FX_AUDIO_ROOT_MISSING";
        public const string DownstreamCompositionReferencesValidDiagnostic =
            "FORMAL_PRESENTATION_DOWNSTREAM_COMPOSITION_REFERENCES_VALID";
        public const string ItemSourceProviderInvalidDiagnostic =
            "FORMAL_PRESENTATION_ITEM_SOURCE_PROVIDER_INVALID";
        public const string ItemSourceProviderBoundDiagnostic =
            "FORMAL_PRESENTATION_ITEM_SOURCE_PROVIDER_BOUND";
        public const string ItemSourceProviderUnboundDiagnostic =
            "FORMAL_PRESENTATION_ITEM_SOURCE_PROVIDER_UNBOUND";

        private const int MaxPendingCausalCueCount = 64;

        private sealed class ScheduledEnemyCarrier
        {
            public ScheduledEnemyCarrier(string actorId, int stableOrder)
            {
                ActorId = actorId ?? string.Empty;
                StableOrder = stableOrder;
            }

            public string ActorId { get; }
            public int StableOrder { get; }
        }

        private sealed class PendingCausalCue
        {
            public PendingCausalCue(
                C1FormalRealtimeBattleCue cue,
                string targetKey,
                float releaseAt)
            {
                Cue = cue;
                TargetKey = targetKey ?? string.Empty;
                ReleaseAt = releaseAt;
            }

            public C1FormalRealtimeBattleCue Cue { get; }
            public string TargetKey { get; }
            public float ReleaseAt { get; private set; }

            public void ShiftClock(float seconds)
            {
                ReleaseAt += Mathf.Max(0f, seconds);
            }
        }

        private sealed class ResolvedCausalSource
        {
            public ResolvedCausalSource(
                C1Pool15FormalItemSourceAnchorBinding binding,
                FormalBattleCausalItemStyle style,
                string grammarKey)
            {
                Binding = binding;
                Style = style;
                GrammarKey = grammarKey ?? string.Empty;
            }

            public C1Pool15FormalItemSourceAnchorBinding Binding { get; }
            public FormalBattleCausalItemStyle Style { get; }
            public string GrammarKey { get; }
        }

        [SerializeField] private FormalBattlePresentationProfile profile;
        [SerializeField] private FormalBattlePlayerPresentationView playerView;
        [SerializeField] private FormalBattleEnemySlotView[] enemySlots =
            Array.Empty<FormalBattleEnemySlotView>();
        [SerializeField] private FormalBattleSelectedEnemyHudView
            selectedEnemyHud;
        [SerializeField] private FormalBattleDamageFloatPool damageFloatPool;
        [SerializeField] private FormalBattleCueFxAudioRoot cueFxAudioRoot;
        [SerializeField] private FormalBattleCausalSourceVisualView
            causalSourceVisualView;
        [SerializeField] private TMP_Text stageLabel;

        private readonly HashSet<string> consumedCueIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> consumedApplicationRoles =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> playedCardTriggerEventIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> shownFeedbackApplicationKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, ScheduledEnemyCarrier>
            scheduledEnemyCarriers =
                new Dictionary<string, ScheduledEnemyCarrier>(
                    StringComparer.Ordinal);
        private readonly HashSet<string> reportedCausalDiagnostics =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<PendingCausalCue> pendingCausalCues =
            new List<PendingCausalCue>(MaxPendingCausalCueCount);
        private readonly Dictionary<string, float> causalArrivalByTarget =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private C1FormalRealtimeBattleSessionStateSnapshot currentState;
        private C1ExactBattleSandboxItemArrangementPresenter
            itemSourceProvider;
        private C1FormalItemPresentationCatalogSnapshot
            itemPresentationCatalog;
        private string activeStartSignature = string.Empty;
        private string activeStageId = string.Empty;
        private string currentTargetActorId = string.Empty;
        private int currentTargetStableOrder = -1;
        private long lastConsumedSequence;
        private int consumedInitialEnvelopeCount;
        private bool paused;
        private float pendingPausedAt;
        private ItemRarityContourBloomVfx activePeriodicNianCarrier;
        private string activePeriodicNianSourceItemInstanceId = string.Empty;

        public string ActiveStartSignature => activeStartSignature;
        public long LastConsumedSequence => lastConsumedSequence;
        public int ConsumedCueCount => consumedCueIds.Count;
        public int ConsumedInitialEnvelopeCount => consumedInitialEnvelopeCount;
        public string CurrentTargetActorId => currentTargetActorId;
        public int CurrentTargetStableOrder => currentTargetStableOrder;
        public FormalBattleDamageFloatPool DamageFloatPool => damageFloatPool;
        public FormalBattleCueFxAudioRoot CueFxAudioRoot => cueFxAudioRoot;
        public FormalBattleCausalSourceVisualView CausalSourceVisualView =>
            causalSourceVisualView;
        public int PendingCausalCueCount => pendingCausalCues.Count;
        public bool ItemSourceProviderBound =>
            itemSourceProvider != null
            && itemPresentationCatalog != null;

        private void Update()
        {
            if (!paused && pendingCausalCues.Count > 0)
            {
                ReleaseReadyPendingCues(Time.unscaledTime);
            }
        }

        public bool BeginRealtime(
            C1FormalRealtimeBattleSessionStartSnapshot start,
            C1FormalItemSessionSnapshot itemSnapshot,
            string stageId,
            out string diagnostic)
        {
            if (start == null
                || !start.accepted
                || start.stateSnapshot == null
                || string.IsNullOrWhiteSpace(start.canonicalSignature))
            {
                diagnostic = "FORMAL_PRESENTATION_START_ENVELOPE_REJECTED";
                return false;
            }

            if (string.Equals(
                    activeStartSignature,
                    start.canonicalSignature,
                    StringComparison.Ordinal))
            {
                diagnostic = "FORMAL_PRESENTATION_START_ALREADY_CONSUMED";
                return true;
            }

            ResetPresentation();
            activeStartSignature = start.canonicalSignature;
            activeStageId = stageId ?? string.Empty;
            currentState = start.stateSnapshot;
            consumedInitialEnvelopeCount = 1;
            if (stageLabel != null)
            {
                stageLabel.text = activeStageId;
            }
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (!BindState(currentState, out diagnostic))
            {
                ResetPresentation();
                return false;
            }

            ConsumeCues(start.initialCues);
            diagnostic = "FORMAL_PRESENTATION_START_CONSUMED";
            return true;
        }

        private void OnDisable()
        {
            ResetPresentation();
            UnbindItemSourceProvider();
        }

        public bool ConsumeRealtime(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues,
            C1FormalItemSessionSnapshot itemSnapshot,
            out string diagnostic)
        {
            if (string.IsNullOrEmpty(activeStartSignature)
                || state == null
                || !state.accepted
                || !ValidateRealtimeStateShape(state))
            {
                diagnostic = "FORMAL_PRESENTATION_REALTIME_STATE_REJECTED";
                return false;
            }

            currentState = state;
            ConsumeCues(emittedCues);
            if (!EnsureStateBindingsWithoutVisualOverwrite(
                    state,
                    out diagnostic))
            {
                return false;
            }
            if (!ApplyObservabilityState(state, out diagnostic))
            {
                return false;
            }
            SetPaused(state.paused);
            diagnostic = "FORMAL_PRESENTATION_REALTIME_CONSUMED";
            return true;
        }

        public void ResetPresentation()
        {
            activeStartSignature = string.Empty;
            activeStageId = string.Empty;
            currentTargetActorId = string.Empty;
            currentTargetStableOrder = -1;
            currentState = null;
            lastConsumedSequence = 0L;
            consumedInitialEnvelopeCount = 0;
            paused = false;
            pendingPausedAt = 0f;
            ClearPeriodicNianPresentation();
            consumedCueIds.Clear();
            consumedApplicationRoles.Clear();
            playedCardTriggerEventIds.Clear();
            shownFeedbackApplicationKeys.Clear();
            scheduledEnemyCarriers.Clear();
            reportedCausalDiagnostics.Clear();
            pendingCausalCues.Clear();
            causalArrivalByTarget.Clear();
            playerView?.Clear();
            selectedEnemyHud?.Clear();
            if (enemySlots != null)
            {
                foreach (FormalBattleEnemySlotView slot in enemySlots)
                {
                    slot?.Clear();
                }
            }
            damageFloatPool?.Clear();
            cueFxAudioRoot?.Clear();
            causalSourceVisualView?.Clear();
            profile?.ClearTransientCausalArtwork();
            if (stageLabel != null)
            {
                stageLabel.text = string.Empty;
            }
        }

        public bool ValidateAuthoredReferences()
        {
            return ValidateDownstreamCompositionReferences(out _);
        }

        public bool ValidateSourceCarrierReferences(out string diagnostic)
        {
            if (profile == null
                || !profile.ValidateAuthoredReferences()
                || playerView == null
                || !playerView.ValidateAuthoredReferences()
                || enemySlots == null
                || enemySlots.Length != 3
                || enemySlots.Any(value => value == null
                    || !value.ValidateAuthoredReferences())
                || !enemySlots.Select(value => value.AuthoredStableOrder)
                    .OrderBy(value => value)
                    .SequenceEqual(new[] { 0, 1, 2 })
                || selectedEnemyHud == null
                || !selectedEnemyHud.ValidateAuthoredReferences()
                || causalSourceVisualView == null
                || !causalSourceVisualView.ValidateAuthoredReferences()
                || stageLabel == null
                || stageLabel.raycastTarget)
            {
                diagnostic = SourceCarrierReferenceMissingDiagnostic;
                return false;
            }

            diagnostic = SourceCarrierReferencesValidDiagnostic;
            return true;
        }

        public bool ValidateDownstreamCompositionReferences(
            out string diagnostic)
        {
            if (!ValidateSourceCarrierReferences(out diagnostic))
            {
                return false;
            }
            if (damageFloatPool == null
                || !damageFloatPool.ValidateAuthoredReferences())
            {
                diagnostic = ExternalDamageFloatPoolMissingDiagnostic;
                return false;
            }
            if (cueFxAudioRoot == null
                || !cueFxAudioRoot.ValidateAuthoredReferences())
            {
                diagnostic = ExternalCueFxAudioRootMissingDiagnostic;
                return false;
            }

            diagnostic = DownstreamCompositionReferencesValidDiagnostic;
            return true;
        }

        public bool BindItemSourceProvider(
            C1ExactBattleSandboxItemArrangementPresenter configuredProvider,
            C1FormalItemPresentationCatalogSnapshot configuredCatalog,
            out string diagnostic)
        {
            if (!ValidateItemSourceProvider(
                    configuredProvider,
                    configuredCatalog))
            {
                UnbindItemSourceProvider();
                diagnostic = ItemSourceProviderInvalidDiagnostic;
                return false;
            }

            if (!ReferenceEquals(itemSourceProvider, configuredProvider)
                || !ReferenceEquals(
                    itemPresentationCatalog,
                    configuredCatalog))
            {
                ClearCausalSourceState();
                itemSourceProvider = configuredProvider;
                itemPresentationCatalog = configuredCatalog;
            }

            diagnostic = ItemSourceProviderBoundDiagnostic;
            return true;
        }

        public void UnbindItemSourceProvider()
        {
            if (itemSourceProvider == null
                && itemPresentationCatalog == null)
            {
                return;
            }

            itemSourceProvider = null;
            itemPresentationCatalog = null;
            ClearCausalSourceState();
        }

        private void ConsumeCues(
            IReadOnlyList<C1FormalRealtimeBattleCue> cues)
        {
            if (cues == null)
            {
                return;
            }
            foreach (C1FormalRealtimeBattleCue cue in cues)
            {
                if (cue == null
                    || cue.sequence <= lastConsumedSequence
                    || string.IsNullOrEmpty(cue.cueId)
                    || !consumedCueIds.Add(cue.cueId))
                {
                    continue;
                }

                lastConsumedSequence = cue.sequence;
                string roleKey = BuildAcceptedApplicationRoleKey(cue);
                if (!string.IsNullOrEmpty(roleKey)
                    && !consumedApplicationRoles.Add(roleKey))
                {
                    continue;
                }
                ApplyCue(cue);
            }
        }

        private void ApplyCue(C1FormalRealtimeBattleCue cue)
        {
            switch (cue.cueKind)
            {
                case C1FormalRealtimeBattleCueKinds.SessionStarted:
                    ClearTransientsAndTarget();
                    break;
                case C1FormalRealtimeBattleCueKinds.ActorSpawnVisible:
                    BindActorFromCurrentState(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.TargetChanged:
                    ApplyAuthoritativeTarget(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.ItemTriggerScheduled:
                    ApplyItemTriggerScheduled(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted:
                    ApplyItemTriggerAccepted(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted:
                case C1FormalRealtimeBattleCueKinds.ActionProgressMutated:
                    ApplyItemTriggerAccepted(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.EnemyAttackScheduled:
                case C1FormalRealtimeBattleCueKinds.EnemySkillScheduled:
                    ApplyEnemyAttackScheduled(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted:
                case C1FormalRealtimeBattleCueKinds.EnemySkillAccepted:
                    ApplyEnemyAttackAccepted(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.EnemyPassiveTriggered:
                    ApplyEnemyPassiveTriggered(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.HitAccepted:
                    ApplyHit(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.PlayerHpChanged:
                    playerView.ApplyHp(
                        cue.playerHpAfter,
                        currentState?.playerSnapshot?.maxHp ?? 1);
                    break;
                case C1FormalRealtimeBattleCueKinds.ActorHpChanged:
                    if (!TryQueuePendingCausalFollowup(cue))
                    {
                        ApplyActorHpChanged(cue);
                    }
                    break;
                case C1FormalRealtimeBattleCueKinds.DamageFloatPayload:
                    if (!TryQueuePendingCausalFollowup(cue))
                    {
                        ApplyDamageFloat(cue);
                    }
                    break;
                case C1FormalRealtimeBattleCueKinds
                    .ApplicationFeedbackPayload:
                    ApplyApplicationFeedback(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.PlayerGuardChanged:
                case C1FormalRealtimeBattleCueKinds.StatusApplied:
                case C1FormalRealtimeBattleCueKinds.StatusRefreshed:
                case C1FormalRealtimeBattleCueKinds.StatusRemoved:
                    ApplySemanticFeedback(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.NianResourceChanged:
                    ApplyNianResourceChanged(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.ActorDefeated:
                    if (!TryQueuePendingCausalFollowup(cue))
                    {
                        ApplyActorDefeated(cue);
                    }
                    break;
                case C1FormalRealtimeBattleCueKinds.BattleTerminal:
                    ClearTarget();
                    break;
                case C1FormalRealtimeBattleCueKinds.SessionPaused:
                    SetPaused(true);
                    break;
                case C1FormalRealtimeBattleCueKinds.SessionResumed:
                    SetPaused(false);
                    break;
                case C1FormalRealtimeBattleCueKinds.SessionReset:
                    ResetPresentation();
                    break;
            }
        }

        private bool BindState(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            out string diagnostic)
        {
            if (state == null
                || state.playerSnapshot == null
                || state.actorSnapshots == null
                || state.actorSnapshots.Count < 1
                || state.actorSnapshots.Count > enemySlots.Length)
            {
                diagnostic = "FORMAL_PRESENTATION_STATE_SHAPE_REJECTED";
                return false;
            }

            playerView.Bind(
                state.playerSnapshot,
                state.nianStateSnapshot);
            HashSet<int> boundOrders = new HashSet<int>();
            foreach (C1FormalRealtimeBattleActorSnapshot actor
                     in state.actorSnapshots)
            {
                FormalBattleEnemySlotView slot = enemySlots.SingleOrDefault(
                    value => value.AuthoredStableOrder
                             == actor.stableActorOrder);
                if (slot == null
                    || !boundOrders.Add(actor.stableActorOrder)
                    || !slot.Bind(actor, profile, out diagnostic))
                {
                    diagnostic = "FORMAL_PRESENTATION_ACTOR_BIND_REJECTED_"
                                 + actor.stableActorOrder;
                    return false;
                }
            }

            foreach (FormalBattleEnemySlotView slot in enemySlots)
            {
                if (!boundOrders.Contains(slot.AuthoredStableOrder))
                {
                    slot.Clear();
                }
            }
            if (!ApplyObservabilityState(state, out diagnostic))
            {
                return false;
            }
            diagnostic = "FORMAL_PRESENTATION_STATE_BOUND";
            return true;
        }

        private bool ApplyObservabilityState(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            out string diagnostic)
        {
            C1FormalRealtimeBattleStatusSnapshot[] playerStatuses =
                state.statusSnapshots
                    .Where(value => value != null
                        && value.targetStableOrder == -1
                        && string.Equals(
                            value.targetActorId,
                            C1FormalRealtimeBattleSessionContract.PlayerActorId,
                            StringComparison.Ordinal))
                    .ToArray();
            if (!playerView.ApplyObservability(
                    state.playerSnapshot,
                    playerStatuses,
                    profile,
                    state.battleTimeMs,
                    out diagnostic))
            {
                return false;
            }
            playerView.ApplyNian(state.nianStateSnapshot);
            ApplyPeriodicNianPresentation(state);

            foreach (C1FormalRealtimeBattleActorSnapshot actor
                     in state.actorSnapshots)
            {
                FormalBattleEnemySlotView slot = enemySlots.SingleOrDefault(
                    value => value.AuthoredStableOrder
                             == actor.stableActorOrder);
                if (slot == null)
                {
                    diagnostic = "FORMAL_PRESENTATION_ACTOR_BIND_REJECTED_"
                                 + actor.stableActorOrder;
                    return false;
                }

                C1FormalRealtimeBattleStatusSnapshot[] actorStatuses =
                    state.statusSnapshots
                        .Where(value => value != null
                            && value.targetStableOrder
                                == actor.stableActorOrder
                            && string.Equals(
                                value.targetActorId,
                                actor.actorBalanceId,
                                StringComparison.Ordinal))
                        .ToArray();
                if (!slot.ApplyObservability(
                        actor,
                        actorStatuses,
                        profile,
                        state.battleTimeMs,
                        out diagnostic))
                {
                    return false;
                }
            }

            if (!ApplySelectedEnemyHud(state, out diagnostic))
            {
                return false;
            }

            diagnostic = "FORMAL_PRESENTATION_OBSERVABILITY_BOUND";
            return true;
        }

        private bool ApplySelectedEnemyHud(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            out string diagnostic)
        {
            if (string.IsNullOrEmpty(currentTargetActorId)
                || currentTargetStableOrder < 0)
            {
                selectedEnemyHud.Clear();
                diagnostic = "FORMAL_SELECTED_ENEMY_HUD_NO_TARGET";
                return true;
            }

            C1FormalRealtimeBattleActorSnapshot actor = state.actorSnapshots
                .SingleOrDefault(value => value != null
                    && value.stableActorOrder == currentTargetStableOrder
                    && string.Equals(
                        value.actorBalanceId,
                        currentTargetActorId,
                        StringComparison.Ordinal));
            if (actor == null)
            {
                selectedEnemyHud.Clear();
                diagnostic = "FORMAL_SELECTED_ENEMY_HUD_TARGET_MISSING";
                return false;
            }

            C1FormalRealtimeBattleStatusSnapshot[] statuses =
                state.statusSnapshots
                    .Where(value => value != null
                        && value.targetStableOrder == actor.stableActorOrder
                        && string.Equals(
                            value.targetActorId,
                            actor.actorBalanceId,
                            StringComparison.Ordinal))
                    .ToArray();
            return selectedEnemyHud.Bind(
                actor,
                statuses,
                profile,
                state.battleTimeMs,
                out diagnostic);
        }

        private bool EnsureStateBindingsWithoutVisualOverwrite(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            out string diagnostic)
        {
            HashSet<int> boundOrders = new HashSet<int>();
            foreach (C1FormalRealtimeBattleActorSnapshot actor
                     in state.actorSnapshots)
            {
                FormalBattleEnemySlotView slot = enemySlots.SingleOrDefault(
                    value => value.AuthoredStableOrder
                             == actor.stableActorOrder);
                if (slot == null
                    || !boundOrders.Add(actor.stableActorOrder))
                {
                    diagnostic = "FORMAL_PRESENTATION_ACTOR_BIND_REJECTED_"
                                 + actor.stableActorOrder;
                    return false;
                }

                if ((!slot.Bound
                     || !string.Equals(
                         slot.ActorBalanceId,
                         actor.actorBalanceId,
                         StringComparison.Ordinal))
                    && !slot.Bind(actor, profile, out diagnostic))
                {
                    diagnostic = "FORMAL_PRESENTATION_ACTOR_BIND_REJECTED_"
                                 + actor.stableActorOrder;
                    return false;
                }
            }

            foreach (FormalBattleEnemySlotView slot in enemySlots)
            {
                if (!boundOrders.Contains(slot.AuthoredStableOrder))
                {
                    slot.Clear();
                }
            }
            diagnostic = "FORMAL_PRESENTATION_STATE_REFERENCES_BOUND";
            return true;
        }

        private bool ValidateRealtimeStateShape(
            C1FormalRealtimeBattleSessionStateSnapshot state)
        {
            return state != null
                   && state.playerSnapshot != null
                   && state.actorSnapshots != null
                   && enemySlots != null
                   && state.actorSnapshots.Count >= 1
                   && state.actorSnapshots.Count <= enemySlots.Length;
        }

        private void BindActorFromCurrentState(C1FormalRealtimeBattleCue cue)
        {
            C1FormalRealtimeBattleActorSnapshot actor = FindExactActor(
                cue.targetActorId,
                cue.targetStableOrder);
            FormalBattleEnemySlotView slot = enemySlots.SingleOrDefault(
                value => value.AuthoredStableOrder == cue.targetStableOrder);
            if (actor != null && slot != null)
            {
                slot.Bind(actor, profile, out _);
            }
        }

        private void ApplyAuthoritativeTarget(C1FormalRealtimeBattleCue cue)
        {
            ClearTarget();
            if (!TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView slot)
                || slot.Defeated)
            {
                return;
            }
            currentTargetActorId = cue.targetActorId;
            currentTargetStableOrder = cue.targetStableOrder;
            slot.SetTarget(true);
            if (currentState != null)
            {
                ApplySelectedEnemyHud(currentState, out _);
            }
        }

        private void ApplyEnemyAttackScheduled(C1FormalRealtimeBattleCue cue)
        {
            if (string.IsNullOrEmpty(cue.sourceId)
                || !TryGetExactEnemySlot(
                    cue.sourceId,
                    cue.sourceStableOrder,
                    out FormalBattleEnemySlotView slot)
                || slot.Defeated)
            {
                return;
            }
            scheduledEnemyCarriers[cue.sourceId] =
                new ScheduledEnemyCarrier(
                    slot.ActorBalanceId,
                    slot.AuthoredStableOrder);
            slot.PlayTelegraph();
        }

        private void ApplyEnemyAttackAccepted(C1FormalRealtimeBattleCue cue)
        {
            if (string.IsNullOrEmpty(cue.sourceId)
                || !scheduledEnemyCarriers.TryGetValue(
                    cue.sourceId,
                    out ScheduledEnemyCarrier carrier)
                || !TryGetExactEnemySlot(
                    carrier.ActorId,
                    carrier.StableOrder,
                    out FormalBattleEnemySlotView slot)
                || slot.Defeated)
            {
                return;
            }
            scheduledEnemyCarriers.Remove(cue.sourceId);
            slot.PlayAttack();
            cueFxAudioRoot.PlayEnemyAttackAudio();
        }

        private void ApplyEnemyPassiveTriggered(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null
                || !TryGetExactEnemySlot(
                    cue.sourceId,
                    cue.sourceStableOrder,
                    out FormalBattleEnemySlotView slot)
                || slot.Defeated)
            {
                return;
            }

            slot.PlayAttack();
        }

        private void ApplyHit(C1FormalRealtimeBattleCue cue)
        {
            if (IsPlayerOwnedItemHit(cue)
                && TryResolveReceiver(cue, out RectTransform receiver)
                && TryPlayItemCausalBridge(cue, receiver))
            {
                string targetKey = BuildCausalPendingTargetKey(cue);
                float arrivalDelay = Mathf.Clamp(
                    profile.CausalRibbonDuration * 0.5f,
                    0.1f,
                    0.25f);
                float releaseAt = Time.unscaledTime + arrivalDelay;
                causalArrivalByTarget[targetKey] = releaseAt;
                EnqueuePendingCausalCue(cue, targetKey, releaseAt);
                return;
            }

            ApplyHitFeedback(cue);
        }

        private void ApplyHitFeedback(C1FormalRealtimeBattleCue cue)
        {
            if (string.Equals(
                    cue.targetActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    StringComparison.Ordinal)
                && cue.targetStableOrder == -1)
            {
                playerView.PlayHit();
                cueFxAudioRoot.PlayImpact(playerView.DamageAnchor);
                return;
            }
            if (TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView slot))
            {
                slot.PlayHit();
                cueFxAudioRoot.PlayImpact(slot.DamageAnchor);
                return;
            }

            ReportCausalDiagnosticOnce(
                "TARGET_ANCHOR_MISSING",
                cue);
            causalSourceVisualView?.CancelAcceptedEvent(
                cue.acceptedApplicationEventId);
        }

        private void ApplyItemTriggerScheduled(
            C1FormalRealtimeBattleCue cue)
        {
            if (!TryResolveCausalSource(
                    cue,
                    out ResolvedCausalSource resolved,
                    out string resolutionDiagnostic))
            {
                ReportCausalDiagnosticOnce(resolutionDiagnostic, cue);
                return;
            }

            RectTransform sourceAnchor = resolved.Binding.sourceAnchor;
            bool shown = causalSourceVisualView != null
                         && causalSourceVisualView.ShowScheduled(
                     resolved.GrammarKey,
                     resolved.Binding.sourceAnchor,
                     resolved.Style,
                     resolved.Binding.artwork,
                     out sourceAnchor);
            if (!shown)
            {
                ReportCausalDiagnosticOnce(
                    "SOURCE_LOCAL_CARRIER_SKIPPED",
                    cue);
                sourceAnchor = resolved.Binding.sourceAnchor;
            }
            if (!cueFxAudioRoot.PlaySourcePulse(
                    sourceAnchor,
                    true,
                    resolved.Style))
            {
                ReportCausalDiagnosticOnce(
                    "FORMAL_SOURCE_PULSE_UNAVAILABLE",
                    cue);
            }
        }

        private void ApplyItemTriggerAccepted(
            C1FormalRealtimeBattleCue cue)
        {
            string resolutionDiagnostic = null;
            if (string.IsNullOrWhiteSpace(cue.acceptedApplicationEventId)
                || !TryResolveCausalSource(
                    cue,
                    out ResolvedCausalSource resolved,
                    out resolutionDiagnostic))
            {
                ReportCausalDiagnosticOnce(
                    string.IsNullOrWhiteSpace(
                        cue.acceptedApplicationEventId)
                        ? "ACCEPTED_EVENT_ID_MISSING"
                        : resolutionDiagnostic,
                    cue);
                return;
            }

            PlayCardTriggerOnce(cue, resolved);
            causalSourceVisualView.CancelAcceptedEvent(
                cue.acceptedApplicationEventId);
            RectTransform sourceAnchor = resolved.Binding.sourceAnchor;
            bool begun = causalSourceVisualView != null
                         && causalSourceVisualView.BeginAccepted(
                     cue.acceptedApplicationEventId,
                     resolved.GrammarKey,
                     resolved.Binding.sourceAnchor,
                     resolved.Style,
                     resolved.Binding.artwork,
                     out sourceAnchor);
            if (!begun)
            {
                ReportCausalDiagnosticOnce(
                    "SOURCE_LOCAL_CARRIER_SKIPPED",
                    cue);
                sourceAnchor = resolved.Binding.sourceAnchor;
            }

            if (!cueFxAudioRoot.PlaySourcePulse(
                    sourceAnchor,
                    false,
                    resolved.Style))
            {
                ReportCausalDiagnosticOnce(
                    "FORMAL_SOURCE_PULSE_UNAVAILABLE",
                    cue);
            }
        }

        private bool TryPlayItemCausalBridge(
            C1FormalRealtimeBattleCue cue,
            RectTransform receiver)
        {
            if (cue == null
                || receiver == null
                || cue.appliedDamage <= 0
                || !string.Equals(
                    cue.sourceOwner,
                    C1FormalRealtimeBattleSessionContract.PlayerOwner,
                    StringComparison.Ordinal))
            {
                return false;
            }

            if (!TryResolveCausalSource(
                    cue,
                    out ResolvedCausalSource resolved,
                    out string resolutionDiagnostic))
            {
                ReportCausalDiagnosticOnce(resolutionDiagnostic, cue);
                return false;
            }

            causalSourceVisualView.CancelAcceptedEvent(
                cue.acceptedApplicationEventId);
            RectTransform sourceAnchor = resolved.Binding.sourceAnchor;
            bool begun = causalSourceVisualView != null
                         && causalSourceVisualView.BeginAccepted(
                     cue.acceptedApplicationEventId,
                     resolved.GrammarKey,
                     resolved.Binding.sourceAnchor,
                     resolved.Style,
                     resolved.Binding.artwork,
                     out sourceAnchor);
            if (!begun)
            {
                ReportCausalDiagnosticOnce(
                    "ACCEPTED_LOCAL_CARRIER_SKIPPED",
                    cue);
                sourceAnchor = resolved.Binding.sourceAnchor;
            }

            string targetKey = string.Join("#", new[]
            {
                cue.targetActorId ?? string.Empty,
                cue.targetStableOrder.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            });
            if (!cueFxAudioRoot.PlayCausalBridge(
                    cue.acceptedApplicationEventId,
                    targetKey,
                    resolved.Style,
                    sourceAnchor,
                    receiver))
            {
                ReportCausalDiagnosticOnce("RIBBON_CARRIER_UNAVAILABLE", cue);
                causalSourceVisualView.CancelAcceptedEvent(
                    cue.acceptedApplicationEventId);
                return false;
            }
            return true;
        }

        private void PlayCardTriggerOnce(
            C1FormalRealtimeBattleCue cue,
            ResolvedCausalSource resolved)
        {
            if (cue == null
                || resolved?.Binding == null
                || string.IsNullOrWhiteSpace(
                    cue.acceptedApplicationEventId)
                || playedCardTriggerEventIds.Contains(
                    cue.acceptedApplicationEventId))
            {
                return;
            }

            var presentationCarrier =
                resolved.Binding.presentationCarrier;
            if (presentationCarrier == null)
            {
                return;
            }

            presentationCarrier.PlayTrigger(Time.unscaledTime);
            playedCardTriggerEventIds.Add(
                cue.acceptedApplicationEventId);
        }

        private bool TryQueuePendingCausalFollowup(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null
                || string.IsNullOrWhiteSpace(
                    cue.acceptedApplicationEventId))
            {
                return false;
            }

            string targetKey = BuildCausalPendingTargetKey(cue);
            if (!causalArrivalByTarget.TryGetValue(
                    targetKey,
                    out float releaseAt))
            {
                return false;
            }

            EnqueuePendingCausalCue(cue, targetKey, releaseAt);
            return true;
        }

        private void EnqueuePendingCausalCue(
            C1FormalRealtimeBattleCue cue,
            string targetKey,
            float releaseAt)
        {
            if (cue == null || string.IsNullOrEmpty(targetKey))
            {
                return;
            }

            if (pendingCausalCues.Count >= MaxPendingCausalCueCount)
            {
                PendingCausalCue oldest = pendingCausalCues[0];
                pendingCausalCues.RemoveAt(0);
                ReportCausalDiagnosticOnce(
                    "PENDING_QUEUE_CAPACITY_RELEASE",
                    oldest.Cue);
                ReleasePendingCausalCue(oldest);
                if (!pendingCausalCues.Any(value => string.Equals(
                        value.TargetKey,
                        oldest.TargetKey,
                        StringComparison.Ordinal)))
                {
                    causalArrivalByTarget.Remove(oldest.TargetKey);
                }
                if (string.Equals(
                        oldest.TargetKey,
                        targetKey,
                        StringComparison.Ordinal))
                {
                    causalArrivalByTarget[targetKey] = releaseAt;
                }
            }

            PendingCausalCue pending = new PendingCausalCue(
                cue,
                targetKey,
                releaseAt);
            int insertAt = pendingCausalCues.FindIndex(value =>
                value.Cue.sequence > cue.sequence);
            if (insertAt < 0)
            {
                pendingCausalCues.Add(pending);
            }
            else
            {
                pendingCausalCues.Insert(insertAt, pending);
            }
        }

        private void ReleaseReadyPendingCues(float now)
        {
            while (pendingCausalCues.Count > 0)
            {
                PendingCausalCue pending = pendingCausalCues[0];
                if (pending.ReleaseAt > now)
                {
                    return;
                }
                pendingCausalCues.RemoveAt(0);

                if (!PendingTargetIsStillValid(pending.Cue))
                {
                    ReportCausalDiagnosticOnce(
                        "PENDING_TARGET_INVALIDATED",
                        pending.Cue);
                    causalSourceVisualView?.CancelAcceptedEvent(
                        pending.Cue.acceptedApplicationEventId);
                    pendingCausalCues.RemoveAll(value => string.Equals(
                        value.TargetKey,
                        pending.TargetKey,
                        StringComparison.Ordinal));
                    causalArrivalByTarget.Remove(pending.TargetKey);
                    continue;
                }

                ReleasePendingCausalCue(pending);
                if (!pendingCausalCues.Any(value => string.Equals(
                        value.TargetKey,
                        pending.TargetKey,
                        StringComparison.Ordinal)))
                {
                    causalArrivalByTarget.Remove(pending.TargetKey);
                }
            }
        }

        private void ReleasePendingCausalCue(PendingCausalCue pending)
        {
            C1FormalRealtimeBattleCue cue = pending?.Cue;
            if (cue == null)
            {
                return;
            }

            switch (cue.cueKind)
            {
                case C1FormalRealtimeBattleCueKinds.HitAccepted:
                    ApplyHitFeedback(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.ActorHpChanged:
                    ApplyActorHpChanged(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.DamageFloatPayload:
                    ApplyDamageFloat(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.NianResourceChanged:
                    ApplyPeriodicNianGainArrival(cue);
                    break;
                case C1FormalRealtimeBattleCueKinds.ActorDefeated:
                    ApplyActorDefeated(cue);
                    break;
            }
        }

        private bool PendingTargetIsStillValid(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null)
            {
                return false;
            }
            if (string.Equals(
                    cue.targetActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    StringComparison.Ordinal)
                && cue.targetStableOrder == -1)
            {
                if (string.Equals(
                        cue.resultKind,
                        C1FormalRealtimeBattleFeedbackResultKinds.NianGain,
                        StringComparison.Ordinal))
                {
                    return playerView != null
                           && playerView.NianAnchor != null;
                }
                return playerView != null
                       && playerView.DamageAnchor != null;
            }
            return TryGetExactEnemySlot(
                cue.targetActorId,
                cue.targetStableOrder,
                out FormalBattleEnemySlotView slot)
                   && slot.DamageAnchor != null;
        }

        private void ApplyActorHpChanged(C1FormalRealtimeBattleCue cue)
        {
            if (!TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView hpSlot))
            {
                return;
            }
            C1FormalRealtimeBattleActorSnapshot hpActor = FindExactActor(
                cue.targetActorId,
                cue.targetStableOrder);
            hpSlot.ApplyHp(
                cue.actorHpAfter,
                hpActor == null ? 1 : hpActor.maxHp);
            if (selectedEnemyHud != null
                && string.Equals(
                    selectedEnemyHud.ActorBalanceId,
                    cue.targetActorId,
                    StringComparison.Ordinal)
                && selectedEnemyHud.StableActorOrder
                    == cue.targetStableOrder)
            {
                selectedEnemyHud.ApplyHp(
                    cue.actorHpAfter,
                    hpActor == null ? 1 : hpActor.maxHp);
            }
        }

        private void ApplyDamageFloat(C1FormalRealtimeBattleCue cue)
        {
            ApplySemanticFeedback(cue);
        }

        private void ApplyApplicationFeedback(
            C1FormalRealtimeBattleCue cue)
        {
            ApplySemanticFeedback(cue);
        }

        private void ApplySemanticFeedback(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null
                || string.IsNullOrWhiteSpace(cue.resultKind)
                || !TryResolveFeedbackReceiver(
                    cue,
                    out RectTransform receiver))
            {
                return;
            }

            ApplySemanticFeedbackAt(cue, receiver);
        }

        private void ApplySemanticFeedbackAt(
            C1FormalRealtimeBattleCue cue,
            RectTransform receiver)
        {
            if (cue == null
                || receiver == null
                || string.IsNullOrWhiteSpace(cue.resultKind))
            {
                return;
            }

            string feedbackKey = BuildFeedbackVisualKey(cue);
            if (shownFeedbackApplicationKeys.Contains(feedbackKey))
            {
                return;
            }

            string payload = BuildFeedbackPayload(cue);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return;
            }

            string styleKey = ResolveFeedbackStyleKey(cue.resultKind);
            if (damageFloatPool.Show(
                    payload,
                    receiver,
                    styleKey,
                    cue.triggerKind,
                    cue.deliveryKind,
                    cue.resultKind))
            {
                shownFeedbackApplicationKeys.Add(feedbackKey);
            }
        }

        private void ApplyNianResourceChanged(
            C1FormalRealtimeBattleCue cue)
        {
            if (IsPeriodicNianGainCue(cue)
                && TryPlayPeriodicNianGainCausalFeedback(cue))
            {
                return;
            }

            ApplySemanticFeedback(cue);
        }

        private bool TryPlayPeriodicNianGainCausalFeedback(
            C1FormalRealtimeBattleCue cue)
        {
            if (!TryResolveExactItemSourceBinding(
                    cue.sourceItemInstanceId,
                    cue.sourceBaseItemId,
                    out C1Pool15FormalItemSourceAnchorBinding binding,
                    out string resolutionDiagnostic))
            {
                ReportCausalDiagnosticOnce(resolutionDiagnostic, cue);
                return false;
            }
            if (profile == null
                || !profile.TryGetCausalItemStyle(
                    FormalBattlePresentationProfile
                        .NianResourceGainGrammarKey,
                    out FormalBattleCausalItemStyle style))
            {
                ReportCausalDiagnosticOnce(
                    "NIAN_GAIN_VISUAL_STYLE_MISSING",
                    cue);
                return false;
            }
            if (playerView == null || playerView.NianAnchor == null)
            {
                ReportCausalDiagnosticOnce(
                    "PLAYER_NIAN_RECEIVER_MISSING",
                    cue);
                return false;
            }

            ResolvedCausalSource resolved = new ResolvedCausalSource(
                binding,
                style,
                style.GrammarKey);
            PlayCardTriggerOnce(cue, resolved);
            causalSourceVisualView?.CancelAcceptedEvent(
                cue.acceptedApplicationEventId);
            RectTransform sourceAnchor = binding.sourceAnchor;
            bool begun = causalSourceVisualView != null
                         && causalSourceVisualView.BeginAccepted(
                             cue.acceptedApplicationEventId,
                             resolved.GrammarKey,
                             binding.sourceAnchor,
                             style,
                             binding.artwork,
                             out sourceAnchor);
            if (!begun)
            {
                sourceAnchor = binding.sourceAnchor;
                ReportCausalDiagnosticOnce(
                    "SOURCE_LOCAL_CARRIER_SKIPPED",
                    cue);
            }
            if (!cueFxAudioRoot.PlaySourcePulse(
                    sourceAnchor,
                    false,
                    style))
            {
                ReportCausalDiagnosticOnce(
                    "FORMAL_SOURCE_PULSE_UNAVAILABLE",
                    cue);
            }

            string targetKey = BuildCausalPendingTargetKey(cue);
            if (!cueFxAudioRoot.PlayCausalBridge(
                    cue.acceptedApplicationEventId,
                    targetKey,
                    style,
                    sourceAnchor,
                    playerView.NianAnchor))
            {
                ReportCausalDiagnosticOnce(
                    "RIBBON_CARRIER_UNAVAILABLE",
                    cue);
                ApplyPeriodicNianGainArrival(cue);
                return true;
            }

            float arrivalDelay = Mathf.Clamp(
                profile.CausalRibbonDuration * 0.5f,
                0.1f,
                0.25f);
            float releaseAt = Time.unscaledTime + arrivalDelay;
            causalArrivalByTarget[targetKey] = releaseAt;
            EnqueuePendingCausalCue(cue, targetKey, releaseAt);
            return true;
        }

        private void ApplyPeriodicNianGainArrival(
            C1FormalRealtimeBattleCue cue)
        {
            playerView?.PlayNianReceive();
            if (playerView?.NianAnchor != null)
            {
                cueFxAudioRoot?.PlayImpact(playerView.NianAnchor);
            }
            ApplySemanticFeedbackAt(cue, playerView?.NianAnchor);
        }

        private void ApplyPeriodicNianPresentation(
            C1FormalRealtimeBattleSessionStateSnapshot state)
        {
            C1FormalRealtimeBattleNianStateSnapshot nian =
                state?.nianStateSnapshot;
            if (nian == null
                || nian.generationIntervalMilliseconds <= 0L
                || nian.nextGenerationAtBattleTimeMs <= 0L
                || string.IsNullOrWhiteSpace(nian.sourceItemInstanceId)
                || string.IsNullOrWhiteSpace(nian.sourceBaseItemId)
                || !TryResolveExactItemSourceBinding(
                    nian.sourceItemInstanceId,
                    nian.sourceBaseItemId,
                    out C1Pool15FormalItemSourceAnchorBinding binding,
                    out _)
                || binding.presentationCarrier == null)
            {
                ClearPeriodicNianPresentation();
                return;
            }

            ItemRarityContourBloomVfx carrier =
                binding.presentationCarrier;
            if (activePeriodicNianCarrier != carrier
                || !string.Equals(
                    activePeriodicNianSourceItemInstanceId,
                    nian.sourceItemInstanceId,
                    StringComparison.Ordinal))
            {
                ClearPeriodicNianPresentation();
                activePeriodicNianCarrier = carrier;
                activePeriodicNianSourceItemInstanceId =
                    nian.sourceItemInstanceId;
            }

            long remaining = Math.Max(
                0L,
                nian.nextGenerationAtBattleTimeMs - state.battleTimeMs);
            float progress = 1f - Mathf.Clamp01(
                remaining
                / (float)nian.generationIntervalMilliseconds);
            activePeriodicNianCarrier.SetPeriodicChargeProgress(progress);
        }

        private void ClearPeriodicNianPresentation()
        {
            activePeriodicNianCarrier?.ClearPeriodicCharge();
            activePeriodicNianCarrier = null;
            activePeriodicNianSourceItemInstanceId = string.Empty;
        }

        private static bool IsPeriodicNianGainCue(
            C1FormalRealtimeBattleCue cue)
        {
            return cue != null
                   && cue.resourceAppliedAmount > 0
                   && string.Equals(
                       cue.cueKind,
                       C1FormalRealtimeBattleCueKinds.NianResourceChanged,
                       StringComparison.Ordinal)
                   && string.Equals(
                       cue.triggerKind,
                       C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
                       StringComparison.Ordinal)
                   && string.Equals(
                       cue.deliveryKind,
                       C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
                       StringComparison.Ordinal)
                   && string.Equals(
                       cue.resultKind,
                       C1FormalRealtimeBattleFeedbackResultKinds.NianGain,
                       StringComparison.Ordinal)
                   && string.Equals(
                       cue.targetActorId,
                       C1FormalRealtimeBattleSessionContract.PlayerActorId,
                       StringComparison.Ordinal)
                   && cue.targetStableOrder == -1
                   && !string.IsNullOrWhiteSpace(
                       cue.acceptedApplicationEventId);
        }

        private string BuildFeedbackPayload(
            C1FormalRealtimeBattleCue cue)
        {
            int applied = ResolveFeedbackAppliedAmount(cue);
            switch (cue.resultKind)
            {
                case C1FormalRealtimeBattleFeedbackResultKinds.HpDamage:
                    return applied > 0 ? "-" + applied : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage:
                    return applied > 0
                        ? "破壳 -" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage:
                    return applied > 0
                        ? "护势 -" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.Heal:
                    return applied > 0
                        ? "回复 +" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardGain:
                    return applied > 0
                        ? "护势 +" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellGain:
                    return applied > 0
                        ? "护壳 +" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffApply:
                    return "获得 " + ResolveStatusDisplayName(cue, "增益");
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRefresh:
                    return ResolveStatusDisplayName(cue, "增益") + " 刷新";
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRemove:
                    return ResolveStatusDisplayName(cue, "增益") + " 结束";
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply:
                    return "受到 " + ResolveStatusDisplayName(cue, "减益");
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRefresh:
                    return ResolveStatusDisplayName(cue, "减益") + " 刷新";
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRemove:
                    return ResolveStatusDisplayName(cue, "减益") + " 消退";
                case C1FormalRealtimeBattleFeedbackResultKinds.Control:
                    return applied > 0
                        ? "迟滞 +" + applied
                        : "控制";
                case C1FormalRealtimeBattleFeedbackResultKinds.Cleanse:
                    return applied > 0 ? "净化 " + applied : "净化";
                case C1FormalRealtimeBattleFeedbackResultKinds.NianGain:
                    return applied > 0
                        ? "念力 +" + applied
                        : string.Empty;
                case C1FormalRealtimeBattleFeedbackResultKinds.NianSpend:
                    return applied > 0
                        ? "念力 -" + applied
                        : string.Empty;
                default:
                    return string.Empty;
            }
        }

        private string ResolveStatusDisplayName(
            C1FormalRealtimeBattleCue cue,
            string fallback)
        {
            return profile != null
                   && profile.TryGetStatusVisualStyle(
                       cue.effectVariantId,
                       cue.effectFamilyId,
                       out FormalBattleStatusVisualStyle style)
                ? style.DisplayName
                : fallback;
        }

        private static int ResolveFeedbackAppliedAmount(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null)
            {
                return 0;
            }
            if (cue.resourceAppliedAmount != 0)
            {
                return Mathf.Abs(cue.resourceAppliedAmount);
            }
            if (cue.shellAppliedAmount != 0)
            {
                return Mathf.Abs(cue.shellAppliedAmount);
            }
            return Mathf.Abs(cue.appliedDamage);
        }

        private static string ResolveFeedbackStyleKey(string resultKind)
        {
            switch (resultKind)
            {
                case C1FormalRealtimeBattleFeedbackResultKinds.Heal:
                    return FormalBattleDamageFloatStyleKeys.Heal;
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardGain:
                    return FormalBattleDamageFloatStyleKeys.Guard;
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage:
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellGain:
                    return FormalBattleDamageFloatStyleKeys.Shell;
                case C1FormalRealtimeBattleFeedbackResultKinds.NianGain:
                case C1FormalRealtimeBattleFeedbackResultKinds.NianSpend:
                    return FormalBattleDamageFloatStyleKeys.Nian;
                case C1FormalRealtimeBattleFeedbackResultKinds.Cleanse:
                    return FormalBattleDamageFloatStyleKeys.Cleanse;
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRemove:
                    return FormalBattleDamageFloatStyleKeys.Guard;
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds
                    .DebuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRemove:
                case C1FormalRealtimeBattleFeedbackResultKinds.Control:
                    return FormalBattleDamageFloatStyleKeys.Control;
                default:
                    return FormalBattleDamageFloatStyleKeys.Damage;
            }
        }

        private static string BuildFeedbackVisualKey(
            C1FormalRealtimeBattleCue cue)
        {
            if (string.IsNullOrWhiteSpace(
                    cue.acceptedApplicationEventId))
            {
                return cue.cueId ?? string.Empty;
            }
            return string.Join("|", new[]
            {
                cue.acceptedApplicationEventId,
                cue.resultKind,
                cue.sourceItemInstanceId,
                cue.sourceId,
                cue.targetActorId,
                cue.targetStableOrder.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        private void ApplyActorDefeated(C1FormalRealtimeBattleCue cue)
        {
            if (!TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView deathSlot))
            {
                return;
            }
            deathSlot.PlayDeath();
            scheduledEnemyCarriers.Remove(cue.targetActorId);
            if (string.Equals(
                    currentTargetActorId,
                    cue.targetActorId,
                    StringComparison.Ordinal)
                && currentTargetStableOrder == cue.targetStableOrder)
            {
                ClearTarget();
            }
        }

        private static bool IsPlayerOwnedItemHit(
            C1FormalRealtimeBattleCue cue)
        {
            return cue != null
                   && cue.cueKind
                   == C1FormalRealtimeBattleCueKinds.HitAccepted
                   && cue.appliedDamage > 0
                   && !string.IsNullOrWhiteSpace(
                       cue.acceptedApplicationEventId)
                   && string.Equals(
                       cue.sourceOwner,
                       C1FormalRealtimeBattleSessionContract.PlayerOwner,
                       StringComparison.Ordinal);
        }

        private static string BuildCausalPendingTargetKey(
            C1FormalRealtimeBattleCue cue)
        {
            return string.Join("#", new[]
            {
                cue?.acceptedApplicationEventId ?? string.Empty,
                cue?.targetActorId ?? string.Empty,
                (cue?.targetStableOrder ?? -1).ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        private bool TryResolveReceiver(
            C1FormalRealtimeBattleCue cue,
            out RectTransform receiver)
        {
            receiver = null;
            if (string.Equals(
                    cue.targetActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    StringComparison.Ordinal)
                && cue.targetStableOrder == -1)
            {
                receiver = playerView.DamageAnchor;
                return receiver != null;
            }
            if (TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView slot))
            {
                receiver = slot.DamageAnchor;
                return receiver != null;
            }
            return false;
        }

        private bool TryResolveFeedbackReceiver(
            C1FormalRealtimeBattleCue cue,
            out RectTransform receiver)
        {
            receiver = null;
            if (string.Equals(
                    cue.targetActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    StringComparison.Ordinal)
                && cue.targetStableOrder == -1)
            {
                receiver = playerView.FeedbackReceiverLanes?.Resolve(
                    cue.resultKind);
                return receiver != null;
            }
            if (TryGetExactEnemySlot(
                    cue.targetActorId,
                    cue.targetStableOrder,
                    out FormalBattleEnemySlotView slot))
            {
                receiver = slot.FeedbackReceiverLanes?.Resolve(
                    cue.resultKind);
                return receiver != null;
            }
            return false;
        }

        private bool TryResolveCausalSource(
            C1FormalRealtimeBattleCue cue,
            out ResolvedCausalSource resolved,
            out string diagnostic)
        {
            resolved = null;
            if (!TryResolveExactItemSourceBinding(
                    cue?.sourceItemInstanceId,
                    cue?.sourceBaseItemId,
                    out C1Pool15FormalItemSourceAnchorBinding binding,
                    out diagnostic)
                || profile == null)
            {
                return false;
            }
            if (!string.IsNullOrWhiteSpace(cue.effectFamilyId)
                && !string.Equals(
                    binding.effectFamilyKey,
                    cue.effectFamilyId,
                    StringComparison.Ordinal))
            {
                diagnostic = "SOURCE_EFFECT_FAMILY_MISMATCH";
                return false;
            }
            if (!profile.TryResolveCausalGrammar(
                    binding.presentationStyleKey,
                    binding.effectFamilyKey,
                    binding.cueIdentity,
                    cue.cueKind,
                    cue.effectVariantId,
                    out string grammarKey,
                    out FormalBattleCausalItemStyle style))
            {
                diagnostic = "SOURCE_GRAMMAR_MISSING_OR_AMBIGUOUS";
                return false;
            }

            resolved = new ResolvedCausalSource(
                binding,
                style,
                grammarKey);
            diagnostic = string.Empty;
            return true;
        }

        private bool TryResolveExactItemSourceBinding(
            string sourceItemInstanceId,
            string sourceBaseItemId,
            out C1Pool15FormalItemSourceAnchorBinding binding,
            out string diagnostic)
        {
            binding = null;
            diagnostic = "SOURCE_PROVIDER_UNBOUND";
            if (itemSourceProvider == null
                || itemPresentationCatalog == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(sourceItemInstanceId))
            {
                diagnostic = "SOURCE_ITEM_INSTANCE_ID_MISSING";
                return false;
            }
            if (string.IsNullOrWhiteSpace(sourceBaseItemId))
            {
                diagnostic = "SOURCE_BASE_ITEM_ID_MISSING";
                return false;
            }

            if (!itemSourceProvider
                    .TryResolveCurrentPlacedLitPresentationSource(
                        sourceItemInstanceId,
                        sourceBaseItemId,
                        out binding)
                || binding == null)
            {
                binding = null;
                diagnostic = "SOURCE_BINDING_REJECTED";
                return false;
            }
            if (!string.Equals(
                    binding.itemInstanceId,
                    sourceItemInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    binding.baseItemId,
                    sourceBaseItemId,
                    StringComparison.Ordinal)
                || binding.sourceAnchor == null
                || !binding.sourceAnchor.gameObject.activeInHierarchy
                || binding.artwork == null
                || string.IsNullOrWhiteSpace(binding.artworkIdentity)
                || string.IsNullOrWhiteSpace(binding.presentationStyleKey)
                || string.IsNullOrWhiteSpace(binding.effectFamilyKey)
                || string.IsNullOrWhiteSpace(binding.cueIdentity)
                || binding.presentationCarrier == null)
            {
                binding = null;
                diagnostic = "SOURCE_BINDING_IDENTITY_MISMATCH";
                return false;
            }

            diagnostic = string.Empty;
            return true;
        }

        private static bool ValidateItemSourceProvider(
            C1ExactBattleSandboxItemArrangementPresenter provider,
            C1FormalItemPresentationCatalogSnapshot catalog)
        {
            return provider != null
                   && provider.IsBound
                   && provider.Current != null
                   && catalog != null
                   && string.Equals(
                       catalog.schemaId,
                       C1FormalItemPresentationCatalogSnapshot.SchemaId,
                       StringComparison.Ordinal)
                   && catalog.schemaVersion
                    == C1FormalItemPresentationCatalogSnapshot.SchemaVersion
                   && !string.IsNullOrWhiteSpace(
                       catalog.canonicalSignature)
                   && catalog.Rows != null
                   && catalog.Rows.Count > 0
                   && catalog.Rows.All(value => value != null
                       && !string.IsNullOrWhiteSpace(value.baseItemId)
                       && !string.IsNullOrWhiteSpace(value.artworkIdentity)
                       && !string.IsNullOrWhiteSpace(value.effectFamilyKey)
                       && !string.IsNullOrWhiteSpace(
                           value.presentationStyleKey)
                       && !string.IsNullOrWhiteSpace(value.cueIdentity)
                       && !string.IsNullOrWhiteSpace(
                           value.rowCanonicalSignature))
                   && catalog.Rows.Select(value =>
                           value.rarityVersionIdentity)
                       .Distinct(StringComparer.Ordinal).Count()
                    == catalog.Rows.Count;
        }

        private void ReportCausalDiagnosticOnce(
            string code,
            C1FormalRealtimeBattleCue cue)
        {
            string key = string.Join("|", new[]
            {
                code ?? string.Empty,
                cue?.cueId ?? string.Empty
            });
            if (!reportedCausalDiagnostics.Add(key))
            {
                return;
            }
            Debug.LogWarning(
                "[FormalBattlePresentation] CAUSAL_VISUAL_SKIPPED "
                + (code ?? "UNKNOWN")
                + " cue="
                + (cue?.cueId ?? string.Empty),
                this);
        }

        private C1FormalRealtimeBattleActorSnapshot FindExactActor(
            string actorId,
            int stableOrder)
        {
            if (currentState?.actorSnapshots == null)
            {
                return null;
            }
            return currentState.actorSnapshots.SingleOrDefault(value =>
                value != null
                && value.stableActorOrder == stableOrder
                && string.Equals(
                    value.actorBalanceId,
                    actorId,
                    StringComparison.Ordinal));
        }

        private bool TryGetExactEnemySlot(
            string actorId,
            int stableOrder,
            out FormalBattleEnemySlotView slot)
        {
            slot = enemySlots.SingleOrDefault(value =>
                value != null
                && value.AuthoredStableOrder == stableOrder
                && value.Bound
                && string.Equals(
                    value.ActorBalanceId,
                    actorId,
                    StringComparison.Ordinal));
            return slot != null;
        }

        private bool TryGetEnemySlotBySourceId(
            string sourceActorId,
            out FormalBattleEnemySlotView slot)
        {
            slot = enemySlots.SingleOrDefault(value =>
                value != null
                && value.Bound
                && string.Equals(
                    value.ActorBalanceId,
                    sourceActorId,
                    StringComparison.Ordinal));
            return slot != null;
        }

        private void ClearTarget()
        {
            currentTargetActorId = string.Empty;
            currentTargetStableOrder = -1;
            selectedEnemyHud?.Clear();
            foreach (FormalBattleEnemySlotView slot in enemySlots)
            {
                slot?.SetTarget(false);
            }
        }

        private void ClearTransientsAndTarget()
        {
            ClearTarget();
            scheduledEnemyCarriers.Clear();
            shownFeedbackApplicationKeys.Clear();
            ClearCausalSourceState();
            damageFloatPool.Clear();
        }

        private void ClearCausalSourceState()
        {
            ClearPeriodicNianPresentation();
            playedCardTriggerEventIds.Clear();
            pendingCausalCues.Clear();
            causalArrivalByTarget.Clear();
            pendingPausedAt = 0f;
            cueFxAudioRoot?.Clear();
            causalSourceVisualView?.Clear();
            profile?.ClearTransientCausalArtwork();
        }

        private void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }
            paused = value;
            if (paused)
            {
                pendingPausedAt = Time.unscaledTime;
            }
            else
            {
                float shift = Mathf.Max(
                    0f,
                    Time.unscaledTime - pendingPausedAt);
                foreach (PendingCausalCue pending in pendingCausalCues)
                {
                    pending?.ShiftClock(shift);
                }
                foreach (string targetKey
                         in causalArrivalByTarget.Keys.ToArray())
                {
                    causalArrivalByTarget[targetKey] += shift;
                }
                pendingPausedAt = 0f;
            }
            playerView.SetPaused(value);
            foreach (FormalBattleEnemySlotView slot in enemySlots)
            {
                slot?.SetPaused(value);
            }
            damageFloatPool.SetPaused(value);
            cueFxAudioRoot.SetPaused(value);
            causalSourceVisualView?.SetPaused(value);
        }

        private static string BuildAcceptedApplicationRoleKey(
            C1FormalRealtimeBattleCue cue)
        {
            if (cue == null
                || string.IsNullOrEmpty(cue.acceptedApplicationEventId))
            {
                return string.Empty;
            }
            return string.Join("|", new[]
            {
                cue.acceptedApplicationEventId,
                cue.cueKind,
                cue.sourceItemInstanceId,
                cue.sourceBaseItemId,
                cue.effectFamilyId,
                cue.effectVariantId,
                cue.targetActorId,
                cue.targetStableOrder.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            });
        }

#if UNITY_EDITOR
        public void AssignSourceCarrierForEditor(
            FormalBattlePresentationProfile configuredProfile,
            FormalBattlePlayerPresentationView configuredPlayerView,
            FormalBattleEnemySlotView[] configuredEnemySlots,
            FormalBattleCausalSourceVisualView
                configuredCausalSourceVisualView,
            TMP_Text configuredStageLabel)
        {
            AssignSourceCarrierForEditor(
                configuredProfile,
                configuredPlayerView,
                configuredEnemySlots,
                selectedEnemyHud,
                configuredCausalSourceVisualView,
                configuredStageLabel);
        }

        public void AssignSourceCarrierForEditor(
            FormalBattlePresentationProfile configuredProfile,
            FormalBattlePlayerPresentationView configuredPlayerView,
            FormalBattleEnemySlotView[] configuredEnemySlots,
            FormalBattleSelectedEnemyHudView configuredSelectedEnemyHud,
            FormalBattleCausalSourceVisualView
                configuredCausalSourceVisualView,
            TMP_Text configuredStageLabel)
        {
            profile = configuredProfile;
            playerView = configuredPlayerView;
            enemySlots = configuredEnemySlots
                ?? Array.Empty<FormalBattleEnemySlotView>();
            selectedEnemyHud = configuredSelectedEnemyHud;
            causalSourceVisualView = configuredCausalSourceVisualView;
            stageLabel = configuredStageLabel;
        }

        public void AssignExternalCompositionForEditor(
            FormalBattleDamageFloatPool configuredDamageFloatPool,
            FormalBattleCueFxAudioRoot configuredCueFxAudioRoot)
        {
            damageFloatPool = configuredDamageFloatPool;
            cueFxAudioRoot = configuredCueFxAudioRoot;
        }

        [Obsolete(
            "Item source cards are runtime-provider owned; extra Editor "
            + "composition arguments are ignored.")]
        public void AssignExternalCompositionForEditor(
            FormalBattleDamageFloatPool configuredDamageFloatPool,
            FormalBattleCueFxAudioRoot configuredCueFxAudioRoot,
            params UnityEngine.Object[] retiredCompositionArguments)
        {
            AssignExternalCompositionForEditor(
                configuredDamageFloatPool,
                configuredCueFxAudioRoot);
        }

        public void AssignForEditor(
            FormalBattlePresentationProfile configuredProfile,
            FormalBattlePlayerPresentationView configuredPlayerView,
            FormalBattleEnemySlotView[] configuredEnemySlots,
            FormalBattleDamageFloatPool configuredDamageFloatPool,
            FormalBattleCueFxAudioRoot configuredCueFxAudioRoot,
            FormalBattleCausalSourceVisualView
                configuredCausalSourceVisualView,
            TMP_Text configuredStageLabel)
        {
            AssignForEditor(
                configuredProfile,
                configuredPlayerView,
                configuredEnemySlots,
                selectedEnemyHud,
                configuredDamageFloatPool,
                configuredCueFxAudioRoot,
                configuredCausalSourceVisualView,
                configuredStageLabel);
        }

        public void AssignForEditor(
            FormalBattlePresentationProfile configuredProfile,
            FormalBattlePlayerPresentationView configuredPlayerView,
            FormalBattleEnemySlotView[] configuredEnemySlots,
            FormalBattleSelectedEnemyHudView configuredSelectedEnemyHud,
            FormalBattleDamageFloatPool configuredDamageFloatPool,
            FormalBattleCueFxAudioRoot configuredCueFxAudioRoot,
            FormalBattleCausalSourceVisualView
                configuredCausalSourceVisualView,
            TMP_Text configuredStageLabel)
        {
            AssignSourceCarrierForEditor(
                configuredProfile,
                configuredPlayerView,
                configuredEnemySlots,
                configuredSelectedEnemyHud,
                configuredCausalSourceVisualView,
                configuredStageLabel);
            AssignExternalCompositionForEditor(
                configuredDamageFloatPool,
                configuredCueFxAudioRoot);
        }
#endif
    }
}
