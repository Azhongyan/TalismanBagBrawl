#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class FormationCorePowerRangeValidator
    {
        public const string PackageName = FormationCorePowerRangePreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/FormationCoreAndPowerRange01/[QA Only] Run Formation Core Power Range";

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

        public static FormationCorePowerRangePreview BuildDefaultPowerPreview()
        {
            BattleSandboxBuildCombatPreview preview = BuildDefaultPreview();
            return FormationCorePowerRangeResolver.Apply(preview?.context?.layoutSnapshot);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Formation Core And Power Range 01");
            BattleSandboxBuildCombatPreview combatPreview = BuildDefaultPreview();
            FormationCorePowerRangePreview powerPreview =
                FormationCorePowerRangeResolver.Apply(combatPreview?.context?.layoutSnapshot);

            ValidateIsolation(report, combatPreview, powerPreview);
            ValidateCoreAndPowerRows(report, combatPreview, powerPreview);
            ValidateFeedbackConnection(report, combatPreview);
            return report;
        }

        public static IReadOnlyList<BuildSandboxPlacedItemSnapshot> PlacedItems(
            BattleSandboxBuildCombatPreview preview)
        {
            return preview?.context?.layoutSnapshot?.placedItems
                ?? (IReadOnlyList<BuildSandboxPlacedItemSnapshot>)Array.Empty<BuildSandboxPlacedItemSnapshot>();
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview combatPreview,
            FormationCorePowerRangePreview powerPreview)
        {
            if (combatPreview == null || powerPreview == null)
            {
                report.AddError("FORMATION_POWER_PREVIEW_NULL", "Formation power preview was not created.", PackageName);
                return;
            }

            if (!powerPreview.devOnly || powerPreview.isEnabled)
            {
                report.AddError(
                    "FORMATION_POWER_SCOPE_LEAK",
                    "Formation power preview must stay devOnly=true and isEnabled=false.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.ScopeLeakCount != 0)
            {
                report.AddError(
                    "FORMATION_POWER_LEAK_COUNT_NONZERO",
                    $"Formation power scope leak count must be 0. actual={powerPreview.ScopeLeakCount}.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "FORMATION_POWER_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if ((combatPreview.FormalFlowLeakCount
                    + combatPreview.PlayerSideAnswerLeakCount
                    + combatPreview.FeatureFlagDefaultTrueCount) != 0)
            {
                report.AddError(
                    "FORMATION_POWER_COMBAT_PREVIEW_LEAK",
                    "Formation power range must reuse leak-free BuildSandbox preview data only.",
                    nameof(BattleSandboxBuildCombatPreview));
            }
        }

        private static void ValidateCoreAndPowerRows(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview combatPreview,
            FormationCorePowerRangePreview powerPreview)
        {
            if (powerPreview.coreCell.x != 2 || powerPreview.coreCell.y != 2)
            {
                report.AddError(
                    "FORMATION_CORE_CELL_MISMATCH",
                    $"Default core cell must be 2,2. actual={powerPreview.coreCell}.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (!powerPreview.EyeCellExists)
            {
                report.AddError(
                    "FORMATION_EYE_CELL_MISSING",
                    "V04 sandbox eye cell must exist inside the board.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.EyeCellOccupiedCount != 0)
            {
                report.AddError(
                    "FORMATION_EYE_CELL_OCCUPIED",
                    $"Default preview must keep the fixed eye cell visible and empty. occupied={powerPreview.EyeCellOccupiedCount}.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.WeakPulseCellCount < 4)
            {
                report.AddError(
                    "FORMATION_WEAK_PULSE_RANGE_MISSING",
                    $"Eye WeakPulse range must be report-visible. weakPulseCells={powerPreview.WeakPulseCellCount}.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.ProviderCount < 1)
            {
                report.AddError(
                    "FORMATION_POWER_PROVIDER_MISSING",
                    "Default preview must include at least one power provider.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.PoweredItemCount < 1)
            {
                report.AddError(
                    "FORMATION_POWERED_ITEM_MISSING",
                    "Default preview must produce at least one powered item.",
                    nameof(FormationCorePowerRangePreview));
            }

            if (powerPreview.PowerRangeCellCount < 4)
            {
                report.AddError(
                    "FORMATION_POWER_RANGE_TOO_SMALL",
                    $"Power range cells are too low. actual={powerPreview.PowerRangeCellCount}.",
                    nameof(FormationCorePowerRangePreview));
            }

            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items = PlacedItems(combatPreview);
            BuildSandboxPlacedItemSnapshot provider = items.FirstOrDefault(item =>
                FormationCorePowerRangeResolver.IsPowerProvider(item));
            if (provider == null)
            {
                report.AddError(
                    "FORMATION_PROVIDER_SNAPSHOT_MISSING",
                    "Power provider snapshot missing after resolver applied.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }
            else if (!provider.isPowered || string.IsNullOrWhiteSpace(provider.energySourceId))
            {
                report.AddError(
                    "FORMATION_PROVIDER_NOT_POWERED",
                    $"Provider must power itself. itemId={provider.itemId}.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }

            int poweredNonProviders = items.Count(item => item != null
                && item.isPowered
                && !FormationCorePowerRangeResolver.IsPowerProvider(item));
            if (poweredNonProviders < 1)
            {
                report.AddError(
                    "FORMATION_RANGE_TARGET_MISSING",
                    "At least one non-provider must be powered by provider range in the default preview.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }

            foreach (FormationCorePowerRangeRow row in powerPreview.rows ?? new List<FormationCorePowerRangeRow>())
            {
                if (row == null)
                {
                    continue;
                }

                bool derivedPowered = row.energyState == EnergyState.Powered;
                if (row.isPowered != derivedPowered)
                {
                    report.AddError(
                        "FORMATION_OLD_ISPOWERED_MAIN_JUDGMENT",
                        $"isPowered must remain derived from EnergyState only. itemId={row.itemId}, state={row.energyState}, isPowered={row.isPowered}.",
                        nameof(FormationCorePowerRangeRow));
                }

                if (row.energyState == EnergyState.WeakPulse && row.isPowered)
                {
                    report.AddError(
                        "FORMATION_WEAKPULSE_MARKED_POWERED",
                        $"WeakPulse must not display or resolve as Powered. itemId={row.itemId}.",
                        nameof(FormationCorePowerRangeRow));
                }

                if (row.isPowerProvider && !row.isEnergyStoneSource)
                {
                    report.AddError(
                        "FORMATION_NONSTONE_PROVIDER",
                        $"Only spirit/energy stone family can be Powered providers. itemId={row.itemId}.",
                        nameof(FormationCorePowerRangeRow));
                }

                if (row.hasEnergyRoleViolation && row.isPowerProvider)
                {
                    report.AddError(
                        "FORMATION_FORBIDDEN_PROVIDER_MISIDENTIFIED",
                        $"Forbidden provider candidate was promoted to provider. itemId={row.itemId}.",
                        nameof(FormationCorePowerRangeRow));
                }

                if (row.isPowerProvider
                    && (row.energyDiagnostics ?? new List<string>())
                        .Contains(FormationEnergyDiagnosticCodes.TagProviderViolation))
                {
                    report.AddError(
                        "FORMATION_TAG_AUTO_POWER",
                        $"Provider-like tags must not auto-promote a provider. itemId={row.itemId}.",
                        nameof(FormationCorePowerRangeRow));
                }
            }

            ValidateSampleItem(report, powerPreview, "spirit_stone_basic", EnergyState.Powered, expectedProvider: true);
            ValidateSampleItem(report, powerPreview, "preview_energy_incense", EnergyState.WeakPulse, expectedProvider: false);
            ValidateSampleItem(report, powerPreview, "preview_stone_core", EnergyState.Powered, expectedProvider: false);
            ValidateSampleItem(report, powerPreview, "preview_taomu_sword", EnergyState.None, expectedProvider: false);

            report.AddInfo(
                "FORMATION_POWER_COUNTS",
                $"eyeCellExists={powerPreview.EyeCellExists}, eyeCellOccupied={powerPreview.EyeCellOccupiedCount}, weakPulseCells={powerPreview.WeakPulseCellCount}, providers={powerPreview.ProviderCount}, powered={powerPreview.PoweredItemCount}, rangeCells={powerPreview.PowerRangeCellCount}, forbiddenProviderMisidentified={powerPreview.ForbiddenProviderMisidentifiedCount}, tagAutoPowerProvider={powerPreview.TagAutoPowerProviderCount}.",
                nameof(FormationCorePowerRangePreview));
        }

        private static void ValidateSampleItem(
            BuildSandboxValidationReport report,
            FormationCorePowerRangePreview powerPreview,
            string itemId,
            EnergyState expectedState,
            bool expectedProvider)
        {
            FormationCorePowerRangeRow row = powerPreview?.rows?.FirstOrDefault(candidate =>
                candidate != null && string.Equals(candidate.itemId, itemId, StringComparison.Ordinal));
            if (row == null)
            {
                report.AddError(
                    "FORMATION_SAMPLE_ITEM_MISSING",
                    $"Default preview must keep sample item for report coverage. itemId={itemId}.",
                    nameof(FormationCorePowerRangeRow));
                return;
            }

            if (row.energyState != expectedState)
            {
                report.AddError(
                    "FORMATION_SAMPLE_ENERGY_STATE_MISMATCH",
                    $"Sample item energy state mismatch. itemId={itemId}, expected={expectedState}, actual={row.energyState}.",
                    nameof(FormationCorePowerRangeRow));
            }

            if (row.isPowerProvider != expectedProvider)
            {
                report.AddError(
                    "FORMATION_SAMPLE_PROVIDER_MISMATCH",
                    $"Sample item provider flag mismatch. itemId={itemId}, expected={expectedProvider}, actual={row.isPowerProvider}.",
                    nameof(FormationCorePowerRangeRow));
            }
        }

        private static void ValidateFeedbackConnection(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview combatPreview)
        {
            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows =
                combatPreview?.rows ?? new List<BattleSandboxBuildCombatPreviewRow>();
            int formationRows = rows.Count(row => row != null
                && !string.IsNullOrWhiteSpace(row.sourceDataPath)
                && row.sourceDataPath.IndexOf("formationCorePowerRange", StringComparison.OrdinalIgnoreCase) >= 0);

            if (formationRows < 1)
            {
                report.AddError(
                    "FORMATION_POWER_FEEDBACK_ROW_MISSING",
                    "Build combat preview must expose masked player feedback for formation core / power range.",
                    nameof(BattleSandboxBuildCombatPreviewRow));
            }

            if (rows.Any(row => row != null && row.playerSideAnswerLeak))
            {
                report.AddError(
                    "FORMATION_POWER_PLAYER_TEXT_LEAK",
                    "Formation power player feedback must not leak answer tokens or English stable keys.",
                    nameof(BattleSandboxBuildCombatPreviewRow));
            }
        }
    }
}
#endif
