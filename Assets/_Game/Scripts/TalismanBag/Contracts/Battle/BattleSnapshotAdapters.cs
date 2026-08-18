using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.Contracts.Battle
{
    public static class V04BattleLayoutNormalizer
    {
        public const string DefaultBoardId = "v04_buildsandbox_backpack_5x5";

        public static BattleLayoutSnapshot FromBuildSandboxLayout(
            BuildSandboxLayoutSnapshot source,
            string snapshotId = "",
            string boardId = DefaultBoardId,
            int gridWidth = 5,
            int gridHeight = 5)
        {
            BattleLayoutSnapshot snapshot = new()
            {
                snapshotId = string.IsNullOrWhiteSpace(snapshotId) ? "v04_buildsandbox_layout" : snapshotId.Trim(),
                boardId = string.IsNullOrWhiteSpace(boardId) ? DefaultBoardId : boardId.Trim(),
                gridWidth = Math.Max(0, gridWidth),
                gridHeight = Math.Max(0, gridHeight),
                createdAtFrame = Time.frameCount,
                devOnly = true,
                sourceAdapter = nameof(V04BattleLayoutNormalizer),
                legacySingleCell = false
            };

            BackpackLayoutConfig layout = BackpackLayoutConfig.CreateDefault();
            EyeConfig eye = EyeConfig.CreateDefault();
            snapshot.formationEyes.Add(new BattleFormationEyeSnapshot
            {
                eyeId = eye.eyeId,
                cell = ToCell(eye.eyeCell),
                energyState = BattleContractEnergyState.WeakPulse,
                stability = eye.eyeStability,
                devOnly = true
            });

            foreach (ItemShapeCell blocked in layout.blockedCells ?? new List<ItemShapeCell>())
            {
                snapshot.blockedCells.Add(ToCell(blocked));
            }

            foreach (BuildSandboxPlacedItemSnapshot item in source?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                BattleItemSnapshot mapped = MapItem(item);
                snapshot.placedItems.Add(mapped);
                snapshot.energyDiagnostics.AddRange(item.energyDiagnostics ?? new List<string>());

                if (item.isEnergyStoneSource || string.Equals(item.formalEnergySourceItemId, item.itemId, StringComparison.Ordinal))
                {
                    snapshot.energySources.Add(new BattleEnergySourceSnapshot
                    {
                        energySourceId = FirstNonEmpty(item.energySourceId, item.itemId),
                        itemId = Normalize(item.itemId),
                        cell = mapped.anchorCell,
                        energyState = mapped.energyState,
                        suppliedItemIds = CollectSuppliedItems(source, item.itemId),
                        devOnly = true
                    });
                }
            }

            snapshot.currentEnergyState = ResolveLayoutEnergyState(snapshot.placedItems);
            snapshot.blockedCells = NormalizeCells(snapshot.blockedCells);
            snapshot.lockedCells = NormalizeCells(snapshot.lockedCells);
            return snapshot;
        }

        public static BattleItemSnapshot MapItem(BuildSandboxPlacedItemSnapshot item)
        {
            BuildSandboxItemIdentityFamilyRecord identity =
                BuildSandboxItemIdentityFamilyCatalog.Resolve(item?.itemId);
            List<BattleGridCell> occupiedCells = NormalizeCells(
                (item?.occupiedCells ?? new List<ItemShapeCell>()).Select(ToCell));
            BattleGridCell anchor = item == null
                ? default
                : ToCell(item.anchorCell);
            if (occupiedCells.Count > 0 && !occupiedCells.Contains(anchor))
            {
                anchor = occupiedCells[0];
            }

            BuildSandboxItemStat stat =
                BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
            BattleContractEnergyState energyState = MapEnergyState(item?.energyState ?? EnergyState.None);
            string displayName = string.IsNullOrWhiteSpace(identity.DisplayName)
                ? Normalize(item?.itemId)
                : identity.DisplayName;

            BattleItemSnapshot snapshot = new()
            {
                itemInstanceId = Normalize(item?.itemId),
                itemId = Normalize(item?.itemId),
                displayName = displayName,
                familyId = Normalize(item?.itemFamily),
                baseItemId = Normalize(item?.baseItemId),
                shapeId = Normalize(item?.shapeId),
                rotationIndex = (int)(item?.rotation ?? ItemShapeRotation.Rotation0),
                anchorCell = anchor,
                occupiedCells = occupiedCells,
                itemStats = MapStats(stat),
                affixes = Clean(item?.affixList),
                rarity = Normalize(item?.rarity),
                energyState = energyState,
                connectedEyeId = Normalize(item?.connectedEyeId),
                energySourceId = Normalize(item?.energySourceId),
                sourceContainer = "v04_buildsandbox_layout",
                synergyTags = Clean(item?.tags),
                legacySingleCell = false,
                runtimeId = string.Empty,
                sourceDataPath = "BuildSandboxLayoutSnapshot.placedItems[]",
                energyRoleViolation = item?.hasEnergyRoleViolation == true,
                developerOnlyDiagnostics = Clean(item?.energyDiagnostics)
            };

            if (!string.IsNullOrWhiteSpace(item?.formalEnergySourceItemId))
            {
                snapshot.developerOnlyDiagnostics.Add("formalEnergySourceItemId=" + item.formalEnergySourceItemId.Trim());
            }

            return snapshot;
        }

        public static BattleContractEnergyState MapEnergyState(EnergyState state)
        {
            return state switch
            {
                EnergyState.None => BattleContractEnergyState.None,
                EnergyState.WeakPulse => BattleContractEnergyState.WeakPulse,
                EnergyState.Powered => BattleContractEnergyState.Powered,
                EnergyState.Suppressed => BattleContractEnergyState.Suppressed,
                _ => BattleContractEnergyState.Unknown
            };
        }

        private static BattleItemStatsSnapshot MapStats(BuildSandboxItemStat stat)
        {
            BuildSandboxItemStat safeStat = stat ?? new BuildSandboxItemStat();
            return new BattleItemStatsSnapshot
            {
                statProfileId = Normalize(safeStat.statProfileId),
                attack = safeStat.attack,
                guard = safeStat.guard,
                spirit = safeStat.spirit,
                control = safeStat.control,
                shieldBreak = safeStat.shieldBreak,
                cleanse = safeStat.cleanse,
                manaGainPerTick = safeStat.manaGainPerTick,
                manaCostPerCast = safeStat.manaCostPerCast,
                castIntervalSeconds = safeStat.castIntervalSeconds
            };
        }

        private static BattleContractEnergyState ResolveLayoutEnergyState(
            IReadOnlyList<BattleItemSnapshot> items)
        {
            if (items == null || items.Count == 0)
            {
                return BattleContractEnergyState.None;
            }

            if (items.Any(item => item != null && item.energyState == BattleContractEnergyState.Suppressed))
            {
                return BattleContractEnergyState.Suppressed;
            }

            if (items.Any(item => item != null && item.energyState == BattleContractEnergyState.Powered))
            {
                return BattleContractEnergyState.Powered;
            }

            return items.Any(item => item != null && item.energyState == BattleContractEnergyState.WeakPulse)
                ? BattleContractEnergyState.WeakPulse
                : BattleContractEnergyState.None;
        }

        private static List<string> CollectSuppliedItems(BuildSandboxLayoutSnapshot source, string sourceItemId)
        {
            string normalizedSource = Normalize(sourceItemId);
            return Clean((source?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>())
                .Where(item => item != null && string.Equals(item.formalEnergySourceItemId, normalizedSource, StringComparison.Ordinal))
                .Select(item => item.itemId));
        }

        private static BattleGridCell ToCell(ItemShapeCell cell)
        {
            return new BattleGridCell(cell.x, cell.y);
        }

        private static List<BattleGridCell> NormalizeCells(IEnumerable<BattleGridCell> cells)
        {
            List<BattleGridCell> result = new();
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (BattleGridCell cell in cells ?? Enumerable.Empty<BattleGridCell>())
            {
                string key = cell.x + ":" + cell.y;
                if (seen.Add(key))
                {
                    result.Add(cell);
                }
            }

            result.Sort((left, right) =>
            {
                int compareX = left.x.CompareTo(right.x);
                return compareX != 0 ? compareX : left.y.CompareTo(right.y);
            });
            return result;
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return (values ?? Array.Empty<string>())
                .Select(Normalize)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
