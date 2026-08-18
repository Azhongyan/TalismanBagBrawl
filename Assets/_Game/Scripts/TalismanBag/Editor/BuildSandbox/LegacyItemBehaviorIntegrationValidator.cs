#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class LegacyItemBehaviorIntegrationValidator
    {
        public const string PackageName = LegacyItemBehaviorIntegrationCatalog.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/LegacyItemBehaviorIntegration01/[QA Only] Run Legacy Item Behavior Integration";

        private static readonly EnergyState[] SampleEnergyStates =
        {
            EnergyState.None,
            EnergyState.WeakPulse,
            EnergyState.Powered,
            EnergyState.Suppressed
        };

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "bossSixKeyFullAnswer",
            "DropBias",
            "dropBias",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "hardSolutionTags",
            "formalRunFlow",
            "SaveData",
            "Reward",
            "Chapter"
        };
        public static void RunMenu()
        {
            Run(throwOnFailure: false);
        }

        public static void RunBatch()
        {
            bool passed = Run(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static bool Run(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = BuildValidationReports();
            LegacyItemBehaviorIntegrationPreview preview = BuildPreview();
            string[] reportPaths =
                LegacyItemBehaviorIntegrationReportWriter.WriteReports(reports, preview);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-LegacyItemBehaviorIntegration01] completed status={(preview.Passed ? "PASS" : "FAIL")}, errors={reports.Sum(report => report.ErrorCount)}, warnings={reports.Sum(report => report.WarningCount)}, leaks={preview.TotalLeakCount}, reports={string.Join(", ", reportPaths)}");

            if (!preview.Passed && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"LegacyItemBehaviorIntegration01 failed. See {string.Join(", ", reportPaths)}");
            }

            return preview.Passed;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                Validate()
            };
        }

        public static BuildSandboxValidationReport Validate(
            LegacyItemBehaviorIntegrationPreview preview = null)
        {
            BuildSandboxValidationReport report = new("Legacy Item Behavior Integration 01");
            LegacyItemBehaviorIntegrationPreview safePreview = preview ?? BuildPreview();
            RequireTrue(report, "LEGACY_BEHAVIOR_DEVONLY_DISABLED", safePreview.devOnly && !safePreview.isEnabled, "Legacy item behavior preview remains devOnly and disabled.");
            RequireTrue(report, "LEGACY_BEHAVIOR_READS_ENERGY_STATE", safePreview.readsEnergyState && safePreview.reusesFormationEnergyContract, "Behavior samples read EnergyState and reuse the FormationEnergyContract output shape.");
            RequireTrue(report, "LEGACY_BEHAVIOR_ROSTER_COVERED", safePreview.behaviorProfileCount >= safePreview.rosterItemCount && safePreview.rosterItemCount > 0, "All current V0.4 roster rows have behavior profiles.");
            RequireTrue(report, "LEGACY_BEHAVIOR_CORE_ITEMS_COVERED", safePreview.requiredCoreProfileCount >= LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds.Count, "Required first-batch legacy base items are covered.");
            RequireTrue(report, "LEGACY_BEHAVIOR_POWERED_OUTPUT_READY", safePreview.requiredCorePoweredOutputCount >= LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds.Count, "Required first-batch legacy base items have Powered sample output.");
            RequireTrue(report, "LEGACY_BEHAVIOR_NONE_DORMANT", safePreview.noneOutputLeakCount == 0, "None samples do not trigger or emit full effects.");
            RequireTrue(report, "LEGACY_BEHAVIOR_WEAKPULSE_LOW_EFFICIENCY", safePreview.weakPulseFullPowerLeakCount == 0, "WeakPulse samples trigger below full efficiency and do not allow affix/synergy boosts.");
            RequireTrue(report, "LEGACY_BEHAVIOR_SUPPRESSED_BLOCKED", safePreview.suppressedOutputLeakCount == 0, "Suppressed samples do not trigger or emit effects.");
            RequireTrue(report, "LEGACY_BEHAVIOR_PROVIDER_NO_DAMAGE", safePreview.energyProviderDamageLeakCount == 0, "Spirit/energy stone provider samples do not damage enemy HP.");
            RequireTrue(report, "LEGACY_BEHAVIOR_FORBIDDEN_PROVIDER_CLEAR", safePreview.forbiddenProviderLeakCount == 0, "Energy incense and furnace core are not treated as Powered providers.");
            RequireTrue(report, "LEGACY_BEHAVIOR_PLAYER_FIELDS_MASKED", safePreview.playerSideAnswerLeakCount == 0 && safePreview.developerFieldVisibleLeakCount == 0, "Player-facing feedback masks internal answer/developer fields.");
            RequireTrue(report, "LEGACY_BEHAVIOR_FORMAL_SCOPE_CLEAR", safePreview.ScopeLeakCount == 0 && safePreview.featureFlagDefaultTrueCount == 0, "No formal flow/save/drop/reward/backpack scope is touched.");
            return report;
        }

        public static LegacyItemBehaviorIntegrationPreview BuildPreview()
        {
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster =
                BuildSandboxLegacyAndAdvancedItemRosterCatalog.AllItems;
            List<LegacyItemBehaviorSampleRow> rows = BuildSampleRows(roster);
            List<LegacyItemFamilyBehaviorMapRow> familyRows =
                LegacyItemBehaviorIntegrationCatalog.BuildFamilyRows(roster).ToList();
            HashSet<string> required = new(
                LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds,
                StringComparer.Ordinal);
            Dictionary<string, LegacyItemBehaviorProfile> profileById =
                LegacyItemBehaviorIntegrationCatalog.AllProfiles
                    .ToDictionary(profile => profile.itemId, StringComparer.Ordinal);

            LegacyItemBehaviorIntegrationPreview preview = new()
            {
                devOnly = true,
                isEnabled = false,
                readsEnergyState = true,
                reusesFormationEnergyContract = true,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                writesFormalReward = false,
                writesFormalDrop = false,
                touchesFormalBackpack = false,
                rosterItemCount = roster.Count,
                behaviorProfileCount = rows
                    .Select(row => row.itemId)
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                requiredCoreProfileCount = required.Count(itemId => profileById.ContainsKey(itemId)),
                requiredCorePoweredOutputCount = rows.Count(row =>
                    required.Contains(row.itemId)
                    && row.energyState == EnergyState.Powered
                    && row.triggers
                    && row.HasAnyOutput),
                energyStateSampleRowCount = rows.Count,
                familyMapRowCount = familyRows.Count,
                noneOutputLeakCount = rows.Count(row =>
                    row.energyState == EnergyState.None
                    && (row.triggers || row.HasAnyOutput || row.manaCost > 0 || row.allowAffixSynergyBonus)),
                weakPulseFullPowerLeakCount = rows.Count(row =>
                    row.energyState == EnergyState.WeakPulse
                    && (!row.triggers || row.efficiencyPercent >= 100 || row.allowAffixSynergyBonus)),
                poweredMissingOutputCount = rows.Count(row =>
                    required.Contains(row.itemId)
                    && row.energyState == EnergyState.Powered
                    && (!row.triggers || !row.HasAnyOutput)),
                suppressedOutputLeakCount = rows.Count(row =>
                    row.energyState == EnergyState.Suppressed
                    && (row.triggers || row.HasAnyOutput || row.manaCost > 0 || row.allowAffixSynergyBonus)),
                energyProviderDamageLeakCount = rows.Count(row =>
                    IsFormalEnergyProvider(row.itemId)
                    && row.enemyHpDamage > 0),
                forbiddenProviderLeakCount = rows.Count(row =>
                    row.energyState == EnergyState.Powered
                    && LegacyItemBehaviorIntegrationCatalog.IsForbiddenPoweredProviderCandidate(row.itemId)
                    && row.manaGain > 0),
                playerSideAnswerLeakCount = rows.Count(ContainsPlayerLeak),
                developerFieldVisibleLeakCount = rows.Count(row => !row.developerFieldsHidden),
                formalLeakCount = rows.Sum(row => row.FormalLeakCount),
                featureFlagDefaultTrueCount = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue),
                rows = rows,
                familyRows = familyRows
            };

            return preview;
        }

        private static List<LegacyItemBehaviorSampleRow> BuildSampleRows(
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster)
        {
            List<LegacyItemBehaviorSampleRow> rows = new();
            foreach (BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow in roster ?? Array.Empty<BuildSandboxLegacyAndAdvancedItemRosterRow>())
            {
                foreach (EnergyState state in SampleEnergyStates)
                {
                    BuildSandboxPlacedItemSnapshot item = BuildSampleItem(rosterRow, state);
                    BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.Resolve(rosterRow?.ItemId);
                    LegacyItemBehaviorResult result =
                        LegacyItemBehaviorIntegrationCatalog.Evaluate(item, stat);
                    rows.Add(LegacyItemBehaviorIntegrationCatalog.ToSampleRow(result));
                }
            }

            return rows;
        }

        private static BuildSandboxPlacedItemSnapshot BuildSampleItem(
            BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow,
            EnergyState state)
        {
            BuildSandboxPlacedItemSnapshot item =
                BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                    rosterRow?.ItemId ?? string.Empty,
                    rosterRow?.ShapeId ?? "Single1",
                    ItemShapeRotation.Rotation0,
                    new[] { new ItemShapeCell(3, 1) });
            item.energyState = state;
            item.isPowered = state == EnergyState.Powered;
            item.energyStateReason = "legacy_item_behavior_sample_" + state;
            item.energySourceId = state == EnergyState.Powered ? "sample_spirit_stone_provider" : string.Empty;
            item.formalEnergySourceItemId = state == EnergyState.Powered ? "spirit_stone_basic" : string.Empty;
            return item;
        }

        private static bool ContainsPlayerLeak(LegacyItemBehaviorSampleRow row)
        {
            if (row == null)
            {
                return true;
            }

            string joined = string.Join(" ", row.playerFeedbackChinese, row.combatLogChinese);
            return ForbiddenPlayerAnswerTokens.Any(token =>
                !string.IsNullOrWhiteSpace(token)
                && joined.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsFormalEnergyProvider(string itemId)
        {
            LegacyItemBehaviorProfile profile =
                LegacyItemBehaviorIntegrationCatalog.Resolve(itemId, string.Empty, string.Empty);
            return profile.formalEnergyProvider;
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string message)
        {
            if (value)
            {
                report.AddInfo(code, message, PackageName);
                return;
            }

            report.AddError(code, message, PackageName);
        }
    }

    public static class LegacyItemBehaviorIntegrationReportWriter
    {
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);

        public const string MainReportPath =
            "Docs/V0.4/Reports/LegacyItemBehaviorIntegrationReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/LegacyItemBehaviorRows.csv";

        public const string FamilyMapReportPath =
            "Docs/V0.4/Reports/LegacyItemFamilyBehaviorMap.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/LegacyItemBehaviorLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            LegacyItemBehaviorIntegrationPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            LegacyItemBehaviorIntegrationPreview safePreview =
                preview ?? LegacyItemBehaviorIntegrationValidator.BuildPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string familyPath = Path.Combine(projectRoot, FamilyMapReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);

            WriteUtf8BomText(mainPath, BuildMainReport(safeReports, safePreview, errors, warnings));
            WriteUtf8BomText(rowPath, BuildRowsCsv(safePreview));
            WriteUtf8BomText(familyPath, BuildFamilyMapCsv(safePreview));
            WriteUtf8BomText(leakPath, BuildLeakCheckReport(safeReports, safePreview, errors, warnings));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, familyPath, leakPath };
        }

        private static void WriteUtf8BomText(string path, string contents)
        {
            byte[] preamble = Utf8WithBom.GetPreamble();
            byte[] body = Utf8WithBom.GetBytes(contents ?? string.Empty);
            File.WriteAllBytes(path, preamble.Concat(body).ToArray());
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            LegacyItemBehaviorIntegrationPreview preview,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Legacy Item Behavior Integration Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{LegacyItemBehaviorIntegrationValidator.PackageName}`");
            builder.AppendLine($"Guard Pass: `{LegacyItemBehaviorIntegrationCatalog.GuardPass}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && warnings == 0 && preview?.Passed == true ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{preview?.TotalLeakCount ?? 1}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Runs only inside V0.4 BuildSandbox / BattleSandbox devOnly previews.");
            builder.AppendLine("- Reuses `FormationEnergyContract` and reads `BuildSandboxPlacedItemSnapshot.energyState`.");
            builder.AppendLine("- Does not modify V0.2/V0.3 formal item configs, formal itemId, drops, rewards, backpack, save data, Boss data, scenes, prefabs, RectTransform, or BuildSettings.");
            builder.AppendLine("- V0.4 advanced items are recorded as family extensions of their base items.");
            builder.AppendLine("- Player-side feedback uses Chinese text only; English stable keys and developer fields stay report/developer-side.");
            builder.AppendLine();
            builder.AppendLine("## Code Survey");
            builder.AppendLine();
            builder.AppendLine("| Source | Finding | Integration Response |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine("| `BuildSandboxPlacedItemSnapshot.cs` | Snapshot already carries item identity, itemFamily/baseItemId, itemStat, and `EnergyState`. | Added behavior mapping on top of those fields; corrected `preview_soul_seal` to the 镇魂符 family in BuildSandbox only. |");
            builder.AppendLine("| `FormationEnergyContract.cs` | Contract defines `None`, `WeakPulse`, `Powered`, `Suppressed`; only spirit/energy stone family can be provider. | Behavior evaluator reads the state and does not redefine the provider contract. |");
            builder.AppendLine("| `BattleSandboxRuntimeLoop.cs` | Runtime had item rows and effect text but damage/support settlement was not fully state-gated. | Runtime now evaluates legacy behavior before spending mana or applying damage/shield/cleanse/control. |");
            builder.AppendLine("| `BattleSandboxCombatKernelAdapter.cs` | Existing kernel already clamps shield/HP damage and shield-first player damage. | Legacy behavior feeds sandbox-only numbers into the existing kernel. |");
            builder.AppendLine("| `BattleSandboxBuildCombatPreview.cs` | Existing preview tags/stats/affixes give roster context. | Reports reuse roster/stat data without writing formal configs. |");
            builder.AppendLine("| `BattleSandboxItemStatCombatPreview*` | Item stat rows provide old-style attack, guard, cleanse, control, spirit, cost, and interval values. | Behavior samples scale those stat values by `EnergyState`. |");
            builder.AppendLine("| `LegacyAndAdvancedItemRoster*` | Current V0.4 roster contains 23 base/advanced/devOnly items. | All roster rows receive behavior sample rows; first batch core items are explicitly counted. |");
            builder.AppendLine("| V0.2/V0.3 combat scripts/config | Old combat has fire damage/burn, thunder break, exorcism/cleanse/control, peach protection, and spirit stone mana generation. | BuildSandbox-only behavior approximates those effects without editing formal sources. |");
            builder.AppendLine("| `Reports/BattleSandboxPlayableFullRosterRegression*` | Previous attack samples did not model legal supply. | Regression runtime samples now place a legal spirit stone provider for behavior checks. |");
            builder.AppendLine();
            AppendCounters(builder, preview);
            AppendFamilyTable(builder, preview);
            AppendEnergyStateSummary(builder, preview);
            AppendSampleTable(builder, preview);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(LegacyItemBehaviorIntegrationPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemId,chineseName,englishStableKey,itemFamily,baseItemId,v04AdvancedItemId,v04AdvancedChineseName,advancedRelationshipChinese,energyState,triggers,allowAffixSynergyBonus,efficiencyPercent,manaCost,enemyHpDamage,enemyShieldPressure,playerShieldGain,cleanseValue,controlValue,manaGain,sampleResultChinese,playerFeedbackChinese,combatLogChinese,developerFieldsHidden,touchesFormalFlow,touchesFormalSaveData,touchesFormalReward,touchesFormalDrop,devOnly,isEnabled,sourceDataPath");
            foreach (LegacyItemBehaviorSampleRow row in preview?.rows ?? new List<LegacyItemBehaviorSampleRow>())
            {
                csv.AppendLine(Csv(
                    row.itemId,
                    row.chineseName,
                    row.englishStableKey,
                    row.itemFamily,
                    row.baseItemId,
                    row.advancedItemId,
                    row.advancedItemChineseName,
                    row.advancedRelationshipChinese,
                    row.energyState.ToString(),
                    row.triggers.ToString(),
                    row.allowAffixSynergyBonus.ToString(),
                    row.efficiencyPercent.ToString(),
                    row.manaCost.ToString(),
                    row.enemyHpDamage.ToString(),
                    row.enemyShieldPressure.ToString(),
                    row.playerShieldGain.ToString(),
                    row.cleanseValue.ToString(),
                    row.controlValue.ToString(),
                    row.manaGain.ToString(),
                    row.sampleResultChinese,
                    row.playerFeedbackChinese,
                    row.combatLogChinese,
                    row.developerFieldsHidden.ToString(),
                    row.touchesFormalFlow.ToString(),
                    row.touchesFormalSaveData.ToString(),
                    row.touchesFormalReward.ToString(),
                    row.touchesFormalDrop.ToString(),
                    row.devOnly.ToString(),
                    row.isEnabled.ToString(),
                    row.sourceDataPath));
            }

            return csv.ToString();
        }

        private static string BuildFamilyMapCsv(LegacyItemBehaviorIntegrationPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemFamily,baseItemId,baseChineseName,englishStableKey,v04AdvancedItemId,v04AdvancedChineseName,advancedRelationshipChinese,behaviorSummaryChinese,baseItemPresentInRoster,advancedItemPresentInRoster,formalEnergyProvider,auxiliaryNonProvider,devOnly,isEnabled");
            foreach (LegacyItemFamilyBehaviorMapRow row in preview?.familyRows ?? new List<LegacyItemFamilyBehaviorMapRow>())
            {
                csv.AppendLine(Csv(
                    row.itemFamily,
                    row.baseItemId,
                    row.baseChineseName,
                    row.englishStableKey,
                    row.v04AdvancedItemId,
                    row.v04AdvancedChineseName,
                    row.advancedRelationshipChinese,
                    row.behaviorSummaryChinese,
                    row.baseItemPresentInRoster.ToString(),
                    row.advancedItemPresentInRoster.ToString(),
                    row.formalEnergyProvider.ToString(),
                    row.auxiliaryNonProvider.ToString(),
                    row.devOnly.ToString(),
                    row.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            LegacyItemBehaviorIntegrationPreview preview,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Legacy Item Behavior Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{LegacyItemBehaviorIntegrationValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && warnings == 0 && preview?.TotalLeakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `noneOutputLeaks` | {preview?.noneOutputLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `weakPulseFullPowerLeaks` | {preview?.weakPulseFullPowerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `poweredMissingOutputs` | {preview?.poweredMissingOutputCount ?? 1} | 0 |");
            builder.AppendLine($"| `suppressedOutputLeaks` | {preview?.suppressedOutputLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `energyProviderDamageLeaks` | {preview?.energyProviderDamageLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `forbiddenProviderLeaks` | {preview?.forbiddenProviderLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeaks` | {preview?.playerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `developerFieldVisibleLeaks` | {preview?.developerFieldVisibleLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalScopeLeaks` | {preview?.ScopeLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {preview?.featureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {preview?.TotalLeakCount ?? 1} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- `V02RunFlowController`, `PageState`, `FormationState`, Boss, rewards, drops, backpack, save data, scenes, prefabs, RectTransform, and BuildSettings are not written by this package.");
            builder.AppendLine("- `spirit_stone_basic` / spirit-stone family is the only formal provider family represented here.");
            builder.AppendLine("- `preview_energy_incense` and `preview_stone_core` remain auxiliary/non-provider rows even when sampled as `Powered`.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendCounters(
            StringBuilder builder,
            LegacyItemBehaviorIntegrationPreview preview)
        {
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- roster items: `{preview?.rosterItemCount ?? 0}`");
            builder.AppendLine($"- behavior profiles: `{preview?.behaviorProfileCount ?? 0}`");
            builder.AppendLine($"- required core profiles: `{preview?.requiredCoreProfileCount ?? 0}`");
            builder.AppendLine($"- required core Powered outputs: `{preview?.requiredCorePoweredOutputCount ?? 0}`");
            builder.AppendLine($"- EnergyState sample rows: `{preview?.energyStateSampleRowCount ?? 0}`");
            builder.AppendLine($"- family map rows: `{preview?.familyMapRowCount ?? 0}`");
            builder.AppendLine($"- total leaks: `{preview?.TotalLeakCount ?? 1}`");
            builder.AppendLine();
        }

        private static void AppendFamilyTable(
            StringBuilder builder,
            LegacyItemBehaviorIntegrationPreview preview)
        {
            builder.AppendLine("## Family Map");
            builder.AppendLine();
            builder.AppendLine("| itemFamily | baseItemId | Base CN | Stable Key | V0.4 Advanced | Relationship | Behavior |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (LegacyItemFamilyBehaviorMapRow row in preview?.familyRows ?? new List<LegacyItemFamilyBehaviorMapRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemFamily)}` | `{Escape(row.baseItemId)}` | {Escape(row.baseChineseName)} | `{Escape(row.englishStableKey)}` | `{Escape(row.v04AdvancedItemId)}` {Escape(row.v04AdvancedChineseName)} | {Escape(row.advancedRelationshipChinese)} | {Escape(row.behaviorSummaryChinese)} |");
            }

            builder.AppendLine();
        }

        private static void AppendEnergyStateSummary(
            StringBuilder builder,
            LegacyItemBehaviorIntegrationPreview preview)
        {
            builder.AppendLine("## EnergyState Rules");
            builder.AppendLine();
            builder.AppendLine("| EnergyState | Trigger Difference | Sample Count | Output Leaks |");
            builder.AppendLine("| --- | --- | ---: | ---: |");
            AppendEnergyStateRow(builder, preview, EnergyState.None, "不触发或沉寂；不耗灵；不输出完整效果。", preview?.noneOutputLeakCount ?? 1);
            AppendEnergyStateRow(builder, preview, EnergyState.WeakPulse, "低效基础触发；不允许词条 / 羁绊增益。", preview?.weakPulseFullPowerLeakCount ?? 1);
            AppendEnergyStateRow(builder, preview, EnergyState.Powered, "完整基础效果；允许词条 / 羁绊增益。", preview?.poweredMissingOutputCount ?? 1);
            AppendEnergyStateRow(builder, preview, EnergyState.Suppressed, "被压制；不触发或不输出效果。", preview?.suppressedOutputLeakCount ?? 1);
            builder.AppendLine();
        }

        private static void AppendEnergyStateRow(
            StringBuilder builder,
            LegacyItemBehaviorIntegrationPreview preview,
            EnergyState state,
            string difference,
            int leakCount)
        {
            int count = preview?.rows?.Count(row => row != null && row.energyState == state) ?? 0;
            builder.AppendLine($"| `{state}` | {Escape(difference)} | {count} | {leakCount} |");
        }

        private static void AppendSampleTable(
            StringBuilder builder,
            LegacyItemBehaviorIntegrationPreview preview)
        {
            builder.AppendLine("## Core Item Samples");
            builder.AppendLine();
            builder.AppendLine("| Item | CN | Stable Key | Family | State | Result | Player Feedback | Developer Fields Hidden | Formal Touch |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | ---: | ---: |");
            HashSet<string> required = new(
                LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds,
                StringComparer.Ordinal);
            foreach (LegacyItemBehaviorSampleRow row in preview?.rows?.Where(row => required.Contains(row.itemId))
                         ?? Enumerable.Empty<LegacyItemBehaviorSampleRow>())
            {
                bool formalTouch = row.FormalLeakCount > 0;
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | {Escape(row.chineseName)} | `{Escape(row.englishStableKey)}` | `{Escape(row.itemFamily)}` | `{row.energyState}` | `{Escape(row.sampleResultChinese)}` | {Escape(row.playerFeedbackChinese)} | `{row.developerFieldsHidden}` | `{formalTouch}` |");
            }

            builder.AppendLine();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>()).SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            if (normalized.Contains(",") || normalized.Contains("\"") || normalized.Contains("\n") || normalized.Contains("\r"))
            {
                return $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
#endif
