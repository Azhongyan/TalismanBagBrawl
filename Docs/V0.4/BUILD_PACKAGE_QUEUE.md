# BuildSandbox Package Queue / 后台沙盒执行队列

状态：`PHASE_COMPLETE / WAITING_REPOOPS_CHECKPOINT`

维护方：Codex Guard / 记忆治理窗口

权威边界：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
用户最新明确指令
```

## 0. 当前 Guard 结论

```text
GUARD_ACCEPT_AS_PARALLEL_SANDBOX_QUEUE
GUARD_DO_NOT_BLOCK_ON_UI_HAND_TUNE
GUARD_KEEP_ISOLATED_FROM_V0.3_UI_MAINLINE
GUARD_PHASE1_COMPLETE_PHASE2_QUEUE_OPENED
```

用户意图：

```text
用户继续手调 UI / 美术 / 位置 / 尺寸。
Codex 不等待，转入后台 BuildSandbox 技术队列。
```

## 1. Light Path

本队列日常执行流程：

```text
Build Package Queue
→ Guard 给当前包 assignment
→ 开发窗口执行
→ 自动验证 / 报告导出
→ 用户确认
→ TASK_STATUS_SYNC_TO_GUARD_REPOOPS
→ Guard 更新队列
→ RepoOps 记录状态
```

不再每包回 PM / Architect / TaskWriter。

## 2. 当前队列

| 顺序 | 包体 | 状态 | 定位 | 下一步 |
| --- | --- | --- | --- | --- |
| 0 | `V0.3/V0.4-BuildSandbox-GuardBaseline01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 沙盒护栏、FeatureFlag、devOnly 隔离、Config Validation、UI Layout Guard 入口 | 用户确认“没问题”；等待 RepoOps 记录 |
| 1 | `V0.3/V0.4-BuildSandbox-SynergyDataFoundation01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | Build / 羁绊 / 标签数据底座 | 用户确认“SynergyDataFoundation01 通过”；等待 RepoOps 记录 |
| 2 | `V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 多格道具占格形状 / 摆放检测底座 | 用户确认“ItemShapeOccupancy01 通过”；等待 RepoOps 记录 |
| 3 | `V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 纯函数羁绊计算器 | 已完成；等待 RepoOps 记录 |
| 4 | `V0.3/V0.4-BuildSandbox-AffixRaritySandbox01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 品质 / 词条沙盒 | 已补齐 white / green / blue / purple / orange 五档与橙色核心词条；等待 RepoOps 记录 |
| 5 | `V0.3/V0.4-BuildSandbox-ModifierEventBridge01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | Build 结果到 Modifier / Event 的中间桥 | Unity batch PASS；报告 PASS；等待 RepoOps 记录 |
| 6 | `V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | Build 自动模拟与 Benchmark 报告 | 用户确认“BuildSimulationBenchmark01 通过”；等待 RepoOps 记录 |
| 7 | `V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | devOnly 敌人 / Boss 验证池 | Unity batch PASS；报告 PASS；等待 RepoOps 记录 |
| 8 | `V0.3/V0.4-BuildSandbox-DevChapterContentPool01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | devOnly 开发章节 / 关卡池 | 用户确认“可以了”；等待 RepoOps 记录 |
| 9 | `V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 照灯账本 / 成长手册 Build 任务钩子 | 用户非 Play 菜单验证无红黄报错；等待 RepoOps 记录 |
| 10 | `V0.3/V0.4-BuildSandbox-ConfigValidatorReport01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 配置校验与报告导出 | Unity batch PASS；报告 PASS；等待 RepoOps 记录 |
| 11 | `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01` | `USER_ACCEPTED / QA_PASSED_BY_USER / WAITING_REPOOPS_RECORD` | 全链路干跑，不正式打开 FeatureFlag | Unity batch PASS；最终 5 份报告 PASS；等待 RepoOps checkpoint |

## 3. 当前包 assignment

当前包：

```text
无。BuildSandbox 当前技术队列已阶段完成。
```

正式 assignment：

```text
无新包。等待 RepoOps 记录 BuildSandbox phase checkpoint。
```

Guard 回执：

```text
GUARD_BUILDSANDBOX_PHASE_COMPLETE_WAITING_REPOOPS_CHECKPOINT
```

下一阶段队列：

```text
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
```

## 4. 每包统一禁止

所有包默认禁止：

```text
修改当前 UI Scene / RectTransform / 用户手调布局
修改正式 1-10 / 2-10 主线
修改 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改 DamageText / V02FormationGridFrame
默认启用 FeatureFlag
让 devOnly 内容进入正式流程
commit / tag / push
```

## 5. 每包统一回执

任务窗口完成后同步：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
完成状态：
修改文件清单：
FeatureFlag 默认关闭检查：
devOnly 隔离检查：
Config Validate：
UI Layout Guard：
报告输出：
是否触碰禁止范围：
用户确认：
下一步建议：
```
