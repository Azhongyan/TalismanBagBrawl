#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattlePageViewAdapterValidator
    {
        public const string PackageName = BattlePageViewAdapter.PackageName;
        public const int RequiredSpecSectionCount = 10;

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                BuildProblemRulePoolValidator.Validate(),
                BuildProblemSeedDataValidator.Validate(),
                BuildSandboxConfigPanelValidator.Validate(),
                BattleSandboxPreviewSceneVerifier.Validate(),
                BuildSandboxPreviewContextValidator.Validate(),
                Validate()
            };
        }

        public static BattlePageViewAdapter BuildDefaultAdapter()
        {
            return BattlePageViewAdapter.CreateDefault();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Battle Page View Adapter 01");
            BattlePageViewAdapter adapter = BuildDefaultAdapter();
            ValidateAdapterIsolation(report, adapter);
            ValidateSpec(report, adapter?.spec);
            ValidateBattlePrepareVisualLanguage(report, adapter?.spec);
            ValidateDataPanelDock(report, adapter?.spec?.dataPanelDock);
            ValidateBattleUiReuseSpec(report, adapter?.spec?.battleUiReuse);
            ValidateDeveloperTuningPanel(report, adapter?.spec?.developerTuningPanel);
            ValidatePlayerHintMaskingSpec(report, adapter?.spec?.playerHintMasking);
            ValidateReadOnlyFormalReferenceList(report, adapter?.spec);
            return report;
        }

        private static void ValidateAdapterIsolation(
            BuildSandboxValidationReport report,
            BattlePageViewAdapter adapter)
        {
            if (adapter == null)
            {
                report.AddError("BATTLE_PAGE_ADAPTER_NULL", "BattlePageViewAdapter was not created.", PackageName);
                return;
            }

            if (!string.Equals(adapter.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_PAGE_ADAPTER_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={adapter.packageName}.",
                    nameof(BattlePageViewAdapter));
            }

            if (!adapter.devOnly)
            {
                report.AddError(
                    "BATTLE_PAGE_ADAPTER_DEVONLY_FALSE",
                    "BattlePageViewAdapter must remain devOnly=true.",
                    nameof(BattlePageViewAdapter));
            }

            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_ENABLED_TRUE", adapter.isEnabled, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_READS_FORMAL_SAVE", adapter.readsFormalSaveData, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_WRITES_FORMAL_SCENE", adapter.writesFormalScene, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_WRITES_FORMAL_UI", adapter.writesFormalUi, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_SETPARENT_FORMAL_UI", adapter.setParentsFormalUi, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_OVERRIDES_FORMAL_LAYOUT", adapter.overridesFormalLayoutFrame, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_TOUCHES_V02_GRID", adapter.touchesV02FormationGridFrame, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_TOUCHES_PREPARE_CONTROLLER", adapter.touchesFormalPrepareController, nameof(BattlePageViewAdapter));
            ValidateFalse(report, "BATTLE_PAGE_ADAPTER_CONNECTS_FORMAL_BATTLE", adapter.connectsFormalBattle, nameof(BattlePageViewAdapter));

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BATTLE_PAGE_ADAPTER_FEATURE_FLAG_TRUE",
                    "All BuildSandbox FeatureFlags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PAGE_ADAPTER_ISOLATION_PASS",
                    "Adapter is devOnly, disabled, read-only, and disconnected from formal UI/battle/save surfaces.",
                    nameof(BattlePageViewAdapter));
            }
        }

        private static void ValidateSpec(BuildSandboxValidationReport report, BattlePageViewSpec spec)
        {
            if (spec == null)
            {
                report.AddError("BATTLE_PAGE_SPEC_NULL", "BattlePageViewSpec was not created.", PackageName);
                return;
            }

            if (!spec.devOnly)
            {
                report.AddError("BATTLE_PAGE_SPEC_DEVONLY_FALSE", "BattlePageViewSpec must keep devOnly=true.", nameof(BattlePageViewSpec));
            }

            ValidateFalse(report, "BATTLE_PAGE_SPEC_ENABLED_TRUE", spec.isEnabled, nameof(BattlePageViewSpec));
            ValidateFalse(report, "BATTLE_PAGE_SPEC_REUSES_FORMAL_GAMEOBJECTS", spec.reusesFormalGameObjects, nameof(BattlePageViewSpec));
            ValidateFalse(report, "BATTLE_PAGE_SPEC_WRITES_FORMAL_UI", spec.writesFormalUi, nameof(BattlePageViewSpec));

            if (!spec.referenceOnly)
            {
                report.AddError(
                    "BATTLE_PAGE_SPEC_NOT_REFERENCE_ONLY",
                    "BattlePageViewSpec must be referenceOnly=true.",
                    nameof(BattlePageViewSpec));
            }

            IReadOnlyList<BattlePageViewSpecSection> sections = spec.BuildSections();
            if (sections.Count < RequiredSpecSectionCount)
            {
                report.AddError(
                    "BATTLE_PAGE_SPEC_SECTION_COUNT_LOW",
                    $"Adapter must output at least {RequiredSpecSectionCount} spec sections; actual={sections.Count}.",
                    nameof(BattlePageViewSpec));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PAGE_SPEC_SECTION_COUNT_PASS",
                    $"Adapter spec section count={sections.Count}.",
                    nameof(BattlePageViewSpec));
            }

            string[] requiredSections =
            {
                "BoardGrid",
                "ItemTray",
                "CategoryTabs",
                "SelectedItemInfo",
                "PlacementFeedback",
                "ActionButtons",
                "DataPanelDock",
                "BattleUiReuse",
                "DeveloperTuningPanel",
                "PlayerHintMasking"
            };
            HashSet<string> actualSections = new(sections.Select(section => section.SectionId), StringComparer.Ordinal);
            foreach (string required in requiredSections)
            {
                if (!actualSections.Contains(required))
                {
                    report.AddError("BATTLE_PAGE_SPEC_SECTION_MISSING", $"Missing spec section: {required}.", nameof(BattlePageViewSpec));
                }
            }

            int writableSections = sections.Count(section => section.CanWriteFormalUi || !section.ReferenceOnly);
            if (writableSections > 0)
            {
                report.AddError(
                    "BATTLE_PAGE_SPEC_WRITABLE_SECTION",
                    $"All sections must be read-only formal references. writableSections={writableSections}.",
                    nameof(BattlePageViewSpecSection));
            }
        }

        private static void ValidateBattlePrepareVisualLanguage(
            BuildSandboxValidationReport report,
            BattlePageViewSpec spec)
        {
            if (spec?.battlePrepare == null || spec.boardGrid == null || spec.itemTray == null || spec.categoryTabs == null)
            {
                report.AddError(
                    "BATTLE_PAGE_VISUAL_SPEC_NULL",
                    "Battle prepare, board, item tray, and category specs must all exist.",
                    nameof(BattlePageViewSpec));
                return;
            }

            ValidateFloat(report, "BATTLE_PAGE_BOARD_SIZE", "Board size", spec.boardGrid.width, 800f, nameof(BattleGridVisualSpec));
            ValidateFloat(report, "BATTLE_PAGE_BOARD_HEIGHT", "Board height", spec.boardGrid.height, 800f, nameof(BattleGridVisualSpec));
            ValidateFloat(report, "BATTLE_PAGE_ITEM_TRAY_SIZE", "Item tray width", spec.itemTray.width, 800f, nameof(ItemTrayVisualSpec));
            ValidateFloat(report, "BATTLE_PAGE_ITEM_TRAY_HEIGHT", "Item tray height", spec.itemTray.height, 800f, nameof(ItemTrayVisualSpec));
            ValidateMinimum(report, "BATTLE_PAGE_TRAY_COLUMNS", "Item tray columns", spec.itemTray.columnCount, 5);
            ValidateMinimum(report, "BATTLE_PAGE_TRAY_ROWS", "Item tray rows", spec.itemTray.rowCount, 8);
            ValidateMinimum(report, "BATTLE_PAGE_TRAY_SLOT_COUNT", "Item tray slots", spec.itemTray.slotCount, 40);
            ValidateMinimum(report, "BATTLE_PAGE_CATEGORY_COUNT", "Category tabs", spec.categoryTabs.categoryIds?.Count ?? 0, 6);

            if (!spec.itemTray.verticalScroll)
            {
                report.AddError("BATTLE_PAGE_TRAY_SCROLL_FALSE", "ItemTray spec must expose vertical scroll.", nameof(ItemTrayVisualSpec));
            }

            if (!spec.battlePrepare.overlayBehindBoardAndTray || !spec.battlePrepare.boardAndTrayMoveTogether)
            {
                report.AddError(
                    "BATTLE_PAGE_PREPARE_LANGUAGE_MISMATCH",
                    "Battle prepare visual language must keep overlay behind board/tray and board+tray synchronized.",
                    nameof(BattlePrepareVisualSpec));
            }
        }

        private static void ValidateDataPanelDock(
            BuildSandboxValidationReport report,
            DataPanelDockBindingSpec dock)
        {
            if (dock == null)
            {
                report.AddError("BATTLE_PAGE_DATA_DOCK_NULL", "DataPanelDock spec is missing.", nameof(DataPanelDockBindingSpec));
                return;
            }

            string[] requiredSlots =
            {
                "BuildSummaryPanelSlot",
                "SynergyPanelSlot",
                "ShapeOccupancyPanelSlot",
                "AffixModifierPanelSlot",
                "ProblemReadinessPanelSlot",
                "SimulationResultPanelSlot"
            };
            HashSet<string> actualSlots = new(dock.bindingSlotNames ?? new List<string>(), StringComparer.Ordinal);
            foreach (string required in requiredSlots)
            {
                if (!actualSlots.Contains(required))
                {
                    report.AddError("BATTLE_PAGE_DATA_DOCK_SLOT_MISSING", $"Missing data panel binding slot: {required}.", nameof(DataPanelDockBindingSpec));
                }
            }

            if (dock.canWriteFormalUi || !dock.referenceOnly)
            {
                report.AddError(
                    "BATTLE_PAGE_DATA_DOCK_WRITABLE",
                    "DataPanelDock binding spec must stay V04-only and read-only.",
                    nameof(DataPanelDockBindingSpec));
            }
        }

        private static void ValidateBattleUiReuseSpec(
            BuildSandboxValidationReport report,
            BattleUiReuseSpec spec)
        {
            if (spec == null)
            {
                report.AddError("BATTLE_UI_REUSE_SPEC_NULL", "BattleUiReuse spec is missing.", nameof(BattleUiReuseSpec));
                return;
            }

            if (!spec.referenceOnly || spec.canWriteFormalUi)
            {
                report.AddError(
                    "BATTLE_UI_REUSE_WRITABLE",
                    "BattleUiReuse spec must stay read-only and cannot write formal UI.",
                    nameof(BattleUiReuseSpec));
            }

            if (spec.createsDedicatedMechanicPanels)
            {
                report.AddError(
                    "BATTLE_UI_REUSE_DEDICATED_PANEL",
                    "BattleUiReuse must prefer shared UI language instead of one panel per mechanic.",
                    nameof(BattleUiReuseSpec));
            }

            string[] requiredChannels =
            {
                "BattleHint",
                "DamageFeedback",
                "CombatLog",
                "Tooltip",
                "BossInfo"
            };
            IReadOnlyList<BattleUiReuseRecommendation> rows =
                spec.recommendations ?? new List<BattleUiReuseRecommendation>();
            HashSet<string> channels = new(rows.Select(row => row.channelId), StringComparer.Ordinal);
            foreach (string channel in requiredChannels)
            {
                if (!channels.Contains(channel))
                {
                    report.AddError(
                        "BATTLE_UI_REUSE_CHANNEL_MISSING",
                        $"Missing UI reuse channel: {channel}.",
                        nameof(BattleUiReuseSpec));
                }
            }

            foreach (BattleUiReuseRecommendation row in rows)
            {
                if (string.IsNullOrWhiteSpace(row?.englishStableKey) || string.IsNullOrWhiteSpace(row.chineseDisplayName))
                {
                    report.AddError(
                        "BATTLE_UI_REUSE_LABEL_MISSING",
                        "Each UI reuse row needs a Chinese display name and English stable key.",
                        nameof(BattleUiReuseRecommendation));
                    continue;
                }

                if (!row.referenceOnly || row.canWriteFormalUi || row.createsDedicatedPanel)
                {
                    report.AddError(
                        "BATTLE_UI_REUSE_ROW_WRITABLE",
                        $"UI reuse row must stay read-only and shared. channel={row.channelId}.",
                        nameof(BattleUiReuseRecommendation));
                }
            }

            if (report.ErrorCount == 0)
            {
                report.AddInfo(
                    "BATTLE_UI_REUSE_PASS",
                    $"UI reuse channels present={rows.Count}.",
                    nameof(BattleUiReuseSpec));
            }
        }

        private static void ValidateDeveloperTuningPanel(
            BuildSandboxValidationReport report,
            DeveloperTuningPanelFieldSpec spec)
        {
            if (spec == null)
            {
                report.AddError("DEVELOPER_TUNING_PANEL_SPEC_NULL", "Developer tuning field spec is missing.", nameof(DeveloperTuningPanelFieldSpec));
                return;
            }

            if (!spec.referenceOnly || spec.canWriteFormalUi)
            {
                report.AddError(
                    "DEVELOPER_TUNING_PANEL_WRITABLE",
                    "Developer tuning field spec must stay report/spec only.",
                    nameof(DeveloperTuningPanelFieldSpec));
            }

            if (!spec.playerUiChineseOnly)
            {
                report.AddError(
                    "PLAYER_UI_NOT_CHINESE_ONLY",
                    "Player UI must remain Chinese-only.",
                    nameof(DeveloperTuningPanelFieldSpec));
            }

            if (spec.playerUiShowsEnglishStableKey)
            {
                report.AddError(
                    "PLAYER_UI_SHOWS_ENGLISH_KEY",
                    "English stable keys must not be shown in player UI.",
                    nameof(DeveloperTuningPanelFieldSpec));
            }

            if (!spec.englishStableKeysConfigReportDevPanelOnly)
            {
                report.AddError(
                    "ENGLISH_KEY_SCOPE_INVALID",
                    "English stable keys are only for config/report/developer data-panel field layers.",
                    nameof(DeveloperTuningPanelFieldSpec));
            }

            IReadOnlyList<DeveloperTuningFieldBinding> fields =
                spec.fields ?? new List<DeveloperTuningFieldBinding>();
            ValidateMinimum(report, "DEVELOPER_TUNING_FIELD_COUNT", "Developer tuning fields", fields.Count, 8);
            foreach (DeveloperTuningFieldBinding field in fields)
            {
                if (string.IsNullOrWhiteSpace(field?.englishStableKey)
                    || string.IsNullOrWhiteSpace(field.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(field.dataPanelSlot))
                {
                    report.AddError(
                        "DEVELOPER_TUNING_FIELD_LABEL_MISSING",
                        "Every developer tuning field needs an English stable key, Chinese display name, and V04 data-panel slot.",
                        nameof(DeveloperTuningFieldBinding));
                    continue;
                }

                if (!field.developerOnly)
                {
                    report.AddError(
                        "DEVELOPER_TUNING_FIELD_NOT_DEV_ONLY",
                        $"Developer tuning field must be developerOnly. key={field.englishStableKey}.",
                        nameof(DeveloperTuningFieldBinding));
                }
            }
        }

        private static void ValidatePlayerHintMaskingSpec(
            BuildSandboxValidationReport report,
            PlayerHintMaskingSpec spec)
        {
            if (spec == null)
            {
                report.AddError("PLAYER_HINT_MASKING_SPEC_NULL", "PlayerHint masking spec is missing.", nameof(PlayerHintMaskingSpec));
                return;
            }

            if (!spec.referenceOnly || spec.canWriteFormalUi)
            {
                report.AddError(
                    "PLAYER_HINT_MASKING_WRITABLE",
                    "PlayerHint masking spec must stay report/spec only.",
                    nameof(PlayerHintMaskingSpec));
            }

            if (!spec.playerUiChineseOnly)
            {
                report.AddError("PLAYER_HINT_NOT_CHINESE_ONLY", "Player hint UI must be Chinese-only.", nameof(PlayerHintMaskingSpec));
            }

            if (spec.playerUiShowsEnglishStableKey)
            {
                report.AddError("PLAYER_HINT_SHOWS_ENGLISH_KEY", "Player hint UI must not show English stable keys.", nameof(PlayerHintMaskingSpec));
            }

            if (!spec.developerPanelMayShowFullRules)
            {
                report.AddError(
                    "DEVELOPER_PANEL_CANNOT_SHOW_FULL_RULES",
                    "Developer tuning view must be allowed to show full rules for QA and tuning.",
                    nameof(PlayerHintMaskingSpec));
            }

            if (spec.playerHintPreviewShowsFullAnswers)
            {
                report.AddError(
                    "PLAYER_HINT_SHOWS_FULL_ANSWERS",
                    "Player hint preview must not show full answers.",
                    nameof(PlayerHintMaskingSpec));
            }

            string[] requiredMaskedFields =
            {
                "hardSolutionTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "dropBiasWeights",
                "bossSixKeyFullAnswer"
            };
            IReadOnlyList<PlayerHintMaskingRule> rules =
                spec.maskingRules ?? new List<PlayerHintMaskingRule>();
            HashSet<string> actualRules = new(rules.Select(rule => rule.englishStableKey), StringComparer.Ordinal);
            foreach (string required in requiredMaskedFields)
            {
                if (!actualRules.Contains(required))
                {
                    report.AddError(
                        "PLAYER_HINT_MASK_RULE_MISSING",
                        $"Missing masking rule: {required}.",
                        nameof(PlayerHintMaskingSpec));
                }
            }

            foreach (PlayerHintMaskingRule rule in rules)
            {
                if (string.IsNullOrWhiteSpace(rule?.englishStableKey)
                    || string.IsNullOrWhiteSpace(rule.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(rule.playerChineseHintPolicy))
                {
                    report.AddError(
                        "PLAYER_HINT_MASK_RULE_LABEL_MISSING",
                        "Every masking rule needs an English stable key, Chinese display name, and Chinese hint policy.",
                        nameof(PlayerHintMaskingRule));
                    continue;
                }

                if (!rule.maskRequired || rule.playerVisible || !rule.developerDataPanelVisible)
                {
                    report.AddError(
                        "PLAYER_HINT_MASK_RULE_LEAK",
                        $"Sensitive field must be masked from player UI and visible only in config/report/developer data-panel layers. key={rule.englishStableKey}.",
                        nameof(PlayerHintMaskingRule));
                }
            }

            if (report.ErrorCount == 0)
            {
                report.AddInfo(
                    "PLAYER_HINT_MASKING_PASS",
                    $"Player hint masking rules present={rules.Count}.",
                    nameof(PlayerHintMaskingSpec));
            }
        }

        private static void ValidateReadOnlyFormalReferenceList(
            BuildSandboxValidationReport report,
            BattlePageViewSpec spec)
        {
            if (spec == null)
            {
                return;
            }

            string[] expectedReferences =
            {
                BattlePageViewFormalReferenceKeys.PrepareController,
                "V02FormationGridFrame",
                BattlePageViewFormalReferenceKeys.BottomOperationArea,
                "V02PrimaryActionButtons"
            };
            HashSet<string> references = new(spec.readOnlyFormalReferences ?? new List<string>(), StringComparer.Ordinal);
            foreach (string expected in expectedReferences)
            {
                if (!references.Contains(expected))
                {
                    report.AddError(
                        "BATTLE_PAGE_FORMAL_REFERENCE_MISSING",
                        $"Read-only reference list should document {expected}.",
                        nameof(BattlePageViewSpec));
                }
            }

            report.AddInfo(
                "BATTLE_PAGE_FORMAL_REFERENCE_READONLY",
                "Formal battle page names are recorded as strings for read-only specification reports only.",
                nameof(BattlePageViewSpec));
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
                report.AddError(code, $"{label} too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} pass. actual={actual}, expected>={expected}.", PackageName);
        }

        private static void ValidateFloat(
            BuildSandboxValidationReport report,
            string code,
            string label,
            float actual,
            float expected,
            string path)
        {
            if (Math.Abs(actual - expected) > 0.01f)
            {
                report.AddError(code, $"{label} mismatch. actual={actual}, expected={expected}.", path);
                return;
            }

            report.AddInfo(code, $"{label} pass. actual={actual}.", path);
        }
    }
}
#endif
