# JuNianLightingAndAdjacentRelay01 Report

- Package: `JuNianLightingAndAdjacentRelay01`
- Guard receipt target: `GUARD_PASS_JUNIANLIGHTINGANDADJACENTRELAY01`
- Verification: PASS
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`

## Implemented
- Pure rule layer: `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs`.
- Default direct range: `OrthogonalAdjacentLightingRangeRule`, offsets `(0,1);(1,0);(0,-1);(-1,0)`.
- coreCell判定：普通道具只检查 `coreCellWorld in litRangeCells`，不要求整件道具进入范围。
- 相邻判定：两件普通道具任意 `occupiedCells` 曼哈顿距离为 1。
- 链式遍历：直亮普通道具为 depth 0，逐层接亮；多来源先选最小 `litDepth`，同深度按稳定 `itemId`。
- ViewModel投影：`ItemSandboxLightingDetailProjection` 将结果写入 `ItemDetailViewModel` / `ItemLightingPreview` / `ItemBattleEffectPreview`，详情 UI 不读解析器内部对象。
- Sandbox灰盒：`ItemSandboxGridPlacementPreviewView` 提供 10 个可重复切换案例，格子显示阵眼、阵脉、聚念石、点亮范围、直亮、接亮、未亮。

## Test Layouts
- `S01_DirectSingle`: 聚念石直接点亮一个单格普通道具。
- `S02_MultiCoreInRange`: 多格道具只有 coreCellWorld 进入点亮范围，也应直亮。
- `S03_MultiPartOnlyUnlit`: 多格道具部分 occupiedCells 在范围内，但 coreCellWorld 未进入，保持未亮。
- `S04_RelayChain`: 聚念石直亮 A，A 接亮 B，B 接亮 C。
- `S05_DiagonalNoRelay`: 斜角相邻不算接亮。
- `S06_EmptyGapNoRelay`: 中间隔着空格不能跨格接亮。
- `S07_EyeNoBridge`: 中间隔着阵眼石，不能穿过阵眼接亮；点亮范围几何可包含阵眼。
- `S08_MultiPathMinDepth`: 同一道具可被两条同深度路径接亮时，按稳定 itemId 选择来源。
- `S09_NoSourceAllUnlit`: 没有聚念石时，普通道具即使相邻也全部未亮。
- `S10_OrdinaryRelayOnly`: 普通道具不能产生直接点亮范围，但已点亮后可以继续接亮相邻道具。

## Notes
- Rule constants checked: 5x5 board, eyeCell=(2,2), four arrayBonusCells, direct range is orthogonal adjacent only.
- Catalog source boundary passed: I031 聚念石 is the only isLightingSource=true item.
- Scenario samples checked: direct core-cell lighting, multi-cell core-only rule, relay chain, diagonal/gap/eye blockers, stable tie source, no-source all-unlit, ordinary relay-only.
- ItemDetailViewModel projection passed: relay-lit status, litByItemId, litDepth, and basicEffectActive are displayed through ViewModel fields.
- BuildSettings check passed: Item Sandbox scene is not registered.
- Scene scenario switch check passed: 10 repeatable greybox lighting cases are configured.
- Hierarchy naming check passed: no Chinese GameObject names were added.
- Source scope check completed: Item Sandbox lighting files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.

## Errors
- None

## Modified Files
- `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`
- `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemGridPlacementAndEyeRuleVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/JuNianLightingAndAdjacentRelayVerifier.cs`
- `Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md`
- `Docs/V0.4/Reports/JuNianLightingAndAdjacentRelaySpec.csv`
- `Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayLeakCheckReport.md`
