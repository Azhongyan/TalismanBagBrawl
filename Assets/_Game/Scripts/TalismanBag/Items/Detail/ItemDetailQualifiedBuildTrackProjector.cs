using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Generation.Potential;

namespace TalismanBag.Items.Detail
{
    public sealed class ItemDetailQualifiedBuildTrackProjectionResult
    {
        internal ItemDetailQualifiedBuildTrackProjectionResult(
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

    public static class ItemDetailQualifiedBuildTrackProjector
    {
        public const string SafeUnavailableText = "当前Build状态暂不可用";
        private const string SystemItemId = "I031";
        private static readonly int[] FaMenStageRows = { 2, 4, 6 };
        private static readonly int[] QiLeiStageRows = { 2, 4 };

        public static ItemDetailQualifiedBuildTrackProjectionResult Project(
            ItemDetailViewModel composedModel,
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildSnapshot)
        {
            try
            {
                return ProjectCore(
                    composedModel,
                    Normalize(baseItemId),
                    Normalize(itemInstanceId),
                    Normalize(placementId),
                    itemSystemSnapshot,
                    qualifiedBuildSnapshot);
            }
            catch
            {
                return Failure("DETAIL_QUALIFIED_PROJECTION_EXCEPTION");
            }
        }

        private static ItemDetailQualifiedBuildTrackProjectionResult ProjectCore(
            ItemDetailViewModel composedModel,
            string baseItemId,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildSnapshot)
        {
            if (composedModel == null)
            {
                return Failure("DETAIL_COMPOSED_MODEL_MISSING");
            }
            if (string.IsNullOrEmpty(baseItemId))
            {
                return Failure("DETAIL_BASE_ITEM_REQUIRED");
            }
            string modelBaseItemId = Normalize(string.IsNullOrWhiteSpace(composedModel.baseItemId)
                ? composedModel.itemId
                : composedModel.baseItemId);
            if (!string.Equals(modelBaseItemId, baseItemId, StringComparison.Ordinal))
            {
                return Failure("DETAIL_BASE_ITEM_MISMATCH");
            }
            if (!IsCompleteV2ItemSystemSnapshot(itemSystemSnapshot))
            {
                return Failure("DETAIL_ITEM_SYSTEM_SNAPSHOT_INVALID");
            }
            if (!IsAcceptedQualifiedSnapshot(qualifiedBuildSnapshot))
            {
                return Failure("DETAIL_QUALIFIED_SNAPSHOT_INVALID");
            }

            string expectedItemSystemSignature = Sha256(itemSystemSnapshot.BuildDebugSignature());
            if (!string.Equals(
                    qualifiedBuildSnapshot.sourceItemSystemCanonicalSignature,
                    expectedItemSystemSignature,
                    StringComparison.Ordinal))
            {
                return Failure("DETAIL_QUALIFIED_SOURCE_CANONICAL_MISMATCH");
            }

            if (string.Equals(baseItemId, SystemItemId, StringComparison.Ordinal))
            {
                return ProjectSystemItem(
                    composedModel,
                    itemInstanceId,
                    placementId,
                    itemSystemSnapshot,
                    qualifiedBuildSnapshot);
            }

            if (string.IsNullOrEmpty(itemInstanceId))
            {
                return Failure("DETAIL_ITEM_INSTANCE_REQUIRED");
            }
            if (!string.IsNullOrWhiteSpace(composedModel.itemInstanceId)
                && !string.Equals(composedModel.itemInstanceId, itemInstanceId,
                    StringComparison.Ordinal))
            {
                return Failure("DETAIL_COMPOSED_INSTANCE_MISMATCH");
            }

            ItemInstanceQualifiedBuildItemSnapshot item =
                qualifiedBuildSnapshot.FindItemInstance(itemInstanceId);
            if (item == null)
            {
                return Failure("DETAIL_QUALIFIED_ITEM_NOT_FOUND");
            }
            if (!string.Equals(item.baseItemId, baseItemId, StringComparison.Ordinal))
            {
                return Failure("DETAIL_QUALIFIED_BASE_ITEM_MISMATCH");
            }
            if (!ValidateOrdinaryLocation(
                    baseItemId, placementId, itemSystemSnapshot, item,
                    qualifiedBuildSnapshot.status))
            {
                return Failure(string.IsNullOrEmpty(placementId)
                    ? "DETAIL_QUALIFIED_INVENTORY_IDENTITY_MISMATCH"
                    : "DETAIL_QUALIFIED_PLACEMENT_IDENTITY_MISMATCH");
            }

            if (qualifiedBuildSnapshot.status ==
                ItemInstanceQualifiedBuildStateStatus.Unknown)
            {
                return ProjectUnknown(
                    composedModel,
                    item,
                    qualifiedBuildSnapshot.canonicalSignature);
            }
            if (!qualifiedBuildSnapshot.isValid
                || qualifiedBuildSnapshot.rosterCompleteness !=
                    QualifiedBuildRosterCompleteness.CompleteOwnedRoster
                || item.buildFactCompleteness !=
                    ItemInstanceQualifiedBuildFactCompleteness.Complete)
            {
                return Failure("DETAIL_QUALIFIED_COMPLETE_STATE_REQUIRED");
            }

            if (!TryBuildTrackRows(
                    composedModel,
                    item,
                    qualifiedBuildSnapshot,
                    out List<ItemDetailQualifiedBuildTrackRow> trackRows,
                    out string diagnosticCode))
            {
                return Failure(diagnosticCode);
            }

            ItemDetailViewModel projected = composedModel.Clone();
            projected.itemId = baseItemId;
            projected.baseItemId = baseItemId;
            projected.itemInstanceId = itemInstanceId;
            projected.placementId = placementId ?? string.Empty;
            projected.qualifiedBuildTrack = new ItemDetailQualifiedBuildTrackProjection(
                ItemDetailQualifiedBuildProjectionStatus.Complete,
                item.buildFactCompleteness,
                item.itemInstanceId,
                item.baseItemId,
                item.placementId,
                item.location,
                item.buildQualification,
                item.isLitFact,
                item.sourceIsCountedFact,
                item.qualifiedIsCountedFact,
                item.eligibleFaMenBuildId,
                item.eligibleQiLeiBuildId,
                item.faMenBuildCount,
                item.qiLeiBuildCount,
                item.faMenActiveStagePieceCount,
                item.qiLeiActiveStagePieceCount,
                trackRows,
                "NONE",
                string.Empty,
                qualifiedBuildSnapshot.canonicalSignature);
            ApplyPlayerSections(
                projected,
                trackRows,
                item.buildQualification,
                false,
                qualifiedBuildSnapshot);
            AppendDebugSection(projected);
            return Success(projected);
        }

        private static ItemDetailQualifiedBuildTrackProjectionResult ProjectSystemItem(
            ItemDetailViewModel composedModel,
            string itemInstanceId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildSnapshot)
        {
            if (!string.IsNullOrEmpty(itemInstanceId)
                || qualifiedBuildSnapshot.Items.Any(item => item != null
                    && string.Equals(item.baseItemId, SystemItemId,
                        StringComparison.Ordinal)))
            {
                return Failure("DETAIL_I031_ORDINARY_IDENTITY_FORBIDDEN");
            }

            ItemSystemPlacementSnapshot[] systemPlacements = itemSystemSnapshot.placements
                .Where(value => value != null
                    && string.Equals(value.itemId, SystemItemId,
                        StringComparison.Ordinal))
                .ToArray();
            if (string.IsNullOrEmpty(placementId))
            {
                if (systemPlacements.Length != 0
                    || itemSystemSnapshot.i031State.location != I031Location.Inventory)
                {
                    return Failure("DETAIL_I031_INVENTORY_IDENTITY_MISMATCH");
                }
            }
            else if (systemPlacements.Length != 1
                || !string.Equals(systemPlacements[0].placementId, placementId,
                    StringComparison.Ordinal)
                || !string.Equals(placementId,
                    I031InventoryPlacementContract.StablePlacementId,
                    StringComparison.Ordinal)
                || itemSystemSnapshot.i031State.location != I031Location.Board)
            {
                return Failure("DETAIL_I031_PLACEMENT_IDENTITY_MISMATCH");
            }

            ItemDetailViewModel projected = composedModel.Clone();
            projected.itemId = SystemItemId;
            projected.baseItemId = SystemItemId;
            projected.itemInstanceId = string.Empty;
            projected.placementId = placementId ?? string.Empty;
            projected.qualifiedBuildTrack = ItemDetailQualifiedBuildTrackProjection.NotApplicable(
                SystemItemId,
                "DETAIL_I031_QUALIFIED_BUILD_NOT_APPLICABLE");
            HideBuildPlayerSections(projected);
            return Success(projected);
        }

        private static ItemDetailQualifiedBuildTrackProjectionResult ProjectUnknown(
            ItemDetailViewModel composedModel,
            ItemInstanceQualifiedBuildItemSnapshot item,
            string sourceCanonicalSignature)
        {
            ItemDetailViewModel projected = composedModel.Clone();
            projected.itemId = item.baseItemId;
            projected.baseItemId = item.baseItemId;
            projected.itemInstanceId = item.itemInstanceId;
            projected.placementId = item.placementId ?? string.Empty;
            projected.qualifiedBuildTrack = new ItemDetailQualifiedBuildTrackProjection(
                ItemDetailQualifiedBuildProjectionStatus.Unknown,
                ItemInstanceQualifiedBuildFactCompleteness.Unknown,
                item.itemInstanceId,
                item.baseItemId,
                item.placementId,
                item.location,
                item.buildQualification,
                item.isLitFact,
                item.sourceIsCountedFact,
                item.qualifiedIsCountedFact,
                item.eligibleFaMenBuildId,
                item.eligibleQiLeiBuildId,
                null,
                null,
                null,
                null,
                Array.Empty<ItemDetailQualifiedBuildTrackRow>(),
                "DETAIL_QUALIFIED_STATE_UNKNOWN",
                SafeUnavailableText,
                sourceCanonicalSignature);
            ApplyPlayerSections(
                projected,
                Array.Empty<ItemDetailQualifiedBuildTrackRow>(),
                item.buildQualification,
                true,
                null);
            AppendDebugSection(projected);
            return Success(projected, "DETAIL_QUALIFIED_STATE_UNKNOWN");
        }

        private static bool TryBuildTrackRows(
            ItemDetailViewModel composedModel,
            ItemInstanceQualifiedBuildItemSnapshot item,
            ItemInstanceQualifiedBuildStateSnapshot snapshot,
            out List<ItemDetailQualifiedBuildTrackRow> rows,
            out string diagnosticCode)
        {
            rows = new List<ItemDetailQualifiedBuildTrackRow>();
            diagnosticCode = "NONE";
            if (!TryAddTrack(
                    rows,
                    item.eligibleFaMenBuildId,
                    ItemBuildTrackKind.FaMen,
                    item.faMenBuildCount,
                    item.faMenActiveStagePieceCount,
                    snapshot.FindFaMenTrack(item.eligibleFaMenBuildId),
                    composedModel.displayFaMenBuilds,
                    FaMenStageRows,
                    out diagnosticCode))
            {
                return false;
            }
            if (!TryAddTrack(
                    rows,
                    item.eligibleQiLeiBuildId,
                    ItemBuildTrackKind.QiLei,
                    item.qiLeiBuildCount,
                    item.qiLeiActiveStagePieceCount,
                    snapshot.FindQiLeiTrack(item.eligibleQiLeiBuildId),
                    composedModel.displayQiLeiBuilds,
                    QiLeiStageRows,
                    out diagnosticCode))
            {
                return false;
            }
            return true;
        }

        private static bool TryAddTrack(
            ICollection<ItemDetailQualifiedBuildTrackRow> rows,
            string eligibleBuildId,
            ItemBuildTrackKind expectedKind,
            int? itemCount,
            int? itemActiveStage,
            ItemInstanceQualifiedBuildTrackSnapshot source,
            IReadOnlyList<ItemDetailBuildPreview> candidateStages,
            IReadOnlyList<int> presentationStages,
            out string diagnosticCode)
        {
            diagnosticCode = "NONE";
            if (string.IsNullOrWhiteSpace(eligibleBuildId))
            {
                return true;
            }
            if (source == null
                || source.trackKind != expectedKind
                || !string.Equals(source.buildId, eligibleBuildId,
                    StringComparison.Ordinal)
                || !itemCount.HasValue
                || !itemActiveStage.HasValue
                || itemCount.Value != source.qualifiedItemCount
                || itemActiveStage.Value != source.activeStagePieceCount)
            {
                diagnosticCode = "DETAIL_QUALIFIED_TRACK_LINEAGE_MISMATCH";
                return false;
            }
            if (presentationStages.Count == 0
                || source.maxPieceCount != presentationStages[presentationStages.Count - 1]
                || candidateStages == null
                || candidateStages.Count != presentationStages.Count)
            {
                diagnosticCode = "DETAIL_QUALIFIED_TRACK_PRESENTATION_MISMATCH";
                return false;
            }

            List<ItemDetailQualifiedBuildStageRow> stages = new();
            for (int index = 0; index < presentationStages.Count; index++)
            {
                int stagePieceCount = presentationStages[index];
                ItemDetailBuildPreview candidate = candidateStages[index];
                if (candidate == null || string.IsNullOrWhiteSpace(candidate.previewText))
                {
                    diagnosticCode = "DETAIL_QUALIFIED_STAGE_DESCRIPTION_MISSING";
                    return false;
                }
                stages.Add(new ItemDetailQualifiedBuildStageRow(
                    stagePieceCount,
                    candidate.previewText,
                    source.activeStagePieceCount >= stagePieceCount));
            }
            rows.Add(new ItemDetailQualifiedBuildTrackRow(
                source.buildId,
                source.trackKind,
                source.stableTag,
                source.displayName,
                ItemInstanceQualifiedBuildFactCompleteness.Complete,
                source.qualifiedItemCount,
                source.maxPieceCount,
                source.activeStagePieceCount,
                source.nextStagePieceCount,
                stages));
            return true;
        }

        private static void ApplyPlayerSections(
            ItemDetailViewModel model,
            IReadOnlyList<ItemDetailQualifiedBuildTrackRow> rows,
            ItemBuildQualification qualification,
            bool unknown,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildSnapshot)
        {
            if (unknown)
            {
                HideBuildPlayerSections(model);
                return;
            }

            if (qualification == ItemBuildQualification.None)
            {
                HideBuildPlayerSections(model);
                return;
            }

            ItemDetailQualifiedBuildTrackRow faMen = rows.FirstOrDefault(
                row => row.trackKind == ItemBuildTrackKind.FaMen);
            ItemDetailQualifiedBuildTrackRow qiLei = rows.FirstOrDefault(
                row => row.trackKind == ItemBuildTrackKind.QiLei);
            ReplaceSection(model, "famenBuild",
                faMen == null
                    ? string.Empty
                    : BuildTrackBody(
                        faMen,
                        model,
                        qualifiedBuildSnapshot),
                faMen != null);
            ReplaceSection(model, "qileiBuild",
                qiLei == null
                    ? string.Empty
                    : BuildTrackBody(
                        qiLei,
                        model,
                        qualifiedBuildSnapshot),
                qiLei != null);
        }

        private static string BuildTrackBody(
            ItemDetailQualifiedBuildTrackRow row,
            ItemDetailViewModel model,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildSnapshot)
        {
            ItemInstanceQualifiedBuildTrackSnapshot source =
                row.trackKind == ItemBuildTrackKind.FaMen
                    ? qualifiedBuildSnapshot?.FindFaMenTrack(row.buildId)
                    : qualifiedBuildSnapshot?.FindQiLeiTrack(row.buildId);
            HashSet<string> sourceInstanceIds = new(
                source?.SourceItemInstanceIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            string[] activeMemberBaseItemIds =
                (qualifiedBuildSnapshot?.Items ??
                    Array.Empty<ItemInstanceQualifiedBuildItemSnapshot>())
                .Where(item => item != null
                    && sourceInstanceIds.Contains(item.itemInstanceId))
                .Select(item => item.baseItemId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            ItemBuildPlayerPresentationStage[] stages = row.StageRows
                .Select(stage => new ItemBuildPlayerPresentationStage(
                    stage.stagePieceCount,
                    NormalizeStageDescription(stage.effectDescription),
                    stage.isActive))
                .ToArray();
            return ItemBuildPlayerPresentationFormatter.FormatTrack(
                row.trackKind == ItemBuildTrackKind.FaMen,
                row.stableTag,
                row.qualifiedItemCount ?? 0,
                row.maxPieceCount ?? 0,
                stages,
                activeMemberBaseItemIds,
                "  ",
                ItemBuildPlayerPresentationFormatter.ResolveRarityColorHex(
                    model?.rarityKey,
                    model?.rarityColorKey));
        }

        private static void HideBuildPlayerSections(ItemDetailViewModel model)
        {
            ReplaceSection(model, "famenBuild", string.Empty, false);
            ReplaceSection(model, "qileiBuild", string.Empty, false);
        }

        private static void ReplaceSection(
            ItemDetailViewModel model,
            string stateKey,
            string body,
            bool keepWhenEmpty)
        {
            int index = model.displayPlayerSections.FindIndex(section => section != null
                && string.Equals(section.stateKey, stateKey, StringComparison.Ordinal));
            string title = string.Equals(stateKey, "famenBuild", StringComparison.Ordinal)
                ? ItemBuildPlayerPresentationFormatter.FaMenSectionTitle
                : ItemBuildPlayerPresentationFormatter.QiLeiSectionTitle;
            ItemDetailSectionViewModel replacement = new(
                title,
                body ?? string.Empty,
                stateKey,
                keepWhenEmpty);
            if (index >= 0)
            {
                model.displayPlayerSections[index] = replacement;
            }
            else
            {
                model.displayPlayerSections.Add(replacement);
            }
        }

        private static void AppendDebugSection(ItemDetailViewModel model)
        {
            ItemDetailQualifiedBuildTrackProjection projection = model.qualifiedBuildTrack;
            model.displayDebugSections.RemoveAll(section => section != null
                && string.Equals(section.stateKey, "qualifiedBuildTrack",
                    StringComparison.Ordinal));
            model.displayDebugSections.Add(new ItemDetailSectionViewModel(
                "Debug / Qualified Build Track",
                "status: " + projection.status
                + "\ncompleteness: " + projection.completeness
                + "\nitemInstanceId: " + (projection.itemInstanceId ?? "Missing")
                + "\nlocation: " + projection.location
                + "\nbuildQualification: "
                    + (projection.buildQualification?.ToString() ?? "NotApplicable")
                + "\nsourceCanonical: "
                    + (projection.sourceQualifiedBuildCanonicalSignature ?? "Missing")
                + "\nprojectionCanonical: " + projection.canonicalSignature,
                "qualifiedBuildTrack"));
        }

        private static bool ValidateOrdinaryLocation(
            string baseItemId,
            string placementId,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildItemSnapshot item,
            ItemInstanceQualifiedBuildStateStatus status)
        {
            ItemSystemPlacementSnapshot[] basePlacements = itemSystemSnapshot.placements
                .Where(value => value != null
                    && string.Equals(value.itemId, baseItemId,
                        StringComparison.Ordinal))
                .ToArray();
            if (string.IsNullOrEmpty(placementId))
            {
                if (basePlacements.Length != 0 || item.placementId != null)
                {
                    return false;
                }
                return status == ItemInstanceQualifiedBuildStateStatus.Unknown
                    ? item.location == ItemInstanceQualifiedBuildLocation.Unknown
                    : item.location == ItemInstanceQualifiedBuildLocation.Inventory;
            }
            return basePlacements.Length == 1
                && string.Equals(basePlacements[0].placementId, placementId,
                    StringComparison.Ordinal)
                && item.location == ItemInstanceQualifiedBuildLocation.Board
                && item.placementFactCompleteness ==
                    ItemInstanceQualifiedBuildFactCompleteness.Complete
                && string.Equals(item.placementId, placementId,
                    StringComparison.Ordinal);
        }

        private static bool IsCompleteV2ItemSystemSnapshot(ItemSystemSnapshot snapshot)
        {
            return snapshot != null
                && snapshot.isValid
                && string.Equals(snapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal)
                && snapshot.i031State != null
                && snapshot.i031State.ownershipCompleteness ==
                    I031OwnershipCompleteness.Complete
                && snapshot.i031State.isOwned
                && snapshot.i031State.location != I031Location.Unknown;
        }

        private static bool IsAcceptedQualifiedSnapshot(
            ItemInstanceQualifiedBuildStateSnapshot snapshot)
        {
            return snapshot != null
                && string.Equals(snapshot.schemaId,
                    ItemInstanceQualifiedBuildStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                && snapshot.status != ItemInstanceQualifiedBuildStateStatus.Invalid
                && string.Equals(snapshot.canonicalSignature,
                    Sha256(snapshot.BuildCanonicalPayload()),
                    StringComparison.Ordinal);
        }

        private static string NormalizeStageDescription(string value) =>
            string.Join(" ", (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0));

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static ItemDetailQualifiedBuildTrackProjectionResult Success(
            ItemDetailViewModel model,
            string diagnosticCode = "NONE") =>
            new(true, diagnosticCode, model);

        private static ItemDetailQualifiedBuildTrackProjectionResult Failure(string code) =>
            new(false, code, null);

        private static string Sha256(string payload)
        {
            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return "sha256:" + BitConverter.ToString(digest).Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
