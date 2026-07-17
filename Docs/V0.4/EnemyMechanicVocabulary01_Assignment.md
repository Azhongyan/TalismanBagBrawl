# V0.4-EnemyMechanicVocabulary01 Assignment

Guard assignment：

```text
ENEMY_GUARD_ASSIGNMENT_ENEMYMECHANICVOCABULARY01
```

生成日期：2026-07-13  
所属：Enemy / Boss / Encounter System Guard  
包序号：E02

## 1. 包定位

本包只建立 Enemy 系统的稳定机制词汇、抽象 Build 能力词汇、压力通道、反制窗口类型、玩家提示类别、开发者诊断类别，以及旧 devOnly 标签到新稳定键的映射。

本包不把机制绑定到具体敌人，不迁移 11/7 与 10/6 内容，不实现任何战斗运行时。

内部口径：

```text
EnemyArchetype = 玩家在打谁
Mechanic = 敌人通过什么方式制造压力
BuildCapability = 玩家 Build 具备什么反制能力
PressureChannel = 当前战斗压力属于什么方向
CounterWindow = 成功反制后出现什么战斗窗口
```

“机制验证”不是问卷、选择题或答案面板。

## 2. 启动必读

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
Docs/V0.4/EnemyMechanicVocabulary01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/*
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/*
```

## 3. 必须做

在独立新目录建立纯 C# Vocabulary：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
```

至少建立以下稳定词汇类别：

```text
MechanicKey
BuildCapabilityKey
PressureChannelKey
CounterWindowTypeKey
PlayerHintCategoryKey
DeveloperDiagnosticCategoryKey
```

稳定键必须使用可扩展字符串身份，不得把可增长的机制全集锁死为必须改代码顺序的数值枚举。

推荐稳定格式：

```text
mechanic.layered_shield
capability.break_power
pressure.shield
counter_window.shell_break
player_hint.shield_pressure
diagnostic.capability_gap
```

必须提供：

```text
EnemyVocabularyEntrySnapshot
LegacyEnemyVocabularyMappingSnapshot
EnemyMechanicVocabularySnapshot
IEnemyMechanicVocabularyProvider
严格 Ordinal 查询
不可变集合
确定性 Canonical Signature
```

类名可按既有代码风格微调，但语义和边界不得改变。

## 4. 首批语义覆盖

Mechanic 至少覆盖现有含义：

```text
基础压力
层叠护盾
群怪 / 召唤压力
毒
燃烧
偷灵 / 能量削减
封符
高爆发
长读条施法
厚血 / 长线战斗
污染格
阵眼干扰
```

BuildCapability 至少覆盖现有含义：

```text
BreakPower
ClearPower
CleansePower
DebuffCounter
EnergyStability
SpiritLock
ControlPower
PlacementShape
BurstWindow
GuardPower
InterruptTiming
SustainedDamage
ChainReaction
CooldownRecovery
CasterInterrupt
ThunderChain
```

PressureChannel 至少覆盖：

```text
护盾压力
多目标压力
持续伤害 / 负面状态压力
资源干扰压力
封符 / 控制压力
爆发承伤压力
施法打断压力
长线输出压力
摆放 / 阵眼压力
```

CounterWindowType 至少覆盖当前 6 类语义：

```text
破壳后虚弱
净化后显形
打断后僵直
护阵后反震
清场后核心暴露
供能反制后全阵窗口
```

本包只定义稳定类别和开发者中文说明，不写正式玩家 UI 文案。

## 5. 旧标签映射

只读盘点以下旧来源中的机制相关标签：

```text
enemyType
bossMechanic
validationTags
mechanicType
requiredCapabilityTags
optionalCapabilityTags
MapRule affected / buff / debuff tags
WeaknessWindow mechanic type
```

每个旧键必须：

```text
映射到一个或多个新稳定键
或明确标记 OUT_OF_SCOPE，并记录原因
```

允许一对多映射，例如组合型旧标签可拆为多个机制原语。禁止为了表格整齐而强制一对一。

以下内容不是本包词汇，不得误收：

```text
具体敌人 ID
具体 Boss ID
具体 Item ID
具体羁绊名称
具体词条 ID
DropBias
章节 ID
伤害、生命、时间或概率数值
```

## 6. 玩家层与开发者层

PlayerHintCategory 只表达提示类别，例如护盾正在恢复、施法需要打断、供能受到干扰。

DeveloperDiagnosticCategory 可以表达能力缺口、未映射旧键、引用无效等诊断类别。

禁止在玩家类别中出现：

```text
solution
answer
requiredSynergy
requiredAffix
requiredStats
hardSolutionTags
minimumKeysRequired
dropBias
exactThreshold
```

## 7. 允许新增范围

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs
对应新增文件与目录的 .meta
Docs/V0.4/Reports/EnemyMechanicVocabularyReport.md
Docs/V0.4/Reports/EnemyMechanicVocabularySpec.csv
Docs/V0.4/Reports/EnemyMechanicVocabularyInventory.csv
Docs/V0.4/Reports/EnemyMechanicVocabularyLegacyMapping.csv
Docs/V0.4/Reports/EnemyMechanicVocabularyLeakCheckReport.md
```

开发窗口不得修改本 Assignment、Guard CurrentRules 或 Package Queue。

## 8. 受保护文件

E01 已交付文件必须保持 byte-identical：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs
```

开发前记录上述文件 hash，开发后复核。

## 9. 绝对禁止

```text
不改任何 Scene / Prefab / Config
不新增 MonoBehaviour 或 ScriptableObject
不改 BattleSandbox、BattleContract 或 Battle Bridge
不改棋盘摆放、拖拽、旋转、托盘和输入
不改 Item 系统
不改 EnemyDefinition / EnemySkillDefinition / 正式敌人资产
不把词汇绑定到 11/7 或 10/6 的具体条目
不实现 MapRule Runtime、技能 Runtime、Boss Phase Runtime
不写伤害、护盾、状态、计时或目标选择逻辑
不改 1-10 / 2-10
不改 RunFlow / Reward / SaveData / Boss 正式流程
不接正式章节入口
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 10. Verifier 要求

至少验证：

```text
所有稳定键非空、唯一、严格区分大小写
稳定键符合 namespaced lowercase 格式
不同 Vocabulary 类别不会误共享同一个完整稳定键
输入与输出集合不可变
输入顺序变化不影响 Canonical Signature
内容变化会改变 Canonical Signature
所有 Legacy Mapping target 可解析
一对多 Legacy Mapping 可被保留
重复 legacy source key 可被发现
每个盘点旧键均已映射或明确 OUT_OF_SCOPE
玩家类别无完整答案字段
无 UnityEngine / MonoBehaviour / ScriptableObject
无 BuildSandbox Runtime / Battle / Board / Item 依赖
无章节硬编码
E01 受保护文件 hash 不变
Scene / Prefab / Config / Battle / Board / Item 修改均为 0
```

## 11. 报告要求

必须输出：

```text
EnemyMechanicVocabularyReport.md
EnemyMechanicVocabularySpec.csv
EnemyMechanicVocabularyInventory.csv
EnemyMechanicVocabularyLegacyMapping.csv
EnemyMechanicVocabularyLeakCheckReport.md
```

Inventory 至少包含：

```text
category
stableKey
developerLabelZh
description
playerVisible
developerOnly
```

LegacyMapping 至少包含：

```text
legacySourceKind
legacyKey
canonicalCategory
canonicalKey
mappingMode
status
reason
```

## 12. 自动验证

必须尝试：

```text
Unity batch compile
Unity batch Verifier
同源离线 Verifier
git diff --check
新增文件尾随空白检查
精确变更文件清单
```

如同一工程被用户 Unity Editor 占用：

```text
不得关闭用户 Editor
不得结束 Unity 进程
不得声称 Unity Verifier PASS
记录 BLOCKED 原因
继续完成同源离线 Verifier
由 Guard 决定是否以记录阻塞方式收口
```

本包无需场景手测。

## 13. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyMechanicVocabulary01

Result:
DEV_COMPLETE / QA_RESULT

新增文件：
修改已有文件：必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改：必须全部为 0
E01 protected hash：必须全部不变
Vocabulary 数量：按类别报告
Legacy keys：总数 / mapped / out-of-scope / unresolved
Verifier：通过数 / 总数
Unity batch compile：PASS / FAIL / BLOCKED
Unity batch Verifier：PASS / FAIL / BLOCKED
Leak：必须为 0
git diff --check：PASS / FAIL
未 commit / tag / push
```

## 14. 通过后

通过后下一候选包：

```text
V0.4-EnemyValidationContentNormalize01
```

E03 不自动启动。
