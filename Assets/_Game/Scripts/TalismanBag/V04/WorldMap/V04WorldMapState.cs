using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.V04.WorldMap
{
    public enum V04WorldMapPage
    {
        WorldRegionView = 0,
        QingshifangRegionView = 1,
        ChapterStageMapView = 2
    }

    public sealed class V04WorldMapProgressSnapshot
    {
        public const string SchemaId = "V04WorldMapProgressSnapshot.v1";
        public const string UnboundAuthority = "WORLD_MAP_PROGRESS_OWNER_UNBOUND";
        public const string CompletionRepositoryAuthority =
            "V04CampaignStageCompletionRepository";

        public readonly string authority;
        public readonly IReadOnlyList<string> clearedStageIds;
        public readonly IReadOnlyList<string> availableStageIds;
        public readonly string patrolStageId;
        public readonly bool formalProgress;

        public V04WorldMapProgressSnapshot(
            string authority,
            IEnumerable<string> clearedStageIds,
            IEnumerable<string> availableStageIds,
            string patrolStageId,
            bool formalProgress)
        {
            this.authority = Normalize(authority);
            this.clearedStageIds = NormalizeIds(clearedStageIds);
            this.availableStageIds = NormalizeIds(availableStageIds);
            this.patrolStageId = Normalize(patrolStageId);
            this.formalProgress = formalProgress;
        }

        public static V04WorldMapProgressSnapshot CreateUnboundPreview()
        {
            return new V04WorldMapProgressSnapshot(
                UnboundAuthority,
                Array.Empty<string>(),
                new[] { V04WorldMapCatalog.DefaultAvailableStageId },
                string.Empty,
                false);
        }

        public static V04WorldMapProgressSnapshot CreateFromCompletion(
            V04CampaignStageCompletionSnapshot completion)
        {
            if (completion == null
                || !string.Equals(
                    completion.schemaId,
                    V04CampaignStageCompletionRepository.SchemaId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    completion.campaignId,
                    V04CampaignStageCompletionRepository.CampaignId,
                    StringComparison.Ordinal))
            {
                return new V04WorldMapProgressSnapshot(
                    CompletionRepositoryAuthority,
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    string.Empty,
                    true);
            }

            IReadOnlyList<string> completed = completion.completedStageIds
                ?? Array.Empty<string>();
            return new V04WorldMapProgressSnapshot(
                CompletionRepositoryAuthority,
                completed,
                completed.Count == 0
                    ? new[] { V04WorldMapCatalog.DefaultAvailableStageId }
                    : Array.Empty<string>(),
                string.Empty,
                true);
        }

        public bool IsCleared(string stageId)
        {
            return clearedStageIds.Contains(Normalize(stageId), StringComparer.Ordinal);
        }

        public bool IsAvailable(string stageId)
        {
            return availableStageIds.Contains(Normalize(stageId), StringComparer.Ordinal);
        }

        private static ReadOnlyCollection<string> NormalizeIds(IEnumerable<string> values)
        {
            return new ReadOnlyCollection<string>(
                (values ?? Array.Empty<string>())
                    .Select(Normalize)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList());
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public static class V04WorldMapStageStateResolver
    {
        public static V04WorldMapStageVisualState Resolve(
            V04WorldMapStageDefinition stage,
            V04WorldMapProgressSnapshot progress)
        {
            if (stage == null)
            {
                return V04WorldMapStageVisualState.Locked;
            }

            V04WorldMapProgressSnapshot safeProgress =
                progress ?? V04WorldMapProgressSnapshot.CreateUnboundPreview();
            if (safeProgress.IsCleared(stage.stageId))
            {
                return V04WorldMapStageVisualState.Cleared;
            }

            if (stage.isBossStage)
            {
                return V04WorldMapStageVisualState.Boss;
            }

            return safeProgress.IsAvailable(stage.stageId)
                ? V04WorldMapStageVisualState.Available
                : V04WorldMapStageVisualState.Locked;
        }
    }

    [Serializable]
    public sealed class V04WorldMapBattleEntryRequest
    {
        public const string SchemaId = "V04WorldMapBattleEntryRequest.v1";

        public string schemaId = SchemaId;
        public string requestId = string.Empty;
        public string worldId = string.Empty;
        public string regionId = string.Empty;
        public string chapterId = string.Empty;
        public string stageId = string.Empty;
        public string returnSceneName = string.Empty;
        public string returnPage = string.Empty;
        public bool replay;
        public bool devOnly = true;
        public bool formalFlow;
    }

    [Serializable]
    public sealed class V04WorldMapReturnContext
    {
        public const string SchemaId = "V04WorldMapReturnContext.v1";

        public string schemaId = SchemaId;
        public string regionId = string.Empty;
        public string chapterId = string.Empty;
        public string stageId = string.Empty;
        public bool reopenStageDetail;
    }

    [Serializable]
    public sealed class V04WorldMapPatrolSelectionRequest
    {
        public const string SchemaId = "V04WorldMapPatrolSelectionRequest.v1";

        public string schemaId = SchemaId;
        public string stageId = string.Empty;
        public string dropTableId = string.Empty;
        public bool requiresClearedStage = true;
        public bool devOnly = true;
        public bool formalFlow;
    }

    [Serializable]
    public sealed class V04WorldMapOfflineSettlementRequest
    {
        public const string SchemaId = "V04WorldMapOfflineSettlementRequest.v1";

        public string schemaId = SchemaId;
        public string patrolStageId = string.Empty;
        public string dropTableId = string.Empty;
        public long elapsedSeconds;
        public string settlementRequestId = string.Empty;
        public bool simulatePerFrameBattle;
        public bool devOnly = true;
        public bool formalFlow;
    }
}
