#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxItemStatFoundationValidator
    {
        public const string PackageName = "V0.4-BuildSandboxItemStatFoundation01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BuildSandboxItemStatFoundation01/[QA Only] Run ItemStat Foundation";

        private const string TaomuSwordItemId = "preview_taomu_sword";
        private const string Vertical3ShapeId = "vertical_3";
        private const string TaomuStatProfileId = "stat_taomu_sword_vertical_3";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                Validate()
            };
        }

        public static BattleSandboxBuildCombatPreview BuildDefaultPreview()
        {
            return BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BuildSandbox ItemStat Foundation 01");
            BattleSandboxBuildCombatPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateItemStats(report, preview);
            ValidateTaomuSword(report, preview);
            return report;
        }

        public static IReadOnlyList<BuildSandboxPlacedItemSnapshot> PlacedItems(
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items =
                preview?.context?.layoutSnapshot?.placedItems;
            return items ?? Array.Empty<BuildSandboxPlacedItemSnapshot>();
        }

        public static int CountItemStatScopeLeaks(
            BattleSandboxBuildCombatPreview preview)
        {
            return PlacedItems(preview).Count(item =>
                item == null
                || item.itemStat == null
                || !item.itemStat.devOnly
                || item.itemStat.isEnabled);
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if (preview == null)
            {
                report.AddError("ITEM_STAT_PREVIEW_NULL", "Default BuildSandbox preview was not created.", PackageName);
                return;
            }

            if (!preview.devOnly || preview.isEnabled)
            {
                report.AddError(
                    "ITEM_STAT_PREVIEW_SCOPE_LEAK",
                    "Source preview must stay devOnly=true and isEnabled=false.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "ITEM_STAT_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if ((preview.FormalFlowLeakCount
                    + preview.PlayerSideAnswerLeakCount
                    + preview.FeatureFlagDefaultTrueCount) != 0)
            {
                report.AddError(
                    "ITEM_STAT_PREVIEW_LEAK_COUNT_NONZERO",
                    "ItemStat foundation must reuse leak-free BuildSandbox preview data only.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            BuildSandboxPreviewContext context = preview.context;
            if (context == null
                || !context.devOnly
                || context.isEnabled
                || context.readsFormalSaveData
                || context.writesFormalFlow
                || context.writesFormalData
                || context.touchesFormalScene)
            {
                report.AddError(
                    "ITEM_STAT_CONTEXT_SCOPE_LEAK",
                    "Preview context must remain devOnly, disabled, and disconnected from formal flow/data/scene surfaces.",
                    nameof(BuildSandboxPreviewContext));
            }
        }

        private static void ValidateItemStats(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items = PlacedItems(preview);
            if (items.Count == 0)
            {
                report.AddError("ITEM_STAT_PLACED_ITEMS_EMPTY", "Default preview needs placed items.", PackageName);
                return;
            }

            int statProfileCount = 0;
            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item == null)
                {
                    report.AddError("ITEM_STAT_PLACED_ITEM_NULL", "Placed item snapshot is null.", PackageName);
                    continue;
                }

                BuildSandboxItemStat stat = item.itemStat;
                if (stat == null)
                {
                    report.AddError(
                        "ITEM_STAT_PROFILE_NULL",
                        $"ItemStat is missing for itemId={item.itemId}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(stat.statProfileId))
                {
                    report.AddError(
                        "ITEM_STAT_PROFILE_ID_MISSING",
                        $"ItemStat profile id is empty for itemId={item.itemId}.",
                        nameof(BuildSandboxItemStat));
                }

                if (!stat.devOnly)
                {
                    report.AddError(
                        "ITEM_STAT_DEVONLY_FALSE",
                        $"ItemStat must keep devOnly=true for itemId={item.itemId}.",
                        nameof(BuildSandboxItemStat));
                }

                if (stat.isEnabled)
                {
                    report.AddError(
                        "ITEM_STAT_ENABLED_TRUE",
                        $"ItemStat must keep isEnabled=false for itemId={item.itemId}.",
                        nameof(BuildSandboxItemStat));
                }

                if (stat.attack < 0
                    || stat.guard < 0
                    || stat.spirit < 0
                    || stat.control < 0
                    || stat.shieldBreak < 0
                    || stat.cleanse < 0
                    || stat.manaGainPerTick < 0
                    || stat.manaCostPerCast < 0)
                {
                    report.AddError(
                        "ITEM_STAT_NEGATIVE_VALUE",
                        $"ItemStat values cannot be negative for itemId={item.itemId}.",
                        nameof(BuildSandboxItemStat));
                }

                if (float.IsNaN(stat.castIntervalSeconds)
                    || float.IsInfinity(stat.castIntervalSeconds)
                    || stat.castIntervalSeconds <= 0f)
                {
                    report.AddError(
                        "ITEM_STAT_CAST_INTERVAL_INVALID",
                        $"ItemStat castIntervalSeconds must be positive for itemId={item.itemId}.",
                        nameof(BuildSandboxItemStat));
                }

                statProfileCount++;
            }

            report.AddInfo(
                "ITEM_STAT_PROFILE_COUNT",
                $"ItemStat profiles present on placed snapshots: {statProfileCount}.",
                nameof(BuildSandboxPlacedItemSnapshot));
        }

        private static void ValidateTaomuSword(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            BuildSandboxPlacedItemSnapshot item = PlacedItems(preview)
                .FirstOrDefault(snapshot => string.Equals(snapshot?.itemId, TaomuSwordItemId, StringComparison.Ordinal));

            if (item == null)
            {
                report.AddError(
                    "TAOMU_SWORD_MISSING",
                    "Default preview must include the devOnly taomu sword item.",
                    PackageName);
                return;
            }

            if (!string.Equals(item.shapeId, Vertical3ShapeId, StringComparison.Ordinal))
            {
                report.AddError(
                    "TAOMU_SWORD_SHAPE_MISMATCH",
                    $"Taomu sword shapeId must be {Vertical3ShapeId}. actual={item.shapeId}.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }

            if (!HasCell(item, 0, 0) || !HasCell(item, 0, 1) || !HasCell(item, 0, 2))
            {
                report.AddError(
                    "TAOMU_SWORD_VERTICAL3_CELLS_MISSING",
                    "Taomu sword must occupy a vertical three-cell preview footprint.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }

            BuildSandboxItemStat stat = item.itemStat;
            if (stat == null)
            {
                report.AddError("TAOMU_SWORD_STAT_MISSING", "Taomu sword ItemStat is missing.", nameof(BuildSandboxItemStat));
                return;
            }

            if (!string.Equals(stat.statProfileId, TaomuStatProfileId, StringComparison.Ordinal))
            {
                report.AddError(
                    "TAOMU_SWORD_STAT_PROFILE_MISMATCH",
                    $"Taomu sword statProfileId must be {TaomuStatProfileId}. actual={stat.statProfileId}.",
                    nameof(BuildSandboxItemStat));
            }

            if (stat.attack <= 0 || stat.guard <= 0 || stat.shieldBreak <= 0 || stat.cleanse <= 0)
            {
                report.AddError(
                    "TAOMU_SWORD_STAT_FOUNDATION_INCOMPLETE",
                    "Taomu sword must carry attack, guard, shieldBreak, and cleanse preview values.",
                    nameof(BuildSandboxItemStat));
            }

            if (stat.manaGainPerTick <= 0
                || stat.manaCostPerCast <= 0
                || float.IsNaN(stat.castIntervalSeconds)
                || float.IsInfinity(stat.castIntervalSeconds)
                || stat.castIntervalSeconds <= 0f)
            {
                report.AddError(
                    "TAOMU_SWORD_MANA_STAT_INCOMPLETE",
                    "Taomu sword must carry devOnly mana gain, mana cost, and cast interval values.",
                    nameof(BuildSandboxItemStat));
            }

            report.AddInfo(
                "TAOMU_SWORD_VERTICAL3_READY",
                "Taomu sword uses vertical_3 and a disabled devOnly ItemStat profile.",
                nameof(BuildSandboxPlacedItemSnapshot));
        }

        private static bool HasCell(BuildSandboxPlacedItemSnapshot item, int x, int y)
        {
            return item?.occupiedCells?.Any(cell => cell.x == x && cell.y == y) == true;
        }
    }

    public static class BuildSandboxItemIdentityFamilyCorrectionValidator
    {
        public const string PackageName = "V0.4-ItemIdentityFamilyCorrection01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/ItemIdentityFamilyCorrection01/[QA Only] Run Item Identity Family Correction";

        private static readonly string[] RequiredBasicDisplayNames =
        {
            "火符",
            "雷符",
            "护身符",
            "丹药",
            "聚灵石",
            "剑丸",
            "连锁雷符",
            "净化符",
            "镇魂符",
            "法印",
            "水符",
            "驱邪铃",
            "桃木牌"
        };

        private static readonly string[] RequiredV04DisplayNames =
        {
            "炽火符",
            "雷引剑符",
            "护阵木牌",
            "守护木牌",
            "净化折符",
            "炉芯石",
            "醒符香",
            "镇邪铃",
            "镇魂法印",
            "桃木剑"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                Validate()
            };
        }

        public static BattleSandboxBuildCombatPreview BuildDefaultPreview()
        {
            return BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreview();
        }

        public static IReadOnlyList<BuildSandboxPlacedItemSnapshot> PlacedItems(
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items =
                preview?.context?.layoutSnapshot?.placedItems;
            return items ?? Array.Empty<BuildSandboxPlacedItemSnapshot>();
        }

        public static int CountIdentityScopeLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return PlacedItems(preview).Count(item =>
                item == null
                || string.IsNullOrWhiteSpace(item.itemFamily)
                || string.IsNullOrWhiteSpace(item.tier)
                || string.IsNullOrWhiteSpace(item.relationshipToBase)
                || !BuildSandboxItemIdentityFamilyCatalog.IsAllowedTier(item.tier)
                || !BuildSandboxItemIdentityFamilyCatalog.IsAllowedRelationship(item.relationshipToBase)
                || IsReplacingBaseItem(item));
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Item Identity Family Correction 01");
            BattleSandboxBuildCombatPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateCatalogRows(report);
            ValidatePreviewItems(report);
            ValidatePlacedSnapshots(report, preview);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if (preview == null)
            {
                report.AddError("ITEM_FAMILY_PREVIEW_NULL", "Default BuildSandbox preview was not created.", PackageName);
                return;
            }

            if (!preview.devOnly || preview.isEnabled)
            {
                report.AddError(
                    "ITEM_FAMILY_PREVIEW_SCOPE_LEAK",
                    "Source preview must stay devOnly=true and isEnabled=false.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "ITEM_FAMILY_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if ((preview.FormalFlowLeakCount
                    + preview.PlayerSideAnswerLeakCount
                    + preview.FeatureFlagDefaultTrueCount) != 0)
            {
                report.AddError(
                    "ITEM_FAMILY_PREVIEW_LEAK_COUNT_NONZERO",
                    "Identity family correction must reuse leak-free BuildSandbox preview data only.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            BuildSandboxPreviewContext context = preview.context;
            if (context == null
                || !context.devOnly
                || context.isEnabled
                || context.readsFormalSaveData
                || context.writesFormalFlow
                || context.writesFormalData
                || context.touchesFormalScene)
            {
                report.AddError(
                    "ITEM_FAMILY_CONTEXT_SCOPE_LEAK",
                    "Preview context must remain devOnly, disabled, and disconnected from formal flow/data/scene surfaces.",
                    nameof(BuildSandboxPreviewContext));
            }
        }

        private static void ValidateCatalogRows(BuildSandboxValidationReport report)
        {
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> basics =
                BuildSandboxItemIdentityFamilyCatalog.BasicItems;
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> v04Rows =
                BuildSandboxItemIdentityFamilyCatalog.V04PreviewItems;

            if (basics.Count != RequiredBasicDisplayNames.Length)
            {
                report.AddError(
                    "ITEM_FAMILY_BASIC_COUNT_MISMATCH",
                    $"Basic item count must be {RequiredBasicDisplayNames.Length}. actual={basics.Count}.",
                    nameof(BuildSandboxItemIdentityFamilyCatalog));
            }

            if (v04Rows.Count != RequiredV04DisplayNames.Length)
            {
                report.AddError(
                    "ITEM_FAMILY_V04_COUNT_MISMATCH",
                    $"V0.4 item count must be {RequiredV04DisplayNames.Length}. actual={v04Rows.Count}.",
                    nameof(BuildSandboxItemIdentityFamilyCatalog));
            }

            ValidateRequiredNames(report, basics, RequiredBasicDisplayNames, "ITEM_FAMILY_BASIC_NAME_MISSING");
            ValidateRequiredNames(report, v04Rows, RequiredV04DisplayNames, "ITEM_FAMILY_V04_NAME_MISSING");

            foreach (BuildSandboxItemIdentityFamilyRecord row in basics)
            {
                ValidateCommonRow(report, row);
                if (!string.Equals(row.Tier, BuildSandboxItemIdentityFamilyCatalog.TierBasic, StringComparison.Ordinal)
                    || !string.Equals(row.RelationshipToBase, BuildSandboxItemIdentityFamilyCatalog.RelationshipBase, StringComparison.Ordinal)
                    || !string.Equals(row.BaseItemId, row.ItemId, StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_FAMILY_BASIC_RELATIONSHIP_INVALID",
                        $"Basic item {row.DisplayName} must keep tier=basic, relationshipToBase=base, and baseItemId=itemId.",
                        row.ItemId);
                }
            }

            foreach (BuildSandboxItemIdentityFamilyRecord row in v04Rows)
            {
                ValidateCommonRow(report, row);
                if (!row.DevOnly || row.IsEnabled)
                {
                    report.AddError(
                        "ITEM_FAMILY_V04_SCOPE_LEAK",
                        $"V0.4 preview item {row.DisplayName} must stay devOnly=true and isEnabled=false.",
                        row.ItemId);
                }

                if (string.Equals(row.Tier, BuildSandboxItemIdentityFamilyCatalog.TierBasic, StringComparison.Ordinal)
                    || string.Equals(row.RelationshipToBase, BuildSandboxItemIdentityFamilyCatalog.RelationshipBase, StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_FAMILY_V04_MARKED_AS_BASE",
                        $"V0.4 preview item {row.DisplayName} must not be marked as a basic/base item.",
                        row.ItemId);
                }

                if (IsReplacingBaseItem(row))
                {
                    report.AddError(
                        "ITEM_FAMILY_V04_REPLACES_BASE_ID",
                        $"V0.4 preview item {row.DisplayName} must not reuse or replace its base itemId.",
                        row.ItemId);
                }

                if (!string.Equals(row.RelationshipToBase, BuildSandboxItemIdentityFamilyCatalog.RelationshipNewMechanic, StringComparison.Ordinal)
                    && string.IsNullOrWhiteSpace(row.BaseItemId))
                {
                    report.AddError(
                        "ITEM_FAMILY_V04_BASE_MISSING",
                        $"V0.4 preview item {row.DisplayName} must keep a baseItemId unless it is a new mechanism.",
                        row.ItemId);
                }
            }

            report.AddInfo(
                "ITEM_FAMILY_CATALOG_COUNTS",
                $"basic={basics.Count}, v04={v04Rows.Count}.",
                nameof(BuildSandboxItemIdentityFamilyCatalog));
        }

        private static void ValidatePreviewItems(BuildSandboxValidationReport report)
        {
            List<BuildGridInteractionPreviewController.PreviewItem> previewItems =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            HashSet<string> seenIds = new(StringComparer.Ordinal);

            foreach (BuildGridInteractionPreviewController.PreviewItem item in previewItems)
            {
                if (item == null)
                {
                    report.AddError("ITEM_FAMILY_PREVIEW_ITEM_NULL", "Preview item row is null.", PackageName);
                    continue;
                }

                if (!seenIds.Add(item.ItemId))
                {
                    report.AddError(
                        "ITEM_FAMILY_PREVIEW_ID_DUPLICATE",
                        $"Duplicate preview item id: {item.ItemId}.",
                        nameof(BuildGridInteractionPreviewController));
                }

                BuildSandboxItemIdentityFamilyRecord expected =
                    BuildSandboxItemIdentityFamilyCatalog.Resolve(item.ItemId);
                if (!string.Equals(expected.DisplayName, item.DisplayName, StringComparison.Ordinal)
                    || !string.Equals(expected.ItemFamily, item.ItemFamily, StringComparison.Ordinal)
                    || !string.Equals(expected.BaseItemId, item.BaseItemId, StringComparison.Ordinal)
                    || !string.Equals(expected.Tier, item.Tier, StringComparison.Ordinal)
                    || !string.Equals(expected.RelationshipToBase, item.RelationshipToBase, StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_FAMILY_PREVIEW_METADATA_MISMATCH",
                        $"Preview metadata mismatch for itemId={item.ItemId}.",
                        nameof(BuildGridInteractionPreviewController.PreviewItem));
                }

                if (string.Equals(item.DisplayName, "聚能香", StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_FAMILY_OLD_INCENSE_NAME_PRESENT",
                        "preview_energy_incense must use the V0.4 correction display name 醒符香.",
                        item.ItemId);
                }
            }

            foreach (BuildSandboxItemIdentityFamilyRecord required in BuildSandboxItemIdentityFamilyCatalog.V04PreviewItems)
            {
                if (!seenIds.Contains(required.ItemId))
                {
                    report.AddError(
                        "ITEM_FAMILY_PREVIEW_ID_MISSING",
                        $"CreatePreviewItems is missing V0.4 preview itemId={required.ItemId}.",
                        nameof(BuildGridInteractionPreviewController));
                }
            }

            report.AddInfo(
                "ITEM_FAMILY_PREVIEW_ITEM_COUNT",
                $"CreatePreviewItems count={previewItems.Count}.",
                nameof(BuildGridInteractionPreviewController));
        }

        private static void ValidatePlacedSnapshots(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items = PlacedItems(preview);
            if (items.Count == 0)
            {
                report.AddError("ITEM_FAMILY_PLACED_ITEMS_EMPTY", "Default preview needs placed items.", PackageName);
                return;
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item == null)
                {
                    report.AddError("ITEM_FAMILY_PLACED_ITEM_NULL", "Placed item snapshot is null.", PackageName);
                    continue;
                }

                BuildSandboxItemIdentityFamilyRecord expected =
                    BuildSandboxItemIdentityFamilyCatalog.Resolve(item.itemId);
                if (!string.Equals(item.itemFamily, expected.ItemFamily, StringComparison.Ordinal)
                    || !string.Equals(item.baseItemId, expected.BaseItemId, StringComparison.Ordinal)
                    || !string.Equals(item.tier, expected.Tier, StringComparison.Ordinal)
                    || !string.Equals(item.relationshipToBase, expected.RelationshipToBase, StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_FAMILY_PLACED_METADATA_MISMATCH",
                        $"Placed snapshot metadata mismatch for itemId={item.itemId}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                }

                if (IsReplacingBaseItem(item))
                {
                    report.AddError(
                        "ITEM_FAMILY_PLACED_REPLACES_BASE_ID",
                        $"Placed V0.4 snapshot must not reuse its base itemId. itemId={item.itemId}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                }
            }

            report.AddInfo(
                "ITEM_FAMILY_PLACED_ITEM_COUNT",
                $"Placed snapshots with identity family fields: {items.Count}.",
                nameof(BuildSandboxPlacedItemSnapshot));
        }

        private static void ValidateRequiredNames(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSandboxItemIdentityFamilyRecord> rows,
            IReadOnlyList<string> requiredNames,
            string errorCode)
        {
            HashSet<string> names = new(rows.Select(row => row.DisplayName), StringComparer.Ordinal);
            foreach (string required in requiredNames)
            {
                if (!names.Contains(required))
                {
                    report.AddError(
                        errorCode,
                        $"Missing required classification item: {required}.",
                        nameof(BuildSandboxItemIdentityFamilyCatalog));
                }
            }
        }

        private static void ValidateCommonRow(
            BuildSandboxValidationReport report,
            BuildSandboxItemIdentityFamilyRecord row)
        {
            if (row == null)
            {
                report.AddError("ITEM_FAMILY_ROW_NULL", "Catalog row is null.", nameof(BuildSandboxItemIdentityFamilyCatalog));
                return;
            }

            if (string.IsNullOrWhiteSpace(row.ItemId)
                || string.IsNullOrWhiteSpace(row.DisplayName)
                || string.IsNullOrWhiteSpace(row.ItemFamily)
                || string.IsNullOrWhiteSpace(row.Tier)
                || string.IsNullOrWhiteSpace(row.RelationshipToBase))
            {
                report.AddError(
                    "ITEM_FAMILY_REQUIRED_FIELD_MISSING",
                    $"Catalog row has missing required fields. itemId={row.ItemId}, displayName={row.DisplayName}.",
                    nameof(BuildSandboxItemIdentityFamilyCatalog));
            }

            if (!BuildSandboxItemIdentityFamilyCatalog.IsAllowedTier(row.Tier))
            {
                report.AddError(
                    "ITEM_FAMILY_TIER_INVALID",
                    $"Invalid tier={row.Tier} for itemId={row.ItemId}.",
                    nameof(BuildSandboxItemIdentityFamilyCatalog));
            }

            if (!BuildSandboxItemIdentityFamilyCatalog.IsAllowedRelationship(row.RelationshipToBase))
            {
                report.AddError(
                    "ITEM_FAMILY_RELATIONSHIP_INVALID",
                    $"Invalid relationshipToBase={row.RelationshipToBase} for itemId={row.ItemId}.",
                    nameof(BuildSandboxItemIdentityFamilyCatalog));
            }
        }

        private static bool IsReplacingBaseItem(BuildSandboxItemIdentityFamilyRecord row)
        {
            return row != null
                && string.Equals(row.Classification, BuildSandboxItemIdentityFamilyCatalog.V04Classification, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(row.BaseItemId)
                && string.Equals(row.ItemId, row.BaseItemId, StringComparison.Ordinal);
        }

        private static bool IsReplacingBaseItem(BuildSandboxPlacedItemSnapshot item)
        {
            BuildSandboxItemIdentityFamilyRecord row =
                BuildSandboxItemIdentityFamilyCatalog.Resolve(item?.itemId);
            return string.Equals(row.Classification, BuildSandboxItemIdentityFamilyCatalog.V04Classification, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(item?.baseItemId)
                && string.Equals(item.itemId, item.baseItemId, StringComparison.Ordinal);
        }
    }
}
#endif
