using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience
{
    public sealed class LayoutResilienceValidationIssue
    {
        public LayoutResilienceValidationIssue(string code, string path, string message)
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

    public sealed class LayoutResilienceValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<LayoutResilienceValidationIssue> issues;

        public LayoutResilienceValidationException(
            IReadOnlyList<LayoutResilienceValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceValidationIssue>()).ToArray());
        }

        public IReadOnlyList<LayoutResilienceValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<LayoutResilienceValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Layout resilience structural predicate validation failed.");
            foreach (LayoutResilienceValidationIssue issue in issues ??
                Array.Empty<LayoutResilienceValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }
            return builder.ToString();
        }
    }

    public sealed class DefaultLayoutResilienceStructuralPredicateValidator :
        ILayoutResilienceStructuralPredicateValidator
    {
        public static readonly DefaultLayoutResilienceStructuralPredicateValidator Instance =
            new DefaultLayoutResilienceStructuralPredicateValidator();

        public IReadOnlyList<LayoutResilienceValidationIssue> Validate(
            LayoutResilienceEvaluationInput input)
        {
            List<LayoutResilienceValidationIssue> issues =
                new List<LayoutResilienceValidationIssue>();
            if (input == null)
            {
                Add(issues, "INPUT_NULL", "$", "Evaluation input is required.");
                return Freeze(issues);
            }

            if (!string.Equals(input.SchemaId,
                LayoutResilienceStructuralPredicateSchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(issues, "SCHEMA_ID_MISMATCH", "schemaId",
                    "Schema identity must match with ordinal semantics.");
            }
            if (input.SchemaVersion !=
                LayoutResilienceStructuralPredicateSchema.SchemaVersion)
            {
                Add(issues, "SCHEMA_VERSION_MISMATCH", "schemaVersion",
                    "Schema version must match exactly.");
            }
            ValidateIdentity(input.EvaluationId, "evaluationId", "EVALUATION_ID", issues);

            bool applicabilityDefined = IsDefined(input.ApplicabilityState);
            bool buildCompletenessDefined = IsDefined(input.BuildFactsCompleteness);
            bool pressureCompletenessDefined = IsDefined(input.PressureFactsCompleteness);
            if (!applicabilityDefined)
            {
                Add(issues, "APPLICABILITY_UNDEFINED", "applicabilityState",
                    "Applicability must be a named N01B value.");
            }
            if (!buildCompletenessDefined)
            {
                Add(issues, "BUILD_COMPLETENESS_UNDEFINED", "buildFactsCompleteness",
                    "Build completeness must be a named value.");
            }
            if (!pressureCompletenessDefined)
            {
                Add(issues, "PRESSURE_COMPLETENESS_UNDEFINED",
                    "pressureFactsCompleteness",
                    "Pressure completeness must be a named value.");
            }

            if (applicabilityDefined && buildCompletenessDefined &&
                pressureCompletenessDefined)
            {
                ValidateTruthTable(input, issues);
            }

            if (input.BuildFactsCompleteness == LayoutResilienceInputCompleteness.Complete &&
                input.BuildFacts == null)
            {
                Add(issues, "BUILD_FACTS_REQUIRED", "buildFacts",
                    "Complete build facts require a payload.");
            }
            if (input.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Complete &&
                input.Pressure == null)
            {
                Add(issues, "PRESSURE_FACTS_REQUIRED", "pressure",
                    "Complete pressure facts require a payload.");
            }
            if (input.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.NotRequired &&
                input.BuildFacts != null)
            {
                Add(issues, "BUILD_NOT_REQUIRED_PAYLOAD", "buildFacts",
                    "NotRequired build facts must not carry payload.");
            }
            if (input.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.NotRequired &&
                input.Pressure != null)
            {
                Add(issues, "PRESSURE_NOT_REQUIRED_PAYLOAD", "pressure",
                    "NotRequired pressure facts must not carry payload.");
            }

            if (input.BuildFacts != null)
            {
                ValidateBuildFacts(input.BuildFacts,
                    input.BuildFactsCompleteness ==
                        LayoutResilienceInputCompleteness.Complete,
                    issues);
            }
            if (input.Pressure != null)
            {
                int? boardSize = input.BuildFacts == null
                    ? null
                    : input.BuildFacts.BoardSize;
                ValidatePressure(input.Pressure,
                    input.PressureFactsCompleteness ==
                        LayoutResilienceInputCompleteness.Complete,
                    boardSize, issues);
            }

            return Freeze(issues);
        }

        public IReadOnlyList<LayoutResilienceValidationIssue> ValidateResult(
            LayoutResilienceEvaluationInput input,
            LayoutResiliencePredicateResultSnapshot result)
        {
            List<LayoutResilienceValidationIssue> issues = Validate(input).ToList();
            if (result == null)
            {
                Add(issues, "RESULT_NULL", "result", "Predicate result is required.");
                return Freeze(issues);
            }

            if (input == null)
            {
                return Freeze(issues);
            }
            if (!string.Equals(result.SchemaId, input.SchemaId, StringComparison.Ordinal) ||
                result.SchemaVersion != input.SchemaVersion)
            {
                Add(issues, "RESULT_SCHEMA_MISMATCH", "result.schema",
                    "Result schema must match its input.");
            }
            if (!string.Equals(result.EvaluationId, input.EvaluationId,
                StringComparison.Ordinal))
            {
                Add(issues, "RESULT_EVALUATION_ID_MISMATCH", "result.evaluationId",
                    "Result identity must match its input.");
            }
            if (result.ApplicabilityState != input.ApplicabilityState)
            {
                Add(issues, "RESULT_APPLICABILITY_MISMATCH",
                    "result.applicabilityState",
                    "Result applicability must match its input.");
            }
            if (!IsDefined(result.PredicateState))
            {
                Add(issues, "PREDICATE_STATE_UNDEFINED", "result.predicateState",
                    "Predicate state must be a named value.");
            }

            HashSet<LayoutResiliencePredicateClauseKind> seen =
                new HashSet<LayoutResiliencePredicateClauseKind>();
            for (int index = 0; index < result.ClauseRows.Count; index++)
            {
                LayoutResiliencePredicateClauseSnapshot row = result.ClauseRows[index];
                string path = "result.clauseRows[" + index.ToString(
                    CultureInfo.InvariantCulture) + "]";
                if (row == null)
                {
                    Add(issues, "RESULT_CLAUSE_NULL", path,
                        "Result clause rows cannot contain null.");
                    continue;
                }
                if (!IsDefined(row.ClauseKind))
                {
                    Add(issues, "RESULT_CLAUSE_KIND_UNDEFINED", path + ".clauseKind",
                        "Clause kind must be named.");
                }
                else if (!seen.Add(row.ClauseKind))
                {
                    Add(issues, "RESULT_CLAUSE_DUPLICATE", path + ".clauseKind",
                        "Clause kinds must be unique.");
                }
                if (!IsDefined(row.ClauseState))
                {
                    Add(issues, "RESULT_CLAUSE_STATE_UNDEFINED", path + ".clauseState",
                        "Clause state must be named.");
                }
            }

            bool known = result.PredicateState == LayoutResiliencePredicateState.KnownTrue ||
                result.PredicateState == LayoutResiliencePredicateState.KnownFalse;
            if (!known && result.ClauseRows.Count > 0)
            {
                Add(issues, "NON_KNOWN_RESULT_HAS_CLAUSES", "result.clauseRows",
                    "Unknown and NotApplicable results cannot carry clauses.");
            }
            if (known)
            {
                if (input.ApplicabilityState !=
                        EnemyRequirementApplicabilityState.Applicable ||
                    input.BuildFactsCompleteness !=
                        LayoutResilienceInputCompleteness.Complete ||
                    input.PressureFactsCompleteness !=
                        LayoutResilienceInputCompleteness.Complete)
                {
                    Add(issues, "KNOWN_RESULT_INPUT_INCOMPLETE", "result.predicateState",
                        "Known results require Applicable and complete inputs.");
                }
                LayoutResiliencePredicateClauseKind[] required = input.Pressure == null
                    ? Array.Empty<LayoutResiliencePredicateClauseKind>()
                    : input.Pressure.RequiredPredicateClauses.ToArray();
                LayoutResiliencePredicateClauseKind[] actual = result.ClauseRows
                    .Where(value => value != null && IsDefined(value.ClauseKind))
                    .Select(value => value.ClauseKind).Distinct().OrderBy(value => (int)value)
                    .ToArray();
                if (!required.OrderBy(value => (int)value).SequenceEqual(actual))
                {
                    Add(issues, "RESULT_CLAUSE_SET_MISMATCH", "result.clauseRows",
                        "Known results must carry exactly the required clauses.");
                }
                if (result.PredicateState == LayoutResiliencePredicateState.KnownTrue &&
                    result.ClauseRows.Any(value => value != null &&
                        value.ClauseState ==
                            LayoutResiliencePredicateClauseState.Violated))
                {
                    Add(issues, "KNOWN_TRUE_HAS_VIOLATED", "result.clauseRows",
                        "KnownTrue cannot contain a violated clause.");
                }
                if (result.PredicateState == LayoutResiliencePredicateState.KnownFalse &&
                    !result.ClauseRows.Any(value => value != null &&
                        value.ClauseState ==
                            LayoutResiliencePredicateClauseState.Violated))
                {
                    Add(issues, "KNOWN_FALSE_WITHOUT_VIOLATED", "result.clauseRows",
                        "KnownFalse requires at least one violated clause.");
                }
            }

            LayoutResiliencePredicateState expectedState = ExpectedDisposition(input);
            if ((expectedState == LayoutResiliencePredicateState.Unknown ||
                 expectedState == LayoutResiliencePredicateState.NotApplicable) &&
                result.PredicateState != expectedState)
            {
                Add(issues, "RESULT_DISPOSITION_MISMATCH", "result.predicateState",
                    "Unknown and NotApplicable dispositions cannot be converted.");
            }
            if (IsDefined(result.PredicateState))
            {
                string expectedSignature = LayoutResilienceCanonical.Signature(
                    input, result.PredicateState, result.ClauseRows);
                if (!string.Equals(result.CanonicalSignature, expectedSignature,
                    StringComparison.Ordinal))
                {
                    Add(issues, "RESULT_SIGNATURE_MISMATCH", "result.canonicalSignature",
                        "Canonical signature must cover input and result exactly.");
                }
            }

            return Freeze(issues);
        }

        private static void ValidateTruthTable(
            LayoutResilienceEvaluationInput input,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            if (input.ApplicabilityState ==
                EnemyRequirementApplicabilityState.NotInChannel)
            {
                Add(issues, "NOT_IN_CHANNEL_REJECTED", "applicabilityState",
                    "This evaluator cannot create a NotInChannel result.");
                return;
            }
            bool hasNotRequired = input.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.NotRequired ||
                input.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.NotRequired;
            if ((input.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable ||
                 input.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Unknown) && hasNotRequired)
            {
                Add(issues, "NOT_REQUIRED_COMBINATION_INVALID", "completeness",
                    "Applicable and Unknown cannot use NotRequired completeness.");
            }
            if (input.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.NotApplicable &&
                (input.BuildFactsCompleteness !=
                    LayoutResilienceInputCompleteness.NotRequired ||
                 input.PressureFactsCompleteness !=
                    LayoutResilienceInputCompleteness.NotRequired))
            {
                Add(issues, "NOT_APPLICABLE_COMPLETENESS_INVALID", "completeness",
                    "NotApplicable requires NotRequired on both input sides.");
            }
        }

        private static void ValidateBuildFacts(
            LayoutResilienceBuildFactSnapshot build,
            bool complete,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            if (!build.BoardSize.HasValue)
            {
                if (complete)
                {
                    Add(issues, "BOARD_SIZE_MISSING", "buildFacts.boardSize",
                        "Complete build facts require BoardSize.");
                }
            }
            else if (build.BoardSize.Value <= 0)
            {
                Add(issues, "BOARD_SIZE_INVALID", "buildFacts.boardSize",
                    "BoardSize must be positive when supplied.");
            }
            if (build.BaselineEyeCell == null)
            {
                if (complete)
                {
                    Add(issues, "BASELINE_EYE_MISSING", "buildFacts.baselineEyeCell",
                        "Complete build facts require the baseline eye cell.");
                }
            }
            else if (build.BoardSize.HasValue && build.BoardSize.Value > 0 &&
                !InBounds(build.BaselineEyeCell, build.BoardSize.Value))
            {
                Add(issues, "BASELINE_EYE_OUT_OF_BOUNDS",
                    "buildFacts.baselineEyeCell",
                    "Baseline eye cell must be within BoardSize.");
            }

            HashSet<string> placementIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < build.PlacementRows.Count; index++)
            {
                LayoutResiliencePlacedItemFactSnapshot row = build.PlacementRows[index];
                string path = "buildFacts.placementRows[" +
                    index.ToString(CultureInfo.InvariantCulture) + "]";
                if (row == null)
                {
                    Add(issues, "PLACEMENT_ROW_NULL", path,
                        "Placement rows cannot contain null.");
                    continue;
                }
                ValidateIdentity(row.PlacementId, path + ".placementId",
                    "PLACEMENT_ID", issues);
                ValidateIdentity(row.ItemId, path + ".itemId", "ITEM_ID", issues);
                if (!placementIds.Add(row.PlacementId))
                {
                    Add(issues, "PLACEMENT_ID_DUPLICATE", path + ".placementId",
                        "PlacementId must be unique with ordinal semantics.");
                }
                if (row.Rotation != 0 && row.Rotation != 90 && row.Rotation != 180 &&
                    row.Rotation != 270)
                {
                    Add(issues, "ROTATION_INVALID", path + ".rotation",
                        "Rotation must be 0 90 180 or 270.");
                }
                ValidateCellSet(row.ShapeCells, path + ".shapeCells", "SHAPE", true,
                    issues);
                ValidateCellSet(row.OccupiedCells, path + ".occupiedCells", "OCCUPIED",
                    true, issues);
                if (row.ShapeCells.Count != row.OccupiedCells.Count)
                {
                    Add(issues, "SHAPE_OCCUPIED_CARDINALITY_MISMATCH", path,
                        "ShapeCells and OccupiedCells must have equal cardinality.");
                }
                if (row.CoreCellWorld == null)
                {
                    Add(issues, "CORE_CELL_MISSING", path + ".coreCellWorld",
                        "Every supplied placement row requires a core cell.");
                }
                else if (!row.OccupiedCells.Any(value =>
                    value != null && value.Equals(row.CoreCellWorld)))
                {
                    Add(issues, "CORE_CELL_NOT_OCCUPIED", path + ".coreCellWorld",
                        "CoreCellWorld must be one of the OccupiedCells.");
                }
                if (row.AnchorCell == null)
                {
                    Add(issues, "ANCHOR_CELL_MISSING", path + ".anchorCell",
                        "Every supplied placement row requires an anchor cell.");
                }
                if (row.IsCountedInBuild && !row.IsLit)
                {
                    Add(issues, "COUNTED_NOT_LIT", path,
                        "A counted placement cannot be unlit.");
                }
                if (build.BoardSize.HasValue && build.BoardSize.Value > 0)
                {
                    int size = build.BoardSize.Value;
                    if (row.AnchorCell != null && !InBounds(row.AnchorCell, size))
                    {
                        Add(issues, "ANCHOR_CELL_OUT_OF_BOUNDS", path + ".anchorCell",
                            "AnchorCell must be within BoardSize.");
                    }
                    if (row.CoreCellWorld != null && !InBounds(row.CoreCellWorld, size))
                    {
                        Add(issues, "CORE_CELL_OUT_OF_BOUNDS", path + ".coreCellWorld",
                            "CoreCellWorld must be within BoardSize.");
                    }
                    if (row.OccupiedCells.Any(value => value != null &&
                        !InBounds(value, size)))
                    {
                        Add(issues, "OCCUPIED_CELL_OUT_OF_BOUNDS", path + ".occupiedCells",
                            "OccupiedCells must be within BoardSize.");
                    }
                }
            }
        }

        private static void ValidatePressure(
            LayoutPressureSnapshot pressure,
            bool complete,
            int? boardSize,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            ValidateIdentity(pressure.PressureInputId, "pressure.pressureInputId",
                "PRESSURE_INPUT_ID", issues);
            ValidateEnumSet(pressure.PressureKinds, "pressure.pressureKinds",
                "PRESSURE_KIND", IsDefined, complete, issues);
            ValidateEnumSet(pressure.RequiredPredicateClauses,
                "pressure.requiredPredicateClauses", "REQUIRED_CLAUSE", IsDefined,
                complete, issues);
            ValidateCellSet(pressure.LayoutDomainCells, "pressure.layoutDomainCells",
                "DOMAIN", complete, issues);
            ValidateCellSet(pressure.UsableCellsAfterPressure,
                "pressure.usableCellsAfterPressure", "USABLE", false, issues);

            HashSet<LayoutCellCoordinate> domain = new HashSet<LayoutCellCoordinate>(
                pressure.LayoutDomainCells.Where(value => value != null));
            HashSet<LayoutCellCoordinate> usable = new HashSet<LayoutCellCoordinate>(
                pressure.UsableCellsAfterPressure.Where(value => value != null));
            if (usable.Any(value => !domain.Contains(value)))
            {
                Add(issues, "USABLE_CELL_OUTSIDE_DOMAIN",
                    "pressure.usableCellsAfterPressure",
                    "Every usable cell must belong to LayoutDomainCells.");
            }
            if (pressure.EffectiveEyeAnchorCellAfterPressure == null)
            {
                if (complete)
                {
                    Add(issues, "EFFECTIVE_EYE_MISSING",
                        "pressure.effectiveEyeAnchorCellAfterPressure",
                        "Complete pressure facts require an effective eye anchor.");
                }
            }
            else if (boardSize.HasValue && boardSize.Value > 0 &&
                !InBounds(pressure.EffectiveEyeAnchorCellAfterPressure,
                    boardSize.Value))
            {
                Add(issues, "EFFECTIVE_EYE_OUT_OF_BOUNDS",
                    "pressure.effectiveEyeAnchorCellAfterPressure",
                    "Effective eye anchor must be within BoardSize.");
            }

            if (boardSize.HasValue && boardSize.Value > 0)
            {
                int size = boardSize.Value;
                long expectedCount = (long)size * size;
                bool exactDomain = pressure.LayoutDomainCells.Count == expectedCount &&
                    pressure.LayoutDomainCells.All(value => value != null &&
                        InBounds(value, size)) && domain.Count == expectedCount;
                if (!exactDomain)
                {
                    Add(issues, "DOMAIN_NOT_EXACT_BOARD", "pressure.layoutDomainCells",
                        "LayoutDomainCells must exactly enumerate the declared board.");
                }
                if (pressure.UsableCellsAfterPressure.Any(value => value != null &&
                    !InBounds(value, size)))
                {
                    Add(issues, "USABLE_CELL_OUT_OF_BOUNDS",
                        "pressure.usableCellsAfterPressure",
                        "Usable cells must be within BoardSize.");
                }
            }

            HashSet<string> connectionKeys = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0;
                index < pressure.PreservedStructuralConnectionsAfterPressure.Count;
                index++)
            {
                LayoutCellConnection connection =
                    pressure.PreservedStructuralConnectionsAfterPressure[index];
                string path = "pressure.preservedStructuralConnectionsAfterPressure[" +
                    index.ToString(CultureInfo.InvariantCulture) + "]";
                if (connection == null)
                {
                    Add(issues, "CONNECTION_NULL", path,
                        "Connections cannot contain null.");
                    continue;
                }
                if (connection.CellA == null || connection.CellB == null)
                {
                    Add(issues, "CONNECTION_ENDPOINT_MISSING", path,
                        "Both connection endpoints are required.");
                    continue;
                }
                if (connection.CellA.Equals(connection.CellB))
                {
                    Add(issues, "CONNECTION_SELF_LOOP", path,
                        "Self-loop connections are invalid.");
                }
                if (LayoutCellCoordinateComparer.Instance.Compare(
                    connection.CellA, connection.CellB) >= 0)
                {
                    Add(issues, "CONNECTION_NOT_NORMALIZED", path,
                        "Undirected endpoints must be in canonical cell order.");
                }
                if (!usable.Contains(connection.CellA) ||
                    !usable.Contains(connection.CellB))
                {
                    Add(issues, "CONNECTION_ENDPOINT_NOT_USABLE", path,
                        "Connection endpoints must both be declared usable.");
                }
                string key = connection.CellA + ">" + connection.CellB;
                if (!connectionKeys.Add(key))
                {
                    Add(issues, "CONNECTION_DUPLICATE", path,
                        "Structural connections must be unique.");
                }
            }
        }

        private static void ValidateCellSet(
            IReadOnlyList<LayoutCellCoordinate> values,
            string path,
            string codePrefix,
            bool requireNonEmpty,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            if (requireNonEmpty && values.Count == 0)
            {
                Add(issues, codePrefix + "_CELLS_EMPTY", path,
                    "The cell collection must be non-empty.");
            }
            if (values.Any(value => value == null))
            {
                Add(issues, codePrefix + "_CELL_NULL", path,
                    "Cell collections cannot contain null.");
            }
            int distinct = values.Where(value => value != null).Distinct().Count();
            if (distinct != values.Count(value => value != null))
            {
                Add(issues, codePrefix + "_CELL_DUPLICATE", path,
                    "Cell collections cannot contain duplicates.");
            }
        }

        private static void ValidateEnumSet<T>(
            IReadOnlyList<T> values,
            string path,
            string codePrefix,
            Func<T, bool> defined,
            bool requireNonEmpty,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            if (requireNonEmpty && values.Count == 0)
            {
                Add(issues, codePrefix + "_EMPTY", path,
                    "Complete pressure requires at least one declared value.");
            }
            if (values.Any(value => !defined(value)))
            {
                Add(issues, codePrefix + "_UNDEFINED", path,
                    "All enum values must be named.");
            }
            if (values.Distinct().Count() != values.Count)
            {
                Add(issues, codePrefix + "_DUPLICATE", path,
                    "Enum collections cannot contain duplicates.");
            }
        }

        private static LayoutResiliencePredicateState ExpectedDisposition(
            LayoutResilienceEvaluationInput input)
        {
            if (input.ApplicabilityState ==
                EnemyRequirementApplicabilityState.Unknown)
            {
                return LayoutResiliencePredicateState.Unknown;
            }
            if (input.ApplicabilityState ==
                EnemyRequirementApplicabilityState.NotApplicable)
            {
                return LayoutResiliencePredicateState.NotApplicable;
            }
            if (input.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable &&
                (input.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete ||
                 input.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete))
            {
                return LayoutResiliencePredicateState.Unknown;
            }
            return 0;
        }

        private static bool InBounds(LayoutCellCoordinate cell, int boardSize)
        {
            return cell != null && cell.X >= 0 && cell.Y >= 0 &&
                cell.X < boardSize && cell.Y < boardSize;
        }

        private static void ValidateIdentity(
            string value,
            string path,
            string codePrefix,
            ICollection<LayoutResilienceValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Add(issues, codePrefix + "_EMPTY", path,
                    "A non-empty ordinal identity is required.");
            }
            else if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                Add(issues, codePrefix + "_OUTER_WHITESPACE", path,
                    "Identity values are never trimmed implicitly.");
            }
        }

        private static bool IsDefined(EnemyRequirementApplicabilityState value)
        {
            return value == EnemyRequirementApplicabilityState.Applicable ||
                value == EnemyRequirementApplicabilityState.Unknown ||
                value == EnemyRequirementApplicabilityState.NotApplicable ||
                value == EnemyRequirementApplicabilityState.NotInChannel;
        }

        private static bool IsDefined(LayoutResilienceInputCompleteness value)
        {
            return value == LayoutResilienceInputCompleteness.Complete ||
                value == LayoutResilienceInputCompleteness.Incomplete ||
                value == LayoutResilienceInputCompleteness.NotRequired;
        }

        private static bool IsDefined(LayoutResiliencePredicateState value)
        {
            return value == LayoutResiliencePredicateState.KnownTrue ||
                value == LayoutResiliencePredicateState.KnownFalse ||
                value == LayoutResiliencePredicateState.Unknown ||
                value == LayoutResiliencePredicateState.NotApplicable;
        }

        private static bool IsDefined(LayoutPressureKind value)
        {
            return value == LayoutPressureKind.PollutedCellMask ||
                value == LayoutPressureKind.EyeRelocationOrDisruption ||
                value == LayoutPressureKind.StructuralConnectionCut;
        }

        private static bool IsDefined(LayoutResiliencePredicateClauseKind value)
        {
            return value == LayoutResiliencePredicateClauseKind.CountedLayoutPresent ||
                value == LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable ||
                value == LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable ||
                value == LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .EyeToCountedCoreStructurallyConnected;
        }

        private static bool IsDefined(LayoutResiliencePredicateClauseState value)
        {
            return value == LayoutResiliencePredicateClauseState.Satisfied ||
                value == LayoutResiliencePredicateClauseState.Violated;
        }

        private static ReadOnlyCollection<LayoutResilienceValidationIssue> Freeze(
            IEnumerable<LayoutResilienceValidationIssue> issues)
        {
            return Array.AsReadOnly((issues ??
                    Array.Empty<LayoutResilienceValidationIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal).ToArray());
        }

        private static void Add(
            ICollection<LayoutResilienceValidationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new LayoutResilienceValidationIssue(code, path, message));
        }
    }
}
