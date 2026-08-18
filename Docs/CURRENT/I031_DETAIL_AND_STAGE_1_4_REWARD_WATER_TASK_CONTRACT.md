# I031 Detail and Stage 1-4 Reward Water Task Contract

## Outcome

- `SPECIAL_I031` 使用现有 Item Detail Popup 展示现有正式念力容量与产出事实。
- 1-4 胜利后的现有领取按钮能够明确区分输入未到达、Reward 拒绝或领取后整理界面拒绝；只修真实首断点并继续 1-5。

## Owner and class

- Product Context: `CAMPAIGN_NORMAL_LV1`
- Task Class: `COMPLEX_GUARDED_ONCE`
- Primary / Development Owner: 当前 Codex 任务窗口
- New task or dispatch: `NO`

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs`
- `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs`
- 本 Task Contract

## Boundaries

- 复用现有 Item Authority、I031 Nian Capacity Projection、Item Detail Popup、Reward Bridge 与 Mainline Loop。
- 不新增 Scene、Prefab、Popup、Item/Reward System、掉落算法、掉落池或存档结构。
- 不把 I031 伪造成普通掉落 ItemInstance，不按普通道具 ID 增加战斗分支。
- 不修改装修、数值、VFX、Battle Session 或用户 Unity。

## Acceptance and stop

1. I031 详情读取 `C1FormalI031NianCapacityFact`，显示初始念力、上限、产出量与产出间隔，并保留系统阵眼身份。
2. 1-4 领取仍沿现有 `TryClaimCurrentVictoryReward`；首个运行拒绝必须能在 Editor.log 精确命名。
3. 若领取请求未到达 Host，停止在输入遮挡；若到达后拒绝，只修改诊断命名的 Owner。
4. 编译与 Item Detail 聚焦验证通过后，用户只需走一次 1-4 胜利页验证。

PlayerVisibleDelivery: `PARTIAL` 直到用户正常路线通过。
MilestoneCompletionEvidence: `NO`。
