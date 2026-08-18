using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure
{
    public static class AuthoredLayoutPressureSourceValidationCodes
    {
        public const string SourceMissing = "SOURCE_MISSING";
        public const string SchemaMismatch = "SCHEMA_ID_OR_VERSION_MISMATCH";
        public const string CompletenessUndefined = "AUTHORING_COMPLETENESS_UNDEFINED";
        public const string PressureInputIdMissing = "PRESSURE_INPUT_ID_MISSING";
        public const string PressureInputIdOuterWhitespace = "PRESSURE_INPUT_ID_OUTER_WHITESPACE";
        public const string BoardSizeMissing = "DECLARED_BOARD_SIZE_MISSING";
        public const string BoardSizeInvalid = "DECLARED_BOARD_SIZE_INVALID";
        public const string PressureKindsMissing = "PRESSURE_KINDS_MISSING";
        public const string PressureKindUndefined = "PRESSURE_KIND_UNDEFINED";
        public const string PressureKindDuplicate = "PRESSURE_KIND_DUPLICATE";
        public const string DomainMissing = "LAYOUT_DOMAIN_MISSING";
        public const string DomainIncomplete = "LAYOUT_DOMAIN_INCOMPLETE";
        public const string DomainCellNull = "LAYOUT_DOMAIN_CELL_NULL";
        public const string DomainCellDuplicate = "LAYOUT_DOMAIN_CELL_DUPLICATE";
        public const string DomainCellOutOfBounds = "LAYOUT_DOMAIN_CELL_OUT_OF_BOUNDS";
        public const string UsableMissing = "USABLE_CELLS_MISSING";
        public const string UsableCellNull = "USABLE_CELL_NULL";
        public const string UsableCellDuplicate = "USABLE_CELL_DUPLICATE";
        public const string UsableCellOutsideDomain = "USABLE_CELL_OUTSIDE_DOMAIN";
        public const string UsableCellOutOfBounds = "USABLE_CELL_OUT_OF_BOUNDS";
        public const string EffectiveEyeMissing = "EFFECTIVE_EYE_MISSING";
        public const string EffectiveEyeOutsideDomain = "EFFECTIVE_EYE_OUTSIDE_DOMAIN";
        public const string EffectiveEyeOutOfBounds = "EFFECTIVE_EYE_OUT_OF_BOUNDS";
        public const string ConnectionsMissing = "CONNECTIONS_MISSING";
        public const string ConnectionNull = "CONNECTION_NULL";
        public const string ConnectionEndpointMissing = "CONNECTION_ENDPOINT_MISSING";
        public const string ConnectionSelfLoop = "CONNECTION_SELF_LOOP";
        public const string ConnectionEndpointOutOfBounds = "CONNECTION_ENDPOINT_OUT_OF_BOUNDS";
        public const string ConnectionEndpointNotUsable = "CONNECTION_ENDPOINT_NOT_USABLE";
        public const string ConnectionDuplicate = "CONNECTION_DUPLICATE";
        public const string ClausesMissing = "REQUIRED_CLAUSES_MISSING";
        public const string ClauseUndefined = "REQUIRED_CLAUSE_UNDEFINED";
        public const string ClauseDuplicate = "REQUIRED_CLAUSE_DUPLICATE";
        public const string AuthoringIncomplete = "AUTHORING_INCOMPLETE";
        public const string N01CRejected = "N01C_PRESSURE_VALIDATION_REJECTED";
    }

    internal sealed class AuthoredLayoutPressureAssessment
    {
        public AuthoredLayoutPressureAssessment(
            IReadOnlyList<AuthoredLayoutPressureSourceIssue> issues,
            LayoutPressureSnapshot candidate)
        {
            Issues = issues;
            Candidate = candidate;
        }

        public IReadOnlyList<AuthoredLayoutPressureSourceIssue> Issues { get; }
        public LayoutPressureSnapshot Candidate { get; }
    }

    public sealed class DefaultAuthoredLayoutPressureSourceValidator
    {
        public static readonly DefaultAuthoredLayoutPressureSourceValidator Instance =
            new DefaultAuthoredLayoutPressureSourceValidator();

        public IReadOnlyList<AuthoredLayoutPressureSourceIssue> Validate(
            AuthoredLayoutPressureSourceInput source)
        {
            return Assess(source).Issues;
        }

        internal AuthoredLayoutPressureAssessment Assess(
            AuthoredLayoutPressureSourceInput source)
        {
            List<AuthoredLayoutPressureSourceIssue> issues =
                new List<AuthoredLayoutPressureSourceIssue>();
            if (source == null)
            {
                AddUnknown(issues, AuthoredLayoutPressureSourceValidationCodes.SourceMissing,
                    "$", "The authored pressure source is missing.");
                return Finish(issues, null);
            }

            if (!string.Equals(source.SchemaId,
                    AuthoredLayoutPressureSourceSchema.SchemaId,
                    StringComparison.Ordinal) ||
                source.SchemaVersion != AuthoredLayoutPressureSourceSchema.SchemaVersion)
            {
                AddInvalid(issues, AuthoredLayoutPressureSourceValidationCodes.SchemaMismatch,
                    "schema", "Schema identity and version must match exactly.");
            }
            if (!Defined(source.AuthoringCompleteness))
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.CompletenessUndefined,
                    "authoringCompleteness", "Completeness must be a named value.");
            }
            ValidateIdentity(source.PressureInputId, issues);
            ValidateBoard(source.DeclaredBoardSize, issues);
            ValidateKinds(source.PressureKinds, issues);
            ValidateCells(source.LayoutDomainCells, "layoutDomainCells", true,
                AuthoredLayoutPressureSourceValidationCodes.DomainMissing,
                AuthoredLayoutPressureSourceValidationCodes.DomainCellNull,
                AuthoredLayoutPressureSourceValidationCodes.DomainCellDuplicate,
                issues);
            ValidateCells(source.UsableCellsAfterPressure,
                "usableCellsAfterPressure", false,
                AuthoredLayoutPressureSourceValidationCodes.UsableMissing,
                AuthoredLayoutPressureSourceValidationCodes.UsableCellNull,
                AuthoredLayoutPressureSourceValidationCodes.UsableCellDuplicate,
                issues);
            ValidateDomainRelations(source, issues);
            ValidateConnections(source, issues);
            ValidateClauses(source.RequiredPredicateClauses, issues);

            if (source.AuthoringCompleteness ==
                AuthoredLayoutPressureAuthoringCompleteness.Incomplete)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.AuthoringIncomplete,
                    "authoringCompleteness",
                    "Incomplete authored pressure facts cannot create a snapshot.");
            }

            if (issues.Any(value =>
                    value.Status == AuthoredLayoutPressureSourceStatus.Invalid) ||
                issues.Any(value =>
                    value.Status == AuthoredLayoutPressureSourceStatus.Unknown))
            {
                return Finish(issues, null);
            }

            LayoutPressureSnapshot candidate = new LayoutPressureSnapshot(
                source.PressureInputId,
                source.PressureKinds.OrderBy(value => (int)value),
                source.LayoutDomainCells.OrderBy(value => value.Y)
                    .ThenBy(value => value.X),
                source.UsableCellsAfterPressure.OrderBy(value => value.Y)
                    .ThenBy(value => value.X),
                source.EffectiveEyeAnchorCellAfterPressure,
                source.PreservedStructuralConnectionsAfterPressure
                    .Select(AuthoredLayoutPressureCanonical.NormalizeConnection)
                    .OrderBy(AuthoredLayoutPressureCanonical.ConnectionKey,
                        StringComparer.Ordinal),
                source.RequiredPredicateClauses.OrderBy(value => (int)value));
            return Finish(issues, candidate);
        }

        private static void ValidateIdentity(
            string value,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.PressureInputIdMissing,
                    "pressureInputId", "PressureInputId is required.");
            }
            else if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes
                        .PressureInputIdOuterWhitespace,
                    "pressureInputId", "PressureInputId cannot have outer whitespace.");
            }
        }

        private static void ValidateBoard(
            int? value,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            if (!value.HasValue)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.BoardSizeMissing,
                    "declaredBoardSize", "DeclaredBoardSize is required.");
            }
            else if (value.Value <= 0)
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.BoardSizeInvalid,
                    "declaredBoardSize", "DeclaredBoardSize must be positive.");
            }
        }

        private static void ValidateKinds(
            IReadOnlyList<LayoutPressureKind> values,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            if (values == null || values.Count == 0)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.PressureKindsMissing,
                    "pressureKinds", "At least one explicit pressure kind is required.");
                return;
            }
            if (values.Any(value => !Defined(value)))
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.PressureKindUndefined,
                    "pressureKinds", "All pressure kinds must be named values.");
            }
            if (values.Distinct().Count() != values.Count)
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.PressureKindDuplicate,
                    "pressureKinds", "Pressure kinds must be unique.");
            }
        }

        private static void ValidateCells(
            IReadOnlyList<LayoutCellCoordinate> values,
            string path,
            bool emptyIsMissing,
            string missingCode,
            string nullCode,
            string duplicateCode,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            if (values == null || (emptyIsMissing && values.Count == 0))
            {
                AddUnknown(issues, missingCode, path,
                    "The authored cell collection is missing.");
                return;
            }
            if (values.Any(value => value == null))
            {
                AddUnknown(issues, nullCode, path,
                    "A supplied cell row is missing its coordinate.");
            }
            int nonNull = values.Count(value => value != null);
            if (values.Where(value => value != null).Distinct().Count() != nonNull)
            {
                AddInvalid(issues, duplicateCode, path,
                    "Supplied cells must be unique.");
            }
        }

        private static void ValidateDomainRelations(
            AuthoredLayoutPressureSourceInput source,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            bool hasBoard = source.DeclaredBoardSize.HasValue &&
                source.DeclaredBoardSize.Value > 0;
            int boardSize = hasBoard ? source.DeclaredBoardSize.Value : 0;
            IReadOnlyList<LayoutCellCoordinate> domain = source.LayoutDomainCells;
            IReadOnlyList<LayoutCellCoordinate> usable = source.UsableCellsAfterPressure;
            bool exactDomain = false;
            if (domain != null && hasBoard)
            {
                if (domain.Any(value => value != null && !InBounds(value, boardSize)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .DomainCellOutOfBounds,
                        "layoutDomainCells", "Domain cells must be within the board.");
                }
                long expected = (long)boardSize * boardSize;
                exactDomain = domain.Count == expected &&
                    domain.All(value => value != null && InBounds(value, boardSize)) &&
                    domain.Distinct().LongCount() == expected;
                if (!exactDomain && !domain.Any(value => value != null &&
                    !InBounds(value, boardSize)))
                {
                    AddUnknown(issues,
                        AuthoredLayoutPressureSourceValidationCodes.DomainIncomplete,
                        "layoutDomainCells",
                        "Domain must explicitly enumerate every declared board cell.");
                }
            }
            HashSet<LayoutCellCoordinate> domainSet = new HashSet<LayoutCellCoordinate>(
                domain == null
                    ? Enumerable.Empty<LayoutCellCoordinate>()
                    : domain.Where(value => value != null));
            if (usable != null)
            {
                if (hasBoard && usable.Any(value => value != null &&
                    !InBounds(value, boardSize)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .UsableCellOutOfBounds,
                        "usableCellsAfterPressure",
                        "Usable cells must be within the declared board.");
                }
                if (exactDomain && usable.Any(value => value != null &&
                    !domainSet.Contains(value)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .UsableCellOutsideDomain,
                        "usableCellsAfterPressure",
                        "Usable cells must belong to the explicit domain.");
                }
            }
            LayoutCellCoordinate eye = source.EffectiveEyeAnchorCellAfterPressure;
            if (eye == null)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.EffectiveEyeMissing,
                    "effectiveEyeAnchorCellAfterPressure",
                    "An effective eye anchor is required.");
            }
            else
            {
                if (hasBoard && !InBounds(eye, boardSize))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .EffectiveEyeOutOfBounds,
                        "effectiveEyeAnchorCellAfterPressure",
                        "The effective eye anchor must be within the board.");
                }
                if (exactDomain && !domainSet.Contains(eye))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .EffectiveEyeOutsideDomain,
                        "effectiveEyeAnchorCellAfterPressure",
                        "The effective eye anchor must belong to the domain.");
                }
            }
        }

        private static void ValidateConnections(
            AuthoredLayoutPressureSourceInput source,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            IReadOnlyList<LayoutCellConnection> values =
                source.PreservedStructuralConnectionsAfterPressure;
            if (values == null)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.ConnectionsMissing,
                    "preservedStructuralConnectionsAfterPressure",
                    "The connection collection is missing.");
                return;
            }
            HashSet<LayoutCellCoordinate> usable = new HashSet<LayoutCellCoordinate>(
                source.UsableCellsAfterPressure == null
                    ? Enumerable.Empty<LayoutCellCoordinate>()
                    : source.UsableCellsAfterPressure.Where(value => value != null));
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                LayoutCellConnection value = values[index];
                string path = "preservedStructuralConnectionsAfterPressure[" +
                    index.ToString(CultureInfo.InvariantCulture) + "]";
                if (value == null)
                {
                    AddUnknown(issues,
                        AuthoredLayoutPressureSourceValidationCodes.ConnectionNull,
                        path, "A supplied connection row is missing.");
                    continue;
                }
                if (value.CellA == null || value.CellB == null)
                {
                    AddUnknown(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .ConnectionEndpointMissing,
                        path, "Both connection endpoints are required.");
                    continue;
                }
                if (value.CellA.Equals(value.CellB))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes.ConnectionSelfLoop,
                        path, "Self-loop connections are invalid.");
                }
                if (source.DeclaredBoardSize.HasValue &&
                    source.DeclaredBoardSize.Value > 0 &&
                    (!InBounds(value.CellA, source.DeclaredBoardSize.Value) ||
                     !InBounds(value.CellB, source.DeclaredBoardSize.Value)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .ConnectionEndpointOutOfBounds,
                        path, "Connection endpoints must be within the board.");
                }
                if (source.UsableCellsAfterPressure != null &&
                    (!usable.Contains(value.CellA) || !usable.Contains(value.CellB)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes
                            .ConnectionEndpointNotUsable,
                        path, "Both endpoints must belong to the usable mask.");
                }
                if (!keys.Add(AuthoredLayoutPressureCanonical.ConnectionKey(value)))
                {
                    AddInvalid(issues,
                        AuthoredLayoutPressureSourceValidationCodes.ConnectionDuplicate,
                        path, "Undirected connections must be unique.");
                }
            }
        }

        private static void ValidateClauses(
            IReadOnlyList<LayoutResiliencePredicateClauseKind> values,
            ICollection<AuthoredLayoutPressureSourceIssue> issues)
        {
            if (values == null || values.Count == 0)
            {
                AddUnknown(issues,
                    AuthoredLayoutPressureSourceValidationCodes.ClausesMissing,
                    "requiredPredicateClauses",
                    "At least one explicit predicate clause is required.");
                return;
            }
            if (values.Any(value => !Defined(value)))
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.ClauseUndefined,
                    "requiredPredicateClauses", "All clauses must be named values.");
            }
            if (values.Distinct().Count() != values.Count)
            {
                AddInvalid(issues,
                    AuthoredLayoutPressureSourceValidationCodes.ClauseDuplicate,
                    "requiredPredicateClauses", "Clauses must be unique.");
            }
        }

        private static AuthoredLayoutPressureAssessment Finish(
            IEnumerable<AuthoredLayoutPressureSourceIssue> issues,
            LayoutPressureSnapshot candidate)
        {
            return new AuthoredLayoutPressureAssessment(
                Array.AsReadOnly(issues.OrderBy(value => value.Path,
                        StringComparer.Ordinal)
                    .ThenBy(value => value.Code, StringComparer.Ordinal).ToArray()),
                candidate);
        }

        private static bool InBounds(LayoutCellCoordinate value, int boardSize)
        {
            return value != null && value.X >= 0 && value.Y >= 0 &&
                value.X < boardSize && value.Y < boardSize;
        }

        private static bool Defined(AuthoredLayoutPressureAuthoringCompleteness value)
        {
            return value == AuthoredLayoutPressureAuthoringCompleteness.Complete ||
                value == AuthoredLayoutPressureAuthoringCompleteness.Incomplete;
        }

        private static bool Defined(LayoutPressureKind value)
        {
            return value == LayoutPressureKind.PollutedCellMask ||
                value == LayoutPressureKind.EyeRelocationOrDisruption ||
                value == LayoutPressureKind.StructuralConnectionCut;
        }

        private static bool Defined(LayoutResiliencePredicateClauseKind value)
        {
            return value == LayoutResiliencePredicateClauseKind.CountedLayoutPresent ||
                value == LayoutResiliencePredicateClauseKind
                    .CountedPlacementCellsUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .CountedPlacementCoresUsable ||
                value == LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .EyeToCountedCoreStructurallyConnected;
        }

        private static void AddUnknown(
            ICollection<AuthoredLayoutPressureSourceIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new AuthoredLayoutPressureSourceIssue(code,
                AuthoredLayoutPressureSourceStatus.Unknown, path, message));
        }

        private static void AddInvalid(
            ICollection<AuthoredLayoutPressureSourceIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new AuthoredLayoutPressureSourceIssue(code,
                AuthoredLayoutPressureSourceStatus.Invalid, path, message));
        }
    }
}
