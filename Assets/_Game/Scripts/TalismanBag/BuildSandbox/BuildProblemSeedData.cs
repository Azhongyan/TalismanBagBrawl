using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildProblemSeedDataset
    {
        public string packageName = "V0.4-BuildProblemSeedData01";
        public bool devOnly = true;
        public bool isEnabled;
        public List<MapRuleSeed> mapRules = new();
        public List<EnemyProblemSeed> enemyProblems = new();
        public List<BossProblemSeed> bossProblems = new();
        public List<WeaknessWindowSeed> weaknessWindows = new();
        public List<DropBiasSeed> dropBiases = new();
        public List<FailureHintSeed> failureHints = new();

        public static BuildProblemSeedDataset CreateDefault()
        {
            BuildProblemSeedDataset dataset = new()
            {
                mapRules = CreateMapRules().ToList(),
                enemyProblems = CreateEnemyProblems().ToList(),
                bossProblems = CreateBossProblems().ToList()
            };
            dataset.weaknessWindows = dataset.bossProblems.SelectMany(boss => boss.weaknessWindows).ToList();
            dataset.dropBiases = dataset.bossProblems.SelectMany(boss => boss.dropBiases).ToList();
            dataset.failureHints = dataset.enemyProblems
                .Select(problem => problem.failureHint)
                .Concat(dataset.bossProblems.SelectMany(boss => boss.failureHints))
                .ToList();
            return dataset;
        }

        public BossProblemSeed FindBoss(string bossProblemId)
        {
            return bossProblems.FirstOrDefault(boss =>
                string.Equals(boss.bossProblemId, bossProblemId, StringComparison.Ordinal));
        }

        private static IEnumerable<MapRuleSeed> CreateMapRules()
        {
            yield return MapRule(
                "dev_map_black_furnace_smoke",
                "黑炉烟尘",
                "黑炉烟尘压低视野并抬高破盾需求，适合检查惊雷与清场 Build 的起手完整度。",
                new[] { "SmokePressure", "BreakPower", "ClearPower" },
                new[] { "ThunderEcho" },
                new[] { "SightReduced", "ShieldLayered" },
                -1,
                -1,
                1,
                "烟尘会放大护壳压力，缺少破盾或清场时会拖入失败循环。",
                new[] { "dev_enemy_shield_problem", "dev_enemy_swarm_problem" },
                new[] { "dev_boss_black_furnace_shell" },
                "drop_bias_black_furnace_shell_01");

            yield return MapRule(
                "dev_map_bluestone_damp",
                "青石回潮",
                "青石巷地面回潮，符纸与阵眼更容易被污染，适合检查净厄与供能稳定。",
                new[] { "CleansePower", "EnergyStability", "PollutedTile" },
                new[] { "WaterTrace" },
                new[] { "TalismanDamp", "EnergyLeak" },
                -1,
                -2,
                1,
                "回潮会持续拉低供能效率，净化不足时污染格会扩大。",
                new[] { "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem" },
                new[] { "dev_boss_dirty_dream_mother" },
                "drop_bias_dirty_dream_mother_01");

            yield return MapRule(
                "dev_map_lamp_shortage",
                "灯火不足",
                "灯火不足让爆发窗口变短，适合检查 BurstWindow 与快速压制能力。",
                new[] { "BurstWindow", "ControlPower", "ClearPower" },
                new[] { "LampFocus" },
                new[] { "WindowShort" },
                0,
                -1,
                2,
                "灯火不足会缩短可输出窗口，慢热 Build 需要补爆发或控制。",
                new[] { "dev_enemy_burst_problem", "dev_enemy_caster_problem" },
                new[] { "dev_boss_paper_faces" },
                "drop_bias_paper_faces_01");

            yield return MapRule(
                "dev_map_copper_bell_night",
                "铜铃夜响",
                "夜里铜铃乱响干扰施法节奏，适合检查镇魂打断与护阵防守。",
                new[] { "ControlPower", "GuardPower", "CasterInterrupt" },
                new[] { "BellResonance" },
                new[] { "CastNoise", "GuardDrain" },
                -1,
                0,
                1,
                "铜铃会打乱施法节奏，缺少镇魂或护阵时容易被连锁施法压制。",
                new[] { "dev_enemy_seal_problem", "dev_enemy_caster_problem" },
                new[] { "dev_boss_bronze_formation_general" },
                "drop_bias_bronze_general_01");

            yield return MapRule(
                "dev_map_formation_eye_shift",
                "阵眼偏移",
                "阵眼坐标偏移，检查摆放形态与供能链是否能保持稳定。",
                new[] { "PlacementShape", "EnergyStability", "GuardPower" },
                new[] { "FormationAnchor" },
                new[] { "AnchorShift", "ShapePenalty" },
                -2,
                -1,
                0,
                "阵眼偏移会惩罚松散摆放，需补形态钥匙或供能链。",
                new[] { "dev_enemy_formation_eye_problem", "dev_enemy_spirit_thief_problem" },
                new[] { "dev_boss_bronze_formation_general" },
                "drop_bias_bronze_general_02");

            yield return MapRule(
                "dev_map_talisman_paper_damp",
                "符纸受潮",
                "符纸受潮削弱持续触发，检查净化、供能与冷却补偿。",
                new[] { "CleansePower", "EnergyStability", "CooldownRecovery" },
                new[] { "DryCharm" },
                new[] { "CooldownSlow", "PaperDamp" },
                0,
                -1,
                2,
                "符纸受潮会让冷却和供能同时吃紧，缺少净化会持续恶化。",
                new[] { "dev_enemy_poison_burn_problem", "dev_enemy_thick_blood_problem" },
                new[] { "dev_boss_dirty_dream_mother" },
                "drop_bias_dirty_dream_mother_02");

            yield return MapRule(
                "dev_map_old_lane_echo",
                "旧巷回音",
                "旧巷回音会复制召唤压力，适合检查清场与离火连锁。",
                new[] { "ClearPower", "BurstWindow", "ChainReaction" },
                new[] { "EchoTrace" },
                new[] { "SummonEcho" },
                -1,
                0,
                1,
                "回音会放大群怪压力，清场不足时会被召唤物拖死。",
                new[] { "dev_enemy_swarm_problem", "dev_enemy_thick_blood_problem" },
                new[] { "dev_boss_paper_faces" },
                "drop_bias_paper_faces_02");

            yield return MapRule(
                "dev_map_furnace_ash_fall",
                "炉灰落阵",
                "炉灰落入阵盘，持续污染关键格，适合检查污染格清理与护阵。",
                new[] { "CleansePower", "GuardPower", "PollutedTile" },
                new[] { "AshWard" },
                new[] { "TilePollution", "GuardPressure" },
                -1,
                -1,
                1,
                "炉灰会污染关键摆放位，净化和护阵缺一都容易失守。",
                new[] { "dev_enemy_polluted_tile_problem", "dev_enemy_formation_eye_problem" },
                new[] { "dev_boss_black_furnace_complex_eye" },
                "drop_bias_complex_eye_01");

            yield return MapRule(
                "dev_map_night_watch_lamp_out",
                "夜巡灯灭",
                "夜巡灯灭后爆发节奏被压缩，检查控制后爆发的衔接。",
                new[] { "BurstWindow", "ControlPower", "BreakPower" },
                new[] { "WatchLamp" },
                new[] { "BlindInterval", "BurstTax" },
                0,
                -1,
                2,
                "灯灭时窗口极短，需要先控制再爆发，否则关键钥匙无法达标。",
                new[] { "dev_enemy_burst_problem", "dev_enemy_seal_problem" },
                new[] { "dev_boss_spirit_thief_core" },
                "drop_bias_spirit_core_01");

            yield return MapRule(
                "dev_map_bluestone_crack",
                "青石裂纹",
                "青石裂纹会切断部分供能路径，适合综合检查摆放、供能与毕业 Build。",
                new[] { "PlacementShape", "EnergyStability", "BurstWindow", "ClearPower" },
                new[] { "StoneLine" },
                new[] { "EnergyCut", "ShapeSplit" },
                -2,
                -2,
                1,
                "裂纹会切断供能链，综合 Build 需要同时补形态、供能和爆发。",
                new[] { "dev_enemy_spirit_thief_problem", "dev_enemy_formation_eye_problem" },
                new[] { "dev_boss_black_furnace_complex_eye" },
                "drop_bias_complex_eye_02");
        }

        private static IEnumerable<EnemyProblemSeed> CreateEnemyProblems()
        {
            yield return EnemyProblem(
                "dev_enemy_shield_problem",
                "护盾",
                "layered_shield",
                "层叠护盾要求稳定破盾。",
                new[] { "BreakPower", "ThunderChain" },
                new[] { "AffixBreakBoost", "GuardStall" },
                new[] { "jing_lei_break", "black_furnace_shell" },
                Hint("hint_enemy_shield_break", "破盾不足", "补惊雷破盾、破甲词条或短窗爆发。"),
                "优先补 BreakPower，并确认关键道具摆放在供能链上。");

            yield return EnemyProblem(
                "dev_enemy_swarm_problem",
                "群怪",
                "swarm_pressure",
                "多目标召唤要求清场和连锁。",
                new[] { "ClearPower", "ChainReaction" },
                new[] { "BurstWindow", "ControlPower" },
                new[] { "li_huo_clear", "paper_faces" },
                Hint("hint_enemy_swarm_clear", "清场不足", "补离火清场、连锁或短冷却词条。"),
                "优先补 ClearPower，并避免单点爆发被召唤物分摊。");

            yield return EnemyProblem(
                "dev_enemy_poison_burn_problem",
                "毒 / 燃",
                "dot_and_debuff",
                "持续污秽和燃烧要求净化或反制。",
                new[] { "CleansePower", "DebuffCounter" },
                new[] { "GuardPower", "EnergyStability" },
                new[] { "jing_e_cleanse", "dirty_dream_mother" },
                Hint("hint_enemy_dot_cleanse", "净化不足", "补净厄、清污词条或护阵承伤。"),
                "优先补 CleansePower，再用护阵拖过污染峰值。");

            yield return EnemyProblem(
                "dev_enemy_spirit_thief_problem",
                "偷灵",
                "energy_drain",
                "偷灵会切断供能节奏。",
                new[] { "EnergyStability", "SpiritLock" },
                new[] { "ControlPower", "PlacementShape" },
                new[] { "ju_neng_energy", "spirit_thief_core" },
                Hint("hint_enemy_energy_stability", "供能不稳", "补聚能、镇魂或缩短关键道具供能链。"),
                "先保护核心供能点，再补反制窗口。");

            yield return EnemyProblem(
                "dev_enemy_seal_problem",
                "封符",
                "seal_lock",
                "封符会压制关键符箓触发。",
                new[] { "ControlPower", "CleansePower" },
                new[] { "BurstWindow", "GuardPower" },
                new[] { "zhen_hun_control", "dirty_dream_mother" },
                Hint("hint_enemy_seal_unlock", "封符未解", "补镇魂打断或净厄解封。"),
                "优先处理封符源头，避免核心符箓被锁到窗口结束。");

            yield return EnemyProblem(
                "dev_enemy_burst_problem",
                "高爆发",
                "burst_spike",
                "短时间高压要求护阵或先手控制。",
                new[] { "GuardPower", "ControlPower" },
                new[] { "BurstWindow", "EnergyStability" },
                new[] { "hu_zhen_guard", "bronze_formation_general" },
                Hint("hint_enemy_burst_guard", "承伤不足", "补护阵、减伤词条或先手镇魂。"),
                "用护阵挡第一波，再用爆发窗口反打。");

            yield return EnemyProblem(
                "dev_enemy_caster_problem",
                "施法",
                "long_cast",
                "读条施法要求打断或沉默。",
                new[] { "ControlPower", "InterruptTiming" },
                new[] { "BurstWindow", "CleansePower" },
                new[] { "zhen_hun_control", "spirit_thief_core" },
                Hint("hint_enemy_caster_interrupt", "打断不足", "补镇魂、控制词条或在施法前压低血线。"),
                "围绕施法读条安排镇魂窗口，不要让控制断档。");

            yield return EnemyProblem(
                "dev_enemy_thick_blood_problem",
                "厚血",
                "high_health",
                "厚血拖长战斗，要求持续输出或破防。",
                new[] { "BreakPower", "SustainedDamage" },
                new[] { "ClearPower", "EnergyStability" },
                new[] { "black_furnace_shell", "li_huo_clear" },
                Hint("hint_enemy_thick_blood", "长线输出不足", "补破防、持续离火或供能稳定。"),
                "优先补持续输出，再看是否需要破防词条。");

            yield return EnemyProblem(
                "dev_enemy_polluted_tile_problem",
                "污染格",
                "polluted_tile",
                "污染格会限制摆放和关键阵眼。",
                new[] { "CleansePower", "PlacementShape" },
                new[] { "GuardPower", "EnergyStability" },
                new[] { "jing_e_cleanse", "complex_eye_exam" },
                Hint("hint_enemy_polluted_tile", "污染格未清", "补净化次数，或调整摆放避开污染位。"),
                "先清关键阵眼附近污染，再补形态稳定。");

            yield return EnemyProblem(
                "dev_enemy_formation_eye_problem",
                "阵眼干扰",
                "formation_eye_jam",
                "阵眼干扰会破坏摆放和供能。",
                new[] { "PlacementShape", "EnergyStability" },
                new[] { "GuardPower", "ControlPower" },
                new[] { "ju_neng_energy", "bronze_formation_general" },
                Hint("hint_enemy_formation_eye", "阵眼失稳", "补供能链、护阵或调整核心符箓形态。"),
                "把核心符箓移回稳定供能区，并用护阵挡住干扰波。");
        }

        private static IEnumerable<BossProblemSeed> CreateBossProblems()
        {
            yield return Boss(
                "dev_boss_black_furnace_shell",
                "黑炉护壳师",
                "验证惊雷破盾 Build",
                new[] { "BreakPower", "BurstWindow" },
                new[]
                {
                    Key("boss_black_shell_synergy", "羁绊钥匙", "jing_lei_break", "BreakPower", 3, "惊雷羁绊需要稳定触发三段破盾。"),
                    Key("boss_black_shell_item", "关键道具钥匙", "thunder_sword", "BreakPower", 2, "至少一个破盾核心道具在供能链上。"),
                    Key("boss_black_shell_affix", "关键词条钥匙", "affix_break_boost", "BurstWindow", 2, "短窗破盾需要破甲或爆发词条。"),
                    Key("boss_black_shell_energy", "供能钥匙", "powered_chain", "EnergyStability", 2, "破盾核心不能断供。")
                },
                Weakness("weak_black_shell_break_3", "连续破三层护壳 -> Boss 虚弱", "layered_shell_break", 12f, 5f, new[] { "BreakPower", "BurstWindow" }),
                new[]
                {
                    Drop("drop_bias_black_furnace_shell_01", "黑炉破壳倾向一", new[] { "BreakPower" }, new[] { "thunder_sword", "shell_cracker" }, new[] { "affix_break_boost" }, 1.2f),
                    Drop("drop_bias_black_furnace_shell_02", "黑炉破壳倾向二", new[] { "BurstWindow" }, new[] { "quick_charm" }, new[] { "affix_burst_window" }, 0.9f),
                    Drop("drop_bias_black_furnace_shell_03", "黑炉破壳倾向三", new[] { "EnergyStability" }, new[] { "energy_anchor" }, new[] { "affix_powered_chain" }, 0.8f)
                },
                new[] { Hint("hint_boss_black_shell", "黑炉护壳未破", "先补 BreakPower，再补短窗爆发和供能稳定。") });

            yield return Boss(
                "dev_boss_dirty_dream_mother",
                "秽签梦母",
                "验证净厄反制 Build",
                new[] { "CleansePower", "ControlPower" },
                new[]
                {
                    Key("boss_dirty_mother_synergy", "羁绊钥匙", "jing_e_cleanse", "CleansePower", 3, "净厄羁绊需要覆盖三次污染峰值。"),
                    Key("boss_dirty_mother_item", "关键道具钥匙", "purifying_seal", "CleansePower", 2, "净化道具要能覆盖核心污染格。"),
                    Key("boss_dirty_mother_attribute", "破题属性钥匙", "cleanse_counter", "CleansePower", 3, "净化次数不足会让本体隐藏。"),
                    Key("boss_dirty_mother_control", "关键词条钥匙", "affix_debuff_counter", "ControlPower", 1, "反制词条可压低污染蔓延速度。")
                },
                Weakness("weak_dirty_mother_cleanse_3", "净化三次污染 -> Boss 本体显露", "cleanse_reveal", 8f, 6f, new[] { "CleansePower", "ControlPower" }),
                new[]
                {
                    Drop("drop_bias_dirty_dream_mother_01", "秽签净化倾向一", new[] { "CleansePower" }, new[] { "purifying_seal" }, new[] { "affix_purifying_seal" }, 1.3f),
                    Drop("drop_bias_dirty_dream_mother_02", "秽签反制倾向二", new[] { "ControlPower" }, new[] { "spirit_bell" }, new[] { "affix_debuff_counter" }, 0.9f),
                    Drop("drop_bias_dirty_dream_mother_03", "秽签护阵倾向三", new[] { "GuardPower" }, new[] { "ward_plate" }, new[] { "affix_guardian_ward" }, 0.7f)
                },
                new[] { Hint("hint_boss_dirty_mother", "净厄反制不足", "补 CleansePower，并用控制或护阵拖过污染扩散。") });

            yield return Boss(
                "dev_boss_spirit_thief_core",
                "偷灵炉心",
                "验证聚能 / 镇魂 / 供能稳定 Build",
                new[] { "EnergyStability", "ControlPower", "BurstWindow" },
                new[]
                {
                    Key("boss_spirit_core_synergy", "羁绊钥匙", "ju_neng_energy", "EnergyStability", 3, "聚能羁绊需要在偷灵后恢复供能。"),
                    Key("boss_spirit_core_control", "羁绊钥匙", "zhen_hun_control", "ControlPower", 2, "镇魂需要打断偷灵蓄力。"),
                    Key("boss_spirit_core_placement", "摆放形态钥匙", "compact_power_chain", "EnergyStability", 2, "核心供能链需要紧凑，避免被切断。"),
                    Key("boss_spirit_core_affix", "关键词条钥匙", "affix_powered_chain", "BurstWindow", 1, "反制成功后需要一次全阵爆发。")
                },
                Weakness("weak_spirit_core_interrupt", "镇魂打断蓄力 -> Boss 僵直", "charge_interrupt", 10f, 4f, new[] { "ControlPower", "BurstWindow" }),
                new[]
                {
                    Drop("drop_bias_spirit_core_01", "偷灵供能倾向一", new[] { "EnergyStability" }, new[] { "energy_anchor", "furnace_core" }, new[] { "affix_powered_chain" }, 1.4f),
                    Drop("drop_bias_spirit_core_02", "偷灵镇魂倾向二", new[] { "ControlPower" }, new[] { "spirit_bell" }, new[] { "affix_control_focus" }, 1f),
                    Drop("drop_bias_spirit_core_03", "偷灵爆发倾向三", new[] { "BurstWindow" }, new[] { "quick_charm" }, new[] { "affix_burst_window" }, 0.8f)
                },
                new[] { Hint("hint_boss_spirit_core", "供能链被偷灵切断", "补 EnergyStability，再用镇魂打断蓄力窗口。") });

            yield return Boss(
                "dev_boss_bronze_formation_general",
                "叩阵铜将",
                "验证护阵防守 Build",
                new[] { "GuardPower", "PlacementShape", "EnergyStability" },
                new[]
                {
                    Key("boss_bronze_general_synergy", "羁绊钥匙", "hu_zhen_guard", "GuardPower", 3, "护阵羁绊要挡住连续敲阵。"),
                    Key("boss_bronze_general_item", "关键道具钥匙", "ward_plate", "GuardPower", 2, "护阵核心道具要覆盖阵眼。"),
                    Key("boss_bronze_general_shape", "摆放形态钥匙", "formation_anchor", "PlacementShape", 2, "核心符箓需要守住阵眼附近形态。"),
                    Key("boss_bronze_general_energy", "供能钥匙", "stable_guard_chain", "EnergyStability", 2, "护阵不能因供能断档失效。")
                },
                Weakness("weak_bronze_general_counter", "护阵挡住敲阵 -> Boss 被反震", "guard_counter", 9f, 5f, new[] { "GuardPower", "PlacementShape" }),
                new[]
                {
                    Drop("drop_bias_bronze_general_01", "铜将护阵倾向一", new[] { "GuardPower" }, new[] { "ward_plate" }, new[] { "affix_guardian_ward" }, 1.4f),
                    Drop("drop_bias_bronze_general_02", "铜将形态倾向二", new[] { "PlacementShape" }, new[] { "formation_anchor" }, new[] { "affix_shape_lock" }, 1f),
                    Drop("drop_bias_bronze_general_03", "铜将供能倾向三", new[] { "EnergyStability" }, new[] { "energy_anchor" }, new[] { "affix_powered_chain" }, 0.8f)
                },
                new[] { Hint("hint_boss_bronze_general", "护阵挡不住敲阵", "补 GuardPower，并把核心符箓摆回阵眼保护位。") });

            yield return Boss(
                "dev_boss_paper_faces",
                "纸人千面阵",
                "验证离火 / 清场 / 连锁 Build",
                new[] { "ClearPower", "BurstWindow", "ControlPower" },
                new[]
                {
                    Key("boss_paper_faces_synergy", "羁绊钥匙", "li_huo_clear", "ClearPower", 3, "离火羁绊需要能清掉多波纸人。"),
                    Key("boss_paper_faces_item", "关键道具钥匙", "fire_talisman", "ClearPower", 2, "清场核心道具要覆盖召唤物。"),
                    Key("boss_paper_faces_affix", "关键词条钥匙", "affix_chain_reaction", "BurstWindow", 2, "连锁词条决定能否在窗口内清场。"),
                    Key("boss_paper_faces_control", "破题属性钥匙", "summon_control", "ControlPower", 1, "控制可以压低纸人复制频率。")
                },
                Weakness("weak_paper_faces_clear_all", "清掉全部召唤物 -> Boss 核心暴露", "summon_clear_reveal", 7f, 5f, new[] { "ClearPower", "BurstWindow" }),
                new[]
                {
                    Drop("drop_bias_paper_faces_01", "纸人清场倾向一", new[] { "ClearPower" }, new[] { "fire_talisman" }, new[] { "affix_lihuo_spark" }, 1.5f),
                    Drop("drop_bias_paper_faces_02", "纸人连锁倾向二", new[] { "BurstWindow" }, new[] { "chain_charm" }, new[] { "affix_chain_reaction" }, 1f),
                    Drop("drop_bias_paper_faces_03", "纸人控制倾向三", new[] { "ControlPower" }, new[] { "spirit_bell" }, new[] { "affix_control_focus" }, 0.7f)
                },
                new[] { Hint("hint_boss_paper_faces", "召唤物清不完", "补 ClearPower 和连锁词条，再用控制压住复制节奏。") });

            yield return Boss(
                "dev_boss_black_furnace_complex_eye",
                "黑炉复合阵眼",
                "验证综合 Build 毕业考试",
                new[] { "BreakPower", "CleansePower", "ControlPower", "GuardPower", "EnergyStability", "ClearPower", "BurstWindow" },
                new[]
                {
                    Key("boss_complex_eye_synergy", "羁绊钥匙", "mixed_build_core", "BreakPower", 3, "综合 Build 至少要有一个主破题羁绊。"),
                    Key("boss_complex_eye_item", "关键道具钥匙", "core_talisman_set", "ClearPower", 3, "核心道具组要同时覆盖清场和破盾。"),
                    Key("boss_complex_eye_affix", "关键词条钥匙", "affix_orange_core", "BurstWindow", 2, "橙色核心词条提供毕业爆发窗口。"),
                    Key("boss_complex_eye_shape", "摆放形态钥匙", "stable_compact_shape", "GuardPower", 2, "摆放形态要兼顾护阵与输出。"),
                    Key("boss_complex_eye_energy", "供能钥匙", "full_power_loop", "EnergyStability", 3, "完整供能回路是毕业检查底线。")
                },
                Weakness("weak_complex_eye_counter_loop", "供能链反制成功 -> 玩家全阵爆发一次", "full_loop_counter", 15f, 6f, new[] { "EnergyStability", "BurstWindow", "ClearPower" }),
                new[]
                {
                    Drop("drop_bias_complex_eye_01", "复合阵眼毕业倾向一", new[] { "EnergyStability", "GuardPower" }, new[] { "energy_anchor", "ward_plate" }, new[] { "affix_orange_core" }, 1.2f),
                    Drop("drop_bias_complex_eye_02", "复合阵眼毕业倾向二", new[] { "BreakPower", "ClearPower" }, new[] { "thunder_sword", "fire_talisman" }, new[] { "affix_break_boost", "affix_lihuo_spark" }, 1.1f),
                    Drop("drop_bias_complex_eye_03", "复合阵眼毕业倾向三", new[] { "CleansePower", "ControlPower", "BurstWindow" }, new[] { "purifying_seal", "spirit_bell", "quick_charm" }, new[] { "affix_purifying_seal", "affix_control_focus", "affix_burst_window" }, 1f)
                },
                new[]
                {
                    Hint("hint_boss_complex_eye", "综合钥匙缺口", "毕业检查需要同时覆盖破盾、净化、控制、护阵、供能、清场和爆发。"),
                    Hint("hint_boss_complex_eye_energy", "毕业供能不稳", "先补完整供能回路，再补其他短板。")
                });
        }

        private static MapRuleSeed MapRule(
            string mapRuleId,
            string displayName,
            string description,
            IEnumerable<string> affectedTags,
            IEnumerable<string> buffTags,
            IEnumerable<string> debuffTags,
            int placementModifier,
            int energyModifier,
            int cooldownModifier,
            string warningText,
            IEnumerable<string> enemyProblemIds,
            IEnumerable<string> bossProblemIds,
            string dropBiasId)
        {
            return new MapRuleSeed
            {
                mapRuleId = mapRuleId,
                displayName = displayName,
                description = description,
                affectedTags = affectedTags.ToList(),
                buffTags = buffTags.ToList(),
                debuffTags = debuffTags.ToList(),
                placementModifier = placementModifier,
                energyModifier = energyModifier,
                cooldownModifier = cooldownModifier,
                warningText = warningText,
                enemyProblemIds = enemyProblemIds.ToList(),
                bossProblemIds = bossProblemIds.ToList(),
                dropBiasId = dropBiasId
            };
        }

        private static EnemyProblemSeed EnemyProblem(
            string problemType,
            string displayName,
            string pressureType,
            string problemSummary,
            IEnumerable<string> hardSolutionTags,
            IEnumerable<string> softSolutionTags,
            IEnumerable<string> validatedBuildTags,
            FailureHintSeed failureHint,
            string recommendedAction)
        {
            return new EnemyProblemSeed
            {
                problemType = problemType,
                displayName = displayName,
                pressureType = pressureType,
                problemSummary = problemSummary,
                hardSolutionTags = hardSolutionTags.ToList(),
                softSolutionTags = softSolutionTags.ToList(),
                validatedBuildTags = validatedBuildTags.ToList(),
                failureHint = failureHint,
                recommendedAction = recommendedAction
            };
        }

        private static BossProblemSeed Boss(
            string bossProblemId,
            string displayName,
            string validationGoal,
            IEnumerable<string> requiredProblemAttributes,
            IEnumerable<BossProblemKeySeed> keys,
            WeaknessWindowSeed weakness,
            IEnumerable<DropBiasSeed> dropBiases,
            IEnumerable<FailureHintSeed> failureHints)
        {
            List<BossProblemKeySeed> keyList = keys.ToList();
            return new BossProblemSeed
            {
                bossProblemId = bossProblemId,
                displayName = displayName,
                validationGoal = validationGoal,
                requiredProblemAttributes = requiredProblemAttributes.ToList(),
                keyRequirements = keyList,
                minimumKeysRequired = Math.Min(3, keyList.Count),
                weaknessWindows = new List<WeaknessWindowSeed> { weakness },
                dropBiases = dropBiases.ToList(),
                failureHints = failureHints.ToList()
            };
        }

        private static BossProblemKeySeed Key(
            string keyId,
            string keyCategory,
            string requirementId,
            string problemAttribute,
            int requiredScore,
            string hint)
        {
            return new BossProblemKeySeed
            {
                keyId = keyId,
                keyCategory = keyCategory,
                requirementId = requirementId,
                problemAttribute = problemAttribute,
                requiredScore = requiredScore,
                hint = hint
            };
        }

        private static WeaknessWindowSeed Weakness(
            string weaknessWindowId,
            string displayName,
            string triggerCondition,
            float startSecond,
            float durationSecond,
            IEnumerable<string> exposedBuildTags)
        {
            return new WeaknessWindowSeed
            {
                weaknessWindowId = weaknessWindowId,
                displayName = displayName,
                triggerCondition = triggerCondition,
                startSecond = startSecond,
                durationSecond = durationSecond,
                exposedBuildTags = exposedBuildTags.ToList()
            };
        }

        private static DropBiasSeed Drop(
            string dropBiasId,
            string displayName,
            IEnumerable<string> targetBuildTags,
            IEnumerable<string> targetItemTags,
            IEnumerable<string> targetAffixIds,
            float previewWeight)
        {
            return new DropBiasSeed
            {
                dropBiasId = dropBiasId,
                displayName = displayName,
                targetBuildTags = targetBuildTags.ToList(),
                targetItemTags = targetItemTags.ToList(),
                targetAffixIds = targetAffixIds.ToList(),
                previewWeight = previewWeight
            };
        }

        private static FailureHintSeed Hint(string failureHintId, string headline, string detail)
        {
            return new FailureHintSeed
            {
                failureHintId = failureHintId,
                headline = headline,
                detail = detail
            };
        }
    }

    [Serializable]
    public sealed class MapRuleSeed
    {
        public string mapRuleId = string.Empty;
        public string displayName = string.Empty;
        public string description = string.Empty;
        public List<string> affectedTags = new();
        public List<string> buffTags = new();
        public List<string> debuffTags = new();
        public int placementModifier;
        public int energyModifier;
        public int cooldownModifier;
        public string warningText = string.Empty;
        public List<string> enemyProblemIds = new();
        public List<string> bossProblemIds = new();
        public string dropBiasId = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool usesFormalStageData;
    }

    [Serializable]
    public sealed class EnemyProblemSeed
    {
        public string problemType = string.Empty;
        public string displayName = string.Empty;
        public string pressureType = string.Empty;
        public string problemSummary = string.Empty;
        public List<string> hardSolutionTags = new();
        public List<string> softSolutionTags = new();
        public List<string> validatedBuildTags = new();
        public FailureHintSeed failureHint = new();
        public string recommendedAction = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool referencesProductEnemyList;
        public bool affectsFormalCombat;
    }

    [Serializable]
    public sealed class BossProblemSeed
    {
        public string bossProblemId = string.Empty;
        public string displayName = string.Empty;
        public string validationGoal = string.Empty;
        public List<string> requiredProblemAttributes = new();
        public List<BossProblemKeySeed> keyRequirements = new();
        public int minimumKeysRequired = 3;
        public List<WeaknessWindowSeed> weaknessWindows = new();
        public List<DropBiasSeed> dropBiases = new();
        public List<FailureHintSeed> failureHints = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool referencesProductBossList;
        public bool affectsFormalCombat;
    }

    [Serializable]
    public sealed class BossProblemKeySeed
    {
        public string keyId = string.Empty;
        public string keyCategory = string.Empty;
        public string requirementId = string.Empty;
        public string problemAttribute = string.Empty;
        public int requiredScore = 1;
        public bool devOnly = true;
        public bool isEnabled;
        public bool gatesProductBoss;
        public string hint = string.Empty;
    }

    [Serializable]
    public sealed class WeaknessWindowSeed
    {
        public string weaknessWindowId = string.Empty;
        public string displayName = string.Empty;
        public string triggerCondition = string.Empty;
        public float startSecond;
        public float durationSecond = 3f;
        public List<string> exposedBuildTags = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool affectsFormalCombat;
    }

    [Serializable]
    public sealed class DropBiasSeed
    {
        public string dropBiasId = string.Empty;
        public string displayName = string.Empty;
        public string biasType = "dev_only_problem_support";
        public List<string> targetBuildTags = new();
        public List<string> targetItemTags = new();
        public List<string> targetAffixIds = new();
        public float previewWeight = 1f;
        public bool reportsOnly = true;
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool referencesProductDrops;
        public bool grantsProductItems;
    }

    [Serializable]
    public sealed class FailureHintSeed
    {
        public string failureHintId = string.Empty;
        public string headline = string.Empty;
        public string detail = string.Empty;
        public bool reportsOnly = true;
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool writesRuntimeUi;
    }
}
