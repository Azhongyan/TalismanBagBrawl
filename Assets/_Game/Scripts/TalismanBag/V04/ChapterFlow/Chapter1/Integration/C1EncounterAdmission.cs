using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public enum C1EncounterAdmissionKind
    {
        None = 0,
        Ordinary = 1,
        Boss = 2
    }

    public sealed class C1EncounterAdmissionActorSnapshot
    {
        public C1EncounterAdmissionActorSnapshot(
            string waveEntryId,
            string enemyInstanceId,
            string enemyContentId,
            string runtimeProfileId,
            string visualProfileKey,
            int occurrenceOrdinal,
            string displaySlotId,
            int slotOrdinal,
            int resetGeneration)
        {
            WaveEntryId = waveEntryId ?? string.Empty;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            EnemyContentId = enemyContentId ?? string.Empty;
            RuntimeProfileId = runtimeProfileId ?? string.Empty;
            VisualProfileKey = visualProfileKey ?? string.Empty;
            OccurrenceOrdinal = occurrenceOrdinal;
            DisplaySlotId = displaySlotId ?? string.Empty;
            SlotOrdinal = slotOrdinal;
            ResetGeneration = resetGeneration;
        }

        public string WaveEntryId { get; }
        public string EnemyInstanceId { get; }
        public string EnemyContentId { get; }
        public string RuntimeProfileId { get; }
        public string VisualProfileKey { get; }
        public int OccurrenceOrdinal { get; }
        public string DisplaySlotId { get; }
        public int SlotOrdinal { get; }
        public int ResetGeneration { get; }
    }

    public sealed class C1EncounterAdmissionSnapshot
    {
        public C1EncounterAdmissionSnapshot(
            string currentStageId,
            C1EncounterAdmissionKind admittedEncounterKind,
            string admittedEnemyContentId,
            string runtimeProfileId,
            string admittedVisualProfileKey,
            bool isBossAdmissionAccepted,
            string acceptedBattleRequestId,
            string waveEntryId = "",
            int waveIndex = 0,
            int occurrenceOrdinal = 0,
            int resetGeneration = 0,
            string waveId = "",
            IReadOnlyList<C1EncounterAdmissionActorSnapshot> actors = null)
        {
            CurrentStageId = currentStageId ?? string.Empty;
            AdmittedEncounterKind = admittedEncounterKind;
            AdmittedEnemyContentId =
                admittedEnemyContentId ?? string.Empty;
            RuntimeProfileId = runtimeProfileId ?? string.Empty;
            AdmittedVisualProfileKey =
                admittedVisualProfileKey ?? string.Empty;
            IsBossAdmissionAccepted = isBossAdmissionAccepted;
            AcceptedBattleRequestId =
                acceptedBattleRequestId ?? string.Empty;
            WaveEntryId = waveEntryId ?? string.Empty;
            WaveId = waveId ?? string.Empty;
            WaveIndex = waveIndex;
            OccurrenceOrdinal = occurrenceOrdinal;
            ResetGeneration = resetGeneration;
            Actors = (actors ??
                Array.Empty<C1EncounterAdmissionActorSnapshot>())
                .Where(value => value != null)
                .OrderBy(value => value.SlotOrdinal)
                .ToArray();
        }

        public string CurrentStageId { get; }
        public C1EncounterAdmissionKind AdmittedEncounterKind { get; }
        public string AdmittedEnemyContentId { get; }
        public string RuntimeProfileId { get; }
        public string AdmittedVisualProfileKey { get; }
        public bool IsBossAdmissionAccepted { get; }
        public string AcceptedBattleRequestId { get; }
        public string WaveEntryId { get; }
        public string WaveId { get; }
        public int WaveIndex { get; }
        public int OccurrenceOrdinal { get; }
        public int ResetGeneration { get; }
        public IReadOnlyList<C1EncounterAdmissionActorSnapshot> Actors
        {
            get;
        }
        public int ActiveActorCount => Actors.Count;

        public bool IsOrdinary =>
            AdmittedEncounterKind == C1EncounterAdmissionKind.Ordinary
            && !string.IsNullOrWhiteSpace(WaveId)
            && WaveIndex > 0
            && ResetGeneration > 0
            && Actors.Count > 0
            && Actors.All(value =>
                !string.IsNullOrWhiteSpace(value.WaveEntryId)
                && !string.IsNullOrWhiteSpace(value.EnemyInstanceId)
                && value.SlotOrdinal > 0);

        public bool IsAcceptedBoss =>
            AdmittedEncounterKind == C1EncounterAdmissionKind.Boss
            && IsBossAdmissionAccepted
            && string.Equals(
                CurrentStageId,
                "1-10",
                StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(AcceptedBattleRequestId);

        public static C1EncounterAdmissionSnapshot None(
            string currentStageId = "")
        {
            return new C1EncounterAdmissionSnapshot(
                currentStageId,
                C1EncounterAdmissionKind.None,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                string.Empty);
        }
    }

    public interface IC1EncounterAdmissionAuthority
    {
        C1EncounterAdmissionSnapshot EncounterAdmission { get; }

        bool IsShougunuPhase1StartAdmitted(
            string requesterSceneName);
    }
}
