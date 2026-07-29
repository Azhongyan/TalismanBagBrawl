using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness
{
    public sealed class DefaultLayoutResilienceStructuralReadinessConsumer :
        ILayoutResilienceStructuralReadinessConsumer
    {
        public static readonly DefaultLayoutResilienceStructuralReadinessConsumer
            Instance = new DefaultLayoutResilienceStructuralReadinessConsumer();

        private readonly ILayoutResilienceStructuralPredicateEvaluator evaluator;
        private readonly Action evaluateObserver;
        private readonly Action predicateValidateObserver;
        private readonly Action assemblerValidateObserver;

        public DefaultLayoutResilienceStructuralReadinessConsumer()
            : this(null, null, null, null)
        {
        }

        internal DefaultLayoutResilienceStructuralReadinessConsumer(
            ILayoutResilienceStructuralPredicateEvaluator syntheticEvaluator,
            Action evaluateObserver,
            Action predicateValidateObserver,
            Action assemblerValidateObserver)
        {
            evaluator = syntheticEvaluator ??
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance;
            this.evaluateObserver = evaluateObserver;
            this.predicateValidateObserver = predicateValidateObserver;
            this.assemblerValidateObserver = assemblerValidateObserver;
        }

        public LayoutResilienceStructuralReadinessConsumerResult Consume(
            LayoutResilienceStructuralReadinessConsumerInput input)
        {
            if (input == null)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .InputNull,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "$",
                        "Consumer input is required."));
            }

            if (!string.Equals(input.SchemaId,
                    LayoutResilienceStructuralReadinessConsumerSchema.SchemaId,
                    StringComparison.Ordinal) ||
                input.SchemaVersion !=
                    LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .SchemaMismatch,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "schema",
                        "Consumer input schema must match exactly."));
            }

            LayoutResilienceEvaluationInputAssemblerResult source =
                input.AssemblerResult;
            if (source == null)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .AssemblerResultMissing,
                        LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                        "assemblerResult",
                        "P3 assembler result is missing."));
            }

            IReadOnlyList<LayoutResilienceEvaluationInputAssemblerIssue>
                sourceValidation;
            try
            {
                assemblerValidateObserver?.Invoke();
                sourceValidation =
                    DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance
                        .ValidateResult(source);
            }
            catch (Exception)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .AssemblerResultValidationException,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "assemblerResult",
                        "P3 result validation raised an exception."));
            }

            if (sourceValidation == null || sourceValidation.Count != 0)
            {
                IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> issues =
                    sourceValidation == null
                        ? new[]
                        {
                            Issue(
                                LayoutResilienceStructuralReadinessConsumerValidationCodes
                                    .AssemblerResultRejected,
                                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                                "assemblerResult",
                                "P3 result validator returned no diagnostic collection.")
                        }
                        : sourceValidation.Select(value => Issue(
                            LayoutResilienceStructuralReadinessConsumerValidationCodes
                                .AssemblerResultRejected,
                            LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                            "assemblerResult." + (value == null
                                ? string.Empty
                                : value.Path),
                            "P3 result rejected by its public validator."));
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    issues);
            }

            if (source.Status ==
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    PrefixSourceIssues(source.Issues,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid));
            }

            if (source.Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown &&
                source.EvaluationInput == null)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                    null,
                    PrefixSourceIssues(source.Issues,
                        LayoutResilienceStructuralReadinessConsumerStatus.Unknown));
            }

            if (!DispositionAllowsEvaluation(source))
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .AssemblerDispositionInvalid,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "assemblerResult",
                        "P3 status and EvaluationInput disposition are inconsistent."));
            }

            LayoutResiliencePredicateResultSnapshot evaluated;
            try
            {
                evaluateObserver?.Invoke();
                evaluated = evaluator.Evaluate(source.EvaluationInput);
            }
            catch (Exception)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .EvaluatorException,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "predicateResult",
                        "The authoritative evaluator raised an exception."));
            }

            IReadOnlyList<LayoutResilienceValidationIssue> resultValidation;
            try
            {
                predicateValidateObserver?.Invoke();
                resultValidation =
                    DefaultLayoutResilienceStructuralPredicateValidator.Instance
                        .ValidateResult(source.EvaluationInput, evaluated);
            }
            catch (Exception)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .PredicateValidationException,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "predicateResult",
                        "N01C result validation raised an exception."));
            }

            if (resultValidation == null || resultValidation.Count != 0)
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .PredicateResultRejected,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "predicateResult",
                        "N01C result was rejected by the authoritative validator."));
            }

            if (!PredicateDispositionMatches(source.Status, evaluated.PredicateState))
            {
                return Create(input,
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    null,
                    Issue(
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .PredicateDispositionMismatch,
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "predicateResult.predicateState",
                        "P3 status and N01C predicate state are inconsistent."));
            }

            LayoutResilienceStructuralReadinessConsumerStatus outputStatus =
                evaluated.PredicateState == LayoutResiliencePredicateState.Unknown
                    ? LayoutResilienceStructuralReadinessConsumerStatus.Unknown
                    : LayoutResilienceStructuralReadinessConsumerStatus.Complete;
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> outputIssues =
                outputStatus == LayoutResilienceStructuralReadinessConsumerStatus.Unknown
                    ? PrefixSourceIssues(source.Issues,
                        LayoutResilienceStructuralReadinessConsumerStatus.Unknown)
                    : Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>();
            return Create(input, outputStatus, evaluated, outputIssues);
        }

        private static bool DispositionAllowsEvaluation(
            LayoutResilienceEvaluationInputAssemblerResult source)
        {
            if (source == null || source.EvaluationInput == null)
            {
                return false;
            }
            LayoutResilienceEvaluationInput value = source.EvaluationInput;
            if (source.Status ==
                LayoutResilienceEvaluationInputAssemblerStatus.Complete)
            {
                bool applicable =
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable &&
                    value.BuildFactsCompleteness ==
                        LayoutResilienceInputCompleteness.Complete &&
                    value.PressureFactsCompleteness ==
                        LayoutResilienceInputCompleteness.Complete;
                bool notApplicable =
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotApplicable &&
                    value.BuildFactsCompleteness ==
                        LayoutResilienceInputCompleteness.NotRequired &&
                    value.PressureFactsCompleteness ==
                        LayoutResilienceInputCompleteness.NotRequired &&
                    value.BuildFacts == null &&
                    value.Pressure == null;
                return applicable || notApplicable;
            }

            if (source.Status !=
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown)
            {
                return false;
            }

            bool legalCompleteness = SideIsKnownOrIncomplete(
                    value.BuildFactsCompleteness) &&
                SideIsKnownOrIncomplete(value.PressureFactsCompleteness);
            if (!legalCompleteness)
            {
                return false;
            }
            if (value.ApplicabilityState ==
                EnemyRequirementApplicabilityState.Unknown)
            {
                return true;
            }
            return value.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable &&
                (value.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete ||
                 value.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete);
        }

        private static bool SideIsKnownOrIncomplete(
            LayoutResilienceInputCompleteness value)
        {
            return value == LayoutResilienceInputCompleteness.Complete ||
                value == LayoutResilienceInputCompleteness.Incomplete;
        }

        private static bool PredicateDispositionMatches(
            LayoutResilienceEvaluationInputAssemblerStatus sourceStatus,
            LayoutResiliencePredicateState predicateState)
        {
            if (sourceStatus ==
                LayoutResilienceEvaluationInputAssemblerStatus.Complete)
            {
                return predicateState == LayoutResiliencePredicateState.KnownTrue ||
                    predicateState == LayoutResiliencePredicateState.KnownFalse ||
                    predicateState == LayoutResiliencePredicateState.NotApplicable;
            }
            return sourceStatus ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown &&
                predicateState == LayoutResiliencePredicateState.Unknown;
        }

        private static IEnumerable<
            LayoutResilienceStructuralReadinessConsumerIssue> PrefixSourceIssues(
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> values,
            LayoutResilienceStructuralReadinessConsumerStatus status)
        {
            LayoutResilienceEvaluationInputAssemblerIssue[] rows = (values ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray();
            if (rows.Length == 0)
            {
                return status == LayoutResilienceStructuralReadinessConsumerStatus.Invalid
                    ? new[]
                    {
                        Issue(
                            LayoutResilienceStructuralReadinessConsumerValidationCodes
                                .AssemblerSourceInvalid,
                            status,
                            "assemblerResult",
                            "P3 reported Invalid without a consumable diagnostic.")
                    }
                    : Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>();
            }
            return rows.Select(value => Issue(
                "P3." + value.Code,
                status,
                "assemblerResult." + value.Path,
                value.Message));
        }

        private static LayoutResilienceStructuralReadinessConsumerIssue Issue(
            string code,
            LayoutResilienceStructuralReadinessConsumerStatus status,
            string path,
            string message)
        {
            return new LayoutResilienceStructuralReadinessConsumerIssue(
                code, status, path, message);
        }

        private static LayoutResilienceStructuralReadinessConsumerResult Create(
            LayoutResilienceStructuralReadinessConsumerInput input,
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicateResult,
            params LayoutResilienceStructuralReadinessConsumerIssue[] issues)
        {
            return Create(input, status, predicateResult,
                (IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue>)issues);
        }

        private static LayoutResilienceStructuralReadinessConsumerResult Create(
            LayoutResilienceStructuralReadinessConsumerInput input,
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicateResult,
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> issues)
        {
            LayoutResilienceStructuralReadinessConsumerIssue[] frozen = (issues ??
                    Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray();
            string signature =
                LayoutResilienceStructuralReadinessConsumerCanonical.Create(
                    input, status, predicateResult, frozen);
            return new LayoutResilienceStructuralReadinessConsumerResult(
                status, predicateResult, frozen, signature);
        }
    }
}
