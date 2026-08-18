using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.Items.Combat
{
    public interface IItemCombatEffectRequestAssembler
    {
        ItemCombatEffectRequestSnapshot Assemble(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildState,
            ItemInstanceCoreEffectRuntimeStateSnapshot coreEffectRuntimeState);
    }

    public sealed class ItemCombatEffectRequestAssembler :
        IItemCombatEffectRequestAssembler
    {
        public const string DamageStatId = "damage";
        public const string DamageUpAffixId = "affix_damage_up";
        public const string AdjacentDamageAffixId = "affix_adjacent_damage";
        public const string DamageCostTradeAffixId =
            "affix_damage_cost_trade";
        public const string CooldownReductionAffixId =
            "affix_cooldown_reduction";
        public const string LiHuoBuildId = "famen:lihuo";
        public const string LiHuoBuild2EffectId =
            "famen:lihuo:build2:effect";
        public const string LiHuoBuild4EffectId =
            "famen:lihuo:build4:effect";
        public const string LiHuoBuild6EffectId =
            "famen:lihuo:build6:effect";
        public const long LiHuoBuild2BasisPoints = 800L;
        public const int AdjacentDamageStackLimit = 3;

        public static readonly ItemCombatEffectRequestAssembler Instance =
            new ItemCombatEffectRequestAssembler();

        public ItemCombatEffectRequestSnapshot Assemble(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildState,
            ItemInstanceCoreEffectRuntimeStateSnapshot coreEffectRuntimeState)
        {
            ItemCombatEffectRequestInputValidationResult validation =
                ItemCombatEffectRequestValidation.Validate(
                    projectionSet,
                    itemSystemSnapshot,
                    qualifiedBuildState,
                    coreEffectRuntimeState);
            if (!validation.canEmitRequests)
            {
                return Snapshot(
                    validation,
                    Array.Empty<ItemCombatEffectRequestRow>(),
                    Array.Empty<ItemCombatEffectUnsupportedTelemetry>(),
                    validation.ValidationErrors);
            }

            List<ItemCombatEffectRequestRow> requests = new();
            List<ItemCombatEffectUnsupportedTelemetry> telemetry = new();
            List<ItemCombatEffectRequestValidationError> errors =
                validation.ValidationErrors.ToList();
            try
            {
                foreach (ItemCombatEffectRequestBoardJoin join
                         in validation.BoardJoins)
                {
                    BuildCoreTelemetry(join, telemetry);
                    BuildLiHuoUnsupportedTelemetry(
                        join, qualifiedBuildState, telemetry);
                    if (!join.Placement.isLit
                        || !join.Placement.isCountedInBuild)
                    {
                        telemetry.Add(Telemetry(
                            join,
                            "item:activation",
                            ItemCombatEffectUnsupportedSourceKind.Build,
                            "RequestEligibility",
                            "boolean",
                            ItemCombatEffectMagnitudePresence.Present,
                            join.Placement.isLit
                                && join.Placement.isCountedInBuild ? 1L : 0L,
                            ItemCombatEffectUnsupportedDisposition.NotExecuted,
                            "ITEM_NOT_LIT_OR_SOURCE_COUNTED",
                            new[]
                            {
                                "isLit=" + join.Placement.isLit,
                                "isCountedInBuild="
                                + join.Placement.isCountedInBuild
                            }));
                        continue;
                    }

                    if (!TryBuildRequest(
                            join,
                            itemSystemSnapshot,
                            qualifiedBuildState,
                            telemetry,
                            errors,
                            out ItemCombatEffectRequestRow request))
                    {
                        continue;
                    }
                    requests.Add(request);
                }
            }
            catch (OverflowException)
            {
                errors.Add(new ItemCombatEffectRequestValidationError(
                    "DAMAGE_INTEGER_OVERFLOW",
                    ItemCombatEffectRequestSnapshotStatus.Invalid,
                    null,
                    null,
                    "Checked Item damage arithmetic overflowed."));
            }
            catch (Exception exception)
            {
                errors.Add(new ItemCombatEffectRequestValidationError(
                    "ASSEMBLY_EXCEPTION_" + exception.GetType().Name,
                    ItemCombatEffectRequestSnapshotStatus.Invalid,
                    null,
                    null,
                    "Item request construction failed closed."));
            }

            if (errors.Any(value =>
                    value.status ==
                        ItemCombatEffectRequestSnapshotStatus.Invalid))
            {
                return Snapshot(
                    validation,
                    Array.Empty<ItemCombatEffectRequestRow>(),
                    telemetry,
                    errors,
                    ItemCombatEffectRequestSnapshotStatus.Invalid);
            }

            return Snapshot(
                validation,
                requests,
                telemetry,
                errors,
                ItemCombatEffectRequestSnapshotStatus.Valid);
        }

        private static bool TryBuildRequest(
            ItemCombatEffectRequestBoardJoin join,
            ItemSystemSnapshot itemSystem,
            ItemInstanceQualifiedBuildStateSnapshot qualifiedState,
            ICollection<ItemCombatEffectUnsupportedTelemetry> telemetry,
            ICollection<ItemCombatEffectRequestValidationError> errors,
            out ItemCombatEffectRequestRow request)
        {
            request = null;
            ItemInstanceProjectionStatSnapshot[] damageStats =
                join.Projection.Stats.Where(value => string.Equals(
                    value.statId, DamageStatId, StringComparison.Ordinal))
                .ToArray();
            if (damageStats.Length != 1 || damageStats[0].rawUnits < 0)
            {
                Invalid(
                    errors,
                    "BASE_DAMAGE_CARDINALITY_OR_SIGN_INVALID",
                    join,
                    "Exactly one non-negative damage stat is required.");
                return false;
            }

            List<ItemCombatEffectRequestContribution> contributions = new();
            long baseDamage = damageStats[0].rawUnits;
            contributions.Add(new ItemCombatEffectRequestContribution(
                ItemCombatEffectContributionSourceKind.BaseStat,
                "stat:damage",
                "AddFlat",
                DamageStatId,
                "rawUnit",
                baseDamage,
                1,
                0L,
                ItemCombatEffectContributionDisposition.AppliedToRequest,
                "ITEM_INSTANCE_PROJECTION_V1"));

            long additiveBasisPoints = 0L;
            foreach (ItemInstanceProjectionAffixSnapshot affix
                     in join.Projection.Affixes
                         .OrderBy(value => value.slotId, StringComparer.Ordinal)
                         .ThenBy(value => value.affixId, StringComparer.Ordinal))
            {
                if (affix.rawUnits < 0)
                {
                    Invalid(
                        errors,
                        "AFFIX_MAGNITUDE_NEGATIVE",
                        join,
                        "Affix rawUnits cannot be negative.");
                    return false;
                }
                switch (affix.affixId)
                {
                    case DamageUpAffixId:
                        if (affix.slotKind != ItemAffixSlotKind.Fixed)
                        {
                            Invalid(
                                errors,
                                "DAMAGE_UP_NOT_FIXED_SLOT",
                                join,
                                "affix_damage_up must use the fixed slot.");
                            return false;
                        }
                        additiveBasisPoints = checked(
                            additiveBasisPoints + affix.rawUnits);
                        contributions.Add(PercentContribution(
                            ItemCombatEffectContributionSourceKind.Affix,
                            "affix_damage_up:effect",
                            affix.rawUnits,
                            1,
                            affix.rawUnits,
                            "BALANCE_CANDIDATE"));
                        break;
                    case AdjacentDamageAffixId:
                        int stackCount = CountAdjacentSameFaMen(
                            join, itemSystem);
                        long applied = checked(
                            affix.rawUnits * stackCount);
                        additiveBasisPoints = checked(
                            additiveBasisPoints + applied);
                        contributions.Add(PercentContribution(
                            ItemCombatEffectContributionSourceKind.Affix,
                            "affix_adjacent_damage:effect",
                            affix.rawUnits,
                            stackCount,
                            applied,
                            "BALANCE_CANDIDATE"));
                        break;
                    case DamageCostTradeAffixId:
                        additiveBasisPoints = checked(
                            additiveBasisPoints + affix.rawUnits);
                        contributions.Add(PercentContribution(
                            ItemCombatEffectContributionSourceKind.Affix,
                            "affix_damage_cost_trade:damage",
                            affix.rawUnits,
                            1,
                            affix.rawUnits,
                            "BALANCE_CANDIDATE"));
                        telemetry.Add(Telemetry(
                            join,
                            "affix_damage_cost_trade:resource_cost",
                            ItemCombatEffectUnsupportedSourceKind.Resource,
                            "IncreaseResourceCost",
                            "unknown",
                            ItemCombatEffectMagnitudePresence.Missing,
                            null,
                            ItemCombatEffectUnsupportedDisposition.NotExecuted,
                            "RESOURCE_OWNER_NOT_IN_ITEM_REQUEST_CONTRACT_V1",
                            AffixIdentities(affix)));
                        break;
                    case CooldownReductionAffixId:
                        telemetry.Add(Telemetry(
                            join,
                            "affix_cooldown_reduction:effect",
                            ItemCombatEffectUnsupportedSourceKind.Cooldown,
                            "ReduceFlat",
                            "turn",
                            ItemCombatEffectMagnitudePresence.Conflicted,
                            affix.rawUnits,
                            ItemCombatEffectUnsupportedDisposition.NotExecuted,
                            "COOLDOWN_MACHINE_UNIT_CONFLICT_NOT_EXECUTED_V1",
                            AffixIdentities(affix)));
                        break;
                    default:
                        telemetry.Add(Telemetry(
                            join,
                            affix.affixId + ":effect",
                            ItemCombatEffectUnsupportedSourceKind.Affix,
                            "Unapproved",
                            "unknown",
                            ItemCombatEffectMagnitudePresence.Present,
                            affix.rawUnits,
                            ItemCombatEffectUnsupportedDisposition.Unknown,
                            "AFFIX_MACHINE_CONTRACT_NOT_APPROVED_V1",
                            AffixIdentities(affix)));
                        break;
                }
            }

            ItemInstanceProjectionAffixSnapshot[] damageUpRows =
                join.Projection.Affixes.Where(value => string.Equals(
                    value.affixId, DamageUpAffixId,
                    StringComparison.Ordinal)).ToArray();
            if (damageUpRows.Length > 1)
            {
                Invalid(
                    errors,
                    "DAMAGE_UP_AFFIX_DUPLICATE",
                    join,
                    "affix_damage_up may not appear more than once.");
                return false;
            }

            if (ShouldApplyLiHuoBuild2(join, qualifiedState))
            {
                additiveBasisPoints = checked(
                    additiveBasisPoints + LiHuoBuild2BasisPoints);
                contributions.Add(PercentContribution(
                    ItemCombatEffectContributionSourceKind.Build,
                    LiHuoBuild2EffectId,
                    LiHuoBuild2BasisPoints,
                    1,
                    LiHuoBuild2BasisPoints,
                    "QUALIFIED_BUILD_V1"));
            }

            long multiplier = checked(10000L + additiveBasisPoints);
            long resolved = checked(baseDamage * multiplier) / 10000L;
            string[] activeCoreEffectIds = join.CoreItem.CoreEffectRows
                .Where(value =>
                    value.activeFact ==
                        ItemCoreEffectBooleanFact.KnownTrue)
                .Select(value => value.candidateDefinitionId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] effectIds = contributions
                .Select(value => value.sourceEffectId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            request = new ItemCombatEffectRequestRow(
                "itemcombat:" + join.Projection.itemInstanceId + ":"
                + join.Placement.placementId + ":direct_flat_damage",
                ItemCombatEffectRequestKind.DirectFlatDamage,
                join.Projection.itemInstanceId,
                join.Projection.baseItemId,
                join.Placement.placementId,
                join.Projection.rarity,
                join.Projection.rootSeed,
                join.CatalogItem.faMenTag,
                join.CatalogItem.qiLeiTag,
                effectIds,
                baseDamage,
                additiveBasisPoints,
                resolved,
                ItemCombatEffectMagnitudeCompleteness.Complete,
                true,
                ItemCombatEffectTargetRequestKind
                    .SingleHostileDamageableRuntimeActor,
                join.QualifiedItem.isLitFact,
                join.QualifiedItem.sourceIsCountedFact,
                join.QualifiedItem.qualifiedIsCountedFact,
                join.QualifiedItem.eligibleFaMenBuildId,
                join.QualifiedItem.faMenBuildCount ?? 0,
                join.QualifiedItem.faMenActiveStagePieceCount ?? 0,
                activeCoreEffectIds,
                ItemCombatEffectRequestCanonical.Sha256(
                    join.Projection.BuildCanonicalSignature()),
                BuildPlacementCanonicalFacts(join),
                contributions);
            return true;
        }

        private static ItemCombatEffectRequestContribution
            PercentContribution(
                ItemCombatEffectContributionSourceKind sourceKind,
                string sourceEffectId,
                long rawUnits,
                int stackCount,
                long appliedBasisPoints,
                string sourceDataMaturity)
        {
            return new ItemCombatEffectRequestContribution(
                sourceKind,
                sourceEffectId,
                "AddPercent",
                DamageStatId,
                "basisPoint",
                rawUnits,
                stackCount,
                appliedBasisPoints,
                ItemCombatEffectContributionDisposition.AppliedToRequest,
                sourceDataMaturity);
        }

        private static int CountAdjacentSameFaMen(
            ItemCombatEffectRequestBoardJoin source,
            ItemSystemSnapshot itemSystem)
        {
            if (string.IsNullOrWhiteSpace(source.CatalogItem.faMenTag))
            {
                return 0;
            }
            Dictionary<string, ItemSystemCatalogItemSnapshot> catalog =
                itemSystem.catalogItems
                    .Where(value => value != null
                        && !string.IsNullOrWhiteSpace(value.itemId))
                    .GroupBy(value => value.itemId, StringComparer.Ordinal)
                    .Where(group => group.Count() == 1)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Single(),
                        StringComparer.Ordinal);
            int distinct = 0;
            foreach (ItemSystemPlacementSnapshot other
                     in itemSystem.placements.Where(value =>
                         value != null
                         && !ReferenceEquals(value, source.Placement)
                         && !string.Equals(
                             value.placementId,
                             source.Placement.placementId,
                             StringComparison.Ordinal)
                         && !string.Equals(
                             value.itemId, "I031", StringComparison.Ordinal)))
            {
                if (!catalog.TryGetValue(
                        other.itemId,
                        out ItemSystemCatalogItemSnapshot otherCatalog)
                    || !string.Equals(
                        otherCatalog.faMenTag,
                        source.CatalogItem.faMenTag,
                        StringComparison.Ordinal))
                {
                    continue;
                }
                bool sharesEdge = source.Placement.OccupiedCells.Any(left =>
                    other.OccupiedCells.Any(right =>
                        Math.Abs(left.x - right.x)
                        + Math.Abs(left.y - right.y) == 1));
                if (sharesEdge)
                {
                    distinct++;
                }
            }
            return Math.Min(distinct, AdjacentDamageStackLimit);
        }

        private static bool ShouldApplyLiHuoBuild2(
            ItemCombatEffectRequestBoardJoin join,
            ItemInstanceQualifiedBuildStateSnapshot state)
        {
            ItemInstanceQualifiedBuildTrackSnapshot track =
                state.FindFaMenTrack(LiHuoBuildId);
            return join.QualifiedItem.location ==
                    ItemInstanceQualifiedBuildLocation.Board
                && join.QualifiedItem.qualifiedIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && join.QualifiedItem.contributesToFaMen ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && string.Equals(
                    join.QualifiedItem.eligibleFaMenBuildId,
                    LiHuoBuildId,
                    StringComparison.Ordinal)
                && track != null
                && track.activeStagePieceCount >= 2;
        }

        private static void BuildLiHuoUnsupportedTelemetry(
            ItemCombatEffectRequestBoardJoin join,
            ItemInstanceQualifiedBuildStateSnapshot state,
            ICollection<ItemCombatEffectUnsupportedTelemetry> telemetry)
        {
            bool contributor =
                join.QualifiedItem.location ==
                    ItemInstanceQualifiedBuildLocation.Board
                && join.QualifiedItem.qualifiedIsCountedFact ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && join.QualifiedItem.contributesToFaMen ==
                    ItemInstanceQualifiedBuildBooleanFact.True
                && string.Equals(
                    join.QualifiedItem.eligibleFaMenBuildId,
                    LiHuoBuildId,
                    StringComparison.Ordinal);
            if (!contributor)
            {
                return;
            }
            ItemInstanceQualifiedBuildTrackSnapshot track =
                state.FindFaMenTrack(LiHuoBuildId);
            int activeStage = track?.activeStagePieceCount ?? 0;
            telemetry.Add(Telemetry(
                join,
                LiHuoBuild4EffectId,
                ItemCombatEffectUnsupportedSourceKind.Build,
                "ExtraTrigger",
                "trigger",
                ItemCombatEffectMagnitudePresence.Missing,
                null,
                ItemCombatEffectUnsupportedDisposition.NotExecuted,
                activeStage >= 4
                    ? "BATTLE_TRIGGER_LEDGER_NOT_OWNED_BY_ITEM_V1"
                    : "BUILD4_STAGE_INACTIVE",
                new[]
                {
                    "buildId=" + LiHuoBuildId,
                    "qualifiedItemCount="
                    + (track?.qualifiedItemCount ?? 0).ToString(
                        CultureInfo.InvariantCulture),
                    "activeStagePieceCount="
                    + activeStage.ToString(CultureInfo.InvariantCulture)
                }));
            telemetry.Add(Telemetry(
                join,
                LiHuoBuild6EffectId,
                ItemCombatEffectUnsupportedSourceKind.Build,
                "Unknown",
                "unknown",
                ItemCombatEffectMagnitudePresence.Missing,
                null,
                activeStage >= 6
                    ? ItemCombatEffectUnsupportedDisposition.Unknown
                    : ItemCombatEffectUnsupportedDisposition.NotExecuted,
                activeStage >= 6
                    ? "BUILD6_MACHINE_CONTRACT_NOT_APPROVED_V1"
                    : "BUILD6_STAGE_INACTIVE",
                new[]
                {
                    "buildId=" + LiHuoBuildId,
                    "qualifiedItemCount="
                    + (track?.qualifiedItemCount ?? 0).ToString(
                        CultureInfo.InvariantCulture),
                    "activeStagePieceCount="
                    + activeStage.ToString(CultureInfo.InvariantCulture)
                }));
            telemetry.Add(Telemetry(
                join,
                "famen:lihuo:text:burn",
                ItemCombatEffectUnsupportedSourceKind.TextSemantic,
                "UnknownTextSemantic",
                "unknown",
                ItemCombatEffectMagnitudePresence.Missing,
                null,
                ItemCombatEffectUnsupportedDisposition.Unknown,
                "TEXT_ONLY_BURN_SEMANTICS_NOT_EXECUTED_V1",
                new[]
                {
                    "buildId=" + LiHuoBuildId,
                    "textInferenceForbidden=true"
                }));
        }

        private static void BuildCoreTelemetry(
            ItemCombatEffectRequestBoardJoin join,
            ICollection<ItemCombatEffectUnsupportedTelemetry> telemetry)
        {
            foreach (ItemInstanceCoreEffectRuntimeRow row
                     in join.CoreItem.CoreEffectRows)
            {
                List<string> identities = new()
                {
                    "candidateDefinitionId=" + row.candidateDefinitionId,
                    "awakeningNodeId=" + row.awakeningNodeId,
                    "nodeKind=" + row.nodeKind,
                    "eligibleFact=" + row.eligibleFact,
                    "visibleFact=" + row.visibleFact,
                    "unlockedFact=" + row.unlockedFact,
                    "litFact=" + row.litFact,
                    "activeFact=" + row.activeFact
                };
                identities.AddRange(row.SourceIdentities);
                telemetry.Add(Telemetry(
                    join,
                    row.candidateDefinitionId,
                    ItemCombatEffectUnsupportedSourceKind.Core,
                    "UnapprovedCoreEffect",
                    "unknown",
                    ItemCombatEffectMagnitudePresence.Missing,
                    null,
                    ItemCombatEffectUnsupportedDisposition.Unknown,
                    "CORE_MAGNITUDE_NOT_APPROVED_IN_ITEM_REQUEST_CONTRACT_V1",
                    identities));
            }
        }

        private static ItemCombatEffectUnsupportedTelemetry Telemetry(
            ItemCombatEffectRequestBoardJoin join,
            string effectId,
            ItemCombatEffectUnsupportedSourceKind sourceKind,
            string operation,
            string unit,
            ItemCombatEffectMagnitudePresence presence,
            long? magnitude,
            ItemCombatEffectUnsupportedDisposition disposition,
            string reason,
            IEnumerable<string> identities)
        {
            return new ItemCombatEffectUnsupportedTelemetry(
                join.Projection.itemInstanceId,
                join.Projection.baseItemId,
                join.Placement.placementId,
                effectId,
                sourceKind,
                operation,
                unit,
                presence,
                magnitude,
                disposition,
                reason,
                identities);
        }

        private static IEnumerable<string> AffixIdentities(
            ItemInstanceProjectionAffixSnapshot affix)
        {
            return new[]
            {
                "slotId=" + affix.slotId,
                "slotKind=" + affix.slotKind,
                "affixId=" + affix.affixId,
                "affixValueProfileId=" + affix.affixValueProfileId
            };
        }

        private static string BuildPlacementCanonicalFacts(
            ItemCombatEffectRequestBoardJoin join)
        {
            StringBuilder builder = new StringBuilder();
            ItemCombatEffectRequestCanonical.Field(
                builder, "itemInstanceId", join.Projection.itemInstanceId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "placementId", join.Placement.placementId);
            ItemCombatEffectRequestCanonical.Field(
                builder, "baseItemId", join.Placement.itemId);
            ItemCombatEffectRequestCanonical.IntField(
                builder, "anchor.x", join.Placement.anchorCell.x);
            ItemCombatEffectRequestCanonical.IntField(
                builder, "anchor.y", join.Placement.anchorCell.y);
            ItemCombatEffectRequestCanonical.IntField(
                builder, "rotation", join.Placement.rotation);
            foreach (UnityEngine.Vector2Int cell
                     in join.Placement.OccupiedCells
                         .OrderBy(value => value.y)
                         .ThenBy(value => value.x))
            {
                ItemCombatEffectRequestCanonical.Field(
                    builder,
                    "occupiedCell",
                    cell.x.ToString(CultureInfo.InvariantCulture)
                    + ","
                    + cell.y.ToString(CultureInfo.InvariantCulture));
            }
            ItemCombatEffectRequestCanonical.Field(
                builder, "isLit",
                join.Placement.isLit ? "true" : "false");
            ItemCombatEffectRequestCanonical.Field(
                builder, "sourceIsCounted",
                join.Placement.isCountedInBuild ? "true" : "false");
            ItemCombatEffectRequestCanonical.Field(
                builder, "qualifiedIsCounted",
                join.QualifiedItem.qualifiedIsCountedFact.ToString());
            return builder.ToString();
        }

        private static void Invalid(
            ICollection<ItemCombatEffectRequestValidationError> errors,
            string code,
            ItemCombatEffectRequestBoardJoin join,
            string message)
        {
            errors.Add(new ItemCombatEffectRequestValidationError(
                code,
                ItemCombatEffectRequestSnapshotStatus.Invalid,
                join.Projection.itemInstanceId,
                join.Placement.placementId,
                message));
        }

        private static ItemCombatEffectRequestSnapshot Snapshot(
            ItemCombatEffectRequestInputValidationResult validation,
            IEnumerable<ItemCombatEffectRequestRow> requests,
            IEnumerable<ItemCombatEffectUnsupportedTelemetry> telemetry,
            IEnumerable<ItemCombatEffectRequestValidationError> errors,
            ItemCombatEffectRequestSnapshotStatus? overrideStatus = null)
        {
            return new ItemCombatEffectRequestSnapshot(
                overrideStatus ?? validation.status,
                validation.sourceProjectionSetCanonicalSignature,
                validation.sourceQualifiedProjectionSetIdentity,
                validation.sourceBindingCanonicalSignature,
                validation.sourceItemSystemCanonicalSignature,
                validation.sourceQualifiedBuildCanonicalSignature,
                validation.sourceCoreRuntimeCanonicalSignature,
                requests,
                telemetry,
                errors);
        }
    }
}
