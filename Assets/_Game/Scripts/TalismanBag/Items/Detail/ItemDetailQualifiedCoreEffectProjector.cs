using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Generation;

namespace TalismanBag.Items.Detail
{
    public enum ItemDetailQualifiedCoreEffectProjectionStatus
    {
        Complete = 0,
        Unknown = 1,
        NotApplicable = 2
    }

    public sealed class ItemDetailQualifiedCoreEffectProjectionResult
    {
        internal ItemDetailQualifiedCoreEffectProjectionResult(
            bool isSuccess,
            string diagnosticCode,
            ItemDetailViewModel viewModel)
        {
            IsSuccess = isSuccess;
            DiagnosticCode = diagnosticCode ?? string.Empty;
            ViewModel = viewModel?.Clone();
        }

        public bool IsSuccess { get; }
        public string DiagnosticCode { get; }
        public ItemDetailViewModel ViewModel { get; }
    }

    public sealed class ItemDetailQualifiedCoreEffectRow
    {
        private readonly ReadOnlyCollection<string> sourceIdentities;

        public ItemDetailQualifiedCoreEffectRow(
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemCoreEffectRuntimeLocation location,
            ItemInstanceRarity rarity,
            ItemCoreAwakeningNodeKind nodeKind,
            string candidateDefinitionId,
            string awakeningNodeId,
            string displayName,
            string description,
            int requiredLevel,
            ItemInstanceRarity requiredRarity,
            ItemCoreEffectBooleanFact eligibleFact,
            ItemCoreEffectBooleanFact visibleFact,
            ItemCoreEffectBooleanFact unlockedFact,
            ItemCoreEffectBooleanFact litFact,
            ItemCoreEffectBooleanFact activeFact,
            ItemCoreEffectFactCompleteness stateCompleteness,
            IEnumerable<string> sourceIdentities,
            string sourceRuntimeCanonicalSignature)
        {
            this.baseItemId = Normalize(baseItemId);
            this.itemInstanceId = Normalize(itemInstanceId);
            this.placementId = Normalize(placementId);
            this.location = location;
            this.rarity = rarity;
            this.nodeKind = nodeKind;
            this.candidateDefinitionId = Normalize(candidateDefinitionId);
            this.awakeningNodeId = Normalize(awakeningNodeId);
            this.displayName = displayName ?? string.Empty;
            this.description = description ?? string.Empty;
            this.requiredLevel = requiredLevel;
            this.requiredRarity = requiredRarity;
            this.eligibleFact = eligibleFact;
            this.visibleFact = visibleFact;
            this.unlockedFact = unlockedFact;
            this.litFact = litFact;
            this.activeFact = activeFact;
            this.stateCompleteness = stateCompleteness;
            this.sourceIdentities = Array.AsReadOnly((sourceIdentities ??
                    Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            this.sourceRuntimeCanonicalSignature =
                Normalize(sourceRuntimeCanonicalSignature);
            canonicalSignature = ItemDetailQualifiedCoreEffectCanonical.Sha256(
                BuildCanonicalPayload());
        }

        public string baseItemId { get; }
        public string itemInstanceId { get; }
        public string placementId { get; }
        public ItemCoreEffectRuntimeLocation location { get; }
        public ItemInstanceRarity rarity { get; }
        public ItemCoreAwakeningNodeKind nodeKind { get; }
        public string candidateDefinitionId { get; }
        public string awakeningNodeId { get; }
        public string displayName { get; }
        public string description { get; }
        public int requiredLevel { get; }
        public ItemInstanceRarity requiredRarity { get; }
        public ItemCoreEffectBooleanFact eligibleFact { get; }
        public ItemCoreEffectBooleanFact visibleFact { get; }
        public ItemCoreEffectBooleanFact unlockedFact { get; }
        public ItemCoreEffectBooleanFact litFact { get; }
        public ItemCoreEffectBooleanFact activeFact { get; }
        public ItemCoreEffectFactCompleteness stateCompleteness { get; }
        public IReadOnlyList<string> SourceIdentities => sourceIdentities;
        public string sourceRuntimeCanonicalSignature { get; }
        public string canonicalSignature { get; }

        public ItemDetailQualifiedCoreEffectRow Clone() =>
            new(
                baseItemId,
                itemInstanceId,
                placementId,
                location,
                rarity,
                nodeKind,
                candidateDefinitionId,
                awakeningNodeId,
                displayName,
                description,
                requiredLevel,
                requiredRarity,
                eligibleFact,
                visibleFact,
                unlockedFact,
                litFact,
                activeFact,
                stateCompleteness,
                sourceIdentities,
                sourceRuntimeCanonicalSignature);

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new();
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "baseItemId", baseItemId);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "itemInstanceId", itemInstanceId);
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "placementId", placementId);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "location", location.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "rarity", rarity.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "nodeKind", nodeKind.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "candidateDefinitionId", candidateDefinitionId);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "awakeningNodeId", awakeningNodeId);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "displayName", displayName);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "description", description);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "requiredLevel",
                requiredLevel.ToString(CultureInfo.InvariantCulture));
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "requiredRarity", requiredRarity.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "eligibleFact", eligibleFact.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "visibleFact", visibleFact.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "unlockedFact", unlockedFact.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "litFact", litFact.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "activeFact", activeFact.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "stateCompleteness", stateCompleteness.ToString());
            foreach (string source in sourceIdentities)
            {
                ItemDetailQualifiedCoreEffectCanonical.Append(
                    builder, "sourceIdentity", source);
            }
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "sourceRuntimeCanonicalSignature",
                sourceRuntimeCanonicalSignature);
            return builder.ToString();
        }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed class ItemDetailQualifiedCoreEffectProjection
    {
        private readonly ReadOnlyCollection<ItemDetailQualifiedCoreEffectRow>
            rows;

        public ItemDetailQualifiedCoreEffectProjection(
            ItemDetailQualifiedCoreEffectProjectionStatus status,
            ItemCoreEffectFactCompleteness completeness,
            string itemInstanceId,
            string baseItemId,
            string placementId,
            ItemCoreEffectRuntimeLocation location,
            ItemInstanceRarity? rarity,
            int? cultivationLevel,
            string cultivationLevelSource,
            ItemCoreEffectFactCompleteness cultivationFactCompleteness,
            ItemCoreEffectBooleanFact isLitFact,
            IEnumerable<ItemDetailQualifiedCoreEffectRow> rows,
            string diagnosticCode,
            string safeUnavailableReason,
            string sourceRuntimeCanonicalSignature)
        {
            this.status = status;
            this.completeness = completeness;
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.placementId = Normalize(placementId);
            this.location = location;
            this.rarity = rarity;
            this.cultivationLevel = cultivationLevel;
            this.cultivationLevelSource = Normalize(cultivationLevelSource);
            this.cultivationFactCompleteness = cultivationFactCompleteness;
            this.isLitFact = isLitFact;
            this.rows = Array.AsReadOnly((rows ??
                    Array.Empty<ItemDetailQualifiedCoreEffectRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value =>
                    ItemDetailQualifiedCoreEffectCanonical.NodeRank(
                        value.nodeKind))
                .ThenBy(value => value.candidateDefinitionId,
                    StringComparer.Ordinal)
                .ToArray());
            this.diagnosticCode = Normalize(diagnosticCode);
            this.safeUnavailableReason = safeUnavailableReason ?? string.Empty;
            this.sourceRuntimeCanonicalSignature =
                Normalize(sourceRuntimeCanonicalSignature);
            canonicalSignature = ItemDetailQualifiedCoreEffectCanonical.Sha256(
                BuildCanonicalPayload());
        }

        public ItemDetailQualifiedCoreEffectProjectionStatus status { get; }
        public ItemCoreEffectFactCompleteness completeness { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string placementId { get; }
        public ItemCoreEffectRuntimeLocation location { get; }
        public ItemInstanceRarity? rarity { get; }
        public int? cultivationLevel { get; }
        public string cultivationLevelSource { get; }
        public ItemCoreEffectFactCompleteness cultivationFactCompleteness
        {
            get;
        }
        public ItemCoreEffectBooleanFact isLitFact { get; }
        public IReadOnlyList<ItemDetailQualifiedCoreEffectRow> Rows => rows;
        public string diagnosticCode { get; }
        public string safeUnavailableReason { get; }
        public string sourceRuntimeCanonicalSignature { get; }
        public string canonicalSignature { get; }

        public ItemDetailQualifiedCoreEffectProjection Clone() =>
            new(
                status,
                completeness,
                itemInstanceId,
                baseItemId,
                placementId,
                location,
                rarity,
                cultivationLevel,
                cultivationLevelSource,
                cultivationFactCompleteness,
                isLitFact,
                rows,
                diagnosticCode,
                safeUnavailableReason,
                sourceRuntimeCanonicalSignature);

        public static ItemDetailQualifiedCoreEffectProjection NotApplicable(
            string baseItemId = null,
            string diagnosticCode =
                "DETAIL_QUALIFIED_CORE_NOT_APPLICABLE") =>
            new(
                ItemDetailQualifiedCoreEffectProjectionStatus.NotApplicable,
                ItemCoreEffectFactCompleteness.NotApplicable,
                null,
                baseItemId,
                null,
                ItemCoreEffectRuntimeLocation.Unknown,
                null,
                null,
                null,
                ItemCoreEffectFactCompleteness.NotApplicable,
                ItemCoreEffectBooleanFact.NotApplicable,
                Array.Empty<ItemDetailQualifiedCoreEffectRow>(),
                diagnosticCode,
                string.Empty,
                null);

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new();
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "status", status.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "completeness", completeness.ToString());
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "itemInstanceId", itemInstanceId);
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "baseItemId", baseItemId);
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "placementId", placementId);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "location", location.ToString());
            ItemDetailQualifiedCoreEffectCanonical.AppendNullableEnum(
                builder, "rarity", rarity);
            ItemDetailQualifiedCoreEffectCanonical.AppendNullableInt(
                builder, "cultivationLevel", cultivationLevel);
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "cultivationLevelSource", cultivationLevelSource);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "cultivationFactCompleteness",
                cultivationFactCompleteness.ToString());
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "isLitFact", isLitFact.ToString());
            foreach (ItemDetailQualifiedCoreEffectRow row in rows)
            {
                ItemDetailQualifiedCoreEffectCanonical.Append(
                    builder, "row", row.BuildCanonicalPayload());
            }
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "diagnosticCode", diagnosticCode);
            ItemDetailQualifiedCoreEffectCanonical.Append(
                builder, "safeUnavailableReason", safeUnavailableReason);
            ItemDetailQualifiedCoreEffectCanonical.AppendOptional(
                builder, "sourceRuntimeCanonicalSignature",
                sourceRuntimeCanonicalSignature);
            return builder.ToString();
        }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static class ItemDetailQualifiedCoreEffectProjector
    {
        public const string SafeUnavailableText =
            "当前核心效果状态暂不可用";
        private const string SystemItemId = "I031";

        private static readonly ItemCoreAwakeningNodeKind[] FormalOrder =
        {
            ItemCoreAwakeningNodeKind.Core1,
            ItemCoreAwakeningNodeKind.Core2,
            ItemCoreAwakeningNodeKind.Core3,
            ItemCoreAwakeningNodeKind.Ultimate
        };

        public static ItemDetailQualifiedCoreEffectProjectionResult Project(
            ItemDetailViewModel buildProjectedModel,
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceCoreEffectRuntimeStateSnapshot runtimeSnapshot)
        {
            try
            {
                return ProjectCore(
                    buildProjectedModel,
                    Normalize(baseItemId),
                    Normalize(itemInstanceId),
                    Normalize(placementId),
                    itemSystemSnapshot,
                    runtimeSnapshot);
            }
            catch
            {
                return Failure(
                    "DETAIL_QUALIFIED_CORE_PROJECTION_EXCEPTION");
            }
        }

        private static ItemDetailQualifiedCoreEffectProjectionResult
            ProjectCore(
                ItemDetailViewModel buildProjectedModel,
                string baseItemId,
                string itemInstanceId,
                string placementId,
                ItemSystemSnapshot itemSystemSnapshot,
                ItemInstanceCoreEffectRuntimeStateSnapshot runtimeSnapshot)
        {
            if (buildProjectedModel == null)
            {
                return Failure("DETAIL_CORE_COMPOSED_MODEL_MISSING");
            }
            if (string.IsNullOrEmpty(baseItemId))
            {
                return Failure("DETAIL_CORE_BASE_ITEM_REQUIRED");
            }
            string modelBaseItemId = Normalize(
                string.IsNullOrWhiteSpace(buildProjectedModel.baseItemId)
                    ? buildProjectedModel.itemId
                    : buildProjectedModel.baseItemId);
            if (!string.Equals(
                    modelBaseItemId, baseItemId,
                    StringComparison.Ordinal))
            {
                return Failure("DETAIL_CORE_BASE_ITEM_MISMATCH");
            }
            if (!IsCompleteV2ItemSystemSnapshot(itemSystemSnapshot))
            {
                return Failure("DETAIL_CORE_ITEM_SYSTEM_SNAPSHOT_INVALID");
            }
            if (!IsAcceptedRuntimeSnapshot(
                    itemSystemSnapshot, runtimeSnapshot))
            {
                return Failure("DETAIL_CORE_RUNTIME_SNAPSHOT_INVALID");
            }

            if (string.Equals(
                    baseItemId, SystemItemId,
                    StringComparison.Ordinal))
            {
                return ProjectSystemItem(
                    buildProjectedModel,
                    itemInstanceId,
                    placementId,
                    itemSystemSnapshot,
                    runtimeSnapshot);
            }

            if (string.IsNullOrEmpty(itemInstanceId))
            {
                return Failure("DETAIL_CORE_ITEM_INSTANCE_REQUIRED");
            }
            if (!string.IsNullOrWhiteSpace(
                    buildProjectedModel.itemInstanceId)
                && !string.Equals(
                    buildProjectedModel.itemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    "DETAIL_CORE_COMPOSED_INSTANCE_MISMATCH");
            }

            ItemInstanceCoreEffectRuntimeItemSnapshot[] matches =
                runtimeSnapshot.Items.Where(value => value != null
                    && string.Equals(
                        value.itemInstanceId,
                        itemInstanceId,
                        StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
            {
                return Failure("DETAIL_CORE_RUNTIME_ITEM_NOT_FOUND");
            }
            ItemInstanceCoreEffectRuntimeItemSnapshot item = matches[0];
            if (!string.Equals(
                    item.baseItemId, baseItemId,
                    StringComparison.Ordinal))
            {
                return Failure(
                    "DETAIL_CORE_RUNTIME_BASE_ITEM_MISMATCH");
            }
            if (!ValidateLocation(
                    baseItemId,
                    placementId,
                    itemSystemSnapshot,
                    runtimeSnapshot.status,
                    item))
            {
                return Failure(
                    string.IsNullOrEmpty(placementId)
                        ? "DETAIL_CORE_INVENTORY_IDENTITY_MISMATCH"
                        : "DETAIL_CORE_PLACEMENT_IDENTITY_MISMATCH");
            }
            if (!TryReadFormalRows(
                    item,
                    runtimeSnapshot.canonicalSignature,
                    out List<ItemDetailQualifiedCoreEffectRow> rows))
            {
                return Failure("DETAIL_CORE_FORMAL_ROWS_INVALID");
            }

            if (runtimeSnapshot.status ==
                ItemCoreEffectRuntimeStateStatus.Unknown)
            {
                return ProjectUnknown(
                    buildProjectedModel,
                    item,
                    rows,
                    runtimeSnapshot.canonicalSignature);
            }
            if (!runtimeSnapshot.isValid
                || runtimeSnapshot.rosterCompleteness !=
                    ItemCoreEffectRosterCompleteness.Complete
                || item.stateCompleteness !=
                    ItemCoreEffectFactCompleteness.Complete
                || item.cultivationFactCompleteness !=
                    ItemCoreEffectFactCompleteness.Complete
                || !item.cultivationLevel.HasValue
                || rows.Any(value =>
                    value.stateCompleteness !=
                        ItemCoreEffectFactCompleteness.Complete
                    || !IsKnownBoolean(value.eligibleFact)
                    || !IsKnownBoolean(value.visibleFact)
                    || !IsKnownBoolean(value.unlockedFact)))
            {
                return Failure(
                    "DETAIL_CORE_COMPLETE_STATE_REQUIRED");
            }
            if (item.location ==
                ItemCoreEffectRuntimeLocation.Inventory)
            {
                if (item.isLitFact !=
                        ItemCoreEffectBooleanFact.NotApplicable
                    || rows.Any(value =>
                        value.litFact !=
                            ItemCoreEffectBooleanFact.NotApplicable
                        || value.activeFact !=
                            ItemCoreEffectBooleanFact.NotApplicable))
                {
                    return Failure(
                        "DETAIL_CORE_INVENTORY_FACT_MISMATCH");
                }
            }
            else if (!IsKnownBoolean(item.isLitFact)
                || rows.Any(value =>
                    !IsKnownBoolean(value.litFact)
                    || !IsKnownBoolean(value.activeFact)))
            {
                return Failure("DETAIL_CORE_BOARD_FACT_MISMATCH");
            }

            ItemDetailViewModel projected = buildProjectedModel.Clone();
            projected.itemId = baseItemId;
            projected.baseItemId = baseItemId;
            projected.itemInstanceId = itemInstanceId;
            projected.placementId = placementId ?? string.Empty;
            projected.qualifiedCoreEffect =
                new ItemDetailQualifiedCoreEffectProjection(
                    ItemDetailQualifiedCoreEffectProjectionStatus.Complete,
                    item.stateCompleteness,
                    item.itemInstanceId,
                    item.baseItemId,
                    item.placementId,
                    item.location,
                    item.rarity,
                    item.cultivationLevel,
                    item.cultivationLevelSource,
                    item.cultivationFactCompleteness,
                    item.isLitFact,
                    rows,
                    "NONE",
                    string.Empty,
                    runtimeSnapshot.canonicalSignature);
            if (!ApplyKnownPresentation(projected, rows, item))
            {
                return Failure(
                    "DETAIL_CORE_PLAYER_SECTION_MISSING");
            }
            return Success(projected);
        }

        private static ItemDetailQualifiedCoreEffectProjectionResult
            ProjectSystemItem(
                ItemDetailViewModel model,
                string itemInstanceId,
                string placementId,
                ItemSystemSnapshot itemSystemSnapshot,
                ItemInstanceCoreEffectRuntimeStateSnapshot runtimeSnapshot)
        {
            if (!string.IsNullOrEmpty(itemInstanceId)
                || runtimeSnapshot.Items.Any(value => value != null
                    && string.Equals(
                        value.baseItemId,
                        SystemItemId,
                        StringComparison.Ordinal)))
            {
                return Failure(
                    "DETAIL_CORE_I031_ORDINARY_IDENTITY_FORBIDDEN");
            }
            ItemSystemPlacementSnapshot[] systemPlacements =
                itemSystemSnapshot.placements.Where(value => value != null
                    && string.Equals(
                        value.itemId,
                        SystemItemId,
                        StringComparison.Ordinal)).ToArray();
            if (string.IsNullOrEmpty(placementId))
            {
                if (systemPlacements.Length != 0
                    || itemSystemSnapshot.i031State.location !=
                        I031Location.Inventory)
                {
                    return Failure(
                        "DETAIL_CORE_I031_INVENTORY_IDENTITY_MISMATCH");
                }
            }
            else if (systemPlacements.Length != 1
                || !string.Equals(
                    systemPlacements[0].placementId,
                    placementId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    placementId,
                    I031InventoryPlacementContract.StablePlacementId,
                    StringComparison.Ordinal)
                || itemSystemSnapshot.i031State.location !=
                    I031Location.Board)
            {
                return Failure(
                    "DETAIL_CORE_I031_PLACEMENT_IDENTITY_MISMATCH");
            }

            ItemDetailViewModel projected = model.Clone();
            projected.itemId = SystemItemId;
            projected.baseItemId = SystemItemId;
            projected.itemInstanceId = string.Empty;
            projected.placementId = placementId ?? string.Empty;
            projected.displayCoreEffects =
                new List<ItemDetailTextLine>();
            projected.qualifiedCoreEffect =
                ItemDetailQualifiedCoreEffectProjection.NotApplicable(
                    SystemItemId,
                    "DETAIL_CORE_I031_NOT_APPLICABLE");
            return Success(projected);
        }

        private static ItemDetailQualifiedCoreEffectProjectionResult
            ProjectUnknown(
                ItemDetailViewModel model,
                ItemInstanceCoreEffectRuntimeItemSnapshot item,
                IReadOnlyList<ItemDetailQualifiedCoreEffectRow> rows,
                string sourceCanonicalSignature)
        {
            ItemDetailViewModel projected = model.Clone();
            projected.itemId = item.baseItemId;
            projected.baseItemId = item.baseItemId;
            projected.itemInstanceId = item.itemInstanceId;
            projected.placementId = item.placementId ?? string.Empty;
            projected.qualifiedCoreEffect =
                new ItemDetailQualifiedCoreEffectProjection(
                    ItemDetailQualifiedCoreEffectProjectionStatus.Unknown,
                    ItemCoreEffectFactCompleteness.Unknown,
                    item.itemInstanceId,
                    item.baseItemId,
                    item.placementId,
                    item.location,
                    item.rarity,
                    item.cultivationLevel,
                    item.cultivationLevelSource,
                    item.cultivationFactCompleteness,
                    item.isLitFact,
                    rows,
                    "DETAIL_CORE_STATE_UNKNOWN",
                    SafeUnavailableText,
                    sourceCanonicalSignature);
            projected.displayCoreEffects =
                new List<ItemDetailTextLine>();
            projected.displayAwakeningStatusText =
                SafeUnavailableText;
            ItemDetailSectionViewModel section =
                FindSingleCoreSection(projected);
            if (section == null)
            {
                return Failure(
                    "DETAIL_CORE_PLAYER_SECTION_MISSING");
            }
            section.body = SafeUnavailableText;
            section.coreEffectRowStates =
                new List<ItemDetailCoreEffectRowState>();
            projected.awakeningPreview ??=
                new ItemAwakeningPreview();
            projected.awakeningPreview.validationErrorsText =
                SafeUnavailableText;
            projected.awakeningPreview.readOnlyInputText =
                "Unknown";
            return Success(projected, "DETAIL_CORE_STATE_UNKNOWN");
        }

        private static bool ApplyKnownPresentation(
            ItemDetailViewModel model,
            IReadOnlyList<ItemDetailQualifiedCoreEffectRow> rows,
            ItemInstanceCoreEffectRuntimeItemSnapshot item)
        {
            ItemDetailQualifiedCoreEffectRow[] visible = rows
                .Where(value =>
                    value.visibleFact ==
                    ItemCoreEffectBooleanFact.KnownTrue)
                .OrderBy(value =>
                    ItemDetailQualifiedCoreEffectCanonical.NodeRank(
                        value.nodeKind))
                .ToArray();
            model.displayCoreEffects = visible.Select(value =>
                    new ItemDetailTextLine(
                        value.displayName,
                        value.description,
                        value.candidateDefinitionId))
                .ToList();
            List<ItemDetailCoreEffectRowState> rowStates =
                visible.Select(value =>
                    value.activeFact ==
                        ItemCoreEffectBooleanFact.KnownTrue
                        ? ItemDetailCoreEffectRowState.Active
                        : value.unlockedFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                            ? ItemDetailCoreEffectRowState
                                .UnlockedInactive
                            : ItemDetailCoreEffectRowState.Locked)
                .ToList();
            ItemDetailSectionViewModel section =
                FindSingleCoreSection(model);
            if (section == null)
            {
                return false;
            }
            section.stateKey = "coreEffect";
            section.body = string.Join("\n",
                model.displayCoreEffects.Select(value =>
                    "  " + value.title + "：" + value.body));
            section.keepWhenEmpty = true;
            section.coreEffectRowStates = rowStates;

            ItemDetailQualifiedCoreEffectRow[] unlocked =
                visible.Where(value =>
                    value.unlockedFact ==
                    ItemCoreEffectBooleanFact.KnownTrue).ToArray();
            ItemDetailQualifiedCoreEffectRow[] active =
                visible.Where(value =>
                    value.activeFact ==
                    ItemCoreEffectBooleanFact.KnownTrue).ToArray();
            ItemDetailQualifiedCoreEffectRow[] locked =
                visible.Where(value =>
                    value.unlockedFact ==
                    ItemCoreEffectBooleanFact.KnownFalse).ToArray();
            int currentLevel = unlocked.Length == 0
                ? 0
                : unlocked.Max(value => value.requiredLevel);
            int nextLevel = locked.Length == 0
                ? 0
                : locked.Min(value => value.requiredLevel);

            model.statusFlags ??= new ItemDetailStatusFlags();
            model.statusFlags.coreEffectUnlocked =
                unlocked.Length > 0;
            model.statusFlags.coreEffectActive =
                active.Length > 0;
            model.statusFlags.unlockedCoreEffectCount =
                unlocked.Length;
            model.statusFlags.activeCoreEffectCount =
                active.Length;
            model.statusFlags.currentAwakeningNodeLevel =
                currentLevel;
            model.statusFlags.nextAwakeningNodeLevel =
                nextLevel;

            model.awakeningPreview ??= new ItemAwakeningPreview();
            int level = item.cultivationLevel ?? 0;
            model.awakeningPreview.inputLevel = level;
            model.awakeningPreview.resolvedLevel = level;
            model.awakeningPreview.itemLevel = level;
            model.awakeningPreview.coreEffectUnlocked =
                unlocked.Length > 0;
            model.awakeningPreview.coreEffectActive =
                active.Length > 0;
            model.awakeningPreview.unlockedCoreEffectCount =
                unlocked.Length;
            model.awakeningPreview.activeCoreEffectCount =
                active.Length;
            model.awakeningPreview.currentNodeText =
                FormatNode("当前开窍", currentLevel);
            model.awakeningPreview.nextNodeText =
                FormatNode("下一节点", nextLevel);
            model.awakeningPreview.unlockedNodeLevelsText =
                FormatLevels(unlocked.Select(value =>
                    value.requiredLevel));
            model.awakeningPreview.activeNodeLevelsText =
                FormatLevels(active.Select(value =>
                    value.requiredLevel));
            model.awakeningPreview.unlockedCoreEffectIdsText =
                FormatIds(unlocked.Select(value =>
                    value.awakeningNodeId));
            model.awakeningPreview.activeCoreEffectIdsText =
                FormatIds(active.Select(value =>
                    value.awakeningNodeId));
            model.awakeningPreview.lockedCoreEffectIdsText =
                FormatIds(locked.Select(value =>
                    value.awakeningNodeId));
            model.awakeningPreview.validationErrorsText =
                "None";
            model.awakeningPreview.readOnlyInputText =
                (item.cultivationLevelSource ?? "Unknown")
                + ": cultivationLevel="
                + level.ToString(CultureInfo.InvariantCulture);
            model.displayAwakeningStatusText =
                item.location ==
                    ItemCoreEffectRuntimeLocation.Inventory
                    ? "未入阵，点亮后生效"
                    : active.Length > 0
                        ? "已开窍且可生效"
                        : unlocked.Length > 0
                            ? "已开窍，点亮后生效"
                            : "尚未开窍";
            return true;
        }

        private static ItemDetailSectionViewModel FindSingleCoreSection(
            ItemDetailViewModel model)
        {
            ItemDetailSectionViewModel[] matches =
                (model?.displayPlayerSections ??
                    new List<ItemDetailSectionViewModel>())
                .Where(value => value != null
                    && (string.Equals(
                            value.stateKey,
                            "coreEffect",
                            StringComparison.Ordinal)
                        || string.Equals(
                            value.stateKey,
                            "awakening",
                            StringComparison.Ordinal)))
                .ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static bool TryReadFormalRows(
            ItemInstanceCoreEffectRuntimeItemSnapshot item,
            string sourceCanonicalSignature,
            out List<ItemDetailQualifiedCoreEffectRow> rows)
        {
            rows = new List<ItemDetailQualifiedCoreEffectRow>();
            if (item?.CoreEffectRows == null
                || item.CoreEffectRows.Count != FormalOrder.Length
                || item.CoreEffectRows.Any(value => value == null
                    || value.nodeKind ==
                        ItemCoreAwakeningNodeKind.Core4
                    || !string.Equals(
                        value.baseItemId,
                        item.baseItemId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        value.itemInstanceId,
                        item.itemInstanceId,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(
                        value.candidateDefinitionId)
                    || string.IsNullOrWhiteSpace(
                        value.awakeningNodeId)
                    || string.IsNullOrWhiteSpace(
                        value.displayName)
                    || string.IsNullOrWhiteSpace(
                        value.description)))
            {
                return false;
            }
            foreach (ItemCoreAwakeningNodeKind kind in FormalOrder)
            {
                ItemInstanceCoreEffectRuntimeRow[] matches =
                    item.CoreEffectRows.Where(value =>
                        value.nodeKind == kind).ToArray();
                if (matches.Length != 1)
                {
                    return false;
                }
                ItemInstanceCoreEffectRuntimeRow value =
                    matches[0];
                rows.Add(new ItemDetailQualifiedCoreEffectRow(
                    item.baseItemId,
                    item.itemInstanceId,
                    item.placementId,
                    item.location,
                    item.rarity,
                    value.nodeKind,
                    value.candidateDefinitionId,
                    value.awakeningNodeId,
                    value.displayName,
                    value.description,
                    value.requiredLevel,
                    value.requiredRarity,
                    value.eligibleFact,
                    value.visibleFact,
                    value.unlockedFact,
                    value.litFact,
                    value.activeFact,
                    value.stateCompleteness,
                    value.SourceIdentities,
                    sourceCanonicalSignature));
            }
            return true;
        }

        private static bool ValidateLocation(
            string baseItemId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemCoreEffectRuntimeStateStatus status,
            ItemInstanceCoreEffectRuntimeItemSnapshot item)
        {
            ItemSystemPlacementSnapshot[] basePlacements =
                itemSystemSnapshot.placements.Where(value => value != null
                    && string.Equals(
                        value.itemId,
                        baseItemId,
                        StringComparison.Ordinal)).ToArray();
            if (string.IsNullOrEmpty(placementId))
            {
                return basePlacements.Length == 0
                    && item.placementId == null
                    && (item.location ==
                            ItemCoreEffectRuntimeLocation.Inventory
                        || status ==
                            ItemCoreEffectRuntimeStateStatus.Unknown
                        && item.location ==
                            ItemCoreEffectRuntimeLocation.Unknown);
            }
            return basePlacements.Length == 1
                && string.Equals(
                    basePlacements[0].placementId,
                    placementId,
                    StringComparison.Ordinal)
                && item.location ==
                    ItemCoreEffectRuntimeLocation.Board
                && string.Equals(
                    item.placementId,
                    placementId,
                    StringComparison.Ordinal);
        }

        private static bool IsAcceptedRuntimeSnapshot(
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceCoreEffectRuntimeStateSnapshot snapshot)
        {
            return snapshot != null
                && string.Equals(
                    snapshot.schemaId,
                    ItemInstanceCoreEffectRuntimeStateSnapshot
                        .CurrentSchemaId,
                    StringComparison.Ordinal)
                && snapshot.status !=
                    ItemCoreEffectRuntimeStateStatus.Invalid
                && string.Equals(
                    snapshot.canonicalSignature,
                    ItemDetailQualifiedCoreEffectCanonical.Sha256(
                        snapshot.BuildCanonicalPayload()),
                    StringComparison.Ordinal)
                && string.Equals(
                    snapshot.sourceItemSystemSignature,
                    ItemDetailQualifiedCoreEffectCanonical.Sha256(
                        itemSystemSnapshot.BuildDebugSignature()),
                    StringComparison.Ordinal);
        }

        private static bool IsCompleteV2ItemSystemSnapshot(
            ItemSystemSnapshot snapshot)
        {
            return snapshot != null
                && snapshot.isValid
                && string.Equals(
                    snapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal)
                && snapshot.i031State != null
                && snapshot.i031State.ownershipCompleteness ==
                    I031OwnershipCompleteness.Complete
                && snapshot.i031State.isOwned
                && snapshot.i031State.location !=
                    I031Location.Unknown;
        }

        private static bool IsKnownBoolean(
            ItemCoreEffectBooleanFact fact) =>
            fact == ItemCoreEffectBooleanFact.KnownFalse
            || fact == ItemCoreEffectBooleanFact.KnownTrue;

        private static string FormatNode(
            string label,
            int level) =>
            (label ?? string.Empty) + "："
            + (level > 0 ? "Lv." + level : "None");

        private static string FormatLevels(
            IEnumerable<int> levels)
        {
            int[] values = (levels ?? Array.Empty<int>())
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            return values.Length == 0
                ? "None"
                : string.Join(
                    " / ",
                    values.Select(value => "Lv." + value));
        }

        private static string FormatIds(
            IEnumerable<string> values)
        {
            string[] ids = (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return ids.Length == 0
                ? "None"
                : string.Join(" / ", ids);
        }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ItemDetailQualifiedCoreEffectProjectionResult
            Success(
                ItemDetailViewModel model,
                string diagnosticCode = "NONE") =>
            new(true, diagnosticCode, model);

        private static ItemDetailQualifiedCoreEffectProjectionResult
            Failure(string diagnosticCode) =>
            new(false, diagnosticCode, null);
    }

    internal static class ItemDetailQualifiedCoreEffectCanonical
    {
        public static int NodeRank(
            ItemCoreAwakeningNodeKind nodeKind) =>
            nodeKind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => 0,
                ItemCoreAwakeningNodeKind.Core2 => 1,
                ItemCoreAwakeningNodeKind.Core3 => 2,
                ItemCoreAwakeningNodeKind.Ultimate => 3,
                _ => int.MaxValue
            };

        public static void Append(
            StringBuilder builder,
            string key,
            string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(
                    safeKey.Length.ToString(
                        CultureInfo.InvariantCulture))
                .Append(':').Append(safeKey).Append('=')
                .Append(
                    safeValue.Length.ToString(
                        CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        public static void AppendOptional(
            StringBuilder builder,
            string key,
            string value)
        {
            Append(
                builder,
                key + ".presence",
                value == null ? "Missing" : "Present");
            if (value != null)
            {
                Append(builder, key, value);
            }
        }

        public static void AppendNullableInt(
            StringBuilder builder,
            string key,
            int? value)
        {
            Append(
                builder,
                key + ".presence",
                value.HasValue ? "Present" : "Missing");
            if (value.HasValue)
            {
                Append(
                    builder,
                    key,
                    value.Value.ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        public static void AppendNullableEnum<T>(
            StringBuilder builder,
            string key,
            T? value)
            where T : struct
        {
            Append(
                builder,
                key + ".presence",
                value.HasValue ? "Present" : "Missing");
            if (value.HasValue)
            {
                Append(builder, key, value.Value.ToString());
            }
        }

        public static string Sha256(string payload)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(
                sha.ComputeHash(
                    Encoding.UTF8.GetBytes(
                        payload ?? string.Empty))
                .Select(value => value.ToString(
                    "x2", CultureInfo.InvariantCulture)));
        }
    }
}
