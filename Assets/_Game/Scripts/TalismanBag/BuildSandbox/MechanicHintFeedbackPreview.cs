using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class MechanicHintFeedbackPreview
    {
        public const string PackageName = "V0.4-MechanicHintFeedbackPreview01";
        public const string ReferenceMode = "PlayerHintAdapterNoFormalUiWrite";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = string.Empty;
        public string referenceMode = ReferenceMode;
        public bool devOnly = true;
        public bool isEnabled;
        public bool writesFormalUi;
        public bool writesFormalScene;
        public bool changesHandTunedLayout;
        public bool createsMechanicUiFrame;
        public bool createsDedicatedMechanicPanels;
        public bool touchesRunFlow;
        public bool touchesPageState;
        public bool touchesFormationState;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsEnglishStableKey;
        public bool playerUiShowsFullAnswers;
        public bool developerFullAnswersStayInDataPanel = true;
        public BattleFeedbackMechanicHintExtensionViewModel feedbackExtension = new();
        public List<MechanicHintFeedbackPreviewRow> playerRows = new();
        public List<MechanicHintDeveloperDataPanelLink> developerDataPanelLinks = new();

        public int PlayerVisibleRowCount => playerRows?.Count(row => row != null && row.playerVisible) ?? 0;
        public int MaskedPlayerRowCount => playerRows?.Count(row => row != null && row.maskedFromPlayer) ?? 0;
        public int DeveloperLinkCount => developerDataPanelLinks?.Count ?? 0;
        public int ReuseChannelCount => playerRows?
            .Where(row => row != null && !string.IsNullOrWhiteSpace(row.reuseChannel))
            .Select(row => row.reuseChannel)
            .Distinct(StringComparer.Ordinal)
            .Count() ?? 0;
    }

    [Serializable]
    public sealed class MechanicHintFeedbackPreviewRow
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string category = string.Empty;
        public string reuseChannel = string.Empty;
        public string reuseSurface = string.Empty;
        public string playerChineseText = string.Empty;
        public string atmosphereChinese = string.Empty;
        public string sourceDataPath = string.Empty;
        public string developerDataPanelFieldKey = string.Empty;
        public bool playerVisible = true;
        public bool maskedFromPlayer;
        public bool developerPanelVisible = true;
        public bool canWriteFormalUi;
        public bool createsDedicatedPanel;
        public bool exposesFullAnswerToPlayer;
    }

    [Serializable]
    public sealed class MechanicHintDeveloperDataPanelLink
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string dataPanelSlot = "ProblemReadinessPanelSlot";
        public string sourceDataPath = string.Empty;
        public bool developerVisible = true;
        public bool playerVisible;
        public bool maskedFromPlayer = true;
    }

    public static class MechanicHintFeedbackPreviewBuilder
    {
        private static readonly string[] SensitiveDeveloperKeys =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBiasWeights",
            "bossSixKeyFullAnswer"
        };

        public static MechanicHintFeedbackPreview Build(
            BuildSandboxPreviewContext context,
            BattlePageViewSpec spec = null,
            BuildTuningDataPanelPreview dataPanel = null)
        {
            BuildSandboxPreviewContext safeContext = context ?? new BuildSandboxPreviewContext();
            BuildProblemSeedDataset dataset =
                safeContext.problemSeedDataset ?? BuildProblemSeedDataset.CreateDefault();

            MechanicHintFeedbackPreview preview = new()
            {
                packageName = MechanicHintFeedbackPreview.PackageName,
                sourcePreviewBuildId = safeContext.previewBuildId ?? string.Empty,
                referenceMode = MechanicHintFeedbackPreview.ReferenceMode,
                devOnly = true,
                isEnabled = false,
                writesFormalUi = false,
                writesFormalScene = false,
                changesHandTunedLayout = false,
                createsMechanicUiFrame = false,
                createsDedicatedMechanicPanels = false,
                touchesRunFlow = false,
                touchesPageState = false,
                touchesFormationState = false,
                touchesSaveData = false,
                touchesBossRewardDropNumeric = false,
                playerUiChineseOnly = true,
                playerUiShowsEnglishStableKey = false,
                playerUiShowsFullAnswers = false,
                developerFullAnswersStayInDataPanel = true
            };

            AddMapMechanicRows(preview, dataset);
            AddEnemyMechanicRows(preview, dataset);
            AddBossSkillRows(preview, dataset);
            AddFailureFeedbackRows(preview, dataset);
            AddWeaknessWindowRows(preview, dataset);
            AddDropBiasAtmosphereRows(preview, dataset);
            AddDeveloperDataPanelLinks(preview, dataPanel);
            preview.feedbackExtension = BuildFeedbackExtension(preview);
            return preview;
        }

        public static IReadOnlyList<string> SensitiveKeys => SensitiveDeveloperKeys;

        private static void AddMapMechanicRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (MapRuleSeed rule in dataset.mapRules ?? new List<MapRuleSeed>())
            {
                if (rule == null)
                {
                    continue;
                }

                string mapName = ChineseOnly(rule.displayName, "地图异象");
                string pressure = DescribeCapabilities((rule.affectedTags ?? new List<string>())
                    .Concat(rule.debuffTags ?? new List<string>()));
                AddRow(
                    preview,
                    $"mapMechanic.{CleanStableKeySegment(rule.mapRuleId)}",
                    "地图机制线索",
                    "mapMechanic",
                    "BattleHint",
                    "PlacementFeedback / BattleHint lane",
                    $"【{mapName}】压上阵面时，场上会偏向{pressure}。先看棋盘气息和供能变化，再决定是否换位。",
                    "灯火、地气和阵眼变化只做氛围提示，不显示解法清单。",
                    "BuildProblemSeedDataset.mapRules",
                    "hardSolutionTags");
            }
        }

        private static void AddEnemyMechanicRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (EnemyProblemSeed problem in dataset.enemyProblems ?? new List<EnemyProblemSeed>())
            {
                if (problem == null)
                {
                    continue;
                }

                string enemyName = ChineseOnly(problem.displayName, "敌人");
                string pressure = DescribePressure(problem.pressureType);
                string hint = MaskSolutionTerms(problem.failureHint?.detail);
                if (string.IsNullOrWhiteSpace(hint))
                {
                    hint = $"这类敌人会把{pressure}放大，战斗提示只提醒短板方向。";
                }

                AddRow(
                    preview,
                    $"enemyMechanic.{CleanStableKeySegment(problem.problemType)}",
                    "敌人机制线索",
                    "enemyMechanic",
                    "CombatLog",
                    "CombatLog / Tooltip language",
                    $"【{enemyName}】会制造{pressure}。{hint}",
                    "敌人压力通过战斗日志和状态提示露出，不展示硬解标签。",
                    "BuildProblemSeedDataset.enemyProblems",
                    "hardSolutionTags");
            }
        }

        private static void AddBossSkillRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (BossProblemSeed boss in dataset.bossProblems ?? new List<BossProblemSeed>())
            {
                if (boss == null)
                {
                    continue;
                }

                string bossName = ChineseOnly(boss.displayName, "首领");
                string pressure = DescribeCapabilities(boss.requiredProblemAttributes);
                AddRow(
                    preview,
                    $"bossSkill.{CleanStableKeySegment(boss.bossProblemId)}",
                    "首领技能线索",
                    "bossSkill",
                    "BossInfo",
                    "BossInfoPanel language",
                    $"【{bossName}】会分段试探{pressure}。首领信息只显示阶段线索和威胁感，不列出完整钥匙。",
                    "首领提示保持 BossInfo 的技能、威胁、应对语气，但答案留在数据面板。",
                    "BuildProblemSeedDataset.bossProblems",
                    "bossSixKeyFullAnswer");
            }
        }

        private static void AddFailureFeedbackRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (FailureHintSeed hint in dataset.failureHints ?? new List<FailureHintSeed>())
            {
                if (hint == null)
                {
                    continue;
                }

                string headline = ChineseOnly(hint.headline, "反馈");
                string detail = MaskSolutionTerms(hint.detail);
                if (string.IsNullOrWhiteSpace(detail))
                {
                    detail = "当前构筑还有短板，回到整备时再补一手。";
                }

                AddRow(
                    preview,
                    $"failureFeedback.{CleanStableKeySegment(hint.failureHintId)}",
                    "失败反馈",
                    "failureFeedback",
                    "BattleHint",
                    "BattleHint / failed placement feedback lane",
                    $"【{headline}】{detail}",
                    "失败反馈只告诉玩家缺口方向和下一步感受，不显示配置答案。",
                    "BuildProblemSeedDataset.failureHints",
                    "requiredStats");
            }
        }

        private static void AddWeaknessWindowRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (BossProblemSeed boss in dataset.bossProblems ?? new List<BossProblemSeed>())
            {
                string bossName = ChineseOnly(boss?.displayName, "首领");
                foreach (WeaknessWindowSeed window in boss?.weaknessWindows ?? new List<WeaknessWindowSeed>())
                {
                    if (window == null)
                    {
                        continue;
                    }

                    AddRow(
                        preview,
                        $"weaknessWindow.{CleanStableKeySegment(window.weaknessWindowId)}",
                        "弱点窗口反馈",
                        "weaknessWindow",
                        "DamageFeedback",
                        "Damage feedback / floating combat text rhythm",
                        $"【{bossName}】露出破绽时，战斗反馈只提示窗口已开和该抓时机，不显示触发公式。",
                        "弱点窗口用伤害反馈的短促节奏表现。",
                        "BuildProblemSeedDataset.bossProblems.weaknessWindows",
                        "bossSixKeyFullAnswer");
                }
            }
        }

        private static void AddDropBiasAtmosphereRows(
            MechanicHintFeedbackPreview preview,
            BuildProblemSeedDataset dataset)
        {
            foreach (DropBiasSeed drop in dataset.dropBiases ?? new List<DropBiasSeed>())
            {
                if (drop == null)
                {
                    continue;
                }

                string displayName = ChineseOnly(drop.displayName, "战利品气息");
                AddRow(
                    preview,
                    $"dropBiasAtmosphere.{CleanStableKeySegment(drop.dropBiasId)}",
                    "掉落氛围提示",
                    "dropBiasAtmosphere",
                    "Tooltip",
                    "Reward / Tooltip atmosphere language",
                    $"【{displayName}】战后物件的气息会偏向当前短板，玩家只看到氛围提示。",
                    "掉落倾向只表现为氛围，不显示权重、道具清单或词条清单。",
                    "BuildProblemSeedDataset.dropBiases",
                    "dropBiasWeights");
            }
        }

        private static void AddDeveloperDataPanelLinks(
            MechanicHintFeedbackPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            foreach (string key in SensitiveDeveloperKeys)
            {
                BuildTuningDataPanelFieldRow row = dataPanel?.Rows?
                    .FirstOrDefault(field => string.Equals(field.englishStableKey, key, StringComparison.Ordinal));

                preview.developerDataPanelLinks.Add(new MechanicHintDeveloperDataPanelLink
                {
                    englishStableKey = key,
                    chineseDisplayName = row?.chineseDisplayName ?? ToSensitiveChineseName(key),
                    dataPanelSlot = row?.dataPanelSlot ?? "ProblemReadinessPanelSlot",
                    sourceDataPath = row?.sourceDataPath ?? ResolveSensitiveSourcePath(key),
                    developerVisible = true,
                    playerVisible = false,
                    maskedFromPlayer = true
                });
            }
        }

        private static BattleFeedbackMechanicHintExtensionViewModel BuildFeedbackExtension(
            MechanicHintFeedbackPreview preview)
        {
            BattleFeedbackMechanicHintExtensionViewModel model = new()
            {
                extensionId = "BattleFeedbackMechanicHintExtension",
                targetComponentName = "BattleHint / Tooltip / BossInfo / CombatLog",
                targetNodeName = "Existing battle feedback language",
                adapterOutputKey = "mechanicHintFeedbackPreview",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true
            };

            foreach (MechanicHintFeedbackPreviewRow row in preview.playerRows ?? new List<MechanicHintFeedbackPreviewRow>())
            {
                if (row == null || !row.playerVisible)
                {
                    continue;
                }

                model.rows.Add(new BattleFeedbackMechanicHintRow
                {
                    englishStableKey = row.englishStableKey,
                    chineseDisplayName = row.chineseDisplayName,
                    playerChineseText = row.playerChineseText,
                    playerVisible = true,
                    maskedFromPlayer = false,
                    developerPanelVisible = true,
                    canWriteFormalUi = false
                });
            }

            return model;
        }

        private static void AddRow(
            MechanicHintFeedbackPreview preview,
            string englishStableKey,
            string chineseDisplayName,
            string category,
            string reuseChannel,
            string reuseSurface,
            string playerChineseText,
            string atmosphereChinese,
            string sourceDataPath,
            string developerDataPanelFieldKey)
        {
            preview.playerRows.Add(new MechanicHintFeedbackPreviewRow
            {
                englishStableKey = englishStableKey ?? string.Empty,
                chineseDisplayName = chineseDisplayName ?? string.Empty,
                category = category ?? string.Empty,
                reuseChannel = reuseChannel ?? string.Empty,
                reuseSurface = reuseSurface ?? string.Empty,
                playerChineseText = PlayerText(playerChineseText),
                atmosphereChinese = PlayerText(atmosphereChinese),
                sourceDataPath = sourceDataPath ?? string.Empty,
                developerDataPanelFieldKey = developerDataPanelFieldKey ?? string.Empty,
                playerVisible = true,
                maskedFromPlayer = false,
                developerPanelVisible = true,
                canWriteFormalUi = false,
                createsDedicatedPanel = false,
                exposesFullAnswerToPlayer = false
            });
        }

        private static string PlayerText(string value)
        {
            return StripAsciiLetters(MaskSolutionTerms(value));
        }

        private static string MaskSolutionTerms(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string text = value;
            Dictionary<string, string> replacements = new(StringComparer.OrdinalIgnoreCase)
            {
                ["hardSolutionTags"] = "构筑答案",
                ["requiredSynergy"] = "协同要求",
                ["requiredAffix"] = "词条要求",
                ["requiredStats"] = "属性要求",
                ["DropBias"] = "掉落倾向",
                ["Boss"] = "首领",
                ["Build"] = "构筑",
                ["BreakPower"] = "破盾能力",
                ["ClearPower"] = "清场能力",
                ["CleansePower"] = "净厄能力",
                ["EnergyStability"] = "供能稳定",
                ["ControlPower"] = "镇魂打断",
                ["GuardPower"] = "护阵承伤",
                ["BurstWindow"] = "短窗爆发",
                ["PlacementShape"] = "摆放形态",
                ["ThunderChain"] = "惊雷连锁",
                ["DebuffCounter"] = "负面反制",
                ["SpiritLock"] = "镇灵锁定",
                ["InterruptTiming"] = "打断时机",
                ["SustainedDamage"] = "持续输出",
                ["PollutedTile"] = "污染格",
                ["ChainReaction"] = "连锁反应",
                ["CooldownRecovery"] = "冷却回转",
                ["ShieldLayered"] = "层叠护盾",
                ["WindowShort"] = "短窗口",
                ["GuardDrain"] = "护阵消耗",
                ["AnchorShift"] = "阵眼偏移",
                ["EnergyLeak"] = "供能外泄",
                ["PaperDamp"] = "符纸受潮"
            };

            foreach (KeyValuePair<string, string> pair in replacements)
            {
                text = ReplaceOrdinalIgnoreCase(text, pair.Key, pair.Value);
            }

            return NormalizeSpaces(text);
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

        private static string StripAsciiLetters(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            char[] chars = value
                .Where(character => !(character <= 127 && char.IsLetter(character)))
                .ToArray();
            return NormalizeSpaces(new string(chars));
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
                .Trim();
        }

        private static string ChineseOnly(string value, string fallback)
        {
            string masked = PlayerText(value);
            return string.IsNullOrWhiteSpace(masked) ? fallback : masked;
        }

        private static string DescribeCapabilities(IEnumerable<string> values)
        {
            List<string> labels = (values ?? Enumerable.Empty<string>())
                .Select(ToCapabilityChinese)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Take(3)
                .ToList();
            return labels.Count == 0 ? "阵面变化" : string.Join("、", labels);
        }

        private static string DescribePressure(string pressureType)
        {
            string lower = pressureType?.ToLowerInvariant() ?? string.Empty;
            if (lower.Contains("shield")) return "护盾压力";
            if (lower.Contains("swarm")) return "群怪压力";
            if (lower.Contains("debuff") || lower.Contains("dot")) return "污染压力";
            if (lower.Contains("energy")) return "供能压力";
            if (lower.Contains("seal")) return "封锁压力";
            if (lower.Contains("burst")) return "爆发压力";
            if (lower.Contains("cast")) return "施法压力";
            if (lower.Contains("health")) return "长线压力";
            if (lower.Contains("tile")) return "格位压力";
            if (lower.Contains("formation")) return "阵眼压力";
            return "构筑压力";
        }

        private static string ToCapabilityChinese(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string lower = value.ToLowerInvariant();
            if (ContainsAny(lower, "break", "shield", "thunder", "jing_lei")) return "破盾";
            if (ContainsAny(lower, "clear", "fire", "lihuo", "li_huo")) return "清场";
            if (ContainsAny(lower, "cleanse", "purify", "jing_e", "polluted")) return "净厄";
            if (ContainsAny(lower, "energy", "ju_neng", "powered")) return "供能";
            if (ContainsAny(lower, "control", "interrupt", "zhen_hun", "seal")) return "镇魂";
            if (ContainsAny(lower, "guard", "ward", "hu_zhen")) return "护阵";
            if (ContainsAny(lower, "burst", "window", "cooldown")) return "短窗爆发";
            if (ContainsAny(lower, "shape", "placement", "formation", "anchor")) return "摆放形态";
            if (ContainsAny(lower, "summon", "chain", "echo")) return "连锁清场";
            return string.Empty;
        }

        private static string ToSensitiveChineseName(string key)
        {
            return key switch
            {
                "hardSolutionTags" => "硬解标签",
                "requiredSynergy" => "必需协同",
                "requiredAffix" => "必需词条",
                "requiredStats" => "必需属性",
                "dropBiasWeights" => "掉落倾向权重",
                "bossSixKeyFullAnswer" => "首领六钥匙完整答案",
                _ => "开发者字段"
            };
        }

        private static string ResolveSensitiveSourcePath(string key)
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

        private static string CleanStableKeySegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            char[] chars = value
                .Trim()
                .Select(character => char.IsLetterOrDigit(character) || character == '_' || character == '-'
                    ? character
                    : '_')
                .ToArray();
            return new string(chars);
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
