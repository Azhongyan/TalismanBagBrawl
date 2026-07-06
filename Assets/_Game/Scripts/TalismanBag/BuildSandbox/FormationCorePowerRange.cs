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
        public List<ItemShapeCell> weakPulseCells = new();
        public List<FormationCorePowerRangeRow> rows = new();

        public bool EyeCellExists => coreCell.x >= 0 && coreCell.y >= 0 && coreCell.x < boardWidth && coreCell.y < boardHeight;
        public int EyeCellOccupiedCount => rows?.Count(row => row != null && row.touchesFormationCore) ?? 0;
        public int WeakPulseCellCount => weakPulseCells?.Distinct().Count() ?? 0;
        public int WeakPulseItemCount => rows?.Count(row => row != null && row.energyState == EnergyState.WeakPulse) ?? 0;
        public int ProviderCount => rows?.Count(row => row != null && row.isPowerProvider) ?? 0;
        public int PoweredItemCount => rows?.Count(row => row != null && row.isPowered) ?? 0;
        public int UnpoweredItemCount => rows?.Count(row => row != null && !row.isPowered && !row.isPowerProvider) ?? 0;
        public int CoreTouchCount => rows?.Count(row => row != null && row.touchesFormationCore) ?? 0;
        public int PowerRangeCellCount => rows?
            .SelectMany(row => row?.powerRangeCells ?? new List<ItemShapeCell>())
            .Distinct()
            .Count() ?? 0;
        public int ForbiddenProviderCandidateCount => rows?
            .Count(row => row != null && row.hasEnergyRoleViolation) ?? 0;
        public int ForbiddenProviderMisidentifiedCount => rows?
            .Count(row => row != null && row.hasEnergyRoleViolation && row.isPowerProvider) ?? 0;
        public int TagProviderViolationCount => rows?
            .Count(row => row != null
                && (row.energyDiagnostics ?? new List<string>())
                    .Contains(FormationEnergyDiagnosticCodes.TagProviderViolation)) ?? 0;
        public int TagAutoPowerProviderCount => rows?
            .Count(row => row != null
                && row.isPowerProvider
                && (row.energyDiagnostics ?? new List<string>())
                    .Contains(FormationEnergyDiagnosticCodes.TagProviderViolation)) ?? 0;
        public int LegacyProviderViolationCount => rows?
            .Count(row => row != null
                && (row.energyDiagnostics ?? new List<string>())
                    .Contains(FormationEnergyDiagnosticCodes.LegacyProviderViolation)) ?? 0;
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
        public EnergyState energyState = EnergyState.None;
        public bool isPowerProvider;
        public bool isPowered;
        public bool touchesFormationCore;
        public string energySourceId = string.Empty;
        public string connectedEyeId = string.Empty;
        public string formalEnergySourceItemId = string.Empty;
        public bool isEnergyStoneSource;
        public bool isInBasePulseRange;
        public bool isEyeAdjacent;
        public bool hasEnergyRoleViolation;
        public int powerRangeRadius;
        public List<ItemShapeCell> occupiedCells = new();
        public List<ItemShapeCell> powerRangeCells = new();
        public List<string> energyDiagnostics = new();
        public string powerConnectionState = string.Empty;
        public string energyStateReason = string.Empty;
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
            FormationEnergyContractPreview energyPreview =
                FormationEnergyContractResolver.Apply(snapshot);
            FormationCorePowerRangePreview preview = new()
            {
                devOnly = true,
                isEnabled = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                touchesFormalScene = false,
                opensFeatureFlag = false,
                boardWidth = energyPreview?.layoutConfig?.width ?? BoardWidth,
                boardHeight = energyPreview?.layoutConfig?.height ?? BoardHeight,
                coreCell = energyPreview?.eyeConfig?.eyeCell ?? DefaultCoreCell,
                weakPulseCells = NormalizeCells(energyPreview?.eyeRuntimeData?.basePulseCells),
                rows = new List<FormationCorePowerRangeRow>()
            };

            List<BuildSandboxPlacedItemSnapshot> items = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                preview.rows.Add(BuildRow(item));
            }

            return preview;
        }

        public static bool IsPowerProvider(BuildSandboxPlacedItemSnapshot item)
        {
            return item != null
                && item.energyState == EnergyState.Powered
                && FormationEnergyContractResolver.IsEnergyStoneItem(item);
        }

        public static int ResolvePowerRadius(BuildSandboxPlacedItemSnapshot item)
        {
            return FormationEnergyContractResolver.IsEnergyStoneItem(item)
                ? EnergyStoneConfig.CreateDefault(item?.itemId).supplyRange
                : 0;
        }

        private static FormationCorePowerRangeRow BuildRow(BuildSandboxPlacedItemSnapshot item)
        {
            return new FormationCorePowerRangeRow
            {
                itemId = item?.itemId ?? string.Empty,
                shapeId = item?.shapeId ?? string.Empty,
                energyState = item?.energyState ?? EnergyState.None,
                isPowerProvider = IsPowerProvider(item),
                isPowered = item?.energyState == EnergyState.Powered,
                touchesFormationCore = item?.touchesFormationCore ?? false,
                energySourceId = item?.energySourceId ?? string.Empty,
                connectedEyeId = item?.connectedEyeId ?? string.Empty,
                formalEnergySourceItemId = item?.formalEnergySourceItemId ?? string.Empty,
                isEnergyStoneSource = item?.isEnergyStoneSource ?? false,
                isInBasePulseRange = item?.isInBasePulseRange ?? false,
                isEyeAdjacent = item?.isEyeAdjacent ?? false,
                hasEnergyRoleViolation = item?.hasEnergyRoleViolation ?? false,
                powerRangeRadius = item?.powerRangeRadius ?? 0,
                occupiedCells = NormalizeCells(item?.occupiedCells),
                powerRangeCells = NormalizeCells(item?.powerRangeCells),
                energyDiagnostics = new List<string>(item?.energyDiagnostics ?? new List<string>()),
                powerConnectionState = item?.powerConnectionState ?? string.Empty,
                energyStateReason = item?.energyStateReason ?? string.Empty,
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
                return "\u805a\u80fd\u4f9b\u80fd";
            }

            if (item.energyState == EnergyState.Powered)
            {
                return "\u805a\u80fd\u4f9b\u80fd";
            }

            if (item.energyState == EnergyState.WeakPulse)
            {
                return "\u9635\u8109\u5fae\u4eae";
            }

            if (item.energyState == EnergyState.Suppressed)
            {
                return "\u88ab\u538b\u5236";
            }

            return "\u672a\u4f9b\u80fd";
        }

        private static string ResolvePlayerFeedback(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (IsPowerProvider(item) && item.touchesFormationCore)
            {
                return "\u9635\u52bf\u53d7\u538b\uff0c\u7075\u529b\u88ab\u6270\u4e71\u3002";
            }

            if (IsPowerProvider(item))
            {
                return "\u805a\u80fd\u77f3\u8fde\u901a\uff0c\u7b26\u9635\u4f9b\u80fd\u7a33\u5b9a\u3002";
            }

            if (item.energyState == EnergyState.Powered)
            {
                return "\u805a\u80fd\u77f3\u8fde\u901a\uff0c\u7b26\u9635\u4f9b\u80fd\u7a33\u5b9a\u3002";
            }

            if (item.energyState == EnergyState.WeakPulse)
            {
                return "\u9635\u773c\u5fae\u4eae\uff0c\u7b26\u9635\u4ec5\u88ab\u5f31\u6fc0\u6d3b\u3002";
            }

            if (item.energyState == EnergyState.Suppressed)
            {
                return "\u9635\u52bf\u53d7\u538b\uff0c\u7075\u529b\u88ab\u6270\u4e71\u3002";
            }

            return "\u4f9b\u80fd\u65ad\u5f00\uff0c\u90e8\u5206\u9053\u5177\u6c89\u5bc2\u3002";
        }

        private sealed class PowerProvider
        {
            public BuildSandboxPlacedItemSnapshot Source;
            public int Radius;
            public List<ItemShapeCell> Cells = new();
        }
    }
}
