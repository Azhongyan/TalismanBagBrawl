using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.Items.Combat
{
    public sealed class ItemCombatEffectRequestInputValidationResult
    {
        private readonly ReadOnlyCollection<ItemCombatEffectRequestValidationError>
            validationErrors;
        private readonly ReadOnlyCollection<ItemCombatEffectRequestBoardJoin>
            boardJoins;

        internal ItemCombatEffectRequestInputValidationResult(
            ItemCombatEffectRequestSnapshotStatus status,
            string sourceProjectionSetCanonicalSignature,
            string sourceQualifiedProjectionSetIdentity,
            string sourceBindingCanonicalSignature,
            string sourceItemSystemCanonicalSignature,
            string sourceQualifiedBuildCanonicalSignature,
            string sourceCoreRuntimeCanonicalSignature,
            IEnumerable<ItemCombatEffectRequestValidationError> validationErrors,
            IEnumerable<ItemCombatEffectRequestBoardJoin> boardJoins)
        {
            this.status = status;
            this.sourceProjectionSetCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceProjectionSetCanonicalSignature);
            this.sourceQualifiedProjectionSetIdentity =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceQualifiedProjectionSetIdentity);
            this.sourceBindingCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceBindingCanonicalSignature);
            this.sourceItemSystemCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceItemSystemCanonicalSignature);
            this.sourceQualifiedBuildCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceQualifiedBuildCanonicalSignature);
            this.sourceCoreRuntimeCanonicalSignature =
                ItemCombatEffectRequestCanonical.Optional(
                    sourceCoreRuntimeCanonicalSignature);
            this.validationErrors = ItemCombatEffectRequestReadOnly.Freeze(
                (validationErrors ??
                    Array.Empty<ItemCombatEffectRequestValidationError>())
                .Where(value => value != null)
                .GroupBy(value => value.code + "|" + value.status + "|"
                    + value.itemInstanceId + "|" + value.placementId + "|"
                    + value.message, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal));
            this.boardJoins = ItemCombatEffectRequestReadOnly.Freeze(
                (boardJoins ?? Array.Empty<ItemCombatEffectRequestBoardJoin>())
                .Where(value => value != null)
                .OrderBy(value => value.Projection.itemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.Placement.placementId,
                    StringComparer.Ordinal));
        }

        public ItemCombatEffectRequestSnapshotStatus status { get; }
        public bool canEmitRequests =>
            status == ItemCombatEffectRequestSnapshotStatus.Valid
            && validationErrors.Count == 0;
        public string sourceProjectionSetCanonicalSignature { get; }
        public string sourceQualifiedProjectionSetIdentity { get; }
        public string sourceBindingCanonicalSignature { get; }
        public string sourceItemSystemCanonicalSignature { get; }
        public string sourceQualifiedBuildCanonicalSignature { get; }
        public string sourceCoreRuntimeCanonicalSignature { get; }
        public IReadOnlyList<ItemCombatEffectRequestValidationError>
            ValidationErrors => validationErrors;
        internal IReadOnlyList<ItemCombatEffectRequestBoardJoin> BoardJoins =>
            boardJoins;
    }

    internal sealed class ItemCombatEffectRequestBoardJoin
    {
        public ItemCombatEffectRequestBoardJoin(
            ItemInstanceProjectionContractSnapshot projection,
            ItemSystemPlacementSnapshot placement,
            ItemSystemCatalogItemSnapshot catalogItem,
            ItemInstanceQualifiedBuildItemSnapshot qualifiedItem,
            ItemInstanceCoreEffectRuntimeItemSnapshot coreItem)
        {
            Projection = projection;
            Placement = placement;
            CatalogItem = catalogItem;
            QualifiedItem = qualifiedItem;
            CoreItem = coreItem;
        }

        public ItemInstanceProjectionContractSnapshot Projection { get; }
        public ItemSystemPlacementSnapshot Placement { get; }
        public ItemSystemCatalogItemSnapshot CatalogItem { get; }
        public ItemInstanceQualifiedBuildItemSnapshot QualifiedItem { get; }
        public ItemInstanceCoreEffectRuntimeItemSnapshot CoreItem { get; }
    }

    public static class ItemCombatEffectRequestValidation
    {
        public const string ProjectionRosterIncomplete =
            "PROJECTION_ROSTER_INCOMPLETE";
        public const string SourceItemSystemSignatureMismatch =
            "SOURCE_ITEM_SYSTEM_SIGNATURE_MISMATCH";
        public const string SourceProjectionSignatureMismatch =
            "SOURCE_PROJECTION_SIGNATURE_MISMATCH";
        public const string SourceBindingSignatureMismatch =
            "SOURCE_BINDING_SIGNATURE_MISMATCH";
        public const string PlacementQualifiedJoinMissing =
            "PLACEMENT_QUALIFIED_JOIN_MISSING";
        public const string PlacementCoreJoinMissing =
            "PLACEMENT_CORE_JOIN_MISSING";
        public const string PlacementBaseIdentityMismatch =
            "PLACEMENT_BASE_IDENTITY_MISMATCH";
        public const string PlacementIdDuplicate =
            "ITEM_SYSTEM_PLACEMENT_ID_DUPLICATE";
        public const string ItemInstanceIdDuplicate =
            "ITEM_INSTANCE_ID_DUPLICATE";

        public static ItemCombatEffectRequestInputValidationResult Validate(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildState,
            ItemInstanceCoreEffectRuntimeStateSnapshot coreEffectRuntimeState)
        {
            List<ItemCombatEffectRequestValidationError> errors = new();
            List<ItemCombatEffectRequestBoardJoin> joins = new();

            string projectionSignature = projectionSet == null
                ? null
                : ItemCombatEffectRequestCanonical.Sha256(
                    projectionSet.BuildCanonicalSignature());
            string itemSystemSignature = itemSystemSnapshot == null
                ? null
                : ItemCombatEffectRequestCanonical.Sha256(
                    itemSystemSnapshot.BuildDebugSignature());
            string projectionIdentity =
                qualifiedBuildState?.sourceProjectionSetIdentity;
            string bindingSignature =
                qualifiedBuildState?.sourceBindingCanonicalSignature;
            string qualifiedSignature = qualifiedBuildState?.canonicalSignature;
            string coreSignature = coreEffectRuntimeState?.canonicalSignature;

            ValidateProjection(projectionSet, errors);
            ValidateItemSystem(itemSystemSnapshot, errors);
            ValidateQualified(qualifiedBuildState, errors);
            ValidateCore(coreEffectRuntimeState, errors);
            ValidateSourceSignatures(
                projectionSignature,
                itemSystemSignature,
                qualifiedBuildState,
                coreEffectRuntimeState,
                errors);

            bool sourcesStructurallyUsable =
                projectionSet?.isValid == true
                && string.Equals(
                    projectionSet.schemaId,
                    ItemInstanceProjectionSetSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                && itemSystemSnapshot?.isValid == true
                && string.Equals(
                    itemSystemSnapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal)
                && qualifiedBuildState?.isValid == true
                && qualifiedBuildState.rosterCompleteness ==
                    QualifiedBuildRosterCompleteness.CompleteOwnedRoster
                && coreEffectRuntimeState?.isValid == true
                && coreEffectRuntimeState.rosterCompleteness ==
                    ItemCoreEffectRosterCompleteness.Complete;
            if (sourcesStructurallyUsable)
            {
                ValidateRostersAndBuildJoins(
                    projectionSet,
                    itemSystemSnapshot,
                    qualifiedBuildState,
                    coreEffectRuntimeState,
                    errors,
                    joins);
            }

            ItemCombatEffectRequestSnapshotStatus status = ResolveStatus(errors);
            if (status != ItemCombatEffectRequestSnapshotStatus.Valid)
            {
                joins.Clear();
            }
            return new ItemCombatEffectRequestInputValidationResult(
                status,
                projectionSignature,
                projectionIdentity,
                bindingSignature,
                itemSystemSignature,
                qualifiedSignature,
                coreSignature,
                errors,
                joins);
        }

        private static void ValidateProjection(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            if (projectionSet == null)
            {
                Unknown(errors, "PROJECTION_SET_MISSING",
                    null, null, "ProjectionSet authority is unavailable.");
                return;
            }
            if (!string.Equals(
                    projectionSet.schemaId,
                    ItemInstanceProjectionSetSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal))
            {
                Invalid(errors, "PROJECTION_SCHEMA_MISMATCH",
                    null, null, "ProjectionSet schema is not v1.");
            }
            if (!projectionSet.isValid)
            {
                Invalid(errors, "PROJECTION_SET_INVALID",
                    null, null, "ProjectionSet has validation errors.");
            }
            ItemInstanceProjectionContractSnapshot[] rows =
                (projectionSet.Projections ??
                    Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .Where(value => value != null)
                .ToArray();
            string[] expected = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString(
                    "000", System.Globalization.CultureInfo.InvariantCulture))
                .ToArray();
            if (rows.GroupBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .Any(group => string.IsNullOrWhiteSpace(group.Key)
                    || group.Count() != 1))
            {
                Invalid(errors, ItemInstanceIdDuplicate,
                    null, null,
                    "Projection itemInstanceId identities are empty or duplicated.");
            }
            string[] actualBaseIds = rows
                .Select(value => value.baseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (actualBaseIds.Distinct(StringComparer.Ordinal).Count()
                != actualBaseIds.Length
                || actualBaseIds.Any(value => string.Equals(
                    value, "I031", StringComparison.Ordinal))
                || actualBaseIds.Except(expected, StringComparer.Ordinal).Any())
            {
                Invalid(errors, "PROJECTION_ORDINARY_ROSTER_INVALID",
                    null, null,
                    "ProjectionSet ordinary roster contains duplicate, extra, or I031 rows.");
            }
            else if (!actualBaseIds.SequenceEqual(expected, StringComparer.Ordinal))
            {
                Unknown(errors, ProjectionRosterIncomplete,
                    null, null,
                    "ProjectionSet does not yet expose exact I001-I030.");
            }
        }

        private static void ValidateItemSystem(
            ItemSystemSnapshot snapshot,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            if (snapshot == null)
            {
                Unknown(errors, "ITEM_SYSTEM_SNAPSHOT_MISSING",
                    null, null, "ItemSystem authority is unavailable.");
                return;
            }
            if (!string.Equals(
                    snapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal))
            {
                Invalid(errors, "ITEM_SYSTEM_SCHEMA_MISMATCH",
                    null, null, "ItemSystem schema is not v2.");
            }
            if (!snapshot.isValid)
            {
                Invalid(errors, "ITEM_SYSTEM_SNAPSHOT_INVALID",
                    null, null, "ItemSystem contains validation errors.");
            }
        }

        private static void ValidateQualified(
            ItemInstanceQualifiedBuildStateSnapshot snapshot,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            if (snapshot == null)
            {
                Unknown(errors, "QUALIFIED_BUILD_STATE_MISSING",
                    null, null, "QualifiedBuild authority is unavailable.");
                return;
            }
            if (!string.Equals(
                    snapshot.schemaId,
                    ItemInstanceQualifiedBuildStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal))
            {
                Invalid(errors, "QUALIFIED_BUILD_SCHEMA_MISMATCH",
                    null, null, "QualifiedBuild schema is not v1.");
            }
            if (snapshot.status ==
                ItemInstanceQualifiedBuildStateStatus.Invalid)
            {
                Invalid(errors, "QUALIFIED_BUILD_STATE_INVALID",
                    null, null, "QualifiedBuild state is Invalid.");
            }
            else if (!snapshot.isValid)
            {
                Unknown(errors, "QUALIFIED_BUILD_STATE_UNKNOWN",
                    null, null, "QualifiedBuild state is not complete.");
            }
            if (snapshot.rosterCompleteness !=
                QualifiedBuildRosterCompleteness.CompleteOwnedRoster)
            {
                Unknown(errors, "QUALIFIED_BUILD_ROSTER_INCOMPLETE",
                    null, null, "QualifiedBuild roster is not CompleteOwnedRoster.");
            }
            if (string.IsNullOrWhiteSpace(snapshot.sourceProjectionSetIdentity))
            {
                Unknown(errors, "QUALIFIED_PROJECTION_IDENTITY_MISSING",
                    null, null, "Qualified projection identity is unavailable.");
            }
            if (string.IsNullOrWhiteSpace(snapshot.canonicalSignature))
            {
                Unknown(errors, "QUALIFIED_CANONICAL_MISSING",
                    null, null, "Qualified canonical signature is unavailable.");
            }
        }

        private static void ValidateCore(
            ItemInstanceCoreEffectRuntimeStateSnapshot snapshot,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            if (snapshot == null)
            {
                Unknown(errors, "CORE_RUNTIME_STATE_MISSING",
                    null, null, "CoreRuntime authority is unavailable.");
                return;
            }
            if (!string.Equals(
                    snapshot.schemaId,
                    ItemInstanceCoreEffectRuntimeStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal))
            {
                Invalid(errors, "CORE_RUNTIME_SCHEMA_MISMATCH",
                    null, null, "CoreRuntime schema is not v2.");
            }
            if (snapshot.status == ItemCoreEffectRuntimeStateStatus.Invalid)
            {
                Invalid(errors, "CORE_RUNTIME_STATE_INVALID",
                    null, null, "CoreRuntime state is Invalid.");
            }
            else if (!snapshot.isValid)
            {
                Unknown(errors, "CORE_RUNTIME_STATE_UNKNOWN",
                    null, null, "CoreRuntime state is not complete.");
            }
            if (snapshot.rosterCompleteness !=
                ItemCoreEffectRosterCompleteness.Complete)
            {
                Unknown(errors, "CORE_RUNTIME_ROSTER_INCOMPLETE",
                    null, null, "CoreRuntime roster is not Complete.");
            }
            if (string.IsNullOrWhiteSpace(snapshot.canonicalSignature))
            {
                Unknown(errors, "CORE_RUNTIME_CANONICAL_MISSING",
                    null, null, "CoreRuntime canonical signature is unavailable.");
            }
        }

        private static void ValidateSourceSignatures(
            string projectionSignature,
            string itemSystemSignature,
            ItemInstanceQualifiedBuildStateSnapshot qualified,
            ItemInstanceCoreEffectRuntimeStateSnapshot core,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            if (qualified != null)
            {
                if (string.IsNullOrWhiteSpace(
                        qualified.sourceItemSystemCanonicalSignature))
                {
                    Unknown(errors, "QUALIFIED_ITEM_SYSTEM_SIGNATURE_MISSING",
                        null, null,
                        "QualifiedBuild ItemSystem signature is unavailable.");
                }
                else if (!string.Equals(
                    qualified.sourceItemSystemCanonicalSignature,
                    itemSystemSignature,
                    StringComparison.Ordinal))
                {
                    Invalid(errors, SourceItemSystemSignatureMismatch,
                        null, null,
                        "QualifiedBuild does not bind the current ItemSystem.");
                }
            }
            if (core != null)
            {
                if (string.IsNullOrWhiteSpace(core.sourceItemSystemSignature))
                {
                    Unknown(errors, "CORE_ITEM_SYSTEM_SIGNATURE_MISSING",
                        null, null,
                        "CoreRuntime ItemSystem signature is unavailable.");
                }
                else if (!SameSha256Digest(
                    core.sourceItemSystemSignature,
                    itemSystemSignature))
                {
                    Invalid(errors, SourceItemSystemSignatureMismatch,
                        null, null,
                        "CoreRuntime does not bind the current ItemSystem.");
                }
                if (string.IsNullOrWhiteSpace(core.sourceProjectionSetSignature))
                {
                    Unknown(errors, "CORE_PROJECTION_SIGNATURE_MISSING",
                        null, null,
                        "CoreRuntime ProjectionSet signature is unavailable.");
                }
                else if (!SameSha256Digest(
                    core.sourceProjectionSetSignature,
                    projectionSignature))
                {
                    Invalid(errors, SourceProjectionSignatureMismatch,
                        null, null,
                        "CoreRuntime does not bind the current ProjectionSet.");
                }
            }
            if (qualified != null && core != null)
            {
                if (string.IsNullOrWhiteSpace(
                        qualified.sourceBindingCanonicalSignature)
                    || string.IsNullOrWhiteSpace(core.sourceBindingSignature))
                {
                    Unknown(errors, "SOURCE_BINDING_SIGNATURE_MISSING",
                        null, null, "Binding signature is unavailable.");
                }
                else if (!string.Equals(
                    qualified.sourceBindingCanonicalSignature,
                    core.sourceBindingSignature,
                    StringComparison.Ordinal))
                {
                    Invalid(errors, SourceBindingSignatureMismatch,
                        null, null,
                        "QualifiedBuild and CoreRuntime binding signatures differ.");
                }
            }
        }

        private static void ValidateRostersAndBuildJoins(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemSystemSnapshot itemSystem,
            ItemInstanceQualifiedBuildStateSnapshot qualified,
            ItemInstanceCoreEffectRuntimeStateSnapshot core,
            ICollection<ItemCombatEffectRequestValidationError> errors,
            ICollection<ItemCombatEffectRequestBoardJoin> joins)
        {
            ValidateCompleteAuthorityRosters(
                projectionSet, qualified, core, errors);

            ItemSystemPlacementSnapshot[] placements =
                (itemSystem.placements ??
                    Array.Empty<ItemSystemPlacementSnapshot>())
                .Where(value => value != null)
                .ToArray();
            foreach (IGrouping<string, ItemSystemPlacementSnapshot> duplicate
                     in placements.GroupBy(
                         value => value.placementId, StringComparer.Ordinal)
                         .Where(group => string.IsNullOrWhiteSpace(group.Key)
                             || group.Count() != 1))
            {
                Invalid(errors, PlacementIdDuplicate,
                    null, duplicate.Key,
                    "ItemSystem placementId is empty or duplicated.");
            }
            foreach (IGrouping<string, ItemSystemPlacementSnapshot> duplicate
                     in placements.Where(value => !string.Equals(
                         value.itemId, "I031", StringComparison.Ordinal))
                         .GroupBy(value => value.itemId, StringComparer.Ordinal)
                         .Where(group => string.IsNullOrWhiteSpace(group.Key)
                             || group.Count() != 1))
            {
                Invalid(errors, "ITEM_SYSTEM_BASE_ID_DUPLICATE",
                    null, duplicate.FirstOrDefault()?.placementId,
                    "An ordinary base item has multiple board placements.");
            }

            Dictionary<string, ItemSystemCatalogItemSnapshot[]> catalog =
                (itemSystem.catalogItems ??
                    Array.Empty<ItemSystemCatalogItemSnapshot>())
                .Where(value => value != null)
                .GroupBy(value => value.itemId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray(),
                    StringComparer.Ordinal);
            foreach (KeyValuePair<string, ItemSystemCatalogItemSnapshot[]> pair
                     in catalog.Where(pair => pair.Value.Length != 1))
            {
                Invalid(errors, "ITEM_SYSTEM_CATALOG_ID_DUPLICATE",
                    null, null,
                    "Catalog itemId is duplicated: " + pair.Key);
            }

            foreach (ItemSystemPlacementSnapshot placement in placements
                         .OrderBy(value => value.placementId,
                             StringComparer.Ordinal))
            {
                if (string.Equals(
                        placement.itemId, "I031", StringComparison.Ordinal))
                {
                    if (!string.Equals(
                        placement.placementId,
                        "P_SYSTEM_I031",
                        StringComparison.Ordinal))
                    {
                        Invalid(errors, "I031_PLACEMENT_IDENTITY_INVALID",
                            null, placement.placementId,
                            "I031 may only use P_SYSTEM_I031.");
                    }
                    continue;
                }

                ItemInstanceQualifiedBuildItemSnapshot[] qualifiedMatches =
                    qualified.Items.Where(value =>
                        value.location ==
                            ItemInstanceQualifiedBuildLocation.Board
                        && string.Equals(
                            value.placementId,
                            placement.placementId,
                            StringComparison.Ordinal))
                    .ToArray();
                if (qualifiedMatches.Length != 1)
                {
                    Invalid(errors, PlacementQualifiedJoinMissing,
                        null, placement.placementId,
                        "Placement has no exact QualifiedBuild board join.");
                    continue;
                }
                ItemInstanceQualifiedBuildItemSnapshot qualifiedItem =
                    qualifiedMatches[0];
                ItemInstanceProjectionContractSnapshot[] projectionMatches =
                    projectionSet.Projections.Where(value =>
                        string.Equals(
                            value.itemInstanceId,
                            qualifiedItem.itemInstanceId,
                            StringComparison.Ordinal))
                    .ToArray();
                if (projectionMatches.Length != 1)
                {
                    Invalid(errors, "PLACEMENT_PROJECTION_JOIN_MISSING",
                        qualifiedItem.itemInstanceId, placement.placementId,
                        "Placement has no exact Projection join.");
                    continue;
                }
                ItemInstanceCoreEffectRuntimeItemSnapshot[] coreMatches =
                    core.Items.Where(value => string.Equals(
                        value.itemInstanceId,
                        qualifiedItem.itemInstanceId,
                        StringComparison.Ordinal)).ToArray();
                if (coreMatches.Length != 1)
                {
                    Invalid(errors, PlacementCoreJoinMissing,
                        qualifiedItem.itemInstanceId, placement.placementId,
                        "Placement has no exact CoreRuntime join.");
                    continue;
                }
                if (!catalog.TryGetValue(
                        placement.itemId,
                        out ItemSystemCatalogItemSnapshot[] catalogMatches)
                    || catalogMatches.Length != 1)
                {
                    Invalid(errors, "PLACEMENT_CATALOG_JOIN_MISSING",
                        qualifiedItem.itemInstanceId, placement.placementId,
                        "Placement has no exact catalog join.");
                    continue;
                }

                ItemInstanceProjectionContractSnapshot projection =
                    projectionMatches[0];
                ItemInstanceCoreEffectRuntimeItemSnapshot coreItem =
                    coreMatches[0];
                if (!ExactIdentity(
                        placement, projection, qualifiedItem, coreItem))
                {
                    Invalid(errors, PlacementBaseIdentityMismatch,
                        qualifiedItem.itemInstanceId, placement.placementId,
                        "Instance/placement/base/rarity identity is mismatched.");
                    continue;
                }
                if (!CompleteFacts(placement, qualifiedItem, coreItem))
                {
                    Invalid(errors, "PLACEMENT_FACTS_CONTRADICTED",
                        qualifiedItem.itemInstanceId, placement.placementId,
                        "Board facts are not complete or do not match.");
                    continue;
                }
                joins.Add(new ItemCombatEffectRequestBoardJoin(
                    projection,
                    placement,
                    catalogMatches[0],
                    qualifiedItem,
                    coreItem));
            }

            string[] ordinaryPlacementIds = placements
                .Where(value => !string.Equals(
                    value.itemId, "I031", StringComparison.Ordinal))
                .Select(value => value.placementId)
                .ToArray();
            foreach (ItemInstanceQualifiedBuildItemSnapshot item
                     in qualified.Items.Where(value =>
                         value.location ==
                            ItemInstanceQualifiedBuildLocation.Board))
            {
                if (!ordinaryPlacementIds.Contains(
                        item.placementId, StringComparer.Ordinal))
                {
                    Invalid(errors, "QUALIFIED_BOARD_ORPHAN",
                        item.itemInstanceId, item.placementId,
                        "QualifiedBuild board item has no ItemSystem placement.");
                }
            }
            foreach (ItemInstanceCoreEffectRuntimeItemSnapshot item
                     in core.Items.Where(value =>
                         value.location ==
                            ItemCoreEffectRuntimeLocation.Board))
            {
                if (!ordinaryPlacementIds.Contains(
                        item.placementId, StringComparer.Ordinal))
                {
                    Invalid(errors, "CORE_BOARD_ORPHAN",
                        item.itemInstanceId, item.placementId,
                        "CoreRuntime board item has no ItemSystem placement.");
                }
            }
        }

        private static void ValidateCompleteAuthorityRosters(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemInstanceQualifiedBuildStateSnapshot qualified,
            ItemInstanceCoreEffectRuntimeStateSnapshot core,
            ICollection<ItemCombatEffectRequestValidationError> errors)
        {
            string[] projectionIds = projectionSet.Projections
                .Select(value => value.itemInstanceId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] qualifiedIds = qualified.Items
                .Select(value => value.itemInstanceId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] coreIds = core.Items
                .Select(value => value.itemInstanceId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (qualifiedIds.Length != 30
                || qualifiedIds.Distinct(StringComparer.Ordinal).Count()
                    != qualifiedIds.Length
                || !qualifiedIds.SequenceEqual(
                    projectionIds, StringComparer.Ordinal))
            {
                Invalid(errors, "QUALIFIED_ROSTER_CONTRADICTION",
                    null, null,
                    "QualifiedBuild roster is not the exact Projection roster.");
            }
            if (coreIds.Length != 30
                || coreIds.Distinct(StringComparer.Ordinal).Count()
                    != coreIds.Length
                || !coreIds.SequenceEqual(
                    projectionIds, StringComparer.Ordinal))
            {
                Invalid(errors, "CORE_ROSTER_CONTRADICTION",
                    null, null,
                    "CoreRuntime roster is not the exact Projection roster.");
            }
            ItemCoreAwakeningNodeKind[] exactKinds =
            {
                ItemCoreAwakeningNodeKind.Core1,
                ItemCoreAwakeningNodeKind.Core2,
                ItemCoreAwakeningNodeKind.Core3,
                ItemCoreAwakeningNodeKind.Ultimate
            };
            foreach (ItemInstanceCoreEffectRuntimeItemSnapshot item in core.Items)
            {
                ItemCoreAwakeningNodeKind[] kinds = item.CoreEffectRows
                    .Select(value => value.nodeKind)
                    .OrderBy(value => (int)value)
                    .ToArray();
                bool exact = kinds.SequenceEqual(exactKinds)
                    && item.CoreEffectRows.All(value =>
                        !string.IsNullOrWhiteSpace(
                            value.candidateDefinitionId)
                        && !string.IsNullOrWhiteSpace(value.awakeningNodeId)
                        && value.stateCompleteness ==
                            ItemCoreEffectFactCompleteness.Complete
                        && value.eligibleFact !=
                            ItemCoreEffectBooleanFact.Unknown
                        && value.visibleFact !=
                            ItemCoreEffectBooleanFact.Unknown
                        && value.unlockedFact !=
                            ItemCoreEffectBooleanFact.Unknown
                        && value.activeFact !=
                            ItemCoreEffectBooleanFact.Unknown);
                if (!exact)
                {
                    Invalid(errors, "CORE_FOUR_ROW_AUTHORITY_INVALID",
                        item.itemInstanceId, item.placementId,
                        "CoreRuntime item is not exact Core1/Core2/Core3/ULT.");
                }
            }
        }

        private static bool ExactIdentity(
            ItemSystemPlacementSnapshot placement,
            ItemInstanceProjectionContractSnapshot projection,
            ItemInstanceQualifiedBuildItemSnapshot qualified,
            ItemInstanceCoreEffectRuntimeItemSnapshot core)
        {
            return string.Equals(
                    placement.itemId,
                    projection.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    qualified.baseItemId,
                    projection.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    core.baseItemId,
                    projection.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    qualified.itemInstanceId,
                    projection.itemInstanceId,
                    StringComparison.Ordinal)
                && string.Equals(
                    core.itemInstanceId,
                    projection.itemInstanceId,
                    StringComparison.Ordinal)
                && string.Equals(
                    qualified.placementId,
                    placement.placementId,
                    StringComparison.Ordinal)
                && string.Equals(
                    core.placementId,
                    placement.placementId,
                    StringComparison.Ordinal)
                && qualified.rarity == projection.rarity
                && core.rarity == projection.rarity
                && qualified.buildQualification ==
                    projection.buildQualification;
        }

        private static bool CompleteFacts(
            ItemSystemPlacementSnapshot placement,
            ItemInstanceQualifiedBuildItemSnapshot qualified,
            ItemInstanceCoreEffectRuntimeItemSnapshot core)
        {
            ItemInstanceQualifiedBuildBooleanFact lit =
                placement.isLit
                    ? ItemInstanceQualifiedBuildBooleanFact.True
                    : ItemInstanceQualifiedBuildBooleanFact.False;
            ItemInstanceQualifiedBuildBooleanFact counted =
                placement.isCountedInBuild
                    ? ItemInstanceQualifiedBuildBooleanFact.True
                    : ItemInstanceQualifiedBuildBooleanFact.False;
            ItemCoreEffectBooleanFact coreLit =
                placement.isLit
                    ? ItemCoreEffectBooleanFact.KnownTrue
                    : ItemCoreEffectBooleanFact.KnownFalse;
            return qualified.placementFactCompleteness ==
                    ItemInstanceQualifiedBuildFactCompleteness.Complete
                && qualified.buildFactCompleteness ==
                    ItemInstanceQualifiedBuildFactCompleteness.Complete
                && qualified.isLitFact == lit
                && qualified.sourceIsCountedFact == counted
                && qualified.faMenBuildCount.HasValue
                && qualified.qiLeiBuildCount.HasValue
                && qualified.faMenActiveStagePieceCount.HasValue
                && qualified.qiLeiActiveStagePieceCount.HasValue
                && core.location == ItemCoreEffectRuntimeLocation.Board
                && core.cultivationFactCompleteness ==
                    ItemCoreEffectFactCompleteness.Complete
                && core.cultivationLevel.HasValue
                && core.isLitFact == coreLit
                && core.stateCompleteness ==
                    ItemCoreEffectFactCompleteness.Complete;
        }

        private static ItemCombatEffectRequestSnapshotStatus ResolveStatus(
            IEnumerable<ItemCombatEffectRequestValidationError> errors)
        {
            ItemCombatEffectRequestValidationError[] values =
                (errors ?? Array.Empty<ItemCombatEffectRequestValidationError>())
                .ToArray();
            if (values.Any(value =>
                    value.status ==
                        ItemCombatEffectRequestSnapshotStatus.Invalid))
            {
                return ItemCombatEffectRequestSnapshotStatus.Invalid;
            }
            return values.Any(value =>
                    value.status ==
                        ItemCombatEffectRequestSnapshotStatus.Unknown)
                ? ItemCombatEffectRequestSnapshotStatus.Unknown
                : ItemCombatEffectRequestSnapshotStatus.Valid;
        }

        private static bool SameSha256Digest(string left, string right)
        {
            const string prefix = "sha256:";
            string normalizedLeft = (left ?? string.Empty).Trim();
            string normalizedRight = (right ?? string.Empty).Trim();
            if (normalizedLeft.StartsWith(
                    prefix, StringComparison.Ordinal))
            {
                normalizedLeft = normalizedLeft.Substring(prefix.Length);
            }
            if (normalizedRight.StartsWith(
                    prefix, StringComparison.Ordinal))
            {
                normalizedRight = normalizedRight.Substring(prefix.Length);
            }
            return normalizedLeft.Length == 64
                && normalizedRight.Length == 64
                && string.Equals(
                    normalizedLeft,
                    normalizedRight,
                    StringComparison.Ordinal);
        }

        private static void Invalid(
            ICollection<ItemCombatEffectRequestValidationError> errors,
            string code,
            string itemInstanceId,
            string placementId,
            string message)
        {
            errors.Add(new ItemCombatEffectRequestValidationError(
                code,
                ItemCombatEffectRequestSnapshotStatus.Invalid,
                itemInstanceId,
                placementId,
                message));
        }

        private static void Unknown(
            ICollection<ItemCombatEffectRequestValidationError> errors,
            string code,
            string itemInstanceId,
            string placementId,
            string message)
        {
            errors.Add(new ItemCombatEffectRequestValidationError(
                code,
                ItemCombatEffectRequestSnapshotStatus.Unknown,
                itemInstanceId,
                placementId,
                message));
        }
    }
}
