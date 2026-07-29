# V0.4-DevEncounterLayoutPressureAuthoring01 报告

状态：`DEV_COMPLETE / USER_COORDINATE_ACCEPTANCE_REQUIRED`

## Guard 回执

```text
GUARD_PASS_ASSIGNMENT_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01_REVISION01
ENEMY_GUARD_CONFIRM_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01
CAPABILITY_ALGORITHM_GUARD_PASS_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01
```

## 结果

- Schema：`DevEncounterLayoutPressureAuthoring.v1 / 1`
- Canonical Signature：`sha256:8b9ee4674020c5405d7b90cf2793b37be32bf0ea08e8a981291ed6790309da54`
- Context：`4/4`
- Candidate identity：`2`
- PressureInputId：`4/4 unique`
- Domain cells：`25 / 25 / 25 / 25`
- Usable cells：`25 / 25 / 23 / 21`
- Preserved connections：`35 / 35 / 32 / 24`
- 显式作者化 cell rows：`100`
- 显式作者化 edge rows：`126`
- P2 Adapter：`4/4 Complete`
- P2 Project：每个 Context 恰好一次，默认 Catalog 共 `4`
- 直接 N01C Validator / Evaluator：`0 / 0`
- Migration / N01B real row / P3 / P4 / readiness：`0`
- C02 blocked rows：`32/32`
- Actual Unknown reduction: `0`

## 隔离状态

```text
userCoordinateAccepted = false
activated = false
devOnly = true
isEnabled = false
migrationCreated = false
evaluationExecuted = false
```

本包只提供 devOnly 作者化候选。用户必须查看
`DevEncounterLayoutPressureAuthoringMaps.md` 的四张中文 5×5 格图并明确回复通过，之后才可另行评审 overlay migration。
