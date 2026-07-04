using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxShapeBuildRulePreview
    {
        public const string PackageName = "V0.4-BattleSandboxShapeBuildRulePreview01";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsCurrentBoardSnapshot = true;
        public bool evaluatesShapeBuildRules = true;
        public bool writesBossStateShortLine = true;
        public bool writesMechanicFloatingText = true;
        public bool writesCombatFeedbackText = true;
        public bool runsFormalCombat;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public bool playerUiShowsFullAnswers;
        public List<BattleSandboxShapeBuildRuleMatch> matches = new();

        public int RuleDefinitionCount => matches?.Count ?? 0;
        public int MatchedRuleCount => matches?.Count(match => match != null && match.isMatched) ?? 0;
        public int FeedbackRowCount => matches?.Count(match => match != null && match.isMatched && match.playerVisible) ?? 0;
        public int PlayerSideAnswerLeakCount => matches?.Count(match => match != null && match.PlayerSideAnswerLeak) ?? 1;
        public int FormalLeakCount => CountFormalLeaks();

        public bool DevOnlyIsolationPass =>
            devOnly
            && !isEnabled
            && readsCurrentBoardSnapshot
            && evaluatesShapeBuildRules
            && !runsFormalCombat
            && !writesFormalFlow
            && !writesFormalSaveData
            && !grantsFormalReward
            && !advancesChapter
            && !opensFeatureFlag
            && !playerUiShowsFullAnswers
            && PlayerSideAnswerLeakCount == 0
            && FormalLeakCount == 0;

        private int CountFormalLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!readsCurrentBoardSnapshot) leaks++;
            if (!evaluatesShapeBuildRules) leaks++;
            if (!writesBossStateShortLine) leaks++;
            if (!writesMechanicFloatingText) leaks++;
            if (!writesCombatFeedbackText) leaks++;
            if (runsFormalCombat) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (grantsFormalReward) leaks++;
            if (advancesChapter) leaks++;
            if (opensFeatureFlag) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            leaks += matches?.Count(match => match == null || match.FormalLeak) ?? 1;
            return leaks;
        }
    }

    [Serializable]
    public sealed class BattleSandboxShapeBuildRuleMatch
    {
        public string ruleId = string.Empty;
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public bool isMatched;
        public int matchedItemCount;
        public string matchedItemIds = string.Empty;
        public string matchedShapeIds = string.Empty;
        public string matchedOccupiedCells = string.Empty;
        public string matchedRotations = string.Empty;
        public string bossStateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string feedbackKind = BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback;
        public string sourceDataPath = string.Empty;
        public bool usesCastBar;
        public bool usesBossInfoLanguage;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerVisible = true;
        public bool developerPanelVisible = true;
        public bool playerShowsCompleteAnswer;
        public bool runsFormalCombat;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;

        public bool PlayerSideAnswerLeak =>
            PlayerTextFields().Any(value =>
                string.IsNullOrWhiteSpace(value)
                || ContainsLatin(value)
                || ContainsForbiddenPlayerToken(value));

        public bool FormalLeak =>
            !devOnly
            || isEnabled
            || !playerVisible
            || !developerPanelVisible
            || playerShowsCompleteAnswer
            || runsFormalCombat
            || writesFormalFlow
            || writesFormalSaveData
            || grantsFormalReward
            || advancesChapter
            || opensFeatureFlag;

        private IEnumerable<string> PlayerTextFields()
        {
            yield return bossStateLineChinese;
            yield return castSkillLineChinese;
            yield return floatingTextChinese;
            yield return combatLogLineChinese;
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string[] tokens =
            {
                "hardSolutionTags",
                "requiredTags",
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "DropBias",
                "dropBias",
                "Boss",
                "keyRequirements",
                "previewWeight",
                "bossSixKeyFullAnswer",
                "solution",
                "\u7b54\u6848",
                "\u89e3\u6cd5",
                "\u6743\u91cd",
                "\u516d\u94a5\u5319",
                "\u9898\u76ee"
            };
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public static class BattleSandboxShapeBuildRulePreviewBuilder
    {
        private const string SourceDataPath = "BattleSandboxShapeBuildRulePreview.currentV04Board.shapeBuildRules";

        public static BattleSandboxShapeBuildRulePreview Evaluate(BuildSandboxLayoutSnapshot snapshot)
        {
            List<BuildSandboxPlacedItemSnapshot> items = NormalizeItems(snapshot);
            BattleSandboxShapeBuildRulePreview preview = new()
            {
                packageName = BattleSandboxShapeBuildRulePreview.PackageName,
                devOnly = true,
                isEnabled = false,
                readsCurrentBoardSnapshot = true,
                evaluatesShapeBuildRules = true,
                writesBossStateShortLine = true,
                writesMechanicFloatingText = true,
                writesCombatFeedbackText = true,
                runsFormalCombat = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false,
                playerUiShowsFullAnswers = false
            };

            preview.matches.Add(EvaluateVerticalDefenseWall(items));
            preview.matches.Add(EvaluateCornerCleanseArray(items));
            preview.matches.Add(EvaluateFurnaceCoreArray(items));
            preview.matches.Add(EvaluateThunderFireCrossArray(items));
            return preview;
        }

        public static IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> BuildFeedbackRows(
            BattleSandboxShapeBuildRulePreview preview)
        {
            return (preview?.matches ?? new List<BattleSandboxShapeBuildRuleMatch>())
                .Where(match => match != null && match.isMatched && match.playerVisible)
                .Select(ToFeedbackRow)
                .ToArray();
        }

        private static BattleSandboxEnemyCombatFeedbackRow ToFeedbackRow(BattleSandboxShapeBuildRuleMatch match)
        {
            return new BattleSandboxEnemyCombatFeedbackRow
            {
                feedbackId = "shapeBuild." + CleanStableKeySegment(match.englishStableKey),
                feedbackKind = match.feedbackKind,
                bossDisplayNameChinese = "\u9996\u9886",
                stateLineChinese = match.bossStateLineChinese,
                castSkillLineChinese = match.castSkillLineChinese,
                floatingTextChinese = match.floatingTextChinese,
                combatLogLineChinese = match.combatLogLineChinese,
                reuseSourceComponent = "BattleFeedbackMechanicHintExtension",
                reuseLanguagePattern = "Shape build phenomenon masked as combat feedback",
                sourceDataPath = match.sourceDataPath,
                developerDataPanelFieldKey = match.englishStableKey,
                castDurationSeconds = match.usesCastBar ? 2.8f : 2.2f,
                devOnly = true,
                isEnabled = false,
                playerVisible = true,
                developerPanelVisible = true,
                playerShowsCompleteAnswer = false,
                usesFloatingCombatTextLanguage = true,
                usesEnemyCastBarLanguage = match.usesCastBar,
                usesBossInfoLanguage = match.usesBossInfoLanguage,
                runsFormalCombat = false,
                callsFormalDamageSettlement = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false
            };
        }

        private static BattleSandboxShapeBuildRuleMatch EvaluateVerticalDefenseWall(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            List<BuildSandboxPlacedItemSnapshot> candidates = items
                .Where(item => HasShape(item, "Vertical2") || HasShape(item, "Square4"))
                .ToList();
            List<BuildSandboxPlacedItemSnapshot> matched = new();

            foreach (int column in candidates
                         .SelectMany(item => item.occupiedCells ?? new List<ItemShapeCell>())
                         .Select(cell => cell.x)
                         .Distinct()
                         .OrderBy(value => value))
            {
                List<BuildSandboxPlacedItemSnapshot> inColumn = candidates
                    .Where(item => (item.occupiedCells ?? new List<ItemShapeCell>()).Any(cell => cell.x == column))
                    .Distinct()
                    .ToList();
                bool hasVertical = inColumn.Any(item => HasShape(item, "Vertical2"));
                bool hasSquare = inColumn.Any(item => HasShape(item, "Square4"));
                if (inColumn.Count >= 2 && (hasVertical && hasSquare || inColumn.Count(item => HasShape(item, "Vertical2")) >= 2))
                {
                    matched = inColumn;
                    break;
                }
            }

            return CreateMatch(
                "shape_rule_vertical_defense_wall",
                "vertical_defense_wall",
                "\u7ad6\u5411\u62a4\u9635\u5899",
                matched,
                BattleSandboxEnemyCombatFeedbackKinds.BossState,
                "\u9996\u9886\uff1a\u653b\u52bf\u88ab\u62a4\u9635\u538b\u4f4f",
                "\u9632\u7ebf\u6536\u7d27",
                "\u62a4\u9635\u6210\u5899",
                "\u3010\u9635\u52bf\u3011\u62a4\u9635\u8fde\u6210\u5899\uff0c\u9996\u9886\u653b\u52bf\u53d8\u7f13\u3002",
                usesCastBar: false,
                usesBossInfoLanguage: true);
        }

        private static BattleSandboxShapeBuildRuleMatch EvaluateCornerCleanseArray(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            List<BuildSandboxPlacedItemSnapshot> matched = items
                .Where(item => HasShape(item, "Corner3") && HasAnyTag(item, "cleanse", "purify", "jing_e", "cleanse_preview", "purifying"))
                .ToList();

            return CreateMatch(
                "shape_rule_corner_cleanse_array",
                "corner_cleanse_array",
                "\u62d0\u89d2\u51c0\u5316\u9635",
                matched,
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                "\u673a\u5236\u53cd\u9988\uff1a\u6d4a\u6c14\u88ab\u62d0\u89d2\u5bfc\u5f00",
                "\u51c0\u5316\u56de\u54cd",
                "\u6c61\u75d5\u9000\u6563",
                "\u3010\u673a\u5236\u3011\u62d0\u89d2\u5904\u4eae\u8d77\u51c0\u5316\u56de\u54cd\u3002",
                usesCastBar: false,
                usesBossInfoLanguage: false);
        }

        private static BattleSandboxShapeBuildRuleMatch EvaluateFurnaceCoreArray(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            List<BuildSandboxPlacedItemSnapshot> cores = items
                .Where(item => HasShape(item, "Square4") && HasAnyTag(item, "core", "core_preview", "orange_core_preview"))
                .ToList();
            List<BuildSandboxPlacedItemSnapshot> talismans = items
                .Where(item => HasAnyTag(item, "talisman_preview", "lihuo", "jing_lei", "jing_e", "fire", "thunder", "cleanse"))
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot core in cores)
            {
                List<BuildSandboxPlacedItemSnapshot> nearby = talismans
                    .Where(item => !ReferenceEquals(item, core) && AreNear(core, item, includeDiagonal: true))
                    .ToList();
                if (nearby.Count > 0)
                {
                    List<BuildSandboxPlacedItemSnapshot> matched = new() { core };
                    matched.AddRange(nearby);
                    return CreateMatch(
                        "shape_rule_furnace_core_array",
                        "furnace_core_array",
                        "\u7089\u82af\u6838\u5fc3\u9635",
                        matched,
                        BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                        "\u9996\u9886\uff1a\u7089\u82af\u5149\u52bf\u6b63\u5728\u805a\u62e2",
                        "\u7089\u82af\u805a\u5149",
                        "\u7089\u82af\u56de\u54cd",
                        "\u3010\u6838\u5fc3\u3011\u7b26\u5149\u8d34\u8fd1\u7089\u82af\uff0c\u573a\u4e0a\u8282\u594f\u53d8\u7a33\u3002",
                        usesCastBar: false,
                        usesBossInfoLanguage: false);
                }
            }

            return CreateMatch(
                "shape_rule_furnace_core_array",
                "furnace_core_array",
                "\u7089\u82af\u6838\u5fc3\u9635",
                Array.Empty<BuildSandboxPlacedItemSnapshot>(),
                BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                "\u9996\u9886\uff1a\u7089\u82af\u5149\u52bf\u6b63\u5728\u805a\u62e2",
                "\u7089\u82af\u805a\u5149",
                "\u7089\u82af\u56de\u54cd",
                "\u3010\u6838\u5fc3\u3011\u7b26\u5149\u8d34\u8fd1\u7089\u82af\uff0c\u573a\u4e0a\u8282\u594f\u53d8\u7a33\u3002",
                usesCastBar: false,
                usesBossInfoLanguage: false);
        }

        private static BattleSandboxShapeBuildRuleMatch EvaluateThunderFireCrossArray(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items)
        {
            List<BuildSandboxPlacedItemSnapshot> fireItems = items
                .Where(item => HasAnyTag(item, "lihuo", "fire", "damage_preview"))
                .ToList();
            List<BuildSandboxPlacedItemSnapshot> thunderItems = items
                .Where(item => HasAnyTag(item, "jing_lei", "thunder", "shieldBreak", "shield_break"))
                .ToList();

            foreach (BuildSandboxPlacedItemSnapshot fire in fireItems)
            {
                BuildSandboxPlacedItemSnapshot thunder = thunderItems
                    .FirstOrDefault(item => !ReferenceEquals(item, fire) && AreNear(fire, item, includeDiagonal: true));
                if (thunder != null)
                {
                    return CreateMatch(
                        "shape_rule_thunder_fire_cross_array",
                        "thunder_fire_cross_array",
                        "\u96f7\u706b\u4ea4\u9519\u9635",
                        new[] { fire, thunder },
                        BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                        "\u9996\u9886\uff1a\u7206\u53d1\u7a97\u53e3\u88ab\u903c\u51fa",
                        "\u96f7\u706b\u4ea4\u9519",
                        "\u96f7\u706b\u4ea4\u54cd",
                        "\u3010\u8f93\u51fa\u3011\u96f7\u706b\u76f8\u90bb\uff0c\u538b\u5236\u611f\u589e\u5f3a\u3002",
                        usesCastBar: true,
                        usesBossInfoLanguage: false);
                }
            }

            return CreateMatch(
                "shape_rule_thunder_fire_cross_array",
                "thunder_fire_cross_array",
                "\u96f7\u706b\u4ea4\u9519\u9635",
                Array.Empty<BuildSandboxPlacedItemSnapshot>(),
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                "\u9996\u9886\uff1a\u7206\u53d1\u7a97\u53e3\u88ab\u903c\u51fa",
                "\u96f7\u706b\u4ea4\u9519",
                "\u96f7\u706b\u4ea4\u54cd",
                "\u3010\u8f93\u51fa\u3011\u96f7\u706b\u76f8\u90bb\uff0c\u538b\u5236\u611f\u589e\u5f3a\u3002",
                usesCastBar: true,
                usesBossInfoLanguage: false);
        }

        private static BattleSandboxShapeBuildRuleMatch CreateMatch(
            string ruleId,
            string englishStableKey,
            string chineseDisplayName,
            IEnumerable<BuildSandboxPlacedItemSnapshot> matchedItems,
            string feedbackKind,
            string bossStateLine,
            string castSkillLine,
            string floatingText,
            string combatLogLine,
            bool usesCastBar,
            bool usesBossInfoLanguage)
        {
            List<BuildSandboxPlacedItemSnapshot> items = (matchedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
                .Where(item => item != null)
                .Distinct()
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToList();
            return new BattleSandboxShapeBuildRuleMatch
            {
                ruleId = ruleId ?? string.Empty,
                englishStableKey = englishStableKey ?? string.Empty,
                chineseDisplayName = chineseDisplayName ?? string.Empty,
                isMatched = items.Count > 0,
                matchedItemCount = items.Count,
                matchedItemIds = string.Join("|", items.Select(item => item.itemId)),
                matchedShapeIds = string.Join("|", items.Select(item => item.shapeId).Distinct(StringComparer.Ordinal)),
                matchedOccupiedCells = string.Join("|", items.Select(item => FormatCells(item.occupiedCells))),
                matchedRotations = string.Join("|", items.Select(item => item.rotation.ToString()).Distinct(StringComparer.Ordinal)),
                bossStateLineChinese = bossStateLine ?? string.Empty,
                castSkillLineChinese = castSkillLine ?? string.Empty,
                floatingTextChinese = floatingText ?? string.Empty,
                combatLogLineChinese = combatLogLine ?? string.Empty,
                feedbackKind = feedbackKind ?? BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                sourceDataPath = SourceDataPath + "." + (englishStableKey ?? string.Empty),
                usesCastBar = usesCastBar,
                usesBossInfoLanguage = usesBossInfoLanguage,
                devOnly = true,
                isEnabled = false,
                playerVisible = true,
                developerPanelVisible = true,
                playerShowsCompleteAnswer = false,
                runsFormalCombat = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false
            };
        }

        private static List<BuildSandboxPlacedItemSnapshot> NormalizeItems(BuildSandboxLayoutSnapshot snapshot)
        {
            return (snapshot?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>())
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.itemId))
                .Select(item =>
                {
                    if (item.occupiedCells == null || item.occupiedCells.Count == 0)
                    {
                        item.occupiedCells = new List<ItemShapeCell> { item.anchorCell };
                    }

                    if (item.tags == null)
                    {
                        item.tags = new List<string>();
                    }

                    return item;
                })
                .ToList();
        }

        private static bool HasShape(BuildSandboxPlacedItemSnapshot item, string shapeId)
        {
            return string.Equals(item?.shapeId, shapeId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasAnyTag(BuildSandboxPlacedItemSnapshot item, params string[] tags)
        {
            return (item?.tags ?? new List<string>()).Any(itemTag =>
                tags.Any(tag => string.Equals(itemTag, tag, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool AreNear(
            BuildSandboxPlacedItemSnapshot a,
            BuildSandboxPlacedItemSnapshot b,
            bool includeDiagonal)
        {
            if (a == null || b == null)
            {
                return false;
            }

            foreach (ItemShapeCell aCell in a.occupiedCells ?? new List<ItemShapeCell>())
            {
                foreach (ItemShapeCell bCell in b.occupiedCells ?? new List<ItemShapeCell>())
                {
                    int dx = Math.Abs(aCell.x - bCell.x);
                    int dy = Math.Abs(aCell.y - bCell.y);
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    if (includeDiagonal ? dx <= 1 && dy <= 1 : dx + dy == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(
                ";",
                (cells ?? Array.Empty<ItemShapeCell>())
                .OrderBy(cell => cell.x)
                .ThenBy(cell => cell.y)
                .Select(cell => cell.x + ":" + cell.y));
        }

        private static string CleanStableKeySegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            char[] chars = value
                .Where(character => char.IsLetterOrDigit(character) || character == '.' || character == '_' || character == '-')
                .ToArray();
            return chars.Length == 0 ? "unknown" : new string(chars);
        }
    }
}
