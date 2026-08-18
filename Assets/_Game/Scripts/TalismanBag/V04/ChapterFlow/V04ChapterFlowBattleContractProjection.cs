using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.ChapterFlow
{
    public sealed class V04BattleResultInterpretation
    {
        public bool accepted;
        public V04ChapterFlowBattleOutcome outcome;
        public string diagnosticCode = string.Empty;
        public List<string> diagnostics = new();
        public List<string> ignoredAuthorityFields = new();
        public string resultFingerprint = string.Empty;
    }

    public static class V04ChapterFlowBattleContractProjection
    {
        public const string SchemaId = "V04ChapterFlowBattleContractProjection.v1";
        public const string SourceControllerId = "V04ChapterFlowBattleContractProjection";

        public static bool TryCreateRequest(
            V04ChapterStageDefinition stage,
            string requestId,
            string callerEnemyProfileId,
            string callerSeedId,
            out BattleStartRequest request,
            out string diagnosticCode)
        {
            request = null;
            diagnosticCode = string.Empty;
            string stableRequestId = Normalize(requestId);
            if (stage == null)
            {
                diagnosticCode = V04ChapterFlowDiagnostics.UnknownStage;
                return false;
            }

            if (string.IsNullOrWhiteSpace(stableRequestId))
            {
                diagnosticCode = V04ChapterFlowDiagnostics.RequestMismatch;
                return false;
            }

            request = new BattleStartRequest
            {
                requestId = stableRequestId,
                chapterId = stage.chapterId,
                stageId = stage.stageId,
                roundId = stage.stageId,
                isBossStage = stage.isBossStage,
                enemyProfileId = Normalize(callerEnemyProfileId),
                bossProfileId = stage.isBossStage ? stage.bossProfileId : string.Empty,
                entrySource = BattleEntrySource.V04BuildSandbox,
                allowedItemRosterId = string.Empty,
                formalFlow = false,
                devOnly = true,
                sourceRoute = V04ChapterFlowManifest.SourceRouteId,
                sourceScene = string.Empty,
                sourceController = SourceControllerId,
                seedId = Normalize(callerSeedId)
            };
            return true;
        }

        public static V04BattleResultInterpretation InterpretResult(
            V04ChapterFlowStateSnapshot state,
            V04ChapterStageDefinition stage,
            string suppliedStageId,
            BattleResultSnapshot result)
        {
            V04BattleResultInterpretation interpretation = new()
            {
                resultFingerprint = ComputeResultFingerprint(result)
            };

            if (state == null || stage == null || result == null)
            {
                return Reject(interpretation, "BLINE_RESULT_NULL_INPUT");
            }

            string resultId = Normalize(result.resultId);
            if (string.IsNullOrWhiteSpace(resultId))
            {
                return Reject(interpretation, "BLINE_RESULT_ID_MISSING");
            }

            if (!string.Equals(Normalize(suppliedStageId), state.currentStageId, StringComparison.Ordinal)
                || !string.Equals(stage.stageId, state.currentStageId, StringComparison.Ordinal))
            {
                return Reject(interpretation, V04ChapterFlowDiagnostics.StageMismatch);
            }

            if (!string.Equals(Normalize(result.requestId), state.activeBattleRequestId, StringComparison.Ordinal))
            {
                return Reject(interpretation, V04ChapterFlowDiagnostics.RequestMismatch);
            }

            if (!string.Equals(Normalize(result.roundId), stage.stageId, StringComparison.Ordinal))
            {
                return Reject(interpretation, V04ChapterFlowDiagnostics.RoundMismatch);
            }

            if (result.shouldWriteSave)
            {
                return Reject(interpretation, "BLINE_RESULT_PERSISTENCE_WRITE_REJECTED");
            }

            if (result.shouldGrantReward)
            {
                return Reject(interpretation, "BLINE_RESULT_GRANT_REJECTED");
            }

            int outcomeFlagCount = (result.win ? 1 : 0)
                + (result.lose ? 1 : 0)
                + (result.abandon ? 1 : 0);
            if (outcomeFlagCount != 1)
            {
                return Reject(interpretation, "BLINE_RESULT_CONTRADICTORY_FLAGS");
            }

            V04ChapterFlowBattleOutcome outcome = ResolveOutcome(result);
            if (outcome == V04ChapterFlowBattleOutcome.Unknown)
            {
                return Reject(interpretation, "BLINE_RESULT_TYPE_FLAG_MISMATCH");
            }

            if (stage.isBossStage)
            {
                if (outcome == V04ChapterFlowBattleOutcome.Win
                    && stage.bossResolutionMode != V04BossResolutionMode.YieldAndAllowPassage
                    && !result.bossDefeated)
                {
                    return Reject(interpretation, "BLINE_RESULT_BOSS_COMPLETION_FACT_MISSING");
                }

                if (outcome != V04ChapterFlowBattleOutcome.Win && result.bossDefeated)
                {
                    return Reject(interpretation, "BLINE_RESULT_BOSS_FACT_CONTRADICTION");
                }
            }
            else if (result.bossDefeated)
            {
                return Reject(interpretation, "BLINE_RESULT_NORMAL_STAGE_BOSS_FACT_REJECTED");
            }

            AddIgnoredAuthorityDiagnostics(interpretation, result);
            interpretation.accepted = true;
            interpretation.outcome = outcome;
            interpretation.diagnosticCode = V04ChapterFlowDiagnostics.Accepted;
            return interpretation;
        }

        public static string ComputeResultFingerprint(BattleResultSnapshot result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            string payload = string.Join(
                "\n",
                new[]
                {
                    "resultId=" + Escape(result.resultId),
                    "requestId=" + Escape(result.requestId),
                    "resultType=" + result.resultType,
                    "win=" + result.win,
                    "lose=" + result.lose,
                    "abandon=" + result.abandon,
                    "roundId=" + Escape(result.roundId),
                    "bossDefeated=" + result.bossDefeated,
                    "durationSeconds=" + result.durationSeconds.ToString("R", CultureInfo.InvariantCulture),
                    "chapterProgressDelta=" + Escape(result.chapterProgressDelta),
                    "rewardPreview=" + JoinValues(result.rewardPreview),
                    "itemDrops=" + JoinValues(result.itemDrops),
                    "buildPerformanceSummary=" + Escape(result.buildPerformanceSummary),
                    "eventSummary=" + JoinValues(result.eventSummary),
                    "rewardClaimToken=" + Escape(result.rewardClaimToken),
                    "nextRouteHint=" + Escape(result.nextRouteHint),
                    "devOnly=" + result.devOnly,
                    "shouldWriteSave=" + result.shouldWriteSave,
                    "shouldGrantReward=" + result.shouldGrantReward
                }) + "\n";

            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static V04ChapterFlowBattleOutcome ResolveOutcome(BattleResultSnapshot result)
        {
            if (result.win && result.resultType == BattleResultType.Win)
            {
                return V04ChapterFlowBattleOutcome.Win;
            }

            if (result.lose && result.resultType == BattleResultType.Lose)
            {
                return V04ChapterFlowBattleOutcome.Lose;
            }

            if (result.abandon && result.resultType == BattleResultType.Abandon)
            {
                return V04ChapterFlowBattleOutcome.Abandon;
            }

            return V04ChapterFlowBattleOutcome.Unknown;
        }

        private static void AddIgnoredAuthorityDiagnostics(
            V04BattleResultInterpretation interpretation,
            BattleResultSnapshot result)
        {
            if ((result.rewardPreview ?? new List<string>()).Any())
            {
                interpretation.ignoredAuthorityFields.Add("rewardPreview");
            }

            if ((result.itemDrops ?? new List<string>()).Any())
            {
                interpretation.ignoredAuthorityFields.Add("itemDrops");
            }

            if (!string.IsNullOrWhiteSpace(result.rewardClaimToken))
            {
                interpretation.ignoredAuthorityFields.Add("rewardClaimToken");
            }

            if (!string.IsNullOrWhiteSpace(result.chapterProgressDelta))
            {
                interpretation.ignoredAuthorityFields.Add("chapterProgressDelta");
            }

            foreach (string field in interpretation.ignoredAuthorityFields)
            {
                interpretation.diagnostics.Add("ignored_non_authoritative_field=" + field);
            }
        }

        private static V04BattleResultInterpretation Reject(
            V04BattleResultInterpretation interpretation,
            string diagnosticCode)
        {
            interpretation.accepted = false;
            interpretation.outcome = V04ChapterFlowBattleOutcome.Unknown;
            interpretation.diagnosticCode = diagnosticCode;
            interpretation.diagnostics.Add(diagnosticCode);
            return interpretation;
        }

        private static string JoinValues(IEnumerable<string> values)
        {
            return string.Join(
                "\u001f",
                (values ?? Array.Empty<string>()).Select(Escape));
        }

        private static string Escape(string value)
        {
            return Normalize(value)
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\u001f", "\\u001f");
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
