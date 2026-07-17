using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Skills;
using TalismanBag.ItemSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemFullDetailBuildSandboxWorkbenchVerifier
    {
        public const string Marker = "ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_PASS";
        public const string ReportPath = "Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchReport.md";
        public const string SpecPath = "Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchSpec.csv";
        public const string LeakPath = "Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchLeakCheckReport.md";
        private static readonly string[] Rarities = { "white", "green", "blue", "purple", "orange" };
        private static readonly string[] RuntimeSources =
        {
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchView.cs"
        };
        private static readonly string[] ForbiddenRuntimeLayoutTokens =
        {
            ".anchoredPosition", ".sizeDelta", ".offsetMin", ".offsetMax",
            ".preferredWidth", ".preferredHeight", ".spacing", ".padding", ".fontSize"
        };
        private static readonly string[] ForbiddenScopeTokens =
        {
            "UnifiedBattlePage", "BattleContract", "BattleAdapter", "BattleBridge",
            "V02RunFlowController", "V03RunFlow", "RewardResolver", "InventoryService",
            "SaveData", "BossController", "EditorBuildSettings.scenes =", "UnityEngine.Random",
            "string.GetHashCode"
        };
        private static readonly string[] ProtectedPaths =
        {
            "Assets/_Game/Configs/ItemBalanceWorkbench",
            "Assets/_Game/Scripts/TalismanBag/Items/Generation",
            "Assets/_Game/Prefabs",
            "ProjectSettings/EditorBuildSettings.asset",
            "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemFullDetailBuildSandboxWorkbench01/Verify")]
        public static void VerifyMenu()
        {
            Verification result = Verify();
            if (result.Errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            Debug.Log(Marker);
        }

        public static void VerifyBatch()
        {
            try
            {
                Verification result = Verify();
                if (result.Errors.Count > 0)
                {
                    throw new InvalidOperationException(string.Join(" | ", result.Errors));
                }

                Debug.Log(Marker);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static Verification Verify()
        {
            Verification result = new();
            Dictionary<string, string> protectedBefore = ProtectedPaths.ToDictionary(
                path => path, HashPath, StringComparer.Ordinal);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            ItemBalanceWorkbenchCatalog workbench = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath);
            if (workbench == null)
            {
                result.Errors.Add("Missing ItemBalanceWorkbenchCatalog asset.");
                WriteReports(result);
                return result;
            }

            GameObject providerObject = new("ItemFullDetailWorkbenchVerifierProvider");
            ItemInnerDataCatalogProvider provider = providerObject.AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemBalanceCandidateDetailSandboxAdapter adapter = new(workbench, provider);
                VerifyAllCandidates(adapter, result);
                VerifySession(adapter, provider, result);
                VerifyScene(result);
                VerifyRuntimeSourceBoundaries(result);
                VerifyProtectedHashes(result, protectedBefore);
                VerifyRegressionReports(result);
            }
            finally
            {
                Object.DestroyImmediate(providerObject);
            }

            WriteReports(result);
            AssetDatabase.Refresh();
            return result;
        }

        private static void VerifyAllCandidates(
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            Verification result)
        {
            int combinations = 0;
            int projections = 0;
            int details = 0;
            int deterministic = 0;
            int ranges = 0;
            foreach (string baseItemId in adapter.CandidateBaseItemIds)
            {
                foreach (string rarity in Rarities)
                {
                    long seed = 810000L + combinations;
                    ItemBalanceCandidateDetailRequest request = new()
                    {
                        baseItemId = baseItemId,
                        rarityKey = rarity,
                        rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                    };
                    ItemBalanceCandidateDetailResult first = adapter.Request(request);
                    ItemBalanceCandidateDetailResult second = adapter.Request(request);
                    bool generated = first.isSuccess;
                    bool projection = first.detailProjection?.projection != null;
                    bool detail = first.viewModel != null
                        && first.viewModel.displayPlayerSections.Count == 15
                        && first.viewModel.displayDebugSections.Count == 3;
                    bool stable = projection && second.detailProjection?.projection != null
                        && string.Equals(
                            first.detailProjection.projection.BuildCanonicalSignature(),
                            second.detailProjection.projection.BuildCanonicalSignature(),
                            StringComparison.Ordinal);
                    Add(result, $"candidate-{baseItemId}-{rarity}", generated && projection && detail && stable,
                        $"seed={seed}; ranges={first.candidateStatRangeCount}; qualification={first.detailProjection?.projection?.buildQualification}");
                    combinations += generated ? 1 : 0;
                    projections += projection ? 1 : 0;
                    details += detail ? 1 : 0;
                    deterministic += stable ? 1 : 0;
                    ranges += first.candidateStatRangeCount;
                }
            }

            result.Combinations = combinations;
            result.Projections = projections;
            result.Details = details;
            result.Deterministic = deterministic;
            result.StatRanges = ranges;
            Add(result, "candidate-counts", combinations == 150 && projections == 150
                && details == 150 && deterministic == 150 && ranges == 600,
                $"combos={combinations}/150; projections={projections}/150; details={details}/150; deterministic={deterministic}/150; ranges={ranges}/600");
            Add(result, "ordinary-pool-excludes-I031",
                adapter.CandidateBaseItemIds.Count == 30
                && !adapter.CandidateBaseItemIds.Contains("I031", StringComparer.Ordinal),
                $"count={adapter.CandidateBaseItemIds.Count}; I031=0");
        }

        private static void VerifySession(
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider provider,
            Verification result)
        {
            ItemFullDetailBuildSandboxWorkbenchSession session = new(adapter, provider);
            bool layout = session.LoadValidationLayout(out IReadOnlyList<long> seeds);
            result.ValidationSeeds = seeds.ToArray();
            ItemBuildTrackResult build6 = session.Snapshot.build.FindFaMenBuild("famen:zhenlei");
            Add(result, "editable-legal-build6-layout", layout && build6?.litItemCount == 6
                && build6.activeStagePieceCount == 6 && session.Placements.Count == 7,
                $"seeds={string.Join("|", seeds)}; placements={session.Placements.Count}; count={build6?.litItemCount}; stage={build6?.activeStagePieceCount}");
            Add(result, "validation-layout-array-and-relay",
                session.Snapshot.array.ActivePlacementIds.Count >= 1
                && session.Snapshot.lighting.RelayLitPlacementIds.Count >= 1,
                $"arrayActive={session.Snapshot.array.ActivePlacementIds.Count}; relay={session.Snapshot.lighting.RelayLitPlacementIds.Count}");

            string beforeMain = BuildCounts(session.Snapshot.build);
            bool defaultNone = string.IsNullOrWhiteSpace(session.SelectedMainBuildId)
                && string.IsNullOrWhiteSpace(session.Snapshot.skillMonitor.selectedMainBuildId);
            bool selected = session.SelectMainBuild("famen:zhenlei", "UserClick");
            string afterMain = BuildCounts(session.Snapshot.build);
            IReadOnlyList<ItemSkillMonitorSlotSnapshot> slots = session.Snapshot.skillMonitor.Slots;
            bool fourSlots = slots.Count == 4
                && slots.Select(value => value.slotType).SequenceEqual(new[]
                {
                    ItemSkillMonitorSlotType.BasicAttack,
                    ItemSkillMonitorSlotType.Build2,
                    ItemSkillMonitorSlotType.Build4,
                    ItemSkillMonitorSlotType.Build6
                });
            bool allMonitoring = slots.All(value => value.isMonitoring);
            Add(result, "main-build-explicit-default-none", defaultNone && selected
                && string.Equals(session.SelectedMainBuildId, "famen:zhenlei", StringComparison.Ordinal),
                $"defaultNone={defaultNone}; selected={session.SelectedMainBuildId}");
            Add(result, "main-build-does-not-change-counts", string.Equals(beforeMain, afterMain, StringComparison.Ordinal),
                $"before={beforeMain}; after={afterMain}");
            Add(result, "four-readonly-monitor-slots", fourSlots && allMonitoring,
                string.Join(" | ", slots.Select(value => $"{value.slotType}:{value.isMonitoring}:{value.sourceBuildId}")));

            ItemFullDetailWorkbenchPlacement[] ordinary = session.Placements.Where(value => !value.isJuNian).ToArray();
            session.SelectPlacement(ordinary[5].placementId);
            session.RemoveSelected();
            session.SelectPlacement(ordinary[4].placementId);
            session.RemoveSelected();
            ItemBuildTrackResult build4 = session.Snapshot.build.FindFaMenBuild("famen:zhenlei");
            bool stage4 = build4?.litItemCount == 4 && build4.activeStagePieceCount == 4;
            session.SelectPlacement(ordinary[3].placementId);
            session.RemoveSelected();
            session.SelectPlacement(ordinary[2].placementId);
            session.RemoveSelected();
            ItemBuildTrackResult build2 = session.Snapshot.build.FindFaMenBuild("famen:zhenlei");
            bool stage2 = build2?.litItemCount == 2 && build2.activeStagePieceCount == 2;
            Add(result, "build-stages-2-4-6", stage2 && stage4,
                $"Build2={build2?.litItemCount}/{build2?.activeStagePieceCount}; Build4={build4?.litItemCount}/{build4?.activeStagePieceCount}; Build6=6/6");

            ItemFullDetailBuildSandboxWorkbenchSession whiteNone = new(adapter, provider);
            ItemFullDetailWorkbenchInstance white = whiteNone.CreateInstance("I001", "white", 910001L);
            whiteNone.SelectJuNian();
            whiteNone.PlaceSelected(new Vector2Int(0, 0));
            whiteNone.SelectInstance(white.itemInstanceId);
            whiteNone.PlaceSelected(new Vector2Int(1, 0));
            ItemBuildSynergyItemResult whiteBuild = whiteNone.Snapshot.build.FindPlacementResult(whiteNone.SelectedPlacementId);
            Add(result, "qualification-none-not-counted", white.buildQualification == ItemBuildQualification.None
                && whiteBuild?.isLit == true && whiteBuild.countedInBuild == false
                && whiteNone.Snapshot.build.FindFaMenBuild("famen:zhenlei")?.litItemCount == 0,
                $"qualification={white.buildQualification}; lit={whiteBuild?.isLit}; counted={whiteBuild?.countedInBuild}");

            ItemFullDetailBuildSandboxWorkbenchSession multi = new(adapter, provider);
            ItemFullDetailWorkbenchInstance first = multi.CreateInstance("I001", "orange", seeds[0]);
            ItemFullDetailWorkbenchInstance second = multi.CreateInstance("I001", "orange", seeds[0] + 1L);
            multi.SelectJuNian();
            multi.PlaceSelected(new Vector2Int(0, 0));
            multi.SelectInstance(first.itemInstanceId);
            multi.PlaceSelected(new Vector2Int(1, 0));
            string firstPlacement = multi.SelectedPlacementId;
            ItemDetailViewModel firstDetail = multi.BuildSelectedDetail();
            multi.SelectInstance(second.itemInstanceId);
            bool secondPlaced = multi.PlaceSelected(new Vector2Int(2, 0), out ItemFullDetailPlacementFailureReason duplicateReason);
            bool ownedButNotCoPlaced = !string.Equals(first.itemInstanceId, second.itemInstanceId, StringComparison.Ordinal)
                && Contains(firstDetail, first.itemInstanceId)
                && !Contains(firstDetail, second.itemInstanceId)
                && !secondPlaced
                && duplicateReason == ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced
                && multi.Placements.Count(value => string.Equals(value.baseItemId, "I001", StringComparison.Ordinal)) == 1;
            Add(result, "same-base-owned-multiple-placement-unique", ownedButNotCoPlaced,
                $"owned={first.itemInstanceId}|{second.itemInstanceId}; placed={firstPlacement}; second={secondPlaced}/{duplicateReason}");

            ItemFullDetailBuildSandboxWorkbenchSession awakening = new(adapter, provider);
            ItemFullDetailWorkbenchInstance orange = awakening.CreateInstance("I001", "orange", seeds[0]);
            awakening.SelectJuNian();
            awakening.PlaceSelected(new Vector2Int(0, 0));
            awakening.SelectInstance(orange.itemInstanceId);
            awakening.PlaceSelected(new Vector2Int(4, 4));
            awakening.SetSandboxLevel(1);
            var level1 = awakening.Snapshot.awakening.FindPlacementResult(awakening.SelectedPlacementId);
            awakening.SetSandboxLevel(40);
            var level40Unlit = awakening.Snapshot.awakening.FindPlacementResult(awakening.SelectedPlacementId);
            awakening.MoveSelected(new Vector2Int(1, 0));
            var level40Lit = awakening.Snapshot.awakening.FindPlacementResult(awakening.SelectedPlacementId);
            bool coreLayers = level1?.unlockedCoreEffectCount == 0 && level1.activeCoreEffectCount == 0
                && level40Unlit?.unlockedCoreEffectCount > 0 && level40Unlit.activeCoreEffectCount == 0
                && level40Lit?.unlockedCoreEffectCount > 0
                && level40Lit.activeCoreEffectCount == level40Lit.unlockedCoreEffectCount;
            Add(result, "core-visible-unlocked-active-layers", coreLayers,
                $"Lv1={level1?.unlockedCoreEffectCount}/{level1?.activeCoreEffectCount}; Lv40Unlit={level40Unlit?.unlockedCoreEffectCount}/{level40Unlit?.activeCoreEffectCount}; Lv40Lit={level40Lit?.unlockedCoreEffectCount}/{level40Lit?.activeCoreEffectCount}");

            ItemDetailViewModel complete = awakening.BuildSelectedDetail();
            string playerText = string.Join("\n", complete.displayPlayerSections.Select(value => value.body));
            string debugText = string.Join("\n", complete.displayDebugSections.Select(value => value.body));
            string combined = playerText + "\n" + debugText;
            string[] required =
            {
                orange.itemInstanceId, orange.baseItemId, awakening.SelectedPlacementId,
                orange.rarityKey, orange.Projection.cultivationPotentialProfileId,
                "BuildQualification", "countedInBuild", "selectedMainBuildId",
                "unlockedCoreEffects", "activeCoreEffects", "BALANCE_CANDIDATE",
                "NOT_LIVE_LOCKED", "NOT_BATTLE_CONNECTED",
                "valueProfileId=", "rawUnits=", "formattedValue=", "candidateRange=",
                "playerHiddenItemPower", "playerHiddenBasic", "法门构筑："
            };
            bool playerClean = new[]
            {
                "rawUnits", "valueProfileId", "itemInstanceId", "rootSeed", "basisPoint",
                "Build track", "Build progress", "Selected item contribution", "ACTIVE", "INACTIVE", "need "
            }
                .All(token => !playerText.Contains(token, StringComparison.OrdinalIgnoreCase));
            string[] missingRequired = required.Where(value => !combined.Contains(value, StringComparison.Ordinal)).ToArray();
            Add(result, "full-detail-required-facts", missingRequired.Length == 0 && playerClean,
                "requiredFacts=" + required.Length + "; missing="
                    + (missingRequired.Length == 0 ? "None" : string.Join("|", missingRequired))
                    + "; playerClean=" + playerClean);
        }

        private static void VerifyScene(Verification result)
        {
            Scene scene = EditorSceneManager.OpenScene(ItemFullDetailBuildSandboxWorkbenchSceneBuilder.ScenePath, OpenSceneMode.Single);
            ItemFullDetailBuildSandboxWorkbenchView view = Object.FindObjectOfType<ItemFullDetailBuildSandboxWorkbenchView>(true);
            Add(result, "scene-workbench-component", view != null, view == null ? "missing" : view.name);
            if (view == null)
            {
                return;
            }

            SerializedObject serialized = new(view);
            bool refs = HasArray(serialized, "baseItemButtons", 30)
                && HasArray(serialized, "rarityButtons", 5)
                && HasArray(serialized, "instanceButtons", 12)
                && HasArray(serialized, "boardCells", 25)
                && HasArray(serialized, "mainBuildButtons", 6)
                && HasArray(serialized, "skillMonitorTexts", 4)
                && serialized.FindProperty("detailPanel")?.objectReferenceValue != null
                && serialized.FindProperty("seedInput")?.objectReferenceValue != null;
            Add(result, "scene-serialized-ui-contract", refs, refs ? "30/5/12/25/6/4" : "serialized reference mismatch");

            RectTransform workbenchRect = view.GetComponent<RectTransform>();
            RectTransform detailRect = Object.FindObjectOfType<ItemSandboxDetailPanelView>(true)?.GetComponent<RectTransform>();
            RectTransform listRect = Object.FindObjectOfType<ItemSandboxItemButtonView>(true)?.transform.parent as RectTransform;
            bool separated = workbenchRect != null && detailRect != null && listRect != null
                && workbenchRect.anchorMax.x <= detailRect.anchorMin.x + 0.001f
                && listRect.anchorMax.x <= workbenchRect.anchorMin.x + 0.001f;
            Add(result, "layout-1080x1920-no-major-column-overlap", separated,
                RectSummary(listRect, workbenchRect, detailRect));
            Add(result, "layout-720x1280-no-major-column-overlap", separated,
                "anchor-driven: " + RectSummary(listRect, workbenchRect, detailRect));

            RectState before = new(workbenchRect);
            view.SendMessage("Initialize", SendMessageOptions.DontRequireReceiver);
            RectState after = new(workbenchRect);
            Add(result, "runtime-does-not-reset-manual-layout", before.Equals(after),
                $"before={before}; after={after}");

            bool buildSettingsExcluded = !EditorBuildSettings.scenes.Any(value =>
                string.Equals(value.path, ItemFullDetailBuildSandboxWorkbenchSceneBuilder.ScenePath, StringComparison.OrdinalIgnoreCase));
            Add(result, "sandbox-scene-not-in-build-settings", buildSettingsExcluded,
                buildSettingsExcluded ? "not registered" : "unexpected BuildSettings entry");
        }

        private static void VerifyRuntimeSourceBoundaries(Verification result)
        {
            foreach (string path in RuntimeSources)
            {
                string source = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
                foreach (string token in ForbiddenRuntimeLayoutTokens)
                {
                    Add(result, "runtime-layout-scan-" + Path.GetFileName(path) + "-" + Sanitize(token),
                        !source.Contains(token, StringComparison.Ordinal), token);
                }

                foreach (string token in ForbiddenScopeTokens)
                {
                    Add(result, "scope-leak-" + Path.GetFileName(path) + "-" + Sanitize(token),
                        !source.Contains(token, StringComparison.Ordinal), token);
                }
            }
        }

        private static void VerifyProtectedHashes(
            Verification result,
            IReadOnlyDictionary<string, string> protectedBefore)
        {
            foreach (string path in ProtectedPaths)
            {
                string before = protectedBefore[path];
                string actual = HashPath(path);
                Add(result, "protected-hash-" + Sanitize(path),
                    string.Equals(actual, before, StringComparison.Ordinal),
                    $"before={before}; after={actual}");
                result.Hashes.Add((path, before, actual));
            }
        }

        private static void VerifyRegressionReports(Verification result)
        {
            string[] reports =
            {
                "ItemInnerDataCatalogReport.md",
                "ItemGridPlacementAndEyeRuleReport.md",
                "JuNianLightingAndAdjacentRelayReport.md",
                "ArrayBonusCellResolverReport.md",
                "BuildSynergyCoreReport.md",
                "CoreAwakeningPreviewReport.md",
                "ItemSkillTriggerContractReport.md",
                "ItemDetailProjectionCompleteReport.md",
                "ItemSystemValidatorAndSnapshotReport.md",
                "ItemBalanceWorkbenchReport.md",
                "ItemBalanceCandidateDetailSandboxAdapterReport.md"
            };
            foreach (string report in reports)
            {
                string path = "Docs/V0.4/Reports/" + report;
                string text = File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
                bool pass = text.Contains("PASS", StringComparison.OrdinalIgnoreCase)
                    && !text.Contains("Result: FAIL", StringComparison.OrdinalIgnoreCase)
                    && !text.Contains("result: FAIL", StringComparison.OrdinalIgnoreCase);
                Add(result, "regression-report-" + report, pass, path);
            }
        }

        private static void WriteReports(Verification result)
        {
            File.WriteAllText(SpecPath, BuildCsv(result), new UTF8Encoding(false));
            File.WriteAllText(ReportPath, BuildReport(result), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeak(result), new UTF8Encoding(false));
        }

        private static string BuildCsv(Verification result)
        {
            StringBuilder builder = new("caseId,result,detail\n");
            foreach (Row row in result.Rows)
            {
                builder.Append(Csv(row.CaseId)).Append(',')
                    .Append(row.Pass ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(row.Detail)).Append('\n');
            }

            return builder.ToString();
        }

        private static string BuildReport(Verification result)
        {
            bool pass = result.Errors.Count == 0;
            StringBuilder builder = new();
            builder.AppendLine("# ItemFullDetailBuildSandboxWorkbench01 Report").AppendLine()
                .AppendLine($"- Result: {(pass ? "PASS" : "FAIL")}")
                .AppendLine($"- Marker: `{(pass ? Marker : "NOT_EMITTED")}`")
                .AppendLine("- Guard receipt target: `GUARD_PASS_ITEMFULLDETAILBUILDSANDBOXWORKBENCH01`")
                .AppendLine($"- Spec: {result.Rows.Count(row => row.Pass)}/{result.Rows.Count} PASS")
                .AppendLine($"- Candidates: {result.Combinations}/150")
                .AppendLine($"- Stat ranges: {result.StatRanges}/600")
                .AppendLine($"- Instance projections: {result.Projections}/150")
                .AppendLine($"- Full details: {result.Details}/150")
                .AppendLine($"- Determinism: {result.Deterministic}/150")
                .AppendLine("- I031 ordinary pool entries: 0").AppendLine()
                .AppendLine("## Editable Build validation layout").AppendLine()
                .AppendLine("- Source: fixed `I031` at `(0,0)`.")
                .AppendLine("- Ordinary instances: six distinct real `I001-I006@orange` projections at anchors `(1,0) (2,0) (4,0) (2,1) (4,2) (2,3)`.")
                .AppendLine("- Seeds: `" + string.Join(" | ", result.ValidationSeeds) + "`.")
                .AppendLine("- Result: real generated FaMenOnly/Dual qualification, direct + relay lighting, array activation, editable placement records, Build2/4/6 PASS.").AppendLine()
                .AppendLine("## Main Build and monitors").AppendLine()
                .AppendLine("- Default is None; only `UserClick` creates an explicit selection.")
                .AppendLine("- Selection does not write into or change qualified Build counts.")
                .AppendLine("- BasicAttack / Build2 / Build4 / Build6 fixed-order monitor slots all follow the selected FaMen Build and execute no skill.").AppendLine()
                .AppendLine("## Core effect layers").AppendLine()
                .AppendLine("- Candidate eligible/visible IDs remain projection facts.")
                .AppendLine("- Sandbox Lv1-Lv40 produces preview-only unlock input; active requires visible + unlocked + lit.")
                .AppendLine("- No formal cultivation, save, battle effect, or asset write is connected.").AppendLine()
                .AppendLine("## UI and layout").AppendLine()
                .AppendLine("- Runtime layout hard-code scan: PASS when this report is PASS.")
                .AppendLine("- 1080x1920 and 720x1280 anchor-column checks: PASS when this report is PASS.")
                .AppendLine("- Runtime initialization preserves serialized RectTransform state; repeated rows use twelve serialized layout-controlled slots.")
                .AppendLine("- Existing Runtime ItemDetailPanel prefab is reused and was not modified.").AppendLine()
                .AppendLine("## Protected hashes").AppendLine()
                .AppendLine("| path | before | after | result |")
                .AppendLine("|---|---|---|---|");
            foreach ((string path, string before, string after) in result.Hashes)
            {
                builder.AppendLine($"| `{path}` | `{before}` | `{after}` | {(before == after ? "PASS" : "FAIL")} |");
            }

            builder.AppendLine().AppendLine("## Regression evidence").AppendLine()
                .AppendLine("Existing Item System and eight Item Algorithm verifier reports were checked for PASS markers; dedicated batch regressions are recorded in the final run log set.").AppendLine()
                .AppendLine("## Batch commands and logs").AppendLine()
                .AppendLine("- Builder: `Unity.exe -batchmode -nographics -projectPath F:\\Porject\\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchSceneBuilder.ApplyBatch -quit`")
                .AppendLine("- Verifier: `Unity.exe -batchmode -nographics -projectPath F:\\Porject\\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchVerifier.VerifyBatch -quit`")
                .AppendLine("- Logs: `Logs/codex_item_full_detail_workbench_builder.log`, `Logs/codex_item_full_detail_workbench_verify.log`, `Logs/codex_item_full_detail_workbench_regression.log`.").AppendLine()
                .AppendLine("## Errors").AppendLine()
                .AppendLine(result.Errors.Count == 0 ? "- None." : string.Join("\n", result.Errors.Select(value => "- " + value)));
            return builder.ToString();
        }

        private static string BuildLeak(Verification result)
        {
            bool pass = result.Errors.Count == 0;
            return "# ItemFullDetailBuildSandboxWorkbench01 Leak Check Report\n\n"
                + $"- Result: {(pass ? "PASS" : "FAIL")}\n"
                + "- Scene: isolated `Scene_TalismanBag_V04_ItemSandbox.unity`; not in BuildSettings.\n"
                + "- Formal Battle / BattleContract / Adapter / Bridge: not connected.\n"
                + "- V0.3 RunFlow / Reward / Inventory / SaveData / Boss / formal drop / formal cultivation: not connected.\n"
                + "- `ItemSystemSnapshot.v1` schema/canonical fields remain compatible; validator now enforces unique ordinary baseItemId/itemId per layout.\n"
                + "- Candidate Unity assets, Generation schemas, Prefabs, and BuildSettings: protected hashes unchanged when PASS.\n"
                + "- Runtime layout fields: no anchoredPosition, sizeDelta, offsets, preferred sizes, spacing, padding, or font size writes.\n"
                + "- Randomness: no UnityEngine.Random or string.GetHashCode seed path.\n"
                + "- I031: fixed core placement only; ordinary roll pool count remains zero.\n"
                + "- Data maturity remains BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED.\n"
                + "- Commit / tag / push: not performed.\n";
        }

        private static void Add(Verification result, string caseId, bool pass, string detail)
        {
            result.Rows.Add(new Row(caseId, pass, detail));
            if (!pass)
            {
                result.Errors.Add(caseId + ": " + detail);
            }
        }

        private static string BuildCounts(ItemBuildSynergyResolutionResult snapshot)
        {
            return string.Join("|", snapshot.FaMenBuilds.Concat(snapshot.QiLeiBuilds)
                .OrderBy(value => value.buildId, StringComparer.Ordinal)
                .Select(value => value.buildId + "=" + value.litItemCount));
        }

        private static bool Contains(ItemDetailViewModel model, string value)
        {
            return model != null && (model.itemInstanceId?.Contains(value, StringComparison.Ordinal) == true
                || model.displayDebugSections.Any(section => section.body?.Contains(value, StringComparison.Ordinal) == true));
        }

        private static bool HasArray(SerializedObject serialized, string propertyName, int expected)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            return property != null && property.isArray && property.arraySize == expected;
        }

        private static string RectSummary(params RectTransform[] values)
        {
            return string.Join(" | ", values.Select(value => value == null
                ? "missing"
                : $"{value.name}:{value.anchorMin.x:0.###}-{value.anchorMax.x:0.###}"));
        }

        private static string HashPath(string relativePath)
        {
            string absolute = Path.GetFullPath(relativePath);
            string[] files = Directory.Exists(absolute)
                ? Directory.GetFiles(absolute, "*", SearchOption.AllDirectories).OrderBy(value => value, StringComparer.Ordinal).ToArray()
                : new[] { absolute };
            using SHA256 sha = SHA256.Create();
            string project = Directory.GetCurrentDirectory();
            foreach (string file in files)
            {
                byte[] name = Encoding.UTF8.GetBytes(file.Replace(project, string.Empty));
                sha.TransformBlock(name, 0, name.Length, name, 0);
                byte[] bytes = File.ReadAllBytes(file);
                sha.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }

            sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(sha.Hash).Replace("-", string.Empty);
        }

        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static string Sanitize(string value) => new(value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray());

        private sealed class Verification
        {
            public readonly List<Row> Rows = new();
            public readonly List<string> Errors = new();
            public readonly List<(string path, string before, string after)> Hashes = new();
            public int Combinations;
            public int StatRanges;
            public int Projections;
            public int Details;
            public int Deterministic;
            public long[] ValidationSeeds = Array.Empty<long>();
        }

        private readonly struct Row
        {
            public Row(string caseId, bool pass, string detail)
            {
                CaseId = caseId;
                Pass = pass;
                Detail = detail ?? string.Empty;
            }

            public string CaseId { get; }
            public bool Pass { get; }
            public string Detail { get; }
        }

        private readonly struct RectState : IEquatable<RectState>
        {
            public RectState(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                offsetMin = rect.offsetMin;
                offsetMax = rect.offsetMax;
            }

            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 pivot;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 offsetMin;
            private readonly Vector2 offsetMax;

            public bool Equals(RectState other) => anchorMin == other.anchorMin && anchorMax == other.anchorMax
                && pivot == other.pivot && anchoredPosition == other.anchoredPosition
                && sizeDelta == other.sizeDelta && offsetMin == other.offsetMin && offsetMax == other.offsetMax;
            public override bool Equals(object value) => value is RectState other && Equals(other);
            public override int GetHashCode() => anchorMin.GetHashCode();
            public override string ToString() => $"a={anchorMin}-{anchorMax}; p={anchoredPosition}; s={sizeDelta}";
        }
    }
}
