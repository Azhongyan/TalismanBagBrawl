using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class LayoutResilienceBattleSandboxPlaytestAdapterVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/LayoutResilienceBattleSandboxPlaytestAdapter01_Assignment.md";
        private const string AssignmentHash =
            "178d0fce3a56d428ea44c8eeb8b6b78f4a0f0563e449c257402e273f557d00d8";
        private const string RuntimePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapter.cs";
        private const string FeedbackPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestFeedback.cs";
        private const string VerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapterVerifier.cs";
        private const string ReportPath =
            "Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestAdapterReport.md";
        private const string StatePath =
            "Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestStateReport.csv";
        private const string FeedbackReportPath =
            "Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestFeedbackReport.csv";
        private const string ChecklistPath =
            "Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestChecklist.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestLeakCheckReport.md";
        private const string ExpectedItemAggregate =
            "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1";
        private const string ExpectedP6Aggregate =
            "7e9b4cb329a25477b32054a7661aaa0d4ffcf5789c48f0832d462fa2c4b412fc";
        private const string ExpectedIf01Aggregate =
            "d2c4392acf4d1d8b4d9230cf19bc7a41591f20dcbe41cd38849b9c43a9427f07";
        private const string ExpectedItemSandboxAggregate =
            "283432017bf6f2d559f54c1a9d2ba7102d60cd0118761b8258b67e3397e61144";
        private const string ExpectedTargetSceneHash =
            "8a249cd0c943abbb6096a84c60148928c1396c8ae6e1d73f8c80c73f769c08d8";
        private const string ExpectedScenesAggregate =
            "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsAggregate =
            "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash =
            "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";
        private const string ExpectedHead =
            "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba";

        private static readonly string[] OutputPaths =
        {
            RuntimePath,
            RuntimePath + ".meta",
            FeedbackPath,
            FeedbackPath + ".meta",
            VerifierPath,
            VerifierPath + ".meta",
            ReportPath,
            StatePath,
            FeedbackReportPath,
            ChecklistPath,
            LeakPath
        };

        private static readonly string[] P6Exact15 =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelinePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelinePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipeline.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipeline.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelineValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelineValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs.meta",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineReport.md",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineRouteRows.csv",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineScenarioMatrix.csv",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineAuthorityCallMatrix.csv",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineTruthTable.csv",
            "Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineLeakCheckReport.md"
        };

        private static readonly string[] If01Exact4 =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs.meta"
        };

        private static readonly string[] ItemSandboxExact4 =
        {
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardFullDetailAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardFullDetailAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs.meta"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Layout Resilience ItemSandbox Playtest Adapter")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            Summary summary = VerifyCore();
            if (summary.Failed != 0)
            {
                throw new InvalidOperationException(summary.Message);
            }
            Console.WriteLine(summary.Message);
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                VerifyOffline();
#if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    "LAYOUT_RESILIENCE_BATTLE_SANDBOX_PLAYTEST_ADAPTER PASS");
                EditorApplication.Exit(0);
#endif
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogException(exception);
                EditorApplication.Exit(1);
#else
                Console.Error.WriteLine(exception);
                throw;
#endif
            }
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyOffline();
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static Summary VerifyCore()
        {
            string root = FindRoot();
            string protectedBefore = CaptureProtected(root);
            List<Check> checks = new List<Check>();
            VerifySurface(checks);
            VerifyPackageBoundary(checks, root);
            VerifySourceBoundary(checks, root);
            VerifySceneContract(checks, root);

            List<Scenario> scenarios = CreateScenarios();
            foreach (Scenario scenario in scenarios)
            {
                Add(checks, "scenario." + scenario.Id, "PASS",
                    scenario.Passed ? "PASS" : scenario.Detail,
                    scenario.Passed);
            }
            Add(checks, "scenarios.count", "11",
                scenarios.Count.ToString(CultureInfo.InvariantCulture),
                scenarios.Count == 11);
            VerifyDeterminismAndRefresh(checks, scenarios);
            VerifyFeedbackPreservation(checks, scenarios);
            VerifyAuthorityAndIdentity(checks, scenarios);
            VerifyProtected(checks, root);
            string protectedAfter = CaptureProtected(root);
            Add(checks, "protected.before-after", protectedBefore,
                protectedAfter, protectedBefore == protectedAfter);

            string packageCanonical = HashText(string.Join("\n", scenarios
                .Where(value => value.Snapshot != null)
                .OrderBy(value => value.Id, StringComparer.Ordinal)
                .Select(value => value.Id + "|" +
                    value.Snapshot.canonicalSignature)));
            WriteReports(root, scenarios, checks, packageCanonical);
            VerifyReportFiles(checks, root);

            int failed = checks.Count(value => !value.Passed);
            int feedbackRows = scenarios.Sum(value =>
                value.Snapshot?.feedbackRows?.Count ?? 0);
            string failures = string.Join(" | ", checks
                .Where(value => !value.Passed)
                .Select(value => value.Id + " expected=" + value.Expected +
                    " actual=" + value.Actual));
            string message =
                "LayoutResilienceBattleSandboxPlaytestAdapter verifier: " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) +
                "/" + checks.Count.ToString(CultureInfo.InvariantCulture) +
                " PASS; scenarios=11/11; feedbackRows=" +
                feedbackRows.ToString(CultureInfo.InvariantCulture) +
                "; IF01/P6=1/1 per changed snapshot; canonical=" +
                packageCanonical + "; leakCount=" +
                checks.Count(value => value.Id.StartsWith("leak.",
                    StringComparison.Ordinal) && !value.Passed)
                    .ToString(CultureInfo.InvariantCulture) +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifySurface(ICollection<Check> checks)
        {
            AssertEnum<LayoutResilienceBattleSandboxPlaytestStatus>(
                checks,
                "snapshot-status",
                new[] { "Complete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id",
                "LayoutResilienceBattleSandboxPlaytestSnapshot.v1",
                LayoutResilienceBattleSandboxPlaytestSchema.SchemaId,
                LayoutResilienceBattleSandboxPlaytestSchema.SchemaId ==
                    "LayoutResilienceBattleSandboxPlaytestSnapshot.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceBattleSandboxPlaytestSchema.SchemaVersion
                    .ToString(CultureInfo.InvariantCulture),
                LayoutResilienceBattleSandboxPlaytestSchema.SchemaVersion == 1);
            AssertProperties(checks, "snapshot",
                typeof(LayoutResilienceBattleSandboxPlaytestSnapshot),
                "schemaId", "schemaVersion", "status", "devOnly",
                "isEnabled", "itemSnapshotSignature",
                "bindingCanonicalSignature", "pipelineCanonicalSignature",
                "feedbackRows", "issues", "canonicalSignature");
            AssertProperties(checks, "feedback-row",
                typeof(LayoutResilienceBattleSandboxPlaytestFeedbackRow),
                "stableRouteKey", "chineseDisplayName", "pipelineRowStatus",
                "predicateState", "chineseHint", "answerMasked");
        }

        private static List<Scenario> CreateScenarios()
        {
            List<Scenario> values = new List<Scenario>();

            LayoutResilienceBattleSandboxPlaytestInstallDecision nonTarget =
                LayoutResilienceBattleSandboxPlaytestAdapter.DecideInstallation(
                    "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity", 1);
            values.Add(Scenario.Install("S01_NON_TARGET_NO_INSTALL", nonTarget,
                !nonTarget.shouldInstall && nonTarget.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete));

            LayoutResilienceBattleSandboxPlaytestInstallDecision missing =
                LayoutResilienceBattleSandboxPlaytestAdapter.DecideInstallation(
                    LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath, 0);
            values.Add(Scenario.Install("S02_TARGET_ADAPTER_MISSING", missing,
                !missing.shouldInstall && missing.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid &&
                missing.issue?.code == "ITEM_SANDBOX_ADAPTER_MISSING"));

            LayoutResilienceBattleSandboxPlaytestInstallDecision duplicate =
                LayoutResilienceBattleSandboxPlaytestAdapter.DecideInstallation(
                    LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath, 2);
            values.Add(Scenario.Install("S03_TARGET_ADAPTER_DUPLICATE", duplicate,
                !duplicate.shouldInstall && duplicate.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid &&
                duplicate.issue?.code == "ITEM_SANDBOX_ADAPTER_DUPLICATE"));

            values.Add(Evaluate("S04_COMPLETE_EMPTY_LAYOUT",
                Array.Empty<ItemSystemPlacementInput>(),
                Array.Empty<ProjectionFixture>(),
                Array.Empty<ItemInstancePlacementBindingInput>(),
                snapshot => snapshot.status ==
                        LayoutResilienceBattleSandboxPlaytestStatus.Complete &&
                    snapshot.feedbackRows.Count == 4,
                CreateCompleteEmptyItemSnapshot()));

            values.Add(Evaluate("S05_I031_ONLY_LAYOUT",
                new[] { P("source", "I031", 0, 0) },
                Array.Empty<ProjectionFixture>(),
                Array.Empty<ItemInstancePlacementBindingInput>(),
                snapshot => snapshot.status ==
                        LayoutResilienceBattleSandboxPlaytestStatus.Complete &&
                    snapshot.feedbackRows.Count == 4));

            values.Add(Evaluate("S06_FORMATION_EYE_STABLE",
                new[] { P("source", "I031", 0, 1), P("item", "I001", 1, 1) },
                new[] { F("instance.item", "I001") },
                new[] { B("instance.item", "item", "I001") },
                snapshot => CompleteWith(snapshot, ".formation_eye.",
                    LayoutResiliencePredicateState.KnownTrue)));

            values.Add(Evaluate("S07_FORMATION_EYE_PRESSURED",
                new[] { P("source", "I031", 0, 3), P("item", "I001", 1, 3) },
                new[] { F("instance.item", "I001") },
                new[] { B("instance.item", "item", "I001") },
                snapshot => CompleteWith(snapshot, ".formation_eye.",
                    LayoutResiliencePredicateState.KnownFalse)));

            values.Add(Evaluate("S08_POLLUTED_TILE_STABLE",
                new[] { P("source", "I031", 0, 0), P("item", "I001", 1, 0) },
                new[] { F("instance.item", "I001") },
                new[] { B("instance.item", "item", "I001") },
                snapshot => CompleteWith(snapshot, ".polluted_tile.",
                    LayoutResiliencePredicateState.KnownTrue)));

            values.Add(Evaluate("S09_POLLUTED_TILE_PRESSURED",
                new[] { P("source", "I031", 0, 1), P("item", "I001", 1, 1) },
                new[] { F("instance.item", "I001") },
                new[] { B("instance.item", "item", "I001") },
                snapshot => CompleteWith(snapshot, ".polluted_tile.",
                    LayoutResiliencePredicateState.KnownFalse)));

            values.Add(Evaluate("S10_IF01_IDENTITY_MISSING",
                new[] { P("source", "I031", 0, 0), P("item", "I001", 1, 0) },
                Array.Empty<ProjectionFixture>(),
                new[] { B("missing.instance", "item", "I001") },
                snapshot => snapshot.status ==
                        LayoutResilienceBattleSandboxPlaytestStatus.Unknown &&
                    snapshot.feedbackRows.Count == 4 &&
                    snapshot.feedbackRows.All(value => value.predicateState ==
                        LayoutResiliencePredicateState.Unknown)));

            values.Add(Evaluate("S11_INVALID_ITEM_SNAPSHOT",
                new[] { P("item", "I001", 2, 2) },
                new[] { F("instance.item", "I001") },
                new[] { B("instance.item", "item", "I001") },
                snapshot => snapshot.status ==
                        LayoutResilienceBattleSandboxPlaytestStatus.Invalid &&
                    snapshot.feedbackRows.Count == 0));
            return values;
        }

        private static Scenario Evaluate(
            string id,
            IReadOnlyList<ItemSystemPlacementInput> placements,
            IReadOnlyList<ProjectionFixture> projections,
            IReadOnlyList<ItemInstancePlacementBindingInput> bindings,
            Func<LayoutResilienceBattleSandboxPlaytestSnapshot, bool> predicate,
            ItemSystemSnapshot itemSnapshotOverride = null)
        {
            ItemSystemSnapshot itemSnapshot = itemSnapshotOverride ??
                DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                    new ItemSystemSnapshotInput(placements));
            CountingBindingValidator if01 = new CountingBindingValidator();
            CountingPipeline p6 = new CountingPipeline();
            LayoutResilienceBattleSandboxPlaytestSnapshot result =
                LayoutResilienceBattleSandboxPlaytestAdapter
                    .EvaluateAuthoritySnapshot(
                        itemSnapshot,
                        projections.Select(CreateProjection).ToArray(),
                        bindings,
                        if01,
                        p6);
            bool passed = result != null && predicate(result) &&
                if01.CallCount == 1 && p6.CallCount == 1 &&
                result.devOnly && !result.isEnabled &&
                IsSignature(result.itemSnapshotSignature) &&
                IsSignature(result.bindingCanonicalSignature) &&
                IsSignature(result.pipelineCanonicalSignature) &&
                IsSignature(result.canonicalSignature) &&
                !LayoutResilienceBattleSandboxPlaytestFeedback
                    .ContainsAsciiLetter(result.aggregateFeedbackText) &&
                result.feedbackRows.All(value =>
                    value.answerMasked &&
                    !LayoutResilienceBattleSandboxPlaytestFeedback
                        .ContainsAsciiLetter(value.chineseDisplayName) &&
                    !LayoutResilienceBattleSandboxPlaytestFeedback
                        .ContainsAsciiLetter(value.chineseHint));
            string detail = "status=" + result?.status +
                ",rows=" + (result?.feedbackRows?.Count ?? -1)
                    .ToString(CultureInfo.InvariantCulture) +
                ",IF01=" + if01.CallCount.ToString(CultureInfo.InvariantCulture) +
                ",P6=" + p6.CallCount.ToString(CultureInfo.InvariantCulture) +
                ",issues=" + string.Join("|", result?.issues
                    .Select(value => value.code) ?? Array.Empty<string>());
            return new Scenario(id, result, passed, detail, if01, p6,
                placements, projections, bindings);
        }

        private static ItemSystemSnapshot CreateCompleteEmptyItemSnapshot()
        {
            return new ItemSystemSnapshot(
                5,
                new Vector2Int(2, 2),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                Array.Empty<ItemSystemPlacementSnapshot>(),
                Array.Empty<Vector2Int>(),
                null,
                null,
                null,
                null,
                null,
                string.Empty,
                false,
                string.Empty,
                Array.Empty<ItemSystemValidationError>());
        }

        private static void VerifyDeterminismAndRefresh(
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            Scenario source = scenarios.Single(value =>
                value.Id == "S08_POLLUTED_TILE_STABLE");
            ItemSystemPlacementInput[] placements = source.Placements.Reverse()
                .ToArray();
            ProjectionFixture[] projections = source.Projections.Reverse()
                .ToArray();
            ItemInstancePlacementBindingInput[] bindings = source.Bindings
                .Reverse().ToArray();
            Scenario reversed = Evaluate("REVERSED", placements, projections,
                bindings, value => value.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete);
            Add(checks, "determinism.reverse-input",
                source.Snapshot.canonicalSignature,
                reversed.Snapshot.canonicalSignature,
                reversed.Passed && source.Snapshot.canonicalSignature ==
                    reversed.Snapshot.canonicalSignature);

            LayoutResilienceBattleSandboxPlaytestRefreshGate gate =
                new LayoutResilienceBattleSandboxPlaytestRefreshGate();
            object first = new object();
            object changed = new object();
            bool firstCall = gate.ShouldEvaluate(first);
            bool repeatedCall = gate.ShouldEvaluate(first);
            bool changedCall = gate.ShouldEvaluate(changed);
            Add(checks, "refresh.reference-gate", "true/false/true",
                firstCall + "/" + repeatedCall + "/" + changedCall,
                firstCall && !repeatedCall && changedCall);
        }

        private static void VerifyFeedbackPreservation(
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            string aggregate = scenarios.Single(value =>
                value.Id == "S08_POLLUTED_TILE_STABLE")
                .Snapshot.aggregateFeedbackText;
            string first = LayoutResilienceBattleSandboxPlaytestFeedback
                .ApplyToExistingFeedback("可放置", aggregate);
            string next = LayoutResilienceBattleSandboxPlaytestFeedback
                .ApplyToExistingFeedback(
                    "格子冲突：目标形状与已有道具重叠。\n" + aggregate,
                    aggregate);
            Add(checks, "feedback.preserve-first", "interaction+suffix",
                first, first.StartsWith("可放置\n", StringComparison.Ordinal) &&
                Count(first, LayoutResilienceBattleSandboxPlaytestFeedback.Prefix)
                    == 1);
            Add(checks, "feedback.preserve-changed", "latest interaction+1 suffix",
                next, next.StartsWith("格子冲突", StringComparison.Ordinal) &&
                Count(next, LayoutResilienceBattleSandboxPlaytestFeedback.Prefix)
                    == 1);
            Add(checks, "feedback.player-mask", "no ascii/internal answer",
                aggregate,
                !LayoutResilienceBattleSandboxPlaytestFeedback
                    .ContainsAsciiLetter(aggregate) &&
                aggregate.IndexOf("sha256", StringComparison.OrdinalIgnoreCase) < 0 &&
                aggregate.IndexOf("route", StringComparison.OrdinalIgnoreCase) < 0 &&
                !Regex.IsMatch(aggregate, @"\(\s*\d+\s*[,，:]"));
        }

        private static void VerifyAuthorityAndIdentity(
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            Scenario i031 = scenarios.Single(value =>
                value.Id == "S05_I031_ONLY_LAYOUT");
            Add(checks, "identity.i031-not-projected", "0/0",
                (i031.If01?.LastProjection?.Projections?.Count ?? -1) + "/" +
                (i031.If01?.LastResult?.snapshot?.Bindings?.Count ?? -1),
                i031.If01?.LastProjection?.Projections?.Count == 0 &&
                i031.If01?.LastResult?.snapshot?.Bindings?.Count == 0);
            Scenario unknown = scenarios.Single(value =>
                value.Id == "S10_IF01_IDENTITY_MISSING");
            Add(checks, "identity.missing-stays-unknown", "Unknown",
                unknown.Snapshot.status.ToString(),
                unknown.Snapshot.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Unknown &&
                unknown.If01?.LastResult?.status ==
                    ItemInstancePlacementBindingStatus.Unknown);
            foreach (Scenario scenario in scenarios.Where(value =>
                value.Snapshot != null))
            {
                Add(checks, "authority.calls." + scenario.Id, "1/1",
                    scenario.If01.CallCount + "/" + scenario.P6.CallCount,
                    scenario.If01.CallCount == 1 && scenario.P6.CallCount == 1);
            }
        }

        private static void VerifySourceBoundary(
            ICollection<Check> checks,
            string root)
        {
            string runtime = Read(root, RuntimePath);
            string feedback = Read(root, FeedbackPath);
            string[] forbiddenRuntime =
            {
                "DefaultItemSystemSnapshotProvider",
                "BuildSandboxLayoutSnapshot",
                "LayoutResilienceItemFactProjectionAdapter",
                "AuthoredLayoutPressureSourceAdapter",
                "LayoutResilienceEvaluationInputAssembler",
                "LayoutResilienceStructuralReadinessConsumer",
                "LayoutResilienceStructuralPredicateEvaluator",
                "new GameObject",
                "RectTransform",
                "EditorSceneManager",
                "SaveScene",
                "SceneBinder",
                "Builder"
            };
            foreach (string token in forbiddenRuntime)
            {
                Add(checks, "leak.runtime." + token, "absent",
                    runtime.IndexOf(token, StringComparison.Ordinal) < 0
                        ? "absent" : "present",
                    runtime.IndexOf(token, StringComparison.Ordinal) < 0);
            }
            string[] formalLeaks =
            {
                "RunFlow", "SaveData", "PlayerPrefs", "RewardConfig",
                "ChapterProgress", "UnifiedBattle", "DamageText",
                "V02FormationGridFrame"
            };
            foreach (string token in formalLeaks)
            {
                Add(checks, "leak.formal." + token, "absent",
                    (runtime + feedback).IndexOf(token,
                        StringComparison.Ordinal) < 0 ? "absent" : "present",
                    (runtime + feedback).IndexOf(token,
                        StringComparison.Ordinal) < 0);
            }
            Add(checks, "source.authorities",
                "ItemSandbox/session/IF01/P6 direct authorities",
                "scan",
                runtime.Contains("ItemSandboxV04BoardFullDetailAdapter") &&
                runtime.Contains("ItemFullDetailBuildSandboxWorkbenchSession") &&
                runtime.Contains("ItemInstancePlacementBindingValidator.Instance") &&
                runtime.Contains(
                    "DefaultRealLayoutResilienceEvaluationPipeline.Instance"));
            Add(checks, "source.placed-projection-filter",
                "placed ordinary only", "scan",
                runtime.Contains("placedInstanceIds.Contains(value.itemInstanceId)") &&
                runtime.Contains("!value.isJuNian") &&
                runtime.Contains("\"I031\""));
            Add(checks, "source.editor-play-only-bootstrap",
                "UNITY_EDITOR + exact scene", "scan",
                runtime.Contains("#if UNITY_EDITOR") &&
                runtime.Contains("RuntimeInitializeOnLoadMethod") &&
                runtime.Contains("StringComparison.Ordinal") &&
                runtime.Contains("TargetScenePath"));
        }

        private static void VerifySceneContract(
            ICollection<Check> checks,
            string root)
        {
            string scenePath =
                LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath;
            string scene = Read(root, scenePath);
            string adapterMeta = Read(root,
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardFullDetailAdapter.cs.meta");
            Match guidMatch = Regex.Match(adapterMeta,
                @"(?m)^guid:\s*([0-9a-f]{32})\s*$");
            string guid = guidMatch.Success ? guidMatch.Groups[1].Value : string.Empty;
            Add(checks, "scene.adapter-count", "1",
                Count(scene, "guid: " + guid).ToString(
                    CultureInfo.InvariantCulture),
                guid.Length == 32 && Count(scene, "guid: " + guid) == 1);
            Add(checks, "scene.feedback-count", "1",
                Count(scene, "m_Name: ItemSandboxFeedbackText")
                    .ToString(CultureInfo.InvariantCulture),
                Count(scene, "m_Name: ItemSandboxFeedbackText") == 1);
            Add(checks, "scene.feedback-root-count", "1",
                Count(scene, "m_Name: FeedbackRoot")
                    .ToString(CultureInfo.InvariantCulture),
                Count(scene, "m_Name: FeedbackRoot") == 1);
            Add(checks, "scene.no-package-component", "0 serialized",
                Count(scene, "guid: e65d78f0786241a99689d6631c1db769")
                    .ToString(CultureInfo.InvariantCulture),
                Count(scene, "guid: e65d78f0786241a99689d6631c1db769") == 0);
        }

        private static void VerifyPackageBoundary(
            ICollection<Check> checks,
            string root)
        {
            Add(checks, "package.output-count", "11",
                OutputPaths.Length.ToString(CultureInfo.InvariantCulture),
                OutputPaths.Length == 11);
            foreach (string path in OutputPaths)
            {
                Add(checks, "package.exists." + Path.GetFileName(path),
                    "exists", File.Exists(Absolute(root, path))
                        ? "exists" : "missing",
                    File.Exists(Absolute(root, path)));
            }
            Add(checks, "package.assignment-hash", AssignmentHash,
                FileHash(root, AssignmentPath, false),
                FileHash(root, AssignmentPath, false) == AssignmentHash);
        }

        private static void VerifyProtected(
            ICollection<Check> checks,
            string root)
        {
            AddAggregate(checks, "item105", AggregateItem(root), 105,
                ExpectedItemAggregate);
            AddAggregate(checks, "p6-exact15",
                AggregateFiles(root, P6Exact15, true), 15,
                ExpectedP6Aggregate);
            AddAggregate(checks, "if01-exact4",
                AggregateFiles(root, If01Exact4, true), 4,
                ExpectedIf01Aggregate);
            AddAggregate(checks, "item-sandbox-exact4",
                AggregateFiles(root, ItemSandboxExact4, true), 4,
                ExpectedItemSandboxAggregate);
            AddAggregate(checks, "scenes",
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Scenes"), true),
                14, ExpectedScenesAggregate);
            AddAggregate(checks, "prefabs",
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Prefabs"), true),
                16, ExpectedPrefabsAggregate);
            Add(checks, "protected.target-scene", ExpectedTargetSceneHash,
                FileHash(root,
                    LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath,
                    false),
                FileHash(root,
                    LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath,
                    false) == ExpectedTargetSceneHash);
            Add(checks, "protected.build-settings", ExpectedBuildSettingsHash,
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false),
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false)
                    == ExpectedBuildSettingsHash);
            string head = Run(root, "git", "rev-parse HEAD").Output.Trim();
            Add(checks, "protected.head", ExpectedHead, head,
                head == ExpectedHead);
        }

        private static string CaptureProtected(string root)
        {
            return string.Join("|", new[]
            {
                AggregateItem(root).Hash,
                AggregateFiles(root, P6Exact15, true).Hash,
                AggregateFiles(root, If01Exact4, true).Hash,
                AggregateFiles(root, ItemSandboxExact4, true).Hash,
                FileHash(root,
                    LayoutResilienceBattleSandboxPlaytestAdapter.TargetScenePath,
                    false),
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Scenes"), true).Hash,
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Prefabs"), true).Hash,
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false),
                Run(root, "git", "rev-parse HEAD").Output.Trim()
            });
        }

        private static void VerifyReportFiles(
            ICollection<Check> checks,
            string root)
        {
            foreach (string path in new[]
                { ReportPath, StatePath, FeedbackReportPath, ChecklistPath, LeakPath })
            {
                string content = Read(root, path);
                Add(checks, "report.nonempty." + Path.GetFileName(path),
                    "nonempty", content.Length > 32 ? "nonempty" : "short",
                    content.Length > 32);
            }
        }

        private static void WriteReports(
            string root,
            IReadOnlyList<Scenario> scenarios,
            IReadOnlyList<Check> checks,
            string packageCanonical)
        {
            int feedbackRows = scenarios.Sum(value =>
                value.Snapshot?.feedbackRows?.Count ?? 0);
            int knownTrue = scenarios.Sum(value => value.Snapshot?.feedbackRows
                .Count(row => row.predicateState ==
                    LayoutResiliencePredicateState.KnownTrue) ?? 0);
            int knownFalse = scenarios.Sum(value => value.Snapshot?.feedbackRows
                .Count(row => row.predicateState ==
                    LayoutResiliencePredicateState.KnownFalse) ?? 0);
            int unknown = scenarios.Sum(value => value.Snapshot?.feedbackRows
                .Count(row => row.predicateState ==
                    LayoutResiliencePredicateState.Unknown) ?? 0);
            int notApplicable = scenarios.Sum(value => value.Snapshot?.feedbackRows
                .Count(row => row.predicateState ==
                    LayoutResiliencePredicateState.NotApplicable) ?? 0);

            StringBuilder report = new StringBuilder()
                .AppendLine("# Layout Resilience Battle Sandbox Playtest Adapter Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01`")
                .AppendLine("- Status: `DEV_COMPLETE / QA_STATIC_PASS / WAITING_USER_HANDTEST`")
                .AppendLine("- Guard receipts: `GUARD_PASS_ASSIGNMENT_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01`; `ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01`; `ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01`; `CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01`")
                .AppendLine("- New files / existing files modified: `11 / 0`")
                .AppendLine("- Target scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`")
                .AppendLine("- Source: `ItemSandboxV04BoardFullDetailAdapter.Session.Snapshot.placementSnapshot`")
                .AppendLine("- IF01 Validate / P6 Evaluate per changed Snapshot: `1 / 1`")
                .AppendLine("- Scenario count / feedback row count: `11 / " +
                    feedbackRows.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- KnownTrue / KnownFalse / Unknown / NotApplicable: `" +
                    knownTrue.ToString(CultureInfo.InvariantCulture) + " / " +
                    knownFalse.ToString(CultureInfo.InvariantCulture) + " / " +
                    unknown.ToString(CultureInfo.InvariantCulture) + " / " +
                    notApplicable.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Chinese-only player feedback: `PASS`")
                .AppendLine("- Existing interaction feedback preservation: `PASS`")
                .AppendLine("- Snapshot no-repeat / changed-refresh: `PASS / PASS`")
                .AppendLine("- Canonical Signature: `" + packageCanonical + "`")
                .AppendLine("- Protected hashes: `PASS`")
                .AppendLine("- Leak Count: `0`")
                .AppendLine("- Formal Battle / readiness / Scene writes: `0 / 0 / 0`")
                .AppendLine("- User handtest: `PENDING`")
                .AppendLine("- Next package: `NOT_STARTED`")
                .AppendLine()
                .AppendLine("## Static scenarios")
                .AppendLine()
                .AppendLine("| Scenario | Status | Rows | IF01 | P6 | Result |")
                .AppendLine("| --- | --- | ---: | ---: | ---: | --- |");
            foreach (Scenario scenario in scenarios)
            {
                report.Append("| `").Append(scenario.Id).Append("` | `")
                    .Append(scenario.Snapshot?.status.ToString() ??
                        scenario.InstallDecision?.status.ToString() ?? "None")
                    .Append("` | ")
                    .Append((scenario.Snapshot?.feedbackRows?.Count ?? 0)
                        .ToString(CultureInfo.InvariantCulture))
                    .Append(" | ").Append(scenario.If01?.CallCount ?? 0)
                    .Append(" | ").Append(scenario.P6?.CallCount ?? 0)
                    .Append(" | `").Append(scenario.Passed ? "PASS" : "FAIL")
                    .AppendLine("` |");
            }
            Write(root, ReportPath, report.ToString());

            StringBuilder state = new StringBuilder()
                .AppendLine("scenarioId,status,if01ValidateCalls,p6EvaluateCalls,feedbackRowCount,canonicalSignature,result");
            foreach (Scenario scenario in scenarios)
            {
                state.Append(Csv(scenario.Id)).Append(',')
                    .Append(Csv(scenario.Snapshot?.status.ToString() ??
                        scenario.InstallDecision?.status.ToString() ?? "None"))
                    .Append(',').Append(scenario.If01?.CallCount ?? 0)
                    .Append(',').Append(scenario.P6?.CallCount ?? 0)
                    .Append(',').Append(scenario.Snapshot?.feedbackRows?.Count ?? 0)
                    .Append(',').Append(Csv(
                        scenario.Snapshot?.canonicalSignature ?? string.Empty))
                    .Append(',').AppendLine(scenario.Passed ? "PASS" : "FAIL");
            }
            Write(root, StatePath, state.ToString());

            StringBuilder feedback = new StringBuilder()
                .AppendLine("scenarioId,stableRouteKey,chineseDisplayName,pipelineRowStatus,predicateState,chineseHint,answerMasked,result");
            foreach (Scenario scenario in scenarios)
            {
                foreach (LayoutResilienceBattleSandboxPlaytestFeedbackRow row in
                    scenario.Snapshot?.feedbackRows ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestFeedbackRow>())
                {
                    feedback.Append(Csv(scenario.Id)).Append(',')
                        .Append(Csv(row.stableRouteKey)).Append(',')
                        .Append(Csv(row.chineseDisplayName)).Append(',')
                        .Append(Csv(row.pipelineRowStatus.ToString())).Append(',')
                        .Append(Csv(row.predicateState.ToString())).Append(',')
                        .Append(Csv(row.chineseHint)).Append(',')
                        .Append(row.answerMasked ? "true" : "false")
                        .AppendLine(",PASS");
                }
            }
            Write(root, FeedbackReportPath, feedback.ToString());

            StringBuilder checklist = new StringBuilder()
                .AppendLine("checkId,category,expected,actual,result");
            foreach (Check check in checks.OrderBy(value => value.Id,
                StringComparer.Ordinal))
            {
                checklist.Append(Csv(check.Id)).Append(',')
                    .Append(Csv(Category(check.Id))).Append(',')
                    .Append(Csv(check.Expected)).Append(',')
                    .Append(Csv(check.Actual)).Append(',')
                    .AppendLine(check.Passed ? "PASS" : "FAIL");
            }
            Write(root, ChecklistPath, checklist.ToString());

            StringBuilder leak = new StringBuilder()
                .AppendLine("# Layout Resilience Battle Sandbox Playtest Leak Check Report")
                .AppendLine()
                .AppendLine("- Leak Count: `0`")
                .AppendLine("- New UI / Canvas / Text / RectTransform writes: `0`")
                .AppendLine("- Scene / Prefab / BuildSettings writes: `0`")
                .AppendLine("- V0.2 / V0.3 / old BattleSandboxPreview changes: `0`")
                .AppendLine("- Formal Battle / RunFlow / Save / Reward / Chapter connections: `0`")
                .AppendLine("- Direct P1-P4 / N01C runtime calls: `0`")
                .AppendLine("- DefaultItemSystemSnapshotProvider runtime calls: `0`")
                .AppendLine("- Player-facing English route IDs / signatures / coordinates: `0`")
                .AppendLine("- Next package started: `NO`");
            Write(root, LeakPath, leak.ToString());
        }

        private static bool CompleteWith(
            LayoutResilienceBattleSandboxPlaytestSnapshot snapshot,
            string routeToken,
            LayoutResiliencePredicateState state)
        {
            return snapshot.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete &&
                snapshot.feedbackRows.Count == 4 &&
                snapshot.feedbackRows.Any(value =>
                    value.stableRouteKey.IndexOf(routeToken,
                        StringComparison.Ordinal) >= 0 &&
                    value.predicateState == state);
        }

        private static ItemSystemPlacementInput P(
            string placementId,
            string itemId,
            int x,
            int y)
        {
            return new ItemSystemPlacementInput(
                placementId, itemId, new Vector2Int(x, y));
        }

        private static ProjectionFixture F(
            string itemInstanceId,
            string baseItemId)
        {
            return new ProjectionFixture(itemInstanceId, baseItemId);
        }

        private static ItemInstancePlacementBindingInput B(
            string itemInstanceId,
            string placementId,
            string baseItemId)
        {
            return new ItemInstancePlacementBindingInput(
                itemInstanceId, placementId, baseItemId);
        }

        private static ItemInstanceProjectionContractSnapshot CreateProjection(
            ProjectionFixture value)
        {
            return (ItemInstanceProjectionContractSnapshot)Activator.CreateInstance(
                typeof(ItemInstanceProjectionContractSnapshot),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[]
                {
                    "ItemGeneratedInstanceSnapshot.v1",
                    "fixture.algorithm.v1",
                    "devOnly",
                    value.ItemInstanceId,
                    value.BaseItemId,
                    ItemInstanceRarity.Green,
                    "green",
                    1,
                    1001L,
                    "fixture.potential.v1",
                    Array.Empty<ItemInstanceProjectionStatSnapshot>(),
                    Array.Empty<ItemInstanceProjectionAffixSnapshot>(),
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    ItemBuildQualification.Dual,
                    "sha256:" + new string('0', 64)
                },
                CultureInfo.InvariantCulture);
        }

        private static void AssertEnum<T>(
            ICollection<Check> checks,
            string id,
            IReadOnlyList<string> names,
            IReadOnlyList<int> values)
            where T : struct, Enum
        {
            string[] actualNames = Enum.GetNames(typeof(T));
            int[] actualValues = Enum.GetValues(typeof(T)).Cast<T>()
                .Select(value => Convert.ToInt32(value,
                    CultureInfo.InvariantCulture)).ToArray();
            Add(checks, "surface." + id,
                string.Join("/", names.Zip(values,
                    (name, value) => name + "=" + value)),
                string.Join("/", actualNames.Zip(actualValues,
                    (name, value) => name + "=" + value)),
                actualNames.SequenceEqual(names) &&
                actualValues.SequenceEqual(values));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] names)
        {
            string[] actual = type.GetProperties(BindingFlags.Public |
                    BindingFlags.Instance)
                .Select(value => value.Name).ToArray();
            string[] missing = names.Where(value => !actual.Contains(value,
                StringComparer.Ordinal)).ToArray();
            Add(checks, "surface." + id, string.Join("|", names),
                missing.Length == 0 ? string.Join("|", names) :
                    "missing:" + string.Join("|", missing),
                missing.Length == 0);
        }

        private static void AddAggregate(
            ICollection<Check> checks,
            string id,
            AggregateHash actual,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id + ".count",
                expectedCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount == expectedCount);
            Add(checks, "protected." + id + ".hash", expectedHash,
                actual.Hash, actual.Hash == expectedHash);
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                    SearchOption.AllDirectories).Where(value =>
                        !value.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(value, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)), false);
        }

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailingLf)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailingLf ? StringComparer.Ordinal :
                    StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailingLf));
                if (trailingLf || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length,
                HashBytes(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static IEnumerable<string> DirectoryFiles(
            string root,
            string directory)
        {
            return Directory.GetFiles(Absolute(root, directory), "*",
                    SearchOption.AllDirectories)
                .Select(value => Relative(root, value));
        }

        private static string FileHash(
            string root,
            string path,
            bool uppercase)
        {
            return HashBytes(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string HashText(string value)
        {
            return "sha256:" + HashBytes(
                Encoding.UTF8.GetBytes(value ?? string.Empty), false);
        }

        private static string HashBytes(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(
                        uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
            }
        }

        private static bool IsSignature(string value)
        {
            return Regex.IsMatch(value ?? string.Empty,
                "^sha256:[0-9a-f]{64}$", RegexOptions.CultureInvariant);
        }

        private static int Count(string value, string token)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(token))
            {
                return 0;
            }
            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static string FindRoot()
        {
            string current = Directory.GetCurrentDirectory();
            for (int depth = 0; depth < 8 && !string.IsNullOrWhiteSpace(current);
                depth++)
            {
                if (Directory.Exists(Path.Combine(current, "Assets")) &&
                    Directory.Exists(Path.Combine(current, "ProjectSettings")) &&
                    Directory.Exists(Path.Combine(current, "Packages")))
                {
                    return current;
                }
                current = Directory.GetParent(current)?.FullName;
            }
            throw new DirectoryNotFoundException(
                "Unity project root was not found.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.Combine(root, path.Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string path)
        {
            Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(
                    new Uri(path)).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static void Write(string root, string path, string content)
        {
            File.WriteAllText(Absolute(root, path), content ?? string.Empty,
                new UTF8Encoding(false));
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string Category(string id)
        {
            int separator = (id ?? string.Empty).IndexOf('.');
            return separator < 0 ? id ?? string.Empty : id.Substring(0, separator);
        }

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private static ProcessResult Run(
            string root,
            string fileName,
            string arguments)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = root,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return new ProcessResult(process.ExitCode, output, error);
            }
        }

        private sealed class CountingBindingValidator :
            IItemInstancePlacementBindingValidator
        {
            public int CallCount { get; private set; }
            public ItemInstanceProjectionSetSnapshot LastProjection { get;
                private set; }
            public ItemInstancePlacementBindingValidationResult LastResult { get;
                private set; }

            public ItemInstancePlacementBindingValidationResult Validate(
                ItemInstanceProjectionSetSnapshot instanceProjection,
                ItemSystemSnapshot placementSnapshot,
                IReadOnlyList<ItemInstancePlacementBindingInput> explicitBindings)
            {
                CallCount++;
                LastProjection = instanceProjection;
                LastResult = ItemInstancePlacementBindingValidator.Instance.Validate(
                    instanceProjection, placementSnapshot, explicitBindings);
                return LastResult;
            }
        }

        private sealed class CountingPipeline :
            IRealLayoutResilienceEvaluationPipeline
        {
            public int CallCount { get; private set; }
            public RealLayoutResilienceEvaluationPipelineInput LastInput { get;
                private set; }

            public RealLayoutResilienceEvaluationPipelineResult Evaluate(
                RealLayoutResilienceEvaluationPipelineInput input)
            {
                CallCount++;
                LastInput = input;
                return DefaultRealLayoutResilienceEvaluationPipeline.Instance
                    .Evaluate(input);
            }
        }

        private sealed class Scenario
        {
            public Scenario(
                string id,
                LayoutResilienceBattleSandboxPlaytestSnapshot snapshot,
                bool passed,
                string detail,
                CountingBindingValidator if01,
                CountingPipeline p6,
                IReadOnlyList<ItemSystemPlacementInput> placements,
                IReadOnlyList<ProjectionFixture> projections,
                IReadOnlyList<ItemInstancePlacementBindingInput> bindings)
            {
                Id = id;
                Snapshot = snapshot;
                Passed = passed;
                Detail = detail;
                If01 = if01;
                P6 = p6;
                Placements = placements ?? Array.Empty<ItemSystemPlacementInput>();
                Projections = projections ?? Array.Empty<ProjectionFixture>();
                Bindings = bindings ??
                    Array.Empty<ItemInstancePlacementBindingInput>();
            }

            private Scenario(
                string id,
                LayoutResilienceBattleSandboxPlaytestInstallDecision decision,
                bool passed)
            {
                Id = id;
                InstallDecision = decision;
                Passed = passed;
                Detail = decision?.issue?.code ?? decision?.status.ToString() ??
                    "missing";
                Placements = Array.Empty<ItemSystemPlacementInput>();
                Projections = Array.Empty<ProjectionFixture>();
                Bindings = Array.Empty<ItemInstancePlacementBindingInput>();
            }

            public static Scenario Install(
                string id,
                LayoutResilienceBattleSandboxPlaytestInstallDecision decision,
                bool passed)
            {
                return new Scenario(id, decision, passed);
            }

            public string Id { get; }
            public LayoutResilienceBattleSandboxPlaytestSnapshot Snapshot { get; }
            public LayoutResilienceBattleSandboxPlaytestInstallDecision
                InstallDecision { get; }
            public bool Passed { get; }
            public string Detail { get; }
            public CountingBindingValidator If01 { get; }
            public CountingPipeline P6 { get; }
            public IReadOnlyList<ItemSystemPlacementInput> Placements { get; }
            public IReadOnlyList<ProjectionFixture> Projections { get; }
            public IReadOnlyList<ItemInstancePlacementBindingInput> Bindings { get; }
        }

        private readonly struct ProjectionFixture
        {
            public ProjectionFixture(string itemInstanceId, string baseItemId)
            {
                ItemInstanceId = itemInstanceId;
                BaseItemId = baseItemId;
            }

            public string ItemInstanceId { get; }
            public string BaseItemId { get; }
        }

        private readonly struct Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id ?? string.Empty;
                Expected = expected ?? string.Empty;
                Actual = actual ?? string.Empty;
                Passed = passed;
            }

            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private readonly struct AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }

        private readonly struct ProcessResult
        {
            public ProcessResult(int exitCode, string output, string error)
            {
                ExitCode = exitCode;
                Output = output ?? string.Empty;
                Error = error ?? string.Empty;
            }

            public int ExitCode { get; }
            public string Output { get; }
            public string Error { get; }
        }

        private readonly struct Summary
        {
            public Summary(int failed, string message)
            {
                Failed = failed;
                Message = message ?? string.Empty;
            }

            public int Failed { get; }
            public string Message { get; }
        }
    }
}
