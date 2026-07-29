using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.Balance
{
    public enum ItemCandidateEffectCategory
    {
        NumericModifier, ConditionalModifier, TriggeredEffect, ResourceEffect, CooldownEffect,
        SpatialEffect, BuildSynergyEffect, CoreEffect, MechanicConversion, TradeoffEffect
    }

    public enum ItemCandidateEffectOperation
    {
        AddFlat, AddPercent, Multiply, ReduceFlat, ReducePercent, Refund,
        ExtraTarget, ExtraTrigger, Convert, Override
    }

    [Serializable]
    public sealed class ItemCandidateEffectPayload
    {
        public string effectId = string.Empty;
        public string displayName = string.Empty;
        public string description = string.Empty;
        public ItemCandidateEffectCategory effectCategory;
        public string triggerEventId = string.Empty;
        public string conditionId = string.Empty;
        public string targetSelector = string.Empty;
        public string targetStatId = string.Empty;
        public ItemCandidateEffectOperation operation;
        public string valueUnitKey = string.Empty;
        public long valueUnits;
        public long secondaryValueUnits;
        public long durationUnits;
        public int stackLimit = 1;
        public long internalCooldownUnits;
        public string parameterProfileId = string.Empty;
        public string mutexGroupId = "NONE";
        public List<string> effectTags = new();
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateRarityValue
    {
        public ItemInstanceRarity rarity;
        public long minUnits;
        public long maxUnits;
    }

    [Serializable]
    public sealed class ItemCandidateAffixDefinition
    {
        public string affixId = string.Empty;
        public string displayName = string.Empty;
        public string description = string.Empty;
        public ItemCandidateEffectPayload effectPayload = new();
        public List<ItemCandidateRarityValue> rarityRanges = new();
        public string mutexGroupId = "NONE";
        public string repeatPolicy = "NO_DUPLICATE";
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateBuildStageDefinition
    {
        public string buildId = string.Empty;
        public string stableTag = string.Empty;
        public string displayName = string.Empty;
        public int stagePieceCount;
        public string description = string.Empty;
        public ItemCandidateEffectPayload effectPayload = new();
        public string conditionText = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateDisplayProfile
    {
        public string triggerDescription = string.Empty;
        public string basicEffectDescription = string.Empty;
        public string lightingDescription = string.Empty;
        public string placementRecommendation = string.Empty;
        public string flavorText = string.Empty;
        public string faMenDisplayName = string.Empty;
        public string qiLeiDisplayName = string.Empty;
        public string shapeDescription = string.Empty;
        public string iconKey = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
    }

    [Serializable]
    public sealed class ItemCandidateItemPowerCoefficient
    {
        public string coefficientId = string.Empty;
        public float value = 1f;
        public string description = string.Empty;
    }

    public static class ItemCompleteCandidateContentSeed
    {
        public const string Revision = "ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_R1";

        private sealed class SignatureSeed
        {
            public SignatureSeed(string name, string description, ItemCandidateEffectCategory category,
                ItemCandidateEffectOperation operation, string statId, string trigger, string condition,
                string unit, long value, long secondary = 0)
            {
                this.name = name; this.description = description; this.category = category;
                this.operation = operation; this.statId = statId; this.trigger = trigger;
                this.condition = condition; this.unit = unit; this.value = value; this.secondary = secondary;
            }
            public readonly string name, description, statId, trigger, condition, unit;
            public readonly ItemCandidateEffectCategory category;
            public readonly ItemCandidateEffectOperation operation;
            public readonly long value, secondary;
        }

        private static readonly SignatureSeed[] SignatureSeeds =
        {
            S("引雷", "首次命中有护盾的目标时，额外提高破盾并标记一道雷隙。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.AddPercent, "break", "on_first_hit_shielded", "target_has_shield", "basisPoint", 500),
            S("急引", "连续两次触发间隔不超过两回合时，返还念并推进下一次触发。", ItemCandidateEffectCategory.CooldownEffect, ItemCandidateEffectOperation.Refund, "nian", "on_consecutive_trigger", "within_2_turns", "point", 1, 1),
            S("破壳", "压印命中护盾时，使本次破盾结果额外提高并延长雷隙。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "break", "on_hit", "target_has_shield", "basisPoint", 650, 1),
            S("催雷", "直接点亮后，下一次震雷触发额外追加一个近邻目标。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTarget, "targetCount", "on_direct_lit", "next_zhenlei_trigger", "count", 1),
            S("照壳", "镜照出护盾弱处后，下一次雷击将部分伤害转换为破盾。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "damage_to_break", "on_reveal_shield", "next_hit", "basisPoint", 3000),
            S("天鼓", "每完成三次震雷触发，天鼓额外震鸣一次范围破盾。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTrigger, "break", "on_zhenlei_trigger", "trigger_count_3", "basisPoint", 4000),
            S("燃符", "点亮后首次命中附加一层余焰，余焰在下回合结算。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.AddFlat, "burn", "on_first_hit", "is_lit", "stack", 1, 1),
            S("焚邪", "目标身上每有一层余焰，长符持续伤害提高，最多四层。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "damage", "on_tick", "burn_stack", "basisPoint", 350, 4),
            S("落火", "压印命中带余焰目标时延长余焰，并立即结算一小段伤害。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTrigger, "burn", "on_hit_burning", "burn_stack_gt_0", "turn", 1, 2500),
            S("风助", "令旗点亮时为相邻离火道具增加持续时间，并降低一次耗念。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.ReduceFlat, "nianCost", "on_lit", "adjacent_lihuo", "point", 1, 1),
            S("回光", "镜照触发后复制目标身上一层余焰到另一名敌人。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "burn", "on_mirror_trigger", "target_has_burn", "stack", 1),
            S("长明", "离火效果自然结束时保留一枚火种，下一次点燃消耗火种追加触发。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.ExtraTrigger, "ember", "on_burn_expire", "duration_natural_end", "count", 1),
            S("护身", "受到伤害后若护势低于阈值，立即获得一层轻护。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddFlat, "guard", "on_damaged", "guard_below_threshold", "flat", 4),
            S("镇宅", "每有一个横向相邻道具，护势提高，最多两个相邻来源。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddPercent, "guard", "on_layout_evaluate", "horizontal_adjacent", "basisPoint", 400, 2),
            S("镇守", "直接点亮时获得护势；护势未耗尽前减少一次控制。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.AddFlat, "guard", "on_direct_lit", "is_direct_lit", "flat", 8, 1),
            S("护坛", "同法门道具首次受到伤害时，令旗分担部分伤害并获得护势。", ItemCandidateEffectCategory.TradeoffEffect, ItemCandidateEffectOperation.Convert, "damage_to_guard", "on_ally_damaged", "same_famen", "basisPoint", 2000, 4),
            S("定影", "镜照成功后记录本次伤害，下一次受击按记录值生成护势。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "damage_to_guard", "on_mirror_trigger", "next_incoming_hit", "basisPoint", 2500),
            S("镇山", "占据阵位点并已点亮时，每回合获得护势；移动后效果重置。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddFlat, "guard", "on_turn_start", "lit_on_array_cell", "flat", 6),
            S("净水", "完成一次净化后恢复生命，溢出的恢复转为护势。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "heal_to_guard", "on_cleanse", "cleanse_success", "basisPoint", 2500, 3),
            S("涤秽", "连续净化同一目标时，第二次起额外移除一层负面状态。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddFlat, "cleanse", "on_cleanse", "same_target_chain", "stack", 1),
            S("涤心", "印心完成净化时，返还念并使下一次回复提高。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.Refund, "nian", "on_cleanse", "cleanse_success", "point", 1, 500),
            S("回流", "玄水效果结束时推进相邻道具冷却，若其已点亮则再返还念。", ItemCandidateEffectCategory.CooldownEffect, ItemCandidateEffectOperation.ReduceFlat, "cooldown", "on_effect_end", "adjacent_lit_item", "turn", 1, 1),
            S("照秽", "镜照到负面状态时复制其剩余持续时间，转化为回复持续时间。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "debuff_to_heal", "on_mirror_trigger", "target_has_debuff", "turn", 2),
            S("蓄水", "每次净化积蓄一滴净水；三滴时自动治疗生命最低的目标。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.ExtraTrigger, "heal", "on_cleanse", "water_charge_3", "flat", 12, 3),
            S("封煞", "命中正在施法的目标时延缓其施法，并暴露一层破绽。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ReduceFlat, "castProgress", "on_hit", "target_casting", "turn", 1, 1),
            S("太白", "对带破绽目标的首次斩击提高伤害，并消耗一层破绽。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "damage", "on_slash", "target_has_flaw", "basisPoint", 800, 1),
            S("斩煞", "压印命中带破绽目标时追加一次小斩击；目标低血量时倍率提高。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTrigger, "damage", "on_hit_flaw", "target_low_health", "basisPoint", 3000, 4500),
            S("断令", "令文触发后锁定目标，下一次太白伤害优先指向该目标。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.Override, "targeting", "on_command_trigger", "next_taibai_hit", "count", 1),
            S("照煞", "首次命中有护盾目标时照出弱点，使下一次攻击额外造成破盾。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.AddPercent, "break", "on_first_hit_shielded", "mirror_mark_active", "basisPoint", 700),
            S("桃木", "击败带破绽目标后保留剑势，下一次斩击改为贯穿相邻目标。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.ExtraTarget, "targetCount", "on_kill_flaw", "next_slash", "count", 1)
        };

        public static ItemCandidateAffixDefinition BuildSignatureAffix(ItemBalanceProfile profile)
        {
            int index = ParseIndex(profile?.baseItemId);
            SignatureSeed seed = SignatureSeeds[Math.Max(0, Math.Min(SignatureSeeds.Length - 1, index - 1))];
            string id = "signature_" + profile.baseItemId.ToLowerInvariant();
            return new ItemCandidateAffixDefinition
            {
                affixId = id,
                displayName = seed.name,
                description = seed.description,
                effectPayload = Payload(id, seed.name, seed.description, seed.category, seed.operation,
                    seed.statId, seed.trigger, seed.condition, seed.unit, seed.value, seed.secondary,
                    profile.baseItemId.ToLowerInvariant() + "_signature", "SIGNATURE_" + profile.baseItemId),
                rarityRanges = Ranges(seed.value, Math.Max(1, seed.value / 4)),
                mutexGroupId = "SIGNATURE_" + profile.baseItemId,
                repeatPolicy = "UNIQUE_FIXED",
                designNote = profile.displayName + "专属身份词条；候选数据，不接正式战斗。"
            };
        }

        public static List<ItemBalanceCoreCandidate> BuildCoreCandidates(ItemBalanceProfile profile)
        {
            SignatureSeed seed = SignatureSeeds[Math.Max(0, Math.Min(SignatureSeeds.Length - 1, ParseIndex(profile.baseItemId) - 1))];
            string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
            string[] kinds = { "Core1", "Core2", "Core3", "Core4", "Ultimate" };
            string[] suffixes = { "初识", "承式", "合脉", "转机", "成法" };
            ItemInstanceRarity[] rarities = { ItemInstanceRarity.White, ItemInstanceRarity.Green,
                ItemInstanceRarity.Blue, ItemInstanceRarity.Purple, ItemInstanceRarity.Orange };
            int[] levels = { 10, 20, 30, 40, 40 };
            List<ItemBalanceCoreCandidate> result = new();
            int[] retainedStageIndices = { 0, 1, 2, 4 };
            foreach (int i in retainedStageIndices)
            {
                bool ultimate = i == 4;
                string id = ultimate ? stem + "_ultimate" : stem + "_0" + (i + 1).ToString(CultureInfo.InvariantCulture);
                ItemCandidateEffectCategory category = ultimate ? ItemCandidateEffectCategory.MechanicConversion
                    : i >= 2 ? ItemCandidateEffectCategory.CoreEffect : seed.category;
                ItemCandidateEffectOperation operation = ultimate ? ItemCandidateEffectOperation.Convert
                    : i == 3 ? ItemCandidateEffectOperation.ExtraTrigger : i == 2
                        ? ItemCandidateEffectOperation.AddPercent : seed.operation;
                long value = seed.value + Math.Max(1, seed.value / 3) * i;
                string name = profile.displayName + "·" + suffixes[i];
                string description = ultimate
                    ? $"{profile.displayName}完成成法后，将“{seed.name}”的触发结果转换为一次不消耗原状态的机制回响；每轮限一次。"
                    : i switch
                    {
                        0 => $"{profile.displayName}点亮后，“{seed.name}”的基础候选效果获得第一层强化。",
                        1 => $"触发“{seed.name}”后返还一部分节奏资源，并保留下一次触发条件。",
                        2 => $"同属{profile.faMenTag}法门的已点亮道具达到Build2时，“{seed.name}”追加协同数值。",
                        _ => $"“{seed.name}”满足关键条件后额外触发一次，但受独立冷却限制。"
                    };
                result.Add(new ItemBalanceCoreCandidate
                {
                    coreEffectId = id,
                    displayName = name,
                    description = description,
                    isUltimate = ultimate,
                    nodeKind = kinds[i],
                    unlockLevel = levels[i],
                    requiredRarity = rarities[i],
                    effectType = category.ToString(),
                    effectPayload = Payload(id, name, description, category, operation, seed.statId,
                        ultimate ? "on_signature_resolved" : seed.trigger,
                        ultimate ? "ultimate_ready" : seed.condition, seed.unit, value,
                        ultimate ? seed.value : seed.secondary, profile.baseItemId.ToLowerInvariant() + "_core_" + (i + 1),
                        "CORE_" + profile.baseItemId),
                    stateDescription = "品阶满足后可见；养成解锁且实例点亮时才可激活。",
                    designNote = ultimate ? "机制转换型终极核心；不是单纯倍率放大。" : "同原型五层递进候选核心。"
                });
            }
            return result;
        }

        public static ItemCandidateDisplayProfile BuildDisplayProfile(ItemBalanceProfile profile)
        {
            ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(profile.baseItemId);
            SignatureSeed seed = SignatureSeeds[Math.Max(0, Math.Min(SignatureSeeds.Length - 1, ParseIndex(profile.baseItemId) - 1))];
            string faMen = item?.FaMenDisplayName ?? profile.faMenTag;
            string qiLei = item?.QiLeiDisplayName ?? profile.qiLeiTag;
            int cells = item?.defaultLocalCells?.Count ?? 1;
            return new ItemCandidateDisplayProfile
            {
                triggerDescription = "核心格已点亮时，每次满足“" + seed.name + "”的触发条件即可发动；候选内部冷却按载荷读取。",
                basicEffectDescription = CleanPreview(item?.basicEffectText) + " 本效果为Sandbox候选，不进入正式战斗结算。",
                lightingDescription = "核心格进入聚念范围或由正交相邻道具接亮后生效；未点亮时不发动，也不计入Build。",
                placementRecommendation = Placement(profile.faMenTag, profile.qiLeiTag, cells),
                flavorText = $"旧物记：{profile.displayName}原是青石坊里常见的{qiLei}，经{faMen}法脉重题符胆后，留下了“{seed.name}”这一手独门用法。",
                faMenDisplayName = faMen,
                qiLeiDisplayName = qiLei,
                shapeDescription = (item?.shapeId ?? "shape_unknown") + " · " + cells + "格 · 核心格" + (item == null ? "(0,0)" : ItemInnerDataDefinition.FormatCell(item.coreCellLocal)),
                iconKey = item?.iconPlaceholderKey ?? profile.baseItemId.ToLowerInvariant() + "_placeholder"
            };
        }

        public static List<ItemCandidateAffixDefinition> BuildRandomAffixDictionary()
        {
            List<ItemCandidateAffixDefinition> result = new();
            AddRandom(result, "affix_damage_up", "伤害提升", "伤害提高X%。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddPercent, "damage", "always", "NONE", "basisPoint", 300);
            AddRandom(result, "affix_break_up", "破盾提升", "破盾提高X%。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddPercent, "break", "always", "NONE", "basisPoint", 300);
            AddRandom(result, "affix_guard_up", "护势提升", "获得的护势提高X%。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddPercent, "guard", "always", "NONE", "basisPoint", 300);
            AddRandom(result, "affix_heal_up", "回复提升", "回复效果提高X%。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddPercent, "heal", "always", "NONE", "basisPoint", 300);
            AddRandom(result, "affix_cleanse_up", "净化增量", "每次净化额外移除X层。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddFlat, "cleanse", "on_cleanse", "cleanse_success", "stack", 1);
            AddRandom(result, "affix_control_up", "控制增量", "控制强度提高X点。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddFlat, "control", "always", "NONE", "point", 1);
            AddRandom(result, "affix_duration_up", "持续延长", "持续时间延长X回合。", ItemCandidateEffectCategory.NumericModifier, ItemCandidateEffectOperation.AddFlat, "duration", "on_apply_duration", "duration_gt_0", "turn", 1);
            result.Add(BuildNianEfficiencyCandidate());
            AddRandom(result, "affix_cooldown_reduction", "回转", "冷却缩短X回合。", ItemCandidateEffectCategory.CooldownEffect, ItemCandidateEffectOperation.ReduceFlat, "cooldown", "after_trigger", "cooldown_gt_0", "turn", 1);
            AddRandom(result, "affix_trigger_refund", "回念", "触发后返还X点念。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.Refund, "nian", "after_trigger", "trigger_success", "point", 1);
            AddRandom(result, "affix_direct_lit_power", "明点", "直接点亮时基础效果提高X%。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_direct_lit", "is_direct_lit", "basisPoint", 400);
            AddRandom(result, "affix_relay_lit_power", "接明", "由相邻道具接亮时基础效果提高X%。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_relay_lit", "is_relay_lit", "basisPoint", 350);
            AddRandom(result, "affix_array_guard", "镇位", "占据阵位点且已点亮时获得X点护势。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddFlat, "guard", "on_turn_start", "lit_on_array_cell", "flat", 3);
            AddRandom(result, "affix_adjacent_guard", "邻护", "每有一个相邻道具，护势提高X%，最多Y层。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddPercent, "guard", "on_layout_evaluate", "orthogonal_adjacent", "basisPoint", 250, 3);
            AddRandom(result, "affix_adjacent_damage", "邻攻", "每有一个相邻同法门道具，伤害提高X%，最多Y层。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddPercent, "damage", "on_layout_evaluate", "adjacent_same_famen", "basisPoint", 250, 3);
            AddRandom(result, "affix_isolated_power", "独阵", "无相邻道具时基础效果提高X%。", ItemCandidateEffectCategory.SpatialEffect, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_layout_evaluate", "adjacent_count_0", "basisPoint", 700);
            AddRandom(result, "affix_break_cooldown", "破壳回转", "破盾成功后冷却推进X回合。", ItemCandidateEffectCategory.CooldownEffect, ItemCandidateEffectOperation.ReduceFlat, "cooldown", "on_break_success", "break_success", "turn", 1);
            AddRandom(result, "affix_cleanse_refund", "净后回念", "净化成功后返还X点念。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.Refund, "nian", "on_cleanse", "cleanse_success", "point", 1);
            AddRandom(result, "affix_guard_refund", "护势回流", "护势耗尽时返还X点念。", ItemCandidateEffectCategory.ResourceEffect, ItemCandidateEffectOperation.Refund, "nian", "on_guard_depleted", "guard_was_positive", "point", 1);
            AddRandom(result, "affix_core_next_power", "窍后蓄势", "核心效果激活时，下一次基础效果提高X%。", ItemCandidateEffectCategory.CoreEffect, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_core_active", "next_basic_effect", "basisPoint", 600);
            AddRandom(result, "affix_core_cooldown", "窍后回转", "核心效果激活后冷却推进X回合。", ItemCandidateEffectCategory.CoreEffect, ItemCandidateEffectOperation.ReduceFlat, "cooldown", "on_core_active", "core_effect_active", "turn", 1);
            AddRandom(result, "affix_famen2_power", "法门初成", "法门Build2激活时基础效果提高X%。", ItemCandidateEffectCategory.BuildSynergyEffect, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_build_change", "famen_stage_2", "basisPoint", 500);
            AddRandom(result, "affix_famen4_trigger", "法门合式", "法门Build4激活时每轮追加一次候选触发。", ItemCandidateEffectCategory.BuildSynergyEffect, ItemCandidateEffectOperation.ExtraTrigger, "basicEffect", "on_round", "famen_stage_4", "count", 1);
            AddRandom(result, "affix_qilei2_power", "器类成对", "器类Build2激活时关键数值提高X%。", ItemCandidateEffectCategory.BuildSynergyEffect, ItemCandidateEffectOperation.AddPercent, "primaryStat", "on_build_change", "qilei_stage_2", "basisPoint", 500);
            AddRandom(result, "affix_qilei4_target", "器类合阵", "器类Build4激活时额外影响X个目标。", ItemCandidateEffectCategory.BuildSynergyEffect, ItemCandidateEffectOperation.ExtraTarget, "targetCount", "on_trigger", "qilei_stage_4", "count", 1);
            AddRandom(result, "affix_overheal_to_guard", "盈疗成护", "治疗溢出转为护势。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "heal_to_guard", "on_overheal", "overheal_gt_0", "basisPoint", 5000);
            AddRandom(result, "affix_break_to_damage", "破势成伤", "溢出的破盾转为伤害。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "break_to_damage", "on_break_overflow", "break_overflow_gt_0", "basisPoint", 3000);
            AddRandom(result, "affix_cleanse_to_heal", "清秽回生", "多余净化层数转为回复。", ItemCandidateEffectCategory.MechanicConversion, ItemCandidateEffectOperation.Convert, "cleanse_to_heal", "on_cleanse", "cleanse_overflow_gt_0", "flat", 4);
            AddRandom(result, "affix_damage_cost_trade", "烈性", "伤害提高X%，但耗念增加Y。", ItemCandidateEffectCategory.TradeoffEffect, ItemCandidateEffectOperation.AddPercent, "damage", "always", "NONE", "basisPoint", 900, 1);
            AddRandom(result, "affix_guard_slow_trade", "沉守", "护势提高X%，但冷却增加Y回合。", ItemCandidateEffectCategory.TradeoffEffect, ItemCandidateEffectOperation.AddPercent, "guard", "always", "NONE", "basisPoint", 1000, 1);
            AddRandom(result, "affix_fast_weak_trade", "急式", "冷却降低X回合，但基础效果降低Y%。", ItemCandidateEffectCategory.TradeoffEffect, ItemCandidateEffectOperation.ReduceFlat, "cooldown", "always", "NONE", "turn", 1, 500);
            AddRandom(result, "affix_low_health_power", "危时", "生命低于阈值时基础效果提高X%。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_trigger", "owner_low_health", "basisPoint", 800);
            AddRandom(result, "affix_shield_target_power", "破甲", "目标有护盾时效果提高X%。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_trigger", "target_has_shield", "basisPoint", 700);
            AddRandom(result, "affix_debuff_target_power", "逐秽", "目标有负面状态时效果提高X%。", ItemCandidateEffectCategory.ConditionalModifier, ItemCandidateEffectOperation.AddPercent, "basicEffect", "on_trigger", "target_has_debuff", "basisPoint", 650);
            AddRandom(result, "affix_first_trigger_bonus", "先声", "每场首次触发额外执行一次X%效果。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTrigger, "basicEffect", "on_first_trigger", "first_trigger_in_battle", "basisPoint", 3000);
            AddRandom(result, "affix_chain_target", "连应", "连续触发达到三次时额外影响一个目标。", ItemCandidateEffectCategory.TriggeredEffect, ItemCandidateEffectOperation.ExtraTarget, "targetCount", "on_consecutive_trigger", "chain_count_3", "count", 1);
            return result;
        }

        public static List<ItemBalanceWeightedAffix> BuildRandomPool(ItemBalanceProfile profile,
            IReadOnlyList<ItemCandidateAffixDefinition> dictionary)
        {
            string[] global = { "affix_nian_efficiency", "affix_cooldown_reduction", "affix_trigger_refund",
                "affix_direct_lit_power", "affix_core_next_power", "affix_first_trigger_bonus" };
            string role = profile.primaryStatId switch
            {
                "break" => "affix_break_up", "guard" => "affix_guard_up", "heal" => "affix_heal_up",
                "cleanse" => "affix_cleanse_up", "control" => "affix_control_up", _ => "affix_damage_up"
            };
            string spatial = profile.qiLeiTag == "fa" ? "affix_isolated_power"
                : profile.qiLeiTag == "yin" ? "affix_array_guard" : "affix_adjacent_damage";
            string conversion = profile.faMenTag == "xuanshui" ? "affix_overheal_to_guard"
                : profile.faMenTag == "zhongyue" ? "affix_guard_refund"
                : profile.faMenTag == "zhenlei" ? "affix_break_cooldown"
                : profile.faMenTag == "lihuo" ? "affix_damage_cost_trade" : "affix_debuff_target_power";
            return new[] { role, spatial, conversion }.Concat(global).Distinct(StringComparer.Ordinal)
                .Where(id => !string.Equals(id, profile.fixedAffixId, StringComparison.Ordinal))
                .Where(id => dictionary.Any(value => string.Equals(value.affixId, id, StringComparison.Ordinal)))
                .Take(8)
                .Select((id, index) => new ItemBalanceWeightedAffix
                {
                    affixId = id,
                    weight = Math.Max(1, 8 - index),
                    mutexGroupId = dictionary.First(value => value.affixId == id).mutexGroupId,
                    repeatPolicy = "NO_DUPLICATE",
                    designNote = profile.displayName + "定向候选池"
                }).ToList();
        }

        public static List<ItemCandidateBuildStageDefinition> BuildFaMenStages()
        {
            return BuildStages(new[]
            {
                ("zhenlei", "震雷法", "破盾提高", "破盾后追加雷击", "连锁命中额外目标", "break"),
                ("lihuo", "离火法", "余焰持续提高", "余焰叠层触发爆燃", "爆燃后保留火种", "damage"),
                ("zhongyue", "中岳法", "护势获得提高", "受击时反制并补护", "护势耗尽前分担伤害", "guard"),
                ("xuanshui", "玄水法", "净化与回复提高", "净化后推进冷却", "溢出回复转为护势", "heal"),
                ("taibai", "太白法", "对破绽目标增伤", "斩击后追加收割", "击破目标后贯穿连斩", "damage")
            });
        }

        public static List<ItemCandidateBuildStageDefinition> BuildQiLeiStages()
        {
            var seeds = new[]
            {
                ("fu", "符", "触发频率提高", "每轮追加一次符纸触发", "cooldown"),
                ("yin", "印", "护势与稳定性提高", "承伤后为同类印补护", "guard"),
                ("ling", "令", "控制强度提高", "号令额外影响一个目标", "control"),
                ("jing", "镜", "镜照效果提高", "复制一次关键状态变化", "basicEffect"),
                ("fa", "法", "耗念降低", "施法后推进全组冷却", "nianCost")
            };
            List<ItemCandidateBuildStageDefinition> result = new();
            foreach (var seed in seeds)
            for (int i = 0; i < 2; i++)
            {
                int stage = i == 0 ? 2 : 4;
                string id = "qilei:" + seed.Item1 + ":build" + stage;
                string desc = i == 0 ? seed.Item3 : seed.Item4;
                result.Add(Stage(id, seed.Item1, seed.Item2 + " Build" + stage, stage, desc,
                    i == 0 ? ItemCandidateEffectOperation.AddPercent : ItemCandidateEffectOperation.ExtraTrigger,
                    seed.Item5, i == 0 ? 600 : 1, i == 0 ? "basisPoint" : "count", "qilei_stage_" + stage));
            }
            return result;
        }

        public static long CalculateCandidateItemPower(ItemBalanceProfile profile,
            ItemBalanceRarityVersion version, IReadOnlyList<ItemCandidateItemPowerCoefficient> coefficients)
        {
            double stats = (version?.statRanges ?? new List<ItemBalanceRange>())
                .Where(value => value != null).Sum(value => (value.minUnits + value.maxUnits) * 0.5d);
            double statWeight = Coefficient(coefficients, "STAT_MIDPOINT", 1d);
            double signature = Coefficient(coefficients, "SIGNATURE_AFFIX", 12d);
            double core = Coefficient(coefficients, "VISIBLE_CORE", 8d) * (version?.visibleCoreEffectIds?.Count ?? 0);
            double build = Coefficient(coefficients, "BUILD_ELIGIBILITY", 6d) * Math.Max(0, version == null ? 0 : version.rarity.ToTierIndex());
            return Math.Max(1L, (long)Math.Round(stats * statWeight + signature + core + build,
                MidpointRounding.AwayFromZero));
        }

        public static List<ItemCandidateItemPowerCoefficient> DefaultPowerCoefficients() => new()
        {
            new() { coefficientId = "STAT_MIDPOINT", value = 1f, description = "四条基础属性中点总和系数" },
            new() { coefficientId = "SIGNATURE_AFFIX", value = 12f, description = "专属固定词条候选价值" },
            new() { coefficientId = "VISIBLE_CORE", value = 8f, description = "每个当前品阶可见核心的候选价值" },
            new() { coefficientId = "BUILD_ELIGIBILITY", value = 6f, description = "品阶Build资格潜力候选价值" }
        };

        private static List<ItemCandidateBuildStageDefinition> BuildStages(
            IEnumerable<(string tag, string name, string b2, string b4, string b6, string stat)> seeds)
        {
            List<ItemCandidateBuildStageDefinition> result = new();
            foreach (var seed in seeds)
            {
                result.Add(Stage("famen:" + seed.tag + ":build2", seed.tag, seed.name + " Build2", 2,
                    seed.b2, ItemCandidateEffectOperation.AddPercent, seed.stat, 800, "basisPoint", "famen_stage_2"));
                result.Add(Stage("famen:" + seed.tag + ":build4", seed.tag, seed.name + " Build4", 4,
                    seed.b4, ItemCandidateEffectOperation.ExtraTrigger, seed.stat, 3000, "basisPoint", "famen_stage_4"));
                result.Add(Stage("famen:" + seed.tag + ":build6", seed.tag, seed.name + " Build6", 6,
                    seed.b6, ItemCandidateEffectOperation.ExtraTarget, seed.stat, 1, "count", "famen_stage_6"));
            }
            return result;
        }

        private static ItemCandidateBuildStageDefinition Stage(string id, string tag, string name, int stage,
            string desc, ItemCandidateEffectOperation operation, string stat, long value, string unit, string condition)
        {
            return new ItemCandidateBuildStageDefinition
            {
                buildId = id, stableTag = tag, displayName = name, stagePieceCount = stage,
                description = desc + "（候选）", conditionText = "已点亮且具有对应Build资格的实例达到" + stage + "件。",
                effectPayload = Payload(id + ":effect", name, desc, ItemCandidateEffectCategory.BuildSynergyEffect,
                    operation, stat, "on_build_stage_active", condition, unit, value, 0, id.Replace(':', '_'), "NONE"),
                designNote = "Build阶段候选效果；不接正式Battle。"
            };
        }

        private static void AddRandom(List<ItemCandidateAffixDefinition> result, string id, string name,
            string description, ItemCandidateEffectCategory category, ItemCandidateEffectOperation operation,
            string stat, string trigger, string condition, string unit, long value, long secondary = 0)
        {
            string mutex = category == ItemCandidateEffectCategory.TradeoffEffect ? "TRADEOFF" : "NONE";
            result.Add(new ItemCandidateAffixDefinition
            {
                affixId = id, displayName = name, description = description,
                effectPayload = Payload(id + ":effect", name, description, category, operation, stat,
                    trigger, condition, unit, value, secondary, id + "_params", mutex),
                rarityRanges = Ranges(value, Math.Max(1, Math.Abs(value) / 3)),
                mutexGroupId = mutex, repeatPolicy = "NO_DUPLICATE",
                designNote = "共享随机词条候选；五品阶范围可编辑。"
            });
        }

        private static ItemCandidateAffixDefinition BuildNianEfficiencyCandidate()
        {
            const string id = "affix_nian_efficiency";
            const string name = "省念";
            const string description = "耗念降低X%。";
            return new ItemCandidateAffixDefinition
            {
                affixId = id,
                displayName = name,
                description = description,
                effectPayload = Payload(
                    id + ":effect",
                    name,
                    description,
                    ItemCandidateEffectCategory.ResourceEffect,
                    ItemCandidateEffectOperation.ReducePercent,
                    "nianCost",
                    "before_trigger",
                    "nian_cost_gt_0",
                    "basisPoint",
                    200,
                    0,
                    id + "_params",
                    "NONE"),
                rarityRanges = NianEfficiencyRanges(),
                mutexGroupId = "NONE",
                repeatPolicy = "NO_DUPLICATE",
                designNote = "共享随机词条候选；省念使用固定五品阶 BP 范围。"
            };
        }

        private static List<ItemCandidateRarityValue> NianEfficiencyRanges()
        {
            return new List<ItemCandidateRarityValue>
            {
                new() { rarity = ItemInstanceRarity.White, minUnits = 200, maxUnits = 400 },
                new() { rarity = ItemInstanceRarity.Green, minUnits = 350, maxUnits = 650 },
                new() { rarity = ItemInstanceRarity.Blue, minUnits = 550, maxUnits = 900 },
                new() { rarity = ItemInstanceRarity.Purple, minUnits = 800, maxUnits = 1300 },
                new() { rarity = ItemInstanceRarity.Orange, minUnits = 1200, maxUnits = 1800 }
            };
        }

        private static ItemCandidateEffectPayload Payload(string id, string name, string description,
            ItemCandidateEffectCategory category, ItemCandidateEffectOperation operation, string stat,
            string trigger, string condition, string unit, long value, long secondary, string parameters,
            string mutex)
        {
            return new ItemCandidateEffectPayload
            {
                effectId = id, displayName = name, description = description,
                effectCategory = category, triggerEventId = trigger, conditionId = condition,
                targetSelector = "effect_primary_target", targetStatId = stat, operation = operation,
                valueUnitKey = unit, valueUnits = value, secondaryValueUnits = secondary,
                durationUnits = 1, stackLimit = secondary > 1 && secondary < 100 ? (int)secondary : 1,
                internalCooldownUnits = 1, parameterProfileId = parameters, mutexGroupId = mutex,
                effectTags = new List<string> { category.ToString(), operation.ToString(), "DEV_ONLY" },
                designNote = "BALANCE_CANDIDATE / sandbox formatter payload; no formal battle executor."
            };
        }

        private static List<ItemCandidateRarityValue> Ranges(long baseValue, long step)
        {
            long sign = baseValue < 0 ? -1 : 1;
            long start = Math.Max(1, Math.Abs(baseValue));
            return ItemInstanceRarityCatalog.All.Select((rarity, index) => new ItemCandidateRarityValue
            {
                rarity = rarity.rarity,
                minUnits = sign * (start + step * index),
                maxUnits = sign * (start + step * (index + 1))
            }).ToList();
        }

        private static SignatureSeed S(string name, string description, ItemCandidateEffectCategory category,
            ItemCandidateEffectOperation operation, string stat, string trigger, string condition,
            string unit, long value, long secondary = 0) =>
            new(name, description, category, operation, stat, trigger, condition, unit, value, secondary);

        private static int ParseIndex(string itemId) => int.TryParse((itemId ?? string.Empty).TrimStart('I'),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 1;

        private static string CleanPreview(string value) => string.IsNullOrWhiteSpace(value)
            ? "点亮后发动本件道具的基础效果。"
            : value.Replace("预览", string.Empty).Trim(' ', '。') + "。";

        private static string Placement(string faMen, string qiLei, int cells)
        {
            string spatial = cells >= 3 ? "优先贴近阵位点边缘，给小件留出转向空间"
                : qiLei == "jing" ? "放在能接触两件同法门道具的位置"
                : qiLei == "yin" ? "优先占据阵位点并承接相邻接亮"
                : "让核心格靠近聚念范围，并保留正交相邻接亮位置";
            return spatial + "；当前" + faMen + "法门候选Build需要已点亮实例计数。";
        }

        private static double Coefficient(IReadOnlyList<ItemCandidateItemPowerCoefficient> values,
            string id, double fallback) => values?.FirstOrDefault(value => value != null
                && string.Equals(value.coefficientId, id, StringComparison.Ordinal))?.value ?? fallback;
    }
}
