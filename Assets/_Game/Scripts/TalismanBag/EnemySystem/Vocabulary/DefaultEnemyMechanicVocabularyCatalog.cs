using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.Vocabulary
{
    public static class DefaultEnemyMechanicVocabularyCatalog
    {
        public const string EnemyTypeSource = "EnemyBossValidationPool.enemyType";
        public const string BossMechanicSource = "EnemyBossValidationPool.bossMechanic";
        public const string ValidationTagsSource = "EnemyBossValidationPool.validationTags";
        public const string MechanicTypeSource = "BuildProblemSeedData.pressureType (mechanicType semantic)";
        public const string RequiredCapabilitySource = "BuildProblemSeedData.hardSolutionTags (requiredCapabilityTags semantic)";
        public const string OptionalCapabilitySource = "BuildProblemSeedData.softSolutionTags (optionalCapabilityTags semantic)";
        public const string MapAffectedSource = "BuildProblemSeedData.MapRuleSeed.affectedTags";
        public const string MapBuffSource = "BuildProblemSeedData.MapRuleSeed.buffTags";
        public const string MapDebuffSource = "BuildProblemSeedData.MapRuleSeed.debuffTags";
        public const string WeaknessMechanicSource = "BuildProblemSeedData.WeaknessWindowSeed.triggerCondition";
        public const string WeaknessWindowTypeSource = "BuildProblemRuleConfigs.WeaknessWindowConfig.windowType";

        private const string AtomicMappingReason = "Legacy semantic decomposed into stable Enemy vocabulary primitives.";
        private const string CapabilityMappingReason = "Legacy capability label maps to the matching abstract Build capability identity.";

        public static EnemyMechanicVocabularySnapshotInput CreateInput(bool reverseInputOrder = false)
        {
            List<EnemyVocabularyEntrySnapshot> entries = CreateEntries().ToList();
            List<LegacyEnemyVocabularyMappingSnapshot> mappings = CreateLegacyMappings().ToList();
            if (reverseInputOrder)
            {
                entries.Reverse();
                mappings.Reverse();
            }

            return new EnemyMechanicVocabularySnapshotInput(entries, mappings);
        }

        public static IReadOnlyList<EnemyVocabularyEntrySnapshot> CreateEntries()
        {
            List<EnemyVocabularyEntrySnapshot> entries = new List<EnemyVocabularyEntrySnapshot>
            {
                Entry(new MechanicKey("mechanic.basic_pressure"), "基础压力", "不带专用机制原语的基础战斗压力。"),
                Entry(new MechanicKey("mechanic.layered_shield"), "层叠护盾", "敌方以可重复叠加或恢复的护盾层制造压力。"),
                Entry(new MechanicKey("mechanic.swarm_summon"), "群怪与召唤", "敌方通过多目标、增援或召唤物扩大场面压力。"),
                Entry(new MechanicKey("mechanic.poison"), "毒", "敌方施加持续毒性负面状态。"),
                Entry(new MechanicKey("mechanic.burning"), "燃烧", "敌方施加持续燃烧负面状态。"),
                Entry(new MechanicKey("mechanic.energy_drain"), "偷灵与能量削减", "敌方削减、窃取或切断玩家供能资源。"),
                Entry(new MechanicKey("mechanic.talisman_seal"), "封符", "敌方压制或暂时封锁符箓触发。"),
                Entry(new MechanicKey("mechanic.burst_spike"), "高爆发", "敌方在短时间内制造显著承伤峰值。"),
                Entry(new MechanicKey("mechanic.long_cast"), "长读条施法", "敌方通过可观察的长读条准备关键技能。"),
                Entry(new MechanicKey("mechanic.high_health_endurance"), "厚血与长线战斗", "敌方依靠高耐久把战斗拖入长线输出检查。"),
                Entry(new MechanicKey("mechanic.polluted_tile"), "污染格", "敌方或环境污染可摆放格并限制阵盘空间。"),
                Entry(new MechanicKey("mechanic.formation_eye_disruption"), "阵眼干扰", "敌方干扰阵眼、阵位或供能链稳定。"),

                Entry(new BuildCapabilityKey("capability.break_power"), "破盾能力", "Build 对护盾、护壳或防御层的处理能力。"),
                Entry(new BuildCapabilityKey("capability.clear_power"), "清场能力", "Build 同时处理多目标与召唤物的能力。"),
                Entry(new BuildCapabilityKey("capability.cleanse_power"), "净化能力", "Build 清除负面状态或污染影响的能力。"),
                Entry(new BuildCapabilityKey("capability.debuff_counter"), "负面状态反制", "Build 压低、免疫或反制负面状态的能力。"),
                Entry(new BuildCapabilityKey("capability.energy_stability"), "供能稳定", "Build 面对资源削减时维持供能链的能力。"),
                Entry(new BuildCapabilityKey("capability.spirit_lock"), "锁灵能力", "Build 防止灵力被窃取或切断的能力。"),
                Entry(new BuildCapabilityKey("capability.control_power"), "控制能力", "Build 施加控制并争取反制节奏的能力。"),
                Entry(new BuildCapabilityKey("capability.placement_shape"), "摆放形态", "Build 通过占格形态与阵位保持可用布局的能力。"),
                Entry(new BuildCapabilityKey("capability.burst_window"), "爆发窗口", "Build 在短窗口内集中输出或完成反制的能力。"),
                Entry(new BuildCapabilityKey("capability.guard_power"), "护阵能力", "Build 承受爆发并保护阵位的能力。"),
                Entry(new BuildCapabilityKey("capability.interrupt_timing"), "打断时机", "Build 把握施法时机完成打断的能力。"),
                Entry(new BuildCapabilityKey("capability.sustained_damage"), "持续输出", "Build 在长线战斗中稳定输出的能力。"),
                Entry(new BuildCapabilityKey("capability.chain_reaction"), "连锁反应", "Build 通过连锁触发扩大清场或输出的能力。"),
                Entry(new BuildCapabilityKey("capability.cooldown_recovery"), "冷却恢复", "Build 对冷却延缓或持续触发压力的恢复能力。"),
                Entry(new BuildCapabilityKey("capability.caster_interrupt"), "施法打断", "Build 专门阻止敌方读条施法完成的能力。"),
                Entry(new BuildCapabilityKey("capability.thunder_chain"), "雷链能力", "Build 以雷系连锁强化破盾或多段处理的能力。"),

                Entry(new PressureChannelKey("pressure.shield"), "护盾压力", "战斗压力主要来自护盾或护壳层。"),
                Entry(new PressureChannelKey("pressure.multi_target"), "多目标压力", "战斗压力主要来自多目标、增援或召唤物。"),
                Entry(new PressureChannelKey("pressure.status_damage"), "持续伤害与负面状态压力", "战斗压力主要来自持续伤害、污染或负面状态。"),
                Entry(new PressureChannelKey("pressure.resource_disruption"), "资源干扰压力", "战斗压力主要来自灵力与供能资源被干扰。"),
                Entry(new PressureChannelKey("pressure.seal_control"), "封符与控制压力", "战斗压力主要来自符箓受封或行动受控。"),
                Entry(new PressureChannelKey("pressure.burst_survival"), "爆发承伤压力", "战斗压力主要来自短时高额伤害。"),
                Entry(new PressureChannelKey("pressure.cast_interrupt"), "施法打断压力", "战斗压力要求识别并处理敌方读条。"),
                Entry(new PressureChannelKey("pressure.sustained_output"), "长线输出压力", "战斗压力要求稳定持续输出。"),
                Entry(new PressureChannelKey("pressure.placement_formation"), "摆放与阵眼压力", "战斗压力来自占格、阵位、阵眼或供能链限制。"),

                Entry(new CounterWindowTypeKey("counter_window.shell_break"), "破壳后虚弱", "成功破除护壳后出现的虚弱窗口。"),
                Entry(new CounterWindowTypeKey("counter_window.cleanse_reveal"), "净化后显形", "完成净化反制后出现的显形窗口。"),
                Entry(new CounterWindowTypeKey("counter_window.interrupt_stagger"), "打断后僵直", "成功打断施法后出现的僵直窗口。"),
                Entry(new CounterWindowTypeKey("counter_window.guard_rebound"), "护阵后反震", "成功护阵后触发的反震窗口。"),
                Entry(new CounterWindowTypeKey("counter_window.clear_core_exposure"), "清场后核心暴露", "清除召唤物后出现的核心暴露窗口。"),
                Entry(new CounterWindowTypeKey("counter_window.energy_counter_full_array"), "供能反制后全阵窗口", "完成供能反制后出现的全阵协同窗口。"),

                PlayerHint("player_hint.basic_pressure", "基础压力信号", "敌方压力正在提升。"),
                PlayerHint("player_hint.shield_pressure", "护盾正在恢复", "敌方护盾状态正在发生变化。"),
                PlayerHint("player_hint.multi_target_pressure", "敌群正在聚集", "场上目标数量正在增加。"),
                PlayerHint("player_hint.status_pressure", "持续异常状态", "持续异常影响正在累积。"),
                PlayerHint("player_hint.energy_disrupted", "供能受到干扰", "当前阵盘供能出现干扰。"),
                PlayerHint("player_hint.talisman_sealed", "符箓暂时受封", "部分符箓触发暂时受到限制。"),
                PlayerHint("player_hint.burst_incoming", "高威胁攻击将至", "敌方正在准备高威胁攻击。"),
                PlayerHint("player_hint.cast_interrupt_opportunity", "施法可被打断", "敌方施法出现可观察的反制时机。"),
                PlayerHint("player_hint.sustained_pressure", "长线压力升高", "战斗正在进入持续消耗阶段。"),
                PlayerHint("player_hint.formation_disrupted", "阵位受到干扰", "阵眼或关键摆放区域正在受到影响。"),

                Diagnostic("diagnostic.capability_gap", "能力缺口", "开发者诊断发现抽象 Build 能力不足。"),
                Diagnostic("diagnostic.unmapped_legacy_key", "未映射旧键", "开发者诊断发现旧键未完成映射。"),
                Diagnostic("diagnostic.invalid_reference", "无效引用", "开发者诊断发现稳定键引用无法解析。"),
                Diagnostic("diagnostic.duplicate_stable_key", "重复稳定键", "开发者诊断发现稳定键重复。"),
                Diagnostic("diagnostic.duplicate_legacy_source", "重复旧来源", "开发者诊断发现旧来源键重复登记。"),
                Diagnostic("diagnostic.invalid_stable_key_format", "稳定键格式错误", "开发者诊断发现稳定键不符合命名格式。"),
                Diagnostic("diagnostic.category_collision", "类别键冲突", "开发者诊断发现不同类别共享完整稳定键。"),
                Diagnostic("diagnostic.player_answer_leak", "玩家答案泄漏", "开发者诊断发现玩家提示包含答案字段。"),
                Diagnostic("diagnostic.canonical_signature_mismatch", "签名不一致", "开发者诊断发现确定性签名异常。"),
                Diagnostic("diagnostic.out_of_scope_legacy_key", "旧键范围外", "开发者诊断记录旧键明确不属于本词汇包。")
            };

            return Array.AsReadOnly(entries.ToArray());
        }

        public static IReadOnlyList<LegacyEnemyVocabularyMappingSnapshot> CreateLegacyMappings()
        {
            List<LegacyEnemyVocabularyMappingSnapshot> rows = new List<LegacyEnemyVocabularyMappingSnapshot>();

            AddMechanicPressure(rows, EnemyTypeSource, "basic", "mechanic.basic_pressure", null);
            AddMechanicPressure(rows, EnemyTypeSource, "shield_guard", "mechanic.layered_shield", "pressure.shield");
            AddMechanicPressure(rows, EnemyTypeSource, "poison", "mechanic.poison", "pressure.status_damage");
            AddMechanicPressure(rows, EnemyTypeSource, "burning", "mechanic.burning", "pressure.status_damage");
            AddMechanicPressure(rows, EnemyTypeSource, "spirit_thief", "mechanic.energy_drain", "pressure.resource_disruption");
            AddMechanicPressure(rows, EnemyTypeSource, "seal_lock", "mechanic.talisman_seal", "pressure.seal_control");
            AddMechanicPressure(rows, EnemyTypeSource, "swarm", "mechanic.swarm_summon", "pressure.multi_target");
            AddMechanicPressure(rows, EnemyTypeSource, "burst", "mechanic.burst_spike", "pressure.burst_survival");
            AddMechanicPressure(rows, EnemyTypeSource, "caster", "mechanic.long_cast", "pressure.cast_interrupt");
            AddMechanicPressure(rows, EnemyTypeSource, "high_hp", "mechanic.high_health_endurance", "pressure.sustained_output");
            AddMechanicPressure(rows, EnemyTypeSource, "formation_eye_disrupt", "mechanic.formation_eye_disruption", "pressure.placement_formation");

            AddMechanicPressure(rows, BossMechanicSource, "shield_boss", "mechanic.layered_shield", "pressure.shield");
            AddMechanicPressure(rows, BossMechanicSource, "swarm_boss", "mechanic.swarm_summon", "pressure.multi_target");
            AddMechanicPressure(rows, BossMechanicSource, "burst_boss", "mechanic.burst_spike", "pressure.burst_survival");
            Mapped(rows, BossMechanicSource, "debuff_boss", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.status_damage"));
            AddMechanicPressure(rows, BossMechanicSource, "caster_boss", "mechanic.long_cast", "pressure.cast_interrupt");
            AddMechanicPressure(rows, BossMechanicSource, "energy_jammer_boss", "mechanic.energy_drain", "pressure.resource_disruption");
            Out(rows, BossMechanicSource, "hybrid_combo_boss", "Composite Boss marker has no atomic mechanic meaning; E03 must decompose it by explicit profile references.");

            AddMechanicPressure(rows, ValidationTagsSource, "basic", "mechanic.basic_pressure", null);
            Out(rows, ValidationTagsSource, "baseline", "Benchmark metadata marker, not an Enemy mechanic, capability, pressure, counter window, hint, or diagnostic category.");
            AddMechanicPressure(rows, ValidationTagsSource, "shield", "mechanic.layered_shield", "pressure.shield");
            Mapped(rows, ValidationTagsSource, "break_test", CapabilityMappingReason, T(EnemyVocabularyCategory.BuildCapability, "capability.break_power"));
            AddMechanicPressure(rows, ValidationTagsSource, "poison", "mechanic.poison", "pressure.status_damage");
            Mapped(rows, ValidationTagsSource, "negative_status", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.status_damage"));
            AddMechanicPressure(rows, ValidationTagsSource, "burning", "mechanic.burning", "pressure.status_damage");
            Mapped(rows, ValidationTagsSource, "dot_pressure", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.status_damage"));
            AddMechanicPressure(rows, ValidationTagsSource, "energy_drain", "mechanic.energy_drain", "pressure.resource_disruption");
            Mapped(rows, ValidationTagsSource, "resource_disrupt", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.resource_disruption"));
            AddMechanicPressure(rows, ValidationTagsSource, "seal_lock", "mechanic.talisman_seal", "pressure.seal_control");
            Mapped(rows, ValidationTagsSource, "control", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.seal_control"));
            AddMechanicPressure(rows, ValidationTagsSource, "swarm", "mechanic.swarm_summon", "pressure.multi_target");
            Mapped(rows, ValidationTagsSource, "multi_target", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.multi_target"));
            AddMechanicPressure(rows, ValidationTagsSource, "burst", "mechanic.burst_spike", "pressure.burst_survival");
            Mapped(rows, ValidationTagsSource, "low_hp_pressure", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.burst_survival"));
            AddMechanicPressure(rows, ValidationTagsSource, "caster", "mechanic.long_cast", "pressure.cast_interrupt");
            Mapped(rows, ValidationTagsSource, "skill_cast", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.cast_interrupt"));
            AddMechanicPressure(rows, ValidationTagsSource, "high_hp", "mechanic.high_health_endurance", "pressure.sustained_output");
            Mapped(rows, ValidationTagsSource, "long_fight", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.sustained_output"));
            AddMechanicPressure(rows, ValidationTagsSource, "formation_eye", "mechanic.formation_eye_disruption", "pressure.placement_formation");
            Mapped(rows, ValidationTagsSource, "placement_disrupt", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.placement_formation"));
            Out(rows, ValidationTagsSource, "boss", "Archetype marker, not a mechanic primitive.");
            Mapped(rows, ValidationTagsSource, "cleanse", CapabilityMappingReason, T(EnemyVocabularyCategory.BuildCapability, "capability.cleanse_power"));
            Out(rows, ValidationTagsSource, "hybrid", "Composite marker requires explicit mechanic references in E03.");
            Out(rows, ValidationTagsSource, "combo", "Composition marker requires explicit mechanic references in E03.");

            AddMechanicPressure(rows, MechanicTypeSource, "layered_shield", "mechanic.layered_shield", "pressure.shield");
            AddMechanicPressure(rows, MechanicTypeSource, "swarm_pressure", "mechanic.swarm_summon", "pressure.multi_target");
            Mapped(rows, MechanicTypeSource, "dot_and_debuff", AtomicMappingReason,
                T(EnemyVocabularyCategory.Mechanic, "mechanic.poison"),
                T(EnemyVocabularyCategory.Mechanic, "mechanic.burning"),
                T(EnemyVocabularyCategory.PressureChannel, "pressure.status_damage"));
            AddMechanicPressure(rows, MechanicTypeSource, "energy_drain", "mechanic.energy_drain", "pressure.resource_disruption");
            AddMechanicPressure(rows, MechanicTypeSource, "seal_lock", "mechanic.talisman_seal", "pressure.seal_control");
            AddMechanicPressure(rows, MechanicTypeSource, "burst_spike", "mechanic.burst_spike", "pressure.burst_survival");
            AddMechanicPressure(rows, MechanicTypeSource, "long_cast", "mechanic.long_cast", "pressure.cast_interrupt");
            AddMechanicPressure(rows, MechanicTypeSource, "high_health", "mechanic.high_health_endurance", "pressure.sustained_output");
            AddMechanicPressure(rows, MechanicTypeSource, "polluted_tile", "mechanic.polluted_tile", "pressure.placement_formation");
            AddMechanicPressure(rows, MechanicTypeSource, "formation_eye_jam", "mechanic.formation_eye_disruption", "pressure.placement_formation");

            AddDirectCapabilityMappings(rows, RequiredCapabilitySource, new[]
            {
                "BreakPower", "ThunderChain", "ClearPower", "ChainReaction", "CleansePower", "DebuffCounter",
                "EnergyStability", "SpiritLock", "ControlPower", "PlacementShape", "GuardPower",
                "InterruptTiming", "SustainedDamage"
            });

            AddDirectCapabilityMappings(rows, OptionalCapabilitySource, new[]
            {
                "BurstWindow", "ControlPower", "GuardPower", "EnergyStability", "PlacementShape", "CleansePower", "ClearPower"
            });
            Mapped(rows, OptionalCapabilitySource, "AffixBreakBoost", "Concrete affix-era label collapses to its abstract break capability.", T(EnemyVocabularyCategory.BuildCapability, "capability.break_power"));
            Mapped(rows, OptionalCapabilitySource, "GuardStall", "Legacy stall label collapses to abstract guard capability.", T(EnemyVocabularyCategory.BuildCapability, "capability.guard_power"));

            AddDirectCapabilityMappings(rows, MapAffectedSource, new[]
            {
                "BreakPower", "ClearPower", "CleansePower", "EnergyStability", "BurstWindow", "ControlPower",
                "GuardPower", "CasterInterrupt", "PlacementShape", "CooldownRecovery", "ChainReaction"
            });
            Mapped(rows, MapAffectedSource, "PollutedTile", AtomicMappingReason,
                T(EnemyVocabularyCategory.Mechanic, "mechanic.polluted_tile"),
                T(EnemyVocabularyCategory.PressureChannel, "pressure.placement_formation"));
            Out(rows, MapAffectedSource, "SmokePressure", "Visibility/environment effect is not one of the first stable pressure channels; retain for E03 map-rule normalization.");

            foreach (string key in new[] { "ThunderEcho", "WaterTrace", "LampFocus", "BellResonance", "FormationAnchor", "DryCharm", "EchoTrace", "AshWard", "WatchLamp", "StoneLine" })
            {
                Out(rows, MapBuffSource, key, "Map-specific buff/effect token is content data, not an abstract Enemy mechanic or Build capability.");
            }

            Out(rows, MapDebuffSource, "SightReduced", "Visibility effect is outside the first stable pressure-channel set.");
            AddMechanicPressure(rows, MapDebuffSource, "ShieldLayered", "mechanic.layered_shield", "pressure.shield");
            Out(rows, MapDebuffSource, "TalismanDamp", "Map flavor status needs E03 normalization before assigning an atomic mechanic.");
            AddMechanicPressure(rows, MapDebuffSource, "EnergyLeak", "mechanic.energy_drain", "pressure.resource_disruption");
            Mapped(rows, MapDebuffSource, "WindowShort", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.burst_survival"));
            Out(rows, MapDebuffSource, "CastNoise", "Player-side cast-noise effect is not the enemy long-cast/interrupt pressure primitive.");
            Mapped(rows, MapDebuffSource, "GuardDrain", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.burst_survival"));
            AddMechanicPressure(rows, MapDebuffSource, "AnchorShift", "mechanic.formation_eye_disruption", "pressure.placement_formation");
            AddMechanicPressure(rows, MapDebuffSource, "ShapePenalty", "mechanic.formation_eye_disruption", "pressure.placement_formation");
            Mapped(rows, MapDebuffSource, "CooldownSlow", AtomicMappingReason,
                T(EnemyVocabularyCategory.BuildCapability, "capability.cooldown_recovery"),
                T(EnemyVocabularyCategory.PressureChannel, "pressure.sustained_output"));
            Out(rows, MapDebuffSource, "PaperDamp", "Map flavor status needs E03 normalization before assigning an atomic mechanic.");
            AddMechanicPressure(rows, MapDebuffSource, "SummonEcho", "mechanic.swarm_summon", "pressure.multi_target");
            AddMechanicPressure(rows, MapDebuffSource, "TilePollution", "mechanic.polluted_tile", "pressure.placement_formation");
            Mapped(rows, MapDebuffSource, "GuardPressure", AtomicMappingReason, T(EnemyVocabularyCategory.PressureChannel, "pressure.burst_survival"));
            Out(rows, MapDebuffSource, "BlindInterval", "Visibility interval is outside the first stable pressure-channel set.");
            AddMechanicPressure(rows, MapDebuffSource, "BurstTax", "mechanic.burst_spike", "pressure.burst_survival");
            AddMechanicPressure(rows, MapDebuffSource, "EnergyCut", "mechanic.energy_drain", "pressure.resource_disruption");
            AddMechanicPressure(rows, MapDebuffSource, "ShapeSplit", "mechanic.formation_eye_disruption", "pressure.placement_formation");

            Mapped(rows, WeaknessMechanicSource, "layered_shell_break", "Legacy weakness semantic becomes the shell-break counter-window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.shell_break"));
            Mapped(rows, WeaknessMechanicSource, "cleanse_reveal", "Legacy weakness semantic becomes the cleanse-reveal counter-window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.cleanse_reveal"));
            Mapped(rows, WeaknessMechanicSource, "charge_interrupt", "Legacy weakness semantic becomes the interrupt-stagger counter-window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.interrupt_stagger"));
            Mapped(rows, WeaknessMechanicSource, "guard_counter", "Legacy weakness semantic becomes the guard-rebound counter-window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.guard_rebound"));
            Mapped(rows, WeaknessMechanicSource, "summon_clear_reveal", "Legacy weakness semantic becomes the clear-core-exposure counter-window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.clear_core_exposure"));
            Mapped(rows, WeaknessMechanicSource, "full_loop_counter", "Legacy weakness semantic becomes the energy-counter full-array window type.", T(EnemyVocabularyCategory.CounterWindowType, "counter_window.energy_counter_full_array"));
            Out(rows, WeaknessWindowTypeSource, "timed_vulnerability", "Generic timing schema does not identify one of the six semantic counter-window categories.");

            return Array.AsReadOnly(rows.ToArray());
        }

        public static string LegacySourcePath(string sourceKind)
        {
            if (sourceKind == EnemyTypeSource || sourceKind == BossMechanicSource || sourceKind == ValidationTagsSource)
            {
                return "Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs";
            }

            if (sourceKind == WeaknessWindowTypeSource)
            {
                return "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs";
            }

            return "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs";
        }

        private static EnemyVocabularyEntrySnapshot Entry(
            EnemyVocabularyStableKey key,
            string developerLabelZh,
            string description)
        {
            return new EnemyVocabularyEntrySnapshot(key, developerLabelZh, description, false, false);
        }

        private static EnemyVocabularyEntrySnapshot PlayerHint(string stableKey, string developerLabelZh, string description)
        {
            return new EnemyVocabularyEntrySnapshot(new PlayerHintCategoryKey(stableKey), developerLabelZh, description, true, false);
        }

        private static EnemyVocabularyEntrySnapshot Diagnostic(string stableKey, string developerLabelZh, string description)
        {
            return new EnemyVocabularyEntrySnapshot(new DeveloperDiagnosticCategoryKey(stableKey), developerLabelZh, description, false, true);
        }

        private static void AddMechanicPressure(
            ICollection<LegacyEnemyVocabularyMappingSnapshot> rows,
            string sourceKind,
            string legacyKey,
            string mechanicKey,
            string pressureKey)
        {
            List<EnemyVocabularyKeyReferenceSnapshot> targets = new List<EnemyVocabularyKeyReferenceSnapshot>
            {
                T(EnemyVocabularyCategory.Mechanic, mechanicKey)
            };
            if (!string.IsNullOrEmpty(pressureKey))
            {
                targets.Add(T(EnemyVocabularyCategory.PressureChannel, pressureKey));
            }

            Mapped(rows, sourceKind, legacyKey, AtomicMappingReason, targets.ToArray());
        }

        private static void AddDirectCapabilityMappings(
            ICollection<LegacyEnemyVocabularyMappingSnapshot> rows,
            string sourceKind,
            IEnumerable<string> legacyKeys)
        {
            foreach (string legacyKey in legacyKeys)
            {
                Mapped(rows, sourceKind, legacyKey, CapabilityMappingReason,
                    T(EnemyVocabularyCategory.BuildCapability, CapabilityStableKey(legacyKey)));
            }
        }

        private static string CapabilityStableKey(string legacyKey)
        {
            switch (legacyKey)
            {
                case "BreakPower": return "capability.break_power";
                case "ClearPower": return "capability.clear_power";
                case "CleansePower": return "capability.cleanse_power";
                case "DebuffCounter": return "capability.debuff_counter";
                case "EnergyStability": return "capability.energy_stability";
                case "SpiritLock": return "capability.spirit_lock";
                case "ControlPower": return "capability.control_power";
                case "PlacementShape": return "capability.placement_shape";
                case "BurstWindow": return "capability.burst_window";
                case "GuardPower": return "capability.guard_power";
                case "InterruptTiming": return "capability.interrupt_timing";
                case "SustainedDamage": return "capability.sustained_damage";
                case "ChainReaction": return "capability.chain_reaction";
                case "CooldownRecovery": return "capability.cooldown_recovery";
                case "CasterInterrupt": return "capability.caster_interrupt";
                case "ThunderChain": return "capability.thunder_chain";
                default: throw new ArgumentException("Unknown abstract capability key: " + legacyKey, nameof(legacyKey));
            }
        }

        private static EnemyVocabularyKeyReferenceSnapshot T(EnemyVocabularyCategory category, string stableKey)
        {
            return new EnemyVocabularyKeyReferenceSnapshot(category, stableKey);
        }

        private static void Mapped(
            ICollection<LegacyEnemyVocabularyMappingSnapshot> rows,
            string sourceKind,
            string legacyKey,
            string reason,
            params EnemyVocabularyKeyReferenceSnapshot[] targets)
        {
            rows.Add(new LegacyEnemyVocabularyMappingSnapshot(
                sourceKind,
                legacyKey,
                targets,
                LegacyEnemyVocabularyMappingStatus.Mapped,
                reason));
        }

        private static void Out(
            ICollection<LegacyEnemyVocabularyMappingSnapshot> rows,
            string sourceKind,
            string legacyKey,
            string reason)
        {
            rows.Add(new LegacyEnemyVocabularyMappingSnapshot(
                sourceKind,
                legacyKey,
                Array.Empty<EnemyVocabularyKeyReferenceSnapshot>(),
                LegacyEnemyVocabularyMappingStatus.OutOfScope,
                reason));
        }
    }
}
