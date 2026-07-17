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
using TalismanBag.Items.Skills;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemSkillTriggerContractVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemSkillTriggerContractSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemSkillTriggerContractLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Skills/ItemSkillMonitorRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxSkillMonitorDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSkillTriggerContractVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Skills/ItemSkillMonitorRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxSkillMonitorDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSkillTriggerContractVerifier.cs",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity",
            "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md",
            "Docs/V0.4/Reports/ItemSkillTriggerContractSpec.csv",
            "Docs/V0.4/Reports/ItemSkillTriggerContractLeakCheckReport.md"
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
            "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemSkillTriggerContract01/[Guard Only] Verify And Write Reports")]
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
            List<ItemSkillTriggerSpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("ItemSkillTriggerContract01 verification passed and reports were written.");
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

        private static void RunChecks(VerificationResult result, List<ItemSkillTriggerSpecRow> rows)
        {
            CheckMonitorCases(result, rows);
            CheckDetailProjection(result);
            CheckSandboxUiContract(result);
            CheckLightingArrayBuildAwakeningRegressions(result, rows);
            CheckSourceScope(result);
        }

        private static void CheckMonitorCases(VerificationResult result, List<ItemSkillTriggerSpecRow> rows)
        {
            CheckCase(result, rows, "emptySelection", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), string.Empty, 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), Array.Empty<string>());
            CheckCase(result, rows, "validFaMenCount0", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 0)), "famen:zhenlei", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), Array.Empty<string>());
            CheckCase(result, rows, "validFaMenCount1", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 1)), "famen:zhenlei", 1, 0, new[] { ItemSkillMonitorSlotType.BasicAttack }, Array.Empty<string>());
            CheckCase(result, rows, "validFaMenBuild2", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 2)), "famen:zhenlei", 2, 2, new[] { ItemSkillMonitorSlotType.BasicAttack, ItemSkillMonitorSlotType.Build2 }, Array.Empty<string>());
            CheckCase(result, rows, "validFaMenBuild4", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 4)), "famen:zhenlei", 4, 4, new[] { ItemSkillMonitorSlotType.BasicAttack, ItemSkillMonitorSlotType.Build2, ItemSkillMonitorSlotType.Build4 }, Array.Empty<string>());
            CheckCase(result, rows, "validFaMenBuild6", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), "famen:zhenlei", 6, 6, AllSlotTypes(), Array.Empty<string>());
            CheckCase(result, rows, "build6DropsTo5", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 5)), "famen:zhenlei", 5, 4, new[] { ItemSkillMonitorSlotType.BasicAttack, ItemSkillMonitorSlotType.Build2, ItemSkillMonitorSlotType.Build4 }, Array.Empty<string>());
            CheckCase(result, rows, "twoFaMenActiveNoSelection", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6), FaMen("famen:lihuo", "lihuo", "离火法", 4)), string.Empty, 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), Array.Empty<string>());
            CheckCase(result, rows, "explicitLowerStageNoAutoSwitch", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6), FaMen("famen:lihuo", "lihuo", "离火法", 2)), "famen:lihuo", 2, 2, new[] { ItemSkillMonitorSlotType.BasicAttack, ItemSkillMonitorSlotType.Build2 }, Array.Empty<string>());
            CheckCase(result, rows, "selectQiLeiRejected", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6), QiLei("qilei:fu", "fu", "符", 4)), "qilei:fu", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), new[] { "qiLei Build" });
            CheckCase(result, rows, "selectMissingBuildRejected", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), "famen:xuanshui", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), new[] { "was not found" });
            CheckCase(result, rows, "nullBuildSnapshotRejected", null, "famen:zhenlei", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), new[] { "Build snapshot is null" });
            CheckCase(result, rows, "slotOrderFixedUnique", BuildSnapshot(FaMen("famen:taibai", "taibai", "太白法", 6)), "famen:taibai", 6, 6, AllSlotTypes(), Array.Empty<string>(), requireFixedOrder: true);
            CheckCase(result, rows, "triggerAlwaysFalse", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), "famen:zhenlei", 6, 6, AllSlotTypes(), Array.Empty<string>(), requireTriggerFalse: true);
            CheckCase(result, rows, "cooldownChargeAlwaysZero", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), "famen:zhenlei", 6, 6, AllSlotTypes(), Array.Empty<string>(), requireZeroProgress: true);
            CheckStableKeys(result, rows);
            CheckBuildResolverIsolation(result, rows);
            CheckCase(result, rows, "formatInvalidRejected", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)), "zhenlei", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), new[] { "format is invalid" });
            CheckCase(result, rows, "duplicateFaMenTrackRejected", BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6), FaMen("famen:zhenlei", "zhenlei", "震雷法 Duplicate", 4)), "famen:zhenlei", 0, 0, Array.Empty<ItemSkillMonitorSlotType>(), new[] { "duplicate faMen build track" });
        }

        private static void CheckCase(
            VerificationResult result,
            List<ItemSkillTriggerSpecRow> rows,
            string caseId,
            ItemBuildSynergyResolutionResult buildSnapshot,
            string selectedMainBuildId,
            int expectedBuildCount,
            int expectedStage,
            IReadOnlyList<ItemSkillMonitorSlotType> expectedMonitoringSlots,
            IReadOnlyList<string> expectedErrorFragments,
            bool requireFixedOrder = false,
            bool requireTriggerFalse = false,
            bool requireZeroProgress = false)
        {
            ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                buildSnapshot,
                new ItemMainBuildSelectionInput(selectedMainBuildId, "Verifier", 1));
            HashSet<ItemSkillMonitorSlotType> expectedMonitoring = new(expectedMonitoringSlots ?? Array.Empty<ItemSkillMonitorSlotType>());
            IReadOnlyList<string> expectedErrors = expectedErrorFragments ?? Array.Empty<string>();
            bool errorsPassed = expectedErrors.Count == 0
                ? monitor.ValidationErrors.Count == 0
                : expectedErrors.All(fragment => monitor.ValidationErrors.Any(error => ContainsOrdinal(error, fragment)));
            bool countPassed = monitor.selectedMainBuildCount == expectedBuildCount;
            bool stagePassed = monitor.selectedMainBuildStage == expectedStage;
            bool orderPassed = !requireFixedOrder || monitor.Slots.Select(slot => slot.slotType).SequenceEqual(AllSlotTypes());
            bool uniquePassed = !requireFixedOrder || monitor.Slots.Select(slot => slot.slotType).Distinct().Count() == 4;
            bool triggerPassed = !requireTriggerFalse || monitor.Slots.All(slot => !slot.isTriggered);
            bool progressPassed = !requireZeroProgress || monitor.Slots.All(slot => Math.Abs(slot.cooldown01) <= 0.0001f && Math.Abs(slot.charge01) <= 0.0001f);

            foreach (ItemSkillMonitorSlotSnapshot slot in monitor.Slots)
            {
                bool expectedMonitoringState = expectedMonitoring.Contains(slot.slotType);
                bool slotPassed = slot.isMonitoring == expectedMonitoringState
                    && slot.isUnlocked == expectedMonitoringState
                    && !slot.isTriggered
                    && Math.Abs(slot.cooldown01) <= 0.0001f
                    && Math.Abs(slot.charge01) <= 0.0001f
                    && errorsPassed
                    && countPassed
                    && stagePassed
                    && orderPassed
                    && uniquePassed
                    && triggerPassed
                    && progressPassed;
                if (!slotPassed)
                {
                    result.Errors.Add($"{caseId}/{slot.slotType} failed: expected monitoring={expectedMonitoringState}, count={expectedBuildCount}, stage={expectedStage}, errors={string.Join("|", expectedErrors)}; actual monitoring={slot.isMonitoring}, unlocked={slot.isUnlocked}, count={monitor.selectedMainBuildCount}, stage={monitor.selectedMainBuildStage}, errors={ItemSkillMonitorResolver.FormatValidationErrors(monitor.ValidationErrors)}.");
                }

                rows.Add(Row(caseId, selectedMainBuildId, monitor, slot, expectedMonitoringState ? "Monitoring" : "Locked", slotPassed ? "PASS" : "FAIL"));
            }
        }

        private static void CheckStableKeys(VerificationResult result, List<ItemSkillTriggerSpecRow> rows)
        {
            ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)),
                new ItemMainBuildSelectionInput("famen:zhenlei", "Verifier", 1));
            string[] expectedKeys =
            {
                "zhenlei_basic_attack",
                "zhenlei_build2",
                "zhenlei_build4",
                "zhenlei_build6"
            };
            bool passed = monitor.Slots.Select(slot => slot.iconKey).SequenceEqual(expectedKeys)
                && monitor.Slots.All(slot => slot.iconKey == slot.presentationCueId && slot.iconKey == slot.chibiActionKey);
            if (!passed)
            {
                result.Errors.Add("Stable presentation keys failed for zhenlei skill monitor preview.");
            }

            foreach (ItemSkillMonitorSlotSnapshot slot in monitor.Slots)
            {
                rows.Add(Row("stableCueKeysOnly", "famen:zhenlei", monitor, slot, "stable key only", passed ? "PASS" : "FAIL"));
            }
        }

        private static void CheckBuildResolverIsolation(VerificationResult result, List<ItemSkillTriggerSpecRow> rows)
        {
            ItemBuildSynergyResolutionResult build = ItemBuildSynergyResolver.Resolve(
                CreateBuildRegressionLighting(),
                ItemInnerDataCatalog.AllItems);
            bool selectedEmpty = string.IsNullOrWhiteSpace(build.selectedMainBuildId);
            bool legacyMemberAbsent = typeof(ItemBuildSynergyResolutionResult).GetMember("MonitorSlots").Length == 0;
            bool legacyTypeAbsent = typeof(ItemBuildSynergyResolutionResult).Assembly.GetType("TalismanBag.Items.Build.ItemSkillMonitorSlot") == null;
            bool passed = selectedEmpty && legacyMemberAbsent && legacyTypeAbsent;
            if (!passed)
            {
                result.Errors.Add("Build Resolver isolation failed: selectedMainBuildId must stay empty and legacy monitor members/types must remain absent.");
            }

            ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                build,
                new ItemMainBuildSelectionInput("famen:zhenlei", "Verifier", 1));
            foreach (ItemSkillMonitorSlotSnapshot slot in monitor.Slots)
            {
                rows.Add(Row("buildResolverIsolation", "famen:zhenlei", monitor, slot, "Build resolver remains pure statistics", passed ? "PASS" : "FAIL"));
            }
        }

        private static void CheckDetailProjection(VerificationResult result)
        {
            ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                BuildSnapshot(FaMen("famen:zhenlei", "zhenlei", "震雷法", 6)),
                new ItemMainBuildSelectionInput("famen:zhenlei", "Verifier", 1));
            ItemDetailViewModel model = new()
            {
                itemId = "I001",
                displayItemName = "Verifier Item"
            };
            ItemDetailViewModel projected = ItemSandboxSkillMonitorDetailProjection.Project(model, monitor);
            bool passed = projected.skillMonitorPreview != null
                && projected.skillMonitorPreview.slotPreviewLines.Count == 4
                && projected.skillMonitorPreview.selectedMainBuildId == "famen:zhenlei"
                && projected.displayPrimaryStats.Any(line => line.label == "skillMonitorSelectedMainBuild")
                && projected.skillMonitorPreview.slotPreviewLines.All(line => !ContainsOrdinal(line.body, "isTriggered=true"));
            if (!passed)
            {
                result.Errors.Add("ItemDetailViewModel SkillMonitor projection failed.");
            }
            else
            {
                result.Notes.Add("ItemDetailViewModel projection checked: independent skillMonitorPreview appears only when a SkillMonitor snapshot is supplied.");
            }
        }

        private static void CheckSandboxUiContract(VerificationResult result)
        {
            bool interfacesPassed = typeof(IItemMainBuildSelectionProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView))
                && typeof(IItemSkillMonitorSnapshotProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            bool optionsPassed = ItemSandboxGridPlacementPreviewView.MainBuildSelectionIds.Count == 6
                && string.IsNullOrEmpty(ItemSandboxGridPlacementPreviewView.MainBuildSelectionIds[0])
                && ItemSandboxGridPlacementPreviewView.MainBuildSelectionIds.Contains("famen:zhenlei")
                && ItemSandboxGridPlacementPreviewView.MainBuildSelectionIds.Contains("famen:taibai");
            bool scenePassed = false;
            if (File.Exists(ItemSandboxDetailUiSceneBuilder.ScenePath))
            {
                EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
                ItemSandboxGridPlacementPreviewView preview = Object.FindObjectOfType<ItemSandboxGridPlacementPreviewView>(true);
                scenePassed = preview != null
                    && preview.ConfiguredMainBuildSelectionButtonCount == 6
                    && preview.ConfiguredSkillMonitorIconCount == 4
                    && GameObject.Find("MainBuildSelectionPanel") != null
                    && GameObject.Find("SkillMonitorIconGrid") != null;
            }

            if (!interfacesPassed || !optionsPassed || !scenePassed)
            {
                result.Errors.Add($"Sandbox UI contract failed: interfaces={interfacesPassed}, options={optionsPassed}, scene={scenePassed}.");
            }
            else
            {
                result.Notes.Add("Sandbox greybox UI checked: explicit None + five faMen choices and four fixed icon slots exist in the Item Sandbox scene.");
            }
        }

        private static void CheckLightingArrayBuildAwakeningRegressions(VerificationResult result, List<ItemSkillTriggerSpecRow> rows)
        {
            ItemLightingBoardConfig board = ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig();
            ItemLightingResolutionResult lighting = ItemLightingResolver.Resolve(
                board,
                new[]
                {
                    new ItemLightingPlacedItem("I031", "JuNian", new[] { new Vector2Int(2, 1) }, new Vector2Int(2, 1), true, new Vector2Int(2, 1), "P_SOURCE"),
                    new ItemLightingPlacedItem("I001", "Zhenlei", new[] { new Vector2Int(1, 1) }, new Vector2Int(1, 1), false, Vector2Int.zero, "P_LIGHT_DIRECT")
                });
            bool lightingPassed = lighting.FindPlacementResult("P_LIGHT_DIRECT")?.isDirectLit == true;

            ItemArrayBonusResolutionResult array = ItemArrayBonusResolver.Resolve(
                board,
                new[]
                {
                    new ItemLightingItemResult("I001", "Array", new[] { new Vector2Int(2, 1) }, new Vector2Int(2, 1), false, true, true, "I031", 0, "P_AP_LIT", "P_SOURCE")
                });
            bool arrayPassed = array.FindPlacementResult("P_AP_LIT")?.isArrayBonusActive == true;

            ItemBuildSynergyResolutionResult build = ItemBuildSynergyResolver.Resolve(CreateBuildRegressionLighting(), ItemInnerDataCatalog.AllItems);
            bool buildPassed = build.FindFaMenBuild("famen:zhenlei")?.build6Active == true
                && string.IsNullOrWhiteSpace(build.selectedMainBuildId);

            ItemCoreAwakeningItemResult awakening = ItemCoreAwakeningResolver.ResolveItem(
                new ItemLightingItemResult("I001", "Awakening", new[] { Vector2Int.zero }, Vector2Int.zero, false, true, true, "I031", 0, "P_AWAKE", "P_SOURCE"),
                new ItemCoreAwakeningInput("I001", "P_AWAKE", 10, false, "Verifier"));
            bool awakeningPassed = awakening.coreEffectUnlocked && awakening.coreEffectActive;
            bool passed = lightingPassed && arrayPassed && buildPassed && awakeningPassed;
            if (!passed)
            {
                result.Errors.Add($"Regression failed: lighting={lightingPassed}, array={arrayPassed}, build={buildPassed}, awakening={awakeningPassed}.");
            }
            else
            {
                result.Notes.Add("Lighting / ArrayBonus / BuildSynergyCore / CoreAwakening regression checks passed inside ItemSkillTriggerContract01 verifier.");
            }

            ItemSkillMonitorResolutionResult monitor = ItemSkillMonitorResolver.Resolve(
                build,
                new ItemMainBuildSelectionInput("famen:zhenlei", "Verifier", 1));
            foreach (ItemSkillMonitorSlotSnapshot slot in monitor.Slots)
            {
                rows.Add(Row("lightingArrayBuildAwakeningRegression", "famen:zhenlei", monitor, slot, "Lighting/Array/Build/Awakening PASS", passed ? "PASS" : "FAIL"));
            }
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

                if (sourcePath.EndsWith("ItemSkillTriggerContractVerifier.cs", StringComparison.Ordinal)
                    || sourcePath.EndsWith("ItemSandboxDetailUiSceneBuilder.cs", StringComparison.Ordinal))
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

            result.Notes.Add("Source scope check completed: Item skill monitor contract stays inside Items/Skills and ItemSandbox preview paths, with no formal battle, bridge, run flow, save, reward, boss, or BuildSettings writes.");
        }

        private static ItemSkillTriggerSpecRow Row(
            string caseId,
            string selectedMainBuildId,
            ItemSkillMonitorResolutionResult monitor,
            ItemSkillMonitorSlotSnapshot slot,
            string expectedResult,
            string actualResult)
        {
            return new ItemSkillTriggerSpecRow(
                caseId,
                selectedMainBuildId,
                monitor.selectedMainBuildCount,
                monitor.selectedMainBuildStage,
                slot.slotType.ToString(),
                slot.isUnlocked,
                slot.isMonitoring,
                slot.isTriggered,
                slot.cooldown01,
                slot.charge01,
                slot.presentationCueId,
                slot.chibiActionKey,
                ItemSkillMonitorResolver.FormatValidationErrors(monitor.ValidationErrors),
                expectedResult,
                actualResult);
        }

        private static ItemBuildSynergyResolutionResult BuildSnapshot(params ItemBuildTrackResult[] tracks)
        {
            IReadOnlyList<ItemBuildTrackResult> source = tracks ?? Array.Empty<ItemBuildTrackResult>();
            return new ItemBuildSynergyResolutionResult(
                source.Where(track => track.trackKind == ItemBuildTrackKind.FaMen).ToArray(),
                source.Where(track => track.trackKind == ItemBuildTrackKind.QiLei).ToArray(),
                Array.Empty<ItemBuildSynergyItemResult>(),
                Array.Empty<string>());
        }

        private static ItemBuildTrackResult FaMen(string buildId, string stableTag, string displayName, int count)
        {
            return new ItemBuildTrackResult(
                buildId,
                ItemBuildTrackKind.FaMen,
                stableTag,
                displayName,
                count,
                6,
                SourceIds("P", stableTag, count),
                SourceIds("I", stableTag, count));
        }

        private static ItemBuildTrackResult QiLei(string buildId, string stableTag, string displayName, int count)
        {
            return new ItemBuildTrackResult(
                buildId,
                ItemBuildTrackKind.QiLei,
                stableTag,
                displayName,
                count,
                4,
                SourceIds("P", stableTag, count),
                SourceIds("I", stableTag, count));
        }

        private static IReadOnlyList<string> SourceIds(string prefix, string tag, int count)
        {
            return Enumerable.Range(1, Math.Max(0, count))
                .Select(index => $"{prefix}_{tag}_{index:00}")
                .ToArray();
        }

        private static ItemLightingResolutionResult CreateBuildRegressionLighting()
        {
            return new ItemLightingResolutionResult(
                Array.Empty<Vector2Int>(),
                new[]
                {
                    Lit("I001", "P_I001"),
                    Lit("I002", "P_I002"),
                    Lit("I003", "P_I003"),
                    Lit("I004", "P_I004"),
                    Lit("I005", "P_I005"),
                    Lit("I006", "P_I006")
                });
        }

        private static ItemLightingItemResult Lit(string itemId, string placementId)
        {
            return new ItemLightingItemResult(
                itemId,
                itemId,
                new[] { new Vector2Int(Mathf.Abs(placementId.GetHashCode()) % 5, Mathf.Abs(itemId.GetHashCode()) % 5) },
                Vector2Int.zero,
                false,
                true,
                true,
                "I031",
                0,
                placementId,
                "P_SOURCE");
        }

        private static ItemSkillMonitorSlotType[] AllSlotTypes()
        {
            return new[]
            {
                ItemSkillMonitorSlotType.BasicAttack,
                ItemSkillMonitorSlotType.Build2,
                ItemSkillMonitorSlotType.Build4,
                ItemSkillMonitorSlotType.Build6
            };
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<ItemSkillTriggerSpecRow> rows)
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
            builder.AppendLine("# ItemSkillTriggerContract01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `ItemSkillTriggerContract01`");
            builder.AppendLine("- Guard receipt target: `GUARD_PASS_ITEMSKILLTRIGGERCONTRACT01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox only.");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- Explicit Sandbox Preview main Build selection input; default is None.");
            builder.AppendLine("- Main Build selection accepts only faMen Build ids.");
            builder.AppendLine("- Four fixed monitor slots: BasicAttack / Build2 / Build4 / Build6.");
            builder.AppendLine("- Locked vs Monitoring greybox state only; no skill trigger, cooldown, charge, animation, damage, heal, shield, status, reward, save, boss, or battle wiring.");
            builder.AppendLine("- Read-only future presentation fields: iconKey, tooltipText, presentationCueId, chibiActionKey.");
            builder.AppendLine("- Independent ItemDetailViewModel `skillMonitorPreview` projection; BuildSynergyCore result remains pure Build statistics.");
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

        private static string BuildSpecCsv(IReadOnlyList<ItemSkillTriggerSpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,selectedMainBuildId,buildCount,activeStage,slotType,isUnlocked,isMonitoring,isTriggered,cooldown01,charge01,presentationCueId,chibiActionKey,validationErrors,expectedResult,actualResult");
            foreach (ItemSkillTriggerSpecRow row in rows)
            {
                string[] values =
                {
                    row.caseId,
                    row.selectedMainBuildId,
                    row.buildCount.ToString(),
                    row.activeStage.ToString(),
                    row.slotType,
                    row.isUnlocked ? "true" : "false",
                    row.isMonitoring ? "true" : "false",
                    row.isTriggered ? "true" : "false",
                    row.cooldown01.ToString("0"),
                    row.charge01.ToString("0"),
                    row.presentationCueId,
                    row.chibiActionKey,
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
            builder.AppendLine("# ItemSkillTriggerContract01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox explicit main Build selection, skill monitor contract snapshot, detail projection, and greybox preview UI.");
            builder.AppendLine("- BuildSettings: no writes; Item Sandbox remains manual-only.");
            builder.AppendLine("- Forbidden integrations: formal battle resolver, battle bridge, UnifiedBattlePage, V0.3 RunFlow, save data, rewards, drops, boss data, formal upgrade systems, and BuildSettings writers.");
            builder.AppendLine("- Not implemented: automatic main Build selection, Build Resolver writeback, skill trigger execution, damage/heal/shield/status settlement, cooldown or charge calculation, Q-character animation, formal skill icons, core effect execution, save serialization, rewards, drops, boss logic, or battle wiring.");
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

        private sealed class ItemSkillTriggerSpecRow
        {
            public ItemSkillTriggerSpecRow(
                string caseId,
                string selectedMainBuildId,
                int buildCount,
                int activeStage,
                string slotType,
                bool isUnlocked,
                bool isMonitoring,
                bool isTriggered,
                float cooldown01,
                float charge01,
                string presentationCueId,
                string chibiActionKey,
                string validationErrors,
                string expectedResult,
                string actualResult)
            {
                this.caseId = caseId;
                this.selectedMainBuildId = selectedMainBuildId;
                this.buildCount = buildCount;
                this.activeStage = activeStage;
                this.slotType = slotType;
                this.isUnlocked = isUnlocked;
                this.isMonitoring = isMonitoring;
                this.isTriggered = isTriggered;
                this.cooldown01 = cooldown01;
                this.charge01 = charge01;
                this.presentationCueId = presentationCueId;
                this.chibiActionKey = chibiActionKey;
                this.validationErrors = validationErrors;
                this.expectedResult = expectedResult;
                this.actualResult = actualResult;
            }

            public readonly string caseId;
            public readonly string selectedMainBuildId;
            public readonly int buildCount;
            public readonly int activeStage;
            public readonly string slotType;
            public readonly bool isUnlocked;
            public readonly bool isMonitoring;
            public readonly bool isTriggered;
            public readonly float cooldown01;
            public readonly float charge01;
            public readonly string presentationCueId;
            public readonly string chibiActionKey;
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
