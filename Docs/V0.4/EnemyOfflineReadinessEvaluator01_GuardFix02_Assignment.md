# V0.4-EnemyOfflineReadinessEvaluator01-GuardFix02 Assignment

Guard rework assignment:

```text
ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX02
```

生成日期：2026-07-15  
所属：Enemy / Boss / Encounter System Guard  
原包：E08 `V0.4-EnemyOfflineReadinessEvaluator01`

## 1. 返工原因

GuardFix01 已正确修复 Verifier、主报告与 Spec：

```text
baseline.e05.fixture
Expected: 4/3/3/3/2
Actual:   4/3/3/3/2
Result:   PASS
```

但 E08 的 LeakCheck 报告仍保留旧值：

```text
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorLeakCheckReport.md
baseline.e05.fixture: PASS (expected 4/3/3/3/2, actual 4/5/4/3/6)
```

原因不是 GuardFix01 开发窗口越界或漏改。GuardFix01 白名单只允许修改 Verifier、主报告与 Spec，而 Verifier 当前对三个派生报告使用 `WriteUtf8IfMissing`：

```text
EnemyOfflineReadinessEvaluatorFieldMatrix.csv
EnemyOfflineReadinessEvaluatorScenarioRows.csv
EnemyOfflineReadinessEvaluatorLeakCheckReport.md
```

因此同源 Verifier 重跑时不会刷新已经存在的派生报告。GuardFix01 的白名单遗漏了这一生成策略问题。

Guard return：

```text
GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_STALELEAKREPORT01
```

## 2. 必须修复

同源 Verifier 每次运行时，五份 E08 报告都必须由当前检查结果确定性重建：

```text
EnemyOfflineReadinessEvaluatorReport.md
EnemyOfflineReadinessEvaluatorSpec.csv
EnemyOfflineReadinessEvaluatorFieldMatrix.csv
EnemyOfflineReadinessEvaluatorScenarioRows.csv
EnemyOfflineReadinessEvaluatorLeakCheckReport.md
```

不得再对 FieldMatrix、ScenarioRows 或 LeakCheck 使用“文件存在就跳过”的生成策略。

修复后 LeakCheck 必须包含：

```text
baseline.e05.fixture: PASS (expected 4/3/3/3/2, actual 4/3/3/3/2)
```

主报告、Spec、FieldMatrix 与 ScenarioRows 在相同输入下必须保持 byte-identical，不得借本修复改变内容。

## 3. 允许内容修改范围

只允许产生内容变化：

```text
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorLeakCheckReport.md
```

运行 Verifier 时允许重新写入以下文件，但最终内容 hash 必须与 GuardFix01 后完全一致：

```text
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorSpec.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorFieldMatrix.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorScenarioRows.csv
```

新增文件必须为 `0`。

## 4. 禁止

```text
不修改 Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
不修改 E01-E07
不改变 15 个 scenario 或任何 Evaluator 计算语义
不改变 PlayerHintProjection 字段
不手工只改 LeakCheck 文字而保留 WriteUtf8IfMissing
不修改 Scene / Prefab / Config / Battle / Board / Item
不触碰原有 BattleSandbox Scene 四处尾随空白
不启动 E09
不 commit / tag / push
```

## 5. 回归要求

必须保持：

```text
Verifier: 212/212 PASS
Scenarios: 15
ReadinessBand: 5/5 covered
Any-All semantics: PASS
Known Zero-Unknown semantics: PASS
Map adjustment-clamp-order: PASS
Player-safe isolation: PASS
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Runtime E08 files: unchanged
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Leak: 0
Package-scope check: PASS
```

必须额外验证：

```text
连续运行同源 Verifier 两次
五份报告第二次生成后内容 hash 与第一次完全相同
删除一个临时副本并重新生成的 LeakCheck 内容与正式 LeakCheck 一致
LeakCheck 不再出现 actual 4/5/4/3/6
```

必须重新运行 Unity batch compile、Unity batch Verifier、同源离线 Verifier、package-scope whitespace check 与全局 `git diff --check`。

全局检查若仍只因开工前无关 BattleSandbox Scene 四处尾随空白失败，继续记录 `PREEXISTING_UNRELATED_DIFF`，不得修改该场景。

## 6. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyOfflineReadinessEvaluator01-GuardFix02

Guard marker:
ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX02

Result:
DEV_COMPLETE / QA_RESULT

E05 baseline across Report / Spec / LeakCheck: 4/3/3/3/2 / PASS
内容修改文件: 必须为 2
新增文件: 必须为 0
Five-report repeat generation determinism: PASS / FAIL
Unchanged four-report hash: 4/4 unchanged
Runtime E08 hash: unchanged
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

GuardFix02 通过前，E08 不签发 Guard PASS，E09 保持 `NOT_RELEASED`。
