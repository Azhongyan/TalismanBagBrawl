# V0.4-LayoutResilienceEvaluationInputAssembler01 Assignment

## 1. Guard 结论

```text
Package: V0.4-LayoutResilienceEvaluationInputAssembler01
Queue Id: N01C-P3
Status: JOINT_GUARD_PASS / READY_FOR_DEV
Risk: YELLOW / PURE-DATA INPUT ASSEMBLY ONLY

GUARD_PASS_LAYOUTRESILIENCEEVALUATIONINPUTASSEMBLER01
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCEEVALUATIONINPUTASSEMBLER01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEEVALUATIONINPUTASSEMBLER01
```

本包只建立纯数据 Assembler：读取已验收的 P1 BuildFacts result、P2 Pressure result 与 N01B applicability Snapshot，装配并验证合法的 N01C `LayoutResilienceEvaluationInput`。

本包不调用 N01C Evaluator，不产生 predicate/clause/readiness 结果，不迁移真实 requirement，不修改任何前置合同或消费者。

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
```

前置状态固定：

```text
N01B    GUARD_ACCEPTED / QA_PASS
N01C    GUARD_ACCEPTED / QA_PASS
N01C-P1 GUARD_ACCEPTED / QA_PASS
N01C-P2 GUARD_ACCEPTED / QA_PASS
```

若任一前置文件、公开类型或 Protected baseline 不匹配，立即停止并返回 Guard，不得刷新基线或兼容猜测。

## 3. 所有权与依赖方向

本包拥有：

```text
Assembler 自身 Schema / Status / Issue
P1/P2/N01B 公开结果到 N01C EvaluationInput 的纯数据装配
显式 ID 选择、来源状态归一、真值表、Canonical Signature
Assembler 自身 Validator 与开发诊断
```

本包不拥有：

```text
P1 BuildFacts、P2 PressureSnapshot 或 N01B applicability 的生产与语义
ItemSystemSnapshot、IF01、Item 私有状态
真实 Enemy MapRule、Encounter、requirement、readiness
N01C 谓词算法、predicate/clause result
Board/Map/Battle/Scene/Prefab/UI/BuildSettings
BP、score、threshold、difficulty 或 readiness 汇总
```

Runtime 允许依赖：

```text
System.*
EnemyRequirementChannelApplicability.v1 的公开 Snapshot/row/enum
LayoutResilienceStructuralPredicate.v1 的 DTO 与 Validator
LayoutResilienceItemFactProjectionAdapter.v1 的公开 Result
AuthoredLayoutPressureSourceAdapter.v1 的公开 Result
```

Runtime 禁止依赖或调用：

```text
ItemSystemSnapshot / IF01 / Item namespace 或 provider
P1/P2 Adapter、Provider 或 Validator
N01B Provider 或 Validator
N01C Evaluator / Evaluate / ValidateResult
真实 Enemy、MapRule、Encounter、requirement、readiness consumer
UnityEngine、Board、Map、Battle、Scene、Prefab、UI、BuildSettings
```

## 4. Assembler Schema、状态与类型

固定：

```text
schemaId = LayoutResilienceEvaluationInputAssembler.v1
schemaVersion = 1
```

`LayoutResilienceEvaluationInputAssemblerStatus` 恰好三个命名有效值：

```csharp
Complete = 1
Unknown = 2
Invalid = 3
```

数值 `0` 与任何未定义整数必须拒绝。不得新增 KnownTrue、KnownFalse、NotApplicable、NotInChannel、Met、Unmet 或 Zero 作为 Assembler Status。

必须建立等价类型：

```text
LayoutResilienceEvaluationInputAssemblerSchema
LayoutResilienceEvaluationInputAssemblerStatus
LayoutResilienceEvaluationInputAssemblerIssue
LayoutResilienceEvaluationInputAssemblerInput
LayoutResilienceEvaluationInputAssemblerResult
ILayoutResilienceEvaluationInputAssembler
DefaultLayoutResilienceEvaluationInputAssembler
LayoutResilienceEvaluationInputAssemblerValidationCodes
DefaultLayoutResilienceEvaluationInputAssemblerValidator
```

Issue 只允许：

```text
Code
Status                    // Unknown 或 Invalid；不得为 Complete
Path
Message
```

## 5. 精确输入与输出字段

`LayoutResilienceEvaluationInputAssemblerInput` 顶层只允许：

```text
SchemaId
SchemaVersion
EvaluationId
RequirementId
ApplicabilitySnapshot     // N01B EnemyRequirementChannelApplicabilitySnapshot，可 null
BuildProjectionResult     // P1 LayoutResilienceItemFactProjectionResult，可 null
PressureSourceResult      // P2 AuthoredLayoutPressureSourceResult，可 null
```

不得增加 EncounterId、MapRuleId、capability key、BP、threshold、result hint、fallback applicability 或真实 requirement 映射字段。

`LayoutResilienceEvaluationInputAssemblerResult` 只允许：

```text
SchemaId
SchemaVersion
Status
EvaluationInput           // N01C LayoutResilienceEvaluationInput，可 null
Issues
CanonicalSignature
```

输出字段组合固定：

| Status | EvaluationInput | 含义 |
|---|---|---|
| Complete | 非 null | 已形成无未决装配事实的合法 N01C input；不表示谓词通过 |
| Unknown | 非 null | 已有显式 N01B row，但 applicability 或所需 P1/P2 来源不完整；N01C input 保留 Unknown/Incomplete |
| Unknown | null | N01B Snapshot 缺失，无法合法作者化 applicability |
| Invalid | null | Schema、ID、通道、来源结果形状或 N01C Validator 拒绝 |

Invalid 结果不得携带 EvaluationInput。Unknown 不得携带 KnownFalse、KnownZero、空布局失败或任何 predicate result。

## 6. ID 连接规则

三个身份命名空间相互独立：

```text
EvaluationId   = 调用方显式提供的 N01C 求值输入身份
RequirementId  = 在 N01B Snapshot 中选择唯一 row 的 Ordinal selector
PressureInputId = P2 LayoutPressureSnapshot 自身身份
```

固定规则：

1. `EvaluationId` 与 `RequirementId` 必须非空、无外侧空白，比较使用 Ordinal；输出 `EvaluationInput.EvaluationId` 原样复制显式值。
2. `RequirementId` 必须在非空 N01B Snapshot 中精确、大小写敏感地命中恰好一行。零行或多行命中均为 ID join Invalid；不得把零行命中解释为 applicability Unknown。
3. N01B Snapshot 的 `EvaluationChannel` 必须是 `StructuralPredicate`。选中行只有同通道的 Applicable、Unknown、NotApplicable 可进入装配；NotInChannel 必须拒绝为 Invalid。
4. P2 `PressureInputId` 必须原样留在其 `LayoutPressureSnapshot` 中，不 Trim、不改大小写、不拼接、不映射。
5. P1 当前没有稳定 BuildId；本包不得发明 BuildId、用 Canonical Signature 冒充业务 ID，或从 placement/item ID 推导 Build 身份。
6. 不要求 EvaluationId、RequirementId、N01B SnapshotId 与 PressureInputId 相等，也不得从它们相互派生。
7. Canonical Signature 绑定这些独立身份及被选中的公开结果，但不建立 requirement→pressure 业务映射。

所有 fixture 只能使用 `dev.*` / `fixture.*` 等 synthetic ID；真实 E06/E10 requirement ID、EncounterId、MapRuleId 和四个 capability key 出现数必须为 0。

## 7. 上游结果资格与归一

Assembler 只能读取三个前置包已经公开的不可变结果，不得回读其来源。

### 7.1 P1 Build source

| P1 输入 | 归一后的 BuildFactsCompleteness | BuildFacts payload |
|---|---|---|
| null | Incomplete | null |
| Status=Complete 且前置结果形状合法 | Complete | 原样防御性复制，必须非 null |
| Status=Unknown 且前置结果形状合法 | Incomplete | 保留 P1 已公开的安全 partial payload；允许 null |
| Status=Invalid | — | 全局 Invalid，不输出 EvaluationInput |

P1 SchemaId/Version、Status、BuildFactsCompleteness、payload、Issues 与 Canonical Signature 必须符合已验收 P1 合同。任何未定义枚举、Schema mismatch、Complete/Unknown/Invalid 与 payload/completeness 的矛盾或非法 signature 格式均为 Invalid。不得调用 P1 Adapter/Validator 重新判断来源。

### 7.2 P2 Pressure source

| P2 输入 | 归一后的 PressureFactsCompleteness | Pressure payload |
|---|---|---|
| null | Incomplete | null |
| Status=Complete 且前置结果形状合法 | Complete | 原样防御性复制，必须非 null |
| Status=Unknown 且前置结果形状合法 | Incomplete | null |
| Status=Invalid | — | 全局 Invalid，不输出 EvaluationInput |

P2 SchemaId/Version、Status、PressureFactsCompleteness、payload、Issues 与 Canonical Signature 必须符合已验收 P2 合同。任何未定义枚举、Schema mismatch、结果形状矛盾或非法 signature 格式均为 Invalid。不得调用 P2 Adapter/Validator，也不得补 cell、eye、edge、kind 或 clause。

### 7.3 N01B applicability source

```text
null Snapshot
  -> Unknown / null EvaluationInput；不存在的事实不能合成 applicability=Unknown。

显式 Applicable row
  -> 使用第 8 节 Applicable 矩阵。

显式 Unknown row
  -> 可形成 ApplicabilityState=Unknown 的合法 N01C input；Assembler Status 固定 Unknown。

显式 NotApplicable row
  -> 只形成 NotRequired/NotRequired + null/null；不得由 P1/P2 缺失推导。

显式 NotInChannel row
  -> Invalid / null EvaluationInput。
```

N01B SchemaId/Version、SnapshotId、EvaluationChannel、row ID、DeclaredChannel、ApplicabilityState、唯一性与 Canonical Signature 格式必须符合已验收 N01B 合同。不得调用 N01B Provider/Validator，不得用缺行合成状态。

## 8. 完整状态真值表

先归一 P1/P2 为：

```text
C = 合法 Complete
U = 合法 Unknown
M = null / Missing
I = Invalid 或任何来源结果形状畸形
```

全局优先级固定：

```text
Assembler 输入自身 Invalid、N01B/ID/通道 Invalid、P1 I、P2 I、N01C Validator issue
  > explicit applicability Unknown / source Unknown / source Missing
  > Complete
```

任何 `I` 在 Applicable、Unknown、NotApplicable 或 N01B 缺失时都使最终结果为 `Invalid + null`。Invalid 必须收集双方稳定排序的诊断；双方 Invalid 不得只保留先遇到的一侧。

### 8.1 Applicability=Applicable

| P1 | P2 | Assembler Status | Build side | Pressure side | EvaluationInput |
|---|---|---|---|---|---|
| C | C | Complete | Complete + payload | Complete + payload | 非 null |
| C | U | Unknown | Complete + payload | Incomplete + null | 非 null |
| C | M | Unknown | Complete + payload | Incomplete + null | 非 null |
| U | C | Unknown | Incomplete + P1 safe payload/null | Complete + payload | 非 null |
| U | U | Unknown | Incomplete + P1 safe payload/null | Incomplete + null | 非 null |
| U | M | Unknown | Incomplete + P1 safe payload/null | Incomplete + null | 非 null |
| M | C | Unknown | Incomplete + null | Complete + payload | 非 null |
| M | U | Unknown | Incomplete + null | Incomplete + null | 非 null |
| M | M | Unknown | Incomplete + null | Incomplete + null | 非 null |

### 8.2 Applicability=Unknown

沿用上表逐侧 completeness/payload。所有九个 C/U/M 组合的 Assembler Status 都是 Unknown，输出非 null、`ApplicabilityState=Unknown` 的合法 N01C input。即使 P1/P2 都 Complete，也不得升级为 Applicable 或运行求值。

### 8.3 Applicability=NotApplicable

只要 P1/P2 不是 I，所有 C/U/M 组合都固定输出：

```text
Assembler Status = Complete
ApplicabilityState = NotApplicable
BuildFactsCompleteness = NotRequired
PressureFactsCompleteness = NotRequired
BuildFacts = null
Pressure = null
```

已提供的非 Invalid P1/P2 payload 必须丢弃，不得带入 N01C input。P1/P2 null 不产生 Unknown，因为该上下文已由 N01B 明确声明 NotApplicable。任何 P1/P2 Invalid 仍按全局优先级使装配 Invalid。

### 8.4 N01B 缺失与 NotInChannel

```text
N01B Snapshot == null -> Unknown + null EvaluationInput
N01B exact RequirementId row missing/duplicate -> Invalid + null
selected NotInChannel -> Invalid + null
undefined applicability/channel -> Invalid + null
Assembler input == null -> Invalid + null
```

## 9. 唯一装配与 Validator-only 流程

1. 验证 Assembler Schema、EvaluationId、RequirementId 与所有已提供上游结果的公开形状。
2. 扫描所有已提供来源并收集 Invalid；Invalid 优先于 Unknown，不得短路丢失另一侧 Invalid 诊断。
3. 在 N01B Snapshot 中以 `StringComparer.Ordinal` 选择唯一 RequirementId row，并应用第 6 至 8 节。
4. 按真值表构造候选 N01C `LayoutResilienceEvaluationInput`；所有 payload 均防御性复制。
5. 只允许调用 `DefaultLayoutResilienceStructuralPredicateValidator.Validate(candidate)`。
6. 任一 N01C validation issue 使 Assembler `Invalid` 并丢弃 candidate；不得把 issue 转成 KnownFalse。
7. 验证通过后返回 Complete/Unknown、合法 EvaluationInput、稳定 Issues 与 Canonical Signature。

绝对禁止：

```text
调用 ILayoutResilienceStructuralPredicateEvaluator / Default...Evaluator / Evaluate
调用 N01C ValidateResult
创建 LayoutResiliencePredicateResultSnapshot 或 LayoutResiliencePredicateClauseSnapshot
输出 KnownTrue、KnownFalse、Satisfied、Violated、`LayoutResiliencePredicateState.NotApplicable` 业务结果或 readiness
调用 P1/P2/N01B Adapter、Provider 或 Validator 重新生产事实
```

本包允许装配 N01C input，不允许执行该 input。

## 10. Canonical Signature 与不可变性

格式固定：

```text
sha256: + 64 lowercase hex
```

Canonical payload 必须覆盖：

```text
Assembler SchemaId / SchemaVersion
Assembler input presence
EvaluationId / RequirementId
N01B source presence、Schema/Version、SnapshotId、EvaluationChannel、CanonicalSignature、选中 row
P1 source presence、Schema/Version、Status、Completeness、CanonicalSignature、公开 BuildFacts、Issues
P2 source presence、Schema/Version、Status、Completeness、CanonicalSignature、公开 PressureSnapshot、Issues
Assembler Status
输出 EvaluationInput 全部七个 N01C 顶层字段及两个 payload 的完整中立数据
Assembler Issues
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
issue：Path Ordinal，再 Code Ordinal，再 Message Ordinal
null 与显式空集合必须是不同 payload
```

必须证明：

```text
相同内容重复装配、Culture 改变、上游输入集合反序 -> 相同结果与签名
改变 EvaluationId、RequirementId、选中 applicability、任一上游 signature/payload/status 或输出 -> 签名变化
调用后修改原始 synthetic source 集合 -> Result、EvaluationInput 与签名不变
不读取时间、随机数、文件系统、静态可变状态或 Unity runtime
```

不得用 P1/P2/N01B Canonical Signature 替代字段校验，也不得重新解释其业务语义。

## 11. Synthetic fixtures

固定 22 个场景，全部使用 dev/synthetic ID：

```text
1. Applicable + P1 Complete + P2 Complete -> Complete / C+C / input
2. Applicable + P1 Unknown(partial payload) + P2 Complete -> Unknown / I+C / input
3. Applicable + P1 Complete + P2 Unknown -> Unknown / C+I / input
4. Applicable + P1 Unknown + P2 Unknown -> Unknown / I+I / input
5. Applicable + P1 Invalid + P2 Complete -> Invalid / null
6. Applicable + P1 Complete + P2 Invalid -> Invalid / null
7. Applicable + P1 Invalid + P2 Invalid -> Invalid / null，双方 issue 均保留
8. Applicable + P1 null + P2 Complete -> Unknown / I+C / input
9. Applicable + P1 Complete + P2 null -> Unknown / C+I / input
10. Applicable + P1 null + P2 null -> Unknown / I+I / input
11. explicit applicability Unknown + P1/P2 Complete -> Unknown / C+C / input
12. explicit applicability Unknown + P1/P2 missing/Unknown -> Unknown / I+I / input
13. explicit NotApplicable + P1/P2 null -> Complete / NotRequired+NotRequired / null payloads
14. explicit NotApplicable + P1/P2 Complete -> 同 13，两个 payload 被丢弃
15. explicit NotApplicable + 任一 upstream Invalid -> Invalid / null
16. selected NotInChannel -> Invalid / null
17. N01B Snapshot null -> Unknown / null，不合成 applicability
18. Assembler SchemaId 或 SchemaVersion mismatch -> Invalid / null
19. N01B Schema/Version 或 EvaluationChannel mismatch -> Invalid / null
20. RequirementId 大小写/Ordinal 错配、零命中或重复命中 -> Invalid / null
21. P1 BoardSize 与 P2 domain 不兼容 -> N01C Validator rejected / Invalid / null
22. 输入反序、Culture 改变、重复装配、防御性复制与字段敏感性 -> deterministic PASS
```

Verifier 还必须覆盖：

```text
所有 enum 0/undefined 拒绝
P1/P2 result shape mismatch 与非法 signature format 拒绝
EvaluationId 原样复制；PressureInputId 原样保留；无跨命名空间相等要求
N01C Validator-only = PASS
N01C Evaluator/ValidateResult call count = 0
predicate result / clause result / readiness result = 0
真实 requirement/readiness mapping = 0
C02 blocked rows = 32/32；Actual Unknown reduction = 0
```

Fixture setup 只能在 Editor verifier 内以内存 synthetic 数据取得前置公开结果；不得读取真实 Item/IF01 provider、asset、Scene、MapRule、Encounter 或 requirement。Runtime Assembler 对 ItemSystemSnapshot/IF01 的类型、路径和调用引用必须为 0。

## 12. 精确文件白名单

任务窗口只允许新增以下 14 个文件：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerPrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssembler.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssembler.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md
Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerSpec.csv
Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerTruthTable.csv
Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerFixtureCases.csv
Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerLeakCheckReport.md
```

预期新增：`14`。允许修改已有文件：`0`。

本 Assignment 由 Guard 写入，不计入任务窗口 14 文件。任务窗口不得修改 Assignment、Queue、CurrentRules、AGENTS、LOCKED 或任何前置文件。

## 13. Protected baseline

冻结且不允许刷新：

```text
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

工作区已有用户 dirty/untracked 与已验收未提交包。任务窗口必须记录 pre-existing status，只按第 12 节 package delta 判定；不得清理、覆盖、还原、暂存、认领或刷新任何既有基线。

## 14. 绝对禁止

```text
不修改任何已有文件
不修改 P1、P2、N01B、N01C、Item、IF01、Enemy requirement 或 readiness
不直接读取 ItemSystemSnapshot、IF01、Item 私有状态或真实 Enemy 内容
不作者化 BuildFacts、Pressure、applicability 或 requirement→pressure mapping
不调用 N01C Evaluator、Evaluate、ValidateResult 或生成 predicate/clause result
不把 Unknown/Incomplete/null/空布局变成 false、zero、KnownFalse 或 NotApplicable
不修改 Scene、Prefab、Board、Map、Battle、UI、RectTransform、BuildSettings
不接 Runtime Producer、事件、计时器、状态机或 Battle tick
不增加 BP、score、threshold、ratio、weight、clamp、difficulty 或 readiness 汇总
不输出正式 ReadinessBand、难度、胜负、奖励或掉落结论
不修改 RunFlow、SaveData、Reward、Chapter、Boss
不实现真实 LayoutResilience requirement/readiness migration
不启动 N02、PA01、C02B、C02R1、C03
不修改 AGENTS、LOCKED、ROADMAP、CURRENT、Package Queue、CurrentRules
不 commit、tag、push
不自动启动下一包
```

## 15. Verifier 与报告验收

Offline 与 Unity 入口冻结为：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceEvaluationInputAssemblerVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceEvaluationInputAssemblerVerifier.VerifyStaticBatch
```

两个入口必须委托同一 assertion core。可选 Editor Menu 只能委托该核心，不得建立第二套语义。

必须验证：

```text
Schema/Version、3 个 Status、Input/Result/Issue 字段精确
ID join 与三个独立命名空间规则精确
Applicable/Unknown/NotApplicable/NotInChannel 完整真值表
Invalid > Unknown；双方 Invalid issue 均保留且稳定排序
null N01B、null P1/P2、Schema/Version mismatch、ID mismatch 精确
P1 partial payload 保留；P2 Unknown payload 为 null；NotApplicable payload 丢弃
22/22 synthetic fixtures PASS
Ordinal、InvariantCulture、集合反序、重复装配、防御性复制
Canonical Signature 格式、幂等、字段敏感性
N01C Validator-only = PASS
N01C Evaluator / ValidateResult calls = 0
predicate result / clause result / readiness result = 0
Runtime ItemSystemSnapshot/IF01/provider/private-state references = 0
真实 MapRule/Encounter/requirement/readiness rows = 0
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
LayoutResilienceEvaluationInputAssemblerReport.md
  总结所有权、来源资格、ID、真值表、Validator-only、确定性与无行为变化。
LayoutResilienceEvaluationInputAssemblerSpec.csv
  固定 Schema、类型、输入输出字段、状态、错误码、Canonical 与禁止项。
LayoutResilienceEvaluationInputAssemblerTruthTable.csv
  逐行列出 applicability、P1/P2 来源状态、completeness、payload、Assembler status。
LayoutResilienceEvaluationInputAssemblerFixtureCases.csv
  列出 22 个 fixture、期望状态、input disposition、codes 与签名断言。
LayoutResilienceEvaluationInputAssemblerLeakCheckReport.md
  逐项列出 Item/IF01、真实 Enemy、Evaluator/result/readiness、runtime、BP 与白名单扫描。
```

若 Unity Editor 锁阻 batch，不得关闭用户 Editor 或结束 Unity 进程，不得声称 Unity PASS；如实报告 BLOCKED 并保留 Offline 结果。本包无需场景或玩家手测。

## 16. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-LayoutResilienceEvaluationInputAssembler01

Guard receipts
New files / existing files modified
Schema / Status enum
Input / Result field inventory
ID join rules
Applicable / Unknown / NotApplicable / NotInChannel dispositions
Full source truth table
Invalid precedence
N01C Validator result
N01C Evaluator / predicate / clause / readiness counts
22/22 fixture result
Canonical Signature
Protected hashes
Offline verifier
Unity compile / verifier
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
N01C-P3 开发、自测、用户验收、Guard/RepoOps 收口
-> 未来独立 LayoutResilience requirement migration Assignment（仍未授权）
-> 未来独立 structural readiness consumer Assignment（仍未授权）
```

N02、PA01、C02B、C02R1、C03 继续按 Queue 阻塞。本 Assignment 不授权任何后续包自动启动。
