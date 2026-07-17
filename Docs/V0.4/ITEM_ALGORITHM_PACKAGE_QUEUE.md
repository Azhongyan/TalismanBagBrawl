# Item 算法 Package Queue

更新时间：2026-07-11  
所属：总体 Item System 下属 Item 算法线

## 总体状态

```text
Status: 8/8 COMPLETE
Final package: V0.4-ItemInstanceProjectionContract01
Final marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Final spec: 156/156 PASS
User hand test: PASS
Cross-system integration: NOT_STARTED
```

## 已完成包

### 1. V0.4-ItemRarityInstanceFoundation01

```text
Status: COMPLETE / PASS
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMRARITYINSTANCEFOUNDATION01
User approval: CONFIRMED
```

只做：

```text
I001-I030普通原型身份
五品阶稳定定义
30×5=150派生组合
baseItemId / itemInstanceId / placementId边界
ItemInstanceIdentitySnapshot.v1
generationVersion / rootSeed
I031普通生成排除
cultivationPotentialProfileId接口预留
Verifier与报告
```

禁止：

```text
真实属性与数值
词条池与词条Roll
核心效果数量与解锁
Build资格概率
掉落与Reward
养成、Save、Battle或正式页面接线
修改ItemSystemSnapshot.v1
```

### 2. ItemStatRangeSchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_STAT_RANGE_SCHEMA01_PASS
Spec: 39/39 PASS
```

建立属性字典、方向、精度、步长、总范围与五品阶子区间。先使用少量锚点道具 `SEED_DATA`，不填30件正式数值。

### 3. ItemCorePotentialAndBuildEligibilitySchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_CORE_POTENTIAL_AND_BUILD_ELIGIBILITY_SCHEMA01_PASS
Spec: 69/69 PASS
```

建立品阶核心潜力与 BuildQualification Schema。只做结构和校验，不擅自填写核心数量或 Build 概率。

### 4. ItemAffixPoolAndRangeSchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_AFFIX_POOL_AND_RANGE_SCHEMA01_PASS
Spec: 88/88 PASS
```

建立固定词条、随机词条、品阶数值区间、槽位、权重、互斥和重复规则结构。未获确认的数据保持 `Unresolved`。

### 5. ItemInstanceRollEngine01

```text
Status: COMPLETE / PASS
Marker: ITEM_INSTANCE_ROLL_ENGINE01_PASS
Spec: 72/72 PASS
```

使用稳定 seed 生成属性、词条、核心潜力与 Build 资格结果。必须可复现，不接 Reward、Inventory 或 Save。

### 6. ItemDropGenerationSandbox01

```text
Status: COMPLETE / PASS
Marker: ITEM_DROP_GENERATION_SANDBOX01_PASS
Spec: 87/87 PASS
Determinism: 6/6 PASS
```

建立 DropGenerationRequest、DropSourceContext、候选池、品阶权重、30件普通道具抽取和 I031 排除。只在独立 Sandbox 验证。

### 7. ItemGenerationSimulationValidator01

```text
Status: COMPLETE / PASS
Marker: ITEM_GENERATION_SIMULATION_VALIDATOR01_GUARDFIX01_PASS
Spec: 65/65 PASS
Distribution: 46/46 PASS
Determinism: 12/12 PASS
Invariant violations: 0
```

执行大样本模拟，验证区间、权重偏差、互斥、重复、seed复现和 I031 泄漏；输出报告，不改正式概率。

### 8. ItemInstanceProjectionContract01

```text
Status: COMPLETE / PASS
Marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Spec: 156/156 PASS
Instances: 6
Queries: 9
Leaks: 0
User hand test: PASS
```

把生成实例以只读契约提供给总体 Item System 与详情投影，不接正式 Battle、养成、Reward 或 Save。

## 后置跨系统包

```text
ItemDropRewardIntegration01
```

该包不在当前八包内。当前状态为 `NOT_STARTED / REQUIRES_EXPLICIT_CROSS_SYSTEM_ASSIGNMENT`，不得因算法线 8/8 完成而自动启动。

## 队列规则

```text
第一包只允许SCHEMA_ONLY
未确认概率与数值不得进入assignment
v0.5草案只能作为PROPOSAL_ONLY参考
每包先经Item算法Guard收口
涉及ItemSystemSnapshot或详情公共字段时同步总体Item Guard
涉及Reward/RunFlow/Save/Battle时停止并另开跨系统包
不commit / tag / push
RepoOps必须精确选择单包文件，不得整目录git add
```

## Balance Candidate Phase

### V0.4-ItemBalanceWorkbenchAnd150CandidateSeed01 + GuardFix01

```text
Status: COMPLETE / PASS
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
Item System Guard receipt: GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
User hand test: PASS
GuardFix Spec: 1286/1286 PASS
Ordinary prototypes: 30/30
Rarity versions: 150/150
Stat ranges: 600/600
Roll / Projection / Determinism: 150/150
I031 ordinary-pool entries: 0
```

该包是算法基础八包完成后的独立平衡候选阶段，不改变 `8/8 COMPLETE` 的基础包计数。

数据成熟度固定为：

```text
BALANCE_CANDIDATE
EDITABLE
NOT_LIVE_LOCKED
NOT_BATTLE_CONNECTED
```

候选 Unity 资产可编译进既有 Stat / Affix / Core / Build / Drop Schema，并可在 Editor Workbench 中执行 Roll 与 Projection；但尚未接入正式 Item Runtime、正式 Item Detail、Battle、Reward/RunFlow、Inventory/Save 或正式掉落系统。

`AFFIX_DURATION_BRIDGE` 仅是 duration secondary-stat pool 所需的候选定义，不代表正式词条规则已获批准。

不得将本包自动提升为 `PLAYTEST_ACCEPTED` 或 `LIVE_LOCKED`。`ItemDropRewardIntegration01` 继续保持 `NOT_STARTED / REQUIRES_EXPLICIT_CROSS_SYSTEM_ASSIGNMENT`。

### V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01

```text
Status: READY_FOR_TASK_WINDOW
Guard rule: RULE_CONFIRMED
Assignment: Docs/V0.4/ItemDetailTwoStateAndInlineArrayModifierGuardFix01_Assignment.md
Expected marker: ITEM_DETAIL_TWO_STATE_INLINE_ARRAY_MODIFIER_GUARDFIX01_PASS
```

本包只处理详情页头部状态收敛与阵脉行内修正展示：

```text
头部仅保留点亮状态与阵脉状态
开窍状态回到核心效果行内
阵脉生效时，在前两大类的实际目标行后显示 Sprite 图标 + 专色增量
阵脉未生效时不显示该增量
不新增阵脉大区块
不改用户手调布局
不改三大分类
不覆盖现有候选内容与数值
```

因当前阵脉 Runtime 只有状态布尔量，本包允许增加独立的可编辑 `BALANCE_CANDIDATE` 阵脉修正载荷与只读解析结果；不得接正式 Battle、Reward/RunFlow、Inventory/Save，不得修改 `ItemSystemSnapshot.v1` 或算法基础八包契约。
