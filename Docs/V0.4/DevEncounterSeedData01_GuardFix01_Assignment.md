# V0.4-DevEncounterSeedData01-GuardFix01 Assignment

Guard rework marker:

```text
ENEMY_GUARD_REWORK_DEVENCOUNTERSEEDDATA01_GUARDFIX01
```

生成日期：2026-07-16
所属：Enemy / Boss / Encounter System Guard
原包：E10 `V0.4-DevEncounterSeedData01`

## 1. 返回原因

原 E10 的文件范围、四套 Encounter、计数、隔离、签名、报告确定性和跨系统 Leak 均通过静态复审，但当前 `50/50 PASS` 尚未证明两组内容语义不变量。

### A. E03 -> E06 内容忠实性没有被 Validator 锁定

当前 Provider 从 E03 生成正确内容，但 E10 Validator 只验证：

```text
Pressure 数量
RequirementGroup / Requirement 数量
Required=All / Recommended=Any
Pressure channel weight total=10000
```

它没有拒绝以下合法引用但错误内容：

```text
把 caster pressure channel 从 cast_interrupt 换成 shield
把 caster required capability 从 control_power 换成其他合法 capability
把 MechanicProfile / SkillPattern / BossPhase source 指向另一组合法 Pressure
```

这会让 E03 与 E06 再次形成可漂移的双重事实源，违反原 Assignment 第 3、9、11、17 节。

### B. CounterWindow 关系可解析，但部分关系没有可达 Source

当前至少存在：

```text
dev_enemy_polluted_tile_problem -> dev_window_cleanse_reveal
```

PollutedTile 是 `MAP_RULE_ONLY`，其 PressureSource 来自 MechanicProfile / MapRule；但 Cleanse Window 当前只绑定 SkillPattern / BossPhase，二者没有共同 Source。

以及：

```text
dev_boss_black_furnace_complex_eye -> dev_window_energy_counter_full_array
```

ComplexEye Pressure 的来源与 Energy Window 当前来源没有交集。

另外，部分 `CounterWindowSourceBinding` 与窗口自身 OpenCondition 不兼容。例如 seal Skill 绑定 Guard Window，但 Guard Window 只监听 bronze-general Skill；spirit-core Skill/Phase 绑定 Interrupt Window，但 Interrupt Window 只监听 caster Skill。

这不是运行时执行问题，而是 E06 纯数据关系尚未形成未来适配器可消费的闭合图。

Guard receipt：

```text
GUARD_RETURN_DEVENCOUNTERSEEDDATA01_SEMANTICREACHABILITY01
```

## 2. 修复定位

本修复只补：

```text
E03 -> E06 capability/channel/source fidelity validation
CounterWindow source-condition compatibility
Pressure -> CounterWindow source reachability
对应负例、报告与确定性证明
```

不得改四个 Seed、Encounter/Wave/Slot、E05 技能/阶段内容、Snapshot public shape、玩家字段、basis-point 生成策略或任何跨系统内容。

## 3. CounterWindow 开启信号

E10 六个 Window 必须改为可按 SourceBinding 复用的内部 MechanicSignal；只改 InternalOnly OpenCondition，不改 PlayerSafe key、窗口类型或持续时间：

| Window | CounterWindowType | OpenCondition kind | ReferenceId |
|---|---|---|---|
| `dev_window_interrupt_stagger` | `counter_window.interrupt_stagger` | `MechanicSignal` | `signal.cast.interrupted` |
| `dev_window_cleanse_reveal` | `counter_window.cleanse_reveal` | `MechanicSignal` | `signal.status.cleansed` |
| `dev_window_energy_counter_full_array` | `counter_window.energy_counter_full_array` | `MechanicSignal` | `signal.energy.countered` |
| `dev_window_guard_rebound` | `counter_window.guard_rebound` | `MechanicSignal` | `signal.guard.succeeded` |
| `dev_window_shell_break` | `counter_window.shell_break` | `MechanicSignal` | `signal.shell.broken` |
| `dev_window_clear_core_exposure` | `counter_window.clear_core_exposure` | `MechanicSignal` | `signal.core.exposed` |

每个窗口仍恰好一个 OpenCondition、无 CloseCondition、MaximumDurationMilliseconds 保持原值。信号只在 internal-only 层，E10 不执行或监听信号。

Validator 不得简单硬编码“所有未来窗口只能用 MechanicSignal”。通用兼容规则为：

```text
CounterWindowSourceBinding.SourceKind = SkillPattern
-> Window 至少有同 SourceId 的 SkillPattern* OpenCondition，或至少有一个 MechanicSignal OpenCondition

CounterWindowSourceBinding.SourceKind = BossPhase
-> Window 至少有同 SourceId 的 BossPhase* OpenCondition，或至少有一个 MechanicSignal OpenCondition

CounterWindowSourceBinding.SourceKind = MechanicProfile / MapRule
-> Window 至少有一个 MechanicSignal OpenCondition
```

若不满足，E10 Validator 必须报稳定 issue，例如：

```text
E10_WINDOW_SOURCE_CONDITION_MISMATCH
```

## 4. 补齐 Window Source

在现有 17 条 CounterWindowSourceBinding 基础上只新增三条：

```text
MapRule dev_map_bluestone_damp
  -> dev_window_cleanse_reveal

MapRule dev_map_furnace_ash_fall
  -> dev_window_cleanse_reveal

BossPhase dev_phase_complex_eye_pressure
  -> dev_window_energy_counter_full_array
```

修复后固定：

```text
CounterWindowSourceBinding rows: 20
SkillPattern rows / distinct sources: 9 / 9
BossPhase rows / distinct sources: 9 / 8
MapRule rows / distinct sources: 2 / 2
MechanicProfile rows: 0
```

现有 `dev_phase_complex_eye_pressure -> dev_window_clear_core_exposure` 保留，因此该 BossPhase 合法一对多。

Validator 的覆盖判断必须按 distinct source ID 检查 `9 SkillPattern / 8 BossPhase`，不得把合法的一对多误判成数量错误。

## 5. Pressure -> Window Source 可达性

对每一条 `PressureCounterWindowBinding`，必须存在至少一个相同复合 Source：

```text
PressureSourceBinding(SourceKind, SourceId -> PressureId)
与
CounterWindowSourceBinding(SourceKind, SourceId -> WindowId)
```

即二者的 `(SourceKind, SourceId)` 交集不得为空。

修复后三条关键证明：

```text
polluted-tile -> cleanse
  通过 bluestone-damp / furnace-ash MapRule source 可达

complex-eye -> energy
  通过 complex-eye pressure BossPhase source 可达

所有 13 条 PressureCounterWindowBinding
  均至少有 1 个共同 Source
```

不可达时 E10 Validator 必须报稳定 issue，例如：

```text
E10_PRESSURE_WINDOW_SOURCE_UNREACHABLE
```

不得删除原 Assignment 要求的 Pressure/Window 多对多关系来规避该检查。

## 6. E03 -> E06 Capability Fidelity

对每个 E10 `BuildPressureProfileId`，必须解析到同 ID 的 E03 `MechanicProfile`。

逐 Profile 精确比较：

```text
E03 RequiredCapabilityKeys
= E06 唯一 Required + All group 的 capability key 集合

E03 OptionalCapabilityKeys 为空
-> E06 不得有 Recommended group

E03 OptionalCapabilityKeys 非空
-> E06 唯一 Recommended + Any group 的 capability key 集合必须完全相等
```

比较按 Ordinal 集合语义；不得只比较数量。Basis-point 数值继续允许由当前 devOnly 确定性策略生成，只校验 E06 合法范围，不把具体数值冻结成正式平衡。

不一致时必须报稳定 issue，例如：

```text
E10_REQUIRED_CAPABILITY_FIDELITY
E10_RECOMMENDED_CAPABILITY_FIDELITY
```

## 7. E03 -> E06 PressureChannel Fidelity

以下九个 Profile 的 E06 channel key 集合必须与 E03 `PressureKeys` 完全相等：

```text
dev_enemy_caster_problem
dev_enemy_seal_problem
dev_enemy_poison_burn_problem
dev_enemy_polluted_tile_problem
dev_enemy_spirit_thief_problem
dev_enemy_formation_eye_problem
dev_boss_bronze_formation_general
dev_boss_dirty_dream_mother
dev_boss_spirit_thief_core
```

`dev_boss_black_furnace_complex_eye` 的 E03 pressureKeys 为空，E10 固定使用以下复合通道集合：

```text
pressure.cast_interrupt
pressure.placement_formation
pressure.resource_disruption
pressure.shield
pressure.status_damage
```

所有权重仍必须合计 `10000`。不一致时必须报稳定 issue，例如：

```text
E10_PRESSURE_CHANNEL_FIDELITY
```

## 8. Pressure Source Fidelity

必须补充以下通用校验：

```text
MechanicProfile source
-> SourceId 必须等于目标 BuildPressureProfileId

SkillPattern source
-> 该 Pattern 的 InternalOnly.MechanicProfileIds 必须包含目标 BuildPressureProfileId

BossPhase source
-> 该 Phase 的 InternalOnly.MechanicProfileIds 必须包含目标 BuildPressureProfileId

MapRule source
-> 继续严格匹配原 Assignment 的 4 x 3 matrix
```

不一致时必须报稳定 issue，例如：

```text
E10_PRESSURE_SOURCE_FIDELITY
```

## 9. Verifier 新增证明

必须在真实 E10 Snapshot 基础上增加至少以下检查：

```text
capability fidelity: 10/10 PASS
pressure channel fidelity: 10/10 PASS
pressure source fidelity: all rows PASS
window source-condition compatibility: 20/20 PASS
pressure-window source reachability: 13/13 PASS
```

必须增加至少四个不会先被 E06 基础 Validator 拒绝的 E10 负例：

```text
把一个 required capability 换成另一个合法 E02 capability
-> E10_REQUIRED_CAPABILITY_FIDELITY

把一个 channel 换成另一个合法 E02 pressure channel，权重仍为 10000
-> E10_PRESSURE_CHANNEL_FIDELITY

把一个 source 指向另一组合法且可解析的 Pressure
-> E10_PRESSURE_SOURCE_FIDELITY

移除共同 WindowSource 或构造不兼容 OpenCondition
-> E10_PRESSURE_WINDOW_SOURCE_UNREACHABLE / E10_WINDOW_SOURCE_CONDITION_MISMATCH
```

不得用“未知引用”或 E06 基础校验失败代替这些 E10 语义负例。

原 `50/50` 全部回归；修复后 Verifier 总数必须增加，具体总数由同源 checks 决定，不硬凑固定数字。

## 10. 报告修复

八份原 E10 报告必须全部由同源 Verifier 确定性重建。

主报告必须新增：

```text
Capability fidelity: 10/10
Pressure channel fidelity: 10/10
Pressure source fidelity: PASS
Window source-condition compatibility: 20/20
Pressure-window source reachability: 13/13
```

`DevEncounterSeedPressureWindowRows.csv` 必须新增 `CounterWindowOpenCondition` 逻辑行，至少可审计：

```text
windowId
conditionId
conditionKind
referenceId
open match mode
maximum duration
```

并完整显示 20 条 CounterWindowSourceBinding。不得只显示 Window type 而隐藏条件。

`DevEncounterSeedDataSpec.csv` 必须新增对应正例与负例结果。Leak 报告继续逐项证明 PlayerSafe、Runtime 依赖和正式流程为 0。

连续生成两次必须 `8/8 hash identical`；删除一份报告后的重建继续 PASS。

## 11. 精确允许修改

允许内容修改且仅允许：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataValidation.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs
Docs/V0.4/Reports/DevEncounterSeedDataReport.md
Docs/V0.4/Reports/DevEncounterSeedDataSpec.csv
Docs/V0.4/Reports/DevEncounterSeedInventory.csv
Docs/V0.4/Reports/DevEncounterSeedCompositionRows.csv
Docs/V0.4/Reports/DevEncounterSeedSkillPhaseRows.csv
Docs/V0.4/Reports/DevEncounterSeedPressureWindowRows.csv
Docs/V0.4/Reports/DevEncounterSeedLegacyMapping.csv
Docs/V0.4/Reports/DevEncounterSeedDataLeakCheckReport.md
```

允许新增文件：`0`。

八份报告必须重建；预期只有包含新 checks、条件行或汇总的报告内容变化，其余报告若逻辑内容不变，hash 应保持不变。

## 12. 冻结范围

必须保持：

```text
E01-E09 protected source: 44/44 unchanged
Legacy source: 5/5 unchanged
E10 其余冻结文件: 10/10 unchanged
Scene / Prefab / Config / Battle / Board / Item: 0 / 0 / 0 / 0 / 0 / 0
```

E10 其余冻结文件：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataProvider.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataProvider.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs.meta
```

## 13. 绝对禁止

```text
不新增文件
不修改 Snapshot public shape / schemaId / schemaVersion
不修改四个 Seed / Encounter / Wave / Slot 映射
不修改 E05 Pattern / Sequence / CarrierBinding / Phase / Plan 内容
不改变 10 Pressure / 6 Window / 16 Group / 39 Requirement 计数
不删除原 PressureCounterWindow 多对多关系
不改变 basis-point 确定性生成策略
不改 PlayerSafe key 或新增玩家字段
不修改 Scene / Prefab / Config / Battle / Board / Item
不接 Runtime、BattleContract、Battle Bridge 或 Item Adapter
不修改旧 BuildSandbox / 正式流程 / 1-10 / 2-10
不 commit / tag / push
```

## 14. 验证与回报

必须执行：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
八报告确定性与缺失重建
protected / frozen hash
GUID / whitespace / package-scope check
```

本修复无需场景手测。不得处理开工前 BattleSandbox Scene 的四处尾随空白。

完成后发送：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-DevEncounterSeedData01-GuardFix01

Guard marker:
ENEMY_GUARD_REWORK_DEVENCOUNTERSEEDDATA01_GUARDFIX01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 0
内容修改文件: 仅来自 11 文件允许清单
Seeds / Encounters / Waves / Slots: 4 / 4 / 12 / 12
Pattern / Sequence / CarrierBinding / Phase / Plan: 9 / 9 / 10 / 8 / 4
Pressure / CounterWindow / RequirementGroup / Requirement: 10 / 6 / 16 / 39
CounterWindowSource rows: 20
CounterWindowSource distinct Skill / Phase / Map: 9 / 8 / 2
Capability fidelity: 10/10 PASS / FAIL
Pressure channel fidelity: 10/10 PASS / FAIL
Pressure source fidelity: PASS / FAIL
Window source-condition compatibility: 20/20 PASS / FAIL
Pressure-window source reachability: 13/13 PASS / FAIL
Semantic negative fixtures: PASS / FAIL
Original E10 regression: PASS / FAIL
Eight-report determinism: 8/8 PASS / FAIL
Missing-report regeneration: PASS / FAIL
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: 通过数 / 总数
E01-E09 protected hash: 44/44 unchanged
Legacy source hash: 5/5 unchanged
Other E10 frozen hash: 10/10 unchanged
Runtime dependency leak: 必须为 0
Player answer leak: 必须为 0
Formal flow references: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
HEAD unchanged
未 commit / tag / push
```

GuardFix01 通过前不签发 E10 Guard PASS，不释放任何后置包。
