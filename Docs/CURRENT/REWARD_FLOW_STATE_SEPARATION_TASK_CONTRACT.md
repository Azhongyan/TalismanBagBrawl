# REWARD_FLOW_STATE_SEPARATION

## Outcome

正式流程明确分为：

`Victory -> Automatic RewardClaimed -> Arrange -> Continue`

胜利被正式 Battle 接受后，现有 Reward/Item Owner 自动且只接收一次精确奖励实例；弹窗直接显示该实例。玩家点击“整理道具”才打开现有 Board/Tray 整理面，整理完成后再以独立动作进入下一关或返回 WorldMap。玩家不可见的 Result 只作为胜利终态与奖励接收之间的瞬时内部边界。

## Product Context

- `CAMPAIGN_NORMAL_LV1`
- Task Class: `COMPLEX_GUARDED_ONCE`
- Primary Owner: Mainline Flow
- Dev Owner: 当前用户授权的单任务窗口

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/C1FormalObtainRebuildBattleLoop.cs`
- `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1FormalObtainRebuildBattleLoopTests.cs`
- 本合同

## Forbidden Writes

- Scene、Prefab、PresentationRoot、HUD View、Profile 与任何装修序列化资产
- Reward 掉落算法、Canonical Item 数据库、Item Authority、Board/Tray 规则
- Save 范围与 Stage-clear-only 持久化语义
- 第二套 Reward 状态机、兼容层或新流程 Owner

## Required Behavior

1. 战斗胜利进入现有 Result 后，Mainline 立即调用现有 `TryClaimCurrentVictoryReward`。
2. 自动领取成功进入 `RewardClaimed`，并直接显示同一奖励 Item；不显示空白待领取弹窗。
3. 同一胜利重复接收不新增 Item。
4. `RewardClaimed` 的独立动作才进入现有整理阶段。
5. 未进入整理阶段前不得开始下一关。
6. 整理后的独立动作才进入正式后继关卡；无后继关卡时返回 WorldMap。

## Verification

- Runtime/Editor scoped compile。
- 现有 Formal Loop focused tests 覆盖状态顺序、重复领取与 Continue 门。
- 本包只可声明内部流程闭合；奖励弹窗与正常玩家路径仍由后续装修/手测证明。

## Stop Conditions

- 若实现必须修改装修正在占用的 Prefab、View、PresentationRoot 或 Scene，立即停止。
- 若现有 Reward 合同无法保持精确 ItemInstance 与一次领取，停止并只命名首个缺失 Owner/API。

## Development Result — 2026-08-17

- Status: `IMPLEMENTED / RUNTIME_OFFLINE_COMPILE_PASS / EDITOR_OFFLINE_COMPILE_PASS / INTERNAL_QA / UNITY_NOT_RUN / PLAYER_PATH_NOT_PROVEN`。
- `StageOneResult / StageTwoResult` 只承担瞬时内部终态；胜利在 Mainline Loop 内自动接收奖励并进入 `RewardClaimed`。
- 玩家不再看到“领取掉落/领取奖励”；奖励弹窗直接显示真实奖励，独立动作“整理道具”才进入现有 `RebuildForStageTwo`。
- `TryStartNextStage` 在 `RewardClaimed` 阶段保持拒绝，只有进入整理阶段后才能继续。
- 重复领取沿用现有 Reward/Item 幂等结果，不新增 Item；重复进入整理阶段也保持 no-op。
- Host 的玩家动作改为“整理道具”→“进入下一关/完成并返回”。
- 未修改 Reward 算法、Item Authority、Board/Tray、Save、Scene、Prefab、PresentationRoot、HUD View 或 Profile。
- Offline compile：Runtime `PASS / 0 errors / 2 pre-existing warnings`；Editor `PASS / 0 errors / 1 pre-existing warning`。
- Source boundary check：`REWARD_FLOW_SOURCE_BOUNDARY_CHECK_PASS`。
- 现有 Formal Loop focused tests 已更新并通过 Editor 离线编译；为避免与装修 Unity 生命周期冲突，本轮未启动 Unity、未执行菜单测试或 Fresh Play。

## Same-package Product Correction — 2026-08-17

- 用户最新权威交互取消手动领取：胜利奖励没有需要玩家决定的领取分支。
- 自动接收仍复用同一个 Reward/Item 幂等 Owner，不增加第二状态机，不改变掉落概率或 ItemInstance 身份。
- Runtime/Editor 离线编译通过，仅保留既有警告。
- 当前状态为 `IMPLEMENTED / OFFLINE_COMPILE_PASS / INTERNAL_QA / PLAYER_PATH_NOT_PROVEN`，正常路线需验证弹窗直接显示奖励后再整理。

## Same-package Navigation Lifecycle Correction — 2026-08-18

- 玩家语义保持“奖励自动接收并直接显示同一 ItemInstance”；弹窗按钮文案为“领取道具”，其职责是确认奖励展示并进入现有整备面，不再次生成或接收奖励。
- Reward Popup 使用独立回调进入 `RebuildForStageTwo`，不再与被隐藏的底部 Primary Button 共用点击入口。
- 整备面打开后，底部中央按钮明确显示“开始战斗”；玩家点击后才关闭整备并启动正式后继 Stage，启动成功后恢复“战斗进行中”。
- 未修改 Reward 算法、Item Authority、Board/Tray、Battle 数值、Scene 或 Prefab。
- Unity Runtime/Editor 导入编译通过，未出现 C# error；仅保留工程既有 warning。
- 现有完整 Formal Loop 测试在进入本包断言前，被旧的 speed/battle-log 静态语义校验阻挡；定向 Loop 建立又被当前 1-1 StageConfig formal-route flag 阻挡。未为通过测试修改这些无关数据或继续叠加验证器补丁，临时命令入口已移除。
- 当前状态：`CODE_COMPILE_PASS / INTERNAL_STATE_TEST_BLOCKED_BY_EXISTING_QA_FIXTURE / USER_HANDTEST_REQUIRED / NOT_COMPLETE`。

## Same-package Reward Popup Binding Repair — 2026-08-18

- 用户正常路径首先出现的两个错误 `FORMAL_REWARD_POPUP_BINDING_MISSING` 与 `UNIFIED_FORMAL_REWARD_POPUP_BINDING_INVALID` 来自同一个 authored-reference 失败，不是两个 Reward 逻辑 Bug。
- 使用现有 `FormalBattleRewardPopupDecorAuthoring` 重新序列化唯一 Reward Popup Prefab 及其在现有 Unified Shell Prefab 中的 Host 引用；未增加运行时查找、备用 Popup 或第二套绑定。
- Unity 作者化与持久化校验完成：`FORMAL_BATTLE_REWARD_POPUP_AND_ARRANGE_VISUAL_PASS`；C# compile 无 error。
- 未修改 Reward 状态机、Item Authority、Board/Tray、Battle 数值或正式 Scene。下一门仍是用户正常路径验证。
