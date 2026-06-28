# V0.3 MainHome UIUE SYNC TO GUARD

来源窗口：Codex / V0.3-ForgeFirstUpgradeGuide01 当前开发窗口
同步对象：Guard - AGENTS 记忆收口
同步类型：首页产品/UIUE 文档收口，不授权开发
当前状态：WAITING_GUARD_MEMORY_CLOSE

## 1. 一句话摘要

用户新增《V0.3 首页 UI 数据 / 切图 / UIUE 交互文档》，需要 Guard 将 V0.3 首页口径收为“照灯小铺空间化首页”，避免后续继续把首页做成灰盒功能大厅或战斗/养成入口堆叠页。

## 2. 本次同步来源

- 用户附件：《符箓背包》V0.3 首页 UI 数据 / 切图 / UIUE 交互文档。
- 用户附件：《符箓背包》V0.3-DevelopUpgradePage01 技术开发文档。
- 用户最新首页相关口径：
  - `Scene_TalismanBag_V03_MainHome` 需要垫底 fullbackground 场景图片插槽。
  - fullbackground 当前用黑色占位，必须满屏，不露背后内容。
  - 首页根节点的 `Image` 和 `Outline` 用户已先关掉，后续不要再使用或脚本重开。
  - 不要再出现“暂时收起”按钮。

## 3. Guard 需要收口的首页定位

V0.3 首页不是功能大厅，而是：

```text
小满接手照灯小铺后，用几本册子记录当前目标、获得收藏与剧情追查进度的空间。
```

首页核心结构应收为：

```text
顶部状态栏：玩家身份 + 资源 + 设置
中部空间物件：照灯小铺热点
右侧运营层：活动 / 邮件 / 商店 / 公告 / 设置
底部系统Bar：首页 / 养成 / 试炼 / 探索 / 更多
```

设计基准：

```text
竖屏设计分辨率：1080 x 2400
SafeArea：按设备适配
风格：照灯小铺物件化 UI / 符纸卡片 / 木牌按钮 / 圆角面板 / 小屏可读
```

## 4. 首页 Canvas 层级建议

Guard 需要记忆后续 MainHome 场景/Builder/Verifier 不应继续只生成一个 `MainHomeRoot` 灰盒大面板。建议层级为：

```text
MainHomeCanvas
├─ BG_Root                     // 照灯小铺背景 / fullbackground 图片插槽
├─ SceneHotspot_Root            // 首页空间热点点击区
├─ TopBar_Root                  // 顶部状态栏
├─ RightQuickBar_Root           // 右侧运营快捷栏
├─ BottomNavBar_Root            // 底部系统 Bar
├─ PopupLayer_Root              // 弹窗层
├─ ToastLayer_Root              // Toast / ComingSoon 提示
└─ GuideHighlight_Root          // 新手引导承载层，当前不做高亮逻辑
```

当前实现里的 `FullBackgroundImageSlot` 应作为 BG_Root / 背景图片插槽的临时落点：

- 必须全屏锚定。
- 当前占位色为黑色。
- 不拦截输入。
- 不依赖 Home 根节点 `Image` 兜底。

## 5. 顶部 TopBar 收口

首页 TopBar 只显示：

```text
玩家身份组 + 资源组 + 设置
```

不得显示当前关卡，不得显示当前任务。当前阶段 / 当前目标交给“柜台：照灯账本”承载。

资源顺序固定：

```text
灵石 -> 符纸 -> 朱砂
```

资源点击只弹资源说明小窗：

```text
资源名
当前数量
主要用途
主要获取来源
```

V0.3 禁止资源点击直接跳充值或复杂商店。

## 6. 底部 BottomBar 收口

底部 Bar 顺序必须为：

```text
首页 / 养成 / 试炼 / 探索 / 更多
```

允许 NavId：

```text
Home
Develop
Trial
Explore
More
```

禁止作为底栏入口：

```text
BattleBackpack
PVP
Store
Activity
Mail
Settings
```

特别注意：

- 战斗背包不得作为底部入口。
- PVP 后续归属 `Explore > 斗法台`。
- 试炼按钮可以轻微突出，但不做巨大凸起按钮。

## 7. 首页空间热点收口

首页空间物件不重复底部系统入口，负责表达“照灯小铺里发生什么”。

允许的首页热点方向：

```text
Ledger        / 照灯账本
CodexBook     / 道藏典册
ClueBook      / 旧物线索簿
DreamSign     / 梦签
Xiaoman       / 小满
BackRoom      / 后屋
StreetEntrance / 青石坊街口
```

功能边界：

- 柜台必须是“照灯账本”，不是单纯试炼入口。
- 道藏典册是轻量图鉴/收藏入口，不是战斗摆放页。
- 旧物线索簿是轻量剧情线索记录。
- 梦签只做轻签到/小奖励，不做复杂随机事件系统。
- 小满不是系统入口，是点击气泡/引导情绪反馈。
- 后屋与青石坊街口当前只保留插槽或 ComingSoon，不做正式系统。

## 8. 三本册认知系统

首页核心认知由三本册建立：

```text
照灯账本：我现在该干嘛
道藏典册：我已经获得/解锁过什么
旧物线索簿：我追查到哪里了
```

照灯账本外显摘要示例：

```text
照灯账本
当前阶段：接手小铺
当前目标：完成首次符箓升级
进度：2 / 4
[查看]
```

照灯账本前往规则需要能引导不同系统：

```text
继续主线试炼 -> 试炼
完成符箓升级 -> 养成
查看新符箓 -> 道藏典册
完成梦签 -> 梦签柜
开始探索 -> 探索
Boss 前整备 -> 试炼战备页
领取奖励 -> 留在账本
```

## 9. 右侧快捷栏收口

右侧运营快捷栏推荐入口：

```text
活动 / 邮件 / 商店 / 公告 / 设置
```

这些入口不能抢占首页中心，不应被做成主玩法入口。

## 10. 首页弹窗与提示收口

V0.3 首页应有统一弹窗层 / Toast 层：

- `PopupLayer_Root`
- `ToastLayer_Root`

ComingSoon 统一提示适用于：

- 玩家信息灰盒。
- 未开放后屋。
- 青石坊街口。
- 未开放活动/商店等运营入口。

## 11. 首页当前代码/场景状态说明

截至本同步文档生成时，当前开发窗口已有首页相关 dirty 文件：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity
Assets/_Game/Scripts/TalismanBag/V02/UI/MainHomeGreyboxPanel.cs
Assets/_Game/Scripts/TalismanBag/V03/MainHome/V03MainHomeSceneBootstrap.cs
Assets/_Game/Scripts/TalismanBag/V03/Editor/V03MainHomeSceneFix02.cs
```

这些 dirty 内容的性质：

- 给 `Scene_TalismanBag_V03_MainHome` 增加/保留黑色满屏 `FullBackgroundImageSlot`。
- 停用 `MainHomeRoot` 自身 `Image` / `Outline`，避免盖住用户后续图片背景。
- Runtime 不再创建 Home 根节点 `Image` / `Outline`。
- Bootstrap 启动时兜底关闭 Home 根节点 `Image` / `Outline`。
- Builder/Verifier 增加 Home 根节点 `Image` / `Outline` 不启用的校验。
- 不再显示“暂时收起”按钮的前置改动已在本包早些时候完成。

验证状态：

- `git diff --check` 已通过。
- 当前 Unity 已打开，但本轮脚本修改后程序集尚未自动刷新到最新时间；未宣称 Unity 编译通过。

## 12. 与 DevelopUpgradePage01 的边界

用户同时提供了《V0.3-DevelopUpgradePage01 技术开发文档》。Guard 需要明确：该文档是后续“养成 / 符箓升级页”的开发口径，不应把养成页功能混进首页。

首页只负责：

- 底栏 `养成` 入口。
- 照灯账本当前目标可前往 `养成`。
- 首次升级节点的首页引导承载层 / 图片槽口径。

首页不负责：

- 完整符箓升级 UI。
- 符箓列表、升级前后属性对比、材料消耗等养成页主体。
- 战斗背包、战斗棋盘、拖拽摆阵。

DevelopUpgradePage01 后续如果启动，必须先做 Code Survey，并遵守：

- 不改 SaveData 结构。
- 不重写 UpgradeService。
- 不重写 ResourceService。
- UI 通过 Adapter / ViewModel 调用已有服务。
- 洗练 / 合成 / 锻造只做锁定占位。

## 13. 首页 S 级阻断口径

以下应记入 Guard 首页验收红线：

```text
1. 底部 Bar 顺序不是：首页 / 养成 / 试炼 / 探索 / 更多。
2. 底部 Bar 出现“背包”入口。
3. 首页把试炼 / 养成 / 探索重复做成大系统按钮。
4. 柜台被做成单纯试炼入口，而不是照灯账本。
5. 照灯账本无法引导去不同系统。
6. 道藏典册被做成战斗摆放页。
7. 青石坊街口被做成当前版本正式系统。
8. 首页 TopBar 显示当前关卡，导致与账本重复。
9. Home 根节点 Image / Outline 被脚本或 Builder 重新启用，盖住 fullbackground 图片插槽。
10. 场景没有满屏背景/图片插槽，导致露出背后内容。
11. 首页仍出现“暂时收起”按钮。
```

## 14. 请求 Guard 收口项

请 Guard 判断并回执：

```text
GUARD_SYNC_MAINHOME_UIUE01_ACCEPTED
```

或：

```text
GUARD_RETURN_MAINHOME_UIUE01 <原因>
```

建议 Guard 写入记忆/Package Queue/Sync Hub 的收口口径：

- `Scene_TalismanBag_V03_MainHome` 后续应朝“照灯小铺空间化首页”演进。
- `FullBackgroundImageSlot` / BG_Root 是正式背景图片插槽的临时占位，当前黑色满屏，不得露背后内容。
- Home 根节点 `Image` / `Outline` 暂不使用，不得被 Runtime/Builder 自动重开。
- “暂时收起”按钮不得回归。
- 首页 TopBar / BottomBar / 热点 / 右侧栏 / 弹窗层按本同步文档口径收口。
- DevelopUpgradePage01 属于后续养成页开发，不应混入 MainHome 首页实现。

## 15. 本同步文档不授权

本文件只用于 Guard 记忆收口，不直接授权开发，不要求 RepoOps 上传，不代表 QA 通过。
