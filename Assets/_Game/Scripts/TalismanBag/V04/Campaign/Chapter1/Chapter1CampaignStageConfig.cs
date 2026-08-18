using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Contracts.Battle;
using UnityEngine;

namespace TalismanBag.V04.Campaign.Chapter1
{
    [CreateAssetMenu(
        fileName = "Chapter1CampaignStageConfig",
        menuName = "TalismanBag/V0.4/Campaign/Chapter 1 Stage Config")]
    public sealed class Chapter1CampaignStageConfig : ScriptableObject
    {
        public const string SchemaId = "Chapter1CampaignStageConfig.v1";
        public const int CurrentSchemaVersion = 1;
        public const string ProductContextId = "CAMPAIGN_NORMAL_LV1";
        public const string ChapterIdValue = "bone_aspect_chapter_1";
        public const string StageOneId = "1-1";
        public const string StageTwoId = "1-2";
        public const string StageThreeId = "1-3";
        public const string StageFourId = "1-4";
        public const string StageFiveId = "1-5";
        public const string BalanceProfileIdValue =
            "campaign.normal.lv1.balance.identity.c1";
        private const string EncounterVariantPrefix =
            "campaign.normal.lv1.encounter.c1.";
        private const string EnemyPresentationProfilePrefix =
            "campaign.normal.lv1.enemy_presentation.c1.";
        public const string SourceRouteId = "TalismanSceneRoute.WorldMap";
        public const string ReturnRouteId = "TalismanSceneRoute.WorldMap";

        private static readonly ReadOnlyCollection<string> SupportedStages =
            Array.AsReadOnly(new[]
            {
                StageOneId,
                StageTwoId,
                StageThreeId,
                StageFourId,
                StageFiveId
            });

        public static IReadOnlyList<string> SupportedStageIds => SupportedStages;

        public static string EncounterVariantIdFor(string candidateStageId)
        {
            return SupportedStages.Contains(
                candidateStageId,
                StringComparer.Ordinal)
                ? EncounterVariantPrefix + candidateStageId
                : string.Empty;
        }

        public static string EnemyPresentationProfileIdFor(
            string candidateStageId)
        {
            return SupportedStages.Contains(
                candidateStageId,
                StringComparer.Ordinal)
                ? EnemyPresentationProfilePrefix + candidateStageId
                : string.Empty;
        }

        [SerializeField] private string schemaId = SchemaId;
        [SerializeField] private int schemaVersion = CurrentSchemaVersion;
        [SerializeField] private string productContext = ProductContextId;
        [SerializeField] private string chapterId = ChapterIdValue;
        [SerializeField] private string stageId = string.Empty;
        [SerializeField] private string balanceProfileId = string.Empty;
        [SerializeField] private string encounterVariantId = string.Empty;
        [SerializeField] private string stageThemeProfileId = string.Empty;
        [SerializeField] private string enemyPresentationProfileId = string.Empty;
        [SerializeField] private bool routeEnabled;
        [SerializeField] private bool battleContentReady;
        [SerializeField] private string[] reservedExtensionKeys = Array.Empty<string>();

        public string StageId => stageId;
        public string ChapterId => chapterId;
        public string ProductContext => productContext;
        public string BalanceProfileId => balanceProfileId;
        public string EncounterVariantId => encounterVariantId;
        public string StageThemeProfileId => stageThemeProfileId;
        public string EnemyPresentationProfileId => enemyPresentationProfileId;
        public bool RouteEnabled => routeEnabled;
        public bool BattleContentReady => battleContentReady;

        public bool TryValidate(out string diagnostic)
        {
            if (!EqualsOrdinal(schemaId, SchemaId)
                || schemaVersion != CurrentSchemaVersion)
            {
                diagnostic = "STAGE_CONFIG_SCHEMA_MISMATCH";
                return false;
            }

            if (!EqualsOrdinal(productContext, ProductContextId))
            {
                diagnostic = "STAGE_CONFIG_PRODUCT_CONTEXT_REJECTED";
                return false;
            }

            if (!EqualsOrdinal(chapterId, ChapterIdValue))
            {
                diagnostic = "STAGE_CONFIG_CHAPTER_ID_REJECTED";
                return false;
            }

            if (!SupportedStages.Contains(stageId, StringComparer.Ordinal))
            {
                diagnostic = "STAGE_CONFIG_UNKNOWN_STAGE";
                return false;
            }

            if (!EqualsOrdinal(balanceProfileId, BalanceProfileIdValue)
                || !EqualsOrdinal(
                    encounterVariantId,
                    EncounterVariantIdFor(stageId))
                || string.IsNullOrWhiteSpace(stageThemeProfileId)
                || !EqualsOrdinal(
                    enemyPresentationProfileId,
                    EnemyPresentationProfileIdFor(stageId)))
            {
                diagnostic = "STAGE_CONFIG_FORMAL_IDENTITY_MISMATCH";
                return false;
            }

            if (reservedExtensionKeys == null
                || Array.Exists(
                    reservedExtensionKeys,
                    key => !string.IsNullOrWhiteSpace(key)))
            {
                diagnostic = "STAGE_CONFIG_UNKNOWN_FIELD_REJECTED";
                return false;
            }

            if (!routeEnabled || !battleContentReady)
            {
                diagnostic = "STAGE_CONFIG_FORMAL_ROUTE_FLAGS_INVALID stageId=" +
                             stageId;
                return false;
            }

            diagnostic = "STAGE_CONFIG_VALID";
            return true;
        }

        public bool MatchesContext(BattleLaunchContext context)
        {
            return context != null
                && EqualsOrdinal(context.ProductContext, productContext)
                && EqualsOrdinal(context.ChapterId, chapterId)
                && EqualsOrdinal(context.StageId, stageId)
                && EqualsOrdinal(context.BalanceProfileId, balanceProfileId)
                && EqualsOrdinal(context.EncounterVariantId, encounterVariantId)
                && EqualsOrdinal(context.StageThemeProfileId, stageThemeProfileId)
                && EqualsOrdinal(
                    context.EnemyPresentationProfileId,
                    enemyPresentationProfileId);
        }

        public BattleLaunchContext CreateLaunchContext(
            string launchId,
            string token,
            long generation)
        {
            return new BattleLaunchContext(
                BattleLaunchContext.SchemaId,
                BattleLaunchContext.CurrentSchemaVersion,
                launchId,
                token,
                generation,
                productContext,
                chapterId,
                stageId,
                balanceProfileId,
                encounterVariantId,
                stageThemeProfileId,
                enemyPresentationProfileId,
                SourceRouteId,
                ReturnRouteId);
        }

        public void ConfigureForEditor(
            string configuredStageId,
            string configuredBalanceProfileId,
            string configuredEncounterVariantId,
            string configuredStageThemeProfileId,
            string configuredEnemyPresentationProfileId,
            bool configuredRouteEnabled,
            bool configuredBattleContentReady)
        {
            schemaId = SchemaId;
            schemaVersion = CurrentSchemaVersion;
            productContext = ProductContextId;
            chapterId = ChapterIdValue;
            stageId = Normalize(configuredStageId);
            balanceProfileId = Normalize(configuredBalanceProfileId);
            encounterVariantId = Normalize(configuredEncounterVariantId);
            stageThemeProfileId = Normalize(configuredStageThemeProfileId);
            enemyPresentationProfileId = Normalize(
                configuredEnemyPresentationProfileId);
            routeEnabled = configuredRouteEnabled;
            battleContentReady = configuredBattleContentReady;
            reservedExtensionKeys = Array.Empty<string>();
        }

        private static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
