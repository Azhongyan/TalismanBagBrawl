# V0.3 Editor Tools Inventory

包体：`V0.3-SceneResidueIsolation01-B4-EditorMenuGovernance`

范围：只盘点和治理 `Tools / Talisman Bag` 菜单下的 Editor 工具。本文不授权清理场景残留，不授权运行会写入场景、BuildSettings、ProjectSettings 或 PlayerSettings 的工具。

## 风险标识

| 标识 | 含义 |
| --- | --- |
| `[Writes Scene]` | 会创建、修改、置脏或保存 `.unity` 场景。 |
| `[Guard Only]` | 只能在 Guard 或用户明确确认后执行。 |
| `[Deprecated]` | 历史包工具，仅保留追溯能力，日常不使用。 |
| `[QA Only]` | 仅用于验证、Smoke 或 PlayMode QA，不作为编辑/生成工具使用。 |
| `[Manual Only]` | 手动调试或人工编辑辅助工具，执行前必须确认当前编辑上下文。 |

## 工具清单

| 工具名 | 菜单路径 | 脚本 | 用途 | 什么时候能用 | 写 Scene | 改 BuildSettings / PlayerSettings | 生成 UI | 危险等级 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Build Android Playtest APK | `Tools/Talisman Bag/Build/[Guard Only] Build Android Playtest APK [Writes PlayerSettings]` | `Assets/_Game/Scripts/TalismanBag/Editor/AndroidApkBuilder.cs` | 构建 Android playtest APK。 | 仅打包窗口/用户明确要求。 | 否 | 改 PlayerSettings；写 APK 输出 | 否 | 高 |
| Ensure V02 Stage Progress Bar | `Tools/Talisman Bag/V0.2/[Writes Scene][Deprecated][Guard Only] Ensure V02 Stage Progress Bar` | `Assets/_Game/Scripts/TalismanBag/Editor/TalismanBagSceneBuilder.cs` | 给 V02 FormationCounter 场景创建或绑定 `V02StageProgressBar_Runtime`。 | 仅 V0.2 历史维护，Guard 确认后。 | 是：`Scene_TalismanBag_V02_FormationCounter` | 否 | 是 | 高 |
| Item Balance Panel | `Tools/Talisman Bag/V0.2/Data/[Manual Only] Item Balance Panel` | `Assets/_Game/Scripts/TalismanBag/Editor/TalismanItemBalancePanel.cs` | 道具平衡数据面板。 | 手动数据查看/编辑时。 | 否 | 否 | 否 | 中 |
| Enemy Identity Panel | `Tools/Talisman Bag/V0.2/Data/[Manual Only] Enemy Identity Panel` | `Assets/_Game/Scripts/TalismanBag/Editor/TalismanEnemyBalancePanel.cs` | 敌人身份/平衡数据面板。 | 手动数据查看/编辑时。 | 否 | 否 | 否 | 中 |
| Save Selected Play UI To Scene | `Tools/Talisman Bag/UI Hand Tune/[Writes Scene][Manual Only] Save Selected Play UI To Scene` | `Assets/_Game/Scripts/TalismanBag/Editor/UiHandTuneSnapshotTool.cs` | Play Mode 手调 UI 后，把白名单 UI 字段回写到当前场景。 | Play Mode 中选中 UI root，用户明确确认后。 | 是：当前 active scene | 否 | 回写 UI 布局 | 高 |
| Clear Pending Play UI Snapshot | `Tools/Talisman Bag/UI Hand Tune/[Manual Only] Clear Pending Play UI Snapshot` | 同上 | 清除待应用的 Play UI snapshot。 | Snapshot 卡住或取消手调流程时。 | 否 | 否 | 否 | 低 |
| Run Boot Entry Flow 01 Smoke | `Tools/Talisman Bag/V0.3/BootEntryFlow01/[QA Only] Run Boot Entry Flow 01 Smoke` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03BootEntryFlow01Smoke.cs` | 检查 BootEntry BuildSettings 顺序和基础对象。 | QA 验证时。 | 否 | 读取 BuildSettings | 否 | 中 |
| Build Boot Entry Flow 01 Scene | `Tools/Talisman Bag/V0.3/BootEntryFlow01/[Writes Scene][Guard Only] Build Boot Entry Flow 01 Scene` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03BootEntryFlow01SceneBuilder.cs` | 重建 BootEntry 场景、相机、EventSystem。 | Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_BootEntry` | 改 BuildSettings | 是 | 高 |
| Bind Boot Entry Runtime Lock Scene Nodes | `Tools/Talisman Bag/V0.3/BootEntryFlow01/[Writes Scene][Guard Only] Bind Boot Entry Runtime Lock Scene Nodes` | 同上 | 创建/绑定 BootEntry canvas、page、button、controller refs。 | Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_BootEntry` | 否 | 是 | 高 |
| Build Upgrade Scene | `Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Build Upgrade Scene` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03TalismanUpgradeSceneBuilder.cs` | 重建 Upgrade 场景和 editable preview。 | Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_TalismanUpgrade` | 可能改 BuildSettings | 是 | 高 |
| Verify Upgrade Scene | `Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[QA Only] Verify Upgrade Scene` | 同上 | 静态校验 Upgrade 场景和 BuildSettings。 | QA 验证时。 | 否 | 读取 BuildSettings | 否 | 中 |
| Rebuild Editable Upgrade Preview | `Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Rebuild Editable Upgrade Preview` | 同上 | 在 Upgrade 场景内重建 `V03TalismanUpgradePageRoot` 预览 UI。 | Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_TalismanUpgrade` | 否 | 是 | 高 |
| Bind Upgrade Runtime Lock Services | `Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Bind Upgrade Runtime Lock Services` | 同上 | 只用于 `Scene_TalismanBag_V03_TalismanUpgrade`，只在 Edit Mode 使用；写入或补齐 `V03Upgrade_ResourceService`、`V03Upgrade_UpgradeService`、`V03Upgrade_MainTrialFlowService`、`EventSystem`。 | Guard 或用户确认后；执行后按 Ctrl+S 确认保存状态。 | 是：`Scene_TalismanBag_V03_TalismanUpgrade` | 否 | 否 | 高 |
| Build NavigationFlow01 Scene | `Tools/Talisman Bag/V0.3/NavigationFlow01/[Writes Scene][Deprecated][Guard Only] Build Scene` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03NavigationFlow01SceneBuilder.cs` | 重建 MainHome 二级页 root、底部导航和 navigation controller。 | 历史修复追溯，Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_MainHome` | 可能改 BuildSettings | 是 | 高 |
| Verify NavigationFlow01 Static | `Tools/Talisman Bag/V0.3/NavigationFlow01/[QA Only] Verify Static` | 同上 | 静态校验 MainHome 导航结构。 | QA 验证时。 | 否 | 读取 BuildSettings | 否 | 中 |
| Verify NavigationFlow01 PlayMode | `Tools/Talisman Bag/V0.3/NavigationFlow01/[QA Only] Verify PlayMode` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03NavigationFlow01PlayModeVerifier.cs` | PlayMode 驱动底部导航，验证跳转。 | QA 验证时，不能作为编辑工具。 | 否 | 否 | 否 | 中 |
| Build Independent Main Home Scene | `Tools/Talisman Bag/V0.3/Fix02/[Writes Scene][Deprecated][Guard Only] Build Independent Main Home Scene` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03MainHomeSceneFix02.cs` | 重建独立 MainHome 场景，并包含旧场景 cleanup 路径。 | 历史修复追溯，Guard 明确确认后。 | 是：`Scene_TalismanBag_V03_MainHome`，且可能触碰 `Scene_TalismanBag_V02_FormationCounter` | 否 | 是 | 高 |
| Verify MainHome Fix02 Static | `Tools/Talisman Bag/V0.3/Fix02/[QA Only] Verify Static Scene` | 同上 | 静态校验 MainHome 场景结构。 | QA 验证时。 | 否 | 否 | 否 | 中 |
| Verify MainHome Fix02 PlayMode First Frame | `Tools/Talisman Bag/V0.3/Fix02/[QA Only] Verify PlayMode First Frame` | 同上 | PlayMode 首帧验证并截图。 | QA 验证时。 | 否 | 否；写截图日志 | 否 | 中 |
| Refresh MainHome Edit Preview | `Tools/Talisman Bag/V0.3/MainHome/[Writes Scene][Manual Only] Refresh Edit Preview (No Play)` | `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03MainHomeEditPreviewTools.cs` | 刷新 MainHome edit-time preview，可能绑定 bootstrap。 | 手动预览维护，用户确认后。 | 会置脏：`Scene_TalismanBag_V03_MainHome` | 否 | 可能更新 UI 预览 | 高 |
| Run Main Home Retry Smoke | `Tools/Talisman Bag/V0.3/MainHome/[QA Only] Run Main Home Retry Smoke` | `Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/V03MainHomeScene01RetrySmoke.cs` | MainHome retry smoke，创建临时运行时对象验证。 | QA 验证时。 | 否 | 否 | 临时对象 | 中 |
| Balance Debug Panel | `Tools/Talisman Bag/V0.2/Data/[Manual Only] Balance Debug Panel` | `Assets/_Game/Scripts/TalismanBag/V02/Balance/Editor/V02BalanceDebugWindow.cs` | V0.2 平衡调试窗口。 | 手动数据查看/编辑时。 | 否 | 否 | 否 | 中 |
| Stage Config Panel 01 | `Tools/Talisman Bag/V0.2/Data/[Manual Only] Stage Config Panel 01` | `Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/DataCatalogEditorWindow.cs` | Stage/DataCatalog 编辑窗口。 | 手动数据查看/编辑时。 | 否 | 否 | 否 | 中 |
| Validate Data Catalog | `Tools/Talisman Bag/V0.2/Data/[QA Only] Validate Data Catalog` | `Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/DataCatalogValidator.cs` | 只读校验 DataCatalog 配置。 | QA 或数据检查时。 | 否 | 否 | 否 | 低 |

## 使用规则

- 带 `[Writes Scene]` 的工具执行前必须确认当前任务包允许写入对应场景。
- 带 `[Guard Only]` 的工具必须先获得 Guard 或用户明确确认。
- 带 `[Deprecated]` 的工具默认只保留追溯能力，不作为当前 V0.3 日常构建入口。
- 带 `[QA Only]` 的工具只能用于验证，不允许用来生成、修复或清理场景。
- 带 `[Manual Only]` 的工具必须由用户理解当前上下文后手动触发。
- 本文不授权处理 FormationCounter 场景残留，不授权修改 RunFlow、PageState、FormationState、SaveData、Boss、奖励或数值。
