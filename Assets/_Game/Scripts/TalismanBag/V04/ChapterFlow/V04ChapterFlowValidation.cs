using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.V04.ChapterFlow
{
    public static class V04ChapterFlowValidation
    {
        public const string SchemaId = "V04ChapterFlowValidation.v1";

        private static readonly string[] ExactChapterIds =
        {
            "bone_aspect_chapter_1",
            "bone_aspect_chapter_2",
            "bone_aspect_chapter_3",
            "bone_aspect_chapter_4"
        };

        private static readonly string[] ExactBossProfileIds =
        {
            "bone_aspect_boss_c1_bone_guard",
            "bone_aspect_boss_c2_identity_bidder",
            "bone_aspect_boss_c3_array_eye_guardian",
            "bone_aspect_boss_c4_myriad_bone_beast"
        };

        public static V04ChapterFlowValidationResult ValidateManifest()
        {
            V04ChapterFlowValidationResult result = new();
            IReadOnlyList<V04ChapterDefinition> chapters = V04ChapterFlowManifest.Chapters;
            IReadOnlyList<V04ChapterStageDefinition> stages = V04ChapterFlowManifest.Stages;

            Require(result, "chapters", "manifest", 4, chapters.Count);
            Require(result, "stages", "manifest", 40, stages.Count);
            Require(
                result,
                "encounter-nodes",
                "manifest",
                12,
                stages.Select(row => row.encounterNodeId).Distinct(StringComparer.Ordinal).Count());

            for (int index = 0; index < ExactChapterIds.Length; index++)
            {
                V04ChapterDefinition chapter = chapters.ElementAtOrDefault(index);
                if (chapter == null)
                {
                    result.Add(
                        "chapter-" + (index + 1),
                        "identity",
                        ExactChapterIds[index],
                        "missing",
                        "Chapter row is missing.");
                    continue;
                }

                Require(
                    result,
                    "chapter-id-" + (index + 1),
                    "identity",
                    ExactChapterIds[index],
                    chapter.chapterId);
                Require(
                    result,
                    "boss-profile-" + (index + 1),
                    "identity",
                    ExactBossProfileIds[index],
                    chapter.bossProfileId);
                Require(result, "chapter-dev-only-" + (index + 1), "isolation", true, chapter.devOnly);
                Require(result, "chapter-enabled-" + (index + 1), "isolation", false, chapter.isEnabled);
                Require(result, "chapter-formal-" + (index + 1), "isolation", false, chapter.formalFlow);
            }

            HashSet<string> stageIds = new(StringComparer.Ordinal);
            HashSet<string> hookIds = new(StringComparer.Ordinal);
            foreach (V04ChapterStageDefinition stage in stages)
            {
                if (!stageIds.Add(stage.stageId))
                {
                    result.Add(
                        "duplicate-stage-" + stage.stageId,
                        "identity",
                        "unique",
                        stage.stageId,
                        "Duplicate stage ID.");
                }

                string expectedStageId = stage.chapterIndex + "-" + stage.stageIndex;
                Require(result, "stage-id-" + stage.stageId, "identity", expectedStageId, stage.stageId);
                Require(
                    result,
                    "boss-stage-" + stage.stageId,
                    "boss",
                    stage.stageIndex == 10,
                    stage.isBossStage);
                Require(
                    result,
                    "boss-stop-" + stage.stageId,
                    "boss",
                    stage.stageIndex == 9,
                    stage.stopBeforeBossAfterWin);
                Require(
                    result,
                    "manual-boss-" + stage.stageId,
                    "boss",
                    stage.stageIndex == 10,
                    stage.requiresManualBossChallenge);
                Require(result, "stage-dev-only-" + stage.stageId, "isolation", true, stage.devOnly);
                Require(result, "stage-enabled-" + stage.stageId, "isolation", false, stage.isEnabled);
                Require(result, "stage-formal-" + stage.stageId, "isolation", false, stage.formalFlow);

                if (stage.isBossStage)
                {
                    Require(
                        result,
                        "boss-profile-stage-" + stage.stageId,
                        "boss",
                        ExactBossProfileIds[stage.chapterIndex - 1],
                        stage.bossProfileId);
                    Require(
                        result,
                        "boss-content-slot-" + stage.stageId,
                        "binding",
                        string.Empty,
                        stage.contentBindingSlotId);
                    Require(
                        result,
                        "boss-resolution-" + stage.stageId,
                        "boss",
                        stage.chapterIndex == 3
                            ? V04BossResolutionMode.YieldAndAllowPassage
                            : V04BossResolutionMode.RouteCompletion,
                        stage.bossResolutionMode);
                }
                else
                {
                    Require(
                        result,
                        "normal-boss-profile-" + stage.stageId,
                        "binding",
                        string.Empty,
                        stage.bossProfileId);
                    Require(
                        result,
                        "normal-content-slot-" + stage.stageId,
                        "binding",
                        true,
                        IsStableId(stage.contentBindingSlotId));
                    Require(
                        result,
                        "normal-resolution-" + stage.stageId,
                        "boss",
                        V04BossResolutionMode.None,
                        stage.bossResolutionMode);
                }

                ValidateHook(result, stage, "before-stage-tutorial", stage.beforeStageTutorialHookId, hookIds);
                ValidateHook(result, stage, "before-stage-story", stage.beforeStageStoryHookId, hookIds);
                ValidateHook(result, stage, "after-result-tutorial", stage.afterResultTutorialHookId, hookIds);
                ValidateHook(result, stage, "after-result-story", stage.afterResultStoryHookId, hookIds);

                ValidateApplicableHook(
                    result,
                    stage,
                    "before-chapter-tutorial",
                    stage.stageIndex == 1,
                    stage.beforeChapterTutorialHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-chapter-story",
                    stage.stageIndex == 1,
                    stage.beforeChapterStoryHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-boss-gate-tutorial",
                    stage.stageIndex == 9,
                    stage.beforeBossTutorialHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-boss-gate-story",
                    stage.stageIndex == 9,
                    stage.beforeBossStoryHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-boss-challenge-tutorial",
                    stage.isBossStage,
                    stage.beforeBossChallengeTutorialHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-boss-challenge-story",
                    stage.isBossStage,
                    stage.beforeBossChallengeStoryHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "after-boss-result-tutorial",
                    stage.isBossStage,
                    stage.afterBossResultTutorialHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "after-boss-result-story",
                    stage.isBossStage,
                    stage.afterBossResultStoryHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-next-chapter-tutorial",
                    stage.isBossStage,
                    stage.beforeNextChapterUnlockTutorialHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "before-next-chapter-story",
                    stage.isBossStage,
                    stage.beforeNextChapterUnlockStoryHookId,
                    hookIds);
                ValidateApplicableHook(
                    result,
                    stage,
                    "drop-candidate",
                    !stage.isBossStage,
                    stage.dropCandidateHookId,
                    hookIds);
            }

            return result;
        }

        public static V04ChapterFlowValidationResult ValidateState(V04ChapterFlowStateSnapshot state)
        {
            V04ChapterFlowValidationResult result = new();
            if (state == null)
            {
                result.Add("state-null", "state", "non-null", "null", "State is required.");
                return result;
            }

            Require(result, "state-schema", "state", V04ChapterFlowStateSnapshot.SchemaId, state.schemaId);
            Require(result, "state-dev-only", "isolation", true, state.devOnly);
            Require(result, "state-enabled", "isolation", false, state.isEnabled);
            Require(result, "state-formal", "isolation", false, state.formalFlow);

            int duplicateUnlocks = (state.unlockedChapterIds ?? new List<string>())
                .GroupBy(value => value, StringComparer.Ordinal)
                .Count(group => group.Count() > 1);
            int duplicateCompletions = (state.completedStageIds ?? new List<string>())
                .GroupBy(value => value, StringComparer.Ordinal)
                .Count(group => group.Count() > 1);
            Require(result, "state-duplicate-unlocks", "state", 0, duplicateUnlocks);
            Require(result, "state-duplicate-completions", "state", 0, duplicateCompletions);
            return result;
        }

        private static void ValidateApplicableHook(
            V04ChapterFlowValidationResult result,
            V04ChapterStageDefinition stage,
            string field,
            bool applicable,
            string value,
            ISet<string> seen)
        {
            if (applicable)
            {
                ValidateHook(result, stage, field, value, seen);
            }
            else
            {
                Require(result, field + "-" + stage.stageId, "hook", string.Empty, value);
            }
        }

        private static void ValidateHook(
            V04ChapterFlowValidationResult result,
            V04ChapterStageDefinition stage,
            string field,
            string value,
            ISet<string> seen)
        {
            if (!IsStableId(value))
            {
                result.Add(
                    field + "-" + stage.stageId,
                    "hook",
                    "stable non-empty ASCII ID",
                    value,
                    "Hook ID is missing or unstable.");
                return;
            }

            if (!seen.Add(value))
            {
                result.Add(
                    "duplicate-hook-" + value,
                    "hook",
                    "unique",
                    value,
                    "Hook ID must be unique.");
            }
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (char character in value)
            {
                bool allowed = character >= 'a' && character <= 'z'
                    || character >= '0' && character <= '9'
                    || character == '.'
                    || character == '_'
                    || character == '-';
                if (!allowed)
                {
                    return false;
                }
            }

            return true;
        }

        private static void Require<T>(
            V04ChapterFlowValidationResult result,
            string assertionId,
            string category,
            T expected,
            T actual)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                result.Add(
                    assertionId,
                    category,
                    Convert.ToString(expected),
                    Convert.ToString(actual),
                    "Expected and actual values differ.");
            }
        }
    }
}
