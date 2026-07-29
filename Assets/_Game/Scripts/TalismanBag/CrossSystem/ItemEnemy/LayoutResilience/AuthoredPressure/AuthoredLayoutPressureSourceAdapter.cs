using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure
{
    public sealed class DefaultAuthoredLayoutPressureSourceAdapter :
        IAuthoredLayoutPressureSourceAdapter
    {
        private const string ValidationEvaluationId =
            "dev.authored-layout-pressure-source.validation";

        public static readonly DefaultAuthoredLayoutPressureSourceAdapter Instance =
            new DefaultAuthoredLayoutPressureSourceAdapter();

        public AuthoredLayoutPressureSourceResult Project(
            AuthoredLayoutPressureSourceInput source)
        {
            AuthoredLayoutPressureAssessment assessment =
                DefaultAuthoredLayoutPressureSourceValidator.Instance.Assess(source);
            List<AuthoredLayoutPressureSourceIssue> issues =
                assessment.Issues.ToList();
            AuthoredLayoutPressureSourceStatus status = issues.Any(value =>
                    value.Status == AuthoredLayoutPressureSourceStatus.Invalid)
                ? AuthoredLayoutPressureSourceStatus.Invalid
                : issues.Count > 0
                    ? AuthoredLayoutPressureSourceStatus.Unknown
                    : AuthoredLayoutPressureSourceStatus.Complete;
            LayoutPressureSnapshot snapshot = status ==
                    AuthoredLayoutPressureSourceStatus.Complete
                ? assessment.Candidate
                : null;

            if (snapshot != null)
            {
                LayoutResilienceEvaluationInput validationEnvelope =
                    new LayoutResilienceEvaluationInput(
                        ValidationEvaluationId,
                        EnemyRequirementApplicabilityState.Unknown,
                        LayoutResilienceInputCompleteness.Incomplete,
                        LayoutResilienceInputCompleteness.Complete,
                        null,
                        snapshot);
                IReadOnlyList<LayoutResilienceValidationIssue> n01cIssues =
                    DefaultLayoutResilienceStructuralPredicateValidator.Instance
                        .Validate(validationEnvelope);
                if (n01cIssues.Count > 0)
                {
                    issues.AddRange(n01cIssues.Select(value =>
                        new AuthoredLayoutPressureSourceIssue(
                            AuthoredLayoutPressureSourceValidationCodes.N01CRejected,
                            AuthoredLayoutPressureSourceStatus.Invalid,
                            "n01c." + value.Path,
                            value.Code + ": " + value.Message)));
                    status = AuthoredLayoutPressureSourceStatus.Invalid;
                    snapshot = null;
                }
            }

            LayoutResilienceInputCompleteness completeness = status ==
                    AuthoredLayoutPressureSourceStatus.Complete
                ? LayoutResilienceInputCompleteness.Complete
                : LayoutResilienceInputCompleteness.Incomplete;
            string signature = AuthoredLayoutPressureCanonical.Create(
                source, status, completeness, snapshot, issues);
            return new AuthoredLayoutPressureSourceResult(
                status, completeness, snapshot, issues, signature);
        }
    }
}
