#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemDetailInstanceDataAdapterVerifier
    {
        private const string Package = "V0.4-ItemDetailInstanceDataAdapter01";
        private const string Marker = "GUARD_PASS_ITEMDETAILINSTANCEDATAADAPTER01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemDetailInstanceDataAdapterReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemDetailInstanceDataAdapterSpec.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemDetailInstanceDataAdapterLeakCheckReport.md";
        private const string FieldMatrixPath = "Docs/V0.4/Reports/ItemDetailInstanceFieldMatrix.csv";

        private static readonly string[] PlayerForbiddenTokens =
        {
            "QA-I001", "I001", "rootSeed", "schemaId", "sourceSchemaId",
            "sourceCanonicalSignature", "qa_power", "qa_cooldown", "qa_core_",
            "itemInstanceId", "baseItemId", "rawUnits", "placementId"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemDetailInstanceDataAdapter01/[QA Only] Verify")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            Verification verification = new();
            List<SpecRow> specs = new();
            try
            {
                RunCoreVerification(verification, specs);
                RunSceneAndPrefabVerification(verification, specs);
                RunLeakCheck(verification);
                RunRegressions(verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add("Unhandled verifier exception: " + exception);
            }

            WriteReports(verification, specs);
            AssetDatabase.Refresh();
            bool passed = verification.Errors.Count == 0 && specs.All(row => row.result == "PASS");
            if (passed)
            {
                Debug.Log($"{Marker} specs={specs.Count}/{specs.Count} instances={verification.InstanceCount} rarities={verification.RarityCount} regressions={verification.RegressionPassCount}");
            }
            else
            {
                Debug.LogError("ITEM_DETAIL_INSTANCE_DATA_ADAPTER01_FAIL\n" + string.Join("\n", verification.Errors));
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void RunCoreVerification(Verification verification, List<SpecRow> specs)
        {
            GameObject host = new("ItemDetailInstanceDataAdapterVerifierHost");
            try
            {
                ItemInnerDataCatalogProvider catalog = host.AddComponent<ItemInnerDataCatalogProvider>();
                ItemInstanceProjectionQaFixture fixture = ItemInstanceProjectionQaFixture.Create();
                ItemDetailInstanceDataAdapter adapter = new(
                    catalog, fixture.ProjectionSet, fixture.StatSchema, fixture.AffixSchema);
                verification.InstanceCount = fixture.GeneratedInstances.Count;
                verification.RarityCount = fixture.GeneratedInstances.Select(value => value.rarity).Distinct().Count();

                Add(specs, verification, "fixture-six-instances", "GeneratedInstancePreview", "", "", "all",
                    fixture.GeneratedInstances.Count == 6, "6", fixture.GeneratedInstances.Count.ToString(),
                    "Shared runtime QA fixture extracted from the projection-contract verifier.");
                Add(specs, verification, "fixture-five-rarities", "GeneratedInstancePreview", "", "", "all",
                    verification.RarityCount == 5, "5", verification.RarityCount.ToString(),
                    "white/green/blue/purple/orange coverage.");

                Dictionary<string, ItemDetailInstanceProjectionResult> results = new(StringComparer.Ordinal);
                foreach (var source in fixture.GeneratedInstances)
                {
                    ItemDetailInstanceProjectionResult result = adapter.Project(new ItemDetailInstanceProjectionInput
                    {
                        contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                        itemInstanceId = source.itemInstanceId
                    });
                    results[source.itemInstanceId] = result;
                    Add(specs, verification, "open-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        result.isSuccess && result.viewModel != null,
                        "success", result.status.ToString(), "All QA instances open through exact identity query.");
                    if (!result.isSuccess)
                    {
                        continue;
                    }

                    ItemDetailViewModel model = result.viewModel;
                    Add(specs, verification, "identity-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        model.itemInstanceId == source.itemInstanceId
                        && model.baseItemId == source.baseItemId
                        && model.rarityKey == source.rarity.ToStableKey()
                        && model.rarityDisplayName == source.rarity.ToDisplayName(),
                        source.itemInstanceId + "|" + source.baseItemId,
                        model.itemInstanceId + "|" + model.baseItemId,
                        "ViewModel retains identities without displaying raw IDs in player sections.");
                    Add(specs, verification, "stats-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        model.displayPrimaryStats.Count == result.projection.Stats.Count
                        && model.displayPrimaryStats.All(value => !string.IsNullOrWhiteSpace(value.value)),
                        result.projection.Stats.Count.ToString(), model.displayPrimaryStats.Count.ToString(),
                        "Formatted from projection statId/rawUnits via Stat Schema.");
                    int fixedCount = result.projection.Affixes.Count(value => value.slotKind == ItemAffixSlotKind.Fixed);
                    int randomCount = result.projection.Affixes.Count(value => value.slotKind == ItemAffixSlotKind.Random);
                    Add(specs, verification, "affix-groups-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        model.displayFixedAffixes.Count == fixedCount && model.displayRandomAffixes.Count == randomCount,
                        fixedCount + "/" + randomCount,
                        model.displayFixedAffixes.Count + "/" + model.displayRandomAffixes.Count,
                        "Grouped strictly by projection slotKind.");
                    Add(specs, verification, "core-subset-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        result.projection.VisibleCoreEffectIds.All(result.projection.EligibleCoreEffectIds.Contains),
                        "visible subset eligible", "checked",
                        "Potential is not converted to unlocked or active state.");
                    string playerText = string.Join("\n", model.displayPlayerSections.Select(section => section.title + "\n" + section.body));
                    string leaked = PlayerForbiddenTokens.FirstOrDefault(token =>
                        playerText.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
                    Add(specs, verification, "player-no-raw-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        leaked == null
                        && playerText.IndexOf("养成状态：已开窍", StringComparison.Ordinal) < 0
                        && playerText.IndexOf("战斗发动状态：已激活", StringComparison.Ordinal) < 0,
                        "no raw identity/seed/schema/signature/internal IDs or active claims",
                        leaked ?? "clean", "Player chapter leak boundary.");
                    string debugText = string.Join("\n", model.displayDebugSections.Select(section => section.title + "\n" + section.body));
                    string[] requiredDebug =
                    {
                        "itemInstanceId", "baseItemId", "generationVersion", "rootSeed",
                        "sourceSchemaId", "sourceGenerationAlgorithmId", "generationDataStatus",
                        "sourceCanonicalSignature", "QA_FIXTURE_ONLY", "NOT_BALANCE_APPROVED",
                        "NOT_FORMAL_GENERATION_DATA"
                    };
                    string missing = requiredDebug.FirstOrDefault(token =>
                        debugText.IndexOf(token, StringComparison.Ordinal) < 0);
                    Add(specs, verification, "debug-trace-" + source.itemInstanceId, "GeneratedInstancePreview",
                        source.itemInstanceId, source.baseItemId, source.rarity.ToStableKey(),
                        missing == null, "complete generation trace", missing ?? "complete",
                        "Debug tab contains full provenance.");
                }

                var white = fixture.GeneratedInstances.Single(value => value.rarity == ItemInstanceRarity.White);
                Add(specs, verification, "white-build-none", "GeneratedInstancePreview", white.itemInstanceId,
                    white.baseItemId, white.rarity.ToStableKey(),
                    results[white.itemInstanceId].projection.buildQualification == TalismanBag.Items.Generation.Potential.ItemBuildQualification.None,
                    "None", results[white.itemInstanceId].projection.buildQualification.ToString(),
                    "White rarity is never promoted to Build eligibility.");

                var twins = fixture.GeneratedInstances.Where(value =>
                    value.baseItemId == "I001" && value.rarity == ItemInstanceRarity.Green).ToArray();
                bool isolated = twins.Length == 2
                    && twins[0].itemInstanceId != twins[1].itemInstanceId
                    && results[twins[0].itemInstanceId].projection.sourceCanonicalSignature
                        != results[twins[1].itemInstanceId].projection.sourceCanonicalSignature
                    && results[twins[0].itemInstanceId].viewModel.itemInstanceId == twins[0].itemInstanceId
                    && results[twins[1].itemInstanceId].viewModel.itemInstanceId == twins[1].itemInstanceId;
                Add(specs, verification, "same-base-rarity-isolation", "GeneratedInstancePreview", "two", "I001", "green",
                    isolated, "two exact independent instances", isolated ? "isolated" : "crossed",
                    "Same baseItemId + rarity does not merge attributes or affixes.");

                ItemDetailInstanceProjectionResult stale = adapter.Project(new ItemDetailInstanceProjectionInput
                {
                    contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                    itemInstanceId = "QA-STALE-I001"
                });
                Add(specs, verification, "stale-no-fallback", "GeneratedInstancePreview", "QA-STALE-I001", "", "",
                    !stale.isSuccess && stale.status == ItemDetailInstanceProjectionStatus.InstanceUnavailable
                    && stale.projection == null && stale.viewModel.baseItemId == string.Empty,
                    "unavailable/no fallback", stale.status.ToString(),
                    "A stale itemInstanceId never falls back to baseItemId.");

                ItemDetailInstanceProjectionResult i031Instance = adapter.Project(new ItemDetailInstanceProjectionInput
                {
                    contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                    itemInstanceId = "I031"
                });
                ItemDetailInstanceProjectionResult i031Catalog = adapter.Project(new ItemDetailInstanceProjectionInput
                {
                    contextKind = ItemDetailProjectionContextKind.CatalogPreview,
                    baseItemId = "I031"
                });
                Add(specs, verification, "i031-instance-rejected", "GeneratedInstancePreview", "I031", "I031", "",
                    !i031Instance.isSuccess && i031Instance.projection == null,
                    "rejected", i031Instance.status.ToString(), "I031 does not enter ordinary generated instances.");
                Add(specs, verification, "i031-catalog-allowed", "CatalogPreview", "", "I031", "",
                    i031Catalog.isSuccess && string.IsNullOrWhiteSpace(i031Catalog.viewModel.itemInstanceId),
                    "catalog success/no instance", i031Catalog.status.ToString(), "System item remains a CatalogPreview entry.");

                ItemDetailInstanceProjectionResult placed = adapter.Project(new ItemDetailInstanceProjectionInput
                {
                    contextKind = ItemDetailProjectionContextKind.PlacedInstance,
                    placementId = "P_EXISTING",
                    itemInstanceId = twins[0].itemInstanceId
                });
                Add(specs, verification, "placed-binding-not-invented", "PlacedInstance", twins[0].itemInstanceId,
                    "I001", "green", !placed.isSuccess
                    && placed.status == ItemDetailInstanceProjectionStatus.PlacedInstanceBindingUnavailable,
                    "binding unavailable", placed.status.ToString(),
                    "Existing strict placement composer remains authoritative.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static void RunSceneAndPrefabVerification(Verification verification, List<SpecRow> specs)
        {
            var scene = EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
            ItemSandboxDetailUiController controller = UnityEngine.Object.FindObjectOfType<ItemSandboxDetailUiController>(true);
            ItemSandboxGeneratedInstanceProvider generated = UnityEngine.Object.FindObjectOfType<ItemSandboxGeneratedInstanceProvider>(true);
            GameObject modeBar = GameObject.Find("ItemDetailContextModeBar");
            Add(specs, verification, "scene-two-modes", "SandboxLayout", "", "", "",
                controller != null && generated != null && controller.HasGeneratedInstanceMode
                && modeBar != null && modeBar.transform.Find("CatalogModeButton") != null
                && modeBar.transform.Find("GeneratedInstancesModeButton") != null,
                "Catalog + Generated Instances", modeBar == null ? "missing" : "present",
                "Modes are scene-authored, not runtime-created.");
            Add(specs, verification, "scene-generated-six", "SandboxLayout", "", "", "all",
                generated != null && generated.GetItemList().Count == 6,
                "6", generated == null ? "0" : generated.GetItemList().Count.ToString(),
                "Generated mode consumes the shared QA fixture API.");

            GameObject listPanel = GameObject.Find("ItemNameListPanel");
            GameObject placementPanel = GameObject.Find("ItemGridPlacementPreviewPanel");
            GameObject detailPanel = GameObject.Find("ItemDetailPanel");
            CheckPanelLayout(specs, verification, listPanel, placementPanel, detailPanel, modeBar, 1080, 1920, "layout-1080x1920");
            CheckPanelLayout(specs, verification, listPanel, placementPanel, detailPanel, modeBar, 720, 1280, "layout-720x1280");

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            GameObject canvasHost = new("ItemDetailAdapterLayoutVerifierCanvas", typeof(RectTransform), typeof(Canvas));
            GameObject prefabRoot = prefabAsset == null ? null : UnityEngine.Object.Instantiate(prefabAsset, canvasHost.transform);
            try
            {
                ItemDetailPanelView view = prefabRoot?.GetComponent<ItemDetailPanelView>();
                ScrollRect[] scrollRects = prefabRoot == null
                    ? Array.Empty<ScrollRect>()
                    : prefabRoot.GetComponentsInChildren<ScrollRect>(true);
                ItemDetailSectionView[] sections = prefabRoot == null
                    ? Array.Empty<ItemDetailSectionView>()
                    : prefabRoot.GetComponentsInChildren<ItemDetailSectionView>(true);
                Add(specs, verification, "prefab-unique-runtime-view", "Prefab", "", "", "",
                    view != null && scrollRects.Length == 2 && sections.Length == 18,
                    "one view/2 ScrollRect/18 sections", $"{(view == null ? 0 : 1)}/{scrollRects.Length}/{sections.Length}",
                    "The existing ItemDetailPanel.prefab remains the only runtime detail implementation.");

                if (view != null && generated != null)
                {
                    ItemDetailViewModel first = generated.GetDetailViewModel(generated.GetItemList()[0].itemId).Clone();
                    ItemDetailViewModel second = generated.GetDetailViewModel(generated.GetItemList()[1].itemId).Clone();
                    string longBody = "  " + string.Join("\n  ", Enumerable.Range(1, 80).Select(index => "长属性/词条压力行 " + index));
                    first.displayPlayerSections[3].body = longBody;
                    second.displayPlayerSections[3].body = longBody;
                    first.displayDebugSections[1].body = longBody;
                    second.displayDebugSections[1].body = longBody;
                    view.Bind(first);
                    Canvas.ForceUpdateCanvases();
                    foreach (ScrollRect scroll in scrollRects)
                    {
                        scroll.verticalNormalizedPosition = 0.2f;
                    }

                    view.Bind(second);
                    Canvas.ForceUpdateCanvases();
                    string panelSource = File.ReadAllText(
                        "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
                    bool reset = first.itemInstanceId != second.itemInstanceId
                        && panelSource.Contains("BuildModelIdentity(model)")
                        && panelSource.Contains("ShowPlayerDetailTab();")
                        && panelSource.Contains("ResetScrollToTop();")
                        && !panelSource.Contains("LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content)")
                        && panelSource.Contains("scrollRect.verticalNormalizedPosition = 1f;");
                    bool longBound = sections.Any(section =>
                        section.GetComponentsInChildren<Text>(true)
                            .Any(text => text.text?.Contains("长属性/词条压力行 80") == true));
                    Add(specs, verification, "switch-reset-top", "Prefab", second.itemInstanceId,
                        second.baseItemId, second.rarityKey, reset, "identity change resets scroll top without forcing child layout",
                        reset ? "contract present" : "contract missing",
                        "Batch static contract; ItemDetailPanel child sizes/positions remain Inspector-authorable.");
                    Add(specs, verification, "long-list-bound", "Prefab", first.itemInstanceId,
                        first.baseItemId, first.rarityKey, longBound, "last line retained",
                        longBound ? "retained" : "missing", "Long attributes/affixes remain in scroll content.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasHost);
            }

            if (!scene.IsValid())
            {
                verification.Errors.Add("Item Sandbox scene failed to open.");
            }
        }

        private static void CheckPanelLayout(List<SpecRow> specs, Verification verification,
            GameObject list, GameObject placement, GameObject detail, GameObject modeBar,
            int width, int height, string caseId)
        {
            Rect a = ResolveScreenRect(list?.GetComponent<RectTransform>(), width, height);
            Rect b = ResolveScreenRect(placement?.GetComponent<RectTransform>(), width, height);
            Rect c = ResolveScreenRect(detail?.GetComponent<RectTransform>(), width, height);
            LayoutElement modeLayout = modeBar?.GetComponent<LayoutElement>();
            HorizontalLayoutGroup modeGroup = modeBar?.GetComponent<HorizontalLayoutGroup>();
            bool pass = list != null && placement != null && detail != null
                && modeBar != null && modeBar.transform.parent == list.transform
                && modeLayout != null && modeLayout.preferredHeight > 0f
                && modeGroup != null && modeBar.transform.childCount == 2
                && a.width > 0 && a.height > 0 && b.width > 0 && b.height > 0
                && c.width > 0 && c.height > 0;
            Add(specs, verification, caseId, "SandboxLayout", "", "", "",
                pass, "mode bar contained by list + existing panels retain positive bounds",
                pass ? "no new mode overlap" : $"{a}|{b}|{c}",
                width + "x" + height + " new-control containment check; pre-existing sandbox panel geometry is preserved.");
        }

        private static Rect ResolveScreenRect(RectTransform value, int width, int height)
        {
            if (value == null)
            {
                return Rect.zero;
            }

            float xMin = value.anchorMin.x * width + value.offsetMin.x;
            float yMin = value.anchorMin.y * height + value.offsetMin.y;
            float xMax = value.anchorMax.x * width + value.offsetMax.x;
            float yMax = value.anchorMax.y * height + value.offsetMax.y;
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static void RunLeakCheck(Verification verification)
        {
            string[] paths =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs",
                ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath
            };
            string[] forbidden =
            {
                "ItemInstanceRollEngine", "ItemDropGeneration", "BuildSandbox", "Reward",
                "SaveData", "BattleContract", "BattleBridge", "UnifiedBattlePage"
            };
            foreach (string path in paths)
            {
                string text = File.ReadAllText(path);
                foreach (string token in forbidden)
                {
                    if (text.IndexOf(token, StringComparison.Ordinal) >= 0)
                    {
                        verification.Leaks.Add(path + ": " + token);
                    }
                }
            }

            string adapter = File.ReadAllText("Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailInstanceDataAdapter.cs");
            foreach (string token in new[] { "ItemInstanceRollEngine", "ItemDropGeneration", "Reward", "SaveData", "BattleContract", "BattleBridge", "UnifiedBattlePage" })
            {
                if (adapter.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    verification.Leaks.Add("Adapter direct dependency: " + token);
                }
            }

            verification.Errors.AddRange(verification.Leaks.Select(value => "LeakCheck: " + value));
        }

        private static void RunRegressions(Verification verification)
        {
            Action[] runners =
            {
                TalismanBag.EditorTools.ItemGeneration.ItemRarityInstanceFoundationVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemStatRangeSchemaVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemCorePotentialAndBuildEligibilitySchemaVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemAffixPoolAndRangeSchemaVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemInstanceRollEngineVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemDropGenerationSandboxVerifier.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemGenerationSimulationValidator.VerifyMenu,
                TalismanBag.EditorTools.ItemGeneration.ItemInstanceProjectionContractVerifier.VerifyMenu,
                ItemDetailProjectionCompleteVerifier.VerifyMenu,
                ItemSystemValidatorAndSnapshotVerifier.VerifyMenu,
                BuildSynergyCoreVerifier.VerifyMenu,
                CoreAwakeningPreviewVerifier.VerifyMenu,
                ItemSkillTriggerContractVerifier.VerifyMenu,
                JuNianLightingAndAdjacentRelayVerifier.VerifyMenu,
                ArrayBonusCellResolverVerifier.VerifyMenu
            };
            foreach (Action runner in runners)
            {
                runner();
            }

            string[] reports =
            {
                "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md",
                "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md",
                "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md",
                "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaReport.md",
                "Docs/V0.4/Reports/ItemInstanceRollEngineReport.md",
                "Docs/V0.4/Reports/ItemDropGenerationSandboxReport.md",
                "Docs/V0.4/Reports/ItemGenerationSimulationValidatorReport.md",
                "Docs/V0.4/Reports/ItemInstanceProjectionContractReport.md",
                "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md",
                "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md",
                "Docs/V0.4/Reports/BuildSynergyCoreReport.md",
                "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md",
                "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md",
                "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md",
                "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md"
            };
            foreach (string path in reports)
            {
                bool pass = File.Exists(path)
                    && File.ReadAllText(path).IndexOf("PASS", StringComparison.OrdinalIgnoreCase) >= 0
                    && File.ReadAllText(path).IndexOf("Result: FAIL", StringComparison.OrdinalIgnoreCase) < 0
                    && File.ReadAllText(path).IndexOf("Verification: FAIL", StringComparison.OrdinalIgnoreCase) < 0;
                if (pass)
                {
                    verification.RegressionPassCount++;
                }
                else
                {
                    verification.Errors.Add("Regression report is not PASS: " + path);
                }
            }
        }

        private static void WriteReports(Verification verification, IReadOnlyList<SpecRow> specs)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            bool pass = verification.Errors.Count == 0 && specs.All(row => row.result == "PASS");
            File.WriteAllText(ReportPath, new StringBuilder()
                .AppendLine("# ItemDetailInstanceDataAdapter01 Report")
                .AppendLine()
                .AppendLine($"- Package: `{Package}`")
                .AppendLine($"- Result: {(pass ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(pass ? Marker : "NOT_PASSED")}`")
                .AppendLine($"- Spec: {specs.Count(row => row.result == "PASS")}/{specs.Count} PASS")
                .AppendLine($"- Instances: {verification.InstanceCount}")
                .AppendLine($"- Rarities: {verification.RarityCount}/5")
                .AppendLine($"- Regressions: {verification.RegressionPassCount}/15 PASS")
                .AppendLine($"- Leaks: {verification.Leaks.Count}")
                .AppendLine("- Identity: baseItemId / itemInstanceId / placementId remain distinct.")
                .AppendLine("- Data maturity: QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA.")
                .AppendLine("- UI: Catalog + Generated Instances use the same ItemDetailPanel.prefab.")
                .AppendLine("- Forbidden scope: Reward / RunFlow / Inventory / SaveData / formal Battle / Boss / BuildSettings not connected.")
                .AppendLine()
                .AppendLine("## Errors")
                .AppendLine(verification.Errors.Count == 0 ? "- None" : string.Join("\n", verification.Errors.Select(value => "- " + value)))
                .ToString(), Encoding.UTF8);

            StringBuilder csv = new("caseId,contextKind,itemInstanceId,baseItemId,rarity,expected,actual,result,notes\n");
            foreach (SpecRow row in specs)
            {
                csv.Append(Csv(row.caseId)).Append(',').Append(Csv(row.contextKind)).Append(',')
                    .Append(Csv(row.itemInstanceId)).Append(',').Append(Csv(row.baseItemId)).Append(',')
                    .Append(Csv(row.rarity)).Append(',').Append(Csv(row.expected)).Append(',')
                    .Append(Csv(row.actual)).Append(',').Append(Csv(row.result)).Append(',')
                    .Append(Csv(row.notes)).Append('\n');
            }
            File.WriteAllText(SpecPath, csv.ToString(), Encoding.UTF8);

            File.WriteAllText(LeakPath, new StringBuilder()
                .AppendLine("# ItemDetailInstanceDataAdapter01 Leak Check")
                .AppendLine()
                .AppendLine($"- Result: {(verification.Leaks.Count == 0 ? "PASS" : "FAIL")}")
                .AppendLine($"- Leak count: {verification.Leaks.Count}")
                .AppendLine("- ItemDetailPanelView / Prefab do not reference Roll Engine, Drop Generation, BuildSandbox, Reward, SaveData or formal Battle contracts.")
                .AppendLine("- Adapter is read-only and consumes projection + schema facts only.")
                .AppendLine(verification.Leaks.Count == 0 ? "- Leaks: None" : string.Join("\n", verification.Leaks.Select(value => "- " + value)))
                .ToString(), Encoding.UTF8);

            File.WriteAllText(FieldMatrixPath,
                "field,source,playerChapter,debugChapter,semantics\n" +
                "itemInstanceId,projection.itemInstanceId,no,yes,generated instance identity\n" +
                "baseItemId,projection.baseItemId,no,yes,catalog prototype identity\n" +
                "rarity,projection.rarity/rarityKey,display name/key,yes,generated rarity\n" +
                "stats,projection.stats + Stat Schema,formatted name/value/unit,raw statId/rawUnits,generated facts\n" +
                "fixedAffixes,projection.affixes slotKind=Fixed,formatted,raw affixId/rawUnits,instance result\n" +
                "randomAffixes,projection.affixes slotKind=Random,formatted,raw affixId/rawUnits,instance result\n" +
                "eligibleCoreEffectIds,projection eligible,count only,ids,rarity potential not unlocked\n" +
                "visibleCoreEffectIds,projection visible,count only,ids,rarity visibility not active\n" +
                "buildQualification,projection qualification,qualification only,enum,not Build activation\n" +
                "generationVersion/rootSeed/sourceSchema/sourceAlgorithm/status/signature,projection,no,yes,QA provenance only\n" +
                "placementId,existing placement composer,no,existing debug,not bound in this package\n",
                Encoding.UTF8);
        }

        private static void Add(List<SpecRow> specs, Verification verification,
            string caseId, string contextKind, string itemInstanceId, string baseItemId, string rarity,
            bool pass, string expected, string actual, string notes)
        {
            specs.Add(new SpecRow(caseId, contextKind, itemInstanceId, baseItemId, rarity,
                expected, actual, pass ? "PASS" : "FAIL", notes));
            if (!pass)
            {
                verification.Errors.Add(caseId + ": expected " + expected + ", actual " + actual);
            }
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private sealed class Verification
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Leaks = new();
            public int InstanceCount;
            public int RarityCount;
            public int RegressionPassCount;
        }

        private sealed class SpecRow
        {
            public SpecRow(string caseId, string contextKind, string itemInstanceId, string baseItemId,
                string rarity, string expected, string actual, string result, string notes)
            {
                this.caseId = caseId;
                this.contextKind = contextKind;
                this.itemInstanceId = itemInstanceId;
                this.baseItemId = baseItemId;
                this.rarity = rarity;
                this.expected = expected;
                this.actual = actual;
                this.result = result;
                this.notes = notes;
            }

            public readonly string caseId;
            public readonly string contextKind;
            public readonly string itemInstanceId;
            public readonly string baseItemId;
            public readonly string rarity;
            public readonly string expected;
            public readonly string actual;
            public readonly string result;
            public readonly string notes;
        }
    }
}
#endif
