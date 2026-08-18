using System;
using System.Globalization;

namespace TalismanBag.Items.CampaignBaseline
{
    public enum C1FormalItemInteractionMode
    {
        BATTLE_LOCKED = 0,
        PREPARE_ENABLED = 1
    }

    public static class C1FormalItemInteractionAuthorizationDiagnostics
    {
        public const string None = "NONE";
        public const string DuplicateAcceptedNoOp =
            "DUPLICATE_ACCEPTED_NO_OP";
        public const string DefaultLocked = "DEFAULT_LOCKED";
        public const string LifecycleBoundLocked =
            "LIFECYCLE_BOUND_LOCKED";
        public const string LifecycleReboundLocked =
            "LIFECYCLE_REBOUND_LOCKED";
        public const string LifecycleUnboundLocked =
            "LIFECYCLE_UNBOUND_LOCKED";
        public const string RequestNull = "REQUEST_NULL";
        public const string SessionSnapshotMissing =
            "SESSION_SNAPSHOT_MISSING";
        public const string ProductContextMismatch =
            "PRODUCT_CONTEXT_MISMATCH";
        public const string SessionTokenRequired =
            "SESSION_TOKEN_REQUIRED";
        public const string SessionTokenMismatch =
            "SESSION_TOKEN_MISMATCH";
        public const string ResetGenerationInvalid =
            "RESET_GENERATION_INVALID";
        public const string ResetGenerationMismatch =
            "RESET_GENERATION_MISMATCH";
        public const string ExpectedSessionSignatureRequired =
            "EXPECTED_SESSION_SIGNATURE_REQUIRED";
        public const string ExpectedSessionSignatureMismatch =
            "EXPECTED_SESSION_SIGNATURE_MISMATCH";
        public const string ModeUnsupported = "MODE_UNSUPPORTED";
        public const string InteractionLocked = "INTERACTION_LOCKED";
    }

    public sealed class C1FormalItemInteractionAuthorizationRequest
    {
        public const string SchemaId =
            "C1FormalItemInteractionAuthorizationRequest.v1";

        public C1FormalItemInteractionAuthorizationRequest(
            string productContext,
            string sessionToken,
            long resetGeneration,
            string expectedSessionCanonicalSignature,
            C1FormalItemInteractionMode mode)
        {
            this.productContext = productContext ?? string.Empty;
            this.sessionToken = sessionToken ?? string.Empty;
            this.resetGeneration = resetGeneration;
            this.expectedSessionCanonicalSignature =
                expectedSessionCanonicalSignature ?? string.Empty;
            this.mode = mode;
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|",
                new[]
                {
                    SchemaId,
                    this.productContext,
                    this.sessionToken,
                    this.resetGeneration.ToString(
                        CultureInfo.InvariantCulture),
                    this.expectedSessionCanonicalSignature,
                    ((int)this.mode).ToString(CultureInfo.InvariantCulture)
                }));
        }

        public string productContext { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string expectedSessionCanonicalSignature { get; }
        public C1FormalItemInteractionMode mode { get; }
        public string canonicalSignature { get; }

        public static C1FormalItemInteractionAuthorizationRequest FromSnapshot(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemInteractionMode mode)
        {
            return new C1FormalItemInteractionAuthorizationRequest(
                snapshot?.productContext,
                snapshot?.sessionToken,
                snapshot?.resetGeneration ?? 0L,
                snapshot?.canonicalSignature,
                mode);
        }
    }

    public sealed class C1FormalItemInteractionAuthorizationResult
    {
        public const string SchemaId =
            "C1FormalItemInteractionAuthorizationResult.v1";

        private C1FormalItemInteractionAuthorizationResult(
            bool accepted,
            bool changed,
            string diagnostic,
            C1FormalItemInteractionMode resolvedMode,
            string sourceProductContext,
            string sourceSessionToken,
            long sourceResetGeneration,
            string sourceSessionCanonicalSignature)
        {
            this.accepted = accepted;
            this.changed = changed;
            this.diagnostic = diagnostic ?? string.Empty;
            this.resolvedMode = resolvedMode;
            this.sourceProductContext = sourceProductContext ?? string.Empty;
            this.sourceSessionToken = sourceSessionToken ?? string.Empty;
            this.sourceResetGeneration = sourceResetGeneration;
            this.sourceSessionCanonicalSignature =
                sourceSessionCanonicalSignature ?? string.Empty;
            sourceLineage = BuildSourceLineage(
                this.sourceProductContext,
                this.sourceSessionToken,
                this.sourceResetGeneration,
                this.sourceSessionCanonicalSignature);
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|",
                new[]
                {
                    SchemaId,
                    this.accepted ? "accepted" : "rejected",
                    this.changed ? "changed" : "unchanged",
                    this.diagnostic,
                    ((int)this.resolvedMode).ToString(
                        CultureInfo.InvariantCulture),
                    this.sourceLineage
                }));
        }

        public bool accepted { get; }
        public bool changed { get; }
        public string diagnostic { get; }
        public C1FormalItemInteractionMode resolvedMode { get; }
        public string sourceProductContext { get; }
        public string sourceSessionToken { get; }
        public long sourceResetGeneration { get; }
        public string sourceSessionCanonicalSignature { get; }
        public string sourceLineage { get; }
        public string canonicalSignature { get; }
        public bool interactionEnabled =>
            resolvedMode == C1FormalItemInteractionMode.PREPARE_ENABLED;

        public static C1FormalItemInteractionAuthorizationResult InitialLocked()
        {
            return new C1FormalItemInteractionAuthorizationResult(
                false,
                false,
                C1FormalItemInteractionAuthorizationDiagnostics.DefaultLocked,
                C1FormalItemInteractionMode.BATTLE_LOCKED,
                string.Empty,
                string.Empty,
                0L,
                string.Empty);
        }

        internal static C1FormalItemInteractionAuthorizationResult Accepted(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemInteractionMode mode,
            bool changed,
            bool duplicate)
        {
            return FromSnapshot(
                true,
                changed,
                duplicate
                    ? C1FormalItemInteractionAuthorizationDiagnostics
                        .DuplicateAcceptedNoOp
                    : C1FormalItemInteractionAuthorizationDiagnostics.None,
                mode,
                snapshot);
        }

        internal static C1FormalItemInteractionAuthorizationResult
            LockedForLifecycle(
                C1FormalItemSessionSnapshot snapshot,
                string diagnostic,
                C1FormalItemInteractionAuthorizationResult current)
        {
            string nextLineage = BuildSourceLineage(snapshot);
            bool changed = current != null
                           && (current.resolvedMode
                               != C1FormalItemInteractionMode.BATTLE_LOCKED
                               || !string.Equals(
                                   current.sourceLineage,
                                   nextLineage,
                                   StringComparison.Ordinal));
            return FromSnapshot(
                true,
                changed,
                diagnostic,
                C1FormalItemInteractionMode.BATTLE_LOCKED,
                snapshot);
        }

        internal static C1FormalItemInteractionAuthorizationResult
            RejectedLocked(
                string diagnostic,
                C1FormalItemSessionSnapshot snapshot,
                C1FormalItemInteractionAuthorizationResult current)
        {
            string nextLineage = BuildSourceLineage(snapshot);
            bool changed = current != null
                           && (current.resolvedMode
                               != C1FormalItemInteractionMode.BATTLE_LOCKED
                               || !string.Equals(
                                   current.sourceLineage,
                                   nextLineage,
                                   StringComparison.Ordinal));
            return FromSnapshot(
                false,
                changed,
                diagnostic,
                C1FormalItemInteractionMode.BATTLE_LOCKED,
                snapshot);
        }

        private static C1FormalItemInteractionAuthorizationResult FromSnapshot(
            bool accepted,
            bool changed,
            string diagnostic,
            C1FormalItemInteractionMode mode,
            C1FormalItemSessionSnapshot snapshot)
        {
            return new C1FormalItemInteractionAuthorizationResult(
                accepted,
                changed,
                diagnostic,
                mode,
                snapshot?.productContext,
                snapshot?.sessionToken,
                snapshot?.resetGeneration ?? 0L,
                snapshot?.canonicalSignature);
        }

        private static string BuildSourceLineage(
            C1FormalItemSessionSnapshot snapshot)
        {
            return BuildSourceLineage(
                snapshot?.productContext,
                snapshot?.sessionToken,
                snapshot?.resetGeneration ?? 0L,
                snapshot?.canonicalSignature);
        }

        private static string BuildSourceLineage(
            string productContext,
            string sessionToken,
            long resetGeneration,
            string sessionCanonicalSignature)
        {
            return C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                "C1FormalItemInteractionAuthorizationSource.v1",
                productContext ?? string.Empty,
                sessionToken ?? string.Empty,
                resetGeneration.ToString(CultureInfo.InvariantCulture),
                sessionCanonicalSignature ?? string.Empty
            }));
        }
    }

    internal static class C1FormalItemInteractionAuthorization
    {
        internal static C1FormalItemInteractionAuthorizationResult Evaluate(
            C1FormalItemInteractionAuthorizationRequest request,
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemInteractionAuthorizationResult current)
        {
            C1FormalItemInteractionAuthorizationResult safeCurrent = current
                ?? C1FormalItemInteractionAuthorizationResult.InitialLocked();
            string diagnostic = Validate(request, snapshot);
            if (!string.Equals(
                    diagnostic,
                    C1FormalItemInteractionAuthorizationDiagnostics.None,
                    StringComparison.Ordinal))
                return C1FormalItemInteractionAuthorizationResult
                    .RejectedLocked(diagnostic, snapshot, safeCurrent);

            bool duplicate = safeCurrent.resolvedMode == request.mode
                             && string.Equals(
                                 safeCurrent.sourceProductContext,
                                 snapshot.productContext,
                                 StringComparison.Ordinal)
                             && string.Equals(
                                 safeCurrent.sourceSessionToken,
                                 snapshot.sessionToken,
                                 StringComparison.Ordinal)
                             && safeCurrent.sourceResetGeneration
                             == snapshot.resetGeneration
                             && string.Equals(
                                 safeCurrent.sourceSessionCanonicalSignature,
                                 snapshot.canonicalSignature,
                                 StringComparison.Ordinal);
            return C1FormalItemInteractionAuthorizationResult.Accepted(
                snapshot,
                request.mode,
                !duplicate,
                duplicate);
        }

        private static string Validate(
            C1FormalItemInteractionAuthorizationRequest request,
            C1FormalItemSessionSnapshot snapshot)
        {
            if (request == null)
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .RequestNull;
            if (snapshot == null)
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .SessionSnapshotMissing;
            if (!Enum.IsDefined(
                    typeof(C1FormalItemInteractionMode),
                    request.mode))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ModeUnsupported;
            if (!string.Equals(
                    request.productContext,
                    C1FormalItemSessionContract.ProductContext,
                    StringComparison.Ordinal)
                || !string.Equals(
                    request.productContext,
                    snapshot.productContext,
                    StringComparison.Ordinal))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ProductContextMismatch;
            if (string.IsNullOrWhiteSpace(request.sessionToken))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .SessionTokenRequired;
            if (!string.Equals(
                    request.sessionToken,
                    snapshot.sessionToken,
                    StringComparison.Ordinal))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .SessionTokenMismatch;
            if (request.resetGeneration <= 0L)
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ResetGenerationInvalid;
            if (request.resetGeneration != snapshot.resetGeneration)
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ResetGenerationMismatch;
            if (string.IsNullOrWhiteSpace(
                    request.expectedSessionCanonicalSignature))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ExpectedSessionSignatureRequired;
            if (!string.Equals(
                    request.expectedSessionCanonicalSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal))
                return C1FormalItemInteractionAuthorizationDiagnostics
                    .ExpectedSessionSignatureMismatch;
            return C1FormalItemInteractionAuthorizationDiagnostics.None;
        }
    }
}
