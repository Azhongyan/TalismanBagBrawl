using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace TalismanBag.V04.Campaign.Chapter1
{
    public static class Chapter1CampaignStageCatalog
    {
        public const string ResourcesPath =
            "TalismanBag/Campaign/Chapter1/Stages";

        public static bool TryLoad(
            out IReadOnlyDictionary<string, Chapter1CampaignStageConfig> stages,
            out string diagnostic)
        {
            Chapter1CampaignStageConfig[] loaded =
                Resources.LoadAll<Chapter1CampaignStageConfig>(ResourcesPath);
            return TryBuildValidatedCatalog(loaded, out stages, out diagnostic);
        }

        public static bool TryBuildValidatedCatalog(
            IEnumerable<Chapter1CampaignStageConfig> configs,
            out IReadOnlyDictionary<string, Chapter1CampaignStageConfig> stages,
            out string diagnostic)
        {
            Dictionary<string, Chapter1CampaignStageConfig> rows =
                new(StringComparer.Ordinal);
            foreach (Chapter1CampaignStageConfig config in
                     (configs ?? Array.Empty<Chapter1CampaignStageConfig>())
                     .Where(config => config != null)
                     .OrderBy(config => config.StageId, StringComparer.Ordinal))
            {
                if (!config.TryValidate(out string configDiagnostic))
                {
                    stages = Empty();
                    diagnostic = configDiagnostic + " stageId=" + config.StageId;
                    return false;
                }

                if (!rows.TryAdd(config.StageId, config))
                {
                    stages = Empty();
                    diagnostic = "STAGE_CONFIG_DUPLICATE_STAGE_ID stageId=" +
                                 config.StageId;
                    return false;
                }
            }

            if (rows.Count != Chapter1CampaignStageConfig.SupportedStageIds.Count
                || Chapter1CampaignStageConfig.SupportedStageIds.Any(
                    stageId => !rows.ContainsKey(stageId)))
            {
                stages = Empty();
                diagnostic = "STAGE_CONFIG_REQUIRED_IDENTITIES_MISSING";
                return false;
            }

            stages = new ReadOnlyDictionary<string, Chapter1CampaignStageConfig>(rows);
            diagnostic = "STAGE_CONFIG_CATALOG_VALID";
            return true;
        }

        public static bool TryGet(
            string stageId,
            out Chapter1CampaignStageConfig config,
            out string diagnostic)
        {
            config = null;
            if (!TryLoad(out IReadOnlyDictionary<string, Chapter1CampaignStageConfig> rows,
                    out diagnostic))
            {
                return false;
            }

            string normalized = string.IsNullOrWhiteSpace(stageId)
                ? string.Empty
                : stageId.Trim();
            if (!rows.TryGetValue(normalized, out config))
            {
                diagnostic = "STAGE_CONFIG_UNKNOWN_STAGE stageId=" + normalized;
                return false;
            }

            diagnostic = "STAGE_CONFIG_FOUND";
            return true;
        }

        private static IReadOnlyDictionary<string, Chapter1CampaignStageConfig> Empty()
        {
            return new ReadOnlyDictionary<string, Chapter1CampaignStageConfig>(
                new Dictionary<string, Chapter1CampaignStageConfig>(
                    StringComparer.Ordinal));
        }
    }
}
