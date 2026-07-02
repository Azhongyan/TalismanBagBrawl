# V0.4-BattlePrepareComponentAdapterRuntimePlaytest01-FixtureFeedbackFix01 Assignment

Guard 回执：

```text
GUARD_PASS_RUNTIME_PLAYTEST_FIXTURE_FEEDBACK01
```

## 1. 包定位

```text
RuntimePlaytest 夹具补齐包
```

用户手测已确认：

```text
V0.4 runtime playtest 可以进入 V0.2 / V0.3 成熟 BattlePrepare 战斗页和整备页。
成熟手感路径成立。
```

但用户同时确认：

```text
没有出现 V0.4 多格 / 旋转 / 占格反馈。
原因：没有新道具或 devOnly 测试道具可以触发该扩展层。
```

因此 `V0.4-BattlePrepareComponentAdapterRuntimePlaytest01` 当前只能算：

```text
PARTIAL_PASS_HAND_FEEL_ONLY
V04_EXTENSION_FEEDBACK_NOT_VERIFIED
```

本包目标是补齐最小 devOnly 测试夹具，让用户能真实看到并测试：

```text
多格道具
旋转
占格预览
越界 / 重叠 / 合法反馈
```

## 2. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareComponentAdapterRuntimePlaytest01_Assignment.md
```

## 3. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许新增：

```text
devOnly test fixture items
runtime playtest fixture provider
fixture-only shape / rotation sample data
report / validator
```

建议报告：

```text
Docs/V0.4/Reports/BattlePrepareRuntimePlaytestFixtureFeedbackReport.md
Docs/V0.4/Reports/BattlePrepareRuntimePlaytestFixtureFeedbackLeakCheckReport.md
```

## 4. 必须满足

本包必须让用户在 Unity Play 中看到至少：

```text
Single1
Vertical2
Corner3
Square4
```

以及：

```text
旋转按钮或旋转操作
占格预览
合法放置反馈
越界反馈
重叠反馈
```

这些只能作为：

```text
devOnly runtime playtest fixture
```

不得进入正式道具池、正式掉落、正式存档、正式 1-10 / 2-10。

## 5. 禁止范围

禁止：

```text
重画 V04 棋盘 / 道具栏 / 拖拽 / 上拉动画
替换成熟 BattlePrepare UI
修改正式 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 V02 / V03 场景布局
覆盖用户手调 RectTransform
把测试道具写入正式配置或正式玩家存档
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 6. 用户手测清单

用户需要确认：

```text
1. 仍然进入成熟 V0.2 / V0.3 BattlePrepare 页面与手感。
2. 道具栏中能看到 devOnly 测试道具。
3. 至少能测试 2 格 / 3 格 / 4 格形状。
4. 能旋转。
5. 能看到占格预览。
6. 越界 / 重叠 / 合法放置都有中文反馈。
7. 没有新画一套 UI 取代成熟组件。
8. Console 无本包红色 Error / 黄色 Warning。
```

## 7. 通过条件

只有同时满足：

```text
成熟手感可摸
V0.4 多格 / 旋转 / 占格反馈可摸
测试数据 devOnly 隔离
正式系统未污染
```

才可将 RuntimePlaytest 标记为：

```text
USER_ACCEPTED / RUNTIME_HAND_FEEL_AND_V04_EXTENSION_VERIFIED
```
