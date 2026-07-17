# Item 算法 Guard 当前规则

同步日期：2026-07-11  
来源窗口：Item算法窗口  
规则状态：`RULE_CONFIRMED`  
代码状态：`NO_CODE_CHANGE`  

## 一、归属与职责

Item 算法系统从属于总体 Item 系统，负责“一个道具实例出生时是什么样”。

负责：

```text
五品阶定义
基础属性区间与属性 Roll
固定词条数值 Roll
随机词条池、槽位、权重、互斥与数值 Roll
实例 Build 资格
品阶允许的核心效果潜力
关卡候选池与品阶权重
seed 确定性
实例生成、大样本模拟与概率报告
```

不负责：

```text
摆放、旋转、点亮、接亮与阵脉
Build 2/4/6 统计与激活
养成解锁、突破、消耗、洗练与重 Roll
战斗结算与技能执行
Reward / RunFlow / Inventory / SaveData 正式接线
正式掉落 UI
```

总体 Item System 继续拥有基础身份、运行时规则、详情投影和跨系统只读契约。

## 二、道具身份

```text
I001-I030 = 30件普通道具原型
I031 = 聚念石，系统节奏道具
```

正式组合：

```text
30个普通道具原型 × 5个品阶 = 150个品阶版本
```

150 表示 `baseItemId + rarity` 的组合，不创建 150 个独立 `itemId`。

聚念石：

```text
不属于30件普通道具
不参与30×5组合
不进入普通随机掉落池
不随机普通词条
不随机普通Build资格
不作为普通养成品阶道具
由主线、引导、关卡节点或系统定向发放
```

## 三、五品阶稳定键

玩家显示：

```text
凡品
良品
灵品
玄品
道品
```

正式技术稳定键：

```text
white
green
blue
purple
orange
```

`bai/qing/lan/zi/cheng`、`Common/Uncommon/Rare/Epic/Legendary` 等只能作为迁移别名，不得继续形成正式稳定键。

## 四、三层身份模型

`baseItemId`：

```text
I001-I030普通原型身份
决定名称、文化法门、文化器类、形状、coreCell、基础效果方向与视觉血统
```

`rarityVersion`：

```text
baseItemId + rarity 的派生组合
不创建新的独立itemId
```

`itemInstanceId`：

```text
每次实际生成实例的唯一身份
保存品阶、生成结果、seed与generationVersion
同一baseItemId + rarity允许存在多个不同实例
```

`placementId`：

```text
实例本次在阵盘中的摆放身份
只服务摆放、点亮、阵脉和运行时布局
不得代替itemInstanceId
```

## 五、基础属性规则

每件道具的每条属性分别定义：

```text
属性类型
完整数值范围
五品阶子区间
HigherIsBetter / LowerIsBetter / Neutral
精度与步长
取整规则
```

锁定规则：

```text
品阶不使用统一数值倍率
可以提供全局默认band帮助初始化
每件道具的每条属性必须能够单独覆盖五品阶区间
第一版每条属性在自己的品阶区间内独立Roll
```

当前拒绝：

```text
主品质值 + 全属性扰动
用隐藏总品质分替代逐属性判断
```

## 六、词条规则

固定词条：

```text
词条类型由道具原型确定
实际数值按品阶区间Roll
“固定”表示类型固定，不表示数值固定
```

随机词条：

```text
从该道具允许的词条池抽取
词条ID与实际数值分开保存
数值按该词条自己的五品阶区间Roll
槽位、权重、互斥和重复规则配置化
```

尚未确认：

```text
各品阶固定词条槽数量
各品阶随机词条槽数量
随机词条能否重复
道品专属词条是否必出
正式词条概率与权重
```

不得把 v0.5 草案中的 1/2/3/4/5 槽写成正式规则。

## 七、核心效果与养成边界

```text
品阶决定：可以拥有、显示和培养哪些核心效果
养成决定：符合品阶资格的核心效果是否已经开窍
点亮决定：已开窍核心效果当前能否在战斗中发动
```

关系：

```text
rarityAllowed
+ cultivationUnlocked
+ isLit
= coreEffectActive
```

生成系统只输出：

```text
eligibleCoreEffectIds
visibleCoreEffectIds
cultivationPotentialProfileId（接口预留）
```

生成系统不负责：

```text
unlockedCoreEffectIds
等级与突破成长
养成消耗
满级上限
洗练与重Roll
```

尚未确认五品阶核心效果数量、开放顺序、终极核心条件和养成上限。不得把任何临时数量写成正式 QA 规则。

## 八、Build 资格

原型文化身份：

```text
faMenTag
qiLeiTag
```

实例资格：

```text
BuildQualification
```

两者必须分离。

凡品锁定：

```text
BuildQualification = None
保留法门/器类文化身份
不计入法门Build
不计入器类Build
```

良品及以上：

```text
开始有概率获得Build资格
品阶越高，获得资格概率方向上越高
```

最终计数：

```text
isLit = true
+ 实例具有对应Build资格
= 计入对应Build
```

结构可预留：

```text
Unresolved
None
FaMenOnly
QiLeiOnly
Dual
```

尚未确认法门/器类是否独立抽取、各品阶概率、Dual 规则和升阶继承方式。不得采用 v0.5 草案概率或“道品100% Dual”。

## 九、主法门与普攻

“按固有 faMenTag 自动决定普攻主法门”尚未获得用户确认，并与当前“主 Build 显式选择、默认 None”契约可能冲突。

当前裁决：

```text
不进入Item生成算法
不进入ItemRarityInstanceFoundation01
不作为前10关凡品普攻来源的默认答案
后续由总体Item Guard联合Battle/技能规则单独讨论
```

## 十、掉落算法边界

Item 算法线允许：

```text
DropGenerationRequest / DropSourceContext
关卡与来源候选池
品阶权重算法
30件普通道具抽取
seed确定性
ItemInstance生成
大样本模拟与概率报告
I031排除验证
```

当前唯一锁定的关卡规则：

```text
第一阶段/前10关只掉凡品
```

其余开放节点和概率尚未锁定。

当前禁止正式接入：

```text
Reward / RunFlow / Boss奖励
正式掉落表与掉落UI
Inventory写入 / SaveData
正式保底
秘境、爬塔、稀有事件、锻造等正式来源
```

正式接线以后另开 `ItemDropRewardIntegration01`。Reward、Save、Battle 和 UI 不得重新 Roll。

## 十一、数值成熟度

```text
SCHEMA_ONLY
SEED_DATA
BALANCE_CANDIDATE
PLAYTEST_ACCEPTED
LIVE_LOCKED
```

当前第一包只允许 `SCHEMA_ONLY`。

现有 V0.2/V0.3 少量固定战斗值只能作为稳定锚点；BuildSandbox 的 `devOnly=true / isEnabled=false` 数值只能作为测试参考；两者都不能直接视为 I001-I030 的正式五品阶数值。

第二包先建立属性字典和区间表结构，再填少量锚点道具 `SEED_DATA`；模拟与实战通过后才能升级成熟度。

## 十二、明确拒绝的未批准草案

以下只能标记为 `PROPOSAL_ONLY / NOT_APPROVED`：

```text
全属性统一归一化band
主品质值 + 属性扰动
凡品1条、良品2条等正式核心数量
1/2/3/4/5正式词条槽表
良品至道品具体Build概率
道品100% Dual
11-20、21-40等正式掉落概率
itemPower 55/25/10/10公式
自动普攻主法门
Item算法文档规定正式Battle结算顺序
```

## 十三、ItemSystemSnapshot.v1 兼容策略

现有 `ItemSystemSnapshot.v1` 继续负责 Catalog、placement、摆放、点亮、阵脉、Build统计、开窍投影和技能监控预览。

不得重新解释 `itemId` 或 `placementId`，当前不得修改其字段或 Canonical Signature。

新系统先建立：

```text
ItemInstanceIdentitySnapshot.v1
```

字段：

```text
itemInstanceId
baseItemId
rarity
generationVersion
rootSeed
cultivationPotentialProfileId
```

后续再由 `ItemGeneratedInstanceSnapshot.v1` 组合属性、词条、核心潜力和 Build 资格。

## 十四、算法线完成状态

```text
Queue status: 8/8 COMPLETE
Final package: V0.4-ItemInstanceProjectionContract01
Final result: COMPLETE
Final marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Final spec: 156/156 PASS
Instances: 6
Queries: 9
Leaks: 0
User hand test: PASS
```

八个算法包已完成身份、属性区间、核心潜力/Build资格、词条 Schema、实例 Roll、掉落 Sandbox、模拟校验和实例只读投影契约。

当前仍保持：

```text
QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA
ItemDropRewardIntegration01未启动
未接Reward / RunFlow / Inventory / SaveData / Battle / Boss或正式掉落UI
```

RepoOps 后续操作必须按包精确选择文件，不得整目录 `git add`，避免混入进入任务前已有的 dirty/untracked 内容。

## 十五、2026-07-12 平衡候选工作台收口

```text
Package: V0.4-ItemBalanceWorkbenchAnd150CandidateSeed01 + GuardFix01
Status: COMPLETE / PASS
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
Item System Guard receipt: GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
GuardFix Spec: 1286/1286 PASS
```

已验收范围：

```text
30/30 ordinary prototypes
150/150 rarity versions
600/600 stat ranges
150/150 roll
150/150 projection
150/150 determinism
I031 ordinary-pool entries = 0
```

当前数据状态必须完整表述为：

```text
BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED
```

工作台候选资产允许编译到既有 Stat / Affix / Core / Build / Drop Schema，并允许在 Editor 工作台执行 Roll 与 Projection。它们是可调平衡候选，不是正式运行时配置，也没有获得 `PLAYTEST_ACCEPTED` 或 `LIVE_LOCKED` 身份。

尚未接入：

```text
Formal Item Runtime
Formal Item Detail
Battle
Reward / RunFlow
Inventory / SaveData
Formal Drop System
```

`AFFIX_DURATION_BRIDGE` 只作为 duration secondary-stat pool 的候选桥接定义存在；在单独规则审核前不得作为正式词条定义传播。

八个算法基础包仍记为 `8/8 COMPLETE`；本包归入后续 Balance Candidate Phase，不改写基础包数量。`ItemDropRewardIntegration01` 继续冻结，必须由用户另行确认并下发跨系统 assignment。

## 十六、2026-07-15 Item 详情头部双状态与阵脉行内修正规则

```text
Rule status: RULE_CONFIRMED
Package: V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01
Package status: READY_FOR_TASK_WINDOW
Assignment: Docs/V0.4/ItemDetailTwoStateAndInlineArrayModifierGuardFix01_Assignment.md
```

用户确认本轮只收口以下显示与候选数据规则：

```text
不修改用户已手动调整的详情页布局、RectTransform、字号、间距与现有视觉层级
不修改详情页当前三大分类、分类名称、顺序或模块归属
不修改现有 Stat / Signature / Affix / Core / Build 候选内容与数值
只把头部状态收敛为“点亮状态 + 阵脉状态”两个状态
开窍状态不再占用头部状态位，回到每条核心效果内部显示
阵脉数值不建立独立大区块，直接附着到前两类中实际受影响的数据行
```

术语与状态边界固定为：

```text
入阵 = 是否已经摆入阵盘；不是阵脉状态，也不与点亮等价
点亮 = 点亮源 / 直接点亮 / 相邻接亮 / 未点亮；未入阵时显示“尚未入阵”
阵脉 = 未占阵脉 / 占阵脉但待点亮 / 阵脉生效；未入阵时显示“阵脉未计算”
开窍 = 单条核心效果的养成解锁状态；在核心效果行内显示
```

阵脉行内修正显示规则固定为：

```text
原始道具数值保持原样显示
仅当 isArrayBonusActive=true 且存在合法目标修正时，显示“阵脉图标 + 专色增量”
阵脉未生效时，不显示图标、增量或占位文案
基础属性、本命效果、固定词条、随机词条、核心效果可显示对应阵脉增量
法门/器类 Build 只有在阵脉载荷明确指向该阶段且该阶段当前有效时才显示增量
新增机制型阵脉效果必须作为专色子行附着到其目标效果，不得形成孤立的阵脉说明块
移出阵脉或失去点亮后，阵脉增量必须消失，实例原始数据不得被覆写
```

阵脉视觉来源固定为：

```text
Theme token: arrayModifierColor
Candidate default: #55C6B3
Icon key: Icon_ArrayVeinModifier
正式玩家显示使用 Sprite 图标；不得使用 Unicode 字符充当最终图标
颜色与图标必须同时出现，颜色不能成为唯一识别方式
```

当前 `ItemArrayBonusRules` 只有占位与激活布尔状态，没有数值或机制载荷。本轮允许在 `BALANCE_CANDIDATE / EDITABLE / NOT_BATTLE_CONNECTED` 边界内增加独立、可编辑、可追踪来源的阵脉候选修正数据；不得由 UI 根据字符串或固定比例自行编造，也不得修改 `ItemSystemSnapshot.v1`、正式 Battle、Reward、RunFlow、Inventory 或 SaveData。
