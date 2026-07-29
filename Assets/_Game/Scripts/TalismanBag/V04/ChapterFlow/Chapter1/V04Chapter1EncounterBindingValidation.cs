using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public static class V04Chapter1EncounterBindingValidation
    {
        public const string SchemaId = "V04Chapter1EncounterBindingValidation.v1";

        private const string HeldContentBoundaryId =
            "bone_aspect_enemy_c1_03_bone_swap_remnant";

        private static readonly string[] ExpectedStageIds =
        {
            "1-1", "1-2", "1-3", "1-4", "1-5", "1-6", "1-7", "1-8", "1-9"
        };

        private static readonly string[] ExpectedContentIds =
        {
            V04Chapter1EncounterManifest.ShatteredHostContentId,
            V04Chapter1EncounterManifest.ShatteredHostContentId,
            V04Chapter1EncounterManifest.ShatteredHostContentId,
            V04Chapter1EncounterManifest.PorcelainHoundContentId,
            V04Chapter1EncounterManifest.PorcelainHoundContentId,
            V04Chapter1EncounterManifest.PorcelainHoundContentId,
            V04Chapter1EncounterManifest.PorcelainHoundContentId,
            V04Chapter1EncounterManifest.ShatteredHostContentId,
            V04Chapter1EncounterManifest.PorcelainHoundContentId
        };

        private static readonly string[] ExpectedPurposes =
        {
            "First-zone minimal encounter",
            "Repeat deterministic Runtime coverage",
            "First-zone close",
            "Introduce shell pressure",
            "Shell repeat",
            "Shell repeat",
            "Second-zone close",
            "Temporary dev substitute for held future slot",
            "Boss-precheck dev encounter"
        };

        public static V04Chapter1EncounterBindingValidationResult Validate()
        {
            return Validate(
                V04Chapter1EncounterManifest.Bindings,
                V04Chapter1EncounterManifest.HeldContent,
                V04Chapter1DevBossCompletionPolicy.Current);
        }

        public static V04Chapter1EncounterBindingValidationResult Validate(
            IReadOnlyList<V04Chapter1EncounterBindingDefinition> bindings,
            V04Chapter1HeldContentDefinition held,
            V04Chapter1DevBossCompletionPolicyDefinition policy)
        {
            V04Chapter1EncounterBindingValidationResult result = new();
            IReadOnlyList<V04Chapter1EncounterBindingDefinition> rows =
                bindings ?? Array.Empty<V04Chapter1EncounterBindingDefinition>();

            Require(result, "active-stage-bindings", "manifest", 9, rows.Count);
            Require(
                result,
                "duplicate-stage-bindings",
                "manifest",
                0,
                rows.Where(row => row != null)
                    .GroupBy(row => row.stageId, StringComparer.Ordinal)
                    .Count(group => group.Count() > 1));

            for (int index = 0; index < rows.Count; index++)
            {
                V04Chapter1EncounterBindingDefinition row = rows[index];
                if (row == null)
                {
                    Add(result, "row-null-" + index, "manifest", "non-null", "null");
                    continue;
                }

                string suffix = row.stageId + "-" + index;
                Require(
                    result,
                    "schema-" + suffix,
                    "schema",
                    V04Chapter1EncounterManifest.SchemaId,
                    row.schemaId);
                Require(
                    result,
                    "chapter-" + suffix,
                    "identity",
                    V04Chapter1EncounterManifest.ChapterId,
                    row.chapterId);
                Require(
                    result,
                    "composition-" + suffix,
                    "binding",
                    V04Chapter1EncounterManifest.CompositionMode,
                    row.compositionMode);
                Require(
                    result,
                    "runtime-status-" + suffix,
                    "binding",
                    V04Chapter1EncounterManifest.RuntimeBindingStatus,
                    row.runtimeBindingStatus);
                Require(result, "dev-only-" + suffix, "isolation", true, row.devOnly);
                Require(result, "enabled-" + suffix, "isolation", false, row.isEnabled);
                Require(result, "formal-" + suffix, "isolation", false, row.formalFlow);

                V04ChapterStageDefinition core = V04ChapterFlowManifest.FindStage(row.stageId);
                if (core == null)
                {
                    Add(
                        result,
                        "unknown-stage-" + suffix,
                        "chapter-flow-parity",
                        "known Chapter 1 normal stage",
                        row.stageId);
                }
                else
                {
                    Require(
                        result,
                        "core-chapter-" + suffix,
                        "chapter-flow-parity",
                        V04Chapter1EncounterManifest.ChapterId,
                        core.chapterId);
                    Require(
                        result,
                        "normal-stage-" + suffix,
                        "chapter-flow-parity",
                        false,
                        core.isBossStage);
                    Require(
                        result,
                        "slot-join-" + suffix,
                        "chapter-flow-parity",
                        core.contentBindingSlotId,
                        row.chapterFlowSlotId);
                    Require(
                        result,
                        "encounter-join-" + suffix,
                        "chapter-flow-parity",
                        core.encounterNodeId,
                        row.encounterNodeId);
                }

                if (string.Equals(
                        row.activeEnemyContentId,
                        HeldContentBoundaryId,
                        StringComparison.Ordinal))
                {
                    Add(
                        result,
                        "active-held-content-" + suffix,
                        "decision-boundary",
                        "0 active held-content bindings",
                        row.activeEnemyContentId);
                }

                if (IsLegacyContentId(row.activeEnemyContentId))
                {
                    Add(
                        result,
                        "legacy-content-" + suffix,
                        "leak",
                        "new V0.4 content identity",
                        row.activeEnemyContentId);
                }
            }

            int comparisonCount = Math.Min(rows.Count, ExpectedStageIds.Length);
            for (int index = 0; index < comparisonCount; index++)
            {
                V04Chapter1EncounterBindingDefinition row = rows[index];
                if (row == null)
                {
                    continue;
                }

                Require(
                    result,
                    "mapping-stage-" + index,
                    "frozen-mapping",
                    ExpectedStageIds[index],
                    row.stageId);
                Require(
                    result,
                    "mapping-slot-" + index,
                    "frozen-mapping",
                    "bline.content_binding_slot.c1.s" + (index + 1),
                    row.chapterFlowSlotId);
                Require(
                    result,
                    "mapping-content-" + index,
                    "frozen-mapping",
                    ExpectedContentIds[index],
                    row.activeEnemyContentId);
                Require(
                    result,
                    "mapping-purpose-" + index,
                    "frozen-mapping",
                    ExpectedPurposes[index],
                    row.bindingPurpose);
            }

            Require(
                result,
                "shattered-host-bindings",
                "counts",
                4,
                rows.Count(row => row != null && string.Equals(
                    row.activeEnemyContentId,
                    V04Chapter1EncounterManifest.ShatteredHostContentId,
                    StringComparison.Ordinal)));
            Require(
                result,
                "porcelain-hound-bindings",
                "counts",
                5,
                rows.Count(row => row != null && string.Equals(
                    row.activeEnemyContentId,
                    V04Chapter1EncounterManifest.PorcelainHoundContentId,
                    StringComparison.Ordinal)));
            Require(
                result,
                "active-bone-swap-bindings",
                "decision-boundary",
                0,
                rows.Count(row => row != null && string.Equals(
                    row.activeEnemyContentId,
                    HeldContentBoundaryId,
                    StringComparison.Ordinal)));

            ValidateHeld(result, held);
            ValidatePolicy(result, policy);
            ValidateCoreParity(result);
            return result;
        }

        private static void ValidateHeld(
            V04Chapter1EncounterBindingValidationResult result,
            V04Chapter1HeldContentDefinition held)
        {
            if (held == null)
            {
                Add(result, "held-content-row", "decision-boundary", "one held row", "missing");
                return;
            }

            Require(
                result,
                "held-content-id",
                "decision-boundary",
                HeldContentBoundaryId,
                held.heldContentId);
            Require(
                result,
                "held-display",
                "decision-boundary",
                "换骨残相",
                held.heldDisplayIdentity);
            Require(
                result,
                "held-preferred-slot",
                "decision-boundary",
                "bline.content_binding_slot.c1.s8",
                held.preferredChapterFlowSlotId);
            Require(
                result,
                "held-slot",
                "decision-boundary",
                "bline.c1.future_content_slot.bone_swap_remnant",
                held.heldSlotId);
            Require(result, "held-status", "decision-boundary", "HELD_BY_BA-D3", held.status);
            Require(result, "held-dependency", "decision-boundary", "BA-D3", held.decisionDependency);
            Require(result, "held-runtime-active", "decision-boundary", false, held.activeRuntimeBinding);
            Require(result, "held-stage-active", "decision-boundary", false, held.activeStageBinding);
            Require(result, "held-copy-scope", "decision-boundary", false, held.copyScopeAuthored);
            Require(result, "held-dev-only", "isolation", true, held.devOnly);
            Require(result, "held-enabled", "isolation", false, held.isEnabled);
            Require(result, "held-formal", "isolation", false, held.formalFlow);
        }

        private static void ValidatePolicy(
            V04Chapter1EncounterBindingValidationResult result,
            V04Chapter1DevBossCompletionPolicyDefinition policy)
        {
            if (policy == null)
            {
                Add(result, "phase1-policy", "completion-policy", "one policy", "missing");
                return;
            }

            Require(
                result,
                "policy-schema",
                "completion-policy",
                V04Chapter1DevBossCompletionPolicy.SchemaId,
                policy.schemaId);
            Require(
                result,
                "policy-chapter",
                "completion-policy",
                "bone_aspect_chapter_1",
                policy.chapterId);
            Require(result, "policy-stage", "completion-policy", "1-10", policy.stageId);
            Require(
                result,
                "policy-boss",
                "completion-policy",
                "bone_aspect_boss_c1_bone_guard",
                policy.bossProfileId);
            Require(
                result,
                "policy-truth-owner",
                "completion-policy",
                V04ChapterFlowManifest.SchemaId,
                policy.chapterFlowTruthOwnerId);
            Require(
                result,
                "policy-resolver-slot",
                "completion-policy",
                "bline.c1.boss_completion_resolver",
                policy.completionResolverSlotId);
            Require(
                result,
                "policy-source",
                "completion-policy",
                "ShougunuPhase1RuntimeAndActionContract.v1",
                policy.currentSourceContractId);
            Require(
                result,
                "policy-terminal-fact",
                "completion-policy",
                "ShougunuPhase1LifecycleState.Defeated",
                policy.currentTerminalFactId);
            Require(
                result,
                "policy-temp-clear",
                "completion-policy",
                true,
                policy.temporaryClearAllowed);
            Require(
                result,
                "policy-truth-unchanged",
                "completion-policy",
                true,
                policy.chapterFlowTruthMustRemainUnchanged);
            Require(
                result,
                "policy-replacement",
                "completion-policy",
                "REPLACE_COMPLETION_RESOLVER_ONLY",
                policy.replacementMode);
            Require(
                result,
                "policy-label-a",
                "completion-policy",
                "PHASE1_DEV_VERTICAL_SLICE",
                policy.labelA);
            Require(
                result,
                "policy-label-b",
                "completion-policy",
                "NOT_CONTENT_FINAL",
                policy.labelB);
            Require(
                result,
                "policy-label-c",
                "completion-policy",
                "NOT_FORMAL_BOSS_COMPLETION",
                policy.labelC);
            Require(
                result,
                "policy-owns-truth",
                "completion-policy",
                false,
                policy.ownsChapterFlowTruth);
            Require(
                result,
                "policy-direct-runtime",
                "completion-policy",
                false,
                policy.directRuntimeInvocation);
            Require(
                result,
                "policy-direct-battle",
                "completion-policy",
                false,
                policy.directBattleInvocation);
            Require(result, "policy-dev-only", "isolation", true, policy.devOnly);
            Require(result, "policy-enabled", "isolation", false, policy.isEnabled);
            Require(result, "policy-formal", "isolation", false, policy.formalFlow);
        }

        private static void ValidateCoreParity(
            V04Chapter1EncounterBindingValidationResult result)
        {
            IReadOnlyList<V04ChapterStageDefinition> chapterStages =
                V04ChapterFlowManifest.Stages
                    .Where(row => string.Equals(
                        row.chapterId,
                        V04Chapter1EncounterManifest.ChapterId,
                        StringComparison.Ordinal))
                    .OrderBy(row => row.stageIndex)
                    .ToList();
            Require(
                result,
                "core-schema",
                "chapter-flow-parity",
                "V04ChapterFlowManifest.v1",
                V04ChapterFlowManifest.SchemaId);
            Require(
                result,
                "chapter1-stage-count",
                "chapter-flow-parity",
                10,
                chapterStages.Count);
            Require(
                result,
                "chapter1-normal-slot-count",
                "chapter-flow-parity",
                9,
                chapterStages.Count(row =>
                    !row.isBossStage
                    && !string.IsNullOrWhiteSpace(row.contentBindingSlotId)));

            V04ChapterStageDefinition stageNine = V04ChapterFlowManifest.FindStage("1-9");
            V04ChapterStageDefinition stageTen = V04ChapterFlowManifest.FindStage("1-10");
            Require(
                result,
                "stage-1-9-exists",
                "chapter-flow-parity",
                true,
                stageNine != null);
            Require(
                result,
                "stage-1-10-exists",
                "chapter-flow-parity",
                true,
                stageTen != null);
            if (stageNine != null)
            {
                Require(
                    result,
                    "stage-1-9-stop",
                    "chapter-flow-parity",
                    true,
                    stageNine.stopBeforeBossAfterWin);
                Require(
                    result,
                    "stage-1-9-boss",
                    "chapter-flow-parity",
                    false,
                    stageNine.isBossStage);
            }

            if (stageTen != null)
            {
                Require(
                    result,
                    "stage-1-10-boss",
                    "chapter-flow-parity",
                    true,
                    stageTen.isBossStage);
                Require(
                    result,
                    "stage-1-10-manual",
                    "chapter-flow-parity",
                    true,
                    stageTen.requiresManualBossChallenge);
                Require(
                    result,
                    "stage-1-10-profile",
                    "chapter-flow-parity",
                    "bone_aspect_boss_c1_bone_guard",
                    stageTen.bossProfileId);
                Require(
                    result,
                    "stage-1-10-slot",
                    "chapter-flow-parity",
                    string.Empty,
                    stageTen.contentBindingSlotId);
            }

            Require(
                result,
                "automatic-boss-start-impossible",
                "chapter-flow-parity",
                true,
                stageTen != null
                    && stageTen.isBossStage
                    && stageTen.requiresManualBossChallenge
                    && string.IsNullOrWhiteSpace(stageTen.contentBindingSlotId));
        }

        private static bool IsLegacyContentId(string value)
        {
            string normalized = value ?? string.Empty;
            return normalized.IndexOf("v02", StringComparison.OrdinalIgnoreCase) >= 0
                || normalized.IndexOf("legacy", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void Require<T>(
            V04Chapter1EncounterBindingValidationResult result,
            string assertionId,
            string category,
            T expected,
            T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                Add(
                    result,
                    assertionId,
                    category,
                    Convert.ToString(expected),
                    Convert.ToString(actual));
            }
        }

        private static void Add(
            V04Chapter1EncounterBindingValidationResult result,
            string assertionId,
            string category,
            string expected,
            string actual)
        {
            result.Add(
                assertionId,
                category,
                expected,
                actual,
                "Expected and actual values differ.");
        }
    }
}
