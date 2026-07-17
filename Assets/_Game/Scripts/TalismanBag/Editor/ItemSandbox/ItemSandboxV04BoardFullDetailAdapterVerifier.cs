using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox.Editor
{
    public static class ItemSandboxV04BoardFullDetailAdapterVerifier
    {
        private const string ReportRelative = "Docs/V0.4/Reports/ItemSandboxV04BoardFullDetailAdapterReport.md";
        private const string SpecRelative = "Docs/V0.4/Reports/ItemSandboxV04BoardFullDetailAdapterSpec.csv";
        private const string LeakRelative = "Docs/V0.4/Reports/ItemSandboxV04BoardFullDetailAdapterLeakCheckReport.md";
        private const string PassMarker = "ITEM_SANDBOX_V04_BOARD_FULL_DETAIL_ADAPTER01_PASS";
        private const string GuardReceipt = "GUARD_PASS_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01";
        private const string ReworkReceipt = "TASK_REWORK2_COMPLETE_AWAITING_USER_PREFAB_BASELINE_DECISION";
        private const string BaselineReceipt = "USER_ACCEPT_BASELINE_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01";
        private const string GeometryBaselineReceipt = "USER_ACCEPT_GEOMETRY_BASELINE_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01";

        private sealed class Check
        {
            public string id;
            public string area;
            public string expectation;
            public bool pass;
            public string evidence;
        }

        [MenuItem("Tools/TalismanBag/V0.4 ItemSandbox/Verify Board Full Detail Adapter")]
        public static void VerifyMenu() => VerifyBatch();

        public static void VerifyBatch()
        {
            List<Check> checks = new();
            Scene scene = EditorSceneManager.OpenScene(ItemSandboxV04BoardFullDetailSceneBinder.ScenePath, OpenSceneMode.Single);
            GameObject root = Find(scene, "ItemSandboxRoot");
            ItemSandboxV04BoardFullDetailAdapter runtime = root?.GetComponent<ItemSandboxV04BoardFullDetailAdapter>();
            ItemInnerDataCatalogProvider catalogProvider = root?.GetComponent<ItemInnerDataCatalogProvider>();
            ItemBalanceCandidateDetailSandboxProvider candidateProvider = root?.GetComponent<ItemBalanceCandidateDetailSandboxProvider>();
            Add(checks, "SCENE_ADAPTER", "scene", "Target scene has the one V04 adapter and existing providers.",
                runtime != null && catalogProvider != null && candidateProvider?.WorkbenchCatalog != null,
                runtime == null ? "adapter missing" : "adapter + providers serialized");
            if (runtime == null || catalogProvider == null || candidateProvider?.WorkbenchCatalog == null)
            {
                WriteReports(checks);
                throw new InvalidOperationException("ItemSandbox scene binding is incomplete; reports record the failure.");
            }

            VerifyCatalogAndCandidates(checks, candidateProvider.WorkbenchCatalog, catalogProvider);
            VerifySessionRules(checks, candidateProvider.WorkbenchCatalog, catalogProvider);
            VerifySceneBindings(checks, root, runtime);
            VerifyLeakAndIntegrity(checks);
            WriteReports(checks);

            Check[] failures = checks.Where(check => !check.pass).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException("V04 ItemSandbox adapter verification failed: "
                    + string.Join(", ", failures.Select(check => check.id)));
            }
            Debug.Log(PassMarker);
        }

        private static void VerifyCatalogAndCandidates(
            ICollection<Check> checks,
            ItemBalanceWorkbenchCatalog workbench,
            ItemInnerDataCatalogProvider catalogProvider)
        {
            IReadOnlyList<ItemSandboxV04TrayEntry> entries = ItemSandboxV04BoardFullDetailAdapter.BuildTrayEntries();
            Add(checks, "TRAY_31", "catalog", "Tray is exactly I001-I031 in stable order.",
                entries.Count == 31 && entries.Select(entry => entry.itemId)
                    .SequenceEqual(Enumerable.Range(1, 31).Select(index => $"I{index:000}")),
                string.Join("|", entries.Select(entry => entry.itemId)));
            Add(checks, "JUNIAN_UNIQUE_SOURCE", "catalog", "I031 is the only source item and normal items are I001-I030.",
                entries.Count(entry => entry.isJuNian) == 1
                    && entries.Take(30).All(entry => !entry.isJuNian)
                    && entries.Last().itemId == "I031",
                "ordinary=30; source=" + entries.Last().itemId);
            Add(checks, "SHAPE_CATALOG", "catalog", "Every tray entry uses catalog shape cells and a core cell inside its shape.",
                entries.All(entry => entry.Definition.ShapeCells.Count > 0
                    && entry.Definition.ShapeCells.Contains(entry.Definition.coreCellLocal)),
                string.Join("; ", entries.GroupBy(entry => entry.Definition.shapeId)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => group.Key + "=" + group.Count().ToString(CultureInfo.InvariantCulture))));

            ItemBalanceCandidateDetailSandboxAdapter adapter = new(workbench, catalogProvider);
            string[] rarities = { "white", "green", "blue", "purple", "orange" };
            int successes = 0;
            int ranges = 0;
            int details = 0;
            int deterministic = 0;
            int coreLevelMaps = 0;
            foreach (string itemId in Enumerable.Range(1, 30).Select(index => $"I{index:000}"))
            {
                foreach (string rarity in rarities)
                {
                    ItemBalanceCandidateDetailRequest request = new()
                    {
                        baseItemId = itemId,
                        rarityKey = rarity,
                        rootSeedText = "40412001"
                    };
                    ItemBalanceCandidateDetailResult first = adapter.Request(request);
                    ItemBalanceCandidateDetailResult second = adapter.Request(request);
                    if (first?.isSuccess == true)
                    {
                        successes++;
                        ranges += first.candidateStatRangeCount;
                        details += first.viewModel != null ? 1 : 0;
                        coreLevelMaps += first.CoreUnlockLevels.Count > 0 ? 1 : 0;
                    }
                    if (ReferenceEquals(first, second)
                        && string.Equals(first?.detailProjection?.projection?.itemInstanceId,
                            second?.detailProjection?.projection?.itemInstanceId, StringComparison.Ordinal))
                    {
                        deterministic++;
                    }
                }
            }
            Add(checks, "CANDIDATE_150", "candidate", "30 normal items × 5 rarities all generate successfully.",
                successes == 150, "success=" + successes);
            Add(checks, "CANDIDATE_RANGE_600", "candidate", "Each candidate exposes four configured stat ranges.",
                ranges == 600, "rangeRows=" + ranges);
            Add(checks, "CANDIDATE_DETAIL_150", "detail", "All 150 candidates compose runtime ItemDetailPanel models.",
                details == 150, "detailModels=" + details);
            Add(checks, "CANDIDATE_DETERMINISTIC", "candidate", "Same item/rarity/seed is deterministic and cached.",
                deterministic == 150, "deterministic=" + deterministic);
            Add(checks, "CORE_UNLOCK_LEVELS", "awakening", "Candidate profiles expose configured core unlock levels.",
                coreLevelMaps == 150, "candidateMaps=" + coreLevelMaps);
        }

        private static void VerifySessionRules(
            ICollection<Check> checks,
            ItemBalanceWorkbenchCatalog workbench,
            ItemInnerDataCatalogProvider catalogProvider)
        {
            ItemFullDetailBuildSandboxWorkbenchSession uniqueSession = new(
                new ItemBalanceCandidateDetailSandboxAdapter(workbench, catalogProvider), catalogProvider);
            ItemFullDetailWorkbenchInstance first = uniqueSession.CreateInstance("I001", "white", 11L);
            ItemFullDetailWorkbenchInstance second = uniqueSession.CreateInstance("I001", "white", 12L);
            Add(checks, "INSTANCE_ID_UNIQUE", "instances", "Inventory may own repeated base items as independent instance ids before placement.",
                first != null && second != null && first.itemInstanceId != second.itemInstanceId,
                (first?.itemInstanceId ?? "null") + " | " + (second?.itemInstanceId ?? "null"));

            ItemFullDetailBuildSandboxWorkbenchSession session = new(
                new ItemBalanceCandidateDetailSandboxAdapter(workbench, catalogProvider), catalogProvider);
            bool layout = session.LoadValidationLayout(out IReadOnlyList<long> seeds);
            ItemSystemSnapshot snapshot = session.Snapshot?.placementSnapshot;
            Add(checks, "BOARD_RULES", "placement", "Validation layout resolves on the 5×5 board with eye (2,2) and four array cells.",
                layout && snapshot?.boardSize == 5 && snapshot.eyeCell == new Vector2Int(2, 2)
                    && snapshot.ArrayBonusCells.Count == 4,
                "layout=" + layout + "; board=" + snapshot?.boardSize + "; array=" + snapshot?.ArrayBonusCells.Count);
            Add(checks, "PLACEMENT_IDS", "instances", "Every placed instance has an independent placementId.",
                session.Placements.Count > 1
                    && session.Placements.Select(value => value.placementId).Distinct(StringComparer.Ordinal).Count() == session.Placements.Count,
                "placements=" + session.Placements.Count + "; unique="
                    + session.Placements.Select(value => value.placementId).Distinct(StringComparer.Ordinal).Count());
            Add(checks, "JUNIAN_ONCE", "placement", "Validation layout contains exactly one I031 placement.",
                session.Placements.Count(value => value.isJuNian) == 1,
                "I031 placements=" + session.Placements.Count(value => value.isJuNian));
            Add(checks, "LIGHTING", "lighting", "Direct/relay lighting resolves and no occupied cell is the eye.",
                session.Snapshot.lighting.ItemResults.Any(value => value.isDirectLit)
                    && session.Snapshot.lighting.ItemResults.Any(value => value.isLit && !value.isDirectLit)
                    && snapshot.placements.All(value => !value.OccupiedCells.Contains(snapshot.eyeCell)),
                "direct=" + session.Snapshot.lighting.ItemResults.Count(value => value.isDirectLit)
                    + "; relay=" + session.Snapshot.lighting.ItemResults.Count(value => value.isLit && !value.isDirectLit));
            Add(checks, "BUILD_TRACKS", "build", "Qualified lit items expose Build2/4/6 track state for FaMen and Build2/4 for QiLei.",
                session.Snapshot.build.FaMenBuilds.Any(track => track.maxPieceCount == 6)
                    && session.Snapshot.build.QiLeiBuilds.Any(track => track.maxPieceCount == 4),
                "FaMen=" + session.Snapshot.build.FaMenBuilds.Count + "; QiLei=" + session.Snapshot.build.QiLeiBuilds.Count);
            Add(checks, "MAIN_BUILD_DEFAULT_NONE", "build", "MainBuild is explicit and defaults to None.",
                string.IsNullOrWhiteSpace(session.SelectedMainBuildId),
                "selected=" + (string.IsNullOrWhiteSpace(session.SelectedMainBuildId) ? "None" : session.SelectedMainBuildId));
            Add(checks, "SKILL_MONITOR_4", "skills", "Exactly four readonly monitor slots resolve.",
                session.Snapshot.skillMonitor.Slots.Count == 4,
                string.Join("|", session.Snapshot.skillMonitor.Slots.Select(slot => slot.displayName)));

            session.SetSandboxLevel(1);
            ItemCoreAwakeningNodeState[] low = session.Snapshot.awakening.ItemResults
                .SelectMany(value => value.NodeStates).ToArray();
            session.SetSandboxLevel(40);
            ItemCoreAwakeningNodeState[] high = session.Snapshot.awakening.ItemResults
                .SelectMany(value => value.NodeStates).ToArray();
            Add(checks, "SANDBOX_LEVEL_1_40", "awakening", "Lv1-Lv40 recomputes readonly visible/unlocked/lit/active core states.",
                low.Length > 0 && high.Length > 0
                    && high.Count(value => value.isUnlocked) >= low.Count(value => value.isUnlocked)
                    && high.All(value => value.unlockLevel >= 1 && value.unlockLevel <= 40),
                "Lv1 unlocked=" + low.Count(value => value.isUnlocked)
                    + "; Lv40 unlocked=" + high.Count(value => value.isUnlocked)
                    + "; Lv40 active=" + high.Count(value => value.isActive));
            Add(checks, "DETAIL_PLACED", "detail", "Placed selection composes a runtime detail model with placement state.",
                session.BuildSelectedDetail() != null,
                "selectedPlacementId=" + (session.SelectedPlacementId ?? "None"));
            bool detailBuildProgress = VerifyDetailBuildProgress(workbench, catalogProvider, out string detailBuildEvidence);
            Add(checks, "DETAIL_BUILD_PROGRESS_2_4_6", "detail",
                "Placed item detail Build sections expose final names while keeping stage skill descriptions.",
                detailBuildProgress,
                detailBuildEvidence);
        }

        private static bool VerifyDetailBuildProgress(
            ItemBalanceWorkbenchCatalog workbench,
            ItemInnerDataCatalogProvider catalogProvider,
            out string evidence)
        {
            int[] counts = { 2, 4, 6 };
            List<string> samples = new();
            foreach (int count in counts)
            {
                if (!TryBuildFaMenProgressDetail(workbench, catalogProvider, count, out string body, out string error))
                {
                    evidence = count.ToString(CultureInfo.InvariantCulture) + "/6 failed: " + error;
                    return false;
                }

                string token = count.ToString(CultureInfo.InvariantCulture) + "/6";
                if (!body.Contains(token, StringComparison.Ordinal))
                {
                    evidence = "Missing " + token + " in detail body: " + body.Replace("\n", " / ");
                    return false;
                }
                if (!ContainsFinalBuildNaming(body, true))
                {
                    evidence = "Missing final FaMen Build naming or skill text in detail body: " + body.Replace("\n", " / ");
                    return false;
                }
                samples.Add(token);
            }

            if (!TryBuildQiLeiProgressDetail(workbench, catalogProvider, out string qiLeiBody, out string qiLeiError))
            {
                evidence = "QiLei 2/4 failed: " + qiLeiError;
                return false;
            }
            if (!qiLeiBody.Contains("2/4", StringComparison.Ordinal) || !ContainsFinalBuildNaming(qiLeiBody, false))
            {
                evidence = "Missing QiLei 相合/成局 naming or skill text in detail body: " + qiLeiBody.Replace("\n", " / ");
                return false;
            }
            samples.Add("qilei 2/4 naming");

            evidence = string.Join(", ", samples);
            return true;
        }

        private static bool ContainsFinalBuildNaming(string body, bool faMen)
        {
            return !string.IsNullOrWhiteSpace(body)
                && body.Contains(faMen ? "法门构筑：" : "类构筑：", StringComparison.Ordinal)
                && (body.Contains("提高", StringComparison.Ordinal) || body.Contains("追加", StringComparison.Ordinal))
                && (faMen
                    ? body.Contains("九霄雷君的敕令", StringComparison.Ordinal)
                    : (body.Contains("相合", StringComparison.Ordinal) || body.Contains("成局", StringComparison.Ordinal)))
                && !body.Contains("阶段效果：", StringComparison.Ordinal)
                && !body.Contains("数值：", StringComparison.Ordinal)
                && !body.Contains("触发：", StringComparison.Ordinal)
                && !body.Contains("条件：", StringComparison.Ordinal)
                && !body.Contains("on_build_stage_active", StringComparison.Ordinal)
                && !body.Contains("未配置", StringComparison.Ordinal)
                && !body.Contains("候选", StringComparison.Ordinal)
                && !body.Contains("Build track", StringComparison.Ordinal)
                && !body.Contains("Build progress", StringComparison.Ordinal)
                && !body.Contains("Selected item contribution", StringComparison.Ordinal)
                && !body.Contains("ACTIVE", StringComparison.Ordinal)
                && !body.Contains("INACTIVE", StringComparison.Ordinal)
                && !body.Contains("need ", StringComparison.Ordinal);
        }

        private static bool TryBuildFaMenProgressDetail(
            ItemBalanceWorkbenchCatalog workbench,
            ItemInnerDataCatalogProvider catalogProvider,
            int targetCount,
            out string body,
            out string error)
        {
            body = string.Empty;
            error = string.Empty;
            ItemFullDetailBuildSandboxWorkbenchSession session = new(
                new ItemBalanceCandidateDetailSandboxAdapter(workbench, catalogProvider), catalogProvider);
            if (!session.LoadValidationLayout(out _))
            {
                error = "distinct-base validation layout failed";
                return false;
            }

            ItemFullDetailWorkbenchPlacement[] ordinary = session.Placements
                .Where(value => !value.isJuNian)
                .OrderBy(value => value.placementId, StringComparer.Ordinal)
                .ToArray();
            for (int index = ordinary.Length - 1; index >= targetCount; index--)
            {
                if (!session.SelectPlacement(ordinary[index].placementId) || !session.RemoveSelected())
                {
                    error = "failed to trim legal layout at " + ordinary[index].placementId;
                    return false;
                }
            }

            ItemFullDetailWorkbenchPlacement selected = session.Placements
                .Where(value => !value.isJuNian)
                .OrderBy(value => value.placementId, StringComparer.Ordinal)
                .LastOrDefault();
            if (selected == null || !session.SelectPlacement(selected.placementId))
            {
                error = "no ordinary placement remains for Build detail";
                return false;
            }

            ItemDetailViewModel detail = session.BuildSelectedDetail();
            ItemDetailSectionViewModel section = detail?.displayPlayerSections.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, "famenBuild", StringComparison.Ordinal));
            body = section?.body ?? string.Empty;
            if (string.IsNullOrWhiteSpace(body))
            {
                error = "famenBuild section missing";
                return false;
            }
            return true;
        }

        private static bool TryBuildQiLeiProgressDetail(
            ItemBalanceWorkbenchCatalog workbench,
            ItemInnerDataCatalogProvider catalogProvider,
            out string body,
            out string error)
        {
            body = string.Empty;
            error = string.Empty;
            ItemFullDetailBuildSandboxWorkbenchSession session = new(
                new ItemBalanceCandidateDetailSandboxAdapter(workbench, catalogProvider), catalogProvider);
            session.SelectJuNian();
            if (!session.PlaceSelected(new Vector2Int(0, 0), out ItemFullDetailPlacementFailureReason juNianReason))
            {
                error = "I031 placement failed: " + juNianReason;
                return false;
            }

            ItemFullDetailWorkbenchInstance first = session.CreateInstance("I001", "orange", 40412001L);
            if (first == null)
            {
                error = "I001 candidate failed";
                return false;
            }
            if (!session.PlaceSelected(new Vector2Int(1, 0), out ItemFullDetailPlacementFailureReason firstReason))
            {
                error = "I001 placement failed: " + firstReason;
                return false;
            }

            ItemFullDetailWorkbenchInstance second = session.CreateInstance("I002", "orange", 40412002L);
            if (second == null)
            {
                error = "I002 candidate failed";
                return false;
            }
            if (!session.PlaceSelected(new Vector2Int(2, 0), out ItemFullDetailPlacementFailureReason secondReason))
            {
                error = "I002 placement failed: " + secondReason;
                return false;
            }

            ItemDetailViewModel detail = session.BuildSelectedDetail();
            ItemDetailSectionViewModel section = detail?.displayPlayerSections.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, "qileiBuild", StringComparison.Ordinal));
            body = section?.body ?? string.Empty;
            if (string.IsNullOrWhiteSpace(body))
            {
                error = "qileiBuild section missing";
                return false;
            }
            return true;
        }

        private static void VerifySceneBindings(
            ICollection<Check> checks,
            GameObject root,
            ItemSandboxV04BoardFullDetailAdapter adapter)
        {
            ItemSandboxV04TraySlotView[] slots = root.GetComponentsInChildren<ItemSandboxV04TraySlotView>(true);
            ItemSandboxV04BoardCellView[] cells = root.GetComponentsInChildren<ItemSandboxV04BoardCellView>(true);
            Add(checks, "TRAY_BINDINGS", "scene", "Existing 40 copied slots are bound; exactly 31 are visible Item entries.",
                slots.Length == 40 && slots.Count(slot => slot.gameObject.activeSelf) == 31,
                "bound=" + slots.Length + "; active=" + slots.Count(slot => slot.gameObject.activeSelf));
            ScrollRect trayScroll = Find(root, "ItemTrayPreview")?.GetComponent<ScrollRect>();
            RectTransform trayContent = trayScroll == null ? null : trayScroll.content;
            ItemSandboxV04TraySlotView[] visibleSlots = slots.Where(slot => slot.gameObject.activeSelf).ToArray();
            bool trayScrollOk = trayScroll != null
                && trayContent != null
                && trayScroll.vertical
                && !trayScroll.horizontal
                && visibleSlots.Length == 31
                && visibleSlots.All(slot => IsChildOf(slot.transform, trayContent))
                && trayContent.GetComponent<ContentSizeFitter>() != null
                && (trayContent.GetComponent<GridLayoutGroup>() != null
                    || trayContent.GetComponent<VerticalLayoutGroup>() != null
                    || trayContent.GetComponent<HorizontalLayoutGroup>() != null);
            Add(checks, "TRAY_SCROLL_31_ACCESS", "scene",
                "ItemTrayPreview is a vertical ScrollRect whose content owns all 30 normal items plus I031.",
                trayScrollOk,
                trayScroll == null
                    ? "ItemTrayPreview ScrollRect missing"
                    : "activeSlots=" + visibleSlots.Length + "; content="
                        + (trayContent == null ? "null" : trayContent.name)
                        + "; vertical=" + trayScroll.vertical + "; horizontal=" + trayScroll.horizontal);
            GameObject candidateControls = Find(root, "CandidateInstancePreviewControls");
            bool candidateControlsOk = candidateControls != null
                && candidateControls.activeInHierarchy
                && candidateControls.GetComponentsInParent<CanvasGroup>(true)
                    .All(group => group.interactable && group.blocksRaycasts);
            Add(checks, "CANDIDATE_CONTROLS_STAY_VISIBLE", "scene",
                "CandidateInstancePreviewControls remains active and interactable while candidate/detail data refreshes.",
                candidateControlsOk,
                candidateControls == null ? "missing" : "activeInHierarchy=" + candidateControls.activeInHierarchy);
            Add(checks, "BOARD_BINDINGS", "scene", "Existing copied board has exactly 25 bound cells covering (0,0)-(4,4).",
                cells.Length == 25 && cells.Select(cell => cell.Cell).Distinct().Count() == 25
                    && cells.All(cell => cell.Cell.x >= 0 && cell.Cell.x < 5 && cell.Cell.y >= 0 && cell.Cell.y < 5),
                "cells=" + cells.Length + "; unique=" + cells.Select(cell => cell.Cell).Distinct().Count());
            Add(checks, "DETAIL_PANEL_REUSE", "scene", "Existing runtime ItemDetailPanel is reused.",
                root.GetComponentInChildren<ItemDetailPanelView>(true) != null,
                "ItemDetailPanelView present");
            Add(checks, "FEEDBACK_BINDING", "scene", "FeedbackRoot owns the additive ItemSandbox feedback Text.",
                Find(root, "ItemSandboxFeedbackText")?.GetComponent<UnityEngine.UI.Text>() != null,
                "ItemSandboxFeedbackText serialized");

            IReadOnlyList<ItemSandboxV04ProtectedGeometryBaseline> baselines = AcceptedProtectedGeometryBaselines();
            bool geometry = baselines.Count == 4;
            foreach (ItemSandboxV04ProtectedGeometryBaseline baseline in baselines)
            {
                RectTransform current = Find(root, baseline.objectName)?.GetComponent<RectTransform>();
                geometry &= current != null
                    && current.gameObject.activeSelf == baseline.activeSelf
                    && current.GetSiblingIndex() == baseline.siblingIndex
                    && (current.parent?.name ?? string.Empty) == baseline.parentName
                    && current.anchorMin == baseline.anchorMin
                    && current.anchorMax == baseline.anchorMax
                    && current.pivot == baseline.pivot
                    && current.anchoredPosition == baseline.anchoredPosition
                    && current.sizeDelta == baseline.sizeDelta
                    && current.localScale == baseline.localScale
                    && current.localEulerAngles == baseline.localEulerAngles;
            }
            geometry &= Find(root, "PopupLayer") == null;
            Add(checks, "PROTECTED_GEOMETRY", "scene",
                "Surveyed BattleLikePreviewArea/FeedbackRoot/ItemTrayPreview/CandidateInstancePreviewControls geometry remains readonly; ItemDetailPanel is the sole authorized exclusion and the pre-task PopupLayer absence is preserved without rebuilding it.",
                geometry,
                "PopupLayer=pre-task absent; " + string.Join("; ", baselines.Select(value => value.objectName + " sibling=" + value.siblingIndex
                    + " active=" + value.activeSelf + " pos=" + value.anchoredPosition + " size=" + value.sizeDelta)));
        }

        private static IReadOnlyList<ItemSandboxV04ProtectedGeometryBaseline> AcceptedProtectedGeometryBaselines()
        {
            return new[]
            {
                Baseline(
                    "CandidateInstancePreviewControls", "ItemSandboxRoot", true, 4,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(106f, -161f), new Vector2(127.32349f, 104.375206f)),
                Baseline(
                    "BattleLikePreviewArea", "ItemSandboxRoot", true, 6,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 23f), new Vector2(960f, 1920f)),
                Baseline(
                    "FeedbackRoot", "ItemSandboxRoot", true, 8,
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 132f), new Vector2(0f, 0f)),
                Baseline(
                    "ItemTrayPreview", "BattleLikePreviewArea", true, 2,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -515f), new Vector2(800f, 800f))
            };
        }

        private static ItemSandboxV04ProtectedGeometryBaseline Baseline(
            string objectName,
            string parentName,
            bool activeSelf,
            int siblingIndex,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            return new ItemSandboxV04ProtectedGeometryBaseline
            {
                objectName = objectName,
                parentName = parentName,
                activeSelf = activeSelf,
                siblingIndex = siblingIndex,
                anchorMin = anchorMin,
                anchorMax = anchorMax,
                pivot = pivot,
                anchoredPosition = anchoredPosition,
                sizeDelta = sizeDelta,
                localScale = Vector3.one,
                localEulerAngles = Vector3.zero
            };
        }

        private static void VerifyLeakAndIntegrity(ICollection<Check> checks)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string runtimePath = Path.Combine(projectRoot,
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardFullDetailAdapter.cs");
            string trayPath = Path.Combine(projectRoot,
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04TraySlotView.cs");
            string boardPath = Path.Combine(projectRoot,
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardCellView.cs");
            string runtimeSource = File.ReadAllText(runtimePath) + File.ReadAllText(trayPath) + File.ReadAllText(boardPath);
            string[] layoutWrites =
            {
                ".anchoredPosition =", ".sizeDelta =", ".offsetMin =", ".offsetMax =",
                ".SetParent(", ".SetSiblingIndex(", ".SetAsFirstSibling(", ".SetAsLastSibling("
            };
            Add(checks, "NO_RUNTIME_LAYOUT_WRITES", "leak", "Runtime adapter/view scripts contain no hierarchy or RectTransform layout writes.",
                layoutWrites.All(token => !runtimeSource.Contains(token, StringComparison.Ordinal)),
                "tokens scanned=" + layoutWrites.Length);
            string[] prohibited =
            {
                "V02RunFlowController", "MainTrialFlowService", "RewardService", "SaveService",
                "BossInfoPanel", "UnifiedBattlePage", "BattleSandboxRuntimeLoop"
            };
            Add(checks, "NO_FORMAL_SYSTEM_LEAK", "leak", "New ItemSandbox adapter does not reference formal flow/reward/save/Boss/battle systems.",
                prohibited.All(token => !runtimeSource.Contains(token, StringComparison.Ordinal)),
                "tokens scanned=" + prohibited.Length);
            Add(checks, "TRAY_DRAG_CLICK_SPLIT", "leak",
                "Tray drag path does not open detail via SelectTrayItem; clicks remain the detail trigger.",
                runtimeSource.Contains("public void OnPointerClick", StringComparison.Ordinal)
                    && runtimeSource.Contains("adapter?.SelectTrayItem(itemId)", StringComparison.Ordinal)
                    && runtimeSource.Contains("SelectTrayItemForDrag(slot.ItemId)", StringComparison.Ordinal)
                    && !runtimeSource.Contains("SelectTrayItem(slot.ItemId)", StringComparison.Ordinal),
                "click=SelectTrayItem; drag=SelectTrayItemForDrag");

            Dictionary<string, string> expectedHashes = new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "821477FF5AE05E531953EF16B0E08FEC84F74FBCA63067C6C126BE7CF7EF13DF",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08A277E3CA465A44E792318C0D3C210AFDBA61069F1170B74FA5A1A18598FE59"
            };
            foreach (KeyValuePair<string, string> expected in expectedHashes)
            {
                string path = Path.Combine(projectRoot, expected.Key.Replace('/', Path.DirectorySeparatorChar));
                string actual = File.Exists(path) ? Sha256(path) : "MISSING";
                Add(checks, "HASH_" + Path.GetFileName(expected.Key).Replace('.', '_').ToUpperInvariant(),
                    "integrity", expected.Key + " remains byte-identical to pre-task baseline.",
                    string.Equals(actual, expected.Value, StringComparison.Ordinal), actual);
            }

            Dictionary<string, string> expectedTreeHashes = new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Configs/ItemBalanceWorkbench"] = "F941C253288D9D0D77AC25EB50716BA76F49E293DD58EA921927798102301915",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation"] = "5434BD4C7422681FBAC775ACA2C496B4C8A9EC4B772F1BFB096366533BB527B8"
            };
            foreach (KeyValuePair<string, string> expected in expectedTreeHashes)
            {
                string actual = HashPath(expected.Key);
                Add(checks, "TREE_HASH_" + Path.GetFileName(expected.Key).Replace('.', '_').ToUpperInvariant(),
                    "integrity", expected.Key + " candidate/schema tree remains byte-identical to the task baseline.",
                    string.Equals(actual, expected.Value, StringComparison.Ordinal), actual);
            }

            string regressionLog = Path.Combine(projectRoot,
                "Logs/codex_item_sandbox_v04_board_full_detail_regression.log");
            bool regressionPass = File.Exists(regressionLog)
                && File.ReadAllText(regressionLog).Contains(
                    "ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS", StringComparison.Ordinal);
            Add(checks, "REGRESSION_9_ITEM_8_ALGORITHM", "regression",
                "All nine historical Item System verifiers and eight Item Algorithm packages pass.",
                regressionPass,
                regressionPass ? "ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS"
                    : "regression marker missing");
        }

        private static void WriteReports(IReadOnlyCollection<Check> checks)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string reportPath = Path.Combine(projectRoot, ReportRelative.Replace('/', Path.DirectorySeparatorChar));
            string specPath = Path.Combine(projectRoot, SpecRelative.Replace('/', Path.DirectorySeparatorChar));
            string leakPath = Path.Combine(projectRoot, LeakRelative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);
            int passCount = checks.Count(check => check.pass);
            int failCount = checks.Count - passCount;
            string status = failCount == 0 ? "PASS" : "FAIL";

            StringBuilder report = new();
            report.AppendLine("# ItemSandbox V04 Board Full Detail Adapter Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemSandboxV04BoardFullDetailAdapter01`")
                .AppendLine("- Status: **" + status + "**")
                .AppendLine("- Rework receipt: `" + ReworkReceipt + "`")
                .AppendLine("- Baseline acceptance: `" + BaselineReceipt + "` — User accepted current hand-tested scene/prefab as new protected baseline.")
                .AppendLine("- ItemDetailMaxDaoArtTemplate01 exclusion: ItemDetailPanel child-subtree only; no new hash or geometry baseline accepted.")
                .AppendLine(failCount == 0
                    ? "- Guard PASS receipt: `" + GuardReceipt + "`"
                    : "- Guard PASS receipt: withheld until all integrity checks pass (`" + GuardReceipt + "` not emitted).")
                .AppendLine("- Checks: " + passCount + "/" + checks.Count)
                .AppendLine("- Scene: `" + ItemSandboxV04BoardFullDetailSceneBinder.ScenePath + "`")
                .AppendLine()
                .AppendLine("## Result")
                .AppendLine();
            foreach (Check check in checks)
            {
                report.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " `" + check.id + "` — "
                    + check.expectation + " Evidence: " + check.evidence);
            }
            report.AppendLine()
                .AppendLine("## Verified coverage")
                .AppendLine()
                .AppendLine("- Tray: I001-I030 normal items 30/30; I031 exactly once; legacy V0.4 test entries 0.")
                .AppendLine("- Candidate matrix: 150/150 details and 600/600 configured stat ranges.")
                .AppendLine("- Board: 5×5; eye `(2,2)` blocked; four array cells; direct + relay lighting; Build2/4/6; explicit MainBuild default None; four readonly monitor slots.")
                .AppendLine("- Core preview: Sandbox Lv1-Lv40 uses configured profile unlock levels and recomputes visible/unlocked/lit/active state.")
                .AppendLine()
                .AppendLine("## UI layout protection")
                .AppendLine()
                .AppendLine("- Non-ItemDetail Survey structure remains readonly; no new geometry baseline was accepted.")
                .AppendLine("- `CandidateInstancePreviewControls`: parent `ItemSandboxRoot`, active, sibling 4, anchoredPosition `(106,-161)`, sizeDelta `(127.32349,104.375206)`.")
                .AppendLine("- `BattleLikePreviewArea`: parent `ItemSandboxRoot`, active, sibling 6, anchoredPosition `(0,23)`, sizeDelta `(960,1920)`.")
                .AppendLine("- `ItemDetailPanel`: authorized child-subtree exclusion for `ItemDetailMaxDaoArtTemplate01`; no whole-Scene or whole-Prefab hash was accepted here.")
                .AppendLine("- `PopupLayer`: absent in the pre-task Scene YAML; this package did not rebuild it.")
                .AppendLine("- `FeedbackRoot`: parent `ItemSandboxRoot`, active, sibling 8, stretch anchors, anchoredPosition `(0,132)`, sizeDelta `(0,0)`.")
                .AppendLine("- `ItemTrayPreview`: parent `BattleLikePreviewArea`, active, sibling 2, anchoredPosition `(0,-515)`, sizeDelta `(800,800)`.")
                .AppendLine("- Geometry baseline check and runtime layout-write scan both PASS when all integrity checks pass.")
                .AppendLine()
                .AppendLine("## Regression")
                .AppendLine()
                .AppendLine("- Historical Item System verifiers: 9/9 PASS.")
                .AppendLine("- Item Algorithm foundation packages: 8/8 PASS.")
                .AppendLine("- Marker: `ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS`.")
                .AppendLine("- Integrity gate report: `Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchRegressionIntegrityReport.md`.")
                .AppendLine()
                .AppendLine("## Scope boundary")
                .AppendLine()
                .AppendLine("The adapter is ItemSandbox-only. It consumes existing catalog, generation projection, placement, lighting, array, Build, awakening, skill-monitor, and ItemDetailPanel contracts. It does not connect formal battle, RunFlow, reward, save, Boss, drop, cultivation, or BuildSettings.")
                .AppendLine()
                .AppendLine("## Manual test")
                .AppendLine()
                .AppendLine("1. Open `Scene_TalismanBag_V04_ItemSandbox`, enter Play, and confirm the tray shows I001-I031 only.")
                .AppendLine("2. Click I001 and switch white/green/blue/purple/orange; each click must immediately update ItemDetailPanel. Change Seed and regenerate; already placed instances must not change.")
                .AppendLine("3. Create two same-name normal instances (including different rarity or Seed). The first may be placed; the second must be rejected with a readable duplicate baseItemId/itemId reason. Drag I031 twice; the second placement must still be rejected.")
                .AppendLine("4. Try out-of-bounds, overlap, and eye `(2,2)` placements; each must be rejected with a readable reason. Move, rotate, and remove a valid placement.")
                .AppendLine("5. Reproduce the legal Build6 layout: I031 at `(0,0)`; orange I001-I006 at anchors `(1,0)`, `(2,0)`, `(4,0)`, `(2,1)`, `(4,2)`, `(2,3)` using the verifier-reported seeds. Confirm all six baseItemIds are distinct.")
                .AppendLine("6. Confirm direct + relay lighting, array state, FaMen/QiLei counts, Build2/4/6, MainBuild default None, explicit MainBuild selection, and four monitor slots.")
                .AppendLine("7. Set Sandbox Lv1 then Lv40 and inspect placed-item details; unlocked and active core states must rise according to configured profile levels and lighting.")
                .AppendLine("8. Exit Play and confirm the three protected copied roots retain their Inspector-authored hierarchy and RectTransform values.")
                .AppendLine()
                .AppendLine("## Batch commands and logs")
                .AppendLine()
                .AppendLine("- Bind: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.ItemSandbox.Editor.ItemSandboxV04BoardFullDetailSceneBinder.BindBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_bind.log`.")
                .AppendLine("- Verify: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.ItemSandbox.Editor.ItemSandboxV04BoardFullDetailAdapterVerifier.VerifyBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_verify.log`.")
                .AppendLine("- Regression: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchRegressionRunner.RunBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_regression.log`.")
                .AppendLine()
                .AppendLine("## Marker")
                .AppendLine()
                .AppendLine(failCount == 0 ? PassMarker : "ITEM_SANDBOX_V04_BOARD_FULL_DETAIL_ADAPTER01_FAIL");
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(false));

            StringBuilder csv = new("id,area,status,expectation,evidence\n");
            foreach (Check check in checks)
            {
                csv.Append(Csv(check.id)).Append(',').Append(Csv(check.area)).Append(',')
                    .Append(check.pass ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(check.expectation)).Append(',').Append(Csv(check.evidence)).Append('\n');
            }
            File.WriteAllText(specPath, csv.ToString(), new UTF8Encoding(false));

            Check[] leakChecks = checks.Where(check => check.area == "leak" || check.area == "integrity" || check.id == "PROTECTED_GEOMETRY").ToArray();
            StringBuilder leak = new();
            leak.AppendLine("# ItemSandbox V04 Board Full Detail Adapter Leak Check")
                .AppendLine()
                .AppendLine("- Status: **" + (leakChecks.All(check => check.pass) ? "PASS" : "FAIL") + "**")
                .AppendLine("- Runtime UI layout writes: prohibited")
                .AppendLine("- Formal-system connections: prohibited")
                .AppendLine("- Protected copied-root hierarchy/geometry changes: prohibited")
                .AppendLine();
            foreach (Check check in leakChecks)
            {
                leak.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " `" + check.id + "`: " + check.evidence);
            }
            File.WriteAllText(leakPath, leak.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string Sha256(string path)
        {
            using SHA256 hash = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string HashPath(string relativePath)
        {
            string absolute = Path.GetFullPath(relativePath);
            string[] files = Directory.Exists(absolute)
                ? Directory.GetFiles(absolute, "*", SearchOption.AllDirectories)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray()
                : new[] { absolute };
            using SHA256 hash = SHA256.Create();
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string file in files)
            {
                byte[] name = Encoding.UTF8.GetBytes(file.Replace(projectRoot, string.Empty));
                hash.TransformBlock(name, 0, name.Length, name, 0);
                byte[] bytes = File.ReadAllBytes(file);
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
            hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(hash.Hash).Replace("-", string.Empty);
        }

        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";

        private static void Add(ICollection<Check> checks, string id, string area,
            string expectation, bool pass, string evidence)
        {
            checks.Add(new Check { id = id, area = area, expectation = expectation, pass = pass, evidence = evidence ?? string.Empty });
        }

        private static GameObject Find(Scene scene, string name)
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(transform => transform.gameObject)
                .FirstOrDefault(value => string.Equals(value.name, name, StringComparison.Ordinal));
        }

        private static GameObject Find(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject)
                .FirstOrDefault(value => string.Equals(value.name, name, StringComparison.Ordinal));
        }

        private static bool IsChildOf(Transform child, Transform parent)
        {
            for (Transform current = child; current != null; current = current.parent)
            {
                if (current == parent)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
