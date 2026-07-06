# GUARD_SYNC_UNIFIED_BATTLEPAGE_STRATEGY

日期：2026-07-06
维护方：Codex Guard / 记忆治理窗口
状态：GUARD_MEMORY_SYNC / NOT_ASSIGNMENT / NO_CODE_NO_SCENE_CHANGE

## 1. Guard 收口结论

```text
GUARD_SYNC_UNIFIED_BATTLEPAGE_STRATEGY_ACCEPTED
GUARD_HOLD_UNIFIED_BATTLEPAGE_UNTIL_FORMATION_POWER_LEGACY_FEEDBACK_FIXED
GUARD_KEEP_V03_STABLE_FLOW_AS_REFERENCE_AND_ROUTE_SOURCE
GUARD_KEEP_V04_BUILDSANDBOX_AS_NEW_BATTLE_EXPERIENCE_SOURCE
GUARD_FORBID_CONTINUING_TO_TUNE_V03_AND_V04_AS_TWO_FINAL_BATTLE_PAGES
```

本文件只记录路线判断，不等于新包 assignment，不授权修改 Unity 代码、场景、存档、奖励、数值、RunFlow 或正式章节。

## 2. 核心判断

后续不应把 `Scene_TalismanBag_V02_FormationCounter` / V0.3 战斗页和 `Scene_TalismanBag_V04_BattleSandboxPreview` 同时当成两个最终正式战斗页继续调。

正确路线是：

```text
V0.2 / V0.3 稳定战斗流转
        +
V0.4 BuildSandbox 新战斗体验
        ↓
Unified BattlePage / 统一正式战斗页
```

也就是：

```text
V0.3 取流程骨架。
V0.4 取新玩法和手感。
Unified BattlePage 承接最终正式体验。
```

## 3. V0.2 / V0.3 后续定位

V0.2 / V0.3 不再作为新玩法实验场，也不应继续投入大量 UI 手感调优作为最终页。

它后续提供：

```text
1-10 / 2-10 Golden Path
关卡推进顺序
怪物 / Boss 出场顺序
Boss 前停止
BossInfo / 手动挑战
胜利 / 失败 / 结算
奖励领取
回首页
新手流程与老道具行为参考
正式主线流转稳定性参考
```

默认禁止：

```text
为了 V0.4 新玩法大改 V02RunFlowController
为了多格道具重构 FormationState / PageState
为了新棋盘重写 V02FormationGridFrame
把 V0.4 devOnly 内容直接塞进 1-10 / 2-10 正式流
继续把 V0.3 战斗页作为最终新战斗页反复调 UI
```

## 4. V0.4 BuildSandbox 后续定位

V0.4 BuildSandbox 当前是新能力验证场，不是正式主线入口。

它后续提供：

```text
x2 / x3 / x4 / vertical_3 多格道具
新道具池与旧道具扩展能力
ItemStat / Shape / Synergy / Affix / Modifier
Build 反馈
敌人 / Boss 机制反馈
3-10 / 4-10 devOnly 可玩验证
道具栏与棋盘的新手感参考
开发者调参面板和报告
```

默认禁止：

```text
把当前 V04 sandbox 直接升为正式 BattlePage
把 devOnly 3-10 / 4-10 直接接正式章节
开启正式 FeatureFlag
写正式 SaveData / Reward / Chapter progress
把 V04 的临时反馈面板当成最终玩家 UI
继续忽视阵眼 / 聚能石供能合同 / 旧道具行为缺口
```

## 5. Unified BattlePage 后续定位

Unified BattlePage 是未来正式战斗页，不是简单复制 V0.3，也不是直接把 V0.4 sandbox 原样上线。

它应该吸收：

```text
来自 V0.3：稳定过关流程、Boss 节点、奖励结算、回首页、老道具行为。
来自 V0.4：新棋盘 / 道具栏体验、多格道具、ItemStat、Build 反馈、Boss 机制反馈、3-10 / 4-10 验证内容。
```

它未来承接：

```text
正式战斗手感
正式棋盘 / 道具栏 / 整备页
战斗中剧情 / 引导 / Boss 台词
战斗反馈 / 施法条 / 机制浮字
新章节 3-10 / 4-10 以及后续 5-10 / 6-10 / 7-10
```

## 6. 当前不可直接进入 Unified 的原因

当前 V0.4 虽然已有可玩沙盒，但还不是可直接合入 V0.3 正式流的状态。

主要缺口：

```text
1. V0.4 棋盘还缺阵眼 / 核心格规则。
2. 阵眼 / 聚能石供能合同还未写清：聚能石 / 聚灵石才是唯一正式供能源；醒符香、炉芯石、桃木剑、法印类不得再被视为正式供能源。
3. 0.2 / 0.3 老道具行为还没有完整自然地融入 V0.4 战斗规则。
4. V0.4 战斗反馈能跑，但玩家可读性、胜负节奏、Boss 技能反馈还需要整理。
5. 当前 V0.4 是 devOnly sandbox，不应直接接正式 Save / Reward / Chapter。
```

因此 Guard 判断：先补 V0.4 战斗底座缺口，再进入 Unified BattlePage。

## 7. 推荐后续包顺序

### 7.1 V0.4-FormationEnergyContract01

目标：先把阵眼 / 聚能石 / 供能状态写成稳定数据合同，再继续做表现、战斗反馈或 Unified BattlePage。

核心判断：

```text
阵眼不是正式供能源。
阵眼是阵盘核心 / 阵脉弱激活 / 战斗规则中心。
聚能石 / 聚灵石是唯一正式供能源。
```

必须定义：

```text
EnergyState:
- None
- WeakPulse
- Powered
- Suppressed

BackpackLayoutConfig:
- layoutId
- width / height
- eyeCell
- blockedCells
- availableCells

EyeConfig:
- eyeId
- eyeCell
- eyeStability
- basePulseRange
- basePulseStrength
- baseWeakSupplyRate
- energyBusCapacity
- connectedEnergySourceLimit
- eyeGuardValue
- pollutionResistance
- overloadLimit
- bossPressureResistance

EyeRuntimeData:
- currentStability
- currentGuardValue
- currentPollution
- connectedEnergySources
- eyeAdjacentCells
- basePulseCells
- isOverloaded
- isPolluted
- isSuppressed

EnergyStoneConfig:
- itemId
- energyOutput
- supplyRange
- supplyShape
- connectionRequired
- disconnectedEfficiency
- efficiency
- stability
- targetLimit
- priorityRule
- overloadRisk

EnergyStoneRuntimeData:
- itemId
- anchorCell
- occupiedCells
- connectedToEye
- suppliedCells
- suppliedItemIds
- currentEfficiency
- isOverloaded
- isSuppressed

BattleLayoutSnapshot:
- layoutId
- eyeCell
- eyeRuntime
- placedItems
- occupiedCells
- energySources
- each item EnergyState
```

规则锁定：

```text
只有聚能石 / 聚灵石家族可以产生 Powered。
阵眼只产生 WeakPulse / 阵脉弱激活，不产生正式 Powered。
醒符香 = rhythm support，不是供能源。
炉芯石 = core aura / core mechanic，不是供能源。
桃木剑 = combat item，不是供能源。
法印类 = aura / control / ward，不是供能源。
普通道具不得通过 tag 自动变成供能源。
isPowered 不再作为主判断字段，只能由 energyState == Powered 推导。
```

第一版战斗计算顺序：

```text
1. 读取 BackpackLayoutConfig。
2. 校验 eyeCell 未被道具覆盖。
3. 计算所有 placedItems 的 occupiedCells。
4. 生成 EyeRuntimeData。
5. 计算阵眼 basePulseCells。
6. 识别并连接聚能石 / 聚灵石家族供能源。
7. 为每个道具分配 EnergyState：None / WeakPulse / Powered / Suppressed。
8. None 不触发；WeakPulse 只允许低效基础触发；Powered 才允许完整基础效果、词条、羁绊和 Build 增益。
9. 输出 BattleLayoutSnapshot 与供能诊断。
```

必须新增校验 / 警告：

```text
EyeCellOccupied
EnergyRoleViolation
WeakPulsePoweredViolation
EnergyStoneConnectionViolation
ItemCombatEffectMissing
RhythmTargetViolation
```

不做：

```text
不改正式 SaveData
不改 V0.3 RunFlow
不改正式奖励 / 数值主线
不重画整套 UI
不把 devOnly 供能规则接入正式章节
不让醒符香 / 炉芯石 / 桃木剑 / 法印类继续被识别为正式供能源
```

输出：

```text
FormationEnergyContractReport.md
FormationEnergyStateSchema.csv
FormationEnergyProviderRuleReport.csv
FormationEnergyLeakCheckReport.md
```

### 7.2 V0.4-FormationCoreAndPowerRange01

目标：在 FormationEnergyContract01 之后，补齐阵眼表现、聚能石 / 聚灵石家族供能区域、供能范围、灵力来源、供能反馈。

做：

```text
阵眼 / 核心格数据
聚能石 / 聚灵石供能范围
道具是否被供能的可视反馈
灵力不足 / 供能成功 / 供能断开反馈
阵眼 WeakPulse 与聚能石 Powered 的视觉区分
只在 V0.4 sandbox 内验证
```

不做：

```text
不接正式 SaveData
不改 V0.3 RunFlow
不改正式奖励 / 数值主线
不重画整套 UI
```

### 7.3 V0.4-LegacyItemBehaviorIntegration01

目标：把 V0.2 / V0.3 基础道具行为按家族关系接入 V0.4 sandbox。

做：

```text
火符 / 雷符 / 驱邪铃 / 净化符 / 镇魂符 / 桃木牌 / 聚灵石等基础道具行为
V0.4 进阶道具作为基础道具能力扩展，而不是重复道具
同 family 下 base / advanced / branch 的显示与战斗效果区分
```

不做：

```text
不替换正式 itemId
不改正式掉落
不写正式背包 / 存档
不把 devOnly preview ID 当正式 ID
```

### 7.4 V0.4-BattleFeedbackReadability01

目标：整理玩家侧战斗反馈，让 Boss 技能、机制、道具效果、供能状态都能读懂，但不泄漏完整答案。

做：

```text
Boss 状态 / 施法条 / 机制浮字
道具触发反馈
伤害 / 护盾 / 治疗 / 净化 / 控制反馈
失败原因提示
Build 有效 / 无效的模糊反馈
```

不做：

```text
不显示 hardSolutionTags
不显示 DropBias 权重
不显示 Boss 六钥匙完整答案
不新增大量独立 UI 框，优先复用战斗反馈语言
```

### 7.5 V0.4-MobileRotateZoneInteraction01

目标：替代当前“道具详情面板里的旋转按钮”，把 V0.4 多格道具旋转升级为更干净的移动端拖动热区交互。

定位：

```text
这是手感升级包，不是战斗数值包。
这是 V0.4 多格摆放输入协议的一部分，不是普通 UI 按钮补丁。
必须在当前点击详情 / 拖动摆放 / 合法放置 / 非法弹回主干稳定后再做。
```

最终交互口径：

```text
取消详情角标。
取消道具详情面板里的旋转按钮作为正式方案。
取消点击道具旋转。
取消拖动后二次点击虚影确认。

单次点击道具 = 打开详情。
按住并移动超过拖动阈值 = 拖动摆放 / 移动。
拖动经过棋盘外左侧热区 = 逆时针旋转一次。
拖动经过棋盘外右侧热区 = 顺时针旋转一次。
松手在合法位置 = 放置成功。
松手在非法位置或棋盘外 = 弹回来源。
战斗中单次点击道具 = 打开只读详情。
战斗中拖动道具 = Toast：阵势已启，战后可整备。
```

输入阈值建议：

```text
Tap / 点击详情：
TouchDown 在道具主体区域内
TouchMove 距离 < 12px ~ 15px
TouchUp
= 打开道具详情

Drag / 拖动摆放：
TouchDown 在道具主体区域内
TouchMove 距离 >= 18px
= 进入 DraggingItem
= 本次触摸结束时禁止再触发 OnClick / 打开详情
```

技术实现建议：

```text
不要拖 RectTransform 本身。
拖动数据必须是 ShapeItemPayload。
摆放状态必须由 ShapePlacementSession 统一管理。
道具栏和棋盘都必须通过 ShapeGridReceiver 协议接收。
Ghost / ItemView 只负责渲染 Session 状态，不反向决定逻辑。
```

通用多格摆放锚点规则：

```text
本规则适用于 x1 / x2 / x3 / x4 以及未来所有新增形状。
不得为某个特殊形状单独硬写拖拽逻辑。
新增形状只能新增 ShapeDefinition 配置。
```

多格道具必须拆成三个概念：

```text
GripPoint：手指抓取点，表示玩家按在哪里。
VisualCenter：视觉中心，表示道具如何跟手显示。
PlacementAnchor：放置锚点，表示真正吸附棋盘 / 道具栏格子的逻辑点。
```

最终判定只允许使用：

```text
PlacementAnchor + rotationIndex + occupiedOffsetsByRotation
```

禁止使用：

```text
几何重心作为放置判定
手指坐标直接作为 anchorCell
视觉中心直接作为 anchorCell
RectTransform 当前位置反推放置格
```

ShapeDefinition 必须包含：

```text
shapeId
rotationCount
canRotate
placementAnchorPolicy
occupiedOffsetsByRotation
visualBounds
```

统一放置流程：

```text
1. 玩家拖动道具。
2. 系统计算 PlacementAnchor 的世界坐标。
3. ShapeGridReceiver 把 PlacementAnchorWorldPosition 转成 anchorCell。
4. ShapeDefinition 根据 rotationIndex 返回 occupiedOffsets。
5. occupiedCells = anchorCell + occupiedOffsets。
6. CanPlace 检查全部 occupiedCells 是否在棋盘内、无重叠、无锁格冲突、满足特殊规则。
7. Ghost 渲染 occupiedCells。
8. 松手合法 Commit。
9. 松手非法 ReturnToSource。
```

各形状示例：

```text
x1 单格：
offsets = [(0,0)]
rotationCount = 1
canRotate = false

x2 横竖：
横向 offsets = [(0,0),(1,0)]
竖向 offsets = [(0,0),(0,1)]
rotationCount = 2
canRotate = true

x3 三角 / L 形：
使用拐角格作为 PlacementAnchor。
rotationCount = 4
旋转时保持 anchorCell 不变，只切换 occupiedOffsets。

x4 方块：
offsets = [(0,0),(1,0),(0,1),(1,1)]
rotationCount = 1
canRotate = false
```

体验原则：

```text
玩家不需要手动让所有格子逐一对准棋盘。
玩家只负责拖动整体道具到大概位置。
系统用 PlacementAnchor 自动吸附最近格子。
系统生成完整 occupiedCells 虚影。
只有所有 occupiedCells 合法，才允许放置。
```

手感口径：

```text
手感用跟手。
逻辑用锚点。
合法性看 occupiedCells。
```

旋转热区规则：

```text
只在 DraggingItem / DraggingPlacedItem 状态下显示旋转热区。
x1 单格不显示热区。
x2 横竖显示热区。
x3 三角显示热区。
x4 方块不显示热区。

进入左侧热区：逆时针旋转一次。
进入右侧热区：顺时针旋转一次。
停留在热区内不得连续旋转。
离开热区后再次进入才允许再次旋转。
建议增加 0.15 ~ 0.25 秒旋转冷却。
热区判定优先使用道具视觉中心点，而不是手指点。
```

旋转后必须重新计算：

```text
rotationIndex
occupiedOffsets
occupiedCells
CanPlace
Ghost 合法 / 非法颜色
越界 / 重叠 / 锁格原因
```

如果旋转后会越界或重叠：

```text
旋转失败。
保持原方向。
给出轻反馈：当前位置无法旋转。
不得强行挤开其他道具。
不得把道具弹到未知位置。
```

状态机：

```text
Idle
├─ TapItem -> OpenItemDetail
└─ DragItem -> DraggingItem
              ├─ EnterBoard -> ShowPlacementPreview
              ├─ EnterRotateZoneRight -> RotateClockwiseOnce
              ├─ EnterRotateZoneLeft -> RotateCounterClockwiseOnce
              ├─ ReleaseOnValidBoard -> PlaceItem
              ├─ ReleaseOnInvalidBoard -> ReturnToSource
              └─ ReleaseOutsideBoard -> ReturnToSource

CombatLocked
├─ TapItem -> OpenReadOnlyDetail
└─ DragItem -> ToastLocked
```

战斗锁定 Toast：

```text
阵势已启，战后可整备
```

做：

```text
在 V0.4 sandbox / 后续 Unified BattlePage 测试入口中验证。
复用现有 ShapePlacementSession / ShapeItemPayload / ShapeGridReceiver。
复用现有道具详情面板，但详情面板不再承担旋转职责。
拖动过程中显示左右热区，热区只作为旋转输入，不作为新详情按钮。
确保点击详情和拖动摆放不会同时触发。
确保战斗中只能查看只读详情，不能拖动改阵。
```

不做：

```text
不新增详情角标。
不在道具卡上塞小按钮。
不在详情面板里继续放正式旋转按钮。
不使用 overlay / ignoreLayout / 延后一帧读 drop 的补丁路线。
不重写 V0.3 正式 BattlePrepare。
不改 V0.3 RunFlow / SaveData / Reward / Boss / 正式数值。
不重画整套道具栏 / 棋盘 UI。
```

验收：

```text
单击道具只打开详情，不进入拖动。
拖动道具不会打开详情。
拖动过程中左 / 右热区各能触发一次旋转。
停留热区不会连续狂转。
离开热区后再次进入才能再次旋转。
x1 / x4 不显示旋转热区。
x2 / x3 显示旋转热区。
旋转后 Ghost 占格正确刷新。
旋转后若越界 / 重叠，旋转失败并保持原方向。
松手合法直接放置。
松手非法弹回来源。
战斗中点击可看只读详情。
战斗中拖动只提示“阵势已启，战后可整备”。
Console 无本包红色 Error / 黄色 Warning。
Leak Check = 0。
```

优先级：

```text
先补道具攻击 / 触发 / 生效反馈。
再做当前摆放交互稳定回归。
再做 MobileRotateZoneInteraction01。
通过后先做 BattleContractSurvey01。
合同清楚后再进入 UnifiedBattlePageShell01。
```

### 7.6 V0.4-BattleContractSurvey01

目标：在进入 Unified BattlePage 前，先定义 V0.3 稳定流程、V0.4 BuildSandbox 战斗模块、未来统一战斗页之间的接口合同。只调查和落合同草案，不开发，不改代码，不改场景。

核心判断：

```text
不要直接把 V0.3 稳定流程、V0.4 最新战斗系统和新统一 BattlePage 揉成一个超级整合包。
先统一合同，再统一页面。
```

必须回答：

```text
V0.3 战斗入口有哪些？
V0.3 稳定流程能提供哪些只读数据？
V0.4 战斗模块需要哪些输入？
V0.4 战斗结果应该如何返回流程层？
整备提交的是旧 FormationState，还是新 BattleLayoutSnapshot / ShapePlacementSnapshot？
胜利 / 失败 / Boss 前停止 / 奖励结算由谁负责？
哪些模块只读复用？
哪些模块绝对不能动？
Adapter 应该放在哪一层？
```

合同草案至少包含：

```text
BattleStartRequest：
chapterId / roundId / enemyId / bossId / playerStateRef / initialItemRoster / isBossRound / isTutorial

BattleLayoutSnapshot：
itemId / itemFamily / shapeId / rotation / anchorCell / occupiedCells / isPowered / energySourceId / itemStats / affixes / synergyTags

BattleEventStream：
damage / shield / heal / cleanse / control / bossCast / mechanicHint / itemTriggered / storyLine

BattleResult：
winOrLose / roundId / bossDefeated / duration / buildSummary / rewardClaimToken / nextRouteHint
```

推荐接法：

```text
V0.3 RunFlow
-> 生成 BattleStartRequest
-> Unified BattlePage / V0.4 Battle Kernel 运行
-> 输出 BattleResult
-> V0.3 RunFlow 决定奖励、章节推进、回首页
```

禁止：

```text
V0.4 直接写 SaveData
V0.4 直接发奖励
V0.4 直接推进章节
V0.4 直接改 V02RunFlowController 内部状态
V0.4 直接改正式 BattleScene / FormationGrid / UI Root
未完成 BattleContractSurvey01 前开发 FormalBattleIntegration
未完成 BattleContractSurvey01 前开发正式 3-10 / 4-10 接入
未完成 BattleContractSurvey01 前让 Unified BattlePage 接正式 Save / Reward / Chapter
```

输出：

```text
BattleContractSurveyReport.md
BattleFlowEntrySurvey.csv
BattleSnapshotFieldDraft.csv
BattleAdapterRiskReport.md
ForbiddenTouchList.md
```

### 7.7 V0.4-UnifiedBattlePageShell01

目标：新建或指定统一正式战斗页壳，只搭承载结构，不接正式主线。

做：

```text
BattlePageRoot
BoardArea
ItemTrayArea
EnemyInfo / BossCastBar
BattleFeedbackLayer
Story / Guide / Popup Layer
Result / Reward placeholder
V0.3 flow adapter slot
V0.4 sandbox adapter slot
```

不做：

```text
不直接替换 V0.3 FormationCounter
不接正式章节入口
不写存档 / 奖励 / 章节推进
不把 V04 sandbox 临时 UI 原样当正式 UI
```

### 7.8 V0.4-LegacyChapterBattleAdapter01

目标：把 1-10 / 2-10 的关卡、Boss、奖励、胜负流转以 adapter 方式映射到 Unified BattlePage。

做：

```text
只读读取 V0.3 稳定流程数据
关卡 / Boss / 奖励 / 结算路径映射
不改原 V0.3 Golden Path
```

### 7.9 V0.4-BattleRouteBridge01

目标：让首页 / 试炼入口可以在测试模式进入 Unified BattlePage，但默认不替换正式线。

做：

```text
dev/debug route
FeatureFlag 默认 false
手测路径
回退路径
```

### 7.10 V0.4-GoldenPathBridgeRegression01

目标：统一页接入前做全链路回归。

验收：

```text
1-10 / 2-10 旧流程不坏
V0.4 sandbox 可玩不坏
Unified BattlePage 测试入口可进可退
无 SaveData / Reward / Chapter 意外写入
无 V0.3 / V0.4 双正式战斗页并行污染
```

## 8. 后续内容维护口径

以后加怪物、加道具、加章节、加剧情时按以下归属处理：

```text
新道具 / 新机制 / 新敌人题库：先进入 V0.4 BuildSandbox / devOnly 数据与调参面板。
正式战斗手感 / 棋盘 / 道具栏：进入 Unified BattlePage。
首页、升级、试炼入口、产品流转：仍归 V0.3 MainHome / ProductFlow 相关页面。
战斗中剧情 / Boss 台词 / 机制提示：未来归 Unified BattlePage 的 Story / Feedback / BossCast 层。
1-10 / 2-10 稳定过关流程：继续保留为 V0.3 Golden Path 参考和回归对象。
```

## 9. 红灯条件

以下任一出现，Guard 应拦截：

```text
继续把 V0.3 和 V0.4 当两个最终战斗页分别调
在阵眼 / 供能 / 老道具行为未补齐前直接做 Unified 正式接入
V0.4 devOnly 内容写入正式 SaveData / Reward / Chapter progress
把 V0.4 sandbox 直接接正式 3-10 / 4-10
为了 Unified 直接重构 V02RunFlowController / FormationState / PageState
把玩家侧 UI 做成完整答案面板
把 hardSolutionTags / DropBias / Boss 六钥匙答案显示给玩家
开启默认 FeatureFlag
```

## 10. 一句话收口

```text
V0.3 是稳定流程来源，V0.4 是新战斗体验来源；二者不再各自成为最终页。后续先补 V0.4 的阵眼 / 聚能石供能合同、供能表现、老道具行为和反馈可读性，再新建 Unified BattlePage，把 V0.3 流程和 V0.4 体验合成一套正式战斗页。
```

## 11. 为什么不直接在 V0.3 上改

用户问题：

```text
V0.3 还没上线，为什么之前这些不能直接在 V0.3 上直接改？
```

Guard 收口：

```text
不是因为 V0.3 已上线不能动，而是因为 V0.3 当前承担的是稳定产品流程主线。
```

V0.3 已经证明并承载：

```text
1-10 / 2-10 Golden Path
Boss 前停止
Boss 手动挑战
奖励结算
回首页
首次升级 / 养成入口
首页试炼入口
老道具基础行为
```

V0.4 正在验证的是另一层能力：

```text
多格道具
新棋盘
ShapePlacementSession
聚能石供能
ItemStat
词条 / 羁绊 / Build
敌人 / Boss 机制反馈
3-10 / 4-10 devOnly 验证
```

如果直接在 V0.3 上改，会把这些风险叠在一起：

```text
流程风险：1-10 / 2-10 过关、奖励、回首页可能被打断。
状态风险：RunFlow / PageState / FormationState 可能被新战斗状态污染。
UI 风险：已手调的战斗页、整备页、道具栏、棋盘可能再次被 Runtime / Builder 覆盖。
数据风险：devOnly 3-10 / 4-10、BuildSandbox 道具、词条、羁绊可能误进正式存档 / 奖励 / 章节。
定位风险：V0.3 会同时变成流程页、实验战斗页、统一页，失败后很难归因。
```

因此后续策略不是“永远不改 V0.3”，而是：

```text
V0.3 先保持稳定流程主线。
V0.4 继续作为新战斗能力沙盒。
先做 BattleContract / Snapshot / Adapter。
合同清楚后，再把 V0.3 流程和 V0.4 战斗能力接进 Unified BattlePage。
```

最终合并路径：

```text
V0.3 提供流程骨架和 Golden Path。
V0.4 提供新战斗体验和 Build 数据。
BattleContract 定义两者怎么说话。
Unified BattlePage 承接最终正式战斗页。
```
