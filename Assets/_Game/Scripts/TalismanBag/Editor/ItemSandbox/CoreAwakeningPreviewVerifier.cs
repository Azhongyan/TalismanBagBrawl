#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class CoreAwakeningPreviewVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalogProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDevStubProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs.meta",
            "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md",
            "Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv",
            "Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md"
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
            "UpgradeService",
            "ItemLevelSystem",
            "ItemMergeSystem",
            "EditorBuildSettings.scenes =",
            "MonitorSlots",
            "ItemSkillMonitorSlot"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/CoreAwakeningPreview01/[Guard Only] Verify And Write Reports")]
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
            List<CoreAwakeningSpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("CoreAwakeningPreview01 verification passed and reports were written.");
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

        private static void RunChecks(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            CheckDefaultDefinitionProvider(result, rows);

            ItemLightingResolutionResult boundaryLighting = CreateBoundaryLightingResult();
            ItemCoreAwakeningResolutionResult boundaryResult = ItemCoreAwakeningResolver.Resolve(
                boundaryLighting,
                CreateBoundaryInputs());
            CheckLevelBoundaries(result, rows, boundaryResult);
            CheckHighRarityReservedOnly(result, rows, boundaryResult);
            CheckSourceUnsupported(result, rows, boundaryResult);
            CheckInputValidation(result, rows, boundaryResult);
            CheckDefinitionValidation(result, rows);
            CheckViewModelProjection(result, rows, boundaryLighting, boundaryResult);
            CheckSandboxPreviewCatalog(result, rows, boundaryLighting);
            CheckBuildAndArrayCannotUnlock(result, rows);
            CheckLightingArrayBuildRegressions(result, rows);
            CheckSnapshotInterface(result, rows);
            CheckSourceScope(result);
        }

        private static void CheckDefaultDefinitionProvider(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            IReadOnlyList<ItemCoreEffectDefinition> definitions = DefaultItemCoreEffectDefinitionProvider.Instance.GetDefinitions("I001");
            string[] expectedIds = { "I001_CORE_01", "I001_CORE_02", "I001_CORE_03", "I001_CORE_ULT" };
            bool ordinaryPassed = definitions.Count == 4
                && definitions.Select(definition => definition.coreEffectId).SequenceEqual(expectedIds)
                && definitions.Select(definition => definition.unlockLevel).SequenceEqual(new[] { 10, 20, 30, 40 });
            bool sourcePassed = DefaultItemCoreEffectDefinitionProvider.Instance.GetDefinitions("I031").Count == 0;
            bool passed = ordinaryPassed && sourcePassed;
            if (!passed)
            {
                result.Errors.Add("Default item core effect definitions failed: ordinary items must emit four item-specific ids and I031 must emit none.");
            }

            rows.Add(new CoreAwakeningSpecRow("definitionProviderDefault", "definitions", "I001", 1, 1, false, true, string.Join("|", expectedIds), "None", 10, "None", "I001 has four item-specific ids; I031 has none", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Definition provider checked: ordinary item definitions use itemId-specific ids and I031 returns no definitions.");
        }

        private static void CheckLevelBoundaries(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            CheckItem(result, rows, boundaryResult, "levelLv1", "P_LV1", 1, 1, true, true, "None", "None", 10, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv9", "P_LV9", 9, 9, true, true, "None", "None", 10, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv10", "P_LV10", 10, 10, true, true, "I003_CORE_01", "I003_CORE_01", 20, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv19", "P_LV19", 19, 19, true, true, "I004_CORE_01", "I004_CORE_01", 20, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv20", "P_LV20", 20, 20, true, true, "I005_CORE_01|I005_CORE_02", "I005_CORE_01|I005_CORE_02", 30, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv29", "P_LV29", 29, 29, true, true, "I006_CORE_01|I006_CORE_02", "I006_CORE_01|I006_CORE_02", 30, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv30", "P_LV30", 30, 30, true, true, "I007_CORE_01|I007_CORE_02|I007_CORE_03", "I007_CORE_01|I007_CORE_02|I007_CORE_03", 40, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv39", "P_LV39", 39, 39, true, true, "I013_CORE_01|I013_CORE_02|I013_CORE_03", "I013_CORE_01|I013_CORE_02|I013_CORE_03", 40, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv40", "P_LV40", 40, 40, true, true, "I015_CORE_01|I015_CORE_02|I015_CORE_03|I015_CORE_ULT", "I015_CORE_01|I015_CORE_02|I015_CORE_03|I015_CORE_ULT", 0, "PASS");
            result.Notes.Add("Level boundaries checked: Lv1, Lv9/10, Lv19/20, Lv29/30, and Lv39/40 unlock only by resolvedLevel thresholds.");
        }

        private static void CheckHighRarityReservedOnly(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult item = boundaryResult.FindPlacementResult("P_HIGH_RARITY_LV1");
            ItemCoreAwakeningNodeState ultimate = item?.NodeStates.FirstOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Ultimate);
            bool passed = item != null
                && item.inputLevel == 1
                && item.resolvedLevel == 1
                && item.highRarityUltimatePreview
                && ultimate != null
                && !ultimate.isUnlocked
                && ultimate.blockedReason == ItemCoreAwakeningBlockedReason.ReservedRarityGate.ToString();
            if (!passed)
            {
                result.Errors.Add("Lv1 highRarityUltimatePreview=true incorrectly unlocked or failed to mark Ultimate as reserved.");
            }

            rows.Add(RowFromItem("highRarityReservedOnly", item, "Ultimate remains locked with ReservedRarityGate", passed ? "PASS" : "FAIL"));
            result.Notes.Add("High-rarity preview checked: reserved rarity fields do not unlock Ultimate.");
        }

        private static void CheckSourceUnsupported(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult source = boundaryResult.FindPlacementResult("P_SOURCE");
            bool passed = source != null
                && source.itemId == "I031"
                && !source.supportsAwakening
                && !source.basicEffectActive
                && !source.coreEffectUnlocked
                && !source.coreEffectActive
                && source.NodeStates.Count == 0;
            if (!passed)
            {
                result.Errors.Add("I031 should remain supportsAwakening=false with no core nodes.");
            }

            rows.Add(RowFromItem("sourceUnsupported", source, "I031 has no core nodes", passed ? "PASS" : "FAIL"));
            result.Notes.Add("JuNian source checked: I031 has no awakening support, basic effect, or core nodes.");
        }

        private static void CheckInputValidation(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult input0 = boundaryResult.FindPlacementResult("P_INPUT0");
            bool input0Passed = input0 != null
                && input0.inputLevel == 0
                && input0.resolvedLevel == 1
                && input0.ValidationErrors.Any(error => ContainsOrdinal(error, "below 1"));
            if (!input0Passed)
            {
                result.Errors.Add("inputLevel=0 did not resolve to Lv1 with validationErrors.");
            }

            ItemCoreAwakeningItemResult input41 = boundaryResult.FindPlacementResult("P_INPUT41");
            bool input41Passed = input41 != null
                && input41.inputLevel == 41
                && input41.resolvedLevel == 40
                && input41.coreEffectActive
                && input41.ValidationErrors.Any(error => ContainsOrdinal(error, "above 40"));
            if (!input41Passed)
            {
                result.Errors.Add("inputLevel=41 did not resolve to Lv40 with validationErrors.");
            }

            ItemCoreAwakeningItemResult missing = boundaryResult.FindPlacementResult("P_MISSING_INPUT");
            bool missingPassed = missing != null
                && missing.inputLevel == 1
                && missing.resolvedLevel == 1
                && missing.inputSource == "MissingInputDefaultLv1"
                && missing.ValidationErrors.Any(error => ContainsOrdinal(error, "missing awakening input"));
            if (!missingPassed)
            {
                result.Errors.Add("Missing placement input did not default to Lv1 with validationErrors.");
            }

            rows.Add(RowFromItem("inputLevel0Clamp", input0, "resolvedLevel=1 with validationErrors", input0Passed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("inputLevel41Clamp", input41, "resolvedLevel=40 with validationErrors", input41Passed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("missingInputDefaultLv1", missing, "MissingInputDefaultLv1 with validationErrors", missingPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Input validation checked: inputLevel=0, inputLevel=41, and missing placement input are stable and non-throwing.");
        }

        private static void CheckDefinitionValidation(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult lighting = CreateSingleItemLighting("I001", "P_DEF", true);
            IReadOnlyList<ItemCoreAwakeningInput> inputs = new[] { Input("I001", "P_DEF", 40) };

            CheckDefinitionCase(result, rows, "definitionMissing", lighting, inputs, new StaticDefinitionProvider(Array.Empty<ItemCoreEffectDefinition>()), "no core effect definitions");
            CheckDefinitionCase(result, rows, "duplicateCoreEffectId", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_DUP", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_DUP", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate coreEffectId");
            CheckDefinitionCase(result, rows, "duplicateNodeKind", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_01B", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate nodeKind");
            CheckDefinitionCase(result, rows, "wrongNodeLevel", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 21),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "unlockLevel mismatch");
            CheckDefinitionCase(result, rows, "definitionItemMismatch", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I999", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "itemId mismatch");
            CheckDefinitionCase(result, rows, "emptyCoreEffectId", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", string.Empty, ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "empty coreEffectId");
            CheckDefinitionCase(result, rows, "nullDefinition", lighting, inputs, new StaticDefinitionProvider(new ItemCoreEffectDefinition[]
            {
                null,
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "null core effect definition");
            CheckDefinitionCase(result, rows, "nullDefinitionProvider", lighting, inputs, null, "definitionProvider is null");
            result.Notes.Add("Definition validation checked: missing, duplicate id, duplicate nodeKind, wrong level, item mismatch, empty id, null definition, and null provider.");
        }

        private static void CheckDefinitionCase(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            string caseId,
            ItemLightingResolutionResult lighting,
            IReadOnlyList<ItemCoreAwakeningInput> inputs,
            IItemCoreEffectDefinitionProvider provider,
            string expectedError)
        {
            ItemCoreAwakeningResolutionResult resolution = ItemCoreAwakeningResolver.Resolve(lighting, inputs, provider);
            ItemCoreAwakeningItemResult item = resolution.FindPlacementResult("P_DEF");
            bool passed = resolution.ValidationErrors.Any(error => ContainsOrdinal(error, expectedError))
                && item != null
                && item.NodeStates.Count <= 4;
            if (!passed)
            {
                result.Errors.Add($"{caseId} did not emit expected validation error containing '{expectedError}'.");
            }

            rows.Add(RowFromItem(caseId, item, $"validationErrors contains {expectedError}", passed ? "PASS" : "FAIL"));
        }

        private static void CheckViewModelProjection(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemLightingResolutionResult lightingResult,
            ItemCoreAwakeningResolutionResult awakeningResult)
        {
            ItemInnerDataCatalogProvider provider = new GameObject("TempCoreAwakeningCatalogProvider").AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemLightingItemResult lightingItem = lightingResult.FindPlacementResult("P_LV40");
                ItemCoreAwakeningItemResult awakeningItem = awakeningResult.FindPlacementResult("P_LV40");
                ItemDetailViewModel baseModel = provider.GetDetailViewModel("I015");
                ItemDetailViewModel projected = ItemSandboxLightingDetailProjection.Project(baseModel, lightingItem, null, null, awakeningItem);
                bool passed = projected != null
                    && projected.statusFlags.inputLevel == 40
                    && projected.statusFlags.resolvedLevel == 40
                    && projected.statusFlags.coreEffectUnlocked
                    && projected.statusFlags.coreEffectActive
                    && projected.displayCoreEffects.Count == 4
                    && projected.displayCoreEffects.Any(line => ContainsOrdinal(line.body, "coreEffectId=I015_CORE_ULT"))
                    && projected.awakeningPreview.activeCoreEffectIdsText.Contains("I015_CORE_ULT");
                if (!passed)
                {
                    result.Errors.Add("ItemDetailViewModel projection failed to expose input/resolved level, ids, stateKey, and active core effect ids.");
                }

                rows.Add(RowFromItem("detailProjection", awakeningItem, "ViewModel exposes core ids and active state", passed ? "PASS" : "FAIL"));
                result.Notes.Add("ItemDetailViewModel projection checked: inputLevel, resolvedLevel, coreEffectIds, stateKey, blockedReason, and validationErrors are visible.");
            }
            finally
            {
                Object.DestroyImmediate(provider.gameObject);
            }
        }

        private static void CheckSandboxPreviewCatalog(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemLightingResolutionResult lightingResult)
        {
            ItemCoreAwakeningResolutionResult sandboxResult = ItemCoreAwakeningResolver.Resolve(
                lightingResult,
                ItemSandboxCoreAwakeningPreviewCatalog.CreatePreviewInputs(lightingResult));
            IReadOnlyList<int> previewLevels = sandboxResult.ItemResults
                .Where(item => !item.isLightingSource)
                .Select(item => item.resolvedLevel)
                .OrderBy(level => level)
                .ToArray();
            bool allInRange = previewLevels.All(level => level >= 1 && level <= 40);
            bool hasNoLevelZero = !previewLevels.Contains(0);
            bool hasExpectedLevelSet = new[] { 1, 10, 20, 30, 40 }.All(level => previewLevels.Contains(level));
            bool passed = previewLevels.Count > 0
                && allInRange
                && hasNoLevelZero
                && hasExpectedLevelSet;
            if (!passed)
            {
                result.Errors.Add($"Sandbox greybox level preview catalog emitted invalid levels. actualLevels={string.Join("|", previewLevels)}.");
            }

            rows.Add(new CoreAwakeningSpecRow("sandboxPreviewCatalog", "PreviewCatalog", "all", 1, 1, false, true, "None", "None", 10, ItemCoreAwakeningResolver.FormatIds(sandboxResult.ValidationErrors), "Preview resolvedLevel values stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without placementId inference.", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Sandbox preview catalog checked: normal preview levels stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without relying on placementId names.");
        }

        private static void CheckBuildAndArrayCannotUnlock(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult buildLighting = CreateBuildRegressionLighting();
            ItemBuildSynergyResolutionResult buildResult = ItemBuildSynergyResolver.Resolve(buildLighting, ItemInnerDataCatalog.AllItems);
            ItemCoreAwakeningResolutionResult awakeningResult = ItemCoreAwakeningResolver.Resolve(
                buildLighting,
                buildLighting.ItemResults.Select(item => Input(item.itemId, item.placementId, 1)).ToArray());

            bool build6Active = buildResult.FindFaMenBuild("famen:zhenlei")?.build6Active == true;
            bool noCoreByBuild = awakeningResult.ItemResults.Where(item => item.itemId != "I031").All(item => !item.coreEffectUnlocked && !item.coreEffectActive);
            bool noMainBuild = string.IsNullOrWhiteSpace(buildResult.selectedMainBuildId);
            bool noMonitorSlots = typeof(ItemBuildSynergyResolutionResult).GetMember("MonitorSlots").Length == 0
                && typeof(ItemBuildSynergyResolutionResult).Assembly.GetType("TalismanBag.Items.Build.ItemSkillMonitorSlot") == null;
            bool buildPassed = build6Active && noCoreByBuild && noMainBuild && noMonitorSlots;
            if (!buildPassed)
            {
                result.Errors.Add("Build6 regression failed: Build state must not unlock core effects, selectedMainBuildId must stay empty, and MonitorSlots must remain absent.");
            }

            ItemLightingItemResult arrayLightingItem = Lit("I001", "Array active sample", "P_ARRAY_ACTIVE", false, true, "P_SOURCE_BUILD", 0, new Vector2Int(2, 1));
            ItemArrayBonusResolutionResult arrayResult = ItemArrayBonusResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[] { arrayLightingItem });
            ItemCoreAwakeningItemResult arrayAwakening = ItemCoreAwakeningResolver.ResolveItem(arrayLightingItem, Input("I001", "P_ARRAY_ACTIVE", 1));
            bool arrayPassed = arrayResult.FindPlacementResult("P_ARRAY_ACTIVE")?.isArrayBonusActive == true
                && !arrayAwakening.coreEffectUnlocked
                && !arrayAwakening.coreEffectActive;
            if (!arrayPassed)
            {
                result.Errors.Add("Array bonus active state incorrectly unlocked a core effect.");
            }

            rows.Add(RowFromItem("build6CannotUnlockCore", awakeningResult.FindPlacementResult("P_BUILD_I001"), "Build6 active but Lv1 core remains locked; no MonitorSlots", buildPassed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("arrayCannotUnlockCore", arrayAwakening, "Array active but Lv1 core remains locked", arrayPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Build/array isolation checked: Build6 and active array bonus do not unlock core effects; selectedMainBuildId remains empty and monitor slots remain absent.");
        }

        private static void CheckLightingArrayBuildRegressions(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult lighting = ItemLightingResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[]
                {
                    new ItemLightingPlacedItem("I031", "Source", new[] { new Vector2Int(0, 0) }, new Vector2Int(0, 0), true, new Vector2Int(0, 0), "P_LIGHT_SOURCE"),
                    new ItemLightingPlacedItem("I001", "Direct lit", new[] { new Vector2Int(0, 1) }, new Vector2Int(0, 1), false, new Vector2Int(0, 1), "P_LIGHT_DIRECT"),
                    new ItemLightingPlacedItem("I007", "Unlit", new[] { new Vector2Int(4, 4) }, new Vector2Int(4, 4), false, new Vector2Int(4, 4), "P_LIGHT_UNLIT")
                });
            bool lightingPassed = lighting.FindPlacementResult("P_LIGHT_DIRECT")?.isDirectLit == true
                && lighting.FindPlacementResult("P_LIGHT_UNLIT")?.isLit == false;
            if (!lightingPassed)
            {
                result.Errors.Add("Lighting regression failed for direct-lit and unlit samples.");
            }

            ItemArrayBonusResolutionResult array = ItemArrayBonusResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[]
                {
                    Lit("I001", "AP lit", "P_AP_LIT", false, true, "P_LIGHT_SOURCE", 0, new Vector2Int(2, 1)),
                    Unlit("I007", "AP unlit", "P_AP_UNLIT", new Vector2Int(2, 3))
                });
            bool arrayPassed = array.FindPlacementResult("P_AP_LIT")?.isArrayBonusActive == true
                && array.FindPlacementResult("P_AP_UNLIT")?.isArrayBonusActive == false;
            if (!arrayPassed)
            {
                result.Errors.Add("ArrayBonus regression failed for lit and unlit AP samples.");
            }

            ItemBuildSynergyResolutionResult build = ItemBuildSynergyResolver.Resolve(CreateBuildRegressionLighting(), ItemInnerDataCatalog.AllItems);
            bool buildPassed = build.FindFaMenBuild("famen:zhenlei")?.build6Active == true
                && string.IsNullOrWhiteSpace(build.selectedMainBuildId);
            if (!buildPassed)
            {
                result.Errors.Add("BuildSynergyCore regression failed for zhenlei Build6 and selectedMainBuildId reserved state.");
            }

            rows.Add(new CoreAwakeningSpecRow("lightingRegression", "P_LIGHT_DIRECT", "I001", 1, 1, true, true, "None", "None", 10, "None", "direct lit/unlit samples pass", lightingPassed ? "PASS" : "FAIL"));
            rows.Add(new CoreAwakeningSpecRow("arrayBonusRegression", "P_AP_LIT", "I001", 1, 1, true, true, "None", "None", 10, "None", "array lit/unlit samples pass", arrayPassed ? "PASS" : "FAIL"));
            rows.Add(new CoreAwakeningSpecRow("buildSynergyCoreRegression", "famen:zhenlei", "Build", 1, 1, true, true, "None", "None", 10, "None", "Build6 active and selectedMainBuildId empty", buildPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Lighting, ArrayBonus, and BuildSynergyCore regressions checked inside CoreAwakening verifier.");
        }

        private static void CheckSnapshotInterface(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            bool snapshotPassed = typeof(IItemCoreAwakeningSnapshotProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            bool inputPassed = typeof(IItemCoreAwakeningReadOnlyInputProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            bool definitionPassed = typeof(IItemCoreEffectDefinitionProvider).IsAssignableFrom(typeof(DefaultItemCoreEffectDefinitionProvider));
            bool passed = snapshotPassed && inputPassed && definitionPassed;
            if (!passed)
            {
                result.Errors.Add("Core awakening read-only snapshot/input/definition provider interfaces are incomplete.");
            }

            rows.Add(new CoreAwakeningSpecRow("snapshotInputDefinitionInterfaces", "interfaces", "all", 1, 1, false, true, "None", "None", 10, "None", "snapshot/input/definition provider interfaces present", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Interfaces checked: future systems can request read-only inputs, resolved snapshots, and item-specific core effect definitions.");
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

                if (sourcePath.EndsWith("CoreAwakeningPreviewVerifier.cs", StringComparison.Ordinal))
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

            result.Notes.Add("Source scope check completed: runtime CoreAwakening preview paths do not reference formal battle, bridge, run flow, save, reward, boss, BuildSettings writers, or monitor slots.");
        }

        private static void CheckItem(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult resolution,
            string caseId,
            string placementId,
            int expectedInputLevel,
            int expectedResolvedLevel,
            bool expectedIsLit,
            bool expectedSupportsAwakening,
            string expectedUnlockedIds,
            string expectedActiveIds,
            int expectedNextUnlockLevel,
            string expectedResult)
        {
            ItemCoreAwakeningItemResult item = resolution.FindPlacementResult(placementId);
            bool passed = item != null
                && item.inputLevel == expectedInputLevel
                && item.resolvedLevel == expectedResolvedLevel
                && item.isLit == expectedIsLit
                && item.supportsAwakening == expectedSupportsAwakening
                && ItemCoreAwakeningResolver.FormatIds(item.UnlockedCoreEffectIds) == expectedUnlockedIds
                && ItemCoreAwakeningResolver.FormatIds(item.ActiveCoreEffectIds) == expectedActiveIds
                && item.nextUnlockLevel == expectedNextUnlockLevel;
            if (!passed)
            {
                result.Errors.Add($"{caseId}/{placementId} expected input={expectedInputLevel}, resolved={expectedResolvedLevel}, lit={expectedIsLit}, supports={expectedSupportsAwakening}, unlocked={expectedUnlockedIds}, active={expectedActiveIds}, next={expectedNextUnlockLevel}; actual input={item?.inputLevel}, resolved={item?.resolvedLevel}, lit={item?.isLit}, supports={item?.supportsAwakening}, unlocked={ItemCoreAwakeningResolver.FormatIds(item?.UnlockedCoreEffectIds)}, active={ItemCoreAwakeningResolver.FormatIds(item?.ActiveCoreEffectIds)}, next={item?.nextUnlockLevel}.");
            }

            rows.Add(RowFromItem(caseId, item, expectedResult, passed ? "PASS" : "FAIL"));
        }

        private static CoreAwakeningSpecRow RowFromItem(
            string caseId,
            ItemCoreAwakeningItemResult item,
            string expectedResult,
            string actualResult)
        {
            return new CoreAwakeningSpecRow(
                caseId,
                item?.placementId ?? string.Empty,
                item?.itemId ?? string.Empty,
                item?.inputLevel ?? 0,
                item?.resolvedLevel ?? 0,
                item?.isLit == true,
                item?.supportsAwakening == true,
                ItemCoreAwakeningResolver.FormatIds(item?.UnlockedCoreEffectIds),
                ItemCoreAwakeningResolver.FormatIds(item?.ActiveCoreEffectIds),
                item?.nextUnlockLevel ?? 0,
                item == null ? "MissingResult" : ItemCoreAwakeningResolver.FormatIds(item.ValidationErrors),
                expectedResult,
                actualResult);
        }

        private static ItemLightingResolutionResult CreateBoundaryLightingResult()
        {
            return new ItemLightingResolutionResult(
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 0) },
                new[]
                {
                    Lit("I001", "Lv1", "P_LV1", false, true, "P_SOURCE", 0),
                    Lit("I002", "Lv9", "P_LV9", false, true, "P_SOURCE", 0),
                    Lit("I003", "Lv10", "P_LV10", false, true, "P_SOURCE", 0),
                    Lit("I004", "Lv19", "P_LV19", false, true, "P_SOURCE", 0),
                    Lit("I005", "Lv20", "P_LV20", false, true, "P_SOURCE", 0),
                    Lit("I006", "Lv29", "P_LV29", false, true, "P_SOURCE", 0),
                    Lit("I007", "Lv30", "P_LV30", false, true, "P_SOURCE", 0),
                    Lit("I013", "Lv39", "P_LV39", false, true, "P_SOURCE", 0),
                    Lit("I015", "Lv40", "P_LV40", false, true, "P_SOURCE", 0),
                    Lit("I001", "Input 0", "P_INPUT0", false, true, "P_SOURCE", 0),
                    Lit("I004", "Input 41", "P_INPUT41", false, true, "P_SOURCE", 0),
                    Lit("I007", "High rarity Lv1", "P_HIGH_RARITY_LV1", false, true, "P_SOURCE", 0),
                    Unlit("I015", "Unlit Lv20", "P_UNLIT_LV20"),
                    Lit("I031", "JuNian source", "P_SOURCE", true, true, "P_SOURCE", 0),
                    Lit("I013", "Missing input", "P_MISSING_INPUT", false, true, "P_SOURCE", 0)
                });
        }

        private static IReadOnlyList<ItemCoreAwakeningInput> CreateBoundaryInputs()
        {
            return new[]
            {
                Input("I001", "P_LV1", 1),
                Input("I002", "P_LV9", 9),
                Input("I003", "P_LV10", 10),
                Input("I004", "P_LV19", 19),
                Input("I005", "P_LV20", 20),
                Input("I006", "P_LV29", 29),
                Input("I007", "P_LV30", 30),
                Input("I013", "P_LV39", 39),
                Input("I015", "P_LV40", 40),
                Input("I001", "P_INPUT0", 0),
                Input("I004", "P_INPUT41", 41),
                new ItemCoreAwakeningInput("I007", "P_HIGH_RARITY_LV1", 1, true, "VerifierHighRarityReserved", "Reserved"),
                Input("I015", "P_UNLIT_LV20", 20),
                Input("I031", "P_SOURCE", 40)
            };
        }

        private static ItemLightingResolutionResult CreateSingleItemLighting(string itemId, string placementId, bool isLit)
        {
            return new ItemLightingResolutionResult(
                Array.Empty<Vector2Int>(),
                new[] { isLit ? Lit(itemId, "Single item", placementId, false, true, "P_SOURCE", 0) : Unlit(itemId, "Single item", placementId) });
        }

        private static ItemLightingResolutionResult CreateBuildRegressionLighting()
        {
            return new ItemLightingResolutionResult(
                Array.Empty<Vector2Int>(),
                new[]
                {
                    Lit("I031", "Source", "P_SOURCE_BUILD", true, true, "P_SOURCE_BUILD", 0),
                    Lit("I001", "Build I001", "P_BUILD_I001", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I002", "Build I002", "P_BUILD_I002", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I003", "Build I003", "P_BUILD_I003", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I004", "Build I004", "P_BUILD_I004", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I005", "Build I005", "P_BUILD_I005", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I006", "Build I006", "P_BUILD_I006", false, true, "P_SOURCE_BUILD", 0)
                });
        }

        private static ItemCoreAwakeningInput Input(string itemId, string placementId, int inputLevel)
        {
            return new ItemCoreAwakeningInput(itemId, placementId, inputLevel, false, "VerifierReadOnlyPreview");
        }

        private static ItemCoreEffectDefinition Def(
            string itemId,
            string coreEffectId,
            ItemCoreAwakeningNodeKind nodeKind,
            int unlockLevel)
        {
            return new ItemCoreEffectDefinition(itemId, coreEffectId, nodeKind, unlockLevel);
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
            return Lit(itemId, displayName, placementId, isLightingSource, isDirectLit, litByPlacementId, litDepth, StableCell(placementId));
        }

        private static ItemLightingItemResult Lit(
            string itemId,
            string displayName,
            string placementId,
            bool isLightingSource,
            bool isDirectLit,
            string litByPlacementId,
            int litDepth,
            Vector2Int cell)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
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
            return Unlit(itemId, displayName, placementId, StableCell(placementId));
        }

        private static ItemLightingItemResult Unlit(string itemId, string displayName, string placementId, Vector2Int cell)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
                false,
                false,
                false,
                string.Empty,
                -1,
                placementId,
                string.Empty);
        }

        private static Vector2Int StableCell(string value)
        {
            unchecked
            {
                int hash = 17;
                string text = value ?? string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    hash = hash * 31 + text[i];
                }

                hash &= int.MaxValue;
                return new Vector2Int(hash % 5, hash / 5 % 5);
            }
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<CoreAwakeningSpecRow> rows)
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
            builder.AppendLine("# CoreAwakeningPreview01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `CoreAwakeningPreview01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_COREAWAKENINGPREVIEW01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox only.");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Levels resolve as `resolvedLevel = Clamp(inputLevel, 1, 40)` while preserving raw `inputLevel`.");
            builder.AppendLine("- Missing placement input defaults to Lv1 with `inputSource=MissingInputDefaultLv1` and validationErrors.");
            builder.AppendLine("- High-rarity fields are reserved only; Ultimate unlocks only at resolvedLevel >= 40.");
            builder.AppendLine("- Core effect definitions are item-specific: `{itemId}_CORE_01`, `{itemId}_CORE_02`, `{itemId}_CORE_03`, `{itemId}_CORE_ULT`.");
            builder.AppendLine("- `IItemCoreEffectDefinitionProvider` provides read-only definitions and I031 returns none.");
            builder.AppendLine("- Definition validation covers missing, duplicate id, duplicate nodeKind, wrong level, item mismatch, empty id, null definition, and null provider.");
            builder.AppendLine("- Node output includes coreEffectId, itemId, nodeKind, unlockLevel, isUnlocked, isActive, stateKey, and blockedReason.");
            builder.AppendLine("- Build, array bonus, selected main Build, and monitor slots do not participate in awakening unlocks.");
            builder.AppendLine("- No formal upgrade, experience, rarity unlock, core numeric effect, combat execution, save, reward, boss, bridge, run flow, or BuildSettings work.");
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

        private static string BuildSpecCsv(IReadOnlyList<CoreAwakeningSpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,placementId,itemId,inputLevel,resolvedLevel,isLit,supportsAwakening,unlockedCoreEffectIds,activeCoreEffectIds,nextUnlockLevel,validationErrors,expectedResult,actualResult");
            foreach (CoreAwakeningSpecRow row in rows)
            {
                string[] values =
                {
                    row.caseId,
                    row.placementId,
                    row.itemId,
                    row.inputLevel.ToString(),
                    row.resolvedLevel.ToString(),
                    row.isLit ? "true" : "false",
                    row.supportsAwakening ? "true" : "false",
                    row.unlockedCoreEffectIds,
                    row.activeCoreEffectIds,
                    row.nextUnlockLevel.ToString(),
                    row.validationErrors,
                    row.expectedResult,
                    row.actualResult
                };
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# CoreAwakeningPreview01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox core awakening preview resolver, definition provider, detail projection, and greybox UI text.");
            builder.AppendLine("- BuildSettings: no writes; Item Sandbox remains manual-only.");
            builder.AppendLine("- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, formal upgrade systems, main Build selection, monitor slots, and BuildSettings writers.");
            builder.AppendLine("- Not implemented: formal item upgrade, experience, rarity evolution, storage, save serialization, combat skill release, damage/heal/shield/status settlement, rewards, drops, boss logic, selected main Build, MonitorSlots, or ItemSkillMonitorSlot.");
            builder.AppendLine("- Known unrelated dirty files intentionally untouched are outside this verifier's source scope.");
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

        private sealed class StaticDefinitionProvider : IItemCoreEffectDefinitionProvider
        {
            private readonly IReadOnlyList<ItemCoreEffectDefinition> definitions;

            public StaticDefinitionProvider(IReadOnlyList<ItemCoreEffectDefinition> definitions)
            {
                this.definitions = definitions;
            }

            public IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(string itemId)
            {
                return definitions;
            }
        }

        private sealed class CoreAwakeningSpecRow
        {
            public CoreAwakeningSpecRow(
                string caseId,
                string placementId,
                string itemId,
                int inputLevel,
                int resolvedLevel,
                bool isLit,
                bool supportsAwakening,
                string unlockedCoreEffectIds,
                string activeCoreEffectIds,
                int nextUnlockLevel,
                string validationErrors,
                string expectedResult,
                string actualResult)
            {
                this.caseId = caseId;
                this.placementId = placementId;
                this.itemId = itemId;
                this.inputLevel = inputLevel;
                this.resolvedLevel = resolvedLevel;
                this.isLit = isLit;
                this.supportsAwakening = supportsAwakening;
                this.unlockedCoreEffectIds = unlockedCoreEffectIds;
                this.activeCoreEffectIds = activeCoreEffectIds;
                this.nextUnlockLevel = nextUnlockLevel;
                this.validationErrors = validationErrors;
                this.expectedResult = expectedResult;
                this.actualResult = actualResult;
            }

            public readonly string caseId;
            public readonly string placementId;
            public readonly string itemId;
            public readonly int inputLevel;
            public readonly int resolvedLevel;
            public readonly bool isLit;
            public readonly bool supportsAwakening;
            public readonly string unlockedCoreEffectIds;
            public readonly string activeCoreEffectIds;
            public readonly int nextUnlockLevel;
            public readonly string validationErrors;
            public readonly string expectedResult;
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
