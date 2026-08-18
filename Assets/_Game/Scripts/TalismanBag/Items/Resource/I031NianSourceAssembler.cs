using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;

namespace TalismanBag.Items.Resource
{
    public static class I031NianSourceAssembler
    {
        private static readonly I031NianCostRequestFact[] CostFacts =
        {
            new I031NianCostRequestFact("I007", 2),
            new I031NianCostRequestFact("I008", 3),
            new I031NianCostRequestFact("I009", 5),
            new I031NianCostRequestFact("I010", 3),
            new I031NianCostRequestFact("I011", 3),
            new I031NianCostRequestFact("I012", 6)
        };

        public static I031NianSourceSnapshot Assemble(
            ItemSystemSnapshot itemSystemSnapshot,
            int sourceGeneration)
        {
            List<string> errors = new List<string>();
            if (sourceGeneration <= 0)
            {
                errors.Add("SOURCE_GENERATION_INVALID");
            }

            if (itemSystemSnapshot == null)
            {
                errors.Add("ITEM_SYSTEM_SNAPSHOT_MISSING");
                return BuildInvalid(sourceGeneration, errors);
            }

            if (!string.Equals(
                    itemSystemSnapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal))
            {
                errors.Add("ITEM_SYSTEM_SCHEMA_NOT_V2");
            }

            if (!itemSystemSnapshot.isValid)
            {
                errors.Add("ITEM_SYSTEM_SNAPSHOT_INVALID");
            }

            I031InventoryPlacementStateSnapshot state = itemSystemSnapshot.i031State;
            if (state == null)
            {
                errors.Add("I031_STATE_MISSING");
            }
            else
            {
                if (!string.Equals(state.itemId, I031InventoryPlacementContract.ItemId,
                        StringComparison.Ordinal))
                {
                    errors.Add("I031_ITEM_ID_MISMATCH");
                }

                if (!string.Equals(state.specialIdentityId,
                        I031InventoryPlacementContract.SpecialIdentityId,
                        StringComparison.Ordinal))
                {
                    errors.Add("I031_SPECIAL_IDENTITY_MISMATCH");
                }

                if (!string.Equals(state.stablePlacementId,
                        I031InventoryPlacementContract.StablePlacementId,
                        StringComparison.Ordinal))
                {
                    errors.Add("I031_STABLE_PLACEMENT_MISMATCH");
                }

                if (state.ownershipCompleteness != I031OwnershipCompleteness.Complete)
                {
                    errors.Add("I031_OWNERSHIP_INCOMPLETE");
                }

                if (!state.isOwned)
                {
                    errors.Add("I031_NOT_OWNED");
                }

                if (state.location != I031Location.Board)
                {
                    errors.Add("I031_NOT_ON_BOARD");
                }
            }

            ItemSystemPlacementSnapshot[] placements = itemSystemSnapshot.placements
                .Where(value => value != null && string.Equals(
                    value.itemId,
                    I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal))
                .ToArray();
            if (placements.Length != 1)
            {
                errors.Add("I031_PLACEMENT_COUNT_NOT_ONE");
            }

            ItemSystemPlacementSnapshot placement = placements.Length == 1 ? placements[0] : null;
            if (placement != null)
            {
                if (!string.Equals(
                        placement.placementId,
                        I031InventoryPlacementContract.StablePlacementId,
                        StringComparison.Ordinal))
                {
                    errors.Add("I031_PLACEMENT_ID_MISMATCH");
                }

                if (!placement.isLightingSource)
                {
                    errors.Add("I031_NOT_LIGHTING_SOURCE");
                }
            }

            bool eligible = errors.Count == 0;
            return new I031NianSourceSnapshot(
                sourceGeneration,
                itemSystemSnapshot.schemaVersion,
                itemSystemSnapshot.BuildDebugSignature(),
                state?.isOwned == true,
                state?.location ?? I031Location.Unknown,
                placement?.isLightingSource == true,
                eligible,
                CostFacts,
                errors);
        }

        private static I031NianSourceSnapshot BuildInvalid(
            int sourceGeneration,
            IReadOnlyList<string> errors)
        {
            return new I031NianSourceSnapshot(
                sourceGeneration,
                string.Empty,
                string.Empty,
                false,
                I031Location.Unknown,
                false,
                false,
                CostFacts,
                errors);
        }
    }
}
