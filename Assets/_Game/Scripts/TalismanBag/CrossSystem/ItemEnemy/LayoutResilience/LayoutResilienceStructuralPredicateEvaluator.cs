using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience
{
    public sealed class DefaultLayoutResilienceStructuralPredicateEvaluator :
        ILayoutResilienceStructuralPredicateEvaluator
    {
        public static readonly DefaultLayoutResilienceStructuralPredicateEvaluator Instance =
            new DefaultLayoutResilienceStructuralPredicateEvaluator();

        private readonly ILayoutResilienceStructuralPredicateValidator validator;

        public DefaultLayoutResilienceStructuralPredicateEvaluator(
            ILayoutResilienceStructuralPredicateValidator validator = null)
        {
            this.validator = validator ??
                DefaultLayoutResilienceStructuralPredicateValidator.Instance;
        }

        public LayoutResiliencePredicateResultSnapshot Evaluate(
            LayoutResilienceEvaluationInput input)
        {
            IReadOnlyList<LayoutResilienceValidationIssue> inputIssues =
                validator.Validate(input);
            if (inputIssues.Count > 0)
            {
                throw new LayoutResilienceValidationException(inputIssues);
            }

            LayoutResiliencePredicateState state;
            IReadOnlyList<LayoutResiliencePredicateClauseSnapshot> clauses;
            if (input.ApplicabilityState ==
                EnemyRequirementApplicabilityState.NotApplicable)
            {
                state = LayoutResiliencePredicateState.NotApplicable;
                clauses = Array.Empty<LayoutResiliencePredicateClauseSnapshot>();
            }
            else if (input.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Unknown ||
                input.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete ||
                input.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete)
            {
                state = LayoutResiliencePredicateState.Unknown;
                clauses = Array.Empty<LayoutResiliencePredicateClauseSnapshot>();
            }
            else
            {
                clauses = EvaluateClauses(input.BuildFacts, input.Pressure);
                state = clauses.All(value => value.ClauseState ==
                        LayoutResiliencePredicateClauseState.Satisfied)
                    ? LayoutResiliencePredicateState.KnownTrue
                    : LayoutResiliencePredicateState.KnownFalse;
            }

            string signature = LayoutResilienceCanonical.Signature(input, state, clauses);
            LayoutResiliencePredicateResultSnapshot result =
                new LayoutResiliencePredicateResultSnapshot(
                    input.SchemaId,
                    input.SchemaVersion,
                    input.EvaluationId,
                    input.ApplicabilityState,
                    state,
                    clauses,
                    signature);
            IReadOnlyList<LayoutResilienceValidationIssue> resultIssues =
                validator.ValidateResult(input, result);
            if (resultIssues.Count > 0)
            {
                throw new LayoutResilienceValidationException(resultIssues);
            }
            return result;
        }

        private static IReadOnlyList<LayoutResiliencePredicateClauseSnapshot>
            EvaluateClauses(
                LayoutResilienceBuildFactSnapshot build,
                LayoutPressureSnapshot pressure)
        {
            LayoutResiliencePlacedItemFactSnapshot[] counted = build.PlacementRows
                .Where(value => value.IsCountedInBuild).ToArray();
            HashSet<LayoutCellCoordinate> usable = new HashSet<LayoutCellCoordinate>(
                pressure.UsableCellsAfterPressure);
            List<LayoutResiliencePredicateClauseSnapshot> rows =
                new List<LayoutResiliencePredicateClauseSnapshot>();

            foreach (LayoutResiliencePredicateClauseKind clause in
                pressure.RequiredPredicateClauses.OrderBy(value => (int)value))
            {
                bool satisfied;
                switch (clause)
                {
                    case LayoutResiliencePredicateClauseKind.CountedLayoutPresent:
                        satisfied = counted.Length > 0;
                        break;
                    case LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable:
                        satisfied = counted.All(placement =>
                            placement.OccupiedCells.All(usable.Contains));
                        break;
                    case LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable:
                        satisfied = counted.All(placement =>
                            usable.Contains(placement.CoreCellWorld));
                        break;
                    case LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable:
                        satisfied = usable.Contains(
                            pressure.EffectiveEyeAnchorCellAfterPressure);
                        break;
                    case LayoutResiliencePredicateClauseKind
                        .EyeToCountedCoreStructurallyConnected:
                        satisfied = AllCountedCoresReachable(
                            pressure.EffectiveEyeAnchorCellAfterPressure,
                            counted.Select(value => value.CoreCellWorld),
                            usable,
                            pressure.PreservedStructuralConnectionsAfterPressure);
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Validator allowed an undefined predicate clause.");
                }
                rows.Add(new LayoutResiliencePredicateClauseSnapshot(
                    clause,
                    satisfied
                        ? LayoutResiliencePredicateClauseState.Satisfied
                        : LayoutResiliencePredicateClauseState.Violated));
            }
            return rows.AsReadOnly();
        }

        private static bool AllCountedCoresReachable(
            LayoutCellCoordinate eye,
            IEnumerable<LayoutCellCoordinate> cores,
            ISet<LayoutCellCoordinate> usable,
            IEnumerable<LayoutCellConnection> connections)
        {
            if (!usable.Contains(eye))
            {
                return false;
            }
            Dictionary<LayoutCellCoordinate, List<LayoutCellCoordinate>> graph =
                new Dictionary<LayoutCellCoordinate, List<LayoutCellCoordinate>>();
            foreach (LayoutCellCoordinate cell in usable)
            {
                graph[cell] = new List<LayoutCellCoordinate>();
            }
            foreach (LayoutCellConnection connection in connections)
            {
                graph[connection.CellA].Add(connection.CellB);
                graph[connection.CellB].Add(connection.CellA);
            }
            Queue<LayoutCellCoordinate> pending = new Queue<LayoutCellCoordinate>();
            HashSet<LayoutCellCoordinate> visited = new HashSet<LayoutCellCoordinate>();
            pending.Enqueue(eye);
            visited.Add(eye);
            while (pending.Count > 0)
            {
                LayoutCellCoordinate current = pending.Dequeue();
                foreach (LayoutCellCoordinate next in graph[current]
                    .OrderBy(value => value, LayoutCellCoordinateComparer.Instance))
                {
                    if (visited.Add(next))
                    {
                        pending.Enqueue(next);
                    }
                }
            }
            return cores.All(visited.Contains);
        }
    }
}
