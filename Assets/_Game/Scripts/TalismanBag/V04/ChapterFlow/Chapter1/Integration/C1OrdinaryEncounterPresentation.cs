using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BuildSandbox;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public sealed class C1OrdinaryEncounterApplicationPresentationEvent :
        IBattleSandboxAcceptedDamagePresentation
    {
        private readonly ReadOnlyCollection<ItemShapeCell> occupiedCells;
        private readonly ReadOnlyCollection<string> reducerApplicationIds;

        public C1OrdinaryEncounterApplicationPresentationEvent(
            string applicationIdentity,
            long applicationSequence,
            IEnumerable<string> reducerApplicationIds,
            int resetGeneration,
            long battleTick,
            string sourceRequestId,
            string sourceBaseItemId,
            string sourceItemInstanceId,
            string sourcePlacementId,
            IEnumerable<ItemShapeCell> occupiedCells,
            int resolvedPreMitigationDamageUnits,
            int hpBefore,
            int hpAfter,
            int shellBefore,
            int shellAfter,
            string cueIdentity,
            long cueSequence,
            C1EnemyRuntimeSnapshot enemySnapshot)
        {
            ApplicationIdentity = applicationIdentity ?? string.Empty;
            ApplicationSequence = applicationSequence;
            this.reducerApplicationIds = Array.AsReadOnly(
                (reducerApplicationIds ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray());
            ResetGeneration = resetGeneration;
            BattleTick = battleTick;
            SourceRequestId = sourceRequestId ?? string.Empty;
            SourceBaseItemId = sourceBaseItemId ?? string.Empty;
            SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            SourcePlacementId = sourcePlacementId ?? string.Empty;
            this.occupiedCells = Array.AsReadOnly(
                (occupiedCells ?? Array.Empty<ItemShapeCell>())
                    .Select(value => new ItemShapeCell(value.x, value.y))
                    .Distinct()
                    .OrderBy(value => value.y)
                    .ThenBy(value => value.x)
                    .ToArray());
            ResolvedPreMitigationDamageUnits =
                resolvedPreMitigationDamageUnits;
            HpBefore = hpBefore;
            HpAfter = hpAfter;
            ShellBefore = shellBefore;
            ShellAfter = shellAfter;
            CueIdentity = cueIdentity ?? string.Empty;
            CueSequence = cueSequence;
            EnemySnapshot = enemySnapshot;
            EnemyRevision = enemySnapshot?.Revision ?? -1L;
        }

        public string ApplicationIdentity { get; }
        public long ApplicationSequence { get; }
        public IReadOnlyList<string> ReducerApplicationIds =>
            reducerApplicationIds;
        public string EventId => ApplicationIdentity;
        public int ResetGeneration { get; }
        public long BattleTick { get; }
        public string SourceRequestId { get; }
        public string SourceBaseItemId { get; }
        public string SourceItemInstanceId { get; }
        public string SourcePlacementId { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells => occupiedCells;
        public int ResolvedPreMitigationDamageUnits { get; }
        public int HpBefore { get; }
        public int HpAfter { get; }
        public int ShellBefore { get; }
        public int ShellAfter { get; }
        public int HpDamageApplied => Math.Max(0, HpBefore - HpAfter);
        public int ShellDamageApplied =>
            Math.Max(0, ShellBefore - ShellAfter);
        public int TotalDamageApplied =>
            checked(HpDamageApplied + ShellDamageApplied);
        public bool ShellBreakApplied =>
            ShellBefore > 0 && ShellAfter == 0;
        public string CueIdentity { get; }
        public long CueSequence { get; }
        public C1EnemyRuntimeSnapshot EnemySnapshot { get; }
        public long EnemyRevision { get; }

        public bool IsAcceptedCorrelation =>
            ApplicationSequence > 0L
            && reducerApplicationIds.Count > 0
            && ResetGeneration > 0
            && BattleTick > 0L
            && !string.IsNullOrWhiteSpace(ApplicationIdentity)
            && !string.IsNullOrWhiteSpace(SourceRequestId)
            && !string.IsNullOrWhiteSpace(SourceBaseItemId)
            && !string.IsNullOrWhiteSpace(SourceItemInstanceId)
            && !string.IsNullOrWhiteSpace(SourcePlacementId)
            && occupiedCells.Count > 0
            && ResolvedPreMitigationDamageUnits > 0
            && TotalDamageApplied > 0
            && HpBefore >= HpAfter
            && ShellBefore >= ShellAfter
            && EnemySnapshot != null
            && EnemySnapshot.ResetGeneration == ResetGeneration
            && EnemyRevision == EnemySnapshot.Revision;
    }

    public sealed class C1OrdinaryEncounterEnemyToPlayerPresentationEvent
    {
        public C1OrdinaryEncounterEnemyToPlayerPresentationEvent(
            string eventId,
            long presentationSequence,
            int resetGeneration,
            long battleTick,
            string enemyInstanceId,
            string requestId,
            string requestKey,
            string sourceIdentity,
            int hpBefore,
            int hpAfter)
        {
            EventId = eventId ?? string.Empty;
            PresentationSequence = presentationSequence;
            ResetGeneration = resetGeneration;
            BattleTick = battleTick;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            RequestId = requestId ?? string.Empty;
            RequestKey = requestKey ?? string.Empty;
            SourceIdentity = sourceIdentity ?? string.Empty;
            HpBefore = hpBefore;
            HpAfter = hpAfter;
        }

        public string EventId { get; }
        public long PresentationSequence { get; }
        public int ResetGeneration { get; }
        public long BattleTick { get; }
        public string EnemyInstanceId { get; }
        public string RequestId { get; }
        public string RequestKey { get; }
        public string SourceIdentity { get; }
        public int HpBefore { get; }
        public int HpAfter { get; }
        public int HpDamageApplied => Math.Max(0, HpBefore - HpAfter);

        public bool IsAcceptedCorrelation =>
            PresentationSequence > 0L
            && ResetGeneration > 0
            && BattleTick > 0L
            && !string.IsNullOrWhiteSpace(EventId)
            && !string.IsNullOrWhiteSpace(EnemyInstanceId)
            && !string.IsNullOrWhiteSpace(RequestId)
            && !string.IsNullOrWhiteSpace(RequestKey)
            && HpBefore >= HpAfter
            && HpDamageApplied > 0;
    }
}
