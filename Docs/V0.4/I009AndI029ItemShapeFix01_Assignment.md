# V0.4-I009AndI029ItemShapeFix01 Assignment

```text
TASK_START_V0.4_I009_AND_I029_ITEM_SHAPE_FIX01
GUARD_PASS_I009_I029_ITEM_SHAPE_FIX01
```

工程根目录：

```text
F:\Porject\TalismanBagBrawl
```

任务类型：

```text
Item 原型形状数据真源纠错
```

## 一、用户确认的权威结论

```text
I009「离火焚邪印」是单格道具。
I029「照煞镜」是单格道具。
```

两件道具的正式形状字段均为：

```text
shapeId = shape_single_1
defaultLocalCells / shapeCells = (0,0)
coreCellLocal = (0,0)
```

该形状属于 `baseItemId` 原型身份。五品阶与所有 Roll 实例必须继承，不允许按品阶、seed 或实例重新决定形状。

## 二、当前工作树事实

### I009

当前脏工作树已经存在未提交修正：

```text
ItemInnerDataCatalog：shape_single_1 / Single / core(0,0)
ItemBalanceProfile_I009：shape_single_1 · 1格 · 核心格(0,0)
ItemInnerDataCatalogSpec：单格
ItemCandidateDisplayContent30：单格
ItemInnerDataCatalogVerifier：已有 I009 单格与四方向旋转断言
```

这些修改属于进入本包前已经存在的用户/其他任务窗口工作。不得覆盖、回滚或重复重写；本包只需保护并纳入最终回归。

### I029

当前仍是错误数据：

```text
ItemInnerDataCatalog：shape_line2_v / Line2V / core(0,1)
ItemBalanceProfile_I029：shape_line2_v · 2格 · 核心格(0,1)
ItemInnerDataCatalogSpec：(0,0);(0,1) / core(0,1)
ItemCandidateDisplayContent30：shape_line2_v · 2格 · 核心格(0,1)
```

I029 是本包需要实际补齐的形状纠错对象。

## 三、修复目标

优先修正 I029 的唯一数据真源：

```text
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
```

将 I029 改为：

```csharp
CreateFaMenItem(
    "I029",
    "照煞镜",
    ItemFaMenTag.Taibai,
    ItemQiLeiTag.Jing,
    "shape_single_1",
    Single,
    new Vector2Int(0, 0),
    ...)
```

随后通过既有显式生成/验证流程同步所有实际依赖出口，包括但不限于：

```text
ItemBalanceProfile_I029 的 candidateDisplay.shapeDescription
ItemBalanceWorkbench Catalog 对 I029 的引用结果
Candidate 30/150 派生内容
ItemSystemSnapshot / Item Sandbox 棋盘占格
道具栏拖入后的占格
0/90/180/270 旋转结果
Rarity Foundation / Roll 实例继承的 shapeId
Item Detail 的 displayShapeName 与图档选择
相关 verifier 与派生报告
```

报告必须由真实验证/生成流程刷新；不得只手改 CSV 使其看起来通过。

## 四、详情页交付契约

现有详情页按 `displayShapeName / shapeId` 路由图片槽：

```text
single_1 / 单格 → DaojuSingleCellImage
其他多格形状 → DaojuMultiCellImage
```

修复后要求：

```text
I009 所有品阶继续使用 DaojuSingleCellImage
I029 所有品阶使用 DaojuSingleCellImage
不得为 I009 / I029 添加 UI 特判
不得通过修改图片尺寸或 UI 缩放掩盖错误形状数据
若 I029 当前没有可用正式 PNG，只验证槽位路由与 fallback，不得在本包伪造美术资源
```

## 五、允许修改

仅允许按实际需要最小修改：

```text
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/ItemBalanceProfile_I029.asset
与 I009/I029 形状专项断言直接相关的既有 verifier
由既有验证/生成流程刷新的 I009/I029 相关派生报告
为本包新增的最小 QA 报告（如确有必要）
```

若调查证明某个 Candidate/Projection 资产由显式生成器负责，优先运行或最小修正该生成流程，不得建立第二套形状真源。

## 六、禁止修改

```text
I009 已有未提交单格修正不得回滚或覆盖
I001-I008、I010-I028、I030 的形状
I031 聚念石规则
ItemDetailPanel 当前排版
DaojuSingleCellImage RectTransform
DaojuMultiCellImage RectTransform
现有 Image Shadow
zhenfaicon / qixingicon / daoju 身份与层级
用户手调字号、位置、尺寸、缩放、间距与 sibling order
棋盘尺寸与道具栏布局
品阶概率、词条概率、Build 资格、核心效果与养成规则
Battle、Reward、RunFlow、Inventory、SaveData、Boss 与正式掉落
ProjectSettings / BuildSettings
AGENTS.md
Docs/LOCKED/*
其他 unrelated dirty/untracked 文件
```

不得因为 I029 修复而批量推测其他道具。发现其他疑似形状错误只在回执中列清单，不修改。

## 七、必须新增/补齐的验证

I009 与 I029 均需独立断言：

```text
shapeId == shape_single_1
defaultLocalCells.Count == 1
defaultLocalCells[0] == (0,0)
coreCellLocal == (0,0)
rotation 0/90/180/270 均只占 anchor 对应的一个格子
五品阶版本的 shapeId 均为 shape_single_1
同原型不同实例不改变形状
Candidate shapeDescription 为 1格 / 核心格(0,0)
详情 displayShapeName 解析为单格
详情槽位路由选择 DaojuSingleCellImage，不启用 DaojuMultiCellImage
```

验证不得通过临时修改生产数据、反射绕过正常路径或在测试中主动制造成功状态。

## 八、回归要求

至少执行并汇报：

```text
Unity C# 编译 PASS，error CS = 0
ItemInnerDataCatalog verifier PASS
ItemSystemValidatorAndSnapshot PASS
ItemGridPlacementAndEyeRule / ShapePlacement 相关回归 PASS
ItemRarityInstanceFoundation PASS
ItemBalanceWorkbench / Candidate 150 回归 PASS
ItemInstanceRollEngine PASS
ItemInstanceProjectionContract PASS
Item Detail / ItemFullDetail Sandbox 相关回归 PASS
I031 普通生成排除保持 PASS
git diff --check 无本包新增错误
```

若历史 verifier 名称或入口已变化，使用当前等价的真实入口，并在回执中给出实际菜单、marker 和报告路径。

## 九、用户手测

开发窗口完成后必须给出真实 Unity 手测路径，至少覆盖：

```text
1. 在 Item Sandbox / 当前详情工作台创建或选择 I009。
2. 确认棋盘与道具栏中只占一个格子。
3. 旋转四个方向仍只占一个格子。
4. 确认详情使用 DaojuSingleCellImage。
5. 对 white/green/blue/purple/orange 分别创建或预览 I009，结果一致。
6. 对 I029 重复以上检查。
7. 确认其他道具形状与用户手调详情布局没有变化。
```

## 十、完成回执

完成后输出：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-I009AndI029ItemShapeFix01
Status:
ModifiedFiles:
I009ShapeResult:
I029ShapeResult:
FiveRarityInheritance:
RotationResult:
DetailSlotRouting:
UnityCompile:
VerifierMarkers:
RegressionResults:
GitDiffCheck:
DirtyWorktreeRisk:
UserHandTestPath:
RedlineTouched: NO
CommitTagPush: NO
```

预期本包 marker：

```text
I009_I029_ITEM_SHAPE_FIX01_PASS
```

不得 commit、tag、push、reset、切分支或清理工作树。
