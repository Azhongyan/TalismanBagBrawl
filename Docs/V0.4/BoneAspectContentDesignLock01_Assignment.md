# V0.4-BoneAspectContentDesignLock01 Guard Assignment

状态：`ENEMY_GUARD_ASSIGNMENT_BONEASPECTCONTENTDESIGNLOCK01 / READY_FOR_REPORT_ONLY_CONTENT_LOCK`
总体归属复核：`SYSTEM_OWNERSHIP_SPLIT_GUARD_BONEASPECT_DECISION / FEASIBLE_WITH_CHANGES`
风险：`YELLOW / CONTENT_IDENTITY_AND_SEMANTIC_LOCK_ONLY`
维护方：Enemy / Boss / Encounter System Guard
日期：2026-07-23

## 0. 包定位

本包是《骨相》Enemy 数据线 P0，只把用户已经提供的首版本策划整理为稳定、可核验、可供后续数据包引用的内容规格。

本包锁定：

```text
12 个普通怪身份
4 个 Boss 身份
12 个 Encounter 场景节点
四章视觉家族与统一视觉语言
稳定 contentId / presentationKey
每个敌人和 Boss 已承诺的机制现象
四项 USER_DECISION_REQUIRED
```

本包不创建 Runtime 数据对象，不接 E01-E10 Provider，不修改任何既有 Enemy 事实源，不实现技能、伤害、状态、波次、阶段或场景。

这里的“机制题”仍表示遭遇对玩家 Build 和操作的检验，不是试卷、选择题、答题 UI 或完整答案题库。

## 1. 启动必读

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
Docs/V0.4/BoneAspectContentDesignLock01_Assignment.md
```

只读参考：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/**
Docs/V0.4/Reports/EnemyDomainDataContract*.*
Docs/V0.4/Reports/EnemyMechanicVocabulary*.*
Docs/V0.4/Reports/EnemyValidation*.*
Docs/V0.4/Reports/EncounterCompositionSchema*.*
Docs/V0.4/Reports/EnemySkillBossPhaseSchema*.*
Docs/V0.4/Reports/CounterWindowAndPressureSchema*.*
Docs/V0.4/Reports/DevEncounterSeed*.*
```

用户策划原始输入：

```text
C:\Users\Ella\.codex\attachments\39b477b4-cf82-4285-b587-0bc55040f032\pasted-text.txt
SHA-256:
cd3d72ec7339b2f0118e765d929f3ca102a6bec4efeaf9ffe45c94abd23fc21e
```

若原始输入无法读取或哈希不一致，必须停止并返回 `SOURCE_INPUT_BLOCKED`，不得凭聊天记忆补写。

## 2. 当前架构裁定

本包必须遵守：

```text
Carrier / Presentation = 玩家在看谁
Mechanic Profile = 敌人带来什么机制压力
Skill / CounterWindow / BossPhase = 技能、反制窗口与阶段数据
Encounter / Wave / Slot / MapRule = 遭遇编排与环境压力
```

Carrier 与 Mechanic 为多对多。不得因为《骨相》新增 16 个叙事身份，就为每个身份复制一套已有护壳、封印、长施法、群体或状态机制。

本包只声明策划承诺和后续映射候选，不决定最终 Vocabulary key，不写 E03 binding，不修改 E10。

## 3. 隔离状态

全部内容规格必须固定：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
formalRouteBound = false
runtimeImplemented = false
```

`1-1` 至 `4-10` 只作为策划章节和场景范围标签，不是正式 StageConfig、RunFlow、SaveData、Reward、BossConfig 或 Chapter 路由。

不得把《骨相》内容直接覆盖：

```text
正式 1-10 破阵妖将
正式 2-10 既有 Boss 流程
E10 的四个 devOnly 3-10 / 4-10 验证 Seed
```

## 4. 稳定视觉家族

必须锁定以下四个视觉家族：

| chapterId | visualFamilyId | 名称 | 核心材质 | 主要叙事 |
| --- | --- | --- | --- | --- |
| `bone_aspect_chapter_1` | `bone_aspect_visual_c1_bone_porcelain` | 骨瓷与旧街 | 骨瓷、旧木、符绳、青石、暗朱砂骨纹 | 骨相从熟悉老街的旧物内部渗出 |
| `bone_aspect_chapter_2` | `bone_aspect_visual_c2_contract_identity` | 契纸与错误身份 | 契纸、旧钞、骨牌、反写招牌、身份叠影 | 身份、才华和命运被交易与定价 |
| `bone_aspect_chapter_3` | `bone_aspect_visual_c3_array_stone_memory` | 阵石与记忆叠影 | 阵石、残符、山雾、朱砂显影、记忆叠影 | 明箓山旧阵、资格判断与残缺记忆 |
| `bone_aspect_chapter_4` | `bone_aspect_visual_c4_counterfeit_composite` | 赝骨与复合法相 | 骨瓷、契纸、重复旧木结构、骨环、空白骨片 | 被夺身份汇聚为不可定价的终局冲突 |

全局视觉语言固定：

```text
中国志怪现代化
青石坊现实生活底色
骨瓷、骨片、契纸、阵纹形成版本识别
骨瓷白 / 旧纸黄 / 旧木褐 / 暗朱砂红 / 青石灰
大轮廓 / 高对比 / 手机小屏可读
一怪一个主机制标识
同章节共享材质家族
Boss 具有单一远距离记忆点
机制状态使用分层图，不烧入基础立绘
```

禁止视觉方向：

```text
欧美骷髅兵或西式墓园
恶魔祭坛或克苏鲁血肉
大量裸露骨架、器官或血腥爆炸
普通古风修仙、明亮仙山旅游图
赛博实验室
满身随机骨刺、人脸和无意义碎细节
全部场景使用同一暗色调
把 UI、技能答案、机制阈值或正式状态烧进图片
```

## 5. 12 个普通怪稳定身份

必须恰好建立以下 12 行，不得新增第 13 个普通怪：

| contentId | chapterId | displayName | presentationKey | visualFamilyId | 核心识别点 |
| --- | --- | --- | --- | --- | --- |
| `bone_aspect_enemy_c1_01_shattered_host` | `bone_aspect_chapter_1` | 碎骨附身者 | `enemy.bone_aspect.c1.shattered_host` | `bone_aspect_visual_c1_bone_porcelain` | 街坊残影、一处骨瓷覆盖、错误身份影子 |
| `bone_aspect_enemy_c1_02_porcelain_hound` | `bone_aspect_chapter_1` | 骨瓷犬 | `enemy.bone_aspect.c1.porcelain_hound` | `bone_aspect_visual_c1_bone_porcelain` | 低矮镇物兽、大块骨瓷硬壳、符绳 |
| `bone_aspect_enemy_c1_03_bone_swap_remnant` | `bone_aspect_chapter_1` | 换骨残相 | `enemy.bone_aspect.c1.bone_swap_remnant` | `bone_aspect_visual_c1_bone_porcelain` | 一个主轮廓、两层身份叠影、未干骨瓷釉面 |
| `bone_aspect_enemy_c2_01_false_identity_bone` | `bone_aspect_chapter_2` | 假相浮骨 | `enemy.bone_aspect.c2.false_identity_bone` | `bone_aspect_visual_c2_contract_identity` | 漂浮骨牌、外层假身份、背面真实核心 |
| `bone_aspect_enemy_c2_02_contract_clerk` | `bone_aspect_chapter_2` | 契纸吏 | `enemy.bone_aspect.c2.contract_clerk` | `bone_aspect_visual_c2_contract_identity` | 废契纸执法人形、印章头部、长封条 |
| `bone_aspect_enemy_c2_03_bone_bidder` | `bone_aspect_chapter_2` | 竞骨客 | `enemy.bone_aspect.c2.bone_bidder` | `bone_aspect_visual_c2_contract_identity` | 单一错误人生主题、竞价身份标记 |
| `bone_aspect_enemy_c3_01_array_stone_puppet` | `bone_aspect_chapter_3` | 残阵石傀 | `enemy.bone_aspect.c3.array_stone_puppet` | `bone_aspect_visual_c3_array_stone_memory` | 厚重阵石人形、胸肩护板、核心位置 |
| `bone_aspect_enemy_c3_02_old_edict_shadow` | `bone_aspect_chapter_3` | 旧令道影 | `enemy.bone_aspect.c3.old_edict_shadow` | `bone_aspect_visual_c3_array_stone_memory` | 无脸弟子残影、止步/返回令文贯穿身体 |
| `bone_aspect_enemy_c3_03_bone_dust_effigy` | `bone_aspect_chapter_3` | 骨尘小像 | `enemy.bone_aspect.c3.bone_dust_effigy` | `bone_aspect_visual_c3_array_stone_memory` | 骨粉微型人形、偷来的小型符号碎片 |
| `bone_aspect_enemy_c4_01_faceless_bone_figure` | `bone_aspect_chapter_4` | 无面骨俑 | `enemy.bone_aspect.c4.faceless_bone_figure` | `bone_aspect_visual_c4_counterfeit_composite` | 平整骨瓷面、统一人形、少量职业残片 |
| `bone_aspect_enemy_c4_02_contract_attendant` | `bone_aspect_chapter_4` | 骨契商侍 | `enemy.bone_aspect.c4.contract_attendant` | `bone_aspect_visual_c4_counterfeit_composite` | 契纸绳结分段、封存盒或契印 |
| `bone_aspect_enemy_c4_03_false_celestial_form` | `bone_aspect_chapter_4` | 天骨赝相 | `enemy.bone_aspect.c4.false_celestial_form` | `bone_aspect_visual_c4_counterfeit_composite` | 错误天师主轮廓、两类法门符号、结构不完整 |

这些 ID 一旦 P0 验收即作为 P1 Carrier Catalog 的输入规范。后续显示名可以走本地化，但不得因为文案微调重建 contentId。

## 6. 4 个 Boss 稳定身份

必须恰好建立以下 4 行：

| contentId | chapterId | displayName | presentationKey | visualFamilyId | 远距离识别核心 |
| --- | --- | --- | --- | --- | --- |
| `bone_aspect_boss_c1_bone_guard` | `bone_aspect_chapter_1` | 守骨奴 | `boss.bone_aspect.c1.bone_guard` | `bone_aspect_visual_c1_bone_porcelain` | 宽肩护主轮廓、胸口巨大“忠”字骨铭 |
| `bone_aspect_boss_c2_identity_bidder` | `bone_aspect_chapter_2` | 竞骨人 | `boss.bone_aspect.c2.identity_bidder` | `bone_aspect_visual_c2_contract_identity` | 温吞老人本体、书生/将军/富商三层身份、骨契册 |
| `bone_aspect_boss_c3_array_eye_guardian` | `bone_aspect_chapter_3` | 阵眼守护者 | `boss.bone_aspect.c3.array_eye_guardian` | `bone_aspect_visual_c3_array_stone_memory` | 碑形门框轮廓、半石半符线、胸口骨相空位 |
| `bone_aspect_boss_c4_myriad_bone_beast` | `bone_aspect_chapter_4` | 万相骨兽 | `boss.bone_aspect.c4.myriad_bone_beast` | `bone_aspect_visual_c4_counterfeit_composite` | 巨兽主轮廓、胸腹交易口、中央骨商核心 |

固定禁止：

```text
守骨奴不得变成普通骷髅将军或西式亡灵骑士
竞骨人不得增加第四种身份，不得做三头六臂
阵眼守护者不得被描述为纯邪恶击杀目标
万相骨兽不得做传统饕餮复刻、满身人脸或血肉爆炸
```

## 7. 12 个 Encounter 场景节点

这些是内容节点，不是 Unity Scene 文件名，不得创建 Scene 或修改 BuildSettings。

| nodeId | chapterId | stageRangeLabel | displayName | visualFamilyId | 核心作用 |
| --- | --- | --- | --- | --- | --- |
| `bone_aspect_node_c1_01_ding_lane_outer` | `bone_aspect_chapter_1` | `1-1—1-3` | 青石坊丁巷外街 | `bone_aspect_visual_c1_bone_porcelain` | 正常老街中出现第一点身份异常 |
| `bone_aspect_node_c1_02_bone_shop_outer_lane` | `bone_aspect_chapter_1` | `1-4—1-7` | 骨铺外巷 | `bone_aspect_visual_c1_bone_porcelain` | 建立骨铺入口与骨瓷章节标志 |
| `bone_aspect_node_c1_03_bone_shop_inner_hall` | `bone_aspect_chapter_1` | `1-8—1-10` | 骨铺内堂 | `bone_aspect_visual_c1_bone_porcelain` | 从现实老街进入规则明确的骨相交易空间 |
| `bone_aspect_node_c2_01_parking_entrance` | `bone_aspect_chapter_2` | `2-1—2-3` | 地下停车场入口 | `bone_aspect_visual_c2_contract_identity` | 现代地下空间逐步显露反向交易规则 |
| `bone_aspect_node_c2_02_rakshasa_market_street` | `bone_aspect_chapter_2` | `2-4—2-7` | 罗刹海市内街 | `bone_aspect_visual_c2_contract_identity` | 将身份和命运作为公开商品展示 |
| `bone_aspect_node_c2_03_bone_auction_hall` | `bone_aspect_chapter_2` | `2-8—2-10` | 骨相拍卖厅 | `bone_aspect_visual_c2_contract_identity` | 将换骨推至公开竞价高潮 |
| `bone_aspect_node_c3_01_abandoned_mountain_path` | `bone_aspect_chapter_3` | `3-1—3-3` | 荒废山道 | `bone_aspect_visual_c3_array_stone_memory` | 沿林照灯少年时期旧路进入旧案 |
| `bone_aspect_node_c3_02_collapsed_mountain_gate` | `bone_aspect_chapter_3` | `3-4—3-7` | 倒塌山门 | `bone_aspect_visual_c3_array_stone_memory` | 建立明箓山规则压力与历史重量 |
| `bone_aspect_node_c3_03_memory_hall_array_eye` | `bone_aspect_chapter_3` | `3-8—3-10` | 记忆大殿·阵眼层 | `bone_aspect_visual_c3_array_stone_memory` | 揭示骨相与明箓山旧阵的关系 |
| `bone_aspect_node_c4_01_sealed_ding_lane_defense` | `bone_aspect_chapter_4` | `4-1—4-3` | 封锁丁巷·街坊防线 | `bone_aspect_visual_c4_counterfeit_composite` | 让青石坊人情与群体压力同时出现 |
| `bone_aspect_node_c4_02_transformed_bone_shop` | `bone_aspect_chapter_4` | `4-4—4-7` | 异化骨铺 | `bone_aspect_visual_c4_counterfeit_composite` | 进入骨商真正控制的交易系统 |
| `bone_aspect_node_c4_03_myriad_bone_belly` | `bone_aspect_chapter_4` | `4-8—4-10` | 万相骨腹 | `bone_aspect_visual_c4_counterfeit_composite` | 将空白骨片命题与最终冲突汇合 |

每章三个节点必须共享资产家族。报告必须明确：

```text
12 个节点 != 12 套完全独立资产
节点范围标签 != 正式关卡路由
场景母图 != MapRule 真源
Scene / Prefab != Encounter / Wave / Mechanic 真源
```

## 8. 机制承诺

报告必须逐项保留以下玩家可观察承诺，不得擅自删减或升级成实现：

| contentId | 已锁机制承诺 |
| --- | --- |
| `bone_aspect_enemy_c1_01_shattered_host` | 基础攻击、附身状态、死亡后短暂骨粉区域 |
| `bone_aspect_enemy_c1_02_porcelain_hound` | 骨瓷护壳、冲撞、破壳后短暂虚弱 |
| `bone_aspect_enemy_c1_03_bone_swap_remnant` | 状态叠加、低血量分裂、复制一种简单能力 |
| `bone_aspect_enemy_c2_01_false_identity_bone` | 制造错误目标、交换真假位置、识破后快速失去防御 |
| `bone_aspect_enemy_c2_02_contract_clerk` | 封招、读条、打断玩家一个效果 |
| `bone_aspect_enemy_c2_03_bone_bidder` | 多目标竞争、抢夺增益、争抢出价标记 |
| `bone_aspect_enemy_c3_01_array_stone_puppet` | 高护盾、减伤、破盾后核心暴露 |
| `bone_aspect_enemy_c3_02_old_edict_shadow` | 打断、压制、限制技能触发 |
| `bone_aspect_enemy_c3_03_bone_dust_effigy` | 偷念、复制、短暂模仿玩家一个效果 |
| `bone_aspect_enemy_c4_01_faceless_bone_figure` | 成群压迫、数量优势、死亡后短暂骨纹区域 |
| `bone_aspect_enemy_c4_02_contract_attendant` | 封存道具效果、锁定一个器类、持续读契 |
| `bone_aspect_enemy_c4_03_false_celestial_form` | 复合法门攻击、切换两种属性、预演最终 Boss 机制 |
| `bone_aspect_boss_c1_bone_guard` | 骨瓷护壳、保护盲眼女人、破壳后“忠”字骨铭核心暴露；禁止召唤和分裂 |
| `bone_aspect_boss_c2_identity_bidder` | 书生/将军/富商身份切换、弱点随身份变化、翻阅骨契册时暴露打断窗口；辅助机制二选一 |
| `bone_aspect_boss_c3_array_eye_guardian` | 石相防守、符影进攻、空位阶段暴露阵眼；结局为停止攻击并让路，不是普通死亡 |
| `bone_aspect_boss_c4_myriad_bone_beast` | 交易封存、两种能力形态切换、空白骨片解除封存并进入输出阶段、记忆回流使骨瓷结构崩解 |

这些是策划承诺，不等于：

```text
已存在 Vocabulary key
已存在 Runtime Producer
已存在 Battle Executor
已存在 Item/Board 修改能力
已存在 HP、伤害、阈值、持续时间或平衡数值
已存在正式章节入口
```

`BoneAspectMechanicGapSurvey01` 才允许把承诺分类为：

```text
可复用现有 Mechanic
需要新增 Enemy Mechanic
需要 Battle Runtime
需要 Item/Battle 跨系统合同
需要用户设计
```

本包不得提前实施该分类的代码结果。

## 9. 四项用户决策

必须恰好生成四项 `USER_DECISION_REQUIRED`，不得自动选择默认答案：

| decisionId | 内容 | 当前允许锁定 | 当前禁止锁定 |
| --- | --- | --- | --- |
| `BA-D1` | 守骨奴的“替盲眼女人承担伤害”是否存在真实可受击保护目标 | 守护姿态、盲眼女人叙事关系、“忠”字核心 | 保护目标 HP、目标选择、承伤比例、连线与执行规则 |
| `BA-D2` | 竞骨人辅助机制选择“虚假分身”或“竞价护盾” | 老人本体、三身份、骨契册、身份切换与打断窗口 | 同时使用两个辅助机制或由开发窗口代选 |
| `BA-D3` | 换骨残相与骨尘小像的 Copy 范围 | “复制/模仿”作为策划承诺和视觉母题 | 复制视觉、数值、触发行为或 Item 内部实现的具体边界 |
| `BA-D4` | 万相骨兽封存对象与两种正式形态 | 巨兽主轮廓、交易口、骨商核心、封存/解封母题 | 锁器类还是单项效果、具体两形态、颜色、弱点与执行规则 |

四项未决不阻塞：

```text
12 普通怪和 4 Boss 的基础身份
四章视觉家族
稳定 contentId / presentationKey
普通怪基础概念美术
12 场景母图与分层规划
```

四项未决必须阻塞对应 Runtime、VFX 状态机、最终机制 UI 和正式 Boss 阶段演出。

## 10. 系统所有权边界

必须在主报告中固定：

```text
Enemy Carrier Catalog
  拥有敌人/Boss身份、名称、描述、presentationKey。

Enemy Mechanic / Skill / Phase Data
  拥有机制意图、技能定义、反制窗口和 Boss 阶段声明。

EnemyRuntimeState
  未来只拥有当前敌人 HP、护壳、状态、施法、形态、阶段和保护关系事实。

Battle Resolver / Field Runtime
  未来执行伤害、死亡地面危险、真假目标、承伤分配和战斗结果。

Item System
  继续拥有 Item 身份、器类、效果和可用状态事实。

CrossSystem Effect Contract
  未来承载 Enemy 发出的封存、复制、资源争夺等请求。
  Enemy 不得直接修改 Item、Board、资源或 GameObject。

Shared Presentation Contract
  未来拥有 EncounterPresentationProfile Schema。
  Encounter 内容通过 sidecar binding 绑定视觉 Profile。
  E04、Scene、Prefab 和图片均不得复制 MapRule 或机制真源。

ProductFlow / RunFlow / Reward / Save / Drop / Operations
  继续拥有正式章节、奖励、存档、掉落、签到、服装与营销运营。
```

本包中的道具、掉落、奖励、签到、服装、背包外观、KV、PV 只能列为 `EXTERNAL_REFERENCE / OUT_OF_SCOPE`，不得建立数据表、数值、概率、发放规则或实现 Assignment。

## 11. 玩家安全与开发者信息

当前输出是开发规格，不是玩家 UI。报告仍须区分：

允许未来玩家看见：

```text
敌人/Boss名称
外形与可观察状态
护壳、施法、身份切换、核心暴露等机制线索
场景氛围
失败后的现象提示
```

禁止作为玩家完整答案：

```text
完整 Build 解法
推荐具体 Item / 核心道具 / Affix / Synergy
BuildCapability key 与阈值
Any / All requirement
Readiness 详细判定
Boss 六钥匙
DropBias、奖励权重和掉落概率
开发者机制分类或跨系统 Owner
```

## 12. 文件白名单

开发任务只允许新增以下 8 个文件：

```text
Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md
Docs/V0.4/Reports/BoneAspectContentCatalog.csv
Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv
Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv
Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv
Docs/V0.4/Reports/BoneAspectContentDesignLockLeakCheckReport.md
```

允许修改已有文件：`0`。

任务窗口不得修改：

```text
本 Assignment
ENEMY_SYSTEM_PACKAGE_QUEUE.md
EnemySystemGuard_CurrentRules.md
其他 Guard CurrentRules / Queue / Assignment
```

不得新增 `.cs`、`.meta`、`.asset`、`.prefab`、`.unity`、图片、动画、材质、配置或临时工程文件。

## 13. CSV 要求

`BoneAspectContentCatalog.csv` 必须恰好 `16` 个数据行，并至少包含：

```text
contentId
contentKind
chapterId
displayName
localizationKey
presentationKey
visualFamilyId
primarySilhouetteKey
coreRecognitionKey
mechanicCommitmentSummary
designStatus
devOnly
isEnabled
entersFormalFlow
```

固定计数：

```text
Enemy = 12
Boss = 4
Duplicate contentId = 0
Duplicate presentationKey = 0
devOnly=false = 0
isEnabled=true = 0
entersFormalFlow=true = 0
```

`BoneAspectEncounterNodeCatalog.csv` 必须恰好 `12` 个数据行，并至少包含：

```text
nodeId
chapterId
stageRangeLabel
displayName
visualFamilyId
sharedAssetFamilyId
primaryVisualPurpose
battleActorSafeAreaRequirement
uiSafeAreaRequirement
formalRouteBound
contentStatus
```

固定计数：

```text
Chapter = 4
Nodes per chapter = 3 / 3 / 3 / 3
Duplicate nodeId = 0
formalRouteBound=true = 0
```

`BoneAspectMechanicCommitmentMatrix.csv` 必须覆盖 16/16 Carrier，逐条记录原策划机制承诺、现有机制复用候选、新机制缺口候选、跨系统候选和决策状态。候选只用于后续 Survey，不得标记 `IMPLEMENTED`。

`BoneAspectVisualLanguageSpec.csv` 必须覆盖：

```text
全局视觉定位
五种主色
四章视觉家族
六种统一材质/叙事含义
产品化可读性规则
禁止方向
普通怪交付视图要求
Boss交付视图要求
场景分层与安全区要求
```

`BoneAspectExternalReferenceBoundary.csv` 必须逐项列出：

```text
道具保证件
Boss奖励
随机掉落
骨相念痕
照灯记服装
骨相·初痕背包
签到
Logo / KV / PV / Boss海报
正式章节 / Reward / Save / RunFlow
```

每项必须为 `EXTERNAL_REFERENCE / OUT_OF_SCOPE`，并标注真实 Owner 候选；不得写正式数值、概率或执行逻辑。

`BoneAspectUserDecisionSheet.csv` 必须恰好 4 行 BA-D1 至 BA-D4，状态全部为 `USER_DECISION_REQUIRED`。

## 14. Canonical 与离线检查

本包不新增 Runtime Schema，但必须生成一个内容规格 Canonical Signature：

```text
格式：sha256: + 64 lowercase hex
输入：按相对报告路径 Ordinal 排序
每个 CSV 先以 UTF-8、LF、原始字节计算 lowercase SHA-256
Canonical 行：relativePath|lowercaseFileSha256
```

Canonical 输入只包括六份 CSV：

```text
BoneAspectContentCatalog.csv
BoneAspectEncounterNodeCatalog.csv
BoneAspectMechanicCommitmentMatrix.csv
BoneAspectVisualLanguageSpec.csv
BoneAspectExternalReferenceBoundary.csv
BoneAspectUserDecisionSheet.csv
```

主报告必须记录该 Signature 和生成算法。连续计算两次必须一致。报告本身和 LeakCheck 不参与签名，避免自引用。

允许使用一次性只读 PowerShell 命令解析 CSV、验证行数、唯一性、布尔隔离、决策状态、UTF-8/LF 和 Canonical；不得把临时脚本写入工程。

由于本包不新增或修改任何 C#、Unity 资产或配置：

```text
Unity compile = NOT_REQUIRED
Unity batch verifier = NOT_REQUIRED
Scene handtest = NOT_REQUIRED
```

不得为了获得 Unity PASS 而新增 Editor Verifier、打开 Builder 或保存项目。

## 15. Protected baseline

任务开始时必须记录当前磁盘状态和 HEAD。工作区已有大量 Item、BattleSandbox、Scene、Prefab、Report 与资源改动，全部视为 `PREEXISTING_UNRELATED_DIRTY`。

本包与当前 Item 修正线共享同一 Unity checkout，但没有文件或状态 Owner 重叠。Protected baseline 必须以开发任务开始时的磁盘状态为准，不得使用 Git HEAD 或过期全仓哈希覆盖当前用户/其他任务改动。

本包保护要求：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/** = byte-identical
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/** = byte-identical
Assets/_Game/Scripts/TalismanBag/Items/** = byte-identical
Assets/_Game/Scripts/TalismanBag/BuildSandbox/** = byte-identical
Assets/_Game/Scenes/** = byte-identical
Assets/**/*.prefab = byte-identical
Assets/_Game/Configs/** = byte-identical
ProjectSettings/EditorBuildSettings.asset = byte-identical
Docs/LOCKED/** = byte-identical
AGENTS.md = byte-identical
```

开发任务必须在开工前记录上述集合的稳定聚合哈希，并在结束时以同一文件集合和算法复核。不得把用户工作期间新产生的无关文件纳入本包，也不得清理、回退、移动或覆盖任何既有 dirty 状态。

Assignment 文件由 Guard 新增，不计入开发任务的 8 个交付文件；开发任务不得修改它。

并行隔离固定：

```text
Items/** = 只读
BuildSandbox/** = 只读
CrossSystem Item 合同 = 只读
Item Queue / Item Reports = 只读
Scene / Prefab / BuildSettings / ProjectSettings = 只读
不得运行 Unity batch、Unity Verifier、Builder 或任何会触发场景/Prefab保存的命令
不得关闭用户 Editor、Item 开发进程或其他 batch
不得认领 Unity 导入产生的非白名单 .meta
若本任务产生任何非白名单文件，必须停止、归因并只清除本进程副作用
```

因为本包明确 `Unity = NOT_REQUIRED`，检测 UnityLockfile 或其他 batch 时也不得等待后补 Unity PASS；直接保持 Unity 验证为 `NOT_REQUIRED`。

## 16. 绝对禁止

```text
不修改任何已有工程文件
不开发 Runtime 或 Editor 代码
不新增或修改 Enemy Domain / Vocabulary / E03-E10
不新增 Skill、Phase、CounterWindow、MapRule、Pressure 或 Requirement 实现
不创建 Encounter Runtime、Spawn、Timer、状态机、目标选择或伤害逻辑
不修改正式 1-10 / 2-10
不接 3-10 / 4-10 正式入口
不修改 BossConfig、StageConfig、RunFlow、SaveData、Reward、Drop 或 Chapter
不修改 Item、Board、BattleContract、Battle Bridge 或 UnifiedBattlePage
不创建 Scene、Prefab、ScriptableObject、MonoBehaviour 或 GameObject
不导入或移动用户美术
不创建 EncounterPresentationProfile 实现
不把场景视觉字段塞进 E04
不把 MapRule、伤害、机制或阶段写进图片、Prefab 或 Scene
不替用户决定 BA-D1 至 BA-D4
不为未决机制创建临时假逻辑
不建立第二套 Item、资源、HP、Boss阶段或正式流程真源
不修改 AGENTS.md / Docs/LOCKED/*
不修改 Queue / Guard CurrentRules
不运行会保存 Scene/Prefab/Config 的 Builder
不覆盖或回退既有工作区改动
不 commit / tag / push
不自动启动 P1 或其他后续包
```

## 17. 验收要求

必须满足：

```text
新增交付文件 = 8
修改已有文件 = 0
普通怪 = 12/12
Boss = 4/4
场景节点 = 12/12
章节视觉家族 = 4/4
Duplicate contentId / presentationKey / nodeId = 0
Mechanic commitment coverage = 16/16
USER_DECISION_REQUIRED = 4/4
External reference boundary coverage = PASS
devOnly / disabled / no formal flow = PASS
正式 1-10 / 2-10 覆盖 = 0
E10 修改 = 0
Runtime / Battle / Item / Scene / Prefab / Config 修改 = 0
Canonical determinism = PASS
Source attachment hash = MATCH
Protected hashes = unchanged
Leak Count = 0
package-scope whitespace = PASS
package-scope UTF-8/LF = PASS
git diff --check = PASS 或仅记录开工前 PREEXISTING_UNRELATED_DIFF
HEAD unchanged
Unity / Scene handtest = NOT_REQUIRED
commit / tag / push = 0 / 0 / 0
```

主报告必须明确：

```text
本包锁定的是内容身份和策划承诺，不是 Runtime 实现完成。
16 个 Carrier 可以进入 P1 数据包。
四项用户决策不阻塞普通怪基础美术。
Encounter Presentation 由 Shared Presentation Contract 后续独立收口。
```

## 18. 回传格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BoneAspectContentDesignLock01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_BONEASPECTCONTENTDESIGNLOCK01

Result:
CONTENT_LOCK_COMPLETE / QA_RESULT

新增文件:
修改已有文件:
Enemy / Boss / EncounterNode:
Visual families:
Mechanic commitment coverage:
USER_DECISION_REQUIRED:
Duplicate IDs:
External references out of scope:
Source attachment hash:
Canonical signature:
Canonical determinism:
Runtime / Battle / Item / Scene / Prefab / Config 修改:
Formal 1-10 / 2-10 references changed:
E10 changed:
Protected hashes:
Leak Count:
git diff --check:
HEAD unchanged:
Unity compile / verifier:
Scene handtest:
commit / tag / push:
P1 status: NOT_STARTED
```

## 19. 通过后

P0 通过只代表《骨相》Enemy/Encounter 内容规格完成。

Guard 复核 P0 后，才允许直接准备：

```text
V0.4-BoneAspectCarrierPresentationCatalog01
```

P1 预计建立 16 个 devOnly Carrier，只负责身份、名称、描述、presentationKey、美术交付槽位和多对多机制引用，不接 Runtime。

以下继续冻结：

```text
EncounterPresentationProfileContract01
BoneAspectMechanicGapSurvey01
MechanicVocabularyExtension01
Skill / CounterWindow / BossPhase 内容
EnemyRuntimeState
CrossSystem Effect Request
BattleSandbox 竖切
Scene / Prefab
正式 1-10 / 2-10
Reward / Save / Drop / RunFlow
```

开发任务不得自动启动 P1。
