using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TalismanBag.EnemySystem.ReadinessEvaluation
{
    public static class EnemyOfflineReadinessSchema
    {
        public const string SchemaId = "EnemyOfflineReadiness.v1";
        public const int SchemaVersion = 1;
    }

    public enum RequirementEvaluationStatus
    {
        Met = 0,
        Unmet = 1,
        Unknown = 2
    }

    public enum RequirementGroupStatus
    {
        Met = 0,
        Unmet = 1,
        Unknown = 2
    }

    public enum PlayerHintSeverity
    {
        None = 0,
        Notice = 1,
        Warning = 2,
        Critical = 3
    }

    public interface IEnemyReadinessReferenceResolver
    {
        bool HasBuildCapabilityKey(string key);
        bool HasMapRule(string mapRuleId);
    }

    public interface IEnemyOfflineReadinessEvaluator
    {
        EnemyReadinessEvaluationResult Evaluate(
            EnemyReadinessEvaluationInput input,
            IEnemyReadinessReferenceResolver resolver);
    }

    public sealed class EnemyReadinessValidationIssue
    {
        public EnemyReadinessValidationIssue(
            string code,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public sealed class EnemyReadinessEvaluationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EnemyReadinessValidationIssue> issues;

        public EnemyReadinessEvaluationException(
            IReadOnlyList<EnemyReadinessValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<EnemyReadinessValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemyReadinessValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<EnemyReadinessValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Enemy offline readiness evaluation failed validation.");
            foreach (EnemyReadinessValidationIssue issue in
                issues ?? Array.Empty<EnemyReadinessValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    internal static class EnemyReadinessReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<string> Strings(
            IEnumerable<string> values,
            bool sortOrdinal = true)
        {
            IEnumerable<string> source =
                (values ?? Array.Empty<string>()).Select(Text);
            if (sortOrdinal)
            {
                source = source.OrderBy(value => value, StringComparer.Ordinal);
            }

            return Array.AsReadOnly(source.ToArray());
        }

        public static ReadOnlyCollection<T> Freeze<T>(
            IEnumerable<T> values,
            Func<T, T> clone)
            where T : class
        {
            if (clone == null)
            {
                throw new ArgumentNullException(nameof(clone));
            }

            return Array.AsReadOnly((values ?? Array.Empty<T>())
                .Select(value => value == null ? null : clone(value))
                .ToArray());
        }

        public static ReadOnlyCollection<T> ToReadOnly<T>(
            this IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }
}
