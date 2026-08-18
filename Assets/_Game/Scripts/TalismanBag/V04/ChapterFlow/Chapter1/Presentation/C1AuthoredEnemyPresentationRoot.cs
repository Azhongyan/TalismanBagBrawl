using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    public sealed class C1AuthoredEnemyPresentationRoot : MonoBehaviour
    {
        [SerializeField] private C1AuthoredEnemyPresentationSlotView[]
            ordinarySlots = Array.Empty<
                C1AuthoredEnemyPresentationSlotView>();

        private readonly Dictionary<
            string,
            C1AuthoredEnemyPresentationSlotView> slotsById =
                new(StringComparer.Ordinal);
        private bool initialized;

        public event Action<string> TargetSelectionRequested;

        public IReadOnlyList<C1AuthoredEnemyPresentationSlotView> Slots =>
            ordinarySlots ?? Array.Empty<
                C1AuthoredEnemyPresentationSlotView>();
        public bool IsInitialized => initialized;
        public string LastDiagnostic { get; private set; } =
            "NOT_INITIALIZED";

        private void OnEnable()
        {
            TryInitialize(out _);
        }

        private void OnDisable()
        {
            DetachWithoutPresentationWrite();
        }

        public bool TryInitialize(out string diagnostic)
        {
            diagnostic = string.Empty;
            if (initialized)
            {
                return true;
            }

            C1AuthoredEnemyPresentationSlotView[] candidates =
                ordinarySlots?.Where(value => value != null).ToArray()
                ?? Array.Empty<
                    C1AuthoredEnemyPresentationSlotView>();
            if (candidates.Length != 3)
            {
                diagnostic =
                    "AUTHORED_SLOT_COUNT_MUST_BE_EXACTLY_3";
                LastDiagnostic = diagnostic;
                return false;
            }

            slotsById.Clear();
            foreach (C1AuthoredEnemyPresentationSlotView slot
                     in candidates)
            {
                if (!slot.TryCaptureAuthoredBaseline(
                        out string slotDiagnostic))
                {
                    slotsById.Clear();
                    diagnostic = slot.DisplaySlotId + ":"
                        + slotDiagnostic;
                    LastDiagnostic = diagnostic;
                    return false;
                }
                if (!slotsById.TryAdd(slot.DisplaySlotId, slot))
                {
                    slotsById.Clear();
                    diagnostic = "DISPLAY_SLOT_ID_DUPLICATE_"
                        + slot.DisplaySlotId;
                    LastDiagnostic = diagnostic;
                    return false;
                }
            }
            if (!slotsById.ContainsKey("Slot01")
                || !slotsById.ContainsKey("Slot02")
                || !slotsById.ContainsKey("Slot03"))
            {
                slotsById.Clear();
                diagnostic =
                    "DISPLAY_SLOT_IDS_MUST_BE_SLOT01_SLOT02_SLOT03";
                LastDiagnostic = diagnostic;
                return false;
            }
            foreach (C1AuthoredEnemyPresentationSlotView slot
                     in slotsById.Values)
            {
                slot.TargetSelectionRequested +=
                    OnSlotSelectionRequested;
            }

            initialized = true;
            LastDiagnostic = "AUTHORED_SLOTS_READY";
            return true;
        }

        public bool TryApply(
            IReadOnlyCollection<C1EnemyPresentationActorFrame> frames,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (!TryInitialize(out diagnostic))
            {
                return false;
            }

            C1EnemyPresentationActorFrame[] rows =
                frames?.Where(value => value != null).ToArray()
                ?? Array.Empty<C1EnemyPresentationActorFrame>();
            if (rows.Length > ordinarySlots.Length)
            {
                diagnostic = "ACTOR_FRAME_COUNT_EXCEEDS_SLOT_COUNT";
                LastDiagnostic = diagnostic;
                return false;
            }
            if (rows.Any(value =>
                    string.IsNullOrWhiteSpace(value.EnemyInstanceId)
                    || string.IsNullOrWhiteSpace(value.DisplaySlotId)
                    || !slotsById.ContainsKey(
                        value.DisplaySlotId)))
            {
                diagnostic =
                    "ACTOR_FRAME_IDENTITY_OR_SLOT_UNKNOWN";
                LastDiagnostic = diagnostic;
                return false;
            }
            if (rows.Select(value => value.EnemyInstanceId)
                    .Distinct(StringComparer.Ordinal).Count()
                != rows.Length)
            {
                diagnostic = "ENEMY_INSTANCE_ID_DUPLICATE";
                LastDiagnostic = diagnostic;
                return false;
            }
            if (rows.Select(value => value.DisplaySlotId)
                    .Distinct(StringComparer.Ordinal).Count()
                != rows.Length)
            {
                diagnostic = "DISPLAY_SLOT_ID_DUPLICATE_IN_FRAME";
                LastDiagnostic = diagnostic;
                return false;
            }

            Dictionary<string, C1EnemyPresentationActorFrame> bySlot =
                rows.ToDictionary(
                    value => value.DisplaySlotId,
                    value => value,
                    StringComparer.Ordinal);
            foreach (KeyValuePair<
                         string,
                         C1AuthoredEnemyPresentationSlotView> pair
                     in slotsById)
            {
                if (!bySlot.TryGetValue(
                        pair.Key,
                        out C1EnemyPresentationActorFrame frame))
                {
                    pair.Value.ClearRuntimePresentation();
                    continue;
                }
                if (!pair.Value.TryApply(
                        frame,
                        out string slotDiagnostic))
                {
                    diagnostic = pair.Key + ":" + slotDiagnostic;
                    LastDiagnostic = diagnostic;
                    return false;
                }
            }

            LastDiagnostic = "ACTOR_FRAMES_BOUND_"
                + string.Join(
                    "_",
                    rows.OrderBy(value => value.DisplaySlotId,
                            StringComparer.Ordinal)
                        .Select(value => value.BindingKey));
            return true;
        }

        public void ClearRuntimePresentation()
        {
            if (!TryInitialize(out _))
            {
                return;
            }
            foreach (C1AuthoredEnemyPresentationSlotView slot
                     in slotsById.Values)
            {
                slot.ClearRuntimePresentation();
            }
            LastDiagnostic = "AUTHORED_SLOTS_CLEARED";
        }

        public void RestoreAuthoredBaseline()
        {
            if (!initialized)
            {
                return;
            }
            foreach (C1AuthoredEnemyPresentationSlotView slot
                     in slotsById.Values)
            {
                slot.TargetSelectionRequested -=
                    OnSlotSelectionRequested;
                slot.RestoreAuthoredBaseline();
            }
            slotsById.Clear();
            initialized = false;
            LastDiagnostic = "AUTHORED_BASELINE_RESTORED";
        }

        private void DetachWithoutPresentationWrite()
        {
            foreach (C1AuthoredEnemyPresentationSlotView slot
                     in slotsById.Values)
            {
                slot.TargetSelectionRequested -=
                    OnSlotSelectionRequested;
            }
            slotsById.Clear();
            initialized = false;
            LastDiagnostic =
                "EDIT_MODE_DETACHED_WITHOUT_PRESENTATION_WRITE";
        }

        private void OnSlotSelectionRequested(
            C1AuthoredEnemyPresentationSlotView slot)
        {
            if (slot != null
                && !string.IsNullOrWhiteSpace(
                    slot.BoundEnemyInstanceId))
            {
                TargetSelectionRequested?.Invoke(
                    slot.BoundEnemyInstanceId);
            }
        }
    }
}
