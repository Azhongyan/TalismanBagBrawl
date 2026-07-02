# V0.4 BuildSandbox Integration Strategy

维护方：Codex Guard / 记忆治理窗口

状态：

```text
GUARD_SYNC_STABLE_BATTLE_RUNTIME_PLUS_BUILDSANDBOX_MODULES_ACCEPTED
GUARD_FORBID_DUPLICATE_FULL_BATTLE_SYSTEM
GUARD_FORBID_FORCED_REFACTOR_OF_STABLE_BATTLE
```

## 1. 核心结论

V0.4 BuildSandbox 与 V0.2 / V0.3 稳定战斗的关系不是二选一。

正确路线是：

```text
稳定主链路保留。
V0.4 新能力隔离开发。
纯逻辑模块干净设计。
表现层在 V0.4 沙盒验证。
最终通过 Snapshot / Adapter 合入。
```

一句话：

```text
不要复制两套战斗，也不要拆坏旧战斗。
```

## 2. 三层策略

### 2.1 第一层：V0.2 / V0.3 Stable Battle Runtime

V0.2 / V0.3 是稳定战斗主链路：

```text
V0.2 / V0.3 Battle = Stable Battle Runtime
```

职责：

```text
1-10 / 2-10 主流程
棋盘显示
伤害数字
道具触发
Boss 前停止
Boss 手动触发
奖励结算
升级生效
```

原则：

```text
只修 Bug。
不大重构。
不承载 V0.4 实验。
不为了 V0.4 拆坏主链路。
```

### 2.2 第二层：V0.4 BuildSandbox 能力模块

V0.4 新能力先在沙盒中作为独立能力模块开发：

```text
ShapePlacementSession
ShapeGridReceiver
ShapeItemPayload
ItemShapeOccupancy
SynergyEvaluator
AffixEvaluator
BuildReadinessCheck
BuildSimulationBenchmark
ModifierEventBridge
```

这些模块应尽量：

```text
纯逻辑
devOnly
featureFlag false
不直接依赖正式 UI
不直接改正式战斗场景
不写正式存档
```

输入：

```text
BattleLayoutSnapshot
ItemConfig
ShapeConfig
AffixConfig
SynergyConfig
Enemy / Boss problem config
```

输出：

```text
PlacementResult
SynergyResult
ModifierBundle
BuildReadinessReport
BenchmarkReport
```

### 2.3 第三层：Snapshot / Adapter 合入

当 V0.4 能力模块稳定后，未来合入正式战斗必须走：

```text
V0.2 / V0.3 Battle
→ BattleLayoutSnapshot
→ V0.4 Shape / Synergy / Affix 模块
→ ModifierBundle / PlacementResult
→ BattleSystem 按需读取
```

禁止直接：

```text
V0.4 模块修改正式 BattleScene
V0.4 模块改 RunFlow
V0.4 模块改正式 SaveData
V0.4 模块替换正式拖拽 / 棋盘 / 战斗结算
```

## 3. 不做两套完整战斗系统

禁止把 V0.4 写成完整独立正式战斗系统。

不得出现：

```text
两套伤害逻辑
两套道具触发
两套棋盘
两套拖拽
两套结算
两套奖励
两套存档
两套正式 UI 状态
```

V0.4 可以有：

```text
BattleSandboxPreviewScene
devOnly 可玩验证舱
调参面板
报告
模拟器
```

但这不是正式第二战斗系统。

## 4. 不强行重构旧系统

禁止为了 V0.4 直接重构 V0.2 / V0.3 成熟战斗主链路。

不得因为 V0.4 多格摆放而直接拆：

```text
RunFlow
FormationState
PageState
V02FormationGridFrame
DamageText
BossInfo / Boss trigger
Reward / Drop / SaveData
```

如果未来需要合入，只能通过：

```text
只读快照
Adapter
FeatureFlag
小范围 seam
分阶段 Promote
```

## 5. V0.4 多格道具当前路线

当前 V0.4 多格摆放不再继续强接 V0.3 BattlePrepare runtime 内核。

原因：

```text
V0.3 BattlePrepare 原生不是 shape-aware grid。
旧 Layout / Drag / Drop / Commit 权威和 V0.4 多格摆放冲突。
强行接入成本过高，且已在多包手测中证明不稳定。
```

当前路线：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
→ V0.4 ShapePlacementSession
→ V0.4 ShapeAwareItemTrayGrid
→ V0.4 ShapeGridReceiver
→ V0.4 MobileShapePlacementInput
```

目标：

```text
先在 V0.4 独立沙盒中跑通 x2 最小闭环。
再扩 x3 / x4。
再做手感调优。
最后输出 PromoteCandidateDraft。
```

## 6. 哪些可以复用

可以复用或只读参考：

```text
itemId / item config
ResourceService
RewardService
UpgradeService
EnemyConfig
StageConfig
伤害数字表现思路
战斗反馈语言
BossInfo 表达语言
战斗结算入口
Boss 前停止规则
```

## 7. 哪些不要硬复用

不要硬复用：

```text
旧 ItemTray 的 LayoutGroup 内部逻辑
旧单格拖拽逻辑
旧 drop 后处理逻辑
旧 Ensure / Builder UI 生成逻辑
旧 FormationGrid 内部坐标假设
```

这些只能作为：

```text
视觉参考
手感参考
未来 Adapter / Snapshot 的参考
```

不能作为 V0.4 多格摆放的一等状态源。

## 8. 后续合入原则

V0.4 通过沙盒验证后，不得自动进入正式主线。

只能输出：

```text
PromoteCandidateDraft
```

正式合入必须另开：

```text
V0.4-PromoteToMainline01
```

并按顺序：

```text
1. 先合纯配置候选，不开正式入口。
2. 再合只读展示。
3. 再开 dev/debug FeatureFlag。
4. 再接战斗效果。
5. 最后才考虑正式 3-10 / 4-10 章节。
```

## 9. 当前 Guard 裁决

```text
V0.2 / V0.3 Stable Battle Runtime 保留。
V0.4 BuildSandbox 作为独立能力模块开发。
Scene_TalismanBag_V04_BattleSandboxPreview 作为新能力验证舱。
V0.4 多格摆放先在沙盒中跑通，不继续强接旧 BattlePrepare runtime。
未来通过 Snapshot / Adapter 合入，而不是复制第二套正式战斗，也不是拆坏旧战斗。
```
