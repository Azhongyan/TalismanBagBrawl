using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class FormationCorePowerRangePreview
    {
        public const string PackageName = "V0.4-FormationCoreAndPowerRange01";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool touchesFormalScene;
        public bool opensFeatureFlag;
        public int boardWidth = 5;
        public int boardHeight = 5;
        public ItemShapeCell coreCell = FormationCorePowerRangeResolver.DefaultCoreCell;
        public List<FormationCorePowerRangeRow> rows = new();

        public int ProviderCount => rows?.Count(row => row != null && row.isPowerProvider) ?? 0;
        public int PoweredItemCount => rows?.Count(row => row != null && row.isPowered) ?? 0;
        public int UnpoweredItemCount => rows?.Count(row => row != null && !row.isPowered && !row.isPowerProvider) ?? 0;
        public int CoreTouchCount => rows?.Count(row => row != null && row.touchesFormationCore) ?? 0;
        public int PowerRangeCellCount => rows?
            .SelectMany(row => row?.powerRangeCells ?? new List<ItemShapeCell>())
            .Distinct()
            .Count() ?? 0;
        public int ScopeLeakCount => CountScopeLeaks();

        private int CountScopeLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (writesFormalReward) leaks++;
            if (touchesFormalScene) leaks++;
            if (opensFeatureFlag) leaks++;
            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled()) leaks++;
            leaks += rows?.Count(row => row == null || !row.devOnly || row.isEnabled) ?? 1;
            return leaks;
        }
    }

    [Serializable]
    public sealed class FormationCorePowerRangeRow
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public bool isPowerProvider;
        public bool isPowered;
        public bool touchesFormationCore;
        public string energySourceId = string.Empty;
        public int powerRangeRadius;
        public List<ItemShapeCell> occupiedCells = new();
        public List<ItemShapeCell> powerRangeCells = new();
        public string powerConnectionState = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string englishStableKey = string.Empty;
        public string playerFeedbackChinese = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
    }

    public static class FormationCorePowerRangeResolver
    {
        public const string PackageName = FormationCorePowerRangePreview.PackageName;
        public const string FormationCoreId = "v04_center_core_cell";
        public static readonly ItemShapeCell DefaultCoreCell = new(2, 2);

        private const int BoardWidth = 5;
        private const int BoardHeight = 5;
        private const int DefaultProviderRadius = 1;
        private const int CoreProviderRadius = 2;

        private static readonly string[] ProviderTags =
        {
            "energy_preview",
            "energy_source",
            "ju_neng",
            "energy",
            "core_preview",
            "orange_core_preview"
        };

        public static FormationCorePowerRangePreview Apply(BuildSandboxLayoutSnapshot snapshot)
        {
            FormationCorePowerRangePreview preview = new()
            {
                devOnly = true,
                isEnabled = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                touchesFormalScene = false,
                opensFeatureFlag = false,
                boardWidth = BoardWidth,
                boardHeight = BoardHeight,
                coreCell = DefaultCoreCell,
                rows = new List<FormationCorePowerRangeRow>()
            };

            List<BuildSandboxPlacedItemSnapshot> items = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();

            List<PowerProvider> providers = items
                .Where(IsPowerProvider)
                .Select(CreatePowerProvider)
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                ResetPowerFields(item);
                bool touchesCore = OccupiesCell(item, DefaultCoreCell);
                item.touchesFormationCore = touchesCore;
                item.formationCoreId = touchesCore ? FormationCoreId : string.Empty;

                if (IsPowerProvider(item))
                {
                    int radius = ResolvePowerRadius(item);
                    item.isPowered = true;
                    item.energySourceId = item.itemId ?? string.Empty;
                    item.powerRangeRadius = radius;
                    item.powerRangeCells = BuildPowerRangeCells(item, radius);
                    item.powerConnectionState = touchesCore ? "provider_on_core" : "provider_active";
                }
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item == null || item.isPowered)
                {
                    continue;
                }

                PowerProvider provider = providers
                    .Where(candidate => candidate != null && IsInPowerRange(item, candidate))
                    .OrderBy(candidate => MinDistance(item, candidate.Source))
                    .ThenBy(candidate => candidate.Source.itemId, StringComparer.Ordinal)
                    .FirstOrDefault();

                if (provider == null)
                {
                    item.powerConnectionState = "unpowered_out_of_range";
                    continue;
                }

                item.isPowered = true;
                item.energySourceId = provider.Source.itemId ?? string.Empty;
                item.powerRangeRadius = 0;
                item.powerConnectionState = "powered_by_range";
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                preview.rows.Add(BuildRow(item));
            }

            return preview;
        }

        public static bool IsPowerProvider(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return false;
            }

            string itemId = (item.itemId ?? string.Empty).Trim().ToLowerInvariant();
            if (itemId.Contains("spirit_stone")
                || itemId.Contains("energy_incense")
                || itemId.Contains("stone_core"))
            {
                return true;
            }

            return (item.tags ?? new List<string>()).Any(tag =>
                ProviderTags.Any(providerTag =>
                    string.Equals(tag, providerTag, StringComparison.OrdinalIgnoreCase)));
        }

        public static int ResolvePowerRadius(BuildSandboxPlacedItemSnapshot item)
        {
            string itemId = (item?.itemId ?? string.Empty).Trim().ToLowerInvariant();
            if (itemId.Contains("stone_core") || HasTag(item, "core_preview") || HasTag(item, "orange_core_preview"))
            {
                return CoreProviderRadius;
            }

            return DefaultProviderRadius;
        }

        private static FormationCorePowerRangeRow BuildRow(BuildSandboxPlacedItemSnapshot item)
        {
            return new FormationCorePowerRangeRow
            {
                itemId = item?.itemId ?? string.Empty,
                shapeId = item?.shapeId ?? string.Empty,
                isPowerProvider = IsPowerProvider(item),
                isPowered = item?.isPowered ?? false,
                touchesFormationCore = item?.touchesFormationCore ?? false,
                energySourceId = item?.energySourceId ?? string.Empty,
                powerRangeRadius = item?.powerRangeRadius ?? 0,
                occupiedCells = NormalizeCells(item?.occupiedCells),
                powerRangeCells = NormalizeCells(item?.powerRangeCells),
                powerConnectionState = item?.powerConnectionState ?? string.Empty,
                chineseDisplayName = ResolveChineseDisplayName(item),
                englishStableKey = "formationCorePowerRange." + (item?.itemId ?? string.Empty),
                playerFeedbackChinese = ResolvePlayerFeedback(item),
                devOnly = true,
                isEnabled = false
            };
        }

        private static void ResetPowerFields(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return;
            }

            item.isPowered = false;
            item.energySourceId = string.Empty;
            item.touchesFormationCore = false;
            item.formationCoreId = string.Empty;
            item.powerRangeRadius = 0;
            item.powerRangeCells = new List<ItemShapeCell>();
            item.powerConnectionState = string.Empty;
        }

        private static PowerProvider CreatePowerProvider(BuildSandboxPlacedItemSnapshot item)
        {
            int radius = ResolvePowerRadius(item);
            return new PowerProvider
            {
                Source = item,
                Radius = radius,
                Cells = BuildPowerRangeCells(item, radius)
            };
        }

        private static bool IsInPowerRange(BuildSandboxPlacedItemSnapshot item, PowerProvider provider)
        {
            if (item == null || provider?.Source == null)
            {
                return false;
            }

            if (string.Equals(item.itemId, provider.Source.itemId, StringComparison.Ordinal))
            {
                return true;
            }

            HashSet<ItemShapeCell> range = new(provider.Cells ?? new List<ItemShapeCell>());
            return (item.occupiedCells ?? new List<ItemShapeCell>()).Any(range.Contains);
        }

        private static int MinDistance(BuildSandboxPlacedItemSnapshot item, BuildSandboxPlacedItemSnapshot provider)
        {
            int best = int.MaxValue;
            foreach (ItemShapeCell itemCell in item?.occupiedCells ?? new List<ItemShapeCell>())
            {
                foreach (ItemShapeCell providerCell in provider?.occupiedCells ?? new List<ItemShapeCell>())
                {
                    int distance = Math.Abs(itemCell.x - providerCell.x) + Math.Abs(itemCell.y - providerCell.y);
                    if (distance < best)
                    {
                        best = distance;
                    }
                }
            }

            return best == int.MaxValue ? 999 : best;
        }

        private static List<ItemShapeCell> BuildPowerRangeCells(BuildSandboxPlacedItemSnapshot provider, int radius)
        {
            HashSet<ItemShapeCell> cells = new();
            foreach (ItemShapeCell origin in provider?.occupiedCells ?? new List<ItemShapeCell>())
            {
                for (int x = origin.x - radius; x <= origin.x + radius; x++)
                {
                    for (int y = origin.y - radius; y <= origin.y + radius; y++)
                    {
                        ItemShapeCell cell = new(x, y);
                        if (!IsWithinBoard(cell))
                        {
                            continue;
                        }

                        int distance = Math.Abs(origin.x - x) + Math.Abs(origin.y - y);
                        if (distance <= radius)
                        {
                            cells.Add(cell);
                        }
                    }
                }
            }

            return NormalizeCells(cells);
        }

        private static bool OccupiesCell(BuildSandboxPlacedItemSnapshot item, ItemShapeCell target)
        {
            return (item?.occupiedCells ?? new List<ItemShapeCell>()).Any(cell => cell.Equals(target));
        }

        private static bool HasTag(BuildSandboxPlacedItemSnapshot item, string expectedTag)
        {
            return (item?.tags ?? new List<string>()).Any(tag =>
                string.Equals(tag, expectedTag, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsWithinBoard(ItemShapeCell cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < BoardWidth && cell.y < BoardHeight;
        }

        private static List<ItemShapeCell> NormalizeCells(IEnumerable<ItemShapeCell> cells)
        {
            List<ItemShapeCell> result = new();
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (ItemShapeCell cell in cells ?? Enumerable.Empty<ItemShapeCell>())
            {
                string key = cell.x + ":" + cell.y;
                if (seen.Add(key))
                {
                    result.Add(cell);
                }
            }

            result.Sort((left, right) =>
            {
                int compareY = left.y.CompareTo(right.y);
                return compareY != 0 ? compareY : left.x.CompareTo(right.x);
            });
            return result;
        }

        private static string ResolveChineseDisplayName(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (IsPowerProvider(item))
            {
                return "\u4f9b\u80fd\u6e90";
            }

            if (item.isPowered)
            {
                return "\u5df2\u63a5\u7075";
            }

            return "\u672a\u63a5\u7075";
        }

        private static string ResolvePlayerFeedback(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (IsPowerProvider(item) && item.touchesFormationCore)
            {
                return "\u9635\u773c\u88ab\u70b9\u4eae\uff0c\u7075\u529b\u5411\u5916\u6269\u6563";
            }

            if (IsPowerProvider(item))
            {
                return "\u4f9b\u80fd\u7269\u5df2\u5f62\u6210\u7075\u529b\u533a";
            }

            if (item.isPowered)
            {
                return "\u8fd9\u4ef6\u9053\u5177\u63a5\u4e0a\u4e86\u7075\u529b";
            }

            return "\u8fd9\u4ef6\u9053\u5177\u6682\u672a\u63a5\u4e0a\u7075\u529b";
        }

        private sealed class PowerProvider
        {
            public BuildSandboxPlacedItemSnapshot Source;
            public int Radius;
            public List<ItemShapeCell> Cells = new();
        }
    }
}
