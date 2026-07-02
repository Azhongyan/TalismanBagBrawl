#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattlePrepareComponentAdapterValidator
    {
        public const string PackageName = BattlePrepareComponentAdapter.PackageName;
        public const int RequiredOutputSectionCount = 5;

        private static readonly string[] RequiredExtensionIds =
        {
            "BoardOccupancyExtension",
            "ItemTrayShapeExtension",
            "DragRotationPlacementExtension",
            "ItemInfoBuildFieldsExtension",
            "BattleFeedbackMechanicHintExtension"
        };

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "Boss六钥匙",
            "Boss 六钥匙"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxPreviewContextValidator.BuildValidationReports();
            reports.Add(BattlePageViewAdapterValidator.Validate());
            reports.Add(Validate());
            return reports;
        }

        public static BattlePrepareComponentAdapter BuildDefaultAdapter()
        {
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            BattlePageViewAdapter battlePageAdapter =
                BattlePageViewAdapterValidator.BuildDefaultAdapter();
            return BattlePrepareComponentAdapter.Build(context, battlePageAdapter);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattlePrepare Component Adapter 01");
            BattlePrepareComponentAdapter adapter = BuildDefaultAdapter();
            ValidateIsolation(report, adapter);
            ValidateSections(report, adapter?.viewModel);
            ValidateBoardOccupancy(report, adapter?.viewModel?.boardOccupancy);
            ValidateItemTrayShape(report, adapter?.viewModel?.itemTrayShape);
            ValidateDragRotation(report, adapter?.viewModel?.dragRotationPlacement);
            ValidateItemInfoFields(report, adapter?.viewModel?.itemInfoBuildFields);
            ValidateFeedback(report, adapter?.viewModel?.battleFeedbackMechanicHint);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapter adapter)
        {
            if (adapter == null)
            {
                report.AddError("BATTLE_PREPARE_COMPONENT_ADAPTER_NULL", "Adapter was not created.", PackageName);
                return;
            }

            if (!string.Equals(adapter.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_PREPARE_COMPONENT_ADAPTER_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={adapter.packageName}.",
                    nameof(BattlePrepareComponentAdapter));
            }

            if (!adapter.devOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_COMPONENT_ADAPTER_DEVONLY_FALSE",
                    "Adapter must remain devOnly=true.",
                    nameof(BattlePrepareComponentAdapter));
            }

            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_ENABLED_TRUE", adapter.isEnabled, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_READS_FORMAL_SAVE", adapter.readsFormalSaveData, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_WRITES_FORMAL_SCENE", adapter.writesFormalScene, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_WRITES_FORMAL_UI", adapter.writesFormalUi, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_SETPARENT_FORMAL_UI", adapter.setParentsFormalUi, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_OVERRIDES_LAYOUT", adapter.overridesFormalLayoutFrame, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_PREPARE_CONTROLLER", adapter.touchesFormalPrepareController, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_RUNFLOW", adapter.touchesRunFlow, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_PAGESTATE", adapter.touchesPageState, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_FORMATIONSTATE", adapter.touchesFormationState, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_SAVE", adapter.touchesSaveData, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_TOUCHES_BOSS_REWARD_NUMERIC", adapter.touchesBossRewardDropNumeric, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_PLAYER_SHOWS_ENGLISH_KEY", adapter.playerUiShowsEnglishStableKey, nameof(BattlePrepareComponentAdapter));
            ValidateFalse(report, "BATTLE_PREPARE_COMPONENT_ADAPTER_PLAYER_SHOWS_FULL_ANSWERS", adapter.playerUiShowsFullAnswers, nameof(BattlePrepareComponentAdapter));

            if (!string.Equals(adapter.referenceMode, BattlePrepareComponentAdapter.ReferenceMode, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_PREPARE_COMPONENT_ADAPTER_REFERENCE_MODE_INVALID",
                    $"Reference mode mismatch. actual={adapter.referenceMode}.",
                    nameof(BattlePrepareComponentAdapter));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BATTLE_PREPARE_COMPONENT_ADAPTER_FEATURE_FLAG_TRUE",
                    "All BuildSandbox FeatureFlags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_COMPONENT_ADAPTER_ISOLATION_PASS",
                    "Adapter is devOnly, disabled, and disconnected from formal UI/battle/save surfaces.",
                    nameof(BattlePrepareComponentAdapter));
            }
        }

        private static void ValidateSections(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterViewModel viewModel)
        {
            if (viewModel == null)
            {
                report.AddError("BATTLE_PREPARE_COMPONENT_VIEWMODEL_NULL", "Adapter ViewModel was not created.", PackageName);
                return;
            }

            ValidateMinimum(
                report,
                "BATTLE_PREPARE_COMPONENT_SECTION_COUNT",
                "Output section",
                viewModel.OutputSectionCount,
                RequiredOutputSectionCount);

            HashSet<string> actual = new(StringComparer.Ordinal)
            {
                viewModel.boardOccupancy?.extensionId ?? string.Empty,
                viewModel.itemTrayShape?.extensionId ?? string.Empty,
                viewModel.dragRotationPlacement?.extensionId ?? string.Empty,
                viewModel.itemInfoBuildFields?.extensionId ?? string.Empty,
                viewModel.battleFeedbackMechanicHint?.extensionId ?? string.Empty
            };

            foreach (string required in RequiredExtensionIds)
            {
                if (!actual.Contains(required))
                {
                    report.AddError(
                        "BATTLE_PREPARE_COMPONENT_EXTENSION_MISSING",
                        $"Missing extension layer: {required}.",
                        nameof(BattlePrepareComponentAdapterViewModel));
                }
            }
        }

        private static void ValidateBoardOccupancy(
            BuildSandboxValidationReport report,
            BoardOccupancyExtensionViewModel model)
        {
            if (model == null)
            {
                report.AddError("BOARD_OCCUPANCY_EXTENSION_NULL", "BoardOccupancyExtension is missing.", nameof(BoardOccupancyExtensionViewModel));
                return;
            }

            ValidateExtensionIsolation(report, model.extensionId, model.canWriteFormalUi, model.requiresExplicitRuntimeBinding);
            ValidateMinimum(report, "BOARD_OCCUPANCY_CELL_COUNT", "Occupied cell", model.occupiedCellCount, 1);
            ValidateMinimum(report, "BOARD_OCCUPANCY_ITEM_COUNT", "Placed item", model.placedItemCount, 1);

            if (!string.Equals(model.targetNodeName, "V02FormationGridFrame", StringComparison.Ordinal))
            {
                report.AddError(
                    "BOARD_OCCUPANCY_TARGET_INVALID",
                    $"Board target must reference mature board source. actual={model.targetNodeName}.",
                    nameof(BoardOccupancyExtensionViewModel));
            }

            ValidatePlayerTextRows(report, model.cells.Select(row => row.playerHintChinese), nameof(BoardOccupancyCellRow));
        }

        private static void ValidateItemTrayShape(
            BuildSandboxValidationReport report,
            ItemTrayShapeExtensionViewModel model)
        {
            if (model == null)
            {
                report.AddError("ITEM_TRAY_SHAPE_EXTENSION_NULL", "ItemTrayShapeExtension is missing.", nameof(ItemTrayShapeExtensionViewModel));
                return;
            }

            ValidateExtensionIsolation(report, model.extensionId, model.canWriteFormalUi, model.requiresExplicitRuntimeBinding);
            ValidateMinimum(report, "ITEM_TRAY_SHAPE_ITEM_COUNT", "Item tray shape row", model.items?.Count ?? 0, 1);
            ValidateMinimum(report, "ITEM_TRAY_SHAPE_CATEGORY_COUNT", "Category label", model.categoryLabels?.Count ?? 0, 6);

            if (!string.Equals(model.targetNodeName, BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot, StringComparison.Ordinal))
            {
                report.AddError(
                    "ITEM_TRAY_SHAPE_TARGET_INVALID",
                    $"Item tray target must reference mature BattlePrepare item tray. actual={model.targetNodeName}.",
                    nameof(ItemTrayShapeExtensionViewModel));
            }

            foreach (ItemTrayShapeRow row in model.items ?? new List<ItemTrayShapeRow>())
            {
                if (row == null
                    || string.IsNullOrWhiteSpace(row.itemId)
                    || string.IsNullOrWhiteSpace(row.shapeId)
                    || row.occupiedCellCount <= 0)
                {
                    report.AddError(
                        "ITEM_TRAY_SHAPE_ROW_INCOMPLETE",
                        "Each item tray shape row needs itemId, shapeId, and occupied cell count.",
                        nameof(ItemTrayShapeRow));
                }
            }
        }

        private static void ValidateDragRotation(
            BuildSandboxValidationReport report,
            DragRotationPlacementExtensionViewModel model)
        {
            if (model == null)
            {
                report.AddError("DRAG_ROTATION_EXTENSION_NULL", "DragRotationPlacementExtension is missing.", nameof(DragRotationPlacementExtensionViewModel));
                return;
            }

            ValidateExtensionIsolation(report, model.extensionId, model.canWriteFormalUi, model.requiresExplicitRuntimeBinding);
            ValidateMinimum(report, "DRAG_ROTATION_ROW_COUNT", "Drag/rotation placement row", model.rows?.Count ?? 0, 1);
            ValidatePlayerTextRows(report, model.rows.Select(row => row?.playerFeedbackChinese), nameof(DragRotationPlacementRow));
        }

        private static void ValidateItemInfoFields(
            BuildSandboxValidationReport report,
            ItemInfoBuildFieldsExtensionViewModel model)
        {
            if (model == null)
            {
                report.AddError("ITEM_INFO_FIELDS_EXTENSION_NULL", "ItemInfoBuildFieldsExtension is missing.", nameof(ItemInfoBuildFieldsExtensionViewModel));
                return;
            }

            ValidateExtensionIsolation(report, model.extensionId, model.canWriteFormalUi, model.requiresExplicitRuntimeBinding);
            ValidateMinimum(report, "ITEM_INFO_FIELDS_ITEM_COUNT", "Item info row", model.items?.Count ?? 0, 1);

            foreach (ItemInfoBuildFieldsRow row in model.items ?? new List<ItemInfoBuildFieldsRow>())
            {
                if (row == null)
                {
                    report.AddError("ITEM_INFO_FIELDS_ROW_NULL", "Null item info row.", nameof(ItemInfoBuildFieldsRow));
                    continue;
                }

                ValidatePlayerTextRows(report, new[] { row.displayNameChinese, row.playerSummaryChinese }, nameof(ItemInfoBuildFieldsRow));
                ValidateMinimum(report, "ITEM_INFO_FIELD_COUNT", "Item info field", row.fields?.Count ?? 0, 5);
                foreach (BattlePrepareItemInfoFieldRow field in row.fields ?? new List<BattlePrepareItemInfoFieldRow>())
                {
                    ValidateField(report, field);
                }
            }
        }

        private static void ValidateFeedback(
            BuildSandboxValidationReport report,
            BattleFeedbackMechanicHintExtensionViewModel model)
        {
            if (model == null)
            {
                report.AddError("BATTLE_FEEDBACK_EXTENSION_NULL", "BattleFeedbackMechanicHintExtension is missing.", nameof(BattleFeedbackMechanicHintExtensionViewModel));
                return;
            }

            ValidateExtensionIsolation(report, model.extensionId, model.canWriteFormalUi, model.requiresExplicitRuntimeBinding);
            ValidateMinimum(report, "BATTLE_FEEDBACK_ROW_COUNT", "Feedback hint row", model.rows?.Count ?? 0, 1);
            foreach (BattleFeedbackMechanicHintRow row in model.rows ?? new List<BattleFeedbackMechanicHintRow>())
            {
                ValidateLabelPair(report, row?.englishStableKey, row?.chineseDisplayName, nameof(BattleFeedbackMechanicHintRow));
                if (row != null && row.canWriteFormalUi)
                {
                    report.AddError(
                        "BATTLE_FEEDBACK_ROW_WRITES_FORMAL_UI",
                        $"Feedback row must not write formal UI. key={row.englishStableKey}.",
                        nameof(BattleFeedbackMechanicHintRow));
                }
            }

            ValidatePlayerTextRows(
                report,
                model.rows
                    .Where(row => row != null && row.playerVisible)
                    .Select(row => row.playerChineseText),
                nameof(BattleFeedbackMechanicHintRow));
        }

        private static void ValidateField(
            BuildSandboxValidationReport report,
            BattlePrepareItemInfoFieldRow field)
        {
            if (field == null)
            {
                report.AddError("ITEM_INFO_FIELD_NULL", "Null item info field.", nameof(BattlePrepareItemInfoFieldRow));
                return;
            }

            ValidateLabelPair(report, field.englishStableKey, field.chineseDisplayName, nameof(BattlePrepareItemInfoFieldRow));
            if (field.canWriteFormalUi)
            {
                report.AddError(
                    "ITEM_INFO_FIELD_WRITES_FORMAL_UI",
                    $"Item info field must not write formal UI. key={field.englishStableKey}.",
                    nameof(BattlePrepareItemInfoFieldRow));
            }

            if (field.developerOnly && (!field.maskedFromPlayer || field.playerVisible))
            {
                report.AddError(
                    "ITEM_INFO_DEVELOPER_FIELD_LEAK",
                    $"Developer-only item info field must be masked from player UI. key={field.englishStableKey}.",
                    nameof(BattlePrepareItemInfoFieldRow));
            }

            if (field.playerVisible)
            {
                ValidatePlayerTextRows(report, new[] { field.playerChineseText }, nameof(BattlePrepareItemInfoFieldRow));
            }
        }

        private static void ValidateExtensionIsolation(
            BuildSandboxValidationReport report,
            string extensionId,
            bool canWriteFormalUi,
            bool requiresExplicitRuntimeBinding)
        {
            if (canWriteFormalUi)
            {
                report.AddError(
                    "BATTLE_PREPARE_EXTENSION_WRITES_FORMAL_UI",
                    $"Extension must not write formal UI. extension={extensionId}.",
                    extensionId);
            }

            if (!requiresExplicitRuntimeBinding)
            {
                report.AddError(
                    "BATTLE_PREPARE_EXTENSION_RUNTIME_AUTOBIND",
                    $"Extension must require explicit future runtime binding. extension={extensionId}.",
                    extensionId);
            }
        }

        private static void ValidatePlayerTextRows(
            BuildSandboxValidationReport report,
            IEnumerable<string> values,
            string path)
        {
            foreach (string value in values ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (value.Any(character => character <= 127 && char.IsLetter(character)))
                {
                    report.AddError(
                        "BATTLE_PREPARE_PLAYER_TEXT_NOT_CHINESE_ONLY",
                        $"Player-facing text contains Latin letters: {value}.",
                        path);
                }

                foreach (string token in ForbiddenPlayerAnswerTokens)
                {
                    if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "BATTLE_PREPARE_PLAYER_TEXT_FORBIDDEN_TOKEN",
                            $"Player-facing text exposes forbidden answer token: {token}.",
                            path);
                    }
                }
            }
        }

        private static void ValidateLabelPair(
            BuildSandboxValidationReport report,
            string englishStableKey,
            string chineseDisplayName,
            string path)
        {
            if (string.IsNullOrWhiteSpace(englishStableKey)
                || string.IsNullOrWhiteSpace(chineseDisplayName))
            {
                report.AddError(
                    "BATTLE_PREPARE_LABEL_PAIR_MISSING",
                    "Every field row needs Chinese display name and English stable key.",
                    path);
                return;
            }

            if (!englishStableKey.All(character =>
                    character <= 127
                    && (char.IsLetterOrDigit(character)
                        || character == '_'
                        || character == '-'
                        || character == '.')))
            {
                report.AddError(
                    "BATTLE_PREPARE_ENGLISH_KEY_INVALID",
                    $"English stable key must be ASCII and stable. key={englishStableKey}.",
                    path);
            }

            if (!chineseDisplayName.Any(character => character > 127))
            {
                report.AddError(
                    "BATTLE_PREPARE_CHINESE_NAME_MISSING",
                    $"Chinese display name must contain Chinese text. key={englishStableKey}.",
                    path);
            }
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string path)
        {
            if (value)
            {
                report.AddError(code, "This isolation flag must stay false.", path);
            }
        }

        private static void ValidateMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
        }
    }
}
#endif
