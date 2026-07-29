# V0.4-ItemCapabilityRuntimeFactContract01 Guard Assignment

状态：`GUARD_PASS_ASSIGNMENT_ITEMCAPABILITYRUNTIMEFACTCONTRACT01 / READY_FOR_TASK_WINDOW`  
目标回执：`GUARD_PASS_ITEMCAPABILITYRUNTIMEFACTCONTRACT01`  
维护方：Item Guard  
日期：2026-07-19

## 0. 包定位

本包建立独立、只读、不可变、可空字段的 Item Runtime Fact 合同。只验证权威生产者输入的事件事实，不执行技能、词条、伤害、净化、控制或念计算，不连接 Battle、Sandbox、MonoBehaviour、Scene 或正式 Producer。

## 1. 文件白名单

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityRuntimeFactContractVerifier.cs
Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractReport.md
Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractSpec.csv
Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractLeakCheckReport.md
```

仅允许新增对应目录与 `.meta`。现有文件修改数必须为 `0`。

## 2. 独立合同

新建：

```text
ItemCapabilityRuntimeFactContractSnapshot.v1
```

至少包含：

```text
schemaId
bindingContractCanonicalSignature
unitContractCanonicalSignature
factCompleteness
Facts[]
ValidationErrors[]
canonicalSignature
```

每条事实至少包含：

```text
battleSessionId
eventId
eventSequence
itemInstanceId
baseItemId
placementId
triggerOrdinal
triggerSuccess
firstTriggerInBattle
cleanseSuccess
cleanseExtraStackCount
consecutiveTriggerCount
chainCount3
nianCostBefore
nianCostAfter
refundUnits
```

所有布尔和数值输入必须可空，禁止用默认 `false / 0` 代替缺失事实。

## 3. 状态模型

输出状态固定为：

```text
KnownTrue
KnownFalse
Unknown
Invalid
```

完整性至少区分：

```text
Complete
Incomplete
```

规则：

```text
Complete + 明确 true  = KnownTrue
Complete + 明确 false = KnownFalse
缺字段、缺事件、缺身份、Incomplete = Unknown
冲突、重复、负值、错误单位、身份错配 = Invalid
单条 KnownFalse 不自动等于整项 capability KnownZero
```

## 4. 身份验证

Validator 必须消费：

```text
ItemInstancePlacementBindingContractSnapshot.v1
ItemCapabilityUnitContractSnapshot.v1
```

每条事件必须在已验证 Binding 中精确找到：

```text
itemInstanceId ↔ placementId ↔ baseItemId
```

禁止猜测身份。I031、孤儿事件、重复身份、错配身份不得进入有效 Facts。

## 5. 事实规则

首次触发：

```text
firstTriggerInBattle = KnownTrue
必须同时满足同一 battleSessionId + itemInstanceId
triggerSuccess = KnownTrue
triggerOrdinal = 1
eventId / eventSequence 完整唯一
```

后续成功触发必须为 ordinal 2、3……；跨战斗会话重新从 1 开始。原始输入顺序可反转，但按 `eventSequence` 还原后 ordinal 倒退、重复或跳回必须 Invalid。

净化：

```text
cleanseSuccess 只能来自完整事件
cleanseExtraStackCount 使用 cleanse_stack / stack
```

静态持有 `affix_cleanse_up` 不构成净化成功。

连触：

```text
consecutiveTriggerCount 使用 target_count / count 语义边界
chainCount3=true  时 count >= 3
chainCount3=false 时 count < 3
```

连触是否中断由未来权威 Producer 报告，本合同不推导战斗重置规则。

念事实：

```text
nianCostBefore / nianCostAfter / refundUnits = nian_point / point
省念词条参数 = nian_cost_basis_point / basisPoint
```

两种单位不可互换。本合同只验证已报告事实，不根据省念 BP 计算 `nianCostAfter`。

## 6. Producer 边界

允许定义只读输入接口：

```text
IItemCapabilityRuntimeFactInputProvider
```

但只能在内存 Fixture 中实现。正式生产者必须另拆：

```text
V0.4-ItemCapabilityRuntimeFactProducerAdapter01
```

该未来包连接 Battle，归跨系统 / Battle Guard 审批，本包不得启动。

## 7. 事件与不可变规则

```text
eventId 在 Contract Snapshot 内唯一
(battleSessionId,eventSequence) 唯一
同一会话按数值 sequence 排序
同一实例的成功 triggerOrdinal 严格递增
允许 sequence 存在间隔
Facts / ValidationErrors / 嵌套集合必须防御复制并只读
外部数组或 IList<T> 修改不得影响 Snapshot
```

## 8. Canonical Signature

排序固定为：

```text
battleSessionId Ordinal
eventSequence numeric
eventId Ordinal
itemInstanceId Ordinal
```

Canonical Payload 必须覆盖全部公开字段、状态、完整性、两个前置合同签名及 ValidationErrors，并使用：

```text
StringComparer.Ordinal
CultureInfo.InvariantCulture
UTF-8
SHA-256
```

输入顺序反转必须产生相同签名；任一事实值变化必须改变签名。

## 9. Verifier

Offline 纯内存案例至少覆盖：

```text
首次触发 true / 后续 false / 跨会话重置
触发失败
净化成功与额外 stack
连触 2=false、3=true
念前后值与返还
Incomplete→Unknown
显式 false→KnownFalse
身份缺失/错配/I031
重复 eventId/sequence
ordinal 倒退
负数、单位冲突
输入反转确定性
集合不可变
```

Unity batch 入口：

```text
TalismanBag.EditorTools.ItemCapability.ItemCapabilityRuntimeFactContractVerifier.VerifyStaticBatch
```

Verifier 只使用内存 Fixture 和已发布合同，不得加载或保存 Scene/Prefab，不得运行 Builder。

## 10. Protected Hash

以下全部以任务开始时磁盘状态为基线，前后必须一致：

```text
ItemCapabilityUnitContract.cs / Validator
ItemInstancePlacementBindingContract.cs / Validator
ItemSystemSnapshot.cs
ItemInstanceProjectionContract.cs
ItemAffixPoolAndRangeSchema.cs
ItemCompleteCandidateContent.cs
ItemBalanceWorkbenchCatalog.asset 与 30 个 Profiles
ItemSandbox / Item Detail / Resources/item / Resources/item_daoju
Scenes / Prefabs / EditorBuildSettings.asset
EnemySystem / CrossSystem
```

固定 Canonical 基线：

```text
Unit Contract: sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673
Binding Contract: sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
Affix Schema: sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f
150 Roll: sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
150 Projection: sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
```

## 11. C02B 未来读取边界

C02B 未来只允许读取验证后的完整 Facts、身份字段、Truth 状态和两个前置合同签名。不得把 Unknown/Invalid 当作零，不得自行推导未报告事件。

即使本包通过，`point / stack / count → capability BP` 仍未批准；C02B、C02R1、C03 继续阻塞。

## 12. 禁止范围

```text
AutoCombatController / TalismanItemRuntime / 正式 Producer
技能、词条、伤害、净化、控制或念计算
Enemy / CrossSystem / Battle / Board
RunFlow / SaveData / Reward / Chapter
Scene / Prefab / UI / RectTransform / BuildSettings
ItemSystemSnapshot.v1 或既有合同签名算法
Builder / 历史写资源回归
AGENTS.md / Docs/LOCKED/* / Guard owner docs
commit / tag / push / reset / rollback
```

不得启动 Runtime Fact Producer Adapter、C02B、C02R1 或 C03。

## 13. 回传

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemCapabilityRuntimeFactContract01
Guard receipt: GUARD_PASS_ITEMCAPABILITYRUNTIMEFACTCONTRACT01
新增文件 / 修改已有文件
Fact / scenario counts
KnownTrue / KnownFalse / Unknown / Invalid counts
Identity / duplicate / sequence / ordinal checks
Canonical Signature
Input reversal / mutation sensitivity / immutability checks
Protected hashes and fixed signatures
Offline verifier
Unity verifier
Leak Count
git diff --check
Forbidden scope touched
HEAD status
```
