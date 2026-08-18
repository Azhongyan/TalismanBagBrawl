#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class BuildSynergyCoreVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/BuildSynergyCoreReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/BuildSynergyCoreSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/BuildSynergyCoreLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/BuildSynergyCoreVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Build.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/BuildSynergyCoreVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/BuildSynergyCoreVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildSynergyCoreReport.md",
            "Docs/V0.4/Reports/BuildSynergyCoreSpec.csv",
            "Docs/V0.4/Reports/BuildSynergyCoreLeakCheckReport.md"
        };

        private static readonly string[] ForbiddenSourceTokens =
        {
            "BattleResolver",
            "BattleSnapshotAdapter",
            "UnifiedBattlePage",
            "BattleBridge",
            "V02RunFlow",
            "V03RunFlow",
            "RunFlowController",
            "SaveData",
            "PlayerPrefs",
            "RewardConfig",
            "DropTable",
            "BossInfo",
            "EditorBuildSettings.scenes ="
        };
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<BuildSynergySpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("BuildSynergyCore01 verification passed and reports were written.");
                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        Debug.LogError(error);
                    }

                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result.Errors.Add(exception.Message);
                WriteReports(result, rows);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(VerificationResult result, List<BuildSynergySpecRow> rows)
        {
            ItemLightingResolutionResult lightingResult = CreateSampleLightingResult();
            ItemBuildSynergyResolutionResult buildResult = ItemBuildSynergyResolver.Resolve(lightingResult, ItemInnerDataCatalog.AllItems);

            CheckBuildTracks(result, rows, buildResult);
            CheckCountingExclusions(result, rows, buildResult);
            CheckValidationScenarios(result, rows, buildResult);
            CheckNoMainBuildAndNoMonitorSlots(result, rows, buildResult);
            CheckViewModelProjection(result, rows, lightingResult, buildResult);
            CheckSnapshotInterface(result, rows);
            CheckOverview(result, rows, buildResult);
            CheckNullSnapshot(result, rows);
            CheckSourceScope(result);
        }

        private static ItemLightingResolutionResult CreateSampleLightingResult()
        {
            return new ItemLightingResolutionResult(
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 0) },
                new[]
                {
                    Lit("I031", "JuNian source", "P_I031_SOURCE", true, false, "P_I031_SOURCE", 0),
                    Lit("I001", "Zhenlei fu 1", "P_I001", false, true, "P_I031_SOURCE", 0),
                    Lit("I002", "Zhenlei fu 2", "P_I002", false, true, "P_I031_SOURCE", 0),
                    Lit("I003", "Zhenlei yin", "P_I003", false, false, "P_I001", 1),
                    Lit("I004", "Zhenlei ling", "P_I004", false, false, "P_I003", 2),
                    Lit("I005", "Zhenlei jing", "P_I005", false, false, "P_I004", 3),
                    Lit("I006", "Zhenlei fa", "P_I006", false, false, "P_I005", 4),
                    Lit("I001", "Duplicate base item excluded", "P_I001_DUP_BASE", false, true, "P_I031_SOURCE", 0),
                    Lit("I002", "Duplicate placement ignored", "P_I001", false, true, "P_I031_SOURCE", 0),
                    Lit("I007", "Lihuo fu 1", "P_I007", false, true, "P_I031_SOURCE", 0),
                    Lit("I008", "Lihuo fu 2", "P_I008", false, false, "P_I007", 1),
                    Unlit("I009", "Lihuo yin unlit", "P_I009_UNLIT"),
                    Lit("I999", "Missing catalog", "P_MISSING_CATALOG", false, true, "P_I031_SOURCE", 0),
                    BlankPlacementLit("I012", "Blank placement ignored")
                });
        }

        private static ItemLightingItemResult Lit(
            string itemId,
            string displayName,
            string placementId,
            bool isLightingSource,
            bool isDirectLit,
            string litByPlacementId,
            int litDepth)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { new Vector2Int(Mathf.Abs(placementId.GetHashCode()) % 5, Mathf.Abs(itemId.GetHashCode()) % 5) },
                new Vector2Int(0, 0),
                isLightingSource,
                isDirectLit,
                true,
                litByPlacementId,
                litDepth,
                placementId,
                litByPlacementId);
        }

        private static ItemLightingItemResult Unlit(string itemId, string displayName, string placementId)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { new Vector2Int(4, 4) },
                new Vector2Int(4, 4),
                false,
                false,
                false,
                string.Empty,
                -1,
                placementId,
                string.Empty);
        }

        private static ItemLightingItemResult BlankPlacementLit(string itemId, string displayName)
        {
            ItemLightingItemResult item = Lit(
                itemId,
                displayName,
                "P_BLANK_TEMP",
                false,
                true,
                "P_I031_SOURCE",
                0);
            item.placementId = string.Empty;
            return item;
        }

        private static void CheckBuildTracks(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult)
        {
            CheckTrack(result, rows, buildResult.FindFaMenBuild("famen:zhenlei"), "famen:zhenlei", "faMen", 6, true, true, true);
            CheckTrack(result, rows, buildResult.FindFaMenBuild("famen:lihuo"), "famen:lihuo", "faMen", 2, true, false, false);
            CheckTrack(result, rows, buildResult.FindQiLeiBuild("qilei:fu"), "qilei:fu", "qiLei", 4, true, true, false);
            CheckTrack(result, rows, buildResult.FindQiLeiBuild("qilei:yin"), "qilei:yin", "qiLei", 1, false, false, false);
            result.Notes.Add("Build tracks checked: faMen Build2/4/6 and qiLei Build2/4 resolve from lit, non-JuNian item counts.");
        }

        private static void CheckTrack(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildTrackResult track,
            string buildId,
            string category,
            int expectedCount,
            bool expectedBuild2,
            bool expectedBuild4,
            bool expectedBuild6)
        {
            bool passed = track != null
                && track.litItemCount == expectedCount
                && track.build2Active == expectedBuild2
                && track.build4Active == expectedBuild4
                && track.build6Active == expectedBuild6;
            if (!passed)
            {
                result.Errors.Add($"{buildId} expected count={expectedCount}, B2={expectedBuild2}, B4={expectedBuild4}, B6={expectedBuild6}; actual count={track?.litItemCount}, B2={track?.build2Active}, B4={track?.build4Active}, B6={track?.build6Active}.");
            }

            rows.Add(new BuildSynergySpecRow(
                "track",
                buildId,
                category,
                track?.litItemCount ?? -1,
                track?.build2Active == true,
                track?.build4Active == true,
                track?.build6Active == true,
                string.Empty,
                passed ? "PASS" : "FAIL"));
        }

        private static void CheckCountingExclusions(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult)
        {
            CheckItemCountState(result, rows, buildResult, "P_I031_SOURCE", false, "JuNian source is excluded");
            CheckItemCountState(result, rows, buildResult, "P_I001", true, "Lit valid faMen/qiLei item counts");
            CheckItemCountState(result, rows, buildResult, "P_I001_DUP_BASE", false, "Duplicate base item is excluded");
            CheckItemCountState(result, rows, buildResult, "P_I009_UNLIT", false, "Unlit item is excluded");
            result.Notes.Add("Counting exclusions checked: only isLit=true, unique-base, non-JuNian, valid faMenTag/qiLeiTag placements count.");
        }

        private static void CheckItemCountState(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult,
            string placementId,
            bool expectedCounted,
            string caseId)
        {
            ItemBuildSynergyItemResult item = buildResult.FindPlacementResult(placementId);
            bool passed = item != null && item.countedInBuild == expectedCounted;
            if (!passed)
            {
                result.Errors.Add($"{placementId} expected countedInBuild={expectedCounted}; actual {item?.countedInBuild}.");
            }

            rows.Add(new BuildSynergySpecRow(
                caseId,
                placementId,
                "item",
                item?.countedInBuild == true ? 1 : 0,
                item?.faMenActiveStagePieceCount >= 2,
                item?.faMenActiveStagePieceCount >= 4,
                item?.faMenActiveStagePieceCount >= 6,
                item?.qiLeiBuildId ?? string.Empty,
                passed ? "PASS" : "FAIL"));
        }

        private static void CheckValidationScenarios(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult)
        {
            bool duplicatePassed = buildResult.ValidationErrors.Any(error => ContainsOrdinal(error, "duplicate placementId 'P_I001'"))
                && buildResult.ItemResults.Count(item => item.placementId == "P_I001") == 1
                && buildResult.FindFaMenBuild("famen:zhenlei")?.litItemCount == 6;
            if (!duplicatePassed)
            {
                result.Errors.Add("Duplicate placementId validation/dedup failed for P_I001.");
            }

            bool duplicateBasePassed = buildResult.ValidationErrors.Any(error => ContainsOrdinal(error, "DUPLICATE_BASE_ITEM_PLACED")
                    && ContainsOrdinal(error, "P_I001_DUP_BASE")
                    && ContainsOrdinal(error, "P_I001"))
                && buildResult.FindPlacementResult("P_I001_DUP_BASE")?.countedInBuild == false
                && buildResult.FindFaMenBuild("famen:zhenlei")?.litItemCount == 6;
            if (!duplicateBasePassed)
            {
                result.Errors.Add("Duplicate base item validation/count exclusion failed for P_I001_DUP_BASE.");
            }

            bool missingCatalogPassed = buildResult.ValidationErrors.Any(error => ContainsOrdinal(error, "missing catalog item"))
                && buildResult.FindPlacementResult("P_MISSING_CATALOG")?.countedInBuild == false;
            if (!missingCatalogPassed)
            {
                result.Errors.Add("Missing catalog validation failed for P_MISSING_CATALOG.");
            }

            bool emptyPlacementPassed = buildResult.ValidationErrors.Any(error => ContainsOrdinal(error, "placementId is empty"))
                && !buildResult.ItemResults.Any(item => string.IsNullOrWhiteSpace(item.placementId));
            if (!emptyPlacementPassed)
            {
                result.Errors.Add("Empty placementId validation failed.");
            }

            ItemBuildSynergyResolutionResult invalidTagResult = ItemBuildSynergyResolver.Resolve(
                new ItemLightingResolutionResult(
                    Array.Empty<Vector2Int>(),
                    new[] { Lit("I_BAD_TAG", "Invalid tag sample", "P_BAD_TAG", false, true, "P_I031_SOURCE", 0) }),
                new[]
                {
                    new ItemInnerDataDefinition
                    {
                        itemId = "I_BAD_TAG",
                        displayName = "Invalid tag sample",
                        itemFamily = ItemFamilyTag.FaMenCombat,
                        faMenTag = ItemFaMenTag.Zhonggong,
                        qiLeiTag = ItemQiLeiTag.ZhonggongQi,
                        isLightingSource = false
                    }
                });
            bool invalidTagPassed = invalidTagResult.ValidationErrors.Any(error => ContainsOrdinal(error, "invalid faMenTag"))
                && invalidTagResult.ValidationErrors.Any(error => ContainsOrdinal(error, "invalid qiLeiTag"))
                && invalidTagResult.FindPlacementResult("P_BAD_TAG")?.countedInBuild == false;
            if (!invalidTagPassed)
            {
                result.Errors.Add("Invalid faMenTag/qiLeiTag validation failed for P_BAD_TAG.");
            }

            rows.Add(new BuildSynergySpecRow("duplicatePlacementDedup", "P_I001", "validation", buildResult.FindFaMenBuild("famen:zhenlei")?.litItemCount ?? -1, duplicatePassed, duplicatePassed, duplicatePassed, "duplicate ignored; count unchanged", duplicatePassed ? "PASS" : "FAIL"));
            rows.Add(new BuildSynergySpecRow("duplicateBaseItemExcluded", "P_I001_DUP_BASE", "validation", buildResult.FindFaMenBuild("famen:zhenlei")?.litItemCount ?? -1, duplicateBasePassed, duplicateBasePassed, duplicateBasePassed, "DUPLICATE_BASE_ITEM_PLACED; duplicate base excluded; legal I001-I006 count remains 6", duplicateBasePassed ? "PASS" : "FAIL"));
            rows.Add(new BuildSynergySpecRow("missingCatalog", "P_MISSING_CATALOG", "validation", 0, missingCatalogPassed, missingCatalogPassed, missingCatalogPassed, "validationErrors includes missing catalog", missingCatalogPassed ? "PASS" : "FAIL"));
            rows.Add(new BuildSynergySpecRow("emptyPlacementId", "empty", "validation", 0, emptyPlacementPassed, emptyPlacementPassed, emptyPlacementPassed, "empty placement ignored", emptyPlacementPassed ? "PASS" : "FAIL"));
            rows.Add(new BuildSynergySpecRow("invalidTags", "P_BAD_TAG", "validation", 0, invalidTagPassed, invalidTagPassed, invalidTagPassed, "invalid faMenTag/qiLeiTag excluded", invalidTagPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Validation checked: duplicate placementId is deduped; duplicate base item is reported and excluded; empty placementId, missing catalog, and invalid tags are reported in validationErrors.");
        }

        private static void CheckNoMainBuildAndNoMonitorSlots(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult)
        {
            bool selectedMainPassed = string.IsNullOrWhiteSpace(buildResult.selectedMainBuildId);
            if (!selectedMainPassed)
            {
                result.Errors.Add($"selectedMainBuildId should remain empty in this package; actual {buildResult.selectedMainBuildId}.");
            }

            bool monitorMemberAbsent = typeof(ItemBuildSynergyResolutionResult).GetMember("MonitorSlots").Length == 0;
            bool monitorTypeAbsent = typeof(ItemBuildSynergyResolutionResult).Assembly.GetType("TalismanBag.Items.Build.ItemSkillMonitorSlot") == null;
            bool monitorPassed = monitorMemberAbsent && monitorTypeAbsent;
            if (!monitorPassed)
            {
                result.Errors.Add("BuildSynergyCore01 must not expose or generate MonitorSlots / ItemSkillMonitorSlot.");
            }

            rows.Add(new BuildSynergySpecRow("selectedMainBuildReserved", "selectedMainBuildId", "readonlySnapshot", 0, selectedMainPassed, selectedMainPassed, selectedMainPassed, "reserved empty", selectedMainPassed ? "PASS" : "FAIL"));
            rows.Add(new BuildSynergySpecRow("monitorSlotsNotGenerated", "MonitorSlots", "readonlySnapshot", 0, monitorPassed, monitorPassed, monitorPassed, "no MonitorSlots member or ItemSkillMonitorSlot type", monitorPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Main Build and MonitorSlots checked: selectedMainBuildId remains empty and no monitor slot type/member is generated.");
        }

        private static void CheckViewModelProjection(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemLightingResolutionResult lightingResult,
            ItemBuildSynergyResolutionResult buildResult)
        {
            ItemLightingItemResult lightingItem = lightingResult.FindPlacementResult("P_I001");
            ItemInnerDataCatalogProvider provider = new GameObject("TempBuildSynergyCatalogProvider").AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemDetailViewModel baseModel = provider.GetDetailViewModel("I001");
                ItemDetailViewModel projected = ItemSandboxLightingDetailProjection.Project(baseModel, lightingItem, null, buildResult);
                bool hasCountStat = projected != null
                    && projected.displayPrimaryStats.Any(line => line != null && line.label == "faMenBuildCount" && line.value == "6");
                bool hasValidationStat = projected != null
                    && projected.displayPrimaryStats.Any(line => line != null && line.label == "buildValidationErrors");
                bool hasForbiddenMainOrMonitorStat = projected != null
                    && projected.displayPrimaryStats.Any(line => line != null
                        && (line.label == "mainBuildId"
                            || line.label == "mainBuildStage"
                            || line.label == "skillMonitorSlots"));
                bool passed = projected != null
                    && projected.placementId == "P_I001"
                    && projected.statusFlags.countedInBuild
                    && projected.buildPreview.countedInBuild
                    && projected.displayFaMenBuilds.Count > 0
                    && projected.displayQiLeiBuilds.Count > 0
                    && hasCountStat
                    && hasValidationStat
                    && !hasForbiddenMainOrMonitorStat;
                if (!passed)
                {
                    result.Errors.Add("ItemDetailViewModel Build projection failed for P_I001.");
                }

                rows.Add(new BuildSynergySpecRow("detailProjection", "P_I001", "viewModel", hasCountStat ? 6 : -1, projected?.statusFlags.countedInBuild == true, projected?.displayFaMenBuilds.Any(build => build.isActivePreview) == true, !hasForbiddenMainOrMonitorStat, "faMen/qiLei progress only; validationErrors visible", passed ? "PASS" : "FAIL"));
                result.Notes.Add("ItemDetailViewModel projection checked: countedInBuild and faMen/qiLei progress are visible without mainBuild or monitor-slot projection.");
            }
            finally
            {
                Object.DestroyImmediate(provider.gameObject);
            }
        }

        private static void CheckSnapshotInterface(VerificationResult result, List<BuildSynergySpecRow> rows)
        {
            bool passed = typeof(IItemBuildSynergySnapshotProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            if (!passed)
            {
                result.Errors.Add("ItemSandboxGridPlacementPreviewView does not expose IItemBuildSynergySnapshotProvider.");
            }

            rows.Add(new BuildSynergySpecRow("snapshotProvider", "IItemBuildSynergySnapshotProvider", "interface", passed ? 1 : 0, passed, passed, passed, "ItemSandboxGridPlacementPreviewView", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Snapshot interface checked: future packages can request read-only Build track and placement results.");
        }

        private static void CheckOverview(
            VerificationResult result,
            List<BuildSynergySpecRow> rows,
            ItemBuildSynergyResolutionResult buildResult)
        {
            string overview = ItemBuildSynergyResolver.FormatOverview(buildResult);
            bool passed = overview.Contains("FaMen")
                && overview.Contains("QiLei")
                && overview.Contains("SelectedMainBuildId")
                && overview.Contains("ValidationErrors")
                && overview.Contains("lit non-JuNian")
                && !overview.Contains("MonitorSlots");
            if (!passed)
            {
                result.Errors.Add("Build overview text is missing expected greybox summary fields.");
            }

            rows.Add(new BuildSynergySpecRow("greyboxOverview", "BuildOverview", "ui", buildResult.FaMenBuilds.Count + buildResult.QiLeiBuilds.Count, passed, passed, passed, overview.Replace(Environment.NewLine, " / "), passed ? "PASS" : "FAIL"));
            result.Notes.Add("Greybox overview checked: status text summarizes faMen, qiLei, selectedMainBuildId reserved state, and validationErrors.");
        }

        private static void CheckNullSnapshot(VerificationResult result, List<BuildSynergySpecRow> rows)
        {
            ItemBuildSynergyResolutionResult nullResult = ItemBuildSynergyResolver.Resolve(null, ItemInnerDataCatalog.AllItems);
            bool passed = nullResult != null
                && nullResult.ItemResults.Count == 0
                && nullResult.FaMenBuilds.Count == 5
                && nullResult.QiLeiBuilds.Count == 5
                && nullResult.ValidationErrors.Any(error => ContainsOrdinal(error, "lightingResult is null"));
            if (!passed)
            {
                result.Errors.Add("Null lighting snapshot validation failed.");
            }

            rows.Add(new BuildSynergySpecRow("nullLightingSnapshot", "lightingResult", "validation", 0, passed, passed, passed, "validationErrors includes null snapshot", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Null snapshot checked: resolver emits empty track data and validationErrors instead of throwing.");
        }

        private static void CheckSourceScope(VerificationResult result)
        {
            foreach (string sourcePath in ScopedSourcePaths)
            {
                if (!File.Exists(sourcePath))
                {
                    result.Errors.Add($"Missing source file: {sourcePath}");
                    continue;
                }

                if (sourcePath.EndsWith("BuildSynergyCoreVerifier.cs", StringComparison.Ordinal))
                {
                    continue;
                }

                string content = File.ReadAllText(sourcePath, Encoding.UTF8);
                foreach (string forbiddenToken in ForbiddenSourceTokens)
                {
                    if (content.Contains(forbiddenToken))
                    {
                        result.Errors.Add($"Forbidden source token '{forbiddenToken}' found in {sourcePath}.");
                    }
                }
            }

            result.Notes.Add("Source scope check completed: Build synergy files stay inside Items / ItemSandbox and do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.");
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<BuildSynergySpecRow> rows)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            UTF8Encoding utf8WithBom = new(true);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), utf8WithBom);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), utf8WithBom);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), utf8WithBom);
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BuildSynergyCore01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `BuildSynergyCore01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_BUILDSYNERGYCORE01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox only.");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Lit-only faMen Build count for valid non-JuNian placements.");
            builder.AppendLine("- Lit-only qiLei Build count for valid non-JuNian placements.");
            builder.AppendLine("- faMen stages: Build2 / Build4 / Build6.");
            builder.AppendLine("- qiLei stages: Build2 / Build4.");
            builder.AppendLine("- ItemDetailViewModel projection for count, stage, progress, and validationErrors.");
            builder.AppendLine("- Greybox overview and read-only `IItemBuildSynergySnapshotProvider` interface.");
            builder.AppendLine("- placementId dedupe plus baseItemId/itemId uniqueness validation; duplicate base items are excluded from Build counts.");
            builder.AppendLine("- validationErrors for duplicate base, duplicate/empty placementId, missing catalog, invalid tag, and null snapshot cases.");
            builder.AppendLine("- No selected main Build, MonitorSlots, Build effects, numeric bonuses, skill releases, or settlement logic.");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            builder.AppendLine("## Modified Files");
            foreach (string sourcePath in ModifiedFilePaths)
            {
                builder.AppendLine($"- `{sourcePath}`");
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<BuildSynergySpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,buildId,category,litCount,build2Active,build4Active,build6Active,note,actualResult");
            foreach (BuildSynergySpecRow row in rows)
            {
                string[] values =
                {
                    row.caseId,
                    row.buildId,
                    row.category,
                    row.litCount.ToString(),
                    row.build2Active ? "true" : "false",
                    row.build4Active ? "true" : "false",
                    row.build6Active ? "true" : "false",
                    row.note,
                    row.actualResult
                };
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BuildSynergyCore01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox Build synergy state resolver and detail projection.");
            builder.AppendLine("- BuildSettings: no writes; Item Sandbox remains manual-only.");
            builder.AppendLine("- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, and BuildSettings writers.");
            builder.AppendLine("- Not implemented: selected main Build, MonitorSlots, Build effects, combat settlement, skill execution, numeric multipliers, save serialization, rewards, drops, or boss logic.");
            builder.AppendLine("- Known unrelated dirty file intentionally untouched: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.");
            builder.AppendLine();
            AppendResultList(builder, "Passed Checks / Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static void AppendResultList(StringBuilder builder, string title, IReadOnlyList<string> values)
        {
            builder.AppendLine($"## {title}");
            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- None");
                builder.AppendLine();
                return;
            }

            foreach (string value in values)
            {
                builder.AppendLine($"- {value}");
            }

            builder.AppendLine();
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool needsEscape = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            return needsEscape ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private static bool ContainsOrdinal(string value, string expected)
        {
            return value != null
                && expected != null
                && value.IndexOf(expected, StringComparison.Ordinal) >= 0;
        }

        private sealed class BuildSynergySpecRow
        {
            public BuildSynergySpecRow(
                string caseId,
                string buildId,
                string category,
                int litCount,
                bool build2Active,
                bool build4Active,
                bool build6Active,
                string note,
                string actualResult)
            {
                this.caseId = caseId;
                this.buildId = buildId;
                this.category = category;
                this.litCount = litCount;
                this.build2Active = build2Active;
                this.build4Active = build4Active;
                this.build6Active = build6Active;
                this.note = note;
                this.actualResult = actualResult;
            }

            public readonly string caseId;
            public readonly string buildId;
            public readonly string category;
            public readonly int litCount;
            public readonly bool build2Active;
            public readonly bool build4Active;
            public readonly bool build6Active;
            public readonly string note;
            public readonly string actualResult;
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }
    }
}
#endif
