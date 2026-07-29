using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness;
using TalismanBag.EnemySystem.RequirementChannel;
using TalismanBag.Items;
using TalismanBag.Items.Capability;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline
{
    public sealed class DefaultRealLayoutResilienceEvaluationPipeline :
        IRealLayoutResilienceEvaluationPipeline
    {
        public static readonly DefaultRealLayoutResilienceEvaluationPipeline Instance =
            new DefaultRealLayoutResilienceEvaluationPipeline();

        private const string ExpectedOverlaySignature =
            "sha256:a8d40066b7d7cbaecc85e0b427dda5c5aa0ddfb209613c29fea984d3fb52bad1";

        private readonly RealLayoutResilienceEvaluationAuthoritySeam authority;

        public DefaultRealLayoutResilienceEvaluationPipeline()
            : this(RealLayoutResilienceEvaluationAuthoritySeam.Production())
        {
        }

        internal DefaultRealLayoutResilienceEvaluationPipeline(
            RealLayoutResilienceEvaluationAuthoritySeam authority)
        {
            this.authority = authority ??
                RealLayoutResilienceEvaluationAuthoritySeam.Production();
        }

        public RealLayoutResilienceEvaluationPipelineResult Evaluate(
            RealLayoutResilienceEvaluationPipelineInput input)
        {
            RealLayoutResilienceEvaluationPipelineIssue gateIssue =
                ValidateInput(input);
            if (gateIssue != null)
            {
                return Result(
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                    input == null ? string.Empty : input.EvaluationBatchId,
                    false, null, null, string.Empty,
                    false, string.Empty,
                    Array.Empty<RealLayoutResilienceEvaluationRouteRowSnapshot>(),
                    new[] { gateIssue });
            }

            LayoutResilienceRequirementChannelMigrationResult overlay;
            try
            {
                overlay = authority.CreateOverlay();
            }
            catch (Exception)
            {
                return GateFailure(input.EvaluationBatchId,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .OverlayAuthorityException,
                    "Overlay",
                    "The P5-M overlay authority did not complete.",
                    false, string.Empty, false, null, null, string.Empty);
            }

            string overlayFailure = ValidateOverlay(overlay);
            if (overlayFailure != null)
            {
                return GateFailure(input.EvaluationBatchId,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .OverlayResultInvalid,
                    "Overlay",
                    overlayFailure,
                    overlay != null,
                    overlay == null ? string.Empty : overlay.CanonicalSignature,
                    false, null, null, string.Empty);
            }

            LayoutResilienceItemFactProjectionResult build;
            try
            {
                build = authority.ProjectBuild(
                    input.ItemSnapshot, input.BindingSnapshot);
            }
            catch (Exception)
            {
                return GateFailure(input.EvaluationBatchId,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .BuildProjectionAuthorityException,
                    "BuildProjection",
                    "The P1 item fact authority did not complete.",
                    true, overlay.CanonicalSignature,
                    false, null, null, string.Empty);
            }

            string buildFailure = ValidateBuild(build);
            if (buildFailure != null ||
                build.Status == LayoutResilienceItemFactProjectionStatus.Invalid)
            {
                return GateFailure(input.EvaluationBatchId,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .BuildProjectionResultInvalid,
                    "BuildProjection",
                    buildFailure ?? "The P1 item fact result is Invalid.",
                    true, overlay.CanonicalSignature,
                    build != null,
                    build == null
                        ? (LayoutResilienceItemFactProjectionStatus?)null
                        : build.Status,
                    build == null
                        ? (LayoutResilienceInputCompleteness?)null
                        : build.BuildFactsCompleteness,
                    build == null ? string.Empty : build.CanonicalSignature);
            }

            DevEncounterLayoutPressureAuthoringSource sources = null;
            bool sourceAuthorityFailed = false;
            try
            {
                sources = authority.CreatePressureSource();
            }
            catch (Exception)
            {
                sourceAuthorityFailed = true;
            }

            List<RealLayoutResilienceEvaluationPipelineIssue> issues =
                new List<RealLayoutResilienceEvaluationPipelineIssue>();
            List<RealLayoutResilienceEvaluationRouteRowSnapshot> rows =
                new List<RealLayoutResilienceEvaluationRouteRowSnapshot>();
            foreach (LayoutResilienceRequirementChannelMigrationRowSnapshot route in
                overlay.Payload.Rows.OrderBy(value => value.MigrationRouteId,
                    StringComparer.Ordinal))
            {
                DevEncounterLayoutPressureAuthoringSourceRow source =
                    sourceAuthorityFailed
                        ? null
                        : FindSource(sources, route);
                rows.Add(EvaluateRoute(
                    input.EvaluationBatchId,
                    route,
                    source,
                    overlay.Payload.ChannelApplicabilitySnapshot,
                    build,
                    issues));
            }

            RealLayoutResilienceEvaluationPipelineStatus status =
                rows.Any(value => value.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid)
                    ? RealLayoutResilienceEvaluationPipelineStatus.Invalid
                    : rows.Any(value => value.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Unknown)
                        ? RealLayoutResilienceEvaluationPipelineStatus.Unknown
                        : RealLayoutResilienceEvaluationPipelineStatus.Complete;
            return Result(
                status,
                input.EvaluationBatchId,
                true,
                build.Status,
                build.BuildFactsCompleteness,
                build.CanonicalSignature,
                true,
                overlay.CanonicalSignature,
                rows,
                issues);
        }

        private RealLayoutResilienceEvaluationRouteRowSnapshot EvaluateRoute(
            string evaluationBatchId,
            LayoutResilienceRequirementChannelMigrationRowSnapshot route,
            DevEncounterLayoutPressureAuthoringSourceRow source,
            EnemyRequirementChannelApplicabilitySnapshot applicabilitySnapshot,
            LayoutResilienceItemFactProjectionResult build,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            RouteState state = new RouteState(route);
            string routePath = "Rows[" + route.MigrationRouteId + "]";
            if (source == null || !RealLayoutResilienceEvaluationRouteIdentity
                .Matches(route, source))
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .PressureSourceIdentityInvalid,
                    routePath + ".P2",
                    "The P5-A source is missing or does not match the P5-M route tuple.");
            }

            AuthoredLayoutPressureSourceResult p2;
            try
            {
                p2 = authority.ProjectPressure(source.PressureSource);
            }
            catch (Exception)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P2AuthorityException,
                    routePath + ".P2",
                    "The P2 pressure authority did not complete.");
            }
            state.Capture(p2);
            if (!ValidP2(p2, route))
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P2ResultInvalid,
                    routePath + ".P2",
                    "The P2 result is malformed, non-authoritative, or differs from the overlay projection.");
            }

            string evaluationId = evaluationBatchId + "|" + route.MigrationRouteId;
            LayoutResilienceEvaluationInputAssemblerResult p3;
            try
            {
                p3 = authority.Assemble(new LayoutResilienceEvaluationInputAssemblerInput(
                    evaluationId,
                    route.MigrationRouteId,
                    applicabilitySnapshot,
                    build,
                    p2));
            }
            catch (Exception)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P3AuthorityException,
                    routePath + ".P3",
                    "The P3 assembly authority did not complete.");
            }
            state.Capture(p3);
            if (!ValidP3(p3, evaluationId, route))
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P3ResultInvalid,
                    routePath + ".P3",
                    "The P3 result is malformed or does not preserve the route identity.");
            }

            LayoutResilienceStructuralReadinessConsumerResult p4;
            try
            {
                p4 = authority.Consume(
                    new LayoutResilienceStructuralReadinessConsumerInput(p3));
            }
            catch (Exception)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P4AuthorityException,
                    routePath + ".P4",
                    "The P4 structural consumer authority did not complete.");
            }
            state.Capture(p4);

            IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>
                validationIssues;
            try
            {
                validationIssues = authority.ValidateP4(p4);
            }
            catch (Exception)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P4ResultValidatorException,
                    routePath + ".P4",
                    "The P4 Result Validator did not complete.");
            }
            if (validationIssues == null || validationIssues.Count != 0)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P4ResultRejected,
                    routePath + ".P4",
                    "The P4 Result Validator rejected the authority result.");
            }
            state.P4ResultValidated = true;

            if (!ValidP4(p4, evaluationId, route))
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P4ResultInvalid,
                    routePath + ".P4",
                    "The validated P4 result does not preserve the applicable route identity and state.");
            }

            if (p4.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid)
            {
                return FailRoute(state, issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .P4ResultInvalid,
                    routePath + ".P4",
                    "The P4 authority result is Invalid.");
            }
            if (p4.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown)
            {
                state.Status = RealLayoutResilienceEvaluationPipelineStatus.Unknown;
                state.PredicateResult = p4.PredicateResult;
                issues.Add(Issue(
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .RouteEvaluationUnknown,
                    RealLayoutResilienceEvaluationPipelineStatus.Unknown,
                    routePath,
                    "The authoritative structural evaluation remains Unknown."));
                return state.ToSnapshot();
            }

            state.Status = RealLayoutResilienceEvaluationPipelineStatus.Complete;
            state.PredicateResult = p4.PredicateResult;
            return state.ToSnapshot();
        }

        private static RealLayoutResilienceEvaluationRouteRowSnapshot FailRoute(
            RouteState state,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues,
            string code,
            string path,
            string message)
        {
            state.Status = RealLayoutResilienceEvaluationPipelineStatus.Invalid;
            state.PredicateResult = null;
            issues.Add(Issue(code,
                RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                path, message));
            return state.ToSnapshot();
        }

        private static RealLayoutResilienceEvaluationPipelineIssue ValidateInput(
            RealLayoutResilienceEvaluationPipelineInput input)
        {
            if (input == null)
            {
                return Issue(
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .InputMissing,
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                    "Input",
                    "Pipeline input is required.");
            }
            if (!string.Equals(input.SchemaId,
                    RealLayoutResilienceEvaluationPipelineSchema.SchemaId,
                    StringComparison.Ordinal) ||
                input.SchemaVersion !=
                    RealLayoutResilienceEvaluationPipelineSchema.SchemaVersion)
            {
                return Issue(
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .InputSchemaInvalid,
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                    "Input.Schema",
                    "Pipeline input schema must match RealLayoutResilienceEvaluationPipeline.v1 / 1.");
            }
            if (string.IsNullOrWhiteSpace(input.EvaluationBatchId))
            {
                return Issue(
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .EvaluationBatchIdMissing,
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                    "Input.EvaluationBatchId",
                    "EvaluationBatchId must be non-empty.");
            }
            return null;
        }

        private static string ValidateOverlay(
            LayoutResilienceRequirementChannelMigrationResult overlay)
        {
            if (overlay == null)
            {
                return "The P5-M overlay result is missing.";
            }
            if (!string.Equals(overlay.SchemaId,
                    LayoutResilienceRequirementChannelMigrationSchema.SchemaId,
                    StringComparison.Ordinal) ||
                overlay.SchemaVersion !=
                    LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion ||
                overlay.Status !=
                    LayoutResilienceRequirementChannelMigrationStatus.Complete ||
                !overlay.DevOnly || overlay.IsEnabled ||
                !overlay.CoordinateBaselineAccepted || overlay.Activated ||
                overlay.Payload == null || overlay.Issues == null ||
                overlay.Issues.Count != 0 ||
                !string.Equals(overlay.CanonicalSignature,
                    ExpectedOverlaySignature, StringComparison.Ordinal))
            {
                return "The P5-M overlay result failed its frozen schema, status, isolation, payload, issue, or canonical contract.";
            }
            if (overlay.Payload.Rows == null || overlay.Payload.Rows.Count != 4 ||
                overlay.Payload.ChannelApplicabilitySnapshot == null ||
                overlay.Payload.FormalRequirementSourceModified ||
                overlay.Payload.BehaviorChanged)
            {
                return "The P5-M overlay payload must contain exactly four isolated routes and one channel snapshot.";
            }
            LayoutResilienceRequirementChannelMigrationRowSnapshot[] rows =
                overlay.Payload.Rows.OrderBy(value => value == null
                        ? string.Empty
                        : value.MigrationRouteId,
                    StringComparer.Ordinal).ToArray();
            if (rows.Any(value => value == null) ||
                !RealLayoutResilienceEvaluationRouteIdentity.AllMatch(rows))
            {
                return "The P5-M overlay route identities differ from the four frozen routes.";
            }
            return null;
        }

        private static string ValidateBuild(
            LayoutResilienceItemFactProjectionResult build)
        {
            if (build == null)
            {
                return "The P1 item fact result is missing.";
            }
            if (!Enum.IsDefined(typeof(LayoutResilienceItemFactProjectionStatus),
                    build.Status) ||
                !Enum.IsDefined(typeof(LayoutResilienceInputCompleteness),
                    build.BuildFactsCompleteness) ||
                !RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(build.CanonicalSignature))
            {
                return "The P1 item fact result has an undefined status, completeness, or malformed canonical signature.";
            }
            return null;
        }

        private static bool ValidP2(
            AuthoredLayoutPressureSourceResult value,
            LayoutResilienceRequirementChannelMigrationRowSnapshot route)
        {
            return value != null &&
                Enum.IsDefined(typeof(AuthoredLayoutPressureSourceStatus),
                    value.Status) &&
                Enum.IsDefined(typeof(LayoutResilienceInputCompleteness),
                    value.PressureFactsCompleteness) &&
                RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(value.CanonicalSignature) &&
                value.Status == route.P2Status &&
                value.PressureFactsCompleteness == route.P2Completeness &&
                string.Equals(value.CanonicalSignature,
                    route.P2CanonicalSignature, StringComparison.Ordinal);
        }

        private static bool ValidP3(
            LayoutResilienceEvaluationInputAssemblerResult value,
            string evaluationId,
            LayoutResilienceRequirementChannelMigrationRowSnapshot route)
        {
            if (value == null ||
                !Enum.IsDefined(
                    typeof(LayoutResilienceEvaluationInputAssemblerStatus),
                    value.Status) ||
                !RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(value.CanonicalSignature) ||
                value.Status == LayoutResilienceEvaluationInputAssemblerStatus.Invalid ||
                value.EvaluationInput == null)
            {
                return false;
            }
            return string.Equals(value.EvaluationInput.EvaluationId,
                    evaluationId, StringComparison.Ordinal) &&
                value.EvaluationInput.ApplicabilityState == route.ApplicabilityState;
        }

        private static bool ValidP4(
            LayoutResilienceStructuralReadinessConsumerResult value,
            string evaluationId,
            LayoutResilienceRequirementChannelMigrationRowSnapshot route)
        {
            if (value == null ||
                !Enum.IsDefined(
                    typeof(LayoutResilienceStructuralReadinessConsumerStatus),
                    value.Status) ||
                !RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(value.CanonicalSignature))
            {
                return false;
            }
            if (value.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid)
            {
                return value.PredicateResult == null;
            }
            LayoutResiliencePredicateResultSnapshot predicate =
                value.PredicateResult;
            if (predicate == null)
            {
                return value.Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Unknown;
            }
            if (!string.Equals(predicate.EvaluationId, evaluationId,
                    StringComparison.Ordinal) ||
                predicate.ApplicabilityState != route.ApplicabilityState ||
                predicate.ApplicabilityState !=
                    EnemyRequirementApplicabilityState.Applicable ||
                !Enum.IsDefined(typeof(LayoutResiliencePredicateState),
                    predicate.PredicateState) ||
                !RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(predicate.CanonicalSignature))
            {
                return false;
            }
            return value.Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete
                ? predicate.PredicateState ==
                        LayoutResiliencePredicateState.KnownTrue ||
                    predicate.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse
                : predicate.PredicateState ==
                    LayoutResiliencePredicateState.Unknown;
        }

        private static DevEncounterLayoutPressureAuthoringSourceRow FindSource(
            DevEncounterLayoutPressureAuthoringSource source,
            LayoutResilienceRequirementChannelMigrationRowSnapshot route)
        {
            if (source == null || source.Rows == null ||
                !string.Equals(source.SchemaId,
                    DevEncounterLayoutPressureAuthoringSchema.SchemaId,
                    StringComparison.Ordinal) ||
                source.SchemaVersion !=
                    DevEncounterLayoutPressureAuthoringSchema.SchemaVersion ||
                !source.DevOnly || source.IsEnabled)
            {
                return null;
            }
            DevEncounterLayoutPressureAuthoringSourceRow[] matches = source.Rows
                .Where(value => RealLayoutResilienceEvaluationRouteIdentity
                    .Matches(route, value)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static RealLayoutResilienceEvaluationPipelineResult GateFailure(
            string evaluationBatchId,
            string code,
            string path,
            string message,
            bool overlayPresent,
            string overlaySignature,
            bool buildPresent,
            LayoutResilienceItemFactProjectionStatus? buildStatus,
            LayoutResilienceInputCompleteness? completeness,
            string buildSignature)
        {
            return Result(
                RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                evaluationBatchId,
                buildPresent,
                buildStatus,
                completeness,
                buildSignature,
                overlayPresent,
                overlaySignature,
                Array.Empty<RealLayoutResilienceEvaluationRouteRowSnapshot>(),
                new[]
                {
                    Issue(code,
                        RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                        path, message)
                });
        }

        private static RealLayoutResilienceEvaluationPipelineResult Result(
            RealLayoutResilienceEvaluationPipelineStatus status,
            string evaluationBatchId,
            bool buildPresent,
            LayoutResilienceItemFactProjectionStatus? buildStatus,
            LayoutResilienceInputCompleteness? completeness,
            string buildSignature,
            bool overlayPresent,
            string overlaySignature,
            IEnumerable<RealLayoutResilienceEvaluationRouteRowSnapshot> rows,
            IEnumerable<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            RealLayoutResilienceEvaluationPipelineResult draft =
                new RealLayoutResilienceEvaluationPipelineResult(
                    status, evaluationBatchId, buildPresent, buildStatus,
                    completeness, buildSignature, overlayPresent,
                    overlaySignature, rows, issues, string.Empty);
            return new RealLayoutResilienceEvaluationPipelineResult(
                status, evaluationBatchId, buildPresent, buildStatus,
                completeness, buildSignature, overlayPresent,
                overlaySignature, draft.Rows, draft.Issues,
                RealLayoutResilienceEvaluationPipelineCanonical.Create(draft));
        }

        private static RealLayoutResilienceEvaluationPipelineIssue Issue(
            string code,
            RealLayoutResilienceEvaluationPipelineStatus status,
            string path,
            string message)
        {
            return new RealLayoutResilienceEvaluationPipelineIssue(
                code, status, path, message);
        }

        private sealed class RouteState
        {
            private readonly LayoutResilienceRequirementChannelMigrationRowSnapshot
                route;

            public RouteState(
                LayoutResilienceRequirementChannelMigrationRowSnapshot route)
            {
                this.route = route;
                Status = RealLayoutResilienceEvaluationPipelineStatus.Invalid;
            }

            public RealLayoutResilienceEvaluationPipelineStatus Status;
            public bool P2ResultPresent;
            public AuthoredLayoutPressureSourceStatus? P2Status;
            public LayoutResilienceInputCompleteness? P2Completeness;
            public string P2CanonicalSignature = string.Empty;
            public bool P3ResultPresent;
            public LayoutResilienceEvaluationInputAssemblerStatus? P3Status;
            public string P3CanonicalSignature = string.Empty;
            public bool P4ResultPresent;
            public LayoutResilienceStructuralReadinessConsumerStatus? P4Status;
            public string P4CanonicalSignature = string.Empty;
            public bool P4ResultValidated;
            public LayoutResiliencePredicateResultSnapshot PredicateResult;

            public void Capture(AuthoredLayoutPressureSourceResult value)
            {
                P2ResultPresent = value != null;
                P2Status = value == null
                    ? (AuthoredLayoutPressureSourceStatus?)null
                    : value.Status;
                P2Completeness = value == null
                    ? (LayoutResilienceInputCompleteness?)null
                    : value.PressureFactsCompleteness;
                P2CanonicalSignature = value == null
                    ? string.Empty
                    : value.CanonicalSignature;
            }

            public void Capture(LayoutResilienceEvaluationInputAssemblerResult value)
            {
                P3ResultPresent = value != null;
                P3Status = value == null
                    ? (LayoutResilienceEvaluationInputAssemblerStatus?)null
                    : value.Status;
                P3CanonicalSignature = value == null
                    ? string.Empty
                    : value.CanonicalSignature;
            }

            public void Capture(
                LayoutResilienceStructuralReadinessConsumerResult value)
            {
                P4ResultPresent = value != null;
                P4Status = value == null
                    ? (LayoutResilienceStructuralReadinessConsumerStatus?)null
                    : value.Status;
                P4CanonicalSignature = value == null
                    ? string.Empty
                    : value.CanonicalSignature;
            }

            public RealLayoutResilienceEvaluationRouteRowSnapshot ToSnapshot()
            {
                return new RealLayoutResilienceEvaluationRouteRowSnapshot(
                    route.MigrationRouteId,
                    route.CandidateMigrationRequirementId,
                    route.OwnerId,
                    route.RequirementGroupId,
                    route.SeedId,
                    route.EncounterId,
                    route.MapRuleId,
                    route.PressureInputId,
                    route.DeclaredChannel,
                    route.EvaluationChannel,
                    route.ApplicabilityState,
                    route.CoordinateBaselineAccepted,
                    route.Activated,
                    Status,
                    P2ResultPresent,
                    P2Status,
                    P2Completeness,
                    P2CanonicalSignature,
                    P3ResultPresent,
                    P3Status,
                    P3CanonicalSignature,
                    P4ResultPresent,
                    P4Status,
                    P4CanonicalSignature,
                    P4ResultValidated,
                    PredicateResult);
            }
        }
    }

    internal sealed class RealLayoutResilienceEvaluationAuthoritySeam
    {
        public RealLayoutResilienceEvaluationAuthoritySeam(
            Func<LayoutResilienceRequirementChannelMigrationResult> createOverlay,
            Func<ItemSystemSnapshot,
                ItemInstancePlacementBindingContractSnapshot,
                LayoutResilienceItemFactProjectionResult> projectBuild,
            Func<DevEncounterLayoutPressureAuthoringSource> createPressureSource,
            Func<AuthoredLayoutPressureSourceInput,
                AuthoredLayoutPressureSourceResult> projectPressure,
            Func<LayoutResilienceEvaluationInputAssemblerInput,
                LayoutResilienceEvaluationInputAssemblerResult> assemble,
            Func<LayoutResilienceStructuralReadinessConsumerInput,
                LayoutResilienceStructuralReadinessConsumerResult> consume,
            Func<LayoutResilienceStructuralReadinessConsumerResult,
                IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>>
                validateP4)
        {
            CreateOverlay = createOverlay ?? throw new ArgumentNullException(
                nameof(createOverlay));
            ProjectBuild = projectBuild ?? throw new ArgumentNullException(
                nameof(projectBuild));
            CreatePressureSource = createPressureSource ??
                throw new ArgumentNullException(nameof(createPressureSource));
            ProjectPressure = projectPressure ?? throw new ArgumentNullException(
                nameof(projectPressure));
            Assemble = assemble ?? throw new ArgumentNullException(nameof(assemble));
            Consume = consume ?? throw new ArgumentNullException(nameof(consume));
            ValidateP4 = validateP4 ?? throw new ArgumentNullException(
                nameof(validateP4));
        }

        public Func<LayoutResilienceRequirementChannelMigrationResult>
            CreateOverlay { get; }
        public Func<ItemSystemSnapshot,
            ItemInstancePlacementBindingContractSnapshot,
            LayoutResilienceItemFactProjectionResult> ProjectBuild { get; }
        public Func<DevEncounterLayoutPressureAuthoringSource>
            CreatePressureSource { get; }
        public Func<AuthoredLayoutPressureSourceInput,
            AuthoredLayoutPressureSourceResult> ProjectPressure { get; }
        public Func<LayoutResilienceEvaluationInputAssemblerInput,
            LayoutResilienceEvaluationInputAssemblerResult> Assemble { get; }
        public Func<LayoutResilienceStructuralReadinessConsumerInput,
            LayoutResilienceStructuralReadinessConsumerResult> Consume { get; }
        public Func<LayoutResilienceStructuralReadinessConsumerResult,
            IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>>
            ValidateP4 { get; }

        public static RealLayoutResilienceEvaluationAuthoritySeam Production()
        {
            return new RealLayoutResilienceEvaluationAuthoritySeam(
                () => LayoutResilienceRequirementChannelMigrationValidation
                    .ValidateAndCreateOverlay(
                        LayoutResilienceRequirementChannelMigrationCatalog
                            .CreateOverlaySource()),
                (item, binding) =>
                    DefaultLayoutResilienceItemFactProjectionAdapter.Instance
                        .Project(item, binding),
                () => DevEncounterLayoutPressureCatalog.CreateCandidateSource(),
                pressure => DefaultAuthoredLayoutPressureSourceAdapter.Instance
                    .Project(pressure),
                input => DefaultLayoutResilienceEvaluationInputAssembler.Instance
                    .Assemble(input),
                input => DefaultLayoutResilienceStructuralReadinessConsumer.Instance
                    .Consume(input),
                result =>
                    DefaultLayoutResilienceStructuralReadinessConsumerValidator
                        .Instance.ValidateResult(result));
        }
    }

    internal sealed class RealLayoutResilienceEvaluationRouteIdentity
    {
        private RealLayoutResilienceEvaluationRouteIdentity(
            string route,
            string candidate,
            string owner,
            string group,
            string seed,
            string encounter,
            string map,
            string pressure)
        {
            MigrationRouteId = route;
            CandidateMigrationRequirementId = candidate;
            OwnerId = owner;
            RequirementGroupId = group;
            SeedId = seed;
            EncounterId = encounter;
            MapRuleId = map;
            PressureInputId = pressure;
        }

        public string MigrationRouteId { get; }
        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }

        public static readonly RealLayoutResilienceEvaluationRouteIdentity[] All =
        {
            new RealLayoutResilienceEvaluationRouteIdentity(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                "layout_resilience.formation_eye.placement_shape",
                "dev_enemy_formation_eye_problem",
                "dev_enemy_formation_eye_problem.required",
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1"),
            new RealLayoutResilienceEvaluationRouteIdentity(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1",
                "layout_resilience.formation_eye.placement_shape",
                "dev_enemy_formation_eye_problem",
                "dev_enemy_formation_eye_problem.required",
                "dev_seed_4_10_thunder_fire_cross",
                "dev_encounter_4_10_thunder_fire_cross",
                "dev_map_bluestone_crack",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1"),
            new RealLayoutResilienceEvaluationRouteIdentity(
                "route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1",
                "layout_resilience.polluted_tile.placement_shape",
                "dev_enemy_polluted_tile_problem",
                "dev_enemy_polluted_tile_problem.required",
                "dev_seed_3_10_cleanse_corner",
                "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp",
                "pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1"),
            new RealLayoutResilienceEvaluationRouteIdentity(
                "route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                "layout_resilience.polluted_tile.placement_shape",
                "dev_enemy_polluted_tile_problem",
                "dev_enemy_polluted_tile_problem.required",
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1")
        };

        public static bool AllMatch(
            IReadOnlyList<LayoutResilienceRequirementChannelMigrationRowSnapshot>
                rows)
        {
            return rows != null && rows.Count == All.Length &&
                All.OrderBy(value => value.MigrationRouteId,
                        StringComparer.Ordinal)
                    .Zip(rows.OrderBy(value => value.MigrationRouteId,
                            StringComparer.Ordinal),
                        (expected, actual) => expected.Matches(actual))
                    .All(value => value);
        }

        public static bool Matches(
            LayoutResilienceRequirementChannelMigrationRowSnapshot route,
            DevEncounterLayoutPressureAuthoringSourceRow source)
        {
            return route != null && source != null &&
                string.Equals(route.CandidateMigrationRequirementId,
                    source.CandidateMigrationRequirementId,
                    StringComparison.Ordinal) &&
                string.Equals(route.OwnerId, source.OwnerId,
                    StringComparison.Ordinal) &&
                string.Equals(route.RequirementGroupId, source.RequirementGroupId,
                    StringComparison.Ordinal) &&
                string.Equals(route.SeedId, source.SeedId,
                    StringComparison.Ordinal) &&
                string.Equals(route.EncounterId, source.EncounterId,
                    StringComparison.Ordinal) &&
                string.Equals(route.MapRuleId, source.MapRuleId,
                    StringComparison.Ordinal) &&
                string.Equals(route.PressureInputId, source.PressureInputId,
                    StringComparison.Ordinal);
        }

        public bool Matches(
            LayoutResilienceRequirementChannelMigrationRowSnapshot value)
        {
            return value != null &&
                string.Equals(MigrationRouteId, value.MigrationRouteId,
                    StringComparison.Ordinal) &&
                string.Equals(CandidateMigrationRequirementId,
                    value.CandidateMigrationRequirementId,
                    StringComparison.Ordinal) &&
                string.Equals(OwnerId, value.OwnerId, StringComparison.Ordinal) &&
                string.Equals(RequirementGroupId, value.RequirementGroupId,
                    StringComparison.Ordinal) &&
                string.Equals(SeedId, value.SeedId, StringComparison.Ordinal) &&
                string.Equals(EncounterId, value.EncounterId,
                    StringComparison.Ordinal) &&
                string.Equals(MapRuleId, value.MapRuleId,
                    StringComparison.Ordinal) &&
                string.Equals(PressureInputId, value.PressureInputId,
                    StringComparison.Ordinal) &&
                value.DeclaredChannel == EnemyRequirementChannel.StructuralPredicate &&
                value.EvaluationChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                value.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable &&
                value.CoordinateBaselineAccepted && !value.Activated;
        }

        public bool Matches(
            RealLayoutResilienceEvaluationRouteRowSnapshot value)
        {
            return value != null &&
                string.Equals(MigrationRouteId, value.MigrationRouteId,
                    StringComparison.Ordinal) &&
                string.Equals(CandidateMigrationRequirementId,
                    value.CandidateMigrationRequirementId,
                    StringComparison.Ordinal) &&
                string.Equals(OwnerId, value.OwnerId, StringComparison.Ordinal) &&
                string.Equals(RequirementGroupId, value.RequirementGroupId,
                    StringComparison.Ordinal) &&
                string.Equals(SeedId, value.SeedId, StringComparison.Ordinal) &&
                string.Equals(EncounterId, value.EncounterId,
                    StringComparison.Ordinal) &&
                string.Equals(MapRuleId, value.MapRuleId,
                    StringComparison.Ordinal) &&
                string.Equals(PressureInputId, value.PressureInputId,
                    StringComparison.Ordinal) &&
                value.DeclaredChannel == EnemyRequirementChannel.StructuralPredicate &&
                value.EvaluationChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                value.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable &&
                value.CoordinateBaselineAccepted && !value.Activated;
        }
    }
}
