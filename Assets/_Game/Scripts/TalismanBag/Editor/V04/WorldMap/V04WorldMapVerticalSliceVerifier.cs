#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.V04.ChapterFlow;
using TalismanBag.V04.WorldMap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.Editor.V04.WorldMap
{
    public static class V04WorldMapVerticalSliceVerifier
    {
        public const string ReportPath =
            "Docs/V0.4/Reports/WorldMapQingshifangStageLoopVerticalSlice01Report.md";
        private const string FullMenu =
            "Tools/TalismanBag/V0.4/World Map/Verify Qingshifang World Map Vertical Slice 01";
        private const string CatalogMenu =
            "Tools/TalismanBag/V0.4/World Map/Verify World Map Catalog Only";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapState.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapChapterEntryView.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapStageNodeView.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapDropPreviewRowView.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapStageDetailDrawerView.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/WorldMap/V04WorldMapSceneController.cs"
        };

        private static readonly string[] ForbiddenRuntimeHierarchyWrites =
        {
            "new GameObject(",
            "new GameObject (",
            ".AddComponent<",
            ".SetParent(",
            ".SetSiblingIndex(",
            ".SetAsFirstSibling(",
            ".SetAsLastSibling(",
            ".anchoredPosition =",
            ".sizeDelta =",
            ".anchorMin =",
            ".anchorMax =",
            ".pivot ="
        };
        public static void VerifyFromMenu()
        {
            VerificationResult result = Verify(includeScene: true);
            WriteReport(result);
            LogResult(result);
        }
        public static void VerifyCatalogFromMenu()
        {
            VerificationResult result = Verify(includeScene: false);
            WriteReport(result);
            LogResult(result);
        }

        public static void RunBatch()
        {
            VerificationResult result = Verify(includeScene: true);
            WriteReport(result);
            LogResult(result);
            EditorApplication.Exit(result.Passed ? 0 : 1);
        }

        public static void RunCatalogBatch()
        {
            VerificationResult result = Verify(includeScene: false);
            WriteReport(result);
            LogResult(result);
            EditorApplication.Exit(result.Passed ? 0 : 1);
        }

        private static VerificationResult Verify(bool includeScene)
        {
            VerificationResult result = new(includeScene);
            VerifyCatalog(result);
            VerifyDefaultState(result);
            VerifyBridgeContracts(result);
            VerifyRuntimeLock(result);
            if (includeScene)
            {
                VerifyScene(result);
            }

            return result;
        }

        private static void VerifyCatalog(VerificationResult result)
        {
            Expect(
                result,
                V04WorldMapCatalog.World.regions.Count == 1,
                "world.regions",
                "1",
                V04WorldMapCatalog.World.regions.Count.ToString());
            Expect(
                result,
                V04WorldMapCatalog.Chapters.Count == 4,
                "region.chapters",
                "4",
                V04WorldMapCatalog.Chapters.Count.ToString());
            Expect(
                result,
                V04WorldMapCatalog.ChapterOneStages.Count == 10,
                "chapter1.stages",
                "10",
                V04WorldMapCatalog.ChapterOneStages.Count.ToString());
            Expect(
                result,
                V04WorldMapCatalog.StageVisualStates.Count == 4,
                "stage.visualStates",
                "Locked/Available/Cleared/Boss",
                string.Join(
                    "/",
                    V04WorldMapCatalog.StageVisualStates.Select(row => row.state)));

            string[] expectedChapterIds =
            {
                "bone_aspect_chapter_1",
                "bone_aspect_chapter_2",
                "bone_aspect_chapter_3",
                "bone_aspect_chapter_4"
            };
            Expect(
                result,
                V04WorldMapCatalog.Chapters
                    .Select(row => row.chapterId)
                    .SequenceEqual(expectedChapterIds, StringComparer.Ordinal),
                "chapter.stableIds",
                string.Join("|", expectedChapterIds),
                string.Join(
                    "|",
                    V04WorldMapCatalog.Chapters.Select(row => row.chapterId)));

            for (int index = 0; index < 10; index++)
            {
                V04WorldMapStageDefinition mapStage =
                    V04WorldMapCatalog.ChapterOneStages[index];
                V04ChapterStageDefinition chapterFlowStage =
                    V04ChapterFlowManifest.FindStage("1-" + (index + 1));
                Expect(
                    result,
                    chapterFlowStage != null
                    && string.Equals(
                        mapStage.stageId,
                        chapterFlowStage.stageId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        mapStage.chapterId,
                        chapterFlowStage.chapterId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        mapStage.encounterNodeId,
                        chapterFlowStage.encounterNodeId,
                        StringComparison.Ordinal),
                    "stage.identity." + mapStage.stageId,
                    "read-only ChapterFlow identity match",
                    chapterFlowStage == null ? "missing" : "matched");
                Expect(
                    result,
                    mapStage.dropPreviewRows.Count == 3
                    && mapStage.dropPreviewRows.All(
                        row => string.Equals(
                            row.bindingStatus,
                            V04WorldMapCatalog.UnboundDropStatus,
                            StringComparison.Ordinal)),
                    "stage.dropPreview." + mapStage.stageId,
                    "3 placeholder rows, owner unbound",
                    mapStage.dropPreviewRows.Count.ToString());
            }

            V04WorldMapStageDefinition boss =
                V04WorldMapCatalog.FindChapterOneStage("1-10");
            Expect(
                result,
                boss != null
                && boss.isBossStage
                && string.Equals(
                    boss.bossProfileId,
                    "bone_aspect_boss_c1_bone_guard",
                    StringComparison.Ordinal),
                "stage.1-10.boss",
                "boss identity only",
                boss?.bossProfileId ?? "missing");
        }

        private static void VerifyDefaultState(VerificationResult result)
        {
            V04WorldMapProgressSnapshot unbound =
                V04WorldMapProgressSnapshot.CreateUnboundPreview();
            Expect(
                result,
                !unbound.formalProgress
                && unbound.clearedStageIds.Count == 0
                && string.Equals(
                    unbound.authority,
                    V04WorldMapProgressSnapshot.UnboundAuthority,
                    StringComparison.Ordinal),
                "progress.default",
                "unbound/non-formal/zero cleared",
                unbound.authority);
            ExpectStageState(
                result,
                "1-1",
                unbound,
                V04WorldMapStageVisualState.Available);
            ExpectStageState(
                result,
                "1-2",
                unbound,
                V04WorldMapStageVisualState.Locked);
            ExpectStageState(
                result,
                "1-10",
                unbound,
                V04WorldMapStageVisualState.Boss);

            V04WorldMapProgressSnapshot explicitCleared =
                new(
                    "VERIFIER_EXPLICIT_FIXTURE",
                    new[] { "1-1" },
                    new[] { "1-2" },
                    string.Empty,
                    false);
            ExpectStageState(
                result,
                "1-1",
                explicitCleared,
                V04WorldMapStageVisualState.Cleared);
        }

        private static void VerifyBridgeContracts(VerificationResult result)
        {
            V04WorldMapBattleEntryRequest battleRequest = new();
            V04WorldMapPatrolSelectionRequest patrolRequest = new();
            V04WorldMapOfflineSettlementRequest offlineRequest = new();
            Expect(
                result,
                battleRequest.devOnly && !battleRequest.formalFlow,
                "bridge.battle.default",
                "devOnly=true formalFlow=false",
                $"devOnly={battleRequest.devOnly} formalFlow={battleRequest.formalFlow}");
            Expect(
                result,
                patrolRequest.requiresClearedStage
                && patrolRequest.devOnly
                && !patrolRequest.formalFlow,
                "bridge.patrol.default",
                "requiresClearedStage/devOnly/non-formal",
                $"requiresClearedStage={patrolRequest.requiresClearedStage}");
            Expect(
                result,
                !offlineRequest.simulatePerFrameBattle
                && offlineRequest.devOnly
                && !offlineRequest.formalFlow,
                "bridge.offline.default",
                "no per-frame battle simulation",
                $"simulatePerFrameBattle={offlineRequest.simulatePerFrameBattle}");
        }

        private static void VerifyRuntimeLock(VerificationResult result)
        {
            int forbiddenHits = 0;
            foreach (string path in RuntimeSourcePaths)
            {
                if (!File.Exists(path))
                {
                    result.Errors.Add("runtime source missing: " + path);
                    continue;
                }

                string source = File.ReadAllText(path);
                foreach (string forbidden in ForbiddenRuntimeHierarchyWrites)
                {
                    if (source.IndexOf(forbidden, StringComparison.Ordinal) < 0)
                    {
                        continue;
                    }

                    forbiddenHits++;
                    result.Errors.Add(
                        "runtime hierarchy/layout write hit path=" + path +
                        " token=" + forbidden);
                }
            }

            Expect(
                result,
                forbiddenHits == 0,
                "runtimeLock.forbiddenHierarchyWrites",
                "0",
                forbiddenHits.ToString());
        }

        private static void VerifyScene(VerificationResult result)
        {
            if (!File.Exists(V04WorldMapSceneController.ScenePath))
            {
                result.Errors.Add(
                    "SCENE_AUTHORING_WAITING_FOR_CLEAN_UNITY_LEASE path=" +
                    V04WorldMapSceneController.ScenePath);
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(
                V04WorldMapSceneController.ScenePath,
                OpenSceneMode.Single);
            V04WorldMapSceneController controller =
                UnityEngine.Object.FindObjectOfType<V04WorldMapSceneController>(true);
            Expect(
                result,
                controller != null,
                "scene.controller",
                "1",
                controller == null ? "0" : "1");
            if (controller != null)
            {
                Expect(
                    result,
                    controller.ValidateAuthoredBindings(out string diagnostic),
                    "scene.authoredBindings",
                    "valid",
                    string.IsNullOrEmpty(diagnostic) ? "valid" : diagnostic);
            }

            Transform[] transforms = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToArray();
            VerifyUniqueName(result, transforms, "WorldMapCanvas");
            VerifyUniqueName(result, transforms, "WorldRegionView");
            VerifyUniqueName(result, transforms, "QingshifangRegionView");
            VerifyUniqueName(result, transforms, "ChapterStageMapView");
            VerifyUniqueName(result, transforms, "StageDetailDrawer");
            VerifyCountByPrefix(result, transforms, "ChapterEntry_", 4);
            VerifyCountByPrefix(result, transforms, "StageNode_", 10);
            VerifyCountByPrefix(result, transforms, "DropPreviewRow_", 3);
            Expect(
                result,
                !scene.isDirty,
                "scene.verifierReadOnly",
                "scene.isDirty=false",
                "scene.isDirty=" + scene.isDirty);
        }

        private static void VerifyUniqueName(
            VerificationResult result,
            IEnumerable<Transform> transforms,
            string name)
        {
            int count = transforms.Count(
                transform => string.Equals(transform.name, name, StringComparison.Ordinal));
            Expect(result, count == 1, "scene.unique." + name, "1", count.ToString());
        }

        private static void VerifyCountByPrefix(
            VerificationResult result,
            IEnumerable<Transform> transforms,
            string prefix,
            int expected)
        {
            int count = transforms.Count(
                transform => transform.name.StartsWith(prefix, StringComparison.Ordinal));
            Expect(
                result,
                count == expected,
                "scene.count." + prefix,
                expected.ToString(),
                count.ToString());
        }

        private static void ExpectStageState(
            VerificationResult result,
            string stageId,
            V04WorldMapProgressSnapshot progress,
            V04WorldMapStageVisualState expected)
        {
            V04WorldMapStageVisualState actual =
                V04WorldMapStageStateResolver.Resolve(
                    V04WorldMapCatalog.FindChapterOneStage(stageId),
                    progress);
            Expect(
                result,
                actual == expected,
                "stage.state." + stageId + "." + expected,
                expected.ToString(),
                actual.ToString());
        }

        private static void Expect(
            VerificationResult result,
            bool passed,
            string assertionId,
            string expected,
            string actual)
        {
            result.Assertions.Add(
                new VerificationAssertion(assertionId, expected, actual, passed));
            if (!passed)
            {
                result.Errors.Add(
                    assertionId + " expected=" + expected + " actual=" + actual);
            }
        }

        private static void WriteReport(VerificationResult result)
        {
            StringBuilder report = new();
            report.AppendLine("# V0.4 World Map Qingshifang Stage Loop Vertical Slice 01");
            report.AppendLine();
            report.AppendLine("- Catalog schema: `" + V04WorldMapCatalog.SchemaId + "`");
            report.AppendLine("- Verification scope: `" +
                              (result.IncludeScene ? "FULL_WITH_SCENE" : "CODE_AND_DATA_ONLY") +
                              "`");
            report.AppendLine("- Result: `" +
                              (result.Passed ? "PASS" : "FAIL_OR_WAITING") + "`");
            report.AppendLine("- Scene: `" + V04WorldMapSceneController.ScenePath + "`");
            report.AppendLine();
            report.AppendLine("## Assertions");
            report.AppendLine();
            report.AppendLine("| Assertion | Expected | Actual | Result |");
            report.AppendLine("| --- | --- | --- | --- |");
            foreach (VerificationAssertion assertion in result.Assertions)
            {
                report.Append("| `")
                    .Append(Escape(assertion.AssertionId))
                    .Append("` | ")
                    .Append(Escape(assertion.Expected))
                    .Append(" | ")
                    .Append(Escape(assertion.Actual))
                    .Append(" | ")
                    .Append(assertion.Passed ? "PASS" : "FAIL")
                    .AppendLine(" |");
            }

            report.AppendLine();
            report.AppendLine("## Errors / waiting conditions");
            report.AppendLine();
            if (result.Errors.Count == 0)
            {
                report.AppendLine("- None.");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    report.AppendLine("- " + error);
                }
            }

            report.AppendLine();
            report.AppendLine("## Ownership boundary");
            report.AppendLine();
            report.AppendLine(
                "- WorldMap owns display/navigation shell only; ChapterFlow IDs are read-only inputs.");
            report.AppendLine(
                "- No formal Save, Reward, Drop, Battle executor, Patrol settlement, or offline reward owner is connected.");
            report.AppendLine(
                "- Runtime binds authored objects and updates state/text only; it does not create final UI or write geometry.");
            report.AppendLine();
            report.AppendLine(result.Passed
                ? "WORLD_MAP_QINGSHIFANG_VERTICAL_SLICE01_PASS"
                : "WORLD_MAP_QINGSHIFANG_VERTICAL_SLICE01_FAIL_OR_WAITING");

            string directory = Path.GetDirectoryName(ReportPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(ReportPath, report.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static void LogResult(VerificationResult result)
        {
            if (result.Passed)
            {
                Debug.Log(
                    "WORLD_MAP_QINGSHIFANG_VERTICAL_SLICE01_PASS " +
                    "regions=1 chapters=4 chapter1Stages=10 visualStates=4");
                return;
            }

            foreach (string error in result.Errors)
            {
                Debug.LogError("[V0.4-WorldMapVerifier] " + error);
            }

            Debug.Log(
                "WORLD_MAP_QINGSHIFANG_VERTICAL_SLICE01_FAIL_OR_WAITING errors=" +
                result.Errors.Count);
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private sealed class VerificationResult
        {
            public readonly bool IncludeScene;
            public readonly List<VerificationAssertion> Assertions = new();
            public readonly List<string> Errors = new();

            public VerificationResult(bool includeScene)
            {
                IncludeScene = includeScene;
            }

            public bool Passed => Errors.Count == 0;
        }

        private sealed class VerificationAssertion
        {
            public readonly string AssertionId;
            public readonly string Expected;
            public readonly string Actual;
            public readonly bool Passed;

            public VerificationAssertion(
                string assertionId,
                string expected,
                string actual,
                bool passed)
            {
                AssertionId = assertionId;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }
        }
    }
}
#endif
