# V0.4-EnemyDataValidatorAndSnapshot01-GuardFix01 Assignment

Guard rework marker:

```text
ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX01
```

生成日期：2026-07-15  
所属：Enemy / Boss / Encounter System Guard  
原包：E09 `V0.4-EnemyDataValidatorAndSnapshot01`

## 1. 返工原因

E09 当前实现未发现 Scene、Battle、Board、Item 或正式流程越界，但不能签发 Guard PASS，原因如下：

```text
1. E02 CounterWindowType 词汇项被登记为 CounterWindow 实体身份，
   与 E06 CounterWindowProfile 实例共用 IdentityKind。
   这会造成词汇类型与实例 ID 的假冲突，并可能让 E02 词汇项错误满足 E06 实例引用。

2. 签名敏感性只覆盖了 E04 developer tag 与一个 E05 player 字段，
   未逐项证明 E03/E05/E06 developer-only 与 player-safe 变化语义。

3. E07/E08 transient stability 检查把 Root 签名与自身比较，
   没有构造变化后的 transient fixture，因此属于无效证明。

4. Assignment 要求的 carrier-kind mismatch、isolation mismatch、
   component mismatch、全部 RelationKind、输入/输出不可变证明未完整覆盖。

5. LeakCheck 声称列出 Root property shape，实际只检查两个精确属性名，
   未建立 EnemySystemSnapshot 的完整公开属性闭包。
```

本返工只修 E09 自身，不重做 E01-E08，不创建 E10 内容。

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
```

## 3. 允许修改文件

只允许内容修改以下 10 个文件：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs
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
修改其他 E09 文件: 0
修改 E01-E08 文件: 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
```

## 4. CounterWindow 身份语义修正

`EnemySystemIdentityKind.CounterWindow` 只表示 E06 `CounterWindowProfileSnapshot` 实例：

```text
E02 CounterWindowTypeKey = 词汇类型
E06 CounterWindowProfileSnapshot.CounterWindowId = 窗口实例
```

必须做到：

```text
不再把 E02 CounterWindowType vocabulary entry 加入 IdentityIndex
CounterWindow identity 的 OwningComponentId 必须全部为 E06
当前基础 fixture CounterWindow identity 数量必须为 3
E02 六个 CounterWindowTypeKey 继续只留在 E02 Catalog 与 E02 signature
E02 类型词汇不得满足 E01/E06 对 CounterWindow 实例的解析
```

Verifier 必须增加回归正例：新增一个合法、未引用的 E06 CounterWindowProfile，令它的实例 ID 文本恰好等于一个 E02 CounterWindowTypeKey；Root 必须接受，且不得报 identity duplicate。该正例只存在于 Verifier mutation fixture，不得改变基础 `4 Pressure / 3 Window` 基线。

## 5. 签名矩阵补全

Verifier 必须通过原 Provider / Normalizer 构造有效 mutation fixture，逐项证明：

```text
E03 developer-only 变化 -> Full 变化 / PlayerSafe 不变
E05 developer-only 变化 -> Full 变化 / PlayerSafe 不变
E06 developer-only 变化 -> Full 变化 / PlayerSafe 不变

E03 player-safe 变化 -> Full 与 PlayerSafe 都变化
E05 player-safe 变化 -> Full 与 PlayerSafe 都变化
E06 player-safe 变化 -> Full 与 PlayerSafe 都变化

E04 Encounter 编排变化 -> Full 变化 / PlayerSafe 不变
合法 relation target 变化 -> Full 变化
排序变化 -> Full 与 PlayerSafe 均稳定
```

禁止用同一个泛化 `developerChanged` 只代表 E04，也禁止用一个 E05 player 字段代替 E03/E05/E06 三项覆盖。

## 6. E07/E08 transient 证明修正

必须创建至少两套内容确实不同的 transient fixture：

```text
BuildCapabilitySnapshot 内容或 signature 不同
EnemyReadinessEvaluationResult 内容或 player hint severity 不同
六个 E01-E06 static snapshot 完全相同
```

然后明确比较：

```text
Baseline Root Full == transient-changed Root Full
Baseline Root PlayerSafe == transient-changed Root PlayerSafe
E07/E08 仍不出现在 Root property、IdentityIndex、RelationIndex 与静态签名 payload
```

禁止再写 `fixture.Root.CanonicalSignature == fixture.Root.CanonicalSignature` 一类自比较检查。

E07/E08 schema contract mismatch 负例继续保留并必须拒绝。

## 7. Verifier 覆盖补齐

至少新增以下明确命名检查：

```text
coverage.relationKinds = 17/17
negative.componentMismatch
negative.carrierKindMismatch
negative.isolationMismatch
immutability.input
immutability.rootCollections
immutability.playerCollections
identity.counterWindowOwner = E06 only
identity.counterWindowTypeCollision = accepted
```

要求：

```text
Section 8 的 17 个 RelationKind 必须全部被基础 Root 或 synthetic positive row 覆盖
carrier-kind mismatch 必须由原 Provider 或 E09 明确拒绝
isolation mismatch 必须产生 E09_RELATION_ISOLATION_MISMATCH 或 E09_ISOLATION_INVALID
component mismatch 必须明确验证 E03.DomainSnapshot 与 E01 signature 不一致
Root SchemaManifest / IdentityIndex / RelationIndex 必须不可写
PlayerSafe 所有集合必须不可写
尝试修改后 Root 两个 signature 与集合内容必须保持不变
```

synthetic relation row 只能用于证明 Validator 语义，不得伪装成基础 Root 的实际关系数量。

## 8. Root 属性闭包

`EnemySystemDataValidation.cs` 必须对 `EnemySystemSnapshot` 建立完整公开属性闭包，且 LeakCheck 必须列出 expected 与 actual：

```text
SchemaId
SchemaVersion
EnemyDomainSnapshot
EnemyMechanicVocabularySnapshot
EnemyValidationContentSnapshot
EncounterCompositionCatalogSnapshot
EnemySkillBossPhaseCatalogSnapshot
CounterWindowAndPressureCatalogSnapshot
SchemaManifest
IdentityIndex
RelationIndex
PlayerSafe
CanonicalSignature
PlayerSafeCanonicalSignature
DevOnly
IsEnabled
EntersFormalFlow
```

不得只检查名为 `BuildCapabilitySnapshot` / `EnemyReadinessEvaluationResult` 的两个属性。闭包外任意公开属性都必须触发 E09 validation issue。

PlayerSafe 现有属性与类型闭包保持不变。

## 9. 报告重建

七份报告必须由同源 Verifier 每次确定性覆盖生成，禁止 `WriteIfMissing`：

```text
连续运行两次: 7/7 hash identical
IdentityInventory CounterWindow: 基础 fixture 3，全部 owner E06
RelationMatrix 基础 Root 实际数量必须真实，不得把 synthetic coverage 混入
主报告必须新增签名矩阵、17/17 RelationKind、immutability、Root property closure 结果
LeakCheck 必须列出完整 Root expected/actual property shape
```

GuardFix 后 Verifier 总检查数必须高于 173，且全部 PASS；不得保留失效自比较检查凑数。

## 10. 回归与保护

必须保持：

```text
E01-E08 protected hash: 38/38 unchanged
Legacy source hash: 3/3 unchanged
E02: 63 total / 16 BuildCapability / 6 CounterWindowType
E03: 11/7/18 carriers; 10/6/16 profiles; 10 map rules; 17/30 bindings; 2 exceptions
E04: 2 Encounter / 4 Wave / 6 Slot
E05: 4 Pattern / 3 Sequence / 3 Binding / 3 Phase / 2 Plan
E06: 4 Pressure / 3 Window / 6 PressureSource / 4 WindowSource / 5 PressureWindow / 11 Requirement
E07: 16/16/4/6/3/5
E08: 15 scenarios / 212 checks / 5 reports deterministic
Manifest: 8/8 / Static 6 / Transient 2 / E07-E08 embedded 0/0
Player leak: 0
Formal flow reference: 0
```

允许清单之外的其余 E09 文件必须全部 hash unchanged。

## 11. 自动验证

必须尝试：

```text
同源离线 Verifier
Unity active Editor compile 状态确认
Unity batch compile
Unity batch Verifier
七报告连续两次确定性重建
package-scope whitespace check
GUID conflict check
精确变更文件清单
git diff --check
```

若同工程仍由用户 Unity Editor 打开，不得关闭 Editor 或结束进程。Batch 可按原 Assignment 记录 `BLOCKED`，但必须提供 active Editor compile 无 E09 编译错误与同源离线 Verifier PASS。

当前全局 `git diff --check` 若仍仅为开工前 BattleSandbox Scene 四处尾随空白，记录 `PREEXISTING_UNRELATED_DIFF`；不得触碰该 Scene。

本包无需场景手测。

## 12. 绝对禁止

```text
不修改 E01-E08
不新增文件
不修改任何 .meta
不创建 E10 dev encounter 内容
不接 Scene / Battle / BattleContract / Board / Item
不读取真实 Build、棋盘或战斗状态
不修改正式 1-10 / 2-10
不接正式章节入口
不发奖励、不写 SaveData、不推进章节
不覆盖或回退无关 dirty 改动
不 commit / tag / push
```

## 13. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyDataValidatorAndSnapshot01-GuardFix01

Guard marker:
ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 0
内容修改文件: 必须仅来自 10 文件允许清单
CounterWindow identity: 3 / E06 only
CounterWindowType collision fixture: PASS / FAIL
Signature matrix: E03/E05/E06 developer + player / E04 / relation target PASS / FAIL
Transient changed fixture: Full stable / PlayerSafe stable PASS / FAIL
RelationKind coverage: 17/17
Component mismatch negative: PASS / FAIL
Carrier kind mismatch negative: PASS / FAIL
Isolation mismatch negative: PASS / FAIL
Immutability: input / root / player PASS / FAIL
Root property closure: PASS / FAIL
Seven-report determinism: 7/7 PASS / FAIL
Verifier: PASS 数 / 总数
E01-E08 protected hash: 38/38 unchanged
Legacy source hash: 3/3 unchanged
其余 E09 frozen hash: 全部 unchanged
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Unity active Editor compile: PASS / FAIL / NOT_AVAILABLE
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: PASS 数 / 总数
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
commit / tag / push: 0 / 0 / 0
```

完成后停止。GuardFix01 通过前不得签发 E09 Guard PASS，不得启动 E10。
