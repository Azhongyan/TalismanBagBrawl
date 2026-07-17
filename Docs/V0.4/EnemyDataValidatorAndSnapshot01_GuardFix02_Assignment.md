# V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02 Assignment

Guard rework marker:

```text
ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02
```

生成日期：2026-07-16  
所属：Enemy / Boss / Encounter System Guard  
原包：E09 `V0.4-EnemyDataValidatorAndSnapshot01`

## 1. 返工原因

GuardFix01 已通过以下静态复审项：

```text
CounterWindow identity = 3 / E06 only
CounterWindowType collision fixture = PASS
E03/E05/E06 developer 与 player signature matrix = PASS
E07/E08 changed transient fixture isolation = PASS
RelationKind = 17/17
component / carrier-kind / isolation negative = PASS
input / root / player immutability = PASS
Root property closure = PASS
```

但 E09 Full Root signature 仍违反原 Assignment：

```text
EnemySystemSnapshot.ManifestRow 对 E04 单独清空 ComponentCanonicalSignature。
E04 Encounter quantity 从 2 改为 3 后，E04 Catalog 内容已变化，
但 E09 Full CanonicalSignature 保持不变。
```

原 Assignment 要求 Full signature 覆盖六个静态组件的 Full signature；GuardFix01 也明确要求：

```text
E04 Encounter 编排变化 -> Full 变化 / PlayerSafe 不变
```

当前 Verifier 却把 `E04Composition` 的预期写成 `Full stable / PlayerSafe stable`，因此 `215/215 PASS` 不能作为 Guard PASS。

## 2. 启动必读

开发前完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/EnemyDataValidatorAndSnapshot01_Assignment.md
Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix01_Assignment.md
Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix02_Assignment.md
```

## 3. 允许修改文件

只允许内容修改以下 9 个文件：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotReport.md
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSpec.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotFieldMatrix.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSchemaManifest.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotIdentityInventory.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotRelationMatrix.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotLeakCheckReport.md
```

规则：

```text
新增文件: 0
修改 .meta: 0
修改 EnemySystemDataValidation.cs: 0
修改其他 E09 文件: 0
修改 E01-E08 文件: 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
```

## 4. 唯一运行时修复

`EnemySystemSnapshot.ManifestRow` 必须无条件包含每个 Manifest entry 的 `ComponentCanonicalSignature`：

```text
E01 full signature included
E02 full signature included
E03 full signature included
E04 full signature included
E05 full signature included
E06 full signature included
E07/E08 component signature 继续为空，因为它们是 transient contract
```

禁止：

```text
不得对 E04 或其他静态组件做 special case
不得因为 E04 没有 player-safe projection 就排除 E04 full signature
不得把 Encounter quantity / wave / slot / developer tag 加进 PlayerSafe
不得改 E04 Catalog 或 E04 CanonicalSignature 实现
```

`PlayerSafeCanonicalSignature` 对 E04 编排变化继续保持稳定，这是因为 E04 当前没有 player-safe projection；这不影响 Full Root 必须感知 E04 Full signature。

## 5. Verifier 修正

保留当前 `E04Composition` mutation：

```text
slot_poison quantity: 2 -> 3
relation target 不变
其他 E01-E03 / E05-E08 内容不变
```

必须改为验证：

```text
E04 Catalog CanonicalSignature: changes
E09 Root Full CanonicalSignature: changes
E09 Root PlayerSafeCanonicalSignature: stable
RelationIndex: stable
IdentityIndex: stable
```

报告中的 Signature mutation matrix 必须写：

```text
E04 composition only | changes | stable | PASS
```

不得通过修改 expected 为 stable 来制造 PASS。

增加或保留明确检查，证明基础 Root 的 Full payload 覆盖 E01-E06 六个静态 Manifest `ComponentCanonicalSignature`。至少要求：

```text
manifest.staticFullSignatures = 6/6 non-empty
signature.e04.composition.catalog = different
signature.e04.composition.full = different
signature.e04.composition.player = stable
signature.e04.composition.identities = stable
signature.e04.composition.relations = stable
```

## 6. 冻结与回归

GuardFix01 已通过的全部结果必须继续保持：

```text
CounterWindow identity: 3 / E06 only
CounterWindowType collision fixture: PASS
E03/E05/E06 developer + player matrix: PASS
E04 valid relation target: Full changes / PlayerSafe stable
Transient changed fixture: Full stable / PlayerSafe stable
RelationKind coverage: 17/17
Component mismatch negative: PASS
Carrier kind mismatch negative: PASS
Isolation mismatch negative: PASS
Immutability input / root / player: PASS
Root property closure: PASS
Player leak: 0
Formal flow reference: 0
Manifest: 8/8 / Static 6 / Transient 2 / E07-E08 embedded 0/0
```

保护要求：

```text
E01-E08 protected hash: 38/38 unchanged
Legacy source hash: 3/3 unchanged
允许清单之外 E09 frozen hash: 11/11 unchanged
```

Verifier 的 frozen E09 基线必须把当前 GuardFix01 通过后的 `EnemySystemDataValidation.cs` 纳入冻结，总数从 10 更新为 11；不得借 Fix02 再改 Validator、PlayerSafe、Input、Primitives 或任何 `.meta`。

## 7. 报告与验证

七份报告继续由同源 Verifier 每次确定性覆盖生成：

```text
连续运行两次: 7/7 hash identical
Verifier 总检查数必须高于 215
全部检查 PASS
E04 composition row 必须为 Full changes / PlayerSafe stable
基础 Full signature 因恢复 E04 component signature 后必须更新
基础 PlayerSafe signature 必须保持 GuardFix01 值不变
```

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
七报告连续两次确定性重建
package-scope whitespace check
GUID conflict check
精确变更文件清单
git diff --check
```

若 Unity Editor 占用同工程，按原规则记录 `BLOCKED`，不得关闭 Editor 或结束进程。

当前全局 `git diff --check` 若仍仅为开工前 BattleSandbox Scene 四处尾随空白，记录 `PREEXISTING_UNRELATED_DIFF`；不得触碰该 Scene。

本包无需场景手测。

## 8. 绝对禁止

```text
不修改 E01-E08
不新增文件
不修改任何 .meta
不修改 EnemySystemDataValidation.cs
不创建 E10 内容
不接 Scene / Battle / BattleContract / Board / Item
不读取真实 Build、棋盘或战斗状态
不修改正式 1-10 / 2-10
不接正式章节入口
不发奖励、不写 SaveData、不推进章节
不覆盖或回退无关 dirty 改动
不 commit / tag / push
```

## 9. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02

Guard marker:
ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 0
内容修改文件: 必须仅来自 9 文件允许清单
Static Manifest full signatures: 6/6 non-empty
E04 Catalog composition sensitivity: PASS / FAIL
E04 Root Full sensitivity: PASS / FAIL
E04 Root PlayerSafe stability: PASS / FAIL
E04 identity/relation stability: PASS / FAIL
GuardFix01 regression: PASS / FAIL
Seven-report determinism: 7/7 PASS / FAIL
Verifier: PASS 数 / 总数，必须高于 215
E01-E08 protected hash: 38/38 unchanged
Legacy source hash: 3/3 unchanged
其余 E09 frozen hash: 11/11 unchanged
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: PASS 数 / 总数
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
commit / tag / push: 0 / 0 / 0
```

完成后停止。GuardFix02 通过前不得签发 E09 Guard PASS，不得启动 E10。
