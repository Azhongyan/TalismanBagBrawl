using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSandboxPlacedItemSnapshot
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public ItemShapeCell anchorCell;
        public List<ItemShapeCell> occupiedCells = new();
        public ItemShapeRotation rotation = ItemShapeRotation.Rotation0;
        public List<string> tags = new();
        public EnergyState energyState = EnergyState.None;
        public bool isPowered;
        public string energySourceId = string.Empty;
        public string connectedEyeId = string.Empty;
        public string formalEnergySourceItemId = string.Empty;
        public string energyStateReason = string.Empty;
        public bool isInBasePulseRange;
        public bool isEyeAdjacent;
        public bool isEnergyStoneSource;
        public bool hasEnergyRoleViolation;
        public List<string> energyDiagnostics = new();
        public List<string> affixList = new();
        public string rarity = "sandbox";
        public string itemFamily = string.Empty;
        public string baseItemId = string.Empty;
        public string tier = BuildSandboxItemIdentityFamilyCatalog.TierTestOnly;
        public string relationshipToBase = BuildSandboxItemIdentityFamilyCatalog.RelationshipTestOnly;
        public bool touchesFormationCore;
        public string formationCoreId = string.Empty;
        public int powerRangeRadius;
        public List<ItemShapeCell> powerRangeCells = new();
        public string powerConnectionState = string.Empty;
        public BuildSandboxItemStat itemStat = new();

        public static BuildSandboxPlacedItemSnapshot FromPlacementResult(ShapePlacementResult result)
        {
            BuildSandboxPlacedItemSnapshot snapshot = new();
            if (result == null)
            {
                return snapshot;
            }

            snapshot.itemId = result.ItemId;
            snapshot.shapeId = result.ShapeId;
            snapshot.anchorCell = result.AnchorCell;
            snapshot.occupiedCells = new List<ItemShapeCell>(result.OccupiedCells);
            snapshot.energyState = result.EnergyConnected ? EnergyState.Powered : EnergyState.None;
            snapshot.isPowered = snapshot.energyState == EnergyState.Powered;
            snapshot.energySourceId = result.EnergyConnectionNote;
            snapshot.energyStateReason = result.EnergyConnected
                ? "placement_result_energy_connected"
                : "placement_result_energy_none";
            snapshot.itemStat = BuildSandboxItemStatCatalog.Resolve(result.ItemId);
            BuildSandboxItemIdentityFamilyCatalog.ApplyTo(snapshot);
            return snapshot;
        }
    }

    [Serializable]
    public sealed class BuildSandboxItemIdentityFamilyRecord
    {
        public BuildSandboxItemIdentityFamilyRecord(
            string classification,
            string itemId,
            string displayName,
            string itemFamily,
            string baseItemId,
            string tier,
            string relationshipToBase,
            bool devOnly,
            bool isEnabled,
            string sourceScope,
            string notes)
        {
            Classification = classification ?? string.Empty;
            ItemId = itemId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            ItemFamily = itemFamily ?? string.Empty;
            BaseItemId = baseItemId ?? string.Empty;
            Tier = tier ?? string.Empty;
            RelationshipToBase = relationshipToBase ?? string.Empty;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            SourceScope = sourceScope ?? string.Empty;
            Notes = notes ?? string.Empty;
        }

        public string Classification { get; }
        public string ItemId { get; }
        public string DisplayName { get; }
        public string ItemFamily { get; }
        public string BaseItemId { get; }
        public string Tier { get; }
        public string RelationshipToBase { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public string SourceScope { get; }
        public string Notes { get; }
    }

    public static class BuildSandboxItemIdentityFamilyCatalog
    {
        public const string TierBasic = "basic";
        public const string TierAdvanced = "advanced";
        public const string TierCore = "core";
        public const string TierSupport = "support";
        public const string TierTestOnly = "test_only";

        public const string RelationshipBase = "base";
        public const string RelationshipAdvancedVariant = "advanced_variant";
        public const string RelationshipNewMechanic = "new_mechanic";
        public const string RelationshipSupportVariant = "support_variant";
        public const string RelationshipTestOnly = "test_only";

        public const string BasicClassification = "v02_v03_basic_item";
        public const string V04Classification = "v04_advanced_or_new_mechanic_item";
        public const string TestOnlyClassification = "buildsandbox_test_only";

        private const string FormalReferenceScope = "v02_v03_formal_reference_read_only";
        private const string BuildSandboxScope = "buildsandbox_devonly_preview";

        private static readonly BuildSandboxItemIdentityFamilyRecord[] BasicRows =
        {
            Basic("fire_talisman_basic", "火符", "fire_talisman"),
            Basic("thunder_talisman_basic", "雷符", "thunder_talisman"),
            Basic("shield_talisman_basic", "护身符", "guardian_ward"),
            Basic("qi_pill_basic", "丹药", "pill"),
            Basic("spirit_stone_basic", "聚灵石", "spirit_stone"),
            Basic("sword_pill_basic", "剑丸", "sword_pill"),
            Basic("chain_thunder_talisman_basic", "连锁雷符", "chain_thunder_talisman"),
            Basic("purify_talisman_basic", "净化符", "purify_talisman"),
            Basic("soul_suppress_talisman_basic", "镇魂符", "soul_suppress_talisman"),
            Basic("seal_basic", "法印", "seal"),
            Basic("water_talisman_basic", "水符", "water_talisman"),
            Basic("exorcism_bell_basic", "驱邪铃", "exorcism_bell"),
            Basic("peach_wood_basic", "桃木牌", "peach_wood")
        };

        private static readonly BuildSandboxItemIdentityFamilyRecord[] V04Rows =
        {
            V04(
                "preview_fire_talisman",
                "炽火符",
                "fire_talisman",
                "fire_talisman_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 fire escalation; keeps fire_talisman_basic as the formal base item."),
            V04(
                "preview_thunder_sword",
                "雷引剑符",
                "thunder_talisman",
                "thunder_talisman_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 thunder/sword build piece; does not replace thunder_talisman_basic or chain_thunder_talisman_basic."),
            V04(
                "preview_x2_wood_talisman",
                "护阵木牌",
                "guardian_ward",
                "shield_talisman_basic",
                TierSupport,
                RelationshipSupportVariant,
                "V0.4 support ward plate; shield_talisman_basic remains the basic defense item."),
            V04(
                "preview_guard_wood",
                "守护木牌",
                "guardian_ward",
                "shield_talisman_basic",
                TierSupport,
                RelationshipSupportVariant,
                "V0.4 support guard piece; not a shield_talisman_basic replacement."),
            V04(
                "preview_cleanse_corner",
                "净化折符",
                "purify_talisman",
                "purify_talisman_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 folded cleanse shape; purify_talisman_basic remains the basic cleanse item."),
            V04(
                "preview_stone_core",
                "炉芯石",
                "furnace_core",
                string.Empty,
                TierCore,
                RelationshipNewMechanic,
                "V0.4 core mechanism seed; no V0.2/V0.3 base item is replaced."),
            V04(
                "preview_energy_incense",
                "醒符香",
                "awakening_incense",
                string.Empty,
                TierSupport,
                RelationshipNewMechanic,
                "V0.4 rhythm/awakening support seed; no formal consumable flow is enabled."),
            V04(
                "preview_old_bell",
                "镇邪铃",
                "exorcism_bell",
                "exorcism_bell_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 bell variant; exorcism_bell_basic remains the basic bell item."),
            V04(
                "preview_soul_seal",
                "镇魂法印",
                "soul_suppress_talisman",
                "soul_suppress_talisman_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 soul-control seal variant; soul_suppress_talisman_basic remains the basic control item."),
            V04(
                "preview_taomu_sword",
                "桃木剑",
                "peach_wood",
                "peach_wood_basic",
                TierAdvanced,
                RelationshipAdvancedVariant,
                "V0.4 peach-wood weapon variant; peach_wood_basic remains the basic plate item.")
        };

        private static readonly BuildSandboxItemIdentityFamilyRecord[] AllRows =
            BasicRows.Concat(V04Rows).ToArray();

        private static readonly Dictionary<string, BuildSandboxItemIdentityFamilyRecord> RowsByItemId =
            AllRows.ToDictionary(row => row.ItemId, StringComparer.Ordinal);

        public static IReadOnlyList<string> AllowedTierValues { get; } =
            new[] { TierBasic, TierAdvanced, TierCore, TierSupport, TierTestOnly };

        public static IReadOnlyList<string> AllowedRelationshipValues { get; } =
            new[]
            {
                RelationshipBase,
                RelationshipAdvancedVariant,
                RelationshipNewMechanic,
                RelationshipSupportVariant,
                RelationshipTestOnly
            };

        public static IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> BasicItems => BasicRows;
        public static IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> V04PreviewItems => V04Rows;
        public static IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> AllItems => AllRows;

        public static BuildSandboxItemIdentityFamilyRecord Resolve(string itemId)
        {
            string safeItemId = itemId ?? string.Empty;
            return RowsByItemId.TryGetValue(safeItemId, out BuildSandboxItemIdentityFamilyRecord record)
                ? record
                : new BuildSandboxItemIdentityFamilyRecord(
                    TestOnlyClassification,
                    safeItemId,
                    string.Empty,
                    "buildsandbox_unknown",
                    string.Empty,
                    TierTestOnly,
                    RelationshipTestOnly,
                    true,
                    false,
                    BuildSandboxScope,
                    "Fallback identity for ad-hoc BuildSandbox test-only snapshots.");
        }

        public static void ApplyTo(BuildSandboxPlacedItemSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            BuildSandboxItemIdentityFamilyRecord record = Resolve(snapshot.itemId);
            snapshot.itemFamily = record.ItemFamily;
            snapshot.baseItemId = record.BaseItemId;
            snapshot.tier = record.Tier;
            snapshot.relationshipToBase = record.RelationshipToBase;
        }

        public static bool IsAllowedTier(string tier)
        {
            return AllowedTierValues.Contains(tier ?? string.Empty);
        }

        public static bool IsAllowedRelationship(string relationshipToBase)
        {
            return AllowedRelationshipValues.Contains(relationshipToBase ?? string.Empty);
        }

        private static BuildSandboxItemIdentityFamilyRecord Basic(
            string itemId,
            string displayName,
            string itemFamily)
        {
            return new BuildSandboxItemIdentityFamilyRecord(
                BasicClassification,
                itemId,
                displayName,
                itemFamily,
                itemId,
                TierBasic,
                RelationshipBase,
                false,
                true,
                FormalReferenceScope,
                "Read-only V0.2/V0.3 base item identity; not modified by BuildSandbox.");
        }

        private static BuildSandboxItemIdentityFamilyRecord V04(
            string itemId,
            string displayName,
            string itemFamily,
            string baseItemId,
            string tier,
            string relationshipToBase,
            string notes)
        {
            return new BuildSandboxItemIdentityFamilyRecord(
                V04Classification,
                itemId,
                displayName,
                itemFamily,
                baseItemId,
                tier,
                relationshipToBase,
                true,
                false,
                BuildSandboxScope,
                notes);
        }
    }

    [Serializable]
    public sealed class BuildSandboxLegacyAndAdvancedItemRosterRow
    {
        public BuildSandboxLegacyAndAdvancedItemRosterRow(
            BuildSandboxItemIdentityFamilyRecord identity,
            string shapeId,
            string shapeDisplayName,
            params string[] categoryIds)
        {
            Identity = identity ?? BuildSandboxItemIdentityFamilyCatalog.Resolve(string.Empty);
            ShapeId = string.IsNullOrWhiteSpace(shapeId) ? "Single1" : shapeId;
            ShapeDisplayName = string.IsNullOrWhiteSpace(shapeDisplayName) ? "单格" : shapeDisplayName;
            CategoryIds = (categoryIds ?? Array.Empty<string>())
                .Select(BuildSandboxLegacyAndAdvancedItemRosterCatalog.NormalizeCategoryId)
                .Where(categoryId => !string.IsNullOrWhiteSpace(categoryId)
                    && !string.Equals(categoryId, BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryAll, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (CategoryIds.Count == 0)
            {
                CategoryIds = new[] { BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryTestDevOnly };
            }
        }

        public BuildSandboxItemIdentityFamilyRecord Identity { get; }
        public string ItemId => Identity.ItemId;
        public string DisplayName => Identity.DisplayName;
        public string ShapeId { get; }
        public string ShapeDisplayName { get; }
        public IReadOnlyList<string> CategoryIds { get; }
        public string PrimaryCategoryId => CategoryIds.Count == 0
            ? BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryTestDevOnly
            : CategoryIds[0];
        public string CategoryDisplayName =>
            BuildSandboxLegacyAndAdvancedItemRosterCatalog.ToCategoryDisplayName(PrimaryCategoryId);
        public bool SandboxRosterDevOnly => true;
        public bool SandboxRosterEnabled => false;

        public bool HasCategory(string categoryId)
        {
            string normalized = BuildSandboxLegacyAndAdvancedItemRosterCatalog.NormalizeCategoryId(categoryId);
            return CategoryIds.Any(value => string.Equals(value, normalized, StringComparison.Ordinal));
        }
    }

    public static class BuildSandboxLegacyAndAdvancedItemRosterCatalog
    {
        public const string PackageName = "V0.4-LegacyAndAdvancedItemRoster01";
        public const string CategoryAll = "all";
        public const string CategoryBasic = "basic";
        public const string CategoryAdvanced = "advanced";
        public const string CategoryCore = "core";
        public const string CategorySupport = "support";
        public const string CategoryArtifact = "artifact";
        public const string CategoryTestDevOnly = "test/devOnly";

        private const string SingleShapeId = "Single1";
        private const string SingleShapeDisplayName = "单格";

        private static readonly string[] CategoryDisplayRows =
        {
            "\u5168\u90e8",
            "\u8fdb\u9636",
            "\u6838\u5fc3",
            "\u8f85\u52a9",
            "\u6cd5\u5668"
        };

        private static readonly BuildSandboxLegacyAndAdvancedItemRosterRow[] RosterRows =
        {
            Row("preview_stone_core", "Square4", "方四格", CategoryCore, CategoryTestDevOnly),
            Row("preview_soul_seal", "Square4", "方四格", CategoryCore, CategoryArtifact, CategoryTestDevOnly),
            Row("preview_taomu_sword", "vertical_3", "竖三格", CategoryAdvanced, CategoryArtifact, CategoryTestDevOnly),
            Row("preview_cleanse_corner", "Corner3", "拐角三格", CategoryAdvanced, CategorySupport, CategoryTestDevOnly),
            Row("preview_old_bell", "Corner3", "拐角三格", CategoryAdvanced, CategoryArtifact, CategoryTestDevOnly),
            Row("preview_x2_wood_talisman", "Vertical2", "竖二格", CategorySupport, CategoryTestDevOnly),
            Row("preview_guard_wood", "Vertical2", "竖二格", CategorySupport, CategoryArtifact, CategoryTestDevOnly),
            Row("preview_energy_incense", "Vertical2", "竖二格", CategorySupport, CategoryTestDevOnly),
            Row("preview_fire_talisman", SingleShapeId, SingleShapeDisplayName, CategoryAdvanced, CategoryTestDevOnly),
            Row("preview_thunder_sword", SingleShapeId, SingleShapeDisplayName, CategoryAdvanced, CategoryArtifact, CategoryTestDevOnly),

            Basic("fire_talisman_basic"),
            Basic("thunder_talisman_basic"),
            Basic("shield_talisman_basic", CategorySupport),
            Basic("qi_pill_basic", CategorySupport),
            Basic("spirit_stone_basic", CategorySupport),
            Basic("sword_pill_basic", CategoryArtifact),
            Basic("chain_thunder_talisman_basic"),
            Basic("purify_talisman_basic", CategorySupport),
            Basic("soul_suppress_talisman_basic", CategorySupport),
            Basic("seal_basic", CategoryArtifact),
            Basic("water_talisman_basic", CategorySupport),
            Basic("exorcism_bell_basic", CategoryArtifact),
            Basic("peach_wood_basic", CategoryArtifact)
        };

        private static readonly Dictionary<string, BuildSandboxLegacyAndAdvancedItemRosterRow> RowsByItemId =
            RosterRows.ToDictionary(row => row.ItemId, StringComparer.Ordinal);

        public static IReadOnlyList<string> CategoryDisplayLabels => CategoryDisplayRows;
        public static IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> AllItems => RosterRows;

        public static BuildSandboxLegacyAndAdvancedItemRosterRow Resolve(string itemId)
        {
            string safeItemId = itemId ?? string.Empty;
            return RowsByItemId.TryGetValue(safeItemId, out BuildSandboxLegacyAndAdvancedItemRosterRow row)
                ? row
                : new BuildSandboxLegacyAndAdvancedItemRosterRow(
                    BuildSandboxItemIdentityFamilyCatalog.Resolve(safeItemId),
                    SingleShapeId,
                    SingleShapeDisplayName,
                    CategoryTestDevOnly);
        }

        public static string NormalizeCategoryId(string categoryOrDisplay)
        {
            if (string.IsNullOrWhiteSpace(categoryOrDisplay))
            {
                return CategoryAll;
            }

            string value = categoryOrDisplay.Trim();
            switch (value)
            {
                case CategoryAll:
                case "全部":
                case "鍏ㄩ儴":
                    return CategoryAll;
                case CategoryBasic:
                case "基础":
                case "基礎":
                    return CategoryBasic;
                case CategoryAdvanced:
                case "进阶":
                case "進階":
                case "符箓":
                case "符籙":
                case "绗︾畵":
                    return CategoryAdvanced;
                case CategoryCore:
                case "核心":
                case "材料":
                case "鏉愭枡":
                    return CategoryCore;
                case CategorySupport:
                case "辅助":
                case "輔助":
                case "消耗":
                    return CategorySupport;
                case CategoryArtifact:
                case "法器":
                case "娉曞櫒":
                    return CategoryArtifact;
                case CategoryTestDevOnly:
                case "test/devonly":
                case "test":
                case "devOnly":
                case "devonly":
                case "测试":
                case "測試":
                case "特殊":
                case "鐗规畩":
                    return CategoryTestDevOnly;
                default:
                    return value.StartsWith("娑堣", StringComparison.Ordinal)
                        ? CategorySupport
                        : value;
            }
        }

        public static string ToCategoryDisplayName(string categoryId)
        {
            switch (NormalizeCategoryId(categoryId))
            {
                case CategoryAll:
                    return "\u5168\u90e8";
                case CategoryBasic:
                    return "基础";
                case CategoryAdvanced:
                    return "进阶";
                case CategoryCore:
                    return "核心";
                case CategorySupport:
                    return "辅助";
                case CategoryArtifact:
                    return "法器";
                case CategoryTestDevOnly:
                    return "测试";
                default:
                    return "测试";
            }
        }

        private static BuildSandboxLegacyAndAdvancedItemRosterRow Basic(
            string itemId,
            params string[] secondaryCategoryIds)
        {
            string[] categories = new[] { CategoryBasic }
                .Concat(secondaryCategoryIds ?? Array.Empty<string>())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return Row(itemId, SingleShapeId, SingleShapeDisplayName, categories);
        }

        private static BuildSandboxLegacyAndAdvancedItemRosterRow Row(
            string itemId,
            string shapeId,
            string shapeDisplayName,
            params string[] categoryIds)
        {
            return new BuildSandboxLegacyAndAdvancedItemRosterRow(
                BuildSandboxItemIdentityFamilyCatalog.Resolve(itemId),
                shapeId,
                shapeDisplayName,
                categoryIds);
        }
    }

    [Serializable]
    public sealed class BuildSandboxItemStat
    {
        public string statProfileId = string.Empty;
        public int attack;
        public int guard;
        public int spirit;
        public int control;
        public int shieldBreak;
        public int cleanse;
        public int manaGainPerTick;
        public int manaCostPerCast;
        public float castIntervalSeconds = 2f;
        public string statTag = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;

        public BuildSandboxItemStat Clone()
        {
            return new BuildSandboxItemStat
            {
                statProfileId = statProfileId ?? string.Empty,
                attack = attack,
                guard = guard,
                spirit = spirit,
                control = control,
                shieldBreak = shieldBreak,
                cleanse = cleanse,
                manaGainPerTick = manaGainPerTick,
                manaCostPerCast = manaCostPerCast,
                castIntervalSeconds = castIntervalSeconds,
                statTag = statTag ?? string.Empty,
                devOnly = devOnly,
                isEnabled = isEnabled
            };
        }
    }

    public static class BuildSandboxItemStatCatalog
    {
        public static BuildSandboxItemStat Resolve(string itemId)
        {
            string id = (itemId ?? string.Empty).Trim().ToLowerInvariant();
            switch (id)
            {
                case "fire_talisman_basic":
                    return Create("stat_v02_fire_talisman_single_1", 12, 0, 0, 0, 0, 0, 0, 10, 2.5f, "v02_fire_damage_single");
                case "thunder_talisman_basic":
                    return Create("stat_v02_thunder_talisman_single_1", 18, 0, 0, 0, 8, 0, 0, 16, 3f, "v02_thunder_shieldbreak_single");
                case "shield_talisman_basic":
                    return Create("stat_v02_shield_talisman_single_1", 0, 18, 0, 0, 0, 0, 0, 8, 5f, "v02_guard_single");
                case "qi_pill_basic":
                    return Create("stat_v02_qi_pill_single_1", 0, 0, 20, 0, 0, 3, 0, 12, 8f, "v02_heal_single");
                case "spirit_stone_basic":
                    return Create("stat_v02_spirit_stone_single_1", 0, 0, 12, 0, 0, 0, 12, 0, 2f, "v02_energy_source_single");
                case "sword_pill_basic":
                    return Create("stat_v02_sword_pill_single_1", 8, 0, 0, 0, 2, 0, 0, 8, 2.2f, "v02_sword_burst_single");
                case "chain_thunder_talisman_basic":
                    return Create("stat_v02_chain_thunder_single_1", 14, 0, 0, 0, 4, 0, 0, 18, 3.8f, "v02_chain_thunder_single");
                case "purify_talisman_basic":
                    return Create("stat_v02_purify_talisman_single_1", 0, 2, 0, 0, 0, 8, 0, 8, 6f, "v02_cleanse_single");
                case "soul_suppress_talisman_basic":
                    return Create("stat_v02_soul_suppress_single_1", 0, 4, 0, 10, 0, 2, 0, 10, 4.5f, "v02_anti_ghost_single");
                case "seal_basic":
                    return Create("stat_v02_seal_single_1", 0, 1, 1, 0, 0, 0, 0, 0, 2f, "v02_enhance_single");
                case "water_talisman_basic":
                    return Create("stat_v02_water_talisman_single_1", 0, 0, 10, 0, 0, 4, 0, 12, 4f, "v02_water_heal_single");
                case "exorcism_bell_basic":
                    return Create("stat_v02_exorcism_bell_single_1", 12, 0, 0, 4, 0, 2, 0, 10, 4f, "v02_exorcism_bell_single");
                case "peach_wood_basic":
                    return Create("stat_v02_peach_wood_single_1", 0, 2, 0, 4, 0, 1, 0, 0, 2f, "v02_peach_wood_single");
                case "preview_old_bell":
                    return Create("stat_old_bell_corner_3", 3, 4, 2, 6, 1, 3, 2, 6, 2.6f, "old_bell_control_corner");
                case "preview_soul_seal":
                    return Create("stat_soul_seal_square_4", 3, 6, 4, 7, 1, 3, 2, 7, 2.8f, "soul_seal_core_square");
            }

            if (id.Contains("taomu_sword"))
            {
                return Create("stat_taomu_sword_vertical_3", 8, 5, 2, 1, 4, 2, 1, 9, 2.6f, "taomu_sword_vertical_3");
            }

            if (id.Contains("thunder_sword"))
            {
                return Create("stat_thunder_sword_single_1", 7, 1, 3, 1, 5, 0, 1, 8, 2.1f, "thunder_break_single");
            }

            if (id.Contains("fire"))
            {
                return Create("stat_fire_talisman_single_1", 6, 0, 3, 0, 1, 0, 1, 6, 1.8f, "fire_damage_single");
            }

            if (id.Contains("guard_wood") || id.Contains("wood_talisman"))
            {
                return Create("stat_guard_wood_vertical_2", 2, 6, 3, 1, 1, 0, 2, 4, 2.4f, "guard_ward_vertical");
            }

            if (id.Contains("cleanse"))
            {
                return Create("stat_cleanse_corner_3", 2, 3, 4, 4, 0, 6, 2, 5, 2.8f, "cleanse_corner");
            }

            if (id.Contains("stone_core"))
            {
                return Create("stat_stone_core_square_4", 5, 6, 6, 1, 2, 0, 4, 7, 3f, "core_square");
            }

            if (id.Contains("energy"))
            {
                return Create("stat_energy_incense_vertical_2", 1, 1, 7, 0, 0, 0, 8, 0, 1f, "energy_vertical");
            }

            if (id.Contains("old_bell") || id.Contains("soul_seal"))
            {
                return Create("stat_control_ward_preview", 2, 5, 3, 6, 1, 2, 2, 6, 2.6f, "control_ward");
            }

            return Create("stat_sandbox_default", 1, 1, 1, 1, 0, 0, 1, 2, 2.4f, "sandbox_default");
        }

        public static BuildSandboxItemStat ResolveFrom(BuildSandboxItemStat itemStat, string itemId)
        {
            if (itemStat == null || string.IsNullOrWhiteSpace(itemStat.statProfileId))
            {
                return Resolve(itemId);
            }

            BuildSandboxItemStat clone = itemStat.Clone();
            clone.devOnly = true;
            clone.isEnabled = false;
            if (float.IsNaN(clone.castIntervalSeconds)
                || float.IsInfinity(clone.castIntervalSeconds)
                || clone.castIntervalSeconds <= 0f)
            {
                clone.castIntervalSeconds = 2f;
            }

            return clone;
        }

        public static string FormatPlayerFacing(BuildSandboxItemStat itemStat)
        {
            BuildSandboxItemStat stat = ResolveFrom(itemStat, string.Empty);
            return $"攻{stat.attack} / 守{stat.guard} / 灵{stat.spirit} / 控{stat.control} / 破{stat.shieldBreak} / 净{stat.cleanse} / 产灵{stat.manaGainPerTick} / 耗灵{stat.manaCostPerCast}";
        }

        private static BuildSandboxItemStat Create(
            string statProfileId,
            int attack,
            int guard,
            int spirit,
            int control,
            int shieldBreak,
            int cleanse,
            int manaGainPerTick,
            int manaCostPerCast,
            float castIntervalSeconds,
            string statTag)
        {
            return new BuildSandboxItemStat
            {
                statProfileId = statProfileId,
                attack = attack,
                guard = guard,
                spirit = spirit,
                control = control,
                shieldBreak = shieldBreak,
                cleanse = cleanse,
                manaGainPerTick = manaGainPerTick,
                manaCostPerCast = manaCostPerCast,
                castIntervalSeconds = castIntervalSeconds,
                statTag = statTag,
                devOnly = true,
                isEnabled = false
            };
        }
    }
}
