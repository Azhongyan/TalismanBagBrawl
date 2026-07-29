# V0.4-ItemCapabilityUnitContract01 Guard Assignment

状态：`GUARD_PASS_ASSIGNMENT_ITEMCAPABILITYUNITCONTRACT01 / READY_FOR_TASK_WINDOW`  
目标回执：`GUARD_PASS_ITEMCAPABILITYUNITCONTRACT01`  
维护方：Item Guard  
日期：2026-07-19

## 0. 包定位

本包只建立 Item 原生单位合同，并修正 `affix_nian_efficiency` 在候选重复表示中的错误单位、操作和值域。不进行 Item→BuildCapability 映射，不做 point / stack / count→BP 换算。

## 1. 正式规则

```text
affix_nian_efficiency
unit             = basisPoint
operation        = ReducePercent
total range      = 200-1800 BP
display meaning  = 2%-18%
trigger          = before_trigger
condition        = nian_cost_gt_0
```

五品阶固定范围：

```text
White   200-400 BP
Green   350-650 BP
Blue    550-900 BP
Purple  800-1300 BP
Orange  1200-1800 BP
```

当前 Catalog 候选重复数据中的 `point / ReduceFlat / 1-6` 必须同步为上述既有 Schema 数据。这是错误表示修正，不是重新设计数值。

## 2. 文件白名单

允许新增：

```text
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitValidator.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityUnitContractVerifier.cs
Docs/V0.4/Reports/ItemCapabilityUnitContractReport.md
Docs/V0.4/Reports/ItemCapabilityUnitContractSpec.csv
Docs/V0.4/Reports/ItemCapabilityUnitContractLeakCheckReport.md
```

允许定点修改：

```text
Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs
Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset
```

仅允许相应新文件/目录的 `.meta`。除此之外全部禁止修改。

## 3. 候选数据修正

只允许修正唯一的 `affix_nian_efficiency` 候选行：

```text
“耗念降低X点。” → “耗念降低X%。”
ReduceFlat       → ReducePercent
point            → basisPoint
1-6 rarity range → White 200-400 / Green 350-650 / Blue 550-900 / Purple 800-1300 / Orange 1200-1800 BP
effectTags 中 ReduceFlat → ReducePercent
```

Payload 示例值固定使用第一档合法最小值 `200`，只作为合法示例，不改变 Roll 规则。

不得修改通用 `Ranges()` 行为从而影响其他词条；应对该词条使用显式五档范围或定点专用构造。

## 4. 不做迁移或重建

```text
不运行 ItemBalanceCandidateSeedBuilder
不重新生成 30 个 Profile
不重新 Roll 150 个候选版本
不修改 ItemBalanceCandidateSeedBuilder.cs
不修改 ItemBalanceWorkbenchValidation.cs
不提升正式数据成熟度
```

`ItemCompleteCandidateContentSeed.Revision` 与各 Profile revision 保持不变；新合同使用独立版本标识。

## 5. 新合同

新建：

```text
ItemCapabilityUnitContractSnapshot.v1
```

至少覆盖：

```text
affix_control_up          point / AddFlat
affix_cleanse_up          stack / AddFlat
affix_chain_target        count / ExtraTarget
affix_trigger_refund      point / Refund
affix_first_trigger_bonus basisPoint
affix_nian_efficiency     basisPoint / ReducePercent
```

合同必须区分单位所属领域，禁止把控制点、念点、净化层、目标数量视为可互换单位。

解析状态必须支持：

```text
KnownValue
KnownZero
Unknown
Invalid
```

只有字段完整、单位和 semantic 合法且明确为 `0` 才是 KnownZero；缺失为 Unknown；负值、越界、单位冲突、错误 operation 为 Invalid。

## 6. Canonical 边界

必须保持不变：

```text
ItemSystemSnapshot.v1 / BuildDebugSignature
ItemInstanceProjectionContractSnapshot.v1 / Set v1
ItemAffixPoolAndRangeSchemaSnapshot.v1 的结构、SchemaId 及签名算法
ItemInstancePlacementBindingContractSnapshot.v1
绑定合同代表签名 sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
现有 150 次 Roll / Projection 结果签名
```

允许变化：

```text
ItemCompleteCandidateContent.cs 文件哈希
ItemBalanceWorkbenchCatalog.asset 文件哈希
新 ItemCapabilityUnitContractSnapshot.v1 的独立 Canonical Signature
```

编译后的 Affix Schema Canonical Signature 必须前后相同，因为正式 Schema 原本已经是正确的 BP 范围。

## 7. 验证

Offline 纯内存验证必须覆盖：

```text
合法单位
零值
缺失
负值
越界
单位冲突
operation 冲突
顺序反转
不可变集合
```

Unity batch 入口：

```text
TalismanBag.EditorTools.ItemCapability.ItemCapabilityUnitContractVerifier.VerifyStaticBatch
```

必须验证真实 Catalog、省念五档范围、Source/Config 一致性、Ordinal 排序、InvariantCulture、确定性签名，以及 150 个 Roll/Projection 签名不变。Verifier 不得保存 Scene、Prefab 或运行任何 Builder。

## 8. Protected Hash

任务开始时按当前磁盘状态记录并在结束时复核：

```text
EnemySystem
CrossSystem
ItemSystemSnapshot.cs
ItemInstanceProjectionContract.cs
ItemAffixPoolAndRangeSchema.cs
ItemInstancePlacementBinding 合同
30 个 Item Balance Profile
Assets/_Game/Scenes
Assets/_Game/Prefabs
Item UI 与美术目录
EditorBuildSettings.asset
```

工作区已有 dirty/untracked 内容不得清理、回退或纳入本包。

## 9. 禁止范围

```text
Enemy / CrossSystem / Battle / Board
RunFlow / SaveData / Reward / Chapter
Scene / Prefab / UI / RectTransform / BuildSettings
Item→BuildCapability 映射
任何 point / stack / count→BP 比例
正式掉落、Roll、品阶概率或奖励
ItemBalanceCandidateSeedBuilder / 历史 Builder
AGENTS.md / Docs/LOCKED/* / Guard owner docs
commit / tag / push / reset / rollback
```

不得自动启动 `ItemCapabilityRuntimeFactContract01`、C02B、C02R1 或 C03。

## 10. 回传

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemCapabilityUnitContract01
Guard receipt: GUARD_PASS_ITEMCAPABILITYUNITCONTRACT01
新增文件 / 修改已有文件
省念 source/config 修正结果
6 个原生单位合同结果
KnownValue / KnownZero / Unknown / Invalid 案例
Canonical Signature
Affix Schema Signature 前后
150 Roll / Projection Signature 前后
Protected hashes
Offline verifier
Unity verifier
Leak Count
git diff --check
Forbidden scope touched
HEAD status
```

包 3 的前置条件为 IF01 与 IF02 均 `GUARD_ACCEPTED / QA_PASS`，并由用户再次明确下发。
