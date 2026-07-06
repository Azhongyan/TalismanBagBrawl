#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxItemEffectRuntimePreviewValidator
    {
        public const string PackageName = BattleSandboxItemEffectRuntimePreviewCatalog.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxItemEffectRuntimePreview01/[QA Only] Run Item Effect Runtime Preview";

        private static readonly string[] RequiredEffectFamilyKeys =
        {
            "fire",
            "thunder",
            "exorcism",
            "cleanse",
            "energy",
            "guard",
            "control",
            "peach"
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
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot = BuildSnapshot(reports);
            string[] reportPaths =
                BattleSandboxItemEffectRuntimePreviewReportWriter.WriteReports(reports, snapshot);

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
                $"[BuildSandbox-BattleSandboxItemEffectRuntimePreview01] completed status={(snapshot.Passed ? "PASS" : "FAIL")}, errors={reports.Sum(report => report.ErrorCount)}, warnings={reports.Sum(report => report.WarningCount)}, reports={string.Join(", ", reportPaths)}");

            if (!snapshot.Passed && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BattleSandboxItemEffectRuntimePreview01 failed. See {string.Join(", ", reportPaths)}");
            }

            return snapshot.Passed;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxUiLayoutGuard.Validate(),
                BattleSandboxRuntimeLoopValidator.Validate(),
                Validate()
            };
            return reports;
        }

        public static BuildSandboxValidationReport Validate(
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot = null)
        {
            BuildSandboxValidationReport report = new("BattleSandbox Item Effect Runtime Preview 01");
            BattleSandboxItemEffectRuntimePreviewSnapshot safeSnapshot =
                snapshot ?? BuildSnapshot(Array.Empty<BuildSandboxValidationReport>());

            ValidateIsolation(report, safeSnapshot);
            ValidateProfiles(report, safeSnapshot);
            ValidateRuntimeSamples(report, safeSnapshot);
            return report;
        }

        public static BattleSandboxItemEffectRuntimePreviewSnapshot BuildSnapshot(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster =
                BuildSandboxLegacyAndAdvancedItemRosterCatalog.AllItems;
            List<BattleSandboxItemEffectRuntimeProfileRow> profileRows =
                BuildProfileRows(roster);
            List<BattleSandboxItemEffectRuntimeSampleRow> sampleRows =
                BuildSampleRows(roster);

            int playerLeakCount = sampleRows.Sum(row => row.playerSideAnswerLeakCount)
                + profileRows.Count(row => ContainsForbiddenPlayerToken(row.displayNameChinese)
                    || ContainsForbiddenPlayerToken(row.effectFamilyChinese)
                    || ContainsForbiddenPlayerToken(row.effectRoleChinese));
            int formalLeakCount =
                safeReports.Sum(report => report.Issues.Count(issue =>
                    issue.Code.IndexOf("FORMAL", StringComparison.OrdinalIgnoreCase) >= 0
                    && issue.Level == BuildSandboxValidationLevel.Error));

            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot = new()
            {
                devOnly = true,
                isEnabled = false,
                reusesRuntimeLoopFeedback = true,
                reusesExistingHudText = true,
                createsNewUiFrame = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                touchesFormalSceneUiLayout = false,
                rosterItemCount = roster.Count,
                profileCount = BattleSandboxItemEffectRuntimePreviewCatalog.AllProfiles.Count,
                mappedRosterItemCount = profileRows.Count(row => !string.IsNullOrWhiteSpace(row.itemEffectKey)),
                runtimeRowCount = sampleRows.Sum(row => row.runtimeRows),
                itemEffectRuntimeRowCount = sampleRows.Sum(row => row.effectRows > 0 ? 1 : 0),
                distinctEffectFamilyCount = profileRows
                    .Select(row => row.effectFamilyKey)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .Count(),
                attackProfileCount = profileRows.Count(row => row.dealsEnemyDamage),
                supportProfileCount = profileRows.Count(row => row.supportOnly && !row.dealsEnemyDamage),
                energyProfileCount = profileRows.Count(row => row.restoresMana),
                guardProfileCount = profileRows.Count(row => row.grantsShield),
                cleanseProfileCount = profileRows.Count(row => row.cleanses),
                controlProfileCount = profileRows.Count(row => row.controlsBoss),
                peachProfileCount = profileRows.Count(row => string.Equals(row.effectFamilyKey, "peach", StringComparison.Ordinal)),
                supportDamageLeakCount = sampleRows.Count(row =>
                    row.expectedSupportNoDamage && row.enemyHpDamageTotal > 0),
                playerSideAnswerLeakCount = playerLeakCount,
                formalLeakCount = formalLeakCount,
                uiLayoutWriteCount = 0,
                profileRows = profileRows,
                sampleRows = sampleRows
            };

            return snapshot;
        }

        private static List<BattleSandboxItemEffectRuntimeProfileRow> BuildProfileRows(
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster)
        {
            List<BattleSandboxItemEffectRuntimeProfileRow> rows = new();
            foreach (BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow in roster ?? Array.Empty<BuildSandboxLegacyAndAdvancedItemRosterRow>())
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.Resolve(rosterRow?.ItemId);
                BattleSandboxItemEffectRuntimeProfile profile =
                    BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(
                        rosterRow?.ItemId,
                        rosterRow?.Identity?.ItemFamily,
                        stat.statTag);
                rows.Add(new BattleSandboxItemEffectRuntimeProfileRow
                {
                    itemId = rosterRow?.ItemId ?? string.Empty,
                    displayNameChinese = profile.displayNameChinese,
                    itemEffectKey = profile.itemEffectKey,
                    effectFamilyKey = profile.effectFamilyKey,
                    effectFamilyChinese = profile.effectFamilyChinese,
                    effectRoleChinese = profile.effectRoleChinese,
                    statProfileId = stat.statProfileId,
                    statTag = stat.statTag,
                    dealsEnemyDamage = profile.dealsEnemyDamage,
                    supportOnly = profile.supportOnly,
                    restoresMana = profile.restoresMana,
                    grantsShield = profile.grantsShield,
                    cleanses = profile.cleanses,
                    controlsBoss = profile.controlsBoss,
                    breaksShield = profile.breaksShield,
                    devOnly = profile.devOnly,
                    isEnabled = profile.isEnabled,
                    sourceDataPath = profile.SourceDataPath
                });
            }

            return rows;
        }

        private static List<BattleSandboxItemEffectRuntimeSampleRow> BuildSampleRows(
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster)
        {
            List<BattleSandboxItemEffectRuntimeSampleRow> rows = new();
            foreach (BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow in roster ?? Array.Empty<BuildSandboxLegacyAndAdvancedItemRosterRow>())
            {
                string itemId = rosterRow?.ItemId ?? string.Empty;
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.Resolve(itemId);
                BattleSandboxItemEffectRuntimeProfile profile =
                    BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(itemId, rosterRow?.Identity?.ItemFamily, stat.statTag);
                BattleSandboxRuntimeLoopPreview preview =
                    BattleSandboxRuntimeLoopPreviewBuilder.Build(
                        BuildSampleSnapshot(itemId),
                        $"item_effect_runtime_{itemId}",
                        BuildSampleScenario(itemId));
                IReadOnlyList<BattleSandboxRuntimeLoopRow> itemRows =
                    (preview?.rows ?? new List<BattleSandboxRuntimeLoopRow>())
                    .Where(row => row != null && string.Equals(row.itemId, itemId, StringComparison.Ordinal))
                    .ToArray();
                int enemyDamage = itemRows.Sum(row => Mathf.Max(0, row.enemyHpBefore - row.enemyHpAfter));
                bool expectedAttack =
                    BattleSandboxPlayableFullRosterRegression.ExpectedAttackDamageItemIds.Contains(itemId);
                bool expectedSupport =
                    BattleSandboxPlayableFullRosterRegression.ExpectedNoDamageSupportItemIds.Contains(itemId);
                BattleSandboxRuntimeLoopRow sampleTextRow = itemRows.FirstOrDefault(row =>
                    !string.IsNullOrWhiteSpace(row.combatLogLineChinese)
                    || !string.IsNullOrWhiteSpace(row.floatingTextChinese));

                rows.Add(new BattleSandboxItemEffectRuntimeSampleRow
                {
                    itemId = itemId,
                    itemEffectKey = profile.itemEffectKey,
                    effectFamilyChinese = profile.effectFamilyChinese,
                    effectRoleChinese = profile.effectRoleChinese,
                    runtimeRows = preview?.rows?.Count ?? 0,
                    effectRows = itemRows.Count(row => !string.IsNullOrWhiteSpace(row.itemEffectKey)),
                    enemyHpDamageTotal = enemyDamage,
                    expectedAttackDamage = expectedAttack,
                    expectedSupportNoDamage = expectedSupport,
                    playerSideAnswerLeakCount = CountPlayerLeaks(itemRows),
                    sampleFloatingChinese = sampleTextRow?.floatingTextChinese ?? string.Empty,
                    sampleLogChinese = sampleTextRow?.combatLogLineChinese ?? string.Empty,
                    passed = itemRows.Count > 0
                        && !string.IsNullOrWhiteSpace(profile.itemEffectKey)
                        && (!expectedAttack || enemyDamage > 0)
                        && (!expectedSupport || enemyDamage == 0)
                        && CountPlayerLeaks(itemRows) == 0
                });
            }

            return rows;
        }

        private static BuildSandboxLayoutSnapshot BuildSampleSnapshot(string itemId)
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            bool isSpiritStone = string.Equals(itemId, "spirit_stone_basic", StringComparison.Ordinal);
            snapshot.placedItems.Add(BuildItem(itemId, 3, isSpiritStone ? 2 : 1));
            if (!isSpiritStone)
            {
                snapshot.placedItems.Add(BuildItem("spirit_stone_basic", 3, 2));
            }
            else
            {
                snapshot.placedItems.Add(BuildItem("preview_energy_incense", 1, 2));
            }

            FormationEnergyContractResolver.Apply(snapshot);
            return snapshot;
        }

        private static BuildSandboxPlacedItemSnapshot BuildItem(
            string itemId,
            int x,
            int y)
        {
            BuildSandboxLegacyAndAdvancedItemRosterRow rosterRow =
                BuildSandboxLegacyAndAdvancedItemRosterCatalog.Resolve(itemId);
            BuildSandboxPlacedItemSnapshot item = new()
            {
                itemId = itemId ?? string.Empty,
                shapeId = rosterRow.ShapeId,
                anchorCell = new ItemShapeCell(x, y),
                occupiedCells = new List<ItemShapeCell> { new(x, y) },
                tags = new List<string> { rosterRow.PrimaryCategoryId, rosterRow.Identity.ItemFamily },
                itemStat = BuildSandboxItemStatCatalog.Resolve(itemId)
            };
            BuildSandboxItemIdentityFamilyCatalog.ApplyTo(item);
            return item;
        }

        private static BattleSandboxRuntimeLoopScenario BuildSampleScenario(string itemId)
        {
            return new BattleSandboxRuntimeLoopScenario
            {
                stageId = "item_effect_runtime_preview",
                devChapterLabel = "V0.4",
                previewBuildId = $"item_effect_runtime_{itemId}",
                enemyDisplayNameChinese = "沙盒首领",
                simulatedWinRate = 0.5f,
                expectsSandboxVictory = false,
                devOnlyProfileId = "item_effect_runtime_preview",
                attackSourcePath = "EnemyBossValidationPool.itemEffectRuntimePreview.attackDamage",
                attackDamage = 36,
                attackIntervalSeconds = 2.4f,
                attackFromDevOnlyProfile = true,
                playerMechanicFeedbackChinese = "首领正在观察当前道具触发节奏。",
                playerBuildPressureChinese = "重点观察道具发动、供能、护阵与净化反馈是否清楚。"
            };
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            if (snapshot == null)
            {
                report.AddError("ITEM_EFFECT_PREVIEW_NULL", "Item effect runtime preview snapshot is null.", PackageName);
                return;
            }

            RequireTrue(report, "ITEM_EFFECT_DEVONLY_TRUE", snapshot.devOnly);
            RequireFalse(report, "ITEM_EFFECT_ENABLED_FALSE", snapshot.isEnabled);
            RequireTrue(report, "ITEM_EFFECT_REUSES_RUNTIME_FEEDBACK", snapshot.reusesRuntimeLoopFeedback);
            RequireTrue(report, "ITEM_EFFECT_REUSES_EXISTING_HUD", snapshot.reusesExistingHudText);
            RequireFalse(report, "ITEM_EFFECT_CREATES_UI_FALSE", snapshot.createsNewUiFrame);
            RequireFalse(report, "ITEM_EFFECT_FLOW_FALSE", snapshot.writesFormalFlow);
            RequireFalse(report, "ITEM_EFFECT_SAVE_FALSE", snapshot.writesFormalSaveData);
            RequireFalse(report, "ITEM_EFFECT_REWARD_FALSE", snapshot.grantsFormalReward);
            RequireFalse(report, "ITEM_EFFECT_CHAPTER_FALSE", snapshot.advancesChapter);
            RequireFalse(report, "ITEM_EFFECT_UI_LAYOUT_FALSE", snapshot.touchesFormalSceneUiLayout);
            RequireEquals(report, "ITEM_EFFECT_FORMAL_LEAK_ZERO", "formal leak count", snapshot.formalLeakCount, 0);
            RequireEquals(report, "ITEM_EFFECT_UI_LAYOUT_ZERO", "UI layout write count", snapshot.uiLayoutWriteCount, 0);
        }

        private static void ValidateProfiles(
            BuildSandboxValidationReport report,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            RequireEquals(report, "ITEM_EFFECT_ROSTER_COUNT", "roster item count", snapshot.rosterItemCount, BattleSandboxPlayableFullRosterRegression.ExpectedRosterItemCount);
            RequireEquals(report, "ITEM_EFFECT_MAPPED_ROSTER_COUNT", "mapped roster item count", snapshot.mappedRosterItemCount, snapshot.rosterItemCount);
            RequireMinimum(report, "ITEM_EFFECT_FAMILY_COUNT", "distinct effect family", snapshot.distinctEffectFamilyCount, RequiredEffectFamilyKeys.Length);
            RequireMinimum(report, "ITEM_EFFECT_ATTACK_COUNT", "attack profile", snapshot.attackProfileCount, BattleSandboxPlayableFullRosterRegression.ExpectedAttackDamageItemIds.Length);
            RequireMinimum(report, "ITEM_EFFECT_SUPPORT_COUNT", "support profile", snapshot.supportProfileCount, BattleSandboxPlayableFullRosterRegression.ExpectedNoDamageSupportItemIds.Length);
            RequireMinimum(report, "ITEM_EFFECT_ENERGY_COUNT", "energy profile", snapshot.energyProfileCount, 1);
            RequireMinimum(report, "ITEM_EFFECT_GUARD_COUNT", "guard profile", snapshot.guardProfileCount, 3);
            RequireMinimum(report, "ITEM_EFFECT_CLEANSE_COUNT", "cleanse profile", snapshot.cleanseProfileCount, 4);
            RequireMinimum(report, "ITEM_EFFECT_CONTROL_COUNT", "control profile", snapshot.controlProfileCount, 4);
            RequireMinimum(report, "ITEM_EFFECT_PEACH_COUNT", "peach profile", snapshot.peachProfileCount, 2);

            foreach (string required in RequiredEffectFamilyKeys)
            {
                if (!snapshot.profileRows.Any(row => string.Equals(row.effectFamilyKey, required, StringComparison.Ordinal)))
                {
                    report.AddError(
                        "ITEM_EFFECT_REQUIRED_FAMILY_MISSING",
                        $"Missing required effect family {required}.",
                        nameof(BattleSandboxItemEffectRuntimeProfileRow));
                }
            }

            foreach (BattleSandboxItemEffectRuntimeProfileRow row in snapshot.profileRows)
            {
                if (row == null
                    || string.IsNullOrWhiteSpace(row.itemId)
                    || string.IsNullOrWhiteSpace(row.itemEffectKey)
                    || string.IsNullOrWhiteSpace(row.displayNameChinese)
                    || string.IsNullOrWhiteSpace(row.effectFamilyChinese)
                    || string.IsNullOrWhiteSpace(row.effectRoleChinese))
                {
                    report.AddError(
                        "ITEM_EFFECT_PROFILE_IDENTITY_MISSING",
                        $"Profile row is missing item/effect display identity. item={row?.itemId}.",
                        nameof(BattleSandboxItemEffectRuntimeProfileRow));
                    continue;
                }

                if (!row.devOnly || row.isEnabled)
                {
                    report.AddError(
                        "ITEM_EFFECT_PROFILE_SCOPE",
                        $"Profile must remain devOnly=true and isEnabled=false. item={row.itemId}.",
                        nameof(BattleSandboxItemEffectRuntimeProfileRow));
                }
            }
        }

        private static void ValidateRuntimeSamples(
            BuildSandboxValidationReport report,
            BattleSandboxItemEffectRuntimePreviewSnapshot snapshot)
        {
            RequireEquals(report, "ITEM_EFFECT_SUPPORT_DAMAGE_ZERO", "support damage leak", snapshot.supportDamageLeakCount, 0);
            RequireEquals(report, "ITEM_EFFECT_PLAYER_LEAK_ZERO", "player-side answer leak", snapshot.playerSideAnswerLeakCount, 0);
            RequireMinimum(report, "ITEM_EFFECT_RUNTIME_ROW_COVERAGE", "item effect runtime sample", snapshot.itemEffectRuntimeRowCount, snapshot.rosterItemCount);

            foreach (BattleSandboxItemEffectRuntimeSampleRow row in snapshot.sampleRows)
            {
                if (row == null || !row.passed)
                {
                    report.AddError(
                        "ITEM_EFFECT_SAMPLE_FAILED",
                        $"Runtime item effect sample failed. item={row?.itemId}, effectRows={row?.effectRows}, damage={row?.enemyHpDamageTotal}.",
                        nameof(BattleSandboxItemEffectRuntimeSampleRow));
                }
            }
        }

        private static int CountPlayerLeaks(IEnumerable<BattleSandboxRuntimeLoopRow> rows)
        {
            int count = 0;
            foreach (BattleSandboxRuntimeLoopRow row in rows ?? Array.Empty<BattleSandboxRuntimeLoopRow>())
            {
                string joinedText =
                    $"{row.stateLineChinese} {row.castSkillLineChinese} {row.combatLogLineChinese} {row.floatingTextChinese}";
                if (ContainsForbiddenPlayerToken(row.stateLineChinese)
                    || ContainsForbiddenPlayerToken(row.castSkillLineChinese)
                    || ContainsForbiddenPlayerToken(row.combatLogLineChinese)
                    || ContainsForbiddenPlayerToken(row.floatingTextChinese)
                    || (!string.IsNullOrWhiteSpace(row.itemId)
                        && joinedText.IndexOf(row.itemId, StringComparison.OrdinalIgnoreCase) >= 0)
                    || joinedText.IndexOf("preview_", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            string safeValue = value ?? string.Empty;
            return ForbiddenPlayerAnswerTokens.Any(token =>
                !string.IsNullOrWhiteSpace(token)
                && safeValue.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            string code,
            bool condition)
        {
            if (!condition)
            {
                report.AddError(code, "Expected true.", PackageName);
            }
        }

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            string code,
            bool condition)
        {
            if (condition)
            {
                report.AddError(code, "Expected false.", PackageName);
            }
        }

        private static void RequireEquals(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual != expected)
            {
                report.AddError(code, $"{label} expected {expected}, actual {actual}.", PackageName);
            }
        }

        private static void RequireMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int minimum)
        {
            if (actual < minimum)
            {
                report.AddError(code, $"{label} expected at least {minimum}, actual {actual}.", PackageName);
            }
        }
    }
}
#endif
