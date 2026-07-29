# V0.4-ItemDetailQualifiedBuildTrackProjection01 Assignment

## 1. Guard Receipt

```text
Package: V0.4-ItemDetailQualifiedBuildTrackProjection01
PrimaryGuard: Item Guard
JointGuard: NOT_REQUIRED
AssignmentStatus: GUARD_APPROVED_FOR_DEVELOPMENT
TargetReceipt: GUARD_PASS_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01
Prerequisite: V0.4-ItemInstanceQualifiedBuildStateAdapter01 = GUARD_ACCEPTED / QA_PASS
UserHandtestAtAssignment: WAITING
DownstreamPackages: NOT_STARTED
```

本 Assignment 只放行 Communication / Projection 层开发。它不修改 Item Build 资格算法、Item 状态真源、详情 View、Scene 或 Prefab。

## 2. Guard Audit Conclusion

玩家当前看不到真实 Build 轨道，不是字体、图标或面板布局问题，也不是 Package A 没有生产数据。

当前真实断点为：

```text
BuildGridInteractionPreviewController
  -> 只传 baseItemId + placementId + ItemSystemSnapshot
  -> ItemSystemBattleSandboxItemDetailAdapter.Show(...)
  -> 道具栏 placementId 为空时退化为 CatalogPreview
  -> ItemDetailProjectionComposer 无法按 placement 找到 Build 行
  -> 显示“当前道具没有可统计的Build轨道”
```

Package A 已经在 `ItemInstanceQualifiedBuildStateSnapshot.v1` 中提供普通实例的权威只读 Build 资格与实时状态。本包应补齐：

```text
baseItemId + itemInstanceId + placementId
+ latest ItemSystemSnapshot.v2
+ latest ItemInstanceQualifiedBuildStateSnapshot.v1
-> Item Detail qualified Build projector
-> ItemDetailViewModel
-> existing ItemDetailPanelView.Bind()
```

第一缺失层：`Communication / Projection`。

不需要修改：

- Package A Producer / Assembler / Contract；
- `ItemSystemSnapshot.v2`；
- `IF01`；
- `ItemBuildSynergyRules`；
- `ItemDetailProjectionComposer`；
- `ItemDetailPanelView` / `ItemDetailSectionView`；
- Scene / Prefab。

因此本包不需要 Cross-System、Enemy 或 Capability Algorithm 联合 Guard。

## 3. Package Goal

将 `ItemSystemBattleSandboxBoardAuthority.CurrentQualifiedBuildState` 只读投影到 Item Detail，使 BattleSandbox 中的道具栏实例和棋盘实例均能显示真实的：

- `BuildQualification`；
- 法门 / 器型可参与轨道；
- Inventory / Board 位置语义；
- `isLit`；
- `sourceIsCountedFact`；
- `qualifiedIsCountedFact`；
- 当前轨道数量；
- Package A 已计算的当前阶段和下一阶段；
- Unknown、Known Zero、None、NotApplicable 的区别。

完整字段链必须达到：

```text
Generated Item Projection
-> IF01
-> ItemSystemSnapshot.v2
-> ItemInstanceQualifiedBuildStateSnapshot.v1
-> Item Detail qualified Build projector
-> ItemDetailViewModel
-> existing ItemDetailPanelView
-> BattleSandbox real runtime sample
-> user handtest
```

## 4. Authoritative Inputs

### 4.1 Item identity

普通道具详情必须显式消费：

```text
baseItemId
itemInstanceId
placementId (Inventory 可为空；Board 必须存在)
```

联结规则：

- 只能使用 `itemInstanceId + baseItemId` 查找 Package A 实例行；
- Board 行还必须精确匹配 `placementId`；
- 不得假设 `itemInstanceId == placementId`；
- 不得只凭 `baseItemId`、中文名、图标、品阶或 UI 节点猜实例；
- 任何错配、孤儿或过期身份必须拒绝，不得继续显示上一件道具的数据。

### 4.2 Runtime state

必须使用同一次详情打开 / 刷新时读取到的：

```text
ItemSystemBattleSandboxBoardAuthority.CurrentSnapshot
ItemSystemBattleSandboxBoardAuthority.CurrentQualifiedBuildState
```

要求：

- `CurrentQualifiedBuildState.SchemaVersion` 必须为 Package A 已接受版本；
- Package A snapshot 必须通过其既有验证；
- `sourceItemSystemCanonicalSignature` 必须与本次 `ItemSystemSnapshot.v2` 的 Canonical Signature 一致；
- 不得混用不同帧、不同提交或不同布局版本的两份 Snapshot；
- adapter 不得缓存并复用已过期的 qualified snapshot。

### 4.3 Build stages

必须直接消费 Package A Track 行：

```text
qualifiedItemCount
maxPieceCount
activeStagePieceCount
nextStagePieceCount
```

UI、Composer、Projector、Adapter 均不得重新计算 2/4/6 阈值。展示 2/4/6 或 2/4 的固定段落，只是呈现既有轨道阶段，不得形成第二套 Build evaluator。

## 5. Projection Semantics

### 5.1 Inventory ordinary instance

- 显示实例真实 `BuildQualification`；
- 显示该实例可加入的法门 / 器型轨道；
- 实例自己的实时摆放贡献显示为“未入阵”或 NotApplicable；
- 不得把未入阵写成 `false`、`0`、`None` 或“无资格”；
- 如果 Package A Track 完整，可只读显示当前棋盘的全局轨道进度，并明确该实例尚未入阵；
- 如果完整名册或权威状态不完整，必须显示安全 Unknown，不得猜 Inventory。

### 5.2 Board ordinary instance

- 精确匹配 `itemInstanceId + placementId + baseItemId`；
- 显示 Package A 的 `isLit`、`sourceIsCountedFact`、`qualifiedIsCountedFact`；
- 显示资格允许的法门 / 器型轨道；
- 显示轨道当前数量、上限、当前阶段、下一阶段；
- 移动、点亮变化和返回道具栏后重新打开 / 刷新必须读取最新状态。

### 5.3 BuildQualification.None

- 明确显示该普通实例“不具备 Build 资格”；
- Known Zero 保持真实 0，不得转为 Unknown；
- 即使道具已点亮，也不得产生法门或器型贡献。

### 5.4 Unknown / Invalid

- 玩家安全文案使用“当前Build状态暂不可用”或等价稳定文案；
- Unknown 的数值字段保持缺失，不得转换为 0；
- Invalid identity / stale snapshot 必须关闭或保持不显示本次详情，并清空 adapter 的上一份投影；
- 不得用 Catalog fallback、默认轨道或上一件道具的数据掩盖错误。

### 5.5 I031

- 不生成普通 `itemInstanceId`；
- 不进入 Package A 普通实例行；
- 不生成普通 `BuildQualification`；
- 不进入法门 / 器型 Build 轨道；
- 继续使用既有 I031 系统道具详情与供能状态展示；
- 本包不得修改 I031 placement、lighting 或 inventory 合同。

## 6. ViewModel Boundary

允许在 `ItemDetailViewModel` 中增加一个明确的只读 Qualified Build 详情结构，至少能无损表达：

```text
status / completeness
itemInstanceId
baseItemId
placementId
location
buildQualification
isLit fact
sourceIsCountedFact
qualifiedIsCountedFact
eligibleFaMenBuildId
eligibleQiLeiBuildId
qualifiedItemCount (nullable)
maxPieceCount (nullable)
activeStagePieceCount (nullable)
nextStagePieceCount (nullable)
track preview rows
diagnostic / safe unavailable reason
```

要求：

- 新结构及嵌套集合必须深克隆；
- Unknown / NotApplicable / Known False / Known True 必须显式区分；
- 旧 `statusFlags.countedInBuild` 或旧 `buildPreview.countedInBuild` 不得成为本包权威字段；
- 不得为了兼容旧 bool 把 Unknown 转成 false；
- ViewModel 只承载投影结果，不成为状态 Owner。

## 7. Presentation Compatibility

本包不得修改 View。Projector 输出必须兼容现有：

```text
ItemDetailPanelView
ItemDetailSectionView
stateKey = famenBuild / qileiBuild
```

要求：

- 保留现有候选详情中的 Build 效果描述与图标身份；
- 不硬编码新的技能效果内容；
- 法门轨道显示 2/4/6 阶段；
- 器型轨道显示 2/4 阶段；
- 阶段激活视觉必须来自 Package A 已计算阶段；
- 未具备某类资格时，不得虚构该类轨道；
- 现有玩家可见段落、字体、图标、布局和 RectTransform 不得变化。

## 8. Allowed File Whitelist

### 8.1 Existing files allowed for targeted modification

1. `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs`
3. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`

### 8.2 New files allowed

4. `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs`
5. `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs.meta`
6. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDetailQualifiedBuildTrackProjectionVerifier.cs`
7. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDetailQualifiedBuildTrackProjectionVerifier.cs.meta`
8. `Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionReport.md`
9. `Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionSpec.csv`
10. `Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionFieldLineage.csv`
11. `Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionLeakCheckReport.md`
12. `Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionManualTest.md`

包体精确范围：

```text
Existing modified: 3
New files: 9
Package files total: 12
```

Unity 只允许为上述两个新 `.cs` 生成对应 `.meta`。发现必须修改其他文件时，立即停止并回 Item Guard，不得自行扩大白名单。

## 9. Required Implementation Shape

### 9.1 Projector

新增纯只读 `ItemDetailQualifiedBuildTrackProjector`：

```text
existing ItemDetailProjectionComposer result
+ exact item identity
+ ItemSystemSnapshot.v2
+ ItemInstanceQualifiedBuildStateSnapshot.v1
-> final ItemDetailViewModel
```

Projector：

- 不持有场景对象；
- 不成为 Runtime Producer；
- 不改变 Package A Snapshot；
- 不写棋盘、道具栏或 Item 状态；
- 不重新计算资格、计数或阶段；
- 可读取 `ItemInnerDataCatalog` 作为显示名册，但不得用它进行实例身份联结；
- 不得读取 Item 私有运行状态。

### 9.2 Adapter

BattleSandbox 正式运行路径必须使用严格入口，参数至少包含：

```text
baseItemId
itemInstanceId
placementId
latest ItemSystemSnapshot.v2
latest ItemInstanceQualifiedBuildStateSnapshot.v1
```

如保留旧三参数 `Show(baseItemId, placementId, snapshot)`，它只能作为编译兼容的 fail-closed 入口：

- 普通实例缺少 qualified context 时返回 `DETAIL_QUALIFIED_CONTEXT_REQUIRED`；
- 不得继续走 Catalog fallback；
- 不得显示默认 Build 轨道；
- 不得成为 Controller 的生产路径。

### 9.3 Controller

打开和刷新详情时必须从同一真实 `ItemSystemBattleSandboxViewRow` 与同一 BoardAuthority 状态读取：

```text
row.ItemId
row.ItemInstanceId
resolved placementId
boardAuthority.CurrentSnapshot
boardAuthority.CurrentQualifiedBuildState
```

关闭、选择变化、身份错误或刷新失败时必须清除旧投影，避免 A 道具的 Build 信息残留到 B 道具。

## 10. Forbidden Modifications

禁止修改或生成：

- `ItemDetailProjectionComposer.cs`；
- `ItemDetailInstanceDataAdapter.cs`；
- `ItemBalanceCandidateDetailSandboxAdapter.cs`；
- Package A Contract / Assembler / Validation / verifier；
- `ItemSystemSnapshot.cs`；
- IF01；
- ItemInstanceProjection / Roll；
- `ItemBuildSynergyRules.cs`；
- BoardAuthority / ViewProjection；
- `ItemDetailPanelView.cs`；
- `ItemDetailSectionView.cs`；
- Scene / Prefab / UI / Font / Sprite / Icon / RectTransform；
- Item Catalog、Candidate Profiles、算法、概率、词条、核心效果或品阶；
- Enemy / Capability / P1 / P6；
- 正式 Battle、RunFlow、SaveData、Reward、Chapter、Boss；
- BuildSettings；
- `AGENTS.md` / `Docs/LOCKED/*`；
- Package Queue / Guard memory。

不得：

- 运行 Scene Builder；
- 运行会重写 ItemDetailPanel、Scene、Prefab 或资源的旧 Builder / Regression；
- 用 Fixture、硬编码轨道、默认文案或虚构进度代替真实路径；
- commit / tag / push / reset / rollback；
- 自动启动 ItemDetail Base Prefab Migration、Consumer Migration、UnifiedBattlePage 或正式 Battle 接入。

## 11. Task-Start SHA-256

开发窗口必须以任务开始时磁盘内容为基线，不得用 Git HEAD 覆盖、清理或恢复当前 dirty/untracked 文件。

### 11.1 Allowed existing files

| File | Task-start SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs` | `e5a9aaab7557fd178cf0ab6d2cf06f2919d78c8018f93327b8b3a92c24476112` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs` | `4033de527cd7f4f0a3e3226c810e20b7728234a01ff4c8e38dc41483d30b83db` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `a0e4760481419939df9de7e9db9c4812f161075d326d81ec172c39107da7e7da` |

### 11.2 Protected source baselines

| Protected file | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs` | `f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs` | `798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs` | `95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827` |
| `Assets/_Game/Scripts/TalismanBag/Items/Instances/ItemInstancePlacementBindingContract.cs` | `335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` |
| `Assets/_Game/Scripts/TalismanBag/Items/Instances/ItemInstanceProjectionContract.cs` | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` |
| `Assets/_Game/Scripts/TalismanBag/Items/Instances/ItemInstanceRollContract.cs` | `19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs` | `64b9d3e2e407dc014695b4f466c4c82803384b9b37dc919c435bd181e2c13910` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs` | `e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs` | `ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs` | `5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailInstanceDataAdapter.cs` | `0eaae3f90dd585f77d8f6d3f5426916ff6ff5c7cc1727f299974825228538806` |
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs` | `65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs` | `957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d` |
| `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs` | `606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9` |
| `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset` | `5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45` |

### 11.3 Protected asset baselines

| Protected asset | SHA-256 |
|---|---|
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Profiles30 聚合保护 SHA-256：

```text
a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0
```

聚合方式必须沿用 Package A：对 30 个 Profile 按文件名 Ordinal 排序，每行写入 `relativePath|lowercaseFileSha256\n` 后计算 SHA-256。

## 12. Real Sample Matrix

| Item | Instance | Qualification | Inventory expectation | Board lit expectation |
|---|---|---|---|---|
| I001 | `wb_i001_orange_404310001` | QiLeiOnly | 显示 `qilei:fu` 资格；自身未入阵 | `qilei:fu = 1/4`；不显示法门贡献 |
| I004 | `wb_i004_orange_404310004` | Dual | 显示 `famen:zhenlei` + `qilei:ling` | 两条权威轨道分别增加 1 |
| I006 | `wb_i006_orange_404310006` | None | 明确不具备 Build 资格 | 点亮后仍为 Known Zero，不贡献轨道 |
| I009 | `wb_i009_orange_404310009` | FaMenOnly | 显示 `famen:lihuo` 资格 | `famen:lihuo = 1/6`；不显示器型贡献 |
| I031 | none | NotApplicable | 系统道具详情；无普通实例资格 | 不进入普通 Build 轨道 |

每个普通样本必须验证：

```text
Inventory
-> Board unlit
-> Board lit
-> Move and refresh
-> ReturnToInventory
-> stale/mismatched identity rejection
```

如果真实名册、Package A 快照或权威轨道输出与表中预期不一致，不得篡改数据来满足表格；必须停止并向 Item Guard 报告真实证据。

## 13. Verifier

新增 Unity batch 入口：

```text
TalismanBag.EditorTools.BuildSandbox.ItemDetailQualifiedBuildTrackProjectionVerifier.VerifyStaticBatch
```

推荐命令：

```text
F:\2022.3.50f1c1\Editor\Unity.exe -batchmode -quit -projectPath F:\Porject\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.BuildSandbox.ItemDetailQualifiedBuildTrackProjectionVerifier.VerifyStaticBatch
```

Verifier 必须实际执行并分别报告：

```text
COMPONENT_FIXTURE_PASS
REAL_RUNTIME_PATH_PASS
USER_HANDTEST_WAITING
```

Fixture 通过不得代替真实 BattleSandbox runtime assembly 路径。

## 14. Required Verification Cases

1. I001 / I004 / I006 / I009 / I031 真实样本均按第 12 节通过。
2. Inventory 与 Board 投影有明确语义差异。
3. Board 未点亮、点亮、移动、收回后详情刷新。
4. 2/4/6 与 2/4 阶段直接读取 Package A 结果。
5. Unknown 与 Known Zero 的 ViewModel、正文和 Canonical 明确不同。
6. stale `itemInstanceId` 拒绝。
7. stale `placementId` 拒绝。
8. `baseItemId` mismatch 拒绝。
9. Package A / ItemSystem Canonical mismatch 拒绝。
10. 失败后 Panel 不显示旧详情，adapter 旧模型被清空。
11. 连续点击不同道具无数据残留。
12. 普通生产路径不调用旧三参数 Catalog fallback。
13. I031 不产生普通 Build row。
14. Package A Canonical Signature 在本包前后不变。
15. 第 11 节全部 protected SHA 前后一致。
16. Profiles30 聚合 SHA 前后一致。
17. Unity compile PASS。
18. Unity verifier PASS。
19. LeakCheck PASS。
20. Package-scoped `git diff --check` PASS。
21. 不新增 Scene / Prefab / UI / asset dirty。
22. 报告明确保留 `USER_HANDTEST_WAITING`，不得写用户已通过。

## 15. Reports

必须由 verifier 生成或刷新：

```text
Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionReport.md
Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionSpec.csv
Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionFieldLineage.csv
Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionLeakCheckReport.md
Docs/V0.4/Reports/ItemDetailQualifiedBuildTrackProjectionManualTest.md
```

Field Lineage 报告至少包含：

```text
fieldKey
playerLabel
dataOwner
runtimeProducer
snapshotField
projector
viewModelField
viewSection
realSampleId
sourceValue/rowCount
snapshotValue/rowCount
viewModelValue/rowCount
componentFixtureResult
realRuntimeResult
userHandtestResult
firstMissingLayer
```

## 16. User Handtest Gate

开发窗口完成静态和 Unity 验证后，必须停在：

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_RUNTIME_PATH_PASS
WAITING_USER_HANDTEST
```

用户手测场景：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

手测步骤：

1. Play 后点击道具栏 I001：应显示器型 `符` Build 资格，不再显示旧 fallback。
2. 将 I001 放到未点亮位置：显示未点亮、source / qualified 均不贡献。
3. 合法放置 I031 使 I001 点亮：显示 `qilei:fu = 1/4`。
4. 点击 I004：显示法门震雷与器型令两条资格；点亮上阵后两条轨道均反映该实例贡献。
5. 点击 I006：Inventory 与 Board 都明确无 Build 资格；点亮后仍不贡献。
6. 点击 I009：只显示离火法门资格；点亮上阵后显示 `famen:lihuo = 1/6`。
7. 将普通道具移位、收回道具栏并重新打开：详情立即跟随最新状态。
8. 点击 I031：只显示系统道具详情，不出现普通法门 / 器型资格。
9. 连续点击多件道具：不得残留上一件道具的轨道、进度或阶段。

用户明确通过前不得回 `GUARD_PASS_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01`，只能同步“等待用户手测”。

## 17. Delivery Format

开发窗口完成后只向 Item Guard + RepoOps 同步：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
Package: V0.4-ItemDetailQualifiedBuildTrackProjection01
ComponentFixture: PASS/FAIL
RealRuntimePath: PASS/FAIL
UserHandtest: WAITING
PackageARegression: PASS/FAIL
ProtectedHashes: PASS/FAIL
LeakCheck: PASS/FAIL
PackageScopedDiffCheck: PASS/FAIL
DownstreamPackages: NOT_STARTED
```

必须附：

- 精确修改文件；
- Unity 命令与退出码；
- 五份报告路径；
- 真实样本结果；
- protected SHA 前后对照；
- 禁区触碰情况；
- 用户手测步骤。

## 18. Guard Final Decision

```text
ITEM_GUARD_PASS_ASSIGNMENT_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01
TargetReceipt: GUARD_PASS_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01
JointGuard: NOT_REQUIRED
PackageDevelopment: NOT_STARTED_BY_GUARD
PrefabMigration: BLOCKED_UNTIL_USER_HANDTEST_PASS
UnifiedBattlePage: NOT_STARTED
```
