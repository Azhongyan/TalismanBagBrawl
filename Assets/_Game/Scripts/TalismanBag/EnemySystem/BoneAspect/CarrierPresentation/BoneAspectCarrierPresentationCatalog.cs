using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.CarrierPresentation
{
    public static class BoneAspectCarrierPresentationCatalog
    {
        internal static readonly string[] EnemyBaseArtSlotKeys =
        {
            "view.normal_enemy.front_and_three_quarter",
            "view.normal_enemy.silhouette",
            "view.normal_enemy.primary_material",
            "view.normal_enemy.mechanic_marker",
            "view.normal_enemy.state_set",
            "view.normal_enemy.small_screen"
        };

        internal static readonly string[] BossBaseArtSlotKeys =
        {
            "view.boss.front",
            "view.boss.battle_three_quarter",
            "view.boss.phase_states",
            "view.boss.poster_reference_only"
        };

        public static BoneAspectCarrierPresentationCatalogInput CreateInput(
            bool reverseInputOrder = false)
        {
            List<BoneAspectCarrierPresentationSnapshot> carriers = CreateLockedCarrierBlueprints()
                .Select(value => value.Clone())
                .ToList();
            List<BoneAspectArtDeliverySlotSnapshot> artSlots = CreateArtDeliverySlots(carriers);
            List<BoneAspectMechanicReferenceSnapshot> mechanicReferences =
                CreateMechanicReferences(carriers);
            List<BoneAspectUserDecisionReferenceSnapshot> decisions =
                CreateDecisionReferences();

            if (reverseInputOrder)
            {
                carriers.Reverse();
                artSlots.Reverse();
                mechanicReferences.Reverse();
                decisions.Reverse();
                foreach (BoneAspectCarrierPresentationSnapshot carrier in carriers)
                {
                    // Decision identifiers are canonicalized independently of caller order.
                    carrier.DecisionIds.Reverse().ToArray();
                }
            }

            return new BoneAspectCarrierPresentationCatalogInput(
                carriers,
                artSlots,
                mechanicReferences,
                decisions);
        }

        internal static IReadOnlyList<BoneAspectCarrierPresentationSnapshot>
            CreateLockedCarrierBlueprints()
        {
            return new[]
            {
                Carrier(
                    "bone_aspect_enemy_c1_01_shattered_host",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_1",
                    "碎骨附身者",
                    "enemy.bone_aspect.c1.shattered_host.name",
                    "enemy.bone_aspect.c1.shattered_host",
                    "bone_aspect_visual_c1_bone_porcelain",
                    "enemy.bone_aspect.c1.shattered_host.silhouette",
                    "enemy.bone_aspect.c1.shattered_host.recognition",
                    "街坊残影、一处骨瓷覆盖、错误身份影子",
                    "基础攻击、附身状态、死亡后短暂骨粉区域"),
                Carrier(
                    "bone_aspect_enemy_c1_02_porcelain_hound",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_1",
                    "骨瓷犬",
                    "enemy.bone_aspect.c1.porcelain_hound.name",
                    "enemy.bone_aspect.c1.porcelain_hound",
                    "bone_aspect_visual_c1_bone_porcelain",
                    "enemy.bone_aspect.c1.porcelain_hound.silhouette",
                    "enemy.bone_aspect.c1.porcelain_hound.recognition",
                    "低矮镇物兽、大块骨瓷硬壳、符绳",
                    "骨瓷护壳、冲撞、破壳后短暂虚弱"),
                Carrier(
                    "bone_aspect_enemy_c1_03_bone_swap_remnant",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_1",
                    "换骨残相",
                    "enemy.bone_aspect.c1.bone_swap_remnant.name",
                    "enemy.bone_aspect.c1.bone_swap_remnant",
                    "bone_aspect_visual_c1_bone_porcelain",
                    "enemy.bone_aspect.c1.bone_swap_remnant.silhouette",
                    "enemy.bone_aspect.c1.bone_swap_remnant.recognition",
                    "一个主轮廓、两层身份叠影、未干骨瓷釉面",
                    "状态叠加、低血量分裂、复制一种简单能力",
                    "BA-D3"),
                Carrier(
                    "bone_aspect_enemy_c2_01_false_identity_bone",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_2",
                    "假相浮骨",
                    "enemy.bone_aspect.c2.false_identity_bone.name",
                    "enemy.bone_aspect.c2.false_identity_bone",
                    "bone_aspect_visual_c2_contract_identity",
                    "enemy.bone_aspect.c2.false_identity_bone.silhouette",
                    "enemy.bone_aspect.c2.false_identity_bone.recognition",
                    "漂浮骨牌、外层假身份、背面真实核心",
                    "制造错误目标、交换真假位置、识破后快速失去防御"),
                Carrier(
                    "bone_aspect_enemy_c2_02_contract_clerk",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_2",
                    "契纸吏",
                    "enemy.bone_aspect.c2.contract_clerk.name",
                    "enemy.bone_aspect.c2.contract_clerk",
                    "bone_aspect_visual_c2_contract_identity",
                    "enemy.bone_aspect.c2.contract_clerk.silhouette",
                    "enemy.bone_aspect.c2.contract_clerk.recognition",
                    "废契纸执法人形、印章头部、长封条",
                    "封招、读条、打断玩家一个效果"),
                Carrier(
                    "bone_aspect_enemy_c2_03_bone_bidder",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_2",
                    "竞骨客",
                    "enemy.bone_aspect.c2.bone_bidder.name",
                    "enemy.bone_aspect.c2.bone_bidder",
                    "bone_aspect_visual_c2_contract_identity",
                    "enemy.bone_aspect.c2.bone_bidder.silhouette",
                    "enemy.bone_aspect.c2.bone_bidder.recognition",
                    "单一错误人生主题、竞价身份标记",
                    "多目标竞争、抢夺增益、争抢出价标记"),
                Carrier(
                    "bone_aspect_enemy_c3_01_array_stone_puppet",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_3",
                    "残阵石傀",
                    "enemy.bone_aspect.c3.array_stone_puppet.name",
                    "enemy.bone_aspect.c3.array_stone_puppet",
                    "bone_aspect_visual_c3_array_stone_memory",
                    "enemy.bone_aspect.c3.array_stone_puppet.silhouette",
                    "enemy.bone_aspect.c3.array_stone_puppet.recognition",
                    "厚重阵石人形、胸肩护板、核心位置",
                    "高护盾、减伤、破盾后核心暴露"),
                Carrier(
                    "bone_aspect_enemy_c3_02_old_edict_shadow",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_3",
                    "旧令道影",
                    "enemy.bone_aspect.c3.old_edict_shadow.name",
                    "enemy.bone_aspect.c3.old_edict_shadow",
                    "bone_aspect_visual_c3_array_stone_memory",
                    "enemy.bone_aspect.c3.old_edict_shadow.silhouette",
                    "enemy.bone_aspect.c3.old_edict_shadow.recognition",
                    "无脸弟子残影、止步/返回令文贯穿身体",
                    "打断、压制、限制技能触发"),
                Carrier(
                    "bone_aspect_enemy_c3_03_bone_dust_effigy",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_3",
                    "骨尘小像",
                    "enemy.bone_aspect.c3.bone_dust_effigy.name",
                    "enemy.bone_aspect.c3.bone_dust_effigy",
                    "bone_aspect_visual_c3_array_stone_memory",
                    "enemy.bone_aspect.c3.bone_dust_effigy.silhouette",
                    "enemy.bone_aspect.c3.bone_dust_effigy.recognition",
                    "骨粉微型人形、偷来的小型符号碎片",
                    "偷念、复制、短暂模仿玩家一个效果",
                    "BA-D3"),
                Carrier(
                    "bone_aspect_enemy_c4_01_faceless_bone_figure",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_4",
                    "无面骨俑",
                    "enemy.bone_aspect.c4.faceless_bone_figure.name",
                    "enemy.bone_aspect.c4.faceless_bone_figure",
                    "bone_aspect_visual_c4_counterfeit_composite",
                    "enemy.bone_aspect.c4.faceless_bone_figure.silhouette",
                    "enemy.bone_aspect.c4.faceless_bone_figure.recognition",
                    "平整骨瓷面、统一人形、少量职业残片",
                    "成群压迫、数量优势、死亡后短暂骨纹区域"),
                Carrier(
                    "bone_aspect_enemy_c4_02_contract_attendant",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_4",
                    "骨契商侍",
                    "enemy.bone_aspect.c4.contract_attendant.name",
                    "enemy.bone_aspect.c4.contract_attendant",
                    "bone_aspect_visual_c4_counterfeit_composite",
                    "enemy.bone_aspect.c4.contract_attendant.silhouette",
                    "enemy.bone_aspect.c4.contract_attendant.recognition",
                    "契纸绳结分段、封存盒或契印",
                    "封存道具效果、锁定一个器类、持续读契"),
                Carrier(
                    "bone_aspect_enemy_c4_03_false_celestial_form",
                    BoneAspectCarrierKind.Enemy,
                    "bone_aspect_chapter_4",
                    "天骨赝相",
                    "enemy.bone_aspect.c4.false_celestial_form.name",
                    "enemy.bone_aspect.c4.false_celestial_form",
                    "bone_aspect_visual_c4_counterfeit_composite",
                    "enemy.bone_aspect.c4.false_celestial_form.silhouette",
                    "enemy.bone_aspect.c4.false_celestial_form.recognition",
                    "错误天师主轮廓、两类法门符号、结构不完整",
                    "复合法门攻击、切换两种属性、预演最终 Boss 机制"),
                Carrier(
                    "bone_aspect_boss_c1_bone_guard",
                    BoneAspectCarrierKind.Boss,
                    "bone_aspect_chapter_1",
                    "守骨奴",
                    "boss.bone_aspect.c1.bone_guard.name",
                    "boss.bone_aspect.c1.bone_guard",
                    "bone_aspect_visual_c1_bone_porcelain",
                    "boss.bone_aspect.c1.bone_guard.silhouette",
                    "boss.bone_aspect.c1.bone_guard.recognition",
                    "宽肩护主轮廓、胸口巨大“忠”字骨铭",
                    "骨瓷护壳、保护盲眼女人、破壳后“忠”字骨铭核心暴露；禁止召唤和分裂",
                    "BA-D1"),
                Carrier(
                    "bone_aspect_boss_c2_identity_bidder",
                    BoneAspectCarrierKind.Boss,
                    "bone_aspect_chapter_2",
                    "竞骨人",
                    "boss.bone_aspect.c2.identity_bidder.name",
                    "boss.bone_aspect.c2.identity_bidder",
                    "bone_aspect_visual_c2_contract_identity",
                    "boss.bone_aspect.c2.identity_bidder.silhouette",
                    "boss.bone_aspect.c2.identity_bidder.recognition",
                    "温吞老人本体、书生/将军/富商三层身份、骨契册",
                    "书生/将军/富商身份切换、弱点随身份变化、翻阅骨契册时暴露打断窗口；辅助机制二选一",
                    "BA-D2"),
                Carrier(
                    "bone_aspect_boss_c3_array_eye_guardian",
                    BoneAspectCarrierKind.Boss,
                    "bone_aspect_chapter_3",
                    "阵眼守护者",
                    "boss.bone_aspect.c3.array_eye_guardian.name",
                    "boss.bone_aspect.c3.array_eye_guardian",
                    "bone_aspect_visual_c3_array_stone_memory",
                    "boss.bone_aspect.c3.array_eye_guardian.silhouette",
                    "boss.bone_aspect.c3.array_eye_guardian.recognition",
                    "碑形门框轮廓、半石半符线、胸口骨相空位",
                    "石相防守、符影进攻、空位阶段暴露阵眼；结局为停止攻击并让路，不是普通死亡"),
                Carrier(
                    "bone_aspect_boss_c4_myriad_bone_beast",
                    BoneAspectCarrierKind.Boss,
                    "bone_aspect_chapter_4",
                    "万相骨兽",
                    "boss.bone_aspect.c4.myriad_bone_beast.name",
                    "boss.bone_aspect.c4.myriad_bone_beast",
                    "bone_aspect_visual_c4_counterfeit_composite",
                    "boss.bone_aspect.c4.myriad_bone_beast.silhouette",
                    "boss.bone_aspect.c4.myriad_bone_beast.recognition",
                    "巨兽主轮廓、胸腹交易口、中央骨商核心",
                    "交易封存、两种能力形态切换、空白骨片解除封存并进入输出阶段、记忆回流使骨瓷结构崩解",
                    "BA-D4")
            };
        }

        internal static List<BoneAspectArtDeliverySlotSnapshot> CreateArtDeliverySlots(
            IEnumerable<BoneAspectCarrierPresentationSnapshot> carriers)
        {
            List<BoneAspectArtDeliverySlotSnapshot> rows =
                new List<BoneAspectArtDeliverySlotSnapshot>();
            foreach (BoneAspectCarrierPresentationSnapshot carrier in carriers)
            {
                string[] keys = carrier.CarrierKind == BoneAspectCarrierKind.Enemy
                    ? EnemyBaseArtSlotKeys
                    : BossBaseArtSlotKeys;
                rows.AddRange(keys.Select(key => new BoneAspectArtDeliverySlotSnapshot(
                    carrier.ContentId,
                    key,
                    BoneAspectArtDeliveryStatus.RequiredByLockedDesign,
                    string.Empty,
                    string.Empty)));
            }

            rows.Add(BlockedSlot(
                "bone_aspect_boss_c1_bone_guard",
                "view.boss.deferred.protected_target_execution_state",
                "BA-D1"));
            rows.Add(BlockedSlot(
                "bone_aspect_boss_c2_identity_bidder",
                "view.boss.deferred.auxiliary_mechanic_variant",
                "BA-D2"));
            rows.Add(BlockedSlot(
                "bone_aspect_enemy_c1_03_bone_swap_remnant",
                "view.normal_enemy.deferred.copy_execution_state",
                "BA-D3"));
            rows.Add(BlockedSlot(
                "bone_aspect_enemy_c3_03_bone_dust_effigy",
                "view.normal_enemy.deferred.copy_execution_state",
                "BA-D3"));
            rows.Add(BlockedSlot(
                "bone_aspect_boss_c4_myriad_bone_beast",
                "view.boss.deferred.seal_target_and_form_variants",
                "BA-D4"));
            return rows;
        }

        internal static List<BoneAspectMechanicReferenceSnapshot> CreateMechanicReferences(
            IEnumerable<BoneAspectCarrierPresentationSnapshot> carriers)
        {
            Dictionary<string, string> commitmentByCarrier = carriers.ToDictionary(
                value => value.ContentId,
                value => value.MechanicCommitmentId,
                StringComparer.Ordinal);
            List<BoneAspectMechanicReferenceSnapshot> rows =
                new List<BoneAspectMechanicReferenceSnapshot>();

            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c1_01_shattered_host",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.polluted_tile"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c1_02_porcelain_hound",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.layered_shield"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.shell_break"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c1_03_bone_swap_remnant",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.swarm_summon"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c2_01_false_identity_bone",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.cleanse_reveal"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c2_02_contract_clerk",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.long_cast"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.talisman_seal"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.interrupt_stagger"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c2_03_bone_bidder",
                Ref(EnemyVocabularyCategory.PressureChannel, "pressure.multi_target"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c3_01_array_stone_puppet",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.layered_shield"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.shell_break"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c3_02_old_edict_shadow",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.long_cast"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.talisman_seal"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.interrupt_stagger"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c3_03_bone_dust_effigy",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c4_01_faceless_bone_figure",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.swarm_summon"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.polluted_tile"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c4_02_contract_attendant",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.long_cast"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.talisman_seal"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.interrupt_stagger"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_enemy_c4_03_false_celestial_form",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.burst_spike"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_boss_c1_bone_guard",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.layered_shield"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.shell_break"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_boss_c2_identity_bidder",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.long_cast"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.interrupt_stagger"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_boss_c3_array_eye_guardian",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.layered_shield"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.formation_eye_disruption"),
                Ref(EnemyVocabularyCategory.CounterWindowType, "counter_window.shell_break"));
            AddRefs(rows, commitmentByCarrier, "bone_aspect_boss_c4_myriad_bone_beast",
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.talisman_seal"),
                Ref(EnemyVocabularyCategory.Mechanic, "mechanic.burst_spike"));
            return rows;
        }

        internal static List<BoneAspectUserDecisionReferenceSnapshot> CreateDecisionReferences()
        {
            return new List<BoneAspectUserDecisionReferenceSnapshot>
            {
                Decision(
                    "BA-D1",
                    "守骨奴的“替盲眼女人承担伤害”是否存在真实可受击保护目标",
                    "bone_aspect_boss_c1_bone_guard"),
                Decision(
                    "BA-D2",
                    "竞骨人辅助机制选择“虚假分身”或“竞价护盾”",
                    "bone_aspect_boss_c2_identity_bidder"),
                Decision(
                    "BA-D3",
                    "换骨残相与骨尘小像的 Copy 范围",
                    "bone_aspect_enemy_c1_03_bone_swap_remnant",
                    "bone_aspect_enemy_c3_03_bone_dust_effigy"),
                Decision(
                    "BA-D4",
                    "万相骨兽封存对象与两种正式形态",
                    "bone_aspect_boss_c4_myriad_bone_beast")
            };
        }

        private static BoneAspectCarrierPresentationSnapshot Carrier(
            string contentId,
            BoneAspectCarrierKind kind,
            string chapterId,
            string displayName,
            string nameLocalizationKey,
            string presentationKey,
            string visualFamilyId,
            string primarySilhouetteKey,
            string coreRecognitionKey,
            string coreRecognitionPoint,
            string mechanicCommitmentSummary,
            params string[] decisionIds)
        {
            return new BoneAspectCarrierPresentationSnapshot(
                contentId,
                kind,
                chapterId,
                displayName,
                nameLocalizationKey,
                presentationKey + ".description",
                presentationKey,
                visualFamilyId,
                primarySilhouetteKey,
                coreRecognitionKey,
                coreRecognitionPoint,
                mechanicCommitmentSummary,
                contentId + ".mechanic_commitment",
                true,
                decisionIds);
        }

        private static BoneAspectArtDeliverySlotSnapshot BlockedSlot(
            string carrierId,
            string slotKey,
            string decisionId)
        {
            return new BoneAspectArtDeliverySlotSnapshot(
                carrierId,
                slotKey,
                BoneAspectArtDeliveryStatus.BlockedByUserDecision,
                decisionId,
                string.Empty);
        }

        private static BoneAspectUserDecisionReferenceSnapshot Decision(
            string decisionId,
            string title,
            params string[] carrierIds)
        {
            return new BoneAspectUserDecisionReferenceSnapshot(
                decisionId,
                title,
                carrierIds,
                "USER_DECISION_REQUIRED",
                "NOT_SELECTED",
                string.Empty,
                false);
        }

        private static KeyValuePair<EnemyVocabularyCategory, string> Ref(
            EnemyVocabularyCategory category,
            string stableKey)
        {
            return new KeyValuePair<EnemyVocabularyCategory, string>(category, stableKey);
        }

        private static void AddRefs(
            ICollection<BoneAspectMechanicReferenceSnapshot> rows,
            IReadOnlyDictionary<string, string> commitmentByCarrier,
            string carrierId,
            params KeyValuePair<EnemyVocabularyCategory, string>[] references)
        {
            foreach (KeyValuePair<EnemyVocabularyCategory, string> reference in references)
            {
                rows.Add(new BoneAspectMechanicReferenceSnapshot(
                    carrierId,
                    reference.Key,
                    reference.Value,
                    BoneAspectMechanicReferenceBindingStatus.CandidateReuseOnly,
                    false,
                    commitmentByCarrier[carrierId]));
            }
        }
    }
}
