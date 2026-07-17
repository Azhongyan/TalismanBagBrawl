# V0.4-ItemSandboxDetailUi01 Assignment

Guard 回执：

```text
GUARD_PASS_ITEMSANDBOX_DETAILUI01
```

Guard 收口状态（2026-07-09）：

```text
Status: PASS
Scope: Item Sandbox scene + greybox item detail UI + ViewModel / preview interfaces
Reports:
  Docs/V0.4/Reports/ItemSandboxDetailUiReport.md
  Docs/V0.4/Reports/ItemDetailViewModelSpec.csv
  Docs/V0.4/Reports/ItemSandboxDetailUiLeakCheckReport.md
Verification:
  Unity scene build batch: PASS
  Unity final verifier batch: PASS
  Leak check: PASS
Boundary:
  未接正式点亮、Build、开窍、掉落、升级、存档、BattleResolver、Battle Bridge。
  未修改 V0.3 RunFlow、V0.4 Battle system、UnifiedBattlePage、Boss、Reward、SaveData、BuildSettings。
  进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

## 1. 包定位

```text
Item 系统独立沙盒 / 道具详情页 UI 首包
```

本包目标是新建一个独立 Item Sandbox 场景，用灰盒道具验证道具详情页 UI、选择交互和 ViewModel 接口。

本包不是完整 Item 内核，不做真实点亮、接亮、Build、开窍、掉落、养成、战斗结算。

## 2. 上游规则

必须读取并遵守：

```text
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/LOCKED/DO_NOT_TOUCH.md
Docs/LOCKED/CURRENT_VERSION_SCOPE_LOCK.md
```

当前 Item Guard 口径：

```text
Item 系统先在独立 Item Sandbox 中验证。
5×5 中心阵眼石不可覆盖。
阵眼石上下左右四格是固定阵脉点 / 阵位点，用于加成已点亮道具。
聚念石是第一版唯一点亮来源。
相邻接亮只做上下左右。
核心效果靠养成开窍解锁。
本包只做详情页 UI 外壳和接口，不做这些规则的真实计算。
```

## 3. 必须做

### 3.1 新建独立 Item Sandbox 场景

建议新场景：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

场景用途：

```text
独立验证 Item 详情页 UI。
不接正式战斗。
不接 V0.3 Flow。
不接 Bridge Page。
不进入 BuildSettings。
```

场景内至少包含：

```text
ItemSandboxRoot
ItemNameList / 灰盒道具列表
ItemDetailPanel / 道具详情页
DevStubProvider / 沙盒假数据来源
```

Hierarchy / GameObject / 文件名 / 脚本 key 必须使用英文稳定命名。中文只进入玩家显示文案。

### 3.2 灰盒道具列表

沙盒内先放灰盒道具，只写名字。

要求：

```text
道具列表可点击选择。
每个道具块只显示中文道具名。
不做正式 icon。
不做正式美术。
不做拖拽。
不做占格摆放。
不做真实 item rarity / affix roll。
```

建议首批灰盒名：

```text
聚念石
火符
震雷符
护身符
净水符
桃木剑
醒符香
炉芯石
照煞镜
中岳镇守章
```

这些名称只用于 UI 灰盒预览，不代表正式 31 件清单已经落定。

### 3.3 道具详情页 UI

详情页必须按单列竖排阅读。

整体场景可以是左侧灰盒列表、右侧详情栏；但详情栏本身不得做复杂左右双栏。

详情栏字段顺序：

```text
1. 道具名
2. 品阶 · 法门/器类 · 占格
3. 物品强度
4. 基础属性
5. 触发说明
6. 点亮 / 接亮 / 开窍状态
7. 固定词条
8. 随机词条
9. 分割线
10. 道痕 / 橙色专属，如有
11. 基础效果
12. 核心效果
13. 法门 / Build
14. 器类 / Build
15. 分割线
16. 推荐摆放
17. 旧物记
```

当前包可用 stub 文案填充，但 UI 结构必须预留完整字段。

低阶 / 空字段允许隐藏，不要显示空标题。

### 3.4 详情页状态表达

必须能用假数据展示以下状态：

```text
已点亮
未点亮
未开窍
已开窍
阵位加成未生效
阵位加成已生效
```

文案示例：

```text
已点亮：基础效果生效
未点亮：无法发动，也不计入 Build
核心效果：Lv.10 开窍后解锁
阵位加成：已点亮后生效
```

注意：

```text
聚念石不直接解锁核心效果。
已点亮 + 已开窍，核心效果才发动。
```

### 3.5 预留接口

本包必须建立 UI 绑定接口，避免后续 UI 直接读 ItemDefinition 或正式战斗对象。

建议最小接口 / 数据结构：

```text
ItemDetailViewModel
ItemDetailStatLine
ItemDetailTextLine
ItemDetailBuildPreview
ItemDetailStatusFlags
IItemDetailViewModelProvider
```

`ItemDetailViewModel` 建议字段：

```text
itemId
displayItemName
displayRarityName
displayFaMenName
displayQiLeiName
displayShapeName
displayItemPower
displayPrimaryStats[]
displayTriggerText
displayLightingStatusText
displayAwakeningStatusText
displayArrayBonusStatusText
displayFixedAffixes[]
displayRandomAffixes[]
displayOrangeAffix
displayBasicEffects[]
displayCoreEffects[]
displayFaMenBuilds[]
displayQiLeiBuilds[]
displayPlacementTips[]
displayFlavorText
iconPlaceholderKey
rarityColorKey
```

本包可以使用 `DevStubProvider` 返回假数据，但 UI 必须只绑定 ViewModel。

### 3.6 必须预留但不得实现的未来接口

本包不做真实点亮、Build、开窍、掉落、养成、战斗，但必须预留未来系统接入点。

允许新增只读接口 / DTO：

```text
IItemLightingPreviewProvider
IItemBuildPreviewProvider
IItemAwakeningPreviewProvider
IItemAcquirePreviewProvider
IItemUpgradePreviewProvider
IItemBattleEffectPreviewProvider
```

这些接口当前只能由沙盒 stub 数据实现，不得接正式系统。

建议最小职责：

```text
IItemLightingPreviewProvider
  提供已点亮 / 未点亮 / 直接点亮 / 接亮 / 阵位加成状态的预览文本和标记。

IItemBuildPreviewProvider
  提供法门 Build、器类 Build 的显示预览，不做真实统计。

IItemAwakeningPreviewProvider
  提供未开窍 / 已开窍 / Lv.10-Lv.40 节点预览，不接真实养成。

IItemAcquirePreviewProvider
  提供来源提示 / sourceHint 占位，不接正式掉落或奖励。

IItemUpgradePreviewProvider
  提供升级、品阶、词条槽占位显示，不接正式升级或 SaveData。

IItemBattleEffectPreviewProvider
  提供基础效果 / 核心效果 / 战斗生效状态的只读预览，不接 BattleResolver。
```

接口边界：

```text
可以定义字段。
可以返回 stub 假数据。
可以让 UI 显示未来状态。
不可以实现真实规则。
不可以读写正式存档。
不可以接正式掉落、升级、战斗、Boss、Reward。
```

`ItemDetailViewModel` 必须能承载这些接口的结果，但本包只用 stub 填充。

## 4. 允许新增 / 修改范围

允许新增：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
Assets/_Game/Scripts/TalismanBag/ItemSandbox/**
Assets/_Game/Scripts/TalismanBag/Items/Detail/**
Assets/_Game/Configs/ItemSandbox/**
Docs/V0.4/Reports/**
```

允许输出报告：

```text
Docs/V0.4/Reports/ItemSandboxDetailUiReport.md
Docs/V0.4/Reports/ItemDetailViewModelSpec.csv
Docs/V0.4/Reports/ItemSandboxDetailUiLeakCheckReport.md
```

如果现有目录结构更适合放在 `BuildSandbox` 下，任务窗口必须在报告中说明原因，并保证不混入 Battle 逻辑。

## 5. 禁止范围

本包禁止：

```text
实现正式 31 件 ItemInnerDataCatalog
实现真实点亮 / 接亮计算
实现真实 BuildSynergyResolver
实现真实开窍 / 养成系统
实现正式掉落 / 获得
实现正式升级 / 洗词条
实现正式存档
实现 BattleResolver
实现 Battle Bridge
修改 V0.3 RunFlow
修改 V0.4 Battle system
修改 UnifiedBattlePage
修改 Boss / Reward / SaveData
修改正式 V02 / V03 场景
修改现有 V04 BattleSandboxPreview 场景
修改 BuildSettings
使用中文 Hierarchy / GameObject 名称
玩家 UI 显示中英文双语
commit / tag / push
```

本包不做拖拽、不做真实摆放、不做格子占用验证。

允许定义未来接口和 DTO，但禁止在本包实现真实业务逻辑。

## 6. UI 规则

详情页视觉遵守：

```text
旧纸符器鉴定卡气质。
字段标题使用中性标题色。
道具名与关键数值可跟随品阶色。
不要整段高亮。
不要把“核心效果”标题染成品阶色。
不要把“道痕”标题做成橙金大标题。
不要做西幻装备栏。
不要做赛博芯片面板。
```

灰盒阶段可以简化视觉，但必须保证：

```text
可读。
不重叠。
长文本可滚动。
字段顺序正确。
选择道具后详情页刷新明确。
```

## 7. 报告要求

必须输出：

```text
Docs/V0.4/Reports/ItemSandboxDetailUiReport.md
Docs/V0.4/Reports/ItemDetailViewModelSpec.csv
Docs/V0.4/Reports/ItemSandboxDetailUiLeakCheckReport.md
```

报告至少说明：

```text
新场景路径
灰盒道具数量
详情页字段覆盖情况
ViewModel 字段清单
预留接口清单
哪些接口当前仅 stub
是否只绑定 ViewModel
是否接入正式战斗
是否修改正式场景
是否修改 BuildSettings
是否存在中文 GameObject 名称
是否有玩家侧中英文双语
是否触碰 SaveData / Reward / Boss / RunFlow
```

## 8. 验收标准

必须满足：

```text
1. C# 编译通过。
2. Item Sandbox 新场景可打开。
3. 场景内有灰盒道具列表，且道具块只显示名字。
4. 点击不同灰盒道具能刷新详情页。
5. 详情页显示道具名、品阶/法门/器类/占格、物品强度、基础属性、状态、效果、Build、推荐摆放、旧物记等区域。
6. UI 通过 ItemDetailViewModel 或 Provider 绑定，不直接读正式战斗对象。
7. 能展示已点亮 / 未点亮 / 未开窍 / 已开窍 / 阵位加成状态的假数据。
8. 预留 Lighting / Build / Awakening / Acquire / Upgrade / BattleEffect Preview 接口，且当前仅 stub。
9. 未实现真实点亮、Build、开窍、掉落、升级、存档。
10. 未修改 V0.3 / V0.4 正式流程和正式场景。
11. 未修改 BuildSettings。
12. 报告与 LeakCheck 输出完成。
```

## 9. 用户手测建议

用户只需要验证：

```text
打开 Scene_TalismanBag_V04_ItemSandbox。
点击不同灰盒道具。
确认右侧 / 主详情栏能刷新。
确认灰盒道具只显示名字。
确认详情页字段顺序和文案方向符合 Item 视觉体系。
确认没有进入战斗、没有拖拽、没有正式掉落 / 养成。
```

## 10. 通过后下一包

通过后建议进入：

```text
ItemInnerDataCatalog01
```

或根据用户选择，先进入：

```text
ItemGridPlacementAndEyeRule01
```
