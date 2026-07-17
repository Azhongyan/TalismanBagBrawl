# V0.4-EnemyValidationContentNormalize01-GuardFix01 Assignment

Guard rework assignment：

```text
ENEMY_GUARD_REWORK_ENEMYVALIDATIONCONTENTNORMALIZE01_GUARDFIX01
```

生成日期：2026-07-14  
所属：Enemy / Boss / Encounter System Guard  
原包：E03 `V0.4-EnemyValidationContentNormalize01`

## 1. 返工原因

`EnemyValidationContentSnapshot.BuildPayload(false)` 当前对 MechanicProfile 与 MapRule 会写入 `DeveloperOnly` 投影，但对 Enemy 与 Boss 只写入 `PlayerSafe` 投影。

当前问题代码：

```text
EnemyValidationContentSnapshot.cs:289
EnemyValidationContentSnapshot.cs:290
```

后果：

```text
Enemy / Boss 的 LegacySource、LegacyFacts、MappingTrace 等 developer-only 内容变化
→ CanonicalSignature 可能保持不变
→ 完整归一化快照签名不能完整代表快照内容
```

现有 Verifier 只检查输入顺序不影响签名，没有检查 developer-only 内容变化时完整签名必须变化。

## 2. 必须修复

完整 `CanonicalSignature` 必须包含：

```text
Enemy PlayerSafe + DeveloperOnly
Boss PlayerSafe + DeveloperOnly
MechanicProfile PlayerSafe + DeveloperOnly
MapRule PlayerSafe + DeveloperOnly
Bindings
Exceptions
DomainSnapshot signature
```

`PlayerSafeCanonicalSignature` 必须继续只包含 player-safe 内容，不得因 developer-only 内容变化而变化。

## 3. 必须新增验证

至少新增：

```text
只改变一个 Enemy developer-only 字段
→ CanonicalSignature 必须变化
→ PlayerSafeCanonicalSignature 必须不变

只改变一个 Boss developer-only 字段
→ CanonicalSignature 必须变化
→ PlayerSafeCanonicalSignature 必须不变

只改变 player-safe 字段
→ 两个签名都必须变化

输入顺序变化
→ 两个签名仍必须保持稳定
```

报告必须明确列出上述检查，不得只提高通过总数。

## 4. 允许修改范围

只允许修改：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/EnemyValidationContentSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizeVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs
Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md
Docs/V0.4/Reports/EnemyValidationContentNormalizeSpec.csv
Docs/V0.4/Reports/EnemyValidationCarrierInventory.csv
Docs/V0.4/Reports/EnemyValidationMechanicInventory.csv
Docs/V0.4/Reports/EnemyCarrierMechanicBinding.csv
Docs/V0.4/Reports/EnemyMapRuleMechanicBinding.csv
Docs/V0.4/Reports/EnemyValidationNormalizationExceptions.csv
Docs/V0.4/Reports/EnemyValidationContentNormalizeLeakCheckReport.md
```

`EnemyValidationContentNormalizer.cs` 只允许为构造 verifier 的 developer-only 变化样例做最小测试接缝；不得改变默认归一化内容和 18/16/10、17/30、2 exceptions 结果。

## 5. 禁止

```text
不改 EnemyValidationContentNormalizerCore.cs
不改 E01 / E02
不改三个 Legacy source 文件
不改 Scene / Prefab / Config / Battle / Board / Item
不改任何战斗、棋盘或输入逻辑
不新增机制、原型、地图规则或绑定
不改变玩家字段与开发者字段边界
不接 Runtime / BattleContract / Item
不 commit / tag / push
```

## 6. 回归要求

必须保持：

```text
Carriers: 11 / 7 / 18
Mechanic profiles: 10 / 6 / 16
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
Unresolved / unexplained orphan: 0
Runtime BuildSandbox references: 0
Editor-only legacy reader: 1
E01 + E02 protected hash: 10/10 unchanged
Legacy source hash: 3/3 unchanged
Leak: 0
```

必须重新运行 Unity batch compile、Unity batch Verifier、同源离线 Verifier、`git diff --check` 与尾随空白检查。

## 7. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyValidationContentNormalize01-GuardFix01

Result:
DEV_COMPLETE / QA_RESULT

必须报告：
完整签名 developer-content sensitivity：PASS / FAIL
Player-safe 签名 developer-content isolation：PASS / FAIL
修复后 Verifier：通过数 / 总数
原 E03 全部计数回归
protected hash
Leak
Unity 状态
精确修改文件
未 commit / tag / push
```

GuardFix01 通过前，E03 不签发 Guard PASS，E04 不启动。
