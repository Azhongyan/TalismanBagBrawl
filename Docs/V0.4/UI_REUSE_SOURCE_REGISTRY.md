# V0.4 UI Reuse Source Registry

维护方：Codex Guard / 记忆治理窗口

状态：

```text
GUARD_SYNC_UI_REUSE_SOURCE_REGISTRY_ACCEPTED
GUARD_DOWNGRADE_V04_PREVIEW_UI_TO_TEMPORARY
GUARD_REQUIRE_ADAPTER_BEFORE_MAINLINE_UI
```

## 1. 建立目的

本文件用于防止 V0.4 BuildSandbox 在可玩预览阶段重复重画 V0.2 / V0.3 已经调好的成熟 UI、动画和交互手感。

当前结论：

```text
V0.4 沙盒可以验证规则、数据、构筑逻辑和可玩流程。
成熟 UI / 动画 / 交互手感默认复用 V0.2 / V0.3。
任务窗口不得再仿制一套“看起来类似”的 UI。
成熟组件不是不能改，而是应作为 Base Component 扩展。
```

## 2. 总原则

```text
成熟 UI 优先复用。
新系统只做 Adapter / ViewModel / 字段扩展。
不能复用时必须先向 Guard 写明原因，不得直接重画。
```

补充：

```text
新玩法可以新增交互、按钮、字段、状态层和视觉反馈。
但这些应作为 Extension Layer 接到成熟组件上，而不是重建职责重叠的大组件。
```

具体含义：

```text
复用的是 UI 结构、交互手感、动画节奏、ScrollRect / Layout / Button / Popup 行为和信息组织方式。
V0.4 新增的是 BuildSandbox 数据适配层，而不是新 UI 真源。
```

## 3. V0.4 预览 UI 的定位

`Scene_TalismanBag_V04_BattleSandboxPreview` 中为验证多格、拖拽、旋转、越界、重叠而创建的 UI，默认定位为：

```text
temporary preview / logic test bench
```

它可以继续用于：

```text
验证 BuildSandbox 数据是否可视化
验证多格占位 / 旋转 / 碰撞 / 越界逻辑
验证分类筛选与预览数据流
验证报告和 leak check
```

但不得被视为：

```text
正式背包 UI 真源
正式棋盘 UI 真源
正式上拉动画真源
正式道具信息弹窗真源
正式 Boss / 地图机制说明 UI 真源
```

## 3.1 V04 沙盒视觉复制规则

当 V0.4 新玩法无法稳定承载在 V0.2 / V0.3 旧 runtime 内核中时，允许在 V04 独立沙盒场景中复制成熟 UI 的视觉表层。

允许复制：

```text
尺寸
颜色
按钮风格
边框
字体
间距
上拉节奏
视觉层级
```

但必须替换内核：

```text
视觉表层：尽量复刻 V0.2 / V0.3 BattlePrepare。
交互内核：使用 V0.4 ShapePlacement。
```

禁止：

```text
复制旧 V03 单格 drag/drop 内核
复制旧 LayoutGroup 抢位置逻辑作为多格摆放权威
继续 overlay + ignoreLayout 补丁路线
```

一句话：

```text
复制皮，换内核。
```

## 4. 当前成熟 UI 复用源登记

以下对象先登记为“优先复用源”。具体文件路径、Prefab / Scene 节点和字段映射可在后续 `UIReuseCorrection / Adapter` 包中做只读 Survey 后补充，不得因此重画一套新 UI。

| 复用源 | 状态 | 用途 | 后续接入方式 | 禁止 |
| --- | --- | --- | --- | --- |
| V0.3 BattlePrepare 上拉背包 / 道具栏 / 棋盘整备手感 | `REUSE_SOURCE / NEEDS_SOURCE_SURVEY` | 背包上拉、道具栏滚动、棋盘交互、整备按钮、继续战斗手感 | V0.4 Build 数据通过 Adapter 喂给成熟整备 UI | 不得在 V0.4 重新精修一套背包 / 棋盘 / 上拉动画 |
| V0.2 / V0.3 道具信息弹窗或道具 Tooltip 信息结构 | `REUSE_SOURCE / NEEDS_SOURCE_SURVEY` | 道具名称、品质、描述、数值、标签、词条、效果说明 | 新增 BuildSandboxItemInfoAdapter / ViewModel 字段映射 | 不得长期维护新的 V0.4 ItemInfoPopup 作为正式弹窗 |
| `BossInfoPanel` / Boss 信息展示语言 | `REUSE_SOURCE` | Boss 技能、威胁、推荐应对、奖励预告、机制线索 | BossProblem / Readiness 转成玩家侧模糊提示与开发者侧完整字段 | 不得为每个 Boss 新画独立说明框 |
| 战斗提示 / 伤害反馈 / CombatLog / Tooltip 语言 | `REUSE_SOURCE / NEEDS_SOURCE_SURVEY` | 机制触发、道具贡献、失败反馈、弱点窗口提示 | MechanicHintFeedback 只做 Adapter，不另建大量框 | 不得给每种机制单独造新 UI |
| V0.4 BuildSandbox Config Panel 01 | `DEV_PANEL_SOURCE` | 开发者调参、规则查看、中文显示名 + English stable key | 继续扩展字段和 Tab，不进入玩家 UI | 不得把完整答案显示给玩家 |

## 5. Adapter 接入规则

V0.4 数据接入成熟 UI 时，必须优先使用：

```text
BuildSandbox data
→ Extension Layer / Adapter / ViewModel
→ 已登记成熟 UI / 交互源
```

允许新增：

```text
Adapter
ViewModel
DTO
Extension Layer
只读 source survey report
字段映射表
devOnly validator / report
```

禁止：

```text
复制一套相似 UI
为了接新数据重写成熟 UI 手感
为了沙盒方便修改正式 V0.2 / V0.3 场景对象
绕过 Adapter 直接写正式 UI
因为新增功能就重建职责重叠的大组件
```

## 6. Hierarchy 命名与复用关系

复用旧 UI 时允许将新增节点、Adapter 节点、V0.4 沙盒节点命名为英文稳定名。

但如果要改旧成熟 UI 对象名，必须先做引用扫描：

```text
transform.Find
GameObject.Find
Serialized reference
Button onClick
Prefab / Scene reference
Editor tool lookup
```

禁止未扫描就重命名成熟对象，避免破坏既有交互。

中文只进入：

```text
UI 显示文案
配置 displayName / description
开发者数据面板中文显示名
报告说明
```

## 7. 对 BuildGridInteractionPreview01 的裁定

`V0.4-BuildGridInteractionPreview01` 不整包重做。

保留：

```text
多格占位逻辑
旋转逻辑
越界 / 重叠 / 合法放置反馈
分类筛选逻辑
BuildSandbox PreviewContext 数据流
报告 / Validator / leak check
```

降级为临时预览：

```text
V04 新建棋盘 UI
V04 新建背包 / 道具栏 UI
V04 新建上拉 / 摆放手感
V04 新建道具信息弹窗
ProblemSelector / BuildSandboxData 临时框架
```

后续不得继续把这些临时 UI 当正式方向精修。

## 8. 后续纠偏包

后续应插入：

```text
V0.4-BuildGridInteractionPreview01-UIReuseCorrection01
```

目标：

```text
1. 审计 BuildGridInteractionPreview01 中哪些 UI 是 temporary preview。
2. 标记 mature reuse source 与临时 UI 的边界。
3. 产出 V0.3 BattlePrepare / V0.2 item info / battle feedback 的只读 source survey。
4. 建立 BuildSandbox → mature UI 的 Extension / Adapter 字段映射草案。
5. 不改正式 V0.2 / V0.3 场景，不改用户手调布局。
```

该纠偏包完成前，不得把 V0.4 沙盒 UI 推进为正式 UI 真源。
