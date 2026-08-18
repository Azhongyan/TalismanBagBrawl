using System;
using System.Collections.Generic;
using TMPro;
using TalismanBag.Contracts.Battle;
using UnityEngine;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleDamageFloatPool : MonoBehaviour
    {
        private const int MaxPendingFloatCount = 32;

        private sealed class PendingFloat
        {
            public PendingFloat(
                string payload,
                RectTransform receiver,
                string visualStyleKey,
                string triggerKind,
                string deliveryKind,
                string resultKind)
            {
                Payload = payload;
                Receiver = receiver;
                VisualStyleKey = visualStyleKey ?? string.Empty;
                TriggerKind = triggerKind ?? string.Empty;
                DeliveryKind = deliveryKind ?? string.Empty;
                ResultKind = resultKind ?? string.Empty;
            }

            public string Payload { get; }
            public RectTransform Receiver { get; }
            public string VisualStyleKey { get; }
            public string TriggerKind { get; }
            public string DeliveryKind { get; }
            public string ResultKind { get; }
        }

        [SerializeField] private RectTransform poolRect;
        [SerializeField] private FormalBattlePresentationProfile profile;
        [SerializeField] private FormalBattleFeedbackVisualModule[]
            authoredModules =
                Array.Empty<FormalBattleFeedbackVisualModule>();
        [Header("Authorable motion")]
        [SerializeField] private Vector2[] receiverLaneOffsets =
        {
            Vector2.zero,
            new Vector2(-32f, 12f),
            new Vector2(32f, 24f),
            new Vector2(-20f, 36f),
            new Vector2(20f, 48f),
            new Vector2(-38f, 60f),
            new Vector2(0f, 72f),
            new Vector2(38f, 84f),
            new Vector2(-18f, 96f),
            new Vector2(18f, 108f)
        };
        [SerializeField, Min(0.1f)] private float durationSeconds = 0.82f;
        [SerializeField] private float riseDistance = 76f;
        [SerializeField, Range(0f, 1f)] private float fadeStartNormalized =
            0.68f;
        [SerializeField, Min(0f)] private float sameReceiverDispatchInterval =
            0.08f;

        private int replacementCursor;
        private bool paused;
        private float pausedAt;
        private readonly List<PendingFloat> pendingFloats =
            new List<PendingFloat>();
        private readonly Dictionary<int, float> nextDispatchAtByReceiver =
            new Dictionary<int, float>();
        private readonly Dictionary<int, int> nextLaneByReceiver =
            new Dictionary<int, int>();

#if UNITY_EDITOR
        [SerializeField] private bool showAuthoringPreview;

        private static readonly string[] AuthoringPreviewLabels =
        {
            "-82",
            "+24",
            "护盾 -18",
            "余焰 -1",
            "破壳 -11",
            "念力 +1",
            "格挡 4",
            "暴击 -126",
            "净化",
            "回复 +16"
        };

        private static readonly string[] AuthoringPreviewStyleKeys =
        {
            FormalBattleDamageFloatStyleKeys.Damage,
            FormalBattleDamageFloatStyleKeys.Heal,
            FormalBattleDamageFloatStyleKeys.Guard,
            FormalBattleDamageFloatStyleKeys.Damage,
            FormalBattleDamageFloatStyleKeys.Shell,
            FormalBattleDamageFloatStyleKeys.Nian,
            FormalBattleDamageFloatStyleKeys.Guard,
            FormalBattleDamageFloatStyleKeys.Damage,
            FormalBattleDamageFloatStyleKeys.Cleanse,
            FormalBattleDamageFloatStyleKeys.Heal
        };

        private static readonly string[] AuthoringPreviewTriggerKinds =
        {
            C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction,
            C1FormalRealtimeBattleFeedbackTriggerKinds.Skill,
            C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
            C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger,
            C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
            C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
            C1FormalRealtimeBattleFeedbackTriggerKinds.Passive,
            C1FormalRealtimeBattleFeedbackTriggerKinds.Skill,
            C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger,
            C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger
        };

        private static readonly string[] AuthoringPreviewDeliveryKinds =
        {
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Delayed,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant,
            C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic
        };

        private static readonly string[] AuthoringPreviewResultKinds =
        {
            C1FormalRealtimeBattleFeedbackResultKinds.HpDamage,
            C1FormalRealtimeBattleFeedbackResultKinds.Heal,
            C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage,
            C1FormalRealtimeBattleFeedbackResultKinds.HpDamage,
            C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage,
            C1FormalRealtimeBattleFeedbackResultKinds.NianGain,
            C1FormalRealtimeBattleFeedbackResultKinds.GuardGain,
            C1FormalRealtimeBattleFeedbackResultKinds.HpDamage,
            C1FormalRealtimeBattleFeedbackResultKinds.Cleanse,
            C1FormalRealtimeBattleFeedbackResultKinds.Heal
        };
#endif

        private void Awake()
        {
            Clear();
        }

        private void Update()
        {
            if (paused)
            {
                return;
            }

            float now = Time.unscaledTime;
            foreach (FormalBattleFeedbackVisualModule entry
                     in authoredModules)
            {
                entry?.Tick(now);
            }
            DrainPending(now);
        }

        public bool Show(
            string payload,
            RectTransform receiver,
            string visualStyleKey)
        {
            return Show(
                payload,
                receiver,
                visualStyleKey,
                string.Empty,
                string.Empty,
                string.Empty);
        }

        public bool Show(
            string payload,
            RectTransform receiver,
            string visualStyleKey,
            string triggerKind,
            string deliveryKind,
            string resultKind)
        {
            if (string.IsNullOrWhiteSpace(payload)
                || receiver == null
                || poolRect == null
                || profile == null
                || authoredModules == null
                || authoredModules.Length == 0
                || !profile.TryGetDamageFloatStyle(
                    visualStyleKey,
                    out _))
            {
                return false;
            }

            float now = Time.unscaledTime;
            if (!paused)
            {
                DrainPending(now);
            }
            if (pendingFloats.Count >= MaxPendingFloatCount)
            {
                return false;
            }

            pendingFloats.Add(new PendingFloat(
                payload,
                receiver,
                visualStyleKey,
                triggerKind,
                deliveryKind,
                resultKind));
            if (!paused)
            {
                DrainPending(now);
            }
            return true;
        }

        private void DrainPending(float now)
        {
            while (pendingFloats.Count > 0
                   && TryFindAvailableFloat(
                       out FormalBattleFeedbackVisualModule entry,
                       out int entryIndex))
            {
                bool dispatched = false;
                for (int index = 0; index < pendingFloats.Count; index++)
                {
                    PendingFloat pending = pendingFloats[index];
                    if (pending?.Receiver == null)
                    {
                        pendingFloats.RemoveAt(index--);
                        continue;
                    }

                    int receiverId = pending.Receiver.GetInstanceID();
                    if (nextDispatchAtByReceiver.TryGetValue(
                            receiverId,
                            out float nextDispatchAt)
                        && nextDispatchAt > now)
                    {
                        continue;
                    }

                    Vector2 screenPoint =
                        RectTransformUtility.WorldToScreenPoint(
                            null,
                            pending.Receiver.position);
                    if (!RectTransformUtility
                            .ScreenPointToLocalPointInRectangle(
                                poolRect,
                                screenPoint,
                                null,
                                out Vector2 localPoint))
                    {
                        pendingFloats.RemoveAt(index--);
                        continue;
                    }

                    int lane = nextLaneByReceiver.TryGetValue(
                        receiverId,
                        out int nextLane)
                        ? nextLane
                        : 0;
                    entry.Show(
                        pending.Payload,
                        localPoint + receiverLaneOffsets[
                            lane % receiverLaneOffsets.Length],
                        profile.DamageFont,
                        ResolvePendingVisualStyle(pending),
                        pending.TriggerKind,
                        pending.DeliveryKind,
                        pending.ResultKind,
                        durationSeconds,
                        riseDistance,
                        fadeStartNormalized,
                        now);
                    replacementCursor = (entryIndex + 1)
                                        % authoredModules.Length;
                    nextLaneByReceiver[receiverId] = (lane + 1)
                                                     % receiverLaneOffsets
                                                         .Length;
                    nextDispatchAtByReceiver[receiverId] = now
                        + sameReceiverDispatchInterval;
                    pendingFloats.RemoveAt(index);
                    dispatched = true;
                    break;
                }

                if (!dispatched)
                {
                    return;
                }
            }
        }

        private bool TryFindAvailableFloat(
            out FormalBattleFeedbackVisualModule entry,
            out int entryIndex)
        {
            entry = null;
            entryIndex = -1;
            for (int index = 0; index < authoredModules.Length; index++)
            {
                int candidateIndex = (replacementCursor + index)
                                     % authoredModules.Length;
                FormalBattleFeedbackVisualModule candidate =
                    authoredModules[candidateIndex];
                if (candidate != null && candidate.IsAvailable)
                {
                    entry = candidate;
                    entryIndex = candidateIndex;
                    return true;
                }
            }
            return false;
        }

        public void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }
            paused = value;
            if (paused)
            {
                pausedAt = Time.unscaledTime;
                return;
            }

            float shift = Mathf.Max(0f, Time.unscaledTime - pausedAt);
            foreach (FormalBattleFeedbackVisualModule entry
                     in authoredModules)
            {
                entry?.ShiftClock(shift);
            }
            foreach (int receiverId in new List<int>(
                         nextDispatchAtByReceiver.Keys))
            {
                nextDispatchAtByReceiver[receiverId] += shift;
            }
        }

        public void Clear()
        {
            paused = false;
            pausedAt = 0f;
            replacementCursor = 0;
            pendingFloats.Clear();
            nextDispatchAtByReceiver.Clear();
            nextLaneByReceiver.Clear();
            foreach (FormalBattleFeedbackVisualModule entry
                     in authoredModules)
            {
                entry?.Clear();
            }
        }

        public bool ValidateAuthoredReferences()
        {
            return poolRect != null
                   && profile != null
                   && profile.ValidateAuthoredReferences()
                   && authoredModules != null
                   && authoredModules.Length == 10
                   && Array.TrueForAll(
                       authoredModules,
                       value => value != null
                                && value.ValidateAuthoredReferences())
                   && receiverLaneOffsets != null
                   && receiverLaneOffsets.Length > 0
                   && durationSeconds >= 0.1f
                   && fadeStartNormalized >= 0f
                   && fadeStartNormalized < 1f
                   && sameReceiverDispatchInterval >= 0f;
        }

        private FormalBattleDamageFloatVisualStyle
            ResolvePendingVisualStyle(PendingFloat pending)
        {
            profile.TryGetDamageFloatStyle(
                pending.VisualStyleKey,
                out FormalBattleDamageFloatVisualStyle visualStyle);
            return visualStyle;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyAuthoringPreviewForEditor();
            }
        }

        public void SetAuthoringPreviewForEditor(bool visible)
        {
            showAuthoringPreview = visible;
            ApplyAuthoringPreviewForEditor();
        }

        private void ApplyAuthoringPreviewForEditor()
        {
            if (authoredModules == null || profile == null)
            {
                return;
            }

            for (int index = 0; index < authoredModules.Length; index++)
            {
                FormalBattleFeedbackVisualModule entry =
                    authoredModules[index];
                if (entry == null)
                {
                    continue;
                }

                if (showAuthoringPreview)
                {
                    int previewIndex = index
                                       % AuthoringPreviewLabels.Length;
                    if (profile.TryGetDamageFloatStyle(
                            AuthoringPreviewStyleKeys[previewIndex],
                            out FormalBattleDamageFloatVisualStyle style))
                    {
                        entry.SetAuthoringPreviewForEditor(
                            AuthoringPreviewLabels[previewIndex],
                            profile.DamageFont,
                            style,
                            AuthoringPreviewTriggerKinds[previewIndex],
                            AuthoringPreviewDeliveryKinds[previewIndex],
                            AuthoringPreviewResultKinds[previewIndex]);
                    }
                }
                else
                {
                    entry.Clear();
                }
            }
        }

        public void AssignForEditor(
            RectTransform configuredPoolRect,
            FormalBattlePresentationProfile configuredProfile,
            FormalBattleFeedbackVisualModule[] configuredModules)
        {
            poolRect = configuredPoolRect;
            profile = configuredProfile;
            authoredModules = configuredModules
                ?? Array.Empty<FormalBattleFeedbackVisualModule>();
        }

        public void AssignForEditor(
            RectTransform configuredPoolRect,
            FormalBattlePresentationProfile configuredProfile,
            RectTransform[] configuredRects,
            CanvasGroup[] configuredGroups,
            TMP_Text[] configuredLabels)
        {
            poolRect = configuredPoolRect;
            profile = configuredProfile;
            int count = Mathf.Min(
                configuredRects == null ? 0 : configuredRects.Length,
                Mathf.Min(
                    configuredGroups == null ? 0 : configuredGroups.Length,
                    configuredLabels == null ? 0 : configuredLabels.Length));
            authoredModules = new FormalBattleFeedbackVisualModule[count];
            for (int index = 0; index < count; index++)
            {
                authoredModules[index] = configuredRects[index] == null
                    ? null
                    : configuredRects[index]
                        .GetComponent<FormalBattleFeedbackVisualModule>();
            }
        }

        public void AssignMotionForEditor(
            Vector2[] configuredReceiverLaneOffsets,
            float configuredDurationSeconds,
            float configuredRiseDistance,
            float configuredFadeStartNormalized,
            float configuredSameReceiverDispatchInterval)
        {
            receiverLaneOffsets = configuredReceiverLaneOffsets
                ?? Array.Empty<Vector2>();
            durationSeconds = Mathf.Max(0.1f, configuredDurationSeconds);
            riseDistance = configuredRiseDistance;
            fadeStartNormalized = Mathf.Clamp(
                configuredFadeStartNormalized,
                0f,
                0.99f);
            sameReceiverDispatchInterval = Mathf.Max(
                0f,
                configuredSameReceiverDispatchInterval);
        }

        public bool HasValidMotionForEditor()
        {
            return receiverLaneOffsets != null
                   && receiverLaneOffsets.Length > 0
                   && durationSeconds >= 0.1f
                   && fadeStartNormalized >= 0f
                   && fadeStartNormalized < 1f
                   && sameReceiverDispatchInterval >= 0f;
        }
#endif
    }
}
