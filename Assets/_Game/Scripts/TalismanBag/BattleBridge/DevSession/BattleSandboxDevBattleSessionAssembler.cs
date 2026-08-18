using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Resource;

namespace TalismanBag.BattleBridge.DevSession
{
    public static class BattleSandboxDevBattleSessionAssembler
    {
        private const string LiHuoBuildId = "famen:lihuo";

        private static readonly IReadOnlyDictionary<string, string>
            ControlledIdentityByBaseId =
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "I031", "SPECIAL_I031" },
                    { "I007", "wb_i007_orange_404310007" },
                    { "I008", "wb_i008_orange_404310008" },
                    { "I009", "wb_i009_orange_404310009" },
                    { "I010", "wb_i010_orange_404310010" },
                    { "I011", "wb_i011_orange_404310011" },
                    { "I012", "wb_i012_orange_404310012" }
                };

        private static readonly string[] InitialBaseIds =
        {
            "I008", "I010", "I011", "I031"
        };

        public static bool TryAssemble(
            IItemSystemBattleSandboxBoardAuthority authority,
            string hostSessionToken,
            int resetGeneration,
            string labProfileId,
            string mechanicsProfileId,
            int battleIndex,
            string selectedRewardBaseItemId,
            BattleSandboxExplicitDevEncounterRequest encounter,
            out BattleSandboxDevBattleSessionRequest request,
            out string diagnosticCode)
        {
            request = null;
            diagnosticCode = "NONE";
            if (authority == null
                || authority.CurrentSnapshot == null
                || authority.CurrentBindingSnapshot == null
                || authority.CurrentQualifiedBuildState?.isValid != true
                || authority.CurrentCoreEffectRuntimeState?.isValid != true
                || authority.CurrentDevSessionRosterAvailability == null)
            {
                diagnosticCode = "ITEM_AUTHORITY_SOURCE_MISSING";
                return false;
            }

            string[] expectedBaseIds = battleIndex == 1
                ? InitialBaseIds
                : InitialBaseIds
                    .Append(selectedRewardBaseItemId ?? string.Empty)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
            string[] availableBaseIds =
                authority.CurrentDevSessionRosterAvailability
                    .AvailableBaseItemIds
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
            if (!availableBaseIds.SequenceEqual(
                    expectedBaseIds.OrderBy(value => value,
                        StringComparer.Ordinal),
                    StringComparer.Ordinal))
            {
                diagnosticCode = "CONTROLLED_ROSTER_BASE_IDS_MISMATCH";
                return false;
            }

            List<string> availableIdentityIds = new();
            foreach (string baseItemId in availableBaseIds)
            {
                if (!ControlledIdentityByBaseId.TryGetValue(
                        baseItemId,
                        out string expectedIdentity))
                {
                    diagnosticCode = "CONTROLLED_ROSTER_IDENTITY_MAPPING_MISSING";
                    return false;
                }

                if (string.Equals(baseItemId, "I031",
                        StringComparison.Ordinal))
                {
                    if (!string.Equals(
                            expectedIdentity,
                            I031InventoryPlacementContract.SpecialIdentityId,
                            StringComparison.Ordinal))
                    {
                        diagnosticCode = "I031_SPECIAL_IDENTITY_MISMATCH";
                        return false;
                    }
                }
                else
                {
                    ItemSystemBattleSandboxViewRow row = authority.Rows
                        .SingleOrDefault(value => value != null
                            && string.Equals(value.BaseItemId,
                                baseItemId,
                                StringComparison.Ordinal));
                    if (row?.Projection == null
                        || !string.Equals(row.ItemInstanceId,
                            expectedIdentity,
                            StringComparison.Ordinal))
                    {
                        diagnosticCode =
                            "CONTROLLED_ROSTER_AUTHORITATIVE_IDENTITY_MISMATCH";
                        return false;
                    }
                }

                availableIdentityIds.Add(expectedIdentity);
            }

            ItemInstanceProjectionSetSnapshot projectionSet = new(
                authority.Rows
                    .Where(row => row != null && !row.IsSystemItem)
                    .Select(row => row.Projection)
                    .Where(value => value != null),
                Array.Empty<ItemInstanceProjectionValidationError>());
            ItemCombatEffectRequestSnapshot itemRequests =
                ItemCombatEffectRequestAssembler.Instance.Assemble(
                    projectionSet,
                    authority.CurrentSnapshot,
                    authority.CurrentQualifiedBuildState,
                    authority.CurrentCoreEffectRuntimeState);
            if (itemRequests?.status !=
                    ItemCombatEffectRequestSnapshotStatus.Valid
                || itemRequests.entersFormalBattle
                || itemRequests.requestCount == 0)
            {
                diagnosticCode = "ITEM_COMBAT_REQUEST_ASSEMBLY_REJECTED";
                return false;
            }

            I031NianSourceSnapshot nianSource =
                I031NianSourceAssembler.Assemble(
                    authority.CurrentSnapshot,
                    resetGeneration);
            if (nianSource.status != I031NianSourceStatus.Valid
                || !nianSource.isEligible)
            {
                diagnosticCode = "I031_NIAN_SOURCE_ASSEMBLY_REJECTED";
                return false;
            }

            List<BattleSandboxDevBattleItemRequestFact> itemFacts = new();
            foreach (ItemCombatEffectRequestRow itemRequest
                     in itemRequests.Requests)
            {
                if (!availableBaseIds.Contains(
                        itemRequest.sourceBaseItemId,
                        StringComparer.Ordinal)
                    || !ControlledIdentityByBaseId.TryGetValue(
                        itemRequest.sourceBaseItemId,
                        out string expectedIdentity)
                    || !string.Equals(itemRequest.sourceItemInstanceId,
                        expectedIdentity,
                        StringComparison.Ordinal))
                {
                    diagnosticCode = "ITEM_REQUEST_OUTSIDE_CONTROLLED_ROSTER";
                    return false;
                }

                ItemSystemBattleSandboxViewRow row = authority.Rows
                    .SingleOrDefault(value => value != null
                        && string.Equals(value.BaseItemId,
                            itemRequest.sourceBaseItemId,
                            StringComparison.Ordinal));
                I031NianCostRequestFact cost =
                    nianSource.FindCostFact(itemRequest.sourceBaseItemId);
                if (row == null || cost == null)
                {
                    diagnosticCode = "ITEM_REQUEST_AUTHORITY_JOIN_MISSING";
                    return false;
                }

                itemFacts.Add(new BattleSandboxDevBattleItemRequestFact(
                    itemRequest.requestId,
                    itemRequest.sourceItemInstanceId,
                    itemRequest.sourceBaseItemId,
                    itemRequest.sourcePlacementId,
                    itemRequest.sourceProjectionCanonicalSignature,
                    cost.requestFactId,
                    row.ShapeCells.Count,
                    itemRequest.baseDamageRawUnits,
                    itemRequest.resolvedPreMitigationDamageUnits));
            }

            string rewardIdentity = string.Empty;
            if (battleIndex == 2
                && (!ControlledIdentityByBaseId.TryGetValue(
                        selectedRewardBaseItemId ?? string.Empty,
                        out rewardIdentity)
                    || !itemFacts.Any(value => string.Equals(
                        value.SourceBaseItemId,
                        selectedRewardBaseItemId,
                        StringComparison.Ordinal))))
            {
                diagnosticCode = "SELECTED_REWARD_NOT_INSTALLED_LIT_AND_LEGAL";
                return false;
            }
            if (!itemFacts.Any(value => string.Equals(
                    value.SourceBaseItemId,
                    "I010",
                    StringComparison.Ordinal)))
            {
                diagnosticCode = "I010_NOT_INSTALLED_LIT_AND_LEGAL";
                return false;
            }

            var liHuoTrack = authority.CurrentQualifiedBuildState
                .FindFaMenTrack(LiHuoBuildId);
            if (liHuoTrack == null)
            {
                diagnosticCode = "LIHUO_QUALIFIED_TRACK_MISSING";
                return false;
            }

            request = new BattleSandboxDevBattleSessionRequest(
                BattleSandboxDevBattleSessionRequest.CurrentSchemaId,
                true,
                BattleSandboxDevBattleSessionRequest.RequiredProductContext,
                BattleSandboxDevBattleSessionRequest.ApprovedHostContext,
                BattleSandboxDevBattleSessionRequest.ApprovedHostPackageId,
                false,
                false,
                false,
                hostSessionToken,
                resetGeneration,
                labProfileId,
                mechanicsProfileId,
                battleIndex,
                battleIndex == 2 ? selectedRewardBaseItemId : string.Empty,
                battleIndex == 2 ? rewardIdentity : string.Empty,
                encounter,
                new BattleSandboxDevBattleLiveSignatures(
                    authority.CurrentDevSessionRosterAvailability
                        .CanonicalSignature,
                    authority.CurrentSnapshot.BuildDebugSignature(),
                    authority.CurrentQualifiedBuildState.canonicalSignature,
                    authority.CurrentCoreEffectRuntimeState.canonicalSignature,
                    nianSource.canonicalSignature),
                availableBaseIds,
                availableIdentityIds,
                liHuoTrack.qualifiedItemCount,
                liHuoTrack.activeStagePieceCount,
                liHuoTrack.SourceBaseItemIds,
                liHuoTrack.activeStagePieceCount >= 2,
                nianSource,
                itemFacts);
            if (!BattleSandboxDevBattleSessionContract.Validate(
                    request,
                    true,
                    out diagnosticCode))
            {
                request = null;
                return false;
            }

            diagnosticCode = "NONE";
            return true;
        }

        public static bool TryCaptureLiveSignatures(
            IItemSystemBattleSandboxBoardAuthority authority,
            int resetGeneration,
            out BattleSandboxDevBattleLiveSignatures signatures,
            out string diagnosticCode)
        {
            signatures = null;
            diagnosticCode = "NONE";
            if (authority == null
                || authority.CurrentSnapshot == null
                || authority.CurrentQualifiedBuildState?.isValid != true
                || authority.CurrentCoreEffectRuntimeState?.isValid != true
                || authority.CurrentDevSessionRosterAvailability == null)
            {
                diagnosticCode = "ITEM_AUTHORITY_SOURCE_MISSING";
                return false;
            }

            I031NianSourceSnapshot source = I031NianSourceAssembler.Assemble(
                authority.CurrentSnapshot,
                resetGeneration);
            if (source.status != I031NianSourceStatus.Valid
                || !source.isEligible)
            {
                diagnosticCode = "I031_NIAN_SOURCE_DRIFTED";
                return false;
            }
            signatures = new BattleSandboxDevBattleLiveSignatures(
                authority.CurrentDevSessionRosterAvailability.CanonicalSignature,
                authority.CurrentSnapshot.BuildDebugSignature(),
                authority.CurrentQualifiedBuildState.canonicalSignature,
                authority.CurrentCoreEffectRuntimeState.canonicalSignature,
                source.canonicalSignature);
            return true;
        }
    }
}
