# V0.4-LayoutResilienceStructuralReadinessConsumer01 Assignment

## 1. Guard 结论

```text
Package: V0.4-LayoutResilienceStructuralReadinessConsumer01
Queue Id: N01C-P4
Status: JOINT_GUARD_PASS / READY_FOR_DEV
Risk: YELLOW / ISOLATED STRUCTURAL-PREDICATE DIAGNOSTIC ONLY

GUARD_PASS_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01
```

本包建立独立的 StructuralPredicate 通道消费者。它只消费 P3 公开只读 Result；仅对其中合法、非 null 的 N01C `LayoutResilienceEvaluationInput` 调用权威 N01C Evaluator，并原样保留 N01C `KnownTrue / KnownFalse / Unknown / NotApplicable` 结果。

这些结果只是 StructuralPredicate 通道诊断，不是 Enemy 总 readiness、Build Ready/Failed、BP、难度或正式战斗结论。

## 2. 前置与启动必读

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/*
Docs/CURRENT/*
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ItemAlgorithmGuard_CurrentRules.md
Docs/V0.4/EnemyRequirementChannelApplicabilitySchemaContract01_Assignment.md
Docs/V0.4/LayoutResilienceStructuralPredicateContract01_Assignment.md
Docs/V0.4/LayoutResilienceItemFactProjectionAdapter01_Assignment.md
Docs/V0.4/AuthoredLayoutPressureSourceAdapter01_Assignment.md
Docs/V0.4/LayoutResilienceEvaluationInputAssembler01_Assignment.md
```

前置状态固定：

```text
N01B    GUARD_ACCEPTED / QA_PASS
N01C    GUARD_ACCEPTED / QA_PASS
N01C-P1 GUARD_ACCEPTED / QA_PASS
N01C-P2 GUARD_ACCEPTED / QA_PASS
N01C-P3 GUARD_ACCEPTED / QA_PASS
```

任一前置文件、公开类型或 Protected baseline 不匹配时立即停止并返回 Guard；不得刷新基线、复制前置实现或增加兼容 fallback。

## 3. 通道语义与所有权

Enemy Guard 确认，本包只形成独立 `StructuralPredicate` 通道诊断：

```text
KnownTrue
  只表示本次显式结构谓词的全部 required clauses 满足。
  不表示最终 Build Ready、Encounter Ready、胜利或数值足够。

KnownFalse
  只表示本次显式结构谓词至少一个 required clause 违反。
  不表示最终 Build Failed、战斗失败或所有通道不满足。

Unknown
  表示 applicability 或必要结构输入仍不完整。
  不得解释为 false、zero、失败、未满足或 NotApplicable。

NotApplicable
  表示 N01B 的显式稳定事实已声明该结构要求在当前上下文不适用。
  不表示 Ready、通过、KnownZero 或免除其他通道。
```

Clause rows 只是 N01C 结构谓词的 Satisfied/Violated 证据；不得换算 BP、score、ratio、强弱排名或 readiness gap。

本包拥有：

```text
Consumer 自身 Schema / Status / Issue / Canonical Signature
P3 Result 到独立 StructuralPredicate 诊断结果的消费真值表
权威 N01C Evaluator 的单次调用边界
权威 N01C ValidateResult 的结果门禁
```

本包不拥有：

```text
P3 EvaluationInput 的装配或修正
P1/P2/N01B、Item、IF01、Enemy requirement/readiness 数据
N01C Evaluator、Validator、predicate 或 clause 算法
真实 E10 requirement 迁移或跨通道汇总
Battle/Board/Map/Scene/UI/Save/Reward/Chapter/RunFlow
```

## 4. Consumer Schema、状态与类型

固定：

```text
schemaId = LayoutResilienceStructuralReadinessConsumer.v1
schemaVersion = 1
```

`LayoutResilienceStructuralReadinessConsumerStatus` 恰好三个命名有效值：

```csharp
Complete = 1
Unknown = 2
Invalid = 3
```

数值 `0` 与任何未定义整数必须拒绝。不得把 N01C PredicateState 并入 Consumer Status，也不得增加 Ready、Failed、Met、Unmet、KnownZero、NotApplicable 或 NotInChannel Status。

必须建立等价类型：

```text
LayoutResilienceStructuralReadinessConsumerSchema
LayoutResilienceStructuralReadinessConsumerStatus
LayoutResilienceStructuralReadinessConsumerIssue
LayoutResilienceStructuralReadinessConsumerInput
LayoutResilienceStructuralReadinessConsumerResult
ILayoutResilienceStructuralReadinessConsumer
DefaultLayoutResilienceStructuralReadinessConsumer
LayoutResilienceStructuralReadinessConsumerValidationCodes
DefaultLayoutResilienceStructuralReadinessConsumerValidator
```

Issue 只允许：

```text
Code
Status                    // Unknown 或 Invalid；不得为 Complete
Path
Message
```

## 5. 精确输入与输出字段

`LayoutResilienceStructuralReadinessConsumerInput` 顶层只允许：

```text
SchemaId
SchemaVersion
AssemblerResult           // P3 LayoutResilienceEvaluationInputAssemblerResult，可 null
```

不得增加 P1、P2、N01B、Item、IF01、MapRule、Encounter、requirement、BP、threshold、readiness hint 或 fallback input。

`LayoutResilienceStructuralReadinessConsumerResult` 只允许：

```text
SchemaId
SchemaVersion
Status
PredicateResult           // N01C LayoutResiliencePredicateResultSnapshot，可 null
Issues
CanonicalSignature
```

固定输出组合：

| Consumer Status | PredicateResult | 合法 PredicateState |
|---|---|---|
| Complete | 非 null | KnownTrue |
| Complete | 非 null | KnownFalse |
| Complete | 非 null | NotApplicable |
| Unknown | 非 null | Unknown |
| Unknown | null | 无；不得伪造 N01C result |
| Invalid | null | 无；不得保留畸形或异常 result |

Complete 不等于 total readiness complete。Result 不得包含 `IsReady`、`IsFailed`、ReadinessBand、score、gap、difficulty、reward 或 cross-channel summary。

## 6. P3 来源资格门禁

Consumer 只能读取 P3 公开 Result。不得读取或重建 P3 的上游输入。

固定顺序：

1. Consumer input 为 null、SchemaId/Version mismatch 或未定义 Consumer enum，结果 Invalid，不调用 Evaluator。
2. `AssemblerResult == null` 表示 P3 来源缺失，结果 `Unknown + null`，不调用 Evaluator，不合成 N01C result。
3. 对非 null P3 Result 先调用 `DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance.ValidateResult(...)` 恰好一次。
4. P3 Result Validator 返回 issue、P3 Schema/Status/payload/issues/signature 形状畸形或 undefined enum，Consumer 结果 Invalid，不调用 Evaluator。
5. 合法 P3 `Status=Invalid` 固定映射为 Consumer `Invalid + null`，不调用 Evaluator；P3 Invalid issues 必须作为带前缀、稳定排序的 Consumer 诊断保留。
6. 合法 P3 `Status=Unknown` 且 `EvaluationInput=null` 固定映射为 Consumer `Unknown + null`，不调用 Evaluator。
7. 只有合法 P3 `Status=Complete/Unknown` 且 `EvaluationInput!=null` 才可进入第 8 节求值路径。

Consumer 不得通过 P3 Canonical Signature 反查来源，不得调用 P3 Assembler，不得修改、补齐或重新构造 `LayoutResilienceEvaluationInput`。传给 Evaluator 的对象必须是 P3 公开 Result 中的同一只读 EvaluationInput。

P3 `ValidateResult` 只是首道公开结果门禁，不覆盖本包全部 status/applicability/completeness disposition；Consumer 必须继续执行第 7 节专属组合检查，不能仅凭 P3 Validator PASS 就调用 Evaluator。

## 7. P3 → Consumer 真值表

| P3 状态 | P3 EvaluationInput | Input disposition | Evaluator | Expected Consumer output |
|---|---|---|---|---|
| Complete | Applicable + Complete/Complete | 合法 | 恰好 1 次 | Complete + KnownTrue 或 KnownFalse |
| Complete | explicit NotApplicable + NotRequired/NotRequired + null/null | 合法 | 恰好 1 次 | Complete + NotApplicable |
| Complete | 其他 applicability/completeness 组合 | 畸形 | 0 | Invalid + null |
| Unknown | explicit Unknown + 合法 Complete/Incomplete sides | 合法 | 恰好 1 次 | Unknown + N01C Unknown result |
| Unknown | Applicable + 至少一侧 Incomplete | 合法 | 恰好 1 次 | Unknown + N01C Unknown result |
| Unknown | null | 合法缺事实 | 0 | Unknown + null result |
| Unknown | 会产生 Known/NotApplicable 的 input 组合 | 畸形 | 0 | Invalid + null |
| Invalid | 必须 null | 合法 P3 Invalid | 0 | Invalid + null |
| Invalid | 非 null | 畸形 | 0 | Invalid + null |
| undefined/malformed | 任意 | 畸形 | 0 | Invalid + null |

补充不变量：

```text
P3 Complete + Applicable + Complete/Complete 只能接受 KnownTrue/KnownFalse。
P3 Complete + explicit NotApplicable 只能接受 NotApplicable。
P3 Unknown + non-null input 只能接受 Unknown。
任何 disposition mismatch 都是 Invalid，不得降级为 Unknown 或 KnownFalse。
P3 Unknown/Invalid issue 不得被解释为 N01C clause result。
```

## 8. 唯一合法 Evaluator 与结果验证路径

Production 默认依赖必须是：

```text
Evaluator = DefaultLayoutResilienceStructuralPredicateEvaluator.Instance
ResultValidator = DefaultLayoutResilienceStructuralPredicateValidator.Instance
```

对第 7 节允许求值的每个 P3 EvaluationInput，唯一流程固定：

1. 调用 `ILayoutResilienceStructuralPredicateEvaluator.Evaluate(input)` 恰好一次；不得 retry、fallback 或调用第二个算法。
2. 若 Evaluate 抛出可捕获异常，立即返回 `Invalid + null`；不得调用第二次，不得降级为 Unknown/KnownFalse，不得保存 partial result。
3. 若 Evaluate 正常返回，包括返回 null，调用 `ILayoutResilienceStructuralPredicateValidator.ValidateResult(input, result)` 恰好一次。
4. ValidateResult 抛出异常、返回任一 issue、result 为 null、Schema/Version/EvaluationId/Applicability/PredicateState/clause/signature 畸形，均返回 `Invalid + null`。
5. ValidateResult 通过后，再按第 7 节核对 P3 Status 与 PredicateState disposition；不匹配则 Invalid。
6. 通过全部门禁后，防御性复制完整 N01C Result 和全部 clause rows，映射 Consumer Status，并生成 Consumer Canonical Signature。

N01C Evaluator 内部已有 input/result 验证不替代 Consumer 的显式 ValidateResult gate；Consumer 也不得复制或修改 N01C 的空间、可达性或 clause 算法。

公开 Runtime Consumer API 不得接受任意 evaluator/validator。固定限制为：

```text
公开 Instance、公开无参构造与公开 Consume 路径必须硬绑定上述两个权威 N01C 默认实现。
公开构造函数、公开方法和公开属性中的 evaluator/validator 参数或 setter 数量必须为 0。
为覆盖异常与畸形返回，只允许非公开 test-only seam（internal/private constructor 或同核心 helper）注入 synthetic evaluator 行为；ResultValidator 在测试中也必须保持权威默认实例，不得替换。
Editor verifier 如需访问该 seam，必须仍委托同一 Consumer core；不得复制生产流程。
test-only seam 只能模拟 throw、null 或 malformed result，不得实现第二套成功谓词算法。
test-only seam 不得成为 public runtime bypass、第二个生产入口或 player-facing API。
仓库内除本包 Editor verifier 外的非默认 evaluator 注入调用点必须为 0；非默认 validator 注入调用点在全仓必须为 0。
不得建立 service locator、全局 registry、静态可变 override 或运行时 provider 选择。
```

## 9. 结果保存与禁止解释

Consumer 必须原样保留：

```text
N01C SchemaId / SchemaVersion
EvaluationId
ApplicabilityState
PredicateState
全部 ClauseRows 的 ClauseKind / ClauseState
N01C CanonicalSignature
```

不得：

```text
添加、删除、合并或重排为不同语义的 clause
把 Satisfied/Violated 计数、求和、加权或换算 BP/score
把 KnownTrue 命名为 Ready/Pass
把 KnownFalse 命名为 Failed/Unready
把 NotApplicable 命名为 Ready/Zero
把 Unknown 命名为 false/zero
跨 ContinuousBP、RuntimeEventSignal 或其他结构谓词汇总
```

Unknown 与 NotApplicable 的 N01C Result 都必须保持空 ClauseRows；Consumer 不得伪造诊断 clause。

## 10. Canonical Signature 与不可变性

格式固定：

```text
sha256: + 64 lowercase hex
```

Canonical payload 必须覆盖：

```text
Consumer SchemaId / SchemaVersion
Consumer input presence
P3 result presence、Schema/Version、Status、CanonicalSignature、Issues
P3 EvaluationInput 的全部七个顶层字段及完整 Build/Pressure 中立 payload
Consumer Status
PredicateResult presence 与全部 N01C Result 字段
全部 ClauseRows
N01C CanonicalSignature
Consumer Issues
```

规范固定：

```text
所有身份与文本：Ordinal exact，不 Trim、不改大小写
整数：InvariantCulture decimal
bool/presence：lowercase true/false
placement rows：PlacementId Ordinal
cell：Y ascending，再 X ascending
pressure kind / clause：numeric ascending
无向 edge：规范化端点后按 A.Y/A.X/B.Y/B.X
clause rows：ClauseKind numeric ascending
issue rows：Path Ordinal，再 Code Ordinal，再 Message Ordinal
null 与显式空集合必须产生不同 payload
```

异常诊断必须使用固定 invariant code/message；不得把本地化 exception message、stack trace、时间、线程、随机数或 runtime type identity 写入 Canonical payload。

必须证明：

```text
相同输入重复消费、Culture 改变、P3 内集合反序 -> 相同结果与签名
改变 P3 CanonicalSignature、EvaluationInput、PredicateState、任一 clause 或 issue -> 签名变化
调用后修改原始 synthetic source 集合 -> Consumer Result、ClauseRows 与签名不变
不读取文件系统、时间、随机数、静态可变状态或 Unity runtime
```

## 11. Synthetic fixtures

固定 20 个场景，全部使用 dev/synthetic ID：

```text
1. P3 Complete + Applicable C/C + 单 clause satisfied -> Complete / KnownTrue
2. P3 Complete + Applicable C/C + 单 clause violated -> Complete / KnownFalse
3. P3 Complete + Applicable C/C + 多 clause 全 satisfied -> KnownTrue，完整保留 rows
4. P3 Complete + Applicable C/C + 多 clause mixed -> KnownFalse，完整保留 rows/states
5. P3 Complete + explicit NotApplicable -> Complete / NotApplicable / 0 clauses
6. P3 Unknown + explicit applicability Unknown + C/C -> Unknown / N01C Unknown result
7. P3 Unknown + Applicable + Build Incomplete -> Unknown / N01C Unknown result
8. P3 Unknown + Applicable + Pressure Incomplete -> Unknown / N01C Unknown result
9. P3 Unknown + null EvaluationInput -> Unknown / null / Evaluate 0
10. P3 source null -> Unknown / null / Evaluate 0
11. P3 Invalid -> Invalid / null / Evaluate 0
12. malformed P3 Complete/null、Invalid/non-null 或非法 signature shape -> Invalid / Evaluate 0
13. undefined Consumer/P3/N01C enum -> Invalid；不得构造业务结果
14. null Consumer input 或 Consumer SchemaId/Version mismatch -> Invalid / Evaluate 0
15. 合法外形但 N01C input 畸形 -> 权威 Evaluator rejection/exception -> Invalid
16. injected verifier evaluator 抛出异常 -> Invalid / no retry
17. injected verifier evaluator 返回 null -> ValidateResult 1 / Invalid
18. injected verifier evaluator 返回 malformed schema/id/applicability/state/clause/signature -> ValidateResult rejected / Invalid
19. 输入集合反序、Culture 改变、重复消费 -> 相同结果与 Canonical Signature
20. 防御性复制、不可变 ClauseRows、Canonical 字段敏感性 -> PASS
```

Fixture 还必须证明：

```text
默认成功路径使用权威 N01C Evaluator；每个非 null 合法 input Evaluate=1
正常返回后 ValidateResult=1；异常抛出后 ValidateResult=0
P3 Invalid/malformed/Unknown-null/P3-null Evaluate=0
KnownTrue/KnownFalse/NotApplicable/Unknown 原样保留
任何结果畸形或 disposition mismatch = Invalid，不是 Unknown/KnownFalse
真实 requirement mapping = 0
真实 readiness aggregation = 0
C02 blocked rows = 32/32；Actual Unknown reduction = 0
```

Fixture setup 只能在 Editor verifier 中构造 synthetic P3 Result；不得调用 P3 Assembler，不得读取 P1/P2/N01B、Item/IF01、Enemy、asset、Scene、MapRule、Encounter 或 requirement。

## 12. 精确文件白名单

任务窗口只允许新增以下 14 个文件：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumerPrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumerPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumer.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumer.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumerValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer/LayoutResilienceStructuralReadinessConsumerValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceStructuralReadinessConsumerReport.md
Docs/V0.4/Reports/LayoutResilienceStructuralReadinessConsumerSpec.csv
Docs/V0.4/Reports/LayoutResilienceStructuralReadinessConsumerTruthTable.csv
Docs/V0.4/Reports/LayoutResilienceStructuralReadinessConsumerFixtureCases.csv
Docs/V0.4/Reports/LayoutResilienceStructuralReadinessConsumerLeakCheckReport.md
```

预期新增：`14`。允许修改已有文件：`0`。

本 Assignment 由 Guard 写入，不计入任务窗口 14 文件。任务窗口不得修改 Assignment、Queue、CurrentRules、AGENTS、LOCKED 或任何前置文件。

Runtime 依赖方向：

```text
StructuralReadinessConsumer -> System.*
StructuralReadinessConsumer -> P3 public Result / Result Validator
StructuralReadinessConsumer -> N01C Evaluator / Result Validator / Result DTO
StructuralReadinessConsumer -X-> P3 Assembler
StructuralReadinessConsumer -X-> P1/P2/N01B/Item/IF01/Enemy source
StructuralReadinessConsumer -X-> requirement/readiness aggregation
StructuralReadinessConsumer -X-> Board/Map/Battle/Scene/Prefab/UI/UnityEngine
```

## 13. Protected baseline

冻结且不允许刷新：

```text
N01C-P3 exact 14 = c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c
N01C-P3 Canonical Signature = sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a
N01C-P2 exact 14 = 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050
N01C-P2 Canonical Signature = sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc
N01C-P1 exact 14 = 7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48
N01C-P1 Canonical Signature = sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd
N01C exact 16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical Signature = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
N01B exact 14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical Signature = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
IF01 exact 11 = 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8
Enemy existing 89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 = fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 = 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A = a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
N01 exact 8 = 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
C02A-D1 exact 7 = c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0
N01A exact 8 = 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Exact scope 聚合规则：按 relativePath Ordinal 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 无 BOM、LF，并保留最终 LF。

工作区已有用户 dirty/untracked 与已验收未提交包。任务窗口必须记录 pre-existing status，只按第 12 节 package delta 判定；不得清理、覆盖、还原、暂存、认领或刷新既有基线。

## 14. 绝对禁止

```text
不修改任何已有文件
不修改 P3、P2、P1、N01C、N01B、Item、IF01、Enemy requirement/readiness
不直接读取 P1/P2/N01B、ItemSystemSnapshot、IF01 或 Enemy 私有状态
不重新装配 EvaluationInput，不调用 P3 Assembler
不修改或复制 N01C Evaluator/Validator/clauses/结构算法
不创建新的结构算法、路径、评分或 clause 规则
不根据名称、标签、文案、占格数、BP 或旧 MapRule 推导结果
不迁移真实 E10 requirement，不接 Enemy 总 readiness，不跨通道汇总
不连接 MapRule、Encounter、Battle、Board、Map runtime、Scene、Prefab、UI、RectTransform、BuildSettings
不写 SaveData、Reward、Chapter、RunFlow、Boss 或正式战斗结果
不生成 BP、score、threshold、ratio、difficulty、胜负、奖励或掉落结论
不启动 N02、PA01、C02B、C02R1、C03
不修改 AGENTS、LOCKED、ROADMAP、CURRENT、Package Queue、CurrentRules
不 commit、tag、push
不自动启动下一包
```

## 15. Verifier 与报告验收

Offline 与 Unity 入口冻结为：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceStructuralReadinessConsumerVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceStructuralReadinessConsumerVerifier.VerifyStaticBatch
```

两个入口必须委托同一 assertion core。可选 Editor Menu 只能委托该核心，不得建立第二套语义。

必须验证：

```text
Schema/Version、3 个 Consumer Status、Input/Result/Issue 字段精确
六种固定输出组合精确
P3 Result ValidateResult gate 与完整 P3→Consumer 真值表
Evaluate/ValidateResult 调用次数、异常与无 retry 语义精确
KnownTrue/KnownFalse/Unknown/NotApplicable 与全部 clause rows 原样保留
畸形 evaluator result、exception、undefined enum -> Invalid/null
20/20 synthetic fixtures PASS
Ordinal、InvariantCulture、集合反序、重复消费、防御性复制
Canonical Signature 格式、幂等、字段敏感性
P3 Assembler calls = 0
P1/P2/N01B/Item/IF01/Enemy source reads = 0
真实 requirement mapping / readiness aggregation = 0 / 0
Board/Map/Battle/Scene/Prefab/UI/BuildSettings connections = 0
BP/score/threshold/difficulty conversion = 0
C02 blocked rows = 32/32；Actual Unknown reduction = 0
现有文件修改 = 0
Protected hashes identical
Package Leak Count / GUID conflicts / trailing whitespace = 0
git diff --check = PASS
```

报告职责：

```text
LayoutResilienceStructuralReadinessConsumerReport.md
  总结独立通道语义、P3 来源、求值路径、状态解释、确定性与无正式接线。
LayoutResilienceStructuralReadinessConsumerSpec.csv
  固定 Schema、类型、字段、状态、错误码、Evaluator/Validator 路径与 Canonical。
LayoutResilienceStructuralReadinessConsumerTruthTable.csv
  逐行列出 P3 status/input、调用次数、N01C state 与 Consumer disposition。
LayoutResilienceStructuralReadinessConsumerFixtureCases.csv
  列出 20 个 fixture、期望状态、结果、clause、calls、codes 与签名断言。
LayoutResilienceStructuralReadinessConsumerLeakCheckReport.md
  逐项列出 P3 Assembler、P1/P2/N01B、Item/Enemy、真实映射、跨通道、runtime、BP 与白名单扫描。
```

若 Unity Editor 锁阻 batch，不得关闭用户 Editor 或结束 Unity 进程，不得声称 Unity PASS；如实报告 BLOCKED 并保留 Offline 结果。本包无需场景或玩家手测。

## 16. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-LayoutResilienceStructuralReadinessConsumer01

Guard receipts
New files / existing files modified
Schema / Status enum
Input / Result field inventory
P3 source gate
P3→Consumer truth table
KnownTrue / KnownFalse / Unknown / NotApplicable dispositions
Evaluate / ValidateResult call matrix
Exception / malformed-result disposition
20/20 fixture result
Canonical Signature
Protected hashes
Offline verifier
Unity compile / verifier
Real requirement mapping / readiness aggregation
C02 blocked rows / Actual Unknown reduction
Leak Count / GUID conflicts / trailing whitespace
git diff --check
Forbidden scope touched
Pre-existing dirty preserved
HEAD unchanged
Commit / tag / push: none
Next package: NOT_STARTED
```

## 17. 后续包顺序

```text
N01C-P4 开发、自测、用户验收、Guard/RepoOps 收口
-> 未来独立真实 LayoutResilience requirement migration Assignment（仍未授权）
-> 未来独立跨通道 readiness aggregation Assignment（仍未授权）
```

N02、PA01、C02B、C02R1、C03 继续按 Queue 阻塞。本 Assignment 不授权任何后续包自动启动。
