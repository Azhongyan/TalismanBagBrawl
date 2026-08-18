# Item 算法 Guard 当前规则

同步日期：2026-07-21
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

## 十七、2026-07-21 I009 / I029 单格形状真源纠错

```text
Rule status: RULE_CONFIRMED
Package: V0.4-I009AndI029ItemShapeFix01
Package status: PACKAGE_SCOPE_SELFTEST_PASS / EXTERNAL_REGRESSION_BLOCKERS_RECORDED / AWAITING_USER_HANDTEST
Assignment: Docs/V0.4/I009AndI029ItemShapeFix01_Assignment.md
```

用户已明确确认以下形状身份为正式当前口径：

```text
I009 离火焚邪印：shape_single_1 / cells=(0,0) / coreCellLocal=(0,0)
I029 照煞镜：shape_single_1 / cells=(0,0) / coreCellLocal=(0,0)
```

该结论属于 `baseItemId` 原型身份真源，五品阶版本与所有实例必须继承同一形状，不得按品阶或 Roll 改写：

```text
I009@white/green/blue/purple/orange 全部为单格
I029@white/green/blue/purple/orange 全部为单格
任意 rotation 0/90/180/270 后仍只占一个格子
```

当前工作树事实：

```text
I009 的 ItemInnerDataCatalog、Candidate shapeDescription、派生报告和专项 verifier 已出现未提交的单格修正；本次 Guard 只确认规则，不把这些既有脏改动认定为已验收。
I029 的 ItemInnerDataCatalog 与 Candidate shapeDescription 仍记录为 shape_line2_v / 2格 / 核心格(0,1)，属于待修错误。
```

修复原则：

```text
优先修正 ItemInnerDataCatalog 唯一形状真源，再通过既有显式生成/验证流程同步 Candidate、实例、投影与派生报告
不得只手改 CSV 报告掩盖真源错误
不得根据 PNG 分辨率反推占格
不得在 ItemDetailPanel 对 I009 或 I029 写特殊硬编码
详情页继续按 displayShapeName / shapeId 选择 DaojuSingleCellImage
不得修改用户手调的 DaojuSingleCellImage / DaojuMultiCellImage 布局、尺寸、缩放、阴影或层级
不得推测并批量修改 I001-I008、I010-I028、I030 的形状；发现疑点只列清单
```

本包只修原型形状与其派生出口，不修改品阶概率、词条概率、Build 资格、核心效果、战斗数值、正式掉落、Reward、RunFlow、Inventory、SaveData、Battle 或 BuildSettings。

### Guard 实施复核（2026-07-21）

```text
Guard review: PACKAGE_SCOPE_PASS
Expected marker: I009_I029_ITEM_SHAPE_FIX01_PASS
Unity compile: PASS / 0 C# errors
User hand test: PENDING
Global historical suite: NOT_FULLY_GREEN / EXTERNAL_BLOCKERS_RECORDED
```

专项证据已证明 I009 与 I029 的原型真源、四方向旋转、五品阶继承、双 seed Roll / Projection、Candidate 出口、详情单格名称与 `DaojuSingleCellImage` 共享路由均符合本节锁定规则。I031 普通生成仍为 0；本包未修改其他道具形状、详情布局、正式系统或 BuildSettings。

以下失败被 Guard 归类为范围外既有/并发状态，不得在本形状包中顺手修复：

```text
ItemDetailProjectionComplete：用户手调布局当前为 12/14 active；本包明令禁止修改布局
ItemCompleteCandidateContent：全体 150 个候选命中既有 player-skill-monitor 文本检查；与 I009/I029 形状无关
ItemInstanceProjectionContract：155/156，由上游 Item Detail FAIL marker 连锁触发
ItemBalanceWorkbench：由 Item Detail / Projection 连锁，以及 verifier 源码中的 BuildSettings 文字被扫描器误命中
BattleSandbox vertical slice：仍依赖旧 Vertical2 测试道具和旧场景节点，共 14 个范围外错误
git diff --check：全仓失败来自任务入场前 ItemSandbox 场景既有尾随空格；本包目标文件范围检查无新增空白错误
```

上述分类不等于这些问题已经通过，只表示它们不是本包目标回退，也不授权形状任务窗口扩包处理。下一步仅进入用户手测；用户确认 I009/I029 棋盘单格、旋转单格及详情小图槽正确后，任务窗口再按实际状态发送 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`，并必须保留 `PACKAGE_SCOPE_PASS / GLOBAL_REGRESSION_BLOCKERS_RECORDED`，不得声称全量历史套件通过。

## 十八、2026-07-21 I001-I030 形状批量纠错统一 Guard

```text
Rule status: MULTI_GUARD_CONVERGED
Package: V0.4-ItemShapeCatalogBatchCorrection01
Package status: COMPLETE / USER_HANDTEST_PASS
Assignment: Docs/V0.4/ItemShapeCatalogBatchCorrection01_Assignment.md
GuardFix assignment: Docs/V0.4/ItemShapeCatalogBatchCorrection01_GuardFix01_Assignment.md
Primary Guard: ITEM_GUARD
Joint Guard: ENEMY_GUARD
Capability review: PASS_WITH_EXPECTED_DERIVED_DELTAS
System ownership: ITEM_OWNED / CROSS_SYSTEM_CONSUMED
Approved matrix: Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv
```

### 18.1 唯一真源与读取链

```text
ItemInnerDataCatalog / 经批准的 Item Shape 定义层
→ ItemSystemSnapshot.v1
→ IF01 ItemInstancePlacementBinding
→ Item Fact Projection
→ Layout Resilience / Enemy / Survey Capability consumers
```

`baseItemId` 对应的 `shapeId / ordered localCells / coreCellLocal / 默认朝向` 只允许由 Item 系统持有。五品阶、Roll 实例、不同 seed、Placement、详情和跨系统消费者必须继承或投影该事实，不得保存第二套可漂移形状表。I031 不在本批次内。

### 18.2 启动门槛

批量包在用户批准完整 I001-I030 矩阵前保持 `HOLD`。每行必须包含：

```text
itemId
decisionStatus = APPROVED_KEEP / APPROVED_CHANGE / UNRESOLVED
shapeId
ordered localCells
coreCellLocal
默认朝向
changeReason
```

`UNRESOLVED` 必须记录为 `NO_CHANGE_UNRESOLVED`，不得修改。I009、I029 已批准为 `shape_single_1 / (0,0) / core(0,0)`，并保留既有专项报告；其余 ID 不得从图片尺寸、器类、名称、文案或旧模板推断。

### 18.3 单包边界

统一包允许：

```text
修改用户批准 ItemId 的 Item 形状真源
通过现有生成流程同步对应 Candidate shapeDescription 与当前派生报告
增加 Editor-only QA Shape Fixture，使通用两格/拐角/四格算法测试不再借用正式 Item 身份
更新确实使用修正 Item 的 Item QA 布局、断言与 Golden
运行 Enemy/CrossSystem 只读回归
按精确文件白名单迁移仅由 Item105 / 修正目录事实导致变化的 Editor QA baseline
输出 ApprovedMatrix、ChangeLedger、CanonicalDelta 与 Regression 报告
```

禁止：

```text
修改 ItemSystemSnapshot.v1、IF01 或 CrossSystem Runtime Schema/API/Canonical 算法
修改 Enemy Runtime、Layout Resilience Evaluator、Capability Normalization Runtime、BP 映射或阈值
修改基础属性、itemPower、词条、核心定义、Build资格/效果、品阶、Roll/掉落概率
修改 Battle、Reward、RunFlow、Inventory、Save、Scene、Prefab、BuildSettings、Item Detail UI布局或图片资源
运行会重建或覆盖用户 Scene/Prefab/UI 的 Builder
为维持旧 Golden 而保留错误形状、改变 Enemy requirement/压力/clause/route 或削弱 verifier
自动刷新全局 Protected Hash
```

若发现 `Assets/_Game/Scripts/TalismanBag/CrossSystem/` Runtime 内存在第二套 Item 形状真源，或必须修改 Runtime 才能适配，立即退出单包并申请独立 CrossSystem Cleanup；当前三方审计未发现该情况。

### 18.4 Canonical 与历史审计政策

合理允许变化：

```text
Item Catalog / Foundation 中包含 shapeId 的内容签名
包含修正目录或实际布局的 ItemSystemSnapshot / Item105 aggregate
OccupiedCells / coreCellWorld / 点亮链 / 阵脉覆盖 / isLit / isCountedInBuild
BuildDebugSignature 与使用修正 Item 的 P1/P3/P4/P6/P7 结构事实、诊断和 Canonical
Candidate 形状说明及由格数分支正式生成的摆放建议
C01 BuildSourceRevisionId（仅来源修订变化，不代表 BP 值变化）
```

原则上必须稳定：

```text
当前不含形状字段的 Roll / Instance Projection Canonical
SchemaId / SchemaVersion / Canonical 算法 / 稳定排序 / InvariantCulture
Enemy E01-E10、N01B、P2、P5-A、P5-M、requirement/pressure/clause/applicability/migration route
C01 break_power / guard_power / cooldown_recovery 的 BP 数值、聚合公式与映射状态
C02 32/32 blocked、Actual Unknown reduction=0
基础属性、词条、核心与 Build 定义
```

若某项原则稳定签名实际变化，必须追踪到明确 payload 差异并回流 Guard，不能直接更新 baseline。旧值不得无痕覆盖；新包必须输出 `旧值 → 新值` Ledger、修正前后签名和 `superseded` 关系。当前报告可以重生成，但已验收历史证据必须保留来源。

### 18.5 Capability 与结构结果边界

形状、格数、摆放效率和空间覆盖当前不得换算为连续 BP，也不得用伤害、词条、核心或 Build 数值补偿形状变化。Capability Normalization Runtime 不修改。

修正后允许重新投影并产生真实结构差异：越界/碰撞、旋转合法性、核心格位置、接亮、阵脉占格、特定布局下的 Build 计入，以及固定作者化压力下的 Layout Resilience KnownTrue/KnownFalse。核心和 Build 的定义保持不变，但具体实例在某布局中是否点亮、激活或计入可以随空间事实合理变化。

### 18.6 QA 规则

```text
验证正式 Item 身份的测试：按用户批准矩阵更新并保留集成断言
只验证通用形状算法的测试：迁移到 Editor-only QA Shape Fixture
QA Fixture 不得进入正式 Catalog、Candidate、150品阶、掉落池或 ItemSandbox 玩家道具栏
P1 synthetic dev_item_a 等独立合同 fixture 不得仅因真实目录变化而改签名
P6 若使用发生变化的 I001，允许在保持 requirement/pressure 不变的前提下重新验收真实布局结果
P7 当前 Guard Return 不得升级为本包 Scene 手测或 Scene 修改要求
```

最终 Assignment 只能在用户批准30件矩阵、Item Guard 主批准和 Enemy Guard 联合确认后生成。

### 18.7 用户矩阵批准与核心格延期（2026-07-21）

用户已用五张原始占格表截图与其按初始数据制作的150张五品阶道具主图完成交叉确认。最终矩阵为 `30/30 APPROVED`：`14 APPROVED_KEEP / 16 APPROVED_CHANGE / 0 UNRESOLVED`。批准文件为 `Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv`。

I024 的截图文字与美术证据冲突：截图写竖向两格，而五品阶主图均为 `1024×512` 横向水盂。用户明确裁定以已制作美术素材为准，因此 I024 锁定为：

```text
shape_line2_h
ordered cells=(0,0);(1,0)
default orientation=horizontal
```

空间核心格机制延期：

```text
coreCellLocal contract field: KEEP
current status: TECHNICAL_RESERVED
player-facing mechanic: DEFERRED
formal UI / tutorial / artwork marker: NO
balance compensation or BP mapping: FORBIDDEN
future activation: REQUIRES_SEPARATE_VERSION_AND_GUARD
```

本批次不要求用户逐件批准正式核心格设计。`technicalCoreCellLocal` 仅按机械规则保证合法：单格为 `(0,0)`；原坐标在新 cells 内则保留；单纯横竖转换同步坐标；失效时映射到合法对应格。所有行均标记 `TECHNICAL_RESERVED / NOT_PLAYER_RULE_APPROVED / RESERVED_FOR_FUTURE_VERSION`。现有 Sandbox 对核心格的读取只能视为技术预览，不得据此建立正式教学或平衡结论。

批准的16项变更为：

```text
I002  line2_h → line2_v
I004  line2_h → line2_v
I006  line2_h → line2_v
I010  line2_v → corner3
I011  line2_h → line2_v
I014  line3_h → line3_v
I015  corner3 → line3_v
I016  line2_v → corner3
I017  line3_v → line2_v
I021  line2_h → line2_v
I022  line2_v → line3_v
I023  line2_h → line2_v
I024  corner3 → line2_h
I026  line2_h → line3_v
I028  line2_h → line2_v
I030  line3_h → line3_v
```

该批准解除矩阵 `HOLD`，允许下一步生成唯一正式 Assignment；在 Assignment 下发前仍不得修改 Runtime。

### 18.8 Assignment 下发（2026-07-21）

唯一正式任务书已生成：

```text
Docs/V0.4/ItemShapeCatalogBatchCorrection01_Assignment.md
TASK_START_V0.4_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01
GUARD_PASS_ASSIGNMENT_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01
```

状态更新为 `ASSIGNMENT_ISSUED / READY_FOR_TASK_WINDOW`。开发窗口只能执行该 Assignment 与批准矩阵，不得以早期截图、旧报告或历史 I009/I029 小包扩大范围。

### 18.9 实现回执与独占复验 HOLD（2026-07-21）

任务窗口已回传实现结果。Guard 接受以下实现证据，但不接受为最终 PASS：

```text
Package: V0.4-ItemShapeCatalogBatchCorrection01
Implementation: COMPLETE
Auto acceptance: HOLD_EXTERNAL_EDITOR_LOCK_AND_SCENE_DRIFT
Matrix: 30/30
KEEP / CHANGE / UNRESOLVED: 14 / 16 / 0
Unexpected shape changes: 0
I024: shape_line2_h / (0,0);(1,0) / technical core(0,0)
Rotation: 120/120 PASS
Rarity inheritance: 150/150; dual-seed 300/300 PASS
Roll Canonical: STABLE
Projection Canonical: STABLE
Capability BP: STABLE
Enemy Runtime changed: NO
CrossSystem Runtime changed: NO
Item105 baseline ledger: 11/11
C02: 32/32 blocked; Actual Unknown reduction=0
Unity source compile: PASS; error CS=0
Final package marker: NOT_ACCEPTED
```

唯一专项失败为 `protected.scenes`。Guard 复核时：

```text
PID 10076 = Unity.exe
BatchMode = 0
IsHumanControllingUs = 1
Temp/UnityLockfile = PRESENT
protected scene = Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
task-start protected aggregate = E8E0ABC43941ECB689EF47615C4A1B937180D704DC7F503819EEBC593004693C
verifier observed aggregate = E1B4B463234A950E898B07561335A5CB56FA0A5AB2202F9BCCEA474B21703D7B
later live file hash = 4E94389FE0A027E3D7C7E47968A24501426FF5EBCF31643485BA01C763B21508
```

因此当前归因是“人工控制的外部 Unity Editor 持锁并在任务期间改写受保护 Scene”，不是 Item 形状断言失败。禁止任务窗口：

```text
杀死 Unity 进程
回滚或覆盖 Scene
自行批准当前 Scene 新哈希
把 protected.scenes 断言删除、跳过或改成恒真
在工程仍被锁定时继续 batch 重试
发送 PASS marker 或同步 RepoOps 为完成
```

恢复流程：用户先决定当前 Scene 改动是保留还是放弃，并正常关闭 Unity Editor；之后 Guard 根据用户决定批准稳定 Scene 基线或要求保留旧基线，再下发仅限复验/必要基线迁移的 GuardFix。最终必须在无工程锁、Scene 前后稳定的独占环境中运行完整 assignment batch。未完成该流程前状态保持 `IMPLEMENTED / AWAITING_EXCLUSIVE_REVALIDATION`。

### 18.10 关闭 Unity 后的 Guard 独占复验（2026-07-21）

用户关闭 Unity 后，Guard 确认 `Unity process = 0`、`Temp/UnityLockfile = absent`，并执行真实独占 batch：

```text
Entry: TalismanBag.EditorTools.ItemShape.ItemShapeCatalogBatchCorrectionVerifier.VerifyStaticBatch
Log: Logs/codex_item_shape_catalog_batch_correction01_guard_rerun.log
Unity compile: PASS / Tundra build success / error CS=0
Final marker: ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_FAIL errors=2
```

本次不再存在工程锁阻断。两个失败均为受保护资产域偏离任务起始基线：

```text
protected.scenes
expected aggregate: E8E0ABC43941ECB689EF47615C4A1B937180D704DC7F503819EEBC593004693C
actual aggregate:   F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B
tracked dirty:
- Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
- Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity

protected.prefabs
expected aggregate: DC689B674A701A62D4D999EA7DD26579E8B9964E0F15A9CF8B48DF1BC561187D
actual aggregate:   3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6
tracked dirty:
- Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/BattleLikePreviewAreaBridge.prefab
```

矩阵、Catalog、120次旋转、150品阶、300双seed、Candidate/Profile 非形状字段、I024、Roll/Projection Canonical、Capability BP、11处 Item105 Ledger、C02 与全部 Runtime 红线仍为 PASS。故状态从“等待独占复验”细化为：

```text
IMPLEMENTED / AWAITING_USER_PROTECTED_ASSET_DECISION
```

用户必须明确以上两个 Scene 与一个 Prefab 是否属于应保留的外部工作。Guard 未批准前不得更新其 protected expected hashes，也不得回滚这些资产。

### 18.11 用户批准保留资产与 GuardFix01 下发（2026-07-21）

用户明确回复：

```text
PROTECTED_ASSET_DECISION: KEEP_CURRENT_UI_SCENE_PREFAB
```

Guard 在 Unity 关闭且无工程锁时复核稳定聚合：

```text
Scenes 7: F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B
Prefabs 5: 3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6
```

这次批准仅表示上述三个 tracked dirty 资产作为用户外部工作保留，不表示它们属于 Item 形状包，也不允许 GuardFix 修改资产内容。

唯一 GuardFix Assignment：

```text
Docs/V0.4/ItemShapeCatalogBatchCorrection01_GuardFix01_Assignment.md
TASK_START_V0.4_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_GUARDFIX01
```

GuardFix 仅允许迁移形状专项 verifier 的 Scene/Prefab 两个 expected aggregate、刷新专项报告并独占复验。

### 18.12 GuardFix01 自动验收通过（2026-07-21）

任务窗口回执及 Guard 独立复核结论：

```text
Package: V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01
Auto acceptance: PASS
Final marker: ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0
Unity compile: PASS / Tundra build success / error CS=0
Scenes 7 before/after: F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B
Prefabs 5 before/after: 3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6
Protected assets modified by GuardFix: NO
Matrix / Rotation / Rarity / DualSeed: PASS
Roll / Projection Canonical: STABLE
Capability BP: STABLE
Item105 ledger: 11/11
C02: 32/32 blocked / Actual Unknown reduction=0
Runtime changes: NO
```

Guard 同时修正 GuardFix Assignment 中 Roll SHA 少一个字符的纯文档笔误：实际 verifier、原回执与刷新报告始终使用正确的64字符值 `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8`，未发生代码或基线变化。

当前状态为 `AUTO_ACCEPTANCE_PASS / AWAITING_USER_HANDTEST`。用户手测通过前，RepoOps completion 继续 HOLD，不得开始依赖新形状的后续正式包。

### 18.13 用户手测通过与包关闭（2026-07-21）

用户明确回执：

```text
USER_HANDTEST: PASS
```

最终状态：

```text
Package: V0.4-ItemShapeCatalogBatchCorrection01
GuardFix: V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01
Status: COMPLETE / USER_HANDTEST_PASS
Auto acceptance: PASS
User hand-test: PASS
Matrix: 30/30
KEEP / CHANGE / UNRESOLVED: 14 / 16 / 0
I024: shape_line2_h / (0,0);(1,0) / technical core(0,0)
Final marker: ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0
Runtime redline touched: NO
Commit / tag / push: NO
```

`TECHNICAL_RESERVED` 核心格延期政策保持不变。后续 Item、Enemy、Layout Resilience 与 Capability 包可消费本次批准后的 Item shape truth，但仍必须遵守各自 Package Queue、Guard 和 Runtime 边界；本包完成不自动批准任何后续开发包。

## Shared Module Layering / Capability 边界

Capability Algorithm Guard 后续必须同时读取：

```text
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
```

算法只消费稳定 Snapshot / Fact / Contract，输出算法结果；不得读取 Scene、Prefab、
Text、Image、Sprite、中文文案或 Scene-local 状态。Prefab 不得保存 Capability 权威
分数、阈值或通过结论。发现算法进入 View、Prefab 挂载算法真源、或 UI 根据文案自行
计算结果时，必须返回 `GUARD_LAYERING_WARNING`，向用户解释玩家侧风险后停止放行。

Capability / Build 字段进入玩家 UI 时，还必须提供端到端 Field Lineage Matrix，并分开
声明 Fixture、真实 Runtime 与用户手测结果。算法合同存在、Canonical稳定或离线 Fixture
通过，不等于真实 Item/Enemy Runtime 已生产并投影该字段。
