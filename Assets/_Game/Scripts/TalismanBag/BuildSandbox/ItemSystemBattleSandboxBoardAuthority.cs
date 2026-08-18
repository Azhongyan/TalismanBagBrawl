using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public enum ItemSystemBattleSandboxBoardOperationStatus
    {
        Valid = 0,
        Unknown = 1,
        Invalid = 2
    }

    public sealed class ItemSystemBattleSandboxBoardOperationResult
    {
        internal ItemSystemBattleSandboxBoardOperationResult(
            ItemSystemBattleSandboxBoardOperationStatus status,
            bool accepted,
            bool changed,
            string diagnosticCode,
            string chineseMessage,
            ItemSystemSnapshot snapshot,
            IReadOnlyList<ItemShapeCell> occupiedCells)
        {
            Status = status;
            Accepted = accepted;
            Changed = changed;
            DiagnosticCode = diagnosticCode ?? string.Empty;
            ChineseMessage = chineseMessage ?? string.Empty;
            Snapshot = snapshot;
            OccupiedCells = (occupiedCells ?? Array.Empty<ItemShapeCell>()).ToArray();
        }

        public ItemSystemBattleSandboxBoardOperationStatus Status { get; }
        public bool Accepted { get; }
        public bool Changed { get; }
        public string DiagnosticCode { get; }
        public string ChineseMessage { get; }
        public ItemSystemSnapshot Snapshot { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells { get; }
    }

    public enum ItemDevSessionRosterAvailabilityOperation
    {
        Reset = 0,
        AddOnce = 1
    }

    public sealed class ItemDevSessionRosterAvailabilityRequest
    {
        public ItemDevSessionRosterAvailabilityRequest(
            ItemDevSessionRosterAvailabilityOperation operation,
            string approvedDevHostId,
            string productContext,
            string sessionToken,
            int resetGeneration,
            string baseItemId = null,
            IReadOnlyList<string> initialAvailableBaseItemIds = null)
        {
            Operation = operation;
            ApprovedDevHostId = approvedDevHostId?.Trim() ?? string.Empty;
            ProductContext = productContext?.Trim() ?? string.Empty;
            SessionToken = sessionToken?.Trim() ?? string.Empty;
            ResetGeneration = resetGeneration;
            BaseItemId = baseItemId?.Trim() ?? string.Empty;
            InitialAvailableBaseItemIds = Array.AsReadOnly(
                (initialAvailableBaseItemIds ?? Array.Empty<string>())
                .Select(value => value?.Trim() ?? string.Empty)
                .ToArray());
        }

        public ItemDevSessionRosterAvailabilityOperation Operation { get; }
        public string ApprovedDevHostId { get; }
        public string ProductContext { get; }
        public string SessionToken { get; }
        public int ResetGeneration { get; }
        public string BaseItemId { get; }
        public IReadOnlyList<string> InitialAvailableBaseItemIds { get; }
    }

    public sealed class ItemDevSessionRosterAvailabilitySnapshot
    {
        public const string CurrentSchemaId =
            "item-dev-session-roster-availability/v1";

        internal ItemDevSessionRosterAvailabilitySnapshot(
            string approvedDevHostId,
            string productContext,
            string sessionToken,
            int resetGeneration,
            IReadOnlyList<string> initialAvailableBaseItemIds,
            IReadOnlyList<string> availableBaseItemIds)
        {
            SchemaId = CurrentSchemaId;
            ApprovedDevHostId = approvedDevHostId ?? string.Empty;
            ProductContext = productContext ?? string.Empty;
            SessionToken = sessionToken ?? string.Empty;
            ResetGeneration = resetGeneration;
            InitialAvailableBaseItemIds = Array.AsReadOnly(
                Canonicalize(initialAvailableBaseItemIds));
            AvailableBaseItemIds = Array.AsReadOnly(
                Canonicalize(availableBaseItemIds));
            CanonicalSignature = BuildCanonicalSignature(
                SchemaId,
                ApprovedDevHostId,
                ProductContext,
                SessionToken,
                ResetGeneration,
                InitialAvailableBaseItemIds,
                AvailableBaseItemIds);
        }

        public string SchemaId { get; }
        public string ApprovedDevHostId { get; }
        public string ProductContext { get; }
        public string SessionToken { get; }
        public int ResetGeneration { get; }
        public IReadOnlyList<string> InitialAvailableBaseItemIds { get; }
        public IReadOnlyList<string> AvailableBaseItemIds { get; }
        public string CanonicalSignature { get; }
        public bool HasDevSessionScope => ResetGeneration > 0;

        private static string[] Canonicalize(IEnumerable<string> values)
        {
            return (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        private static string BuildCanonicalSignature(
            string schemaId,
            string approvedDevHostId,
            string productContext,
            string sessionToken,
            int resetGeneration,
            IReadOnlyList<string> initialAvailableBaseItemIds,
            IReadOnlyList<string> availableBaseItemIds)
        {
            StringBuilder canonical = new();
            AppendCanonical(canonical, schemaId);
            AppendCanonical(canonical, approvedDevHostId);
            AppendCanonical(canonical, productContext);
            AppendCanonical(canonical, sessionToken);
            canonical.Append(resetGeneration.ToString(CultureInfo.InvariantCulture))
                .Append('|');
            AppendCanonicalList(canonical, initialAvailableBaseItemIds);
            AppendCanonicalList(canonical, availableBaseItemIds);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical.ToString())))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void AppendCanonical(StringBuilder target, string value)
        {
            string safe = value ?? string.Empty;
            target.Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('|');
        }

        private static void AppendCanonicalList(
            StringBuilder target,
            IReadOnlyList<string> values)
        {
            IReadOnlyList<string> safe = values ?? Array.Empty<string>();
            target.Append(safe.Count.ToString(CultureInfo.InvariantCulture))
                .Append('|');
            foreach (string value in safe)
            {
                AppendCanonical(target, value);
            }
        }
    }

    public sealed class ItemDevSessionRosterAvailabilityResult
    {
        internal ItemDevSessionRosterAvailabilityResult(
            bool accepted,
            bool changed,
            string diagnosticCode,
            ItemDevSessionRosterAvailabilitySnapshot snapshot)
        {
            Accepted = accepted;
            Changed = changed;
            DiagnosticCode = diagnosticCode ?? string.Empty;
            Snapshot = snapshot;
        }

        public bool Accepted { get; }
        public bool Changed { get; }
        public string DiagnosticCode { get; }
        public ItemDevSessionRosterAvailabilitySnapshot Snapshot { get; }
    }

    public interface IItemSystemBattleSandboxBoardAuthority
    {
        IReadOnlyList<ItemSystemBattleSandboxViewRow> Rows { get; }
        ItemSystemSnapshot CurrentSnapshot { get; }
        ItemInstancePlacementBindingContractSnapshot CurrentBindingSnapshot { get; }
        ItemInstanceQualifiedBuildStateSnapshot CurrentQualifiedBuildState { get; }
        ItemInstanceCoreEffectRuntimeStateSnapshot CurrentCoreEffectRuntimeState { get; }
        LayoutResilienceBattleSandboxPlaytestSnapshot CurrentLayoutResilienceSnapshot { get; }
        ItemDevSessionRosterAvailabilitySnapshot CurrentDevSessionRosterAvailability { get; }
        int If01ValidationCount { get; }
        int QualifiedBuildAssemblyCount { get; }
        int CoreEffectRuntimeAssemblyCount { get; }
        int P6EvaluationCount { get; }

        ItemSystemBattleSandboxBoardOperationResult PreviewPlacement(
            string baseItemId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation);
        ItemSystemBattleSandboxBoardOperationResult CommitFromTray(
            string baseItemId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation);
        ItemSystemBattleSandboxBoardOperationResult CommitMove(
            string placementId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation);
        ItemSystemBattleSandboxBoardOperationResult ReturnToTray(string placementId);
        ItemSystemBattleSandboxBoardOperationResult Reset();
        ItemDevSessionRosterAvailabilityResult ApplyDevSessionRosterAvailability(
            ItemDevSessionRosterAvailabilityRequest request);
    }

    public sealed class ItemSystemBattleSandboxBoardAuthority :
        IItemSystemBattleSandboxBoardAuthority
    {
        public const string PackageKey =
            "V0.4-ItemSystemBattleSandboxBoardAdapter01";
        public const string SystemItemId = "I031";
        public const string SystemPlacementId = "P_SYSTEM_I031";

        private readonly IItemSystemSnapshotProvider snapshotProvider;
        private readonly IItemInstancePlacementBindingValidator bindingValidator;
        private readonly IItemInstanceQualifiedBuildStateAssembler qualifiedBuildAssembler;
        private readonly ItemCoreEffectIdentityCatalogSnapshot coreEffectIdentityCatalog;
        private readonly ItemCoreEffectCultivationRosterSnapshot cultivationRoster;
        private readonly IItemInstanceCoreEffectRuntimeStateAssembler
            coreEffectRuntimeAssembler;
        private readonly IRealLayoutResilienceEvaluationPipeline p6Pipeline;
        private readonly ItemInstanceProjectionSetSnapshot completeProjectionSet;
        private readonly ItemSystemBattleSandboxViewRow[] rows;
        private readonly Dictionary<string, ItemSystemBattleSandboxViewRow> rowById;
        private List<ItemSystemPlacementInput> committedInputs = new();

        public ItemSystemBattleSandboxBoardAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection,
            IItemSystemSnapshotProvider snapshotProvider,
            IItemInstancePlacementBindingValidator bindingValidator,
            IRealLayoutResilienceEvaluationPipeline p6Pipeline)
            : this(
                projection,
                snapshotProvider,
                bindingValidator,
                ItemInstanceQualifiedBuildStateAssembler.Instance,
                null,
                null,
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance,
                p6Pipeline)
        {
        }

        public ItemSystemBattleSandboxBoardAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection,
            IItemSystemSnapshotProvider snapshotProvider,
            IItemInstancePlacementBindingValidator bindingValidator,
            IItemInstanceQualifiedBuildStateAssembler qualifiedBuildAssembler,
            IRealLayoutResilienceEvaluationPipeline p6Pipeline)
            : this(
                projection,
                snapshotProvider,
                bindingValidator,
                qualifiedBuildAssembler,
                null,
                null,
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance,
                p6Pipeline)
        {
        }

        public ItemSystemBattleSandboxBoardAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection,
            IItemSystemSnapshotProvider snapshotProvider,
            IItemInstancePlacementBindingValidator bindingValidator,
            ItemCoreEffectIdentityCatalogSnapshot coreEffectIdentityCatalog,
            ItemCoreEffectCultivationRosterSnapshot cultivationRoster,
            IRealLayoutResilienceEvaluationPipeline p6Pipeline)
            : this(
                projection,
                snapshotProvider,
                bindingValidator,
                ItemInstanceQualifiedBuildStateAssembler.Instance,
                coreEffectIdentityCatalog,
                cultivationRoster,
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance,
                p6Pipeline)
        {
        }

        public ItemSystemBattleSandboxBoardAuthority(
            ItemSystemBattleSandboxViewProjectionResult projection,
            IItemSystemSnapshotProvider snapshotProvider,
            IItemInstancePlacementBindingValidator bindingValidator,
            IItemInstanceQualifiedBuildStateAssembler qualifiedBuildAssembler,
            ItemCoreEffectIdentityCatalogSnapshot coreEffectIdentityCatalog,
            ItemCoreEffectCultivationRosterSnapshot cultivationRoster,
            IItemInstanceCoreEffectRuntimeStateAssembler
                coreEffectRuntimeAssembler,
            IRealLayoutResilienceEvaluationPipeline p6Pipeline)
        {
            if (projection == null || !projection.IsValid)
            {
                throw new ArgumentException("View projection must be valid.", nameof(projection));
            }
            this.snapshotProvider = snapshotProvider ??
                throw new ArgumentNullException(nameof(snapshotProvider));
            this.bindingValidator = bindingValidator ??
                throw new ArgumentNullException(nameof(bindingValidator));
            this.qualifiedBuildAssembler = qualifiedBuildAssembler ??
                throw new ArgumentNullException(nameof(qualifiedBuildAssembler));
            this.coreEffectIdentityCatalog = coreEffectIdentityCatalog;
            this.cultivationRoster = cultivationRoster;
            this.coreEffectRuntimeAssembler = coreEffectRuntimeAssembler ??
                throw new ArgumentNullException(nameof(coreEffectRuntimeAssembler));
            this.p6Pipeline = p6Pipeline ??
                throw new ArgumentNullException(nameof(p6Pipeline));
            completeProjectionSet = projection.OrdinaryProjectionSet ??
                throw new ArgumentException(
                    "View projection has no complete ordinary ProjectionSet.",
                    nameof(projection));
            rows = projection.Rows.OrderBy(row => row.BaseItemId, StringComparer.Ordinal).ToArray();
            rowById = rows.ToDictionary(row => row.BaseItemId, row => row, StringComparer.Ordinal);
            CurrentDevSessionRosterAvailability =
                new ItemDevSessionRosterAvailabilitySnapshot(
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    rows.Select(row => row.BaseItemId).ToArray(),
                    rows.Select(row => row.BaseItemId).ToArray());

            ItemSystemBattleSandboxBoardOperationResult initial = CommitCandidate(
                CreateResetInputs(), allowNoOp: false);
            if (!initial.Accepted || CurrentSnapshot == null || !CurrentSnapshot.isValid
                || CurrentQualifiedBuildState?.isValid != true
                || CurrentCoreEffectRuntimeState == null
                || CurrentCoreEffectRuntimeState.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                || HasCompleteCoreEffectInputs
                    && !CurrentCoreEffectRuntimeState.isValid)
            {
                throw new InvalidOperationException(
                    "Initial ItemSystem baseline was rejected: " + initial.DiagnosticCode);
            }
        }

        public IReadOnlyList<ItemSystemBattleSandboxViewRow> Rows => rows;
        public ItemSystemSnapshot CurrentSnapshot { get; private set; }
        public ItemInstancePlacementBindingContractSnapshot CurrentBindingSnapshot { get; private set; }
        public ItemInstanceQualifiedBuildStateSnapshot CurrentQualifiedBuildState { get; private set; }
        public ItemInstanceCoreEffectRuntimeStateSnapshot
            CurrentCoreEffectRuntimeState { get; private set; }
        public LayoutResilienceBattleSandboxPlaytestSnapshot CurrentLayoutResilienceSnapshot { get; private set; }
        public ItemDevSessionRosterAvailabilitySnapshot
            CurrentDevSessionRosterAvailability { get; private set; }
        public int If01ValidationCount { get; private set; }
        public int QualifiedBuildAssemblyCount { get; private set; }
        public int CoreEffectRuntimeAssemblyCount { get; private set; }
        public int P6EvaluationCount { get; private set; }

        private bool HasCompleteCoreEffectInputs =>
            coreEffectIdentityCatalog?.isValid == true
            && cultivationRoster?.isValid == true
            && cultivationRoster.rosterCompleteness ==
                ItemCoreEffectRosterCompleteness.Complete;

        public ItemSystemBattleSandboxBoardOperationResult PreviewPlacement(
            string baseItemId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation)
        {
            if (!TryResolveAvailableRow(baseItemId, out ItemSystemBattleSandboxViewRow row,
                    out ItemSystemBattleSandboxBoardOperationResult failure))
            {
                return failure;
            }
            List<ItemSystemPlacementInput> candidate = ReplacePlacement(
                committedInputs, row, anchorCell, rotation);
            ItemSystemSnapshot snapshot = CreateSnapshot(candidate);
            if (snapshot == null || !snapshot.isValid)
            {
                return SnapshotFailure(snapshot);
            }
            ItemSystemPlacementSnapshot placement = snapshot.FindPlacement(
                PlacementIdFor(row.BaseItemId));
            return Success(
                changed: !SameSignature(CurrentSnapshot, snapshot),
                snapshot,
                placement?.OccupiedCells);
        }

        public ItemSystemBattleSandboxBoardOperationResult CommitFromTray(
            string baseItemId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation)
        {
            if (!TryResolveAvailableRow(baseItemId, out ItemSystemBattleSandboxViewRow row,
                    out ItemSystemBattleSandboxBoardOperationResult failure))
            {
                return failure;
            }
            if (HasPlacement(row.BaseItemId))
            {
                return Failure("ITEM_ALREADY_PLACED", "该道具已经在棋盘上。", CurrentSnapshot);
            }
            return CommitCandidate(ReplacePlacement(
                committedInputs, row, anchorCell, rotation), allowNoOp: true);
        }

        public ItemSystemBattleSandboxBoardOperationResult CommitMove(
            string placementId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation)
        {
            if (!TryResolvePlacedRow(placementId, out ItemSystemBattleSandboxViewRow row,
                    out ItemSystemBattleSandboxBoardOperationResult failure))
            {
                return failure;
            }
            if (!HasPlacement(row.BaseItemId))
            {
                return Failure("BOARD_PLACEMENT_MISSING", "棋盘上没有可移动的该道具。", CurrentSnapshot);
            }
            return CommitCandidate(ReplacePlacement(
                committedInputs, row, anchorCell, rotation), allowNoOp: true);
        }

        public ItemSystemBattleSandboxBoardOperationResult ReturnToTray(string placementId)
        {
            if (!TryResolvePlacedRow(placementId, out ItemSystemBattleSandboxViewRow row,
                    out ItemSystemBattleSandboxBoardOperationResult failure))
            {
                return failure;
            }
            if (!HasPlacement(row.BaseItemId))
            {
                return Failure("BOARD_PLACEMENT_MISSING", "棋盘上没有可收回的该道具。", CurrentSnapshot);
            }
            List<ItemSystemPlacementInput> candidate = committedInputs
                .Where(input => !string.Equals(input.itemId, row.BaseItemId,
                    StringComparison.Ordinal))
                .ToList();
            return CommitCandidate(candidate, allowNoOp: true);
        }

        public ItemSystemBattleSandboxBoardOperationResult Reset()
        {
            return CommitCandidate(CreateResetInputs(), allowNoOp: true);
        }

        public ItemDevSessionRosterAvailabilityResult
            ApplyDevSessionRosterAvailability(
                ItemDevSessionRosterAvailabilityRequest request)
        {
            if (request == null)
            {
                return AvailabilityFailure("DEV_ROSTER_REQUEST_MISSING");
            }
            if (!Enum.IsDefined(typeof(ItemDevSessionRosterAvailabilityOperation),
                    request.Operation))
            {
                return AvailabilityFailure("DEV_ROSTER_OPERATION_INVALID");
            }
            if (!string.Equals(
                    request.ApprovedDevHostId,
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                    StringComparison.Ordinal))
            {
                return AvailabilityFailure("DEV_ROSTER_HOST_REJECTED");
            }
            if (!string.Equals(
                    request.ProductContext,
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabHostContext,
                    StringComparison.Ordinal))
            {
                return AvailabilityFailure("DEV_ROSTER_CONTEXT_REJECTED");
            }
            if (string.IsNullOrWhiteSpace(request.SessionToken))
            {
                return AvailabilityFailure("DEV_ROSTER_SESSION_TOKEN_EMPTY");
            }
            if (request.ResetGeneration <= 0)
            {
                return AvailabilityFailure("DEV_ROSTER_GENERATION_INVALID");
            }

            return request.Operation ==
                    ItemDevSessionRosterAvailabilityOperation.Reset
                ? ApplyDevSessionRosterReset(request)
                : ApplyDevSessionRosterAddOnce(request);
        }

        private ItemDevSessionRosterAvailabilityResult
            ApplyDevSessionRosterReset(
                ItemDevSessionRosterAvailabilityRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.BaseItemId))
            {
                return AvailabilityFailure("DEV_ROSTER_RESET_ITEM_ID_FORBIDDEN");
            }

            string[] requested = request.InitialAvailableBaseItemIds.ToArray();
            if (requested.Any(string.IsNullOrWhiteSpace))
            {
                return AvailabilityFailure("DEV_ROSTER_RESET_ITEM_UNKNOWN");
            }
            if (requested.Distinct(StringComparer.Ordinal).Count() != requested.Length)
            {
                return AvailabilityFailure("DEV_ROSTER_RESET_ITEM_DUPLICATE");
            }
            if (requested.Any(baseItemId => !rowById.ContainsKey(baseItemId)))
            {
                return AvailabilityFailure("DEV_ROSTER_RESET_ITEM_UNKNOWN");
            }

            string[] canonicalInitial = requested
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            ItemDevSessionRosterAvailabilitySnapshot current =
                CurrentDevSessionRosterAvailability;
            if (current.HasDevSessionScope)
            {
                if (request.ResetGeneration < current.ResetGeneration)
                {
                    return AvailabilityFailure("DEV_ROSTER_STALE_GENERATION");
                }
                if (request.ResetGeneration == current.ResetGeneration)
                {
                    if (!string.Equals(request.SessionToken, current.SessionToken,
                            StringComparison.Ordinal))
                    {
                        return AvailabilityFailure("DEV_ROSTER_TOKEN_MISMATCH");
                    }
                    if (!canonicalInitial.SequenceEqual(
                            current.InitialAvailableBaseItemIds,
                            StringComparer.Ordinal))
                    {
                        return AvailabilityFailure("DEV_ROSTER_BASELINE_CONFLICT");
                    }
                }
            }

            ItemDevSessionRosterAvailabilitySnapshot candidate =
                new ItemDevSessionRosterAvailabilitySnapshot(
                    request.ApprovedDevHostId,
                    request.ProductContext,
                    request.SessionToken,
                    request.ResetGeneration,
                    canonicalInitial,
                    canonicalInitial);
            HashSet<string> available = new(
                candidate.AvailableBaseItemIds,
                StringComparer.Ordinal);
            List<ItemSystemPlacementInput> candidateInputs = committedInputs
                .Where(input => input != null && available.Contains(input.itemId))
                .Select(CloneInput)
                .ToList();
            ItemSystemBattleSandboxBoardOperationResult boardResult =
                CommitCandidate(candidateInputs, allowNoOp: true);
            if (!boardResult.Accepted)
            {
                return AvailabilityFailure(
                    "DEV_ROSTER_BOARD_REBUILD_" + boardResult.DiagnosticCode);
            }

            bool changed = !string.Equals(
                current.CanonicalSignature,
                candidate.CanonicalSignature,
                StringComparison.Ordinal);
            CurrentDevSessionRosterAvailability = candidate;
            return AvailabilitySuccess(
                changed,
                changed ? "DEV_ROSTER_RESET_APPLIED" : "DEV_ROSTER_RESET_NOOP");
        }

        private ItemDevSessionRosterAvailabilityResult
            ApplyDevSessionRosterAddOnce(
                ItemDevSessionRosterAvailabilityRequest request)
        {
            ItemDevSessionRosterAvailabilitySnapshot current =
                CurrentDevSessionRosterAvailability;
            if (request.InitialAvailableBaseItemIds.Count != 0)
            {
                return AvailabilityFailure("DEV_ROSTER_ADD_BASELINE_FORBIDDEN");
            }
            if (!current.HasDevSessionScope)
            {
                return AvailabilityFailure("DEV_ROSTER_SESSION_NOT_ESTABLISHED");
            }
            if (!string.Equals(request.SessionToken, current.SessionToken,
                    StringComparison.Ordinal))
            {
                return AvailabilityFailure("DEV_ROSTER_TOKEN_MISMATCH");
            }
            if (request.ResetGeneration != current.ResetGeneration)
            {
                return AvailabilityFailure(request.ResetGeneration < current.ResetGeneration
                    ? "DEV_ROSTER_STALE_GENERATION"
                    : "DEV_ROSTER_GENERATION_MISMATCH");
            }
            if (string.IsNullOrWhiteSpace(request.BaseItemId)
                || !rowById.ContainsKey(request.BaseItemId))
            {
                return AvailabilityFailure("DEV_ROSTER_ADD_ITEM_UNKNOWN");
            }
            if (current.AvailableBaseItemIds.Contains(
                    request.BaseItemId,
                    StringComparer.Ordinal))
            {
                return AvailabilitySuccess(false, "DEV_ROSTER_ADD_ONCE_NOOP");
            }

            string[] available = current.AvailableBaseItemIds
                .Append(request.BaseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            CurrentDevSessionRosterAvailability =
                new ItemDevSessionRosterAvailabilitySnapshot(
                    current.ApprovedDevHostId,
                    current.ProductContext,
                    current.SessionToken,
                    current.ResetGeneration,
                    current.InitialAvailableBaseItemIds,
                    available);
            return AvailabilitySuccess(true, "DEV_ROSTER_ADD_ONCE_APPLIED");
        }

        private ItemDevSessionRosterAvailabilityResult AvailabilitySuccess(
            bool changed,
            string diagnosticCode)
        {
            return new ItemDevSessionRosterAvailabilityResult(
                true,
                changed,
                diagnosticCode,
                CurrentDevSessionRosterAvailability);
        }

        private ItemDevSessionRosterAvailabilityResult AvailabilityFailure(
            string diagnosticCode)
        {
            return new ItemDevSessionRosterAvailabilityResult(
                false,
                false,
                diagnosticCode,
                CurrentDevSessionRosterAvailability);
        }

        private ItemSystemBattleSandboxBoardOperationResult CommitCandidate(
            IReadOnlyList<ItemSystemPlacementInput> candidateInputs,
            bool allowNoOp)
        {
            ItemSystemSnapshot candidate = CreateSnapshot(candidateInputs);
            if (candidate == null || !candidate.isValid)
            {
                return SnapshotFailure(candidate);
            }

            if (allowNoOp && SameSignature(CurrentSnapshot, candidate))
            {
                return Success(false, CurrentSnapshot,
                    Array.Empty<Vector2Int>());
            }

            ItemInstancePlacementBindingValidationResult binding;
            try
            {
                ItemSystemPlacementSnapshot[] ordinaryPlacements = candidate.placements
                    .Where(placement => placement != null
                        && !string.Equals(placement.itemId, SystemItemId,
                            StringComparison.Ordinal))
                    .ToArray();
                ItemInstanceProjectionContractSnapshot[] projections = ordinaryPlacements
                    .Select(placement => rowById.TryGetValue(placement.itemId,
                        out ItemSystemBattleSandboxViewRow row)
                        ? row.Projection
                        : null)
                    .Where(value => value != null)
                    .ToArray();
                ItemInstancePlacementBindingInput[] bindings = ordinaryPlacements
                    .Select(placement => new ItemInstancePlacementBindingInput(
                        rowById.TryGetValue(placement.itemId,
                            out ItemSystemBattleSandboxViewRow row)
                            ? row.ItemInstanceId
                            : string.Empty,
                        placement.placementId,
                        placement.itemId))
                    .ToArray();
                ItemInstanceProjectionSetSnapshot projectionSet = new(
                    projections,
                    Array.Empty<ItemInstanceProjectionValidationError>());
                If01ValidationCount++;
                binding = bindingValidator.Validate(projectionSet, candidate, bindings);
            }
            catch (Exception exception)
            {
                return Failure("IF01_EXCEPTION_" + exception.GetType().Name,
                    "道具身份校验未完成。", CurrentSnapshot,
                    ItemSystemBattleSandboxBoardOperationStatus.Unknown);
            }

            if (binding == null
                || binding.status != ItemInstancePlacementBindingStatus.Valid
                || !binding.isValid)
            {
                string code = binding?.primaryValidationCode ?? "IF01_RESULT_MISSING";
                return Failure(code, "道具身份校验未通过。", CurrentSnapshot,
                    binding?.status == ItemInstancePlacementBindingStatus.Unknown
                        ? ItemSystemBattleSandboxBoardOperationStatus.Unknown
                        : ItemSystemBattleSandboxBoardOperationStatus.Invalid);
            }

            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildState;
            try
            {
                QualifiedBuildAssemblyCount++;
                qualifiedBuildState = qualifiedBuildAssembler.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        completeProjectionSet,
                        binding.snapshot,
                        candidate,
                        QualifiedBuildRosterCompleteness.CompleteOwnedRoster));
            }
            catch (Exception exception)
            {
                return Failure(
                    "QUALIFIED_BUILD_EXCEPTION_" + exception.GetType().Name,
                    "实例限定 Build 状态未完成。",
                    CurrentSnapshot,
                    ItemSystemBattleSandboxBoardOperationStatus.Unknown);
            }

            if (qualifiedBuildState == null || !qualifiedBuildState.isValid)
            {
                ItemInstanceQualifiedBuildValidationError error =
                    qualifiedBuildState?.ValidationErrors?.FirstOrDefault();
                return Failure(
                    error?.code ?? "QUALIFIED_BUILD_RESULT_MISSING",
                    "实例限定 Build 状态未通过。",
                    CurrentSnapshot,
                    qualifiedBuildState?.status ==
                        ItemInstanceQualifiedBuildStateStatus.Unknown
                        ? ItemSystemBattleSandboxBoardOperationStatus.Unknown
                        : ItemSystemBattleSandboxBoardOperationStatus.Invalid);
            }

            ItemInstanceCoreEffectRuntimeStateSnapshot coreEffectRuntimeState;
            try
            {
                CoreEffectRuntimeAssemblyCount++;
                coreEffectRuntimeState = coreEffectRuntimeAssembler.Assemble(
                    new ItemInstanceCoreEffectRuntimeStateInput(
                        coreEffectIdentityCatalog,
                        cultivationRoster,
                        completeProjectionSet,
                        binding.snapshot,
                        candidate,
                        HasCompleteCoreEffectInputs
                            ? ItemCoreEffectRosterCompleteness.Complete
                            : ItemCoreEffectRosterCompleteness.Unknown));
            }
            catch (Exception exception)
            {
                return Failure(
                    "CORE_EFFECT_RUNTIME_EXCEPTION_"
                    + exception.GetType().Name,
                    "实例核心效果运行状态未完成。",
                    CurrentSnapshot,
                    ItemSystemBattleSandboxBoardOperationStatus.Unknown);
            }

            if (coreEffectRuntimeState == null
                || coreEffectRuntimeState.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                || HasCompleteCoreEffectInputs
                    && !coreEffectRuntimeState.isValid)
            {
                ItemCoreEffectRuntimeStateValidationError error =
                    coreEffectRuntimeState?.ValidationErrors?.FirstOrDefault();
                return Failure(
                    error?.code ?? "CORE_EFFECT_RUNTIME_RESULT_MISSING",
                    "实例核心效果运行状态未通过。",
                    CurrentSnapshot,
                    coreEffectRuntimeState?.status ==
                        ItemCoreEffectRuntimeStateStatus.Unknown
                        ? ItemSystemBattleSandboxBoardOperationStatus.Unknown
                        : ItemSystemBattleSandboxBoardOperationStatus.Invalid);
            }

            LayoutResilienceBattleSandboxPlaytestSnapshot p6Feedback;
            try
            {
                string itemSignature = LayoutResilienceBattleSandboxPlaytestAdapter
                    .BuildItemSnapshotSignature(candidate);
                P6EvaluationCount++;
                RealLayoutResilienceEvaluationPipelineResult p6 = p6Pipeline.Evaluate(
                    new RealLayoutResilienceEvaluationPipelineInput(
                        PackageKey + "|" + itemSignature,
                        candidate,
                        binding.snapshot));
                p6Feedback = LayoutResilienceBattleSandboxPlaytestFeedback.FromAuthorities(
                    itemSignature,
                    binding,
                    p6,
                    Array.Empty<LayoutResilienceBattleSandboxPlaytestIssue>());
            }
            catch (Exception exception)
            {
                return Failure("P6_EXCEPTION_" + exception.GetType().Name,
                    "阵势韧性诊断未完成。", CurrentSnapshot,
                    ItemSystemBattleSandboxBoardOperationStatus.Unknown);
            }

            if (p6Feedback == null
                || p6Feedback.status !=
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete)
            {
                return Failure(
                    p6Feedback == null
                        ? "P6_RESULT_MISSING"
                        : "P6_RESULT_" + p6Feedback.status.ToString().ToUpperInvariant(),
                    "阵势韧性诊断未通过。",
                    CurrentSnapshot,
                    p6Feedback?.status ==
                        LayoutResilienceBattleSandboxPlaytestStatus.Unknown
                        ? ItemSystemBattleSandboxBoardOperationStatus.Unknown
                        : ItemSystemBattleSandboxBoardOperationStatus.Invalid);
            }

            committedInputs = CloneInputs(candidateInputs);
            CurrentSnapshot = candidate;
            CurrentBindingSnapshot = binding.snapshot;
            CurrentQualifiedBuildState = qualifiedBuildState;
            CurrentCoreEffectRuntimeState = coreEffectRuntimeState;
            CurrentLayoutResilienceSnapshot = p6Feedback;
            return Success(true, candidate, Array.Empty<Vector2Int>());
        }

        private ItemSystemSnapshot CreateSnapshot(
            IReadOnlyList<ItemSystemPlacementInput> inputs)
        {
            I031InventoryPlacementStateInput i031State =
                ResolveI031StateInput(inputs);
            return snapshotProvider.CreateSnapshot(new ItemSystemSnapshotInput(
                inputs,
                ItemSystemBoardConfigInput.Default(),
                awakeningInputs: BuildAwakeningInputs(inputs),
                catalogItems: ItemInnerDataCatalog.AllItems,
                i031StateInputs: new[] { i031State }));
        }

        private IReadOnlyList<ItemCoreAwakeningInput> BuildAwakeningInputs(
            IReadOnlyList<ItemSystemPlacementInput> inputs)
        {
            if (!HasCompleteCoreEffectInputs)
            {
                return Array.Empty<ItemCoreAwakeningInput>();
            }
            List<ItemCoreAwakeningInput> result = new();
            foreach (ItemSystemPlacementInput input in
                     (inputs ?? Array.Empty<ItemSystemPlacementInput>())
                     .Where(value => value != null
                         && !string.Equals(value.itemId, SystemItemId,
                             StringComparison.Ordinal))
                     .OrderBy(value => value.placementId,
                         StringComparer.Ordinal))
            {
                if (!rowById.TryGetValue(input.itemId,
                        out ItemSystemBattleSandboxViewRow row)
                    || row == null)
                {
                    continue;
                }
                ItemCoreEffectCultivationRow cultivation =
                    cultivationRoster.Find(row.ItemInstanceId);
                if (cultivation?.factCompleteness !=
                        ItemCoreEffectFactCompleteness.Complete
                    || !cultivation.inputLevel.HasValue)
                {
                    continue;
                }
                result.Add(new ItemCoreAwakeningInput(
                    input.itemId,
                    input.placementId,
                    cultivation.inputLevel.Value,
                    false,
                    cultivation.sourceKey));
            }
            return result;
        }

        private List<ItemSystemPlacementInput> CreateResetInputs()
        {
            return new List<ItemSystemPlacementInput>();
        }

        private static I031InventoryPlacementStateInput ResolveI031StateInput(
            IReadOnlyList<ItemSystemPlacementInput> inputs)
        {
            ItemSystemPlacementInput[] i031Placements = (inputs ??
                    Array.Empty<ItemSystemPlacementInput>())
                .Where(input => input != null && string.Equals(
                    input.itemId, SystemItemId, StringComparison.Ordinal))
                .ToArray();
            return i031Placements.Length == 1
                && string.Equals(i031Placements[0].placementId,
                    SystemPlacementId, StringComparison.Ordinal)
                ? I031InventoryPlacementContract.OwnedBoard()
                : I031InventoryPlacementContract.OwnedInventory();
        }

        private List<ItemSystemPlacementInput> ReplacePlacement(
            IReadOnlyList<ItemSystemPlacementInput> source,
            ItemSystemBattleSandboxViewRow row,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation)
        {
            List<ItemSystemPlacementInput> candidate = (source ??
                    Array.Empty<ItemSystemPlacementInput>())
                .Where(input => input != null
                    && !string.Equals(input.itemId, row.BaseItemId,
                        StringComparison.Ordinal))
                .Select(CloneInput)
                .ToList();
            candidate.Add(new ItemSystemPlacementInput(
                PlacementIdFor(row.BaseItemId),
                row.BaseItemId,
                new Vector2Int(anchorCell.x, anchorCell.y),
                ItemSystemBattleSandboxViewProjection.RotationToDegrees(rotation)));
            return candidate;
        }

        private bool HasPlacement(string baseItemId)
        {
            return CurrentSnapshot?.placements?.Any(placement => placement != null
                && string.Equals(placement.itemId, baseItemId,
                    StringComparison.Ordinal)) == true;
        }

        private bool TryResolveRow(
            string baseItemId,
            out ItemSystemBattleSandboxViewRow row,
            out ItemSystemBattleSandboxBoardOperationResult failure)
        {
            row = null;
            if (string.IsNullOrWhiteSpace(baseItemId)
                || !rowById.TryGetValue(baseItemId, out row)
                || row == null)
            {
                failure = Failure("BASE_ITEM_UNKNOWN", "找不到对应的道具目录项。",
                    CurrentSnapshot, ItemSystemBattleSandboxBoardOperationStatus.Unknown);
                return false;
            }
            failure = null;
            return true;
        }

        private bool TryResolveAvailableRow(
            string baseItemId,
            out ItemSystemBattleSandboxViewRow row,
            out ItemSystemBattleSandboxBoardOperationResult failure)
        {
            if (!TryResolveRow(baseItemId, out row, out failure))
            {
                return false;
            }
            if (!CurrentDevSessionRosterAvailability.AvailableBaseItemIds.Contains(
                    baseItemId,
                    StringComparer.Ordinal))
            {
                row = null;
                failure = Failure(
                    "BASE_ITEM_UNAVAILABLE",
                    "该道具不在当前开发会话可用名册中。",
                    CurrentSnapshot);
                return false;
            }
            return true;
        }

        private bool TryResolvePlacedRow(
            string placementId,
            out ItemSystemBattleSandboxViewRow row,
            out ItemSystemBattleSandboxBoardOperationResult failure)
        {
            row = null;
            ItemSystemPlacementSnapshot[] matches = (CurrentSnapshot?.placements ??
                    Array.Empty<ItemSystemPlacementSnapshot>())
                .Where(value => value != null && string.Equals(
                    value.placementId, placementId, StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1
                || !rowById.TryGetValue(matches[0].itemId, out row)
                || row == null)
            {
                failure = Failure("PLACEMENT_ID_UNKNOWN",
                    "找不到对应的棋盘道具。", CurrentSnapshot,
                    ItemSystemBattleSandboxBoardOperationStatus.Unknown);
                return false;
            }
            failure = null;
            return true;
        }

        private ItemSystemBattleSandboxBoardOperationResult SnapshotFailure(
            ItemSystemSnapshot snapshot)
        {
            ItemSystemValidationError error = snapshot?.validationErrors?.FirstOrDefault();
            string code = error?.code ?? "ITEM_SYSTEM_SNAPSHOT_MISSING";
            return Failure(code, ChinesePlacementFailure(code), snapshot ?? CurrentSnapshot);
        }

        private static string ChinesePlacementFailure(string code)
        {
            return code switch
            {
                "ITEM_OUT_OF_BOUNDS" => "该形状超出棋盘范围。",
                "PLACEMENT_OVERLAP" => "该位置与已有道具重叠。",
                "EYE_CELL_COVERED" => "道具不可覆盖中心阵眼。",
                "JUNIAN_MISSING" => "聚念石基线缺失。",
                _ => "该位置未通过 ItemSystem 校验。"
            };
        }

        private static ItemSystemBattleSandboxBoardOperationResult Success(
            bool changed,
            ItemSystemSnapshot snapshot,
            IEnumerable<Vector2Int> occupiedCells)
        {
            return new ItemSystemBattleSandboxBoardOperationResult(
                ItemSystemBattleSandboxBoardOperationStatus.Valid,
                true,
                changed,
                "NONE",
                string.Empty,
                snapshot,
                (occupiedCells ?? Array.Empty<Vector2Int>())
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .ToArray());
        }

        private static ItemSystemBattleSandboxBoardOperationResult Failure(
            string code,
            string chinese,
            ItemSystemSnapshot snapshot,
            ItemSystemBattleSandboxBoardOperationStatus status =
                ItemSystemBattleSandboxBoardOperationStatus.Invalid)
        {
            return new ItemSystemBattleSandboxBoardOperationResult(
                status,
                false,
                false,
                code,
                chinese,
                snapshot,
                Array.Empty<ItemShapeCell>());
        }

        private static string PlacementIdFor(string baseItemId)
        {
            return string.Equals(baseItemId, SystemItemId, StringComparison.Ordinal)
                ? SystemPlacementId
                : "P_BOARD_" + (baseItemId ?? string.Empty);
        }

        private static bool SameSignature(ItemSystemSnapshot left, ItemSystemSnapshot right)
        {
            return left != null && right != null
                && string.Equals(left.BuildDebugSignature(), right.BuildDebugSignature(),
                    StringComparison.Ordinal);
        }

        private static List<ItemSystemPlacementInput> CloneInputs(
            IReadOnlyList<ItemSystemPlacementInput> source)
        {
            return (source ?? Array.Empty<ItemSystemPlacementInput>())
                .Where(input => input != null)
                .Select(CloneInput)
                .ToList();
        }

        private static ItemSystemPlacementInput CloneInput(
            ItemSystemPlacementInput input)
        {
            return new ItemSystemPlacementInput(
                input.placementId,
                input.itemId,
                input.anchorCell,
                input.rotation);
        }
    }
}
