# V0.4 BuildSandbox Phase 2 Package Queue

状态：`ACTIVE_PHASE2_PLAYABLE_PREVIEW_QUEUE`

维护方：Codex Guard / 记忆治理窗口

权威边界：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BUILD_SANDBOX_INTEGRATION_STRATEGY.md
用户最新明确指令
```

## 0. Guard 结论

```text
GUARD_SYNC_BUILD_PROBLEM_RULE_POOL_ACCEPTED
GUARD_ACCEPT_BUILDSANDBOX_PHASE2_PLAYABLE_PREVIEW_QUEUE
GUARD_KEEP_PHASE2_DEVONLY_AND_ISOLATED
GUARD_SYNC_STABLE_BATTLE_RUNTIME_PLUS_BUILDSANDBOX_MODULES_ACCEPTED
```

## 1. Phase 2 定位

```text
Phase 1 = 上层发动机，已完成。
Phase 2 = 中层题库规则 + 下层可玩验证舱。
```

Phase 2 目标是把 BuildSandbox 从后台报告推进到独立可玩沙盒。

Phase 2 不是正式 3-10 / 4-10 开发，不进入 V0.3 产品主线，不打开正式 FeatureFlag。

## 2. Light Path

```text
Phase 2 Package Queue
→ Guard 给当前包 assignment
→ 开发窗口执行
→ 自动验证 / 报告导出
→ 用户确认
→ TASK_STATUS_SYNC_TO_GUARD_REPOOPS
→ Guard 更新队列
→ RepoOps 记录状态
```

## 3. 当前队列

| 顺序 | 包体 | 状态 | 定位 | 下一步 |
| --- | --- | --- | --- | --- |
| 0 | `V0.4-BuildSandboxPhase2RoadmapGuard01` | `GUARD_DONE / PHASE2_QUEUE_OPENED / WAITING_REPOOPS_RECORD` | Phase 2 路线、队列、边界落盘 | 等待 RepoOps 记录 |
| 1 | `V0.4-BuildProblemRulePool01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 地图规则 / 敌人题目 / Boss 多钥匙锁 / Readiness / DropBias 配置层 | 规则层静态 PASS；Unity batch 未捕获但已如实记录；等待 RepoOps 记录 |
| 2 | `V0.4-BuildProblemSeedData01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 10 地图规则、10 敌人题目、6 Boss、弱点窗口、失败提示、DropBias 种子数据 | 用户菜单 QA 通过；等待 RepoOps 记录 |
| 3 | `V0.4-BuildSandboxConfigPanel01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | V0.4 devOnly 配置面板，参考 Stage Config Panel 01 但独立数据源 | 用户菜单 QA 通过；等待 RepoOps 记录 |
| 4 | `V0.4-BattleSandboxPreviewScene01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 独立可玩沙盒场景 `Scene_TalismanBag_V04_BattleSandboxPreview` | Unity batch PASS；场景骨架报告 PASS；等待 RepoOps 记录 |
| 5 | `V0.4-BuildSandboxPreviewContext01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 统一 PreviewContext / ViewModel，汇总 Phase 1 + 题库数据 | Unity batch PASS；Context / ViewModel / LeakCheck 报告 PASS；等待 RepoOps 记录 |
| 6 | `V0.4-BattlePageViewAdapter01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 战斗整备 / 战斗提示 / 信息反馈 UI 语言只读适配，不反向写正式 UI | Unity batch PASS；五份报告 PASS；等待 RepoOps 记录 |
| 7 | `V0.4-BuildGridInteractionPreview01` | `DEV_DONE / BATCH_PASS / TEMP_PREVIEW_ONLY / UI_REUSE_CORRECTION_REQUIRED` | 沙盒拖拽、多格、旋转、越界、重叠、道具栏滚动、分类筛选；仅保留逻辑验证价值，不作为正式 UI 真源 | 用户只需判断基础拖拽/旋转/反馈是否可用；不要继续精修 V04 临时 UI 手感 |
| 7A | `V0.4-UIReuseSourceRegistry01` | `GUARD_DONE / RULES_LOCKED` | 登记 V0.2 / V0.3 成熟 UI / 动画 / 交互手感为优先复用源，防止 V0.4 重画 | 已落盘 `Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md` |
| 7B | `V0.4-BuildGridInteractionPreview01-UIReuseCorrection01` | `NEXT_CORRECTION / GUARD_PASS_UIREUSE_CORRECTION01 / READY_FOR_DEV` | 审计 7 包临时 UI，标记 temporary preview，建立 V0.4 数据 → 成熟 UI 的 Extension / Adapter 映射草案 | assignment：`Docs/V0.4/BuildGridInteractionPreview01_UIReuseCorrection01_Assignment.md`；不改正式场景、不改手调布局 |
| 7C | `V0.4-ReusableComponentExtensionRule01` | `GUARD_DONE / RULES_LOCKED` | 锁定“成熟组件优先扩展，不重建”：新玩法加 Extension Layer / Adapter，不重画职责重叠组件 | 已落盘 `Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md` |
| 8 | `V0.4-BuildTuningDataPanelPreview01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 统一开发者调参数据面板：BuildSummary / Synergy / Shape / AffixModifier / Readiness / Simulation / DropBias；字段层中文名 + English stable key，玩家 UI 只中文 | 用户已确认通过；等待 RepoOps 记录 |
| 9 | `V0.4-BattlePrepareComponentAdapter01` | `BATCH_PASS / USER_ACCEPTED_CANDIDATE / WAITING_HAND_FEEL_PLAYTEST` | V0.4 BuildSandbox 数据只读映射到成熟 BattlePrepare 组件扩展点；证明不重画 UI、可做 Extension / Adapter | 报告 PASS；需要 playtest 包验证真实手感 |
| 9A | `V0.4-BattlePrepareComponentAdapterPlaytest01` | `STATIC_PASS / DEVONLY_SAFE / NOT_RUNTIME_HAND_FEEL_VERIFIED` | devOnly 静态手感验证 probe；证明安全但没有真实 Play 手感验证 | 已完成静态报告；不能代替运行时体验 |
| 9B | `V0.4-BattlePrepareComponentAdapterRuntimePlaytest01` | `PARTIAL_PASS_HAND_FEEL_ONLY / V04_EXTENSION_FEEDBACK_NOT_VERIFIED` | 已确认能进入成熟 V0.2 / V0.3 BattlePrepare 页面和手感；但没有新道具，未看到 V0.4 多格 / 旋转 / 占格反馈 | 需要 Fix01 补 devOnly fixture |
| 9C | `V0.4-BattlePrepareComponentAdapterRuntimePlaytest01-FixtureFeedbackFix01` | `PARTIAL_PASS_BOARD_OCCUPANCY_FEEDBACK / ITEMTRAY_SHAPE_AWARE_DISPLAY_NOT_VERIFIED` | devOnly 测试道具已进入成熟托盘，棋盘占格反馈可测；但道具栏内仍不是 x2/x3/x4 真实形状 | 需要 Fix02 做 Shape-aware ItemTray |
| 9C-Fix02 | `V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix02-ShapeAwareItemTray` | `SHAPE_VISIBLE_BUT_PACKING_AND_DRAG_FAILED / USER_QA_NOT_PASSED` | 多格 footprint 已能显示，但道具栏内排布错误、超出托盘、拖拽后无法固定并回到错误初始位置 | 需要 Fix03 |
| 9C-Fix03 | `V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag` | `REJECTED_DO_NOT_CONTINUE / USER_QA_FAILED / STRUCTURAL_DIRECTION_FAILED` | overlay + ignoreLayout + 延后一帧读 drop 路线导致视觉层 / Layout 层 / 拖拽层 / drop 层 / anchor 状态层多套权威抢位置；不建议继续补 | 停止 Fix03 路线 |
| 9D | `V0.4-MobileShapePlacementInteraction01` | `CODE_DONE / USER_ACCEPTED_PROTOCOL_LAYER / NEEDS_RUNTIME_INTEGRATION` | 移动端多格道具摆放输入协议：点选拿起、点选旋转、拖动虚影、松手锁定、点击虚影确认 | 协议层已完成，但 RuntimePlaytest 完整链路未稳定 |
| 9E | `V0.4-ShapePlacementSession01-TechSurvey` | `SURVEY_DONE / REDESIGN_ACCEPTED` | 停止补丁路线，调查 ShapePlacementSession + ShapeItemPayload + ShapeGridReceiver 统一状态源方案 | Survey 已完成，ShapePlacementSession 方向接受 |
| 9F | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` | `STOPPED_FOR_NOW / SWITCH_TO_V04_PREVIEW_SCENE` | 强接 V0.3 BattlePrepare runtime 仍未跑通；不再继续当前接入路线 | 保留报告与代码作参考，不作为当前推进方向 |
| 9F-Seam01 | `V0.4-BattlePrepareExtensionSeam01` | `DONE_AS_REFERENCE / NOT_CURRENT_ROUTE` | 成熟 BattlePrepare seam 已验证可加，未来回迁 V0.3 可参考；当前不继续强接 V0.3 runtime | 保留为参考 |
| 9G | `V0.4-BattleSandboxShapePlacementVerticalSlice01` | `USER_QA_FAILED / INPUT_CONFLICT_AND_TRAY_SHAPE_FAILED` | 回到 `Scene_TalismanBag_V04_BattleSandboxPreview`，用 V0.4 ShapePlacement 内核跑通 x2 最小真实闭环；视觉表层尽量复刻 V0.2/V0.3 | 用户手测发现：单次点击道具当前打开物品信息，与“点击再点击旋转”冲突；x2 道具未在道具栏中真实 2 格显示 |
| 9G-Fix01 | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixInputConflict01` | `USER_QA_FAILED / INTERACTION_STILL_TOO_COMPLEX` | 修正 x2 垂直切片的输入语义与托盘形状显示，不扩 x3/x4 | 用户决定砍掉复杂交互：不再做持有态 / 虚影态旋转，不再继续二次确认复杂链路 |
| 9G-Fix02 | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` | `DEV_DONE / STATIC_READY / UNITY_BATCH_INCOMPLETE / WAITING_USER_HANDTEST` | 回退到更简单稳定的摆放逻辑：只允许在道具栏内点击旋转按钮，旋转好后再拖到棋盘放置 | 等待用户 Play 手测：托盘内 Rotate、拖动后不再旋转、合法松手直接放置、非法松手返回托盘 |
| 10 | `V0.4-MechanicHintFeedbackPreview01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 地图机制 / 敌人机制 / Boss 技能 / 失败反馈的玩家侧线索预览，复用既有战斗提示 UI 语言，不显示答案 | 用户已确认通过；等待 RepoOps 记录 |
| 11 | `V0.4-DevChapterBalanceRun01` | `QUEUED / WAITING_GUARD_ASSIGNMENT` | devOnly 3-10 / 4-10 难度曲线与调参验证流，不是玩家正式章节 | 等机制反馈预览稳定 |
| 12 | `V0.4-BuildSandboxPlayableRegression01` | `QUEUED / WAITING_GUARD_ASSIGNMENT` | Phase 2 整体验收与 PromoteCandidateDraft | 等全部包通过 |

## 4. 当前包 assignment

当前包：

```text
V0.4-BattleSandboxShapePlacementVerticalSlice01
```

正式 assignment：

```text
Docs/V0.4/BattleSandboxShapePlacementVerticalSlice01_Assignment.md
```

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_SHAPE_PLACEMENT_VERTICAL_SLICE01
```

最新 Guard 裁定：

```text
此前强接 V0.2 / V0.3 BattlePrepare runtime 的路线暂时停止。
V0.4 多格摆放先回到 Scene_TalismanBag_V04_BattleSandboxPreview 独立沙盒场景。
视觉表层可以尽量复制 V0.2 / V0.3 成熟 BattlePrepare。
交互内核必须使用 V0.4 ShapePlacementSession / ShapeAwareItemTrayGrid / MobileShapePlacementInput。
当前只跑通 x2 最小真实闭环，不扩 x3 / x4，不做 Boss，不做章节。
本包通过后，才能进入后续 VerticalSlice02 / x3 x4 扩展。
```

当前记录：

```text
开发窗口已回报 DEV_DONE。
Unity scene binder 已保存 V04 Preview Scene。
Unity validator completed errors=0, warnings=0。
报告：
Docs/V0.4/Reports/BattleSandboxShapePlacementVerticalSliceReport.md
Docs/V0.4/Reports/BattleSandboxShapePlacementStateReport.csv
Docs/V0.4/Reports/BattleSandboxShapePlacementLeakCheckReport.md

用户手测不通过。
失败原因：
1. 单次点击道具当前用于打开物品信息窗口，与“点击已选中道具旋转”发生输入语义冲突。
2. x2 测试道具没有在道具栏中真实显示为 2 格。

Guard 当前切到 FixInputConflict01。
FixInputConflict01 开发窗口已回报完成。
静态检查通过，Unity batch 因当前工程已有 Unity Editor 打开而阻断。
Guard 当前只记录为 WAITING_USER_HANDTEST。
未通过用户手测前，不进入 VerticalSlice02。

FixInputConflict01 手测重点：
1. 道具栏中 `护阵木牌 / Vertical2` 显示为真实 2 格。
2. 单击道具只打开 / 刷新物品信息，不触发旋转。
3. 拖动道具可以拿起并进入摆放流程。
4. 持有态 / 虚影态有明确 Rotate 按钮。
5. 点击 Rotate 按钮可以旋转 x2。
6. 拖到棋盘出现同形状 GhostPreview。
7. 松手后只锁定虚影，不正式放下。
8. 点击 GhostPreview 后才正式提交。
9. 取消按钮可清空拿起 / 虚影状态。
10. Console 无本包红色 Error / 黄色 Warning。

FixInputConflict01 用户手测仍不通过。
最新用户裁剪：
1. 复杂交互逻辑先砍掉。
2. 仍使用以前更直接的拖放逻辑。
3. 旋转不允许在拖动中、持有态、虚影态或棋盘上发生。
4. 旋转只允许在道具栏中点击道具自身的旋转按钮发生。
5. 旋转完成后再拖到棋盘放置。
6. 如果旋转后会与其他物体重叠或越界，旋转不成功，保持原方向。

Guard 当前切到 FixSimplifiedTrayRotate01。
FixSimplifiedTrayRotate01 开发窗口已回报完成。
静态扫描通过，Unity batch 未完成。
Guard 当前只记录为 WAITING_USER_HANDTEST。
未通过该简化包用户手测前，不进入 VerticalSlice02。

FixSimplifiedTrayRotate01 手测重点：
1. 道具栏中 `护阵木牌 / Vertical2` 默认显示为竖向真实 2 格。
2. 单击道具主体只打开 / 刷新物品信息。
3. 道具栏内有明确 Rotate 按钮。
4. 点击 Rotate 按钮后，Vertical2 在道具栏中变为横向 2 格。
5. 再点 Rotate 可回到竖向 2 格。
6. 如果旋转后越界或重叠，旋转失败，方向保持不变。
7. 拖动道具到棋盘，方向保持拖动前在道具栏中设置好的方向。
8. 合法位置松手后直接放置成功。
9. 非法位置松手后不放置，返回道具栏原位置。
10. 拖动中、棋盘上、预览中都不能旋转。
11. Console 无本包红色 Error / 黄色 Warning。
```

## 5. Phase 2 统一禁止

所有 Phase 2 包默认禁止：

```text
修改当前 UI Scene / RectTransform / 用户手调布局
修改正式 1-10 / 2-10 主线
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 EnemyDefinition / V02RunConfig / DropTable / BossConfig
默认启用 FeatureFlag
让 devOnly 内容进入正式流程
BattlePageViewAdapter 反向写正式 UI
给每个机制新增独立 UI 框而不复用既有 UI 语言
重复重画 V0.2 / V0.3 已成熟 UI / 动画 / 交互手感
把 V04 temporary preview UI 当作正式 UI 真源
未经 Guard 说明理由就绕过 mature UI reuse source 另造相似 UI
因为新增功能就重建职责重叠的大组件，而不是在成熟组件上加 Extension / Adapter
玩家侧直接显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙完整答案
在 Hierarchy 中新增中文 GameObject 名
用中文对象名作为脚本查找路径 / 稳定 id
commit / tag / push
```

## 6. Phase 2 统一回执

任务窗口完成后同步：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
完成状态:
修改文件清单:
FeatureFlag 默认关闭检查:
devOnly / isEnabled 隔离检查:
正式流程泄漏检查:
报告输出:
是否触碰禁止范围:
用户确认:
下一步建议:
```
