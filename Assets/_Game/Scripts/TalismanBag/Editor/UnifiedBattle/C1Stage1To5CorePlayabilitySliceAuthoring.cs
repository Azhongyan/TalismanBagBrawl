#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.V04.Campaign.Chapter1;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1Stage1To5CorePlayabilitySliceAuthoring
    {
        public const string SuccessMarker =
            "C1_STAGE_1_TO_5_CONFIG_AUTHORING_AND_VALIDATION_PASS";
        private const string Folder =
            "Assets/_Game/Resources/TalismanBag/Campaign/Chapter1/Stages";

        private sealed class Definition
        {
            internal Definition(
                string stageId,
                string balance,
                string encounter,
                string theme,
                string presentation)
            {
                StageId = stageId;
                Balance = balance;
                Encounter = encounter;
                Theme = theme;
                Presentation = presentation;
            }

            internal string StageId { get; }
            internal string Balance { get; }
            internal string Encounter { get; }
            internal string Theme { get; }
            internal string Presentation { get; }
        }

        private static readonly Definition[] Definitions =
        {
            new Definition(
                "1-1",
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor("1-1"),
                "campaign.normal.lv1.theme.bone_aspect.c1",
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor("1-1")),
            new Definition(
                "1-2",
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor("1-2"),
                "campaign.normal.lv1.theme.bone_aspect.c1",
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor("1-2")),
            new Definition(
                "1-3",
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor("1-3"),
                "campaign.normal.lv1.theme.bone_aspect.c1.exterior",
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor("1-3")),
            new Definition(
                "1-4",
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor("1-4"),
                "campaign.normal.lv1.theme.bone_aspect.c1.exterior",
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor("1-4")),
            new Definition(
                "1-5",
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor("1-5"),
                "campaign.normal.lv1.theme.bone_aspect.c1.night_animated",
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor("1-5"))
        };

        [MenuItem(
            "TalismanBag/V0.4/Unified Battle/Author Stage 1-1 To 1-5 Configs Once")]
        public static void ApplyFromMenu()
        {
            C1FormalObtainRebuildBattleLoopTests
                .RunStage1To5CorePlayabilitySliceOrThrow();
            ApplySinglePass();
            Debug.Log(SuccessMarker);
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplyFromMenu();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "C1_STAGE_1_TO_5_AUTHORING_FAILED " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void ApplySinglePass()
        {
            string[] existing = AssetDatabase.FindAssets(
                    "t:Chapter1CampaignStageConfig",
                    new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            if (existing.Any(path => !Definitions.Any(definition =>
                    string.Equals(
                        path,
                        PathFor(definition.StageId),
                        StringComparison.Ordinal))))
            {
                throw new InvalidOperationException(
                    "C1_STAGE_CONFIG_UNEXPECTED_ASSET "
                    + string.Join(",", existing));
            }

            foreach (Definition definition in Definitions)
            {
                string path = PathFor(definition.StageId);
                Chapter1CampaignStageConfig config =
                    AssetDatabase.LoadAssetAtPath<Chapter1CampaignStageConfig>(path);
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<
                        Chapter1CampaignStageConfig>();
                    config.name = "CampaignStage_" + definition.StageId;
                    AssetDatabase.CreateAsset(config, path);
                }

                config.ConfigureForEditor(
                    definition.StageId,
                    definition.Balance,
                    definition.Encounter,
                    definition.Theme,
                    definition.Presentation,
                    true,
                    true);
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(Folder, ImportAssetOptions.ForceUpdate);
            ValidateAll();
        }

        public static void ValidateAll()
        {
            List<Chapter1CampaignStageConfig> configs = Definitions.Select(
                    definition => AssetDatabase.LoadAssetAtPath<
                        Chapter1CampaignStageConfig>(
                        PathFor(definition.StageId)))
                .ToList();
            string diagnostic = string.Empty;
            if (configs.Any(config => config == null)
                || configs.Count != 5
                || configs.Any(config => !config.TryValidate(out _))
                || !Chapter1CampaignStageCatalog.TryBuildValidatedCatalog(
                    configs,
                    out IReadOnlyDictionary<string,
                        Chapter1CampaignStageConfig> catalog,
                    out diagnostic)
                || catalog.Count != 5)
            {
                throw new InvalidOperationException(
                    "C1_STAGE_CONFIG_FINAL_VALIDATION_FAILED " + diagnostic);
            }

            foreach (Definition expected in Definitions)
            {
                Chapter1CampaignStageConfig actual = catalog[expected.StageId];
                if (!string.Equals(actual.BalanceProfileId, expected.Balance,
                        StringComparison.Ordinal)
                    || !string.Equals(actual.EncounterVariantId,
                        expected.Encounter, StringComparison.Ordinal)
                    || !string.Equals(actual.StageThemeProfileId,
                        expected.Theme, StringComparison.Ordinal)
                    || !string.Equals(actual.EnemyPresentationProfileId,
                        expected.Presentation, StringComparison.Ordinal)
                    || !actual.RouteEnabled || !actual.BattleContentReady)
                {
                    throw new InvalidOperationException(
                        "C1_STAGE_CONFIG_IDENTITY_INVALID stageId="
                        + expected.StageId);
                }
            }
        }

        private static string PathFor(string stageId)
        {
            return Folder + "/CampaignStage_" + stageId + ".asset";
        }
    }
}
#endif
