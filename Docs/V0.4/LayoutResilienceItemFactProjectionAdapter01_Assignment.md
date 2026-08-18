# V0.4-LayoutResilienceItemFactProjectionAdapter01 Assignment

Joint Guard receipts:

```text
GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01
ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01
```

生成日期：2026-07-20

所属：Overall / Cross-System Guard + Item Guard + Capability Algorithm Guard

包序号：N01C-P1

## 1. 包定位

本包只建立只读 Adapter，将权威 `ItemSystemSnapshot.v1` 中获准的公开事实与必要的 `ItemInstancePlacementBindingContractSnapshot.v1` 身份证据，确定性投影为既有 `LayoutResilienceStructuralPredicate.v1` 的 `LayoutResilienceBuildFactSnapshot`。

本包拥有：

```text
公开 Item Snapshot -> 中立整数 cell DTO 的只读字段投影
普通 placement 与 IF01 三方身份的完备性检查
I031 系统 placement 例外
Complete / Unknown / Invalid 投影状态、诊断与 Canonical Signature
```

本包不拥有：

```text
Item、IF01 或 N01C 事实生产和 Schema
地图压力、applicability、结构谓词结果或 readiness
Board/Map/Battle/Scene/UI/Runtime Producer
任何 BP、score、threshold、weight 或 capability normalization
```

风险等级：`YELLOW / READ-ONLY CROSS-SYSTEM ADAPTER ONLY`。

## 2. 前置与启动必读

前置状态：

```text
N01C LayoutResilienceStructuralPredicateContract01 = GUARD_ACCEPTED / QA_PASS
N01C Offline / Unity verifier = 348/348 PASS
N01C Canonical Signature = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
IF01 ItemInstancePlacementBindingContract01 = GUARD_ACCEPTED / QA_PASS
```

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/*
Docs/CURRENT/*
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/ItemAlgorithmGuard_CurrentRules.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/LayoutResilienceStructuralPredicateContract01_Assignment.md
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFieldMatrix.csv
Docs/V0.4/Reports/ItemInstancePlacementBindingContractReport.md
Docs/V0.4/Reports/ItemInstancePlacementBindingContractSpec.csv
Docs/V0.4/LayoutResilienceItemFactProjectionAdapter01_Assignment.md
```

只读源码：

```text
Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/**
```

## 3. 输入所有权与唯一合格字段

### 3.1 ItemSystemSnapshot.v1

只允许读取以下公开只读事实：

```text
ItemSystemSnapshot.schemaVersion
ItemSystemSnapshot.isValid / validationErrors
ItemSystemSnapshot.boardSize
ItemSystemSnapshot.eyeCell
ItemSystemSnapshot.catalogItems
ItemSystemSnapshot.placements

ItemSystemCatalogItemSnapshot.itemId
ItemSystemCatalogItemSnapshot.ShapeCells

ItemSystemPlacementSnapshot.placementId
ItemSystemPlacementSnapshot.itemId
ItemSystemPlacementSnapshot.anchorCell
ItemSystemPlacementSnapshot.rotation
ItemSystemPlacementSnapshot.OccupiedCells
ItemSystemPlacementSnapshot.coreCellWorld
ItemSystemPlacementSnapshot.isLit
ItemSystemPlacementSnapshot.isCountedInBuild
```

禁止读取或推断：

```text
Item 私有字段、反射字段、SerializedObject、资源资产或配置内部状态
displayName、名称、标签、文案、shapeId、等级、稀有度、词缀、开窍、监控位
litByItemId / litByPlacementId / litDepth、LitRangeCells 或正式供能结论
OccupiedArrayBonusCells、阵脉、Build Track 计数或 selectedMainBuild
Board/GameObject/Transform/RectTransform、Scene 或运行时对象
```

### 3.2 IF01 身份合同

只允许消费已生成的：

```text
ItemInstancePlacementBindingContractSnapshot.schemaId
status / isValid
Bindings[itemInstanceId, placementId, baseItemId]
ValidationErrors
canonicalSignature
```

Adapter 不得调用 IF01 Validator 重新生产合同，不得读取 `ItemInstanceProjectionSetSnapshot`，也不得把 `itemInstanceId == placementId` 当作证据。

### 3.3 N01C 目标合同

目标类型必须直接复用且不得修改：

```text
LayoutCellCoordinate
LayoutResiliencePlacedItemFactSnapshot
LayoutResilienceBuildFactSnapshot
LayoutResilienceInputCompleteness
DefaultLayoutResilienceStructuralPredicateValidator（仅验证 BuildFacts 兼容性）
```

不得修改 N01C 类型、字段、枚举、Canonical Signature 或 Evaluator。

## 4. 固定字段投影

顶层映射唯一：

| Source | Target | Rule |
|---|---|---|
| `ItemSystemSnapshot.boardSize` | `BuildFacts.BoardSize` | 精确复制 |
| `ItemSystemSnapshot.eyeCell.x/y` | `BuildFacts.BaselineEyeCell.X/Y` | 精确整数投影 |
| `ItemSystemSnapshot.placements` | `BuildFacts.PlacementRows` | 按 PlacementId Ordinal 排序后逐行投影 |

每个 placement row 的映射唯一：

| Source | Target | Rule |
|---|---|---|
| `placement.placementId` | `PlacementId` | 精确 Ordinal 身份 |
| `placement.itemId` | `ItemId` | 精确复制；它是 base Item 定义身份 |
| `placement.anchorCell.x/y` | `AnchorCell.X/Y` | 精确整数投影 |
| `placement.rotation` | `Rotation` | 精确复制，不重新旋转 |
| `catalogItems[itemId].ShapeCells` | `ShapeCells` | 唯一同 itemId Catalog row 的本地 cells |
| `placement.OccupiedCells` | `OccupiedCells` | 已物化世界 cells；不重新摆放 |
| `placement.coreCellWorld` | `CoreCellWorld` | 精确整数投影 |
| `placement.isLit` | `IsLit` | 基线资格事实；不重算点亮 |
| `placement.isCountedInBuild` | `IsCountedInBuild` | 基线 Build 归属事实；不重算 Build |

禁止添加 `itemInstanceId` 到 N01C BuildFacts。IF01 只证明普通 placement 身份链完整，不扩展目标 Schema。

占格数量只能用于复制 cells、重复检查和 N01C 已有结构一致性验证；不得换算 BP、score、ratio、weight、threshold、clamp、紧凑度或布局强弱。

## 5. 普通 placement 与 IF01 Join

`ordinary placement` 固定定义为：

```text
placement.itemId != "I031"（StringComparison.Ordinal）
```

每个普通 placement 必须存在且只存在一个可验证 IF01 row：

```text
binding.placementId == placement.placementId
binding.baseItemId == placement.itemId
binding.itemInstanceId 非空
```

不得：

```text
从 placementId、itemId、索引或名称生成 itemInstanceId
因 itemInstanceId 与 placementId 文本相等而视为已绑定
按 itemId 选择第一件同类实例
把缺 binding 的 placement 当成未摆放、false、zero 或合法 Complete
```

跨源状态：

```text
普通 placement 缺 IF01 row                      -> Unknown
IF01 row 指向当前 Item Snapshot 中不存在的普通 placement -> Unknown
IF01 status == Unknown                           -> Unknown
baseItemId 与 placement.itemId 不一致             -> Invalid
重复 itemInstanceId / placementId、Schema 错误或 IF01 Invalid -> Invalid
```

若当前 Item Snapshot 中普通 placement 数量为 0，则 IF01 不属于必要输入：缺 IF01 可以保持 Complete。若调用方仍提供 IF01，则它必须与当前 Item Snapshot 一致；额外/孤儿 binding 仍为 Unknown，Invalid 或 I031 伪造仍为 Invalid。

## 6. I031 系统 placement 规则

I031 固定为系统聚念石/阵眼 placement，不是普通生成实例：

```text
I031 placement 可以按第 4 节投影为 N01C 几何 row。
I031 不要求且不允许普通 IF01 binding。
Adapter 不创建、猜测或输出 I031 itemInstanceId。
禁止使用 placementId、"I031"、"system:I031" 或任何派生文本伪造 itemInstanceId。
IF01 中 baseItemId == I031，或 IF01 row 指向 I031 placement -> Invalid。
I031 的 IsLit / IsCountedInBuild 必须原样复制，不得强制改写。
```

有效 I031-only Snapshot 可以得到 `Complete` BuildFacts；它不因没有普通 binding 而成为 Unknown。I031 是否计入 Build 继续由 ItemSystemSnapshot 的权威事实决定，本包不解释或修改。

## 7. Adapter Schema 与输出状态

固定：

```text
schemaId = LayoutResilienceItemFactProjectionAdapter.v1
schemaVersion = 1
```

`LayoutResilienceItemFactProjectionStatus` 恰好三个命名有效值：

```csharp
Complete = 1
Unknown = 2
Invalid = 3
```

必须建立等价语义：

```text
LayoutResilienceItemFactProjectionSchema
LayoutResilienceItemFactProjectionStatus
LayoutResilienceItemFactProjectionIssue
LayoutResilienceItemFactProjectionResult
ILayoutResilienceItemFactProjectionAdapter
DefaultLayoutResilienceItemFactProjectionAdapter
LayoutResilienceItemFactProjectionValidationCodes
```

Result 只允许携带：

```text
SchemaId / SchemaVersion
Status
BuildFactsCompleteness
BuildFacts
Issues
CanonicalSignature
```

Issue 可以携带：

```text
Code / Status
ItemInstanceId（仅来源诊断，不进入 BuildFacts）
PlacementId / ItemId
Message
```

固定状态映射：

| Adapter Status | N01C BuildFactsCompleteness | BuildFacts payload | 可交给未来 N01C 输入 |
|---|---|---|---|
| Complete | Complete | 必须非 null，可含 0..n rows | 可以，但本包不接线/不求值 |
| Unknown | Incomplete | Item 来源可安全投影时允许完整或部分只读 payload；否则 null | 只能保持 Incomplete，不能求 KnownFalse |
| Invalid | Incomplete | 必须 null | 不得交给 N01C；先修复来源 |

`NotRequired`、`NotApplicable`、`KnownTrue`、`KnownFalse` 均不属于本 Adapter 输出。Invalid 优先级高于 Unknown；任一 Invalid issue 使结果 Invalid。

## 8. 缺失、Invalid 与空布局语义

```text
Item Snapshot == null                            -> Unknown / Incomplete / null BuildFacts
Item schema 不匹配、isValid == false 或有 validationErrors -> Invalid / null BuildFacts
必要 IF01 == null                                -> Unknown / Incomplete
IF01 Unknown 或普通身份缺失/孤儿                 -> Unknown / Incomplete
IF01 Invalid、重复或三方身份矛盾                 -> Invalid / null BuildFacts
Catalog 缺 placed item、重复 itemId 或 Shape/Placement 结构矛盾 -> Invalid / null BuildFacts
```

以下均是合法 Complete，不得自动转成 Unknown、Invalid、NotApplicable、false 或 zero：

```text
有效 Item Snapshot 的 placements 为空，且没有必要的普通 IF01 身份
只有 I031 placement
存在 placement 但所有 IsCountedInBuild == false
存在未点亮且未计入 Build 的合法 placement
```

`IsLit` 或 `IsCountedInBuild` 均不能单独证明布局韧性；本包只复制基线事实。

## 9. 唯一投影流程

固定顺序：

1. 检查 Item source presence、`ItemSystemSnapshot.v1` Schema 与 `isValid`。
2. 收集公开 Catalog/Placement rows；只按 Ordinal 身份查找，不做 fallback。
3. 判断普通 placement 数量，并按第 5、6 节验证必要 IF01 和 I031 例外。
4. 将所有 Item placement（包括 I031、未点亮或未计入 Build 的合法 rows）按第 4 节投影。
5. 用 N01C 中立类型做防御性复制；不得重新旋转、摆放、点亮或 Build 解析。
6. 仅允许构造 `Applicability=Unknown + PressureFactsCompleteness=Incomplete + pressure=null` 的内部 validation-only envelope，调用 `DefaultLayoutResilienceStructuralPredicateValidator` 验证 BuildFacts 兼容性。
7. 任一 N01C BuildFacts validation issue 视为 Invalid；不得调用 `ILayoutResilienceStructuralPredicateEvaluator` 或任何 `Evaluate` 方法。
8. 按 Invalid > Unknown > Complete 生成结果、诊断和 Canonical Signature。

Validation-only envelope 不是正式 `LayoutResilienceEvaluationInput` 输出，不得保存、报告为 Encounter 结果或连接压力。

## 10. Validator 不变量

至少必须检查：

```text
Item/IF01 Schema 精确；所有未定义 Adapter enum 被拒绝
Item isValid 与 validationErrors 一致
boardSize > 0；eyeCell、anchor、occupied、core 均在 boardSize 内
Catalog itemId 与 placementId Ordinal 唯一
每个 placement 恰有一个 Catalog row；ShapeCells 非空且唯一
placementId/itemId 非空；rotation 仅 0/90/180/270
OccupiedCells 非空且唯一；Shape/Occupied cardinality 相等
coreCellWorld 属于 OccupiedCells
IsCountedInBuild == true 且 IsLit == false 为 Invalid
普通 placement 的 IF01 完整性、唯一性与 base identity 一致
I031 无普通 binding 且没有伪造 itemInstanceId
Unknown/Invalid/Complete 与 payload/completeness 组合精确
空 placement rows 合法 Complete
防御性复制、只读集合和 Ordinal 排序
```

不得通过反射、私有字段、名称、标签、中文文案、资源路径或同类第一实例补足事实。

## 11. Canonical Signature 与确定性

格式固定：

```text
sha256: + 64 lowercase hex
```

签名只覆盖获准来源与 Adapter 结果：

```text
Adapter Schema/Version
Item source presence、schemaVersion、isValid
boardSize / eyeCell
仅 placed item 对应的 Catalog ShapeCells
全部 placement 的第 4 节字段
IF01 presence、schemaId、status、Bindings、ValidationErrors、canonicalSignature
Adapter Status / BuildFactsCompleteness / BuildFacts / Issues
```

不得调用或嵌入完整 `ItemSystemSnapshot.BuildDebugSignature()`，避免未获准的 Item 字段影响 Adapter 身份。

规范顺序：

```text
Catalog/Placement/Binding/Issue identity: StringComparer.Ordinal
cells: Y ascending, then X ascending
整数: InvariantCulture decimal
bool: 固定 true/false
输入集合重排不改变签名
```

必须证明重复投影、不同 Culture 和输入集合重排得到相同结果/签名；改变任一获准字段、binding、status 或 issue 必须改变签名；输入集合在调用后被修改不得影响输出。

## 12. Synthetic fixture 与 no-behavior-change

Fixture 只使用通用 dev IDs，不得读取真实存档、场景、地图、Encounter 或 requirement。

固定 15 个场景：

```text
1. 两个普通 placement + I031，显式 IF01 完整 -> Complete
2. 输入反序 -> 与 1 相同签名
3. 同 baseItemId 多 placement、各自唯一 binding -> Complete
4. 有效空 Item Snapshot + 无必要 IF01 -> Complete / 0 rows
5. I031-only + 无 IF01 -> Complete / 1 system row / no itemInstanceId
6. 全部 placement 未计入 Build，但身份完整 -> Complete
7. Item source missing -> Unknown
8. 普通 placement 存在而 IF01 missing -> Unknown
9. IF01 Unknown / binding missing -> Unknown
10. IF01 binding 相对当前 Item source 孤儿 -> Unknown
11. Item source Invalid -> Invalid
12. IF01 source Invalid/duplicate -> Invalid
13. baseItemId 与 placement.itemId mismatch -> Invalid
14. I031 ordinary binding forgery -> Invalid
15. 输出不可变、Culture 稳定、重复投影幂等
```

必须证明：

```text
N01C Evaluator calls = 0
Pressure snapshot/config rows = 0
Real requirement/Encounter/readiness mappings = 0
Item/IF01/N01C existing-file modifications = 0
Board/Map/Battle/Scene/Prefab/UI/RectTransform connections = 0
occupied-cell -> BP/score conversions = 0
C02 仍为 32/32 blocked rows
Actual Unknown reduction = 0
```

## 13. 精确文件白名单

任务窗口只允许新增以下 14 个文件：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterReport.md
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterSpec.csv
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFieldMatrix.csv
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFixtureCases.csv
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterLeakCheckReport.md
```

预期新增文件：`14`。允许修改已有文件：`0`。

本 Assignment 由 Guard 写入，不计入任务窗口 14 文件。任务窗口不得修改本 Assignment、Package Queue、CurrentRules、AGENTS 或 LOCKED。

Runtime 依赖方向固定：

```text
ItemFactProjection -> System.*
ItemFactProjection -> ItemSystemSnapshot.v1 公共只读类型
ItemFactProjection -> IF01 公共只读合同
ItemFactProjection -> N01C 中立 BuildFacts/Validator
ItemFactProjection -> UnityEngine.Vector2Int 仅用于 x/y DTO 桥接
ItemFactProjection -X-> Item private/config/resource/provider/validator
ItemFactProjection -X-> N01C Evaluator / Pressure / Enemy requirement / readiness
ItemFactProjection -X-> Board/Map/Battle/Scene/UI/MonoBehaviour/GameObject/Transform
```

## 14. Protected baseline

继承并冻结，不允许刷新：

```text
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
ItemSystemSnapshot.cs = 821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df
IF01 exact 11 = 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8
IF01 Canonical Signature = sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
N01C exact 16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical Signature = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
Enemy existing 89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 = fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 = 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A = a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
N01 exact 8 = 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
C02A-D1 exact 7 = c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0
N01A exact 8 = 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de
N01B exact 14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Exact scope 聚合：按 relativePath Ordinal 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 无 BOM，LF，并保留最终 LF。

当前工作区包含用户 Item visual/content dirty/untracked 内容与已验收未提交包。任务窗口必须记录 pre-existing status，只按第 13 节 package delta 判断；不得清理、覆盖、还原、暂存、认领或刷新任何既有基线。

## 15. 绝对禁止

```text
不修改任何已有文件
不修改 Item、IF01、N01C、Enemy requirement 或 readiness
不读取 Item 私有状态、资源、配置或 Scene 对象
不调用 Item Provider/Validator、IF01 Validator 或 N01C Evaluator
不作者化、读取或生成地图压力
不创建真实 EvaluationInput、requirement mapping 或 readiness result
不连接 Board/Map/Battle/Scene/Prefab/UI/RectTransform
不创建 Runtime Producer、事件订阅、计时器、状态机或 Battle tick
不伪造 I031 itemInstanceId
不把缺身份、Unknown 或空布局变成 zero/false/NotApplicable
不把 occupied-cell count/shape size/compactness 换算 BP 或 score
不输出正式 ReadinessBand、难度、胜负、奖励或掉落结论
不修改 RunFlow/SaveData/Reward/Chapter/Boss/BuildSettings
不实现 AuthoredLayoutPressureSourceAdapter01
不实现 N02/PA01/C02B/C02R1/C03
不修改 AGENTS/LOCKED/ROADMAP/CURRENT/Package Queue/CurrentRules
不 commit/tag/push
不自动启动下一包
```

## 16. Verifier 与报告验收

Offline 与 Unity 入口必须调用同一套断言：

```text
Adapter Schema/Version 与 3 个 Status 精确
第 4 节所有字段逐一精确映射
普通 placement IF01 join、I031 例外和状态优先级精确
Complete/Unknown/Invalid 与 N01C completeness/payload 组合精确
15/15 fixture 场景通过
空布局、I031-only、全 uncounted 布局保持 Complete
N01C validation-only BuildFacts 检查通过
N01C Evaluator symbol/call count = 0
压力、真实 requirement、readiness mapping = 0
BP/score/threshold 字段和算法 = 0
Ordinal 排序、防御性复制、Culture/重排确定性
Canonical Signature 格式、幂等、字段敏感性
现有文件修改 = 0
Protected baseline 前后一致
Package Leak Count = 0
GUID 冲突 = 0
尾随空白 = 0
git diff --check = PASS
```

报告：

```text
LayoutResilienceItemFactProjectionAdapterReport.md
  总结来源资格、字段映射、身份/I031、状态、确定性和无行为变化。
LayoutResilienceItemFactProjectionAdapterSpec.csv
  固定 Schema、状态、映射、错误码、payload 规则和签名。
LayoutResilienceItemFactProjectionAdapterFieldMatrix.csv
  sourceField / targetField / transform / eligibility / missingDisposition / owner。
LayoutResilienceItemFactProjectionAdapterFixtureCases.csv
  caseId / ordinaryCount / i031Count / bindingStatus / expectedStatus / completeness / rowCount / codes。
LayoutResilienceItemFactProjectionAdapterLeakCheckReport.md
  逐项列出私有 Item、I031 forgery、Pressure、Evaluator、requirement、readiness、Board/Map/Battle/UI、BP 与白名单扫描。
```

若 Unity Editor 锁阻 batch，不得关闭用户 Editor 或结束 Unity 进程，不得声称 Unity PASS；如实报告 BLOCKED 并保留 Offline 结果。本包不需要场景或玩家手测。

## 17. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-LayoutResilienceItemFactProjectionAdapter01

Guard receipts:
GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01
ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01

Result: DEV_COMPLETE / QA_RESULT
新增文件: 必须为 14
修改已有文件: 必须为 0
Schema / Status counts
Source -> BuildFacts field mapping
Ordinary binding / I031 disposition
Complete / Unknown / Invalid counts
Empty / I031-only / uncounted layout disposition
Fixture: 必须为 15/15
Canonical Signature / determinism
N01C validation-only result
N01C Evaluator calls: 必须为 0
Pressure / real requirement / readiness mappings: 必须为 0
C02 blocked rows: 仍为 32/32
Actual Unknown reduction: 必须为 0
Protected hashes
Offline verifier
Unity compile / verifier
Leak Count / GUID conflicts / trailing whitespace: 必须均为 0
git diff --check
Forbidden scope touched: 必须为 0
Pre-existing dirty preserved
HEAD unchanged
Commit / tag / push: none
Next package: NOT_STARTED
```

## 18. 后续包门禁

本包验收只解除 Item BuildFacts 投影前置，不产生布局结果，也不自动启动下一包：

```text
N01C-P1 LayoutResilienceItemFactProjectionAdapter01
-> 用户验收 + Guard / RepoOps 收口
-> AuthoredLayoutPressureSourceAdapter01（仍 HOLD；须 Enemy + Map + Cross-System 独立 Assignment）
-> LayoutResilience Requirement Migration（独立包）
-> Structural Readiness Channel Consumer（独立包）
```

`N02 / PA01 / C02B / C02R1 / C03` 继续 HOLD/BLOCKED。任何后续包均须新 Assignment。
