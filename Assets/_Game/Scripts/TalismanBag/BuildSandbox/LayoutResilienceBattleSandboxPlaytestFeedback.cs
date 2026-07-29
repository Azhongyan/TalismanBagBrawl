using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items.Capability;

namespace TalismanBag.BuildSandbox
{
    public static class LayoutResilienceBattleSandboxPlaytestSchema
    {
        public const string SchemaId =
            "LayoutResilienceBattleSandboxPlaytestSnapshot.v1";
        public const int SchemaVersion = 1;
    }

    public enum LayoutResilienceBattleSandboxPlaytestStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class LayoutResilienceBattleSandboxPlaytestIssue
    {
        public LayoutResilienceBattleSandboxPlaytestIssue(
            string code,
            LayoutResilienceBattleSandboxPlaytestStatus status,
            string path,
            string message)
        {
            this.code = code ?? string.Empty;
            this.status = status;
            this.path = path ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public LayoutResilienceBattleSandboxPlaytestStatus status { get; }
        public string path { get; }
        public string message { get; }
    }

    public sealed class LayoutResilienceBattleSandboxPlaytestFeedbackRow
    {
        public LayoutResilienceBattleSandboxPlaytestFeedbackRow(
            string stableRouteKey,
            string chineseDisplayName,
            RealLayoutResilienceEvaluationPipelineStatus pipelineRowStatus,
            LayoutResiliencePredicateState predicateState,
            string chineseHint)
        {
            this.stableRouteKey = stableRouteKey ?? string.Empty;
            this.chineseDisplayName = chineseDisplayName ?? string.Empty;
            this.pipelineRowStatus = pipelineRowStatus;
            this.predicateState = predicateState;
            this.chineseHint = chineseHint ?? string.Empty;
            answerMasked = true;
        }

        public string stableRouteKey { get; }
        public string chineseDisplayName { get; }
        public RealLayoutResilienceEvaluationPipelineStatus pipelineRowStatus { get; }
        public LayoutResiliencePredicateState predicateState { get; }
        public string chineseHint { get; }
        public bool answerMasked { get; }
    }

    public sealed class LayoutResilienceBattleSandboxPlaytestSnapshot
    {
        private readonly ReadOnlyCollection<
            LayoutResilienceBattleSandboxPlaytestFeedbackRow> feedbackRowsValue;
        private readonly ReadOnlyCollection<
            LayoutResilienceBattleSandboxPlaytestIssue> issuesValue;

        internal LayoutResilienceBattleSandboxPlaytestSnapshot(
            LayoutResilienceBattleSandboxPlaytestStatus status,
            string itemSnapshotSignature,
            string bindingCanonicalSignature,
            string pipelineCanonicalSignature,
            IEnumerable<LayoutResilienceBattleSandboxPlaytestFeedbackRow> feedbackRows,
            IEnumerable<LayoutResilienceBattleSandboxPlaytestIssue> issues)
        {
            schemaId = LayoutResilienceBattleSandboxPlaytestSchema.SchemaId;
            schemaVersion = LayoutResilienceBattleSandboxPlaytestSchema.SchemaVersion;
            this.status = status;
            devOnly = true;
            isEnabled = false;
            this.itemSnapshotSignature = itemSnapshotSignature ?? string.Empty;
            this.bindingCanonicalSignature = bindingCanonicalSignature ?? string.Empty;
            this.pipelineCanonicalSignature = pipelineCanonicalSignature ?? string.Empty;
            feedbackRowsValue = Array.AsReadOnly((feedbackRows ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestFeedbackRow>())
                .Where(value => value != null)
                .OrderBy(value => value.stableRouteKey, StringComparer.Ordinal)
                .ToArray());
            issuesValue = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.path, StringComparer.Ordinal)
                .ThenBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal)
                .ToArray());
            aggregateFeedbackText =
                LayoutResilienceBattleSandboxPlaytestFeedback.BuildAggregateText(
                    status, feedbackRowsValue);
            canonicalSignature =
                LayoutResilienceBattleSandboxPlaytestCanonical.Create(this);
        }

        public string schemaId { get; }
        public int schemaVersion { get; }
        public LayoutResilienceBattleSandboxPlaytestStatus status { get; }
        public bool devOnly { get; }
        public bool isEnabled { get; }
        public string itemSnapshotSignature { get; }
        public string bindingCanonicalSignature { get; }
        public string pipelineCanonicalSignature { get; }
        public IReadOnlyList<LayoutResilienceBattleSandboxPlaytestFeedbackRow>
            feedbackRows => feedbackRowsValue;
        public IReadOnlyList<LayoutResilienceBattleSandboxPlaytestIssue> issues =>
            issuesValue;
        public string aggregateFeedbackText { get; }
        public string canonicalSignature { get; }

        internal LayoutResilienceBattleSandboxPlaytestSnapshot WithIssue(
            LayoutResilienceBattleSandboxPlaytestIssue issue)
        {
            if (issue == null)
            {
                return this;
            }
            return new LayoutResilienceBattleSandboxPlaytestSnapshot(
                LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                itemSnapshotSignature,
                bindingCanonicalSignature,
                pipelineCanonicalSignature,
                feedbackRowsValue,
                issuesValue.Concat(new[] { issue }));
        }
    }

    public static class LayoutResilienceBattleSandboxPlaytestFeedback
    {
        public const string Prefix = "【阵势韧性】";

        private const string FormationFurnaceRoute =
            "route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1";
        private const string FormationThunderRoute =
            "route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1";
        private const string PollutionBluestoneRoute =
            "route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1";
        private const string PollutionFurnaceRoute =
            "route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1";

        internal static LayoutResilienceBattleSandboxPlaytestSnapshot FromAuthorities(
            string itemSnapshotSignature,
            ItemInstancePlacementBindingValidationResult binding,
            RealLayoutResilienceEvaluationPipelineResult pipeline,
            IEnumerable<LayoutResilienceBattleSandboxPlaytestIssue> localIssues)
        {
            List<LayoutResilienceBattleSandboxPlaytestIssue> issues =
                new List<LayoutResilienceBattleSandboxPlaytestIssue>(
                    localIssues ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestIssue>());

            if (binding == null)
            {
                issues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                    "IF01_RESULT_MISSING",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "IF01",
                    "IF01 validation did not return a result."));
            }
            else
            {
                foreach (ItemInstancePlacementBindingValidationError error in
                    binding.ValidationErrors ??
                    Array.Empty<ItemInstancePlacementBindingValidationError>())
                {
                    issues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                        error.code,
                        error.status == ItemInstancePlacementBindingStatus.Invalid
                            ? LayoutResilienceBattleSandboxPlaytestStatus.Invalid
                            : LayoutResilienceBattleSandboxPlaytestStatus.Unknown,
                        "IF01/" + error.placementId,
                        error.message));
                }
            }

            List<LayoutResilienceBattleSandboxPlaytestFeedbackRow> rows =
                new List<LayoutResilienceBattleSandboxPlaytestFeedbackRow>();
            if (pipeline == null)
            {
                issues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                    "P6_RESULT_MISSING",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "P6",
                    "P6 evaluation did not return a result."));
            }
            else
            {
                foreach (RealLayoutResilienceEvaluationPipelineIssue issue in
                    pipeline.Issues ??
                    Array.Empty<RealLayoutResilienceEvaluationPipelineIssue>())
                {
                    issues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                        issue.Code,
                        MapStatus(issue.Status),
                        "P6/" + issue.Path,
                        issue.Message));
                }

                foreach (RealLayoutResilienceEvaluationRouteRowSnapshot row in
                    pipeline.Rows ??
                    Array.Empty<RealLayoutResilienceEvaluationRouteRowSnapshot>())
                {
                    if (row == null)
                    {
                        issues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                            "P6_ROUTE_ROW_MISSING",
                            LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                            "P6/Rows",
                            "P6 returned a null route row."));
                        continue;
                    }
                    LayoutResiliencePredicateState state = row.PredicateResult == null
                        ? LayoutResiliencePredicateState.Unknown
                        : row.PredicateResult.PredicateState;
                    rows.Add(new LayoutResilienceBattleSandboxPlaytestFeedbackRow(
                        row.MigrationRouteId,
                        ChineseDisplayName(row.MigrationRouteId),
                        row.Status,
                        state,
                        ChineseHint(state)));
                }
            }

            LayoutResilienceBattleSandboxPlaytestStatus status = pipeline == null
                ? LayoutResilienceBattleSandboxPlaytestStatus.Invalid
                : MapStatus(pipeline.Status);
            if (issues.Any(value => value.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid))
            {
                status = LayoutResilienceBattleSandboxPlaytestStatus.Invalid;
            }
            else if (status == LayoutResilienceBattleSandboxPlaytestStatus.Complete &&
                issues.Any(value => value.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Unknown))
            {
                status = LayoutResilienceBattleSandboxPlaytestStatus.Unknown;
            }

            return new LayoutResilienceBattleSandboxPlaytestSnapshot(
                status,
                itemSnapshotSignature,
                binding?.snapshot?.canonicalSignature ?? string.Empty,
                pipeline?.CanonicalSignature ?? string.Empty,
                rows,
                issues);
        }

        public static string ApplyToExistingFeedback(
            string existingFeedback,
            string aggregateFeedback)
        {
            string[] lines = (existingFeedback ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split(new[] { '\n' }, StringSplitOptions.None);
            string preserved = string.Join("\n", lines
                .Where(line => !line.TrimStart().StartsWith(
                    Prefix, StringComparison.Ordinal)))
                .TrimEnd('\n');
            string next = aggregateFeedback ?? string.Empty;
            if (next.Length == 0)
            {
                return preserved;
            }
            return preserved.Length == 0 ? next : preserved + "\n" + next;
        }

        public static bool ContainsAsciiLetter(string value)
        {
            return (value ?? string.Empty).Any(character =>
                (character >= 'A' && character <= 'Z') ||
                (character >= 'a' && character <= 'z'));
        }

        internal static string BuildAggregateText(
            LayoutResilienceBattleSandboxPlaytestStatus status,
            IReadOnlyList<LayoutResilienceBattleSandboxPlaytestFeedbackRow> rows)
        {
            if (status == LayoutResilienceBattleSandboxPlaytestStatus.Invalid)
            {
                return Prefix + "结构预览数据异常";
            }
            if (status == LayoutResilienceBattleSandboxPlaytestStatus.Unknown)
            {
                return Prefix + "当前布局信息不足，暂无法判断";
            }

            int stable = (rows ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestFeedbackRow>())
                .Count(value => value.predicateState ==
                    LayoutResiliencePredicateState.KnownTrue);
            int pressured = (rows ??
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestFeedbackRow>())
                .Count(value => value.predicateState ==
                    LayoutResiliencePredicateState.KnownFalse);
            if (stable == 0 && pressured == 0)
            {
                return Prefix + "当前机制不适用";
            }
            return Prefix + "稳定 " + stable.ToString(CultureInfo.InvariantCulture) +
                " 项／受压 " + pressured.ToString(CultureInfo.InvariantCulture) + " 项";
        }

        private static LayoutResilienceBattleSandboxPlaytestStatus MapStatus(
            RealLayoutResilienceEvaluationPipelineStatus status)
        {
            return status switch
            {
                RealLayoutResilienceEvaluationPipelineStatus.Complete =>
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete,
                RealLayoutResilienceEvaluationPipelineStatus.Unknown =>
                    LayoutResilienceBattleSandboxPlaytestStatus.Unknown,
                _ => LayoutResilienceBattleSandboxPlaytestStatus.Invalid
            };
        }

        private static string ChineseHint(LayoutResiliencePredicateState state)
        {
            return state switch
            {
                LayoutResiliencePredicateState.KnownTrue =>
                    "阵势在当前压力下保持稳定",
                LayoutResiliencePredicateState.KnownFalse =>
                    "阵势在当前压力下出现断点",
                LayoutResiliencePredicateState.NotApplicable =>
                    "当前机制不适用",
                _ => "当前布局信息不足，暂无法判断"
            };
        }

        private static string ChineseDisplayName(string route)
        {
            return route switch
            {
                FormationFurnaceRoute => "炉灰落阵·阵眼压力",
                FormationThunderRoute => "青石裂隙·阵眼压力",
                PollutionBluestoneRoute => "青石回潮·污染压力",
                PollutionFurnaceRoute => "炉灰落阵·污染压力",
                _ => "结构压力"
            };
        }
    }

    internal static class LayoutResilienceBattleSandboxPlaytestCanonical
    {
        public static string Create(
            LayoutResilienceBattleSandboxPlaytestSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "schemaId", snapshot?.schemaId);
            Field(builder, "schemaVersion", snapshot == null ? null :
                snapshot.schemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "status", snapshot == null ? null :
                ((int)snapshot.status).ToString(CultureInfo.InvariantCulture));
            Field(builder, "devOnly", snapshot == null ? null :
                (snapshot.devOnly ? "true" : "false"));
            Field(builder, "isEnabled", snapshot == null ? null :
                (snapshot.isEnabled ? "true" : "false"));
            Field(builder, "itemSnapshotSignature", snapshot?.itemSnapshotSignature);
            Field(builder, "bindingCanonicalSignature",
                snapshot?.bindingCanonicalSignature);
            Field(builder, "pipelineCanonicalSignature",
                snapshot?.pipelineCanonicalSignature);
            Field(builder, "aggregateFeedbackText", snapshot?.aggregateFeedbackText);

            IReadOnlyList<LayoutResilienceBattleSandboxPlaytestFeedbackRow> rows =
                snapshot?.feedbackRows;
            Field(builder, "feedbackRows.count", rows == null ? null :
                rows.Count.ToString(CultureInfo.InvariantCulture));
            if (rows != null)
            {
                for (int index = 0; index < rows.Count; index++)
                {
                    LayoutResilienceBattleSandboxPlaytestFeedbackRow row = rows[index];
                    string prefix = "feedbackRows[" +
                        index.ToString(CultureInfo.InvariantCulture) + "].";
                    Field(builder, prefix + "stableRouteKey", row.stableRouteKey);
                    Field(builder, prefix + "chineseDisplayName",
                        row.chineseDisplayName);
                    Field(builder, prefix + "pipelineRowStatus",
                        ((int)row.pipelineRowStatus).ToString(
                            CultureInfo.InvariantCulture));
                    Field(builder, prefix + "predicateState",
                        ((int)row.predicateState).ToString(
                            CultureInfo.InvariantCulture));
                    Field(builder, prefix + "chineseHint", row.chineseHint);
                    Field(builder, prefix + "answerMasked",
                        row.answerMasked ? "true" : "false");
                }
            }

            IReadOnlyList<LayoutResilienceBattleSandboxPlaytestIssue> issues =
                snapshot?.issues;
            Field(builder, "issues.count", issues == null ? null :
                issues.Count.ToString(CultureInfo.InvariantCulture));
            if (issues != null)
            {
                for (int index = 0; index < issues.Count; index++)
                {
                    LayoutResilienceBattleSandboxPlaytestIssue issue = issues[index];
                    string prefix = "issues[" +
                        index.ToString(CultureInfo.InvariantCulture) + "].";
                    Field(builder, prefix + "code", issue.code);
                    Field(builder, prefix + "status", ((int)issue.status)
                        .ToString(CultureInfo.InvariantCulture));
                    Field(builder, prefix + "path", issue.path);
                    Field(builder, prefix + "message", issue.message);
                }
            }
            return Hash(builder.ToString());
        }

        public static string HashText(string value)
        {
            return Hash(value ?? string.Empty);
        }

        private static void Field(
            StringBuilder builder,
            string name,
            string value)
        {
            string safeName = name ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeName).Append('=');
            if (value == null)
            {
                builder.Append("N;");
                return;
            }
            builder.Append('V')
                .Append(value.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(value).Append(';');
        }

        private static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(payload ?? string.Empty))
                    .Select(value => value.ToString("x2",
                        CultureInfo.InvariantCulture)));
            }
        }
    }
}
