using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.Items
{
    public enum I031OwnershipCompleteness
    {
        Unknown = 0,
        Complete = 1
    }

    public enum I031Location
    {
        Unknown = 0,
        Inventory = 1,
        Board = 2
    }

    public sealed class I031InventoryPlacementStateInput
    {
        public I031InventoryPlacementStateInput(
            string itemId,
            string specialIdentityId,
            string stablePlacementId,
            I031OwnershipCompleteness ownershipCompleteness,
            I031Location location,
            bool isOwned)
        {
            this.itemId = itemId ?? string.Empty;
            this.specialIdentityId = specialIdentityId ?? string.Empty;
            this.stablePlacementId = stablePlacementId ?? string.Empty;
            this.ownershipCompleteness = ownershipCompleteness;
            this.location = location;
            this.isOwned = isOwned;
        }

        public string itemId { get; }
        public string specialIdentityId { get; }
        public string stablePlacementId { get; }
        public I031OwnershipCompleteness ownershipCompleteness { get; }
        public I031Location location { get; }
        public bool isOwned { get; }
        public bool isPlaced => location == I031Location.Board;

        internal I031InventoryPlacementStateInput Clone()
        {
            return new I031InventoryPlacementStateInput(
                itemId,
                specialIdentityId,
                stablePlacementId,
                ownershipCompleteness,
                location,
                isOwned);
        }
    }

    public sealed class I031InventoryPlacementStateSnapshot
    {
        public I031InventoryPlacementStateSnapshot(
            string itemId,
            string specialIdentityId,
            string stablePlacementId,
            I031OwnershipCompleteness ownershipCompleteness,
            I031Location location,
            bool isOwned)
        {
            this.itemId = itemId ?? string.Empty;
            this.specialIdentityId = specialIdentityId ?? string.Empty;
            this.stablePlacementId = stablePlacementId ?? string.Empty;
            this.ownershipCompleteness = ownershipCompleteness;
            this.location = location;
            this.isOwned = isOwned;
        }

        public string itemId { get; }
        public string specialIdentityId { get; }
        public string stablePlacementId { get; }
        public I031OwnershipCompleteness ownershipCompleteness { get; }
        public I031Location location { get; }
        public bool isOwned { get; }
        public bool isPlaced => location == I031Location.Board;

        internal I031InventoryPlacementStateSnapshot Clone()
        {
            return new I031InventoryPlacementStateSnapshot(
                itemId,
                specialIdentityId,
                stablePlacementId,
                ownershipCompleteness,
                location,
                isOwned);
        }
    }

    public static class I031InventoryPlacementContract
    {
        public const string ItemId = "I031";
        public const string SpecialIdentityId = "SPECIAL_I031";
        public const string StablePlacementId = "P_SYSTEM_I031";
        public const string CatalogShapeId = "shape_single_1";

        public static I031InventoryPlacementStateInput OwnedInventory()
        {
            return OwnedAt(I031Location.Inventory);
        }

        public static I031InventoryPlacementStateInput OwnedBoard()
        {
            return OwnedAt(I031Location.Board);
        }

        public static I031InventoryPlacementStateInput OwnedAt(I031Location location)
        {
            return new I031InventoryPlacementStateInput(
                ItemId,
                SpecialIdentityId,
                StablePlacementId,
                I031OwnershipCompleteness.Complete,
                location,
                true);
        }

        public static I031InventoryPlacementStateInput Unknown()
        {
            return new I031InventoryPlacementStateInput(
                ItemId,
                SpecialIdentityId,
                StablePlacementId,
                I031OwnershipCompleteness.Unknown,
                I031Location.Unknown,
                false);
        }

        public static I031InventoryPlacementStateInput
            CreateLegacyBoardStateFromSingleExplicitI031Placement(
                IReadOnlyList<ItemSystemPlacementInput> placements)
        {
            ItemSystemPlacementInput[] rows = (placements ??
                    Array.Empty<ItemSystemPlacementInput>())
                .Where(value => value != null && string.Equals(
                    value.itemId, ItemId, StringComparison.Ordinal))
                .ToArray();
            return rows.Length == 1 && string.Equals(
                    rows[0].placementId,
                    StablePlacementId,
                    StringComparison.Ordinal)
                ? OwnedBoard()
                : Unknown();
        }

        public static I031InventoryPlacementStateInput
            CreateLegacyInventoryStateFromLockedV04CoreGrantFixture(
                bool lockedCoreGrantFixture)
        {
            return lockedCoreGrantFixture ? OwnedInventory() : Unknown();
        }
    }
}
