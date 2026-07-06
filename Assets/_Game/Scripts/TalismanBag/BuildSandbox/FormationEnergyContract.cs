using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public enum EnergyState
    {
        None = 0,
        WeakPulse = 1,
        Powered = 2,
        Suppressed = 3
    }

    [Serializable]
    public sealed class BackpackLayoutConfig
    {
        public const string DefaultLayoutId = "v04_buildsandbox_backpack_5x5";

        public string layoutId = DefaultLayoutId;
        public int width = 5;
        public int height = 5;
        public string eyeId = EyeConfig.DefaultEyeId;
        public ItemShapeCell eyeCell = new(2, 2);
        public List<ItemShapeCell> blockedCells = new();
        public bool devOnly = true;
        public bool isEnabled;

        public static BackpackLayoutConfig CreateDefault()
        {
            BackpackLayoutConfig config = new();
            config.blockedCells.Add(config.eyeCell);
            return config;
        }
    }

    [Serializable]
    public sealed class EyeConfig
    {
        public const string DefaultEyeId = "v04_center_eye";

        public string eyeId = DefaultEyeId;
        public string layoutId = BackpackLayoutConfig.DefaultLayoutId;
        public ItemShapeCell eyeCell = new(2, 2);
        public int eyeStability = 100;
        public int basePulseRange = 1;
        public int basePulseStrength = 1;
        public int baseWeakSupplyRate = 0;
        public int energyBusCapacity = 1;
        public int connectedEnergySourceLimit = 1;
        public int eyeGuardValue = 0;
        public int pollutionResistance = 0;
        public int overloadLimit = 0;
        public int bossPressureResistance = 0;
        public bool devOnly = true;
        public bool isEnabled;

        public static EyeConfig CreateDefault()
        {
            return new EyeConfig();
        }
    }

    [Serializable]
    public sealed class EyeRuntimeData
    {
        public string eyeId = EyeConfig.DefaultEyeId;
        public EnergyState energyState = EnergyState.WeakPulse;
        public int currentStability = 100;
        public int currentGuardValue;
        public int currentPollution;
        public List<string> connectedEnergySources = new();
        public List<ItemShapeCell> eyeAdjacentCells = new();
        public List<ItemShapeCell> basePulseCells = new();
        public bool isOverloaded;
        public bool isPolluted;
        public bool isSuppressed;
        public bool devOnly = true;
        public bool isEnabled;

        public static EyeRuntimeData CreateDefault(EyeConfig eyeConfig, BackpackLayoutConfig layoutConfig)
        {
            EyeConfig eye = eyeConfig ?? EyeConfig.CreateDefault();
            BackpackLayoutConfig layout = layoutConfig ?? BackpackLayoutConfig.CreateDefault();
            return new EyeRuntimeData
            {
                eyeId = eye.eyeId,
                currentStability = eye.eyeStability,
                currentGuardValue = eye.eyeGuardValue,
                currentPollution = 0,
                eyeAdjacentCells = FormationEnergyContractResolver.BuildCellsWithinRange(
                    eye.eyeCell,
                    1,
                    layout.width,
                    layout.height,
                    includeOrigin: false),
                basePulseCells = FormationEnergyContractResolver.BuildCellsWithinRange(
                    eye.eyeCell,
                    Math.Max(0, eye.basePulseRange),
                    layout.width,
                    layout.height,
                    includeOrigin: false)
            };
        }
    }

    [Serializable]
    public sealed class EnergyStoneConfig
    {
        public string itemId = FormationEnergyContractResolver.DefaultEnergyStoneItemId;
        public int energyOutput = 1;
        public int supplyRange = 1;
        public string supplyShape = "manhattan";
        public bool connectionRequired = true;
        public float disconnectedEfficiency = 0f;
        public float efficiency = 1f;
        public int stability = 100;
        public int targetLimit = 4;
        public string priorityRule = "nearest_then_itemId";
        public int overloadRisk = 0;
        public bool devOnly = true;
        public bool isEnabled;

        public static EnergyStoneConfig CreateDefault(string itemId = FormationEnergyContractResolver.DefaultEnergyStoneItemId)
        {
            EnergyStoneConfig config = new();
            config.itemId = string.IsNullOrWhiteSpace(itemId)
                ? FormationEnergyContractResolver.DefaultEnergyStoneItemId
                : itemId;
            return config;
        }
    }

    [Serializable]
    public sealed class EnergyStoneRuntimeData
    {
        public string itemId = string.Empty;
        public ItemShapeCell placedCell;
        public string connectionState = string.Empty;
        public string connectedEyeId = string.Empty;
        public float currentEfficiency;
        public List<string> suppliedItems = new();
        public bool isStolen;
        public bool isSuppressed;
        public int overloadValue;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class FormationEnergyBattleLayoutSnapshot
    {
        public string sourceSnapshotType = "BuildSandboxLayoutSnapshot";
        public BackpackLayoutConfig backpackLayoutConfig = BackpackLayoutConfig.CreateDefault();
        public EyeConfig eyeConfig = EyeConfig.CreateDefault();
        public EyeRuntimeData eyeRuntimeData = EyeRuntimeData.CreateDefault(EyeConfig.CreateDefault(), BackpackLayoutConfig.CreateDefault());
        public List<EnergyStoneRuntimeData> energyStones = new();
        public List<BuildSandboxPlacedItemSnapshot> placedItems = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool touchesFormalScene;
    }

    [Serializable]
    public sealed class FormationEnergyContractPreview
    {
        public const string PackageName = "V0.4-FormationEnergyContract01";

        public string packageName = PackageName;
        public string guardPass = "GUARD_PASS_FORMATION_ENERGY_CONTRACT01";
        public bool devOnly = true;
        public bool isEnabled;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool touchesFormalScene;
        public bool opensFeatureFlag;
        public BackpackLayoutConfig layoutConfig = BackpackLayoutConfig.CreateDefault();
        public EyeConfig eyeConfig = EyeConfig.CreateDefault();
        public EyeRuntimeData eyeRuntimeData = EyeRuntimeData.CreateDefault(EyeConfig.CreateDefault(), BackpackLayoutConfig.CreateDefault());
        public FormationEnergyBattleLayoutSnapshot battleLayoutSnapshot = new();
        public List<FormationEnergyContractRow> rows = new();
        public List<FormationEnergyDiagnosticRow> diagnostics = new();

        public int PoweredItemCount => rows?.Count(row => row != null && row.energyState == EnergyState.Powered) ?? 0;
        public int WeakPulseItemCount => rows?.Count(row => row != null && row.energyState == EnergyState.WeakPulse) ?? 0;
        public int SuppressedItemCount => rows?.Count(row => row != null && row.energyState == EnergyState.Suppressed) ?? 0;
        public int EnergyStoneProviderCount => rows?.Count(row => row != null && row.isEnergyStoneSource && row.energyState == EnergyState.Powered) ?? 0;
        public int ForbiddenProviderCandidateCount => diagnostics?.Count(row => row != null && row.code == FormationEnergyDiagnosticCodes.ForbiddenProviderCandidate) ?? 0;
        public int TagProviderViolationCount => diagnostics?.Count(row => row != null && row.code == FormationEnergyDiagnosticCodes.TagProviderViolation) ?? 0;
        public int LegacyProviderViolationCount => diagnostics?.Count(row => row != null && row.code == FormationEnergyDiagnosticCodes.LegacyProviderViolation) ?? 0;
        public int EyeCellOccupiedCount => diagnostics?.Count(row => row != null && row.code == FormationEnergyDiagnosticCodes.EyeCellOccupied) ?? 0;
        public int ContractViolationCount => diagnostics?.Count(row => row != null && row.isViolation) ?? 0;
        public int ScopeLeakCount => CountScopeLeaks();

        public bool DevOnlyIsolationPass => ScopeLeakCount == 0;

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
            leaks += diagnostics?.Count(row => row == null || !row.devOnly || row.isEnabled) ?? 1;
            return leaks;
        }
    }

    [Serializable]
    public sealed class FormationEnergyContractRow
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public EnergyState energyState = EnergyState.None;
        public bool isPowered;
        public string energySourceId = string.Empty;
        public string connectedEyeId = string.Empty;
        public string formalEnergySourceItemId = string.Empty;
        public bool isEnergyStoneSource;
        public bool isInBasePulseRange;
        public bool isEyeAdjacent;
        public bool touchesEyeCell;
        public bool hasEnergyRoleViolation;
        public int powerRangeRadius;
        public List<ItemShapeCell> occupiedCells = new();
        public List<ItemShapeCell> powerRangeCells = new();
        public string powerConnectionState = string.Empty;
        public string energyStateReason = string.Empty;
        public string playerFeedbackChinese = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class FormationEnergyDiagnosticRow
    {
        public string code = string.Empty;
        public string severity = "info";
        public string itemId = string.Empty;
        public EnergyState energyState = EnergyState.None;
        public bool isPowered;
        public string detail = string.Empty;
        public string sourceDataPath = string.Empty;
        public bool isViolation;
        public bool devOnly = true;
        public bool isEnabled;
    }

    public static class FormationEnergyDiagnosticCodes
    {
        public const string ForbiddenProviderCandidate = "FORBIDDEN_PROVIDER_CANDIDATE";
        public const string TagProviderViolation = "TAG_PROVIDER_VIOLATION";
        public const string LegacyProviderViolation = "LEGACY_PROVIDER_VIOLATION";
        public const string EyeCellOccupied = "EYE_CELL_OCCUPIED";
        public const string IsPoweredDerived = "IS_POWERED_DERIVED";
        public const string EnergyStoneDisconnected = "ENERGY_STONE_DISCONNECTED";
        public const string OrdinaryManaSource = "ORDINARY_MANA_SOURCE";
    }

    public static class FormationEnergyContractResolver
    {
        public const string PackageName = FormationEnergyContractPreview.PackageName;
        public const string DefaultEnergyStoneItemId = "spirit_stone_basic";
        public const string SpiritStoneFamily = "spirit_stone";
        public const string SourceDataPath = "BuildSandboxLayoutSnapshot.formationEnergyContract";

        private static readonly string[] ForbiddenProviderTokens =
        {
            "energy_incense",
            "stone_core",
            "taomu",
            "peach_wood",
            "seal"
        };

        private static readonly string[] LegacyProviderTags =
        {
            "energy_preview",
            "energy_source",
            "ju_neng",
            "energy",
            "core_preview",
            "orange_core_preview"
        };

        public static FormationEnergyContractPreview Apply(BuildSandboxLayoutSnapshot snapshot)
        {
            BackpackLayoutConfig layoutConfig = BackpackLayoutConfig.CreateDefault();
            EyeConfig eyeConfig = EyeConfig.CreateDefault();
            EyeRuntimeData eyeRuntime = EyeRuntimeData.CreateDefault(eyeConfig, layoutConfig);
            FormationEnergyContractPreview preview = new()
            {
                layoutConfig = layoutConfig,
                eyeConfig = eyeConfig,
                eyeRuntimeData = eyeRuntime,
                battleLayoutSnapshot = new FormationEnergyBattleLayoutSnapshot
                {
                    backpackLayoutConfig = layoutConfig,
                    eyeConfig = eyeConfig,
                    eyeRuntimeData = eyeRuntime,
                    devOnly = true,
                    isEnabled = false
                }
            };

            List<BuildSandboxPlacedItemSnapshot> items = snapshot?.placedItems?
                .Where(item => item != null)
                .ToList() ?? new List<BuildSandboxPlacedItemSnapshot>();

            Dictionary<BuildSandboxPlacedItemSnapshot, bool> previousPowered = items.ToDictionary(
                item => item,
                item => item.isPowered);

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                BuildSandboxItemIdentityFamilyCatalog.ApplyTo(item);
                ResetEnergyFields(item);
                ApplyEyePulseState(item, eyeConfig);
            }

            List<BuildSandboxPlacedItemSnapshot> poweredStones = items
                .Where(item => item != null && item.isEnergyStoneSource && item.energyState == EnergyState.Powered)
                .OrderBy(item => MinDistanceToCell(item, eyeConfig.eyeCell))
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item == null || item.energyState == EnergyState.Suppressed || item.isEnergyStoneSource)
                {
                    continue;
                }

                BuildSandboxPlacedItemSnapshot provider = poweredStones
                    .Where(stone => IsInEnergyStoneSupplyRange(item, stone, EnergyStoneConfig.CreateDefault(stone.itemId)))
                    .OrderBy(stone => MinDistance(item, stone))
                    .ThenBy(stone => stone.itemId, StringComparer.Ordinal)
                    .FirstOrDefault();

                if (provider == null)
                {
                    continue;
                }

                ApplyState(
                    item,
                    EnergyState.Powered,
                    provider.itemId,
                    eyeConfig.eyeId,
                    provider.itemId,
                    "powered_by_energy_stone_range");
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                DeriveLegacyPowerFields(item);
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                AppendStaticDiagnostics(preview, item, previousPowered[item]);
            }

            eyeRuntime.connectedEnergySources = poweredStones
                .Select(item => item.itemId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            Dictionary<string, List<string>> suppliedItemsByStone = items
                .Where(item => item != null
                    && item.energyState == EnergyState.Powered
                    && !item.isEnergyStoneSource
                    && !string.IsNullOrWhiteSpace(item.formalEnergySourceItemId))
                .GroupBy(item => item.formalEnergySourceItemId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(item => item.itemId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(id => id, StringComparer.Ordinal)
                        .ToList(),
                    StringComparer.Ordinal);
            preview.battleLayoutSnapshot.energyStones = poweredStones
                .Select(item => BuildEnergyStoneRuntimeData(item, suppliedItemsByStone))
                .ToList();
            preview.battleLayoutSnapshot.placedItems = items;

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                preview.rows.Add(BuildRow(item));
            }

            return preview;
        }

        public static bool IsEnergyStoneItem(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return false;
            }

            string itemId = Normalize(item.itemId);
            string family = Normalize(item.itemFamily);
            string baseItemId = Normalize(item.baseItemId);
            return itemId.Contains("spirit_stone")
                || itemId.Contains("energy_stone")
                || family == SpiritStoneFamily
                || baseItemId == DefaultEnergyStoneItemId;
        }

        public static bool IsPowered(BuildSandboxPlacedItemSnapshot item)
        {
            return item != null && item.energyState == EnergyState.Powered;
        }

        public static bool WouldLegacyProvider(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return false;
            }

            string itemId = Normalize(item.itemId);
            if (itemId.Contains("spirit_stone")
                || itemId.Contains("energy_incense")
                || itemId.Contains("stone_core"))
            {
                return true;
            }

            return HasAnyLegacyProviderTag(item);
        }

        public static bool HasAnyLegacyProviderTag(BuildSandboxPlacedItemSnapshot item)
        {
            return (item?.tags ?? new List<string>()).Any(tag =>
                LegacyProviderTags.Any(providerTag =>
                    string.Equals(tag, providerTag, StringComparison.OrdinalIgnoreCase)));
        }

        public static List<ItemShapeCell> BuildCellsWithinRange(
            ItemShapeCell origin,
            int radius,
            int width,
            int height,
            bool includeOrigin)
        {
            List<ItemShapeCell> cells = new();
            for (int x = origin.x - radius; x <= origin.x + radius; x++)
            {
                for (int y = origin.y - radius; y <= origin.y + radius; y++)
                {
                    ItemShapeCell cell = new(x, y);
                    if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
                    {
                        continue;
                    }

                    if (!includeOrigin && cell.Equals(origin))
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

            return NormalizeCells(cells);
        }

        public static List<ItemShapeCell> NormalizeCells(IEnumerable<ItemShapeCell> cells)
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

        private static void ResetEnergyFields(BuildSandboxPlacedItemSnapshot item)
        {
            item.energyState = EnergyState.None;
            item.isPowered = false;
            item.energySourceId = string.Empty;
            item.connectedEyeId = string.Empty;
            item.formalEnergySourceItemId = string.Empty;
            item.energyStateReason = string.Empty;
            item.isInBasePulseRange = false;
            item.isEyeAdjacent = false;
            item.isEnergyStoneSource = false;
            item.hasEnergyRoleViolation = false;
            item.energyDiagnostics = new List<string>();
            item.touchesFormationCore = false;
            item.formationCoreId = string.Empty;
            item.powerRangeRadius = 0;
            item.powerRangeCells = new List<ItemShapeCell>();
            item.powerConnectionState = string.Empty;
        }

        private static void ApplyEyePulseState(BuildSandboxPlacedItemSnapshot item, EyeConfig eyeConfig)
        {
            bool touchesEye = OccupiesCell(item, eyeConfig.eyeCell);
            int eyeDistance = MinDistanceToCell(item, eyeConfig.eyeCell);
            bool inPulseRange = eyeDistance <= Math.Max(0, eyeConfig.basePulseRange);
            bool isEnergyStone = IsEnergyStoneItem(item);

            item.touchesFormationCore = touchesEye;
            item.formationCoreId = touchesEye ? eyeConfig.eyeId : string.Empty;
            item.isInBasePulseRange = inPulseRange;
            item.isEyeAdjacent = eyeDistance == 1;
            item.isEnergyStoneSource = isEnergyStone;

            if (touchesEye)
            {
                ApplyState(item, EnergyState.Suppressed, string.Empty, eyeConfig.eyeId, string.Empty, "eye_cell_occupied");
                return;
            }

            if (isEnergyStone)
            {
                if (item.isEyeAdjacent)
                {
                    ApplyState(item, EnergyState.Powered, item.itemId, eyeConfig.eyeId, item.itemId, "energy_stone_connected_to_eye");
                    EnergyStoneConfig stoneConfig = EnergyStoneConfig.CreateDefault(item.itemId);
                    item.powerRangeRadius = stoneConfig.supplyRange;
                    item.powerRangeCells = BuildPowerRangeCells(item, stoneConfig.supplyRange);
                    return;
                }

                ApplyState(
                    item,
                    inPulseRange ? EnergyState.WeakPulse : EnergyState.None,
                    inPulseRange ? eyeConfig.eyeId : string.Empty,
                    inPulseRange ? eyeConfig.eyeId : string.Empty,
                    string.Empty,
                    "energy_stone_disconnected");
                return;
            }

            if (inPulseRange)
            {
                ApplyState(item, EnergyState.WeakPulse, eyeConfig.eyeId, eyeConfig.eyeId, string.Empty, "eye_weak_pulse_only");
            }
        }

        private static void ApplyState(
            BuildSandboxPlacedItemSnapshot item,
            EnergyState state,
            string energySourceId,
            string connectedEyeId,
            string formalEnergySourceItemId,
            string reason)
        {
            item.energyState = state;
            item.energySourceId = energySourceId ?? string.Empty;
            item.connectedEyeId = connectedEyeId ?? string.Empty;
            item.formalEnergySourceItemId = formalEnergySourceItemId ?? string.Empty;
            item.energyStateReason = reason ?? string.Empty;
            item.powerConnectionState = reason ?? string.Empty;
        }

        private static void DeriveLegacyPowerFields(BuildSandboxPlacedItemSnapshot item)
        {
            item.isPowered = item.energyState == EnergyState.Powered;
            if (!item.isPowered && !string.IsNullOrWhiteSpace(item.formalEnergySourceItemId))
            {
                item.formalEnergySourceItemId = string.Empty;
            }
        }

        private static void AppendStaticDiagnostics(
            FormationEnergyContractPreview preview,
            BuildSandboxPlacedItemSnapshot item,
            bool previousPowered)
        {
            if (item == null)
            {
                return;
            }

            if (item.touchesFormationCore)
            {
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.EyeCellOccupied,
                    "violation",
                    "The fixed eye cell is occupied by an item; the new contract treats the eye as layout state, not an item slot.",
                    isViolation: true);
            }

            if (!IsEnergyStoneItem(item) && IsForbiddenProviderCandidate(item))
            {
                item.hasEnergyRoleViolation = true;
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.ForbiddenProviderCandidate,
                    "violation",
                    "This item family may receive energy but must not be recognized as a formal Powered source.",
                    isViolation: true);
            }

            if (!IsEnergyStoneItem(item) && HasAnyLegacyProviderTag(item))
            {
                item.hasEnergyRoleViolation = true;
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.TagProviderViolation,
                    "violation",
                    "Provider-like tags are diagnostic only; tags must not promote an ordinary item into a formal source.",
                    isViolation: true);
            }

            if (!IsEnergyStoneItem(item) && WouldLegacyProvider(item))
            {
                item.hasEnergyRoleViolation = true;
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.LegacyProviderViolation,
                    "violation",
                    "FormationCoreAndPowerRange01 would classify this item as a provider; the new contract overrides it.",
                    isViolation: true);
            }

            BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item.itemStat, item.itemId);
            if (!IsEnergyStoneItem(item) && stat.manaGainPerTick > 0)
            {
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.OrdinaryManaSource,
                    "warning",
                    "manaGainPerTick exists on a non-stone item; runtime mana gain must not treat it as a formal energy source.",
                    isViolation: false);
            }

            if (previousPowered && item.energyState != EnergyState.Powered)
            {
                AddDiagnostic(
                    preview,
                    item,
                    FormationEnergyDiagnosticCodes.IsPoweredDerived,
                    "info",
                    "Previous isPowered was replaced by derived energyState == Powered.",
                    isViolation: false);
            }
        }

        private static void AddDiagnostic(
            FormationEnergyContractPreview preview,
            BuildSandboxPlacedItemSnapshot item,
            string code,
            string severity,
            string detail,
            bool isViolation)
        {
            item.energyDiagnostics.Add(code);
            preview.diagnostics.Add(new FormationEnergyDiagnosticRow
            {
                code = code,
                severity = severity,
                itemId = item.itemId ?? string.Empty,
                energyState = item.energyState,
                isPowered = item.energyState == EnergyState.Powered,
                detail = detail ?? string.Empty,
                sourceDataPath = SourceDataPath + "." + code,
                isViolation = isViolation,
                devOnly = true,
                isEnabled = false
            });
        }

        private static bool IsForbiddenProviderCandidate(BuildSandboxPlacedItemSnapshot item)
        {
            string itemId = Normalize(item.itemId);
            string family = Normalize(item.itemFamily);
            string baseItemId = Normalize(item.baseItemId);
            return ForbiddenProviderTokens.Any(token =>
                itemId.Contains(token) || family.Contains(token) || baseItemId.Contains(token));
        }

        private static bool IsInEnergyStoneSupplyRange(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxPlacedItemSnapshot stone,
            EnergyStoneConfig stoneConfig)
        {
            HashSet<string> range = new(
                BuildPowerRangeCells(stone, Math.Max(0, stoneConfig?.supplyRange ?? 0))
                    .Select(CellKey),
                StringComparer.Ordinal);
            return (item?.occupiedCells ?? new List<ItemShapeCell>()).Any(cell => range.Contains(CellKey(cell)));
        }

        private static List<ItemShapeCell> BuildPowerRangeCells(BuildSandboxPlacedItemSnapshot provider, int radius)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            List<ItemShapeCell> cells = new();
            foreach (ItemShapeCell origin in provider?.occupiedCells ?? new List<ItemShapeCell>())
            {
                foreach (ItemShapeCell cell in BuildCellsWithinRange(origin, radius, 5, 5, includeOrigin: true))
                {
                    if (seen.Add(CellKey(cell)))
                    {
                        cells.Add(cell);
                    }
                }
            }

            return NormalizeCells(cells);
        }

        private static EnergyStoneRuntimeData BuildEnergyStoneRuntimeData(
            BuildSandboxPlacedItemSnapshot item,
            IReadOnlyDictionary<string, List<string>> suppliedItemsByStone)
        {
            return new EnergyStoneRuntimeData
            {
                itemId = item.itemId ?? string.Empty,
                placedCell = (item.occupiedCells ?? new List<ItemShapeCell>()).FirstOrDefault(),
                connectionState = item.powerConnectionState ?? string.Empty,
                connectedEyeId = item.connectedEyeId ?? string.Empty,
                currentEfficiency = 1f,
                suppliedItems = suppliedItemsByStone != null
                    && suppliedItemsByStone.TryGetValue(item.itemId ?? string.Empty, out List<string> supplied)
                        ? new List<string>(supplied)
                        : new List<string>(),
                isStolen = false,
                isSuppressed = item.energyState == EnergyState.Suppressed,
                overloadValue = 0,
                devOnly = true,
                isEnabled = false
            };
        }

        private static FormationEnergyContractRow BuildRow(BuildSandboxPlacedItemSnapshot item)
        {
            return new FormationEnergyContractRow
            {
                itemId = item?.itemId ?? string.Empty,
                shapeId = item?.shapeId ?? string.Empty,
                energyState = item?.energyState ?? EnergyState.None,
                isPowered = item?.isPowered ?? false,
                energySourceId = item?.energySourceId ?? string.Empty,
                connectedEyeId = item?.connectedEyeId ?? string.Empty,
                formalEnergySourceItemId = item?.formalEnergySourceItemId ?? string.Empty,
                isEnergyStoneSource = item?.isEnergyStoneSource ?? false,
                isInBasePulseRange = item?.isInBasePulseRange ?? false,
                isEyeAdjacent = item?.isEyeAdjacent ?? false,
                touchesEyeCell = item?.touchesFormationCore ?? false,
                hasEnergyRoleViolation = item?.hasEnergyRoleViolation ?? false,
                powerRangeRadius = item?.powerRangeRadius ?? 0,
                occupiedCells = NormalizeCells(item?.occupiedCells),
                powerRangeCells = NormalizeCells(item?.powerRangeCells),
                powerConnectionState = item?.powerConnectionState ?? string.Empty,
                energyStateReason = item?.energyStateReason ?? string.Empty,
                playerFeedbackChinese = ResolvePlayerFeedback(item),
                devOnly = true,
                isEnabled = false
            };
        }

        private static string ResolvePlayerFeedback(BuildSandboxPlacedItemSnapshot item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            switch (item.energyState)
            {
                case EnergyState.Powered:
                    return item.isEnergyStoneSource
                        ? "\u805a\u80fd\u77f3\u8fde\u901a\uff0c\u7b26\u9635\u4f9b\u80fd\u7a33\u5b9a"
                        : "\u7075\u529b\u63a5\u901a\uff0c\u9053\u5177\u56de\u5e94\u7a33\u5b9a";
                case EnergyState.WeakPulse:
                    return "\u7b26\u9635\u5fae\u4eae\uff0c\u6548\u679c\u8f83\u5f31";
                case EnergyState.Suppressed:
                    return "\u7075\u529b\u88ab\u538b\u4f4f\uff0c\u9053\u5177\u6682\u4e0d\u56de\u5e94";
                default:
                    return "\u4f9b\u80fd\u65ad\u5f00\uff0c\u9053\u5177\u6c89\u5bc2";
            }
        }

        private static bool OccupiesCell(BuildSandboxPlacedItemSnapshot item, ItemShapeCell target)
        {
            return (item?.occupiedCells ?? new List<ItemShapeCell>()).Any(cell => cell.Equals(target));
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

        private static int MinDistanceToCell(BuildSandboxPlacedItemSnapshot item, ItemShapeCell target)
        {
            int best = int.MaxValue;
            foreach (ItemShapeCell cell in item?.occupiedCells ?? new List<ItemShapeCell>())
            {
                int distance = Math.Abs(cell.x - target.x) + Math.Abs(cell.y - target.y);
                if (distance < best)
                {
                    best = distance;
                }
            }

            return best == int.MaxValue ? 999 : best;
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string CellKey(ItemShapeCell cell)
        {
            return cell.x + ":" + cell.y;
        }
    }
}
