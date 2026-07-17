# Item System Guard 当前收口规则

生成日期：2026-07-09
负责人：Item System Guard
范围：V0.4 Item Sandbox 与后续 Item 系统任务拆包

本文只记录 Item Guard 当前有效口径，不代表开发代码、改场景、改 RunFlow、改 SaveData、改 Reward、改 Boss、接 Battle Bridge、接 UnifiedBattlePage 或调整 V0.3 流程。

## 一、当前总定位

Item 系统先在独立 Item Sandbox 中开发和验证。

其他系统未来可以消费稳定的 Item Snapshot、Item Contract 或 Item Detail ViewModel，但不得各自重写 Item 规则。

```text
Item Sandbox
→ Item Runtime / Item Contract / Item Detail Projection
→ V0.4 Battle、V0.3 Flow、Bridge Page、后续获得/养成系统只读消费
```

核心原则：

```text
Item 系统拥有道具事实。
其他系统只消费道具快照。
```

## 二、V0.4 第一版有效规则

### 1. 阵眼

阵眼石是 5×5 背包中宫核心，类似王者荣耀水晶，是需要保护的核心标志物。

```text
eyeCell 固定存在。
eyeCell 不可放置。
eyeCell 不可被任何 occupiedCells 覆盖。
eyeCell 不可移动。
eyeCell 不算道具。
```

任何道具覆盖 `eyeCell` 都是非法摆放。

### 2. 阵位点

V0.4 第一版固定 4 个阵脉 / 阵位点，位于阵眼石上下左右四个正交相邻格。

标准 5×5 坐标示例：

```text
eyeCell = (2,2)
arrayBonusCells =
  (2,1)
  (2,3)
  (1,2)
  (3,2)
```

通用规则：

```text
如果 eyeCell = (x,y)
arrayBonusCells =
  (x,y-1)
  (x,y+1)
  (x-1,y)
  (x+1,y)
```

玩家前台可称为：

```text
阵脉
阵脉点
阵位点
```

技术字段统一使用：

```text
arrayBonusCells
```

```text
arrayBonusCells 不是阵眼。
arrayBonusCells 可以被道具占用。
arrayBonusCells 不点亮道具。
arrayBonusCells 不传导点亮。
arrayBonusCells 只强化已点亮道具。
```

生效规则：

```text
isLit = true
+ isOnArrayBonusCell = true
→ isArrayBonusActive = true
```

未点亮道具即使占用阵位点，也不获得阵位加成。

### 3. 聚念石

聚念石是 V0.4 第一版唯一点亮来源。

```text
聚念石接入阵眼。
接入成功后产生 litRangeCells。
道具 coreCell 落在 litRangeCells 内时，直接点亮。
```

玩家外显优先使用：

```text
聚念石
点亮范围
已点亮 / 未点亮
```

不要把旧的“灵力 / 魔力 / 供能电池 / 复杂阵脉传导”作为正式玩家口径。

### 4. coreCell

每件道具只检查自己的核心格是否被直接点亮，不要求整件道具都在点亮范围内。

```text
符 → 符胆格
印 → 印心格
令 → 令心格
镜 → 镜心格
法 → 器心格
```

技术字段统一为：

```text
coreCell
```

直接点亮规则：

```text
coreCell in litRangeCells
→ isDirectLit = true
→ isLit = true
```

### 5. 相邻接亮

V0.4 第一版接亮只做上下左右相邻。

```text
A 已点亮
B 与 A 上下左右相邻
→ B 被接亮
```

允许链式接亮：

```text
聚念石 → A → B → C
```

第一版不算斜角。

### 6. 第一版不做的连接概念

以下概念当前全部后置：

```text
复杂阵脉传导
入阵 / 挂脉
同脉接法
指向接法
镜照接法
远程供能
供能衰减
多种供能源
普通道具供能
聚念石直接解锁核心效果
```

## 三、已点亮 / 未点亮边界

未点亮道具：

```text
不发动基础效果
不计入法门 Build
不计入器类 Build
不触发成法
不触发接亮
不吃阵位点加成
不发动已开窍核心效果
```

已点亮道具：

```text
发动基础效果
计入法门 Build
计入器类 Build
可以继续接亮相邻道具
可以吃阵位点加成
可以发动已开窍核心效果
```

## 四、核心效果与开窍

核心效果可以理解为道具本身技能，但它不由聚念石直接解锁。

```text
点亮 = 道具在战斗中有效。
开窍 = 道具养成解锁核心效果。
```

核心效果发动规则：

```text
isLit = true
+ coreEffectUnlocked = true
→ 对应核心效果可以发动
```

只点亮、未开窍：

```text
只发动基础效果。
核心效果不发动。
```

已开窍、未点亮：

```text
核心效果不发动。
道具不计入 Build。
```

规划开窍节点：

```text
Lv.10 → 普通核心效果 1
Lv.20 → 普通核心效果 2
Lv.30 → 普通核心效果 3
Lv.40 / 高品阶 → 终极核心效果
```

## 五、Build 统计规则

Build 只统计已点亮道具。

```text
法门 Build = 统计 isLit=true 道具的 faMenTag
器类 Build = 统计 isLit=true 道具的 qiLeiTag
```

例如：

```text
4震雷法 = 4件已点亮的 zhenlei 道具
2印 = 2件已点亮的 yin 道具
```

未点亮道具永远不计入完整 Build。

## 六、棋盘容量裁定

V0.4 第一版容量：

```text
5×5 棋盘 = 25格
阵眼不可覆盖 = -1格
1个聚念石占 1格 = -1格
剩余可放战斗道具 = 23格
```

按当前 31 件核心道具草案的占格粗算，23 格最多支持两个完整 6 件法门 Build，不支持三个完整 6 件法门 Build。

当前完整法门粗略占格：

```text
震雷法 = 11格
离火法 = 11格
中岳法 = 16格
玄水法 = 12格
太白法 = 12格
```

可放下的双完整法门示例：

```text
震雷 + 离火 = 22格
震雷 + 玄水 = 23格
震雷 + 太白 = 23格
离火 + 玄水 = 23格
离火 + 太白 = 23格
```

放不下：

```text
玄水 + 太白 = 24格
中岳 + 任意完整 6 件法门
三个完整 6 件法门
```

推荐构筑目标：

```text
一个主法门 4/6 或 6/6
+ 一个副法门 2/4
+ 一个到两个器类 Build
+ 聚念石点亮与接亮路线
+ 阵位点强化核心件
```

## 七、玩家外显解释

第一版玩家只需要理解：

```text
阵眼是要保护的中宫核心。
阵眼上下左右四格是阵脉点。
聚念石点亮附近道具。
亮着的道具可以接亮上下左右相邻道具。
阵脉点会强化已点亮道具。
核心效果需要养成开窍后才会发动。
```

## 八、推荐 Item Sandbox 拆包顺序

1. `ItemSandboxDetailUi01`
2. `ItemInnerDataCatalog01`
3. `ItemGridPlacementAndEyeRule01`
4. `JuNianLightingAndAdjacentRelay01`
5. `ArrayBonusCellResolver01`
6. `BuildSynergyCore01`
7. `CoreAwakeningPreview01`
8. `ItemDetailProjection01`
9. `ItemSystemValidator01`

## 九、当前 Guard 线禁止范围

Item Guard 线不得直接实现或修改：

```text
V0.3 RunFlow
V0.4 Battle system
Battle Bridge
UnifiedBattlePage
SaveData
Reward
正式掉落
正式升级存档
Boss 流程
Item Sandbox 之外的正式场景生产
```

## 十、首包建议

建议首包：

```text
ItemSandboxDetailUi01
```

原因：

```text
先建立独立 Item Sandbox 场景与道具详情页 UI 外壳。
沙盒内先使用只写名字的灰盒道具，提前留 ItemDetailViewModel / Provider 接口。
本包重点验证玩家外显详情栏的版式、字段顺序、选择交互和状态表达。
真实 31 件道具数据、摆放规则、点亮、接亮、Build、开窍计算后续分包进入。
```

## 十一、外部 Item 总纲吸收裁定

`C:/Users/Ella/Downloads/codex/item/item_system.md` 可作为 Item 系统上游总纲参考，但不得原样覆盖本 Guard。

可吸收内容：

```text
Item = 法门Tag + 器类Tag + 占格Shape + coreCell + 基础效果 + 开窍核心 + 词条 + Build修饰。
五法门：震雷法 / 离火法 / 中岳法 / 玄水法 / 太白法。
五器类：符 / 印 / 令 / 镜 / 法。
31件核心战斗道具草案：30件法门道具 + 1件聚念石。
道具详情旧纸暗金视觉体系。
法门套系视觉、器类视觉、品阶视觉递进。
核心效果靠开窍解锁。
已点亮道具才发动、计入 Build、吃阵位点加成。
Item 驱动技能规则，角色承接技能表现。
主 Build 生成 4 个自动监控 icon：普攻 / Build2 / Build4 / Build6。
```

必须覆盖修正内容：

```text
任何“阵眼可以被道具覆盖”的旧口径无效。
本 Guard 以“阵眼石是 5x5 中心核心，不可覆盖”为准。
阵眼石不是道具，不是阵位点，不是普通可占格。
阵眼石上下左右四格才是阵脉 / 阵位点。
```

当前阶段只允许把该总纲用于 Guard 口径、任务拆包、接口预留和 UI/视觉方向，不直接进入正式战斗、正式流程、掉落、养成或存档实现。

## 十二、技能表现与主 Build 自动监控接口预留

Item 系统未来允许把“道具触发”转译为战斗中 Q 版小人的技能表现。

核心原则：

```text
Item / Build 决定规则来源。
Q 版角色承接视觉表现。
玩家不为每件道具手动点技能。
主 Build 只生成 4 个自动监控 icon。
```

4 个自动监控位固定为：

```text
普攻
Build2
Build4
Build6
```

含义：

```text
普攻 = 当前主 Build 的基础循环 / 默认自动触发表现。
Build2 = 主 Build 2件档状态启动或被动监控。
Build4 = 主 Build 4件档状态转化或条件触发监控。
Build6 = 主 Build 6件档大成终结或充能爆发监控。
```

第一版只留接口，不做真实技能战斗逻辑、不做 Q 版动画、不做 Build 结算：

```text
ItemSkillMonitorSlot
  slotType              // BasicAttack / Build2 / Build4 / Build6
  sourceBuildId         // 主 Build 来源
  sourceFaMenTag        // 法门来源，可空
  sourceQiLeiTag        // 器类来源，可空
  triggerKind           // PassiveTick / Condition / Cooldown / ChargeFull 等占位
  isUnlocked            // 是否已解锁该档位
  isActive              // 是否当前可监控
  cooldown01            // 冷却进度占位
  charge01              // 充能进度占位
  displayName           // UI 名称
  iconKey               // UI 图标 Key
  tooltipText           // 详情说明
  presentationCueId     // 未来技能表现 Cue
  chibiActionKey        // 未来 Q 版角色动作 Key
```

该接口未来由 Item / Build 系统只读输出，Battle / UnifiedBattlePage / 表现层只能消费，不得反向改写 Item 规则。

## 十三、包状态记录

### 2026-07-09：ItemSandboxDetailUi01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMSANDBOX_DETAILUI01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
独立 Item Sandbox 场景。
灰盒道具名列表。
旧纸单列道具详情卡。
ItemDetailViewModel 与 IItemDetailViewModelProvider。
Lighting / Build / Awakening / Acquire / Upgrade / BattleEffect Preview 接口预留。
stub 数据覆盖已点亮 / 未点亮、已开窍 / 未开窍、阵位加成生效 / 未生效。
```

验收依据：

```text
Docs/V0.4/Reports/ItemSandboxDetailUiReport.md
Docs/V0.4/Reports/ItemDetailViewModelSpec.csv
Docs/V0.4/Reports/ItemSandboxDetailUiLeakCheckReport.md
```

边界确认：

```text
未实现真实点亮、接亮、Build、开窍、掉落、升级、存档、战斗结算。
未接 BattleResolver / Battle Bridge / UnifiedBattlePage。
未修改 V0.3 RunFlow、V0.4 Battle system、Boss、Reward、SaveData、BuildSettings。
进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

后续拆包注意：

```text
后续包不得重复搭建 Item Sandbox 详情 UI 外壳。
后续真实规则应通过已预留的 ViewModel / Provider / Preview 接口逐步接入。
```

### 2026-07-09：ItemInnerDataCatalog01

```text
Status: SELFTEST_PASS_AWAITING_USER_HANDTEST
Pending guard receipt: GUARD_PASS_ITEMINNERDATACATALOG01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
31 件 Item 内层目录数据：30 件法门道具 + I031 聚念石。
五法门 + 中宫 tag / enum。
五器类 + 中宫符器 tag / enum。
Item Sandbox 场景切到 ItemInnerDataCatalogProvider。
沙盒列表生成 31 个 catalog 道具按钮。
详情页仍只通过 ItemDetailViewModel 展示。
DevStubProvider 保留为旧灰盒样例边界，当前场景使用 CatalogProvider。
```

自测依据：

```text
Docs/V0.4/Reports/ItemInnerDataCatalogReport.md
Docs/V0.4/Reports/ItemInnerDataCatalogSpec.csv
Docs/V0.4/Reports/ItemInnerDataCatalogLeakCheckReport.md
```

自测结论：

```text
Unity verifier PASS。
Unity compile PASS。
LeakCheck PASS。
CSV 为 1 行表头 + 31 行数据。
BuildSettings 未修改，Item Sandbox scene 未注册。
阵眼石未进入 catalog。
聚念石是唯一 isLightingSource=true 条目。
```

边界确认：

```text
未实现真实点亮、接亮、Build、开窍、掉落、升级、存档、战斗结算。
未接 BattleResolver / Battle Bridge / UnifiedBattlePage。
未修改 V0.3 RunFlow、V0.4 Battle system、Boss、Reward、SaveData、BuildSettings。
进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

手测通过后更新：

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMINNERDATACATALOG01
```

### 2026-07-10：ItemGridPlacementAndEyeRule01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMGRIDPLACEMENTANDEYERULE01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
Item Sandbox 内新增 5x5 摆放规则与灰盒棋盘预览。
eyeCell 固定为 (2,2)，不可覆盖。
arrayBonusCells 固定为 (2,1) / (2,3) / (1,2) / (3,2)，可占用但只显示身份。
点击格子会移动 anchorCell。
预览 shapeCells / occupiedCells / coreCellLocal / coreCellWorld。
道具选择仍通过 ItemDetailViewModel 刷新详情页。
```

自测依据：

```text
Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleReport.md
Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleSpec.csv
Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleLeakCheckReport.md
```

自测结论：

```text
Unity scene build PASS。
Unity verifier PASS。
LeakCheck PASS。
git diff --check 无本包空白错误。
BuildSettings 未修改，Item Sandbox scene 未注册。
5x5 board / eyeCell / arrayBonusCells 常量校验通过。
阵脉格占用允许。
覆盖阵眼石返回 EyeCellCovered。
越界返回 OutOfGrid。
重叠返回 CellOccupied。
coreCellWorld 可视化。
```

边界确认：

```text
未实现真实点亮、接亮、Build、开窍、掉落、升级、存档、战斗结算。
未执行阵脉加成计算。
未接 BattleResolver / Battle Bridge / UnifiedBattlePage。
未修改 V0.3 RunFlow、V0.4 Battle system、Boss、Reward、SaveData、BuildSettings。
进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

手测重点：

```text
点击中心 (2,2) 应显示 EyeCellCovered。
点击阵脉格如 (2,1) 应允许占用。
横向二格道具点击 (4,0) 应显示 OutOfGrid。
点击 demo 占用格 (0,0) 应显示 CellOccupied。
```

用户确认：

```text
2026-07-10 用户确认本包手测通过。
```

### 2026-07-10：JuNianLightingAndAdjacentRelay01

```text
Status: SELFTEST_PASS_AWAITING_USER_HANDTEST
Pending guard receipt: GUARD_PASS_JUNIANLIGHTINGANDADJACENTRELAY01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
聚念石使用上下左右一格作为 Sandbox 默认直接点亮范围。
普通道具只按 coreCellWorld 是否进入 litRangeCells 判断直接点亮。
已点亮普通道具可通过 occupiedCells 上下左右相邻继续接亮。
允许链式接亮，不允许斜角、跨空格或通过阵眼石传导。
多路径优先最小 litDepth，同深度按稳定 itemId 选择来源。
ItemDetailViewModel 投影 isDirectLit / isLit / litByItemId / litDepth / basicEffectActive。
Item Sandbox 提供 10 个可重复切换的灰盒 Lighting Scenario。
```

自测依据：

```text
Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md
Docs/V0.4/Reports/JuNianLightingAndAdjacentRelaySpec.csv
Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayLeakCheckReport.md
```

自测结论：

```text
Unity verifier PASS。
ItemGridPlacementAndEyeRule01 回归 PASS。
ItemSandboxDetailUi01 回归 PASS。
ItemInnerDataCatalog01 回归 PASS。
LeakCheck PASS。
BuildSettings 未修改，Item Sandbox scene 未注册。
禁区扫描只命中 verifier 黑名单、只读检查与保护字段，没有正式接线。
git diff --check 仅保留进入前已有 BattleSandboxPreview scene 的 CRLF 提醒。
```

边界确认：

```text
未实现 Build 计数、阵脉加成、开窍、掉落、升级、存档或战斗结算。
未实现四个自动技能监控 icon 或 Q 版角色表现。
未接 BattleContract / BattleResolver / Battle Bridge / UnifiedBattlePage。
未修改 V0.3 RunFlow、V0.4 Battle system、Boss、Reward、SaveData、BuildSettings。
进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

手测重点：

```text
切换 10 个 Lighting Scenario，逐项核对直亮、coreCell 判定、链式接亮和阻断案例。
确认斜角、空格和阵眼石均不能形成接亮。
确认多路径案例稳定选择最小深度与稳定来源。
点击道具后确认详情显示 isDirectLit / isLit / litByItemId / litDepth / basicEffectActive。
```

后续接口约束：

```text
当前 Sandbox 使用 catalog itemId 作为场景内解析键，只覆盖单实例测试。
进入 Build 或正式 Item Snapshot 前必须新增稳定 itemInstanceId / placementId。
itemId 继续表示道具定义身份，itemInstanceId / placementId 表示本次摆放实例。
解析、选源和详情查询不得因同一 itemId 的多个实例发生键冲突。
```

手测通过后更新：

```text
Status: PASS
Guard receipt: GUARD_PASS_JUNIANLIGHTINGANDADJACENTRELAY01
```

### 2026-07-10：ArrayBonusCellResolver01

```text
Status: PASS
Guard receipt: GUARD_PASS_ARRAYBONUSCELLRESOLVER01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
固定 AP01=(2,1) / AP02=(2,3) / AP03=(1,2) / AP04=(3,2)。
道具 occupiedCells 与阵脉点重叠时输出 isOnArrayBonusCell=true。
isArrayBonusActive 严格按 isLit && isOnArrayBonusCell 计算。
未点亮道具只记录阵脉占用，不激活阵脉状态。
多格道具可识别一个或多个 occupiedArrayBonusCells。
新增 placementId，itemId 继续表示目录定义，placementId 表示摆放实例。
Lighting / ArrayBonus / ItemDetailViewModel 均已支持同 itemId 多实例隔离。
```

验收依据：

```text
Docs/V0.4/Reports/ArrayBonusCellResolverReport.md
Docs/V0.4/Reports/ArrayBonusCellResolverSpec.csv
Docs/V0.4/Reports/ArrayBonusCellResolverLeakCheckReport.md
```

验收结论：

```text
Report: PASS。
Spec CSV: 全部案例 PASS。
LeakCheck: PASS。
用户确认 Scenario 4 接亮后阵脉激活、Scenario 7 阵眼阻断后只占用不激活。
Detail 面板可读取 placementId / occupiedArrayBonusCells / isOnArrayBonusCell / isArrayBonusActive。
JuNianLightingAndAdjacentRelay01 记录的 placementId 前置约束已由本包收口。
```

边界确认：

```text
本包只验收阵脉状态，不验收或实现攻击、防御、冷却、倍率等数值加成。
未实现 Build 计数、战斗结算、掉落、养成、存档、奖励、Boss 或主流程接入。
未接 BattleResolver / Battle Bridge / UnifiedBattlePage / V0.3 RunFlow。
BuildSettings 未修改，Item Sandbox scene 未注册。
进入前已有的 Scene_TalismanBag_V04_BattleSandboxPreview.unity dirty 状态不属于本包。
```

### 2026-07-10：BuildSynergyCore01

```text
Status: PASS
Guard receipt: GUARD_PASS_BUILDSYNERGYCORE01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已通过部分：

```text
已点亮普通道具的法门 / 器类统计主体已建立。
法门 Build2 / Build4 / Build6 与器类 Build2 / Build4 阶段计算已建立。
聚念石排除、详情投影、只读 Snapshot Provider 和灰盒总览已建立。
未接正式战斗、存档、奖励、Boss 或 BuildSettings。
```

Guard 阻断项：

```text
本包 assignment 明确禁止自动选择主 Build，但实现了 SelectMainBuild 并按阶段、数量、法门优先级和 tag 自动选中。
本包 assignment 明确禁止生成四个监控位，但实现并激活 BasicAttack / Build2 / Build4 / Build6 MonitorSlots。
Build Resolver 未按 placementId 去重；重复 placementId 会进入 sourceItems.Count 并被重复计数。
缺少 assignment 要求的 validationErrors，以及空 placementId、重复 placementId、缺 Catalog/tag 等明确校验输出。
Verifier 把主 Build 自动选择和四监控位生成作为 PASS 条件，与本包禁止项相反。
Spec CSV 未覆盖 assignment 要求的完整最小案例和校验案例。
```

返修边界：

```text
保留 Build 统计、阶段、贡献 placementId、详情进度和只读 Build Snapshot。
移除 SelectMainBuild 的自动决策及其详情/总览投影；selectedMainBuildId 保持空或纯预留字段。
本包不得生成 ItemSkillMonitorSlot 实例；四监控位留到独立 ItemSkillTriggerContract 包。
按 placementId 去重，并把重复/空 placementId 等问题写入 validationErrors，不能未处理异常或重复计数。
Verifier 改为断言 mainBuild 未自动选择、MonitorSlots 未生成，并补齐统计与校验案例。
返修不得扩展到 Build 效果、技能、战斗、存档或正式流程。
```

返修验收：

```text
2026-07-10 返修完成并通过 Guard 复验。
selectedMainBuildId 保持空预留，本包不自动选择主 Build。
ItemSkillMonitorSlot / MonitorSlots 已从本包移除，不生成四个监控位。
重复 placementId 先去重再统计，并写入 ValidationErrors。
空 placementId、缺 Catalog、非法法门/器类 tag、null snapshot 均输出 validationErrors，不抛未处理异常。
详情投影只保留 countedInBuild、法门/器类进度、阶段与 validationErrors。
Verifier 已改为断言无主 Build 自动选择、无 MonitorSlots，并覆盖去重与校验案例。
BuildSynergyCoreReport / Spec CSV / LeakCheck 全部 PASS。
未实现 Build 效果、数值、技能释放或正式系统接线。
```

### 2026-07-10：CoreAwakeningPreview01

```text
Status: PASS
Guard receipt: GUARD_PASS_COREAWAKENINGPREVIEW01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已通过部分：

```text
已建立独立 Awakening 规则层和只读输入 / Snapshot Provider。
Lv.10 / Lv.20 / Lv.30 / Lv.40 四节点主体已建立。
已区分 coreEffectUnlocked 与 coreEffectActive。
基础效果按 isLit，核心效果按 isLit && isUnlocked 处理。
聚念石不生成或发动普通核心效果。
详情 ViewModel 与 Sandbox 灰盒预览接口已接入。
Scoped forbidden-token scan 和 runtime-only Roslyn compile 通过。
```

Guard 阻断项：

```text
assignment 锁定 Sandbox 等级范围为 1-40，并要求越界校验与夹取；当前实现允许 Lv.0、只做 Math.Max(0)，也未限制大于40。
assignment 明确高品阶终极开窍只留字段、本包不计算；当前 highRarityUltimatePreview 会直接解锁 Ultimate。
四个 nodeId 是所有道具共享的 core_1_lv10 等通用 ID，没有形成 I001_CORE_01 这类按道具稳定身份。
Resolver 固定使用 DefaultNodeDefinitions，未提供定义输入 / Provider，无法校验缺失定义或重复 coreEffectId。
Spec 未覆盖 Lv.1/9/19/29/39、低于1、高于40、高品阶预留不生效、缺定义和重复效果ID等 assignment 案例。
Unity / Tuanjie batch verifier 未运行，当前只有 runtime-only Roslyn compile，不能作为最终 Unity PASS。
```

返修边界：

```text
保留现有点亮与开窍分离、聚念石排除、详情投影和灰盒结构。
输入同时保留 inputLevel 与 resolvedLevel；resolvedLevel 必须夹在1-40，越界写入 validationErrors。
缺少等级输入时使用明确 Sandbox 默认值 Lv.1，并写入可追踪输入来源；不得继续用 Lv.0 作为正常状态。
highRarityUltimatePreview / requiredRarityKey 只保留为 Reserved，本包不得参与 isUnlocked 计算。
普通道具核心效果 ID 必须包含 itemId，并稳定区分 Core1 / Core2 / Core3 / Ultimate。
增加只读核心效果定义 Provider 或等价输入，校验缺定义、重复 coreEffectId、错误节点等级与重复节点。
补齐 assignment 的等级临界、越界、定义校验、Build/阵脉不解锁和回归案例。
返修后先完成 runtime verifier，再在可用 Unity / Tuanjie Editor 环境运行 batch verifier 并重写三份报告。
不得扩展到正式升级、品阶解锁、数值效果、技能释放、存档或正式流程。
```

返修复验进展：

```text
规则返修静态复验通过：1-40夹取、高品阶只预留、itemId专属核心效果ID、定义Provider与校验均已落地。
Guard 从项目历史日志定位到 Unity Editor：F:/2022.3.50f1c1/Editor/Unity.exe。
2026-07-10 已实际运行 CoreAwakeningPreviewVerifier.VerifyStaticBatch。
Unity 项目加载与脚本编译完成，没有 C# 编译错误，但 verifier 最终结果为 FAIL。
当前唯一失败：Sandbox greybox level preview catalog still emits invalid Lv0 or misses Lv40 preview。
失败归因是 verifier 断言错误，不是 Runtime 规则错误：PreviewCatalog 明确把 I015 映射为 Lv.20，verifier 却按 placementId 名称 P_LV40 要求该 I015 解析为 Lv.40。
PreviewCatalog 实际已通过 I001/I007/I015/I004/I013 提供 Lv.1/Lv.10/Lv.20/Lv.30/Lv.40，且没有正常 Lv.0。
最小返修应只修改 CheckSandboxPreviewCatalog：校验全部 preview resolvedLevel 均在1-40，且集合包含1/10/20/30/40；不得依赖 P_LV40 名称推断等级。
修正 verifier 后使用同一 batch 方法重跑，三份报告必须全部改为 PASS，才签最终 Guard receipt。
Unity log: Logs/codex_coreawakeningpreview01_unity_verify.log
```

最终返修验收：

```text
2026-07-10 TASK_REWORK_COREAWAKENINGPREVIEW01_VERIFYFIX 完成。
CheckSandboxPreviewCatalog 已改为校验实际 resolvedLevel 集合，不再依据 placementId 名称推断等级。
正常 Preview 等级全部位于1-40，不含Lv.0，并覆盖1/10/20/30/40。
CoreAwakeningPreviewReport: PASS。
CoreAwakeningPreviewSpec: 全部案例 PASS，sandboxPreviewCatalog PASS。
CoreAwakeningPreviewLeakCheckReport: PASS。
Unity batch verifier: PASS。
Runtime Awakening 规则在 verifier 修复中未修改。
git diff --check 仅保留无关旧文件 CRLF warning。
```

### 2026-07-10：ItemSkillTriggerContract01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMSKILLTRIGGERCONTRACT01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已完成内容：

```text
新增独立 ItemMainBuildSelectionInput 与 ItemSkillMonitorResolutionResult。
主 Build 只能由 Sandbox 显式选择，默认 None，不自动推断或切换。
主 Build 只接受法门 Build，器类 Build、非法 ID、缺失轨道和重复轨道均锁定并输出 validationErrors。
固定输出 BasicAttack / Build2 / Build4 / Build6 四个只读监控位。
解锁条件分别为主法门已点亮数量1件、Build2、Build4、Build6。
isTriggered 始终为 false，cooldown01 / charge01 始终为0。
iconKey / presentationCueId / chibiActionKey 仅输出稳定预留 Key，不绑定正式资源或执行表现。
ItemDetailViewModel 通过独立 skillMonitorPreview 投影，不回写 ItemBuildSynergyResolutionResult。
Sandbox 提供 None + 五法门显式选择按钮与四个不可点击灰盒 icon。
```

验收依据：

```text
Docs/V0.4/Reports/ItemSkillTriggerContractReport.md
Docs/V0.4/Reports/ItemSkillTriggerContractSpec.csv
Docs/V0.4/Reports/ItemSkillTriggerContractLeakCheckReport.md
```

验收结论：

```text
ItemSkillTriggerContractVerifier: PASS。
BuildSynergyCore verifier: PASS。
CoreAwakeningPreview verifier: PASS。
Lighting / ArrayBonus 回归: PASS。
Spec 覆盖无选择、数量0/1、Build2/4/6、Build6降档、双法门不自动选、显式低阶段不切换、器类/非法/缺失/重复/null输入。
四个 slot 顺序固定且唯一，isTriggered=false，cooldown01=0，charge01=0。
BuildSynergyCore01.selectedMainBuildId 继续为空预留，Build Resolver 不包含 MonitorSlots。
Report / Spec / LeakCheck 全部 PASS，Errors: None。
```

边界确认：

```text
未实现技能触发、伤害、治疗、护盾、状态、冷却、充能或核心效果执行。
未制作正式技能图标、Q版角色动作、动画或特效。
未接 BattleResolver / BattleContract / Battle Bridge / UnifiedBattlePage。
未修改 V0.3 RunFlow、SaveData、Reward、Boss 或 BuildSettings。
进入前已有的 BattleSandbox / BuildSandbox dirty 文件和报告不属于本包。
```

### 2026-07-10：ItemDetailProjectionComplete01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMDETAILPROJECTIONCOMPLETE01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已通过部分：

```text
单一 ItemDetailProjectionComposer 已建立。
Catalog / Lighting / ArrayBonus / Build / Awakening / SkillMonitor 已统一投影到最终 ItemDetailViewModel。
15个玩家章节与3个调试章节已建立，章节顺序通过。
玩家字段与原始调试字段分离通过。
深克隆、幂等投影、缺 Snapshot 降级、有效 placementId 多实例隔离通过。
详情/调试页签、双 ScrollRect、动态 LayoutGroup / ContentSizeFitter 和切换回顶已建立。
Unity布局应用与本包 verifier 报告为 PASS，禁区扫描通过。
```

Guard 阻断项：

```text
当调用方提供非空 placementId 但 Snapshot 中找不到该实例时，Composer 会退回 FindItemResult(itemId)，读取第一件同类实例状态。
该回退会在 placementId 过期、布局刷新或同 itemId 多实例时把其他实例的点亮/阵脉/Build/开窍状态投影到当前详情。
当前只有 TalismanBag.ItemSandbox.ItemSandboxDetailPanelView / ItemSandboxDetailSectionView 和场景内对象，没有可供0.4战斗页复用的 Runtime ItemDetailPanel Prefab。
用户已明确要求后续0.4保留原摆放手感并直接弹出本详情；场景绑定的 ItemSandbox View 不能作为正式接入交付。
报告与 Spec 未覆盖 assignment 要求的31件Catalog逐项打开、长文本压力、1080x1920和720x1280布局验证。
Spec CSV 未采用 assignment 规定的完整状态/布局列，也没有 stale placementId 不得回退的案例。
```

返修边界：

```text
保留当前 Composer、ViewModel、15/3章节、页签、ScrollRect和视觉结构。
为投影输入增加明确上下文：CatalogPreview 与 PlacedInstance，或等价不可混淆的模式。
CatalogPreview 不读取任何 placement-scoped Lighting / Array / BuildItem / Awakening 状态，只显示尚未摆放或状态不可用。
PlacedInstance 必须使用非空 placementId 严格查询；查不到时返回状态不可用并写调试诊断，禁止回退 FindItemResult(itemId)。
聚念石已摆放详情也必须使用 placementId；不得依赖 itemId 回退。
新增 stale placementId + 同itemId多实例测试，断言绝不串状态。
提取 Runtime 可复用 ItemDetailPanelView / ItemDetailSectionView，命名空间不得依赖 ItemSandbox。
交付一个包含详情/调试页签、双ScrollRect和15/3章节绑定的可复用 Prefab；Prefab 只接收 ItemDetailViewModel，不引用 Battle、RunFlow 或 Resolver。
Item Sandbox 应使用同一 Prefab 或同一 Runtime View 结构验证，不能保留两套详情实现。
补齐31件Catalog打开、长文本完整滚动、1080x1920和720x1280布局检查，并写入 Report / Spec。
更新 verifier：检查 Prefab、Runtime命名空间、严格placement查询、双分辨率、长文本和31件全量案例。
返修不得接入0.4正式战斗页；正式页面Adapter仍由后续战斗接入包负责。
```

最终返修验收：

```text
CatalogPreview 与 PlacedInstance 投影上下文已严格分离。
PlacedInstance 只按 placementId 查询；缺失或过期实例返回状态不可用，不再按 itemId 回退或串用同类实例状态。
I031 聚念石已摆放详情同样遵守严格 placementId 边界。
已交付 Runtime ItemDetailPanelView / ItemDetailSectionView 与可复用 ItemDetailPanel Prefab，命名空间不依赖 ItemSandbox。
Item Sandbox 通过适配层复用 Runtime View，不保留第二套正式详情实现。
31件 CatalogPreview 全量验证通过，包含 I031 聚念石边界。
长文本完整滚动、切换道具滚动复位、1080x1920 与 720x1280 布局验证通过，无重叠。
本包 verifier、LeakCheck 与8个相关回归 verifier 均为 PASS。
未接入 Battle、Battle Bridge、UnifiedBattlePage、RunFlow、SaveData、Reward、Boss 或 BuildSettings。
```

### 2026-07-10：ItemSystemValidatorAndSnapshot01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMSYSTEMVALIDATORANDSNAPSHOT01
Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
```

已通过部分：

```text
ItemSystemSnapshot.v1、IItemSystemSnapshotProvider 与 IItemSystemValidator 已建立。
Catalog、Placement、Lighting、ArrayBonus、Build、Awakening、SkillMonitor 与主 Build 选择已汇总进统一 Snapshot。
Provider 已复制输入，并对输出排序；相同输入的当前调试签名保持稳定。
棋盘、阵眼、四阵脉、31件 Catalog、I031 聚念石、placementId、越界、Build、阵脉、开窍和监控位基础校验已建立。
Item Sandbox 已增加只读 Snapshot 灰盒预览。
开发侧 Unity batch verifier 为 32/32 PASS，九项历史回归与 LeakCheck 均为 PASS。
未接入正式 Battle、BattleContract、Bridge、RunFlow、SaveData、Reward、Boss 或 BuildSettings。
```

Guard 阻断项：

```text
Snapshot 各集合以 IReadOnlyList 暴露，但属性直接返回内部数组；调用方可把接口强制转换回数组并改写，未满足“快照生成后不能被外部修改”。
当前 immutable 案例只验证创建输入后修改源 List，不验证调用方修改 Snapshot 暴露集合；多数 Spec 行的 immutable 值被直接写为 true。
独立 ItemSystemValidator 未检查不同 placement 的 occupiedCells 重叠，公开构造的重叠 Snapshot 可绕过 PLACEMENT_OVERLAP。
独立 Validator 未完整检查 Catalog 重复 itemId 与普通道具伪造 lighting source；Provider 前置检查不能替代公开 Validator 的完整契约。
Snapshot 同时保存 placement 汇总状态与 Lighting / ArrayBonus / Build / Awakening 子快照，但 Validator 未核对这些分支的 placementId 集合和状态一致性。
因此可构造 placement 显示未点亮、Build 子快照却计数，或 placement 与 Array/Awakening 结果互相矛盾但仍被判定有效的 Snapshot。
BuildDebugSignature 未覆盖完整 Catalog、Lighting、ArrayBonus、Awakening 与全部元数据，当前确定性测试不能证明整个 Snapshot 确定。
```

返修边界：

```text
保留现有 Snapshot v1 字段、Provider 计算顺序、Sandbox 灰盒和既有 Resolver，不重写玩法规则。
所有 Snapshot 及嵌套 Snapshot 集合必须真正只读：使用 ReadOnlyCollection/等价封装或返回防御性副本，禁止向外暴露可强转修改的内部数组。
新增外部集合篡改测试，覆盖顶层 placements/catalog/lighting/array/awakening/errors 以及 placement/build/awakening/monitor 内嵌集合；篡改必须被拒绝或不能改变 Snapshot。
补齐独立 Validator 的 placement 重叠、Catalog 重复ID、普通道具 lighting source 与非法 rotation 校验。
补齐跨分支一致性校验：placement、lighting、array、build、awakening 必须按 placementId 一一对应，不得缺失、孤儿、itemId 不同或关键状态互相矛盾。
至少验证 isLit/isDirectLit/isLightingSource、isOnArrayBonusCell/isArrayBonusActive、countedInBuild、unlocked/active coreEffect 在汇总与子快照之间一致。
新增伪造子快照案例，证明未点亮 Build 计数、未点亮阵脉激活、错误核心效果状态不能只靠修改另一个汇总字段逃逸。
完整确定性签名或等价深比较必须覆盖 Snapshot 全字段和全部集合，并使用文化无关格式；反转输入顺序后结果仍一致。
Spec 的 immutable 列必须来自真实断言，不得由公共 helper 无条件写 true。
返修后重跑本包 Unity batch verifier、九项回归、LeakCheck 与 git diff --check，并刷新三份报告。
不得接正式战斗页、Battle Adapter、BattleContract、Battle Bridge、RunFlow、SaveData、Reward、Boss、BuildSettings，也不得扩展真实技能、数值、掉落或养成。
```

首次返修复验：

```text
ReadOnlyCollection / Array.AsReadOnly 已覆盖 Snapshot、Input 与嵌套集合，外部数组强转和 IList 修改攻击测试通过。
完整 Canonical Signature 已覆盖公开 Snapshot 分支并使用 InvariantCulture；正常、反转输入与无效输入确定性测试通过。
placement 与 Lighting / ArrayBonus / Build / Awakening 的缺失、孤儿、重复、itemId 和主要状态一致性校验已落地。
placement overlap、Catalog duplicate/source、非法 rotation、Lighting 基础自一致性与32行旧案例回归已补齐。
Unity batch verifier 为52/52 PASS，九项历史回归、LeakCheck 与 git diff --check 通过。
```

首次返修剩余阻断：

```text
ValidateBuildTrack 当前只检查 litItemCount 等于 SourcePlacementIds 数量，并检查来源 placement 已 countedInBuild。
它没有按 trackKind / stableTag / buildId 校验来源 ItemResult 的 faMenTag 或 qiLeiTag，因此错误法门/器类来源可以混入其他 Track 而不报错。
它没有校验 SourceItemIds 的数量、顺序和 placement→itemId 映射；伪造 SourceItemIds 不会被发现。
它也没有证明某 Track 的来源集合与该 Tag 下全部 countedInBuild ItemResult 完全相等，合法来源可被静默漏掉后同步缩小 litItemCount。
当前52行 Spec 没有 wrong-tag source、SourceItemIds mismatch、missing counted source 案例，不能证明“法门/器类 Track 不得串 Tag”。
```

最小二次返修：

```text
只修改 Build Track 内部一致性校验与对应 verifier，不改 Provider、Resolver、Snapshot schema、Sandbox 或其他玩法。
按 trackKind 选择 faMenTag/faMenBuildId 或 qiLeiTag/qiLeiBuildId，从 countedInBuild ItemResults 重建该 Track 的期望来源。
断言 track.buildId、stableTag、SourcePlacementIds、SourceItemIds 与 litItemCount 全部等于期望集合；来源必须按稳定顺序比较。
新增 build-track-wrong-tag-source、build-track-source-itemids-mismatch、build-track-missing-counted-source 三个伪造 Snapshot 案例，均输出 BUILD_SNAPSHOT_MISMATCH。
重跑本包 Unity batch verifier、九项回归、LeakCheck 与 git diff --check，并刷新三份报告。
```

二次返修复验：

```text
Build Track 已按 trackKind / stableTag / buildId 从 countedInBuild ItemResults 反推期望来源。
wrong-tag source、SourceItemIds mismatch、missing counted source、kind/buildId mismatch 四个伪造案例均为 PASS。
Unity batch verifier 为56/56 PASS，九项回归、LeakCheck 与 git diff --check 通过。
```

二次返修剩余兼容性阻断：

```text
既有 ItemBuildTrackResult 构造器会对 SourceItemIds 执行 Distinct，SourceItemIds 的既有语义是“参与该 Track 的唯一 itemId 集合”，不是与 SourcePlacementIds 等长的一一映射表。
当前 ValidateBuildTrack 从 expectedSources 直接生成含重复值的 expectedItemIds，并要求实际 SourceItemIds 与其等长。
当两个同 itemId 实例同时已点亮并计入同一 Build 时，SourcePlacementIds 有两个 placementId，但 SourceItemIds 合法地只有一个唯一 itemId，当前 Validator 会误报 BUILD_SNAPSHOT_MISMATCH。
这与 placementId 支持同 itemId 多实例的既有规则冲突，当前56行 Spec 未覆盖两个同类实例同时计入 Build 的合法布局。
```

最小三次返修：

```text
不修改 ItemBuildTrackResult、Build Resolver、Provider 或 Snapshot schema。
SourcePlacementIds 继续与该 Track 的全部 expectedSources placementId 做完整、唯一、稳定排序比较。
SourceItemIds 改为与 expectedSources.itemId 的 Distinct + Ordinal稳定排序集合比较，不要求与 SourcePlacementIds 等长。
placementId→itemId 的真实对应关系继续以 buildSnapshot.ItemResults 为准；SourceItemIds 只作为唯一身份集合校验。
保留四个二次返修伪造案例，并新增 build-track-same-itemid-multi-instance-valid：两个同 itemId placement 同时已点亮、同时计入同一 Track，Snapshot 必须有效。
重跑本包 Unity batch verifier、九项回归、LeakCheck 与 git diff --check，刷新三份报告后再申请最终 Guard receipt。
```

最终三次返修验收：

```text
SourcePlacementIds 继续按完整 counted placement 集合严格校验。
SourceItemIds 已按既有契约改为 expectedSources.itemId 的 Distinct + Ordinal 稳定排序唯一集合。
placementId→itemId 对应关系继续由 buildSnapshot.ItemResults 提供，没有修改 Build Resolver 或 Snapshot schema。
build-track-same-itemid-multi-instance-valid 已覆盖两个 lit/counted I001 实例：SourcePlacementIds=2，SourceItemIds 唯一集合为 I001，Snapshot 判定有效。
此前 wrong-tag source、SourceItemIds mismatch、missing counted source、kind/buildId mismatch 四个伪造案例继续通过。
ItemSystemValidatorAndSnapshotSpec 共57行，57/57 PASS。
Unity batch verifier、九项历史回归、LeakCheck 与 git diff --check 全部通过。
未接入正式 Battle、BattleContract、Battle Bridge、UnifiedBattlePage、RunFlow、SaveData、Reward、Boss 或 BuildSettings。
```

### 2026-07-11：Item 算法规则同步

```text
Status: RULE_SYNC_ACCEPTED
Source: Item算法窗口
Algorithm rules: Docs/V0.4/ItemAlgorithmGuard_CurrentRules.md
Algorithm queue: Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Code change: None
Locked docs change: None
```

总体 Item Guard 接受以下归属：

```text
Item算法系统是总体Item系统下属实例生成分线，不是平级系统。
总体Item系统拥有基础身份、摆放、点亮、Build统计、开窍语义、详情投影和公共只读契约。
Item算法系统拥有品阶、属性区间、词条池、实例Roll、Build资格、核心潜力、关卡品阶权重、seed与概率模拟。
Reward、Save、Battle、养成与UI只能消费结果，不得重新Roll。
```

已锁定的关键新规则：

```text
I001-I030为30件普通原型；I031聚念石不参与30×5普通生成。
五品阶稳定键为white/green/blue/purple/orange，对应凡/良/灵/玄/道。
150表示baseItemId + rarity派生组合，不创建150个独立itemId。
baseItemId、itemInstanceId与placementId三层身份必须分离。
基础属性按每件道具、每条属性、每个品阶独立区间Roll，不使用统一倍率或主品质扰动。
固定词条类型固定但数值可Roll；随机词条ID与数值分离，槽位/权重/互斥配置化。
品阶决定核心效果潜力，养成决定开窍，点亮决定战斗发动。
凡品BuildQualification=None；良品及以上概率方向递增，但具体分布尚未锁定。
前10关只掉凡品；其余关卡概率尚未锁定。
```

明确不接受为正式规则：

```text
v0.5草案中的具体词条槽、Build概率、掉落概率、道品100% Dual、itemPower公式和自动普攻主法门。
这些内容保持PROPOSAL_ONLY / NOT_APPROVED。
```

第一包状态：

```text
Package: V0.4-ItemRarityInstanceFoundation01
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMRARITYINSTANCEFOUNDATION01
User approval: CONFIRMED
Assignment: ISSUED
Execution: WAITING_DEV_WINDOW_EXECUTION
Boundary: SCHEMA_ONLY
```

兼容策略：

```text
当前不得修改ItemSystemSnapshot.v1字段或Canonical Signature。
先独立建立ItemInstanceIdentitySnapshot.v1；后续再建立ItemGeneratedInstanceSnapshot.v1。
任何公共字段接入必须同时经过Item算法Guard与总体Item Guard复验。
```

### 2026-07-11：Item 算法线 8/8 完成同步

```text
Status: COMPLETE
Queue: 8/8 COMPLETE
Final package: V0.4-ItemInstanceProjectionContract01
Final marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Spec: 156/156 PASS
Instances: 6
Queries: 9
Leaks: 0
User hand test: PASS
```

总 Item Guard 复验结论：

```text
ItemInstanceProjectionContractSnapshot.v1 与 ItemInstanceProjectionSetSnapshot.v1 已建立。
投影保持 itemInstanceId / baseItemId / rarity / generationVersion / rootSeed 等实例身份事实。
stats、affixes、eligible/visible core IDs 与 BuildQualification 以只读原始事实提供。
同 baseItemId + rarity 多实例保留，itemInstanceId 唯一；I031 被普通实例投影拒绝。
不包含 placement、lighting、cultivation unlock、Build计数或战斗激活状态。
ItemSystemSnapshot.v1 与 BuildDebugSignature 未修改；ItemDetailProjectionComplete 回归保持 PASS。
最终投影报告156/156 PASS，6实例、9查询、0泄漏；历史算法包与Item相关回归均通过。
```

跨系统边界：

```text
ItemDropRewardIntegration01当前不得启动。
它属于Reward / Inventory / SaveData正式接线，需要用户另行确认并下发跨系统assignment。
Reward、Save、Battle与UI不得重新Roll实例。
当前生成数据继续标记QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA。
```

RepoOps 约束：

```text
只允许精确选择本包及已确认算法包文件。
不得整目录git add。
不得混入进入任务前已有的BattleSandbox、BuildSandbox、动画资源或其他dirty/untracked内容。
本Guard不执行commit / tag / push。
```

### 2026-07-12：ItemBalanceWorkbenchAnd150CandidateSeed01 + GuardFix01

```text
Status: PASS
Guard receipt: GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
User hand test: PASS
GuardFix Spec: 1286/1286 PASS
```

总体 Item Guard 接受以下结果：

```text
算法基础包 8/8 COMPLETE
可编辑平衡工作台 COMPLETE
普通原型 30/30
五品阶候选版本 150/150
属性区间 600/600
Roll / Projection / Determinism 150/150
I031 聚念石普通生成池条目 0
```

当前成熟度边界：

```text
BALANCE_CANDIDATE
EDITABLE
NOT_LIVE_LOCKED
NOT_BATTLE_CONNECTED
```

这些候选资产可以编译进既有 Stat / Affix / Core / Build / Drop Schema，并在 Editor Workbench 中完成 150 版本 Roll 与 Projection；它们不能被解释为正式运行时数据、已通过实战平衡的数据或锁定上线数据。

尚未发生的正式接线：

```text
Formal Item Runtime: NO
Formal Item Detail: NO
Battle: NO
Reward / RunFlow: NO
Inventory / SaveData: NO
Formal Drop System: NO
```

`AFFIX_DURATION_BRIDGE` 仅为候选池所需定义，不自动成为正式词条规则。后续任何正式 Item Detail 数据接入、Battle 接入、Reward/RunFlow、Inventory/Save 或掉落接线都必须另开明确授权包；本回执不自动启动 `ItemDropRewardIntegration01`。

RepoOps 只能精确选择本包文件，不得整目录 `git add`，不得混入进入任务前已有的 dirty/untracked 内容。本 Guard 不执行 commit / tag / push。

### 2026-07-16：ItemDetail 非 Play 最满道品模板 / 美术插槽 / Roll 稳定复用规则

```text
Status: RULE_SYNC_ACCEPTED
Scope: ITEM DETAIL / ITEM SANDBOX ONLY
Code change: None
Scene change: None
Locked docs change: None
```

用户最终工作方式：

```text
先在 Unity 非 Play 状态交付一张“最满状态的道品详情模板”。
模板内先建立全部美术插槽，并放入当前已有的对应美术素材。
用户在非 Play 状态手动微调全部 UI 位置、大小、锚点、层级、字体、间距和图片表现。
用户确认后，该模板成为全部 Item Detail 的唯一全局版面真源。
之后再建立插槽素材管理工具和运行时数据绑定。
30件普通道具 × 5品阶、全部候选实例和所有 Roll 结果只能复用该模板，不得各自生成或改写版面。
```

最满道品模板定义：

```text
模板是 Editor / 非 Play 排版样本，不是正式掉落实例，也不代表数值已 LIVE_LOCKED。
模板必须以内容最满、文案压力最大的道品状态展示完整章节。
至少覆盖：道品背景、品阶、道具主体、阵法、器类、基础属性、固定词条、随机词条、4个核心效果、法门 Build 2/4/6、器类 Build、普攻 / Build2 / Build4 / Build6 技能图标，以及详情页当前已支持的点亮 / 阵脉 / 开窍状态。
模板只用于排版和美术插槽验证，不得因此执行真实 Roll、真实 Build 效果、真实战斗、真实掉落或真实养成。
```

当前用户已建立的三个美术插槽：

```text
zhenfaicon
→ 按 faMenTag 显示 Assets/_Game/Resources/item/阵法icon 下的阵法图标。

qixingicon
→ 当前层级名称保留；技术语义按 qiLeiTag 显示 Assets/_Game/Resources/item/器类icon 下的器类图标。
→ 不得把 qixingicon 解释为“七星”系统，也不得由 Builder 擅自重命名或移动该对象。

daoju
→ 按 baseItemId / itemId 显示 I001-I030 的普通道具主体美术。
→ 同一道具主体美术允许被道具栏、拖拽影子和详情页共同消费；品阶差异由背景、品阶图标、边框、词条等插槽表达，不要求制作150套主体图。
```

素材管理口径：

```text
现有素材根：Assets/_Game/Resources/item/
现有阵法、器类、品阶、基础属性、固定/随机词条、核心、Build技能、阵脉、聚念石、分割线与弹窗背景素材均通过稳定 visualKey / iconKey 解析。
缺少的 I001-I030 主体素材建议进入 Assets/_Game/Resources/item/道具icon/，以 I001.png 至 I030.png 的稳定 baseItemId 命名。
缺图时必须显示明确灰盒占位并输出缺口报告，不得阻断模板排版，也不得运行时创建另一套 UI。
.DS_Store 等系统文件不得进入素材注册表。
```

版面真源与 Roll 稳定硬规则：

```text
所有 UI 位置、大小、锚点、层级、字体、间距和视觉字段必须允许用户在 Unity Inspector 的非 Play 状态自由调整。
“稳定复用”锁的是 Runtime / Builder 不得覆盖用户排版，不是锁死用户不能修改。
运行时 Binder 只允许更新文字、Sprite、颜色、状态和必要 SetActive；不得修改 RectTransform、父节点、siblingIndex 或外层布局参数。
切换 itemId、rarity、等级、Build选择、点亮状态或重新 Roll 时，不得重建 ItemDetailPanel，不得替换插槽物体，不得运行 SceneBuilder / Ensure / CreateRuntime 重写版面。
所有150个 baseItemId + rarity 候选版本与同版本的不同 itemInstanceId 必须使用同一模板和相同外层相对排版。
属性、词条和文案数量变化只能发生在预设的固定内容槽或固定视口 ScrollRect 内；不得把外层章节推离用户手调位置。
没有内容的可选插槽默认只隐藏内容并保持版面契约；若未来允许折叠章节，必须另行取得用户版面授权。
Verifier / Smoke / Report 只读检查用户排版，不得在验证过程中修复、重建、保存 Prefab/Scene 或自动接受新的 hash / geometry baseline。
```

后续固定拆包顺序：

```text
1. ItemDetailMaxDaoArtTemplate01
   在 ItemSandbox / 可复用 ItemDetailPanel 的非 Play 状态建立最满道品展示模板，补齐全部插槽并放入当前已有素材。
   完成后必须暂停，由用户手调并确认；本包不建立全局 Runtime Roll 绑定。

2. ItemDetailVisualSlotTool01
   在用户确认的模板之上建立可编辑素材注册表、稳定 visualKey/iconKey、Inspector 替换与缺口/重复映射校验工具。
   工具只管理素材映射和引用，不得重建模板、移动插槽或覆盖任何用户手调字段。

3. ItemDetailGlobalReuseAndRollStability01
   让 I001-I030 × 五品阶、全部候选实例与不同 Roll 结果通过统一 ViewModel/Binder 复用已确认模板。
   批量验证切换道具、品阶、等级、Build状态和 Roll 前后外层布局不变；长文案与可变列表只在固定滚动视口内部变化。
```

三包共同禁止项：

```text
不接 V0.4 正式战斗页、BattleContract、Battle Bridge 或 UnifiedBattlePage。
不接 V0.3 RunFlow、正式养成页、Reward、Inventory、SaveData、Boss 或正式掉落。
不修改 Item 算法的 Roll 规则、候选数值、品阶概率、词条概率或 Build资格。
不把最满道品排版样本当作正式玩家获得实例。
不修改 BuildSettings。
不修改 AGENTS.md / Docs/LOCKED/*。
不 commit / tag / push。
```

当前队列指针：

```text
NEXT: ItemDetailMaxDaoArtTemplate01
STATE: WAITING_TASK_ASSIGNMENT
GATE: 完成后必须等待用户非 Play 手调与明确确认，才可下发 ItemDetailVisualSlotTool01。
```

### 2026-07-17：ItemDetailMaxDaoArtTemplate01 阶段状态同步

```text
Package: ItemDetailMaxDaoArtTemplate01
Status: USER_VISUAL_TUNING_NEAR_COMPLETE
Layout: LAYOUT_CHECKPOINT_ACCEPTABLE
Art: ART_ASSETS_STILL_PARTIAL
Guard: NOT_FINAL_GUARD_PASS
Next: WAITING_FINAL_MANUAL_ACCEPTANCE_AND_CLOSEOUT
Source: ITEMDETAILMAXDAOARTTEMPLATE01_INTERIM_SUMMARY_TO_ITEM_GUARD
```

Guard 当前只接受为阶段性成果：

```text
Item Detail 编辑真源为 Scene_TalismanBag_V04_ItemSandbox.unity 内原有 ItemDetailPanel 场景子树。
ItemDetailPanel 未删除重建；Canvas、ItemSandboxRoot、棋盘和道具栏未被本包重建。
zhenfaicon、qixingicon、daoju 原对象保留；qixingicon 的技术语义仍为 qiLeiTag，不是七星系统。
daoju 下已拆分 DaojuSingleCellImage / DaojuMultiCellImage 两个可独立手调 Image；Runtime 只允许选择显隐和 Sprite，不得写入其位置或尺寸。
Header、品阶背景、状态、基础属性、固定/随机词条、道痕、四个核心效果、法门 Build、器类 Build、摆放提示和旧物记已形成当前近完成视觉结构。
普通样本与 Roll 详情当前共用同一详情投影/绑定入口，但本阶段接受不代表150版本全量稳定复用已完成。
```

当前已接素材范围：

```text
五品阶弹窗背景。
震雷法 I001-I006 核心效果图标。
离火法 I007-I012 核心效果图标。
器类 Build 的符 / 印 / 令 / 镜 / 法专用技能图标。
item_daoju 中震雷法 I001-I006、离火法 I007-I012 的 _1.._5 品阶主体图。
当前主体图只接 Item Detail 的 ItemArtworkFrame；尚未接道具栏、棋盘或拖拽图。
其余普通道具与后续美术仍属于 ART_ASSETS_STILL_PARTIAL。
```

阶段验证边界：

```text
最近 Assembly Reload 后未观察到新的 C# 编译错误，但这不是最终完整 Unity verifier 回执。
尚未确认 ItemDetailMaxDaoArtTemplate 最终 verifier PASS、历史 Item System / Item Algorithm 全回归、最终 LeakCheck 与全包 git diff --check。
当前三份报告必须在最终收口前刷新，不能用较早报告冒充当前最终场景状态。
工作树中的 .DS_Store / .ds_store 及其他系统垃圾文件不得进入任何 checkpoint 或最终提交。
阶段性 RepoOps checkpoint 只保存当前版面，不等于最终 Guard PASS、最终美术完成或 hash baseline 接受。
```

基线与禁区继续锁定：

```text
当前不接受、不更新 Scene / Prefab hash 或 geometry baseline。
不启动 ItemDetailVisualSlotTool01。
不接道具栏、棋盘或拖拽美术绑定。
不接 V0.4 Battle、BattleContract、Battle Bridge、UnifiedBattlePage。
不接 V0.3 RunFlow、Reward、Inventory、SaveData、Boss。
不修改 BuildSettings、Item算法、品阶/属性/词条概率或正式 Candidate 数据。
本 Guard 不执行 commit / tag / push。
```

最终关闭条件：

```text
1. 用户补充当前计划内剩余美术，并给出最终视觉确认。
2. 用户完成非 Play 手调、保存重开、Play 后退出不回退、Inspector 可自由调整等最终手测。
3. 最终 verifier、历史回归、LeakCheck 与全包 git diff --check 完成。
4. 刷新 ItemDetailMaxDaoArtTemplateReport / Spec / LeakCheckReport。
5. 用户明确接受最终 Scene / Prefab / BuildSettings hash 与授权范围内 geometry 状态。
6. 以上完成后，Guard 才决定是否给出 GUARD_PASS_ITEMDETAILMAXDAOARTTEMPLATE01。
```

当前队列覆盖状态：

```text
CURRENT: ItemDetailMaxDaoArtTemplate01
STATE: USER_VISUAL_TUNING_NEAR_COMPLETE / NOT_FINAL_GUARD_PASS
NEXT: ItemDetailVisualSlotTool01
NEXT_STATE: BLOCKED_UNTIL_FINAL_MANUAL_ACCEPTANCE_AND_CLOSEOUT
```

### 2026-07-17：Item Detail 弹窗阶段性大包用户验收

```text
User acceptance: 我觉得item弹窗系统可以上传一个阶段性大包了，已经很满意了
Package: ItemDetailMaxDaoArtTemplate01
Milestone: ITEMDETAIL_PHASE_BIG_PACKAGE01
Status: USER_ACCEPTED_PHASE_MILESTONE
Layout: USER_ACCEPTED
Visual: USER_ACCEPTED_FOR_PHASE
Art: PARTIAL / DEFERRED_COMPLETION
RepoOps: COMMIT_AND_PUSH_AUTHORIZED
Tag: NOT_AUTHORIZED
```

Guard 收口：

```text
当前 Item Detail 弹窗的场景原地版面、核心信息结构、美术插槽和当前已有素材表现已达到用户满意，可作为阶段性大包保存并上传。
本次用户验收解除“等待最终视觉确认”的阻断，但只针对当前阶段版面与已接素材。
剩余 I013-I030 等后续美术、未来素材替换和全量150版本复用不属于当前阶段已完成声明。
阶段性上传不得写成 FINAL_ART_COMPLETE、LIVE_LOCKED 或正式战斗接入完成。
RepoOps 允许在当前工作分支精确选择 Item Detail 本包文件，创建 checkpoint / milestone commit 并 push。
不得使用整目录 git add，不得纳入 .DS_Store / .ds_store / 未引用的 -2 重复素材或无关 dirty 文件。
不得 tag、merge、改 BuildSettings 或接入 Battle / RunFlow / Save / Reward / Boss。
```

上传前最低门槛：

```text
Unity 当前编译无新增 C# Error。
ItemDetailMaxDaoArtTemplate verifier 若可只读运行，应记录真实结果；失败时不得伪装 PASS，也不得由 RepoOps 修代码。
git diff --cached --check 无本包新增 whitespace error。
暂存清单经人工核对，只包含 Item Detail 场景、脚本、报告、Guard规则与已确认美术及配套 .meta。
阶段性 commit 必须明确标记 art pending / phase milestone，不打 verified tag。
```

当前队列覆盖状态：

```text
CURRENT: ItemDetailMaxDaoArtTemplate01
STATE: USER_ACCEPTED_PHASE_MILESTONE / READY_FOR_REPOOPS_UPLOAD
NEXT: ItemDetailVisualSlotTool01
NEXT_STATE: WAIT_UNTIL_REPOOPS_UPLOAD_RECEIPT
```
