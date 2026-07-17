# V0.4-EnemyOfflineReadinessEvaluator01-GuardFix01 Assignment

Guard rework assignment:

```text
ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX01
```

生成日期：2026-07-15  
所属：Enemy / Boss / Encounter System Guard  
原包：E08 `V0.4-EnemyOfflineReadinessEvaluator01`

## 1. 返工原因

E08 的运行时实现、玩家字段隔离、15 个 scenario 与 16 文件白名单均通过 Guard 静态复审，但 E05 fixture 基线报告存在自相矛盾：

```text
EnemyOfflineReadinessEvaluatorReport.md
baseline.e05.fixture
Expected: 4/3/3/3/2
Actual:   4/5/4/3/6
Result:   PASS
```

E05 权威口径是：

```text
4 SkillPattern
3 SkillSequence
3 CarrierSkillBinding
3 BossPhaseProfile
2 BossPhasePlan
```

当前 Verifier 的 PASS 布尔表达式按逻辑实体计数，结果正确；但 `Actual` 字符串使用 CSV 展开行数，分别把 SequenceStep、CarrierSkillBinding 展开项和 BossPhasePlanEntry 当成实体总数，因此写出了 `4/5/4/3/6`。

这会让报告出现“Expected 与 Actual 不相等但 PASS”，不能作为后续 E09 的可信基线。

Guard return：

```text
GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_BASELINEREPORT01
```

## 2. 必须修复

只修 E05 fixture 基线的实际值计算与生成报告：

```text
SkillPattern = SkillPattern 行数
SkillSequence = SkillSequenceStep 的 sequenceId 去重数
CarrierSkillBinding = CarrierSkillBinding 的 carrierId 去重数
BossPhaseProfile = BossPhaseProfile 行数
BossPhasePlan = BossPhasePlanEntry 的 boss carrierId 去重数
```

修复后必须得到：

```text
Expected: 4/3/3/3/2
Actual:   4/3/3/3/2
Result:   PASS
```

同一个计算结果必须同时用于 `Actual` 展示和 PASS 判定，不得继续维护一套显示计数与另一套判定计数。

## 3. 允许修改范围

只允许修改：

```text
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorSpec.csv
```

新增文件必须为 `0`。

`Report.md` 与 `Spec.csv` 必须由修复后的同源 Verifier 重新生成，不得手工只改报告文字掩盖 Verifier 问题。

## 4. 禁止

```text
不修改 Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
不修改 E01-E07
不修改 ScenarioRows 或 FieldMatrix 内容
不改变 15 个 scenario、ReadinessBand、Any/All、Unknown、Map adjustment 或签名语义
不修改 PlayerHintProjection 字段
不修改 Scene / Prefab / Config / Battle / Board / Item
不触碰原有 BattleSandbox 场景尾随空白
不启动 E09
不 commit / tag / push
```

## 5. 回归要求

必须保持：

```text
Scenarios: 15
ReadinessBand: Unknown / Blocked / Strained / Ready / Strong 全覆盖
Any-All semantics: PASS
Known Zero-Unknown semantics: PASS
Map adjustment-clamp-order: PASS
Player-safe isolation: PASS
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Leak: 0
GUID conflicts: 0
Package-scope check: PASS
```

必须重新运行：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
package-scope whitespace check
global git diff --check
```

全局 `git diff --check` 若仍只因开工前无关 BattleSandbox Scene 的四处尾随空白失败，继续记录 `PREEXISTING_UNRELATED_DIFF`，不得修改该场景。

## 6. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyOfflineReadinessEvaluator01-GuardFix01

Guard marker:
ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX01

Result:
DEV_COMPLETE / QA_RESULT

E05 fixture baseline: Expected 4/3/3/3/2 / Actual 4/3/3/3/2 / PASS
修改文件: 必须为 3
新增文件: 必须为 0
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: 通过数 / 总数
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Leak: 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
未 commit / tag / push
```

GuardFix01 通过前，E08 不签发 Guard PASS，E09 保持 `NOT_RELEASED`。
