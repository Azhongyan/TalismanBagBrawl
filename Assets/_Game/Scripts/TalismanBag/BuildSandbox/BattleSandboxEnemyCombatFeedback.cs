using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxEnemyCombatFeedbackPreview
    {
        public const string PackageName = "V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01";
        public const string ReferenceMode = "DevOnlyCombatFeedbackUiReuseNoFormalCombat";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = string.Empty;
        public string referenceMode = ReferenceMode;
        public bool devOnly = true;
        public bool isEnabled;
        public bool usesFloatingCombatTextLanguage = true;
        public bool usesEnemyIntentCastBarLanguage = true;
        public bool usesBossInfoLanguage = true;
        public bool playerTopicPanelRemoved = true;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsFullAnswers;
        public bool developerFullAnswersStayInDataPanel = true;
        public bool runsFormalCombat;
        public bool callsFormalDamageSettlement;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;
        public List<BattleSandboxEnemyCombatFeedbackRow> rows = new();
        public List<BattleSandboxCombatFeedbackDeveloperLink> developerAnswerLinks = new();

        public int RowCount => rows?.Count(row => row != null) ?? 0;
        public int PlayerVisibleRowCount => rows?.Count(row => row != null && row.playerVisible) ?? 0;
        public int BossStateRowCount => rows?.Count(row => row != null && row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.BossState) ?? 0;
        public int CastBarRowCount => rows?.Count(row => row != null && row.usesEnemyCastBarLanguage) ?? 0;
        public int FloatingFeedbackRowCount => rows?.Count(row => row != null && row.usesFloatingCombatTextLanguage) ?? 0;
        public int MechanicFeedbackRowCount => rows?.Count(row => row != null && row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback) ?? 0;
        public int FailureFeedbackRowCount => rows?.Count(row => row != null && row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback) ?? 0;
        public int MaskedAnswerFieldCount => developerAnswerLinks?.Count(link => link != null && link.maskedFromPlayer) ?? 0;
        public int FormalLeakCount => CountFormalLeaks();

        private int CountFormalLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!usesFloatingCombatTextLanguage) leaks++;
            if (!usesEnemyIntentCastBarLanguage) leaks++;
            if (!usesBossInfoLanguage) leaks++;
            if (!playerTopicPanelRemoved) leaks++;
            if (!playerUiChineseOnly) leaks++;
            if (playerUiShowsFullAnswers) leaks++;
            if (!developerFullAnswersStayInDataPanel) leaks++;
            if (runsFormalCombat) leaks++;
            if (callsFormalDamageSettlement) leaks++;
            if (writesFormalFlow) leaks++;
            if (writesFormalSaveData) leaks++;
            if (grantsFormalReward) leaks++;
            if (advancesChapter) leaks++;
            if (opensFeatureFlag) leaks++;
            leaks += rows?.Count(row => row == null || row.FormalLeak) ?? 1;
            leaks += developerAnswerLinks?.Count(link => link == null || link.playerVisible || !link.maskedFromPlayer) ?? 1;
            return leaks;
        }
    }

    public static class BattleSandboxEnemyCombatFeedbackKinds
    {
        public const string BossState = "bossState";
        public const string BossSkillCast = "bossSkillCast";
        public const string MechanicFeedback = "mechanicFeedback";
        public const string WeaknessWindow = "weaknessWindow";
        public const string FailureFeedback = "failureFeedback";
        public const string EnemyState = "enemyState";
    }

    [Serializable]
    public sealed class BattleSandboxEnemyCombatFeedbackRow
    {
        public string feedbackId = string.Empty;
        public string feedbackKind = string.Empty;
        public string bossDisplayNameChinese = string.Empty;
        public string stateLineChinese = string.Empty;
        public string castSkillLineChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public string combatLogLineChinese = string.Empty;
        public string reuseSourceComponent = string.Empty;
        public string reuseLanguagePattern = string.Empty;
        public string sourceDataPath = string.Empty;
        public string developerDataPanelFieldKey = string.Empty;
        public float castDurationSeconds = 2.8f;
        public bool devOnly = true;
        public bool isEnabled;
        public bool playerVisible = true;
        public bool developerPanelVisible = true;
        public bool playerShowsCompleteAnswer;
        public bool usesFloatingCombatTextLanguage = true;
        public bool usesEnemyCastBarLanguage;
        public bool usesBossInfoLanguage;
        public bool runsFormalCombat;
        public bool callsFormalDamageSettlement;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool opensFeatureFlag;

        public bool FormalLeak =>
            !devOnly
            || isEnabled
            || !playerVisible
            || !developerPanelVisible
            || playerShowsCompleteAnswer
            || runsFormalCombat
            || callsFormalDamageSettlement
            || writesFormalFlow
            || writesFormalSaveData
            || grantsFormalReward
            || advancesChapter
            || opensFeatureFlag;
    }

    [Serializable]
    public sealed class BattleSandboxCombatFeedbackDeveloperLink
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string sourceDataPath = string.Empty;
        public string dataPanelSlot = "EnemyCombatFeedbackDeveloperPanel";
        public bool developerVisible = true;
        public bool playerVisible;
        public bool maskedFromPlayer = true;
    }

    public static class BattleSandboxEnemyCombatFeedbackBuilder
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

        private static readonly string[] BossStatePhrases =
        {
            "首领气息翻涌",
            "护盾正在变厚",
            "阵眼开始发亮",
            "怒意正在升高",
            "灵压逼近边线",
            "毒火绕场蔓延",
            "召唤预兆出现",
            "封印气息聚拢"
        };

        private static readonly string[] BossCastNames =
        {
            "锁阵冲击",
            "裂甲咒阵",
            "魂火压境",
            "封印阵眼",
            "回春护幕",
            "怒意爆发",
            "幽光召唤",
            "毒火蔓延"
        };

        private static readonly string[] MechanicPhrases =
        {
            "阵面压力升高",
            "地脉正在换向",
            "供能短暂紊乱",
            "边线出现压迫",
            "灵气流速改变",
            "场上气息偏冷",
            "符位开始震动",
            "阵眼需要留意"
        };

        private static readonly string[] WeaknessPhrases =
        {
            "破绽窗口出现",
            "护盾露出裂口",
            "首领短暂停顿",
            "反制时机已到",
            "弱点正在显形",
            "压制机会打开"
        };

        private static readonly string[] FailurePhrases =
        {
            "短板被放大",
            "供能跟不上",
            "抗压略显不足",
            "清场节奏偏慢",
            "回复窗口偏窄",
            "护盾承压过高"
        };

        private static readonly string[] EnemyStatePhrases =
        {
            "敌势正在聚拢",
            "敌人准备突进",
            "敌人护势上升",
            "敌人施压加快",
            "敌人露出意图",
            "敌人短暂停顿"
        };

        public static IReadOnlyList<string> SensitiveKeys => SensitiveAnswerKeys;

        public static BattleSandboxEnemyCombatFeedbackPreview Build(
            BuildSandboxPreviewContext context,
            MechanicHintFeedbackPreview hintPreview = null,
            BuildTuningDataPanelPreview dataPanel = null)
        {
            BuildSandboxPreviewContext safeContext = context ?? new BuildSandboxPreviewContext();
            BuildProblemSeedDataset dataset =
                safeContext.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();
            EnemyBossValidationPool pool =
                safeContext.enemyBossValidationPool ?? EnemyBossValidationPool.CreateDefault();
            MechanicHintFeedbackPreview safeHintPreview =
                hintPreview ?? MechanicHintFeedbackPreviewBuilder.Build(safeContext, dataPanel: dataPanel);

            BattleSandboxEnemyCombatFeedbackPreview preview = new()
            {
                packageName = BattleSandboxEnemyCombatFeedbackPreview.PackageName,
                sourcePreviewBuildId = safeContext.previewBuildId ?? string.Empty,
                referenceMode = BattleSandboxEnemyCombatFeedbackPreview.ReferenceMode,
                devOnly = true,
                isEnabled = false,
                usesFloatingCombatTextLanguage = true,
                usesEnemyIntentCastBarLanguage = true,
                usesBossInfoLanguage = true,
                playerTopicPanelRemoved = true,
                playerUiChineseOnly = true,
                playerUiShowsFullAnswers = false,
                developerFullAnswersStayInDataPanel = true,
                runsFormalCombat = false,
                callsFormalDamageSettlement = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false
            };

            AddBossStateRows(preview, pool);
            AddBossCastRows(preview, dataset);
            AddWeaknessRows(preview, dataset);
            AddMechanicRows(preview, safeHintPreview);
            AddFailureRows(preview, dataset);
            AddEnemyStateRows(preview, pool);
            AddDeveloperAnswerLinks(preview, dataPanel);
            return preview;
        }

        private static void AddBossStateRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            EnemyBossValidationPool pool)
        {
            int index = 0;
            foreach (BuildSandboxBossProfile boss in pool.bosses ?? new List<BuildSandboxBossProfile>())
            {
                if (boss == null)
                {
                    continue;
                }

                string bossName = ChineseOnly(boss.chineseRole, "首领");
                string phrase = Pick(BossStatePhrases, index);
                AddRow(
                    preview,
                    "bossState." + CleanStableKeySegment(boss.bossId),
                    BattleSandboxEnemyCombatFeedbackKinds.BossState,
                    bossName,
                    $"{bossName}：{phrase}",
                    "首领正在蓄力",
                    phrase,
                    $"【状态】{phrase}，注意下一次施法。",
                    "BossInfoPanel",
                    "Boss state headline / threat short line",
                    "EnemyBossValidationPool.bosses",
                    "bossSixKeyFullAnswer",
                    usesCastBar: false,
                    usesBossInfo: true,
                    castDurationSeconds: 2.6f + index % 3 * 0.4f);
                index++;
            }
        }

        private static void AddBossCastRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            int index = 0;
            foreach (BossProblemSeed boss in dataset.bossProblems ?? new List<BossProblemSeed>())
            {
                if (boss == null)
                {
                    continue;
                }

                string bossName = ChineseOnly(boss.displayName, "首领");
                string skill = Pick(BossCastNames, index);
                AddRow(
                    preview,
                    "bossCast." + CleanStableKeySegment(boss.bossProblemId),
                    BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                    bossName,
                    $"{bossName}：正在准备技能",
                    $"施法中：{skill}",
                    "施法预兆",
                    $"【施法】{skill} 即将落下。",
                    "V02EnemyIntentUI",
                    "Enemy intent title + remaining cast timer",
                    "BuildProblemSeedDataset.bossProblems",
                    "bossSixKeyFullAnswer",
                    usesCastBar: true,
                    usesBossInfo: true,
                    castDurationSeconds: 2.2f + index % 4 * 0.45f);
                index++;
            }
        }

        private static void AddWeaknessRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            int index = 0;
            foreach (BossProblemSeed boss in dataset.bossProblems ?? new List<BossProblemSeed>())
            {
                string bossName = ChineseOnly(boss?.displayName, "首领");
                foreach (WeaknessWindowSeed window in boss?.weaknessWindows ?? new List<WeaknessWindowSeed>())
                {
                    if (window == null)
                    {
                        continue;
                    }

                    string phrase = Pick(WeaknessPhrases, index);
                    AddRow(
                        preview,
                        "weakness." + CleanStableKeySegment(window.weaknessWindowId),
                        BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                        bossName,
                        $"{bossName}：{phrase}",
                        "破绽窗口",
                        phrase,
                        $"【破绽】{phrase}，抓住短暂节奏。",
                        "FloatingCombatText",
                        "Short floating combat feedback rhythm",
                        "BuildProblemSeedDataset.bossProblems.weaknessWindows",
                        "bossSixKeyFullAnswer",
                        usesCastBar: false,
                        usesBossInfo: false,
                        castDurationSeconds: 1.8f);
                    index++;
                }
            }
        }

        private static void AddMechanicRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            MechanicHintFeedbackPreview hintPreview)
        {
            int index = 0;
            foreach (MechanicHintFeedbackPreviewRow hint in hintPreview?.playerRows ?? new List<MechanicHintFeedbackPreviewRow>())
            {
                if (hint == null
                    || !hint.playerVisible
                    || hint.category == "dropBiasAtmosphere")
                {
                    continue;
                }

                if (hint.category != "mapMechanic"
                    && hint.category != "enemyMechanic"
                    && hint.category != "bossSkill")
                {
                    continue;
                }

                string phrase = Pick(MechanicPhrases, index);
                AddRow(
                    preview,
                    "mechanic." + CleanStableKeySegment(hint.englishStableKey),
                    BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                    "首领",
                    $"机制反馈：{phrase}",
                    "机制变化",
                    phrase,
                    $"【机制】{phrase}。",
                    "BattleHint / CombatLog",
                    "Battle hint lane + short combat log phrase",
                    hint.sourceDataPath,
                    string.IsNullOrWhiteSpace(hint.developerDataPanelFieldKey)
                        ? "hardSolutionTags"
                        : hint.developerDataPanelFieldKey,
                    usesCastBar: false,
                    usesBossInfo: false,
                    castDurationSeconds: 2.4f);
                index++;
            }
        }

        private static void AddFailureRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            int index = 0;
            foreach (FailureHintSeed hint in dataset.failureHints ?? new List<FailureHintSeed>())
            {
                if (hint == null)
                {
                    continue;
                }

                string phrase = Pick(FailurePhrases, index);
                AddRow(
                    preview,
                    "failure." + CleanStableKeySegment(hint.failureHintId),
                    BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                    "首领",
                    $"反馈：{phrase}",
                    "压力反馈",
                    phrase,
                    $"【反馈】{phrase}，回整备再补一手。",
                    "BattleHint",
                    "Failed placement feedback tone",
                    "BuildProblemSeedDataset.failureHints",
                    "requiredStats",
                    usesCastBar: false,
                    usesBossInfo: false,
                    castDurationSeconds: 2f);
                index++;
            }
        }

        private static void AddEnemyStateRows(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            EnemyBossValidationPool pool)
        {
            int index = 0;
            foreach (BuildSandboxEnemyProfile enemy in pool.enemies ?? new List<BuildSandboxEnemyProfile>())
            {
                if (enemy == null)
                {
                    continue;
                }

                string enemyName = ChineseOnly(enemy.chineseRole, "敌人");
                string phrase = Pick(EnemyStatePhrases, index);
                AddRow(
                    preview,
                    "enemyState." + CleanStableKeySegment(enemy.enemyId),
                    BattleSandboxEnemyCombatFeedbackKinds.EnemyState,
                    enemyName,
                    $"{enemyName}：{phrase}",
                    "敌人意图",
                    phrase,
                    $"【敌势】{phrase}。",
                    "V02EnemyIntentUI",
                    "Enemy intent short state",
                    "EnemyBossValidationPool.enemies",
                    "hardSolutionTags",
                    usesCastBar: true,
                    usesBossInfo: false,
                    castDurationSeconds: 1.8f + index % 3 * 0.35f);
                index++;
            }
        }

        private static void AddDeveloperAnswerLinks(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            foreach (string key in SensitiveAnswerKeys)
            {
                BuildTuningDataPanelFieldRow row = dataPanel?.Rows?
                    .FirstOrDefault(field => string.Equals(field.englishStableKey, key, StringComparison.Ordinal));
                preview.developerAnswerLinks.Add(new BattleSandboxCombatFeedbackDeveloperLink
                {
                    englishStableKey = key,
                    chineseDisplayName = row?.chineseDisplayName ?? SensitiveChineseName(key),
                    sourceDataPath = row?.sourceDataPath ?? SensitiveSourcePath(key),
                    dataPanelSlot = "EnemyCombatFeedbackDeveloperPanel",
                    developerVisible = true,
                    playerVisible = false,
                    maskedFromPlayer = true
                });
            }
        }

        private static void AddRow(
            BattleSandboxEnemyCombatFeedbackPreview preview,
            string feedbackId,
            string feedbackKind,
            string bossName,
            string stateLine,
            string castSkillLine,
            string floatingText,
            string combatLogLine,
            string reuseSource,
            string reusePattern,
            string sourcePath,
            string developerKey,
            bool usesCastBar,
            bool usesBossInfo,
            float castDurationSeconds)
        {
            preview.rows.Add(new BattleSandboxEnemyCombatFeedbackRow
            {
                feedbackId = feedbackId ?? string.Empty,
                feedbackKind = feedbackKind ?? string.Empty,
                bossDisplayNameChinese = ChineseOnly(bossName, "首领"),
                stateLineChinese = SanitizePlayerText(stateLine, "首领状态变化"),
                castSkillLineChinese = SanitizePlayerText(castSkillLine, "施法预兆"),
                floatingTextChinese = SanitizePlayerText(floatingText, "机制反馈"),
                combatLogLineChinese = SanitizePlayerText(combatLogLine, "战斗反馈出现"),
                reuseSourceComponent = reuseSource ?? string.Empty,
                reuseLanguagePattern = reusePattern ?? string.Empty,
                sourceDataPath = sourcePath ?? string.Empty,
                developerDataPanelFieldKey = string.IsNullOrWhiteSpace(developerKey) ? "hardSolutionTags" : developerKey,
                castDurationSeconds = Math.Max(0.6f, castDurationSeconds),
                devOnly = true,
                isEnabled = false,
                playerVisible = true,
                developerPanelVisible = true,
                playerShowsCompleteAnswer = false,
                usesFloatingCombatTextLanguage = true,
                usesEnemyCastBarLanguage = usesCastBar,
                usesBossInfoLanguage = usesBossInfo,
                runsFormalCombat = false,
                callsFormalDamageSettlement = false,
                writesFormalFlow = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                opensFeatureFlag = false
            });
        }

        private static string Pick(IReadOnlyList<string> values, int index)
        {
            if (values == null || values.Count == 0)
            {
                return "战斗反馈";
            }

            return values[Math.Abs(index) % values.Count];
        }

        private static string ChineseOnly(string value, string fallback)
        {
            string trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            return string.IsNullOrWhiteSpace(trimmed) || ContainsLatin(trimmed)
                ? fallback
                : trimmed;
        }

        private static string SanitizePlayerText(string value, string fallback)
        {
            string text = ChineseOnly(value, fallback);
            if (ContainsForbiddenPlayerToken(text))
            {
                return fallback;
            }

            return text;
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
                "requiredSynergy",
                "requiredAffix",
                "requiredStats",
                "DropBias",
                "dropBias",
                "Boss",
                "keyRequirements",
                "previewWeight",
                "BuildProblemSeedDataset",
                "EnemyBossValidationPool",
                "答案",
                "解法",
                "权重",
                "六钥匙",
                "题目",
                "准备度"
            };
            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
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

        private static string SensitiveChineseName(string key)
        {
            return key switch
            {
                "hardSolutionTags" => "硬解标签",
                "requiredSynergy" => "需求羁绊",
                "requiredAffix" => "需求词条",
                "requiredStats" => "需求数值",
                "dropBiasWeights" => "掉落倾向权重",
                "bossSixKeyFullAnswer" => "首领六钥匙完整答案",
                _ => "开发者字段"
            };
        }

        private static string SensitiveSourcePath(string key)
        {
            return key switch
            {
                "dropBiasWeights" => "BuildProblemSeedDataset.dropBiases",
                "bossSixKeyFullAnswer" => "BuildProblemSeedDataset.bossProblems",
                "hardSolutionTags" => "BuildProblemSeedDataset.enemyProblems",
                "requiredSynergy" => "BuildProblemSeedDataset.bossProblems",
                "requiredAffix" => "BuildProblemSeedDataset.bossProblems",
                "requiredStats" => "BuildProblemSeedDataset.failureHints",
                _ => "BuildProblemSeedDataset"
            };
        }
    }
}
