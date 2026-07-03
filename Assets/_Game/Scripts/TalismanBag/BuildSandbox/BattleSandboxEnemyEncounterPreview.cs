using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxEnemyEncounterPreview
    {
        public const string PackageName = "V0.4-BattleSandboxEnemyEncounterPreview01";
        public const string ReferenceMode = "DevOnlyEnemyBossPreviewNoCombat";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = string.Empty;
        public string referenceMode = ReferenceMode;
        public bool devOnly = true;
        public bool isEnabled;
        public bool runsFormalCombat;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool readsFormalEnemyDefinition;
        public bool readsFormalBossConfig;
        public bool changesFormalDamage;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsFullAnswers;
        public bool developerFullAnswersStayInDataPanel = true;
        public List<BattleSandboxEnemyEncounterRow> rows = new();
        public List<BattleSandboxEncounterDeveloperAnswerLink> developerAnswerLinks = new();

        public int EnemyCount => rows?.Count(row => row != null && row.encounterKind == "enemy") ?? 0;
        public int BossCount => rows?.Count(row => row != null && row.encounterKind == "boss") ?? 0;
        public int DisplayedHintCount => rows?.Sum(row => row?.PlayerHintCount ?? 0) ?? 0;
        public int MaskedAnswerFieldCount => developerAnswerLinks?.Count(link => link != null && link.maskedFromPlayer) ?? 0;
        public int FormalLeakCount => CountFormalLeaks();

        private int CountFormalLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (runsFormalCombat) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (grantsFormalReward) leaks++;
            if (readsFormalEnemyDefinition) leaks++;
            if (readsFormalBossConfig) leaks++;
            if (changesFormalDamage) leaks++;
            if (!playerUiChineseOnly) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            if (!developerFullAnswersStayInDataPanel) leaks++;
            leaks += rows?.Count(row => row == null || row.FormalLeak) ?? 1;
            leaks += developerAnswerLinks?.Count(link => link == null || link.playerVisible || !link.maskedFromPlayer) ?? 1;
            return leaks;
        }
    }

    [Serializable]
    public sealed class BattleSandboxEnemyEncounterRow
    {
        public string encounterId = string.Empty;
        public string encounterKind = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string selectorLabel = string.Empty;
        public string mapMechanicChinese = string.Empty;
        public string encounterMechanicChinese = string.Empty;
        public string bossSkillChinese = string.Empty;
        public string weaknessWindowChinese = string.Empty;
        public string testTargetChinese = string.Empty;
        public string readinessPreviewChinese = string.Empty;
        public string failureFeedbackChinese = string.Empty;
        public string dropBiasAtmosphereChinese = string.Empty;
        public string sourceDataPath = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerVisible = true;
        public bool showsCompleteAnswer;
        public bool runsFormalCombat;
        public bool grantsFormalReward;
        public bool writesFormalSaveData;
        public bool writesFormalFlow;

        public int PlayerHintCount => CountNonEmpty(
            mapMechanicChinese,
            encounterMechanicChinese,
            bossSkillChinese,
            weaknessWindowChinese,
            testTargetChinese,
            readinessPreviewChinese,
            failureFeedbackChinese,
            dropBiasAtmosphereChinese);

        public bool FormalLeak =>
            !devOnly
            || isEnabled
            || !playerVisible
            || showsCompleteAnswer
            || runsFormalCombat
            || grantsFormalReward
            || writesFormalSaveData
            || writesFormalFlow;

        private static int CountNonEmpty(params string[] values)
        {
            return values.Count(value => !string.IsNullOrWhiteSpace(value));
        }
    }

    [Serializable]
    public sealed class BattleSandboxEncounterDeveloperAnswerLink
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string sourceDataPath = string.Empty;
        public string dataPanelSlot = "ProblemReadinessPanelSlot";
        public bool developerVisible = true;
        public bool playerVisible;
        public bool maskedFromPlayer = true;
    }

    public static class BattleSandboxEnemyEncounterPreviewBuilder
    {
        private static readonly string[] SensitiveAnswerKeys =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBiasWeights",
            "bossSixKeyFullAnswer"
        };

        public static BattleSandboxEnemyEncounterPreview Build(
            BuildSandboxPreviewContext context,
            MechanicHintFeedbackPreview hintPreview = null,
            BuildTuningDataPanelPreview dataPanel = null)
        {
            BuildSandboxPreviewContext safeContext = context ?? new BuildSandboxPreviewContext();
            BuildProblemSeedDataset dataset =
                safeContext.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            EnemyBossValidationPool pool =
                safeContext.enemyBossValidationPool ?? EnemyBossValidationPool.CreateDefault();

            BattleSandboxEnemyEncounterPreview preview = new()
            {
                packageName = BattleSandboxEnemyEncounterPreview.PackageName,
                sourcePreviewBuildId = safeContext.previewBuildId ?? string.Empty,
                referenceMode = BattleSandboxEnemyEncounterPreview.ReferenceMode,
                devOnly = true,
                isEnabled = false,
                runsFormalCombat = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                readsFormalEnemyDefinition = false,
                readsFormalBossConfig = false,
                changesFormalDamage = false,
                playerUiChineseOnly = true,
                playerUiShowsFullAnswers = false,
                developerFullAnswersStayInDataPanel = true
            };

            AddEnemyRows(preview, pool, dataset);
            AddBossRows(preview, pool, dataset);
            AddDeveloperAnswerLinks(preview, dataPanel);
            return preview;
        }

        public static IReadOnlyList<string> SensitiveKeys => SensitiveAnswerKeys;

        private static void AddEnemyRows(
            BattleSandboxEnemyEncounterPreview preview,
            EnemyBossValidationPool pool,
            BuildProblemSeedDataset dataset)
        {
            List<EnemyProblemSeed> problems = dataset.enemyProblems ?? new List<EnemyProblemSeed>();
            List<MapRuleSeed> mapRules = dataset.mapRules ?? new List<MapRuleSeed>();
            int index = 0;
            foreach (BuildSandboxEnemyProfile enemy in pool.enemies ?? new List<BuildSandboxEnemyProfile>())
            {
                if (enemy == null)
                {
                    continue;
                }

                EnemyProblemSeed problem = FindEnemyProblem(enemy, problems, index);
                MapRuleSeed mapRule = FindMapRuleForEnemy(problem, mapRules, index);
                FailureHintSeed failureHint = problem?.failureHint ?? new FailureHintSeed();

                preview.rows.Add(new BattleSandboxEnemyEncounterRow
                {
                    encounterId = enemy.enemyId ?? string.Empty,
                    encounterKind = "enemy",
                    chineseDisplayName = PlayerText(enemy.chineseRole, "敌人"),
                    selectorLabel = $"敌人：{PlayerText(enemy.chineseRole, "敌人")}",
                    mapMechanicChinese = MapHint(mapRule),
                    encounterMechanicChinese = EnemyHint(enemy, problem),
                    bossSkillChinese = "普通敌人题目不显示首领阶段；这里只看敌人压迫节奏。",
                    weaknessWindowChinese = "若敌人露出破绽，只提示短暂窗口与战斗反馈，不显示触发公式。",
                    testTargetChinese = TestTargetText(enemy.validationTags, problem?.pressureType),
                    readinessPreviewChinese = "准备度预览只告诉你当前构筑是否覆盖这类压力；完整答案留在开发者数据面板。",
                    failureFeedbackChinese = FailureText(failureHint),
                    dropBiasAtmosphereChinese = "战后物件气息会偏向补短板，但这里不显示权重或清单。",
                    sourceDataPath = "EnemyBossValidationPool.enemies + BuildProblemSeedDataset.enemyProblems",
                    devOnly = enemy.devOnly && (problem?.devOnly ?? true) && (mapRule?.devOnly ?? true),
                    isEnabled = enemy.isEnabled || (problem?.isEnabled ?? false) || (mapRule?.isEnabled ?? false),
                    playerVisible = true,
                    showsCompleteAnswer = false,
                    runsFormalCombat = false,
                    grantsFormalReward = false,
                    writesFormalSaveData = false,
                    writesFormalFlow = enemy.entersFormalFlow || (problem?.entersFormalFlow ?? false)
                });
                index++;
            }
        }

        private static void AddBossRows(
            BattleSandboxEnemyEncounterPreview preview,
            EnemyBossValidationPool pool,
            BuildProblemSeedDataset dataset)
        {
            List<BossProblemSeed> problems = dataset.bossProblems ?? new List<BossProblemSeed>();
            List<MapRuleSeed> mapRules = dataset.mapRules ?? new List<MapRuleSeed>();
            int index = 0;
            foreach (BuildSandboxBossProfile boss in pool.bosses ?? new List<BuildSandboxBossProfile>())
            {
                if (boss == null)
                {
                    continue;
                }

                BossProblemSeed problem = FindBossProblem(boss, problems, index);
                MapRuleSeed mapRule = FindMapRuleForBoss(problem, mapRules, index);
                WeaknessWindowSeed weakness = problem?.weaknessWindows?.FirstOrDefault();
                FailureHintSeed failureHint = problem?.failureHints?.FirstOrDefault() ?? new FailureHintSeed();

                preview.rows.Add(new BattleSandboxEnemyEncounterRow
                {
                    encounterId = boss.bossId ?? string.Empty,
                    encounterKind = "boss",
                    chineseDisplayName = PlayerText(boss.chineseRole, "首领"),
                    selectorLabel = $"首领：{PlayerText(boss.chineseRole, "首领")}",
                    mapMechanicChinese = MapHint(mapRule),
                    encounterMechanicChinese = BossMechanicHint(boss, problem),
                    bossSkillChinese = BossSkillHint(problem),
                    weaknessWindowChinese = WeaknessHint(weakness),
                    testTargetChinese = TestTargetText(boss.validationTags, boss.bossMechanic),
                    readinessPreviewChinese = "准备度预览只显示是否大致够用；钥匙、数值和完整答案留在开发者数据面板。",
                    failureFeedbackChinese = FailureText(failureHint),
                    dropBiasAtmosphereChinese = DropBiasHint(problem),
                    sourceDataPath = "EnemyBossValidationPool.bosses + BuildProblemSeedDataset.bossProblems",
                    devOnly = boss.devOnly && (problem?.devOnly ?? true) && (mapRule?.devOnly ?? true),
                    isEnabled = boss.isEnabled || (problem?.isEnabled ?? false) || (mapRule?.isEnabled ?? false),
                    playerVisible = true,
                    showsCompleteAnswer = false,
                    runsFormalCombat = false,
                    grantsFormalReward = false,
                    writesFormalSaveData = false,
                    writesFormalFlow = boss.entersFormalFlow || (problem?.entersFormalFlow ?? false)
                });
                index++;
            }
        }

        private static EnemyProblemSeed FindEnemyProblem(
            BuildSandboxEnemyProfile enemy,
            IReadOnlyList<EnemyProblemSeed> problems,
            int index)
        {
            if (problems == null || problems.Count == 0)
            {
                return null;
            }

            IEnumerable<string> tokens = (enemy.validationTags ?? new List<string>())
                .Concat(enemy.validationTargetBuilds ?? new List<string>())
                .Concat(new[] { enemy.enemyType, enemy.enemyId });
            EnemyProblemSeed match = problems.FirstOrDefault(problem =>
                problem != null
                && tokens.Any(token => ContainsRelated(token, problem.problemType)
                    || ContainsRelated(token, problem.pressureType)
                    || ContainsRelated(problem.problemType, token)
                    || ContainsRelated(problem.pressureType, token)));
            return match ?? problems[Math.Abs(index) % problems.Count];
        }

        private static BossProblemSeed FindBossProblem(
            BuildSandboxBossProfile boss,
            IReadOnlyList<BossProblemSeed> problems,
            int index)
        {
            if (problems == null || problems.Count == 0)
            {
                return null;
            }

            IEnumerable<string> tokens = (boss.validationTags ?? new List<string>())
                .Concat(boss.validationTargetBuilds ?? new List<string>())
                .Concat(boss.recommendedSynergies ?? new List<string>())
                .Concat(new[] { boss.bossMechanic, boss.bossId });
            if (tokens.Any(token => ContainsAny(token, "hybrid", "combo")))
            {
                BossProblemSeed complexMatch = problems.FirstOrDefault(problem =>
                    problem != null && ContainsAny(problem.bossProblemId, "complex", "mixed"));
                if (complexMatch != null)
                {
                    return complexMatch;
                }
            }

            BossProblemSeed match = problems.FirstOrDefault(problem =>
                problem != null
                && tokens.Any(token => ContainsRelated(token, problem.bossProblemId)
                    || ContainsRelated(problem.bossProblemId, token)
                    || ContainsRelated(token, problem.displayName)
                    || ContainsRelated(token, problem.validationGoal)
                    || (problem.requiredProblemAttributes ?? new List<string>()).Any(attribute => ContainsRelated(token, attribute))
                    || (problem.keyRequirements ?? new List<BossProblemKeySeed>()).Any(key =>
                        key != null
                        && (ContainsRelated(token, key.requirementId)
                            || ContainsRelated(token, key.keyCategory)
                            || ContainsRelated(token, key.hint)))));
            return match ?? problems[Math.Abs(index) % problems.Count];
        }

        private static MapRuleSeed FindMapRuleForEnemy(
            EnemyProblemSeed problem,
            IReadOnlyList<MapRuleSeed> mapRules,
            int index)
        {
            if (mapRules == null || mapRules.Count == 0)
            {
                return null;
            }

            MapRuleSeed match = mapRules.FirstOrDefault(rule =>
                rule != null
                && problem != null
                && (rule.enemyProblemIds ?? new List<string>()).Contains(problem.problemType, StringComparer.Ordinal));
            return match ?? mapRules[Math.Abs(index) % mapRules.Count];
        }

        private static MapRuleSeed FindMapRuleForBoss(
            BossProblemSeed problem,
            IReadOnlyList<MapRuleSeed> mapRules,
            int index)
        {
            if (mapRules == null || mapRules.Count == 0)
            {
                return null;
            }

            MapRuleSeed match = mapRules.FirstOrDefault(rule =>
                rule != null
                && problem != null
                && (rule.bossProblemIds ?? new List<string>()).Contains(problem.bossProblemId, StringComparer.Ordinal));
            return match ?? mapRules[Math.Abs(index) % mapRules.Count];
        }

        private static void AddDeveloperAnswerLinks(
            BattleSandboxEnemyEncounterPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            foreach (string key in SensitiveAnswerKeys)
            {
                BuildTuningDataPanelFieldRow row = dataPanel?.Rows?
                    .FirstOrDefault(field => string.Equals(field.englishStableKey, key, StringComparison.Ordinal));
                preview.developerAnswerLinks.Add(new BattleSandboxEncounterDeveloperAnswerLink
                {
                    englishStableKey = key,
                    chineseDisplayName = row?.chineseDisplayName ?? SensitiveChineseName(key),
                    sourceDataPath = row?.sourceDataPath ?? SensitiveSourcePath(key),
                    dataPanelSlot = row?.dataPanelSlot ?? "ProblemReadinessPanelSlot",
                    developerVisible = true,
                    playerVisible = false,
                    maskedFromPlayer = true
                });
            }
        }

        private static string MapHint(MapRuleSeed mapRule)
        {
            if (mapRule == null)
            {
                return "当前地图只显示沙盒题目线索，不接正式章节。";
            }

            string displayName = PlayerText(mapRule.displayName, "地图异象");
            string warning = PlayerText(mapRule.warningText, "阵面会发生变化，请观察供能和摆放。");
            return $"【{displayName}】{warning}";
        }

        private static string EnemyHint(BuildSandboxEnemyProfile enemy, EnemyProblemSeed problem)
        {
            string name = PlayerText(enemy?.chineseRole, "敌人");
            string summary = PlayerText(problem?.problemSummary, "会制造一类构筑压力。");
            return $"【{name}】{summary}";
        }

        private static string BossMechanicHint(BuildSandboxBossProfile boss, BossProblemSeed problem)
        {
            string name = PlayerText(boss?.chineseRole, "首领");
            string goal = PlayerText(problem?.validationGoal, "会分阶段检查构筑短板。");
            return $"【{name}】{goal}";
        }

        private static string BossSkillHint(BossProblemSeed problem)
        {
            string displayName = PlayerText(problem?.displayName, "首领");
            return $"【{displayName}】会用阶段压力试探你的构筑完整度；这里只显示威胁线索。";
        }

        private static string WeaknessHint(WeaknessWindowSeed weakness)
        {
            if (weakness == null)
            {
                return "弱点窗口：露出破绽时会有短促反馈；本包不显示触发公式。";
            }

            string name = PlayerText(weakness.displayName, "短暂破绽");
            return $"弱点窗口：{name}。只提示时机，不显示完整答案。";
        }

        private static string FailureText(FailureHintSeed hint)
        {
            string headline = PlayerText(hint?.headline, "当前构筑仍有短板");
            string detail = PlayerText(hint?.detail, "回到整备时补一手即可。");
            return $"【{headline}】{detail}";
        }

        private static string DropBiasHint(BossProblemSeed problem)
        {
            int count = problem?.dropBiases?.Count ?? 0;
            return count > 0
                ? "战后物件气息会偏向补足当前短板；不显示权重、道具清单或词条清单。"
                : "本题暂无掉落气息线索；不接正式奖励。";
        }

        private static string TestTargetText(IEnumerable<string> tags, string extraToken)
        {
            List<string> targets = (tags ?? Enumerable.Empty<string>())
                .Concat(new[] { extraToken })
                .Select(ToTargetChinese)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Take(5)
                .ToList();
            if (targets.Count == 0)
            {
                targets.Add("构筑短板");
            }

            return "测试目标：" + string.Join("、", targets) + "。";
        }

        private static string ToTargetChinese(string value)
        {
            string lower = value?.ToLowerInvariant() ?? string.Empty;
            if (ContainsAny(lower, "shield", "break")) return "护盾 / 破盾";
            if (ContainsAny(lower, "poison", "burn", "negative", "debuff", "polluted", "dot")) return "负面 / 污染";
            if (ContainsAny(lower, "control", "seal", "caster", "cast", "interrupt")) return "控制 / 打断";
            if (ContainsAny(lower, "energy", "spirit", "resource")) return "供能干扰";
            if (ContainsAny(lower, "swarm", "multi", "clear")) return "群怪清场";
            if (ContainsAny(lower, "burst", "low_hp")) return "爆发承压";
            if (ContainsAny(lower, "formation", "placement", "shape")) return "阵眼 / 摆放";
            if (ContainsAny(lower, "high_hp", "long")) return "长线输出";
            if (ContainsAny(lower, "hybrid", "combo")) return "组合构筑";
            return string.Empty;
        }

        private static string PlayerText(string value, string fallback)
        {
            string text = string.IsNullOrWhiteSpace(value) ? fallback : value;
            Dictionary<string, string> replacements = new(StringComparer.OrdinalIgnoreCase)
            {
                ["Boss"] = "首领",
                ["Build"] = "构筑",
                ["DropBias"] = "掉落倾向",
                ["hardSolutionTags"] = "解法标签",
                ["requiredSynergy"] = "协同要求",
                ["requiredAffix"] = "词条要求",
                ["requiredStats"] = "属性要求",
                ["BreakPower"] = "破盾能力",
                ["ClearPower"] = "清场能力",
                ["CleansePower"] = "净化能力",
                ["EnergyStability"] = "供能稳定",
                ["ControlPower"] = "控制打断",
                ["GuardPower"] = "护阵承伤",
                ["BurstWindow"] = "短窗爆发",
                ["PlacementShape"] = "摆放形态"
            };

            foreach (KeyValuePair<string, string> pair in replacements)
            {
                text = ReplaceOrdinalIgnoreCase(text, pair.Key, pair.Value);
            }

            return NormalizeSpaces(StripAsciiLetters(text));
        }

        private static string StripAsciiLetters(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            char[] chars = value
                .Where(character => !(character <= 127 && char.IsLetter(character)))
                .ToArray();
            return new string(chars);
        }

        private static string ReplaceOrdinalIgnoreCase(
            string source,
            string oldValue,
            string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
            {
                return source ?? string.Empty;
            }

            StringBuilder builder = new();
            int startIndex = 0;
            int matchIndex = source.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
            while (matchIndex >= 0)
            {
                builder.Append(source, startIndex, matchIndex - startIndex);
                builder.Append(newValue ?? string.Empty);
                startIndex = matchIndex + oldValue.Length;
                matchIndex = source.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
            }

            builder.Append(source, startIndex, source.Length - startIndex);
            return builder.ToString();
        }

        private static string NormalizeSpaces(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                .Replace(" ，", "，")
                .Replace(" 。", "。")
                .Replace(" 、", "、")
                .Replace(" ：", "：")
                .Replace(" ；", "；")
                .Trim();
        }

        private static bool ContainsRelated(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
            {
                return false;
            }

            string leftLower = left.ToLowerInvariant();
            string rightLower = right.ToLowerInvariant();
            return leftLower.Contains(rightLower)
                || rightLower.Contains(leftLower)
                || TokenOverlap(leftLower, rightLower);
        }

        private static bool TokenOverlap(string left, string right)
        {
            HashSet<string> leftParts = new(
                left.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(part => !IsGenericStableKeyToken(part)),
                StringComparer.Ordinal);
            return right
                .Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(part => part.Length >= 4
                    && !IsGenericStableKeyToken(part)
                    && leftParts.Contains(part));
        }

        private static bool IsGenericStableKeyToken(string value)
        {
            return string.Equals(value, "dev", StringComparison.Ordinal)
                || string.Equals(value, "enemy", StringComparison.Ordinal)
                || string.Equals(value, "boss", StringComparison.Ordinal)
                || string.Equals(value, "problem", StringComparison.Ordinal)
                || string.Equals(value, "build", StringComparison.Ordinal);
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens.Any(token => !string.IsNullOrWhiteSpace(token)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string SensitiveChineseName(string key)
        {
            return key switch
            {
                "hardSolutionTags" => "硬解标签",
                "requiredSynergy" => "必需协同",
                "requiredAffix" => "必需词条",
                "requiredStats" => "必需属性",
                "dropBiasWeights" => "掉落倾向权重",
                "bossSixKeyFullAnswer" => "首领六钥匙完整答案",
                _ => "开发者答案字段"
            };
        }

        private static string SensitiveSourcePath(string key)
        {
            return key switch
            {
                "hardSolutionTags" => "BuildProblemSeedDataset.enemyProblems.hardSolutionTags",
                "requiredSynergy" => "BuildProblemSeedDataset.bossProblems.keyRequirements",
                "requiredAffix" => "BuildProblemSeedDataset.bossProblems.keyRequirements",
                "requiredStats" => "BuildProblemSeedDataset.bossProblems.requiredProblemAttributes",
                "dropBiasWeights" => "BuildProblemSeedDataset.dropBiases.previewWeight",
                "bossSixKeyFullAnswer" => "BuildProblemSeedDataset.bossProblems.keyRequirements",
                _ => "BuildProblemSeedDataset"
            };
        }
    }

}
